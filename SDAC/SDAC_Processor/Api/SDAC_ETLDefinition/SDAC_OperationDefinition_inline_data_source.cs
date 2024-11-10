using System.Collections.Generic;

namespace SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition
{
    public class SDAC_OperationDefinition_inline_data_source
    {
        public string endpoint_ref { get; set; }
        public string StatementDefMode { get; set; }
        public string Query { get; set; }
        public string QueryLibrary_provider { get; set; }
        public string[] Query_multiline { get; set; }
        public SDAC_OperationDefinition_sda_parameter[] Query_parameters { get; set; }
        public Dictionary<string, string[]> QueryLibrary_content { get; set; }

       
    }

}
