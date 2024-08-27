using Autofac;
using SIPS.Framework.Core.AutoRegister.Extensions;
using System;
using SIPS.Framework.Core.Extensions;
using SIPS.Framework.SDA_SqlServer.Providers;
using SIPS.Framework.SDA.interfaces;
using SIPS.Framework.SDA.Attributes;

[assembly: AssemblySdaProviderContainer("SIPS.Framework.SDA_SqlServer.Provider")]

namespace SIPS.Framework.SDA_SqlServer.Extensions
{
    [AssemblySdaProviderRegistrationExtensionType("RegistrationExtension for SIPS.Framework.SDA_SqlServer.Provider")]
    public static class SipsSda_SqlServerContainerBuilderExtensions
    {
        private static bool _isRegistered = false;

        [AssemblySdaProviderRegistrationMethod("RegisterSda_PostgressProviders")]
        public static ContainerBuilder RegisterSda_SqlServerProviders(this ContainerBuilder containerBuilder)
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

            var assembly = typeof(SipsSda_SqlServerContainerBuilderExtensions).Assembly;
            SIPSRegistrationToolbox.ConfigureContainer(containerBuilder, assembly, "SDA");
            SIPSRegistrationToolbox.ConfigureContainerNamedByInterface<ISDA_Endpoint_DBCommandProvider, SDA_DBEndpoint_SQLServerCommandProvider>(containerBuilder, assembly, "SDA");

            _isRegistered = true;
            return containerBuilder;
        }
    }
}
