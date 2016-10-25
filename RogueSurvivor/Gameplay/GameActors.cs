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
    private ActorModel[] m_Models = new ActorModel[27];
    private const int UNDEAD_FOOD = 2*Actor.ROT_HUNGRY_LEVEL;
    private const int HUMAN_HUN = 2*Actor.FOOD_HUNGRY_LEVEL;
    private const int HUMAN_SLP = 2*Actor.SLEEP_SLEEPY_LEVEL;
    private const int HUMAN_SAN = 4*WorldTime.TURNS_PER_DAY;
    private const int HUMAN_INVENTORY = 7;
    private const int DOG_HUN = 2*Actor.FOOD_HUNGRY_LEVEL;
    private const int DOG_SLP = 2*Actor.SLEEP_SLEEPY_LEVEL;
    private const int DOG_INVENTORY = 1;
    private const int NO_INVENTORY = 0;
    private const int NO_FOOD = 0;
    private const int NO_SLEEP = 0;
    private const int NO_SANITY = 0;
    private const int NO_SMELL = 0;
    private const int NO_AUDIO = 0;
    private GameActors.ActorData DATA_SKELETON;
    private GameActors.ActorData DATA_RED_EYED_SKELETON;
    private GameActors.ActorData DATA_RED_SKELETON;
    private GameActors.ActorData DATA_ZOMBIE;
    private GameActors.ActorData DATA_DARK_EYED_ZOMBIE;
    private GameActors.ActorData DATA_DARK_ZOMBIE;
    private GameActors.ActorData DATA_MALE_ZOMBIFIED;
    private GameActors.ActorData DATA_FEMALE_ZOMBIFIED;
    private GameActors.ActorData DATA_MALE_NEOPHYTE;
    private GameActors.ActorData DATA_FEMALE_NEOPHYTE;
    private GameActors.ActorData DATA_MALE_DISCIPLE;
    private GameActors.ActorData DATA_FEMALE_DISCIPLE;
    private GameActors.ActorData DATA_ZM;
    private GameActors.ActorData DATA_ZP;
    private GameActors.ActorData DATA_ZL;
    private GameActors.ActorData DATA_RAT_ZOMBIE;
    private GameActors.ActorData DATA_SEWERS_THING;
    private GameActors.ActorData DATA_MALE_CIVILIAN;
    private GameActors.ActorData DATA_FEMALE_CIVILIAN;
    private GameActors.ActorData DATA_FERAL_DOG;
    private GameActors.ActorData DATA_POLICEMAN;
    private GameActors.ActorData DATA_CHAR_GUARD;
    private GameActors.ActorData DATA_NATGUARD;
    private GameActors.ActorData DATA_BIKER_MAN;
    private GameActors.ActorData DATA_GANGSTA_MAN;
    private GameActors.ActorData DATA_BLACKOPS_MAN;
    private GameActors.ActorData DATA_JASON_MYERS;

    public override ActorModel this[int id]
    {
      get
      {
        Contract.Ensures(null!=Contract.Result<ActorModel>().DollBody);
        Contract.Ensures(null!=Contract.Result<ActorModel>().StartingSheet);
        return m_Models[id];
      }
    }

    public ActorModel this[GameActors.IDs id]
    {
      get
      {
        return this[(int) id];
      }
      private set
      {
        m_Models[(int) id] = value;
        m_Models[(int) id].ID = (int) id;
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
      Models.Actors = (ActorModelDB) this;
    }

    public void CreateModels()
    {
      this[GameActors.IDs._FIRST] = new ActorModel("Actors\\skeleton", DATA_SKELETON.NAME, DATA_SKELETON.PLURAL, DATA_SKELETON.SCORE, new DollBody(true, DATA_SKELETON.SPD), new Abilities()
      {
        IsUndead = true
      }, new ActorSheet(DATA_SKELETON.HP, DATA_SKELETON.STA, NO_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("claw"), DATA_SKELETON.ATK, DATA_SKELETON.DMG), new Defence(DATA_SKELETON.DEF, DATA_SKELETON.PRO_HIT, DATA_SKELETON.PRO_SHOT), DATA_SKELETON.FOV, NO_AUDIO, NO_SMELL, NO_INVENTORY), typeof (SkeletonAI))
      {
        FlavorDescription = DATA_SKELETON.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_RED_EYED_SKELETON] = new ActorModel("Actors\\red_eyed_skeleton", DATA_RED_EYED_SKELETON.NAME, DATA_RED_EYED_SKELETON.PLURAL, DATA_RED_EYED_SKELETON.SCORE, new DollBody(true, DATA_RED_EYED_SKELETON.SPD), new Abilities()
      {
        IsUndead = true
      }, new ActorSheet(DATA_RED_EYED_SKELETON.HP, DATA_RED_EYED_SKELETON.STA, NO_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("claw"), DATA_RED_EYED_SKELETON.ATK, DATA_RED_EYED_SKELETON.DMG), new Defence(DATA_RED_EYED_SKELETON.DEF, DATA_RED_EYED_SKELETON.PRO_HIT, DATA_RED_EYED_SKELETON.PRO_SHOT), DATA_RED_EYED_SKELETON.FOV, NO_AUDIO, NO_SMELL, NO_INVENTORY), typeof (SkeletonAI))
      {
        FlavorDescription = DATA_RED_EYED_SKELETON.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_RED_SKELETON] = new ActorModel("Actors\\red_skeleton", DATA_RED_SKELETON.NAME, DATA_RED_SKELETON.PLURAL, DATA_RED_SKELETON.SCORE, new DollBody(true, DATA_RED_SKELETON.SPD), new Abilities()
      {
        IsUndead = true
      }, new ActorSheet(DATA_RED_SKELETON.HP, DATA_RED_SKELETON.STA, NO_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("claw"), DATA_RED_SKELETON.ATK, DATA_RED_SKELETON.DMG), new Defence(DATA_RED_SKELETON.DEF, DATA_RED_SKELETON.PRO_HIT, DATA_RED_SKELETON.PRO_SHOT), DATA_RED_SKELETON.FOV, NO_AUDIO, NO_SMELL, NO_INVENTORY), typeof (SkeletonAI))
      {
        FlavorDescription = DATA_RED_SKELETON.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_ZOMBIE] = new ActorModel("Actors\\zombie", DATA_ZOMBIE.NAME, DATA_ZOMBIE.PLURAL, DATA_ZOMBIE.SCORE, new DollBody(true, DATA_ZOMBIE.SPD), new Abilities()
      {
        IsUndead = true,
        IsRotting = true,
        CanZombifyKilled = true,
        CanBashDoors = true,
        CanBreakObjects = true,
        ZombieAI_Explore = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(DATA_ZOMBIE.HP, DATA_ZOMBIE.STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_ZOMBIE.ATK, DATA_ZOMBIE.DMG), new Defence(DATA_ZOMBIE.DEF, DATA_ZOMBIE.PRO_HIT, DATA_ZOMBIE.PRO_SHOT), DATA_ZOMBIE.FOV, NO_AUDIO, DATA_ZOMBIE.SMELL, NO_INVENTORY), typeof (ZombieAI))
      {
        FlavorDescription = DATA_ZOMBIE.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_DARK_EYED_ZOMBIE] = new ActorModel("Actors\\dark_eyed_zombie", DATA_DARK_EYED_ZOMBIE.NAME, DATA_DARK_EYED_ZOMBIE.PLURAL, DATA_DARK_EYED_ZOMBIE.SCORE, new DollBody(true, DATA_DARK_EYED_ZOMBIE.SPD), new Abilities()
      {
        IsUndead = true,
        IsRotting = true,
        CanZombifyKilled = true,
        CanBashDoors = true,
        CanBreakObjects = true,
        ZombieAI_Explore = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(DATA_DARK_EYED_ZOMBIE.HP, DATA_DARK_EYED_ZOMBIE.STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_DARK_EYED_ZOMBIE.ATK, DATA_DARK_EYED_ZOMBIE.DMG), new Defence(DATA_DARK_EYED_ZOMBIE.DEF, DATA_DARK_EYED_ZOMBIE.PRO_HIT, DATA_DARK_EYED_ZOMBIE.PRO_SHOT), DATA_DARK_EYED_ZOMBIE.FOV, NO_AUDIO, DATA_DARK_EYED_ZOMBIE.SMELL, NO_INVENTORY), typeof (ZombieAI))
      {
        FlavorDescription = DATA_DARK_EYED_ZOMBIE.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_DARK_ZOMBIE] = new ActorModel("Actors\\dark_zombie", DATA_DARK_ZOMBIE.NAME, DATA_DARK_ZOMBIE.PLURAL, DATA_DARK_ZOMBIE.SCORE, new DollBody(true, DATA_DARK_ZOMBIE.SPD), new Abilities()
      {
        IsUndead = true,
        IsRotting = true,
        CanZombifyKilled = true,
        CanBashDoors = true,
        CanBreakObjects = true,
        ZombieAI_Explore = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(DATA_DARK_ZOMBIE.HP, DATA_DARK_ZOMBIE.STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_DARK_ZOMBIE.ATK, DATA_DARK_ZOMBIE.DMG), new Defence(DATA_DARK_ZOMBIE.DEF, DATA_DARK_ZOMBIE.PRO_HIT, DATA_DARK_ZOMBIE.PRO_SHOT), DATA_DARK_ZOMBIE.FOV, NO_AUDIO, DATA_DARK_ZOMBIE.SMELL, NO_INVENTORY), typeof (ZombieAI))
      {
        FlavorDescription = DATA_DARK_ZOMBIE.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_MALE_ZOMBIFIED] = new ActorModel((string) null, DATA_MALE_ZOMBIFIED.NAME, DATA_MALE_ZOMBIFIED.PLURAL, DATA_MALE_ZOMBIFIED.SCORE, new DollBody(true, DATA_MALE_ZOMBIFIED.SPD), new Abilities()
      {
        IsUndead = true,
        IsRotting = true,
        CanZombifyKilled = true,
        CanBashDoors = true,
        CanBreakObjects = true,
        ZombieAI_Explore = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(DATA_MALE_ZOMBIFIED.HP, DATA_MALE_ZOMBIFIED.STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_MALE_ZOMBIFIED.ATK, DATA_MALE_ZOMBIFIED.DMG), new Defence(DATA_MALE_ZOMBIFIED.DEF, DATA_MALE_ZOMBIFIED.PRO_HIT, DATA_MALE_ZOMBIFIED.PRO_SHOT), DATA_MALE_ZOMBIFIED.FOV, NO_AUDIO, DATA_MALE_ZOMBIFIED.SMELL, NO_INVENTORY), typeof (ZombieAI))
      {
        FlavorDescription = DATA_MALE_ZOMBIFIED.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED] = new ActorModel((string) null, DATA_FEMALE_ZOMBIFIED.NAME, DATA_FEMALE_ZOMBIFIED.PLURAL, DATA_FEMALE_ZOMBIFIED.SCORE, new DollBody(true, DATA_FEMALE_ZOMBIFIED.SPD), new Abilities()
      {
        IsUndead = true,
        IsRotting = true,
        CanZombifyKilled = true,
        CanBashDoors = true,
        CanBreakObjects = true,
        ZombieAI_Explore = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(DATA_FEMALE_ZOMBIFIED.HP, DATA_FEMALE_ZOMBIFIED.STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_FEMALE_ZOMBIFIED.ATK, DATA_FEMALE_ZOMBIFIED.DMG), new Defence(DATA_FEMALE_ZOMBIFIED.DEF, DATA_FEMALE_ZOMBIFIED.PRO_HIT, DATA_FEMALE_ZOMBIFIED.PRO_SHOT), DATA_FEMALE_ZOMBIFIED.FOV, NO_AUDIO, DATA_FEMALE_ZOMBIFIED.SMELL, NO_INVENTORY), typeof (ZombieAI))
      {
        FlavorDescription = DATA_FEMALE_ZOMBIFIED.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_MALE_NEOPHYTE] = new ActorModel("Actors\\male_neophyte", DATA_MALE_NEOPHYTE.NAME, DATA_MALE_NEOPHYTE.PLURAL, DATA_MALE_NEOPHYTE.SCORE, new DollBody(true, DATA_MALE_NEOPHYTE.SPD), new Abilities()
      {
        IsUndead = true,
        IsRotting = true,
        CanZombifyKilled = true,
        CanBashDoors = true,
        CanBreakObjects = true,
        CanPush = true,
        ZombieAI_AssaultBreakables = true,
        ZombieAI_Explore = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(DATA_MALE_NEOPHYTE.HP, DATA_MALE_NEOPHYTE.STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_MALE_NEOPHYTE.ATK, DATA_MALE_NEOPHYTE.DMG), new Defence(DATA_MALE_NEOPHYTE.DEF, DATA_MALE_NEOPHYTE.PRO_HIT, DATA_MALE_NEOPHYTE.PRO_SHOT), DATA_MALE_NEOPHYTE.FOV, NO_AUDIO, DATA_MALE_NEOPHYTE.SMELL, NO_INVENTORY), typeof (ZombieAI))
      {
        FlavorDescription = DATA_MALE_NEOPHYTE.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_FEMALE_NEOPHYTE] = new ActorModel("Actors\\female_neophyte", DATA_FEMALE_NEOPHYTE.NAME, DATA_FEMALE_NEOPHYTE.PLURAL, DATA_FEMALE_NEOPHYTE.SCORE, new DollBody(true, DATA_FEMALE_NEOPHYTE.SPD), new Abilities()
      {
        IsUndead = true,
        IsRotting = true,
        CanZombifyKilled = true,
        CanBashDoors = true,
        CanBreakObjects = true,
        CanPush = true,
        ZombieAI_AssaultBreakables = true,
        ZombieAI_Explore = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(DATA_FEMALE_NEOPHYTE.HP, DATA_FEMALE_NEOPHYTE.STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_FEMALE_NEOPHYTE.ATK, DATA_FEMALE_NEOPHYTE.DMG), new Defence(DATA_FEMALE_NEOPHYTE.DEF, DATA_FEMALE_NEOPHYTE.PRO_HIT, DATA_FEMALE_NEOPHYTE.PRO_SHOT), DATA_FEMALE_NEOPHYTE.FOV, NO_AUDIO, DATA_FEMALE_NEOPHYTE.SMELL, NO_INVENTORY), typeof (ZombieAI))
      {
        FlavorDescription = DATA_FEMALE_NEOPHYTE.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_MALE_DISCIPLE] = new ActorModel("Actors\\male_disciple", DATA_MALE_DISCIPLE.NAME, DATA_MALE_DISCIPLE.PLURAL, DATA_MALE_DISCIPLE.SCORE, new DollBody(true, DATA_MALE_DISCIPLE.SPD), new Abilities()
      {
        IsUndead = true,
        IsRotting = true,
        CanZombifyKilled = true,
        CanBashDoors = true,
        CanBreakObjects = true,
        CanPush = true,
        ZombieAI_AssaultBreakables = true,
        ZombieAI_Explore = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(DATA_MALE_DISCIPLE.HP, DATA_MALE_DISCIPLE.STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_MALE_DISCIPLE.ATK, DATA_MALE_DISCIPLE.DMG), new Defence(DATA_MALE_DISCIPLE.DEF, DATA_MALE_DISCIPLE.PRO_HIT, DATA_MALE_DISCIPLE.PRO_SHOT), DATA_MALE_DISCIPLE.FOV, NO_AUDIO, DATA_MALE_DISCIPLE.SMELL, NO_INVENTORY), typeof (ZombieAI))
      {
        FlavorDescription = DATA_MALE_DISCIPLE.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_FEMALE_DISCIPLE] = new ActorModel("Actors\\female_disciple", DATA_FEMALE_DISCIPLE.NAME, DATA_FEMALE_DISCIPLE.PLURAL, DATA_FEMALE_DISCIPLE.SCORE, new DollBody(true, DATA_FEMALE_DISCIPLE.SPD), new Abilities()
      {
        IsUndead = true,
        IsRotting = true,
        CanZombifyKilled = true,
        CanBashDoors = true,
        CanBreakObjects = true,
        CanPush = true,
        ZombieAI_AssaultBreakables = true,
        ZombieAI_Explore = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(DATA_FEMALE_DISCIPLE.HP, DATA_FEMALE_DISCIPLE.STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_FEMALE_DISCIPLE.ATK, DATA_FEMALE_DISCIPLE.DMG), new Defence(DATA_FEMALE_DISCIPLE.DEF, DATA_FEMALE_DISCIPLE.PRO_HIT, DATA_FEMALE_DISCIPLE.PRO_SHOT), DATA_FEMALE_DISCIPLE.FOV, NO_AUDIO, DATA_FEMALE_DISCIPLE.SMELL, NO_INVENTORY), typeof (ZombieAI))
      {
        FlavorDescription = DATA_FEMALE_DISCIPLE.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_ZOMBIE_MASTER] = new ActorModel("Actors\\zombie_master", DATA_ZM.NAME, DATA_ZM.PLURAL, DATA_ZM.SCORE, new DollBody(true, DATA_ZM.SPD), new Abilities()
      {
        IsUndead = true,
        IsUndeadMaster = true,
        IsRotting = true,
        CanZombifyKilled = true,
        CanBashDoors = true,
        CanBreakObjects = true,
        CanUseMapObjects = true,
        CanJump = true,
        CanJumpStumble = true,
        CanPush = true,
        ZombieAI_Explore = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(DATA_ZM.HP, DATA_ZM.STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_ZM.ATK, DATA_ZM.DMG), new Defence(DATA_ZM.DEF, DATA_ZM.PRO_HIT, DATA_ZM.PRO_SHOT), DATA_ZM.FOV, NO_AUDIO, DATA_ZM.SMELL, NO_INVENTORY), typeof (ZombieAI))
      {
        FlavorDescription = DATA_ZM.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_ZOMBIE_LORD] = new ActorModel("Actors\\zombie_lord", DATA_ZL.NAME, DATA_ZL.PLURAL, DATA_ZL.SCORE, new DollBody(true, DATA_ZL.SPD), new Abilities()
      {
        IsUndead = true,
        IsUndeadMaster = true,
        IsRotting = true,
        CanZombifyKilled = true,
        CanBashDoors = true,
        CanBreakObjects = true,
        CanUseMapObjects = true,
        CanJump = true,
        CanJumpStumble = true,
        CanPush = true,
        ZombieAI_AssaultBreakables = true,
        ZombieAI_Explore = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(DATA_ZL.HP, DATA_ZL.STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_ZL.ATK, DATA_ZL.DMG), new Defence(DATA_ZL.DEF, DATA_ZL.PRO_HIT, DATA_ZL.PRO_SHOT), DATA_ZL.FOV, NO_AUDIO, DATA_ZL.SMELL, NO_INVENTORY), typeof (ZombieAI))
      {
        FlavorDescription = DATA_ZL.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_ZOMBIE_PRINCE] = new ActorModel("Actors\\zombie_prince", DATA_ZP.NAME, DATA_ZP.PLURAL, DATA_ZP.SCORE, new DollBody(true, DATA_ZP.SPD), new Abilities()
      {
        IsUndead = true,
        IsUndeadMaster = true,
        IsRotting = true,
        CanZombifyKilled = true,
        CanBashDoors = true,
        CanBreakObjects = true,
        CanUseMapObjects = true,
        CanJump = true,
        CanJumpStumble = true,
        CanPush = true,
        ZombieAI_AssaultBreakables = true,
        ZombieAI_Explore = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(DATA_ZP.HP, DATA_ZP.STA, UNDEAD_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_ZP.ATK, DATA_ZP.DMG), new Defence(DATA_ZP.DEF, DATA_ZP.PRO_HIT, DATA_ZP.PRO_SHOT), DATA_ZP.FOV, NO_AUDIO, DATA_ZP.SMELL, NO_INVENTORY), typeof (ZombieAI))
      {
        FlavorDescription = DATA_ZP.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_RAT_ZOMBIE] = new ActorModel("Actors\\rat_zombie", DATA_RAT_ZOMBIE.NAME, DATA_RAT_ZOMBIE.PLURAL, DATA_RAT_ZOMBIE.SCORE, new DollBody(true, DATA_RAT_ZOMBIE.SPD), new Abilities()
      {
        IsUndead = true,
        IsSmall = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(DATA_RAT_ZOMBIE.HP, DATA_RAT_ZOMBIE.STA, NO_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_RAT_ZOMBIE.ATK, DATA_RAT_ZOMBIE.DMG), new Defence(DATA_RAT_ZOMBIE.DEF, DATA_RAT_ZOMBIE.PRO_HIT, DATA_RAT_ZOMBIE.PRO_SHOT), DATA_RAT_ZOMBIE.FOV, NO_AUDIO, DATA_RAT_ZOMBIE.SMELL, NO_INVENTORY), typeof (RatAI))
      {
        FlavorDescription = DATA_RAT_ZOMBIE.FLAVOR
      };
      this[GameActors.IDs.SEWERS_THING] = new ActorModel("Actors\\sewers_thing", DATA_SEWERS_THING.NAME, DATA_SEWERS_THING.PLURAL, DATA_SEWERS_THING.SCORE, new DollBody(true, DATA_SEWERS_THING.SPD), new Abilities()
      {
        IsUndead = true,
        CanBashDoors = true,
        CanBreakObjects = true
      }, new ActorSheet(DATA_SEWERS_THING.HP, DATA_SEWERS_THING.STA, NO_FOOD, NO_SLEEP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), DATA_SEWERS_THING.ATK, DATA_SEWERS_THING.DMG), new Defence(DATA_SEWERS_THING.DEF, DATA_SEWERS_THING.PRO_HIT, DATA_SEWERS_THING.PRO_SHOT), DATA_SEWERS_THING.FOV, NO_AUDIO, DATA_SEWERS_THING.SMELL, NO_INVENTORY), typeof (SewersThingAI))
      {
        FlavorDescription = DATA_SEWERS_THING.FLAVOR
      };
      this[GameActors.IDs.MALE_CIVILIAN] = new ActorModel((string) null, DATA_MALE_CIVILIAN.NAME, DATA_MALE_CIVILIAN.PLURAL, DATA_MALE_CIVILIAN.SCORE, new DollBody(true, DATA_MALE_CIVILIAN.SPD), new Abilities()
      {
        HasInventory = true,
        HasToEat = true,
        HasToSleep = true,
        HasSanity = true,
        CanTalk = true,
        CanUseMapObjects = true,
        CanBreakObjects = true,
        CanJump = true,
        CanTire = true,
        CanRun = true,
        CanUseItems = true,
        CanTrade = true,
        CanBarricade = true,
        CanPush = true,
        IsIntelligent = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(DATA_MALE_CIVILIAN.HP, DATA_MALE_CIVILIAN.STA, HUMAN_HUN, HUMAN_SLP, HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, DATA_MALE_CIVILIAN.ATK, DATA_MALE_CIVILIAN.DMG), new Defence(DATA_MALE_CIVILIAN.DEF, DATA_MALE_CIVILIAN.PRO_HIT, DATA_MALE_CIVILIAN.PRO_SHOT), DATA_MALE_CIVILIAN.FOV, DATA_MALE_CIVILIAN.AUDIO, NO_SMELL, HUMAN_INVENTORY), typeof (CivilianAI))
      {
        FlavorDescription = DATA_MALE_CIVILIAN.FLAVOR
      };
      this[GameActors.IDs.FEMALE_CIVILIAN] = new ActorModel((string) null, DATA_FEMALE_CIVILIAN.NAME, DATA_FEMALE_CIVILIAN.PLURAL, DATA_FEMALE_CIVILIAN.SCORE, new DollBody(false, DATA_FEMALE_CIVILIAN.SPD), new Abilities()
      {
        HasInventory = true,
        HasToEat = true,
        HasToSleep = true,
        HasSanity = true,
        CanTalk = true,
        CanUseMapObjects = true,
        CanBreakObjects = true,
        CanJump = true,
        CanTire = true,
        CanRun = true,
        CanUseItems = true,
        CanTrade = true,
        CanBarricade = true,
        CanPush = true,
        IsIntelligent = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(DATA_FEMALE_CIVILIAN.HP, DATA_FEMALE_CIVILIAN.STA, HUMAN_HUN, HUMAN_SLP, HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, DATA_FEMALE_CIVILIAN.ATK, DATA_FEMALE_CIVILIAN.DMG), new Defence(DATA_FEMALE_CIVILIAN.DEF, DATA_FEMALE_CIVILIAN.PRO_HIT, DATA_FEMALE_CIVILIAN.PRO_SHOT), DATA_FEMALE_CIVILIAN.FOV, DATA_FEMALE_CIVILIAN.AUDIO, NO_SMELL, HUMAN_INVENTORY), typeof (CivilianAI))
      {
        FlavorDescription = DATA_FEMALE_CIVILIAN.FLAVOR
      };
      this[GameActors.IDs.CHAR_GUARD] = new ActorModel((string) null, DATA_CHAR_GUARD.NAME, DATA_CHAR_GUARD.PLURAL, DATA_CHAR_GUARD.SCORE, new DollBody(true, DATA_CHAR_GUARD.SPD), new Abilities()
      {
        HasInventory = true,
        CanUseMapObjects = true,
        CanBreakObjects = true,
        CanJump = true,
        CanTire = true,
        CanRun = true,
        CanUseItems = true,
        HasToSleep = true,
        HasSanity = true,
        CanTalk = true,
        CanPush = true,
        CanBarricade = true,
        IsIntelligent = true
      }, new ActorSheet(DATA_CHAR_GUARD.HP, DATA_CHAR_GUARD.STA, HUMAN_HUN, HUMAN_SLP, HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, DATA_CHAR_GUARD.ATK, DATA_CHAR_GUARD.DMG), new Defence(DATA_CHAR_GUARD.DEF, DATA_CHAR_GUARD.PRO_HIT, DATA_CHAR_GUARD.PRO_SHOT), DATA_CHAR_GUARD.FOV, DATA_CHAR_GUARD.AUDIO, NO_SMELL, HUMAN_INVENTORY), typeof (CHARGuardAI))
      {
        FlavorDescription = DATA_CHAR_GUARD.FLAVOR
      };
      this[GameActors.IDs.ARMY_NATIONAL_GUARD] = new ActorModel((string) null, DATA_NATGUARD.NAME, DATA_NATGUARD.PLURAL, DATA_NATGUARD.SCORE, new DollBody(true, DATA_NATGUARD.SPD), new Abilities()
      {
        HasInventory = true,
        CanUseMapObjects = true,
        CanBreakObjects = true,
        CanJump = true,
        CanTire = true,
        CanRun = true,
        CanUseItems = true,
        CanTalk = true,
        HasToSleep = true,
        HasSanity = true,
        CanPush = true,
        CanBarricade = true,
        IsIntelligent = true
      }, new ActorSheet(DATA_NATGUARD.HP, DATA_NATGUARD.STA, HUMAN_HUN, HUMAN_SLP, HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, DATA_NATGUARD.ATK, DATA_NATGUARD.DMG), new Defence(DATA_NATGUARD.DEF, DATA_NATGUARD.PRO_HIT, DATA_NATGUARD.PRO_SHOT), DATA_NATGUARD.FOV, DATA_NATGUARD.AUDIO, NO_SMELL, HUMAN_INVENTORY), typeof (SoldierAI))
      {
        FlavorDescription = DATA_NATGUARD.FLAVOR
      };
      this[GameActors.IDs.BIKER_MAN] = new ActorModel((string) null, DATA_BIKER_MAN.NAME, DATA_BIKER_MAN.PLURAL, DATA_BIKER_MAN.SCORE, new DollBody(true, DATA_BIKER_MAN.SPD), new Abilities()
      {
        HasInventory = true,
        CanUseMapObjects = true,
        CanBreakObjects = true,
        CanJump = true,
        CanTire = true,
        CanRun = true,
        CanUseItems = true,
        HasToEat = true,
        HasToSleep = true,
        HasSanity = true,
        CanTalk = true,
        CanPush = true,
        CanBarricade = true,
        CanTrade = true,
        IsIntelligent = true,
        AI_NotInterestedInRangedWeapons = true
      }, new ActorSheet(DATA_BIKER_MAN.HP, DATA_BIKER_MAN.STA, HUMAN_HUN, HUMAN_SLP, HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, DATA_BIKER_MAN.ATK, DATA_BIKER_MAN.DMG), new Defence(DATA_BIKER_MAN.DEF, DATA_BIKER_MAN.PRO_HIT, DATA_BIKER_MAN.PRO_SHOT), DATA_BIKER_MAN.FOV, DATA_BIKER_MAN.AUDIO, NO_SMELL, HUMAN_INVENTORY), typeof (GangAI))
      {
        FlavorDescription = DATA_BIKER_MAN.FLAVOR
      };
      this[GameActors.IDs.GANGSTA_MAN] = new ActorModel((string) null, DATA_GANGSTA_MAN.NAME, DATA_GANGSTA_MAN.PLURAL, DATA_GANGSTA_MAN.SCORE, new DollBody(true, DATA_GANGSTA_MAN.SPD), new Abilities()
      {
        HasInventory = true,
        CanUseMapObjects = true,
        CanBreakObjects = true,
        CanJump = true,
        CanTire = true,
        CanRun = true,
        CanUseItems = true,
        HasToEat = true,
        HasToSleep = true,
        HasSanity = true,
        CanTalk = true,
        CanPush = true,
        CanBarricade = true,
        CanTrade = true,
        IsIntelligent = true
      }, new ActorSheet(DATA_GANGSTA_MAN.HP, DATA_GANGSTA_MAN.STA, HUMAN_HUN, HUMAN_SLP, HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, DATA_GANGSTA_MAN.ATK, DATA_GANGSTA_MAN.DMG), new Defence(DATA_GANGSTA_MAN.DEF, DATA_GANGSTA_MAN.PRO_HIT, DATA_GANGSTA_MAN.PRO_SHOT), DATA_GANGSTA_MAN.FOV, DATA_GANGSTA_MAN.AUDIO, NO_SMELL, HUMAN_INVENTORY), typeof (GangAI))
      {
        FlavorDescription = DATA_GANGSTA_MAN.FLAVOR
      };
      this[GameActors.IDs.POLICEMAN] = new ActorModel((string) null, DATA_POLICEMAN.NAME, DATA_POLICEMAN.PLURAL, DATA_POLICEMAN.SCORE, new DollBody(true, DATA_POLICEMAN.SPD), new Abilities()
      {
        HasInventory = true,
        HasToEat = true,
        HasToSleep = true,
        HasSanity = true,
        CanTalk = true,
        CanUseMapObjects = true,
        CanBreakObjects = true,
        CanJump = true,
        CanTire = true,
        CanRun = true,
        CanUseItems = true,
        CanTrade = true,
        CanBarricade = true,
        CanPush = true,
        AI_CanUseAIExits = true,
        IsLawEnforcer = true,
        IsIntelligent = true
      }, new ActorSheet(DATA_POLICEMAN.HP, DATA_POLICEMAN.STA, HUMAN_HUN, HUMAN_SLP, HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, DATA_POLICEMAN.ATK, DATA_POLICEMAN.DMG), new Defence(DATA_POLICEMAN.DEF, DATA_POLICEMAN.PRO_HIT, DATA_POLICEMAN.PRO_SHOT), DATA_POLICEMAN.FOV, DATA_POLICEMAN.AUDIO, NO_SMELL, HUMAN_INVENTORY), typeof (CivilianAI))
      {
        FlavorDescription = DATA_POLICEMAN.FLAVOR
      };
      this[GameActors.IDs.BLACKOPS_MAN] = new ActorModel((string) null, DATA_BLACKOPS_MAN.NAME, DATA_BLACKOPS_MAN.PLURAL, DATA_BLACKOPS_MAN.SCORE, new DollBody(true, DATA_BLACKOPS_MAN.SPD), new Abilities()
      {
        HasInventory = true,
        CanUseMapObjects = true,
        CanBreakObjects = true,
        CanJump = true,
        CanTire = true,
        CanRun = true,
        CanUseItems = true,
        CanTalk = true,
        HasToSleep = true,
        HasSanity = true,
        CanPush = true,
        CanBarricade = true,
        IsIntelligent = true
      }, new ActorSheet(DATA_BLACKOPS_MAN.HP, DATA_BLACKOPS_MAN.STA, HUMAN_HUN, HUMAN_SLP, HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, DATA_BLACKOPS_MAN.ATK, DATA_BLACKOPS_MAN.DMG), new Defence(DATA_BLACKOPS_MAN.DEF, DATA_BLACKOPS_MAN.PRO_HIT, DATA_BLACKOPS_MAN.PRO_SHOT), DATA_BLACKOPS_MAN.FOV, DATA_BLACKOPS_MAN.AUDIO, NO_SMELL, HUMAN_INVENTORY), typeof (SoldierAI))
      {
        FlavorDescription = DATA_BLACKOPS_MAN.FLAVOR
      };
      GameActors.ActorData actorData = DATA_FERAL_DOG;
      this[GameActors.IDs.FERAL_DOG] = new ActorModel((string) null, actorData.NAME, actorData.PLURAL, actorData.SCORE, new DollBody(true, actorData.SPD), new Abilities()
      {
        HasInventory = true,
        HasToEat = true,
        HasToSleep = true,
        CanBreakObjects = true,
        CanJump = true,
        CanTire = true,
        CanRun = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(actorData.HP, actorData.STA, DOG_HUN, DOG_SLP, NO_SANITY, new Attack(AttackKind.PHYSICAL, new Verb("bite"), actorData.ATK, actorData.DMG), new Defence(actorData.DEF, actorData.PRO_HIT, actorData.PRO_SHOT), actorData.FOV, actorData.AUDIO, actorData.SMELL, DOG_INVENTORY), typeof (FeralDogAI))
      {
        FlavorDescription = actorData.FLAVOR
      };
      this[GameActors.IDs.JASON_MYERS] = new ActorModel((string) null, DATA_JASON_MYERS.NAME, DATA_JASON_MYERS.PLURAL, DATA_JASON_MYERS.SCORE, new DollBody(true, DATA_JASON_MYERS.SPD), new Abilities()
      {
        HasInventory = true,
        CanUseMapObjects = true,
        CanBreakObjects = true,
        CanJump = true,
        CanTire = true,
        CanRun = true,
        CanUseItems = true,
        HasToEat = false,
        HasToSleep = false,
        CanTalk = true,
        CanPush = true,
        CanBarricade = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(DATA_JASON_MYERS.HP, DATA_JASON_MYERS.STA, HUMAN_HUN, HUMAN_SLP, NO_SANITY, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, DATA_JASON_MYERS.ATK, DATA_JASON_MYERS.DMG), new Defence(DATA_JASON_MYERS.DEF, DATA_JASON_MYERS.PRO_HIT, DATA_JASON_MYERS.PRO_SHOT), DATA_JASON_MYERS.FOV, DATA_JASON_MYERS.AUDIO, NO_SMELL, HUMAN_INVENTORY), typeof (InsaneHumanAI))
      {
        FlavorDescription = DATA_JASON_MYERS.FLAVOR
      };
    }

    public bool LoadFromCSV(IRogueUI ui, string path)
    {
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
      CSVTable toTable = new CSVParser().ParseToTable(stringList.ToArray(), 16);
      Notify(ui, "reading data...");
      DATA_SKELETON = GetDataFromCSVTable(ui, toTable, GameActors.IDs._FIRST);
            DATA_RED_EYED_SKELETON = GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_RED_EYED_SKELETON);
            DATA_RED_SKELETON = GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_RED_SKELETON);
            DATA_ZOMBIE = GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_ZOMBIE);
            DATA_DARK_EYED_ZOMBIE = GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_DARK_EYED_ZOMBIE);
            DATA_DARK_ZOMBIE = GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_DARK_ZOMBIE);
            DATA_MALE_ZOMBIFIED = GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_MALE_ZOMBIFIED);
            DATA_FEMALE_ZOMBIFIED = GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED);
            DATA_MALE_NEOPHYTE = GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_MALE_NEOPHYTE);
            DATA_FEMALE_NEOPHYTE = GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_FEMALE_NEOPHYTE);
            DATA_MALE_DISCIPLE = GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_MALE_DISCIPLE);
            DATA_FEMALE_DISCIPLE = GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_FEMALE_DISCIPLE);
            DATA_ZM = GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_ZOMBIE_MASTER);
            DATA_ZL = GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_ZOMBIE_LORD);
            DATA_ZP = GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_ZOMBIE_PRINCE);
            DATA_RAT_ZOMBIE = GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_RAT_ZOMBIE);
            DATA_SEWERS_THING = GetDataFromCSVTable(ui, toTable, GameActors.IDs.SEWERS_THING);
            DATA_MALE_CIVILIAN = GetDataFromCSVTable(ui, toTable, GameActors.IDs.MALE_CIVILIAN);
            DATA_FEMALE_CIVILIAN = GetDataFromCSVTable(ui, toTable, GameActors.IDs.FEMALE_CIVILIAN);
            DATA_FERAL_DOG = GetDataFromCSVTable(ui, toTable, GameActors.IDs.FERAL_DOG);
            DATA_POLICEMAN = GetDataFromCSVTable(ui, toTable, GameActors.IDs.POLICEMAN);
            DATA_CHAR_GUARD = GetDataFromCSVTable(ui, toTable, GameActors.IDs.CHAR_GUARD);
            DATA_NATGUARD = GetDataFromCSVTable(ui, toTable, GameActors.IDs.ARMY_NATIONAL_GUARD);
            DATA_BIKER_MAN = GetDataFromCSVTable(ui, toTable, GameActors.IDs.BIKER_MAN);
            DATA_GANGSTA_MAN = GetDataFromCSVTable(ui, toTable, GameActors.IDs.GANGSTA_MAN);
            DATA_BLACKOPS_MAN = GetDataFromCSVTable(ui, toTable, GameActors.IDs.BLACKOPS_MAN);
            DATA_JASON_MYERS = GetDataFromCSVTable(ui, toTable, GameActors.IDs.JASON_MYERS);
            CreateModels();
            Notify(ui, "done!");
      return true;
    }

    private CSVLine FindLineForModel(CSVTable table, GameActors.IDs modelID)
    {
      foreach (CSVLine line in table.Lines) {
        if (line[0].ParseText() == modelID.ToString())
          return line;
      }
      throw new InvalidOperationException(string.Format("model {0} not found", (object) modelID.ToString()));
    }

    private GameActors.ActorData GetDataFromCSVTable(IRogueUI ui, CSVTable table, GameActors.IDs modelID)
    {
      CSVLine lineForModel = FindLineForModel(table, modelID);
      try
      {
        return GameActors.ActorData.FromCSVLine(lineForModel);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException(string.Format("invalid data format for model {0}; exception : {1}", (object) modelID.ToString(), (object) ex.ToString()));
      }
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
      _COUNT = 27,
    }

    public struct ActorData
    {
      public const int COUNT_FIELDS = 16;

      public string NAME { get; set; }

      public string PLURAL { get; set; }

      public int SPD { get; set; }

      public int HP { get; set; }

      public int STA { get; set; }

      public int ATK { get; set; }

      public int DMG { get; set; }

      public int DEF { get; set; }

      public int PRO_HIT { get; set; }

      public int PRO_SHOT { get; set; }

      public int FOV { get; set; }

      public int AUDIO { get; set; }

      public int SMELL { get; set; }

      public int SCORE { get; set; }

      public string FLAVOR { get; set; }

      public static GameActors.ActorData FromCSVLine(CSVLine line)
      {
        return new GameActors.ActorData()
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
}
