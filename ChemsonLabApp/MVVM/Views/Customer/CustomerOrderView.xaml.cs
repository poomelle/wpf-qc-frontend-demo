﻿using ChemsonLabApp.MVVM.ViewModels.CustomerVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChemsonLabApp.MVVM.Views.Customer
{
    /// <summary>
    /// Interaction logic for CustomerOrderView.xaml
    /// </summary>
    public partial class CustomerOrderView : ContentControl
    {
        public CustomerOrderView(CustomerOrderViewModel customerOrderViewModel)
        {
            InitializeComponent();
            DataContext = customerOrderViewModel;
        }
    }
}
