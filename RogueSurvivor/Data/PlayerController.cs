// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.PlayerController
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Gameplay.AI;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using Zaimoni.Data;

using Point = Zaimoni.Data.Vector2D_short;
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
        (viewpoint.Controller as PlayerController)?.InstallAfterAction(new UITracking(who));
        var game = RogueForm.Game;
        game.PlayEventMusic(music);
        game.ClearMessages();
        game.AddMessage(new Data.Message(msg, Session.Get.WorldTime.TurnCounter, Color.Yellow));
        game.AddMessagePressEnter();
        return true;
    }
  }

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
    private readonly Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int> m_itemMemory;
    private readonly List<Data.Message> m_MsgCache = new List<Data.Message>();

    private static List<EventUnconditional>? s_BeforeAction;
    private static List<EventUnconditional>? s_AfterAction;

	public PlayerController(Actor src) : base(src) {
      m_LOSSensor = new Gameplay.AI.Sensors.LOSSensor(VISION_SEES(), src);   // deal with vision capabilities
      m_itemMemory = m_Actor.IsFaction(Gameplay.GameFactions.IDs.ThePolice) ? Session.Get.PoliceItemMemory
                                                                            : new Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int>();
    }

    public void DeferMessage(Data.Message x) { m_MsgCache.Add(x); }
    public void DeferMessages(IEnumerable<Data.Message> x) {
      foreach(Data.Message msg in x) m_MsgCache.Add(msg);
    }
    public List<Data.Message> ReleaseMessages() {
      if (0 >= m_MsgCache.Count) return null;
      var ret = new List<Data.Message>(m_MsgCache);
      m_MsgCache.Clear();
      return ret;
    }

    // forwarder system for to RogueGame::AddMessage
    public override void AddMessage(Data.Message msg) {
      if (RogueGame.IsPlayer(m_Actor)) RogueForm.Game.AddMessage(msg);
      else DeferMessage(msg);
    }

    public void AddMessages(IEnumerable<Data.Message> msgs) {
      if (RogueGame.IsPlayer(m_Actor)) RogueForm.Game.AddMessages(msgs);
      else DeferMessages(msgs);
    }

    public override void AddMessageForceRead(Data.Message msg) {
      if (RogueGame.IsPlayer(m_Actor)) {
        var game = RogueForm.Game;
        game.ClearMessages();
        game.AddMessage(msg);
        game.AddMessagePressEnter();
      } else DeferMessage(msg);
    }

    public override void AddMessageForceReadClear(Data.Message msg) {
      if (RogueGame.IsPlayer(m_Actor)) {
        var game = RogueForm.Game;
        game.ClearMessages();
        game.AddMessage(msg);
        game.AddMessagePressEnter();
        game.ClearMessages();
      } else DeferMessage(msg);
    }

    public override Zaimoni.Data.Ary2Dictionary<Location, Gameplay.GameItems.IDs, int>? ItemMemory { get { return m_itemMemory; } }

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
      s_BeforeAction = new List<EventUnconditional>();
      // set up cosmetic UI handlers
      s_BeforeAction.Add(new UIOnSighting(Session.Get.UniqueActors.TheSewersThing.TheActor, "Hey! What's that THING!?", Gameplay.GameMusics.FIGHT));
      s_BeforeAction.Add(new UIOnSighting(Session.Get.UniqueActors.JasonMyers.TheActor, "Nice axe you have there!"));

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
      (s_AfterAction ?? (s_AfterAction = new List<EventUnconditional>())).Add(e);
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
      if (aggressor.IsFaction(Gameplay.GameFactions.IDs.ThePolice) && m_Actor.IsFaction(Gameplay.GameFactions.IDs.TheCHARCorporation) && 1 > Session.Get.ScriptStage_PoliceCHARrelations) {
        // same technical issues as DoMakeEnemyOfCop
        Session.Get.ScriptStage_PoliceCHARrelations = 1;
//      GameFactions.ThePolice.AddEnemy(GameFactions.TheCHARCorporation);   // works here, but parallel when loading the game doesn't
        RogueGame.DamnCHARtoPoliceInvestigation();
        return "Just following ORDERS!";
      }
      return null;
    }

#nullable enable
    public override ActorAction? GetAction()
    {
      throw new InvalidOperationException("do not call PlayerController.GetAction()");
    }

    protected override ActorAction? SelectAction()
    {
      throw new InvalidOperationException("do not call PlayerController.SelectAction()");
    }
