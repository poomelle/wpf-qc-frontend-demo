using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.DailyQCVM.Command
{
    public class SelectTestStatusChangeCommand : ICommand
    {
        private readonly DailyQCViewModel _dailyQCViewModel;

        public event EventHandler CanExecuteChanged;

        public SelectTestStatusChangeCommand(DailyQCViewModel dailyQCViewModel)
        {
            this._dailyQCViewModel = dailyQCViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is string text)
            {
                _dailyQCViewModel.SelectedTestStatus = text;
            }
        }
    }
}
