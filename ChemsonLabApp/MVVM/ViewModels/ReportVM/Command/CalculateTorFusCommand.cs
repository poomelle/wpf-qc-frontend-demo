using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.ReportVM.Command
{
    public class CalculateTorFusCommand : ICommand
    {
        public NewReportViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;
        public CalculateTorFusCommand(NewReportViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.OnStdChangedCalculateTorAndFus();
        }
    }
}
