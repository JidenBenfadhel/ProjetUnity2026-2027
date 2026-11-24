using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Genies.ServiceManagement;
using UnityEngine;

namespace Genies.Sdk
{
    public sealed partial class AvatarSdk
    {
        /// <summary>
        /// Initializes the Genies Avatar SDK.
        /// Calling is optional as all operations will initialize the SDK if it is not already initialized.
        /// This method is safe to call multiple times - subsequent calls return the cached initialization result.
        /// </summary>
        /// <returns>True if initialization succeeded, false otherwise.</returns>
        public static async UniTask<bool> InitializeAsync()
        {
            return await Instance.InitializeInternalAsync();
        }

        internal static async UniTask<bool> InitializeDemoModeAsync()
        {
            return await Instance.InitializeDemoModeInternalAsync();
        }

        /// <summary>
        /// Loads an avatar using the provided load options.
        /// This is the preferred method for loading avatars. The concrete type of <paramref name="options"/>
        /// determines which avatar is loaded - see each type's documentation for details:
        /// <list type="bullet">
        /// <item><see cref="LoadAvatarOptions.User"/> - loads the authenticated user's avatar, or a specific user's avatar by ID.</item>
        /// <item><see cref="LoadAvatarOptions.ByDefinition"/> - loads an avatar from a cached JSON definition string for optimized subsequent loads.</item>
        /// <item><see cref="LoadAvatarOptions.Default"/> - loads the default fallback avatar.</item>
        /// <item><see cref="LoadAvatarOptions.Test"/> - loads a test avatar for demo mode and offline testing.</item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// For faster load times, consider pre-caching avatar assets ahead of time using
        /// <see cref="PrecacheUserAvatarAssetsAsync"/>, <see cref="PrecacheDefaultAvatarAssetsAsync"/>,
        /// or <see cref="PrecacheAvatarAssetsByDefinitionAsync"/>.
        /// </remarks>
        /// <param name="options">The load options that specify which avatar to load and how. See <see cref="ILoadAvatarOptions"/> for shared properties.</param>
        /// <returns>A ManagedAvatar instance, or null if loading failed.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="options"/> is an unsupported type.</exception>
        public static async UniTask<ManagedAvatar> LoadAvatarAsync<T>(T options) where T : ILoadAvatarOptions
        {
            await Instance.InitializeInternalAsync();
            return await Instance.CoreSdk.AvatarApi.LoadAvatarAsync(options);
        }

        /// <summary>
        /// Gets the authenticated user's avatar definition as a JSON string.
        /// This fetches the latest user avatar definition from the server.
        /// The returned definition can be cached and used with <see cref="LoadAvatarByDefinitionAsync"/> for optimized loading.
        /// User must be logged in.
        /// </summary>
        /// <returns>The avatar definition JSON string, or null if retrieval failed.</returns>
        public static async UniTask<string> GetUserAvatarDefinition()
        {
            await Instance.InitializeInternalAsync();
            return await Instance.CoreSdk.AvatarApi.GetUserAvatarDefinition();
        }

        /// <summary>
        /// Gets a specific user's avatar definition as a JSON string by user ID.
        /// This fetches the latest avatar definition for the specified user from the server.
        /// The returned definition can be cached and used with <see cref="LoadAvatarByDefinitionAsync"/> for optimized loading.
        /// The local client must be authenticated with Genies services (i.e. user is logged in).
        /// </summary>
        /// <param name="userId">The user ID whose avatar definition to retrieve.</param>
        /// <returns>The avatar definition JSON string, or null if retrieval failed.</returns>
        public static async UniTask<string> GetUserAvatarDefinition(string userId)
        {
            await Instance.InitializeInternalAsync();
            return await Instance.CoreSdk.AvatarApi.GetUserAvatarDefinition(userId);
        }

        /// <summary>
        /// Pre-caches assets required for the authenticated user's avatar without loading it.
        /// This downloads and caches all assets needed for the avatar, improving subsequent load times.
        /// User must be logged in.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>True if pre-caching was successful, false otherwise.</returns>
        public static async UniTask<bool> PrecacheUserAvatarAssetsAsync(CancellationToken cancellationToken = default)
        {
            await Instance.InitializeInternalAsync();
            return await Instance.CoreSdk.AvatarApi.PrecacheUserAvatarAssetsAsync(cancellationToken);
        }

