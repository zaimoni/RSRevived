// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.FeralDogAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using djack.RogueSurvivor.Gameplay.AI.Tools;
using System;
using System.Collections.Generic;

using Point = Zaimoni.Data.Vector2D_short;
using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class FeralDogAI : BaseAI
  {
    private static readonly string[] FIGHT_EMOTES = new string[MAX_EMOTES]
    {
      "waf",
      "waf!?",
      "GRRRRR WAF WAF"
    };
    private const int RUN_TO_TARGET_DISTANCE = 3;  // dogs run to their target when close enough

    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.CORPSES;

    private readonly LOSSensor m_LOSSensor = new LOSSensor(VISION_SEES);
    private readonly SmellSensor m_LivingSmellSensor = new SmellSensor(Odor.LIVING);

    public FeralDogAI()
    {
    }

    public override HashSet<Point> FOV { get { return m_LOSSensor.FOV; } }
#nullable enable
    public override Dictionary<Location, Actor>? friends_in_FOV { get { return m_LOSSensor.friends; } }
    public override Dictionary<Location, Actor>? enemies_in_FOV { get { return m_LOSSensor.enemies; } }
#nullable restore
    protected override void SensorsOwnedBy(Actor actor) { m_LOSSensor.OwnedBy(actor); }

#nullable enable
    public override List<Percept> UpdateSensors()
    {
      m_LivingSmellSensor.Sense(m_Actor);
      return m_LOSSensor.Sense(m_Actor);
    }
#nullable restore

    protected override ActorAction SelectAction(RogueGame game)
    {
      ActorAction tmpAction;

      // dogs target their leader's enemy before the usual check for enemies
      if (m_Actor.HasLeader) {
        Actor targetActor = m_Actor.Leader.TargetActor;
        if (targetActor != null && targetActor.Location.Map == m_Actor.Location.Map) {
          RogueGame.DoSay(m_Actor, targetActor, "GRRRRRR WAF WAF", RogueGame.Sayflags.IS_FREE_ACTION | RogueGame.Sayflags.IS_DANGER);
          tmpAction = BehaviorStupidBumpToward(targetActor.Location, true, false);
          if (null != tmpAction) {
            RunToIfCloseTo(targetActor.Location, RUN_TO_TARGET_DISTANCE);
            m_Actor.Activity = Activity.FIGHTING;
            m_Actor.TargetActor = targetActor;
            return tmpAction;
          }
        }
      }

      // dogs cannot order their followers to stay behind
      if (null != (_enemies = SortByGridDistance(FilterEnemies(_all = FilterSameMap(UpdateSensors()))))) {
        tmpAction = BehaviorFightOrFlee(game, FIGHT_EMOTES, RouteFinder.SpecialActions.JUMP);
        if (null != tmpAction) {
          // run to (or away if fleeing) if close.
          if (m_Actor.TargetActor != null) RunToIfCloseTo(m_Actor.TargetActor.Location, RUN_TO_TARGET_DISTANCE);
          return tmpAction;
        }
      }
      if (m_Actor.IsAlmostHungry) {
        tmpAction = BehaviorGoEatFoodOnGround(_all?.FilterT<Inventory>());
        if (null != tmpAction) {
          m_Actor.Run();
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      if (m_Actor.IsHungry) {
        tmpAction = BehaviorGoEatCorpse(_all);
        if (null != tmpAction) {
          m_Actor.Run();
          return tmpAction;
        }
      }
      if (m_Actor.IsTired) return new ActionWait(m_Actor);
      if (m_Actor.IsSleepy) return new ActionSleep(m_Actor);
      if (m_Actor.HasLeader) {
        tmpAction = BehaviorFollowActor(m_Actor.Leader, 1);
        if (null != tmpAction) {
          m_Actor.Walk();
          m_Actor.Activity = Activity.FOLLOWING;
          m_Actor.TargetActor = m_Actor.Leader;
          return tmpAction;
        }
      }
      return BehaviorWander();
    }

    protected void RunToIfCloseTo(Location loc, int closeDistance)
    {
      if (Rules.GridDistance(m_Actor.Location, in loc) <= closeDistance) {
        m_Actor.Run();
      } else {
        m_Actor.Walk();
      }
    }
  }
}
