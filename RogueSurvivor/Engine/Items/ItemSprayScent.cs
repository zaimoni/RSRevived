// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemSprayScent
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemSprayScent : Item
  {
    public int SprayQuantity { get; set; }

    public override bool IsUseless {
      get { return 0 >= SprayQuantity; }
    }

    public ItemSprayScent(ItemModel model)
      : base(model)
    {
      if (!(model is ItemSprayScentModel))
        throw new ArgumentException("model is not a ItemScentSprayModel");
            SprayQuantity = (model as ItemSprayScentModel).MaxSprayQuantity;
    }
  }
}
