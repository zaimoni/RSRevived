// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.MapObject
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using Zaimoni.Data;

using ItemBarricadeMaterial = djack.RogueSurvivor.Engine.Items.ItemBarricadeMaterial;
using Point = Zaimoni.Data.Vector2D_short;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class MapObject
  {
    public const int CAR_WEIGHT = 100;

    private IDs m_ID;
    private Flags m_Flags;
    private int m_JumpLevel;
    public readonly byte Weight; // Weight is positive if and only if the object is movable
    private Break m_BreakState;
    public readonly int MaxHitPoints;
    private int m_HitPoints;
    private Fire m_FireState;
    private Location m_Location;

    public string AName { get { return IsPlural ? _ID_Name(m_ID).PrefixIndefinitePluralArticle() : _ID_Name(m_ID).PrefixIndefiniteSingularArticle(); } }
    public string TheName { get { return _ID_Name(m_ID).PrefixDefiniteSingularArticle(); } }
    public bool IsPlural { get { return GetFlag(Flags.IS_PLURAL); } }

    public IDs ID {
      get {
        return m_ID;
      }
      set {
#if DEBUG
        if (!_IDchangeIsLegal(value)) throw new ArgumentOutOfRangeException(nameof(value), "!_IDchangeIsLegal(value)");
#endif
        m_ID = value;
        _InitModel();
      }
    }

    public virtual string ImageID { get { return m_ID.ImageID(); } }
    public string HiddenImageID { get { return m_ID.ImageID(); } }

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

    public bool IsMaterialTransparent { get { return GetFlag(Flags.IS_MATERIAL_TRANSPARENT); } }
    virtual public bool IsWalkable { get { return GetFlag(Flags.IS_WALKABLE); } }
    public int JumpLevel { get { return m_JumpLevel; } }
    public bool IsJumpable { get { return m_JumpLevel > 0; } }
    public bool IsContainer { get { return GetFlag(Flags.IS_CONTAINER); } }
    public bool IsCouch { get { return GetFlag(Flags.IS_COUCH); } }
    public bool IsBreakable { get { return m_BreakState == Break.BREAKABLE; } }

    public Break BreakState {
      get {
        return m_BreakState;
      }
      protected set { // Cf IsTransparent which affects LOS calculations
        Break old = m_BreakState;
        m_BreakState = value;
        if ((Break.BROKEN == old)!=(Break.BROKEN == value)) InvalidateLOS();
      }
    }

    protected void _break()
    {
      BreakState = Break.BROKEN;    // use accessor to trigger LOS invalidation
      m_HitPoints = 0;
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

    public int HitPoints { get { return m_HitPoints; } }

    public bool Repair(int hp)
    {
      if (m_HitPoints >= MaxHitPoints) return false;
      if (MaxHitPoints- m_HitPoints > hp) m_HitPoints += hp;
      else m_HitPoints = MaxHitPoints;
      return true;
    }

    /// <returns>true if and only if destroyed</returns>
    public bool Damage(int hp)
    {
      if (0 >= hp) return false;    // insignificant damage
      if (0 >= MaxHitPoints) return false;  // object is made of indestructible unobtainium
      if (hp < m_HitPoints) {
        m_HitPoints -= hp;
        return false;
      }
      Destroy();
      return true;
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
        case IDs.CAR1: return CAR_WEIGHT;  // all cars should have same weight
        case IDs.CAR2: return CAR_WEIGHT;
        case IDs.CAR3: return CAR_WEIGHT;
        case IDs.CAR4: return CAR_WEIGHT;
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
        case IDs.GARDEN_FENCE: return true;
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
        case IDs.GARDEN_FENCE: return true;
        case IDs.WIRE_FENCE: return true;
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
        case IDs.GARDEN_FENCE: return true;
        case IDs.WIRE_FENCE: return true;
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

    static private bool _ID_IsContainer(IDs x)
    {
      switch (x) {
        case IDs.DRAWER: return true;
        case IDs.FRIDGE: return true;
        case IDs.WARDROBE: return true;
        case IDs.HOSPITAL_WARDROBE: return true;
        case IDs.SHOP_SHELF: return true;
//      case IDs.: return true;
        default: return false;
      }
    }

    static private int _ID_Jumplevel(IDs x)
    {
      switch (x) {
        case IDs.FENCE: return 1;
        case IDs.GARDEN_FENCE: return 1;
        case IDs.WIRE_FENCE: return 1;
        case IDs.BENCH: return 1;
        case IDs.IRON_BENCH: return 1;
        case IDs.SMALL_FORTIFICATION: return 1;
        case IDs.CHAIR: return 1;
        case IDs.HOSPITAL_CHAIR: return 1;
        case IDs.CHAR_CHAIR: return 1;
        case IDs.TABLE: return 1;
        case IDs.CHAR_TABLE: return 1;
        case IDs.NIGHT_TABLE: return 1;
        case IDs.HOSPITAL_NIGHT_TABLE: return 1;
        case IDs.CAR1: return 1;
        case IDs.CAR2: return 1;
        case IDs.CAR3: return 1;
        case IDs.CAR4: return 1;
//      case IDs.: return 1;
        default: return 0;
      }
    }

    static private bool _ID_IsWalkable(IDs x)
    {
      switch (x) {
        case IDs.IRON_GATE_OPEN: return true;
        case IDs.DOOR: return true;
        case IDs.WINDOW: return true;
        case IDs.HOSPITAL_DOOR: return true;
        case IDs.GLASS_DOOR: return true;
        case IDs.CHAR_DOOR: return true;
        case IDs.IRON_DOOR: return true;
        case IDs.BED: return true;
        case IDs.HOSPITAL_BED: return true;
//      case MapObject.IDs.: return true;
        default: return false;
      }
    }

    static private bool _ID_StartsBroken(IDs x)
    {
      switch (x) {
        case IDs.CAR1: return true;
        case IDs.CAR2: return true;
        case IDs.CAR3: return true;
        case IDs.CAR4: return true;
//      case IDs.: return true;
        default: return false;
      }
    }

    public MapObject(string hiddenImageID, int hitPoints=0, Fire burnable = Fire.UNINFLAMMABLE)
    {
#if DEBUG
      if (string.IsNullOrEmpty(hiddenImageID)) throw new ArgumentNullException(nameof(hiddenImageID));
#endif
      m_BreakState = (0==hitPoints ? Break.UNBREAKABLE : Break.BREAKABLE); // breakable := nonzero max hp
      m_FireState = burnable;   // XXX should be able to infer burnable from gives wood; other materials may also be burnable (need fire spread for this to matter)

      m_ID = hiddenImageID.MapObject_ID();
      if (_ID_StartsBroken(m_ID)) m_BreakState = Break.BROKEN;

      // model properties that may reasonably be expected to be invariant across changes
      Weight = _ID_Weight(m_ID);
      if (0 < Weight) m_Flags |= Flags.IS_MOVABLE;

      _InitModel();

      if (0 == hitPoints && burnable == Fire.UNINFLAMMABLE) return;
      m_HitPoints = MaxHitPoints = hitPoints;
    }

    private void _InitModel()
    {
      if (_ID_GivesWood(m_ID)) m_Flags |= Flags.GIVES_WOOD;
      if (_ID_StandOnFOVbonus(m_ID)) m_Flags |= Flags.STANDON_FOV_BONUS;
      if (_ID_BreaksWhenFiredThrough(m_ID)) m_Flags |= Flags.BREAKS_WHEN_FIRED_THROUGH;
      if (_ID_IsCouch(m_ID)) m_Flags |= Flags.IS_COUCH;
      if (_ID_IsPlural(m_ID)) m_Flags |= Flags.IS_PLURAL;
      if (_ID_MaterialIsTransparent(m_ID)) m_Flags |= Flags.IS_MATERIAL_TRANSPARENT;
      if (_ID_IsContainer(m_ID)) m_Flags |= Flags.IS_CONTAINER;
      if (_ID_IsWalkable(m_ID)) m_Flags |= Flags.IS_WALKABLE;
      m_JumpLevel = _ID_Jumplevel(m_ID);

      // following are currently mutually exclusive: IsWalkable, IsJumpable, IsContainer
      // would be nice if it was possible to move on a container (this would make the starting game items more accessible), but there are UI issues
      // StandsOnFovBonus requires IsJumpable
#if DEBUG
      if (StandOnFovBonus && !IsJumpable) throw new InvalidOperationException("must be able to jump on an object providing FOV bonus for standing on it");
      if (IsWalkable && IsJumpable) throw new InvalidOperationException("map objects may not be both walkable and jumpable");
#endif
    }

    private bool _IDchangeIsLegal(IDs dest)
    {
      if (IDs.IRON_GATE_CLOSED == dest && IDs.IRON_GATE_OPEN   == m_ID) return true;
      if (IDs.IRON_GATE_OPEN   == dest && IDs.IRON_GATE_CLOSED == m_ID) return true;
      return false;
    }

    protected void InvalidateLOS()
    {
      if (null != m_Location.Map) Engine.LOS.Validate(m_Location.Map,los => !los.Contains(m_Location.Position));
    }

#nullable enable
    public string ReasonCantPutItemIn(Actor actor)
    {
      if (!IsContainer) return "object is not a container";
      if (   !actor.Model.Abilities.HasInventory
          || !actor.Model.Abilities.CanUseMapObjects
          ||  actor.Inventory == null)
          return "cannot take an item";
      var itemsAt = Inventory;
      if (null != itemsAt && itemsAt.IsFull) return "container is full";
      return "";
    }
#nullable restore

    public void PutItemIn(Item gift)
    {
#if DEBUG
      if (null == gift) throw new ArgumentNullException(nameof(gift));
      if (!IsContainer) throw new InvalidOperationException("cannot put "+gift+" into non-container "+this+" @ "+Location);
#endif
      if (gift is Engine.Items.ItemTrap trap) trap.Desactivate();    // alpha10
      Location.Drop(gift);  // VAPORWARE: containers actually have inventory and items they contain are pushed with them
    }

#nullable enable
    public Inventory? Inventory { get {
#if DEBUG
      if (!IsContainer) throw new InvalidOperationException("cannot get contents of non-container "+this+" @ "+Location);
#endif
      return Location.Items;
    } }
#nullable restore

    private string ReasonCantPushTo(Point toPos)
    {
      var tile_loc = Location.Map.GetTileModelLocation(toPos);
      if (null == tile_loc.Key) return "out of map";
      if (!tile_loc.Key.IsWalkable) return "blocked by an obstacle";
      if (tile_loc.Value.HasMapObject) return "blocked by an object";
      if (tile_loc.Value.StrictHasActorAt) return "blocked by someone";
      return "";
    }

    /// <param name="to">Assumed in canonical form (in-bounds)</param>
    private string ReasonCantPushTo(in Location to)
    {
      Map map = to.Map;
      Point pos = to.Position;
      if (!map.GetTileModelAt(pos).IsWalkable) return "blocked by an obstacle";
      if (map.HasMapObjectAt(pos)) return "blocked by an object";
      if (map.HasActorAt(in pos)) return "blocked by someone";
      return "";
    }

    public bool CanPushTo(Point toPos, out string reason) { return string.IsNullOrEmpty(reason = ReasonCantPushTo(toPos)); }

    public bool CanPushTo(Point toPos) { return string.IsNullOrEmpty(ReasonCantPushTo(toPos)); }

#if DEAD_FUNC
    /// <param name="to">Assumed in canonical form (in-bounds)</param>
    public bool CanPushTo(in Location to, out string reason) { return string.IsNullOrEmpty(reason = ReasonCantPushTo(in to)); }
#endif
    /// <param name="to">Assumed in canonical form (in-bounds)</param>
    public bool CanPushTo(in Location to) { return string.IsNullOrEmpty(ReasonCantPushTo(in to)); }

    public void PlaceAt(Map m, in Point pos) {m.PlaceAt(this, pos);} // this guaranteed non-null so non-null precondition ok

    public void Remove()
    {
      if (null == Location.Map) return;
      Location.Map.RemoveMapObjectAt(Location.Position);
    }

    protected virtual void _destroy() { Remove(); }   // subtypes that do not simply vanish must override

    public void Destroy()
    {
      m_HitPoints = 0;
      if (GivesWood) {
        int val2 = 1 + MaxHitPoints / 40;
        while (val2 > 0) {
          ItemBarricadeMaterial barricadeMaterial = new ItemBarricadeMaterial(Gameplay.GameItems.WOODENPLANK) {
            Quantity = (sbyte)Math.Min(Gameplay.GameItems.WOODENPLANK.StackingLimit, val2)
          };
          val2 -= barricadeMaterial.Quantity;
          Location.Drop(barricadeMaterial);
        }
        if (RogueForm.Game.Rules.RollChance(Engine.Rules.IMPROVED_WEAPONS_FROM_BROKEN_WOOD_CHANCE)) {
          Location.Drop((RogueForm.Game.Rules.RollChance(50) ? Gameplay.GameItems.IMPROVISED_CLUB : Gameplay.GameItems.IMPROVISED_SPEAR).instantiate());
        }
      }
      _destroy();
      RogueForm.Game.OnLoudNoise(in m_Location, "A loud *CRASH*");
    }

    // flag handling
    private bool GetFlag(Flags f) { return (m_Flags & f) != Flags.NONE; }

#if DEAD_FUNC
    private void SetFlag(Flags f, bool value)
    {
      if (value)
        m_Flags |= f;
      else
        m_Flags &= ~f;
    }

    private void OneFlag(Flags f)
    {
      m_Flags |= f;
    }

    private void ZeroFlag(Flags f)
    {
      m_Flags &= ~f;
    }
#endif

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

    // could do this as non-static member function by hard coding m_ID as the switch
    static private string _ID_Name(IDs x)
    {
      switch (x) {
        case IDs.FENCE: return "fence";
        case IDs.IRON_FENCE: return "iron fence";
        case IDs.GARDEN_FENCE: return "garden fence";
        case IDs.WIRE_FENCE: return "wire fence";
        case IDs.BOARD: return "board";
        case IDs.TREE: return "tree";
        case IDs.IRON_GATE_CLOSED: return "iron gate";
        case IDs.IRON_GATE_OPEN: return "iron gate";
        case IDs.CHAR_POWER_GENERATOR: return "power generator";
        case IDs.DOOR: return "wooden door";
        case IDs.WINDOW: return "window";
        case IDs.HOSPITAL_DOOR: return "door";
        case IDs.GLASS_DOOR: return "glass door";
        case IDs.CHAR_DOOR: return "CHAR door";
        case IDs.IRON_DOOR: return "iron door";
        case IDs.BENCH: return "bench";
        case IDs.IRON_BENCH: return "iron bench";
        case IDs.LARGE_FORTIFICATION: return "large fortification";
        case IDs.SMALL_FORTIFICATION: return "small fortification";
        case IDs.BED: return "bed";
        case IDs.HOSPITAL_BED: return "bed";
        case IDs.CHAIR: return "chair";
        case IDs.HOSPITAL_CHAIR: return "chair";
        case IDs.CHAR_CHAIR: return "chair";
        case IDs.TABLE: return "table";
        case IDs.CHAR_TABLE: return "table";
        case IDs.NIGHT_TABLE: return "night table";
        case IDs.HOSPITAL_NIGHT_TABLE: return "night table";
        case IDs.DRAWER: return "drawer";
        case IDs.FRIDGE: return "fridge";
        case IDs.WARDROBE: return "wardrobe";
        case IDs.HOSPITAL_WARDROBE: return "wardrobe";
        case IDs.CAR1: return "wrecked car";
        case IDs.CAR2: return "wrecked car";
        case IDs.CAR3: return "wrecked car";
        case IDs.CAR4: return "wrecked car";
        case IDs.SHOP_SHELF: return "shelf";
        case IDs.JUNK: return "junk";
        case IDs.BARRELS: return "barrels";
//      case MapObject.IDs.: return Gameplay.GameImages.;
        default: throw new ArgumentOutOfRangeException(nameof(x), x, "not mapped to an MapObject ID");
      }
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
      GARDEN_FENCE,
      WIRE_FENCE,
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
        case MapObject.IDs.GARDEN_FENCE: return Gameplay.GameImages.OBJ_GARDEN_FENCE;
        case MapObject.IDs.WIRE_FENCE: return Gameplay.GameImages.OBJ_WIRE_FENCE;
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
        case Gameplay.GameImages.OBJ_GARDEN_FENCE: return MapObject.IDs.GARDEN_FENCE;
        case Gameplay.GameImages.OBJ_WIRE_FENCE: return MapObject.IDs.WIRE_FENCE;
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
