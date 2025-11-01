using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static EXOApp.Models.SettingsViewModel;
using EXOApp.GlobalsClasses;

using System.Globalization;
using static EXOApp.Models.PlusViewModel;

namespace EXOApp.Models
{
    /// <summary>
    /// Model for the Pod.
    /// </summary>
    class PodModel : IComparable<PodModel>
    {
        public string ipAddress { get; set; }
        public string version { get; set; }
        public int podNumber { get; set; }
        public List<DataPoint> status { get; set; }
        public List<DataPoint> settings { get; set; }
        public List<FloatTime> floatTimes { get; set; }
        public List<FillDelay> fillDelays { get; set; }
        public List<string> lightingProfiles { get; set; }
        public List<MusicTrack> musicTracks { get; set; }
        public List<string> musicProfiles { get; set; }
        public List<string> softwareVersion { get; set; }
        public DateTime filter { get; set; }
        public DateTime uv { get; set; }
        private List<MusicProfileStartEnd> musicProfilesStartEnd { get; set; }
        private int floatMusicTime = 0;


        /// <summary>
        ///  Contructor for the Pod Model. Do not run SQL commands in this statement otherwise the program is likely to crash on a fail.
        /// </summary>
        public PodModel(string ip, int orbitNumber)
        {
            //DO NOT RUN SQL QUERIES HERE
            ipAddress = ip;
            this.podNumber = orbitNumber;
            lightingProfiles = new List<string>();
            musicProfiles = new List<string>();
            musicProfilesStartEnd = new List<MusicProfileStartEnd>(); 
            softwareVersion = new List<string>();

        }

        /// <summary>
        /// Internal class for serialising each data point pulled from the Pod config and Pod Status
        /// </summary>
        internal class DataPoint
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        /// <summary>
        /// Internal class for serialising each data point pulled from the float time
        /// </summary>
        internal class FloatTime
        {
            public string name { get; set; }
            public string display_name { get; set; }
            public string time { get; set; }
        }

        /// <summary>
        /// Internal class for serialising each data point pulled from the fill delay
        /// </summary>
        internal class FillDelay
        {
            public string name { get; set; }
            public string display_name { get; set; }
            public string time { get; set; }
        }
        /// <summary>
        /// Internal class for serialising each data point pulled from the music profiles
        /// </summary>
        internal class MusicProfiles
        {
            public string music_options_name { get; set; }
        }
        /// <summary>
        /// Internal class for serialising each data point pulled from the lighting profiles
        /// </summary>
        internal class LightingProfile
        {
            public string lighting_options_name { get; set; }
        }

        /// <summary>
        /// Internal class for serialising each data point pulled from the versions
        /// </summary>
        internal class Version
        {
            public string name { get; set; }
            public string version { get; set; }
        }

        /// <summary>
        /// Internal class for storing the paired music profile information. Music profiles are stored in two separate SQL entries for
        /// beginning and end. This class groups them together.
        /// </summary>
        internal class MusicProfileStartEnd
        {
            public string start { get; set; }
            public string end { get; set; }

            public MusicProfileStartEnd(string start, string end)
            {
                this.start = start;
                this.end = end;
            }
        }

        /// <summary>
        /// Internal class for serialising each data point pulled from filter and UV change dates
        /// </summary>
        internal class FilterUVData
        {
            public string type { get; set; }
            public DateTime lastChange { get; set; }
        }

