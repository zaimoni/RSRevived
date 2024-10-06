// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.GameActors
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Gameplay.AI;
using djack.RogueSurvivor.UI;
using System;
using System.Collections.Generic;
using System.IO;
using Zaimoni.Data;

namespace djack.RogueSurvivor.Gameplay
{
  public static class GameActors
  {
    private static readonly ActorModel[] m_Models = new ActorModel[(int) IDs._COUNT];
    private const int HUMAN_STA = 2*WorldTime.TURNS_PER_HOUR;
    public const int HUMAN_AUDIO = 8*WorldTime.TURNS_PER_HOUR/15; // Also max hearing range; swamp thing also hears but much lower range
    private const int DOG_STA = 3*WorldTime.TURNS_PER_HOUR;

#nullable enable
    static public ActorModel From(IDs id) { return m_Models[(int)id]; }

    private static void _setModel(ActorModel model)
    {
#if DEBUG
        if (null != m_Models[(int) model.ID]) throw new InvalidOperationException("can only set actor model once");
#endif
        m_Models[(int) model.ID] = model;
    }

    static public ActorModel Skeleton { get { return m_Models[(int) IDs.UNDEAD_SKELETON]; } }
    static public ActorModel Red_Eyed_Skeleton { get { return m_Models[(int) IDs.UNDEAD_RED_EYED_SKELETON]; } }
    static public ActorModel Red_Skeleton { get { return m_Models[(int) IDs.UNDEAD_RED_SKELETON]; } }
    static public ActorModel Zombie { get { return m_Models[(int) IDs.UNDEAD_ZOMBIE]; } }
    static public ActorModel DarkEyedZombie { get { return m_Models[(int) IDs.UNDEAD_DARK_EYED_ZOMBIE]; } }
    static public ActorModel DarkZombie { get { return m_Models[(int) IDs.UNDEAD_DARK_ZOMBIE]; } }
    static public ActorModel ZombieMaster { get { return m_Models[(int) IDs.UNDEAD_ZOMBIE_MASTER]; } }
    static public ActorModel ZombieLord { get { return m_Models[(int) IDs.UNDEAD_ZOMBIE_LORD]; } }
    static public ActorModel ZombiePrince { get { return m_Models[(int) IDs.UNDEAD_ZOMBIE_PRINCE]; } }
    static public ActorModel MaleZombified { get { return m_Models[(int) IDs.UNDEAD_MALE_ZOMBIFIED]; } }
    static public ActorModel FemaleZombified { get { return m_Models[(int) IDs.UNDEAD_FEMALE_ZOMBIFIED]; } }
    static public ActorModel MaleNeophyte { get { return m_Models[(int) IDs.UNDEAD_MALE_NEOPHYTE]; } }
    static public ActorModel FemaleNeophyte { get { return m_Models[(int) IDs.UNDEAD_FEMALE_NEOPHYTE]; } }
    static public ActorModel MaleDisciple { get { return m_Models[(int) IDs.UNDEAD_MALE_DISCIPLE]; } }
    static public ActorModel FemaleDisciple { get { return m_Models[(int) IDs.UNDEAD_FEMALE_DISCIPLE]; } }
    static public ActorModel RatZombie { get { return m_Models[(int) IDs.UNDEAD_RAT_ZOMBIE]; } }
    static public ActorModel SewersThing { get { return m_Models[(int) IDs.SEWERS_THING]; } }
    static public ActorModel MaleCivilian { get { return m_Models[(int) IDs.MALE_CIVILIAN]; } }
    static public ActorModel FemaleCivilian { get { return m_Models[(int) IDs.FEMALE_CIVILIAN]; } }
    static public ActorModel FeralDog { get { return m_Models[(int) IDs.FERAL_DOG]; } }
    static public ActorModel CHARGuard { get { return m_Models[(int) IDs.CHAR_GUARD]; } }
    static public ActorModel NationalGuard { get { return m_Models[(int) IDs.ARMY_NATIONAL_GUARD]; } }
    static public ActorModel BikerMan { get { return m_Models[(int) IDs.BIKER_MAN]; } }
    static public ActorModel GangstaMan { get { return m_Models[(int) IDs.GANGSTA_MAN]; } }
    static public ActorModel Policeman { get { return m_Models[(int) IDs.POLICEMAN]; } }
    static public ActorModel BlackOps { get { return m_Models[(int) IDs.BLACKOPS_MAN]; } }
    static public ActorModel JasonMyers { get { return m_Models[(int) IDs.JASON_MYERS]; } }

