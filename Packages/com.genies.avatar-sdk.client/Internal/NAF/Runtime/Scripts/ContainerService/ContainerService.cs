using System;
using System.Linq;
using System.Collections.Generic;
using Genies.Refs;
using GnWrappers;

namespace Genies.Naf
{
    /// <summary>
    /// Native Container API service manager that provides high level asset loading and preloading functionalities.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal sealed class ContainerService : IDisposable
#else
    public sealed class ContainerService : IDisposable
#endif
    {
        /// <summary>
        /// The default material texture streaming settings to be used when creating new ContainerService instances.
        /// </summary>
        public static MaterialTextureStreamingSettings DefaultMaterialTextureStreamingSettings;

        public CombinableAssetLoader CombinableAsset { get; }
        public SilhouetteAssetLoader SilhouetteAsset { get; }
        public TextureLoader         Texture         { get; }
        public IconLoader            Icon            { get; }

        public bool EnableTextureStreaming
        {
            get => _containerApi.Item.IsMaterialTextureStreamingEnabled();
            set => _containerApi.Item.SetMaterialTextureStreamingEnabled(value);
        }

        public bool IsDisposed => !_containerApi.IsAlive;

        private readonly Ref<ContainerApi> _containerApi;

        /// <summary>
        /// Initializes a new instance using the default <see cref="NafAssetResolverConfig"/> instance.
        /// </summary>
        public ContainerService()
            : this(NafAssetResolverConfig.Default.Serialize())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerService"/> class with the specified configuration.
        /// </summary>
        /// <param name="config">The asset resolver configuration. If null, uses the default configuration.</param>
        public ContainerService(NafAssetResolverConfig config)
            : this(config ? config.Serialize() : NafAssetResolverConfig.Default.Serialize())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerService"/> class with a serialized configuration.
        /// </summary>
        /// <param name="serializedConfig">The serialized asset resolver configuration JSON string.</param>
        public ContainerService(string serializedConfig)
        {
            _containerApi = CreateRef.FromDisposable(new ContainerApi(serializedConfig));

            SetMaterialTextureStreamingSettings(DefaultMaterialTextureStreamingSettings);

            CombinableAsset = new CombinableAssetLoader(_containerApi.Handle);
            SilhouetteAsset = new SilhouetteAssetLoader(_containerApi.Handle);
            Texture         = new TextureLoader(_containerApi.Handle);
            Icon            = new IconLoader(_containerApi.Handle);
        }

        public void SetMaterialTextureStreamingSettings(MaterialTextureStreamingSettings settings)
        {
            if (settings is not null)
            {
                EnableTextureStreaming = settings.enableTextureStreaming;
                SetMaterialDynamicLods(settings.dynamicLods);
            }
        }

        public int[] GetMaterialDynamicLods()
        {
            using var lods = _containerApi.Item.GetMaterialDynamicLods();
            return lods.ToArray();
        }

        public void SetMaterialDynamicLods(IEnumerable<int> dynamicLods)
        {
            if ((dynamicLods is not null) & (dynamicLods.Any()))
            {
                using var values = new VectorInt(dynamicLods);
                _containerApi.Item.SetMaterialDynamicLods(values);
            }
            else
            {
                _containerApi.Item.SetMaterialDynamicLods(new VectorInt {2});
            }
        }

        public void Dispose()
        {
            _containerApi.Dispose();
        }
    }
}
