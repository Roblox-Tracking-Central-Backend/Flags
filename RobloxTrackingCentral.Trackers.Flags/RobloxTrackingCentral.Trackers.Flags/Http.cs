using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobloxTrackingCentral.Trackers.Flags
{
    internal static class Http
    {
        public static HttpClient Client { get; }

        static Http()
        {
            Console.WriteLine("Creating " + nameof(HttpClient));
            HttpClientHandler handler = new HttpClientHandler() { AutomaticDecompression = System.Net.DecompressionMethods.All };
            Client = new HttpClient(handler);
        }
    }
}
