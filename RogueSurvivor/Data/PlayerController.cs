// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.PlayerController
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Gameplay.AI;
using djack.RogueSurvivor.UI;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using Zaimoni.Data;
using static Zaimoni.Data.Functor;

using Point = Zaimoni.Data.Vector2D<short>;
using Rectangle = Zaimoni.Data.Box2D<short>;
using Color = System.Drawing.Color;
using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;
using ItemLight = djack.RogueSurvivor.Engine.Items.ItemLight;
using ItemMedicine = djack.RogueSurvivor.Engine.Items.ItemMedicine;
using ItemTracker = djack.RogueSurvivor.Engine.Items.ItemTracker;
using ActionSequence = djack.RogueSurvivor.Engine.Actions.ActionSequence;

namespace djack.RogueSurvivor.Data
{
  internal interface EventUnconditional // maybe more general
  {
    bool Expire(Actor viewpoint);
  }

  [Serializable]
  internal class UIOnSighting : EventUnconditional
  {
    private bool expired = false;
    private readonly Actor who;
    private readonly string msg;
    private readonly string? music;

    public UIOnSighting(Actor _who, string _msg, string? _music = null)
    {
        who = _who;
        msg = _msg;
        music = _music;
    }

    public bool Expire(Actor viewpoint)
    {
        if (expired || who.IsDead) return true;
        if (null == viewpoint.Sees(who)) return false;
        expired = true; // just in case of severe multithreading bug
        if (viewpoint.Controller is PlayerController pc) {
            pc.InstallAfterAction(new UITracking(who));
            var game = RogueGame.Game;
            game.PlayEventMusic(music);
            pc.Messages.Clear();
            pc.Messages.Add(new Message(msg, Session.Get.WorldTime.TurnCounter, Color.Yellow));
            game.AddMessagePressEnter(pc);
        }
        return true;
    }
  }

  [Serializable]
  internal class UITracking : EventUnconditional
  {
    private readonly Actor who;

    public UITracking(Actor _who) { who = _who; }

    public bool Expire(Actor viewpoint) { return who.IsDead; }

    public bool IsTarget(Actor x) { return x == who; }
  }

  [Serializable]
  internal class PlayerController : ObjectiveAI
    {
    private readonly Gameplay.AI.Sensors.LOSSensor m_LOSSensor;
    private readonly Zaimoni.Data.Ary2Dictionary<Location, Gameplay.Item_IDs, int> m_itemMemory;
    private List<Waypoint_s>? m_Waypoints = null;
    public readonly MessageManager Messages;

    private static List<EventUnconditional>? s_BeforeAction;
    private static List<EventUnconditional>? s_AfterAction;

	public PlayerController(Actor src, PlayerController? upgrading = null) : base(src) {
      m_LOSSensor = new Gameplay.AI.Sensors.LOSSensor(VISION_SEES(), src);   // deal with vision capabilities
      m_itemMemory = m_Actor.IsFaction(GameFactions.IDs.ThePolice) ? Session.Get.Police.ItemMemory
                                                                   : new();

      const int MESSAGES_SPACING = RogueGame.LINE_SPACING;
      const int MAX_MESSAGES = (RogueGame.CANVAS_HEIGHT - RogueGame.LOCATIONPANEL_Y) / MESSAGES_SPACING; // historically 7
      const int MESSAGES_HISTORY = (RogueGame.CANVAS_HEIGHT - 3 * RogueGame.BOLD_LINE_SPACING) / MESSAGES_SPACING; // historically 59

      Messages = upgrading?.Messages ?? new MessageManager(MESSAGES_SPACING, MESSAGES_HISTORY, MAX_MESSAGES);
    }

#region UI messages
    // forwarder system for to RogueGame::AddMessage
    public bool AddMessage(Message msg) {
        Messages.Add(msg);
        if (RogueGame.IsPlayer(m_Actor)) {
          RogueGame.Game.RedrawPlayScreen();
          return true;
        }
        return false;
    }
    public bool AddMessages(IEnumerable<Message> msgs) {
        Messages.Add(msgs);
        if (RogueGame.IsPlayer(m_Actor)) {
          RogueGame.Game.RedrawPlayScreen();
          return true;
        }
        return false;
    }

