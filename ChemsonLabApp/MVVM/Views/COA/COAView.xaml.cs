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
    /// Interaction logic for COAView.xaml
    /// </summary>
    public partial class COAView : ContentControl
    {
        public COAView(COAViewModel cOAViewModel)
        {
            InitializeComponent();
            DataContext = cOAViewModel;
        }
    }
}
