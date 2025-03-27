using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.Models
{
    [AddINotifyPropertyChangedInterface]
    public class TestResult
    {
        public int id { get; set; }
        public Product product { get; set; }
        public Instrument machine { get; set; }
        public int productId { get; set; }
        public int machineId { get; set; }
        public string testDate { get; set; }
        public string operatorName { get; set; }
        public string driveUnit { get; set; }
        public string mixer { get; set; }
        public string loadingChute { get; set; }
        public string additive { get; set; }
        public double? speed { get; set; }
        public double? mixerTemp { get; set; }
        public double? startTemp { get; set; }
        public int? measRange { get; set; }
        public int? damping { get; set; }
        public double? testTime { get; set; }
        public double? sampleWeight { get; set; }
        public string codeNumber { get; set; }
        public string plasticizer { get; set; }
        public double? plastWeight { get; set; }
        public double? loadTime { get; set; }
        public double? loadSpeed { get; set; }
        public string liquid { get; set; }
        public double? titrate { get; set; }
        public int testNumber { get; set; }
        public string testType { get; set; }
        public string batchGroup { get; set; }
        public string testMethod { get; set; }
        public string colour { get; set; }
        public bool status { get; set; }
        public string fileName { get; set; }

        // helping properties for Batch
        public string batchName { get; set; }
        public string sampleBy { get; set; }
        public string suffix { get; set; }

        // helping properties for set default
        public string sampleNameFromMTF { get; set; }

        // List of Evaluation's Torque at point x and fusion at point t to calculate
        public double torque { get; set; }
        public int torqueId { get; set; }
        public int fusion { get; set; }
        public int fusionId { get; set; }

    }
}
