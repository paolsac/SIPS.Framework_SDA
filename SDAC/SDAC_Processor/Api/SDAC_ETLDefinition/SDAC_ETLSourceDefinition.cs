using System.Collections.Generic;

namespace SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition
{

    public class SDAC_ETLSourceDefinition
    {
        public SDAC_ManifestDefinition manifest { get; set; }
        public Dictionary<string, string> endpoints { get; set; }
        public Dictionary<string, SDAC_OperationDefinition> operations { get; set; }
        public List<SDAC_ProgramOfOperationDefinition> program_of_operations { get; set; }
    }
}
