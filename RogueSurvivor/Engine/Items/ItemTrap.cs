// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemTrap
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemTrap : Item
  {
    new public ItemTrapModel Model { get { return base.Model as ItemTrapModel; } }

    private bool m_IsActivated;
    private bool m_IsTriggered;

    // unclear whether current game logic allows a trap to be both activated and triggered at once.
    // leave getter/setter overhead in place in case these should be mutually exclusive.
    public bool IsActivated {
      get {
        return m_IsActivated;
      }
      set {
        m_IsActivated = value;
      }
    }

    public bool IsTriggered {
      get {
        return m_IsTriggered;
      }
      set {
        m_IsTriggered = value;
      }
    }

    public ItemTrap(ItemTrapModel model)
      : base(model)
    {
    }

    public ItemTrap Clone()
    {
      return new ItemTrap(Model);
    }
  }
}
