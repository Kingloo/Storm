using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StormDesktop.Common
{
	public abstract class Command : ICommand
	{
		public event EventHandler? CanExecuteChanged;

		public abstract void Execute(object? parameter);
		public abstract bool CanExecute(object? parameter);

#pragma warning disable CA1030
		public void RaiseCanExecuteChanged()
			=> CanExecuteChanged?.Invoke(this, new EventArgs());
#pragma warning restore CA1030
	}

	public class DelegateCommand : Command
	{
		private readonly Action _execute;
		private readonly Predicate<object?> _canExecute;

		public DelegateCommand(Action execute)
		{
			ArgumentNullException.ThrowIfNull(execute);

			_execute = execute;
			_canExecute = (_) => true;
		}

		public DelegateCommand(Action execute, Predicate<object?> canExecute)
		{
			ArgumentNullException.ThrowIfNull(execute);
			ArgumentNullException.ThrowIfNull(canExecute);

			_execute = execute;
			_canExecute = canExecute;
		}

		public void Execute()
			=> _execute();

		public override void Execute(object? parameter)
			=> _execute();

		public override bool CanExecute(object? parameter)
			=> _canExecute(parameter);
	}

	public class DelegateCommand<T> : Command
	{
		private readonly Action<T> _execute;
		private readonly Predicate<T> _canExecute;

		public DelegateCommand(Action<T> execute)
		{
			ArgumentNullException.ThrowIfNull(execute);

			_execute = execute;
			_canExecute = (_) => true;
		}

		public DelegateCommand(Action<T> execute, Predicate<T> canExecute)
		{
			ArgumentNullException.ThrowIfNull(execute);
			ArgumentNullException.ThrowIfNull(canExecute);

			_execute = execute;
			_canExecute = canExecute;
		}

		public override void Execute(object? parameter)
		{
			ArgumentNullException.ThrowIfNull(parameter);

			_execute((T)parameter);
		}

		public override bool CanExecute(object? parameter)
		{
			ArgumentNullException.ThrowIfNull(parameter);

			return _canExecute((T)parameter);
		}
	}

	public class DelegateCommandAsync : Command
	{
		private readonly Func<Task> _executeAsync;
		private readonly Predicate<object?> _canExecute;
		private bool _isExecuting = false;

		public DelegateCommandAsync(Func<Task> executeAsync)
		{
			ArgumentNullException.ThrowIfNull(executeAsync);

			_executeAsync = executeAsync;
			_canExecute = (_) => true;
		}

		public DelegateCommandAsync(Func<Task> executeAsync, Predicate<object?> canExecute)
		{
			ArgumentNullException.ThrowIfNull(executeAsync);
			ArgumentNullException.ThrowIfNull(canExecute);

			_executeAsync = executeAsync;
			_canExecute = canExecute;
		}

		public async void Execute()
			=> await ExecuteAsync().ConfigureAwait(true);

		public async override void Execute(object? parameter)
			=> await ExecuteAsync().ConfigureAwait(true);

		private async Task ExecuteAsync()
		{
			_isExecuting = true;
			RaiseCanExecuteChanged();

			await _executeAsync().ConfigureAwait(true);

			_isExecuting = false;
			RaiseCanExecuteChanged();
		}

		public override bool CanExecute(object? parameter)
			=> !_isExecuting && _canExecute(parameter);
	}

	public class DelegateCommandAsync<T> : Command
	{
		private readonly Func<T, Task> _executeAsync;
		private readonly Predicate<T> _canExecute;
		private bool _isExecuting = false;

		public DelegateCommandAsync(Func<T, Task> executeAsync)
		{
			ArgumentNullException.ThrowIfNull(executeAsync);

			_executeAsync = executeAsync;
			_canExecute = (_) => true;
		}

		public DelegateCommandAsync(Func<T, Task> executeAsync, Predicate<T> canExecute)
		{
			ArgumentNullException.ThrowIfNull(executeAsync);
			ArgumentNullException.ThrowIfNull(canExecute);

			_executeAsync = executeAsync;
			_canExecute = canExecute;
		}

		public override async void Execute(object? parameter)
		{
			ArgumentNullException.ThrowIfNull(parameter);

			await ExecuteAsync((T)parameter).ConfigureAwait(true);
		}

		private async Task ExecuteAsync(T parameter)
		{
			_isExecuting = true;
			RaiseCanExecuteChanged();

			await _executeAsync(parameter).ConfigureAwait(true);

			_isExecuting = false;
			RaiseCanExecuteChanged();
		}

		public override bool CanExecute(object? parameter)
		{
			ArgumentNullException.ThrowIfNull(parameter);

			return !_isExecuting && _canExecute((T)parameter);
		}
	}
}
