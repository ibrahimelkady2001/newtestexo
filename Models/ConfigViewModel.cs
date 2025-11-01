using EXOApp.GlobalsClasses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;


namespace EXOApp.Models
{
    /// <summary>
    /// Handles the Pod Config section of the Pod
    /// </summary>
    class ConfigViewModel
    {

        public ICommand OnExit { protected set; get; }

        private readonly Helpers.MessageInterface _messageInterface;
        PodModel Orb;
        ObservableCollection<Config> configList = new ObservableCollection<Config>();
        public ObservableCollection<Config> ConfigList { get { return configList; } }


        public string PodLabel { get; set; }

        /// <summary>
        /// Constructor for Config model. Gets the current Pod and create UI items for each config setting
        /// </summary>
        public ConfigViewModel()
        {
            Orb = Globals.getCurrentPod();
            this._messageInterface = DependencyService.Get<Helpers.MessageInterface>();

            PodLabel = "Pod - " + Orb.podNumber;
            OnExit = new Command(async () => await onExit());
            List<ConfigSettingPoint> config = Orb.getOrbitConfig();
            if(config == null)
            {
                _ = databaseError();
                return;
            }
            foreach(ConfigSettingPoint point in config)
            {
                configList.Add(new Config(point.description, point.Value, point.units, point.Name, _messageInterface, Orb));
            }
        }
        /// <summary>
        /// Checks for database errors. Returns to dashboard on error
        /// </summary>
        /// <returns></returns>
        async Task databaseError()
        {
            await _messageInterface.ShowAsyncOK("Database Error", "Config Files could not be found.\nReturning to dashboard", "OK");
            await onExit();
        }
        /// <summary>
        /// Returns to the main dashboard.
        /// </summary>
        /// <returns></returns>
        private async Task onExit()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }

        /// <summary>
        /// Class for each individual config settings
        /// </summary>
        public class Config : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public ICommand SettingsChange { protected set; get; }

            PodModel Orb;
            public int Decimals { get; set; }
            public string DisplayName { get; set; }
            public string Units { get; set; }

            string sqlSettingName;
            string currentValue;
            float entryValue;
            public string Category { get; set; }
            private readonly Helpers.MessageInterface _messageInterface;

