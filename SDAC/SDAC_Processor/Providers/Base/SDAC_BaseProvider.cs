using System;

namespace SDAC_Processor.Providers.Base
{
    public class SDAC_BaseProvider : IDisposable
    {
        private readonly SDAC_ProvidersCollectionForBaseProvider _sDAC_ProvidersCollection;
        private bool disposedValue;

        public int InstanceId { get; private set; }
        public SDAC_BaseProvider(SDAC_ProvidersCollectionForBaseProvider providers)
        {
            _sDAC_ProvidersCollection = providers;
            InstanceId = _sDAC_ProvidersCollection.Counter.GetInstanceId(GetType().Name);
        }

        public virtual string ProviderName { get => GetType().Name; }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _sDAC_ProvidersCollection.Counter.SetRelease(GetType().Name, InstanceId);
                }

                // TODO: liberare risorse non gestite (oggetti non gestiti) ed eseguire l'override del finalizzatore
                // TODO: impostare campi di grandi dimensioni su Null
                disposedValue = true;
            }
        }

        // // TODO: eseguire l'override del finalizzatore solo se 'Dispose(bool disposing)' contiene codice per liberare risorse non gestite
        // ~SDA_BaseProvider()
        // {
        //     // Non modificare questo codice. Inserire il codice di pulizia nel metodo 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Non modificare questo codice. Inserire il codice di pulizia nel metodo 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
