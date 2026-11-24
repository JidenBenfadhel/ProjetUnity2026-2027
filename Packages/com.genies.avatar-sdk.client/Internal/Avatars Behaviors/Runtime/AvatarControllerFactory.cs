using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Genies.Naf;
using Genies.Naf.Content.AvatarBaseConfig;
using Genies.Refs;
using Genies.Utilities;
using UnityEngine;

namespace Genies.Avatars.Behaviors
{
    /// <summary>
    /// Factory class for creating different types of avatar controllers and genies.
    /// This static class provides methods for instantiating avatars with various configurations including unified genies, baked genies, and non-UMA genies.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal static class AvatarControllerFactory
#else
    public static class AvatarControllerFactory
#endif
    {
        private const string ComposerUnifiedGeniePrefabPath = "UnifiedGenieInstance";
        private const string UnifiedDefaultBodyType = "AvatarBase/recmDqoKYpEG1TQV";
        private const string SilhouetteAssetId = "AvatarBase/recmDqoKYpEG1TQV";
        private const string UnifiedDefaultBodyAssetLod = "0";
        private const string SilhouetteAssetLod = "0";
        private static readonly int[] _defaultLods = { 2, };
        private const string AvatarLayerName = "Avatar";

        /// <summary>
        /// This method creates a new instance of an editable genie, assigned a layer, components and disposal procedure.
        /// </summary>
        /// <param name="avatarDefinition">Json representation of the avatar</param>
        /// <param name="root">Transform of the parent</param>
        /// <param name="assetParamsService">Service to fetch asset params (version, lod)</param>
        /// <param name="containerServiceRef">Internal Configs about avatar loading: lods, quality, etc</param>
        /// <param name="showLoadingSilhouette">Flag to enable display of silhouette while avatar is loading</param>
        /// <param name="lods">LOD levels for avatar to load</param>
        /// <returns>IAvatarController</returns>
        public static async UniTask<IAvatarController> CreateNafGenie(
            string avatarDefinition,
            Transform root,
            IAssetParamsService assetParamsService = null,
            Ref<ContainerService> containerServiceRef = default,
            bool showLoadingSilhouette = true,
            int[] lods = null)
        {

            // Derive fetch LOD from dynamic LODs so CDN assets match the quality tier
            var lodAwareParamsService = GetLodAwareParamsService(assetParamsService, lods);

            // load the user unified genie prefab that contains the animation controller and clone camera
            var unifiedGeniePrefab = Resources.Load<AvatarController>(ComposerUnifiedGeniePrefabPath);
            var genieInstance      = Object.Instantiate(unifiedGeniePrefab, Vector3.one * 1000, Quaternion.identity, root);

            genieInstance.name = "UnifiedGenieInstance";

            // build silhouette params so the silhouette can load without hitting inventory
            Dictionary<string, string> silhouetteParams = null;
            if (showLoadingSilhouette)
            {
                silhouetteParams = await GetSilhouetteParams();
            }

            // create the unified genie instance and get the controller back
            NativeUnifiedGenieController controller = await NativeAvatarsFactory.CreateUnifiedGenieAsync(
                avatarDefinition,
                genieInstance.transform,
                lodAwareParamsService,
                containerServiceRef,
                SilhouetteAssetId,
                silhouetteParams,
                lods);

            // add the default body if not containing any assets TODO create a default avatar definition for NAF avatars
            if (controller.GetEquippedAssetIds().Count == 0)
            {
                var bodyParams = await controller.AssetParamsService.FetchParamsAsync(UnifiedDefaultBodyType);
                bodyParams ??= new Dictionary<string, string>();
                await controller.EquipAssetAsync(UnifiedDefaultBodyType, bodyParams);
            }

            controller.Genie.Root.SetLayerRecursive(LayerMask.NameToLayer(AvatarLayerName));
            controller.Genie.Disposed += () => Object.Destroy(genieInstance);

            // move the camera as a child of the genie
            var camera = genieInstance.GetComponentInChildren<Camera>();
            camera.transform.SetParent(controller.Genie.Root.transform, false);

            genieInstance.Initialize(controller);
            return genieInstance;
        }

        public static async UniTask<NativeUnifiedGenieController> CreateSimpleNafGenie(
            string avatarDefinition,
            Transform root,
            IAssetParamsService assetParamsService = null,
            bool showLoadingSilhouette = true,
            int[] lods = null)
        {
            // build silhouette params so the silhouette can load without hitting inventory
            Dictionary<string, string> silhouetteParams = null;
            if (showLoadingSilhouette)
            {
                silhouetteParams = await GetSilhouetteParams();
            }

            // Derive fetch LOD from LOD levels so CDN assets match the quality tier
            var lodAwareParamsService = GetLodAwareParamsService(assetParamsService, lods);

            // create the unified genie instance and get the controller back, returning early after the
            // silhouette is loaded so the caller can interact with the avatar while the full definition loads
            var controller = await NativeAvatarsFactory.CreateUnifiedGenieAsync(new NativeAvatarsFactory.UnifiedConfig
            {
                Parent                    = root,
                AssetParamsService        = lodAwareParamsService,
                Definition                = avatarDefinition,
                SilhouetteAssetId         = showLoadingSilhouette ? SilhouetteAssetId : null,
                SilhouetteAssetParams     = silhouetteParams,
                SkipWaitingDefinitionLoad = showLoadingSilhouette,
                MaterialLods =  lods ?? _defaultLods
            });

            // add the default body if the definition doesn't contain any assets
            if (controller.GetEquippedAssetIds().Count == 0)
            {
                var bodyParams = await controller.AssetParamsService.FetchParamsAsync(UnifiedDefaultBodyType);
                bodyParams ??= new Dictionary<string, string>();
                bodyParams["v"] = await AvatarBaseVersionService.GetAvatarBaseVersion();
                await controller.EquipAssetAsync(UnifiedDefaultBodyType, bodyParams);
            }

            var layer = LayerMask.NameToLayer(AvatarLayerName);
            if (layer >= 0)
            {
                controller.Genie.Root.SetLayerRecursive(layer);
            }

            return controller;
        }

        /// <summary>
        /// Gets the parameters needed to load the default silhouette from naf
        /// </summary>
        /// <returns>Dictionary of parameters</returns>
        private static async UniTask<Dictionary<string, string>> GetSilhouetteParams()
        {
            // build silhouette params so the silhouette can load without hitting inventory
            return new Dictionary<string, string>
            {
                { "v", await AvatarBaseVersionService.GetAvatarBaseVersion() },
                { "lod", SilhouetteAssetLod },
            };
        }

        /// <summary>
        /// Wraps the given IAssetParamsService in a higher-level class that uses the requested lods
        /// </summary>
        /// <param name="originalAssetParamsService">The IAssetParamsService you wish to use to pull other params from (i.e. version/v)</param>
        /// <param name="lods">The requested lods</param>
        /// <returns>The wrapped service</returns>
        private static IAssetParamsService GetLodAwareParamsService(IAssetParamsService originalAssetParamsService, int[] lods)
        {
            // Derive fetch LOD from LOD levels so CDN assets match the quality tier
            var fetchLod = LodOverrideAssetParamsService.DeriveFromLods(lods);
            return new LodOverrideAssetParamsService(originalAssetParamsService ?? new NoOpAssetParamsService(), fetchLod);
        }
    }
}
