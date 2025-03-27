using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.DialogService
{
    public interface IDialogService
    {
        // Common Delete Views
        void ShowDeleteView<T>(T item) where T : class;

        // Common Add Views
        void ShowView(string viewName);

        // QcLabel
        void ShowMakeQcLabels(List<QCLabel> qcLabels);

        // Report
        void ShowMakeReportView(List<BatchTestResult> batchTestResults);
        void ShowOpenReportView(string fileLocation);
        Task ShowMakeReportGraphView(TestResultReport testResultReport);

        // Specification
        void ShowEditSpecificationView(Specification specification);

        // Data Loader
        void ShowEditDataLoaderView(BatchTestResult batchTestResult);
        void ShowDeleteDataLoaderView(List<BatchTestResult> batchTestResults);

        // COA
        void ShowMakeCoaView(List<TestResultReport> resultReports, string poNumber);
    }
}
