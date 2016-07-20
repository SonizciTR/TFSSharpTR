using ArchiMetrics.Analysis.Common.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.Roslyn.Metrics
{
    public class MetricProject
    {
        public string ProjectName { get; set; }
        public List<INamespaceMetric> Metric { get; set; }
        public string AssemblyName { get; internal set; }
    }
}
