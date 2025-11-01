
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using EXOApp.Helpers;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Storage;

namespace EXOApp.Models
{
    /// <summary>
    /// View model for the main dashboard.
    /// </summary>
    class OverviewViewModelV2 : INotifyPropertyChanged
    {


        public ICommand DashboardCommand { protected set; get; }
        public ICommand ChangePodCommand { protected set; get; }
        public ICommand MenuCommand { protected set; get; }
        public ICommand AlarmCommand { protected set; get; }
        public ICommand OnExit { protected set; get; }
        public ICommand OnTimeChangeCommand { protected set; get; }

        public ICommand FloatTimeCommand { protected set; get; }

        private readonly Helpers.MessageInterface _messageInterface;

        public Action fadeInOutEvent;
        public Action<string> handleRenderEvent;
        public Action<bool> assistEvent;
        public Action fadeMenu;
        public Action<float> handleLevelEvent;


        public event PropertyChangedEventHandler PropertyChanged;
        PodListItem currentPod;
        PodListItem oldPod;
        ObservableCollection<PodListItem> podSideList = new ObservableCollection<PodListItem>();



        ObservableCollection<MenuButton> menuButtons = new ObservableCollection<MenuButton>();
        public ObservableCollection<MenuButton> MenuButtons { get { return menuButtons; } }

        ObservableCollection<MenuButton> powerButtons = new ObservableCollection<MenuButton>();
        public ObservableCollection<MenuButton> PowerButtons { get { return powerButtons; } }



        ObservableCollection<SelectionItem> floatTimesList = new ObservableCollection<SelectionItem>();
        public ObservableCollection<SelectionItem> FloatTimesList { get { return floatTimesList; } }
        SelectionItem selectedFloatTime;


        ObservableCollection<SelectionItem> fillDelayList = new ObservableCollection<SelectionItem>();
        public ObservableCollection<SelectionItem> FillDelayList { get { return fillDelayList; } }
        SelectionItem selectedFillDelay;


        ObservableCollection<SelectionItem> musicStartTime = new ObservableCollection<SelectionItem>();
        public ObservableCollection<SelectionItem> MusicStartTime { get { return musicStartTime; } }
        SelectionItem selectedMusicStartTime;


        ObservableCollection<SelectionItem> musicEndTime = new ObservableCollection<SelectionItem>();
        public ObservableCollection<SelectionItem> MusicEndTime { get { return musicEndTime; } }
        SelectionItem selectedMusicEndTime;



        ObservableCollection<string> lightingProfile = new ObservableCollection<string>();
        public ObservableCollection<string> LightingProfile { get { return lightingProfile; } }
        string selectedLightingProfile;



        ObservableCollection<MusicTrack> musicTracks = new ObservableCollection<MusicTrack>();
        public ObservableCollection<MusicTrack> MusicTracks { get { return musicTracks; } }
        MusicTrack selectedMusicTrackStart;
        MusicTrack selectedMusicTrackEnd;


        bool selectedheatedFloat;
        bool heatedFloatEnabled;

        bool selectedContinousLights;
        bool continousLightsEnabled;


        ObservableCollection<Alarm> alarms = new ObservableCollection<Alarm>();
        public ObservableCollection<Alarm> Alarms { get { return alarms; } }
        List<Alarm> alarmList;

        SmartButton topLeftButton;
        SmartButton topRightButton;
        SmartButton bottomLeftButton;
        SmartButton bottomCentreButton;
        SmartButton bottomRightButton;
        SmartButton bottomLargeButton;


        bool lockScreen;
        string temperatureLabel;
        string timeLabel;

        string ipLabel;
        string versionLabel;
        string worldTimeLabel;

        int timerCounter = 12;
        int currentState = 0;

        bool timerRunning = true;
        bool updateRunning = false;
        bool floatTimeChangeRunning = false;

        bool remainingTimeActive = false;
        string floatTimeLabel;
        int timeLabelToggle = 0;
        int maxTimeLabel = 1;
        bool floatTimeLocked = false;

        int floatTimePosition;
        string currentFloatTime;
        /// <summary>
        /// Constructor for main dashboard
        /// </summary>
        public OverviewViewModelV2()
        {
            DashboardCommand = new Command<string>((button) => buttonFunctionHandler(button));
            ChangePodCommand = new Command(async() => await switchCurrentPod());
            OnExit = new Command(async () => await onExit());
            MenuCommand = new Command<string>(async(command) => await menuButtonClick(command));
            AlarmCommand = new Command<string>(async (name) => await alarmFunction(name));
            OnTimeChangeCommand = new Command(() => setHeatedFloatEnabled());
            FloatTimeCommand = new Command<string>((upwards) => floatTimeChange(upwards));

            TopLeftButton = new SmartButton("Start", true, Color.FromArgb("1c3e70"), true, DashboardCommand);
            TopRightButton = new SmartButton("Stop", true, Color.FromArgb("1c3e70"), true, DashboardCommand);
            BottomLeftButton= new SmartButton("Empty", false, Color.FromArgb("1c3e70"), true, DashboardCommand);
            BottomCentreButton = new SmartButton("Fill", false, Color.FromArgb("1c3e70"), true, DashboardCommand);
            BottomRightButton = new SmartButton("Cleaning", false, Color.FromArgb("1c3e70"), true, DashboardCommand);
            BottomLargeButton = new SmartButton("Manual Commands", true, Color.FromArgb("1c3e70"), true, DashboardCommand);
            buttonAppearanceHandler(currentState);
            LockScreen = false;
            this._messageInterface = DependencyService.Get<Helpers.MessageInterface>();
            foreach (PodModel orb in Globals.getList())
            {
                podSideList.Add(new PodListItem(orb, _messageInterface));
            }

            CurrentPod = PodSideList[0];
            currentPod.Selected = true;
            oldPod = CurrentPod;
            VersionLabel = currentPod.version;
            IpLabel = currentPod.ip.Replace("http://","").Replace("/","");
            WorldTimeLabel = DateTime.Now.ToString("g");
            HeatedFloatEnabled = true;
            generateLists();
            generateMenuButtons();
            prepareAlarms();
            // TODO Xamarin.Forms.Device.StartTimer is no longer supported. Use Microsoft.Maui.Dispatching.DispatcherExtensions.StartTimer instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            Device.StartTimer(TimeSpan.FromSeconds(0.5), () =>
            {
                _ = update();
                return timerRunning;
            });
        }

        /// <summary>
        /// Updates the dashboard. The selected Pod is refreshed at a higher rate than unselected pods
        /// </summary>
        async Task update()
        {
            if (updateRunning)
            {
                return;
            }
            updateRunning = true;

            //Updates Every 0.5 Seconds
            if (timerCounter % 1 == 0)
            {
                buttonStateManager(-1);
                loadingScreen(CurrentPod.busy);
                if(CurrentPod.busy)
                {
                    await CurrentPod.update();
                    handleRenderEvent.Invoke(currentPod.getStringRender());
                }
            }
            //Updates Every 2 Seconds
            if (timerCounter % 4 == 0 && timerCounter != 12)
            {
                await CurrentPod.update();
                alarmHandler();
                handleRenderEvent.Invoke(currentPod.getStringRender());
                handleLevelEvent.Invoke(currentPod.level);
                TemperatureLabel = CurrentPod.getTemperature();
                WorldTimeLabel = DateTime.Now.ToString("d/M/y HH:mm");
                handleFloatTimer();
            }
            //Updates Every 6 Seconds
            else if (timerCounter == 12)
            {
                handleTimeLabelToggle();

                List<Task<int>> updates = new List<Task<int>>();

                foreach(PodListItem orb in PodSideList)
                {
                    updates.Add(orb.update());
                }
                while (updates.Any())
                {
                    Task<int> finishedTask = await Task.WhenAny(updates);
                    updates.Remove(finishedTask);
                }
            }
            timerCounter++;
            if (timerCounter > 12)
            {
                timerCounter = 0;
            }
            updateRunning = false;
        }

        /// <summary>
        /// Increments the time label counter and prevents it from going over the maximum. This changes what's displayed on float time remaining.
        /// </summary>
        void handleTimeLabelToggle()
        {
            if(timeLabelToggle >= maxTimeLabel)
            {
                timeLabelToggle = 0;
            }
            else
            {
                timeLabelToggle++;
            }
        }

        /// <summary>
        /// Prepares all the alarms, only needs to be run once
        /// </summary>
        void prepareAlarms()
        {
            alarmList = new List<Alarm>();
            alarmList.Add(new Alarm("h2o2Alarm", AlarmCommand));
            alarmList.Add(new Alarm("assistAlarm", AlarmCommand));
            alarmList.Add(new Alarm("filterAlarmLow", AlarmCommand));
            alarmList.Add(new Alarm("filterAlarmReplace", AlarmCommand));
            alarmList.Add(new Alarm("uvAlarmLow", AlarmCommand));
            alarmList.Add(new Alarm("uvAlarmReplace", AlarmCommand));
            alarmList.Add(new Alarm("podNotConnectedAlarm", AlarmCommand));
            alarmList.Add(new Alarm("backendBENotRespondingAlarm", AlarmCommand));
            alarmList.Add(new Alarm("wipeOrbitAlarm", AlarmCommand));
            alarmList.Add(new Alarm("stopOrbitAlarm", AlarmCommand));
        }

        /// <summary>
        /// Handles which alarms are active. Active alarms are added to a list to display on the view.
        /// </summary>
        void alarmHandler()
        {
            indiviudalAlarmHandler(alarmList[0], currentPod.h2o2Alarm);
            indiviudalAlarmHandler(alarmList[1], currentPod.assistAlarm);
            indiviudalAlarmHandler(alarmList[2], currentPod.filterLowAlarm);
            indiviudalAlarmHandler(alarmList[3], currentPod.filterReplaceAlarm);
            indiviudalAlarmHandler(alarmList[4], currentPod.uvLowAlarm);
            indiviudalAlarmHandler(alarmList[5], currentPod.uvReplaceAlarm);
            indiviudalAlarmHandler(alarmList[6], currentPod.podNotConnectedAlarm);
            indiviudalAlarmHandler(alarmList[7], currentPod.backendBENotRespondingAlarm);
            indiviudalAlarmHandler(alarmList[8], currentPod.wipeOrbitAlarm);
            indiviudalAlarmHandler(alarmList[9], currentPod.stopOrbitAlarm);

            bool assist = false;
            foreach(PodListItem orb in podSideList)
            {
                assist = assist || orb.assistAlarm;
            }
            assistEvent.Invoke(assist);
        }

        /// <summary>
        /// Checks whether is an alarm is active or inactive.
        /// </summary>
        void indiviudalAlarmHandler(Alarm alarm, bool active)
        {
            if (!alarms.Contains(alarm) && active)
            {
                alarms.Add(alarm);
            }
            else if (alarms.Contains(alarm) && !active)
            {
                alarms.Remove(alarm);
            }
        }

        /// <summary>
        /// Checks whether the lighting profile has a continuous variant available
        /// </summary>
        public void setContinousLightingAvailable()
        {
            if(SelectedLightingProfile == null)
            {
                Debug.WriteLine("Selected Lighting Profile is null");
                return;
            }    
            switch(SelectedLightingProfile)
            {
                case "Sunset/Sunrise":
                case "Aurora":
                case "Night Sky":
                case "Warm Light":
                    {
                        ContinousLightsEnabled = true;
                        break;
                    }
                default:
                    {
                        ContinousLightsEnabled = false;
                        SelectedContinousLights = false;
                        break;
                    }

            }
        }

        /// <summary>
        /// Modifies the lighting profile string to use the continuous version.
        /// </summary>
        string handleContinousLights()
        {
            if(!SelectedContinousLights)
            {
                return SelectedLightingProfile;
            }
            else
            {
                switch(SelectedLightingProfile)
                {
                    case "Sunset/Sunrise":
                        {
                            return "Sunset/Sunrise Continuous"; 
                        }
                    case "Aurora":
                        {
                            return "Aurora Continuous"; 
                        }
                    case "Night Sky":
                        {
                            return "Night Sky Continuous";
                        }
                    case "Warm Light":
                        {
                            return "Warm Light Continuous";
                        }
                    default:
                        {
                            _ = _messageInterface.ShowAsyncOK("Error 4311", "Error Finding Continous Lighting Profile\nReverting to Normal Lighting\nPlease contact Wellness Support", "Ok");
                            return SelectedLightingProfile;
                        }
                }
            }
        }

