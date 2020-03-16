// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorController
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using Zaimoni.Data;

using Point = Zaimoni.Data.Vector2D_short;
using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

#nullable enable

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal abstract class ActorController
  {
    protected readonly Actor m_Actor;

    protected ActorController(Actor src) { m_Actor = src; }

    public Actor ControlledActor { get { return m_Actor; } } // alpha10
    public virtual void TakeControl() { SensorsOwnedBy(m_Actor); }
    protected abstract void SensorsOwnedBy(Actor actor);
    public virtual void LeaveControl() {}

    // forwarder system for to RogueGame::AddMessage
    public virtual void AddMessage(Data.Message msg) { RogueForm.Game.AddMessage(msg); }
    public virtual void AddMessageForceRead(Data.Message msg) { RogueForm.Game.AddMessage(msg); }
    public virtual void AddMessageForceReadClear(Data.Message msg) { RogueForm.Game.AddMessage(msg); }

    public virtual Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int>? ItemMemory {
       get {
         return (m_Actor.IsFaction(Gameplay.GameFactions.IDs.ThePolice)) ? Session.Get.PoliceItemMemory : null;
       }
    }

    public bool LastSeen(Location x, out int turn) {
      turn = 0;
      var memory = ItemMemory;
      return null != memory && Map.Canonical(ref x) && memory.HaveEverSeen(x, out turn);
    }

    public bool IsKnown(Location x) {
      return LastSeen(x, out int discard);
    }

    public void ForceKnown(Point x) {   // for world creation
      var map = m_Actor.Location.Map;
      ItemMemory?.Set(new Location(map, x), null, map.LocalTime.TurnCounter);
    }

    public List<Gameplay.GameItems.IDs>? WhatHaveISeen() { return ItemMemory?.WhatHaveISeen(); }
    public Dictionary<Location, int>? WhereIs(Gameplay.GameItems.IDs x) { return ItemMemory?.WhereIs(x); }

    public HashSet<Point>? WhereIs(IEnumerable<Gameplay.GameItems.IDs> src, Map map) {
      var it_memory = ItemMemory;
      if (null == it_memory) return null;
      var ret = new HashSet<Point>();
      bool IsInHere(Location loc) { return loc.Map == map; };
      foreach(Gameplay.GameItems.IDs it in src) {
        Dictionary<Location, int> tmp = it_memory.WhereIs(it, IsInHere);
        if (null == tmp) continue;
        tmp.OnlyIf(loc => !m_Actor.StackIsBlocked(in loc));
        // XXX cheating postfilter: if it is a ranged weapon but we do not have ammo for that RW, actually check the map inventory and reject if rw has 0 ammo.
        if (0 >= tmp.Count) continue;
        if (Gameplay.GameItems.ranged.Contains(it)) {
          Engine.Items.ItemRangedWeaponModel model = (Models.Items[(int)it] as Engine.Items.ItemRangedWeaponModel)!;
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
        // cheating post-filter: reject boring entertainment
        if (Models.Items[(int)it] is Engine.Items.ItemEntertainmentModel ent) {
            tmp.OnlyIf(loc => {
                // Cf. LOSSensor::_seeItems
                var itemsAt = loc.Map.GetItemsAt(loc.Position);
                if (null == itemsAt) {
                  it_memory.Set(loc,null,loc.Map.LocalTime.TurnCounter);   // Lost faith there was anything there
                  return false;
                }
                if (null != itemsAt.GetFirstByModel<Engine.Items.ItemEntertainment>(ent, e => !e.IsBoringFor(m_Actor))) return true;
                if (null == itemsAt.GetFirstByModel(ent))
                  it_memory.Set(loc, new HashSet<Gameplay.GameItems.IDs>(itemsAt.Items.Select(x => x.Model.ID)), loc.Map.LocalTime.TurnCounter);   // extrasensory perception update
                return false;
            });
        }
        // cheating post-filter: reject dead flashlights at full inventory (these look useless as items but the type may not be useless)
        if (m_Actor.Inventory.IsFull) {
          if (Models.Items[(int)it] is Engine.Items.ItemLightModel || Models.Items[(int)it] is Engine.Items.ItemTrackerModel) {   // want to say "the item type this model is for, is BatteryPowered" without thrashing garbage collector
            tmp.OnlyIf(loc => {
                // Cf. LOSSensor::_seeItems
                var itemsAt = loc.Map.GetItemsAt(loc.Position);
                if (null == itemsAt) {
                  it_memory.Set(loc,null,loc.Map.LocalTime.TurnCounter);   // Lost faith there was anything there
                  return false;
                }
                var test = itemsAt.GetFirstByModel(Models.Items[(int)it]);
                if (null == test) {
                  it_memory.Set(loc, new HashSet<Gameplay.GameItems.IDs>(itemsAt.Items.Select(x => x.Model.ID)), loc.Map.LocalTime.TurnCounter);   // extrasensory perception update
                  return false;
                }
                if (!test.IsUseless) return true;   // actualy want this one
                return null!= itemsAt.GetFirstByModel<Item>(Models.Items[(int)it],obj => !obj.IsUseless);
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
#nullable enable
    public abstract HashSet<Point> FOV { get; }
    public abstract Dictionary<Location, Actor>? friends_in_FOV { get; }
    public abstract Dictionary<Location, Actor>? enemies_in_FOV { get; }
    public virtual Dictionary<Location, Inventory>? items_in_FOV { get { return null; } }
#nullable restore

    public virtual bool IsEngaged { get {
      return null!=enemies_in_FOV;
    } }

    public virtual bool InCombat { get { return IsEngaged; } }


    public abstract bool IsMyTurn();
    /// <returns>null, or an action x for which x.IsPerformable() is true</returns>
#nullable enable
    public virtual ActorAction? ExecAryZeroBehavior(int code) { return null; }

    /// <param name="x"></param>
    private bool _CanSee(Point pos)
    {
      if (pos == m_Actor.Location.Position) return true; // for GUI purposes can see oneself even if sleeping.
      return !m_Actor.IsSleeping && FOV.Contains(pos);
    }
#nullable restore

    public bool CanSee(in Location x)  // correctness requires Location being value-copied
    {
      if (null == x.Map) return false;     // convince Duckman to not superheroically crash many games on turn 0
      if (x.Map != m_Actor.Location.Map) {
        Location? test = m_Actor.Location.Map.Denormalize(in x);
        return null!=test && _CanSee(test.Value.Position);
      }
      return _CanSee(x.Position);
    }

    // we would like to use the CanSee function name for these, but we probably don't need the overhead for sleeping special cases
    private bool _IsVisibleTo(Map map, Point position)
    {
      var a_loc = m_Actor.Location;
      var e = map.GetExitAt(position);
      if (null != e && e.Location == a_loc) return true;
      var a_map = a_loc.Map;
      if (map != a_map) {
        Location? tmp = a_map.Denormalize(new Location(map, position));
        return null!=tmp && _IsVisibleTo(a_map, tmp.Value.Position);
      }
      return map.IsValid(position) && FOV.Contains(position);
    }

    private bool _IsVisibleTo(in Location location)
    {
      return _IsVisibleTo(location.Map, location.Position);
    }

#nullable enable
    public bool IsVisibleTo(Map? map, in Point position)
    {
      return null != map && _IsVisibleTo(map, position);    // convince Duckman to not superheroically crash many games on turn 0
    }
#nullable restore

    public bool IsVisibleTo(in Location loc)
    {
      return null != loc.Map && _IsVisibleTo(loc.Map, loc.Position);    // convince Duckman to not superheroically crash many games on turn 0
    }

#nullable enable
    public bool IsVisibleTo(Actor actor)
    {
      return actor == m_Actor || IsVisibleTo(actor.Location);
    }

    public abstract ActorAction? GetAction();
#nullable restore

    /// <returns>number of turns of trap activation it takes to kill, or int.MaxValue for no known problem</returns>
    public virtual int FastestTrapKill(in Location loc) { return int.MaxValue; }   // z are unaware of deathtraps.  \todo override for dogs
  }
}
