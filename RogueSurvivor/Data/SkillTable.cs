// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.SkillTable
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;

#nullable enable

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class SkillTable
  {
    private Dictionary<Gameplay.Skills.IDs, sbyte>? m_Table;

    public IEnumerable<KeyValuePair<Gameplay.Skills.IDs,sbyte>>? Skills { get { return m_Table; } }

    public Gameplay.Skills.IDs[]? SkillsList
    {
      get {
        int ub = CountSkills;
        if (0 >= ub) return null;
        var ret = new Gameplay.Skills.IDs[ub];
        ub = 0;
        foreach(var x in m_Table!) ret[ub++] = x.Key;
        return ret;
      }
    }

    public int CountSkills { get { return m_Table?.Count ?? 0; } }

    public int CountTotalSkillLevels {
      get {
        if (0 >= CountSkills) return 0;
        int num = 0;
        foreach(var x in m_Table!) num += x.Value;
        return num;
      }
    }

    public SkillTable()
    {
    }

    public SkillTable(SkillTable src)
    {
      if (0<src.CountSkills) m_Table = new Dictionary<Gameplay.Skills.IDs, sbyte>(src.m_Table);
    }

    public int GetSkillLevel(djack.RogueSurvivor.Gameplay.Skills.IDs id)
    {
      if (null == m_Table) return 0;
      m_Table.TryGetValue(id, out sbyte skill); // default(sbyte) is 0: ok
      return skill;
    }

    public void AddOrIncreaseSkill(Gameplay.Skills.IDs id)
    {
      if (null == m_Table) m_Table = new Dictionary<Gameplay.Skills.IDs, sbyte>(3);
      if (!m_Table.ContainsKey(id)) m_Table[id] = 1;
      else ++m_Table[id];
    }

    public void DecOrRemoveSkill(Gameplay.Skills.IDs id)
    {
      if (null == m_Table || !m_Table.ContainsKey(id) || 0 < --m_Table[id]) return;
      m_Table.Remove(id);
      if (0 >= m_Table.Count) m_Table = null;
    }
  }
}
