using ChemsonLabApp.MVVM.ViewModels.ReportVM;
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
    /// Interaction logic for OpenReportView.xaml
    /// </summary>
    public partial class OpenReportView : Window
    {
        public OpenReportView(OpenReportViewModel openReportViewModel)
        {
            InitializeComponent();
            DataContext = openReportViewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
