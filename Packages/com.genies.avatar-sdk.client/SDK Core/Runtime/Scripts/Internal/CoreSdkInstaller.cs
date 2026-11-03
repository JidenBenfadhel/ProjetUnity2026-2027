using System.Collections.Generic;
using Genies.Avatars.Sdk;
using Genies.ServiceManagement;
using VContainer;

namespace Genies.Sdk
{
    /// <summary>
    /// Installer for Core SDK-level services.
    /// Requires <see cref="GeniesAvatarSdkInstaller"/> to ensure the base Genies Avatars SDK
    /// services are installed first.
    /// </summary>
    internal class CoreSdkInstaller : IGeniesInstaller,
        IRequiresInstaller<GeniesAvatarSdkInstaller>
    {
        private GeniesAvatarSdkInstaller GeniesAvatarSdkInstallerOverride { get; } = new();

        public int OperationOrder => GeniesAvatarSdkInstallerOverride.OperationOrder + 1;

        public void Install(IContainerBuilder builder)
        {
            // Register Core SDK-level services.
        }

        public IEnumerable<IGeniesInstaller> GetRequiredInstallers()
        {
            return new IGeniesInstaller[] { GeniesAvatarSdkInstallerOverride };
        }
    }
}
