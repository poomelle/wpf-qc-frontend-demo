using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.MVVM.Models
{
    public class QcAveTestTimeKpi
    {
        public int id { get; set; }
        public Product product { get; set; }
        public Instrument instrument { get; set; }
        public string year { get; set; }
        public string month { get; set; }
        public int totalTest { get; set; }
        public long aveTestTime { get; set; }
    }

    
}
