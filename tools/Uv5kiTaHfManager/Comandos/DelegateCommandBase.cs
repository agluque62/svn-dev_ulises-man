using System;
using System.Windows.Input;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uv5kiTaHfManager.Comandos
{
    class DelegateCommandBase : ICommand
    {
         private Action<object> _action;
         public DelegateCommandBase(Action<object> action)
         {
             _action = action;
         }

        #region ICommand Members
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }

        #endregion    
    }
}
