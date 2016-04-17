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
        return this.m_Body;
      }
    }

    public Doll(DollBody body)
    {
      this.m_Body = body;
      this.m_Decorations = new List<string>[9];
    }

    public List<string> GetDecorations(DollPart part)
    {
      return this.m_Decorations[(int) part];
    }

    public int CountDecorations(DollPart part)
    {
      List<string> decorations = this.GetDecorations(part);
      if (decorations != null)
        return decorations.Count;
      return 0;
    }

    public void AddDecoration(DollPart part, string imageID)
    {
      (this.GetDecorations(part) ?? (this.m_Decorations[(int) part] = new List<string>(1))).Add(imageID);
    }

    public void RemoveDecoration(string imageID)
    {
      for (int index = 0; index < 9; ++index)
      {
        List<string> stringList = this.m_Decorations[index];
        if (stringList != null && stringList.Contains(imageID))
        {
          stringList.Remove(imageID);
          if (stringList.Count != 0)
            break;
          this.m_Decorations[index] = (List<string>) null;
          break;
        }
      }
    }

    public void RemoveDecoration(DollPart part)
    {
      this.m_Decorations[(int) part] = (List<string>) null;
    }

    public void RemoveAllDecorations()
    {
      for (int index = 0; index < 9; ++index)
        this.m_Decorations[index] = (List<string>) null;
    }
  }
}
