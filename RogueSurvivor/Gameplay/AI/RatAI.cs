// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.RatAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
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
  internal class RatAI : BaseAI
  {
    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.CORPSES;

    private readonly LOSSensor m_LOSSensor = new LOSSensor(VISION_SEES);
    private readonly SmellSensor m_LivingSmellSensor = new SmellSensor(Odor.LIVING);

    public RatAI()
    {
    }

    public override List<Percept> UpdateSensors()
    {
      List<Percept> perceptList = m_LOSSensor.Sense(m_Actor);
      perceptList.AddRange(m_LivingSmellSensor.Sense(m_Actor));
      return perceptList;
    }

    public override HashSet<Point> FOV { get { return m_LOSSensor.FOV; } }
    protected override void SensorsOwnedBy(Actor actor) { m_LOSSensor.OwnedBy(actor); }

    protected override ActorAction SelectAction(RogueGame game)
    {
      List<Percept> percepts1 = FilterSameMap(UpdateSensors());
      List<Percept> enemies = SortByGridDistance(FilterEnemies(percepts1));
      ActorAction tmpAction;
      if (enemies != null) {
        tmpAction = TargetGridMelee(FilterCurrent(enemies));
        if (null != tmpAction) return tmpAction;
        tmpAction = TargetGridMelee(FilterOld(enemies));
        if (null != tmpAction) return tmpAction;
      }
      tmpAction = BehaviorGoEatCorpse(percepts1);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      tmpAction = BehaviorTrackScent(m_LivingSmellSensor.Scents);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.TRACKING;
        return tmpAction;
      }
      m_Actor.Activity = Activity.IDLE;
      return BehaviorWander();
    }
  }
}
