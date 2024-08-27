using SIPS.Framework.Core.Api;
using SIPS.Framework.Core.Interfaces;

namespace SIPS.Framework.SDA.Api
{
    public class SDA_Response: FC_Base_Response, IFCResponse
    {
        public static SDA_Response CreateSuccess(string message)
        {
            return new SDA_Response() { StatusMessage = message, Success = true };
        }

        public static SDA_Response OK
        {
            get
            {
                return new SDA_Response() { StatusMessage = "OK", Success = true };
            }
        }

        //error response
        public static SDA_Response Error(string errorMessage, string detail = null)
        {
            return new SDA_Response() { StatusMessage = detail, Success = false, ErrorMessage = errorMessage };
        }
    }

}
