using log4net;
using log4net.Config;
using Unity;
using Unity.Extension;

namespace AggregationTestProject.Extensions
{
    public class Log4NetExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo("log4net.config"));
            Container.RegisterInstance(typeof(ILog), LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!));
        }
    }
}
