using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.MVVM.Models
{
    public class TestResultReport
    {
        public int id { get; set; }
        public Report report { get; set; }
        public int reportId { get; set; }
        public BatchTestResult batchTestResult { get; set; }
        public int batchTestResultId { get; set; }
        public Specification productSpecification { get; set; }
        public int productSpecificationId { get; set; }
        public string standardReference { get; set; }
        public double? torqueDiff { get; set; }
        public double? fusionDiff { get; set; }
        public bool result { get; set; }
        public long? aveTestTime { get; set; }
        public string fileLocation { get; set; }

        //helping properties
        public int? batchNumber { get; set; }
    }
}
