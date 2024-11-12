using AggregationTestProject.Models.Settings;
using AggregationTestProject.Services.Api;
using AggregationTestProject.Services.Devices.Camera;
using AggregationTestProject.Services.Settings;
using AggregationTestProject.Services.Utilities;
using AggregationTestProject.Shared;
using AggregationTestProject.Utilities.Interfaces;
using log4net;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using Unity;
using Unity.Lifetime;

namespace AggregationTestProject.Extensions
{
    public static class ServicesRegistrationExtension
    {
        public static IUnityContainer RegisterServices(this IUnityContainer services)
        {
            services.RegisterInstance("Logs", new ObservableCollection<string>(), new ContainerControlledLifetimeManager());
            services.AddNewExtension<Log4NetExtension>();

            services.RegisterSingleton<SettingsFromFileService<Setting>>();
            services.RegisterFactory<ISettingsService<Setting>>(c => c.Resolve<SettingsFromFileService<Setting>>(), FactoryLifetime.Singleton);

            services.RegisterFactory<Setting>(c => c.Resolve<ISettingsService<Setting>>().GetSettings());

            services.RegisterSingleton<SharedState>();

            services.RegisterType<Ping>(new TransientLifetimeManager());
            services.RegisterType<CognexCameraDiscoverer>(new TransientLifetimeManager());

            services.RegisterFactory<HttpClientHandler>(c => new HttpClientHandler
            {
                CookieContainer = new CookieContainer()
            }, FactoryLifetime.Singleton);

            // Register HttpClient with factory
            services.RegisterFactory<HttpClient>(c =>
            {
                var handler = c.Resolve<HttpClientHandler>();
                var settings = c.Resolve<Setting>();

                return new HttpClient(handler)
                {
                    BaseAddress = new Uri(settings.ServerOptions.Host),
                    Timeout = TimeSpan.FromMilliseconds(settings.ServerOptions.Timeout)
                };
            });

            services.RegisterFactory<ApiService>(c => new ApiService(c.Resolve<ILog>(), c), FactoryLifetime.Singleton);

            services.RegisterSingleton<InternetDeviceHelper>();
            services.RegisterFactory<ApplicationApiService>(c =>
            {
                var settings = c.Resolve<Setting>();

                return new ApplicationApiService(
                    c.Resolve<ILog>(),
                    c.Resolve<ApiService>(),
                    c.Resolve<InternetDeviceHelper>(),
                    settings.ServerOptions.Ip,
                    settings.AdditionalOptions.OperatorIp1,
                    TimeSpan.FromMilliseconds(settings.ServerOptions.PingInterval),
                    settings.ServerOptions.Email,
                    settings.ServerOptions.Password);
            }, FactoryLifetime.Singleton);

            return services;
        }
    }
}
