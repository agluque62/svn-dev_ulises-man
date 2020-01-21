using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Windows;

using WpfUv5kiNbxSim.Model;

namespace WpfUv5kiNbxSim.ViewModel
{
    class ViewModelMain : WpfUv5kiNbxSim.MvvmFramework.ViewModelBase, IDisposable
    {
       
        System.Threading.SynchronizationContext uiContext = null;
        
        public ViewModelMain()
        {
            bool designTime = DesignerProperties.GetIsInDesignMode(new DependencyObject());
            
            uiContext = System.Threading.SynchronizationContext.Current;
            UlisesNbx.NotifyChange += (msg) =>
            {
                uiContext.Send(x =>
                {
                    if (_mensajes.Count >= 4)
                    {
                        _mensajes.RemoveAt(3);
                    }
                    _mensajes.Insert(0, new LogMessage() { msg = DateTime.Now.ToLongTimeString() + ": " + msg });
                    OnPropertyChanged("Mensajes");
                }, null);
            };

            using (var cfg = new LocalConfig(designTime))
            {
                UlisesNbx.ServerIp = cfg.Config.serverIP;
                UlisesNbx.ServerPort = cfg.Config.serverPort;

                cfg.Config.nbxs.ForEach(nbx =>
                {
                    Nbxs.Add(new UlisesNbx(nbx.ip, nbx.wp));
                });
            }

            AppExit = new MvvmFramework.DelegateCommandBase((obj) =>
            {
                System.Windows.Application.Current.Shutdown();
            });

            AppConfig = new MvvmFramework.DelegateCommandBase((obj) =>
            {

            });
        }

        public void Dispose()
        {
            _nbxs.ForEach(nbx => nbx.Dispose());
            _nbxs.Clear();
        }

        private ObservableCollection<LogMessage> _mensajes = new ObservableCollection<LogMessage>();
        public ObservableCollection<LogMessage> Mensajes { get { return _mensajes; } }
        private List<UlisesNbx> _nbxs = new List<UlisesNbx>();
        public List<UlisesNbx> Nbxs { get { return _nbxs; } }
        public MvvmFramework.DelegateCommandBase AppExit { get; set; }
        public MvvmFramework.DelegateCommandBase AppConfig { get; set; }
    }
}
