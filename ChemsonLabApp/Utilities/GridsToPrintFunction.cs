using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

namespace ChemsonLabApp.Utilities
{
    /// <summary>
    /// Provides utility functions for printing multiple WPF Grid elements as a combined document.
    /// Includes methods for preparing, cloning, and scaling grids to fit printable page sizes.
    /// </summary>
    public static class GridsToPrintFunction
    {
        /// <summary>
        /// Prints a list of Grid elements as a combined document, with options for orientation and margin.
        /// </summary>
        /// <param name="gridsToPrint">The list of Grid elements to print.</param>
        /// <param name="documentTitle">The title of the print document.</param>
        /// <param name="portrait">True for portrait orientation, false for landscape.</param>
        /// <param name="gridMargin">True to add margin to each grid, false otherwise.</param>
        public static void PrintCombinedGrids(List<Grid> gridsToPrint, string documentTitle, bool portrait, bool gridMargin)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                if (portrait)
                {
                    printDialog.PrintTicket.PageOrientation = PageOrientation.Portrait;
                }
                else
                {
                    printDialog.PrintTicket.PageOrientation = PageOrientation.Landscape;
                }

                FixedDocument fixedDoc = new FixedDocument();
                fixedDoc.DocumentPaginator.PageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);

                foreach (Grid grid in gridsToPrint)
                {
                    // Create a FixedPage and add the prepared grid
                    FixedPage fixedPage = new FixedPage();
                    fixedPage.Width = printDialog.PrintableAreaWidth;
                    fixedPage.Height = printDialog.PrintableAreaHeight;

                    // Add content
                    UIElement preparedElement = Application.Current.Dispatcher.Invoke(() => PrepareGridForPrinting(grid, printDialog, gridMargin));
                    Size elementSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
                    preparedElement.Measure(elementSize);
                    preparedElement.Arrange(new Rect(elementSize));

                    fixedPage.Children.Add(preparedElement);

                    // Add the FixedPage to the FixedDocument
                    PageContent pageContent = new PageContent();
                    ((IAddChild)pageContent).AddChild(fixedPage);
                    fixedDoc.Pages.Add(pageContent);
                }

                // Print the document
                Application.Current.Dispatcher.Invoke(() => printDialog.PrintDocument(fixedDoc.DocumentPaginator, documentTitle));
            }
        }

        /// <summary>
        /// Prepares a Grid for printing by cloning, optionally adding margin, and scaling to fit the page.
        /// </summary>
        /// <param name="originalGrid">The original Grid to prepare.</param>
        /// <param name="printDialog">The PrintDialog providing page size information.</param>
        /// <param name="gridMargin">True to add margin to the grid, false otherwise.</param>
        /// <returns>A UIElement ready for printing.</returns>
        private static UIElement PrepareGridForPrinting(Grid originalGrid, PrintDialog printDialog, bool gridMargin)
        {
            Grid printGrid = CloneGrid(originalGrid);

            Grid wrapperGrid = new Grid();
            wrapperGrid.Children.Add(printGrid);

            if (gridMargin)
            {
                printGrid.Margin = new Thickness(12, 24, 12, 12);
            }

            ScaleTransform scaleTransform = FitGridToPageSize(wrapperGrid, printDialog);
            wrapperGrid.LayoutTransform = scaleTransform;

            Size pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
            wrapperGrid.Measure(pageSize);
            wrapperGrid.Arrange(new Rect(new Point(0, 0), pageSize));

            return wrapperGrid;
        }

        /// <summary>
        /// Creates a deep clone of the specified Grid using XAML serialization.
        /// </summary>
        /// <param name="originalGrid">The Grid to clone.</param>
        /// <returns>A cloned Grid instance.</returns>
        private static Grid CloneGrid(Grid originalGrid)
        {
            string gridXaml = XamlWriter.Save(originalGrid);
            StringReader stringReader = new StringReader(gridXaml);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            Grid clonedGrid = (Grid)XamlReader.Load(xmlReader);
            return clonedGrid;
        }

        /// <summary>
        /// Calculates a ScaleTransform to fit the Grid within the printable area of the page.
        /// </summary>
        /// <param name="grid">The Grid to scale.</param>
        /// <param name="printDialog">The PrintDialog providing printable area dimensions.</param>
        /// <returns>A ScaleTransform for fitting the grid to the page.</returns>
        private static ScaleTransform FitGridToPageSize(Grid grid, PrintDialog printDialog)
        {
            double printableWidth = printDialog.PrintableAreaWidth;
            double printableHeight = printDialog.PrintableAreaHeight;

            grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            double scaleX = printableWidth / grid.DesiredSize.Width;
            double scaleY = printableHeight / grid.DesiredSize.Height;

            double scale = Math.Min(scaleX, scaleY);

            return new ScaleTransform(scale, scale);
        }
    }
}
