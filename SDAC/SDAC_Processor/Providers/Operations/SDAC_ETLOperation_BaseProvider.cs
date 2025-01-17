using Microsoft.Extensions.Logging;
using SIPS.Framework.SDAC_Processor.Api;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLOperation;
using SIPS.Framework.SDAC_Processor.Providers.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SIPS.Framework.SDAC_Processor.Providers.Operations
{
    public abstract class SDAC_ETLOperation_BaseProvider : IDisposable
    {

        #region Dispose
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _sDAC_ProvidersCollection.Counter.SetRelease(GetType().Name, InstanceId);
                }

                // TODO: liberare risorse non gestite (oggetti non gestiti) ed eseguire l'override del finalizzatore
                // TODO: impostare campi di grandi dimensioni su Null
                disposedValue = true;
            }
        }

        // // TODO: eseguire l'override del finalizzatore solo se 'Dispose(bool disposing)' contiene codice per liberare risorse non gestite
        // ~SDA_BaseProvider()
        // {
        //     // Non modificare questo codice. Inserire il codice di pulizia nel metodo 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Non modificare questo codice. Inserire il codice di pulizia nel metodo 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

        protected object _lock = new object();

        public int InstanceId { get; private set; }

        private SDAC_OperationRunStateOptions _RunState;
        private SDAC_OperationCompletionResultOptions _CompletionResult;
        private SDAC_OperationTerminationReasonOptions _TerminationReason;
        private SDAC_OperationStartReadinessOptions _Readiness;
        private readonly SDAC_ProvidersCollectionForBaseProvider _sDAC_ProvidersCollection;

        public bool LogActive { get; set; }

        public virtual string ProviderName { get => GetType().Name; }

        private List<SDAC_ProgramOfOperationDefinition_dependency> _dependencies;

        public SDAC_ETLOperation_BaseProvider(SDAC_ProvidersCollectionForBaseProvider sDAC_ProvidersCollection)
        {
            _dependencies = new List<SDAC_ProgramOfOperationDefinition_dependency>();
            _sDAC_ProvidersCollection = sDAC_ProvidersCollection;
            InstanceId = _sDAC_ProvidersCollection.Counter.GetInstanceId(GetType().Name);
        }

        public string Key { get; set; }
        public string Name { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
        public SDAC_OperationDefinition definition { get; set; }

        public SDAC_OperationRunStateOptions RunState { get { lock (_lock) return _RunState; } set { lock (_lock) _RunState = value; } }
        public SDAC_OperationCompletionResultOptions CompletionResult { get { lock (_lock) return _CompletionResult; } set { lock (_lock) _CompletionResult = value; } }
        public SDAC_OperationTerminationReasonOptions TerminationReason { get { lock (_lock) return _TerminationReason; } set { lock (_lock) _TerminationReason = value; } }
        public SDAC_OperationStartReadinessOptions Readiness { get { lock (_lock) return _Readiness; } set { lock (_lock) _Readiness = value; } }

        public void LockedUpdate(SDAC_OperationTerminationReasonOptions reason, SDAC_OperationRunStateOptions state, SDAC_OperationCompletionResultOptions result)
        {
            lock (_lock)
            {
                TerminationReason = reason;
                RunState = state;
                CompletionResult = result;
            }
        }

        public abstract SDAC_Response SpecificSetup(SDAC_ETLSourceDefinition def);



        public SDAC_Response Setup(SDAC_ETLSourceDefinition def)
        {
            lock (_lock)
            {
                Readiness = SDAC_OperationStartReadinessOptions.NotReady;
                RunState = SDAC_OperationRunStateOptions.NotStarted;
                CompletionResult = SDAC_OperationCompletionResultOptions.None;
                TerminationReason = SDAC_OperationTerminationReasonOptions.NotTerminated;

                // set the dependencies
                foreach (var op_dep in def.program_of_operations.Where(e => e.operation == Key))
                {

                    // if the operation has dependencies, add them to the list
                    if (op_dep.depends_on == null)
                    {
                        continue;
                    }

                    foreach (var dep in op_dep.depends_on)
                    {
                        _dependencies.Add(new SDAC_ProgramOfOperationDefinition_dependency() { operation = dep.operation, when = dep.when });

                    }
                }

                // configuration specific for each operatioon type
                SDAC_Response response = SpecificSetup(def);
                if (!response.Success)
                {
                    return response;
                }
            }
            return new SDAC_Response() { Success = true };
        }

        public SDAC_OperationStartReadinessOptions CheckReadinessStatus(SDAC_ETLOperationProgram program)
        {
            lock (_lock)
            {
                // if the operation is in NotReady then check, otherwise return the current status
                if (Readiness != SDAC_OperationStartReadinessOptions.NotReady)
                {
                    return Readiness;
                }

                // If there are no dependencies, the operation is ready to start
                if (_dependencies.Count == 0)
                {
                    return SDAC_OperationStartReadinessOptions.Ready;
                }

                // If there are dependencies, check if they are all completed
                // and if they have the expected completion result
                foreach (var dependency in _dependencies)
                {
                    ISDAC_ETLOperation_Provider predecessor = program.GetOperation(dependency.operation);

                    // If the predecessor is not completed, return NotReady
                    if (predecessor.RunState != SDAC_OperationRunStateOptions.Completed)
                    {
                        return SDAC_OperationStartReadinessOptions.NotReady;
                    }

                    // If the predecessor is completed, but the completion result is not set, return NotReady
                    if (predecessor.CompletionResult == SDAC_OperationCompletionResultOptions.None)
                    {
                        return SDAC_OperationStartReadinessOptions.NotReady;
                    }

                    // compare the completion result of the predecessor with the expected result
                    // if they are different, return Unreachable, beacuse the result will not change
                    var when_coded = dependency.when_coded;
                    var completionResult = predecessor.CompletionResult;
                    if (when_coded != completionResult)
                    {
                        
                        _sDAC_ProvidersCollection.BaseLogger.LogWarning($"{ProviderName}- Operation {Name} is not reachable, because predecessor {predecessor.Name} completed as {completionResult} instead of {when_coded}");
                        _sDAC_ProvidersCollection.BaseLogger.LogWarning($"{ProviderName} (live reference)- Operation {Name} is not reachable, because predecessor {predecessor.Name} completed as {predecessor.CompletionResult} instead of {when_coded}");

                        return SDAC_OperationStartReadinessOptions.Unreachable;
                    }
                }

                // If all predecessors are completed and have the expected result, the operation is ready to start
                return SDAC_OperationStartReadinessOptions.Ready;
            }
        }



    }

}
