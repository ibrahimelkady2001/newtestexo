using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;



namespace EXOApp
{
    /// <summary>
    /// Handles communication with the Pod SQL Database
    /// </summary>
    class CustomWebRequest
    {
        // Internal members
        bool getRequest = false;
        bool authorityProvided = false;
        HttpWebRequest webRequest = null;
        string url = null;
        string resolvedIP = null;
        /// <summary>
        /// Stores the Pod URL
        /// </summary>
        private CustomWebRequest(string theURL)
        {
            // Store the URL
            url = theURL;
        }

        /// <summary>
        /// Creates a post web request
        /// </summary>
        public static CustomWebRequest pPost(String pURL)
        {
            CustomWebRequest pResult = new CustomWebRequest(pURL);
            pResult.getRequest = false;
            pResult.createHTTP(pURL, WebRequestMethods.Http.Post);
            return pResult;
        }

        /// <summary>
        /// Creates a get web request
        /// </summary>
        public static CustomWebRequest pGet(String pURL)
        {
            CustomWebRequest pResult = new CustomWebRequest(pURL);
            pResult.getRequest = true;
            pResult.createHTTP(pURL, WebRequestMethods.Http.Get);
            return pResult;
        }

        /// <summary>
        /// Gets the HTTP
        /// </summary>
        public HttpWebRequest pGetHTTP()
        {
            return webRequest;
        }

        /// <summary>
        /// Gets the resolved IP Address
        /// </summary>
        public string resolvedIPAddress()
        {
            return resolvedIP;
        }

        /// <summary>
        /// Checks if the request is a Get request
        /// </summary>
        public bool isGetRequest()
        {
            return getRequest;
        }

        /// <summary>
        /// Sets the username and password for accessing the Pod
        /// </summary>
        public void setAuth(string userID, string psw)
        {
            authorityProvided = true;
            string authInfo = userID + ":" + psw;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            webRequest.Headers["Authorization"] = "Basic " + authInfo;
        }

        /// <summary>
        /// Set the web timeout
        /// </summary>
        public void setTimeout(int milliSecs)
        {
            webRequest.Timeout = milliSecs;
        }

        /// <summary>
        /// Attemps to communicate with the apache webserver on the Pod
        /// </summary>
        public String performBlockingRequest(out HttpStatusCode statusCode, byte[] data = null)
        {
            Debug.Assert(webRequest != null);
            statusCode = HttpStatusCode.NoContent;
            if (!authorityProvided && Globals.getUserID() != null)
            {
                setAuth(Globals.getUserID(), Globals.getPassword());
            }
            if (!isGetRequest())
            {
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = data.Length;
                try
                {
                    using (var theRequest = webRequest.GetRequestStream())
                    {
                        theRequest.Write(data, 0, data.Length);
                    }
                }
                catch (Exception e)
                {
                    // Determine the status code
                    if (e is WebException)
                    {
                        WebException we = e as WebException;
                        if (we.Response is HttpWebResponse)
                        {
                            statusCode = (we.Response as HttpWebResponse).StatusCode;
                        }
                    }
                    Debug.WriteLine("Web request failed. URL=" + getBasicURL() + "  Error=" + e.Message);
                }

            }
            try
            {
                using (var theResponse = webRequest.GetResponse())
                {
                    var dataStream = theResponse.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    object objResponse = reader.ReadToEnd();
                    dataStream.Close();
                    theResponse.Close();
                    return objResponse.ToString();
                }
            }
            catch (Exception e)
            {
                // Determine the status code
                if (e is WebException)
                {
                    WebException we = e as WebException;
                    if (we.Response is HttpWebResponse)
                    {
                        statusCode = (we.Response as HttpWebResponse).StatusCode;
                    }
                }
                Debug.WriteLine("Web request failed. URL=" + getBasicURL() + "  Error=" + e.Message);
            }

            return null;
        }

        /// <summary>
        /// Runs specific commands, which can be specified looking at the routes.php on the pod's webserver
        /// </summary>
        public static string manualRequest(string ipAddress, string request, string[] data = null)
        {
            string result = null;
            string userID = Globals.getUserID();
            string psw = Globals.getPassword();
            string url = ipAddress + request;
            CustomWebRequest pWR = null;
            if (data == null)
            {
                pWR = CustomWebRequest.pGet(url);
            }
            else
            {
                pWR = CustomWebRequest.pPost(url);
            }

            pWR.setAuth(userID, psw);
            pWR.setTimeout(2000);
            HttpStatusCode statusCode;
            if (data == null)
            {
                result = pWR.performBlockingRequest(out statusCode);
            }
            else
            {
                string jsonData = JsonConvert.SerializeObject(data);
                result = pWR.performBlockingRequest(out statusCode, Encoding.UTF8.GetBytes(jsonData));
            }

            if (result != null)
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Runs a SQL command on the Pods DB. Use this for most functions involving changing data
        /// </summary>
        public static string generalSQLRequest(string ipAddress, string type, string query)
        {
            string result = null;
            string userID = Globals.getUserID();
            string psw = Globals.getPassword();
            string url = ipAddress + "generalSQLQuery";
            CustomWebRequest pWR = CustomWebRequest.pPost(url);
            pWR.setAuth(userID, psw);
            pWR.setTimeout(2000);
            HttpStatusCode statusCode;
            string[] array = new string[3] { "2r5u8x/A?D(G-KaPdSgVkYp3s6v9yB&E)H@MbQeThWmZq4t7w!z%C*F-JaNdRfU", type, query }; // example query "SELECT value FROM config WHERE name = 'night_mode_enabled'"|| 2nd example "UPDATE config SET value = 1 WHERE name = 'night_mode_enabled'";
            string jsonData = JsonConvert.SerializeObject(array);
            result = pWR.performBlockingRequest(out statusCode, Encoding.UTF8.GetBytes(jsonData));
            if (result != null)
            {
                return result;
            }
            else
            {
                Debug.WriteLine(query);
                return null;
            }
        }
        //
        // Internal methods
        //
        private void createHTTP(String pURL, String method)
        {
            Debug.Assert(webRequest == null);
            webRequest = (HttpWebRequest)WebRequest.Create(pURL);
            webRequest.Method = method;
            webRequest.Timeout = 3000;
        }
        private string getBasicURL()
        {
            int pos = url.IndexOf('?');
            if (pos >= 0)
            {
                return url.Substring(0, pos);
            }
            return url;
        }

    }
}
