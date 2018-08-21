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
    new public ItemSprayPaintModel Model { get {return base.Model as ItemSprayPaintModel; } }
    public int PaintQuantity { get; set; }

    public override bool IsUseless {
      get { return 0 >= PaintQuantity; }
    }

    public override ItemStruct Struct { get { return new ItemStruct(Model.ID, PaintQuantity); } }

    public ItemSprayPaint(ItemSprayPaintModel model)
      : base(model)
    {
      PaintQuantity = model.MaxPaintQuantity;
    }
  }
}
