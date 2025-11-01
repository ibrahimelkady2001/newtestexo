using System;
using System.Collections.Generic;
using System.Text;

namespace EXOApp.GlobalsClasses
{
    /// <summary>
    /// Class holding each parameter from a config database entry
    /// </summary>
    class ConfigSettingPoint
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string units { get; set; }
        public string description { get; set; }
    }
}
