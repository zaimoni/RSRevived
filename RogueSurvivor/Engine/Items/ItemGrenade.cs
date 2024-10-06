// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemGrenade
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  public sealed class ItemGrenade : ItemExplosive, Zaimoni.Serialization.ISerialize
    {
    new public ItemGrenadeModel Model { get {return (base.Model as ItemGrenadeModel)!; } }
    public ItemGrenade(ItemGrenadeModel model, ItemGrenadePrimedModel primedModel, int qty=1)
      : base(model, primedModel, qty)
    {
    }

#region implement Zaimoni.Serialization.ISerialize
    protected ItemGrenade(Zaimoni.Serialization.DecodeObjects decode) : base(decode) {}
    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode) => base.save(encode);
#endregion

  }
}
