using AggregationTestProject.DTOs;
using AggregationTestProject.Services.Api;
using AggregationTestProject.Shared;
using AggregationTestProject.Utilities.Interfaces;
using log4net;

namespace AggregationTestProject.ViewModels
{
    public class CurrentMissionViewModel : BaseViewModel
    {
        private readonly ApplicationApiService _appApiService;
        private readonly ILog _log;
        private readonly SharedState _sharedState;

        private MissionDto _currentMission;
        public MissionDto CurrentMission
        {
            get => _currentMission;
            set { _currentMission = value; OnPropertyChanged(nameof(CurrentMission)); }
        }

        public ICommand GetCurrentTaskCommand { get; }
        public ICommand InitializeCommand { get; }

        public CurrentMissionViewModel(ApplicationApiService applicationApiService, ILog log, SharedState sharedState)
        {
            _appApiService = applicationApiService;
            _log = log;
            _sharedState = sharedState;

            GetCurrentTaskCommand = ICommand.From(GetCurrentTaskAsync);
            InitializeCommand = ICommand.From(Initialize);

            GetCurrentTaskCommand.Execute(this);
            InitializeCommand.Execute(this);
        }

        private async Task Initialize()
        {
            await _appApiService.SetShiftAsync(1);
            var box = await _appApiService.InitializeBoxAsync();
            var pallet = await _appApiService.InitializePalletAsync();

            _sharedState.UpdateCurrentBoxId(this, box.Id);
            _sharedState.UpdateCurrentPalletId(this, pallet.Id);
        }

        private async Task GetCurrentTaskAsync()
        {
            try
            {
                var mission = await _appApiService.GetCurrentMissionAsync();

                if (mission is not null)
                {
                    CurrentMission = mission;

                    if (_sharedState is not null && (_sharedState.CurrentMission is null || !_sharedState.CurrentMission.Equals(mission)))
                    {
                        _sharedState.UpdateCurrentMission(this, mission);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("Current mission view model: server error", ex);

                throw;
            }
        }
    }
}
