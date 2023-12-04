// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemSprayPaint
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#nullable enable

// VAPORWARE this needs something that makes it AI-worthy to use.  Graffiti was a pre-apocalypse thing.
// * Improvised flamethrower worked for James Bond 007.
// * Z may not be that easy to blind in the first place.

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemSprayPaint : Item
  {
    new public ItemSprayPaintModel Model { get {return (base.Model as ItemSprayPaintModel)!; } }
    private int m_PaintQty;

    public int PaintQuantity {
      get { return m_PaintQty; }
      set {
        if (value < 0) value = 0;
        m_PaintQty = Math.Min(value, Model.MaxPaintQuantity);
      }
    }

    public override bool IsUseless { get { return 0 >= PaintQuantity; } }

    public override Item_s toStruct() { return new Item_s(ModelID, PaintQuantity); }
    public override void toStruct(ref Item_s dest)
    {
            dest.ModelID = ModelID;
            dest.QtyLike = PaintQuantity;
            dest.Flags = 0;
    }

    public ItemSprayPaint(ItemSprayPaintModel model) : base(model)
    {
      PaintQuantity = model.MaxPaintQuantity;
    }
  }
}
