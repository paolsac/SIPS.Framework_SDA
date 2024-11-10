using Microsoft.Extensions.Logging;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.SDA.Api;
using SIPS.Framework.SDA.Providers;
using SIPS.Framework.SDAC_Processor.Api;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLTask;
using SIPS.Framework.SDAC_Processor.Providers.Base;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowConnections.Interfaces;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowTasks.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SIPS.Framework.SDAC_Processor.Providers.DataFlowTasks.targets
{
    public class SDAC_ETLDataflowTask_DataBulkInsertProvider : SDAC_ETLDataflowTask_BaseProvider, IFCAutoRegisterTransientNamed, ISDAC_ETLDataflowTask_Provider, ISDAC_SupportDataReaderInputs
    {

        private readonly object _lock = new object();
        private readonly ILogger<SDAC_ETLDataflowTask_DataBulkInsertProvider> _logger;
        private readonly SDA_StatementProcessorProvider _statementProcessorProvider;
        private Func<IDataReader> _f_dataReader;
        private readonly SDA_BulkDestinationDefinition _destinationDefinition;
        private readonly SDAC_Response _resultResponse;

        public SDAC_ETLDataflowTask_DataBulkInsertProvider(
            SDAC_ProvidersCollectionForBaseProvider sDAC_ProvidersCollection,
            ILogger<SDAC_ETLDataflowTask_DataBulkInsertProvider> logger,
            SDA_StatementProcessorProvider statementProcessorProvider)
            : base(sDAC_ProvidersCollection)
        {
            _logger = logger;
            _statementProcessorProvider = statementProcessorProvider;
            _resultResponse = new SDAC_Response() { Success = true };
            _destinationDefinition = new SDA_BulkDestinationDefinition() { sourceType = SDA_BulkDestinationDefinition.SDA_BulkSourceTypeOptions.DataReader };
        }

        public SDAC_Response SetupGetterForInputDataReader(Func<IDataReader> reader_getter)
        {
            _f_dataReader = reader_getter;
            return new SDAC_Response() { Success = true };
        }

        public SDAC_Response HandleDataFlowEnd()
        {
            return SDAC_Response.OK;
        }

        public SDAC_Response HandleDataFlowStart()
        {
            RunStatus = SDAC_ETLTaskRunStatusOptions.Starting;
            TimeSpan tsStartTimeOut = TimeSpan.FromSeconds(5);
            DateTime dtTimeout = DateTime.Now.Add(tsStartTimeOut);
            bool timedOut = true;
            while (DateTime.Now < dtTimeout)
            {
                var dataReader = _f_dataReader();
                if (dataReader == null)
                {
                    // wait for the data reader to be set
                    Task.Delay(100).Wait();
                    continue;
                }
                timedOut = false;
                break;
            }
            if (timedOut)
            {
                RunStatus = SDAC_ETLTaskRunStatusOptions.Terminating;
                TerminationReason = SDAC_ETLTaskTerminationReasonOptions.Timeout;
                return new SDAC_Response() { Success = false, ErrorMessage = "Data reader not set in time" };
            }
            RunStatus = SDAC_ETLTaskRunStatusOptions.Running;
            _destinationDefinition.dataReader = _f_dataReader();

            Task.Run(() =>
            {
                try
                {
                    SDA_Response bulkResponse = _statementProcessorProvider.ExecuteBulkInsert(_destinationDefinition);
                    if (!bulkResponse.Success)
                    {
                        RunStatus = SDAC_ETLTaskRunStatusOptions.Terminating;
                        TerminationReason = SDAC_ETLTaskTerminationReasonOptions.Exception;
                        ErrorMessage = $"{bulkResponse.ErrorMessage} - {bulkResponse.ErrorMessage}";
                    }
                    SetResult(bulkResponse);
                    RunStatus = SDAC_ETLTaskRunStatusOptions.Completing;
                }
                catch (Exception ex)
                {
                    RunStatus = SDAC_ETLTaskRunStatusOptions.Terminating;
                    TerminationReason = SDAC_ETLTaskTerminationReasonOptions.Exception;
                    ErrorMessage = ex.Message;

                }
            });
            return SDAC_Response.OK;
        }

        public override SDAC_Response SpecificSetup(SDAC_ETLSourceDefinition scriptDef)
        {
            string endpointRef = definition.bulk_insert.endpoint_ref;
            if (!scriptDef.endpoints.TryGetValue(endpointRef, out string endpointName))
            {
                return new SDAC_Response() { Success = false, ErrorMessage = $"Endpoint {endpointRef} not found" };
            }
            _destinationDefinition.EndpointName = endpointName;
            _destinationDefinition.table = definition.bulk_insert.table;
            return new SDAC_Response() { Success = true };
        }

        public override SDAC_ETLTaskCategoryOptions GetTaskCategory()
        {
            return SDAC_ETLTaskCategoryOptions.Destination;
        }

        public override SDAC_ETLTaskCompletionModeOptions GetCompletionMode()
        {
            return SDAC_ETLTaskCompletionModeOptions.Master;
        }

        public override SDAC_Response HandleDataResults()
        {
            lock (_lock)
            {
                return _resultResponse;
            }
        }

        private void SetResult(SDA_Response bulkResponse)
        {
            lock (_lock)
            {
                _resultResponse.Success = bulkResponse.Success;
                _resultResponse.ErrorMessage = bulkResponse.ErrorMessage;
                _resultResponse.Value = bulkResponse.Value;
            }
        }

        public override void SetExternalParameters(Dictionary<string, object> parameters)
        {
            // do nothing
        }
    }

}
