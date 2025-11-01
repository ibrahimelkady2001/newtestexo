using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Storage;

namespace EXOApp.Models
{
    /// <summary>
    /// Viewmodel for the settings page
    /// </summary>
    class SettingsViewModelV2 : INotifyPropertyChanged
    {
        private readonly Helpers.MessageInterface _messageInterface;
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand settingsChangeCommand { protected set; get; }
        public ICommand exitCommand { protected set; get; }
        public ICommand otherConfirmCommand { protected set; get; }

        ObservableCollection<SettingsList> sideBar = new ObservableCollection<SettingsList>();
        public ObservableCollection<SettingsList> SideBar { get { return sideBar; } }


        ObservableCollection<SettingsItem> temperatureSettings = new ObservableCollection<SettingsItem>();

        ObservableCollection<SettingsItem> skimmerSettings = new ObservableCollection<SettingsItem>();

        ObservableCollection<SettingsItem> fillEmptySettings = new ObservableCollection<SettingsItem>();

        ObservableCollection<SettingsItem> floatingSettings = new ObservableCollection<SettingsItem>();

        ObservableCollection<SettingsItem> lightingSettings = new ObservableCollection<SettingsItem>();

        ObservableCollection<SettingsItem> dosingSettings = new ObservableCollection<SettingsItem>();

        ObservableCollection<SettingsItem> washDownSettings = new ObservableCollection<SettingsItem>();

        ObservableCollection<SettingsItem> debugSettings = new ObservableCollection<SettingsItem>();


        ObservableCollection<SettingsItem> viewedSettings = new ObservableCollection<SettingsItem>();
        public ObservableCollection<SettingsItem> ViewedSettings { get { return viewedSettings; } }
        SettingsList currentSetting;

        public Action fadeInOutEvent;

        ObservableCollection<string> temperatureSettingList = new ObservableCollection<string>();
        public ObservableCollection<string> TemperatureSettingList { get { return temperatureSettingList; } }

        string uvText;
        string filterText;

        PodModel Pod;
        public string OrbitNumber { get; set; }
        bool globalOpen;
        bool filterUVOpen;
        bool mainOpen;

        bool perTrackVolume;
        bool loggingOn;
        int selectedTemperature;

        /// <summary>
        /// Constructor for the settings viewmodel
        /// </summary>
        public SettingsViewModelV2()
        {
            this._messageInterface = DependencyService.Get<Helpers.MessageInterface>();
            Pod = Globals.getCurrentPod();
            Pod.updateSettings();
            OrbitNumber = " - " + Pod.podNumber.ToString();
            doLists();
            settingsChangeCommand = new Command(async ()=> await settingsChange());
            exitCommand = new Command(async () => await onExit());
            otherConfirmCommand = new Command<string>(async (button) => await otherConfirm(button));
            CurrentSetting = sideBar[0];
            viewedSettings = sideBar[0].settings;
            GlobalOpen = false;
            FilterUVOpen = false;
            MainOpen = true;
            calculateTimeLeft(true);
            calculateTimeLeft(false);
            foreach (SettingsList setting in sideBar)
            {
                setting.setSelected(false);
            }
            CurrentSetting.setSelected(true);
            foreach (var setting in viewedSettings)
            {
                setting.restoreCurrentValue();
            }

        }

        /// <summary>
        /// Creates a settings entry
        /// </summary>
        /// <param name="sqlSettingName">Name of the setting in the SQL database</param>
        /// <param name="controlType">The way the setting is changed. Choose between "bool" or "number"</param>
        /// <param name="maxValue">Max value of the setting</param>
        /// <param name="minValue">Minimum value of the setting</param>
        /// <param name="increment">The amount each setting can be incremented by</param>
        /// <param name="units">The units of the setting. Use a blank string if no units</param>
        /// <returns></returns>
        SettingsItem createSetting(string sqlSettingName, string controlType, float maxValue, float minValue, float increment, string units)
        {
            return new SettingsItem(Pod, _messageInterface, sqlSettingName, controlType, maxValue, minValue, increment, units);
        }

