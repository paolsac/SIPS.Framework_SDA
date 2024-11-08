using Autofac;
using Microsoft.Extensions.Configuration;
using SDAC_Processor.Providers.Operations;
using SIPS.Framework.Core.AutoRegister.Extensions;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.Core.Extensions;
using SIPS.Framework.SDA.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[assembly: AssemblySdaProviderContainer("SIPS.Framework.SDAC_Processor.Provider")]

namespace SIPS.Framework.SDAC_Processor.Extensions
{
    [AssemblySdaProviderRegistrationExtensionType("RegistrationExtension for SIPS.Framework.SDAC_Processor.Provider")]
    public static class SipsSDAC_ProcessorContainerBuilderExtensions
    {
        private static bool _isRegistered = false;

        [AssemblySdaProviderRegistrationMethodAttribute("RegisterSDAC_Processors")]
        public static ContainerBuilder RegisterSDAC_Processors(this ContainerBuilder containerBuilder)
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

            var assembly = typeof(SipsSDAC_ProcessorContainerBuilderExtensions).Assembly;

            // commented to avoid double registration
            // SIPSRegistrationToolbox.ConfigureContainer(containerBuilder, assembly, "SDAC");

            SIPSRegistrationToolbox.ConfigureContainerNamedByInterface<ISDAC_ETLOperation_Provider, SDAC_ETLOperation__DataFlowProvider>(containerBuilder, assembly, "SDAC", "data-flow");
            SIPSRegistrationToolbox.ConfigureContainerNamedByInterface<ISDAC_ETLOperation_Provider, SDAC_ETLOperation__SqlScriptProvider>(containerBuilder, assembly, "SDAC", "sql-script");

            _isRegistered = true;
            return containerBuilder;
        }





    }
}
