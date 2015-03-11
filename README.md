Getting Started with the Power BI REST API
=

The Power BI getting started sample shows you how to

- [Register a Native Client Application](#registerClientApp)
- [Get an Azure Active Directory access token](#AAD)
- [Get all datasets](#get)
- [Create a dataset](#create)
- [Add rows to a dataset](#add)
- [How to add Active Directory Authentication Library](#addlib)

If this is your first time creating a Power BI app, see [Power BI Quick Start](Power+BI+app+quick+start.md).

Before you run the sample code, register the sample client app in your Azure Active Directory. Power BI apps are integrated with Azure Active Directory (AD) to provide secure sign in and authorization for your app. To integrate a Power BI app with Azure AD, you register the details about your application with Azure AD by using the Azure Management Portal.

To register an app in Azure Active Directory, see [Register an app](Register+an+app.md).
<a name="registerClientApp"></a>
## Register a Native Client Application ##
When you register a native client application, such as a console app, you receive a Client ID.  The Client ID is used by the application to identify themselves to the users that they are requesting permissions from. For a .NET application, you use the Client ID to get an authentication token. For more information about Power BI Authentication, see [Authenticate with Power BI](http://go.microsoft.com/fwlink/?LinkId=519359).

To register an app in Azure Active Directory, see [Register an app](Register+an+app.md).
<a name="getClientID"></a>
### How to get a client app id ###
1. In the **Microsoft Azure portal**, choose **ACTIVE DIRECTORY** in the left pane or **Directory** in the **All Items** list. If you do not have an organizational Azure Active Directory, see [Setup Azure Active Directory](Setup+Azure+Active+Directory.md). 
2. In the **Azure Active Directory** page, choose **APPLICATIONS**.
3. Choose your application.
4. In the **CONFIGURE** page, copy the **CLIENT ID**.

### To run the sample ###

1. Replace **clientID** with your client app ID. To learn how to get a client app ID, see [How to get a client app id](#getClientID).

		private static string clientID = ""; 
<a name="AAD"></a>
### How to get an Azure Active Directory access token ###

1. Add the "Active Directory Authentication Library" NuGet package to your project. To learn how to add Active Directory Authentication Library, see [How to add Active Directory Authentication Library](#addlib).
2. Add a reference to Microsoft.IdentityModel.Clients.ActiveDirectory.
3. Create a new **AuthenticationContext** passing an Authority. An authority knows about the service you want to invoke, and knows how to authenticate you.
4. Get an **Azure Active Directory** token by calling **AcquireToken**.

		//Client app ID. To learn how to get a client app ID, see [How to register an app](Register+an+app.md).
		private static string clientID = ""; 
		
		//Power BI resource uri
		private static string resourceUri = "https://analysis.windows.net/powerbi/api"; 		
		
		//OAuth2 authority
		private static string authority = "https://login.windows.net/common/oauth2/authorize";
		
		//Create a new AuthenticationContext passing an Authority.
		AuthenticationContext authContext = new AuthenticationContext(authority);

		//Get an Azure Active Directory token by calling AcquireToken
    	string token = authContext.AcquireToken(resourceUri, clientID, new Uri(redirectUri)).AccessToken.ToString();

<a name="get"></a>
### How to get all datasets ###

1. Get an access token. See [How to get an Azure Active Directory access token](#AAD).
2. Create an **HttpWebRequest** using a GET method.  The sample uses DatsetRequest(datasetsUri, "GET", AccessToken) to make a request to the service. See [How to make a Power BI request](#request).
3. Get a response from the Power BI service.
	
	
		private static string datasetsUri = "https://api.powerbi.com/beta/myorg/datasets";

  		static List<Object> GetAllDatasets()
  		{
	  		List<Object> datasets = null;
	
	      	//Create a GET web request to list all datasets
	  		HttpWebRequest request = DatsetRequest(datasetsUri, "GET", AccessToken);
	
	      	//Get HttpWebResponse from GET request
	      	string responseContent = GetResponse(request);
	
	      	//Get list from response. ToObject() is a sample Power BI extension method
	      	List<Object> datasets = responseContent.ToObject<List<Object>>();
	
	      	return datasets;
		}


<a name="create"></a>
### How to create a dataset ###
1. Get an access token. See [How to get an Azure Active Directory access token](#AAD).
2. Create an **HttpWebRequest** using a POST method. The sample uses DatsetRequest(datasetsUri, "POST", AccessToken) to make a request to the service. See [How to make a Power BI request](#request). 
3. Get a response from the Power BI service.

		static void CreateDataset()
		{
			string name = "SalesMarketing";
	
			//In a production application, use more specific exception handling.           
			try
			{               
				//Create a POST web request to list all datasets
				HttpWebRequest request = DatsetRequest(datasetsUri, "POST", AccessToken);
				
				//Datasets is a sample Power BI extension method
				var datasets = GetAllDatasets().Datasets(name);
	
				if (datasets.Count() == 0)
				{ 
					//POST request using the json schema from a Product
					//ToJsonSchema is a sample Power BI extension method
					PostRequest(request, new Product().ToJsonSchema(name));
	                
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


<a name="add"></a>
### How to add rows to a dataset ###
1. Get an access token. See How to get an [How to get an Azure Active Directory access token](#AAD).
2. Create an **HttpWebRequest** using a POST method. The sample uses DatsetRequest(datasetsUri, "POST", AccessToken) to make a request to the service. See [How to make a Power BI request](#request). 
3. To identify the dataset to add rows to, the resource uri for a POST method has a dataset id.
3. Get a response from the Power BI service.

		static void AddRows()
    	{
    		string tableName = "SalesMarketing";

        	//Get dataset id from a table name.
			//Datasets is a sample Power BI extension method
        	string datasetId = GetAllDatasets().Datasets(tableName).First()["id"].ToString();

        	//In a production application, use more specific exception handling. 
        	try
        	{
            	HttpWebRequest request = DatsetRequest(String.Format("{0}/{1}/tables/{2}/rows", datasetsUri, datasetId, tableName), "POST", AccessToken);

                //Create a list of Product
                List<Product> products = new List<Product>
                {
                    new Product{ProductID = 1, Name="Adjustable Race", Category="Components", IsCompete = true, ManufacturedOn = new DateTime(2014, 7, 30)},
                    new Product{ProductID = 1, Name="LL Crankarm", Category="Components", IsCompete = true, ManufacturedOn = new DateTime(2014, 7, 30)},
                    new Product{ProductID = 1, Name="HL Mountain Frame - Silver", Category="Bikes", IsCompete = true, ManufacturedOn = new DateTime(2014, 7, 30)},
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

<a name="request"></a>
### How to make a Power BI request ###
To make a GET or POST request on the Power BI service:
- Create an **HttpWebRequest** from a dataset resource url. 
- Set **Method** to GET or POST. GET returns JSON objects, such as a dataset, from the service. POST is similar to saving data, it POST's JSON objects to the service.
- Add an Authorization header to an **HttpWebRequest** object.

		private static string datasetsUri = "https://api.powerbi.com/beta/myorg/datasets";

        private static HttpWebRequest DatsetRequest(string datasetsUri, string method, string authorizationToken)
        {
            HttpWebRequest request = System.Net.WebRequest.Create(datasetsUri) as System.Net.HttpWebRequest;
            request.KeepAlive = true;
            request.Method = method;
            request.ContentLength = 0;
            request.ContentType = "application/json";
			
			//Add an Authorization header to an HttpWebRequest object
            request.Headers.Add("Authorization", String.Format( "Bearer {0}", authorizationToken));

            return request;
        }

### How to get a JSON response from the service ###
To GET a JSON response from an **HttpWebRequest** request, call request.**GetResponse()** and read the response into a **StreamReader**.

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

### How to POST JSON to the service ###
To POST JSON to the service from an **HttpWebRequest** request and json string, write a json byte[] array into a **Stream**.

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


###  Power BI sample extension methods ###
Power BI uses JSON to transmit data objects between a client and the Power BI service. JSON is a text string that represents a collection of objects. The Power BI Getting Started Sample uses several extension methods that make using JSON with Power BI API really easy. To learn more about the Power BI sample extension methods, see JSONBuilder.cs in the sample source code. For more information about Power BI JSON, see [Introduction to Power BI Data Push (Preview](Introduction+to+Power+BI+REST+API+(Preview).md).

object.ToJsonSchema(string) - Returns Power BI JSON schema from a .NET class.
    
	public class jsonExample
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }

	string json = new jsonExample().ToJsonSchema("JSON Example");

	{"name": "JSON Example","tables": 
		[{"name": "jsonExample", "columns": 
			[{ "name": "Name", "dataType": "string"},{ "name": "Date", "dataType": "DateTime"}]}]}


List&lt;T&gt;.ToJson(JavaScriptSerializer&lt;T&gt;) - Returns Power BI JSON rows from a List&lt;T&gt;.

    List<jsonExample> jsonExamples = new List<jsonExample>
    {
        new jsonExample{Name = "Name1", Date = DateTime.Now},
        new jsonExample{Name = "Name2", Date = DateTime.Parse("12/4/2014")},
        new jsonExample{Name = "Name3", Date = DateTime.Parse("9/14/2015")}
    };

    string json = jsonExamples.ToJson(JavaScriptConverter<jsonExample>.GetSerializer());

	"{"rows":[
		{"Name":"Name1","Date":"12/04/2014"},
		{"Name":"Name2","Date":"12/04/2014"},
		{"Name":"Name3","Date":"09/14/2015"}]}"


string.ToObject&lt;List&lt;Object&gt;&gt;() - Gets a List&lt;Object&gt; from a JSON string

	List<Object> datasets = responseJson.ToObject<List<Object>>();

Datasets(string) - Gets an IEnumerable&lt;Dictionary&lt;string, object&gt;&gt; from a dataset name.

	string datasetId = GetAllDatasets().Datasets(tableName).First()["id"].ToString();

For more information about JSON, see [Introducing JSON](http://json.org/). 
<a name="addlib"></a>
## How to add Active Directory Authentication Library  ##
You can install the **Active Directory Authentication Library** NuGet package from Visual Studio. When you install a NuGet package, Visual Studio creates a reference to the required assemblies.

1.	Right click a solution.
2.	Choose **Manage NuGet Packages**.
3.	Search for **Active Directory Authentication Library**.
4.	Choose **Active Directory Authentication Library** in the list of packages, and click **Install**.
