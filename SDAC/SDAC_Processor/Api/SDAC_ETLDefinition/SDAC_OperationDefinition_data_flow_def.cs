﻿using System.Collections.Generic;

namespace SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition
{
    public class SDAC_OperationDefinition_data_flow_def
    {
        public Dictionary<string, SDAC_OperationDefinition_data_flow_def_element> elements { get; set; }
        public Dictionary<string,SDAC_OperationDefinition_data_flow_def_relationship> relationships { get; set; }
    }
}
