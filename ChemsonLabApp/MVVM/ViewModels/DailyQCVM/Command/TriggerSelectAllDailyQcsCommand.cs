using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.DailyQCVM.Command
{
    public class TriggerSelectAllDailyQcsCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public DailyQCViewModel viewModel { get; set; }
        public TriggerSelectAllDailyQcsCommand(DailyQCViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {

            viewModel.TriggerSelectAllDailyQcs();
            
        }
    }
}
