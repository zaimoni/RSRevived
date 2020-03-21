// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemTrapModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemTrapModel : ItemModel
  {
    private readonly Flags m_Flags = Flags.NONE;
    public readonly int TriggerChance;
    public readonly int Damage;
    public readonly int BreakChance;
    public readonly int BreakChanceWhenEscape;
    public readonly int BlockChance;
    public readonly string NoiseName;

    public bool UseToActivate { get { return (m_Flags & Flags.USE_TO_ACTIVATE) != Flags.NONE; } }
    public bool IsNoisy { get { return (m_Flags & Flags.IS_NOISY) != Flags.NONE; } }
    public bool IsOneTimeUse { get { return (m_Flags & Flags.IS_ONE_TIME_USE) != Flags.NONE; } }
    public bool IsFlammable { get { return (m_Flags & Flags.IS_FLAMMABLE) != Flags.NONE; } }
    public bool ActivatesWhenDropped { get { return (m_Flags & Flags.DROP_ACTIVATE) != Flags.NONE; } }

    public ItemTrapModel(Gameplay.GameItems.IDs _id, string aName, string theNames, string imageID, int stackLimit, int triggerChance, int damage, bool dropActivate, bool useToActivate, bool IsOneTimeUse, int breakChance, int blockChance, int breakChanceWhenEscape, bool IsNoisy, string noiseName, bool IsFlammable, string flavor)
      : base(_id, aName, theNames, imageID, flavor)
    {
      if (stackLimit > 1) StackingLimit = stackLimit;
      TriggerChance = triggerChance;
      Damage = damage;
      BreakChance = breakChance;
      BlockChance = blockChance;
      BreakChanceWhenEscape = breakChanceWhenEscape;
      if (dropActivate) m_Flags |= Flags.DROP_ACTIVATE;
      if (useToActivate) m_Flags |= Flags.USE_TO_ACTIVATE;
      if (IsNoisy) {
        m_Flags |= Flags.IS_NOISY;
        NoiseName = noiseName;
      }
      if (IsOneTimeUse) m_Flags |= Flags.IS_ONE_TIME_USE;
      if (IsFlammable) m_Flags |= Flags.IS_FLAMMABLE;
    }

    public override Item create() { return new ItemTrap(this); }
    public ItemTrap instantiate() { return new ItemTrap(this); }

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