    public void AddMessageForceRead(Message msg) {
      if (RogueGame.IsPlayer(m_Actor) && !RogueGame.IsSimulating) {
        Messages.Clear();
        Messages.Add(msg);
        RogueGame.Game.AddMessagePressEnter(this);
      } else Messages.Add(msg);
    }

    public override void AddMessageForceRead(UI.Message msg, KeyValuePair<List<PlayerController>, List<Actor>> witnesses) {
      bool have_panned = false;
      if (!RogueGame.IsSimulating) {
        if (witnesses.Key.Remove(this)) {
          Messages.Clear();
          Messages.Add(msg);
          RogueGame.Game.AddMessagePressEnter(this);
          have_panned = true;
          if (0 >= witnesses.Key.Count) return;
        }
      }

      if (0 < witnesses.Key.Count) {
        foreach(var witness in witnesses.Key) witness.AddMessage(msg);
        if (!RogueGame.IsSimulating) {
          if (!have_panned) RogueGame.Game.PanViewportTo(witnesses.Key);
          have_panned = true;
        }
      }

      if (have_panned || 0 >= witnesses.Value.Count || RogueGame.IsSimulating) return;
      RogueGame.Game.PanViewportTo(witnesses.Value);
      RogueGame.Game.RedrawPlayScreen(msg);
    }

    public void AddMessagesForceRead(IEnumerable<Message> msgs) {
      if (RogueGame.IsPlayer(m_Actor) && !RogueGame.IsSimulating) {
        Messages.Clear();
        Messages.Add(msgs);
        RogueGame.Game.AddMessagePressEnter(this);
      } else Messages.Add(msgs);
    }

    public override void AddMessageForceReadClear(Message msg, KeyValuePair<List<PlayerController>, List<Actor>> witnesses) {
      bool have_panned = false;
      if (!RogueGame.IsSimulating) {
        if (witnesses.Key.Remove(this)) {
          Messages.Clear();
          Messages.Add(msg);
          RogueGame.Game.AddMessagePressEnter(this);
          have_panned = true;
          if (0 >= witnesses.Key.Count) return;
        }
      }

      if (0 < witnesses.Key.Count) {
        foreach(var witness in witnesses.Key) witness.AddMessage(msg);
        if (!RogueGame.IsSimulating) {
          if (!have_panned) RogueGame.Game.PanViewportTo(witnesses.Key);
          have_panned = true;
        }
      }

      if (have_panned || 0 >= witnesses.Value.Count || RogueGame.IsSimulating) return;
      RogueGame.Game.PanViewportTo(witnesses.Value);
      RogueGame.Game.RedrawPlayScreen(msg);
    }

    private void _handleReport(string raw_text, int code, Actor who)
    {
      if (1 == code) {
        who.Say(ControlledActor, raw_text, RogueGame.Sayflags.IS_FREE_ACTION);
      }
      if (2 == code) {
        Message msg = new("(police radio, "+ who.Name +") "+raw_text, Session.Get.WorldTime.TurnCounter, RogueGame.SAYOREMOTE_NORMAL_COLOR);
        Action<PlayerController> pc_add_msg = pc => pc.AddMessage(msg);
        who.MessageAllInDistrictByRadio(NOP, FALSE, pc_add_msg, pc_add_msg, TRUE);
      }
      // defer army radio and cell phones for now
    }

