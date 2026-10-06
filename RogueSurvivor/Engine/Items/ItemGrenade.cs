// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemGrenade
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Text.Json;
using Zaimoni.JSON;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  public sealed class ItemGrenade : ItemExplosive, IJsonStaged<ItemGrenade>
    {
    new public Data.Model.Explosive Model { get {return (base.Model as Data.Model.Explosive)!; } }
    public ItemGrenade(Data.Model.Explosive model, int qty=1) : base(model, qty) {}

    private ItemGrenade(JsonElement src, JsonSerializerOptions options) : base(src, options) {}

    static ItemGrenade IJsonStaged<ItemGrenade>.fromJson(JsonElement src, JsonSerializerOptions options) => new(src, options);

  }
}
