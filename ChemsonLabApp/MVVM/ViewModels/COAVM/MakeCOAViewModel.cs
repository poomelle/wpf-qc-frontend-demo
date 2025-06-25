using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.COAVM.Commands;
using PropertyChanged;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using SharpVectors.Dom;
using System.IO;
using System.Xml;
using System.Windows.Markup;
using PdfSharp;
using System.Linq;
using Microsoft.Office.Interop.Outlook;
using Application = Microsoft.Office.Interop.Outlook.Application;
using System.Threading.Tasks;
using ChemsonLabApp.RestAPI;
using System.Net.Http;
using ChemsonLabApp.Utilities;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Services.EmailServices.IEmailService;
using ChemsonLabApp.Services.COAService;

namespace ChemsonLabApp.MVVM.ViewModels.COAVM
{
    [AddINotifyPropertyChangedInterface]
    public class MakeCOAViewModel
    {
        public List<TestResultReport> TestResultReports { get; set; }
        public List<CustomerOrder> CustomerOrders { get; set; } = new List<CustomerOrder>();
        public CustomerOrder SelectedCustomerOrder { get; set; }
        public Customer Customer { get; set; }
        public bool CombineCOA { get; set; } = true;
        public bool SeparateCOA { get; set; } = false;
        public int biggerFontSize { get; set; } = 22;
        public int bigFontSize { get; set; } = 20;
        public int fontSize { get; set; } = 14;
        public int smallFontSize { get; set; } = 14;
        public int TotalTestReports { get; set; }
        public int SelectedTestReport { get; set; } = 1;
        public string PONumber { get; set; }
        public List<string> BatchesName { get; set; } = new List<string>();
        public List<string> CoAFilePaths { get; set; } = new List<string>();
        public FlowDocument COAReport { get; set; }
        public List<FlowDocument> COAReports { get; set; } = new List<FlowDocument>();
        public Grid COAReportGrid { get; set; } = new Grid();
        public List<Grid> COAReportDocs { get; set; } = new List<Grid>();

        // Local variables
        private Random random = new Random();
        private readonly IEmailService _emailService;
        private readonly IMakeCoaService _makeCoaService;

        // Commands
        public ReGenerateCOACommand ReGenerateCOACommand { get; set; }
        public NextCOACommand NextCOACommand { get; set; }
        public PreviousCOACommand PreviousCOACommand { get; set; }
        public SaveAndSendCOACommand SaveAndSendCOACommand { get; set; }

        public MakeCOAViewModel(IEmailService emailService, IMakeCoaService makeCoaService)
        {
            //commands
            ReGenerateCOACommand = new ReGenerateCOACommand(this);
            NextCOACommand = new NextCOACommand(this);
            PreviousCOACommand = new PreviousCOACommand(this);
            SaveAndSendCOACommand = new SaveAndSendCOACommand(this);

            // services
            this._emailService = emailService;
            this._makeCoaService = makeCoaService;
        }

