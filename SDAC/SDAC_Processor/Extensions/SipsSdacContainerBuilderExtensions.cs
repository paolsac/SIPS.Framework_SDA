using Autofac;
using SIPS.Framework.Core.AutoRegister.Extensions;
using SIPS.Framework.Core.Extensions;
using SIPS.Framework.SDAC_Processor.Attributes;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SIPS.Framework.SDAC_Processor.Extensions
{

    public static class SipsSdacContainerBuilderExtensions
    {
        private static bool _isRegistered = false;

        public static ContainerBuilder RegisterSdacProviders(this ContainerBuilder containerBuilder)
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

            var thisAssembly = typeof(SipsSdacContainerBuilderExtensions).Assembly;
            SIPSRegistrationToolbox.ConfigureContainer(containerBuilder, thisAssembly, "SDAC");

            // Ottieni tutti gli assembly caricati nel dominio corrente

            LoadAndRegisterSDACProviders(containerBuilder);

            _isRegistered = true;
            return containerBuilder;
        }

        private static void LoadAndRegisterSDACProviders(ContainerBuilder containerBuilder)
        {
            // carica dinamicamente gli assembly denominati SDA_xxxx.dll
            string binPath = AppDomain.CurrentDomain.BaseDirectory;

            // crea lista files che abbiano un nome che inizia per SDA_
            string[] files = Directory.GetFiles(binPath, "SDAC_*.dll");

            foreach (string file in files)
            {
                Assembly.LoadFrom(file);
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.CustomAttributes.Any(cad => cad.AttributeType == typeof(AssemblySdacProviderContainerAttribute)));

            object[] parameters = new object[] { containerBuilder };
            // Loop attraverso ogni assembly
            foreach (Assembly assembly in assemblies)
            {
                // Puoi anche ottenere i tipi definiti in ogni assembly
                var types = assembly.GetTypes()
                    .Where(t => t.CustomAttributes.Any(cad => cad.AttributeType == typeof(AssemblySdacProviderRegistrationExtensionTypeAttribute)));
                ;
                foreach (Type type in types)
                {
                    // estrai metodi che supportano attributo AssemblySdaProviderRegistrationMethodAttribute
                    var methods = type.GetMethods()
                        .Where(m => m.CustomAttributes.Any(cad => cad.AttributeType == typeof(AssemblySdacProviderRegistrationMethodAttribute)));
                    foreach (MethodInfo method in methods)
                    {
                        method.Invoke(null, parameters);
                    }
                }
            }
        }
    }
}

