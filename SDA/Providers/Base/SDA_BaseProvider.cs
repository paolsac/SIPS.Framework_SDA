using System;

namespace SIPS.Framework.SDA.Providers.Base
{

    public class SDA_BaseProvider: IDisposable
    {
        private readonly SDA_ProvidersCollectionForBaseProvider _sDA_ProvidersCollection;
        private bool disposedValue;

        public int InstanceId { get; private set; }
        public SDA_BaseProvider(SDA_ProvidersCollectionForBaseProvider providers)
        {
            _sDA_ProvidersCollection = providers;
            InstanceId = _sDA_ProvidersCollection.Counter.GetInstanceId(GetType().Name);
        }

        public virtual string ProviderName { get => GetType().Name; }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _sDA_ProvidersCollection.Counter.SetRelease(GetType().Name, InstanceId);
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


