// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemEntertainmentModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemEntertainmentModel : ItemModel
  {
    private int m_Value;
    private int m_BoreChance;

    public int Value
    {
      get
      {
        return m_Value;
      }
      set
      {
                m_Value = value;
      }
    }

    public int BoreChance
    {
      get
      {
        return m_BoreChance;
      }
      set
      {
                m_BoreChance = value;
      }
    }

    public ItemEntertainmentModel(string aName, string theNames, string imageID, int value, int boreChance)
      : base(aName, theNames, imageID)
    {
            m_Value = value;
            m_BoreChance = boreChance;
    }
  }
}
