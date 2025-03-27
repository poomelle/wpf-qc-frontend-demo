using ChemsonLabApp.MVVM.ViewModels.ReportVM;
using ChemsonLabApp.Services;
using ChemsonLabApp.Services.IService;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for NewReportView.xaml
    /// </summary>
    public partial class NewReportView : ContentControl
    {
        //public NewReportView(
        //    IProductService productService, 
        //    IEvaluationService evaluationService, 
        //    IBatchTestResultService batchTestResultService, 
        //    IReportService reportService,
        //    ITestResultReportService testResultReportService)
        //{
        //    InitializeComponent();
        //    DataContext = new NewReportViewModel(productService, evaluationService, batchTestResultService, reportService, testResultReportService);
        //}

        public NewReportView(NewReportViewModel newReportViewModel)
        {
            InitializeComponent();
            DataContext = newReportViewModel;
        }
    }
}
