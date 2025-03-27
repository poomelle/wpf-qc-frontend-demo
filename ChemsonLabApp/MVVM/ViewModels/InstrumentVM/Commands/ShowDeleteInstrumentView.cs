using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.InstrumentVM.Commands
{
    public class ShowDeleteInstrumentView : ICommand
    {
        public InstrumentViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;
        public ShowDeleteInstrumentView(InstrumentViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var deleteInstrument = parameter as Instrument;
            viewModel.PopupDeleteInstrumentView(deleteInstrument);
        }
    }
}
