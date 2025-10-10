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
  public sealed class ItemSprayScent : Item, Zaimoni.Serialization.ISerialize
    {
    new public ItemSprayScentModel Model { get {return (base.Model as ItemSprayScentModel)!; } }
    private int m_SprayQty;

    public int SprayQuantity {
      get { return m_SprayQty; }
      set {
        if (value < 0) value = 0;
        m_SprayQty = Math.Min(value, Model.MaxSprayQuantity);
      }
    }


    public override bool IsUseless { get { return 0 >= SprayQuantity; } }

    public override Item_s toStruct() { return new Item_s(ModelID, SprayQuantity); }
    public override void toStruct(ref Item_s dest)
    {
        dest.ModelID = ModelID;
        dest.QtyLike = SprayQuantity;
        dest.Flags = 0;
    }

    public ItemSprayScent(ItemSprayScentModel model, int qty) : base(model) { SprayQuantity = qty; }
    public ItemSprayScent(ItemSprayScentModel model) : this(model, model.MaxSprayQuantity) {}

#region implement Zaimoni.Serialization.ISerialize
    protected ItemSprayScent(Zaimoni.Serialization.DecodeObjects decode) : base(decode) {
        Zaimoni.Serialization.Formatter.Deserialize7bit(decode.src, ref m_SprayQty);
    }

    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode) {
        base.save(encode);
        Zaimoni.Serialization.Formatter.Serialize7bit(encode.dest, m_SprayQty);
    }
#endregion
  }
}
