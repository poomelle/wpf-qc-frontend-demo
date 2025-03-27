using ChemsonLabApp.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemsonLabApp.Constants
{
    public static class MainMenuItems
    {
        public static ObservableCollection<Menu> Menus => new ObservableCollection<Menu>()
        {
            new Menu
            {
                MenuTitle = "Daily QC",
                IconName = "Home",
            },
            new Menu
            {
                MenuTitle = "Data Loader",
                IconName = "Upload",
                SubMenus = new List<SubMenu>
                {
                    new SubMenu
                    {
                        SubMenuTitle = "New Data Loader",
                    },
                    new SubMenu
                    {
                        SubMenuTitle = "Search Data Loader",
                    }
                }
            },
            new Menu
            {
                MenuTitle = "Report",
                IconName = "Document",
                SubMenus = new List<SubMenu>
                {
                    new SubMenu
                    {
                        SubMenuTitle = "New Report",
                    },
                    new SubMenu
                    {
                        SubMenuTitle = "Search Report",
                    },
                }
            },
            new Menu
            {
                MenuTitle = "COA",
                IconName = "Bookmark",
            },
            new Menu
            {
                MenuTitle = "QC Label",
                IconName = "Bookmark",
            },
            new Menu
            {
                MenuTitle = "Dashboard",
                IconName = "Chartbar",
            },
            new Menu
            {
                MenuTitle = "Product",
                IconName = "Beaker",
                SubMenus = new List<SubMenu>
                {
                    new SubMenu
                    {
                        SubMenuTitle = "Product",
                    },
                    new SubMenu
                    {
                        SubMenuTitle = "Specification",
                    }
                }
            },
            new Menu
            {
                MenuTitle = "Instrument",
                IconName = "GlobeOutline",
            },
            new Menu
            {
                MenuTitle = "Customer",
                IconName = "User",
                SubMenus = new List<SubMenu>
                {
                    new SubMenu
                    {
                        SubMenuTitle = "Customer Order",
                    },
                    new SubMenu
                    {
                        SubMenuTitle = "Customer",
                    }
                }
            },
            new Menu
            {
                MenuTitle = "PDF",
                IconName = "Document",
            },
            new Menu
            {
                MenuTitle = "Setting",
                IconName = "CogOutline",
            },
        };
    }
}
