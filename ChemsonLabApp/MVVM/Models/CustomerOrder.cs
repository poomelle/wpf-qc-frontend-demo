using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.Models
{
    [AddINotifyPropertyChangedInterface]
    public class CustomerOrder
    {
        public int id { get; set; }
        public Customer customer { get; set; }
        public int customerId { get; set; }
        public Product product { get; set; }
        public int productId { get; set; }
        public bool status { get; set; }
        public bool show { get; set; } = true;
        public bool isViewMode { get; set; } = true;
        public bool isEditMode { get; set; } = false;
    }
}
