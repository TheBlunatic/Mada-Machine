using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blunatic.Core
{
    public class RejectionStatus
    {
        public bool HasBeenRejected { get; set; }
        public string RejectionMessage { get; set; }

        public RejectionStatus()
        {
            HasBeenRejected = false;
            RejectionMessage = string.Empty;
        }
    }
}