        /// <summary>
        /// Enables heated float to be selected for floats 60 minutes or longer
        /// </summary>
        public void setHeatedFloatEnabled()
        {
            if(SelectedFloatTime == null)
            {
                return;
            }    
            string currentTime = SelectedFloatTime.Name;
            if(currentTime.Contains("secs"))
            {
                SelectedheatedFloat = false;
                HeatedFloatEnabled = false;
                return;
            }
            currentTime = currentTime.Replace("_mins","");
            currentTime = currentTime.Replace("_min", "");

            int time;
            if(!int.TryParse(currentTime, out time))
            {
                _messageInterface.ShowAsyncOK("Error 4200", "Heated Float Error\nPlease contact Wellness support quoting the error code\nand selected float time", "Close");
                SelectedheatedFloat = false;
                HeatedFloatEnabled = false;
            }
            if(time > 59)
            {
                HeatedFloatEnabled = true;
            }
            else
            {
                SelectedheatedFloat = false;
                HeatedFloatEnabled = false;
            }
        }

        /// <summary>
        /// Generates the list of float parameters displayed on the view
        /// </summary>
        void generateLists()
        {
            FloatTimesList.Add(new SelectionItem("660_secs", "Demo"));
            FloatTimesList.Add(new SelectionItem("30_mins", "30:00"));
            FloatTimesList.Add(new SelectionItem("45_mins", "45:00"));
            FloatTimesList.Add(new SelectionItem("60_mins", "60:00"));
            FloatTimesList.Add(new SelectionItem("75_min", "75:00"));
            FloatTimesList.Add(new SelectionItem("90_mins", "90:00"));
            FloatTimesList.Add(new SelectionItem("120_mins", "120:00"));
            FloatTimesList.Add(new SelectionItem("150_mins", "150:00"));
            FloatTimesList.Add(new SelectionItem("180_mins", "180:00"));
            SelectedFloatTime = FloatTimesList[3];
            CurrentFloatTime = SelectedFloatTime.DisplayName;
            setHeatedFloatEnabled();

            FillDelayList.Add(new SelectionItem("NoDelay", "No Delay"));
            FillDelayList.Add(new SelectionItem("1Min", "1 Minute"));
            FillDelayList.Add(new SelectionItem("2Min", "2 Minutes"));
            FillDelayList.Add(new SelectionItem("4Min", "4 Minutes"));
            FillDelayList.Add(new SelectionItem("8Min", "8 Minutes"));
            SelectedFillDelay = FillDelayList[0];

            MusicStartTime.Add(new SelectionItem("00/", "No Music"));
            MusicStartTime.Add(new SelectionItem("05/", "5 Minutes"));
            MusicStartTime.Add(new SelectionItem("10/", "10 Minutes"));
            MusicStartTime.Add(new SelectionItem("15/", "15 Minutes"));
            MusicStartTime.Add(new SelectionItem("55/", "55 Minutes"));
            MusicStartTime.Add(new SelectionItem("Continuous/", "Continuous"));
            SelectedMusicStartTime = MusicStartTime[1];

            MusicEndTime.Add(new SelectionItem("05", "5 Minutes"));
            MusicEndTime.Add(new SelectionItem("10", "10 Minutes"));
            MusicEndTime.Add(new SelectionItem("15", "15 Minutes"));
            SelectedMusicEndTime = MusicEndTime[0];

            lightingProfile = currentPod.getLightingProfiles();
            List<string> toRemove = new List<string>();
            foreach(string s in lightingProfile)
            {
                if(s.Contains("Continuous") || s.Contains ("Everlasting"))
                {
                    toRemove.Add(s);
                }
            }
            foreach(string s in toRemove)
            {
                lightingProfile.Remove(s);
            }
            SelectedLightingProfile = lightingProfile[0];
            setContinousLightingAvailable();
            foreach(PodListItem orb in PodSideList)
            {
                orb.getMusicTracks();
            }
            musicTracks = currentPod.getMusicTracks();
            SelectedMusicTrackStart = musicTracks[1];
            SelectedMusicTrackEnd = musicTracks[1];
        }

        /// <summary>
        /// Creates buttons for the side menu. Each button checks the user privelege before adding to ensure correct functions are accessible
        /// </summary>
        void generateMenuButtons()
        {
            menuButtons = new ObservableCollection<MenuButton>();
            addMenuButtonsToMenuList("Settings", "Settings", 39);
            addMenuButtonsToMenuList("Dose", "Dose", 30);
            addMenuButtonsToMenuList("Hibernate", "Hibernate", 39);
            addMenuButtonsToMenuList("Lighting", "Lighting", 99);
            addMenuButtonsToMenuList("Logs", "Float Logs", 30);
            addMenuButtonsToMenuList("Music", "Music Manager", 39);
            addMenuButtonsToMenuList("Salt_Mix", "Salt Mix", 30);
            if(checkForWashDown())
            {
                addMenuButtonsToMenuList("DrainDown", "Wash Down", 30);
            }
            addMenuButtonsToMenuList("Support", "Troubleshooting", 0);
            addMenuButtonsToMenuList("Service_ModeFillPod", "Service - EXO", 99);
            addMenuButtonsToMenuList("Service_ModeFillRes", "Service - Res", 99);
            addMenuButtonsToMenuList("FillEmpty", "Fill Empty Mode", 99);
            addMenuButtonsToMenuList("DryFloat", "Dry Float", 99);
            addMenuButtonsToMenuList("Exo-Exo", "EXO-EXO", 99);
            addMenuButtonsToMenuList("Plus", "Plus Overview", 49);
            addMenuButtonsToMenuList("Config", "EXO Config", 99);
            addMenuButtonsToMenuList("Update", "Update EXO", 39);

            addMenuButtonsToPowerList("Close", "Close Menu", -1);
            addMenuButtonsToPowerList("LogOff", "Log Off", -1);
            addMenuButtonsToPowerList("Reboot", "Reboot", 30);
            addMenuButtonsToPowerList("Shutdown", "Shutdown", 30);
        }

        /// <summary>
        /// Check the current Pod to see if the wash down pump is available
        /// </summary>
        bool checkForWashDown()
        {
            foreach(PodModel pod in Globals.getList())
            {
                if(pod.getData("wash_down_available", false) == "1")
                {
                    return true;
                }

            }
            return false;
        }

        /// <summary>
        /// Adds individual buttons to the menu. If user Auth is not high enough, the button is not added
        /// </summary>
        /// <param name="image">Image used for the button</param>
        /// <param name="title">Button description</param>
        /// <param name="auth">Required user authority</param>
        void addMenuButtonsToMenuList(string image, string title, int auth)
        {
            if (Globals.getUserSecurityLevel() >= auth)
            {
                MenuButtons.Add(new MenuButton(getImage("FunctionIcons." + image +".png"), title, MenuCommand));
            }
        }

        /// <summary>
        /// Adds buttons to the menu in power section. If user Auth is not high enough, the button is not added
        /// </summary>
        /// <param name="image">Image used for the button</param>
        /// <param name="title">Button description</param>
        /// <param name="auth">Required user authority</param>
        void addMenuButtonsToPowerList(string image, string title, int auth)
        {
            if (Globals.getUserSecurityLevel() >= auth)
            {
                PowerButtons.Add(new MenuButton(getImage("FunctionIcons." + image + ".png"), title, MenuCommand));
            }
        }

        /// <summary>
        /// Method used to get the images used in the view
        /// </summary>
        /// <param name="file">File name of the image. Must be located under the images folder</param>
        /// <returns></returns>
        private ImageSource getImage(string file)
        {
            string assemblyName = GetType().GetTypeInfo().Assembly.GetName().Name;
            return ImageSource.FromResource(assemblyName + ".Assets.Images." + file, typeof(OverviewViewModel).GetTypeInfo().Assembly);
        }

        /// <summary>
        /// Check the length of the lighting profile and whether it is compatible with the current settings
        /// </summary>
        /// <returns>Returns an error string, else returns an empty string on success</returns>
        string checkLightingProfileLength()
        {
            switch(SelectedLightingProfile)
            {
                case "Skin Booster":
                case "Pain Reliever":
                    {
                        if(SelectedFloatTime.DisplayName == "Demo")
                        {
                            return "This Lighting Profile cannot be used with a demo float";
                        }
                        else if(SelectedMusicEndTime.Name == "05" && CurrentPod.getLightProfileSync())
                        {
                            return "Ending Music Length is too short for this lighting profile.\nIncrease ending music length or disable End of Float Lighting Music Sync";
                        }
                        else
                        {
                            return "";
                        }
                    }
                default:
                    return "";
            }
        }

        /// <summary>
        /// Unused, Checks if lighting profile sync is on for the longer music tracks
        /// </summary>
        bool checkLightingProfileMusic()
        {
            switch (SelectedLightingProfile)
            {
                case "Skin Booster":
                case "Pain Reliever":
                    {
                        if (CurrentPod.getLightProfileSync())
                        {
                            return false;
                        }
                        return true;
                    }
                default:
                    return true;
            }
        }

        /// <summary>
        /// Checks all the float parameters, if all is good, begins the start of a float.
        /// </summary>
        async Task startFloat()
        {
            string error = checkLightingProfileLength();
            if(error != "")
            {
                await _messageInterface.ShowAsyncOK("Alert", error, "Close");
                return;
            }
            if (!checkMusicLength())
            {
                await _messageInterface.ShowAsyncOK("Alert", "Music Length Is Longer than the float session\nPlease change the parameters to enusre music length is shorter than the float session", "Close");
                return;
            }
            if (currentPod.temperature < 33)
            {
                bool response = await _messageInterface.ShowAsyncAcceptCancel("Alert", "Solution Temperature is low. Do you wish to continue?", "Yes", "No");
                if (!response)
                {
                    return;
                }
            }
            int startVolume;
            int endVolume;
            if(new Version(currentPod.Pod.softwareVersion[0]) < new Version("1.2.2.3"))
            {
                _ = currentPod.startFloat(SelectedFloatTime.Name, handleContinousLights(), SelectedMusicStartTime.Name + SelectedMusicEndTime.Name, SelectedMusicTrackStart.name, SelectedMusicTrackEnd.name, SelectedFillDelay.Name, SelectedheatedFloat, SelectedFloatTime.DisplayName);
            }
            else
            {
                if (Preferences.Get("PerTrackVolume", false))
                {
                    startVolume = SelectedMusicTrackStart.defaultVolume;
                    endVolume = SelectedMusicTrackEnd.defaultVolume;
                    _ = currentPod.startFloat(SelectedFloatTime.Name, handleContinousLights(), SelectedMusicStartTime.Name + SelectedMusicEndTime.Name, SelectedMusicTrackStart.name, SelectedMusicTrackEnd.name, SelectedFillDelay.Name, SelectedheatedFloat, SelectedFloatTime.DisplayName, startVolume, endVolume);
                }
                else
                {
                    int vol;
                    if(!Int32.TryParse(currentPod.Pod.getData("volume_percentage", false), out vol))
                    {
                        vol = 100;
                        await _messageInterface.ShowAsyncOK("Error 4220", "Volume Percentage cannot be parsed - reverting to default (100%).\nPlease Contact Wellness Support with the Error Code", "Close");
                    }
                    startVolume = vol;
                    endVolume = vol;
                    _ = currentPod.startFloat(SelectedFloatTime.Name, handleContinousLights(), SelectedMusicStartTime.Name + SelectedMusicEndTime.Name, SelectedMusicTrackStart.name, SelectedMusicTrackEnd.name, SelectedFillDelay.Name, SelectedheatedFloat, SelectedFloatTime.DisplayName, startVolume, endVolume);
                }
            }



            handleFloatTimer();
            currentState = 4;
            currentPod.largeButtonText = "Float Starting";
            buttonStateManager(0);
        }


