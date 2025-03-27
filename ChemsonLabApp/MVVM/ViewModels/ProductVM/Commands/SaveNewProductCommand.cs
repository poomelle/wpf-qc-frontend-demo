using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.ProductVM.Commands
{
    public class SaveNewProductCommand : ICommand
    {
        public AddProductViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;

        public SaveNewProductCommand(AddProductViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.AddNewProduct();
        }
    }
}
