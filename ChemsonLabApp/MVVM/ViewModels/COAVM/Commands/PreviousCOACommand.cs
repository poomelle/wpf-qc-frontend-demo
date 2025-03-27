using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.COAVM.Commands
{
    public class PreviousCOACommand : ICommand
    {
        public MakeCOAViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;
        public PreviousCOACommand(MakeCOAViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.PreviousIndividualCOA();
        }
    }
}
