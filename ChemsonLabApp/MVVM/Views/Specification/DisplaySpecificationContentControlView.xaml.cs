using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
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
using ChemsonLabApp.MVVM.ViewModels.SpecificationVM;

namespace ChemsonLabApp.MVVM.Views.Specification
{
    /// <summary>
    /// Interaction logic for DisplaySpecificationContentControlView.xaml
    /// </summary>
    public partial class DisplaySpecificationContentControlView : ContentControl
    {
        public DisplaySpecificationContentControlView(DisplaySpecificationContentControlViewModel displaySpecificationContentControlViewModel)
        {
            InitializeComponent();
            DataContext = displaySpecificationContentControlViewModel;
        }
    }
}
