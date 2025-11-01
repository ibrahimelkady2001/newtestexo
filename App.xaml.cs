using System;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

// [assembly: ExportFont("Poppins-Regular.ttf", Alias = "Poppins")]
// [assembly: ExportFont("Poppins-Bold.ttf", Alias = "PoppinsBold")]


namespace EXOApp
{
    public partial class App : Application
    {
        public App()
        {
            DependencyService.Register<Helpers.MessageInterface, Helpers.MessageService>();
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH9fd3VcQ2NYUEB0WEpWYEg=");
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
