using System;

namespace SERIAL_PORT.Models
{
    public partial class User
    {
        public Guid id { get; set; }

        public int gen { get; set; }

        public string fullName { get; set; }

        public string shift { get; set; }

        public string group { get; set; }
    }
}
