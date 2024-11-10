using Autofac;
using Microsoft.Extensions.Logging;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition;
using SIPS.Framework.SDAC_Processor.Providers.Base;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowConnections;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowTasks.Interfaces;
using System;
using System.Collections.Generic;

namespace SIPS.Framework.SDAC_Processor.Providers.DataFlowTasks
{
    public partial class SDAC_ETLDataflowTask_FactoryProvider : SDAC_BaseProvider, IFCAutoRegisterTransient
    {
        private readonly ILifetimeScope _autofac;
        private readonly ILogger<SDAC_ETLDataflowTask_FactoryProvider> _logger;

        public SDAC_ETLDataflowTask_FactoryProvider(
            ILogger<SDAC_ETLDataflowTask_FactoryProvider> logger,
            ILifetimeScope autofac,
            SDAC_ProvidersCollectionForBaseProvider providers)
            : base(providers)
        {
            _logger = logger;
            _autofac = autofac;
        }

        internal ISDAC_ETLDataflowTask_Provider LocateDataFlowTaskProvider(KeyValuePair<string, SDAC_OperationDefinition_data_flow_def_element> element)
        {
            try
            {
                ISDAC_ETLDataflowTask_Provider provider = ResolveProvider(element.Value.type);
                var castedProvider = provider as ISDAC_ETLDataflowTask_Provider;
                castedProvider.Key = element.Key;
                castedProvider.Name = element.Key;
                castedProvider.TaskType = element.Value.type;
                castedProvider.definition = element.Value;
                return castedProvider;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error resolving DataFlowTask provider for {element.Value.type}");
                throw ex;
            }


        }

        private ISDAC_ETLDataflowTask_Provider ResolveProvider(string registeredProviderName)
        {
            var provider = _autofac.ResolveNamed<ISDAC_ETLDataflowTask_Provider>(registeredProviderName);
            return provider;
        }
    }
}
