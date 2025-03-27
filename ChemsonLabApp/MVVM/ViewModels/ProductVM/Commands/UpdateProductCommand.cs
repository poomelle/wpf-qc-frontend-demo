using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.ProductVM.Commands
{
    public class UpdateProductCommand : ICommand
    {
        public ProductViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;

        public UpdateProductCommand(ProductViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var updateProduct = parameter as Product;
            if (updateProduct != null)
            {
                viewModel.UpdateProductAsync(updateProduct);
            }
        }
    }
}
