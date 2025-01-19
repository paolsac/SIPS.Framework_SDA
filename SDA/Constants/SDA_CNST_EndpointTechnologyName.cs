using Amazon.Runtime;

namespace SIPS.Framework.SDA.Constants
{
    public class SDA_CNST_EndpointTechnologyName : ConstantClass
    {
        public static readonly SDA_CNST_EndpointTechnologyName Undefined = new SDA_CNST_EndpointTechnologyName("Undefined");
        public static readonly SDA_CNST_EndpointTechnologyName SQLServer = new SDA_CNST_EndpointTechnologyName("SQLServer");
        public static readonly SDA_CNST_EndpointTechnologyName PostgreSQL = new SDA_CNST_EndpointTechnologyName("PostgreSQL");
        public static readonly SDA_CNST_EndpointTechnologyName RedShift = new SDA_CNST_EndpointTechnologyName("RedShift");
        public static readonly SDA_CNST_EndpointTechnologyName Oracle = new SDA_CNST_EndpointTechnologyName("Oracle");


        protected SDA_CNST_EndpointTechnologyName(string value) : base(value)
        {
        }

        public static SDA_CNST_EndpointTechnologyName FindValue(string value)
        {
            try
            {
                return ConstantClass.FindValue<SDA_CNST_EndpointTechnologyName>(value);

            }
            catch (System.Exception)
            {
                var type = typeof(SDA_CNST_EndpointTechnologyName);
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
        public static implicit operator SDA_CNST_EndpointTechnologyName(string value)
        {
            return FindValue(value);
        }
    }
}
