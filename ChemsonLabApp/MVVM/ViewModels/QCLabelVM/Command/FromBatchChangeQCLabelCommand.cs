using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.QCLabelVM.Command
{
    public class FromBatchChangeQCLabelCommand : ICommand
    {
        private readonly QCLabelViewModel _qCLabelViewModel;

        public event EventHandler CanExecuteChanged;

        public FromBatchChangeQCLabelCommand(QCLabelViewModel qCLabelViewModel)
        {
            this._qCLabelViewModel = qCLabelViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is string text)
            {
                _qCLabelViewModel.BatchNumberStart = text;
            }
        }
    }
}
