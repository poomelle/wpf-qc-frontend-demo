using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.RestAPI.IRestAPI
{
    public interface IQcLabelRestAPI
    {
        Task<List<QCLabel>> GetAllQCLabelsAsync(string filter = "", string sort = "");
        Task<QCLabel> GetQCLabelByIdAsync(int id);
        Task<QCLabel> CreateQCLabelAsync(QCLabel qcLabel);
        Task<QCLabel> UpdateQCLabelAsync(QCLabel qcLabel);
        Task<QCLabel> DeleteQCLabelAsync(QCLabel qcLabel);
    }
}
