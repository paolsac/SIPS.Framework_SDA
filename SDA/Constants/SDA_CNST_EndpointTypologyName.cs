using Amazon.Runtime;

namespace SIPS.Framework.SDA.Constants
{
    public class SDA_CNST_EndpointTypologyName : ConstantClass
    {
        public static readonly SDA_CNST_EndpointTypologyName Undefined = new SDA_CNST_EndpointTypologyName("Undefined");
        public static readonly SDA_CNST_EndpointTypologyName RelationalDatabase = new SDA_CNST_EndpointTypologyName("RelationalDatabase");
        public static readonly SDA_CNST_EndpointTypologyName File = new SDA_CNST_EndpointTypologyName("File");


        protected SDA_CNST_EndpointTypologyName(string value) : base(value)
        {
        }

        public static SDA_CNST_EndpointTypologyName FindValue(string value)
        {
            try
            {
                return ConstantClass.FindValue<SDA_CNST_EndpointTypologyName>(value);

            }
            catch (System.Exception)
            {
                var type = typeof(SDA_CNST_EndpointTypologyName);
                throw new System.Exception($"Value {value} is not valid for {type.Name}");
            }
        }

        //
        // Riepilogo:
        //     Utility method to convert strings to the constant class.
        //
        // Parametri:
        //   value:
        //     The string value to convert to the constant class.
        public static implicit operator SDA_CNST_EndpointTypologyName(string value)
        {
            return FindValue(value);
        }
    }
}