#nullable restore

    // originally in Actor
    private string ReasonCantGetFromContainer(Location loc)
    {
      var obj = loc.MapObject;
      if (null == obj || !obj.IsContainer) return "object is not a container";
      if (!obj.Inventory.IsEmpty) return "";
      if (loc.Items?.IsEmpty ?? true) return "nothing to take there";
      return "";
    }

    private string ReasonCantGetFromContainer(Point position)
    {
      return ReasonCantGetFromContainer(new Location(m_Actor.Location.Map, position));
    }

	public bool CanGetFromContainer(Location loc, out string reason)
	{
	  reason = ReasonCantGetFromContainer(loc);
	  return string.IsNullOrEmpty(reason);
	}

	public bool CanGetFromContainer(Point position)
	{
	  return string.IsNullOrEmpty(ReasonCantGetFromContainer(position));
	}

	public bool CanGetFromContainer(Location loc)
	{
	  return string.IsNullOrEmpty(ReasonCantGetFromContainer(loc));
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

    public void WalkTo(in Location loc, int n = int.MaxValue)
    {   // triggered from far look mode
      Objectives.Insert(0,new Goal_PathTo(Session.Get.WorldTime.TurnCounter, m_Actor, in loc, true, n));
    }

    public void WalkTo(IEnumerable<Location> locs, int n = int.MaxValue)
    {
      Objectives.Insert(0,new Goal_PathTo(Session.Get.WorldTime.TurnCounter, m_Actor, locs, true, n));
    }

    public void RunTo(in Location loc, int n = int.MaxValue)
    {   // triggered from far look mode
      Objectives.Insert(0,new Goal_PathTo(Session.Get.WorldTime.TurnCounter, m_Actor, in loc, false, n));
    }

    public void RunTo(IEnumerable<Location> locs, int n = int.MaxValue)
    {
      Objectives.Insert(0,new Goal_PathTo(Session.Get.WorldTime.TurnCounter, m_Actor, locs, false, n));
    }

    public List<string> GetValidSelfOrders()
    { 
      var ret = new List<string>();
      bool in_combat = (null!=m_Actor.Controller.enemies_in_FOV);   // not using InCombat getter as we don't want to be that draconian

      if (!in_combat) {
      if (m_Actor.IsTired && null == enemies_in_FOV) ret.Add("Rest in place");

      ItemMedicine stim = (m_Actor?.Inventory.GetBestDestackable(Models.Items[(int)Gameplay.GameItems.IDs.MEDICINE_PILLS_STA]) as ItemMedicine);
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
          if (Actor.STAMINA_MIN_FOR_ACTIVITY+MapObject.CAR_WEIGHT < threshold) threshold = Actor.STAMINA_MIN_FOR_ACTIVITY + MapObject.CAR_WEIGHT;   // no-op at 30 turns/hour, but not at 900 turns/hour
          if (m_Actor.StaminaPoints < threshold && null == enemies_in_FOV) ret.Add("Brace for pushing car in place");
        }
      }

      var generators = m_Actor.Location.Map.PowerGenerators.Get.Where(power => Rules.IsAdjacent(m_Actor.Location,power.Location)).ToList();
      if (0 < generators.Count) {
        var lights = m_Actor?.Inventory.GetItemsByType<ItemLight>(it => it.MaxBatteries-1>it.Batteries);
        var trackers = m_Actor?.Inventory.GetItemsByType<ItemTracker>(it => Gameplay.GameItems.IDs.TRACKER_POLICE_RADIO != it.Model.ID && it.MaxBatteries - 1 > it.Batteries);
        if (0 < (lights?.Count ?? 0) || 0 < (trackers?.Count ?? 0)) ret.Add("Recharge everything to full");
      }
      if (null != TurnOnAdjacentGenerators()) ret.Add("Turn on all adjacent generators");

      if (m_Actor.IsTired) ret.Add("Rest rather than lose turn when tired");

      var corpses_at = m_Actor.Location.Map.GetCorpsesAt(m_Actor.Location.Position);
      if (null != corpses_at) {
        foreach(Corpse c in corpses_at) ret.Add("Butcher "+c.ToString());
      }

      Objective test = new Goal_MedicateSLP(m_Actor);
      if (test.UrgentAction(out ActorAction testAction) && null!=testAction) ret.Add("Medicate sleep");
      test = new Goal_MedicateHP(m_Actor);
      if (test.UrgentAction(out testAction) && null!=testAction) ret.Add("Medicate HP");
      } // if (!in_combat)
      return ret;
    }

    public bool InterpretSelfOrder(int i, List<string> orders)
    {
      if (orders[i].StartsWith("Butcher corpse of ")) {
        var corpses_at = m_Actor.Location.Map.GetCorpsesAt(m_Actor.Location.Position);
#if DEBUG
        if (null == corpses_at) throw new InvalidOperationException("no corpses here");
#else
        if (null == corpses_at) return false;
#endif
        string target_name = orders[i].Substring(18);
        foreach(Corpse c in corpses_at) {
          if (target_name == c.DeadGuy.Name) {
            Objectives.Insert(0,new Goal_Butcher(Session.Get.WorldTime.TurnCounter, m_Actor,c));
            return true;
          }
        }
#if DEBUG
        throw new InvalidOperationException("requested corpse not here");
#else
        return false;
#endif

      }

      switch(orders[i])
      {
      case "Rest in place":
        Objectives.Insert(0,new Goal_RecoverSTA(Session.Get.WorldTime.TurnCounter,m_Actor,Actor.STAMINA_MIN_FOR_ACTIVITY));
        return true;
      case "Brace for pushing car in place":
        {
        ItemMedicine stim = (m_Actor?.Inventory.GetBestDestackable(Models.Items[(int)Gameplay.GameItems.IDs.MEDICINE_PILLS_STA]) as ItemMedicine);
        if (null == stim) return false; // actually invariant failure
        int threshold = m_Actor.MaxSTA-m_Actor.ScaleMedicineEffect(stim.StaminaBoost)+2;
        // currently all wrecked cars have weight 100
        if (Actor.STAMINA_MIN_FOR_ACTIVITY+MapObject.CAR_WEIGHT < threshold) threshold = Actor.STAMINA_MIN_FOR_ACTIVITY + MapObject.CAR_WEIGHT;   // no-op at 30 turns/hour, but not at 900 turns/hour
        Objectives.Insert(0,new Goal_RecoverSTA(Session.Get.WorldTime.TurnCounter,m_Actor, threshold));
        }
        return true;
      case "Recharge everything to full":
        Objectives.Insert(0,new Goal_RechargeAll(m_Actor));
        return true;
      case "Rest rather than lose turn when tired":
        Objectives.Insert(0,new Goal_RestRatherThanLoseturnWhenTired(m_Actor));
        return true;
      case "Turn on all adjacent generators":
        Objectives.Insert(0,new Goal_NonCombatComplete(m_Actor.Location.Map.LocalTime.TurnCounter, m_Actor, new ActionSequence(m_Actor, new int[] { (int)ZeroAryBehaviors.TurnOnAdjacentGenerators_ObjAI })));
        return true;
      case "Medicate sleep":
        Objectives.Insert(0,new Goal_MedicateSLP(m_Actor));
        return true;
      case "Medicate HP":
        Objectives.Insert(0,new Goal_MedicateHP(m_Actor));
        return true;
      default: return false;  // automatic failure
      }
    }

    private void _raidMsg(Data.Message desc_msg, Data.Message where_msg, string music)
    {
        if (RogueGame.IsPlayer(m_Actor)) {
            var game = RogueForm.Game;
            game.PlayEventMusic(music);
            game.ClearMessages();
            AddMessage(desc_msg);
            AddMessage(where_msg);
            game.AddMessagePressEnter();
            game.ClearMessages();
        } else {
            DeferMessage(desc_msg);
            DeferMessage(where_msg);
        }
    }

    protected override void _onRaid(RaidType raid, in Location loc)
    {
      if (!m_Actor.Model.DefaultController.IsSubclassOf(typeof(Gameplay.AI.OrderableAI))) return;
      var turn = Session.Get.WorldTime.TurnCounter;
      switch (raid)
      {
      case RaidType.NATGUARD:
        _raidMsg(new Data.Message("A National Guard squad has arrived!", turn, Color.LightGreen),
                 MakeCentricMessage("Soldiers seem to come from", loc), Gameplay.GameMusics.ARMY);
        // XXX should be district event
        m_Actor.ActorScoring.AddEvent(turn, "A National Guard squad arrived at " + loc.Map.District.ToString()+".");
        break;
      case RaidType.ARMY_SUPLLIES:
        _raidMsg(new Data.Message("An Army chopper has dropped supplies!", turn, Color.LightGreen),
                 MakeCentricMessage("The drop point seems to be", loc), Gameplay.GameMusics.ARMY);
        // XXX should be district event
        m_Actor.ActorScoring.AddEvent(turn, "An army chopper dropped supplies in " + loc.Map.District.ToString()+".");
        break;
      case RaidType.BIKERS:
        _raidMsg(new Data.Message("You hear the sound of roaring engines!", turn, Color.LightGreen),
                 MakeCentricMessage("Motorbikes seem to come from", loc), Gameplay.GameMusics.BIKER);
        // XXX should be district event
        m_Actor.ActorScoring.AddEvent(turn, "Bikers raided " + loc.Map.District.ToString()+".");
        break;
      case RaidType.GANGSTA:
        _raidMsg(new Data.Message("You hear obnoxious loud music!", turn, Color.LightGreen),
                 MakeCentricMessage("Cars seem to come from", loc), Gameplay.GameMusics.GANGSTA);
        // XXX should be district event
        m_Actor.ActorScoring.AddEvent(turn, "Gangstas raided " + loc.Map.District.ToString()+".");
        break;
      case RaidType.BLACKOPS:
        _raidMsg(new Data.Message("You hear a chopper flying over the city!", turn, Color.LightGreen),
                 MakeCentricMessage("The chopper has dropped something", loc), Gameplay.GameMusics.ARMY);
        // XXX should be district event
        m_Actor.ActorScoring.AddEvent(turn, "BlackOps raided " + loc.Map.District.ToString()+".");
        break;
      case RaidType.SURVIVORS:
        _raidMsg(new Data.Message("You hear shooting and honking in the distance.", turn, Color.LightGreen),
                 MakeCentricMessage("A van has stopped", loc), Gameplay.GameMusics.SURVIVORS);
        // XXX should be district event
        m_Actor.ActorScoring.AddEvent(turn, "A Band of Survivors entered "+loc.Map.District.ToString()+".");
        break;
      }
    }

    public override bool IsInterestingTradeItem(Actor speaker, Item offeredItem)
    {
      if (Gameplay.GameItems.IDs.TRACKER_POLICE_RADIO == offeredItem.Model.ID && m_Actor.IsFaction(Gameplay.GameFactions.IDs.ThePolice)) return false; // very selective extraction from ItIsUseleess
      return true;
    }

