using EXOApp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EXOApp.Models
{
    /// <summary>
    /// View model for the Plus view
    /// </summary>
    class PlusViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand OnExit { protected set; get; }

        Color primaryRows = Color.FromArgb("deebf7");
        Color secondaryRows = Color.FromArgb("ffffff");
        Color titleRows = Color.FromArgb("1c82c5");

        public string PodLabel { get; set; }
        PodModel Pod;
        ObservableCollection<Parameter> parameters = new ObservableCollection<Parameter>();
        public ObservableCollection<Parameter> Parameters { get { return parameters; } }
        bool timerRunning;
        bool running;

        int podMissCount;
        string podTime;

        bool indicatorRunning;

        /// <summary>
        /// Constructor for the plus view model
        /// </summary>
        public PlusViewModel()
        {
            OnExit = new Command(async () => await onExit());
            Pod = Globals.getCurrentPod();
            doLists();
            timerRunning = true;
            PodLabel = "EXO - " + Pod.podNumber;
            IndicatorRunning = true;
            // TODO Xamarin.Forms.Device.StartTimer is no longer supported. Use Microsoft.Maui.Dispatching.DispatcherExtensions.StartTimer instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            Device.StartTimer(TimeSpan.FromSeconds(2), () =>
            {
                _ = update();
                return timerRunning;
            });
        }

        /// <summary>
        /// Updates the Plus view.
        /// </summary>
        async Task update()
        {
            if(running)
            {
                return;
            }
            running = true;
            await Task.Run(() =>
            {
                Pod.updateStatus();
                Pod.updateSettings();
                foreach(Parameter para in Parameters)
                {
                    para.update();
                }
                checkPodRunning();
            });
            running = false;
        }

        /// <summary>
        /// Checks whether the Pod is running
        /// </summary>
        void checkPodRunning()
        {

            if (Pod.getData("lastUpdateSecond", true) == podTime)
            {
                podMissCount++;
                if (podMissCount > 2)
                {
                    IndicatorRunning = false;
                }
            }
            else
            {
                podMissCount = 0;
                podTime = Pod.getData("lastUpdateSecond", true);
                IndicatorRunning = true;
            }

        }

        /// <summary>
        /// Adds all the parameters to the list displayed on the plus view
        /// </summary>
        void doLists()
        {
            Parameters.Add(new Parameter("Divider", Pod, titleRows, "Temperature"));

            Parameters.Add(new Parameter("TempRes", Pod, primaryRows));
            Parameters.Add(new Parameter("TempPump", Pod, secondaryRows));
            Parameters.Add(new Parameter("TempOrbit", Pod, primaryRows));

            Parameters.Add(new Parameter("Divider", Pod, titleRows, "Hardware Parameters"));

            Parameters.Add(new Parameter("SysMode", Pod, primaryRows));
            Parameters.Add(new Parameter("HeaterMode", Pod, secondaryRows));
            Parameters.Add(new Parameter("PumpSpeed", Pod, primaryRows));
            Parameters.Add(new Parameter("InValvePos", Pod, secondaryRows));
            Parameters.Add(new Parameter("OutValvePos", Pod, primaryRows));
            Parameters.Add(new Parameter("Flow", Pod, secondaryRows));
            if (Pod.getData("level_sensor_enabled", false) == "1")
            {
                Parameters.Add(new Parameter("ResTankLevel", Pod, primaryRows));
                Parameters.Add(new Parameter("flowRate", Pod, secondaryRows));
                Parameters.Add(new Parameter("DoorState", Pod, primaryRows));
                Parameters.Add(new Parameter("HeaterCurrent", Pod, secondaryRows));
            }
            else
            {
                Parameters.Add(new Parameter("FloatSwitch", Pod, primaryRows));
                Parameters.Add(new Parameter("DoorState", Pod, secondaryRows));
                Parameters.Add(new Parameter("HeaterCurrent", Pod, primaryRows));
            }


            Parameters.Add(new Parameter("Divider", Pod, titleRows, "Plus"));

            Parameters.Add(new Parameter("pH", Pod, primaryRows));
            Parameters.Add(new Parameter("h2o_Cl", Pod, secondaryRows));
            Parameters.Add(new Parameter("SolDen", Pod, primaryRows));
            Parameters.Add(new Parameter("OrbitSolPres", Pod, secondaryRows));
            Parameters.Add(new Parameter("ResSolPres", Pod, primaryRows));
            Parameters.Add(new Parameter("BagFS", Pod, secondaryRows));


        }

        /// <summary>
        /// Exits the Plus view and returns to the dashboard
        /// </summary>
        private async Task onExit()
        {
                timerRunning = false;
                await App.Current.MainPage.Navigation.PopAsync();
        }

        /// <summary>
        /// Internal class for the parameters
        /// </summary>
        internal class Parameter : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            
            public string Units { get; set; }
            public bool Gauge { get; set; }
            public string Name { get; set; }
            public float Min { get; set; }
            public float Max { get; set; }
            public float LowerQuartile { get; set; }
            public float UpperQuartile { get; set; }
            public string Title { get; set; }
            public Color BackgroundColour { get; set; }
            string SQLName { get; set; }

            float readValue { get; set; }
            string stringValue { get; set; }
            string status { get; set; }

            PodModel Orb;

            /// <summary>
            /// Contstructor for the parameter
            /// </summary>
            public Parameter(string sqlname, PodModel orb, Color BackgroundColour ,string title = "")
            {
                this.BackgroundColour = BackgroundColour;
                SQLName = sqlname;
                Orb = orb;
                fillDetails();
                Title = title;
                update();
            }

            /// <summary>
            /// Updates the values for the parameter
            /// </summary>
            public void update()
            {
                if(SQLName == "Divider")
                {
                    return;
                }
                if(Gauge)
                {
                    ReadValue = float.Parse(Orb.getData(SQLName, true));
                }
                else
                {
                    StringValue = Orb.getData(SQLName, true);
                }
            }

            /// <summary>
            /// Sets details for specific parameters
            /// </summary>
            void fillDetails()
            {
                switch (SQLName)
                {
                    case "Divider":
                        {
                            Gauge = false;
                            StringValue = "";
                            break;
                        }
                    case "TempRes":
                        {
                            Name = "Reservoir Temperature";
                            Gauge = true;
                            Units = "°C";
                            Min = 0;
                            Max = 45;
                            LowerQuartile = 34;
                            UpperQuartile = 40;
                            break;
                        }
                    case "TempPump":
                        {
                            Name = "Pump Temperature";
                            Gauge = true;
                            Units = "°C";
                            Min = 0;
                            Max = 45;
                            LowerQuartile = 34;
                            UpperQuartile = 40;
                            break;
                        }
                    case "TempOrbit":
                        {
                            Name = "Pod Temperature";
                            Gauge = true;
                            Units = "°C";
                            Min = 0;
                            Max = 45;
                            LowerQuartile = 34;
                            UpperQuartile = 40;

                            break;
                        }
                    case "pH":
                        {
                            Name = "pH Level"; 
                            Gauge = true;
                            Units = "pH";
                            Min = 0;
                            Max = 14;
                            LowerQuartile = 4;
                            UpperQuartile = 9;

                            break;
                        }
                    case "h2o_Cl":
                        {
                            Name = "H2O2 Level"; 
                            Gauge = true;
                            Units = "ppm";
                            Min = 0;
                            Max = 500;
                            LowerQuartile = 30;
                            UpperQuartile = 300;
                            break;
                        }
                    case "SolDen":
                        {
                            Name = "Solution Density"; 
                            Gauge = true;
                            Units = "SG";
                            Min = 0.9f;
                            Max = 1.5f;
                            LowerQuartile = 1.2f;
                            UpperQuartile = 1.3f;
                            break;
                        }
                    case "OrbitSolPres":
                        {                          
                            Name = "Pod Level Pressure";
                            Gauge = true;
                            Units = "%";
                            Min = 0;
                            Max = 100;
                            LowerQuartile = 0;
                            UpperQuartile = 100;
                            break;
                        }
                    case "ResSolPres":
                        {
                            Name = "Reservoir Level Pressure";
                            Gauge = true;
                            Units = "%";
                            Min = 0;
                            Max = 100;
                            LowerQuartile = 0;
                            UpperQuartile = 100;
                            break;
                        }
                    case "BagFS":
                        {
                            Name = "Bag Filter Pressure";
                            Gauge = true;
                            Units = "%";
                            Min = 0;
                            Max = 100;
                            LowerQuartile = 0;
                            UpperQuartile = 100;
                            break;
                        }
                    case "SysMode":
                        {
                            Name = "System Mode";
                            Gauge = false;
                            Units = "";
                            Min = 0;
                            Max = 0;
                            LowerQuartile = 0;
                            UpperQuartile = 0;
                            break;
                        }
                    case "HeaterMode":
                        {
                            Name = "Heater Mode";
                            Gauge = false;
                            Units = ""; 
                            Min = 0;
                            Max = 0;
                            LowerQuartile = 0;
                            UpperQuartile = 0;
                            break;
                        }
                    case "PumpSpeed":
                        {
                            Name = "Pump Speed";
                            Gauge = true;
                            Units = "Hz";
                            Min = 0;
                            Max = 50;
                            LowerQuartile = 0;
                            UpperQuartile = 50;
                            break;
                        }
                    case "InValvePos":
                        {
                            Name = "Inlet Valve Postiion";
                            Gauge = false;
                            Units = "";
                            Min = 0;
                            Max = 0;
                            LowerQuartile = 0;
                            UpperQuartile = 0;
                            break;
                        }
                    case "OutValvePos":
                        {
                            Name = "Outlet Valve Position";
                            Gauge = false;
                            Units = "";
                            Min = 0;
                            Max = 0;
                            LowerQuartile = 0;
                            UpperQuartile = 0;
                            break;
                        }
                    case "Flow":
                        {
                            Name = "Flow";
                            Gauge = false;
                            Units = "";
                            Min = 0;
                            Max = 0;
                            LowerQuartile = 0;
                            UpperQuartile = 0;
                            break;
                        }
                    case "FloatSwitch":
                        {
                            Name = "Float Switch";
                            Gauge = false;
                            Units = "";
                            Min = 0;
                            Max = 0;
                            LowerQuartile = 0;
                            UpperQuartile = 0;
                            break;
                        }
                    case "ResTankLevel":
                        {
                            Name = "Reservoir Solution Level";
                            Gauge = true;
                            Units = "m";
                            Min = 0;
                            Max = 2.5f;
                            LowerQuartile = 0;
                            UpperQuartile = 1.7f;
                            break;
                        }
                    case "DoorState":
                        {
                            Name = "Door Status";
                            Gauge = false;
                            Units = "";
                            Min = 0;
                            Max = 0;
                            LowerQuartile = 0;
                            UpperQuartile = 0;
                            break;
                        }
                    case "flowRate":
                        {
                            Name = "Reservoir Flow Rate";
                            Gauge = true;
                            Units = "cm/s";
                            Min = -10;
                            Max = 10;
                            LowerQuartile = -10;
                            UpperQuartile = 10;
                            break;
                        }
                    case "HeaterCurrent":
                        {
                            Name = "Heater Current";
                            Gauge = true;
                            Units = "A";
                            Min = 0;
                            Max = 20;
                            LowerQuartile = 0;
                            UpperQuartile = 20;
                            break;
                        }
                }
            }

            public float ReadValue
            {
                set
                {
                    if (readValue != value)
                    {
                        readValue = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("ReadValue"));
                        }
                    }
                }
                get
                {
                    return readValue;
                }
            }

            public string StringValue
            {
                set
                {
                    if (stringValue != value)
                    {
                        stringValue = value;
                        if (PropertyChanged != null)
                        {
                            PropertyChanged(this, new PropertyChangedEventArgs("StringValue"));
                        }
                    }
                }
                get
                {
                    return stringValue;
                }
            }

        }


        public bool IndicatorRunning
        {
            set
            {
                if (indicatorRunning != value)
                {
                    indicatorRunning = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("IndicatorRunning"));
                    }
                }
            }
            get
            {
                return indicatorRunning;
            }
        }

    }


}
