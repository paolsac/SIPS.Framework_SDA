using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using SDAC_Processor.Api;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.SDA.Api;
using SIPS.Framework.SDA.Providers;
using System;
using System.Collections.Generic;

namespace SDAC_Processor.Providers.Script
{
    //todo paolo: add more providers (also for the other technologies)
    //todo paolo: add a provider for the other types of commands

    public class SDAC_ETLScriptSchemaValidator : IFCAutoRegisterTransient
    {
        private readonly object _lock = new object();
        private readonly ILogger<SDAC_ETLScriptSchemaValidator> _logger;
        private readonly IConfiguration _configuration;
        private readonly SDA_DataSourceProvider _dataSourceProvider;
        private readonly SDA_StatementProcessorProvider _statementProcessorProvider;
        private const string SCHEMA_NAME_PREFIX = "ETLScript";

        public SDAC_ETLScriptSchemaValidator(
            ILogger<SDAC_ETLScriptSchemaValidator> logger,
            SDA_DataSourceProvider dataSourceProvider,
            SDA_StatementProcessorProvider statementProcessorProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _dataSourceProvider = dataSourceProvider;
            _statementProcessorProvider = statementProcessorProvider;
            _configuration = configuration;
        }

        public SDAC_Response Validate(string jsonData, string validation_schema)
        {
            string schemaJson = GetETLSourceScriptSchema(validation_schema);
            JSchema schema = JSchema.Parse(schemaJson);
            JObject jsonObject = JObject.Parse(jsonData);

            // Validazione
            bool isValid = jsonObject.IsValid(schema, out IList<string> errorMessages);
            if (isValid)
            {
                return SDAC_Response.CreateSuccess("JSON valido");
            }
            else
            {
                return SDAC_Response.Error("JSON non valido: " + string.Join(", ", errorMessages));
            }
        }

        private string GetETLSourceScriptSchema(string validation_schema)
        {
            var datasourceName = _configuration.GetValue("SIPS_Framework:SDAC:Providers:SDAC_ETLSourceProvider:DataSources:LoadETLValidationSchema", "SDAC.GetETLValidationSchema");
            string schema_name = $"{validation_schema}";
            SDA_DataSourceDefinition ds = _dataSourceProvider.GetDataSource(datasourceName, new Dictionary<string, object> { { "script_name", schema_name } });
            SDA_Response ds_response = _statementProcessorProvider.ReadOneRow<string>(ds);

            if (!ds_response.Success)
            {
                _logger.LogError("Error validation schema {schema_name}: {ErrorMessage}, {StatusMessage}", schema_name, ds_response.ErrorMessage, ds_response.StatusMessage);
                throw new Exception($"Error validation schema : {ds_response.ErrorMessage}, {ds_response.StatusMessage}");
            }

            if (ds_response.Value == null)
            {
                _logger.LogError("Error validation schema {schema_name}: return value is null", schema_name);
                throw new Exception($"{schema_name} does is unknown, please check either table etl_script_library or 'validation_schema' in the manifest");
            }

            return ds_response.Value as string;
        }

    }
}
