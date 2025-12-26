// Decompiled with JetBrains decompiler
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

using Point = Zaimoni.Data.Vector2D<short>;
using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

#nullable enable

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  public abstract class ActorController
  {
    protected readonly Actor m_Actor;

    protected ActorController(Actor src) { m_Actor = src; }

    public virtual void RepairLoad() { }

    public Actor ControlledActor { get { return m_Actor; } } // alpha10
    public virtual void TakeControl() {}
    public virtual void LeaveControl() {}

    public virtual Gameplay.GameGangs.IDs GangID { get { return Gameplay.GameGangs.IDs.NONE; } }

#region UI messages
    // forwarder system for to RogueGame::AddMessage
    public virtual void AddMessageForceRead(UI.Message msg, List<PlayerController> witnesses) {
      if (0 < witnesses.Count) {
        bool rendered = false;
        foreach(var witness in witnesses) if (witness.AddMessage(msg)) rendered = true;
        if (!rendered) RogueGame.Game.PanViewportTo(witnesses);
        return;
      }
    }

    public virtual void AddMessageForceReadClear(UI.Message msg, List<PlayerController> witnesses) {
      if (0 < witnesses.Count) {
        bool rendered = false;
        foreach(var witness in witnesses) if (witness.AddMessage(msg)) rendered = true;
        if (!rendered) RogueGame.Game.PanViewportTo(witnesses);
        return;
      }
    }

    // check-in with leader
    public virtual bool ReportBlocked(in Data.Model.InvOrigin src, Actor who) { return true; }
    public virtual bool ReportGone(in Data.Model.InvOrigin src, Actor who) { return true; }
    public virtual bool ReportNotThere(in Data.Model.InvOrigin src, Gameplay.Item_IDs what, Actor who) { return true; }
    public virtual bool ReportTaken(in Data.Model.InvOrigin src, Item it, Actor who) { return true; }
#endregion

    public virtual Zaimoni.Data.Ary2Dictionary<Location, Gameplay.Item_IDs, int>? ItemMemory {
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

    public List<Gameplay.Item_IDs>? WhatHaveISeen() { return ItemMemory?.WhatHaveISeen(); }
    public Dictionary<Location, int>? WhereIs(Gameplay.Item_IDs x) { return ItemMemory?.WhereIs(x); }

    virtual public IEnumerable<Gameplay.Item_IDs>? RejectUnwanted(IEnumerable<Gameplay.Item_IDs>? src, Location loc) { return src; }

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
          if (ok(itemsAt.Inventory)) loc_ok = true;
        }
        if (loc_ok) ret.Add(x.Key, x.Value);
        else {
          HashSet<Gameplay.Item_IDs> staging = new(allItems[0].Inventory.Select(x => x.InventoryMemoryID));
          ub = allItems.Count;
          while (1 < ub) {
            var itemsAt = allItems[--ub];
            staging.UnionWith(itemsAt.Inventory.Select(x => x.InventoryMemoryID));
          }
          it_memory.Set(x.Key, staging, x.Key.Map.LocalTime.TurnCounter);   // extrasensory perception update
        }
      }
      return 0 < ret.Count ? ret : null;
    }

    public HashSet<Point>? WhereIs(IEnumerable<Gameplay.Item_IDs> src, Map map) {
      var it_memory = ItemMemory;
      if (null == it_memory) return null;
      var ret = new HashSet<Point>();
      bool IsInHere(Location loc) { return loc.Map == map; }
      foreach(var it in src) {
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
    public virtual Dictionary<Location, Data.Model.InvOrigin>? items_in_FOV { get => null; }

    public List<KeyValuePair<double, List<Data.Model.CombatActor>>>? model_now { get {
      List<Data.Model.CombatActor> stage = new();

      var who = friends_in_FOV;
      if (null != who) foreach(var a in who) stage.Add(new(a.Value));

      who = enemies_in_FOV;
      if (null != who) foreach(var a in who) stage.Add(new(a.Value));

      if (0 >= stage.Count) return null;

      stage.Sort(Data.Model.CombatActor.CompareAP);
      stage.Insert(0, new(m_Actor));

      List<KeyValuePair<double, List<Data.Model.CombatActor>>> ret = new();
      ret.Add(new(1, stage));
      return ret;
    } }

    public virtual bool IsEngaged { get => null!=enemies_in_FOV; }
    public virtual bool InCombat { get => IsEngaged; }

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
      Location loc = new (map, position);
      if (!Map.Canonical(ref loc)) return false;
      return 0 <= Array.IndexOf(FOVloc, loc);
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
    public string VisibleIdentity<T>(T x) where T:ILocation,INoun => IsVisibleTo(x.Location) ? x.TheName : x.UnknownPronoun;

    public abstract ActorAction? GetAction();

    // for _Action.Choice
    public virtual ActorAction Choose(List<ActorAction> src) => Rules.Get.DiceRoller.Choose(src);

#nullable restore

    /// <returns>number of turns of trap activation it takes to kill, or int.MaxValue for no known problem</returns>
    public virtual int FastestTrapKill(in Location loc) { return int.MaxValue; }   // z are unaware of deathtraps.  \todo override for dogs
  }
}
