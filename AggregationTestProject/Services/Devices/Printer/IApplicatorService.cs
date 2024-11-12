using AggregationTestProject.Utilities.Interfaces;

namespace AggregationTestProject.Services.Devices.Printer
{
    public interface IApplicatorService : IConnactableDevice
    {
        bool IsConnected { get; }
        string Name { get; }

        event EventHandler<string> ErrorMessageReceived;
        event EventHandler HeartbeatReceived;

        Task<bool> ConnectAsync(string ip, int port);
        Task SendDataAsync(byte[] data);
    }
}
