using Unity;

namespace AggregationTestProject
{
    public class Program
    {
        private static App _app;
        private static IUnityContainer _container;
        public static IUnityContainer Container
        {
            get => _container;
            set => _container = value;
        }

        [STAThread]
        public static void Main()
        {
            Container = new UnityContainer().AddExtension(new Diagnostic());
            Container.RegisterType<App>();
            _app = Container.Resolve<App>();
            _app.Run();
        }
    }
}
