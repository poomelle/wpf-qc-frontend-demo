using ClosedXML.Graphics;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChemsonLabApp.Utilities
{
    public class PdfUtility
    {
        public static List<string> CreateCOAPdfFromGrids(List<Grid> coaReportDocs, string productName, bool combineCOA, string filePath)
        {
            List<string> savedPdfPaths = new List<string>();
            // get the first page of the COA report

            string firstBatch = ExtractBatchNumber(coaReportDocs, 5);
            string lastBatch = ExtractBatchNumber(coaReportDocs, coaReportDocs[0].RowDefinitions.Count - 1);

            string batchName = combineCOA ? BatchUtility.FormatBatchRange(firstBatch, lastBatch) : firstBatch;
            string sanitizedProductName = FileFolderUtility.SanitizeFileName(productName);
            string yearFolder = BatchUtility.YearFromBatchName(firstBatch);

            string fullPath = Path.Combine(filePath, yearFolder, sanitizedProductName);

            FileFolderUtility.EnsureDirectoryExists(fullPath);

            string pdfFileName = $"CoA {sanitizedProductName} {batchName}.pdf";

            string pdfPath = FileFolderUtility.GetValidFilePath(fullPath, pdfFileName);

            foreach (var grid in coaReportDocs)
            {
                double dpi = 96;
                double padding = 36 * (96.0 / 72.0);

                double drawableWidth = (595 - 2 * (padding / (dpi / 72.0))) * (dpi / 72.0);
                double drawableHeight = (842 - 2 * (padding / (dpi / 72.0))) * (dpi / 72.0);

                Size size = new Size(drawableWidth, drawableHeight);

                grid.Measure(size);
                grid.Arrange(new Rect(new Point(0, 0), size));

                RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96d, 96d, PixelFormats.Default);
                renderTarget.Render(grid);

                PdfDocument document = new PdfDocument();

                using (MemoryStream memStream = new MemoryStream())
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(renderTarget));
                    encoder.Save(memStream);
                    memStream.Position = 0;

                    XImage image = XImage.FromStream(memStream);

                    PdfPage page = document.AddPage();
                    page.Size = PageSize.A4;
                    XGraphics gfx = XGraphics.FromPdfPage(page);

                    gfx.DrawImage(image, padding, padding, page.Width - 2 * padding, page.Height - 2 * padding);
                }

                document.Save(pdfPath);

                savedPdfPaths.Add(pdfPath);

                document.Close();
            }

            return savedPdfPaths;

        }

        //private static PdfPage CreateCOAPDFPageFromGrid(Grid grid)
        //{
        //    double dpi = 96; 
        //    double padding = 36 * (96.0 / 72.0);

        //    double drawableWidth = (595 - 2 * (padding / (dpi / 72.0))) * (dpi / 72.0);
        //    double drawableHeight = (842 - 2 * (padding / (dpi / 72.0))) * (dpi / 72.0);

        //    Size size = new Size(drawableWidth, drawableHeight);

        //    grid.Measure(size);
        //    grid.Arrange(new Rect(new Point(0, 0), size));

        //    RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96d, 96d, PixelFormats.Default);
        //    renderTarget.Render(grid);

        //    PdfDocument document = new PdfDocument();

        //    using (MemoryStream memStream = new MemoryStream())
        //    {
        //        PngBitmapEncoder encoder = new PngBitmapEncoder();
        //        encoder.Frames.Add(BitmapFrame.Create(renderTarget));
        //        encoder.Save(memStream);
        //        memStream.Position = 0;

        //        XImage image = XImage.FromStream(memStream);

        //        PdfPage page = document.AddPage();
        //        page.Size = PageSize.A4;
        //        XGraphics gfx = XGraphics.FromPdfPage(page);

        //        gfx.DrawImage(image, padding, padding, page.Width - 2 * padding, page.Height - 2 * padding);
        //    }

        //    return document.;
        //}

        //private static PdfPage CreateCOAPDFPageFromGrid(Grid grid)
        //{
        //    PdfPage page = new PdfPage { Size = PdfSharp.PageSize.A4 };
        //    XGraphics gfx = XGraphics.FromPdfPage(page);


        //    double dpi = 96, padding = 36 * (96.0 / 72.0);
        //    double drawableWidth = (595 - 2 * (padding / (dpi / 72.0))) * (dpi / 72.0);
        //    double drawableHeight = (842 - 2 * (padding / (dpi / 72.0))) * (dpi / 72.0);

        //    Size size = new Size(drawableWidth, drawableHeight);
        //    grid.Measure(size);
        //    grid.Arrange(new Rect(new Point(0, 0), size));

        //    RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)size.Width, (int)size.Height, dpi, dpi, PixelFormats.Default);
        //    renderTarget.Render(grid);

        //    using (MemoryStream memStream = new MemoryStream())
        //    {
        //        PngBitmapEncoder encoder = new PngBitmapEncoder();
        //        encoder.Frames.Add(BitmapFrame.Create(renderTarget));
        //        encoder.Save(memStream);
        //        memStream.Position = 0;

        //        XImage image = XImage.FromStream(memStream);
        //        gfx.DrawImage(image, padding, padding, page.Width - 2 * padding, page.Height - 2 * padding);
        //    }

        //    return page;
        //}

        private static string ExtractBatchNumber(List<Grid> coaReportDocs, int row)
        {
            int batchColumn = 2;
            return ((TextBox)coaReportDocs[0].Children.Cast<UIElement>().First(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == batchColumn)).Text;
        }
    }


}
