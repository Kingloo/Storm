using System;
using System.Threading.Tasks;

namespace Storm
{
    public abstract class Command : System.Windows.Input.ICommand
    {
        public event EventHandler CanExecuteChanged;

        public abstract void Execute(object parameter);
        public abstract bool CanExecute(object parameter);

        public void RaiseCanExecuteChanged()
        {
            EventHandler handler = this.CanExecuteChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }
    }

    public class DelegateCommand<T> : Command
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public DelegateCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute", "execute was null");
            }

            if (canExecute == null)
            {
                throw new ArgumentNullException("canExecute", "canExecute was null");
            }

            this._execute = execute;
            this._canExecute = canExecute;
        }

        public override void Execute(object parameter)
        {
            this._execute((T)parameter);
        }

        public override bool CanExecute(object parameter)
        {
            return this._canExecute((T)parameter);
        }
    }

    public class DelegateCommandAsync<T> : Command
    {
        private readonly Func<T, Task> _executeAsync;
        private readonly Predicate<T> _canExecute;
        private bool _isExecuting = false;

        public DelegateCommandAsync(Func<T, Task> executeAsync, Predicate<T> canExecute)
        {
            if (executeAsync == null)
            {
                throw new ArgumentNullException("executeAsync is null");
            }

            if (canExecute == null)
            {
                throw new ArgumentNullException("canExecute is null");
            }

            this._executeAsync = executeAsync;
            this._canExecute = canExecute;
        }

        public override async void Execute(object parameter)
        {
            await this.ExecuteAsync((T)parameter);
        }

        private async Task ExecuteAsync(T parameter)
        {
            this._isExecuting = true;
            this.RaiseCanExecuteChanged();

            await this._executeAsync(parameter);

            this._isExecuting = false;
            this.RaiseCanExecuteChanged();
        }

        public override bool CanExecute(object parameter)
        {
            if (this._isExecuting == true)
            {
                return false;
            }
            else
            {
                return this._canExecute((T)parameter);
            }
        }
    }
}
