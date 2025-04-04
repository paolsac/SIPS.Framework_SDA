using System;

namespace SIPS.Framework.SDA.Exceptions
{
    public class SDA_DatasourceException : SDA_Exception
    {
        public SDA_DatasourceException() { }
        public SDA_DatasourceException(string message) : base(message) { }
        public SDA_DatasourceException(string message, Exception inner) : base(message, inner) { }
        protected SDA_DatasourceException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
