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
        return this.State == 2;
      }
    }

    public bool IsClosed
    {
      get
      {
        return this.State == 1;
      }
    }

    public bool IsBroken
    {
      get
      {
        return this.State == 3;
      }
    }

    public override bool IsTransparent
    {
      get
      {
        if (this.m_BarricadePoints > 0)
          return false;
        if (this.State != 2)
          return base.IsTransparent;
        return this.FireState != MapObject.Fire.ONFIRE;
      }
    }

    public bool IsWindow
    {
      get
      {
        return this.m_IsWindow;
      }
      set
      {
        this.m_IsWindow = value;
      }
    }

    public int BarricadePoints
    {
      get
      {
        return this.m_BarricadePoints;
      }
      set
      {
        if (value > 0 && this.m_BarricadePoints <= 0)
        {
          --this.JumpLevel;
          this.IsWalkable = false;
        }
        else if (value <= 0 && this.m_BarricadePoints > 0)
          this.SetState(this.State);
        this.m_BarricadePoints = value;
        if (this.m_BarricadePoints >= 0)
          return;
        this.m_BarricadePoints = 0;
      }
    }

    public bool IsBarricaded
    {
      get
      {
        return this.m_BarricadePoints > 0;
      }
    }

    public DoorWindow(string name, string closedImageID, string openImageID, string brokenImageID, int hitPoints)
      : base(name, closedImageID, MapObject.Break.BREAKABLE, MapObject.Fire.BURNABLE, hitPoints)
    {
      this.m_ClosedImageID = closedImageID;
      this.m_OpenImageID = openImageID;
      this.m_BrokenImageID = brokenImageID;
      this.m_BarricadePoints = 0;
      this.SetState(1);
    }

    public override void SetState(int newState)
    {
      switch (newState)
      {
        case 1:
          this.ImageID = this.m_ClosedImageID;
          this.IsWalkable = false;
          break;
        case 2:
          this.ImageID = this.m_OpenImageID;
          this.IsWalkable = true;
          break;
        case 3:
          this.ImageID = this.m_BrokenImageID;
          this.BreakState = MapObject.Break.BROKEN;
          this.HitPoints = 0;
          this.m_BarricadePoints = 0;
          this.IsWalkable = true;
          break;
        default:
          throw new ArgumentOutOfRangeException("newState unhandled");
      }
      base.SetState(newState);
    }
  }
}
