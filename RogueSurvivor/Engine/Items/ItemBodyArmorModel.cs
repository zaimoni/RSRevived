// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemBodyArmorModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemBodyArmorModel : ItemModel
  {
    private readonly int m_Protection_Hit;
    private readonly int m_Protection_Shot;
    private readonly int m_Encumbrance;
    private readonly int m_Weight;

    public int Protection_Hit {
      get {
        return m_Protection_Hit;
      }
    }

    public int Protection_Shot {
      get {
        return m_Protection_Shot;
      }
    }

    public int Encumbrance {
      get {
        return m_Encumbrance;
      }
    }

    public int Weight {
      get {
        return m_Weight;
      }
    }

    public ItemBodyArmorModel(string aName, string theNames, string imageID, int protection_hit, int protection_shot, int encumbrance, int weight)
      : base(aName, theNames, imageID)
    {
      m_Protection_Hit = protection_hit;
      m_Protection_Shot = protection_shot;
      m_Encumbrance = encumbrance;
      m_Weight = weight;
    }

    public Defence ToDefence()
    {
      return new Defence(-m_Encumbrance, m_Protection_Hit, m_Protection_Shot);
    }
  }
}
