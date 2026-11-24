using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Genies.Sdk.Avatar.Samples.CustomAvatarEditor
{
    /// <summary>
    /// Instantiates UI cells of <see cref="_cellPrefab"/> under the <see cref="_parentObject"/> transform.
    /// The <see cref="category"/> determines what data the cells will show.
    ///
    /// Subscribe to <see cref="CellCreated"/> to layer on animations, sounds, or any other
    /// external behavior when a new cell is generated.
    /// </summary>
    public class UIGenerator : MonoBehaviour
    {
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private GameObject _parentObject;

        [Tooltip("When enabled, an additional 'none' cell is generated before the asset cells " +
                 "that unequips the current asset type. Only applies to asset-type categories.")]
        [SerializeField] private bool _includeNoneOption;

        [Tooltip("The icon displayed on the 'none' cell. Only used when Include None Option is enabled.")]
        [SerializeField] private Sprite _noneIcon;

        public enum Category
        {
            Shirt,
            Hoodies,
            Jackets,
            Shorts,
            Pants,
            Skirts,
            Dresses,
            Shoes,
            Earrings,
            Glasses,
            Hats,
            Masks,
            Hair,
            FacialHair,
            Eyebrows,
            Eyelashes,
            Eyes,
            Nose,
            Lips,
            Jaw,
            HairColor,
            FacialHairColor,
            EyeColor,
            SkinColor,
            EyebrowColor,
            EyelashColor,
            EyeStatistics,
            NoseStatistics,
            LipStatistics,
            BodyStatistics,
            JawStatistics,
            EyebrowStatistics,
            MakeupStickers,
            MakeupLipstick,
            MakeupFreckles,
            MakeupFaceGems,
            MakeupEyeshadow,
            MakeupBlush,
            MakeupStickersColor,
            MakeupLipstickColor,
            MakeupFrecklesColor,
            MakeupFaceGemsColor,
            MakeupEyeshadowColor,
            MakeupBlushColor,
            Tattoos,
            None
        }

        public Category category = Category.Shirt;

        /// <summary>
        /// Invoked every time a new <see cref="UICell"/> is instantiated by this generator.
        /// Use this to add animations, sounds, or other per-cell logic externally.
        /// </summary>
        public event Action<UICell> CellCreated;

        /// <summary>
        /// Callback invoked when any asset cell is clicked (wearable, feature, makeup, tattoo, color).
        /// The argument is the asset info object whose concrete type corresponds to the category.
        /// Set by <see cref="UIGeneratorGroup"/>, which dispatches to the appropriate typed handler.
        /// </summary>
        public Func<object, UniTask> OnItemClicked { get; set; }

        /// <summary>
        /// Callback invoked when the "none" cell is clicked to unequip the current asset type.
        /// Receives this generator's <see cref="category"/> so the handler knows what to unequip.
        /// Set by <see cref="UIGeneratorGroup"/>
        /// </summary>
        public Func<Category, UniTask> OnNoneClicked { get; set; }

        /// <summary>
        /// When true, user-owned wearables are included alongside default wearables
        /// for categories that support them.
        /// Set by <see cref="UIGeneratorGroup"/>
        /// </summary>
        public bool IncludeUserWearables { get; set; }

        /// <summary>
        /// The avatar this generator operates on. Used for stat queries and slider cells.
        /// Set by <see cref="UIGeneratorGroup"/>
        /// </summary>
        public ManagedAvatar Avatar
        {
            get => _avatar;
            set => _avatar = value;
        }

        private ManagedAvatar _avatar;

        private CancellationTokenSource _generationCts;

        #region UI Generation

        public async UniTask Generate(bool generateCellByCell = true, float generationDelay = 0.05f)
        {
            _generationCts?.Cancel();
            _generationCts?.Dispose();
            _generationCts = new CancellationTokenSource();

            await CreateCells(generateCellByCell, generationDelay, _generationCts.Token);
        }

        private static readonly Dictionary<Category, WearablesCategory> WearableMap = new()
        {
            { Category.Shirt, WearablesCategory.Shirt },
            { Category.Hoodies, WearablesCategory.Hoodie },
            { Category.Jackets, WearablesCategory.Jacket },
            { Category.Shorts, WearablesCategory.Shorts },
            { Category.Pants, WearablesCategory.Pants },
            { Category.Skirts, WearablesCategory.Skirt },
            { Category.Dresses, WearablesCategory.Dress },
            { Category.Shoes, WearablesCategory.Shoes },
            { Category.Earrings, WearablesCategory.Earrings },
            { Category.Glasses, WearablesCategory.Glasses },
            { Category.Hats, WearablesCategory.Hat },
            { Category.Masks, WearablesCategory.Mask },
        };

        private static readonly Dictionary<Category, HairType> HairMap = new()
        {
            { Category.Hair, HairType.Hair },
            { Category.FacialHair, HairType.FacialHair },
            { Category.Eyebrows, HairType.Eyebrows },
            { Category.Eyelashes, HairType.Eyelashes },
        };

        private static readonly Dictionary<Category, AvatarFeatureCategory> FeatureMap = new()
        {
            { Category.Eyes, AvatarFeatureCategory.Eyes },
            { Category.Nose, AvatarFeatureCategory.Nose },
            { Category.Lips, AvatarFeatureCategory.Lips },
            { Category.Jaw, AvatarFeatureCategory.Jaw },
        };

        private static readonly Dictionary<Category, AvatarMakeupCategory> MakeupMap = new()
        {
            { Category.MakeupBlush, AvatarMakeupCategory.Blush },
            { Category.MakeupEyeshadow, AvatarMakeupCategory.Eyeshadow },
            { Category.MakeupFreckles, AvatarMakeupCategory.Freckles },
            { Category.MakeupFaceGems, AvatarMakeupCategory.FaceGems },
            { Category.MakeupLipstick, AvatarMakeupCategory.Lipstick },
            { Category.MakeupStickers, AvatarMakeupCategory.Stickers },
        };

        private static readonly Dictionary<Category, ColorType> ColorMap = new()
        {
            { Category.HairColor, ColorType.Hair },
            { Category.FacialHairColor, ColorType.FacialHair },
            { Category.EyeColor, ColorType.Eyes },
            { Category.SkinColor, ColorType.Skin },
            { Category.EyebrowColor, ColorType.Eyebrow },
            { Category.EyelashColor, ColorType.Eyelash },
            { Category.MakeupBlushColor, ColorType.MakeupBlush },
            { Category.MakeupEyeshadowColor, ColorType.MakeupEyeshadow },
            { Category.MakeupFrecklesColor, ColorType.MakeupFreckles },
            { Category.MakeupFaceGemsColor, ColorType.MakeupFaceGems },
            { Category.MakeupStickersColor, ColorType.MakeupStickers },
            { Category.MakeupLipstickColor, ColorType.MakeupLipstick },
        };

        private static readonly Dictionary<Category, AvatarFeatureStatType> StatMap = new()
        {
            { Category.EyeStatistics, AvatarFeatureStatType.Eyes },
            { Category.NoseStatistics, AvatarFeatureStatType.Nose },
            { Category.LipStatistics, AvatarFeatureStatType.Lips },
            { Category.BodyStatistics, AvatarFeatureStatType.Body },
            { Category.JawStatistics, AvatarFeatureStatType.Jaw },
            { Category.EyebrowStatistics, AvatarFeatureStatType.EyeBrows },
        };

        private async UniTask CreateCells(bool generateCellByCell, float generationDelay, CancellationToken token)
        {
            DestroyChildren();

            if (token.IsCancellationRequested)
            {
                return;
            }

            if (WearableMap.TryGetValue(category, out var wearable))
            {
                await CreateWearableCells(wearable, generateCellByCell, generationDelay, token);
            }
            else if (HairMap.TryGetValue(category, out var hair))
            {
                await CreateWearableCells(await AssetInfoDataSource.GetAvatarHair(hair), generateCellByCell, generationDelay, token);
            }
            else if (FeatureMap.TryGetValue(category, out var feature))
            {
                await CreateFeatureCells(feature, generateCellByCell, generationDelay, token);
            }
            else if (MakeupMap.TryGetValue(category, out var makeup))
            {
                await CreateMakeupCells(makeup, generateCellByCell, generationDelay, token);
            }
            else if (ColorMap.TryGetValue(category, out var color))
            {
                await CreateColorCells(color, generateCellByCell, generationDelay, token);
            }
            else if (StatMap.TryGetValue(category, out var stat))
            {
                await CreateStatisticsCells(stat, generateCellByCell, generationDelay, token);
            }
            else if (category == Category.Tattoos)
            {
                await CreateTattooCells(generateCellByCell, generationDelay, token);
            }
        }

        /// <summary>
        /// Shared cell-generation loop. Optionally prepends a "none" cell, then creates one
        /// cell per item using <paramref name="setupCell"/>, yielding between cells when
        /// <paramref name="generateCellByCell"/> is true.
        /// </summary>
        private async UniTask EmitCells<T>(
            List<T> items,
            Action<UICell, T> setupCell,
            bool includeNone,
            bool generateCellByCell,
            float generationDelay,
            CancellationToken token)
        {
            if (includeNone && _includeNoneOption && OnNoneClicked != null)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                CreateCell(cell => cell.SetUpCellAsAsset(_noneIcon, () => OnNoneClicked(category), Color.black));
                if (generateCellByCell)
                {
                    await UniTask.WaitForSeconds(generationDelay, cancellationToken: token);
                }
            }

            foreach (var item in items)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                var captured = item;
                CreateCell(cell => setupCell(cell, captured));
                if (generateCellByCell)
                {
                    await UniTask.WaitForSeconds(generationDelay, cancellationToken: token);
                }
            }

            await ForceLayoutUpdate();
        }

        private async UniTask CreateWearableCells(WearablesCategory wearablesCategory, bool generateCellByCell, float generationDelay, CancellationToken token)
        {
            var assets = await AssetInfoDataSource.GetWearablesDataForCategory(wearablesCategory);

            if (IncludeUserWearables)
            {
                var userCategory = ToUserWearablesCategory(wearablesCategory);
                if (userCategory.HasValue)
                {
                    var userAssets = await AssetInfoDataSource.GetUserWearablesDataForCategory(userCategory.Value);
                    if (userAssets != null && userAssets.Count > 0)
                    {
                        var combined = new List<WearableAssetInfo>(userAssets);
                        combined.AddRange(assets);
                        assets = combined;
                    }
                }
            }

            await CreateWearableCells(assets, generateCellByCell, generationDelay, token);
        }

        private static UserWearablesCategory? ToUserWearablesCategory(WearablesCategory wearablesCategory)
        {
            return wearablesCategory switch
            {
                WearablesCategory.Shirt => UserWearablesCategory.Shirt,
                WearablesCategory.Hoodie => UserWearablesCategory.Hoodie,
                WearablesCategory.Jacket => UserWearablesCategory.Jacket,
                WearablesCategory.Dress => UserWearablesCategory.Dress,
                WearablesCategory.Pants => UserWearablesCategory.Pants,
                WearablesCategory.Shorts => UserWearablesCategory.Shorts,
                WearablesCategory.Skirt => UserWearablesCategory.Skirt,
                WearablesCategory.Shoes => UserWearablesCategory.Shoes,
                _ => null
            };
        }

        private async UniTask CreateWearableCells(List<WearableAssetInfo> assets, bool generateCellByCell, float generationDelay, CancellationToken token)
        {
            await EmitCells(assets, (cell, a) => cell.SetUpCellAsAsset(a.Icon, () => OnItemClicked(a)), true, generateCellByCell, generationDelay, token);
        }

        private async UniTask CreateFeatureCells(AvatarFeatureCategory featureCategory, bool generateCellByCell, float generationDelay, CancellationToken token)
        {
            var features = await AssetInfoDataSource.GetAvatarFeatureDataForCategory(featureCategory);
            await EmitCells(features, (cell, f) => cell.SetUpCellAsAsset(f.Icon, () => OnItemClicked(f)), false, generateCellByCell, generationDelay, token);
        }

        private async UniTask CreateMakeupCells(AvatarMakeupCategory makeupCategory, bool generateCellByCell, float generationDelay, CancellationToken token)
        {
            var assets = await AssetInfoDataSource.GetMakeupDataForCategory(makeupCategory);
            await EmitCells(assets, (cell, a) => cell.SetUpCellAsAsset(a.Icon, () => OnItemClicked(a)), true, generateCellByCell, generationDelay, token);
        }

        private async UniTask CreateTattooCells(bool generateCellByCell, float generationDelay, CancellationToken token)
        {
            var assets = await AssetInfoDataSource.GetTattooData();
            await EmitCells(assets, (cell, a) => cell.SetUpCellAsAsset(a.Icon, () => OnItemClicked(a)), true, generateCellByCell, generationDelay, token);
        }

        private async UniTask CreateColorCells(ColorType colorType, bool generateCellByCell, float generationDelay, CancellationToken token)
        {
            var colors = await AssetInfoDataSource.GetColorDataForCategory(colorType);
            await EmitCells(colors, (cell, c) => cell.SetUpCellAsColor(c.Hexes[0], () => OnItemClicked(c)), false, generateCellByCell, generationDelay, token);
        }

        private async UniTask CreateStatisticsCells(AvatarFeatureStatType statType, bool generateCellByCell, float generationDelay, CancellationToken token)
        {
            if (Avatar == null)
            {
                Debug.LogError("Avatar must be assigned to this UI Generator or it's parent " +
                               "UI Generator Group in order to generate stat sliders");
                return;
            }

            var stats = await AssetInfoDataSource.GetCurrentStatsForCategory(Avatar, statType);
            var statList = new List<KeyValuePair<AvatarFeatureStat, float>>(stats);
            await EmitCells(statList, (cell, kv) => cell.SetUpCellAsSlider(
                FormatStatLabel(kv.Key),
                kv.Value,
                v => AvatarSdk.ModifyAvatarFeatureStatAsync(Avatar, kv.Key, v).Forget()), false, generateCellByCell, generationDelay, token);
        }

        private static string FormatStatLabel(AvatarFeatureStat stat)
        {
            var parts = stat.ToString().Split("_");
            var label = parts.Length > 1 ? parts[1] : parts[0];
            return Regex.Replace(label, "(?<!^)([A-Z])", " $1");
        }

        #endregion

        #region Cell Creation

        private UICell CreateCell(Action<UICell> setup)
        {
            var cell = Instantiate(_cellPrefab, _parentObject.transform).GetComponent<UICell>();
            setup(cell);
            CellCreated?.Invoke(cell);
            return cell;
        }

        /// <summary>
        /// Force Unity layout system to recalculate
        /// </summary>
        private async UniTask ForceLayoutUpdate()
        {
            await UniTask.DelayFrame(2);
            var rect = (RectTransform)_parentObject.transform.parent.transform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            Canvas.ForceUpdateCanvases();
        }

        #endregion

        #region Disposal and Destruction

        private void DestroyChildren()
        {
            if (_parentObject == null)
            {
                return;
            }

            for (int i = _parentObject.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(_parentObject.transform.GetChild(i).gameObject);
            }
        }

        public void ResetState()
        {
            _generationCts?.Cancel();
            _generationCts?.Dispose();
            _generationCts = null;

            DestroyChildren();
        }

        private void OnDisable()
        {
           ResetState();
        }

        private void OnDestroy()
        {
            ResetState();
        }

        #endregion
    }
}
