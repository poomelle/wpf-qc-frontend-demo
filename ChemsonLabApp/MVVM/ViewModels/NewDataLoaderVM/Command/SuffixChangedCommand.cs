using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.NewDataLoaderVM.Command
{
    public class SuffixChangedCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public NewDataLoaderViewModel viewModel { get; set; }
        public SuffixChangedCommand(NewDataLoaderViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {

            if (parameter is TestResult testResult)
            {
                viewModel.SuffixChanged(testResult);
            }

        }
    }
}
