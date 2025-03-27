﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.SpecificationVM.Commands
{
    public class ReloadDataCommand : ICommand
    {
        public SpecificationViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;
        public ReloadDataCommand(SpecificationViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.ReloadSpecification();
        }
    }
}
