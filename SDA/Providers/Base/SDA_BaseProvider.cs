namespace SIPS.Framework.SDA.Providers.Base
{

    public class SDA_BaseProvider
    {
        private readonly SDA_ProvidersCollectionForBaseProvider _sDA_ProvidersCollection;
        public int InstanceId { get; private set; }
        public SDA_BaseProvider(SDA_ProvidersCollectionForBaseProvider providers)
        {
            _sDA_ProvidersCollection = providers;
            InstanceId = _sDA_ProvidersCollection.Counter.GetInstanceId(GetType().Name);
        }

        public virtual string ProviderName { get => GetType().Name; }
    }
}


