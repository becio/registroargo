using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace School.Core.Models
{
    public class HomeTasksModel
    {
        internal HomeTasksModel(string html)
        {
            ParseHtml(html);
        }

        public IList<TasksPerDate> TasksPerDate { get; private set; }

        void ParseHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var jobs = new List<TasksPerDate>();

            var fields = doc.DocumentNode.Descendants("fieldset");
            var itCulture = new CultureInfo("it-IT");
            foreach(var field in fields)
            {
                var job = new TasksPerDate();

                var legend = field.Element("legend");
                var s = legend.InnerText.Clean();
                job.DueDate = DateTime.Parse(s, itCulture.DateTimeFormat);

                var trs = field.Descendants("tr");
                var lines = new List<TaskLine>();
                foreach(var tr in trs)
                {
                    var tdMatter = tr.SelectSingleNode("td[1]");
                    var tdJob = tr.SelectSingleNode("td[2]");

                    var line = new TaskLine
                    {
                        SchoolMatter = tdMatter.InnerText.Clean(),
                        Description = tdJob.InnerText.Clean()
                    };
                    lines.Add(line);
                }
                job.Tasks = lines.ToArray();

                jobs.Add(job);
            }
            TasksPerDate = jobs.AsReadOnly();
        }
    }
}
