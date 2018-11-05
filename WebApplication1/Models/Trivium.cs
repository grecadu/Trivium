using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Trivium
    {
        public string Encrypted { get; set; }
        public string Clear { get; set; }
        public bool IsEncrypting { get; set; }
        public string Key { get; set; }
        public string VI { get; set; }

    }
}

