// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.SkillTable
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Data
{
  // \todo NEXT SAVEFILE BREAK?  We don't seem to be getting anything from the Skill class.  Eliminate completely to conserve object ids in the savefile.
  [Serializable]
  internal class SkillTable
  {
    private Dictionary<Gameplay.Skills.IDs, Skill> m_Table;

    public IEnumerable<Skill> Skills { get { return m_Table?.Values; } }

    public int[] SkillsList
    {
      get {
        if (m_Table == null) return null;
        int[] numArray = new int[CountSkills];
        int num = 0;
        foreach (Skill skill in m_Table.Values)
          numArray[num++] = (int)skill.ID;
        return numArray;
      }
    }

    public int CountSkills { get { return m_Table?.Count ?? 0; } }

    public int CountTotalSkillLevels {
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
#if DEBUG
      if (null == startingSkills) throw new ArgumentNullException(nameof(startingSkills));
#endif
      foreach (Skill startingSkill in startingSkills)
        AddSkill(startingSkill);
    }

    public Skill GetSkill(djack.RogueSurvivor.Gameplay.Skills.IDs id)
    {
      if (m_Table == null) return null;
      if (m_Table.TryGetValue(id, out Skill skill)) return skill;
      return null;
    }

    public int GetSkillLevel(djack.RogueSurvivor.Gameplay.Skills.IDs id)
    {
      return GetSkill(id)?.Level ?? 0;
    }

    private void AddSkill(Skill sk)
    {
#if DEBUG
      if (null == sk) throw new ArgumentNullException(nameof(sk));
#endif
      if (m_Table == null) m_Table = new Dictionary<Gameplay.Skills.IDs, Skill>(3);
      if (m_Table.ContainsKey(sk.ID)) throw new ArgumentException("skill of same ID already in table");
      if (m_Table.ContainsValue(sk)) throw new ArgumentException("skill already in table");
      m_Table.Add(sk.ID, sk);
    }

    public void AddOrIncreaseSkill(Gameplay.Skills.IDs id)
    {
      Skill skill = GetSkill(id);
      if (skill == null) {
        skill = new Skill(id);
        AddSkill(skill);
      }
      ++skill.Level;
    }

    public void DecOrRemoveSkill(djack.RogueSurvivor.Gameplay.Skills.IDs id)
    {
      Skill skill = GetSkill(id);
      if (skill == null || --skill.Level > 0) return;
      m_Table.Remove(id);
      if (0 >= m_Table.Count) m_Table = null;
    }
  }
}