    public static void Init(IRogueUI ui) {
      LoadFromCSV(ui, Path.Combine("Resources", "Data", "Actors.csv"));
    }
#nullable restore

/*
to transform from MALE_CIVILIAN to POLICEMAN:
* man -> policeman
* men -> policemen
* flavor text becomes To protect and to die
* add  | Abilities.Flags.IS_LAW_ENFORCER
*/
    private static ActorData ZFeminize(ActorData src) {
      ActorData ret = src;
      ret.DMG -= 2;
      ret.DEF += 2;
      ret.NAME = src.NAME.Feminine();
      ret.PLURAL = ret.NAME.Plural(true);
      return ret;
    }

    private static ActorData LFeminize(ActorData src) { // only for formal verifiability
      ActorData ret = src;
      ret.HP -= 2;
      ret.DMG -= 2;
      ret.DEF += 2;
      ret.NAME = src.NAME.Feminine();
      ret.PLURAL = ret.NAME.Plural(true);
      return ret;
    }

    static private void CreateModels(CSVTable toTable)
    {
      const Abilities.Flags STD_ZOMBIFYING = Abilities.Flags.IS_ROTTING | Abilities.Flags.CAN_ZOMBIFY_KILLED | Abilities.Flags.CAN_BASH_DOORS | Abilities.Flags.CAN_BREAK_OBJECTS | Abilities.Flags.ZOMBIEAI_EXPLORE | Abilities.Flags.AI_CAN_USE_AI_EXITS;
      const Abilities.Flags STD_ZM = Abilities.Flags.CAN_USE_MAP_OBJECTS | Abilities.Flags.CAN_JUMP | Abilities.Flags.CAN_JUMP_STUMBLE;
      const Abilities.Flags STD_LIVING = Abilities.Flags.HAS_INVENTORY | Abilities.Flags.CAN_BREAK_OBJECTS | Abilities.Flags.CAN_JUMP | Abilities.Flags.CAN_TIRE | Abilities.Flags.CAN_RUN;
      const Abilities.Flags STD_HUMAN = Abilities.Flags.CAN_USE_MAP_OBJECTS | Abilities.Flags.CAN_USE_ITEMS | Abilities.Flags.CAN_TALK | Abilities.Flags.CAN_PUSH | Abilities.Flags.CAN_BARRICADE;
      const Abilities.Flags STD_SANE = Abilities.Flags.HAS_SANITY | Abilities.Flags.HAS_TO_SLEEP | Abilities.Flags.IS_INTELLIGENT | Abilities.Flags.CAN_TRADE;  // RS Alpha 10.1- did not allow soldiers or CHAR to trade

      static ActorData parse_fn(CSVLine CSV) { return new ActorData(CSV); }

      ActorData DATA_SKELETON = toTable.GetDataFor(parse_fn, IDs.UNDEAD_SKELETON);
      ActorData DATA_RED_EYED_SKELETON = toTable.GetDataFor(parse_fn, IDs.UNDEAD_RED_EYED_SKELETON);
      ActorData DATA_RED_SKELETON = toTable.GetDataFor(parse_fn, IDs.UNDEAD_RED_SKELETON);
      ActorData DATA_ZOMBIE = toTable.GetDataFor(parse_fn, IDs.UNDEAD_ZOMBIE);
      ActorData DATA_DARK_EYED_ZOMBIE = toTable.GetDataFor(parse_fn, IDs.UNDEAD_DARK_EYED_ZOMBIE);
      ActorData DATA_DARK_ZOMBIE = toTable.GetDataFor(parse_fn, IDs.UNDEAD_DARK_ZOMBIE);
      ActorData DATA_MALE_ZOMBIFIED = toTable.GetDataFor(parse_fn, IDs.UNDEAD_MALE_ZOMBIFIED);
      ActorData DATA_FEMALE_ZOMBIFIED = ZFeminize(DATA_MALE_ZOMBIFIED);
      ActorData DATA_MALE_NEOPHYTE = toTable.GetDataFor(parse_fn, IDs.UNDEAD_MALE_NEOPHYTE);
      ActorData DATA_FEMALE_NEOPHYTE = ZFeminize(DATA_MALE_NEOPHYTE);
      ActorData DATA_MALE_DISCIPLE = toTable.GetDataFor(parse_fn, IDs.UNDEAD_MALE_DISCIPLE);
      ActorData DATA_FEMALE_DISCIPLE = ZFeminize(DATA_MALE_DISCIPLE);
      ActorData DATA_ZM = toTable.GetDataFor(parse_fn, IDs.UNDEAD_ZOMBIE_MASTER);
      ActorData DATA_ZL = toTable.GetDataFor(parse_fn, IDs.UNDEAD_ZOMBIE_LORD);
      ActorData DATA_ZP = toTable.GetDataFor(parse_fn, IDs.UNDEAD_ZOMBIE_PRINCE);
      ActorData DATA_RAT_ZOMBIE = toTable.GetDataFor(parse_fn, IDs.UNDEAD_RAT_ZOMBIE);
      ActorData DATA_SEWERS_THING = toTable.GetDataFor(parse_fn, IDs.SEWERS_THING);
      ActorData DATA_MALE_CIVILIAN = toTable.GetDataFor(parse_fn, IDs.MALE_CIVILIAN);
      ActorData DATA_FEMALE_CIVILIAN = LFeminize(DATA_MALE_CIVILIAN);
      ActorData DATA_FERAL_DOG = toTable.GetDataFor(parse_fn, IDs.FERAL_DOG);
      ActorData DATA_POLICEMAN = toTable.GetDataFor(parse_fn, IDs.POLICEMAN);  // XXX to be synthesized
      ActorData DATA_POLICEWOMAN = LFeminize(DATA_POLICEMAN);
      ActorData DATA_CHAR_GUARD = toTable.GetDataFor(parse_fn, IDs.CHAR_GUARD);  // XXX to be synthesized
      ActorData DATA_NATGUARD = toTable.GetDataFor(parse_fn, IDs.ARMY_NATIONAL_GUARD);  // XXX to be synthesized
      ActorData DATA_BIKER_MAN = toTable.GetDataFor(parse_fn, IDs.BIKER_MAN);  // XXX to be synthesized
      ActorData DATA_GANGSTA_MAN = toTable.GetDataFor(parse_fn, IDs.GANGSTA_MAN);  // XXX to be synthesized
      ActorData DATA_BLACKOPS_MAN = toTable.GetDataFor(parse_fn, IDs.BLACKOPS_MAN);  // XXX to be synthesized
      ActorData DATA_JASON_MYERS = toTable.GetDataFor(parse_fn, IDs.JASON_MYERS);  // XXX to be synthesized

      // XXX postprocessing stage should go here
      // XXX stamina column was reconnected with the constructor changes
      // XXX pro hit and pro shot columns are currently constant zero but that's fine for now
      // male to female delta: -2 HP -2 DAM +2 DEF
      // * the z-human chain doesn't use HP, does use DAM/DEF
      // * dynamic test is: .IsFemale() [implied above]
      // so as long as Jason myers is interpolated correctly for SPD from the male civilian, we can decommission all other human stat columsn and just leave the text configuration
      // NOTE: AudioRange space-time scales.  Currently, zero or 16==3*LOUD_NOISE_RADIUS+1

      _setModel(new ActorModel(IDs.UNDEAD_SKELETON, GameImages.ACTOR_SKELETON, DATA_SKELETON.NAME, DATA_SKELETON.PLURAL, DATA_SKELETON.SCORE, DATA_SKELETON.FLAVOR, new DollBody(true, DATA_SKELETON.SPD), new Abilities(
          Abilities.Flags.UNDEAD),
          DATA_SKELETON, typeof (SkeletonAI)));
      _setModel(new ActorModel(IDs.UNDEAD_RED_EYED_SKELETON, GameImages.ACTOR_RED_EYED_SKELETON, DATA_RED_EYED_SKELETON.NAME, DATA_RED_EYED_SKELETON.PLURAL, DATA_RED_EYED_SKELETON.SCORE, DATA_RED_EYED_SKELETON.FLAVOR, new DollBody(true, DATA_RED_EYED_SKELETON.SPD), new Abilities(
          Abilities.Flags.UNDEAD),
          DATA_RED_EYED_SKELETON, typeof (SkeletonAI)));
      _setModel(new ActorModel(IDs.UNDEAD_RED_SKELETON, GameImages.ACTOR_RED_SKELETON, DATA_RED_SKELETON.NAME, DATA_RED_SKELETON.PLURAL, DATA_RED_SKELETON.SCORE, DATA_RED_SKELETON.FLAVOR, new DollBody(true, DATA_RED_SKELETON.SPD), new Abilities(
          Abilities.Flags.UNDEAD),
          DATA_RED_SKELETON, typeof(SkeletonAI)));
      _setModel(new ActorModel(IDs.UNDEAD_ZOMBIE, GameImages.ACTOR_ZOMBIE, DATA_ZOMBIE.NAME, DATA_ZOMBIE.PLURAL, DATA_ZOMBIE.SCORE, DATA_ZOMBIE.FLAVOR, new DollBody(true, DATA_ZOMBIE.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING),
          DATA_ZOMBIE, typeof (ZombieAI)));
      _setModel(new ActorModel(IDs.UNDEAD_DARK_EYED_ZOMBIE, GameImages.ACTOR_DARK_EYED_ZOMBIE, DATA_DARK_EYED_ZOMBIE.NAME, DATA_DARK_EYED_ZOMBIE.PLURAL, DATA_DARK_EYED_ZOMBIE.SCORE, DATA_DARK_EYED_ZOMBIE.FLAVOR, new DollBody(true, DATA_DARK_EYED_ZOMBIE.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING),
          DATA_DARK_EYED_ZOMBIE, typeof(ZombieAI)));
      _setModel(new ActorModel(IDs.UNDEAD_DARK_ZOMBIE, GameImages.ACTOR_DARK_ZOMBIE, DATA_DARK_ZOMBIE.NAME, DATA_DARK_ZOMBIE.PLURAL, DATA_DARK_ZOMBIE.SCORE, DATA_DARK_ZOMBIE.FLAVOR, new DollBody(true, DATA_DARK_ZOMBIE.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING),
          DATA_DARK_ZOMBIE, typeof(ZombieAI)));
      _setModel(new ActorModel(IDs.UNDEAD_MALE_ZOMBIFIED, null, DATA_MALE_ZOMBIFIED.NAME, DATA_MALE_ZOMBIFIED.PLURAL, DATA_MALE_ZOMBIFIED.SCORE, DATA_MALE_ZOMBIFIED.FLAVOR, new DollBody(true, DATA_MALE_ZOMBIFIED.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING),
          DATA_MALE_ZOMBIFIED, typeof(ZombieAI)));
      _setModel(new ActorModel(IDs.UNDEAD_FEMALE_ZOMBIFIED, null, DATA_FEMALE_ZOMBIFIED.NAME, DATA_FEMALE_ZOMBIFIED.PLURAL, DATA_FEMALE_ZOMBIFIED.SCORE, DATA_FEMALE_ZOMBIFIED.FLAVOR, new DollBody(true, DATA_FEMALE_ZOMBIFIED.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING),
          DATA_FEMALE_ZOMBIFIED, typeof(ZombieAI)));
      _setModel(new ActorModel(IDs.UNDEAD_MALE_NEOPHYTE, GameImages.ACTOR_MALE_NEOPHYTE, DATA_MALE_NEOPHYTE.NAME, DATA_MALE_NEOPHYTE.PLURAL, DATA_MALE_NEOPHYTE.SCORE, DATA_MALE_NEOPHYTE.FLAVOR, new DollBody(true, DATA_MALE_NEOPHYTE.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING | Abilities.Flags.CAN_PUSH),
          DATA_MALE_NEOPHYTE, typeof(ZombieAI)));
      _setModel(new ActorModel(IDs.UNDEAD_FEMALE_NEOPHYTE, GameImages.ACTOR_FEMALE_NEOPHYTE, DATA_FEMALE_NEOPHYTE.NAME, DATA_FEMALE_NEOPHYTE.PLURAL, DATA_FEMALE_NEOPHYTE.SCORE, DATA_FEMALE_NEOPHYTE.FLAVOR, new DollBody(true, DATA_FEMALE_NEOPHYTE.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING | Abilities.Flags.CAN_PUSH),
          DATA_FEMALE_NEOPHYTE, typeof(ZombieAI)));
      _setModel(new ActorModel(IDs.UNDEAD_MALE_DISCIPLE, GameImages.ACTOR_MALE_DISCIPLE, DATA_MALE_DISCIPLE.NAME, DATA_MALE_DISCIPLE.PLURAL, DATA_MALE_DISCIPLE.SCORE, DATA_MALE_DISCIPLE.FLAVOR, new DollBody(true, DATA_MALE_DISCIPLE.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING | Abilities.Flags.CAN_PUSH),
          DATA_MALE_DISCIPLE, typeof(ZombieAI)));
      _setModel(new ActorModel(IDs.UNDEAD_FEMALE_DISCIPLE, GameImages.ACTOR_FEMALE_DISCIPLE, DATA_FEMALE_DISCIPLE.NAME, DATA_FEMALE_DISCIPLE.PLURAL, DATA_FEMALE_DISCIPLE.SCORE, DATA_FEMALE_DISCIPLE.FLAVOR, new DollBody(true, DATA_FEMALE_DISCIPLE.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING | Abilities.Flags.CAN_PUSH),
          DATA_FEMALE_DISCIPLE, typeof(ZombieAI)));
      _setModel(new ActorModel(IDs.UNDEAD_ZOMBIE_MASTER, GameImages.ACTOR_ZOMBIE_MASTER, DATA_ZM.NAME, DATA_ZM.PLURAL, DATA_ZM.SCORE, DATA_ZM.FLAVOR, new DollBody(true, DATA_ZM.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING | Abilities.Flags.CAN_PUSH |
          Abilities.Flags.UNDEAD_MASTER | STD_ZM),
          DATA_ZM, typeof(ZombieAI)));
      _setModel(new ActorModel(IDs.UNDEAD_ZOMBIE_LORD, GameImages.ACTOR_ZOMBIE_LORD, DATA_ZL.NAME, DATA_ZL.PLURAL, DATA_ZL.SCORE, DATA_ZL.FLAVOR, new DollBody(true, DATA_ZL.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING | Abilities.Flags.CAN_PUSH |
          Abilities.Flags.UNDEAD_MASTER | STD_ZM),
          DATA_ZL, typeof(ZombieAI)));
      _setModel(new ActorModel(IDs.UNDEAD_ZOMBIE_PRINCE, GameImages.ACTOR_ZOMBIE_PRINCE, DATA_ZP.NAME, DATA_ZP.PLURAL, DATA_ZP.SCORE, DATA_ZP.FLAVOR, new DollBody(true, DATA_ZP.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING | Abilities.Flags.CAN_PUSH |
          Abilities.Flags.UNDEAD_MASTER | STD_ZM),
          DATA_ZP, typeof(ZombieAI)));
      _setModel(new ActorModel(IDs.UNDEAD_RAT_ZOMBIE, GameImages.ACTOR_RAT_ZOMBIE, DATA_RAT_ZOMBIE.NAME, DATA_RAT_ZOMBIE.PLURAL, DATA_RAT_ZOMBIE.SCORE, DATA_RAT_ZOMBIE.FLAVOR, new DollBody(true, DATA_RAT_ZOMBIE.SPD), new Abilities(
          Abilities.Flags.UNDEAD | Abilities.Flags.IS_SMALL | Abilities.Flags.AI_CAN_USE_AI_EXITS),
          DATA_RAT_ZOMBIE, typeof (RatAI)));
      _setModel(new ActorModel(IDs.SEWERS_THING, GameImages.ACTOR_SEWERS_THING, DATA_SEWERS_THING.NAME, DATA_SEWERS_THING.PLURAL, DATA_SEWERS_THING.SCORE, DATA_SEWERS_THING.FLAVOR, new DollBody(true, DATA_SEWERS_THING.SPD), new Abilities(
          Abilities.Flags.UNDEAD | Abilities.Flags.CAN_BASH_DOORS | Abilities.Flags.CAN_BREAK_OBJECTS),
          DATA_SEWERS_THING, typeof (SewersThingAI)));
      _setModel(new ActorModel(IDs.MALE_CIVILIAN, null, DATA_MALE_CIVILIAN.NAME, DATA_MALE_CIVILIAN.PLURAL, DATA_MALE_CIVILIAN.SCORE, DATA_MALE_CIVILIAN.FLAVOR, new DollBody(true, DATA_MALE_CIVILIAN.SPD), new Abilities(
          STD_LIVING | STD_HUMAN | STD_SANE |
          Abilities.Flags.HAS_TO_EAT | Abilities.Flags.AI_CAN_USE_AI_EXITS),
          DATA_MALE_CIVILIAN, typeof (CivilianAI)));
      _setModel(new ActorModel(IDs.FEMALE_CIVILIAN, null, DATA_FEMALE_CIVILIAN.NAME, DATA_FEMALE_CIVILIAN.PLURAL, DATA_FEMALE_CIVILIAN.SCORE, DATA_FEMALE_CIVILIAN.FLAVOR, new DollBody(false, DATA_FEMALE_CIVILIAN.SPD), new Abilities(
          STD_LIVING | STD_HUMAN | STD_SANE |
          Abilities.Flags.HAS_TO_EAT | Abilities.Flags.AI_CAN_USE_AI_EXITS),
          DATA_FEMALE_CIVILIAN, typeof (CivilianAI)));
      _setModel(new ActorModel(IDs.CHAR_GUARD, null, DATA_CHAR_GUARD.NAME, DATA_CHAR_GUARD.PLURAL, DATA_CHAR_GUARD.SCORE, DATA_CHAR_GUARD.FLAVOR, new DollBody(true, DATA_CHAR_GUARD.SPD), new Abilities(
          STD_LIVING | STD_HUMAN | STD_SANE),
          DATA_CHAR_GUARD, typeof (CHARGuardAI)));
      _setModel(new ActorModel(IDs.ARMY_NATIONAL_GUARD, null, DATA_NATGUARD.NAME, DATA_NATGUARD.PLURAL, DATA_NATGUARD.SCORE, DATA_NATGUARD.FLAVOR, new DollBody(true, DATA_NATGUARD.SPD), new Abilities(
          STD_LIVING | STD_HUMAN | STD_SANE),
          DATA_NATGUARD, typeof (SoldierAI)));
      _setModel(new ActorModel(IDs.BIKER_MAN, null, DATA_BIKER_MAN.NAME, DATA_BIKER_MAN.PLURAL, DATA_BIKER_MAN.SCORE, DATA_BIKER_MAN.FLAVOR, new DollBody(true, DATA_BIKER_MAN.SPD), new Abilities(
          STD_LIVING | STD_HUMAN | STD_SANE |
          Abilities.Flags.HAS_TO_EAT | Abilities.Flags.AI_NOT_INTERESTED_IN_RANGED_WEAPONS),
          DATA_BIKER_MAN, typeof (GangAI)));
      _setModel(new ActorModel(IDs.GANGSTA_MAN, null, DATA_GANGSTA_MAN.NAME, DATA_GANGSTA_MAN.PLURAL, DATA_GANGSTA_MAN.SCORE, DATA_GANGSTA_MAN.FLAVOR, new DollBody(true, DATA_GANGSTA_MAN.SPD), new Abilities(
          STD_LIVING | STD_HUMAN | STD_SANE |
          Abilities.Flags.HAS_TO_EAT),
          DATA_GANGSTA_MAN, typeof(GangAI)));
      _setModel(new ActorModel(IDs.POLICEMAN, null, DATA_POLICEMAN.NAME, DATA_POLICEMAN.PLURAL, DATA_POLICEMAN.SCORE, DATA_POLICEMAN.FLAVOR, new DollBody(true, DATA_POLICEMAN.SPD), new Abilities(
          STD_LIVING | STD_HUMAN | STD_SANE |
          Abilities.Flags.HAS_TO_EAT | Abilities.Flags.AI_CAN_USE_AI_EXITS | Abilities.Flags.IS_LAW_ENFORCER),
          DATA_POLICEMAN, typeof(CivilianAI)));
      _setModel(new ActorModel(IDs.BLACKOPS_MAN, null, DATA_BLACKOPS_MAN.NAME, DATA_BLACKOPS_MAN.PLURAL, DATA_BLACKOPS_MAN.SCORE, DATA_BLACKOPS_MAN.FLAVOR, new DollBody(true, DATA_BLACKOPS_MAN.SPD), new Abilities(
          STD_LIVING | STD_HUMAN | STD_SANE),
          DATA_BLACKOPS_MAN, typeof(SoldierAI)));
      _setModel(new ActorModel(IDs.FERAL_DOG, null, DATA_FERAL_DOG.NAME, DATA_FERAL_DOG.PLURAL, DATA_FERAL_DOG.SCORE, DATA_FERAL_DOG.FLAVOR, new DollBody(true, DATA_FERAL_DOG.SPD), new Abilities(
          STD_LIVING |
          Abilities.Flags.HAS_TO_EAT | Abilities.Flags.HAS_TO_SLEEP | Abilities.Flags.AI_CAN_USE_AI_EXITS),
          DATA_FERAL_DOG, typeof (FeralDogAI)));
      _setModel(new ActorModel(IDs.JASON_MYERS, null, DATA_JASON_MYERS.NAME, DATA_JASON_MYERS.PLURAL, DATA_JASON_MYERS.SCORE, DATA_JASON_MYERS.FLAVOR, new DollBody(true, DATA_JASON_MYERS.SPD), new Abilities(
          STD_LIVING | STD_HUMAN |
          Abilities.Flags.AI_CAN_USE_AI_EXITS),
          DATA_JASON_MYERS, typeof (InsaneHumanAI)));
      _setModel(new ActorModel(IDs.POLICEWOMAN, null, DATA_POLICEWOMAN.NAME, DATA_POLICEWOMAN.PLURAL, DATA_POLICEWOMAN.SCORE, DATA_POLICEWOMAN.FLAVOR, new DollBody(false, DATA_POLICEWOMAN.SPD), new Abilities(
          STD_LIVING | STD_HUMAN | STD_SANE |
          Abilities.Flags.HAS_TO_EAT | Abilities.Flags.AI_CAN_USE_AI_EXITS | Abilities.Flags.IS_LAW_ENFORCER),
          DATA_POLICEWOMAN, typeof(CivilianAI)));
    }