    // check-in with leader
    public override bool ReportBlocked(in InvOrigin src, Actor who) {
      var code = CommunicationMethodCode(who);
      if (0 >= code) return false;

      _handleReport(src.ToString() + " is blocked.", code, who);
      return true;
    }
    public override bool ReportGone(in InvOrigin src, Actor who) {
      var code = CommunicationMethodCode(who);
      if (0 >= code) return false;

      _handleReport("Nothing at " + src.ToString() + ".", code, who);
      return true;
    }
    public override bool ReportNotThere(in InvOrigin src, Gameplay.Item_IDs what, Actor who) {
      var code = CommunicationMethodCode(who);
      if (0 >= code) return false;

      _handleReport(what.ToString()+" is not at " + src.ToString() + ".", code, who);
      return true;
    }
    public override bool ReportTaken(in InvOrigin src, Item it, Actor who)  {
      var code = CommunicationMethodCode(who);
      if (0 >= code) return false;

      _handleReport("Have taken "+it.ToString()+" from " + src.ToString() + ".", code, who);
      return true;
    }



#endregion

    public void AddWaypoint(in Location dest, string why) {
        (m_Waypoints ??= new()).Add(new(dest, why));
    }

    public bool HasWaypoint(in Location dest) {
        if (null == m_Waypoints) return false;
        foreach(var w in m_Waypoints) if (w.dest == dest) return true;
        return false;
    }

    public void RemoveWaypoint(in Location dest) {
        if (null != m_Waypoints) {
            var ub = m_Waypoints.Count;
            while(0 <= --ub) {
                if (m_Waypoints[ub].dest == dest) m_Waypoints.RemoveAt(ub);
            }
            if (0 >= m_Waypoints.Count) m_Waypoints = null;
        }
    }

    public void RemoveWaypoints(Predicate<Location> fail) {
        if (null != m_Waypoints) {
            var ub = m_Waypoints.Count;
            while(0 <= --ub) {
                if (fail(m_Waypoints[ub].dest)) m_Waypoints.RemoveAt(ub);
            }
            if (0 >= m_Waypoints.Count) m_Waypoints = null;
        }
    }

    public IReadOnlyList<Waypoint_s>? Waypoints { get { return m_Waypoints; } }


    public void Exec_RangedAttack(Actor currentTarget, List<Point> LoF, Engine.Actions.FireMode mode)
    {
        var ra = new Engine.Actions.ActionRangedAttack(m_Actor, currentTarget, LoF, mode);
        ra.Perform();
        RecordLastAction(ra);
    }

    public void ResetRecoil() => _recoil = 0;

    public override Zaimoni.Data.Ary2Dictionary<Location, Gameplay.Item_IDs, int>? ItemMemory { get { return m_itemMemory; } }

    private bool ShowThis(Gameplay.Item_IDs src, in Location loc) {
      if (Gameplay.Item_IDs.TRAP_EMPTY_CAN == src) return false;
      return true;
    }

    public override IEnumerable<Gameplay.Item_IDs>? RejectUnwanted(IEnumerable<Gameplay.Item_IDs>? src, Location loc) {
      if (null == src) return null;
      List<Gameplay.Item_IDs> ret = new();
      // \todo need a UI for this (cf. Angband)
      foreach(var it in src) {
        if (ShowThis(it, in loc)) ret.Add(it);
      }
      return 0<ret.Count ? ret : null;
    }


