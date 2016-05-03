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
    private string m_OffImageID;
    private string m_OnImageID;

    public bool IsOn
    {
      get
      {
        return State == 1;
      }
    }

    public PowerGenerator(string name, string offImageID, string onImageID)
      : base(name, offImageID)
    {
            m_OffImageID = offImageID;
            m_OnImageID = onImageID;
    }

    public override void SetState(int newState)
    {
      base.SetState(newState);
      switch (newState)
      {
        case 0:
                    ImageID = m_OffImageID;
          break;
        case 1:
                    ImageID = m_OnImageID;
          break;
        default:
          throw new ArgumentOutOfRangeException("unhandled state");
      }
    }

    public void TogglePower()
    {
            SetState(State == 0 ? 1 : 0);
    }
  }
}
