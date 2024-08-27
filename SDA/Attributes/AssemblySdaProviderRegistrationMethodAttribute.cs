using System;

namespace SIPS.Framework.SDA.Attributes
{
    // Definisci un custom attribute chiamato AssemblyInfoAttribute
    [AttributeUsage(AttributeTargets.Method)]
    public class AssemblySdaProviderRegistrationMethodAttribute : Attribute
    {
        public string Information { get; }

        public AssemblySdaProviderRegistrationMethodAttribute(string information = null)
        {
            Information = information;
        }
    }

}
