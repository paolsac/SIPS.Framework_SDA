using Autofac;
using SIPS.Framework.Core.AutoRegister.Extensions;
using SIPS.Framework.Core.Extensions;
using SIPS.Framework.SDA.Attributes;
using SIPS.Framework.SDA.interfaces;
using SIPS.Framework.SDA_Postgress.Providers;
using System;

[assembly: AssemblySdaProviderContainer("SIPS.Framework.SDA_Postgress.Provider")]

namespace SIPS.Framework.SDA_Postgress.Extensions
{
    [AssemblySdaProviderRegistrationExtensionType("RegistrationExtension for SIPS.Framework.SDA_Postgress.Provider")]
    public static class SipsSda_PostgressContainerBuilderExtensions
    {
        private static bool _isRegistered = false;

        [AssemblySdaProviderRegistrationMethodAttribute("RegisterSda_PostgressProviders")]
        public static ContainerBuilder RegisterSda_PostgressProviders(this ContainerBuilder containerBuilder)
        {
            if (_isRegistered)
            {
                return containerBuilder;
            }
            if (containerBuilder == null)
            {
                throw new ArgumentNullException("containerBuilder");
            }

            // Register all providers in the dependency injection container
            containerBuilder.RegisterCoreProviders();

            var assembly = typeof(SipsSda_PostgressContainerBuilderExtensions).Assembly;
            SIPSRegistrationToolbox.ConfigureContainer(containerBuilder, assembly, "SDA");
            SIPSRegistrationToolbox.ConfigureContainerNamedByInterface<ISDA_Endpoint_DBCommandProvider, SDA_DBEndpoint_PGCommandProvider>(containerBuilder, assembly, "SDA");

            _isRegistered = true;
            return containerBuilder;
        }
    }
}
