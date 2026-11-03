using System.Collections.Generic;
using Genies.Addressables;
using Genies.Avatars.Services;
using Genies.Login.Native;
using Genies.Naf.Addressables;
using Genies.ServiceManagement;
using Genies.Services.Configs;
using Genies.Telemetry;
using VContainer;

namespace Genies.Avatars.Sdk
{
    [AutoResolve]
#if GENIES_SDK && !GENIES_INTERNAL
    internal class GeniesAvatarSdkInstaller : IGeniesInstaller,
#else
    public class GeniesAvatarSdkInstaller : IGeniesInstaller,
#endif
        IRequiresInstaller<AddressableServicesInstaller>,
        IRequiresInstaller<AvatarServiceInstaller>,
        IRequiresInstaller<NafResourceProviderInstaller>
    {
        public int OperationOrder => DefaultInstallationGroups.DefaultServices + 3; // Must come after dependent IGeniesInstallers.

        private BackendEnvironment TargetEnvironment { get; }

        public GeniesAvatarSdkInstaller()
        {
#if GENIES_DEV
            // Force dev environment
            TargetEnvironment = BackendEnvironment.Dev;
#else
            // Use configured environment (defaults to Prod)
            TargetEnvironment = GeniesApiConfigManager.TargetEnvironment;
#endif
        }

        public void Install(IContainerBuilder builder)
        {
            var newInstance = new GeniesAvatarSdkService();
            newInstance.RegisterSelf().As<IGeniesAvatarSdkService>();
        }

        public IEnumerable<IGeniesInstaller> GetRequiredInstallers()
        {
            var apiConfig = new GeniesApiConfig
            {
                TargetEnv = TargetEnvironment,
            };

            return new GeniesInstallersSetup(apiConfig).ConstructInstallersList();
        }
    }
}
