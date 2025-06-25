using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using System.IO;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows;
using System.Xml;
using ChemsonLabApp.MVVM.ViewModels.SpecificationVM.Commands;
using ChemsonLabApp.Services.IService;

namespace ChemsonLabApp.MVVM.ViewModels.SpecificationVM
{
    [AddINotifyPropertyChangedInterface]
    public class PrintSpecificationContentViewModel
    {
        private readonly ISpecificationService _specificationService;
        private readonly IInstrumentService _instrumentService;

        public ObservableCollection<PrintOutSpecification> PrintSpecifications { get; set; } = new ObservableCollection<PrintOutSpecification>();
        public ObservableCollection<Specification> Specifications { get; set; } = new ObservableCollection<Specification>();
        public ObservableCollection<Instrument> Instruments { get; set; } = new ObservableCollection<Instrument>();
        public List<SolidColorBrush> BackgroundColours { get; set; } = new List<SolidColorBrush>
        {
            (SolidColorBrush)new BrushConverter().ConvertFromString("#ffdfba"),
            (SolidColorBrush)new BrushConverter().ConvertFromString("#bae1ff"),
            (SolidColorBrush)new BrushConverter().ConvertFromString("#ffffba"),
            (SolidColorBrush)new BrushConverter().ConvertFromString("#baffc9")
        };

        public SolidColorBrush DbDateBackgroundColour { get; set; } = (SolidColorBrush)new BrushConverter().ConvertFromString("#ffb3ba");
        public int fontSize { get; set; } = 18;
        public FlowDocument SpecificationTable1 { get; set; }
        public FlowDocument SpecificationTable2 { get; set; }
        public Grid TableGrid1 { get; set; }
        public Grid TableGrid2 { get; set; }
        public PrintSpecificationCommand PrintSpecificationCommand { get; set; }

        public PrintSpecificationContentViewModel(
            ISpecificationService specificationService,
            IInstrumentService instrumentService
            )
        {
            // services
            this._specificationService = specificationService;
            this._instrumentService = instrumentService;

            // commands
            PrintSpecificationCommand = new PrintSpecificationCommand(this);

            // initialize
            InitializeAsync();
        }

        /// <summary>
        /// Initializes the PrintSpecificationContentViewModel by asynchronously loading specifications and instruments,
        /// consolidating the specification data, and creating the display grids for printing.
        /// </summary>
        private async void InitializeAsync()
        {
            await GetAllSpecificationsAsync(filter: "?inUse=true", sort: "&sortBy=productName&isAscending=true");
            await GetAllInstrumentsAsync(filter: "?status=true");
            ConsolidateSpecification();
            CreateAndShowGrid();
        }

        /// <summary>
        /// Asynchronously retrieves all specifications from the specification service using the provided filter and sort parameters,
        /// clears the current Specifications collection, and populates it with the retrieved specifications.
        /// </summary>
        private async Task GetAllSpecificationsAsync(string filter = "", string sort = "")
        {
            var specifications = await _specificationService.GetAllSpecificationsAsync(filter, sort);
            Specifications.Clear();
            foreach (var specification in specifications)
            {
                Specifications.Add(specification);
            }

        }

        /// <summary>
        /// Asynchronously retrieves all instruments from the instrument service using the provided filter and sort parameters,
        /// clears the current Instruments collection, and populates it with the retrieved instruments.
        /// </summary>
        private async Task GetAllInstrumentsAsync(string filter = "", string sort = "")
        {
            var instruments = await _instrumentService.GetAllInstrumentsAsync(filter, sort);
            Instruments.Clear();
            foreach (var instrument in instruments)
            {
                Instruments.Add(instrument);
            }
        }

