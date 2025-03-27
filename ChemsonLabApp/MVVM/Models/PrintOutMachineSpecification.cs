using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ChemsonLabApp.MVVM.Models
{
    public class PrintOutMachineSpecification
    {
        public int MachineId { get; set; }
        public string MachineName { get; set; }
        public bool MachineStatus { get; set; }
        public string Temp { get; set; }
        public string Load { get; set; }
        public string Rpm { get; set; }

    }
}
