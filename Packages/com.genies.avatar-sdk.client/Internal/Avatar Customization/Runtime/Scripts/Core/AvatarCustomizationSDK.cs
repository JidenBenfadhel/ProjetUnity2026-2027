using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Genies.Avatars.Sdk;
using Genies.CrashReporting;
using Genies.Inventory;
using Genies.Looks.Customization.Commands;
using GnWrappers;
using Genies.ServiceManagement;
using CancellationToken = System.Threading.CancellationToken;

namespace Genies.Avatars.Customization
{
     /// <summary>
    /// Configuration for avatar headshot screenshot capture.
    /// </summary>
    internal struct ScreenshotConfig
    {
        /// <summary>Output width in pixels.</summary>
        public int Width;
        /// <summary>Output height in pixels.</summary>
        public int Height;
        /// <summary>If true, background alpha is 0.</summary>
        public bool TransparentBackground;
        /// <summary>MSAA level for the RenderTexture (1, 2, 4, 8).</summary>
        public int Msaa;
        /// <summary>Camera field of view in degrees.</summary>
        public float FieldOfView;
        /// <summary>Approximate head radius used for framing.</summary>
        public float HeadRadiusMeters;
        /// <summary>Camera distance from head center before FOV fit.</summary>
        public float ForwardDistance;
        /// <summary>Vertical offset for camera position.</summary>
        public Vector3 CameraUpOffset;

        /// <summary>
        /// Default configuration (512x512, transparent, MSAA 8, etc.).
        /// </summary>
        public static ScreenshotConfig Default => new ScreenshotConfig
        {
            Width = 512,
            Height = 512,
            TransparentBackground = true,
            Msaa = 8,
            FieldOfView = 25f,
            HeadRadiusMeters = 0.23f,
            ForwardDistance = 0.8f,
            CameraUpOffset = new Vector3(0f, 0.05f, 0f)
        };
    }

    /// <summary>
    /// Root location for resolving avatar screenshot save paths. Local mapping for SDK <see cref="ScreenshotSaveLocation"/>.
    /// </summary>
    internal enum ScreenshotSaveLocation
    {
        /// <summary>Resolve path under Application.persistentDataPath. Recommended for built applications.</summary>
        PersistentDataPath = 0,

        /// <summary>Resolve path under Application.dataPath. May not work in built applications.</summary>
        ProjectRoot = 1
    }

    /// <summary>
    /// Static facade for avatar customization APIs.
    /// Provides methods for getting and setting avatar data (wearables, makeup, tattoos, colors,
    /// body presets, feature stats, etc.) without requiring the full avatar editor UI.
    /// </summary>
    internal static class AvatarCustomizationSDK
    {
        public static bool IsInitialized { get; private set; }

        private static readonly Dictionary<HairType, WardrobeSubcategory> HairTypeToWardrobeSubcategory = new()
        {
            { HairType.Hair, WardrobeSubcategory.hair },
            { HairType.FacialHair, WardrobeSubcategory.facialHair },
            { HairType.Eyebrows, WardrobeSubcategory.eyebrows },
            { HairType.Eyelashes, WardrobeSubcategory.eyelashes }
        };

        private static readonly Dictionary<WearablesCategory, WardrobeSubcategory> WearableCategoryToWardrobeSubcategory = new()
        {
            { WearablesCategory.Hoodie, WardrobeSubcategory.hoodie },
            { WearablesCategory.Jacket, WardrobeSubcategory.jacket },
            { WearablesCategory.Shirt, WardrobeSubcategory.shirt },
            { WearablesCategory.Dress, WardrobeSubcategory.dress },
            { WearablesCategory.Pants, WardrobeSubcategory.pants },
            { WearablesCategory.Shorts, WardrobeSubcategory.shorts },
            { WearablesCategory.Skirt, WardrobeSubcategory.skirt },
            { WearablesCategory.Shoes, WardrobeSubcategory.shoes },
            { WearablesCategory.Earrings, WardrobeSubcategory.earrings },
            { WearablesCategory.Glasses, WardrobeSubcategory.glasses },
            { WearablesCategory.Hat, WardrobeSubcategory.hat },
            { WearablesCategory.Mask, WardrobeSubcategory.mask },
        };

