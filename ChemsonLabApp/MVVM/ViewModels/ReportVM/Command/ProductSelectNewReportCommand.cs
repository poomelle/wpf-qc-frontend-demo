﻿using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.ReportVM.Command
{
    public class ProductSelectNewReportCommand : ICommand
    {
        private readonly NewReportViewModel _newReportViewModel;

        public event EventHandler CanExecuteChanged;
        public ProductSelectNewReportCommand(NewReportViewModel newReportViewModel)
        {
            this._newReportViewModel = newReportViewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is Product product)
            {
                _newReportViewModel.SelectedProduct = product;
            }
        }
    }
}
