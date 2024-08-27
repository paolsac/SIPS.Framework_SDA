using System;

namespace SIPS.Framework.SDA.Attributes
{
    // Definisci un custom attribute chiamato AssemblyInfoAttribute
    [AttributeUsage(AttributeTargets.Class)]
    public class AssemblySdaProviderRegistrationExtensionTypeAttribute : Attribute
    {
        public string Information { get; }

        public AssemblySdaProviderRegistrationExtensionTypeAttribute(string information)
        {
            Information = information;
        }
    }

}
