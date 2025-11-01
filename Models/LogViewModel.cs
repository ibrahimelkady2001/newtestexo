
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Windows.Input;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using static EXOApp.LogPage;


namespace EXOApp.Models
{
    /// <summary>
    /// View Model for the logging screen
    /// </summary>
    class LogViewModel : INotifyPropertyChanged
    {
        List<PodLogData> podLogs = new List<PodLogData>();
        List<PodModel> orbits;
        public event PropertyChangedEventHandler PropertyChanged;

        public Action<List<List<ChartDataPoint>>, List<ChartDataPoint>, List<string>> handleChartEvent;
        public ICommand GenerateGraphCommand { protected set; get; }
        public ICommand exitCommand { protected set; get; }




        ObservableCollection<string> podList = new ObservableCollection<string>();
        public ObservableCollection<string> PodList { get { return podList; } }

        ObservableCollection<string> parameterList = new ObservableCollection<string>();
        public ObservableCollection<string> ParameterList { get { return parameterList; } }

        public DateTime FirstDate { get; set; }
        public DateTime LastDate { get; set; }

        public DateTime chosenFirstDate;
        public DateTime chosenLastDate;

        string selectedParameter;


        private readonly Helpers.MessageInterface _messageInterface;

        bool include660 = false;

        bool controlAvailable;

        string label0;
        string label1;
        string label2;
        string label3;


