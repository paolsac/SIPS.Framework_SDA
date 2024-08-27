using Amazon.Runtime;

namespace SIPS.Framework.SDA.Constants
{
    public class SDA_CNST_StatementDefinitionMode: ConstantClass
    {
        public static readonly SDA_CNST_StatementDefinitionMode Undefined = new SDA_CNST_StatementDefinitionMode("Undefined");
        public static readonly SDA_CNST_StatementDefinitionMode ExplicitSQL = new SDA_CNST_StatementDefinitionMode("ExplicitSQL");
        public static readonly SDA_CNST_StatementDefinitionMode ByLibrary = new SDA_CNST_StatementDefinitionMode("ByLibrary");


        protected SDA_CNST_StatementDefinitionMode(string value) : base(value)
        {
        }

        public static SDA_CNST_StatementDefinitionMode FindValue(string value)
        {
            try
            {
                return ConstantClass.FindValue<SDA_CNST_StatementDefinitionMode>(value);

            }
            catch (System.Exception)
            {
                var type = typeof(SDA_CNST_StatementDefinitionMode);
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
        public static implicit operator SDA_CNST_StatementDefinitionMode(string value)
        {
            return FindValue(value);
        }
    }
}
