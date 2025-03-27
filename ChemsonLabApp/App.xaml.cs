using ChemsonLabApp.MVVM.ViewModels;
using ChemsonLabApp.MVVM.ViewModels.COAVM;
using ChemsonLabApp.MVVM.ViewModels.CustomerVM;
using ChemsonLabApp.MVVM.ViewModels.DailyQCVM;
using ChemsonLabApp.MVVM.ViewModels.DashboardVM;
using ChemsonLabApp.MVVM.ViewModels.FormulationPdfVM;
using ChemsonLabApp.MVVM.ViewModels.InstrumentVM;
using ChemsonLabApp.MVVM.ViewModels.NewDataLoaderVM;
using ChemsonLabApp.MVVM.ViewModels.ProductVM;
using ChemsonLabApp.MVVM.ViewModels.QCLabelVM;
using ChemsonLabApp.MVVM.ViewModels.ReportVM;
using ChemsonLabApp.MVVM.ViewModels.SpecificationVM;
using ChemsonLabApp.MVVM.Views;
using ChemsonLabApp.MVVM.Views.COA;
using ChemsonLabApp.MVVM.Views.Customer;
using ChemsonLabApp.MVVM.Views.DailyQC;
using ChemsonLabApp.MVVM.Views.Dashboard;
using ChemsonLabApp.MVVM.Views.DataLoader;
using ChemsonLabApp.MVVM.Views.FormulationPdf;
using ChemsonLabApp.MVVM.Views.Instrument;
using ChemsonLabApp.MVVM.Views.Product;
using ChemsonLabApp.MVVM.Views.QCLabel;
using ChemsonLabApp.MVVM.Views.Report;
using ChemsonLabApp.MVVM.Views.Specification;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.RestAPI.IRestAPI;
using ChemsonLabApp.Services;
using ChemsonLabApp.Services.COAService;
using ChemsonLabApp.Services.CustomerValidationService;
using ChemsonLabApp.Services.DataLoaderService;
using ChemsonLabApp.Services.DialogService;
using ChemsonLabApp.Services.EmailServices.IEmailService;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Windows.Devices.WiFiDirect.Services;

