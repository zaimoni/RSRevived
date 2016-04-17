// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.MapObject
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class MapObject
  {
    private string m_ImageID;
    private string m_HiddenImageID;
    private string m_Name;
    private MapObject.Flags m_Flags;
    private int m_JumpLevel;
    private int m_Weight;
    private MapObject.Break m_BreakState;
    private int m_MaxHitPoints;
    private int m_HitPoints;
    private MapObject.Fire m_FireState;
    private Location m_Location;

    public string AName
    {
      get
      {
        return (this.IsAn ? "an " : (this.IsPlural ? "some " : "a ")) + this.m_Name;
      }
    }

    public string TheName
    {
      get
      {
        return "the " + this.m_Name;
      }
    }

    public bool IsAn
    {
      get
      {
        return this.GetFlag(MapObject.Flags.IS_AN);
      }
      set
      {
        this.SetFlag(MapObject.Flags.IS_AN, value);
      }
    }

    public bool IsPlural
    {
      get
      {
        return this.GetFlag(MapObject.Flags.IS_PLURAL);
      }
      set
      {
        this.SetFlag(MapObject.Flags.IS_PLURAL, value);
      }
    }

    public string ImageID
    {
      get
      {
        return this.m_ImageID;
      }
      set
      {
        this.m_ImageID = value;
      }
    }

    public string HiddenImageID
    {
      get
      {
        return this.m_HiddenImageID;
      }
    }

    public Location Location
    {
      get
      {
        return this.m_Location;
      }
      set
      {
        this.m_Location = value;
      }
    }

    public virtual bool IsTransparent
    {
      get
      {
        if (this.m_FireState == MapObject.Fire.ONFIRE)
          return false;
        if (this.m_BreakState == MapObject.Break.BROKEN || this.m_FireState == MapObject.Fire.ASHES)
          return true;
        return this.GetFlag(MapObject.Flags.IS_MATERIAL_TRANSPARENT);
      }
    }

    public bool IsMaterialTransparent
    {
      get
      {
        return this.GetFlag(MapObject.Flags.IS_MATERIAL_TRANSPARENT);
      }
      set
      {
        this.SetFlag(MapObject.Flags.IS_MATERIAL_TRANSPARENT, value);
      }
    }

    public bool IsWalkable
    {
      get
      {
        return this.GetFlag(MapObject.Flags.IS_WALKABLE);
      }
      set
      {
        this.SetFlag(MapObject.Flags.IS_WALKABLE, value);
      }
    }

    public int JumpLevel
    {
      get
      {
        return this.m_JumpLevel;
      }
      set
      {
        this.m_JumpLevel = value;
      }
    }

    public bool IsJumpable
    {
      get
      {
        return this.m_JumpLevel > 0;
      }
    }

    public bool IsContainer
    {
      get
      {
        return this.GetFlag(MapObject.Flags.IS_CONTAINER);
      }
      set
      {
        this.SetFlag(MapObject.Flags.IS_CONTAINER, value);
      }
    }

    public bool IsCouch
    {
      get
      {
        return this.GetFlag(MapObject.Flags.IS_COUCH);
      }
      set
      {
        this.SetFlag(MapObject.Flags.IS_COUCH, value);
      }
    }

    public bool IsBreakable
    {
      get
      {
        return this.m_BreakState == MapObject.Break.BREAKABLE;
      }
    }

    public MapObject.Break BreakState
    {
      get
      {
        return this.m_BreakState;
      }
      set
      {
        this.m_BreakState = value;
      }
    }

    public bool GivesWood
    {
      get
      {
        if (this.GetFlag(MapObject.Flags.GIVES_WOOD))
          return this.m_BreakState != MapObject.Break.BROKEN;
        return false;
      }
      set
      {
        this.SetFlag(MapObject.Flags.GIVES_WOOD, value);
      }
    }

    public bool IsMovable
    {
      get
      {
        return this.GetFlag(MapObject.Flags.IS_MOVABLE);
      }
      set
      {
        this.SetFlag(MapObject.Flags.IS_MOVABLE, value);
      }
    }

    public bool BreaksWhenFiredThrough
    {
      get
      {
        return this.GetFlag(MapObject.Flags.BREAKS_WHEN_FIRED_THROUGH);
      }
      set
      {
        this.SetFlag(MapObject.Flags.BREAKS_WHEN_FIRED_THROUGH, value);
      }
    }

    public bool StandOnFovBonus
    {
      get
      {
        return this.GetFlag(MapObject.Flags.STANDON_FOV_BONUS);
      }
      set
      {
        this.SetFlag(MapObject.Flags.STANDON_FOV_BONUS, value);
      }
    }

    public int Weight
    {
      get
      {
        return this.m_Weight;
      }
      set
      {
        this.m_Weight = Math.Max(1, value);
      }
    }

    public bool IsFlammable
    {
      get
      {
        if (this.m_FireState != MapObject.Fire.ONFIRE)
          return this.m_FireState == MapObject.Fire.BURNABLE;
        return true;
      }
    }

    public bool IsOnFire
    {
      get
      {
        return this.m_FireState == MapObject.Fire.ONFIRE;
      }
    }

    public bool IsBurntToAshes
    {
      get
      {
        return this.m_FireState == MapObject.Fire.ASHES;
      }
    }

    public MapObject.Fire FireState
    {
      get
      {
        return this.m_FireState;
      }
      set
      {
        this.m_FireState = value;
      }
    }

    public int HitPoints
    {
      get
      {
        return this.m_HitPoints;
      }
      set
      {
        this.m_HitPoints = value;
      }
    }

    public int MaxHitPoints
    {
      get
      {
        return this.m_MaxHitPoints;
      }
    }

    public MapObject(string aName, string hiddenImageID)
      : this(aName, hiddenImageID, MapObject.Break.UNBREAKABLE, MapObject.Fire.UNINFLAMMABLE, 0)
    {
    }

    public MapObject(string aName, string hiddenImageID, MapObject.Break breakable, MapObject.Fire burnable, int hitPoints)
    {
      if (aName == null)
        throw new ArgumentNullException("aName");
      if (hiddenImageID == null)
        throw new ArgumentNullException("hiddenImageID");
      this.m_Name = aName;
      this.m_ImageID = this.m_HiddenImageID = hiddenImageID;
      this.m_BreakState = breakable;
      this.m_FireState = burnable;
      if (breakable == MapObject.Break.UNBREAKABLE && burnable == MapObject.Fire.UNINFLAMMABLE)
        return;
      this.m_HitPoints = this.m_MaxHitPoints = hitPoints;
    }

    private bool GetFlag(MapObject.Flags f)
    {
      return (this.m_Flags & f) != MapObject.Flags.NONE;
    }

    private void SetFlag(MapObject.Flags f, bool value)
    {
      if (value)
        this.m_Flags |= f;
      else
        this.m_Flags &= ~f;
    }

    private void OneFlag(MapObject.Flags f)
    {
      this.m_Flags |= f;
    }

    private void ZeroFlag(MapObject.Flags f)
    {
      this.m_Flags &= ~f;
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
