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
  internal class ItemLight : Item
  {
    private int m_Batteries;

    public int Batteries
    {
      get
      {
        return this.m_Batteries;
      }
      set
      {
        if (value < 0)
          value = 0;
        this.m_Batteries = Math.Min(value, (this.Model as ItemLightModel).MaxBatteries);
      }
    }

    public int FovBonus
    {
      get
      {
        return (this.Model as ItemLightModel).FovBonus;
      }
    }

    public bool IsFullyCharged
    {
      get
      {
        return this.m_Batteries >= (this.Model as ItemLightModel).MaxBatteries;
      }
    }

    public override string ImageID
    {
      get
      {
        if (this.IsEquipped && this.Batteries > 0)
          return base.ImageID;
        return (this.Model as ItemLightModel).OutOfBatteriesImageID;
      }
    }

    public ItemLight(ItemModel model)
      : base(model)
    {
      if (!(model is ItemLightModel))
        throw new ArgumentException("model is not a LightModel");
      this.Batteries = (model as ItemLightModel).MaxBatteries;
    }

    public void Recharge()
    {
      Batteries += Math.Max(WorldTime.TURNS_PER_HOUR, (Model as ItemLightModel).MaxBatteries/8);
    }
  }
}
