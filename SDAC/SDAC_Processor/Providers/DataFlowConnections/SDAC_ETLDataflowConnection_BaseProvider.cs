using SIPS.Framework.SDAC_Processor.Api;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLDefinition;
using SIPS.Framework.SDAC_Processor.Api.SDAC_ETLTask;
using SIPS.Framework.SDAC_Processor.Providers.Base;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowTasks.Interfaces;
using System;

namespace SIPS.Framework.SDAC_Processor.Providers.DataFlowConnections
{
    public abstract class SDAC_ETLDataflowConnection_BaseProvider : IDisposable
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

        private readonly SDAC_ProvidersCollectionForBaseProvider _sDAC_ProvidersCollection;

        public SDAC_ETLDataflowConnection_BaseProvider(SDAC_ProvidersCollectionForBaseProvider sDAC_ProvidersCollection)
        {
            _sDAC_ProvidersCollection = sDAC_ProvidersCollection;
            InstanceId = _sDAC_ProvidersCollection.Counter.GetInstanceId(GetType().Name);
        }

        public string Key { get; set; }
        public string Name { get; set; }
        public string ConnectionType { get; set; }

        public SDAC_OperationDefinition_data_flow_def_relationship definition { get; set; }

        public SDAC_Response Setup(SDAC_ETLSourceDefinition def, SDAC_ETLTaskFlow flow)
        {
            SDAC_Response response = null;
            lock (_lock)
            {
                foreach (var task in definition.to)
                {
                    var t = flow.GetTask(task);
                    if (t == null)
                    {
                        return new SDAC_Response() { Success = false, ErrorMessage = $"Task {task} not found in flow" };
                    }
                    response = AddTo(t);
                    if (!response.Success)
                    {
                        return response;
                    }
                }

                foreach (var task in definition.from)
                {
                    var t = flow.GetTask(task);
                    if (t == null)
                    {
                        return new SDAC_Response() { Success = false, ErrorMessage = $"Task {task} not found in flow" };
                    }
                    response = AddFrom(t);
                }
            }

            response = SpecificSetup(def, flow);
            if (!response.Success)
            {
                return response;
            }

            response = new SDAC_Response() { Success = true };

            return response;

        }
        public abstract SDAC_Response SpecificSetup(SDAC_ETLSourceDefinition def, SDAC_ETLTaskFlow flow);
        public abstract SDAC_Response AddFrom(ISDAC_ETLDataflowTask_Provider task);
        public abstract SDAC_Response AddTo(ISDAC_ETLDataflowTask_Provider task);
    }
}
