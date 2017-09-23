﻿// Decompiled with JetBrains decompiler
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
    public const int BASE_HITPOINTS = 40;   // XXX spacetime scaling candidate
    public const int STATE_CLOSED = 0;
    public const int STATE_OPEN = 1;
    public const int STATE_BROKEN = 2;
    private const int MAX_STATE = 3;

    public enum DW_type : byte {
      WOODEN = 0,
      HOSPITAL,
      CHAR,
      GLASS,
      IRON,
      WINDOW,
      MAX
    };

    // don't fold maxhp into here just yet.
    // if we end up allowing constructing wooden doors w/carpentry, perhaps 
    // post-apocalypse doors shouldn't be as durable as pre-apocalypse
    // or maybe other choices available in this regard
    static string[] names = new string[(int)DW_type.MAX]{
      "wooden door",
      "door",
      "CHAR door",
      "glass door",
      "iron door",
      "window"
    };

    static string[][] images = new string[(int)DW_type.MAX][]{
       new string[MAX_STATE]{ Gameplay.GameImages.OBJ_WOODEN_DOOR_CLOSED, Gameplay.GameImages.OBJ_WOODEN_DOOR_OPEN, Gameplay.GameImages.OBJ_WOODEN_DOOR_BROKEN},
       new string[MAX_STATE]{ Gameplay.GameImages.OBJ_HOSPITAL_DOOR_CLOSED, Gameplay.GameImages.OBJ_HOSPITAL_DOOR_OPEN, Gameplay.GameImages.OBJ_HOSPITAL_DOOR_BROKEN},
       new string[MAX_STATE]{ Gameplay.GameImages.OBJ_CHAR_DOOR_CLOSED, Gameplay.GameImages.OBJ_CHAR_DOOR_OPEN, Gameplay.GameImages.OBJ_CHAR_DOOR_BROKEN},
       new string[MAX_STATE]{ Gameplay.GameImages.OBJ_GLASS_DOOR_CLOSED, Gameplay.GameImages.OBJ_GLASS_DOOR_OPEN, Gameplay.GameImages.OBJ_GLASS_DOOR_BROKEN},
       new string[MAX_STATE]{ Gameplay.GameImages.OBJ_IRON_DOOR_CLOSED, Gameplay.GameImages.OBJ_IRON_DOOR_OPEN, Gameplay.GameImages.OBJ_IRON_DOOR_BROKEN},
       new string[MAX_STATE]{ Gameplay.GameImages.OBJ_WINDOW_CLOSED, Gameplay.GameImages.OBJ_WINDOW_OPEN, Gameplay.GameImages.OBJ_WINDOW_BROKEN }
    };

    private readonly byte m_type;
    private int m_BarricadePoints = 0;

    public bool IsOpen { get { return State == STATE_OPEN; } }
    public bool IsClosed { get { return State == STATE_CLOSED; } }
    public bool IsBroken { get { return State == STATE_BROKEN; } }

    public override bool IsTransparent
    {
      get {
        if (m_BarricadePoints > 0) return false;
        if (State != STATE_OPEN) return base.IsTransparent;
        return FireState != Fire.ONFIRE;
      }
    }

    public bool IsWindow { get { return m_type==(byte)DW_type.WINDOW; } }

    public int BarricadePoints {
      get {
        return m_BarricadePoints;
      }
      private set {
        if (value > 0 && m_BarricadePoints <= 0) {
          --JumpLevel;
          IsWalkable = false;
        }
        else if (value <= 0 && m_BarricadePoints > 0)
          SetState(State);
        if (0>value) value = 0;
        if (Rules.BARRICADING_MAX < value) value = Rules.BARRICADING_MAX;
        m_BarricadePoints = value;
      }
    }

    public bool IsBarricaded { get { return m_BarricadePoints > 0; } }

    public DoorWindow(DW_type _type, int hitPoints)
      : base(names[(int)(_type)], images[(int)(_type)][STATE_CLOSED], MapObject.Break.BREAKABLE, hitPoints, MapObject.Fire.BURNABLE)
    {
      m_type = (byte)_type;
      _SetState(STATE_CLOSED);
      switch(m_type)
      {
      case (byte)DW_type.WOODEN:
      case (byte)DW_type.HOSPITAL:
        GivesWood = true;
        break;
      case (byte)DW_type.CHAR:
      case (byte)DW_type.IRON:
        break;
      case (byte)DW_type.GLASS:
        IsMaterialTransparent = true;
        BreaksWhenFiredThrough = true;
        break;
      case (byte)DW_type.WINDOW:
        IsMaterialTransparent = true;
        GivesWood = true;
        BreaksWhenFiredThrough = true;
        break;
      }
    }

    public void Barricade(int delta)
    {
      int old = BarricadePoints;
      BarricadePoints += delta;
      if ((0 < old)!=(0 < BarricadePoints)) InvalidateLOS();
    }

    private string ReasonCantBarricade()
    {
      if (!IsClosed && !IsBroken) return "not closed or broken";
      if (BarricadePoints >= Rules.BARRICADING_MAX) return "barricade limit reached";
      if (Location.Actor != null) return "someone is there";
      return "";
    }

    public bool CanBarricade(out string reason)
    {
      reason = ReasonCantBarricade();
      return string.IsNullOrEmpty(reason);
    }

    public bool CanBarricade()
    {
      return string.IsNullOrEmpty(ReasonCantBarricade());
    }

    override protected string StateToID(int x)
    {
#if DEBUG
      if (0>x || MAX_STATE<= x) throw new ArgumentOutOfRangeException("newState unhandled");
#endif
      return images[m_type][x];
    }

    private void _SetState(int newState)
    { // cf IsTransparent
      if ((STATE_OPEN==State)!=(STATE_OPEN==newState)) InvalidateLOS();
      base.SetState(newState);
      IsWalkable = (State!= STATE_CLOSED);
      if (STATE_BROKEN == State) { 
          BreakState = Break.BROKEN;
          HitPoints = 0;
          m_BarricadePoints = 0;
      }
    }

    public override void SetState(int newState) {
      _SetState(newState);
    }
  }
}
