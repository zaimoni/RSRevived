// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemSprayScentModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemSprayScentModel : ItemModel
  {
    private int m_MaxSprayQuantity;
    private Odor m_Odor;
    private int m_Strength;

    public int MaxSprayQuantity
    {
      get
      {
        return m_MaxSprayQuantity;
      }
    }

    public int Strength
    {
      get
      {
        return m_Strength;
      }
    }

    public Odor Odor
    {
      get
      {
        return m_Odor;
      }
    }

    public ItemSprayScentModel(string aName, string theNames, string imageID, int sprayQuantity, Odor odor, int strength)
      : base(aName, theNames, imageID)
    {
            m_MaxSprayQuantity = sprayQuantity;
            m_Odor = odor;
            m_Strength = strength;
    }
  }
}
