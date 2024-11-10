using System;

namespace SIPS.Framework.SDAC_Processor.Attributes
{
    // Definisci un custom attribute chiamato AssemblyInfoAttribute
    [AttributeUsage(AttributeTargets.Class)]
    public class AssemblySdacProviderRegistrationExtensionTypeAttribute : Attribute
    {
        public string Information { get; }

        public AssemblySdacProviderRegistrationExtensionTypeAttribute(string information)
        {
            Information = information;
        }
    }
}
