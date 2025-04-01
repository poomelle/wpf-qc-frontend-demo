using ChemsonLabApp.MVVM.Models;
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

namespace ChemsonLabApp.Controls.ProductNameComboBox
{
    /// <summary>
    /// Interaction logic for ProductNameComboBox.xaml
    /// </summary>
    public partial class ProductNameComboBox : UserControl
    {
        public ObservableCollection<string> ProductsName { get; set; } = new ObservableCollection<string>();
        public ProductNameComboBox()
        {
            InitializeComponent();
        }

        public ICommand SelectedProductNameChange
        {
            get { return (ICommand)GetValue(SelectedProductNameChangeProperty); }
            set { SetValue(SelectedProductNameChangeProperty, value); }
        }

        public static readonly DependencyProperty SelectedProductNameChangeProperty =
            DependencyProperty.Register("SelectedProductNameChange", typeof(ICommand), typeof(ProductNameComboBox), new PropertyMetadata(null));

        private async void ProductNameCombo_Loaded(object sender, RoutedEventArgs e)
        {
            var products = await LoadProductsAsync();
            var productsName = products.Select(p => p.name).ToList();

            ProductsName.Add("All");
            foreach (var product in products)
            {
                ProductsName.Add(product.name);
            }
        }

        private async Task<List<Product>> LoadProductsAsync()
        {
            var productRestAPI = new ProductRestAPI();
            return await productRestAPI.GetProductsAsync("?status=true", "&sortBy=Name&isAscending=true");
        }

        private void productComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedProductNameChange != null)
            {
                SelectedProductNameChange.Execute(productComboBox.SelectedItem);
            }
        }
    }
}
