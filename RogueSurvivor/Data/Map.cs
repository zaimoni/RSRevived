// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Map
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Map : ISerializable
  {
    public const int GROUND_INVENTORY_SLOTS = 10;
    private int m_Seed;
    private District m_District;
    private string m_Name;
    private Lighting m_Lighting;
    private WorldTime m_LocalTime;
    private int m_Width;
    private int m_Height;
    private Rectangle m_Rectangle;
    private Tile[,] m_Tiles;
    private Dictionary<Point, Exit> m_Exits;
    private List<Zone> m_Zones;
    private List<Actor> m_ActorsList;
    private int m_iCheckNextActorIndex;
    private List<MapObject> m_MapObjectsList;
    private Dictionary<Point, Inventory> m_GroundItemsByPosition;
    private List<Corpse> m_CorpsesList;
    private List<OdorScent> m_Scents;
    private List<TimedTask> m_Timers;
    [NonSerialized]
    private Dictionary<Point, Actor> m_aux_ActorsByPosition;
    [NonSerialized]
    private Dictionary<Point, MapObject> m_aux_MapObjectsByPosition;
    [NonSerialized]
    private List<Inventory> m_aux_GroundItemsList;
    [NonSerialized]
    private Dictionary<Point, List<Corpse>> m_aux_CorpsesByPosition;
    [NonSerialized]
    private Dictionary<Point, List<OdorScent>> m_aux_ScentsByPosition;

    public District District
    {
      get
      {
        return this.m_District;
      }
      set
      {
        this.m_District = value;
      }
    }

    public string Name
    {
      get
      {
        return this.m_Name;
      }
      set
      {
        this.m_Name = value;
      }
    }

    public int Seed
    {
      get
      {
        return this.m_Seed;
      }
    }

    public bool IsSecret { get; set; }

    public Lighting Lighting
    {
      get
      {
        return this.m_Lighting;
      }
      set
      {
        this.m_Lighting = value;
      }
    }

    public WorldTime LocalTime
    {
      get
      {
        return this.m_LocalTime;
      }
    }

    public int Width
    {
      get
      {
        return this.m_Width;
      }
    }

    public int Height
    {
      get
      {
        return this.m_Height;
      }
    }

    public Rectangle Rect
    {
      get
      {
        return this.m_Rectangle;
      }
    }

    public IEnumerable<Zone> Zones
    {
      get
      {
        return (IEnumerable<Zone>) this.m_Zones;
      }
    }

    public IEnumerable<Exit> Exits
    {
      get
      {
        return (IEnumerable<Exit>) this.m_Exits.Values;
      }
    }

    public int CountExits
    {
      get
      {
        if (this.m_Exits.Values != null)
          return this.m_Exits.Values.Count;
        return 0;
      }
    }

    public IEnumerable<Actor> Actors
    {
      get
      {
        return (IEnumerable<Actor>) this.m_ActorsList;
      }
    }

    public int CountActors
    {
      get
      {
        return this.m_ActorsList.Count;
      }
    }

    public int CheckNextActorIndex
    {
      get
      {
        return this.m_iCheckNextActorIndex;
      }
      set
      {
        this.m_iCheckNextActorIndex = value;
      }
    }

    public IEnumerable<MapObject> MapObjects
    {
      get
      {
        return (IEnumerable<MapObject>) this.m_MapObjectsList;
      }
    }

    public IEnumerable<Inventory> GroundInventories
    {
      get
      {
        return (IEnumerable<Inventory>) this.m_aux_GroundItemsList;
      }
    }

    public IEnumerable<Corpse> Corpses
    {
      get
      {
        return (IEnumerable<Corpse>) this.m_CorpsesList;
      }
    }

    public int CountCorpses
    {
      get
      {
        return this.m_CorpsesList.Count;
      }
    }

    public IEnumerable<TimedTask> Timers
    {
      get
      {
        return (IEnumerable<TimedTask>) this.m_Timers;
      }
    }

    public int CountTimers
    {
      get
      {
        if (this.m_Timers != null)
          return this.m_Timers.Count;
        return 0;
      }
    }

    public IEnumerable<OdorScent> Scents
    {
      get
      {
        return (IEnumerable<OdorScent>) this.m_Scents;
      }
    }

    public Map(int seed, string name, int width, int height)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (width <= 0)
        throw new ArgumentOutOfRangeException("width <=0");
      if (height <= 0)
        throw new ArgumentOutOfRangeException("height <=0");
      this.m_Seed = seed;
      this.m_Name = name;
      this.m_Width = width;
      this.m_Height = height;
      this.m_Rectangle = new Rectangle(0, 0, width, height);
      this.m_LocalTime = new WorldTime();
      this.Lighting = Lighting.OUTSIDE;
      this.IsSecret = false;
      this.m_Tiles = new Tile[width, height];
      for (int index1 = 0; index1 < width; ++index1)
      {
        for (int index2 = 0; index2 < height; ++index2)
          this.m_Tiles[index1, index2] = new Tile(TileModel.UNDEF);
      }
      this.m_Exits = new Dictionary<Point, Exit>();
      this.m_Zones = new List<Zone>(5);
      this.m_aux_ActorsByPosition = new Dictionary<Point, Actor>(5);
      this.m_ActorsList = new List<Actor>(5);
      this.m_aux_MapObjectsByPosition = new Dictionary<Point, MapObject>(5);
      this.m_MapObjectsList = new List<MapObject>(5);
      this.m_GroundItemsByPosition = new Dictionary<Point, Inventory>(5);
      this.m_aux_GroundItemsList = new List<Inventory>(5);
      this.m_CorpsesList = new List<Corpse>(5);
      this.m_aux_CorpsesByPosition = new Dictionary<Point, List<Corpse>>(5);
      this.m_Scents = new List<OdorScent>(128);
      this.m_aux_ScentsByPosition = new Dictionary<Point, List<OdorScent>>(128);
      this.m_Timers = new List<TimedTask>(5);
    }

    protected Map(SerializationInfo info, StreamingContext context)
    {
      this.m_Seed = (int) info.GetValue("m_Seed", typeof (int));
      this.m_District = (District) info.GetValue("m_District", typeof (District));
      this.m_Name = (string) info.GetValue("m_Name", typeof (string));
      this.m_LocalTime = (WorldTime) info.GetValue("m_LocalTime", typeof (WorldTime));
      this.m_Width = (int) info.GetValue("m_Width", typeof (int));
      this.m_Height = (int) info.GetValue("m_Height", typeof (int));
      this.m_Rectangle = (Rectangle) info.GetValue("m_Rectangle", typeof (Rectangle));
      this.m_Tiles = (Tile[,]) info.GetValue("m_Tiles", typeof (Tile[,]));
      this.m_Exits = (Dictionary<Point, Exit>) info.GetValue("m_Exits", typeof (Dictionary<Point, Exit>));
      this.m_Zones = (List<Zone>) info.GetValue("m_Zones", typeof (List<Zone>));
      this.m_ActorsList = (List<Actor>) info.GetValue("m_ActorsList", typeof (List<Actor>));
      this.m_MapObjectsList = (List<MapObject>) info.GetValue("m_MapObjectsList", typeof (List<MapObject>));
      this.m_GroundItemsByPosition = (Dictionary<Point, Inventory>) info.GetValue("m_GroundItemsByPosition", typeof (Dictionary<Point, Inventory>));
      this.m_CorpsesList = (List<Corpse>) info.GetValue("m_CorpsesList", typeof (List<Corpse>));
      this.m_Lighting = (Lighting) info.GetValue("m_Lighting", typeof (Lighting));
      this.m_Scents = (List<OdorScent>) info.GetValue("m_Scents", typeof (List<OdorScent>));
      this.m_Timers = (List<TimedTask>) info.GetValue("m_Timers", typeof (List<TimedTask>));
    }

    public bool IsInBounds(int x, int y)
    {
      if (x >= 0 && x < this.m_Width && y >= 0)
        return y < this.m_Height;
      return false;
    }

    public bool IsInBounds(Point p)
    {
      return this.IsInBounds(p.X, p.Y);
    }

    public void TrimToBounds(ref int x, ref int y)
    {
      if (x < 0)
        x = 0;
      else if (x > this.m_Width - 1)
        x = this.m_Width - 1;
      if (y < 0)
      {
        y = 0;
      }
      else
      {
        if (y <= this.m_Height - 1)
          return;
        y = this.m_Height - 1;
      }
    }

    public void TrimToBounds(ref Point p)
    {
      if (p.X < 0)
        p.X = 0;
      else if (p.X > this.m_Width - 1)
        p.X = this.m_Width - 1;
      if (p.Y < 0)
      {
        p.Y = 0;
      }
      else
      {
        if (p.Y <= this.m_Height - 1)
          return;
        p.Y = this.m_Height - 1;
      }
    }

    public bool IsMapBoundary(int x, int y)
    {
      if (x != -1 && x != this.m_Width && y != -1)
        return y == this.m_Height;
      return true;
    }

    public bool IsOnMapBorder(int x, int y)
    {
      if (x != 0 && x != this.m_Width - 1 && y != 0)
        return y == this.m_Height - 1;
      return true;
    }

    public Tile GetTileAt(int x, int y)
    {
      return this.m_Tiles[x, y];
    }

    public Tile GetTileAt(Point p)
    {
      return this.m_Tiles[p.X, p.Y];
    }

    public void SetTileModelAt(int x, int y, TileModel model)
    {
      if (!this.IsInBounds(x, y))
        throw new ArgumentOutOfRangeException("position out of map bounds");
      if (model == null)
        throw new ArgumentNullException("model");
      this.m_Tiles[x, y].Model = model;
    }

    public Exit GetExitAt(Point pos)
    {
      Exit exit;
      if (this.m_Exits.TryGetValue(pos, out exit))
        return exit;
      return (Exit) null;
    }

    public Exit GetExitAt(int x, int y)
    {
      return this.GetExitAt(new Point(x, y));
    }

    public void SetExitAt(Point pos, Exit exit)
    {
      this.m_Exits.Add(pos, exit);
    }

    public void RemoveExitAt(Point pos)
    {
      this.m_Exits.Remove(pos);
    }

    public bool HasAnExitIn(Rectangle rect)
    {
      for (int left = rect.Left; left < rect.Right; ++left)
      {
        for (int top = rect.Top; top < rect.Bottom; ++top)
        {
          if (this.GetExitAt(left, top) != null)
            return true;
        }
      }
      return false;
    }

    public Point? GetExitPos(Exit exit)
    {
      if (exit == null)
        return new Point?();
      foreach (KeyValuePair<Point, Exit> mExit in this.m_Exits)
      {
        if (mExit.Value == exit)
          return new Point?(mExit.Key);
      }
      return new Point?();
    }

    public void AddZone(Zone zone)
    {
      this.m_Zones.Add(zone);
    }

    public void RemoveZone(Zone zone)
    {
      this.m_Zones.Remove(zone);
    }

    public void RemoveAllZonesAt(int x, int y)
    {
      List<Zone> zonesAt = this.GetZonesAt(x, y);
      if (zonesAt == null)
        return;
      foreach (Zone zone in zonesAt)
        this.RemoveZone(zone);
    }

    public List<Zone> GetZonesAt(int x, int y)
    {
      List<Zone> zoneList = (List<Zone>) null;
      foreach (Zone mZone in this.m_Zones)
      {
        if (mZone.Bounds.Contains(x, y))
        {
          if (zoneList == null)
            zoneList = new List<Zone>(this.m_Zones.Count / 4);
          zoneList.Add(mZone);
        }
      }
      return zoneList;
    }

    public Zone GetZoneByName(string name)
    {
      foreach (Zone mZone in this.m_Zones)
      {
        if (mZone.Name == name)
          return mZone;
      }
      return (Zone) null;
    }

    public Zone GetZoneByPartialName(string partOfname)
    {
      foreach (Zone mZone in this.m_Zones)
      {
        if (mZone.Name.Contains(partOfname))
          return mZone;
      }
      return (Zone) null;
    }

    public bool HasZonePartiallyNamedAt(Point pos, string partOfName)
    {
      List<Zone> zonesAt = this.GetZonesAt(pos.X, pos.Y);
      if (zonesAt == null)
        return false;
      foreach (Zone zone in zonesAt)
      {
        if (zone.Name.Contains(partOfName))
          return true;
      }
      return false;
    }

    public bool HasActor(Actor actor)
    {
      return this.m_ActorsList.Contains(actor);
    }

    public Actor GetActor(int index)
    {
      return this.m_ActorsList[index];
    }

    public Actor GetActorAt(Point position)
    {
      Actor actor;
      if (this.m_aux_ActorsByPosition.TryGetValue(position, out actor))
        return actor;
      return (Actor) null;
    }

    public Actor GetActorAt(int x, int y)
    {
      return this.GetActorAt(new Point(x, y));
    }

    public void PlaceActorAt(Actor actor, Point position)
    {
      if (actor == null)
        throw new ArgumentNullException("actor");
      Actor actorAt = this.GetActorAt(position);
      if (actorAt == actor)
        throw new InvalidOperationException("actor already at position");
      if (actorAt != null)
        throw new InvalidOperationException("another actor already at position");
      if (!this.IsInBounds(position.X, position.Y))
        throw new ArgumentOutOfRangeException("position out of map bounds");
      if (this.HasActor(actor))
        this.m_aux_ActorsByPosition.Remove(actor.Location.Position);
      else
        this.m_ActorsList.Add(actor);
      this.m_aux_ActorsByPosition.Add(position, actor);
      actor.Location = new Location(this, position);
      this.m_iCheckNextActorIndex = 0;
    }

    public void MoveActorToFirstPosition(Actor actor)
    {
      if (!this.m_ActorsList.Contains(actor))
        throw new ArgumentException("actor not in map");
      this.m_ActorsList.Remove(actor);
      if (this.m_ActorsList.Count == 0)
        this.m_ActorsList.Add(actor);
      else
        this.m_ActorsList.Insert(0, actor);
      this.m_iCheckNextActorIndex = 0;
    }

    public void RemoveActor(Actor actor)
    {
      if (!this.m_ActorsList.Contains(actor))
        return;
      this.m_ActorsList.Remove(actor);
      this.m_aux_ActorsByPosition.Remove(actor.Location.Position);
      this.m_iCheckNextActorIndex = 0;
    }

    public bool HasMapObject(MapObject mapObj)
    {
      return this.m_MapObjectsList.Contains(mapObj);
    }

    public MapObject GetMapObjectAt(Point position)
    {
      MapObject mapObject;
      if (this.m_aux_MapObjectsByPosition.TryGetValue(position, out mapObject))
        return mapObject;
      return (MapObject) null;
    }

    public MapObject GetMapObjectAt(int x, int y)
    {
      return this.GetMapObjectAt(new Point(x, y));
    }

    public void PlaceMapObjectAt(MapObject mapObj, Point position)
    {
      if (mapObj == null)
        throw new ArgumentNullException("actor");
      MapObject mapObjectAt = this.GetMapObjectAt(position);
      if (mapObjectAt == mapObj)
        return;
      if (mapObjectAt == mapObj)
        throw new InvalidOperationException("mapObject already at position");
      if (mapObjectAt != null)
        throw new InvalidOperationException("another mapObject already at position");
      if (!this.IsInBounds(position.X, position.Y))
        throw new ArgumentOutOfRangeException("position out of map bounds");
      if (!this.GetTileAt(position.X, position.Y).Model.IsWalkable)
        throw new InvalidOperationException("cannot place map objects on unwalkable tiles");
      if (this.HasMapObject(mapObj))
        this.m_aux_MapObjectsByPosition.Remove(mapObj.Location.Position);
      else
        this.m_MapObjectsList.Add(mapObj);
      this.m_aux_MapObjectsByPosition.Add(position, mapObj);
      mapObj.Location = new Location(this, position);
    }

    public void RemoveMapObjectAt(int x, int y)
    {
      MapObject mapObjectAt = this.GetMapObjectAt(x, y);
      if (mapObjectAt == null)
        return;
      this.m_MapObjectsList.Remove(mapObjectAt);
      this.m_aux_MapObjectsByPosition.Remove(new Point(x, y));
    }

    public Inventory GetItemsAt(Point position)
    {
      if (!this.IsInBounds(position))
        return (Inventory) null;
      Inventory inventory;
      if (this.m_GroundItemsByPosition.TryGetValue(position, out inventory))
        return inventory;
      return (Inventory) null;
    }

    public Inventory GetItemsAt(int x, int y)
    {
      return this.GetItemsAt(new Point(x, y));
    }

    public Point? GetGroundInventoryPosition(Inventory groundInv)
    {
      foreach (KeyValuePair<Point, Inventory> keyValuePair in this.m_GroundItemsByPosition)
      {
        if (keyValuePair.Value == groundInv)
          return new Point?(keyValuePair.Key);
      }
      return new Point?();
    }

    public void DropItemAt(Item it, Point position)
    {
      if (it == null)
        throw new ArgumentNullException("item");
      if (!this.IsInBounds(position))
        throw new ArgumentOutOfRangeException("position out of map bounds");
      Inventory itemsAt = this.GetItemsAt(position);
      if (itemsAt == null)
      {
        Inventory inventory = new Inventory(10);
        this.m_aux_GroundItemsList.Add(inventory);
        this.m_GroundItemsByPosition.Add(position, inventory);
        inventory.AddAll(it);
      }
      else if (itemsAt.IsFull)
      {
        int quantity = it.Quantity;
        int quantityAdded;
        itemsAt.AddAsMuchAsPossible(it, out quantityAdded);
        if (quantityAdded >= quantity)
          return;
        itemsAt.RemoveAllQuantity(itemsAt.BottomItem);
        itemsAt.AddAsMuchAsPossible(it, out quantityAdded);
      }
      else
        itemsAt.AddAll(it);
    }

    public void DropItemAt(Item it, int x, int y)
    {
      this.DropItemAt(it, new Point(x, y));
    }

    public void RemoveItemAt(Item it, Point position)
    {
      if (it == null)
        throw new ArgumentNullException("item");
      if (!this.IsInBounds(position))
        throw new ArgumentOutOfRangeException("position out of map bounds");
      Inventory itemsAt = this.GetItemsAt(position);
      if (itemsAt == null)
        throw new ArgumentException("no items at this position");
      if (!itemsAt.Contains(it))
        throw new ArgumentException("item not at this position");
      itemsAt.RemoveAllQuantity(it);
      if (!itemsAt.IsEmpty)
        return;
      this.m_GroundItemsByPosition.Remove(position);
      this.m_aux_GroundItemsList.Remove(itemsAt);
    }

    public void RemoveItemAt(Item it, int x, int y)
    {
      this.RemoveItemAt(it, new Point(x, y));
    }

    public void RemoveAllItemsAt(Point position)
    {
      Inventory itemsAt = this.GetItemsAt(position);
      if (itemsAt == null)
        return;
      this.m_GroundItemsByPosition.Remove(position);
      this.m_aux_GroundItemsList.Remove(itemsAt);
    }

    public List<Corpse> GetCorpsesAt(Point p)
    {
      List<Corpse> corpseList;
      if (this.m_aux_CorpsesByPosition.TryGetValue(p, out corpseList))
        return corpseList;
      return (List<Corpse>) null;
    }

    public List<Corpse> GetCorpsesAt(int x, int y)
    {
      return this.GetCorpsesAt(new Point(x, y));
    }

    public bool HasCorpse(Corpse c)
    {
      return this.m_CorpsesList.Contains(c);
    }

    public void AddCorpseAt(Corpse c, Point p)
    {
      if (this.m_CorpsesList.Contains(c))
        throw new ArgumentException("corpse already in this map");
      c.Position = p;
      this.m_CorpsesList.Add(c);
      this.InsertCorpseAtPos(c);
      c.DeadGuy.Location = new Location(this, p);
    }

    public void MoveCorpseTo(Corpse c, Point newPos)
    {
      if (!this.m_CorpsesList.Contains(c))
        throw new ArgumentException("corpse not in this map");
      this.RemoveCorpseFromPos(c);
      c.Position = newPos;
      this.InsertCorpseAtPos(c);
      c.DeadGuy.Location = new Location(this, newPos);
    }

    public void RemoveCorpse(Corpse c)
    {
      if (!this.m_CorpsesList.Contains(c))
        throw new ArgumentException("corpse not in this map");
      this.m_CorpsesList.Remove(c);
      this.RemoveCorpseFromPos(c);
    }

    public bool TryRemoveCorpseOf(Actor a)
    {
      foreach (Corpse mCorpses in this.m_CorpsesList)
      {
        if (mCorpses.DeadGuy == a)
        {
          this.RemoveCorpse(mCorpses);
          return true;
        }
      }
      return false;
    }

    private void RemoveCorpseFromPos(Corpse c)
    {
      List<Corpse> corpseList;
      if (!this.m_aux_CorpsesByPosition.TryGetValue(c.Position, out corpseList))
        return;
      corpseList.Remove(c);
      if (corpseList.Count != 0)
        return;
      this.m_aux_CorpsesByPosition.Remove(c.Position);
    }

    private void InsertCorpseAtPos(Corpse c)
    {
      List<Corpse> corpseList;
      if (this.m_aux_CorpsesByPosition.TryGetValue(c.Position, out corpseList))
        corpseList.Insert(0, c);
      else
        this.m_aux_CorpsesByPosition.Add(c.Position, new List<Corpse>(1) { c });
    }

    public void AddTimer(TimedTask t)
    {
      if (this.m_Timers == null)
        this.m_Timers = new List<TimedTask>(5);
      this.m_Timers.Add(t);
    }

    public void RemoveTimer(TimedTask t)
    {
      this.m_Timers.Remove(t);
    }

    public int GetScentByOdorAt(Odor odor, Point position)
    {
      if (!this.IsInBounds(position))
        return 0;
      OdorScent scentByOdor = this.GetScentByOdor(odor, position);
      if (scentByOdor != null)
        return scentByOdor.Strength;
      return 0;
    }

    private OdorScent GetScentByOdor(Odor odor, Point p)
    {
      List<OdorScent> odorScentList;
      if (!this.m_aux_ScentsByPosition.TryGetValue(p, out odorScentList))
        return (OdorScent) null;
      foreach (OdorScent odorScent in odorScentList)
      {
        if (odorScent.Odor == odor)
          return odorScent;
      }
      return (OdorScent) null;
    }

    private void AddNewScent(OdorScent scent)
    {
      if (!this.m_Scents.Contains(scent))
        this.m_Scents.Add(scent);
      List<OdorScent> odorScentList;
      if (this.m_aux_ScentsByPosition.TryGetValue(scent.Position, out odorScentList))
      {
        odorScentList.Add(scent);
      }
      else
      {
        odorScentList = new List<OdorScent>(2);
        odorScentList.Add(scent);
        this.m_aux_ScentsByPosition.Add(scent.Position, odorScentList);
      }
    }

    public void ModifyScentAt(Odor odor, int strengthChange, Point position)
    {
      if (!this.IsInBounds(position))
        throw new ArgumentOutOfRangeException("position");
      OdorScent scentByOdor = this.GetScentByOdor(odor, position);
      if (scentByOdor == null)
        this.AddNewScent(new OdorScent(odor, strengthChange, position));
      else
        scentByOdor.Change(strengthChange);
    }

    public void RefreshScentAt(Odor odor, int freshStrength, Point position)
    {
      if (!this.IsInBounds(position))
        throw new ArgumentOutOfRangeException(string.Format("position; ({0},{1}) map {2} odor {3}", (object) position.X, (object) position.Y, (object) this.m_Name, (object) odor.ToString()));
      OdorScent scentByOdor = this.GetScentByOdor(odor, position);
      if (scentByOdor == null)
      {
        this.AddNewScent(new OdorScent(odor, freshStrength, position));
      }
      else
      {
        if (scentByOdor.Strength >= freshStrength)
          return;
        scentByOdor.Set(freshStrength);
      }
    }

    public void RemoveScent(OdorScent scent)
    {
      this.m_Scents.Remove(scent);
      List<OdorScent> odorScentList;
      if (!this.m_aux_ScentsByPosition.TryGetValue(scent.Position, out odorScentList))
        return;
      odorScentList.Remove(scent);
      if (odorScentList.Count != 0)
        return;
      this.m_aux_ScentsByPosition.Remove(scent.Position);
    }

    public void ClearView()
    {
      for (int index1 = 0; index1 < this.m_Width; ++index1)
      {
        for (int index2 = 0; index2 < this.m_Height; ++index2)
          this.m_Tiles[index1, index2].IsInView = false;
      }
    }

    public void SetView(IEnumerable<Point> visiblePositions)
    {
      this.ClearView();
      foreach (Point visiblePosition in visiblePositions)
      {
        if (!this.IsInBounds(visiblePosition.X, visiblePosition.Y))
          throw new ArgumentOutOfRangeException("point " + (object) visiblePosition + " not in map bounds");
        this.m_Tiles[visiblePosition.X, visiblePosition.Y].IsInView = true;
      }
    }

    public void MarkAsVisited(IEnumerable<Point> positions)
    {
      foreach (Point position in positions)
      {
        if (!this.IsInBounds(position.X, position.Y))
          throw new ArgumentOutOfRangeException("point " + (object) position + " not in map bounds");
        this.m_Tiles[position.X, position.Y].IsVisited = true;
      }
    }

    public void SetViewAndMarkVisited(IEnumerable<Point> visiblePositions)
    {
      this.ClearView();
      foreach (Point visiblePosition in visiblePositions)
      {
        if (!this.IsInBounds(visiblePosition.X, visiblePosition.Y))
          throw new ArgumentOutOfRangeException("point " + (object) visiblePosition + " not in map bounds");
        this.m_Tiles[visiblePosition.X, visiblePosition.Y].IsInView = true;
        this.m_Tiles[visiblePosition.X, visiblePosition.Y].IsVisited = true;
      }
    }

    public void SetAllAsVisited()
    {
      for (int index1 = 0; index1 < this.m_Width; ++index1)
      {
        for (int index2 = 0; index2 < this.m_Height; ++index2)
          this.m_Tiles[index1, index2].IsVisited = true;
      }
    }

    public void SetAllAsUnvisited()
    {
      for (int index1 = 0; index1 < this.m_Width; ++index1)
      {
        for (int index2 = 0; index2 < this.m_Height; ++index2)
          this.m_Tiles[index1, index2].IsVisited = false;
      }
    }

    public bool IsTransparent(int x, int y)
    {
      if (!this.IsInBounds(x, y) || !this.m_Tiles[x, y].Model.IsTransparent)
        return false;
      MapObject mapObjectAt = this.GetMapObjectAt(x, y);
      if (mapObjectAt == null)
        return true;
      return mapObjectAt.IsTransparent;
    }

    public bool IsWalkable(int x, int y)
    {
      if (!this.IsInBounds(x, y) || !this.m_Tiles[x, y].Model.IsWalkable)
        return false;
      MapObject mapObjectAt = this.GetMapObjectAt(x, y);
      if (mapObjectAt == null)
        return true;
      return mapObjectAt.IsWalkable;
    }

    public bool IsWalkable(Point p)
    {
      return this.IsWalkable(p.X, p.Y);
    }

    public bool IsBlockingFire(int x, int y)
    {
      if (!this.IsInBounds(x, y) || !this.m_Tiles[x, y].Model.IsTransparent)
        return true;
      MapObject mapObjectAt = this.GetMapObjectAt(x, y);
      return mapObjectAt != null && !mapObjectAt.IsTransparent || this.GetActorAt(x, y) != null;
    }

    public bool IsBlockingThrow(int x, int y)
    {
      if (!this.IsInBounds(x, y) || !this.m_Tiles[x, y].Model.IsWalkable)
        return true;
      MapObject mapObjectAt = this.GetMapObjectAt(x, y);
      return mapObjectAt != null && !mapObjectAt.IsWalkable && !mapObjectAt.IsJumpable;
    }

    public List<Point> FilterAdjacentInMap(Point position, Predicate<Point> predicateFn)
    {
      if (!this.IsInBounds(position))
        return (List<Point>) null;
      List<Point> pointList = (List<Point>) null;
      foreach (Direction direction in Direction.COMPASS)
      {
        Point p = position + direction;
        if (this.IsInBounds(p) && predicateFn(p))
        {
          if (pointList == null)
            pointList = new List<Point>(8);
          pointList.Add(p);
        }
      }
      return pointList;
    }

    public bool HasAnyAdjacentInMap(Point position, Predicate<Point> predicateFn)
    {
      if (!this.IsInBounds(position))
        return false;
      foreach (Direction direction in Direction.COMPASS)
      {
        Point p = position + direction;
        if (this.IsInBounds(p) && predicateFn(p))
          return true;
      }
      return false;
    }

    public int CountAdjacentInMap(Point position, Predicate<Point> predicateFn)
    {
      if (!this.IsInBounds(position))
        return 0;
      int num = 0;
      foreach (Direction direction in Direction.COMPASS)
      {
        Point p = position + direction;
        if (this.IsInBounds(p) && predicateFn(p))
          ++num;
      }
      return num;
    }

    public void ForEachAdjacentInMap(Point position, Action<Point> fn)
    {
      if (!this.IsInBounds(position))
        return;
      foreach (Direction direction in Direction.COMPASS)
      {
        Point p = position + direction;
        if (this.IsInBounds(p))
          fn(p);
      }
    }

    public Point? FindFirstInMap(Predicate<Point> predicateFn)
    {
      Point point = new Point();
      for (int index1 = 0; index1 < this.m_Width; ++index1)
      {
        point.X = index1;
        for (int index2 = 0; index2 < this.m_Height; ++index2)
        {
          point.Y = index2;
          if (predicateFn(point))
            return new Point?(point);
        }
      }
      return new Point?();
    }

    public void ReconstructAuxiliaryFields()
    {
      this.m_aux_ActorsByPosition = new Dictionary<Point, Actor>();
      foreach (Actor mActors in this.m_ActorsList)
        this.m_aux_ActorsByPosition.Add(mActors.Location.Position, mActors);
      this.m_aux_GroundItemsList = new List<Inventory>();
      foreach (Inventory inventory in this.m_GroundItemsByPosition.Values)
        this.m_aux_GroundItemsList.Add(inventory);
      this.m_aux_MapObjectsByPosition = new Dictionary<Point, MapObject>();
      foreach (MapObject mMapObjects in this.m_MapObjectsList)
        this.m_aux_MapObjectsByPosition.Add(mMapObjects.Location.Position, mMapObjects);
      this.m_aux_ScentsByPosition = new Dictionary<Point, List<OdorScent>>();
      foreach (OdorScent mScent in this.m_Scents)
      {
        List<OdorScent> odorScentList;
        if (this.m_aux_ScentsByPosition.TryGetValue(mScent.Position, out odorScentList))
        {
          odorScentList.Add(mScent);
        }
        else
        {
          odorScentList = new List<OdorScent>()
          {
            mScent
          };
          this.m_aux_ScentsByPosition.Add(mScent.Position, odorScentList);
        }
      }
      this.m_aux_CorpsesByPosition = new Dictionary<Point, List<Corpse>>();
      foreach (Corpse mCorpses in this.m_CorpsesList)
      {
        List<Corpse> corpseList;
        if (this.m_aux_CorpsesByPosition.TryGetValue(mCorpses.Position, out corpseList))
          corpseList.Add(mCorpses);
        else
          this.m_aux_CorpsesByPosition.Add(mCorpses.Position, new List<Corpse>(1)
          {
            mCorpses
          });
      }
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("m_Seed", this.m_Seed);
      info.AddValue("m_District", (object) this.m_District);
      info.AddValue("m_Name", (object) this.m_Name);
      info.AddValue("m_LocalTime", (object) this.m_LocalTime);
      info.AddValue("m_Width", this.m_Width);
      info.AddValue("m_Height", this.m_Height);
      info.AddValue("m_Rectangle", (object) this.m_Rectangle);
      info.AddValue("m_Tiles", (object) this.m_Tiles);
      info.AddValue("m_Exits", (object) this.m_Exits);
      info.AddValue("m_Zones", (object) this.m_Zones);
      info.AddValue("m_ActorsList", (object) this.m_ActorsList);
      info.AddValue("m_MapObjectsList", (object) this.m_MapObjectsList);
      info.AddValue("m_GroundItemsByPosition", (object) this.m_GroundItemsByPosition);
      info.AddValue("m_CorpsesList", (object) this.m_CorpsesList);
      info.AddValue("m_Lighting", (object) this.m_Lighting);
      info.AddValue("m_Scents", (object) this.m_Scents);
      info.AddValue("m_Timers", (object) this.m_Timers);
    }

    public void OptimizeBeforeSaving()
    {
      for (int index1 = 0; index1 < this.m_Width; ++index1)
      {
        for (int index2 = 0; index2 < this.m_Height; ++index2)
          this.m_Tiles[index1, index2].OptimizeBeforeSaving();
      }
      foreach (Actor mActors in this.m_ActorsList)
        mActors.OptimizeBeforeSaving();
      this.m_ActorsList.TrimExcess();
      this.m_MapObjectsList.TrimExcess();
      this.m_Scents.TrimExcess();
      this.m_Zones.TrimExcess();
      this.m_CorpsesList.TrimExcess();
      this.m_Timers.TrimExcess();
    }

    public override int GetHashCode()
    {
      return this.m_Name.GetHashCode() ^ this.m_District.GetHashCode();
    }
  }
}
