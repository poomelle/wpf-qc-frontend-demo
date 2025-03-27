using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.ReportVM;
using ChemsonLabApp.Services.IService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChemsonLabApp.MVVM.Views.Report
{
    /// <summary>
    /// Interaction logic for MakeReportGraphView.xaml
    /// </summary>
    public partial class MakeReportGraphView : Window
    {
        public MakeReportGraphView(MakeReportGraphViewModel makeReportGraphViewModel)
        {
            InitializeComponent();
            DataContext = makeReportGraphViewModel;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Send_Button_Click(object sender, RoutedEventArgs e)
        {
            var gridImage = CaptureGridAsImage(TorqueGraphGrid);
            var imageBytes = ConvertBitmapToByteArray(gridImage);

            CreateOutlookEmailWithAttachment(imageBytes);
        }

        private void CreateOutlookEmailWithAttachment(byte[] imageBytes)
        {
            var tempFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "GraphReport.png");
            File.WriteAllBytes(tempFilePath, imageBytes);

            try
            {
                var outlookApp = new Microsoft.Office.Interop.Outlook.Application();
                var mailItem = (Microsoft.Office.Interop.Outlook.MailItem)outlookApp.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
                mailItem.Subject = "Graph Report";
                mailItem.Body = "Please find the attached graph report.";
                mailItem.Attachments.Add(tempFilePath, Microsoft.Office.Interop.Outlook.OlAttachmentType.olByValue, Type.Missing, "GraphReport.png");
                mailItem.Display(true);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error creating Outlook email: " + ex.Message);
            }
        }

        private byte[] ConvertBitmapToByteArray(BitmapSource bitmap)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                return stream.ToArray();
            }
        }

        private RenderTargetBitmap CaptureGridAsImage(Grid grid)
        {
            var bounds = VisualTreeHelper.GetDescendantBounds(grid);
            var renderTargetBitmap = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, 96, 96, PixelFormats.Pbgra32);

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                var visualBrush = new VisualBrush(grid);
                drawingContext.DrawRectangle(visualBrush, null, new Rect(new Point(), bounds.Size));
            }

            renderTargetBitmap.Render(drawingVisual);
            return renderTargetBitmap;
        }
    }
}
