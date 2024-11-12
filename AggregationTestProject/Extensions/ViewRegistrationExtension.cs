using Unity;
using Unity.Lifetime;

namespace AggregationTestProject.Extensions
{
    public static class ViewRegistrationExtension
    {
        public static IUnityContainer RegisterViews(this IUnityContainer container)
        {
            return container;
        }
    }
}
