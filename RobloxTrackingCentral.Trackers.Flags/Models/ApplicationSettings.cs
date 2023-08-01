using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RobloxTrackingCentral.Trackers.Flags.Models
{
    internal class ApplicationSettings
    {
        [JsonPropertyName("applicationSettings")]
        public Dictionary<string, string> Settings { get; set; } = null!;
    }
}
