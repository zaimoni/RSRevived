// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemGrenadePrimed
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  public sealed class ItemGrenadePrimed : ItemPrimedExplosive, Zaimoni.Serialization.ISerialize
    {
    public ItemGrenadePrimed(Data.Model.Explosive model) : base(model) {}
    public ItemGrenadePrimed(Data.Model.Explosive model, int fuse_left) : base(model, fuse_left) {}


#region implement Zaimoni.Serialization.ISerialize
    protected ItemGrenadePrimed(Zaimoni.Serialization.DecodeObjects decode) : base(decode) {}
    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode) => base.save(encode);
#endregion

  }
}
