using Microsoft.Extensions.Logging;
using SIPS.Framework.SDAC_Processor.Api;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLOperation;
using SIPS.Framework.SDAC_Processor.Providers.Base;
using SIPS.Framework.SDAC_Processor.Providers.Operations;
using SIPS.Framework.Core.AutoRegister.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConsoleTables;

namespace SIPS.Framework.SDAC_Processor.Providers.Script
{
    public class SDAC_ETLScriptProcessorProvider : SDAC_BaseProvider, IFCAutoRegisterTransient
    {
        private readonly object _lock = new object();
        private readonly ILogger<SDAC_ETLScriptProcessorProvider> _logger;
        private readonly SDAC_ETLOperation_FactoryProvider _operationFactory;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool disposedValue;

        public bool LogError { get; set; }
        public bool LogInformation { get; set; }

        public SDAC_ETLScriptProcessorProvider(ILogger<SDAC_ETLScriptProcessorProvider> logger,
                                               SDAC_ProvidersCollectionForBaseProvider sDAC_ProvidersCollection,
                                               SDAC_ETLOperation_FactoryProvider operationFactory)
            : base(sDAC_ProvidersCollection)
        {
            _logger = logger;
            _cancellationTokenSource = new CancellationTokenSource();
            _operationFactory = operationFactory;
        }

        public SDAC_Response BuildProgram(SDAC_ETLSourceDefinition def)
        {
            SDAC_ETLOperationProgram program = new SDAC_ETLOperationProgram();
            program.Name = def.manifest.name;

            foreach (var operationDef in def.operations)
            {
                // check if the operation is part of program
                if (!def.program_of_operations.Any(e => e.operation == operationDef.Key))
                {
                    continue;
                }

                // if is part the configure and add to program
                ISDAC_ETLOperation_Provider instance = _operationFactory.LocateOperationProvider(operationDef);
                // ISDAC_ETLOperation_Provider instance = SDAC_OperationFactory.CreateOperation(opDef);
                SDAC_Response responseSetup = instance.Setup(def);
                instance.LogActive = LogInformation;
                if (!responseSetup.Success)
                {
                    if (LogError)
                    {
                        _logger.LogError("Build - Operation {OperationName} setup failed, because {ErrorMessage}", instance.Name, responseSetup.ErrorMessage);
                    }
                    return responseSetup;
                }
                program.AddOperation(instance);
            }

            if (LogInformation)
            {
                _logger.LogInformation("Build - Program {ProgramName} created", program.Name);
            }

            foreach (var op in program.Operations)
            {
                if (LogInformation)
                {
                    _logger.LogInformation("Build - Operation {OperationName} added to program {ProgramName}", op.Name, program.Name);
                }
            }

            return new SDAC_Response() { Success = true, Value = program };
        }

        private class TableOperationResult
        {
            public string Operation { get; set; }
            public string Status { get; set; }
            public string Result { get; set; }
            public string Readiness { get; set; }
            public string Error { get; set; }
        }

        public async Task<SDAC_Response> RunProgramAsync(SDAC_ETLOperationProgram program, Dictionary<string, object> parameters)
        {
            CancellationToken cancellationToken = _cancellationTokenSource.Token;
            bool succeded = true;
            if (LogInformation)
            {
                _logger.LogInformation("Run - Program {ProgramName} started -------------------------", program.Name);
            }

            while (!program.ProgramFinished)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Task.Delay(100).Wait();
                    if (LogInformation)
                    {
                        _logger.LogWarning("Run - Program {ProgramName} cancelled", program.Name);
                    }

                    continue;
                }

                if (program.HasOperationTerminated)
                {
                    _logger.LogError("Run - Program {ProgramName} has operation terminated", program.Name);
                    succeded = false;
                    break;
                }

                if (program.HasOperationNotReachable)
                {
                    _logger.LogError("Run - Program {ProgramName} has operation not reachable", program.Name);
                    succeded = false;

                    break;
                }

                program.UpdateReadiness();

                if (program.OperationsReadyToStart.Length == 0)
                {
                    Task.Delay(100).Wait();
                    continue;
                }

                var opToStart = program.OperationsReadyToStart;

                foreach (var op in opToStart)
                {
                    if (op.RunState == SDAC_OperationRunStateOptions.NotStarted)
                        op.RunState = SDAC_OperationRunStateOptions.Running;
                    else
                        throw new System.Exception($"Run - Operation {op.Name} already started");

#pragma warning disable CS4014 // Non è possibile attendere la chiamata, pertanto l'esecuzione del metodo corrente continuerà prima del completamento della chiamata
                    Task.Run(async () =>
                    {
                        try
                        {
                            if (LogInformation)
                            {
                                _logger.LogInformation("Run - Operation {OperationName} starting", op.Name);
                            }

                            SDAC_OperationActionResponse response = await op.RunAsync(parameters, cancellationToken);

                            // merge output parameters
                            if (response.Succeded)
                            {
                                foreach (var key in response.OutputParameters.Keys)
                                {
                                    if (parameters.ContainsKey(key))
                                    {
                                        parameters[key] = response.OutputParameters[key];
                                    }
                                    else
                                    {
                                        parameters.Add(key, response.OutputParameters[key]);
                                    }
                                }
                            }


                            // if the operation is terminated, then stop the program
                            if (cancellationToken.IsCancellationRequested)
                            {
                                op.LockedUpdate(SDAC_OperationTerminationReasonOptions.Cancel, SDAC_OperationRunStateOptions.Terminated, SDAC_OperationCompletionResultOptions.None);
                                if (LogInformation)
                                {
                                    _logger.LogWarning("Run - Operation {OperationName} cancelled", op.Name);
                                }
                                return;
                            }


                            if (response.Succeded)
                            {
                                op.LockedUpdate(SDAC_OperationTerminationReasonOptions.NotTerminated, SDAC_OperationRunStateOptions.Completed, SDAC_OperationCompletionResultOptions.Success);
                            }
                            else
                            {
                                if (LogError)
                                {
                                    _logger.LogError("Run - Operation {OperationName} failed, because {ErrorMessage}, {StatusMessage}", op.Name, response.ErrorMessage, response.StatusMessage);
                                }

                                op.LockedUpdate(SDAC_OperationTerminationReasonOptions.NotTerminated, SDAC_OperationRunStateOptions.Completed, SDAC_OperationCompletionResultOptions.Error);
                                op.CompletionResult = SDAC_OperationCompletionResultOptions.Error;
                            }
                            if (LogInformation)
                            {
                                _logger.LogInformation("Run - Operation {OperationName} completed", op.Name);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            op.LockedUpdate(SDAC_OperationTerminationReasonOptions.Exception, SDAC_OperationRunStateOptions.Terminated, SDAC_OperationCompletionResultOptions.Error);
                            _logger.LogError(ex, "Run - Operation {OperationName} terminated with exception", op.Name);
                        }
                    });

#pragma warning restore CS4014 // Non è possibile attendere la chiamata, pertanto l'esecuzione del metodo corrente continuerà prima del completamento della chiamata
                }


            }
            if (cancellationToken.IsCancellationRequested)
            {
                return new SDAC_Response() { Success = false, ErrorMessage = "Operation cancelled" };
            }

            if (LogInformation)
            {
                _logger.LogInformation("Run - Program {ProgramName} completed -------------------------", program.Name);
            }

            return new SDAC_Response() { Success = succeded };
        }

        override protected void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposedValue)
            {
                if (disposing)
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                    disposedValue = true;
                }
            }
        }
    }
}
