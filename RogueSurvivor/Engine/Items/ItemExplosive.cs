// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemExplosive
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  public abstract class ItemExplosive : Item
  {
    new public ItemExplosiveModel Model { get {return (base.Model as ItemExplosiveModel)!; } }
    public readonly int PrimedModelID;

    public ItemExplosive(ItemExplosiveModel model, ItemExplosiveModel primedModel, int qty=1)
      : base(model, qty)
    {
      PrimedModelID = (int) primedModel.ID;
    }

#region implement Zaimoni.Serialization.ISerialize
    protected ItemExplosive(Zaimoni.Serialization.DecodeObjects decode) : base(decode) {
        Zaimoni.Serialization.Formatter.Deserialize7bit(decode.src, ref PrimedModelID);
    }

    new protected void save(Zaimoni.Serialization.EncodeObjects encode) {
        base.save(encode);
        Zaimoni.Serialization.Formatter.Serialize7bit(encode.dest, PrimedModelID);
    }
#endregion

  }
}
