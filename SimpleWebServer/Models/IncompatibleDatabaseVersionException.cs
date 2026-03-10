using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWebServer.Models
{
    public class IncompatibleDatabaseVersionException : Exception
    {
        public IncompatibleDatabaseVersionException() :base() { }
        public IncompatibleDatabaseVersionException(string message) : base(message) { }

    }
}
