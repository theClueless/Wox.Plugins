using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wox.Plugin.Test.Common
{
    public static class QueryBuilder
    {
        public static Query Create(string query, string action = "")
        {
            var fullquery = action == "" ? query : $"{action} {query}";
            var splitted = fullquery.Split(' ');
            return new Query(fullquery, query, splitted, action);
        }
    }
}
