﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemMedicine
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal sealed class ItemMedicine : Item,UsableItem, Zaimoni.Serialization.ISerialize
    {
    new public ItemMedicineModel Model { get {return (base.Model as ItemMedicineModel)!; } }
    public int Healing { get { return Model.Healing; } }
    public int StaminaBoost { get { return Model.StaminaBoost; } }
    public int SleepBoost { get { return Model.SleepBoost; } }
    public int InfectionCure { get { return Model.InfectionCure; } }
    public int SanityCure { get { return Model.SanityCure; } }

    public ItemMedicine(ItemMedicineModel model) : base(model) {}

#region implement Zaimoni.Serialization.ISerialize
    protected ItemMedicine(Zaimoni.Serialization.DecodeObjects decode) : base(decode) {}
    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode) => base.save(encode);
#endregion


#region UsableItem implementation
    public bool CouldUse() { return true; }
    public bool CouldUse(Actor a) { return !a.Model.Abilities.IsUndead; } // currently redundant, but in RS Alpha 9
    public bool CanUse(Actor a) { return CouldUse(a); }
    public void Use(Actor actor, Inventory inv) {
#if DEBUG
      if (!inv.Contains(this)) throw new InvalidOperationException("inventory did not contain "+ToString());
#endif
      actor.SpendActionPoints();
      actor.RegenHitPoints(actor.ScaleMedicineEffect(Healing));
      actor.RegenStaminaPoints(actor.ScaleMedicineEffect(StaminaBoost));
      actor.Rest(actor.ScaleMedicineEffect(SleepBoost));
      actor.Cure(actor.ScaleMedicineEffect(InfectionCure));
      actor.RegenSanity(actor.ScaleMedicineEffect(SanityCure));
      inv.Consume(this);
      actor.PlayersInLOS()?.RedrawPlayScreen(RogueGame.MakePanopticMessage(actor, RogueGame.VERB_HEAL_WITH.Conjugate(actor), this));
    }
    public string ReasonCantUse(Actor a) {
      if (!CouldUse(a)) return "undeads cannot use medecine";
      return "";
    }
    public bool UseBeforeDrop(Actor a) {
      // this will need re-visiting when building more more complex medicines
      if (0<SleepBoost) {
        int need = a.MaxSleep - a.SleepPoints;
        int num4 = a.ScaleMedicineEffect(SleepBoost);
        return num4 <= need;
//      if (num4 <= need) return true;
      }
      if (0 < SanityCure && a.Controller is Gameplay.AI.ObjectiveAI oai) {
        return 2 <= oai.WantRestoreSAN;
//      if (2 <= oai.WantRestoreSAN) return true;
      }
      if (0< StaminaBoost && a.Inventory!.Contains(this)) {
        int need = a.MaxSTA - a.StaminaPoints;
        int num4 = a.ScaleMedicineEffect(StaminaBoost)+4; // plan on two turns after this
        return num4 <= need;
//      if (num4 <= need) return true;
      }
      // medikit is dual-function
      if (0<InfectionCure) {
        if (0 < a.Infection) return true;
      }
      if (0<Healing) {
        return a.HitPoints < a.MaxHPs;
      }
      return false;
    }
    public bool FreeSlotByUse(Actor a) {
      // this will need re-visiting when building more more complex medicines
      if (0<SleepBoost) {
        int need = a.MaxSleep - a.SleepPoints;
        int num4 = a.ScaleMedicineEffect(SleepBoost);
        return Quantity <= need/num4;
//      if (Quantity <= need/num4) return true;
      }
      if (0 < SanityCure && a.Controller is Gameplay.AI.ObjectiveAI oai) {
        return 2 <= oai.WantRestoreSAN;
//      if (2 <= oai.WantRestoreSAN) return true;
      }
      if (0<StaminaBoost && a.Inventory!.Contains(this)) {
        int need = a.MaxSTA - a.StaminaPoints;
        int num4 = a.ScaleMedicineEffect(StaminaBoost)+4; // plan on two turns after this
        return Quantity <= need/num4;
//      if (Quantity <= need/num4) return true;
      }
      // medikit is dual-function
      if (0<InfectionCure && 0 < a.Infection) {
        if (Quantity <= a.Infection/a.ScaleMedicineEffect(InfectionCure)) return true;
      }
      if (0<Healing && a.HitPoints < a.MaxHPs) {
        return Quantity <= (a.MaxHPs - a.HitPoints + Healing -1)/Healing;
      }
      return false;
    }
#endregion

  }
}
