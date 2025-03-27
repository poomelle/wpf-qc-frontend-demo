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

        private void CreateOutlookEmailWithImage(string imagePath, string fromAddress)
        {
            string mailSubject = GetMailSubject();
            _emailService.CreateReportEmailAndOpenOutlook(mailSubject, imagePath, fromAddress);
        }

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
