using ChemsonLabApp.MVVM.ViewModels.QCLabelVM;
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

namespace ChemsonLabApp.MVVM.Views.QCLabel
{
    /// <summary>
    /// Interaction logic for QCLabelView.xaml
    /// </summary>
    public partial class QCLabelView : ContentControl
    {
        public QCLabelView(QCLabelViewModel qCLabelViewModel)
        {
            InitializeComponent();
            DataContext = qCLabelViewModel;
        }
    }
}