	private Gameplay.AI.Sensors.LOSSensor.SensingFilter VISION_SEES() {
	  switch(m_Actor.Model.DefaultController.Name)
	  {
	  case nameof(CHARGuardAI): return Gameplay.AI.CHARGuardAI.VISION_SEES;
	  case nameof(CivilianAI): return Gameplay.AI.CivilianAI.VISION_SEES;
	  case nameof(FeralDogAI): return Gameplay.AI.FeralDogAI.VISION_SEES;
	  case nameof(GangAI): return Gameplay.AI.GangAI.VISION_SEES;
	  case nameof(InsaneHumanAI): return Gameplay.AI.InsaneHumanAI.VISION_SEES;
	  case nameof(RatAI): return Gameplay.AI.RatAI.VISION_SEES;
	  case nameof(SewersThingAI): return Gameplay.AI.SewersThingAI.VISION_SEES;
	  case nameof(SkeletonAI): return Gameplay.AI.SkeletonAI.VISION_SEES;
	  case nameof(SoldierAI): return Gameplay.AI.SoldierAI.VISION_SEES;
	  case nameof(ZombieAI): return Gameplay.AI.ZombieAI.VISION_SEES;
#if DEBUG
	  default: throw new InvalidOperationException("unhandled case");
#else
	  default: return Gameplay.AI.Sensors.LOSSensor.SensingFilter.ACTORS | Gameplay.AI.Sensors.LOSSensor.SensingFilter.ITEMS | Gameplay.AI.Sensors.LOSSensor.SensingFilter.CORPSES;
#endif
      }
	}

    public void InstallHandlers()
    {
      Actor.Says += HandleSay;
    }

	public override void TakeControl()
    {
      base.TakeControl();
      Actor.Says += HandleSay;
    }

    public override void LeaveControl()
    {
      base.LeaveControl();
      Actor.Says -= HandleSay;
    }

#nullable enable
    public static void Reset()
    {
      s_BeforeAction = new List<EventUnconditional>{ // set up cosmetic UI handlers
        new UIOnSighting(Session.Get.UniqueActors.TheSewersThing.TheActor, "Hey! What's that THING!?", Gameplay.GameMusics.FIGHT),
        new UIOnSighting(Session.Get.UniqueActors.JasonMyers.TheActor, "Nice axe you have there!")
      };

      s_AfterAction = new List<EventUnconditional>(s_BeforeAction); // all cosmetic handlers should be safe to execute both before/after action
      // set up material UI handlers
    }

#region Session save/load assistants
    static public void Load(SerializationInfo info, StreamingContext context)
    {
      info.read_nullsafe(ref s_BeforeAction, nameof(s_BeforeAction));
      info.read_nullsafe(ref s_AfterAction,  nameof(s_AfterAction));
    }

    static public void Save(SerializationInfo info, StreamingContext context)
    {
      info.AddValue(nameof(s_BeforeAction), s_BeforeAction);
      info.AddValue(nameof(s_AfterAction),  s_AfterAction);
    }
#endregion

    // event handlers use countdown loop to allow safely adding event handlers from within an event handler
    public void BeforeAction()
    {
      if (null != s_BeforeAction) {
        int ub = s_BeforeAction.Count;
        while(0 <= --ub) {
          if (s_BeforeAction[ub].Expire(m_Actor)) s_BeforeAction.RemoveAt(ub);
        }
        if (0 >= s_BeforeAction.Count) s_BeforeAction = null;
      }
    }

    public void AfterAction()
    {
      if (null != s_AfterAction) {
        int ub = s_AfterAction.Count;
        while(0 <= --ub) {
          if (s_AfterAction[ub].Expire(m_Actor)) s_AfterAction.RemoveAt(ub);
        }
        if (0 >= s_AfterAction.Count) s_AfterAction = null;
      }
    }

    public void InstallAfterAction(EventUnconditional e)
    {
      (s_AfterAction ??= new List<EventUnconditional>()).Add(e);
    }

    public bool KnowsWhere(Actor a)
    {
      if (!a.IsDead && null != s_AfterAction) foreach(var e in s_AfterAction) if (e is UITracking track && track.IsTarget(a)) return true;
      return false;
    }

