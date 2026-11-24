using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Genies.Assets.Services;
using Genies.Avatars.Sdk;
using Genies.Avatars.Services;
using Genies.CrashReporting;
using Genies.Inventory;
using Genies.Inventory.UIData;
using Genies.Login.Native;
using Genies.Looks.Customization.Commands;
using Genies.Naf;
using Genies.Naf.Content;
using Genies.Refs;
using GnWrappers;
using Genies.ServiceManagement;
using Genies.Services.Model;
using Genies.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif
using CancellationToken = System.Threading.CancellationToken;

namespace Genies.Avatars.Customization
{
    /// <summary>
    /// Implementation of IAvatarCustomizationService providing avatar customization APIs
    /// without requiring the avatar editor UI.
    /// </summary>
    internal class AvatarCustomizationService : IAvatarCustomizationService, IDisposable
    {
        private const string _headTransformPath = "Root/Hips/Spine/Spine1/Spine2/Neck/Head";

        private readonly HashSet<Ref<Sprite>> _spritesGivenToUser = new();

        private InventoryUIDataProvider<ColorTaggedInventoryAsset, BasicInventoryUiData> _defaultWearablesProvider;
        private InventoryUIDataProvider<DefaultInventoryAsset, BasicInventoryUiData> _defaultFlairProvider;
        private InventoryUIDataProvider<ColorTaggedInventoryAsset, BasicInventoryUiData> _userWearablesProvider;
        private InventoryUIDataProvider<DefaultInventoryAsset, BasicInventoryUiData> _defaultMakeupProvider;
        private InventoryUIDataProvider<DefaultAvatarBaseAsset, BasicInventoryUiData> _defaultAvatarBaseProvider;
        private InventoryUIDataProvider<DefaultInventoryAsset, BasicInventoryUiData> _defaultImageLibraryProvider;

        public void Dispose()
        {
            foreach (var sprite in _spritesGivenToUser)
            {
                sprite.Dispose();
            }
            _spritesGivenToUser.Clear();

            _defaultWearablesProvider?.Dispose();
            _defaultWearablesProvider = null;
            _defaultFlairProvider?.Dispose();
            _defaultFlairProvider = null;
            _userWearablesProvider?.Dispose();
            _userWearablesProvider = null;
            _defaultMakeupProvider?.Dispose();
            _defaultMakeupProvider = null;
            _defaultAvatarBaseProvider?.Dispose();
            _defaultAvatarBaseProvider = null;
            _defaultImageLibraryProvider?.Dispose();
            _defaultImageLibraryProvider = null;
        }

        #region Sprite Reference Management

        private void KeepSpriteReference(Ref<Sprite> spriteRef)
        {
            _spritesGivenToUser.Add(spriteRef);
        }

        public void RemoveSpriteReference(Ref<Sprite> spriteRef)
        {
            spriteRef.Dispose();

            if (_spritesGivenToUser.Contains(spriteRef))
            {
                _spritesGivenToUser.Remove(spriteRef);
            }
        }
        #endregion

        #region Cached UI Data Providers

        private InventoryUIDataProvider<ColorTaggedInventoryAsset, BasicInventoryUiData> GetDefaultWearablesProvider()
        {
            return _defaultWearablesProvider ??= new InventoryUIDataProvider<ColorTaggedInventoryAsset, BasicInventoryUiData>(
                UIDataProviderConfigs.DefaultWearablesConfig,
                ServiceManager.Get<IAssetsService>()
            );
        }

        private InventoryUIDataProvider<DefaultInventoryAsset, BasicInventoryUiData> GetDefaultFlairProvider()
        {
            return _defaultFlairProvider ??= new InventoryUIDataProvider<DefaultInventoryAsset, BasicInventoryUiData>(
                UIDataProviderConfigs.DefaultAvatarFlairConfig,
                ServiceManager.Get<IAssetsService>()
            );
        }

        private InventoryUIDataProvider<ColorTaggedInventoryAsset, BasicInventoryUiData> GetUserWearablesProvider()
        {
            return _userWearablesProvider ??= new InventoryUIDataProvider<ColorTaggedInventoryAsset, BasicInventoryUiData>(
                UIDataProviderConfigs.UserWearablesConfig,
                ServiceManager.Get<IAssetsService>()
            );
        }

        private InventoryUIDataProvider<DefaultInventoryAsset, BasicInventoryUiData> GetDefaultMakeupProvider()
        {
            return _defaultMakeupProvider ??= new InventoryUIDataProvider<DefaultInventoryAsset, BasicInventoryUiData>(
                UIDataProviderConfigs.DefaultAvatarMakeupConfig,
                ServiceManager.Get<IAssetsService>()
            );
        }

        private InventoryUIDataProvider<DefaultAvatarBaseAsset, BasicInventoryUiData> GetDefaultAvatarBaseProvider()
        {
            return _defaultAvatarBaseProvider ??= new InventoryUIDataProvider<DefaultAvatarBaseAsset, BasicInventoryUiData>(
                UIDataProviderConfigs.DefaultAvatarBaseConfig,
                ServiceManager.Get<IAssetsService>()
            );
        }

        private InventoryUIDataProvider<DefaultInventoryAsset, BasicInventoryUiData> GetDefaultImageLibraryProvider()
        {
            return _defaultImageLibraryProvider ??= new InventoryUIDataProvider<DefaultInventoryAsset, BasicInventoryUiData>(
                UIDataProviderConfigs.DefaultImageLibraryConfig,
                ServiceManager.Get<IAssetsService>()
            );
        }

        #endregion

        #region Asset Granting

        public async UniTask<(bool, string)> GiveAssetToUserAsync(string assetId)
        {
            try
            {
                if (string.IsNullOrEmpty(assetId))
                {
                    string error = "Asset id cannot be null or empty";
                    CrashReporter.LogError(error);
                    return (false, error);
                }

                if (!GeniesLoginSdk.IsUserSignedIn())
                {
                    string error = "You need to be logged in to give an asset to a user";
                    CrashReporter.LogError(error);
                    return (false, error);
                }

                var defaultInventoryService = ServiceManager.Get<IDefaultInventoryService>();
                return await defaultInventoryService.GiveAssetToUserAsync(assetId);
            }
            catch (Exception ex)
            {
                string error = $"Failed to give asset to user: {ex.Message}";
                CrashReporter.LogError(error);
                return (false, error);
            }
        }

        #endregion

        #region Equip / Unequip

        public async UniTask EquipOutfitAsync(GeniesAvatar avatar, string wearableId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (avatar == null)
                {
                    CrashReporter.LogError("An avatar is required in order to equip an asset.");
                    return;
                }

                if (avatar.Controller.IsAssetEquipped(wearableId))
                {
                    CrashReporter.LogWarning("Asset is already equipped.");
                    return;
                }

                var command = new EquipNativeAvatarAssetCommand(wearableId, avatar.Controller);
                await command.ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to equip outfit with ID '{wearableId}': {ex.Message}");
            }
        }

