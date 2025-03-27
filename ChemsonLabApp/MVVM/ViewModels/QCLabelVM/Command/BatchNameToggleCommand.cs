using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.QCLabelVM.Command
{
    public class BatchNameToggleCommand : ICommand
    {
        public QCLabelViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;
        public BatchNameToggleCommand(QCLabelViewModel viewModel)
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
                if (parameter is QCLabel qcLabel)
                {
                    viewModel.BatchNameToggle(qcLabel);
                }
            }catch (Exception e)
            {
                NotificationUtility.ShowError("Error in BatchNameToggleCommand");
                LoggerUtility.LogError(e);
            }
        }
    }
}
