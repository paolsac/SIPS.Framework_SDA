using Autofac;
using Microsoft.Extensions.Logging;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition;
using SIPS.Framework.SDAC_Processor.Providers.Base;
using System.Collections.Generic;

namespace SIPS.Framework.SDAC_Processor.Providers.Operations
{

    public partial class SDAC_ETLOperation_FactoryProvider : SDAC_BaseProvider, IFCAutoRegisterTransient
    {
        private readonly ILogger<SDAC_ETLOperation_FactoryProvider> _logger;
        private readonly ILifetimeScope _autofac;


        public SDAC_ETLOperation_FactoryProvider(
            ILogger<SDAC_ETLOperation_FactoryProvider> logger,
            ILifetimeScope autofac,
            SDAC_ProvidersCollectionForBaseProvider providers)
            : base(providers)
        {
            _logger = logger;
            _autofac = autofac;
        }

        internal ISDAC_ETLOperation_Provider LocateOperationProvider(KeyValuePair<string, SDAC_OperationDefinition> operationDef)
        {
            var provider = ResolveProvider(operationDef.Value.operation_type);
            if (provider is ISDAC_ETLOperation_Provider)
            {
                var castedProvider = provider as ISDAC_ETLOperation_Provider;
                castedProvider.Key = operationDef.Key;
                castedProvider.Name = operationDef.Value.name;
                castedProvider.definition = operationDef.Value;
                return castedProvider;
            }
            else
            {
                throw new System.Exception($"Operation type {operationDef.Value.operation_type} is not a ISDAC_ETLOperation_Provider");
            }
        }

        private ISDAC_ETLOperation_Provider ResolveProvider(string registeredProviderName)
        {
            var provider = _autofac.ResolveNamed<ISDAC_ETLOperation_Provider>(registeredProviderName);
            return provider;
        }
        
    }
}