    private static void LoadFromCSV(IRogueUI ui, string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path), path, "string.IsNullOrEmpty(path)");
#endif
      Notify(ui, "loading file...");
      List<string> stringList = new List<string>();
      bool flag = true;
      using (StreamReader streamReader = File.OpenText(path)) {
        while (!streamReader.EndOfStream) {
          string str = streamReader.ReadLine();
          if (flag) flag = false;
          else stringList.Add(str);
        }
      }
      Notify(ui, "parsing CSV...");
      CSVTable toTable = new CSVParser().ParseToTable(stringList.ToArray(), ActorData.COUNT_FIELDS);
      Notify(ui, "reading data...");
      CreateModels(toTable);
      Notify(ui, "done!");
    }

    private static void Notify(IRogueUI ui, string stage) => ui.DrawHeadNote("Loading actors data : " + stage);

#if DEAD_FUNC
    static public bool IsZombifiedBranch(ActorModel m)
    {
      return m == MaleZombified || m == FemaleZombified
          || m == MaleNeophyte  || m == FemaleNeophyte
          || m == MaleDisciple  || m == FemaleDisciple;
    }

    static public bool IsZMBranch(ActorModel m)
    {
      return m == ZombieMaster || m == ZombieLord || m == ZombiePrince;
    }
#endif