        /// <summary>
        /// Consolidates the list of specifications by grouping them based on product properties,
        /// and creates a collection of <see cref="PrintOutSpecification"/> objects.
        /// Each consolidated specification contains product details and a list of machine-specific specifications
        /// for all available instruments. The resulting collection is assigned to <see cref="PrintSpecifications"/>.
        /// </summary>
        public void ConsolidateSpecification()
        {
            var consolidatedSpecifications = Specifications
               .GroupBy(specification => new
               {
                   specification.product.id,
                   specification.product.name,
                   specification.product.status,
                   specification.product.sampleAmount,
                   specification.product.dbDate,
                   specification.product.comment,
                   specification.product.coa,
                   specification.product.colour,
                   specification.product.torqueWarning,
                   specification.product.torqueFail,
                   specification.product.fusionWarning,
                   specification.product.fusionFail,
               })
               .Select(group => new PrintOutSpecification
               {
                   ProductId = group.Key.id,
                   ProductName = group.Key.name,
                   ProductStatus = group.Key.status,
                   SampleAmount = group.Key.sampleAmount,
                   DbDate = group.Key.dbDate,
                   Comment = group.Key.comment,
                   Coa = group.Key.coa,
                   Colour = group.Key.colour,
                   TorqueFail = group.Key.torqueFail ?? default(double),
                   TorqueWarning = group.Key.torqueWarning ?? default(double),
                   FusionFail = group.Key.fusionFail ?? default(double),
                   FusionWarning = group.Key.fusionWarning ?? default(double),
                   MachineSpecs = Instruments.Select(instrument =>
                   {
                       var specForMachine = group.FirstOrDefault(spec => spec.machine.id == instrument.id);
                       return new PrintOutMachineSpecification
                       {
                           MachineId = instrument.id,
                           MachineName = instrument.name,
                           MachineStatus = instrument.status,
                           Temp = specForMachine?.temp.ToString() ?? "",
                           Load = specForMachine?.load.ToString() ?? "",
                           Rpm = specForMachine?.rpm.ToString() ?? "",
                       };
                   }).ToList()
               }).ToList();

            PrintSpecifications.Clear();
            foreach (var spec in consolidatedSpecifications)
            {
                PrintSpecifications.Add(spec);
            }
        }

        /// <summary>
        /// Creates a header label for use in the specification grid, with optional background color.
        /// The header label is styled with bold font, centered alignment, and a specified font size and family.
        /// </summary>
        /// <param name="text">The text to display in the header label.</param>
        /// <param name="colourBrush">An optional SolidColorBrush to use as the background color. If null, the background is transparent.</param>
        /// <returns>A configured Label control representing the header.</returns>
        private Label CreateHeader(string text, SolidColorBrush colourBrush = null)
        {
            return new Label
            {
                Content = text,
                Padding = new Thickness(5),
                BorderThickness = new Thickness(0.5),
                BorderBrush = Brushes.Black,
                Background = colourBrush ?? Brushes.Transparent,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontSize = fontSize,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Arial")
            };
        }

        /// <summary>
        /// Creates a content label for use in the specification grid, with optional background color.
        /// The content label is styled with bold font, centered alignment, and a specified font size and family.
        /// </summary>
        /// <param name="text">The text to display in the content label.</param>
        /// <param name="colourBrush">An optional SolidColorBrush to use as the background color. If null, the background is transparent.</param>
        /// <returns>A configured Label control representing the content cell.</returns>
        private Label CreateContent(string text, SolidColorBrush colourBrush = null)
        {
            return new Label
            {
                Content = text ?? "",
                Padding = new Thickness(5, 0, 5, 0),
                BorderThickness = new Thickness(0.5),
                BorderBrush = Brushes.Gray,
                Background = colourBrush ?? Brushes.Transparent,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontSize = fontSize,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Arial"),
            };
        }

        /// <summary>
        /// Creates a label for displaying product content in the specification grid.
        /// The label is styled with left horizontal alignment, gray border, and uses the specified font size and Arial font family.
        /// </summary>
        /// <param name="text">The text to display in the product content label.</param>
        /// <returns>A configured Label control representing the product content cell.</returns>
        private Label CreateProductContent(string text)
        {
            return new Label
            {
                Content = text ?? "",
                Padding = new Thickness(0.5),
                BorderThickness = new Thickness(0.5),
                BorderBrush = Brushes.Gray,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Center,
                FontSize = fontSize,
                FontFamily = new FontFamily("Arial")
            };
        }

