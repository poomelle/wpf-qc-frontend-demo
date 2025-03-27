using ChemsonLabApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.DailyQCVM.Command
{
    public class SaveDataDailyQcCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public DailyQCViewModel viewModel { get; set; }
        public SaveDataDailyQcCommand(DailyQCViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                viewModel.SaveDailyQcsToDatabase();
            }
            catch (Exception ex)
            {
                NotificationUtility.ShowError("An error occurred while saving data. Please try again.");
                LoggerUtility.LogError(ex);
            }
        }
    }
}
