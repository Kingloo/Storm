using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Storm
{
    public abstract class Command : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public abstract void Execute(object parameter);
        public abstract bool CanExecute(object parameter);

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }

    public class DelegateCommand : Command
    {
        private readonly Action _execute = null;
        private readonly Predicate<object> _canExecute = null;

        public DelegateCommand(Action execute, Predicate<object> canExecute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            if (canExecute == null) throw new ArgumentNullException(nameof(canExecute));

            _execute = execute;
            _canExecute = canExecute;
        }

        public override void Execute(object parameter)
        {
            _execute();
        }

        public override bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }
    }

    public class DelegateCommand<T> : Command
    {
        private readonly Action<T> _execute = null;
        private readonly Predicate<T> _canExecute = null;

        public DelegateCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            if (canExecute == null) throw new ArgumentNullException(nameof(canExecute));

            _execute = execute;
            _canExecute = canExecute;
        }

        public override void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public override bool CanExecute(object parameter)
        {
            return _canExecute((T)parameter);
        }
    }

    public class DelegateCommandAsync : Command
    {
        private readonly Func<Task> _executeAsync = null;
        private readonly Predicate<object> _canExecute = null;
        private bool _isExecuting = false;

        public DelegateCommandAsync(Func<Task> executeAsync, Predicate<object> canExecute)
        {
            if (executeAsync == null) throw new ArgumentNullException(nameof(executeAsync));
            if (canExecute == null) throw new ArgumentNullException(nameof(canExecute));

            _executeAsync = executeAsync;
            _canExecute = canExecute;
        }

        public async override void Execute(object parameter)
        {
            await ExecuteAsync();
        }

        private async Task ExecuteAsync()
        {
            _isExecuting = true;
            RaiseCanExecuteChanged();

            await _executeAsync();

            _isExecuting = false;
            RaiseCanExecuteChanged();
        }

        public override bool CanExecute(object parameter)
        {
            if (_isExecuting == true)
            {
                return false;
            }
            else
            {
                return _canExecute(parameter);
            }
        }
    }

    public class DelegateCommandAsync<T> : Command
    {
        private readonly Func<T, Task> _executeAsync;
        private readonly Predicate<T> _canExecute;
        private bool _isExecuting = false;

        public DelegateCommandAsync(Func<T, Task> executeAsync, Predicate<T> canExecute)
        {
            if (executeAsync == null) throw new ArgumentNullException(nameof(executeAsync));
            if (canExecute == null) throw new ArgumentNullException(nameof(canExecute));

            _executeAsync = executeAsync;
            _canExecute = canExecute;
        }

        public override async void Execute(object parameter)
        {
            await ExecuteAsync((T)parameter);
        }

        private async Task ExecuteAsync(T parameter)
        {
            _isExecuting = true;
            RaiseCanExecuteChanged();

            await _executeAsync(parameter);

            _isExecuting = false;
            RaiseCanExecuteChanged();
        }

        public override bool CanExecute(object parameter)
        {
            if (_isExecuting == true)
            {
                return false;
            }
            else
            {
                return _canExecute((T)parameter);
            }
        }
    }
}
