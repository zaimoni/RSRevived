// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Skill
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Skill
  {
    private int m_ID;
    private int m_Level;

    public int ID
    {
      get
      {
        return m_ID;
      }
    }

    public int Level
    {
      get
      {
        return m_Level;
      }
      set
      {
                m_Level = value;
      }
    }

    public Skill(djack.RogueSurvivor.Gameplay.Skills.IDs id)
    {
      m_ID = (int) id;
    }
  }
}
