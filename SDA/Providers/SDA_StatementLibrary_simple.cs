using SIPS.Framework.SDA.interfaces;
using SIPS.Framework.SDA.Providers.Base;
using System.Collections.Generic;
using System.Linq;
namespace SIPS.Framework.SDA.Providers
{
    public class SDA_StatementLibrary_simple: ISDA_StatementLibrary
    {
        public string ProviderName { get => this.GetType().Name; }
        private readonly Dictionary<string, string> _queries;

        public SDA_StatementLibrary_simple(Dictionary<string, string> queries)
        {
            _queries = queries;
        }

        public SDA_StatementLibrary_simple(Dictionary<string, string[]> queryLines)
        {
            _queries = queryLines.ToDictionary(kv=>kv.Key, kv=> string.Join(" ", kv.Value));
        }


        public string GetStatement(string statementName)
        {
            if (_queries.ContainsKey(statementName))
            {
                return _queries[statementName];
            }
            else
            {
                return string.Empty;
            }
        }
    }

}
