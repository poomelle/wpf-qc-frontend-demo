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

namespace ChemsonLabApp.Controls.BatchesInput
{
    /// <summary>
    /// Interaction logic for BatchesInput.xaml
    /// </summary>
    public partial class BatchesInput : UserControl
    {
        public BatchesInput()
        {
            InitializeComponent();
        }

        public ICommand FromBatchTextChangeCommand
        {
            get { return (ICommand)GetValue(FromBatchTextChangeCommandProperty); }
            set { SetValue(FromBatchTextChangeCommandProperty, value); }
        }

        public static readonly DependencyProperty FromBatchTextChangeCommandProperty =
            DependencyProperty.Register("FromBatchTextChangeCommand", typeof(ICommand), typeof(BatchesInput), new PropertyMetadata(null));

        private void FromBatch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FromBatchTextChangeCommand != null)
            {
                FromBatchTextChangeCommand.Execute(FromBatchTextBox.Text);
            }
        }

        public ICommand ToBatchTextChangeCommnad
        {
            get { return (ICommand)GetValue(ToBatchTextChangeCommnadProperty); }
            set { SetValue(ToBatchTextChangeCommnadProperty, value); }
        }

        public static readonly DependencyProperty ToBatchTextChangeCommnadProperty =
            DependencyProperty.Register("ToBatchTextChangeCommnad", typeof(ICommand), typeof(BatchesInput), new PropertyMetadata(null));

        private void ToBatch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ToBatchTextChangeCommnad != null)
            {
                ToBatchTextChangeCommnad.Execute(ToBatchTextBox.Text);
            }
        }
    }
}
