// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.GameActors
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Gameplay.AI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Gameplay
{
  internal class GameActors : ActorModelDB
  {
    private static readonly Verb VERB_PUNCH = new Verb("punch", "punches");
    private readonly ActorModel[] m_Models = new ActorModel[(int) IDs._COUNT];
    private const int UNDEAD_FOOD = 2*Actor.ROT_HUNGRY_LEVEL;
    private const int HUMAN_HUN = 2*Actor.FOOD_HUNGRY_LEVEL;
    private const int HUMAN_SLP = 2*Actor.SLEEP_SLEEPY_LEVEL;
    private const int HUMAN_SAN = 4*WorldTime.TURNS_PER_DAY;
    private const int HUMAN_STA = 2*WorldTime.TURNS_PER_HOUR;
    private const int HUMAN_INVENTORY = 7;
    private const int DOG_HUN = 2*Actor.FOOD_HUNGRY_LEVEL;
    private const int DOG_SLP = 2*Actor.SLEEP_SLEEPY_LEVEL;
    private const int DOG_STA = 3*WorldTime.TURNS_PER_HOUR;
    private const int DOG_INVENTORY = 1;
    private const int NO_INVENTORY = 0;
    private const int NO_FOOD = 0;
    private const int NO_SLEEP = 0;
    private const int NO_SANITY = 0;
    private const int NO_SMELL = 0;
    private const int NO_AUDIO = 0;
    private const int NO_STA = 99;  // XXX arguably should be Actor.STAMINA_INFINITE

    public ActorModel this[int id] {
      get {
        Contract.Ensures(null!=Contract.Result<ActorModel>().DollBody);
        Contract.Ensures(null!=Contract.Result<ActorModel>().StartingSheet);
        return m_Models[id];
      }
    }

    public ActorModel this[GameActors.IDs id] {
      get {
        return this[(int) id];
      }
      private set {
        m_Models[(int) id] = value;
        m_Models[(int) id].ID = id;
      }
    }

    public ActorModel Skeleton
    {
      get
      {
        return this[GameActors.IDs._FIRST];
      }
    }

    public ActorModel Red_Eyed_Skeleton
    {
      get
      {
        return this[GameActors.IDs.UNDEAD_RED_EYED_SKELETON];
      }
    }

    public ActorModel Red_Skeleton
    {
      get
      {
        return this[GameActors.IDs.UNDEAD_RED_SKELETON];
      }
    }

    public ActorModel Zombie
    {
      get
      {
        return this[GameActors.IDs.UNDEAD_ZOMBIE];
      }
    }

    public ActorModel DarkEyedZombie
    {
      get
      {
        return this[GameActors.IDs.UNDEAD_DARK_EYED_ZOMBIE];
      }
    }

    public ActorModel DarkZombie
    {
      get
      {
        return this[GameActors.IDs.UNDEAD_DARK_ZOMBIE];
      }
    }

    public ActorModel ZombieMaster
    {
      get
      {
        return this[GameActors.IDs.UNDEAD_ZOMBIE_MASTER];
      }
    }

    public ActorModel ZombieLord
    {
      get
      {
        return this[GameActors.IDs.UNDEAD_ZOMBIE_LORD];
      }
    }

    public ActorModel ZombiePrince
    {
      get
      {
        return this[GameActors.IDs.UNDEAD_ZOMBIE_PRINCE];
      }
    }

    public ActorModel MaleZombified
    {
      get
      {
        return this[GameActors.IDs.UNDEAD_MALE_ZOMBIFIED];
      }
    }

    public ActorModel FemaleZombified
    {
      get
      {
        return this[GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED];
      }
    }

    public ActorModel MaleNeophyte
    {
      get
      {
        return this[GameActors.IDs.UNDEAD_MALE_NEOPHYTE];
      }
    }

    public ActorModel FemaleNeophyte
    {
      get
      {
        return this[GameActors.IDs.UNDEAD_FEMALE_NEOPHYTE];
      }
    }

    public ActorModel MaleDisciple
    {
      get
      {
        return this[GameActors.IDs.UNDEAD_MALE_DISCIPLE];
      }
    }

    public ActorModel FemaleDisciple
    {
      get
      {
        return this[GameActors.IDs.UNDEAD_FEMALE_DISCIPLE];
      }
    }

    public ActorModel RatZombie
    {
      get
      {
        return this[GameActors.IDs.UNDEAD_RAT_ZOMBIE];
      }
    }

    public ActorModel SewersThing
    {
      get
      {
        return this[GameActors.IDs.SEWERS_THING];
      }
    }

    public ActorModel MaleCivilian
    {
      get
      {
        return this[GameActors.IDs.MALE_CIVILIAN];
      }
    }

    public ActorModel FemaleCivilian
    {
      get
      {
        return this[GameActors.IDs.FEMALE_CIVILIAN];
      }
    }

    public ActorModel FeralDog
    {
      get
      {
        return this[GameActors.IDs.FERAL_DOG];
      }
    }

    public ActorModel CHARGuard
    {
      get
      {
        return this[GameActors.IDs.CHAR_GUARD];
      }
    }

    public ActorModel NationalGuard
    {
      get
      {
        return this[GameActors.IDs.ARMY_NATIONAL_GUARD];
      }
    }

    public ActorModel BikerMan
    {
      get
      {
        return this[GameActors.IDs.BIKER_MAN];
      }
    }

    public ActorModel GangstaMan
    {
      get
      {
        return this[GameActors.IDs.GANGSTA_MAN];
      }
    }

    public ActorModel Policeman
    {
      get
      {
        return this[GameActors.IDs.POLICEMAN];
      }
    }

    public ActorModel BlackOps
    {
      get
      {
        return this[GameActors.IDs.BLACKOPS_MAN];
      }
    }

    public ActorModel JasonMyers
    {
      get
      {
        return this[GameActors.IDs.JASON_MYERS];
      }
    }

    public GameActors()
    {
      Models.Actors = this;
    }

/*
to transform from MALE_CIVILIAN to POLICEMAN:
* man -> policeman
* men -> policemen
* flavor text becomes To protect and to die
* add  | Abilities.Flags.IS_LAW_ENFORCER
*/

    public void CreateModels(CSVTable toTable)
    {
      const Abilities.Flags STD_ZOMBIFYING = Abilities.Flags.IS_ROTTING | Abilities.Flags.CAN_ZOMBIFY_KILLED | Abilities.Flags.CAN_BASH_DOORS | Abilities.Flags.CAN_BREAK_OBJECTS | Abilities.Flags.ZOMBIEAI_EXPLORE | Abilities.Flags.AI_CAN_USE_AI_EXITS;
      const Abilities.Flags STD_ZM = Abilities.Flags.CAN_USE_MAP_OBJECTS | Abilities.Flags.CAN_JUMP | Abilities.Flags.CAN_JUMP_STUMBLE;
      const Abilities.Flags STD_LIVING = Abilities.Flags.HAS_INVENTORY | Abilities.Flags.CAN_BREAK_OBJECTS | Abilities.Flags.CAN_JUMP | Abilities.Flags.CAN_TIRE | Abilities.Flags.CAN_RUN;
      const Abilities.Flags STD_HUMAN = Abilities.Flags.CAN_USE_MAP_OBJECTS | Abilities.Flags.CAN_USE_ITEMS | Abilities.Flags.CAN_TALK | Abilities.Flags.CAN_PUSH | Abilities.Flags.CAN_BARRICADE;
      const Abilities.Flags STD_SANE = Abilities.Flags.HAS_SANITY | Abilities.Flags.HAS_TO_SLEEP | Abilities.Flags.IS_INTELLIGENT;

      Func<CSVLine, GameActors.ActorData> parse_fn = new Func<CSVLine, GameActors.ActorData>(GameActors.ActorData.FromCSVLine);
      ActorData DATA_SKELETON = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn, GameActors.IDs._FIRST);
      ActorData DATA_RED_EYED_SKELETON = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.UNDEAD_RED_EYED_SKELETON);
      ActorData DATA_RED_SKELETON = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.UNDEAD_RED_SKELETON);
      ActorData DATA_ZOMBIE = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.UNDEAD_ZOMBIE);
      ActorData DATA_DARK_EYED_ZOMBIE = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.UNDEAD_DARK_EYED_ZOMBIE);
      ActorData DATA_DARK_ZOMBIE = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.UNDEAD_DARK_ZOMBIE);
      ActorData DATA_MALE_ZOMBIFIED = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.UNDEAD_MALE_ZOMBIFIED);
      ActorData DATA_FEMALE_ZOMBIFIED = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED);
      ActorData DATA_MALE_NEOPHYTE = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.UNDEAD_MALE_NEOPHYTE);
      ActorData DATA_FEMALE_NEOPHYTE = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.UNDEAD_FEMALE_NEOPHYTE);
      ActorData DATA_MALE_DISCIPLE = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.UNDEAD_MALE_DISCIPLE);
      ActorData DATA_FEMALE_DISCIPLE = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.UNDEAD_FEMALE_DISCIPLE);
      ActorData DATA_ZM = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.UNDEAD_ZOMBIE_MASTER);
      ActorData DATA_ZL = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.UNDEAD_ZOMBIE_LORD);
      ActorData DATA_ZP = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.UNDEAD_ZOMBIE_PRINCE);
      ActorData DATA_RAT_ZOMBIE = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.UNDEAD_RAT_ZOMBIE);
      ActorData DATA_SEWERS_THING = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.SEWERS_THING);
      ActorData DATA_MALE_CIVILIAN = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.MALE_CIVILIAN);
      ActorData DATA_FEMALE_CIVILIAN = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.FEMALE_CIVILIAN);
      ActorData DATA_FERAL_DOG = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.FERAL_DOG);
      ActorData DATA_POLICEMAN = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.POLICEMAN);
      ActorData DATA_CHAR_GUARD = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.CHAR_GUARD);
      ActorData DATA_NATGUARD = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.ARMY_NATIONAL_GUARD);
      ActorData DATA_BIKER_MAN = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.BIKER_MAN);
      ActorData DATA_GANGSTA_MAN = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.GANGSTA_MAN);
      ActorData DATA_BLACKOPS_MAN = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.BLACKOPS_MAN);
      ActorData DATA_JASON_MYERS = toTable.GetDataFor<GameActors.ActorData, GameActors.IDs>(parse_fn,  GameActors.IDs.JASON_MYERS);

      // XXX postprocessing stage should go here

      this[GameActors.IDs._FIRST] = new ActorModel(GameImages.ACTOR_SKELETON, DATA_SKELETON.NAME, DATA_SKELETON.PLURAL, DATA_SKELETON.SCORE, DATA_SKELETON.FLAVOR, new DollBody(true, DATA_SKELETON.SPD), new Abilities(
          Abilities.Flags.UNDEAD),
          new ActorSheet(DATA_SKELETON.HP, NO_STA, NO_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("claw"), DATA_SKELETON.ATK, DATA_SKELETON.DMG), new Defence(DATA_SKELETON.DEF, DATA_SKELETON.PRO_HIT, DATA_SKELETON.PRO_SHOT), DATA_SKELETON.FOV, NO_AUDIO, NO_SMELL, NO_INVENTORY), typeof (SkeletonAI));
      this[GameActors.IDs.UNDEAD_RED_EYED_SKELETON] = new ActorModel(GameImages.ACTOR_RED_EYED_SKELETON, DATA_RED_EYED_SKELETON.NAME, DATA_RED_EYED_SKELETON.PLURAL, DATA_RED_EYED_SKELETON.SCORE, DATA_RED_EYED_SKELETON.FLAVOR, new DollBody(true, DATA_RED_EYED_SKELETON.SPD), new Abilities(
          Abilities.Flags.UNDEAD),
          new ActorSheet(DATA_RED_EYED_SKELETON.HP, NO_STA, NO_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("claw"), DATA_RED_EYED_SKELETON.ATK, DATA_RED_EYED_SKELETON.DMG), new Defence(DATA_RED_EYED_SKELETON.DEF, DATA_RED_EYED_SKELETON.PRO_HIT, DATA_RED_EYED_SKELETON.PRO_SHOT), DATA_RED_EYED_SKELETON.FOV, NO_AUDIO, NO_SMELL, NO_INVENTORY), typeof (SkeletonAI));
      this[GameActors.IDs.UNDEAD_RED_SKELETON] = new ActorModel(GameImages.ACTOR_RED_SKELETON, DATA_RED_SKELETON.NAME, DATA_RED_SKELETON.PLURAL, DATA_RED_SKELETON.SCORE, DATA_RED_SKELETON.FLAVOR, new DollBody(true, DATA_RED_SKELETON.SPD), new Abilities(
          Abilities.Flags.UNDEAD),
          new ActorSheet(DATA_RED_SKELETON.HP, NO_STA, NO_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("claw"), DATA_RED_SKELETON.ATK, DATA_RED_SKELETON.DMG), new Defence(DATA_RED_SKELETON.DEF, DATA_RED_SKELETON.PRO_HIT, DATA_RED_SKELETON.PRO_SHOT), DATA_RED_SKELETON.FOV, NO_AUDIO, NO_SMELL, NO_INVENTORY), typeof (SkeletonAI));
      this[GameActors.IDs.UNDEAD_ZOMBIE] = new ActorModel(GameImages.ACTOR_ZOMBIE, DATA_ZOMBIE.NAME, DATA_ZOMBIE.PLURAL, DATA_ZOMBIE.SCORE, DATA_ZOMBIE.FLAVOR, new DollBody(true, DATA_ZOMBIE.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING),
          new ActorSheet(DATA_ZOMBIE.HP, NO_STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_ZOMBIE.ATK, DATA_ZOMBIE.DMG), new Defence(DATA_ZOMBIE.DEF, DATA_ZOMBIE.PRO_HIT, DATA_ZOMBIE.PRO_SHOT), DATA_ZOMBIE.FOV, NO_AUDIO, DATA_ZOMBIE.SMELL, NO_INVENTORY), typeof (ZombieAI));
      this[GameActors.IDs.UNDEAD_DARK_EYED_ZOMBIE] = new ActorModel(GameImages.ACTOR_DARK_EYED_ZOMBIE, DATA_DARK_EYED_ZOMBIE.NAME, DATA_DARK_EYED_ZOMBIE.PLURAL, DATA_DARK_EYED_ZOMBIE.SCORE, DATA_DARK_EYED_ZOMBIE.FLAVOR, new DollBody(true, DATA_DARK_EYED_ZOMBIE.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING),
          new ActorSheet(DATA_DARK_EYED_ZOMBIE.HP, NO_STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_DARK_EYED_ZOMBIE.ATK, DATA_DARK_EYED_ZOMBIE.DMG), new Defence(DATA_DARK_EYED_ZOMBIE.DEF, DATA_DARK_EYED_ZOMBIE.PRO_HIT, DATA_DARK_EYED_ZOMBIE.PRO_SHOT), DATA_DARK_EYED_ZOMBIE.FOV, NO_AUDIO, DATA_DARK_EYED_ZOMBIE.SMELL, NO_INVENTORY), typeof (ZombieAI));
      this[GameActors.IDs.UNDEAD_DARK_ZOMBIE] = new ActorModel(GameImages.ACTOR_DARK_ZOMBIE, DATA_DARK_ZOMBIE.NAME, DATA_DARK_ZOMBIE.PLURAL, DATA_DARK_ZOMBIE.SCORE, DATA_DARK_ZOMBIE.FLAVOR, new DollBody(true, DATA_DARK_ZOMBIE.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING),
          new ActorSheet(DATA_DARK_ZOMBIE.HP, NO_STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_DARK_ZOMBIE.ATK, DATA_DARK_ZOMBIE.DMG), new Defence(DATA_DARK_ZOMBIE.DEF, DATA_DARK_ZOMBIE.PRO_HIT, DATA_DARK_ZOMBIE.PRO_SHOT), DATA_DARK_ZOMBIE.FOV, NO_AUDIO, DATA_DARK_ZOMBIE.SMELL, NO_INVENTORY), typeof (ZombieAI));
      this[GameActors.IDs.UNDEAD_MALE_ZOMBIFIED] = new ActorModel(null, DATA_MALE_ZOMBIFIED.NAME, DATA_MALE_ZOMBIFIED.PLURAL, DATA_MALE_ZOMBIFIED.SCORE, DATA_MALE_ZOMBIFIED.FLAVOR, new DollBody(true, DATA_MALE_ZOMBIFIED.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING),
          new ActorSheet(DATA_MALE_ZOMBIFIED.HP, NO_STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_MALE_ZOMBIFIED.ATK, DATA_MALE_ZOMBIFIED.DMG), new Defence(DATA_MALE_ZOMBIFIED.DEF, DATA_MALE_ZOMBIFIED.PRO_HIT, DATA_MALE_ZOMBIFIED.PRO_SHOT), DATA_MALE_ZOMBIFIED.FOV, NO_AUDIO, DATA_MALE_ZOMBIFIED.SMELL, NO_INVENTORY), typeof (ZombieAI));
      this[GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED] = new ActorModel(null, DATA_FEMALE_ZOMBIFIED.NAME, DATA_FEMALE_ZOMBIFIED.PLURAL, DATA_FEMALE_ZOMBIFIED.SCORE, DATA_FEMALE_ZOMBIFIED.FLAVOR, new DollBody(true, DATA_FEMALE_ZOMBIFIED.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING),
          new ActorSheet(DATA_FEMALE_ZOMBIFIED.HP, NO_STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_FEMALE_ZOMBIFIED.ATK, DATA_FEMALE_ZOMBIFIED.DMG), new Defence(DATA_FEMALE_ZOMBIFIED.DEF, DATA_FEMALE_ZOMBIFIED.PRO_HIT, DATA_FEMALE_ZOMBIFIED.PRO_SHOT), DATA_FEMALE_ZOMBIFIED.FOV, NO_AUDIO, DATA_FEMALE_ZOMBIFIED.SMELL, NO_INVENTORY), typeof (ZombieAI));
      this[GameActors.IDs.UNDEAD_MALE_NEOPHYTE] = new ActorModel(GameImages.ACTOR_MALE_NEOPHYTE, DATA_MALE_NEOPHYTE.NAME, DATA_MALE_NEOPHYTE.PLURAL, DATA_MALE_NEOPHYTE.SCORE, DATA_MALE_NEOPHYTE.FLAVOR, new DollBody(true, DATA_MALE_NEOPHYTE.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING | Abilities.Flags.CAN_PUSH),
          new ActorSheet(DATA_MALE_NEOPHYTE.HP, NO_STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_MALE_NEOPHYTE.ATK, DATA_MALE_NEOPHYTE.DMG), new Defence(DATA_MALE_NEOPHYTE.DEF, DATA_MALE_NEOPHYTE.PRO_HIT, DATA_MALE_NEOPHYTE.PRO_SHOT), DATA_MALE_NEOPHYTE.FOV, NO_AUDIO, DATA_MALE_NEOPHYTE.SMELL, NO_INVENTORY), typeof (ZombieAI));
      this[GameActors.IDs.UNDEAD_FEMALE_NEOPHYTE] = new ActorModel(GameImages.ACTOR_FEMALE_NEOPHYTE, DATA_FEMALE_NEOPHYTE.NAME, DATA_FEMALE_NEOPHYTE.PLURAL, DATA_FEMALE_NEOPHYTE.SCORE, DATA_FEMALE_NEOPHYTE.FLAVOR, new DollBody(true, DATA_FEMALE_NEOPHYTE.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING | Abilities.Flags.CAN_PUSH),
          new ActorSheet(DATA_FEMALE_NEOPHYTE.HP, NO_STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_FEMALE_NEOPHYTE.ATK, DATA_FEMALE_NEOPHYTE.DMG), new Defence(DATA_FEMALE_NEOPHYTE.DEF, DATA_FEMALE_NEOPHYTE.PRO_HIT, DATA_FEMALE_NEOPHYTE.PRO_SHOT), DATA_FEMALE_NEOPHYTE.FOV, NO_AUDIO, DATA_FEMALE_NEOPHYTE.SMELL, NO_INVENTORY), typeof (ZombieAI));
      this[GameActors.IDs.UNDEAD_MALE_DISCIPLE] = new ActorModel(GameImages.ACTOR_MALE_DISCIPLE, DATA_MALE_DISCIPLE.NAME, DATA_MALE_DISCIPLE.PLURAL, DATA_MALE_DISCIPLE.SCORE, DATA_MALE_DISCIPLE.FLAVOR, new DollBody(true, DATA_MALE_DISCIPLE.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING | Abilities.Flags.CAN_PUSH),
          new ActorSheet(DATA_MALE_DISCIPLE.HP, NO_STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_MALE_DISCIPLE.ATK, DATA_MALE_DISCIPLE.DMG), new Defence(DATA_MALE_DISCIPLE.DEF, DATA_MALE_DISCIPLE.PRO_HIT, DATA_MALE_DISCIPLE.PRO_SHOT), DATA_MALE_DISCIPLE.FOV, NO_AUDIO, DATA_MALE_DISCIPLE.SMELL, NO_INVENTORY), typeof (ZombieAI));
      this[GameActors.IDs.UNDEAD_FEMALE_DISCIPLE] = new ActorModel(GameImages.ACTOR_FEMALE_DISCIPLE, DATA_FEMALE_DISCIPLE.NAME, DATA_FEMALE_DISCIPLE.PLURAL, DATA_FEMALE_DISCIPLE.SCORE, DATA_FEMALE_DISCIPLE.FLAVOR, new DollBody(true, DATA_FEMALE_DISCIPLE.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING | Abilities.Flags.CAN_PUSH),
          new ActorSheet(DATA_FEMALE_DISCIPLE.HP, NO_STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_FEMALE_DISCIPLE.ATK, DATA_FEMALE_DISCIPLE.DMG), new Defence(DATA_FEMALE_DISCIPLE.DEF, DATA_FEMALE_DISCIPLE.PRO_HIT, DATA_FEMALE_DISCIPLE.PRO_SHOT), DATA_FEMALE_DISCIPLE.FOV, NO_AUDIO, DATA_FEMALE_DISCIPLE.SMELL, NO_INVENTORY), typeof (ZombieAI));
      this[GameActors.IDs.UNDEAD_ZOMBIE_MASTER] = new ActorModel(GameImages.ACTOR_ZOMBIE_MASTER, DATA_ZM.NAME, DATA_ZM.PLURAL, DATA_ZM.SCORE, DATA_ZM.FLAVOR, new DollBody(true, DATA_ZM.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING | Abilities.Flags.CAN_PUSH |
          Abilities.Flags.UNDEAD_MASTER | STD_ZM),
          new ActorSheet(DATA_ZM.HP, NO_STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_ZM.ATK, DATA_ZM.DMG), new Defence(DATA_ZM.DEF, DATA_ZM.PRO_HIT, DATA_ZM.PRO_SHOT), DATA_ZM.FOV, NO_AUDIO, DATA_ZM.SMELL, NO_INVENTORY), typeof (ZombieAI));
      this[GameActors.IDs.UNDEAD_ZOMBIE_LORD] = new ActorModel(GameImages.ACTOR_ZOMBIE_LORD, DATA_ZL.NAME, DATA_ZL.PLURAL, DATA_ZL.SCORE, DATA_ZL.FLAVOR, new DollBody(true, DATA_ZL.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING | Abilities.Flags.CAN_PUSH |
          Abilities.Flags.UNDEAD_MASTER | STD_ZM),
          new ActorSheet(DATA_ZL.HP, NO_STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_ZL.ATK, DATA_ZL.DMG), new Defence(DATA_ZL.DEF, DATA_ZL.PRO_HIT, DATA_ZL.PRO_SHOT), DATA_ZL.FOV, NO_AUDIO, DATA_ZL.SMELL, NO_INVENTORY), typeof (ZombieAI));
      this[GameActors.IDs.UNDEAD_ZOMBIE_PRINCE] = new ActorModel(GameImages.ACTOR_ZOMBIE_PRINCE, DATA_ZP.NAME, DATA_ZP.PLURAL, DATA_ZP.SCORE, DATA_ZP.FLAVOR, new DollBody(true, DATA_ZP.SPD), new Abilities(
          Abilities.Flags.UNDEAD | STD_ZOMBIFYING | Abilities.Flags.CAN_PUSH |
          Abilities.Flags.UNDEAD_MASTER | STD_ZM),
          new ActorSheet(DATA_ZP.HP, NO_STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_ZP.ATK, DATA_ZP.DMG), new Defence(DATA_ZP.DEF, DATA_ZP.PRO_HIT, DATA_ZP.PRO_SHOT), DATA_ZP.FOV, NO_AUDIO, DATA_ZP.SMELL, NO_INVENTORY), typeof (ZombieAI));
      this[GameActors.IDs.UNDEAD_RAT_ZOMBIE] = new ActorModel(GameImages.ACTOR_RAT_ZOMBIE, DATA_RAT_ZOMBIE.NAME, DATA_RAT_ZOMBIE.PLURAL, DATA_RAT_ZOMBIE.SCORE, DATA_RAT_ZOMBIE.FLAVOR, new DollBody(true, DATA_RAT_ZOMBIE.SPD), new Abilities(
        Abilities.Flags.UNDEAD | Abilities.Flags.IS_SMALL | Abilities.Flags.AI_CAN_USE_AI_EXITS),
        new ActorSheet(DATA_RAT_ZOMBIE.HP, NO_STA, NO_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_RAT_ZOMBIE.ATK, DATA_RAT_ZOMBIE.DMG), new Defence(DATA_RAT_ZOMBIE.DEF, DATA_RAT_ZOMBIE.PRO_HIT, DATA_RAT_ZOMBIE.PRO_SHOT), DATA_RAT_ZOMBIE.FOV, NO_AUDIO, DATA_RAT_ZOMBIE.SMELL, NO_INVENTORY), typeof (RatAI));
      this[GameActors.IDs.SEWERS_THING] = new ActorModel(GameImages.ACTOR_SEWERS_THING, DATA_SEWERS_THING.NAME, DATA_SEWERS_THING.PLURAL, DATA_SEWERS_THING.SCORE, DATA_SEWERS_THING.FLAVOR, new DollBody(true, DATA_SEWERS_THING.SPD), new Abilities(
        Abilities.Flags.UNDEAD | Abilities.Flags.CAN_BASH_DOORS | Abilities.Flags.CAN_BREAK_OBJECTS),
        new ActorSheet(DATA_SEWERS_THING.HP, NO_STA, NO_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_SEWERS_THING.ATK, DATA_SEWERS_THING.DMG), new Defence(DATA_SEWERS_THING.DEF, DATA_SEWERS_THING.PRO_HIT, DATA_SEWERS_THING.PRO_SHOT), DATA_SEWERS_THING.FOV, NO_AUDIO, DATA_SEWERS_THING.SMELL, NO_INVENTORY), typeof (SewersThingAI));
      this[GameActors.IDs.MALE_CIVILIAN] = new ActorModel(null, DATA_MALE_CIVILIAN.NAME, DATA_MALE_CIVILIAN.PLURAL, DATA_MALE_CIVILIAN.SCORE, DATA_MALE_CIVILIAN.FLAVOR, new DollBody(true, DATA_MALE_CIVILIAN.SPD), new Abilities(
          STD_LIVING | STD_HUMAN | STD_SANE |
          Abilities.Flags.CAN_TRADE | Abilities.Flags.HAS_TO_EAT | Abilities.Flags.AI_CAN_USE_AI_EXITS),
          new ActorSheet(DATA_MALE_CIVILIAN.HP, HUMAN_STA, HUMAN_HUN, HUMAN_SLP, HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, DATA_MALE_CIVILIAN.ATK, DATA_MALE_CIVILIAN.DMG), new Defence(DATA_MALE_CIVILIAN.DEF, DATA_MALE_CIVILIAN.PRO_HIT, DATA_MALE_CIVILIAN.PRO_SHOT), DATA_MALE_CIVILIAN.FOV, DATA_MALE_CIVILIAN.AUDIO, NO_SMELL, HUMAN_INVENTORY), typeof (CivilianAI));
      this[GameActors.IDs.FEMALE_CIVILIAN] = new ActorModel(null, DATA_FEMALE_CIVILIAN.NAME, DATA_FEMALE_CIVILIAN.PLURAL, DATA_FEMALE_CIVILIAN.SCORE, DATA_FEMALE_CIVILIAN.FLAVOR, new DollBody(false, DATA_FEMALE_CIVILIAN.SPD), new Abilities(
          STD_LIVING | STD_HUMAN | STD_SANE |
          Abilities.Flags.CAN_TRADE | Abilities.Flags.HAS_TO_EAT | Abilities.Flags.AI_CAN_USE_AI_EXITS), 
          new ActorSheet(DATA_FEMALE_CIVILIAN.HP, HUMAN_STA, HUMAN_HUN, HUMAN_SLP, HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, DATA_FEMALE_CIVILIAN.ATK, DATA_FEMALE_CIVILIAN.DMG), new Defence(DATA_FEMALE_CIVILIAN.DEF, DATA_FEMALE_CIVILIAN.PRO_HIT, DATA_FEMALE_CIVILIAN.PRO_SHOT), DATA_FEMALE_CIVILIAN.FOV, DATA_FEMALE_CIVILIAN.AUDIO, NO_SMELL, HUMAN_INVENTORY), typeof (CivilianAI));
      this[GameActors.IDs.CHAR_GUARD] = new ActorModel(null, DATA_CHAR_GUARD.NAME, DATA_CHAR_GUARD.PLURAL, DATA_CHAR_GUARD.SCORE, DATA_CHAR_GUARD.FLAVOR, new DollBody(true, DATA_CHAR_GUARD.SPD), new Abilities(
          STD_LIVING | STD_HUMAN | STD_SANE),
          new ActorSheet(DATA_CHAR_GUARD.HP, HUMAN_STA, NO_FOOD, HUMAN_SLP, HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, DATA_CHAR_GUARD.ATK, DATA_CHAR_GUARD.DMG), new Defence(DATA_CHAR_GUARD.DEF, DATA_CHAR_GUARD.PRO_HIT, DATA_CHAR_GUARD.PRO_SHOT), DATA_CHAR_GUARD.FOV, DATA_CHAR_GUARD.AUDIO, NO_SMELL, HUMAN_INVENTORY), typeof (CHARGuardAI));
      this[GameActors.IDs.ARMY_NATIONAL_GUARD] = new ActorModel(null, DATA_NATGUARD.NAME, DATA_NATGUARD.PLURAL, DATA_NATGUARD.SCORE, DATA_NATGUARD.FLAVOR, new DollBody(true, DATA_NATGUARD.SPD), new Abilities(
          STD_LIVING | STD_HUMAN | STD_SANE),
          new ActorSheet(DATA_NATGUARD.HP, HUMAN_STA, NO_FOOD, HUMAN_SLP, HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, DATA_NATGUARD.ATK, DATA_NATGUARD.DMG), new Defence(DATA_NATGUARD.DEF, DATA_NATGUARD.PRO_HIT, DATA_NATGUARD.PRO_SHOT), DATA_NATGUARD.FOV, DATA_NATGUARD.AUDIO, NO_SMELL, HUMAN_INVENTORY), typeof (SoldierAI));
      this[GameActors.IDs.BIKER_MAN] = new ActorModel(null, DATA_BIKER_MAN.NAME, DATA_BIKER_MAN.PLURAL, DATA_BIKER_MAN.SCORE, DATA_BIKER_MAN.FLAVOR, new DollBody(true, DATA_BIKER_MAN.SPD), new Abilities(
          STD_LIVING | STD_HUMAN | STD_SANE |
          Abilities.Flags.CAN_TRADE | Abilities.Flags.HAS_TO_EAT | Abilities.Flags.AI_NOT_INTERESTED_IN_RANGED_WEAPONS),
          new ActorSheet(DATA_BIKER_MAN.HP, HUMAN_STA, HUMAN_HUN, HUMAN_SLP, HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, DATA_BIKER_MAN.ATK, DATA_BIKER_MAN.DMG), new Defence(DATA_BIKER_MAN.DEF, DATA_BIKER_MAN.PRO_HIT, DATA_BIKER_MAN.PRO_SHOT), DATA_BIKER_MAN.FOV, DATA_BIKER_MAN.AUDIO, NO_SMELL, HUMAN_INVENTORY), typeof (GangAI));
      this[GameActors.IDs.GANGSTA_MAN] = new ActorModel(null, DATA_GANGSTA_MAN.NAME, DATA_GANGSTA_MAN.PLURAL, DATA_GANGSTA_MAN.SCORE, DATA_GANGSTA_MAN.FLAVOR, new DollBody(true, DATA_GANGSTA_MAN.SPD), new Abilities(
          STD_LIVING | STD_HUMAN | STD_SANE |
          Abilities.Flags.CAN_TRADE | Abilities.Flags.HAS_TO_EAT),
          new ActorSheet(DATA_GANGSTA_MAN.HP, HUMAN_STA, HUMAN_HUN, HUMAN_SLP, HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, DATA_GANGSTA_MAN.ATK, DATA_GANGSTA_MAN.DMG), new Defence(DATA_GANGSTA_MAN.DEF, DATA_GANGSTA_MAN.PRO_HIT, DATA_GANGSTA_MAN.PRO_SHOT), DATA_GANGSTA_MAN.FOV, DATA_GANGSTA_MAN.AUDIO, NO_SMELL, HUMAN_INVENTORY), typeof (GangAI));
      this[GameActors.IDs.POLICEMAN] = new ActorModel(null, DATA_POLICEMAN.NAME, DATA_POLICEMAN.PLURAL, DATA_POLICEMAN.SCORE, DATA_POLICEMAN.FLAVOR, new DollBody(true, DATA_POLICEMAN.SPD), new Abilities(
          STD_LIVING | STD_HUMAN | STD_SANE |
          Abilities.Flags.CAN_TRADE | Abilities.Flags.HAS_TO_EAT | Abilities.Flags.AI_CAN_USE_AI_EXITS | Abilities.Flags.IS_LAW_ENFORCER),
          new ActorSheet(DATA_POLICEMAN.HP, HUMAN_STA, HUMAN_HUN, HUMAN_SLP, HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, DATA_POLICEMAN.ATK, DATA_POLICEMAN.DMG), new Defence(DATA_POLICEMAN.DEF, DATA_POLICEMAN.PRO_HIT, DATA_POLICEMAN.PRO_SHOT), DATA_POLICEMAN.FOV, DATA_POLICEMAN.AUDIO, NO_SMELL, HUMAN_INVENTORY), typeof (CivilianAI));
      this[GameActors.IDs.BLACKOPS_MAN] = new ActorModel(null, DATA_BLACKOPS_MAN.NAME, DATA_BLACKOPS_MAN.PLURAL, DATA_BLACKOPS_MAN.SCORE, DATA_BLACKOPS_MAN.FLAVOR, new DollBody(true, DATA_BLACKOPS_MAN.SPD), new Abilities(
          STD_LIVING | STD_HUMAN | STD_SANE),
          new ActorSheet(DATA_BLACKOPS_MAN.HP, HUMAN_STA, NO_FOOD, HUMAN_SLP, HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, DATA_BLACKOPS_MAN.ATK, DATA_BLACKOPS_MAN.DMG), new Defence(DATA_BLACKOPS_MAN.DEF, DATA_BLACKOPS_MAN.PRO_HIT, DATA_BLACKOPS_MAN.PRO_SHOT), DATA_BLACKOPS_MAN.FOV, DATA_BLACKOPS_MAN.AUDIO, NO_SMELL, HUMAN_INVENTORY), typeof (SoldierAI));
      this[GameActors.IDs.FERAL_DOG] = new ActorModel(null, DATA_FERAL_DOG.NAME, DATA_FERAL_DOG.PLURAL, DATA_FERAL_DOG.SCORE, DATA_FERAL_DOG.FLAVOR, new DollBody(true, DATA_FERAL_DOG.SPD), new Abilities(
          STD_LIVING |
          Abilities.Flags.HAS_TO_EAT | Abilities.Flags.HAS_TO_SLEEP | Abilities.Flags.AI_CAN_USE_AI_EXITS),
          new ActorSheet(DATA_FERAL_DOG.HP, DOG_STA, DOG_HUN, DOG_SLP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_FERAL_DOG.ATK, DATA_FERAL_DOG.DMG), new Defence(DATA_FERAL_DOG.DEF, DATA_FERAL_DOG.PRO_HIT, DATA_FERAL_DOG.PRO_SHOT), DATA_FERAL_DOG.FOV, DATA_FERAL_DOG.AUDIO, DATA_FERAL_DOG.SMELL, DOG_INVENTORY), typeof (FeralDogAI));
      this[GameActors.IDs.JASON_MYERS] = new ActorModel(null, DATA_JASON_MYERS.NAME, DATA_JASON_MYERS.PLURAL, DATA_JASON_MYERS.SCORE, DATA_JASON_MYERS.FLAVOR, new DollBody(true, DATA_JASON_MYERS.SPD), new Abilities(
          STD_LIVING | STD_HUMAN |
          Abilities.Flags.AI_CAN_USE_AI_EXITS),
          new ActorSheet(DATA_JASON_MYERS.HP, HUMAN_STA, NO_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, DATA_JASON_MYERS.ATK, DATA_JASON_MYERS.DMG), new Defence(DATA_JASON_MYERS.DEF, DATA_JASON_MYERS.PRO_HIT, DATA_JASON_MYERS.PRO_SHOT), DATA_JASON_MYERS.FOV, DATA_JASON_MYERS.AUDIO, NO_SMELL, HUMAN_INVENTORY), typeof (InsaneHumanAI));
      this[GameActors.IDs.POLICEWOMAN] = new ActorModel(null, "policewoman", "policewomen", DATA_POLICEMAN.SCORE, DATA_POLICEMAN.FLAVOR, new DollBody(true, DATA_POLICEMAN.SPD), new Abilities(
          STD_LIVING | STD_HUMAN | STD_SANE |
          Abilities.Flags.CAN_TRADE | Abilities.Flags.HAS_TO_EAT | Abilities.Flags.AI_CAN_USE_AI_EXITS | Abilities.Flags.IS_LAW_ENFORCER),
          new ActorSheet(DATA_FEMALE_CIVILIAN.HP, HUMAN_STA, HUMAN_HUN, HUMAN_SLP, HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, DATA_FEMALE_CIVILIAN.ATK, DATA_FEMALE_CIVILIAN.DMG), new Defence(DATA_POLICEMAN.DEF, DATA_POLICEMAN.PRO_HIT, DATA_POLICEMAN.PRO_SHOT), DATA_POLICEMAN.FOV, DATA_POLICEMAN.AUDIO, NO_SMELL, HUMAN_INVENTORY), typeof (CivilianAI));
    }

    public bool LoadFromCSV(IRogueUI ui, string path)
    {
      Contract.Requires(null!=ui);
      Contract.Requires(!string.IsNullOrEmpty(path));
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
      return true;
    }

    private void Notify(IRogueUI ui, string stage)
    {
      ui.UI_Clear(Color.Black);
      ui.UI_DrawStringBold(Color.White, "Loading actors data : " + stage, 0, 0, new Color?());
      ui.UI_Repaint();
    }

    public bool IsZombifiedBranch(ActorModel m)
    {
      if (m != MaleZombified && m != FemaleZombified && (m != MaleNeophyte && m != FemaleNeophyte) && m != MaleDisciple)
        return m == FemaleDisciple;
      return true;
    }

    public bool IsZMBranch(ActorModel m)
    {
      if (m != ZombieMaster && m != ZombieLord)
        return m == ZombiePrince;
      return true;
    }

    public bool IsSkeletonBranch(ActorModel m)
    {
      if (m != Skeleton && m != Red_Eyed_Skeleton)
        return m == Red_Skeleton;
      return true;
    }

    public bool IsShamblerBranch(ActorModel m)
    {
      if (m != Zombie && m != DarkEyedZombie)
        return m == DarkZombie;
      return true;
    }

    public bool IsRatBranch(ActorModel m)
    {
      return m == RatZombie;
    }

    public enum IDs
    {
      _FIRST = 0,
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
      public const int COUNT_FIELDS = 16;

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
      public static ActorData FromCSVLine(CSVLine line)
      {
        return new ActorData()
        {
          NAME = line[1].ParseText(),
          PLURAL = line[2].ParseText(),
          SPD = line[3].ParseInt(),
          HP = line[4].ParseInt(),
          STA = line[5].ParseInt(),
          ATK = line[6].ParseInt(),
          DMG = line[7].ParseInt(),
          DEF = line[8].ParseInt(),
          PRO_HIT = line[9].ParseInt(),
          PRO_SHOT = line[10].ParseInt(),
          FOV = line[11].ParseInt(),
          AUDIO = line[12].ParseInt(),
          SMELL = line[13].ParseInt(),
          SCORE = line[14].ParseInt(),
          FLAVOR = line[15].ParseText()
        };
      }
    }
  }

	internal static class ID_ext
	{
	  internal static GameActors.IDs NextUndeadEvolution(this GameActors.IDs from)
	  {
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
