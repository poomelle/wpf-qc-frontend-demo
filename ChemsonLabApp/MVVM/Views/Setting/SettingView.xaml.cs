using ChemsonLabApp.MVVM.ViewModels.SettingVM;
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

namespace ChemsonLabApp.MVVM.Views.Setting
{
    /// <summary>
    /// Interaction logic for SettingView.xaml
    /// </summary>
    public partial class SettingView : ContentControl
    {
        public SettingView()
        {
            InitializeComponent();
            DataContext = new SettingViewModel();
        }

        private void Username_Edit_Click(object sender, RoutedEventArgs e)
        {
            UsernameTextBox.IsEnabled = UsernameTextBox.IsEnabled ? false : true;
        }

        private void Email_Edit_Click(object sender, RoutedEventArgs e)
        {
            EmailTextBox.IsEnabled = EmailTextBox.IsEnabled ? false : true;
        }

        private void COAPath_Edit_Click(object sender, RoutedEventArgs e)
        {
            COAPathTextBox.IsEnabled = COAPathTextBox.IsEnabled ? false : true;
        }

        private void IPAddress_Edit_Click(object sender, RoutedEventArgs e)
        {
            IPAddressTextBox.IsEnabled = IPAddressTextBox.IsEnabled ? false : true;
        }

        private void FormulationPdfPath_Edit_Click(object sender, RoutedEventArgs e)
        {
            FormulationPdfPathTextBox.IsEnabled = FormulationPdfPathTextBox.IsEnabled ? false : true;
        }
    }
}
