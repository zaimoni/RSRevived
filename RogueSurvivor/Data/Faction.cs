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

    public int ID { get; set; }
    public string Name { get; private set; }
    public string MemberName { get; private set; }
    public bool LeadOnlyBySameFaction { get; set; }

    public IEnumerable<Faction> Enemies
    {
      get
      {
        return (IEnumerable<Faction>) m_Enemies;
      }
    }

    public Faction(string name, string memberName)
    {
      if (name == null) throw new ArgumentNullException("name");
      if (memberName == null) throw new ArgumentNullException("memberName");
      Name = name;
      MemberName = memberName;
    }

    public void AddEnemy(Faction other)
    {
      m_Enemies.Add(other);
    }

    public virtual bool IsEnemyOf(Faction other)
    {
      if (other != this) return m_Enemies.Contains(other);
      return false;
    }
  }
}
