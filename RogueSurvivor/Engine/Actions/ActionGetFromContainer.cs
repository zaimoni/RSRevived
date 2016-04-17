// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionGetFromContainer
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System.Drawing;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionGetFromContainer : ActorAction
  {
    private Point m_Position;

    public Item Item
    {
      get
      {
        return this.m_Actor.Location.Map.GetItemsAt(this.m_Position).TopItem;
      }
    }

    public ActionGetFromContainer(Actor actor, RogueGame game, Point position)
      : base(actor, game)
    {
      this.m_Position = position;
    }

    public override bool IsLegal()
    {
      return this.m_Game.Rules.CanActorGetItemFromContainer(this.m_Actor, this.m_Position, out this.m_FailReason);
    }

    public override void Perform()
    {
      this.m_Game.DoTakeFromContainer(this.m_Actor, this.m_Position);
    }
  }
}
