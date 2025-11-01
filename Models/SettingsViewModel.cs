using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EXOApp.Models
{
    /// <summary>
    /// Settings view model. Not used. Use SettingViewModelV2
    /// </summary>
    class SettingsViewModel
    {
        public ObservableCollection<SettingsGroup> settingsObservable { get; set; }
        PodModel pod;

        SettingsGroup Pressure;
        SettingsGroup Temperature;
        SettingsGroup Fill;
        SettingsGroup Empty;
        SettingsGroup OrbitToOrbit;
        SettingsGroup Dosing;
        SettingsGroup Skimmer;
        SettingsGroup PrePrime;
        SettingsGroup Unsorted;




        public SettingsViewModel()
        {
            Pressure = new SettingsGroup("Pressure", new List<Settings>());
            Temperature = new SettingsGroup("Temperature", new List<Settings>());
            Fill = new SettingsGroup("Fill", new List<Settings>());
            Empty = new SettingsGroup("Empty", new List<Settings>());
            OrbitToOrbit = new SettingsGroup("EXO To EXO", new List<Settings>());
            Dosing = new SettingsGroup("Dosing", new List<Settings>());
            Skimmer = new SettingsGroup("Skimmer", new List<Settings>());
            PrePrime = new SettingsGroup("Pre-Prime", new List<Settings>());
            Unsorted = new SettingsGroup("Unsorted", new List<Settings>());






            pod = Globals.getCurrentPod();
            pod.updateSettings();
            List<Settings> settings = new List<Settings>();
            for (int i = 0; i < pod.settings.Count; i++)
            {
                generateCategory(pod.settings[i].Name, newSetting(pod.settings[i].Name));
            }
            settingsObservable = new ObservableCollection<SettingsGroup>();
            settingsObservable.Add(Pressure);
            settingsObservable.Add(Temperature);
            settingsObservable.Add(Fill);
            settingsObservable.Add(Empty);
            settingsObservable.Add(OrbitToOrbit);
            settingsObservable.Add(Dosing);
            settingsObservable.Add(Skimmer);
            settingsObservable.Add(PrePrime);
            settingsObservable.Add(Unsorted);
        }

        private Settings newSetting(string name)
        {
            Settings setting = new Settings(name, pod.getData(name, false), generateDescription(name), generateDefaultValue(name), generateDisplayName(name));
            return setting;
        }

        private string generateDescription(string name)
        {
            string description = "";
            switch (name)
            {
                case "water_temp_setting":
                    description = "The temperature the heater should maintain the water";
                    break;
                case "storage_heater_run_speed":
                    description = "The pump speed during standby";
                    break;
                default:
                    description = name.Replace("_", " ");
                    break;
            }
            return description;
        }

        private string generateDisplayName(string name)
        {
            string displayName = "";
            switch (name)
            {
                case "water_temp_setting":
                    displayName = "Water Temperature";
                    break;
                case "storage_heater_run_speed":
                    displayName = "Standby Speed";
                    break;
                default:
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                    displayName = name.Replace("_", " ");
                    displayName = displayName.Replace("proc1", "start");
                    displayName = displayName.Replace("proc2", "middle");
                    displayName = displayName.Replace("proc3", "end");
                    displayName = textInfo.ToTitleCase(displayName);
                    break;
            }
            return displayName;
        }

        private string generateDefaultValue(string name)
        {
            string defaultValue = "";
            switch (name)
            {
                case "water_temp_setting":
                    defaultValue = "37";
                    break;
                case "storage_heater_run_speed":
                    defaultValue = "30";
                    break;
                default:
                    defaultValue = name.Replace("_", " ");
                    break;
            }
            return defaultValue;
        }

        private void generateCategory(string name, Settings setting)
        {
            if (name.Contains("pressure"))
            {
                Pressure.Add(setting);
            }
            else if (name.Contains("temp"))
            {
                Temperature.Add(setting);
            }
            else if (name.Contains("fill"))
            {
                Fill.Add(setting);
            }
            else if(name.Contains("empty"))
            {
                Empty.Add(setting);
            }
            else if(name.Contains("orbit_orbit") || name.Contains("orbit_to_orbit"))
            {
                OrbitToOrbit.Add(setting);
            }
            else if(name.Contains("dosing"))
            {
                Dosing.Add(setting);
            }
            else if (name.Contains("skimmer"))
            {
                Skimmer.Add(setting);
            }
            else if (name.Contains("pre_prime"))
            {
                PrePrime.Add(setting);
            }
            else
            {
                Unsorted.Add(setting);
            }
        }

        internal class Settings
        {
            public string name { get; set; }
            public string value { get; set; }
            public string description { get; set; }
            public string defaultValue {get; set;}
            public string displayName { get; set; }

            public Settings(string name, string value, string description, string defaultValue, string displayName)
            {
                this.name = name;
                this.value = value;
                this.description = description;
                this.defaultValue = defaultValue;
                this.displayName = displayName;

            }
        }

        internal class SettingsGroup : List<Settings>
        {
            public string name { get; set; }

            public SettingsGroup(string name, List<Settings> settings) : base(settings)
            {
                this.name = name;
          
            }

        }

    }
}
