using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.InstrumentVM.Commands;
using ChemsonLabApp.RestAPI;
using ChemsonLabApp.Services.IService;
using ChemsonLabApp.Utilities;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.ViewModels.InstrumentVM
{
    [AddINotifyPropertyChangedInterface]
    public class DeleteInstrumentViewModel
    {
        private readonly IInstrumentService _instrumentService;

        public string DeleteConfirm { get; set; }
        public Instrument Instrument { get; set; }
        public DeleteInstrumentCommand DeleteInstrumentCommand { get; set; }
        public DeleteInstrumentViewModel(IInstrumentService instrumentService)
        {
            // services
            this._instrumentService = instrumentService;

            // commands
            DeleteInstrumentCommand = new DeleteInstrumentCommand(this);
        }

        public async void DeleteInstrument()
        { 
            var deletedInstrument = await _instrumentService.DeleteInstrumentAsync(Instrument, DeleteConfirm);

            if (deletedInstrument != null)
            {
                NotificationUtility.ShowSuccess("Instrument has been deleted");
            }
        }
    }
}
