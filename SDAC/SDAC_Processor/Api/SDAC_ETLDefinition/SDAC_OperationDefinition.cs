namespace SDAC_Processor.Api.SDAC_ETLDefinition
{
    public class SDAC_OperationDefinition
    {
        public string name { get; set; }
        public string operation_type { get; set; }
        public SDAC_OperationDefinition_inline_data_source sql_script_def { get; set; }
        public SDAC_OperationDefinition_data_flow_def data_flow_def { get; set; }
    }
}
