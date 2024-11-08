using SDAC_Processor.Api;
using SDAC_Processor.Api.SDAC_ETLDefinition;
using SDAC_Processor.Api.SDAC_ETLOperation;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SDAC_Processor.Providers.Operations
{
    public interface ISDAC_ETLOperation_Provider
    {
        string Key { get; set; }
        string Name { get; set; }

        SDAC_OperationStartReadinessOptions Readiness { get; set; }
        SDAC_OperationRunStateOptions RunState { get; set; }
        SDAC_OperationCompletionResultOptions CompletionResult { get; set; }
        SDAC_OperationTerminationReasonOptions TerminationReason { get; set; }

        SDAC_OperationDefinition definition { get; set; }
        Task<SDAC_OperationActionResponse> RunAsync(Dictionary<string, object> parameters, CancellationToken token);
        SDAC_Response Setup(SDAC_ETLSourceDefinition def);
        SDAC_Response SpecificSetup(SDAC_ETLSourceDefinition def);
        SDAC_OperationStartReadinessOptions CheckReadinessStatus(SDAC_ETLOperationProgram program);
        void LockedUpdate(SDAC_OperationTerminationReasonOptions reason, SDAC_OperationRunStateOptions state, SDAC_OperationCompletionResultOptions result);
    }

}
