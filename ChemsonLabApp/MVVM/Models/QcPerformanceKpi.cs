using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.Models
{
    [AddINotifyPropertyChangedInterface]
    public class QcPerformanceKpi
    {
        public int id { get; set; }
        public Product product { get; set; }
        public Instrument instrument { get; set; }
        public string year { get; set; }
        public string month { get; set; }
        public int totalTest { get; set; }
        public int firstPass { get; set; }
        public int secondPass { get; set; }
        public int thirdPass { get; set; }
        public int productId { get; set; }
        public int instrumentId { get; set; }
    }
}