            /// <summary>
            /// Gets the Config Values and formats them correctly
            /// </summary>
            /// <param name="displayName">Readable name of the config</param>
            /// <param name="currentValue">Current Value of the config</param>
            /// <param name="units">Units of the config</param>
            /// <param name="sqlName">Name of the config from the Pod Database</param>
            /// <param name="messageInterface">Used to show popup messages</param>
            /// <param name="orb">Current Pod</param>
            public Config(string displayName, string currentValue, string units, string sqlName, Helpers.MessageInterface messageInterface , PodModel orb)
            {
                Orb = orb;
                SettingsChange = new Command(async () => await saveSetting());
                DisplayName = displayName;
                Units = units;
                _messageInterface = messageInterface;
                sqlSettingName = sqlName;
                getCategory();
                CurrentValue = currentValue + " " + Units;
                float entry;
                if(float.TryParse(currentValue, out entry))
                {
                    EntryValue = entry;
                }
                else
                {
                    EntryValue = 0;
                }
            }
            /// <summary>
            /// Works out the category for each config
            /// </summary>
            void getCategory()
            {

                switch(sqlSettingName)
                {
                    case "water_temp_setting" :
                        {
                            Category = "Temperature";
                            Units = "°C";
                            Decimals = 2;
                            break;
                        }
                    case "ui_status_line":
                        {
                            Category = "Misc";
                            DisplayName = "Show Display Status Line";
                            Decimals = 0;
                            break;
                        }
                    case "storage_heater_run_speed":
                        {
                            Category = "Standby";
                            DisplayName = "Heater Run Speed";
                            Decimals = 0;
                            break;
                        }
                    case "fill_run_speed_start":
                        {
                            Category = "Fill";
                            Decimals = 0;
                            break;
                        }
                    case "fill_run_speed_middle":
                        {
                            Category = "Fill";
                            Decimals = 0;
                            break;
                        }
                    case "fill_run_speed_end":
                        {
                            Category = "Fill";
                            Decimals = 0;
                            break;
                        }
                    case "fill_run_time_start":
                        {
                            Category = "Fill";
                            Decimals = 0;
                            break;
                        }
                    case "fill_run_time_middle":
                        {
                            Category = "Fill";
                            Decimals = 0;
                            break;
                        }
                    case "fill_run_time_end":
                        {
                            Category = "Fill";
                            Decimals = 0;
                            break;
                        }
                    case "empty_run_speed_start":
                        {
                            Category = "Empty";
                            Decimals = 0;
                            break;
                        }
                    case "empty_run_speed_middle":
                        {
                            Category = "Empty";
                            Decimals = 0;
                            break;
                        }
                    case "empty_run_speed_end":
                        {
                            Category = "Empty";
                            Decimals = 0;
                            break;
                        }
                    case "empty_run_time_start":
                        {
                            Category = "Empty";
                            Decimals = 0;
                            break;
                        }
                    case "empty_run_time_middle":
                        {
                            Category = "Empty";
                            Decimals = 0;
                            break; 
                        }
                    case "empty_run_time_end":
                        {
                            Category = "Empty";
                            Decimals = 0;
                            break; 
                        }
                    case "float_default_time_to_start":
                        {
                            Category = "Float";
                            Decimals = 0;
                            break; 
                        }
                    case "orbit_orbit_run_speed_end":
                        {
                            Category = "EXO to EXO";
                            DisplayName = "EXO to EXO Run Speed End";
                            Decimals = 0;
                            break;
                        }
                    case "filtering_run_speed":
                        {
                            Category = "Misc";
                            Decimals = 0;
                            break;
                        }
                    case "skimmer_proc1_run_speed":
                        {
                            Category = "Skimmer";
                            Decimals = 0;
                            break;
                        }
                    case "skimmer_proc1_run_time":
                        {
                            Category = "Skimmer";
                            Decimals = 0;
                            break;
                        }
                    case "skimmer_proc2_run_speed":
                        {
                            Category = "Skimmer";
                            Decimals = 0;
                            break;
                        }
                    case "skimmer_proc2_run_time":
                        {
                            Category = "Skimmer";
                            Decimals = 0;
                            break;
                        }
                    case "skimmer_proc3_run_time":
                        {
                            Category = "Skimmer";
                            Decimals = 0;
                            break;
                        }
                    case "skimmer_proc3_run_speed":
                        {
                            Category = "Skimmer";
                            Decimals = 0;
                            break;
                        }
                    case "water_temp_tolerance":
                        {
                            Category = "Temperature";
                            DisplayName = "Water Temperature Tolerance";
                            Units = "°C";
                            Decimals = 2;
                            break;
                        }
                    case "dosing_time_of_day":
                        {
                            Category = "Dosing";
                            Decimals = 0;
                            break;
                        }
                    case "dosing_sample_time":
                        {
                            Category = "Dosing";
                            Decimals = 0;
                            break;
                        }
                    case "dosing_run_time":
                        {
                            Category = "Dosing";
                            Decimals = 0;
                            break;
                        }
                    case "dosing_min_level":
                        {
                            Category = "Dosing";
                            Decimals = 0;
                            break;
                        }
                    case "dosing_run_speed":
                        {
                            Category = "Dosing";
                            Decimals = 0;
                            break;
                        }
                    case "night_mode_enabled":
                        {
                            Category = "Lighting";
                            Decimals = 0;
                            break;
                        }
                    case "night_mode_on_hour":
                        {
                            Category = "Lighting";
                            Decimals = 0;
                            break;
                        }
                    case "night_mode_off_hour":
                        {
                            Category = "Lighting";
                            Decimals = 0;
                            break;
                        }
                    case "blocked_pressure":
                        {
                            Category = "Plus";
                            Decimals = 0;
                            break;
                        }
                    case "negate_inverter_speed":
                        {
                            Category = "Hardware";
                            Decimals = 0;
                            break;
                        }
                    case "pre_prime_enabled":
                        {
                            Category = "Empty";
                            Decimals = 0;
                            break;
                        }
                    case "pre_prime_speed":
                        {
                            Category = "Empty";
                            Decimals = 0;
                            break;
                        }
                    case "pre_prime_duration":
                        {
                            Category = "Empty";
                            Decimals = 0;
                            break;
                        }
                    case "volume_percentage":
                        {
                            Category = "Float";
                            DisplayName = "Master Volume";
                            Decimals = 0;
                            break;
                        }
                    case "temp_pump_offset":
                        {
                            Category = "Temperature";
                            Units = "°C";
                            Decimals = 2;
                            break;
                        }
                    case "temp_orbit_offset":
                        {
                            Category = "Temperature";
                            DisplayName = "Pod Temperature";
                            Units = "°C";
                            Decimals = 2;
                            break;
                        }
                    case "temp_res_offset":
                        {
                            Category = "Temperature";
                            Units = "°C";
                            Decimals = 2;
                            break;
                        }
                    case "sg_offset":
                        {
                            Category = "Plus";
                            Decimals = 2;
                            break;
                        }
                    case "pump_temp_cutoff":
                        {
                            Category = "Temperature";
                            Units = "°C";
                            Decimals = 2;
                            break;
                        }
                    case "orbit_orbit_run_speed_start":
                        {
                            Category = "EXO to EXO";
                            DisplayName = "EXO to EXO Run Speed Start";
                            Decimals = 0;
                            break;
                        }
                    case "orbit_orbit_run_speed_middle":
                        {
                            Category = "EXO to EXO";
                            DisplayName = "EXO to EXO Run Speed Middle";
                            Decimals = 0;
                            break;
                        }
                    case "orbit_orbit_run_time_start":
                        {
                            Category = "EXO to EXO";
                            DisplayName = "EXO to EXO Run Time Start";
                            Decimals = 0;
                            break;
                        }
                    case "orbit_orbit_run_time_middle":
                        {
                            Category = "EXO to EXO";
                            DisplayName = "EXO to EXO Run Time Middle";
                            Decimals = 0;
                            break;
                        }
                    case "orbit_orbit_run_time_end":
                        {
                            Category = "EXO to EXO";
                            DisplayName = "EXO to EXO Run Time End";
                            Decimals = 0;
                            break;
                        }
                    case "h2o2_offset":
                        {
                            Category = "Plus";
                            Decimals = 0;
                            break;
                        }
                    case "orbit_empty_pressure_hex":
                        {
                            Category = "Plus";
                            DisplayName = "Pod Empty Pressure Hex";
                            Decimals = 0;
                            break;
                        }
                    case "orbit_full_pressure_hex":
                        {
                            Category = "Plus";
                            DisplayName = "Pod Full Pressure Hex";
                            Decimals = 0;
                            break;
                        }
                    case "res_empty_pressure_hex":
                        {
                            Category = "Plus";
                            Decimals = 0;
                            break;
                        }
                    case "res_full_pressure_hex":
                        {
                            Category = "Plus";
                            Decimals = 0;
                            break;
                        }
                    case "default_storage_heater_run_speed":
                        {
                            Category = "Default";
                            DisplayName = "Heater Run Speed";
                            Decimals = 0;
                            break;
                        }
                    case "default_fill_run_time_middle":
                        {
                            Category = "Default";
                            Decimals = 0;
                            break;
                        }
                    case "default_empty_run_time_middle":
                        {
                            Category = "Default";
                            Decimals = 0;
                            break;
                        }
                    case "default_dosing_run_time":
                        {
                            Category = "Default";
                            Decimals = 0;
                            break;
                        }
                    case "default_dosing_time_of_day":
                        {
                            Category = "Default";
                            Decimals = 0;
                            break;
                        }
                    case "default_skimmer_proc1_run_time":
                        {
                            Category = "Default";
                            Decimals = 0;
                            break;
                        }
                    case "default_skimmer_proc2_run_time":
                        {
                            Category = "Default";
                            Decimals = 0;
                            break;
                        }
                    case "default_water_temp_tolerance":
                        {
                            Category = "Default";
                            DisplayName = "Water Temperature Tolerance";
                            Decimals = 2;
                            break;
                        }
                    case "default_water_temp_setting":
                        {
                            Category = "Default";
                            Decimals = 2;
                            break;
                        }
                    case "orbit_to_orbit_during_float":
                        {
                            Category = "Float";
                            DisplayName = "Heated Float Enabled";
                            Decimals = 0;
                            break;
                        }
                    case "user_orbit_to_orbit_run_speed_start":
                        {
                            Category = "Float";
                            DisplayName = "Heated Float Starting Speed";
                            Decimals = 0;
                            break;
                        }
                    case "user_orbit_to_orbit_run_speed_middle":
                        {
                            Category = "Float";
                            DisplayName = "Heated Float During Float Speed";
                            Decimals = 0;
                            break;
                        }
                    case "user_orbit_to_orbit_run_time_start":
                        {
                            Category = "Float";
                            DisplayName = "Heated Float Start Time";
                            Decimals = 0;
                            break;
                        }
                    case "default_volume_percentage":
                        {
                            Category = "Default";
                            DisplayName = "Master Volume";
                            Decimals = 0;
                            break;
                        }
                    case "default_user_orbit_to_orbit_run_speed_middle":
                        {
                            Category = "Default";
                            DisplayName = "Heated Float During Float Speed";
                            Decimals = 0;
                            break;
                        }
                    case "res_tank_fill_level":
                        {
                            Category = "Misc";
                            DisplayName += "(X)";
                            Decimals = 2;
                            break;
                        }
                    case "orbit_revision":
                        {
                            Category = "Hardware";
                            DisplayName = "IO Revision";
                            Decimals = 0;
                            break;
                        }
                    case "level_sensor_enabled":
                        {
                            Category = "Hardware";
                            Decimals = 0;
                            break;
                        }
                    case "ignore_res_sensors":
                        {
                            Category = "Reservoir";
                            Decimals = 0;
                            break;
                        }
                    case "standard_led_on":
                        {
                            Category = "Lighting";
                            Decimals = 0;
                            break;
                        }
                    case "smart_empty_enabled":
                        {
                            Category = "Empty";
                            Decimals = 0;
                            break;
                        }
                    case "max_empty_time":
                        {
                            Category = "Empty";
                            Decimals = 0;
                            break;
                        }
                    case "target_level_top":
                        {
                            Category = "Reservoir";
                            Decimals = 2;
                            break;
                        }
                    case "target_level_bottom":
                        {
                            Category = "Reservoir";
                            Decimals = 2;
                            break;
                        }
                    case "pre_float_heat_enabled":
                        {
                            Category = "Float";
                            Decimals = 0;
                            break;
                        }
                    case "pre_float_heat_inverter_speed":
                        {
                            Category = "Float";
                            Decimals = 0;
                            break;
                        }
                    case "pre_float_heat_run_time":
                        {
                            Category = "Float";
                            Decimals = 0;
                            break;
                        }
                    case "pre_float_heat_use_float_time":
                        {
                            Category = "Float";
                            Decimals = 0;
                            break;
                        }
                    case "wash_down_available":
                        {
                            Category = "Hardware";
                            Decimals = 0;
                            break;
                        }
                    case "wash_down_time":
                        {
                            Category = "Wash Down";
                            Decimals = 0;
                            Units = "s";
                            break;
                        }
                    case "light_music_end_sync":
                        {
                            Category = "Lighting";
                            Decimals = 0;
                            break;
                        }
                    default:
                        {
                            Category = "Uncategorised";
                            Decimals = 2;
                            break;
                        }
                        
                }
                if(Category == "Default")
                {
                    DisplayName += " (D)";
                }
                if (Units == "Bool" || Units == "bool")
                {
                    Units = "";
                }
                if (Units == "Hour")
                {
                    Units = "O'Clock";
                }
                if (Units == "%-age")
                {
                    Units = "%";
                }
            }
            /// <summary>
            /// Saves the setting change to the database
            /// </summary>
            /// <returns></returns>
            async Task saveSetting()
            {
                bool response = await _messageInterface.ShowAsyncAcceptCancel("Settings Change", "Are you sure you want to change " + DisplayName + " to " + EntryValue.ToString() + Units + "?", "Yes", "No");
                
                if(!response)
                {
                    return;
                }
                if (!Orb.changeOrbitConfigSetting(sqlSettingName, EntryValue.ToString()))
                {
                    await _messageInterface.ShowAsyncOK("Error 1015", "Unable to save settings. Please try again", "Close");
                    return;
                }
                _ = _messageInterface.ShowAsyncOK("Settings Changed", DisplayName + " has successfully been changed", "Close");
                Orb.updateSettings();
                CurrentValue = EntryValue.ToString() + " " + Units;
            }


            public string CurrentValue
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
            public float EntryValue
            {
                set
                {
                    if (entryValue != value)
                    {
                        entryValue = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("EntryValue"));
                        }
                    }
                }
                get
                {
                    return entryValue;
                }
            }

        }
    }
}
