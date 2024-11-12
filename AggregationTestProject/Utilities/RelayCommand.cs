using AggregationTestProject.Utilities.Interfaces;

namespace AggregationTestProject.Utilities
{
    public class RelayCommand : ICommand
    {
        private readonly Action? _action;
        private readonly Func<Task>? _asyncAction;

        public RelayCommand(Action action)
        {
            _action = action;
        }

        public RelayCommand(Func<Task> asyncAction)
        {
            _asyncAction = asyncAction;
        }

        public event EventHandler? CanExecuteChanged;

        public async Task ExecuteAsync()
        {
            if (_action != null)
            {
                _action.Invoke();
            }
            else if (_asyncAction != null)
            {
                await _asyncAction.Invoke();
            }
        }
    }

    public class RelayCommand<T> : ICommand<T>
    {
        private readonly Action<T>? _execute;
        private readonly Func<T, Task>? _executeAsync;

        public RelayCommand(Action<T> execute)
        {
            _execute = execute;
        }

        public RelayCommand(Func<T, Task> func)
        {
            _executeAsync = func;
        }

        public event EventHandler? CanExecuteChanged;

        public async Task ExecuteAsync(T parameter)
        {
            if (_execute is not null)
            {
                _execute?.Invoke(parameter);
            }
            else if (_executeAsync is not null)
            {
                await _executeAsync.Invoke(parameter);
            }
        }
    }
}
