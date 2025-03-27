using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ChemsonLabApp.MVVM.Models
{
    public class PrintOutSpecification
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public bool ProductStatus { get; set; }
        public double? SampleAmount { get; set; }
        public DateTime? DbDate { get; set; }
        public string Comment { get; set; }
        public bool? Coa { get; set; }
        public string Colour { get; set; }
        public double? TorqueWarning { get; set; }
        public double? TorqueFail { get; set; }
        public double? FusionWarning { get; set; }
        public double? FusionFail { get; set; }
        public List<PrintOutMachineSpecification> MachineSpecs { get; set; } = new List<PrintOutMachineSpecification>();
    }
}
