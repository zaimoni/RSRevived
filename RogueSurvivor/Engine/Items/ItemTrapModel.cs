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
    private ItemTrapModel.Flags m_Flags;
    private int m_TriggerChance;
    private int m_Damage;
    private int m_BreakChance;
    private int m_BreakChanceWhenEscape;
    private int m_BlockChance;
    private string m_NoiseName;

    public int TriggerChance
    {
      get
      {
        return this.m_TriggerChance;
      }
    }

    public int Damage
    {
      get
      {
        return this.m_Damage;
      }
    }

    public bool UseToActivate
    {
      get
      {
        return (this.m_Flags & ItemTrapModel.Flags.USE_TO_ACTIVATE) != ItemTrapModel.Flags.NONE;
      }
    }

    public bool IsNoisy
    {
      get
      {
        return (this.m_Flags & ItemTrapModel.Flags.IS_NOISY) != ItemTrapModel.Flags.NONE;
      }
    }

    public bool IsOneTimeUse
    {
      get
      {
        return (this.m_Flags & ItemTrapModel.Flags.IS_ONE_TIME_USE) != ItemTrapModel.Flags.NONE;
      }
    }

    public bool IsFlammable
    {
      get
      {
        return (this.m_Flags & ItemTrapModel.Flags.IS_FLAMMABLE) != ItemTrapModel.Flags.NONE;
      }
    }

    public bool ActivatesWhenDropped
    {
      get
      {
        return (this.m_Flags & ItemTrapModel.Flags.DROP_ACTIVATE) != ItemTrapModel.Flags.NONE;
      }
    }

    public int BreakChance
    {
      get
      {
        return this.m_BreakChance;
      }
    }

    public int BlockChance
    {
      get
      {
        return this.m_BlockChance;
      }
    }

    public int BreakChanceWhenEscape
    {
      get
      {
        return this.m_BreakChanceWhenEscape;
      }
    }

    public string NoiseName
    {
      get
      {
        return this.m_NoiseName;
      }
    }

    public ItemTrapModel(string aName, string theNames, string imageID, int stackLimit, int triggerChance, int damage, bool dropActivate, bool useToActivate, bool IsOneTimeUse, int breakChance, int blockChance, int breakChanceWhenEscape, bool IsNoisy, string noiseName, bool IsFlammable)
      : base(aName, theNames, imageID)
    {
      this.DontAutoEquip = true;
      if (stackLimit > 1)
      {
        this.IsStackable = true;
        this.StackingLimit = stackLimit;
      }
      this.m_TriggerChance = triggerChance;
      this.m_Damage = damage;
      this.m_BreakChance = breakChance;
      this.m_BlockChance = blockChance;
      this.m_BreakChanceWhenEscape = breakChanceWhenEscape;
      this.m_Flags = ItemTrapModel.Flags.NONE;
      if (dropActivate)
        this.m_Flags |= ItemTrapModel.Flags.DROP_ACTIVATE;
      if (useToActivate)
        this.m_Flags |= ItemTrapModel.Flags.USE_TO_ACTIVATE;
      if (IsNoisy)
      {
        this.m_Flags |= ItemTrapModel.Flags.IS_NOISY;
        this.m_NoiseName = noiseName;
      }
      if (IsOneTimeUse)
        this.m_Flags |= ItemTrapModel.Flags.IS_ONE_TIME_USE;
      if (!IsFlammable)
        return;
      this.m_Flags |= ItemTrapModel.Flags.IS_FLAMMABLE;
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
