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
    private int m_Protection_Hit;
    private int m_Protection_Shot;
    private int m_Encumbrance;
    private int m_Weight;

    public int Protection_Hit
    {
      get
      {
        return this.m_Protection_Hit;
      }
    }

    public int Protection_Shot
    {
      get
      {
        return this.m_Protection_Shot;
      }
    }

    public int Encumbrance
    {
      get
      {
        return this.m_Encumbrance;
      }
    }

    public int Weight
    {
      get
      {
        return this.m_Weight;
      }
    }

    public ItemBodyArmorModel(string aName, string theNames, string imageID, int protection_hit, int protection_shot, int encumbrance, int weight)
      : base(aName, theNames, imageID)
    {
      this.m_Protection_Hit = protection_hit;
      this.m_Protection_Shot = protection_shot;
      this.m_Encumbrance = encumbrance;
      this.m_Weight = weight;
    }

    public Defence ToDefence()
    {
      return new Defence(-this.m_Encumbrance, this.m_Protection_Hit, this.m_Protection_Shot);
    }
  }
}
