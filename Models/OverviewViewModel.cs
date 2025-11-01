using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Storage;
using Plugin.Maui.Audio;

namespace EXOApp.Models
{
    /// <summary>
    /// Unused, Prototype version of the front end
    /// </summary>
     class OverviewViewModel :INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public Color color { get; set; }
        public IList<Pod> orbits { get; set; }
        bool borderRed = false;
        private Color normalColor = Color.FromArgb("F0F0F0");
        Stream soundAlert;
        bool alertPlaying = false;
        public ICommand OnExit { protected set; get; }
        private readonly Helpers.MessageInterface _messageInterface;
        bool running = true;
        public OverviewViewModel()
        {
            Color = normalColor;
            orbits = new ObservableCollection<Pod>();
            var assembly = typeof(App).GetTypeInfo().Assembly;
            soundAlert = assembly.GetManifestResourceStream("EXOApp.Assets.Audio.Alert.mp3");
            this._messageInterface = DependencyService.Get<Helpers.MessageInterface>();
            OnExit = new Command<string>(async (instruction) => await onExit());
            foreach (var pod in Globals.getList())
            {
                orbits.Add(new Pod(pod));
            }
            // TODO Xamarin.Forms.Device.StartTimer is no longer supported. Use Microsoft.Maui.Dispatching.DispatcherExtensions.StartTimer instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                handleAssist();
                return running;
            });



        }

        private async Task onExit()
        {
            bool response = await _messageInterface.ShowAsyncAcceptCancel("Sign Out", "Do you want to Sign Out?", "Yes", "No");
            if (response)
            {  
                foreach(Pod pod in orbits)
                {
                    pod.running = false;
                }
                running = false;
                await App.Current.MainPage.Navigation.PopAsync();
            }
        }

        private void handleAssist()
        {
            bool notActive = true;
            Globals.assistAlarm = false;
            int[] alertList = new int[orbits.Count];
            for(int i = 0; i < orbits.Count; i++)
            {
                notActive = notActive && !orbits[i].AssistAlarm;
                Globals.assistAlarm = !notActive;
                if (orbits[i].AssistAlarm)
                {
                    alertList[i] = i;
                }
                else
                {
                    alertList[i] = -1;
                }
            }
            if (!notActive)
            {
                startAlertSound();
                if (borderRed)
                {
                    borderRed = false;
                    Color = normalColor;
                    foreach (int i in alertList)
                    {
                        if(i != -1)
                        {
                            orbits[i].changeAssistIndicator(Colors.White, false);
                        }
                    }
                }
                else
                {
                    borderRed = true;
                    Color = Colors.Red;
                    foreach (int i in alertList)
                    {
                        if (i != -1)
                        {
                            orbits[i].changeAssistIndicator(Colors.Red, true);
                        }
                    }
                }
            }
            else
            {
                stopAlertSound();
                if(borderRed)
                {
                    borderRed = false;
                    Color = normalColor;
                    for (int i = 0; i < orbits.Count; i++)
                    {
                        orbits[i].changeAssistIndicator(Colors.White, false);
                    }
                }
            }


        }
        public Color Color
        {
            set
            {
                if (color != value)
                {
                    color = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Color"));
                    }
                }
            }
            get
            {
                return color;
            }
        }
        private void startAlertSound()
        {
            if(alertPlaying)
            {
                return;
            }
            alertPlaying = true;
            var audio = AudioManager.Current.CreatePlayer(soundAlert);

            audio.Loop = true;

          
            audio.Play();
        }

        private void stopAlertSound()
        {
            alertPlaying = false;
            var audio = OverviewPageV2.audio;
            audio.Stop();
        }




        internal class Pod : INotifyPropertyChanged
        {

            PodModel pod;
            string temperature;
            float temperatureNumber;
            string lastMode;
            string podStatus;
            bool currentResTemp = true;
            string tabString;
            string previousMode ="";
            public event PropertyChangedEventHandler PropertyChanged;
            public ICommand OrbitCommand { protected set; get; }
            public ICommand SettingsCommand { protected set; get; }
            public ICommand MaintenanceCommand { protected set; get; }
            public ICommand AlarmCommand { protected set; get; }
            public ICommand ExtraButtonCommand { protected set; get; }
            public ICommand MusicCommand { protected set; get; }
            public ICommand SwitchMenu { protected set; get; }

            bool buttonsEnabled { set; get; }
            public ImageSource iconImage { set; get; }
            private readonly Helpers.MessageInterface _messageInterface;
            public bool running = true;
            public Xamarin.CommunityToolkit.UI.Views.SideMenuState sideMenuState { get; set; }

            //UI
            bool waitingBar { set; get; }
            bool controlAvailable { set; get; }

            //Alarms
            bool assistAlarm { get; set; }
            bool h2o2Alarm { get; set; }
            bool podNotRespondingAlarm { get; set; }
            bool filterAlarm { get; set; }
            bool connectionAlarm { get; set; }
            bool uvAlarm { get; set; }
            bool assistAlarmTriangle { get; set; }
            string podTime = "0";
            int podMissCount = 0;
            int podFailureCount = 0;
            Color assistColor { get; set; }

            //FloatTimes
            ObservableCollection<string> floatTimeList = new ObservableCollection<string>();
            public ObservableCollection<string> FloatTimeList { get { return floatTimeList; } }
            public int selectedFloatTime { get; set; }
            public bool floatSliderVisible { get; set; }
            public int floatSliderValue { get; set; }
            public string floatSliderLabel { get; set; }


            //FillDelays
            ObservableCollection<string> fillDelayList = new ObservableCollection<string>();
            public ObservableCollection<string> FillDelayList { get { return fillDelayList; } }
            public int selectedFillDelay { get; set; }
            public bool fillSliderVisible { get; set; }
            public int fillSliderValue { get; set; }
            public string fillSliderLabel { get; set; }

            //Lighting Profiles
            ObservableCollection<string> lightProfileList = new ObservableCollection<string>();
            public ObservableCollection<string> LightProfileList { get { return lightProfileList; } }
            public int selectedLights { get; set; }


            //Music Tracks
            ObservableCollection<string> musicList = new ObservableCollection<string>();
            public ObservableCollection<string> MusicList { get { return musicList; } }
            public int selectedMusicStart { get; set; }
            public int selectedMusicEnd { get; set; }

            //Music Times
            ObservableCollection<string> musicProfileListStart = new ObservableCollection<string>();
            ObservableCollection<string> musicProfileListEnd = new ObservableCollection<string>();
            public int selectedMusicStartTime { get; set; }
            public int selectedMusicEndTime { get; set; }
            public bool musicSliderVisible { get; set; }
            public bool endMusicSelectVisible { get; set; }
            public int musicSliderStartValue { get; set; }
            public int musicSliderEndValue { get; set; }
            public string startMusicLabel { get; set; }
            public string endMusicLabel { get; set; }

            //Renders
            public bool f100;
            public bool f75;
            public bool f50;
            public bool f25;
            public bool doorOpen;
            public bool emptyLights;
            public bool fillLights;
            public bool floatLights;
            public bool saltMixLights;
            public bool skimLights;
            public bool standbyLights;
            public bool stopLights;

            //InfoTab
            int infoIteration = 0;
            bool floatFinished = false;
            bool animationInProgress = false;
            public string status { get; set; }
            public float infoTempDial { get; set; }
            public string infoMode { get; set; }
            public float infoProgress { get; set; }

            public Pod(PodModel orb)
            {
                this._messageInterface = DependencyService.Get<Helpers.MessageInterface>();
                pod = orb;
                WaitingBar = false;
                ControlAvailable = true;
                OrbitCommand = new Command<string>(async (instruction) => await startFloat(instruction));
                SettingsCommand = new Command(async () => await settingsPage());
                MaintenanceCommand = new Command(async () => await maintenancePage());
                MusicCommand = new Command(async () => await musicPage());
                AlarmCommand = new Command<string>(async (alarm) => await doAlarms(alarm));
                ExtraButtonCommand = new Command<string>(async (instruction) => await extraButton(instruction));
                SwitchMenu = new Command(() => changeMenuState());
                pod.updateStatus();
                pod.getFilter();
                pod.getUV();
                doLists();
                this.podStatus = pod.getData("SysMode", true);
                generateTabString();
                decideTemperature();
                setIcon();
                handleButtons();
                handleAlarms();
                AssistColor = Colors.White;
                pod.updateSettings();
                SideMenuState = Xamarin.CommunityToolkit.UI.Views.SideMenuState.LeftMenuShown;
                handleRender();
                handleInfoMode();
                int currentTimer = 0;
                // TODO Xamarin.Forms.Device.StartTimer is no longer supported. Use Microsoft.Maui.Dispatching.DispatcherExtensions.StartTimer instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    handleInfoMode();
                    if (currentTimer == 0)
                    {
                        Task.Run(async () =>
                        {
                            await updateStatusTask();
                            if (!animationInProgress)
                            {
                                //Check temperature dial animations with lengthened timer
                                animationInProgress = true;
                                await animateInfoDial(temperatureNumber, InfoTempDial);
                            }
                            this.PodStatus = pod.getData("SysMode", true);
                            setIcon();
                            generateTabString();
                            decideTemperature();
                            handleButtons();
                            handleAlarms();
                            handleRender();
                        });
                    }
                    currentTimer++;
                    if(!controlAvailable && currentTimer >= 2)
                    {
                        currentTimer = 0;
                    }
                    else if(currentTimer >= 5)
                    {
                        currentTimer = 0;
                    }
                    return running;
                });
            }

            async Task updateStatusTask()
            {
                await Task.Run(() =>
                {
                    bool success = pod.updateStatus();
                    if (!success)
                    {
                        podFailureCount++;
                    }
                    else
                    {
                        podFailureCount = 0;
                    }
                });
            }

            async Task settingsPage()
            {
                Globals.setCurrentPod(pod);
                await App.Current.MainPage.Navigation.PushAsync(new SettingsPage());
            }

            async Task musicPage()
            {
                Globals.setCurrentPod(pod);
                await App.Current.MainPage.Navigation.PushAsync(new MusicManagerPage());
            }

            async Task maintenancePage()
            {
                Globals.setCurrentPod(pod);
                await App.Current.MainPage.Navigation.PushAsync(new Maintenance());
            }
            async Task extraButton(string instruction)
            {
                switch(instruction)
                {
                    case "Restart":
                        {
                            bool response = await _messageInterface.ShowAsyncAcceptCancel("Alert", "Do you want to restart this Pod? It will take around 60 seconds", "Yes", "No");
                            if (response)
                            {
                                pod.restart();
                            }
                            break;
                        }
                    case "Shutdown":
                        {
                            bool response = await _messageInterface.ShowAsyncAcceptCancel("Alert", "Do you want to shutdown this Pod?", "Yes", "No");
                            if (response)
                            {
                                response = await _messageInterface.ShowAsyncAcceptCancel("Alert", "The Pod will be off until manually restarted. Do you wish to continue", "Yes", "No");
                                if(response)
                                {
                                    pod.shutdown();
                                }
                            }
                            break;
                        }
                    case "Quit":
                        {
                            bool response = await _messageInterface.ShowAsyncAcceptCancel("Alert", "Are you sure you want to close the App?", "Yes", "No");
                            if (response)
                            {
                                Globals.Quit();
                            }
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            async Task doAlarms(string alarm)
            {
                switch(alarm)
                {
                    case "Assist":
                        {
                            bool response = await _messageInterface.ShowAsyncAcceptCancel("Alert", "Do you want to dismiss this alert?", "Yes", "No");
                            if(response)
                            {
                                if(!pod.dismissAssist())
                                {
                                    await _messageInterface.ShowAsyncOK("Error 1012", "Unable to set float parameters. Please try again", "Close");
                                }
                                AssistAlarm = false;
                            }
                            break;
                        }
                    case "H2O2":
                        {
                            await _messageInterface.ShowAsyncOK("Error 2020", "Please check dosing Hydrogen Peroxide Levels.", "Close");
                            break;
                        }
                    case "Responding":
                        {
                            await _messageInterface.ShowAsyncOK("Error 1011", "Pod not responding, please restart your Pod.", "Close");
                            break;
                        }
                    case "UV":
                        {
                            await _messageInterface.ShowAsyncOK("Error 2030", "UV bulb needs replacing. Please replace your UV bulb.", "Close");
                            break;
                        }
                    case "Filter":
                        {
                            await _messageInterface.ShowAsyncOK("Error 2010", "Filters need replacing. Please replace your filters", "Close");
                            break;
                        }
                    case "Connection":
                        {
                            await _messageInterface.ShowAsyncOK("Error 1020", "Pod is offline. Please check your Pod is on and connected to your router", "Close");
                            break;
                        }

                }
            }

            void changeMenuState()
            {
                if(SideMenuState == Xamarin.CommunityToolkit.UI.Views.SideMenuState.MainViewShown)
                {
                    SideMenuState = Xamarin.CommunityToolkit.UI.Views.SideMenuState.RightMenuShown;
                }
                else
                {
                    SideMenuState = Xamarin.CommunityToolkit.UI.Views.SideMenuState.MainViewShown;
                }
                
            }

            void handleAlarms()
            {
                bool alarmTest = true;
                if(alarmTest)
                {
                    AssistAlarm = false;
                    H2o2Alarm = false;
                    PodNotRespondingAlarm = false;
                    FilterAlarm = false;
                    UvAlarm = false;
                    ConnectionAlarm = false;
                    return;
                }
                
                if(pod.getData("Assist", true) == "1")
                {
                    AssistAlarm = true;
                }
                else
                {
                    AssistAlarm = false;
                }

                if (pod.getData("H2O2Bottle", true) == "1")
                {
                    H2o2Alarm = true;
                }
                else
                {
                    H2o2Alarm = false;
                }
                if (pod.getData("lastUpdateSecond", true) == podTime)
                {
                    podMissCount++;
                    if(podMissCount > 10)
                    {
                        PodNotRespondingAlarm = true;
                    }
                }
                else
                {
                    podMissCount = 0;
                    PodNotRespondingAlarm = false;
                    podTime = pod.getData("lastUpdateSecond", true);
                }

                if(pod.filter.AddMonths(1) < DateTime.Now)
                {
                    FilterAlarm = true;
                }
                else
                {
                    FilterAlarm = false;
                }

                if (pod.uv.AddYears(1) < DateTime.Now)
                {
                    UvAlarm = true;
                }
                else
                {
                    UvAlarm = false;
                }

                if(podFailureCount > 5)
                {
                    ConnectionAlarm = true;
                }
                else
                {
                    ConnectionAlarm = false;
                }


            }

            private void handleRender()
            {
                if(previousMode != podStatus)
                {
                    previousMode = podStatus;
                    EmptyLights = false;
                    FillLights = false;
                    FloatLights = false;
                    SaltMixLights = false;
                    SkimLights = false;
                    StandbyLights = false;
                    StopLights = false;
                    switch (podStatus)
                    {
                        case "Float":
                            FloatLights = true;
                            break;
                        case "Idle":
                            StopLights = true;
                            break;
                        case "Empty":
                            EmptyLights = true;
                            break;
                        case "Fill":
                        case "Pre-Prime":
                            FillLights = true;
                            break;
                        case "Standby":
                            StandbyLights = true;
                            break;
                        case "Salt-Mix":
                            SaltMixLights = true;
                            break;
                        case "Skimmer":
                            SkimLights = true;
                            break;
                        default:
                            break;
                    }
                }

                if(pod.getData("DoorState", true) == "Open")
                {
                    DoorOpen = true;
                }
                else
                {
                    DoorOpen = false;
                }
                if(pod.getData("level_sensor_enabled", false) == "1")
                {
                    handleRenderRes(pod.getData("ResTankLevel", true), true);
                }
                else
                {
                    handleRenderRes(pod.getData("FloatSwitch", true), false);
                }

            }

            private void handleRenderRes(string level, bool levelSensor)
            {
                //Debug.WriteLine("RenderResMethod");
                //Debug.WriteLine(level);
                if(levelSensor)
                {
                    float levelInt;
                    float.TryParse(level, out levelInt);
                    if(levelInt < 0.3)
                    {
                        F100 = false;
                        F75 = false;
                        F50 = false;
                        F25 = false;
                    }
                    else if(levelInt < 0.6)
                    {
                        F100 = false;
                        F75 = false;
                        F50 = false;

                        F25 = true;
                    }
                    else if (levelInt < 0.9)
                    {
                        F100 = false;
                        F75 = false;
                        F25 = false;

                        F50 = true;

                    }
                    else if (levelInt < 1.5)
                    {
                        F100 = false;
                        F50 = false;
                        F25 = false;

                        F75 = true;
                    }
                    else
                    {
                        F75 = false;
                        F50 = false;
                        F25 = false;

                        F100 = true;
                    }
                }
                else
                {
                    if(level == "Up")
                    {
                        F75 = false;
                        F50 = false;
                        F25 = false;
                        F100 = true;
                    }
                    else
                    {
                        F75 = false;
                        F50 = false;
                        F25 = false;
                        F100 = false;
                    }
                }
            }

            async Task animateInfoDial(float temperatureNumber, float startingAngle)
            {
                float endingAngle;

                if (temperatureNumber > 41.5)
                {
                    endingAngle = 135;
                }
                else if (temperatureNumber < 32.5)
                {
                    endingAngle = -135;
                }
                else
                {
                    endingAngle = (temperatureNumber - 37) * 30;
                }
                //
                //ADD don't run animation on no change
                //
                DateTime start = DateTime.Now;
                float deltaT = 0;
                float oldDeltaT = -1;
                await Task.Run(() =>
                {
                    while((DateTime.Now - start).Seconds < 1)
                    {
                        deltaT = (DateTime.Now - start).Milliseconds;
                        if(deltaT > oldDeltaT)
                        {
                            InfoTempDial = Utility.Lerp(startingAngle, endingAngle, deltaT,1000,out _);
                            oldDeltaT = deltaT;
                        }
                    }
                });
                animationInProgress = false;
            }

            private void handleInfoMode()
            {
                if(PodNotRespondingAlarm || ConnectionAlarm)
                {
                    InfoMode = "Pod Connection Lost";
                }
                if (pod.getData("ElapsedModeTime", true) != "0")
                {
                    float remainingTime = float.Parse(pod.getData("TotalModeTime", true)) - float.Parse(pod.getData("ElapsedModeTime", true));
                    switch (infoIteration)
                    {
                        case 0:
                            {
                                InfoMode = "Finishes at " + DateTime.Now.AddSeconds(remainingTime).ToString("HH:mm");
                                break;
                            }
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                            {
                                float timeRemainingMins = remainingTime / 60;
                                timeRemainingMins = (float)Math.Floor(timeRemainingMins);
                                if(timeRemainingMins == 0)
                                {
                                    InfoMode = remainingTime + " Seconds Remaining";
                                    floatFinished = true;
                                }
                                else
                                {
                                    if(timeRemainingMins > 1)
                                    {
                                        InfoMode = timeRemainingMins + " Minutes Remaining";
                                    }
                                    else
                                    {
                                        InfoMode = timeRemainingMins + " Minute Remaining";
                                    }
                                }
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                    infoIteration++;
                    if (infoIteration > 7)
                    {
                        infoIteration = 0;
                    }
                    float tempProgress = float.Parse(pod.getData("ElapsedModeTime", true)) / float.Parse(pod.getData("TotalModeTime", true));
                    if(tempProgress > 1)
                    {
                        tempProgress = 1;
                    }
                    InfoProgress = tempProgress;
                }
                else if(floatFinished || infoIteration > 7)
                {
                    InfoMode = "Float Finished";
                    if(floatFinished)
                    {
                        infoIteration = 8;
                    }
                    infoIteration++;
                    floatFinished = false;
                    if(infoIteration > 15)
                    {
                        infoIteration = 0;
                    }
                }
                else
                {
                    InfoProgress = 1;
                    InfoMode = "";
                }
               
            }

            public void changeAssistIndicator(Color color, bool alarmActive)
            {
                AssistColor = color;
                AssistAlarmTriangle = alarmActive;
            }

            private void decideTemperature()
            {
                int temp = 0;
                string unroundedTemp ="";
                switch (podStatus)
                {
                    case "Float":
                    case "Skimmer":
                    case "EXO-EXO":
                        unroundedTemp = pod.getData("TempOrbit", true);
                        currentResTemp = false;
                        break;
                    case "Idle":
                    case "Empty":
                        if (currentResTemp)
                        {
                            unroundedTemp = pod.getData("TempRes", true);
                        }
                        else
                        {
                            unroundedTemp = pod.getData("TempOrbit", true);
                        }
                        break;
                    default:
                        unroundedTemp = pod.getData("TempRes", true);
                        currentResTemp = true;
                        break;
                }
                temperatureNumber = float.Parse(unroundedTemp);
                temp = (int)Math.Round(temperatureNumber);
                if (Preferences.Get("Celsius", false))
                {
                    temp = ((temp * 9) / 5) + 32;
                    Temperature = temp.ToString() + "°F";
                }
                else
                {
                    Temperature = temp.ToString() + "°C";
                }
                
            }

            private void generateTabString()
            {
                this.TabString = "EXO - " + pod.podNumber;// + "\n" + podStatus;

            }

            private string generateMusicProfileString()
            {
                if (MusicProfileListStart[SelectedMusicStartTime] != "Custom")
                {
                    return MusicProfileListStart[SelectedMusicStartTime] + "/" + musicProfileListEnd[selectedMusicEndTime];
                }
                else
                {
                    return "Custom";

                }

            }

            private async Task startFloat(string instruction)
            {
                string currentMode = pod.getData("SysMode", true);
                if (parseModeInstruction(currentMode, instruction))
                {
                    await _messageInterface.ShowAsyncOK("Error 4010", "You are already in " + currentMode, "Close");
                    return;
                }
                if (currentMode != "Standby" && currentMode != "Idle")
                {
                    bool cont = await _messageInterface.ShowAsyncAcceptCancel("Notice", "Are you sure you want to interrupt the current " + currentMode, "Yes", "No");
                    if (!cont)
                    {
                        return;
                    }
                        
                }

                if (instruction == "start")
                {
                    if (temperatureNumber < 31)
                    {
                        bool cont = await _messageInterface.ShowAsyncAcceptCancel("Error 3000", "Solution Temperature is low, Do you want to proceed?", "Yes", "No");
                        if(!cont)
                        {
                            return;
                        }
                    }
                    bool settingsChanged = true;
                    Debug.WriteLine(MusicProfileListStart[SelectedMusicStartTime]);
                    if ((musicProfileListEnd == null || MusicProfileListStart == null || MusicProfileListStart.Count == 0 || musicProfileListEnd.Count == 0) && MusicProfileListStart[SelectedMusicStartTime] != "Custom")
                    {
                        await _messageInterface.ShowAsyncOK("Error 4000", "Please fill in all float parameters", "Close");
                        return;
                    }
                    //Add error for custom music longer than float
                    //if(MusicSliderEndValue + MusicSliderStartValue )
                    //
                    if (pod.floatTimes[SelectedFloatTime].name == "Custom")
                    {
                        settingsChanged = settingsChanged && pod.customFloatTimeChange((FloatSliderValue * 5 * 60).ToString());
                    }
                    if (pod.musicProfiles[selectedMusicStart] == "Custom")
                    {
                        string musicProfileStart = (musicSliderStartValue * 60).ToString();
                        string musicProfileEnd = (musicSliderEndValue * 60).ToString();
                        settingsChanged = settingsChanged && pod.customMusicChange(musicProfileStart, musicProfileEnd);
                    }
                    if (pod.fillDelays[SelectedFillDelay].name == "Custom")
                    {
                        settingsChanged = settingsChanged && pod.customFillDelayChange((fillSliderValue * 60).ToString());
                    }
                    settingsChanged = settingsChanged && pod.changeFloatCurrentSettings(pod.floatTimes[SelectedFloatTime].name, pod.lightingProfiles[selectedLights], generateMusicProfileString(), pod.musicTracks[selectedMusicStart].name, pod.musicTracks[selectedMusicEnd].name, pod.fillDelays[SelectedFillDelay].name);
                    if(!settingsChanged)
                    {
                        await _messageInterface.ShowAsyncOK("Error 1012", "Unable to set float parameters. Please try again", "Close");
                        return;
                    }
                }
                ControlAvailable = false;
                WaitingBar = true;
                bool pass = true;
                pod.startOrbit(instruction);
                await Task.Run(async () =>
                {
                    string mode = pod.getData("SysMode", true);
                    string newMode = mode;
                    int counter = 0;
                    while (mode == newMode)
                    {
                        newMode = pod.getData("SysMode", true);
                        counter++;
                        await Task.Delay(100);
                        if (counter > 250)
                        {
                            pass = false;
                            break;
                        }
                    }
                    return;
                });
                ControlAvailable = true;
                WaitingBar = false;
                if(!pass)
                {
                    await _messageInterface.ShowAsyncOK("Error 1010", "Unable to set Mode, Please restart your Pod", "Close");
                }
            }

            bool parseModeInstruction(string mode, string instruction)
            {
                switch(instruction)
                {
                    case "start":
                        {
                            break;
                        }
                    case "end":
                        {
                            break;
                        }
                    case "stop":
                        {
                            if(mode == "Idle")
                            {
                                return true;
                            }
                            break;
                        }
                    case "standby":
                        {
                            if (mode == "Standby")
                            {
                                return true;
                            }
                            break;
                        }
                    case "fill":
                        {
                            if (mode == "Fill")
                            {
                                return true;
                            }
                            break;
                        }
                    case "empty":
                        {
                            if (mode == "Empty")
                            {
                                return true;
                            }
                            break;
                        }
                    case "float":
                        {
                            if (mode == "Float")
                            {
                                return true;
                            }
                            break;
                        }
                    case "EXO-EXO":
                        {
                            if (mode == "EXO-EXO")
                            {
                                return true;
                            }
                            break;
                        }
                    case "filtering":
                        {
                            if (mode == "Filter")
                            {
                                return true;
                            }
                            break;
                        }
                    case "skimmer":
                        {
                            if (mode == "Skimmer")
                            {
                                return true;
                            }
                            break;
                        }
                    case "dosing":
                        {
                            if (mode == "Dosing")
                            {
                                return true;
                            }
                            break;
                        }
                    case "fill-empty":
                        {
                            break;
                        }
                    case "salt-mix":
                        {
                            break;
                        }
                    default:
                        {
                            return false;
                            break;
                        }
                }
                return false;

            }


            private void doLists()
            {
                //Float Times
                pod.getFloatTimes();
                List<string> temp = new List<string>();
                int counter = 0;
                while (counter < pod.floatTimes.Count)
                {
                    temp.Add(pod.floatTimes[counter].display_name);
                    counter++;
                }
                floatTimeList = new ObservableCollection<string>(temp);

                //Fill Delay
                pod.getFillDelay();
                temp = new List<string>();
                counter = 0;
                while (counter < pod.fillDelays.Count)
                {
                    temp.Add(pod.fillDelays[counter].display_name);
                    counter++;
                }
                fillDelayList = new ObservableCollection<string>(temp);

                //Light Profiles
                pod.getLightingProfiles();
                lightProfileList = new ObservableCollection<string>(pod.lightingProfiles);

                //MusicTracks
                pod.getMusicTracks();
                temp = new List<string>();
                foreach (MusicTrack m in pod.musicTracks)
                {
                    string name = m.name.Replace("df_", "");
                    name = name.Replace("_", " ");
                    name = name.Replace(".mp3", "");
                    temp.Add(name);
                    Debug.WriteLine(name);
                }
                musicList = new ObservableCollection<string>(temp);

                ///MusicTimes
                pod.getMusicProfiles();
                MusicProfileListStart = new ObservableCollection<string>(pod.getMusicProfileListStart(floatTimeList[SelectedFloatTime]));
                SelectedMusicStartTime = 0;
                MusicProfileListEnd = new ObservableCollection<string>(pod.getMusicProfileListEnd(MusicProfileListStart[SelectedMusicStartTime]));
                EndMusicSelectVisible = true;

            }




            private ImageSource getImage(string file)
            {
                string assemblyName = GetType().GetTypeInfo().Assembly.GetName().Name;
                return ImageSource.FromResource(assemblyName + ".Assets.Images." + file, typeof(OverviewViewModel).GetTypeInfo().Assembly);
            }
            private void setIcon()
            {
                if (lastMode == podStatus)
                {
                    return;
                }
                lastMode = podStatus;
                switch (podStatus)
                {
                    case "Float":
                        IconImage = getImage("podFloat.png");
                        break;
                    case "Idle":
                        IconImage = getImage("podStop.png");
                        break;
                    case "Empty":
                        IconImage = getImage("podEmpty.png");
                        break;
                    case "Fill":
                    case "EXO-EXO":
                        IconImage = getImage("podFill.png");
                        break;
                    case "Standby":
                        IconImage = getImage("podStandby.png");
                        break;
                    default:
                        IconImage = getImage("podicon.png");
                        break;
                }
            }
            private void handleButtons()
            {
                if (podStatus == "Idle" || podStatus == "Standby")
                {
                    ButtonsEnabled = true;
                }
                else
                {
                    ButtonsEnabled = false;
                }
            }


            //---------------------------------------------------------------------------------------------------
            // Model Live Updates
            //---------------------------------------------------------------------------------------------------




            public bool AssistAlarm
            {
                set
                {
                    if (assistAlarm != value)
                    {
                        assistAlarm = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("AssistAlarm"));
                        }
                    }
                }
                get
                {
                    return assistAlarm;
                }
            }
            public bool H2o2Alarm
            {
                set
                {
                    if (h2o2Alarm != value)
                    {
                        h2o2Alarm = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("H2o2Alarm"));
                        }
                    }
                }
                get
                {
                    return h2o2Alarm;
                }
            }
            public bool PodNotRespondingAlarm
            {
                set
                {
                    if (podNotRespondingAlarm != value)
                    {
                        podNotRespondingAlarm = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("PodNotRespondingAlarm"));
                        }
                    }
                }
                get
                {
                    return podNotRespondingAlarm;
                }
            }
            public bool FilterAlarm
            {
                set
                {
                    if (filterAlarm != value)
                    {
                        filterAlarm = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("FilterAlarm"));
                        }
                    }
                }
                get
                {
                    return filterAlarm;
                }
            }
            public bool ConnectionAlarm
            {
                set
                {
                    if (connectionAlarm != value)
                    {
                        connectionAlarm = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("ConnectionAlarm"));
                        }
                    }
                }
                get
                {
                    return connectionAlarm;
                }
            }
            public bool UvAlarm
            {
                set
                {
                    if (uvAlarm != value)
                    {
                        uvAlarm = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("UvAlarm"));
                        }
                    }
                }
                get
                {
                    return uvAlarm;
                }
            }
            public bool AssistAlarmTriangle
            {
                set
                {
                    if (assistAlarmTriangle != value)
                    {
                        assistAlarmTriangle = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("AssistAlarmTriangle"));
                        }
                    }
                }
                get
                {
                    return assistAlarmTriangle;
                }
            }



            public Color AssistColor
            {
                set
                {
                    if (assistColor != value)
                    {
                        assistColor = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("AssistColor"));
                        }
                    }
                }
                get
                {
                    return assistColor;
                }
            }


            public bool WaitingBar
            {
                set
                {
                    if (waitingBar != value)
                    {
                        waitingBar = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("WaitingBar"));
                        }
                    }
                }
                get
                {
                    return waitingBar;
                }
            }
            public bool ControlAvailable
            {
                set
                {
                    if (controlAvailable != value)
                    {
                        controlAvailable = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("ControlAvailable"));
                        }
                    }
                }
                get
                {
                    return controlAvailable;
                }
            }

            public string Temperature
            {
                set
                {
                    if (temperature != value)
                    {
                        temperature = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("Temperature"));
                        }
                    }
                }
                get
                {
                    return temperature;
                }
            }
            public string PodStatus
            {
                set
                {
                    if (podStatus != value)
                    {
                        podStatus = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("PodStatus"));
                        }
                    }
                }
                get
                {
                    return podStatus;
                }
            }
            public ImageSource IconImage
            {
                set
                {
                    if (iconImage != value)
                    {
                        iconImage = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("IconImage"));
                        }
                    }
                }
                get
                {
                    return iconImage;
                }

            }
            public string TabString
            {
                set
                {
                    if (tabString != value)
                    {
                        tabString = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("TabString"));
                        }
                    }
                }
                get
                {
                    return tabString;
                }
            }

            public bool ButtonsEnabled
            {
                set
                {
                    if (buttonsEnabled != value)
                    {
                        buttonsEnabled = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("ButtonsEnabled"));
                        }
                    }
                }
                get
                {
                    return buttonsEnabled;
                }
            }

            public bool FloatSliderVisible
            {
                set
                {
                    if (floatSliderVisible != value)
                    {
                        floatSliderVisible = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("FloatSliderVisible"));
                        }
                    }
                }
                get
                {
                    return floatSliderVisible;
                }
            }
            public bool FillSliderVisible
            {
                set
                {
                    if (fillSliderVisible != value)
                    {
                        fillSliderVisible = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("FillSliderVisible"));
                        }
                    }
                }
                get
                {
                    return fillSliderVisible;
                }
            }
            public bool MusicSliderVisible
            {
                set
                {
                    if (musicSliderVisible != value)
                    {
                        musicSliderVisible = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("MusicSliderVisible"));
                        }
                    }
                }
                get
                {
                    return musicSliderVisible;
                }
            }
            public bool EndMusicSelectVisible
            {
                set
                {
                    if (endMusicSelectVisible != value)
                    {
                        endMusicSelectVisible = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("EndMusicSelectVisible"));
                        }
                    }
                }
                get
                {
                    return endMusicSelectVisible;
                }
            }

            public int SelectedFloatTime
            {
                set
                {
                    if (selectedFloatTime != value)
                    {
                        selectedFloatTime = value;
                        if (floatTimeList[selectedFloatTime] == "Custom")
                        {
                            FloatSliderVisible = true;
                            FloatSliderValue = 2;
                        }
                        else
                        {
                            FloatSliderVisible = false;
                        }
                        if (value != -1)
                        {
                            MusicProfileListStart = new ObservableCollection<string>(pod.getMusicProfileListStart(floatTimeList[SelectedFloatTime]));
                            SelectedMusicStartTime = 0;
                            MusicProfileListEnd = new ObservableCollection<string>(pod.getMusicProfileListEnd(MusicProfileListStart[SelectedMusicStartTime]));
                            EndMusicSelectVisible = true;
                        }
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("SelectedFloatTime"));
                        }
                    }
                }
                get
                {
                    return selectedFloatTime;
                }
            }
            public int SelectedFillDelay
            {
                set
                {
                    if (selectedFillDelay != value)
                    {
                        selectedFillDelay = value;
                        if (fillDelayList[selectedFillDelay] == "Custom")
                        {
                            FillSliderVisible = true;
                            FillSliderValue = 1;
                        }
                        else
                        {
                            FillSliderVisible = false;
                        }
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("SelectedFillDelay"));
                        }
                    }
                }
                get
                {
                    return selectedFillDelay;
                }
            }
            public int SelectedMusicStartTime
            {
                set
                {
                    if (selectedMusicStartTime != value)
                    {
                        selectedMusicStartTime = value;
                        if (value != -1 && MusicProfileListStart[selectedMusicStartTime] == "Custom")
                        {
                            MusicSliderVisible = true;
                            EndMusicSelectVisible = false;
                            MusicSliderStartValue = 1;
                            MusicSliderEndValue = 1;
                        }
                        else
                        {
                            MusicSliderVisible = false;
                            EndMusicSelectVisible = true;
                        }
                        if (value != 0 && value != -1)
                        {
                            MusicProfileListEnd = new ObservableCollection<string>(pod.getMusicProfileListEnd(MusicProfileListStart[SelectedMusicStartTime]));
                        }
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("SelectedMusicStartTime"));
                        }
                    }
                }
                get
                {
                    return selectedMusicStartTime;
                }
            }

            public ObservableCollection<string> MusicProfileListStart
            {
                set
                {
                    if (musicProfileListStart != value)
                    {
                        musicProfileListStart = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("MusicProfileListStart"));
                        }
                    }
                }
                get
                {
                    return musicProfileListStart;
                }
            }
            public ObservableCollection<string> MusicProfileListEnd
            {
                set
                {
                    if (musicProfileListEnd != value)
                    {
                        musicProfileListEnd = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("MusicProfileListEnd"));
                        }
                    }
                }
                get
                {
                    return musicProfileListEnd;
                }
            }

            public int FloatSliderValue
            {
                set
                {
                    if (floatSliderValue != value)
                    {
                        floatSliderValue = value;
                        FloatSliderLabel = (value * 5).ToString() + " Minutes";
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("FloatSliderValue"));
                        }
                    }
                }
                get
                {
                    return floatSliderValue;
                }
            }
            public int FillSliderValue
            {
                set
                {
                    if (fillSliderValue != value)
                    {
                        fillSliderValue = value;
                        FillSliderLabel = value.ToString() + " Minutes";
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("FillSliderValue"));
                        }
                    }
                }
                get
                {
                    return fillSliderValue;
                }
            }
            public int MusicSliderStartValue
            {
                set
                {
                    if (musicSliderStartValue != value)
                    {
                        musicSliderStartValue = value;
                        StartMusicLabel = value.ToString() + " Minutes";
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("MusicSliderStartValue"));
                        }
                    }
                }
                get
                {
                    return musicSliderStartValue;
                }
            }
            public int MusicSliderEndValue
            {
                set
                {
                    if (musicSliderEndValue != value)
                    {
                        musicSliderEndValue = value;
                        EndMusicLabel = value.ToString() + " Minutes";
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("MusicSliderEndValue"));
                        }
                    }
                }
                get
                {
                    return musicSliderEndValue;
                }
            }

            public string FloatSliderLabel
            {
                set
                {
                    if (floatSliderLabel != value)
                    {
                        floatSliderLabel = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("FloatSliderLabel"));
                        }
                    }
                }
                get
                {
                    return floatSliderLabel;
                }
            }
            public string FillSliderLabel
            {
                set
                {
                    if (fillSliderLabel != value)
                    {
                        fillSliderLabel = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("FillSliderLabel"));
                        }
                    }
                }
                get
                {
                    return fillSliderLabel;
                }
            }
            public string StartMusicLabel
            {
                set
                {
                    if (startMusicLabel != value)
                    {
                        startMusicLabel = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("StartMusicLabel"));
                        }
                    }
                }
                get
                {
                    return startMusicLabel;
                }
            }
            public string EndMusicLabel
            {
                set
                {
                    if (endMusicLabel != value)
                    {
                        endMusicLabel = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("EndMusicLabel"));
                        }
                    }
                }
                get
                {
                    return endMusicLabel;
                }
            }

            public bool F100
            {
                set
                {
                    if (f100 != value)
                    {
                        f100 = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("F100"));
                        }
                    }
                }
                get
                {
                    return f100;
                }
            }
            public bool F75
            {
                set
                {
                    if (f75 != value)
                    {
                        f75 = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("F75"));
                        }
                    }
                }
                get
                {
                    return f75;
                }
            }
            public bool F50
            {
                set
                {
                    if (f50 != value)
                    {
                        f50 = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("F50"));
                        }
                    }
                }
                get
                {
                    return f50;
                }
            }
            public bool F25
            {
                set
                {
                    if (f25 != value)
                    {
                        f25 = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("F25"));
                        }
                    }
                }
                get
                {
                    return f25;
                }
            }
            public bool DoorOpen
            {
                set
                {
                    if (doorOpen != value)
                    {
                        doorOpen = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("DoorOpen"));
                        }
                    }
                }
                get
                {
                    return doorOpen;
                }
            }
            public bool EmptyLights
            {
                set
                {
                    if (emptyLights != value)
                    {
                        emptyLights = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("EmptyLights"));
                        }
                    }
                }
                get
                {
                    return emptyLights;
                }
            }
            public bool FillLights
            {
                set
                {
                    if (fillLights != value)
                    {
                        fillLights = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("FillLights"));
                        }
                    }
                }
                get
                {
                    return fillLights;
                }
            }
            public bool FloatLights
            {
                set
                {
                    if (floatLights != value)
                    {
                        floatLights = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("FloatLights"));
                        }
                    }
                }
                get
                {
                    return floatLights;
                }
            }
            public bool SaltMixLights
            {
                set
                {
                    if (saltMixLights != value)
                    {
                        saltMixLights = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("SaltMixLights"));
                        }
                    }
                }
                get
                {
                    return saltMixLights;
                }
            }
            public bool SkimLights
            {
                set
                {
                    if (skimLights != value)
                    {
                        skimLights = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("SkimLights"));
                        }
                    }
                }
                get
                {
                    return skimLights;
                }
            }
            public bool StandbyLights
            {
                set
                {
                    if (standbyLights != value)
                    {
                        standbyLights = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("StandbyLights"));
                        }
                    }
                }
                get
                {
                    return standbyLights;
                }
            }
            public bool StopLights
            {
                set
                {
                    if (stopLights != value)
                    {
                        stopLights = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("StopLights"));
                        }
                    }
                }
                get
                {
                    return stopLights;
                }
            }


            public string Status
            {
                set
                {
                    if (status != value)
                    {
                        status = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("Status"));
                        }
                    }
                }
                get
                {
                    return status;
                }
            }
            public float InfoTempDial
            {
                set
                {
                    if (infoTempDial != value)
                    {
                        infoTempDial = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("InfoTempDial"));
                        }
                    }
                }
                get
                {
                    return infoTempDial;
                }
            }

            public string InfoMode
            {
                set
                {
                    if (infoMode != value)
                    {
                        infoMode = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("InfoMode"));
                        }
                    }
                }
                get
                {
                    return infoMode;
                }
            }

            public float InfoProgress
            {
                set
                {
                    if (infoProgress != value)
                    {
                        infoProgress = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("InfoProgress"));
                        }
                    }
                }
                get
                {
                    return infoProgress;
                }
            }


            public Xamarin.CommunityToolkit.UI.Views.SideMenuState SideMenuState
            {
                set
                {
                    if (sideMenuState != value)
                    {
                        sideMenuState = value;

                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("SideMenuState"));
                        }
                    }
                }
                get
                {
                    return sideMenuState;
                }
            }
        }

    }

}
