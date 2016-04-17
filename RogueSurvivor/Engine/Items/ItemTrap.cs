// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemTrap
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

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
        return this.m_IsActivated;
      }
      set
      {
        this.m_IsActivated = value;
      }
    }

    public bool IsTriggered
    {
      get
      {
        return this.m_IsTriggered;
      }
      set
      {
        this.m_IsTriggered = value;
      }
    }

    public ItemTrapModel TrapModel
    {
      get
      {
        return this.Model as ItemTrapModel;
      }
    }

    public ItemTrap(ItemModel model)
      : base(model)
    {
      if (!(model is ItemTrapModel))
        throw new ArgumentException("model is not a TrapModel");
    }

    public ItemTrap Clone()
    {
      return new ItemTrap((ItemModel) this.TrapModel);
    }
  }
}
