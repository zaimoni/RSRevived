// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Faction
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Data
{
  internal sealed class Faction
  {
    private readonly List<Faction> m_Enemies = new List<Faction>(1);

    public readonly int ID;
    public readonly string Name;
    public readonly string MemberName;
    public readonly bool LeadOnlyBySameFaction;

    public IEnumerable<Faction> Enemies {
      get {
        return m_Enemies;
      }
    }

    public Faction(string name, string memberName, int id, bool leadOnlyBySameFaction = false)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));
      Contract.Requires(!string.IsNullOrEmpty(memberName));
      ID = id;
      Name = name;
      MemberName = memberName;
      LeadOnlyBySameFaction = leadOnlyBySameFaction;
    }

    // This is supposed to be reflexive.
    // if the contains tests are omitted, the list entries can duplicate in theory.
    // In practice, we launch-block and fail during construction of GameFactions
    public void AddEnemy(Faction other)
    {
      Contract.Requires(null != other);
      if (!m_Enemies.Contains(other)) m_Enemies.Add(other);
      if (!other.m_Enemies.Contains(this)) other.m_Enemies.Add(this);
    }

    public bool IsEnemyOf(Faction other)
    {
      return other!=this && m_Enemies.Contains(other);
    }
  }
}
