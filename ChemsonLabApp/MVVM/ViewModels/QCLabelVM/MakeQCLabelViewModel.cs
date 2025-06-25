using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.CustomerVM.Commands;
using ChemsonLabApp.MVVM.ViewModels.QCLabelVM.Command;
using ChemsonLabApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using PropertyChanged;
using System.CodeDom;
using Windows.Media.Audio;
using System.IO.Packaging;
using System.IO;
using System.Windows.Xps.Packaging;
using System.Windows.Markup;
using System.Xml;
using Windows.Media.Capture;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Services.IService;

namespace ChemsonLabApp.MVVM.ViewModels.QCLabelVM
{
    [AddINotifyPropertyChangedInterface]
    public class MakeQCLabelViewModel
    {
        private readonly IQcLabelService _qcLabelService;

        public List<QCLabel> QCLabels { get; set; } = new List<QCLabel>();
        public List<Grid> QCLabelPageGrids { get; set; } = new List<Grid>();
        public FlowDocument QCLabelPageDoc { get; set; }
        public List<FlowDocument> QCLabelPageDocs { get; set; } = new List<FlowDocument>();
        public double NumberOfLabelsPerPage { get; set; } = 8.0;
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; } = 1;
        public NextQCLabelCommand NextQCLabelCommand { get; set; }
        public PreviousQCLabelCommand PreviousQCLabelCommand { get; set; }
        public PrintQCLabelsCommand PrintQCLabelsCommand { get; set; }

        public MakeQCLabelViewModel(IQcLabelService qcLabelService)
        {
            // services
            this._qcLabelService = qcLabelService;

            // commands
            NextQCLabelCommand = new NextQCLabelCommand(this);
            PreviousQCLabelCommand = new PreviousQCLabelCommand(this);
            PrintQCLabelsCommand = new PrintQCLabelsCommand(this);
        }

        /// <summary>
        /// Creates the QC label pages by generating grids for each QC label, organizing them into pages,
        /// and converting each page grid into a FlowDocument for display or printing.
        /// Each QC label is represented by two grids, and up to 8 labels are placed on a single page.
        /// Updates the current page document and total page count accordingly.
        /// </summary>
        public void CreateQCLabelsPage()
        {
            try
            {
                QCLabelPageGrids.Clear();
                QCLabelPageDocs.Clear();

                //var numberOfQCLabels = QCLabels.Count;

                List<Grid> qcLabelGrids = new List<Grid>();

                // Create 2 QC Label Grids per each qcLabel
                foreach (var qcLabel in QCLabels)
                {
                    Grid qcLabelGridFirst = new Grid();
                    Grid qcLabelGridSecond = new Grid();

                    // Create QC Label of each qcLabels
                    CreateQCLabelGrid(qcLabelGridFirst, qcLabel);
                    CreateQCLabelGrid(qcLabelGridSecond, qcLabel);

                    // Add QC Label Grids to qcLabelGrids
                    qcLabelGrids.Add(qcLabelGridFirst);
                    qcLabelGrids.Add(qcLabelGridSecond);
                }

                // Create QC Label Page Grids and populate QC Label Grids from qcLabelGrids (8 QC Labels in one page)
                CreateQCLabelPageGrids(qcLabelGrids);
                InputQCLabelPages(qcLabelGrids);


                foreach (var qcLabelPageGrid in QCLabelPageGrids)
                {
                    FlowDocument qcLabelDoc = new FlowDocument();
                    qcLabelDoc.Blocks.Add(new BlockUIContainer(qcLabelPageGrid));
                    QCLabelPageDocs.Add(qcLabelDoc);
                }
                QCLabelPageDoc = QCLabelPageDocs[CurrentPage - 1];
                TotalPages = QCLabelPageDocs.Count;
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error in CreateQCLabelsPage");
                LoggerUtility.LogError(ex);
            }

        }

