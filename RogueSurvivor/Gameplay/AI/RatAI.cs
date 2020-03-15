// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.RatAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using System;
using System.Collections.Generic;

using Point = Zaimoni.Data.Vector2D_short;
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

#nullable enable
    public override List<Percept> UpdateSensors()
    {
      m_LivingSmellSensor.Sense(m_Actor);
      return m_LOSSensor.Sense();
    }

    public override HashSet<Point> FOV { get { return m_LOSSensor.FOV; } }
    public override Dictionary<Location, Actor>? friends_in_FOV { get { return m_LOSSensor.friends; } }
    public override Dictionary<Location, Actor>? enemies_in_FOV { get { return m_LOSSensor.enemies; } }
#nullable restore
    protected override void SensorsOwnedBy(Actor actor) { m_LOSSensor.OwnedBy(actor); }

    protected override ActorAction SelectAction()
    {
      ActorAction? tmpAction;
      if (null != (_enemies = SortByGridDistance(FilterEnemies(_all = FilterSameMap(UpdateSensors()))))) {
        tmpAction = TargetGridMelee(_enemies);
        if (null != tmpAction) return tmpAction;
      }
      tmpAction = BehaviorGoEatCorpse(_all);
      if (null != tmpAction) return tmpAction;

      tmpAction = BehaviorTrackScent(m_LivingSmellSensor.Scents);
      if (null != tmpAction) {
        m_Actor.Activity = Activity.TRACKING;
        return tmpAction;
      }
      return BehaviorWander();
    }
  }
}
