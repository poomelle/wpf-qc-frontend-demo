using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.DailyQCVM.Command;
using ChemsonLabApp.MVVM.ViewModels.SpecificationVM;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.VariantTypes;
using PropertyChanged;
using Windows.Networking.NetworkOperators;
using Windows.System.UserProfile;

namespace ChemsonLabApp.MVVM.ViewModels.DailyQCVM
{
    [AddINotifyPropertyChangedInterface]
    public class DailyQCViewModel
    {
        public ObservableCollection<DailyQc> DailyQcs { get; set; } = new ObservableCollection<DailyQc>();

        // User input: Filter combo box Items Sources and Selected Items
        public List<string> TestStatuses { get; set; } = new List<string>();
        public List<string> DataTableTestStatuses { get; set; } = new List<string>();
        public string SelectedTestStatus { get; set; }
        public List<string> DataTableProductsName { get; set; } = new List<string>();
        public string SelectedProduct { get; set; }
        public List<string> Years { get; set; } = new List<string>();
        public string SelectedYear { get; set; }
        public List<string> Months { get; set; } = new List<string>();
        public string SelectedMonth { get; set; }
        public List<string> StdReqdComboBox { get; set; }
        public List<int> PriorityComboBox { get; set; }
        public List<string> ExtraComboBox { get; set; }
        public List<string> CommentComboBox { get; set; }
        public bool IsAllSelected { get; set; }
        public bool IsOnlyCOARequired { get; set; } = false;
        public DailyQc SelectedDailyQc { get; set; }

        // services
        private readonly IDailyQcService _dailyQcService;
        private readonly IProductService _productService;

        //Commands
        public SaveDataDailyQcCommand SaveDataDailyQcCommand { get; set; }
        public DeleteDailyQcCommand DeleteDailyQcCommand { get; set; }
        public TriggerSelectAllDailyQcsCommand TriggerSelectAllDailyQcsCommand { get; set; }
        public SearchDataDailyQcCommand SearchDataDailyQcCommand { get; set; }
        public LoadLastBatchTestCommand LoadLastBatchTestCommand { get; set; }
        public ProductSelectionChangedCommand ProductSelectionChangedCommand { get; set; }
        public ProductSelectionSearchCommand ProductSelectionSearchCommand { get; set; }
        public SelectTestStatusChangeCommand SelectTestStatusChangeCommand { get; set; }

        public DailyQCViewModel(
            IDailyQcService dailyQcService,
            IProductService productService)
        {
            // services
            this._dailyQcService = dailyQcService;
            this._productService = productService;

            // commands
            SaveDataDailyQcCommand = new SaveDataDailyQcCommand(this);
            DeleteDailyQcCommand = new DeleteDailyQcCommand(this);
            TriggerSelectAllDailyQcsCommand = new TriggerSelectAllDailyQcsCommand(this);
            SearchDataDailyQcCommand = new SearchDataDailyQcCommand(this);
            LoadLastBatchTestCommand = new LoadLastBatchTestCommand(this);
            ProductSelectionChangedCommand = new ProductSelectionChangedCommand(this);
            ProductSelectionSearchCommand = new ProductSelectionSearchCommand(this);
            SelectTestStatusChangeCommand = new SelectTestStatusChangeCommand(this);


            // Initialize default parameters
            InitializeDefaultParameters();
            InitializeDailyQcs();
        }

        /// <summary>
        /// Initializes the default parameters for the DailyQCViewModel, including filter combo box items,
        /// default selected values, and other UI-related lists such as years, months, priorities, and comments.
        /// </summary>
        private void InitializeDefaultParameters()
        {
            TestStatuses = new List<string> { "All", "In Process", "Yes", "No" };
            DataTableTestStatuses = new List<string> { "In Process", "Yes", "No" };
            SelectedTestStatus = "In Process";

            // Add all years from start year to current year in Years
            Years = Enumerable.Range(2022, DateTime.Now.Year - 2022 + 1).Select(y => y.ToString()).ToList();
            SelectedYear = DateTime.Now.Year.ToString(); ;

            // Add Months selection
            Months = Constants.Constants.Months;
            SelectedMonth = Months[DateTime.Now.Month];

            StdReqdComboBox = new List<string> { "DPC", "" };

            PriorityComboBox = new List<int> { 0, 1, 2, 3, 4 };

            ExtraComboBox = new List<string> { "NEW", "" };

            CommentComboBox = new List<string> { "await samples", "OKs printed", "" };
        }

        /// <summary>
        /// Loads and initializes the DailyQcs collection for the dashboard view.
        /// Fetches daily QC data for the selected product, year, month, and test status
        /// from the service, populates the observable collection, and handles UI cursor state.
        /// Displays error notifications if data loading fails.
        /// </summary>
        public async void InitializeDailyQcs()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                var dailyQcs = await _dailyQcService.LoadTodayDashboardDailyQcAsync(SelectedProduct, SelectedYear, SelectedMonth, SelectedTestStatus);
                PopulateDailyQcsData(dailyQcs);

