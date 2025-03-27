using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.NewDataLoaderVM.Command
{
    public class SelectAllBatchTestResultCommand : ICommand
    {
        public SearchDataLoaderViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;
        public SelectAllBatchTestResultCommand(SearchDataLoaderViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.SelectAllBatchTestResult();
        }
    }
}
