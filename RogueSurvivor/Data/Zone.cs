// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Zone
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace djack.RogueSurvivor.Data
{
  // not meant to be self-contained
  [Serializable]
  internal class Zone
  {
    private readonly string m_Name = "unnamed zone";
    private Rectangle m_Bounds; // assumed to be fully in bounds of the underlying map
    private Dictionary<string, object> m_Attributes;

    public string Name { get { return m_Name; } }
    public Rectangle Bounds { get { return m_Bounds; } }

    public Zone(string name, Rectangle bounds)
    {
#if DEBUG
      if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
#endif
      m_Name = name;
      m_Bounds = bounds;
    }

    // while zone attributes have great potential, RS Alpha 9 underwhelms in its use of them.
    public bool HasGameAttribute(string key)
    {
      return m_Attributes?.ContainsKey(key) ?? false;
    }

    public void SetGameAttribute<_T_>(string key, _T_ value)
    {
#if DEBUG
      if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
#endif
      if (m_Attributes == null) m_Attributes = new Dictionary<string, object>(1);
      m_Attributes[key] = value;
    }

    public _T_ GetGameAttribute<_T_>(string key)
    {
#if DEBUG
      if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
#endif
      if (m_Attributes == null) return default (_T_);
      if (!m_Attributes.TryGetValue(key, out object obj)) return default (_T_);
      if (!(obj is _T_)) throw new InvalidOperationException("game attribute is not of requested type");
      return (_T_) obj;
    }
  }

  [Serializable]    // just in case
  internal class ZoneLoc
  {
    public readonly Map m;
    public readonly Rectangle Rect; // doesn't have to be normalized

    public ZoneLoc(Map _m, Rectangle _r)
    {
      m = _m;
      Rect = _r;
    }

    public bool Contains(Location loc) { return m == loc.Map && Rect.Contains(loc.Position); }
    public bool ContainsExt(Location loc) {
      if (m == loc.Map) return Rect.Contains(loc.Position);
      var test = m.Denormalize(loc);
      return null!=test && Rect.Contains(test.Value.Position);
    }
  }
}
