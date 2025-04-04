using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.SDA.Api;
using SIPS.Framework.SDA.interfaces;
using SIPS.Framework.SDA.Providers.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SIPS.Framework.SDA_SqlServer.Providers
{
    public class SDA_DBEndpoint_SQLServerCommandProvider : SDA_BaseProvider, IFCAutoRegisterTransientNamed, ISDA_Endpoint_DBCommandProvider
    {
        #region Dispose
        override protected void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposedValue)
            {
                if (disposing)
                {

                    disposedValue = true;
                }
            }
        }


        public new void Dispose()
        {
            // Non modificare questo codice. Inserire il codice di pulizia nel metodo 'Dispose(bool disposing)'
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }

        #endregion
        private readonly ILogger<SDA_DBEndpoint_SQLServerCommandProvider> _logger;
        private readonly Dictionary<Guid, IDbConnection> _openConnections;
        private readonly IConfiguration _configuration;
        private bool disposedValue;

        public SDA_DBEndpoint_SQLServerCommandProvider(ILogger<SDA_DBEndpoint_SQLServerCommandProvider> logger, IConfiguration configuration
            , SDA_ProvidersCollectionForBaseProvider providers)
            : base(providers)
        {
            _logger = logger;
            _configuration = configuration;
            _openConnections = new Dictionary<Guid, IDbConnection>();
        }

        public string ConnectionStringName { get; set; }
        public string ConnectionString { get; set; }

        private string GetConnectionString()
        {
            if (ConnectionString != null)
            {
                return ConnectionString;
            }
            if (ConnectionStringName != null)
            {
                throw new System.Exception("Not implemented logic");
            }
            throw new System.Exception("ConnectionString is null");
        }
        //{ 
        //    var parametri = new { Nome = "Mario", Cognome = "Rossi" };
        //    var risultato = connection.Query<Prodotto>("SELECT * FROM Prodotti WHERE Nome = @Nome AND Cognome = @Cognome", parametri);
        //}


        public IEnumerable<T> ReadFromquery<T>(string query, object parameter= null, SDA_CommandOptions options = null)
        {
            string connectionString = GetConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                int? commandTimeout = null;
                if (options != null)
                {
                    if (options.CommandTimeout != SDA_CommandOptions.COMMAND_TIMEOUT_NOT_SET)
                    {
                        commandTimeout = options.CommandTimeout;
                    }
                }

                object param = null;
                if (parameter != null)
                {
                    param = parameter;
                }

                return connection.Query<T>(query, param: param, commandTimeout: commandTimeout);
            }
        }

        public void ExecCommand(string query, object parameter = null, SDA_CommandOptions options = null)
        {
            string connectionString = GetConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                int? commandTimeout = null;
                if (options != null)
                {
                    if (options.CommandTimeout != SDA_CommandOptions.COMMAND_TIMEOUT_NOT_SET)
                    {
                        commandTimeout = options.CommandTimeout;
                    }
                }

                object param = null;
                if (parameter != null)
                {
                    param = parameter;
                }

                connection.Execute(query, param: param, commandTimeout: commandTimeout); 

            }
        }



        public SDA_BullkCopyResult BulkLoadFromDataTable(string tableName, DataTable dtTable, SDA_ColumnMappingCollections columnMappings, SDA_CommandOptions options)
        {
            long rowsCopied = 0;
            DateTime startTime = DateTime.Now;
            try
            {
                string connectionString = GetConnectionString();

                string columns = string.Join(",", dtTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName));

                // if columnMappings is not null, it means that the columns are mapped
                if (columnMappings != null && columnMappings.GetMappings().Any())
                {
                    SDA_ColumnMappingCollections.ColumnMappingType[] supportedMappingTypes =
                        new SDA_ColumnMappingCollections.ColumnMappingType[] {
                            SDA_ColumnMappingCollections.ColumnMappingType.nameToName,
                            SDA_ColumnMappingCollections.ColumnMappingType.indexToName,
                        };
                    if (!supportedMappingTypes.Contains(columnMappings.MappingType))
                    {
                        throw new System.Exception($"Mapping type {columnMappings.MappingType} not supported");
                    }
                    switch (columnMappings.MappingType)
                    {
                        case SDA_ColumnMappingCollections.ColumnMappingType.nameToName:
                            columns = string.Join(",", dtTable.Columns.Cast<DataColumn>().Select(c => columnMappings.GetMappings().First(m => m.SourceColumn == c.ColumnName).DestinationColumn));
                            break;
                        case SDA_ColumnMappingCollections.ColumnMappingType.indexToName:
                            columns = string.Join(",", dtTable.Columns.Cast<DataColumn>().Select(c => columnMappings.GetMappings().First(m => m.SourceColumnIndex == c.Ordinal).DestinationColumn));
                            break;
                    }
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Configurazione di SqlBulkCopy
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        

                        if (columnMappings != null && columnMappings.GetMappings().Any())
                        {
                            foreach (var mapping in columnMappings.GetMappings())
                            {
                                bulkCopy.ColumnMappings.Add(mapping.SourceColumn, mapping.DestinationColumn);
                            }
                        }

                        if (options != null && options.UseBatch)
                        {
                            int batchSize = options.BatchSize > 0 ? options.BatchSize : options.BacthSizeDefault;
                            bulkCopy.BatchSize = batchSize;
                        }

                        // check timeout option
                        if (options != null && options.CommandTimeout != SDA_CommandOptions.COMMAND_TIMEOUT_NOT_SET)
                        {
                            bulkCopy.BulkCopyTimeout = options.CommandTimeout;
                        }

                        bulkCopy.DestinationTableName = tableName; // Nome della tabella di destinazione nel database
                        bulkCopy.WriteToServer(dtTable);
                    }
                }

                rowsCopied = dtTable.Rows.Count;
                return new SDA_BullkCopyResult() { Success = true, RowsCopied = rowsCopied, Duration = DateTime.Now - startTime };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk load from DataTable");
                return new SDA_BullkCopyResult() { Success = false, ErrorMessage = ex.Message };
            }

        }

        public SDA_BullkCopyResult BulkLoadFromDataReader(string tableName, IDataReader dtReader, SDA_ColumnMappingCollections columnMappings, SDA_CommandOptions options)
        {
            long rowsCopied = 0;
            DateTime startTime = DateTime.Now;
            try
            {
                string connectionString = GetConnectionString();

                // stringa  con i nomi delle colonne utilizzando dtreader, elenco concatenato con virgola
                List<string> columnNames = new List<string>();

                for (int i = 0; i < dtReader.FieldCount; i++)
                {
                    columnNames.Add(dtReader.GetName(i));
                }
                string columns = string.Join(",", columnNames);

                // if columnMappings is not null, it means that the columns are mapped
                if (columnMappings != null && columnMappings.GetMappings().Any())
                {
                    SDA_ColumnMappingCollections.ColumnMappingType[] supportedMappingTypes =
                        new SDA_ColumnMappingCollections.ColumnMappingType[] {
                            SDA_ColumnMappingCollections.ColumnMappingType.nameToName,
                            SDA_ColumnMappingCollections.ColumnMappingType.indexToName,
                        };
                    if (!supportedMappingTypes.Contains(columnMappings.MappingType))
                    {
                        throw new System.Exception($"Mapping type {columnMappings.MappingType} not supported");
                    }
                    switch (columnMappings.MappingType)
                    {
                        case SDA_ColumnMappingCollections.ColumnMappingType.nameToName:
                            columns = string.Join(",", columnNames.Select(c => columnMappings.GetMappings().First(m => m.SourceColumn == c).DestinationColumn));
                            break;
                        case SDA_ColumnMappingCollections.ColumnMappingType.indexToName:
                            columns = string.Join(",", columnNames.Select(c => columnMappings.GetMappings().First(m => m.SourceColumnIndex == columnNames.IndexOf(c)).DestinationColumn));
                            break;
                    }
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Configurazione di SqlBulkCopy
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        if (columnMappings != null && columnMappings.GetMappings().Any())
                        {
                            foreach (var mapping in columnMappings.GetMappings())
                            {
                                bulkCopy.ColumnMappings.Add(mapping.SourceColumn, mapping.DestinationColumn);
                            }
                        }

                        if (options != null && options.UseBatch)
                        {
                            int batchSize = options.BatchSize > 0 ? options.BatchSize : options.BacthSizeDefault;
                            bulkCopy.BatchSize = batchSize;
                            bulkCopy.EnableStreaming = true;
                        }

                        // check timeout option
                        if (options != null && options.CommandTimeout != SDA_CommandOptions.COMMAND_TIMEOUT_NOT_SET)
                        {
                            bulkCopy.BulkCopyTimeout = options.CommandTimeout;
                        }

                        bulkCopy.DestinationTableName = tableName; // Nome della tabella di destinazione nel database
                        bulkCopy.WriteToServer(dtReader);
                        rowsCopied = dtReader.RecordsAffected;
                    }
                }
                return new SDA_BullkCopyResult() { Success = true, RowsCopied = rowsCopied, Duration = DateTime.Now - startTime };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk load from DataReader");
                return new SDA_BullkCopyResult() { Success = false, ErrorMessage = ex.Message };
            }
        }

        public void BulkLoadFromDataTableWithoutMap(string tableName, DataTable dataTable, SDA_CommandOptions options = null)
        {
            string connectionString = GetConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // Configurazione di SqlBulkCopy
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {


                    if (options != null && options.UseBatch)
                    {
                        int batchSize = options.BatchSize > 0 ? options.BatchSize : options.BacthSizeDefault;
                        bulkCopy.BatchSize = batchSize;
                        bulkCopy.EnableStreaming = true;
                    }

                    // check timeout option
                    if (options != null && options.CommandTimeout != SDA_CommandOptions.COMMAND_TIMEOUT_NOT_SET)
                    {
                        bulkCopy.BulkCopyTimeout = options.CommandTimeout;
                    }


                    bulkCopy.DestinationTableName = tableName; // Nome della tabella di destinazione nel database
                    bulkCopy.WriteToServer(dataTable);
                }
            }
        }

        public SDA_DataReaderWrapper BeginDataReader(string query, object parameter, SDA_CommandOptions options)
        {
            string connectionString = GetConnectionString();
            var connection = new SqlConnection(connectionString);

            int? commandTimeout = null;
            if (options != null)
            {
                if (options.CommandTimeout != SDA_CommandOptions.COMMAND_TIMEOUT_NOT_SET)
                {
                    commandTimeout = options.CommandTimeout;
                }
            }

            connection.Open();
            IDataReader reader = connection.ExecuteReader(query, param:  parameter, commandTimeout : commandTimeout );
            Guid guid = Guid.NewGuid();
            AddOpenConnection(guid, connection);
            var wrapper = new SDA_DataReaderWrapper(reader, guid);
            return wrapper;
        }

        public void EndDataReader(SDA_DataReaderWrapper wrapper)
        {
            wrapper.Reader.Close();
            wrapper.Reader.Dispose();
            CloseAndRemoveOpenConnection(wrapper.Guid);
        }

        private void AddOpenConnection(Guid guid, IDbConnection connection)
        {
            _openConnections.Add(guid, connection);
        }

        private void CloseAndRemoveOpenConnection(Guid guid)
        {
            IDbConnection connection = null;
            if (!_openConnections.TryGetValue(guid, out connection))
            {
                _logger.LogWarning($"Connection with guid {guid} not found");
                return;
            }

            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
            _openConnections.Remove(guid);
            connection.Dispose();
        }
    }

}
