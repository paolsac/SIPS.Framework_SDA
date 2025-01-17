using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.SDA.Api;
using SIPS.Framework.SDA.Providers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SIPS.Framework.SDA.Providers
{
    public class SDA_EndpointDescriptorProvider : SDA_BaseProvider, IFCAutoRegisterSingleton //  IFCAutoRegisterTransient
    {
        private readonly ILogger<SDA_EndpointDescriptorProvider> _logger;
        private readonly IConfiguration _configuration;
        //private readonly Dictionary<string, SDA_EndpointDescriptor> _endpoints = new Dictionary<string, SDA_EndpointDescriptor>();
        private readonly MemoryCache _cache;
        private readonly object _lock = new object();

        public SDA_EndpointDescriptorProvider(ILogger<SDA_EndpointDescriptorProvider> logger,
                                              IConfiguration configuration,
                                              SDA_ProvidersCollectionForBaseProvider providers)
            : base(providers)
        {
            _logger = logger;
            _configuration = configuration;
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        public SDA_EndpointDescriptor GetEndpoint(string endpointName)
        {
            lock (_lock)
            {
                SDA_EndpointDescriptor endpoint = null;
                if (!_cache.TryGetValue(endpointName, out endpoint))
                {
                    endpoint = RetrieveEndPoint(endpointName);
                    _cache.Set(endpointName, endpoint, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5)));
                }
                return endpoint;
            }
        }

        private SDA_EndpointDescriptor RetrieveEndPoint(string endpointName)
        {
            SDA_EndpointDescriptor endpoint = null;

            var registrationInfo = _configuration.GetSection($"SDA_EndpointDescriptors:{endpointName}").Get<Dictionary<string, string>>();
            if (registrationInfo == null)
            {
                throw new KeyNotFoundException($"Endpoint {endpointName} not found in configuration");
            }
            bool validated = _Validate(registrationInfo, out string invalidReason);
            if (!validated)
            {
                throw new Exception($"Endpoint {endpointName} configuration is not valid. {invalidReason}");
            }

            try
            {
                endpoint = new SDA_EndpointDescriptor()
                {
                    Name = endpointName,
                    Technology = registrationInfo["technology"],
                    Typology = registrationInfo["typology"],
                };

                switch (endpoint.Typology)
                {
                    case "RelationalDatabase":
                        endpoint.ConnectionString = _RetrieveConnectionString(registrationInfo);
                        break;
                    default:
                        throw new NotSupportedException($"Typology {endpoint.Typology} is not supported");
                }


                return endpoint;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private bool _Validate(Dictionary<string, string> registrationInfo, out string invalidReason)
        {
            invalidReason = null;
            if (registrationInfo == null)
            {
                invalidReason = "Registration info is null";
            }
            if (!registrationInfo.ContainsKey("technology"))
            {
                invalidReason = "Technology is missing";
            }
            if (!registrationInfo.ContainsKey("typology"))
            {
                invalidReason = "Typology is missing";
            }
            if (!registrationInfo.ContainsKey("reference_type"))
            {
                invalidReason = "Reference type is missing";
            }
            if (!registrationInfo.ContainsKey("reference"))
            {
                invalidReason = "Reference is missing";
            }
            return invalidReason == null;
        }

        private string _RetrieveConnectionString(Dictionary<string, string> info)
        {
            var referenceType = info["reference_type"];
            switch (referenceType)
            {
                case "structured-app-settings":
                    var reference = info["reference"];
                    var registration_type = _configuration.GetValue<string>($"{info["reference"]}Type");
                    if (string.IsNullOrEmpty(registration_type) )
                    {
                        throw new Exception($"Type is missing for {reference}");
                    }
                    switch (registration_type)
                    {
                        case "SecretManager":
                            return _ReadFromSecretManager(info["technology"], reference);
                        case "ConnectionString":
                            return _ReadConnectionString(info["technology"], reference);
                        default:
                            break;
                    }
                    return info["ConnectionString"];
                default:
                    throw new NotSupportedException($"Reference type {referenceType} not supported");
            }
            throw new NotImplementedException();
        }

        private string _ReadFromSecretManager(string technology, string reference)
        {
            var secretsClient = new AmazonSecretsManagerClient(Amazon.RegionEndpoint.EUWest1);
            var secretId = _configuration.GetValue<string>($"{reference}SecretName");
            if (string.IsNullOrEmpty(secretId))
            {
                throw new Exception($"SecretName is missing for {reference}");
            }
            var secretString = secretsClient.GetSecretValueAsync(new GetSecretValueRequest
            {
                SecretId = secretId
            }).GetAwaiter().GetResult().SecretString;
            Dictionary<string, string> secretJsonRaw = JsonSerializer.Deserialize<Dictionary<string, string>>(secretString);
            Dictionary<string, string> secretJsonToUpper = secretJsonRaw.ToDictionary(kv => kv.Key.ToUpper(), kv => kv.Value);
            var database = _configuration.GetValue<string>($"{reference}DatabaseName");
            if (string.IsNullOrEmpty(database))
            {
                throw new Exception($"DatabaseName is missing for {reference}");
            }


            string host;
            string port;
            string username;
            string password;

            if (!secretJsonToUpper.TryGetValue("HOST", out host))
            {
                throw new System.Exception("Invalid Secret: no host specified");
            }
            if (!secretJsonToUpper.TryGetValue("PORT", out port))
            {
                throw new System.Exception("Invalid Secret: no port specified");
            }
            if (!secretJsonToUpper.TryGetValue("USERNAME", out username))
            {
                throw new System.Exception("Invalid Secret: no username specified");
            }
            if (!secretJsonToUpper.TryGetValue("PASSWORD", out password))
            {
                throw new System.Exception("Invalid Secret: no password specified");
            }


            switch (technology)
            {
                case "PostgreSQL":
                case "Redshift":
                    return $"Host='{host}';Port='{port}';Username='{username}';Password='{password}';Database='{database}';CommandTimeout=600";
                case "SQLServer":
                    return $"Server={host};Database={database};User Id={username};Password={password};Persist Security Info=True; TrustServerCertificate = True";
                default:
                    throw new System.NotSupportedException($"Technology {technology} not supported");
            }

        }
        private string _ReadConnectionString(string technology, string reference)
        {
            var cnstring = _configuration.GetValue<string>($"{reference}Value");

            switch (technology)
            {
                case "PostgreSQL":
                case "Redshift":
                    return cnstring;
                case "SQLServer":
                    return cnstring;
                default:
                    throw new System.NotSupportedException($"Technology {technology} not supported");
            }

        }
    }
}


