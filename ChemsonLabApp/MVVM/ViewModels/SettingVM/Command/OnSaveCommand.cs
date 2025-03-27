using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.SettingVM.Command
{
    public class OnSaveCommand : ICommand
    {
        public SettingViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;
        public OnSaveCommand(SettingViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.SaveNewSettingValues();
        }
    }
}
