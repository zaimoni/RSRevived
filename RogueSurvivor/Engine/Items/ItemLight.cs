// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemLight
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemLight : Item, BatteryPowered
    {
    new public ItemLightModel Model { get {return (base.Model as ItemLightModel)!; } }
    private int m_Batteries;

    public int Batteries {
      get { return m_Batteries; }
      set {
        if (value < 0) value = 0;
        m_Batteries = Math.Min(value, Model.MaxBatteries);
      }
    }

    public int MaxBatteries { get { return Model.MaxBatteries; } }
    public short FovBonus { get { return Model.FovBonus; } }
    public bool AugmentsSenses(Actor a) { return true; }

    public override string ImageID {
      get {
        return (IsEquipped && 0 < m_Batteries) ? base.ImageID : Model.OutOfBatteriesImageID;
      }
    }

    public override bool IsUseless { get { return 0 >= m_Batteries; } }

    // precondtion: neither is useless
    public bool IsLessUsableThan(ItemLight rhs)
    {
      if (2 >= Batteries) return true;
      else if (2 >= rhs.Batteries) return false;
      if (FovBonus<rhs.FovBonus) return true;
      else if (FovBonus > rhs.FovBonus) return false;
      // Gresham's law: use the worst one first
      return Batteries < rhs.Batteries;
    }

    public ItemLight(ItemLightModel model) : base(model)
    {
      Batteries = model.MaxBatteries;
    }

    public override Item_s toStruct() { return new Item_s(ModelID, m_Batteries); }
    public override void toStruct(ref Item_s dest)
    {
        dest.ModelID = ModelID;
        dest.QtyLike = m_Batteries;
        dest.Flags = 0;
    }

    public override string ToString()
    {
      return ModelID.ToString()+ string.Format(" {0}/{1} ({2}h)", Batteries, MaxBatteries, (Batteries / WorldTime.TURNS_PER_HOUR));
    }
  }
}
