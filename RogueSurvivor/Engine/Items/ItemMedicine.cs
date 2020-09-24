// Decompiled with JetBrains decompiler
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
  internal class ItemMedicine : Item,UsableItem
  {
    new public ItemMedicineModel Model { get {return (base.Model as ItemMedicineModel)!; } }
    public int Healing { get { return Model.Healing; } }
    public int StaminaBoost { get { return Model.StaminaBoost; } }
    public int SleepBoost { get { return Model.SleepBoost; } }
    public int InfectionCure { get { return Model.InfectionCure; } }
    public int SanityCure { get { return Model.SanityCure; } }

    public ItemMedicine(ItemMedicineModel model) : base(model) {}

#region UsableItem implementation
    public bool CouldUse() { return true; }
    public bool CouldUse(Actor a) { return !a.Model.Abilities.IsUndead; } // currently redundant, but in RS Alpha 9
    public bool CanUse(Actor a) { return CouldUse(a); }
    public void Use(Actor actor, Inventory inv) {
#if DEBUG
      if (!inv.Contains(this)) throw new InvalidOperationException("inventory did not contain "+ToString());
#endif
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      actor.RegenHitPoints(actor.ScaleMedicineEffect(Healing));
      actor.RegenStaminaPoints(actor.ScaleMedicineEffect(StaminaBoost));
      actor.Rest(actor.ScaleMedicineEffect(SleepBoost));
      actor.Cure(actor.ScaleMedicineEffect(InfectionCure));
      actor.RegenSanity(actor.ScaleMedicineEffect(SanityCure));
      inv.Consume(this);
      var game = RogueForm.Game;
      if (game.ForceVisibleToPlayer(actor))
        game.AddMessage(RogueGame.MakeMessage(actor, RogueGame.VERB_HEAL_WITH.Conjugate(actor), this));
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
      if (0<StaminaBoost) {
        int need = a.MaxSTA - a.StaminaPoints;
        int num4 = a.ScaleMedicineEffect(StaminaBoost)+4; // plan on two turns after this
        return num4 <= need;
//      if (num4 <= need) return true;
      }
      // medikit is dual-function
      if (0<InfectionCure) {
        if (a.Infection <= a.ScaleMedicineEffect(InfectionCure)) return true;
      }
      if (0<Healing) {
        return 0 < a.MaxHPs - a.HitPoints;
      }
      return false;
    }
#endregion

  }
}
