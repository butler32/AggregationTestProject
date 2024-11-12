using AggregationTestProject.Utilities.ApplicationEventArgs;
using CoreScanner;
using log4net;
using System.Xml;

namespace AggregationTestProject.Services.Utilities
{
    public class DevicesDiscoverService
    {
        private readonly ILog _log;
        private readonly Models.Settings.Setting _settings;
        private readonly InternetDeviceHelper _deviceHelper;
        private CCoreScanner _zebraScanner;
        public EventHandler<CameraFoundEventArgs> CameraFound;
        public EventHandler<PrinterFoundEventArgs> PrinterFound;
        public EventHandler<ScannerFoundEventArgs> ScannerFound;

        public DevicesDiscoverService(ILog log, Models.Settings.Setting settings, InternetDeviceHelper deviceHelper)
        {
            _log = log;
            _settings = settings;
            _deviceHelper = deviceHelper;

            _zebraScanner = new CCoreScanner();
        }

        public async Task StartDiscover()
        {
            var scanners =  Task.Run(CheckScannerDevices);

            var cameras = Task.Run(CheckCameraDevices);

            var printers = Task.Run(CheckPrinterDevices);

            await Task.WhenAll(scanners, cameras, printers);
        }

        public void CheckScannerDevices()
        {
            try
            {
                if (_settings.ScannerOptions is null)
                {
                    return;
                }

                if (_settings.ScannerOptions.Scanner is null || _settings.ScannerOptions.Station is null)
                {
                    throw new Exception("Scanner or stations config was null");
                }

                var currentScanner = _settings.ScannerOptions.Scanner;
                var currentStation = _settings.ScannerOptions.Station;

                //Scanner Types you are interested in
                short numberOfScannerTypes = 1; // Size of the scannerTypes array                

                short[] scannerTypes = [
                    1 // 1 for all scanner types
                ];

                _zebraScanner.Open(0, scannerTypes, numberOfScannerTypes, out int _status);

                string discoveredScanners = DiscoverZebraDevices();

                var baseId = GetScannerIDFromSerialNumber(discoveredScanners, currentStation.SerialNumber);
                var scannerId = GetScannerIDFromSerialNumber(discoveredScanners, currentScanner.SerialNumber);

                bool scannerOk = false;
                bool stationOk = false;

                if (baseId is not null)
                {
                    stationOk = true;
                }

                if (scannerId is not null)
                {
                    scannerOk = true;
                }

                ScannerFound?.Invoke(this, new ScannerFoundEventArgs(currentScanner.Brand, currentScanner.SerialNumber, false, scannerOk));
                ScannerFound?.Invoke(this, new ScannerFoundEventArgs(currentStation.Brand, currentStation.SerialNumber, true, stationOk));

                _zebraScanner.Close(0, out _);
            }
            catch (Exception ex)
            {
                _log.Error("Scanner discover: error", ex);
            }
        }

        public async Task CheckPrinterDevices()
        {
            if (_settings.PrinterOptions is null)
            {
                return;
            }

            if (_settings.PrinterOptions.BoxPrinter is not null)
            {
                var currentPrinter = _settings.PrinterOptions.BoxPrinter;

                var isPrinterOk = await _deviceHelper.PingDeviceAsync(currentPrinter.Ip);

                PrinterFound?.Invoke(this, new PrinterFoundEventArgs(currentPrinter.Name, currentPrinter.Ip, isPrinterOk));
            }

            if (_settings.PrinterOptions.PalletPrinter is not null)
            {
                var currentPrinter = _settings.PrinterOptions.PalletPrinter;

                var isPrinterOk = await _deviceHelper.PingDeviceAsync(currentPrinter.Ip);

                PrinterFound?.Invoke(this, new PrinterFoundEventArgs(currentPrinter.Name, currentPrinter.Ip, isPrinterOk));
            }

            if (_settings.PrinterOptions.BoxApplicator is not null)
            {
                var currentPrinter = _settings.PrinterOptions.BoxApplicator;

                var isPrinterOk = await _deviceHelper.PingDeviceAsync(currentPrinter.Ip);

                PrinterFound?.Invoke(this, new PrinterFoundEventArgs(currentPrinter.Name, currentPrinter.Ip, isPrinterOk));
            }

            if (_settings.PrinterOptions.PalletApplicator is not null)
            {
                var currentPrinter = _settings.PrinterOptions.PalletApplicator;

                var isPrinterOk = await _deviceHelper.PingDeviceAsync(currentPrinter.Ip);

                PrinterFound?.Invoke(this, new PrinterFoundEventArgs(currentPrinter.Name, currentPrinter.Ip, isPrinterOk));
            }
        }

        public async Task CheckCameraDevices()
        {
            if (_settings.CameraOptions is null)
            {
                return;
            }

            if (_settings.CameraOptions.AggregationCamera is not null)
            {
                var currentCamera = _settings.CameraOptions.AggregationCamera;

                var isCameraOk = await _deviceHelper.PingDeviceAsync(currentCamera.Ip);

                CameraFound?.Invoke(this, new CameraFoundEventArgs(currentCamera.Name, currentCamera.Ip, isCameraOk));
            }
            
            //if (_settings.CameraOptions.BoxAssembleCamera is not null)
            //{
            //    var currentCamera = _settings.CameraOptions.BoxAssembleCamera;

            //    var isCameraOk = await _deviceHelper.PingDeviceAsync(currentCamera.Ip);

            //    CameraFound?.Invoke(this, new CameraFoundEventArgs(currentCamera.Name, currentCamera.Ip, isCameraOk));
            //}

            //if (_settings.CameraOptions.PalletAssembleCamera is not null)
            //{
            //    var currentCamera = _settings.CameraOptions.PalletAssembleCamera;

            //    var isCameraOk = await _deviceHelper.PingDeviceAsync(currentCamera.Ip);

            //    CameraFound?.Invoke(this, new CameraFoundEventArgs(currentCamera.Name, currentCamera.Ip, isCameraOk));
            //}

            //if (_settings.CameraOptions.CheckBoxToPrintCamera is not null)
            //{
            //    var currentCamera = _settings.CameraOptions.CheckBoxToPrintCamera;

            //    var isCameraOk = await _deviceHelper.PingDeviceAsync(currentCamera.Ip);

            //    CameraFound?.Invoke(this, new CameraFoundEventArgs(currentCamera.Name, currentCamera.Ip, isCameraOk));
            //}
        }

        private string DiscoverZebraDevices()
        {
            // Lets list down all the scanners connected to the host
            short numberOfScanners;

            // Number of scanners expect to be used
            int[] connectedScannerIDList = new int[255];

            //Scanner details output
            _zebraScanner.GetScanners(out numberOfScanners, connectedScannerIDList, out string outXML, out int status);

            return outXML;
        }

        private int? GetScannerIDFromSerialNumber(string xmlString, string serialNumberToFind)
        {
            int? scannerID = null;

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlString);

                XmlNodeList scannerNodes = xmlDoc.SelectNodes("/scanners/scanner")!;
                foreach (XmlNode scannerNode in scannerNodes)
                {
                    XmlNode serialNumberNode = scannerNode.SelectSingleNode("serialnumber")!;
                    if (serialNumberNode != null && serialNumberNode.InnerText.Trim() == serialNumberToFind)
                    {
                        XmlNode scannerIDNode = scannerNode.SelectSingleNode("scannerID")!;
                        if (scannerIDNode != null)
                        {
                            scannerID = int.Parse(scannerIDNode.InnerText);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Сканер: Произошла ошибка: {ex.Message}");
            }

            return scannerID;
        }
    }
}
