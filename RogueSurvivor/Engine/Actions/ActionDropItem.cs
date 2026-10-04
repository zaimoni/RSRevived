// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionDropItem
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using djack.RogueSurvivor.Data;
using System.Text.Json;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Engine.Actions
{
  [Serializable]
  internal class ActionDropItem : ActorAction,ActorGive, IJsonStaged<ActionDropItem>
  {
    private readonly Item m_Item;

    public Item Give { get { return m_Item; } }

    #region JSON save/load
    // the live constructor also resets the actor's activity: not for a load
    private ActionDropItem(Actor actor, JsonElement src, JsonSerializerOptions options) : base(actor)
    {
      m_Item = src.Require("Item").RequireRef<Item>();
    }

    static ActionDropItem IJsonStaged<ActionDropItem>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new ActionDropItem(JsonActor(src), src, options));

    public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      toJsonActor(writer);
      writer.WriteRef("Item", m_Item);
    }
    #endregion

    public ActionDropItem(Actor actor, Item it) : base(actor)
    {
      m_Item = it
#if DEBUG
        ?? throw new ArgumentNullException(nameof(it))
#endif
      ;
      actor.Activity = Activity.IDLE;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanDrop(m_Item, out m_FailReason);
    }

    public override void Perform()
    {
      RogueGame.Game.DoDropItem(m_Actor, m_Item);
    }
  }
}
