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

namespace ChemsonLabApp.MVVM.Views.Specification
{
    /// <summary>
    /// Interaction logic for DeleteSpecificationView.xaml
    /// </summary>
    public partial class DeleteSpecificationView : Window
    {
        public DeleteSpecificationView(DeleteSpecificationViewModel deleteSpecificationViewModel)
        {
            InitializeComponent();
            DataContext = deleteSpecificationViewModel;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
