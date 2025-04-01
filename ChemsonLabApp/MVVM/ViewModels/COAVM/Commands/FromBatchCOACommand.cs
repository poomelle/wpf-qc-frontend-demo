using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.COAVM.Commands
{
    public class FromBatchCOACommand : ICommand
    {
        private readonly COAViewModel _cOAViewModel;

        public event EventHandler CanExecuteChanged;

        public FromBatchCOACommand(COAViewModel cOAViewModel)
        {
            this._cOAViewModel = cOAViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is string text)
            {
                _cOAViewModel.FromBatch = text;
            }
        }
    }
}
