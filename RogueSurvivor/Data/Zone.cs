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
  [Serializable]
  internal class Zone
  {
    private string m_Name = "unnamed zone";
    private Rectangle m_Bounds;
    private Dictionary<string, object> m_Attributes;

    public string Name
    {
      get
      {
        return m_Name;
      }
      set
      {
                m_Name = value;
      }
    }

    public Rectangle Bounds
    {
      get
      {
        return m_Bounds;
      }
      set
      {
                m_Bounds = value;
      }
    }

    public Zone(string name, Rectangle bounds)
    {
      if (name == null)
        throw new ArgumentNullException("name");
            m_Name = name;
            m_Bounds = bounds;
    }

    public bool HasGameAttribute(string key)
    {
      if (m_Attributes == null)
        return false;
      return m_Attributes.Keys.Contains<string>(key);
    }

    public void SetGameAttribute<_T_>(string key, _T_ value)
    {
      if (m_Attributes == null)
                m_Attributes = new Dictionary<string, object>(1);
      if (m_Attributes.Keys.Contains<string>(key))
                m_Attributes[key] = (object) value;
      else
                m_Attributes.Add(key, (object) value);
    }

    public _T_ GetGameAttribute<_T_>(string key)
    {
      if (m_Attributes == null)
        return default (_T_);
      object obj;
      if (!m_Attributes.TryGetValue(key, out obj))
        return default (_T_);
      if (!(obj is _T_))
        throw new InvalidOperationException("game attribute is not of requested type");
      return (_T_) obj;
    }
  }
}
