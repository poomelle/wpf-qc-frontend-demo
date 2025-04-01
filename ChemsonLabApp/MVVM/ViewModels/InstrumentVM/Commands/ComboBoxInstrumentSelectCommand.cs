using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.InstrumentVM.Commands
{
    public class ComboBoxInstrumentSelectCommand : ICommand
    {
        public InstrumentViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;
        public ComboBoxInstrumentSelectCommand(InstrumentViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is Instrument instrument)
            {
                viewModel.ComboBoxInstrumentFilter(instrument);
            }

            if (parameter is string status)
            {
                viewModel.ComboBoxInstrumentFilter(null, status);
            }
        }
    }
}
