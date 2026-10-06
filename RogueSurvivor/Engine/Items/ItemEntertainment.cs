// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemEntertainment
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Gameplay.AI;
using System.Text.Json;
using Zaimoni.JSON;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  public sealed class ItemEntertainment : Item, UsableItem, IJsonStaged<ItemEntertainment>
    {
    List<ActorTag>? m_BoringFor = null; // alpha10 boring items moved out of Actor

    new public ItemEntertainmentModel Model { get {return (base.Model as ItemEntertainmentModel)!; } }

    public ItemEntertainment(ItemEntertainmentModel model) : base(model) {}
    public ItemEntertainment(ItemEntertainmentModel model, int qty) : base(model, qty) {}

    private ItemEntertainment(JsonElement src, JsonSerializerOptions options) : base(src, options) {
        RequireModel<ItemEntertainmentModel>();
        if (src.TryGetProperty("BoringFor", out var relay)) {
            m_BoringFor = new();
            foreach (var x in relay.EnumerateArray()) m_BoringFor.Add(x.Deserialize<ActorTag>(options));
        }
    }

    static ItemEntertainment IJsonStaged<ItemEntertainment>.fromJson(JsonElement src, JsonSerializerOptions options) => new(src, options);

    public override void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options) {
        base.toJsonFields(writer, options);
        if (null != m_BoringFor) writer.WriteArray("BoringFor", m_BoringFor, options);
    }


    public void AddBoringFor(in ActorTag a)
    {
      if (null == m_BoringFor) m_BoringFor = new List<ActorTag> { a };
      else if (!m_BoringFor.Contains(a)) m_BoringFor.Add(a);
    }

    public void AddBoringFor(Actor a) => AddBoringFor(new ActorTag(a));

    public bool IsBoringFor(ActorTag a) { return m_BoringFor?.Contains(a) ?? false; }
    public bool IsBoringFor(Actor a) => IsBoringFor(new ActorTag(a));

#region UsableItem implementation
    public bool CouldUse() { return true; }
    public bool CouldUse(Actor a) { return a.Model.Abilities.IsIntelligent && !IsBoringFor(a); }
    public bool CanUse(Actor a) { return CouldUse(a); }
    public void Use(Actor actor, Inventory inv) {
#if DEBUG
      if (!inv.Contains(this)) throw new InvalidOperationException("inventory did not contain "+ToString());
#endif
      RogueGame.Game.DoUseEntertainmentItem(actor, this);   // forward to RogueGame -- CHAR manual is bloated
    }
    public string ReasonCantUse(Actor a) {
      if (!a.Model.Abilities.IsIntelligent) return "not intelligent";
      if (IsBoringFor(a)) return "bored by this";
      return "";
    }
    public bool UseBeforeDrop(Actor a) {
      if (a.Controller is ObjectiveAI oai) return 2<=oai.WantRestoreSAN;
      return false;
    }
    public bool FreeSlotByUse(Actor a) { return false; } // not quite correct
#endregion
  }
}
