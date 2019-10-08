// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Doll
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;

#nullable enable

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal readonly struct Doll
  {
    public readonly DollBody Body;
    private readonly List<string>?[]? m_Decorations;  // XXX only valid if the Model imageID is null

    public Doll(ActorModel model)
    {
      Body = model.DollBody;
      m_Decorations = (string.IsNullOrEmpty(model.ImageID) ? new List<string>[(int) DollPart._COUNT] : null);
    }

    public List<string>? GetDecorations(DollPart part)
    {
      return null!= m_Decorations ? m_Decorations[(int) part] : null;
    }

    public void AddDecoration(DollPart part, string imageID)
    {
#if DEBUG
      if (null == m_Decorations) throw new ArgumentNullException(nameof(m_Decorations));
#endif
      (GetDecorations(part) ?? (m_Decorations[(int) part] = new List<string>(1))).Add(imageID);
    }

    public void RemoveDecoration(string imageID)
    {
      for (DollPart index = DollPart.NONE; index < DollPart._COUNT; ++index) {
        var x = GetDecorations(index);
        if (null != x && x.Remove(imageID) && 0 >= x.Count) {
          m_Decorations![(int)index] = null;
          break;
        }
      }
    }

    public void RemoveAllDecorations()
    {
#if DEBUG
      if (null == m_Decorations) throw new ArgumentNullException(nameof(m_Decorations));
#endif
      for (DollPart index = DollPart.NONE; index < DollPart._COUNT; ++index)
        m_Decorations[(int)index] = null;
    }
  }
}
