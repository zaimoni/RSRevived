﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemEntertainment
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using djack.RogueSurvivor.Data;
using Zaimoni.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemEntertainment : Item, UsableItem
    {
    List<Actor>? m_BoringFor = null; // alpha10 boring items moved out of Actor

    new public ItemEntertainmentModel Model { get {return (base.Model as ItemEntertainmentModel)!; } }

    public ItemEntertainment(ItemEntertainmentModel model) : base(model) {}

    public void AddBoringFor(Actor a)
    {
      if (null == m_BoringFor) m_BoringFor = new List<Actor>{ a };
      else if (!m_BoringFor.Contains(a)) m_BoringFor.Add(a);
    }

    public bool IsBoringFor(Actor a) { return m_BoringFor?.Contains(a) ?? false; }

#region UsableItem implementation
    public bool CouldUse() { return true; }
    public bool CouldUse(Actor a) { return a.Model.Abilities.IsIntelligent && !IsBoringFor(a); }
    public bool CanUse(Actor a) { return CouldUse(a); }
    public void Use(Actor actor, Inventory inv) {
#if DEBUG
      if (!inv.Contains(this)) throw new InvalidOperationException("inventory did not contain "+ToString());
#endif
      RogueForm.Game.DoUseEntertainmentItem(actor, this);   // forward to RogueGame -- CHAR manual is bloated
    }
#endregion

    [OnSerializing] private void OptimizeBeforeSaving(StreamingContext context)
    {
      // clean up dead actors refs
      // side effect: revived actors will forget about boring items
      if (null != m_BoringFor) {
        m_BoringFor.OnlyIfNot(Actor.IsDeceased);
        if (m_BoringFor.Count == 0) m_BoringFor = null;
      }
    }
  }
}
