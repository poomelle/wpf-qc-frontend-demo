using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.SpecificationVM.Commands;
using ChemsonLabApp.MVVM.Views.Specification;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Services.DialogService;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.ViewModels.SpecificationVM
{
    [AddINotifyPropertyChangedInterface]
    public class SpecificationViewModel
    {
        public ObservableCollection<Specification> Specifications { get; set; } = new ObservableCollection<Specification>();
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Instrument> Instruments { get; set; } = new List<Instrument>();
        public List<string> StatusComboBox { get; set; } = new List<string> { "All", "Active", "Inactive" };
        public Product SelectedProduct { get; set; }
        public Instrument SelectedInstrument { get; set; }
        public string SelectedStatus { get; set; }
        public bool IsImportingExcelData { get; set; }
        public bool IsShowingListBox { get; set; } = true;

        // Progress Bar Properties
        public int ProgressValue { get; set; }
        public int ProgressMaxValue { get; set; }
        public int ProgressMinValue { get; set; } = 0;
        public string ProgressMessage { get; set; }


        // local variables
        List<string> columnNames = new List<string>();

        // services
        private readonly ISpecificationService _specificationService;
        private readonly IProductService _productService;
        private readonly IInstrumentService _instrumentService;
        private readonly IDialogService _dialogService;


        // Commands Parameters
        public ShowAddNewSpecificationView ShowAddNewSpecificationView { get; set; }
        public ReloadDataCommand ReloadDataCommand { get; set; }
        public ShowSpecificationDetailCommand ShowSpecificationDetailCommand { get; set; }
        public ProductNameSearchCommand ProductNameSearchCommand { get; set; }
        public ShowPrintSpecificationView ShowPrintSpecificationView { get; set; }
        public ImportSpecificationExcelFileCommand ImportSpecificationExcelFileCommand { get; set; }
        public ProductSelectSpecificationCommand ProductSelectSpecificationCommand { get; set; }
        public InstrumentSelectSpecificationCommand InstrumentSelectSpecificationCommand { get; set; }
        public SelectStatusChangeSpecificationCommand SelectStatusChangeSpecificationCommand { get; set; }

        public SpecificationViewModel(
            ISpecificationService specificationService,
            IProductService productService,
            IInstrumentService instrumentService,
            IDialogService dialogService
            )
        {
            // services
            this._specificationService = specificationService;
            this._productService = productService;
            this._instrumentService = instrumentService;
            this._dialogService = dialogService;

            // Commands
            ShowAddNewSpecificationView = new ShowAddNewSpecificationView(this);
            ShowSpecificationDetailCommand = new ShowSpecificationDetailCommand(this);
            ReloadDataCommand = new ReloadDataCommand(this);
            ProductNameSearchCommand = new ProductNameSearchCommand(this);
            ShowPrintSpecificationView = new ShowPrintSpecificationView(this);
            ImportSpecificationExcelFileCommand = new ImportSpecificationExcelFileCommand(this);
            ProductSelectSpecificationCommand = new ProductSelectSpecificationCommand(this);
            InstrumentSelectSpecificationCommand = new InstrumentSelectSpecificationCommand(this);
            SelectStatusChangeSpecificationCommand = new SelectStatusChangeSpecificationCommand(this);

            // Initialize
            InitializeParameter();
        }

        /// <summary>
        /// Initializes parameters for the SpecificationViewModel, including loading all active specifications,
        /// products, and instruments. Handles exceptions and manages the cursor state during the operation.
        /// </summary>
        public async void InitializeParameter()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                var specifications = await _specificationService.GetAllActiveSpecificationsAsync();
                PopulateSpecifications(specifications);

                Products = await _productService.LoadActiveProducts();
                Instruments = await _instrumentService.GetAllActiveInstrument();
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Error: Failed to connect to the server. Please check your internet connection and try again.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: An error occurred while loading the data. Please try again.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        /// <summary>
        /// Opens the Add Specification dialog view using the dialog service.
        /// </summary>
        public void PopupAddSpecificationView()
        {
            _dialogService.ShowView("AddSpecification");
        }

        /// <summary>
        /// Opens the Edit Specification dialog view for the specified specification using the dialog service.
        /// </summary>
        /// <param name="specification">The specification to be edited.</param>
        public void PopupEditSpecificationView(Specification specification)
        {
            _dialogService.ShowEditSpecificationView(specification);
        }

        /// <summary>
        /// Opens the Print Specification dialog view using the dialog service.
        /// </summary>
        public void PopupPrintSpecificationView()
        {
            _dialogService.ShowView("PrintSpecification");
        }

        /// <summary>
        /// Resets the selected product, instrument, and status to their default values and reloads the specifications.
        /// </summary>
        public void ReloadSpecification()
        {
            SelectedProduct = null;
            SelectedInstrument = null;
            SelectedStatus = "Active";
            InitializeParameter();
        }

        /// <summary>
        /// Filters the specifications based on the selected product, instrument, and status.
        /// Constructs a filter string using the selected values and retrieves the filtered specifications
        /// from the specification service. Handles exceptions by displaying error notifications and logs errors.
        /// Manages the cursor state during the operation.
        /// </summary>
        public async void SpecificationFilter()
        {
            string productName = SelectedProduct == null ? "" : SelectedProduct.name;
            string instrumentName = SelectedInstrument == null ? "" : SelectedInstrument.name;
            string status = SelectedStatus == "Active" ? "true" : SelectedStatus == "Inactive" ? "false" : "";

            var specificationFilter = $"?productName={productName}&machineName={instrumentName}&inUse={status}";

            try
            {
                CursorUtility.DisplayCursor(true);
                var specifications = await _specificationService.GetAllSpecificationsAsync(specificationFilter);
                PopulateSpecifications(specifications);
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Error: Failed to connect to the server. Please check your internet connection and try again.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: An error occurred while loading the data. Please try again.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        /// <summary>
        /// Imports product specifications from an Excel file selected by the user.
        /// Displays a file dialog for Excel file selection, validates the file, reads and saves the specifications,
        /// and provides user feedback on success or failure. Manages the cursor state during the operation.
        /// </summary>
        public async void ImportSpecificationExcelFile()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                string filePath = ExcelUtility.GetExcelFilePath();

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    NotificationUtility.ShowError("Error: No file selected. Please select an excel file to import.");
                    return;
                }

                await ReadAndSaveSpecificationExcelFile(filePath);

            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: Failed to import the excel file. Please try again.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        /// <summary>
        /// Reads product specifications from the specified Excel file and saves them using the specification service.
        /// Displays a progress bar and status messages during the import process.
        /// Validates the Excel file and each row before processing.
        /// Handles exceptions by showing error notifications and logging errors.
        /// Updates UI state to indicate importing progress and completion.
        /// </summary>
        private async Task ReadAndSaveSpecificationExcelFile(string filePath)
        {
            try
            {
                IsImportingExcelData = true;
                IsShowingListBox = false;

                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RowsUsed();
                    var totalProductSpecifications = rows.Skip(1).Count();

                    if (!IsExcelFileValidated(totalProductSpecifications)) return;

                    ProgressMaxValue = totalProductSpecifications;
                    ProgressValue = 0;

                    // Get the column names
                    columnNames = rows.First().Cells().Select(cell => cell.Value.ToString()).ToList();

                    // Read the data from the excel file
                    foreach (var row in rows.Skip(1))
                    {
                        // verify if the row has data in it (especially product name in cell 1)
                        if (!IsRowDataVerified(row)) continue;

                        string productName = row.Cell(columnNames.IndexOf("Product") + 1).Value.ToString();

                        ProgressMessage = $"Processing.....{productName}";

                        //await CreateOrUpdateSpecificationFromExcel(row);
                        await _specificationService.CreateOrUpdateSpecificationFromExcel(row, columnNames, Instruments);

                        ProgressValue++;
                    }

                    NotificationUtility.ShowSuccess("Product specifications imported successfully.");

                }
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error reading the excel file. Please try again or check if the excel file is using.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                IsImportingExcelData = false;
                IsShowingListBox = true;
            }
        }

        /// <summary>
        /// Verifies if the given Excel row contains valid data for processing, specifically checking
        /// that the "Product" column is not empty or whitespace. Returns true if the row contains
        /// a valid product name; otherwise, returns false.
        /// </summary>
        private bool IsRowDataVerified(IXLRow row)
        {
            // Check if the row has data in it (especially product name in cell 1)
            bool hasProductData = !string.IsNullOrWhiteSpace(row.Cell(columnNames.IndexOf("Product") + 1).Value.ToString());

            return hasProductData;
        }

        /// <summary>
        /// Validates the Excel file by confirming with the user if they want to import the specified number of product specifications.
        /// Checks if the file contains any data and shows an error if not.
        /// Returns true if the user confirms and the file has data; otherwise, false.
        /// </summary>
        private bool IsExcelFileValidated(int totalProductSpecifications)
        {
            var isUserConfirmed = NotificationUtility.ShowConfirmation($"Do you want to import {totalProductSpecifications} product specifications?");

            bool hasDataInFile = totalProductSpecifications > 0;
            if (!hasDataInFile)
            {
                NotificationUtility.ShowError("No data found in the excel file.");
            }
            return isUserConfirmed && hasDataInFile;
        }

        /// <summary>
        /// Clears the current Specifications collection and populates it with the provided list of specifications.
        /// </summary>
        /// <param name="specifications">The list of specifications to add to the collection.</param>
        private void PopulateSpecifications(List<Specification> specifications)
        {
            Specifications.Clear();
            foreach (var specification in specifications)
            {
                Specifications.Add(specification);
            }
        }
    }
}
