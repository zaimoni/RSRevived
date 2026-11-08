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
using System.Linq;
using System.Text.Json;
using Zaimoni.JSON;

using Point = Zaimoni.Data.Vector2D<short>;
using Percept = djack.RogueSurvivor.Engine.AI.Percept_<object>;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal sealed class FeralDogAI : ActorController, IJsonStaged<FeralDogAI>
  {
    // JSON: saves $type only so far (the AI reloads with amnesia)
    static FeralDogAI IJsonStaged<FeralDogAI>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new FeralDogAI(JsonOwner));

    private static readonly string[] FIGHT_EMOTES = new string[MAX_EMOTES]
    {
      "waf",
      "waf!?",
      "GRRRRR WAF WAF"
    };
    private const int RUN_TO_TARGET_DISTANCE = 3;  // dogs run to their target when close enough

    public const LOSSensor.SensingFilter VISION_SEES = LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.CORPSES;

    private LOSSensor m_LOSSensor;
    private SmellSensor m_LivingSmellSensor = new SmellSensor(Odor.LIVING);

    private new readonly LivingActor m_Actor;   // a dog has a one-slot inventory; this view is the same object as the base field

    public FeralDogAI(Actor src) : base(src)
    {
      m_Actor = (LivingActor)src;
      m_LOSSensor =  new LOSSensor(VISION_SEES, src);
    }

    #region JSON save/load
    public override void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      base.toJsonFields(writer, options);
      writer.WritePropertyName("LOS");
      JsonSerializer.Serialize(writer, m_LOSSensor, options);
      writer.WritePropertyName("Smell");
      JsonSerializer.Serialize(writer, m_LivingSmellSensor, options);
    }

    protected override void loadJsonFields(JsonElement src, JsonSerializerOptions options)
    {
      base.loadJsonFields(src, options);
      JsonElement x;
      m_LOSSensor = src.Require("LOS").Deserialize<LOSSensor>(options) ?? throw new JsonException("null LOS");
      m_LivingSmellSensor = src.Require("Smell").Deserialize<SmellSensor>(options) ?? throw new JsonException("null Smell");
    }
    #endregion

    public override HashSet<Point> FOV { get { return m_LOSSensor.FOV; } }
    public override Location[] FOVloc { get { return m_LOSSensor.FOVloc; } }
    public override Dictionary<Location, Actor>? friends_in_FOV { get { return m_LOSSensor.friends; } }
    public override Dictionary<Location, Actor>? enemies_in_FOV { get { return m_LOSSensor.enemies; } }

    public override List<Percept> UpdateSensors()
    {
      m_LivingSmellSensor.Sense(m_Actor);
      return m_LOSSensor.Sense();
    }

    protected override ActorAction? SelectAction()
    {
      var game = RogueGame.Game;

      ActorAction? tmpAction;

      // dogs target their leader's enemy before the usual check for enemies
      var leader = m_Actor.LiveLeader;
      if (leader is not null) {
        var targetActor = leader.TargetActor;
        if (targetActor is not null && targetActor.Location.Map == m_Actor.Location.Map) {
          m_Actor.Say(targetActor, "GRRRRRR WAF WAF", Sayflags.IS_FREE_ACTION | Sayflags.IS_DANGER);
          tmpAction = BehaviorStupidBumpToward(targetActor.Location, true, false);
          if (tmpAction is not null) {
            RunToIfCloseTo(targetActor.Location, RUN_TO_TARGET_DISTANCE);
            m_Actor.TargetedActivity(Activity.FIGHTING, targetActor);
            return tmpAction;
          }
        }
      }

      // dogs cannot order their followers to stay behind
      if ((_enemies = SortByGridDistance(FilterEnemies(_all = FilterSameMap(UpdateSensors())))) is not null) {
        tmpAction = BehaviorFightOrFlee(FIGHT_EMOTES, RouteFinder.SpecialActions.JUMP);
        if (tmpAction is not null) {
          // run to (or away if fleeing) if close.
          if (m_Actor.TargetActor is not null) RunToIfCloseTo(m_Actor.TargetActor.Location, RUN_TO_TARGET_DISTANCE);
          return tmpAction;
        }
      }
      if (m_Actor.IsAlmostHungry) {
        tmpAction = BehaviorGoEatFoodOnGround(_all?.FilterCast<Inventory>());
        if (tmpAction is not null) {
          m_Actor.Run();
          m_Actor.Activity = Activity.IDLE;
          return tmpAction;
        }
      }
      if (m_Actor.IsHungry) {
        tmpAction = BehaviorGoEatCorpse(_all);
        if (tmpAction is not null) {
          m_Actor.Run();
          return tmpAction;
        }
      }
      if (m_Actor.IsTired) return new ActionWait(m_Actor);
      if (m_Actor.IsSleepy) return new ActionSleep(m_Actor);
      if (leader is not null) {
        tmpAction = BehaviorFollowActor(leader, 1);
        if (tmpAction is not null) {
          m_Actor.Walk();
          m_Actor.TargetedActivity(Activity.FOLLOWING, m_Actor.Leader);
          return tmpAction;
        }
      }
      return BehaviorWander();
    }

    private void RunToIfCloseTo(Location loc, int closeDistance)
    {
      if (Rules.GridDistance(m_Actor.Location, in loc) <= closeDistance) {
        m_Actor.Run();
      } else {
        m_Actor.Walk();
      }
    }

    private ActorAction? BehaviorGoEatFoodOnGround(List<Percept_<Inventory>>? stacksPercepts)
    {
      if (stacksPercepts is null) return null;
      var firstByType = m_Actor.Location.Items?.GetFirst<Engine.Items.ItemFood>();
      if (firstByType is not null) return new ActionEatFoodOnGround(m_Actor, firstByType);
      var percept = FilterNearest(stacksPercepts);
      return BehaviorStupidBumpToward(percept.Location, false, false);
    }

    private ActorAction? BehaviorWalkAwayFrom(IEnumerable<Point> goals)
    {
      var leader = (m_Actor as LivingActor)?.LiveLeader;
      Dictionary<Direction, ActionBump> cache = new();
      var choiceEval = Choose(Direction.COMPASS, dir => {
        var bump = ActionBump.create(m_Actor, dir);
        if (bump is null) return float.NaN;
        if (!IsValidFleeingAction(bump)) return float.NaN;
        Location location = m_Actor.Location + dir;
        float num = SafetyFrom(location.Position, goals);
        if (leader is not null) {
          num -= (float)Rules.StdDistance(in location, leader.Location);
        }
        cache.Add(dir, bump);
        return num;
      }, (a, b) => a > b);
      return ((choiceEval is not null) ? cache[choiceEval.Choice] : null);
    }

    // Feral dogs use BehaviorFightOrFlee; simplified version of what OrderableAI uses
    private ActorAction? BehaviorFightOrFlee(string[] emotes, RouteFinder.SpecialActions allowedChargeActions)
    {
      const ActorCourage courage = ActorCourage.CAUTIOUS;
      var game = RogueGame.Game;
      var target = FilterNearest(_enemies);
      bool doRun = false;	// only matters when fleeing
      Actor enemy = target.Percepted;
      bool decideToFlee;
      if (enemy.HasEquipedRangedWeapon()) decideToFlee = false;
      else if (m_Actor.IsTired && Rules.IsAdjacent(m_Actor.Location, enemy.Location))
        decideToFlee = true;
      else {
        decideToFlee = WantToEvadeMelee(m_Actor, courage, enemy);
        doRun = !m_Actor.HasSpeedAdvantage(enemy);
      }
      if (!decideToFlee && m_Actor.WillTireAfterAttack()) {
        decideToFlee = true;    // but do not run as otherwise we won't build up stamina
      }

      ActorAction? tmpAction = null;

      if (decideToFlee) {
        if (m_Actor.Model.Abilities.CanTalk && Rules.Get.RollChance(EMOTE_FLEE_CHANCE))
          game.DoEmote(m_Actor, string.Format("{0} {1}!", emotes[0], enemy.Name));
        // using map objects goes here
        // barricading goes here
        if (m_Actor.Model.Abilities.AI_CanUseAIExits && (Lighting.DARKNESS== m_Actor.Location.Map.Lighting || Rules.Get.RollChance(FLEE_THROUGH_EXIT_CHANCE))) {
          tmpAction = BehaviorUseExit(UseExitFlags.NONE);
          if (tmpAction is not null) {
            bool flag3 = true;
            var leader = (m_Actor as LivingActor)?.LiveLeader;
            if (leader is not null) {
              var exitAt = m_Actor.Location.Exit;
              if (exitAt is not null) flag3 = leader.Location.Map == exitAt.ToMap;
            }
            if (flag3) {
              m_Actor.Activity = Activity.FLEEING;
              return tmpAction;
            }
          }
        }
        // XXX we should run for the exit here
        tmpAction = BehaviorWalkAwayFrom(_enemies.Select(p => p.Location.Position));
        if (tmpAction is not null) {
          if (doRun) m_Actor.Run();
          m_Actor.Activity = Activity.FLEEING;
          return tmpAction;
        }
        if (enemy.IsAdjacentToEnemy) {  // yes, any enemy...not just me
          if (m_Actor.Model.Abilities.CanTalk && Rules.Get.RollChance(EMOTE_FLEE_TRAPPED_CHANCE))
            game.DoEmote(m_Actor, emotes[1], true);
          return BehaviorMeleeAttack(target.Percepted as Actor);
        }
        return null;
      } // if (decldeToFlee)

      // redo the pause check
      if (m_Actor.Speed > enemy.Speed) {
        int dist = Rules.GridDistance(m_Actor.Location,target.Location);
        if (2==dist) {
          if (!m_Actor.WillActAgainBefore(enemy)) return new ActionWait(m_Actor);
          // cannot close at normal speed safely; run-hit ok but requires situational analysis
          return new ActionWait(m_Actor);
        }
      }

      // charge
      tmpAction = BehaviorChargeEnemy(target, true, true);
      if (tmpAction is not null) {
        if (m_Actor.Model.Abilities.CanTalk && Rules.Get.RollChance(EMOTE_CHARGE_CHANCE))
          game.DoEmote(m_Actor, string.Format("{0} {1}!", emotes[2], enemy.Name), true);
        return tmpAction;
      }
      return null;
    }
  }
}
