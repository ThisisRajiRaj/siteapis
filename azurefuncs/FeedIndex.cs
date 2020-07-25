using Newtonsoft.Json;

namespace Rajirajcom.Api
{
    public class FeedIndex
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "date")]
        public string Date { get; set; }
        [JsonProperty(PropertyName = "image")]
        public string Image { get; set; }
    }
}
