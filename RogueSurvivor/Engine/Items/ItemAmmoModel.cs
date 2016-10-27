// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemAmmoModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemAmmoModel : ItemModel
  {
    private readonly AmmoType m_AmmoType;

    public AmmoType AmmoType {
      get {
        return m_AmmoType;
      }
    }

    public int MaxQuantity {
      get {
        return StackingLimit;
      }
    }

    public ItemAmmoModel(string aName, string theNames, string imageID, AmmoType ammoType, int maxQuantity)
      : base(aName, theNames, imageID)
    {
      m_AmmoType = ammoType;
      StackingLimit = maxQuantity;
      FlavorDescription = "";
    }
  }
}
