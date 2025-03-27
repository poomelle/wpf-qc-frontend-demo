using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using ChemsonLabApp.MVVM.ViewModels.FormulationPdfVM.Command;
using System.IO;
using OfficeOpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Office.Interop.Excel;
using System.Windows.Input;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;


namespace ChemsonLabApp.MVVM.ViewModels.FormulationPdfVM
{
    [AddINotifyPropertyChangedInterface]
    public class FormulationPdfViewModel
    {
        // Public properties
        public ObservableCollection<FormulationPdf> FormulationPdfs { get; set; } = new ObservableCollection<FormulationPdf>();
        public List<string> Products { get; set; } = new List<string>();
        public string ExcelFolderLocation { get; set; }
        public bool IsQcPdfCheckedAll { get; set; }
        public bool IsDpcPdfCheckedAll { get; set; }

        // Commands
        public SearchExcelFileNameCommand SearchExcelFileNameCommand { get; set; }
        public SavePDFCommand SavePDFCommand { get; set; }
        public TriggerQcPdfCommand TriggerQcPdfCommand { get; set; }
        public TriggerDpcPdfCommand TriggerDpcPdfCommand { get; set; }
        public TriggerQcPdfAllCommand TriggerQcPdfAllCommand { get; set; }
        public TriggerDpcPdfAllCommand TriggerDpcPdfAllCommand { get; set; }
        public BrowseExcelFileCommand BrowseExcelFileCommand { get; set; }
        public OpenExcelFileCommand OpenExcelFileCommand { get; set; }

        // Local variables
        List<string> allExcelFiles = new List<string>();
        List<string> clippingProductName = new List<string> { "CP", "GRX", "GKX"};
        string productWorksheetName = "MASTER 100% Fmln";
        string batchDateWorksheetName = "Batch Sheet";
        List<string> potentialProductNameLocations = new List<string> { "A7", "B1", "B7", "C1" };
        string batchLocation = "B1";
        string dateLocation = "E1";
        List<string> excludePotentialProductName = new List<string> { "RM No.", "Blend Formulation" };

        string qcWorksheetName = "QC";
        string dpcWorksheetName = "QC Std Components";
        string outputFolderLocation = "S:\\Lab\\Formulations\\PDF";
        private readonly IProductService _productService;

        public FormulationPdfViewModel(IProductService productService)
        {
            // Services
            this._productService = productService;

            // Commands
            SearchExcelFileNameCommand = new SearchExcelFileNameCommand(this);
            SavePDFCommand = new SavePDFCommand(this);
            TriggerQcPdfCommand = new TriggerQcPdfCommand(this);
            TriggerDpcPdfCommand = new TriggerDpcPdfCommand(this);
            TriggerQcPdfAllCommand = new TriggerQcPdfAllCommand(this);
            TriggerDpcPdfAllCommand = new TriggerDpcPdfAllCommand(this);
            BrowseExcelFileCommand = new BrowseExcelFileCommand(this);
            OpenExcelFileCommand = new OpenExcelFileCommand(this);

            // initial
            InitializeParameters();
        }

        public void BrowseExcelFile(FormulationPdf formulationPdf)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
                openFileDialog.Title = "Select an Excel File";
                openFileDialog.InitialDirectory = Directory.Exists(Constants.Constants.FormulationExcelPath) ? Constants.Constants.FormulationExcelPath : null;

