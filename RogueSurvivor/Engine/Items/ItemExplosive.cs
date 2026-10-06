// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemExplosive
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Text.Json;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  public abstract class ItemExplosive : Item
  {
    new public Data.Model.Explosive Model { get {return (base.Model as Data.Model.Explosive)!; } }

    public ItemExplosive(Data.Model.Explosive model, int qty=1) : base(model, qty) {}

    protected ItemExplosive(JsonElement src, JsonSerializerOptions options) : base(src, options) { RequireModel<Data.Model.Explosive>(); }

  }
}
