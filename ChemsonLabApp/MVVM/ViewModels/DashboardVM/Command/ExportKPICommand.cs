using ChemsonLabApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.DashboardVM.Command
{
    public class ExportKPICommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public DashboardViewModel viewModel { get; set; }
        public ExportKPICommand(DashboardViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.ExportKPI();
        }
    }
}
