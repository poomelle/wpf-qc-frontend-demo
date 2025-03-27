using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.CustomerVM.Commands
{
    public class ReloadDataCommand : ICommand
    {
        public CustomerOrderViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;
        public ReloadDataCommand(CustomerOrderViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.ReloadData();
        }
    }
}
