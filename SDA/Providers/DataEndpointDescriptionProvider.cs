using Microsoft.Extensions.Logging;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.Core.Providers;
using SIPS.Framework.SDA.Providers.Base;
using System;

namespace SIPS.Framework.SDA.Providers
{
    public class DataEndpointDescriptionProvider : SDA_BaseProvider, IFCAutoRegisterSingleton
    {
        private int invocationCounter = 0;
        private readonly ILogger<DataEndpointDescriptionProvider> _logger;


        public DataEndpointDescriptionProvider(ILogger<DataEndpointDescriptionProvider> logger
            , SDA_ProvidersCollectionForBaseProvider providers)
            : base(providers)
        {
            _logger = logger;
        }

        public void test()
        {
            invocationCounter++;
            _logger.LogInformation($"Hello, from DataEndpointDescriptionProvider! InstanceId: {InstanceId}, Invocation: {invocationCounter}");
        }
    }
}
