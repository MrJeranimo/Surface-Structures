using KSA;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surface_Structures
{
    public class LocationStruct
    {
        public string Name;
        public string Celestial;
        public bool Visible;

        public LocationStruct(string name, string celestial, bool visible)
        {
            Name = name;
            Celestial = celestial;
            Visible = visible;
        }
    }
}
