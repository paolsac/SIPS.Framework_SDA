namespace SIPS.Framework.SDAC_Processor.Api.SDAC_ETLTask
{
    public enum SDAC_ETLTaskTerminationReasonOptions
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
