// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.MapObject
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Diagnostics.Contracts;
using Point = System.Drawing.Point;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class MapObject
  {
    private string m_ImageID;
    private readonly string m_HiddenImageID;
    private readonly string m_Name;
    private MapObject.Flags m_Flags;
    private int m_JumpLevel;
    private int m_Weight;
    private MapObject.Break m_BreakState;
    private readonly int m_MaxHitPoints;
    private int m_HitPoints;
    private MapObject.Fire m_FireState;
    private Location m_Location;

    public string AName {
      get {
        return (IsAn ? "an " : (IsPlural ? "some " : "a ")) + m_Name;
      }
    }

    public string TheName {
      get {
        return "the " + m_Name;
      }
    }

    public bool IsAn {
      get {
        return GetFlag(MapObject.Flags.IS_AN);
      }
      set {
        SetFlag(MapObject.Flags.IS_AN, value);
      }
    }

    public bool IsPlural {
      get {
        return GetFlag(MapObject.Flags.IS_PLURAL);
      }
      set {
        SetFlag(MapObject.Flags.IS_PLURAL, value);
      }
    }

    public string ImageID {
      get {
        return m_ImageID;
      }
      set {
        m_ImageID = value;
      }
    }

    public string HiddenImageID {
      get {
        return m_HiddenImageID;
      }
    }

    public Location Location {
      get {
        return m_Location;
      }
      set {
        InvalidateLOS();
        m_Location = value;
        InvalidateLOS();
      }
    }

    public virtual bool IsTransparent {
      get {
        if (Fire.ONFIRE == m_FireState) return false;
        if (m_BreakState == Break.BROKEN || m_FireState == Fire.ASHES) return true;
        return GetFlag(Flags.IS_MATERIAL_TRANSPARENT);
      }
    }

    public bool IsMaterialTransparent {
      get {
        return GetFlag(MapObject.Flags.IS_MATERIAL_TRANSPARENT);
      }
      set {
        SetFlag(MapObject.Flags.IS_MATERIAL_TRANSPARENT, value);
      }
    }

    public bool IsWalkable {
      get {
        return GetFlag(MapObject.Flags.IS_WALKABLE);
      }
      set {
        SetFlag(MapObject.Flags.IS_WALKABLE, value);
      }
    }

    public int JumpLevel {
      get {
        return m_JumpLevel;
      }
      set {
        m_JumpLevel = value;
      }
    }

    public bool IsJumpable {
      get {
        return m_JumpLevel > 0;
      }
    }

    public bool IsContainer {
      get {
        return GetFlag(MapObject.Flags.IS_CONTAINER);
      }
      set {
        SetFlag(MapObject.Flags.IS_CONTAINER, value);
      }
    }

    public bool IsCouch {
      get {
        return GetFlag(MapObject.Flags.IS_COUCH);
      }
      set {
        SetFlag(MapObject.Flags.IS_COUCH, value);
      }
    }

    public bool IsBreakable {
      get {
        return m_BreakState == MapObject.Break.BREAKABLE;
      }
    }

    public Break BreakState {
      get {
        return m_BreakState;
      }
      set { // Cf IsTransparent which affects LOS calculations
        Break old = m_BreakState;
        m_BreakState = value;
        if ((Break.BROKEN == old)!=(Break.BROKEN == value)) InvalidateLOS();
      }
    }

    public bool GivesWood {
      get {
        if (GetFlag(MapObject.Flags.GIVES_WOOD))
          return m_BreakState != MapObject.Break.BROKEN;
        return false;
      }
      set {
        SetFlag(MapObject.Flags.GIVES_WOOD, value);
      }
    }

    public bool IsMovable {
      get {
        return GetFlag(MapObject.Flags.IS_MOVABLE);
      }
      set {
        SetFlag(MapObject.Flags.IS_MOVABLE, value);
      }
    }

    public bool BreaksWhenFiredThrough {
      get {
        return GetFlag(MapObject.Flags.BREAKS_WHEN_FIRED_THROUGH);
      }
      set {
        SetFlag(MapObject.Flags.BREAKS_WHEN_FIRED_THROUGH, value);
      }
    }

    public bool StandOnFovBonus {
      get {
        return GetFlag(MapObject.Flags.STANDON_FOV_BONUS);
      }
      set {
        SetFlag(MapObject.Flags.STANDON_FOV_BONUS, value);
      }
    }

    public int Weight {
      get {
        return m_Weight;
      }
      set {
        m_Weight = Math.Max(1, value);
      }
    }

    public bool IsFlammable {
      get {
        if (m_FireState != Fire.ONFIRE)
          return m_FireState == Fire.BURNABLE;
        return true;
      }
    }

    public bool IsOnFire {
      get {
        return m_FireState == Fire.ONFIRE;
      }
    }

    public bool IsBurntToAshes {
      get {
        return m_FireState == Fire.ASHES;
      }
    }

    public Fire FireState {
      get {
        return m_FireState;
      }
      set { // cf IsTransparent which affects LOS calculations
        Fire old = m_FireState;
        m_FireState = value;
        if (   (Fire.ONFIRE == old) != (Fire.ONFIRE == value)
            || (Fire.ASHES == old)  != (Fire.ASHES == value))
          InvalidateLOS();
      }
    }

    public int HitPoints {
      get {
        return m_HitPoints;
      }
      set {
        m_HitPoints = value;
      }
    }

    public int MaxHitPoints {
      get {
        return m_MaxHitPoints;
      }
    }

    public MapObject(string aName, string hiddenImageID)
      : this(aName, hiddenImageID, MapObject.Break.UNBREAKABLE, MapObject.Fire.UNINFLAMMABLE, 0)
    {
    }

    public MapObject(string aName, string hiddenImageID, MapObject.Break breakable, MapObject.Fire burnable, int hitPoints)
    {
      Contract.Requires(null!=aName);
      Contract.Requires(null!= hiddenImageID);
      m_Name = aName;
      m_ImageID = m_HiddenImageID = hiddenImageID;
      m_BreakState = breakable;
      m_FireState = burnable;
      if (breakable == MapObject.Break.UNBREAKABLE && burnable == MapObject.Fire.UNINFLAMMABLE) return;
      m_HitPoints = m_MaxHitPoints = hitPoints;
    }

    protected void InvalidateLOS() 
    {
      if (null != m_Location.Map) Engine.LOS.Validate(m_Location.Map,los => !los.Contains(m_Location.Position));
    }

    private string ReasonCantPushTo(Point toPos)
    {
      Map tmp = Location.Map;
      if (!tmp.IsInBounds(toPos)) return "out of map";
      if (!tmp.GetTileModelAt(toPos).IsWalkable) return "blocked by an obstacle";
      if (tmp.GetMapObjectAt(toPos) != null) return "blocked by an object";
      if (tmp.GetActorAt(toPos) != null) return "blocked by someone";
      return "";
    }

    public bool CanPushTo(Point toPos, out string reason)
    { 
      reason = ReasonCantPushTo(toPos);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanPushTo(Point toPos)
    { 
      return string.IsNullOrEmpty(ReasonCantPushTo(toPos));
    }

    // flag handling
    private bool GetFlag(MapObject.Flags f)
    {
      return (m_Flags & f) != MapObject.Flags.NONE;
    }

    private void SetFlag(MapObject.Flags f, bool value)
    {
      if (value)
        m_Flags |= f;
      else
        m_Flags &= ~f;
    }

    private void OneFlag(MapObject.Flags f)
    {
      m_Flags |= f;
    }

    private void ZeroFlag(MapObject.Flags f)
    {
      m_Flags &= ~f;
    }

    // fire
    public void Ignite()
    {
      m_FireState = MapObject.Fire.ONFIRE;
      --m_JumpLevel;
    }

    public void Extinguish()
    {
      ++m_JumpLevel;
      m_FireState = MapObject.Fire.BURNABLE;
    }

    [Serializable]
    public enum Break : byte
    {
      UNBREAKABLE,
      BREAKABLE,
      BROKEN,
    }

    [Serializable]
    public enum Fire : byte
    {
      UNINFLAMMABLE,
      BURNABLE,
      ONFIRE,
      ASHES,
    }

    [System.Flags]
    private enum Flags
    {
      NONE = 0,
      IS_AN = 1,
      IS_PLURAL = 2,
      IS_MATERIAL_TRANSPARENT = 4,
      IS_WALKABLE = 8,
      IS_CONTAINER = 16,
      IS_COUCH = 32,
      GIVES_WOOD = 64,
      IS_MOVABLE = 128,
      BREAKS_WHEN_FIRED_THROUGH = 256,
      STANDON_FOV_BONUS = 512,
    }
  }
}
