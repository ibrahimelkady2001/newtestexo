using System;
using System.Collections.Generic;
using System.Text;

namespace EXOApp
{
    /// <summary>
    /// Class to store music track parameters
    /// </summary>
    public class MusicTrack
    {
        public string name { get; set; }
        public int premiumLevel { get; set; }
        public int defaultVolume { get; set; }
    }
}
