using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.COAVM.Commands;
using ChemsonLabApp.MVVM.ViewModels.QCLabelVM;
using ChemsonLabApp.MVVM.Views.COA;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Services.DialogService;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.ViewModels.COAVM
{
    [AddINotifyPropertyChangedInterface]
    public class COAViewModel
    {
        private readonly IProductService _productService;
        private readonly ICoaService _coaService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<TestResultReport> TestResultReports { get; set; } = new ObservableCollection<TestResultReport>();
        public List<Product> Products { get; set; } = new List<Product>();
        public Product SelectedProduct { get; set; }
        public string FromBatch { get; set; }
        public string ToBatch { get; set; }
        public double? TorqueWarning { get; set; }
        public double? TorqueFail { get; set; }
        public double? FusionWarning { get; set; }
        public double? FusionFail { get; set; }
        public bool IsLoading { get; set; }
        public string ErrorMessage { get; set; }
        public bool HasError { get; set; }
        public string PONumber { get; set; } = "";
        public ShowMakeCOAViewCommand ShowMakeCOAViewCommand { get; set; }
        public SearchTestResultReportCommand SearchTestResultReportCommand { get; set; }
        public RemoveReportCommand RemoveReportCommand { get; set; }

        public COAViewModel(
            IProductService productService,
            ICoaService coaService,
            IDialogService dialogService
            )
        {
            this._productService = productService;
            this._coaService = coaService;
            this._dialogService = dialogService;

            // commands
            SearchTestResultReportCommand = new SearchTestResultReportCommand(this);
            ShowMakeCOAViewCommand = new ShowMakeCOAViewCommand(this);
            RemoveReportCommand = new RemoveReportCommand(this);

            // initialize
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
                NotificationUtility.ShowError("An unexpected error occurred. Please try again or contact support.");
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
                if (!InputProductBatchValidation()) return;

                List<TestResultReport> reports = await _coaService.GetProductTestResultReportsWithBatchRange(SelectedProduct, FromBatch, ToBatch);

                PopulateTestResultReports(reports);
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Network error: Unable to retrieve test results. Please check your internet connection.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("An unexpected error occurred. Please try again or contact support.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }

        }

        private bool InputProductBatchValidation()
        {
            bool fromBatchValidation;
            bool selectedProductValidation;
            bool toBatchValidation;

            fromBatchValidation = InputValidationUtility.ValidateNotNullInput(FromBatch, "From Batch") && InputValidationUtility.ValidateBatchNumberFormat(FromBatch);
            selectedProductValidation = InputValidationUtility.ValidateNotNullObject(SelectedProduct, "Product");

            if (!string.IsNullOrWhiteSpace(ToBatch))
            {
                toBatchValidation = InputValidationUtility.ValidateBatchNumberFormat(ToBatch);
            }
            else
            {
                toBatchValidation = true;
            }

            return fromBatchValidation && selectedProductValidation && toBatchValidation;
        }

        private void PopulateTestResultReports(List<TestResultReport> reports)
        {
            if (reports.Count == 0)
            {
                MessageBox.Show("0 reports found", "Data not found!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Set threshold values from the first report (assumes all reports belong to the same product)
            TorqueWarning = reports[0].batchTestResult.testResult.product.torqueWarning;
            TorqueFail = reports[0].batchTestResult.testResult.product.torqueFail;
            FusionWarning = reports[0].batchTestResult.testResult.product.fusionWarning;
            FusionFail = reports[0].batchTestResult.testResult.product.fusionFail;

            // Sort and update the collection
            var sortedReports = _coaService.SortTestReportsByBatchNumber(reports);

            TestResultReports.Clear();
            foreach (var item in sortedReports)
            {
                TestResultReports.Add(item);
            }
        }

        public void PopupMakeCOAView()
        {
            if (TestResultReports.Count == 0)
            {
                NotificationUtility.ShowWarning("No test results found. Please search for test results first.");
                return;
            }

            var resultReports = TestResultReports.Where(report => report.result == true).ToList();

            _dialogService.ShowMakeCoaView(resultReports, PONumber);
        }

        public void RemoveTestReport(TestResultReport testResultReport)
        {
            for (int i = 0; i < TestResultReports.Count; i++)
            {
                if (testResultReport == TestResultReports[i])
                {
                    TestResultReports.RemoveAt(i);
                }
            }
        }
    }
}
