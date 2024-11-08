using SDAC_Processor.Api;
using SDAC_Processor.Api.SDAC_ETLDefinition;
using SDAC_Processor.Api.SDAC_ETLOperation;
using SDAC_Processor.Providers.Base;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SDAC_Processor.Providers.Operations
{
    public class SDAC_ETLOperation__DataFlowProvider : SDAC_ETLOperation_BaseProvider, IFCAutoRegisterTransientNamed, ISDAC_ETLOperation_Provider
    {

        public SDAC_ETLOperation__DataFlowProvider(SDAC_ProvidersCollectionForBaseProvider sDAC_ProvidersCollection) 
            : base(sDAC_ProvidersCollection)
        {
        }

        public async Task<SDAC_OperationActionResponse> RunAsync(Dictionary<string, object> parameters, CancellationToken token)
        {
            Task.Delay(1000).Wait();
            return new SDAC_OperationActionResponse() { Succeded = true };
        }

        public override SDAC_Response SpecificSetup(SDAC_ETLSourceDefinition def)
        {
            throw new System.NotImplementedException();
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
        #endregion

    }

}
