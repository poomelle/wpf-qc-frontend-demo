using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.MVVM.Models
{
    public class Report
    {
        public int id { get; set; }
        public string createBy { get; set; }
        public DateTime createDate { get; set; }
        public bool status { get; set; }
    }
}
