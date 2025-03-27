using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.InstrumentVM.Commands
{
    public class SaveNewInstrumentCommand : ICommand
    {
        public AddInstrumentViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;

        public SaveNewInstrumentCommand(AddInstrumentViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.AddNewInstrument();
        }
    }
}
