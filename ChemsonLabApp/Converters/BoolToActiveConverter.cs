using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ChemsonLabApp.Converters
{
    public class BoolToActiveConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool status)
            {
                return status ? "Active" : "Inactive";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == "Active")
            {
                return true;
            }
            else if (value == "Inactive")
            {
                return false;
            }
            else
            {
                return null;
            }
        }
    }
}
