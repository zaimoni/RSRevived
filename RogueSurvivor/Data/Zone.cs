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
        return this.m_Name;
      }
      set
      {
        this.m_Name = value;
      }
    }

    public Rectangle Bounds
    {
      get
      {
        return this.m_Bounds;
      }
      set
      {
        this.m_Bounds = value;
      }
    }

    public Zone(string name, Rectangle bounds)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      this.m_Name = name;
      this.m_Bounds = bounds;
    }

    public bool HasGameAttribute(string key)
    {
      if (this.m_Attributes == null)
        return false;
      return this.m_Attributes.Keys.Contains<string>(key);
    }

    public void SetGameAttribute<_T_>(string key, _T_ value)
    {
      if (this.m_Attributes == null)
        this.m_Attributes = new Dictionary<string, object>(1);
      if (this.m_Attributes.Keys.Contains<string>(key))
        this.m_Attributes[key] = (object) value;
      else
        this.m_Attributes.Add(key, (object) value);
    }

    public _T_ GetGameAttribute<_T_>(string key)
    {
      if (this.m_Attributes == null)
        return default (_T_);
      object obj;
      if (!this.m_Attributes.TryGetValue(key, out obj))
        return default (_T_);
      if (!(obj is _T_))
        throw new InvalidOperationException("game attribute is not of requested type");
      return (_T_) obj;
    }
  }
}