    public override List<Percept> UpdateSensors()
    {
        var ret = m_LOSSensor.Sense();
        if (null == enemies_in_FOV) AdviseFriendsOfSafety();  // XXX works even when fleeing from explosives

        // function extraction target
        Span<bool> find_us = stackalloc bool[(int)Engine.Items.ItemTrackerModel.TrackingOffset.STRICT_UB];
        m_Actor.Tracks(ref find_us);
        var threat = m_Actor.Threats;

        if (find_us[(int)Engine.Items.ItemTrackerModel.TrackingOffset.UNDEADS]) {
            var scan = new ZoneLoc(m_Actor.Location.Map, new Rectangle(m_Actor.Location.Position - (Point)Rules.ZTRACKINGRADIUS, (Point)(2 * Rules.ZTRACKINGRADIUS + 1)));
            if (null != threat) {
                var could_find = threat.ThreatAt(scan, a => a.Model.Abilities.IsUndead);
                var reject = new List<Actor>();
                foreach (var x in could_find.Keys) {
                    if (scan.ContainsExt(x.Location)) {
                        threat.Sighted(x, x.Location);
                        reject.Add(x);
                    }
                }
                foreach (var actor in reject) could_find.Remove(actor);
                if (0 < could_find.Count) threat.Cleared(could_find);
            }
        }
        // end function extraction target

        return ret;
    }

    public override HashSet<Point> FOV { get { return m_LOSSensor.FOV; } }
    public override Location[] FOVloc { get { return m_LOSSensor.FOVloc; } }

    public override Dictionary<Location, Actor>? friends_in_FOV { get { return m_LOSSensor.friends; } }
    public override Dictionary<Location, Actor>? enemies_in_FOV { get { return m_LOSSensor.enemies; } }
    public override Dictionary<Location, Inventory>? items_in_FOV { get { return m_LOSSensor.items; } }
#nullable restore

    // if the underlying controller has a non-default behavior we do want that here
    // i.e. a full migration of legacy code merely ends up relocating the complexity; rationale needed to proceed
    public override string AggressedBy(Actor aggressor)
    {
      if (aggressor.IsFaction(GameFactions.IDs.ThePolice) && m_Actor.IsFaction(GameFactions.IDs.TheCHARCorporation) && 1 > Session.Get.ScriptStage_PoliceCHARrelations) {
        // same technical issues as DoMakeEnemyOfCop
        Session.Get.ScriptStage_PoliceCHARrelations = 1;
//      GameFactions.ThePolice.AddEnemy(GameFactions.TheCHARCorporation);   // works here, but parallel when loading the game doesn't
        RogueGame.DamnCHARtoPoliceInvestigation();
        return "Just following ORDERS!";
      }
      return null;
    }

#nullable enable
    public override ActorAction? GetAction() => throw new InvalidOperationException("do not call PlayerController.GetAction()");
    protected override ActorAction? SelectAction() => throw new InvalidOperationException("do not call PlayerController.SelectAction()");

    public override ActorAction? Choose(List<ActorAction> src) {
        if (1 < src.Count) {
            ActorAction? ret = null;
            string label(int index) { return src[index].ToString(); }
            bool details(int index) {
                ret = src[index];
                return true;
            }

            RogueGame.Game.PagedPopup(m_Actor.UnmodifiedName+"'s options", src.Count, label, details);
            return ret;
        }
        return src[0];
    }

#nullable restore

    // originally in Actor
    static private string ReasonCantGetFromContainer(Location loc)
    {
      var obj = loc.MapObject as ShelfLike;
      if (null == obj) return "object is not a container";
      if (!obj.Inventory.IsEmpty) return "";
      if (loc.Items?.IsEmpty ?? true) return "nothing to take there";
      return "";
    }

	public bool CanGetFromContainer(Point position)
	{
	  return string.IsNullOrEmpty(ReasonCantGetFromContainer(new Location(m_Actor.Location.Map, position)));
	}

    public bool AutoPilotIsOn { get { return 0 < Objectives.Count;  } }

