using AggregationTestProject.Models;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AggregationTestProject.Utilities.Converters
{
    public class DeviceFoundToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value)
                {
                    return new SolidColorBrush(Colors.Green);
                }
                else
                {
                    return new SolidColorBrush(Colors.Red);
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
