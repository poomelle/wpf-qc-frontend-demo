using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.ProductVM.Commands
{
    public class ProductNameSearchCommand : ICommand
    {
        public ProductViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;
        public ProductNameSearchCommand(ProductViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            //viewModel.ProductSearch();
            await viewModel.GetAllProductsAsync();
        }
    }
}
