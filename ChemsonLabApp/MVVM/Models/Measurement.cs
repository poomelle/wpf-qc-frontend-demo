using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.MVVM.Models
{
    public class Measurement
    {
        public int id { get; set; }
        public TestResult testResult { get; set; }
        public int testResultId { get; set; }
        public string timeAct { get; set; }
        public double? torque { get; set; }
        public double? bandwidth { get; set; }
        public double? stockTemp { get; set; }
        public double? speed { get; set; }
        public string fileName { get; set; }
    }
}
