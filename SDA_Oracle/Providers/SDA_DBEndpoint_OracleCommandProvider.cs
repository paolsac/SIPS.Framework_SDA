using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.SDA.Api;
using SIPS.Framework.SDA.interfaces;
using SIPS.Framework.SDA.Providers.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SIPS.Framework.SDA_Oracle.Providers
{
    public class SDA_DBEndpoint_OracleCommandProvider : SDA_BaseProvider, IFCAutoRegisterTransientNamed, ISDA_Endpoint_DBCommandProvider
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
        private readonly ILogger<SDA_DBEndpoint_OracleCommandProvider> _logger;
        private readonly Dictionary<Guid, IDbConnection> _openConnections;
        private readonly IConfiguration _configuration;
        private bool disposedValue;

        public SDA_DBEndpoint_OracleCommandProvider(ILogger<SDA_DBEndpoint_OracleCommandProvider> logger, IConfiguration configuration
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


        public IEnumerable<T> ReadFromquery<T>(string query, object parameter)
        {
            string connectionString = GetConnectionString();
            using (var connection = new OracleConnection(connectionString))
            {
                connection.Open();
                if (parameter != null)
                {
                    return connection.Query<T>(query, parameter);
                }
                else
                {
                    return connection.Query<T>(query);
                }
            }
        }

        public void ExecCommand(string query, object parameter = null)
        {
            string connectionString = GetConnectionString();
            using (var connection = new OracleConnection(connectionString))
            {
                connection.Open();
                if (parameter == null)
                {
                    connection.Execute(query);
                }
                else
                {
                    connection.Execute(query, parameter);
                }
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

                using (var connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    // Configurazione di SqlBulkCopy
                    using (OracleBulkCopy bulkCopy = new OracleBulkCopy(connection))
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

                using (var connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    // Configurazione di SqlBulkCopy
                    using (OracleBulkCopy bulkCopy = new OracleBulkCopy(connection))
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

        public void BulkLoadFromDataTableWithoutMap(string tableName, DataTable dataTable)
        {
            string connectionString = GetConnectionString();
            using (var connection = new OracleConnection(connectionString))
            {
                connection.Open();
                // Configurazione di SqlBulkCopy
                using (OracleBulkCopy bulkCopy = new OracleBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = tableName; // Nome della tabella di destinazione nel database
                    bulkCopy.WriteToServer(dataTable);
                }
            }
        }

        public SDA_DataReaderWrapper BeginDataReader(string query, object parameter)
        {
            string connectionString = GetConnectionString();
            var connection = new OracleConnection(connectionString);
            connection.Open();
            IDataReader reader = connection.ExecuteReader(query, parameter);
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
