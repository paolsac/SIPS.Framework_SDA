using Autofac;
using SIPS.Framework.Core.AutoRegister.Extensions;
using SIPS.Framework.Core.Extensions;
using SIPS.Framework.SDA.Attributes;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SIPS.Framework.SDA.Extensions
{
    public static class SipsSdaContainerBuilderExtensions
    {
        private static bool _isRegistered = false;

        public static ContainerBuilder RegisterSdaProviders(this ContainerBuilder containerBuilder)
        {
            if (_isRegistered)
            {
                return containerBuilder;
            }
            if (containerBuilder == null)
            {
                throw new ArgumentNullException("containerBuilder");
            }

            // Register all providers in the dependency injection container
            containerBuilder.RegisterCoreProviders();

            var thisAssembly = typeof(SipsSdaContainerBuilderExtensions).Assembly;
            SIPSRegistrationToolbox.ConfigureContainer(containerBuilder, thisAssembly, "SDA");

            // Ottieni tutti gli assembly caricati nel dominio corrente

            LoadAndRegisterSDAProviders(containerBuilder);

            _isRegistered = true;
            return containerBuilder;
        }

        private static void LoadAndRegisterSDAProviders(ContainerBuilder containerBuilder)
        {
            // carica dinamicamente gli assembly denominati SDA_xxxx.dll
            string binPath = AppDomain.CurrentDomain.BaseDirectory;

            // crea lista files che abbiano un nome che inizia per SDA_
            string[] files = Directory.GetFiles(binPath, "SDA_*.dll");

            foreach (string file in files)
            {
                Assembly.LoadFrom(file);
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.CustomAttributes.Any(cad => cad.AttributeType == typeof(AssemblySdaProviderContainerAttribute)));

            object[] parameters = new object[] { containerBuilder };
            // Loop attraverso ogni assembly
            foreach (Assembly assembly in assemblies)
            {
                // Puoi anche ottenere i tipi definiti in ogni assembly
                var types = assembly.GetTypes()
                    .Where(t => t.CustomAttributes.Any(cad => cad.AttributeType == typeof(AssemblySdaProviderRegistrationExtensionTypeAttribute)));
                ;
                foreach (Type type in types)
                {
                    // estrai metodi che supportano attributo AssemblySdaProviderRegistrationMethodAttribute
                    var methods = type.GetMethods()
                        .Where(m => m.CustomAttributes.Any(cad => cad.AttributeType == typeof(AssemblySdaProviderRegistrationMethodAttribute)));
                    foreach (MethodInfo method in methods)
                    {
                        method.Invoke(null, parameters);
                    }
                }
            }
        }
    }
}
