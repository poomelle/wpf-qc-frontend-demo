using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.NewDataLoaderVM.Command
{
    public class SelectProductCommand : ICommand
    {
        private readonly SearchDataLoaderViewModel _searchDataLoaderViewModel;

        public event EventHandler CanExecuteChanged;
        public SelectProductCommand(SearchDataLoaderViewModel searchDataLoaderViewModel)
        {
            this._searchDataLoaderViewModel = searchDataLoaderViewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is Product product)
            {
                _searchDataLoaderViewModel.SelectedProduct = product;
            }
        }
    }
}
