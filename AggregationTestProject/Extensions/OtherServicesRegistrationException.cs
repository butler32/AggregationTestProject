using AggregationTestProject.Services.Devices.Camera;
using AggregationTestProject.Services.Devices.Printer;
using AggregationTestProject.Services.Devices.Scanner;
using AggregationTestProject.Services.Utilities;
using Unity;

namespace AggregationTestProject.Extensions
{
    public static class OtherServicesRegistrationException
    {
        public static IUnityContainer RegisterOtherServices(this IUnityContainer container)
        {
            container.RegisterSingleton<DevicesDiscoverService>();
            container.RegisterSingleton<CognexCamera>();
            container.RegisterFactory<ICameraService>(c => c.Resolve<CognexCamera>());
            container.RegisterSingleton<ZebraPrinter>();
            container.RegisterSingleton<ZebraScanner>();
            container.RegisterSingleton<CarlValentineApplicator>();
            container.RegisterFactory<IApplicatorService>(c => c.Resolve<CarlValentineApplicator>());

            return container;
        }
    }
}
