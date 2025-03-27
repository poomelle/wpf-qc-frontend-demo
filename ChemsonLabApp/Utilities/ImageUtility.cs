using ChemsonLabApp.MVVM.Models;
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
    public static class ImageUtility
    {
        public static void CreateAndSaveImageFromGrid(Grid grid, string fullFilePath)
        {
            grid.UpdateLayout();
            grid.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            grid.Arrange(new Rect(0, 0, grid.DesiredSize.Width, grid.DesiredSize.Height));

            var testReportImage = RenderVisualToBitmap(grid, (int)grid.DesiredSize.Width, (int)grid.DesiredSize.Height);
            SaveBitmapToFile(testReportImage, fullFilePath);
        }

        private static RenderTargetBitmap RenderVisualToBitmap(Visual visual, int width, int height)
        {
            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            return bitmap;
        }

        private static void SaveBitmapToFile(RenderTargetBitmap bitmap, string filePath)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
    }
}