        /// <summary>
        /// Fills are the settings list.
        /// </summary>
        void doLists()
        {
            temperatureSettings.Add(createSetting("water_temp_setting", "number", 40, 34, 0.1f, "°C"));
            temperatureSettings.Add(createSetting("storage_heater_run_speed", "number", 50, 30, 10, "Hz"));

            skimmerSettings.Add(createSetting("skimmer_proc1_run_time", "number", 60, 0, 1, "Seconds"));
            skimmerSettings.Add(createSetting("skimmer_proc2_run_time", "number", 600, 60, 1, "Seconds"));

            fillEmptySettings.Add(createSetting("fill_run_time_middle", "number", 600, 100, 1, "Seconds"));
            fillEmptySettings.Add(createSetting("empty_run_time_middle", "number", 600, 100, 1, "Seconds"));

            floatingSettings.Add(createSetting("user_orbit_to_orbit_run_speed_middle", "number", 30, 5, 1, "Hz"));
            floatingSettings.Add(createSetting("pre_float_heat_enabled", "bool", 1, 0, 1, ""));
            floatingSettings.Add(createSetting("pre_float_heat_run_time", "number", 120, 60, 1, "Seconds"));
            floatingSettings.Add(createSetting("pre_float_heat_use_float_time", "bool", 1, 0, 1, ""));
            floatingSettings.Add(createSetting("volume_percentage", "number", 100, 0, 1, "%"));

            lightingSettings.Add(createSetting("night_mode_enabled", "bool", 1, 0, 1, ""));
            lightingSettings.Add(createSetting("night_mode_on_hour", "number", 23, 0, 1, ": 00"));
            lightingSettings.Add(createSetting("night_mode_off_hour", "number", 23, 0, 1, ": 00"));
            lightingSettings.Add(createSetting("light_music_end_sync", "bool", 1, 0, 1, ""));


            dosingSettings.Add(createSetting("dosing_run_time", "number", 500, 0, 1, "ml"));
            dosingSettings.Add(createSetting("dosing_time_of_day", "number", 23, 0, 1, ": 00"));

            //Side list
            sideBar.Add(new SettingsList("Standby", temperatureSettings));
            sideBar.Add(new SettingsList("Skimmer", skimmerSettings));
            sideBar.Add(new SettingsList("Fill and Empty", fillEmptySettings));
            sideBar.Add(new SettingsList("Floating", floatingSettings));
            sideBar.Add(new SettingsList("Dosing", dosingSettings));
            sideBar.Add(new SettingsList("Lighting", lightingSettings));
            if (Pod.getData("wash_down_available", false) == "1")
            {
                washDownSettings.Add(createSetting("wash_down_time", "number", 600, 120, 1, "s"));
                sideBar.Add(new SettingsList("Wash Down", washDownSettings));
            }
            if (Globals.getUserSecurityLevel() >= 99)
            {
                debugSettings.Add(createSetting("target_level_top", "number", 1.7f, 0, 0.1f, "m"));
                debugSettings.Add(createSetting("ignore_res_sensors", "bool", 1, 0, 1, ""));
                debugSettings.Add(createSetting("smart_empty_enabled", "bool", 1, 0, 1, ""));
                debugSettings.Add(createSetting("wash_down_available", "bool", 1, 0, 1, ""));

                sideBar.Add(new SettingsList("Debug", debugSettings));
            }
            sideBar.Add(new SettingsList("Filter/UV", null));
            sideBar.Add(new SettingsList("Global", null));
            //sideBar.Add(new SettingsList("Global\nSettings"));
            //sideBar.Add(new SettingsList("Debug\nSettings"));
            //

            TemperatureSettingList.Add("Celsius");
            TemperatureSettingList.Add("Farenheit");
            if (Preferences.Get("Celsius", false))
            {
                SelectedTemperature = 1;
            }
            else
            {
                SelectedTemperature = 0;
            }
            if (Preferences.Get("Logging", true))
            {
                LoggingOn = true;
            }
            else
            {
                LoggingOn = false;
            }
            if (Preferences.Get("PerTrackVolume", false)) //Unused
            {
                PerTrackVolume = true;
            }
            else
            {
                PerTrackVolume = false;
            }


        }

