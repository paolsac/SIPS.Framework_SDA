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
        public SDA_CNST_StatementDefinitionMode StatementDefMode { get; internal set; }
        public ISDA_StatementLibrary QueryLibrary { get; internal set; }
        public Func<object> ParametersGetter { get; set; }
        public Func<Dictionary<string,string>> PlaceholdersGetter { get; set; }
        public Dictionary<string, object> DynamicProperties { get; set; } = new Dictionary<string, object>();
    }


}
