using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TradingStrategy;
namespace TradingStrategyEvaluation
{
    internal sealed class StandardGroupRuntimeMetricManager 
        : IGroupRuntimeMetricManager
        , IRuntimeMetricManagerObserver
    {
        private IRuntimeMetricManager _manager;

        private List<IGroupRuntimeMetric> _metrics = new List<IGroupRuntimeMetric>();

        private List<int[]> _dependedMetrics = new List<int[]>();

        private List<int[]> _tradingObjects = new List<int[]>();

        public StandardGroupRuntimeMetricManager(IRuntimeMetricManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException();
            }

            _manager = manager;
        }

        public int RegisterMetric(IGroupRuntimeMetric metric)
        {
            if (metric == null)
            {
                throw new ArgumentNullException();
            }

            var dependedMetricIndices = metric.DependedRawMetrics
                .Select(s => _manager.RegisterMetric(s))
                .ToArray();

            _dependedMetrics.Add(dependedMetricIndices);

            var tradingObjectIndices = metric.TradingObjects
                .Select(t => t.Index)
                .ToArray();

            _tradingObjects.Add(tradingObjectIndices);

            _metrics.Add(metric);

            return _metrics.Count - 1;
        }

        public IGroupRuntimeMetric GetMetric(int index)
        {
            if (index < 0 || index >= _metrics.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _metrics[index];
        }

        public void Observe(IRuntimeMetricManager manager)
        {
            UpdateMetrics(manager);
        }

        /// <summary>
        /// Update all registered group runtime metric according to the value of depended metric value
        /// </summary>
        /// <param name="manager">the runtime metric manager which manages all metrics depended by the group runtime metrics</param>
        private void UpdateMetrics(IRuntimeMetricManager manager)
        {
            if (manager != _manager)
            {
                throw new InvalidProgramException();
            }

            for (int i = 0; i < _metrics.Count; ++i)
            {
                IGroupRuntimeMetric metric = _metrics[i];
                int[] tradingObjectIndices = _tradingObjects[i];
                int[] dependedMetricIndices = _dependedMetrics[i];

                if (dependedMetricIndices.Length == 0)
                {
                    continue;
                }

                IRuntimeMetric[][] runtimeMetrics = new IRuntimeMetric[dependedMetricIndices.Length][];
                for (int j = 0; j < runtimeMetrics.Length; ++j)
                {
                    IRuntimeMetric[] fullMetrics = manager.GetMetrics(dependedMetricIndices[j]);

                    IRuntimeMetric[] subMetrics = new IRuntimeMetric[tradingObjectIndices.Length];

                    for (int k = 0; k < tradingObjectIndices.Length; ++k)
                    {
                        subMetrics[k] = fullMetrics[tradingObjectIndices[k]];
                    }

                    runtimeMetrics[j] = subMetrics;
                }

                // update metric
                metric.Update(runtimeMetrics);
            }
        }
    }
}
