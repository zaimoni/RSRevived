// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionEatFoodOnGround
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Items;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionEatFoodOnGround : ActorAction
  {
    private ItemFood m_Item;

    public ActionEatFoodOnGround(Actor actor, RogueGame game, ItemFood it)
      : base(actor, game)
    {
      if (it == null) throw new ArgumentNullException("item");
      m_Item = it;
    }

    public override bool IsLegal()
    {
      return m_Game.Rules.CanActorEatFoodOnGround(m_Actor, m_Item, out m_FailReason);
    }

    public override void Perform()
    {
      m_Game.DoEatFoodFromGround(m_Actor, m_Item);
    }
  }
}
