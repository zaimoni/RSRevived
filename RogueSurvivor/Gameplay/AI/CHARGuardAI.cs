// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.CHARGuardAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class CHARGuardAI : OrderableAI
  {
    private static string[] FIGHT_EMOTES = new string[3]
    {
      "Go away",
      "Damn it I'm trapped!",
      "Hey"
    };
    private const int LOS_MEMORY = 10;
    private LOSSensor m_LOSSensor;
    private MemorizedSensor m_MemorizedSensor;

    protected override void CreateSensors()
    {
      this.m_LOSSensor = new LOSSensor(LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.ITEMS);
      this.m_MemorizedSensor = new MemorizedSensor((Sensor) this.m_LOSSensor, 10);
    }

    public override void TakeControl(Actor actor)
    {
      base.TakeControl(actor);
    }

    protected override List<Percept> UpdateSensors(RogueGame game)
    {
      return this.m_MemorizedSensor.Sense(game, this.m_Actor);
    }

    protected override ActorAction SelectAction(RogueGame game, List<Percept> percepts)
    {
      List<Percept> percepts1 = this.FilterSameMap(game, percepts);
      if (this.Order != null)
      {
        ActorAction actorAction = this.ExecuteOrder(game, this.Order, percepts1);
        if (actorAction == null)
        {
          this.SetOrder((ActorOrder) null);
        }
        else
        {
          this.m_Actor.Activity = Activity.FOLLOWING_ORDER;
          return actorAction;
        }
      }
      this.m_Actor.IsRunning = false;
      List<Percept> percepts2 = this.FilterEnemies(game, percepts1);
      List<Percept> perceptList1 = this.FilterCurrent(game, percepts2);
      bool flag1 = this.m_Actor.HasLeader && !this.DontFollowLeader;
      bool flag2 = percepts2 != null;
      ActorAction actorAction1 = this.BehaviorEquipWeapon(game);
      if (actorAction1 != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return actorAction1;
      }
      ActorAction actorAction2 = this.BehaviorEquipBodyArmor(game);
      if (actorAction2 != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return actorAction2;
      }
      if (perceptList1 != null)
      {
        List<Percept> percepts3 = this.FilterFireTargets(game, perceptList1);
        if (percepts3 != null)
        {
          Percept target = this.FilterNearest(game, percepts3);
          Actor actor = target.Percepted as Actor;
          ActorAction actorAction3 = this.BehaviorRangedAttack(game, target);
          if (actorAction3 != null)
          {
            this.m_Actor.Activity = Activity.FIGHTING;
            this.m_Actor.TargetActor = actor;
            return actorAction3;
          }
        }
      }
      if (perceptList1 != null)
      {
        object percepted = this.FilterNearest(game, perceptList1).Percepted;
        ActorAction actorAction3 = this.BehaviorFightOrFlee(game, perceptList1, true, true, ActorCourage.COURAGEOUS, CHARGuardAI.FIGHT_EMOTES);
        if (actorAction3 != null)
          return actorAction3;
      }
      List<Percept> perceptList2 = this.FilterNonEnemies(game, percepts1);
      if (perceptList2 != null)
      {
        List<Percept> percepts3 = this.Filter(game, perceptList2, (Predicate<Percept>) (p =>
        {
          Actor actor = p.Percepted as Actor;
          if (actor.Faction == game.GameFactions.TheCHARCorporation)
            return false;
          return game.IsInCHARProperty(actor.Location);
        }));
        if (percepts3 != null)
        {
          Actor target = this.FilterNearest(game, percepts3).Percepted as Actor;
          game.DoMakeAggression(this.m_Actor, target);
          this.m_Actor.Activity = Activity.FIGHTING;
          this.m_Actor.TargetActor = target;
          return (ActorAction) new ActionSay(this.m_Actor, game, target, "Hey YOU!", RogueGame.Sayflags.IS_IMPORTANT);
        }
      }
      if (flag2 && perceptList2 != null)
      {
        ActorAction actorAction3 = this.BehaviorWarnFriends(game, perceptList2, this.FilterNearest(game, percepts2).Percepted as Actor);
        if (actorAction3 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction3;
        }
      }
      if (this.BehaviorRestIfTired(game) != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return (ActorAction) new ActionWait(this.m_Actor, game);
      }
      if (percepts2 != null)
      {
        Percept target = this.FilterNearest(game, percepts2);
        if (this.m_Actor.Location == target.Location)
        {
          Actor actor = target.Percepted as Actor;
          target = new Percept((object) actor, this.m_Actor.Location.Map.LocalTime.TurnCounter, actor.Location);
        }
        ActorAction actorAction3 = this.BehaviorChargeEnemy(game, target);
        if (actorAction3 != null)
        {
          this.m_Actor.Activity = Activity.FIGHTING;
          this.m_Actor.TargetActor = target.Percepted as Actor;
          return actorAction3;
        }
      }
      if (game.Rules.IsActorSleepy(this.m_Actor) && !flag2)
      {
        ActorAction actorAction3 = this.BehaviorSleep(game, this.m_LOSSensor.FOV);
        if (actorAction3 != null)
        {
          if (actorAction3 is ActionSleep)
            this.m_Actor.Activity = Activity.SLEEPING;
          return actorAction3;
        }
      }
      if (flag1)
      {
        Point position = this.m_Actor.Leader.Location.Position;
        bool isVisible = this.m_LOSSensor.FOV.Contains(this.m_Actor.Leader.Location.Position);
        ActorAction actorAction3 = this.BehaviorFollowActor(game, this.m_Actor.Leader, position, isVisible, 1);
        if (actorAction3 != null)
        {
          this.m_Actor.Activity = Activity.FOLLOWING;
          this.m_Actor.TargetActor = this.m_Actor.Leader;
          return actorAction3;
        }
      }
      ActorAction actorAction4 = this.BehaviorWander(game, (Predicate<Location>) (loc => RogueGame.IsInCHAROffice(loc)));
      if (actorAction4 != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return actorAction4;
      }
      this.m_Actor.Activity = Activity.IDLE;
      return this.BehaviorWander(game);
    }
  }
}
