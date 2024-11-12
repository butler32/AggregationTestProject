using AggregationTestProject.DTOs;
using AggregationTestProject.Services.Api;
using AggregationTestProject.Services.Devices.Camera;
using AggregationTestProject.Services.Devices.Printer;
using AggregationTestProject.Services.Devices.Scanner;
using AggregationTestProject.Shared;
using AggregationTestProject.Utilities.ApplicationEventArgs;
using AggregationTestProject.Utilities.Interfaces;
using log4net;
using System.Globalization;
using System.Windows.Media;

namespace AggregationTestProject.ViewModels
{
    public class PalletAssembleViewModel : BaseViewModel
    {
        private readonly ILog _log;
        private MissionDto _missionDto;
        private SharedState _sharedState;

        private ICameraService _cameraService;
        private ZebraPrinter _printerService;
        private ZebraScanner _scannerService;

        private readonly ApplicationApiService _apiService;

        private bool _isPalletFilled = false;
        private bool _isLabelPrinted = false;
        private int _currentPalletId;

        private string _process;
        public string Process
        {
            get => _process;
            set { _process = value; OnPropertyChanged(nameof(Process)); }
        }

        private int _palletFormat;
        public int PalletFormat
        {
            get => _palletFormat;
            set { _palletFormat = value; OnPropertyChanged(nameof(PalletFormat)); }
        }

        private int _itemsInPallet;
        public int ItemsInPallet
        {
            get => _itemsInPallet;
            set { _itemsInPallet = value; OnPropertyChanged(); }
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

        public ICommand TriggerCameraCommand { get; }
        public ICommand ConnectToCameraCommand { get; }
        public ICommand DisconnectFromCameraCommand { get; }
        public ICommand CloseUnfilledPalletCommand { get; }

        private async Task CloseUnfilledPallet()
        {
            Process = "Closing...";

            try
            {
                var pallet = await _apiService.ManualClosePalletAsync(_currentPalletId);

                await PrintLabel(pallet.Code);

                ItemsInPallet = 0;

                Process = "Done";
            }
            catch (Exception ex)
            {
                Process = "Error while closing";
            }
        }

        private async Task PrintLabel(string code)
        {
            //var label = _printerService.PreparePalletLabel(Constants.DPI.DPI203, code, 99, 90, 24, 1, 11, 3, 37);

            //await _printerService.PrintPalletLabel(label);

            _isLabelPrinted = true;
        }

        private async Task<string> GetPalletCodeToPrint()
        {
            try
            {
                var pallet = await _apiService.GetPalletInfoByBoxCode(Code);

                return pallet.Code;
            }
            catch (Exception ex)
            {
                Process = "Error while getting pallet info";

                return string.Empty;
            }
        }

        public PalletAssembleViewModel(ILog log, SharedState sharedState, ZebraScanner zebraScanner, ZebraPrinter zebraPrinter, ICameraService cognexCamera, ApplicationApiService applicationApiService)
        {
            _log = log;
            _sharedState = sharedState;
            _cameraService = cognexCamera;
            _printerService = zebraPrinter;
            _apiService = applicationApiService;
            _scannerService = zebraScanner;

            Task.Run(_scannerService.Connect);

            _missionDto = _sharedState.CurrentMission;
            PalletFormat = _missionDto.Lot.Package.PalletFormat;
            _currentPalletId = _sharedState.CurrentPalletId;

            _cameraService.PictureTaken += ProcessPicture;
            _scannerService.ScannedCodeEvent += _scannerService_ScannedCodeEvent;

            ConnectToCameraCommand = ICommand.From(_cameraService.ConnectAsync);
            DisconnectFromCameraCommand = ICommand.From(_cameraService.DisconnectAsync);
            TriggerCameraCommand = ICommand.From(_cameraService.TriggerCameraAsync);
            CloseUnfilledPalletCommand = ICommand.From(CloseUnfilledPallet);
        }

        private async Task PutBoxOnPallet(string code)
        {
            Process = "Putting box on pallet...";

            try
            {
                var pallet = await _apiService.PutBoxOnPalletAsync(code);
                ItemsInPallet = pallet.BoxesInsidePallet;
                _currentPalletId = _sharedState.CurrentPalletId;

                Process = "Done";

                if (pallet.BoxesInsidePallet == pallet.PalletFormat)
                {
                    Process = "Getting new pallet...";
                    _isPalletFilled = true;

                    try
                    {
                        var codeToPrint = await GetPalletCodeToPrint();

                        await PrintLabel(codeToPrint);

                        _currentPalletId = (await _apiService.InitializePalletAsync()).Id;

                        _sharedState.UpdateCurrentPalletId(this, _currentPalletId);

                        ItemsInPallet = 0;

                        Process = "Done";
                    }
                    catch
                    {
                        Process = "Print or pallet initialization error";
                    }
                }
            }
            catch (Exception ex)
            {
                Process = "Error";
                ErrorLabel = ex.Message;
            }
        }

        private async void _scannerService_ScannedCodeEvent(object? sender, ScannedCodeEventArgs e)
        {
            if (!_sharedState.IsPalletAssView)
            {
                return;
            }

            if (e.ReadableData is null || string.IsNullOrEmpty(e.ReadableData))
            {
                return;
            }

            Code = e.ReadableData;

            await PutBoxOnPallet(Code);
        }

        private void ProcessPicture(object? sender, PictureTakenEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
