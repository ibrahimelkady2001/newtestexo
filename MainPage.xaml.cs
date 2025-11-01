using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Diagnostics;
using Newtonsoft.Json;
using EXOApp.Helpers;
using System.Windows.Input;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;

namespace EXOApp
{
    /// <summary>
    /// Login page for the Exo app. This page does not use a view model.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        int success = 0;
        int fail = 0;
        int threadCount = 0;
        bool badLogin = false;
        List<int> list = new List<int>();
        bool userDetailsSaved = false;
        int securityLevel = 0;
        bool testMode = false;
        private readonly Helpers.MessageInterface _messageInterface;

        /// <summary>
        /// Contructor for the Mainpage
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            this._messageInterface = DependencyService.Get<Helpers.MessageInterface>();

            //Default login credentials
            userName_Entry.Value = "guest";
            password_Entry.Value = "guest";
#if DEBUG
            //On debug builds, admin is the default login
            userName_Entry.Value = "admin";
            password_Entry.Value = "5ecureOrb1t";
#endif
            //password_Entry.ReturnCommand = new Command(() => returnCommand());
            Globals.assistAlarm = false;
            fast_login.IsChecked = false;
            //Checks if existing record of Pods exist in the program.
            if (!Preferences.ContainsKey("orbit_IP")) 
            {
                fast_login.IsChecked = true;
                fast_login.IsEnabled = false;
            }
            
            //Checks the local IP address
            ipAddress.Text = Utility.getLocalIPv4();
            if(ipAddress.Text == null)
            {
                ipAddress.Text = "Error 1003 - No IP Found, Check your Control PC is plugged in via ethernet";
                ipAddress.TextColor = Colors.Red;
            }

