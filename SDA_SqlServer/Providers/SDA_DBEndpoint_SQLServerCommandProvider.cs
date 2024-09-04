using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.SDA.interfaces;
using SIPS.Framework.SDA.Providers.Base;
using System.Collections.Generic;
using System.Data;

namespace SIPS.Framework.SDA_SqlServer.Providers
{
    public class SDA_DBEndpoint_SQLServerCommandProvider : SDA_BaseProvider, IFCAutoRegisterTransientNamed, ISDA_Endpoint_DBCommandProvider
    {
        private readonly ILogger<SDA_DBEndpoint_SQLServerCommandProvider> _logger;
        private readonly IConfiguration _configuration;
        private bool disposedValue;

        public SDA_DBEndpoint_SQLServerCommandProvider(ILogger<SDA_DBEndpoint_SQLServerCommandProvider> logger, IConfiguration configuration
            , SDA_ProvidersCollectionForBaseProvider providers)
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
        //{ 
        //    var parametri = new { Nome = "Mario", Cognome = "Rossi" };
        //    var risultato = connection.Query<Prodotto>("SELECT * FROM Prodotti WHERE Nome = @Nome AND Cognome = @Cognome", parametri);
        //}


        public IEnumerable<T> ReadFromquery<T>(string query, object parameter)
        {
            string connectionString = GetConnectionString();
            using (var connection = new SqlConnection(connectionString))
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
            using (var connection = new SqlConnection(connectionString))
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

        public void BulkLoadFromDataTableWithoutMap(string tableName, DataTable dataTable)
        {
            string connectionString = GetConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // Configurazione di SqlBulkCopy
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = tableName; // Nome della tabella di destinazione nel database
                    bulkCopy.WriteToServer(dataTable);
                }
            }
        }

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
    }

}
