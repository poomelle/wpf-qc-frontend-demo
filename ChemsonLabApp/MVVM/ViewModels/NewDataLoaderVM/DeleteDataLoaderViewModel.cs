using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.NewDataLoaderVM.Command;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Services.DataLoaderService;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.ViewModels.NewDataLoaderVM
{
    [AddINotifyPropertyChangedInterface]
    public class DeleteDataLoaderViewModel
    {
        private readonly IDeleteDataLoaderService _deleteDataLoaderService;

        public string DeleteConfirm { get; set; }
        public BatchTestResult BatchTestResult { get; set; }
        public List<BatchTestResult> BatchTestResults { get; set; }
        public DeleteTestResultCommand DeleteTestResultCommand { get; set; }

        public DeleteDataLoaderViewModel(
            IDeleteDataLoaderService deleteDataLoaderService
            )
        {
            // services
            this._deleteDataLoaderService = deleteDataLoaderService;

            // commands
            DeleteTestResultCommand = new DeleteTestResultCommand(this);
        }

        /// <summary>
        /// Deletes the selected batch test results using the delete data loader service.
        /// Displays a busy cursor during the operation, shows a success notification on completion,
        /// and handles errors by displaying appropriate error messages and logging them.
        /// </summary>
        public async void DeleteBatchTestResultsCommand()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                await _deleteDataLoaderService.DeleteBatchTestResults(BatchTestResults, DeleteConfirm);
                NotificationUtility.ShowSuccess("The Data have been deleted");
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Error: Failed to delete data. Please check internet connection and try again later.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: Failed to delete data. Please try again later.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }
    }
}
