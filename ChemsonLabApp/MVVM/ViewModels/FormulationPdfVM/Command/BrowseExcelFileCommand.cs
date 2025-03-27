using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.FormulationPdfVM.Command
{
    public class BrowseExcelFileCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public FormulationPdfViewModel viewModel { get; set; }
        public BrowseExcelFileCommand(FormulationPdfViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is FormulationPdf)
            {
                viewModel.BrowseExcelFile(parameter as FormulationPdf);
            }
        }
    }
}
