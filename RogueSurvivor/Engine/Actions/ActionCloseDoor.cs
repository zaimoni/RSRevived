// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionCloseDoor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;
using System.Text.Json;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Engine.Actions
{
  [Serializable]
  internal class ActionCloseDoor : ActorAction, IJsonStaged<ActionCloseDoor>
  {
    private readonly DoorWindow m_Door;
    private readonly bool m_IsFreeAction;

    #region JSON save/load
    private ActionCloseDoor(Actor actor, JsonElement src, JsonSerializerOptions options) : base(actor)
    {
      m_Door = src.Require("Door").RequireRef<DoorWindow>();
      if (src.TryGetProperty("Free", out var x)) m_IsFreeAction = x.GetBoolean();
    }

    static ActionCloseDoor IJsonStaged<ActionCloseDoor>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new ActionCloseDoor(JsonActor(src), src, options));

    public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      toJsonActor(writer);
      writer.WriteRef("Door", m_Door);
      if (m_IsFreeAction) writer.WriteBoolean("Free", true);
    }
    #endregion

    public ActionCloseDoor(Actor actor, DoorWindow door, bool free = false) : base(actor)
    {
      m_Door = door;
      m_IsFreeAction = free;
    }

    public DoorWindow Door { get { return m_Door; } }
    public bool IsFreeAction { get { return m_IsFreeAction; } }

    public override bool IsLegal()
    {
      return m_Actor.CanClose(m_Door, out m_FailReason);
    }

    public override bool IsPerformable()
    {
      if (!base.IsPerformable()) return false;
      return Rules.IsAdjacent(m_Actor.Location,m_Door.Location);
    }

    public override void Perform()
    {
      m_Door.SetState(DoorWindow.STATE_CLOSED);
      if (!m_IsFreeAction) m_Actor.SpendActionPoints();
      RogueGame.Game.UI_CloseDoor(m_Actor, m_Door);
    }

    public override string? ToString() => "close "+m_Door.TheName;
  }
}
