using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.Core.Providers;
using SIPS.Framework.SDA.Api;
using SIPS.Framework.SDA.Constants;
using SIPS.Framework.SDA.Providers.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SIPS.Framework.SDA.Providers
{
    public class SDA_DataSourceProvider : SDA_BaseProvider,  IFCAutoRegisterSingleton
    {
        private readonly ILogger<SDA_DataSourceProvider> _logger;
        private readonly SDA_StatementProcessorProvider _statementProcessorProvider;
        private readonly Func<string, SDA_DataSourceDefinition> _getDS;
        private readonly MemoryCache _cache;
        private readonly object _lock = new object();
        private readonly IConfiguration _configuration;

        public SDA_DataSourceProvider(ILogger<SDA_DataSourceProvider> logger,
                                      SDA_StatementProcessorProvider statementProcessorProvider,
                                      SDA_ProvidersCollectionForBaseProvider providers,
                                      IConfiguration configuration)
            : base(providers)
        {
            _logger = logger;
            _configuration = configuration;
            _statementProcessorProvider = statementProcessorProvider;
            _cache = new MemoryCache(new MemoryCacheOptions());
            
            var dataSourceLibraryEndpointName = _configuration.GetValue<string>($"{ConfigConstants.SDA_ConfigurationFullSectionName}DataSourceLibrary:EndpointName");
            if(dataSourceLibraryEndpointName == null)
            {
                throw new Exception($"{ConfigConstants.SDA_ConfigurationFullSectionName}DataSourceLibrary:EndpointName is not defined in the configuration file");
            }

            var dataSourceLibraryQuery = _configuration.GetValue<string>($"{ConfigConstants.SDA_ConfigurationFullSectionName}DataSourceLibrary:Query");
            if (dataSourceLibraryQuery == null)
            {
                throw new Exception($"{ConfigConstants.SDA_ConfigurationFullSectionName}DataSourceLibrary:Query is not defined in the configuration file");
            }
            _getDS = (string ds_name) => new SDA_DataSourceDefinition()
            {
                EndpointName = dataSourceLibraryEndpointName,
                StatementDefMode = SDA_CNST_StatementDefinitionMode.ExplicitSQL,
                Query = dataSourceLibraryQuery,
                PlaceholdersGetter = new Dictionary<string, Func<string>> { },
                ParametersGetter = () => new { ds_name = ds_name }
            };
            _configuration = configuration;
        }

        private class implicitVarDef
        {
            public string ds_name { get; set; }
            public string ds_def { get; set; }
        }

        private class implicitDataSourceDef
        {
            public string EndpointName { get; set; }
            public string StatementDefMode { get; set; }
            public string Query { get; set; }
            public string[] Query_multiline { get; set; }
            public SDA_CNST_StatementLibaryType QueryLibrary_provider { get; set; }
            public Dictionary<string, string[]> QueryLibrary_content { get; set; }
        }

        public SDA_DataSourceDefinition GetDataSource(string ds_name, Dictionary<string, object> external_parameters = null)
        {
            lock (_lock)
            {
                return _GetDataSource(ds_name, external_parameters);
            }
        }
        private  SDA_DataSourceDefinition _GetDataSource(string ds_name, Dictionary<string, object> external_parameters = null)
        {


            SDA_Response response;

            // Check if the datasource is already in cache
            if (!_cache.TryGetValue(ds_name, out response))
            {
                response = _statementProcessorProvider.Read<implicitVarDef>(_getDS(ds_name));
                if (response.Success)
                    _cache.Set(ds_name, response, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(1)));
                else
                {
                    _logger.LogError("Error reading datasource definition {ds_name}. Error: {error}", ds_name, response.StatusMessage);
                    throw new Exception($"Error reading datasource definition {ds_name}, Error: {response.StatusMessage}");
                }
            }

            List < implicitVarDef > castedValue = response.Value as List<implicitVarDef>;
            if (!response.Success )
            {
                _logger.LogError("Error reading datasource definition {ds_name}. Error: {error}", ds_name, response.StatusMessage);
                throw new Exception($"Error reading datasource definition {ds_name}, Error: {response.StatusMessage}");
            }
            if (response.Success && !castedValue.Any())
            {
                _logger.LogError("Datasource definition {ds_name} not found", ds_name);
                throw new Exception($"Datasource definition {ds_name} not found");
            }

            var ds_def = castedValue.FirstOrDefault()?.ds_def;
            implicitDataSourceDef implicitDataSource = Newtonsoft.Json.JsonConvert.DeserializeObject<implicitDataSourceDef>(ds_def);

            SDA_DataSourceDefinition dataSource = new SDA_DataSourceDefinition()
            {
                EndpointName = implicitDataSource.EndpointName,
                StatementDefMode = implicitDataSource.StatementDefMode,
                Query = implicitDataSource.Query,
                PlaceholdersGetter = new Dictionary<string, Func<string>> { },
                ParametersGetter = () => null
            };

            if(external_parameters != null   )
            {
                var parameters = new DynamicParameters();
                foreach (var parameter in external_parameters)
                {
                    parameters.Add(parameter.Key, parameter.Value);
                }
                dataSource.ParametersGetter = () => parameters;

            }

            if (implicitDataSource.QueryLibrary_provider == SDA_CNST_StatementLibaryType.SDA_StatementLibrary_simple)
            {
                dataSource.QueryLibrary = new SDA_StatementLibrary_simple(implicitDataSource.QueryLibrary_content);
            }
            if (implicitDataSource.StatementDefMode == SDA_CNST_StatementDefinitionMode.ExplicitSQL && implicitDataSource.Query == null)
            {
                dataSource.Query = string.Join(Environment.NewLine, implicitDataSource.Query_multiline);
            }

            return dataSource;
        }

    }
}


