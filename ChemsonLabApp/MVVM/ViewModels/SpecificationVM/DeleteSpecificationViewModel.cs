using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.SpecificationVM.Commands;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.ViewModels.SpecificationVM
{
    [AddINotifyPropertyChangedInterface]
    public class DeleteSpecificationViewModel
    {
        private readonly ISpecificationService _specificationService;

        public Specification Specification { get; set; }
        public string DeleteConfirm { get; set; }
        public DeletedSpecificationCommand DeletedSpecificationCommand { get; set; }
        public DeleteSpecificationViewModel(ISpecificationService specificationService)
        {
            // services
            this._specificationService = specificationService;

            // commands
            DeletedSpecificationCommand = new DeletedSpecificationCommand(this);
        }

        /// <summary>
        /// Asynchronously deletes the current specification using the provided confirmation string.
        /// Displays a busy cursor during the operation, shows success or error notifications,
        /// and logs errors as appropriate.
        /// </summary>
        public async void DeleteSpecificationAsync()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                var deletedSpecification = await _specificationService.DeleteSpecificationAsync(Specification, DeleteConfirm);
                if (deletedSpecification != null) NotificationUtility.ShowSuccess("Specification has been deleted.");
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Error: Failed to delete specification. Please check internet connection and try again later.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: Failed to delete specification. Please try again later.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }
    }
}
