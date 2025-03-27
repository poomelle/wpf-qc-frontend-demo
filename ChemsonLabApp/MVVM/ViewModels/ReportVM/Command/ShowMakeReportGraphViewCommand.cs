using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.ReportVM.Command
{
    public class ShowMakeReportGraphViewCommand : ICommand
    {
        public SearchReportViewModel SearchReportViewModel { get; set; }

        public event EventHandler CanExecuteChanged;
        public ShowMakeReportGraphViewCommand(SearchReportViewModel searchReportViewModel)
        {
            SearchReportViewModel = searchReportViewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is TestResultReport testResultReport)
            {
                SearchReportViewModel.ShowMakeReportGraphView(testResultReport);
            }
        }
    }
}
