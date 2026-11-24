using System;
using System.Collections.Generic;
using Genies.Avatars.Customization;
using Genies.Refs;
using Genies.Services.Model;
using UnityEngine;

namespace Genies.Sdk
{
    /// <summary>
    /// Wrapper for wearable asset information
    /// </summary>
    [Serializable]
    public class WearableAssetInfo : IDisposable
    {
        private readonly string _assetId;
        private readonly string _name;
        private readonly string _category;
        private readonly Sprite _icon;
        private readonly Action _onDisposed;

        public string AssetId { get => _assetId; }
        public string Name { get => _name; }
        public string Category { get => _category; }
        public Sprite Icon { get => _icon; }

        public WearableAssetInfo(string assetId, string name, string category, Sprite icon, Action onDisposed)
        {
            _assetId = assetId;
            _name = name;
            _category = category;
            _icon = icon;
            _onDisposed = onDisposed;
        }

        public void Dispose()
        {
            _onDisposed?.Invoke();
        }

        /// <summary>
        /// Converts SDK WearableAssetInfo to internal AvatarEditor Core WearableAssetInfo type.
        /// </summary>
        internal static Genies.Avatars.Customization.WearableAssetInfo ToInternal(WearableAssetInfo sdkAsset)
        {
            var ret = new Genies.Avatars.Customization.WearableAssetInfo();
            ret.AssetId = sdkAsset.AssetId;
            ret.Name = sdkAsset.Name;
            ret.Category = sdkAsset.Category;
            // When Core disposes ret.Icon (RemoveSpriteReference), run the SDK's onDisposed callback.
            ret.Icon = sdkAsset.Icon != null
                ? CreateRef.FromAny(sdkAsset.Icon, _ => sdkAsset._onDisposed?.Invoke())
                : default;

            return ret;
        }

        /// <summary>
        /// Creates from the internal AvatarEditor WearableAssetInfo type
        /// </summary>
        internal static WearableAssetInfo FromInternal(Genies.Avatars.Customization.WearableAssetInfo internalAsset)
        {
            return new WearableAssetInfo(
                internalAsset.AssetId,
                internalAsset.Name,
                internalAsset.Category,
                internalAsset.Icon.Item,
                internalAsset.Dispose
            );
        }

        /// <summary>
        /// Converts a list from internal types to SDK types
        /// </summary>
        internal static List<WearableAssetInfo> FromInternalList(List<Genies.Avatars.Customization.WearableAssetInfo> internalList)
        {
            var result = new List<WearableAssetInfo>();
            foreach (var item in internalList)
            {
                result.Add(FromInternal(item));
            }

            return result;
        }
    }

    /// <summary>
    /// Wrapper for default avatar base asset information (avatar features). Converts to/from Genies.Avatars.Customization.AvatarFeaturesInfo.
    /// </summary>
    [Serializable]
    public class AvatarFeaturesInfo : IDisposable
    {
        private readonly string _assetId;
        private readonly List<string> _subCategories;
        private readonly Sprite _icon;
        private readonly Action _onDisposed;

        public string AssetId { get => _assetId; }
        public List<string> SubCategories { get => _subCategories; }
        public Sprite Icon { get => _icon; }

        public AvatarFeaturesInfo(string assetId, List<string> subCategories, Sprite icon, Action onDisposed)
        {
            _assetId = assetId;
            _subCategories = subCategories;
            _icon = icon;
            _onDisposed = onDisposed;
        }

        public void Dispose()
        {
            _onDisposed?.Invoke();
        }

        /// <summary>
        /// Creates from the internal Core AvatarFeaturesInfo type.
        /// </summary>
        internal static AvatarFeaturesInfo FromInternal(Genies.Avatars.Customization.AvatarFeaturesInfo internalAsset)
        {
            return new AvatarFeaturesInfo(
                internalAsset.AssetId,
                internalAsset.SubCategories,
                internalAsset.Icon.Item,
                internalAsset.Dispose
            );
        }

        /// <summary>
        /// Converts a list from internal Core AvatarFeaturesInfo to SDK AvatarFeaturesInfo.
        /// </summary>
        internal static List<AvatarFeaturesInfo> FromInternalList(List<Genies.Avatars.Customization.AvatarFeaturesInfo> internalList)
        {
            var result = new List<AvatarFeaturesInfo>();
            foreach (var item in internalList)
            {
                result.Add(FromInternal(item));
            }

            return result;
        }

        /// <summary>
        /// Converts this SDK type to the internal Core AvatarFeaturesInfo type.
        /// </summary>
        internal static Genies.Avatars.Customization.AvatarFeaturesInfo ToInternal(AvatarFeaturesInfo sdkAsset)
        {
            if (sdkAsset == null)
            {
                return null;
            }

            return new Genies.Avatars.Customization.AvatarFeaturesInfo
            {
                AssetId = sdkAsset.AssetId,
                SubCategories = sdkAsset.SubCategories
            };
        }

        /// <summary>
        /// Converts a list from SDK AvatarFeaturesInfo to internal Core AvatarFeaturesInfo.
        /// </summary>
        internal static List<Genies.Avatars.Customization.AvatarFeaturesInfo> ToInternal(List<AvatarFeaturesInfo> sdkList)
        {
            if (sdkList == null)
            {
                return null;
            }

            var result = new List<Genies.Avatars.Customization.AvatarFeaturesInfo>(sdkList.Count);
            foreach (var item in sdkList)
            {
                var internalItem = ToInternal(item);
                if (internalItem != null)
                {
                    result.Add(internalItem);
                }
            }
            return result;
        }
    }

    /// <summary>
    /// Wrapper for default asset information from the inventory service (e.g. makeup, tattoos). Converts to/from Genies.Avatars.Customization.AvatarItemInfo.
    /// </summary>
    [Serializable]
    public class AvatarMakeupInfo : IDisposable
    {
        private readonly string _assetId;
        private readonly Sprite _icon;
        private readonly Action _onDisposed;

        public string AssetId { get => _assetId; }
        public Sprite Icon { get => _icon; }

        public void Dispose()
        {
            _onDisposed?.Invoke();
        }

        public AvatarMakeupInfo(string assetId, Sprite icon, Action onDisposed)
        {
            _assetId = assetId;
            _icon = icon;
            _onDisposed = onDisposed;
        }

        /// <summary>
        /// Converts this SDK type to the internal Core AvatarItemInfo type.
        /// </summary>
        internal static Genies.Avatars.Customization.AvatarMakeupInfo ToInternal(AvatarMakeupInfo item)
        {
            if (item == null)
            {
                return null;
            }

            return new Genies.Avatars.Customization.AvatarMakeupInfo
            {
                AssetId = item.AssetId
            };
        }

        /// <summary>
        /// Creates from the internal Core AvatarItemInfo type.
        /// </summary>
        internal static AvatarMakeupInfo FromInternal(Genies.Avatars.Customization.AvatarMakeupInfo internalAsset)
        {
            return new AvatarMakeupInfo(
                internalAsset.AssetId,
                internalAsset.Icon.Item,
                internalAsset.Dispose
            );
        }

        /// <summary>
        /// Converts a list from internal Core AvatarItemInfo to SDK AvatarItemInfo.
        /// </summary>
        internal static List<AvatarMakeupInfo> FromInternalList(List<Genies.Avatars.Customization.AvatarMakeupInfo> internalList)
        {
            var result = new List<AvatarMakeupInfo>();
            if (internalList == null)
            {
                return result;
            }

            foreach (var item in internalList)
            {
                result.Add(FromInternal(item));
            }
            return result;
        }
    }

    /// <summary>
    /// Wrapper for default asset information from the inventory service (e.g. makeup, tattoos). Converts to/from Genies.Avatars.Customization.AvatarItemInfo.
    /// </summary>
    [Serializable]
    public class AvatarTattooInfo : IDisposable
    {
        private readonly string _assetId;
        private readonly Sprite _icon;
        private readonly Action _onDisposed;

        public string AssetId { get => _assetId; }
        public Sprite Icon { get => _icon; }

        public void Dispose()
        {
            _onDisposed?.Invoke();
        }

        public AvatarTattooInfo(string assetId, Sprite icon, Action onDisposed)
        {
            _assetId = assetId;
            _icon = icon;
            _onDisposed = onDisposed;
        }

        /// <summary>
        /// Converts this SDK type to the internal Core AvatarItemInfo type.
        /// </summary>
        internal static Genies.Avatars.Customization.AvatarTattooInfo ToInternal(AvatarTattooInfo item)
        {
            if (item == null)
            {
                return null;
            }

            return new Genies.Avatars.Customization.AvatarTattooInfo
            {
                AssetId = item.AssetId
            };
        }

        /// <summary>
        /// Creates from the internal Core AvatarItemInfo type.
        /// </summary>
        internal static AvatarTattooInfo FromInternal(Genies.Avatars.Customization.AvatarTattooInfo internalAsset)
        {
            return new AvatarTattooInfo(
                internalAsset.AssetId,
                internalAsset.Icon.Item,
                internalAsset.Dispose
            );
        }

        /// <summary>
        /// Converts a list from internal Core AvatarItemInfo to SDK AvatarItemInfo.
        /// </summary>
        internal static List<AvatarTattooInfo> FromInternalList(List<Genies.Avatars.Customization.AvatarTattooInfo> internalList)
        {
            var result = new List<AvatarTattooInfo>();
            if (internalList == null)
            {
                return result;
            }

            foreach (var item in internalList)
            {
                result.Add(FromInternal(item));
            }
            return result;
        }
    }

    /// <summary>
    /// Single attribute (name/value) in a native avatar body preset. Mirrors GSkelModValue for SDK use.
    /// </summary>
    [Serializable]
    public struct NativeAvatarBodyPresetAttribute
    {
        private string _name;
        private float _valueField;

        public NativeAvatarBodyPresetAttribute(string name, float value)
        {
            _name = name;
            _valueField = value;
        }

        public string Name { get => _name; }
        public float Value { get => _valueField; }
    }

    /// <summary>
    /// Native avatar body preset data returned by GetNativeAvatarBodyPresetAsync. Mirrors GSkelModifierPreset for SDK use.
    /// </summary>
    [Serializable]
    public class NativeAvatarBodyPresetInfo
    {
        private readonly string _name;
        private readonly string _startingBodyVariation;
        private readonly List<NativeAvatarBodyPresetAttribute> _attributes;

        public string Name { get => _name; }
        public string StartingBodyVariation { get => _startingBodyVariation; }
        public List<NativeAvatarBodyPresetAttribute> Attributes { get => _attributes; }

        public NativeAvatarBodyPresetInfo(string name, string startingBodyVariation, List<NativeAvatarBodyPresetAttribute> attributes)
        {
            _name = name;
            _startingBodyVariation = startingBodyVariation;
            _attributes = attributes ?? new List<NativeAvatarBodyPresetAttribute>();
        }

