using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Windows;

namespace WpfUv5kiNbxSim.ViewModel
{
    class ConfigViewModel : WpfUv5kiNbxSim.MvvmFramework.ViewModelBase, IDisposable
    {
        public ConfigViewModel()
        {            
        }

        public void Dispose()
        {
        }

        bool designTime = DesignerProperties.GetIsInDesignMode(new DependencyObject());
    }
}
