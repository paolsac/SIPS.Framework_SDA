using SIPS.Framework.SDA.Constants;

namespace SIPS.Framework.SDA.Api
{

    public class SDA_EndpointDescriptor
    {
        public string Name { get; set; }
        public SDA_CNST_EndpointTypologyName Typology { get; set; }
        public string ConnectionString { get; set; }
        public SDA_CNST_EndpointTechnologyName Technology { get; set; }
    }

}
