﻿using ChemsonLabApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.QCLabelVM.Command
{
    public class ClearQCLabelsCommand : ICommand
    {
        public QCLabelViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;
        public ClearQCLabelsCommand(QCLabelViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {

            viewModel.ClearQCLabels();

        }
    }
}
