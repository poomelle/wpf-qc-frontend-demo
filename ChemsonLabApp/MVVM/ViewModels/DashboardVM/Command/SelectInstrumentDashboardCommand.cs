using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.DashboardVM.Command
{
    public class SelectInstrumentDashboardCommand : ICommand
    {
        private readonly DashboardViewModel _dashboardViewModel;

        public event EventHandler CanExecuteChanged;

        public SelectInstrumentDashboardCommand(DashboardViewModel dashboardViewModel)
        {
            this._dashboardViewModel = dashboardViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is string instrument)
            {
                _dashboardViewModel.SelectedInstrument = instrument;
            }
        }
    }
}
