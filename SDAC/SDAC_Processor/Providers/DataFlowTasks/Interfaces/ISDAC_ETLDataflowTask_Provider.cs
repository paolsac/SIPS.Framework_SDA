using SIPS.Framework.SDAC_Processor.Api;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLTask;
using System.Collections.Generic;

namespace SIPS.Framework.SDAC_Processor.Providers.DataFlowTasks.Interfaces
{
    public interface ISDAC_ETLDataflowTask_Provider
    {
        string Key { get; set; }
        string Name { get; set; }
        string TaskType { get; set; }
        string ErrorMessage { get; set; }

        SDAC_ETLTaskCategoryOptions TaskCategory { get;  }
        SDAC_ETLTaskCompletionModeOptions CompletionMode { get;  }
        SDAC_ETLTaskRunStatusOptions RunStatus { get;  }
        SDAC_ETLTaskTerminationReasonOptions TerminationReason{ get;  }

        SDAC_OperationDefinition_data_flow_def_element definition { get; set; }
        void SetExternalParameters(Dictionary<string, object> parameters);

        SDAC_Response Setup(SDAC_ETLSourceDefinition value);
        SDAC_Response SpecificSetup(SDAC_ETLSourceDefinition def);
        SDAC_Response HandleDataFlowEnd();
        SDAC_Response HandleDataFlowStart();

        // wip
        SDAC_Response HandleDataFlowCancel();
        SDAC_Response HandleDataResults();
        SDAC_Response HandleDataProgress();

    }
}
