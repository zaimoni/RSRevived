// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionTakeItem
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Drawing;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionTakeItem : ActorAction
  {
    private Point m_Position;
    private Item m_Item;

    public ActionTakeItem(Actor actor, RogueGame game, Point position, Item it)
      : base(actor, game)
    {
      if (it == null)
        throw new ArgumentNullException("item");
      this.m_Position = position;
      this.m_Item = it;
    }

    public override bool IsLegal()
    {
      return this.m_Game.Rules.CanActorGetItem(this.m_Actor, this.m_Item, out this.m_FailReason);
    }

    public override void Perform()
    {
      this.m_Game.DoTakeItem(this.m_Actor, this.m_Position, this.m_Item);
    }
  }
}
