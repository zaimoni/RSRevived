// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.SkillTable
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class SkillTable
  {
    private Dictionary<int, Skill> m_Table;

    public IEnumerable<Skill> Skills
    {
      get {
        return (null== m_Table ? null : m_Table.Values);
      }
    }

    public int[] SkillsList
    {
      get
      {
        if (m_Table == null)
          return (int[]) null;
        int[] numArray = new int[CountSkills];
        int num = 0;
        foreach (Skill skill in m_Table.Values)
          numArray[num++] = skill.ID;
        return numArray;
      }
    }

    public int CountSkills
    {
      get {
        return (null==m_Table ? 0 : m_Table.Values.Count);
      }
    }

    public int CountTotalSkillLevels
    {
      get {
        if (null==m_Table) return 0;
        int num = 0;
        foreach (Skill skill in m_Table.Values)
          num += skill.Level;
        return num;
      }
    }

    public SkillTable()
    {
    }

    public SkillTable(IEnumerable<Skill> startingSkills)
    {
      Contract.Requires(null!=startingSkills);
      foreach (Skill startingSkill in startingSkills)
        AddSkill(startingSkill);
    }

    public Skill GetSkill(djack.RogueSurvivor.Gameplay.Skills.IDs id)
    {
      if (m_Table == null) return null;
      Skill skill;
      if (m_Table.TryGetValue((int) id, out skill))
        return skill;
      return null;
    }

    public int GetSkillLevel(djack.RogueSurvivor.Gameplay.Skills.IDs id)
    {
      Skill skill = GetSkill(id);
      return (null== skill ? 0 : skill.Level);
    }

    public void AddSkill(Skill sk)
    {
      Contract.Requires(null!=sk);
      if (m_Table == null) m_Table = new Dictionary<int, Skill>(3);
      if (m_Table.ContainsKey(sk.ID)) throw new ArgumentException("skill of same ID already in table");
      if (m_Table.ContainsValue(sk)) throw new ArgumentException("skill already in table");
      m_Table.Add(sk.ID, sk);
    }

    public void AddOrIncreaseSkill(djack.RogueSurvivor.Gameplay.Skills.IDs id)
    {
      if (m_Table == null) m_Table = new Dictionary<int, Skill>(3);
      Skill skill = GetSkill(id);
      if (skill == null)
      {
        skill = new Skill(id);
                m_Table.Add((int) id, skill);
      }
      ++skill.Level;
    }

    public void DecOrRemoveSkill(djack.RogueSurvivor.Gameplay.Skills.IDs id)
    {
      if (m_Table == null) return;
      Skill skill = GetSkill(id);
      if (skill == null || --skill.Level > 0) return;
      m_Table.Remove((int) id);
      if (m_Table.Count != 0) return;
      m_Table = null;
    }
  }
}