        /// <summary>
        /// Calcualtes when filter or UV need changing
        /// </summary>
        /// <param name="filter">True for filters, false for UV</param>
        void calculateTimeLeft(bool filter)
        {
            if(filter)
            {
                FilterText = "Filters need replacing on " + Pod.filter.AddDays(28).ToString("dd/MM/yy");
            }
            else
            {
                UVtext = "UV bulb need replacing on " + Pod.uv.AddYears(1).ToString("dd/MM/yy");
            }
        }

        /// <summary>
        /// Change between the various setting categories.
        /// </summary>
        async Task settingsChange()
        {
            fadeInOutEvent.Invoke();
            foreach(SettingsList setting in sideBar)
            {
                setting.setSelected(false);
            }
            CurrentSetting.setSelected(true);
            viewedSettings = CurrentSetting.settings;
            if(viewedSettings == null)
            {
                if(CurrentSetting.Name == "Global")
                {
                    GlobalOpen = true;
                    FilterUVOpen = false;
                    MainOpen = false;
                }
                else if(CurrentSetting.Name == "Filter/UV")
                {
                    FilterUVOpen = true;
                    GlobalOpen = false;
                    MainOpen = false;

                }
            }
            else
            {
                GlobalOpen = false;
                FilterUVOpen = false;
                MainOpen = true;
                foreach (var setting in viewedSettings)
                {
                    setting.restoreCurrentValue();
                }
            }

            PropertyChanged(this, new PropertyChangedEventArgs("ViewedSettings"));

        }

        /// <summary>
        /// Confirmation button for global settings, and filter UV change
        /// </summary>
        /// <param name="button">Specifies which function to use. I.e. "Temperature" for switching between c and f</param>
        /// <returns></returns>
        async Task otherConfirm(string button)
        {
            switch(button)
            {
                case "Temperature":
                    {
                        if (SelectedTemperature == 0)
                        {
                            Preferences.Set("Celsius", false);
                            _ = _messageInterface.ShowAsyncOK("Alert", "Temperature set to Celsius", "Ok");
                        }
                        else
                        {
                            Preferences.Set("Celsius", true);
                            _ = _messageInterface.ShowAsyncOK("Alert", "Temperature set to Farenheit", "Ok");
                        }
                        break;
                    }
                case "Music":
                    {
                        if (PerTrackVolume)
                        {                           
                            Preferences.Set("PerTrackVolume", true);
                            _ = _messageInterface.ShowAsyncOK("Alert", "Per Track Music Volumes will be used", "Ok");
                        }
                        else
                        {
                            Preferences.Set("PerTrackVolume", false);
                            _ = _messageInterface.ShowAsyncOK("Alert", "Global Music Volume will be used", "Ok");
                        }
                        break;
                    }
                case "Logging":
                    {
                        if (LoggingOn)
                        {
                            Preferences.Set("Logging", true);
                            _ = _messageInterface.ShowAsyncOK("Alert", "Session settings are being logged", "Ok");
                        }
                        else
                        {
                            Preferences.Set("Logging", false);
                            _ = _messageInterface.ShowAsyncOK("Alert", "Session settings are no longer being logged", "Ok");
                        }
                        break;
                    }
                case "Filter":
                    {
                        bool response = await _messageInterface.ShowAsyncAcceptCancel("Alert", "Have you changed the filters on Pod-" + Pod.podNumber +"?", "Yes", "No");
                        if(response)
                        {
                            Pod.setFilter();
                            calculateTimeLeft(true);
                        }
                        break;
                    }
                case "UV":
                    {
                        bool response = await _messageInterface.ShowAsyncAcceptCancel("Alert", "Have you changed the UV bulb on Pod-" + Pod.podNumber + "?", "Yes", "No");
                        if (response)
                        {
                            Pod.setUV();
                            calculateTimeLeft(false);
                        }
                        break;
                    }



            }
        }

