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
using ChemsonLabApp.MVVM.ViewModels.NewDataLoaderVM.Command;
using ChemsonLabApp.MVVM.Views.DataLoader;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Services.DataLoaderService;
using ChemsonLabApp.Services.DialogService;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.Extensions.DependencyInjection;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.ViewModels.NewDataLoaderVM
{
    [AddINotifyPropertyChangedInterface]
    public class SearchDataLoaderViewModel
    {
        private readonly IProductService _productService;
        private readonly ISearchDataLoaderService _searchDataLoaderService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<BatchTestResult> BatchTestResults { get; set; } = new ObservableCollection<BatchTestResult>();
        public List<Product> Products { get; set; } = new List<Product>();
        public Product SelectedProduct { get; set; }
        public string FromBatchNumber { get; set; }
        public string ToBatchNumber { get; set; }
        public DateTime TestDate { get; set; } = DateTime.Now;
        public bool SelectedAllBatchTestResult { get; set; } = false;

        // Suffix for batch number
        public bool Default { get; set; } = true;
        public bool RS { get; set; } = false;
        public bool RRS { get; set; } = false;
        public bool RT { get; set; } = false;
        public bool Remix { get; set; } = false;
        public bool Min2 { get; set; } = false;
        public bool Min4 { get; set; } = false;
        public bool Cal { get; set; } = false;

        // Commands
        public SearchBatchTestResultCommand SearchBatchTestResultCommand { get; set; }
        public ShowDataLoaderCommand ShowDataLoaderCommand { get; set; }
        public SelectAllBatchTestResultCommand SelectAllBatchTestResultCommand { get; set; }
        public UnSelectAllBatchTestResultCommand UnSelectAllBatchTestResultCommand { get; set; }
        public ShowDeleteBatchTestResultsCommand ShowDeleteBatchTestResultsCommand { get; set; }

        public SearchDataLoaderViewModel(
            IProductService productService,
            ISearchDataLoaderService searchDataLoaderService,
            IDialogService dialogService
            )
        {
            // services
            this._productService = productService;
            this._searchDataLoaderService = searchDataLoaderService;
            this._dialogService = dialogService;

            // commands
            SearchBatchTestResultCommand = new SearchBatchTestResultCommand(this);
            ShowDataLoaderCommand = new ShowDataLoaderCommand(this);
            SelectAllBatchTestResultCommand = new SelectAllBatchTestResultCommand(this);
            UnSelectAllBatchTestResultCommand = new UnSelectAllBatchTestResultCommand(this);
            ShowDeleteBatchTestResultsCommand = new ShowDeleteBatchTestResultsCommand(this);

            // initialize
            InitializeParameter();
        }

        public async void InitializeParameter()
        {
            CursorUtility.DisplayCursor(true);

            try
            {
                Products = await _productService.LoadActiveProducts();
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

        public async void SearchBatchTestResult()
        {
            BatchTestResults.Clear();

            if (!InputValidationUtility.ValidateNotNullObject(SelectedProduct, "Product")) return;
            if (!InputValidationUtility.ValidateNotNullInput(FromBatchNumber, "Batch Number") || !InputValidationUtility.ValidateBatchNumberFormat(FromBatchNumber)) return;


            CursorUtility.DisplayCursor(false);
            try
            {
                var productName = SelectedProduct.name;

                string testNumber = Default ? "1" :
                                    RS ? "2" :
                                    RRS ? "3" : "";

                string suffix = RS ? "RS" :
                                RRS ? "RRS" :
                                RT ? "RT" :
                                Remix ? "Remix" :
                                Min2 ? "2.00min" :
                                Min4 ? "4.00min" :
                                Cal ? "Cal" : null;

                var bchBatchTestResults = await _searchDataLoaderService.LoadBCHBatchTestResult(productName, testNumber, suffix, FromBatchNumber, ToBatchNumber, TestDate);

                if (bchBatchTestResults.Count() == 0)
                {
                    NotificationUtility.ShowWarning("No data found. Please try again.");
                    CursorUtility.DisplayCursor(false);
                    return;
                }

                var warmUpBatchTestResults = await _searchDataLoaderService.LoadWarmUpAndSTDBatchTestResult(bchBatchTestResults, "W/U");
                var stdBatchTestRestResults = await _searchDataLoaderService.LoadWarmUpAndSTDBatchTestResult(bchBatchTestResults, "STD");

                List<BatchTestResult> allBatchTestResults = new List<BatchTestResult>();
                allBatchTestResults.AddRange(bchBatchTestResults);
                allBatchTestResults.AddRange(warmUpBatchTestResults);
                allBatchTestResults.AddRange(stdBatchTestRestResults);

                PopulateBatchTestResults(allBatchTestResults);

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

        private void PopulateBatchTestResults(List<BatchTestResult> batchTestResults)
        {
            // Populate each unique item in list to BatchTestResults
            foreach (var batchTestResult in batchTestResults)
            {
                if (!BatchTestResults.Any(x => x.id == batchTestResult.id))
                {
                    BatchTestResults.Add(batchTestResult);
                }
            }
        }

        public void PopupEditDataLoaderView(BatchTestResult batchTestResult)
        {
            _dialogService.ShowEditDataLoaderView(batchTestResult);
        }

        public void PopupDeleteDataLoaderView()
        {
            var batchTestResults = BatchTestResults.Where(x => x.isSelected).ToList();

            if (batchTestResults.Count() == 0)
            {
                NotificationUtility.ShowWarning("Please select at least one item to delete.");
                return;
            }

            _dialogService.ShowDeleteDataLoaderView(batchTestResults);
        }

        public void SelectAllBatchTestResult()
        {
            SelectedAllBatchTestResult = true;
            foreach (var batch in BatchTestResults)
            {
                batch.isSelected = true;
            }
        }

        public void UnSelectAllBatchTestResult()
        {
            SelectedAllBatchTestResult = false;
            foreach (var batch in BatchTestResults)
            {
                batch.isSelected = false;
            }
        }
    }
}
