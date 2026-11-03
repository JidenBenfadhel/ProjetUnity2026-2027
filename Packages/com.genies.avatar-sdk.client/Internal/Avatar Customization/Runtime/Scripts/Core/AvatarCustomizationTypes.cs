using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Genies.Inventory;
using Genies.Naf;
using Genies.Refs;

namespace Genies.Avatars.Customization
{
    /// <summary>
    /// Struct containing basic wearable asset information.
    /// </summary>
    internal class WearableAssetInfo : IDisposable
    {
        public string AssetId { get; set; }
        public AssetType AssetType { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public Ref<Sprite> Icon { get; set; }

        public void Dispose()
        {
            Icon.Dispose();
        }
    }

    /// <summary>
    /// Contains tattoo asset information including thumbnail, for use with GetTattooAssetInfoListAsync.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal class TattooAssetInfo : IDisposable
#else
    public class TattooAssetInfo : IDisposable
#endif
    {
        public string AssetId { get; set; }
        public AssetType AssetType { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public Ref<Sprite> Icon { get; set; }

        public void Dispose()
        {
            Icon.Dispose();
        }
    }

    /// <summary>
    /// Wrapper for DefaultAvatarBaseAsset used by GetDefaultAvatarFeaturesByCategory and SetAvatarFeatureAsync.
    /// </summary>
    [Serializable]
#if GENIES_SDK && !GENIES_INTERNAL
    internal class AvatarFeaturesInfo : IDisposable
#else
    public class AvatarFeaturesInfo : IDisposable
#endif
    {
        public string AssetId { get; set; }
        public AssetType AssetType { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public List<string> SubCategories { get; set; }
        public int Order { get; set; }
        public PipelineData PipelineData { get; set; }
        public List<string> Tags { get; set; }
        public Ref<Sprite> Icon { get; set; }

        public void Dispose()
        {
            Icon.Dispose();
        }

        /// <summary>
        /// Converts from the internal Inventory DefaultAvatarBaseAsset type.
        /// </summary>
        internal static AvatarFeaturesInfo FromInternal(DefaultAvatarBaseAsset internalAsset)
        {
            if (internalAsset == null)
            {
                return null;
            }

            return new AvatarFeaturesInfo
            {
                AssetId = internalAsset.AssetId,
                AssetType = internalAsset.AssetType,
                Name = internalAsset.Name,
                Category = internalAsset.Category,
                SubCategories = internalAsset.SubCategories,
                Order = internalAsset.Order,
                PipelineData = internalAsset.PipelineData,
                Tags = internalAsset.Tags
            };
        }

        /// <summary>
        /// Converts a list from internal DefaultAvatarBaseAsset to AvatarFeaturesInfo.
        /// </summary>
        internal static List<AvatarFeaturesInfo> FromInternalList(List<DefaultAvatarBaseAsset> internalList)
        {
            var result = new List<AvatarFeaturesInfo>();
            if (internalList == null)
            {
                return result;
            }

            foreach (var item in internalList)
            {
                var info = FromInternal(item);
                if (info != null)
                {
                    result.Add(info);
                }
            }
            return result;
        }

        /// <summary>
        /// Converts this wrapper to the internal Inventory DefaultAvatarBaseAsset type.
        /// </summary>
        internal static DefaultAvatarBaseAsset ToInternal(AvatarFeaturesInfo info)
        {
            if (info == null)
            {
                return null;
            }

            return new DefaultAvatarBaseAsset
            {
                AssetId = info.AssetId,
                AssetType = info.AssetType,
                Name = info.Name,
                Category = info.Category,
                SubCategories = info.SubCategories,
                Order = info.Order,
                PipelineData = info.PipelineData,
                Tags = info.Tags
            };
        }
    }

    /// <summary>
    /// Wrapper for DefaultInventoryAsset used by GetDefaultMakeupByCategoryAsync, GetDefaultTattoosAsync, EquipMakeupAsync, and UnEquipMakeupAsync.
    /// </summary>
    [Serializable]
#if GENIES_SDK && !GENIES_INTERNAL
    internal class AvatarMakeupInfo
#else
    public class AvatarMakeupInfo
#endif
    {
        public string AssetId { get; set; }
        public AssetType AssetType { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public List<string> SubCategories { get; set; }
        public int Order { get; set; }
        public PipelineData PipelineData { get; set; }
        public Ref<Sprite> Icon { get; set; }

        public void Dispose()
        {
            Icon.Dispose();
        }

        /// <summary>
        /// Converts from the internal Inventory DefaultInventoryAsset type.
        /// </summary>
        internal static AvatarMakeupInfo FromInternal(DefaultInventoryAsset internalAsset)
        {
            if (internalAsset == null)
            {
                return null;
            }

            return new AvatarMakeupInfo
            {
                AssetId = internalAsset.AssetId,
                AssetType = internalAsset.AssetType,
                Name = internalAsset.Name,
                Category = internalAsset.Category,
                SubCategories = internalAsset.SubCategories,
                Order = internalAsset.Order,
                PipelineData = internalAsset.PipelineData
            };
        }

        /// <summary>
        /// Converts a list from internal DefaultInventoryAsset to AvatarMakeupInfo.
        /// </summary>
        internal static List<AvatarMakeupInfo> FromInternalList(List<DefaultInventoryAsset> internalList)
        {
            var result = new List<AvatarMakeupInfo>();
            if (internalList == null)
            {
                return result;
            }

            foreach (var item in internalList)
            {
                var info = FromInternal(item);
                if (info != null)
                {
                    result.Add(info);
                }
            }
            return result;
        }

        /// <summary>
        /// Converts this wrapper to the internal Inventory DefaultInventoryAsset type.
        /// </summary>
        internal static DefaultInventoryAsset ToInternal(AvatarMakeupInfo info)
        {
            if (info == null)
            {
                return null;
            }

            return new DefaultInventoryAsset
            {
                AssetId = info.AssetId,
                AssetType = info.AssetType,
                Name = info.Name,
                Category = info.Category,
                SubCategories = info.SubCategories,
                Order = info.Order,
                PipelineData = info.PipelineData
            };
        }
    }

    /// <summary>
    /// Wrapper for DefaultInventoryAsset used by GetDefaultMakeupByCategoryAsync, GetDefaultTattoosAsync, EquipMakeupAsync, and UnEquipMakeupAsync.
    /// </summary>
    [Serializable]
#if GENIES_SDK && !GENIES_INTERNAL
    internal class AvatarTattooInfo : IDisposable
#else
    public class AvatarTattooInfo : IDisposable
#endif
    {
        public string AssetId { get; set; }
        public AssetType AssetType { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public List<string> SubCategories { get; set; }
        public int Order { get; set; }
        public PipelineData PipelineData { get; set; }
        public Ref<Sprite> Icon { get; set; }

        public void Dispose()
        {
            Icon.Dispose();
        }

        /// <summary>
        /// Converts from the internal Inventory DefaultInventoryAsset type.
        /// </summary>
        internal static AvatarTattooInfo FromInternal(DefaultInventoryAsset internalAsset)
        {
            if (internalAsset == null)
            {
                return null;
            }

            return new AvatarTattooInfo
            {
                AssetId = internalAsset.AssetId,
                AssetType = internalAsset.AssetType,
                Name = internalAsset.Name,
                Category = internalAsset.Category,
                SubCategories = internalAsset.SubCategories,
                Order = internalAsset.Order,
                PipelineData = internalAsset.PipelineData
            };
        }

        /// <summary>
        /// Converts a list from internal DefaultInventoryAsset to AvatarTattooInfo.
        /// </summary>
        internal static List<AvatarTattooInfo> FromInternalList(List<DefaultInventoryAsset> internalList)
        {
            var result = new List<AvatarTattooInfo>();
            if (internalList == null)
            {
                return result;
            }

            foreach (var item in internalList)
            {
                var info = FromInternal(item);
                if (info != null)
                {
                    result.Add(info);
                }
            }
            return result;
        }

        /// <summary>
        /// Converts a list from internal DefaultInventoryAsset to AvatarTattooInfo.
        /// </summary>
        internal static List<AvatarTattooInfo> FromService(List<DefaultInventoryAsset> internalList)
        {
            var result = new List<AvatarTattooInfo>();
            if (internalList == null)
            {
                return result;
            }
            foreach (var item in internalList)
            {
                var info = FromInternal(item);
                if (info != null)
                {
                    result.Add(info);
                }
            }
            return result;
        }

        /// <summary>
        /// Converts from TattooAssetInfo (preserves AssetId, AssetType, Name, Category, Icon). SubCategories, Order, PipelineData are left null/0.
        /// </summary>
        internal static AvatarTattooInfo From(TattooAssetInfo info)
        {
            if (info == null)
            {
                return null;
            }

            return new AvatarTattooInfo
            {
                AssetId = info.AssetId,
                AssetType = info.AssetType,
                Name = info.Name,
                Category = info.Category,
                SubCategories = null,
                Order = 0,
                PipelineData = null,
                Icon = info.Icon
            };
        }

        /// <summary>
        /// Converts a list of TattooAssetInfo to AvatarTattooInfo without losing any attribute (Icon is preserved).
        /// </summary>
        internal static List<AvatarTattooInfo> FromTattooAssetInfoList(List<TattooAssetInfo> list)
        {
            var result = new List<AvatarTattooInfo>();
            if (list == null)
            {
                return result;
            }

            foreach (var item in list)
            {
                var info = From(item);
                if (info != null)
                {
                    result.Add(info);
                }
            }
            return result;
        }

        /// <summary>
        /// Converts this wrapper to the internal Inventory DefaultInventoryAsset type.
        /// </summary>
        internal static DefaultInventoryAsset ToInternal(AvatarTattooInfo info)
        {
            if (info == null)
            {
                return null;
            }

            return new DefaultInventoryAsset
            {
                AssetId = info.AssetId,
                AssetType = info.AssetType,
                Name = info.Name,
                Category = info.Category,
                SubCategories = info.SubCategories,
                Order = info.Order,
                PipelineData = info.PipelineData
            };
        }
    }

    /// <summary>
    /// Gender types for avatar body configuration
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal enum GenderType
#else
    public enum GenderType
#endif
    {
        Male,
        Female,
        Androgynous
    }

    /// <summary>
    /// Body size types for avatar body configuration
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal enum BodySize
#else
    public enum BodySize
#endif
    {
        Skinny,
        Medium,
        Heavy
    }

    /// <summary>
    /// Wardrobe subcategory types for filtering wearable assets
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal enum WardrobeSubcategory
#else
    public enum WardrobeSubcategory
#endif
    {
        none,
        hair,
        eyebrows,
        eyelashes,
        facialHair,
        underwearTop,
        hoodie,
        shirt,
        jacket,
        dress,
        pants,
        shorts,
        skirt,
        underwearBottom,
        socks,
        shoes,
        bag,
        bracelet,
        earrings,
        glasses,
        hat,
        mask,
        watch,
        all
    }

    /// <summary>
    /// Body statistics that can be modified on the avatar
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal enum BodyStats
#else
    public enum BodyStats
#endif
    {
        NeckThickness,
        ShoulderBroadness,
        ChestBustline,
        ArmsThickness,
        WaistThickness,
        BellyFullness,
        HipsThickness,
        LegsThickness,
        HipSize
    }

    /// <summary>
    /// Eyebrow statistics that can be modified on the avatar
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal enum EyeBrowsStats
#else
    public enum EyeBrowsStats
#endif
    {
        Thickness,
        Length,
        VerticalPosition,
        Spacing
    }

    /// <summary>
    /// Eye statistics that can be modified on the avatar
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal enum EyeStats
#else
    public enum EyeStats
#endif
    {
        Size,
        VerticalPosition,
        Spacing,
        Rotation,
    }

    /// <summary>
    /// Jaw statistics that can be modified on the avatar
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal enum JawStats
#else
    public enum JawStats
#endif
    {
        Width,
        Length
    }

    /// <summary>
    /// Lip statistics that can be modified on the avatar
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal enum LipsStats
#else
    public enum LipsStats
#endif
    {
        Width,
        Fullness,
        VerticalPosition
    }

    /// <summary>
    /// Nose statistics that can be modified on the avatar
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal enum NoseStats
#else
    public enum NoseStats
#endif
    {
        Width,
        Length,
        VerticalPosition,
        Tilt,
        Projection
    }

    /// <summary>
    /// Identifies which IAvatarFeatureStat struct type to use (Body, EyeBrows, Eyes, Jaw, Lips, Nose).
    /// Used to request all stat values for that category from GetAvatarFeatureStats.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal enum AvatarFeatureStatType
#else
    public enum AvatarFeatureStatType
#endif
    {
        All,
        Body,
        EyeBrows,
        Eyes,
        Jaw,
        Lips,
        Nose
    }

    /// <summary>
    /// Unified enum for all avatar feature stats. Used as keys for GetAvatarFeatureStats.
    /// Category-prefixed to avoid name clashes (e.g. Width exists in Jaw, Lips, Nose).
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal enum AvatarFeatureStat
#else
    public enum AvatarFeatureStat
#endif
    {
        // Body (9)
        Body_NeckThickness,
        Body_ShoulderBroadness,
        Body_ChestBustline,
        Body_ArmsThickness,
        Body_WaistThickness,
        Body_BellyFullness,
        Body_HipsThickness,
        Body_LegsThickness,
        Body_HipSize,
        // EyeBrows (4)
        EyeBrows_Thickness,
        EyeBrows_Length,
        EyeBrows_VerticalPosition,
        EyeBrows_Spacing,
        // Eyes (4)
        Eyes_Size,
        Eyes_VerticalPosition,
        Eyes_Spacing,
        Eyes_Rotation,
        // Jaw (2)
        Jaw_Width,
        Jaw_Length,
        // Lips (3)
        Lips_Width,
        Lips_Fullness,
        Lips_VerticalPosition,
        // Nose (5)
        Nose_Width,
        Nose_Length,
        Nose_VerticalPosition,
        Nose_Tilt,
        Nose_Projection
    }

    /// <summary>
    /// Hair/flair type for color modification (hair, facial hair, eyebrows, or eyelashes).
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal enum HairType
#else
    public enum HairType
#endif
    {
        Hair,
        FacialHair,
        Eyebrows,
        Eyelashes,
        All = 999
    }

    /// <summary>
    /// Wearable category types (non-hair wardrobe subcategories). Excludes hair, eyebrows, eyelashes, facialHair.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal enum WearablesCategory
#else
    public enum WearablesCategory
#endif
    {
        Hoodie,
        Shirt,
        Jacket,
        Dress,
        Pants,
        Shorts,
        Skirt,
        Shoes,
        Earrings,
        Glasses,
        Hat,
        Mask,
        All = 999
    }

    /// <summary>
    /// Subset of WearablesCategory for user wearable asset APIs (e.g. GetUserWearablesByCategoryAsync).
    /// Maps to WearablesCategory via ToWearablesCategory when calling internal APIs.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal enum UserWearablesCategory
#else
    public enum UserWearablesCategory
#endif
    {
        Hoodie,
        Shirt,
        Jacket,
        Dress,
        Pants,
        Shorts,
        Skirt,
        Shoes,
        All = 999
    }

    /// <summary>
    /// Avatar feature category for GetDefaultAvatarFeaturesByCategory.
    /// Wrapper for Genies.Inventory.AvatarBaseCategory for callers that cannot reference Inventory.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal enum AvatarFeatureCategory
#else
    public enum AvatarFeatureCategory
#endif
    {
        None = 0,
        Lips = 1,
        Jaw = 2,
        Nose = 3,
        Eyes = 4,
        Brow = 5,
        All = 999
    }

    /// <summary>
    /// Source type for color preset retrieval (All, User, or Default).
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal enum ColorSource
#else
    public enum ColorSource
#endif
    {
        All,
        User,
        Default
    }

    /// <summary>
    /// Asset type for wearable asset retrieval (All, User, or Default).
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal enum RequestType
#else
    public enum RequestType
#endif
    {
        All,
        User,
        Default
    }

    /// <summary>
    /// Type of color preset to retrieve.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal enum ColorType
#else
    public enum ColorType
#endif
    {
        Eyes,
        Hair,
        FacialHair,
        Skin,
        Eyebrow,
        Eyelash,
        MakeupStickers,
        MakeupLipstick,
        MakeupFreckles,
        MakeupFaceGems,
        MakeupEyeshadow,
        MakeupBlush,
        All = 999
    }

    /// <summary>
    /// Type of user color preset to retrieve.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal enum UserColorType
#else
    public enum UserColorType
#endif
    {
        Hair,
        Eyebrow,
        Eyelash,
        FacialHair,
        Skin
    }

    /// <summary>
    /// Identifies which IColor type to get (HairColor, FacialHairColor, EyeBrowsColor, EyeLashColor, SkinColor, or EyeColor).
    /// Used with GetColorAsync to return the corresponding color value from the avatar.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal enum AvatarColorKind
#else
    public enum AvatarColorKind
#endif
    {
        Hair,
        FacialHair,
        EyeBrows,
        EyeLash,
        Skin,
        Eyes,
        MakeupStickers,
        MakeupLipstick,
        MakeupFreckles,
        MakeupFaceGems,
        MakeupEyeshadow,
        MakeupBlush
    }

    /// <summary>
    /// Public makeup category for GetDefaultMakeupByCategoryAsync. Mirrors Genies.MakeupPresets.MakeupPresetCategory
    /// so the API is usable from assemblies that cannot reference the internal type.
    /// </summary>
    public enum MakeupCategory
    {
        None = -1,
        Stickers = 0,
        Lipstick = 1,
        Freckles = 2,
        FaceGems = 3,
        Eyeshadow = 4,
        Blush = 5,
        All = 999,
    }

    /// <summary>
    /// Maps public MakeupCategory to the representation used by the internal default inventory API (lowercase string)
    /// and to the integer value used by EquipMakeupColorCommand.
    /// </summary>
    internal static class MakeupCategoryMapper
    {
        internal static string ToInternal(MakeupCategory category) => category.ToString().ToLowerInvariant();

        internal static int ToMakeupPresetCategoryInt(MakeupCategory category)
        {
            return (int)category;
        }
    }

    /// <summary>
    /// Single attribute (name/value) in a native avatar body preset.
    /// Wraps GSkelModValue for callers that cannot reference Genies.Avatars.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal struct NativeAvatarBodyPresetAttribute
#else
    public struct NativeAvatarBodyPresetAttribute
#endif
    {
        public string Name;
        public float Value;
    }

    /// <summary>
    /// Wrapper for native avatar body preset data.
    /// Mirrors GSkelModifierPreset for callers that cannot reference Genies.Avatars.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal class NativeAvatarBodyPresetInfo
#else
    public class NativeAvatarBodyPresetInfo
#endif
    {
        public string Name { get; set; }
        public string StartingBodyVariation { get; set; }
        public List<NativeAvatarBodyPresetAttribute> Attributes { get; set; }

        public NativeAvatarBodyPresetInfo()
        {
            Attributes = new List<NativeAvatarBodyPresetAttribute>();
        }

        public NativeAvatarBodyPresetInfo(string name, string startingBodyVariation, List<NativeAvatarBodyPresetAttribute> attributes)
        {
            Name = name ?? string.Empty;
            StartingBodyVariation = startingBodyVariation ?? string.Empty;
            Attributes = attributes ?? new List<NativeAvatarBodyPresetAttribute>();
        }

        internal static NativeAvatarBodyPresetInfo FromPreset(GSkelModifierPreset preset)
        {
            if (preset == null)
            {
                return null;
            }

            var attributes = preset.GSkelModValues?
                .Select(g => new NativeAvatarBodyPresetAttribute { Name = g.Name, Value = g.Value })
                .ToList() ?? new List<NativeAvatarBodyPresetAttribute>();
            return new NativeAvatarBodyPresetInfo(preset.Name, preset.StartingBodyVariation, attributes);
        }
    }

    /// <summary>
    /// Information about a color preset (either default or custom).
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal class ColorPresetInfo
#else
    public class ColorPresetInfo
#endif
    {
        public string AssetId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public List<Color> Colors { get; set; }
        public bool IsCustom { get; set; }
        public int Order { get; set; }
    }

    /// <summary>
    /// Interface for avatar color types that can be applied to the avatar.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal interface IColor
#else
    public interface IColor
#endif
    {
        Color[] Hexes { get; }
        string AssetId { get; }
        bool IsCustom { get; set; }
    }

    /// <summary>
    /// Colors that participate in user (custom) color APIs and carry an optional inventory instance ID.
    /// <see cref="EyeColor"/> is not custom-color backed and only implements <see cref="IColor"/>.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal interface ICustomColor : IColor
#else
    public interface ICustomColor : IColor
#endif
    {
        /// <summary>
        /// Entity ID for a user-created color preset, when applicable.
        /// </summary>
        string InstanceId { get; }
    }

    /// <summary>
    /// Hair color representation with base, R, G, and B components for gradient effects.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal struct HairColor : ICustomColor
#else
    public struct HairColor : ICustomColor
#endif
    {
        private readonly Color _base;
        private readonly Color _r;
        private readonly Color _g;
        private readonly Color _b;
        private readonly string _instanceId;

        /// <summary>
        /// Creates a default or user (custom) hair color based on <see cref="IsCustom"/> flag.
        /// Pass <c>true</c> for user-created / inventory custom colors; use <see cref="HairColor(Color, Color, Color, Color, string)"/> for default presets.
        /// </summary>
        public HairColor(Color baseColor, Color colorR, Color colorG, Color colorB, string instanceId = null, bool isCustom =  false)
        {
            _base = baseColor;
            _r = colorR;
            _g = colorG;
            _b = colorB;
            _instanceId = instanceId;
            _isCustom = isCustom;
            _name = null;
            _order = 0;
        }

        public Color[] Hexes => new[] { _base, _r, _g, _b };
        public string AssetId => null;
        public string InstanceId => _instanceId;

        private bool _isCustom;
        private string _name;
        private int _order;
        public bool IsCustom { get => _isCustom; set => _isCustom = value; }
        public string Name { get => _name; set => _name = value; }
        public int Order { get => _order; set => _order = value; }
    }

    /// <summary>
    /// Facial hair color representation with base, R, G, and B components for gradient effects.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal struct FacialHairColor : ICustomColor
#else
    public struct FacialHairColor : ICustomColor
#endif
    {
        private readonly Color _base;
        private readonly Color _r;
        private readonly Color _g;
        private readonly Color _b;
        private readonly string _instanceId;

        /// <summary>
        /// Creates a facial hair color with an explicit <see cref="IsCustom"/> flag.
        /// Pass <c>true</c> for user-created / inventory custom colors; use <see cref="FacialHairColor(Color, Color, Color, Color, string)"/> for default presets.
        /// </summary>
        public FacialHairColor(Color baseColor, Color colorR, Color colorG, Color colorB, string instanceId = null, bool isCustom = false)
        {
            _base = baseColor;
            _r = colorR;
            _g = colorG;
            _b = colorB;
            _instanceId = instanceId;
            _isCustom = isCustom;
            _name = null;
            _order = 0;
        }

        public Color[] Hexes => new[] { _base, _r, _g, _b };
        public string AssetId => null;
        public string InstanceId => _instanceId;

        private bool _isCustom;
        private string _name;
        private int _order;
        public bool IsCustom { get => _isCustom; set => _isCustom = value; }
        public string Name { get => _name; set => _name = value; }
        public int Order { get => _order; set => _order = value; }
    }

    /// <summary>
    /// Eyebrow color representation with two base color components.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal struct EyeBrowsColor : ICustomColor
#else
    public struct EyeBrowsColor : ICustomColor
#endif
    {
        private readonly Color _base1;
        private readonly Color _base2;
        private readonly string _instanceId;

        /// <summary>
        /// Creates an eyebrow color with <see cref="IsCustom"/> flag.
        /// Pass <c>true</c> for user-created / inventory custom colors; use <see cref="EyeBrowsColor(Color, Color, string)"/> for default presets.
        /// </summary>
        public EyeBrowsColor(Color baseColor1, Color baseColor2, string instanceId = null, bool isCustom = false)
        {
            _base1 = baseColor1;
            _base2 = baseColor2;
            _instanceId = instanceId;
            _isCustom = isCustom;
            _name = null;
            _order = 0;
        }

        public Color[] Hexes => new[] { _base1, _base2 };
        public string AssetId => null;
        public string InstanceId => _instanceId;

        private bool _isCustom;
        private string _name;
        private int _order;
        public bool IsCustom { get => _isCustom; set => _isCustom = value; }
        public string Name { get => _name; set => _name = value; }
        public int Order { get => _order; set => _order = value; }
    }

    /// <summary>
    /// Eyelash color representation with two base color components.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal struct EyeLashColor : ICustomColor
#else
    public struct EyeLashColor : ICustomColor
#endif
    {
        private readonly Color _base1;
        private readonly Color _base2;
        private readonly string _instanceId;

        /// <summary>
        /// Creates an eyelash color with an explicit <see cref="IsCustom"/> flag.
        /// Pass <c>true</c> for user-created / inventory custom colors; use <see cref="EyeLashColor(Color, Color, string)"/> for default presets.
        /// </summary>
        public EyeLashColor(Color baseColor1, Color baseColor2, string instanceId = null, bool isCustom  = false)
        {
            _base1 = baseColor1;
            _base2 = baseColor2;
            _instanceId = instanceId;
            _isCustom = isCustom;
            _name = null;
            _order = 0;
        }

        public Color[] Hexes => new[] { _base1, _base2 };
        public string AssetId => null;
        public string InstanceId => _instanceId;

        private bool _isCustom;
        private string _name;
        private int _order;
        public bool IsCustom { get => _isCustom; set => _isCustom = value; }
        public string Name { get => _name; set => _name = value; }
        public int Order { get => _order; set => _order = value; }
    }

    /// <summary>
    /// Skin color representation (single color for avatar skin).
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal struct SkinColor : ICustomColor
#else
    public struct SkinColor : ICustomColor
#endif
    {
        private readonly Color _color;
        private bool _isCustom;
        private string _name;
        private int _order;
        private readonly string _instanceId;

        /// <summary>
        /// Creates a skin color with <see cref="IsCustom"/> flag.
        /// Pass <c>true</c> for user-created / inventory custom colors; use <see cref="SkinColor(Color, string)"/> for default presets.
        /// </summary>
        public SkinColor(Color color, string instanceId = null, bool isCustom = false)
        {
            _color = color;
            _isCustom = isCustom;
            _name = null;
            _order = 0;
            _instanceId = instanceId;
        }

        public Color[] Hexes => new[] { _color };
        public string AssetId => null;
        public string InstanceId => _instanceId;
        public bool IsCustom { get => _isCustom; set => _isCustom = value; }
        public string Name { get => _name; set => _name = value; }
        public int Order { get => _order; set => _order = value; }
    }

    /// <summary>
    /// Eye color representation by asset ID (equipped via outfit/wearable).
    /// Implements IColor for use with SetColorAsync.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal struct EyeColor : IColor
#else
    public struct EyeColor : IColor
#endif
    {
        private readonly string _assetId;
        private readonly Color _base1;
        private readonly Color _base2;

        public EyeColor(string assetId, Color baseColor1, Color baseColor2)
        {
            _assetId = assetId;
            _base1 = baseColor1;
            _base2 = baseColor2;
            _isCustom = false;
            _name = null;
            _order = 0;
        }

        public Color[] Hexes => new[] { _base1, _base2 };
        public string AssetId => _assetId;

        private bool _isCustom;
        private string _name;
        private int _order;
        public bool IsCustom { get => _isCustom; set => _isCustom = value; }
        public string Name { get => _name; set => _name = value; }
        public int Order { get => _order; set => _order = value; }
    }

    /// <summary>
    /// Makeup color representation with base, R, G, and B components for gradient effects.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal struct MakeupColor : ICustomColor
#else
    public struct MakeupColor : ICustomColor
#endif
    {
        private readonly Color _base;
        private readonly Color _r;
        private readonly Color _g;
        private readonly Color _b;
        private readonly MakeupCategory _category;
        private readonly string _instanceId;

        public MakeupColor(Color baseColor, Color colorR, Color colorG, Color colorB, string instanceId = null)
            : this(MakeupCategory.None, baseColor, colorR, colorG, colorB, instanceId) { }

        public MakeupColor(MakeupCategory category, Color baseColor, Color colorR, Color colorG, Color colorB, string instanceId = null)
        {
            _category = category;
            _instanceId = instanceId;
            _base = baseColor;
            _r = colorR;
            _g = colorG;
            _b = colorB;
            _isCustom = false;
            _name = null;
            _order = 0;
        }

        public MakeupColor(Color baseColor, Color colorR, Color colorG, Color colorB, string instanceId, bool isCustom)
            : this(MakeupCategory.None, baseColor, colorR, colorG, colorB, instanceId, isCustom) { }

        /// <summary>
        /// Creates a makeup color with an explicit <see cref="IsCustom"/> flag.
        /// Pass <c>true</c> for user-created / inventory custom colors; use <see cref="MakeupColor(MakeupCategory, Color, Color, Color, Color, string)"/> for default presets.
        /// </summary>
        public MakeupColor(MakeupCategory category, Color baseColor, Color colorR, Color colorG, Color colorB, string instanceId, bool isCustom)
        {
            _category = category;
            _instanceId = instanceId;
            _base = baseColor;
            _r = colorR;
            _g = colorG;
            _b = colorB;
            _isCustom = isCustom;
            _name = null;
            _order = 0;
        }

        public MakeupCategory Category => _category;
        public Color[] Hexes => new[] { _base, _r, _g, _b };
        public string AssetId => null;
        public string InstanceId => _instanceId;

        private bool _isCustom;
        private string _name;
        private int _order;
        public bool IsCustom { get => _isCustom; set => _isCustom = value; }
        public string Name { get => _name; set => _name = value; }
        public int Order { get => _order; set => _order = value; }
    }

    /// <summary>
    /// Maps AvatarFeatureStat to body attribute IDs and converts between stat-specific enums.
    /// </summary>
    internal static class AvatarFeatureStatMapping
    {
        internal static AvatarFeatureStat ToAvatarFeatureStat(BodyStats s) => s switch
        {
            BodyStats.NeckThickness => AvatarFeatureStat.Body_NeckThickness,
            BodyStats.ShoulderBroadness => AvatarFeatureStat.Body_ShoulderBroadness,
            BodyStats.ChestBustline => AvatarFeatureStat.Body_ChestBustline,
            BodyStats.ArmsThickness => AvatarFeatureStat.Body_ArmsThickness,
            BodyStats.WaistThickness => AvatarFeatureStat.Body_WaistThickness,
            BodyStats.BellyFullness => AvatarFeatureStat.Body_BellyFullness,
            BodyStats.HipsThickness => AvatarFeatureStat.Body_HipsThickness,
            BodyStats.LegsThickness => AvatarFeatureStat.Body_LegsThickness,
            BodyStats.HipSize => AvatarFeatureStat.Body_HipSize,
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
        };

        internal static AvatarFeatureStat ToAvatarFeatureStat(EyeBrowsStats s) => s switch
        {
            EyeBrowsStats.Thickness => AvatarFeatureStat.EyeBrows_Thickness,
            EyeBrowsStats.Length => AvatarFeatureStat.EyeBrows_Length,
            EyeBrowsStats.VerticalPosition => AvatarFeatureStat.EyeBrows_VerticalPosition,
            EyeBrowsStats.Spacing => AvatarFeatureStat.EyeBrows_Spacing,
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
        };

        internal static AvatarFeatureStat ToAvatarFeatureStat(EyeStats s) => s switch
        {
            EyeStats.Size => AvatarFeatureStat.Eyes_Size,
            EyeStats.VerticalPosition => AvatarFeatureStat.Eyes_VerticalPosition,
            EyeStats.Spacing => AvatarFeatureStat.Eyes_Spacing,
            EyeStats.Rotation => AvatarFeatureStat.Eyes_Rotation,
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
        };

        internal static AvatarFeatureStat ToAvatarFeatureStat(JawStats s) => s switch
        {
            JawStats.Width => AvatarFeatureStat.Jaw_Width,
            JawStats.Length => AvatarFeatureStat.Jaw_Length,
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
        };

        internal static AvatarFeatureStat ToAvatarFeatureStat(LipsStats s) => s switch
        {
            LipsStats.Width => AvatarFeatureStat.Lips_Width,
            LipsStats.Fullness => AvatarFeatureStat.Lips_Fullness,
            LipsStats.VerticalPosition => AvatarFeatureStat.Lips_VerticalPosition,
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
        };

        internal static AvatarFeatureStat ToAvatarFeatureStat(NoseStats s) => s switch
        {
            NoseStats.Width => AvatarFeatureStat.Nose_Width,
            NoseStats.Length => AvatarFeatureStat.Nose_Length,
            NoseStats.VerticalPosition => AvatarFeatureStat.Nose_VerticalPosition,
            NoseStats.Tilt => AvatarFeatureStat.Nose_Tilt,
            NoseStats.Projection => AvatarFeatureStat.Nose_Projection,
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
        };

        /// <summary>
        /// Returns the body attribute ID string for the given AvatarFeatureStat.
        /// </summary>
        internal static string GetAttributeId(AvatarFeatureStat stat) => stat switch
        {
            AvatarFeatureStat.Body_NeckThickness => GenieBodyAttribute.WeightHeadNeck,
            AvatarFeatureStat.Body_ShoulderBroadness => GenieBodyAttribute.ShoulderSize,
            AvatarFeatureStat.Body_ChestBustline => GenieBodyAttribute.WeightUpperTorso,
            AvatarFeatureStat.Body_ArmsThickness => GenieBodyAttribute.WeightArms,
            AvatarFeatureStat.Body_WaistThickness => GenieBodyAttribute.Waist,
            AvatarFeatureStat.Body_BellyFullness => GenieBodyAttribute.Belly,
            AvatarFeatureStat.Body_HipsThickness => GenieBodyAttribute.WeightLowerTorso,
            AvatarFeatureStat.Body_LegsThickness => GenieBodyAttribute.WeightLegs,
            AvatarFeatureStat.Body_HipSize => GenieBodyAttribute.HipSize,
            AvatarFeatureStat.EyeBrows_Thickness => GenieBodyAttribute.BrowThickness,
            AvatarFeatureStat.EyeBrows_Length => GenieBodyAttribute.BrowLength,
            AvatarFeatureStat.EyeBrows_VerticalPosition => GenieBodyAttribute.BrowPositionVert,
            AvatarFeatureStat.EyeBrows_Spacing => GenieBodyAttribute.BrowSpacing,
            AvatarFeatureStat.Eyes_Size => GenieBodyAttribute.EyeSize,
            AvatarFeatureStat.Eyes_VerticalPosition => GenieBodyAttribute.EyePositionVert,
            AvatarFeatureStat.Eyes_Spacing => GenieBodyAttribute.EyeSpacing,
            AvatarFeatureStat.Eyes_Rotation => GenieBodyAttribute.EyeTilt,
            AvatarFeatureStat.Jaw_Width => GenieBodyAttribute.JawWidth,
            AvatarFeatureStat.Jaw_Length => GenieBodyAttribute.JawLength,
            AvatarFeatureStat.Lips_Width => GenieBodyAttribute.LipWidth,
            AvatarFeatureStat.Lips_Fullness => GenieBodyAttribute.LipFullness,
            AvatarFeatureStat.Lips_VerticalPosition => GenieBodyAttribute.LipPositionVert,
            AvatarFeatureStat.Nose_Width => GenieBodyAttribute.NoseWidth,
            AvatarFeatureStat.Nose_Length => GenieBodyAttribute.NoseHeight,
            AvatarFeatureStat.Nose_VerticalPosition => GenieBodyAttribute.NosePositionVert,
            AvatarFeatureStat.Nose_Tilt => GenieBodyAttribute.NoseTilt,
            AvatarFeatureStat.Nose_Projection => GenieBodyAttribute.NoseProjection,
            _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, null)
        };
    }
}

