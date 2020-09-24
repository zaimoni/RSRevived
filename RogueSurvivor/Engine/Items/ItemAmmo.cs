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

    public ItemAmmo(ItemAmmoModel model) : base(model, model.MaxQuantity) {}

#region UsableItem implementation
    public bool CouldUse() { return true; }
    public bool CouldUse(Actor a) {
      if (a.Model.Abilities.AI_NotInterestedInRangedWeapons) return false; // bikers
      var rw = a.Inventory.GetCompatibleRangedWeapon(this);
      return null != rw && rw.Ammo < rw.Model.MaxAmmo;
    }
    public bool CanUse(Actor a) {
      if (!(a.GetEquippedWeapon() is ItemRangedWeapon rw) || rw.AmmoType != AmmoType) return false;
      if (rw.Ammo >= rw.Model.MaxAmmo) return false;
      return true;
    }
    public void Use(Actor actor, Inventory inv) {
#if DEBUG
      if (!inv.Contains(this)) throw new InvalidOperationException("inventory did not contain "+ToString());
#endif
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      var rw = (actor.GetEquippedWeapon() as ItemRangedWeapon)!;
      sbyte num = (sbyte)Math.Min(rw.Model.MaxAmmo - rw.Ammo, Quantity);
      rw.Ammo += num;
      if (0 >= (Quantity -= num)) inv.RemoveAllQuantity(this);
      else inv.IncrementalDefrag(this);
      var game = RogueForm.Game;
      if (game.ForceVisibleToPlayer(actor))
        game.AddMessage(RogueGame.MakeMessage(actor, RogueGame.VERB_RELOAD.Conjugate(actor), rw));
    }
    public string ReasonCantUse(Actor a) {
      if (!(a.GetEquippedWeapon() is ItemRangedWeapon rw) || rw.AmmoType != AmmoType) return "no compatible ranged weapon equipped";
      if (rw.Ammo >= rw.Model.MaxAmmo) return "weapon already fully loaded";
      return "";
    }
    public bool UseBeforeDrop(Actor a) {
      _rw = a.Inventory.GetCompatibleRangedWeapon(this);
      return null != _rw && _rw.Ammo < _rw.Model.MaxAmmo;
    }
#endregion

    static public ItemAmmo make(Gameplay.GameItems.IDs x)
    {
      ItemModel tmp = Models.Items[(int)x];
      if (tmp is ItemRangedWeaponModel rw_model) tmp = Models.Items[(int)(rw_model.AmmoType)+(int)(Gameplay.GameItems.IDs.AMMO_LIGHT_PISTOL)];    // use the ammo of the ranged weapon instead
      if (tmp is ItemAmmoModel am_model) return new ItemAmmo(am_model);
      throw new ArgumentOutOfRangeException(nameof(x), x, "not ammunition or ranged weapon");
    }
  }
}
