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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChemsonLabApp.Controls.SuffixSelector
{
    /// <summary>
    /// Interaction logic for SuffixSelector.xaml
    /// </summary>
    public partial class SuffixSelector : UserControl
    {
        public SuffixSelector()
        {
            InitializeComponent();
        }

        public ICommand RadioButtonCheckCommand
        {
            get { return (ICommand)GetValue(RadioButtonCheckCommandProperty); }
            set { SetValue(RadioButtonCheckCommandProperty, value); }
        }

        public static readonly DependencyProperty RadioButtonCheckCommandProperty =
            DependencyProperty.Register("RadioButtonCheckCommand", typeof(ICommand), typeof(SuffixSelector), new PropertyMetadata(null));

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var senderRadioButton = (RadioButton)sender;

            if (RadioButtonCheckCommand != null && senderRadioButton != null)
            {
                RadioButtonCheckCommand.Execute(senderRadioButton.Tag?.ToString());
            }
        }
    }
}
