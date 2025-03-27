using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.ReportVM.Command;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.ViewModels.ReportVM
{
    public class DeleteReportViewModel
    {
        private readonly IReportService _reportService;

        public TestResultReport TestResultReport { get; set; }
        public string DeleteConfirm { get; set; }
        public DeletedReportCommand DeletedReportCommand { get; set; }

        public DeleteReportViewModel(IReportService reportService)
        {
            DeletedReportCommand = new DeletedReportCommand(this);

            // services
            this._reportService = reportService;
        }

        public async void DeleteTestResultReport()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                var deletedReport = await _reportService.DeleteTestResultReport(TestResultReport, DeleteConfirm);
                if (deletedReport != null) NotificationUtility.ShowSuccess("Report has been deleted");
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Error: Failed to delete the report. Please check internet connection and try again later.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error: Failed to delete the report. Please try again later.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }
    }
}
