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

        private Label CreateProductDetailLabel(string content, int upperMargin = 20, int fontSize=14)
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

        private void CreateLabelDetailGridLayout(Grid qcLabelGrid, int rows)
        {
            qcLabelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            qcLabelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.8, GridUnitType.Star) });

            qcLabelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            qcLabelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            qcLabelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            qcLabelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

        }

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

        private void CreateLabelGridOverallLayout(Grid grid)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }

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

        // Print QC Labels function by printing each QC Label page from QCLabelPageGrids
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

        //private async Task RecordPrintedQCLabels()
        //{
        //    foreach (var qcLabel in QCLabels)
        //    {
        //        // get batch name of print qc label
        //        string batchName = GetBatchName(qcLabel.batchName);
        //        // get product name of print qc label
        //        string productName = qcLabel.product.name;
        //        int productId = qcLabel.product.id;

        //        // check if the qc label is already printed
        //        string qcLabelFilter = $"?batchName={batchName}&productName={productName}";
        //        var qcLabels = await _qcLabelService.GetAllQcLabelsAsync(filter: qcLabelFilter);

        //        // if not, create record of the printed qc label
        //        if (qcLabels.Count == 0 || qcLabels == null)
        //        {
        //            var newQCLabel = new QCLabel
        //            {
        //                batchName = batchName,
        //                productId = productId,
        //                printed = true,
        //            };
        //            await _qcLabelService.CreateQcLabelAsync(newQCLabel);
        //        }
        //    }
        //}

        //private string GetBatchName(string batchName)
        //{
        //    if (batchName.Contains("-"))
        //    {
        //        string yearMonth = batchName.Substring(0,3);
        //        string batchNumber = batchName.Split('-')[1].Trim();
        //        return $"{yearMonth}{batchNumber}";
        //    }
        //    return batchName;
        //}

        //public void PrintCombinedGrids(List<Grid> gridsToPrint, string documentTitle)
        //{
        //    PrintDialog printDialog = new PrintDialog();
        //    if (printDialog.ShowDialog() == true)
        //    {
        //        // Set the print ticket to landscape
        //        printDialog.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;

        //        FixedDocument fixedDoc = new FixedDocument();
        //        fixedDoc.DocumentPaginator.PageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);

        //        foreach (Grid grid in gridsToPrint)
        //        {
        //            // Create a FixedPage and add the prepared grid
        //            FixedPage fixedPage = new FixedPage();
        //            fixedPage.Width = printDialog.PrintableAreaWidth;
        //            fixedPage.Height = printDialog.PrintableAreaHeight;

        //            // Add content
        //            UIElement preparedElement = Application.Current.Dispatcher.Invoke(() => PrepareGridForPrinting(grid, printDialog));
        //            Size elementSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
        //            preparedElement.Measure(elementSize);
        //            preparedElement.Arrange(new Rect(elementSize));

        //            // Add the prepared element to the FixedPage
        //            fixedPage.Children.Add(preparedElement);

        //            // Add the FixedPage to the FixedDocument
        //            PageContent pageContent = new PageContent();
        //            ((IAddChild)pageContent).AddChild(fixedPage);
        //            fixedDoc.Pages.Add(pageContent);
        //        }

        //        // Print the document
        //        Application.Current.Dispatcher.Invoke(() => printDialog.PrintDocument(fixedDoc.DocumentPaginator, documentTitle));
        //    }
        //}

        //private UIElement PrepareGridForPrinting(Grid originalGrid, PrintDialog printDialog)
        //{
        //    Grid printGrid = CloneGrid(originalGrid);

        //    Grid wrapperGrid = new Grid();
        //    wrapperGrid.Children.Add(printGrid);

        //    printGrid.Margin = new Thickness(12, 24, 12, 12);

        //    ScaleTransform scaleTransform = FitGridToPageSize(wrapperGrid, printDialog);
        //    wrapperGrid.LayoutTransform = scaleTransform;

        //    Size pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
        //    wrapperGrid.Measure(pageSize);
        //    wrapperGrid.Arrange(new Rect(new Point(0, 0), pageSize));

        //    return wrapperGrid;
        //}

        //private Grid CloneGrid(Grid originalGrid)
        //{
        //    string gridXaml = XamlWriter.Save(originalGrid);
        //    StringReader stringReader = new StringReader(gridXaml);
        //    XmlReader xmlReader = XmlReader.Create(stringReader);
        //    Grid clonedGrid = (Grid)XamlReader.Load(xmlReader);
        //    return clonedGrid;
        //}

        //private ScaleTransform FitGridToPageSize(Grid grid, PrintDialog printDialog)
        //{
        //    double printableWidth = printDialog.PrintableAreaWidth;
        //    double printableHeight = printDialog.PrintableAreaHeight;

        //    grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        //    double scaleX = printableWidth / grid.DesiredSize.Width;
        //    double scaleY = printableHeight / grid.DesiredSize.Height;

        //    scaleX *= 1;

        //    double scale = Math.Min(scaleX, scaleY);

        //    return new ScaleTransform(scale, scale);
        //}
    }
}
