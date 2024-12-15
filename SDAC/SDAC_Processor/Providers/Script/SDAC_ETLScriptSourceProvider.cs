using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SIPS.Framework.SDAC_Processor.Api;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition;
using SIPS.Framework.SDAC_Processor.Providers.Base;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.SDA.Api;
using SIPS.Framework.SDA.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SIPS.Framework.SDAC_Processor.Providers.Script
{

    public class SDAC_ETLScriptSourceProvider : SDAC_BaseProvider, IFCAutoRegisterSingleton
    {
        private readonly ILogger<SDAC_ETLScriptSourceProvider> _logger;
        private readonly object _lock = new object();
        private readonly IConfiguration _configuration;
        private readonly SDA_DataSourceProvider _dataSourceProvider;
        private readonly SDA_StatementProcessorProvider _statementProcessorProvider;
        private readonly SDAC_ETLScriptSchemaValidator _ETLSchemaValidator;

        public SDAC_ETLScriptSourceProvider(ILogger<SDAC_ETLScriptSourceProvider> logger,
                                      SDAC_ProvidersCollectionForBaseProvider providers,
                                      IConfiguration configuration,
                                      SDA_DataSourceProvider dataSourceProvider,
                                      SDA_StatementProcessorProvider statementProcessorProvider,
                                      SDAC_ETLScriptSchemaValidator eTLSchemaValidator)
            : base(providers)
        {
            _logger = logger;
            _configuration = configuration;
            _dataSourceProvider = dataSourceProvider;
            _statementProcessorProvider = statementProcessorProvider;
            _ETLSchemaValidator = eTLSchemaValidator;
        }

        public SDAC_Response GetETLSource(string scriptName)
        {
            lock (_lock)
            {
                try
                {
                    var datasourceName = _configuration.GetValue("SIPS_Framework:SDAC:Providers:SDAC_ETLSourceProvider:DataSources:LoadETLSource", "SDAC.GetETLSource");
                    SDA_DataSourceDefinition ds = _dataSourceProvider.GetDataSource(datasourceName,
                        new Dictionary<string, object> { { "script_name", scriptName } }
                        );
                    SDA_Response ds_response = _statementProcessorProvider.ReadOneRow<string>(ds);

                    if (!ds_response.Success)
                    {
                        _logger.LogError("Error loading data source {datasourceName}: {ErrorMessage} {StatusMessage}", datasourceName, ds_response.ErrorMessage, ds_response.StatusMessage);
                        throw new Exception($"{ds_response.ErrorMessage}, {ds_response.StatusMessage}");
                    }

                    string jsonETLDocument = ds_response.Value as string;

                    if (jsonETLDocument == null)
                    {
                        _logger.LogError($"Error loading data source {datasourceName}: return value is null");
                        throw new Exception($"Error loading data source {datasourceName}: return value is null");
                    }

                    SDAC_Response response = null;

                    response = ReadManifest(jsonETLDocument);
                    if (!response.Success)
                    {
                        _logger.LogError("Error loading manifest: {ErrorMessage} {StatusMessage}", response.ErrorMessage, response.StatusMessage);
                        throw new Exception($"Error loading manifest: {response.ErrorMessage},  {response.StatusMessage}");
                    }
                    SDAC_ManifestDefinition manifest = response.Value as SDAC_ManifestDefinition;

/*
                    response = ValidateSchema(jsonETLDocument, manifest);
                    if (!response.Success)
                    {
                        _logger.LogError("Error validating schema: {ErrorMessage} {StatusMessage}", response.ErrorMessage, response.StatusMessage);
                        throw new Exception($"Error validating schema: {response.ErrorMessage} {response.StatusMessage}");
                    }
*/
                    response = ReadDefinition(jsonETLDocument, manifest);
                    if (!response.Success)
                    {
                        _logger.LogError("Error loading definition: {ErrorMessage} {StatusMessage}", response.ErrorMessage, response.StatusMessage);
                        throw new Exception($"Error loading definition: {response.ErrorMessage} {response.StatusMessage}");
                    }

                    return response;

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading data: {ErrorMessage} {StatusMessage}", ex.Message, ex.StackTrace);
                    throw ex;
                }
            }
        }

        private SDAC_Response ValidateSchema(string jsonETLDocument, SDAC_ManifestDefinition manifest)
        {
            lock (_lock)
            {

                Dictionary<string, JsonElement> raw_definition = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonETLDocument);
                try
                {
                    switch (manifest.syntax_version)
                    {
                        case "v01":

                            if (manifest.validation_schema == null)
                            {
                                return new SDAC_Response { Success = false, ErrorMessage = "No validation schema defined, please check 'validation_schema' in manifest" };
                            }

                            SDAC_Response validation = _ETLSchemaValidator.Validate(jsonETLDocument, manifest.validation_schema);
                            if (!validation.Success)
                            {
                                return validation;
                            }

                            return new SDAC_Response { Success = true, Value = validation.StatusMessage };
                        default:
                            throw new NotSupportedException($"Unknown syntax_version {manifest.syntax_version}, please check the manifest in the script");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating schema: {ErrorMessage} {StatusMessage}", ex.Message, ex.StackTrace);
                    return SDAC_Response.Error(ex.Message);
                }
            }
        }

        private SDAC_Response ReadDefinition(string jsonETLDocument, SDAC_ManifestDefinition manifest)
        {
            lock (_lock)
            {
                SDAC_ETLSourceDefinition raw_definition = JsonSerializer.Deserialize<SDAC_ETLSourceDefinition>(jsonETLDocument);
                try
                {
                    switch (manifest.syntax_version)
                    {
                        case "v01":
                            SDAC_Response def = new SDAC_Response { Success = true, Value = raw_definition };

                            return def;
                        default:
                            throw new NotSupportedException($"Unknown syntax_version {manifest.syntax_version}, please check the manifest in the script");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error ReadDefinition: {ErrorMessage} {StatusMessage}", ex.Message, ex.StackTrace);
                    return SDAC_Response.Error(ex.Message);
                }
            }
        }

        private SDAC_Response ReadManifest(string jsonETLDocument)
        {
            lock (_lock)
            {
                SDAC_Response response = new SDAC_Response();
                try
                {
                    SDAC_ETLSourceDefinition_onlyManifest def_manifest = JsonSerializer.Deserialize<SDAC_ETLSourceDefinition_onlyManifest>(jsonETLDocument);
                    Dictionary<string, object> raw_manifest = def_manifest.manifest;
                    if (raw_manifest == null)
                    {
                        response.ErrorMessage = "Error loading manifest: manifest is null";
                        response.StatusMessage = "";
                        response.Success = false;
                        return response;
                    }

                    if (!raw_manifest.ContainsKey("syntax_version"))
                    {
                        response.ErrorMessage = "Error loading data: manifest does not contain syntax_version";
                        response.StatusMessage = "";
                        response.Success = false;
                        return response;
                    }

                    string[] validVersions = new string[] { "v01" };
                    string syntax_version = raw_manifest["syntax_version"].ToString();
                    if (!validVersions.Contains(syntax_version))
                    {
                        response.ErrorMessage = $"syntax_version {raw_manifest["syntax_version"]} is not supported";
                        response.StatusMessage = "";
                        response.Success = false;
                        return response;
                    }

                    SDAC_ManifestDefinition manifest = JsonSerializer.Deserialize<SDAC_ManifestDefinition>(JsonSerializer.Serialize(def_manifest.manifest));

                    return new SDAC_Response
                    {
                        Success = true,
                        Value = manifest
                    };
                }
                catch (Exception ex)
                {
                    response.ErrorMessage = ex.Message;
                    response.StatusMessage = ex.StackTrace;
                    response.Success = false;
                    return response;
                }
            }
        }



    }
}
