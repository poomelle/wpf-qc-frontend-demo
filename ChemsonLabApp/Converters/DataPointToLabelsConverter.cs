using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ChemsonLabApp.Converters
{
    public class DataPointToLabelsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dataPoints = value as ObservableCollection<QcPerformanceGraphData>;
            if (dataPoints == null) return null;
            return dataPoints.Select(dp => dp.XAxisValue).ToArray();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
