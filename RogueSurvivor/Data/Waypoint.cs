using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    internal class Waypoint
    {
        public Location dest;
        public string label;

        public Waypoint(Location loc, string text)
        {
            dest = loc;
            label = text;
        }
    };

    [Serializable]
    internal struct Waypoint_s
    {
        public Location dest;
        public string label;

        public Waypoint_s(Location loc, string text) {
            dest = loc;
            label = text;
        }
    };
}
