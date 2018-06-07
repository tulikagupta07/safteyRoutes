using Newtonsoft.Json;

namespace SafetyIndex.Models
{
	public class DocumentResponse
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("score")]
		public string Score { get; set; }
	}
}