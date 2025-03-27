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
    /// Interaction logic for EditSpecificationContenControlView.xaml
    /// </summary>
    public partial class EditSpecificationContenControlView : ContentControl
    {
        public EditSpecificationContenControlView(EditSpecifiationContentControlView editSpecifiationContentControlView)
        {
            InitializeComponent();
            DataContext = editSpecifiationContentControlView;
        }
    }
}