        /// <summary>
        /// Initializes the parameters for the COA view model.
        /// Loads customer orders based on the product name from the first test result report.
        /// Sets the selected customer order to the first in the list.
        /// Handles network and general exceptions with user notifications and logging.
        /// </summary>
        public async void InitializeParameter()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                if (TestResultReports.Count > 0)
                {
                    string filter = $"?productName={TestResultReports.FirstOrDefault().batchTestResult.testResult.product.name}";
                    CustomerOrders = await _makeCoaService.GetAllCustomerOrdersAsync(filter);
                    SelectedCustomerOrder = CustomerOrders.FirstOrDefault();
                }
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Network error: Unable to retrieve test results. Please check your internet connection.");
                LoggerUtility.LogError(ex);
            }
            catch (System.Exception ex)
            {
                NotificationUtility.ShowError("An unexpected error occurred. Please try again or contact support.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        /// <summary>
        /// Generates and displays the COA (Certificate of Analysis) report(s) in the UI.
        /// Clears previous reports, creates new report grids based on the CombineCOA setting,
        /// wraps each grid in a FlowDocument, and updates the current report and total count.
        /// </summary>
        public void ShowingCOAReport()
        {
            COAReports.Clear();
            COAReportDocs.Clear();
            CoAFilePaths.Clear();

            int numberOfBatchPerPage = CombineCOA ? 20 : 1;
            List<Grid> combineCOAGrids = CreateCOAPageGrid(numberOfBatchPerPage);
            AddCOAGridsToCOAReportDocs(combineCOAGrids);

            foreach (var grid in COAReportDocs)
            {
                var flowDoc = new FlowDocument();
                flowDoc.Blocks.Add(new BlockUIContainer(grid));
                COAReports.Add(flowDoc);
            }

            COAReport = COAReports[0];
            TotalTestReports = COAReports.Count;
        }

        /// <summary>
        /// Adds the provided list of COA grids to the COAReportDocs collection if the list is not empty.
        /// </summary>
        private void AddCOAGridsToCOAReportDocs(List<Grid> coaGrids)
        {
            if (coaGrids.Count > 0)
            {
                COAReportDocs.AddRange(coaGrids);
            }
        }

        /// <summary>
        /// Creates a list of COA (Certificate of Analysis) page grids, each containing up to the specified number of batch test results per page.
        /// Splits the TestResultReports into pages, generates a grid for each page with headers and data, and returns the list of grids.
        /// </summary>
        /// <param name="numberOfBatchPerPage">The maximum number of batch test results to include on each COA page.</param>
        /// <returns>A list of Grid objects, each representing a COA page.</returns>
        private List<Grid> CreateCOAPageGrid(int numberOfBatchPerPage)
        {
            // Create COA report for every 20 batches in TestResultReports
            List<Grid> combineCOAGrids = new List<Grid>();
            int totalBatches = TestResultReports.Count;

            // Calculate total number of COA pages and round up to the nearest whole number
            int totalCOAPages = (int)Math.Ceiling((double)totalBatches / numberOfBatchPerPage);

            // Create COA report for every 20 batches and add to combineCOAGrids
            for (int page = 0; page < totalCOAPages; page++)
            {
                List<TestResultReport> testResultReports = TestResultReports.Skip(page * numberOfBatchPerPage).Take(numberOfBatchPerPage).ToList();

                Grid combineCOAGrid = new Grid();

                CreateCOAHeaderAndLayout(combineCOAGrid);

                PopulateCombineTestResultDataPage(combineCOAGrid, testResultReports);

                combineCOAGrids.Add(combineCOAGrid);
            }

            return combineCOAGrids;
        }

        /// <summary>
        /// Creates the header and layout for the COA (Certificate of Analysis) grid.
        /// This includes setting up the grid structure, adding the company logo and address,
        /// displaying the month of test, specification details, product name, and the specification header row.
        /// </summary>
        private void CreateCOAHeaderAndLayout(Grid combineCOAGrid)
        {
            CreateGridLayout(combineCOAGrid);
            CreateLogoAddressHeader(combineCOAGrid);
            CreateMonthOfTest(combineCOAGrid);
            CreateSpecification(combineCOAGrid);
            CreateProductName(combineCOAGrid);
            CreateSpecificationHeader(combineCOAGrid);
        }

        /// <summary>
        /// Adds 5 rows and 5 columns to the provided grid to set up the header layout
        /// for the COA (Certificate of Analysis) report.
        /// </summary>
        private void CreateGridLayout(Grid combineCOAGrid)
        {
            // Adding 5 rows and 5 columns for header
            int totalRows = 5;
            int totalColumns = 5;

            for (int i = 0; i < totalRows; i++)
            {
                combineCOAGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            }

            for (int i = 0; i < totalColumns; i++)
            {
                combineCOAGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }
        }

        /// <summary>
        /// Populates the COA grid with test result data for a single page.
        /// Iterates through the provided test result reports, adds each to the grid,
        /// and then decorates the grid with borders.
        /// </summary>
        private void PopulateCombineTestResultDataPage(Grid combineCOAGrid, List<TestResultReport> testResultReports)
        {
            // iterate testResultReports and populate the grid from index
            for (int i = 0; i < testResultReports.Count; i++)
            {
                PopulateEachTestResultReport(combineCOAGrid, testResultReports[i], i);
            }

            int totalRows = testResultReports.Count + 5;
            int totalColumns = 5;

            DecorateBorder(combineCOAGrid, totalRows, totalColumns);
        }

        /// <summary>
        /// Adds borders to the specified COA grid to visually separate the header and data rows/columns.
        /// Draws upper and lower borders for the header and last row, and left/right borders for each data row.
        /// </summary>
        private void DecorateBorder(Grid combineCOAGrid, int totalRows, int totalColumns)
        {
            // add upper and lower border
            for (int i = 0; i < totalColumns; i++)
            {
                Border upperBorder = new Border
                {
                    BorderThickness = new Thickness(0, 1, 0, 0),
                    BorderBrush = Brushes.Black,
                };
                Grid.SetRow(upperBorder, 2);
                Grid.SetColumn(upperBorder, i);
                combineCOAGrid.Children.Add(upperBorder);

                Border lowerBoarder = new Border
                {
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    BorderBrush = Brushes.Black,
                };
                Grid.SetRow(lowerBoarder, totalRows - 1);
                Grid.SetColumn(lowerBoarder, i);
                combineCOAGrid.Children.Add(lowerBoarder);
            }

            //add left and right border
            for (int i = 2; i < totalRows; i++)
            {
                Border leftBorder = new Border
                {
                    BorderThickness = new Thickness(1, 0, 0, 0),
                    BorderBrush = Brushes.Black,
                };
                Grid.SetRow(leftBorder, i);
                Grid.SetColumn(leftBorder, 0);
                combineCOAGrid.Children.Add(leftBorder);

                Border rightBorder = new Border
                {
                    BorderThickness = new Thickness(0, 0, 1, 0),
                    BorderBrush = Brushes.Black,
                };
                Grid.SetRow(rightBorder, i);
                Grid.SetColumn(rightBorder, totalColumns - 1);
                combineCOAGrid.Children.Add(rightBorder);
            }
        }

        /// <summary>
        /// Populates a single row in the COA grid with the batch number, torque deviation, and fusion time deviation
        /// for the provided test result report. If there are duplicate test results (by file name), and this is not
        /// the first occurrence, the torque and fusion deviations are randomly altered within the allowed limits.
        /// The method then creates and adds the corresponding UI elements to the grid.
        /// </summary>
        private void PopulateEachTestResultReport(Grid combineCOAGrid, TestResultReport testResultReport, int order = 0)
        {
            int startRow = 5 + order;
            int startColumn = 2;

            string batchNumber = testResultReport.batchTestResult.batch.batchName;
            double torqueDiff = (double)testResultReport.torqueDiff;
            double fusionDiff = (double)testResultReport.fusionDiff;

            // find if there any same test result in the list by comparing fileName
            var sameTestResult = TestResultReports.Where(t => t.batchTestResult.testResult.fileName == testResultReport.batchTestResult.testResult.fileName).ToList();

            // index of the current test result in the same test result list
            int index = sameTestResult.IndexOf(testResultReport);

            if (sameTestResult.Count > 1 && index != 0)
            {
                var upperLimitTorque = sameTestResult[0].batchTestResult.testResult.product.torqueFail;
                var lowerLimitTorque = -sameTestResult[0].batchTestResult.testResult.product.torqueFail;

                var upperLimitFusion = sameTestResult[0].batchTestResult.testResult.product.fusionFail;
                var lowerLimitFusion = -sameTestResult[0].batchTestResult.testResult.product.fusionFail;

                if (upperLimitTorque != null && lowerLimitTorque != null && upperLimitFusion != null && lowerLimitFusion != null)
                {
                    torqueDiff = CalculateRandomAlterValue(torqueDiff, (double)upperLimitTorque, (double)lowerLimitTorque);
                    fusionDiff = CalculateRandomAlterValue(fusionDiff, (double)upperLimitFusion, (double)lowerLimitFusion);
                }
            }

            // Decorate the value
            string torqueDeviation = torqueDiff > 0 ? $"+{torqueDiff.ToString("F1")}%" : $"{torqueDiff.ToString("F1")}%";
            string fusionTimeDeviation = fusionDiff > 0 ? $"+{fusionDiff.ToString("F1")}%" : $"{fusionDiff.ToString("F1")}%";

            // Add a new row to grid
            combineCOAGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            var batchNumberContent = CreateContent(batchNumber, 1);
            AddCellToGrid(combineCOAGrid, batchNumberContent, startColumn++, startRow);

            var torqueDevContent = CreateContent(torqueDeviation, 1);
            AddCellToGrid(combineCOAGrid, torqueDevContent, startColumn++, startRow);

            var fusionDevContent = CreateContent(fusionTimeDeviation, 1);
            AddCellToGrid(combineCOAGrid, fusionDevContent, startColumn++, startRow);
        }

        /// <summary>
        /// Alters the given value by a random amount within the specified upper and lower limits.
        /// The method repeatedly generates a random alteration between -1.0 and +0.9 (in 0.1 increments),
        /// adds it to the original value, and checks if the new value is within the provided limits.
        /// If not, it repeats the process until a valid value is found.
        /// </summary>
        /// <param name="value">The original value to alter.</param>
        /// <param name="upperLimit">The maximum allowed value after alteration.</param>
        /// <param name="lowerLimit">The minimum allowed value after alteration.</param>
        /// <returns>A new value within the specified limits, randomly altered from the original.</returns>
        private double CalculateRandomAlterValue(double value, double upperLimit, double lowerLimit)
        {
            double newValue;
            do
            {
                int randomAlterValue = random.Next(-10, 10);
                double alterValue = (double)randomAlterValue / 10;
                newValue = value;
                newValue += alterValue;

            } while (newValue > upperLimit || newValue < lowerLimit);

            return newValue;
        }

        /// <summary>
        /// Advances to the next individual COA (Certificate of Analysis) report in the list, 
        /// if not already at the last report. Updates the SelectedTestReport index and sets the current COAReport.
        /// </summary>
        public void NextIndividualCOA()
        {
            if (SelectedTestReport < COAReports.Count)
            {
                SelectedTestReport++;
                COAReport = COAReports[SelectedTestReport - 1];
            }
        }

        /// <summary>
        /// Moves to the previous individual COA (Certificate of Analysis) report in the list,
        /// if not already at the first report. Updates the SelectedTestReport index and sets the current COAReport.
        /// </summary>
        public void PreviousIndividualCOA()
        {
            if (SelectedTestReport > 1)
            {
                SelectedTestReport--;
                COAReport = COAReports[SelectedTestReport - 1];
            }
        }

        /// <summary>
        /// Adds a UIElement to the specified Grid at the given column and row, with optional column and row spans.
        /// </summary>
        /// <param name="grid">The Grid to add the element to.</param>
        /// <param name="element">The UIElement to add.</param>
        /// <param name="column">The column index.</param>
        /// <param name="row">The row index.</param>
        /// <param name="columnSpan">The number of columns the element should span. Default is 1.</param>
        /// <param name="rowSpan">The number of rows the element should span. Default is 1.</param>
        private void AddCellToGrid(Grid grid, UIElement element, int column, int row, int columnSpan = 1, int rowSpan = 1)
        {
            Grid.SetColumn(element, column);
            Grid.SetRow(element, row);
            Grid.SetColumnSpan(element, columnSpan);
            Grid.SetRowSpan(element, rowSpan);
            grid.Children.Add(element);
        }

        /// <summary>
        /// Creates a TextBox UI element with the specified text and border thickness.
        /// The TextBox is styled for use in COA report grids, with centered content,
        /// Arial font, and stretch alignment. Used for displaying cell content in the COA.
        /// </summary>
        private TextBox CreateContent(string text, double borderThickness = 0)
        {
            return new TextBox
            {
                Text = text ?? "",
                Padding = new Thickness(5),
                BorderThickness = new Thickness(borderThickness),
                BorderBrush = Brushes.Black,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                FontSize = smallFontSize,
                FontFamily = new FontFamily("Arial")
            };
        }

        /// <summary>
        /// Adds the company logo, address, and "Certification of Analysis" header to the specified grid.
        /// The logo is placed in the top-left, the address spans the next row, and the header is styled and centered.
        /// </summary>
        private void CreateLogoAddressHeader(Grid grid)
        {
            // Adding Company Logo Header
            double scaleFactor = 1.0 / 1.2;
            // Adding Company Logo Header
            System.Windows.Controls.Image companyLogo = new System.Windows.Controls.Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/logo_transparent.png", UriKind.Absolute)),
                RenderTransform = new ScaleTransform(scaleFactor, scaleFactor),
                Margin = new Thickness(10)
            };
            AddCellToGrid(grid, companyLogo, 0, 0, 3);

            // Add company Address
            var companyAddress = CreateContent(Constants.Constants.CompanyAddress);
            companyAddress.FontSize = smallFontSize;
            companyAddress.HorizontalContentAlignment = HorizontalAlignment.Left;
            AddCellToGrid(grid, companyAddress, 0, 1, 5);

            // Add "Certification of Analysis" word
            var coa = CreateContent("Certification of Analysis", 1);
            coa.FontSize = bigFontSize;
            coa.FontWeight = FontWeights.Bold;
            coa.Background = new SolidColorBrush(Color.FromArgb(255, 25, 77, 145));
            coa.Foreground = Brushes.White;
            AddCellToGrid(grid, coa, 0, 2, 5);
        }

        /// <summary>
        /// Adds the month and year of the test (or PO number if available) to the specified grid.
        /// Determines the month and year from the batch group name of the first test result report.
        /// If a PO number is provided, it displays the PO number instead.
        /// Styles the cell with a larger, bold font and a gray background.
        /// </summary>
        private void CreateMonthOfTest(Grid grid)
        {
            //COAReportGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            string batchGroupName = TestResultReports[0].batchTestResult.testResult.batchGroup.Substring(0, 3);
            //string month = Utility.TestMonth(batchGroupName);
            string month = TestMonthUtility.TestMonth(batchGroupName);
            string year = $"20{batchGroupName.Substring(0, 2)}";

            var monthYear = !string.IsNullOrWhiteSpace(PONumber) ? CreateContent($"PO: {PONumber}", 1) : CreateContent($"{month}\n{year}", 1);
            monthYear.FontSize = biggerFontSize;
            monthYear.FontWeight = FontWeights.Bold;
            monthYear.Background = new SolidColorBrush(Color.FromRgb(191, 191, 191));
            AddCellToGrid(grid, monthYear, 0, 3, 2);
        }

        /// <summary>
        /// Adds the specification section to the COA grid.
        /// This includes the test method, torque and fusion specifications, and sample weight.
        /// The specification is displayed in a styled cell spanning three columns.
        /// </summary>
        private void CreateSpecification(Grid grid)
        {
            string testMethod = TestResultReports[0].batchTestResult.testResult.testMethod;
            string torqueSpec = $"+/- {TestResultReports[0].batchTestResult.testResult.product.torqueFail}%";
            string fusionSpec = $"+/- {TestResultReports[0].batchTestResult.testResult.product.fusionFail}%";
            string sampleWeight = $"{TestResultReports[0].batchTestResult.testResult.product.sampleAmount}g";

            string content =
                $"Test method: {testMethod} deviation% vs. standard\n" +
                $"torque specification: {torqueSpec}\n" +
                $"fusion specification: {fusionSpec}\n" +
                $"QC results based on {sampleWeight} of sample";

            var specification = CreateContent(content, 1);
            specification.FontSize = fontSize;
            specification.Background = new SolidColorBrush(Color.FromRgb(191, 191, 191));
            AddCellToGrid(grid, specification, 2, 3, 3);
        }

        /// <summary>
        /// Adds the product name to the specified grid as a styled cell.
        /// The product name is retrieved from the first TestResultReport and displayed
        /// with a bold font, left alignment, and a blue background.
        /// </summary>
        private void CreateProductName(Grid grid)
        {
            string productName = TestResultReports[0].batchTestResult.testResult.product.name;
            var content = CreateContent(productName, 1);
            content.FontSize = biggerFontSize;
            content.FontWeight = FontWeights.Bold;
            content.HorizontalContentAlignment = HorizontalAlignment.Left;
            content.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6AAEDF"));
            AddCellToGrid(grid, content, 0, 4, 2);
        }

        /// <summary>
        /// Adds the specification header row to the COA grid.
        /// This row contains column headers for batch numbers, torque deviation, and fusion time deviation.
        /// Each header cell is styled with bold font and a gray background.
        /// </summary>
        private void CreateSpecificationHeader(Grid grid)
        {
            List<string> headers = new List<string>
            {
                "batch nos.",
                "torque\ndeviation",
                "fusion\ntime deviation"
            };

            int startColumn = 2;
            foreach (string header in headers)
            {
                var content = CreateContent(header, 1);
                content.FontSize = fontSize;
                content.FontWeight = FontWeights.Bold;
                content.Background = new SolidColorBrush(Color.FromRgb(191, 191, 191));
                AddCellToGrid(grid, content, startColumn++, 4);
            }
        }

        /// <summary>
        /// Saves the generated COA PDF file, stores the COA data in the database, and sends the COA via email to the selected customer.
        /// Displays appropriate notifications and handles errors and cursor state.
        /// </summary>
        public async Task SaveAndSendCOA()
        {
            if (SelectedCustomerOrder == null)
            {
                NotificationUtility.ShowWarning("Please select a customer order to send the COA.");
                return;
            }

            CursorUtility.DisplayCursor(true);

            try
            {
                CreateCOAPDFFile();

                await SaveCoaToDatabase();

                await SendCoAEmail();
            }
            catch (System.Exception e)
            {
                NotificationUtility.ShowError("An unexpected error occurred. Please try again or contact support.");
                LoggerUtility.LogError(e);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        /// <summary>
        /// Saves the COA data to the database by calling the service method with the current list of TestResultReports.
        /// </summary>
        private async Task SaveCoaToDatabase()
        {
            await _makeCoaService.CreateCOAFromTestResultReportAsync(TestResultReports);
        }

        /// <summary>
        /// Creates the COA PDF file(s) from the current COA report grids, saves them to disk, and updates the file paths list.
        /// Displays a success notification if successful, or an error notification if an exception occurs.
        /// </summary>
        private void CreateCOAPDFFile()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                string filePath = Constants.Constants.COAFilePath;
                string productName = TestResultReports[0].batchTestResult.testResult.product.name;

                CoAFilePaths = PdfUtility.CreateCOAPdfFromGrids(COAReportDocs, productName, CombineCOA, filePath);

                NotificationUtility.ShowSuccess("PDF files have been saved.");
            }
            catch (System.Exception ex)
            {
                NotificationUtility.ShowError("Failed to create COA PDF file. Please try again or contact support.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        /// <summary>
        /// Sends the generated COA (Certificate of Analysis) PDF file(s) as email attachments
        /// to the selected customer's email address using the configured email service.
        /// The sender, recipient, subject, and body are set, and the COA file paths are attached.
        /// </summary>
        private async Task SendCoAEmail()
        {
            var sender = Constants.Constants.FromAddress;
            var recipient = SelectedCustomerOrder.customer.email;
            var subject = "Certificate of Analysis";
            var body = "Please find the attached Certificate of Analysis for your order.";
            var attachments = CoAFilePaths;

            await _emailService.SendEmailAsync(sender, recipient, subject, body, attachments);
        }
    }
}