#nullable enable
    static public bool IsSkeletonBranch(ActorModel m)
    {
      return m == Skeleton || m == Red_Eyed_Skeleton || m == Red_Skeleton;
    }

    static public bool IsShamblerBranch(ActorModel m)
    {
      return m == Zombie || m == DarkEyedZombie || m == DarkZombie;
    }

    static public bool IsRatBranch(ActorModel m)
    {
      return m == RatZombie;
    }
#nullable restore

    public enum IDs
    {
      UNDEAD_SKELETON = 0,
      UNDEAD_RED_EYED_SKELETON = 1,
      UNDEAD_RED_SKELETON = 2,
      UNDEAD_ZOMBIE = 3,
      UNDEAD_DARK_EYED_ZOMBIE = 4,
      UNDEAD_DARK_ZOMBIE = 5,
      UNDEAD_ZOMBIE_MASTER = 6,
      UNDEAD_ZOMBIE_LORD = 7,
      UNDEAD_ZOMBIE_PRINCE = 8,
      UNDEAD_MALE_ZOMBIFIED = 9,
      UNDEAD_FEMALE_ZOMBIFIED = 10,
      UNDEAD_MALE_NEOPHYTE = 11,
      UNDEAD_FEMALE_NEOPHYTE = 12,
      UNDEAD_MALE_DISCIPLE = 13,
      UNDEAD_FEMALE_DISCIPLE = 14,
      UNDEAD_RAT_ZOMBIE = 15,
      MALE_CIVILIAN = 16,
      FEMALE_CIVILIAN = 17,
      FERAL_DOG = 18,
      CHAR_GUARD = 19,
      ARMY_NATIONAL_GUARD = 20,
      BIKER_MAN = 21,
      POLICEMAN = 22,
      GANGSTA_MAN = 23,
      BLACKOPS_MAN = 24,
      SEWERS_THING = 25,
      JASON_MYERS = 26,
      POLICEWOMAN = 27,
      _COUNT = 28,
    }

    public struct ActorData
    {
      public const int COUNT_FIELDS = 16;   // in the CSV: one more than the fields visible here

      public string NAME;
      public string PLURAL;
      public int SPD;
      public int HP;
      public int STA;
      public int ATK;
      public int DMG;
      public int DEF;
      public int PRO_HIT;
      public int PRO_SHOT;
      public int FOV;
      public int AUDIO;
      public int SMELL;
      public int SCORE;
      public string FLAVOR;
      public ActorData(CSVLine line)
      {
        NAME = line[1].ParseText();
        PLURAL = line[2].ParseText();
        SPD = line[3].ParseInt();
        HP = line[4].ParseInt();
        STA = line[5].ParseInt()*WorldTime.TURNS_PER_HOUR/30; // spacetime scales; disconnected
        ATK = line[6].ParseInt();
        DMG = line[7].ParseInt();
        DEF = line[8].ParseInt();
        PRO_HIT = line[9].ParseInt();
        PRO_SHOT = line[10].ParseInt();
        FOV = line[11].ParseInt() * WorldTime.TURNS_PER_HOUR / 30; // spacetime scales
        AUDIO = line[12].ParseInt() * WorldTime.TURNS_PER_HOUR / 30; // spacetime scales; disconnected
        SMELL = line[13].ParseInt();
        SCORE = line[14].ParseInt();
        FLAVOR = line[15].ParseText();
      }
    }
  }

	internal static class ID_ext
	{
	  internal static GameActors.IDs NextUndeadEvolution(this GameActors.IDs from)
	  {
#if DEBUG
        if (!RogueGame.Options.AllowUndeadsEvolution || !Session.Get.HasEvolution) throw new InvalidOperationException("game options disallow undead evolution");
#endif
        switch (from) {
          case GameActors.IDs.UNDEAD_SKELETON: return GameActors.IDs.UNDEAD_RED_EYED_SKELETON;
          case GameActors.IDs.UNDEAD_RED_EYED_SKELETON: return GameActors.IDs.UNDEAD_RED_SKELETON;
          case GameActors.IDs.UNDEAD_ZOMBIE: return GameActors.IDs.UNDEAD_DARK_EYED_ZOMBIE;
          case GameActors.IDs.UNDEAD_DARK_EYED_ZOMBIE: return GameActors.IDs.UNDEAD_DARK_ZOMBIE;
          case GameActors.IDs.UNDEAD_ZOMBIE_MASTER: return GameActors.IDs.UNDEAD_ZOMBIE_LORD;
          case GameActors.IDs.UNDEAD_ZOMBIE_LORD: return GameActors.IDs.UNDEAD_ZOMBIE_PRINCE;
          case GameActors.IDs.UNDEAD_MALE_ZOMBIFIED: return GameActors.IDs.UNDEAD_MALE_NEOPHYTE;
          case GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED: return GameActors.IDs.UNDEAD_FEMALE_NEOPHYTE;
          case GameActors.IDs.UNDEAD_MALE_NEOPHYTE: return GameActors.IDs.UNDEAD_MALE_DISCIPLE;
          case GameActors.IDs.UNDEAD_FEMALE_NEOPHYTE: return GameActors.IDs.UNDEAD_FEMALE_DISCIPLE;
          default: return from;
        }
	  }

      internal static bool IsFemale(this GameActors.IDs x)
      {
        switch(x) {
          case GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED:
          case GameActors.IDs.UNDEAD_FEMALE_NEOPHYTE:
          case GameActors.IDs.UNDEAD_FEMALE_DISCIPLE:
          case GameActors.IDs.FEMALE_CIVILIAN:
          case GameActors.IDs.POLICEWOMAN:
            return true;
          default: return false;
        }
      }
	}
}
