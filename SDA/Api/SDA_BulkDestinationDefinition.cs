using System.Data;

namespace SIPS.Framework.SDA.Api
{
    public class SDA_BulkDestinationDefinition
    {
        public enum SDA_BulkSourceTypeOptions
        {
            DataTable,
            DataReader
        }
        public string EndpointName { get; set; }
        public string table { get; set; }
        public SDA_BulkSourceTypeOptions sourceType { get; set; }

        public SDA_ColumnMappingCollections columnMappings { get; set; }
        public SDA_CommandOptions options { get; set; }

        public DataTable dtTable { get; set; }
        public IDataReader dataReader { get; set; }

    }


}
