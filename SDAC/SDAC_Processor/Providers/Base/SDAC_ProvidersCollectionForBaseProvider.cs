using SDAC_Processor.Api.SDAC_ETLDefinition;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.Core.Providers;
using System;

namespace SDAC_Processor.Providers.Base
{
    public class SDAC_ProvidersCollectionForBaseProvider : IFCAutoRegisterTransient
    {
        private readonly InstanceCounterProvider _counter;
        public SDAC_ProvidersCollectionForBaseProvider(InstanceCounterProvider counter)
        {
            _counter = counter;
        }

        public InstanceCounterProvider Counter => _counter;
    }
}
