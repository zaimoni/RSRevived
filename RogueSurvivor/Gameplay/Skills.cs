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

namespace djack.RogueSurvivor.Gameplay
{
  internal static class Skills
  {
    private static string[] s_Names = new string[29];
    public static Skills.IDs[] UNDEAD_SKILLS = new Skills.IDs[9]
    {
      Skills.IDs._FIRST_UNDEAD,
      Skills.IDs.Z_EATER,
      Skills.IDs.Z_GRAB,
      Skills.IDs.Z_INFECTOR,
      Skills.IDs.Z_LIGHT_EATER,
      Skills.IDs.Z_LIGHT_FEET,
      Skills.IDs.Z_STRONG,
      Skills.IDs.Z_TOUGH,
      Skills.IDs.Z_TRACKER
    };

    public static string Name(Skills.IDs id)
    {
      return Skills.s_Names[(int) id];
    }

    public static string Name(int id)
    {
      return Skills.Name((Skills.IDs) id);
    }

    public static int MaxSkillLevel(Skills.IDs id)
    {
      return id == Skills.IDs.HAULER ? 3 : 5;
    }

    public static int MaxSkillLevel(int id)
    {
      return Skills.MaxSkillLevel((Skills.IDs) id);
    }

    public static Skills.IDs RollLiving(DiceRoller roller)
    {
      return (Skills.IDs) roller.Roll(0, 20);
    }

    public static Skills.IDs RollUndead(DiceRoller roller)
    {
      return (Skills.IDs) roller.Roll(20, 29);
    }

    private static void Notify(IRogueUI ui, string what, string stage)
    {
      ui.UI_Clear(Color.Black);
      ui.UI_DrawStringBold(Color.White, "Loading " + what + " data : " + stage, 0, 0, new Color?());
      ui.UI_Repaint();
    }

    private static CSVLine FindLineForModel(CSVTable table, Skills.IDs skillID)
    {
      foreach (CSVLine line in table.Lines)
      {
        if (line[0].ParseText() == skillID.ToString())
          return line;
      }
      return (CSVLine) null;
    }

    private static _DATA_TYPE_ GetDataFromCSVTable<_DATA_TYPE_>(IRogueUI ui, CSVTable table, Func<CSVLine, _DATA_TYPE_> fn, Skills.IDs skillID)
    {
      CSVLine lineForModel = Skills.FindLineForModel(table, skillID);
      if (lineForModel == null)
        throw new InvalidOperationException(string.Format("skill {0} not found", (object) skillID.ToString()));
      try
      {
        return fn(lineForModel);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException(string.Format("invalid data format for skill {0}; exception : {1}", (object) skillID.ToString(), (object) ex.ToString()));
      }
    }

    private static bool LoadDataFromCSV<_DATA_TYPE_>(IRogueUI ui, string path, string kind, int fieldsCount, Func<CSVLine, _DATA_TYPE_> fn, Skills.IDs[] idsToRead, out _DATA_TYPE_[] data)
    {
      Skills.Notify(ui, kind, "loading file...");
      List<string> stringList = new List<string>();
      bool flag = true;
      using (StreamReader streamReader = File.OpenText(path))
      {
        while (!streamReader.EndOfStream)
        {
          string str = streamReader.ReadLine();
          if (flag)
            flag = false;
          else
            stringList.Add(str);
        }
        streamReader.Close();
      }
      Skills.Notify(ui, kind, "parsing CSV...");
      CSVTable toTable = new CSVParser().ParseToTable(stringList.ToArray(), fieldsCount);
      Skills.Notify(ui, kind, "reading data...");
      data = new _DATA_TYPE_[idsToRead.Length];
      for (int index = 0; index < idsToRead.Length; ++index)
        data[index] = Skills.GetDataFromCSVTable<_DATA_TYPE_>(ui, toTable, fn, idsToRead[index]);
      Skills.Notify(ui, kind, "done!");
      return true;
    }

