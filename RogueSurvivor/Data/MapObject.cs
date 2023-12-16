// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.MapObject
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Engine.MapObjects;
using System;
using System.Linq;
using System.Runtime.Serialization;
using Zaimoni.Data;

using ItemBarricadeMaterial = djack.RogueSurvivor.Engine.Items.ItemBarricadeMaterial;
using Point = Zaimoni.Data.Vector2D<short>;

namespace djack.RogueSurvivor.Data
{
/*
 MapObject inventory is up for a redesign.
 * we would like on-shelf inventory to be independent of non-walkability.
 * we would like the item get UI to allow take/trade with adjacent tables, but not the floor inventory underneath those tables
 * we would like actors to have enough item memory to track seen *internal* inventory (not yet implemented)
 */
  [Serializable]
  internal class MapObject : ILocation, INoun, Zaimoni.Serialization.ISerialize
  {
    public const int CAR_WEIGHT = 100;
    public const int MAX_NORMAL_WEIGHT = 10;

    private IDs m_ID;
    private Break m_BreakState;
    private int m_HitPoints;
    private Fire m_FireState;
    [NonSerialized] private Location m_Location;

    Model.MapObject Model { get => Data.Model.MapObject.from((int)m_ID); }
    public byte Weight { get => Model.Weight; }
    public int MaxHitPoints { get => Model.MaxHitPoints; }

    public string AName { get => IsPlural ? _ID_Name(m_ID).PrefixIndefinitePluralArticle() : _ID_Name(m_ID).PrefixIndefiniteSingularArticle(); }
    public string TheName { get => _ID_Name(m_ID).PrefixDefiniteSingularArticle(); }
    public string UnknownPronoun { get => "it"; }
    public bool IsPlural { get => GetFlag(Data.Model.MapObject.Flags.IS_PLURAL); }

    public IDs ID {
      get => m_ID;
      set {
#if DEBUG
        if (!_IDchangeIsLegal(value)) throw new ArgumentOutOfRangeException(nameof(value), "!_IDchangeIsLegal(value)");
#endif
        m_ID = value;
      }
    }

    public virtual string ImageID { get => m_ID.ImageID(); }
    public string HiddenImageID { get => m_ID.ImageID(); }

#nullable enable
    public Location Location {
      get => m_Location;
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
        return GetFlag(Data.Model.MapObject.Flags.IS_MATERIAL_TRANSPARENT);
      }
    }

    public bool IsMaterialTransparent { get => GetFlag(Data.Model.MapObject.Flags.IS_MATERIAL_TRANSPARENT); }
    virtual public bool IsWalkable { get => GetFlag(Data.Model.MapObject.Flags.IS_WALKABLE); }
    public int JumpLevel { get => (m_FireState != Fire.ONFIRE) ? Model.jumpLevel : 0; }
    public bool IsJumpable { get => 0 < JumpLevel; }
    public bool IsCouch { get => GetFlag(Data.Model.MapObject.Flags.IS_COUCH); }
    public bool IsBreakable { get => m_BreakState == Break.BREAKABLE; }

    // 2023-04-16: a broken object still retains its shelf-like status
    public Break BreakState {
      get { return m_BreakState; }
      protected set { // Cf IsTransparent which affects LOS calculations
        Break old = m_BreakState;
        m_BreakState = value;
        bool now_broken = Break.BROKEN == value;
        if ((Break.BROKEN == old)!= now_broken) InvalidateLOS();
#if OBSOLETE
        if (now_broken) {
          if (null != m_Inventory) {
            if (!m_Inventory.IsEmpty) {
              // cf. RogueGame::KillActor
              foreach (Item it in m_Inventory.Items.ToArray()) {
                if (it.IsUseless) continue;   // if the drop command/behavior would trigger discard instead, omit
                if (it.Model.IsUnbreakable || it.IsUnique || Engine.Rules.Get.RollChance(Engine.RogueGame.ItemSurviveKillProbability(it, "suicide"))) { // not really, but this keys the most lenient drop probabilities
                  m_Inventory.RemoveAllQuantity(it);
                  Location.Drop(it);
                }
              }
              Engine.Session.Get.Police.Investigate.Record(Location);  // cheating ai: police consider death drops tourism targets
            }
            m_Inventory = null;
          }
        }
#endif
      }
    }

