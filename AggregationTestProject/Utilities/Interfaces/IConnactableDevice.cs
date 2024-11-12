namespace AggregationTestProject.Utilities.Interfaces
{
    public interface IConnactableDevice
    {
        public string Ip { get; }
        public event EventHandler<bool> ConnectionStateChanged;
    }
}
