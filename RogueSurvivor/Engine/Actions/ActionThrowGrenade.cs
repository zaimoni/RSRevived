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
using Rectangle = Zaimoni.Data.Box2D<short>;
using PrimedExplosive = djack.RogueSurvivor.Data._Item.PrimedExplosive;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
  public sealed class ActionThrowGrenade : ActorAction, CombatAction, NotSchedulable
    {
    private Point m_ThrowPos;
    private Location m_Loc;
    private readonly ItemGrenade? m_Grenade = null;
    private readonly PrimedExplosive? m_Primed = null;
    private readonly List<Point> m_LoF = new();
    private readonly int maxRange;

    public ActionThrowGrenade(Actor actor, Point throwPos, ItemGrenade it) : base(actor)
    {
#if DEBUG
      if (!actor.Inventory.Contains(it)) throw new InvalidOperationException("tracing");
#endif
      m_ThrowPos = throwPos;
      m_Loc = new(m_Actor.Location.Map, m_ThrowPos);
      if (!Map.Canonical(ref m_Loc)) m_Loc = default;
      m_Grenade = it;
      maxRange = actor.MaxThrowRange(ModelThrow.MaxThrowDistance);
    }

    public ActionThrowGrenade(Actor actor, Point throwPos, PrimedExplosive it) : base(actor)
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
      if (null != m_Grenade) ThrowGrenadeUnprimed();
      else ThrowGrenadePrimed();
    }

    // interactive UI support
    public Point ThrowDest { get => m_ThrowPos; }
    public Location ThrowLoc { get => m_Loc; }
    public IEnumerable<Point> LoF { get => m_LoF; }

    public bool ThrowerInBlast() {
      return Rules.GridDistance(m_Actor.Location.Position, in m_ThrowPos) <= ModelThrow.BlastAttack.Radius;
    }

    public Data.Model.Explosive ModelThrow {
        get {
          if (null != m_Grenade) return m_Grenade.Model;
          else return m_Primed.Unprimed;
        }
    }

    public bool update(Point src) {
      if (Rules.GridDistance(m_Actor.Location.Position, in src) <= maxRange) {
        Location test = new(m_Actor.Location.Map, src);
        if (!Map.Canonical(ref test)) return false;
        m_Loc = test;
        m_ThrowPos = src;
        return true;
      }
      return false;
    }

    // modeled from Actor::ReasonCouldntThrowTo
    private string ReasonCouldntThrowTo(Point pos)
    {
      m_LoF.Clear();
      if (null == m_Loc.Map || Rules.GridDistance(m_Actor.Location.Position, in pos) > maxRange) return "out of throwing range";
      if (!LOS.CanTraceThrowLine(m_Actor.Location, in pos, maxRange, m_LoF)) return "no line of throwing";
      return "";
    }

    private void ThrowGrenadeUnprimed() {
      m_Grenade.EquippedBy(m_Actor);
      m_Actor.SpendActionPoints();
      m_Actor.Inventory.Consume(m_Grenade);
      // XXX \todo fuse affected by whether target district executes before or after ours (need an extra turn if before)
      // Cf. Map::DistrictDeltaCode
      var itemGrenadePrimed = new PrimedExplosive(m_Grenade.Model);
      m_Loc.Drop(itemGrenadePrimed);

      short radius = (short)itemGrenadePrimed.Model.BlastAttack.Radius;
      var avoid = new ZoneLoc(m_Loc.Map, new Rectangle(m_Loc.Position + radius * Direction.NW.Vector, (short)(2* radius+1) *Direction.SE));
      var flee = new Gameplay.AI.Goals.FleeExplosive(m_Actor, avoid, itemGrenadePrimed);

      void fear_explosive(Actor who) {
        if (who.IsDead || who.IsSleeping) return;
        if (!(who.Controller is Gameplay.AI.ObjectiveAI oai)) return;
        if (!oai.UsesExplosives) return;
        oai.SetUnownedObjective(flee);
      }

      RogueGame.PropagateSight(m_Loc, fear_explosive);

      var a_witness = m_Actor.PlayersInLOS();
      var d_witness = RogueGame.PlayersInLOS(m_Loc);
      if (null != a_witness || null != d_witness) RogueGame.Game.UI_ThrowGrenadeUnprimed(m_Actor, m_ThrowPos, m_Grenade, a_witness, d_witness);
    }

    private void ThrowGrenadePrimed() {
      m_Primed.EquippedBy(m_Actor);
      m_Actor.SpendActionPoints();
      m_Actor.Inventory.RemoveAllQuantity(m_Primed);
      // XXX \todo fuse affected by whether target district executes before or after ours (need an extra turn if before)
      // Cf. Map::DistrictDeltaCode
      m_Loc.Drop(m_Primed);

      short radius = (short)m_Primed.Model.BlastAttack.Radius;
      var avoid = new ZoneLoc(m_Loc.Map, new Rectangle(m_Loc.Position + radius * Direction.NW.Vector, (short)(2* radius+1) *Direction.SE));
      var flee = new Gameplay.AI.Goals.FleeExplosive(m_Actor, avoid, m_Primed);

      void fear_explosive(Actor who) {
        if (who.IsDead || who.IsSleeping) return;
        if (!(who.Controller is Gameplay.AI.ObjectiveAI oai)) return;
        if (!oai.UsesExplosives) return;
        oai.SetUnownedObjective(flee);
      }

      RogueGame.PropagateSight(m_Loc, fear_explosive);

      var a_witness = m_Actor.PlayersInLOS();
      var d_witness = RogueGame.PlayersInLOS(m_Loc);
      if (null != a_witness || null != d_witness) RogueGame.Game.UI_ThrowGrenadePrimed(m_Actor, m_ThrowPos, m_Primed, a_witness, d_witness);
    }
  }
}
