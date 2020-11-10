using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class FailuresFilterHelper
    {
        public FailuresFilterHelper(int counters, int consecutiveFails, Action<string> rtTrace)
        {
            InitialValue = new int[counters];
            for (int i = 0; i < counters; i++) InitialValue[i] = consecutiveFails;
            ConsecutiveFailsLimit = consecutiveFails;
            Trace = rtTrace;
        }
        public void LoadOrReload(List<string> sources)
        {
            sources.ForEach(s =>
            {
                // Añadimos los no existentes.
                if (Counters.ContainsKey(s)==false)
                {
                    Counters[s] = new int[InitialValue.Count()];
                    Array.Copy(InitialValue, Counters[s], InitialValue.Count());
                }
            });
            // Borramos los no Existentes.
            var missings = Counters.Keys.Where(k => sources.Contains(k) == false).ToList();
            missings.ForEach(k => Counters.Remove(k));

            Trace($"FFH. LoadOrReload;;;{ToString()}");
        }

        public void FilterFailure(string source, int index, bool lastResult, Action<bool> ExecuteOperation)
        {
            if (Counters.ContainsKey(source) == false || index > Counters[source].Count())
            {
                Trace($"FFH. FilterFailure Error;{source};{index};{ToString()}");
                return;
            }
            var counter = Counters[source];
            counter[index] = lastResult == true ? 0 : (counter[index]) + 1;
            var result = counter[index] >= ConsecutiveFailsLimit ? false : true;
            Trace($"FFH. FilterFailure;{source};{index};{counter[index]};{lastResult};{result}");
            ExecuteOperation(result);
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach(var item in Counters)
            {
                sb.Append($"{item.Key}: {string.Join(",", item.Value)};");
            }
            return sb.ToString();
        }

        private Dictionary<string, int[]> Counters = new Dictionary<string, int[]>();
        private int[] InitialValue;
        private int ConsecutiveFailsLimit = 1;
        private Action<string> Trace = null;
    }
}
