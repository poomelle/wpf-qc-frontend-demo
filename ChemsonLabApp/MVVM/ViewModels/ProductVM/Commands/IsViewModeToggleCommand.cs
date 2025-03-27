using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.ProductVM.Commands
{
    public class IsViewModeToggleCommand : ICommand
    {
        public ProductViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;

        public IsViewModeToggleCommand(ProductViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var product = parameter as Product;
            viewModel.isVewModeToggle(product);
        }
    }
}
