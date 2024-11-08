namespace SDAC_Processor.Api.SDAC_ETLOperation
{
    public enum SDAC_OperationTerminationReasonOptions
    {
        NotTerminated,
        //Error,
        Cancel,
        Timeout,
        User,
        System,
        Unknown,
        External,
        Internal,
        Exception
    }

}
