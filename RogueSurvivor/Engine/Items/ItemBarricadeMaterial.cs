﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemBarricadeMaterial
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal sealed class ItemBarricadeMaterial : Item, Zaimoni.Serialization.ISerialize
    {
    new public ItemBarricadeMaterialModel Model { get {return (base.Model as ItemBarricadeMaterialModel)!; } }

    public ItemBarricadeMaterial(ItemBarricadeMaterialModel model, int qty=1) : base(model, qty) {}

#region implement Zaimoni.Serialization.ISerialize
    protected ItemBarricadeMaterial(Zaimoni.Serialization.DecodeObjects decode) : base(decode) {}
    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode) => base.save(encode);
#endregion
  }
}
