using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.ReportVM.Command
{
    public class OpenReportFile : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public SearchReportViewModel viewModel { get; set; }
        public OpenReportFile(SearchReportViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is TestResultReport testResultReport)
            {
                viewModel.OnOpenReportFile(testResultReport);
            }
        }
    }
}
