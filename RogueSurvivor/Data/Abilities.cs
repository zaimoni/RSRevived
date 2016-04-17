// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Abilities
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Abilities
  {
    [NonSerialized]
    public static readonly Abilities NONE = new Abilities();

    public bool IsUndead { get; set; }

    public bool IsUndeadMaster { get; set; }

    public bool CanZombifyKilled { get; set; }

    public bool CanTire { get; set; }

    public bool HasToEat { get; set; }

    public bool HasToSleep { get; set; }

    public bool HasSanity { get; set; }

    public bool CanRun { get; set; }

    public bool CanTalk { get; set; }

    public bool CanUseMapObjects { get; set; }

    public bool CanBashDoors { get; set; }

    public bool CanBreakObjects { get; set; }

    public bool CanJump { get; set; }

    public bool IsSmall { get; set; }

    public bool HasInventory { get; set; }

    public bool CanUseItems { get; set; }

    public bool CanTrade { get; set; }

    public bool CanBarricade { get; set; }

    public bool CanPush { get; set; }

    public bool CanJumpStumble { get; set; }

    public bool IsLawEnforcer { get; set; }

    public bool IsIntelligent { get; set; }

    public bool IsRotting { get; set; }

    public bool AI_CanUseAIExits { get; set; }

    public bool AI_NotInterestedInRangedWeapons { get; set; }

    public bool ZombieAI_AssaultBreakables { get; set; }

    public bool ZombieAI_Explore { get; set; }
  }
}