        /// <summary>
        /// Pre-caches assets required for a default avatar without loading it.
        /// This downloads and caches all assets needed for the avatar, improving subsequent load times.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>True if pre-caching was successful, false otherwise.</returns>
        public static async UniTask<bool> PrecacheDefaultAvatarAssetsAsync(CancellationToken cancellationToken = default)
        {
            await Instance.InitializeInternalAsync();
            return await Instance.CoreSdk.AvatarApi.PrecacheDefaultAvatarAssetsAsync(cancellationToken);
        }

        /// <summary>
        /// Pre-caches assets required for an avatar based on a JSON definition without loading it.
        /// This downloads and caches all assets needed for the avatar, improving subsequent load times.
        /// User must be logged in.
        /// </summary>
        /// <param name="definition">The avatar definition JSON string.</param>
        /// <returns>True if pre-caching was successful, false otherwise.</returns>
        public static async UniTask<bool> PrecacheAvatarAssetsByDefinitionAsync(string definition)
        {
            await Instance.InitializeInternalAsync();
            return await Instance.CoreSdk.AvatarApi.PrecacheAvatarAssetsByDefinitionAsync(definition);
        }

        private static readonly Lazy<AvatarSdk> _instance = new Lazy<AvatarSdk>(() => new AvatarSdk());
        private static AvatarSdk Instance => _instance.Value;

        private CoreSdk CoreSdk { get; }
        private AsyncLazy<bool> InitializationTask { get; }
        private bool DemoModeRequested { get; set; }

        private AvatarSdk()
        {
            CoreSdk = new CoreSdk();
            InitializationTask = new AsyncLazy<bool>(PerformInitializationAsync);
        }

        private UniTask<bool> InitializeDemoModeInternalAsync()
        {
            DemoModeRequested = true;
            return InitializationTask.Task;
        }

        private UniTask<bool> InitializeInternalAsync()
        {
            DemoModeRequested = false;
            return InitializationTask.Task;
        }

        private async UniTask<bool> PerformInitializationAsync()
        {
            try
            {
                if (DemoModeRequested)
                {
                    return await PerformDemoModeInitializationAsync();
                }

                await new AvatarSdkAppInitializer().InitializeAppAsync();
                return ServiceManager.IsAppInitialized;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize the Avatar SDK: {ex.Message}");
                return false;
            }
        }

        private async UniTask<bool> PerformDemoModeInitializationAsync()
        {
            try
            {
                await new AvatarSdkDemoAppInitializer().InitializeAppAsync();
                return ServiceManager.IsAppInitialized;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize the Avatar SDK in demo mode: {ex.Message}");
                return false;
            }
        }

        #region Legacy - Explicit Avatar load methods
        // These methods are provided for convenience but are considered legacy.
        // Prefer using LoadAvatarAsync<T>(T options) with the appropriate LoadAvatarOptions type instead.
        // See LoadAvatarOptions.User, LoadAvatarOptions.ByDefinition, LoadAvatarOptions.Default, and LoadAvatarOptions.Test.

        /// <summary>
        /// Loads a default avatar with optional configuration.
        /// </summary>
        /// <remarks>
        /// Prefer using <see cref="LoadAvatarAsync{T}"/> with <see cref="LoadAvatarOptions.Default"/> directly
        /// for more control over load options such as silhouette visibility.
        /// </remarks>
        /// <param name="avatarName">Optional name for the avatar GameObject.</param>
        /// <param name="parent">Optional parent transform for the avatar.</param>
        /// <param name="playerAnimationController">Optional animation controller to apply to the avatar.</param>
        /// <param name="showLoadingSilhouette">Optional. Whether to show a placeholder silhouette while the avatar is loading. Defaults to true.</param>
        /// <param name="lodsLevels">Optional LOD levels for avatar quality. Multiple values are loaded sequentially from lowest to highest quality as they become available. Pass a single-element array to target a specific LOD without sequential loading. Defaults to Low. Currently affects material/texture quality only; mesh LOD support will be added in a future update.</param>
        /// <returns>A ManagedAvatar instance, or null if loading failed.</returns>
        public static async UniTask<ManagedAvatar> LoadDefaultAvatarAsync(
            string avatarName = null,
            Transform parent = null,
            RuntimeAnimatorController playerAnimationController = null,
            bool showLoadingSilhouette = true,
            AvatarLods[] lodsLevels = null)
        {
            await Instance.InitializeInternalAsync();
            return await LoadAvatarAsync(new LoadAvatarOptions.Default
            {
                AvatarName = avatarName,
                Parent = parent,
                PlayerAnimationController = playerAnimationController,
                ShowLoadingSilhouette = showLoadingSilhouette,
                TargetLods = lodsLevels,
            });
        }