        /// <summary>
        /// Exits the page and returns to the dashboard
        /// </summary>
        public async Task onExit()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }

        /// <summary>
        /// Internal class for all non global/FilterUV settings
        /// </summary>
        internal class SettingsItem : INotifyPropertyChanged
        {
            public string title { protected set; get; }
            public string description { protected set; get; }
            public float oldValue { protected set; get; }
            public float currentValue { protected set; get; }
            public int boolValue { protected set; get; }
            public float maxValue { protected set; get; }
            public float minValue { protected set; get; }
            public float increment { protected set; get; }
            public float defaultValue { protected set; get; }
            public string units { protected set; get; }
            public bool boolControl { protected set; get; }
            public bool numberControl { protected set; get; }


            public string sqlSettingName { protected set; get; }
            public event PropertyChangedEventHandler PropertyChanged;

            public string controlType { protected set; get; }

            PodModel Pod;

            public ICommand confirmChangesCommand { protected set; get; }
            public ICommand defaultCommand { protected set; get; }

            private readonly Helpers.MessageInterface _messageInterface;

            /// <summary>
            /// Constructor for each settings item.
            /// </summary>
            public SettingsItem(PodModel pod,Helpers.MessageInterface messageInterface, string sqlSettingName, string controlType, float maxValue, float minValue, float increment, string units)
            {
                this.sqlSettingName = sqlSettingName;
                _messageInterface = messageInterface;
                this.Pod = pod;
                this.controlType = controlType;
                this.maxValue = maxValue;
                this.minValue = minValue;
                this.increment = increment;
                this.units = units;
                setControlType();
                if(boolControl)
                {
                    BoolValue = int.Parse(Pod.getData(sqlSettingName, false));
                }
                else if(numberControl)
                {
                    CurrentValue = float.Parse(Pod.getData(sqlSettingName, false));
                }
                oldValue = CurrentValue;
                getDescription(sqlSettingName);
                
                confirmChangesCommand = new Command(async () => await saveSetting());
                defaultCommand = new Command(() => defaultSetting());

            }
            /// <summary>
            /// Restores the setting to its original value if the page is changed and the setting change isn't confirmed.
            /// </summary>
            public void restoreCurrentValue()
            {
                if (boolControl)
                {
                    BoolValue = (int)oldValue;
                }
                else if (numberControl)
                {
                    CurrentValue = oldValue;
                }
            }
            /// <summary>
            /// Tells the view whether to use a slider "number" or to use a switch "bool"
            /// </summary>
            void setControlType()
            {
                switch(controlType)
                {
                    case "bool":
                        {
                            boolControl = true;
                            numberControl = false;
                            break;
                        }
                    case "number":
                        {
                            boolControl = false;
                            numberControl = true;
                            break;
                        }
                }
            }

            /// <summary>
            /// Returns a settign to it's default value
            /// </summary>
            void defaultSetting()
            {
                if (numberControl)
                {
                    CurrentValue = defaultValue;
                }
                else if (boolControl)
                {
                    BoolValue = (int)defaultValue;
                }
                _ = _messageInterface.ShowAsyncOK("Settings Changed", title + " has been set to default\nPlease click \"Confirm Changes\" to save the value", "Close");
            }

            /// <summary>
            /// Saves a setting to it's new value
            /// </summary>
            async Task saveSetting()
            {
                string savedValue = "";
                if(boolControl)
                {
                    savedValue = BoolValue.ToString();
                }
                else if(numberControl)
                {
                    savedValue = CurrentValue.ToString();
                }
                if (!Pod.changeOrbitConfigSetting(sqlSettingName, savedValue))
                {
                    await _messageInterface.ShowAsyncOK("Error 1015", "Unable to save settings. Please try again", "Close");
                    return;
                }
                _ = _messageInterface.ShowAsyncOK("Settings Changed", title + " has successfully been changed", "Close");
                Pod.updateSettings();
            }

