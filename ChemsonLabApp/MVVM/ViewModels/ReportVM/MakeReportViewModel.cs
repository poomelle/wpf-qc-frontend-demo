using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using ChemsonLabApp.MVVM.Models;
using PropertyChanged;
using System.IO;
using System.Windows.Controls;
using ChemsonLabApp.MVVM.ViewModels.ReportVM.Command;
using Microsoft.Office.Interop;
using ChemsonLabApp.RestAPI;
using System.Windows;
using System.Windows.Media.Media3D;
using ChemsonLabApp.Utilities;
using System.Net.Http;
using System.Globalization;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Azure.Identity;
using ChemsonLabApp.Services;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Services.EmailServices.IEmailService;

namespace ChemsonLabApp.MVVM.ViewModels.ReportVM
{
    [AddINotifyPropertyChangedInterface]
    public class MakeReportViewModel
    {
        public List<BatchTestResult> BatchTestResults { get; set; }

        private readonly IReportService _reportService;
        private readonly IEmailService _emailService;

        public ObservableCollection<BatchTestResult> OutputBatchTestResults { get; set; }
        public SaveAndSendCommand SaveAndSendCommand { get; set; }
        public long AveTestTimeTick { get; set; }
        public Grid TestReportGrid { get; set; }

        public MakeReportViewModel(IReportService reportService, IEmailService emailService)
        {
            SaveAndSendCommand = new SaveAndSendCommand(this);

            this._reportService = reportService;
            this._emailService = emailService;
        }

        /// <summary>
        /// Calculates the average test time tick from the provided batch test results
        /// and assigns the result to the AveTestTimeTick property. Handles and logs exceptions.
        /// </summary>
        public void CalculateAveTestTimeTick(List<BatchTestResult> batchTestResults)
        {
            try
            {
                AveTestTimeTick = _reportService.CalculateAveTestTimeTick(batchTestResults);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Failed to calculate average test time tick.");
                LoggerUtility.LogError(ex);
            }
        }

        /// <summary>
        /// Initializes the OutputBatchTestResults collection with the provided batch test results.
        /// Handles exceptions related to HTTP requests and general errors, displaying appropriate notifications and logging errors.
        /// Displays a loading cursor during the operation.
        /// </summary>
        public void InitializeParameter(List<BatchTestResult> batchTestResults)
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                OutputBatchTestResults = new ObservableCollection<BatchTestResult>();
                foreach (var batchTestResult in batchTestResults)
                {
                    OutputBatchTestResults.Add(batchTestResult);
                }
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Error: Failed to load test results. Please check internet connection.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: Failed to load test results.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        /// <summary>
        /// Saves or updates the report data and generates an image from the TestReportGrid.
        /// If the operation is successful, prompts the user to send the report via email.
        /// Handles HTTP and general exceptions, displaying notifications and logging errors.
        /// Displays a loading cursor during the operation.
        /// </summary>
        public async void SaveAndSend()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                if (await _reportService.SaveOrUpdateReports(OutputBatchTestResults.ToList(), AveTestTimeTick))
                {
                    string fullFilePath = _reportService.getFileLocation(OutputBatchTestResults.ToList());
                    ImageUtility.CreateAndSaveImageFromGrid(TestReportGrid, fullFilePath);

                    if (NotificationUtility.ShowConfirmation("Report saved successfully. Do you want to send email?"))
                    {
                        CreateOutlookEmailWithImage(fullFilePath, Constants.Constants.FromAddress);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Error: Failed to save test results. Please check internet connection.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: Failed to save test results.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        /// <summary>
        /// Creates and opens an Outlook email with the specified image attached and sender address.
        /// The email subject is generated based on the current batch test results.
        /// </summary>
        /// <param name="imagePath">The file path of the image to attach to the email.</param>
        /// <param name="fromAddress">The sender's email address.</param>
        private void CreateOutlookEmailWithImage(string imagePath, string fromAddress)
        {
            string mailSubject = GetMailSubject();
            _emailService.CreateReportEmailAndOpenOutlook(mailSubject, imagePath, fromAddress);
        }

        /// <summary>
        /// Generates the email subject for the report based on the product name and batch information
        /// from the OutputBatchTestResults collection. Handles cases with no batch data, a single batch,
        /// or multiple batches, formatting the subject accordingly.
        /// </summary>
        private string GetMailSubject()
        {
            string productName = OutputBatchTestResults.First().testResult.product.name;
            string batchNameSubject = string.Empty;
            var batches = OutputBatchTestResults.Where(x => x.testResult.testType == "BCH");

            if (batches.Count() == 0)
            {
                NotificationUtility.ShowWarning("No Batch Test Data");
            }

            if (batches.Count() == 1)
            {
                batchNameSubject = batches.FirstOrDefault().batch.batchName;
            }
            else
            {
                string firstBatchName = batches.FirstOrDefault().batch.batchName;

                if (firstBatchName.Contains("+"))
                {
                    firstBatchName = firstBatchName.Split('+')[0].Trim();
                }

                string lastBatchName = batches.LastOrDefault().batch.batchName;
                if (lastBatchName.Contains("+"))
                {
                    string yearMonth = lastBatchName.Substring(0, 3);
                    lastBatchName = $"{yearMonth}{lastBatchName.Split('+')[1].Trim()}";
                }

                batchNameSubject = $"{firstBatchName} - {lastBatchName}";
            }

            return $"{productName} {batchNameSubject}";
        }
    }
}