    // This is too dangerous to provide a member function for in ObjectiveAI.
    // We duplicate this code fragment from CivilianAI::SelectAction and siblings to support a reasonable replacement 
    // for the wait command (which has been removed as a cause of 1-keystroke deaths)
    public ActorAction AutoPilot()
    {
      if (0 >= Objectives.Count) return null;
      ActorAction goal_action = null;
      _all = FilterSameMap(UpdateSensors());
      InitAICache(_all, _all);
      foreach(var o in Objectives.ToList()) {
        if (o.IsExpired) Objectives.Remove(o);
        else if (o.UrgentAction(out goal_action)) {
          if (null==goal_action) Objectives.Remove(o);
#if DEBUG
          else if (!goal_action.IsPerformable()) throw new InvalidOperationException("result of UrgentAction should be legal");
#else
          else if (!goal_action.IsPerformable()) Objectives.Remove(o);
#endif
          else {
            ScheduleFollowup(goal_action);  // OrderableAI subclasses have this handled by BaseAI::GetAction so don't need it "here"
            ResetAICache();
            return goal_action;
          }
        }
      }
      ResetAICache();
      return null;
    }

#nullable enable
    // \todo? eliminate this in favor of something more secure
    public Objective[]? CurrentSelfOrders { get { return 0 >= Objectives.Count ? null : Objectives.ToArray(); }  }
    public void Countermand(Objective o) { Objectives.Remove(o); }
#nullable restore

    public override void WalkTo(in Location loc, int n = int.MaxValue)
    {   // triggered from far look mode
      SetObjective(new Goal_PathTo(m_Actor, in loc, true, n));
    }

    public void WalkTo(IEnumerable<Location> locs, int n = int.MaxValue)
    {
      SetObjective(new Goal_PathTo(m_Actor, locs, true, n));
    }

    public void RunTo(in Location loc, int n = int.MaxValue)
    {   // triggered from far look mode
      SetObjective(new Goal_PathTo(m_Actor, in loc, false, n));
    }

    public void RunTo(IEnumerable<Location> locs, int n = int.MaxValue)
    {
      SetObjective(new Goal_PathTo(m_Actor, locs, false, n));
    }