        /// <summary>
        /// Checks whether the music run time is longer than the selected float
        /// </summary>
        /// <returns>Returns true if music length is shorter than the float time </returns>
        bool checkMusicLength()
        {
            string floatTime = SelectedFloatTime.Name;
            floatTime = floatTime.Replace("_mins", "");
            floatTime = floatTime.Replace("_min", "");

            int floatTimeInt;
            if (SelectedFloatTime.DisplayName.Contains("Demo"))
            {
                floatTimeInt = 11;
            }
            else if (!int.TryParse(floatTime, out floatTimeInt))
            {
                _ = _messageInterface.ShowAsyncOK("Error 4201", "Music Length Error\nPlease contact Wellness support quoting the error code\nand selected music lengths\nFloat proceed with the selected parameters", "Close");
                return true;
            }

            string startMusicLength = SelectedMusicStartTime.Name;
            string endMusicLength = SelectedMusicEndTime.Name;
            startMusicLength = startMusicLength.Replace("/", "");
            int startMusicTimeInt;
            int endMusicTimeInt;
            if(startMusicLength.Contains("Continuous"))
            {
                startMusicTimeInt = 0;
            }
            else if(!int.TryParse(startMusicLength, out startMusicTimeInt))
            {
                _ = _messageInterface.ShowAsyncOK("Error 4202", "Music Length Error\nPlease contact Wellness support quoting the error code\nand selected music lengths\nFloat proceed with the selected parameters", "Close");

                return true;
            }
            if (!int.TryParse(endMusicLength, out endMusicTimeInt))
            {
                _ = _messageInterface.ShowAsyncOK("Error 4203", "Music Length Error\nPlease contact Wellness support quoting the error code\nand selected music lengths\nFloat proceed with the selected parameters", "Close");
                return true;
            }

            return floatTimeInt >= (endMusicTimeInt+startMusicTimeInt);
        }

        /// <summary>
        /// Handles the appearance of the buttons depending on the current state of the pod.
        /// </summary>
        /// <param name="state">The current state of the pod</param>
        void buttonAppearanceHandler(int state)
        {

            switch(state)
            {
                case 0: //Standby
                    {
                        TopLeftButton.setButton("Start", true, Color.FromArgb("1c3e70"), true); //Enabled Start
                        TopRightButton.setButton("Emergency Stop", true, Color.FromArgb("1c3e70"), true); //Enabled Stop
                        BottomLeftButton.setButton("Empty Pod", false, Color.FromArgb("1c3e70"), true); //Blocked
                        BottomCentreButton.setButton("Fill Pod", false, Color.FromArgb("1c3e70"), true); //Blocked
                        BottomRightButton.setButton("Cleaning", false, Color.FromArgb("1c3e70"), true); //Blocked
                        BottomLargeButton.setButton("Manual Commands", true, Color.FromArgb("1c3e70"), true); // Enabled Manual Commands
                        break;
                    }
                case 1: //Session in Progress
                    {
                        TopLeftButton.setButton("Start", true, Color.FromArgb("607b9f"), true);//Disabled Start
                        TopRightButton.setButton("Stop", true, Colors.Red, true);//Enabled Stop
                        BottomLeftButton.setButton("Empty Pod", false, Color.FromArgb("1c3e70"), true); //Blocked
                        BottomCentreButton.setButton("Fill Pod", false, Color.FromArgb("1c3e70"), true); //Blocked
                        BottomRightButton.setButton("Cleaning", false, Color.FromArgb("1c3e70"), true); //Blocked
                        BottomLargeButton.setButton(currentPod.largeButtonText, true, Color.FromArgb("1c3e70"), false);//Disabled Manual
                        break;
                    }
                case 2: //Manual Commands
                    {
                        TopLeftButton.setButton("Start", true, Color.FromArgb("607b9f"), true); //Disabled Start
                        TopRightButton.setButton("Back", true, Colors.DimGray, true); //Enabled Back
                        BottomLeftButton.setButton("Empty Pod", true, Color.FromArgb("1c3e70"), true); //Enabled Empty
                        BottomCentreButton.setButton("Standby", true, Color.FromArgb("607b9f"), true);//Disabled Standby
                        BottomRightButton.setButton("Cleaning", true, Color.FromArgb("1c3e70"), true);//Enabled Cleaning
                        BottomLargeButton.setButton("Session in Progress", false, Color.FromArgb("1c3e70"), false); //Disabled
                        break;
                    }
                case 3: //Pod Stopped
                    {
                        TopLeftButton.setButton("Start", true, Color.FromArgb("607b9f"), true);//Disabled Start
                        TopRightButton.setButton("Help", true, Color.FromArgb("1c3e70"), true); //Disabled Stop
                        BottomLeftButton.setButton("Empty Pod", true, Color.FromArgb("1c3e70"), true); //Enabled Empty
                        BottomCentreButton.setButton("Standby", true, Color.FromArgb("1c3e70"), true); //Enabled Standby
                        BottomRightButton.setButton("Cleaning", true, Color.FromArgb("607b9f"), true); //Disabled Empty
                        BottomLargeButton.setButton("Session in Progress", false, Color.FromArgb("1c3e70"), false); //Disabled
                        break;
                    }
                case 4: //Session Loading
                    {
                        TopLeftButton.setButton("Cancel", true, Colors.Red, true);//Enabled Cancel
                        TopRightButton.setButton("Stop", true, Color.FromArgb("1c3e70"), false); //Disabled
                        BottomLeftButton.setButton("Empty Pod", false, Color.FromArgb("1c3e70"), true); //Blocked
                        BottomCentreButton.setButton("Fill Pod", false, Color.FromArgb("1c3e70"), true); //Blocked
                        BottomRightButton.setButton("Cleaning", false, Color.FromArgb("1c3e70"), true); //Blocked
                        BottomLargeButton.setButton("Your session is loading...", true, Color.FromArgb("1c3e70"), false); //Disabled
                        break;
                    }
                case 5: //Salt Mix
                    {
                        TopLeftButton.setButton("Empty", true, Color.FromArgb("1c3e70"), true); //Enabled Empty
                        TopRightButton.setButton("Stop", true, Colors.Red, true);//Enabled Stop
                        BottomLeftButton.setButton("Empty Pod", false, Color.FromArgb("1c3e70"), true);//Blocked
                        BottomCentreButton.setButton("Fill Pod", false, Color.FromArgb("1c3e70"), true);//Blocked
                        BottomRightButton.setButton("Cleaning", false, Color.FromArgb("1c3e70"), true);//Blocked
                        BottomLargeButton.setButton("Salt Mix In Progress", true, Color.FromArgb("1c3e70"), false);//Disabled
                        break;
                    }
                case 6: //Wash Down and Hibernate
                    {
                        TopLeftButton.setButton("Standby", true, Color.FromArgb("1c3e70"), true); //Enabled Standby
                        TopRightButton.setButton("Stop", true, Colors.Red, true);//Enabled Stop
                        BottomLeftButton.setButton("Empty Pod", false, Color.FromArgb("1c3e70"), true);//Blocked
                        BottomCentreButton.setButton("Fill Pod", false, Color.FromArgb("1c3e70"), true);//Blocked
                        BottomRightButton.setButton("Cleaning", false, Color.FromArgb("1c3e70"), true);//Blocked
                        BottomLargeButton.setButton(currentPod.largeButtonText, true, Color.FromArgb("1c3e70"), false);//Disabled
                        break;
                    }
            }
        }

        /// <summary>
        /// Sends a command to start running a mode on the Pod
        /// </summary>
        /// <param name="mode">Mode to run</param>
        /// <param name="warning">When not empty, will display a warning before continuing</param>
        /// <returns></returns>
        async Task currentOrbStartMode(string mode, string warning = "")
        {
            bool response;
            if (warning == "")
            {
                response = true;
            }
            else
            {
                response = await _messageInterface.ShowAsyncAcceptCancel("Stop Current Mode", warning, "Yes", "No");

            }
            if (response)
            {
                CurrentPod.busy = true;
                loadingScreen(true);
                if (mode == "float")
                {
                    CurrentPod.dryFloat(SelectedFloatTime.Name, handleContinousLights(), SelectedMusicStartTime.Name + SelectedMusicEndTime.Name, SelectedMusicTrackStart.name, SelectedMusicTrackEnd.name, SelectedFillDelay.Name);
                }
                else
                {
                    _ = CurrentPod.startMode(mode);
                }
            }
        }

        /// <summary>
        /// Handles what each button does during the various button states
        /// </summary>
        /// <param name="button">The button which is being used</param>
        public void buttonFunctionHandler(string button)
        {
            switch(currentState)
            {
                case 0: //Standby
                    {
                        switch(button)
                        {
                            case "TopLeft": //Start
                                {
                                    _ = startFloat();
                                    break;
                                }
                            case "TopRight": //Stop
                                {
                                    _ = currentOrbStartMode("stop", "Are you sure you wish to Stop.\nThe EXO Pod will not heat the solution during this period");
                                    break;
                                }
                            case "BottomLarge": //Manual Commands
                                {
                                    currentState = 2;
                                    break;
                                }
                            default:
                                {
                                    Debug.WriteLine("Switch Error 0 Button Name");
                                    return;
                                }
                        }
                        break;
                    }
                case 1: //Session in Progress
                    {
                        switch (button)
                        {
                            case "TopLeft":
                                {
                                    _ = _messageInterface.ShowAsyncOK("Error 4011", "Please return the Pod to standby to begin a float", "Close");
                                    break;
                                }
                            case "TopRight": //Stop
                                {
                                    _ = currentOrbStartMode("stop", "Are you sure you wish to stop the current "+ currentPod.status +"?");
                                    break;
                                }
                            default:
                                {
                                    Debug.WriteLine("Switch Error 1 Button Name");
                                    return;
                                }
                        }
                        break;
                    }
                case 2:   //Manual Commands
                    {
                        switch (button)
                        {
                            case "TopLeft":
                                {
                                    _ = _messageInterface.ShowAsyncOK("Error 4013", "Please return to the previous menu to begin a float", "Close");
                                    break;
                                }
                            case "TopRight": //Back
                                {
                                    currentState = 0;
                                    break;
                                }
                            case "BottomLeft": //Empty
                                {
                                    _ = currentOrbStartMode("empty");
                                    currentPod.largeButtonText = "Empty in Progress";

                                    currentState = 1;
                                    break;
                                }
                            case "BottomCentre":
                                {
                                    _ = _messageInterface.ShowAsyncOK("Error 4012", "The Pod is already in standby", "Close");
                                    break;
                                }
                            case "BottomRight": //Skim
                                {
                                    _ = handleCleaning();
                                    break;
                                }
                            default:
                                {
                                    Debug.WriteLine("Switch Error 2 Button Name");
                                    return;
                                }
                        }
                        break;
                    }
                case 3:  //Pod Stopped
                    {
                        switch (button)
                        {
                            case "TopLeft":
                                {
                                    _ = _messageInterface.ShowAsyncOK("Error 4014", "Empty the Pod to start a float.\nIf the Pod is already empty, return the Pod to standby", "Close");
                                    break;
                                }
                            case "TopRight": //Help
                                {
                                    _ = _messageInterface.ShowAsyncOK("Help", "The Pod has been stopped\nTo resume, check if the Pod has solution in it.\nIf empty, click standby, otherwise click empty to empty the solution", "Close");
                                    break;
                                }
                            case "BottomLeft": //Empty
                                {
                                    _ = currentOrbStartMode("empty");
                                    currentPod.largeButtonText = "Empty in Progress";
                                    break;
                                }
                            case "BottomRight": //Skim
                                {
                                    _ = _messageInterface.ShowAsyncOK("Error 4015", "Please return the Pod to standby to begin cleaning.\nCleaning can be found under manual commands", "Close");
                                    break;
                                }
                            case "BottomCentre": //Standby
                                {
                                    if(currentPod.level<0.3)
                                    {
                                        _ = currentOrbStartMode("standby", "Not enough solution is detected inside the reservoir tank? \nAre you sure you want to enter Standby?");
                                    }
                                    else
                                    {
                                        _ = currentOrbStartMode("standby");
                                    }
                                    break;
                                }
                            default:
                                {
                                    Debug.WriteLine("Switch Error 3 Button Name");
                                    return;
                                }
                        }
                        break;
                    }
                case 4: //Pre Start Timing
                    {
                        switch (button)
                        {
                            case "TopLeft": //Cancel
                                {
                                    currentPod.cancel = true;
                                    currentState = 0;
                                    break;
                                }
                            default:
                                {
                                    Debug.WriteLine("Switch Error 4 Button Name");
                                    return;
                                }
                        }
                        break;
                    }
                case 5:
                    {
                        switch(button)
                        {
                            case "TopLeft":
                                {
                                    _ = currentOrbStartMode("empty");
                                    currentPod.largeButtonText = "Empty in Progress";
                                    break;
                                }
                            case "TopRight":
                                {
                                    _ = currentOrbStartMode("stop", "Are you sure you wish to stop the current " + currentPod.status + "?");
                                    break;
                                }
                            default:
                                {
                                    Debug.WriteLine("Switch Error 5 Button Name");
                                    return;
                                }
                        }
                        break;
                    }
                case 6:
                    {
                        switch (button)
                        {
                            case "TopLeft":
                                {
                                    _ = currentOrbStartMode("standby");
                                    break;
                                }
                            case "TopRight":
                                {
                                    _ = currentOrbStartMode("stop", "Are you sure you wish to stop the current " + currentPod.status + "?");
                                    break;
                                }
                            default:
                                {
                                    Debug.WriteLine("Switch Error 6 Button Name");
                                    return;
                                }
                        }
                        break;
                    }
                default:
                    {
                        Debug.WriteLine("Switch Error Button State");
                        return;
                    }
            }
            buttonStateManager(0);
        }
        /// <summary>
        /// Handles the button states based on what the current pod is doing.
        /// </summary>
        /// <param name="command">This paramater is used to lock the current state of the buttons</param>
        void buttonStateManager(int command)
        {
            if(command != -1)
            {
                buttonAppearanceHandler(currentState);
            }
            int modeState = 0; //State 
            switch (currentPod.Status)
            {
                case "Float":
                    modeState = 1;
                    currentPod.largeButtonText = "Float in Progress";
                    break;
                case "Idle":
                    modeState = 3;
                    break;
                case "Pre-Prime":
                case "Empty":
                    if(currentPod.largeButtonText == "Float in Progress")
                    {
                        currentPod.largeButtonText = "Float Ending";
                    }
                    else
                    {
                        currentPod.largeButtonText = "Empty in Progress";
                    }
                    modeState = 1;
                    break;
                case "Pre-Float Heatup":
                case "Fill":
                case "Fill Delay":
                    if (currentPod.largeButtonText == "Float Starting")
                    {
                        //do nothing (Edit text in button function handler)
                    }
                    else
                    {
                        currentPod.largeButtonText = "Fill in Progress";
                    }
                    modeState = 1;
                    break;
                case "Standby":
                    modeState = 0;
                    break;
                case "Salt-Mix":
                case "Orbit-Orbit":
                case "EXO to EXO":
                    modeState = 5;
                    currentPod.largeButtonText = "Salt-Mix in Progress";
                    break;
                case "Skimmer":
                    modeState = 1;
                    currentPod.largeButtonText = "Skim in Progress";
                    break;
                case "Dosing":
                    modeState = 1;
                    currentPod.largeButtonText = "Dosing In Progress";
                    break;
                case "Wash Down":
                    modeState = 6;
                    currentPod.largeButtonText = "Wash Down In Progress";
                    break;
                case "Hibernate":
                    modeState = 6;
                    currentPod.largeButtonText = "Hibernating";
                    break;
                default:
                    break;
            }
            if(command == -1 && currentState != 4 && currentState !=2)
            {
                currentState = modeState;
                buttonAppearanceHandler(currentState);
            }
            else if(currentState == 4 && !currentPod.starting)
            {
                currentState = 1;
                buttonAppearanceHandler(currentState);
            }

        }

