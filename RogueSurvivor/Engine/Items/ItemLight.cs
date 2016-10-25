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
    private int m_Batteries;

    public int Batteries
    {
      get {
        return m_Batteries;
      }
      set {
        if (value < 0) value = 0;
        m_Batteries = Math.Min(value, (Model as ItemLightModel).MaxBatteries);
      }
    }

     public int MaxBatteries {
       get {
         return (Model as ItemLightModel).MaxBatteries;
       }
    }

    public int FovBonus {
      get {
        return (Model as ItemLightModel).FovBonus;
      }
    }

    public override string ImageID {
      get {
        if (IsEquipped && Batteries > 0)
          return base.ImageID;
        return (Model as ItemLightModel).OutOfBatteriesImageID;
      }
    }

    public override bool IsUseless {
      get { return 0 >= m_Batteries; }
    }

    public ItemLight(ItemLightModel model)
      : base(model)
    {
      Batteries = (model as ItemLightModel).MaxBatteries;
    }

    public void Recharge()
    {
      Batteries += Math.Max(WorldTime.TURNS_PER_HOUR, (Model as ItemLightModel).MaxBatteries/8);
    }
  }
}
