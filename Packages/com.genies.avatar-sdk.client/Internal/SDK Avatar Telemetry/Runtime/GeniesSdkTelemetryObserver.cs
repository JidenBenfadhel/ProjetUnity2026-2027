#if UNITY_EDITOR
using System.Collections.Generic;
using Genies.Avatars.Customization;
using Genies.Avatars.Sdk;
using Genies.Telemetry;
using UnityEditor;
using UnityEngine;

namespace Genies.Sdk.Avatar.Telemetry
{
    [InitializeOnLoad]
    internal static class GeniesSdkTelemetryObserver
    {
        private static bool _subscribed;

        private const string ContextValue = "Avatar SDK";

        private static string sdkVersion = "";

        static GeniesSdkTelemetryObserver()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EnsureSubscribed();
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                Unsubscribe();
            }
        }

        private static void EnsureSubscribed()
        {
            if (_subscribed)
                return;

            _subscribed = true;

            AvatarCustomizationSDK.EquippedAsset += OnAssetEquipped;
            AvatarCustomizationSDK.UnequippedAsset += OnAssetUnequipped;

            AvatarCustomizationSDK.SkinColorSet += OnSkinColorSet;
            AvatarCustomizationSDK.TattooEquipped += OnTattooEquipped;
            AvatarCustomizationSDK.TattooUnequipped += OnTattooUnequipped;
            AvatarCustomizationSDK.BodyPresetSet += OnBodyPresetSet;
            AvatarCustomizationSDK.BodyTypeSet += OnBodyTypeSet;

            AvatarCustomizationSDK.AvatarDefinitionSaved += OnAvatarSaved;
            AvatarCustomizationSDK.AvatarDefinitionSavedLocally += OnAvatarSavedLocally;
            AvatarCustomizationSDK.AvatarDefinitionSavedToCloud += OnAvatarSavedToCloud;
            AvatarCustomizationSDK.AvatarLoadedForEditing += OnAvatarLoaded;

            GeniesAvatarsSdk.LoadedAvatar += OnLoadedAvatar;

            GeniesAvatarsSdk.GeniesAvatarsSdkInitialized += OnGeniesAvatarsSdkInitialized;

            SamplePrefabTracker.SamplePrefabUsed += OnSamplePrefabUsed;
        }

        private static void Unsubscribe()
        {
            if (!_subscribed)
                return;

            _subscribed = false;

            AvatarCustomizationSDK.EquippedAsset -= OnAssetEquipped;
            AvatarCustomizationSDK.UnequippedAsset -= OnAssetUnequipped;

            AvatarCustomizationSDK.SkinColorSet -= OnSkinColorSet;
            AvatarCustomizationSDK.TattooEquipped -= OnTattooEquipped;
            AvatarCustomizationSDK.TattooUnequipped -= OnTattooUnequipped;
            AvatarCustomizationSDK.BodyPresetSet -= OnBodyPresetSet;
            AvatarCustomizationSDK.BodyTypeSet -= OnBodyTypeSet;

            AvatarCustomizationSDK.AvatarDefinitionSaved -= OnAvatarSaved;
            AvatarCustomizationSDK.AvatarDefinitionSavedLocally -= OnAvatarSavedLocally;
            AvatarCustomizationSDK.AvatarDefinitionSavedToCloud -= OnAvatarSavedToCloud;
            AvatarCustomizationSDK.AvatarLoadedForEditing -= OnAvatarLoaded;

            GeniesAvatarsSdk.LoadedAvatar -= OnLoadedAvatar;

            GeniesAvatarsSdk.GeniesAvatarsSdkInitialized -= OnGeniesAvatarsSdkInitialized;

            SamplePrefabTracker.SamplePrefabUsed -= OnSamplePrefabUsed;
        }

        private static void OnLoadedAvatar(bool wasDefault, float loadTime)
        {
            var properties = WithSdkMetadata(new Dictionary<string, object>
            {
                { "IsDefault", wasDefault ? "true" : "false" },
                { "loadTime", loadTime },
            });

            GeniesTelemetry.RecordEvent(
                TelemetryEvent.Create("avatar_loaded", properties)
            );
        }

        private static void OnGeniesAvatarsSdkInitialized(float loadTime)
        {
            var properties = WithSdkMetadata(new Dictionary<string, object>
            {
                { "loadTime", loadTime },
            });

            GeniesTelemetry.RecordEvent(
                TelemetryEvent.Create("avatar_core_sdk_initialized", properties)
            );
        }

        private static void OnAssetEquipped(string wearableId)
        {
            var properties = WithSdkMetadata(new Dictionary<string, object>
            {
                { "wearableid", string.IsNullOrEmpty(wearableId) ? "" : wearableId }
            });

            GeniesTelemetry.RecordEvent(
                TelemetryEvent.Create("avatar_editor_asset_equipped", properties)
            );
        }

        private static void OnAssetUnequipped(string wearableId)
        {
            var properties = WithSdkMetadata(new Dictionary<string, object>
            {
                { "wearableid", string.IsNullOrEmpty(wearableId) ? "" : wearableId }
            });

            GeniesTelemetry.RecordEvent(
                TelemetryEvent.Create("avatar_editor_asset_unequipped", properties)
            );
        }

        private static void OnSkinColorSet() =>
            RecordEvent("avatar_editor_skin_color_set");

        private static void OnTattooEquipped(string tattooId)
        {
            var properties = WithSdkMetadata(new Dictionary<string, object>
            {
                { "tattooid", string.IsNullOrEmpty(tattooId) ? "" : tattooId }
            });

            GeniesTelemetry.RecordEvent(
                TelemetryEvent.Create("avatar_editor_tattoo_equipped", properties)
            );
        }

        private static void OnTattooUnequipped(string tattooId)
        {
            var properties = WithSdkMetadata(new Dictionary<string, object>
            {
                { "tattooid", string.IsNullOrEmpty(tattooId) ? "" : tattooId }
            });

            GeniesTelemetry.RecordEvent(
                TelemetryEvent.Create("avatar_editor_tattoo_unequipped", properties)
            );
        }

        private static void OnBodyPresetSet() =>
            RecordEvent("avatar_editor_body_preset_set");

        private static void OnBodyTypeSet() =>
            RecordEvent("avatar_editor_body_type_set");

        private static void OnAvatarSaved() =>
            RecordEvent("avatar_editor_avatar_definition_saved");

        private static void OnAvatarSavedLocally() =>
            RecordEvent("avatar_editor_avatar_definition_saved_locally");

        private static void OnAvatarSavedToCloud() =>
            RecordEvent("avatar_editor_avatar_definition_saved_to_cloud");

        private static void OnAvatarLoaded() =>
            RecordEvent("avatar_editor_avatar_loaded_for_editing");

        private static void OnSamplePrefabUsed(string prefabName)
        {
            var properties = WithSdkMetadata(new Dictionary<string, object>
            {
                { "prefab_name", string.IsNullOrEmpty(prefabName) ? "" : prefabName }
            });

            GeniesTelemetry.RecordEvent(
                TelemetryEvent.Create("user_sample_prefab_used", properties)
            );
        }

        private static void RecordEvent(string eventName)
        {
            GeniesTelemetry.RecordEvent(
                TelemetryEvent.Create(
                    eventName,
                    WithSdkMetadata(null)
                )
            );
        }

        private static Dictionary<string, object> WithSdkMetadata(Dictionary<string, object> properties)
        {
            var result = properties != null
                ? new Dictionary<string, object>(properties)
                : new Dictionary<string, object>();

            if (string.IsNullOrWhiteSpace(sdkVersion))
            {
                sdkVersion = GetSdkVersionFromCache();
            }

            // Context is also set in the low level telemetry script, but eventually all the SDK specific value setting
            // will be moved here.
            result["context"] = ContextValue;
            result["sdkVersion"] = string.IsNullOrWhiteSpace(sdkVersion) ? "unknown" : sdkVersion;

            return result;
        }

        private static string GetSdkVersionFromCache()
        {
            try
            {
                var cache = Resources.Load<GeniesSdkVersionCache>(GeniesTelemetryInstaller.CacheName);
                if (cache != null &&
                    !string.IsNullOrWhiteSpace(cache.Version) &&
                    cache.Version != "unknown")
                {
                    return cache.Version;
                }
            }
            catch
            {
            }

            return "unknown";
        }
    }
}
#endif
