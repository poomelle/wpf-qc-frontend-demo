using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Product
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool status { get; set; }
        public DateTime? dbDate { get; set; }
        public double? sampleAmount { get; set; }
        public string comment { get; set; }
        public bool? coa { get; set; }
        public string colour { get; set; }
        public double? torqueWarning { get; set; }
        public double? torqueFail { get; set; }
        public double? fusionWarning { get; set; }
        public double? fusionFail { get; set; }
        public DateTime? updateDate { get; set; }
        public double? bulkWeight { get; set; }
        public double? paperBagWeight { get; set; }
        public int? paperBagNo { get; set; }
        public double? batchWeight { get; set; }
        public bool show { get; set; } = true;
        public bool isViewMode { get; set; } = true;
        public bool isEditMode { get; set; } = false;
    }
}
