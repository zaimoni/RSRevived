// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.SkillTable
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class SkillTable
  {
    private Dictionary<Gameplay.Skills.IDs, sbyte> m_Table;

    public IEnumerable<KeyValuePair<Gameplay.Skills.IDs,sbyte>> Skills { get { return m_Table; } }

    public int[] SkillsList
    {
      get {
        if (0 >= CountSkills) return null;
        var ret = new List<int>();
        foreach(var x in m_Table) ret.Add((int)x.Key);
        return ret.ToArray();
      }
    }

    public int CountSkills { get { return m_Table?.Count ?? 0; } }

    public int CountTotalSkillLevels {
      get {
        if (0 >= CountSkills) return 0;
        int num = 0;
        foreach(var x in m_Table) num += x.Value;
        return num;
      }
    }

    public SkillTable()
    {
    }

    public SkillTable(SkillTable src)
    {
#if DEBUG
      if (null == src) throw new ArgumentNullException(nameof(src));
#endif
      if (0<src.CountSkills) m_Table = new Dictionary<Gameplay.Skills.IDs, sbyte>(src.m_Table);
    }

    public int GetSkillLevel(djack.RogueSurvivor.Gameplay.Skills.IDs id)
    {
      if (null == m_Table) return 0;
      if (m_Table.TryGetValue(id, out sbyte skill)) return skill;
      return 0;
    }

    public void AddOrIncreaseSkill(Gameplay.Skills.IDs id)
    {
      if (null == m_Table) m_Table = new Dictionary<Gameplay.Skills.IDs, sbyte>(3);
      if (!m_Table.ContainsKey(id)) m_Table[id] = 1;
      else ++m_Table[id];
    }

    public void DecOrRemoveSkill(Gameplay.Skills.IDs id)
    {
      if (!m_Table?.ContainsKey(id) ?? true) return;
      if (0 < --m_Table[id]) return;
      m_Table.Remove(id);
      if (0 >= m_Table.Count) m_Table = null;
    }
  }
}
