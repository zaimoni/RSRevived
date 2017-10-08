// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemLight
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemLight : Item, BatteryPowered
    {
    new public ItemLightModel Model { get {return base.Model as ItemLightModel; } }
    private int m_Batteries;

    public int Batteries {
      get {
        return m_Batteries;
      }
      set {
        if (value < 0) value = 0;
        m_Batteries = Math.Min(value, Model.MaxBatteries);
      }
    }

    public int MaxBatteries { get { return Model.MaxBatteries; } }
    public int FovBonus { get { return Model.FovBonus; } }

    public override string ImageID {
      get {
        if (IsEquipped && Batteries > 0) return base.ImageID;
        return Model.OutOfBatteriesImageID;
      }
    }

    public override bool IsUseless {
      get { return 0 >= m_Batteries; }
    }

    public ItemLight(ItemLightModel model)
      : base(model)
    {
      Batteries = model.MaxBatteries;
    }

    public void Recharge()
    {
      Batteries += Math.Max(WorldTime.TURNS_PER_HOUR, Model.MaxBatteries/8);
    }

    public override string ToString()
    {
      return Model.ID.ToString()+ string.Format(" {0}/{1} ({2}h)", Batteries, MaxBatteries, (Batteries / WorldTime.TURNS_PER_HOUR));
    }
  }
}
