using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.MVVM.ViewModels.COAVM;
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

namespace ChemsonLabApp.MVVM.Views.COA
{
    /// <summary>
    /// Interaction logic for MakeCOAView.xaml
    /// </summary>
    public partial class MakeCOAView : Window
    {
        public MakeCOAView(MakeCOAViewModel makeCOAViewModel)
        {
            InitializeComponent();
            DataContext = makeCOAViewModel;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
