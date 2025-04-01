using ChemsonLabApp.MVVM.ViewModels.NewDataLoaderVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.ReportVM.Command
{
    public class SuffixRadioButtonChangeReportCommand : ICommand
    {
        private readonly NewReportViewModel _newReportViewModel;
        private readonly SearchReportViewModel _searchReportViewModel;

        public SuffixRadioButtonChangeReportCommand(NewReportViewModel newReportViewModel)
        {
            this._newReportViewModel = newReportViewModel;
        }

        public SuffixRadioButtonChangeReportCommand(SearchReportViewModel searchReportViewModel)
        {
            this._searchReportViewModel = searchReportViewModel;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is string selectedSuffix)
            {
                var suffix = selectedSuffix == "Default" ? null : selectedSuffix;

                var testNumber = selectedSuffix == "Default" ? "1" :
                                 selectedSuffix == "RS" ? "2" :
                                 selectedSuffix == "RRS" ? "3" : "";

                if (_newReportViewModel != null)
                {
                    _newReportViewModel.Suffix = suffix;
                    _newReportViewModel.TestNumber = testNumber;
                }

                if (_searchReportViewModel != null)
                {
                    _searchReportViewModel.Suffix = suffix;
                    _searchReportViewModel.TestNumber = testNumber;
                }
            }
        }
    }
}