        private static WearablesCategory ToWearablesCategory(UserWearablesCategory userCategory)
        {
            return userCategory switch
            {
                UserWearablesCategory.Hoodie => WearablesCategory.Hoodie,
                UserWearablesCategory.Shirt => WearablesCategory.Shirt,
                UserWearablesCategory.Jacket => WearablesCategory.Jacket,
                UserWearablesCategory.Dress => WearablesCategory.Dress,
                UserWearablesCategory.Pants => WearablesCategory.Pants,
                UserWearablesCategory.Shorts => WearablesCategory.Shorts,
                UserWearablesCategory.Skirt => WearablesCategory.Skirt,
                UserWearablesCategory.Shoes => WearablesCategory.Shoes,
                _ => throw new ArgumentOutOfRangeException(nameof(userCategory), userCategory, "Invalid UserWearablesCategory")
            };
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

        #region Initialization

        /// <summary>
        /// Ensures SDK dependencies for customization are initialized.
        /// </summary>
        public static async UniTask<bool> InitializeAsync()
        {
            if (IsInitialized)
            {
                return true;
            }

            if (await GeniesAvatarsSdk.InitializeAsync())
            {
                IsInitialized = true;
                return true;
            }

            return false;
        }
        #endregion

        #region Events

        /// <summary>
        /// Event raised when an asset is equipped.
        /// Payload: wearableId
        /// </summary>
        public static event Action<string> EquippedAsset = delegate { };

        /// <summary>
        /// Event raised when an asset is unequipped.
        /// Payload: wearableId
        /// </summary>
        public static event Action<string> UnequippedAsset = delegate { };

        /// <summary>
        /// Event raised when a skin color is set.
        /// </summary>
        public static event Action SkinColorSet = delegate { };

        /// <summary>
        /// Event raised when hair colors are set.
        /// </summary>
        public static event Action HairColorSet = delegate { };

        /// <summary>
        /// Event raised when a hair style is equipped.
        /// Payload: hairAssetId
        /// </summary>
        public static event Action<string> HairEquipped = delegate { };

        /// <summary>
        /// Event raised when a hair style is unequipped.
        /// Payload: hairAssetId
        /// </summary>
        public static event Action<string> HairUnequipped = delegate { };

        /// <summary>
        /// Event raised when a tattoo is equipped.
        /// Payload: tattooId
        /// </summary>
        public static event Action<string> TattooEquipped = delegate { };

        /// <summary>
        /// Event raised when a tattoo is unequipped.
        /// Payload: tattooId
        /// </summary>
        public static event Action<string> TattooUnequipped = delegate { };

        /// <summary>
        /// Event raised when a native avatar body preset is applied.
        /// </summary>
        public static event Action BodyPresetSet = delegate { };

        /// <summary>
        /// Event raised when avatar body type is set (gender + body size).
        /// </summary>
        public static event Action BodyTypeSet = delegate { };

        /// <summary>
        /// Event raised when an avatar definition is saved (local or cloud depending on mode).
        /// </summary>
        public static event Action AvatarDefinitionSaved = delegate { };

        /// <summary>
        /// Event raised when an avatar definition is saved locally.
        /// </summary>
        public static event Action AvatarDefinitionSavedLocally = delegate { };

        /// <summary>
        /// Event raised when an avatar definition is saved to cloud.
        /// </summary>
        public static event Action AvatarDefinitionSavedToCloud = delegate { };

        /// <summary>
        /// Event raised when an avatar is loaded for editing.
        /// </summary>
        public static event Action AvatarLoadedForEditing = delegate { };

        #endregion

        #region Service Access

        private static IAvatarCustomizationService GetService()
        {
            var service = ServiceManager.Get<IAvatarCustomizationService>();
            if (service == null)
            {
                CrashReporter.LogError("AvatarCustomizationService not found. Make sure the SDK is initialized.");
            }
            return service;
        }

        #endregion

        #region Asset Retrieval

        private static async UniTask<List<WearableAssetInfo>> GetWearableAssetsByCategoryAsync(RequestType assetType, List<WardrobeSubcategory> categories = null, bool forceFetch = false, CancellationToken cancellationToken = default)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return new List<WearableAssetInfo>();
                }

                var result = new List<WearableAssetInfo>();

                if (assetType == RequestType.All || assetType == RequestType.Default)
                {
                    var defaultAssets = await service.GetDefaultWearableAssetsListByCategoriesAsync(categories ?? new List<WardrobeSubcategory>(), forceFetch, cancellationToken);
                    if (defaultAssets != null)
                    {
                        result.AddRange(defaultAssets);
                    }
                }

                if (assetType == RequestType.All || assetType == RequestType.User)
                {
                    var userAssets = await service.GetUserWearableAssetsListByCategoriesAsync(categories ?? new List<WardrobeSubcategory>(), forceFetch, cancellationToken);
                    if (userAssets != null)
                    {
                        result.AddRange(userAssets);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to get wearable assets by category: {ex.Message}");
                return new List<WearableAssetInfo>();
            }
        }

        public static async UniTask<List<WearableAssetInfo>> GetDefaultHairAssets(HairType hairType, CancellationToken cancellationToken = default)
        {
            if (await InitializeAsync() is false)
            {
                throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
            }

            var categories = hairType == HairType.All
                ? null
                : HairTypeToWardrobeSubcategory.TryGetValue(hairType, out var subcategory)
                    ? new List<WardrobeSubcategory> { subcategory }
                    : null;

            return await GetWearableAssetsByCategoryAsync(RequestType.Default, categories, false, cancellationToken);
        }