        /// <summary>
        /// Distributes the provided QC label grids into the page grids, placing up to 8 QC label grids per page.
        /// Each label grid is positioned within the page grid using row and column indices.
        /// When a page is filled (8 labels), moves to the next page grid.
        /// </summary>
        private void InputQCLabelPages(List<Grid> qcLabelGrids)
        {
            // input qc label grids to qc label page grids (8 qc label grids in one page)
            int startColumn = 0;
            int startRow = 0;
            int startPage = 0;

            int indexOfLabel = 1;

            foreach (var qcLabelGrid in qcLabelGrids)
            {

                if (startRow == 2)
                {
                    startRow = 0;
                    startColumn++;
                }

                Grid.SetRow(qcLabelGrid, startRow++);
                Grid.SetColumn(qcLabelGrid, startColumn);
                QCLabelPageGrids[startPage].Children.Add(qcLabelGrid);

                if (indexOfLabel % 8 == 0)
                {
                    startColumn = 0;
                    startRow = 0;
                    startPage++;
                }

                indexOfLabel++;
            }
        }

        /// <summary>
        /// Creates the required number of page grids for QC labels, based on the total number of label grids and the configured number of labels per page.
        /// Each page grid is initialized with a specific layout (4 columns, 2 rows) to accommodate the labels.
        /// The created page grids are added to the QCLabelPageGrids collection for further processing or display.
        /// </summary>
        private void CreateQCLabelPageGrids(List<Grid> qcLabelGrids)
        {
            var numberOfPages = (int)Math.Ceiling(qcLabelGrids.Count / NumberOfLabelsPerPage);

            if (numberOfPages > 0)
            {
                for (int i = 0; i < numberOfPages; i++)
                {
                    Grid qcLabelPageGrid = new Grid();
                    CreateGridLayout(qcLabelPageGrid, 4, 2);
                    QCLabelPageGrids.Add(qcLabelPageGrid);
                }
            }
        }

        /// <summary>
        /// Configures a single QC label grid by setting its size, margin, layout, header, and product details.
        /// The grid is sized in millimeters (converted to device-independent pixels), and its layout is prepared
        /// for display or printing. The method adds header and product detail sections to the grid.
        /// </summary>
        private void CreateQCLabelGrid(Grid qcLabelGrid, QCLabel qcLabel)
        {
            double widthInMM = 73.818;
            double heightInMM = 105;

            // Convert mm to DIPs
            double widthInDIPs = widthInMM * 96 / 25.4;
            double heightInDIPs = heightInMM * 96 / 25.4;

            qcLabelGrid.Width = widthInDIPs;
            qcLabelGrid.Height = heightInDIPs;

            qcLabelGrid.Margin = new Thickness(0, 20, 20, 0);

            // Step1 Create QC Label Grid layout
            CreateLabelGridOverallLayout(qcLabelGrid);

            // Step2 Populate QC Label Grid Header
            InputHeaderTemplate(qcLabelGrid);

            // Step3 Populate QC Label Grid Product detail
            InputQCLabelPages(qcLabelGrid, qcLabel);
        }

        /// <summary>
        /// Adds the product detail section to the specified QC label grid.
        /// This method creates a detail grid, sets up its layout, populates it with header and product information
        /// from the provided QCLabel, and inserts it into the main QC label grid at the appropriate position.
        /// </summary>
        private void InputQCLabelPages(Grid qcLabelGrid, QCLabel qcLabel)
        {
            Grid labelDetailGrid = new Grid();
            CreateLabelDetailGridLayout(labelDetailGrid, 4);
            InputDetailHeader(labelDetailGrid);
            InputDetailProduct(labelDetailGrid, qcLabel);

            Grid.SetRow(labelDetailGrid, 2);
            Grid.SetColumn(labelDetailGrid, 0);
            qcLabelGrid.Children.Add(labelDetailGrid);
        }

        /// <summary>
        /// Populates the product detail section of the QC label detail grid with product name, weight, total weight, and batch name.
        /// Each detail is added as a label to the appropriate row and column, and a lower border is added for visual separation.
        /// </summary>
        private void InputDetailProduct(Grid labelDetailGrid, QCLabel qcLabel)
        {
            Label productNameLabel = CreateProductDetailLabel(qcLabel.product.name);
            Grid.SetRow(productNameLabel, 0);
            Grid.SetColumn(productNameLabel, 1);
            labelDetailGrid.Children.Add(productNameLabel);
            AddLowerBorder(labelDetailGrid, 0, 1);

            Label weight = CreateProductDetailLabel($"{qcLabel.weight} (Kg)", fontSize: 14);
            Grid.SetRow(weight, 1);
            Grid.SetColumn(weight, 1);
            labelDetailGrid.Children.Add(weight);
            AddLowerBorder(labelDetailGrid, 1, 1);

            var totalWeight = GetTotalWeight(qcLabel.weight);
            Label totalWeightLabel = CreateProductDetailLabel(totalWeight, 0, fontSize: 14);
            Grid.SetRow(totalWeightLabel, 2);
            Grid.SetColumn(totalWeightLabel, 1);
            labelDetailGrid.Children.Add(totalWeightLabel);

            Label batchName = CreateProductDetailLabel(qcLabel.batchName, fontSize: 14);
            Grid.SetRow(batchName, 3);
            Grid.SetColumn(batchName, 1);
            labelDetailGrid.Children.Add(batchName);
            AddLowerBorder(labelDetailGrid, 3, 1);
        }

