using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Integration.Core.Features.Activities
{
    public class TavilyResponse
    {
        [JsonPropertyName("query")]
        public string Query { get; set; } = string.Empty;

        [JsonPropertyName("images")]
        public List<string> Images { get; set; } = new List<string>();

        [JsonPropertyName("response_time")]
        public double ResponseTime { get; set; }
    }
    public interface ITavilyService
    {
        public Task<List<string>> GetImages(string activityName, string city);
    }
}
