using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UavApp.MAVLINK
{
    internal class LatLngDto
    {
        public double lat { get; set;}
        public double lng { get; set;}
        public float alt { get; set;}
        public float param1 { get; set;}
        public float param2 { get; set;}
        public float param3 { get; set;}
        public float param4 { get; set;}

        public LatLngDto(double latitude, double longitude, float alt, float param1, float param2, float param3, float param4)
        {
            lat = latitude;
            lng = longitude;
            this.alt = alt;
            this.param1 = param1;
            this.param2 = param2;
            this.param3 = param3;
            this.param4 = param4;
        }
    }
}
