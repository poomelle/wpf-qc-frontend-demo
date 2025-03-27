using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.Models
{
    [AddINotifyPropertyChangedInterface]
    public class DailyQc
    {
        // properties from database
        public int id { get; set; }
        public DateTime incomingDate { get; set; }
        public int? productId { get; set; }
        public Product product { get; set; }
        public int? priority { get; set; }
        public string comment { get; set; }
        public string batches { get; set; }
        public string stdReqd { get; set; }
        public string extras { get; set; }
        public int? mixesReqd { get; set; }
        public int? mixed { get; set; }
        public string testStatus { get; set; }
        public DateTime? testedDate { get; set; }
        public string year { get; set; }
        public string month { get; set; }
        public string lastBatch { get; set; }
        public string lastLabel { get; set; }

        // helping properties for last tested batch and last printed label
        public string productName { get; set; }
        public bool isSelected { get; set; } = false;

        // helping properties for creating DPC
        public bool isDPCCreated { get; set; }

        // helping properties for Last COA BatchName
        public string lastCoaBatchName { get; set; }
        public bool isLastBatchLoaded { get; set; }
        public bool isLastLabelLoaded { get; set; }
    }
}