        public static async UniTask<List<WearableAssetInfo>> GetDefaultWearablesByCategoryAsync(WearablesCategory wearableCategory, bool forceFetch, CancellationToken cancellationToken = default)
        {
            if (await InitializeAsync() is false)
            {
                throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
            }

            var categories = wearableCategory == WearablesCategory.All
                ? null
                : WearableCategoryToWardrobeSubcategory.TryGetValue(wearableCategory, out var subcategory)
                    ? new List<WardrobeSubcategory> { subcategory }
                    : null;

            return await GetWearableAssetsByCategoryAsync(RequestType.Default, categories, forceFetch, cancellationToken);
        }

        public static async UniTask<List<WearableAssetInfo>> GetUserWearablesByCategoryAsync(UserWearablesCategory userWearableCategory, bool forceFetch, CancellationToken cancellationToken = default)
        {
            if (await InitializeAsync() is false)
            {
                throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
            }

            var categories = userWearableCategory == UserWearablesCategory.All
                ? null
                : WearableCategoryToWardrobeSubcategory.TryGetValue(ToWearablesCategory(userWearableCategory), out var subcategory)
                    ? new List<WardrobeSubcategory> { subcategory }
                    : null;

            return await GetWearableAssetsByCategoryAsync(RequestType.User, categories, forceFetch, cancellationToken);
        }

        public static async UniTask<List<AvatarFeaturesInfo>> GetDefaultAvatarFeaturesByCategory(AvatarFeatureCategory category, CancellationToken cancellationToken = default)
        {
            string categoryFilter = (category == AvatarFeatureCategory.None || category == AvatarFeatureCategory.All) ? null : category.ToString();
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return new List<AvatarFeaturesInfo>();
                }

