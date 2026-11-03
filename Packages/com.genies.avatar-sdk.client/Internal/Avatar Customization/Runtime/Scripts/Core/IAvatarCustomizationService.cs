using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Genies.Avatars.Sdk;
using Genies.Inventory;
using Genies.Refs;
using GnWrappers;
using UnityEngine;
using Genies.Services.Model;
using CancellationToken = System.Threading.CancellationToken;

namespace Genies.Avatars.Customization
{
    /// <summary>
    /// Core service interface for avatar customization APIs.
    /// Provides methods for getting and setting avatar data (wearables, makeup, tattoos, colors,
    /// body presets, feature stats, etc.) without requiring the full avatar editor UI.
    /// </summary>
    internal interface IAvatarCustomizationService
    {
        /// <summary>
        /// Gets a simplified list of wearable asset information from the default inventory service.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>A list of WearableAssetInfo structs containing AssetId, AssetType, Name, and Category</returns>
        public UniTask<List<WearableAssetInfo>> GetWearableAssetInfoListAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a simplified list of wearable asset information filtered by categories from the default inventory service.
        /// </summary>
        /// <param name="categories">List of WardrobeSubcategory enum values to filter by. If null or empty, returns all wearables.</param>
        /// <param name="forceFetch">If true, bypasses disk and in-memory caches and fetches fresh data from the server.</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>A list of WearableAssetInfo structs containing AssetId, AssetType, Name, and Category</returns>
        public UniTask<List<WearableAssetInfo>> GetDefaultWearableAssetsListByCategoriesAsync(List<WardrobeSubcategory> categories, bool forceFetch = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a simplified list of user wearable asset information filtered by categories from the inventory service.
        /// </summary>
        /// <param name="categories">List of WardrobeSubcategory enum values to filter by. If null or empty, returns all user wearables.</param>
        /// <param name="forceFetch">If true, bypasses disk and in-memory caches and fetches fresh data from the server.</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>A list of WearableAssetInfo structs containing AssetId, AssetType, Name, and Category</returns>
        public UniTask<List<WearableAssetInfo>> GetUserWearableAssetsListByCategoriesAsync(List<WardrobeSubcategory> categories = null, bool forceFetch = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets default avatar makeup assets filtered by category from the default inventory service.
        /// </summary>
        /// <param name="categories">List of MakeupCategory. If null or empty, returns all default makeup.</param>
        /// <param name="limit">Optional limit on the number of items to return.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A list of DefaultInventoryAsset for the requested makeup categories.</returns>
        public UniTask<List<DefaultInventoryAsset>> GetDefaultMakeupByCategoryAsync(List<MakeupCategory> categories, int? limit = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a simplified list of makeup asset information (including thumbnails) for the given categories.
        /// </summary>
        /// <param name="categories">List of MakeupCategory to filter by. If null or empty, returns all default makeup.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A list of AvatarMakeupInfo containing AssetId, AssetType, Name, Category, and Icon.</returns>
        public UniTask<List<AvatarMakeupInfo>> GetMakeupAssetInfoListByCategoryAsync(List<MakeupCategory> categories, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets default avatar features data filtered by category from the default inventory service.
        /// </summary>
        /// <param name="category">The AvatarBaseCategory to filter by. None returns all avatar base data.</param>
        /// <param name="limit">Optional limit for pagination.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A list of AvatarFeaturesInfo containing asset information.</returns>
        public UniTask<List<AvatarFeaturesInfo>> GetDefaultAvatarFeaturesByCategory(AvatarBaseCategory category, int? limit = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets default avatar features data filtered by category string.
        /// </summary>
        /// <param name="categoryFilter">Category name to filter by, or null for all.</param>
        /// <param name="limit">Optional limit for pagination.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A list of AvatarFeaturesInfo containing asset information.</returns>
        public UniTask<List<AvatarFeaturesInfo>> GetDefaultAvatarFeaturesByCategory(string categoryFilter, int? limit = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a simplified list of avatar feature asset information (including thumbnails) for the given category.
        /// </summary>
        /// <param name="category">The AvatarBaseCategory to filter by. None returns all avatar base data.</param>
        /// <param name="limit">Optional limit for pagination.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A list of AvatarFeaturesInfo containing AssetId, AssetType, Name, Category, and Icon.</returns>
        public UniTask<List<AvatarFeaturesInfo>> GetAvatarFeatureAssetInfoListByCategoryAsync(AvatarBaseCategory category, int? limit = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a simplified list of avatar feature asset information (including thumbnails) for the given category string.
        /// </summary>
        /// <param name="categoryFilter">Category name to filter by, or null for all.</param>
        /// <param name="limit">Optional limit for pagination.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A list of AvatarFeaturesInfo containing AssetId, AssetType, Name, Category, and Icon.</returns>
        public UniTask<List<AvatarFeaturesInfo>> GetAvatarFeatureAssetInfoListByCategoryAsync(string categoryFilter, int? limit = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a simplified list of tattoo asset information (including thumbnails) from the default image library.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A list of TattooAssetInfo containing AssetId, AssetType, Name, Category, and Icon.</returns>
        public UniTask<List<TattooAssetInfo>> GetDefaultTattooAssetInfoListAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets default (curated) color presets for the specified color type.
        /// </summary>
        /// <param name="colorType">The type of color to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A list of IColor containing asset IDs and color values.</returns>
        public UniTask<List<IColor>> GetDefaultColorsAsync(ColorType colorType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets user (custom) color presets for the specified color type. Only Hair, Eyebrow, and Eyelash support user colors.
        /// </summary>
        /// <param name="colorType">The type of user color to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>A list of <see cref="ICustomColor"/> containing user-defined color presets.</returns>
        public UniTask<List<ICustomColor>> GetUserColorsAsync(UserColorType colorType, CancellationToken cancellationToken = default);

                /// <summary>
        /// Creates a user (custom) color for the specified color type and stores it via DefaultInventoryService.
        /// Only Hair, Eyebrow, and Eyelash support user colors.
        /// </summary>
        /// <param name="colorType">The type of user color to create (Hair, Eyebrow, or Eyelash).</param>
        /// <param name="colors">The color values (e.g. one color for skin, multiple for hair).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>UniTask that completes with the new custom color instance ID, or null if creation failed.</returns>
        public UniTask<ICustomColor> CreateUserColorAsync(UserColorType colorType, List<Color> colors, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing user (custom) color by instance ID via DefaultInventoryService.
        /// </summary>
        /// <param name="instanceId">The instance ID of the custom color to update.</param>
        /// <param name="colors">The new color values.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        public UniTask UpdateUserColorAsync(string instanceId, List<Color> colors, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing user (custom) color by an instance of ICustomColor.
        /// </summary>
        /// <param name="colorValue">The instance of ICustomColor to update.</param>
        /// <param name="colors">The new color values.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        public UniTask UpdateUserColorAsync(ICustomColor colorValue, List<Color> colors, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a user (custom) color by instance ID via DefaultInventoryService.
        /// </summary>
        /// <param name="instanceId">The instance ID of the custom color to delete.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        public UniTask DeleteUserColorAsync(string instanceId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a user (custom) color by an instance of ICustomColor.
        /// </summary>
        /// <param name="colorValue">The instance of ICustomColor to delete.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        public UniTask DeleteUserColorAsync(ICustomColor colorValue, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets user (custom) colors filtered by category via DefaultInventoryService.
        /// </summary>
        /// <param name="colorType">Optional user color type to filter by (Hair, Eyebrow, Eyelash, etc.). Null returns all categories.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>UniTask that completes with the list of custom color responses from the inventory service.</returns>
        public UniTask<List<CustomColorResponse>> GetUserColorsByCategoryAsync(UserColorType? colorType = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current color from the avatar for the specified color kind.
        /// </summary>
        /// <param name="avatar">The avatar to read the color from.</param>
        /// <param name="colorKind">Which IColor type to return.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>UniTask that completes with the corresponding IColor value, or null if avatar is null.</returns>
        public UniTask<IColor> GetColorAsync(GeniesAvatar avatar, AvatarColorKind colorKind, CancellationToken cancellationToken = default);

        /// <summary>
        /// Equips an avatar makeup asset on the specified avatar by running EquipNativeAvatarAssetCommand with the asset's AssetId.
        /// </summary>
        /// <param name="avatar">The avatar to equip the makeup on.</param>
        /// <param name="asset">The default inventory asset to equip.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>UniTask representing the async operation.</returns>
        public UniTask EquipMakeupAsync(GeniesAvatar avatar, DefaultInventoryAsset asset, CancellationToken cancellationToken = default);

        /// <summary>
        /// Equips an avatar makeup asset on the specified avatar by asset ID.
        /// </summary>
        /// <param name="avatar">The avatar to equip the makeup on.</param>
        /// <param name="makeupAssetId">The ID of the makeup asset to equip.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>UniTask representing the async operation.</returns>
        public UniTask EquipMakeupAsync(GeniesAvatar avatar, string makeupAssetId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unequips a makeup asset from the specified avatar by asset ID.
        /// </summary>
        /// <param name="avatar">The avatar to unequip the makeup from.</param>
        /// <param name="makeupAssetId">The ID of the makeup asset to unequip.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>UniTask representing the async operation.</returns>
        public UniTask UnEquipMakeupAsync(GeniesAvatar avatar, string makeupAssetId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Applies makeup colors for the given makeup category.
        /// </summary>
        /// <param name="avatar">The avatar to apply the makeup colors on.</param>
        /// <param name="category">The makeup category.</param>
        /// <param name="colors">The colors to apply for that category.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>UniTask representing the async operation.</returns>
        public UniTask SetMakeupColorAsync(GeniesAvatar avatar, MakeupCategory category, Color[] colors, CancellationToken cancellationToken = default);

        /// <summary>
        /// Grants an asset to a user, adding it to their inventory.
        /// </summary>
        /// <param name="assetId">Id of the asset</param>
        /// <returns>UniTask with a bool indicating success and string for any failure reason.</returns>
        public UniTask<(bool, string)> GiveAssetToUserAsync(string assetId);

        /// <summary>
        /// Equips an outfit by wearable ID using the default inventory service.
        /// </summary>
        /// <param name="avatar">The avatar to equip the asset on</param>
        /// <param name="wearableId">The ID of the wearable to equip</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>UniTask representing the async operation</returns>
        public UniTask EquipOutfitAsync(GeniesAvatar avatar, string wearableId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unequips an outfit by wearable ID using the default inventory service.
        /// </summary>
        /// <param name="avatar">The avatar to unequip the asset from</param>
        /// <param name="wearableId">The ID of the wearable to unequip</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>UniTask representing the async operation</returns>
        public UniTask UnEquipOutfitAsync(GeniesAvatar avatar, string wearableId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Equips a skin color on the specified avatar.
        /// </summary>
        /// <param name="avatar">The avatar to equip the skin color on</param>
        /// <param name="skinColor">The color to apply as skin color</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>UniTask representing the async operation</returns>
        public UniTask SetSkinColorAsync(GeniesAvatar avatar, Color skinColor, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets hair colors on the specified avatar. Hair colors consist of four components: base, R, G, and B.
        /// </summary>
        /// <param name="avatar">The avatar to set the hair colors on</param>
        /// <param name="hairType">The type of hair to modify</param>
        /// <param name="baseColor">The base hair color</param>
        /// <param name="colorR">The red component of the hair color gradient</param>
        /// <param name="colorG">The green component of the hair color gradient</param>
        /// <param name="colorB">The blue component of the hair color gradient</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>UniTask representing the async operation</returns>
        public UniTask ModifyAvatarHairColorAsync(GeniesAvatar avatar, HairType hairType, Color baseColor, Color colorR, Color colorG, Color colorB, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets flair colors (eyebrows or eyelashes) on the specified avatar.
        /// </summary>
        /// <param name="avatar">The avatar to set the flair colors on</param>
        /// <param name="hairType">The type of flair to modify (HairType.Eyebrows or HairType.Eyelashes)</param>
        /// <param name="colors">The colors</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>UniTask representing the async operation</returns>
        public UniTask ModifyAvatarFlairColorAsync(GeniesAvatar avatar, HairType hairType, Color[] colors, CancellationToken cancellationToken = default);

        /// <summary>
        /// Equips a hair style on the specified avatar.
        /// </summary>
        /// <param name="avatar">The avatar to equip the hair style on</param>
        /// <param name="hairAssetId">The ID of the hair asset to equip</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>UniTask representing the async operation</returns>
        public UniTask EquipHairAsync(GeniesAvatar avatar, string hairAssetId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unequips a hair style from the specified avatar.
        /// </summary>
        /// <param name="avatar">The avatar to unequip the hair style from</param>
        /// <param name="hairType">The type of hair to unequip</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>UniTask representing the async operation</returns>
        public UniTask UnEquipHairAsync(GeniesAvatar avatar, HairType hairType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Equips a tattoo on the specified avatar at the given slot.
        /// </summary>
        /// <param name="avatar">The avatar to equip the tattoo on</param>
        /// <param name="tattooInfo">The tattoo info to equip</param>
        /// <param name="tattooSlot">The MegaSkinTattooSlot where the tattoo should be placed</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>UniTask representing the async operation</returns>
        public UniTask EquipTattooAsync(GeniesAvatar avatar, AvatarTattooInfo tattooInfo, MegaSkinTattooSlot tattooSlot, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unequips a tattoo from the specified avatar at the given slot.
        /// </summary>
        /// <param name="avatar">The avatar to unequip the tattoo from</param>
        /// <param name="tattooSlot">The MegaSkinTattooSlot where the tattoo is placed</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>UniTask that completes with the tattoo ID that was unequipped, or null if none.</returns>
        public UniTask<string> UnEquipTattooAsync(GeniesAvatar avatar, MegaSkinTattooSlot tattooSlot, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the body preset for the specified avatar.
        /// </summary>
        /// <param name="avatar">The avatar to set the body preset on</param>
        /// <param name="preset">The GSkelModifierPreset to apply</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>UniTask representing the async operation</returns>
        public UniTask SetNativeAvatarBodyPresetAsync(GeniesAvatar avatar, GSkelModifierPreset preset, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current native avatar body preset from the specified avatar.
        /// </summary>
        /// <param name="avatar">The avatar to get the body preset from</param>
        /// <returns>The current GSkelModifierPreset, or null if avatar/controller is null</returns>
        public GSkelModifierPreset GetNativeAvatarBodyPreset(GeniesAvatar avatar);

        /// <summary>
        /// Gets current values for a given avatar feature stat type.
        /// </summary>
        /// <param name="avatar">The avatar to read from.</param>
        /// <param name="statType">Which stat category to return.</param>
        /// <returns>Dictionary of AvatarFeatureStat to current value. Empty if avatar/controller is null.</returns>
        public Dictionary<AvatarFeatureStat, float> GetAvatarFeatureStats(GeniesAvatar avatar, AvatarFeatureStatType statType);

        /// <summary>
        /// Modifies a single avatar feature stat. Value is clamped to -1.0..1.0.
        /// </summary>
        /// <param name="avatar">The avatar to modify.</param>
        /// <param name="stat">The feature stat to set.</param>
        /// <param name="value">The value to set (clamped between -1.0 and 1.0).</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>True if the update was applied, false if invalid or error occurred.</returns>
        public bool ModifyAvatarFeatureStat(GeniesAvatar avatar, AvatarFeatureStat stat, float value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the avatar body type with specified gender and body size.
        /// </summary>
        /// <param name="avatar">The avatar to set the body type on</param>
        /// <param name="genderType">The gender type</param>
        /// <param name="bodySize">The body size</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>UniTask representing the async operation</returns>
        public UniTask SetAvatarBodyTypeAsync(GeniesAvatar avatar, GenderType genderType, BodySize bodySize, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves the current avatar definition to the cloud.
        /// </summary>
        /// <param name="avatar">The avatar to save</param>
        /// <returns>A UniTask that completes when the save operation is finished.</returns>
        public UniTask SaveAvatarDefinitionAsync(GeniesAvatar avatar);

        /// <summary>
        /// Saves the current avatar definition locally only.
        /// </summary>
        /// <param name="avatar">The avatar to save</param>
        /// <param name="profileId">The profile ID to save the avatar as. If null, uses the default template name.</param>
        public void SaveAvatarDefinitionLocally(GeniesAvatar avatar, string profileId = null);

        /// <summary>
        /// Loads an avatar definition from a local profile.
        /// </summary>
        /// <param name="profileId">The profile to load</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <param name="showLoadingSilhouette">Optional parameters to preload avatar's silhouette while it is loading</param>
        /// <param name="lods">Optional LOD levels for avatar quality</param>
        /// <returns>A UniTask that completes with the loaded GeniesAvatar</returns>
        public UniTask<GeniesAvatar> LoadFromLocalAvatarDefinitionAsync(string profileId,
            bool showLoadingSilhouette = true,
            int[] lods = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads an avatar definition from a local game object.
        /// </summary>
        /// <param name="profileId">The profile to load</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <param name="showLoadingSilhouette">Optional parameters to preload avatar's silhouette while it is loading</param>
        /// <param name="lods">Optional LOD levels for avatar quality</param>
        /// <returns>A UniTask that completes with the loaded GeniesAvatar</returns>
        public UniTask<GeniesAvatar> LoadFromLocalGameObjectAsync(string profileId,
            bool showLoadingSilhouette = true,
            int[] lods = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Uploads an avatar image for the specified avatar.
        /// </summary>
        /// <param name="imageData">The image data as a byte array.</param>
        /// <param name="avatarId">The ID of the avatar to upload the image for.</param>
        /// <returns>A task that completes when the upload is done.</returns>
        public UniTask UploadAvatarImageAsync(byte[] imageData, string avatarId);

        /// <summary>
        /// Creates an avatar headshot screenshot (PNG) using the avatar's head transform.
        /// </summary>
        /// <param name="avatar">The avatar to capture.</param>
        /// <param name="saveFilePath">Output file path (relative to <paramref name="saveLocation"/> when not rooted). If null or empty, save to file is skipped.</param>
        /// <param name="config">Screenshot options. If null, uses default configuration.</param>
        /// <param name="saveLocation">Root for <paramref name="saveFilePath"/>. When null, uses PersistentDataPath.</param>
        /// <returns>PNG bytes, or null if avatar/controller/head is invalid.</returns>
        byte[] CreateAvatarScreenshot(GeniesAvatar avatar,
            string saveFilePath = null,
            ScreenshotConfig? config = null,
            ScreenshotSaveLocation? saveLocation = null);

        /// <summary>
        /// Remove a sprite from an internal managed cache so it can be garbage collected.
        /// </summary>
        /// <param name="spriteRef">The ref to the sprite</param>
        public void RemoveSpriteReference(Ref<Sprite> spriteRef);
    }
}
