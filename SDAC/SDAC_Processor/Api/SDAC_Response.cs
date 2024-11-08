using SIPS.Framework.Core.Api;
using SIPS.Framework.Core.Interfaces;

namespace SDAC_Processor.Api
{
    public class SDAC_Response : FC_Base_Response, IFCResponse
    {
        public static SDAC_Response CreateSuccess(string message)
        {
            return new SDAC_Response() { StatusMessage = message, Success = true };
        }

        public static SDAC_Response OK
        {
            get
            {
                return new SDAC_Response() { StatusMessage = "OK", Success = true };
            }
        }

        //error response
        public static SDAC_Response Error(string errorMessage, string detail = null)
        {
            return new SDAC_Response() { StatusMessage = detail, Success = false, ErrorMessage = errorMessage };
        }
    }
}
