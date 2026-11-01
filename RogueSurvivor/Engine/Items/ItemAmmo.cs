// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemAmmo
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Gameplay;
using System;
using System.Text.Json;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  public sealed class ItemAmmo : Item, UsableItem, IJsonStaged<ItemAmmo>
    {
    [NonSerialized] private ItemRangedWeapon? _rw = null; // set by UseBeforeDrop

    new public ItemAmmoModel Model { get {return (base.Model as ItemAmmoModel)!; } }
    public AmmoType AmmoType { get { return Model.AmmoType; } }
    public ItemRangedWeapon rw { get { return _rw!; } }
    private ItemRangedWeapon? test_rw(LivingActor a) {
      if (a.Inventory.GetReloadableRangedWeapon(this) is { } rw) {
        _rw = rw;
        return _rw;
      }
      return null;
    }

    public ItemAmmo(ItemAmmoModel model, int qty) : base(model, qty) {}
    public ItemAmmo(ItemAmmoModel model) : this(model, model.MaxQuantity) {}

    private ItemAmmo(JsonElement src, JsonSerializerOptions options) : base(src, options) {
        RequireModel<ItemAmmoModel>();
    }

    static ItemAmmo IJsonStaged<ItemAmmo>.fromJson(JsonElement src, JsonSerializerOptions options) => new(src, options);

#region UsableItem implementation
    public bool CouldUse() { return true; }
    public bool CouldUse(LivingActor a) {
      if (a.Model.Abilities.AI_NotInterestedInRangedWeapons) return false; // bikers
      return test_rw(a) is not null;
    }
    public bool CanUse(LivingActor a) { return CouldUse(a); }
    public void Use(LivingActor actor, Inventory inv) {
#if DEBUG
      if (!inv.Contains(this)) throw new InvalidOperationException("inventory did not contain "+ToString());
#endif
      if (_rw is not null) {
        if (!inv.Contains(_rw)) _rw = null;
        else if (!_rw.IsEquipped) {
          var curr_rw = actor.GetEquippedWeapon() as ItemRangedWeapon;
          if (curr_rw is not null && curr_rw.AmmoType == _rw.AmmoType) _rw = curr_rw;
          else _rw.EquippedBy(actor);
        }
      }
      actor.SpendActionPoints();
      var rw = (_rw ?? (actor.GetEquippedWeapon() as ItemRangedWeapon))!;
      sbyte num = (sbyte)Math.Min(rw.Model.MaxAmmo - rw.Ammo, Quantity);
      rw.Ammo += num;
      if (0 >= (Quantity -= num)) inv.RemoveAllQuantity(this);
      else inv.IncrementalDefrag(this);
      actor.PlayersInLOS()?.RedrawPlayScreen(RogueGame.MakePanopticMessage(actor, RogueGame.VERB_RELOAD.Conjugate(actor), rw));
      _rw = null;
    }
    public string ReasonCantUse(LivingActor a) {
      if (!a.Inventory.Contains(_rw)) _rw = null;
      if (_rw is not null) return "";   // already cleared
      if (test_rw(a) is not null) return "";  // ok
      if (!(a.GetEquippedWeapon() is ItemRangedWeapon rw) || rw.AmmoType != AmmoType) return "no compatible ranged weapon equipped";
      if (rw.Ammo >= rw.Model.MaxAmmo) return "weapon already fully loaded";
      return "";
    }
    public bool UseBeforeDrop(LivingActor a) { return test_rw(a) is not null; }
    public bool FreeSlotByUse(LivingActor a) { return false; } // handled by other behaviors
#endregion

    static public ItemAmmo make(Gameplay.Item_IDs x)
    {
      var tmp = GameItems.From(x);
      if (tmp is ItemRangedWeaponModel rw_model) return new(rw_model.AmmoType.AmmoModel());    // use the ammo of the ranged weapon instead
      if (tmp is ItemAmmoModel am_model) return new(am_model);
      throw new ArgumentOutOfRangeException(nameof(x), x, "not ammunition or ranged weapon");
    }
  }
}