        /// <summary>
        /// Creates a Label control for displaying product detail information on a QC label.
        /// The label is styled with bold Arial font, customizable top margin, and font size.
        /// The content is left-aligned and vertically centered, with stretch alignment for both axes.
        /// </summary>
        private Label CreateProductDetailLabel(string content, int upperMargin = 20, int fontSize = 14)
        {
            return new Label
            {
                Content = content,
                Padding = new Thickness(2),
                Margin = new Thickness(0, upperMargin, 0, 0),
                FontSize = fontSize,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Arial"),
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
        }

        /// <summary>
        /// Adds header labels ("PRODUCT:", "NET WT.:", "BATCH No.:") to the specified QC label detail grid.
        /// Each label is placed in the first column of its respective row to provide descriptive headers
        /// for the product details section of the QC label.
        /// </summary>
        private void InputDetailHeader(Grid labelDetailGrid)
        {
            Label labelProduct = CreateHeaderLabel("PRODUCT:");
            Grid.SetRow(labelProduct, 0);
            Grid.SetColumn(labelProduct, 0);
            labelDetailGrid.Children.Add(labelProduct);

            Label labelWeight = CreateHeaderLabel("NET WT.:");
            Grid.SetRow(labelWeight, 1);
            Grid.SetColumn(labelWeight, 0);
            labelDetailGrid.Children.Add(labelWeight);

            Label labelBatchName = CreateHeaderLabel("BATCH No.:");
            Grid.SetRow(labelBatchName, 3);
            Grid.SetColumn(labelBatchName, 0);
            labelDetailGrid.Children.Add(labelBatchName);
        }

        /// <summary>
        /// Creates a Label control styled for use as a header in the QC label detail grid.
        /// The label uses bold Arial font, right-aligned content, and a top margin for spacing.
        /// </summary>
        private static Label CreateHeaderLabel(string headerName)
        {
            return new Label
            {
                Content = headerName,
                Padding = new Thickness(2),
                Margin = new Thickness(0, 20, 0, 0),
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Arial"),
                HorizontalContentAlignment = HorizontalAlignment.Right,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
        }

        /// <summary>
        /// Configures the layout of the QC label detail grid by adding the required number of column and row definitions.
        /// The grid is set up with two columns (one for headers, one for values) and four rows (for product name, weight, total weight, and batch name).
        /// </summary>
        private void CreateLabelDetailGridLayout(Grid qcLabelGrid, int rows)
        {
            qcLabelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            qcLabelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.8, GridUnitType.Star) });

            qcLabelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            qcLabelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            qcLabelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            qcLabelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        }

        /// <summary>
        /// Adds the header section to the specified QC label grid.
        /// This includes a "QUALITY CONTROL" text block with underline decoration and a large "OK" label,
        /// both styled and positioned appropriately within the grid.
        /// </summary>
        private void InputHeaderTemplate(Grid qcLabelGrid)
        {
            TextBlock qcWord = new TextBlock
            {
                Text = "QUALITY CONTROL",
                TextDecorations = TextDecorations.Underline,
                Padding = new Thickness(24, 0, 0, 0),
                FontSize = 23,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Arial"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetRow(qcWord, 0);
            Grid.SetColumn(qcWord, 0);
            qcLabelGrid.Children.Add(qcWord);

            Label OKWord = new Label
            {
                Content = "OK",
                Padding = new Thickness(0),
                FontSize = 140,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Arial"),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            Grid.SetRow(OKWord, 1);
            Grid.SetColumn(OKWord, 0);
            qcLabelGrid.Children.Add(OKWord);
        }

        /// <summary>
        /// Configures the overall layout of a QC label grid by adding three row definitions.
        /// The first row is set to auto height for the header, and the next two rows use star sizing
        /// to proportionally fill the remaining space for the main content and details.
        /// </summary>
        private void CreateLabelGridOverallLayout(Grid grid)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }

        /// <summary>
        /// Configures the layout of the specified grid by adding the given number of columns and rows.
        /// Each column and row is set to auto size. This method is used to prepare a grid for placing
        /// child elements in a structured layout, such as for label pages or detail sections.
        /// </summary>
        private void CreateGridLayout(Grid grid, int columns, int rows)
        {
            int totalColumns = columns;
            int totalRows = rows;

            for (int i = 0; i < totalColumns; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            }

            for (int i = 0; i < totalRows; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            }
        }

        /// <summary>
        /// Calculates and returns the total weight string for a given weight input.
        /// If the weight string contains 'x', it is interpreted as a multiplication of the number of bags and the weight per bag (e.g., "3x25" means 3 bags of 25 Kg each).
        /// The method multiplies these values and returns the result in the format "= {totalWeight} (Kg)".
        /// If the input does not contain 'x', an empty string is returned.
        /// Handles exceptions by logging and showing an error notification.
        /// </summary>
        private string GetTotalWeight(string weight)
        {
            try
            {
                var totalWeightString = "";
                // check of weight contain x or not
                if (weight.Contains('x'))
                {
                    var numberOfPaperBags = int.Parse(weight.Substring(0, weight.IndexOf('x')).Trim());
                    var weightOfPaperBag = double.Parse(weight.Substring(weight.IndexOf('x') + 1).Trim());
                    var totalWeight = numberOfPaperBags * weightOfPaperBag;

                    totalWeightString = $"= {totalWeight} (Kg)";
                }
                return totalWeightString;
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error in GetTotalWeight");
                LoggerUtility.LogError(ex);
                return "";
            }
        }

        /// <summary>
        /// Adds a lower border to the specified cell in a Grid.
        /// The border is applied to the bottom edge of the cell at the given row and column indices.
        /// </summary>
        /// <param name="grid">The Grid to which the border will be added.</param>
        /// <param name="row">The row index of the cell.</param>
        /// <param name="column">The column index of the cell.</param>
        private void AddLowerBorder(Grid grid, int row, int column)
        {
            Border border = new Border
            {
                BorderThickness = new Thickness(0, 0, 0, 1),
                BorderBrush = Brushes.Black,
            };
            Grid.SetRow(border, row);
            Grid.SetColumn(border, column);
            grid.Children.Add(border);
        }

        /// <summary>
        /// Advances to the next QC label page if available.
        /// Increments the CurrentPage property and updates the QCLabelPageDoc to display the next page of QC labels.
        /// Handles exceptions by showing an error notification and logging the error.
        /// </summary>
        public void NextQCLabels()
        {
            try
            {
                if (CurrentPage < TotalPages)
                {
                    CurrentPage++;
                    QCLabelPageDoc = QCLabelPageDocs[CurrentPage - 1];
                }
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error in NextQCLabels");
                LoggerUtility.LogError(ex);
            }
        }

        /// <summary>
        /// Navigates to the previous QC label page if available.
        /// Decrements the CurrentPage property and updates the QCLabelPageDoc to display the previous page of QC labels.
        /// Handles exceptions by showing an error notification and logging the error.
        /// </summary>
        public void PreviousQCLabels()
        {
            try
            {
                if (CurrentPage > 1)
                {
                    CurrentPage--;
                    QCLabelPageDoc = QCLabelPageDocs[CurrentPage - 1];
                }
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error in PreviousQCLabels");
                LoggerUtility.LogError(ex);
            }
        }

        /// <summary>
        /// Prints the QC labels by sending the current QC label page grids to the print function
        /// and then creates QC label records in the backend service for the printed labels.
        /// Handles exceptions by displaying an error notification and logging the error.
        /// </summary>
        public async void PrintQCLabels()
        {
            try
            {
                GridsToPrintFunction.PrintCombinedGrids(QCLabelPageGrids, "QC Labels", false, true);
                //PrintCombinedGrids(QCLabelPageGrids, "QC Labels");
                //await RecordPrintedQCLabels();
                await _qcLabelService.CreateQcLabelFromListAsync(QCLabels);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error in PrintQCLabels");
                LoggerUtility.LogError(ex);
            }
        }
    }
}
