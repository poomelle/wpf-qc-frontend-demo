using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using ChemsonLabApp.MVVM.ViewModels.SpecificationVM.Commands;
using System.Windows;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using System.Net.Http;

namespace ChemsonLabApp.MVVM.ViewModels.SpecificationVM
{
    [AddINotifyPropertyChangedInterface]
    public class EditSpecifiationContentControlView
    {
        private readonly ISpecificationService _specificationService;

        public Specification Specification { get; set; }
        public List<string> COAComboBoxItems { get; set; } = new List<string> { "COA", "None" };
        public List<string> Status { get; } = new List<string> { "Active", "Inactive" };
        public SaveUpdateSpecificationCommand SaveUpdateSpecificationCommand { get; set; }

        public EditSpecifiationContentControlView(
            ISpecificationService specificationService 
            )
        {
            // services
            this._specificationService = specificationService;

            // Commands
            SaveUpdateSpecificationCommand = new SaveUpdateSpecificationCommand(this);
        }

        /// <summary>
        /// Saves the current Specification and its associated Product by calling the specification service.
        /// Displays a loading cursor during the operation, shows a success notification if the update succeeds,
        /// and handles errors by displaying appropriate error messages and logging them.
        /// </summary>
        public async void SaveSpecificationAndProduct()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                var isUpdateSuccess = await _specificationService.UpdateSpecificationAndUpdateProduct(Specification);
                if (isUpdateSuccess) NotificationUtility.ShowSuccess("Specification and Product updated!");
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Error: Failed to update specification. Please check internet connection and try again later.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: Failed to update specification. Please try again later.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        /// <summary>
        /// Asynchronously retrieves a Specification by its ID and assigns it to the Specification property.
        /// </summary>
        /// <param name="id">The ID of the Specification to retrieve.</param>
        public async void GetSpecificationById(int id)
        {
            Specification = await _specificationService.GetSpecificationByIdAsync(id);
        }
    }
}
