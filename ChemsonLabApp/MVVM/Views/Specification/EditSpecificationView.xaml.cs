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
    /// Interaction logic for EditSpecificationView.xaml
    /// </summary>
    public partial class EditSpecificationView : Window
    {
        public EditSpecificationView(EditSpecificationViewModel editSpecificationViewModel)
        {
            InitializeComponent();
            DataContext = editSpecificationViewModel;
        }

        private void CancelBtnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
