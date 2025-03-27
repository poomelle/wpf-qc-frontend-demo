using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;

namespace ChemsonLabApp.MVVM.Models
{
    [AddINotifyPropertyChangedInterface]
    public class Menu
    {
        public string MenuTitle { get; set; }
        public string IconName { get; set; }
        public bool IsSelected { get; set; } = false;
        public List<SubMenu> SubMenus { get; set; } = new List<SubMenu>();
        public bool IsSubmenuVisible { get; set; } = false;
    }
    [AddINotifyPropertyChangedInterface]
    public class SubMenu
    {
        public string SubMenuTitle { get; set; }
        public bool IsSelected { get; set; } = false;
    }
}
