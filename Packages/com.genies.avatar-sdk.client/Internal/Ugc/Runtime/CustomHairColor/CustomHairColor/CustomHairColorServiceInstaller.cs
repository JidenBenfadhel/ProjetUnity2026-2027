using System;
using Genies.CrashReporting;
using Genies.ServiceManagement;
using UnityEngine;
using VContainer;

namespace Genies.Ugc.CustomHair
{
    [AutoResolve]
    [Serializable]
#if GENIES_SDK && !GENIES_INTERNAL
    internal class CustomHairColorServiceInstaller : IGeniesInstaller
#else
    public class CustomHairColorServiceInstaller : IGeniesInstaller
#endif
    {
        public Shader CustomHairShader;

        public CustomHairColorServiceInstaller(Shader customHairShader)
        {
            CustomHairShader = customHairShader;
        }

        public CustomHairColorServiceInstaller()
        {
        }

        public void Install(IContainerBuilder builder)
        {
            if (CustomHairShader == null)
            {
                // Load hair shader from resources
                CustomHairShader = Resources.Load<Shader>("MegaHair_P");
                if (CustomHairShader == null)
                {
                    CrashReporter.LogError("[CustomHairColorServiceInstaller] Failed to load hair shader from Resources/MegaHair_P");
                    return;
                }
            }

            RegisterCustomHairService(builder);

        }

        private void RegisterCustomHairService(IContainerBuilder builder)
        {
            builder.Register<HairColorService>(Lifetime.Singleton).WithParameter(CustomHairShader);
        }
    }
}
