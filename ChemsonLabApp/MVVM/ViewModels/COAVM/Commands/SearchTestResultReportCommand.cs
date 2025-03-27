using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.COAVM.Commands
{
    public class SearchTestResultReportCommand : ICommand
    {
        public COAViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;
        public SearchTestResultReportCommand(COAViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.SearchTestResultReport();
        }
    }
}
