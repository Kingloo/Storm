using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Storm
{
    abstract class Command : ViewModelBase, ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        protected virtual void OnCanExecuteChanged()
        {
            Disp.BeginInvoke((ThreadStart)OnCanExecuteChanged, DispatcherPriority.Normal);
        }

        public abstract void Execute(object parameter);
        public abstract bool CanExecute(object parameter);
    }

    class DelegateTaskCommand : Command
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public DelegateTaskCommand(Action<object> execute, Predicate<object> canExecute)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }

        public override async void Execute(object parameter)
        {
            await Task.Factory.StartNew(this._execute, parameter);
        }

        public override bool CanExecute(object parameter)
        {
            if (this._canExecute != null)
            {
                return this._canExecute(parameter);
            }
            else
            {
                return true;
            }
        }
    }

    class DelegateCommand : Command
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;
        
        public DelegateCommand(Action<object> execute, Predicate<object> canExecute)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }

        public override void Execute(object parameter)
        {
            this._execute(parameter);
        }

        public override bool CanExecute(object parameter)
        {
            if (this._canExecute != null)
            {
                return this._canExecute(parameter);
            }
            else
            {
                return true;
            }
        }
    }
}
