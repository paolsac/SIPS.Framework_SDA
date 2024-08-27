using Amazon.Runtime;

namespace SIPS.Framework.SDA.Constants
{
    public class SDA_CNST_StatementLibaryType: ConstantClass
    {
        public static readonly SDA_CNST_StatementLibaryType Undefined = new SDA_CNST_StatementLibaryType("Undefined");
        public static readonly SDA_CNST_StatementLibaryType SDA_StatementLibrary_simple = new SDA_CNST_StatementLibaryType("SDA_StatementLibrary_simple");


        protected SDA_CNST_StatementLibaryType(string value) : base(value)
        {
        }

        public static SDA_CNST_StatementLibaryType FindValue(string value)
        {
            try
            {
                return ConstantClass.FindValue<SDA_CNST_StatementLibaryType>(value);

            }
            catch (System.Exception)
            {
                var type = typeof(SDA_CNST_StatementLibaryType);
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
        public static implicit operator SDA_CNST_StatementLibaryType(string value)
        {
            return FindValue(value);
        }
    }
}
