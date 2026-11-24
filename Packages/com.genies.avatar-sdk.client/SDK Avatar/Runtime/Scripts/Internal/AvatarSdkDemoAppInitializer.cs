using Cysharp.Threading.Tasks;
using Genies.Avatars.Sdk;
using Genies.ServiceManagement;
using UnityEngine;

namespace Genies.Sdk
{
    /// <summary>
    /// App initializer for the Avatar SDK layer in demo mode.
    /// Builds the complete installer chain from <see cref="AvatarSdkDemoInstaller"/>
    /// and calls ServiceManager directly.
    /// Overrides <see cref="GeniesAvatarsSdkDemoAppInitializer"/> since this layer is a
    /// superset of the core Genies Avatars SDK demo initialization.
    /// </summary>
    internal class AvatarSdkDemoAppInitializer : IOverridesAppInitializer<GeniesAvatarsSdkDemoAppInitializer>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterOverride()
        {
            ServiceManager.RegisterAppInitializerOverrider<AvatarSdkDemoAppInitializer>();
        }

        public AvatarSdkDemoAppInitializer() { }

        public async UniTask InitializeAppAsync()
        {
            if (ServiceManager.IsAppInitialized)
            {
                return;
            }

            await ServiceManager.InitializeAppAsync<AvatarSdkDemoAppInitializer>(
                () => InstallerChainBuilder.Build(new AvatarSdkDemoInstaller()),
                disableAutoResolve: true);
        }
    }
}
