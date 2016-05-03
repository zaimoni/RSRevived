// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.DollBody
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class DollBody
  {
    [NonSerialized]
    public static readonly DollBody UNDEF = new DollBody(true, 0);
    private readonly bool m_IsMale;
    private readonly int m_Speed;

    public bool IsMale
    {
      get
      {
        return m_IsMale;
      }
    }

    public int Speed
    {
      get
      {
        return m_Speed;
      }
    }

    public DollBody(bool isMale, int speed)
    {
            m_IsMale = isMale;
            m_Speed = speed;
    }
  }
}
