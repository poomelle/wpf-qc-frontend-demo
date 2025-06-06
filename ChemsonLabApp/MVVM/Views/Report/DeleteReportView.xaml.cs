﻿using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.ReportVM;
using ChemsonLabApp.Services.IService;
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

namespace ChemsonLabApp.MVVM.Views.Report
{
    /// <summary>
    /// Interaction logic for DeleteReportView.xaml
    /// </summary>
    public partial class DeleteReportView : Window
    {
        //public DeleteReportView(TestResultReport testResultReport, ITestResultReportService testResultReportService)
        //{
        //    InitializeComponent();
        //    DataContext = new DeleteReportViewModel(testResultReport, testResultReportService);
        //}

        public DeleteReportView(DeleteReportViewModel deleteReportViewModel)
        {
            InitializeComponent();
            DataContext = deleteReportViewModel;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
