// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionBarricadeDoor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Items;
using DoorWindow = djack.RogueSurvivor.Engine.MapObjects.DoorWindow;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionBarricadeDoor : ActorAction,Target<DoorWindow>,Use<ItemBarricadeMaterial>
  {
    private readonly DoorWindow m_Door;

    public ActionBarricadeDoor(Actor actor, DoorWindow door) : base(actor)
    {
      m_Door = door;
    }

    public DoorWindow What { get { return m_Door; } }
    public ItemBarricadeMaterial Use { get { return m_Actor.Inventory?.GetSmallestStackOf<ItemBarricadeMaterial>()!; } } // assume legal/performable

    public override bool IsLegal()
    {
      return m_Actor.CanBarricade(m_Door, out m_FailReason);
    }

    public override void Perform()
    {
      RogueForm.Game.DoBarricadeDoor(m_Actor, m_Door);
    }
  }
}
