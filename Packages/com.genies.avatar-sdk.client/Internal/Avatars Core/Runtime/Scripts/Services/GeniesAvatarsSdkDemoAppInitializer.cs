using Cysharp.Threading.Tasks;
using Genies.ServiceManagement;

namespace Genies.Avatars.Sdk
{
    /// <summary>
    /// App initializer for the core Genies Avatars SDK layer in demo mode.
    /// Seeds the demo inventory disk cache, builds the installer chain from
    /// <see cref="GeniesAvatarSdkDemoInstaller"/>, and calls ServiceManager directly.
    /// Higher-level initializers can override this via <see cref="IOverridesAppInitializer{T}"/>.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal class GeniesAvatarsSdkDemoAppInitializer : IAppInitializer
#else
    public class GeniesAvatarsSdkDemoAppInitializer : IAppInitializer
#endif
    {
        public GeniesAvatarsSdkDemoAppInitializer() { }

        public async UniTask InitializeAppAsync()
        {
            if (ServiceManager.IsAppInitialized)
            {
                return;
            }

            await ServiceManager.InitializeAppAsync<GeniesAvatarsSdkDemoAppInitializer>(
                () => InstallerChainBuilder.Build(new GeniesAvatarSdkDemoInstaller()),
                disableAutoResolve: true);
        }
    }
}
