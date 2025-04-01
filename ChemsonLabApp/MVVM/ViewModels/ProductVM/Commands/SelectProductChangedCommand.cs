using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.ProductVM.Commands
{
    public class SelectProductChangedCommand : ICommand
    {
        private readonly ProductViewModel _productViewModel;

        public event EventHandler CanExecuteChanged;

        public SelectProductChangedCommand(ProductViewModel productViewModel)
        {
            this._productViewModel = productViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is string productName)
            {
                _productViewModel.SelectedProduct = productName;
            }
        }
    }
}
