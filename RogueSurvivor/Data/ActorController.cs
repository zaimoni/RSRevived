// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorController
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

#define NO_PEACE_WALLS

using djack.RogueSurvivor.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using Zaimoni.Data;

#if Z_VECTOR
using Point = Zaimoni.Data.Vector2D_int;
#else
using Point = System.Drawing.Point;
#endif

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal abstract class ActorController
  {
    protected Actor m_Actor;

    public Actor ControlledActor { get { return m_Actor; } } // alpha10

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

    public bool LastSeen(Location x, out int turn) {
      turn = 0;
      var memory = ItemMemory;
      if (null == memory) return false;
      if (memory.HaveEverSeen(x, out turn)) return true;
      Location? test = x.Map.Normalize(x.Position);
      return null!=test && memory.HaveEverSeen(test.Value, out turn);
    }

    public bool IsKnown(Location x) {
      return LastSeen(x, out int discard);
    }

    public void ForceKnown(Point x) {   // for world creation
      ItemMemory?.Set(new Location(m_Actor.Location.Map, x), null, m_Actor.Location.Map.LocalTime.TurnCounter);
    }

    public List<Gameplay.GameItems.IDs> WhatHaveISeen() { return ItemMemory?.WhatHaveISeen(); }
    public Dictionary<Location, int> WhereIs(Gameplay.GameItems.IDs x) { return ItemMemory?.WhereIs(x); }

    public HashSet<Point> WhereIs(IEnumerable<Gameplay.GameItems.IDs> src, Map map) {
      var it_memory = ItemMemory;
      if (null == it_memory) return null;
      var ret = new HashSet<Point>();
      bool IsInHere(Location loc) { return loc.Map == map; };
      foreach(Gameplay.GameItems.IDs it in src) {
        Dictionary<Location, int> tmp = it_memory.WhereIs(it, IsInHere);
        if (null == tmp) continue;
        // XXX cheating postfilter: if it is a ranged weapon but we do not have ammo for that RW, actually check the map inventory and reject if rw has 0 ammo.
        if (Gameplay.GameItems.ranged.Contains(it)) {
          Engine.Items.ItemRangedWeaponModel model = Models.Items[(int)it] as Engine.Items.ItemRangedWeaponModel;
          var ammo = m_Actor.Inventory.GetItemsByType < Engine.Items.ItemAmmo >(am => am.AmmoType== model.AmmoType);
          if (null == ammo) {
            tmp.OnlyIf(loc => {
                // Cf. LOSSensor::_seeItems
                var itemsAt = loc.Map.GetItemsAt(loc.Position);
                if (null == itemsAt) {
                  it_memory.Set(loc,null,loc.Map.LocalTime.TurnCounter);   // Lost faith there was anything there
                  return false;
                }
                if (null != itemsAt.GetFirstByModel<Engine.Items.ItemRangedWeapon>(model, rw => 0 < rw.Ammo)) return true;
                if (null == itemsAt.GetFirstByModel(model))
                  it_memory.Set(loc, new HashSet<Gameplay.GameItems.IDs>(itemsAt.Items.Select(x => x.Model.ID)), loc.Map.LocalTime.TurnCounter);   // extrasensory perception update
                return false;
            });
            if (0 >= tmp.Count) continue;
          }
        }
        ret.UnionWith(tmp.Keys.Select(loc => loc.Position));
      }
      // XXX need to ask allies where hey are headed for (or are), to avoid traffic jams
      return ret;
    }

    public abstract List<Percept> UpdateSensors();

    // vision
    public abstract HashSet<Point> FOV { get; }
    public abstract Dictionary<Location, Actor> friends_in_FOV { get; }
    public abstract Dictionary<Location, Actor> enemies_in_FOV { get; }
    public virtual Dictionary<Location, Inventory> items_in_FOV { get { return null; } }

    public virtual bool InCombat { get {
      return null!=enemies_in_FOV;
    } }

    public bool CanSee(Location x)  // correctness requires Location being value-copied
    {
      if (null == m_Actor) return false;
      if (null == x.Map) return false;    // convince Duckman to not superheroically crash many games on turn 0
      if (x.Map != m_Actor.Location.Map) {
        Location? test = m_Actor.Location.Map.Denormalize(x);
        if (null == test) return false;
        x = test.Value;
      }
      if (x.Position == m_Actor.Location.Position) return true; // for GUI purposes can see oneself even if sleeping.
      if (m_Actor.IsSleeping) return false;
      return FOV?.Contains(x.Position) ?? false;
    }

    // we would like to use the CanSee function name for these, but we probably don't need the overhead for sleeping special cases
    private bool _IsVisibleTo(Map map, Point position)
    {
#if DEBUG
      if (null == m_Actor) throw new ArgumentNullException(nameof(m_Actor));
      if (null == map) throw new ArgumentNullException(nameof(map));
#endif
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
