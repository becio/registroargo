using System;

namespace School.Core.Models
{
    [Serializable]
    public class LoginOptions
    {
        public string SchoolCode { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
