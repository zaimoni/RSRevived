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
    public readonly int Protection_Hit;
    public readonly int Protection_Shot;
    public readonly int Encumbrance;
    public readonly int Weight;

    public ItemBodyArmorModel(string aName, string theNames, string imageID, int protection_hit, int protection_shot, int encumbrance, int weight, string flavor)
      : base(aName, theNames, imageID, flavor, DollPart.TORSO)
    {
      Protection_Hit = protection_hit;
      Protection_Shot = protection_shot;
      Encumbrance = encumbrance;
      Weight = weight;
    }

    public Defence ToDefence() { return new Defence(-Encumbrance, Protection_Hit, Protection_Shot); }
    public int Rating { get { return Protection_Hit + Protection_Shot; } }
  }
}
