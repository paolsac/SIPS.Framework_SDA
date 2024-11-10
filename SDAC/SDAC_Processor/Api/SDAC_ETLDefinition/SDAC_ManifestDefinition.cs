namespace SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition
{
    public class SDAC_ManifestDefinition
    {
        public string syntax_version { get; set; }
        public string validation_schema { get; set; }
        public string name { get; set; }
        public bool replace_placeholders_before_validation { get; set; }
        public string description { get; set; }
        public string notes { get; set; }
        public string log_level { get; set; }
    }
}
