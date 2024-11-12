using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AggregationTestProject.Utilities.Converters
{
    public class DeviceConnectingToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null)
            {
                if ((bool)value)
                {
                    return new SolidColorBrush(Colors.Green);
                }
                else
                {
                    return new SolidColorBrush(Colors.Yellow);
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
