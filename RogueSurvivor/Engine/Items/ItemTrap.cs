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
    // XXX we actually should be tracking who knows how to disarm a trap explicitly (anyone who overhears the explanation should be able to pass)
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
#if PROTOTYPE
      if (null != m_Known) {
        var allies = a.StrictAllies;    // will be in communication due to radio, etc.
        if (null != allies) {
          allies.RemoveWhere(ally => ally.IsSleeping || ally.Controller.IsEngaged || !m_Known.Contains(ally));
          if (0<allies.Count) {
            if (!is_real) return true;
            // \todo: initiate contact w/ally re trap
            // one of the allies on the channel responds; *everyone* who hears the response has a chance of learning how to deal w/trap
          }
        }
      }
#endif
      return false;
    }

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
