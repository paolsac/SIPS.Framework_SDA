using Microsoft.Extensions.Logging;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.Core.Providers;
using SIPS.Framework.SDA.Api;
using SIPS.Framework.SDA.Constants;
using SIPS.Framework.SDA.interfaces;
using SIPS.Framework.SDA.Providers.Base;
using System;
using System.Collections.Generic;
namespace SIPS.Framework.SDA.Providers
{

    public class SDA_StatementBuilderProvider : SDA_BaseProvider, IFCAutoRegisterTransient, ISDA_BuilderProvider, ISDA_SupportExtensions<ISDA_StamentBuilderDynamic>
    {
        private readonly ILogger<SDA_StatementBuilderProvider> _logger;
        private readonly Dictionary<string, ISDA_StamentBuilderDynamic> _extensions;

        public SDA_StatementBuilderProvider(ILogger<SDA_StatementBuilderProvider> logger,
                                            SDA_ProvidersCollectionForBaseProvider providers)
            : base(providers)
        {
            _logger = logger;
            _extensions = new Dictionary<string, ISDA_StamentBuilderDynamic>();
        }

        public bool AddExtension(ISDA_StamentBuilderDynamic extension, bool rejectReplace = false, bool exceptionIfReplaced = true)
        {
            if (!_extensions.ContainsKey(extension.Name))
            {
                _extensions.Add(extension.Name, extension);
                return true;
            }
            else
            {
                if (rejectReplace)
                {
                    if (exceptionIfReplaced)
                    {
                        throw new Exception($"Extension {extension.Name} already exists");
                    }
                    return false;
                }
                else
                {
                    _extensions[extension.Name] = extension;
                    return true;
                }
            }
        }

        public string BuildQuery(SDA_DataSourceDefinition dataSourceDefinition, bool applyPlaceholders = true)
        {
            try
            {
                string query = string.Empty;
                var mode = dataSourceDefinition.StatementDefMode;
                switch (mode)
                {
                    case "ExplicitSQL":
                        query = dataSourceDefinition.Query;
                        break;
                    case "ByLibrary":
                        query = dataSourceDefinition.QueryLibrary.GetStatement(dataSourceDefinition.Query);
                        break;
                    default:
                        query = _CreateByDynamicModes(mode, dataSourceDefinition);
                        if (string.IsNullOrEmpty(query))
                        {
                            throw new Exception($"Statement Definition Mode {dataSourceDefinition.StatementDefMode} not supported");
                        }
                        break;
                }
                if (dataSourceDefinition.PlaceholdersGetter != null && applyPlaceholders)
                {
                    var placeholders = dataSourceDefinition.PlaceholdersGetter();
                    foreach (var placeholder in placeholders)
                    {
                        query = query.Replace(placeholder.Key, placeholder.Value);
                    }
                }
                return query;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw ex;
            }
        }

        private string _CreateByDynamicModes(SDA_CNST_StatementDefinitionMode mode, SDA_DataSourceDefinition dataSourceDefinition)
        {
            if (_extensions.ContainsKey(mode.ToString()))
            {
                return _extensions[mode.ToString()].BuildQuery(dataSourceDefinition);
            }
            return string.Empty;
        }
    }
}


