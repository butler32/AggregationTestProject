using AggregationTestProject.Constants;
using AggregationTestProject.DTOs;
using AggregationTestProject.Models.Settings;
using AggregationTestProject.Services.Api;
using AggregationTestProject.Services.Devices.Camera;
using AggregationTestProject.Services.Devices.Printer;
using AggregationTestProject.Services.Devices.Scanner;
using AggregationTestProject.Services.Utilities;
using AggregationTestProject.Shared;
using AggregationTestProject.Utilities.ApplicationEventArgs;
using AggregationTestProject.Utilities.Interfaces;
using log4net;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Threading;

namespace AggregationTestProject.ViewModels
{
    public class BoxAssembleViewModel : BaseViewModel
    {
        private readonly ILog _log;
        private MissionDto _missionDto;
        private SharedState _sharedState;
        private Dispatcher _dispatcher;

        private IApplicatorService _applicatorService;
        private ICameraService _cameraService;
        private ZebraPrinter _printerService;
        private ZebraScanner _scannerService;

        private readonly ApplicationApiService _apiService;

        private const string _done = "Done";
        private const string _error = "Error";
        private const string _print = "Printing...";
        private const string _binding = "Binding...";
        private const string _putting = "Putting item into box...";
        private const string _scanLabel = "Scan box label";

        private AssembleServiceState _assembleState;

        private bool _isBoxFilled = false;
        private bool _isLabelPrinted = false;
        private int _currentBoxId;

        private int _codesFromCameraToBindCount;
        private List<string> _codesFromCameraToBind = new();
        public string Process
        {
            get
            {
                switch (_assembleState)
                {
                    case AssembleServiceState.Done:
                        return _done;
                    case AssembleServiceState.Putting:
                        return _putting;
                    case AssembleServiceState.Error:
                        return _error;
                    case AssembleServiceState.Printing:
                        return _print;
                    case AssembleServiceState.Binding:
                        return _binding;
                    case AssembleServiceState.NeedToScanLabel:
                        return _scanLabel;
                    default:
                        return string.Empty;
                }
            }
        }

        private bool _isDimVisible;
        public bool IsDimVisible
        {
            get => _isDimVisible;
            set { _isDimVisible = value; OnPropertyChanged(); }
        }

        private int _boxFormat;
        public int BoxFormat
        {
            get => _boxFormat;
            set { _boxFormat = value; OnPropertyChanged(nameof(BoxFormat)); }
        }

        private int _itemsInBox;
        public int ItemsInBox
        {
            get => _itemsInBox;
            set { _itemsInBox = value; OnPropertyChanged(); }
        }

        private string _errorLabel;
        public string ErrorLabel
        {
            get => _errorLabel;
            set { _errorLabel = value; OnPropertyChanged(); }
        }

        private ImageSource _lastImage;
        public ImageSource LastImage
        {
            get => _lastImage;
            set { _lastImage = value; OnPropertyChanged(); }
        }

        private string _code;
        public string Code
        {
            get => _code;
            set { _code = value; OnPropertyChanged(); }
        }

        private void SetCurrentState(AssembleServiceState state)
        {
            _assembleState = state;
            OnPropertyChanged(nameof(Process));
        }

