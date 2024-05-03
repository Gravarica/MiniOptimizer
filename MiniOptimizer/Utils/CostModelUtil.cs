using MiniOptimizer.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Utils
{
    public static class CostModelUtil
    {
        public static Dictionary<string, long> TakeDistinctValues(Dictionary<string, long> childValues, string column, long value)
        {
            Dictionary<string, long> keyValuePairs = new Dictionary<string, long>();
            foreach (var childVal in childValues) 
            {
                if (childVal.Key != column) 
                {
                    keyValuePairs[childVal.Key] = childVal.Value;
                }
            }

            keyValuePairs.Add(column, value);
            return keyValuePairs;
        }

        public static Dictionary<string, long> TakeDistinctValuesForProjection(Dictionary<string, long> childValues, HashSet<string> columns)
        {
            Dictionary<string, long> keyValuePairs = new Dictionary<string, long>();

            foreach (var childVal in childValues)
            {
                foreach (var column in columns)
                {
                    var qualifiedName = ParseHelper.ParseQualifiedName(column);
                    if (childVal.Key == qualifiedName.Item2)
                    {
                        keyValuePairs[childVal.Key] = childVal.Value;
                    }
                }
            }

            return keyValuePairs;
        }

        public static Dictionary<string, long> GetDistinctValuesForJoin(Dictionary<string, long> lChildValues,
            Dictionary<string, long> rChildValues,
            string column, 
            long value) {

            Dictionary<string, long> keyValuePairs = new Dictionary<string, long>();

            foreach(var childVal in lChildValues)
            {
                if (childVal.Key != column)
                {
                    keyValuePairs[childVal.Key] = value;
                }
            }

            foreach(var childVal in rChildValues)
            {
                if (childVal.Key != column)
                {
                    if (keyValuePairs.ContainsKey(childVal.Key))
                    {
                        keyValuePairs[childVal.Key] = keyValuePairs[childVal.Key]/2 + childVal.Value/2;
                    } else
                    {
                        keyValuePairs[childVal.Key] = childVal.Value;
                    }
                }
            }

            keyValuePairs[column] = value;
            return keyValuePairs;
        }

        public static Dictionary<string, long> TakeScanDistinctValues(TableStats stats)
        {
            Dictionary<string, long> keyValuePairs = new Dictionary<string, long>();

            foreach(var cStat in stats.ColumnStats)
            {
                keyValuePairs[cStat.Key] = cStat.Value.DistinctValues;
            }

            return keyValuePairs;
        }
    }
}
