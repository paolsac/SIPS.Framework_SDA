namespace SIPS.Framework.SDA.Api
{


    public class SDA_DBColumnMapping
    {

        public string SourceColumn { get; set; }
        public string DestinationColumn { get; set; }

        // add column index
        public int SourceColumnIndex { get; set; }
        public int DestinationColumnIndex { get; set; }

        // add column type
        public string SourceColumnType { get; set; }
        public string DestinationColumnType { get; set; }

        // add column length
        public int SourceColumnLength { get; set; }
        public int DestinationColumnLength { get; set; }

        // add column precision
        public int SourceColumnPrecision { get; set; }
        public int DestinationColumnPrecision { get; set; }
        public int SourceColumnScale { get; set; }
        public int DestinationColumnScale { get; set; }

    }

}
