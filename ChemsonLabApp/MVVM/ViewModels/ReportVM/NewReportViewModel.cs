using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
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

namespace ChemsonLabApp.MVVM.ViewModels.ReportVM
{
    [AddINotifyPropertyChangedInterface]
    public class NewReportViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly IReportService _reportService;

        public ObservableCollection<BatchTestResult> BatchTestResults { get; set; } = new ObservableCollection<BatchTestResult>();
        public string FromBatch { get; set; }
        public string ToBatch { get; set; }
        public DateTime TestDate { get; set; } = DateTime.Now;
        public Product SelectedProduct { get; set; }
        public List<string> Standards { get; set; } = new List<string>();
        public string SelectedSTD { get; set; } = "STD2";
        public double? TorqueWarning { get; set; }
        public double? TorqueFail { get; set; }
        public double? FusionWarning { get; set; }
        public double? FusionFail { get; set; }
        public Dictionary<string,List<double>> TorqFusValues { get; set; } = new Dictionary<string, List<double>>();
        public string TestNumber { get; set; } = "1";
        public string Suffix { get; set; }

        // commands
        public SearchBatchTestResultCommand SearchBatchTestResultCommand { get; set; }
        public CalculateTorFusCommand CalculateTorFusCommand { get; set; }
        public MakeReportCommand MakeReportCommand { get; set; }
        public RemoveBatchTestResultCommand RemoveBatchTestResultCommand { get; set; }
        public ProductSelectNewReportCommand ProductSelectNewReportCommand { get; set; }
        public FromBatchChangeNewReportCommand FromBatchChangeNewReportCommand { get; set; }
        public ToBatchChangeNewReportCommnad ToBatchChangeNewReportCommnad { get; set; }
        public SuffixRadioButtonChangeReportCommand SuffixRadioButtonChangeReportCommand { get; set; }

        public NewReportViewModel(
            IDialogService dialogService,
            IReportService reportService)
        {
            // Command
            SearchBatchTestResultCommand = new SearchBatchTestResultCommand(this);
            CalculateTorFusCommand = new CalculateTorFusCommand(this);
            MakeReportCommand = new MakeReportCommand(this);
            RemoveBatchTestResultCommand = new RemoveBatchTestResultCommand(this);
            ProductSelectNewReportCommand = new ProductSelectNewReportCommand(this);
            FromBatchChangeNewReportCommand = new FromBatchChangeNewReportCommand(this);
            ToBatchChangeNewReportCommnad = new ToBatchChangeNewReportCommnad(this);
            SuffixRadioButtonChangeReportCommand = new SuffixRadioButtonChangeReportCommand(this);

            // Independent Injection
            this._dialogService = dialogService;
            this._reportService = reportService;
        }

        /// <summary>
        /// Searches for batch test results based on the selected product, batch number range, test date, test number, and suffix.
        /// Loads the batch test results using the report service, populates the standards combo box, and calculates torque and fusion differences.
        /// Populates the BatchTestResults collection with the results.
        /// Displays notifications if no data is found or if an error occurs during the search.
        /// </summary>
        public async void SearchTestResults()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                if (!InputValidationUtility.ValidateNotNullObject(SelectedProduct, "Product")) return;

                TorqueWarning = SelectedProduct.torqueWarning;
                TorqueFail = SelectedProduct.torqueFail;
                FusionWarning = SelectedProduct.fusionWarning;
                FusionFail = SelectedProduct.fusionFail;

                var searchTestDate = TestDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                var batchTestResults = await _reportService.GetAllBatchTestResultsForMakingReport(SelectedProduct, FromBatch.ToUpper(), ToBatch.ToUpper(), searchTestDate, TestNumber, Suffix);

                PopulateStdComboBox(batchTestResults);

