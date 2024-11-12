using AggregationTestProject.Utilities.Interfaces;
using System.Net.NetworkInformation;
using Unity;

namespace AggregationTestProject.Services.Utilities
{
    public class InternetDeviceHelper
    {
        private readonly IUnityContainer _unityContainer;

        public InternetDeviceHelper(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
        }

        public async Task<bool> PingDeviceAsync(string deviceIp)
        {
            var ping = _unityContainer.Resolve<Ping>();

            var reply = await ping.SendPingAsync(deviceIp);

            return reply.Status is IPStatus.Success;
        }

        public async Task PingDevicesAsync(List<IInternetDevice> internetDevices)
        {
            await Task.WhenAll(internetDevices.Select(async device =>
            {
                device.IsAvailable = await PingDeviceAsync(device.Ip);
            }));
        }
    }
}
