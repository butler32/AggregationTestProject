using AggregationTestProject.ViewModels;
using Unity;

namespace AggregationTestProject.Extensions
{
    public static class ViewModelsRegistrationExtension
    {
        public static IUnityContainer RegisterViewModels(this IUnityContainer container)
        {
            container.RegisterSingleton<MainWindowViewModel>();
            container.RegisterSingleton<CurrentMissionViewModel>();
            container.RegisterSingleton<BoxAssembleViewModel>();
            container.RegisterSingleton<PalletAssembleViewModel>();

            return container;
        }
    }
}
