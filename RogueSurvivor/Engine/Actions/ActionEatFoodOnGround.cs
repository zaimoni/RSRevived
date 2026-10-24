// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionEatFoodOnGround
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Items;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionEatFoodOnGround : ActorAction
  {
    private readonly ItemFood m_Item;
    private new readonly LivingActor m_Actor;   // the same object as the base field

    public ActionEatFoodOnGround(LivingActor actor, ItemFood it) : base(actor)
    {
      m_Actor = actor;
      m_Item = it;
    }

    public override bool IsLegal()
    {
      return Rules.CanActorEatFoodOnGround(m_Actor, m_Item, out m_FailReason);
    }

    public override void Perform()
    {
      m_Item.Use(m_Actor, m_Actor.Location.Items!);
    }
  }
}
