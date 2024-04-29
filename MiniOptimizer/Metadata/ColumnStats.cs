using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniOptimizer.Metadata
{
    public class HistogramBucket
    {
        public object LowerBound { get; set; }
        public object UpperBound { get; set; }
        public long RowCount { get; set; }
    }

    public class Histogram
    {
        public List<HistogramBucket> Buckets { get; set; } = new List<HistogramBucket>();

        public Histogram()
        {
            Buckets = new List<HistogramBucket>();
        }
    }

    public class ColumnStats
    {
        public List<int> Values { get; set; } = new List<int>();

        public long DistinctValues { get; set; }
        
        public Histogram Histogram { get; set; }

        public ColumnStats() {
            Histogram = new Histogram();
        }

        // TO DO: 
        // Create Histograms for int and string values
        // Figure out optimal number of buckets to create 
        // Implement Zipfian Distribution
        public void ComputeHistogram()
        {
            int minValue = Values.Min();
            int maxValue = Values.Max();
            int range = (maxValue - minValue) / 10 + 1;

            for (int i = 0; i < 10; i++)
            {
                int lowerBound = minValue + i * range;
                int upperBound = minValue + (i + 1) * range - 1;
                long count = Values.Count(v => v >= lowerBound && v <= upperBound);
                Histogram.Buckets.Add(new HistogramBucket { LowerBound = lowerBound, UpperBound = upperBound, RowCount = count });
            }
        }

        public void PrintHistogram()
        {
            Console.WriteLine("Histogram Data:");
            foreach (var bucket in Histogram.Buckets)
            {
                Console.WriteLine($"Range {bucket.LowerBound} to {bucket.UpperBound}: {bucket.RowCount} records");
            }
        }
    }
}
