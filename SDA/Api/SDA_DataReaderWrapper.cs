using System;
using System.Data;

namespace SIPS.Framework.SDA.Api
{
    public class SDA_DataReaderWrapper
    { 
        readonly IDataReader _reader;
        readonly Guid _guid;
        public SDA_DataReaderWrapper(IDataReader reader, Guid guid)
        {
            _reader = reader;
            _guid = guid;
        }
        public IDataReader Reader { get { return _reader; } }
        public Guid Guid { get { return _guid; } }
    }

}
