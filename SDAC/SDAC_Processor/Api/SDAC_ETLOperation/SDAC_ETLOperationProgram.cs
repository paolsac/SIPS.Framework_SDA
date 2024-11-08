using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SDAC_Processor.Providers.Operations;

namespace SDAC_Processor.Api.SDAC_ETLOperation
{
    public class SDAC_ETLOperationProgram
    {
        private object _lock = new object();
        private Dictionary<string, ISDAC_ETLOperation_Provider> _operations = new Dictionary<string, ISDAC_ETLOperation_Provider>();

        public string Name { get; set; }

        internal void AddOperation(ISDAC_ETLOperation_Provider instance)
        {
            lock (_lock)
            {
                _operations.Add(instance.Key, instance);
            }
        }

        internal void UpdateReadiness()
        {
            ISDAC_ETLOperation_Provider[] operationsToCheck = _operations.Values
                .Where(operation => operation.Readiness == SDAC_OperationStartReadinessOptions.NotReady)
                .ToArray();
            foreach (ISDAC_ETLOperation_Provider operation in operationsToCheck)
            {
                var state = operation.CheckReadinessStatus(this);
                if (state != SDAC_OperationStartReadinessOptions.NotReady)
                {
                    operation.Readiness = state;
                }
            }
        }

        internal ISDAC_ETLOperation_Provider GetOperation(string operation)
        {
            if (_operations.ContainsKey(operation))
            {
                return _operations[operation];
            }
            throw new Exception($"Operation {operation} not found in program");
        }

        public bool ProgramFinished
        {
            get
            {
                lock (_lock)
                {
                    return _operations.Values.All(op => op.RunState == SDAC_OperationRunStateOptions.Completed || op.Readiness == SDAC_OperationStartReadinessOptions.Unreachable)
                        || _operations.Values.Any(op => op.RunState == SDAC_OperationRunStateOptions.Terminated);
                }
            }
        }

        public ISDAC_ETLOperation_Provider[] OperationsReadyToStart
        {
            get
            {
                lock (_lock)
                {
                    return _operations.Values.Where(op => op.Readiness == SDAC_OperationStartReadinessOptions.Ready && op.RunState == SDAC_OperationRunStateOptions.NotStarted).ToArray();
                }
            }
        }

        public bool HasOperationTerminated
        {
            get
            {
                lock (_lock)
                {
                    return _operations.Values.Any(op => op.RunState == SDAC_OperationRunStateOptions.Terminated);
                }
            }
        }

        public ReadOnlyCollection<ISDAC_ETLOperation_Provider> Operations { get {
                lock (_lock)
                {
                    return new ReadOnlyCollection<ISDAC_ETLOperation_Provider>(_operations.Values.ToList());
                }
            } }
    }

}
