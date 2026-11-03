using Cysharp.Threading.Tasks;
using Genies.Avatars.Sdk;
using Genies.ServiceManagement;
using UnityEngine;

namespace Genies.Sdk
{
    /// <summary>
    /// App initializer for the Avatar SDK layer. Builds the complete installer chain
    /// from <see cref="AvatarSdkInstaller"/> and calls ServiceManager directly.
    /// Overrides <see cref="GeniesAvatarsSdkAppInitializer"/> since this layer is a
    /// superset of the core Genies Avatars SDK.
    /// Higher-level SDKs can override this via <see cref="IOverridesAppInitializer{T}"/>.
    /// </summary>
    internal class AvatarSdkAppInitializer : IOverridesAppInitializer<GeniesAvatarsSdkAppInitializer>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterOverride()
        {
            ServiceManager.RegisterAppInitializerOverrider<AvatarSdkAppInitializer>();
        }

        public AvatarSdkAppInitializer() { }

        public async UniTask InitializeAppAsync()
        {
            if (ServiceManager.IsAppInitialized)
            {
                return;
            }

            await ServiceManager.InitializeAppAsync<AvatarSdkAppInitializer>(
                () => InstallerChainBuilder.Build(new AvatarSdkInstaller()),
                disableAutoResolve: true);
        }
    }
}
