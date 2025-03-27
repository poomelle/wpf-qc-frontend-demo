using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.Models
{
    [AddINotifyPropertyChangedInterface]
    public class BatchTestResult
    {
        public int id { get; set; }
        public Batch batch { get; set; }
        public TestResult testResult { get; set; }
        public int batchId { get; set; }
        public int testResultId { get; set; }

        // Helping properties for BatchTestResultReport
        public double torqueDiff { get; set; }
        public double fusionDiff { get; set; }
        public string standardReference { get; set; }
        public bool result { get; set; }
        public string doubleBatchName { get; set; }

        // Helping properties for BatchTestResult Delete selection
        public bool isSelected { get; set; } = false;

        // Helping properties for BatchTestResult Report
        public string reportDisplayResult { get; set; } = "Fail";
        public string batchStatus { get; set; }
        public DateTime testDate { get; set; }
    }
}