                //await LoadLastQcLabelAndTestedBatchAndLastCoa(dailyQcs);
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Failed to connect to server. Please check your internet connection and try again.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("An error occurred while loading data. Please try again.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        /// <summary>
        /// Searches for DailyQc records based on the selected product, year, month, and test status.
        /// Retrieves the filtered DailyQc list from the service, populates additional COA, label, and batch information,
        /// and updates the observable collection for display. Handles and logs any errors that occur during the process.
        /// </summary>
        public async void SearchDataDailyQc()
        {
            try
            {
                var dailyQcs = await _dailyQcService.GetDailyQcs(SelectedProduct, SelectedYear, SelectedMonth, SelectedTestStatus);
                var dailyQcsWithInfo = await _dailyQcService.PopulateCOALabelBatchMixReqd(dailyQcs);
                PopulateDailyQcsData(dailyQcsWithInfo);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("An error occurred while searching data. Please try again.");
                LoggerUtility.LogError(ex);
            }
        }

        /// <summary>
        /// Populates the DailyQcs observable collection with the provided list of DailyQc objects.
        /// Orders the list based on the selected test status (by incoming date if "All", otherwise by priority).
        /// Optionally filters the list to include only products that require a COA if IsOnlyCOARequired is true.
        /// Sets the productName property for each DailyQc and adds it to the collection for UI binding.
        /// </summary>
        private void PopulateDailyQcsData(List<DailyQc> dailyQcs)
        {
            DailyQcs.Clear();
            // Add dailyQcs to observable collection
            if (dailyQcs != null || dailyQcs.Count != 0)
            {
                // if selected status is "All" then order by incoming date
                if (SelectedTestStatus == "All")
                {
                    dailyQcs = dailyQcs.OrderBy(dailyQc => dailyQc.incomingDate).ToList();
                }
                else
                {
                    dailyQcs = dailyQcs.OrderBy(dailyQc => dailyQc.priority).ToList();
                }

                // filter coa
                if (IsOnlyCOARequired)
                {
                    dailyQcs = dailyQcs.Where(dq => dq.product.coa == true).ToList();
                }

                // order by priority

                foreach (var dailyQc in dailyQcs)
                {
                    dailyQc.productName = dailyQc.product.name;
                    DailyQcs.Add(dailyQc);
                }
            }
        }

        /// <summary>
        /// Populates the last tested batch information for the specified DailyQc object.
        /// Retrieves the last batch value asynchronously from the daily QC service and updates the DailyQc object.
        /// Sets the isLastBatchLoaded flag to true if a valid last batch value is retrieved.
        /// </summary>
        private async Task PopulateLastTestedBatch(DailyQc dailyQc)
        {
            dailyQc.lastBatch = await _dailyQcService.GetLastTest(dailyQc);
            if (!string.IsNullOrWhiteSpace(dailyQc.lastBatch))
            {
                dailyQc.isLastBatchLoaded = true;
            }
        }

        /// <summary>
        /// Populates the SelectedDailyQc object with product information for a new DailyQc entry.
        /// Sets the product name, retrieves the corresponding Product object and its ID, sets the test status to "In Process",
        /// and loads the last tested batch information for the selected product.
        /// Handles and logs any exceptions that occur during the process.
        /// </summary>
        public async void PopulateNewDailyQc(string productName)
        {
            try
            {
                SelectedDailyQc.productName = productName;
                SelectedDailyQc.product = await _productService.GetProductFromProductName(productName);
                SelectedDailyQc.productId = SelectedDailyQc.product.id;
                SelectedDailyQc.testStatus = "In Process";

                await PopulateLastTestedBatch(SelectedDailyQc);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("An error occurred while populating new input dailyQc. Please try again.");
                LoggerUtility.LogError(ex);
            }
        }

        /// <summary>
        /// Saves all DailyQc records in the DailyQcs collection to the database.
        /// Displays a success notification upon successful save, or an error notification if an exception occurs.
        /// After saving, reinitializes the DailyQcs collection to reflect the latest data.
        /// </summary>
        public async void SaveDailyQcsToDatabase()
        {
            try
            {
                await _dailyQcService.SaveAllDailyQcs(DailyQcs.ToList());
                NotificationUtility.ShowSuccess("Successfully saved data.");
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("An error occurred while saving data. Please try again.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                InitializeDailyQcs();
            }
        }

        /// <summary>
        /// Deletes the selected DailyQc records from the database.
        /// Prompts the user for confirmation before deletion. If confirmed, deletes all DailyQc items
        /// in the DailyQcs collection that are marked as selected. Displays a success notification if deletion succeeds,
        /// or an error notification if an exception occurs. After deletion, reinitializes the DailyQcs collection
        /// to reflect the latest data.
        /// </summary>
        public async void DeleteDailyQc()
        {
            try
            {
                if (!NotificationUtility.ShowConfirmation("Are you sure you want to delete selected dailyQcs?")) return;
                var dailyQcsToDelete = DailyQcs.Where(dq => dq.isSelected).ToList();
                var isDeleted = await _dailyQcService.DeleteDailyQcAsync(dailyQcsToDelete);
                if (isDeleted) NotificationUtility.ShowSuccess("Selected dailyQcs have been deleted successfully.");
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("An error occurred while deleting data. Please try again.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                InitializeDailyQcs();
            }
        }

        /// <summary>
        /// Selects or deselects all DailyQc items in the DailyQcs collection based on the IsAllSelected property.
        /// When IsAllSelected is true, all items are marked as selected; otherwise, all are deselected.
        /// </summary>
        public void TriggerSelectAllDailyQcs()
        {
            foreach (var dailyQc in DailyQcs)
            {
                dailyQc.isSelected = IsAllSelected;
            }
        }
    }
}
