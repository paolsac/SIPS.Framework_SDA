using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using System.Collections.Generic;

namespace SDAC_Processor.Providers.Operations
{
    public partial class SDAC_ETLOperation_FactoryProvider
    {
        public class State : IFCAutoRegisterSingleton
        {

            public class ProviderDef
            {
                //public string Technology { get; set; }
                public string Name { get; set; }
            }
            private readonly ILogger<State> _logger;
            private readonly IConfiguration _configuration;
            private Dictionary<string, ProviderDef> _providers;
            private readonly object _lock = new object();
            private bool _providersRegistered = false;

            public State(ILogger<State> logger, IConfiguration configuration)
            {
                _logger = logger;
                _configuration = configuration;
                _providers = new Dictionary<string, ProviderDef>();
            }




        }

        
    }
}
