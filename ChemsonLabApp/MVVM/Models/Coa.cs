using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.MVVM.Models
{
    public class Coa
    {
        public int id { get; set; }
        public int productId { get; set; }
        public Product Product { get; set; }
        public string batchName { get; set; }
    }
}
