using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.DailyQCVM;
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

namespace ChemsonLabApp.MVVM.Views.DailyQC
{
    /// <summary>
    /// Interaction logic for DailyQCView.xaml
    /// </summary>
    public partial class DailyQCView : ContentControl
    {
        public DailyQCView(DailyQCViewModel dailyQCViewModel)
        {
            InitializeComponent();
            DataContext = dailyQCViewModel;
        }

        private void DataGrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            if (e.NewItem is DailyQc newDailyQc)
            {
                newDailyQc.incomingDate = DateTime.Now;
            }
        }
    }
}