        /// <summary>
        /// Handles the cleaning function. Runs skimmer if wash down is unavailable. Otherwise gives the user a choice between the two
        /// </summary>
        /// <returns></returns>
        async Task handleCleaning()
        {
            if (currentPod.Pod.getData("wash_down_available", false) == "1")
            {
                bool response = await _messageInterface.ShowAsyncAcceptCancel("Cleaning", "Would you like to run a skimmer or use the Wash Down Pump?", "Skimmer", "Wash Down");
                if(!response)
                {
                    currentState = 6;
                    _ = currentOrbStartMode("washdown");
                    return;
                }
            }
            _ = currentOrbStartMode("skimmer");
            currentPod.largeButtonText = "Skimmer in Progress";
            currentState = 1;
        }

        /// <summary>
        /// Turns the Pod loading screen on
        /// </summary>
        void loadingScreen(bool on)
        {
            LockScreen = on;
        }

        /// <summary>
        /// Prevents the float time selector from being used if the pod is not in standby
        /// </summary>
        void handleFloatSelectorLock()
        {
            if (currentPod.Status == "Standby" && !currentPod.busy)
            {
                FloatTimeLocked = true;
            }
            else
            {
                FloatTimeLocked = false;
            }
        }
        /// <summary>
        /// Calculates the remaining time left for the current function.
        /// </summary>
        void handleFloatTimer()
        {
            if(currentPod.starting)
            {
                RemainingTimeActive = true;
                FloatTimeLabel = currentPod.currentFloatSession + " Float\nLoading";
                return;
            }
            switch(currentPod.Status)
            {
                case "Fill Delay":
                    {
                        RemainingTimeActive = true;
                        maxTimeLabel = 1;
                        if (timeLabelToggle == 0)
                        {
                            FloatTimeLabel = "Finishes at\n" + DateTime.Now.AddSeconds(currentPod.totalTime - currentPod.elaspedTime).ToString("HH:mm");
                        }
                        else
                        {
                            float delayRemainingMins = (currentPod.fillDelayTime - currentPod.elaspedTime) / 60;

                            delayRemainingMins = (float)Math.Floor(delayRemainingMins);

                            if (delayRemainingMins > 1)
                            {
                                FloatTimeLabel = delayRemainingMins + " Minutes\nDelay";
                            }
                            else if(delayRemainingMins < 0)
                            {
                                FloatTimeLabel = "Fill\nDelay";
                            }
                            else
                            {
                                FloatTimeLabel = delayRemainingMins + " Minute\nDelay";
                            }

                        }
                        break;
                    }
                case "Float":
                    {
                        if (currentPod.elaspedTime == -1)
                        {
                            return;
                        }
                        RemainingTimeActive = true;
                        if (currentPod.lastCommand != "start")
                        {
                            maxTimeLabel = 1;
                        }
                        else
                        {
                            maxTimeLabel = 2;
                        }

                        if (timeLabelToggle == 0)
                        {
                            FloatTimeLabel = "Finishes at\n" + DateTime.Now.AddSeconds(currentPod.totalTime - currentPod.elaspedTime).ToString("HH:mm");
                        }
                        else if(timeLabelToggle == 1)
                        {
                            float timeRemainingMins = (currentPod.totalTime - currentPod.elaspedTime) / 60;
                            timeRemainingMins = (float)Math.Floor(timeRemainingMins);

                            if (timeRemainingMins > 1)
                            {
                                FloatTimeLabel = timeRemainingMins + " Minutes\nRemaining";
                            }
                            else
                            {
                                FloatTimeLabel = timeRemainingMins + " Minute\nRemaining";
                            }

                        }
                        else
                        {
                            float timeRemainingMins = (float)((currentPod.totalTime) - (currentPod.emptyTime + currentPod.elaspedTime) )/ 60;
                            timeRemainingMins = (float)Math.Floor(timeRemainingMins);
                            if (timeRemainingMins < 0)
                            {
                                FloatTimeLabel = "Floating";
                            }
                            else if (timeRemainingMins > 1 || timeRemainingMins == 0)
                            {
                                FloatTimeLabel = timeRemainingMins + " Minutes\nFloating Time";
                            }
                            else
                            {
                                FloatTimeLabel = timeRemainingMins + " Minute\nFloating Time";
                            }
                        }
                        break;
                    }
                case "Skimmer":
                case "Pre-Float Heatup":
                    {
                        if(currentPod.elaspedTime == -1)
                        {
                            return;
                        }
                        RemainingTimeActive = true;
                        maxTimeLabel = 1;
                        if (timeLabelToggle == 0)
                        {
                            FloatTimeLabel = "Finishes at\n" + DateTime.Now.AddSeconds(currentPod.totalTime - currentPod.elaspedTime).ToString("HH:mm");
                        }
                        else
                        {
                            float timeRemainingMins = (currentPod.totalTime - currentPod.elaspedTime) / 60;
                            timeRemainingMins = (float)Math.Floor(timeRemainingMins);
                            
                                if (timeRemainingMins > 1)
                                {
                                    FloatTimeLabel = timeRemainingMins + " Minutes\nRemaining";
                                }
                                else
                                {
                                    FloatTimeLabel = timeRemainingMins + " Minute\nRemaining";
                                }
                            
                        }
                        
                        break;
                    }
                case "Fill":
                case "Pre-Prime":
                    {
                       
                        if (currentPod.lastCommand == "start" || currentPod.lastCommand == "skimmer")
                        {
                            maxTimeLabel = 2;
                        }
                        else
                        {
                            maxTimeLabel = 0;
                        }
                        


                        if (currentPod.elaspedTime == -1)
                        {
                            return;
                        }
                        RemainingTimeActive = true;
                        if (timeLabelToggle == 1)
                        {
                            FloatTimeLabel = "Finishes at\n" + DateTime.Now.AddSeconds(currentPod.totalTime - currentPod.elaspedTime).ToString("HH:mm");
                        }
                        else if(timeLabelToggle == 0)
                        {
                            float timeRemainingMins = (float)((CurrentPod.fillDelayTime + CurrentPod.fillTime) - (DateTime.Now - CurrentPod.modeStartTime).TotalSeconds)/60;
                            timeRemainingMins = (float)Math.Floor(timeRemainingMins);
                            if(timeRemainingMins < 0)
                            {
                                FloatTimeLabel = "Pod\nFilling";
                            }
                            else if(timeRemainingMins > 1 || timeRemainingMins == 0)
                            {
                                FloatTimeLabel = timeRemainingMins + " Minutes\nto Fill";
                            }
                            else
                            {
                                FloatTimeLabel = timeRemainingMins + " Minute\nto Fill";
                            }
                        }
                        else
                        {
                            float timeRemainingMins = (currentPod.totalTime - currentPod.elaspedTime) / 60;
                            timeRemainingMins = (float)Math.Floor(timeRemainingMins);

                            if (timeRemainingMins > 1)
                            {
                                FloatTimeLabel = timeRemainingMins + " Minutes\nRemaining";
                            }
                            else
                            {
                                FloatTimeLabel = timeRemainingMins + " Minute\nRemaining";
                            }

                        }
                        

                        break;
                    }
                case "Empty":
                    {
                        RemainingTimeActive = true;
                        if (currentPod.lastCommand != "start")
                        {
                            maxTimeLabel = 1;
                            float timeRemainingMins = (float)((CurrentPod.emptyTime) - (DateTime.Now - CurrentPod.modeStartTime).TotalSeconds) / 60;
                            timeRemainingMins = (float)Math.Floor(timeRemainingMins);
                            if(timeLabelToggle == 0)
                            {
                                if (timeRemainingMins < 0)
                                {
                                    FloatTimeLabel = "Pod\nEmptying";
                                }
                                else if (timeRemainingMins > 1 || timeRemainingMins == 0)
                                {
                                    FloatTimeLabel = timeRemainingMins + " Minutes\nto Empty";
                                }
                                else
                                {
                                    FloatTimeLabel = timeRemainingMins + " Minute\nto Empty";
                                }
                            }
                            else
                            {
                                if (timeRemainingMins < 0)
                                {
                                    FloatTimeLabel = "Pod\nEmptying";
                                }
                                else
                                {
                                    FloatTimeLabel = "Finishes at\n" + DateTime.Now.AddMinutes(timeRemainingMins).ToString("HH:mm");
                                }
                            }

                            return;
                        }
                        else
                        {
                            maxTimeLabel = 1;
                            if (timeLabelToggle == 0)
                            {
                                FloatTimeLabel = "Finishes at\n" + DateTime.Now.AddSeconds(currentPod.totalTime - currentPod.elaspedTime).ToString("HH:mm");
                            }
                            else
                            {
                                float timeRemainingMins = (currentPod.totalTime - currentPod.elaspedTime) / 60;
                                timeRemainingMins = (float)Math.Floor(timeRemainingMins);

                                if (timeRemainingMins > 1)
                                {
                                    FloatTimeLabel = timeRemainingMins + " Minutes\nRemaining";
                                }
                                else
                                {
                                    FloatTimeLabel = timeRemainingMins + " Minute\nRemaining";
                                }

                            }
                        }
                        break;
                    }
                case "Orbit-Orbit":
                case "EXO to EXO":
                case "Salt-Mix":
                    {
                        RemainingTimeActive = true;
                        FloatTimeLabel = "Salt Mixing\nEmpty to End";
                        break;
                    }
                case "Standby":
                    {
                        RemainingTimeActive = false;
                        break;
                    }
                case "Idle":
                    {
                        RemainingTimeActive = true;
                        FloatTimeLabel = "Pod\nStopped";
                        break;
                    }
                case "Dosing":
                    {
                        RemainingTimeActive = true;
                        FloatTimeLabel = "Pod\nDosing";
                        break;
                    }
                case "Wash Down":
                    {
                        if (currentPod.elaspedTime == -1)
                        {
                            return;
                        }
                        RemainingTimeActive = true;
                        maxTimeLabel = 1;
                        if (timeLabelToggle == 0)
                        {
                            FloatTimeLabel = "Finishes at\n" + DateTime.Now.AddSeconds(currentPod.totalTime - currentPod.elaspedTime).ToString("HH:mm");
                        }
                        else if (timeLabelToggle == 1)
                        {
                            float timeRemainingMins = (currentPod.totalTime - currentPod.elaspedTime) / 60;
                            timeRemainingMins = (float)Math.Floor(timeRemainingMins);

                            if (timeRemainingMins > 1)
                            {
                                FloatTimeLabel = timeRemainingMins + " Minutes\nRemaining";
                            }
                            else
                            {
                                FloatTimeLabel = timeRemainingMins + " Minute\nRemaining";
                            }

                        }
                        break;
                    }
                default:
                    {
                        RemainingTimeActive = false;
                        break;
                    }
            }
        }

