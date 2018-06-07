using Newtonsoft.Json;

namespace SafetyIndex.Models
{
	public class Tweet
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("comment")]
		public string Comment { get; set; }

		[JsonProperty("sentimentScore")]
		public double SentimentScore { get; set;}

		[JsonProperty("isProcessed")]
		public bool IsProcessed { get; set; }
	}
}