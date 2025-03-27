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
    /// Interaction logic for SearchReportView.xaml
    /// </summary>
    public partial class SearchReportView : ContentControl
    {
        //public SearchReportView(
        //    IProductService productService,
        //    ITestResultReportService testResultReportService,
        //    IMeasurementService measurementService,
        //    IEvaluationService evaluationService,
        //    IBatchTestResultService batchTestResultService,
        //    ISpecificationService specificationService
        //    )
        //{
        //    InitializeComponent();
        //    DataContext = new SearchReportViewModel(productService, testResultReportService, measurementService, evaluationService, batchTestResultService, specificationService);
        //}

        public SearchReportView(SearchReportViewModel searchReportViewModel)
        {
            InitializeComponent();
            DataContext = searchReportViewModel;
        }
    }
}
