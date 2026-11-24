using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Genies.Sdk.Avatar.Samples.CustomAvatarEditor
{
    /// <summary>
    /// Static class to retrieve data used for UI display and asset equipping.
    /// </summary>
    public static class AssetInfoDataSource
    {
        /// <summary>
        /// Gets data about wearables to be used for UI.
        /// </summary>
        public static async UniTask<List<WearableAssetInfo>> GetWearablesDataForCategory(WearablesCategory category)
        {
            if (!AvatarSdk.IsLoggedIn)
            {
                Debug.LogError("You must be logged in to get data");
                return new();
            }

            return await AvatarSdk.GetDefaultWearablesByCategoryAsync(category);
        }

        /// <summary>
        /// Gets user-owned wearable data.
        /// </summary>
        public static async UniTask<List<WearableAssetInfo>> GetUserWearablesDataForCategory(UserWearablesCategory category)
        {
            if (!AvatarSdk.IsLoggedIn)
            {
                Debug.LogError("You must be logged in to get data");
                return new();
            }

            return await AvatarSdk.GetUserWearablesByCategoryAsync(category);

        }

        /// <summary>
        /// Gets makeup data for the given category.
        /// </summary>
        public static async UniTask<List<AvatarMakeupInfo>> GetMakeupDataForCategory(AvatarMakeupCategory category)
        {
            if (!AvatarSdk.IsLoggedIn)
            {
                Debug.LogError("You must be logged in to get data");
                return new();
            }

            return await AvatarSdk.GetDefaultMakeupByCategoryAsync(category);
        }

        /// <summary>
        /// Gets tattoo data
        /// </summary>
        public static async UniTask<List<AvatarTattooInfo>> GetTattooData()
        {
            if (!AvatarSdk.IsLoggedIn)
            {
                Debug.LogError("You must be logged in to get data");
                return new();
            }

            return await AvatarSdk.GetDefaultTattoosAsync();
        }

        /// <summary>
        /// Gets data about hair to be used for UI.
        /// </summary>
        public static async UniTask<List<WearableAssetInfo>> GetAvatarHair(HairType hairType)
        {
            if (!AvatarSdk.IsLoggedIn)
            {
                Debug.LogError("You must be logged in to get data");
                return new();
            }

            return await AvatarSdk.GetDefaultHairAssets(hairType);
        }

        /// <summary>
        /// Gets data about avatar features to be used for UI.
        /// </summary>
        public static async UniTask<List<AvatarFeaturesInfo>> GetAvatarFeatureDataForCategory(AvatarFeatureCategory category)
        {

            if (!AvatarSdk.IsLoggedIn)
            {
                Debug.LogError("You must be logged in to get data");
                return new();
            }

            return await AvatarSdk.GetDefaultAvatarFeaturesByCategory(category);
        }

        /// <summary>
        /// Gets color options for different avatar features.
        /// </summary>
        public static async UniTask<List<IAvatarColor>> GetColorDataForCategory(ColorType category)
        {
            if (!AvatarSdk.IsLoggedIn)
            {
                Debug.LogError("You must be logged in to get data");
                return new();
            }

            return await AvatarSdk.GetDefaultColorsAsync(category);
        }

        /// <summary>
        /// Gets current statistics for the given face or body feature (like eye size or vertical position for the feature 'eyes')
        /// </summary>
        /// <param name="avatar">The avatar to read stats from</param>
        /// <param name="statType">The type of feature to get stats for</param>
        /// <returns>A dictionary mapping a given statistic to its current value</returns>
        public static async UniTask<Dictionary<AvatarFeatureStat, float>> GetCurrentStatsForCategory(ManagedAvatar avatar, AvatarFeatureStatType statType)
        {
            if (!AvatarSdk.IsLoggedIn)
            {
                Debug.LogError("You must be logged in to get data");
                return new();
            }

            if (avatar == null)
            {
                Debug.LogWarning("No avatar provided — cannot get stats");
                return new();
            }

            return await AvatarSdk.GetAvatarFeatureStatsAsync(avatar, statType);
        }

        public static async UniTask<WearableAssetInfo> GetEquippedAssetForCategory(ManagedAvatar avatar, WearablesCategory category)
        {
            var equippedAssetIds = avatar.GetEquippedAssetIds();
            var equippedSet = new HashSet<string>(equippedAssetIds);
            var wearables = await GetWearablesDataForCategory(category);

            foreach (var wearableInfo in wearables)
            {
                if (equippedSet.Contains("WardrobeGear/" + wearableInfo.AssetId))
                {
                    return wearableInfo;
                }
            }

            return null;
        }
    }
}