        /// <summary>
        /// Adds a UIElement to the specified Grid at the given column and row.
        /// Sets the column and row for the element and adds it to the grid's children.
        /// </summary>
        /// <param name="grid">The Grid to which the element will be added.</param>
        /// <param name="element">The UIElement to add to the grid.</param>
        /// <param name="column">The column index where the element will be placed.</param>
        /// <param name="row">The row index where the element will be placed.</param>
        private void AddCellToGrid(Grid grid, UIElement element, int column, int row)
        {
            Grid.SetColumn(element, column);
            Grid.SetRow(element, row);
            grid.Children.Add(element);
        }

        /// <summary>
        /// Creates and displays two grids for printing specifications, splitting the PrintSpecifications collection
        /// into two pages. Each grid is initialized with headers and populated with data rows, then assigned to
        /// TableGrid1 and TableGrid2. The grids are also displayed in FlowDocuments for preview or printing.
        /// </summary>
        private void CreateAndShowGrid()
        {
            if (PrintSpecifications == null) return;

            var firstPageSpecs = PrintSpecifications.Take((int)Math.Ceiling((double)PrintSpecifications.Count() / 2));
            var secondPageSpecs = PrintSpecifications.Skip(firstPageSpecs.Count());

            var grid1 = InitializeGridWithHeaders();
            AddDataRowsToGrid(grid1, firstPageSpecs);

            var grid2 = InitializeGridWithHeaders();
            AddDataRowsToGrid(grid2, secondPageSpecs);

            TableGrid1 = grid1;
            TableGrid2 = grid2;

            DisplayGridsInFlowDocuments(grid1, grid2);
        }

