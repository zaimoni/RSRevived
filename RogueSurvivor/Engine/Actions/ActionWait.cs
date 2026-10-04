// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionWait
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
  [Serializable]
  internal class ActionWait : ActorAction, IJsonStaged<ActionWait>
  {
    #region JSON save/load
    // the live constructor also resets the actor's activity: not for a load
    private ActionWait(Actor actor, JsonElement src) : base(actor) { }

    static ActionWait IJsonStaged<ActionWait>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new ActionWait(JsonActor(src), src));
    public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options) => toJsonActor(writer);
    #endregion

    public ActionWait(Actor actor)
      : base(actor)
    {
      actor.Activity = Activity.IDLE;   // normal state of a waiting actor
    }

    public override bool IsLegal() { return true; }
    public override void Perform() { RogueGame.Game.DoWait(m_Actor); }
  }
}
