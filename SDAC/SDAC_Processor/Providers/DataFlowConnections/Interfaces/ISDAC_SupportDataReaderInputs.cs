using SIPS.Framework.SDAC_Processor.Api;
using System;
using System.Data;

namespace SIPS.Framework.SDAC_Processor.Providers.DataFlowConnections.Interfaces
{
    public interface ISDAC_SupportDataReaderInputs
    {
        SDAC_Response SetupGetterForInputDataReader(Func<IDataReader> reader_getter);
    }

}
