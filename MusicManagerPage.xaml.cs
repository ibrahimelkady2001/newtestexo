using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Sftp;

using System.Diagnostics;
using Microsoft.Maui.Controls.Xaml;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EXOApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MusicManagerPage : ContentPage
    {



        public MusicManagerPage()
        {
            InitializeComponent();
            //this.BindingContext = this;
            NavigationPage.SetHasNavigationBar(this, false);



        }

       



        


        




        


        
        protected override bool OnBackButtonPressed()
        {
            _ = App.Current.MainPage.Navigation.PopAsync();
            return true;
        }


    }

}