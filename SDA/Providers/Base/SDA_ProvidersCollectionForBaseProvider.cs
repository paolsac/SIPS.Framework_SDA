using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.Core.Providers;

namespace SIPS.Framework.SDA.Providers.Base
{
    public class SDA_ProvidersCollectionForBaseProvider: IFCAutoRegisterTransient
    {
        private readonly InstanceCounterProvider _counter;
        public SDA_ProvidersCollectionForBaseProvider(InstanceCounterProvider counter)
        {
            _counter = counter;
        }

        public InstanceCounterProvider Counter => _counter;
    }
}


