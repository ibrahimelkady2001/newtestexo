using EXOApp.Models;
using System;
using Microsoft.Maui.Controls.Xaml;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using CommunityToolkit.Maui.Extensions;
using System.IO;
using Syncfusion.Maui.ListView;

using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Converters;
using CommunityToolkit.Maui.ImageSources;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Layouts;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Networking;
using Plugin.Maui.Audio;
using Xamarin.CommunityToolkit.Extensions;
namespace EXOApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OverviewPageV2 : ContentPage
    {
        OverviewViewModelV2 viewModel;
        string currentRenderState;
        bool assistActive = false;
        Stream soundAlert;
        bool menuShown = false;
        bool menuTransition = false;
        bool podTransition = false;
    public static  IAudioPlayer audio;

        public OverviewPageV2()
        {
            InitializeComponent();
            settingsButton.Text = "•\n•\n•";
            currentRenderState = "empty";
            NavigationPage.SetHasNavigationBar(this, false);
            viewModel = (Models.OverviewViewModelV2)BindingContext;
            viewModel.fadeInOutEvent += FadeEvent;
            viewModel.handleRenderEvent += changeRender;
            viewModel.assistEvent += assistAlarm;
            viewModel.fadeMenu += hideShowSettings;
            viewModel.handleLevelEvent += handleLevel;
            changePodScreen.Opacity = 0;
            changePodScreen.IsVisible = false;
            var current = Connectivity.NetworkAccess;
            if (current != NetworkAccess.Internet)
            {
                InternetWarning.IsVisible = true;
            }
         
            soundAlert = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("EXOApp.Assets.Audio.Alert.mp3");
            audio = AudioManager.Current.CreatePlayer(soundAlert);
           
            audio.Loop = true;
            Start();

            TemperatureContainer.SizeChanged += (s, e) =>
            {
                TemperatureFrame.HeightRequest = TemperatureContainer.Width;

                if (TemperatureContainer.Width > TemperatureContainer.Height)
                {
                    TemperatureFrame.WidthRequest = TemperatureContainer.Height;
                }
                else
                {
                    TemperatureFrame.WidthRequest = TemperatureContainer.Width;
                }
            };
            TimeContainer.SizeChanged += (s, e) =>
            {
                TimeFrame.HeightRequest = TimeContainer.Width;

                if (TimeContainer.Width > TimeContainer.Height)
                {
                    TimeFrame.WidthRequest = TimeContainer.Height;
                }
                else
                {
                    TimeFrame.WidthRequest = TimeContainer.Width;
                }
            };
        }

        void menuButtonClick(object sender, EventArgs args)
        {
            /*if(sideMenuView.State == SideMenuState.MainViewShown)
            {
                changeButton.ScaleXTo(-1, 250);
                sideMenuView.State = SideMenuState.RightMenuShown;
            }
            else
            {
                changeButton.ScaleXTo(1, 250);
                sideMenuView.State = SideMenuState.MainViewShown;
            }
            */
        }

        void floatTimeChange(object sender, EventArgs args)
        {
            if(viewModel == null)
            {
                return;
            }
            viewModel.setHeatedFloatEnabled();
        }

        void lightingProfileChange(object sender, EventArgs args)
        {
            if (viewModel == null)
            {
                return;
            }
            viewModel.setContinousLightingAvailable();
        }

        private async void assistAlarm(bool on)
        {
            if(!on)
            {
                assistActive = false;
                return;
            }
            if(assistActive)
            {
                return;
            }
           
            assistActive = true;
            audio.Play();
            await Task.Run(async () =>
            {
                while (assistActive)
                {
                    await podBar.ColorTo(Colors.Red, 500);
                    await podBar.ColorTo(Color.FromArgb("1c3e70"), 500);
                }
                await podBar.ColorTo(Color.FromArgb("1c3e70"), 500);

            });
            audio.Stop();

        }
        
        double CustomEase(double t)
        {
            return t == 0 || t == 1 ? t : (int)(5 * t) / 5.0;
        }

        void showSettingsClicked(object sender, EventArgs arg)
        {
            hideShowSettings();
        }

        async void hideShowSettings()
        {
            if (menuTransition)
            {
                return;
            }
            menuTransition = true;
            if (menuShown == false)
            {

                Side_Menu.Opacity = 0;
                Side_Menu.IsVisible = true;
                Side_Menu.IsEnabled = true;
                await Side_Menu.FadeTo(1, 400, (Easing)CustomEase);
                menuShown = true;
                settingsButton.Text = "•\n•\n•";
                //settingsButton.Text = "\n\n\n→";
            }
            else
            {
                Side_Menu.Opacity = 255;
                await Side_Menu.FadeTo(0, 400, (Easing)CustomEase);
                Side_Menu.IsVisible = false;
                Side_Menu.IsEnabled = false;
                menuShown = false;
                settingsButton.Text = "•\n•\n•";
                //settingsButton.Text = "←";
            }
            menuTransition = false;
        }

        private void handleLevel(float level)
        {
            if(level < 0.2)
            {
                emptyImage.IsVisible = true;
                lowImage.IsVisible = false;
                midImage.IsVisible = false;
                fullImage.IsVisible = false;
                levelText.Text = "Empty";
            }
            else if (level < 0.8)
            {
                emptyImage.IsVisible = false;
                lowImage.IsVisible = true;
                midImage.IsVisible = false;
                fullImage.IsVisible = false;
                levelText.Text = "Low";
            }
            else if (level < 1.45)
            {
                emptyImage.IsVisible = false;
                lowImage.IsVisible = false;
                midImage.IsVisible = true;
                fullImage.IsVisible = false;
                levelText.Text = "Mid";
            }
            else
            {
                emptyImage.IsVisible = false;
                lowImage.IsVisible = false;
                midImage.IsVisible = false;
                fullImage.IsVisible = true;
                levelText.Text = "Full";
            }
        }

        private async void changeRender(string state)
        {
            if (state == currentRenderState)
            {
                return;
            }
            renderHide.IsVisible = true;
            await renderHide.FadeTo(1, 200, (Easing)CustomEase);
            if (state[0] != currentRenderState[0])
            {
                if (state[0] == 'o')
                {
                    changeDoor(true);
                    if (state[1] != currentRenderState[1])
                    {
                        if (state[1] == 'f')
                        {
                            changeInterior(true);
                        }
                        else
                        {
                            changeInterior(false);
                        }
                    }
                }
                else
                {
                    changeDoor(false);
                    changeInterior(false);

                }
            }

            if (state[2] != currentRenderState[2])
            {
                if (state[2] == 'w')
                {
                    assistWarning.IsVisible = true;
                }
                else
                {
                    assistWarning.IsVisible = false;
                }
            }
            if (state.Remove(0,3) != currentRenderState.Remove(0, 3))
            {
                changeBase(state.Remove(0, 3));
            }
            currentRenderState = state;
            await renderHide.FadeTo(0, 200, (Easing)CustomEase);
            renderHide.IsVisible = false;

        }

        private void changeDoor(bool doorOpen)
        {
            if(doorOpen)
            {
                renderDoor.IsVisible = true;
            }
            else
            {
                renderDoor.IsVisible = false;
            }
        }

        private void changeInterior(bool filled)
        {
            if (filled)
            {
                renderInterior.IsVisible = true;
            }
            else
            {
                renderInterior.IsVisible = false;
            }
        }

        protected override bool OnBackButtonPressed()
        {
            viewModel.OnExit.Execute(null);
            return true;
        }

        private void changeBase(string colour)
        {
            renderBase.Source = getImage("Renders.Base." + colour +"-Lights.png");
        }

        private ImageSource getImage(string file)
        {
            string assemblyName = GetType().GetTypeInfo().Assembly.GetName().Name;
            return ImageSource.FromResource(assemblyName + ".Assets.Images." + file, typeof(OverviewViewModel).GetTypeInfo().Assembly);
        }

        private async void Start()
        {
            //await Task.Delay(1000);
            //sideMenuView.State = SideMenuState.MainViewShown;
        }

        private async void FadeEvent()
        {
            if(podTransition == true)
            {
                return;
            }
            podTransition = true;
            changePodScreen.Opacity = 0;
            changePodScreen.IsVisible = true;
            await changePodScreen.FadeTo(0.75, 250);
            await changePodScreen.FadeTo(0, 250);
            changePodScreen.Opacity = 0;
            changePodScreen.IsVisible = false;
            podTransition = false;
        }



        /*
private void CarouselView_CurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
{
    prevFloatTimeIndex = -1;
    if (viewModel == null)
    {
        return;
    }
    viewModel.setHeatedFloatEnabled();
}

private void decreaseTime(object sender, EventArgs arg)
{
    Debug.WriteLine("Starting Lower");
    Debug.WriteLine(floatTimeView.Position);
    if (scrollRunning)
    {
        return;
    }
    if(floatTimeView.Position == 0)
    {
        return;
    }

    scrollRunning = true;
    if (prevFloatTimeIndex == floatTimeView.Position)
    {
        floatTimeView.ScrollTo(prevFloatTimeIndex - 1);
    }
    else
    {
        prevFloatTimeIndex = floatTimeView.Position - 1;
        floatTimeView.ScrollTo(floatTimeView.Position - 1);
    }
    scrollRunning = false;
}

private void increaseTime(object sender, EventArgs arg)
{
    Debug.WriteLine("Starting Upper");
    Debug.WriteLine(floatTimeView.Position);
    if (scrollRunning)
    {
        return;
    }
    scrollRunning = true;
    if (prevFloatTimeIndex == floatTimeView.Position)
    {
        floatTimeView.ScrollTo(prevFloatTimeIndex + 2);
    }
    else
    {
        //prevFloatTimeIndex = floatTimeView.Position;
        Debug.WriteLine(prevFloatTimeIndex);
        floatTimeView.ScrollTo(floatTimeView.Position + 1);
    }
    scrollRunning = false;
    Debug.WriteLine("Ending Upper");
}
*/

    }
}