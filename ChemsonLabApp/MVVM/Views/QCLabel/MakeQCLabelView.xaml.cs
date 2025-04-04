﻿using ChemsonLabApp.MVVM.ViewModels.QCLabelVM;
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
using ChemsonLabApp.MVVM.Models;

namespace ChemsonLabApp.MVVM.Views.QCLabel
{
    /// <summary>
    /// Interaction logic for MakeQCLabelView.xaml
    /// </summary>
    public partial class MakeQCLabelView : Window
    {
        public MakeQCLabelView(MakeQCLabelViewModel makeQCLabelViewModel)
        {
            InitializeComponent();
            DataContext = makeQCLabelViewModel;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
