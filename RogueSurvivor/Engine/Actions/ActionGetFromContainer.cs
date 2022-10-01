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

    // error checks in ...::create
    private ActionGetFromContainer(PlayerController pc, MapObject obj) : base(pc.ControlledActor)
    {
      m_Container = obj;
    }

    public override bool IsLegal() => CanGetFrom(m_Container, out m_FailReason);

    public override bool IsPerformable()
    {
      if (1!=Rules.GridDistance(m_Actor.Location, m_Container.Location)) return false;
      return base.IsPerformable();
    }

    public override void Perform()
    {
      RogueGame.Game.HandlePlayerTakeItemFromContainer(m_Actor.Controller as PlayerController, m_Container);
    }

    static private string ReasonCantGetFrom(MapObject? obj)
    {
      if (null == obj || !obj.IsContainer) return "object is not a container";
      if (obj.Inventory.IsEmpty) return "nothing to take there";
      return "";
    }

    static private bool CanGetFrom(MapObject? obj, out string reason)
    {
	  reason = ReasonCantGetFrom(obj);
	  return string.IsNullOrEmpty(reason);
    }

    static public ActionGetFromContainer? create(PlayerController pc, Location loc) {
        if (!Map.Canonical(ref loc)) return null;
        var obj = loc.MapObject;
        if (null == obj || !obj.IsContainer || obj.Inventory.IsEmpty) return null;
        return new ActionGetFromContainer(pc, obj);
    }
  }
}
