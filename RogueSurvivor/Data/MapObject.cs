// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.MapObject
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using Zaimoni.Data;

using Point = System.Drawing.Point;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class MapObject
  {
    private IDs m_ID;
    private string m_ImageID;
    private readonly string m_HiddenImageID;
    private readonly string m_Name;
    private Flags m_Flags;
    private int m_JumpLevel;
    public readonly byte Weight; // Weight is positive if and only if the object is movable
    private Break m_BreakState;
    private readonly int m_MaxHitPoints;
    private int m_HitPoints;
    private Fire m_FireState;
    private Location m_Location;

    public string AName {
      get {
        return IsPlural ? m_Name.PrefixIndefinitePluralArticle() : m_Name.PrefixIndefiniteSingularArticle();
      }
    }

    public string TheName {
      get {
        return m_Name.PrefixDefiniteSingularArticle();
      }
    }

    public bool IsPlural {
      get {
        return GetFlag(Flags.IS_PLURAL);
      }
    }

    public string ImageID {
      get {
        return m_ImageID;
      }
      set { // only used by subclasses of StateMapObject
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
        return GetFlag(Flags.IS_MATERIAL_TRANSPARENT);
      }
    }

    public bool IsWalkable {
      get {
        return GetFlag(Flags.IS_WALKABLE);
      }
      set {
        SetFlag(Flags.IS_WALKABLE, value);
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
        return GetFlag(Flags.IS_CONTAINER);
      }
      set {
        SetFlag(Flags.IS_CONTAINER, value);
      }
    }

    public bool IsCouch {
      get {
        return GetFlag(Flags.IS_COUCH);
      }
    }

    public bool IsBreakable {
      get {
        return m_BreakState == Break.BREAKABLE;
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
        return GetFlag(Flags.GIVES_WOOD) && Break.BROKEN != m_BreakState;
      }
    }

    public bool IsMovable {
      get {
        return GetFlag(Flags.IS_MOVABLE);
      }
    }

    public bool BreaksWhenFiredThrough {
      get {
        return GetFlag(Flags.BREAKS_WHEN_FIRED_THROUGH);
      }
    }

    public bool StandOnFovBonus {
      get {
        return GetFlag(Flags.STANDON_FOV_BONUS);
      }
    }

    public bool IsFlammable {
      get {
        return m_FireState == Fire.ONFIRE || m_FireState == Fire.BURNABLE;
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

    // This section of private switch statements arguably could designate properties of a MapObjectModel class.
    private static byte _ID_Weight(IDs x)
    {
      switch(x) {
        case IDs.SMALL_FORTIFICATION: return 4;
        case IDs.BED: return 6; // XXX all beds should have same weight
        case IDs.HOSPITAL_BED: return 6;
        case IDs.CHAIR: return 1;   // XXX all chairs should have same weight
        case IDs.HOSPITAL_CHAIR: return 1;
        case IDs.CHAR_CHAIR: return 1;
        case IDs.TABLE: return 2;   // XXX all tables should have same weight
        case IDs.CHAR_TABLE: return 2;
        case IDs.NIGHT_TABLE: return 1; // XXX all night tables should have same weight
        case IDs.HOSPITAL_NIGHT_TABLE: return 1;
        case IDs.DRAWER: return 6;
        case IDs.FRIDGE: return 10;
        case IDs.WARDROBE: return 10;   // all wardrobes should have same weight
        case IDs.HOSPITAL_WARDROBE: return 10;
        case IDs.CAR1: return 100;  // all cars should have same weight
        case IDs.CAR2: return 100;
        case IDs.CAR3: return 100;
        case IDs.CAR4: return 100;
        case IDs.SHOP_SHELF: return 6;
        case IDs.JUNK: return 6;
        case IDs.BARRELS: return 10;
//      case MapObject.IDs.: return ;
        default: return 0;  // not moveable
      }
    }

    private static bool _ID_GivesWood(IDs x)
    {
      switch (x) {
        case IDs.FENCE: return true;
        case IDs.TREE: return true;
        case IDs.DOOR: return true;
        case IDs.WINDOW: return true;
        case IDs.HOSPITAL_DOOR: return true;
        case IDs.BENCH: return true;
        case IDs.LARGE_FORTIFICATION: return true;
        case IDs.SMALL_FORTIFICATION: return true;
        case IDs.BED: return true;
        case IDs.HOSPITAL_BED: return true;
        case IDs.CHAIR: return true;
        case IDs.HOSPITAL_CHAIR: return true;
        case IDs.CHAR_CHAIR: return true;
        case IDs.TABLE: return true;
        case IDs.CHAR_TABLE: return true;
        case IDs.NIGHT_TABLE: return true;
        case IDs.HOSPITAL_NIGHT_TABLE: return true;
        case IDs.DRAWER: return true;
        case IDs.WARDROBE: return true;
        case IDs.HOSPITAL_WARDROBE: return true;
        case IDs.SHOP_SHELF: return true;
        case IDs.JUNK: return true;
        case IDs.BARRELS: return true;
//      case IDs.: return true;
        default: return false;
      }
    }

    static private bool _ID_StandOnFOVbonus(IDs x)
    {
      switch (x) {
        case IDs.FENCE: return true;
        case IDs.CAR1: return true;
        case IDs.CAR2: return true;
        case IDs.CAR3: return true;
        case IDs.CAR4: return true;
//      case IDs.: return true;
        default: return false;
      }
    }

    static private bool _ID_BreaksWhenFiredThrough(IDs x)
    {
      switch (x) {
        case IDs.WINDOW: return true;
        case IDs.GLASS_DOOR: return true;
//      case IDs.: return true;
        default: return false;
      }
    }

    static private bool _ID_IsCouch(IDs x)
    {
      switch (x) {
        case IDs.BENCH: return true;
        case IDs.IRON_BENCH: return true;
        case IDs.BED: return true;
        case IDs.HOSPITAL_BED: return true;
//      case IDs.: return true;
        default: return false;
      }
    }

    static private bool _ID_IsPlural(IDs x)
    {
      switch (x) {
        case IDs.JUNK: return true;
        case IDs.BARRELS: return true;
//      case IDs.: return true;
        default: return false;
      }
    }

    static private bool _ID_MaterialIsTransparent(IDs x)
    {
      switch (x) {
        case IDs.FENCE: return true;
        case IDs.IRON_FENCE: return true;
        case IDs.IRON_GATE_CLOSED: return true;
        case IDs.IRON_GATE_OPEN: return true;
        case IDs.WINDOW: return true;
        case IDs.GLASS_DOOR: return true;
        case IDs.BENCH: return true;
        case IDs.IRON_BENCH: return true;
        case IDs.SMALL_FORTIFICATION: return true;
        case IDs.BED: return true;
        case IDs.HOSPITAL_BED: return true;
        case IDs.CHAIR: return true;
        case IDs.HOSPITAL_CHAIR: return true;
        case IDs.CHAR_CHAIR: return true;
        case IDs.TABLE: return true;
        case IDs.CHAR_TABLE: return true;
        case IDs.NIGHT_TABLE: return true;
        case IDs.HOSPITAL_NIGHT_TABLE: return true;
        case IDs.DRAWER: return true;
        case IDs.WARDROBE: return true;
        case IDs.HOSPITAL_WARDROBE: return true;
        case IDs.CAR1: return true;
        case IDs.CAR2: return true;
        case IDs.CAR3: return true;
        case IDs.CAR4: return true;
        case IDs.JUNK: return true;
        case IDs.BARRELS: return true;
//      case MapObject.IDs.: return true;
        default: return false;
      }
    }

    public MapObject(string aName, string hiddenImageID, int hitPoints=0, Fire burnable = Fire.UNINFLAMMABLE)
    {
#if DEBUG
      if (string.IsNullOrEmpty(aName)) throw new ArgumentNullException(nameof(aName));
      if (string.IsNullOrEmpty(hiddenImageID)) throw new ArgumentNullException(nameof(hiddenImageID));
#endif
      m_Name = aName;
      m_ImageID = m_HiddenImageID = hiddenImageID;
      m_BreakState = (0==hitPoints ? Break.UNBREAKABLE : Break.BREAKABLE); // breakable := nonzero max hp
      m_FireState = burnable;

      m_ID = hiddenImageID.MapObject_ID();
      Weight = _ID_Weight(m_ID);
      if (0 < Weight) m_Flags |= Flags.IS_MOVABLE;
      if (_ID_GivesWood(m_ID)) m_Flags |= Flags.GIVES_WOOD;
      if (_ID_StandOnFOVbonus(m_ID)) m_Flags |= Flags.STANDON_FOV_BONUS;
      if (_ID_BreaksWhenFiredThrough(m_ID)) m_Flags |= Flags.BREAKS_WHEN_FIRED_THROUGH;
      if (_ID_IsCouch(m_ID)) m_Flags |= Flags.IS_COUCH;
      if (_ID_IsPlural(m_ID)) m_Flags |= Flags.IS_PLURAL;
      if (_ID_MaterialIsTransparent(m_ID)) m_Flags |= Flags.IS_MATERIAL_TRANSPARENT;
      // following are currently mutually exclusive: IsWalkable, IsJumpable, IsContainer
      // would be nice if it was possible to move on a container (this would make the starting game items more accessible), but there are UI issues
      // StandsOnFovBonus requires IsJumpable

      if (0 == hitPoints && burnable == Fire.UNINFLAMMABLE) return;
      m_HitPoints = m_MaxHitPoints = hitPoints;
    }

    protected void InvalidateLOS()
    {
      if (null != m_Location.Map) Engine.LOS.Validate(m_Location.Map,los => !los.Contains(m_Location.Position));
    }

    private string ReasonCantPushTo(Point toPos)
    {
      Map tmp = Location.Map;
      if (!tmp.IsInBounds(toPos)) return "out of map";  // XXX should be IsValid but that's a completely different code path
      if (!tmp.GetTileModelAt(toPos).IsWalkable) return "blocked by an obstacle";   // XXX likewise should be GetTileModelAtExt
      if (tmp.HasMapObjectAt(toPos)) return "blocked by an object";
      if (tmp.HasActorAt(toPos)) return "blocked by someone";
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

    public void PlaceAt(Map m, Point pos) {m.PlaceAt(this,pos);} // this guaranteed non-null so non-null precondition ok

    public void Remove()
    {
      if (null == Location.Map) return;
      Location.Map.RemoveMapObjectAt(Location.Position.X,Location.Position.Y);
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
      m_FireState = Fire.ONFIRE;
      --m_JumpLevel;
    }

    public void Extinguish()
    {
      ++m_JumpLevel;
      m_FireState = Fire.BURNABLE;
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
      IS_AN = 1,    // XXX dead, retaining for binary compatibility
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

    public enum IDs : byte
    {
      FENCE = 0,    // not-pushable ID block
      IRON_FENCE,
      BOARD,
      TREE,
      IRON_GATE_CLOSED,
      IRON_GATE_OPEN,
      CHAR_POWER_GENERATOR, // Tesla technology
      DOOR,
      WINDOW,
      HOSPITAL_DOOR,
      GLASS_DOOR,
      CHAR_DOOR,
      IRON_DOOR,
      BENCH,
      IRON_BENCH,
      LARGE_FORTIFICATION,
      SMALL_FORTIFICATION,  // pushable ID block
      BED,          
      HOSPITAL_BED,
      CHAIR,
      HOSPITAL_CHAIR,
      CHAR_CHAIR,
      TABLE,
      CHAR_TABLE,
      NIGHT_TABLE,
      HOSPITAL_NIGHT_TABLE,
      DRAWER,
      FRIDGE,
      WARDROBE,
      HOSPITAL_WARDROBE,
      CAR1,
      CAR2,
      CAR3,
      CAR4,
      SHOP_SHELF,
      JUNK,
      BARRELS
//    on release of v0.10.0.0 all enumeration values "freeze" and new enumeration values must be later for savefile compatibility (until value 256 is needed at which point the savefile breaks)
    }
  } // MapObject

  static class MapObject_ext
  {
    // We use a switch to do this translation because this is hand-maintained, and the relative ordering of the enumeration values is not expected to be stable.
    static public string ImageID(this MapObject.IDs x)
    {
      switch (x) {
        case MapObject.IDs.FENCE: return Gameplay.GameImages.OBJ_FENCE;
        case MapObject.IDs.IRON_FENCE: return Gameplay.GameImages.OBJ_IRON_FENCE;
        case MapObject.IDs.BOARD: return Gameplay.GameImages.OBJ_BOARD;
        case MapObject.IDs.TREE: return Gameplay.GameImages.OBJ_TREE;
        case MapObject.IDs.IRON_GATE_CLOSED: return Gameplay.GameImages.OBJ_GATE_CLOSED;
        case MapObject.IDs.IRON_GATE_OPEN: return Gameplay.GameImages.OBJ_GATE_OPEN;
        case MapObject.IDs.CHAR_POWER_GENERATOR: return Gameplay.GameImages.OBJ_POWERGEN_OFF;
        case MapObject.IDs.DOOR: return Gameplay.GameImages.OBJ_WOODEN_DOOR_CLOSED;
        case MapObject.IDs.WINDOW: return Gameplay.GameImages.OBJ_WINDOW_CLOSED;
        case MapObject.IDs.HOSPITAL_DOOR: return Gameplay.GameImages.OBJ_HOSPITAL_DOOR_CLOSED;
        case MapObject.IDs.GLASS_DOOR: return Gameplay.GameImages.OBJ_GLASS_DOOR_CLOSED;
        case MapObject.IDs.CHAR_DOOR: return Gameplay.GameImages.OBJ_CHAR_DOOR_CLOSED;
        case MapObject.IDs.IRON_DOOR: return Gameplay.GameImages.OBJ_IRON_DOOR_CLOSED;
        case MapObject.IDs.BENCH: return Gameplay.GameImages.OBJ_BENCH;
        case MapObject.IDs.IRON_BENCH: return Gameplay.GameImages.OBJ_IRON_BENCH;
        case MapObject.IDs.LARGE_FORTIFICATION: return Gameplay.GameImages.OBJ_LARGE_WOODEN_FORTIFICATION;
        case MapObject.IDs.SMALL_FORTIFICATION: return Gameplay.GameImages.OBJ_SMALL_WOODEN_FORTIFICATION;
        case MapObject.IDs.BED: return Gameplay.GameImages.OBJ_BED;
        case MapObject.IDs.HOSPITAL_BED: return Gameplay.GameImages.OBJ_HOSPITAL_BED;
        case MapObject.IDs.CHAIR: return Gameplay.GameImages.OBJ_CHAIR;
        case MapObject.IDs.HOSPITAL_CHAIR: return Gameplay.GameImages.OBJ_HOSPITAL_CHAIR;
        case MapObject.IDs.CHAR_CHAIR: return Gameplay.GameImages.OBJ_CHAR_CHAIR;
        case MapObject.IDs.TABLE: return Gameplay.GameImages.OBJ_TABLE;
        case MapObject.IDs.CHAR_TABLE: return Gameplay.GameImages.OBJ_CHAR_TABLE;
        case MapObject.IDs.NIGHT_TABLE: return Gameplay.GameImages.OBJ_NIGHT_TABLE;
        case MapObject.IDs.HOSPITAL_NIGHT_TABLE: return Gameplay.GameImages.OBJ_HOSPITAL_NIGHT_TABLE;
        case MapObject.IDs.DRAWER: return Gameplay.GameImages.OBJ_DRAWER;
        case MapObject.IDs.FRIDGE: return Gameplay.GameImages.OBJ_FRIDGE;
        case MapObject.IDs.WARDROBE: return Gameplay.GameImages.OBJ_WARDROBE;
        case MapObject.IDs.HOSPITAL_WARDROBE: return Gameplay.GameImages.OBJ_HOSPITAL_WARDROBE;
        case MapObject.IDs.CAR1: return Gameplay.GameImages.OBJ_CAR1;
        case MapObject.IDs.CAR2: return Gameplay.GameImages.OBJ_CAR2;
        case MapObject.IDs.CAR3: return Gameplay.GameImages.OBJ_CAR3;
        case MapObject.IDs.CAR4: return Gameplay.GameImages.OBJ_CAR4;
        case MapObject.IDs.SHOP_SHELF: return Gameplay.GameImages.OBJ_SHOP_SHELF;
        case MapObject.IDs.JUNK: return Gameplay.GameImages.OBJ_JUNK;
        case MapObject.IDs.BARRELS: return Gameplay.GameImages.OBJ_BARRELS;
//      case MapObject.IDs.: return Gameplay.GameImages.;
        default: throw new ArgumentOutOfRangeException(nameof(x), x, "not mapped to an MapObject ID");
      }
    }

    static public MapObject.IDs MapObject_ID(this string x)
    {
      switch (x) {
        case Gameplay.GameImages.OBJ_FENCE: return MapObject.IDs.FENCE;
        case Gameplay.GameImages.OBJ_IRON_FENCE: return MapObject.IDs.IRON_FENCE;
        case Gameplay.GameImages.OBJ_BOARD: return MapObject.IDs.BOARD;
        case Gameplay.GameImages.OBJ_TREE: return MapObject.IDs.TREE;
        case Gameplay.GameImages.OBJ_GATE_CLOSED: return MapObject.IDs.IRON_GATE_CLOSED;
        case Gameplay.GameImages.OBJ_GATE_OPEN: return MapObject.IDs.IRON_GATE_OPEN;
        case Gameplay.GameImages.OBJ_POWERGEN_OFF: return MapObject.IDs.CHAR_POWER_GENERATOR;
        case Gameplay.GameImages.OBJ_WOODEN_DOOR_CLOSED: return MapObject.IDs.DOOR;
        case Gameplay.GameImages.OBJ_WINDOW_CLOSED: return MapObject.IDs.WINDOW;
        case Gameplay.GameImages.OBJ_HOSPITAL_DOOR_CLOSED: return MapObject.IDs.HOSPITAL_DOOR;
        case Gameplay.GameImages.OBJ_GLASS_DOOR_CLOSED: return MapObject.IDs.GLASS_DOOR;
        case Gameplay.GameImages.OBJ_CHAR_DOOR_CLOSED: return MapObject.IDs.CHAR_DOOR;
        case Gameplay.GameImages.OBJ_IRON_DOOR_CLOSED: return MapObject.IDs.IRON_DOOR;
        case Gameplay.GameImages.OBJ_BENCH: return MapObject.IDs.BENCH;
        case Gameplay.GameImages.OBJ_IRON_BENCH: return MapObject.IDs.IRON_BENCH;
        case Gameplay.GameImages.OBJ_LARGE_WOODEN_FORTIFICATION: return MapObject.IDs.LARGE_FORTIFICATION;
        case Gameplay.GameImages.OBJ_SMALL_WOODEN_FORTIFICATION: return MapObject.IDs.SMALL_FORTIFICATION;
        case Gameplay.GameImages.OBJ_BED: return MapObject.IDs.BED;
        case Gameplay.GameImages.OBJ_HOSPITAL_BED: return MapObject.IDs.HOSPITAL_BED;
        case Gameplay.GameImages.OBJ_CHAIR: return MapObject.IDs.CHAIR;
        case Gameplay.GameImages.OBJ_HOSPITAL_CHAIR: return MapObject.IDs.HOSPITAL_CHAIR;
        case Gameplay.GameImages.OBJ_CHAR_CHAIR: return MapObject.IDs.CHAR_CHAIR;
        case Gameplay.GameImages.OBJ_TABLE: return MapObject.IDs.TABLE;
        case Gameplay.GameImages.OBJ_CHAR_TABLE: return MapObject.IDs.CHAR_TABLE;
        case Gameplay.GameImages.OBJ_NIGHT_TABLE: return MapObject.IDs.NIGHT_TABLE;
        case Gameplay.GameImages.OBJ_HOSPITAL_NIGHT_TABLE: return MapObject.IDs.HOSPITAL_NIGHT_TABLE;
        case Gameplay.GameImages.OBJ_DRAWER: return MapObject.IDs.DRAWER;
        case Gameplay.GameImages.OBJ_FRIDGE: return MapObject.IDs.FRIDGE;
        case Gameplay.GameImages.OBJ_WARDROBE: return MapObject.IDs.WARDROBE;
        case Gameplay.GameImages.OBJ_HOSPITAL_WARDROBE: return MapObject.IDs.HOSPITAL_WARDROBE;
        case Gameplay.GameImages.OBJ_CAR1: return MapObject.IDs.CAR1;
        case Gameplay.GameImages.OBJ_CAR2: return MapObject.IDs.CAR2;
        case Gameplay.GameImages.OBJ_CAR3: return MapObject.IDs.CAR3;
        case Gameplay.GameImages.OBJ_CAR4: return MapObject.IDs.CAR4;
        case Gameplay.GameImages.OBJ_SHOP_SHELF: return MapObject.IDs.SHOP_SHELF;
        case Gameplay.GameImages.OBJ_JUNK: return MapObject.IDs.JUNK;
        case Gameplay.GameImages.OBJ_BARRELS: return MapObject.IDs.BARRELS;
//      case Gameplay.GameImages.: return MapObject.IDs.;
        default: throw new ArgumentOutOfRangeException(nameof(x), x, "not mapped to an MapObject ID");
      }
    }
  } // MapObject_ext
}
