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
    private bool m_IsActivated;
    private bool m_IsTriggered;

    public bool IsActivated
    {
      get
      {
        return m_IsActivated;
      }
      set
      {
                m_IsActivated = value;
      }
    }

    public bool IsTriggered
    {
      get
      {
        return m_IsTriggered;
      }
      set
      {
                m_IsTriggered = value;
      }
    }

    public ItemTrapModel TrapModel
    {
      get
      {
        return Model as ItemTrapModel;
      }
    }

    public ItemTrap(ItemModel model)
      : base(model)
    {
      Contract.Requires(model is ItemTrapModel);
    }

    public ItemTrap Clone()
    {
      return new ItemTrap((ItemModel)TrapModel);
    }
  }
}
