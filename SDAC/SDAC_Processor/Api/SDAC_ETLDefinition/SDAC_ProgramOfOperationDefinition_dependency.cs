using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLOperation;

namespace SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition
{
    public class SDAC_ProgramOfOperationDefinition_dependency
    {
        public string operation { get; set; }
        public string when { get; set; }
        public SDAC_OperationCompletionResultOptions when_coded
        {
            get
            {
                if (string.IsNullOrEmpty(when))
                {
                    return SDAC_OperationCompletionResultOptions.None;
                }
                if (when == "succeded")
                {
                    return SDAC_OperationCompletionResultOptions.Success;
                }
                else
                {
                    return SDAC_OperationCompletionResultOptions.Error;
                }
            }
        }
    }
}
