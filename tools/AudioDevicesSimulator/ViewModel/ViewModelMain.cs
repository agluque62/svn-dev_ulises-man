using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AudioDevicesSimulator.Model;

namespace AudioDevicesSimulator.ViewModel
{
    class ViewModelMain : AudioDevicesSimulator.MvvmFramework.ViewModelBase
    {
        private SimulatedIao _iao = new SimulatedIao();

        public SimulatedIao Iao
        {
            get
            {
                return _iao;
            }
        }

        public ViewModelMain()
        {
            _iao.NotifyChange += (name) =>            
            {
                OnPropertyChanged("Iao");                
            };
        }
    }
}
