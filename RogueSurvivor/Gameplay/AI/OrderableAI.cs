// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.OrderableAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Engine.MapObjects;
using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal abstract class OrderableAI : BaseAI
  {
    protected Percept m_LastEnemySaw;
    protected Percept m_LastItemsSaw;
    protected Percept m_LastSoldierSaw;
    protected Percept m_LastRaidHeard;
    protected bool m_ReachedPatrolPoint;
    protected int m_ReportStage;

    public bool DontFollowLeader { get; set; }

    public override void SetOrder(ActorOrder newOrder)
    {
      base.SetOrder(newOrder);
      this.m_ReachedPatrolPoint = false;
      this.m_ReportStage = 0;
    }

    protected ActorAction ExecuteOrder(RogueGame game, ActorOrder order, List<Percept> percepts)
    {
      if (this.m_Actor.Leader == null || this.m_Actor.Leader.IsDead)
        return (ActorAction) null;
      switch (order.Task)
      {
        case ActorTasks.BARRICADE_ONE:
          return this.ExecuteBarricading(game, order.Location, false);
        case ActorTasks.BARRICADE_MAX:
          return this.ExecuteBarricading(game, order.Location, true);
        case ActorTasks.GUARD:
          return this.ExecuteGuard(game, order.Location, percepts);
        case ActorTasks.PATROL:
          return this.ExecutePatrol(game, order.Location, percepts);
        case ActorTasks.DROP_ALL_ITEMS:
          return this.ExecuteDropAllItems(game);
        case ActorTasks.BUILD_SMALL_FORTIFICATION:
          return this.ExecuteBuildFortification(game, order.Location, false);
        case ActorTasks.BUILD_LARGE_FORTIFICATION:
          return this.ExecuteBuildFortification(game, order.Location, true);
        case ActorTasks.REPORT_EVENTS:
          return this.ExecuteReport(game, percepts);
        case ActorTasks.SLEEP_NOW:
          return this.ExecuteSleepNow(game, percepts);
        case ActorTasks.FOLLOW_TOGGLE:
          return this.ExecuteToggleFollow(game);
        case ActorTasks.WHERE_ARE_YOU:
          return this.ExecuteReportPosition(game);
        default:
          throw new NotImplementedException("order task not handled");
      }
    }

    private ActorAction ExecuteBarricading(RogueGame game, Location location, bool toTheMax)
    {
      if (this.m_Actor.Location.Map != location.Map)
        return (ActorAction) null;
      DoorWindow door = location.Map.GetMapObjectAt(location.Position) as DoorWindow;
      if (door == null)
        return (ActorAction) null;
      if (!game.Rules.CanActorBarricadeDoor(this.m_Actor, door))
        return (ActorAction) null;
      if (game.Rules.IsAdjacent(this.m_Actor.Location.Position, location.Position))
      {
        ActorAction actorAction = (ActorAction) new ActionBarricadeDoor(this.m_Actor, game, door);
        if (!actorAction.IsLegal())
          return (ActorAction) null;
        if (!toTheMax)
          this.SetOrder((ActorOrder) null);
        return actorAction;
      }
      ActorAction actorAction1 = this.BehaviorIntelligentBumpToward(game, location.Position);
      if (actorAction1 == null)
        return (ActorAction) null;
      this.RunIfPossible(game.Rules);
      return actorAction1;
    }

    private ActorAction ExecuteBuildFortification(RogueGame game, Location location, bool isLarge)
    {
      if (this.m_Actor.Location.Map != location.Map)
        return (ActorAction) null;
      if (!game.Rules.CanActorBuildFortification(this.m_Actor, location.Position, isLarge))
        return (ActorAction) null;
      if (game.Rules.IsAdjacent(this.m_Actor.Location.Position, location.Position))
      {
        ActorAction actorAction = (ActorAction) new ActionBuildFortification(this.m_Actor, game, location.Position, isLarge);
        if (!actorAction.IsLegal())
          return (ActorAction) null;
        this.SetOrder((ActorOrder) null);
        return actorAction;
      }
      ActorAction actorAction1 = this.BehaviorIntelligentBumpToward(game, location.Position);
      if (actorAction1 == null)
        return (ActorAction) null;
      this.RunIfPossible(game.Rules);
      return actorAction1;
    }

    private ActorAction ExecuteGuard(RogueGame game, Location location, List<Percept> percepts)
    {
      List<Percept> percepts1 = this.FilterEnemies(game, percepts);
      if (percepts1 != null)
      {
        this.SetOrder((ActorOrder) null);
        Actor actor = this.FilterNearest(game, percepts1).Percepted as Actor;
        if (actor == null)
          throw new InvalidOperationException("null nearest enemy");
        return (ActorAction) new ActionShout(this.m_Actor, game, string.Format("{0} sighted!!", (object) actor.Name));
      }
      ActorAction actorAction1 = this.BehaviorEquipCellPhone(game);
      if (actorAction1 != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return actorAction1;
      }
      ActorAction actorAction2 = this.BehaviorUnequipCellPhoneIfLeaderHasNot(game);
      if (actorAction2 != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return actorAction2;
      }
      if (this.m_Actor.Location.Position != location.Position)
      {
        ActorAction actorAction3 = this.BehaviorIntelligentBumpToward(game, location.Position);
        if (actorAction3 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction3;
        }
      }
      if (game.Rules.IsActorHungry(this.m_Actor))
      {
        ActorAction actorAction3 = this.BehaviorEat(game);
        if (actorAction3 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction3;
        }
      }
      ActorAction actorAction4 = this.BehaviorUseMedecine(game, 2, 1, 2, 4, 2);
      if (actorAction4 != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return actorAction4;
      }
      this.m_Actor.Activity = Activity.IDLE;
      return (ActorAction) new ActionWait(this.m_Actor, game);
    }

    private ActorAction ExecutePatrol(RogueGame game, Location location, List<Percept> percepts)
    {
      List<Percept> percepts1 = this.FilterEnemies(game, percepts);
      if (percepts1 != null)
      {
        this.SetOrder((ActorOrder) null);
        Actor actor = this.FilterNearest(game, percepts1).Percepted as Actor;
        if (actor == null)
          throw new InvalidOperationException("null nearest enemy");
        return (ActorAction) new ActionShout(this.m_Actor, game, string.Format("{0} sighted!!", (object) actor.Name));
      }
      if (!this.m_ReachedPatrolPoint)
        this.m_ReachedPatrolPoint = this.m_Actor.Location.Position == location.Position;
      ActorAction actorAction1 = this.BehaviorEquipCellPhone(game);
      if (actorAction1 != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return actorAction1;
      }
      ActorAction actorAction2 = this.BehaviorUnequipCellPhoneIfLeaderHasNot(game);
      if (actorAction2 != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return actorAction2;
      }
      if (!this.m_ReachedPatrolPoint)
      {
        ActorAction actorAction3 = this.BehaviorIntelligentBumpToward(game, location.Position);
        if (actorAction3 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction3;
        }
      }
      if (game.Rules.IsActorHungry(this.m_Actor))
      {
        ActorAction actorAction3 = this.BehaviorEat(game);
        if (actorAction3 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction3;
        }
      }
      ActorAction actorAction4 = this.BehaviorUseMedecine(game, 2, 1, 2, 4, 2);
      if (actorAction4 != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return actorAction4;
      }
      List<Zone> patrolZones = location.Map.GetZonesAt(this.Order.Location.Position.X, this.Order.Location.Position.Y);
      return this.BehaviorWander(game, (Predicate<Location>) (loc =>
      {
        List<Zone> zonesAt = loc.Map.GetZonesAt(loc.Position.X, loc.Position.Y);
        if (zonesAt == null)
          return false;
        foreach (Zone zone1 in zonesAt)
        {
          foreach (Zone zone2 in patrolZones)
          {
            if (zone1 == zone2)
              return true;
          }
        }
        return false;
      }));
    }

    private ActorAction ExecuteDropAllItems(RogueGame game)
    {
      if (this.m_Actor.Inventory.IsEmpty)
        return (ActorAction) null;
      Item it = this.m_Actor.Inventory[0];
      if (it.IsEquipped)
        return (ActorAction) new ActionUnequipItem(this.m_Actor, game, it);
      return (ActorAction) new ActionDropItem(this.m_Actor, game, it);
    }

    private ActorAction ExecuteReport(RogueGame game, List<Percept> percepts)
    {
      List<Percept> percepts1 = this.FilterEnemies(game, percepts);
      if (percepts1 != null)
      {
        this.SetOrder((ActorOrder) null);
        Actor actor = this.FilterNearest(game, percepts1).Percepted as Actor;
        if (actor == null)
          throw new InvalidOperationException("null nearest enemy");
        return (ActorAction) new ActionShout(this.m_Actor, game, string.Format("{0} sighted!!", (object) actor.Name));
      }
      ActorAction actorAction = (ActorAction) null;
      bool flag = false;
      switch (this.m_ReportStage)
      {
        case 0:
          actorAction = this.m_LastRaidHeard == null ? (ActorAction) new ActionSay(this.m_Actor, game, this.m_Actor.Leader, "No raids heard.", RogueGame.Sayflags.NONE) : this.BehaviorTellFriendAboutPercept(game, this.m_LastRaidHeard);
          ++this.m_ReportStage;
          break;
        case 1:
          actorAction = this.m_LastEnemySaw == null ? (ActorAction) new ActionSay(this.m_Actor, game, this.m_Actor.Leader, "No enemies sighted.", RogueGame.Sayflags.NONE) : this.BehaviorTellFriendAboutPercept(game, this.m_LastEnemySaw);
          ++this.m_ReportStage;
          break;
        case 2:
          actorAction = this.m_LastItemsSaw == null ? (ActorAction) new ActionSay(this.m_Actor, game, this.m_Actor.Leader, "No items sighted.", RogueGame.Sayflags.NONE) : this.BehaviorTellFriendAboutPercept(game, this.m_LastItemsSaw);
          ++this.m_ReportStage;
          break;
        case 3:
          actorAction = this.m_LastSoldierSaw == null ? (ActorAction) new ActionSay(this.m_Actor, game, this.m_Actor.Leader, "No soldiers sighted.", RogueGame.Sayflags.NONE) : this.BehaviorTellFriendAboutPercept(game, this.m_LastSoldierSaw);
          ++this.m_ReportStage;
          break;
        case 4:
          flag = true;
          actorAction = (ActorAction) new ActionSay(this.m_Actor, game, this.m_Actor.Leader, "That's it.", RogueGame.Sayflags.NONE);
          break;
      }
      if (flag)
        this.SetOrder((ActorOrder) null);
      return actorAction ?? (ActorAction) new ActionSay(this.m_Actor, game, this.m_Actor.Leader, "Let me think...", RogueGame.Sayflags.NONE);
    }

    private ActorAction ExecuteSleepNow(RogueGame game, List<Percept> percepts)
    {
      List<Percept> percepts1 = this.FilterEnemies(game, percepts);
      if (percepts1 != null)
      {
        this.SetOrder((ActorOrder) null);
        Actor actor = this.FilterNearest(game, percepts1).Percepted as Actor;
        if (actor == null)
          throw new InvalidOperationException("null nearest enemy");
        return (ActorAction) new ActionShout(this.m_Actor, game, string.Format("{0} sighted!!", (object) actor.Name));
      }
      string reason;
      if (game.Rules.CanActorSleep(this.m_Actor, out reason))
      {
        if (this.m_Actor.Location.Map.LocalTime.TurnCounter % 2 == 0)
          return (ActorAction) new ActionSleep(this.m_Actor, game);
        return (ActorAction) new ActionWait(this.m_Actor, game);
      }
      this.SetOrder((ActorOrder) null);
      game.DoEmote(this.m_Actor, string.Format("I can't sleep now : {0}.", (object) reason));
      return (ActorAction) new ActionWait(this.m_Actor, game);
    }

    private ActorAction ExecuteToggleFollow(RogueGame game)
    {
      this.SetOrder((ActorOrder) null);
      this.DontFollowLeader = !this.DontFollowLeader;
      game.DoEmote(this.m_Actor, this.DontFollowLeader ? "OK I'll do my stuff, see you soon!" : "I'm ready!");
      return (ActorAction) new ActionWait(this.m_Actor, game);
    }

    private ActorAction ExecuteReportPosition(RogueGame game)
    {
      this.SetOrder((ActorOrder) null);
      string text = string.Format("I'm in {0} at {1},{2}.", (object) this.m_Actor.Location.Map.Name, (object) this.m_Actor.Location.Position.X, (object) this.m_Actor.Location.Position.Y);
      return (ActorAction) new ActionSay(this.m_Actor, game, this.m_Actor.Leader, text, RogueGame.Sayflags.NONE);
    }

    public void OnRaid(RaidType raid, Location location, int turn)
    {
      if (this.m_Actor.IsSleeping)
        return;
      string str;
      switch (raid)
      {
        case RaidType.BIKERS:
          str = "motorcycles coming";
          break;
        case RaidType.GANGSTA:
          str = "cars coming";
          break;
        case RaidType.BLACKOPS:
          str = "a chopper hovering";
          break;
        case RaidType.SURVIVORS:
          str = "honking coming";
          break;
        case RaidType.NATGUARD:
          str = "the army coming";
          break;
        case RaidType.ARMY_SUPLLIES:
          str = "a chopper hovering";
          break;
        default:
          throw new ArgumentOutOfRangeException(string.Format("unhandled raidtype {0}", (object) raid.ToString()));
      }
      this.m_LastRaidHeard = new Percept((object) str, turn, location);
    }
  }
}
