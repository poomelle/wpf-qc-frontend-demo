using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.COAVM;
using ChemsonLabApp.MVVM.ViewModels.CustomerVM;
using ChemsonLabApp.MVVM.ViewModels.InstrumentVM;
using ChemsonLabApp.MVVM.ViewModels.NewDataLoaderVM;
using ChemsonLabApp.MVVM.ViewModels.ProductVM;
using ChemsonLabApp.MVVM.ViewModels.QCLabelVM;
using ChemsonLabApp.MVVM.ViewModels.ReportVM;
using ChemsonLabApp.MVVM.ViewModels.SpecificationVM;
using ChemsonLabApp.MVVM.Views.COA;
using ChemsonLabApp.MVVM.Views.Customer;
using ChemsonLabApp.MVVM.Views.DataLoader;
using ChemsonLabApp.MVVM.Views.Instrument;
using ChemsonLabApp.MVVM.Views.Product;
using ChemsonLabApp.MVVM.Views.QCLabel;
using ChemsonLabApp.MVVM.Views.Report;
using ChemsonLabApp.MVVM.Views.Specification;
using ChemsonLabApp.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Services.DialogService
{
    public class DialogService : IDialogService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DialogService(IServiceScopeFactory serviceScopeFactory)
        {
            this._serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Displays a dialog window based on the specified view name.
        /// Supported view names: "AddCustomer", "AddCustomerOrder", "AddInstrument", "AddProduct", "AddSpecification", "PrintSpecification".
        /// Shows an error notification if the view name is invalid.
        /// </summary>
        /// <param name="viewName">The name of the view to display.</param>
        public void ShowView(string viewName)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                switch (viewName)
                {
                    case "AddCustomer":
                        var addCustomerViewModel = scope.ServiceProvider.GetRequiredService<AddCustomerViewModel>();
                        var addCustomerView = new AddCustomerView(addCustomerViewModel);
                        addCustomerView.ShowDialog();
                        break;
                    case "AddCustomerOrder":
                        var addCustomerOrderViewModel = scope.ServiceProvider.GetRequiredService<AddCustomerOrderViewModel>();
                        var addCustomerOrderView = new AddCustomerOrderView(addCustomerOrderViewModel);
                        addCustomerOrderView.ShowDialog();
                        break;
                    case "AddInstrument":
                        var addInstrumentViewModel = scope.ServiceProvider.GetRequiredService<AddInstrumentViewModel>();
                        var addInstrumentView = new AddInstrumentView(addInstrumentViewModel);
                        addInstrumentView.ShowDialog();
                        break;
                    case "AddProduct":
                        var addProductViewModel = scope.ServiceProvider.GetRequiredService<AddProductViewModel>();
                        var addProductView = new AddProductView(addProductViewModel);
                        addProductView.ShowDialog();
                        break;
                    case "AddSpecification":
                        var addSpecificationViewModel = scope.ServiceProvider.GetRequiredService<AddSpecificationViewModel>();
                        var addSpecificationView = new AddSpecificationView(addSpecificationViewModel);
                        addSpecificationView.ShowDialog();
                        break;
                    case "PrintSpecification":
                        var printSpecificationContentViewModel = scope.ServiceProvider.GetRequiredService<PrintSpecificationContentViewModel>();
                        var printSpecificationView = new PrintSpecificationContentView(printSpecificationContentViewModel);
                        printSpecificationView.ShowDialog();
                        break;
                    default:
                        NotificationUtility.ShowError("Invalid view name.");
                        break;
                }
            }
        }

        /// <summary>
        /// Displays a delete confirmation dialog for the specified item type.
        /// Supported types: Customer, CustomerOrder, Instrument, Product, TestResultReport, Specification.
        /// Shows an error notification if the item type is invalid.
        /// </summary>
        /// <typeparam name="T">The type of the item to delete.</typeparam>
        /// <param name="item">The item instance to delete.</param>
        public void ShowDeleteView<T>(T item) where T : class
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                switch (item)
                {
                    case Customer customer:
                        var deleteCustomerViewModel = scope.ServiceProvider.GetRequiredService<DeleteCustomerViewModel>();
                        deleteCustomerViewModel.Customer = customer;
                        var deleteCustomerView = new DeleteCustomerView(deleteCustomerViewModel);
                        deleteCustomerView.ShowDialog();
                        break;
                    case CustomerOrder customerOrder:
                        var deleteCustomerOrderViewModel = scope.ServiceProvider.GetRequiredService<DeleteCustomerOrderViewModel>();
                        deleteCustomerOrderViewModel.CustomerOrder = customerOrder;
                        var deleteCustomerOrderView = new DeleteCustomerOrderView(deleteCustomerOrderViewModel);
                        deleteCustomerOrderView.ShowDialog();
                        break;
                    case Instrument instrument:
                        var deleteInstrumentViewModel = scope.ServiceProvider.GetRequiredService<DeleteInstrumentViewModel>();
                        deleteInstrumentViewModel.Instrument = instrument;
                        var deleteInstrumentView = new DeleteInstrumentView(deleteInstrumentViewModel);
                        deleteInstrumentView.ShowDialog();
                        break;
                    case Product product:
                        var deleteProductViewModel = scope.ServiceProvider.GetRequiredService<DeleteProductViewModel>();
                        deleteProductViewModel.Product = product;
                        var deleteProductView = new DeleteProductView(deleteProductViewModel);
                        deleteProductView.ShowDialog();
                        break;
                    case TestResultReport testResultReport:
                        var deleteReportViewModel = scope.ServiceProvider.GetRequiredService<DeleteReportViewModel>();
                        deleteReportViewModel.TestResultReport = testResultReport;
                        var deleteReportView = new DeleteReportView(deleteReportViewModel);
                        deleteReportView.ShowDialog();
                        break;
                    case Specification specification:
                        var deleteSpecificationViewModel = scope.ServiceProvider.GetRequiredService<DeleteSpecificationViewModel>();
                        deleteSpecificationViewModel.Specification = specification;
                        var deleteSpecificationView = new DeleteSpecificationView(deleteSpecificationViewModel);
                        deleteSpecificationView.ShowDialog();
                        break;
                    default:
                        NotificationUtility.ShowError("Invalid item type.");
                        break;
                }
            }
        }

        /// <summary>
        /// Displays a dialog to confirm deletion of the specified batch test results.
        /// </summary>
        /// <param name="batchTestResults">The list of batch test results to delete.</param>
        public void ShowDeleteDataLoaderView(List<BatchTestResult> batchTestResults)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var deleteDataLoaderViewModel = scope.ServiceProvider.GetRequiredService<DeleteDataLoaderViewModel>();
                deleteDataLoaderViewModel.BatchTestResults = batchTestResults;

                var deleteDataLoaderView = new DeleteDataLoaderView(deleteDataLoaderViewModel);
                deleteDataLoaderView.ShowDialog();
            }
        }

        /// <summary>
        /// Displays a dialog to create QC labels for the provided list of QCLabel objects.
        /// Shows an error notification if the list is empty.
        /// </summary>
        /// <param name="qcLabels">The list of QC labels to create.</param>
        public void ShowMakeQcLabels(List<QCLabel> qcLabels)
        {
            if (qcLabels.Count() == 0)
            {
                NotificationUtility.ShowError("No QC Labels to make.");
                return;
            }

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var makeQCLabelViewModel = scope.ServiceProvider.GetRequiredService<MakeQCLabelViewModel>();
                makeQCLabelViewModel.QCLabels = qcLabels;
                makeQCLabelViewModel.CreateQCLabelsPage();

                var makeQCLabelView = new MakeQCLabelView(makeQCLabelViewModel);
                makeQCLabelView.ShowDialog();
            }
        }

        /// <summary>
        /// Displays a dialog to create a report for the provided list of batch test results.
        /// Shows an error notification if the list is empty.
        /// </summary>
        /// <param name="batchTestResults">The list of batch test results to include in the report.</param>
        public void ShowMakeReportView(List<BatchTestResult> batchTestResults)
        {
            if (batchTestResults.Count() == 0)
            {
                NotificationUtility.ShowError("No Test Result to make.");
                return;
            }

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var makeReportViewModel = scope.ServiceProvider.GetRequiredService<MakeReportViewModel>();
                makeReportViewModel.BatchTestResults = batchTestResults;
                makeReportViewModel.CalculateAveTestTimeTick(batchTestResults);
                makeReportViewModel.InitializeParameter(batchTestResults);

                var makeReportView = new MakeReportView(makeReportViewModel);
                makeReportView.ShowDialog();
            }
        }

        /// <summary>
        /// Displays a dialog to create a Certificate of Analysis (COA) for the provided test result reports and PO number.
        /// </summary>
        /// <param name="resultReports">The list of test result reports to include in the COA.</param>
        /// <param name="poNumber">The purchase order number associated with the COA.</param>
        public void ShowMakeCoaView(List<TestResultReport> resultReports, string poNumber)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var makeCOAViewModel = scope.ServiceProvider.GetRequiredService<MakeCOAViewModel>();
                makeCOAViewModel.TestResultReports = resultReports;
                makeCOAViewModel.PONumber = poNumber;

                makeCOAViewModel.InitializeParameter();
                makeCOAViewModel.ShowingCOAReport();

                var makeCOAView = new MakeCOAView(makeCOAViewModel);
                makeCOAView.ShowDialog();
            }
        }

        /// <summary>
        /// Displays a dialog to open a report view for the specified file location.
        /// </summary>
        /// <param name="fileLocation">The file path of the report to open.</param>
        public void ShowOpenReportView(string fileLocation)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var openReportViewModel = scope.ServiceProvider.GetRequiredService<OpenReportViewModel>();
                openReportViewModel.FilePath = fileLocation;

                var openReportView = new OpenReportView(openReportViewModel);
                openReportView.ShowDialog();
            }
        }

        /// <summary>
        /// Asynchronously displays a dialog to generate and show a report graph for the specified test result report.
        /// </summary>
        /// <param name="testResultReport">The test result report to generate the graph for.</param>
        public async Task ShowMakeReportGraphView(TestResultReport testResultReport)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var makeReportGraphViewModel = scope.ServiceProvider.GetRequiredService<MakeReportGraphViewModel>();
                makeReportGraphViewModel.TestResultReport = testResultReport;
                makeReportGraphViewModel.ProductName = testResultReport.batchTestResult.testResult.product.name;
                makeReportGraphViewModel.BatchName = testResultReport.batchTestResult.batch.batchName;

                await makeReportGraphViewModel.GenerateTorqueGraph();

                var makeReportGraphView = new MakeReportGraphView(makeReportGraphViewModel);
                makeReportGraphView.ShowDialog();
            }
        }

        /// <summary>
        /// Displays a dialog to view and edit the specified specification in view mode.
        /// </summary>
        /// <param name="specification">The specification to view or edit.</param>
        public void ShowEditSpecificationView(Specification specification)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var editSpecificationViewModel = scope.ServiceProvider.GetRequiredService<EditSpecificationViewModel>();
                editSpecificationViewModel.Specification = specification;
                editSpecificationViewModel.Specification.isEditMode = false;
                editSpecificationViewModel.Specification.isViewMode = true;

                var displaySpecificationContentControlViewModel = scope.ServiceProvider.GetRequiredService<DisplaySpecificationContentControlViewModel>();
                displaySpecificationContentControlViewModel.GetSpecificationById(specification.id);

                editSpecificationViewModel.SelectedMode = new DisplaySpecificationContentControlView(displaySpecificationContentControlViewModel);

                var editSpecificationView = new EditSpecificationView(editSpecificationViewModel);
                editSpecificationView.ShowDialog();
            }
        }

        /// <summary>
        /// Displays a dialog to edit the specified batch test result in the data loader view.
        /// </summary>
        /// <param name="batchTestResult">The batch test result to edit.</param>
        public void ShowEditDataLoaderView(BatchTestResult batchTestResult)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var editDataLoaderViewModel = scope.ServiceProvider.GetRequiredService<EditDataLoaderViewModel>();
                editDataLoaderViewModel.BatchTestResult = batchTestResult;
                editDataLoaderViewModel.InitializeParameters();

                var editDataLoaderView = new EditDataLoaderView(editDataLoaderViewModel);
                editDataLoaderView.ShowDialog();
            }
        }
    }
}
