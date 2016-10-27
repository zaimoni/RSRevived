// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemTrapModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemTrapModel : ItemModel
  {
    private readonly ItemTrapModel.Flags m_Flags;
    private readonly int m_TriggerChance;
    private readonly int m_Damage;
    private readonly int m_BreakChance;
    private readonly int m_BreakChanceWhenEscape;
    private readonly int m_BlockChance;
    private readonly string m_NoiseName;

    public int TriggerChance {
      get {
        return m_TriggerChance;
      }
    }

    public int Damage {
      get {
        return m_Damage;
      }
    }

    public bool UseToActivate {
      get {
        return (m_Flags & ItemTrapModel.Flags.USE_TO_ACTIVATE) != ItemTrapModel.Flags.NONE;
      }
    }

    public bool IsNoisy {
      get {
        return (m_Flags & ItemTrapModel.Flags.IS_NOISY) != ItemTrapModel.Flags.NONE;
      }
    }

    public bool IsOneTimeUse {
      get {
        return (m_Flags & ItemTrapModel.Flags.IS_ONE_TIME_USE) != ItemTrapModel.Flags.NONE;
      }
    }

    public bool IsFlammable {
      get {
        return (m_Flags & ItemTrapModel.Flags.IS_FLAMMABLE) != ItemTrapModel.Flags.NONE;
      }
    }

    public bool ActivatesWhenDropped {
      get {
        return (m_Flags & ItemTrapModel.Flags.DROP_ACTIVATE) != ItemTrapModel.Flags.NONE;
      }
    }

    public int BreakChance {
      get {
        return m_BreakChance;
      }
    }

    public int BlockChance {
      get {
        return m_BlockChance;
      }
    }

    public int BreakChanceWhenEscape {
      get {
        return m_BreakChanceWhenEscape;
      }
    }

    public string NoiseName {
      get {
        return m_NoiseName;
      }
    }

    public ItemTrapModel(string aName, string theNames, string imageID, int stackLimit, int triggerChance, int damage, bool dropActivate, bool useToActivate, bool IsOneTimeUse, int breakChance, int blockChance, int breakChanceWhenEscape, bool IsNoisy, string noiseName, bool IsFlammable, string flavor)
      : base(aName, theNames, imageID)
    {
      DontAutoEquip = true;
      if (stackLimit > 1) StackingLimit = stackLimit;
      m_TriggerChance = triggerChance;
      m_Damage = damage;
      m_BreakChance = breakChance;
      m_BlockChance = blockChance;
      m_BreakChanceWhenEscape = breakChanceWhenEscape;
      FlavorDescription = flavor;
      m_Flags = ItemTrapModel.Flags.NONE;
      if (dropActivate) m_Flags |= ItemTrapModel.Flags.DROP_ACTIVATE;
      if (useToActivate) m_Flags |= ItemTrapModel.Flags.USE_TO_ACTIVATE;
      if (IsNoisy) {
        m_Flags |= ItemTrapModel.Flags.IS_NOISY;
        m_NoiseName = noiseName;
      }
      if (IsOneTimeUse) m_Flags |= ItemTrapModel.Flags.IS_ONE_TIME_USE;
      if (IsFlammable) m_Flags |= ItemTrapModel.Flags.IS_FLAMMABLE;
    }

    [System.Flags]
    private enum Flags
    {
      NONE = 0,
      USE_TO_ACTIVATE = 1,
      IS_NOISY = 2,
      IS_ONE_TIME_USE = 4,
      IS_FLAMMABLE = 8,
      DROP_ACTIVATE = 16,
    }
  }
}