        private async Task PutItemIntoBox(string code)
        {
            SetCurrentState(AssembleServiceState.Putting);

            try
            {
                var box = await _apiService.PutItemInBoxAsync(code);
                ItemsInBox = box.ItemsInsideBox;
                _currentBoxId = box.BoxId;

                SetCurrentState(AssembleServiceState.Done);

                if (box.ItemsInsideBox == box.BoxFormat)
                {
                    _isBoxFilled = true;

                    try
                    {
                        await ApplicatorPrint();
                    }
                    catch
                    {
                        SetCurrentState(AssembleServiceState.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                SetCurrentState(AssembleServiceState.Error);
                ErrorLabel = ex.Message;
            }
        }

        public ICommand TriggerCameraCommand { get; }
        public ICommand ConnectToCameraCommand { get; }
        public ICommand DisconnectFromCameraCommand { get; }
        public ICommand GetCurrentMissionCommand { get; }
        
        private async Task GetCurrentMission()
        {
            try
            {
                var mission = await _apiService.GetCurrentMissionAsync();

                if (mission is not null)
                {
                    _missionDto = mission;
                }
                else
                {
                    throw new Exception("Mission was null");
                }
            }
            catch (Exception ex)
            {
                ErrorLabel = ex.Message;
            }
        }

        private async Task BindLabelWithBox(string code)
        {
            SetCurrentState(AssembleServiceState.Binding);
            try
            {
                var box = await _apiService.BindCodeFromLabelWithFilledBox(_currentBoxId, code);

                SetCurrentState(AssembleServiceState.Done);
            }
            catch (Exception ex)
            {
                SetCurrentState(AssembleServiceState.Error);
            }

            ItemsInBox = 0;
        }

        private async Task PrintLabel()
        {
            //var label = _printerService.PrepareBoxLabel(
            //    Constants.DPI.DPI203,
            //    _missionDto.Lot.Product.Gtin,
            //    DateTime.ParseExact(_missionDto.Lot.DateAt, "dd.MM.yy", CultureInfo.InvariantCulture).ToString("yyMMdd"),
            //    DateTime.ParseExact(_missionDto.DateAt, "dd.MM.yy", CultureInfo.InvariantCulture).ToString("yyMMdd"),
            //    _missionDto.Lot.Number,
            //    _missionDto.Lot.Package.BoxFormat.ToString("D3"),
            //    "01", 99, 90, 24, 1, 11, 3, 37);

            _missionDto.BoxCounter++;

            //await _printerService.PrintBoxLabel(label, (_missionDto.BoxCounter).ToString("D4"));

            _sharedState.UpdateCurrentMission(this, _missionDto);

            _isLabelPrinted = true;
        }

        private async void ProcessPicture(object? sender, PictureTakenEventArgs e)
        {
            if (!_sharedState.IsBoxAssView)
            {
                return;
            }

            LastImage = BitmapEditor.ConvertBitmapToImageSource(e.OverlayedBitmap);
            
            if (e.CodesAsList is null || e.CodesAsList.Count == 0)
            {
                return;
            }

            if (_isBoxFilled && e.CodesAsList.Count == 1 && _isLabelPrinted)
            {
                try
                {
                    SetCurrentState(AssembleServiceState.Binding);

                    string codeBuffer = Code;

                    Code = e.CodesAsList.First();

                    await BindLabelWithBox(Code);

                    _isLabelPrinted = false;

                    var box = await _apiService.InitializeBoxAsync();

                    _sharedState.UpdateCurrentBoxId(this, box.Id);

                    _isBoxFilled = false;

                    SetCurrentState(AssembleServiceState.Done);

                    Code = codeBuffer;

                    return;
                }
                catch (Exception ex)
                {
                    SetCurrentState(AssembleServiceState.Error);

                    return;
                }
            }

            if (_codesFromCameraToBindCount == 0)
            {
                Code = string.Empty;
            }

            _codesFromCameraToBindCount += e.CodesAsList.Count;

            foreach (var code in e.CodesAsList)
            {
                _codesFromCameraToBind.Add(code);
            }

            List<string>codes = new List<string>(_codesFromCameraToBind);

            int codesCountBeforeStartPutting = _codesFromCameraToBindCount;

            for (int i = 0; i < codesCountBeforeStartPutting; i++)
            {
                Code += "\n" + codes[i];

                if (!_isBoxFilled)
                {
                    try
                    {
                        SetCurrentState(AssembleServiceState.Putting);

                        await PutItemIntoBox(codes[i]);

                        _codesFromCameraToBindCount--;

                        _codesFromCameraToBind.Remove(codes[i]);

                        SetCurrentState(AssembleServiceState.Done);
                    }
                    catch (Exception ex)
                    {
                        SetCurrentState(AssembleServiceState.Error);
                    }
                }
                else
                {
                    if (!_isLabelPrinted)
                    {
                        await ApplicatorPrint();
                    }
                    else
                    {
                        SetCurrentState(AssembleServiceState.NeedToScanLabel);
                    }
                }
            }
        }

        private async Task ApplicatorPrint()
        {
            try
            {
                await _applicatorService.SendDataAsync(CVLabelHelper.GetLabelData(_missionDto));

                _missionDto.BoxCounter++;

                _sharedState.UpdateCurrentMission(this, _missionDto);

                _isLabelPrinted = true;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        public BoxAssembleViewModel(ILog log, SharedState sharedState, Setting setting, IApplicatorService applicatorService, Dispatcher dispatcher, ZebraScanner zebraScanner, ZebraPrinter zebraPrinter, ICameraService cognexCamera, ApplicationApiService applicationApiService)
        {
            _log = log;
            _sharedState = sharedState;
            _cameraService = cognexCamera;
            _printerService = zebraPrinter;
            _apiService = applicationApiService;
            _scannerService = zebraScanner;
            _dispatcher = dispatcher;
            _applicatorService = applicatorService;

            Task.Run(_scannerService.Connect);

            _applicatorService.ConnectAsync(setting.PrinterOptions.BoxApplicator.Ip, setting.PrinterOptions.BoxApplicator.Port);

            _missionDto = _sharedState.CurrentMission;
            BoxFormat = _missionDto.Lot.Package.BoxFormat;

            _cameraService.PictureTaken += ProcessPicture;
            _scannerService.ScannedCodeEvent += _scannerService_ScannedCodeEvent;

            ConnectToCameraCommand = ICommand.From(_cameraService.ConnectAsync);
            DisconnectFromCameraCommand = ICommand.From(_cameraService.DisconnectAsync);
            TriggerCameraCommand = ICommand.From(_cameraService.TriggerCameraAsync);
            GetCurrentMissionCommand = ICommand.From(GetCurrentMission);
        }

        private async void _scannerService_ScannedCodeEvent(object? sender, ScannedCodeEventArgs e)
        {
            if (!_sharedState.IsBoxAssView)
            {
                return;
            }

            if (string.IsNullOrEmpty(e.ReadableData))
            {
                return;
            }

            Code = e.ReadableData;

            if (Code.Contains((char)0x1D))
            {
                Code += "\ncontains GS symbol";
            }
            else
            {
                Code += "\nnot cantains GS symbol";
            }

            return;

            if (!_isBoxFilled)
            {
                await PutItemIntoBox(Code);
            }
            else
            {
                if (_isLabelPrinted)
                {
                    await BindLabelWithBox(Code);

                    _isLabelPrinted = false;

                    var box = await _apiService.InitializeBoxAsync();

                    _sharedState.UpdateCurrentBoxId(this, box.Id);

                    _isBoxFilled = false;
                }
                else
                {
                    await ApplicatorPrint();
                }
            }
        }
    }
}
