using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Specification
    {
        public int id { get; set; }
        public int productId { get; set; }
        public int machineId { get; set; }
        public Models.Product product { get; set; }
        public Models.Instrument machine { get; set; }
        public bool inUse { get; set; }
        public int? temp { get; set; }
        public int? load { get; set; }
        public int? rpm { get; set; }

        // helping properties
        public bool? show { get; set; } = true;
        public bool isEditMode { get; set; } = false;
        public bool isViewMode { get; set; } = true;
    }
}
