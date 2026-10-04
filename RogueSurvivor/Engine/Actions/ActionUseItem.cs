// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionUseItem
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Text.Json;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Engine.Actions
{
  public interface Use<out T> where T:Item
  {
    public T Use { get; }
  }

  [Serializable]
  public sealed class ActionUseItem : ActorAction, Use<Item>, IJsonStaged<ActionUseItem>
    {
    private readonly Item m_Item;

    #region JSON save/load
    // the live constructor also resets the actor's activity: not for a load
    private ActionUseItem(Actor actor, JsonElement src) : base(actor)
    {
      m_Item = src.Require("Item").RequireRef<Item>();
    }

    static ActionUseItem IJsonStaged<ActionUseItem>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new ActionUseItem(JsonActor(src), src));

    public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      toJsonActor(writer);
      writer.WriteRef("Item", m_Item);
    }
    #endregion

    public ActionUseItem(Actor actor, Item it) : base(actor)
    {
      m_Item = it
#if DEBUG
        ?? throw new ArgumentNullException(nameof(it))
#endif
      ;
      actor.Activity = Activity.IDLE;
    }

    public Item Use { get { return m_Item; } }

    public override bool IsLegal()
    {
      return m_Actor.CanUse(m_Item, out m_FailReason);
    }

    public override void Perform()
    {
      RogueGame.Game.DoUseItem(m_Actor, m_Item);
    }
  } // ActionUseItem

  [Serializable]
  internal class ActionUse : ActorAction, Use<Item>, IJsonStaged<ActionUse>
    {
    private readonly Gameplay.Item_IDs m_ID;
    [NonSerialized] private Item? m_Item = null;

    #region JSON save/load
    private ActionUse(Actor actor, JsonElement src) : base(actor)
    {
      m_ID = src.RequireEnum<Gameplay.Item_IDs>("ID");
    }

    static ActionUse IJsonStaged<ActionUse>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new ActionUse(JsonActor(src), src));

    public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      toJsonActor(writer);
      writer.WriteString("ID", m_ID.ToString());
    }
    #endregion

    public ActionUse(Actor actor, Gameplay.Item_IDs it) : base(actor)
    {
#if DEBUG
      if (!(Gameplay.GameItems.From(it).create() is UsableItem)) throw new ArgumentNullException(nameof(it));
#endif
      m_ID = it;
    }

    public Gameplay.Item_IDs ID { get { return m_ID; } } // for completeness

    public Item? Use {
      get {
        return m_Item ??= m_Actor.Inventory.GetBestDestackable(Gameplay.GameItems.From(m_ID));
      }
    }

    public override bool IsLegal()
    {
      var it = Use;
      if (null == it) {
        m_FailReason = "not in inventory";
        return false;
      }
      return m_Actor.CanUse(it, out m_FailReason);
    }

    public override void Perform()
    {
      m_Actor.Activity = Activity.IDLE;
      RogueGame.Game.DoUseItem(m_Actor, Use);
    }
  } // ActionUse
}
