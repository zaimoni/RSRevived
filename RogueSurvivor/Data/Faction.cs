// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Faction
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Data
{
  internal class Faction
  {
    private readonly List<Faction> m_Enemies = new List<Faction>(1);

    public int ID { get; set; }
    public readonly string Name;
    public readonly string MemberName;
    public readonly bool LeadOnlyBySameFaction;

    public IEnumerable<Faction> Enemies {
      get {
        return m_Enemies;
      }
    }

    public Faction(string name, string memberName, bool leadOnlyBySameFaction = false)
    {
      if (name == null) throw new ArgumentNullException("name");
      if (memberName == null) throw new ArgumentNullException("memberName");
      Name = name;
      MemberName = memberName;
      LeadOnlyBySameFaction = leadOnlyBySameFaction;
    }

    public void AddEnemy(Faction other)
    {
      m_Enemies.Add(other);
    }

    public bool IsEnemyOf(Faction other)
    {
      return other!=this && m_Enemies.Contains(other);
    }
  }
}
