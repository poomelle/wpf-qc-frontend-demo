using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.Models
{
    [AddINotifyPropertyChangedInterface]
    public class QCLabel
    {
        public int id { get; set; }
        public bool printed { get; set; }
        public string batchName { get; set; }
        public string weight { get; set; }
        public Product product { get; set; }
        public int productId { get; set; }
        public string year { get; set; }
        public string month { get; set; }
    }
}