    public List<KeyValuePair<string, Action>> GetValidSelfOrders()
    {
      List<KeyValuePair<string, Action>> ret = new();
      bool in_combat = (null!=m_Actor.Controller.enemies_in_FOV);   // not using InCombat getter as we don't want to be that draconian

      if (!in_combat) {
      if (m_Actor.IsTired && null == enemies_in_FOV) {
        ret.Add(new("Rest in place", () => {
            SetObjective(new Goal_RecoverSTA(Session.Get.WorldTime.TurnCounter, m_Actor, Actor.STAMINA_MIN_FOR_ACTIVITY));
        }));
      }

      ItemMedicine stim = (m_Actor?.Inventory.GetBestDestackable(Gameplay.GameItems.PILLS_STA) as ItemMedicine);
      if (null != stim) {
        MapObject car = null;
        foreach(var pt in m_Actor.Location.Position.Adjacent()) {
          var tmp = m_Actor.Location.Map.GetMapObjectAtExt(pt);
          if (null == tmp) continue;
          switch(tmp.ID) {
          case MapObject.IDs.CAR1:
          case MapObject.IDs.CAR2:
          case MapObject.IDs.CAR3:
          case MapObject.IDs.CAR4:
            car = tmp;
            break;
          default: continue;
          }
          break;
        }
        if (null != car) {
          int threshold = m_Actor.MaxSTA-m_Actor.ScaleMedicineEffect(stim.StaminaBoost)+2;
          // currently all wrecked cars have weight 100
          if (Actor.STAMINA_MIN_FOR_ACTIVITY+MapObject.CAR_WEIGHT < threshold) threshold = Actor.STAMINA_MIN_FOR_ACTIVITY + MapObject.CAR_WEIGHT;   // no-op at 30 turns/hour, but not at 900 turns/hour
          if (m_Actor.StaminaPoints < threshold && null == enemies_in_FOV) {
             ret.Add(new("Brace for pushing car in place",() => {
                 SetObjective(new Goal_RecoverSTA(Session.Get.WorldTime.TurnCounter, m_Actor, threshold));
             }));
          }
        }
      }

      var generators = m_Actor.Location.Map.PowerGenerators.Get.Where(power => Rules.IsAdjacent(m_Actor.Location,power.Location)).ToList();
      if (0 < generators.Count) {
        var lights = m_Actor?.Inventory.GetItemsByType<ItemLight>(it => it.MaxBatteries-1>it.Batteries);
        var trackers = m_Actor?.Inventory.GetItemsByType<ItemTracker>(it => Gameplay.Item_IDs.TRACKER_POLICE_RADIO != it.ModelID && it.MaxBatteries - 1 > it.Batteries);
        if (null != lights || null != trackers) {
          ret.Add(new("Recharge everything to full", () => {
              SetObjective(new Goal_RechargeAll(m_Actor));
          }));
        }
      }
      if (null != TurnOnAdjacentGenerators()) {
        ret.Add(new("Turn on all adjacent generators", () => {
            SetObjective(new Goal_NonCombatComplete(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, new ActionSequence(m_Actor, new int[] { (int)ZeroAryBehaviors.TurnOnAdjacentGenerators_ObjAI })));
        }));
      }

      if (m_Actor.IsTired) {
        ret.Add(new("Rest rather than lose turn when tired", () => {
            SetObjective(new Goal_RestRatherThanLoseturnWhenTired(m_Actor));
        }));
      }
      if (m_Actor.IsSleepy) {
        ret.Add(new("Rest rather than lose turn when sleepy", () => {
            SetObjective(new Goal_RestRatherThanLoseturnWhenTired(m_Actor));
        }));
      }

      var corpses_at = m_Actor.Location.Corpses;
      if (null != corpses_at) {
        foreach(Corpse c in corpses_at) {
          ret.Add(new("Butcher "+c.ToString(), () => {
              SetObjective(new Goal_Butcher(Session.Get.WorldTime.TurnCounter, m_Actor, c));
          }));
        }
      }

      var medicate_slp = new Goal_MedicateSLP(m_Actor);
      if (medicate_slp.UrgentAction(out var testAction) && null!=testAction) {
        ret.Add(new("Medicate sleep", () => {
            SetObjective(medicate_slp);
        }));
      }
      var medicate_hp = new Goal_MedicateHP(m_Actor);
      if (medicate_hp.UrgentAction(out testAction) && null!=testAction) {
        ret.Add(new("Medicate HP", () => {
            SetObjective(medicate_hp);
        }));
      }
      } // if (!in_combat)
      return ret;
    }

    private void _raidMsg(Message desc_msg, Message where_msg, string music)
    {
        if (RogueGame.IsPlayer(m_Actor) && !RogueGame.IsSimulating) {
            var game = RogueGame.Game;
            game.PlayEventMusic(music);
            Messages.Clear();
            Messages.Add(desc_msg);
            Messages.Add(where_msg);
            game.AddMessagePressEnter(this);
        } else {
            Messages.Add(desc_msg);
            Messages.Add(where_msg);
        }
    }

