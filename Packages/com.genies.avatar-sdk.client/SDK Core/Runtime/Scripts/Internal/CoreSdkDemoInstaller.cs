using System.Collections.Generic;
using Genies.Avatars.Sdk;
using Genies.ServiceManagement;
using VContainer;

namespace Genies.Sdk
{
    /// <summary>
    /// Demo mode installer for Core SDK-level services.
    /// Requires <see cref="GeniesAvatarSdkDemoInstaller"/> to ensure demo-specific
    /// base Genies Avatars SDK services are installed first.
    /// </summary>
    internal class CoreSdkDemoInstaller : IGeniesInstaller,
        IRequiresInstaller<GeniesAvatarSdkDemoInstaller>
    {
        private GeniesAvatarSdkDemoInstaller GeniesAvatarSdkDemoInstallerOverride { get; set; } = new();

        public int OperationOrder => GeniesAvatarSdkDemoInstallerOverride.OperationOrder + 1;

        public void Install(IContainerBuilder builder)
        {
        }

        public IEnumerable<IGeniesInstaller> GetRequiredInstallers()
        {
            return new IGeniesInstaller[] { GeniesAvatarSdkDemoInstallerOverride };
        }
    }
}
