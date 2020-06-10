using System;
using System.Collections.Generic;
using System.Text;

namespace Discord_Bot
{
    public class Track
    {
        public string Id { get; set; }
        public string Titulo { get; set; }
        public string Source { get; set; }
        public Uri Link { get; set; }
        public int PosLocal { get; set; }
    }
}