    protected override void _onRaid(RaidType raid, in Location loc)
    {
      if (!m_Actor.Model.DefaultController.IsSubclassOf(typeof(Gameplay.AI.OrderableAI))) return;
      var turn = Session.Get.WorldTime.TurnCounter;
      switch (raid)
      {
      case RaidType.NATGUARD:
        _raidMsg(new("A National Guard squad has arrived!", turn, Color.LightGreen),
                 MakeCentricMessage("Soldiers seem to come from", loc), Gameplay.GameMusics.ARMY);
        // XXX should be district event
        m_Actor.ActorScoring.AddEvent(turn, "A National Guard squad arrived at " + loc.Map.District.ToString()+".");
        break;
      case RaidType.ARMY_SUPLLIES:
        _raidMsg(new("An Army chopper has dropped supplies!", turn, Color.LightGreen),
                 MakeCentricMessage("The drop point seems to be", loc), Gameplay.GameMusics.ARMY);
        // XXX should be district event
        m_Actor.ActorScoring.AddEvent(turn, "An army chopper dropped supplies in " + loc.Map.District.ToString()+".");
        break;
      case RaidType.BIKERS:
        _raidMsg(new("You hear the sound of roaring engines!", turn, Color.LightGreen),
                 MakeCentricMessage("Motorbikes seem to come from", loc), Gameplay.GameMusics.BIKER);
        // XXX should be district event
        m_Actor.ActorScoring.AddEvent(turn, "Bikers raided " + loc.Map.District.ToString()+".");
        break;
      case RaidType.GANGSTA:
        _raidMsg(new("You hear obnoxious loud music!", turn, Color.LightGreen),
                 MakeCentricMessage("Cars seem to come from", loc), Gameplay.GameMusics.GANGSTA);
        // XXX should be district event
        m_Actor.ActorScoring.AddEvent(turn, "Gangstas raided " + loc.Map.District.ToString()+".");
        break;
      case RaidType.BLACKOPS:
        _raidMsg(new("You hear a chopper flying over the city!", turn, Color.LightGreen),
                 MakeCentricMessage("The chopper has dropped something", loc), Gameplay.GameMusics.ARMY);
        // XXX should be district event
        m_Actor.ActorScoring.AddEvent(turn, "BlackOps raided " + loc.Map.District.ToString()+".");
        break;
      case RaidType.SURVIVORS:
        _raidMsg(new("You hear shooting and honking in the distance.", turn, Color.LightGreen),
                 MakeCentricMessage("A van has stopped", loc), Gameplay.GameMusics.SURVIVORS);
        // XXX should be district event
        m_Actor.ActorScoring.AddEvent(turn, "A Band of Survivors entered "+loc.Map.District.ToString()+".");
        break;
      }
    }

    public override bool IsInterestingTradeItem(Actor speaker, Item offeredItem)
    {
      if (Gameplay.Item_IDs.TRACKER_POLICE_RADIO == offeredItem.ModelID && m_Actor.IsFaction(GameFactions.IDs.ThePolice)) return false; // very selective extraction from ItIsUseleess
      return true;
    }

#nullable enable
    protected override ActorAction? BehaviorWouldGrabFrom(in Location loc)
    {
      return Engine.Actions.PlayerTakeFrom.create(this, loc);
    }

    protected override ActorAction? BehaviorWouldGrabFrom(ShelfLike obj)
    {
      return Engine.Actions.PlayerTakeFrom.create(this, obj);
    }
#nullable restore

    // while the following is "valid" for any actor, messages are shown *only* to the player
    public Message MakeCentricMessage(string eventText, in Location loc, Color? color=null)
    {
      Location? test = m_Actor.Location.Map.Denormalize(in loc);
      if (null == test) throw new ArgumentNullException(nameof(test));
      var v = test.Value.Position - m_Actor.Location.Position;
      string msg_text = string.Format("{0} {1} tiles to the {2}.", eventText, (int)Rules.StdDistance(in v), Direction.ApproximateFromVector(v));
      if (null != color) return new(msg_text, Session.Get.WorldTime.TurnCounter, color.Value);
      return new(msg_text, Session.Get.WorldTime.TurnCounter);
    }

    private void HandleSay(object sender, Actor.SayArgs e)
    {
      if (!(sender is Actor speaker)) throw new ArgumentNullException(nameof(sender));
      if (null == e._target) throw new ArgumentNullException("e.target");
      lock (speaker) {
        if (e.shown) return;
        if (m_Actor.IsSleeping) return;
        if (!CanSee(speaker.Location) && !CanSee(e._target.Location)) return;
        if (m_Actor!= e._target && e._target.IsPlayer) return;
        e.shown = true;
      }

      if (!e._important) {
         AddMessages(e.messages);
      } else {
         AddMessagesForceRead(e.messages);
      }
    }
  }
}