        internal static class NativeAvatarBodyPresetInfoMapper
        {
            internal static NativeAvatarBodyPresetInfo FromInternal(Genies.Avatars.Customization.NativeAvatarBodyPresetInfo core)
            {
                if (core == null)
                {
                    return null;
                }

                var attributes = core.Attributes?.ConvertAll(a => new NativeAvatarBodyPresetAttribute(a.Name, a.Value))
                                 ?? new List<NativeAvatarBodyPresetAttribute>();
                return new NativeAvatarBodyPresetInfo(core.Name, core.StartingBodyVariation, attributes);
            }

            internal static Genies.Avatars.Customization.NativeAvatarBodyPresetInfo ToInternal(NativeAvatarBodyPresetInfo sdk)
            {
                if (sdk == null)
                {
                    return null;
                }

                var attributes = sdk.Attributes?.ConvertAll(a => new Genies.Avatars.Customization.NativeAvatarBodyPresetAttribute { Name = a.Name, Value = a.Value })
                                 ?? new List<Genies.Avatars.Customization.NativeAvatarBodyPresetAttribute>();
                return new Genies.Avatars.Customization.NativeAvatarBodyPresetInfo(sdk.Name, sdk.StartingBodyVariation, attributes);
            }
        }
    }

    /// <summary>
    /// User (custom) color information returned by GetUserColorsByCategoryAsync. Mirrors the inventory service custom color response for SDK use.
    /// </summary>
    [Serializable]
    public class UserColorInfo
    {
        /// <summary>Unique ID of the custom color instance.</summary>
        public string InstanceId { get; }
        /// <summary>Base asset ID (e.g., custom-hair-color-base).</summary>
        public string AssetId { get; }
        /// <summary>Category of the custom color (e.g. hair, skin, flair, eyebrow, eyelash).</summary>
        public string Category { get; }
        /// <summary>Array of hex color values.</summary>
        public List<string> ColorsHex { get; }
        /// <summary>Display name of the custom color.</summary>
        public string Name { get; }
        /// <summary>Application ID.</summary>
        public string AppId { get; }
        /// <summary>Organization ID.</summary>
        public string OrgId { get; }
        /// <summary>User ID who owns the custom color.</summary>
        public string OwnerId { get; }
        /// <summary>Creation timestamp.</summary>
        public string DateCreated { get; }
        /// <summary>Color values parsed from ColorsHex (Unity Engine.Color). Empty if hex parsing failed.</summary>
        public List<Color> Colors { get; }

        public UserColorInfo(
            string instanceId,
            string assetId,
            string category,
            List<string> colorsHex,
            string name,
            string appId,
            string orgId,
            string ownerId,
            string dateCreated)
        {
            InstanceId = instanceId ?? string.Empty;
            AssetId = assetId ?? string.Empty;
            Category = category ?? string.Empty;
            ColorsHex = colorsHex ?? new List<string>();
            Name = name ?? string.Empty;
            AppId = appId ?? string.Empty;
            OrgId = orgId ?? string.Empty;
            OwnerId = ownerId ?? string.Empty;
            DateCreated = dateCreated ?? string.Empty;
            Colors = ParseColorsHex(ColorsHex);
        }

        private static List<Color> ParseColorsHex(List<string> colorsHex)
        {
            var list = new List<Color>();
            if (colorsHex == null)
            {
                return list;
            }

            foreach (var hex in colorsHex)
            {
                if (!string.IsNullOrEmpty(hex) && ColorUtility.TryParseHtmlString(hex, out Color color))
                {
                    list.Add(color);
                }
            }
            return list;
        }

        /// <summary>
        /// Converts a list of inventory service CustomColorResponse to SDK UserColorInfo.
        /// </summary>
        internal static List<UserColorInfo> FromCustomColorResponseList(List<Genies.Services.Model.CustomColorResponse> list)
        {
            if (list == null)
            {
                return new List<UserColorInfo>();
            }

            var result = new List<UserColorInfo>(list.Count);
            foreach (var c in list)
            {
                result.Add(new UserColorInfo(
                    c.InstanceId,
                    c.AssetId,
                    c.Category,
                    c.ColorsHex,
                    c.Name,
                    c.AppId,
                    c.OrgId,
                    c.OwnerId,
                    c.DateCreated));
            }
            return result;
        }
    }

    /// <summary>
    /// Configuration for avatar headshot screenshot capture. Maps to <see cref="Genies.AvatarEditor.Core.ScreenshotConfig"/>.
    /// </summary>
    public struct ScreenshotConfig
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

