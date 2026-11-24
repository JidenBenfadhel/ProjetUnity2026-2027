using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Genies.Refs;
using Genies.Utilities;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace Genies.Naf
{
#if GENIES_SDK && !GENIES_INTERNAL
    internal static class NativeAvatarsFactory
#else
    public static class NativeAvatarsFactory
#endif
    {
        public struct UnifiedConfig
        {
            public Transform                  Parent;
            public NativeGenieBuilder         NativeBuilder;
            public IAssetParamsService        AssetParamsService;
            public Ref<ContainerService>      ContainerServiceRef;
            public string                     SilhouetteAssetId;
            public Dictionary<string, string> SilhouetteAssetParams;
            public string                     Definition;
            public bool                       SkipWaitingDefinitionLoad;
            public int[] MaterialLods;
        }

        public static UniTask<NativeUnifiedGenieController> CreateUnifiedGenieAsync(
            string                         definition            = null,
            Transform                      parent                = null,
            IAssetParamsService            assetParamsService    = null,
            Ref<ContainerService>          containerServiceRef   = default,
            string                         silhouetteAssetId     = null,
            Dictionary<string, string>     silhouetteAssetParams = null,
            int[] lodsLevels = null

        ) {
            var config = new UnifiedConfig()
            {
                Parent = parent,
                AssetParamsService = assetParamsService,
                ContainerServiceRef = containerServiceRef,
                Definition          = definition,
                SilhouetteAssetId   = silhouetteAssetId,
                SilhouetteAssetParams = silhouetteAssetParams,
                MaterialLods =  ToMaterialLods(lodsLevels)
            };

            return CreateUnifiedGenieAsync(config);
        }

        public static UniTask<NativeUnifiedGenieController> CreateUnifiedGenieAsync(
            NativeGenieBuilder             builder,
            string                         definition            = null,
            IAssetParamsService            assetParamsService    = null,
            Ref<ContainerService>          containerServiceRef   = default,
            string                         silhouetteAssetId     = null,
            Dictionary<string, string>     silhouetteAssetParams = null,
            int[] lodsLevels = null
        ) {
            var config = new UnifiedConfig()
            {
                NativeBuilder = builder,
                AssetParamsService = assetParamsService,
                ContainerServiceRef = containerServiceRef,
                Definition          = definition,
                SilhouetteAssetId   = silhouetteAssetId,
                SilhouetteAssetParams = silhouetteAssetParams,
                MaterialLods =  ToMaterialLods(lodsLevels)
            };

            return CreateUnifiedGenieAsync(config);
        }

        public static async UniTask<NativeUnifiedGenieController> CreateUnifiedGenieAsync(UnifiedConfig config)
        {
            using var processSpan = new ProcessSpan(ProcessIds.CreateUnifiedGenieAsync);

            // create a NativeGenie builder if none is provided. Also ensure that the given config parent is set, if any
            if (config.NativeBuilder)
            {
                if (config.Parent)
                {
                    config.NativeBuilder.transform.SetParent(config.Parent);
                }
            }
            else
            {
                config.NativeBuilder = CreateDefaultNativeGenieBuilder(config.Parent);
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                config.NativeBuilder.DisableRefitting = true;
#endif
            }

            // set a no-op asset params service if none is provided
            config.AssetParamsService ??= new NoOpAssetParamsService();

            // create a container service with the default NAF config if none is provided
            if (!config.ContainerServiceRef.IsAlive)
            {
                config.ContainerServiceRef = CreateRef.FromDisposable(new ContainerService(NafAssetResolverConfig.Default));
            }

            if (config.MaterialLods != null)
            {
                config.ContainerServiceRef.Item.SetMaterialDynamicLods(config.MaterialLods);
            }

            // create the controller
            var controller = new NativeUnifiedGenieController(config.NativeBuilder, config.AssetParamsService, config.ContainerServiceRef);
            controller.SilhouetteAssetId = config.SilhouetteAssetId;
            controller.SilhouetteAssetParams = config.SilhouetteAssetParams;

            // set the definition if provided
            if (!string.IsNullOrWhiteSpace(config.Definition))
            {
                if (config.SkipWaitingDefinitionLoad)
                {
                    // await the silhouette so it's visible before returning, then fire-and-forget
                    // the full definition load so the caller can interact with the avatar immediately
                    await controller.LoadSilhouetteAsync();
                    controller.SetDefinitionAsync(config.Definition).Forget();
                }
                else
                {
                    await controller.SetDefinitionAsync(config.Definition);
                    await controller.WaitUntilLoadedAllTexturesAsync() ;
                }
            }

            return controller;
        }

        /// <summary>
        /// Extracts material LOD levels from general LOD levels.
        /// Currently a 1:1 pass-through; material quality tiers map directly to the
        /// general LOD indices (0=High, 1=Mid, 2=Low).
        /// </summary>
        private static int[] ToMaterialLods(int[] lods) => lods;

        public static NativeGenieBuilder CreateDefaultNativeGenieBuilder(Transform parent = null)
        {
            using var processSpan = new ProcessSpan(ProcessIds.CreateDefaultNativeGenieBuilder);

            var prefab = Resources.Load<NativeGenieBuilder>("NativeGenie");
            if (!prefab)
            {
                Debug.LogError($"[{nameof(NativeAvatarsFactory)}] could not find {nameof(NativeGenieBuilder)} prefab in Resources.");
                return null;
            }

            NativeGenieBuilder genie = Object.Instantiate(prefab, parent);
            if (!genie)
            {
                return null;
            }

            return genie;
        }
    }
}
