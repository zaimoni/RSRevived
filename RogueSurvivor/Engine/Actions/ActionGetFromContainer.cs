// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionGetFromContainer
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionGetFromContainer : ActorAction   // XXX reskinned ActionTakeItem
  {
    private readonly MapObject m_Container;

    public ActionGetFromContainer(PlayerController pc, Location loc) : base(pc.ControlledActor)
    {
      if (!Map.Canonical(ref loc)) throw new ArgumentOutOfRangeException(nameof(loc),loc,"not canonical");
      var obj = loc.MapObject;
      if (null == obj || !obj.IsContainer) throw new ArgumentNullException(nameof(obj));
      m_Container = obj;
    }

    public override bool IsLegal()
    {
      return (m_Actor.Controller as PlayerController).CanGetFromContainer(m_Container.Location, out m_FailReason);
    }

    public override bool IsPerformable()
    {
      if (1!=Rules.GridDistance(m_Actor.Location, m_Container.Location)) return false;
      return base.IsPerformable();
    }

    public override void Perform()
    {
      RogueGame.Game.HandlePlayerTakeItemFromContainer(m_Actor.Controller as PlayerController, m_Container);
    }
  }
}
