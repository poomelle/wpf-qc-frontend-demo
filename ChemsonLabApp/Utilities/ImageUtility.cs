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
    /// <summary>
    /// Provides utility methods for rendering a WPF Grid to an image and saving it to disk.
    /// </summary>
    public static class ImageUtility
    {
        /// <summary>
        /// Renders the specified Grid to a PNG image and saves it to the given file path.
        /// </summary>
        /// <param name="grid">The Grid to render as an image.</param>
        /// <param name="fullFilePath">The full file path where the image will be saved.</param>
        public static void CreateAndSaveImageFromGrid(Grid grid, string fullFilePath)
        {
            grid.UpdateLayout();
            grid.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            grid.Arrange(new Rect(0, 0, grid.DesiredSize.Width, grid.DesiredSize.Height));

            var testReportImage = RenderVisualToBitmap(grid, (int)grid.DesiredSize.Width, (int)grid.DesiredSize.Height);
            SaveBitmapToFile(testReportImage, fullFilePath);
        }

        /// <summary>
        /// Renders a Visual to a RenderTargetBitmap with the specified width and height.
        /// </summary>
        /// <param name="visual">The Visual to render.</param>
        /// <param name="width">The width of the resulting bitmap.</param>
        /// <param name="height">The height of the resulting bitmap.</param>
        /// <returns>A RenderTargetBitmap containing the rendered visual.</returns>
        private static RenderTargetBitmap RenderVisualToBitmap(Visual visual, int width, int height)
        {
            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            return bitmap;
        }

        /// <summary>
        /// Saves a RenderTargetBitmap to a file as a PNG image.
        /// </summary>
        /// <param name="bitmap">The bitmap to save.</param>
        /// <param name="filePath">The file path where the image will be saved.</param>
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
