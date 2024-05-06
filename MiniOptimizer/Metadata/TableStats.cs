using MiniOptimizer.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Metadata
{
    public class TableStats
    {

        public long RowCount { get; set; }

        public int TupleSize { get; set; }

        public int BlockSize { get; set; }

        public bool Clustered { get; set; }
        public Dictionary<string, ColumnStats> ColumnStats { get; set; } = new Dictionary<string, ColumnStats>();
        public Dictionary<Index, IndexStats> IndexStats { get; set; } = new Dictionary<Index, IndexStats>();

        public TableStats() 
        {
            ColumnStats = [];
        }

        public long GetNumberOfBlocks()
        {
            // Returning worst case of number of blocks if it is not clustered
            if (Clustered)
            {
                return TupleSize;
            } else
            {
                return RowCount * BlockSize / TupleSize;
            }
        }

        public long GetDistinctValues(string indexName)
        {
            foreach (var index in IndexStats)
            {
                if (index.Key.Name == indexName)
                {
                    return ColumnStats[index.Key.IndexedColumns.First()].DistinctValues;
                }
                
            }

            throw new BaseException("Specified index doesn't exist.");
        }
    }
}
