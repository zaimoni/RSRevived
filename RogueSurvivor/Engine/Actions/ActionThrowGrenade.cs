// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionThrowGrenade
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Items;

using Point = Zaimoni.Data.Vector2D_short;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionThrowGrenade : ActorAction
  {
    private readonly Point m_ThrowPos;

    public ActionThrowGrenade(Actor actor, Point throwPos)
      : base(actor)
    {
      m_ThrowPos = throwPos;
    }

    public override bool IsLegal()
    {
      return m_Actor.CanThrowTo(m_ThrowPos, out m_FailReason);
    }

    public override void Perform()
    {
      if (m_Actor.GetEquippedWeapon() is ItemPrimedExplosive)
        RogueForm.Game.DoThrowGrenadePrimed(m_Actor, m_ThrowPos);
      else
        RogueForm.Game.DoThrowGrenadeUnprimed(m_Actor, m_ThrowPos);
    }
  }
}
