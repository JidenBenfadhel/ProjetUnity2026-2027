using Cysharp.Threading.Tasks;
using Genies.ServiceManagement;

namespace Genies.Avatars.Sdk
{
    /// <summary>
    /// App initializer for the core Genies Avatars SDK layer.
    /// Builds the installer chain from <see cref="GeniesAvatarSdkInstaller"/> and calls
    /// ServiceManager directly. Higher-level initializers can override this via
    /// <see cref="IOverridesAppInitializer{T}"/>.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal class GeniesAvatarsSdkAppInitializer : IAppInitializer
#else
    public class GeniesAvatarsSdkAppInitializer : IAppInitializer
#endif
    {
        public GeniesAvatarsSdkAppInitializer() { }

        public async UniTask InitializeAppAsync()
        {
            if (ServiceManager.IsAppInitialized)
            {
                return;
            }

            await ServiceManager.InitializeAppAsync<GeniesAvatarsSdkAppInitializer>(
                () => InstallerChainBuilder.Build(new GeniesAvatarSdkInstaller()),
                disableAutoResolve: true);
        }
    }
}
