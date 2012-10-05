using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.ServiceModel.Web;
using System.Runtime.Serialization.Json;

namespace TentLibrary
{
    public class Functions
    {
        public delegate void HeadRequestCompletedEventHandler(object sender, EventArgs e);
        public delegate void FootRequestCompletedEventHandler(object sender, EventArgs e);

        //[Serializable]
        //public class DatabaseServiceCompletedEventArgs : AsyncCompletedEventArgs

        /// <summary>
        /// Get the Profile URI for a given entity
        /// </summary>
        /// <param name="entity">Entity (user) URI</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>Uri pointing to a Tent profile (e.g. https://tent.johnsmith.me/profile )</returns>
        public static string GetProfile(string entity, int timeout = 1000)
        {
            try
            {
                WebRequest request = HttpWebRequest.Create(entity);

                request.Method = WebRequestMethods.Http.Head;
                request.Timeout = timeout;

                using (WebResponse response = request.GetResponse())
                {
                    string linkHeader = response.Headers.GetValues("Link")[0];

                    return Regex.Match(linkHeader, "[^<](.*?)(?=>)").Value;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get the Server URI for a given entity
        /// </summary>
        /// <param name="profile">Profile URI</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>Uri pointing to a Tent server</returns>
        public static string[] GetServers(string profile, int timeout = 1000)
        {
            try
            {
                WebRequest request = HttpWebRequest.Create(profile);

                request.Method = WebRequestMethods.Http.Get;
                request.Timeout = timeout;

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception(String.Format(
                            "Server error (HTTP {0}: {1}).",
                            response.StatusCode,
                            response.StatusDescription));
                    }
                    else
                    {
                        DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(CoreResponse));

                        object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());

                        CoreResponse jsonResponse = objResponse as CoreResponse;

                        return jsonResponse.Profile.Servers;
                    }
                }                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
