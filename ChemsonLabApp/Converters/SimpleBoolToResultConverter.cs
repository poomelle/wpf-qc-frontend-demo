using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ChemsonLabApp.Converters
{
    public class SimpleBoolToResultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool result)
            {
                return result ? "Pass" : "Fail";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == "Pass")
            {
                return true;
            }
            else if (value == "Fail")
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
