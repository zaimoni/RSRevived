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
    new public Data.Model.Explosive Model { get {return (base.Model as Data.Model.Explosive)!; } }

    public ItemExplosive(Data.Model.Explosive model, int qty=1) : base(model, qty) {}

#region implement Zaimoni.Serialization.ISerialize
    protected ItemExplosive(Zaimoni.Serialization.DecodeObjects decode) : base(decode) {}

    new protected void save(Zaimoni.Serialization.EncodeObjects encode) {
        base.save(encode);
    }
#endregion

    }
}
