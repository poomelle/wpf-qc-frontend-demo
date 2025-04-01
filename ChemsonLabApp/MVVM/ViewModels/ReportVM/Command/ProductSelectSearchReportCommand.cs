using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.ReportVM.Command
{
    public class ProductSelectSearchReportCommand : ICommand
    {
        private readonly SearchReportViewModel _searchReportViewModel;

        public event EventHandler CanExecuteChanged;

        public ProductSelectSearchReportCommand(SearchReportViewModel searchReportViewModel)
        {
            this._searchReportViewModel = searchReportViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is Product product)
            {
                _searchReportViewModel.SelectedProduct = product;
            }
        }
    }
}
