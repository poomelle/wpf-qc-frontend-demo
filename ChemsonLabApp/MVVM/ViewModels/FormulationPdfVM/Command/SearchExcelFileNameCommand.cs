﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.FormulationPdfVM.Command
{
    public class SearchExcelFileNameCommand : ICommand
    {
        public FormulationPdfViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;
        public SearchExcelFileNameCommand(FormulationPdfViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {

            viewModel.SearchExcelFileName();

        }
    }
}
