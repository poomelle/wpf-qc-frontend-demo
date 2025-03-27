using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.COAVM.Commands
{
    public class RemoveReportCommand : ICommand
    {
        public COAViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;
        public RemoveReportCommand(COAViewModel viewModel)
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
                viewModel.RemoveTestReport(testResultReport);
            }
        }
    }
}