        /*  Ignore uneeded
        internal class Status
        {            
            float TempRes { get; set; }
            float TempPump { get; set; }
            float TempOrbit { get; set; }

            float pH { get; set; }
            float h2o_Cl { get; set; }
            float SolDen { get; set; }
            float OrbitSolPres { get; set; }
            float ResSolPres { get; set; }
            float BagFS { get; set; }

            float UVBS { get; set; } //UV Percentage
            string SysMode { get; set; }
            string filter { get; set; }
            double UV { get; set; } //UV uptime
            float Datum { get; set; }
            int Assist { get; set; }
            int H2O2Bottle { get; set; }
            int LastDoseCheckTAD { get; set; }
            int LastDoseApplyTAD { get; set; }

            string HeaterMode { get; set; }
            int PumpSpeed { get; set; }
            string InValvePos { get; set; }
            string OutValvePos { get; set; }
            string Flow { get; set; }
            string FloatSwitch { get; set; }
            int UPSActive { get; set; }
            int IOLANActive { get; set; }
            int IOConfigOK { get; set; }
            long SysModeTAD { get; set; }
            int ProgramStarted { get; set; }
            int ResumeFloat { get; set; }
            int floatState { get; set; }
            int floatCycleState { get; set; }
            int resumeTime { get; set; }
            float ResTankLevel { get; set; }
            int lastUpdateSecond { get; set; }
            

        }
    */
        /* Ignore uneeded
        internal class Settings
        {
            //Settings
            float water_temp_settings { get; set; }
            bool ui_status_line { get; set; }
            int storage_heater_run_speed { get; set; }

            int fill_run_speed_start { get; set; }
            int fill_run_speed_middle { get; set; }
            int fill_run_speed_end { get; set; }
            int fill_run_time_start { get; set; }
            int fill_run_time_middle { get; set; }
            int fill_run_time_end { get; set; }

            int empty_run_speed_start { get; set; }
            int empty_run_speed_middle { get; set; }
            int empty_run_speed_end { get; set; }
            int empty_run_time_start { get; set; }
            int empty_run_time_middle { get; set; }
            int empty_run_time_end { get; set; }

            int float_default_time_to_start { get; set; }
            int orbit_orbit_run_speed_end { get; set; }
            int filtering_run_speed { get; set; }

            int skimmer_proc1_run_speed { get; set; }
            int skimmer_proc1_run_time { get; set; }
            int skimmer_proc2_run_speed { get; set; }
            int skimmer_proc3_run_time { get; set; }
            int skimmer_proc3_run_speed { get; set; }

            float water_temp_tolerance { get; set; }

            int dosing_time_of_day { get; set; }
            int dosing_sample_time { get; set; }
            int dosing_run_time { get; set; }
            int dosing_min_level { get; set; }
            int dosing_run_speed { get; set; }

            int night_mode_enabled { get; set; }
            int night_mode_on_hour { get; set; }
            int night_mode_off_hour { get; set; }

            int blocked_pressure { get; set; }
            int negate_inverter_speed { get; set; }

            int pre_prime_enabled { get; set; }
            int pre_prime_speed { get; set; }
            int pre_prime_duration { get; set; }

            int volume_percentage { get; set; }

            float temp_pump_offset { get; set; }
            float temp_orbit_offset { get; set; }
            float temp_res_offset { get; set; }
            float sg_offset { get; set; }
            float pump_temp_cutoff { get; set; }

            int orbit_orbit_run_speed_start { get; set; }
            int orbit_orbit_run_speed_middle { get; set; }
            int orbit_orbit_run_time_start { get; set; }
            int orbit_orbit_run_time_middle { get; set; }
            int orbit_orbit_run_time_end { get; set; }

            int h2o2offset { get; set; }
            int orbit_empty_pressure_hex { get; set; }
            int orbit_full_pressure_hex { get; set; }
            int res_empty_pressure_hex { get; set; }
            int res_full_pressure_hex { get; set; }

            int default_storage_heater_run_speed { get; set; }
            int default_fill_run_time_middle { get; set; }
            int default_empty_run_time_middle { get; set; }
            int default_dosing_run_time { get; set; }
            int default_dosing_time_of_day { get; set; }
            int default_skimmer_proc1_run_time { get; set; }
            int default_skimmer_proc2_run_time { get; set; }
            float default_water_temp_tolerance { get; set; }
            float default_water_temp_setting { get; set; }

            int orbit_to_orbit_duromg_float { get; set; }
            int user_orbit_to_orbit_run_speed_start { get; set; }
            int user_orbit_to_orbit_run_speed_middle { get; set; }
            int default_volume_percentage { get; set; }
            int default_user_orbit_to_orbit_run_speed_middle { get; set; }

            int res_tank_fill_level { get; set; }
            int orbit_revision { get; set; }
            int level_sensor_enabled { get; set; }
            int ignore_res_sensors { get; set; }
            int standard_led_on { get; set; }

        }
        */

        /// <summary>
        /// Used to compare pod numbers to each other, to sort the pod list.
        /// </summary>
        public int CompareTo(PodModel orb)
        {
            if (orb == null)
            {
                return 1;
            }
            else
            {
                return this.podNumber.CompareTo(orb.podNumber);
            }
        }

        /// <summary>
        /// Requests a shutdown from the Pod
        /// </summary>
        public void shutdown()
        {
            CustomWebRequest.manualRequest(ipAddress, "sysShutdown");
        }

