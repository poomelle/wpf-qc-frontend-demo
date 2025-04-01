using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.SpecificationVM.Commands
{
    public class InstrumentSelectSpecificationCommand : ICommand
    {
        private readonly SpecificationViewModel _specificationViewModel;

        public event EventHandler CanExecuteChanged;

        public InstrumentSelectSpecificationCommand(SpecificationViewModel specificationViewModel)
        {
            this._specificationViewModel = specificationViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is Instrument instrument)
            {
                _specificationViewModel.SelectedInstrument = instrument;
            }
        }
    }
}
