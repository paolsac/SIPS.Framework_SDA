using Microsoft.Extensions.Logging;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.SDA.Api;
using SIPS.Framework.SDA.Providers;
using SIPS.Framework.SDAC_Processor.Api;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLTask;
using SIPS.Framework.SDAC_Processor.Extensions;
using SIPS.Framework.SDAC_Processor.Providers.Base;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowConnections.Interfaces;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowTasks.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace SIPS.Framework.SDAC_Processor.Providers.DataFlowTasks.sources
{

    public class SDAC_ETLDataflowTask_SourceDataReaderProvider : SDAC_ETLDataflowTask_BaseProvider
                                                                , IFCAutoRegisterTransientNamed
                                                                , ISDAC_ETLDataflowTask_Provider
                                                                , ISDAC_SupportDataReaderOutputs
    {

        private readonly object _lock = new object();
        private readonly ILogger<SDAC_ETLDataflowTask_SourceDataReaderProvider> _logger;
        private readonly SDA_StatementProcessorProvider _statementProcessorProvider;
        private SDA_DataSourceDefinition _dataSource;
        private Action<IDataReader> _reader_setter;
        private readonly SDAC_Response _resultResponse;
        private Dictionary<string, object> _externalParameters;

        public SDAC_ETLDataflowTask_SourceDataReaderProvider(
            SDAC_ProvidersCollectionForBaseProvider sDAC_ProvidersCollection,
            SDA_StatementProcessorProvider statementProcessorProvider,
            ILogger<SDAC_ETLDataflowTask_SourceDataReaderProvider> logger) : base(sDAC_ProvidersCollection)
        {
            _statementProcessorProvider = statementProcessorProvider;
            _logger = logger;
            _resultResponse = new SDAC_Response() { Success = true };
        }

        public SDAC_Response SetupSetterForOutputDataReader(Action<IDataReader> reader_setter)
        {
            _reader_setter = reader_setter;
            return new SDAC_Response() { Success = true };
        }

        public SDAC_Response HandleDataFlowEnd()
        {
            // if not terminating, then we are completing
            if (TerminationReason == SDAC_ETLTaskTerminationReasonOptions.NotTerminated)
            {
                RunStatus = SDAC_ETLTaskRunStatusOptions.Completing;
            }
            else
            {
                RunStatus = SDAC_ETLTaskRunStatusOptions.Terminating;
            }
            RunStatus = SDAC_ETLTaskRunStatusOptions.Completing;

            var response = _statementProcessorProvider.EndDataReader();

            if (response.Success)
            {
                // if not terminating, then we are completed
                if (TerminationReason == SDAC_ETLTaskTerminationReasonOptions.NotTerminated)
                {
                    RunStatus = SDAC_ETLTaskRunStatusOptions.Completed;
                }
                else
                {
                    RunStatus = SDAC_ETLTaskRunStatusOptions.Terminated;
                }
                return SDAC_Response.OK;
            }
            RunStatus = SDAC_ETLTaskRunStatusOptions.Terminated;

            // if not already terminating, then terminated now for exception
            if (TerminationReason == SDAC_ETLTaskTerminationReasonOptions.NotTerminated)
            {
                TerminationReason = SDAC_ETLTaskTerminationReasonOptions.Exception;
            }
            return new SDAC_Response() { Success = false, ErrorMessage = response.ErrorMessage };
        }

        public SDAC_Response HandleDataFlowStart()
        {
            RunStatus = SDAC_ETLTaskRunStatusOptions.Starting;

            // if there are external parameters, then pass them to the data source
            _dataSource.OverrideParameters(_externalParameters);

            var response = _statementProcessorProvider.BeginDataReader(_dataSource);
            if (response.Success)
            {
                var wrapper = response.Value as SDA_DataReaderWrapper;
                if (wrapper == null)
                {
                    return new SDAC_Response() { Success = false, ErrorMessage = "Invalid data reader wrapper" };
                }
                _reader_setter(wrapper.Reader);
                RunStatus = SDAC_ETLTaskRunStatusOptions.Running;
                return SDAC_Response.OK;
            }
            RunStatus = SDAC_ETLTaskRunStatusOptions.Terminating;
            TerminationReason = SDAC_ETLTaskTerminationReasonOptions.Exception;
            return new SDAC_Response() { Success = response.Success, ErrorMessage = response.ErrorMessage };
        }

        public override SDAC_Response SpecificSetup(SDAC_ETLSourceDefinition def)
        {
            SDAC_Response response = definition.data_source.ToSDA_DataSourceDefinition(def);
            if (response.Success)
            {
                _dataSource = (SDA_DataSourceDefinition)response.Value;
            }
            return response;
        }

        public override SDAC_ETLTaskCategoryOptions GetTaskCategory()
        {
            return SDAC_ETLTaskCategoryOptions.Source;
        }

        public override SDAC_ETLTaskCompletionModeOptions GetCompletionMode()
        {
            return SDAC_ETLTaskCompletionModeOptions.Slave;
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
            _externalParameters = parameters;
        }
    }

}
