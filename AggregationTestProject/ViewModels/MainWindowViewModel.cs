using AggregationTestProject.Shared;
using AggregationTestProject.Utilities.Interfaces;
using AggregationTestProject.Views;
using System.Windows.Controls;
using Unity;

namespace AggregationTestProject.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private SharedState _sharedState = Program.Container.Resolve<SharedState>();

        private UserControl _currentView;
        public UserControl CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand CurrentMissionCommand => ICommand.From(() => CurrentView =  new CurrentMissionView());
        public ICommand DeviceDiscoverCommand => ICommand.From(() => CurrentView = new DeviceDiscovererView());
        public ICommand BoxAssembleCommand => ICommand.From(() =>
        {
            _sharedState.UpdateBoxAssView(this, true);
            _sharedState.UpdatePalletAssView(this, false);

            CurrentView = new BoxAssembleView();
        });
        public ICommand PalletAssembleCommand => ICommand.From(() =>
        {
            _sharedState.UpdatePalletAssView(this, true);
            _sharedState.UpdateBoxAssView(this, false);

            CurrentView = new PalletAssembleView();
        });
        public ICommand MissionInfoPresenterAssembleCommand => ICommand.From(() => CurrentView =  new MissionInfoPresenter());


        public MainWindowViewModel()
        {
            CurrentMissionCommand.Execute(this);
        }
    }
}
