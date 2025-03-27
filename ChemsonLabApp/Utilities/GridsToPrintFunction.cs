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
    public static class GridsToPrintFunction
    {
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

                    // Calculate the left position to center the element horizontally
                    //double leftOffset = (printDialog.PrintableAreaWidth - preparedElement.DesiredSize.Width) / 2;
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

        private static UIElement PrepareGridForPrinting(Grid originalGrid, PrintDialog printDialog, bool gridMargin)
        {
            Grid printGrid = CloneGrid(originalGrid);

            Grid wrapperGrid = new Grid();
            wrapperGrid.Children.Add(printGrid);
            //printGrid.Margin = new Thickness(150, 50, 150, 200);

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

        private static Grid CloneGrid(Grid originalGrid)
        {
            string gridXaml = XamlWriter.Save(originalGrid);
            StringReader stringReader = new StringReader(gridXaml);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            Grid clonedGrid = (Grid)XamlReader.Load(xmlReader);
            return clonedGrid;
        }

        private static ScaleTransform FitGridToPageSize(Grid grid, PrintDialog printDialog)
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
    }
}