            /// <summary>
            /// Sets the description and title for each setting
            /// </summary>
            /// <param name="sqlSettingName">Name of the setting in the database</param>
            void getDescription(string sqlSettingName)
            {
                switch(sqlSettingName)
                {
                    case "fill_run_time_middle":
                        {
                            title = "Fill Time";
                            this.defaultValue = float.Parse(Pod.getData("default_fill_run_time_middle", false));
                            description = "The maximum amount of time it takes to fill the EXO." +
                                "\nPlease check the solution level before adjusting this setting." +
                                "\nThis prevents the pump over-running if the reservoir tanks are overfilled";
                            break;
                        }
                    case "empty_run_time_middle":
                        {
                            title = "Empty Time";
                            this.defaultValue = float.Parse(Pod.getData("default_empty_run_time_middle", false));
                            description = "The amount of time the pump will run to empty the EXO." +
                                "\nPlease check the solution level before adjusting this settings" +
                                "\nFailure to set this correctly will lead to solution not being drained from the EXO";
                            break;
                        }
                    case "dosing_run_time":
                        {
                            title = "Dosing Amount";
                            this.defaultValue = float.Parse(Pod.getData("default_dosing_run_time", false));
                            description = "The amount of hydrogen peroxide dosed to the EXO." +
                                "\nPlease check the pH level after dosing" +
                                "\nFailure to set this correctly can lead harmful solution";
                            break;
                        }
                    case "dosing_time_of_day":
                        {
                            title = "Auto Dosing Time";
                            this.defaultValue = float.Parse(Pod.getData("default_dosing_time_of_day", false));
                            description = "The time when auto dosing should trigger" +
                                "\nPlease ensure this is during a time that is out of hours";
                            break;
                        }
                    case "volume_percentage":
                        {
                            title = "Music Volume";
                            this.defaultValue = float.Parse(Pod.getData("default_volume_percentage", false));
                            description = "The volume of the speakers used in the EXO" +
                                "\nPlease check you are happy with how loud the speakers are before use"
                            + "\nThis setting is ignored if Per Track Music Volumes are being used";
                            break;
                        }
                    case "water_temp_setting":
                        {
                            title = "Water Temperature";
                            this.defaultValue = float.Parse(Pod.getData("default_water_temp_setting", false));
                            description = "The temperature the EXO maintains the solution at";
                            break;
                        }
                    case "storage_heater_run_speed":
                        {
                            title = "Standby Speed";
                            this.defaultValue = float.Parse(Pod.getData("default_storage_heater_run_speed", false));
                            description = "The speed the pump runs during standby" +
                                "\nIncreasing this increases heat recovery but also increases noise from the system";
                            break;
                        }
                    case "skimmer_proc1_run_time":
                        {
                            title = "Skimmer Fill Time";
                            this.defaultValue = float.Parse(Pod.getData("default_skimmer_proc1_run_time", false));
                            description = "The amount of time the pump will run to fill the EXO during a skim" +
                                "\nChange this setting to change the how much solution is in the EXO during a skim";
                            break;
                        }
                    case "skimmer_proc2_run_time":
                        {
                            title = "Skimmer Duration";
                            this.defaultValue = float.Parse(Pod.getData("default_skimmer_proc2_run_time", false));
                            description = "The amount of time the EXO will stay in skimmer before emptying";
                            break;
                        }
                    case "user_orbit_to_orbit_run_speed_middle":
                        {
                            title = "Heated Float Speed";
                            this.defaultValue = float.Parse(Pod.getData("default_user_orbit_to_orbit_run_speed_middle", false));
                            description = "The speed the pump runs at during a heated float" +
                                "\nThis should be set to the lowest value that still allows flow" +
                                "\nPlease check flow has been established before using this with customers";
                            break;
                        }
                    case "night_mode_enabled":
                        {
                            title = "Night Mode Enabled";
                            this.defaultValue = 0;
                            description = "Night mode will disable lighting during a set period" +
                                "\nto save power and prolong the LED lifetime";
                            break;
                        }
                    case "night_mode_on_hour":
                        {
                            title = "Night Mode Start Time";
                            this.defaultValue = 23;
                            description = "The time night mode starts" +
                                "\nPlease ensure this is not during opening hours";
                            break;
                        }
                    case "night_mode_off_hour":
                        {
                            title = "Night Mode End Time";
                            this.defaultValue = 7;
                            description = "The time night mode ends" +
                                "\nPlease ensure this is before opening hours";
                            break;
                        }
                    case "pre_float_heat_enabled":
                        {
                            title = "Pre-Float Heating Enabled";
                            this.defaultValue = 0;
                            description = "This mode cycles water through the heater just before a float" +
                                "\nto recover heat lost during the fill process" +
                                "\nThis may also assist in priming the pipes for emptying";
                            break;
                        }
                    case "pre_float_heat_run_time":
                        {
                            title = "Pre-Float Heating Duration";
                            this.defaultValue = 120;
                            this.minValue = 30;
                            this.maxValue = 240;
                            description = "The amount of time the pre-float heating will run for" +
                                "\nSetting this value below 30 seconds will not heat solution but may help priming the pipes" +
                                "\nThis time is added to the fill cycle. It will increase the overall length of a float session";
                            break;
                        }
                    case "pre_float_heat_use_float_time":
                        {
                            title = "Pre-Float Heating uses Float Time";
                            this.defaultValue = 0;
                            description = "This will deduct time from the float session for pre-float heating" +
                                "\nIt is up to a Float Centre Manager’s discretion if they wish to use this parameter." +
                                "\nThis parameter will decrease the Float Time and can lead to customer dissatifaction.";
                            break;
                        }
                    case "target_level_top":
                        {
                            title = "Reservoir Target Fill Level";
                            this.defaultValue = 1.38f;
                            description = "The level inside the Reservoir tank that the EXO will attempt to fill towards" +
                                "\nThis should be 0.18 less than the total level" +
                                "\nThis setting has no effect for systems with V1 reservoir tanks";
                            break;
                        }
                    case "ignore_res_sensors":
                        {
                            title = "Ignore Reservoir Sensors";
                            this.defaultValue = 0;
                            description = "This will disable the resrvoir sensors and fill/empty the EXO based on time" +
                                "\nUse this if the reservoir sensors start reporting errors" +
                                "\nThis works on all EXO reservoir tanks";
                            break;
                        }
                    case "smart_empty_enabled":
                        {
                            title = "Smart Empty";
                            this.defaultValue = 0;
                            description = "Makes the EXO use the reservoir sensors to estimate the level" +
                                "\nThis should set to on with V2 reservoir tanks" +
                                "\nOn V1 reservoir tanks, this changes the operation of the empty time";
                            break;
                        }
                    case "wash_down_available":
                        {
                            title = "Wash Down Available";
                            this.defaultValue = 0;
                            description = "Enables to wash down pump to be used" +
                                "\nWash Down mode will do nothing on systems without wash down pumps";
                            break;
                        }
                    case "wash_down_time":
                        {
                            title = "Wash Down Time";
                            this.defaultValue = 180;
                            description = "The period of time the wash down pump is enabled for";
                            break;
                        }
                    case "light_music_end_sync":
                        {
                            title = "End of Float Lighting Music Sync";
                            this.defaultValue = 0;
                            description = "Synchronises the start time of the End of Float Music and Lighting\n" +
                                "Most lighting profiles are made up of a 5 minute starting phase and a 5 minute ending phase\n" +
                                "When this is enabled, the 5 minute ending phase will start at the same time as the ending music\n" +
                                "If the ending music is longer than 5 minutes, the lighting profile will complete before the end of the float and stay white for the remaining time\n";
                            break;
                        }
                    default:
                        {
                            title = "Error 4210";
                            description = "Error - Setting Not found" +
                                "Please report error 4210 to Wellness Support"+
                                "\nSetting name: " + sqlSettingName;
                            break;
                        }

                }
            }
            public float CurrentValue
            {
                set
                {
                    if (currentValue != value)
                    {
                        currentValue = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("CurrentValue"));
                        }
                    }
                }
                get
                {
                    return currentValue;
                }
            }
            public int BoolValue
            {
                set
                {
                    if (boolValue != value)
                    {
                        boolValue = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("BoolValue"));
                        }
                    }
                }
                get
                {
                    return boolValue;
                }
            }



        }

        /// <summary>
        /// Internal class for settings side list. This is used to pick the category of settings to look at/change
        /// </summary>
        internal class SettingsList : INotifyPropertyChanged
        {
            public string Name { get; set; }
            public ImageSource Icon { get; set; }
            public ObservableCollection<SettingsItem> settings { get; set; }
            bool isEnabled { get; set; }
            Color textColor { get; set; }
            public SettingsList(string Name, ObservableCollection<SettingsItem> settings)
            {
                this.Name = Name;
                this.settings = settings;
            }
            public event PropertyChangedEventHandler PropertyChanged;

            public void setSelected(bool selected)
            {
                IsEnabled = selected;
                if(selected)
                {
                    TextColor = Colors.Black;
                }
                else
                {
                    TextColor = Colors.White;
                }
            }




            public bool IsEnabled
            {
                set
                {
                    if (isEnabled != value)
                    {
                        isEnabled = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("IsEnabled"));
                        }
                    }
                }
                get
                {
                    return isEnabled;
                }
            }

            public Color TextColor
            {
                set
                {
                    if (textColor != value)
                    {
                        textColor = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("TextColor"));
                        }
                    }
                }
                get
                {
                    return textColor;
                }
            }
        }

        public SettingsList CurrentSetting
        {
            set
            {
                if (currentSetting != value)
                {
                    currentSetting = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("CurrentSetting"));
                    }
                }
            }
            get
            {
                return currentSetting;
            }
        }

        public bool GlobalOpen
        {
            set
            {
                if (globalOpen != value)
                {
                    globalOpen = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("GlobalOpen"));
                    }
                }
            }
            get
            {
                return globalOpen;
            }
        }

        public bool FilterUVOpen
        {
            set
            {
                if (filterUVOpen != value)
                {
                    filterUVOpen = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("FilterUVOpen"));
                    }
                }
            }
            get
            {
                return filterUVOpen;
            }
        }

        public bool MainOpen
        {
            set
            {
                if (mainOpen != value)
                {
                    mainOpen = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("MainOpen"));
                    }
                }
            }
            get
            {
                return mainOpen;
            }
        }

        public bool LoggingOn
        {
            set
            {
                if (loggingOn != value)
                {
                    loggingOn = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("LoggingOn"));
                    }
                }
            }
            get
            {
                return loggingOn;
            }
        }

        public bool PerTrackVolume
        {
            set
            {
                if (perTrackVolume != value)
                {
                    perTrackVolume = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("PerTrackVolume"));
                    }
                }
            }
            get
            {
                return perTrackVolume;
            }
        }

        public int SelectedTemperature
        {
            set
            {
                if (selectedTemperature != value)
                {
                    selectedTemperature = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SelectedTemperature"));
                    }
                }
            }
            get
            {
                return selectedTemperature;
            }
        }

        public string UVtext
        {
            set
            {
                if (uvText != value)
                {
                    uvText = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("UVtext"));
                    }
                }
            }
            get
            {
                return uvText;
            }
        }

        public string FilterText
        {
            set
            {
                if (filterText != value)
                {
                    filterText = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("FilterText"));
                    }
                }
            }
            get
            {
                return filterText;
            }
        }
    }
}