                return await service.GetAvatarFeatureAssetInfoListByCategoryAsync(categoryFilter, null, cancellationToken);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to get default avatar features data: {ex.Message}");
                return new List<AvatarFeaturesInfo>();
            }
        }

        public static async UniTask<(bool, string)> GiveAssetToUserAsync(string assetId)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return (false, "AvatarCustomizationService not found");
                }

                return await service.GiveAssetToUserAsync(assetId);
            }
            catch (Exception ex)
            {
                string error = $"Failed to give asset to user: {ex.Message}";
                CrashReporter.LogError(error);
                return (false, error);
            }
        }

        public static async UniTask<List<IColor>> GetDefaultColorsAsync(ColorType colorType, CancellationToken cancellationToken = default)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return new List<IColor>();
                }

                if (colorType == ColorType.All)
                {
                    var tasks = new List<UniTask<List<IColor>>>();
                    foreach (ColorType ct in Enum.GetValues(typeof(ColorType)))
                    {
                        if (ct == ColorType.All)
                        {
                            continue;
                        }

                        tasks.Add(service.GetDefaultColorsAsync(ct, cancellationToken));
                    }
                    var results = await UniTask.WhenAll(tasks);
                    var combined = new List<IColor>();
                    foreach (var list in results)
                    {
                        if (list != null)
                        {
                            combined.AddRange(list);
                        }
                    }
                    return combined;
                }

                return await service.GetDefaultColorsAsync(colorType, cancellationToken);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to get default {colorType} colors: {ex.Message}");
                return new List<IColor>();
            }
        }

        public static async UniTask<List<AvatarMakeupInfo>> GetDefaultMakeupByCategoryAsync(MakeupCategory category, CancellationToken cancellationToken = default)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return new List<AvatarMakeupInfo>();
                }

                var categories = category == MakeupCategory.All
                    ? null
                    : new List<MakeupCategory> { category };

                return await service.GetMakeupAssetInfoListByCategoryAsync(categories, cancellationToken);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to get default makeup: {ex.Message}");
                return new List<AvatarMakeupInfo>();
            }
        }

        public static async UniTask<List<AvatarTattooInfo>> GetDefaultTattoosAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return new List<AvatarTattooInfo>();
                }

                var tattooAssetInfoList = await service.GetDefaultTattooAssetInfoListAsync(cancellationToken);
                return AvatarTattooInfo.FromTattooAssetInfoList(tattooAssetInfoList);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to get default tattoos: {ex.Message}");
                return new List<AvatarTattooInfo>();
            }
        }

        public static async UniTask<List<ICustomColor>> GetUserColorsAsync(UserColorType colorType, CancellationToken cancellationToken = default)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return new List<ICustomColor>();
                }

                return await service.GetUserColorsAsync(colorType, cancellationToken);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to get user {colorType} colors: {ex.Message}");
                return new List<ICustomColor>();
            }
        }

        /// <summary>
        /// Creates a user (custom) color for the specified color type and stores it via DefaultInventoryService.
        /// Only Hair, Eyebrow, and Eyelash support user colors.
        /// </summary>
        /// <param name="colorType">The type of user color to create (Hair, Eyebrow, or Eyelash).</param>
        /// <param name="colors">The color values (e.g. one color for skin, multiple for hair).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>UniTask that completes with the new <see cref="ICustomColor"/> value or null if it fails</returns>
        public static async UniTask<ICustomColor> CreateUserColorAsync(UserColorType colorType, List<Color> colors, CancellationToken cancellationToken = default)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarEditorSDK");
                }

                var service = GetService();
                if (service == null)
                {
                    return null;
                }

                return await service.CreateUserColorAsync(colorType, colors, cancellationToken);
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
        /// <param name="colorValue">The custom color value to update.</param>
        /// <param name="colors">The new color values.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        public static async UniTask UpdateUserColorAsync(ICustomColor colorValue, List<Color> colors, CancellationToken cancellationToken = default)
        {
            try
            {
                if (colorValue == null)
                {
                    CrashReporter.LogError("A custom color value is required to update a user color");
                    return;
                }

                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarEditorSDK");
                }

                var service = GetService();
                if (service == null)
                {
                    return;
                }
                await service.UpdateUserColorAsync(colorValue, colors, cancellationToken);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to update user color: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a user (custom) color by instance ID via DefaultInventoryService.
        /// </summary>
        /// <param name="colorValue">The custom color to delete.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        public static async UniTask DeleteUserColorAsync(ICustomColor colorValue, CancellationToken cancellationToken = default)
        {
            try
            {
                if (colorValue == null)
                {
                    CrashReporter.LogError("A custom color value is required to delete a user color");
                    return;
                }

                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarEditorSDK");
                }

                var service = GetService();
                if (service == null)
                {
                    return;
                }
                await service.DeleteUserColorAsync(colorValue, cancellationToken);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to delete user color: {ex.Message}");
            }
        }

        #endregion

        #region Equip / Unequip

        public static async UniTask EquipWearableAsync(GeniesAvatar avatar, WearableAssetInfo assetInfo, CancellationToken cancellationToken = default)
        {
            try
            {
                if (assetInfo == null || string.IsNullOrEmpty(assetInfo.AssetId))
                {
                    CrashReporter.LogError("Valid asset is required to equip wearable");
                    return;
                }

                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return;
                }

                await service.EquipOutfitAsync(avatar, assetInfo.AssetId, cancellationToken);
                EquippedAsset?.Invoke(assetInfo.AssetId);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to equip wearable: {ex.Message}");
            }
        }

        public static async UniTask EquipWearableByWearableIdAsync(GeniesAvatar avatar, string wearableId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return;
                }

                await service.EquipOutfitAsync(avatar, wearableId, cancellationToken);
                EquippedAsset?.Invoke(wearableId);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to equip wearable: {ex.Message}");
            }
        }

        public static async UniTask UnEquipWearableAsync(GeniesAvatar avatar, WearableAssetInfo assetInfo, CancellationToken cancellationToken = default)
        {
            try
            {
                if (assetInfo == null || string.IsNullOrEmpty(assetInfo.AssetId))
                {
                    CrashReporter.LogError("Valid asset is required to unequip wearable");
                    return;
                }

                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return;
                }

                await service.UnEquipOutfitAsync(avatar, assetInfo.AssetId, cancellationToken);
                UnequippedAsset?.Invoke(assetInfo.AssetId);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to unequip wearable: {ex.Message}");
            }
        }

        public static async UniTask UnEquipWearableByWearableIdAsync(GeniesAvatar avatar, string wearableId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return;
                }

                await service.UnEquipOutfitAsync(avatar, wearableId, cancellationToken);
                UnequippedAsset?.Invoke(wearableId);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to unequip wearable: {ex.Message}");
            }
        }

        public static async UniTask EquipMakeupAsync(GeniesAvatar avatar, AvatarMakeupInfo asset, CancellationToken cancellationToken = default)
        {
            if (asset == null || string.IsNullOrEmpty(asset.AssetId))
            {
                return;
            }

            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return;
                }

                await service.EquipMakeupAsync(avatar, asset.AssetId, cancellationToken);
                EquippedAsset?.Invoke(asset.AssetId);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to set avatar makeup: {ex.Message}");
            }
        }

        public static async UniTask UnEquipMakeupAsync(GeniesAvatar avatar, AvatarMakeupInfo asset, CancellationToken cancellationToken = default)
        {
            if (asset == null || string.IsNullOrEmpty(asset.AssetId))
            {
                return;
            }

            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return;
                }

                await service.UnEquipMakeupAsync(avatar, asset.AssetId, cancellationToken);
                UnequippedAsset?.Invoke(asset.AssetId);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to unequip makeup: {ex.Message}");
            }
        }

        public static async UniTask EquipHairAsync(GeniesAvatar avatar, WearableAssetInfo asset, CancellationToken cancellationToken = default)
        {
            if (asset != null && !string.IsNullOrEmpty(asset.AssetId))
            {
                await EquipHairByHairAssetIdAsync(avatar, asset.AssetId, cancellationToken);
            }
        }

        public static async UniTask EquipHairByHairAssetIdAsync(GeniesAvatar avatar, string hairAssetId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return;
                }

                await service.EquipHairAsync(avatar, hairAssetId, cancellationToken);
                HairEquipped?.Invoke(hairAssetId);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to equip hair: {ex.Message}");
            }
        }

        public static async UniTask UnEquipHairAsync(GeniesAvatar avatar, HairType hairType, CancellationToken cancellationToken = default)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return;
                }

                await service.UnEquipHairAsync(avatar, hairType, cancellationToken);
                HairUnequipped?.Invoke(hairType.ToString());
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to unequip hair: {ex.Message}");
            }
        }

        public static async UniTask EquipTattooAsync(GeniesAvatar avatar, AvatarTattooInfo tattooInfo, MegaSkinTattooSlot tattooSlot, CancellationToken cancellationToken = default)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return;
                }

                await service.EquipTattooAsync(avatar, tattooInfo, tattooSlot, cancellationToken);
                TattooEquipped?.Invoke(tattooInfo?.AssetId);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to equip tattoo: {ex.Message}");
            }
        }

        public static async UniTask<string> UnEquipTattooAsync(GeniesAvatar avatar, MegaSkinTattooSlot tattooSlot, CancellationToken cancellationToken = default)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return null;
                }

                var unequippedId = await service.UnEquipTattooAsync(avatar, tattooSlot, cancellationToken);
                if (!string.IsNullOrEmpty(unequippedId))
                {
                    TattooUnequipped?.Invoke(unequippedId);
                }
                return unequippedId;
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to unequip tattoo: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Cache Clearing

        /// <summary>
        /// Clears the disk cache for user wearables only.
        /// Also clears the in-memory cache so subsequent calls re-fetch from the server.
        /// </summary>
        public static void ClearUserWearablesCache()
        {
            try
            {
                var defaultInventoryService = ServiceManager.Get<IDefaultInventoryService>();
                defaultInventoryService?.ClearUserWearablesCache();
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to clear user wearables disk cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears the disk cache for default wearables only (does not affect user wearables).
        /// Also clears the in-memory cache so subsequent calls re-fetch from the server.
        /// </summary>
        public static void ClearDefaultWearablesCache()
        {
            try
            {
                var defaultInventoryService = ServiceManager.Get<IDefaultInventoryService>();
                defaultInventoryService?.ClearDefaultWearablesCache();
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to clear default wearables disk cache: {ex.Message}");
            }
        }

        #endregion

        #region Colors

        public static async UniTask<bool> SetColorAsync(GeniesAvatar avatar, IColor color, CancellationToken cancellationToken = default)
        {
            try
            {
                if (color == null)
                {
                    CrashReporter.LogError("Color cannot be null");
                    return false;
                }

                if (avatar == null)
                {
                    CrashReporter.LogError("Avatar cannot be null");
                    return false;
                }

                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return false;
                }

                switch (color)
                {
                    case HairColor hairColor:
                        if (hairColor.Hexes != null && hairColor.Hexes.Length > 3)
                        {
                            await service.ModifyAvatarHairColorAsync(avatar, HairType.Hair,
                                hairColor.Hexes[0], hairColor.Hexes[1], hairColor.Hexes[2], hairColor.Hexes[3],
                                cancellationToken);
                            HairColorSet?.Invoke();
                            return true;
                        }
                        CrashReporter.LogError("HairColor must have exactly 4 color values (base, r, g, b)");
                        return false;

                    case FacialHairColor facialhairColor:
                        if (facialhairColor.Hexes != null && facialhairColor.Hexes.Length > 3)
                        {
                            await service.ModifyAvatarHairColorAsync(avatar, HairType.FacialHair,
                                facialhairColor.Hexes[0], facialhairColor.Hexes[1], facialhairColor.Hexes[2], facialhairColor.Hexes[3],
                                cancellationToken);
                            HairColorSet?.Invoke();
                            return true;
                        }
                        CrashReporter.LogError("FacialHairColor must have exactly 4 color values (base, r, g, b)");
                        return false;

                    case EyeBrowsColor eyeBrowsColor:
                        if (eyeBrowsColor.Hexes != null && eyeBrowsColor.Hexes.Length > 1)
                        {
                            await service.ModifyAvatarFlairColorAsync(avatar, HairType.Eyebrows, eyeBrowsColor.Hexes, cancellationToken);
                            HairColorSet?.Invoke();
                            return true;
                        }
                        CrashReporter.LogError("EyeBrowsColor must have exactly 2 color values");
                        return false;

                    case EyeLashColor eyeLashColor:
                        if (eyeLashColor.Hexes != null && eyeLashColor.Hexes.Length > 1)
                        {
                            await service.ModifyAvatarFlairColorAsync(avatar, HairType.Eyelashes, eyeLashColor.Hexes, cancellationToken);
                            HairColorSet?.Invoke();
                            return true;
                        }
                        CrashReporter.LogError("EyeLashColor must have exactly 2 color values");
                        return false;

                    case SkinColor skinColor:
                        if (skinColor.Hexes != null && skinColor.Hexes.Length > 0)
                        {
                            await service.SetSkinColorAsync(avatar, skinColor.Hexes[0], cancellationToken);
                            SkinColorSet?.Invoke();
                            return true;
                        }
                        CrashReporter.LogError("SkinColor must have at least one color value.");
                        return false;

                    case EyeColor eyeColor:
                        if (!string.IsNullOrEmpty(eyeColor.AssetId))
                        {
                            await service.EquipOutfitAsync(avatar, eyeColor.AssetId, cancellationToken);
                            try
                            {
                                if (avatar?.Controller == null)
                                {
                                    CrashReporter.LogError("Avatar and controller are required to set eye color");
                                    return false;
                                }

                                var command = new EquipNativeAvatarAssetCommand(eyeColor.AssetId, avatar.Controller);
                                await command.ExecuteAsync(cancellationToken);
                                EquippedAsset?.Invoke(eyeColor.AssetId);
                                return true;
                            }
                            catch (Exception ex)
                            {
                                CrashReporter.LogError($"Failed to set eye color: {ex.Message}");
                                return false;
                            }
                        }
                        CrashReporter.LogError("EyeColor must have a non-empty AssetId.");
                        return false;

                    case MakeupColor makeupColor:
                        if (makeupColor.Hexes != null && makeupColor.Hexes.Length >= 4)
                        {
                            await service.SetMakeupColorAsync(avatar, makeupColor.Category, makeupColor.Hexes, cancellationToken);
                            return true;
                        }
                        CrashReporter.LogError("Makeup color must have exactly 4 values (base, r, g, b)");
                        return false;

                    default:
                        CrashReporter.LogError($"SetColorAsync supports only HairColor, FacialHairColor, EyeBrowsColor, EyeLashColor, SkinColor, EyeColor, or MakeupColor, not {color.GetType().Name}");
                        return false;
                }
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to set color: {ex.Message}");
                return false;
            }
        }

        public static async UniTask<bool> SetColorDataAsync(GeniesAvatar avatar, AvatarColorKind colorKind, Color[] hexes, string assetId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (avatar == null)
                {
                    CrashReporter.LogError("Avatar cannot be null");
                    return false;
                }

                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return false;
                }

                switch (colorKind)
                {
                    case AvatarColorKind.Hair:
                        if (hexes != null && hexes.Length >= 4)
                        {
                            await service.ModifyAvatarHairColorAsync(avatar, HairType.Hair, hexes[0], hexes[1], hexes[2], hexes[3], cancellationToken);
                            HairColorSet?.Invoke();
                            return true;
                        }
                        CrashReporter.LogError("Hair color must have exactly 4 values (base, r, g, b)");
                        return false;

                    case AvatarColorKind.FacialHair:
                        if (hexes != null && hexes.Length >= 4)
                        {
                            await service.ModifyAvatarHairColorAsync(avatar, HairType.FacialHair, hexes[0], hexes[1], hexes[2], hexes[3], cancellationToken);
                            HairColorSet?.Invoke();
                            return true;
                        }
                        CrashReporter.LogError("FacialHair color must have exactly 4 values (base, r, g, b)");
                        return false;

                    case AvatarColorKind.EyeBrows:
                        if (hexes != null && hexes.Length > 1)
                        {
                            await service.ModifyAvatarFlairColorAsync(avatar, HairType.Eyebrows, hexes, cancellationToken);
                            return true;
                        }
                        CrashReporter.LogError("EyeBrows color must have exactly 2 values");
                        return false;

                    case AvatarColorKind.EyeLash:
                        if (hexes != null && hexes.Length > 1)
                        {
                            await service.ModifyAvatarFlairColorAsync(avatar, HairType.Eyelashes, hexes, cancellationToken);
                            return true;
                        }
                        CrashReporter.LogError("EyeLash color must have exactly 2 values");
                        return false;

                    case AvatarColorKind.Skin:
                        if (hexes != null && hexes.Length >= 1)
                        {
                            await service.SetSkinColorAsync(avatar, hexes[0], cancellationToken);
                            SkinColorSet?.Invoke();
                            return true;
                        }
                        CrashReporter.LogError("Skin color must have at least one value.");
                        return false;

                    case AvatarColorKind.Eyes:
                        if (!string.IsNullOrEmpty(assetId))
                        {
                            await service.EquipOutfitAsync(avatar, assetId, cancellationToken);
                            var command = new EquipNativeAvatarAssetCommand(assetId, avatar.Controller);
                            await command.ExecuteAsync(cancellationToken);
                            EquippedAsset?.Invoke(assetId);
                            return true;
                        }
                        CrashReporter.LogError("Eye color must have a non-empty AssetId.");
                        return false;

                    case AvatarColorKind.MakeupStickers:
                    case AvatarColorKind.MakeupLipstick:
                    case AvatarColorKind.MakeupFreckles:
                    case AvatarColorKind.MakeupFaceGems:
                    case AvatarColorKind.MakeupEyeshadow:
                    case AvatarColorKind.MakeupBlush:
                        if (hexes != null && hexes.Length >= 4)
                        {
                            MakeupCategory category = GetMakeupCategoryFromAvatarColorKind(colorKind);
                            await service.SetMakeupColorAsync(avatar, category, hexes, cancellationToken);
                            return true;
                        }
                        CrashReporter.LogError("Makeup color must have exactly 4 values (base, r, g, b)");
                        return false;

                    default:
                        CrashReporter.LogError($"Unknown AvatarColorKind: {colorKind}");
                        return false;
                }
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to set color: {ex.Message}");
                return false;
            }
        }

        public static async UniTask<IColor> GetColorAsync(GeniesAvatar avatar, AvatarColorKind colorKind, CancellationToken cancellationToken = default)
        {
            if (avatar == null)
            {
                CrashReporter.LogError("Avatar cannot be null");
                return null;
            }

            if (await InitializeAsync() is false)
            {
                throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
            }

            var service = GetService();
            if (service == null)
            {
                return null;
            }

            return await service.GetColorAsync(avatar, colorKind, cancellationToken);
        }

        public static async UniTask<(Color[] hexes, string assetId)> GetColorDataAsync(GeniesAvatar avatar, AvatarColorKind colorKind, CancellationToken cancellationToken = default)
        {
            var color = await GetColorAsync(avatar, colorKind, cancellationToken);
            if (color == null)
            {
                return (null, null);
            }
            return (color.Hexes, color.AssetId);
        }

        #endregion

        #region Body Preset / Body Type / Feature Stats

        public static async UniTask SetNativeAvatarBodyPresetAsync(GeniesAvatar avatar, NativeAvatarBodyPresetInfo presetInfo, CancellationToken cancellationToken = default)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return;
                }

                if (presetInfo == null)
                {
                    CrashReporter.LogError("presetInfo cannot be null");
                    return;
                }

                var gSkelModValues = presetInfo.Attributes?
                    .Select(a => new GSkelModValue { Name = a.Name, Value = a.Value })
                    .ToList() ?? new List<GSkelModValue>();

                var preset = GSkelModifierPreset.CreateInstance<GSkelModifierPreset>();
                preset.Name = presetInfo.Name ?? string.Empty;
                preset.StartingBodyVariation = presetInfo.StartingBodyVariation ?? string.Empty;
                preset.GSkelModValues = gSkelModValues;

                await service.SetNativeAvatarBodyPresetAsync(avatar, preset, cancellationToken);
                BodyPresetSet?.Invoke();
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to set native body preset: {ex.Message}");
            }
        }

        public async static UniTask<NativeAvatarBodyPresetInfo> GetNativeAvatarBodyPresetAsync(GeniesAvatar avatar, CancellationToken cancellationToken = default)
        {
            if (await InitializeAsync() is false)
            {
                throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
            }

            var service = GetService();
            if (service == null)
            {
                return null;
            }

            var preset = service.GetNativeAvatarBodyPreset(avatar);
            return NativeAvatarBodyPresetInfo.FromPreset(preset);
        }

        public static async UniTask SetAvatarBodyTypeAsync(GeniesAvatar avatar, GenderType genderType, BodySize bodySize, CancellationToken cancellationToken = default)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return;
                }

                await service.SetAvatarBodyTypeAsync(avatar, genderType, bodySize, cancellationToken);
                BodyTypeSet?.Invoke();
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to set avatar body type: {ex.Message}");
            }
        }

        public static async UniTask<bool> SetAvatarFeatureAsync(GeniesAvatar avatar, AvatarFeaturesInfo feature, CancellationToken cancellationToken = default)
        {
            try
            {
                if (avatar?.Controller == null)
                {
                    CrashReporter.LogError("Avatar and controller are required to set avatar feature");
                    return false;
                }

                if (string.IsNullOrEmpty(feature?.AssetId))
                {
                    CrashReporter.LogError("Valid feature and AssetId is required to set avatar feature");
                    return false;
                }

                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return false;
                }

                await service.EquipOutfitAsync(avatar, feature.AssetId, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to set avatar feature: {ex.Message}");
                return false;
            }
        }

        public static async UniTask<bool> ModifyAvatarFeatureStatAsync(GeniesAvatar avatar, AvatarFeatureStat stat, float value, CancellationToken cancellationToken = default)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                return service != null && service.ModifyAvatarFeatureStat(avatar, stat, Mathf.Clamp(value, -1.0f, 1.0f), cancellationToken);
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

        public static async UniTask<bool> ModifyAvatarFeatureStatsAsync(GeniesAvatar avatar, IReadOnlyDictionary<AvatarFeatureStat, float> stats, CancellationToken cancellationToken = default)
        {
            try
            {
                if (avatar?.Controller == null)
                {
                    CrashReporter.LogError("Avatar and controller are required to modify avatar feature stats");
                    return false;
                }

                if (stats == null || stats.Count == 0)
                {
                    return true;
                }

                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                cancellationToken.ThrowIfCancellationRequested();
                bool result = true;
                foreach (var kvp in stats)
                {
                    result &= await ModifyAvatarFeatureStatAsync(avatar, kvp.Key, kvp.Value, cancellationToken);
                }
                return result;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to modify avatar feature stats: {ex.Message}");
                return false;
            }
        }

        public static async UniTask<Dictionary<AvatarFeatureStat, float>> GetAvatarFeatureStatsAsync(GeniesAvatar avatar, AvatarFeatureStatType statType)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return new Dictionary<AvatarFeatureStat, float>();
                }

                return service.GetAvatarFeatureStats(avatar, statType);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to get avatar feature stats: {ex.Message}");
                return new Dictionary<AvatarFeatureStat, float>();
            }
        }

        #endregion

        #region Save / Load

        public static async UniTask SaveAvatarDefinitionAsync(GeniesAvatar avatar)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return;
                }

                await service.SaveAvatarDefinitionAsync(avatar);
                AvatarDefinitionSaved?.Invoke();
                AvatarDefinitionSavedToCloud?.Invoke();
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to save avatar definition: {ex.Message}");
            }
        }

        public static async UniTask SaveAvatarDefinitionLocallyAsync(GeniesAvatar avatar, string profileId = null)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return;
                }

                service.SaveAvatarDefinitionLocally(avatar, profileId);
                AvatarDefinitionSaved?.Invoke();
                AvatarDefinitionSavedLocally?.Invoke();
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to save avatar definition locally: {ex.Message}");
            }
        }

        public static async UniTask<GeniesAvatar> LoadFromLocalAvatarDefinitionAsync(string profileId,
            bool showLoadingSilhouette = true,
            int[] lods = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return null;
                }

                var avatar = await service.LoadFromLocalAvatarDefinitionAsync(
                    profileId,
                    showLoadingSilhouette,
                    lods,
                    cancellationToken);
                if (avatar == null)
                {
                    CrashReporter.LogError($"Failed to load avatar from definition: {profileId}");
                    return null;
                }

                AvatarLoadedForEditing?.Invoke();
                return avatar;
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to load avatar definition: {ex.Message}");
                return null;
            }
        }

        public static async UniTask<GeniesAvatar> LoadFromLocalGameObjectAsync(string profileId,
            bool showLoadingSilhouette = true,
            int[] lods = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return null;
                }

                var avatar = await service.LoadFromLocalGameObjectAsync(profileId,
                    showLoadingSilhouette,
                    lods,
                    cancellationToken);
                if (avatar == null)
                {
                    CrashReporter.LogError($"Failed to load avatar from game object: {profileId}");
                    return null;
                }

                AvatarLoadedForEditing?.Invoke();
                return avatar;
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to load avatar definition: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Screenshots

        /// <summary>
        /// Creates an avatar headshot screenshot (PNG). Uses the avatar's head transform for framing.
        /// </summary>
        /// <param name="avatar">The avatar to capture.</param>
        /// <param name="saveFilePath">Output file path (relative to <paramref name="saveLocation"/> when not rooted). If null or empty, save to file is skipped.</param>
        /// <param name="config">Screenshot options. If null, uses <see cref="ScreenshotConfig.Default"/>.</param>
        /// <param name="saveLocation">Root for <paramref name="saveFilePath"/>. Default is PersistentDataPath.</param>
        /// <returns>UniTask that completes with PNG bytes, or null if initialization/avatar/head is invalid.</returns>
        public static async UniTask<byte[]> CreateAvatarScreenshotAsync(
            GeniesAvatar avatar,
            string saveFilePath = null,
            ScreenshotConfig? config = null,
            ScreenshotSaveLocation saveLocation = ScreenshotSaveLocation.PersistentDataPath)
        {
            try
            {
                if (await InitializeAsync() is false)
                {
                    throw new InvalidOperationException("Failed to initialize AvatarCustomizationSdk");
                }

                var service = GetService();
                if (service == null)
                {
                    return null;
                }

                return service.CreateAvatarScreenshot(avatar, saveFilePath, config, saveLocation);
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to create avatar screenshot: {ex.Message}");
                return null;
            }
        }


        #endregion
    }
}
