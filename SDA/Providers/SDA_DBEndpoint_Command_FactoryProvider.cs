using Autofac;
using Microsoft.Extensions.Logging;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.SDA.Api;
using SIPS.Framework.SDA.interfaces;
using SIPS.Framework.SDA.Providers.Base;
using System.ComponentModel.DataAnnotations;

namespace SIPS.Framework.SDA.Providers
{

    public partial class SDA_DBEndpoint_Command_FactoryProvider : SDA_BaseProvider, IFCAutoRegisterTransient
    {
        private readonly ILogger<SDA_DBEndpoint_Command_FactoryProvider> _logger;
        private readonly ILifetimeScope _autofac;

        private readonly SDA_EndpointDescriptorProvider _EndpointDescriptorProvider;
        private readonly State _state;

        public SDA_DBEndpoint_Command_FactoryProvider(ILogger<SDA_DBEndpoint_Command_FactoryProvider> logger,
                                                      ILifetimeScope autofac,
                                                      SDA_EndpointDescriptorProvider endpointDescriptorProvider,
                                                      SDA_ProvidersCollectionForBaseProvider providers,
                                                      State state)
            : base(providers)
        {
            _logger = logger;
            _autofac = autofac;
            _EndpointDescriptorProvider = endpointDescriptorProvider;
            _state = state;
        }

        public ISDA_Endpoint_DBCommandProvider LocateDataSourceProvider(string endpointDescriptorName)
        {
            var endpointDescriptor = _EndpointDescriptorProvider.GetEndpoint(endpointDescriptorName);
            if (endpointDescriptor.ConnectionString == null)
            {
                throw new System.Exception("connectionString is null");
            }

            var provider = ResolveProvider(endpointDescriptor) ;
            if (provider is ISDA_Endpoint_DBCommandProvider)
            {
                return provider as ISDA_Endpoint_DBCommandProvider;
            }
            else
            {
                throw new System.Exception($"Endpoit {endpointDescriptorName} is not a ISDA_Endpoint_DBCommandProvider");
            }
        }

        private ISDA_Endpoint_CommandProvider ResolveProvider(SDA_EndpointDescriptor endpointDescriptor)
        {
            var technology = endpointDescriptor.Technology;
            var typology = endpointDescriptor.Typology;

            switch (typology)
            {
                case "RelationalDatabase":
                    switch (technology)
                    {
                        //case "PostgreSQL":
                        //    {
                        //        var provider = _autofac.ResolveNamed<ISDA_Endpoint_DBCommandProvider>("SDA_DBEndpoint_PGcommandProvider");
                        //        provider.ConnectionString = endpointDescriptor.ConnectionString;
                        //        return provider;
                        //    }
                        //case "RedShift":
                        //    {
                        //        var provider = _autofac.ResolveNamed<ISDA_Endpoint_DBCommandProvider>("SDA_DBEndpoint_PGcommandProvider");
                        //        provider.ConnectionString = endpointDescriptor.ConnectionString;
                        //        return provider;
                        //    }
                        //case "SQLServer":
                        //    {
                        //        var provider = _autofac.ResolveNamed<ISDA_Endpoint_DBCommandProvider>("SDA_DBEndpoint_SQLServerCommandProvider");
                        //        provider.ConnectionString = endpointDescriptor.ConnectionString;
                        //        return provider;
                        //    }
                        default:
                            {
                                var def = _state.GetTechnologyProviderDef(technology);
                                if (def == null )
                                {
                                    throw new System.Exception($"Technology {technology} not supported");
                                }
                                var provider = _autofac.ResolveNamed<ISDA_Endpoint_DBCommandProvider>(def.Name);
                                if (provider == null)
                                {
                                    throw new System.Exception($"Technology {technology} is not registered");
                                }
                                provider.ConnectionString = endpointDescriptor.ConnectionString;
                                return provider;
                            }
                    }
                case "File":
                    switch (technology)
                    {
                        default:
                            throw new System.Exception($"Technology {technology} not supported");
                    }
                default:
                    {
                        throw new System.Exception($"Typology {typology} not supported");
                    }
            }
        }
    }
    //todo paolo: add more providers (also for the other technologies)
    //todo paolo: add a provider for the other types of commands

}