                if (openFileDialog.ShowDialog() == true)
                {
                    var fullPath = openFileDialog.FileName;

                    // Create a formulationPdfExcel object
                    FormulationPdfExcel formulationPdfExcel = CreateFormulationPdfExcel(fullPath, productWorksheetName,
                                                                                        potentialProductNameLocations, batchDateWorksheetName,
                                                                                        batchLocation, dateLocation);
                    formulationPdf.excelFilePath = Path.GetFileName(fullPath);

                    formulationPdf.formulationBatch = formulationPdfExcel.batches ?? "";

                    formulationPdf.formulationDate = formulationPdfExcel.dates ?? "";
                }
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: unable to browse the file.");
                LoggerUtility.LogError(ex);
            }
        }

        public void SavePDF()
        {
            try
            {
                CursorUtility.DisplayCursor(true);

                if (FormulationPdfs != null)
                {
                    foreach (var formulationPdf in FormulationPdfs)
                    {
                        if (string.IsNullOrWhiteSpace(formulationPdf.excelFilePath))
                        {
                            continue;
                        }

                        List<string> printWorksheet = GetAllPrintWorksheet(formulationPdf);

                        if (printWorksheet != null && printWorksheet.Count > 0)
                        {
                            PrintOutPDF(formulationPdf.excelFilePath, outputFolderLocation, printWorksheet, formulationPdf.productName);
                        }

                    }

                    NotificationUtility.ShowSuccess("PDFs have been saved successfully.");
                }
                else
                {
                    NotificationUtility.ShowWarning("Formulation Pdfs are empty.");
                }

            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: unable to save PDFs.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        private List<string> GetAllPrintWorksheet(FormulationPdf formulationPdf)
        {
            List<string> printWorksheet = new List<string>();

            if (formulationPdf.isQcPdfChecked)
            {
                printWorksheet.Add(qcWorksheetName);
            }

            if (formulationPdf.isDpcPdfChecked)
            {
                printWorksheet.Add(dpcWorksheetName);
            }

            return printWorksheet;
        }

        private void PrintOutPDF(string fileName, string outputFolder ,List<string> printWorksheet, string productName)
        {
            var excelApp = new Application();
            excelApp.Visible = false;
            excelApp.DisplayAlerts = false;

            try
            {
                var fullFileName = Path.Combine(ExcelFolderLocation, fileName);

                if (!File.Exists(fullFileName))
                {
                    throw new Exception($"{fileName} not found.");
                }

                var workbook = excelApp.Workbooks.Open(fullFileName);

                foreach (var worksheetName in printWorksheet)
                {
                    var worksheet = workbook.Sheets[worksheetName] as Microsoft.Office.Interop.Excel.Worksheet;

                    if (worksheet != null)
                    {
                        worksheet.PageSetup.Orientation = XlPageOrientation.xlLandscape;
                        worksheet.PageSetup.FitToPagesWide = 1;
                        worksheet.PageSetup.FitToPagesTall = false;

                        string outputFileName = worksheetName == "QC" ? $"qc {productName}.pdf" : $"DPC {productName}.pdf";
                        string outputFilePath = Path.Combine(outputFolder, outputFileName);

                        // Export to PDF
                        worksheet.ExportAsFixedFormat(XlFixedFormatType.xlTypePDF, outputFilePath);
                    }
                    else
                    {
                        NotificationUtility.ShowWarning($"{worksheetName} not found in {fileName}");
                    }
                }

                workbook.Close(false);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: unable to print out PDF");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                excelApp.Quit();
            }

        }

        public void SearchExcelFileName()
        {
            try
            {
                CursorUtility.DisplayCursor(true);

                if (FormulationPdfs != null)
                {
                    allExcelFiles = GetAllExcelFilesInFolder(ExcelFolderLocation);

                    // iterate through each excel file and get the product name
                    foreach (var excelFile in allExcelFiles)
                    {
                        try
                        {
                            // Create a formulationPdfExcel object
                            FormulationPdfExcel formulationPdfExcel = CreateFormulationPdfExcel(excelFile, productWorksheetName, 
                                                                                                potentialProductNameLocations, batchDateWorksheetName, 
                                                                                                batchLocation, dateLocation);

                            // compare the product name with the list of products
                            if (formulationPdfExcel.potentialProductName != null)
                            {
                                foreach (var potentialProductName in formulationPdfExcel.potentialProductName)
                                {
                                    // iterate through each FormulationPdfs and compare the product name
                                    foreach (var formulationPdf in FormulationPdfs)
                                    {
                                        if (!string.IsNullOrWhiteSpace(formulationPdf.productName))
                                        {
                                            var compareProductName = ModifiedProductName(formulationPdf.productName);

                                            if (potentialProductName.Equals(compareProductName, StringComparison.OrdinalIgnoreCase))
                                            {
                                                formulationPdf.excelFilePath = Path.GetFileName(excelFile);

                                                formulationPdf.formulationBatch = formulationPdfExcel.batches;

                                                formulationPdf.formulationDate = formulationPdfExcel.dates;
                                            }
                                        }
                                    }
                                }
                            }

                            // check if all formulationPdfs have excel file path then break the loop
                            if (FormulationPdfs.All(f => !string.IsNullOrWhiteSpace(f.excelFilePath)))
                            {
                                NotificationUtility.ShowSuccess("Excel files searching process has done.");
                                break;
                            }

                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    NotificationUtility.ShowWarning("Formulation Pdfs are empty.");
                }
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        private FormulationPdfExcel CreateFormulationPdfExcel(string excelFile, string productWorksheetName, List<string> potentialProductNameLocation, string batchDateWorksheetName, string batchLocation, string dateLocation)
        {
            var formulationPdfExcel = new FormulationPdfExcel();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage excelPackage = new ExcelPackage(new FileInfo(excelFile)))
            {
                ExcelWorksheet productNameWorksheet = excelPackage.Workbook.Worksheets[productWorksheetName];
                if (productNameWorksheet == null)
                {
                    // Worksheet not found
                    return null;
                }

                List<string> potentialProductNames = new List<string>();

                foreach (var location in potentialProductNameLocation)
                {
                    try
                    {
                        // Check if the cell has a value before trying to access it
                        var cellValue = productNameWorksheet.Cells[location]?.Value;

                        // Add to list if not null and not exist in the list
                        if (cellValue != null)
                        {
                            var cellValueString = ModifiedProductName(cellValue.ToString());

                            if (!potentialProductNames.Contains(cellValueString) && !excludePotentialProductName.Contains(cellValueString))
                            {
                                potentialProductNames.Add(cellValueString);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        // Cell not found
                        continue;
                    }
                }
                formulationPdfExcel.potentialProductName = potentialProductNames;

                // Get the batch and date
                ExcelWorksheet batchDateWorksheet = excelPackage.Workbook.Worksheets[batchDateWorksheetName];
                if (batchDateWorksheet != null)
                {
                    formulationPdfExcel.batches = batchDateWorksheet.Cells[batchLocation]?.Value?.ToString() ?? "";
                    formulationPdfExcel.dates = DateTime.TryParse(batchDateWorksheet.Cells[dateLocation]?.Value?.ToString(), out DateTime parsedDate)
                                                ? parsedDate.ToString("dd/MM/yyyy")
                                                : "";
                }
            }

            return formulationPdfExcel;
        }

        public void OpenExcelFile(FormulationPdf formulationPdf)
        {
            if (!string.IsNullOrWhiteSpace(formulationPdf.excelFilePath))
            {
                var filePath = Path.Combine(ExcelFolderLocation, formulationPdf.excelFilePath);

                if (!File.Exists(filePath))
                {
                    NotificationUtility.ShowWarning($"{formulationPdf.excelFilePath} not found.");
                    return;
                }

                // Create a new Excel application instance
                Application excelApp = new Application();

                try
                {
                    CursorUtility.DisplayCursor(true);

                    // Open the workbook
                    Microsoft.Office.Interop.Excel.Workbook workbook = excelApp.Workbooks.Open(filePath);

                    // Make the Excel application visible to the user
                    excelApp.Visible = true;
                    excelApp.UserControl = true; // Allows the user to interact with the Excel window
                }
                catch (Exception ex)
                {
                    // Handle any errors that occur
                    NotificationUtility.ShowError("Error: unable to open excel file");
                    LoggerUtility.LogError(ex);
                }
                finally
                {
                    CursorUtility.DisplayCursor(false);
                }        
            }
        }

        private List<string> GetAllExcelFilesInFolder(string excelFolderLocation)
        {
            // Get all excel files in the folder
            List<string> allExcelList = new List<string>();
            try
            {
                if (Directory.Exists(excelFolderLocation))
                {
                    string[] files = Directory.GetFiles(excelFolderLocation, "*.xlsx", SearchOption.TopDirectoryOnly);

                    foreach (string file in files)
                    {
                        if (!string.IsNullOrWhiteSpace(file))
                        {
                            allExcelList.Add(file);
                        }
                    }
                }

                return allExcelList;
            }
            catch (Exception ex)
            {
                throw new Exception($"Excel folder location is not valid {ex.Message}");
            }
        }

        private string ModifiedProductName(string productName)
        {
            // exclude the words from the product name
            string modifiedProductName = productName;
            modifiedProductName = modifiedProductName.ToString().Trim().Replace("ORG ", "");

            foreach (var word in clippingProductName)
            {
                modifiedProductName = modifiedProductName.Replace(word, "");
            }

            return modifiedProductName.Trim();
        }

        public void TriggerQcPdfAll()
        {
            foreach(var formulationPdf in FormulationPdfs)
            {
                formulationPdf.isQcPdfChecked = IsQcPdfCheckedAll;
            }
        }

        public void TriggerDpcPdfAll()
        {
            foreach(var formulationPdf in FormulationPdfs)
            {
                formulationPdf.isDpcPdfChecked = IsDpcPdfCheckedAll;
            }
        }

        public void TriggerQcPdf(FormulationPdf formulationPdf)
        {
            formulationPdf.isQcPdfChecked = !formulationPdf.isQcPdfChecked;
        }

        public void TriggerDpcPdf(FormulationPdf formulationPdf)
        {
            formulationPdf.isDpcPdfChecked = !formulationPdf.isDpcPdfChecked;
        }

        private async void InitializeParameters()
        {
            await LoadProducts();
            GetExcelFolderLocation();
        }

        private void GetExcelFolderLocation()
        {
            ExcelFolderLocation = Constants.Constants.FormulationExcelPath;
        }

        private async Task LoadProducts()
        {
            var products = await _productService.LoadActiveProducts();

            if (products != null)
            {
                Products = products.Select(p => p.name).ToList();
            }
        }
    }
}
