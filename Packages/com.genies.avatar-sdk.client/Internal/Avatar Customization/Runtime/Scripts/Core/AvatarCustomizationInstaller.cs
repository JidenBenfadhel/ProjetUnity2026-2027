using Genies.Assets.Services;
using Genies.ServiceManagement;
using Genies.Ugc.CustomHair;
using VContainer;

namespace Genies.Avatars.Customization
{
    /// <summary>
    /// Installer for Avatar Customization services.
    /// Registers the AvatarCustomizationService instance with the service manager.
    /// </summary>
    [AutoResolve]
    internal class AvatarCustomizationInstaller : IGeniesInstaller,
        IRequiresInstaller<AssetServiceInstaller>,
        IRequiresInstaller<CustomHairColorServiceInstaller>
    {
        public int OperationOrder => DefaultInstallationGroups.DefaultServices + 5; // Before avatar editor services

        public void Install(IContainerBuilder builder)
        {
            Register();
        }

        public void Register()
        {
            var newInstance = new AvatarCustomizationService();
            newInstance.RegisterSelf().As<IAvatarCustomizationService>();
        }
    }
}

