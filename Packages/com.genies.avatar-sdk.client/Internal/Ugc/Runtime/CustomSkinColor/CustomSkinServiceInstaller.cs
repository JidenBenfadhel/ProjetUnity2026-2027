using Genies.ServiceManagement;
using VContainer;

namespace Genies.Ugc.CustomSkin
{
    [AutoResolve]
#if GENIES_SDK && !GENIES_INTERNAL
    internal class CustomSkinServiceInstaller : IGeniesInstaller
#else
    public class CustomSkinServiceInstaller : IGeniesInstaller
#endif
    {
        public void Install(IContainerBuilder builder)
        {
            RegisterCustomSkin(builder);
        }

        private static void RegisterCustomSkin(IContainerBuilder builder)
        {
            builder.Register<SkinColorService>(Lifetime.Singleton);
        }
    }
}
