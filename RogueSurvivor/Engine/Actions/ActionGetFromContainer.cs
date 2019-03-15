// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionGetFromContainer
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#if Z_VECTOR
using Point = Zaimoni.Data.Vector2D_int;
#else
using Point = System.Drawing.Point;
#endif

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionGetFromContainer : ActorAction   // XXX reskinned ActionTakeItem
  {
    private readonly Point m_Position;

    public Item Item {
      get {
        return m_Actor.Location.Map.GetItemsAt(m_Position).TopItem;
      }
    }

    public ActionGetFromContainer(Actor actor, Point position)
      : base(actor)
    {
      m_Position = position;
#if DEBUG
      Inventory itemsAt = actor.Location.Map.GetItemsAt(position);
      if (null == itemsAt) throw new InvalidOperationException("no items in container");
#endif
    }

    public override bool IsLegal()
    {
      return m_Actor.CanGetFromContainer(m_Position, out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoTakeFromContainer(m_Actor, m_Position);
    }
  }
}
