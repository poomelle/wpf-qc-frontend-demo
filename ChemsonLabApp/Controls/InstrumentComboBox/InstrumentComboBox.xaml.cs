using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChemsonLabApp.Controls.InstrumentComboBox
{
    /// <summary>
    /// Interaction logic for InstrumentComboBox.xaml
    /// </summary>
    public partial class InstrumentComboBox : UserControl
    {
        public ObservableCollection<Instrument> Instruments { get; set; } = new ObservableCollection<Instrument>();
        public InstrumentComboBox()
        {
            InitializeComponent();
        }



        public ICommand InstrumentChangedCommand
        {
            get { return (ICommand)GetValue(InstrumentChangedCommandProperty); }
            set { SetValue(InstrumentChangedCommandProperty, value); }
        }

        public static readonly DependencyProperty InstrumentChangedCommandProperty =
            DependencyProperty.Register("InstrumentChangedCommand", typeof(ICommand), typeof(InstrumentComboBox), new PropertyMetadata(null));



        private void productComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InstrumentChangedCommand != null)
            {
                InstrumentChangedCommand.Execute(instrumentComboBox.SelectedItem);
            }
        }

        private async void instrumentCombo_Loaded(object sender, RoutedEventArgs e)
        {
            var instrumentRestAPI = new InstrumentRestAPI();
            var list = await instrumentRestAPI.GetInstrumentsAsync("?status=true", "&sortBy=Name&isAscending=true");

            Instruments.Clear();
            foreach (var instrument in list)
                Instruments.Add(instrument);
        }
    }
}
