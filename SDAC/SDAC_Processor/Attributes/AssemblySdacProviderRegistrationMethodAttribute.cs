using System;

namespace SDAC_Processor.Attributes
{
    // Definisci un custom attribute chiamato AssemblyInfoAttribute
    [AttributeUsage(AttributeTargets.Method)]
    public class AssemblySdacProviderRegistrationMethodAttribute : Attribute
    {
        public string Information { get; }

        public AssemblySdacProviderRegistrationMethodAttribute(string information = null)
        {
            Information = information;
        }
    }
}