        /// <summary>
        /// Loads the authenticated user's avatar with optional configuration.
        /// This method fetches the latest user avatar definition from the server on each call.
        /// Falls back to default avatar if user is not logged in.
        ///
        /// OPTIMIZATION: For better performance of subsequent loads, consider caching the avatar definition
        /// and loading it with <see cref="LoadAvatarByDefinitionAsync"/>.
        /// </summary>
        /// <remarks>
        /// Prefer using <see cref="LoadAvatarAsync{T}"/> with <see cref="LoadAvatarOptions.User"/> directly
        /// for more control over load options such as silhouette visibility.
        /// </remarks>
        /// <param name="avatarName">Optional name for the avatar GameObject.</param>
        /// <param name="parent">Optional parent transform for the avatar.</param>
        /// <param name="playerAnimationController">Optional animation controller to apply to the avatar.</param>
        /// <param name="showLoadingSilhouette">Optional. Whether to show a placeholder silhouette while the avatar is loading. Defaults to true.</param>
        /// <param name="lodsLevels">Optional LOD levels for avatar quality. Multiple values are loaded sequentially from lowest to highest quality as they become available. Pass a single-element array to target a specific LOD without sequential loading. Defaults to Low. Currently affects material/texture quality only; mesh LOD support will be added in a future update.</param>
        /// <returns>A ManagedAvatar instance, or null if loading failed.</returns>
        public static async UniTask<ManagedAvatar> LoadUserAvatarAsync(
            string avatarName = null,
            Transform parent = null,
            RuntimeAnimatorController playerAnimationController = null,
            bool showLoadingSilhouette = true,
            AvatarLods[] lodsLevels = null)
        {
            await Instance.InitializeInternalAsync();
            return await LoadAvatarAsync(new LoadAvatarOptions.User
            {
                AvatarName = avatarName,
                Parent = parent,
                PlayerAnimationController = playerAnimationController,
                ShowLoadingSilhouette = showLoadingSilhouette,
                TargetLods = lodsLevels,
            });
        }

        /// <summary>
        /// Loads an avatar based on a provided UserId.
        /// </summary>
        /// <remarks>
        /// Prefer using <see cref="LoadAvatarAsync{T}"/> with <see cref="LoadAvatarOptions.User"/> directly,
        /// setting <see cref="LoadAvatarOptions.User.UserId"/> for more control over load options.
        /// </remarks>
        /// <param name="userId">The user ID whose avatar to load.</param>
        /// <param name="avatarName">Optional name for the avatar GameObject.</param>
        /// <param name="parent">Optional parent transform for the avatar.</param>
        /// <param name="playerAnimationController">Optional animation controller to apply to the avatar.</param>
        /// <param name="showLoadingSilhouette">Optional. Whether to show a placeholder silhouette while the avatar is loading. Defaults to true.</param>
        /// <param name="lodsLevels">Optional LOD levels for avatar quality. Multiple values are loaded sequentially from lowest to highest quality as they become available. Pass a single-element array to target a specific LOD without sequential loading. Defaults to Low. Currently affects material/texture quality only; mesh LOD support will be added in a future update.</param>
        /// <returns>A ManagedAvatar instance, or null if loading failed.</returns>
        public static async UniTask<ManagedAvatar> LoadUserAvatarByUserIdAsync(
            string userId,
            string avatarName = null,
            Transform parent = null,
            RuntimeAnimatorController playerAnimationController = null,
            bool showLoadingSilhouette = true,
            AvatarLods[] lodsLevels = null)
        {
            await Instance.InitializeInternalAsync();
            return await LoadAvatarAsync(new LoadAvatarOptions.User
            {
                UserId = userId,
                AvatarName = avatarName,
                Parent = parent,
                PlayerAnimationController = playerAnimationController,
                ShowLoadingSilhouette = showLoadingSilhouette,
                TargetLods = lodsLevels,
            });
        }


