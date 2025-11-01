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
    public partial class OrbitOverviewPage : TabbedPage
    {
        public OrbitOverviewPage ()
        {
            NavigationPage.SetHasNavigationBar(this, false);

            InitializeComponent();
        }
        protected override bool OnBackButtonPressed()
        {
            var vm = (Models.OverviewViewModel)BindingContext;
            vm.OnExit.Execute(null);
            return true;
        }
    }
}