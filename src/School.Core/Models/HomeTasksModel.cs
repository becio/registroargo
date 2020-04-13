using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                TaskLine line = null;
                foreach (var tr in trs)
                {
                    var tdMatter = tr.SelectSingleNode("td[1]");
                    var tdJob = tr.SelectSingleNode("td[2]");

                    var schoolMatter = tdMatter.InnerText.Clean();
                    
                    if (!string.IsNullOrWhiteSpace(schoolMatter))
                    {
                        line = new TaskLine
                        {
                            SchoolMatter = schoolMatter,
                            Description = tdJob.InnerText.Clean()
                        };
                        lines.Add(line);
                    } 
                    else
                    {
                        Debug.Assert(line != null);
                        line.Description += Environment.NewLine + tdJob.InnerText.Clean();
                    }
                }
                job.Tasks = lines.ToArray();

                jobs.Add(job);
            }
            TasksPerDate = jobs.AsReadOnly();
        }
    }
}
