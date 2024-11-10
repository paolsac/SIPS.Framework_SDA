using Microsoft.Extensions.Logging;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.Core.Providers;

namespace SIPS.Framework.SDAC_Processor.Providers.Base
{
    public class SDAC_ProvidersCollectionForBaseProvider : IFCAutoRegisterTransient
    {
        private readonly InstanceCounterProvider _counter;
        private readonly ILogger<SDAC_ProvidersCollectionForBaseProvider> _logger;
        public SDAC_ProvidersCollectionForBaseProvider(InstanceCounterProvider counter, ILogger<SDAC_ProvidersCollectionForBaseProvider> logger)
        {
            _counter = counter;
            _logger = logger;
        }

        public InstanceCounterProvider Counter => _counter;

        public ILogger<SDAC_ProvidersCollectionForBaseProvider> BaseLogger => _logger;
    }
}
