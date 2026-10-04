// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionTrade
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Text.Json;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Engine.Actions
{
  [Serializable]
  internal class ActionTrade : ActorAction, IJsonStaged<ActionTrade>
  {
    private readonly Actor m_Target;

    #region JSON save/load
    private ActionTrade(Actor actor, JsonElement src) : base(actor)
    {
      m_Target = src.Require("Target").RequireRef<Actor>();
    }

    static ActionTrade IJsonStaged<ActionTrade>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new ActionTrade(JsonActor(src), src));

    public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      toJsonActor(writer);
      writer.WriteRef("Target", m_Target);
    }
    #endregion

    public ActionTrade(Actor actor, Actor target) : base(actor)
    {
      m_Target = target
#if DEBUG
        ?? throw new ArgumentNullException(nameof(target))
#endif
      ;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanTradeWith(m_Target);
    }

    public override void Perform()
    {
      RogueGame.Game.DoTrade(m_Actor.Controller as Gameplay.AI.OrderableAI, m_Target.Controller as Gameplay.AI.OrderableAI);
    }
  }
}
