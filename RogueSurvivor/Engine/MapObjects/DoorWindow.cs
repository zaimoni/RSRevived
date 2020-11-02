// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.MapObjects.DoorWindow
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Threading;

#nullable enable

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

    // VAPORWARE: locked doors
    // physical locks require a corresponding key, and may fail-closed on breaking
    // power locks may either fail-closed (subway, technically but that's handled as gates) or fail-open (police station)
    // we want concealed inventory before police door power locks.

    public enum DW_type : byte {
      WOODEN = 0,
      HOSPITAL,
      CHAR,
      GLASS,
      IRON,
      WINDOW,
      MAX
    }

    static readonly string[][] images = new string[(int)DW_type.MAX][]{
       new string[MAX_STATE]{ Gameplay.GameImages.OBJ_WOODEN_DOOR_CLOSED, Gameplay.GameImages.OBJ_WOODEN_DOOR_OPEN, Gameplay.GameImages.OBJ_WOODEN_DOOR_BROKEN},
       new string[MAX_STATE]{ Gameplay.GameImages.OBJ_HOSPITAL_DOOR_CLOSED, Gameplay.GameImages.OBJ_HOSPITAL_DOOR_OPEN, Gameplay.GameImages.OBJ_HOSPITAL_DOOR_BROKEN},
       new string[MAX_STATE]{ Gameplay.GameImages.OBJ_CHAR_DOOR_CLOSED, Gameplay.GameImages.OBJ_CHAR_DOOR_OPEN, Gameplay.GameImages.OBJ_CHAR_DOOR_BROKEN},
       new string[MAX_STATE]{ Gameplay.GameImages.OBJ_GLASS_DOOR_CLOSED, Gameplay.GameImages.OBJ_GLASS_DOOR_OPEN, Gameplay.GameImages.OBJ_GLASS_DOOR_BROKEN},
       new string[MAX_STATE]{ Gameplay.GameImages.OBJ_IRON_DOOR_CLOSED, Gameplay.GameImages.OBJ_IRON_DOOR_OPEN, Gameplay.GameImages.OBJ_IRON_DOOR_BROKEN},
       new string[MAX_STATE]{ Gameplay.GameImages.OBJ_WINDOW_CLOSED, Gameplay.GameImages.OBJ_WINDOW_OPEN, Gameplay.GameImages.OBJ_WINDOW_BROKEN }
    };

    private readonly byte m_type;
    private int m_BarricadePoints;

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

    public override bool IsWalkable {
      get {
        return base.IsWalkable && State != STATE_CLOSED && 0 >= m_BarricadePoints;
      }
    }

    public override bool CoversTraps { get { return false; } }
    public override bool TriggersTraps { get { return false; } }

    public bool IsWindow { get { return m_type==(byte)DW_type.WINDOW; } }
    public int BarricadePoints { get { return m_BarricadePoints; } }
    public bool IsBarricaded { get { return m_BarricadePoints > 0; } }

    public DoorWindow(DW_type _type, int hitPoints)
      : base(images[(int)(_type)][STATE_CLOSED], hitPoints, MapObject.Fire.BURNABLE)
    {
      m_type = (byte)_type;
      _SetState(STATE_CLOSED);
    }

    public void Barricade(int delta)
    {
      int old = m_BarricadePoints;
      if (-old > delta) delta = - old;
      else if (Rules.BARRICADING_MAX-old < delta) delta = Rules.BARRICADING_MAX-old;
      if (0 != delta) {
        if ((0 < old)!=(0 < Interlocked.Add(ref m_BarricadePoints,delta))) InvalidateLOS();
      }
    }

    private string ReasonCantBarricade()
    {
      if (!IsClosed && !IsBroken) return "not closed or broken";
      if (BarricadePoints >= Rules.BARRICADING_MAX) return "barricade limit reached";
      if (Location.StrictHasActorAt) return "someone is there";
      return "";
    }

    public bool CanBarricade(out string reason)
    {
      reason = ReasonCantBarricade();
      return string.IsNullOrEmpty(reason);
    }

    public bool CanBarricade() { return string.IsNullOrEmpty(ReasonCantBarricade()); }

    override protected string StateToID(int x)
    {
#if DEBUG
      if (0>x || MAX_STATE<= x) throw new ArgumentOutOfRangeException("newState unhandled");
#endif
      return images[m_type][x];
    }

    private void _SetState(int newState)
    { // cf IsTransparent
      var old_vis = IsTransparent;
      base.SetState(newState);
      if (STATE_BROKEN == State) {
          _break();
          m_BarricadePoints = 0;
      }
      if (old_vis != IsTransparent) InvalidateLOS();
    }

    public override void SetState(int newState) { _SetState(newState); }

    protected override void _destroy()
    {
      if (IsWindow) SetState(STATE_BROKEN);
      else base._destroy();
    }
  }
}
