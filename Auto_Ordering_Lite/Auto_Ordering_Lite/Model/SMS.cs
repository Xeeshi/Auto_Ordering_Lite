using System;
using System.Collections.Generic;

using System.Linq;
using System.Web;

namespace Auto_Ordering_Lite.Models
{
    public class SMS
    {
        public string Masking { get; set; }
        
        public string Receiver { get; set; }
     
        public string MessageBody { get; set; }

        public string APIKey { get; set; }
    }
}