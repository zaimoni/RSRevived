// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.SkeletonAI
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
using System.Diagnostics.Contracts;

using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class SkeletonAI : BaseAI
  {
    private const int IDLE_CHANCE = 80;

    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS;

    private LOSSensor m_LOSSensor;

    public SkeletonAI()
    {
      m_LOSSensor = new LOSSensor(VISION_SEES);
    }

    public override List<Percept> UpdateSensors()
    {
      return m_LOSSensor.Sense(m_Actor);
    }

    public override HashSet<Point> FOV { get { return m_LOSSensor.FOV; } }

    protected override ActorAction SelectAction(RogueGame game, List<Percept> percepts)
    {
      Contract.Ensures(null == Contract.Result<ActorAction>() || Contract.Result<ActorAction>().IsLegal());
      List<Percept> percepts1 = FilterSameMap(percepts);
      Percept percept = FilterNearest(FilterEnemies(percepts1));
      if (percept != null) {
        ActorAction actorAction = BehaviorStupidBumpToward(percept.Location.Position);
        if (actorAction != null) {
          m_Actor.Activity = Activity.CHASING;
          m_Actor.TargetActor = percept.Percepted as Actor;
          return actorAction;
        }
      }
      if (game.Rules.RollChance(IDLE_CHANCE)) return new ActionWait(m_Actor);
      m_Actor.Activity = Activity.IDLE;
      return BehaviorWander(game);
    }
  }
}
