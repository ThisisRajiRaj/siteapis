using System;
using Newtonsoft.Json;

namespace Rajirajcom.Api
{
    // DTO to pass arounf info about blog
    public class BlogInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("fileurl")]
        public string Path { get; set; }

        [JsonProperty("minstoread")]
        public string MinsToRead { get; set; }
    }
}