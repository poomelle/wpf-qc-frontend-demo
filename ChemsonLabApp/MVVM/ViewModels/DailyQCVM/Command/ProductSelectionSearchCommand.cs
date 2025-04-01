using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.DailyQCVM.Command
{
    public class ProductSelectionSearchCommand : ICommand
    {
        private readonly DailyQCViewModel _dailyQCViewModel;

        public event EventHandler CanExecuteChanged;

        public ProductSelectionSearchCommand(DailyQCViewModel dailyQCViewModel)
        {
            this._dailyQCViewModel = dailyQCViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is string productName)
            {
                _dailyQCViewModel.SelectedProduct = productName;
            }
        }
    }
}
