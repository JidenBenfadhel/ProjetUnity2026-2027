using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Genies.Avatars.Behaviors;
using Genies.Avatars.Services;
using Genies.Avatars.Context;
using Genies.CrashReporting;
using Genies.Login.Native;
using Genies.Naf;
using Genies.ServiceManagement;
using Genies.Utilities;
using Newtonsoft.Json;
using UnityEngine;
using Avatar = Genies.Services.Model.Avatar;
using Object = UnityEngine.Object;

namespace Genies.Avatars.Sdk
{
#if GENIES_SDK && !GENIES_INTERNAL
    internal class GeniesAvatarSdkService : IGeniesAvatarSdkService
#else
    public class GeniesAvatarSdkService : IGeniesAvatarSdkService
#endif
    {
        internal const string DefaultAvatarName = "Genies Avatar";

        private static IAvatarService AvatarServiceInstance =>
            _avatarService ??= ServiceManager.Get<IAvatarService>();

        private static IAvatarService _avatarService;

        // Added: settings + converter to mirror AvatarService behavior
        private readonly JsonSerializerSettings _serializerSettings = new() { Formatting = Formatting.Indented, };
        private readonly IDefinitionConverter _definitionManager = new NafAvatarDefinitionConverter();

        async UniTask IGeniesAvatarSdkService.Initialize()
        {
            if (DefaultAvatarsContext.Instance is null)
            {
                await AvatarsContextProvider.GetOrCreateDefaultInstance();
            }
        }

        private GeniesAvatarLoader CreateAvatarLoader(string name = DefaultAvatarName, Transform parent = null)
        {
            name = string.IsNullOrEmpty(name) ? DefaultAvatarName : name;
            var geniesAvatarGo = new GameObject(name);
            geniesAvatarGo.transform.SetParent(parent, worldPositionStays: false);

            // create and initialize the UserAvatar component
            var userAvatar = geniesAvatarGo.AddComponent<GeniesAvatarLoader>();

            return userAvatar;
        }

        private async UniTask<GeniesAvatarLoader> CreateAndLoadAvatar(
            string definition = null,
            string avatarName = DefaultAvatarName,
            Transform parent = null,
            RuntimeAnimatorController animatorController = null,
            int atlasResolution = 512,
            bool showLoadingSilhouette = true,
            int[] lods = null
        )
        {
            GeniesAvatarLoader geniesAvatarLoader = CreateAvatarLoader(avatarName, parent);
            try
            {
                await geniesAvatarLoader.SetupAvatarAndControllers(definition,
                    parent,
                    animatorController,
                    atlasResolution,
                    showLoadingSilhouette,
                    lods);
            }
            catch (Exception e)
            {
                CrashReporter.LogHandledException(e);
            }

            return geniesAvatarLoader;
        }

        async UniTask<NativeUnifiedGenieController> IGeniesAvatarSdkService.CreateAvatarAsync(
            string definition,
            Transform parent,
            int atlasResolution,
            bool showLoadingSilhouette,
            int[] lods)
        {
            using var _ = new ProcessSpan(ProcessIds.CreateAvatarAsync);

            if (DefaultAvatarsContext.Instance is null)
            {
                await AvatarsContextProvider.GetOrCreateDefaultInstance();
            }

            // create a tmp offscreen GameObject that is far away so the genie doesn't appear on any camera while loading (UMA cannot load the avatar if the GO is inactive)
            Transform offscreenParent = new GameObject("Tmp Offscreen").transform;
            offscreenParent.localPosition = new Vector3(100000.0f, 100000.0f, 100000.0f);

            if (parent == null)
            {
                parent = offscreenParent;
            }

            try
            {
                // Explicit use?
                IAssetParamsService paramsService = ServiceManager.GetService<IAssetParamsService>(null);

                // load a unified genie controller
                NativeUnifiedGenieController controller =
                    await AvatarControllerFactory.CreateSimpleNafGenie(definition,
                        parent,
                        paramsService,
                        showLoadingSilhouette,
                        lods);

                return controller;
            }
            catch (AggregateException e)
            {
                CrashReporter.LogHandledException(e);
            }
            catch (Exception e)
            {
                CrashReporter.LogHandledException(e);
            }
            finally
            {
                if (offscreenParent != null)
                {
                    Object.Destroy(offscreenParent.gameObject);
                }
            }

            return null;
        }

        private async UniTask<IGenie> LoadRuntimeAvatarInternalAsync(
            string definition,
            string avatarName,
            Transform parent,
            RuntimeAnimatorController animatorController,
            int atlasResolution,
            bool showLoadingSilhouette = true,
            int[] lods = null)
        {
            // Create user avatar
            GeniesAvatarLoader avatarLoader =
                await CreateAndLoadAvatar(definition,
                    avatarName,
                    parent,
                    animatorController,
                    atlasResolution,
                    showLoadingSilhouette,
                    lods);
            return avatarLoader?.Genie;
        }

        public async UniTask<string> GetMyAvatarDefinition(bool waitUntilUserIsLoggedIn = false)
        {
            using var processSpanMethod = new ProcessSpan(ProcessIds.GetMyAvatarDefinition);

            string definition;

            if (!waitUntilUserIsLoggedIn && !GeniesLoginSdk.IsUserSignedIn())
            {
                Debug.Log("Loading a default avatar since user is not logged in.");
                definition = JsonConvert.SerializeObject(AvatarExtensions.DefaultDefinition());
            }
            else
            {
                // Get avatar definition from user
                await ((AvatarServiceInstance as AvatarService)!).WaitUntilInitializedAsync();

                using var processSpanFetch = new ProcessSpan(ProcessIds.GetMyAvatarDefinitionFetch, processSpanMethod);
                Genies.Naf.AvatarDefinition avatarDefinition = await AvatarServiceInstance.GetAvatarDefinitionAsync();

                definition = JsonConvert.SerializeObject(avatarDefinition);
            }

            return definition;
        }

        public async UniTask<IGenie> LoadUserRuntimeAvatarAsync(
            string avatarName = DefaultAvatarName,
            Transform parent = null,
            RuntimeAnimatorController animatorController = null,
            int atlasResolution = 512,
            bool waitUntilUserIsLoggedIn = false,
            bool showLoadingSilhouette = true,
            int[] lods = null)
        {
            string definition;

            // Load default avatar if not signed in and not waiting
            if (!waitUntilUserIsLoggedIn && !GeniesLoginSdk.IsUserSignedIn())
            {
                Debug.Log("Loading a default avatar since user is not logged in.");
                definition = JsonConvert.SerializeObject(AvatarExtensions.DefaultDefinition());
            }
            else
            {
                // Get avatar definition from user
                await ((AvatarService)AvatarServiceInstance).WaitUntilInitializedAsync();
                Genies.Naf.AvatarDefinition avatarDefinition = await AvatarServiceInstance.GetAvatarDefinitionAsync();
                definition = JsonConvert.SerializeObject(avatarDefinition);
            }

            IGenie genie = await LoadRuntimeAvatarInternalAsync(definition,
                avatarName,
                parent,
                animatorController,
                atlasResolution,
                showLoadingSilhouette,
                lods);

            Debug.Log(genie != null
                ? "successfully created user avatar"
                : "failed to load user avatar");

            return genie;
        }

        public async UniTask<List<Avatar>> LoadAvatarsDataByUserIdAsync(string userId)
        {
            List<Avatar> avatars = new();
            try
            {
                avatars = await AvatarServiceInstance.GetUserAvatarsAsync(userId);
            }
            catch (Exception e)
            {
                CrashReporter.LogHandledException(e);
            }

            return avatars;
        }

        public async UniTask<string> LoadAvatarDefStringByUserId(string userId)
        {
            try
            {
                var avatars = await AvatarServiceInstance.GetUserAvatarsAsync(userId);
                if (avatars == null || avatars.Count == 0)
                {
                    CrashReporter.LogError($"No avatars found for userId: {userId}");
                    return string.Empty;
                }

                var avatar = avatars[0];
                if (avatar == null || string.IsNullOrEmpty(avatar.Definition))
                {
                    CrashReporter.LogError($"Avatar or empty definition for userId: {userId}");
                    return string.Empty;
                }

                var definitionJson = avatar.Definition;

                // Try to get a concrete definition
                Naf.AvatarDefinition def;
                try
                {
                    def = await NafAvatarExtensions.DeserializeToAvatarDefinitionAsync(definitionJson);
                }
                catch (JsonSerializationException jse)
                {
                    CrashReporter.Log($"Error deserializing Avatar Definition: {jse.Message}. Using default.",
                        LogSeverity.Error);
                    CrashReporter.LogHandledException(jse);
                    def = NafAvatarExtensions.DefaultDefinition();
                }
                catch (AggregateException ae)
                {
                    CrashReporter.LogHandledException(ae);
                    def = NafAvatarExtensions.DefaultDefinition();
                }
                catch (Exception ex)
                {
                    CrashReporter.LogHandledException(ex);
                    def = NafAvatarExtensions.DefaultDefinition();
                }

                // If version differs, try to convert
                if (def != null && !string.Equals(def.JsonVersion, _definitionManager.TargetVersion))
                {
                    var parsed = DefinitionToken.TryParse(definitionJson, out DefinitionToken oldToken, "JsonVersion");
                    if (parsed)
                    {
                        var result = await _definitionManager.ConvertAsync(oldToken, _definitionManager.TargetVersion);
                        def = result.Token.ToObject<Naf.AvatarDefinition>();
                        var convertedJson = JsonConvert.SerializeObject(def, _serializerSettings);

                        return convertedJson;
                    }
                    else
                    {
                        CrashReporter.LogError(
                            $"Cannot convert avatar definition from: {def.JsonVersion} to {_definitionManager.TargetVersion}");
                        // Fall back to original JSON if we can’t convert
                        return definitionJson;
                    }
                }

                // Already at target version; return original
                return definitionJson;
            }
            catch (Exception e)
            {
                CrashReporter.LogHandledException(e);
                return string.Empty;
            }
        }

        public async UniTask<IGenie> LoadRuntimeAvatarAsync(
            string definition,
            string avatarName = "GeniesAvatar",
            Transform parent = null,
            RuntimeAnimatorController animatorController = null,
            int atlasResolution = 512,
            bool showLoadingSilhouette = true,
            int[] lods = null
        )
        {
            IGenie genie = await LoadRuntimeAvatarInternalAsync(definition,
                avatarName,
                parent,
                animatorController,
                atlasResolution,
                showLoadingSilhouette,
                lods);

            Debug.Log(
                genie != null
                    ? $"successfully load avatar: {avatarName}"
                    : $"failed to load avatar: {avatarName}");

            return genie;
        }

        public async UniTask<IGenie> LoadRuntimeAvatarAsync(
            Naf.AvatarDefinition definition,
            string avatarName = DefaultAvatarName,
            Transform parent = null,
            RuntimeAnimatorController animatorController = null,
            int atlasResolution = 512,
            bool showLoadingSilhouette = true,
            int[] lods = null
        )
        {
            var stringDefinition = JsonConvert.SerializeObject(definition);
            return await LoadRuntimeAvatarAsync(
                stringDefinition,
                avatarName,
                parent,
                animatorController,
                atlasResolution,
                showLoadingSilhouette,
                lods
            );
        }

        public async UniTask<IGenie> LoadDefaultRuntimeAvatarAsync(
            string avatarName = DefaultAvatarName,
            Transform parent = null,
            RuntimeAnimatorController animatorController = null,
            int atlasResolution = 512,
            bool showLoadingSilhouette = true,
            int[] lods = null
        )
        {
            return await LoadRuntimeAvatarAsync(
                new Naf.AvatarDefinition(),
                avatarName,
                parent,
                animatorController,
                atlasResolution,
                showLoadingSilhouette,
                lods
            );
        }

        private TextureSettings CreateTextureAtlas(int atlasResolution)
        {
            var textureSettings = ScriptableObject.CreateInstance<TextureSettings>();
            textureSettings.width = atlasResolution;
            textureSettings.height = atlasResolution;

            return textureSettings;
        }
    }
}
