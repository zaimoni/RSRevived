// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemSprayScent
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemSprayScent : Item
  {
    new public ItemSprayScentModel Model { get {return (base.Model as ItemSprayScentModel)!; } }
    public int SprayQuantity { get; set; }

    public override bool IsUseless { get { return 0 >= SprayQuantity; } }

#if USE_ITEM_STRUCT
    public override Item_s toStruct() { return new Item_s(ModelID, SprayQuantity); }
    public override void toStruct(ref Item_s dest)
    {
        dest.ModelID = ModelID;
        dest.QtyLike = SprayQuantity;
        dest.Flags = 0;
    }
#endif

    public ItemSprayScent(ItemSprayScentModel model) : base(model)
    {
      SprayQuantity = model.MaxSprayQuantity;
    }
  }
}
