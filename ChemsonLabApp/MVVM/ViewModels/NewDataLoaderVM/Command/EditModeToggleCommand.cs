using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.NewDataLoaderVM.Command
{
    public class EditModeToggleCommand : ICommand
    {
        public EditDataLoaderViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;
        public EditModeToggleCommand(EditDataLoaderViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.EditModeToggle();
        }
    }
}