    protected void _break()
    {
      BreakState = Break.BROKEN;    // use accessor to trigger LOS invalidation
      m_HitPoints = 0;
    }

    public bool GivesWood {
      get => GetFlag(Data.Model.MapObject.Flags.GIVES_WOOD) && Break.BROKEN != m_BreakState;
    }

    public bool IsMovable { get => GetFlag(Data.Model.MapObject.Flags.IS_MOVABLE); }
    public bool BreaksWhenFiredThrough { get => GetFlag(Data.Model.MapObject.Flags.BREAKS_WHEN_FIRED_THROUGH); }
    public bool StandOnFovBonus { get => GetFlag(Data.Model.MapObject.Flags.STANDON_FOV_BONUS); }
    public bool IsFlammable { get { return m_FireState == Fire.ONFIRE || m_FireState == Fire.BURNABLE; } }
    public bool IsOnFire { get { return m_FireState == Fire.ONFIRE; } }
    public bool IsBurntToAshes { get { return m_FireState == Fire.ASHES; } }

    public Fire FireState {
      get { return m_FireState; }
      set { // cf IsTransparent which affects LOS calculations
        Fire old = m_FireState;
        m_FireState = value;
        if ((Fire.ONFIRE == old) != (Fire.ONFIRE == value)) {
          InvalidateLOS();
          m_Location.Map?.RebuildClearableZones(m_Location.Position);
        } else if ((Fire.ASHES == old) != (Fire.ASHES == value)) {
          InvalidateLOS();
        }
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
      if (   0 >= hp    // insignificant damage
          || 0 >= MaxHitPoints) return false;  // object is made of indestructible unobtainium
      if (hp < m_HitPoints) {
        m_HitPoints -= hp;
        return false;
      }
      Destroy();
      return true;
    }

    // This section of private switch statements arguably could designate properties of a MapObjectModel class.
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

    virtual public bool CoversTraps { get { return IsJumpable || IsWalkable; } }
    virtual public bool TriggersTraps { get { return !IsJumpable && !IsWalkable; } }

    virtual public bool BlocksLivingPathfinding { get {
      return !IsJumpable && !IsWalkable && (!IsMovable || IsOnFire);
    } }

    protected MapObject(string hiddenImageID, Fire burnable = Fire.UNINFLAMMABLE)
    {
#if DEBUG
      if (string.IsNullOrEmpty(hiddenImageID)) throw new ArgumentNullException(nameof(hiddenImageID));
#endif
      m_ID = hiddenImageID.MapObject_ID();
      m_BreakState = (0==Model.MaxHitPoints ? Break.UNBREAKABLE : Break.BREAKABLE); // breakable := nonzero max hp
      m_FireState = burnable;   // XXX should be able to infer burnable from gives wood; other materials may also be burnable (need fire spread for this to matter)
      m_HitPoints = MaxHitPoints;

      if (_ID_StartsBroken(m_ID)) m_BreakState = Break.BROKEN;
    }

#region implement Zaimoni.Serialization.ISerialize
    protected MapObject(Zaimoni.Serialization.DecodeObjects decode) {
        byte stage_byte = 0;
        Zaimoni.Serialization.Formatter.Deserialize(decode.src, ref stage_byte);
        m_ID = (IDs)stage_byte;
        Zaimoni.Serialization.Formatter.Deserialize(decode.src, ref stage_byte);
        m_BreakState = (Break)(stage_byte / max_Fire);
        m_FireState = (Fire)(stage_byte % max_Fire);
        Zaimoni.Serialization.Formatter.Deserialize(decode.src, ref m_HitPoints);
    }

    protected void save(Zaimoni.Serialization.EncodeObjects encode) {
        Zaimoni.Serialization.Formatter.Serialize(encode.dest, (byte)m_ID);
        int crm_code = (int)m_BreakState*max_Fire+(int)m_FireState;
        Zaimoni.Serialization.Formatter.Serialize(encode.dest, (byte)crm_code);
        Zaimoni.Serialization.Formatter.Serialize(encode.dest, m_HitPoints);
    }

    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode) {
        Zaimoni.Serialization.Formatter.Serialize(encode.dest, (byte)m_ID);
        int crm_code = (int)m_BreakState*max_Fire+(int)m_FireState;
        Zaimoni.Serialization.Formatter.Serialize(encode.dest, (byte)crm_code);
        Zaimoni.Serialization.Formatter.Serialize(encode.dest, m_HitPoints);
    }
#endregion


