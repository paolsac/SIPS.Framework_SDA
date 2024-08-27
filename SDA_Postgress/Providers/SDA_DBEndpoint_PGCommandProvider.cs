using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.Core.Providers;
using SIPS.Framework.SDA.interfaces;
using SIPS.Framework.SDA.Providers.Base;
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

        public SDA_DBEndpoint_PGCommandProvider(ILogger<SDA_DBEndpoint_PGCommandProvider> logger,
                                                IConfiguration configuration,
                                                SDA_ProvidersCollectionForBaseProvider providers)
            : base(providers)
        {
            _logger = logger;
            _configuration = configuration;
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

        protected virtual void Dispose(bool disposing)
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
        }

        // // TODO: eseguire l'override del finalizzatore solo se 'Dispose(bool disposing)' contiene codice per liberare risorse non gestite
        // ~SDA_DBEndpoint_PGcommandProvider()
        // {
        //     // Non modificare questo codice. Inserire il codice di pulizia nel metodo 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Non modificare questo codice. Inserire il codice di pulizia nel metodo 'Dispose(bool disposing)'
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
        #endregion
    }

}
