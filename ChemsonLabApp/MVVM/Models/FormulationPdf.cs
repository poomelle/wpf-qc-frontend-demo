using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.Models
{
    [AddINotifyPropertyChangedInterface]
    public class FormulationPdf
    {
        public string productName { get; set; }
        public bool isQcPdfChecked { get; set; }
        public bool isDpcPdfChecked { get; set; }
        public string formulationBatch { get; set; }
        public string formulationDate { get; set; }
        public string excelFilePath { get; set; }
    }
}
