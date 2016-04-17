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
    private Dictionary<int, Skill> m_Table;

    public IEnumerable<Skill> Skills
    {
      get
      {
        if (this.m_Table == null)
          return (IEnumerable<Skill>) null;
        return (IEnumerable<Skill>) this.m_Table.Values;
      }
    }

    public int[] SkillsList
    {
      get
      {
        if (this.m_Table == null)
          return (int[]) null;
        int[] numArray = new int[this.CountSkills];
        int num = 0;
        foreach (Skill skill in this.m_Table.Values)
          numArray[num++] = skill.ID;
        return numArray;
      }
    }

    public int CountSkills
    {
      get
      {
        if (this.m_Table == null)
          return 0;
        return this.m_Table.Values.Count;
      }
    }

    public int CountTotalSkillLevels
    {
      get
      {
        int num = 0;
        foreach (Skill skill in this.m_Table.Values)
          num += skill.Level;
        return num;
      }
    }

    public SkillTable()
    {
    }

    public SkillTable(IEnumerable<Skill> startingSkills)
    {
      if (startingSkills == null)
        throw new ArgumentNullException("startingSkills");
      foreach (Skill startingSkill in startingSkills)
        this.AddSkill(startingSkill);
    }

    public Skill GetSkill(int id)
    {
      if (this.m_Table == null)
        return (Skill) null;
      Skill skill;
      if (this.m_Table.TryGetValue(id, out skill))
        return skill;
      return (Skill) null;
    }

    public int GetSkillLevel(int id)
    {
      Skill skill = this.GetSkill(id);
      if (skill == null)
        return 0;
      return skill.Level;
    }

    public void AddSkill(Skill sk)
    {
      if (this.m_Table == null)
        this.m_Table = new Dictionary<int, Skill>(3);
      if (this.m_Table.ContainsKey(sk.ID))
        throw new ArgumentException("skill of same ID already in table");
      if (this.m_Table.ContainsValue(sk))
        throw new ArgumentException("skill already in table");
      this.m_Table.Add(sk.ID, sk);
    }

    public void AddOrIncreaseSkill(int id)
    {
      if (this.m_Table == null)
        this.m_Table = new Dictionary<int, Skill>(3);
      Skill skill = this.GetSkill(id);
      if (skill == null)
      {
        skill = new Skill(id);
        this.m_Table.Add(id, skill);
      }
      ++skill.Level;
    }

    public void DecOrRemoveSkill(int id)
    {
      if (this.m_Table == null)
        return;
      Skill skill = this.GetSkill(id);
      if (skill == null || --skill.Level > 0)
        return;
      this.m_Table.Remove(id);
      if (this.m_Table.Count != 0)
        return;
      this.m_Table = (Dictionary<int, Skill>) null;
    }
  }
}
