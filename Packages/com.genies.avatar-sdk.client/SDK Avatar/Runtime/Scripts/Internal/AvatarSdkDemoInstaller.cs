using System.Collections.Generic;
using Genies.ServiceManagement;
using VContainer;

namespace Genies.Sdk
{
    /// <summary>
    /// Demo mode installer for Avatar SDK-level services.
    /// Requires <see cref="CoreSdkDemoInstaller"/> to ensure demo-specific
    /// Core SDK services are installed first.
    /// </summary>
    internal class AvatarSdkDemoInstaller : IGeniesInstaller,
        IRequiresInstaller<CoreSdkDemoInstaller>
    {
        private CoreSdkDemoInstaller CoreSdkDemoInstallerOverride { get; set; } = new();

        public int OperationOrder => CoreSdkDemoInstallerOverride.OperationOrder + 1;

        public void Install(IContainerBuilder builder)
        {
        }

        public IEnumerable<IGeniesInstaller> GetRequiredInstallers()
        {
            return new IGeniesInstaller[] { CoreSdkDemoInstallerOverride };
        }
    }
}
