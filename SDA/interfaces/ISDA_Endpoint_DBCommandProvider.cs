using SIPS.Framework.SDA.Api;
using System;
using System.Collections.Generic;
using System.Data;

namespace SIPS.Framework.SDA.interfaces
{
    public interface ISDA_Endpoint_DBCommandProvider : IDisposable, ISDA_Endpoint_CommandProvider
    {
        string ConnectionStringName { get; set; }
        string ConnectionString { get; set; }

        SDA_BullkCopyResult BulkLoadFromDataTable(string tableName, DataTable dtTable, SDA_ColumnMappingCollections columnMappings, SDA_CommandOptions options);
        SDA_BullkCopyResult BulkLoadFromDataReader(string tableName, IDataReader dtReader, SDA_ColumnMappingCollections columnMappings, SDA_CommandOptions options);
        void BulkLoadFromDataTableWithoutMap(string tableName, DataTable dataTable, SDA_CommandOptions options=null);

        void ExecCommand(string query, object parameter = null, SDA_CommandOptions options=null);
        IEnumerable<T> ReadFromquery<T>(string query, object parameter = null, SDA_CommandOptions options = null);
        SDA_DataReaderWrapper BeginDataReader(string query, object parameter = null, SDA_CommandOptions options = null);
        void EndDataReader(SDA_DataReaderWrapper wrapper);
    }
}