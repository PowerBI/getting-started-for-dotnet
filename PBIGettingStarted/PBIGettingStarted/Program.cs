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
using System.Data.SqlClient;
using System.Data;


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
        private static string clientID = "{ClientID}";

        //Step 2 - Replace redirectUri with the uri you used when you registered your app
        private static string redirectUri = "https://login.live.com/oauth20_desktop.srf";
        
        //Power BI resource uri
        private static string resourceUri = "https://analysis.windows.net/powerbi/api";             
        //OAuth2 authority
        private static string authority = "https://login.windows.net/common/oauth2/authorize";
        //Uri for Power BI datasets
        private static string datasetsUri = "https://api.powerbi.com/beta/myorg/datasets";

        private static AuthenticationContext authContext = null;
        private static string token = String.Empty;


        //.NET Class Example:
        private static string datasetName = "SalesMarketing";
        private static string tableName = "Product";

        //SQL Server Examples
        //private static string datasetName = "SQL_ProductList";
        //private static string tableName = "jsonProduct";

        //private static string datasetName = "SQL_vCompanySales";
        //private static string tableName = "vCompanySales";

        //private static string datasetName = "SQL_vWorkOrderRouting";
        //private static string tableName = "vWorkOrderRouting";

        static void Main(string[] args)
        {
            // Test the connection and update the datasetsUri in case of redirect
            datasetsUri = TestConnection();

            CreateDataset();
            //CreateFromSqlSchema();

            List<Object> datasets = GetAllDatasets();

            foreach (Dictionary<string, object> obj in datasets)
            {
                Console.WriteLine(String.Format("id: {0} Name: {1}", obj["id"], obj["name"]));
            }

            // initiate pushing of rows to Power BI
            Console.WriteLine("Press the Enter key to push rows into Power BI:");
            Console.ReadLine();
            AddClassRows();

            //Optional to test clear rows from a table
            //ClearRows();

            //optional if using SQL Azure
            //AddSQLRows();

            // Finished pushing rows to Power BI, close the console window
            Console.WriteLine("Data pushed to Power BI. Press the Enter key to close this window:");
            Console.ReadLine();

        }

        /// <summary>
        /// Create a Power BI schema from a SQL View.
        /// </summary>
        static void CreateFromSqlSchema()
        {
            SqlConnectionStringBuilder connStringBuilder = new SqlConnectionStringBuilder();
            connStringBuilder.IntegratedSecurity = true;
            connStringBuilder.DataSource = @".";
            connStringBuilder.InitialCatalog = "AdventureWorks2012";

            using (SqlConnection conn = new SqlConnection(connStringBuilder.ConnectionString))
            {
                conn.Open();
          
                //In a production application, use more specific exception handling.           
                try
                {
                    //Create a POST web request to list all datasets
                    HttpWebRequest request = DatasetRequest(datasetsUri, "POST", AccessToken);

                    var datasets = GetAllDatasets().Datasets(datasetName);

                    if (datasets.Count() == 0)
                    {
                        //POST request using the json schema from Product
                        Console.WriteLine(PostRequest(request, conn.ToJsonSchema(datasetName, tableName)));

                    }
                    else
                    {
                        Console.WriteLine("Dataset exists");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                } 
            }
        }

        private static string TestConnection()
        {
            // Check the connection for redirects
            HttpWebRequest request = System.Net.WebRequest.Create(datasetsUri) as System.Net.HttpWebRequest;
            request.KeepAlive = true;
            request.Method = "GET";
            request.ContentLength = 0;
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", String.Format("Bearer {0}", AccessToken));
            request.AllowAutoRedirect = false;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.TemporaryRedirect)
            {
                return response.Headers["Location"];
            }
            return datasetsUri;

        }

        static void AddSQLRows()
        {
            //string sqlNamespace = "Sales";
            //string sqlNamespace = "Production";

            string sqlNamespace = "dbo";
            SqlConnectionStringBuilder connStringBuilder = new SqlConnectionStringBuilder();
            connStringBuilder.IntegratedSecurity = true;

            //ConnectionBuilderActivity
            connStringBuilder.DataSource = @".";
            connStringBuilder.InitialCatalog = "AdventureWorks2012";

            using (SqlConnection connection = new SqlConnection(connStringBuilder.ToString()))
            {
                //Next Iteration: Azure SQL DB - Reliable Connection
                using (SqlCommand command = connection.CreateCommand())
                {
                    connection.Open();

                    command.CommandText = String.Format("SELECT {0} FROM {1}.{2}", "*", sqlNamespace, tableName);
                   
                    //Next Iteration: Show ExecuteReaderAsync
                    string json = command.ExecuteReader().ToJson();

                    //Get dataset id from a table name
                    string datasetId = GetAllDatasets().Datasets(datasetName).First()["id"].ToString();

                    //In a production application, use more specific exception handling. 
                    try
                    {
                        HttpWebRequest request = DatasetRequest(String.Format("{0}/{1}/tables/{2}/rows", datasetsUri, datasetId, tableName), "POST", AccessToken);
                        //POST request using the json from a list of Product
                        Console.WriteLine(PostRequest(request, json));

                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    } 
                }
            }
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
            //In a production application, use more specific exception handling.           
            try
            {               
                //Create a POST web request to list all datasets
                HttpWebRequest request = DatasetRequest(datasetsUri, "POST", AccessToken);

                var datasets = GetAllDatasets().Datasets(datasetName);

                if (datasets.Count() == 0)
                { 
                    //POST request using the json schema from Product
                    Console.WriteLine(PostRequest(request, new Product().ToJsonSchema(datasetName)));
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

        static void AddClassRows()
        {
            //Get dataset id from a table name
            string datasetId = GetAllDatasets().Datasets(datasetName).First()["id"].ToString();

            //In a production application, use more specific exception handling. 
            try
            {
                HttpWebRequest request = DatasetRequest(String.Format("{0}/{1}/tables/{2}/rows", datasetsUri, datasetId, tableName), "POST", AccessToken);

                //Create a list of Product
                List<Product> products = new List<Product>
                {
                    new Product{ProductID = 1, Name="Adjustable Race", Category="Components", IsCompete = true, ManufacturedOn = new DateTime(2014, 7, 30)},
                    new Product{ProductID = 1, Name="LL Crankarm", Category="Components", IsCompete = true, ManufacturedOn = new DateTime(2014, 7, 30)},
                    new Product{ProductID = 1, Name="HL Mountain Frame - Silver", Category="Bikes", IsCompete = true, ManufacturedOn = new DateTime(2014, 7, 30)},
                };

                //POST request using the json from a list of Product
                Console.WriteLine(PostRequest(request, products.ToJson(JavaScriptConverter<Product>.GetSerializer())));

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            } 
        }

        static void ClearRows()
        {
            //Get dataset id from a table name
            string datasetId = GetAllDatasets().Datasets(datasetName).First()["id"].ToString();

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

        private static string PostRequest(HttpWebRequest request, string json)
        {
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(json);
            request.ContentLength = byteArray.Length;

            //Write JSON byte[] into a Stream
            using (Stream writer = request.GetRequestStream())
            {
                writer.Write(byteArray, 0, byteArray.Length);
            }
            return GetResponse(request);
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
