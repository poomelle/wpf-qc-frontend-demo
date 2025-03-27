using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.MVVM.Models
{
    public class Evaluation
    {
        public int id { get; set; }
        public TestResult testResult { get; set; }
        public int testResultId { get; set; }
        public int point { get; set; }
        public string pointName { get; set; }
        public string timeEval { get; set; }
        public double torque { get; set; }
        public double? bandwidth { get; set; }
        public int? stockTemp { get; set; }
        public double? speed { get; set; }
        public double? energy { get; set; }
        public string? timeRange { get; set; }
        public int? torqueRange { get; set; }
        public int timeEvalInt { get; set; }
        public int? timeRangeInt { get; set; }
        public string fileName { get; set; }
    }
}
