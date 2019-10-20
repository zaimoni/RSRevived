// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorSheet
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

#nullable enable

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class ActorSheet
  {
    public readonly SkillTable SkillTable = new SkillTable();   // \todo? if this is split out, can convert to readonly struct and use model's copy only
    public readonly int BaseHitPoints;
    public readonly int BaseStaminaPoints;
    public readonly int BaseFoodPoints;
    public readonly int BaseSleepPoints;
    public readonly int BaseSanity;
//  public readonly Attack UnarmedAttack;   // Oct 19 2019: automatic serialization code makes structs flagged readonly (whether normal or readonly)
//  public readonly Defence BaseDefence;    // save fine, but hard-crash on loading before an ISerializable constructor can be called.
    public Attack UnarmedAttack { get; private set; }
    public Defence BaseDefence { get; private set; }
    public readonly int BaseViewRange;
    public readonly int BaseAudioRange;
    public readonly float BaseSmellRating;
    public readonly int BaseInventoryCapacity;

    public ActorSheet(Gameplay.GameActors.ActorData src, int baseFoodPoints, int baseSleepPoints, int baseSanity, Verb unarmedAttack, int inventoryCapacity)
    {
      BaseHitPoints = src.HP;
      BaseStaminaPoints = src.STA;
      BaseFoodPoints = baseFoodPoints;
      BaseSleepPoints = baseSleepPoints;
      BaseSanity = baseSanity;
      UnarmedAttack = new Attack(AttackKind.PHYSICAL, unarmedAttack, src.ATK, src.DMG);
      BaseDefence = new Defence(src.DEF, src.PRO_HIT, src.PRO_SHOT);
      BaseViewRange = src.FOV;
      BaseAudioRange = src.AUDIO;
      BaseSmellRating = (float) src.SMELL / 100f;
      BaseInventoryCapacity = inventoryCapacity;
    }

    public ActorSheet(ActorSheet copyFrom)
    {
      BaseHitPoints = copyFrom.BaseHitPoints;
      BaseStaminaPoints = copyFrom.BaseStaminaPoints;
      BaseFoodPoints = copyFrom.BaseFoodPoints;
      BaseSleepPoints = copyFrom.BaseSleepPoints;
      BaseSanity = copyFrom.BaseSanity;
      UnarmedAttack = copyFrom.UnarmedAttack;
      BaseDefence = copyFrom.BaseDefence;
      BaseViewRange = copyFrom.BaseViewRange;
      BaseAudioRange = copyFrom.BaseAudioRange;
      BaseSmellRating = copyFrom.BaseSmellRating;
      BaseInventoryCapacity = copyFrom.BaseInventoryCapacity;
      if (0 < copyFrom.SkillTable.CountSkills) SkillTable = new SkillTable(copyFrom.SkillTable);
    }
  }
}
