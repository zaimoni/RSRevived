// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.Skills
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Engine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using Actor = djack.RogueSurvivor.Data.Actor;

namespace djack.RogueSurvivor.Gameplay
{
  internal static class Skills
  {
    private readonly static string[] s_Names = new string[(int) IDs._COUNT];
    public readonly static IDs[] UNDEAD_SKILLS = new IDs[(int)IDs._COUNT - (int)IDs_aux._FIRST_UNDEAD]  // \todo adjust this naming when breaking savefile format?
    {
      IDs.Z_AGILE,
      IDs.Z_EATER,
      IDs.Z_GRAB,
      IDs.Z_INFECTOR,
      IDs.Z_LIGHT_EATER,
      IDs.Z_LIGHT_FEET,
      IDs.Z_STRONG,
      IDs.Z_TOUGH,
      IDs.Z_TRACKER
    };

    public static string Name(Skills.IDs id)
    {
      return s_Names[(int) id];
    }

    public static string Name(int id)
    {
      return Skills.Name((Skills.IDs) id);
    }

    public static int MaxSkillLevel(Skills.IDs id)
    {
      return id == Skills.IDs.HAULER ? 3 : 5;
    }

    public static Skills.IDs RollLiving(DiceRoller roller)
    {
      return (Skills.IDs) roller.Roll(0, (int)Skills.IDs_aux._LIVING_COUNT);
    }

    public static IDs RollUndead(DiceRoller roller)
    {
      return roller.Choose(UNDEAD_SKILLS);
    }

    private static void Notify(IRogueUI ui, string what, string stage)
    {
      ui.UI_Clear(Color.Black);
      ui.UI_DrawStringBold(Color.White, "Loading " + what + " data : " + stage, 0, 0, new Color?());
      ui.UI_Repaint();
    }