            //Checks if internet connection is available
            var current = Connectivity.NetworkAccess;
            if (current != NetworkAccess.Internet)
            {
                InternetConnection.IsVisible = true;
            }
            
        }

        /// <summary>
        /// Unused
        /// </summary>
        void returnCommand()
        {
            loginButtonClick(null, null);
        }

        /// <summary>
        /// Login button function
        /// </summary>
        async void loginButtonClick(object sender, EventArgs args)
        {
            //Clears the list of existing pods
            Globals.clearList();
            badLogin = false;
            userDetailsSaved = false;

            //Gets the base ip of the control PC
            string baseIPSubnet = Utility.getIPv4SubnetBase();
            if(baseIPSubnet == "" || baseIPSubnet == null)
            {
                await DisplayAlert("Error 1003", "EXO Control PC is disconnected from the wired network.\nPlease reconnect your control PC to your EXO router", "Ok");
                return;
            }
            busyIndicator.IsRunning = true;
            string userName = userName_Entry.Value.ToString();
            string password = password_Entry.Value.ToString();
            Debug.WriteLine(userName);
            Debug.WriteLine(password);
            
            //Disables user input
            login_button.IsEnabled = false;
            QuitButton.IsEnabled = false;
            SupportButton.IsEnabled = false;
            List<Task<int>> taskList = new List<Task<int>>();

            int totalPods = 0;
   
            if(!fast_login.IsChecked.Value)
            {
                //Creates a task using the IPs previously existing pods and checks all IPs simeltaneously.
                foreach (int i in getPodIps())
                {
                    taskList.Add(_checkUserIDAndPsw(userName, password, baseIPSubnet, i));
                }
                while (taskList.Any())
                {
                    updateProgress(taskList.Count());
                    Task<int> finishedTask = await Task.WhenAny(taskList);
                    taskList.Remove(finishedTask);
                    totalPods += await finishedTask;
                }
            }
            else
            {
                //Creates a task for all IPs between 0 and 254 and checks them all simeltaneously.
                list.Clear();
                for (int i = 0; i < 254; i++)
                {
                    taskList.Add(_checkUserIDAndPsw(userName, password, baseIPSubnet, i));
                }
                while (taskList.Any())
                {
                    updateProgress(taskList.Count());
                    Task<int> finishedTask = await Task.WhenAny(taskList);
                    taskList.Remove(finishedTask);
                    totalPods += await finishedTask;
                }
                updateProgress(0);
                if(totalPods != 0)
                {
                    savePodsIps();
                }
            }

            updateProgress(0);

            if (badLogin && totalPods == 0)
            {
                await DisplayAlert("Error 1000", "Please Check Your Username and Password", "OK");
                login_button.IsEnabled = true;
                QuitButton.IsEnabled = true;
                SupportButton.IsEnabled = true;
                busyIndicator.IsRunning = false;
                return;
            }
            else if (totalPods == 0)
            {
                await DisplayAlert("Error 1001", "Please Check Your EXO Systems are Online\nand your control PC is connected via Ethernet (Not wi-fi)", "OK");
                login_button.IsEnabled = true;
                QuitButton.IsEnabled = true;
                SupportButton.IsEnabled = true;
                busyIndicator.IsRunning = false;
                return;

            }

            if (!fast_login.IsChecked.Value && getPodIps().Length > totalPods)
            {

                bool proceed = await DisplayAlert("Error 1002", "Previous EXO systems not detected.\nDo you wish to proceed to the dashboard for the detected Pods?", "Yes", "No");
                if (!proceed)
                {
                    login_button.IsEnabled = true;
                    QuitButton.IsEnabled = true;
                    SupportButton.IsEnabled = true;
                    busyIndicator.IsRunning = false;
                    return;
                }
            }
            Globals.sortOrbitList();
            userDetails();
            fast_login.IsEnabled = true;
            await Navigation.PushAsync(new OverviewPageV2());
            login_button.IsEnabled = true;
            QuitButton.IsEnabled = true;
            SupportButton.IsEnabled = true;
            busyIndicator.IsRunning = false;

            //loginSucess_label.IsVisible = true;
            //await loginSucess_label.FadeTo(1, 2000);
        }

        /// <summary>
        /// Determines the auth value of each login
        /// </summary>
        void userDetails()
        {
            int auth = 0;
            switch(userName_Entry.Value.ToString())
            {
                case "guest":
                    {
                        auth = 0;
                        break;
                    }
                case "staff":
                    {
                        auth = 30;
                        break;
                    }
                case "settings":
                    {
                        auth = 39;
                        break;
                    }
                case "fcentre":
                    {
                        auth = 39;
                        break;
                    }
                case "plus":
                    {
                        auth = 49;
                        break;
                    }
                case "admin":
                    {
                        auth = 99;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            Globals.setUserDetails(userName_Entry.Value.ToString(), password_Entry.Value.ToString(), auth);

        }

        /// <summary>
        /// Saves the current Pod IPs to the program for quick usage later
        /// </summary>
        void savePodsIps()
        {
            Debug.WriteLine("Save Pod IPs Start");
            string podList = "";
            foreach (int i in list)
            {
                podList += i.ToString();
                podList += ",";
            }
            Debug.WriteLine(podList);
            podList = podList.Remove(podList.Length - 1);
            Preferences.Set("orbit_IP", podList);
            Debug.WriteLine("Save Pod IPs End");
        }

        /// <summary>
        /// Gets a list of save PodIps.
        /// </summary>
        int[] getPodIps()
        {
            Debug.WriteLine("Get Pod IPs Start");
            string orbitList = Preferences.Get("orbit_IP", "");
            if(orbitList == "")
            {
                return null;
            }
            string[] orbs = orbitList.Split(',');
            int[] orbits = new int[orbs.Length];
            for(int i = 0; i < orbs.Length; i++)
            {
                Debug.WriteLine(orbs[i]);
                int orbNumber = int.Parse(orbs[i]);
                if(!orbits.Contains(orbNumber))
                {
                    orbits[i] = orbNumber;
                }
            }
            Debug.WriteLine("Get Pod IPs End");
            return orbits;
        }

      



        /// <summary>
        /// Tries to login to a IP address using the supplied username and password.
        /// This will fail if a Pod doesn't exist on the specified IP.
        /// </summary>
        private async Task<int> _checkUserIDAndPsw(string userID, string psw, string baseIPSubnet, int ipSegment)
        {
            bool podFound = false;
            await Task.Run(() =>
            {
                string result = null;
                string ipAddr = baseIPSubnet + "." + ipSegment;
                string baseOrbitURL = Globals.getPodBaseIP(ipAddr);
                string url = baseOrbitURL + "checkUser?userID=" + WebUtility.UrlEncode(userID) + "&psw=" + WebUtility.UrlEncode(psw);
                CustomWebRequest pWR = CustomWebRequest.pGet(url);


                pWR.setAuth(userID, psw);
                pWR.setTimeout(2000);
                HttpStatusCode statusCode;
                result = pWR.performBlockingRequest(out statusCode);
                if (result != null)
                {
                    if(result.Contains("result"))
                    {
                        int podNumber = getOrbitNumber(result);
                        if (podNumber != -1)
                        {
                            podFound = true;
                            success++;
                            list.Add(ipSegment);
                            Globals.addOrbitToList(new Models.PodModel(baseOrbitURL, podNumber));

                        }


                    }
                    else
                    {
                        Debug.WriteLine("Name Fail");
                        fail++;
                    }
                }
                else if(statusCode.ToString() == "Unauthorized")
                {
                    badLogin = true;
                }
                else
                {
                    Debug.WriteLine("Fail");
                    fail++;
                }
                threadCount--;
            });
            
            if(podFound)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Unused
        /// </summary>
        private async Task podsFound()
        {
            await Task.Run(() =>
            {
                if(Globals.getOrbitById(0) != null)
                {


                }
            });
        }

        /// <summary>
        /// Gets the Pod Number
        /// </summary>
        private int getOrbitNumber(string result)
        {
            dynamic json = JsonConvert.DeserializeObject<object>(result);
            string hostname = json["hostname"];
            int parse = 0;
            if (int.TryParse(hostname.Replace("orbit-", ""), out parse))
            {
                return parse;
            }
            else if(int.TryParse(hostname.Replace("exo-", ""), out parse))
            {
                return parse;
            }
            else
            {
                return -1;
            }

        }

        /// <summary>
        /// Quits the app. When in kiosk mode, this will restart the app.
        /// </summary>
        public async void quitButtonClick(object sender, EventArgs args)
        {
            bool response = await _messageInterface.ShowAsyncAcceptCancel("Alert", "Are you sure you want to Quit the App?", "Yes", "No");
            if (response)
            {
                Globals.Quit();
            }
        }
        /// <summary>
        /// Opens the support page
        /// </summary>
        public async void supportButtonClick(object sender, EventArgs args)
        {
            await this.Navigation.PushAsync(new SupportPage());
        }
        /// <summary>
        /// Used to show a progress bar. Currently does nothing.
        /// </summary>
        private void updateProgress(int count)
        {
            //progressBar.Progress = (254 - count) / 254f;
        }
    }
}
