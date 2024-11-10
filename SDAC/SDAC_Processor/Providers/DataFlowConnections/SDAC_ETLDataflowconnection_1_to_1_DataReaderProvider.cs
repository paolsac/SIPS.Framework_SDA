using Microsoft.Extensions.Logging;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.SDAC_Processor.Api;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLTask;
using SIPS.Framework.SDAC_Processor.Providers.Base;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowConnections.Interfaces;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowTasks.Interfaces;
using System.Data;

namespace SIPS.Framework.SDAC_Processor.Providers.DataFlowConnections
{
    public class SDAC_ETLDataflowconnection_1_to_1_DataReaderProvider : SDAC_ETLDataflowConnection_BaseProvider, IFCAutoRegisterTransientNamed, ISDAC_ETLDataflowConnection_Provider
    {
        private readonly object _lock = new object();
        private readonly ILogger<SDAC_ETLDataflowconnection_1_to_1_DataReaderProvider> _logger;
        private IDataReader _dataReader;

        public SDAC_ETLDataflowconnection_1_to_1_DataReaderProvider(
            SDAC_ProvidersCollectionForBaseProvider sDAC_ProvidersCollection,
            ILogger<SDAC_ETLDataflowconnection_1_to_1_DataReaderProvider> logger)
            : base(sDAC_ProvidersCollection)
        {
            _logger = logger;
        }


        public override SDAC_Response AddFrom(ISDAC_ETLDataflowTask_Provider task)
        {
            if (task == null)
            {
                return new SDAC_Response() { Success = false, ErrorMessage = "Task is null" };
            }
            if (!(task is ISDAC_SupportDataReaderOutputs))
            {
                return new SDAC_Response() { Success = false, ErrorMessage = $"Task - {task.Name} - does not support data reader outputs" };
            }
            var fromTask = task as ISDAC_SupportDataReaderOutputs;
            fromTask.SetupSetterForOutputDataReader((dr) =>
            {
                lock (_lock)
                {
                    _dataReader = dr;
                }
            });
            return new SDAC_Response() { Success = true };
        }

        public override SDAC_Response AddTo(ISDAC_ETLDataflowTask_Provider task)
        {
            if (!(task is ISDAC_SupportDataReaderInputs))
            {
                return new SDAC_Response() { Success = false, ErrorMessage = $"Task - {task.Name} - does not support data reader inputs" };
            }
            var toTask = task as ISDAC_SupportDataReaderInputs;
            toTask.SetupGetterForInputDataReader(() =>
            {
                lock (_lock)
                {
                    return _dataReader;
                }
            });
            return new SDAC_Response() { Success = true };
        }

        public override SDAC_Response SpecificSetup(SDAC_ETLSourceDefinition def, SDAC_ETLTaskFlow flow)
        {
            return new SDAC_Response() { Success = true };
        }
    }
}
