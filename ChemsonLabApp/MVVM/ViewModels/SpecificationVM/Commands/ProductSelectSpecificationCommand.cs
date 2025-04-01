using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.SpecificationVM.Commands
{
    public class ProductSelectSpecificationCommand : ICommand
    {
        private readonly SpecificationViewModel _specificationViewModel;

        public event EventHandler CanExecuteChanged;

        public ProductSelectSpecificationCommand(SpecificationViewModel specificationViewModel)
        {
            this._specificationViewModel = specificationViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is Product product)
            {
                _specificationViewModel.SelectedProduct = product;
            }
        }
    }
}
