using SIPS.Framework.SDA.Constants;
using SIPS.Framework.SDA.interfaces;
using System;
using System.Collections.Generic;

namespace SIPS.Framework.SDA.Api
{

    public class SDA_DataSourceDefinition
    {
        public string EndpointName { get; set; }
        public string Query { get; set; }
        public SDA_CNST_StatementDefinitionMode StatementDefMode { get;  set; }
        public ISDA_StatementLibrary QueryLibrary { get; set; }
        public Func<object> ParametersGetter { get; set; }
        public Dictionary<string,Func<string>> PlaceholdersGetter { get; set; }
        public Dictionary<string, object> DynamicProperties { get; set; } = new Dictionary<string, object>();
    }


}
