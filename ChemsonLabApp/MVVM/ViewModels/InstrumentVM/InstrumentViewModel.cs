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

        public async Task GetAllInstrumentsAsync(string filter = "", string sort = "")
        {
            var instruments = await _instrumentService.GetAllInstrumentsAsync(filter, sort);
            Instruments.Clear();
            foreach (var instrument in instruments)
            {
                Instruments.Add(instrument);
            }
        }

        public async void UpdateInstrumentAsync(Instrument instrument)
        {
            var isUpdateSuccess = await _instrumentService.UpdateInstrumentAsync(instrument);
            if (isUpdateSuccess) NotificationUtility.ShowSuccess("Instrument updated successfully.");
            await GetAllInstrumentsAsync();
        }

        public void PopupAddInstrumentView()
        {
            _dialogService.ShowView("AddInstrument");
        }

        public void PopupDeleteInstrumentView(Instrument instrument)
        {
            _dialogService.ShowDeleteView(instrument);
        }

        public async void isVewModeToggle(Instrument instrument)
        {
            instrument.isViewMode = !instrument.isViewMode;
            instrument.isEditMode = !instrument.isEditMode;

            if (instrument.isViewMode)
            {
                await GetAllInstrumentsAsync();
            }
        }

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
