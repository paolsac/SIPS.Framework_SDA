using Dapper;
using SIPS.Framework.SDA.Api;
using SIPS.Framework.SDA.Providers;
using SIPS.Framework.SDAC_Processor.Api;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition;
using SIPS.Framework.SDAC_Processor.Helpers;
using System;
using System.Collections.Generic;
using System.Data;

namespace SIPS.Framework.SDAC_Processor.Extensions
{
    public static class SDAC_OperationDefinition_inline_data_source_Extension
    {
        public static SDAC_Response ToSDA_DataSourceDefinition(this SDAC_OperationDefinition_inline_data_source def, SDAC_ETLSourceDefinition etlSourceDefinition)
        {
            SDA_DataSourceDefinition dataSource = new SDA_DataSourceDefinition();
            // set endpoint
            if (!etlSourceDefinition.endpoints.TryGetValue(def.endpoint_ref, out string endpointName))
            {
                return new SDAC_Response() { Success = false, ErrorMessage = $"Endpoint {def.endpoint_ref} not found" };
            }
            dataSource.EndpointName = endpointName;

            // set StatementDefMode
            dataSource.StatementDefMode = def.StatementDefMode;

            // set Query (mandatory per library)  
            dataSource.Query = def.Query;
            switch (dataSource.StatementDefMode)
            {
                case "ExplicitSQL":
                    if (dataSource.Query == null)
                        dataSource.Query = string.Join(Environment.NewLine, def.Query_multiline);
                    break;
                case "ByLibrary":
                    switch (def.QueryLibrary_provider)
                    {
                        case "SDA_StatementLibrary_simple":
                            dataSource.QueryLibrary = new SDA_StatementLibrary_simple(def.QueryLibrary_content);
                            break;
                        default:
                            return new SDAC_Response() { Success = false, ErrorMessage = $"QueryLibrary_provider {def.QueryLibrary_provider} not supported" };
                    }
                    break;
                default:
                    return new SDAC_Response() { Success = false, ErrorMessage = $"StatementDefMode {def.StatementDefMode} not supported" };
            }

            // set PlaceholdersGetter
            dataSource.PlaceholdersGetter = new Dictionary<string, Func<string>>();

            if (def.Query_parameters != null)
            {

                var parameters = new DynamicParameters();
                foreach (var parameter in def.Query_parameters)
                {
                    ParameterDirection? direction;

                    switch (parameter.direction)
                    {
                        case "Input":
                            direction = ParameterDirection.Input;
                            break;
                        case "Output":
                            direction = ParameterDirection.Output;
                            break;
                        case "InputOutput":
                            direction = ParameterDirection.InputOutput;
                            break;
                        case "ReturnValue":
                            direction = ParameterDirection.ReturnValue;
                            break;
                        default:
                            direction = null;
                            break;
                    }
                    
                    object value = null;
                    if (parameter.value_as_string != null)
                        value = SDAC_ParseHelper.ParseValue(parameter.value_as_string, parameter.data_type);
                    parameters.Add(parameter.name, value: value, dbType: SDAC_ParseHelper.GetDBType(parameter.data_type), direction: direction, size:parameter.size);
                    //if (parameter.value_as_string != null)
                    //    parameters.Add(parameter.name, value: SDAC_ParseHelper.ParseValue(parameter.value_as_string, parameter.data_type), dbType: SDAC_ParseHelper.GetDBType(parameter.data_type), direction: direction);
                    //else
                    //    parameters.Add(parameter.name, value: null, dbType: SDAC_ParseHelper.GetDBType(parameter.data_type), direction: direction);
                }
                dataSource.ParametersGetter = () => parameters;
            }

            return new SDAC_Response() { Success = true, Value = dataSource };
        }

    }

}
