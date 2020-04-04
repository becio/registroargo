using System;
using System.Collections.Generic;

namespace School.Core.Models
{

    public class BulletinBoardRow
    {
        public DateTime Date { get; set; }
        public string Subject { get; set; }

        public string Message { get; set; }

        public string Url { get; set; }

        public IList<DownloadableFile> Files { get; set; }

    }
}
