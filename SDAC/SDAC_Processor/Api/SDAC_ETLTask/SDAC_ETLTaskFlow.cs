using SIPS.Framework.SDAC_Processor.Providers.DataFlowConnections.Interfaces;
using SIPS.Framework.SDAC_Processor.Providers.DataFlowTasks.Interfaces;
using System.Collections.Generic;

namespace SIPS.Framework.SDAC_Processor.Api.SDAC_ETLTask
{
    public class SDAC_ETLTaskFlow
    {
        private object _lock = new object();
        private Dictionary<string, ISDAC_ETLDataflowTask_Provider> _elements = new Dictionary<string, ISDAC_ETLDataflowTask_Provider>();
        private Dictionary<string, ISDAC_ETLDataflowConnection_Provider> _relationships = new Dictionary<string, ISDAC_ETLDataflowConnection_Provider>();

        internal void AddTask(ISDAC_ETLDataflowTask_Provider instance)
        {
            lock (_lock)
            {
                _elements.Add(instance.Key, instance);
            }
        }

        // get the task by key
        public ISDAC_ETLDataflowTask_Provider GetTask(string key)
        {
            lock (_lock)
            {
                if (_elements.TryGetValue(key, out ISDAC_ETLDataflowTask_Provider task))
                {
                    return task;
                }
            }
            return null;
        }

        // get all tasks as readonly dictionary
        public IReadOnlyDictionary<string, ISDAC_ETLDataflowTask_Provider> GetAllTasks()
        {
            lock (_lock)
            {
                return _elements;
            }
        }

        // add a link between two or more tasks
        internal void AddLink(ISDAC_ETLDataflowConnection_Provider instance)
        {
            lock (_lock)
            {
                _relationships.Add(instance.Key, instance);
            }
        }

        // get the link by key
        public ISDAC_ETLDataflowConnection_Provider GetLink(string key)
        {
            lock (_lock)
            {
                if (_relationships.TryGetValue(key, out ISDAC_ETLDataflowConnection_Provider link))
                {
                    return link;
                }
            }
            return null;
        }

        // get all links as readonly dictionary
        public IReadOnlyDictionary<string, ISDAC_ETLDataflowConnection_Provider> GetAllLinks()
        {
            lock (_lock)
            {
                return _relationships;
            }
        }
    }

}
