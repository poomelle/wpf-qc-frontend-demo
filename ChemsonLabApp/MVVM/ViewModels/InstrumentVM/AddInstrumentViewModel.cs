using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.InstrumentVM.Commands;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PropertyChanged;
using ChemsonLabApp.Services.IService;
using System.Net.Http;

namespace ChemsonLabApp.MVVM.ViewModels.InstrumentVM
{
    [AddINotifyPropertyChangedInterface]
    public class AddInstrumentViewModel
    {
        private readonly IInstrumentService _instrumentService;

        public string InstrumentName { get; set; }
        public SaveNewInstrumentCommand SaveNewInstrumentCommand { get; set; }

        public AddInstrumentViewModel(IInstrumentService instrumentService)
        {
            // Services
            this._instrumentService = instrumentService;

            // Commands
            SaveNewInstrumentCommand = new SaveNewInstrumentCommand(this);
        }

        /// <summary>
        /// Adds a new instrument by calling the instrument service. 
        /// Displays a loading cursor during the operation, shows a success notification if the instrument is created, 
        /// and handles errors by displaying appropriate error messages and logging them.
        /// </summary>
        public async void AddNewInstrument()
        {
            CursorUtility.DisplayCursor(true);
            try
            {
                var isNewInstrumentCreated = await _instrumentService.CreateInstrumentAsync(InstrumentName);
                if (isNewInstrumentCreated) NotificationUtility.ShowSuccess("New Instrument has been saved.");
            }
            catch (HttpRequestException ex)
            {
                NotificationUtility.ShowError("Error : Internet Connection error. Please check your internet connection and try again.");
                LoggerUtility.LogError(ex);
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("Error : Failed to save new instrument. Please check your internet connection and try again.");
                LoggerUtility.LogError(ex);
            }
            finally
            {
                CursorUtility.DisplayCursor(false);
            }
        }
    }
}
