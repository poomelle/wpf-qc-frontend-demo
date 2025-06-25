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
        private readonly IDialogService _dialogService;
        private readonly IReportService _reportService;

        public ObservableCollection<TestResultReport> TestResultReports { get; set; } = new ObservableCollection<TestResultReport>();
        public Product SelectedProduct { get; set; }
        public string FromBatchNumber { get; set; }
        public string ToBatchNumber { get; set; }
        public DateTime TestDate { get; set; } = DateTime.Now;
        public double? TorqueWarning { get; set; }
        public double? TorqueFail { get; set; }
        public double? FusionWarning { get; set; }
        public double? FusionFail { get; set; }
        public string Suffix { get; set; }
        public string TestNumber { get; set; } = "1";

        // commands
        public SearchBatchTestResultReportCommand SearchBatchTestResultReportCommand { get; set; }
        public ShowDeleteReportViewCommand ShowDeleteReportViewCommand { get; set; }
        public ShowMakeReportGraphViewCommand ShowMakeReportGraphViewCommand { get; set; }
        public ProductSelectSearchReportCommand ProductSelectSearchReportCommand { get; set; }
        public FromBatchChangeSearchReportCommand FromBatchChangeSearchReportCommand { get; set; }
        public ToBatchChangeSearchReportCommand ToBatchChangeSearchReportCommand { get; set; }
        public SuffixRadioButtonChangeReportCommand SuffixRadioButtonChangeReportCommand { get; set; }
        public OpenReportFile OpenReportFile { get; set; }

        public SearchReportViewModel(
            IDialogService dialogService,
            IReportService reportService
            )
        {
            SearchBatchTestResultReportCommand = new SearchBatchTestResultReportCommand(this);
            ShowDeleteReportViewCommand = new ShowDeleteReportViewCommand(this);
            ShowMakeReportGraphViewCommand = new ShowMakeReportGraphViewCommand(this);
            ProductSelectSearchReportCommand = new ProductSelectSearchReportCommand(this);
            FromBatchChangeSearchReportCommand = new FromBatchChangeSearchReportCommand(this);
            ToBatchChangeSearchReportCommand = new ToBatchChangeSearchReportCommand(this);
            SuffixRadioButtonChangeReportCommand = new SuffixRadioButtonChangeReportCommand(this);
            OpenReportFile = new OpenReportFile(this);

            this._dialogService = dialogService;
            this._reportService = reportService;
        }

        /// <summary>
        /// Searches for test result reports based on the selected product, batch number range, test number, and suffix.
        /// Validates input fields, loads the test result reports using the report service, and populates the TestResultReports collection.
        /// Displays notifications if no data is found or if an error occurs during the search.
        /// </summary>
        public async void SearchTestResultReport()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                if (!InputValidationUtility.ValidateNotNullObject(SelectedProduct, "Product")) return;
                if (!InputValidationUtility.ValidateBatchNumberFormat(FromBatchNumber) || !InputValidationUtility.ValidateNotNullInput(FromBatchNumber, "From Batch Number")) return;

                var testResultReports = await _reportService.GetProductTestResultReportsWithBatchRange(SelectedProduct, FromBatchNumber, ToBatchNumber, TestNumber, Suffix);

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

        /// <summary>
        /// Populates the TestResultReports collection with the provided list of test result reports.
        /// Clears any existing reports, adds the new ones, and updates the torque and fusion warning/fail values
        /// based on the product information from the first report in the list.
        /// </summary>
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

        /// <summary>
        /// Opens the report file associated with the specified TestResultReport.
        /// Displays a loading cursor while attempting to open the file using the dialog service.
        /// Shows an error notification if the file cannot be opened and logs the exception.
        /// </summary>
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

        /// <summary>
        /// Opens the delete report confirmation dialog for the specified test result report.
        /// </summary>
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
