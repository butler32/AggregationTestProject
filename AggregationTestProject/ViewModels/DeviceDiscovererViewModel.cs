using AggregationTestProject.Models;
using AggregationTestProject.Services.Utilities;
using AggregationTestProject.Utilities.ApplicationEventArgs;
using AggregationTestProject.Utilities.Interfaces;
using log4net;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace AggregationTestProject.ViewModels
{
    public class DeviceDiscovererViewModel : BaseViewModel
    {
        private readonly ILog _log;
        private readonly Models.Settings.Setting _settings;
        private readonly DevicesDiscoverService _devicesDiscoverService;
        private readonly Dispatcher _dispatcher;

        private ObservableCollection<DiscoveredDevice> _scanners;
        public ObservableCollection<DiscoveredDevice> Scanners
        {
            get => _scanners;
            set { _scanners = value; OnPropertyChanged(); }
        }

        private ObservableCollection<DiscoveredDevice> _printers;
        public ObservableCollection<DiscoveredDevice> Printers
        {
            get => _printers;
            set { _printers = value; OnPropertyChanged(); }
        }

        private ObservableCollection<DiscoveredDevice> _cameras;
        public ObservableCollection<DiscoveredDevice> Cameras
        {
            get => _cameras;
            set { _cameras = value; OnPropertyChanged(); }
        }

        public DeviceDiscovererViewModel(ILog log, Models.Settings.Setting settings, DevicesDiscoverService devicesDiscoverService, Dispatcher dispatcher)
        {
            _log = log;
            _settings = settings;
            _devicesDiscoverService = devicesDiscoverService;
            _dispatcher = dispatcher;

            Scanners = new ObservableCollection<DiscoveredDevice>();
            Printers = new ObservableCollection<DiscoveredDevice>();
            Cameras = new ObservableCollection<DiscoveredDevice>();

            _devicesDiscoverService.ScannerFound += ScannerFoundProcess;
            _devicesDiscoverService.PrinterFound += PrinterFoundProcess;
            _devicesDiscoverService.CameraFound += CameraFoundProcess;

            DiscoverDevicesCommand = ICommand.From(Discover);
            DiscoverDevicesCommand.Execute(this);
        }

        public ICommand DiscoverDevicesCommand { get; }

        private async Task Discover()
        {
            await _devicesDiscoverService.StartDiscover();
        }

        private void ScannerFoundProcess(object? sender, ScannerFoundEventArgs e)
        {
            _dispatcher.Invoke(() =>
            {
                Scanners.Add(new DiscoveredDevice
                {
                    Ip = e.SerialNumber,
                    Name = e.Brand,
                    IsReadyToConnect = e.IsOk
                });
            });

        }

        private void CameraFoundProcess(object? sender, CameraFoundEventArgs e)
        {
            _dispatcher?.Invoke(() =>
            {
                Cameras.Add(new DiscoveredDevice
                {
                    Ip = e.Ip,
                    Name = e.Name,
                    IsReadyToConnect = e.IsOk
                });
            });
        }

        private void PrinterFoundProcess(object? sender, PrinterFoundEventArgs e)
        {
            _dispatcher.Invoke(() =>
            {
                Printers.Add(new DiscoveredDevice
                {
                    Ip = e.Ip,
                    Name = e.Name,
                    IsReadyToConnect = e.IsOk
                });
            });
        }
    }
}
