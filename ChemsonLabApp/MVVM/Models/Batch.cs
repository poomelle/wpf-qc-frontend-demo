using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.MVVM.Models
{
    public class Batch
    {
        public int id { get; set; }
        public string batchName { get; set; }
        public string sampleBy { get; set; }
        public Product product { get; set; }
        public int productId { get; set; }
        public string suffix { get; set; }

        // Batch number in int for helping to order batch
        public int batchNumber { get; set; }

    }
}
