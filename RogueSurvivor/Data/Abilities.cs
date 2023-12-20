// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Abilities
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  public class Abilities
  {
    private readonly Flags m_Flags;

    public bool IsUndead { get => default != (m_Flags & Flags.UNDEAD); }
    public bool IsUndeadMaster { get => default != (m_Flags & Flags.UNDEAD_MASTER); }
    public bool CanZombifyKilled  { get => default != (m_Flags & Flags.CAN_ZOMBIFY_KILLED); }
    public bool CanTire { get => default != (m_Flags & Flags.CAN_TIRE); }
    public bool HasToEat { get => default != (m_Flags & Flags.HAS_TO_EAT); }
    public bool HasToSleep { get => default != (m_Flags & Flags.HAS_TO_SLEEP); }
    public bool HasSanity { get => default != (m_Flags & Flags.HAS_SANITY); }
    public bool CanRun { get => default != (m_Flags & Flags.CAN_RUN); }
    public bool CanTalk { get => default != (m_Flags & Flags.CAN_TALK); }
    public bool CanUseMapObjects { get => default != (m_Flags & Flags.CAN_USE_MAP_OBJECTS); }
    public bool CanBashDoors { get => default != (m_Flags & Flags.CAN_BASH_DOORS); }
    public bool CanBreakObjects { get => default != (m_Flags & Flags.CAN_BREAK_OBJECTS); }
    public bool CanJump { get => default != (m_Flags & Flags.CAN_JUMP); }
    public bool IsSmall { get => default != (m_Flags & Flags.IS_SMALL); }
    public bool HasInventory { get => default != (m_Flags & Flags.HAS_INVENTORY); }
    public bool CanUseItems { get => default != (m_Flags & Flags.CAN_USE_ITEMS); }
    public bool CanTrade { get => default != (m_Flags & Flags.CAN_TRADE); }
    public bool CanBarricade { get => default != (m_Flags & Flags.CAN_BARRICADE); }
    public bool CanPush { get => default != (m_Flags & Flags.CAN_PUSH); }
    public bool CanJumpStumble { get => default != (m_Flags & Flags.CAN_JUMP_STUMBLE); }
    public bool IsLawEnforcer { get => default != (m_Flags & Flags.IS_LAW_ENFORCER); }
    public bool IsIntelligent { get => default != (m_Flags & Flags.IS_INTELLIGENT); }
    public bool IsRotting { get => default != (m_Flags & Flags.IS_ROTTING); }
    public bool CanDisarm { get => false; } // mock up alpha10
    public bool AI_CanUseAIExits { get => default != (m_Flags & Flags.AI_CAN_USE_AI_EXITS); }
    public bool AI_NotInterestedInRangedWeapons { get => default != (m_Flags & Flags.AI_NOT_INTERESTED_IN_RANGED_WEAPONS); }
    public bool ZombieAI_Explore { get => default != (m_Flags & Flags.ZOMBIEAI_EXPLORE); }

    public Abilities(Flags in_flags)
    {
#if DEBUG
      if ((default == (in_flags & Flags.HAS_INVENTORY)) && (default != (in_flags & Flags.CAN_TRADE))) throw new InvalidOperationException("must have inventory to trade");
      if ((default == (in_flags & Flags.UNDEAD)) && (default != (in_flags & Flags.UNDEAD_MASTER))) throw new InvalidOperationException("undead master is undead");
      if ((default == (in_flags & Flags.CAN_JUMP)) && (default != (in_flags & Flags.CAN_JUMP_STUMBLE))) throw new InvalidOperationException("can only clumsily jump if can jump");
      if ((default == (in_flags & Flags.CAN_USE_MAP_OBJECTS)) && (default != (in_flags & Flags.CAN_BARRICADE))) throw new InvalidOperationException("hard to barricade without using map objects");
#endif
      m_Flags = in_flags;
    }

    [System.Flags]
    public enum Flags
    {
      UNDEAD = 1,
      UNDEAD_MASTER = 1 << 1,
      CAN_ZOMBIFY_KILLED = 1 << 2,
      CAN_TIRE = 1 << 3,
      HAS_TO_EAT = 1 << 4,
      HAS_TO_SLEEP = 1 << 5,
      HAS_SANITY = 1 << 6,
      CAN_RUN = 1 << 7,
      CAN_TALK = 1 << 8,
      CAN_USE_MAP_OBJECTS = 1 << 9,
      CAN_BASH_DOORS = 1 << 10,
      CAN_BREAK_OBJECTS = 1 << 11,
      CAN_JUMP = 1 << 12,
      IS_SMALL = 1 << 13,
      HAS_INVENTORY = 1 << 14,
      CAN_USE_ITEMS = 1 << 15,
      CAN_TRADE = 1 << 16,  // XXX requires HAS_INVENTORY
      CAN_BARRICADE = 1 << 17,
      CAN_PUSH = 1 << 18,
      CAN_JUMP_STUMBLE = 1 << 19,
      IS_LAW_ENFORCER = 1 << 20,
      IS_INTELLIGENT = 1 << 21,
      IS_ROTTING = 1 << 22,         // XXX very similar to HAS_TO_EAT
      AI_CAN_USE_AI_EXITS = 1 << 23,
      AI_NOT_INTERESTED_IN_RANGED_WEAPONS = 1 << 24,
//    ZOMBIEAI_ASSAULT_BREAKABLES = 1 << 25,
      ZOMBIEAI_EXPLORE = 1 << 26,
    }
  }
}
