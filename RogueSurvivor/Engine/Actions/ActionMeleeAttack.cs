// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionMeleeAttack
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

#define B_MOVIE_MARTIAL_ARTS

using djack.RogueSurvivor.Data;
using System;
using System.Text.Json;
using Zaimoni.JSON;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
  public interface CombatAction
  {
    Actor target { get; }  // of m_Actor
  }

   [Serializable]
  public sealed class ActionMeleeAttack : ActorAction, CombatAction, IJsonStaged<ActionMeleeAttack>
    {
    private readonly Actor m_Target;

    #region JSON save/load
    // the live constructor also resets the actor's activity: not for a load
    private ActionMeleeAttack(Actor actor, JsonElement src, JsonSerializerOptions options) : base(actor)
    {
      m_Target = src.Require("Target").RequireRef<Actor>();
    }

    static ActionMeleeAttack IJsonStaged<ActionMeleeAttack>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new ActionMeleeAttack(JsonActor(src), src, options));

    public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      toJsonActor(writer);
      writer.WriteRef("Target", m_Target);
    }
    #endregion

    public ActionMeleeAttack(Actor actor, Actor target) : base(actor)
    {
      m_Target = target;
      actor.Activity = Activity.IDLE;   // transition to fighting is in DoMeleeAttack
    }

    public Actor target { get { return m_Target; } }

    public override bool IsLegal()
    {
      m_FailReason = ReasonIllegal(target) ?? "";
      return string.IsNullOrEmpty(m_FailReason);
    }

    public override bool IsPerformable()
    {
      m_FailReason = ReasonIllegal(target) ?? ReasonCant(m_Actor, target) ?? "";
      return string.IsNullOrEmpty(m_FailReason);
    }

    public override void Perform()
    {
      RogueGame.Game.DoMeleeAttack(m_Actor, m_Target);
    }

    public static string? ReasonIllegal(Actor target)
    {
      if (target.IsDead) return "already dead!";
      return null;
    }

    public static string? ReasonCant(Actor aggressor, Actor target)
    {
#if B_MOVIE_MARTIAL_ARTS
      bool in_range = Rules.IsAdjacent(aggressor.Location, target.Location);
      // even martial arts 1 unlocks extended range.
      if (!in_range && 0< aggressor.UsingPolearmInBMovie && 2==Rules.GridDistance(aggressor.Location, target.Location)) in_range = true;
      if (!in_range) return "not adjacent";
#else
      if (!Rules.IsAdjacent(Location, target.Location)) return "not adjacent";
#endif
      if (aggressor.StaminaPoints < Actor.STAMINA_MIN_FOR_ACTIVITY) return "not enough stamina to attack";
      return null;
    }

    private static string ReasonCantMeleeAttack(Actor aggressor, Actor target)
    {
      return ReasonIllegal(target) ?? ReasonCant(aggressor, target) ?? "";
    }

    public static bool Can(Actor aggressor, Actor target, out string reason)
    {
      reason = ReasonCantMeleeAttack(aggressor, target);
      return string.IsNullOrEmpty(reason);
    }

    public static bool Can(Actor aggressor, Actor target)
    {
      return string.IsNullOrEmpty(ReasonCantMeleeAttack(aggressor, target));
    }
  }
}
