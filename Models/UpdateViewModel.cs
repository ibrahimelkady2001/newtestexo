using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
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
    /// Viewmodel for the update view
    /// When adding new updates, remember to change the latest versions
    /// </summary>
    class UpdateViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        Version latestBE = new Version("1.2.2.3"); //UPDATE THIS FOR EACH ORBITBE
        Version latestDB = new Version("1.4.0"); //UPDATE THIS FOR EACH DB
        public ICommand OnExit { protected set; get; }
        public ICommand UpdateCommand { protected set; get; }
        public ICommand extraFeaturesCommand { protected set; get; }

        string outputLog { get; set; }
        string podUpdateList { get; set; }

        bool controlsAvailable;
        string extraPassword { get; set; }
        int keyValue;
        string key { get; set; }

        /// <summary>
        /// Constructor for the update viewmodel
        /// </summary>
        public UpdateViewModel()
        {
            OnExit = new Command(async () => await onExit());
            UpdateCommand = new Command(async () => await updateButton());
            extraFeaturesCommand = new Command(() => updateExtraButton());
            OutputLog = "Updater Ready";
            fillPodUpdateList();
            controlsAvailable = true;
            Random random = new Random();
            keyValue = random.Next(9999);
            Key = keyValue.ToString();
        }

        /// <summary>
        /// Fills the Pod update log with the list of available pod and whether they can be updated.
        /// </summary>
        void fillPodUpdateList()
        {
            PodUpdateList = "";
            foreach (PodModel orb in Globals.getList())
            {
                bool needUpdate = false;
                needUpdate = compareVersions(GetVersionOrbit(orb), latestBE);
                needUpdate = needUpdate || compareVersions(GetVersionDatabase(orb), latestDB);

                if(needUpdate)
                {
                    PodUpdateList += "Pod-" + orb.podNumber + ": Update Available\n";
                }
                else
                {
                    PodUpdateList += "Pod-" + orb.podNumber + ": Up to date\n";
                }
            }
        }

        /// <summary>
        /// Exits the Update view and returns to the dashboard.
        /// </summary>
        private async Task onExit()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }

        /// <summary>
        /// Adds a new line to the update log. This version includes the pod number
        /// </summary>
        void updateOutputLog(Models.PodModel orb, string message)
        {
            OutputLog = DateTime.Now.ToString("HH:mm:ss") + " EXO-" + orb.podNumber + ": " + message + "\n" + OutputLog;
        }
        /// <summary>
        /// Adds a new line to the update log. This version does not include the pod number
        /// </summary>
        void updateOutputLog(string message)
        {
            OutputLog = DateTime.Now.ToString("HH:mm:ss") + ": " + message + "\n" + OutputLog;
        }

        /// <summary>
        /// Updates all Pods
        /// </summary>
        async Task updateButton()
        {
            controlsAvailable = false;
            foreach(PodModel orb in Globals.getList())
            {
                updateOutputLog(orb, "Updating EXOBE");
                await updatePod(orb);
                await updateSQL(orb);
                updateOutputLog(orb, "Update Complete");

            }
            updateOutputLog("All Updates Complete");
            updateOutputLog("Please log out and log back into the app");
            fillPodUpdateList();
            controlsAvailable = true;
        }
        
        /// <summary>
        /// Updates the Pods with the optional extra features
        /// </summary>
        void updateExtraButton()
        {
            controlsAvailable = false;
            foreach (PodModel orb in Globals.getList())
            {
                UpdateAdditionalFeatures(orb);
            }
            updateOutputLog("Optional Feature Update Complete - Check the log for details");
            updateOutputLog("Please log out and log back into the app");
            fillPodUpdateList();
            controlsAvailable = true;
        }

        /// <summary>
        /// Used a randomly generated password extra features. Needs replacing in future.
        /// </summary>
        int tempHashFunction()
        {
            return ((keyValue % 17) + (keyValue % 324) + (keyValue % 8592) + (keyValue % 1113) + (keyValue % 3) + (keyValue % 2)); 
        }

        /// <summary>
        /// Extra feature commands
        /// </summary>
        /// <param name="pod"></param>
        void UpdateAdditionalFeatures(PodModel pod)
        {
            switch(ExtraPassword)
            {
                case "adminoverridehwlp1":
                    {
                        updateOutputLog(pod, "Applying Optional Feature Update");
                        lightingTherapyPack(pod);
                        break;
                    }
                case "HealthAndWellness":
                    {
                        if(DateTime.Now > new DateTime(2023, 10, 14))
                        {
                            updateOutputLog("Code Invalid, Please Contact Wellness Support");
                            break;
                        }
                        lightingTherapyPack(pod);
                        break;
                    }
                default:
                    {
                        if (ExtraPassword == tempHashFunction().ToString())
                        {
                            updateOutputLog(pod, "Applying Optional Feature Update");
                            lightingTherapyPack(pod);
                            break;
                        }
                        updateOutputLog("Incorrect Code");
                        break;
                    }


            }
        }


        /// <summary>
        /// Checks if the pod is running the backend.
        /// </summary>
        bool getPodRunning(Models.PodModel orb)
        {
            string programStarted = orb.getData("ProgramStarted", true);
            return programStarted == "1";
        }

        /// <summary>
        /// Compares the two versions to check if any updates are available
        /// </summary>
        private bool compareVersions(Version current, Version update)
        {
            if (current.CompareTo(update) < 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the version of the Pod backend
        /// </summary>
        private Version GetVersionOrbit(Models.PodModel orb)
        {
            string beVersion = orb.softwareVersion[0];
            return new Version(beVersion);
        }

        /// <summary>
        /// Gets the version of the Pod Database.
        /// </summary>
        private Version GetVersionDatabase(Models.PodModel orb)
        {
            string dbVersion = orb.softwareVersion[1];
            return new Version(dbVersion);
        }

        /// <summary>
        /// Gets the hardware of the Pod. 0 - Jessie, 1 - Buster, 2 - Buster using Industrial Shield (not used), 3 - Buster using PCB.
        /// If the parameter is missing, the updater assumes hardware 0
        /// </summary>
        private string GetHardware(Models.PodModel orb)
        {
            return orb.softwareVersion[2];
        }

        /// <summary>
        /// Updates the Pod
        /// </summary>
        async Task<string> updatePod(Models.PodModel orb)
        {
            string host = orb.ipAddress.Replace("http://", "");
            host = host.Replace("/", "");
            string username = "pi";
            string password = "orbit";
            string remoteDirectory = "/home/pi/Orbit/";
            string hardware = GetHardware(orb);
            string updateFile = "";

            if (!compareVersions(GetVersionOrbit(orb), latestBE))
            {
                updateOutputLog(orb, "EXOBE is Already Up to date");
                Debug.WriteLine("EXOBE Already up to date");
                return "EXOBE Already Up to date";
            }
            if(hardware == "3")
            {
                updateFile = "EXOApp.Assets.Backend.BEhw3";
            }
            if (hardware == "1") // Pi4
            {
                updateFile = "EXOApp.Assets.Backend.BEhw1";
            }
            else
            {
                updateFile = "EXOApp.Assets.Backend.BEhw0";
            }
            string status = "All Good";
            await Task.Run(() =>
            {
                using (SshClient sshc = new SshClient(host, username, password))
                {
                    try
                    {
                        //First run the command to stop the backend running
                        updateOutputLog(orb, "Shutting Down EXOBE");
                        sshc.Connect();
                        sshc.RunCommand("sudo systemctl stop orbitBE.service");
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("An exception has been caught " + e.ToString());
                        updateOutputLog(orb, "Intial Connection Error");
                        status = "Intial Connection Error";
                    }
                    if (status == "All Good")
                    {
                        // Wait for the backend to finish running
                        int counter = 0;
                        updateOutputLog(orb, "Waiting for EXOBE to shutdown");
                        while (getPodRunning(orb))
                        {
                            Thread.Sleep(1000);
                            counter++;
                            if (counter > 10)
                            {
                                // If backend takes to long to stop, abort update
                                updateOutputLog(orb, "EXOBE Stopping Error");
                                status = "Pod Shutdown Error";
                                break;
                            }
                        }
                    }
                    if (status == "All Good")
                    {
                        Thread.Sleep(5000);
                        updateOutputLog(orb, "Shutdown complete");

                        using (SftpClient sftp = new SftpClient(host, username, password))
                        {
                            try
                            {
                                //Begin the update process
                                updateOutputLog(orb, "Applying Update");
                                sftp.Connect();
                                sftp.ChangeDirectory(remoteDirectory);
                                using (var uplfileStream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream(updateFile))
                                {
                                    //Copies the backend file to the pod
                                    sftp.UploadFile(uplfileStream, updateFile, true);
                                }
                                //Deletes the existing backend from the pod
                                sftp.DeleteFile("orbitBE");
                                //Renames the new backend to the name of the old one.
                                sftp.RenameFile(updateFile, "orbitBE");
                                sftp.Disconnect();
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("An exception has been caught " + e.ToString());
                            }
                        }
                        try
                        {
                            //Changes the permissions of the backend file, then starts the backend.
                            updateOutputLog(orb, "Update Applied");
                            Thread.Sleep(1000);
                            updateOutputLog(orb, "Restarting EXOBE");
                            Thread.Sleep(4000);
                            sshc.RunCommand("sudo chmod -R 777 /home/pi/Orbit/orbitBE");
                            sshc.RunCommand("sudo systemctl start orbitBE.service");
                            sshc.Disconnect();
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("An exception has been caught " + e.ToString());
                            status = "Ending Connection Error";
                        }
                    }
                }
            });
            updateOutputLog(orb, "EXOBE Update Complete");
            return "";
        }


        // Updates to SQL start here. Naming scheme = updateSQLxx
        //------------------------------------------------------------------------------------------------------------------------------------------


        /// <summary>
        /// Updates the sql database on the pod
        /// </summary>
        async Task updateSQL(Models.PodModel orb)
        {
            updateOutputLog(orb, "Updating EXO Database");
            if (!compareVersions(GetVersionDatabase(orb), latestDB))
            {
                updateOutputLog(orb, "EXO Database Already Up to Date");
                return;
            }
            await Task.Run(() =>
            {
                //In the updateSQLxx functions, always compare the version numbers before continuing.
                updateSQL11(orb, GetVersionDatabase(orb));
                updateSQL12(orb, GetVersionDatabase(orb));
                updateSQL13(orb, GetVersionDatabase(orb));
                updateSQL14(orb, GetVersionDatabase(orb));
                updateOutputLog(orb, "EXO Database Update Complete");
            });
        }

        /// <summary>
        /// SQL update for version 1.1
        /// </summary>
        void updateSQL11(Models.PodModel orb, Version current)
        {
            Version update = new Version("1.1.0");
            if (!compareVersions(current, update))
            {
                return;
            }
            updateOutputLog(orb, "Performing 1.1.0 Database Update");
            //Hardware
            string hardware = GetHardware(orb);
            if (hardware == "1" || hardware == "2")
            {
                //do nothing
            }
            else //Pi3
            {
                checkAndAddDB(orb, "INSERT INTO version(name, version) VALUES ('hardware', '0')", "version", "name = 'hardware'");
            }
            //1.2.2
            checkAndAddDB(orb, "CREATE TABLE leds(id int NOT NULL AUTO_INCREMENT, name text, red_base int, green_base int, blue_base int, red_halo int, green_halo int, blue_halo int, PRIMARY KEY (id))");
            checkAndAddDB(orb, "INSERT INTO config(name, auth_level, value, max_limit, min_limit, orbit_mode, display_group, description) VALUES ('standard_led_on', '39', '1', '1', '0', 'all', 'general', 'Default LEDs On')", "config", "name = 'standard_led_on'");

            //LED Colours
            checkAndAddDB(orb, "INSERT INTO leds(name, red_base, green_base, blue_base, red_halo, green_halo, blue_halo) VALUES ('standby', '255', '255', '255', '255', '255', '255')", "leds", "name = 'standby'");
            checkAndAddDB(orb, "INSERT INTO leds(name, red_base, green_base, blue_base, red_halo, green_halo, blue_halo) VALUES ('fill', '255', '255', '255', '255', '255', '255')", "leds", "name = 'fill'");
            checkAndAddDB(orb, "INSERT INTO leds(name, red_base, green_base, blue_base, red_halo, green_halo, blue_halo) VALUES ('empty', '255', '255', '255', '255', '255', '255')", "leds", "name = 'empty'");

            //Custom Float Time
            //checkAndAddDB(orb, "INSERT INTO float_time_options(name, display_name, time) VALUES ('Custom', 'Custom', '1800')");
            //checkAndAddDB(orb, "INSERT INTO music_profiles(music_options_name, segment, start_delay, volume, ramp_time, run_time) VALUES ('Custom', 'start', '10', '75', '5', '280')");
            //checkAndAddDB(orb, "INSERT INTO music_profiles(music_options_name, segment, start_delay, volume, ramp_time, run_time) VALUES ('Custom', 'end', '300', '75', '5', '280')");


            //Initial Music Database
            checkAndAddDB(orb, "CREATE TABLE music(id int NOT NULL AUTO_INCREMENT, name text, premiumLevel int, defaultVolume int, PRIMARY KEY (id))");
            checkAndAddDB(orb, "INSERT INTO music(name, premiumLevel, defaultVolume) VALUES ('df_Alpha_Waves.mp3', '0', '100')", "music", "name = 'df_Alpha_Waves.mp3'");
            checkAndAddDB(orb, "INSERT INTO music(name, premiumLevel, defaultVolume) VALUES ('df_Meditation.mp3', '0', '100')", "music", "name = 'df_Meditation.mp3'");
            checkAndAddDB(orb, "INSERT INTO music(name, premiumLevel, defaultVolume) VALUES ('df_Gentle_Stream.mp3', '0', '100')", "music", "name = 'df_Gentle_Stream.mp3'");
            checkAndAddDB(orb, "INSERT INTO music(name, premiumLevel, defaultVolume) VALUES ('df_Harp_Music.mp3', '0', '100')", "music", "name = 'df_Harp_Music.mp3'");
            checkAndAddDB(orb, "INSERT INTO music(name, premiumLevel, defaultVolume) VALUES ('df_Thunderstorm_with_Music.mp3', '0', '100')", "music", "name = 'df_Thunderstorm_with_Music.mp3'");
            checkAndAddDB(orb, "INSERT INTO music(name, premiumLevel, defaultVolume) VALUES ('df_Ocean_Waves.mp3', '0', '100')", "music", "name = 'df_Ocean_Waves.mp3'");
            checkAndAddDB(orb, "INSERT INTO music(name, premiumLevel, defaultVolume) VALUES ('df_Rain_and_Thunder.mp3', '0', '100')", "music", "name = 'df_Rain_and_Thunder.mp3'");
            checkAndAddDB(orb, "INSERT INTO music(name, premiumLevel, defaultVolume) VALUES ('df_Wind_Chimes.mp3', '0', '100')", "music", "name = 'df_Wind_Chimes.mp3'");

            //Smart Empty
            checkAndAddDB(orb, "INSERT INTO config(name, auth_level, value, max_limit, min_limit, units, orbit_mode, display_group, edit_type, description) VALUES ('smart_empty_enabled', '39', '0', '1', '0', '', 'empty', 'fill_pump', 'number', 'Enable smart empty')", "config", "name = 'smart_empty_enabled'");
            checkAndAddDB(orb, "INSERT INTO config(name, auth_level, value, max_limit, min_limit, units, orbit_mode, display_group, edit_type, description) VALUES ('max_empty_time', '39', '300', '360', '240', 's', 'empty', 'fill_pump', 'number', 'Smart empty max empty time')", "config", "name = 'max_empty_time'");

            //Door Status
            checkAndAddDB(orb, "INSERT INTO control_status(name, value, display_name, display_order) VALUES ('DoorState', '0', 'Current Door Status', '19')", "control_status", "name = 'DoorState'");

            //Float Time Sync
            checkAndAddDB(orb, "INSERT INTO control_status(name, value, display_name, display_order) VALUES ('TotalModeTime', '0', 'Total Time Requied', '0')", "control_status", "name = 'TotalModeTime'");
            checkAndAddDB(orb, "INSERT INTO control_status(name, value, display_name, display_order) VALUES ('ElapsedModeTime', '0', 'Elapsed Mode Time', '0')", "control_status", "name = 'ElapsedModeTime'");

            //Hibernate and Service Mode
            checkAndAddDB(orb, "INSERT INTO control(name, display_name, request, display_order) VALUES ('hibernate', 'Hibernate', '0', '14')", "control", "name = 'hibernate'");
            checkAndAddDB(orb, "INSERT INTO control(name, display_name, request, display_order) VALUES ('servicepod', 'Solution In Pod', '0', '15')", "control", "name = 'servicepod'");
            checkAndAddDB(orb, "INSERT INTO control(name, display_name, request, display_order) VALUES ('serviceres', 'Solution In Res', '0', '16')", "control", "name = 'serviceres'");

            //Version Number
            checkAndAddDB(orb, "UPDATE version SET version = '1.1.0' WHERE name = 'databaseVersion'", "version", "version = '1.1.0'");

            //Level Parameters
            checkAndAddDB(orb, "INSERT INTO config(name, auth_level, value, max_limit, min_limit, orbit_mode, display_group, description) VALUES ('target_level_top', '39', '1.7', '1', '0', 'Level Parameters', 'general', 'Target Level Top of Res')", "config", "name = 'target_level_top'");
            checkAndAddDB(orb, "INSERT INTO config(name, auth_level, value, max_limit, min_limit, orbit_mode, display_group, description) VALUES ('target_level_bottom', '39', '0.2', '1', '0', 'Level Parameters', 'general', 'Target Level Bottom of Res')", "config", "name = 'target_level_bottom'");
            checkAndAddDB(orb, "INSERT INTO control_status(name, value, display_name, units, display_order) VALUES ('flowRate', '0', 'Flow Rate', 'm/s', '20')", "control_status", "name = 'flowRate'");

            //Pre Float Heating
            checkAndAddDB(orb, "INSERT INTO config(name, auth_level, value, max_limit, min_limit, orbit_mode, display_group, description) VALUES ('pre_float_heat_enabled', '39', '0', '1', '0', 'Pre Float Heating', 'general', 'Pre Float Heat Enabled')", "config", "name = 'pre_float_heat_enabled'");
            checkAndAddDB(orb, "INSERT INTO config(name, auth_level, value, max_limit, min_limit, orbit_mode, display_group, description) VALUES ('pre_float_heat_inverter_speed', '39', '50', '50', '10', 'Pre Float Heating', 'general', 'Pre Float Heat Inverter Speed')", "config", "name = 'pre_float_heat_inverter_speed'");
            checkAndAddDB(orb, "INSERT INTO config(name, auth_level, value, max_limit, min_limit, orbit_mode, display_group, description) VALUES ('pre_float_heat_run_time', '39', '60', '30', '300', 'Pre Float Heating', 'general', 'Pre Float Heat Run Time')", "config", "name = 'pre_float_heat_run_time'");
            checkAndAddDB(orb, "INSERT INTO config(name, auth_level, value, max_limit, min_limit, orbit_mode, display_group, description) VALUES ('pre_float_heat_use_float_time', '39', '0', '1', '0', 'Pre Float Heating', 'general', 'Pre Float Heat Use Float Time')", "config", "name = 'pre_float_heat_use_float_time'");

            //Lighting Profiles
            //Sunset Sunrise Continuous
            addLightingNode(orb, "Sunset/Sunrise Continuous", "1", 255, 255, 160, 0);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "1", 255, 255, 100, 30);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "1", 255, 255, 50, 60);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "1", 255, 255, 0, 90);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "1", 255, 200, 0, 120);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "1", 255, 150, 0, 150);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "1", 200, 100, 0, 180);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "1", 150, 50, 0, 210);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "1", 100, 0, 0, 220);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "1", 50, 0, 0, 240);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "1", 25, 0, 0, 250);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "1", 25, 0, 0, 300);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "2", 25, 0, 0, 0);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "2", 50, 0, 0, 30);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "2", 100, 0, 0, 60);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "2", 150, 50, 0, 90);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "2", 200, 100, 0, 120);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "2", 255, 150, 0, 150);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "2", 255, 200, 0, 180);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "2", 255, 255, 0, 210);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "2", 255, 255, 50, 240);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "2", 255, 255, 160, 250);
            addLightingNode(orb, "Sunset/Sunrise Continuous", "2", 255, 255, 0, 300);
            //Aurora Continuous
            addLightingNode(orb, "Aurora Continuous", "1", 255, 255, 255, 0);
            addLightingNode(orb, "Aurora Continuous", "1", 0, 255, 0, 10);
            addLightingNode(orb, "Aurora Continuous", "1", 0, 255, 0, 30);
            addLightingNode(orb, "Aurora Continuous", "1", 0, 255, 128, 60);
            addLightingNode(orb, "Aurora Continuous", "1", 0, 218, 110, 90);
            addLightingNode(orb, "Aurora Continuous", "1", 0, 187, 187, 120);
            addLightingNode(orb, "Aurora Continuous", "1", 0, 154, 154, 150);
            addLightingNode(orb, "Aurora Continuous", "1", 0, 62, 124, 180);
            addLightingNode(orb, "Aurora Continuous", "1", 0, 45, 91, 210);
            addLightingNode(orb, "Aurora Continuous", "1", 0, 0, 60, 240);
            addLightingNode(orb, "Aurora Continuous", "1", 0, 28, 28, 270);
            addLightingNode(orb, "Aurora Continuous", "1", 0, 25, 0, 300);
            addLightingNode(orb, "Aurora Continuous", "2", 0, 25, 0, 0);
            addLightingNode(orb, "Aurora Continuous", "2", 0, 25, 28, 30);
            addLightingNode(orb, "Aurora Continuous", "2", 0, 0, 60, 60);
            addLightingNode(orb, "Aurora Continuous", "2", 0, 45, 91, 90);
            addLightingNode(orb, "Aurora Continuous", "2", 0, 62, 124, 120);
            addLightingNode(orb, "Aurora Continuous", "2", 0, 154, 154, 150);
            addLightingNode(orb, "Aurora Continuous", "2", 0, 187, 187, 180);
            addLightingNode(orb, "Aurora Continuous", "2", 0, 218, 110, 210);
            addLightingNode(orb, "Aurora Continuous", "2", 0, 255, 128, 240);
            addLightingNode(orb, "Aurora Continuous", "2", 0, 255, 0, 270);
            addLightingNode(orb, "Aurora Continuous", "2", 0, 255, 0, 290);
            addLightingNode(orb, "Aurora Continuous", "2", 255, 255, 255, 300);


            //artifical wait
        }

        /// <summary>
        /// SQL update for version 1.2
        /// </summary>
        void updateSQL12(Models.PodModel orb, Version current)
        {
            Version update = new Version("1.2.0");
            if (!compareVersions(current, update))
            {
                return;
            }
            updateOutputLog(orb, "Performing 1.2.0 Database Update");

            //------------00/15------------
            checkAndAddDB(orb, "INSERT INTO music_profiles(music_options_name, segment, start_delay, volume, ramp_time, run_time) VALUES ('00/15', 'start', '0', '0', '0', '0')", "music_profiles", "music_options_name = '00/15' AND segment = 'start'");
            checkAndAddDB(orb, "INSERT INTO music_profiles(music_options_name, segment, start_delay, volume, ramp_time, run_time) VALUES ('00/15', 'end', '880', '75', '5', '880')", "music_profiles", "music_options_name = '00/15' AND segment = 'end'");
            
            //------------10/10------------
            checkAndAddDB(orb, "INSERT INTO music_profiles(music_options_name, segment, start_delay, volume, ramp_time, run_time) VALUES ('10/10', 'start', '10', '75', '5', '580')", "music_profiles", "music_options_name = '10/10' AND segment = 'start'");
            checkAndAddDB(orb, "INSERT INTO music_profiles(music_options_name, segment, start_delay, volume, ramp_time, run_time) VALUES ('10/10', 'end', '580', '75', '5', '580')", "music_profiles", "music_options_name = '10/10' AND segment = 'end'");

            //------------10/15------------
            checkAndAddDB(orb, "INSERT INTO music_profiles(music_options_name, segment, start_delay, volume, ramp_time, run_time) VALUES ('10/15', 'start', '10', '75', '5', '580')", "music_profiles", "music_options_name = '10/15' AND segment = 'start'");
            checkAndAddDB(orb, "INSERT INTO music_profiles(music_options_name, segment, start_delay, volume, ramp_time, run_time) VALUES ('10/15', 'end', '880', '75', '5', '880')", "music_profiles", "music_options_name = '10/15' AND segment = 'end'");

            //------------15/05------------
            checkAndAddDB(orb, "INSERT INTO music_profiles(music_options_name, segment, start_delay, volume, ramp_time, run_time) VALUES ('15/05', 'start', '10', '75', '5', '880')", "music_profiles", "music_options_name = '15/05' AND segment = 'start'");
            checkAndAddDB(orb, "INSERT INTO music_profiles(music_options_name, segment, start_delay, volume, ramp_time, run_time) VALUES ('15/05', 'end', '300', '75', '5', '280')", "music_profiles", "music_options_name = '15/05' AND segment = 'end'");

            //------------15/10------------
            checkAndAddDB(orb, "INSERT INTO music_profiles(music_options_name, segment, start_delay, volume, ramp_time, run_time) VALUES ('15/10', 'start', '10', '75', '5', '880')", "music_profiles", "music_options_name = '15/10' AND segment = 'start'");
            checkAndAddDB(orb, "INSERT INTO music_profiles(music_options_name, segment, start_delay, volume, ramp_time, run_time) VALUES ('15/10', 'end', '580', '75', '5', '580')", "music_profiles", "music_options_name = '15/10' AND segment = 'end'");

            //------------55/10------------
            checkAndAddDB(orb, "INSERT INTO music_profiles(music_options_name, segment, start_delay, volume, ramp_time, run_time) VALUES ('55/10', 'start', '10', '75', '5', '3280')", "music_profiles", "music_options_name = '55/10' AND segment = 'start'");
            checkAndAddDB(orb, "INSERT INTO music_profiles(music_options_name, segment, start_delay, volume, ramp_time, run_time) VALUES ('55/10', 'end', '580', '75', '5', '580')", "music_profiles", "music_options_name = '55/10' AND segment = 'end'");

            //------------55/15------------
            checkAndAddDB(orb, "INSERT INTO music_profiles(music_options_name, segment, start_delay, volume, ramp_time, run_time) VALUES ('55/15', 'start', '10', '75', '5', '3280')", "music_profiles", "music_options_name = '55/15' AND segment = 'start'");
            checkAndAddDB(orb, "INSERT INTO music_profiles(music_options_name, segment, start_delay, volume, ramp_time, run_time) VALUES ('55/15', 'end', '880', '75', '5', '880')", "music_profiles", "music_options_name = '55/15' AND segment = 'end'");

            //------------Continuous/10------------
            checkAndAddDB(orb, "INSERT INTO music_profiles(music_options_name, segment, start_delay, volume, ramp_time, run_time) VALUES ('Continuous/10', 'start', '10', '80', '5', '-1')", "music_profiles", "music_options_name = 'Continuous/10' AND segment = 'start'");
            checkAndAddDB(orb, "INSERT INTO music_profiles(music_options_name, segment, start_delay, volume, ramp_time, run_time) VALUES ('Continuous/10', 'end', '580', '75', '5', '580')", "music_profiles", "music_options_name = 'Continuous/10' AND segment = 'end'");

            //------------Continuous/15------------
            checkAndAddDB(orb, "INSERT INTO music_profiles(music_options_name, segment, start_delay, volume, ramp_time, run_time) VALUES ('Continuous/15', 'start', '10', '80', '5', '-1')", "music_profiles", "music_options_name = 'Continuous/15' AND segment = 'start'");
            checkAndAddDB(orb, "INSERT INTO music_profiles(music_options_name, segment, start_delay, volume, ramp_time, run_time) VALUES ('Continuous/15', 'end', '880', '75', '5', '880')", "music_profiles", "music_options_name = 'Continuous/15' AND segment = 'end'");

            checkAndAddDB(orb, "UPDATE version SET version = '1.2.0' WHERE name = 'databaseVersion'", "version", "version = '1.2.0'");

        }

        /// <summary>
        /// SQL update for version 1.3
        /// </summary>
        void updateSQL13(Models.PodModel orb, Version current)
        {
            Version update = new Version("1.3.0");
            if (!compareVersions(current, update))
            {
                return;
            }
            updateOutputLog(orb, "Performing 1.3.0 Database Update");

            //Wash Down
            checkAndAddDB(orb, "INSERT INTO config(name, auth_level, value, max_limit, min_limit, orbit_mode, display_group, description) VALUES ('wash_down_available', '99', '0', '1', '0', 'all', 'wash_down_available', 'Wash Down Available')", "config", "name = 'wash_down_available'");
            checkAndAddDB(orb, "INSERT INTO config(name, auth_level, value, max_limit, min_limit, orbit_mode, display_group, description) VALUES ('wash_down_time', '39', '0', '300', '180', 'all', 'wash_down_time', 'Wash Down Time')", "config", "name = 'wash_down_time'");
            checkAndAddDB(orb, "INSERT INTO control(name, display_name, request, display_order) VALUES ('washdown', 'Wash Down', '0', '17')", "control", "name = 'washdown'");

            //Hibernate
            checkAndAddDB(orb, "INSERT INTO control_status(name, value, display_name, display_order) VALUES ('HibernateState', '0', 'Hibernate State', '0')", "control_status", "name = 'HibernateState'");

            
            checkAndAddDB(orb, "UPDATE version SET version = '1.3.0' WHERE name = 'databaseVersion'", "version", "version = '1.3.0'");
        }

        /// <summary>
        /// SQL update for version 1.4
        /// </summary>
        void updateSQL14(Models.PodModel orb, Version current)
        {
           
            Version update = new Version("1.4.0");
            if (!compareVersions(current, update))
            {
                return;
            }
            updateOutputLog(orb, "Performing 1.4.0 Database Update");

            checkAndAddDB(orb, "INSERT INTO control_status(name, value, display_name, display_order) VALUES ('HeaterCurrent', '0', 'Heater Current', '19')", "control_status", "name = 'HeaterCurrent'");

            orb.updateSQLDatabase("ALTER TABLE session_settings ADD startVolume INT");
            orb.updateSQLDatabase("ALTER TABLE session_settings ADD endVolume INT");
            
            // Night Sky 
            addLightingNode(orb, "Night Sky", "1", 255, 255, 255, 0);
            addLightingNode(orb, "Night Sky", "1", 255, 169, 0, 10);
            addLightingNode(orb, "Night Sky", "1", 255, 0, 0, 30);
            addLightingNode(orb, "Night Sky", "1", 230, 0, 45, 60); //80%
            addLightingNode(orb, "Night Sky", "1", 192, 0, 115, 90);// 70%
            addLightingNode(orb, "Night Sky", "1", 153, 0, 153, 120);// 60%
            addLightingNode(orb, "Night Sky", "1", 124, 0, 140, 150);// 50%
            addLightingNode(orb, "Night Sky", "1", 108, 0, 140, 180); // 40%
            addLightingNode(orb, "Night Sky", "1", 66, 0, 102, 210); // 30%
            addLightingNode(orb, "Night Sky", "1", 20, 0, 77, 240); // 20%
            addLightingNode(orb, "Night Sky", "1", 0, 0, 40, 280); // 10%
            addLightingNode(orb, "Night Sky", "1", 0, 0, 0, 300);
            addLightingNode(orb, "Night Sky", "2", 0, 0, 0, 0);
            addLightingNode(orb, "Night Sky", "2", 0, 0, 40, 20);
            addLightingNode(orb, "Night Sky", "2", 20, 0, 77, 60);
            addLightingNode(orb, "Night Sky", "2", 66, 0, 102, 90);
            addLightingNode(orb, "Night Sky", "2", 108, 0, 140, 120);
            addLightingNode(orb, "Night Sky", "2", 153, 0, 153, 150);
            addLightingNode(orb, "Night Sky", "2", 192, 0, 115, 180);
            addLightingNode(orb, "Night Sky", "2", 230, 0, 45, 210);
            addLightingNode(orb, "Night Sky", "2", 255, 0, 0, 240);
            addLightingNode(orb, "Night Sky", "2", 255, 169, 0, 270);
            addLightingNode(orb, "Night Sky", "2", 255, 255, 255, 300);

            // Night Sky Continuous
            addLightingNode(orb, "Night Sky Continuous", "1", 255, 255, 255, 0);
            addLightingNode(orb, "Night Sky Continuous", "1", 255, 169, 0, 10);
            addLightingNode(orb, "Night Sky Continuous", "1", 255, 0, 0, 30);
            addLightingNode(orb, "Night Sky Continuous", "1", 230, 0, 45, 60); //80%
            addLightingNode(orb, "Night Sky Continuous", "1", 192, 0, 115, 90);// 70%
            addLightingNode(orb, "Night Sky Continuous", "1", 153, 0, 153, 120);// 60%
            addLightingNode(orb, "Night Sky Continuous", "1", 124, 0, 140, 150);// 50%
            addLightingNode(orb, "Night Sky Continuous", "1", 108, 0, 140, 180); // 40%
            addLightingNode(orb, "Night Sky Continuous", "1", 66, 0, 102, 210); // 30%
            addLightingNode(orb, "Night Sky Continuous", "1", 20, 0, 77, 240); // 20%
            addLightingNode(orb, "Night Sky Continuous", "1", 10, 0, 40, 280); // 10%
            addLightingNode(orb, "Night Sky Continuous", "1", 5, 0, 20, 300);
            addLightingNode(orb, "Night Sky Continuous", "2", 5, 0, 20, 0);
            addLightingNode(orb, "Night Sky Continuous", "2", 10, 0, 40, 20);
            addLightingNode(orb, "Night Sky Continuous", "2", 20, 0, 77, 60);
            addLightingNode(orb, "Night Sky Continuous", "2", 66, 0, 102, 90);
            addLightingNode(orb, "Night Sky Continuous", "2", 108, 0, 140, 120);
            addLightingNode(orb, "Night Sky Continuous", "2", 153, 0, 153, 150);
            addLightingNode(orb, "Night Sky Continuous", "2", 192, 0, 115, 180);
            addLightingNode(orb, "Night Sky Continuous", "2", 230, 0, 45, 210);
            addLightingNode(orb, "Night Sky Continuous", "2", 255, 0, 0, 240);
            addLightingNode(orb, "Night Sky Continuous", "2", 255, 169, 0, 270);
            addLightingNode(orb, "Night Sky Continuous", "2", 255, 255, 255, 300);
            
            checkAndAddDB(orb, "INSERT INTO config(name, auth_level, value, max_limit, min_limit, orbit_mode, display_group, description) VALUES ('light_music_end_sync', '39', '0', '1', '0', 'float', 'general', 'Light Music Sync')", "config", "name = 'light_music_end_sync'");
            checkAndAddDB(orb, "UPDATE version SET version = '1.4.0' WHERE name = 'databaseVersion'", "version", "version = '1.4.0'");
        }


        /// <summary>
        /// Checks the DB for existing rows, if the new row doesn't exist, add the new row.
        /// </summary>
        private void checkAndAddDB(Models.PodModel orb, string sql, string table = "", string check = "")
        {
            if (table == "" || check == "")
            {
                orb.updateSQLDatabase(sql);
            }
            else if (orb.checkSQLexists(table, check).Contains("0"))
            {
                orb.updateSQLDatabase(sql);
            }
            else
            {
                Debug.WriteLine(table + " " + check);
            }
        }

        /// <summary>
        /// Adds a new lighting node for a lighting profile in the database
        /// </summary>
        /// <param name="orb">Pod to update</param>
        /// <param name="name">Name of the lighting profile</param>
        /// <param name="segIndex">1 for intro phase, 2 for outro phase</param>
        /// <param name="red">Brightness of red</param>
        /// <param name="green">Brightness of green</param>
        /// <param name="blue">Brightness of blue</param>
        /// <param name="startTime">The time from the beginning of the phase where the node should be the specified colours.</param>
        private void addLightingNode(Models.PodModel orb, string name, string segIndex, int red, int green, int blue, int startTime)
        {
            orb.updateSQLDatabase("INSERT INTO lighting_profiles(lighting_options_name, seg_index, light_system, red, green, blue, effect, start_time) VALUES ('" + name + "', '" + segIndex + "', 'base', '" + red + "', '" + green + "', '" + blue + "', '2', '" + startTime + "')");
        }

        //Optional Content
        //On optional content, always add a new row to the premium features table to ensure no duplicates occur.
        
        /// <summary>
        /// Lighting therapy pack commands
        /// </summary>
        private void lightingTherapyPack(Models.PodModel orb)
        {

            if (!orb.checkSQLexists("premium_features", "Name = 'HWLP1'").Contains("0"))
            {
                return;
            }

            checkAndAddDB(orb, "INSERT INTO premium_features(Name, Enabled) VALUES ('HWLP1', '1')");
            updateOutputLog(orb, "Adding Health And Wellness Pack 1");
            //Warm Light
            addLightingNode(orb, "Warm Light", "1", 255, 255, 255, 0);
            addLightingNode(orb, "Warm Light", "1", 255, 200, 180, 10);
            addLightingNode(orb, "Warm Light", "1", 255, 158, 90, 150);
            addLightingNode(orb, "Warm Light", "1", 120, 80, 40, 270);
            addLightingNode(orb, "Warm Light", "1", 40, 40, 40, 280);
            addLightingNode(orb, "Warm Light", "1", 0, 0, 0, 300);

            addLightingNode(orb, "Warm Light", "2", 0, 0, 0, 0);
            addLightingNode(orb, "Warm Light", "2", 60, 40, 20, 20);
            addLightingNode(orb, "Warm Light", "2", 255, 158, 90, 150);
            addLightingNode(orb, "Warm Light", "2", 255, 200, 180, 220);
            addLightingNode(orb, "Warm Light", "2", 255, 255, 255, 300);

            //Warm Light Cont
            addLightingNode(orb, "Warm Light Continuous", "1", 255, 255, 255, 0);
            addLightingNode(orb, "Warm Light Continuous", "1", 255, 200, 180, 10);
            addLightingNode(orb, "Warm Light Continuous", "1", 255, 158, 90, 150);
            addLightingNode(orb, "Warm Light Continuous", "1", 255, 158, 90, 300);

            addLightingNode(orb, "Warm Light Continuous", "2", 255, 158, 90, 0);
            addLightingNode(orb, "Warm Light Continuous", "2", 255, 158, 90, 150);
            addLightingNode(orb, "Warm Light Continuous", "2", 255, 200, 180, 220);
            addLightingNode(orb, "Warm Light Continuous", "2", 255, 255, 255, 300);

            //Energise
            addLightingNode(orb, "Energise", "1", 255, 255, 255, 0);
            addLightingNode(orb, "Energise", "1", 0, 255, 0, 10);
            addLightingNode(orb, "Energise", "1", 0, 204, 0, 100);
            addLightingNode(orb, "Energise", "1", 128, 128, 0, 120);
            addLightingNode(orb, "Energise", "1", 102, 102, 0, 160);
            addLightingNode(orb, "Energise", "1", 128, 0, 0, 180);
            addLightingNode(orb, "Energise", "1", 40, 0, 0, 280);
            addLightingNode(orb, "Energise", "1", 0, 0, 0, 300);

            addLightingNode(orb, "Energise", "2", 0, 0, 0, 0);
            addLightingNode(orb, "Energise", "2", 0, 39, 0, 20);
            addLightingNode(orb, "Energise", "2", 0, 153, 0, 100);
            addLightingNode(orb, "Energise", "2", 0, 0, 178, 120);
            addLightingNode(orb, "Energise", "2", 0, 0, 255, 290);
            addLightingNode(orb, "Energise", "2", 255, 255, 255, 300);
            

            //Skin Booster
            addLightingNode(orb, "Skin Booster", "1", 255, 255, 255, 0);
            addLightingNode(orb, "Skin Booster", "1", 255, 90, 0, 10);
            addLightingNode(orb, "Skin Booster", "1", 128, 45, 0, 180);
            addLightingNode(orb, "Skin Booster", "1", 255, 0, 0, 210);
            addLightingNode(orb, "Skin Booster", "1", 255, 0, 0, 540);
            addLightingNode(orb, "Skin Booster", "1", 0, 0, 0, 600);

            addLightingNode(orb, "Skin Booster", "2", 0, 0, 0, 0);
            addLightingNode(orb, "Skin Booster", "2", 255, 0, 0, 60);
            addLightingNode(orb, "Skin Booster", "2", 255, 0, 0, 400);
            addLightingNode(orb, "Skin Booster", "2", 255, 90, 0, 420);
            addLightingNode(orb, "Skin Booster", "2", 255, 90, 0, 580);
            addLightingNode(orb, "Skin Booster", "2", 255, 255, 255, 600);
            
            //Mood Booster
            addLightingNode(orb, "Mood Elevator", "1", 255, 255, 255, 0);
            addLightingNode(orb, "Mood Elevator", "1", 0, 0, 255, 10);
            addLightingNode(orb, "Mood Elevator", "1", 0, 0, 166, 220);
            addLightingNode(orb, "Mood Elevator", "1", 64, 0, 128, 240);
            addLightingNode(orb, "Mood Elevator", "1", 40, 0, 40, 280);
            addLightingNode(orb, "Mood Elevator", "1", 0, 0, 0, 300);

            addLightingNode(orb, "Mood Elevator", "2", 0, 0, 0, 0);
            addLightingNode(orb, "Mood Elevator", "2", 27, 0, 51, 20);
            addLightingNode(orb, "Mood Elevator", "2", 64, 0, 128, 220);
            addLightingNode(orb, "Mood Elevator", "2", 0, 0, 179, 240);
            addLightingNode(orb, "Mood Elevator", "2", 0, 0, 255, 290);
            addLightingNode(orb, "Mood Elevator", "2", 255, 255, 255, 300);

            //Pain Reliever
            addLightingNode(orb, "Pain Reliever", "1", 255, 255, 255, 0);
            addLightingNode(orb, "Pain Reliever", "1", 255, 210, 10, 10);
            addLightingNode(orb, "Pain Reliever", "1", 128, 105, 5, 60);
            addLightingNode(orb, "Pain Reliever", "1", 0, 128, 0, 80);
            addLightingNode(orb, "Pain Reliever", "1", 0, 128, 0, 540);
            addLightingNode(orb, "Pain Reliever", "1", 0, 0, 0, 600);

            addLightingNode(orb, "Pain Reliever", "2", 0, 0, 0, 0);
            addLightingNode(orb, "Pain Reliever", "2", 0, 128, 0, 60);
            addLightingNode(orb, "Pain Reliever", "2", 0, 128, 0, 480);
            addLightingNode(orb, "Pain Reliever", "2", 128, 105, 5, 520);
            addLightingNode(orb, "Pain Reliever", "2", 255, 210, 10, 580);
            addLightingNode(orb, "Pain Reliever", "2", 255, 255, 255, 600);
            
            updateOutputLog(orb, "Health And Wellness Pack 1 Sucessfully Added");

        }


        public string OutputLog
        {
            set
            {
                if (outputLog != value)
                {
                    outputLog = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("OutputLog"));
                    }
                }
            }
            get
            {
                return outputLog;
            }
        }
        public string PodUpdateList
        {
            set
            {
                if (podUpdateList != value)
                {
                    podUpdateList = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("PodUpdateList"));
                    }
                }
            }
            get
            {
                return podUpdateList;
            }
        }
        public string ExtraPassword
        {
            set
            {
                if (extraPassword != value)
                {
                    extraPassword = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("ExtraPassword"));
                    }
                }
            }
            get
            {
                return extraPassword;
            }
        }

        public bool ControlsAvailable
        {
            set
            {
                if (controlsAvailable != value)
                {
                    controlsAvailable = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("ControlsAvailable"));
                    }
                }
            }
            get
            {
                return controlsAvailable;
            }
        }

        public string Key
        {
            set
            {
                if (key != value)
                {
                    key = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Key"));
                    }
                }
            }
            get
            {
                return key;
            }
        }

    }
}
