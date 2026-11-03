using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Genies.Assets.Services;
using Genies.Avatars;
using Genies.Avatars.Services;
using Genies.CrashReporting;
using Genies.DataRepositoryFramework;
using Genies.Inventory;
using Genies.ServiceManagement;
using Genies.Models;
using Genies.Refs;
using UnityEngine;

namespace Genies.Ugc.CustomHair
{
    /// <summary>
    /// Service that handles returning custom and preset hair colors.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal class HairColorService
#else
    public class HairColorService
#endif
    {
        private readonly Shader _hairShader;
        private readonly IAssetsService _addressableAssetService;
        private readonly IDefaultInventoryService _defaultInventoryService;

        private IUserColorSource _userColorSource => ServiceManager.Get<IUserColorSource>();

        private UniTaskCompletionSource _initializationSource;
        private bool _isInitialized = false;
        private ColorPresetType _colorPresetType = ColorPresetType.Hair;
        private IColorType _colorType = IColorType.Hair;

        private readonly HandleCache<string, Material> _cachedHandles = new();
        private List<ColoredInventoryAsset> _presetColors;

        private static readonly int s_hairColorBase = Shader.PropertyToID("_ColorBase");
        private static readonly int s_hairColorR = Shader.PropertyToID("_ColorR");
        private static readonly int s_hairColorG = Shader.PropertyToID("_ColorG");
        private static readonly int s_hairColorB = Shader.PropertyToID("_ColorB");

        public HairColorService(
            Shader hairShader,
            IAssetsService addressableAssetService,
            IDefaultInventoryService defaultInventoryService
        )
        {
            _hairShader = hairShader;
            _addressableAssetService = addressableAssetService;
            _defaultInventoryService = defaultInventoryService;
        }

        public void SetCategory(ColorPresetType presetCategory)
        {
            _colorPresetType = presetCategory;
            _colorType = (_colorPresetType == ColorPresetType.FacialHair)? IColorType.FacialHair : IColorType.Hair;
        }

        public async UniTask InitializeAsync()
        {
            if (_isInitialized)
            {
                return;
            }

            if (_initializationSource != null)
            {
                await _initializationSource.Task;
                return;
            }

            _initializationSource = new UniTaskCompletionSource();
            try
            {
                _presetColors = await _defaultInventoryService.GetDefaultColorPresets();
                // Filter for hair color presets - typically by category or subcategory
                _presetColors = _presetColors.Where(c =>
                    c.Category?.ToLower().Contains("hair") == true ||
                    c.SubCategories?.Any(s => s.ToLower().Contains("hair")) == true).ToList();
            }
            catch (Exception ex)
            {
                CrashReporter.LogError($"Failed to initialize HairColorService presets: {ex.Message}");
                _presetColors = new List<ColoredInventoryAsset>();
            }

            _isInitialized = true;
            _initializationSource.TrySetResult();
            _initializationSource = null;
        }

        public async UniTask<List<string>> GetAllIdsAsync()
        {
            await InitializeAsync();
            var customIds = await GetAllCustomHairIdsAsync();
            var presetIds = await GetAllPresetHairIdsAsync();

            var allIds = new List<string>();
            allIds.AddRange(customIds);
            allIds.AddRange(presetIds);
            return allIds;
        }

        public async UniTask<List<string>> GetAllCustomHairIdsAsync()
        {
            await InitializeAsync();
            var entries = await _userColorSource.GetUserColorsAsync(_colorType);
            return entries != null ? entries.Where(e => !string.IsNullOrEmpty(e.Id)).Select(e => e.Id).ToList() : new List<string>();
        }

        public async UniTask<List<string>> GetAllPresetHairIdsAsync()
        {
            await InitializeAsync();
            return _presetColors.Select(c => c.AssetId).ToList();
        }

        public async UniTask<bool> CheckIsCustomAsync(string id)
        {
            var customIds = await GetAllCustomHairIdsAsync();
            return customIds.Contains(id);
        }

        public async UniTask<CustomHairColorData> CreateOrUpdateCustomHair(CustomHairColorData customHairColorData)
        {
            await InitializeAsync();
            var colors = new List<Color> { customHairColorData.ColorBase, customHairColorData.ColorR, customHairColorData.ColorG, customHairColorData.ColorB };
            if (!string.IsNullOrEmpty(customHairColorData.Id) && (await GetAllCustomHairIdsAsync()).Contains(customHairColorData.Id))
            {
                await _userColorSource.UpdateUserColorAsync(customHairColorData.Id, colors);
                return customHairColorData;
            }

            var entry = await _userColorSource.CreateUserColorAsync(_colorType, colors);
            if (entry.HasValue)
            {
                return new CustomHairColorData
                {
                    Id = entry.Value.Id,
                    ColorBase = entry.Value.Colors != null && entry.Value.Colors.Length > 0 ? entry.Value.Colors[0] : Color.black,
                    ColorR = entry.Value.Colors != null && entry.Value.Colors.Length > 1 ? entry.Value.Colors[1] : Color.black,
                    ColorG = entry.Value.Colors != null && entry.Value.Colors.Length > 2 ? entry.Value.Colors[2] : Color.black,
                    ColorB = entry.Value.Colors != null && entry.Value.Colors.Length > 3 ? entry.Value.Colors[3] : Color.black
                };
            }

            return null;
        }

        public async UniTask<Ref<Material>> GetHairMaterialForIdAsync(string id)
        {
            await InitializeAsync();
            // check if the material was loaded before and has not been disposed yet
            if (_cachedHandles.TryGetNewReference(id, out Ref<Material> materialRef))
            {
                return materialRef;
            }

            materialRef = await LoadCustomHairColorMaterial(id);
            if (!materialRef.IsAlive)
            {
                materialRef = await LoadPresetHairColorMaterial(id);
            }

            _cachedHandles.CacheHandle(id, materialRef);

            return materialRef;
        }

        private async UniTask<Ref<Material>> LoadPresetHairColorMaterial(string id)
        {
            await InitializeAsync();

            var presetColor = _presetColors.FirstOrDefault(c => c.AssetId == id);
            if (presetColor?.Colors != null && presetColor.Colors.Count >= 4)
            {
                var material = new Material(_hairShader);

                // Map the first 4 colors to the hair color shader properties
                material.SetColor(s_hairColorBase, presetColor.Colors[0]);
                material.SetColor(s_hairColorR, presetColor.Colors.Count > 1 ? presetColor.Colors[1] : Color.black);
                material.SetColor(s_hairColorG, presetColor.Colors.Count > 2 ? presetColor.Colors[2] : Color.black);
                material.SetColor(s_hairColorB, presetColor.Colors.Count > 3 ? presetColor.Colors[3] : Color.black);

                return CreateRef.FromUnityObject(material);
            }
            else if (presetColor?.Colors != null && presetColor.Colors.Count > 0)
            {
                // Fallback: if less than 4 colors, use the first color for base and black for others
                var material = new Material(_hairShader);
                material.SetColor(s_hairColorBase, presetColor.Colors[0]);
                material.SetColor(s_hairColorR, Color.black);
                material.SetColor(s_hairColorG, Color.black);
                material.SetColor(s_hairColorB, Color.black);

                return CreateRef.FromUnityObject(material);
            }

            return default;
        }

        private async UniTask<Ref<Material>> LoadCustomHairColorMaterial(string id)
        {
            await InitializeAsync();

            CustomHairColorData data = null;
            if (await CheckIsCustomAsync(id))
            {
                var entry = await _userColorSource.GetUserColorByIdAsync(id);
                if (entry.HasValue && entry.Value.Colors != null && entry.Value.Colors.Length >= 4)
                {
                    data = new CustomHairColorData
                    {
                        Id = entry.Value.Id,
                        ColorBase = entry.Value.Colors[0],
                        ColorR = entry.Value.Colors[1],
                        ColorG = entry.Value.Colors[2],
                        ColorB = entry.Value.Colors[3]
                    };
                    AvatarEmbeddedData.SetData(id, data);
                }
            }

            if (data is null && !AvatarEmbeddedData.TryGetData(id, out data))
            {
                return default;
            }

            var material = new Material(_hairShader);
            material.SetColor(s_hairColorBase, data.ColorBase);
            material.SetColor(s_hairColorR,    data.ColorR);
            material.SetColor(s_hairColorG,    data.ColorG);
            material.SetColor(s_hairColorB,    data.ColorB);

            return CreateRef.FromUnityObject(material);
        }

        public async UniTask DeleteCustomHairAsync(string id)
        {
            await _userColorSource.DeleteUserColorAsync(id);
        }

        public async UniTask DeleteAllCustomAsync()
        {
            var ids = await GetAllCustomHairIdsAsync();
            foreach (var id in ids)
            {
                await _userColorSource.DeleteUserColorAsync(id);
            }
        }

        public async UniTask<CustomHairColorData> CustomColorDataAsync(string id)
        {
            if (!await CheckIsCustomAsync(id))
            {
                return null;
            }
            var entry = await _userColorSource.GetUserColorByIdAsync(id);
            if (!entry.HasValue || entry.Value.Colors == null || entry.Value.Colors.Length < 4)
            {
                return null;
            }
            return new CustomHairColorData
            {
                Id = entry.Value.Id,
                ColorBase = entry.Value.Colors[0],
                ColorR = entry.Value.Colors[1],
                ColorG = entry.Value.Colors[2],
                ColorB = entry.Value.Colors[3]
            };
        }
    }
}
