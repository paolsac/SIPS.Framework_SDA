using System.Collections.Generic;

namespace SDAC_Processor.Api.SDAC_ETLDefinition
{
    public class SDAC_ProgramOfOperationDefinition
    {
        public string operation { get; set; }
        public List<SDAC_ProgramOfOperationDefinition_dependency> depends_on { get; set; }
    }
}