        /// <summary>
        /// Sets the menu button functions when clicked
        /// </summary>
        /// <param name="command">The button clicked, specified when creating menu buttons</param>
        /// <returns></returns>
        async Task menuButtonClick(string command)
        {
            switch(command)
            {
                case "Settings":
                    {
                        if(!checkInUse())
                        {
                            await _messageInterface.ShowAsyncOK("Error 4020", "The current Pod must be in Standby or Stop/Idle to use this function", "Close");
                            return;
                        }
                        _ = settingsPage();
                        break;
                    }
                case "Dose":
                    {
                        if (!checkInUse())
                        {
                            await _messageInterface.ShowAsyncOK("Error 4020", "The current Pod must be in Standby or Stop/Idle to use this function", "Close");
                            return;
                        }
                        bool response = await _messageInterface.ShowAsyncAcceptCancel("Dosing", "This will manually dose your EXO Pod.\n Do you wish to continue?", "Yes", "No");
                        if (response)
                        {
                            _ = currentOrbStartMode("dosing");
                            currentState = 0;
                            fadeMenu.Invoke();
                        }
                        break;
                    }
                case "Hibernate":
                    {
                        if (!checkInUse())
                        {
                            await _messageInterface.ShowAsyncOK("Error 4020", "The current Pod must be in Standby or Stop/Idle to use this function", "Close");
                            return;
                        }
                        bool response = await _messageInterface.ShowAsyncAcceptCancel("Hibernate", "This mode will hibernate the EXO Pod until it is stopped manually.\nThis will turn off the Pod Lighting and only heat the solution to 30°C\n Do you wish to continue?", "Yes", "No");
                        if (response)
                        {
                            response = await _messageInterface.ShowAsyncAcceptCancel("Hibernate", "I understand that it may take several hours to regain solution temperature after hibernation.", "Yes", "No");
                            if (response)
                            {
                                _ = currentOrbStartMode("hibernate");
                                currentState = 0;
                                fadeMenu.Invoke();
                            }
                        }
                        break;
                    }
                case "Lighting":
                    {
                        if (!checkInUse())
                        {
                            await _messageInterface.ShowAsyncOK("Error 4020", "The current Pod must be in Standby or Stop/Idle to use this function", "Close");
                            return;
                        }
                        _ = lightingProfilePage();
                        break;
                    }
                case "Float Logs":
                    {
                        if (!checkInUse())
                        {
                            await _messageInterface.ShowAsyncOK("Error 4020", "The current Pod must be in Standby or Stop/Idle to use this function", "Close");
                            return;
                        }
                        _ = floatLogsPage();
                        break;
                    }
                case "Music Manager":
                    {
                        if (!checkInUse())
                        {
                            await _messageInterface.ShowAsyncOK("Error 4020", "The current Pod must be in Standby or Stop/Idle to use this function", "Close");
                            return;
                        }
                        _ = musicUploadPage();
                        break;
                    }
                case "Salt Mix":
                    {
                        if (!checkInUse())
                        {
                            await _messageInterface.ShowAsyncOK("Error 4020", "The current Pod must be in Standby or Stop/Idle to use this function", "Close");
                            return;
                        }
                        bool response = await _messageInterface.ShowAsyncAcceptCancel("Salt Mix", "Salt Mix will run continously until stopped.\n Do you wish to continue?", "Yes", "No");
                        if (response)
                        {
                            _ = currentOrbStartMode("salt-mix");
                            currentState = 0;
                            fadeMenu.Invoke();
                        }
                        break;
                    }
                case "Troubleshooting":
                    {
                        _ = supportPage();
                        break;
                    }
                case "Shutdown":
                    {
                        bool response = await _messageInterface.ShowAsyncAcceptCancel("Alert", "Do you want to shutdown this Pod?", "Yes", "No");
                        if (response)
                        {
                            response = await _messageInterface.ShowAsyncAcceptCancel("Alert", "The Pod will be off until manually restarted. Do you wish to continue", "Yes", "No");
                            if (response)
                            {
                                currentPod.shutdownOrbit();
                                fadeMenu.Invoke();
                                await _messageInterface.ShowAsyncOK("Alert", "The pod is now shutting down.", "Close");
                            }
                        }
                        break;
                    }
                case "Reboot":
                    {
                        bool response = await _messageInterface.ShowAsyncAcceptCancel("Alert", "Do you want to restart this Pod? It will take around 60 seconds", "Yes", "No");
                        if (response)
                        {
                            currentPod.restartOrbit();
                            fadeMenu.Invoke();
                            await _messageInterface.ShowAsyncOK("Alert", "The pod is now restarting.", "Close");
                        }
                        break;
                    }
                case "Log Off":
                    {
                        _ = onExit();
                        break;
                    }
                case "Close Menu":
                    {
                        fadeMenu.Invoke();
                        break;
                    }
                case "Service - EXO":
                    {
                        if (!checkInUse())
                        {
                            await _messageInterface.ShowAsyncOK("Error 4020", "The current Pod must be in Standby or Stop/Idle to use this function", "Close");
                            return;
                        }
                        bool response = await _messageInterface.ShowAsyncAcceptCancel("Service", "This will fill the EXO Pod Up and then rotate the valves towards the EXO Pod.\nContinue?", "Yes", "No");
                        if (response)
                        {
                            _ = currentOrbStartMode("servicepod");
                            currentState = 0;
                            fadeMenu.Invoke();
                        }
                        break;
                    }
                case "Service - Res":
                    {
                        if (!checkInUse())
                        {
                            await _messageInterface.ShowAsyncOK("Error 4020", "The current Pod must be in Standby or Stop/Idle to use this function", "Close");
                            return;
                        }
                        bool response = await _messageInterface.ShowAsyncAcceptCancel("Service", "This will rotate the valves towards the Reservoir Tank.\nThe pod will not fill or empty\nContinue?", "Yes", "No");
                        if (response)
                        {
                            _ = currentOrbStartMode("serviceres");
                            currentState = 0;
                            fadeMenu.Invoke();
                        }
                        break;
                    }
                case "Fill Empty Mode":
                    {
                        bool response = await _messageInterface.ShowAsyncAcceptCancel("Service", "This will continuously fill and empty the EXO Pod.\nData logging is increased during this period\nContinue?", "Yes", "No");
                        if (response)
                        {
                            _ = currentOrbStartMode("fill-empty");
                            currentState = 0;
                            fadeMenu.Invoke();
                        }
                        break;
                    }
                case "Dry Float":
                    {
                        bool response = await _messageInterface.ShowAsyncAcceptCancel("Service", "This will perform a float without a fill/empty cycle\nContinue?", "Yes", "No");
                        if (response)
                        {
                            _ = currentOrbStartMode("float");
                            currentState = 0;
                            fadeMenu.Invoke();
                        }
                        break;
                    }
                case "EXO-EXO":
                    {
                        bool response = await _messageInterface.ShowAsyncAcceptCancel("Service", "This will start Pod-Pod mode\nPlease ensure there is solution in the EXO Pod\nContinue?", "Yes", "No");
                        if (response)
                        {
                            _ = currentOrbStartMode("orbit-orbit");
                            currentState = 0;
                            fadeMenu.Invoke();
                        }
                        break;
                    }
                case "Plus Overview":
                    {
                        _ = plusPage();
                        break;
                    }
                case "EXO Config":
                    {
                        _ = configPage();
                        break;
                    }
                case "Update EXO":
                    {
                        if (!checkInUse())
                        {
                            await _messageInterface.ShowAsyncOK("Error 4020", "The current Pod must be in Standby or Stop/Idle to use this function", "Close");
                            return;
                        }
                        _ = updatePage();
                        break;
                    }
                case "Wash Down":
                    {
                        if (!checkInUse())
                        {
                            await _messageInterface.ShowAsyncOK("Error 4020", "The current Pod must be in Standby or Stop/Idle to use this function", "Close");
                            return;
                        }
                        if(currentPod.Pod.getData("wash_down_available", false) != "1")
                        {
                            await _messageInterface.ShowAsyncOK("Error 3040", "The current Pod must have a wash down pump to use this function.\nPlease contact Wellness Technology Support if your current Pod is equipped with one and this error occurs", "Close");
                            return;
                        }
                        bool response = await _messageInterface.ShowAsyncAcceptCancel("Wash Down", "This will turn on the wash down pump\nPlease ensure that all solution is in the Reservoir\nDo not use the door while the pump is active.\nContinue?", "Yes", "No");
                        if (response)
                        {
                            _ = currentOrbStartMode("washdown");
                            currentState = 0;
                            fadeMenu.Invoke();
                        }
                        break;
                    }
                default:
                    {
                        _ = _messageInterface.ShowAsyncOK("Error 4211", "Unable to find menu command.\nPlease contact Wellness Technology Support", "Close");
                        break;
                    }
            }
            
        }

        /// <summary>
        /// Changes the active Pod to the new selected one.
        /// </summary>
        async Task switchCurrentPod()
        {
            fadeInOutEvent.Invoke();
            handleRenderEvent.Invoke(currentPod.getStringRender());
            handleLevelEvent.Invoke(currentPod.level);

            foreach (PodListItem podListItem in podSideList)
            {
                if(podListItem == currentPod)
                {
                    podListItem.Selected = true;
                }
                else
                {
                    podListItem.Selected = false;
                }
            }
            buttonStateManager(-1);
            alarmHandler();
            loadingScreen(CurrentPod.busy);
            TemperatureLabel = CurrentPod.getTemperature();
            handleFloatTimer();
            handleFloatSelectorLock();
            VersionLabel = currentPod.version;
            saveOldPodIndexes();
            musicTracks = currentPod.getMusicTracks();
            setNewPodIndexes();
            PropertyChanged(this, new PropertyChangedEventArgs("MusicTracks"));
            /*if (startIndex > musicTracks.Count - 1)
            {
                SelectedMusicTrackStart = MusicTracks[0];
            }
            else
            {
                SelectedMusicTrackStart = MusicTracks[startIndex];
            }
            if (endIndex > musicTracks.Count - 1)
            {
                SelectedMusicTrackEnd = MusicTracks[0];
            }
            else
            {
                SelectedMusicTrackEnd = MusicTracks[endIndex];
            }
            */

            try
            {
                SelectedMusicTrackStart = musicTracks[currentPod.musicTrackStartIndex];
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                SelectedMusicTrackStart = musicTracks[0];
            }

            try
            {
                Debug.WriteLine(musicTracks[currentPod.musicTrackEndIndex].name);
                SelectedMusicTrackEnd = musicTracks[currentPod.musicTrackEndIndex];
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                SelectedMusicTrackEnd = musicTracks[0];
            }

            IpLabel = currentPod.ip.Replace("http://", "").Replace("/", "");
            oldPod = currentPod;
            if (CurrentPod.busy)
            {
                await CurrentPod.update();
            }
        }

        /// <summary>
        /// Saves the currently selected options for the current pod
        /// </summary>
        private void saveOldPodIndexes()
        {
            oldPod.floatTimeIndex = floatTimesList.IndexOf(SelectedFloatTime);
            oldPod.lightIndex = lightingProfile.IndexOf(SelectedLightingProfile);
            oldPod.fillDelayIndex = fillDelayList.IndexOf(SelectedFillDelay);
            oldPod.musicStartIndex = musicStartTime.IndexOf(SelectedMusicStartTime);
            oldPod.musicEndIndex = musicEndTime.IndexOf(SelectedMusicEndTime);
            oldPod.musicTrackStartIndex = musicTracks.IndexOf(SelectedMusicTrackStart);
            oldPod.musicTrackEndIndex = musicTracks.IndexOf(SelectedMusicTrackEnd);
            oldPod.continousLightsSelected = SelectedContinousLights;
            oldPod.heatedFloatSelected = SelectedheatedFloat;
        }

