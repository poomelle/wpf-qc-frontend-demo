using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Instrument
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool status { get; set; }

        // Helping Properties
        public bool show { get; set; } = true;
        public bool isEditMode { get; set; } = false;
        public bool isViewMode { get; set; } = true;
    }
}
