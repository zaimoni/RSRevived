// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionGetFromContainer
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

using Point = Zaimoni.Data.Vector2D_short;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionGetFromContainer : ActorAction   // XXX reskinned ActionTakeItem
  {
    private readonly Location m_Location;

#if DEAD_FUNC
    public Item Item { get { return m_Location.Items.TopItem; } }
#endif

    public ActionGetFromContainer(PlayerController pc, Location loc)
      : base(pc.ControlledActor)
    {
      if (!Map.Canonical(ref loc)) throw new ArgumentOutOfRangeException(nameof(loc),loc,"not canonical");
      m_Location = loc;
#if DEBUG
      if (null == loc.Items) throw new ArgumentNullException(nameof(loc)+".Items");
#endif
    }

    public override bool IsLegal()
    {
      return (m_Actor.Controller as PlayerController).CanGetFromContainer(m_Location, out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoTakeFromContainer(m_Actor.Controller as PlayerController, in m_Location);
    }
  }
}
