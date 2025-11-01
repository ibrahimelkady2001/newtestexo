using System;
using Microsoft.Maui.Controls.Xaml;
using EXOApp.Models;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Inputs;

namespace EXOApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UpdatePodPage : ContentPage
    {
        UpdateViewModel viewModel;

        public UpdatePodPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            var but = new SfButton();

            viewModel = (Models.UpdateViewModel)BindingContext;
     
            //Debug.WriteLine(Globals.getCurrentPod().checkSQLexists("config", "name = 'fill_run_time_end'"));
            //Debug.WriteLine(Globals.getCurrentPod().checkSQLexists("config", "name = 'fill_run_time_nd'"));
            //updateSQL11(Globals.getCurrentPod(), GetVersionDatabase(Globals.getCurrentPod()));
            //updatePod(Globals.getCurrentPod());
        }


        protected override bool OnBackButtonPressed()
        {
            viewModel.OnExit.Execute(null);
            return true;
        }

  

    }
}