﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorController
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Items;
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

    public virtual void RepairLoad() { }

    public Actor ControlledActor { get { return m_Actor; } } // alpha10
    public virtual void TakeControl() {}
    public virtual void LeaveControl() {}

#region UI messages
    // forwarder system for to RogueGame::AddMessage
    public void AddMessage(UI.Message msg, KeyValuePair<List<PlayerController>, List<Actor>> witnesses) {
      if (0 < witnesses.Key.Count) {
        foreach(var witness in witnesses.Key) {
          witness.AddMessage(msg);
          if (RogueGame.IsPlayer(witness.ControlledActor)) RogueGame.Game.RedrawPlayScreen();
        }
      }
      if (0 < witnesses.Value.Count) {
        foreach(var witness in witnesses.Value) {
            if (RogueGame.IsPlayer(witness)) {
                RogueGame.Game.RedrawPlayScreen(msg);
                return;
            }
        }
      }
    }

    public virtual void AddMessageForceRead(UI.Message msg, KeyValuePair<List<PlayerController>, List<Actor>> witnesses) {
      if (0 < witnesses.Key.Count) {
        foreach(var witness in witnesses.Key) witness.AddMessage(msg);
        RogueGame.Game.PanViewportTo(witnesses.Key);
        return;
      }
      if (0 < witnesses.Value.Count) {
        RogueGame.Game.PanViewportTo(witnesses.Value);
        RogueGame.Game.RedrawPlayScreen(msg);
        return;
      }
    }

    public virtual void AddMessageForceReadClear(UI.Message msg, KeyValuePair<List<PlayerController>, List<Actor>> witnesses) {
      if (0 < witnesses.Key.Count) {
        foreach(var witness in witnesses.Key) witness.AddMessage(msg);
        RogueGame.Game.PanViewportTo(witnesses.Key);
        return;
      }
      if (0 < witnesses.Value.Count) {
        RogueGame.Game.PanViewportTo(witnesses.Value);
        RogueGame.Game.RedrawPlayScreen(msg);
        return;
      }
    }

    // check-in with leader
    public virtual bool ReportBlocked(in InventorySource<Item> src, Actor who) { return true; }
    public virtual bool ReportGone(in InventorySource<Item> src, Actor who) { return true; }
    public virtual bool ReportNotThere(in InventorySource<Item> src, Gameplay.GameItems.IDs what, Actor who) { return true; }
    public virtual bool ReportTaken(in InventorySource<Item> src, Item it, Actor who) { return true; }
