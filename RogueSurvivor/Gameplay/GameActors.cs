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

namespace djack.RogueSurvivor.Gameplay
{
  internal class GameActors : ActorModelDB
  {
    private static readonly Verb VERB_PUNCH = new Verb("punch", "punches");
    private ActorModel[] m_Models = new ActorModel[27];
    public int UNDEAD_FOOD = 4*WorldTime.TURNS_PER_DAY;
    public int HUMAN_HUN = 2*WorldTime.TURNS_PER_DAY;
    public int HUMAN_SLP = 1800;
    public int HUMAN_SAN = 4*WorldTime.TURNS_PER_DAY;
    public int HUMAN_INVENTORY = 7;
    public int DOG_HUN = 2*WorldTime.TURNS_PER_DAY;
    public int DOG_SLP = 1800;
    public int DOG_INVENTORY = 1;
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
        return this.m_Models[id];
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
        this.m_Models[(int) id] = value;
        this.m_Models[(int) id].ID = (int) id;
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
      this[GameActors.IDs._FIRST] = new ActorModel("Actors\\skeleton", this.DATA_SKELETON.NAME, this.DATA_SKELETON.PLURAL, this.DATA_SKELETON.SCORE, new DollBody(true, this.DATA_SKELETON.SPD), new Abilities()
      {
        IsUndead = true
      }, new ActorSheet(this.DATA_SKELETON.HP, this.DATA_SKELETON.STA, 0, 0, 0, new Attack(AttackKind.PHYSICAL, new Verb("claw"), this.DATA_SKELETON.ATK, this.DATA_SKELETON.DMG), new Defence(this.DATA_SKELETON.DEF, this.DATA_SKELETON.PRO_HIT, this.DATA_SKELETON.PRO_SHOT), this.DATA_SKELETON.FOV, 0, 0, 0), typeof (SkeletonAI))
      {
        FlavorDescription = this.DATA_SKELETON.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_RED_EYED_SKELETON] = new ActorModel("Actors\\red_eyed_skeleton", this.DATA_RED_EYED_SKELETON.NAME, this.DATA_RED_EYED_SKELETON.PLURAL, this.DATA_RED_EYED_SKELETON.SCORE, new DollBody(true, this.DATA_RED_EYED_SKELETON.SPD), new Abilities()
      {
        IsUndead = true
      }, new ActorSheet(this.DATA_RED_EYED_SKELETON.HP, this.DATA_RED_EYED_SKELETON.STA, 0, 0, 0, new Attack(AttackKind.PHYSICAL, new Verb("claw"), this.DATA_RED_EYED_SKELETON.ATK, this.DATA_RED_EYED_SKELETON.DMG), new Defence(this.DATA_RED_EYED_SKELETON.DEF, this.DATA_RED_EYED_SKELETON.PRO_HIT, this.DATA_RED_EYED_SKELETON.PRO_SHOT), this.DATA_RED_EYED_SKELETON.FOV, 0, 0, 0), typeof (SkeletonAI))
      {
        FlavorDescription = this.DATA_RED_EYED_SKELETON.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_RED_SKELETON] = new ActorModel("Actors\\red_skeleton", this.DATA_RED_SKELETON.NAME, this.DATA_RED_SKELETON.PLURAL, this.DATA_RED_SKELETON.SCORE, new DollBody(true, this.DATA_RED_SKELETON.SPD), new Abilities()
      {
        IsUndead = true
      }, new ActorSheet(this.DATA_RED_SKELETON.HP, this.DATA_RED_SKELETON.STA, 0, 0, 0, new Attack(AttackKind.PHYSICAL, new Verb("claw"), this.DATA_RED_SKELETON.ATK, this.DATA_RED_SKELETON.DMG), new Defence(this.DATA_RED_SKELETON.DEF, this.DATA_RED_SKELETON.PRO_HIT, this.DATA_RED_SKELETON.PRO_SHOT), this.DATA_RED_SKELETON.FOV, 0, 0, 0), typeof (SkeletonAI))
      {
        FlavorDescription = this.DATA_RED_SKELETON.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_ZOMBIE] = new ActorModel("Actors\\zombie", this.DATA_ZOMBIE.NAME, this.DATA_ZOMBIE.PLURAL, this.DATA_ZOMBIE.SCORE, new DollBody(true, this.DATA_ZOMBIE.SPD), new Abilities()
      {
        IsUndead = true,
        IsRotting = true,
        CanZombifyKilled = true,
        CanBashDoors = true,
        CanBreakObjects = true,
        ZombieAI_Explore = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(this.DATA_ZOMBIE.HP, this.DATA_ZOMBIE.STA, this.UNDEAD_FOOD, 0, 0, new Attack(AttackKind.PHYSICAL, new Verb("bite"), this.DATA_ZOMBIE.ATK, this.DATA_ZOMBIE.DMG), new Defence(this.DATA_ZOMBIE.DEF, this.DATA_ZOMBIE.PRO_HIT, this.DATA_ZOMBIE.PRO_SHOT), this.DATA_ZOMBIE.FOV, 0, this.DATA_ZOMBIE.SMELL, 0), typeof (ZombieAI))
      {
        FlavorDescription = this.DATA_ZOMBIE.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_DARK_EYED_ZOMBIE] = new ActorModel("Actors\\dark_eyed_zombie", this.DATA_DARK_EYED_ZOMBIE.NAME, this.DATA_DARK_EYED_ZOMBIE.PLURAL, this.DATA_DARK_EYED_ZOMBIE.SCORE, new DollBody(true, this.DATA_DARK_EYED_ZOMBIE.SPD), new Abilities()
      {
        IsUndead = true,
        IsRotting = true,
        CanZombifyKilled = true,
        CanBashDoors = true,
        CanBreakObjects = true,
        ZombieAI_Explore = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(this.DATA_DARK_EYED_ZOMBIE.HP, this.DATA_DARK_EYED_ZOMBIE.STA, this.UNDEAD_FOOD, 0, 0, new Attack(AttackKind.PHYSICAL, new Verb("bite"), this.DATA_DARK_EYED_ZOMBIE.ATK, this.DATA_DARK_EYED_ZOMBIE.DMG), new Defence(this.DATA_DARK_EYED_ZOMBIE.DEF, this.DATA_DARK_EYED_ZOMBIE.PRO_HIT, this.DATA_DARK_EYED_ZOMBIE.PRO_SHOT), this.DATA_DARK_EYED_ZOMBIE.FOV, 0, this.DATA_DARK_EYED_ZOMBIE.SMELL, 0), typeof (ZombieAI))
      {
        FlavorDescription = this.DATA_DARK_EYED_ZOMBIE.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_DARK_ZOMBIE] = new ActorModel("Actors\\dark_zombie", this.DATA_DARK_ZOMBIE.NAME, this.DATA_DARK_ZOMBIE.PLURAL, this.DATA_DARK_ZOMBIE.SCORE, new DollBody(true, this.DATA_DARK_ZOMBIE.SPD), new Abilities()
      {
        IsUndead = true,
        IsRotting = true,
        CanZombifyKilled = true,
        CanBashDoors = true,
        CanBreakObjects = true,
        ZombieAI_Explore = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(this.DATA_DARK_ZOMBIE.HP, this.DATA_DARK_ZOMBIE.STA, this.UNDEAD_FOOD, 0, 0, new Attack(AttackKind.PHYSICAL, new Verb("bite"), this.DATA_DARK_ZOMBIE.ATK, this.DATA_DARK_ZOMBIE.DMG), new Defence(this.DATA_DARK_ZOMBIE.DEF, this.DATA_DARK_ZOMBIE.PRO_HIT, this.DATA_DARK_ZOMBIE.PRO_SHOT), this.DATA_DARK_ZOMBIE.FOV, 0, this.DATA_DARK_ZOMBIE.SMELL, 0), typeof (ZombieAI))
      {
        FlavorDescription = this.DATA_DARK_ZOMBIE.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_MALE_ZOMBIFIED] = new ActorModel((string) null, this.DATA_MALE_ZOMBIFIED.NAME, this.DATA_MALE_ZOMBIFIED.PLURAL, this.DATA_MALE_ZOMBIFIED.SCORE, new DollBody(true, this.DATA_MALE_ZOMBIFIED.SPD), new Abilities()
      {
        IsUndead = true,
        IsRotting = true,
        CanZombifyKilled = true,
        CanBashDoors = true,
        CanBreakObjects = true,
        ZombieAI_Explore = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(this.DATA_MALE_ZOMBIFIED.HP, this.DATA_MALE_ZOMBIFIED.STA, this.UNDEAD_FOOD, 0, 0, new Attack(AttackKind.PHYSICAL, new Verb("bite"), this.DATA_MALE_ZOMBIFIED.ATK, this.DATA_MALE_ZOMBIFIED.DMG), new Defence(this.DATA_MALE_ZOMBIFIED.DEF, this.DATA_MALE_ZOMBIFIED.PRO_HIT, this.DATA_MALE_ZOMBIFIED.PRO_SHOT), this.DATA_MALE_ZOMBIFIED.FOV, 0, this.DATA_MALE_ZOMBIFIED.SMELL, 0), typeof (ZombieAI))
      {
        FlavorDescription = this.DATA_MALE_ZOMBIFIED.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED] = new ActorModel((string) null, this.DATA_FEMALE_ZOMBIFIED.NAME, this.DATA_FEMALE_ZOMBIFIED.PLURAL, this.DATA_FEMALE_ZOMBIFIED.SCORE, new DollBody(true, this.DATA_FEMALE_ZOMBIFIED.SPD), new Abilities()
      {
        IsUndead = true,
        IsRotting = true,
        CanZombifyKilled = true,
        CanBashDoors = true,
        CanBreakObjects = true,
        ZombieAI_Explore = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(this.DATA_FEMALE_ZOMBIFIED.HP, this.DATA_FEMALE_ZOMBIFIED.STA, this.UNDEAD_FOOD, 0, 0, new Attack(AttackKind.PHYSICAL, new Verb("bite"), this.DATA_FEMALE_ZOMBIFIED.ATK, this.DATA_FEMALE_ZOMBIFIED.DMG), new Defence(this.DATA_FEMALE_ZOMBIFIED.DEF, this.DATA_FEMALE_ZOMBIFIED.PRO_HIT, this.DATA_FEMALE_ZOMBIFIED.PRO_SHOT), this.DATA_FEMALE_ZOMBIFIED.FOV, 0, this.DATA_FEMALE_ZOMBIFIED.SMELL, 0), typeof (ZombieAI))
      {
        FlavorDescription = this.DATA_FEMALE_ZOMBIFIED.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_MALE_NEOPHYTE] = new ActorModel("Actors\\male_neophyte", this.DATA_MALE_NEOPHYTE.NAME, this.DATA_MALE_NEOPHYTE.PLURAL, this.DATA_MALE_NEOPHYTE.SCORE, new DollBody(true, this.DATA_MALE_NEOPHYTE.SPD), new Abilities()
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
      }, new ActorSheet(this.DATA_MALE_NEOPHYTE.HP, this.DATA_MALE_NEOPHYTE.STA, this.UNDEAD_FOOD, 0, 0, new Attack(AttackKind.PHYSICAL, new Verb("bite"), this.DATA_MALE_NEOPHYTE.ATK, this.DATA_MALE_NEOPHYTE.DMG), new Defence(this.DATA_MALE_NEOPHYTE.DEF, this.DATA_MALE_NEOPHYTE.PRO_HIT, this.DATA_MALE_NEOPHYTE.PRO_SHOT), this.DATA_MALE_NEOPHYTE.FOV, 0, this.DATA_MALE_NEOPHYTE.SMELL, 0), typeof (ZombieAI))
      {
        FlavorDescription = this.DATA_MALE_NEOPHYTE.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_FEMALE_NEOPHYTE] = new ActorModel("Actors\\female_neophyte", this.DATA_FEMALE_NEOPHYTE.NAME, this.DATA_FEMALE_NEOPHYTE.PLURAL, this.DATA_FEMALE_NEOPHYTE.SCORE, new DollBody(true, this.DATA_FEMALE_NEOPHYTE.SPD), new Abilities()
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
      }, new ActorSheet(this.DATA_FEMALE_NEOPHYTE.HP, this.DATA_FEMALE_NEOPHYTE.STA, this.UNDEAD_FOOD, 0, 0, new Attack(AttackKind.PHYSICAL, new Verb("bite"), this.DATA_FEMALE_NEOPHYTE.ATK, this.DATA_FEMALE_NEOPHYTE.DMG), new Defence(this.DATA_FEMALE_NEOPHYTE.DEF, this.DATA_FEMALE_NEOPHYTE.PRO_HIT, this.DATA_FEMALE_NEOPHYTE.PRO_SHOT), this.DATA_FEMALE_NEOPHYTE.FOV, 0, this.DATA_FEMALE_NEOPHYTE.SMELL, 0), typeof (ZombieAI))
      {
        FlavorDescription = this.DATA_FEMALE_NEOPHYTE.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_MALE_DISCIPLE] = new ActorModel("Actors\\male_disciple", this.DATA_MALE_DISCIPLE.NAME, this.DATA_MALE_DISCIPLE.PLURAL, this.DATA_MALE_DISCIPLE.SCORE, new DollBody(true, this.DATA_MALE_DISCIPLE.SPD), new Abilities()
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
      }, new ActorSheet(this.DATA_MALE_DISCIPLE.HP, this.DATA_MALE_DISCIPLE.STA, this.UNDEAD_FOOD, 0, 0, new Attack(AttackKind.PHYSICAL, new Verb("bite"), this.DATA_MALE_DISCIPLE.ATK, this.DATA_MALE_DISCIPLE.DMG), new Defence(this.DATA_MALE_DISCIPLE.DEF, this.DATA_MALE_DISCIPLE.PRO_HIT, this.DATA_MALE_DISCIPLE.PRO_SHOT), this.DATA_MALE_DISCIPLE.FOV, 0, this.DATA_MALE_DISCIPLE.SMELL, 0), typeof (ZombieAI))
      {
        FlavorDescription = this.DATA_MALE_DISCIPLE.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_FEMALE_DISCIPLE] = new ActorModel("Actors\\female_disciple", this.DATA_FEMALE_DISCIPLE.NAME, this.DATA_FEMALE_DISCIPLE.PLURAL, this.DATA_FEMALE_DISCIPLE.SCORE, new DollBody(true, this.DATA_FEMALE_DISCIPLE.SPD), new Abilities()
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
      }, new ActorSheet(this.DATA_FEMALE_DISCIPLE.HP, this.DATA_FEMALE_DISCIPLE.STA, this.UNDEAD_FOOD, 0, 0, new Attack(AttackKind.PHYSICAL, new Verb("bite"), this.DATA_FEMALE_DISCIPLE.ATK, this.DATA_FEMALE_DISCIPLE.DMG), new Defence(this.DATA_FEMALE_DISCIPLE.DEF, this.DATA_FEMALE_DISCIPLE.PRO_HIT, this.DATA_FEMALE_DISCIPLE.PRO_SHOT), this.DATA_FEMALE_DISCIPLE.FOV, 0, this.DATA_FEMALE_DISCIPLE.SMELL, 0), typeof (ZombieAI))
      {
        FlavorDescription = this.DATA_FEMALE_DISCIPLE.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_ZOMBIE_MASTER] = new ActorModel("Actors\\zombie_master", this.DATA_ZM.NAME, this.DATA_ZM.PLURAL, this.DATA_ZM.SCORE, new DollBody(true, this.DATA_ZM.SPD), new Abilities()
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
      }, new ActorSheet(this.DATA_ZM.HP, this.DATA_ZM.STA, this.UNDEAD_FOOD, 0, 0, new Attack(AttackKind.PHYSICAL, new Verb("bite"), this.DATA_ZM.ATK, this.DATA_ZM.DMG), new Defence(this.DATA_ZM.DEF, this.DATA_ZM.PRO_HIT, this.DATA_ZM.PRO_SHOT), this.DATA_ZM.FOV, 0, this.DATA_ZM.SMELL, 0), typeof (ZombieAI))
      {
        FlavorDescription = this.DATA_ZM.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_ZOMBIE_LORD] = new ActorModel("Actors\\zombie_lord", this.DATA_ZL.NAME, this.DATA_ZL.PLURAL, this.DATA_ZL.SCORE, new DollBody(true, this.DATA_ZL.SPD), new Abilities()
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
      }, new ActorSheet(this.DATA_ZL.HP, this.DATA_ZL.STA, this.UNDEAD_FOOD, 0, 0, new Attack(AttackKind.PHYSICAL, new Verb("bite"), this.DATA_ZL.ATK, this.DATA_ZL.DMG), new Defence(this.DATA_ZL.DEF, this.DATA_ZL.PRO_HIT, this.DATA_ZL.PRO_SHOT), this.DATA_ZL.FOV, 0, this.DATA_ZL.SMELL, 0), typeof (ZombieAI))
      {
        FlavorDescription = this.DATA_ZL.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_ZOMBIE_PRINCE] = new ActorModel("Actors\\zombie_prince", this.DATA_ZP.NAME, this.DATA_ZP.PLURAL, this.DATA_ZP.SCORE, new DollBody(true, this.DATA_ZP.SPD), new Abilities()
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
      }, new ActorSheet(this.DATA_ZP.HP, this.DATA_ZP.STA, this.UNDEAD_FOOD, 0, 0, new Attack(AttackKind.PHYSICAL, new Verb("bite"), this.DATA_ZP.ATK, this.DATA_ZP.DMG), new Defence(this.DATA_ZP.DEF, this.DATA_ZP.PRO_HIT, this.DATA_ZP.PRO_SHOT), this.DATA_ZP.FOV, 0, this.DATA_ZP.SMELL, 0), typeof (ZombieAI))
      {
        FlavorDescription = this.DATA_ZP.FLAVOR
      };
      this[GameActors.IDs.UNDEAD_RAT_ZOMBIE] = new ActorModel("Actors\\rat_zombie", this.DATA_RAT_ZOMBIE.NAME, this.DATA_RAT_ZOMBIE.PLURAL, this.DATA_RAT_ZOMBIE.SCORE, new DollBody(true, this.DATA_RAT_ZOMBIE.SPD), new Abilities()
      {
        IsUndead = true,
        IsSmall = true,
        AI_CanUseAIExits = true
      }, new ActorSheet(this.DATA_RAT_ZOMBIE.HP, this.DATA_RAT_ZOMBIE.STA, 0, 0, 0, new Attack(AttackKind.PHYSICAL, new Verb("bite"), this.DATA_RAT_ZOMBIE.ATK, this.DATA_RAT_ZOMBIE.DMG), new Defence(this.DATA_RAT_ZOMBIE.DEF, this.DATA_RAT_ZOMBIE.PRO_HIT, this.DATA_RAT_ZOMBIE.PRO_SHOT), this.DATA_RAT_ZOMBIE.FOV, 0, this.DATA_RAT_ZOMBIE.SMELL, 0), typeof (RatAI))
      {
        FlavorDescription = this.DATA_RAT_ZOMBIE.FLAVOR
      };
      this[GameActors.IDs.SEWERS_THING] = new ActorModel("Actors\\sewers_thing", this.DATA_SEWERS_THING.NAME, this.DATA_SEWERS_THING.PLURAL, this.DATA_SEWERS_THING.SCORE, new DollBody(true, this.DATA_SEWERS_THING.SPD), new Abilities()
      {
        IsUndead = true,
        CanBashDoors = true,
        CanBreakObjects = true
      }, new ActorSheet(this.DATA_SEWERS_THING.HP, this.DATA_SEWERS_THING.STA, 0, 0, 0, new Attack(AttackKind.PHYSICAL, new Verb("bite"), this.DATA_SEWERS_THING.ATK, this.DATA_SEWERS_THING.DMG), new Defence(this.DATA_SEWERS_THING.DEF, this.DATA_SEWERS_THING.PRO_HIT, this.DATA_SEWERS_THING.PRO_SHOT), this.DATA_SEWERS_THING.FOV, 0, this.DATA_SEWERS_THING.SMELL, 0), typeof (SewersThingAI))
      {
        FlavorDescription = this.DATA_SEWERS_THING.FLAVOR
      };
      this[GameActors.IDs.MALE_CIVILIAN] = new ActorModel((string) null, this.DATA_MALE_CIVILIAN.NAME, this.DATA_MALE_CIVILIAN.PLURAL, this.DATA_MALE_CIVILIAN.SCORE, new DollBody(true, this.DATA_MALE_CIVILIAN.SPD), new Abilities()
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
      }, new ActorSheet(this.DATA_MALE_CIVILIAN.HP, this.DATA_MALE_CIVILIAN.STA, this.HUMAN_HUN, this.HUMAN_SLP, this.HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, this.DATA_MALE_CIVILIAN.ATK, this.DATA_MALE_CIVILIAN.DMG), new Defence(this.DATA_MALE_CIVILIAN.DEF, this.DATA_MALE_CIVILIAN.PRO_HIT, this.DATA_MALE_CIVILIAN.PRO_SHOT), this.DATA_MALE_CIVILIAN.FOV, this.DATA_MALE_CIVILIAN.AUDIO, 0, this.HUMAN_INVENTORY), (Type) null)
      {
        FlavorDescription = this.DATA_MALE_CIVILIAN.FLAVOR
      };
      this[GameActors.IDs.FEMALE_CIVILIAN] = new ActorModel((string) null, this.DATA_FEMALE_CIVILIAN.NAME, this.DATA_FEMALE_CIVILIAN.PLURAL, this.DATA_FEMALE_CIVILIAN.SCORE, new DollBody(false, this.DATA_FEMALE_CIVILIAN.SPD), new Abilities()
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
      }, new ActorSheet(this.DATA_FEMALE_CIVILIAN.HP, this.DATA_FEMALE_CIVILIAN.STA, this.HUMAN_HUN, this.HUMAN_SLP, this.HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, this.DATA_FEMALE_CIVILIAN.ATK, this.DATA_FEMALE_CIVILIAN.DMG), new Defence(this.DATA_FEMALE_CIVILIAN.DEF, this.DATA_FEMALE_CIVILIAN.PRO_HIT, this.DATA_FEMALE_CIVILIAN.PRO_SHOT), this.DATA_FEMALE_CIVILIAN.FOV, this.DATA_FEMALE_CIVILIAN.AUDIO, 0, this.HUMAN_INVENTORY), (Type) null)
      {
        FlavorDescription = this.DATA_FEMALE_CIVILIAN.FLAVOR
      };
      this[GameActors.IDs.CHAR_GUARD] = new ActorModel((string) null, this.DATA_CHAR_GUARD.NAME, this.DATA_CHAR_GUARD.PLURAL, this.DATA_CHAR_GUARD.SCORE, new DollBody(true, this.DATA_CHAR_GUARD.SPD), new Abilities()
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
      }, new ActorSheet(this.DATA_CHAR_GUARD.HP, this.DATA_CHAR_GUARD.STA, this.HUMAN_HUN, this.HUMAN_SLP, this.HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, this.DATA_CHAR_GUARD.ATK, this.DATA_CHAR_GUARD.DMG), new Defence(this.DATA_CHAR_GUARD.DEF, this.DATA_CHAR_GUARD.PRO_HIT, this.DATA_CHAR_GUARD.PRO_SHOT), this.DATA_CHAR_GUARD.FOV, this.DATA_CHAR_GUARD.AUDIO, 0, this.HUMAN_INVENTORY), typeof (CHARGuardAI))
      {
        FlavorDescription = this.DATA_CHAR_GUARD.FLAVOR
      };
      this[GameActors.IDs.ARMY_NATIONAL_GUARD] = new ActorModel((string) null, this.DATA_NATGUARD.NAME, this.DATA_NATGUARD.PLURAL, this.DATA_NATGUARD.SCORE, new DollBody(true, this.DATA_NATGUARD.SPD), new Abilities()
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
      }, new ActorSheet(this.DATA_NATGUARD.HP, this.DATA_NATGUARD.STA, this.HUMAN_HUN, this.HUMAN_SLP, this.HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, this.DATA_NATGUARD.ATK, this.DATA_NATGUARD.DMG), new Defence(this.DATA_NATGUARD.DEF, this.DATA_NATGUARD.PRO_HIT, this.DATA_NATGUARD.PRO_SHOT), this.DATA_NATGUARD.FOV, this.DATA_NATGUARD.AUDIO, 0, this.HUMAN_INVENTORY), typeof (SoldierAI))
      {
        FlavorDescription = this.DATA_NATGUARD.FLAVOR
      };
      this[GameActors.IDs.BIKER_MAN] = new ActorModel((string) null, this.DATA_BIKER_MAN.NAME, this.DATA_BIKER_MAN.PLURAL, this.DATA_BIKER_MAN.SCORE, new DollBody(true, this.DATA_BIKER_MAN.SPD), new Abilities()
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
      }, new ActorSheet(this.DATA_BIKER_MAN.HP, this.DATA_BIKER_MAN.STA, this.HUMAN_HUN, this.HUMAN_SLP, this.HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, this.DATA_BIKER_MAN.ATK, this.DATA_BIKER_MAN.DMG), new Defence(this.DATA_BIKER_MAN.DEF, this.DATA_BIKER_MAN.PRO_HIT, this.DATA_BIKER_MAN.PRO_SHOT), this.DATA_BIKER_MAN.FOV, this.DATA_BIKER_MAN.AUDIO, 0, this.HUMAN_INVENTORY), typeof (GangAI))
      {
        FlavorDescription = this.DATA_BIKER_MAN.FLAVOR
      };
      this[GameActors.IDs.GANGSTA_MAN] = new ActorModel((string) null, this.DATA_GANGSTA_MAN.NAME, this.DATA_GANGSTA_MAN.PLURAL, this.DATA_GANGSTA_MAN.SCORE, new DollBody(true, this.DATA_GANGSTA_MAN.SPD), new Abilities()
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
      }, new ActorSheet(this.DATA_GANGSTA_MAN.HP, this.DATA_GANGSTA_MAN.STA, this.HUMAN_HUN, this.HUMAN_SLP, this.HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, this.DATA_GANGSTA_MAN.ATK, this.DATA_GANGSTA_MAN.DMG), new Defence(this.DATA_GANGSTA_MAN.DEF, this.DATA_GANGSTA_MAN.PRO_HIT, this.DATA_GANGSTA_MAN.PRO_SHOT), this.DATA_GANGSTA_MAN.FOV, this.DATA_GANGSTA_MAN.AUDIO, 0, this.HUMAN_INVENTORY), typeof (GangAI))
      {
        FlavorDescription = this.DATA_GANGSTA_MAN.FLAVOR
      };
      this[GameActors.IDs.POLICEMAN] = new ActorModel((string) null, this.DATA_POLICEMAN.NAME, this.DATA_POLICEMAN.PLURAL, this.DATA_POLICEMAN.SCORE, new DollBody(true, this.DATA_POLICEMAN.SPD), new Abilities()
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
      }, new ActorSheet(this.DATA_POLICEMAN.HP, this.DATA_POLICEMAN.STA, this.HUMAN_HUN, this.HUMAN_SLP, this.HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, this.DATA_POLICEMAN.ATK, this.DATA_POLICEMAN.DMG), new Defence(this.DATA_POLICEMAN.DEF, this.DATA_POLICEMAN.PRO_HIT, this.DATA_POLICEMAN.PRO_SHOT), this.DATA_POLICEMAN.FOV, this.DATA_POLICEMAN.AUDIO, 0, this.HUMAN_INVENTORY), (Type) null)
      {
        FlavorDescription = this.DATA_POLICEMAN.FLAVOR
      };
      this[GameActors.IDs.BLACKOPS_MAN] = new ActorModel((string) null, this.DATA_BLACKOPS_MAN.NAME, this.DATA_BLACKOPS_MAN.PLURAL, this.DATA_BLACKOPS_MAN.SCORE, new DollBody(true, this.DATA_BLACKOPS_MAN.SPD), new Abilities()
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
      }, new ActorSheet(this.DATA_BLACKOPS_MAN.HP, this.DATA_BLACKOPS_MAN.STA, this.HUMAN_HUN, this.HUMAN_SLP, this.HUMAN_SAN, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, this.DATA_BLACKOPS_MAN.ATK, this.DATA_BLACKOPS_MAN.DMG), new Defence(this.DATA_BLACKOPS_MAN.DEF, this.DATA_BLACKOPS_MAN.PRO_HIT, this.DATA_BLACKOPS_MAN.PRO_SHOT), this.DATA_BLACKOPS_MAN.FOV, this.DATA_BLACKOPS_MAN.AUDIO, 0, this.HUMAN_INVENTORY), typeof (SoldierAI))
      {
        FlavorDescription = this.DATA_BLACKOPS_MAN.FLAVOR
      };
      GameActors.ActorData actorData = this.DATA_FERAL_DOG;
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
      }, new ActorSheet(actorData.HP, actorData.STA, this.DOG_HUN, this.DOG_SLP, 0, new Attack(AttackKind.PHYSICAL, new Verb("bite"), actorData.ATK, actorData.DMG), new Defence(actorData.DEF, actorData.PRO_HIT, actorData.PRO_SHOT), actorData.FOV, actorData.AUDIO, actorData.SMELL, this.DOG_INVENTORY), typeof (FeralDogAI))
      {
        FlavorDescription = actorData.FLAVOR
      };
      this[GameActors.IDs.JASON_MYERS] = new ActorModel((string) null, this.DATA_JASON_MYERS.NAME, this.DATA_JASON_MYERS.PLURAL, this.DATA_JASON_MYERS.SCORE, new DollBody(true, this.DATA_JASON_MYERS.SPD), new Abilities()
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
      }, new ActorSheet(this.DATA_JASON_MYERS.HP, this.DATA_JASON_MYERS.STA, this.HUMAN_HUN, this.HUMAN_SLP, 0, new Attack(AttackKind.PHYSICAL, GameActors.VERB_PUNCH, this.DATA_JASON_MYERS.ATK, this.DATA_JASON_MYERS.DMG), new Defence(this.DATA_JASON_MYERS.DEF, this.DATA_JASON_MYERS.PRO_HIT, this.DATA_JASON_MYERS.PRO_SHOT), this.DATA_JASON_MYERS.FOV, this.DATA_JASON_MYERS.AUDIO, 0, this.HUMAN_INVENTORY), typeof (InsaneHumanAI))
      {
        FlavorDescription = this.DATA_JASON_MYERS.FLAVOR
      };
    }

    public bool LoadFromCSV(IRogueUI ui, string path)
    {
      this.Notify(ui, "loading file...");
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
      this.Notify(ui, "parsing CSV...");
      CSVTable toTable = new CSVParser().ParseToTable(stringList.ToArray(), 16);
      this.Notify(ui, "reading data...");
      this.DATA_SKELETON = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs._FIRST);
      this.DATA_RED_EYED_SKELETON = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_RED_EYED_SKELETON);
      this.DATA_RED_SKELETON = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_RED_SKELETON);
      this.DATA_ZOMBIE = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_ZOMBIE);
      this.DATA_DARK_EYED_ZOMBIE = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_DARK_EYED_ZOMBIE);
      this.DATA_DARK_ZOMBIE = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_DARK_ZOMBIE);
      this.DATA_MALE_ZOMBIFIED = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_MALE_ZOMBIFIED);
      this.DATA_FEMALE_ZOMBIFIED = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED);
      this.DATA_MALE_NEOPHYTE = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_MALE_NEOPHYTE);
      this.DATA_FEMALE_NEOPHYTE = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_FEMALE_NEOPHYTE);
      this.DATA_MALE_DISCIPLE = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_MALE_DISCIPLE);
      this.DATA_FEMALE_DISCIPLE = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_FEMALE_DISCIPLE);
      this.DATA_ZM = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_ZOMBIE_MASTER);
      this.DATA_ZL = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_ZOMBIE_LORD);
      this.DATA_ZP = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_ZOMBIE_PRINCE);
      this.DATA_RAT_ZOMBIE = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.UNDEAD_RAT_ZOMBIE);
      this.DATA_SEWERS_THING = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.SEWERS_THING);
      this.DATA_MALE_CIVILIAN = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.MALE_CIVILIAN);
      this.DATA_FEMALE_CIVILIAN = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.FEMALE_CIVILIAN);
      this.DATA_FERAL_DOG = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.FERAL_DOG);
      this.DATA_POLICEMAN = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.POLICEMAN);
      this.DATA_CHAR_GUARD = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.CHAR_GUARD);
      this.DATA_NATGUARD = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.ARMY_NATIONAL_GUARD);
      this.DATA_BIKER_MAN = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.BIKER_MAN);
      this.DATA_GANGSTA_MAN = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.GANGSTA_MAN);
      this.DATA_BLACKOPS_MAN = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.BLACKOPS_MAN);
      this.DATA_JASON_MYERS = this.GetDataFromCSVTable(ui, toTable, GameActors.IDs.JASON_MYERS);
      this.CreateModels();
      this.Notify(ui, "done!");
      return true;
    }

    private CSVLine FindLineForModel(CSVTable table, GameActors.IDs modelID)
    {
      foreach (CSVLine line in table.Lines)
      {
        if (line[0].ParseText() == modelID.ToString())
          return line;
      }
      return (CSVLine) null;
    }

    private GameActors.ActorData GetDataFromCSVTable(IRogueUI ui, CSVTable table, GameActors.IDs modelID)
    {
      CSVLine lineForModel = this.FindLineForModel(table, modelID);
      if (lineForModel == null)
        throw new InvalidOperationException(string.Format("model {0} not found", (object) modelID.ToString()));
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
      if (m != this.MaleZombified && m != this.FemaleZombified && (m != this.MaleNeophyte && m != this.FemaleNeophyte) && m != this.MaleDisciple)
        return m == this.FemaleDisciple;
      return true;
    }

    public bool IsZMBranch(ActorModel m)
    {
      if (m != this.ZombieMaster && m != this.ZombieLord)
        return m == this.ZombiePrince;
      return true;
    }

    public bool IsSkeletonBranch(ActorModel m)
    {
      if (m != this.Skeleton && m != this.Red_Eyed_Skeleton)
        return m == this.Red_Skeleton;
      return true;
    }

    public bool IsShamblerBranch(ActorModel m)
    {
      if (m != this.Zombie && m != this.DarkEyedZombie)
        return m == this.DarkZombie;
      return true;
    }

    public bool IsRatBranch(ActorModel m)
    {
      return m == this.RatZombie;
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
