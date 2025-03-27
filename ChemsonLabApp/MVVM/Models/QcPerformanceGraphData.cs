using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.Models
{
    [AddINotifyPropertyChangedInterface]
    public class QcPerformanceGraphData
    {
        public string XAxisValue { get; set; }
        public double YAxisValue { get; set; }
        public TimeSpan YAxisAveTestTimeValue { get; set; }
    }
}
