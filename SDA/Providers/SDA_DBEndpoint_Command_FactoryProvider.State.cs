using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using System.Collections.Generic;

namespace SIPS.Framework.SDA.Providers
{

    public partial class SDA_DBEndpoint_Command_FactoryProvider
    {
        public class State :  IFCAutoRegisterSingleton
        {

            public class ProviderDef
            {
                public string Technology { get; set; }
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

            public ProviderDef GetTechnologyProviderDef(string technology)
            {
                if (!_providersRegistered)
                {
                    var providers = _configuration.GetSection("SIPS_Framework:SDA:Technologies")?.Get<Dictionary<string, ProviderDef>>();
                    if (providers != null)
                    {
                        lock (_lock)
                        {
                            foreach (var provider in providers)
                            {
                                _providers.Add(provider.Key, provider.Value);
                            }
                            _providersRegistered = true;
                        }
                    }
                }
                if (_providers.ContainsKey(technology))
                {
                    return _providers[technology];
                }
                else
                {
                    return null;
                }
            }


        }
    }

}
