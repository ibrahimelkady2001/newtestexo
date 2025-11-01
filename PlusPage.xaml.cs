using EXOApp.Models;
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
    public partial class PlusPage : ContentPage
    {
        PlusViewModel viewModel;


        public PlusPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            viewModel = (Models.PlusViewModel)BindingContext;
            


        }


        protected override bool OnBackButtonPressed()
        {
            viewModel.OnExit.Execute(null);
            return true;
        }


    }
}