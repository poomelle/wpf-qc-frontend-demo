using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ChemsonLabApp.Converters
{
    public class DateFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                return dateTime.ToString("dd/MM/yyyy");
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string dateString)
            {
                if (DateTime.TryParseExact(dateString, "dd/MM/yyyy", culture, DateTimeStyles.None, out DateTime dateTime))
                {
                    return dateTime;
                }
            }
            return null;
        }
    }
}
