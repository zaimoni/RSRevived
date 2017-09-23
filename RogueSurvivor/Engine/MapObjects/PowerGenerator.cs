// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.MapObjects.PowerGenerator
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.MapObjects
{
  [Serializable]
  internal class PowerGenerator : StateMapObject
  {
    public const int STATE_OFF = 0;
    public const int STATE_ON = 1;
    private readonly string m_OffImageID;
    private readonly string m_OnImageID;

    public bool IsOn { get { return State == STATE_ON; } }

    // While there is only one kind of power generator currently, the graphics
    // should be isolated from the constructor "just in case".
    public PowerGenerator(string name, string offImageID, string onImageID)
      : base(name, offImageID)
    {
      m_OffImageID = offImageID;
      m_OnImageID = onImageID;
    }

    override protected string StateToID(int x)
    {
      switch(x)
      {
      case STATE_OFF: return m_OffImageID;
      case STATE_ON: return m_OnImageID;
      default: throw new ArgumentOutOfRangeException("newState unhandled");
      }
    }

    public void TogglePower()
    {
      SetState(State == STATE_OFF ? STATE_ON : STATE_OFF);
    }
  }
}
