using SIPS.Framework.SDAC_Processor.Api;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLOperation;
using SIPS.Framework.SDAC_Processor.Providers.Base;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.SDA.Api;
using SIPS.Framework.SDA.Providers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using SIPS.Framework.SDAC_Processor.Extensions;

namespace SIPS.Framework.SDAC_Processor.Providers.Operations
{
    public class SDAC_ETLOperation__SqlScriptProvider : SDAC_ETLOperation_BaseProvider, IFCAutoRegisterTransientNamed, ISDAC_ETLOperation_Provider
    {

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

        private readonly object _lock = new object();
        private readonly ILogger<SDAC_ETLOperation__SqlScriptProvider> _logger;
        private SDA_DataSourceDefinition _dataSource;
        private readonly SDA_StatementProcessorProvider _statementProcessorProvider;


        public SDAC_ETLOperation__SqlScriptProvider(SDAC_ProvidersCollectionForBaseProvider sDAC_ProvidersCollection, SDA_StatementProcessorProvider statementProcessorProvider, ILogger<SDAC_ETLOperation__SqlScriptProvider> logger)
            : base(sDAC_ProvidersCollection)
        {
            _statementProcessorProvider = statementProcessorProvider;
            _logger = logger;
        }

        public async Task<SDAC_OperationActionResponse> RunAsync(Dictionary<string, object> parameters, CancellationToken token)
        {
            SDA_Response response;
            try
            {
                _dataSource.OverrideParameters(parameters);
                response = _statementProcessorProvider.Execute(_dataSource);

            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                response = new SDA_Response() { Success = false, ErrorMessage = ex.Message };
            }
            var actionResponse = SDAC_OperationActionResponse.CreateFromSDA_Response(response);
            if (response.Success && response.Value is Dictionary<string, object>)
            {
                (response.Value as Dictionary<string, object>).ToList().ForEach(p => actionResponse.AddOutputParameter(p.Key, p.Value));
            }
            return actionResponse;
        }

        public override SDAC_Response SpecificSetup(SDAC_ETLSourceDefinition def)
        {
            // utilizza ToSDA_DataSourceDefinition per produrre un oggetto SDA_DataSourceDefinition da un oggetto SDAC_ETLSourceDefinition
            SDAC_Response response = definition.sql_script_def.ToSDA_DataSourceDefinition(def);
            if (response.Success)
            {
                _dataSource = (SDA_DataSourceDefinition)response.Value;
            }
            return response;
        }

    }
}
