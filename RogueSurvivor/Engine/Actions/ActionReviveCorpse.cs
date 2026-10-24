// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionReviveCorpse
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Items;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionReviveCorpse : ActorAction,TargetCorpse,Use<ItemMedicine>
  {
    private readonly Corpse m_Target;

    private new readonly LivingActor m_Actor;   // the same object as the base field

    public ActionReviveCorpse(LivingActor actor, Corpse target) : base(actor)
    {
      m_Actor = actor;
      m_Target = target
#if DEBUG
        ?? throw new ArgumentNullException(nameof(target))
#endif
      ;
    }

    public Corpse What { get { return m_Target; } }
    public ItemMedicine Use { get { return m_Actor.Inventory.GetFirst(Gameplay.Item_IDs.MEDICINE_MEDIKIT) as ItemMedicine; } } // assume legal

    public override bool IsLegal()
    {
      m_FailReason = ReasonIllegal(m_Actor) ?? ReasonIllegal(m_Target);
      return string.IsNullOrEmpty(m_FailReason);
    }

    public override bool IsPerformable()
    {
      if (!IsLegal()) return false;
      m_FailReason = ReasonCant(m_Actor, m_Target);
      return string.IsNullOrEmpty(m_FailReason);
    }

    public override void Perform()
    {
      RogueGame.Game.DoReviveCorpse(m_Actor, m_Target);
    }

    public static string? ReasonIllegal(LivingActor actor) {
      if (0 == actor.MySkills.GetSkillLevel(Gameplay.Skills.IDs.MEDIC)) return "lack medic skill";
      if (!actor.Inventory.Has(Gameplay.Item_IDs.MEDICINE_MEDIKIT)) return "no medikit";
      return null;
    }

    public static string? ReasonIllegal(Corpse corpse) {
      if (0 < corpse.RotLevel) return "corpse not fresh";
      return null;
    }

    public static string? ReasonCant(LivingActor actor, Corpse corpse)
    {
      if (corpse.Location != actor.Location) return "not there";
      return null;
    }

    private static string ReasonCannot(LivingActor actor, Corpse corpse)
    {
      return ReasonIllegal(actor) ?? ReasonIllegal(corpse) ?? ReasonCant(actor, corpse) ?? "";
    }

    public static bool Can(LivingActor actor, Corpse corpse)
    {
      return string.IsNullOrEmpty(ReasonCannot(actor, corpse));
    }


  }
}
