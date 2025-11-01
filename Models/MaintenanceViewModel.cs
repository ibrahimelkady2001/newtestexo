using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Xaml;
using System.Windows.Input;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EXOApp.Models
{
    /// <summary>
    /// Old, Not in use
    /// </summary>
    class MaintenanceViewModel : INotifyPropertyChanged
    {
        Models.PodModel EXO;
        public event PropertyChangedEventHandler PropertyChanged;

        public int fill { get; set; }
        public int empty { get; set; }
        public float waterTemp { get; set; }
        public int dosingAmount { get; set; }
        public int dosingTime { get; set; }
        public int musicVolume { get; set; }
        public int standbySpeed { get; set; }
        public int skimmerFillTime { get; set; }
        public int skimmerRunTime { get; set; }
        public int heatedFloatSpeed { get; set; }

        public string fillString { get; set; }
        public string emptyString { get; set; }
        public string waterTempString { get; set; }
        public string dosingAmountString { get; set; }
        public string dosingTimeString { get; set; }
        public string musicVolumeString { get; set; }
        public string standbySpeedString { get; set; }
        public string skimmerFillTimeString { get; set; }
        public string skimmerRunTimeString { get; set; }
        public string heatedFloatSpeedString { get; set; }

        private readonly Helpers.MessageInterface _messageInterface;
        public ICommand ComfirmationCommand { protected set; get; }

        public ObservableCollection<string> StandbySpeedList { get; set; }


        public MaintenanceViewModel()
        {
            ComfirmationCommand = new Command<string>(async (setting) => await changeSetting(setting));

            StandbySpeedList = new ObservableCollection<string>();
            StandbySpeedList.Add("Low");
            StandbySpeedList.Add("Medium");
            StandbySpeedList.Add("High");




            this._messageInterface = DependencyService.Get<Helpers.MessageInterface>();
            this.EXO = Globals.getCurrentPod();
            EXO.updateSettings();
            Fill = Int32.Parse(EXO.getData("fill_run_time_middle", false));
            Empty = Int32.Parse(EXO.getData("empty_run_time_middle", false));
            WaterTemp = float.Parse(EXO.getData("water_temp_setting", false));
            DosingAmount = Int32.Parse(EXO.getData("dosing_run_time", false));
            DosingTime = Int32.Parse(EXO.getData("dosing_time_of_day", false));
            MusicVolume = Int32.Parse(EXO.getData("volume_percentage", false));
            int realStandbySpeed = Int32.Parse(EXO.getData("storage_heater_run_speed", false));
            if(realStandbySpeed <=20)
            {
                StandbySpeed = 0;
            }
            else if(realStandbySpeed <= 30)
            {
                StandbySpeed = 1;
            }
            else
            {
                StandbySpeed = 2;
            }
            SkimmerFillTime = Int32.Parse(EXO.getData("skimmer_proc1_run_time", false));
            SkimmerRunTime = Int32.Parse(EXO.getData("skimmer_proc2_run_time", false));
            HeatedFloatSpeed = Int32.Parse(EXO.getData("user_orbit_to_orbit_run_speed_middle", false));
        }




        async Task changeSetting(string settingName)
        {
            bool proceed = await _messageInterface.ShowAsyncAcceptCancel("Settings", "Are you sure you would like to save this setting?", "Yes", "No");

            if(!proceed)
            {
                return;
            }
            string parameter = "";
            string value = "";
            switch(settingName)
            {
                case "Fill":
                    parameter = "fill_run_time_middle";
                    value = FillString;
                    break;
                case "Empty":
                    parameter = "empty_run_time_middle";
                    value = EmptyString;
                    break;
                case "WaterTemp":
                    parameter = "water_temp_setting";
                    value = WaterTempString;
                    break;
                case "DosingAmount":
                    parameter = "dosing_run_time";
                    value = DosingAmountString;
                    break;
                case "DosingTime":
                    parameter = "dosing_time_of_day";
                    value = DosingTimeString;
                    break;
                case "MusicVolume":
                    parameter = "volume_percentage";
                    value = MusicVolumeString;
                    break;
                case "StandbySpeed":
                    parameter = "storage_heater_run_speed";
                    if(StandbySpeed == 0)
                    {
                        value = "20";
                    }
                    else if(StandbySpeed == 1)
                    {
                        value = "30";
                    }
                    else
                    {
                        value = "40";
                    }
                    break;
                case "SkimmerFillTime":
                    parameter = "skimmer_proc1_run_time";
                    value = SkimmerFillTimeString;
                    break;
                case "SkimmerRunTime":
                    parameter = "skimmer_proc2_run_time";
                    value = SkimmerRunTimeString;
                    break;
                case "HeatedFloatSpeed":
                    parameter = "user_orbit_to_orbit_run_speed_middle";
                    value = HeatedFloatSpeedString;
                    break;
            }
                EXO.changeOrbitConfigSetting(parameter, value);
        }


        async void temperatureButtonClick(object sender, EventArgs args)
        {
            await changeSetting("temperature");
        }


        //---------------------------------------------------------------------------------------------------
        // Model Live Updates
        //---------------------------------------------------------------------------------------------------

        public int Fill
        {
            set
            {
                if (fill != value)
                {
                    fill = value;
                    FillString = Fill.ToString();
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Fill"));
                    }
                }
            }
            get
            {
                return fill;
            }
        }
        public int Empty
        {
            set
            {
                if (empty != value)
                {
                    empty = value;
                    EmptyString = Empty.ToString();
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Empty"));
                    }
                }
            }
            get
            {
                return empty;
            }
        }
        public float WaterTemp
        {
            set
            {
                if (waterTemp != value)
                {
                    waterTemp = value;
                    WaterTempString = WaterTemp.ToString();
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("WaterTemp"));
                    }
                }
            }
            get
            {
                return waterTemp;
            }
        }
        public int DosingAmount
        {
            set
            {
                if (dosingAmount != value)
                {
                    dosingAmount = value;
                    DosingAmountString = DosingAmount.ToString();
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("DosingAmount"));
                    }
                }
            }
            get
            {
                return dosingAmount;
            }
        }
        public int DosingTime
        {
            set
            {
                if (dosingTime != value)
                {
                    dosingTime = value;
                    DosingTimeString = DosingTime.ToString();
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("DosingTime"));
                    }
                }
            }
            get
            {
                return dosingTime;
            }
        }
        public int MusicVolume
        {
            set
            {
                if (musicVolume != value)
                {
                    musicVolume = value;
                    MusicVolumeString = MusicVolume.ToString();
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("MusicVolume"));
                    }
                }
            }
            get
            {
                return musicVolume;
            }
        }
        public int StandbySpeed
        {
            set
            {
                if (standbySpeed != value)
                {
                    standbySpeed = value;
                    StandbySpeedString = StandbySpeed.ToString();
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("StandbySpeed"));
                    }
                }
            }
            get
            {
                return standbySpeed;
            }
        }
        public int SkimmerFillTime
        {
            set
            {
                if (skimmerFillTime != value)
                {
                    skimmerFillTime = value;
                    SkimmerFillTimeString = SkimmerFillTime.ToString();
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SkimmerFillTime"));
                    }
                }
            }
            get
            {
                return fill;
            }
        }
        public int SkimmerRunTime
        {
            set
            {
                if (skimmerRunTime != value)
                {
                    skimmerRunTime = value;
                    SkimmerRunTimeString = SkimmerRunTime.ToString();
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SkimmerRunTime"));
                    }
                }
            }
            get
            {
                return skimmerRunTime;
            }
        }
        public int HeatedFloatSpeed
        {
            set
            {
                if (heatedFloatSpeed != value)
                {
                    heatedFloatSpeed = value;
                    HeatedFloatSpeedString = HeatedFloatSpeed.ToString();
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("HeatedFloatSpeed"));
                    }
                }
            }
            get
            {
                return heatedFloatSpeed;
            }
        }


        public string FillString
        {
            set
            {
                if (fillString != value)
                {
                    fillString = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("FillString"));
                    }
                }
            }
            get
            {
                return fillString;
            }
        }
        public string EmptyString
        {
            set
            {
                if (emptyString != value)
                {
                    emptyString = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("EmptyString"));
                    }
                }
            }
            get
            {
                return emptyString;
            }
        }
        public string WaterTempString
        {
            set
            {
                if (waterTempString != value)
                {
                    waterTempString = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("WaterTempString"));
                    }
                }
            }
            get
            {
                return waterTempString;
            }
        }
        public string DosingAmountString
        {
            set
            {
                if (dosingAmountString != value)
                {
                    dosingAmountString = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("DosingAmountString"));
                    }
                }
            }
            get
            {
                return dosingAmountString;
            }
        }
        public string DosingTimeString
        {
            set
            {
                if (dosingTimeString != value)
                {
                    dosingTimeString = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("DosingTimeString"));
                    }
                }
            }
            get
            {
                return dosingTimeString;
            }
        }
        public string MusicVolumeString
        {
            set
            {
                if (musicVolumeString != value)
                {
                    musicVolumeString = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("MusicVolumeString"));
                    }
                }
            }
            get
            {
                return musicVolumeString;
            }
        }
        public string StandbySpeedString
        {
            set
            {
                if (standbySpeedString != value)
                {
                    standbySpeedString = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("StandbySpeedString"));
                    }
                }
            }
            get
            {
                return standbySpeedString;
            }
        }
        public string SkimmerFillTimeString
        {
            set
            {
                if (skimmerFillTimeString != value)
                {
                    skimmerFillTimeString = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SkimmerFillTimeString"));
                    }
                }
            }
            get
            {
                return skimmerFillTimeString;
            }
        }
        public string SkimmerRunTimeString
        {
            set
            {
                if (skimmerRunTimeString != value)
                {
                    skimmerRunTimeString = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SkimmerRunTimeString"));
                    }
                }
            }
            get
            {
                return skimmerRunTimeString;
            }
        }
        public string HeatedFloatSpeedString
        {
            set
            {
                if (heatedFloatSpeedString != value)
                {
                    heatedFloatSpeedString = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("HeatedFloatSpeedString"));
                    }
                }
            }
            get
            {
                return heatedFloatSpeedString;
            }
        }



    }
}
