using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;

namespace TentLibrary
{
    public partial class Functions
    {
        #region Synchronous Method
        /// <summary>
        /// http://tent.io/docs/app-auth
        /// 
        /// http://stackoverflow.com/questions/10654383/wcf-post-json-object
        /// 
        /// http://blog.manglar.com/json-post-request/
        /// </summary>
        /// <param name="server"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static string RegisterApplication(string server, RegistrationData registrationData, int timeout = DEFAULT_TIMEOUT)
        {
            try
            {

                #region Request
                HttpWebRequest request = HttpWebRequest.Create(string.Format("{0}/apps", server)) as HttpWebRequest;

                request.Method = WebRequestMethods.Http.Post;
                request.Timeout = timeout;
                request.ContentType = TENT_CONTENT_TYPE;
                request.Accept = TENT_CONTENT_TYPE;

                DataContractJsonSerializer requestJsonSerializer = new DataContractJsonSerializer(typeof(RegistrationData));
                
                StreamWriter writer = new StreamWriter(request.GetRequestStream());
                requestJsonSerializer.WriteObject(writer.BaseStream, registrationData);

                writer.Close();
                #endregion

                #region Response
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
                        DataContractJsonSerializer responseJsonSerializer = new DataContractJsonSerializer(typeof(RegistrationData));

                        RegistrationData jsonResponse = responseJsonSerializer.ReadObject(response.GetResponseStream()) as RegistrationData;





                        // Testing - this whole thing isn't done yet. We're through step one, but now we need to keep slingin shit back and forth
                        // until the OAuth is completed.
                        MemoryStream ms = new MemoryStream();
                        responseJsonSerializer.WriteObject(ms, jsonResponse);
                        ms.Position = 0;
                        StreamReader sr = new StreamReader(ms);
                        return sr.ReadToEnd();
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}