// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Faction
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;

#nullable enable

namespace djack.RogueSurvivor.Data
{
  internal sealed class Faction
  {
    private readonly List<Faction> m_Enemies = new List<Faction>(1);

    public readonly int ID;
    public readonly string Name;
    public readonly string MemberName;
    public readonly bool LeadOnlyBySameFaction;

    public IEnumerable<Faction> Enemies { get { return m_Enemies; } }

    public Faction(string name, string memberName, int id, bool leadOnlyBySameFaction = false)
    {
#if DEBUG
      if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
      if (string.IsNullOrEmpty(memberName)) throw new ArgumentNullException(nameof(memberName));
#endif
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
      if (!m_Enemies.Contains(other)) m_Enemies.Add(other);
      if (!other.m_Enemies.Contains(this)) other.m_Enemies.Add(this);
    }

    public bool IsEnemyOf(Faction other)
    {
      if (1 <= Engine.Session.Get.ScriptStage_PoliceCHARrelations) {
        if (GameFactions.ThePolice==this && GameFactions.TheCHARCorporation==other) return true;
        if (GameFactions.TheCHARCorporation == this && GameFactions.ThePolice == other) return true;
      }
      return other!=this && m_Enemies.Contains(other);
    }
  }
}
