using System;
using System.Collections.Generic;
using Genies.Avatars.Customization;
using Genies.ServiceManagement;
using VContainer;

namespace Genies.Sdk
{
    /// <summary>
    /// Installer for Avatar SDK-level services.
    /// Requires <see cref="CoreSdkInstaller"/> to ensure Core SDK services are installed first.
    /// </summary>
    internal class AvatarSdkInstaller : IGeniesInstaller,
        IRequiresInstaller<CoreSdkInstaller>,
        IRequiresInstaller<AvatarCustomizationInstaller>
    {
        private CoreSdkInstaller CoreSdkInstallerOverride { get; set; } = new();
        private AvatarCustomizationInstaller AvatarCustomizationInstallerOverride { get; set; } = new();

        public int OperationOrder => Math.Max(CoreSdkInstallerOverride.OperationOrder, AvatarCustomizationInstallerOverride.OperationOrder) + 1;

        public void Install(IContainerBuilder builder)
        {
        }

        public IEnumerable<IGeniesInstaller> GetRequiredInstallers()
        {
            return new IGeniesInstaller[] { CoreSdkInstallerOverride, AvatarCustomizationInstallerOverride };
        }
    }
}