        /// <summary>
        /// Initializes a new Grid with column definitions for product, instrument-specific columns (Temp, Load, RPM for each instrument),
        /// and fixed columns (DHSI, DB Date, Comment, Colour, Smple). Adds a header row to the grid.
        /// </summary>
        private Grid InitializeGridWithHeaders()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }); // Product column
            foreach (var instrument in Instruments)
            {
                for (int i = 0; i < 3; i++)
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // Add fixed columns
            for (int i = 0; i < 5; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            AddHeaderRowToGrid(grid);
            return grid;
        }

        /// <summary>
        /// Adds the header row to the provided grid, including product, instrument-specific columns (Temp, Load, RPM for each instrument),
        /// and fixed columns ("DHSI", "DB Date", "Comment", "Colour", "Smple"). Each instrument's columns are color-coded using the BackgroundColours list.
        /// </summary>
        /// <param name="grid">The Grid to which the header row will be added.</param>
        private void AddHeaderRowToGrid(Grid grid)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            int columnIndex = 0;
            AddCellToGrid(grid, CreateHeader("Product"), columnIndex++, 0);
            for (int i = 0; i < Instruments.Count(); i++)
            {
                AddCellToGrid(grid, CreateHeader($"{Instruments[i].name} \nTemp (°C)", BackgroundColours[i]), columnIndex++, 0);
                AddCellToGrid(grid, CreateHeader($"{Instruments[i].name} \nLoad (g)", BackgroundColours[i]), columnIndex++, 0);
                AddCellToGrid(grid, CreateHeader($"{Instruments[i].name} \nRPM", BackgroundColours[i]), columnIndex++, 0);
            }
            string[] fixedHeaders = { "DHSI", "DB Date", "Comment", "Colour", "Smple" };
            foreach (var header in fixedHeaders)
            {
                AddCellToGrid(grid, CreateHeader(header), columnIndex++, 0);
            }
        }

        /// <summary>
        /// Adds data rows to the provided grid for each specification in the given collection.
        /// Each row contains product information and instrument-specific values (Temp, Load, RPM),
        /// as well as additional fields such as COA, DB Date (with special background if older than 180 days),
        /// Comment, Colour, and Sample Amount. The method applies background colors to instrument columns
        /// and handles null or missing values gracefully.
        /// </summary>
        /// <param name="grid">The Grid to which data rows will be added.</param>
        /// <param name="specifications">The collection of PrintOutSpecification objects to display.</param>
        private void AddDataRowsToGrid(Grid grid, IEnumerable<PrintOutSpecification> specifications)
        {
            int rowIndex = 1;
            var today = DateTime.Now.Date;
            foreach (var spec in specifications)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                int columnIndex = 0;
                AddCellToGrid(grid, CreateProductContent(spec.ProductName), columnIndex++, rowIndex);
                for (int i = 0; i < Instruments.Count(); i++)
                {
                    var specData = spec.MachineSpecs.FirstOrDefault(ms => ms.MachineName == Instruments[i].name);
                    AddCellToGrid(grid, CreateContent(specData?.Temp.ToString() ?? "", BackgroundColours[i]), columnIndex++, rowIndex);
                    AddCellToGrid(grid, CreateContent(specData?.Load.ToString() ?? "", BackgroundColours[i]), columnIndex++, rowIndex);
                    AddCellToGrid(grid, CreateContent(specData?.Rpm.ToString() ?? "", BackgroundColours[i]), columnIndex++, rowIndex);
                }
                AddCellToGrid(grid, CreateContent(spec.Coa == true ? "COA" : ""), columnIndex++, rowIndex);

                if (spec.DbDate != null)
                {
                    TimeSpan dateDifference = today - spec.DbDate.Value;

                    if (dateDifference.Days > 180)
                    {
                        AddCellToGrid(grid, CreateContent(spec.DbDate?.ToString("dd/MM/yyyy"), DbDateBackgroundColour), columnIndex++, rowIndex);
                    }
                    else
                    {
                        AddCellToGrid(grid, CreateContent(spec.DbDate?.ToString("dd/MM/yyyy")), columnIndex++, rowIndex);
                    }

                }
                else
                {
                    AddCellToGrid(grid, CreateContent(spec.DbDate?.ToString("dd/MM/yyyy")), columnIndex++, rowIndex);
                }

                AddCellToGrid(grid, CreateContent(spec.Comment), columnIndex++, rowIndex);
                AddCellToGrid(grid, CreateContent(spec.Colour), columnIndex++, rowIndex);
                AddCellToGrid(grid, CreateContent(spec.SampleAmount?.ToString()), columnIndex++, rowIndex);
                rowIndex++;
            }
        }

        /// <summary>
        /// Displays the provided grids in FlowDocuments for preview or printing.
        /// Each grid is wrapped in a BlockUIContainer and assigned to SpecificationTable1 and SpecificationTable2.
        /// </summary>
        private void DisplayGridsInFlowDocuments(Grid grid1, Grid grid2)
        {
            var flowDoc1 = new FlowDocument();
            flowDoc1.Blocks.Add(new BlockUIContainer(grid1));

            var flowDoc2 = new FlowDocument();
            flowDoc2.Blocks.Add(new BlockUIContainer(grid2));

            SpecificationTable1 = flowDoc1;
            SpecificationTable2 = flowDoc2;
        }

        /// <summary>
        /// Prepares a clone of the provided Grid for printing by wrapping it in a new Grid, 
        /// applying margins, scaling it to fit the printable area of the specified PrintDialog, 
        /// and arranging it to match the page size. Returns the prepared UIElement for printing.
        /// </summary>
        /// <param name="originalGrid">The original Grid to be printed.</param>
        /// <param name="printDialog">The PrintDialog containing printable area information.</param>
        /// <returns>A UIElement ready for printing, containing the scaled and arranged grid.</returns>
        private UIElement PrepareGridForPrinting(Grid originalGrid, PrintDialog printDialog)
        {
            Grid printGrid = CloneGrid(originalGrid);

            Grid wrapperGrid = new Grid();
            wrapperGrid.Children.Add(printGrid);
            printGrid.Margin = new Thickness(150, 50, 150, 200);

            ScaleTransform scaleTransform = FitGridToPageSize(wrapperGrid, printDialog);
            wrapperGrid.LayoutTransform = scaleTransform;

            Size pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
            wrapperGrid.Measure(pageSize);
            wrapperGrid.Arrange(new Rect(new Point(0, 0), pageSize));

            return wrapperGrid;
        }

        /// <summary>
        /// Creates a deep clone of the provided Grid by serializing it to XAML and then deserializing it.
        /// This method is useful for duplicating UI elements for printing or display purposes in WPF.
        /// </summary>
        /// <param name="originalGrid">The original Grid to clone.</param>
        /// <returns>A new Grid instance that is a deep copy of the original.</returns>
        private Grid CloneGrid(Grid originalGrid)
        {
            string gridXaml = XamlWriter.Save(originalGrid);
            StringReader stringReader = new StringReader(gridXaml);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            Grid clonedGrid = (Grid)XamlReader.Load(xmlReader);
            return clonedGrid;
        }

        /// <summary>
        /// Calculates a scale transform to fit the provided grid within the printable area of the specified PrintDialog.
        /// Measures the grid, determines the scaling factors for width and height, and returns a ScaleTransform
        /// that ensures the grid fits within the printable page size while maintaining aspect ratio.
        /// </summary>
        /// <param name="grid">The Grid to be scaled for printing.</param>
        /// <param name="printDialog">The PrintDialog containing printable area dimensions.</param>
        /// <returns>A ScaleTransform that scales the grid to fit the printable area.</returns>
        private ScaleTransform FitGridToPageSize(Grid grid, PrintDialog printDialog)
        {
            double printableWidth = printDialog.PrintableAreaWidth;
            double printableHeight = printDialog.PrintableAreaHeight;

            grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            double scaleX = printableWidth / grid.DesiredSize.Width;
            double scaleY = printableHeight / grid.DesiredSize.Height;

            scaleX *= 1;

            double scale = Math.Min(scaleX, scaleY);

            return new ScaleTransform(scale, scale);
        }

        /// <summary>
        /// Combines multiple grids into a single printable document, adding headers and footers to each page.
        /// Displays a print dialog, prepares each grid for printing, and prints the resulting FixedDocument.
        /// </summary>
        /// <param name="gridsToPrint">A list of Grid controls to be printed, each representing a page.</param>
        /// <param name="documentTitle">The title of the document to be printed.</param>
        private void PrintCombinedGrids(List<Grid> gridsToPrint, string documentTitle)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                FixedDocument fixedDoc = new FixedDocument();
                fixedDoc.DocumentPaginator.PageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);

                foreach (Grid grid in gridsToPrint)
                {

                    // Create a FixedPage and add the prepared grid
                    FixedPage fixedPage = new FixedPage();
                    fixedPage.Width = printDialog.PrintableAreaWidth;
                    fixedPage.Height = printDialog.PrintableAreaHeight;

                    // Add content
                    //UIElement preparedElement = PrepareGridForPrinting(grid, printDialog);
                    UIElement preparedElement = Application.Current.Dispatcher.Invoke(() => PrepareGridForPrinting(grid, printDialog));
                    Size elementSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
                    preparedElement.Measure(elementSize);
                    preparedElement.Arrange(new Rect(elementSize));

                    // Calculate the left position to center the element horizontally
                    double leftOffset = (printDialog.PrintableAreaWidth - preparedElement.DesiredSize.Width) / 2;


                    FixedPage.SetLeft(preparedElement, leftOffset);
                    FixedPage.SetTop(preparedElement, 30);
                    FixedPage.SetBottom(preparedElement, printDialog.PrintableAreaHeight - 180);
                    fixedPage.Children.Add(preparedElement);

                    // Add Header
                    UIElement header = CreateHeader("Specification", printDialog.PrintableAreaWidth, TextAlignment.Center);
                    FixedPage.SetLeft(header, 0);
                    FixedPage.SetTop(header, 20);
                    fixedPage.Children.Add(header);

                    UIElement footer = CreateFooter($"Document No: F-QC-04a\nApproved by (Managing Director): Emre Goroglu\nDate:{DateTime.Now.ToString("d")}", printDialog.PrintableAreaWidth, TextAlignment.Left);
                    // Position the footer at the bottom of the page
                    FixedPage.SetLeft(footer, 0);
                    FixedPage.SetTop(footer, printDialog.PrintableAreaHeight - 50);
                    fixedPage.Children.Add(footer);

                    UIElement secondFooter = CreateFooter("Controlled copy authorised by", printDialog.PrintableAreaWidth, TextAlignment.Right);

                    // Measure and arrange secondFooter to calculate RenderSize
                    secondFooter.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    secondFooter.Arrange(new Rect(0, 0, secondFooter.DesiredSize.Width, secondFooter.DesiredSize.Height));

                    // Position the second footer at the bottom-right of the page
                    FixedPage.SetLeft(secondFooter, printDialog.PrintableAreaWidth - secondFooter.RenderSize.Width - 250);
                    FixedPage.SetTop(secondFooter, printDialog.PrintableAreaHeight - 50);
                    fixedPage.Children.Add(secondFooter);

                    // Add the FixedPage to the FixedDocument
                    PageContent pageContent = new PageContent();
                    ((IAddChild)pageContent).AddChild(fixedPage);
                    fixedDoc.Pages.Add(pageContent);
                }

                // Print the document
                //printDialog.PrintDocument(fixedDoc.DocumentPaginator, documentTitle);
                Application.Current.Dispatcher.Invoke(() => printDialog.PrintDocument(fixedDoc.DocumentPaginator, documentTitle));
            }
        }

        /// <summary>
        /// Creates a header UI element for printing, using a TextBlock with the specified text, width, and alignment.
        /// The header is styled with a top margin, font size 12, and bold font weight.
        /// </summary>
        /// <param name="headerText">The text to display in the header.</param>
        /// <param name="pageWidth">The width of the header, typically matching the printable page width.</param>
        /// <param name="alignment">The text alignment for the header (e.g., Center, Left, Right).</param>
        /// <returns>A TextBlock UIElement configured as a header.</returns>
        private UIElement CreateHeader(string headerText, double pageWidth, TextAlignment alignment)
        {
            TextBlock header = new TextBlock
            {
                Text = headerText,
                Width = pageWidth,
                TextAlignment = alignment,
                Margin = new Thickness(0, 5, 0, 0),
                FontSize = 12,
                FontWeight = FontWeights.Bold
            };

            return header;
        }

        /// <summary>
        /// Creates a footer UI element for printing, using a TextBlock with the specified text, width, and alignment.
        /// The footer is styled with a margin, font size 8, and the provided alignment.
        /// </summary>
        /// <param name="footerText">The text to display in the footer.</param>
        /// <param name="pageWidth">The width of the footer, typically matching the printable page width.</param>
        /// <param name="alignment">The text alignment for the footer (e.g., Center, Left, Right).</param>
        /// <returns>A TextBlock UIElement configured as a footer.</returns>
        private UIElement CreateFooter(string footerText, double pageWidth, TextAlignment alignment)
        {
            TextBlock footer = new TextBlock
            {
                Text = footerText,
                Width = pageWidth,
                TextAlignment = alignment,
                Margin = new Thickness(100, 0, 50, 20),
                FontSize = 8
            };

            return footer;
        }

        /// <summary>
        /// Initiates the print process for the specification tables. 
        /// Checks if the required grids are available, then prepares and prints them on a background thread using the application's dispatcher.
        /// </summary>
        public void Print()
        {
            if (TableGrid1 == null || TableGrid2 == null)
            {
                return;
            }

            List<Grid> gridsToPrint = new List<Grid> { TableGrid1, TableGrid2 };
            //PrintCombinedGrids(gridsToPrint, "Combined Document Title");
            Task.Run(() =>
            {
                // If the user proceeds with printing, continue on the background thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PrintCombinedGrids(gridsToPrint, "Combined Document Title");
                });
            });
        }
    }
}