        /// <summary>
        /// Sets the selected options to the previously selected options
        /// </summary>
        private void setNewPodIndexes()
        {

            try
            {
                SelectedFloatTime = floatTimesList[currentPod.floatTimeIndex];
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                SelectedFloatTime = floatTimesList[0];
            }
            try
            {
                SelectedLightingProfile = lightingProfile[currentPod.lightIndex];
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                SelectedLightingProfile = lightingProfile[0];
            }

            try
            {
                SelectedFillDelay = fillDelayList[currentPod.fillDelayIndex];
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                SelectedFillDelay = fillDelayList[0];
            }

            try
            {
                SelectedMusicStartTime = musicStartTime[currentPod.musicStartIndex];
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                SelectedMusicStartTime = musicStartTime[0];
            }

            try
            {
                SelectedMusicEndTime = musicEndTime[currentPod.musicEndIndex];
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                SelectedMusicEndTime = musicEndTime[0];
            }

            CurrentFloatTime = SelectedFloatTime.DisplayName;
            setHeatedFloatEnabled();
            setContinousLightingAvailable();
            SelectedContinousLights = currentPod.continousLightsSelected;
            SelectedheatedFloat = currentPod.heatedFloatSelected;
        }

        /// <summary>
        /// Handles the functions each alarm does when clicked
        /// </summary>
        /// <param name="name">Name of the alarm</param>
        /// <returns></returns>
        private async Task alarmFunction(string name)
        {
            Debug.WriteLine(name);
            switch (name)
            {
                case "h2o2Alarm":
                    {
                        await _messageInterface.ShowAsyncOK("Error 2020", "Please check dosing Hydrogen Peroxide Levels.\nWellness approved hydrogen peroxide is available from https://www.wellness-supplies.co.uk", "Close");
                        break;
                    }
                case "assistAlarm":
                    {
                        bool response = await _messageInterface.ShowAsyncAcceptCancel("Alert", "Have you seen resolved the customer alert", "Yes", "No");
                        if (response)
                        {
                            _ = currentPod.dismissAssist();
                        }
                        break;
                    }
                case "filterAlarmLow":
                    {
                        await _messageInterface.ShowAsyncOK("Error 2010", "Filters need replacing on " + currentPod.getFilterChangeDate() + ". Please ensure you have new filters available.\nWellness approved filters are available from https://www.wellness-supplies.co.uk ", "Close");
                        break;
                    }
                case "filterAlarmReplace":
                    {
                        await _messageInterface.ShowAsyncOK("Error 2010", "Filters need replacing. Please replace your filters.\nWellness approved filters are available from https://www.wellness-supplies.co.uk ", "Close");
                        break;
                    }
                case "uvAlarmLow":
                    {
                        await _messageInterface.ShowAsyncOK("Error 2030", "UV bulb needs replacing on " + currentPod.getUVChangeDate() + ". Please ensure you have a new UV bulb available.\nWellness approved UV bulbs are available by contacting support@wtec.ltd ", "Close");
                        break;
                    }
                case "uvAlarmReplace":
                    {
                        await _messageInterface.ShowAsyncOK("Error 2030", "UV bulb needs replacing. Please replace your UV bulb.\nWellness approved UV bulbs are available by contacting support@wtec.ltd", "Close");
                        break;
                    }
                case "podNotConnectedAlarm":
                    {
                        await _messageInterface.ShowAsyncOK("Error 1020", "EXO Pod is offline. Please check your EXO Pod is on and connected to your router", "Close");
                        break;
                    }
                case "backendBENotRespondingAlarm":
                    {
                        await _messageInterface.ShowAsyncOK("Error 1011", "Pod not responding, please restart your Pod.", "Close");
                        break;
                    }
                case "wipeOrbitAlarm":
                    {
                        await _messageInterface.ShowAsyncOK("Error 3010", "Level Sensor is blocked. Please wipe the level sensor in the Reservoir Tank", "Close");
                        break;
                    }
                case "stopOrbitAlarm":
                    {
                        await _messageInterface.ShowAsyncOK("Error 3020", "EXO Pod is currently in stop (idle). Please empty the EXO Pod and return to standby to resume solution heating", "Close");
                        break;
                    }
                default:
                    {
                        _ = _messageInterface.ShowAsyncOK("Error 4211", "Unable to find alarm command.\nPlease contact Wellness Support", "Close");
                        break;
                    }

            }
        }
        /// <summary>
        /// Checks whether the Pod is in standby or stop
        /// </summary>
        /// <returns>Returns true if in standby or stop</returns>
        private bool checkInUse()
        {
            if(CurrentPod.Status == "Standby" || CurrentPod.Status == "Idle")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Handles the float time selector.
        /// </summary>
        private void floatTimeChange(string upwards)
        {
            if(floatTimeChangeRunning)
            {
                return;
            }
            floatTimeChangeRunning = true;
            int index = FloatTimesList.IndexOf(SelectedFloatTime);
            if(upwards == "1")
            {
                if (index == FloatTimesList.Count() - 1)
                {
                    index = 0;
                }
                else
                {
                    index++;
                    Debug.WriteLine(index);
                }
            }
            else
            {
                if (index == 0)
                {
                    index = FloatTimesList.Count() - 1;
                }
                else
                {
                    index = index - 1;
                }
            }
            SelectedFloatTime = FloatTimesList[index];
            CurrentFloatTime = SelectedFloatTime.DisplayName;
            setHeatedFloatEnabled();

            floatTimeChangeRunning = false;
        }

        /// <summary>
        /// Exits the dashboard to the login screen
        /// </summary>
        private async Task onExit()
        {
            bool response = await _messageInterface.ShowAsyncAcceptCancel("Sign Out", "Do you want to Sign Out?", "Yes", "No");
            if (response)
            {
                timerRunning = false;
                await App.Current.MainPage.Navigation.PopAsync();
            }
        }

        /// <summary>
        /// Opens the settings page
        /// </summary>
        async Task settingsPage()
        {
            Globals.setCurrentPod(currentPod.Pod);
            await App.Current.MainPage.Navigation.PushAsync(new SettingsPageV2());
        }

        /// <summary>
        /// Opens the Plus stats page
        /// </summary>
        async Task plusPage()
        {
            Globals.setCurrentPod(currentPod.Pod);
            await App.Current.MainPage.Navigation.PushAsync(new PlusPage());
        }

        /// <summary>
        /// Opens the Exo config page
        /// </summary>
        async Task configPage()
        {
            Globals.setCurrentPod(currentPod.Pod);
            await App.Current.MainPage.Navigation.PushAsync(new Config());
        }

        /// <summary>
        /// Open the float logs page
        /// </summary>
        async Task floatLogsPage()
        {
            Globals.setCurrentPod(currentPod.Pod);
            await App.Current.MainPage.Navigation.PushAsync(new LogPage());
        }

        /// <summary>
        /// Not Implemented. Opens the lighting profile setting page
        /// </summary>
        async Task lightingProfilePage()
        {
            _ = _messageInterface.ShowAsyncOK("Error 4300", "Function not implemented", "Close");
            return;
            Globals.setCurrentPod(currentPod.Pod);
            await App.Current.MainPage.Navigation.PushAsync(new Config());
        }

        /// <summary>
        /// Open the music manager
        /// </summary>
        async Task musicUploadPage()
        {
            Globals.setCurrentPod(currentPod.Pod);
            await App.Current.MainPage.Navigation.PushAsync(new MusicManagerPage());
        }

        /// <summary>
        /// Opens the support page
        /// </summary>
        async Task supportPage()
        {
            await App.Current.MainPage.Navigation.PushAsync(new SupportPage());
        }

        /// <summary>
        /// Opens the update pod page
        /// </summary>
        async Task updatePage()
        {
            Globals.setCurrentPod(currentPod.Pod);
            await App.Current.MainPage.Navigation.PushAsync(new UpdatePodPage());
        }

        /// <summary>
        /// Internal class for the adaptable smart buttons.
        /// </summary>
        internal class SmartButton : INotifyPropertyChanged
        {
            public string text { get; set; }
            public bool visible { get; set; }
            public Color color { get; set; }
            public bool enabled { get; set; }
            public event PropertyChangedEventHandler PropertyChanged;
            public ICommand DashboardCommand {get;}

            /// <summary>
            /// Constructor
            /// </summary>
            public SmartButton(string Text, bool Visible, Color Color, bool Enabled, ICommand command)
            {
                this.Text = Text;
                this.Visible = Visible;
                this.Color = Color;
                this.Enabled = Enabled;
                DashboardCommand = command;
            }
            /// <summary>
            /// Sets the parameters of the smart button
            /// </summary>
            public void setButton(string Text, bool Visible, Color Color, bool Enabled)
            {
                this.Text = Text;
                this.Visible = Visible;
                this.Color = Color;
                this.Enabled = Enabled;
            }
            public string Text
            {
                set
                {
                    if (text != value)
                    {
                        text = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("Text"));
                        }
                    }
                }
                get
                {
                    return text;
                }
            }
            public bool Visible
            {
                set
                {
                    if (visible != value)
                    {
                        visible = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("Visible"));
                        }
                    }
                }
                get
                {
                    return visible;
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
            public bool Enabled
            {
                set
                {
                    if (enabled != value)
                    {
                        enabled = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("Enabled"));
                        }
                    }
                }
                get
                {
                    return enabled;
                }
            }

        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////PodListItem//////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Internal class for each Pod.
        /// </summary>
        internal class PodListItem : INotifyPropertyChanged 
        {
            public PodModel Pod { get; protected set; }
            public string Name { get; set; }
            public string status { get; set; }
            public bool starting { get; set; }
            public int temperature { get; set; }
            public bool cancel { get; set; }
            public bool busy { get; set; }
            public string version { get; protected set; }
            public string ip { get; protected set; }
            public string largeButtonText { get; set; }
            public float level { get; set; }
            public int elaspedTime { get; protected set; }
            public int totalTime { get; protected set; }                  
            public int fillDelayTime { get; set; }
            public int fillTime { get; set; }
            public int emptyTime { get; set; }
            public int lastFloatTime { get; set; }
            public DateTime modeStartTime { get; set; }


            public event PropertyChangedEventHandler PropertyChanged;
            private readonly Helpers.MessageInterface _messageInterface;

            private bool selected = false;
            private bool currentResTemp = true;
            private bool inUse = false;
            private string podTime = "";
            private int podMissCount = 0;
            private int connectionMissCount = 0;

            public bool assistAlarm { get; protected set; }
            public bool h2o2Alarm { get; protected set; }
            public bool filterLowAlarm { get; protected set; }
            public bool uvLowAlarm { get; protected set; }
            public bool filterReplaceAlarm { get; protected set; }
            public bool uvReplaceAlarm { get; protected set; }
            public bool podNotConnectedAlarm { get; protected set; }
            public bool backendBENotRespondingAlarm { get; protected set; }
            public bool wipeOrbitAlarm { get; protected set; }
            public bool stopOrbitAlarm { get; protected set; }

            int errorCount;

            public bool manualEmpty;
            public string lastCommand;

            public string currentFloatSession = "";

            public int floatTimeIndex = 3;
            public int fillDelayIndex = 0;
            public int lightIndex = 0;
            public int musicStartIndex = 1;
            public int musicEndIndex = 0;
            public int musicTrackStartIndex = 1;
            public int musicTrackEndIndex = 1;
            public bool continousLightsSelected = false;
            public bool heatedFloatSelected = false;




            ObservableCollection<MusicTrack> musicTracks = new ObservableCollection<MusicTrack>();

            /// <summary>
            /// Constructor for each Pod list item
            /// </summary>
            public PodListItem(PodModel pod, MessageInterface messageInterface)
            {
                this.Pod = pod;
                pod.getUV();
                pod.getFilter();
                starting = false;
                temperature = 0;
                pod.getVersion();
                version = pod.version;
                ip = pod.ipAddress;
                _messageInterface = messageInterface;

                if (!pod.getMusicTracks())
                {
                    _ = _messageInterface.ShowAsyncOK("Error 4310", "Unable to find music database\nReverting to defaults.\nPlease ensure your EXO Pod is up to date", "Ok");
                }
                largeButtonText = "";
                foreach (MusicTrack mu in pod.musicTracks)
                {
                    musicTracks.Add(mu);
                }
                pod.updateSettings();
                Name = pod.podNumber.ToString();
            }

            /// <summary>
            /// Update for parameters for the Pod. Frequency is set in the main class
            /// </summary>
            public async Task<int> update()
            {
                if (inUse)
                {
                    return 0;
                }
                inUse = true;
                await Task.Run(() =>
                {
                    if(!Pod.updateStatus())
                    {
                        connectionMissCount++;
                    }
                    else
                    {
                        connectionMissCount = 0;
                    }
                    Status = Pod.getData("SysMode", true);
                    if(Status == "Orbit-Orbit")
                    {
                        Status = "EXO to EXO";
                    }
                    getLevel();
                    alarmHandler();
                    errorCounter();
                    getElaspedAndTotalTime();
                });
                inUse = false;
                return 1;
            }

            /// <summary>
            /// Sets the current Pod in the global parameters
            /// </summary>
            public void setCurrentPod()
            {
                Globals.setCurrentPod(Pod);
            }

            /// <summary>
            /// Gets the total mode time, and the elapsed mode time from the pod database.
            /// </summary>
            void getElaspedAndTotalTime()
            {
                int totalTime = 0;
                int elaspedTime = 0;
                int.TryParse(Pod.getData("TotalModeTime", true),out totalTime);
                int.TryParse(Pod.getData("ElapsedModeTime", true), out elaspedTime);
                this.totalTime = totalTime;
                this.elaspedTime = elaspedTime;
            }

            /// <summary>
            /// Gets all available lighting profiles on the Pod
            /// </summary>
            public ObservableCollection<string> getLightingProfiles()
            {
                Pod.getLightingProfiles();
                ObservableCollection<string> lightingProfiles = new ObservableCollection<string>();
                foreach(string lt in Pod.lightingProfiles)
                {
                    lightingProfiles.Add(lt);
                }
                return lightingProfiles;
            }

            /// <summary>
            /// Gets the list of music tracks
            /// </summary>
            public ObservableCollection<MusicTrack> getMusicTracks()
            {
                return musicTracks;
            }

            /// <summary>
            /// Restarts the Pod
            /// </summary>
            public void restartOrbit()
            {
                Pod.restart();
            }

            /// <summary>
            /// Shutsdown the Pod
            /// </summary>
            public void shutdownOrbit()
            {
                Pod.shutdown();
            }

            /// <summary>
            /// Generates the string which instructs what to display on the render
            /// </summary>
            public string getStringRender()
            {
                string renderString = "";

                if (Pod.getData("DoorState", true) == "Open")
                {
                    renderString += "o";
                }
                else
                {
                    renderString += "c";
                }

                if(level < 0.5)
                {
                    renderString += "f";
                }
                else
                {
                    renderString += "e";
                }

                if (assistAlarm)
                {
                    renderString += "w";
                }
                else
                {
                    renderString += "n";
                }

                switch (status)
                {
                    case "Float":
                        renderString += "Rainbow";
                        break;
                    case "Skimmer":
                        renderString += "Yellow";
                        break;
                    case "Dosing":
                        renderString += "Orange";
                        break;
                    case "Orbit-Orbit":
                    case "EXO to EXO":
                    case "Salt-Mix":
                        renderString += "Purple";
                        break;
                    case "Idle":
                        renderString += "White";
                        break;
                    case "Fill":
                    case "Fill Delay":
                    case "Pre-Prime":
                    case "Pre-Float Heatup":
                        renderString += "Green";
                        break;
                    case "Empty":
                        renderString += "Red";
                        break;
                    case "Hibernate":
                        renderString += "No";
                        break;
                    case "Wash Down":
                        renderString += "Red";
                        break;
                    default:
                        renderString += "Blue";
                        break;
                }
                return renderString;
            }

            /// <summary>
            /// Gets the current reservoir level.
            /// </summary>
            void getLevel()
            {
                float level = 0;
                if (Pod.getData("level_sensor_enabled", false) == "1")
                {
                    float.TryParse(Pod.getData("ResTankLevel", true), out level);
                }
                else
                {
                    if (Pod.getData("FloatSwitch", true) == "Up")
                    {
                        level = 1.7f;
                    }
                    else
                    {
                        level = 0;
                    }
                }
                this.level = level;
            }

            /// <summary>
            /// Calculates when to change the filters
            /// </summary>
            public string getFilterChangeDate()
            {
                return Pod.filter.AddDays(28).ToString("d");
            }

            /// <summary>
            /// Calculates when to change the UV
            /// </summary>
            public string getUVChangeDate()
            {
                return Pod.uv.AddYears(1).ToString("d");
            }

            /// <summary>
            /// Gets whether lighting profile sync is on 
            /// </summary>
            public bool getLightProfileSync()
            {
                return Pod.getData("light_music_end_sync", false) == "1";
            }

            /// <summary>
            /// Works out whether alarms need to be activated
            /// </summary>
            void alarmHandler()
            {
                /*
                
                AssistAlarm = true;
                filterReplaceAlarm = true;
                uvReplaceAlarm = true;
                podNotConnectedAlarm = true;
                backendBENotRespondingAlarm = true;
                podNotConnectedAlarm = true;
                wipeOrbitAlarm = true;
                stopOrbitAlarm = true;
                return;
                */

                if (Pod.getData("Assist", true) != "0")
                {
                    AssistAlarm = true;
                }
                else
                {
                    AssistAlarm = false;
                }

                if (Pod.getData("H2O2Bottle", true) == "1")
                {
                    h2o2Alarm = true;
                }
                else
                {
                    h2o2Alarm = false;
                }



                if (Pod.filter.AddDays(28) < DateTime.Now)
                {
                    filterLowAlarm = false;
                    filterReplaceAlarm = true;
                }
                else if(Pod.filter.AddDays(21) < DateTime.Now)
                {
                    filterReplaceAlarm = false;
                    filterLowAlarm = true;
                }
                else
                {
                    filterReplaceAlarm = false;
                    filterLowAlarm = false;
                }

                if (Pod.uv.AddYears(1) < DateTime.Now)
                {
                    uvLowAlarm = false;
                    uvReplaceAlarm = true;
                }
                else if(Pod.uv.AddMonths(11) < DateTime.Now)
                {
                    uvReplaceAlarm = false;
                    uvLowAlarm = true;
                }
                else
                {
                    uvReplaceAlarm = false;
                    uvLowAlarm = false;
                }

                if (connectionMissCount > 5)
                {
                    podNotConnectedAlarm = true;
                }
                else
                {
                    podNotConnectedAlarm = false;
                }
                
                if (Pod.getData("lastUpdateSecond", true) == podTime)
                {
                    podMissCount++;
                    if (podMissCount > 10 && !podNotConnectedAlarm)
                    {
                        backendBENotRespondingAlarm = true;
                    }
                }
                else
                {
                    podMissCount = 0;
                    backendBENotRespondingAlarm = false;
                    podTime = Pod.getData("lastUpdateSecond", true);
                }
                 
                if (level == 2.5)
                {
                    wipeOrbitAlarm = true;
                }
                else
                {
                    wipeOrbitAlarm = false;
                }

                if(status == "Idle")
                {
                    stopOrbitAlarm = true;
                }
                else
                {
                    stopOrbitAlarm = false;
                }
                
            }
            /// <summary>
            /// Counts how many alarms are currently active
            /// </summary>
            void errorCounter()
            {
                int i = 0;
                if(AssistAlarm)
                {
                    i++;
                }
                if (h2o2Alarm)
                {
                    i++;
                }
                /*if (filterLowAlarm)   //Ignore Filter/UV Low alarms
                {
                    i++;
                }
                if (uvLowAlarm)
                {
                    i++;
                }
                */
                if (filterReplaceAlarm)
                {
                    i++;
                }
                if (uvReplaceAlarm)
                {
                    i++;
                }
                if (podNotConnectedAlarm)
                {
                    i++;
                }
                if (backendBENotRespondingAlarm)
                {
                    i++;
                }
                if (wipeOrbitAlarm)
                {
                    i++;
                }
                if(stopOrbitAlarm)
                {
                    i++;
                }    
                ErrorCount = i;

            }
            
            /// <summary>
            /// Dismisses the assist alert
            /// </summary>
            public async Task dismissAssist()
            {
                if (!Pod.dismissAssist())
                {
                    await _messageInterface.ShowAsyncOK("Error 1012", "Unable to set float parameters. Please try again", "Close");
                }
                AssistAlarm = false;
            }
            /// <summary>
            /// Gets the solution temperature. The PT100 read will depend on the the mode of the Pod
            /// </summary>
            public string getTemperature()
            {
                string unroundedTemp = "";
                switch (Status)
                {
                    case "Float":
                    case "Skimmer":
                    case "Orbit-Orbit":
                    case "EXO to EXO":
                        unroundedTemp = Pod.getData("TempOrbit", true);
                        currentResTemp = false;
                        break;
                    case "Idle":
                    case "Empty":
                        if (currentResTemp)
                        {
                            unroundedTemp = Pod.getData("TempRes", true);
                        }
                        else
                        {
                            unroundedTemp = Pod.getData("TempOrbit", true);
                        }
                        break;
                    default:
                        unroundedTemp = Pod.getData("TempRes", true);
                        currentResTemp = true;
                        break;
                }
                float temperatureNumber = float.Parse(unroundedTemp);
                temperature = (int)Math.Round(temperatureNumber);
                if(temperature > 60 || temperature < 5)
                {
                    return "Error\n3030";
                }
                if (Preferences.Get("Celsius", false))
                {
                    temperature = ((temperature * 9) / 5) + 32;
                    return temperature.ToString() + "°F";
                }
                else
                {
                    return temperature.ToString() + "°C";
                }

            }

            /// <summary>
            /// Starts a specified mode on the Pod
            /// </summary>
            /// <param name="mode">The name of the mode to start</param>
            public async Task startMode(string mode)
            {
                fillTime = Pod.getFillTime();
                emptyTime = Pod.getEmptyTime();
                busy = true;
                lastCommand = mode;
                int error = 0;
                await Task.Run(async () =>
                {
                    string currentMode = Pod.getData("SysMode", true);
                    string oldMode = currentMode;
                    if (!Pod.startOrbit(mode))
                    {
                        error = 1;
                    }
                    int counter = 0;
                    while (currentMode == oldMode)
                    {
                        currentMode = Pod.getData("SysMode", true);
                        counter++;
                        await Task.Delay(100);
                        if (counter > 100)
                        {
                            error = 2;
                            break;
                        }
                    }
                    return;
                });
                if(error == 1)
                {
                    _ = _messageInterface.ShowAsyncOK("Error 1010", "Unable to start mode. Please try again", "Close");
                }
                else if(error == 2)
                {
                    await _messageInterface.ShowAsyncOK("Error 1014", "Timed out waiting for Pod-" + Pod.podNumber + " to respond", "Close");
                }
                modeStartTime = DateTime.Now;

                busy = false;
            }

            /// <summary>
            /// Runs a dry float
            /// </summary>
            public void dryFloat(string floatTime, string lightingProfile, string musicOption, string startMusic, string endMusic, string fillDelay)
            {
                if (!Pod.changeFloatCurrentSettings(floatTime, lightingProfile, musicOption, startMusic, endMusic, fillDelay))
                {
                    _ = _messageInterface.ShowAsyncOK("Error 1012", "Unable to set float parameters. Please try again", "Close");
                    cancel = true;
                    return;
                }
                if (!Pod.setHeatedFloat(0))
                {
                    _ = _messageInterface.ShowAsyncOK("Error 1012", "Unable to set float parameters. Please try again", "Close");
                    cancel = true;
                    return;
                }
                lastFloatTime = parseFloatTime(floatTime);
                _ = startMode("float");
            }

            /// <summary>
            /// Starts a float and sets all the parameters
            /// </summary>
            public async Task startFloat(string floatTime, string lightingProfile, string musicOption, string startMusic, string endMusic, string fillDelay, bool heatedFloat, string session, int startVol = -1, int endVol = -1)
            {
                starting = true;
                cancel = false;
                int heatedFloatMode = 0;
                if(heatedFloat)
                {
                    heatedFloatMode = 1;
                }
                DateTime start = DateTime.Now;
                currentFloatSession = session;
                await Task.Run(async () =>
                {
                    if(startVol != -1 && endVol != -1)
                    {
                        if (!Pod.changeFloatCurrentSettings(floatTime, lightingProfile, musicOption, startMusic, endMusic, fillDelay ,startVol, endVol))
                        {
                            _ = _messageInterface.ShowAsyncOK("Error 1012", "Unable to set float parameters. Please try again", "Close");
                            cancel = true;
                            return;
                        }
                    }
                    else
                    {
                        if (!Pod.changeFloatCurrentSettings(floatTime, lightingProfile, musicOption, startMusic, endMusic, fillDelay))
                        {
                            _ = _messageInterface.ShowAsyncOK("Error 1012", "Unable to set float parameters. Please try again", "Close");
                            cancel = true;
                            return;
                        }
                    }
                    if (!Pod.setHeatedFloat(heatedFloatMode))
                    {
                        _ = _messageInterface.ShowAsyncOK("Error 1012", "Unable to set float parameters. Please try again", "Close");
                        cancel = true;
                        return;
                    }


                    while ((DateTime.Now - start).Seconds < 10)
                    {
                        await Task.Delay(100);
                        if(cancel)
                        {
                            break;
                        }
                    }
                });
                if (cancel)
                {
                    starting = false;
                    return;
                }
                starting = false;
                _ = startMode("start");

                fillDelayTime = parseFillDelayTime(fillDelay);
                lastFloatTime = parseFloatTime(floatTime);
                if (Preferences.Get("Logging", true))
                {
                    if (!Pod.saveNewLog(floatTime, lightingProfile, musicOption, startMusic, endMusic, fillDelay))
                    {
                        _ = _messageInterface.ShowAsyncOK("Error 1016", "Unable to save log", "Close");
                    }
                }
                cancel = true;
                return;
                
            }
            /// <summary>
            /// Converts Fill delay time into an int
            /// </summary>
            int parseFillDelayTime(string delay)
            {
                switch(delay)
                {
                    default:
                        {
                            return 0;
                        }
                    case "1Min":
                        {
                            return 60;
                        }
                    case "2Min":
                        {
                            return 120;
                        }
                    case "4Min":
                        {
                            return 240;
                        }
                    case "8Min":
                        {
                            return 480;
                        }
                }
            }

            /// <summary>
            /// Converts the float time string into an int
            /// </summary>
            int parseFloatTime(string floatTime)
            {
                switch (floatTime)
                {
                    default:
                        {
                            return 0;
                        }
                    case "660_secs":
                        {
                            return 660;
                        }
                    case "30_mins":
                        {
                            return 1800;
                        }
                    case "45_mins":
                        {
                            return 2700;
                        }
                    case "60_mins":
                        {
                            return 3600;
                        }
                    case "75_min":
                        {
                            return 4500;
                        }
                    case "90_mins":
                        {
                            return 5400;
                        }
                    case "120_mins":
                        {
                            return 7200;
                        }
                    case "150_mins":
                        {
                            return 9000;
                        }
                    case "180_mins":
                        {
                            return 10800;
                        }
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


            public bool Selected
            {
                set
                {
                    if (selected != value)
                    {
                        selected = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("Selected"));
                        }
                    }
                }
                get
                {
                    return selected;
                }
            }

            public int ErrorCount
            {
                set
                {
                    if (errorCount != value)
                    {
                        errorCount = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("ErrorCount"));
                        }
                    }
                }
                get
                {
                    return errorCount;
                }
            }

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
            ////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////End Of Pod List Item/////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////
        }
        /// <summary>
        /// Internal class for menu butons
        /// </summary>
        internal class MenuButton
        {
            public ImageSource iconImage { get; protected set; }
            public string title { get; protected set; }
            public ICommand buttonCommand { get; set; }


            public MenuButton(ImageSource iconImage, string title, ICommand buttonCommand)
            {
                this.iconImage = iconImage;
                this.title = title;
                this.buttonCommand = buttonCommand;
            }



        }

        /// <summary>
        /// Internal class for selection items. Used on the Pod sidebar
        /// </summary>
        internal class SelectionItem
        {
            public string Name { get; protected set; }
            public string DisplayName { get; protected set; }

            public SelectionItem(string name, string displayName)
            {
                Name = name;
                DisplayName = displayName;
            }

        }

        /// <summary>
        /// Internal class for the alarms
        /// </summary>
        internal class Alarm
        {
            public ImageSource Icon { get; set; }
            public string name { get; set; }
            public ICommand AlarmCommand { get; set; }


            public Alarm(string name, ICommand alarmCommand)
            {
                this.name = name;
                this.AlarmCommand = alarmCommand;
                switch (name)
                {
                    case "h2o2Alarm":
                        {
                            Icon = getAlarmImage("H2O2_Empty.png");
                            break;
                        }
                    case "assistAlarm":
                        {
                            Icon = getAlarmImage("Help.png");
                            break;
                        }
                    case "filterAlarmLow":
                        {
                            Icon = getAlarmImage("Filter.png"); //Placeholder
                            break;
                        }
                    case "filterAlarmReplace":
                        {
                            Icon = getAlarmImage("Filter_2.png"); //Placeholder
                            break;
                        }
                    case "uvAlarmLow":
                        {
                            Icon = getAlarmImage("UV1.png"); //Placeholder
                            break;
                        }
                    case "uvAlarmReplace":
                        {
                            Icon = getAlarmImage("UV.png"); //Placeholder
                            break;
                        }
                    case "podNotConnectedAlarm":
                        {
                            Icon = getAlarmImage("Connection_Lost.png");
                            break;
                        }
                    case "backendBENotRespondingAlarm":
                        {
                            Icon = getAlarmImage("BE_Crash.png");
                            break;
                        }
                    case "wipeOrbitAlarm":
                        {
                            Icon = getAlarmImage("Wipe.png");// Placeholder
                            break;
                        }
                    case "stopOrbitAlarm":
                        {
                            Icon = getAlarmImage("Stop.png");// Placeholder
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

            }

            

            private ImageSource getAlarmImage(string file)
            {
                string assemblyName = GetType().GetTypeInfo().Assembly.GetName().Name;
                return ImageSource.FromResource(assemblyName + ".Assets.Images.Alarm." + file, typeof(OverviewViewModel).GetTypeInfo().Assembly);
            }


        }

        public ObservableCollection<PodListItem> PodSideList
        {
            set
            {
                if (podSideList != value)
                {
                    podSideList = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("MusicProfileListStart"));
                    }
                }
            }
            get
            {
                return podSideList;
            }
        }

        public PodListItem CurrentPod
        {
            set
            {
                if (currentPod != value)
                {
                    currentPod = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("CurrentPod"));
                    }
                }
            }
            get
            {
                return currentPod;
            }
        }

        public SmartButton TopLeftButton
        {
            set
            {
                if (topLeftButton != value)
                {
                    topLeftButton = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("TopLeftButton"));
                    }
                }
            }
            get
            {
                return topLeftButton;
            }
        }
        public SmartButton TopRightButton
        {
            set
            {
                if (topRightButton != value)
                {
                    topRightButton = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("TopRightButton"));
                    }
                }
            }
            get
            {
                return topRightButton;
            }
        }
        public SmartButton BottomLeftButton
        {
            set
            {
                if (bottomLeftButton != value)
                {
                    bottomLeftButton = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("BottomLeftButton"));
                    }
                }
            }
            get
            {
                return bottomLeftButton;
            }
        }
        public SmartButton BottomCentreButton
        {
            set
            {
                if (bottomCentreButton != value)
                {
                    bottomCentreButton = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("BottomCentreButton"));
                    }
                }
            }
            get
            {
                return bottomCentreButton;
            }
        }
        public SmartButton BottomRightButton
        {
            set
            {
                if (bottomRightButton != value)
                {
                    bottomRightButton = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("BottomRightButton"));
                    }
                }
            }
            get
            {
                return bottomRightButton;
            }
        }
        public SmartButton BottomLargeButton
        {
            set
            {
                if (bottomLargeButton != value)
                {
                    bottomLargeButton = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("BottomLargeButton"));
                    }
                }
            }
            get
            {
                return bottomLargeButton;
            }
        }

        public string TemperatureLabel
        {
            set
            {
                if (temperatureLabel != value)
                {
                    temperatureLabel = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("TemperatureLabel"));
                    }
                }
            }
            get
            {
                return temperatureLabel;
            }
        }

        public string TimeLabel
        {
            set
            {
                if (timeLabel != value)
                {
                    timeLabel = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("TimeLabel"));
                    }
                }
            }
            get
            {
                return timeLabel;
            }
        }

        public bool LockScreen
        {
            set
            {
                if (lockScreen != value)
                {
                    lockScreen = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("LockScreen"));
                    }
                }
            }
            get
            {
                return lockScreen;
            }
        }

        public string IpLabel
        {
            set
            {
                if (ipLabel != value)
                {
                    ipLabel = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("IpLabel"));
                    }
                }
            }
            get
            {
                return ipLabel;
            }
        }
        public string VersionLabel
        {
            set
            {
                if (versionLabel != value)
                {
                    versionLabel = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("VersionLabel"));
                    }
                }
            }
            get
            {
                return versionLabel;
            }
        }
        public string WorldTimeLabel
        {
            set
            {
                if (worldTimeLabel != value)
                {
                    worldTimeLabel = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("WorldTimeLabel"));
                    }
                }
            }
            get
            {
                return worldTimeLabel;
            }
        }

        public SelectionItem SelectedFloatTime
        {
            set
            {
                if (selectedFloatTime != value)
                {
                    selectedFloatTime = value;
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

        public SelectionItem SelectedFillDelay
        {
            set
            {
                if (selectedFillDelay != value)
                {
                    selectedFillDelay = value;
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

        public string SelectedLightingProfile
        {
            set
            {
                if (selectedLightingProfile != value)
                {
                    selectedLightingProfile = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SelectedLightingProfile"));
                    }
                }
            }
            get
            {
                return selectedLightingProfile;
            }
        }


        public SelectionItem SelectedMusicStartTime
        {
            set
            {
                if (selectedMusicStartTime != value)
                {
                    selectedMusicStartTime = value;
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

        public SelectionItem SelectedMusicEndTime
        {
            set
            {
                if (selectedMusicEndTime != value)
                {
                    selectedMusicEndTime = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SelectedMusicEndTime"));
                    }
                }
            }
            get
            {
                return selectedMusicEndTime;
            }
        }

        public MusicTrack SelectedMusicTrackStart
        {
            set
            {
                if (selectedMusicTrackStart != value)
                {
                    selectedMusicTrackStart = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SelectedMusicTrackStart"));
                    }
                }
            }
            get
            {
                return selectedMusicTrackStart;
            }
        }

        public MusicTrack SelectedMusicTrackEnd
        {
            set
            {
                if (selectedMusicTrackEnd != value)
                {
                    selectedMusicTrackEnd = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SelectedMusicTrackEnd"));
                    }
                }
            }
            get
            {
                return selectedMusicTrackEnd;
            }
        }

        public bool RemainingTimeActive
        {
            set
            {
                if (remainingTimeActive != value)
                {
                    remainingTimeActive = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("RemainingTimeActive"));
                    }
                }
            }
            get
            {
                return remainingTimeActive;
            }
        }

        public string FloatTimeLabel
        {
            set
            {
                if (floatTimeLabel != value)
                {
                    floatTimeLabel = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("FloatTimeLabel"));
                    }
                }
            }
            get
            {
                return floatTimeLabel;
            }
        }
        public bool SelectedheatedFloat
        {
            set
            {
                if (selectedheatedFloat != value)
                {
                    selectedheatedFloat = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SelectedheatedFloat"));
                    }
                }
            }
            get
            {
                return selectedheatedFloat;
            }
        }
        public bool HeatedFloatEnabled
        {
            set
            {
                if (heatedFloatEnabled != value)
                {
                    heatedFloatEnabled = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("HeatedFloatEnabled"));
                    }
                }
            }
            get
            {
                return heatedFloatEnabled;
            }
        }

        public bool SelectedContinousLights
        {
            set
            {
                if (selectedContinousLights != value)
                {
                    selectedContinousLights = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SelectedContinousLights"));
                    }
                }
            }
            get
            {
                return selectedContinousLights;
            }
        }
        public bool ContinousLightsEnabled
        {
            set
            {
                if (continousLightsEnabled != value)
                {
                    continousLightsEnabled = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("ContinousLightsEnabled"));
                    }
                }
            }
            get
            {
                return continousLightsEnabled;
            }
        }
        public bool FloatTimeLocked
        {
            set
            {
                if (floatTimeLocked != value)
                {
                    floatTimeLocked = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("FloatTimeLocked"));
                    }
                }
            }
            get
            {
                return floatTimeLocked;
            }
        }
        public string CurrentFloatTime
        {
            set
            {
                if (currentFloatTime != value)
                {
                    currentFloatTime = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("CurrentFloatTime"));
                    }
                }
            }
            get
            {
                return currentFloatTime;
            }
        }

    }
}
