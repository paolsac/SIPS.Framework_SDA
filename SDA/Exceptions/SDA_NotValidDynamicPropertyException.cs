using System;

namespace SIPS.Framework.SDA.Exceptions
{
    public class SDA_NotValidDynamicPropertyException : SDA_Exception
    {
        public SDA_NotValidDynamicPropertyException() { }
        public SDA_NotValidDynamicPropertyException(string message) : base(message) { }
        public SDA_NotValidDynamicPropertyException(string message, Exception inner) : base(message, inner) { }
        protected SDA_NotValidDynamicPropertyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