#endregion

    public virtual Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int>? ItemMemory {
       get {
         return (m_Actor.IsFaction(GameFactions.IDs.ThePolice)) ? Session.Get.Police.ItemMemory : null;
       }
    }

    public bool LastSeen(Location x, out int turn) {
      turn = 0;
      var memory = ItemMemory;
      return null != memory && Map.Canonical(ref x) && memory.HaveEverSeen(x, out turn);
    }

    public bool IsKnown(Location x) { return LastSeen(x, out _); }

    public void ForceKnown(Point x) {   // for world creation
      var map = m_Actor.Location.Map;
      ItemMemory?.Set(new Location(map, x), null, map.LocalTime.TurnCounter);
    }

    public List<Gameplay.GameItems.IDs>? WhatHaveISeen() { return ItemMemory?.WhatHaveISeen(); }
    public Dictionary<Location, int>? WhereIs(Gameplay.GameItems.IDs x) { return ItemMemory?.WhereIs(x); }

    virtual public IEnumerable<Gameplay.GameItems.IDs>? RejectUnwanted(IEnumerable<Gameplay.GameItems.IDs>? src, Location loc) { return src; }

    public Dictionary<Location, int>? Filter(Dictionary<Location, int> src, Predicate<Inventory> ok) {
      var it_memory = ItemMemory;
      if (null == it_memory) return src;

      Dictionary<Location, int> ret = new();
      foreach(var x in src) {
        // Cf. LOSSensor::_seeItems
        var allItems = Map.AllItemsAt(x.Key, m_Actor);
        if (null == allItems) {
          it_memory.Set(x.Key, null, x.Key.Map.LocalTime.TurnCounter);   // Lost faith there was anything there
          continue;
        }
        var ub = allItems.Count;
        var loc_ok = false;
        while (0 < ub) {
          var itemsAt = allItems[--ub];
          if (ok(itemsAt)) loc_ok = true;
        }
        if (loc_ok) ret.Add(x.Key, x.Value);
        else {
          var staging = new HashSet<Gameplay.GameItems.IDs>(allItems[0].Items.Select(x => x.InventoryMemoryID));
          ub = allItems.Count;
          while (1 < ub) {
            var itemsAt = allItems[--ub];
            staging.UnionWith(itemsAt.Items.Select(x => x.InventoryMemoryID));
          }
          it_memory.Set(x.Key, staging, x.Key.Map.LocalTime.TurnCounter);   // extrasensory perception update
        }
      }
      return 0 < ret.Count ? ret : null;
    }

    public HashSet<Point>? WhereIs(IEnumerable<Gameplay.GameItems.IDs> src, Map map) {
      var it_memory = ItemMemory;
      if (null == it_memory) return null;
      var ret = new HashSet<Point>();
      bool IsInHere(Location loc) { return loc.Map == map; }
      foreach(Gameplay.GameItems.IDs it in src) {
        var tmp = it_memory.WhereIs(it, IsInHere);
        if (null == tmp) continue;
        tmp.OnlyIf(loc => !m_Actor.StackIsBlocked(in loc));
        if (0 >= tmp.Count) continue;
        var it_model = (0 <= (int)it) ? Gameplay.GameItems.From(it) : null; // exclude synthetic item models
        // cheating post-filter: reject boring entertainment
        if (it_model is ItemEntertainmentModel ent) {
          tmp = Filter(tmp, inv => null != inv.GetFirstByModel<ItemEntertainment>(ent, e => !e.IsBoringFor(m_Actor)));
          if (null == tmp) continue;
        }
        // cheating post-filter: reject dead flashlights at full inventory (these look useless as items but the type may not be useless)
        if (m_Actor.Inventory.IsFull) {
          if (it_model is ItemLightModel || it_model is ItemTrackerModel) {   // want to say "the item type this model is for, is BatteryPowered" without thrashing garbage collector
            tmp = Filter(tmp, inv => {
              var test = inv.GetFirstByModel(it_model);
              if (null == test) return false;
              if (!test.IsUseless) return true;
              return null != inv.GetFirstByModel<Item>(it_model, obj => !obj.IsUseless);
            });
            if (null == tmp) continue;
          }
        }
        ret.UnionWith(tmp.Keys.Select(Location.pos));
      }
      // XXX need to ask allies where hey are headed for (or are), to avoid traffic jams
      return ret;
    }

    public abstract List<Percept> UpdateSensors();

    // vision
#nullable enable
    public abstract HashSet<Point> FOV { get; }
    public abstract Location[] FOVloc { get; }
    public virtual void eventFOV() { }

    public abstract Dictionary<Location, Actor>? friends_in_FOV { get; }
    public abstract Dictionary<Location, Actor>? enemies_in_FOV { get; }
    public virtual Dictionary<Location, Inventory>? items_in_FOV { get { return null; } }

    public virtual bool IsEngaged { get {
      return null!=enemies_in_FOV;
    } }

    public virtual bool InCombat { get { return IsEngaged; } }

    public abstract bool IsMyTurn();
    /// <returns>null, or an action x for which x.IsPerformable() is true</returns>
    public virtual ActorAction? ExecAryZeroBehavior(int code) { return null; }
#nullable restore

    public bool CanSee(in Location x)  // correctness requires Location being value-copied
    {
      if (null == x.Map) return false;     // convince Duckman to not superheroically crash many games on turn 0
      return 0 <= Array.IndexOf(FOVloc, x);
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
    public bool IsVisibleTo(Actor actor) => actor == m_Actor || IsVisibleTo(actor.Location);
    public string VisibleIdentity(Actor actor) => IsVisibleTo(actor) ? actor.TheName : "someone";
    public string VisibleIdentity(MapObject mapObj) => IsVisibleTo(mapObj.Location) ? mapObj.TheName : "something";

    public abstract ActorAction? GetAction();
#nullable restore

    /// <returns>number of turns of trap activation it takes to kill, or int.MaxValue for no known problem</returns>
    public virtual int FastestTrapKill(in Location loc) { return int.MaxValue; }   // z are unaware of deathtraps.  \todo override for dogs
  }
}
