Getting Started with the Power BI REST API
=
Before you run the sample code, register a client app in your Azure Active Directory.

## Register a Native Client Application ##
When you register a native client application, such as a console app, you receive a Client ID.  The Client ID is used by the application to identify themselves to the users that they are requesting permissions from. For a .NET application, you use the Client ID to get an authentication token. For more information about Power BI Authentication, see {TODO: link}

### Register a client app ###
1. Sign into your Microsoft Azure subscription.
2. In the left service panel, choose **ACTIVE DIRECTORY**.
3. Click any active directory.
4. Click **APPLICATIONS**.
5. Click **ADD**.
6. In the **Tell us about your application** page, enter a **NAME** such as "My Power BI app", and choose **NATIVE CLIENT APPLICATION** for the type, and click **Next** icon.
7. In the **Application information** page, enter a **REDIRECT URI** such as "https://powerbi.com". The URI to which Windows Azure AD will redirect in response to an OAuth 2.0 request. The value does not need to be a physical endpoint, but must be a valid URI. For client apps, you can use the generic URL of https://login.live.com/oauth20_desktop.srf.
8. Click the **Complete** icon.
9. In the application page, choose **CONFIGURE**. You will see your client id.
10. Add permissions to Power BI at the bottom of the page. For more information about Power BI permissions, see {Power BI Permissions}

TODO: {Can't see Power BI Service listed, How to get a Power BI subscription }

For more information about how to register an app in Azure Active Directory, see http://go.microsoft.com/fwlink/?LinkId=519361.

### How to get a client app id ###
1. In the Microsoft Azure portal, choose an **ACTIVE DIRECTORY** application.
2. In the application page, choose **CONFIGURE**. After you register a new app, the application page is displayed.
3. In the **CONFIGURE** page, copy the **CLIENT ID**. On the **CONFIGURE** page, you can also get the **REDIRECT URI**.

### To run the sample ###

1. Replace **clientID** with your client app ID.

		private static string clientID = "{ClientID}"; 

2. Replace **redirectUri** with the redirect uri used when you registered your app.

    	private static string redirectUri = "https://powerbi.com";

### How to get an Azure Active Directory access token ###

1. Add the “Active Directory Authentication Library” NuGet package to your project. To learn how to add Active Directory Authentication Library, see Authenticate with Power BI.
2. Add a reference to Microsoft.IdentityModel.Clients.ActiveDirectory.
3. Create a new **AuthenticationContext** passing an Authority. An authority  knows about the service you want to invoke, and knows how to authenticate you.
4. Get an **Azure Active Directory** token by calling **AcquireToken**.

		//OAuth2 authority
		private static string authority = "https://login.windows.net/common/oauth2/authorize";

		//Power BI resource uri
		private static string resourceUri = "https://analysis.windows.net/powerbi/api"; 
		
		//Client app ID 
		private static string clientID = "{Client ID}"; 
		
		//Create a new AuthenticationContext passing an Authority.
		AuthenticationContext authContext = new AuthenticationContext(authority);

		//Get an Azure Active Directory token by calling AcquireToken
    	string token = authContext.AcquireToken(resourceUri, clientID, new Uri(redirectUri)).AccessToken.ToString();


### How to get all datasets ###

1. Get an access token. See How to get an Azure Active Directory access token.
2. Create an **HttpWebRequest** using a GET method.  The sample uses DatsetRequest(datasetsUri, "GET", AccessToken) to make a request to the service. See How to make a Power BI request.
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

### How to create a dataset ###
1. Get an access token. See How to get an Azure Active Directory access token.
2. Create an **HttpWebRequest** using a POST method. The sample uses DatsetRequest(datasetsUri, "POST", AccessToken) to make a request to the service. See How to make a Power BI request. 
3. Get a response from the Power BI service.

		static void CreateDataset()
		{
			string name = "MusicHabits";
	
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


### How to add rows to a dataset ###
1. Get an access token. See How to get an Azure Active Directory access token.
2. Create an **HttpWebRequest** using a POST method. The sample uses DatsetRequest(datasetsUri, "POST", AccessToken) to make a request to the service. See How to make a Power BI request. 
3. To identify the dataset to add rows to, the resource uri for a POST method has a dataset id.
3. Get a response from the Power BI service.

		static void AddRows()
    	{
    		string tableName = "MusicHabits";

        	//Get dataset id from a table name.
			//Datasets is a sample Power BI extension method
        	string datasetId = GetAllDatasets().Datasets(tableName).First()["id"].ToString();

        	//In a production application, use more specific exception handling. 
        	try
        	{
            	HttpWebRequest request = DatsetRequest(String.Format("{0}/{1}/tables/{2}/rows", datasetsUri, datasetId, tableName), "POST", AccessToken);

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
Power BI uses JSON to transmit data objects between a client and the Power BI service. JSON is a text string that represents a collection of objects. The Power BI Getting Started Sample uses several extension methods that make using JSON with Power BI API really easy. To learn more about the Power BI sample extension methods, see JSONBuilder.cs in the sample source code. For more information about Power BI JSON, see {TODO}.

**object.ToJsonSchema()** - Returns Power BI JSON schema from a .NET class

	public class jsonExample
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }

	string json = new jsonExample().ToJsonSchema("JSON Example");

**json**

	{"name": "JSON Example","tables": 
		[{"name": "jsonExample", "columns": 
			[{ "name": "Name", "dataType": "string"},{ "name": "Date", "dataType": "DateTime"}]}]}


**object.ToJson()** - Returns Power BI JSON rows from a List\<T\>

    List<jsonExample> jsonExamples = new List<jsonExample>
    {
        new jsonExample{Name = "Name1", Date = DateTime.Now},
        new jsonExample{Name = "Name2", Date = DateTime.Parse("12/4/2014")},
        new jsonExample{Name = "Name3", Date = DateTime.Parse("9/14/2015")}
    };

    string json = jsonExamples.ToJson(JavaScriptConverter<jsonExample>.GetSerializer());

**json**

	"{"rows":[
		{"Name":"Name1","Date":"12/04/2014"},
		{"Name":"Name2","Date":"12/04/2014"},
		{"Name":"Name3","Date":"09/14/2015"}]}"

**string.ToObject()** - Gets a List<Object> from a JSON string

	List<Object> datasets = responseJson.ToObject<List<Object>>();


<p>paragraph <b>emphasis</b>

**List\<Object\>datasets(name)** - Gets an IEnumerable\<Dictionary\<string, object\>\> from a dataset name</div>
	
	string datasetId = GetAllDatasets().Datasets(tableName).First()["id"].ToString();

For more information about JSON, see [Introducing JSON](http://json.org/). 