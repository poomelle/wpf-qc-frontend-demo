using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.ProductVM.Commands
{
    public class DeletedProductCommand : ICommand
    {
        public DeleteProductViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;

        public DeletedProductCommand(DeleteProductViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.DeleteProductAsync();
        }
    }
}