    static public MapObject create(string hiddenImageID, Fire burnable = Fire.UNINFLAMMABLE)
    {
#if DEBUG
        if (hiddenImageID.MapObject_ID().HasShelf()) throw new InvalidOperationException("ID.HasShelf()");
#endif
        return new MapObject(hiddenImageID, burnable);
    }

    public void RepairLoad(Map m, Point pos)
    {
      if (null == m_Location.Map && null != m) m_Location = new Location(m, pos);
#if DEBUG
      else throw new InvalidOperationException("location repair rejected");
#endif
    }

    private bool _IDchangeIsLegal(IDs dest)
    {
      if (IDs.IRON_GATE_CLOSED == dest && IDs.IRON_GATE_OPEN   == m_ID) return true;
      if (IDs.IRON_GATE_OPEN   == dest && IDs.IRON_GATE_CLOSED == m_ID) return true;
      return false;
    }
#nullable restore

    protected void InvalidateLOS()
    {
      if (null != m_Location.Map) Engine.LOS.Validate(m_Location.Map,los => !los.Contains(m_Location.Position));
    }

#nullable enable
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

#if DEAD_FUNC
    public bool CanPushTo(Point toPos, out string reason) { return string.IsNullOrEmpty(reason = ReasonCantPushTo(toPos)); }
#endif
    public bool CanPushTo(Point toPos) { return string.IsNullOrEmpty(ReasonCantPushTo(toPos)); }

    /// <param name="to">Assumed in canonical form (in-bounds)</param>
    public bool CanPushTo(in Location to, out string reason) { return string.IsNullOrEmpty(reason = ReasonCantPushTo(in to)); }
    /// <param name="to">Assumed in canonical form (in-bounds)</param>
    public bool CanPushTo(in Location to) { return string.IsNullOrEmpty(ReasonCantPushTo(in to)); }
    public void PlaceAt(Map m, in Point pos) {m.PlaceAt(this, pos);} // this guaranteed non-null so non-null precondition ok
#nullable restore

    public void Remove()
    {
      if (null == Location.Map) return;
      Location.Map.RemoveMapObjectAt(Location.Position);
    }

    protected virtual void _destroy() { Remove(); }   // subtypes that do not simply vanish must override

    public void Destroy()
    {
      bool must_rebuild_clearable_zones = BlocksLivingPathfinding;
      m_HitPoints = 0;
      if (GivesWood) {
        int val2 = 1 + MaxHitPoints / 40;
        while (val2 > 0) {
          var qty = Math.Min(Gameplay.GameItems.WOODENPLANK.StackingLimit, val2);
          Location.Drop(new ItemBarricadeMaterial(Gameplay.GameItems.WOODENPLANK, qty));
          val2 -= qty;
        }
        var rules = Engine.Rules.Get;
        if (rules.RollChance(Engine.Rules.IMPROVED_WEAPONS_FROM_BROKEN_WOOD_CHANCE)) {
          Location.Drop((rules.RollChance(50) ? Gameplay.GameItems.IMPROVISED_CLUB : Gameplay.GameItems.IMPROVISED_SPEAR).create());
        }
      }
      _destroy();
      if (must_rebuild_clearable_zones) m_Location.Map.RebuildClearableZones(m_Location.Position);
      Engine.RogueGame.Game.OnLoudNoise(m_Location, "A loud *CRASH*");
    }

    // flag handling
    private bool GetFlag(Model.MapObject.Flags f) => default != (Model.flags & f);

    // fire
    public void Ignite() => FireState = Fire.ONFIRE;
    public void Extinguish() => FireState = Fire.BURNABLE;

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
    private const int max_Fire = (int)Fire.ASHES+1;

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

#nullable enable
  [Serializable]
  internal sealed class ShelfLike : MapObject, IInventory, Zaimoni.Serialization.ISerialize
    {
    private readonly Inventory m_Inventory;
    public Inventory Inventory { get => m_Inventory; }
    public Inventory? NonEmptyInventory { get => m_Inventory.IsEmpty ? null : m_Inventory; }

