using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChemsonLabApp.MVVM.ViewModels.ProductVM.Commands
{
    public class SelectStatusChangeProductCommand : ICommand
    {
        private readonly ProductViewModel _productViewMod;

        public event EventHandler CanExecuteChanged;

        public SelectStatusChangeProductCommand(ProductViewModel productViewMod)
        {
            this._productViewMod = productViewMod;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is string text)
            {
                _productViewMod.ComboBoxSelectedStatus = text;
            }
        }
    }
}
