using MiniOptimizer.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Utils
{
    public class FileGenerator
    {
        public static void GenerateDataFile(string filePath, string[] columns, int rows, int[] startValues, int[] maxValues)
        {
            StringBuilder sb = new StringBuilder();
            Random random = new Random();

            sb.AppendLine(string.Join(" ", columns));

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns.Length; j++)
                {
                    sb.Append(random.Next(startValues[j], maxValues[j]));
                    if (j < columns.Length - 1)
                        sb.Append(" ");
                    else
                        sb.AppendLine();
                }
            }

            File.WriteAllText(filePath, sb.ToString());
        }

        public static Table CreateTableFromFile(string filePath, bool clustered, List<string> indexedColumns)
        {
            string tableName = Path.GetFileNameWithoutExtension(filePath);
            var table = new Table(tableName);
            string[] columns = [];

            var lines = File.ReadAllLines(filePath);
            Dictionary<string, HashSet<int>> distinctValues = new Dictionary<string, HashSet<int>>();
            if (lines.Length > 0)
            {
                string header = lines[0];
                columns = header.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var column in columns)
                {
                    bool isPrimaryKey = column.EndsWith("+");
                    string columnName = isPrimaryKey ? column.TrimEnd('+') : column;
                    table.AddColumn(new Column(columnName, isPrimaryKey));
                    distinctValues[columnName] = new HashSet<int>();
                }
            }

            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                int columnIndex = 0;
                foreach (var value in values)
                {
                    string columnName = table.Columns.Keys.ElementAt(columnIndex);
                    int intValue = int.Parse(value);
                    if (!table.Statistics.ColumnStats.ContainsKey(columnName))
                        table.Statistics.ColumnStats[columnName] = new ColumnStats();

                    table.Statistics.ColumnStats[columnName].Values.Add(intValue);
                    distinctValues[columnName].Add(intValue);
                    columnIndex++;
                }
                table.Statistics.RowCount++;
                table.Statistics.TupleSize = columnIndex * 4 + 20; // INT_SIZE = 4 BYTES | HEADER_SIZE = 20 BYTES
                table.Statistics.BlockSize = 4096; // BLOCK_SIZE = 4KB
                table.Statistics.Clustered = clustered;
            }

            for (int i = 0; i < table.Columns.Count; i++)
            {
                string columnName = table.Columns.Keys.ElementAt(i);
                table.Statistics.ColumnStats[columnName].DistinctValues = distinctValues[columnName].Count;
                table.Statistics.ColumnStats[columnName].ComputeHistogram();
            }

            var indexName = CreateIndexName(table.Name, indexedColumns);
            Metadata.Index index = new Metadata.Index(indexName, indexedColumns, table);

            return table;
        }

        public static string CreateIndexName(string tableName, List<string> indexedColumns)
        {
            var indexName = "PK_Index_" + tableName;
            foreach (string columnName in indexedColumns)
            {
                indexName += "_" + columnName;
            }
            return indexName;
        }
    }
}
