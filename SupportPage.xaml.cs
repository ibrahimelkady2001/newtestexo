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
    public partial class SupportPage : ContentPage
    {
        SupportViewModel viewModel;

        public SupportPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            viewModel = (Models.SupportViewModel)BindingContext;
            viewModel.fadeInOutEvent += FadeEvent;

        }



        private async void FadeEvent()
        {
            transition.Opacity = 0;
            transition.IsVisible = true;
            await transition.FadeTo(0.75, 250);
            await transition.FadeTo(0, 250);
            transition.Opacity = 0;
            transition.IsVisible = false;
        }



    }
}