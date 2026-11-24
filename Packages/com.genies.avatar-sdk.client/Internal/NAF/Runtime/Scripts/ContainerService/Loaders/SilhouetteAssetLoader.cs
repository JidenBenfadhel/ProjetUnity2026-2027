using Cysharp.Threading.Tasks;
using Genies.Refs;
using GnWrappers;

namespace Genies.Naf
{
#if GENIES_SDK && !GENIES_INTERNAL
    internal sealed class SilhouetteAssetLoader : ContainerAssetLoaderBase<Entity>
#else
    public sealed class SilhouetteAssetLoader : ContainerAssetLoaderBase<Entity>
#endif
    {
        public SilhouetteAssetLoader(Handle<ContainerApi> containerApi)
            : base(containerApi) { }

        protected override Future LoadAsyncInternal(string assetId, UnorderedMapString cParams, CancellationToken cancellationToken)
        {
            return ContainerApi.LoadSilhouetteAssetAsync(assetId, cParams, cancellationToken);
        }

        protected override BoolFuture PreloadAsyncInternal(string assetId, UnorderedMapString cParams, CancellationToken cancellationToken)
        {
            // TODO implement real silhouette preloading?
            var promise = new BoolPromise();
            EntityFuture future = ContainerApi.LoadSilhouetteAssetAsync(assetId, cParams, cancellationToken);
            future.WaitAsync().ContinueWith(() =>
            {
                try
                {
                    using Entity asset = future.Get();
                    promise.SetValue(!cancellationToken.IsCancelled() && !(asset?.IsNull() ?? true));
                }
                finally
                {
                    future.Dispose();
                    promise.Dispose();
                }
            }).Forget();

            return promise.GetFuture();
        }

        protected override Entity GetResult(Future future)
        {
            return ((EntityFuture)future).Get();
        }
    }
}
