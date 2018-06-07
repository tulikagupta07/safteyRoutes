using System.Collections.Generic;
using Newtonsoft.Json;

namespace SafetyIndex.Models
{
	public class SafetyIndex
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		[JsonProperty("latitude")]
		public string Latitude { get; set; }

		[JsonProperty("longitude")]
		public string Longitude { get; set; }

		[JsonProperty("areaName")]
		public string AreaName { get; set; }

		[JsonProperty("tweetCollection")]
		public List<Tweet> TweetCollection { get; set; }

		[JsonProperty("sentiment")]
		public int Sentiment { get; set; }

		[JsonProperty("tweetCount")]
		public int TweetCount { get; set; }

		[JsonProperty("sentimentPositive")]
		public int SentimentPositive { get; set; }

		[JsonProperty("sentimentNegative")]
		public int SentimentNegative { get; set; }

		[JsonProperty("range")]
		public string Range { get; set; }
	}
}