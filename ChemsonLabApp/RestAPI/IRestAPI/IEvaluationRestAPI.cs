using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.RestAPI.IRestAPI
{
    public interface IEvaluationRestAPI
    {
        Task<List<Evaluation>> GetAllEvaluationsAsync(string filter = "", string sort = "");
        Task<Evaluation> GetEvaluationByIdAsync(int id);
        Task<Evaluation> CreateEvaluationAsync(Evaluation evaluation);
        Task<Evaluation> UpdateEvaluationAsync(Evaluation evaluation);
        Task<Evaluation> DeleteEvaluationAsync(Evaluation evaluation);
    }
}
