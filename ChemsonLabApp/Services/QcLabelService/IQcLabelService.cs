using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.IService
{
    public interface IQcLabelService
    {
        Task<List<QCLabel>> GetAllQcLabelsAsync(string filter = "", string sort = "");
        Task<QCLabel> GetQcLabelByIdAsync(int id);
        Task<QCLabel> CreateQcLabelAsync(QCLabel qcLabel);
        Task<QCLabel> UpdateQcLabelAsync(QCLabel qcLabel);
        Task<QCLabel> DeleteQcLabelAsync(QCLabel qcLabel);
        List<QCLabel> PopulateQcLabels(Product product, string batchStart, string batchEnd, string weight);
        Task<bool> CreateQcLabelFromListAsync(List<QCLabel> qcLabels);
    }
}
