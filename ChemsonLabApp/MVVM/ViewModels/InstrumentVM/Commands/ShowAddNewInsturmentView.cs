using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.InstrumentVM.Commands
{
    public class ShowAddNewInsturmentView : ICommand
    {
        public InstrumentViewModel viewModel { get; set; }

        public event EventHandler CanExecuteChanged;

        public ShowAddNewInsturmentView(InstrumentViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.PopupAddInstrumentView();
        }
    }
}
