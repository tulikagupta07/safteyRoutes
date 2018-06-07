
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SafetyIndex.Models
{
	public class Document
	{
		[JsonProperty("language")]
		public string Language { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("text")]
		public string Text { get; set; }

	}

	public class RootObject
	{
		[JsonProperty("documents")]
		public List<Document> Documents { get; set; }
	}
}