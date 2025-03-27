using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.NewDataLoaderVM.Command;
using ChemsonLabApp.MVVM.Views.DataLoader;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Services.DataLoaderService;
using ChemsonLabApp.Services.DialogService;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.ViewModels.NewDataLoaderVM
{
    [AddINotifyPropertyChangedInterface]
    public class EditDataLoaderViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly IEditDataLoaderService _editDataLoaderService;

        public BatchTestResult BatchTestResult { get; set; }
        public Batch Batch { get; set; }
        public TestResult TestResult { get; set; }
        public Evaluation EvaluationX { get; set; }
        public Evaluation EvaluationT { get; set; }
        public bool EditMode { get; set; } = false;
        public bool ViewMode { get; set; } = true;
        public EditModeToggleCommand EditModeToggleCommand { get; set; }
        public UpdateDataLoaderCommand UpdateDataLoaderCommand { get; set; }
        public ShowDeleteDataLoaderViewCommand ShowDeleteDataLoaderViewCommand { get; set; }

        public EditDataLoaderViewModel(
            IDialogService dialogService,
            IEditDataLoaderService editDataLoaderService
            )
        {
            // services
            this._dialogService = dialogService;
            this._editDataLoaderService = editDataLoaderService;

            // commands
            EditModeToggleCommand = new EditModeToggleCommand(this);
            UpdateDataLoaderCommand = new UpdateDataLoaderCommand(this);
            ShowDeleteDataLoaderViewCommand = new ShowDeleteDataLoaderViewCommand(this);
        }


        public async void InitializeParameters()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                await GetBatchAndTestResult();
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Error: Failed to load data. Please check internet connection and try again later.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: Failed to load data. Please try again later.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        public async Task GetBatchAndTestResult()
        {
            Batch = await _editDataLoaderService.GetBatchInformation(BatchTestResult);
            TestResult = await _editDataLoaderService.GetTestResultInformation(BatchTestResult);
            EvaluationX = await _editDataLoaderService.GetEvaluationAtPoint(BatchTestResult, "X");
            EvaluationT = await _editDataLoaderService.GetEvaluationAtPoint(BatchTestResult, "t");
        }

        public async void UpdateDataLoader()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                await _editDataLoaderService.UpdateDataLoader(TestResult, Batch, BatchTestResult, EvaluationX, EvaluationT);
                NotificationUtility.ShowSuccess("The Data have been updated");
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Error: Fail to update data. Please check internet connection and try again later.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: Fail to update data. Please try again later.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
                EditModeToggle();
            }
        }

        public async void EditModeToggle()
        {
            EditMode = !EditMode;
            ViewMode = !ViewMode;
            if (ViewMode)
            {
                await GetBatchAndTestResult();
            }
        }

        public void PopupDeleteDataLoaderView()
        {
            var batchTestResults = new List<BatchTestResult>() { BatchTestResult };
            _dialogService.ShowDeleteDataLoaderView(batchTestResults);
        }
    }
}