namespace ChemsonLabApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try
            {
                CreateAndSaveSettingFile();
                Constants.Constants.LoadSettingParameters();

                // Ensure dependency injection is configured before resolving services
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);
                _serviceProvider = serviceCollection.BuildServiceProvider();

                using (var scope = _serviceProvider.CreateScope())
                {
                    //var mainWindow = scope.ServiceProvider.GetRequiredService<MainWindowView>();
                    var mainWinsowViewModel = scope.ServiceProvider.GetRequiredService<MainWindowViewModel>();
                    var mainWindow = new MainWindowView(mainWinsowViewModel);

                    if (mainWindow == null)
                    {
                        NotificationUtility.ShowError("Failed to resolve MainWindowView from DI.");
                    }

                    mainWindow.Show();
                }

            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Failed to start application. Please try again later.");
                LoggerUtility.LogError(ex);
            }
        }

        private void CreateAndSaveSettingFile()
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string filePath = Path.Combine(userProfile, "LabAppSettingData.xml");

            // Check if the file already exists to avoid overwriting it
            if (!File.Exists(filePath))
            {
                // Create the XML content
                XDocument settingsDocument = new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XElement("Setting",
                        new XElement("IPAddress", "http://CP-LAB01/api"),
                        new XElement("COAPath", @"S:\\Lab\\Quality Certificates"),
                        new XElement("Username", "TS"),
                        new XElement("Email", "Tariq.Soni@akdenizchemson.com"),
                        new XElement("CompanyAddress", "Akdeniz Chemson Additives Pacific Pty. Ltd.  3/2 Capicure Drive, Eastern Creek NSW, 2766, Australia")
                    )
                );

                try
                {
                    // Save the XML to the specified path
                    settingsDocument.Save(filePath);
                }
                catch (Exception ex)
                {
                    // Handle potential errors, such as permission issues
                    Console.WriteLine("Failed to save settings: " + ex.Message);
                }
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register share Services
            services.AddScoped<IEmailService, Services.EmailServices.OutlookEmailService>();
            services.AddScoped<IDialogService, DialogService>();

            // Register API
            services.AddScoped<IBatchRestAPI, BatchRestAPI>();
            services.AddScoped<IBatchTestResultRestAPI, BatchTestResultRestAPI>();
            services.AddScoped<ICoaRestAPI, CoaRestAPI>();
            services.AddScoped<ICustomerOrderRestAPI, CustomerOrderRestAPI>();
            services.AddScoped<ICustomerRestAPI, CustomerRestAPI>();
            services.AddScoped<IDailyQcRestAPI, DailyQcRestAPI>();
            services.AddScoped<IEvaluationRestAPI, EvaluationRestAPI>();
            services.AddScoped<IInstrumentRestAPI, InstrumentRestAPI>();
            services.AddScoped<IMeasurementRestAPI, MeasurementRestAPI>();
            services.AddScoped<IProductRestAPI, ProductRestAPI>();
            services.AddScoped<IQcAveTimeKpiRestApi, QcAveTestTimeKpiRestApi>();
            services.AddScoped<IQcLabelRestAPI, QcLabelRestAPI>();
            services.AddScoped<IQcPerformanceKpiRestAPI, QcPerformanceKpiRestAPI>();
            services.AddScoped<IReportRestAPI, ReportRestAPI>();
            services.AddScoped<ISpecificationRestAPI, SpecificationRestAPI>();
            services.AddScoped<ITestResultReportRestAPI, TestResultReportRestAPI>();
            services.AddScoped<ITestResultRestAPI, TestResultRestAPI>();

            //////////////////////////////// Register services////////////////////////////////

            // Data Loader
            services.AddScoped<IDataLoaderService, DataLoaderService>();
            services.AddScoped<ISearchDataLoaderService, SearchDataLoaderService>();
            services.AddScoped<IDeleteDataLoaderService, DeleteDataLoaderService>();
            services.AddScoped<IEditDataLoaderService, EditDataLoaderService>();
            // COA
            services.AddScoped<ICoaService, CoaService>();
            services.AddScoped<IMakeCoaService, MakeCoaService>();
            // Customer
            services.AddScoped<ICustomerViewService, CustomerViewService>();
            services.AddScoped<ICustomerValidationService, CustomerValidationService>();
            services.AddScoped<ICustomerOrderService, CustomerOrderService>();
            // Daily QC
            services.AddScoped<IDailyQcService, DailyQcService>();
            // Instrument
            services.AddScoped<IInstrumentService, InstrumentService>();
            // Product
            services.AddScoped<IProductService, ProductService>();
            // Qc Label
            services.AddScoped<IQcLabelService, QcLabelService>();
            // Report
            services.AddScoped<IReportService, ReportService>();
            // Specification
            services.AddScoped<ISpecificationService, SpecificationService>();

            /////// Register ViewModel ///////
            services.AddScoped<MainWindowViewModel>();
            // COA
            services.AddScoped<COAViewModel>();
            services.AddScoped<MakeCOAViewModel>();
            // Customer
            services.AddScoped<CustomerOrderViewModel>();
            services.AddScoped<DeleteCustomerOrderViewModel>();
            services.AddScoped<AddCustomerOrderViewModel>();
            services.AddScoped<CustomerViewModel>();
            services.AddScoped<AddCustomerViewModel>();
            services.AddScoped<DeleteCustomerViewModel>();
            // Daily QC
            services.AddScoped<DailyQCViewModel>();
            // Dashboard
            services.AddScoped<DashboardViewModel>();
            services.AddScoped<ConformanceViewModel>();
            // FormulationPdf
            services.AddScoped<FormulationPdfViewModel>();
            // Instrument
            services.AddScoped<InstrumentViewModel>();
            services.AddScoped<AddInstrumentViewModel>();
            services.AddScoped<DeleteInstrumentViewModel>();
            // New Data Loader
            services.AddScoped<NewDataLoaderViewModel>();
            services.AddScoped<SearchDataLoaderViewModel>();
            services.AddScoped<DeleteDataLoaderViewModel>();
            services.AddScoped<EditDataLoaderViewModel>();
            // Product
            services.AddScoped<ProductViewModel>();
            services.AddScoped<AddProductViewModel>();
            services.AddScoped<DeleteProductViewModel>();
            // Qc Label
            services.AddScoped<QCLabelViewModel>();
            services.AddScoped<MakeQCLabelViewModel>();
            // Report
            services.AddScoped<NewReportViewModel>();
            services.AddScoped<MakeReportViewModel>();
            services.AddScoped<SearchReportViewModel>();
            services.AddScoped<DeleteReportViewModel>();
            services.AddScoped<MakeReportGraphViewModel>();
            services.AddScoped<OpenReportViewModel>();
            // Specification
            services.AddScoped<SpecificationViewModel>();
            services.AddScoped<AddSpecificationViewModel>();
            services.AddScoped<PrintSpecificationContentViewModel>();
            services.AddScoped<EditSpecificationViewModel>();
            services.AddScoped<DeleteSpecificationViewModel>();
            services.AddScoped<DisplaySpecificationContentControlViewModel>();
            services.AddScoped<EditSpecifiationContentControlView>();
            // MainWindow
            services.AddScoped<MainWindowViewModel>();
        }
    }
}
