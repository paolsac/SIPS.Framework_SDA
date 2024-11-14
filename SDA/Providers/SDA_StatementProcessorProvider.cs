using Microsoft.Extensions.Logging;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.SDA.Api;
using SIPS.Framework.SDA.interfaces;
using SIPS.Framework.SDA.Providers.Base;
using System;
using System.Linq;

namespace SIPS.Framework.SDA.Providers
{

    public class SDA_StatementProcessorProvider : SDA_BaseProvider, IFCAutoRegisterTransient, IDisposable
    {

        #region Dispose
        private bool disposedValue;


        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_readerWrapper != null && _providerForDataReader != null)
                    {
                        _providerForDataReader.EndDataReader(_readerWrapper);
                        _readerWrapper = null;
                    }
                    if (_providerForDataReader != null)
                    {
                        _providerForDataReader.Dispose();
                        _providerForDataReader = null;
                    }
                    // TODO: eliminare lo stato gestito (oggetti gestiti)
                }

                // TODO: liberare risorse non gestite (oggetti non gestiti) ed eseguire l'override del finalizzatore
                // TODO: impostare campi di grandi dimensioni su Null
                disposedValue = true;
            }
            base.Dispose(disposing);
        }

        // // TODO: eseguire l'override del finalizzatore solo se 'Dispose(bool disposing)' contiene codice per liberare risorse non gestite
        // ~SDA_DBEndpoint_PGcommandProvider()
        // {
        //     // Non modificare questo codice. Inserire il codice di pulizia nel metodo 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public new void Dispose()
        {
            // Non modificare questo codice. Inserire il codice di pulizia nel metodo 'Dispose(bool disposing)'
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
        #endregion }

        private readonly ILogger<SDA_StatementProcessorProvider> _logger;
        private readonly SDA_EndpointDescriptorProvider _endpointProvider;
        private readonly SDA_DBEndpoint_Command_FactoryProvider _DBEndpoint_Command_FactoryProvider;
        private readonly SDA_StatementBuilderProvider _StatementBuilderProvider;
        private ISDA_Endpoint_DBCommandProvider _providerForDataReader;
        private SDA_DataReaderWrapper _readerWrapper = null;

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
                interfaces.ISDA_Endpoint_DBCommandProvider ds = _DBEndpoint_Command_FactoryProvider.LocateEndpointCommandProvider(dataSourceDefinition.EndpointName))
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
                using (interfaces.ISDA_Endpoint_DBCommandProvider ds = _DBEndpoint_Command_FactoryProvider.LocateEndpointCommandProvider(dataSourceDefinition.EndpointName))
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
                return SDA_Response.Error(ex.Message, ex.Message);
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
                if (dataSourceDefinition == null)
                {
                    return SDA_Response.Error("Internal server error", "DataSourceDefinition is null");
                }
                if (string.IsNullOrEmpty(dataSourceDefinition.EndpointName))
                {
                    return SDA_Response.Error("Internal server error", "EndpointName is null or empty");
                }
                SDA_EndpointDescriptor endpoint = _endpointProvider.GetEndpoint(dataSourceDefinition.EndpointName);
                using (interfaces.ISDA_Endpoint_DBCommandProvider ds = _DBEndpoint_Command_FactoryProvider.LocateEndpointCommandProvider(dataSourceDefinition.EndpointName))
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
                return SDA_Response.Error(ex.Message, ex.Message);
            }

        }

        public SDA_Response BeginDataReader(SDA_DataSourceDefinition dataSourceDefinition)
        {
            SDA_Response response = null;
            try
            {
                SDA_EndpointDescriptor endpoint = _endpointProvider.GetEndpoint(dataSourceDefinition.EndpointName);
                _providerForDataReader = _DBEndpoint_Command_FactoryProvider.LocateEndpointCommandProvider(dataSourceDefinition.EndpointName);
                var statement = _StatementBuilderProvider.BuildQuery(dataSourceDefinition);

                object parameters = null;
                if (dataSourceDefinition.ParametersGetter != null)
                {
                    parameters = dataSourceDefinition.ParametersGetter();
                }

                _readerWrapper = _providerForDataReader.BeginDataReader(statement, parameters);

                response = new SDA_Response() { Success = true, StatusMessage = "Reader readted", Value = _readerWrapper };
                return response;
            }
            catch (Exception ex)
            {
                return SDA_Response.Error(ex.Message);
            }
        }

        public SDA_Response EndDataReader() 
        {
            SDA_Response response = null;
            try
            {
                if (_readerWrapper != null && _providerForDataReader != null)
                {
                    _providerForDataReader.EndDataReader(_readerWrapper);
                    _readerWrapper = null;
                }
                if (_providerForDataReader != null)
                {
                    _providerForDataReader.Dispose();
                    _providerForDataReader = null;
                }
                response = new SDA_Response() { Success = true, StatusMessage = "Reader closed", Value = null };
                return response;
            }
            catch (Exception ex)
            {
                return SDA_Response.Error("Internal server error", ex.Message);
            }
        }

        public SDA_Response ExecuteBulkInsert(SDA_BulkDestinationDefinition targetDefinition)
        {
            SDA_BullkCopyResult result = null;
            SDA_Response response = null;
            try
            {
                //SDA_EndpointDescriptor endpoint = _endpointProvider.GetEndpoint(targetDefinition.EndpointName);
                using (interfaces.ISDA_Endpoint_DBCommandProvider ds = _DBEndpoint_Command_FactoryProvider.LocateEndpointCommandProvider(targetDefinition.EndpointName))
                {
                    switch (targetDefinition.sourceType)
                    {
                        case SDA_BulkDestinationDefinition.SDA_BulkSourceTypeOptions.DataTable:
                            result = ds.BulkLoadFromDataTable(targetDefinition.table, targetDefinition.dtTable, targetDefinition.columnMappings, targetDefinition.options);
                            break;
                        case SDA_BulkDestinationDefinition.SDA_BulkSourceTypeOptions.DataReader:
                            result = ds.BulkLoadFromDataReader(targetDefinition.table, targetDefinition.dataReader, targetDefinition.columnMappings, targetDefinition.options);
                            break;
                        default:
                            throw new Exception($"{targetDefinition.sourceType} Invalid source type");
                    }

                    response = new SDA_Response() { Success = true, StatusMessage = "Data copied executed", Value = result };
                }
                return response;
            }
            catch (Exception ex)
            {
                return SDA_Response.Error(ex.Message, ex.Message);
            }

        }


    }
}


