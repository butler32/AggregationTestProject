using AggregationTestProject.DTOs;
using AggregationTestProject.Models;

namespace AggregationTestProject.Shared
{
    public class SharedState
    {
        public MissionDto CurrentMission { get; private set; }
        public EventHandler<MissionDto> UpdateCurrentMission;

        public bool IsMissionRunning { get; private set; }
        public EventHandler<bool> UpdateMissionRunning;

        public int CurrentBoxId { get; private set; }
        public EventHandler<int> UpdateCurrentBoxId;

        public int CurrentPalletId { get; private set; }
        public EventHandler<int> UpdateCurrentPalletId;

        public bool IsBoxAssView {  get; private set; }
        public EventHandler<bool> UpdateBoxAssView;

        public bool IsPalletAssView { get; private set; }
        public EventHandler<bool> UpdatePalletAssView;

        public SharedState()
        {
            UpdateCurrentMission += (sender, e) => CurrentMission = e;
            UpdateMissionRunning += (sender, e) => IsMissionRunning = e;
            UpdateCurrentBoxId += (sender, e) => CurrentBoxId = e;
            UpdateCurrentPalletId += (sender, e) => CurrentPalletId = e;
            UpdateBoxAssView += (sender, e) => IsBoxAssView = e;
            UpdatePalletAssView += (sender, e) => IsPalletAssView = e;
        }
    }
}
