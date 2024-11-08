using SDAC_Processor.Api;
using SDAC_Processor.Api.SDAC_ETLDefinition;
using SDAC_Processor.Api.SDAC_ETLOperation;
using SDAC_Processor.Providers.Base;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.SDA.Api;
using SIPS.Framework.SDA.Providers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SDAC_Processor.Providers.Operations
{
    public class SDAC_ETLOperation__SqlScriptProvider : SDAC_ETLOperation_BaseProvider, IFCAutoRegisterTransientNamed, ISDAC_ETLOperation_Provider
    {
        private readonly SDA_DataSourceDefinition _dataSource;

        public SDAC_ETLOperation__SqlScriptProvider(SDAC_ProvidersCollectionForBaseProvider sDAC_ProvidersCollection)
            : base(sDAC_ProvidersCollection)
        {
            _dataSource = new SDA_DataSourceDefinition();
        }

        public async Task<SDAC_OperationActionResponse> RunAsync(Dictionary<string, object> parameters, CancellationToken token)
        {
            Task.Delay(1000).Wait();
            return new SDAC_OperationActionResponse() { Succeded = true };
        }

        public override SDAC_Response SpecificSetup(SDAC_ETLSourceDefinition def)
        {
            ;
            // set endpoint
            if (!def.endpoints.TryGetValue(definition.sql_script_def.endpoint_ref, out string endpointName))
            {
                return new SDAC_Response() { Success = false, ErrorMessage = $"Endpoint {definition.sql_script_def.endpoint_ref} not found" };
            }
            _dataSource.EndpointName = endpointName;

            // set StatementDefMode
            _dataSource.StatementDefMode = definition.sql_script_def.StatementDefMode;

            // set Query (mandatory per library)  
            _dataSource.Query = definition.sql_script_def.Query;
            switch (_dataSource.StatementDefMode)
            {
                case "ExplicitSQL":
                    if (_dataSource.Query == null)
                        _dataSource.Query = string.Join(Environment.NewLine, definition.sql_script_def.Query_multiline);
                    break;
                case "ByLibrary":
                    switch (definition.sql_script_def.QueryLibrary_provider)
                    {
                        case "SDA_StatementLibrary_simple":
                            _dataSource.QueryLibrary = new SDA_StatementLibrary_simple(definition.sql_script_def.QueryLibrary_content);
                            break;
                        default:
                            return new SDAC_Response() { Success = false, ErrorMessage = $"QueryLibrary_provider {definition.sql_script_def.QueryLibrary_provider} not supported" };
                    }
                    break;
                default:
                    return new SDAC_Response() { Success = false, ErrorMessage = $"StatementDefMode {definition.sql_script_def.StatementDefMode} not supported" };
            }

            // set PlaceholdersGetter
            _dataSource.PlaceholdersGetter = new Dictionary<string, Func<string>>();

            // set ParametersGetter,
            _dataSource.ParametersGetter = () => null;

            return new SDAC_Response() { Success = true };

        }

        #region Dispose
        private bool disposedValue;


        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: eliminare lo stato gestito (oggetti gestiti)
                }

                // TODO: liberare risorse non gestite (oggetti non gestiti) ed eseguire l'override del finalizzatore
                // TODO: impostare campi di grandi dimensioni su Null
                disposedValue = true;
            }
            base.Dispose(disposing);
        }

        // // TODO: eseguire l'override del finalizzatore solo se 'Dispose(bool disposing)' contiene codice per liberare risorse non gestite
        // ~SDA_DBEndpoint_PGcommandProvider()
        // {
        //     // Non modificare questo codice. Inserire il codice di pulizia nel metodo 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public new void Dispose()
        {
            // Non modificare questo codice. Inserire il codice di pulizia nel metodo 'Dispose(bool disposing)'
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
        #endregion }

    }
}
