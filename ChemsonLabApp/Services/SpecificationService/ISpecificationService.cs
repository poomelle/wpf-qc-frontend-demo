using ChemsonLabApp.MVVM.Models;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.IService
{
    public interface ISpecificationService
    {
        Task<List<Specification>> GetAllSpecificationsAsync(string filter = "", string sort = "");
        Task<Specification> GetSpecificationByIdAsync(int id);
        Task<Specification> CreateSpecificationAsync(Specification specification);
        Task<Specification> UpdateSpecificationAsync(Specification specification);
        Task<Specification> DeleteSpecificationAsync(Specification specification, string deleteConfirm);
        Task UpdateSpecificationFromProductUpdated(Product updateProduct);
        Task<bool> UpdateSpecificationAndUpdateProduct(Specification specification);
        Task<List<Specification>> GetAllActiveSpecificationsAsync();
        Task CreateOrUpdateSpecificationFromExcel(IXLRow row, List<string> columnNames, List<Instrument> instruments);
        Task<bool> CreateSpecificationAndCreateProduct(Product product, Specification specification);
        Task<bool> CreateSpecificationAndUpdateProduct(Product product, Specification specification);
    }
}
