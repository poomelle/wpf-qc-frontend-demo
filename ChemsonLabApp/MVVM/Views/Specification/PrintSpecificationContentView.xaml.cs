using ChemsonLabApp.MVVM.ViewModels.SpecificationVM;
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
using PropertyChanged;
using System.Windows.Controls.Primitives;
using System.Runtime.Remoting.Messaging;
using System.IO;
using System.Xml;
using System.Windows.Markup;
using ChemsonLabApp.MVVM.Models;
using System.Collections.ObjectModel;
using ChemsonLabApp.RestAPI;

namespace ChemsonLabApp.MVVM.Views.Specification
{
    /// <summary>
    /// Interaction logic for PrintSpecificationContentView.xaml
    /// </summary>
    public partial class PrintSpecificationContentView : Window
    {

        public PrintSpecificationContentView(PrintSpecificationContentViewModel printSpecificationContentViewModel)
        {
            InitializeComponent();
            DataContext = printSpecificationContentViewModel;
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }

}
