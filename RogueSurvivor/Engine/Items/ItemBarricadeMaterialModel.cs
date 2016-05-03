// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemBarricadeMaterialModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemBarricadeMaterialModel : ItemModel
  {
    private int m_BarricadingValue;

    public int BarricadingValue
    {
      get
      {
        return m_BarricadingValue;
      }
    }

    public ItemBarricadeMaterialModel(string aName, string theNames, string imageID, int barricadingValue)
      : base(aName, theNames, imageID)
    {
            m_BarricadingValue = barricadingValue;
    }
  }
}
