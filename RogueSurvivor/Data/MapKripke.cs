using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zaimoni.Data;

using Point = Zaimoni.Data.Vector2D_short;
using Rectangle = Zaimoni.Data.Box2D_short;

#nullable enable

namespace djack.RogueSurvivor.Data
{
    class MapKripke : Observer<Location[]>  // For Saul Kripke's possible world semantics
    {
        private ZoneLoc m_Domain; // does not have to be normalized
        private Actor? _viewpoint;
        private Dictionary<Location, MapObject>? _knownMapObjects;

        public ref readonly Rectangle Rect { get { return ref m_Domain.Rect; } }

        /// <param name="viewpoint">If non-null, used as default information source.  real map used if null.</param>
        MapKripke(ZoneLoc z, Actor? viewpoint = null)
        {
            m_Domain = z;
            _viewpoint = viewpoint;
            if (null != viewpoint) update(viewpoint.Controller.FOVloc);  // AI
            else { // Socrates' Daimon
                m_Domain.DoForEach(loc => {
                    var obj = loc.MapObject;
                    if (null != obj) (_knownMapObjects ??= new Dictionary<Location,MapObject>()).Add(loc, obj);
                });
            }
        }

        public bool update(Location[] src)
        {
            foreach (var loc in src) {
                var obj = loc.MapObject;
                if (null != obj) (_knownMapObjects ??= new Dictionary<Location, MapObject>())[loc] = obj;
                else _knownMapObjects?.Remove(loc);
            }
            if (null != _knownMapObjects && 0 >= _knownMapObjects.Count) _knownMapObjects = null;
            return false;
        }

        public MapObject? GetMapObjectAt(Location loc)
        {
            if (null == _knownMapObjects) return null;
            if (_knownMapObjects.TryGetValue(loc, out var obj)) return obj;
            return null;
        }

        // to bring up LoS:
        // GetTileModelLocation
        // IsTransparent

        public KeyValuePair<TileModel?,Location> GetTileModelLocation(Point pt) { return m_Domain.m.GetTileModelLocation(pt); }

        public bool IsTransparent(Point pt)
        {
            var tile_loc = GetTileModelLocation(pt);
            if (!tile_loc.Key?.IsTransparent ?? true) return false;
            return GetMapObjectAt(tile_loc.Value)?.IsTransparent ?? true;
        }
    }
}
