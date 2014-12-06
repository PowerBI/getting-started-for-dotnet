//Copyright Microsoft 2014

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.IO;
using System.Linq;

using PowerBIExtensionMethods;
using System.Web.Script.Serialization;


namespace PBIGettingStarted
{

    //Sample to show how to use the Power BI API
    //  See also, http://docs.powerbi.apiary.io/reference

    //To run this sample

    //See How to register an app (http://go.microsoft.com/fwlink/?LinkId=519361)

    //Step 1 - Replace clientID with your client app ID. To learn how to get a client app ID, see How to register an app (http://go.microsoft.com/fwlink/?LinkId=519361)
    //Step 2 - Replace redirectUri with your redirect uri.

    class Program
    {
        
        //Step 1 - Replace client app ID 
        private static string clientID = "07054a93-dc5b-42c9-b2c9-fc26364150e5";        
        
        //Step 2 - Replace redirectUri
        private static string redirectUri = "https://powerbi.com";

        
        //Power BI resource uri
        private static string resourceUri = "https://analysis.windows.net/powerbi/api";             
        //OAuth2 authority
        private static string authority = "https://login.windows.net/common/oauth2/authorize";
        //Uri for Power BI datasets
        private static string datasetsUri = "https://api.powerbi.com/beta/myorg/datasets";

        private static AuthenticationContext authContext = null;
        private static string token = String.Empty;

        static void Main(string[] args)
        {
            //CreateDataset();

            List<Object> datasets = GetAllDatasets();

            foreach (Dictionary<string, object> obj in datasets)
            {
                Console.WriteLine(String.Format("id: {0} Name: {1}", obj["id"], obj["name"]));
            }

            //AddRows();

            Console.ReadLine();
        }

        static string AccessToken
        {
            get
            {               
                if (token == String.Empty)
                {
                    authContext = new AuthenticationContext(authority);
                    token = authContext.AcquireToken(resourceUri, clientID, new Uri(redirectUri)).AccessToken.ToString();
                    
                    return token;
                }
                else
                {
                    return token;
                }
            }
        }

        static List<Object> GetAllDatasets()
        {
            List<Object> datasets = null;

            //In a production application, use more specific exception handling.
            try
            {
                //Create a GET web request to list all datasets
                HttpWebRequest request = DatasetRequest(datasetsUri, "GET", AccessToken);

                //Get HttpWebResponse from GET request
                string responseContent = GetResponse(request);

                //Get list from response
                datasets = responseContent.ToObject<List<Object>>();

            }
            catch (Exception ex)
            {               
                //In a production application, handle exception
            }

            return datasets;

        }

        static void CreateDataset()
        {
            string name = "TestDataset";

            //In a production application, use more specific exception handling.           
            try
            {               
                //Create a POST web request to list all datasets
                HttpWebRequest request = DatasetRequest(datasetsUri, "POST", AccessToken);

                var datasets = GetAllDatasets().Datasets(name);

                if (datasets.Count() == 0)
                { 
                    //POST request using the json schema from Music
                    PostRequest(request, new Music().ToJsonSchema(name));
                
                    //Get HttpWebResponse from POST request
                    Console.WriteLine(GetResponse(request));

                }
                else
                {
                    Console.WriteLine("Dataset exists");
                }
            }
            catch(Exception ex)
            {

                Console.WriteLine(ex.Message);
            } 
        }

        static void AddRows()
        {
            string tableName = "Music";
            string datasetName = "TestDataset";

            //Get dataset id from a table name
            string datasetId = GetAllDatasets().Datasets(datasetName).First()["id"].ToString();

            //In a production application, use more specific exception handling. 
            try
            {
                HttpWebRequest request = DatasetRequest(String.Format("{0}/{1}/tables/{2}/rows", datasetsUri, datasetId, tableName), "POST", AccessToken);
                
                //Create a list of Music
                List<Music> musicHabits = new List<Music>
                {
                    new Music{Artist = "Jimi Hendrix", Song = "Purple Haze", Genre = "Rock", Location = "Seattle, WA", EventDate = new DateTime(2014, 7, 30)},
                    new Music{Artist = "U2", Song = "Beautiful Day", Genre = "Rock", Location = "Portland, OR", EventDate = new DateTime(2014, 8, 25)},
                    new Music{Artist = "Red Hot Chili Peppers", Song = "Californication", Genre = "Rock", Location = "Portland, OR", EventDate = new DateTime(2014, 9, 25)}
                };

                //POST request using the json from a list of Music
                PostRequest(request, musicHabits.ToJson(JavaScriptConverter<Music>.GetSerializer()));

                //Get HttpWebResponse from POST request
                Console.WriteLine(GetResponse(request));
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            } 
        }

        static void ClearRows()
        {
            string tableName = "MusicHabits";

            //Get dataset id from a table name
            string datasetId = GetAllDatasets().Datasets(tableName).First()["id"].ToString();

            //In a production application, use more specific exception handling. 
            try
            {
                //Create a DELETE web request
                HttpWebRequest request = DatasetRequest(String.Format("{0}/{1}/tables/{2}/rows", datasetsUri, datasetId, tableName), "DELETE", AccessToken);
                request.ContentLength = 0;

                Console.WriteLine(GetResponse(request));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            } 
        }

        private static void PostRequest(HttpWebRequest request, string json)
        {
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(json);
            request.ContentLength = byteArray.Length;

            //Write JSON byte[] into a Stream
            using (Stream writer = request.GetRequestStream())
            {
                writer.Write(byteArray, 0, byteArray.Length);
            }
        }

        private static string GetResponse(HttpWebRequest request)
        {
            string response = string.Empty;

            using (HttpWebResponse httpResponse = request.GetResponse() as System.Net.HttpWebResponse)
            {
                //Get StreamReader that holds the response stream
                using (StreamReader reader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                {
                    response = reader.ReadToEnd();                 
                }
            }

            return response;
        }

        private static HttpWebRequest DatasetRequest(string datasetsUri, string method, string authorizationToken)
        {
            HttpWebRequest request = System.Net.WebRequest.Create(datasetsUri) as System.Net.HttpWebRequest;
            request.KeepAlive = true;
            request.Method = method;
            request.ContentLength = 0;
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", String.Format( "Bearer {0}", authorizationToken));

            return request;
        }
    }
}
