using System.IO;
using System.Net.Http.Headers;

namespace School.Core.Models
{
    public class DownloadResponse
    {
        public MediaTypeHeaderValue ContentType { get; set; }

        public long? ContentLength { get; set; }

        public Stream ResponseStream { get; set; }
    }
}
