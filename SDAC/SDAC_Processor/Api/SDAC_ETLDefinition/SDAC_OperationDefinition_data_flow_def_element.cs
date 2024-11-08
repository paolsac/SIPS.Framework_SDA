namespace SDAC_Processor.Api.SDAC_ETLDefinition
{
    public class SDAC_OperationDefinition_data_flow_def_element
    {
        public string type { get; set; }
        public SDAC_OperationDefinition_inline_data_source data_source { get; set; }
        public SDAC_OperationDefinition_bulk_insert bulk_insert { get; set; }
    }
}
