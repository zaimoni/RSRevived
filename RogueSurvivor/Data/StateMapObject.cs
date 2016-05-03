// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.StateMapObject
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class StateMapObject : MapObject
  {
    private int m_State;

    public int State
    {
      get
      {
        return m_State;
      }
    }

    public StateMapObject(string name, string hiddenImageID)
      : base(name, hiddenImageID)
    {
    }

    public StateMapObject(string name, string hiddenImageID, MapObject.Break breakable, MapObject.Fire burnable, int hitPoints)
      : base(name, hiddenImageID, breakable, burnable, hitPoints)
    {
    }

    public virtual void SetState(int newState)
    {
            m_State = newState;
    }
  }
}
