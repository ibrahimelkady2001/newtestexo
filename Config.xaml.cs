using EXOApp.Models;
using Syncfusion.Maui.DataSource.Extensions;
using Syncfusion.Maui.ListView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EXOApp
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Config : ContentPage
	{
        ConfigViewModel viewModel;
        bool collapsed = true;
        public Config ()
		{
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            viewModel = (Models.ConfigViewModel)BindingContext;
            configListView.Loaded += ListView_Loaded;
            configListView.DataSource.GroupDescriptors.Add(new Syncfusion.Maui.DataSource.GroupDescriptor()
            {
                PropertyName = "Category",
                KeySelector = (object obj1) =>
                {
                    var item = obj1 as ConfigViewModel.Config;
                    return item.Category;
                },
                Comparer = new CustomGroupComparer()
            });
            configListView.CollapseAll();
        }

        private void ListView_Loaded(object sender, ListViewLoadedEventArgs e)
        {
            configListView.CollapseAll();
        }

        void CollapseExpandClick(object sender, EventArgs args)
        {
            if(collapsed)
            {
                collapsed = false;
                collapseExpandAll.Text = "Collapse All";
                configListView.ExpandAll();
            }
            else
            {
                collapsed = true;
                collapseExpandAll.Text = "Expand All";
                configListView.CollapseAll();
            }
        }

        internal class CustomGroupComparer : IComparer<GroupResult>
        {
            public int Compare(GroupResult x, GroupResult y)
            {

                int xResult = getPriority(x.Key.ToString());
                int yResult = getPriority(y.Key.ToString());
                if (xResult > yResult)
                {
                    //GroupResult y is stacked into top of the group i.e., Ascending.
                    //GroupResult x is stacked at the bottom of the group i.e., Descending.
                    return 1;
                }
                else if (xResult < yResult)
                {
                    //GroupResult x is stacked into top of the group i.e., Ascending.
                    //GroupResult y is stacked at the bottom of the group i.e., Descending.
                    return -1;
                }

                return 0;
            }

            private int getPriority(string group)
            {
                switch(group)
                {
                    case "Uncategorised":
                        {
                            return 0;
                        }
                    case "Hardware":
                        {
                            return 1;
                        }
                    case "Temperature":
                        {
                            return 2;
                        }
                    case "Standby":
                        {
                            return 3;
                        }
                    case "Fill":
                        {
                            return 4;
                        }
                    case "Empty":
                        {
                            return 5;
                        }
                    case "Float":
                        {
                            return 6;
                        }
                    case "Reservoir":
                        {
                            return 7;
                        }
                    case "EXO to EXO":
                    case "Orbit-Orbit":
                        {
                            return 8;
                        }
                    case "Skimmer":
                        {
                            return 9;
                        }
                    case "Dosing":
                        {
                            return 10;
                        }
                    case "Lighting":
                        {
                            return 11;
                        }
                    case "Wash Down":
                        {
                            return 12;
                        }
                    case "Plus":
                        {
                            return 20;
                        }
                    case "Default":
                        {
                            return 21;
                        }
                    case "Misc":
                        {
                            return 22;
                        }

                    default:
                        {
                            return 99;
                        }
                }

            }

        }

        protected override bool OnBackButtonPressed()
        {
            viewModel.OnExit.Execute(null);
            return true;
        }
    }
}