#nullable enable
    protected override ActorAction? BehaviorWouldGrabFromStack(in Location loc, Inventory? stack)
    {
      if (CanGetFromContainer(loc)) return new Engine.Actions.ActionGetFromContainer(this, loc);
      return null;
    }
#nullable restore

    // while the following is "valid" for any actor, messages are shown *only* to the player
    public Data.Message MakeCentricMessage(string eventText, in Location loc, Color? color=null)
    {
      Location? test = m_Actor.Location.Map.Denormalize(in loc);
      if (null == test) throw new ArgumentNullException(nameof(test));
      var v = test.Value.Position - m_Actor.Location.Position;
      string msg_text = string.Format("{0} {1} tiles to the {2}.", eventText, (int)Rules.StdDistance(in v), Direction.ApproximateFromVector(v));
      if (null != color) return new Data.Message(msg_text, Session.Get.WorldTime.TurnCounter, color.Value);
      return new Data.Message(msg_text, Session.Get.WorldTime.TurnCounter);
    }

    private void HandleSay(object sender, Actor.SayArgs e)
    {
      Actor speaker = (sender as Actor);
      if (null == speaker) throw new ArgumentNullException(nameof(sender));
      if (null == e._target) throw new ArgumentNullException("e.target");
      lock (speaker) {
        if (e.shown) return;
        if (m_Actor.IsSleeping) return;
        if (!CanSee(speaker.Location) && !CanSee(e._target.Location)) return;
        if (m_Actor!= e._target && e._target.IsPlayer) return;
        e.shown = true;
      }
      var game = RogueForm.Game;
      game.PanViewportTo(m_Actor);

      if (e._important) game.ClearMessages();
      game.AddMessages(e.messages);
      if (!e._important) return;

      game.AddOverlay(new RogueGame.OverlayRect(Color.Yellow, new System.Drawing.Rectangle(RogueGame.MapToScreen(speaker.Location), RogueGame.SIZE_OF_ACTOR)));
      game.AddMessagePressEnter();
      game.ClearOverlays();
      game.RedrawPlayScreen();
    }
  }
}
