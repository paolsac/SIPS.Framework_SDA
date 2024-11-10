using SIPS.Framework.SDAC_Processor.Api;
using System;
using System.Data;

namespace SIPS.Framework.SDAC_Processor.Providers.DataFlowConnections.Interfaces
{
    public interface ISDAC_SupportDataReaderOutputs
    {

        SDAC_Response SetupSetterForOutputDataReader(Action<IDataReader> reader_setter);
    }

}
