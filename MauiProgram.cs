using CommunityToolkit.Maui;
using EXOApp;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Compatibility.Hosting;
using Syncfusion.Maui.Core.Hosting;

namespace MauiApp6;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder  .UseMauiApp<App>()
                    .UseMauiCommunityToolkit() .ConfigureSyncfusionCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Poppins-Regular.ttf", "Poppins");
                fonts.AddFont("Poppins-Bold.ttf", "PoppinsBold");
            });

// .ConfigureMauiHandlers(handlers =>
//             {
// #if ANDROID
//             //Add Legacy MediaElement Renderer
//             handlers.AddCompatibilityRenderer(
//                 typeof(Xamarin.CommunityToolkit.UI.Views.SideMenuView),
//                 typeof(Xamarin.CommunityToolkit.Android.UI.Views.SideMenuViewRenderer));
//                     #elif IOS
                    
//  handlers.AddCompatibilityRenderer(
//                 typeof(Xamarin.CommunityToolkit.UI.Views.SideMenuView),
//                 typeof(Xamarin.CommunityToolkit.iOS.UI.Views.SideMenuViewRenderer));

//                     #endif
//             })

        return builder.Build();
    }
}