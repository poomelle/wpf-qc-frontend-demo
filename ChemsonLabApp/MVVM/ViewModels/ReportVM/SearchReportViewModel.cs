using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.ReportVM.Command;
using ChemsonLabApp.MVVM.Views.Report;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Services;
using ChemsonLabApp.Services.DialogService;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PropertyChanged;
using SharpVectors.Scripting;

namespace ChemsonLabApp.MVVM.ViewModels.ReportVM
{
    [AddINotifyPropertyChangedInterface]
    public class SearchReportViewModel
    {
        private readonly IProductService _productService;
        private readonly IDialogService _dialogService;
        private readonly IReportService _reportService;

        public ObservableCollection<TestResultReport> TestResultReports { get; set; } = new ObservableCollection<TestResultReport>();
        public List<Product> Products { get; set; } = new List<Product>();
        public Product SelectedProduct { get; set; }
        public string FromBatchNumber { get; set; }
        public string ToBatchNumber { get; set; }
        public DateTime TestDate { get; set; } = DateTime.Now;
        public double? TorqueWarning { get; set; }
        public double? TorqueFail { get; set; }
        public double? FusionWarning { get; set; }
        public double? FusionFail { get; set; }

        // Suffix for batch number
        public bool Default { get; set; } = true;
        public bool RS { get; set; } = false;
        public bool RRS { get; set; } = false;
        public bool RT { get; set; } = false;
        public bool Remix { get; set; } = false;
        public bool Min2 { get; set; } = false;
        public bool Min4 { get; set; } = false;
        public bool Cal { get; set; } = false;

        // commands
        public SearchBatchTestResultReportCommand SearchBatchTestResultReportCommand { get; set; }
        public ShowDeleteReportViewCommand ShowDeleteReportViewCommand { get; set; }
        public ShowMakeReportGraphViewCommand ShowMakeReportGraphViewCommand { get; set; }
        public OpenReportFile OpenReportFile { get; set; }

        public SearchReportViewModel(
            IProductService productService,
            IDialogService dialogService,
            IReportService reportService
            )
        {
            SearchBatchTestResultReportCommand = new SearchBatchTestResultReportCommand(this);
            ShowDeleteReportViewCommand = new ShowDeleteReportViewCommand(this);
            ShowMakeReportGraphViewCommand = new ShowMakeReportGraphViewCommand(this);
            OpenReportFile = new OpenReportFile(this);

            this._productService = productService;
            this._dialogService = dialogService;
            this._reportService = reportService;

            InitializeParameter();
        }

        public async void InitializeParameter()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                Products.Clear();
                Products = await _productService.LoadActiveProducts();
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Network error: Unable to retrieve test results. Please check your internet connection.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: Failed to load products. Please try again later.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        public async void SearchTestResultReport()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                string suffix = RS ? "RS" :
                                RRS ? "RRS" :
                                RT ? "RT" :
                                Remix ? "Remix" :
                                Min2 ? "2.00min" :
                                Min4 ? "4.00min" :
                                Cal ? "Cal" : null;

                if (!InputValidationUtility.ValidateNotNullObject(SelectedProduct, "Product")) return;
                if (!InputValidationUtility.ValidateBatchNumberFormat(FromBatchNumber) || !InputValidationUtility.ValidateNotNullInput(FromBatchNumber, "From Batch Number")) return;

                var testResultReports = await _reportService.GetProductTestResultReportsWithBatchRange(SelectedProduct, FromBatchNumber, ToBatchNumber);

                if (testResultReports.Count == 0)
                {
                    NotificationUtility.ShowWarning("Data not found!");
                    return;
                }

                PopulateTestResultReports(testResultReports);
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Error: Failed to load test results. Please check internet connection.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: Failed to load test results.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        private void PopulateTestResultReports(List<TestResultReport> testResultReports)
        {
            TestResultReports.Clear();
            foreach (var testResultReport in testResultReports)
            {
                TestResultReports.Add(testResultReport);
            }

            TorqueWarning = testResultReports[0].batchTestResult.testResult.product.torqueWarning;
            TorqueFail = testResultReports[0].batchTestResult.testResult.product.torqueFail;
            FusionWarning = testResultReports[0].batchTestResult.testResult.product.fusionWarning;
            FusionFail = testResultReports[0].batchTestResult.testResult.product.fusionFail;
        }

        public void OnOpenReportFile(TestResultReport testResultReport)
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                _dialogService.ShowOpenReportView(testResultReport.fileLocation);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: Failed to open report file.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        public void PopupDeleteReportView(TestResultReport testResultReport)
        {
            _dialogService.ShowDeleteView(testResultReport);
        }

        public async void ShowMakeReportGraphView(TestResultReport testResultReport)
        {
            try
            {
                await _dialogService.ShowMakeReportGraphView(testResultReport);
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Error: Failed to load test results. Please check internet connection.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: Failed to load test results.");
                LoggerUtility.LogError(ex);
            }
        }
    }
}
