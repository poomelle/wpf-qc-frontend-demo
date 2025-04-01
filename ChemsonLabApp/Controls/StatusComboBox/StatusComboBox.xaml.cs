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

namespace ChemsonLabApp.Controls.StatusComboBox
{
    /// <summary>
    /// Interaction logic for StatusComboBox.xaml
    /// </summary>
    public partial class StatusComboBox : UserControl
    {
        //public List<string> StatusName { get; set; } = new List<string> { "All", "Active", "Inactive" };

        public StatusComboBox()
        {
            InitializeComponent();
        }

        public List<string> StatusName
        {
            get { return (List<string>)GetValue(StatusNameProperty); }
            set { SetValue(StatusNameProperty, value); }
        }

        public static readonly DependencyProperty StatusNameProperty =
            DependencyProperty.Register("StatusName", typeof(List<string>), typeof(StatusComboBox), new PropertyMetadata(null));


        public ICommand StatusChangeCommand
        {
            get { return (ICommand)GetValue(StatusChangeCommandProperty); }
            set { SetValue(StatusChangeCommandProperty, value); }
        }

        public static readonly DependencyProperty StatusChangeCommandProperty =
            DependencyProperty.Register("StatusChangeCommand", typeof(ICommand), typeof(StatusComboBox), new PropertyMetadata(null));

        private void statusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatusChangeCommand != null)
            {
                StatusChangeCommand.Execute(statusComboBox.SelectedItem);
            }
        }
    }
}
