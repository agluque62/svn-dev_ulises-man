using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class PollingHelper
    {
        public bool IsTaskActive(string key)
        {
            return PollingData.ContainsKey(key) ? PollingData[key].Status != TaskStatus.RanToCompletion : false;
        }
        public void SetTask(string key, Task task)
        {
            PollingData[key] = task;
        }
        public void DeleteNotPresent(List<string> keys)
        {
            var NotPresents = PollingData.Where(d => keys.Contains(d.Key) == false).Select(d => d.Key).ToList();
            NotPresents.ForEach(np => PollingData.Remove(np));           
        }
        private Dictionary<string, Task> PollingData = new Dictionary<string, Task>();
    }
}
