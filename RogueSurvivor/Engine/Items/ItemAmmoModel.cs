// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemAmmoModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemAmmoModel : ItemModel
  {
    public readonly AmmoType AmmoType;

    public int MaxQuantity { get { return StackingLimit; } }

    public ItemAmmoModel(Gameplay.Item_IDs _id, string imageID, AmmoType ammoType, int maxQuantity)
    : base(_id, ammoType.Describe(), ammoType.Describe(true), imageID)
    {
      AmmoType = ammoType;
      StackingLimit = maxQuantity;
    }

    public override Item create() { return new ItemAmmo(this); }
    public ItemAmmo instantiate() { return new ItemAmmo(this); }
  }
}
