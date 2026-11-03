using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Genies.Assets.Services;
using Genies.PerformanceMonitoring;
using Genies.Refs;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Genies.Avatars
{
    /// <summary>
    /// Static factory for loading our avatars.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal static class AvatarsFactory
#else
    public static class AvatarsFactory
#endif
    {
        private static CustomInstrumentationManager _InstrumentationManager => CustomInstrumentationManager.Instance;
        private static string _RootTransaction => CustomInstrumentationOperations.LoadAvatarTransaction;

#region CONTROLLERS
        public static async UniTask<ISpeciesGenieController> CreateGenieAsync(string species, string subSpecies = null,
            string definition = null, Transform parent = null, string lod = AssetLod.Default, AvatarsContext context = null)
        {
            return species switch
            {
                GenieSpecies.Unified => await CreateUnifiedGenieAsync(definition, parent, lod, context),
                GenieSpecies.UnifiedGAP => await CreateUnifiedGAPGenieAsync(subSpecies, definition, parent, lod, context),
                _ => null,
            };
        }

        public static async UniTask<ISpeciesGenieController> CreateGenieAsync(string species, IEditableGenie editableGenie,
            string definition = null, string lod = AssetLod.Default, AvatarsContext context = null)
        {
            return species switch
            {
                GenieSpecies.Unified => await CreateUnifiedGenieAsync(editableGenie, definition, lod, context),
                GenieSpecies.UnifiedGAP => await CreateUnifiedGAPGenieAsync(editableGenie, definition, lod, context),
                _ => null,
            };
        }

        public static async UniTask<UnifiedGenieController> CreateUnifiedGenieAsync(string definition = null,
            Transform parent = null, string lod = AssetLod.Default, AvatarsContext context = null)
        {
            context ??= DefaultAvatarsContext.Instance;
            UmaGenie umaGenie = await UmaGenieFactory.CreateAsync(GenieSpecies.Unified, parent, lod, context);
            UnifiedGenieController controller = await CreateUnifiedGenieAsync(umaGenie, definition, lod, context);
            return controller;
        }

        public static async UniTask<UnifiedGAPGenieController> CreateUnifiedGAPGenieAsync(string subSpecies = null,
            string definition = null, Transform parent = null, string lod = AssetLod.Default, AvatarsContext context = null)
        {
            context ??= DefaultAvatarsContext.Instance;
            EditableGenie umaGenie = await EditableGenieFactory.CreateAsync(GenieSpecies.UnifiedGAP, subSpecies, parent, lod, context);
            UnifiedGAPGenieController controller = await CreateUnifiedGAPGenieAsync(umaGenie, definition, lod, context);
            return controller;
        }

        public static async UniTask<UnifiedGenieController> CreateEditableGenieAsync(string definition = null,
            Transform parent = null, string lod = AssetLod.Default, AvatarsContext context = null)
        {
            context ??= DefaultAvatarsContext.Instance;
            EditableGenie editableGenie = await EditableGenieFactory.CreateAsync(GenieSpecies.Unified, parent : parent, context : context);
            UnifiedGenieController controller = await CreateUnifiedGenieAsync(editableGenie, definition, lod, context);
            return controller;
        }

        public static async UniTask<UnifiedGenieController> CreateUnifiedGenieAsync(IEditableGenie editableGenie,
            string definition = null, string lod = AssetLod.Default, AvatarsContext context = null)
        {
            if (editableGenie is null)
            {
                return null;
            }

            context ??= DefaultAvatarsContext.Instance;
            var controller = new UnifiedGenieController(editableGenie, context);

            await controller.InitializeAsync();

            if (!string.IsNullOrEmpty(definition))
            {
                var wasTracked = false;
                if (!_InstrumentationManager.RunningTransactions.Contains(_RootTransaction))
                {
                    _InstrumentationManager.StartTransaction(_RootTransaction, "AvatarController.SetDefinition");
                    wasTracked = true;
                }

                await controller.SetDefinitionAsync(definition);

                if (wasTracked)
                {
                    _InstrumentationManager.FinishTransaction(_RootTransaction);
                }
            }

            AddGameObjectReferences(controller);

            return controller;
        }

        public static async UniTask<UnifiedGAPGenieController> CreateUnifiedGAPGenieAsync(IEditableGenie editableGenie,
            string definition = null, string lod = AssetLod.Default, AvatarsContext context = null)
        {
            if (editableGenie is null)
            {
                return null;
            }

            context ??= DefaultAvatarsContext.Instance;
            var controller = new UnifiedGAPGenieController(editableGenie, context);
            await controller.InitializeAsync();

            if (!string.IsNullOrEmpty(definition))
            {
                var wasTracked = false;
                if (!_InstrumentationManager.RunningTransactions.Contains(_RootTransaction))
                {
                    _InstrumentationManager.StartTransaction(_RootTransaction, "AvatarController.SetDefinition");
                    wasTracked = true;
                }

                await controller.SetDefinitionAsync(definition);

                if (wasTracked)
                {
                    _InstrumentationManager.FinishTransaction(_RootTransaction);
                }
            }

            AddGameObjectReferences(controller);

            return controller;
        }
#endregion

#region BAKING
        public static async UniTask<IGenie> CreateBakedGenieAsync(string definition, Transform parent = null,
            string lod = AssetLod.Default, bool urpBake = false, AvatarsContext context = null)
        {
            // since we require a definition we can infer the species from it
            if (!TryGetSpeciesFromDefinition(definition, out string species))
            {
                Debug.LogError($"[{nameof(AvatarsFactory)}] couldn't infer the species from the given definition");
                return null;
            }

            string subSpecies = (species == GenieSpecies.UnifiedGAP) ? GetSubSpeciesFromDefinition(definition) : null;

            // create a genie controller (this is the only way to build the genie right now)
            ISpeciesGenieController controller = await CreateGenieAsync(species, subSpecies : subSpecies, definition, parent: null, lod, context);

            // bake the controller's genie
            IGenie bakedGenie = await controller.Genie.BakeAsync(parent, urpBake);

            // dispose the controller as the baked genie to dispose all the resources (baked genie is independent)
            controller.Dispose();

            return bakedGenie;
        }

        public static async UniTask<IGenieSnapshot> CreateGenieSnapshotAsync(string definition, RuntimeAnimatorController pose,
            Transform parent = null, bool urpBake = false, string lod = AssetLod.Default, AvatarsContext context = null)
        {
            if (!TryGetSpeciesFromDefinition(definition, out string species))
            {
                Debug.LogError($"[{nameof(AvatarsFactory)}] couldn't infer the species from the given definition");
                return null;
            }

            string subSpecies = (species == GenieSpecies.UnifiedGAP) ? GetSubSpeciesFromDefinition(definition) : null;

            ISpeciesGenieController controller = await CreateGenieAsync(species, subSpecies : subSpecies, definition, parent: null, lod, context);

            /**
             * This is not ideal but we need to wait for one frame before changing the animation so some bonus components
             * are initialized properly and doesn't throw errors. After changing the animator to the given pose we need
             * to wait for one more frame so it takes effect.
             */
            await UniTask.Yield();
            controller.Genie.Animator.runtimeAnimatorController = pose;
            await UniTask.Yield();

            IGenieSnapshot genieSnapshot = await controller.Genie.TakeSnapshotAsync(parent, urpBake);
            controller.Dispose();

            return genieSnapshot;
        }
#endregion

        public static bool TryGetSpeciesFromDefinition(string definition, out string species)
        {
            if (string.IsNullOrEmpty(definition))
            {
                species = null;
                return false;
            }

            try
            {
                species = JObject.Parse(definition)["Species"]?.Value<string>();
                return !string.IsNullOrEmpty(species);
            }
            catch (Exception)
            {
                species = null;
                return false;
            }
        }

        public static string GetSubSpeciesFromDefinition(string definition)
        {
            if (string.IsNullOrEmpty(definition))
            {
                return null;
            }

            return JObject.Parse(definition)["SubSpecies"]?.Value<string>();
        }


        private static void AddGameObjectReferences(ISpeciesGenieController controller)
        {
            GenieReference.Create(controller.Genie, controller.Genie.Root, disposeOnDestroy: false);
            if (controller.Genie is IEditableGenie editableGenie)
            {
                EditableGenieReference.Create(editableGenie, controller.Genie.Root, disposeOnDestroy: false);
            }

            SpeciesGenieControllerReference.Create(controller, controller.Genie.Root, disposeOnDestroy: true);
        }
    }
}
