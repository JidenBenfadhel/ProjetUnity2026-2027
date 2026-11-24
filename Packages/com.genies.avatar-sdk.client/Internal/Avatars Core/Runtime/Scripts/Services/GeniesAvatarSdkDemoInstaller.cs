using System.Collections.Generic;
using Genies.Inventory.Installers;
using Genies.ServiceManagement;
using VContainer;

namespace Genies.Avatars.Sdk
{
    /// <summary>
    /// Demo mode installer for the core Genies Avatars SDK layer.
    /// Requires <see cref="GeniesAvatarSdkInstaller"/> to inherit all base dependencies,
    /// and provides demo-specific overrides (e.g., demo inventory configuration).
    /// Also seeds the demo inventory disk cache during installation.
    /// </summary>
#if GENIES_SDK && !GENIES_INTERNAL
    internal class GeniesAvatarSdkDemoInstaller : IGeniesInstaller,
#else
    public class GeniesAvatarSdkDemoInstaller : IGeniesInstaller,
#endif
        IRequiresInstaller<GeniesAvatarSdkInstaller>
    {
        private GeniesAvatarSdkInstaller GeniesAvatarSdkInstallerOverride { get; set; } = new();

        public int OperationOrder => GeniesAvatarSdkInstallerOverride.OperationOrder + 1;

        public GeniesAvatarSdkDemoInstaller() { }

        public void Install(IContainerBuilder builder)
        {
            GeniesDemoModeInstallersSetup.SeedDemoInventoryDiskCache();
        }

        public IEnumerable<IGeniesInstaller> GetRequiredInstallers()
        {
            return new IGeniesInstaller[]
            {
                GeniesAvatarSdkInstallerOverride,
                new InventoryServiceInstaller
                {
                    DefaultInventoryAppId = "DEMO_ALL",
                    DefaultInventoryOrgId = "DEMO",
                    cacheExpirationOverride = int.MaxValue,
                    isDemoMode = true,
                },
            };
        }
    }
}
