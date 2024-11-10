using SIPS.Framework.SDAC_Processor.Api;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLTask;
using SIPS.Framework.SDAC_Processor.Providers.Base;
using System;
using System.Collections.Generic;

namespace SIPS.Framework.SDAC_Processor.Providers.DataFlowTasks
{
    public abstract class SDAC_ETLDataflowTask_BaseProvider : IDisposable
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
        private SDAC_ETLTaskCategoryOptions _taskCategory;
        private SDAC_ETLTaskCompletionModeOptions _completionMode;
        private SDAC_ETLTaskRunStatusOptions _runstatus;
        private SDAC_ETLTaskTerminationReasonOptions _terminationReason;


        public int InstanceId { get; private set; }

        private readonly SDAC_ProvidersCollectionForBaseProvider _sDAC_ProvidersCollection;

        public SDAC_ETLDataflowTask_BaseProvider(SDAC_ProvidersCollectionForBaseProvider sDAC_ProvidersCollection)
        {
            _sDAC_ProvidersCollection = sDAC_ProvidersCollection;
            InstanceId = _sDAC_ProvidersCollection.Counter.GetInstanceId(GetType().Name);
            RunStatus = SDAC_ETLTaskRunStatusOptions.NotStarted;
            TerminationReason = SDAC_ETLTaskTerminationReasonOptions.NotTerminated;
        }

        public string Key { get; set; }
        public string Name { get; set; }
        public string TaskType { get; set; }
        public string ErrorMessage { get; set; }

        public SDAC_ETLTaskCompletionModeOptions CompletionMode { get => _completionMode; }
        public SDAC_ETLTaskCategoryOptions TaskCategory { get => _taskCategory; }
        public SDAC_ETLTaskRunStatusOptions RunStatus
        {
            get {
                lock (_lock)
                {
                    return _runstatus;
                }
            }
            protected set
            {
                lock (_lock)
                {
                    _runstatus = value;
                }
            }
        }

        public SDAC_ETLTaskTerminationReasonOptions TerminationReason
        {
            get {
                lock (_lock)
                {
                    return _terminationReason;
                }
            }
            protected set
            {
                lock (_lock)
                {
                    _terminationReason = value;
                }
            }
        }

        public SDAC_OperationDefinition_data_flow_def_element definition { get; set; }

        public SDAC_Response Setup(SDAC_ETLSourceDefinition def)
        {
            SDAC_Response response = null;
            lock (_lock)
            {
            }

            _taskCategory = GetTaskCategory();
            _completionMode = GetCompletionMode();


            response = SpecificSetup(def);
            if (!response.Success)
            {
                return response;
            }

            response = new SDAC_Response() { Success = true };

            return response;

        }
        public abstract SDAC_Response SpecificSetup(SDAC_ETLSourceDefinition def);
        public abstract SDAC_ETLTaskCategoryOptions GetTaskCategory();
        public abstract SDAC_ETLTaskCompletionModeOptions GetCompletionMode();

        public SDAC_Response HandleDataFlowCancel()
        {
            throw new NotImplementedException();
        }

        public SDAC_Response HandleDataProgress()
        {
            throw new NotImplementedException();
        }

        public abstract SDAC_Response HandleDataResults();

        public abstract void SetExternalParameters(Dictionary<string, object> parameters);

    }

}
