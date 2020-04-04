using System;

namespace School.Core.Models
{
    public class TasksPerDate
    {
        public DateTime DueDate { get; set; }

        public TaskLine[] Tasks { get; set; }
    }
}
