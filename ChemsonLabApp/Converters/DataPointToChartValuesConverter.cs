using ChemsonLabApp.MVVM.Models;
using LiveCharts;
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
    public class DataPointToChartValuesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dataPoints = value as ObservableCollection<QcPerformanceGraphData>;
            if (dataPoints == null) return null;
            return new ChartValues<double>(dataPoints.Select(dp => dp.YAxisValue));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