        /// <summary>
        /// Loads an avatar based on a provided JSON definition.
        /// </summary>
        /// <remarks>
        /// Prefer using <see cref="LoadAvatarAsync{T}"/> with <see cref="LoadAvatarOptions.ByDefinition"/> directly
        /// for more control over load options such as silhouette visibility.
        /// </remarks>
        /// <param name="definition">The avatar definition JSON string.</param>
        /// <param name="avatarName">Optional name for the avatar GameObject.</param>
        /// <param name="parent">Optional parent transform for the avatar.</param>
        /// <param name="playerAnimationController">Optional animation controller to apply to the avatar.</param>
        /// <param name="showLoadingSilhouette">Optional. Whether to show a placeholder silhouette while the avatar is loading. Defaults to true.</param>
        /// <param name="lodsLevels">Optional LOD levels for avatar quality. Multiple values are loaded sequentially from lowest to highest quality as they become available. Pass a single-element array to target a specific LOD without sequential loading. Defaults to Low. Currently affects material/texture quality only; mesh LOD support will be added in a future update.</param>
        /// <returns>A ManagedAvatar instance, or null if loading failed.</returns>
        public static async UniTask<ManagedAvatar> LoadAvatarByDefinitionAsync(
            string definition,
            string avatarName = null,
            Transform parent = null,
            RuntimeAnimatorController playerAnimationController = null,
            bool showLoadingSilhouette = true,
            AvatarLods[] lodsLevels = null)
        {
            await Instance.InitializeInternalAsync();
            return await LoadAvatarAsync(new LoadAvatarOptions.ByDefinition
            {
                DefinitionToLoad = definition,
                AvatarName = avatarName,
                Parent = parent,
                PlayerAnimationController = playerAnimationController,
                ShowLoadingSilhouette = showLoadingSilhouette,
                TargetLods = lodsLevels,
            });
        }

        /// <summary>
        /// Loads a test avatar.
        /// </summary>
        /// <remarks>
        /// Prefer using <see cref="LoadAvatarAsync{T}"/> with <see cref="LoadAvatarOptions.Test"/> directly
        /// for more control over load options such as silhouette visibility.
        /// </remarks>
        /// <param name="avatarName">Optional name for the avatar GameObject.</param>
        /// <param name="parent">Optional parent transform for the avatar.</param>
        /// <param name="playerAnimationController">Optional animation controller to apply to the avatar.</param>
        /// <param name="showLoadingSilhouette">Optional. Whether to show a placeholder silhouette while the avatar is loading. Defaults to true.</param>
        /// <param name="lodsLevels">Optional LOD levels for avatar quality. Multiple values are loaded sequentially from lowest to highest quality as they become available. Pass a single-element array to target a specific LOD without sequential loading. Defaults to Low. Currently affects material/texture quality only; mesh LOD support will be added in a future update.</param>
        /// <returns>A ManagedAvatar instance, or null if loading failed.</returns>
        public static async UniTask<ManagedAvatar> LoadTestAvatarAsync(
            string avatarName = null,
            Transform parent = null,
            RuntimeAnimatorController playerAnimationController = null,
            bool showLoadingSilhouette = true,
            AvatarLods[] lodsLevels = null)
        {
            await Instance.InitializeInternalAsync();

            return await LoadAvatarAsync(new LoadAvatarOptions.Test
            {
                AvatarName = avatarName,
                Parent = parent,
                PlayerAnimationController = playerAnimationController,
                ShowLoadingSilhouette = showLoadingSilhouette,
                TargetLods = lodsLevels,
            });
        }
        #endregion

        #region Deprecated - Avatar Editor methods and events
        private static readonly string _genericAvatarEditorError =  "Install the com.genies.avatareditor-sdk.client package for this functionality. " +
                                                        "Download instructions are available via Tools -> Genies -> Download Legacy Avatar Editor";

        private static readonly string _avatarEditorEventError = "This event has been removed from the Avatar SDK. " + _genericAvatarEditorError;

        private static readonly string _avatarEditorMethodError = "This method has been removed from the Avatar SDK. " + _genericAvatarEditorError;
        /// <summary>
        /// Provides events for SDK notifications.
        /// </summary>
        public static partial class Events
        {
            /// <summary>
            /// This event has been removed from the Avatar SDK,
            /// install the com.genies.sdk.avatareditor package for this functionality
            /// Event raised when the avatar editor is opened.
            /// </summary>
            public static event Action AvatarEditorOpened
            {
                add => Debug.LogError(_avatarEditorEventError);
                remove => Debug.LogError(_avatarEditorEventError);
            }

