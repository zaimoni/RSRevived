// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ItemModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  internal class ItemModel : Zaimoni.Data.Factory<Item>
  {
    public Gameplay.GameItems.IDs ID { get; set; }
    public readonly string SingleName;
    public readonly string PluralName;
    public readonly string ImageID;
    public readonly string FlavorDescription;
    public readonly DollPart EquipmentPart;

    private int m_StackingLimit = 1;

    public bool IsPlural { get; protected set; }
    public bool IsProper { get; protected set; }
    public bool DontAutoEquip { get; set; }
    public bool IsUnbreakable { get; protected set; }
    public bool IsUnique { get; set; }
    public bool IsForbiddenToAI { get; set; }

    public int StackingLimit
    {
      get { return m_StackingLimit; }
      protected set {
#if DEBUG
        if (0 >= value) throw new ArgumentOutOfRangeException(nameof(value),value, "0 >= value");
#endif
        m_StackingLimit = value;
      }
    }

    public bool IsStackable { get { return 2 <= m_StackingLimit; } }
    public bool IsEquipable { get { return EquipmentPart != DollPart.NONE; } }

    public ItemModel(string aName, string theNames, string imageID, string flavor = "", DollPart part = DollPart.NONE)
    {
      SingleName = aName;
      PluralName = theNames;
      ImageID = imageID;
      FlavorDescription = flavor;
      // if we are not equippable, then there is no operational difference whether we auto-equip or not.
      if (DollPart.NONE == (EquipmentPart = part)) DontAutoEquip = true;
    }

    public virtual Item create()
    {
      throw new InvalidOperationException("override ItemModel::create to do anything useful");
    }
  }
}
