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

namespace ChemsonLabApp.Controls.InstrumentNameComboBox
{
    /// <summary>
    /// Interaction logic for InstrumentNameComboBox.xaml
    /// </summary>
    public partial class InstrumentNameComboBox : UserControl
    {
        public ObservableCollection<string> InstrumentsName { get; set; } = new ObservableCollection<string>();
        public InstrumentNameComboBox()
        {
            InitializeComponent();
        }

        private async void InstrumentNameCombo_Loaded(object sender, RoutedEventArgs e)
        {
            var instrumentRestAPI = new InstrumentRestAPI();
            var list = await instrumentRestAPI.GetInstrumentsAsync("?status=true", "&sortBy=Name&isAscending=true");

            InstrumentsName.Clear();
            InstrumentsName.Add("All");
            foreach (var instrument in list)
            {
                InstrumentsName.Add(instrument.name);
            }
        }



        public ICommand InstrumentNameChangeCommand
        {
            get { return (ICommand)GetValue(InstrumentNameChangeCommandProperty); }
            set { SetValue(InstrumentNameChangeCommandProperty, value); }
        }

        public static readonly DependencyProperty InstrumentNameChangeCommandProperty =
            DependencyProperty.Register("InstrumentNameChangeCommand", typeof(ICommand), typeof(InstrumentNameComboBox), new PropertyMetadata(null));


        private void instrumentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InstrumentNameChangeCommand != null)
            {
                InstrumentNameChangeCommand.Execute(instrumentComboBox.SelectedItem);
            }
        }
    }
}
