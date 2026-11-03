using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Genies.Naf;

namespace Genies.Avatars.Behaviors
{
    /// <summary>
    /// Decorator that overrides the "lod" parameter returned by the inner
    /// <see cref="IAssetParamsService"/> so asset fetches match the configured
    /// dynamic LOD quality tier.
    /// </summary>
    internal sealed class LodOverrideAssetParamsService : IAssetParamsService
    {
        private readonly IAssetParamsService _inner;
        private readonly string _fetchLod;

        /// <param name="inner">The underlying service to delegate to.</param>
        /// <param name="fetchLod">The LOD string ("0", "1", or "2") to use for asset fetching.</param>
        public LodOverrideAssetParamsService(IAssetParamsService inner, string fetchLod)
        {
            _inner = inner;
            _fetchLod = fetchLod;
        }

        public async UniTask<Dictionary<string, string>> FetchParamsAsync(string assetId)
        {
            var parameters = await _inner.FetchParamsAsync(assetId);
            parameters ??= new Dictionary<string, string>();
            parameters["lod"] = _fetchLod;
            return parameters;
        }

        /// <summary>
        /// Derives the fetch LOD string from LOD levels.
        /// The fetch LOD must match the highest quality tier requested,
        /// because the CDN asset must contain assets at that quality.
        /// Lower int = higher quality (0=High, 1=Mid, 2=Low).
        /// </summary>
        public static string DeriveFromLods(int[] lods)
        {
            if (lods is not { Length: > 0 })
            {
                return "2";
            }

            return lods.Min().ToString();
        }
    }
}
