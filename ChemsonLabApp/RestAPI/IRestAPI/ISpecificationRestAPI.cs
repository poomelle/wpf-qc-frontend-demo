using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.RestAPI.IRestAPI
{
    public interface ISpecificationRestAPI
    {
        Task<List<Specification>> GetAllSpecificationsAsync(string filter = "", string sort = "");
        Task<Specification> GetSpecificationByIdAsync(int id);
        Task<Specification> CreateSpecificationAsync(Specification specification);
        Task<Specification> UpdateSpecificationAsync(Specification specification);
        Task<Specification> DeleteSpecificationAsync(Specification specification);
    }
}
