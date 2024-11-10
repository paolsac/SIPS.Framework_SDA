using SIPS.Framework.SDAC_Processor.Api;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLTask;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowTasks.Interfaces;

namespace SIPS.Framework.SDAC_Processor.Providers.DataFlowConnections.Interfaces
{
    public interface ISDAC_ETLDataflowConnection_Provider
    {
        string Key { get; set; }
        string Name { get; set; }
        string ConnectionType { get; set; }
        SDAC_OperationDefinition_data_flow_def_relationship definition { get; set; }

        SDAC_Response Setup(SDAC_ETLSourceDefinition value, SDAC_ETLTaskFlow flow);
        SDAC_Response SpecificSetup(SDAC_ETLSourceDefinition def, SDAC_ETLTaskFlow flow);
        SDAC_Response AddFrom(ISDAC_ETLDataflowTask_Provider task);
        SDAC_Response AddTo(ISDAC_ETLDataflowTask_Provider task);
    }

}
