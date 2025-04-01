using ChemsonLabApp.MVVM.Models;
using ChemsonLabApp.RestAPI;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace ChemsonLabApp.Controls.ProductComboBox
{
    /// <summary>
    /// Interaction logic for ProductComboBox.xaml
    /// </summary>
    public partial class ProductComboBox : UserControl
    {
        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();

        public ICommand ProductSelectionChangedCommand
        {
            get { return (ICommand)GetValue(ProductSelectionChangedCommandProperty); }
            set { SetValue(ProductSelectionChangedCommandProperty, value); }
        }

        public static readonly DependencyProperty ProductSelectionChangedCommandProperty =
            DependencyProperty.Register("ProductSelectionChangedCommand", typeof(ICommand), typeof(ProductComboBox), new PropertyMetadata(null));

        public ProductComboBox()
        {
            InitializeComponent();
        }

        private async Task LoadProductsAsync()
        {
            var productRestAPI = new ProductRestAPI();
            var list = await productRestAPI.GetProductsAsync("?status=true", "&sortBy=Name&isAscending=true");

            Products.Clear();
            foreach (var product in list)
                Products.Add(product);
        }

        private async void productCombo_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadProductsAsync();
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductSelectionChangedCommand != null)
            {
                ProductSelectionChangedCommand.Execute(productComboBox.SelectedItem);
            }
        }
    }
}
