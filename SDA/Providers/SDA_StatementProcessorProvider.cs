using Microsoft.Extensions.Logging;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.SDA.Api;
using SIPS.Framework.SDA.Providers.Base;
using System;
using System.Linq;

namespace SIPS.Framework.SDA.Providers
{

    public class SDA_StatementProcessorProvider : SDA_BaseProvider, IFCAutoRegisterTransient
    {
        private readonly ILogger<SDA_StatementProcessorProvider> _logger;
        private readonly SDA_EndpointDescriptorProvider _endpointProvider;
        private readonly SDA_DBEndpoint_Command_FactoryProvider _DBEndpoint_Command_FactoryProvider;
        private readonly SDA_StatementBuilderProvider _StatementBuilderProvider;

        public SDA_StatementProcessorProvider(ILogger<SDA_StatementProcessorProvider> logger,
            SDA_EndpointDescriptorProvider endpointProvider,
            SDA_DBEndpoint_Command_FactoryProvider dBEndpoint_Command_FactoryProvider,
            SDA_StatementBuilderProvider statementBuilderProvider
            , SDA_ProvidersCollectionForBaseProvider providers)
            : base(providers)
        {
            _logger = logger;
            _endpointProvider = endpointProvider;
            _DBEndpoint_Command_FactoryProvider = dBEndpoint_Command_FactoryProvider;
            _StatementBuilderProvider = statementBuilderProvider;
        }

        public SDA_Response Read(SDA_DataSourceDefinition dataSourceDefinition)
        {
            return Read<object>(dataSourceDefinition);
        }
        public SDA_Response Read<T>(SDA_DataSourceDefinition dataSourceDefinition)
        {
            SDA_Response response = null;
            try
            {

                SDA_EndpointDescriptor endpoint = _endpointProvider.GetEndpoint(dataSourceDefinition.EndpointName);
                using (
                interfaces.ISDA_Endpoint_DBCommandProvider ds = _DBEndpoint_Command_FactoryProvider.LocateDataSourceProvider(dataSourceDefinition.EndpointName))
                {

                    var statement = _StatementBuilderProvider.BuildQuery(dataSourceDefinition);

                    object parameters = null;
                    if (dataSourceDefinition.ParametersGetter != null)
                    {
                        parameters = dataSourceDefinition.ParametersGetter();
                    }

                    var results = ds.ReadFromquery<T>(statement, parameters);

                    response = new SDA_Response() { Success = true, StatusMessage = "Query executed", Value = results };
                }
                return response;
            }
            catch (Exception ex)
            {
                return SDA_Response.Error("Internal server error", ex.Message);
            }
        }

        public SDA_Response ReadOneRow<T>(SDA_DataSourceDefinition dataSourceDefinition)
        {

            SDA_Response response = null;
            try
            {
                SDA_EndpointDescriptor endpoint = _endpointProvider.GetEndpoint(dataSourceDefinition.EndpointName);
                using (interfaces.ISDA_Endpoint_DBCommandProvider ds = _DBEndpoint_Command_FactoryProvider.LocateDataSourceProvider(dataSourceDefinition.EndpointName))
                {
                    var statement = _StatementBuilderProvider.BuildQuery(dataSourceDefinition);
                    object parameters = null;
                    if (dataSourceDefinition.ParametersGetter != null)
                    {
                        parameters = dataSourceDefinition.ParametersGetter();
                    }

                    var results = ds.ReadFromquery<T>(statement, parameters).FirstOrDefault();

                    response = new SDA_Response() { Success = true, StatusMessage = "Query executed", Value = results };
                }

                return response;
            }
            catch (Exception ex)
            {
                return SDA_Response.Error("Internal server error", ex.Message);
            }
        }

        public SDA_Response ReadOneRow(SDA_DataSourceDefinition dataSourceDefinition)
        {
            return ReadOneRow<object>(dataSourceDefinition);
        }


        public SDA_Response Execute(SDA_DataSourceDefinition dataSourceDefinition)
        {
            SDA_Response response = null;
            try
            {

                SDA_EndpointDescriptor endpoint = _endpointProvider.GetEndpoint(dataSourceDefinition.EndpointName);
                using (interfaces.ISDA_Endpoint_DBCommandProvider ds = _DBEndpoint_Command_FactoryProvider.LocateDataSourceProvider(dataSourceDefinition.EndpointName))
                {
                    var statement = _StatementBuilderProvider.BuildQuery(dataSourceDefinition);

                    object parameters = null;
                    if (dataSourceDefinition.ParametersGetter != null)
                    {
                        parameters = dataSourceDefinition.ParametersGetter();
                    }

                    ds.ExecCommand(statement, parameters);

                    response = new SDA_Response() { Success = true, StatusMessage = "Query executed", Value = null };
                }
                return response;
            }
            catch (Exception ex)
            {
                return SDA_Response.Error("Internal server error", ex.Message);
            }

        }
    }
}


