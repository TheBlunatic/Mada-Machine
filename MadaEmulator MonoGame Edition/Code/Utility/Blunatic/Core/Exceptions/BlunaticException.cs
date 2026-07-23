using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blunatic.Core
{
    public class BlunaticException : Exception
    {
        public BlunaticException() : base() { }
        public BlunaticException(string message) : base(message) { }
    }
}
