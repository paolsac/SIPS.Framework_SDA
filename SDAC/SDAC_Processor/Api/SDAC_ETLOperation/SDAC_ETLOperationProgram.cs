using SIPS.Framework.SDAC_Processor.Providers.Operations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SIPS.Framework.SDAC_Processor.Api.SDAC_ETLOperation
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
                var newReadiness = operation.CheckReadinessStatus(this);
                if (newReadiness != SDAC_OperationStartReadinessOptions.NotReady)
                {
                    operation.Readiness = newReadiness;
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
                    var c1 = _operations.Values.All(op => op.RunState == SDAC_OperationRunStateOptions.Completed || op.Readiness == SDAC_OperationStartReadinessOptions.Unreachable);
                    var c3 = _operations.Values.Any(op => op.RunState == SDAC_OperationRunStateOptions.Terminated);

                    var result = c1 || c3;
                    //if (result)
                    //{
                    //    Console.WriteLine($"- c1 is {c1}");
                    //    Console.WriteLine($"- c3 is {c3}");
                    //    foreach (var op in _operations.Values)
                    //    {
                    //        Console.WriteLine($"- {op.Name} - {op.RunState} - {op.Readiness} - {op.CompletionResult}");
                    //    }

                    //}
                    return c1 || c3;
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

        public ReadOnlyCollection<ISDAC_ETLOperation_Provider> Operations
        {
            get
            {
                lock (_lock)
                {
                    return new ReadOnlyCollection<ISDAC_ETLOperation_Provider>(_operations.Values.ToList());
                }
            }
        }
    }

}