        /// <summary>
        /// Constructor for the Log view screen. Sets up all the commands for buttons and organises dates drawn from the Pod database
        /// </summary>
        public LogViewModel()
        {
            GenerateGraphCommand = new Command(async () => await generateGraph());
            this._messageInterface = DependencyService.Get<Helpers.MessageInterface>();
            exitCommand = new Command(async () => await onExit());
            ControlAvailable = true;
            doLists();
        }
        /// <summary>
        /// Returns to the dashboard
        /// </summary>
        /// <returns></returns>
        public async Task onExit()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }
        /// <summary>
        /// Gets Data from all Pods. Works out the min and maximimum date ranges. Adds parameters to the sorting list
        /// </summary>
        void doLists()
        {
            orbits = Globals.getList();
            DateTime firstDate = DateTime.MaxValue;
            DateTime lastDate = DateTime.MinValue;
            DateTime currentFirst;
            DateTime currentLast;
            
            foreach(PodModel orb in orbits)
            {
                PodList.Add("EXO-" + orb.podNumber);
                currentFirst = orb.getFirstLogDate();
                currentLast = orb.getLastLogDate();
                if(currentFirst < firstDate)
                {
                    firstDate = currentFirst;
                }
                if(currentLast > lastDate)
                {
                    lastDate = currentLast;
                }
            }

            FirstDate = firstDate;
            LastDate = lastDate;
            ChosenFirstDate = FirstDate;
            ChosenLastDate = LastDate;

            ParameterList.Add("Floating Time");
            ParameterList.Add("Float Session Length");
            ParameterList.Add("Lighting Profile");
            ParameterList.Add("Starting Music");
            ParameterList.Add("Ending Music");
            ParameterList.Add("All Music");
            SelectedParameter = ParameterList[0];
        }
        /// <summary>
        /// Generates a graph based on logged data and chosen parameter
        /// </summary>
        /// <returns></returns>
        async Task generateGraph()
        {
            ControlAvailable = false;
            List<List<ChartDataPoint>> lineData = null;
            List<ChartDataPoint> piData = null;
            List<string> seriesName = null; 
            if (ChosenFirstDate > ChosenLastDate)
            {
                _ = _messageInterface.ShowAsyncOK("Log Error", "Please ensure you have chosen a valid date range", "Close");
                return;
            }
            await Task.Run(() =>
            {

                getRawData(ChosenFirstDate, ChosenLastDate);

                switch (selectedParameter)
                {

                    case "Floating Time":
                        {
                            getFloatTimeChart(ChosenFirstDate, ChosenLastDate, out lineData, out piData, out seriesName);
                            break;
                        }
                    case "Float Session Length":
                        {
                            getFloatSessionLengthChart(ChosenFirstDate, ChosenLastDate, out lineData, out piData, out seriesName);
                            break;
                        }
                    case "Lighting Profile":
                        {
                            getLightingChoiceChart(ChosenFirstDate, ChosenLastDate, out lineData, out piData, out seriesName);
                            break;
                        }
                    case "Starting Music":
                        {
                            getStartMusicChart(ChosenFirstDate, ChosenLastDate, out lineData, out piData, out seriesName);
                            break;
                        }
                    case "Ending Music":
                        {
                            getEndMusicChart(ChosenFirstDate, ChosenLastDate, out lineData, out piData, out seriesName);
                            break;
                        }
                    case "All Music":
                        {
                            getAllMusicChart(ChosenFirstDate, ChosenLastDate, out lineData, out piData, out seriesName);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
                nameFormatter(ref piData, ref seriesName);

            });
            labelWriter(lineData, piData, seriesName);
            handleChartEvent.Invoke(lineData, piData, seriesName);
            ControlAvailable = true;
        }
        /// <summary>
        /// Pull raw data from the Pod based on a given time period
        /// </summary>
        /// <param name="start">The start of the time period</param>
        /// <param name="end">The end of the given time period</param>
        /// <returns></returns>
        bool getRawData(DateTime start, DateTime end)
        {
            podLogs.Clear();
            bool anySuccess = false;
            foreach (PodModel orb in orbits)
            {
                List<PodLogDataRaw> data = null;
                string sqlData = orb.getLogs(start, end);
                if (sqlData != null)
                {
                    try
                    {
                        data = JsonConvert.DeserializeObject<List<PodLogDataRaw>>(sqlData);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                }
                if (data != null)
                {
                    anySuccess = true;
                    foreach (PodLogDataRaw dataRaw in data)
                    {
                        if(dataRaw.Page == "start" || dataRaw.Page == "App Dashboard")
                        {
                            podLogs.Add(new PodLogData(dataRaw, orb.podNumber));
                        }
                    }
                }
            }
            return anySuccess;
        }
        /// <summary>
        /// Generates a graph showing float times
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="lineData"></param>
        /// <param name="piData"></param>
        /// <param name="seriesNames"></param>
        void getFloatTimeChart(DateTime start, DateTime end, out List<List<ChartDataPoint>> lineData, out List<ChartDataPoint> piData, out List<string> seriesNames)
        {
            lineData = new List<List<ChartDataPoint>>();
            piData = new List<ChartDataPoint>();
            seriesNames = new List<string>();
            foreach (PodModel orb in orbits)
            {
                piData.Add(prepareOrbitList(orb.podNumber));
                lineData.Add(prepareDateLists(start, end));
                seriesNames.Add(orb.podNumber.ToString());
            }
            foreach (PodLogData pd in podLogs)
            {
                int index = orbits.FindIndex(x => x.podNumber == pd.OrbitNumber);
                if (index == -1)
                {
                    continue;
                }
                if(!Include660 && pd.FloatTimeChoice == "660_secs")
                {
                    continue;
                }
                //Line Graph
                int logIndex = lineData[index].FindIndex(x => (DateTime)x.XValue == pd.Time);
                if (logIndex == -1)
                {
                    continue;
                }
                lineData[index][logIndex].YValue += floatTimeParser(pd.FloatTimeChoice);
                //Pi Graph
                piData[index].YValue += floatTimeParser(pd.FloatTimeChoice);
            }
        }
        /// <summary>
        /// Generates a graph using sessions lengths
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="lineData"></param>
        /// <param name="piData"></param>
        /// <param name="seriesNames"></param>
        void getFloatSessionLengthChart(DateTime start, DateTime end, out List<List<ChartDataPoint>> lineData, out List<ChartDataPoint> piData, out List<string> seriesNames)
        {
            lineData = new List<List<ChartDataPoint>>();
            piData = new List<ChartDataPoint>();
            seriesNames = new List<string>();

            List<string> sessionList = new List<string>();
            if(Include660)
            {
                sessionList.Add("660_secs");
            }
            sessionList.Add("30_mins");
            sessionList.Add("45_mins");
            sessionList.Add("60_mins");
            sessionList.Add("75_min");
            sessionList.Add("90_mins");
            sessionList.Add("120_mins");
            sessionList.Add("150_mins");
            sessionList.Add("180_mins");

            foreach (string str in sessionList)
            {
                lineData.Add(prepareDateLists(start, end));
                piData.Add(prepareOrbitList(str));
                seriesNames.Add(str);
            }
            foreach (PodLogData pd in podLogs)
            {
                int index = sessionList.FindIndex(x => x == pd.FloatTimeChoice);
                if (index == -1)
                {
                    continue;
                }
                if (!Include660 && pd.FloatTimeChoice == "660_secs")
                {
                    continue;
                }


                //Line Graph
                int logIndex = lineData[index].FindIndex(x => (DateTime)x.XValue == pd.Time);
                if(logIndex == -1)
                {
                    continue;
                }
                lineData[index][logIndex].YValue += 1;
                //Pi Graph
                piData[index].YValue += 1;
            }
        }
        /// <summary>
        /// Generates a graph showing lighting choices
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="lineData"></param>
        /// <param name="piData"></param>
        /// <param name="seriesNames"></param>
        void getLightingChoiceChart(DateTime start, DateTime end, out List<List<ChartDataPoint>> lineData, out List<ChartDataPoint> piData, out List<string> seriesNames)
        {
            lineData = new List<List<ChartDataPoint>>();
            piData = new List<ChartDataPoint>();
            seriesNames = new List<string>();

            List<string> lightList = new List<string>();
            foreach (PodModel orb in orbits)
            {
                foreach (string mt in orb.lightingProfiles)
                {
                    if (lightList.FindIndex(x => x == mt) == -1)
                    {
                        lineData.Add(prepareDateLists(start, end));
                        piData.Add(prepareOrbitList(mt));
                        seriesNames.Add(mt);
                        lightList.Add(mt);
                    }
                }
            }
            foreach (PodLogData pd in podLogs)
            {
                int index = lightList.FindIndex(x => x == pd.LightingChoice);
                //Line Graph
                if (index == -1)
                {
                    continue;
                }
                if (!Include660 && pd.FloatTimeChoice == "660_secs")
                {
                    continue;
                }
                int logIndex = lineData[index].FindIndex(x => (DateTime)x.XValue == pd.Time);
                if (logIndex == -1)
                {
                    continue;
                }
                lineData[index][logIndex].YValue += 1;
                //Pi Graph
                piData[index].YValue += 1;
            }
        }
        /// <summary>
        /// Unused
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="lineData"></param>
        /// <param name="piData"></param>
        void getMusicChoiceChart(DateTime start, DateTime end, out List<List<ChartDataPoint>> lineData, out List<ChartDataPoint> piData)
        {
            lineData = new List<List<ChartDataPoint>>();
            piData = new List<ChartDataPoint>();
        }
        /// <summary>
        /// Generates a graph showing starting music
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="lineData"></param>
        /// <param name="piData"></param>
        /// <param name="seriesNames"></param>
        void getStartMusicChart(DateTime start, DateTime end, out List<List<ChartDataPoint>> lineData, out List<ChartDataPoint> piData, out List<string> seriesNames)
        {
            lineData = new List<List<ChartDataPoint>>();
            piData = new List<ChartDataPoint>();
            seriesNames = new List<String>();

            List<string> musicList = new List<string>();
            foreach (PodModel orb in orbits)
            {
                foreach (MusicTrack mt in orb.musicTracks)
                {
                    if (musicList.FindIndex(x => x == mt.name) == -1)
                    {
                        lineData.Add(prepareDateLists(start, end));
                        piData.Add(prepareOrbitList(mt.name));
                        seriesNames.Add(mt.name);
                        musicList.Add(mt.name);
                    }
                }
            }
            foreach (PodLogData pd in podLogs)
            {
                int index = musicList.FindIndex(x => x == pd.StartMusic);
                if (index == -1)
                {
                    continue;
                }
                if (!Include660 && pd.FloatTimeChoice == "660_secs")
                {
                    continue;
                }
                //Line Graph

                int logIndex = lineData[index].FindIndex(x => (DateTime)x.XValue == pd.Time);
                if (logIndex == -1)
                {
                    continue;
                }
                lineData[index][logIndex].YValue += 1;
                //Pi Graph
                piData[index].YValue += 1;
            }
        }
        /// <summary>
        /// Generates a graph showing ending music
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="lineData"></param>
        /// <param name="piData"></param>
        /// <param name="seriesNames"></param>
        void getEndMusicChart(DateTime start, DateTime end, out List<List<ChartDataPoint>> lineData, out List<ChartDataPoint> piData, out List<string> seriesNames)
        {
            lineData = new List<List<ChartDataPoint>>();
            piData = new List<ChartDataPoint>();
            seriesNames = new List<string>();

            List<string> musicList = new List<string>();
            foreach (PodModel orb in orbits)
            {
                foreach (MusicTrack mt in orb.musicTracks)
                {
                    if (musicList.FindIndex(x => x == mt.name) == -1)
                    {
                        lineData.Add(prepareDateLists(start, end));
                        piData.Add(prepareOrbitList(mt.name));
                        seriesNames.Add(mt.name);
                        musicList.Add(mt.name);
                    }
                }
            }
            foreach (PodLogData pd in podLogs)
            {
                int index = musicList.FindIndex(x => x == pd.EndMusic);
                if (index == -1)
                {
                    continue;
                }
                if (!Include660 && pd.FloatTimeChoice == "660_secs")
                {
                    continue;
                }
                //Line Graph
                int logIndex = lineData[index].FindIndex(x => (DateTime)x.XValue == pd.Time);
                if (logIndex == -1)
                {
                    continue;
                }
                lineData[index][logIndex].YValue += 1;
                //Pi Graph
                piData[index].YValue += 1;
            }
        }
        /// <summary>
        /// Generates a graph using start and end music
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="lineData"></param>
        /// <param name="piData"></param>
        /// <param name="seriesNames"></param>
        void getAllMusicChart(DateTime start, DateTime end, out List<List<ChartDataPoint>> lineData, out List<ChartDataPoint> piData, out List<string> seriesNames)
        {
            lineData = new List<List<ChartDataPoint>>();
            piData = new List<ChartDataPoint>();
            seriesNames = new List<string>();

            List<string> musicList = new List<string>();
            foreach (PodModel orb in orbits)
            {
                foreach (MusicTrack mt in orb.musicTracks)
                {
                    if (musicList.FindIndex(x => x == mt.name) == -1)
                    {
                        lineData.Add(prepareDateLists(start, end));
                        piData.Add(prepareOrbitList(mt.name));
                        seriesNames.Add(mt.name);

                        musicList.Add(mt.name);
                    }
                }
            }
            foreach (PodLogData pd in podLogs)
            {
                int index = musicList.FindIndex(x => x == pd.StartMusic); 
                if (index == -1)
                {
                    continue;
                }
                if (!Include660 && pd.FloatTimeChoice == "660_secs")
                {
                    continue;
                }
                //Line Graph
                int logIndex = lineData[index].FindIndex(x => (DateTime)x.XValue == pd.Time);
                if (logIndex == -1)
                {
                    continue;
                }
                lineData[index][logIndex].YValue += 1;
                //Pi Graph
                piData[index].YValue += 1;

                index = musicList.FindIndex(x => x == pd.EndMusic);
                if (index == -1)
                {
                    continue;
                }
                //Line Graph
                logIndex = lineData[index].FindIndex(x => (DateTime)x.XValue == pd.Time);
                if (logIndex == -1)
                {
                    continue;
                }
                lineData[index][logIndex].YValue += 1;
                //Pi Graph
                piData[index].YValue += 1;
            }
        }
        /// <summary>
        /// Generates placeholder points to for the line chart so values return to 0 by the next parameter (if needed) instead of the next point in following parameter
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        List<ChartDataPoint> prepareDateLists(DateTime start, DateTime end)
        {
            DateTime currentDate = start;
            List<ChartDataPoint> list = new List<ChartDataPoint>();
            while(currentDate < end)
            {
                list.Add(new ChartDataPoint(currentDate.Date, 0));
                currentDate = currentDate.AddDays(1);
            }
            return list;
        }
        /// <summary>
        /// Creates a datapoint for a given Pod
        /// </summary>
        /// <param name="OrbitNumber"></param>
        /// <returns></returns>
        ChartDataPoint prepareOrbitList(int OrbitNumber)
        {
            return new ChartDataPoint(OrbitNumber, 0);
        }
        /// <summary>
        /// Prepares a datapoint for a given category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        ChartDataPoint prepareOrbitList(string category)
        {
            return new ChartDataPoint(category, 0);
        }

        /// <summary>
        /// Converts text into int
        /// </summary>
        /// <param name="floatTime"></param>
        /// <returns></returns>
        int floatTimeParser(string floatTime)
        {
            if (floatTime.Contains("secs"))
            {
                return 11;
            }
            floatTime = floatTime.Replace("_mins", "");
            floatTime = floatTime.Replace("_min", "");

            int time;
            if (int.TryParse(floatTime, out time))
            {
                return time;
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// Formats parameter names into a readable form
        /// </summary>
        /// <param name="piData"></param>
        /// <param name="seriesNames"></param>
        void nameFormatter(ref List<ChartDataPoint> piData, ref List<string> seriesNames)
        {
            switch (selectedParameter)
            {

                case "Floating Time":
                    {
                        foreach(ChartDataPoint point in piData)
                        {
                            point.XValue = "EXO-" + point.XValue;
                        }
                        for(int i = 0; i < seriesNames.Count; i++)
                        {
                            seriesNames[i] = "EXO-" + seriesNames[i];
                        }
                        break;
                    }
                case "Float Session Length":
                    {
                        foreach (ChartDataPoint point in piData)
                        {
                            point.XValue = floatSessionNameFormatter((string) point.XValue);
                        }
                        for (int i = 0; i < seriesNames.Count; i++)
                        {
                            seriesNames[i] = floatSessionNameFormatter(seriesNames[i]);
                        }
                        break;
                    }
                case "Lighting Profile":
                    {
                        break;
                    }
                case "Starting Music":
                    {
                        foreach (ChartDataPoint point in piData)
                        {
                            point.XValue = Converters.MusicNameFormatter.convertMusicName((string) point.XValue);
                        }
                        for (int i = 0; i < seriesNames.Count; i++)
                        {
                            seriesNames[i] = Converters.MusicNameFormatter.convertMusicName(seriesNames[i]);
                        }
                        break;
                    }
                case "Ending Music":
                    {
                        foreach (ChartDataPoint point in piData)
                        {
                            point.XValue = Converters.MusicNameFormatter.convertMusicName((string)point.XValue);
                        }
                        for (int i = 0; i < seriesNames.Count; i++)
                        {
                            seriesNames[i] = Converters.MusicNameFormatter.convertMusicName(seriesNames[i]);
                        }
                        break;
                    }
                case "All Music":
                    {
                        foreach (ChartDataPoint point in piData)
                        {
                            point.XValue = Converters.MusicNameFormatter.convertMusicName((string)point.XValue);
                        }
                        for (int i = 0; i < seriesNames.Count; i++)
                        {
                            seriesNames[i] = Converters.MusicNameFormatter.convertMusicName(seriesNames[i]);
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

        }
        /// <summary>
        /// Generates labels with key information
        /// </summary>
        /// <param name="lineData"></param>
        /// <param name="piData"></param>
        /// <param name="seriesNames"></param>
        void labelWriter(in List<List<ChartDataPoint>> lineData, in List<ChartDataPoint> piData, in List<string> seriesNames)
        {
            switch (selectedParameter)
            {

                case "Floating Time":
                    {
                        int index = 0;
                        int currentMax = 0;
                        int counter = 0;
                        int total = 0;
                        foreach(List<ChartDataPoint> point in lineData)
                        {
                            int temp = addUpList(point);
                            total += temp;
                            if (temp > currentMax)
                            {
                                currentMax = temp;
                                index = counter;
                            }
                            counter++;
                        }
                        TimeSpan elapsedTime = ChosenLastDate - ChosenFirstDate;
                        Label0 = "Total Float Time: " + total +" Minutes";
                        Label1 = "Most Used EXO: " + seriesNames[index];
                        Label2 = "Average Float Time Per Day: " + ((float)total / elapsedTime.TotalDays) + " Minutes";
                        break;
                    }
                case "Float Session Length":
                    {
                        int index = 0;
                        int currentMax = 0;
                        int counter = 0;
                        int total = 0;
                        foreach (List<ChartDataPoint> point in lineData)
                        {
                            int temp = addUpList(point);
                            total += temp;
                            if (temp > currentMax)
                            {
                                currentMax = temp;
                                index = counter;
                            }
                            counter++;
                        }
                        TimeSpan elapsedTime = ChosenLastDate - ChosenFirstDate;
                        Label0 = "Number of Floats: " + total + " Floats";
                        Label1 = "Most Common Float Session: " + seriesNames[index];
                        Label2 = "";
                        break;
                    }
                case "Lighting Profile":
                    {
                        int index = 0;
                        int currentMax = 0;
                        int counter = 0;
                        int total = 0;
                        foreach (List<ChartDataPoint> point in lineData)
                        {
                            int temp = addUpList(point);
                            total += temp;
                            if (temp > currentMax)
                            {
                                currentMax = temp;
                                index = counter;
                            }
                            counter++;
                        }
                        TimeSpan elapsedTime = ChosenLastDate - ChosenFirstDate;
                        Label0 = "Most Popular Lighting Profile: " + seriesNames[index];
                        Label1 = "";
                        Label2 = "";
                        break;
                    }
                case "Starting Music":
                    {
                        int index = 0;
                        int currentMax = 0;
                        int counter = 0;
                        int total = 0;
                        foreach (List<ChartDataPoint> point in lineData)
                        {
                            int temp = addUpList(point);
                            total += temp;
                            if (temp > currentMax)
                            {
                                currentMax = temp;
                                index = counter;
                            }
                            counter++;
                        }
                        TimeSpan elapsedTime = ChosenLastDate - ChosenFirstDate;
                        Label0 = "Most Popular Starting Track: " + seriesNames[index];
                        Label1 = "";
                        Label2 = "";
                        break;
                    }
                case "Ending Music":
                    {
                        int index = 0;
                        int currentMax = 0;
                        int counter = 0;
                        int total = 0;
                        foreach (List<ChartDataPoint> point in lineData)
                        {
                            int temp = addUpList(point);
                            total += temp;
                            if (temp > currentMax)
                            {
                                currentMax = temp;
                                index = counter;
                            }
                            counter++;
                        }
                        TimeSpan elapsedTime = ChosenLastDate - ChosenFirstDate;
                        Label0 = "Most Popular Ending Track: " + seriesNames[index];
                        Label1 = "";
                        Label2 = "";
                        break;
                    }
                case "All Music":
                    {
                        int index = 0;
                        int currentMax = 0;
                        int counter = 0;
                        int total = 0;
                        foreach (List<ChartDataPoint> point in lineData)
                        {
                            int temp = addUpList(point);
                            total += temp;
                            if (temp > currentMax)
                            {
                                currentMax = temp;
                                index = counter;
                            }
                            counter++;
                        }
                        TimeSpan elapsedTime = ChosenLastDate - ChosenFirstDate;
                        Label0 = "Most Popular Music Track: " + seriesNames[index];
                        Label1 = "";
                        Label2 = "";
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
        /// <summary>
        /// Renames float sessions into a readable format
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        string floatSessionNameFormatter(string input)
        {
            if(input.Contains("secs"))
            {
                return "660 Seconds";
            }
            input = input.Replace("_mins", "");
            input = input.Replace("_min", "");
            input = input + " Minutes";
            return input;
        }

        /// <summary>
        /// Adds up the total data of a list
        /// </summary>
        /// <param name="cd"></param>
        /// <returns></returns>
        int addUpList(List<ChartDataPoint> cd)
        {
            int sum = 0;
            foreach(ChartDataPoint chartData in cd)
            {
                sum += (int)(chartData.YValue);
            }
            return sum;
        }

        /// <summary>
        /// Class holding the data points for a given float
        /// </summary>
        internal class PodLogData
        {
            public string Page { get; set; }
            public DateTime Time { get; set; }
            public string FloatTimeChoice { get; set; }
            public string LightingChoice { get; set; }
            public string MusicChoice { get; set; }
            public string StartMusic { get; set; }
            public string EndMusic { get; set; }
            public string FillDelayChoice { get; set; }
            public int OrbitNumber { get; set; }
            /// <summary>
            /// Stores all the parameters for a log entry into strings for later use
            /// </summary>
            /// <param name="rawData"></param>
            /// <param name="orbitNumber"></param>
            public PodLogData(PodLogDataRaw rawData, int orbitNumber)
            {
                this.Page = rawData.Page;
                Time = DateTime.Parse(rawData.Time).Date;
                this.FloatTimeChoice = rawData.FloatTimeChoice;
                this.LightingChoice = rawData.LightingChoice;
                this.MusicChoice = rawData.MusicChoice;
                this.StartMusic = rawData.StartMusic;
                this.EndMusic = rawData.EndMusic;
                this.FillDelayChoice = rawData.FillDelayChoice;
                this.OrbitNumber = orbitNumber;
            }
        }
        /// <summary>
        /// Used to store converted raw data from the SQL entry
        /// </summary>
        internal class PodLogDataRaw
        {
            public string Page { get; set; }
            public string Time { get; set; }
            public string FloatTimeChoice { get; set; }
            public string LightingChoice { get; set; }
            public string MusicChoice { get; set; }
            public string StartMusic { get; set; }
            public string EndMusic { get; set; }
            public string FillDelayChoice { get; set; }
        }
        /// <summary>
        /// Unused
        /// </summary>
        internal class ChartDataLine
        {
            public DateTime date { get; set; }
            public int data { get; set; }
            /// <summary>
            /// Unused
            /// </summary>
            /// <param name="date"></param>
            /// <param name="data"></param>
            public ChartDataLine(DateTime date, int data)
            {
                this.date = date;
                this.data = data;
            }
        }




        public DateTime ChosenFirstDate
        {
            set
            {
                if (chosenFirstDate != value)
                {
                    chosenFirstDate = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("ChosenFirstDate"));
                    }
                }
            }
            get
            {
                return chosenFirstDate;
            }
        }

        public DateTime ChosenLastDate
        {
            set
            {
                if (chosenLastDate != value)
                {
                    chosenLastDate = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("ChosenLastDate"));
                    }
                }
            }
            get
            {
                return chosenLastDate;
            }
        }

        public string SelectedParameter
        {
            set
            {
                if (selectedParameter != value)
                {
                    selectedParameter = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SelectedParameter"));
                    }
                }
            }
            get
            {
                return selectedParameter;
            }
        }

        public bool Include660
        {
            set
            {
                if (include660 != value)
                {
                    include660 = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Include660"));
                    }
                }
            }
            get
            {
                return include660;
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

        public string Label0
        {
            set
            {
                if (label0 != value)
                {
                    label0 = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Label0"));
                    }
                }
            }
            get
            {
                return label0;
            }
        }
        public string Label1
        {
            set
            {
                if (label1 != value)
                {
                    label1 = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Label1"));
                    }
                }
            }
            get
            {
                return label1;
            }
        }
        public string Label2
        {
            set
            {
                if (label2 != value)
                {
                    label2 = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Label2"));
                    }
                }
            }
            get
            {
                return label2;
            }
        }
        public string Label3
        {
            set
            {
                if (label3 != value)
                {
                    label3 = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Label3"));
                    }
                }
            }
            get
            {
                return label3;
            }
        }
    }
}
