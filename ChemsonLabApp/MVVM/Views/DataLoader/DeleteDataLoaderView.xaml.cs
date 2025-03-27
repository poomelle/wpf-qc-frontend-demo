using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.NewDataLoaderVM;
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

namespace ChemsonLabApp.MVVM.Views.DataLoader
{
    /// <summary>
    /// Interaction logic for DeleteDataLoaderView.xaml
    /// </summary>
    public partial class DeleteDataLoaderView : Window
    {
        public DeleteDataLoaderView(DeleteDataLoaderViewModel deleteDataLoaderViewModel)
        {
            InitializeComponent();
            DataContext = deleteDataLoaderViewModel;
        }

        //public DeleteDataLoaderView(DeleteDataLoaderViewModel deleteDataLoaderViewModel)
        //{
        //    InitializeComponent();
        //    DataContext = deleteDataLoaderViewModel);
        //}

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