                if (batchTestResults.Count > 0)
                {
                    var standardResult = batchTestResults.FirstOrDefault(btr => btr.batch.batchName == SelectedSTD);
                    batchTestResults = _reportService.CalculateTorDiffAndFusDiff(batchTestResults, standardResult);

                    PopulateDataToBatchTestResults(batchTestResults);
                }
                else
                {
                    NotificationUtility.ShowWarning("No test results found");
                }

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
        /// Populates the Standards combo box with standard batch names (e.g., "STD1", "STD2", ...)
        /// based on the number of batch test results of type "STD" found in the provided list.
        /// Clears the existing Standards list before adding new entries.
        /// </summary>
        private void PopulateStdComboBox(List<BatchTestResult> batchTestResults)
        {
            var stdBatchTestResults = batchTestResults.Where(results => results.testResult.testType == "STD").ToList();
            if (stdBatchTestResults.Count > 0)
            {
                Standards.Clear();
                for (int i = 0; i < stdBatchTestResults.Count; i++)
                {
                    Standards.Add($"STD{i + 1}");
                }
            }
        }

        /// <summary>
        /// Populates the BatchTestResults collection with unique BatchTestResult items from the provided list.
        /// Ensures that no duplicate BatchTestResult (by id) is added to the collection.
        /// </summary>
        private void PopulateDataToBatchTestResults(List<BatchTestResult> batchTestResults)
        {
            foreach (var batchTestResult in batchTestResults)
            {
                if (!BatchTestResults.Any(result => result.id == batchTestResult.id))
                {
                    BatchTestResults.Add(batchTestResult);
                }
            }
        }

        /// <summary>
        /// Recalculates the torque and fusion differences for all batch test results of type "BCH" 
        /// based on the currently selected standard batch (SelectedSTD). 
        /// Updates each batch's standard reference, torqueDiff, fusionDiff, and result properties.
        /// </summary>
        public void OnStdChangedCalculateTorAndFus()
        {
            var standardResult = BatchTestResults.FirstOrDefault(btr => btr.batch.batchName == SelectedSTD);

            if (standardResult != null)
            {
                var refTorque = standardResult.testResult.torque;
                var refFusion = (double)standardResult.testResult.fusion;

                foreach (var batchTestResult in BatchTestResults)
                {
                    if (batchTestResult.testResult.testType == "BCH")
                    {
                        batchTestResult.standardReference = SelectedSTD;

                        batchTestResult.torqueDiff = Math.Round((batchTestResult.testResult.torque - refTorque) / refTorque * 100, 2);

                        batchTestResult.fusionDiff = Math.Round((batchTestResult.testResult.fusion - refFusion) / refFusion * 100, 2);

                        batchTestResult.result = Math.Abs(batchTestResult.torqueDiff) <= TorqueFail && Math.Abs(batchTestResult.fusionDiff) <= FusionFail;
                    }
                }
            }
        }

        /// <summary>
        /// Opens the Make Report window if there are batch test results and a standard is selected.
        /// Checks and updates evaluation results using the report service, then displays the Make Report view.
        /// Shows a warning notification if no test results are found or no standard is selected.
        /// </summary>
        public async void PopupMakeReportWindow()
        {
            if (BatchTestResults.Count > 0 && !string.IsNullOrWhiteSpace(SelectedSTD))
            {
                var batchTestResults = await _reportService.CheckAndUpdateEvaluationResults(BatchTestResults.ToList());

                _dialogService.ShowMakeReportView(batchTestResults);

            }
            else
            {
                NotificationUtility.ShowWarning("No test results found");
            }
        }

        /// <summary>
        /// Removes the specified BatchTestResult from the BatchTestResults collection.
        /// Iterates through the collection and removes the first occurrence that matches the provided batchTestResult reference.
        /// </summary>
        public void RemoveBatchTestResult(BatchTestResult batchTestResult)
        {
            for (int i = 0; i < BatchTestResults.Count; i++)
            {
                if (batchTestResult == BatchTestResults[i])
                {
                    BatchTestResults.RemoveAt(i);
                }
            }
        }
    }
}
