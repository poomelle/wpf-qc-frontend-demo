﻿using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.CustomerVM.Commands
{
    public class UpdateCustomerCommand : ICommand
    {
        public CustomerViewModel viewModel { get; set; }
        public event EventHandler CanExecuteChanged;
        public UpdateCustomerCommand(CustomerViewModel viewModel)
        {
            this.viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            if (parameter is Customer customer)
            {
                await viewModel.SaveChangeCustomer(customer);
            }
        }
    }
}
