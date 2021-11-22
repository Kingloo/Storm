using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StormDesktop.Common
{
    public abstract class Command : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public abstract void Execute(object parameter);
        public abstract bool CanExecute(object parameter);

        [System.Diagnostics.DebuggerStepThrough]
        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, new EventArgs());
    }

    public class DelegateCommand : Command
    {
        private readonly Action _execute;
        private readonly Predicate<object> _canExecute;

        [System.Diagnostics.DebuggerStepThrough]
        public DelegateCommand(Action execute, Predicate<object> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override void Execute(object? parameter)
            => _execute();

        [System.Diagnostics.DebuggerStepThrough]
        public override bool CanExecute(object parameter)
            => _canExecute(parameter);
    }

    public class DelegateCommand<T> : Command
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        [System.Diagnostics.DebuggerStepThrough]
        public DelegateCommand(Action<T> execute, Predicate<T> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override void Execute(object parameter)
            => _execute((T)parameter);

        [System.Diagnostics.DebuggerStepThrough]
        public override bool CanExecute(object parameter)
            => _canExecute((T)parameter);
    }

    public class DelegateCommandAsync : Command
    {
        private readonly Func<Task> _executeAsync;
        private readonly Predicate<object> _canExecute;
        private bool _isExecuting = false;

        [System.Diagnostics.DebuggerStepThrough]
        public DelegateCommandAsync(Func<Task> executeAsync, Predicate<object> canExecute)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }

        [System.Diagnostics.DebuggerStepThrough]
        public async override void Execute(object? parameter)
            => await ExecuteAsync();

        [System.Diagnostics.DebuggerStepThrough]
        private async Task ExecuteAsync()
        {
            _isExecuting = true;
            RaiseCanExecuteChanged();

            await _executeAsync();

            _isExecuting = false;
            RaiseCanExecuteChanged();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override bool CanExecute(object parameter)
            => _isExecuting ? false : _canExecute(parameter);
    }

    public class DelegateCommandAsync<T> : Command
    {
        private readonly Func<T, Task> _executeAsync;
        private readonly Predicate<T> _canExecute;
        private bool _isExecuting = false;

        [System.Diagnostics.DebuggerStepThrough]
        public DelegateCommandAsync(Func<T, Task> executeAsync, Predicate<T> canExecute)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override async void Execute(object parameter)
            => await ExecuteAsync((T)parameter);

        [System.Diagnostics.DebuggerStepThrough]
        private async Task ExecuteAsync(T parameter)
        {
            _isExecuting = true;
            RaiseCanExecuteChanged();

            await _executeAsync(parameter);

            _isExecuting = false;
            RaiseCanExecuteChanged();
        }

        [System.Diagnostics.DebuggerStepThrough]
        public override bool CanExecute(object parameter)
            => _isExecuting ? false : _canExecute((T)parameter);
    }
}
