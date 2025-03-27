using ChemsonLabApp.MVVM.ViewModels.InstrumentVM;
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

namespace ChemsonLabApp.MVVM.Views.Instrument
{
    /// <summary>
    /// Interaction logic for DeleteInstrumentView.xaml
    /// </summary>
    public partial class DeleteInstrumentView : Window
    {
        public DeleteInstrumentView(DeleteInstrumentViewModel deleteInstrumentViewModel)
        {
            InitializeComponent();
            DataContext = deleteInstrumentViewModel;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
