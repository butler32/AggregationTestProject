using AggregationTestProject.Utilities.ApplicationEventArgs;

namespace AggregationTestProject.Services.Devices.Camera
{
    public interface ICameraService
    {
        string Ip { get; }

        event EventHandler<PictureTakenEventArgs> PictureTaken;
        event EventHandler<bool> ConnectionStateChanged;

        Task ConnectAsync();
        Task DisconnectAsync();
        Task TriggerCameraAsync();
    }
}
