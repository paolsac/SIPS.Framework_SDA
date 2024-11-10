using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.Core.Providers;
using SIPS.Framework.SDA.Api;
using SIPS.Framework.SDA.interfaces;
using SIPS.Framework.SDA.Providers.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace SIPS.Framework.SDA_Postgress.Providers
{

    public class SDA_DBEndpoint_PGCommandProvider : SDA_BaseProvider, IFCAutoRegisterTransientNamed, ISDA_Endpoint_DBCommandProvider
    {
        private readonly ILogger<SDA_DBEndpoint_PGCommandProvider> _logger;
        private readonly IConfiguration _configuration;
        private bool disposedValue;
        private readonly Dictionary<Guid, IDbConnection> _openConnections;

        public SDA_DBEndpoint_PGCommandProvider(ILogger<SDA_DBEndpoint_PGCommandProvider> logger,
                                                IConfiguration configuration,
                                                SDA_ProvidersCollectionForBaseProvider providers)
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


        public IEnumerable<T> ReadFromquery<T>(string query, object parameter)
        {
            string connectionString = GetConnectionString();
            using (var connection = new NpgsqlConnection(connectionString))
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
            using (var connection = new NpgsqlConnection(connectionString))
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

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (var writer = connection.BeginBinaryImport($"COPY {tableName} ({columns}) FROM STDIN (FORMAT BINARY)"))
                    {
                        foreach (DataRow row in dtTable.Rows)
                        {
                            writer.StartRow();
                            foreach (DataColumn col in dtTable.Columns)
                            {
                                writer.Write(row[col]);
                            }
                        }
                        writer.Complete();
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

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    if (options != null && options.UseBatch)
                    {
                        int batchSize = options.BatchSize > 0 ? options.BatchSize : options.BacthSizeDefault;
                        long rows = 0;
                        string sql = $"COPY {tableName} ({columns}) FROM STDIN (FORMAT BINARY)";
                        NpgsqlBinaryImporter writer = connection.BeginBinaryImport(sql);
                        try
                        {
                            while (dtReader.Read())
                            {
                                writer.StartRow();
                                for (int i = 0; i < dtReader.FieldCount; i++)
                                {
                                    writer.Write(dtReader[i]);
                                }
                                rows++;
                                if (rows % batchSize == 0)
                                {
                                    writer.Complete();
                                    writer.Dispose();
                                    writer = connection.BeginBinaryImport(sql);
                                }
                            }
                            writer.Complete();
                        }
                        catch (System.Exception ex)
                        {
                            writer.Dispose();
                            throw ex;
                        }
                        finally
                        {
                            writer.Dispose();
                            rowsCopied = rows;
                        }
                    }
                    else
                    {
                        using (var writer = connection.BeginBinaryImport($"COPY {tableName} ({columns}) FROM STDIN (FORMAT BINARY)"))
                        {
                            while (dtReader.Read())
                            {
                                writer.StartRow();
                                for (int i = 0; i < dtReader.FieldCount; i++)
                                {
                                    writer.Write(dtReader[i]);
                                }
                            }
                            writer.Complete();
                        }
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


        public SDA_DataReaderWrapper BeginDataReader(string query, object parameter)
        {
            string connectionString = GetConnectionString();
            var connection = new NpgsqlConnection(connectionString);
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

        public void BulkLoadFromDataTableWithoutMap(string tableName, DataTable dtTable)
        {
            string connectionString = GetConnectionString();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string columns = string.Join(",", dtTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                using (var writer = connection.BeginBinaryImport($"COPY {tableName} ({columns}) FROM STDIN (FORMAT BINARY)"))
                {
                    foreach (DataRow row in dtTable.Rows)
                    {
                        writer.StartRow();
                        foreach (DataColumn col in dtTable.Columns)
                        {
                            writer.Write(row[col]);
                        }
                    }
                    writer.Complete();
                }
            }
        }

        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: eliminare lo stato gestito (oggetti gestiti)
                }

                // TODO: liberare risorse non gestite (oggetti non gestiti) ed eseguire l'override del finalizzatore
                // TODO: impostare campi di grandi dimensioni su Null
                disposedValue = true;
            }
            base.Dispose(disposing);
        }

        // // TODO: eseguire l'override del finalizzatore solo se 'Dispose(bool disposing)' contiene codice per liberare risorse non gestite
        // ~SDA_DBEndpoint_PGcommandProvider()
        // {
        //     // Non modificare questo codice. Inserire il codice di pulizia nel metodo 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public new void Dispose()
        {
            // Non modificare questo codice. Inserire il codice di pulizia nel metodo 'Dispose(bool disposing)'
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
        #endregion
    }

}
