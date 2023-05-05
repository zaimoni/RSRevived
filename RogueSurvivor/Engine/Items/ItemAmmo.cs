// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemAmmo
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemAmmo : Item,UsableItem
  {
    [NonSerialized] private ItemRangedWeapon? _rw = null; // set by UseBeforeDrop

    new public ItemAmmoModel Model { get {return (base.Model as ItemAmmoModel)!; } }
    public AmmoType AmmoType { get { return Model.AmmoType; } }
    public ItemRangedWeapon rw { get { return _rw!; } }
    private ItemRangedWeapon? test_rw(Actor a) {
      var rw = a.Inventory.GetCompatibleRangedWeapon(this);
      if (null != rw && rw.Ammo < rw.Model.MaxAmmo) {
        _rw = rw;
        return _rw;
      }
      return null;
    }

    public ItemAmmo(ItemAmmoModel model) : base(model, model.MaxQuantity) {}

#region UsableItem implementation
    public bool CouldUse() { return true; }
    public bool CouldUse(Actor a) {
      if (a.Model.Abilities.AI_NotInterestedInRangedWeapons) return false; // bikers
      return null != test_rw(a);
    }
    public bool CanUse(Actor a) { return CouldUse(a); }
    public void Use(Actor actor, Inventory inv) {
#if DEBUG
      if (!inv.Contains(this)) throw new InvalidOperationException("inventory did not contain "+ToString());
#endif
      if (null != _rw) {
        if (!inv.Contains(_rw)) _rw = null;
        else if (!_rw.IsEquipped) {
          var curr_rw = actor.GetEquippedWeapon() as ItemRangedWeapon;
          if (null != curr_rw && curr_rw.AmmoType == _rw.AmmoType) _rw = curr_rw;
          else _rw.EquippedBy(actor);
        }
      }
      actor.SpendActionPoints();
      var rw = (_rw ?? (actor.GetEquippedWeapon() as ItemRangedWeapon))!;
      sbyte num = (sbyte)Math.Min(rw.Model.MaxAmmo - rw.Ammo, Quantity);
      rw.Ammo += num;
      if (0 >= (Quantity -= num)) inv.RemoveAllQuantity(this);
      else inv.IncrementalDefrag(this);
      var witnesses = RogueGame.PlayersInLOS(actor.Location);
      if (null != witnesses) RogueGame.Game.RedrawPlayScreen(witnesses.Value, RogueGame.MakePanopticMessage(actor, RogueGame.VERB_RELOAD.Conjugate(actor), rw));
      _rw = null;
    }
    public string ReasonCantUse(Actor a) {
      if (!a.Inventory.Contains(_rw)) _rw = null;
      if (null != _rw) return "";   // already cleared
      if (null != test_rw(a)) return "";  // ok
      if (!(a.GetEquippedWeapon() is ItemRangedWeapon rw) || rw.AmmoType != AmmoType) return "no compatible ranged weapon equipped";
      if (rw.Ammo >= rw.Model.MaxAmmo) return "weapon already fully loaded";
      return "";
    }
    public bool UseBeforeDrop(Actor a) { return null != test_rw(a); }
    public bool FreeSlotByUse(Actor a) { return false; } // handled by other behaviors
#endregion

    static public ItemAmmo make(Gameplay.GameItems.IDs x)
    {
      ItemModel tmp = Gameplay.GameItems.From(x);
      if (tmp is ItemRangedWeaponModel rw_model) tmp = Gameplay.GameItems.From((int)(rw_model.AmmoType)+(int)(Gameplay.GameItems.IDs.AMMO_LIGHT_PISTOL));    // use the ammo of the ranged weapon instead
      if (tmp is ItemAmmoModel am_model) return new ItemAmmo(am_model);
      throw new ArgumentOutOfRangeException(nameof(x), x, "not ammunition or ranged weapon");
    }
  }
}