        /// <summary>
        /// Requests a restart from the Pod
        /// </summary>
        public void restart()
        {
            Debug.WriteLine(CustomWebRequest.manualRequest(ipAddress, "sysReboot"));
        }

        /// <summary>
        /// Updates a parameter in the config table in the sql database
        /// </summary>
        public void updateSettings()
        {
            string result = CustomWebRequest.generalSQLRequest(ipAddress, "get", "SELECT name, value FROM config");
            //Debug.WriteLine(result);
            if(result == null)
            {
                return;
            }
            settings = JsonConvert.DeserializeObject<List<DataPoint>>(result);
        }

        /// <summary>
        /// Updates a parameter in the control status table in the sql database
        /// </summary>
        public bool updateStatus()
        {
            string result = CustomWebRequest.generalSQLRequest(ipAddress, "get", "SELECT name, value FROM control_status");
            //Debug.WriteLine(result);
            if (result == null)
            {
                return false;
            }
            try
            {
                status = JsonConvert.DeserializeObject<List<DataPoint>>(result);

            }
            catch(Newtonsoft.Json.JsonReaderException)
            {
                Debug.WriteLine("Update Status Error");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets every single row from the sql config table
        /// </summary>
        public List<ConfigSettingPoint> getOrbitConfig()
        {
            List<ConfigSettingPoint> configPoints = new List<ConfigSettingPoint>();
            string result = SQLRequestRepeat("get", "SELECT name, value, units, description FROM config");
            if (result == null)
            {
                return null;
            }
            configPoints = JsonConvert.DeserializeObject<List<ConfigSettingPoint>>(result);
            return configPoints;
        }

        /// <summary>
        /// Gets the a specified parameter from the status or config table of the pod.
        /// These parameters are stored locally on the control pc, as such, they must be pulled from the pod first.
        /// </summary>
        /// <param name="variable">Variable to get from the config or status table. Must be an exact string match</param>
        /// <param name="isStatus">Specifies whether to use the status or config table. True for status, false for config</param>
        /// <returns></returns>
        public string getData(string variable, bool isStatus)
        {
            List<DataPoint> currentList;
            if (isStatus)
            {
                currentList = status;
            }
            else
            {
                currentList = settings;
            }
            string output;
            try
            {
                DataPoint dp = currentList.Find(x => x.Name == variable);
                if(dp != null)
                {
                    output = dp.Value;
                }
                else
                {
                    output = "0";
                    Debug.WriteLine(variable + " Data Not Found");
                }
            }
            catch(System.NullReferenceException)
            {
                output = "0";
                Debug.WriteLine(variable + " Data Not Found");
            }
            return output;
        }
        
        /// <summary>
        /// Gets the last date the UV was changed
        /// </summary>
        public void getUV()
        {
            string result ="";
            bool success = false;
            int count = 0;
            while (!success)
            {
                result = CustomWebRequest.manualRequest(ipAddress, "getUV");
                if(result != null)
                {
                    success = true;
                }
                count++;
                if (count > 3)
                {
                    //ADD ERROR MESSAGE
                    Debug.WriteLine("Error");
                    return;
                }
            }
            result = result.Replace("]", "");
            result = result.Replace("[", "");
            result = result.Replace("\"", "");
            uv = DateTime.Parse(result);
        }

        /// <summary>
        /// Updates the last UV change date to the current date
        /// </summary>
        public void setUV()
        {
            string result = "";
            bool success = false;
            int count = 0;
            while (!success)
            {
                result = CustomWebRequest.manualRequest(ipAddress, "setUV");
                if (result != null)
                {
                    success = true;
                }
                count++;
                if (count > 3)
                {
                    //ADD ERROR MESSAGE
                    Debug.WriteLine("Error");
                    return;
                }
            }
            getUV();
        }

        /// <summary>
        /// Gets the last date the filters were changed
        /// </summary>
        public void getFilter()
        {
            string result = "";
            bool success = false;
            int count = 0;
            while (!success)
            {
                result = CustomWebRequest.manualRequest(ipAddress, "getFilter");
                if (result != null)
                {
                    success = true;
                }
                count++;
                if (count > 3)
                {
                    //ADD ERROR MESSAGE
                    Debug.WriteLine("Error");
                    return;
                }
            }
            result = result.Replace("]", "");
            result = result.Replace("[", "");
            result = result.Replace("\"", "");
            filter = DateTime.Parse(result);
        }

        /// <summary>
        /// Updates the last filter change date to the current date
        /// </summary>
        public void setFilter()
        {
            string result = "";
            bool success = false;
            int count = 0;
            while (!success)
            {
                result = CustomWebRequest.manualRequest(ipAddress, "setFilter");
                if (result != null)
                {
                    success = true;
                }
                count++;
                if (count > 3)
                {
                    //ADD ERROR MESSAGE
                    Debug.WriteLine("Error");
                    return;
                }
            }
            getFilter();
        }

        /// <summary>
        /// Runs an SQl request on the Pod. If the request fails, run the sql request up to 3 times before giving up.
        /// Use this function to run SQL queries
        /// </summary>
        /// <param name="getset">Whether the request is "get" or "set"</param>
        /// <param name="query">The SQL query to run</param>
        /// <returns></returns>
        private string SQLRequestRepeat(string getset, string query)
        {
            string result = "";
            bool success = false;
            int count = 0;
            while (!success)
            {
                result = CustomWebRequest.generalSQLRequest(ipAddress, getset, query);
                if (result != null)
                {
                    success = true;
                    return result;
                }
                count++;
                if (count > 3)
                {
                    //ADD ERROR MESSAGE
                    Debug.WriteLine("Error");
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets a list of float times available on the Pod. The custom length is not used.
        /// </summary>
        public void getFloatTimes()
        {

            string result = SQLRequestRepeat("get", "SELECT name, display_name, time FROM float_time_options");
            floatTimes = JsonConvert.DeserializeObject<List<FloatTime>>(result);
            if (!Globals.getPremium())
            {
                floatTimes.RemoveAt(floatTimes.FindIndex(x => x.name == "Custom"));
            }
        }

        /// <summary>
        /// Gets a list of fill delays available on the Pod. The custom length is not used
        /// </summary>
        public void getFillDelay()
        {
            string result = SQLRequestRepeat("get", "SELECT name, display_name, time FROM fill_delay_options");
            fillDelays = JsonConvert.DeserializeObject<List<FillDelay>>(result);
            if (!Globals.getPremium())
            {
                fillDelays.RemoveAt(fillDelays.FindIndex(x => x.name == "Custom"));
            }
        }

        /// <summary>
        /// Gets a list of available lighting profiles from the Pod.
        /// </summary>
        public void getLightingProfiles()
        {
            string result = SQLRequestRepeat("get", "SELECT DISTINCT lighting_options_name FROM lighting_profiles");
            List<LightingProfile> lights = JsonConvert.DeserializeObject<List<LightingProfile>>(result);
            foreach (LightingProfile lt in lights)
            {
                lightingProfiles.Add(lt.lighting_options_name);
            }

        }

        /// <summary>
        /// Gets the list of music tracks from the Pod. If the table doesn't exist (older versions), falls back to a default list of tracks.
        /// </summary>
        public bool getMusicTracks()
        {
            string result = SQLRequestRepeat("get", "SELECT name, premiumLevel, defaultVolume FROM music");
            try
            {
                musicTracks = JsonConvert.DeserializeObject<List<MusicTrack>>(result);
            }
            catch(System.ArgumentNullException)
            {
                musicTracks = new List<MusicTrack>();
                musicTracks.Add(createBackupMusicTrack("df_Alpha_Waves.mp3"));
                musicTracks.Add(createBackupMusicTrack("df_Meditation.mp3"));
                musicTracks.Add(createBackupMusicTrack("df_Gentle_Stream.mp3"));
                musicTracks.Add(createBackupMusicTrack("df_Harp_Music.mp3"));
                musicTracks.Add(createBackupMusicTrack("df_Thunderstorm_with_Music.mp3"));
                musicTracks.Add(createBackupMusicTrack("df_Ocean_Waves.mp3"));
                musicTracks.Add(createBackupMusicTrack("df_Rain_and_Thunder.mp3"));
                musicTracks.Add(createBackupMusicTrack("df_Wind_Chimes.mp3"));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Used to create the default list of tracks for Pods on older versions
        /// </summary>
        MusicTrack createBackupMusicTrack(string name)
        {
            MusicTrack mt = new MusicTrack();
            mt.defaultVolume = 100;
            mt.premiumLevel = 0;
            mt.name = name;

            return mt;
        }

        /// <summary>
        /// Adds a new music track to database. Used for custom music tracks
        /// </summary>
        /// <param name="name">Name of the music tracks</param>
        /// <param name="premiumLevel">The premium level of the music track, currently used to distinguish between custom and default tracks.</param>
        /// <param name="defaultVolume">The  volume of the track used with per track music volumes</param>
        /// <returns></returns>
        public string addMusicTracks(string name, string premiumLevel, string defaultVolume)
        {
            string request = "INSERT INTO music(name, premiumLevel, defaultVolume) VALUES ('" + name + "', '"+ premiumLevel + "', '"+ defaultVolume +"')";
            return SQLRequestRepeat("set", request);
        }

        /// <summary>
        /// Deletes music tracks from the database. This will not delete tracks from the pod
        /// </summary>
        /// <param name="name">Name of the music track in the database to delete.</param>
        /// <returns></returns>
        public string deleteMusicTracks(string name)
        {
            string request = "DELETE FROM music WHERE name='"+ name +"'";
            return SQLRequestRepeat("set", request);
        }

        /// <summary>
        /// Changes the per track music volume
        /// </summary>
        public string changeTrackVolume(int volume, string name)
        {
            string request = "UPDATE music" +
                " SET defaultVolume =" + volume + 
                " WHERE name = '" + name +"'";
            return SQLRequestRepeat("set", request);
        }

        /// <summary>
        /// Gets the version information for the Pod
        /// </summary>
        public void getVersion()
        {
            string result = SQLRequestRepeat("get", "SELECT name, version FROM version");
            List<Version> versions = JsonConvert.DeserializeObject<List<Version>>(result);
            softwareVersion.Add(versions[0].version);
            softwareVersion.Add(versions[1].version);
            if (versions.Count > 2)
            {
                version = versions[0].version + "-" + versions[1].version + "-" + versions[2].version;
                softwareVersion.Add(versions[2].version);

            }
            else
            {
                version = versions[0].version + "-" + versions[1].version + "-0";
                softwareVersion.Add("0");
            }
        }

        /// <summary>
        /// Gets the premium level. This function is not used and is currently a copy paste of get version
        /// </summary>
        public void getPremium()
        {
            string result = SQLRequestRepeat("get", "SELECT name, version FROM version");
            List<Version> versions = JsonConvert.DeserializeObject<List<Version>>(result);
            softwareVersion.Add(versions[0].version);
            softwareVersion.Add(versions[1].version);
            if (versions.Count > 2)
            {
                version = versions[0].version + "-" + versions[1].version + "-" + versions[2].version;
                softwareVersion.Add(versions[2].version);

            }
            else
            {
                version = versions[0].version + "-" + versions[1].version + "-0";
                softwareVersion.Add("0");
            }
        }

        /// <summary>
        /// Gets the music profiles from the pod. Custom music length is not used
        /// </summary>
        public void getMusicProfiles()
        {
            string result = CustomWebRequest.generalSQLRequest(ipAddress, "get", "SELECT DISTINCT music_options_name FROM music_profiles");
            List<MusicProfiles> music = JsonConvert.DeserializeObject<List<MusicProfiles>>(result);
            foreach (MusicProfiles ms in music)
            {
                musicProfiles.Add(ms.music_options_name);
                if(ms.music_options_name == "Custom")
                {
                    continue;
                }
                string[] startend = ms.music_options_name.Split('/');
                musicProfilesStartEnd.Add(new MusicProfileStartEnd(startend[0], startend[1]));
            }
        }

        /// <summary>
        /// Gets a the music profile start times. This is no longer used.
        /// </summary>
        public List<string> getMusicProfileListStart(string floatTime, int floatTimeSlider = 0)
        {
            floatMusicTime = 0;
            List<string> startList = new List<string>();
            if(floatTime != "Custom")
            {
                FloatTime ft = floatTimes.Find(x => x.display_name == floatTime);
                floatMusicTime = Int32.Parse(ft.time);
                Debug.WriteLine(floatMusicTime);
            }
            else
            {
                floatMusicTime = floatTimeSlider * 60;
            }
            startList.Add("00");
            if (floatMusicTime >= 300)
            {
                startList.Add("05");
            }
            if(floatMusicTime >= 900)
            {
                startList.Add("10");
            }
            if (floatMusicTime >= 1800)
            {
                startList.Add("15");
            }
            if (floatMusicTime >= 3600)
            {
                startList.Add("55");
            }
            startList.Add("Continuous");
            startList.Add("Custom");
            return startList;
        }

        /// <summary>
        /// Gets a the music profile end times. This is no longer used.
        /// </summary>
        public List<string> getMusicProfileListEnd(string startLength, int floatTimeSlider = 0)
        {
            Debug.WriteLine("getMusicProfileListEnd");
            int time = 0;
            List<string> endList = new List<string>();
            if (startLength != "Custom" && startLength != "Continuous")
            {
                time = Int32.Parse(startLength) * 60;
            }
            else
            {
                time = floatTimeSlider * 60;
            }
            time = floatMusicTime - time;
            Debug.WriteLine(startLength);
            Debug.WriteLine(time);
            switch(startLength)
            {
                case "00":
                    if(time >= 300)
                    {
                        endList.Add("05");
                    }
                    if(time >= 600)
                    {
                        endList.Add("10");
                    }
                    break;
                case "05":
                    if(time >= 300)
                    {
                        endList.Add("05");
                    }
                    if(time >=600)
                    {
                        endList.Add("10");
                    }
                    if (time >= 900)
                    {
                        endList.Add("15");
                    }
                    break;
                case "10":
                    if (time >= 300)
                    {
                        endList.Add("05");
                    }
                    break;
                case "15":
                    if (time >= 900)
                    {
                        endList.Add("15");
                    }
                    break;
                case "55":
                    if (time >= 300)
                    {
                        endList.Add("05");
                    }
                    break;
                case "Continuous":
                    endList.Add("05");
                    break;
                default:
                    break;
            }
            Debug.WriteLine(endList.Count);
            return endList;
        }

        /// <summary>
        ///  Changes sessions settings. This function is not used.
        /// </summary>
        public bool changeSetting(string setting, string parameter)
        {
            bool success = false;
            int count = 0;
            while (!success)
            {
                success = string.Equals(CustomWebRequest.generalSQLRequest(ipAddress, "set", "UPDATE session_settings SET "+ setting +" '" + parameter + "' WHERE id = 1"), "\"success\"");
                count++;
                if (count > 3)
                {
                    //ADD ERROR MESSAGE
                    Debug.WriteLine("Error");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Sets the specified instruction in the Pod control table to 1. This starts the associated functions
        /// </summary>
        /// <param name="instruction">The mode to start.</param>
        /// <returns></returns>
        public bool startOrbit(string instruction)
        {
            bool success = false;
            int count = 0;
            while (!success)
            {
                success = string.Equals(CustomWebRequest.generalSQLRequest(ipAddress, "set", "UPDATE control SET request = 1 WHERE name = '"+instruction+"'"), "\"success\"");
                count++;
                if (count > 3)
                {
                    //ADD ERROR MESSAGE
                    Debug.WriteLine("Error");
                    return false;
                }
            }
            return true;
        
        }

        /// <summary>
        /// Saves the session settings to the Pod for the next float. Normally just before starting a float. 
        /// This is used for pods on with per track music volumes available (does not have to be enabled).
        /// </summary>
        public bool changeFloatCurrentSettings(string floatTime, string lighting, string musicProfile, string startMusic, string endMusic, string fillDelay, int startVol, int endVol)
        {
            bool success = false;
            int count = 0;
            while (!success)
            {
                success = string.Equals(CustomWebRequest.generalSQLRequest(ipAddress, "set", "UPDATE session_settings SET " +
                    "float_time_options_name = '" + floatTime + 
                    "', lighting_options_name = '" + lighting + 
                    "', music_options_name = '" + musicProfile + 
                    "', start_music = '" + startMusic + 
                    "', end_music = '" + endMusic + 
                    "', fill_delay_options_name = '" + fillDelay +
                    "', startVolume = '" + startVol +
                    "', endVolume = '" + endVol +
                    "' WHERE id = 1"), "\"success\"");
                count++;
                if (count > 3)
                {
                    //ADD ERROR MESSAGE
                    Debug.WriteLine("Error");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Saves the session settings to the Pod for the next float. Normally just before starting a float. This is used for pods on without available.
        /// </summary>
        public bool changeFloatCurrentSettings(string floatTime, string lighting, string musicProfile, string startMusic, string endMusic, string fillDelay)
        {
            bool success = false;
            int count = 0;
            while (!success)
            {
                success = string.Equals(CustomWebRequest.generalSQLRequest(ipAddress, "set", "UPDATE session_settings SET float_time_options_name = '" + floatTime + "', lighting_options_name = '" + lighting + "', music_options_name = '" + musicProfile + "', start_music = '" + startMusic + "', end_music = '" + endMusic + "', fill_delay_options_name = '" + fillDelay + "' WHERE id = 1"), "\"success\"");
                count++;
                if (count > 3)
                {
                    //ADD ERROR MESSAGE
                    Debug.WriteLine("Error");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Saves a new float log to database
        /// </summary>
        public bool saveNewLog(string floatTime, string lighting, string musicProfile, string startMusic, string endMusic, string fillDelay)
        {
            bool success = false;
            int count = 0;
            while (!success)
            {
                success = string.Equals(CustomWebRequest.generalSQLRequest(ipAddress, "set", "INSERT INTO user_log(Time, User, Page, FloatTimeChoice, LightingChoice, MusicChoice, StartMusic, EndMusic, FillDelayChoice, TemperatureRes, TemperaturePump, TemperatureOrbit) VALUES ('"+ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+ "', 'App', 'App Dashboard', '" + floatTime + "', '"+ lighting + "', '" + musicProfile + "', '" + startMusic + "', '"+endMusic+"', '"+ fillDelay + "', '0', '0', '0')"), "\"success\"");
                count++;
                if (count > 3)
                {
                    //ADD ERROR MESSAGE
                    Debug.WriteLine("Error");
                    Debug.WriteLine("INSERT INTO user_log(Time, User, Page, FloatTimeChoice, LightingChoice, MusicChoice, StartMusic, EndMusic, FillDelayChoice, TemperatureRes, TemperaturePump, TemperatureOrbit) VALUES ('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'App', '" + floatTime + "', '" + lighting + "', '" + musicProfile + "', '" + startMusic + "', '" + endMusic + "', '" + fillDelay + "', '0', '0', '0')");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Sets a custom float time. This function is not used
        /// </summary>
        public bool customFloatTimeChange(string time)
        {
            bool success = false;
            int count = 0;
            while (!success)
            {
                success = string.Equals(CustomWebRequest.generalSQLRequest(ipAddress, "set", "UPDATE float_time_options SET time = '" + time + "' WHERE name = 'Custom'"), "\"success\"");
                count++;
                if (count > 3)
                {
                    //ADD ERROR MESSAGE
                    Debug.WriteLine("Error");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Sets a custom fill delay. This function is not used
        /// </summary>
        public bool customFillDelayChange(string time)
        {
            bool success = false;
            int count = 0;
            while (!success)
            {
                success = string.Equals(CustomWebRequest.generalSQLRequest(ipAddress, "set", "UPDATE fill_delay_options SET time = '" + time + "' WHERE name = 'Custom'"), "\"success\"");
                count++;
                if (count > 3)
                {
                    //ADD ERROR MESSAGE
                    Debug.WriteLine("Error");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Sets a custom music time. This function is not used
        /// </summary>
        public bool customMusicChange(string start, string end)
        {
            bool success = false;
            int count = 0;
            while (!success)
            {
                success = string.Equals(CustomWebRequest.generalSQLRequest(ipAddress, "set", "UPDATE music_profiles SET run_time = '" + start + "' WHERE music_options_name = 'Custom' AND segment = 'start'"), "\"success\"");
                count++;
                if (count > 3)
                {
                    //ADD ERROR MESSAGE
                    Debug.WriteLine("Error");
                    return false;
                }
            }
            success = false;
            while (!success)
            {
                success = string.Equals(CustomWebRequest.generalSQLRequest(ipAddress, "set", "UPDATE music_profiles SET run_time = '" + end + "' WHERE music_options_name = 'Custom' AND segment = 'end'"), "\"success\"");
                count++;
                if (count > 3)
                {
                    //ADD ERROR MESSAGE
                    Debug.WriteLine("Error");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Updates a parameter in the Pod config table.
        /// </summary>
        /// <param name="setting">Value of the parameter</param>
        /// <param name="parameter">Setting to update</param>
        /// <returns></returns>
        public bool changeOrbitConfigSetting(string setting, string parameter)
        {
            bool success = false;
            int count = 0;
            Debug.WriteLine("UPDATE config SET value = '" + parameter + "' WHERE name = '" + setting + "'");
            while (!success)
            {
                success = string.Equals(CustomWebRequest.generalSQLRequest(ipAddress, "set", "UPDATE config SET value = '"+parameter+"' WHERE name = '" + setting + "'"), "\"success\"");
                count++;
                if (count > 3)
                {
                    //ADD ERROR MESSAGE
                    Debug.WriteLine("Error");
                    return false;
                }
            }
            return true;

        }

        /// <summary>
        /// Dismisses the assist alert on the Pod
        /// </summary>
        public bool dismissAssist()
        {
            bool success = false;
            int count = 0;
            while (!success)
            {
                success = string.Equals(CustomWebRequest.generalSQLRequest(ipAddress, "set", "UPDATE control_status SET Value = 0 WHERE name = 'Assist'"), "\"success\"");
                count++;
                if (count > 3)
                {
                    //ADD ERROR MESSAGE
                    Debug.WriteLine("Error");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Set heated float mode on the Pod
        /// </summary>
        /// <param name="on">1 is on, 0 is off</param>
        /// <returns></returns>
        public bool setHeatedFloat(int on)
        {
            string request = "UPDATE config SET value = '" + on + "' WHERE name = 'orbit_to_orbit_during_float'";
            if(SQLRequestRepeat("set", request) != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets all the logs between two given dates
        /// </summary>
        public string getLogs(DateTime startDate, DateTime endDate)
        {
            string result = SQLRequestRepeat("get", "SELECT Time, Page, FloatTimeChoice, LightingChoice, MusicChoice, StartMusic, EndMusic, FillDelayChoice FROM user_log WHERE Time BETWEEN '"+ startDate.ToString("yyyy-MM-dd") +"' AND '" + endDate.ToString("yyyy-MM-dd") + "'");
            return result;
        }

        /// <summary>
        /// Gets the date of the first log in the database
        /// </summary>
        public DateTime getFirstLogDate()
        {
            string result = SQLRequestRepeat("get", "SELECT Time FROM user_log ORDER BY Time ASC LIMIT 1");

            result = result.Replace("[{\"Time\":\"", "");
            result = result.Replace("\"}]", "");

            Debug.WriteLine(result);
            return DateTime.ParseExact(result, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the date of the final log in the database
        /// </summary>
        public DateTime getLastLogDate()
        {
            string result = SQLRequestRepeat("get", "SELECT Time FROM user_log ORDER BY Time DESC LIMIT 1");

            result = result.Replace("[{\"Time\":\"", "");
            result = result.Replace("\"}]", "");
            Debug.WriteLine(result);
            return DateTime.ParseExact(result, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Updates the SQL database. Used in the update view model to alter the table.
        /// </summary>
        /// <param name="sql">Sql command to run</param>
        /// <returns></returns>
        public bool updateSQLDatabase(string sql)
        {
            if (SQLRequestRepeat("set", sql) != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the sql row exists. Used to prevent duplicates rows during updates
        /// </summary>
        /// <param name="tableName">Name of the table to check</param>
        /// <param name="check">Statement to check against</param>
        /// <returns></returns>
        public string checkSQLexists(string tableName, string check)
        {
            return SQLRequestRepeat("get", "SELECT COUNT(1) FROM " + tableName + " WHERE " + check);
        }

        /// <summary>
        /// Gets the total fill time, including pre-prime time
        /// </summary>
        public int getFillTime()
        {
            string start = getData("fill_run_time_start", false);
            string middle = getData("fill_run_time_middle", false);
            string end = getData("fill_run_time_end", false);

            int startTime = 0;
            int middleTime = 0;
            int endTime = 0;
            int prePrimeTime = 0;

            int.TryParse(start, out startTime);
            int.TryParse(middle, out middleTime); 
            int.TryParse(end, out endTime);

            
            if(getData("pre_prime_enabled", false) == "1")
            {
                int.TryParse(getData("pre_prime_duration", false), out prePrimeTime);
            }


            return startTime + middleTime + endTime + prePrimeTime;
        }

        /// <summary>
        /// Gets the total empty time
        /// </summary>
        public int getEmptyTime()
        {
            int time = 0;
            string start = getData("empty_run_time_start", false);
            string middle = getData("empty_run_time_middle", false);
            string end = getData("empty_run_time_end", false);

            int startTime;
            int middleTime;
            int endTime;

            int.TryParse(start, out startTime);
            int.TryParse(middle, out middleTime);
            int.TryParse(end, out endTime);

            return time + startTime + middleTime + endTime;
        }
    }
}
