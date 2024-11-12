namespace AggregationTestProject.Utilities.Interfaces
{
    public interface IInternetDevice
    {
        string Ip { get; set; }
        bool IsAvailable { get; set; }
    }
}
