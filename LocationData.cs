using KSA;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surface_Structures
{
    public class LocationData
    {
        public readonly string Name;
        public string Celestial;
        public bool Visible;
        public readonly string Filepath;
        public double Latitude;
        public double Longitude;
        public LocationReference Location;

        public LocationData(string name, string celestial, bool visible, string filepath, double latitude, double longitude, LocationReference location)
        {
            Name = name;
            Celestial = celestial;
            Visible = visible;
            Filepath = filepath;
            Latitude = latitude;
            Longitude = longitude;
            Location = location;
        }
    }
}
