using EXOApp.Helpers;
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
    public partial class SettingsPageV2 : ContentPage
    {
        SettingsViewModelV2 viewModel;
        private readonly Helpers.MessageInterface _messageInterface;


        public SettingsPageV2()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            viewModel = (Models.SettingsViewModelV2)BindingContext;
            viewModel.fadeInOutEvent += FadeEvent;
        }

        public void temperatureClick(object sender, EventArgs args)
        {
            // Preferences.Set("orbit_IP", orbitList);

        }

        public void loggingClick(object sender, EventArgs args)
        {

        }

        protected override bool OnBackButtonPressed()
        {
            viewModel.exitCommand.Execute(null);
            return true;
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