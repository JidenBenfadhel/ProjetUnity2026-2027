using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Genies.Sdk.Avatar.Samples.CustomAvatarEditor
{
    /// <summary>
    /// Contains a list of <see cref="generators"/> which will all generate UI at the same time.
    ///
    /// Can be used in two ways:
    /// 1. Call <see cref="Initialize"/> with a <see cref="ManagedAvatar"/> to set the avatar
    ///    and trigger generation in one step.
    /// 2. Set <see cref="Avatar"/> externally (e.g. via <see cref="UIGenerationManager"/>)
    ///    and enable <see cref="_generateOnEnable"/> so generation starts when the object
    ///    becomes active.
    /// </summary>
    public class UIGeneratorGroup : MonoBehaviour
    {
        private ManagedAvatarComponent _managedAvatarComponent;
        private ManagedAvatar _avatar;

        public enum GenerationType
        {
            CellByCell,
            SectionBySection,
            AllAtOnce
        }

        public List<UIGenerator> generators = new();
        [SerializeField] private Button _generateButton;

        [SerializeField] private bool _generateOnEnable = false;
        [Tooltip("How the generated cells should spawn")]
        [SerializeField] private GenerationType _generationMode = GenerationType.CellByCell;

        [Tooltip("How long (in seconds) that the generator will wait between spawning cells." +
                 "Only applicable when spawning cell by cell or section by section.")]
        [SerializeField] private float _cellGenerationDelay = 0.03f;

        [Tooltip("The body slot used when equipping or unequipping tattoos from this group.")]
        [SerializeField] private MegaSkinTattooSlot _tattooSlot = MegaSkinTattooSlot.LeftTopForearm;

        public MegaSkinTattooSlot TattooSlot
        {
            get => _tattooSlot;
            set => _tattooSlot = value;
        }

        /// <summary>
        /// The avatar this group operates on. Passed to each <see cref="UIGenerator"/>
        /// so that cells, sliders, and stat queries have the avatar they need.
        /// </summary>
        public ManagedAvatar Avatar
        {
            get => _avatar;
            set => _avatar = value;
        }

        public GenerationType GenerationMode
        {
            get => _generationMode;
            set => _generationMode = value;
        }

        /// <summary>
        /// Callback invoked when a wearable cell is clicked.
        /// When null, defaults to <see cref="AvatarSdk.EquipWearableAsync"/>.
        /// </summary>
        public Func<WearableAssetInfo, UniTask> OnWearableClicked { get; set; }

        /// <summary>
        /// Callback invoked when an avatar feature cell is clicked.
        /// When null, defaults to <see cref="AvatarSdk.SetAvatarFeatureAsync"/>.
        /// </summary>
        public Func<AvatarFeaturesInfo, UniTask> OnFeatureClicked { get; set; }

        /// <summary>
        /// Callback invoked when a color cell is clicked.
        /// When null, defaults to <see cref="AvatarSdk.SetColorAsync"/>.
        /// </summary>
        public Func<IAvatarColor, UniTask> OnColorClicked { get; set; }

        /// <summary>
        /// Callback invoked when a makeup cell is clicked.
        /// When null, defaults to <see cref="AvatarSdk.EquipMakeupAsync"/>.
        /// </summary>
        public Func<AvatarMakeupInfo, UniTask> OnMakeupClicked { get; set; }

        /// <summary>
        /// Callback invoked when a tattoo cell is clicked.
        /// When null, defaults to <see cref="AvatarSdk.EquipTattooAsync"/>.
        /// </summary>
        public Func<AvatarTattooInfo, UniTask> OnTattooClicked { get; set; }

        /// <summary>
        /// Callback invoked when a "none" cell is clicked to unequip the current asset type.
        /// When null, defaults to unequipping hair or the equipped wearable for the category.
        /// </summary>
        public Func<UIGenerator.Category, UniTask> OnNoneClicked { get; set; }

        /// <summary>
        /// When true, user-owned wearables are included alongside default wearables
        /// for categories that support them.
        /// Forwarded to each <see cref="UIGenerator"/> in <see cref="CueGeneration"/>
        /// </summary>
        public bool IncludeUserWearables { get; set; }

        private void OnEnable()
        {
            if (_generateButton != null)
            {
                _generateButton.onClick.AddListener(() => CueGeneration().Forget());
            }

            // Check if there needs to be a canvas, and if so, create one
            EnsureCanvasExists();

            // Start Generation if requested for OnEnable
            if (_generateOnEnable && _avatar != null)
            {
                CueGeneration().Forget();
            }
        }

        private void EnsureCanvasExists()
        {
            Canvas canvas = null;
            var t = transform;
            while (canvas == null && t.parent != null)
            {
                t.parent.TryGetComponent(out canvas);
                t = t.parent;
            }

            if (canvas == null)
            {
                var canvasGameObject = new GameObject("Editor Canvas",
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster));

                canvas = canvasGameObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var canvasScaler = canvasGameObject.GetComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1080, 1920);
                canvasScaler.matchWidthOrHeight = 0.7f;

                transform.SetParent(canvas.transform, false);
            }
        }

        /// <summary>
        /// Initializes the group with an avatar and immediately triggers generation.
        /// Equivalent to setting <see cref="Avatar"/> then calling <see cref="CueGeneration"/>.
        /// </summary>
        public async UniTask Initialize(ManagedAvatar avatar)
        {
            Avatar = avatar;
            await CueGeneration();
        }

        public async UniTask CueGeneration()
        {
            var avatar = Avatar;
            if (avatar == null)
            {
                Debug.LogError("UI Generator Groups need an Avatar assigned before they can generate");
                return;
            }

            var onWearableClicked = OnWearableClicked ?? (info => AvatarSdk.EquipWearableAsync(avatar, info));
            var onFeatureClicked = OnFeatureClicked ?? (info => AvatarSdk.SetAvatarFeatureAsync(avatar, info));
            var onColorClicked = OnColorClicked ?? (color => AvatarSdk.SetColorAsync(avatar, color));
            var onMakeupClicked = OnMakeupClicked ?? (info => AvatarSdk.EquipMakeupAsync(avatar, info));
            var onTattooClicked = OnTattooClicked ?? (info => AvatarSdk.EquipTattooAsync(avatar, info, TattooSlot));
            var onNoneClicked = OnNoneClicked ?? (category => DefaultUnequip(avatar, category, TattooSlot));

            Func<object, UniTask> onItemClicked = item => item switch
            {
                WearableAssetInfo w => onWearableClicked(w),
                AvatarFeaturesInfo f => onFeatureClicked(f),
                AvatarMakeupInfo m => onMakeupClicked(m),
                AvatarTattooInfo t => onTattooClicked(t),
                IAvatarColor c => onColorClicked(c),
                _ => UniTask.CompletedTask,
            };

            foreach (var generator in generators)
            {
                generator.Avatar = avatar;
                generator.OnItemClicked = onItemClicked;
                generator.OnNoneClicked = onNoneClicked;
                generator.IncludeUserWearables = IncludeUserWearables;

                if (GenerationMode == GenerationType.CellByCell)
                {
                    await generator.Generate(true, _cellGenerationDelay);
                }
                else if (GenerationMode == GenerationType.SectionBySection)
                {
                    generator.Generate(true, _cellGenerationDelay).Forget();
                }
                else
                {
                    generator.Generate(false).Forget();
                }
            }
        }

        private static async UniTask DefaultUnequip(ManagedAvatar avatar, UIGenerator.Category category, MegaSkinTattooSlot tattooSlot)
        {
            switch (category)
            {
                case UIGenerator.Category.Hair:
                    await AvatarSdk.UnEquipHairAsync(avatar, HairType.Hair);
                    break;
                case UIGenerator.Category.FacialHair:
                    await AvatarSdk.UnEquipHairAsync(avatar, HairType.FacialHair);
                    break;
                case UIGenerator.Category.Eyebrows:
                    await AvatarSdk.UnEquipHairAsync(avatar, HairType.Eyebrows);
                    break;
                case UIGenerator.Category.Eyelashes:
                    await AvatarSdk.UnEquipHairAsync(avatar, HairType.Eyelashes);
                    break;
                case UIGenerator.Category.Glasses:
                    var glasses = await AssetInfoDataSource.GetEquippedAssetForCategory(avatar, WearablesCategory.Glasses);
                    if (glasses != null)
                    {
                        await AvatarSdk.UnEquipWearableAsync(avatar, glasses);
                    }

                    break;
                case UIGenerator.Category.Earrings:
                    var earrings = await AssetInfoDataSource.GetEquippedAssetForCategory(avatar, WearablesCategory.Earrings);
                    if (earrings != null)
                    {
                        await AvatarSdk.UnEquipWearableAsync(avatar, earrings);
                    }

                    break;
                case UIGenerator.Category.Hats:
                    var hat = await AssetInfoDataSource.GetEquippedAssetForCategory(avatar, WearablesCategory.Hat);
                    if (hat != null)
                    {
                        await AvatarSdk.UnEquipWearableAsync(avatar, hat);
                    }

                    break;
                case UIGenerator.Category.Masks:
                    var mask = await AssetInfoDataSource.GetEquippedAssetForCategory(avatar, WearablesCategory.Mask);
                    if (mask != null)
                    {
                        await AvatarSdk.UnEquipWearableAsync(avatar, mask);
                    }

                    break;
                case UIGenerator.Category.Tattoos:
                    await AvatarSdk.UnEquipTattooAsync(avatar, tattooSlot);
                    break;
            }
        }
        private void OnDisable()
        {
            if (_generateButton != null)
            {
                _generateButton.onClick.RemoveAllListeners();
            }
        }
    }
}
