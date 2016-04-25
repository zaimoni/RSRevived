// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemSprayPaint
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemSprayPaint : Item
  {
    public int PaintQuantity { get; set; }

    public override bool IsUseless {
      get { return 0 >= PaintQuantity; }
    }

    public ItemSprayPaint(ItemModel model)
      : base(model)
    {
      if (!(model is ItemSprayPaintModel))
        throw new ArgumentException("model is not a SprayPaintModel");
      this.PaintQuantity = (model as ItemSprayPaintModel).MaxPaintQuantity;
    }
  }
}
