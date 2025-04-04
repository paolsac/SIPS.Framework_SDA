using System;

namespace SIPS.Framework.SDA.Exceptions
{
    public class SDA_Exception : Exception
    {
        public SDA_Exception() { }
        public SDA_Exception(string message) : base(message) { }
        public SDA_Exception(string message, Exception inner) : base(message, inner) { }
        protected SDA_Exception(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
