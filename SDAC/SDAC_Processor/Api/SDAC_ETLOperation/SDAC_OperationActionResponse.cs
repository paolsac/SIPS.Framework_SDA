using SIPS.Framework.SDA.Api;

namespace SIPS.Framework.SDAC_Processor.Api.SDAC_ETLOperation
{
    public class SDAC_OperationActionResponse
    {

        public string StatusMessage { get; set; } = string.Empty;
        public string ErrorMessage { get; set; }
        public object Value { get; set; }
        public bool Succeded { get; set; }

        public SDAC_OperationActionResponse()
        {
            Succeded = false;
        }

        static public SDAC_OperationActionResponse CreateSuccessResponse()
        {
            return new SDAC_OperationActionResponse() { Succeded = true };
        }

        static public SDAC_OperationActionResponse CreateErrorResponse(string errorMessage)
        {
            return new SDAC_OperationActionResponse() { Succeded = false, ErrorMessage = errorMessage };
        }

        static public SDAC_OperationActionResponse CreateErrorResponse(string errorMessage, string statusMessage)
        {
            return new SDAC_OperationActionResponse() { Succeded = false, ErrorMessage = errorMessage, StatusMessage = statusMessage };
        }

        static public SDAC_OperationActionResponse CreateErrorResponse(string errorMessage, string statusMessage, object value)
        {
            return new SDAC_OperationActionResponse() { Succeded = false, ErrorMessage = errorMessage, StatusMessage = statusMessage, Value = value };
        }

        static public SDAC_OperationActionResponse CreateErrorResponse(string errorMessage, object value)
        {
            return new SDAC_OperationActionResponse() { Succeded = false, ErrorMessage = errorMessage, Value = value };
        }

        static public SDAC_OperationActionResponse CreateErrorResponse(object value)
        {
            return new SDAC_OperationActionResponse() { Succeded = false, Value = value };
        }

        static public SDAC_OperationActionResponse CreateSuccessResponse(object value)
        {
            return new SDAC_OperationActionResponse() { Succeded = true, Value = value };
        }

        static public SDAC_OperationActionResponse CreateSuccessResponse(string statusMessage, object value)
        {
            return new SDAC_OperationActionResponse() { Succeded = true, StatusMessage = statusMessage, Value = value };
        }

        public static SDAC_OperationActionResponse CreateSuccessResponse(string statusMessage)
        {
            return new SDAC_OperationActionResponse() { Succeded = true, StatusMessage = statusMessage };
        }

        public static SDAC_OperationActionResponse CreateFromSDA_Response(SDA_Response response)
        {
            if (response.Success)
            {
                return CreateSuccessResponse(response.StatusMessage, response.Value);
            }
            else
            {
                return CreateErrorResponse(response.ErrorMessage, response.StatusMessage, response.Value);
            }
        }
    }

}
