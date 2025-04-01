using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.NewDataLoaderVM.Command
{
    public class SuffixRadioButtonChangeSearchDataLoaderCommand : ICommand
    {
        private readonly SearchDataLoaderViewModel _searchDataLoaderViewModel;

        public event EventHandler CanExecuteChanged;

        public SuffixRadioButtonChangeSearchDataLoaderCommand(SearchDataLoaderViewModel searchDataLoaderViewModel)
        {
            this._searchDataLoaderViewModel = searchDataLoaderViewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is string selectedSuffix)
            {
                var suffix = selectedSuffix == "Default"? null : selectedSuffix;

                var testNumber = selectedSuffix == "Default" ? "1" :
                                selectedSuffix == "RS" ? "2" :
                                selectedSuffix == "RRS" ? "3" : "";

                _searchDataLoaderViewModel.Suffix = suffix;
                _searchDataLoaderViewModel.TestNumber = testNumber;
            }
        }
    }
}
