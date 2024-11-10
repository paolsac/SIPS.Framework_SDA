using System;

namespace SIPS.Framework.SDAC_Processor.Attributes
{
    // Definisci un custom attribute chiamato AssemblyInfoAttribute
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblySdacProviderContainerAttribute : Attribute
    {
        public string Information { get; }

        public AssemblySdacProviderContainerAttribute(string information)
        {
            Information = information;
        }
    }
}
