using AggregationTestProject.Models.Settings;
using AggregationTestProject.Utilities.ApplicationEventArgs;
using Cognex.DataMan.SDK;
using Cognex.DataMan.SDK.Utils;
using log4net;
using System.Drawing;
using System.Net;
using System.Text;
using System.Xml;

namespace AggregationTestProject.Services.Devices.Camera
{
    public class CognexCamera : ICameraService
    {
        private readonly ILog _log;
        private readonly Setting _settings;

        private string _ip;

        private static string[] _stringSeparators = ["\r\n"];
        private const string _noReadReplyStr = "NO_READ";

        private bool _wantToDisconect = false;
        private bool _isConnected = false;

        private EthSystemConnector _systemConnector;
        private DataManSystem _system;
        private ResultCollector _resultCollector;

        public string Ip
        {
            get => _ip;
            private set => _ip = value;
        }

        public event EventHandler<bool> ConnectionStateChanged;
        public event EventHandler<PictureTakenEventArgs> PictureTaken;

        public CognexCamera(ILog log, Setting setting)
        {
            _log = log;
            _settings = setting;

            Ip = _settings.CameraOptions.AggregationCamera.Ip;
        }

        public async Task ConnectAsync()
        {
            _systemConnector = new(IPAddress.Parse(Ip));

            _systemConnector.UserName = "admin";
            _systemConnector.Password = string.Empty;

            _system = new(_systemConnector);

            SubscribeToCameraEvents();

            _system.Connect();

            _isConnected = true;
        }

        public async Task DisconnectAsync()
        {
            _wantToDisconect = true;

            _system.Disconnect();

            _isConnected = false;
        }

        public async Task TriggerCameraAsync()
        {
            _system.SendCommand("TRIGGER ON");

            await Task.Delay(500);

            _system.SendCommand("TRIGGER OFF");
        }

        private void SubscribeToCameraEvents()
        {
            _system.SystemDisconnected += _system_SystemDisconnected;
            _system.KeepAliveResponseMissed += _system_KeepAliveResponseMissed;
            _system.SystemConnected += _system_SystemConnected;

            var fullResult = ResultTypes.ReadXml | ResultTypes.Image | ResultTypes.ImageGraphics;

            _resultCollector = new(_system, fullResult);
            _resultCollector.ComplexResultCompleted += _resultCollector_ComplexResultCompleted;
            _resultCollector.SimpleResultDropped += _resultCollector_SimpleResultDropped;

            _system.SetResultTypes(fullResult);
        }

        private void _system_SystemConnected(object sender, EventArgs args)
        {
            _isConnected = true;

            ConnectionStateChanged?.Invoke(this, true);
        }

        private async void _system_KeepAliveResponseMissed(object sender, EventArgs args)
        {
            _isConnected = false;

            ConnectionStateChanged?.Invoke(this, false);

            await StartReconectionLoop();
        }

        private void _resultCollector_SimpleResultDropped(object sender, SimpleResult e)
        {
            if (e.Id.Type == ResultTypes.ReadXml)
            {
                var readXmlStr = e.GetDataAsString();
                var readCodes = GetReadStringFromResultXml(readXmlStr);

                List<string> itemCodeAsList = readCodes == _noReadReplyStr
                ? new List<string>()
                : readCodes.Split(_stringSeparators, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();

                PictureTaken?.Invoke(
                    this,
                    new PictureTakenEventArgs(null, itemCodeAsList, string.Empty, string.Empty)
                    );
            }
        }

        private void _resultCollector_ComplexResultCompleted(object sender, ComplexResult e)
        {
            ProcessComplexResult(e);
        }

        private async void _system_SystemDisconnected(object sender, EventArgs args)
        {
            _isConnected = false;

            ConnectionStateChanged?.Invoke(this, false);

            await StartReconectionLoop();
        }

        private async Task StartReconectionLoop()
        {
            while (!_isConnected)
            {
                await Task.Delay(2000);

                await ConnectAsync();
            }
        }

        private void ProcessSimpleResult(SimpleResult result, ref Image image, ref string readCodes, ref string readCodesPositions)
        {
            switch (result.Id.Type)
            {
                case ResultTypes.ReadXml:
                    readCodes = GetReadStringFromResultXml(result.GetDataAsString());
                    break;

                case ResultTypes.Image:
                    image = ImageArrivedEventArgs.GetImageFromImageBytes(result.Data);
                    break;

                case ResultTypes.ImageGraphics:
                    readCodesPositions = result.GetDataAsString();
                    break;

                default:
                    break;
            }
        }

        private void ProcessComplexResult(ComplexResult complexResult)
        {
            Image image = null!;
            var readCodes = string.Empty;
            var readCodesPositions = string.Empty;

            foreach (var res in complexResult.SimpleResults)
            {
                ProcessSimpleResult(res, ref image, ref readCodes, ref readCodesPositions);
            }

            List<string> itemCodesList = readCodes.Equals(_noReadReplyStr) ?
                new List<string>() :
                readCodes.Split(_stringSeparators, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();

            Bitmap bitmap = new(image);

            PictureTaken?.Invoke(this, new PictureTakenEventArgs(bitmap, itemCodesList, readCodes, readCodesPositions));
        }

        private string GetReadStringFromResultXml(string resultXml)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(resultXml);

                XmlNode full_string_node = xmlDocument.SelectSingleNode("result/general/full_string")!;

                if (full_string_node != null && full_string_node.Attributes!["encoding"]?.InnerText == "base64")
                {
                    byte[] code = Convert.FromBase64String(full_string_node.InnerText);
                    return Encoding.UTF8.GetString(code);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }
    }
}
