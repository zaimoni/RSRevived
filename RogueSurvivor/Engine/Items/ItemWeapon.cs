// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemWeapon
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
  public abstract class ItemWeapon : Item
  {
    new public ItemWeaponModel Model { get {return (base.Model as ItemWeaponModel)!; } }
    protected ItemWeapon(ItemWeaponModel model) : base(model) {}
    protected ItemWeapon(ItemWeaponModel model, int qty) : base(model, qty) {}
    protected ItemWeapon(JsonElement src, JsonSerializerOptions options) : base(src, options) { RequireModel<ItemWeaponModel>(); }
  }
}
