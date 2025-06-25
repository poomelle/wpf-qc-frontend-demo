using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using ChemsonLabApp.Services.IService;

namespace ChemsonLabApp.MVVM.ViewModels.SpecificationVM
{
    [AddINotifyPropertyChangedInterface]
    public class DisplaySpecificationContentControlViewModel
    {
        private readonly ISpecificationService _specificationService;

        public Specification Specification { get; set; }
        public DisplaySpecificationContentControlViewModel(ISpecificationService specificationService)
        {
            this._specificationService = specificationService;
        }

        /// <summary>
        /// Asynchronously retrieves a Specification by its ID and updates the Specification property.
        /// </summary>
        /// <param name="id">The ID of the specification to retrieve.</param>
        public async void GetSpecificationById(int id)
        {
            Specification = await _specificationService.GetSpecificationByIdAsync(id);
        }
    }
}
