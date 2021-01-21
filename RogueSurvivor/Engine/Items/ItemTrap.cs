// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemTrap
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using djack.RogueSurvivor.Data;
using Zaimoni.Data;

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemTrap : Item,UsableItem
  {
#nullable enable
    new public ItemTrapModel Model { get { return (base.Model as ItemTrapModel)!; } }

    private bool m_IsActivated;
    private bool m_IsTriggered;
    private Actor? m_Owner;  // alpha10
    // we actually should be tracking who knows how to disarm a trap explicitly (anyone who overhears the explanation should be able to pass)
    // this allows the death of the trap-setter to not affect other group mates
    // this also allows overhearing the explanation via police radio to confer ability to pass
    private List<Actor>? m_Known;    // RS Revived

    // unclear whether current game logic allows a trap to be both activated and triggered at once.
    // leave getter/setter overhead in place in case these should be mutually exclusive.
    public bool IsActivated { get { return m_IsActivated; } }

    public bool IsTriggered {   // savefile break \todo convert to member variable, or actually use access control in the setter
      get { return m_IsTriggered; }
      set { m_IsTriggered = value; }    // can't deactivate because that eliminates the owner information
    }

    // alpha10
    public Actor? Owner {
      get {
        // cleanup dead owner reference
        if (m_Owner?.IsDead ?? false) m_Owner = null;
        return m_Owner;
      }
    }

    public IEnumerable<Actor>? KnownBy {
      get {
        if (null != m_Known) {
          m_Known.OnlyIfNot(Actor.IsDeceased);
          if (0 >= m_Known.Count) m_Known = null;
        }
        return m_Known;
      }
    }

    public ItemTrap(ItemTrapModel model) : base(model) {}
    public ItemTrap Clone() { return new ItemTrap(Model); }

#region UsableItem implementation
    public bool CouldUse() { return Model.UseToActivate; }
    public bool CouldUse(Actor a) { return true; }
    public bool CanUse(Actor a) { return CouldUse(a); }
    public void Use(Actor actor, Inventory inv) {
#if DEBUG
      if (!inv.Contains(this)) throw new InvalidOperationException("inventory did not contain "+ToString());
#endif
      actor.SpendActionPoints();
      if (IsActivated) Desactivate();
      else Activate(actor);
      if (RogueGame.Game.ForceVisibleToPlayer(actor))
        RogueGame.AddMessage(RogueGame.MakeMessage(actor, (IsActivated ? RogueGame.VERB_ACTIVATE : RogueGame.VERB_DESACTIVATE).Conjugate(actor), this));
    }
    public string ReasonCantUse(Actor a) {
      if (!CouldUse()) return "does not activate manually";
      return "";
    }

    // Cf. ObjectiveAI::_PrefilterDrop for what can go wrong with this
    public bool UseBeforeDrop(Actor a) { return false; }
    public bool FreeSlotByUse(Actor a) { return false; }
#endregion

    // alpha10
    public void Activate(Actor owner)
    {
      m_Owner = owner;
      m_IsActivated = true;
    }

    public void Desactivate()
    {
      m_Owner = null;
      m_Known = null;
      m_IsActivated = false;
    }

    public bool IsSafeFor(Actor a)  // alpha10 was Actor::IsSafeFrom
    {
      if (m_Known?.Contains(a) ?? false) return true;
      if (null != m_Owner) {
        if (a == m_Owner) return true;
        if (a.IsInGroupWith(m_Owner)) { // XXX telepathy
          (m_Known ??= new List<Actor>()).Add(a);
          return true;
        }
      }
      return false;
    }
#nullable restore

    public bool WouldLearnHowToBypass(Actor a, Location? is_real = null)
    {
      if (IsSafeFor(a)) return false;    // already safe
      if (0 >= Model.Damage) return false;  // mostly harmless
      if (!(a.Controller is Gameplay.AI.ObjectiveAI ai)) return false;

      bool may_ask(Actor act) { // \todo extraction target: ObjectiveAI
        return !act.Controller.IsEngaged && ai.InCommunicationWith(act);
      }

      // Necromancy counts, here.
      // \todo register die handler which auto-cleans m_Owner and m_KnownBy?
      // \todo global-scan ground inventories and request invalidation?
      var allies = a.FilterAllies(m_Known, may_ask);
      if (null != m_Owner && may_ask(m_Owner)) (allies ??= new List<Actor>(1)).Add(m_Owner);
      if (null == allies) return false; // intentionally unrealistically don't burn UI on automatic failure
      if (null == is_real) return true;
      void overheard_trap_instructions(Actor overhear) {
        // The complexity of the instructions is roughly comparable to the plausibility of triggering the trap without help
        // cf. Rules::CheckTrapTriggers (we intentionally allow a low plausibility even for 100% trigger chance)
        if ((null== m_Known || !m_Known.Contains(overhear)) && !Rules.Get.RollChance(TriggerChanceFor(overhear) + 1)) {
          (m_Known ??= new List<Actor>(1)).Add(overhear);
          if (overhear.Model.Abilities.HasSanity) overhear.RegenSanity(Rules.SANITY_RECOVER_CHAT_OR_TRADE / 15);
        }
      }

      string question = "How do I bypass "+ TheName + " at "+is_real.Value+"?";
      string answer = "That " + TheName + " is ....";

      // check for whether an ally is within chat range first
      if (RogueGame.Game.DoBackgroundChat(a, allies, question, answer, overheard_trap_instructions)) {
        (m_Known ??= new List<Actor>(1)).Add(a);
        return true;
      }
      // initiate contact w/ally re trap (ideally cellphone or radio needed)
      // for now just do radios as cellphone needs a major rethinking -- we should be able to have them on w/o conflicting with other items
      // one of the allies on the channel responds; *everyone* who hears both request and response has a chance of learning how to deal w/trap
      // querent is guaranteed
      // \todo reimplement/extend when either army radios or cellphone rewrite lands (police would prefer police radios, Nat guard prefers army radios, etc.)
      if (a.HasActivePoliceRadio) {
        if (RogueGame.Game.DoBackgroundPoliceRadioChat(a, allies, question, answer, overheard_trap_instructions)) {
          (m_Known ??= new List<Actor>(1)).Add(a);
          return true;
        }
      }
      (m_Known ??= new List<Actor>(1)).Add(a);
      return true;
    }

    public bool LearnHowToBypass(Actor a, Location loc) { return WouldLearnHowToBypass(a, loc); }

#nullable enable

    // alpha10
    public int TriggerChanceFor(Actor a)
    {
      if (IsSafeFor(a)) return 0;    // alpha 10.1: safe from trap, means safe from trap

      const int TRAP_UNDEAD_ACTOR_TRIGGER_PENALTY = 30;
      const int TRAP_SMALL_ACTOR_AVOID_BONUS = 90;
      int baseChance = Model.TriggerChance * Quantity;
      int avoidBonus = 0;

      var abilities = a.Model.Abilities;
      if (abilities.IsUndead) avoidBonus -= TRAP_UNDEAD_ACTOR_TRIGGER_PENALTY;
      if (abilities.IsSmall) avoidBonus += TRAP_SMALL_ACTOR_AVOID_BONUS;
      var skills = a.Sheet.SkillTable;
      avoidBonus += skills.GetSkillLevel(Gameplay.Skills.IDs.LIGHT_FEET) * Rules.SKILL_LIGHT_FEET_TRAP_BONUS;
      avoidBonus += skills.GetSkillLevel(Gameplay.Skills.IDs.Z_LIGHT_FEET) * Rules.SKILL_ZLIGHT_FEET_TRAP_BONUS;

      return baseChance - avoidBonus;
    }

    public bool TriggeredBy(Actor a)
    {
      var chance = TriggerChanceFor(a);
      var rules = Rules.Get;
      if (rules.RollChance(chance)) return true;
      if (0 < chance && a.Controller is Gameplay.AI.ObjectiveAI && a.Model.Abilities.CanUseItems) {
        if (!rules.RollChance(chance)) (m_Known ??= new List<Actor>()).Add(a);   // learned, now safe
      }
      return false;
    }

    public bool CheckStepOnBreaks(MapObject mobj) { return Rules.Get.RollChance(Model.BreakChance * mobj.Weight); }
    public bool CheckStepOnBreaks() { return Rules.Get.RollChance(Model.BreakChance); }

    public int EscapeChanceFor(Actor a) {
      var skills = a.Sheet.SkillTable;
      return (skills.GetSkillLevel(Gameplay.Skills.IDs.LIGHT_FEET) * Rules.SKILL_LIGHT_FEET_TRAP_BONUS + skills.GetSkillLevel(Gameplay.Skills.IDs.Z_LIGHT_FEET) * Rules.SKILL_ZLIGHT_FEET_TRAP_BONUS) + (100 - Model.BlockChance * Quantity);
    }

    public override string ToString()
    {
      var ret = base.ToString();
      if (m_IsActivated) ret += " (activated)";
      if (m_IsTriggered) ret += " (triggered)";
      return ret;
    }

    // alpha10
    [OnSerializing] private void OptimizeBeforeSaving(StreamingContext context)
    {
      // cleanup dead owner ref
      if (m_Owner?.IsDead ?? false) m_Owner = null;

      if (null != m_Known) {
        m_Known.OnlyIfNot(Actor.IsDeceased);
        if (0 >= m_Known.Count) m_Known = null;
      }
    }
  }
}