        internal static Genies.Avatars.Customization.ScreenshotConfig ToInternal(ScreenshotConfig sdk)
        {
            return new Genies.Avatars.Customization.ScreenshotConfig
            {
                Width = sdk.Width,
                Height = sdk.Height,
                TransparentBackground = sdk.TransparentBackground,
                Msaa = sdk.Msaa,
                FieldOfView = sdk.FieldOfView,
                HeadRadiusMeters = sdk.HeadRadiusMeters,
                ForwardDistance = sdk.ForwardDistance,
                CameraUpOffset = sdk.CameraUpOffset
            };
        }
    }

    /// <summary>
    /// Public interface for avatar color types used with SetColorAsync and returned by GetColorAsync.
    /// Use CreateHairColor, CreateSkinColor, etc. to build instances.
    /// </summary>
    public interface IAvatarColor
    {
        AvatarColorKind Kind { get; }
        Color[] Hexes { get; }
        string AssetId { get; }
        bool IsCustom { get; }
    }

    /// <summary>
    /// Avatar colors that can carry an inventory instance id (e.g. user custom presets).
    /// <see cref="EyeColor"/> implements only <see cref="IAvatarColor"/> (eye color is asset-based).
    /// </summary>
    public interface IAvatarCustomColor : IAvatarColor
    {
        /// <summary>
        /// Instance id for a user-created color preset when applicable; otherwise null.
        /// </summary>
        string InstanceId { get; }
    }

    /// <summary>
    /// Hair color (base + R,G,B gradient). Use CreateHairColor or GetColorAsync(AvatarColorKind.Hair).
    /// </summary>
    public struct HairColor : IAvatarCustomColor
    {
        private readonly Color _base;
        private readonly Color _r, _g, _b;
        private readonly string _instanceId;
        private readonly bool _isCustom;
        public HairColor(Color baseColor, Color colorR, Color colorG, Color colorB, string instanceId = null, bool isCustom = false) { _base = baseColor; _r = colorR; _g = colorG; _b = colorB; _instanceId = instanceId; _isCustom = isCustom; }
        public AvatarColorKind Kind => AvatarColorKind.Hair;
        public Color[] Hexes => new[] { _base, _r, _g, _b };
        public string AssetId => null;
        public bool IsCustom => _isCustom;
        public string InstanceId => _instanceId;
    }

    /// <summary>
    /// Facial hair color (base + R,G,B gradient). Use CreateFacialHairColor or GetColorAsync(AvatarColorKind.FacialHair).
    /// </summary>
    public struct FacialHairColor : IAvatarCustomColor
    {
        private readonly Color _base;
        private readonly Color _r, _g, _b;
        private readonly string _instanceId;
        private readonly bool _isCustom;
        public FacialHairColor(Color baseColor, Color colorR, Color colorG, Color colorB, string instanceId = null, bool isCustom = false) { _base = baseColor; _r = colorR; _g = colorG; _b = colorB; _instanceId = instanceId; _isCustom = isCustom; }
        public AvatarColorKind Kind => AvatarColorKind.FacialHair;
        public Color[] Hexes => new[] { _base, _r, _g, _b };
        public string AssetId => null;
        public bool IsCustom => _isCustom;
        public string InstanceId => _instanceId;
    }

    /// <summary>
    /// Eyebrow color (base + R,G,B). Use CreateEyeBrowsColor or GetColorAsync(AvatarColorKind.EyeBrows).
    /// </summary>
    public struct EyeBrowsColor : IAvatarCustomColor
    {
        private readonly Color _base;
        private readonly Color _base2;
        private readonly string _instanceId;
        private readonly bool _isCustom;
        public EyeBrowsColor(Color baseColor, Color baseColor2, string instanceId = null, bool isCustom = false) { _base = baseColor; _base2 = baseColor2; _instanceId = instanceId; _isCustom = isCustom; }
        public AvatarColorKind Kind => AvatarColorKind.EyeBrows;
        public Color[] Hexes => new[] { _base, _base2 };
        public string AssetId => null;
        public bool IsCustom => _isCustom;
        public string InstanceId => _instanceId;
    }

    /// <summary>
    /// Eyelash color (base + R,G,B). Use CreateEyeLashColor or GetColorAsync(AvatarColorKind.EyeLash).
    /// </summary>
    public struct EyeLashColor : IAvatarCustomColor
    {
        private readonly Color _base;
        private readonly Color _base2;
        private readonly string _instanceId;
        private bool _isCustom;
        public EyeLashColor(Color baseColor, Color baseColor2, string instanceId = null, bool isCustom = false) { _base = baseColor; _base2 = baseColor2; _instanceId = instanceId; _isCustom = isCustom; }
        public AvatarColorKind Kind => AvatarColorKind.EyeLash;
        public Color[] Hexes => new[] { _base, _base2 };
        public string AssetId => null;
        public bool IsCustom => _isCustom;
        public string InstanceId => _instanceId;
    }

    /// <summary>
    /// Skin color (single color). Use CreateSkinColor or GetColorAsync(AvatarColorKind.Skin).
    /// </summary>
    public struct SkinColor : IAvatarCustomColor
    {
        private readonly Color _color;
        private readonly string _instanceId;
        private bool _isCustom;
        public SkinColor(Color color, string instanceId = null, bool isCustom = false) { _color = color; _instanceId = instanceId; _isCustom = isCustom; }
        public AvatarColorKind Kind => AvatarColorKind.Skin;
        public Color[] Hexes => new[] { _color };
        public string AssetId => null;
        public bool IsCustom => _isCustom;
        public string InstanceId => _instanceId;
    }

    /// <summary>
    /// Eye color by asset ID (equipped via outfit). Use CreateEyeColor or GetColorAsync(AvatarColorKind.Eyes).
    /// </summary>
    public struct EyeColor : IAvatarColor
    {
        private readonly string _assetId;
        private readonly Color _base1;
        private readonly Color _base2;
        private bool _isCustom;
        public EyeColor(string assetId, Color baseColor1, Color baseColor2, bool isCustom = false) { _assetId = assetId; _base1 = baseColor1; _base2 = baseColor2; _isCustom = isCustom; }
        public AvatarColorKind Kind => AvatarColorKind.Eyes;
        public Color[] Hexes => new[] { _base1, _base2 };
        public string AssetId => _assetId;
        public bool IsCustom => _isCustom;
    }

    /// <summary>
    /// Makeup color (base + R,G,B gradient). Use MakeupColor or GetColorAsync(AvatarColorKind.Makeup).
    /// </summary>
    public struct MakeupColor : IAvatarCustomColor
    {
        private readonly Color _base;
        private readonly Color _r, _g, _b;
        private readonly AvatarMakeupCategory _category;
        private readonly string _instanceId;
        private bool _isCustom;
        public MakeupColor(Color baseColor, Color colorR, Color colorG, Color colorB, string instanceId = null, bool isCustom = false)
            : this(AvatarMakeupCategory.Stickers, baseColor, colorR, colorG, colorB, instanceId, isCustom) { }

        public MakeupColor(AvatarMakeupCategory category, Color baseColor, Color colorR, Color colorG, Color colorB, string instanceId = null, bool isCustom = false)
        {
            _category = category;
            _base = baseColor;
            _r = colorR;
            _g = colorG;
            _b = colorB;
            _instanceId = instanceId;
            _isCustom = isCustom;
        }

        /// <summary>
        /// Creates a MakeupColor from an AvatarColorKind (e.g. MakeupLipstick, MakeupBlush). Uses AvatarColorKindMakeupCategoryMapper to get the category.
        /// </summary>
        public MakeupColor(AvatarColorKind colorKind, Color baseColor, Color colorR, Color colorG, Color colorB, string instanceId = null, bool isCustom = false)
            : this(AvatarColorKindMakeupCategoryMapper.ToMakeupCategory(colorKind), baseColor, colorR, colorG, colorB, instanceId, isCustom)
        {
        }

        /// <summary>
        /// The makeup category this color applies to (e.g. Lipstick, Blush, Eyeshadow).
        /// </summary>
        public AvatarMakeupCategory Category => _category;

        /// <summary>
        /// The color kind for this makeup (e.g. MakeupLipstick, MakeupBlush). Derived from Category via AvatarColorKindMakeupCategoryMapper.
        /// </summary>
        public AvatarColorKind Kind => AvatarColorKindMakeupCategoryMapper.ToAvatarColorKind(_category);
        public Color[] Hexes => new[] { _base, _r, _g, _b };
        public string AssetId => null;
        public bool IsCustom => false;
        public string InstanceId => _instanceId;
    }

    /// <summary>
    /// Converts ColorType and color data into an IAvatarColor suitable for SetColorAsync.
    /// If colors is null or empty, returns an IAvatarColor with clear color (Color.clear).
    /// </summary>
    public static class ColorMapper
    {
        /// <summary>
        /// Converts a ColorType and list of color values into an IAvatarColor instance suitable for SetColorAsync.
        /// </summary>
        /// <param name="colorType">The color kind (Eyes, Hair, FacialHair, Skin, Eyebrow, Eyelash). Makeup is not supported and will throw.</param>
        /// <param name="colors">Color values: one for Skin, four (base, R, G, B) for Hair/FacialHair/Eyebrow/Eyelash. If null or empty, a clear IAvatarColor is returned.</param>
        /// <param name="assetId">Required when colorType is Eyes and colors is non-empty. When colors is empty, Eyes returns EyeColor with empty assetId. Ignored for other types.</param>
        /// <returns>An IAvatarColor of the appropriate concrete type.</returns>
        public static IAvatarColor ToIColorValue(ColorType colorType, List<Color> colors, string assetId = null)
        {
            bool isEmpty = colors == null || colors.Count == 0;
            Color clear = Color.clear;

            switch (colorType)
            {
                case ColorType.Skin:
                    return new SkinColor(isEmpty ? clear : colors[0]);

                case ColorType.Hair:
                case ColorType.FacialHair:
                case ColorType.Eyebrow:
                case ColorType.Eyelash:
                case ColorType.Eyes:
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
                        if (colorType == ColorType.Eyes)
                        {
                            return new EyeColor(assetId ?? string.Empty, c0, c1);
                        }

                        if (colorType == ColorType.Hair)
                        {
                            return new HairColor(c0, c1, c2, c3);
                        }

                        if (colorType == ColorType.FacialHair)
                        {
                            return new FacialHairColor(c0, c1, c2, c3);
                        }

                        if (colorType == ColorType.Eyebrow)
                        {
                            return new EyeBrowsColor(c0, c1);
                        }

                        if (colorType == ColorType.Eyelash)
                        {
                            return new EyeLashColor(c0, c1);
                        }

                        return new MakeupColor(c0, c1, c2, c3);
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(colorType), colorType, "Unsupported ColorType.");
            }
        }
    }

    /// <summary>
    /// Asset types available in the avatar system
    /// </summary>
    public enum AssetType
    {
        WardrobeGear = 0,
        AvatarBase = 1,
        AvatarMakeup = 2,
        Flair = 3,
        AvatarEyes = 4,
        ColorPreset = 5,
        ImageLibrary = 6,
        AnimationLibrary = 7,
        Avatar = 8,
        Decor = 9,
        ModelLibrary = 10
    }

    /// <summary>
    /// Avatar feature category for GetDefaultAvatarFeaturesByCategory (e.g. Eyes, Jaw, Lips, Nose). Mirrors Genies.Inventory.AvatarBaseCategory for SDK use.
    /// </summary>
    public enum AvatarFeatureCategory
    {
        Lips = 1,
        Jaw = 2,
        Nose = 3,
        Eyes = 4,
        All = 999
    }

    /// <summary>
    /// Makeup category for GetDefaultMakeupByCategoryAsync (e.g. Stickers, Lipstick, Freckles, FaceGems, Eyeshadow, Blush). Wrapper for Genies.Avatars.Customization.MakeupCategory for SDK use.
    /// </summary>
    public enum AvatarMakeupCategory
    {
        Stickers = 0,
        Lipstick = 1,
        Freckles = 2,
        FaceGems = 3,
        Eyeshadow = 4,
        Blush = 5,
        All = 999,
    }

    /// <summary>
    /// Maps between AvatarColorKind (makeup variants) and AvatarMakeupCategory for use in MakeupColor and color APIs.
    /// </summary>
    public static class AvatarColorKindMakeupCategoryMapper
    {
        /// <summary>
        /// Converts AvatarColorKind to AvatarMakeupCategory when the kind is a makeup kind (MakeupStickers, MakeupLipstick, etc.).
        /// Returns AvatarMakeupCategory.None for non-makeup kinds.
        /// </summary>
        public static AvatarMakeupCategory ToMakeupCategory(AvatarColorKind colorKind)
        {
            return colorKind switch
            {
                AvatarColorKind.MakeupStickers => AvatarMakeupCategory.Stickers,
                AvatarColorKind.MakeupLipstick => AvatarMakeupCategory.Lipstick,
                AvatarColorKind.MakeupFreckles => AvatarMakeupCategory.Freckles,
                AvatarColorKind.MakeupFaceGems => AvatarMakeupCategory.FaceGems,
                AvatarColorKind.MakeupEyeshadow => AvatarMakeupCategory.Eyeshadow,
                AvatarColorKind.MakeupBlush => AvatarMakeupCategory.Blush,
                _ => AvatarMakeupCategory.Stickers
            };
        }


        /// <summary>
        /// Converts AvatarMakeupCategory to the corresponding AvatarColorKind for use with SetColorAsync/GetColorAsync.
        /// Returns AvatarColorKind.MakeupLipstick for None (default makeup kind).
        /// </summary>
        public static AvatarColorKind ToAvatarColorKind(AvatarMakeupCategory category)
        {
            return category switch
            {
                AvatarMakeupCategory.Stickers => AvatarColorKind.MakeupStickers,
                AvatarMakeupCategory.Lipstick => AvatarColorKind.MakeupLipstick,
                AvatarMakeupCategory.Freckles => AvatarColorKind.MakeupFreckles,
                AvatarMakeupCategory.FaceGems => AvatarColorKind.MakeupFaceGems,
                AvatarMakeupCategory.Eyeshadow => AvatarColorKind.MakeupEyeshadow,
                AvatarMakeupCategory.Blush => AvatarColorKind.MakeupBlush,
                _ => AvatarColorKind.MakeupLipstick
            };
        }
    }

    /// <summary>
    /// Gender types for avatar body configuration
    /// </summary>
    public enum GenderType
    {
        Male,
        Female,
        Androgynous
    }

    /// <summary>
    /// Body size types for avatar body configuration
    /// </summary>
    public enum BodySize
    {
        Skinny,
        Medium,
        Heavy
    }

    /// <summary>
    /// Wardrobe subcategory types for filtering wearable assets
    /// </summary>
    public enum WardrobeSubcategory
    {
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
    /// Wearable category types (non-hair wardrobe subcategories). Excludes hair, eyebrows, eyelashes, facialHair.
    /// </summary>
    public enum WearablesCategory
    {
        Hoodie,
        Shirt,
        Dress,
        Pants,
        Shorts,
        Skirt,
        Shoes,
        Earrings,
        Glasses,
        Hat,
        Mask,
        Jacket,
        All = 999
    }

    /// <summary>
    /// Subset of WearablesCategory for user wearable asset APIs (e.g. GetUserWearablesByCategoryAsync).
    /// Maps to WearablesCategory when calling internal APIs.
    /// </summary>
    public enum UserWearablesCategory
    {
        Hoodie,
        Shirt,
        Pants,
        Shorts,
        Jacket,
        Skirt,
        Dress,
        Shoes,
        All = 999
    }

    /// <summary>
    /// Eyebrow statistics that can be modified on the avatar
    /// </summary>
    public enum EyeBrowsStats
    {
        Thickness,          // Thickness of the eyebrows
        Length,             // Length of the eyebrows
        VerticalPosition,   // Vertical position of the eyebrows
        Spacing             // Spacing between eyebrows
    }

    /// <summary>
    /// Eye statistics that can be modified on the avatar
    /// </summary>
    public enum EyeStats
    {
        Size,               // Size of the eyes
        VerticalPosition,   // Vertical position of the eyes
        Spacing,            // Spacing between eyes
        Rotation            // Rotation of the eyes
    }

    /// <summary>
    /// Jaw statistics that can be modified on the avatar
    /// </summary>
    public enum JawStats
    {
        Width,   // Width of the jaw
        Length   // Length of the jaw
    }

    /// <summary>
    /// Lip statistics that can be modified on the avatar
    /// </summary>
    public enum LipsStats
    {
        Width,             // Width of the lips
        Fullness,          // Fullness/thickness of the lips
        VerticalPosition   // Vertical position of the lips
    }

    /// <summary>
    /// Nose statistics that can be modified on the avatar
    /// </summary>
    public enum NoseStats
    {
        Width,             // Width of the nose
        Length,            // Length of the nose
        VerticalPosition,  // Vertical position of the nose
        Tilt,              // Tilt/angle of the nose
        Projection         // Projection/protrusion of the nose
    }

    /// <summary>
    /// Body statistics that can be modified on the avatar
    /// </summary>
    public enum BodyStats
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
    /// Public interface for avatar feature stats used with ModifyAvatarFeatureStatsAsync. Use CreateEyeBrowsStat, CreateEyeStat, etc. to build instances.
    /// </summary>
    public interface IAvatarFeatureStat
    {
        string GetAttributeId();
        string GetFeatureName();
    }

    /// <summary>Attribute ID constants for avatar feature stats (mirrors GenieBodyAttribute for SDK use).</summary>
    internal static class AvatarFeatureStatAttributeIds
    {
        internal const string BrowThickness = "BrowThickness";
        internal const string BrowLength = "BrowLength";
        internal const string BrowPositionVert = "BrowPositionVert";
        internal const string BrowSpacing = "BrowSpacing";
        internal const string EyeSize = "EyeSize";
        internal const string EyePositionVert = "EyePositionVert";
        internal const string EyeSpacing = "EyeSpacing";
        internal const string EyeTilt = "EyeTilt";
        internal const string JawWidth = "JawWidth";
        internal const string JawLength = "JawLength";
        internal const string LipWidth = "LipWidth";
        internal const string LipFullness = "LipFullness";
        internal const string LipPositionVert = "LipPositionVert";
        internal const string NoseWidth = "NoseWidth";
        internal const string NoseHeight = "NoseHeight";
        internal const string NosePositionVert = "NosePositionVert";
        internal const string NoseTilt = "NoseTilt";
        internal const string NoseProjection = "NoseProjection";
        internal const string WeightArms = "WeightArms";
        internal const string Belly = "Belly";
        internal const string WeightUpperTorso = "WeightUpperTorso";
        internal const string WeightLowerTorso = "WeightLowerTorso";
        internal const string WeightLegs = "WeightLegs";
        internal const string WeightHeadNeck = "WeightHeadNeck";
        internal const string ShoulderSize = "ShoulderSize";
        internal const string Waist = "Waist";
        internal const string HipSize = "HipSize";
    }

    /// <summary>Eyebrow stat for ModifyAvatarFeatureStatsAsync. Use CreateEyeBrowsStat.</summary>
    public struct EyeBrowsStat : IAvatarFeatureStat
    {
        private readonly EyeBrowsStats _value;
        public EyeBrowsStat(EyeBrowsStats value) { _value = value; }
        public EyeBrowsStats Value => _value;
        public string GetAttributeId() => _value switch
        {
            EyeBrowsStats.Thickness => AvatarFeatureStatAttributeIds.BrowThickness,
            EyeBrowsStats.Length => AvatarFeatureStatAttributeIds.BrowLength,
            EyeBrowsStats.VerticalPosition => AvatarFeatureStatAttributeIds.BrowPositionVert,
            EyeBrowsStats.Spacing => AvatarFeatureStatAttributeIds.BrowSpacing,
            _ => throw new ArgumentOutOfRangeException(nameof(_value), _value, "Invalid eyebrow stat")
        };
        public string GetFeatureName() => "eyebrow";
    }

    /// <summary>Eye stat for ModifyAvatarFeatureStatsAsync. Use CreateEyeStat.</summary>
    public struct EyeStat : IAvatarFeatureStat
    {
        private readonly EyeStats _value;
        public EyeStat(EyeStats value) { _value = value; }
        public EyeStats Value => _value;
        public string GetAttributeId() => _value switch
        {
            EyeStats.Size => AvatarFeatureStatAttributeIds.EyeSize,
            EyeStats.VerticalPosition => AvatarFeatureStatAttributeIds.EyePositionVert,
            EyeStats.Spacing => AvatarFeatureStatAttributeIds.EyeSpacing,
            EyeStats.Rotation => AvatarFeatureStatAttributeIds.EyeTilt,
            _ => throw new ArgumentOutOfRangeException(nameof(_value), _value, "Invalid eye stat")
        };
        public string GetFeatureName() => "eye";
    }

    /// <summary>Jaw stat for ModifyAvatarFeatureStatsAsync. Use CreateJawStat.</summary>
    public struct JawStat : IAvatarFeatureStat
    {
        private readonly JawStats _value;
        public JawStat(JawStats value) { _value = value; }
        public JawStats Value => _value;
        public string GetAttributeId() => _value switch
        {
            JawStats.Width => AvatarFeatureStatAttributeIds.JawWidth,
            JawStats.Length => AvatarFeatureStatAttributeIds.JawLength,
            _ => throw new ArgumentOutOfRangeException(nameof(_value), _value, "Invalid jaw stat")
        };
        public string GetFeatureName() => "jaw";
    }

    /// <summary>Lip stat for ModifyAvatarFeatureStatsAsync. Use CreateLipsStat.</summary>
    public struct LipsStat : IAvatarFeatureStat
    {
        private readonly LipsStats _value;
        public LipsStat(LipsStats value) { _value = value; }
        public LipsStats Value => _value;
        public string GetAttributeId() => _value switch
        {
            LipsStats.Width => AvatarFeatureStatAttributeIds.LipWidth,
            LipsStats.Fullness => AvatarFeatureStatAttributeIds.LipFullness,
            LipsStats.VerticalPosition => AvatarFeatureStatAttributeIds.LipPositionVert,
            _ => throw new ArgumentOutOfRangeException(nameof(_value), _value, "Invalid lip stat")
        };
        public string GetFeatureName() => "lip";
    }

    /// <summary>Nose stat for ModifyAvatarFeatureStatsAsync. Use CreateNoseStat.</summary>
    public struct NoseStat : IAvatarFeatureStat
    {
        private readonly NoseStats _value;
        public NoseStat(NoseStats value) { _value = value; }
        public NoseStats Value => _value;
        public string GetAttributeId() => _value switch
        {
            NoseStats.Width => AvatarFeatureStatAttributeIds.NoseWidth,
            NoseStats.Length => AvatarFeatureStatAttributeIds.NoseHeight,
            NoseStats.VerticalPosition => AvatarFeatureStatAttributeIds.NosePositionVert,
            NoseStats.Tilt => AvatarFeatureStatAttributeIds.NoseTilt,
            NoseStats.Projection => AvatarFeatureStatAttributeIds.NoseProjection,
            _ => throw new ArgumentOutOfRangeException(nameof(_value), _value, "Invalid nose stat")
        };
        public string GetFeatureName() => "nose";
    }

    /// <summary>Body stat for ModifyAvatarFeatureStatsAsync. Use CreateBodyStat.</summary>
    public struct BodyStat : IAvatarFeatureStat
    {
        private readonly BodyStats _value;
        public BodyStat(BodyStats value) { _value = value; }
        public BodyStats Value => _value;
        public string GetAttributeId() => _value switch
        {
            BodyStats.ArmsThickness => AvatarFeatureStatAttributeIds.WeightArms,
            BodyStats.BellyFullness => AvatarFeatureStatAttributeIds.Belly,
            BodyStats.ChestBustline => AvatarFeatureStatAttributeIds.WeightUpperTorso,
            BodyStats.HipsThickness => AvatarFeatureStatAttributeIds.WeightLowerTorso,
            BodyStats.LegsThickness => AvatarFeatureStatAttributeIds.WeightLegs,
            BodyStats.NeckThickness => AvatarFeatureStatAttributeIds.WeightHeadNeck,
            BodyStats.ShoulderBroadness => AvatarFeatureStatAttributeIds.ShoulderSize,
            BodyStats.WaistThickness => AvatarFeatureStatAttributeIds.Waist,
            BodyStats.HipSize => AvatarFeatureStatAttributeIds.HipSize,
            _ => throw new ArgumentOutOfRangeException(nameof(_value), _value, "Invalid body stat")
        };
        public string GetFeatureName() => "body";
    }

    /// <summary>
    /// Identifies which avatar feature stat category to query (Body, EyeBrows, Eyes, Jaw, Lips, Nose).
    /// Used with GetAllAvatarFeatureStats to retrieve all stat values for that category.
    /// </summary>
    public enum AvatarFeatureStatType
    {
        Body,
        EyeBrows,
        Eyes,
        Jaw,
        Lips,
        Nose,
        All = 999
    }

    /// <summary>
    /// Unified enum for all avatar feature stats. Used as keys for GetAvatarFeatureStats.
    /// Category-prefixed to avoid name clashes (e.g. Width exists in Jaw, Lips, Nose).
    /// </summary>
    public enum AvatarFeatureStat
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
    /// Identifies which IColor type to get (HairColor, FacialHairColor, EyeBrowsColor, EyeLashColor, SkinColor, or EyeColor).
    /// Used with GetColorAsync to return the corresponding color value from the avatar.
    /// </summary>
    public enum AvatarColorKind
    {
        Hair,           // HairColor
        FacialHair,     // FacialHairColor
        EyeBrows,       // EyeBrowsColor
        EyeLash,        // EyeLashColor
        Skin,           // SkinColor
        Eyes,           // EyeColor
        MakeupStickers, // Stickers colors
        MakeupLipstick, // Lipstick colors
        MakeupFreckles, // Freckles colors
        MakeupFaceGems, // Facegems colors
        MakeupEyeshadow,// Eye shadow colors
        MakeupBlush     // Blush colors
    }

    /// <summary>
    /// Hair/flair type for color modification (hair, facial hair, eyebrows, or eyelashes).
    /// </summary>
    public enum HairType
    {
        Hair,        // Regular hair on the head
        FacialHair,  // Facial hair (beard, mustache, etc.)
        Eyebrows,    // Eyebrow colors
        Eyelashes,   // Eyelash colors
        All = 999
    }

    /// <summary>
    /// Source type for color preset retrieval (All, User, or Default).
    /// </summary>
    public enum ColorSource
    {
        All,      // Returns both user and default colors
        User,     // Returns only user-created custom colors
        Default   // Returns only default/preset colors
    }

    /// <summary>
    /// Source type for wearable asset retrieval (All, User, or Default).
    /// </summary>
    public enum WearableAssetSource
    {
        All,      // Returns both user and default wearable assets
        User,     // Returns only user-owned wearable assets
        Default   // Returns only default wearable assets
    }

    /// <summary>
    /// Root location for saving avatar screenshot files when a relative path is provided.
    /// </summary>
    public enum ScreenshotSaveLocation
    {
        /// <summary>Resolve the save path under <see cref="UnityEngine.Application.persistentDataPath"/>. Recommended for built applications.</summary>
        PersistentDataPath,

        /// <summary>Resolve the save path under the project root (<see cref="UnityEngine.Application.dataPath"/>). Warning: in built applications (non-Editor), this may not work as the data path is often read-only or has a different structure.</summary>
        ProjectRoot
    }

    /// <summary>
    /// Type of color preset to retrieve.
    /// </summary>
    public enum ColorType
    {
        Eyes,           // Eye colors
        Hair,           // Regular hair colors
        FacialHair,     // Facial hair colors (beard, mustache, etc.)
        Skin,           // Skin colors
        Eyebrow,        // Eyebrow colors
        Eyelash,        // Eyelash colors
        MakeupStickers, // Stickers colors
        MakeupLipstick, // Lipstick colors
        MakeupFreckles, // Freckles colors
        MakeupFaceGems, // Facegems colors
        MakeupEyeshadow,// Eye shadow colors
        MakeupBlush,    // Blush colors
        All = 999
    }

    /// <summary>
    /// Type of user color preset to retrieve.
    /// </summary>
    public enum UserColorType
    {
        Hair,        // Regular hair colors
        Eyebrow,     // Eyebrow colors
        Eyelash,     // Eyelash colors
        FacialHair,  // Facial Hair
        Skin         // Skin
    }

    /// <summary>
    /// Feature type for avatar feature modification (eyes, jawline, lips, nose, eyebrows, eyelashes, etc.).
    /// </summary>
    public enum FeatureType
    {
        Eyes,        // Eye blend shape assets
        EyeColor,    // Eye color (not yet implemented)
        Jawline,     // Jaw blend shape assets
        Lips,        // Lip blend shape assets
        Nose,        // Nose blend shape assets
        EyeBrows,    // Eyebrow blend shape assets
        EyeLashes   // Eyelash blend shape assets
    }

    /// <summary>
    /// Internal utility class for mapping SDK enums to internal assembly enums.
    /// This provides stable mapping that doesn't rely on enum ordinal positions.
    /// </summary>
    internal static class EnumMapper
    {
        /// <summary>
        /// Maps SDK GenderType to AvatarEditor GenderType using explicit name-based mapping.
        /// </summary>
        internal static Genies.Avatars.Customization.GenderType ToInternal(GenderType genderType)
        {
            return genderType switch
            {
                GenderType.Male => Genies.Avatars.Customization.GenderType.Male,
                GenderType.Female => Genies.Avatars.Customization.GenderType.Female,
                GenderType.Androgynous => Genies.Avatars.Customization.GenderType.Androgynous,
                _ => throw new System.ArgumentException($"Unknown GenderType: {genderType}")
            };
        }

        /// <summary>
        /// Maps SDK BodySize to AvatarEditor BodySize using explicit name-based mapping.
        /// </summary>
        internal static Genies.Avatars.Customization.BodySize ToInternal(BodySize bodySize)
        {
            return bodySize switch
            {
                BodySize.Skinny => Genies.Avatars.Customization.BodySize.Skinny,
                BodySize.Medium => Genies.Avatars.Customization.BodySize.Medium,
                BodySize.Heavy => Genies.Avatars.Customization.BodySize.Heavy,
                _ => throw new System.ArgumentException($"Unknown BodySize: {bodySize}")
            };
        }

        /// <summary>
        /// Maps SDK WardrobeSubcategory to AvatarEditor WardrobeSubcategory using explicit name-based mapping.
        /// </summary>
        internal static Genies.Avatars.Customization.WardrobeSubcategory ToInternal(WardrobeSubcategory subcategory)
        {
            return subcategory switch
            {
                WardrobeSubcategory.hair => Genies.Avatars.Customization.WardrobeSubcategory.hair,
                WardrobeSubcategory.eyebrows => Genies.Avatars.Customization.WardrobeSubcategory.eyebrows,
                WardrobeSubcategory.eyelashes => Genies.Avatars.Customization.WardrobeSubcategory.eyelashes,
                WardrobeSubcategory.facialHair => Genies.Avatars.Customization.WardrobeSubcategory.facialHair,
                WardrobeSubcategory.underwearTop => Genies.Avatars.Customization.WardrobeSubcategory.underwearTop,
                WardrobeSubcategory.hoodie => Genies.Avatars.Customization.WardrobeSubcategory.hoodie,
                WardrobeSubcategory.shirt => Genies.Avatars.Customization.WardrobeSubcategory.shirt,
                WardrobeSubcategory.jacket => Genies.Avatars.Customization.WardrobeSubcategory.jacket,
                WardrobeSubcategory.dress => Genies.Avatars.Customization.WardrobeSubcategory.dress,
                WardrobeSubcategory.pants => Genies.Avatars.Customization.WardrobeSubcategory.pants,
                WardrobeSubcategory.shorts => Genies.Avatars.Customization.WardrobeSubcategory.shorts,
                WardrobeSubcategory.skirt => Genies.Avatars.Customization.WardrobeSubcategory.skirt,
                WardrobeSubcategory.underwearBottom => Genies.Avatars.Customization.WardrobeSubcategory.underwearBottom,
                WardrobeSubcategory.socks => Genies.Avatars.Customization.WardrobeSubcategory.socks,
                WardrobeSubcategory.shoes => Genies.Avatars.Customization.WardrobeSubcategory.shoes,
                WardrobeSubcategory.bag => Genies.Avatars.Customization.WardrobeSubcategory.bag,
                WardrobeSubcategory.bracelet => Genies.Avatars.Customization.WardrobeSubcategory.bracelet,
                WardrobeSubcategory.earrings => Genies.Avatars.Customization.WardrobeSubcategory.earrings,
                WardrobeSubcategory.glasses => Genies.Avatars.Customization.WardrobeSubcategory.glasses,
                WardrobeSubcategory.hat => Genies.Avatars.Customization.WardrobeSubcategory.hat,
                WardrobeSubcategory.mask => Genies.Avatars.Customization.WardrobeSubcategory.mask,
                WardrobeSubcategory.watch => Genies.Avatars.Customization.WardrobeSubcategory.watch,
                WardrobeSubcategory.all => Genies.Avatars.Customization.WardrobeSubcategory.all,
                _ => throw new System.ArgumentException($"Unknown WardrobeSubcategory: {subcategory}")
            };
        }

        /// <summary>
        /// Maps SDK WearablesCategory to AvatarEditor WearablesCategory using explicit name-based mapping.
        /// </summary>
        internal static Genies.Avatars.Customization.WearablesCategory ToInternal(WearablesCategory category)
        {
            return category switch
            {
                WearablesCategory.Hoodie => Genies.Avatars.Customization.WearablesCategory.Hoodie,
                WearablesCategory.Shirt => Genies.Avatars.Customization.WearablesCategory.Shirt,
                WearablesCategory.Jacket => Genies.Avatars.Customization.WearablesCategory.Jacket,
                WearablesCategory.Dress => Genies.Avatars.Customization.WearablesCategory.Dress,
                WearablesCategory.Pants => Genies.Avatars.Customization.WearablesCategory.Pants,
                WearablesCategory.Shorts => Genies.Avatars.Customization.WearablesCategory.Shorts,
                WearablesCategory.Skirt => Genies.Avatars.Customization.WearablesCategory.Skirt,
                WearablesCategory.Shoes => Genies.Avatars.Customization.WearablesCategory.Shoes,
                WearablesCategory.Earrings => Genies.Avatars.Customization.WearablesCategory.Earrings,
                WearablesCategory.Glasses => Genies.Avatars.Customization.WearablesCategory.Glasses,
                WearablesCategory.Hat => Genies.Avatars.Customization.WearablesCategory.Hat,
                WearablesCategory.Mask => Genies.Avatars.Customization.WearablesCategory.Mask,
                WearablesCategory.All => Genies.Avatars.Customization.WearablesCategory.All,
                _ => throw new System.ArgumentException($"Unknown WearableCategory: {category}")
            };
        }

        /// <summary>
        /// Maps SDK UserWearablesCategory to AvatarEditor UserWearablesCategory using explicit name-based mapping.
        /// </summary>
        internal static Genies.Avatars.Customization.UserWearablesCategory ToInternal(UserWearablesCategory category)
        {
            return category switch
            {
                UserWearablesCategory.Hoodie => Genies.Avatars.Customization.UserWearablesCategory.Hoodie,
                UserWearablesCategory.Shirt => Genies.Avatars.Customization.UserWearablesCategory.Shirt,
                UserWearablesCategory.Jacket => Genies.Avatars.Customization.UserWearablesCategory.Jacket,
                UserWearablesCategory.Dress => Genies.Avatars.Customization.UserWearablesCategory.Dress,
                UserWearablesCategory.Pants => Genies.Avatars.Customization.UserWearablesCategory.Pants,
                UserWearablesCategory.Shorts => Genies.Avatars.Customization.UserWearablesCategory.Shorts,
                UserWearablesCategory.Skirt => Genies.Avatars.Customization.UserWearablesCategory.Skirt,
                UserWearablesCategory.Shoes => Genies.Avatars.Customization.UserWearablesCategory.Shoes,
                UserWearablesCategory.All => Genies.Avatars.Customization.UserWearablesCategory.All,
                _ => throw new System.ArgumentException($"Unknown UserWearablesCategory: {category}")
            };
        }

        /// <summary>
        /// Maps SDK AvatarFeatureCategory to AvatarEditor Core AvatarFeatureCategory using explicit name-based mapping.
        /// </summary>
        internal static Genies.Avatars.Customization.AvatarFeatureCategory ToInternal(AvatarFeatureCategory category)
        {
            return category switch
            {
                AvatarFeatureCategory.Lips => Genies.Avatars.Customization.AvatarFeatureCategory.Lips,
                AvatarFeatureCategory.Jaw => Genies.Avatars.Customization.AvatarFeatureCategory.Jaw,
                AvatarFeatureCategory.Nose => Genies.Avatars.Customization.AvatarFeatureCategory.Nose,
                AvatarFeatureCategory.Eyes => Genies.Avatars.Customization.AvatarFeatureCategory.Eyes,
                AvatarFeatureCategory.All => Genies.Avatars.Customization.AvatarFeatureCategory.All,
                _ => throw new System.ArgumentException($"Unknown AvatarFeatureCategory: {category}")
            };
        }

        /// <summary>
        /// Maps SDK AvatarMakeupCategory to AvatarEditor Core MakeupCategory.
        /// </summary>
        internal static Genies.Avatars.Customization.MakeupCategory ToInternal(AvatarMakeupCategory category)
        {
            return category switch
            {
                AvatarMakeupCategory.Stickers => Genies.Avatars.Customization.MakeupCategory.Stickers,
                AvatarMakeupCategory.Lipstick => Genies.Avatars.Customization.MakeupCategory.Lipstick,
                AvatarMakeupCategory.Freckles => Genies.Avatars.Customization.MakeupCategory.Freckles,
                AvatarMakeupCategory.FaceGems => Genies.Avatars.Customization.MakeupCategory.FaceGems,
                AvatarMakeupCategory.Eyeshadow => Genies.Avatars.Customization.MakeupCategory.Eyeshadow,
                AvatarMakeupCategory.Blush => Genies.Avatars.Customization.MakeupCategory.Blush,
                AvatarMakeupCategory.All => Genies.Avatars.Customization.MakeupCategory.All,
                _ => throw new System.ArgumentException($"Unknown AvatarMakeupCategory: {category}")
            };
        }

        /// <summary>
        /// Maps AvatarEditor Core MakeupCategory to SDK AvatarMakeupCategory (e.g. when creating MakeupColor from Core IColor).
        /// </summary>
        internal static AvatarMakeupCategory FromInternal(Genies.Avatars.Customization.MakeupCategory category)
        {
            return category switch
            {
                Genies.Avatars.Customization.MakeupCategory.Stickers => AvatarMakeupCategory.Stickers,
                Genies.Avatars.Customization.MakeupCategory.Lipstick => AvatarMakeupCategory.Lipstick,
                Genies.Avatars.Customization.MakeupCategory.Freckles => AvatarMakeupCategory.Freckles,
                Genies.Avatars.Customization.MakeupCategory.FaceGems => AvatarMakeupCategory.FaceGems,
                Genies.Avatars.Customization.MakeupCategory.Eyeshadow => AvatarMakeupCategory.Eyeshadow,
                Genies.Avatars.Customization.MakeupCategory.Blush => AvatarMakeupCategory.Blush,
                _ => throw new System.ArgumentException($"Unknown MakeupCategory: {category}")
            };
        }

        /// <summary>
        /// Maps SDK EyeBrowsStats to AvatarEditor EyeBrowsStats using explicit name-based mapping.
        /// </summary>
        internal static Genies.Avatars.Customization.EyeBrowsStats ToInternal(EyeBrowsStats stat)
        {
            return stat switch
            {
                EyeBrowsStats.Thickness => Genies.Avatars.Customization.EyeBrowsStats.Thickness,
                EyeBrowsStats.Length => Genies.Avatars.Customization.EyeBrowsStats.Length,
                EyeBrowsStats.VerticalPosition => Genies.Avatars.Customization.EyeBrowsStats.VerticalPosition,
                EyeBrowsStats.Spacing => Genies.Avatars.Customization.EyeBrowsStats.Spacing,
                _ => throw new System.ArgumentException($"Unknown EyeBrowsStats: {stat}")
            };
        }

        /// <summary>
        /// Maps SDK EyeStats to AvatarEditor EyeStats using explicit name-based mapping.
        /// </summary>
        internal static Genies.Avatars.Customization.EyeStats ToInternal(EyeStats stat)
        {
            return stat switch
            {
                EyeStats.Size => Genies.Avatars.Customization.EyeStats.Size,
                EyeStats.VerticalPosition => Genies.Avatars.Customization.EyeStats.VerticalPosition,
                EyeStats.Spacing => Genies.Avatars.Customization.EyeStats.Spacing,
                EyeStats.Rotation => Genies.Avatars.Customization.EyeStats.Rotation,
                _ => throw new System.ArgumentException($"Unknown EyeStats: {stat}")
            };
        }

        /// <summary>
        /// Maps SDK JawStats to AvatarEditor JawStats using explicit name-based mapping.
        /// </summary>
        internal static Genies.Avatars.Customization.JawStats ToInternal(JawStats stat)
        {
            return stat switch
            {
                JawStats.Width => Genies.Avatars.Customization.JawStats.Width,
                JawStats.Length => Genies.Avatars.Customization.JawStats.Length,
                _ => throw new System.ArgumentException($"Unknown JawStats: {stat}")
            };
        }

        /// <summary>
        /// Maps SDK LipsStats to AvatarEditor LipsStats using explicit name-based mapping.
        /// </summary>
        internal static Genies.Avatars.Customization.LipsStats ToInternal(LipsStats stat)
        {
            return stat switch
            {
                LipsStats.Width => Genies.Avatars.Customization.LipsStats.Width,
                LipsStats.Fullness => Genies.Avatars.Customization.LipsStats.Fullness,
                LipsStats.VerticalPosition => Genies.Avatars.Customization.LipsStats.VerticalPosition,
                _ => throw new System.ArgumentException($"Unknown LipsStats: {stat}")
            };
        }

        /// <summary>
        /// Maps SDK NoseStats to AvatarEditor NoseStats using explicit name-based mapping.
        /// </summary>
        internal static Genies.Avatars.Customization.NoseStats ToInternal(NoseStats stat)
        {
            return stat switch
            {
                NoseStats.Width => Genies.Avatars.Customization.NoseStats.Width,
                NoseStats.Length => Genies.Avatars.Customization.NoseStats.Length,
                NoseStats.VerticalPosition => Genies.Avatars.Customization.NoseStats.VerticalPosition,
                NoseStats.Tilt => Genies.Avatars.Customization.NoseStats.Tilt,
                NoseStats.Projection => Genies.Avatars.Customization.NoseStats.Projection,
                _ => throw new System.ArgumentException($"Unknown NoseStats: {stat}")
            };
        }

        /// <summary>
        /// Maps SDK AvatarFeatureStatType to AvatarEditor AvatarFeatureStatType using explicit name-based mapping.
        /// </summary>
        internal static Genies.Avatars.Customization.AvatarFeatureStatType ToInternal(AvatarFeatureStatType statType)
        {
            return statType switch
            {
                AvatarFeatureStatType.All => Genies.Avatars.Customization.AvatarFeatureStatType.All,
                AvatarFeatureStatType.Body => Genies.Avatars.Customization.AvatarFeatureStatType.Body,
                AvatarFeatureStatType.EyeBrows => Genies.Avatars.Customization.AvatarFeatureStatType.EyeBrows,
                AvatarFeatureStatType.Eyes => Genies.Avatars.Customization.AvatarFeatureStatType.Eyes,
                AvatarFeatureStatType.Jaw => Genies.Avatars.Customization.AvatarFeatureStatType.Jaw,
                AvatarFeatureStatType.Lips => Genies.Avatars.Customization.AvatarFeatureStatType.Lips,
                AvatarFeatureStatType.Nose => Genies.Avatars.Customization.AvatarFeatureStatType.Nose,
                _ => throw new System.ArgumentException($"Unknown AvatarFeatureStatType: {statType}")
            };
        }

        /// <summary>
        /// Maps AvatarEditor AvatarFeatureStat to SDK AvatarFeatureStat using explicit name-based mapping.
        /// </summary>
        internal static AvatarFeatureStat FromInternal(Genies.Avatars.Customization.AvatarFeatureStat stat)
        {
            return stat switch
            {
                Genies.Avatars.Customization.AvatarFeatureStat.Body_NeckThickness => AvatarFeatureStat.Body_NeckThickness,
                Genies.Avatars.Customization.AvatarFeatureStat.Body_ShoulderBroadness => AvatarFeatureStat.Body_ShoulderBroadness,
                Genies.Avatars.Customization.AvatarFeatureStat.Body_ChestBustline => AvatarFeatureStat.Body_ChestBustline,
                Genies.Avatars.Customization.AvatarFeatureStat.Body_ArmsThickness => AvatarFeatureStat.Body_ArmsThickness,
                Genies.Avatars.Customization.AvatarFeatureStat.Body_WaistThickness => AvatarFeatureStat.Body_WaistThickness,
                Genies.Avatars.Customization.AvatarFeatureStat.Body_BellyFullness => AvatarFeatureStat.Body_BellyFullness,
                Genies.Avatars.Customization.AvatarFeatureStat.Body_HipsThickness => AvatarFeatureStat.Body_HipsThickness,
                Genies.Avatars.Customization.AvatarFeatureStat.Body_LegsThickness => AvatarFeatureStat.Body_LegsThickness,
                Genies.Avatars.Customization.AvatarFeatureStat.Body_HipSize => AvatarFeatureStat.Body_HipSize,
                Genies.Avatars.Customization.AvatarFeatureStat.EyeBrows_Thickness => AvatarFeatureStat.EyeBrows_Thickness,
                Genies.Avatars.Customization.AvatarFeatureStat.EyeBrows_Length => AvatarFeatureStat.EyeBrows_Length,
                Genies.Avatars.Customization.AvatarFeatureStat.EyeBrows_VerticalPosition => AvatarFeatureStat.EyeBrows_VerticalPosition,
                Genies.Avatars.Customization.AvatarFeatureStat.EyeBrows_Spacing => AvatarFeatureStat.EyeBrows_Spacing,
                Genies.Avatars.Customization.AvatarFeatureStat.Eyes_Size => AvatarFeatureStat.Eyes_Size,
                Genies.Avatars.Customization.AvatarFeatureStat.Eyes_VerticalPosition => AvatarFeatureStat.Eyes_VerticalPosition,
                Genies.Avatars.Customization.AvatarFeatureStat.Eyes_Spacing => AvatarFeatureStat.Eyes_Spacing,
                Genies.Avatars.Customization.AvatarFeatureStat.Eyes_Rotation => AvatarFeatureStat.Eyes_Rotation,
                Genies.Avatars.Customization.AvatarFeatureStat.Jaw_Width => AvatarFeatureStat.Jaw_Width,
                Genies.Avatars.Customization.AvatarFeatureStat.Jaw_Length => AvatarFeatureStat.Jaw_Length,
                Genies.Avatars.Customization.AvatarFeatureStat.Lips_Width => AvatarFeatureStat.Lips_Width,
                Genies.Avatars.Customization.AvatarFeatureStat.Lips_Fullness => AvatarFeatureStat.Lips_Fullness,
                Genies.Avatars.Customization.AvatarFeatureStat.Lips_VerticalPosition => AvatarFeatureStat.Lips_VerticalPosition,
                Genies.Avatars.Customization.AvatarFeatureStat.Nose_Width => AvatarFeatureStat.Nose_Width,
                Genies.Avatars.Customization.AvatarFeatureStat.Nose_Length => AvatarFeatureStat.Nose_Length,
                Genies.Avatars.Customization.AvatarFeatureStat.Nose_VerticalPosition => AvatarFeatureStat.Nose_VerticalPosition,
                Genies.Avatars.Customization.AvatarFeatureStat.Nose_Tilt => AvatarFeatureStat.Nose_Tilt,
                Genies.Avatars.Customization.AvatarFeatureStat.Nose_Projection => AvatarFeatureStat.Nose_Projection,
                _ => throw new System.ArgumentException($"Unknown AvatarFeatureStat: {stat}")
            };
        }

        /// <summary>
        /// Maps SDK AvatarFeatureStat to AvatarEditor AvatarFeatureStat using explicit name-based mapping.
        /// </summary>
        internal static Genies.Avatars.Customization.AvatarFeatureStat ToInternal(AvatarFeatureStat stat)
        {
            return stat switch
            {
                AvatarFeatureStat.Body_NeckThickness => Genies.Avatars.Customization.AvatarFeatureStat.Body_NeckThickness,
                AvatarFeatureStat.Body_ShoulderBroadness => Genies.Avatars.Customization.AvatarFeatureStat.Body_ShoulderBroadness,
                AvatarFeatureStat.Body_ChestBustline => Genies.Avatars.Customization.AvatarFeatureStat.Body_ChestBustline,
                AvatarFeatureStat.Body_ArmsThickness => Genies.Avatars.Customization.AvatarFeatureStat.Body_ArmsThickness,
                AvatarFeatureStat.Body_WaistThickness => Genies.Avatars.Customization.AvatarFeatureStat.Body_WaistThickness,
                AvatarFeatureStat.Body_BellyFullness => Genies.Avatars.Customization.AvatarFeatureStat.Body_BellyFullness,
                AvatarFeatureStat.Body_HipsThickness => Genies.Avatars.Customization.AvatarFeatureStat.Body_HipsThickness,
                AvatarFeatureStat.Body_LegsThickness => Genies.Avatars.Customization.AvatarFeatureStat.Body_LegsThickness,
                AvatarFeatureStat.Body_HipSize => Genies.Avatars.Customization.AvatarFeatureStat.Body_HipSize,
                AvatarFeatureStat.EyeBrows_Thickness => Genies.Avatars.Customization.AvatarFeatureStat.EyeBrows_Thickness,
                AvatarFeatureStat.EyeBrows_Length => Genies.Avatars.Customization.AvatarFeatureStat.EyeBrows_Length,
                AvatarFeatureStat.EyeBrows_VerticalPosition => Genies.Avatars.Customization.AvatarFeatureStat.EyeBrows_VerticalPosition,
                AvatarFeatureStat.EyeBrows_Spacing => Genies.Avatars.Customization.AvatarFeatureStat.EyeBrows_Spacing,
                AvatarFeatureStat.Eyes_Size => Genies.Avatars.Customization.AvatarFeatureStat.Eyes_Size,
                AvatarFeatureStat.Eyes_VerticalPosition => Genies.Avatars.Customization.AvatarFeatureStat.Eyes_VerticalPosition,
                AvatarFeatureStat.Eyes_Spacing => Genies.Avatars.Customization.AvatarFeatureStat.Eyes_Spacing,
                AvatarFeatureStat.Eyes_Rotation => Genies.Avatars.Customization.AvatarFeatureStat.Eyes_Rotation,
                AvatarFeatureStat.Jaw_Width => Genies.Avatars.Customization.AvatarFeatureStat.Jaw_Width,
                AvatarFeatureStat.Jaw_Length => Genies.Avatars.Customization.AvatarFeatureStat.Jaw_Length,
                AvatarFeatureStat.Lips_Width => Genies.Avatars.Customization.AvatarFeatureStat.Lips_Width,
                AvatarFeatureStat.Lips_Fullness => Genies.Avatars.Customization.AvatarFeatureStat.Lips_Fullness,
                AvatarFeatureStat.Lips_VerticalPosition => Genies.Avatars.Customization.AvatarFeatureStat.Lips_VerticalPosition,
                AvatarFeatureStat.Nose_Width => Genies.Avatars.Customization.AvatarFeatureStat.Nose_Width,
                AvatarFeatureStat.Nose_Length => Genies.Avatars.Customization.AvatarFeatureStat.Nose_Length,
                AvatarFeatureStat.Nose_VerticalPosition => Genies.Avatars.Customization.AvatarFeatureStat.Nose_VerticalPosition,
                AvatarFeatureStat.Nose_Tilt => Genies.Avatars.Customization.AvatarFeatureStat.Nose_Tilt,
                AvatarFeatureStat.Nose_Projection => Genies.Avatars.Customization.AvatarFeatureStat.Nose_Projection,
                _ => throw new System.ArgumentException($"Unknown AvatarFeatureStat: {stat}")
            };
        }

        /// <summary>
        /// Maps SDK AvatarColorKind to AvatarEditor AvatarColorKind using explicit name-based mapping.
        /// </summary>
        internal static Genies.Avatars.Customization.AvatarColorKind ToInternal(AvatarColorKind colorKind)
        {
            return colorKind switch
            {
                AvatarColorKind.Hair => Genies.Avatars.Customization.AvatarColorKind.Hair,
                AvatarColorKind.FacialHair => Genies.Avatars.Customization.AvatarColorKind.FacialHair,
                AvatarColorKind.EyeBrows => Genies.Avatars.Customization.AvatarColorKind.EyeBrows,
                AvatarColorKind.EyeLash => Genies.Avatars.Customization.AvatarColorKind.EyeLash,
                AvatarColorKind.Skin => Genies.Avatars.Customization.AvatarColorKind.Skin,
                AvatarColorKind.Eyes => Genies.Avatars.Customization.AvatarColorKind.Eyes,
                AvatarColorKind.MakeupStickers => Genies.Avatars.Customization.AvatarColorKind.MakeupStickers,
                AvatarColorKind.MakeupLipstick => Genies.Avatars.Customization.AvatarColorKind.MakeupLipstick,
                AvatarColorKind.MakeupFreckles => Genies.Avatars.Customization.AvatarColorKind.MakeupFreckles,
                AvatarColorKind.MakeupFaceGems => Genies.Avatars.Customization.AvatarColorKind.MakeupFaceGems,
                AvatarColorKind.MakeupEyeshadow => Genies.Avatars.Customization.AvatarColorKind.MakeupEyeshadow,
                AvatarColorKind.MakeupBlush => Genies.Avatars.Customization.AvatarColorKind.MakeupBlush,
                _ => throw new System.ArgumentException($"Unknown AvatarColorKind: {colorKind}")
            };
        }
        /// <summary>
        /// Maps AvatarEditor AvatarColorKind to SDK AvatarColorKind using explicit name-based mapping.
        /// </summary>
        internal static AvatarColorKind FromInternal(Genies.Avatars.Customization.AvatarColorKind colorKind)
        {
            return colorKind switch
            {
                Genies.Avatars.Customization.AvatarColorKind.Hair => AvatarColorKind.Hair,
                Genies.Avatars.Customization.AvatarColorKind.FacialHair => AvatarColorKind.FacialHair,
                Genies.Avatars.Customization.AvatarColorKind.EyeBrows => AvatarColorKind.EyeBrows,
                Genies.Avatars.Customization.AvatarColorKind.EyeLash => AvatarColorKind.EyeLash,
                Genies.Avatars.Customization.AvatarColorKind.Skin => AvatarColorKind.Skin,
                Genies.Avatars.Customization.AvatarColorKind.Eyes => AvatarColorKind.Eyes,
                Genies.Avatars.Customization.AvatarColorKind.MakeupStickers => AvatarColorKind.MakeupStickers,
                Genies.Avatars.Customization.AvatarColorKind.MakeupLipstick => AvatarColorKind.MakeupLipstick,
                Genies.Avatars.Customization.AvatarColorKind.MakeupFreckles => AvatarColorKind.MakeupFreckles,
                Genies.Avatars.Customization.AvatarColorKind.MakeupFaceGems => AvatarColorKind.MakeupFaceGems,
                Genies.Avatars.Customization.AvatarColorKind.MakeupEyeshadow => AvatarColorKind.MakeupEyeshadow,
                Genies.Avatars.Customization.AvatarColorKind.MakeupBlush => AvatarColorKind.MakeupBlush,
                _ => throw new System.ArgumentException($"Unknown AvatarColorKind: {colorKind}")
            };
        }

        /// <summary>
        /// Maps SDK BodyStats to AvatarEditor BodyStats using explicit name-based mapping.
        /// </summary>
        internal static Genies.Avatars.Customization.BodyStats ToInternal(BodyStats stat)
        {
            return stat switch
            {
                BodyStats.NeckThickness => Genies.Avatars.Customization.BodyStats.NeckThickness,
                BodyStats.ShoulderBroadness => Genies.Avatars.Customization.BodyStats.ShoulderBroadness,
                BodyStats.ChestBustline => Genies.Avatars.Customization.BodyStats.ChestBustline,
                BodyStats.ArmsThickness => Genies.Avatars.Customization.BodyStats.ArmsThickness,
                BodyStats.WaistThickness => Genies.Avatars.Customization.BodyStats.WaistThickness,
                BodyStats.BellyFullness => Genies.Avatars.Customization.BodyStats.BellyFullness,
                BodyStats.HipsThickness => Genies.Avatars.Customization.BodyStats.HipsThickness,
                BodyStats.LegsThickness => Genies.Avatars.Customization.BodyStats.LegsThickness,
                BodyStats.HipSize => Genies.Avatars.Customization.BodyStats.HipSize,
                _ => throw new System.ArgumentException($"Unknown BodyStats: {stat}")
            };
        }

        /// <summary>
        /// Maps SDK HairType to AvatarEditor HairType using explicit name-based mapping.
        /// </summary>
        internal static Genies.Avatars.Customization.HairType ToInternal(HairType hairType)
        {
            return hairType switch
            {
                HairType.Hair => Genies.Avatars.Customization.HairType.Hair,
                HairType.FacialHair => Genies.Avatars.Customization.HairType.FacialHair,
                HairType.Eyebrows => Genies.Avatars.Customization.HairType.Eyebrows,
                HairType.Eyelashes => Genies.Avatars.Customization.HairType.Eyelashes,
                HairType.All => Genies.Avatars.Customization.HairType.All,
                _ => throw new System.ArgumentException($"Unknown HairType: {hairType}")
            };
        }

        /// <summary>
        /// Maps SDK ColorSource to AvatarEditor ColorSource using explicit name-based mapping.
        /// </summary>
        internal static Genies.Avatars.Customization.ColorSource ToInternal(ColorSource source)
        {
            return source switch
            {
                ColorSource.All => Genies.Avatars.Customization.ColorSource.All,
                ColorSource.User => Genies.Avatars.Customization.ColorSource.User,
                ColorSource.Default => Genies.Avatars.Customization.ColorSource.Default,
                _ => throw new System.ArgumentException($"Unknown ColorSource: {source}")
            };
        }

        /// <summary>
        /// Maps SDK WearableAssetSource to AvatarEditor RequestType using explicit name-based mapping.
        /// </summary>
        internal static Genies.Avatars.Customization.RequestType ToInternal(WearableAssetSource source)
        {
            return source switch
            {
                WearableAssetSource.All => Genies.Avatars.Customization.RequestType.All,
                WearableAssetSource.User => Genies.Avatars.Customization.RequestType.User,
                WearableAssetSource.Default => Genies.Avatars.Customization.RequestType.Default,
                _ => throw new System.ArgumentException($"Unknown WearableAssetSource: {source}")
            };
        }

        /// <summary>
        /// Maps SDK ColorType to AvatarEditor ColorType using explicit name-based mapping.
        /// </summary>
        internal static Genies.Avatars.Customization.ColorType ToInternal(ColorType colorType)
        {
            return colorType switch
            {
                ColorType.Eyes => Genies.Avatars.Customization.ColorType.Eyes,
                ColorType.Hair => Genies.Avatars.Customization.ColorType.Hair,
                ColorType.FacialHair => Genies.Avatars.Customization.ColorType.FacialHair,
                ColorType.Skin => Genies.Avatars.Customization.ColorType.Skin,
                ColorType.Eyebrow => Genies.Avatars.Customization.ColorType.Eyebrow,
                ColorType.Eyelash => Genies.Avatars.Customization.ColorType.Eyelash,
                ColorType.MakeupStickers => Genies.Avatars.Customization.ColorType.MakeupStickers,
                ColorType.MakeupLipstick => Genies.Avatars.Customization.ColorType.MakeupLipstick,
                ColorType.MakeupFreckles => Genies.Avatars.Customization.ColorType.MakeupFreckles,
                ColorType.MakeupFaceGems => Genies.Avatars.Customization.ColorType.MakeupFaceGems,
                ColorType.MakeupEyeshadow => Genies.Avatars.Customization.ColorType.MakeupEyeshadow,
                ColorType.MakeupBlush => Genies.Avatars.Customization.ColorType.MakeupBlush,
                ColorType.All => Genies.Avatars.Customization.ColorType.All,
                _ => throw new System.ArgumentException($"Unknown ColorType: {colorType}")
            };
        }

        /// <summary>
        /// Maps SDK UserColorType to AvatarEditor UserColorType (for GetUserColorsAsync).
        /// </summary>
        internal static Genies.Avatars.Customization.UserColorType ToInternal(UserColorType colorType)
        {
            return colorType switch
            {
                UserColorType.Hair => Genies.Avatars.Customization.UserColorType.Hair,
                UserColorType.Eyebrow => Genies.Avatars.Customization.UserColorType.Eyebrow,
                UserColorType.Eyelash => Genies.Avatars.Customization.UserColorType.Eyelash,
                UserColorType.Skin => Genies.Avatars.Customization.UserColorType.Skin,
                UserColorType.FacialHair=> Genies.Avatars.Customization.UserColorType.FacialHair,
                _ => throw new System.ArgumentException($"Unknown UserColorType: {colorType}")
            };
        }

        /// <summary>
        /// Maps SDK ScreenshotSaveLocation to AvatarEditor Core ScreenshotSaveLocation.
        /// </summary>
        internal static Genies.Avatars.Customization.ScreenshotSaveLocation ToInternal(ScreenshotSaveLocation saveLocation)
        {
            return saveLocation switch
            {
                ScreenshotSaveLocation.PersistentDataPath => Genies.Avatars.Customization.ScreenshotSaveLocation.PersistentDataPath,
                ScreenshotSaveLocation.ProjectRoot => Genies.Avatars.Customization.ScreenshotSaveLocation.ProjectRoot,
                _ => throw new System.ArgumentException($"Unknown ScreenshotSaveLocation: {saveLocation}")
            };
        }

        /// <summary>
        /// Maps SDK UserColorType to AvatarEditor ColorType using explicit name-based mapping.
        /// </summary>
        internal static Genies.Avatars.Customization.ColorType ToInternalUser(UserColorType colorType)
        {
            return colorType switch
            {
                UserColorType.Hair => Genies.Avatars.Customization.ColorType.Hair,
                UserColorType.Eyebrow => Genies.Avatars.Customization.ColorType.Eyebrow,
                UserColorType.Eyelash => Genies.Avatars.Customization.ColorType.Eyelash,
                _ => throw new System.ArgumentException($"Unknown UserColorType: {colorType}")
            };
        }

        /// <summary>
        /// Maps SDK AssetType to Inventory AssetType using explicit name-based mapping.
        /// </summary>
        internal static Genies.Inventory.AssetType ToInternal(AssetType assetType)
        {
            return assetType switch
            {
                AssetType.WardrobeGear => Genies.Inventory.AssetType.WardrobeGear,
                AssetType.AvatarBase => Genies.Inventory.AssetType.AvatarBase,
                AssetType.AvatarMakeup => Genies.Inventory.AssetType.AvatarMakeup,
                AssetType.Flair => Genies.Inventory.AssetType.Flair,
                AssetType.AvatarEyes => Genies.Inventory.AssetType.AvatarEyes,
                AssetType.ColorPreset => Genies.Inventory.AssetType.ColorPreset,
                AssetType.ImageLibrary => Genies.Inventory.AssetType.ImageLibrary,
                AssetType.AnimationLibrary => Genies.Inventory.AssetType.AnimationLibrary,
                AssetType.Avatar => Genies.Inventory.AssetType.Avatar,
                AssetType.Decor => Genies.Inventory.AssetType.Decor,
                AssetType.ModelLibrary => Genies.Inventory.AssetType.ModelLibrary,
                _ => throw new System.ArgumentException($"Unknown AssetType: {assetType}")
            };
        }

        /// <summary>
        /// Maps Inventory AssetType to SDK AssetType using explicit name-based mapping.
        /// </summary>
        internal static AssetType FromInternal(Genies.Inventory.AssetType assetType)
        {
            return assetType switch
            {
                Genies.Inventory.AssetType.WardrobeGear => AssetType.WardrobeGear,
                Genies.Inventory.AssetType.AvatarBase => AssetType.AvatarBase,
                Genies.Inventory.AssetType.AvatarMakeup => AssetType.AvatarMakeup,
                Genies.Inventory.AssetType.Flair => AssetType.Flair,
                Genies.Inventory.AssetType.AvatarEyes => AssetType.AvatarEyes,
                Genies.Inventory.AssetType.ColorPreset => AssetType.ColorPreset,
                Genies.Inventory.AssetType.ImageLibrary => AssetType.ImageLibrary,
                Genies.Inventory.AssetType.AnimationLibrary => AssetType.AnimationLibrary,
                Genies.Inventory.AssetType.Avatar => AssetType.Avatar,
                Genies.Inventory.AssetType.Decor => AssetType.Decor,
                Genies.Inventory.AssetType.ModelLibrary => AssetType.ModelLibrary,
                _ => throw new System.ArgumentException($"Unknown AssetType: {assetType}")
            };
        }

        /// <summary>
        /// Converts a list of Core IColor instances into a list of equivalent SDK IAvatarColor instances.
        /// </summary>
        /// <param name="iColors">Core colors (e.g. from GetDefaultColorsAsync or GetUserColorsAsync). Can be null; returns an empty list when null.</param>
        /// <returns>A new list of IAvatarColor in the same order.</returns>
        public static List<IAvatarColor> FromIColors(IEnumerable<Genies.Avatars.Customization.IColor> iColors)
        {
            if (iColors == null)
            {
                return new List<IAvatarColor>();
            }

            var result = new List<IAvatarColor>();
            foreach (var iColor in iColors)
            {
                result.Add(FromIColor(iColor));
            }
            return result;
        }

        /// <summary>
        /// Converts an SDK IAvatarColor into Genies.Avatars.Customization.IColor (e.g. for passing to Core SetColorAsync or internal APIs).
        /// </summary>
        /// <param name="sdkColor">The SDK IAvatarColor (e.g. from CreateHairColor, GetColorAsync, or color list APIs).</param>
        /// <returns>A Genies.Avatars.Customization.IColor of the matching Core type with the same Kind, Hexes, AssetId, Name, IsCustom, and Order.</returns>
        public static Genies.Avatars.Customization.IColor ToIColor(IAvatarColor sdkColor)
        {
            if (sdkColor == null)
            {
                throw new ArgumentNullException(nameof(sdkColor));
            }

            var hexes = sdkColor.Hexes;
            Color c0 = (hexes != null && hexes.Length > 0) ? hexes[0] : Color.clear;
            Color c1 = (hexes != null && hexes.Length > 1) ? hexes[1] : c0;
            Color c2 = (hexes != null && hexes.Length > 2) ? hexes[2] : c0;
            Color c3 = (hexes != null && hexes.Length > 3) ? hexes[3] : c0;
            string assetId = sdkColor.AssetId ?? string.Empty;
            string instanceId = sdkColor is IAvatarCustomColor customSdk ? customSdk.InstanceId : null;
            bool isCustom = sdkColor.IsCustom;

            switch (sdkColor.Kind)
            {
                case AvatarColorKind.Hair:
                    return new Genies.Avatars.Customization.HairColor(c0, c1, c2, c3, instanceId, isCustom);
                case AvatarColorKind.FacialHair:
                    return new Genies.Avatars.Customization.FacialHairColor(c0, c1, c2, c3, instanceId, isCustom);
                case AvatarColorKind.EyeBrows:
                    return new Genies.Avatars.Customization.EyeBrowsColor(c0, c1, instanceId, isCustom);
                case AvatarColorKind.EyeLash:
                    return new Genies.Avatars.Customization.EyeLashColor(c0, c1, instanceId, isCustom);
                case AvatarColorKind.Skin:
                    return new Genies.Avatars.Customization.SkinColor(c0, instanceId, isCustom);
                case AvatarColorKind.Eyes:
                    var eye = new Genies.Avatars.Customization.EyeColor(assetId, c0, c1);
                    eye.IsCustom = isCustom;
                    return eye;
                case AvatarColorKind.MakeupStickers:
                case AvatarColorKind.MakeupLipstick:
                case AvatarColorKind.MakeupFreckles:
                case AvatarColorKind.MakeupFaceGems:
                case AvatarColorKind.MakeupEyeshadow:
                case AvatarColorKind.MakeupBlush:
                    var coreCategory = EnumMapper.ToInternal(AvatarColorKindMakeupCategoryMapper.ToMakeupCategory(sdkColor.Kind));
                    return new Genies.Avatars.Customization.MakeupColor(coreCategory, c0, c1, c2, c3, instanceId, isCustom);

                default:
                    throw new ArgumentException($"Unsupported AvatarColorKind: {sdkColor.Kind}.", nameof(sdkColor));
            }
        }

        /// <summary>
        /// Converts an SDK <see cref="IAvatarColor"/> into <see cref="Genies.Avatars.Customization.ICustomColor"/> for user-color APIs.
        /// Requires <see cref="IAvatarCustomColor"/> (e.g. hair/skin); throws for <see cref="EyeColor"/>.
        /// </summary>
        internal static Genies.Avatars.Customization.ICustomColor ToICustomColor(IAvatarColor sdkColor)
        {
            var c = ToIColor(sdkColor);
            if (c is Genies.Avatars.Customization.ICustomColor cc)
            {
                return cc;
            }

            throw new ArgumentException($"User color operations require a custom color type; got {sdkColor.Kind}.", nameof(sdkColor));
        }

        /// <summary>
        /// Converts a Core IColor (e.g. from AvatarEditorSDK.GetColorAsync) into the equivalent SDK IAvatarColor.
        /// </summary>
        /// <param name="iColor">The Core IColor (HairColor, FacialHairColor, EyeBrowsColor, EyeLashColor, SkinColor, or EyeColor from Genies.AvatarEditor.Core).</param>
        /// <returns>An IAvatarColor of the matching SDK type with the same Hexes and AssetId.</returns>
        public static IAvatarColor FromIColor(Genies.Avatars.Customization.IColor iColor)
        {
            if (iColor == null)
            {
                throw new ArgumentNullException(nameof(iColor));
            }

            var hexes = iColor.Hexes;
            var assetId = iColor.AssetId ?? string.Empty;
            var instanceId = iColor is Genies.Avatars.Customization.ICustomColor cc ? cc.InstanceId : null;
            Color c0 = (hexes != null && hexes.Length > 0) ? hexes[0] : Color.clear;
            Color c1 = (hexes != null && hexes.Length > 1) ? hexes[1] : c0;
            Color c2 = (hexes != null && hexes.Length > 2) ? hexes[2] : c0;
            Color c3 = (hexes != null && hexes.Length > 3) ? hexes[3] : c0;
            bool isCustom = iColor is Genies.Avatars.Customization.ICustomColor ? true : false;
            switch (iColor)
            {
                case Genies.Avatars.Customization.HairColor _:
                    return new HairColor(c0, c1, c2, c3, instanceId, isCustom);
                case Genies.Avatars.Customization.FacialHairColor _:
                    return new FacialHairColor(c0, c1, c2, c3, instanceId,  isCustom);
                case Genies.Avatars.Customization.EyeBrowsColor _:
                    return new EyeBrowsColor(c0, c1, instanceId, isCustom);
                case Genies.Avatars.Customization.EyeLashColor _:
                    return new EyeLashColor(c0, c1, instanceId, isCustom);
                case Genies.Avatars.Customization.SkinColor _:
                    return new SkinColor(c0, instanceId,  isCustom);
                case Genies.Avatars.Customization.EyeColor _:
                    return new EyeColor(assetId, c0, c1, false);
                case Genies.Avatars.Customization.MakeupColor coreMakeup:
                    return new MakeupColor(EnumMapper.FromInternal(coreMakeup.Category), c0, c1, c2, c3, instanceId, isCustom);
                default:
                    throw new ArgumentException($"Unsupported IColor runtime type: {iColor?.GetType().Name ?? "null"}.", nameof(iColor));
            }
        }
    }
}
