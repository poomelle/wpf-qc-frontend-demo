using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using ChemsonLabApp.Constants;
using System.Windows.Controls;
using ChemsonLabApp.MVVM.Views.Home;
using ChemsonLabApp.MVVM.Views.DataLoader;
using ChemsonLabApp.MVVM.Views.Report;
using ChemsonLabApp.MVVM.Views.COA;
using ChemsonLabApp.MVVM.Views.Dashboard;
using ChemsonLabApp.MVVM.Views.Product;
using ChemsonLabApp.MVVM.Views.Specification;
using ChemsonLabApp.MVVM.Views.Customer;
using ChemsonLabApp.MVVM.Views.Setting;
using ChemsonLabApp.MVVM.Views.Instrument;
using System.Windows.Data;
using ChemsonLabApp.MVVM.ViewModels.MainWindow.Commands;
using ChemsonLabApp.MVVM.Views.QCLabel;
using ChemsonLabApp.MVVM.Views.DailyQC;
using ChemsonLabApp.MVVM.Views.FormulationPdf;
using ChemsonLabApp.Services.IService;
using Microsoft.Extensions.DependencyInjection;
using ChemsonLabApp.MVVM.ViewModels.ReportVM;
using ChemsonLabApp.MVVM.ViewModels.COAVM;
using ChemsonLabApp.MVVM.ViewModels.CustomerVM;
using ChemsonLabApp.MVVM.ViewModels.DailyQCVM;
using ChemsonLabApp.MVVM.ViewModels.DashboardVM;
using ChemsonLabApp.MVVM.ViewModels.FormulationPdfVM;
using ChemsonLabApp.MVVM.ViewModels.InstrumentVM;
using ChemsonLabApp.MVVM.ViewModels.NewDataLoaderVM;
using ChemsonLabApp.MVVM.ViewModels.ProductVM;
using ChemsonLabApp.MVVM.ViewModels.QCLabelVM;
using ChemsonLabApp.MVVM.ViewModels.SpecificationVM;

namespace ChemsonLabApp.MVVM.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class MainWindowViewModel
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ObservableCollection<Models.Menu> MenuItems { get; set; } = new ObservableCollection<Models.Menu>();
        public string HeaderTitle { get; set; }
        public ContentControl SelectedContent { get; set; }
        public MenuSelectionCommand MenuSelectionCommand { get; set; }

        public MainWindowViewModel(IServiceScopeFactory serviceScopeFactory)
        {
            MenuSelectionCommand = new MenuSelectionCommand(this);
            MenuItems = MainMenuItems.Menus;
            HeaderTitle = "Daily QC";

            this._serviceScopeFactory = serviceScopeFactory;

            MenuSelected(HeaderTitle);
        }

        public void MenuSelected(string menuTitle)
        {
            HeaderTitle = menuTitle;
            switch (menuTitle)
            {
                case "Daily QC":
                    using(var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dailyQCViewModel = scope.ServiceProvider.GetRequiredService<DailyQCViewModel>();
                        SelectedContent = new DailyQCView(dailyQCViewModel);
                    }
                    break;
                case "New Data Loader":
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var newDataLoaderViewModel = scope.ServiceProvider.GetRequiredService<NewDataLoaderViewModel>();
                        SelectedContent = new NewDataLoaderView(newDataLoaderViewModel);
                    }
                    break;
                case "Search Data Loader":
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var searchDataLoaderViewModel = scope.ServiceProvider.GetRequiredService<SearchDataLoaderViewModel>();
                        SelectedContent = new SearchDataLoaderView(searchDataLoaderViewModel);
                    }
                    break;
                case "New Report":
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var newReportViewModel = scope.ServiceProvider.GetRequiredService<NewReportViewModel>();
                        SelectedContent = new NewReportView(newReportViewModel);
                    }
                    break;
                case "Search Report":
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var searchReportViewModel = scope.ServiceProvider.GetRequiredService<SearchReportViewModel>();
                        SelectedContent = new SearchReportView(searchReportViewModel);
                    }
                    break;
                case "COA":
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var coaViewModel = scope.ServiceProvider.GetRequiredService<COAViewModel>();
                        SelectedContent = new COAView(coaViewModel);
                    }
                    break;
                case "Dashboard":
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dashboardViewModel = scope.ServiceProvider.GetRequiredService<DashboardViewModel>();
                        SelectedContent = new DashboardView(dashboardViewModel);
                    }
                    break;
                case "Product":
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var productViewModel = scope.ServiceProvider.GetRequiredService<ProductViewModel>();
                        SelectedContent = new ProductView(productViewModel);
                    }
                    break;
                case "Specification":
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var specificationViewModel = scope.ServiceProvider.GetRequiredService<SpecificationViewModel>();
                        SelectedContent = new SpecificationView(specificationViewModel);
                    }
                    break;
                case "Customer Order":
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var customerOrderViewModel = scope.ServiceProvider.GetRequiredService<CustomerOrderViewModel>();
                        SelectedContent = new CustomerOrderView(customerOrderViewModel);
                    }
                    break;
                case "Customer":
                    using(var scope = _serviceScopeFactory.CreateScope())
                    {
                        var customerViewModel = scope.ServiceProvider.GetRequiredService<CustomerViewModel>();
                        SelectedContent = new CustomerView(customerViewModel);
                    }
                    break;
                case "Setting":
                    SelectedContent = new SettingView();
                    break;
                case "Instrument":
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var instrumentViewModel = scope.ServiceProvider.GetRequiredService<InstrumentViewModel>();
                        SelectedContent = new InstrumentView(instrumentViewModel);
                    }
                    break;
                case "QC Label":
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var qCLabelViewModel = scope.ServiceProvider.GetRequiredService<QCLabelViewModel>();
                        SelectedContent = new QCLabelView(qCLabelViewModel);
                    }
                    break;
                case "PDF":
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var formulationPdfViewModel = scope.ServiceProvider.GetRequiredService<FormulationPdfViewModel>();
                        SelectedContent = new FormulationPdfView(formulationPdfViewModel);

                    }
                    break;
            }
        }

        public void ShowHideSubMenu(Models.Menu selectedMenu)
        {
            if (selectedMenu.IsSubmenuVisible == true)
            {
                HideAllSubMenu();
            }
            else
            {
                HideAllSubMenu();
                selectedMenu.IsSubmenuVisible = true;
            }
        }

        public void HideAllSubMenu()
        {
            foreach (Models.Menu menu in MenuItems)
            {
                menu.IsSubmenuVisible = false;
            }
        }
    }
}