        public async UniTask UnEquipOutfitAsync(GeniesAvatar avatar, string wearableId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (avatar == null)
                {
                    CrashReporter.LogError("An avatar is required in order to unequip an asset.");
                    return;
                }

                if (!avatar.Controller.IsAssetEquipped(wearableId))
                {
                    CrashReporter.LogError("Asset is already not equipped.");
                    return;
                }

                var command = new UnequipNativeAvatarAssetCommand(wearableId, avatar.Controller);
                await command.ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to unequip outfit with ID '{wearableId}': {ex.Message}");
            }
        }

        public async UniTask EquipMakeupAsync(GeniesAvatar avatar, DefaultInventoryAsset asset, CancellationToken cancellationToken = default)
        {
            try
            {
                if (avatar == null)
                {
                    CrashReporter.LogError("An avatar is required in order to set avatar makeup.");
                    return;
                }

                if (asset == null || string.IsNullOrEmpty(asset.AssetId))
                {
                    CrashReporter.LogError("A valid asset with a non-empty AssetId is required to set avatar makeup.");
                    return;
                }

                var command = new EquipNativeAvatarAssetCommand(asset.AssetId, avatar.Controller);
                await command.ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to set avatar makeup: {ex.Message}");
            }
        }

        public async UniTask EquipMakeupAsync(GeniesAvatar avatar, string makeupAssetId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (avatar == null)
                {
                    CrashReporter.LogError("An avatar is required in order to set avatar makeup.");
                    return;
                }

                if (string.IsNullOrEmpty(makeupAssetId))
                {
                    CrashReporter.LogError("A non-empty makeup asset ID is required to set avatar makeup.");
                    return;
                }

                var command = new EquipNativeAvatarAssetCommand(makeupAssetId, avatar.Controller);
                await command.ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to set avatar makeup: {ex.Message}");
            }
        }

        public async UniTask UnEquipMakeupAsync(GeniesAvatar avatar, string makeupAssetId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (avatar == null)
                {
                    CrashReporter.LogError("An avatar is required in order to unequip makeup.");
                    return;
                }

                if (string.IsNullOrEmpty(makeupAssetId))
                {
                    CrashReporter.LogError("A non-empty makeup asset ID is required to unequip makeup.");
                    return;
                }

                if (!avatar.Controller.IsAssetEquipped(makeupAssetId))
                {
                    CrashReporter.LogError("Makeup asset is not equipped.");
                    return;
                }

                var command = new UnequipNativeAvatarAssetCommand(makeupAssetId, avatar.Controller);
                await command.ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to unequip makeup with ID '{makeupAssetId}': {ex.Message}");
            }
        }

        public async UniTask EquipHairAsync(GeniesAvatar avatar, string hairAssetId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (avatar == null)
                {
                    CrashReporter.LogError("An avatar is required in order to equip a hair style.");
                    return;
                }

                if (avatar.Controller.IsAssetEquipped(hairAssetId))
                {
                    CrashReporter.LogWarning($"{hairAssetId} asset is already equipped.");
                    return;
                }

                var command = new EquipNativeAvatarAssetCommand(hairAssetId, avatar.Controller);
                await command.ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to equip hair style with ID '{hairAssetId}': {ex.Message}");
            }
        }

        public async UniTask UnEquipHairAsync(GeniesAvatar avatar, HairType hairType, CancellationToken cancellationToken = default)
        {
            try
            {
                if (avatar == null)
                {
                    CrashReporter.LogError("An avatar is required in order to unequip a hair style.");
                    return;
                }

                var equippedIds = avatar.Controller.GetEquippedAssetIds();
                if (equippedIds == null || !equippedIds.Any())
                {
                    CrashReporter.LogWarning("No assets are currently equipped on the avatar.");
                    return;
                }

                IDefaultInventoryService defaultInventoryService = ServiceManager.GetService<IDefaultInventoryService>(null);
                if (defaultInventoryService == null)
                {
                    CrashReporter.LogError("DefaultInventoryService not found");
                    return;
                }

                // Eyebrows and Eyelashes: "unequip" by equipping the "none" asset from faceblendshape.
                if (hairType == HairType.Eyebrows || hairType == HairType.Eyelashes)
                {
                    if (avatar.Controller == null)
                    {
                        CrashReporter.LogError("An avatar is required in order to unequip eyebrows/eyelashes.");
                        return;
                    }

                    string subcategory = hairType == HairType.Eyebrows ? "eyebrows" : "eyelashes";
                    List<string> categories = null;
                    if (!string.IsNullOrEmpty(subcategory))
                    {
                        categories = new List<string> { subcategory };
                    }

                    var allData = await defaultInventoryService.GetDefaultAvatarFlair(limit: null, categories);
                    var noneAsset = allData.FirstOrDefault(a =>
                        string.Equals(a?.Name, "none", StringComparison.OrdinalIgnoreCase));
                    if (noneAsset == null || string.IsNullOrEmpty(noneAsset.AssetId))
                    {
                        CrashReporter.LogWarning($"No 'none' asset found for subcategory '{subcategory}'.");
                        return;
                    }

                    var eyecommand = new EquipNativeAvatarAssetCommand(noneAsset.AssetId, avatar.Controller);
                    await eyecommand.ExecuteAsync(cancellationToken);
                    return;
                }

                // Determine category string based on hair type
                string category = hairType == HairType.FacialHair ? "facialhair" : "hair";

                var allHairAssets = await defaultInventoryService.GetDefaultWearables(categories: new List<string> { category });

                if (allHairAssets == null || !allHairAssets.Any())
                {
                    CrashReporter.LogWarning($"No hair assets found in inventory service.");
                    return;
                }

                var hairAssetIds = allHairAssets.Select(w => w.AssetId).ToList();

                var converter = ServiceManager.GetService<IAssetIdConverter>(null);
                if (converter == null)
                {
                    CrashReporter.LogError("IAssetIdConverter service not found.");
                    return;
                }

                var convertedIds = await converter.ConvertToUniversalIdsAsync(hairAssetIds);
                if (convertedIds == null || convertedIds.Count == 0)
                {
                    CrashReporter.LogWarning("Failed to convert hair asset IDs to universal IDs.");
                    return;
                }

                var convertedSet = convertedIds.Values.ToHashSet();
                var matchingHairId = equippedIds.FirstOrDefault(id => convertedSet.Contains(id));

                if (string.IsNullOrEmpty(matchingHairId))
                {
                    CrashReporter.LogWarning($"No hair asset is currently equipped on the avatar.");
                    return;
                }

                var command = new UnequipNativeAvatarAssetCommand(matchingHairId, avatar.Controller);
                await command.ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to unequip hair style: {ex.Message}");
            }
        }

        public async UniTask EquipTattooAsync(GeniesAvatar avatar, AvatarTattooInfo tattooInfo, MegaSkinTattooSlot tattooSlot, CancellationToken cancellationToken = default)
        {
            try
            {
                if (avatar == null)
                {
                    CrashReporter.LogError("An avatar is required in order to equip a tattoo.");
                    return;
                }

                if (string.IsNullOrEmpty(tattooInfo.AssetId))
                {
                    CrashReporter.LogError("Tattoo ID cannot be null or empty");
                    return;
                }

                var command = new EquipNativeAvatarTattooCommand(tattooInfo.AssetId, tattooSlot, avatar.Controller);
                await command.ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to equip tattoo with ID '{tattooInfo.AssetId}' at slot '{tattooSlot}': {ex.Message}");
            }
        }

        public async UniTask<string> UnEquipTattooAsync(GeniesAvatar avatar, MegaSkinTattooSlot tattooSlot, CancellationToken cancellationToken = default)
        {
            try
            {
                if (avatar == null)
                {
                    CrashReporter.LogError("An avatar is required in order to unequip a tattoo.");
                    return null;
                }

                var command = new UnequipNativeAvatarTattooCommand(tattooSlot, avatar.Controller);
                string unequippedTattooId = command.GetPreviousTattooGuid();
                await command.ExecuteAsync(cancellationToken);
                return unequippedTattooId;
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to unequip tattoo at slot '{tattooSlot}': {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Colors

        public async UniTask SetSkinColorAsync(GeniesAvatar avatar, Color skinColor, CancellationToken cancellationToken = default)
        {
            try
            {
                if (avatar == null)
                {
                    CrashReporter.LogError("An avatar is required in order to set a skin color.");
                    return;
                }

                var command = new EquipSkinColorCommand(skinColor, avatar.Controller);
                await command.ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to equip skin color: {ex.Message}");
            }
        }

        public async UniTask ModifyAvatarHairColorAsync(GeniesAvatar avatar, HairType hairType, Color baseColor, Color colorR, Color colorG, Color colorB, CancellationToken cancellationToken = default)
        {
            try
            {
                if (hairType == HairType.Eyebrows || hairType == HairType.Eyelashes)
                {
                    var colors = new Color[] { baseColor, colorR };
                    await ModifyAvatarFlairColorAsync(avatar, hairType, colors, cancellationToken);
                    return;
                }

                if (avatar == null)
                {
                    CrashReporter.LogError("An avatar is required in order to set hair colors.");
                    return;
                }

                GenieColorEntry[] hairColors;
                if (hairType == HairType.FacialHair)
                {
                    hairColors = new GenieColorEntry[]
                    {
                        new GenieColorEntry { ColorId = GenieColor.FacialhairBase, Value = baseColor },
                        new GenieColorEntry { ColorId = GenieColor.FacialhairR, Value = colorR },
                        new GenieColorEntry { ColorId = GenieColor.FacialhairG, Value = colorG },
                        new GenieColorEntry { ColorId = GenieColor.FacialhairB, Value = colorB }
                    };
                }
                else
                {
                    hairColors = new GenieColorEntry[]
                    {
                        new GenieColorEntry { ColorId = GenieColor.HairBase, Value = baseColor },
                        new GenieColorEntry { ColorId = GenieColor.HairR, Value = colorR },
                        new GenieColorEntry { ColorId = GenieColor.HairG, Value = colorG },
                        new GenieColorEntry { ColorId = GenieColor.HairB, Value = colorB }
                    };
                }

                var command = new SetNativeAvatarColorsCommand(hairColors, avatar.Controller);
                await command.ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to set hair color: {ex.Message}");
            }
        }

        public async UniTask ModifyAvatarFlairColorAsync(GeniesAvatar avatar, HairType hairType, Color[] colors, CancellationToken cancellationToken = default)
        {
            try
            {
                if (hairType != HairType.Eyebrows && hairType != HairType.Eyelashes)
                {
                    CrashReporter.LogError("ModifyAvatarFlairColorAsync supports only HairType.Eyebrows or HairType.Eyelashes.");
                    return;
                }

                if (avatar == null)
                {
                    CrashReporter.LogError("An avatar is required in order to set flair colors.");
                    return;
                }

                if (colors == null || colors.Length < 2)
                {
                    CrashReporter.LogError("Insufficient flair colors.");
                    return;
                }

                GenieColorEntry[] flairColors;
                if (hairType == HairType.Eyelashes)
                {
                    flairColors = new GenieColorEntry[]
                    {
                        new GenieColorEntry { ColorId = GenieColor.EyelashesBase, Value = colors[0] },
                        new GenieColorEntry { ColorId = GenieColor.EyelashesR, Value = colors[1] },
                        new GenieColorEntry { ColorId = GenieColor.EyelashesG, Value = colors[1] },
                        new GenieColorEntry { ColorId = GenieColor.EyelashesB, Value = colors[1] }
                    };
                }
                else
                {
                    flairColors = new GenieColorEntry[]
                    {
                        new GenieColorEntry { ColorId = GenieColor.EyebrowsBase, Value = colors[0] },
                        new GenieColorEntry { ColorId = GenieColor.EyebrowsR, Value = colors[1] },
                        new GenieColorEntry { ColorId = GenieColor.EyebrowsG, Value = colors[1] },
                        new GenieColorEntry { ColorId = GenieColor.EyebrowsB, Value = colors[1] }
                    };
                }

                var command = new SetNativeAvatarColorsCommand(flairColors, avatar.Controller);
                await command.ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to set flair color: {ex.Message}");
            }
        }

        public async UniTask SetMakeupColorAsync(GeniesAvatar avatar, MakeupCategory category, Color[] colors, CancellationToken cancellationToken = default)
        {
            try
            {
                if (avatar == null)
                {
                    CrashReporter.LogError("An avatar is required to modify makeup color.");
                    return;
                }

                if (avatar.Controller == null)
                {
                    CrashReporter.LogError("Avatar controller is null; cannot modify makeup color.");
                    return;
                }

                var presetCategoryInt = MakeupCategoryMapper.ToMakeupPresetCategoryInt(category);
                var command = new EquipMakeupColorCommand(presetCategoryInt, colors ?? Array.Empty<Color>(), avatar.Controller);
                await command.ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to modify makeup color: {ex.Message}");
            }
        }

        public async UniTask<List<IColor>> GetDefaultColorsAsync(ColorType colorType, CancellationToken cancellationToken = default)
        {
            try
            {
                IDefaultInventoryService defaultInventoryService = ServiceManager.GetService<IDefaultInventoryService>(null);
                if (defaultInventoryService == null)
                {
                    CrashReporter.LogError("DefaultInventoryService not found");
                    var emptyColors = new List<Color>();
                    return new List<IColor> { ToIColorValueInternal(colorType, emptyColors) };
                }

                string category = GetColorTypeCategory(colorType);
                List<ColoredInventoryAsset> defaultColors;
                if (colorType == ColorType.Eyes)
                {
                    defaultColors = await defaultInventoryService.GetDefaultAvatarEyes(limit: null, categories: null);
                }
                else
                {
                    defaultColors = await defaultInventoryService.GetDefaultColorPresets(categories: new List<string> { category });
                }

                var result = new List<IColor>();
                if (defaultColors != null)
                {
                    if (colorType == ColorType.MakeupStickers || colorType == ColorType.MakeupLipstick || colorType == ColorType.MakeupFreckles || colorType == ColorType.MakeupFaceGems || colorType == ColorType.MakeupEyeshadow || colorType == ColorType.MakeupBlush)
                    {
                        string makeupSubcategory = MakeupCategoryMapper.ToInternal(GetMakeupCategoryFromColorType(colorType));
                        MakeupCategory makeupCategory = GetMakeupCategoryFromColorType(colorType);
                        var filteredMakeupList = defaultColors.Where(c =>
                            string.Equals(c.Category, "makeup", StringComparison.OrdinalIgnoreCase)
                            && c.SubCategories != null
                            && c.SubCategories.Any(sc => string.Equals(sc, makeupSubcategory, StringComparison.OrdinalIgnoreCase))).ToList();

                        foreach (var colorAsset in filteredMakeupList)
                        {
                            var colors = colorAsset.Colors;
                            bool isEmpty = colors == null || colors.Count == 0;
                            Color clear = Color.clear;
                            Color c0 = isEmpty ? clear : colors[0];
                            Color c1 = (colors != null && colors.Count > 1) ? colors[1] : c0;
                            Color c2 = (colors != null && colors.Count > 2) ? colors[2] : c0;
                            Color c3 = (colors != null && colors.Count > 3) ? colors[3] : c0;
                            var iColor = new MakeupColor(makeupCategory, c0, c1, c2, c3);
                            iColor.Name = colorAsset.Name;
                            iColor.IsCustom = false;
                            iColor.Order = colorAsset.Order;
                            result.Add(iColor);
                        }

                        return result;
                    }

                    foreach (var colorAsset in defaultColors)
                    {
                        var iColor = ToIColorValueInternal(colorType, colorAsset.Colors ?? new List<Color>(),
                            colorAsset.AssetId);
                        iColor.IsCustom = false;
                        result.Add(iColor);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to get default {colorType} colors: {ex.Message}");
                return new List<IColor>();
            }
        }

        public async UniTask<List<ICustomColor>> GetUserColorsAsync(UserColorType colorType, CancellationToken cancellationToken = default)
        {
            try
            {
                IDefaultInventoryService defaultInventoryService = ServiceManager.GetService<IDefaultInventoryService>(null);
                if (defaultInventoryService == null)
                {
                    CrashReporter.LogError("DefaultInventoryService not found");
                    var emptyColors = new List<Color>();
                    return new List<ICustomColor> { ToIColorValueInternalUser(colorType, emptyColors) };
                }

                var result = new List<ICustomColor>();
                string category = GetUserColorTypeCategory(colorType);
                var customColors = await defaultInventoryService.GetCustomColors(category);
                if (customColors != null && customColors.Any())
                {
                    foreach (var customColor in customColors)
                    {
                        var colors = new List<Color>();
                        if (customColor.ColorsHex != null)
                        {
                            foreach (var hex in customColor.ColorsHex)
                            {
                                if (ColorUtility.TryParseHtmlString(hex, out Color color))
                                {
                                    colors.Add(color);
                                }
                            }
                        }

                        var iColor = ToIColorValueInternalUser(colorType, colors, instanceId:  customColor.InstanceId);
                        iColor.IsCustom = true;
                        result.Add(iColor);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to get user {colorType} colors: {ex.Message}");
                return new List<ICustomColor>();
            }
        }


        #endregion

        #region Body Preset / Body Type / Feature Stats

        public async UniTask SetNativeAvatarBodyPresetAsync(GeniesAvatar avatar, GSkelModifierPreset preset, CancellationToken cancellationToken = default)
        {
            try
            {
                if (avatar == null)
                {
                    CrashReporter.LogError("An avatar is required in order to set a body preset.");
                    return;
                }

                if (preset == null)
                {
                    CrashReporter.LogError("Body preset cannot be null");
                    return;
                }

                var command = new SetNativeAvatarBodyPresetCommand(preset, avatar.Controller);
                await command.ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to set body preset: {ex.Message}");
            }
        }

        public GSkelModifierPreset GetNativeAvatarBodyPreset(GeniesAvatar avatar)
        {
            if (avatar?.Controller == null)
            {
                return null;
            }

            return avatar.Controller.GetBodyPreset();
        }

        public Dictionary<AvatarFeatureStat, float> GetAvatarFeatureStats(GeniesAvatar avatar, AvatarFeatureStatType statType)
        {
            var result = new Dictionary<AvatarFeatureStat, float>();
            if (avatar?.Controller == null)
            {
                return result;
            }

            if (statType == AvatarFeatureStatType.Body || statType == AvatarFeatureStatType.All)
            {
                foreach (BodyStats s in Enum.GetValues(typeof(BodyStats)))
                {
                    result[AvatarFeatureStatMapping.ToAvatarFeatureStat(s)] = avatar.GetBodyAttribute(AvatarFeatureStatMapping.GetAttributeId(AvatarFeatureStatMapping.ToAvatarFeatureStat(s)));
                }
            }

            if (statType == AvatarFeatureStatType.EyeBrows || statType == AvatarFeatureStatType.All)
            {
                foreach (EyeBrowsStats s in Enum.GetValues(typeof(EyeBrowsStats)))
                {
                    result[AvatarFeatureStatMapping.ToAvatarFeatureStat(s)] = avatar.GetBodyAttribute(
                        AvatarFeatureStatMapping.GetAttributeId(AvatarFeatureStatMapping.ToAvatarFeatureStat(s)));
                }
            }

            if (statType == AvatarFeatureStatType.Eyes || statType == AvatarFeatureStatType.All)
            {
                foreach (EyeStats s in Enum.GetValues(typeof(EyeStats)))
                {
                    result[AvatarFeatureStatMapping.ToAvatarFeatureStat(s)] = avatar.GetBodyAttribute(AvatarFeatureStatMapping.GetAttributeId(AvatarFeatureStatMapping.ToAvatarFeatureStat(s)));
                }
            }

            if (statType == AvatarFeatureStatType.Jaw || statType == AvatarFeatureStatType.All)
            {
                foreach (JawStats s in Enum.GetValues(typeof(JawStats)))
                {
                    result[AvatarFeatureStatMapping.ToAvatarFeatureStat(s)] = avatar.GetBodyAttribute(AvatarFeatureStatMapping.GetAttributeId(AvatarFeatureStatMapping.ToAvatarFeatureStat(s)));
                }
            }

            if (statType == AvatarFeatureStatType.Lips || statType == AvatarFeatureStatType.All)
            {
                foreach (LipsStats s in Enum.GetValues(typeof(LipsStats)))
                {
                    result[AvatarFeatureStatMapping.ToAvatarFeatureStat(s)] = avatar.GetBodyAttribute(AvatarFeatureStatMapping.GetAttributeId(AvatarFeatureStatMapping.ToAvatarFeatureStat(s)));
                }
            }

            if (statType == AvatarFeatureStatType.Nose || statType == AvatarFeatureStatType.All)
            {
                foreach (NoseStats s in Enum.GetValues(typeof(NoseStats)))
                {
                    result[AvatarFeatureStatMapping.ToAvatarFeatureStat(s)] = avatar.GetBodyAttribute(AvatarFeatureStatMapping.GetAttributeId(AvatarFeatureStatMapping.ToAvatarFeatureStat(s)));
                }
            }

            return result;
        }

        public bool ModifyAvatarFeatureStat(GeniesAvatar avatar, AvatarFeatureStat stat, float value, CancellationToken cancellationToken = default)
        {
            try
            {
                if (avatar?.Controller == null)
                {
                    CrashReporter.LogError("Avatar and controller are required to modify avatar feature stats");
                    return false;
                }

                cancellationToken.ThrowIfCancellationRequested();
                string attributeId = AvatarFeatureStatMapping.GetAttributeId(stat);
                avatar.Controller.SetBodyAttribute(attributeId, Mathf.Clamp(value, -1.0f, 1.0f));
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to modify avatar feature stat: {ex.Message}");
                return false;
            }
        }

        public async UniTask SetAvatarBodyTypeAsync(GeniesAvatar avatar, GenderType genderType, BodySize bodySize, CancellationToken cancellationToken = default)
        {
            try
            {
                if (avatar == null)
                {
                    CrashReporter.LogError("An avatar is required in order to set a body type.");
                    return;
                }

                string presetName = GetPresetName(genderType, bodySize);

                var bodyPreset = AssetPath.Load<GSkelModifierPreset>(presetName);

                if (bodyPreset == null)
                {
                    CrashReporter.LogError($"Failed to load body preset: {presetName}");
                    return;
                }

                var command = new SetNativeAvatarBodyPresetCommand(bodyPreset, avatar.Controller);
                await command.ExecuteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to set avatar body type: {ex.Message}");
            }
        }

        #endregion

        #region Asset Info Retrieval

        public async UniTask<List<WearableAssetInfo>> GetWearableAssetInfoListAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                IDefaultInventoryService defaultInventoryService = ServiceManager.GetService<IDefaultInventoryService>(null);
                var defaultWearables = await defaultInventoryService.GetDefaultWearables();

                if (defaultWearables == null || !defaultWearables.Any())
                {
                    CrashReporter.LogError("No default wearables found in inventory service");
                    return new List<WearableAssetInfo>();
                }

                var provider = GetDefaultWearablesProvider();

                var wearableAssetInfoList = await UniTask.WhenAll(
                    defaultWearables.Select(async wearable =>
                    {
                        var data = await provider.GetDataForAssetId(wearable.AssetId);

                        var info = new WearableAssetInfo
                        {
                            AssetId = wearable.AssetId,
                            AssetType = wearable.AssetType,
                            Name = wearable.Name,
                            Category = wearable.Category,
                            Icon = data.Thumbnail
                        };

                        KeepSpriteReference(data.Thumbnail);

                        return info;
                    })
                );

                return wearableAssetInfoList.ToList();
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to get wearable asset info list: {ex.Message}");
                return new List<WearableAssetInfo>();
            }
        }

        public async UniTask<List<WearableAssetInfo>> GetDefaultWearableAssetsListByCategoriesAsync(List<WardrobeSubcategory> categories, bool forceFetch = false, CancellationToken cancellationToken = default)
        {
            try
            {
                List<string> categoryStrings = null;
                if (categories != null && categories.Count > 0)
                {
                    categoryStrings = categories
                        .Where(c => c != WardrobeSubcategory.none && c != WardrobeSubcategory.all)
                        .Select(c => c.ToString().ToLower())
                        .ToList();
                }

                IDefaultInventoryService defaultInventoryService = ServiceManager.GetService<IDefaultInventoryService>(null);
                if (defaultInventoryService == null)
                {
                    CrashReporter.LogError("DefaultInventoryService not found");
                    return new List<WearableAssetInfo>();
                }

                List<string> flairCategoryStrings = null;
                List<string> wearableCategoryStrings = null;
                if (categoryStrings != null && categoryStrings.Count > 0)
                {
                    var flairCategories = new[] { "eyebrows", "eyelashes" };
                    flairCategoryStrings = categoryStrings.Where(c => flairCategories.Contains(c)).ToList();
                    if (flairCategoryStrings.Count == 0)
                    {
                        flairCategoryStrings = null;
                    }

                    wearableCategoryStrings = categoryStrings.Where(c => !flairCategories.Contains(c)).ToList();
                    if (wearableCategoryStrings.Count == 0)
                    {
                        wearableCategoryStrings = null;
                    }
                }

                var result = new List<WearableAssetInfo>();
                bool fetchAll = categoryStrings == null || categoryStrings.Count == 0;

                if (fetchAll || (wearableCategoryStrings != null && wearableCategoryStrings.Count > 0))
                {
                    var defaultWearables = await defaultInventoryService.GetDefaultWearables(categories: fetchAll ? null : wearableCategoryStrings, forceFetch: forceFetch);
                    if (defaultWearables != null && defaultWearables.Any())
                    {
                        var wearablesProvider = GetDefaultWearablesProvider();

                        var wearableInfos = await UniTask.WhenAll(
                            defaultWearables.Select(async wearable =>
                            {
                                var data = await wearablesProvider.GetDataForAssetId(wearable.AssetId);
                                var info = new WearableAssetInfo
                                {
                                    AssetId = wearable.AssetId,
                                    AssetType = wearable.AssetType,
                                    Name = wearable.Name,
                                    Category = wearable.Category,
                                    Icon = data.Thumbnail
                                };
                                KeepSpriteReference(data.Thumbnail);
                                return info;
                            })
                        );
                        result.AddRange(wearableInfos);
                    }
                }

                if (fetchAll || (flairCategoryStrings != null && flairCategoryStrings.Count > 0))
                {
                    var defaultFlair = await defaultInventoryService.GetDefaultAvatarFlair(limit: null, categories: fetchAll ? null : flairCategoryStrings);
                    if (defaultFlair != null && defaultFlair.Any())
                    {
                        var flairProvider = GetDefaultFlairProvider();

                        var flairInfos = await UniTask.WhenAll(
                            defaultFlair.Select(async flair =>
                            {
                                var data = await flairProvider.GetDataForAssetId(flair.AssetId);
                                var info = new WearableAssetInfo
                                {
                                    AssetId = flair.AssetId,
                                    AssetType = flair.AssetType,
                                    Name = flair.Name,
                                    Category = flair.Category,
                                    Icon = data.Thumbnail
                                };
                                KeepSpriteReference(data.Thumbnail);
                                return info;
                            })
                        );

                        result.AddRange(flairInfos);
                        // Do not return the 'none' assets
                        result.RemoveAll(h => string.Equals(h.Name, "none", StringComparison.OrdinalIgnoreCase));
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to get wearable asset info list by categories: {ex.Message}");
                return new List<WearableAssetInfo>();
            }
        }

        public async UniTask<List<WearableAssetInfo>> GetUserWearableAssetsListByCategoriesAsync(List<WardrobeSubcategory> categories = null, bool forceFetch = false, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!GeniesLoginSdk.IsUserSignedIn())
                {
                    CrashReporter.LogError("You need to be logged in to get a user's assets");
                    return new();
                }

                List<string> categoryStrings = null;

                if (categories != null && categories.Count > 0)
                {
                    categoryStrings = categories
                        .Where(c => c != WardrobeSubcategory.none && c != WardrobeSubcategory.all)
                        .Select(c => c.ToString().ToLower())
                        .ToList();
                }

                var defaultInventoryService = ServiceManager.Get<IDefaultInventoryService>();
                var wearables = await defaultInventoryService.GetUserWearables(categories: categoryStrings, forceFetch: forceFetch);

                if (wearables == null || !wearables.Any())
                {
                    CrashReporter.LogInternal($"No user wearables found in inventory service for categories: {(categoryStrings != null && categoryStrings.Count > 0 ? string.Join(", ", categoryStrings) : "all")}", LogSeverity.Warning);
                    return new List<WearableAssetInfo>();
                }

                var provider = GetUserWearablesProvider();

                var wearableAssetInfoList = await UniTask.WhenAll(
                    wearables.Select(async wearable =>
                    {
                        var data = await provider.GetDataForAssetId(wearable.AssetId);

                        var info = new WearableAssetInfo
                        {
                            AssetId = wearable.AssetId,
                            AssetType = wearable.AssetType,
                            Name = wearable.Name,
                            Category = wearable.Category,
                            Icon = data.Thumbnail
                        };

                        KeepSpriteReference(data.Thumbnail);

                        return info;
                    })
                );

                return wearableAssetInfoList.ToList();
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to get user wearable asset info list by categories: {ex.Message}");
                return new();
            }
        }

        public async UniTask<List<DefaultInventoryAsset>> GetDefaultMakeupByCategoryAsync(List<MakeupCategory> categories, int? limit = null, CancellationToken cancellationToken = default)
        {
            try
            {
                List<string> categoriesLower = null;
                if (categories != null && categories.Count > 0)
                {
                    categoriesLower = categories
                        .Where(c => c != MakeupCategory.None)
                        .Select(MakeupCategoryMapper.ToInternal)
                        .ToList();
                    if (categoriesLower.Count == 0)
                    {
                        categoriesLower = null;
                    }
                }

                IDefaultInventoryService defaultInventoryService = ServiceManager.GetService<IDefaultInventoryService>(null);
                if (defaultInventoryService == null)
                {
                    CrashReporter.LogError("DefaultInventoryService not found");
                    return new List<DefaultInventoryAsset>();
                }

                return await defaultInventoryService.GetDefaultAvatarMakeup(limit: limit, categories: categoriesLower) ?? new List<DefaultInventoryAsset>();
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to get default makeup by category: {ex.Message}");
                return new List<DefaultInventoryAsset>();
            }
        }

        public async UniTask<List<AvatarMakeupInfo>> GetMakeupAssetInfoListByCategoryAsync(List<MakeupCategory> categories, CancellationToken cancellationToken = default)
        {
            try
            {
                IDefaultInventoryService defaultInventoryService = ServiceManager.GetService<IDefaultInventoryService>(null);
                if (defaultInventoryService == null)
                {
                    CrashReporter.LogError("DefaultInventoryService not found");
                    return new List<AvatarMakeupInfo>();
                }

                List<string> categoriesLower = null;
                if (categories != null && categories.Count > 0)
                {
                    categoriesLower = categories
                        .Where(c => c != MakeupCategory.None)
                        .Select(MakeupCategoryMapper.ToInternal)
                        .ToList();
                    if (categoriesLower.Count == 0)
                    {
                        categoriesLower = null;
                    }
                }

                var defaultMakeup = await defaultInventoryService.GetDefaultAvatarMakeup(limit: null, categories: categoriesLower) ?? new List<DefaultInventoryAsset>();
                if (defaultMakeup == null || !defaultMakeup.Any())
                {
                    return new List<AvatarMakeupInfo>();
                }

                var provider = GetDefaultMakeupProvider();

                var makeupAssetInfoList = await UniTask.WhenAll(
                    defaultMakeup.Select(async makeup =>
                    {
                        var data = await provider.GetDataForAssetId(makeup.AssetId);

                        var info = new AvatarMakeupInfo
                        {
                            AssetId = makeup.AssetId,
                            AssetType = makeup.AssetType,
                            Name = makeup.Name,
                            Category = makeup.Category,
                            SubCategories = makeup.SubCategories,
                            Order = makeup.Order,
                            PipelineData = makeup.PipelineData,
                            Icon = data.Thumbnail
                        };

                        KeepSpriteReference(data.Thumbnail);

                        return info;
                    })
                );

                return makeupAssetInfoList.ToList();
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to get makeup asset info list by category: {ex.Message}");
                return new List<AvatarMakeupInfo>();
            }
        }

        public async UniTask<List<AvatarFeaturesInfo>> GetDefaultAvatarFeaturesByCategory(AvatarBaseCategory category, int? limit = null, CancellationToken cancellationToken = default)
        {
            try
            {
                IDefaultInventoryService defaultInventoryService = ServiceManager.GetService<IDefaultInventoryService>(null);
                if (defaultInventoryService == null)
                {
                    CrashReporter.LogError("DefaultInventoryService not found");
                    return new List<AvatarFeaturesInfo>();
                }

                var allData = await defaultInventoryService.GetDefaultAvatarBaseData(limit,
                    new List<string> { "faceblendshape" });

                string categoryFilter = category == AvatarBaseCategory.None
                    ? null
                    : category.ToString();
                if (!string.IsNullOrEmpty(categoryFilter) && allData != null)
                {
                    allData = allData.Where(asset =>
                        asset.SubCategories != null &&
                        asset.SubCategories.Any(sub =>
                            string.Equals(sub, categoryFilter, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                return AvatarFeaturesInfo.FromInternalList(allData ?? new List<DefaultAvatarBaseAsset>());
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to get default avatar features data: {ex.Message}");
                return new List<AvatarFeaturesInfo>();
            }
        }

        public async UniTask<List<AvatarFeaturesInfo>> GetDefaultAvatarFeaturesByCategory(string categoryFilter, int? limit = null, CancellationToken cancellationToken = default)
        {
            try
            {
                IDefaultInventoryService defaultInventoryService = ServiceManager.GetService<IDefaultInventoryService>(null);
                if (defaultInventoryService == null)
                {
                    CrashReporter.LogError("DefaultInventoryService not found");
                    return new List<AvatarFeaturesInfo>();
                }

                var allData = await defaultInventoryService.GetDefaultAvatarBaseData(limit,
                    new List<string> { "faceblendshape" });

                if (!string.IsNullOrEmpty(categoryFilter) && allData != null)
                {
                    allData = allData.Where(asset =>
                        asset.SubCategories != null &&
                        asset.SubCategories.Any(sub =>
                            string.Equals(sub, categoryFilter, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                return AvatarFeaturesInfo.FromInternalList(allData ?? new List<DefaultAvatarBaseAsset>());
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to get default avatar features data: {ex.Message}");
                return new List<AvatarFeaturesInfo>();
            }
        }

        public async UniTask<List<AvatarFeaturesInfo>> GetAvatarFeatureAssetInfoListByCategoryAsync(AvatarBaseCategory category, int? limit = null, CancellationToken cancellationToken = default)
        {
            string categoryFilter = category == AvatarBaseCategory.None ? null : category.ToString();
            return await GetAvatarFeatureAssetInfoListByCategoryAsync(categoryFilter, limit, cancellationToken);
        }

        public async UniTask<List<AvatarFeaturesInfo>> GetAvatarFeatureAssetInfoListByCategoryAsync(string categoryFilter, int? limit = null, CancellationToken cancellationToken = default)
        {
            try
            {
                IDefaultInventoryService defaultInventoryService = ServiceManager.GetService<IDefaultInventoryService>(null);
                if (defaultInventoryService == null)
                {
                    CrashReporter.LogError("DefaultInventoryService not found");
                    return new List<AvatarFeaturesInfo>();
                }

                var allData = await defaultInventoryService.GetDefaultAvatarBaseData(limit, new List<string> { "faceblendshape" });
                if (allData == null || !allData.Any())
                {
                    return new List<AvatarFeaturesInfo>();
                }

                allData = allData.Where(asset => asset.SubCategories?.Contains("body") != true).ToList();

                if (!string.IsNullOrEmpty(categoryFilter))
                {
                    allData = allData.Where(asset =>
                        asset.SubCategories != null &&
                        asset.SubCategories.Any(sub =>
                            string.Equals(sub, categoryFilter, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                var provider = GetDefaultAvatarBaseProvider();

                var featureAssetInfoList = await UniTask.WhenAll(
                    allData.Select(async asset =>
                    {
                        var data = await provider.GetDataForAssetId(asset.AssetId);

                        var info = new AvatarFeaturesInfo
                        {
                            AssetId = asset.AssetId,
                            AssetType = asset.AssetType,
                            Name = asset.Name,
                            Category = asset.Category,
                            SubCategories = asset.SubCategories,
                            Order = asset.Order,
                            PipelineData = asset.PipelineData,
                            Tags = asset.Tags,
                            Icon = data.Thumbnail
                        };

                        KeepSpriteReference(data.Thumbnail);

                        return info;
                    })
                );

                return featureAssetInfoList.ToList();
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to get avatar feature asset info list by category: {ex.Message}");
                return new List<AvatarFeaturesInfo>();
            }
        }

        public async UniTask<List<TattooAssetInfo>> GetDefaultTattooAssetInfoListAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                IDefaultInventoryService defaultInventoryService = ServiceManager.GetService<IDefaultInventoryService>(null);
                if (defaultInventoryService == null)
                {
                    CrashReporter.LogError("DefaultInventoryService not found");
                    return new List<TattooAssetInfo>();
                }

                var defaultTattoos = await defaultInventoryService.GetDefaultImageLibrary(null, new List<string> { "tattoo" });
                if (defaultTattoos == null || !defaultTattoos.Any())
                {
                    CrashReporter.LogError("No default tattoos found in image library");
                    return new List<TattooAssetInfo>();
                }

                var provider = GetDefaultImageLibraryProvider();

                var tattooAssetInfoList = await UniTask.WhenAll(
                    defaultTattoos.Select(async tattoo =>
                    {
                        var data = await provider.GetDataForAssetId(tattoo.AssetId);

                        var info = new TattooAssetInfo
                        {
                            AssetId = tattoo.AssetId,
                            AssetType = tattoo.AssetType,
                            Name = tattoo.Name,
                            Category = tattoo.Category,
                            Icon = data.Thumbnail
                        };

                        KeepSpriteReference(data.Thumbnail);

                        return info;
                    })
                );

                return tattooAssetInfoList.ToList();
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to get tattoo asset info list: {ex.Message}");
                return new List<TattooAssetInfo>();
            }
        }

        #endregion

        #region Save / Load

        public void SaveAvatarDefinitionLocally(GeniesAvatar avatar, string profileId)
        {
            try
            {
                var headshotPath = CapturePNG(avatar.Controller, profileId);
                LocalAvatarProcessor.SaveOrUpdate(profileId, avatar.Controller.GetDefinitionType(), headshotPath);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to save avatar definition locally: {ex.Message}");
            }
        }

        public async UniTask SaveAvatarDefinitionAsync(GeniesAvatar avatar)
        {
            try
            {
                if (GeniesLoginSdk.IsUserSignedInAnonymously())
                {
                    CrashReporter.LogWarning("Cannot write avatar info when user is anonymous");
                    return;
                }

                var avatarService = this.GetService<IAvatarService>();
                if (avatarService == null)
                {
                    CrashReporter.LogError("AvatarService not found. Cannot save avatar definition.");
                    return;
                }

                var avatarDefinition = avatar.Controller.GetDefinitionType();

                var genieRoot = avatar.Controller.Genie.Root;
                var head = genieRoot.transform.Find(_headTransformPath);

                var imageData = AvatarPngCapture.CaptureHeadshotPNGDefaultSettings(genieRoot, head);

                if (avatarService.LoadedAvatar != null)
                {
                    _ = await avatarService.UploadAvatarImageAsync(imageData, avatarService.LoadedAvatar.AvatarId);
                }

                await avatarService.UpdateAvatarAsync(avatarDefinition);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to save avatar definition to cloud: {ex.Message}");
            }
        }

        public async UniTask<GeniesAvatar> LoadFromLocalAvatarDefinitionAsync(string profileId,
            bool showLoadingSilhouette = true,
            int[] lods = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(profileId))
                {
                    CrashReporter.LogError("Profile ID cannot be null or empty");
                    return null;
                }

                var avatarDefinitionString = LocalAvatarProcessor.LoadFromJson(profileId);
                var avatarDefinition = avatarDefinitionString.Definition;

                if (avatarDefinition == null)
                {
                    CrashReporter.LogError($"Failed to parse avatar definition for profile ID: {profileId}");
                    return null;
                }

                return await GeniesAvatarsSdk.LoadAvatarControllerWithClassDefinition(avatarDefinition,
                    showLoadingSilhouette: showLoadingSilhouette,
                    lods: lods );
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to load avatar definition from profile ID '{profileId}': {ex.Message}");
            }

            return null;
        }

        public async UniTask<GeniesAvatar> LoadFromLocalGameObjectAsync(string profileId,
            bool showLoadingSilhouette = true,
            int[] lods = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(profileId))
                {
                    CrashReporter.LogError("Profile ID cannot be null or empty");
                    return null;
                }

                var avatarDefinitionString = LocalAvatarProcessor.LoadFromResources(profileId);
                var avatarDefinition = avatarDefinitionString.Definition;

                if (avatarDefinition == null)
                {
                    CrashReporter.LogError($"Failed to parse avatar definition for profile ID: {profileId}");
                    return null;
                }

                return await GeniesAvatarsSdk.LoadAvatarControllerWithClassDefinition(avatarDefinition,
                    showLoadingSilhouette: showLoadingSilhouette,
                    lods: lods );
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to load avatar definition from profile ID '{profileId}': {ex.Message}");
            }

            return null;
        }

        public async UniTask UploadAvatarImageAsync(byte[] imageData, string avatarId)
        {
            try
            {
                var avatarService = this.GetService<IAvatarService>();
                if (avatarService == null)
                {
                    CrashReporter.LogError("AvatarService not found. Cannot upload avatar image.");
                    return;
                }

                await avatarService.UploadAvatarImageAsync(imageData, avatarId);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to upload avatar image: {ex.Message}");
            }
        }

        #endregion

        #region Private Helpers

        private static string GetPresetName(GenderType genderType, BodySize bodySize)
        {
            return (genderType, bodySize) switch
            {
                (GenderType.Male, BodySize.Skinny) => "Resources/Body/gSkelModifierPresets/maleSkinny_gSkelModifierPreset",
                (GenderType.Male, BodySize.Medium) => "Resources/Body/gSkelModifierPresets/maleMedium_gSkelModifierPreset",
                (GenderType.Male, BodySize.Heavy) => "Resources/Body/gSkelModifierPresets/maleHeavy_gSkelModifierPreset",
                (GenderType.Female, BodySize.Skinny) => "Resources/Body/gSkelModifierPresets/femaleSkinny_gSkelModifierPreset",
                (GenderType.Female, BodySize.Medium) => "Resources/Body/gSkelModifierPresets/femaleMedium_gSkelModifierPreset",
                (GenderType.Female, BodySize.Heavy) => "Resources/Body/gSkelModifierPresets/femaleHeavy_gSkelModifierPreset",
                (GenderType.Androgynous, BodySize.Skinny) => "Resources/Body/gSkelModifierPresets/androgynousSkinny_gSkelModifierPreset",
                (GenderType.Androgynous, BodySize.Medium) => "Resources/Body/gSkelModifierPresets/androgynousMedium_gSkelModifierPreset",
                (GenderType.Androgynous, BodySize.Heavy) => "Resources/Body/gSkelModifierPresets/androgynousHeavy_gSkelModifierPreset",
                _ => throw new ArgumentException($"Invalid combination: {genderType}, {bodySize}")
            };
        }

        private static string GetColorTypeCategory(ColorType colorType)
        {
            return colorType switch
            {
                ColorType.Eyes => "eyes",
                ColorType.Hair => "hair",
                ColorType.FacialHair => "facialhair",
                ColorType.Skin => "skin",
                ColorType.Eyebrow => "flaireyebrow",
                ColorType.Eyelash => "flaireyelash",
                ColorType.MakeupStickers => "makeup",
                ColorType.MakeupLipstick => "makeup",
                ColorType.MakeupFreckles => "makeup",
                ColorType.MakeupFaceGems => "makeup",
                ColorType.MakeupEyeshadow => "makeup",
                ColorType.MakeupBlush => "makeup",
                _ => throw new ArgumentOutOfRangeException(nameof(colorType), colorType, "Invalid color type")
            };
        }

        private static MakeupCategory GetMakeupCategoryFromColorType(ColorType colorType)
        {
            return colorType switch
            {
                ColorType.MakeupStickers => MakeupCategory.Stickers,
                ColorType.MakeupLipstick => MakeupCategory.Lipstick,
                ColorType.MakeupFreckles => MakeupCategory.Freckles,
                ColorType.MakeupFaceGems => MakeupCategory.FaceGems,
                ColorType.MakeupEyeshadow => MakeupCategory.Eyeshadow,
                ColorType.MakeupBlush => MakeupCategory.Blush,
                _ => throw new ArgumentOutOfRangeException(nameof(colorType), colorType, "Not a makeup ColorType")
            };
        }

        private static IColor ToIColorValueInternal(ColorType colorType, List<Color> colors, string assetId = null)
        {
            bool isEmpty = colors == null || colors.Count == 0;
            Color clear = Color.clear;

            switch (colorType)
            {
                case ColorType.Skin:
                    return new SkinColor(isEmpty ? clear : colors[0]);

                case ColorType.Hair:
                case ColorType.FacialHair:
                case ColorType.MakeupStickers:
                case ColorType.MakeupLipstick:
                case ColorType.MakeupFreckles:
                case ColorType.MakeupFaceGems:
                case ColorType.MakeupEyeshadow:
                case ColorType.MakeupBlush:
                    {
                        Color c0 = isEmpty ? clear : colors[0];
                        Color c1 = (colors != null && colors.Count > 1) ? colors[1] : c0;
                        Color c2 = (colors != null && colors.Count > 2) ? colors[2] : c0;
                        Color c3 = (colors != null && colors.Count > 3) ? colors[3] : c0;
                        if (colorType == ColorType.Hair)
                        {
                            return new HairColor(c0, c1, c2, c3);
                        }

                        if (colorType == ColorType.FacialHair)
                        {
                            return new FacialHairColor(c0, c1, c2, c3);
                        }

                        return new MakeupColor(c0, c1, c2, c3);
                    }
                case ColorType.Eyebrow:
                case ColorType.Eyelash:
                    {
                        Color c0 = isEmpty ? clear : colors[0];
                        Color c1 = (colors != null && colors.Count > 1) ? colors[1] : c0;
                        if (colorType == ColorType.Eyebrow)
                        {
                            return new EyeBrowsColor(c0, c1);
                        }

                        return new EyeLashColor(c0, c1);
                    }
                case ColorType.Eyes:
                    {
                        Color c0 = isEmpty ? clear : colors[0];
                        Color c1 = (colors != null && colors.Count > 1) ? colors[1] : c0;
                        return new EyeColor(assetId ?? string.Empty, c0, c1);
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(colorType), colorType, "Unsupported ColorType.");
            }
        }

        /// <summary>
        /// Creates a user (custom) color for the specified color type and stores it via DefaultInventoryService.
        /// Only Hair, Facial hair, Skin, Eyebrow, and Eyelash support user colors.
        /// </summary>
        /// <param name="colorType">The type of user color to create (Hair, Eyebrow, or Eyelash).</param>
        /// <param name="colors">The color values (e.g. one color for skin, multiple for hair).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>UniTask that completes with the new custom color instance ID, or null if creation failed.</returns>
        public async UniTask<ICustomColor> CreateUserColorAsync(UserColorType colorType, List<Color> colors, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                IDefaultInventoryService defaultInventoryService = ServiceManager.GetService<IDefaultInventoryService>(null);
                if (defaultInventoryService == null)
                {
                    CrashReporter.LogError("DefaultInventoryService not found");
                    return null;
                }

                if (colors == null || colors.Count == 0)
                {
                    CrashReporter.LogError("At least one color is required to create a user color");
                    return null;
                }

                var listColors = await GetUserColorsAsync(colorType, cancellationToken);
                if (listColors != null)
                {
                    foreach (var color in listColors)
                    {
                        // Check if we already have a close enough match for the create color request
                        // This will help in preventing multiple copies of same colors
                        if (DoColorsMatch(color.Hexes, colors))
                        {
                            return color;
                        }
                    }
                }

                CreateCustomColorRequest.CategoryEnum category = GetUserColorTypeToCreateCustomColorCategory(colorType);
                var customColor = await defaultInventoryService.CreateCustomColor(colors, category);

                var colorList = new List<Color>();
                if (customColor.ColorsHex != null)
                {
                    foreach (var hex in customColor.ColorsHex)
                    {
                        if (ColorUtility.TryParseHtmlString(hex, out Color color))
                        {
                            colorList.Add(color);
                        }
                    }
                }
                return ToIColorValueInternalUser(colorType, colorList, customColor.AssetId, customColor.InstanceId);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to create user {colorType} color: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Updates an existing user (custom) color by instance ID via DefaultInventoryService.
        /// </summary>
        /// <param name="instanceId">The instance ID of the custom color to update.</param>
        /// <param name="colors">The new color values.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>UniTask that completes when the update is finished.</returns>
        public async UniTask UpdateUserColorAsync(string instanceId, List<Color> colors, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrEmpty(instanceId))
                {
                    CrashReporter.LogError("Instance ID is required to update a user color");
                    return;
                }

                if (colors == null || colors.Count == 0)
                {
                    CrashReporter.LogError("At least one color is required to update a user color");
                    return;
                }

                IDefaultInventoryService defaultInventoryService = ServiceManager.GetService<IDefaultInventoryService>(null);
                if (defaultInventoryService == null)
                {
                    CrashReporter.LogError("DefaultInventoryService not found");
                    return;
                }
                await defaultInventoryService.UpdateCustomColor(instanceId, colors);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to update user color: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing user (custom) color using an <see cref="ICustomColor"/> value.
        /// Validates the value then delegates to <see cref="UpdateUserColorAsync(string, List{Color}, CancellationToken)"/>.
        /// </summary>
        /// <param name="colorValue">The custom color to update (must be user-created with a non-empty <see cref="ICustomColor.InstanceId"/>).</param>
        /// <param name="colors">The new color values.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>UniTask that completes when the update is finished.</returns>
        public async UniTask UpdateUserColorAsync(
            ICustomColor colorValue,
            List<Color> colors,
            CancellationToken cancellationToken = default)
        {
            if (colorValue == null)
            {
                CrashReporter.LogError("A custom color value is required to update a user color");
                return;
            }

            if (!colorValue.IsCustom)
            {
                CrashReporter.LogError("Cannot update a non-custom color; use a user-created color from GetUserColorsAsync or CreateUserColorAsync");
                return;
            }

            if (string.IsNullOrEmpty(colorValue.InstanceId))
            {
                CrashReporter.LogError("Instance ID is required to update a user color");
                return;
            }

            await UpdateUserColorAsync(colorValue.InstanceId, colors, cancellationToken);
        }

        /// <summary>
        /// Deletes a user (custom) color by instance ID via DefaultInventoryService.
        /// </summary>
        /// <param name="instanceId">The instance ID of the custom color to delete.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>UniTask that completes when the delete is finished.</returns>
        public async UniTask DeleteUserColorAsync(string instanceId, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrEmpty(instanceId))
                {
                    CrashReporter.LogError("Instance ID is required to delete a user color");
                    return;
                }

                IDefaultInventoryService defaultInventoryService = ServiceManager.GetService<IDefaultInventoryService>(null);
                if (defaultInventoryService == null)
                {
                    CrashReporter.LogError("DefaultInventoryService not found");
                    return;
                }

                await defaultInventoryService.DeleteCustomColor(instanceId, new List<Color>());
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to delete user color: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a user (custom) color using an <see cref="ICustomColor"/> value.
        /// Validates the value then delegates to <see cref="DeleteUserColorAsync(string, CancellationToken)"/>.
        /// </summary>
        /// <param name="colorValue">The custom color to delete (must be user-created with a non-empty <see cref="ICustomColor.InstanceId"/>).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>UniTask that completes when the delete is finished.</returns>
        public async UniTask DeleteUserColorAsync(ICustomColor colorValue, CancellationToken cancellationToken = default)
        {
            if (colorValue == null)
            {
                CrashReporter.LogError("A custom color value is required to delete a user color");
                return;
            }

            if (!colorValue.IsCustom)
            {
                CrashReporter.LogError("Cannot delete a non-custom color; use a user-created color from GetUserColorsAsync or CreateUserColorAsync");
                return;
            }

            if (string.IsNullOrEmpty(colorValue.InstanceId))
            {
                CrashReporter.LogError("Instance ID is required to delete a user color");
                return;
            }

            await DeleteUserColorAsync(colorValue.InstanceId, cancellationToken);
        }

        /// <summary>
        /// Gets user (custom) colors filtered by category via DefaultInventoryService.
        /// </summary>
        /// <param name="colorType">Optional user color type to filter by (Hair, Eyebrow, Eyelash, etc.). Null returns all categories.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>UniTask that completes with the list of custom color responses from the inventory service.</returns>
        public async UniTask<List<CustomColorResponse>> GetUserColorsByCategoryAsync(UserColorType? colorType = null, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                IDefaultInventoryService defaultInventoryService = ServiceManager.GetService<IDefaultInventoryService>(null);
                if (defaultInventoryService == null)
                {
                    CrashReporter.LogError("DefaultInventoryService not found");
                    return new List<CustomColorResponse>();
                }

                string category = colorType.HasValue ? GetUserColorTypeCategory(colorType.Value) : null;
                var customColors = await defaultInventoryService.GetCustomColors(category);
                return customColors ?? new List<CustomColorResponse>();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to get user colors by category: {ex.Message}");
                return new List<CustomColorResponse>();
            }
        }

        /// <summary>
        /// Maps UserColorType to the inventory API's CreateCustomColor category (Hair, Facial hair, Skin, Eye brows or eye lashes)
        /// </summary>
        private static CreateCustomColorRequest.CategoryEnum GetUserColorTypeToCreateCustomColorCategory(UserColorType colorType)
        {
            return colorType switch
            {
                UserColorType.Hair => CreateCustomColorRequest.CategoryEnum.Hair,
                UserColorType.Eyebrow => CreateCustomColorRequest.CategoryEnum.Flair,
                UserColorType.Eyelash => CreateCustomColorRequest.CategoryEnum.Eyelashes,
                UserColorType.FacialHair => CreateCustomColorRequest.CategoryEnum.Facialhair,
                UserColorType.Skin => CreateCustomColorRequest.CategoryEnum.Skin,
                _ => throw new ArgumentOutOfRangeException(nameof(colorType), colorType, "Invalid user color type for create")
            };
        }

        private static string GetUserColorTypeCategory(UserColorType colorType)
        {
            return colorType switch
            {
                UserColorType.Hair => "hair",
                UserColorType.Eyebrow => "flair",
                UserColorType.Eyelash => "eyelashes",
                UserColorType.FacialHair => "facialhair",
                UserColorType.Skin => "skin",
                _ => throw new ArgumentOutOfRangeException(nameof(colorType), colorType, "Invalid user color type")
            };
        }

        /// <summary>
        /// Converts UserColorType and color data to Core <see cref="ICustomColor"/> for GetUserColorsAsync.
        /// </summary>
        /// <param name="instanceId">Optional entity ID (e.g. custom color instance ID). When not null, set on the value and exposed via <see cref="ICustomColor.InstanceId"/>.</param>
        private static ICustomColor ToIColorValueInternalUser(UserColorType colorType, List<Color> colors, string assetId = null, string instanceId = null)
        {
            bool isEmpty = colors == null || colors.Count == 0;
            Color clear = Color.clear;

            switch (colorType)
            {
                case UserColorType.Hair:
                case UserColorType.FacialHair:
                    {
                        Color c0 = isEmpty ? clear : colors[0];
                        Color c1 = (colors != null && colors.Count > 1) ? colors[1] : c0;
                        Color c2 = (colors != null && colors.Count > 2) ? colors[2] : c0;
                        Color c3 = (colors != null && colors.Count > 3) ? colors[3] : c0;
                        if (colorType == UserColorType.Hair)
                        {
                            return new HairColor(c0, c1, c2, c3, instanceId, true);
                        }
                        return new FacialHairColor(c0, c1, c2, c3, instanceId, true);
                    }
                case UserColorType.Eyebrow:
                case UserColorType.Eyelash:
                    {
                        Color c0 = isEmpty ? clear : colors[0];
                        Color c1 = (colors != null && colors.Count > 1) ? colors[1] : c0;
                        if (colorType == UserColorType.Eyebrow)
                        {
                            return new EyeBrowsColor(c0, c1, instanceId, true);
                        }
                        return new EyeLashColor(c0, c1, instanceId, true);
                    }
                case UserColorType.Skin:
                    {
                        Color c0 = isEmpty ? clear : colors[0];
                        return new SkinColor(c0, instanceId, true);
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(colorType), colorType, "Unsupported UserColorType.");
            }
        }

        /// <summary>
        /// Returns true if the two color sequences are considered the same within a small tolerance.
        /// Used to find an existing preset that matches applied colors (e.g. when creating or matching custom colors).
        /// </summary>
        /// <param name="color1">First sequence of colors (e.g. IColor.Hexes).</param>
        /// <param name="color2">Second sequence of colors (e.g. list from API or avatar).</param>
        /// <returns>True if both are non-null, have the same length, and every component of every color is within tolerance; otherwise false.</returns>
        private static bool DoColorsMatch(Color[] color1, List<Color> color2)
        {
            const float tolerance = 0.01f;

            if (color1 == null || color2 == null || color1.Length != color2.Count)
            {
                return false;
            }
            for (var i = 0; i < color1.Length; i++)
            {
                Color a = color1[i];
                Color b = color2[i];
                if (Mathf.Abs(a.r - b.r) >= tolerance || Mathf.Abs(a.g - b.g) >= tolerance ||
                    Mathf.Abs(a.b - b.b) >= tolerance || Mathf.Abs(a.a - b.a) >= tolerance)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the current color from the avatar for the specified color kind (Hair, FacialHair, EyeBrows, EyeLash, Skin, Eyes, or Makeup).
        /// Returns an IColor instance of the corresponding type.
        /// </summary>
        /// <param name="avatar">The avatar to read the color from.</param>
        /// <param name="colorKind">Which IColor type to return.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>UniTask that completes with the corresponding IColor value, or null if avatar is null.</returns>
        public async UniTask<IColor> GetColorAsync(GeniesAvatar avatar, AvatarColorKind colorKind, CancellationToken cancellationToken = default)
        {
            if (avatar == null)
            {
                CrashReporter.LogError("Avatar cannot be null");
                return null;
            }

            cancellationToken.ThrowIfCancellationRequested();
            await UniTask.Yield();

            switch (colorKind)
            {
                case AvatarColorKind.Hair:
                    {
                        var bc = avatar.Controller.GetColor(GenieColor.HairBase);
                        var r = avatar.Controller.GetColor(GenieColor.HairR);
                        var g = avatar.Controller.GetColor(GenieColor.HairG);
                        var b = avatar.Controller.GetColor(GenieColor.HairB);

                        if (bc == null ||  r == null || g == null || b == null)
                        {
                            return null;
                        }
                        return new HairColor(bc.Value, r.Value, g.Value, b.Value);
                    }
                case AvatarColorKind.FacialHair:
                    {
                        var bc = avatar.Controller.GetColor(GenieColor.FacialhairBase);
                        var r = avatar.Controller.GetColor(GenieColor.FacialhairR);
                        var g = avatar.Controller.GetColor(GenieColor.FacialhairG);
                        var b = avatar.Controller.GetColor(GenieColor.FacialhairB);

                        if (bc == null ||  r == null || g == null || b == null)
                        {
                            return null;
                        }
                        return new FacialHairColor(bc.Value, r.Value, g.Value, b.Value);
                    }
                case AvatarColorKind.EyeBrows:
                    {
                        var bc = avatar.Controller.GetColor(GenieColor.EyebrowsBase);
                        var r = avatar.Controller.GetColor(GenieColor.EyebrowsR);

                        if (bc == null ||  r == null)
                        {
                            return null;
                        }
                        return new EyeBrowsColor(bc.Value, r.Value);
                    }
                case AvatarColorKind.EyeLash:
                    {
                        var bc = avatar.Controller.GetColor(GenieColor.EyelashesBase);
                        var r = avatar.Controller.GetColor(GenieColor.EyelashesR);

                        if (bc == null ||  r == null)
                        {
                            return null;
                        }
                        return new EyeLashColor(bc.Value, r.Value);
                    }
                case AvatarColorKind.Skin:
                    {
                        var previousSkin = avatar.Controller.GetColor(GenieColor.Skin);
                        if (previousSkin == null)
                        {
                            return null;
                        }
                        return new SkinColor(previousSkin.Value);
                    }
                case AvatarColorKind.Eyes:
                    {
                        return await GetEyeColorAsync(avatar);
                    }
                case AvatarColorKind.MakeupStickers:
                case AvatarColorKind.MakeupLipstick:
                case AvatarColorKind.MakeupFreckles:
                case AvatarColorKind.MakeupFaceGems:
                case AvatarColorKind.MakeupEyeshadow:
                case AvatarColorKind.MakeupBlush:
                    {
                        var clearColors = new Color[] { Color.clear, Color.clear, Color.clear, Color.clear };
                        MakeupCategory category = GetMakeupCategoryFromAvatarColorKind(colorKind);

                        var makeupCommand = new EquipMakeupColorCommand((int)category, clearColors, avatar.Controller);
                        var prev = makeupCommand.PreviousColors;
                        if (prev == null)
                        {
                            return null;
                        }
                        Color c0 = prev != null && prev.Length > 0 ? (prev[0].Value ?? Color.clear) : Color.clear;
                        Color c1 = prev != null && prev.Length > 1 ? (prev[1].Value ?? Color.clear) : Color.clear;
                        Color c2 = prev != null && prev.Length > 2 ? (prev[2].Value ?? Color.clear) : Color.clear;
                        Color c3 = prev != null && prev.Length > 3 ? (prev[3].Value ?? Color.clear) : Color.clear;
                        return new MakeupColor(category, c0, c1, c2, c3);
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(colorKind), colorKind, "Unsupported avatar color kind");
            }
        }

        private static MakeupCategory GetMakeupCategoryFromAvatarColorKind(AvatarColorKind colorKind)
        {
            return colorKind switch
            {
                AvatarColorKind.MakeupStickers => MakeupCategory.Stickers,
                AvatarColorKind.MakeupLipstick => MakeupCategory.Lipstick,
                AvatarColorKind.MakeupFreckles => MakeupCategory.Freckles,
                AvatarColorKind.MakeupFaceGems => MakeupCategory.FaceGems,
                AvatarColorKind.MakeupEyeshadow => MakeupCategory.Eyeshadow,
                AvatarColorKind.MakeupBlush => MakeupCategory.Blush,
                _ => throw new ArgumentOutOfRangeException(nameof(colorKind), colorKind, "Not a makeup AvatarColorKind")
            };
        }

        private async UniTask<IColor> GetEyeColorAsync(GeniesAvatar avatar)
        {
            var equippedIds = avatar.Controller?.GetEquippedAssetIds();
            if (equippedIds == null || equippedIds.Count == 0)
            {
                return null;
            }

            var defaultInventoryService = ServiceManager.GetService<IDefaultInventoryService>(null);
            if (defaultInventoryService == null)
            {
                return null;
            }

            var eyeAssets = await defaultInventoryService.GetDefaultAvatarEyes(limit: null, categories: null);
            if (eyeAssets == null || eyeAssets.Count == 0)
            {
                return null;
            }

            var eyeAssetIds = eyeAssets
                .Select(a => a.AssetId)
                .Where(id => !string.IsNullOrEmpty(id))
                .ToHashSet();

            var eyeAssetIdToColors = eyeAssets
                .Where(a => !string.IsNullOrEmpty(a.AssetId))
                .ToDictionary(a => a.AssetId, a => a.Colors);

            string eyeAssetId = null;
            Dictionary<string, List<Color>> universalIdToColors = null;
            var converter = ServiceManager.Get<IAssetIdConverter>();
            if (converter != null)
            {
                var convertedIds = await converter.ConvertToUniversalIdsAsync(eyeAssetIds.ToList());
                if (convertedIds != null && convertedIds.Count > 0)
                {
                    universalIdToColors = new Dictionary<string, List<Color>>();
                    foreach (var kvp in convertedIds)
                    {
                        if (eyeAssetIdToColors.TryGetValue(kvp.Key, out var cols))
                        {
                            universalIdToColors[kvp.Value] = cols;
                        }
                    }
                    var universalEyeSet = convertedIds.Values.ToHashSet();
                    eyeAssetId = equippedIds.FirstOrDefault(id => universalEyeSet.Contains(id));
                }
            }

            if (string.IsNullOrEmpty(eyeAssetId))
            {
                eyeAssetId = equippedIds.FirstOrDefault(id => eyeAssetIds.Contains(id));
            }

            var c1 = Color.clear;
            var c2 = Color.clear;
            List<Color> eyeColors = null;
            if (!string.IsNullOrEmpty(eyeAssetId))
            {
                eyeAssetIdToColors.TryGetValue(eyeAssetId, out eyeColors);
            }

            if (eyeColors == null && !string.IsNullOrEmpty(eyeAssetId) && universalIdToColors != null)
            {
                universalIdToColors.TryGetValue(eyeAssetId, out eyeColors);
            }
            if (eyeColors != null && eyeColors.Count > 0)
            {
                c1 = eyeColors[0];
                c2 = eyeColors.Count > 1 ? eyeColors[1] : Color.clear;
            }

            if ((eyeColors == null || eyeColors.Count == 0) && string.IsNullOrEmpty(eyeAssetId))
            {
                return null;
            }

            return new EyeColor(eyeAssetId, c1, c2);
        }

        /// <summary>
        /// Creates an avatar headshot screenshot (PNG) using the avatar's head transform.
        /// Frames the head via the skeleton path "Root/Hips/Spine/Spine1/Spine2/Neck/Head", renders to a temporary camera, and returns PNG bytes.
        /// </summary>
        /// <param name="avatar">The avatar to capture. Must have a valid Controller and Genie.Root.</param>
        /// <param name="saveFilePath">Optional. Output file path (relative to <paramref name="saveLocation"/> when not rooted). If null or empty, save to file is skipped.</param>
        /// <param name="config">Screenshot options (width, height, MSAA, FOV, etc.). If null, uses <see cref="ScreenshotConfig.Default"/> (512x512, transparent, MSAA 8).</param>
        /// <param name="saveLocation">Root for <paramref name="saveFilePath"/>. When null, uses PersistentDataPath.</param>
        /// <returns>PNG bytes of the headshot, or null if avatar is null, Controller is null, Genie.Root is null, or the head transform is not found.</returns>
        public byte[] CreateAvatarScreenshot(GeniesAvatar avatar, string saveFilePath = null, ScreenshotConfig? config = null, ScreenshotSaveLocation? saveLocation = null)
        {
            if (avatar?.Controller == null)
            {
                return null;
            }

            var genieRoot = avatar.Controller.Genie.Root;
            if (genieRoot == null)
            {
                return null;
            }

            var head = genieRoot.transform.Find(_headTransformPath);
            if (head == null)
            {
                return null;
            }

            var location = saveLocation ?? ScreenshotSaveLocation.PersistentDataPath;
            string pathToPass = saveFilePath;
            if (!string.IsNullOrEmpty(saveFilePath))
            {
                if (!System.IO.Path.IsPathRooted(saveFilePath))
                {
                    string root = location == ScreenshotSaveLocation.PersistentDataPath
                        ? Application.persistentDataPath
                        : Application.dataPath;
                    pathToPass = System.IO.Path.Combine(root, saveFilePath);
#if !UNITY_EDITOR
                    if (location == ScreenshotSaveLocation.ProjectRoot)
                    {
                        Debug.LogWarning(
                            "CreateAvatarScreenshot: ScreenshotSaveLocation.ProjectRoot may not work in built applications. Prefer PersistentDataPath.");
                    }
#endif
                }
            }

            var cfg = config ?? ScreenshotConfig.Default;
            var result = AvatarPngCapture.CaptureHeadshotPNG(
                genieRoot,
                head,
                cfg.Width,
                cfg.Height,
                pathToPass,
                cfg.TransparentBackground,
                cfg.Msaa,
                cfg.FieldOfView,
                cfg.HeadRadiusMeters,
                cfg.ForwardDistance,
                cfg.CameraUpOffset);

#if UNITY_EDITOR
            if (location == ScreenshotSaveLocation.ProjectRoot)
            {
                AssetDatabase.Refresh();
            }
#endif
            return result;
        }

        private string CapturePNG(NativeUnifiedGenieController currentCustomizedAvatar, string profileId = null)
        {
            var filename = string.IsNullOrEmpty(profileId) ? "avatar-headshot.png" : $"{profileId}-headshot.png";

            if (!System.IO.Directory.Exists(LocalAvatarProcessor.HeadshotPath))
            {
                System.IO.Directory.CreateDirectory(LocalAvatarProcessor.HeadshotPath);
            }

            var headShotPath = System.IO.Path.Combine(LocalAvatarProcessor.HeadshotPath, filename);
            GameObject genieRoot = currentCustomizedAvatar.Genie.Root;
            var head = genieRoot.transform.Find(_headTransformPath);

            AvatarPngCapture.CaptureHeadshotPNG(genieRoot, head,
                width: 512,
                height: 512,
                saveFilePath: headShotPath,
                transparentBackground: true,
                msaa: 8,
                fieldOfView: 25f,
                headRadiusMeters: 0.23f,
                forwardDistance: 0.8f,
                cameraUpOffset: new Vector3(0f, 0.05f, 0f));

            return headShotPath;
        }

        #endregion
    }
}
