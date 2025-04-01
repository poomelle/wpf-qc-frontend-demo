using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.SpecificationVM.Commands
{
    public class SelectStatusChangeSpecificationCommand : ICommand
    {
        private readonly SpecificationViewModel _specificationViewModel;

        public event EventHandler CanExecuteChanged;

        public SelectStatusChangeSpecificationCommand(SpecificationViewModel specificationViewModel)
        {
            this._specificationViewModel = specificationViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is string text)
            {
                _specificationViewModel.SelectedStatus = text;
            }
        }
    }
}
