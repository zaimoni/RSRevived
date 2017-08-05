// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorController
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

#define NO_PEACE_WALLS

using djack.RogueSurvivor.Engine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Diagnostics.Contracts;
using Zaimoni.Data;

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal abstract class ActorController
  {
    protected Actor m_Actor;

#if DEBUG
	public Actor Actor { get { return m_Actor; } }
#endif

    public virtual void TakeControl(Actor actor)
    {
      m_Actor = actor;
      SensorsOwnedBy(actor);
    }

    protected abstract void SensorsOwnedBy(Actor actor);

    public virtual void LeaveControl()
    {
      m_Actor = null;
    }

    public virtual Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> ItemMemory {
       get { 
         if (null == m_Actor) return null;
         if ((int)Gameplay.GameFactions.IDs.ThePolice == m_Actor.Faction.ID) return Session.Get.PoliceItemMemory;
         return null;
       }
    }

    public bool LastSeen(Location x, out int turn) { turn = 0; return (null != ItemMemory ? ItemMemory.HaveEverSeen(x, out turn) : false); }

    public bool IsKnown(Location x) {
      int discard;
      return LastSeen(x, out discard);
    }

    public void ForceKnown(Point x) {   // for world creation
      ItemMemory?.Set(new Location(m_Actor.Location.Map, x), null, m_Actor.Location.Map.LocalTime.TurnCounter);
    }

    public List<Gameplay.GameItems.IDs> WhatHaveISeen() { return ItemMemory?.WhatHaveISeen(); }
    public Dictionary<Location, int> WhereIs(Gameplay.GameItems.IDs x) { return ItemMemory?.WhereIs(x); }

    public HashSet<Point> WhereIs(IEnumerable<Gameplay.GameItems.IDs> src, Map map) {
      HashSet<Point> ret = new HashSet<Point>();
      foreach(Gameplay.GameItems.IDs it in src) {
        Dictionary<Location, int> tmp = WhereIs(it);
        tmp.OnlyIf(loc=>loc.Map == map);
        if (0 >= tmp.Count) continue;
        ret.UnionWith(tmp.Keys.Select(loc => loc.Position));
      }
      return ret;
    }

    public abstract List<Percept> UpdateSensors();

    // vision
    public abstract HashSet<Point> FOV { get; }

    public bool CanSee(Location x)
    {
      if (null == m_Actor) return false;
      if (null == x.Map) return false;    // convince Duckman to not superheroically crash many games on turn 0 
      if (x.Map != m_Actor.Location.Map) return false;  // revise these two when restricted district exits go away
      if (!x.Map.IsInBounds(x.Position)) return false;
      if (x.Position == m_Actor.Location.Position) return true; // for GUI purposes can see oneself even if sleeping.
      if (m_Actor.IsSleeping) return false;
      return FOV?.Contains(x.Position) ?? false;
    }

    // we would like to use the CanSee function name for these, but we probably don't need the overhead for sleeping special cases
    private bool _IsVisibleTo(Map map, Point position)
    {
      Contract.Requires(null!=m_Actor);
      Contract.Requires(null!=map);
#if NO_PEACE_WALLS
      if (map != m_Actor.Location.Map)
        {
        Location? tmp = m_Actor.Location.Map.Denormalize(new Location(map, position));
        if (null == tmp) return false;
        return _IsVisibleTo(tmp.Value.Map,tmp.Value.Position);
        }
#else
      if (map != m_Actor.Location.Map) return false;
#endif
      if (!map.IsValid(position.X, position.Y)) return false;
      return FOV?.Contains(position) ?? false;
    }

    private bool _IsVisibleTo(Location location)
    {
      return _IsVisibleTo(location.Map, location.Position);
    }

    public bool IsVisibleTo(Map map, Point position)
    {
      if (null == m_Actor) return false;
      if (null == map) return false;    // convince Duckman to not superheroically crash many games on turn 0 
      return _IsVisibleTo(map,position);
    }

    public bool IsVisibleTo(Location loc)
    {
      if (null == m_Actor) return false;
      if (null == loc.Map) return false;    // convince Duckman to not superheroically crash many games on turn 0 
      return _IsVisibleTo(loc);
    }

    public bool IsVisibleTo(Actor actor)
    {
      if (null == m_Actor) return false;
      if (actor == m_Actor) return true;
      return IsVisibleTo(actor.Location);
    }

    public abstract ActorAction GetAction(RogueGame game);

    // savegame support
    public virtual void OptimizeBeforeSaving() { }  // override this if there are memorized sensors
  }
}
