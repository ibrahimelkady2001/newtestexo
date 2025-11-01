using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;


namespace EXOApp
{   
    /// <summary>
    /// Other useful functions
    /// </summary>
    static class Utility
    {
        /// <summary>
        /// Used to linear interpolate between numbers. No longer used
        /// </summary>
        public static float Lerp(float start, float end, float elapsedTime, float maxTime, out bool done, bool clamp = true)
        {
            if(clamp)
            {
                if(elapsedTime > maxTime)
                {
                    done = true;
                    return end;
                }
                else if(elapsedTime < -maxTime)
                {
                    done = true;
                    return start;
                }
            }
            done = false;
            return start + (end - start) * (elapsedTime/maxTime);
        }

        /// <summary>
        /// Clamps a value to its min or max if it exceeds either
        /// </summary>
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        /// <summary>
        /// Gets the Control PCs local IP Address
        /// </summary>
        public static string getLocalIPv4()
        {
            string output = null;
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                //item.NetworkInterfaceType == _type && 
                if (item.OperationalStatus == OperationalStatus.Up)
                {
                    IPInterfaceProperties adapterProperties = item.GetIPProperties();
                    if (adapterProperties.GatewayAddresses.FirstOrDefault() != null)
                    {
                        foreach (UnicastIPAddressInformation ip in adapterProperties.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                output = ip.Address.ToString();
                            }
                        }
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// Gets the first 3 Segments of an IP Address
        /// </summary>
        public static string getIPv4SubnetBase()
        {
            // Returns the first 3 segments of IP address i.e. X.X.X
//#if DEBUG
 //           return "192.168.100";
//#endif


            string ipAddr = getLocalIPv4();
            if (ipAddr != null)
            {
                int lastDot = ipAddr.LastIndexOf('.');
                if (lastDot >= 0)
                {
                    return ipAddr.Substring(0, lastDot);
                }
            }
            // Meh, nothing doing.
            return null;
        }

        /// <summary>
        /// Gets the hostname from the Ip address
        /// </summary>
        public static string getHostNameFromIPAddress(string ipAddress)
        {
            try
            {
                System.Net.IPHostEntry entry = System.Net.Dns.GetHostEntry(ipAddress);
                if (entry != null)
                {
                    return entry.HostName;
                }
            }
            catch (Exception ex)
            {
                // Deliberately empty
                if (ex != null)
                {
                    // Deliberately empty - avoids unused warning
                }
            }
            return null;
        }
    }

}
