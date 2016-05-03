// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.MapObjects.DoorWindow
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.MapObjects
{
  [Serializable]
  internal class DoorWindow : StateMapObject
  {
    public const int BASE_HITPOINTS = 40;
    public const int STATE_CLOSED = 1;
    public const int STATE_OPEN = 2;
    public const int STATE_BROKEN = 3;
    private string m_ClosedImageID;
    private string m_OpenImageID;
    private string m_BrokenImageID;
    private bool m_IsWindow;
    private int m_BarricadePoints;

    public bool IsOpen
    {
      get
      {
        return State == 2;
      }
    }

    public bool IsClosed
    {
      get
      {
        return State == 1;
      }
    }

    public bool IsBroken
    {
      get
      {
        return State == 3;
      }
    }

    public override bool IsTransparent
    {
      get
      {
        if (m_BarricadePoints > 0)
          return false;
        if (State != 2)
          return base.IsTransparent;
        return FireState != MapObject.Fire.ONFIRE;
      }
    }

    public bool IsWindow
    {
      get
      {
        return m_IsWindow;
      }
      set
      {
                m_IsWindow = value;
      }
    }

    public int BarricadePoints
    {
      get
      {
        return m_BarricadePoints;
      }
      set
      {
        if (value > 0 && m_BarricadePoints <= 0)
        {
          --JumpLevel;
                    IsWalkable = false;
        }
        else if (value <= 0 && m_BarricadePoints > 0)
                    SetState(State);
                m_BarricadePoints = value;
        if (m_BarricadePoints >= 0)
          return;
                m_BarricadePoints = 0;
      }
    }

    public bool IsBarricaded
    {
      get
      {
        return m_BarricadePoints > 0;
      }
    }

    public DoorWindow(string name, string closedImageID, string openImageID, string brokenImageID, int hitPoints)
      : base(name, closedImageID, MapObject.Break.BREAKABLE, MapObject.Fire.BURNABLE, hitPoints)
    {
            m_ClosedImageID = closedImageID;
            m_OpenImageID = openImageID;
            m_BrokenImageID = brokenImageID;
            m_BarricadePoints = 0;
            SetState(1);
    }

    public override void SetState(int newState)
    {
      switch (newState)
      {
        case 1:
                    ImageID = m_ClosedImageID;
                    IsWalkable = false;
          break;
        case 2:
                    ImageID = m_OpenImageID;
                    IsWalkable = true;
          break;
        case 3:
                    ImageID = m_BrokenImageID;
                    BreakState = MapObject.Break.BROKEN;
                    HitPoints = 0;
                    m_BarricadePoints = 0;
                    IsWalkable = true;
          break;
        default:
          throw new ArgumentOutOfRangeException("newState unhandled");
      }
      base.SetState(newState);
    }
  }
}
