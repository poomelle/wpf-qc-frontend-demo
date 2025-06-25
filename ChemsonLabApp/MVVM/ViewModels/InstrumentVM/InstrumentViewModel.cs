using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Cache;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.InstrumentVM.Commands;
using ChemsonLabApp.MVVM.ViewModels.ProductVM.Commands;
using ChemsonLabApp.MVVM.Views.Instrument;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Services.DialogService;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.ViewModels.InstrumentVM
{
    [AddINotifyPropertyChangedInterface]
    public class InstrumentViewModel
    {
        private readonly IInstrumentService _instrumentService;
        private readonly IDialogService _dialogService;

        public ObservableCollection<Instrument> Instruments { get; set; } = new ObservableCollection<Instrument>();
        public List<string> StatusComboBox { get; set; } = new List<string>() { "All", "Active", "Inactive" };

        // Commands
        public ShowAddNewInsturmentView ShowAddNewInsturmentView { get; set; }
        public ReloadInstrumentsCommand ReloadInstrumentsCommand { get; set; }
        public ShowDeleteInstrumentView ShowDeleteInstrumentView { get; set; }
        public UpdateInstrumentCommand UpdateInstrumentCommand { get; set; }
        public IsViewToggleCommand IsViewToggleCommand { get; set; }
        public ComboBoxInstrumentSelectCommand ComboBoxInstrumentSelectCommand { get; set; }

        public InstrumentViewModel(IInstrumentService instrumentService, IDialogService dialogService)
        {
            // services
            this._instrumentService = instrumentService;
            this._dialogService = dialogService;

            // commands
            ShowAddNewInsturmentView = new ShowAddNewInsturmentView(this);
            ReloadInstrumentsCommand = new ReloadInstrumentsCommand(this);
            ShowDeleteInstrumentView = new ShowDeleteInstrumentView(this);
            UpdateInstrumentCommand = new UpdateInstrumentCommand(this);
            IsViewToggleCommand = new IsViewToggleCommand(this);
            ComboBoxInstrumentSelectCommand = new ComboBoxInstrumentSelectCommand(this);

            // initialize
            InitializeParameter();
        }

        /// <summary>
        /// Initializes the parameters for the InstrumentViewModel by loading all instruments asynchronously.
        /// Displays a loading cursor during the operation and handles any exceptions by showing appropriate error notifications.
        /// </summary>
        public async void InitializeParameter()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                await GetAllInstrumentsAsync();
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Failed to load instruments. Please check your internet connection and try again.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("An error occurred while loading instruments. Please try again.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }

        /// <summary>
        /// Asynchronously retrieves all instruments from the instrument service using optional filter and sort parameters.
        /// Clears the current Instruments collection and repopulates it with the retrieved instruments.
        /// </summary>
        /// <param name="filter">Optional filter string to apply to the instrument query.</param>
        /// <param name="sort">Optional sort string to order the instrument results.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task GetAllInstrumentsAsync(string filter = "", string sort = "")
        {
            var instruments = await _instrumentService.GetAllInstrumentsAsync(filter, sort);
            Instruments.Clear();
            foreach (var instrument in instruments)
            {
                Instruments.Add(instrument);
            }
        }

        /// <summary>
        /// Asynchronously updates the specified instrument using the instrument service.
        /// Shows a success notification if the update is successful and refreshes the Instruments collection.
        /// </summary>
        /// <param name="instrument">The instrument to update.</param>
        public async void UpdateInstrumentAsync(Instrument instrument)
        {
            var isUpdateSuccess = await _instrumentService.UpdateInstrumentAsync(instrument);
            if (isUpdateSuccess) NotificationUtility.ShowSuccess("Instrument updated successfully.");
            await GetAllInstrumentsAsync();
        }

        /// <summary>
        /// Displays the Add Instrument dialog view using the dialog service.
        /// </summary>
        public void PopupAddInstrumentView()
        {
            _dialogService.ShowView("AddInstrument");
        }

        /// <summary>
        /// Displays the Delete Instrument dialog view for the specified instrument using the dialog service.
        /// </summary>
        public void PopupDeleteInstrumentView(Instrument instrument)
        {
            _dialogService.ShowDeleteView(instrument);
        }

        /// <summary>
        /// Toggles the view and edit mode states for the specified instrument.
        /// If toggled to view mode, refreshes the Instruments collection asynchronously.
        /// </summary>
        /// <param name="instrument">The instrument whose mode is to be toggled.</param>
        public async void isVewModeToggle(Instrument instrument)
        {
            instrument.isViewMode = !instrument.isViewMode;
            instrument.isEditMode = !instrument.isEditMode;

            if (instrument.isViewMode)
            {
                await GetAllInstrumentsAsync();
            }
        }

        /// <summary>
        /// Filters the Instruments collection based on the selected instrument and status.
        /// Constructs a filter string using the selected instrument's name and the specified status,
        /// then asynchronously retrieves and updates the Instruments collection with the filtered results.
        /// </summary>
        /// <param name="selectedInstrument">The instrument selected for filtering, or null to ignore this filter.</param>
        /// <param name="status">The status to filter by ("All", "Active", "Inactive", or empty for no filter).</param>
        public async void ComboBoxInstrumentFilter(Instrument selectedInstrument = null, string status = "")
        {
            //SelectedInstrument = selectedInstrument;
            //ComboBoxSelectedStatus = status;

            string instrumentFilter = selectedInstrument == null ? "" : selectedInstrument.name;
            string statusfilter = status == "" || status == "All" ? "" : status == "Active" ? "true" : "false";

            var filter = $"?name={instrumentFilter}&status={statusfilter}";
            await GetAllInstrumentsAsync(filter);
        }
    }
}
