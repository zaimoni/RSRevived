// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Faction
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Faction
  {
    private List<Faction> m_Enemies = new List<Faction>(1);
    private int m_ID;
    private string m_Name;
    private string m_MemberName;

    public int ID
    {
      get
      {
        return this.m_ID;
      }
      set
      {
        this.m_ID = value;
      }
    }

    public string Name
    {
      get
      {
        return this.m_Name;
      }
    }

    public string MemberName
    {
      get
      {
        return this.m_MemberName;
      }
    }

    public bool LeadOnlyBySameFaction { get; set; }

    public IEnumerable<Faction> Enemies
    {
      get
      {
        return (IEnumerable<Faction>) this.m_Enemies;
      }
    }

    public Faction(string name, string memberName)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (memberName == null)
        throw new ArgumentNullException("memberName");
      this.m_Name = name;
      this.m_MemberName = memberName;
    }

    public void AddEnemy(Faction other)
    {
      this.m_Enemies.Add(other);
    }

    public virtual bool IsEnemyOf(Faction other)
    {
      if (other != this)
        return this.m_Enemies.Contains(other);
      return false;
    }
  }
}
