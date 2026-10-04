// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionSleep
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
  internal class ActionSleep : ActorAction, IJsonStaged<ActionSleep>
  {
    public ActionSleep(Actor actor) : base(actor) {}

    // JSON: no state
    static ActionSleep IJsonStaged<ActionSleep>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new ActionSleep(JsonActor(src)));
    public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options) => toJsonActor(writer);
    public override bool IsLegal() => m_Actor.CanSleep(out m_FailReason);
    public override void Perform() => m_Actor.StartSleeping();
  }
}
