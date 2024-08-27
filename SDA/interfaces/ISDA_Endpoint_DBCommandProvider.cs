using System;
using System.Collections.Generic;
using System.Data;

namespace SIPS.Framework.SDA.interfaces
{
    public interface ISDA_Endpoint_DBCommandProvider : IDisposable, ISDA_Endpoint_CommandProvider
    {
        string ConnectionStringName { get; set; }
        string ConnectionString { get; set; }

        void BulkLoadFromDataTableWithoutMap(string tableName, DataTable dtTable);
        void ExecCommand(string query, object parameter = null);
        IEnumerable<T> ReadFromquery<T>(string query, object parameter);
    }
}