    public ShelfLike(string hiddenImageID, Fire burnable = Fire.UNINFLAMMABLE) : base(hiddenImageID, burnable)
    {
#if DEBUG
      if (!ID.HasShelf()) throw new InvalidOperationException("!ID.HasShelf()");
#endif

      m_Inventory = new Inventory(Map.GROUND_INVENTORY_SLOTS);
    }

#region implement Zaimoni.Serialization.ISerialize
    protected ShelfLike(Zaimoni.Serialization.DecodeObjects decode) : base(decode) {
        m_Inventory = decode.Load<Inventory>(out var code);
        if (null == m_Inventory) {
            if (0 < code) {
                m_Inventory = new(1);
                decode.Schedule(code, (o) => {
                    if (o is Inventory w) w.DestructiveTransferAll(m_Inventory);
                    else throw new InvalidOperationException("Inventory object not loaded");
                });
            } else throw new InvalidOperationException("Inventory object not loaded");
        }
    }

    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode) {
        base.save(encode);
        var code = encode.Saving(m_Inventory); // obligatory, in spite of type prefix/suffix
        if (0 < code) Zaimoni.Serialization.Formatter.SerializeObjCode(encode.dest, code);
        else throw new ArgumentNullException(nameof(m_Inventory));
    }
#endregion

#if DEBUG
    [OnSerializing] private void OptimizeBeforeSaving(StreamingContext context)
    {
      m_Inventory.RepairZeroQty();
    }
#endif

    public void TransferFrom(Item it, Inventory dest) { m_Inventory.Transfer(it, dest); }

    public string ReasonCantPutItemIn(Actor actor)
    {
      if (   !actor.Model.Abilities.HasInventory
          || !actor.Model.Abilities.CanUseMapObjects
          ||  actor.Inventory == null)
          return "cannot take an item";
      if (m_Inventory.IsFull) return "container is full";
      return "";
    }

    public bool PutItemIn(Item gift)
    {
      if (gift is Engine.Items.ItemTrap trap) trap.Desactivate();    // alpha10
      return m_Inventory.AddAll(gift);
    }

    protected override void _destroy() {
      if (!m_Inventory.IsEmpty) {
        foreach (Item it in m_Inventory.Items.ToArray()) {
          if (it.IsUseless) continue;   // if the drop command/behavior would trigger discard instead, omit
          Location.Drop(it);
        }
        m_Inventory.Clear(); // if it wasn't dropped, it's gone; disallows cross-linking
      }
      base._destroy();
    }
  }
#nullable restore

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

    // i.e.: has a flat surface at roughly arm height, so no crouching needed to reach (unlike ground inventory)
    static public bool HasShelf(this MapObject.IDs x)
    {
      switch (x) {
         // 2023-04-27 revised in to make crouching stance not trigger
        case MapObject.IDs.CHAIR: return true;
        case MapObject.IDs.HOSPITAL_CHAIR: return true;
        case MapObject.IDs.CHAR_CHAIR: return true;
        case MapObject.IDs.TABLE: return true;
        case MapObject.IDs.CHAR_TABLE: return true;
        case MapObject.IDs.NIGHT_TABLE: return true;
        case MapObject.IDs.HOSPITAL_NIGHT_TABLE: return true;
         // 2023-04-27 end revised in to make crouching stance not trigger
        case MapObject.IDs.DRAWER: return true;
        case MapObject.IDs.FRIDGE: return true;
        case MapObject.IDs.WARDROBE: return true;
        case MapObject.IDs.HOSPITAL_WARDROBE: return true;
        case MapObject.IDs.SHOP_SHELF: return true;
//      case IDs.: return true;
        default: return false;
      }
    }

    static public bool BlocksReachInto(this MapObject obj) {
        if (null != obj) {
            if (obj.BlocksLivingPathfinding) return true;
            if (obj is DoorWindow door && door.IsClosed) return true;
        }
        return false;
    }
  } // MapObject_ext
}
