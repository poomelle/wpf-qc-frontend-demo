using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Customer
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public bool status { get; set; }

        // Helping properties
        public bool show { get; set; } = true;
        public bool isViewMode { get; set; } = true;
        public bool isEditMode { get; set; } = false;
    }
}
