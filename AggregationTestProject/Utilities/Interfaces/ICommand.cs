namespace AggregationTestProject.Utilities.Interfaces
{
    public interface ICommand : System.Windows.Input.ICommand
    {
        void System.Windows.Input.ICommand.Execute(object? parameter) => ExecuteAsync().ConfigureAwait(false);
        bool System.Windows.Input.ICommand.CanExecute(object? parameter) => true;

        Task ExecuteAsync();

        public static RelayCommand From(Action action) => new(action);
        public static RelayCommand From(Func<Task> action) => new(action);
    }

    public interface ICommand<T> : System.Windows.Input.ICommand
    {
        void System.Windows.Input.ICommand.Execute(object? parameter) => ExecuteAsync((T)parameter).ConfigureAwait(false);
        bool System.Windows.Input.ICommand.CanExecute(object? parameter) => true;

        Task ExecuteAsync(T parameter);

        public static RelayCommand<T> From(Action<T> action) => new(action);
        public static RelayCommand<T> From(Func<T, Task> action) => new(action);
    }
}