    public static bool LoadSkillsFromCSV(IRogueUI ui, string path)
    {
      Skills.SkillData[] data;
      Skills.LoadDataFromCSV<Skills.SkillData>(ui, path, "skills", 6, new Func<CSVLine, Skills.SkillData>(Skills.SkillData.FromCSVLine), new Skills.IDs[29]
      {
        Skills.IDs._FIRST,
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
        Skills.IDs._FIRST_UNDEAD,
        Skills.IDs.Z_EATER,
        Skills.IDs.Z_GRAB,
        Skills.IDs.Z_INFECTOR,
        Skills.IDs.Z_LIGHT_EATER,
        Skills.IDs.Z_LIGHT_FEET,
        Skills.IDs.Z_STRONG,
        Skills.IDs.Z_TOUGH,
        Skills.IDs.Z_TRACKER
      }, out data);
      for (int index = 0; index < 29; ++index)
        Skills.s_Names[index] = data[index].NAME;
      Skills.SkillData skillData1 = data[0];
      Rules.SKILL_AGILE_ATK_BONUS = (int) skillData1.VALUE1;
      Rules.SKILL_AGILE_DEF_BONUS = (int) skillData1.VALUE2;
      skillData1 = data[1];
      Rules.SKILL_AWAKE_SLEEP_BONUS = skillData1.VALUE1;
      Rules.SKILL_AWAKE_SLEEP_REGEN_BONUS = skillData1.VALUE2;
      skillData1 = data[2];
      Rules.SKILL_BOWS_ATK_BONUS = (int) skillData1.VALUE1;
      Rules.SKILL_BOWS_DMG_BONUS = (int) skillData1.VALUE2;
      skillData1 = data[3];
      Rules.SKILL_CARPENTRY_BARRICADING_BONUS = skillData1.VALUE1;
      Rules.SKILL_CARPENTRY_LEVEL3_BUILD_BONUS = (int) skillData1.VALUE2;
      skillData1 = data[4];
      Rules.SKILL_CHARISMATIC_TRUST_BONUS = (int) skillData1.VALUE1;
      Rules.SKILL_CHARISMATIC_TRADE_BONUS = (int) skillData1.VALUE2;
      skillData1 = data[5];
      Rules.SKILL_FIREARMS_ATK_BONUS = (int) skillData1.VALUE1;
      Rules.SKILL_FIREARMS_DMG_BONUS = (int) skillData1.VALUE2;
      skillData1 = data[6];
      Rules.SKILL_HARDY_HEAL_CHANCE_BONUS = (int) skillData1.VALUE1;
      skillData1 = data[7];
      Rules.SKILL_HAULER_INV_BONUS = (int) skillData1.VALUE1;
      skillData1 = data[8];
      Rules.SKILL_HIGH_STAMINA_STA_BONUS = (int) skillData1.VALUE1;
      skillData1 = data[9];
      Rules.SKILL_LEADERSHIP_FOLLOWER_BONUS = (int) skillData1.VALUE1;
      skillData1 = data[10];
      Rules.SKILL_LIGHT_EATER_FOOD_BONUS = skillData1.VALUE1;
      Rules.SKILL_LIGHT_EATER_MAXFOOD_BONUS = skillData1.VALUE2;
      Skills.SkillData skillData2 = data[11];
      Rules.SKILL_LIGHT_FEET_TRAP_BONUS = (int) skillData2.VALUE1;
      skillData2 = data[12];
      Rules.SKILL_LIGHT_SLEEPER_WAKEUP_CHANCE_BONUS = (int) skillData2.VALUE1;
      skillData2 = data[13];
      Rules.SKILL_MARTIAL_ARTS_ATK_BONUS = (int) skillData2.VALUE1;
      Rules.SKILL_MARTIAL_ARTS_DMG_BONUS = (int) skillData2.VALUE2;
      skillData2 = data[14];
      Rules.SKILL_MEDIC_BONUS = skillData2.VALUE1;
      Rules.SKILL_MEDIC_REVIVE_BONUS = (int) skillData2.VALUE2;
      skillData2 = data[15];
      Rules.SKILL_NECROLOGY_UNDEAD_BONUS = (int) skillData2.VALUE1;
      Rules.SKILL_NECROLOGY_CORPSE_BONUS = (int) skillData2.VALUE2;
      skillData2 = data[16];
      Rules.SKILL_STRONG_DMG_BONUS = (int) skillData2.VALUE1;
      Rules.SKILL_STRONG_THROW_BONUS = (int) skillData2.VALUE2;
      skillData2 = data[17];
      Rules.SKILL_STRONG_PSYCHE_LEVEL_BONUS = skillData2.VALUE1;
      Rules.SKILL_STRONG_PSYCHE_ENT_BONUS = skillData2.VALUE2;
      skillData2 = data[18];
      Rules.SKILL_TOUGH_HP_BONUS = (int) skillData2.VALUE1;
      skillData2 = data[19];
      Rules.SKILL_UNSUSPICIOUS_BONUS = (int) skillData2.VALUE1;
      skillData2 = data[20];
      Rules.SKILL_ZAGILE_ATK_BONUS = (int) skillData2.VALUE1;
      Rules.SKILL_ZAGILE_DEF_BONUS = (int) skillData2.VALUE2;
      skillData2 = data[21];
      Rules.SKILL_ZEATER_REGEN_BONUS = skillData2.VALUE1;
      skillData2 = data[23];
      Rules.SKILL_ZINFECTOR_BONUS = skillData2.VALUE1;
      skillData2 = data[22];
      Rules.SKILL_ZGRAB_CHANCE = (int) skillData2.VALUE1;
      skillData2 = data[24];
      Rules.SKILL_ZLIGHT_EATER_FOOD_BONUS = skillData2.VALUE1;
      Rules.SKILL_ZLIGHT_EATER_MAXFOOD_BONUS = skillData2.VALUE2;
      skillData2 = data[25];
      Rules.SKILL_ZLIGHT_FEET_TRAP_BONUS = (int) skillData2.VALUE1;
      skillData2 = data[26];
      Rules.SKILL_ZSTRONG_DMG_BONUS = (int) skillData2.VALUE1;
      skillData2 = data[27];
      Rules.SKILL_ZTOUGH_HP_BONUS = (int) skillData2.VALUE1;
      skillData2 = data[28];
      Rules.SKILL_ZTRACKER_SMELL_BONUS = skillData2.VALUE1;
      return true;
    }

    [Serializable]
    public enum IDs
    {
      AGILE = 0,
      _FIRST = 0,
      _FIRST_LIVING = 0,
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
      _LAST_LIVING = 19,
      Z_AGILE = 20,
      _FIRST_UNDEAD = 20,
      Z_EATER = 21,
      Z_GRAB = 22,
      Z_INFECTOR = 23,
      Z_LIGHT_EATER = 24,
      Z_LIGHT_FEET = 25,
      Z_STRONG = 26,
      Z_TOUGH = 27,
      Z_TRACKER = 28,
      _LAST_UNDEAD = 28,
      _COUNT = 29,
    }

    private struct SkillData
    {
      public const int COUNT_FIELDS = 6;

      public string NAME { get; set; }

      public float VALUE1 { get; set; }

      public float VALUE2 { get; set; }

      public float VALUE3 { get; set; }

      public float VALUE4 { get; set; }

      public static Skills.SkillData FromCSVLine(CSVLine line)
      {
        return new Skills.SkillData()
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
