using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Genies.Sdk.Samples.DebugSdkFunctions
{
#if UNITY_EDITOR
    /// <summary>
    /// Editor window that shows the full list of default wearables; clicking an item equips it via EquipWearableAsync.
    /// </summary>
    internal class DefaultWearablesListWindow : EditorWindow
    {
        private List<WearableAssetInfo> _assets;
        private ManagedAvatar _avatar;
        private string _categoryTitle;
        private Vector2 _scrollPosition;

        public static void Show(List<WearableAssetInfo> assets, ManagedAvatar avatar, string categoryTitle)
        {
            var window = GetWindow<DefaultWearablesListWindow>(true, "Default Wearables – Click to Equip", true);
            window._assets = assets;
            window._avatar = avatar;
            window._categoryTitle = categoryTitle;
            window._scrollPosition = Vector2.zero;
            window.minSize = new Vector2(360, 320);
        }

        private void OnGUI()
        {
            if (_assets == null || _assets.Count == 0)
            {
                EditorGUILayout.HelpBox("No wearable assets in list.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Category: {_categoryTitle}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Count: {_assets.Count}. Click an item to equip on the debug avatar.", EditorStyles.miniLabel);

            if (_avatar == null)
            {
                EditorGUILayout.HelpBox("No avatar selected! Spawn an avatar first and assign the Avatar to Debug in the inspector", MessageType.Warning);
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            for (int i = 0; i < _assets.Count; i++)
            {
                var asset = _assets[i];
                var label = string.IsNullOrEmpty(asset.Name) ? asset.AssetId : $"{asset.Name} ({asset.AssetId})";
                EditorGUILayout.BeginHorizontal();
                bool canEquip = _avatar != null;
                EditorGUI.BeginDisabledGroup(!canEquip);
                if (GUILayout.Button(label, GUILayout.ExpandWidth(true), GUILayout.MinHeight(22)))
                {
                    EquipWearableAsync(asset);
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private async void EquipWearableAsync(WearableAssetInfo asset)
        {
            if (_avatar == null)
            {
                EditorUtility.DisplayDialog("Equip Wearable", "No avatar selected! Spawn an avatar first and assign the Avatar to Debug in the inspector.", "OK");
                return;
            }

            try
            {
                await AvatarSdk.EquipWearableAsync(_avatar, asset);
                Debug.Log($"Equipped wearable: {asset.Name} ({asset.AssetId})");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to equip wearable: {ex.Message}");
                EditorUtility.DisplayDialog("⚠️ Equip Wearable", $"Error: {ex.Message}", "OK");
            }
        }
    }

    /// <summary>
    /// Editor window that shows the full list of default makeup; clicking an item equips it via EquipMakeupAsync.
    /// </summary>
    internal class DefaultMakeupListWindow : EditorWindow
    {
        private List<AvatarMakeupInfo> _assets;
        private ManagedAvatar _avatar;
        private string _categoryTitle;
        private Vector2 _scrollPosition;

        public static void Show(List<AvatarMakeupInfo> assets, ManagedAvatar avatar, string categoryTitle)
        {
            var window = GetWindow<DefaultMakeupListWindow>(true, "Default Makeup – Click to Equip", true);
            window._assets = assets;
            window._avatar = avatar;
            window._categoryTitle = categoryTitle;
            window._scrollPosition = Vector2.zero;
            window.minSize = new Vector2(360, 320);
        }

        private void OnGUI()
        {
            if (_assets == null || _assets.Count == 0)
            {
                EditorGUILayout.HelpBox("No makeup assets in list.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Category: {_categoryTitle}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Count: {_assets.Count}. Click an item to equip on the debug avatar.", EditorStyles.miniLabel);

            if (_avatar == null)
            {
                EditorGUILayout.HelpBox("No avatar selected! Spawn an avatar first and assign the Avatar to Debug in the inspector.", MessageType.Warning);
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            for (int i = 0; i < _assets.Count; i++)
            {
                var asset = _assets[i];
                var label =   $"({asset.AssetId})";
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label, GUILayout.ExpandWidth(true), GUILayout.MinHeight(22));
                EditorGUI.BeginDisabledGroup(_avatar == null);
                if (GUILayout.Button("Equip", GUILayout.Width(60), GUILayout.MinHeight(22)))
                {
                    EquipMakeupAsync(asset, equip: true);
                }
                if (GUILayout.Button("Unequip", GUILayout.Width(60), GUILayout.MinHeight(22)))
                {
                    EquipMakeupAsync(asset, equip: false);
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private async void EquipMakeupAsync(AvatarMakeupInfo asset, bool equip)
        {
            if (_avatar == null)
            {
                EditorUtility.DisplayDialog(equip ? "Equip Makeup" : "Unequip Makeup", "No avatar selected! Spawn an avatar first and assign the Avatar to Debug in the inspector.", "OK");
                return;
            }

            try
            {
                if (equip)
                {
                    await AvatarSdk.EquipMakeupAsync(_avatar, asset);
                    Debug.Log($"Equipped makeup: ({asset.AssetId})");
                }
                else
                {
                    if (!_avatar.IsAssetEquipped(asset.AssetId) )
                    {
                        Debug.LogError($"Failed to {(equip ? "equip" : "unequip")} makeup: Error: Asset ID: {asset.AssetId} is not equipped");
                        EditorUtility.DisplayDialog($"⚠️ {(equip ? "Equip" : "Unequip")} Makeup", $"Error: Asset ID: {asset.AssetId} is not equipped", "OK");
                        return;
                    }
                    await AvatarSdk.UnEquipMakeupAsync(_avatar, asset);
                    Debug.Log($"UnEquipped makeup: ({asset.AssetId})");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to {(equip ? "equip" : "unequip")} makeup: {ex.Message}");
                EditorUtility.DisplayDialog($"⚠️ {(equip ? "Equip" : "Unequip")} Makeup", $"Error: {ex.Message}", "OK");
            }
        }
    }

    /// <summary>
    /// Editor window that shows the full list of default hair assets; clicking an item equips it via EquipHairAsync.
    /// </summary>
    internal class DefaultHairListWindow : EditorWindow
    {
        private List<WearableAssetInfo> _assets;
        private ManagedAvatar _avatar;
        private HairType _hairType;
        private Vector2 _scrollPosition;

        public static void Show(List<WearableAssetInfo> assets, ManagedAvatar avatar, HairType hairType)
        {
            var window = GetWindow<DefaultHairListWindow>(true, "Default Hair – Click to Equip", true);
            window._assets = assets;
            window._avatar = avatar;
            window._hairType = hairType;
            window._scrollPosition = Vector2.zero;
            window.minSize = new Vector2(360, 320);
        }

        private void OnGUI()
        {
            if (_assets == null || _assets.Count == 0)
            {
                EditorGUILayout.HelpBox("No hair assets in list.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Hair type: {_hairType}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Count: {_assets.Count}. Click an item to equip on the debug avatar.", EditorStyles.miniLabel);

            if (_avatar == null)
            {
                EditorGUILayout.HelpBox("No avatar selected. No avatar selected! Spawn an avatar first and assign the Avatar to Debug in the inspector.", MessageType.Warning);
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            for (int i = 0; i < _assets.Count; i++)
            {
                var asset = _assets[i];
                var label = $"({asset.AssetId})";
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(_avatar == null);
                if (GUILayout.Button(label, GUILayout.ExpandWidth(true), GUILayout.MinHeight(22)))
                {
                    EquipHairAsync(asset);
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private async void EquipHairAsync(WearableAssetInfo asset)
        {
            if (_avatar == null)
            {
                EditorUtility.DisplayDialog("Equip Hair", "No avatar selected! Spawn an avatar first and assign the Avatar to Debug in the inspector.", "OK");
                return;
            }

            try
            {
                await AvatarSdk.EquipHairAsync(_avatar, asset);
                Debug.Log($"Equipped hair: {asset.Name} ({asset.AssetId})");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to equip hair: {ex.Message}");
                EditorUtility.DisplayDialog("⚠️ Equip Hair", $"Error: {ex.Message}", "OK");
            }
        }
    }

    /// <summary>
    /// Editor window that shows the full list of default tattoos; clicking an item equips it via EquipTattooAsync at the chosen slot.
    /// </summary>
    internal class DefaultTattoosListWindow : EditorWindow
    {
        private List<AvatarTattooInfo> _assets;
        private ManagedAvatar _avatar;
        private MegaSkinTattooSlot _slot;
        private Vector2 _scrollPosition;

        public static void Show(List<AvatarTattooInfo> assets, ManagedAvatar avatar, MegaSkinTattooSlot slot)
        {
            var window = GetWindow<DefaultTattoosListWindow>(true, "Default Tattoos – Click to Equip", true);
            window._assets = assets;
            window._avatar = avatar;
            window._slot = slot;
            window._scrollPosition = Vector2.zero;
            window.minSize = new Vector2(360, 320);
        }

        private void OnGUI()
        {
            if (_assets == null || _assets.Count == 0)
            {
                EditorGUILayout.HelpBox("No tattoo assets in list.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Slot: {_slot}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Count: {_assets.Count}. Click an item to equip on the debug avatar at this slot.", EditorStyles.miniLabel);

            if (_avatar == null)
            {
                EditorGUILayout.HelpBox("No avatar selected! Spawn an avatar first and assign the Avatar to Debug in the inspector.", MessageType.Warning);
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            for (int i = 0; i < _assets.Count; i++)
            {
                var asset = _assets[i];
                var label = $"({asset.AssetId})";
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(_avatar == null);
                if (GUILayout.Button(label, GUILayout.ExpandWidth(true), GUILayout.MinHeight(22)))
                {
                    EquipTattooAsync(asset);
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private async void EquipTattooAsync(AvatarTattooInfo asset)
        {
            if (_avatar == null)
            {
                EditorUtility.DisplayDialog("Equip Tattoo", "No avatar selected! Spawn an avatar first and assign the Avatar to Debug in the inspector.", "OK");
                return;
            }

            try
            {
                await AvatarSdk.EquipTattooAsync(_avatar, asset, _slot);
                Debug.Log($"Equipped tattoo: ({asset.AssetId}) at {_slot}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to equip tattoo: {ex.Message}");
                EditorUtility.DisplayDialog("⚠️ Equip Tattoo", $"Error: {ex.Message}", "OK");
            }
        }
    }

    /// <summary>
    /// Editor window that shows the full list of default color presets; clicking an item applies it via SetColorAsync.
    /// </summary>
    internal class DefaultColorsListWindow : EditorWindow
    {
        private List<IAvatarColor> _colors;
        private ManagedAvatar _avatar;
        private string _categoryTitle;
        private Vector2 _scrollPosition;

        public static void Show(List<IAvatarColor> colors, ManagedAvatar avatar, string categoryTitle)
        {
            var window = GetWindow<DefaultColorsListWindow>(true, "Default Colors – Click to Apply", true);
            window._colors = colors;
            window._avatar = avatar;
            window._categoryTitle = categoryTitle;
            window._scrollPosition = Vector2.zero;
            window.minSize = new Vector2(360, 320);
        }

        private void OnGUI()
        {
            if (_colors == null || _colors.Count == 0)
            {
                EditorGUILayout.HelpBox("No color presets in list.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Color Type: {_categoryTitle}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Count: {_colors.Count}. Click an item to apply the color on the debug avatar.", EditorStyles.miniLabel);

            if (_avatar == null)
            {
                EditorGUILayout.HelpBox("No avatar selected! Spawn an avatar first and assign the Avatar to Debug in the inspector.", MessageType.Warning);
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            for (int i = 0; i < _colors.Count; i++)
            {
                var preset = _colors[i];
                var colorsStr = preset.Hexes != null && preset.Hexes.Length > 0
                    ? string.Join(", ", preset.Hexes.Take(4).Select(c => $"#{ColorUtility.ToHtmlStringRGB(c)}"))
                    : "—";
                var nameOrId =  (preset.AssetId ?? $"Color {i + 1}");
                var customLabel = preset.IsCustom ? " (Custom)" : "";
                var label = $"{nameOrId}{customLabel} [{colorsStr}]";
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(_avatar == null);
                if (GUILayout.Button(label, GUILayout.ExpandWidth(true), GUILayout.MinHeight(22)))
                {
                    SetColorAsync(preset);
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private async void SetColorAsync(IAvatarColor color)
        {
            if (_avatar == null)
            {
                EditorUtility.DisplayDialog("Set Color", "No avatar selected! Spawn an avatar first and assign the Avatar to Debug in the inspector.", "OK");
                return;
            }

            try
            {
                bool success = await AvatarSdk.SetColorAsync(_avatar, color);
                var nameOrId =  (color.AssetId ?? "Color");
                if (success)
                {
                    Debug.Log($"Applied color: {nameOrId}");
                }
                else
                {
                    EditorUtility.DisplayDialog("⚠️ Set Color", $"Failed to apply: {nameOrId}", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to set color: {ex.Message}");
                EditorUtility.DisplayDialog("⚠️ Set Color", $"Error: {ex.Message}", "OK");
            }
        }
    }

    /// <summary>
    /// Editor window that lists user (custom) colors from <see cref="AvatarSdk.GetUserColorsAsync"/>; clicking an entry deletes it via <see cref="AvatarSdk.DeleteUserColorAsync"/>.
    /// </summary>
    internal class DeleteUserColorListWindow : EditorWindow
    {
        private List<IAvatarColor> _colors;
        private UserColorType _userColorType;
        private Vector2 _scrollPosition;

        public static void Show(List<IAvatarColor> colors, UserColorType userColorType)
        {
            var window = GetWindow<DeleteUserColorListWindow>(true, "User Colors – Click to Delete", true);
            window._colors = colors ?? new List<IAvatarColor>();
            window._userColorType = userColorType;
            window._scrollPosition = Vector2.zero;
            window.minSize = new Vector2(420, 360);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField($"User color type: {_userColorType}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Click a row to delete that user color from inventory (you will be asked to confirm).",
                EditorStyles.miniLabel);

            if (_colors == null || _colors.Count == 0)
            {
                EditorGUILayout.HelpBox("No colors in list.", MessageType.Info);
                return;
            }

            var deletableCount = 0;
            foreach (var c in _colors)
            {
                if (c is IAvatarCustomColor cc && !string.IsNullOrEmpty(cc.InstanceId))
                {
                    deletableCount++;
                }
            }

            if (deletableCount == 0)
            {
                EditorGUILayout.HelpBox(
                    "No entries with a non-empty InstanceId. Create a user color first or try another category.",
                    MessageType.Info);
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            for (var i = 0; i < _colors.Count; i++)
            {
                var entry = _colors[i];
                if (entry is IAvatarCustomColor cc && !string.IsNullOrEmpty(cc.InstanceId))
                {
                    var colorsStr = entry.Hexes != null && entry.Hexes.Length > 0
                        ? string.Join(", ", entry.Hexes.Take(4).Select(c => $"#{ColorUtility.ToHtmlStringRGB(c)}"))
                        : "—";
                    var label = $"{cc.InstanceId} — {entry.Kind} [{colorsStr}]";
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(label, GUILayout.ExpandWidth(true), GUILayout.MinHeight(22)))
                    {
                        DeleteUserColorAsync(cc);
                    }

                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        var reason = entry is not IAvatarCustomColor
                            ? $"Not {nameof(IAvatarCustomColor)}"
                            : "Missing InstanceId";
                        EditorGUILayout.LabelField($"[{reason}] {entry.Kind}", EditorStyles.miniLabel);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private async void DeleteUserColorAsync(IAvatarCustomColor color)
        {
            var id = color.InstanceId ?? "?";
            if (!EditorUtility.DisplayDialog(
                    "Delete user color",
                    $"Permanently delete this user color?\n\nInstanceId: {id}\nKind: {color.Kind}",
                    "Delete",
                    "Cancel"))
            {
                return;
            }

            try
            {
                await AvatarSdk.DeleteUserColorAsync(color);
                Debug.Log($"DeleteUserColorAsync completed for {id}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete user color: {ex.Message}");
                EditorUtility.DisplayDialog("⚠️ Delete User Color", $"Error: {ex.Message}", "OK");
            }
        }
    }

    /// <summary>
    /// Editor window that lists user colors; clicking an entry updates it via <see cref="AvatarSdk.UpdateUserColorAsync"/> with the provided color values.
    /// </summary>
    internal class UpdateUserColorListWindow : EditorWindow
    {
        private List<IAvatarColor> _colors;
        private UserColorType _userColorType;
        private List<Color> _newColors;
        private Vector2 _scrollPosition;

        public static void Show(List<IAvatarColor> colors, UserColorType userColorType, List<Color> newColors)
        {
            var window = GetWindow<UpdateUserColorListWindow>(true, "User Colors – Click to Update", true);
            window._colors = colors ?? new List<IAvatarColor>();
            window._userColorType = userColorType;
            window._newColors = newColors != null && newColors.Count > 0
                ? new List<Color>(newColors)
                : new List<Color> { Color.black };
            window._scrollPosition = Vector2.zero;
            window.minSize = new Vector2(420, 400);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField($"User color type: {_userColorType}", EditorStyles.boldLabel);
            var preview = _newColors != null && _newColors.Count > 0
                ? string.Join(", ", _newColors.Select(c => $"#{ColorUtility.ToHtmlStringRGB(c)}"))
                : "—";
            EditorGUILayout.LabelField(
                $"New values to apply: [{preview}] (from debug inspector when you opened this window).",
                EditorStyles.miniLabel);
            EditorGUILayout.LabelField(
                "Click a row to run UpdateUserColorAsync for that preset (you will be asked to confirm).",
                EditorStyles.miniLabel);

            if (_colors == null || _colors.Count == 0)
            {
                EditorGUILayout.HelpBox("No colors in list.", MessageType.Info);
                return;
            }

            var updatableCount = 0;
            foreach (var c in _colors)
            {
                if (c is IAvatarCustomColor cc && !string.IsNullOrEmpty(cc.InstanceId))
                {
                    updatableCount++;
                }
            }

            if (updatableCount == 0)
            {
                EditorGUILayout.HelpBox(
                    "No entries with a non-empty InstanceId. Create a user color first or try another category.",
                    MessageType.Info);
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            for (var i = 0; i < _colors.Count; i++)
            {
                var entry = _colors[i];
                if (entry is IAvatarCustomColor cc && !string.IsNullOrEmpty(cc.InstanceId))
                {
                    var colorsStr = entry.Hexes != null && entry.Hexes.Length > 0
                        ? string.Join(", ", entry.Hexes.Take(4).Select(c => $"#{ColorUtility.ToHtmlStringRGB(c)}"))
                        : "—";
                    var label = $"{cc.InstanceId} — {entry.Kind} [{colorsStr}]";
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(label, GUILayout.ExpandWidth(true), GUILayout.MinHeight(22)))
                    {
                        UpdateUserColorAsync(cc);
                    }

                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        var reason = entry is not IAvatarCustomColor
                            ? $"Not {nameof(IAvatarCustomColor)}"
                            : "Missing InstanceId";
                        EditorGUILayout.LabelField($"[{reason}] {entry.Kind}", EditorStyles.miniLabel);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private async void UpdateUserColorAsync(IAvatarCustomColor color)
        {
            var id = color.InstanceId ?? "?";
            var preview = _newColors != null && _newColors.Count > 0
                ? string.Join(", ", _newColors.Select(c => $"#{ColorUtility.ToHtmlStringRGB(c)}"))
                : "—";
            if (!EditorUtility.DisplayDialog(
                    "Update user color",
                    $"Apply new colors to this preset?\n\nInstanceId: {id}\nKind: {color.Kind}\n\nNew: [{preview}]",
                    "Update",
                    "Cancel"))
            {
                return;
            }

            try
            {
                await AvatarSdk.UpdateUserColorAsync(color, _newColors);
                Debug.Log($"UpdateUserColorAsync completed for {id}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to update user color: {ex.Message}");
                EditorUtility.DisplayDialog("⚠️ Update User Color", $"Error: {ex.Message}", "OK");
            }
        }
    }

    /// <summary>
    /// Editor window that shows the full list of default avatar features; clicking an item applies it via SetAvatarFeatureAsync.
    /// </summary>
    internal class DefaultAvatarFeaturesListWindow : EditorWindow
    {
        private List<AvatarFeaturesInfo> _assets;
        private ManagedAvatar _avatar;
        private string _categoryTitle;
        private Vector2 _scrollPosition;

        public static void Show(List<AvatarFeaturesInfo> assets, ManagedAvatar avatar, string categoryTitle)
        {
            var window = GetWindow<DefaultAvatarFeaturesListWindow>(true, "Default Avatar Features – Click to Apply", true);
            window._assets = assets;
            window._avatar = avatar;
            window._categoryTitle = categoryTitle;
            window._scrollPosition = Vector2.zero;
            window.minSize = new Vector2(360, 320);
        }

        private void OnGUI()
        {
            if (_assets == null || _assets.Count == 0)
            {
                EditorGUILayout.HelpBox("No avatar feature assets in list.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Category: {_categoryTitle}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Count: {_assets.Count}. Click an item to apply the feature on the debug avatar.", EditorStyles.miniLabel);

            if (_avatar == null)
            {
                EditorGUILayout.HelpBox("No avatar selected! Spawn an avatar first and assign the Avatar to Debug in the inspector.", MessageType.Warning);
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            for (int i = 0; i < _assets.Count; i++)
            {
                var asset = _assets[i];
                var subcategoriesStr = asset.SubCategories != null && asset.SubCategories.Count > 0
                    ? string.Join(", ", asset.SubCategories)
                    : "";
                var label = !string.IsNullOrEmpty(asset.AssetId) ? $"{asset.AssetId}" : $"Feature {i + 1}";
                if (!string.IsNullOrEmpty(subcategoriesStr))
                    label += $" [{subcategoriesStr}]";
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(_avatar == null);
                if (GUILayout.Button(label, GUILayout.ExpandWidth(true), GUILayout.MinHeight(22)))
                {
                    SetAvatarFeatureAsync(asset);
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private async void SetAvatarFeatureAsync(AvatarFeaturesInfo feature)
        {
            if (_avatar == null)
            {
                EditorUtility.DisplayDialog("Set Avatar Feature", "No avatar selected! Spawn an avatar first and assign the Avatar to Debug in the inspector.", "OK");
                return;
            }

            try
            {
                bool success = await AvatarSdk.SetAvatarFeatureAsync(_avatar, feature);
                var nameOrId = feature.AssetId ?? "Feature";
                if (success)
                {
                    Debug.Log($"Applied avatar feature: {nameOrId}");
                }
                else
                {
                    EditorUtility.DisplayDialog("⚠️ Set Avatar Feature", $"Failed to apply: {nameOrId}", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to set avatar feature: {ex.Message}");
                EditorUtility.DisplayDialog("⚠️ Set Avatar Feature", $"Error: {ex.Message}", "OK");
            }
        }
    }
#endif
}
