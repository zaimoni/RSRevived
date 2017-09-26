// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Zone
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Zone
  {
    private readonly string m_Name = "unnamed zone";
    private Rectangle m_Bounds;
    private Dictionary<string, object> m_Attributes;

    public string Name { get { return m_Name; } }
    public Rectangle Bounds { get { return m_Bounds; } }

    public Zone(string name, Rectangle bounds)
    {
      Contract.Requires(null!=name);
      m_Name = name;
      m_Bounds = bounds;
    }

    // while zone attributes have great potential, RS Alpha 9 underwhelms in its use of them.
    public bool HasGameAttribute(string key)
    {
      return (null== m_Attributes ? false : m_Attributes.Keys.Contains<string>(key));
    }

    public void SetGameAttribute<_T_>(string key, _T_ value)
    {
      Contract.Requires(null!=key);
      if (m_Attributes == null) m_Attributes = new Dictionary<string, object>(1);
      if (m_Attributes.Keys.Contains<string>(key))
        m_Attributes[key] = value;
      else
        m_Attributes.Add(key, value);
    }

    public _T_ GetGameAttribute<_T_>(string key)
    {
      if (m_Attributes == null) return default (_T_);
      if (!m_Attributes.TryGetValue(key, out object obj)) return default (_T_);
      if (!(obj is _T_)) throw new InvalidOperationException("game attribute is not of requested type");
      return (_T_) obj;
    }
  }
}
