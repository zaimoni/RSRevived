// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionMeleeAttack
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

using djack.RogueSurvivor.Data;
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
      return m_Actor.CanMeleeAttack(m_Target);
    }

    public override void Perform()
    {
      RogueGame.Game.DoMeleeAttack(m_Actor, m_Target);
    }
  }
}
