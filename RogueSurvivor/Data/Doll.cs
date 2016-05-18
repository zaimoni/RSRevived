// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Doll
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Doll
  {
    private DollBody m_Body;
    private List<string>[] m_Decorations;

    public DollBody Body
    {
      get
      {
        return m_Body;
      }
    }

    public Doll(DollBody body)
    {
            m_Body = body;
            m_Decorations = new List<string>[(int) DollPart._COUNT];
    }

    public List<string> GetDecorations(DollPart part)
    {
      return m_Decorations[(int) part];
    }

    public int CountDecorations(DollPart part)
    {
      List<string> decorations = GetDecorations(part);
      if (decorations != null)
        return decorations.Count;
      return 0;
    }

    public void AddDecoration(DollPart part, string imageID)
    {
      (GetDecorations(part) ?? (m_Decorations[(int) part] = new List<string>(1))).Add(imageID);
    }

    public void RemoveDecoration(string imageID)
    {
      for (int index = 0; index < (int)DollPart._COUNT; ++index)
      {
        List<string> stringList = m_Decorations[index];
        if (stringList != null && stringList.Contains(imageID))
        {
          stringList.Remove(imageID);
          if (stringList.Count == 0) m_Decorations[index] = null;
          break;
        }
      }
    }

    public void RemoveDecoration(DollPart part)
    {
      m_Decorations[(int) part] = null;
    }

    public void RemoveAllDecorations()
    {
      for (int index = 0; index < (int)DollPart._COUNT; ++index)
        m_Decorations[index] = null;
    }
  }
}
