using EXOApp.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EXOApp
{
    /// <summary>
    /// Global Functions. Handles the list of Pods
    /// </summary>
    static class Globals
    {

        // Internal members
        private static string userID_;
        private static string psw_;
        private static int userSecurityLevel_ = 0;
        private static Models.PodModel currentPod = null;
        public static bool demoMode = false;
        public static bool assistAlarm{get; set;}
        // The Pods we've found or know about
        private static List<Models.PodModel> podModels = new List<Models.PodModel>();



        private static bool premium = true;

        /// <summary>
        /// Geneates the Url of the Pod
        /// </summary>
        public static string getPodBaseIP(string ipAddr)
        {
            return "http://" + ipAddr + "/";
        }

        /// <summary>
        /// Returns whether the Pod has premium access. Currently unused
        /// </summary>
        public static bool getPremium()
        {
            return premium;
        }

        /// <summary>
        /// Gets the login User ID
        /// </summary>
        public static string getUserID()
        {
            return userID_;
        }

        /// <summary>
        /// Gets the login password
        /// </summary>
        public static string getPassword()
        {
            return psw_;
        }

        /// <summary>
        /// Gets the User Security Level. Admin = 99, Plus = 49, Settings = 39, Staff = 29, Guest = 0
        /// </summary>
        public static int getUserSecurityLevel()
        {
            return userSecurityLevel_;
        }

        /// <summary>
        /// Sets the user details
        /// </summary>
        public static void setUserDetails(string userID, string psw, int securityLevel)
        {
            userID_ = userID;
            psw_ = psw;
            userSecurityLevel_ = securityLevel;
        }

        /// <summary>
        /// Adds a pod to the list of pods
        /// </summary>
        public static void addOrbitToList(Models.PodModel pod)
        {
            podModels.Add(pod);
        }

        public static Models.PodModel getOrbitByIndex(int index)
        {
            if(podModels.Count == 0)
            {
                return null;
            }
            else
            {
                return podModels[index];
            }
        }

        /// <summary>
        /// Gets a specific Pod by it's id
        /// </summary>
        public static Models.PodModel getOrbitById(int index)
        {
            if (podModels.Count == 0)
            {
                return null;
            }
            else
            {
                return podModels.Find(x => x.podNumber == index);
            }
        }

        /// <summary>
        /// Returns the entire list of Pods
        /// </summary>
        public static List<Models.PodModel> getList()
        {
            return podModels;
        }

        /// <summary>
        /// Clears the list of Pods. Used when logging off
        /// </summary>
        public static void clearList()
        {
            podModels.Clear();
        }

        /// <summary>
        /// Gets the current selected Pod for the related menu
        /// </summary>
        public static Models.PodModel getCurrentPod()
        {
            return currentPod;
        }
        
        /// <summary>
        /// Sets the current Pod to a new Pod. Used when selecting a pod specific menu
        /// </summary>
        public static void setCurrentPod(Models.PodModel newPod)
        {
            currentPod = newPod;
        }

        /// <summary>
        /// Sorts the list by number
        /// </summary>
        public static void sortOrbitList()
        {
            podModels.Sort();
        }

        /// <summary>
        /// Quits the Program. Restarts the program when used in Kiosk
        /// </summary>
        public static void Quit()
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();

            //Environment.Exit(0); //maybe change later
        }
        // Colours
 

        // Debugging
        public static void outputDebug(string msg)
        {
            // Generate a timestamp prefix.
            string prefix = DateTime.Now.ToString("[yyyy-mm-dd HH:mm:ss.fff] ");
            Debug.Print(prefix + msg);
            Console.WriteLine(prefix + msg);
        }


    }
}
