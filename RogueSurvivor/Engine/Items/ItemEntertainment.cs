// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemEntertainment
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Gameplay.AI;
using Zaimoni.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal sealed class ItemEntertainment : Item, UsableItem, Zaimoni.Serialization.ISerialize
    {
    List<ActorTag>? m_BoringFor = null; // alpha10 boring items moved out of Actor

    new public ItemEntertainmentModel Model { get {return (base.Model as ItemEntertainmentModel)!; } }

    public ItemEntertainment(ItemEntertainmentModel model) : base(model) {}

#region implement Zaimoni.Serialization.ISerialize
    protected ItemEntertainment(Zaimoni.Serialization.DecodeObjects decode) : base(decode) {
      Zaimoni.Serialization.ISave.LinearLoad(decode, x => m_BoringFor = new(x));
    }
    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode) {
      base.save(encode);
      Zaimoni.Serialization.ISave.LinearSave(encode, m_BoringFor);
    }
#endregion


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

#if PROTOTYPE
    [OnSerializing] private void OptimizeBeforeSaving(StreamingContext context)
    {
      // clean up dead actors refs
      // side effect: revived actors will forget about boring items
      if (null != m_BoringFor) {
        m_BoringFor.OnlyIfNot(Actor.IsDeceased);
        if (m_BoringFor.Count == 0) m_BoringFor = null;
      }
    }
#endif
  }
}
