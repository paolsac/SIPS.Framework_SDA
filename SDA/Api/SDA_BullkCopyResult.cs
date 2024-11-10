using System;

namespace SIPS.Framework.SDA.Api
{
    public class SDA_BullkCopyResult
    {
        public bool Success { get; set; }
        public long RowsCopied { get; set; }
        public TimeSpan Duration { get; set; }
        public string ErrorMessage { get; set; }
        public int RowsPerSecond
        {
            get
            {
                return (int)(RowsCopied * 1000.0 / Duration.TotalMilliseconds);
            }
        }
    }


}
