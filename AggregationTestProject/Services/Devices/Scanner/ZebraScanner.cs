using AggregationTestProject.Constants;
using AggregationTestProject.Utilities.ApplicationEventArgs;
using CoreScanner;
using log4net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace AggregationTestProject.Services.Devices.Scanner
{
    public class ZebraScanner
    {
        private readonly ILog _log;
        private readonly Models.Settings.Setting _settings;
        private CCoreScanner _scanner;

        public bool IsConnected { get; private set; }

        private int _scannerId;
        private int _baseId;

        public event EventHandler<ScannedCodeEventArgs> ScannedCodeEvent;

        public ZebraScanner(ILog log, Models.Settings.Setting settings)
        {
            _log = log;
            _settings = settings;

            _scanner = new();
        }

        public bool Connect()
        {
            try
            {
                //Scanner Types you are interested in
                short numberOfScannerTypes = 1; // Size of the scannerTypes array                

                short[] scannerTypes = [
                    1 // 1 for all scanner types
                ];

                _scanner.Open(0, scannerTypes, numberOfScannerTypes, out int _status);

                string discoveredScanners = DiscoverZebraDevices();

                string baseSerialNumber = string.Empty;
                string scannerSerialNumber = string.Empty;

                if (_settings is not null && _settings.ScannerOptions is not null)
                {
                    if (_settings.ScannerOptions.Scanner is not null)
                    {
                        scannerSerialNumber = _settings.ScannerOptions.Scanner.SerialNumber;
                    }

                    if (_settings.ScannerOptions.Station is not null)
                    {
                        baseSerialNumber = _settings.ScannerOptions.Station.SerialNumber;
                    }
                }

                var baseId = GetScannerIDFromSerialNumber(discoveredScanners, baseSerialNumber);
                var scannerId = GetScannerIDFromSerialNumber(discoveredScanners, scannerSerialNumber);

                if (scannerId is not null && baseId is not null)
                {
                    _scannerId = (int)scannerId;
                    _baseId = (int)baseId;

                    MakeVibrationFeedback(50);

                    SubscribeToScannerEvents();

                    Initialize();

                    IsConnected = true;
                }
                else
                {
                    throw new Exception(scannerId is null ? "scanner not found" : "station not found");
                }
            }
            catch (Exception ex)
            {
                _log.Warn("Zebra scanner: connection error", ex);

                Disconnect();

                IsConnected = false;
            }

            return IsConnected;
        }

        public bool Disconnect()
        {
            _scanner.Close(0, out _);

            IsConnected = false;

            return true;
        }

        private void Initialize()
        {
            ChangeVolume(ZebraScannerVolume.Low);

            SwitchTheLeds(ZebraColorCode.RedOn);

            Task.Delay(500).Wait();

            SwitchTheLeds(ZebraColorCode.YellowOn);

            Task.Delay(500).Wait();

            SwitchTheLeds(ZebraColorCode.GreenOn);

            Task.Delay(500).Wait();

            SwitchTheLeds(ZebraColorCode.GreenOff);
        }

        private void SubscribeToScannerEvents()
        {
            // Subscribe for barcode events in cCoreScannerClass
            _scanner.BarcodeEvent += OnManualScannerBarcodeEvent;
            // XML Output
            var inXML =
                "<inArgs>" +
                    "<cmdArgs>" +
                        "<arg-int>1</arg-int>" + // Number of events you want to subscribe
                        "<arg-int>1</arg-int>" + // Comma separated event IDs
                    "</cmdArgs>" +
                "</inArgs>";
            _scanner.ExecCommand((int)ZebraOpcode.RegisterForEvents, ref inXML, out string outXML, out int _status);
        }

        public void OnManualScannerBarcodeEvent(short eventType, ref string pscanData)
        {
            var scanDataXml = pscanData;

            var scannerId = GetScannerID(scanDataXml);

            if (scannerId == _baseId)
            {
                var barcodeData = GetScannedDataAsXml(scanDataXml);
                var readableData = GetReadableScanDataLabel(barcodeData);

                ScannedCodeEvent?.Invoke(this, new ScannedCodeEventArgs(readableData));
            }
        }

        public string GetScannedDataAsXml(string scanDataXml)
        {
            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(scanDataXml);
                return xmlDocument.DocumentElement!.GetElementsByTagName("datalabel").Item(0)!.InnerText;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GetReadableScanDataLabel(string scanDataLabel)
        {
            var stringBuilder = new StringBuilder();
            var numbers = scanDataLabel.Split(' ');

            foreach (var number in numbers)
            {
                if (string.IsNullOrEmpty(number))
                {
                    break;
                }
                var character = Convert.ToInt32(number, 16);
                stringBuilder.Append(((char)character).ToString());
            }
            return stringBuilder.ToString();
        }

        private int? GetScannerID(string xmlString)
        {
            try
            {
                XDocument xmlDoc = XDocument.Parse(xmlString);
                XElement scannerIDElement = xmlDoc.Root?.Element("scannerID")!;
                if (scannerIDElement != null)
                {
                    if (int.TryParse(scannerIDElement.Value, out int scannerID))
                    {
                        return scannerID;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("Zebra scanner: xml error", ex);
            }

            return null;
        }

        private void ChangeVolume(ZebraScannerVolume scannerVolume)
        {
            int opcode = (int)ZebraOpcode.RsmAttrSet;
            string outXML;
            int status;
            string inXML =
                "<inArgs>" +
                      "<scannerID>" + _scannerId + "</scannerID>" + // The scanner you need to vibrate
                      "<cmdArgs>" +
                        "<arg-xml>" +
                            "<attrib_list>" +
                                "<attribute>" +
                                    "<id>140</id>" +
                                    "<datatype>B</datatype>" +
                                    "<value>" + (int)scannerVolume + "</value>" +
                                "</attribute>" +
                            "</attrib_list>" +
                        "</arg-xml>" +
                     "</cmdArgs>" +
                "</inArgs>";

            _scanner.ExecCommand(opcode, ref inXML, out outXML, out status);
        }

        private void SwitchTheLeds(ZebraColorCode code)
        {
            int opcode = (int)ZebraOpcode.SetAction;
            string inXML =
                "<inArgs>" +
                    "<scannerID>" + _scannerId + "</scannerID>" + // The scanner you need to beep
                    "<cmdArgs>" +
                        "<arg-int>" + (int)code + "</arg-int>" + // Specify LED code to switch on/off\r\n       
                    "</cmdArgs>" +
                "</inArgs>";

            _scanner.ExecCommand(opcode, inXML, out string s, out int status);
        }

        private void DoBeep(ZebraBeepCode beepCode)
        {
            int opcode = (int)ZebraOpcode.SetAction;
            int code = (int)beepCode;

            string inXML =
                "<inArgs>" +
                    "<scannerID>" + _scannerId + "</scannerID>" +
                    "<cmdArgs>" +
                        "<arg-int>" + code + "</arg-int>" +
                    "</cmdArgs>" +
                "</inArgs>";

            _scanner.ExecCommand(opcode, inXML, out string s, out int status);
        }

        private void MakeVibrationFeedback(byte duration)
        {
            int opcode = (int)ZebraOpcode.RsmAttrSet;
            string outXML;
            int status;
            string inXML =
                "<inArgs>" +
                    "<scannerID>" + _scannerId + "</scannerID>" + // The scanner you need to vibrate
                    "<cmdArgs>" +
                        "<arg-xml>" +
                            "<attrib_list>" +
                                "<attribute>" +
                                    "<id>6033</id>" +
                                    "<datatype>X</datatype>" +
                                    "<value>" + duration + "</value>" +
                                "</attribute>" +
                            "</attrib_list>" +
                        "</arg-xml>" +
                    "</cmdArgs>" +
                "</inArgs>";

            _scanner.ExecCommand(opcode, ref inXML, out outXML, out status);
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
                _log.Error("Zebra service: xml error", ex);
            }

            return scannerID;
        }

        private string DiscoverZebraDevices()
        {
            // Lets list down all the scanners connected to the host
            short numberOfScanners;
            // Number of scanners expect to be used
            int[] connectedScannerIDList = new int[255];
            // List of scanner IDs to be returned
            string outXML;
            int status;
            //Scanner details output
            _scanner.GetScanners(out numberOfScanners, connectedScannerIDList, out outXML, out status);

            return outXML;
        }
    }
}
