// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.StateMapObject
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  abstract class StateMapObject : MapObject
  {
    protected int m_State;

    public int State { get { return m_State; } }
    abstract protected string StateToID(int x); // XXX intent is to validate before actually updating the state
    public override string ImageID {
      get {
        return StateToID(m_State);
      }
    }

    protected StateMapObject(string hiddenImageID)
      : base(hiddenImageID)
    {
    }

    protected StateMapObject(string hiddenImageID, int hitPoints, Fire burnable)
      : base(hiddenImageID, hitPoints, burnable)
    {
    }

    public virtual void SetState(int newState)
    {
       m_State = newState;
    }
  }
}