            /// <summary>
            /// This event has been removed from the Avatar SDK,
            /// install the com.genies.sdk.avatareditor package for this functionality
            /// Event raised when the avatar editor is closed.
            /// </summary>
            public static event Action AvatarEditorClosed
            {
                add => Debug.LogError(_avatarEditorEventError);
                remove => Debug.LogError(_avatarEditorEventError);
            }
        }

        /// <summary>
        /// This bool has been removed from the Avatar SDK,
        /// install the com.genies.sdk.avatareditor package for this functionality
        /// Gets whether the avatar editor is currently open.
        /// </summary>
        /// <returns>True if the editor is open and active, false otherwise</returns>
        public static bool IsAvatarEditorOpen => false;

        /// <summary>
        /// This method has been removed from the Avatar SDK,
        /// install the com.genies.sdk.avatareditor package for this functionality
        /// Opens the Avatar Editor with the specified avatar and camera.
        /// </summary>
        /// <param name="avatar">The avatar to edit. If null, loads the current user's avatar.</param>
        /// <param name="camera">The camera to use for the editor. If null, uses Camera.main.</param>
        /// <returns>A UniTask that completes when the editor is opened.</returns>
        public static UniTask OpenAvatarEditorAsync(ManagedAvatar avatar, Camera camera = null)
        {
            Debug.LogError(_avatarEditorMethodError);
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// This method has been removed from the Avatar SDK,
        /// install the com.genies.sdk.avatareditor package for this functionality
        /// Closes the Avatar Editor and cleans up resources.
        /// </summary>
        /// /// <param name="revertAvatar">Whether the avatar should be reverted to it's pre-edited self.</param>
        /// <returns>A UniTask that completes when the editor is closed.</returns>
        public static UniTask CloseAvatarEditorAsync(bool revertAvatar)
        {
            Debug.LogError(_avatarEditorMethodError);
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// This method has been removed from the Avatar SDK,
        /// install the com.genies.sdk.avatareditor package for this functionality
        /// Gets the active avatar being edited in the Avatar Editor.
        /// </summary>
        /// <returns>The currently active ManagedAvatar, or null if no avatar is currently being edited.</returns>
        public static ManagedAvatar GetAvatarEditorAvatar()
        {
            Debug.LogError(_avatarEditorMethodError);
            return null;
        }

        /// <summary>
        /// This method has been removed from the Avatar SDK,
        /// install the com.genies.sdk.avatareditor package for this functionality
        /// Sets the avatar editor to save locally and continue editing.
        /// </summary>
        /// <param name="profileId">The profile ID to use when saving locally. If null, uses the default template name.</param>
        /// <returns>A UniTask representing the async operation.</returns>
        public static UniTask SetEditorSaveLocallyAndContinueAsync(string profileId)
        {
            Debug.LogError(_avatarEditorMethodError);
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// This method has been removed from the Avatar SDK,
        /// install the com.genies.sdk.avatareditor package for this functionality
        /// Sets the avatar editor to save locally and exit the editor.
        /// </summary>
        /// <param name="profileId">The profile ID to use when saving locally. If null, uses the default template name.</param>
        /// <returns>A UniTask representing the async operation.</returns>
        public static UniTask SetEditorSaveLocallyAndExitAsync(string profileId)
        {
            Debug.LogError(_avatarEditorMethodError);
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// This method has been removed from the Avatar SDK,
        /// install the com.genies.sdk.avatareditor package for this functionality
        /// Sets the avatar editor to save to the cloud and continue editing.
        /// </summary>
        /// <returns>A UniTask representing the async operation.</returns>
        public static UniTask SetEditorSaveRemotelyAndContinueAsync()
        {
            Debug.LogError(_avatarEditorMethodError);
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// This method has been removed from the Avatar SDK,
        /// install the com.genies.sdk.avatareditor package for this functionality
        /// Sets the avatar editor to save to the cloud and exit the editor.
        /// </summary>
        /// <returns>A UniTask representing the async operation.</returns>
        public static UniTask SetEditorSaveRemotelyAndExitAsync()
        {
            Debug.LogError(_avatarEditorMethodError);
            return UniTask.CompletedTask;
        }
        #endregion
    }
}
