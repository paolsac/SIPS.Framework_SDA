using System;

namespace SIPS.Framework.SDA.Attributes
{


    // Definisci un custom attribute chiamato AssemblyInfoAttribute
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblySdaProviderContainerAttribute : Attribute
    {
        public string Information { get; }

        public AssemblySdaProviderContainerAttribute(string information)
        {
            Information = information;
        }
    }

}
