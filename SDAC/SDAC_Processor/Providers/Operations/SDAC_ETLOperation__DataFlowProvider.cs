using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Npgsql;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using SIPS.Framework.SDAC_Processor.Api;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLOperation;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLTask;
using SIPS.Framework.SDAC_Processor.Providers.Base;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowConnections;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowConnections.Interfaces;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowTasks;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowTasks.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SIPS.Framework.SDAC_Processor.Providers.Operations
{
    public class SDAC_ETLOperation__DataFlowProvider : SDAC_ETLOperation_BaseProvider, IFCAutoRegisterTransientNamed, ISDAC_ETLOperation_Provider
    {
        private readonly object _lock = new object();
        private readonly ILogger<SDAC_ETLOperation__DataFlowProvider> _logger;
        private readonly SDAC_ETLDataflowTask_FactoryProvider _taskFactory;
        private readonly SDAC_ETLDataflowconnection_FactoryProvider _connectionFactory;
        SDAC_ETLTaskFlow _flow;

        public SDAC_ETLOperation__DataFlowProvider(
            SDAC_ProvidersCollectionForBaseProvider sDAC_ProvidersCollection,
            ILogger<SDAC_ETLOperation__DataFlowProvider> logger,
            SDAC_ETLDataflowTask_FactoryProvider taskFactory,
            SDAC_ETLDataflowconnection_FactoryProvider connectionFactory)
            : base(sDAC_ProvidersCollection)
        {
            _logger = logger;
            _taskFactory = taskFactory;
            _connectionFactory = connectionFactory;
        }

        public async Task<SDAC_OperationActionResponse> RunAsync(Dictionary<string, object> parameters, CancellationToken token)
        {
            try
            {
                _logger.LogWarning("--- RunAsync {data_flow_name}", definition.name);
                foreach (var task in _flow.GetAllTasks())
                {
                    _logger.LogWarning("--- --- Task {task}", task.Key);
                }
                foreach (var relation in _flow.GetAllLinks())
                {
                    _logger.LogWarning("--- --- Relation {relation}", relation.Key);
                }

                // get the source, target and transformation tasks
                ISDAC_ETLDataflowTask_Provider[] sourceTasks = _flow.GetAllTasks().Values.Where(e => e.TaskCategory == SDAC_ETLTaskCategoryOptions.Source).ToArray();
                ISDAC_ETLDataflowTask_Provider[] targetTasks = _flow.GetAllTasks().Values.Where(e => e.TaskCategory == SDAC_ETLTaskCategoryOptions.Destination).ToArray();
                ISDAC_ETLDataflowTask_Provider[] transformationTasks = _flow.GetAllTasks().Values.Where(e => e.TaskCategory == SDAC_ETLTaskCategoryOptions.Transform).ToArray();

                bool abortStart = false;

                // start the source task
                foreach (var task in sourceTasks)
                {
                    if (abortStart)
                    {
                        break;
                    }
                    task.SetExternalParameters(parameters);
                    var response = task.HandleDataFlowStart();
                    if (!response.Success)
                    {
                        _logger.LogError("Source task {task} failed to start: {error}", task.Name, response.ErrorMessage);
                        abortStart = true;
                        break;
                    }
                    _logger.LogInformation("Source task {task} started", task.Name);
                }

                // check if all source tasks are running
                if (!sourceTasks.All(e => e.RunStatus == SDAC_ETLTaskRunStatusOptions.Running))
                {
                    return SDAC_OperationActionResponse.CreateErrorResponse("Failure during start or sources");
                }

                // wait for all source tasks to be running
                _logger.LogInformation("All source tasks started");


                // start the target tasks
                abortStart = false;

                foreach (var task in targetTasks)
                {
                    if (abortStart)
                    {
                        break;
                    }
                    task.SetExternalParameters(parameters);
                    var response = task.HandleDataFlowStart();
                    if (!response.Success)
                    {
                        abortStart = true;
                        break;
                    }
                }

                // check if all target tasks are running
                if (!targetTasks.All(e => e.RunStatus == SDAC_ETLTaskRunStatusOptions.Running 
                    || e.RunStatus== SDAC_ETLTaskRunStatusOptions.Completing
                    || e.RunStatus == SDAC_ETLTaskRunStatusOptions.Completed
                    ))
                {
                    return SDAC_OperationActionResponse.CreateErrorResponse("Failure during start of destinations");
                }

                // wait for all target tasks to be running
                _logger.LogInformation("All target tasks started");

                // enter wait loop for all tasks to complete
                bool allTasksCompleted = false;
                bool anyTaskFailed = false;
                while (!allTasksCompleted && !anyTaskFailed)
                {
                    allTasksCompleted = true;
                    foreach (var task in _flow.GetAllTasks().Values)
                    {
                        // if a task is terminating or terminated, the operation is considered failed
                        if (task.RunStatus == SDAC_ETLTaskRunStatusOptions.Terminating || task.RunStatus == SDAC_ETLTaskRunStatusOptions.Terminated)
                        {
                            anyTaskFailed = true;
                            break;
                        }

                        // source tasks are not significant if they are running
                        if (task.RunStatus == SDAC_ETLTaskRunStatusOptions.Running && task.CompletionMode== SDAC_ETLTaskCompletionModeOptions.Master)
                        {
                            allTasksCompleted = false;
                            break;
                        }
                    }
                    if (anyTaskFailed)
                    {
                        break;
                    }
                    if (!allTasksCompleted)
                    {
                        await Task.Delay(1000);
                    }
                }
                if (anyTaskFailed)
                {
                    RunState = SDAC_OperationRunStateOptions.Terminated;
                    TerminationReason = SDAC_OperationTerminationReasonOptions.Exception;
                    foreach (var task in _flow.GetAllTasks().Values)
                    {
                        if (task.RunStatus == SDAC_ETLTaskRunStatusOptions.Terminating || task.RunStatus == SDAC_ETLTaskRunStatusOptions.Terminated)
                        {
                            SDAC_OperationTerminationReasonOptions reasonOfTerminated;
                            if (Enum.TryParse(task.TerminationReason.ToString(), out reasonOfTerminated))
                            {
                                TerminationReason = reasonOfTerminated;
                            }
                        }
                    }
                    return SDAC_OperationActionResponse.CreateErrorResponse("Failure during run of tasks");
                }
                if (allTasksCompleted)
                {
                    RunState = SDAC_OperationRunStateOptions.Completed;
                }

            }
            catch (Exception ex)
            {
                RunState = SDAC_OperationRunStateOptions.Terminated;
                TerminationReason = SDAC_OperationTerminationReasonOptions.Exception;
                ErrorMessage = ex.Message;
                _logger.LogError(ex, "Error during RunAsync");
                return SDAC_OperationActionResponse.CreateErrorResponse(ex.Message);
            }
            finally
            {
                foreach (var task in _flow.GetAllTasks().Values)
                {
                    task.HandleDataFlowEnd();
                }
            }

            return SDAC_OperationActionResponse.CreateSuccessResponse("Bulk copy completed");


            var connectionStringSS = "Server=tcp:127.0.0.1\\ps19_sqlserver01,1433;Database=sda_test;User Id=sda_user;Password=Gbm??yCHxiSMH8pJ;TrustServerCertificate=True;Connection Timeout=10;";
            var connectionStringPG = "Host = localhost; Database = 'lab_sda'; Port = 5432; Username = 'sda_admin'; Password = '3yqhRL7eQbEM$Fc!'; Include Error Detail='true'; KeepAlive=100; CommandTimeout=300; Timeout=300";

            try
            {

                // open connection to PostgreSQL using connectionStringPG
                using (var connectionPG = new NpgsqlConnection(connectionStringPG))
                {
                    connectionPG.Open();

                    string sql = "SELECT id, nome FROM public.newtable";

                    // create data reader
                    using (var reader = connectionPG.ExecuteReader(sql))
                    {
                        // open connection to SQL Server using connectionStringSS
                        using (var connectionSS = new SqlConnection(connectionStringSS))
                        {
                            connectionSS.Open();

                            // create bulk copy object
                            using (var bulkCopy = new SqlBulkCopy(connectionSS))
                            {
                                bulkCopy.DestinationTableName = "dbo.newtable";

                                // write data to SQL Server
                                bulkCopy.WriteToServer(reader);
                            }
                        }
                    }

                }

                return SDAC_OperationActionResponse.CreateSuccessResponse("Bulk copy completed");



            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk copy");
                return SDAC_OperationActionResponse.CreateErrorResponse(ex.Message);
            }
        }

        public override SDAC_Response SpecificSetup(SDAC_ETLSourceDefinition def)
        {
            SDAC_Response response;

            response = BuildFlow(def);
            if (!response.Success)
            {
                return response;
            }

            _flow = response.Value as SDAC_ETLTaskFlow;
            if (_flow == null)
            {
                return new SDAC_Response() { Success = false, ErrorMessage = "Flow not built" };
            }

            _logger.LogWarning("SpecificSetup");

            return new SDAC_Response() { Success = true };
        }

        private SDAC_Response BuildFlow(SDAC_ETLSourceDefinition def)
        {
            SDAC_ETLTaskFlow flow = new SDAC_ETLTaskFlow();

            _logger.LogWarning("BuildFlow {data_flow_name}", definition.name);

            // create the tasks
            foreach (var element in definition.data_flow_def.elements)
            {
                ISDAC_ETLDataflowTask_Provider instance = _taskFactory.LocateDataFlowTaskProvider(element);
                SDAC_Response r = instance.Setup(def);
                if (!r.Success)
                {
                    return r;
                }
                flow.AddTask(instance);
            }

            // create the links
            foreach (var link in definition.data_flow_def.relationships)
            {
                ISDAC_ETLDataflowConnection_Provider instance = _connectionFactory.LocateDataFlowconnectionProvider(link);
                SDAC_Response r = instance.Setup(def, flow);
                if (!r.Success)
                {
                    return r;
                }
                flow.AddLink(instance);
            }
            return new SDAC_Response() { Success = true, Value = flow };
        }

        #region Dispose
        private bool disposedValue;


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
