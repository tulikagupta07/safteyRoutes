using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using SafetyIndex.Models;
using System.Net;
using System.Text;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace SafetyIndex.Controllers
{
    public class SafetyIndexController : ApiController
    {

	    // GET api/values
	    public IEnumerable<Models.SafetyIndex> Get(string areaName)
	    {
		    var docClient = new DocumentDBRepository<Models.SafetyIndex>();
		    return docClient.GetItems(x => x.AreaName == areaName);
	    }

	    public async Task<IEnumerable<Models.SafetyIndex>> Post(string areaName, string tweet, string userName)
	    {
		    var docClient = new DocumentDBRepository<Models.SafetyIndex>();
			var items = docClient.GetItems(x => x.AreaName == areaName);
		    var item = items[0];

			//Get Sentiment Score
			var root = new RootObject
			{
				Documents = new List<Document>()
			};

			var doc = new Document()
		    {
			    Id = Guid.NewGuid().ToString(),
			    Language = "en",
			    Text = tweet
		    };

		    root.Documents.Add(doc);
		    var values = JsonConvert.SerializeObject(root);


		    var client = new RestClient("https://eastus2.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment");
		    var request = new RestRequest(Method.POST);
		 
		    request.AddHeader("accept", "application/json");
		    request.AddHeader("ocp-apim-subscription-key", "53b43924a33146e380d1bad7c96451d7");
		    request.AddHeader("content-type", "application/json; charset=utf32");
		    request.AddParameter("application/json; charset=utf32",values, ParameterType.RequestBody);
		    IRestResponse result = client.Execute(request);

		    var response = JObject.Parse(result.Content);
		    var sentimentScoreToken = response.SelectToken("documents[0].score");
		    var sentimentScore = sentimentScoreToken.Value<double>();

			//var sentimentScore = double.Parse(sentimentScoreArray[0].Value<string>());

			var tweetItem = new Tweet()
			{
				 Name = userName,
				Comment = tweet,
				IsProcessed = true,
				SentimentScore = sentimentScore
			};


			item.TweetCollection.Add(tweetItem);
		    //Update Counts
			item.TweetCount = item.TweetCollection.Count;
		    if (sentimentScore < 0.5)
			    item.SentimentNegative += 1;
		    else
			    item.SentimentPositive += 1;

		    item.Sentiment = (item.SentimentPositive * 100) / item.TweetCount;

		    if (item.Sentiment < 30)
			    item.Range = "Red";
			else if (item.Sentiment > 70)
			    item.Range = "Green";
		    else
			    item.Range = "Yellow";

		    docClient.ReplaceDocument(item.Id, item);

			return docClient.GetItems(x => x.AreaName == areaName);
		}

		#region http call methods



		/// <summary>
		/// Reads the response from approval service as JObject
		/// </summary>
		/// <param name="httpResponse">HttpResponseMessage</param>
		/// <param name="httpRequest">HttpRequestMessage</param>
		/// <returns>HttpOperationResponse of type JObject</returns>
		private async Task<HttpOperationResponse<string>> ProcessHttpResponseMessageAsync(HttpResponseMessage httpResponse, HttpRequestMessage httpRequest)
		{
			var statusCode = httpResponse.StatusCode;

			if (statusCode != HttpStatusCode.Created && statusCode != HttpStatusCode.OK)
			{
				var errorMessage = $"Operation returned an invalid status code '{statusCode}'";

				string responseContent;
				if (httpResponse.Content != null)
				{
					responseContent = await httpResponse.Content.ReadAsStringAsync();
				}
				else
				{
					responseContent = string.Empty;
				}

				if (statusCode == HttpStatusCode.BadRequest)
				{
					errorMessage = responseContent;
				}

				var ex = new HttpOperationException(errorMessage)
				{
					Request = new HttpRequestMessageWrapper(httpRequest, httpRequest.Content.AsString()),
					Response = new HttpResponseMessageWrapper(httpResponse, responseContent)
				};

				httpRequest.Dispose();
				httpResponse.Dispose();

				throw ex;
			}

			// Create Result
			var result = new HttpOperationResponse<string>
			{
				Request = httpRequest,
				Response = httpResponse,
				Body = await httpResponse.Content.ReadAsStringAsync()
			};
			httpRequest.Dispose();
			httpResponse.Dispose();
			return result;
		}


		/// <summary>
		/// Sets Headers for Deal Service
		/// </summary>
		/// <param name="httpRequest">HttpRequestMessage</param>

		/// <returns>HttpRequestMessage</returns>
		private HttpRequestMessage SetHeaders(HttpRequestMessage httpRequest)
		{
			// Set Headers
			
			httpRequest.Headers.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", "53b43924a33146e380d1bad7c96451d7");
			httpRequest.Headers.TryAddWithoutValidation("Content-Type", "application/json");
			httpRequest.Headers.TryAddWithoutValidation("Accept", "application/json");
	
			return httpRequest;
		}

	   
	    private HttpRequestMessage GetHttpRequestMessageForPost(string values)
	    {
		    // Construct URL
		    
		    var requestUri = new Uri("https://eastus2.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment");


		    // Create HTTP transport objects
		    var httpRequest = new HttpRequestMessage
		    {
			    Method = HttpMethod.Post,
			    RequestUri = requestUri
		    };

		    httpRequest = SetHeaders(httpRequest);

		    // Serialize Request
		    if (values == null) return httpRequest;
		    var myStringContent = new StringContent(values, Encoding.UTF32, "application/json");
			httpRequest.Content = myStringContent;


		    return httpRequest;
	    }

		#endregion
	}
}
