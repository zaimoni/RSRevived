// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemTrap
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemTrap : Item
  {
    new public ItemTrapModel Model { get { return base.Model as ItemTrapModel; } }

    private bool m_IsActivated;
    private bool m_IsTriggered;
    private Actor m_Owner;  // alpha10
    // we actually should be tracking who knows how to disarm a trap explicitly (anyone who overhears the explanation should be able to pass)
    // this allows the death of the trap-setter to not affect other group mates
    // this also allows overhearing the explanation via police radio to confer ability to pass
    private List<Actor> m_Known;    // RS Revived

    // unclear whether current game logic allows a trap to be both activated and triggered at once.
    // leave getter/setter overhead in place in case these should be mutually exclusive.
    public bool IsActivated { get { return m_IsActivated; } }

    public bool IsTriggered {
      get { return m_IsTriggered; }
      set { m_IsTriggered = value; }
    }


    // alpha10
    public Actor Owner {
      get {
        // cleanup dead owner reference
        if (m_Owner?.IsDead ?? false) m_Owner = null;
        return m_Owner;
      }
    }

    public IEnumerable<Actor> KnownBy {
      get {
        if (null != m_Known) {
          int i = m_Known.Count;
          while(0 < i--) {
            if (m_Known[i].IsDead) m_Known.RemoveAt(i);
          }
          if (0 >= m_Known.Count) m_Known = null;
        }
        return m_Known;
      }
    }

    public ItemTrap(ItemTrapModel model)
      : base(model)
    {
    }

    public ItemTrap Clone()
    {
      return new ItemTrap(Model);
    }

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
      if (null != m_Known && m_Known.Contains(a)) return true;
      if (null != m_Owner) {
        if (a == m_Owner) return true;
        if (a.IsInGroupWith(m_Owner)) { // XXX telepathy
          var test = (m_Known ?? (m_Known = new List<Actor>()));
          if (!test.Contains(a)) test.Add(a);
          return true;
        }
      }
      return false;
    }

    public bool WouldLearnHowToBypass(Actor a, Location? is_real = null)
    {
      if (IsSafeFor(a)) return false;    // already safe
      var ai = a.Controller as Gameplay.AI.ObjectiveAI;
      if (null == ai) return false;
      var allies = a.FilterAllies(m_Known, ally => !ally.IsSleeping && !ally.Controller.IsEngaged && ai.InCommunicationWith(ally));
      if (null == allies) return false; // intentionally unrealistically don't burn UI on automatic failure
      if (null == is_real) return true;
      m_Known.Add(a);
      void overheard_trap_instructions(Actor overhear) {
        // The complexity of the instructions is roughly comparable to the plausibility of triggering the trap without help
        // cf. Rules::CheckTrapTriggers (we intentionally allow a low plausibility even for 100% trigger chance)
        if (!m_Known.Contains(overhear) && !RogueForm.Game.Rules.RollChance(TriggerChanceFor(overhear) + 1)) {
          m_Known.Add(overhear);
          if (overhear.Model.Abilities.HasSanity) overhear.RegenSanity(Rules.SANITY_RECOVER_CHAT_OR_TRADE / 15);
        }
      }

      string question = "How do I bypass "+ TheName + " at "+is_real.Value+"?";
      string answer = "That " + TheName + " is ....";

      // check for whether an ally is within chat range first
      if (RogueForm.Game.DoBackgroundChat(a, allies, question, answer, overheard_trap_instructions)) return true;
      // initiate contact w/ally re trap (ideally cellphone or radio needed)
      // for now just do radios as cellphone needs a major rethinking -- we should be able to have them on w/o conflicting with other items
      // one of the allies on the channel responds; *everyone* who hears both request and response has a chance of learning how to deal w/trap
      // querent is guaranteed
      // \todo reimplement/extend when either army radios or cellphone rewrite lands (police would prefer police radios, Nat guard prefers army radios, etc.)
      if (a.HasActivePoliceRadio) {
        if (RogueForm.Game.DoBackgroundPoliceRadioChat(a, allies, question, answer, player => !m_Known.Contains(player), overheard_trap_instructions)) return true;
      }
      return true;
    }

    public bool LearnHowToBypass(Actor a, Location loc) { return WouldLearnHowToBypass(a, loc); }

    // alpha10
    public int TriggerChanceFor(Actor a)
    {
      if (IsSafeFor(a)) return 0;    // alpha 10.1: safe from trap, means safe from trap

      const int TRAP_UNDEAD_ACTOR_TRIGGER_PENALTY = 30;
      const int TRAP_SMALL_ACTOR_AVOID_BONUS = 90;
      int baseChance = Model.TriggerChance * Quantity;
      int avoidBonus = 0;

      if (a.Model.Abilities.IsUndead) avoidBonus -= TRAP_UNDEAD_ACTOR_TRIGGER_PENALTY;
      if (a.Model.Abilities.IsSmall) avoidBonus += TRAP_SMALL_ACTOR_AVOID_BONUS;
      avoidBonus += a.Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.LIGHT_FEET) * Rules.SKILL_LIGHT_FEET_TRAP_BONUS;
      avoidBonus += a.Sheet.SkillTable.GetSkillLevel(Gameplay.Skills.IDs.Z_LIGHT_FEET) * Rules.SKILL_ZLIGHT_FEET_TRAP_BONUS;

      return baseChance - avoidBonus;
    }

    public bool TriggeredBy(Actor a)
    {
      var chance = TriggerChanceFor(a);
      if (RogueForm.Game.Rules.RollChance(chance)) return true;
      if (0 < chance && a.Controller is Gameplay.AI.ObjectiveAI && a.Model.Abilities.CanUseItems) {
        if (!RogueForm.Game.Rules.RollChance(chance)) m_Known.Add(a);   // learned, now safe
      }
      return false;
    }

    // alpha10
    public override void OptimizeBeforeSaving()
    {
      base.OptimizeBeforeSaving();

      // cleanup dead owner ref
      if (m_Owner?.IsDead ?? false) m_Owner = null;

      if (null != m_Known) {
        int i = m_Known.Count;
        while(0 < i--) {
          if (m_Known[i].IsDead) m_Known.RemoveAt(i);
        }
        if (0 >= m_Known.Count) m_Known = null;
      }
    }
  }
}
