using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.InstrumentVM.Commands
{
    public class IsViewToggleCommand : ICommand
    {
        public InstrumentViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;
        public IsViewToggleCommand(InstrumentViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var instrument = parameter as Instrument;
            if (instrument != null)
            {
                viewModel.isVewModeToggle(instrument);
            }
        }
    }
}
