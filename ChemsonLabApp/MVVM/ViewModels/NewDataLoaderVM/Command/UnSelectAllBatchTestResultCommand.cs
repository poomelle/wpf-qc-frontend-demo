﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.NewDataLoaderVM.Command
{
    public class UnSelectAllBatchTestResultCommand : ICommand
    {
        public SearchDataLoaderViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;
        public UnSelectAllBatchTestResultCommand(SearchDataLoaderViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.UnSelectAllBatchTestResult();
        }
    }
}
