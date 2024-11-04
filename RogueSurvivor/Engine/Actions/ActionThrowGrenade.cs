// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionThrowGrenade
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Items;

using Point = Zaimoni.Data.Vector2D<short>;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
  public sealed class ActionThrowGrenade : ActorAction, CombatAction, NotSchedulable
    {
    private Point m_ThrowPos;
    private readonly ItemGrenade? m_Grenade = null;
    private readonly ItemGrenadePrimed? m_Primed = null;
    private readonly List<Point> m_LoF = new();
    private readonly int maxRange;

    public ActionThrowGrenade(Actor actor, Point throwPos, ItemGrenade it) : base(actor)
    {
#if DEBUG
      if (!actor.Inventory.Contains(it)) throw new InvalidOperationException("tracing");
#endif
      m_ThrowPos = throwPos;
      m_Grenade = it;
      maxRange = actor.MaxThrowRange(ModelThrow.MaxThrowDistance);
    }

    public ActionThrowGrenade(Actor actor, Point throwPos, ItemGrenadePrimed it) : base(actor)
    {
#if DEBUG
      if (!actor.Inventory.Contains(it)) throw new InvalidOperationException("tracing");
#endif
      m_ThrowPos = throwPos;
      m_Primed = it;
      maxRange = actor.MaxThrowRange(ModelThrow.MaxThrowDistance);
    }

    public Actor? target { get { return (new Location(m_Actor.Location.Map, m_ThrowPos)).Actor; } }

    public override bool IsLegal()
    {
      m_FailReason = ReasonCouldntThrowTo(m_ThrowPos);
      return string.IsNullOrEmpty(m_FailReason);
    }

    public override void Perform()
    {
      if (null != m_Grenade) {
        m_Grenade.EquippedBy(m_Actor);
        RogueGame.Game.DoThrowGrenadeUnprimed(m_Actor, in m_ThrowPos);
      } else {
        m_Primed.EquippedBy(m_Actor);
        RogueGame.Game.DoThrowGrenadePrimed(m_Actor, in m_ThrowPos);
      }
    }

    // interactive UI support
    public Point ThrowDest { get => m_ThrowPos; }
    public IEnumerable<Point> LoF { get => m_LoF; }

    public bool ThrowerInBlast() {
      return Rules.GridDistance(m_Actor.Location.Position, in m_ThrowPos) <= ModelThrow.BlastAttack.Radius;
    }

    public ItemGrenadeModel ModelThrow {
        get {
          if (null != m_Grenade) return m_Grenade.Model;
          else return m_Primed.Model.GrenadeModel;
        }
    }

    public bool update(Point src) {
      if (m_Actor.Location.Map.IsValid(src) && Rules.GridDistance(m_Actor.Location.Position, in src) <= maxRange) {
        m_ThrowPos = src;
        return true;
      }
      return false;
    }

    // modeled from Actor::ReasonCouldntThrowTo
    private string ReasonCouldntThrowTo(Point pos)
    {
      m_LoF.Clear();
      if (Rules.GridDistance(m_Actor.Location.Position, in pos) > maxRange) return "out of throwing range";
      if (!LOS.CanTraceThrowLine(m_Actor.Location, in pos, maxRange, m_LoF)) return "no line of throwing";
      return "";
    }
  }
}