    private static void LoadDataFromCSV<_DATA_TYPE_>(IRogueUI ui, string path, string kind, int fieldsCount, Func<CSVLine, _DATA_TYPE_> fn, Skills.IDs[] idsToRead, out _DATA_TYPE_[] data)
    {
#if DEBUG
      if (null == ui) throw new ArgumentNullException(nameof(ui));
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path), path, "string.IsNullOrEmpty(path)");
#endif
      Skills.Notify(ui, kind, "loading file...");
      List<string> stringList = new List<string>();
      bool flag = true;
      using (StreamReader streamReader = File.OpenText(path)) {
        while (!streamReader.EndOfStream) {
          string str = streamReader.ReadLine();
          if (flag) flag = false;
          else stringList.Add(str);
        }
      }
      Notify(ui, kind, "parsing CSV...");
      CSVTable toTable = new CSVParser().ParseToTable(stringList.ToArray(), fieldsCount);
      Notify(ui, kind, "reading data...");
      data = new _DATA_TYPE_[idsToRead.Length];
      for (int index = 0; index < idsToRead.Length; ++index)
        data[index] = toTable.GetDataFor<_DATA_TYPE_, Skills.IDs>(fn, idsToRead[index]);
      Notify(ui, kind, "done!");
    }

    public static bool LoadSkillsFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      Skills.LoadDataFromCSV<Skills.SkillData>(ui, path, "skills", SkillData.COUNT_FIELDS, new Func<CSVLine, SkillData>(Skills.SkillData.FromCSVLine), new Skills.IDs[(int)Skills.IDs._COUNT]
      {
        Skills.IDs.AGILE,
        Skills.IDs.AWAKE,
        Skills.IDs.BOWS,
        Skills.IDs.CARPENTRY,
        Skills.IDs.CHARISMATIC,
        Skills.IDs.FIREARMS,
        Skills.IDs.HARDY,
        Skills.IDs.HAULER,
        Skills.IDs.HIGH_STAMINA,
        Skills.IDs.LEADERSHIP,
        Skills.IDs.LIGHT_EATER,
        Skills.IDs.LIGHT_FEET,
        Skills.IDs.LIGHT_SLEEPER,
        Skills.IDs.MARTIAL_ARTS,
        Skills.IDs.MEDIC,
        Skills.IDs.NECROLOGY,
        Skills.IDs.STRONG,
        Skills.IDs.STRONG_PSYCHE,
        Skills.IDs.TOUGH,
        Skills.IDs.UNSUSPICIOUS,
        Skills.IDs.Z_AGILE,
        Skills.IDs.Z_EATER,
        Skills.IDs.Z_GRAB,
        Skills.IDs.Z_INFECTOR,
        Skills.IDs.Z_LIGHT_EATER,
        Skills.IDs.Z_LIGHT_FEET,
        Skills.IDs.Z_STRONG,
        Skills.IDs.Z_TOUGH,
        Skills.IDs.Z_TRACKER
      }, out SkillData[] data);
      for (int index = 0; index < (int)Skills.IDs._COUNT; ++index)
        s_Names[index] = data[index].NAME;
      SkillData skillData1 = data[0];
      Actor.SKILL_AGILE_ATK_BONUS = (int) skillData1.VALUE1;
      Rules.SKILL_AGILE_DEF_BONUS = (int) skillData1.VALUE2;
      skillData1 = data[1];
      Actor.SKILL_AWAKE_SLEEP_BONUS = skillData1.VALUE1;
      Actor.SKILL_AWAKE_SLEEP_REGEN_BONUS = skillData1.VALUE2;
      skillData1 = data[2];
      Actor.SKILL_BOWS_ATK_BONUS = (int) skillData1.VALUE1;
      Actor.SKILL_BOWS_DMG_BONUS = (int) skillData1.VALUE2;
      skillData1 = data[3];
      Rules.SKILL_CARPENTRY_BARRICADING_BONUS = skillData1.VALUE1;
      Actor.SKILL_CARPENTRY_LEVEL3_BUILD_BONUS = (int) skillData1.VALUE2;
      skillData1 = data[4];
      Rules.SKILL_CHARISMATIC_TRUST_BONUS = (int) skillData1.VALUE1;
      Rules.SKILL_CHARISMATIC_TRADE_BONUS = (int) skillData1.VALUE2;
      skillData1 = data[5];
      Actor.SKILL_FIREARMS_ATK_BONUS = (int) skillData1.VALUE1;
      Actor.SKILL_FIREARMS_DMG_BONUS = (int) skillData1.VALUE2;
      skillData1 = data[6];
      Rules.SKILL_HARDY_HEAL_CHANCE_BONUS = (int) skillData1.VALUE1;
      skillData1 = data[7];
      Actor.SKILL_HAULER_INV_BONUS = (int) skillData1.VALUE1;
      skillData1 = data[8];
      Actor.SKILL_HIGH_STAMINA_STA_BONUS = (int) skillData1.VALUE1;
      skillData1 = data[9];
      Actor.SKILL_LEADERSHIP_FOLLOWER_BONUS = (int) skillData1.VALUE1;
      skillData1 = data[10];
      Actor.SKILL_LIGHT_EATER_FOOD_BONUS = skillData1.VALUE1;
      Actor.SKILL_LIGHT_EATER_MAXFOOD_BONUS = skillData1.VALUE2;
      SkillData skillData2 = data[11];
      Rules.SKILL_LIGHT_FEET_TRAP_BONUS = (int) skillData2.VALUE1;
      skillData2 = data[12];
      Rules.SKILL_LIGHT_SLEEPER_WAKEUP_CHANCE_BONUS = (int) skillData2.VALUE1;
      skillData2 = data[13];
      Actor.SKILL_MARTIAL_ARTS_ATK_BONUS = (int) skillData2.VALUE1;
      Actor.SKILL_MARTIAL_ARTS_DMG_BONUS = (int) skillData2.VALUE2;
      skillData2 = data[14];
      Rules.SKILL_MEDIC_BONUS = skillData2.VALUE1;
      Actor.SKILL_MEDIC_REVIVE_BONUS = (int) skillData2.VALUE2;
      skillData2 = data[15];
      Actor.SKILL_NECROLOGY_UNDEAD_BONUS = (int) skillData2.VALUE1;
      Rules.SKILL_NECROLOGY_CORPSE_BONUS = (int) skillData2.VALUE2;
      skillData2 = data[16];
      Actor.SKILL_STRONG_DMG_BONUS = (int) skillData2.VALUE1;
      Actor.SKILL_STRONG_THROW_BONUS = (int) skillData2.VALUE2;
      skillData2 = data[17];
      Rules.SKILL_STRONG_PSYCHE_LEVEL_BONUS = skillData2.VALUE1;
      Rules.SKILL_STRONG_PSYCHE_ENT_BONUS = skillData2.VALUE2;
      skillData2 = data[18];
      Actor.SKILL_TOUGH_HP_BONUS = (int) skillData2.VALUE1;
      skillData2 = data[19];
      Rules.SKILL_UNSUSPICIOUS_BONUS = (int) skillData2.VALUE1;
      skillData2 = data[20];
      Actor.SKILL_ZAGILE_ATK_BONUS = (int) skillData2.VALUE1;
      Rules.SKILL_ZAGILE_DEF_BONUS = (int) skillData2.VALUE2;
      skillData2 = data[21];
      Rules.SKILL_ZEATER_REGEN_BONUS = skillData2.VALUE1;
      skillData2 = data[23];
      Rules.SKILL_ZINFECTOR_BONUS = skillData2.VALUE1;
      skillData2 = data[22];
      Rules.SKILL_ZGRAB_CHANCE = (int) skillData2.VALUE1;
      skillData2 = data[24];
      Actor.SKILL_ZLIGHT_EATER_FOOD_BONUS = skillData2.VALUE1;
      Actor.SKILL_ZLIGHT_EATER_MAXFOOD_BONUS = skillData2.VALUE2;
      skillData2 = data[25];
      Rules.SKILL_ZLIGHT_FEET_TRAP_BONUS = (int) skillData2.VALUE1;
      skillData2 = data[26];
      Actor.SKILL_ZSTRONG_DMG_BONUS = (int) skillData2.VALUE1;
      skillData2 = data[27];
      Actor.SKILL_ZTOUGH_HP_BONUS = (int) skillData2.VALUE1;
      skillData2 = data[28];
      Actor.SKILL_ZTRACKER_SMELL_BONUS = skillData2.VALUE1;
      return true;
    }

    [Serializable]
    public enum IDs
    {
      AGILE = 0,
      AWAKE = 1,
      BOWS = 2,
      CARPENTRY = 3,
      CHARISMATIC = 4,
      FIREARMS = 5,
      HARDY = 6,
      HAULER = 7,
      HIGH_STAMINA = 8,
      LEADERSHIP = 9,
      LIGHT_EATER = 10,
      LIGHT_FEET = 11,
      LIGHT_SLEEPER = 12,
      MARTIAL_ARTS = 13,
      MEDIC = 14,
      NECROLOGY = 15,
      STRONG = 16,
      STRONG_PSYCHE = 17,
      TOUGH = 18,
      UNSUSPICIOUS = 19,
      Z_AGILE = 20,
      Z_EATER = 21,
      Z_GRAB = 22,
      Z_INFECTOR = 23,
      Z_LIGHT_EATER = 24,
      Z_LIGHT_FEET = 25,
      Z_STRONG = 26,
      Z_TOUGH = 27,
      Z_TRACKER = 28,
      _COUNT = 29,
    }

    public enum IDs_aux {
      _LIVING_COUNT = IDs.Z_AGILE,
      _FIRST_UNDEAD = IDs.Z_AGILE,
    }

    public static IDs? Zombify(this IDs skill)
    {
      switch (skill) {
        case IDs.AGILE: return IDs.Z_AGILE;
        case IDs.LIGHT_EATER: return IDs.Z_LIGHT_EATER;
        case IDs.LIGHT_FEET: return IDs.Z_LIGHT_FEET;
        case IDs.MEDIC: return IDs.Z_INFECTOR;
        case IDs.STRONG: return IDs.Z_STRONG;
        case IDs.TOUGH: return IDs.Z_TOUGH;
        default: return null;
      }
    }

    private struct SkillData
    {
      public const int COUNT_FIELDS = 6;

      public string NAME;
      public float VALUE1;
      public float VALUE2;
      public float VALUE3;
      public float VALUE4;

      public static SkillData FromCSVLine(CSVLine line)
      {
        return new SkillData
        {
          NAME = line[1].ParseText(),
          VALUE1 = line[2].ParseFloat(),
          VALUE2 = line[3].ParseFloat(),
          VALUE3 = line[4].ParseFloat(),
          VALUE4 = line[5].ParseFloat()
        };
      }
    }
  }
}
