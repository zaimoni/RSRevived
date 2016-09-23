// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorSheet
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class ActorSheet
  {
    [NonSerialized]
    public static readonly ActorSheet BLANK = new ActorSheet(0, 0, 0, 0, 0, Attack.BLANK, Defence.BLANK, 0, 0, 0, 0);
    private SkillTable m_SkillTable = new SkillTable(); // this must not be null.  readonly is incompatible with automatically generated serialization

    public int BaseHitPoints { get; private set; }
    public int BaseStaminaPoints { get; private set; }
    public int BaseFoodPoints { get; private set; }
    public int BaseSleepPoints { get; private set; }
    public int BaseSanity { get; private set; }
    public Attack UnarmedAttack { get; private set; }
    public Defence BaseDefence { get; private set; }
    public int BaseViewRange { get; private set; }
    public int BaseAudioRange { get; private set; }
    public float BaseSmellRating { get; private set; }
    public int BaseInventoryCapacity { get; private set; }

    public SkillTable SkillTable {
      get {
        return m_SkillTable;
      }
    }

    public ActorSheet(int baseHitPoints, int baseStaminaPoints, int baseFoodPoints, int baseSleepPoints, int baseSanity, Attack unarmedAttack, Defence baseDefence, int baseViewRange, int baseAudioRange, int smellRating, int inventoryCapacity)
    {
            BaseHitPoints = baseHitPoints;
            BaseStaminaPoints = baseStaminaPoints;
            BaseFoodPoints = baseFoodPoints;
            BaseSleepPoints = baseSleepPoints;
            BaseSanity = baseSanity;
            UnarmedAttack = unarmedAttack;
            BaseDefence = baseDefence;
            BaseViewRange = baseViewRange;
            BaseAudioRange = baseAudioRange;
            BaseSmellRating = (float) smellRating / 100f;
            BaseInventoryCapacity = inventoryCapacity;
    }

    public ActorSheet(ActorSheet copyFrom)
    {
      Contract.Requires(null!=copyFrom);
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
      if (copyFrom.SkillTable.Skills == null) return;
      m_SkillTable = new SkillTable(copyFrom.SkillTable.Skills);
    }
  }
}
