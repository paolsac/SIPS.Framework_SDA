using Autofac;
using Microsoft.Extensions.Logging;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition;
using SIPS.Framework.SDAC_Processor.Providers.Base;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowConnections.Interfaces;
using System.Collections.Generic;

namespace SIPS.Framework.SDAC_Processor.Providers.DataFlowConnections
{
    public partial class SDAC_ETLDataflowconnection_FactoryProvider : SDAC_BaseProvider, IFCAutoRegisterTransient
    {
        private readonly ILifetimeScope _autofac;
        private readonly ILogger<SDAC_ETLDataflowconnection_FactoryProvider> _logger;

        public SDAC_ETLDataflowconnection_FactoryProvider(
            ILogger<SDAC_ETLDataflowconnection_FactoryProvider> logger,
            ILifetimeScope autofac,
            SDAC_ProvidersCollectionForBaseProvider providers)
            : base(providers)
        {
            _logger = logger;
            _autofac = autofac;
        }

        internal ISDAC_ETLDataflowConnection_Provider LocateDataFlowconnectionProvider(KeyValuePair<string, SDAC_OperationDefinition_data_flow_def_relationship> relationship)
        {
            try
            {
                ISDAC_ETLDataflowConnection_Provider provider = ResolveProvider(relationship.Value.type);
                    var castedProvider = provider as ISDAC_ETLDataflowConnection_Provider;
                    castedProvider.Key = relationship.Key;
                    castedProvider.Name = relationship.Key;
                    castedProvider.ConnectionType = relationship.Value.type;
                    castedProvider.definition = relationship.Value;
                    return castedProvider;
            }
            catch(System.Exception ex)
            {
                _logger.LogError(ex, $"Error resolving DataFlowConnection provider for {relationship.Value.type}");
                throw ex;
            }
        }

        private ISDAC_ETLDataflowConnection_Provider ResolveProvider(string registeredProviderName)
        {
            var provider = _autofac.ResolveNamed<ISDAC_ETLDataflowConnection_Provider>(registeredProviderName);
            return provider;
        }
    }
}
