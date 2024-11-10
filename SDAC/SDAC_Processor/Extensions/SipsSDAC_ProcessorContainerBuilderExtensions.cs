using Autofac;
using SIPS.Framework.Core.AutoRegister.Extensions;
using SIPS.Framework.Core.Extensions;
using SIPS.Framework.SDA.Attributes;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowConnections;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowConnections.Interfaces;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowTasks.Interfaces;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowTasks.sources;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowTasks.targets;
using SIPS.Framework.SDAC_Processor.Providers.Operations;
using System;

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
          
            SIPSRegistrationToolbox.ConfigureContainerNamedByInterface<ISDAC_ETLDataflowTask_Provider, SDAC_ETLDataflowTask_SourceDataReaderProvider>(containerBuilder, assembly, "SDAC", "data-reader");
            SIPSRegistrationToolbox.ConfigureContainerNamedByInterface<ISDAC_ETLDataflowTask_Provider, SDAC_ETLDataflowTask_DataBulkInsertProvider>(containerBuilder, assembly, "SDAC", "data-bulk-insert");
            
            SIPSRegistrationToolbox.ConfigureContainerNamedByInterface<ISDAC_ETLDataflowConnection_Provider, SDAC_ETLDataflowconnection_1_to_1_DataReaderProvider>(containerBuilder, assembly, "SDAC", "1-to-1_datareader");

            _isRegistered = true;
            return containerBuilder;
        }





    }
}
