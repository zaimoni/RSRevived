﻿// Decompiled with JetBrains decompiler
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
    public int Seed { get; private set; }
    private District m_District;
    public string Name { get; set; }
    private Lighting m_Lighting;
    public WorldTime LocalTime { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public Rectangle Rect { get; private set; }
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
      get {
        return m_District;
      }
      set {
        m_District = value;
      }
    }

    public bool IsSecret { get; set; }

    public Lighting Lighting
    {
      get {
        return m_Lighting;
      }
      set {
        m_Lighting = value;
      }
    }

    public IEnumerable<Zone> Zones {
      get {
        return (IEnumerable<Zone>)m_Zones;
      }
    }

    public IEnumerable<Exit> Exits {
      get {
        return (IEnumerable<Exit>)m_Exits.Values;
      }
    }

    public int CountExits {
      get {
        if (m_Exits.Values != null)
          return m_Exits.Values.Count;
        return 0;
      }
    }

    public IEnumerable<Actor> Actors {
      get {
        return (IEnumerable<Actor>)m_ActorsList;
      }
    }

    public int CountActors {
      get {
        return m_ActorsList.Count;
      }
    }

    public int CheckNextActorIndex
    {
      get {
        return m_iCheckNextActorIndex;
      }
      set { // nominates RogueGame::NextMapTurn for conversion to Map member function
        m_iCheckNextActorIndex = value;
      }
    }

    public IEnumerable<MapObject> MapObjects {
      get {
        return (IEnumerable<MapObject>)m_MapObjectsList;
      }
    }

    public IEnumerable<Inventory> GroundInventories {
      get {
        return (IEnumerable<Inventory>)m_aux_GroundItemsList;
      }
    }

    public IEnumerable<Corpse> Corpses {
      get {
        return (IEnumerable<Corpse>)m_CorpsesList;
      }
    }

    public int CountCorpses {
      get {
        return m_CorpsesList.Count;
      }
    }

    public IEnumerable<TimedTask> Timers {
      get {
        return (IEnumerable<TimedTask>)m_Timers;
      }
    }

    public int CountTimers {
      get {
        if (m_Timers != null)
          return m_Timers.Count;
        return 0;
      }
    }

    public IEnumerable<OdorScent> Scents {
      get {
        return (IEnumerable<OdorScent>)m_Scents;
      }
    }

    public Map(int seed, string name, int width, int height)
    {
      if (name == null) throw new ArgumentNullException("name");
      if (width <= 0) throw new ArgumentOutOfRangeException("width <=0");
      if (height <= 0) throw new ArgumentOutOfRangeException("height <=0");
      Seed = seed;
      Name = name;
      Width = width;
      Height = height;
      Rect = new Rectangle(0, 0, width, height);
      LocalTime = new WorldTime();
      Lighting = Lighting.OUTSIDE;
      IsSecret = false;
      m_Tiles = new Tile[width, height];
      for (int index1 = 0; index1 < width; ++index1) {
        for (int index2 = 0; index2 < height; ++index2)
          m_Tiles[index1, index2] = new Tile(TileModel.UNDEF);
      }
      m_Exits = new Dictionary<Point, Exit>();
      m_Zones = new List<Zone>(5);
      m_aux_ActorsByPosition = new Dictionary<Point, Actor>(5);
      m_ActorsList = new List<Actor>(5);
      m_aux_MapObjectsByPosition = new Dictionary<Point, MapObject>(5);
      m_MapObjectsList = new List<MapObject>(5);
      m_GroundItemsByPosition = new Dictionary<Point, Inventory>(5);
      m_aux_GroundItemsList = new List<Inventory>(5);
      m_CorpsesList = new List<Corpse>(5);
      m_aux_CorpsesByPosition = new Dictionary<Point, List<Corpse>>(5);
      m_Scents = new List<OdorScent>(128);
      m_aux_ScentsByPosition = new Dictionary<Point, List<OdorScent>>(128);
      m_Timers = new List<TimedTask>(5);
    }

    // next constructor, and function after, implement ISerializable
    protected Map(SerializationInfo info, StreamingContext context)
    {
      Seed = (int) info.GetValue("m_Seed", typeof (int));
      m_District = (District) info.GetValue("m_District", typeof (District));
      Name = (string) info.GetValue("m_Name", typeof (string));
      LocalTime = (WorldTime) info.GetValue("m_LocalTime", typeof (WorldTime));
      Width = (int) info.GetValue("m_Width", typeof (int));
      Height = (int) info.GetValue("m_Height", typeof (int));
      Rect = (Rectangle) info.GetValue("m_Rectangle", typeof (Rectangle));
      m_Tiles = (Tile[,]) info.GetValue("m_Tiles", typeof (Tile[,]));
      m_Exits = (Dictionary<Point, Exit>) info.GetValue("m_Exits", typeof (Dictionary<Point, Exit>));
      m_Zones = (List<Zone>) info.GetValue("m_Zones", typeof (List<Zone>));
      m_ActorsList = (List<Actor>) info.GetValue("m_ActorsList", typeof (List<Actor>));
      m_MapObjectsList = (List<MapObject>) info.GetValue("m_MapObjectsList", typeof (List<MapObject>));
      m_GroundItemsByPosition = (Dictionary<Point, Inventory>) info.GetValue("m_GroundItemsByPosition", typeof (Dictionary<Point, Inventory>));
      m_CorpsesList = (List<Corpse>) info.GetValue("m_CorpsesList", typeof (List<Corpse>));
      m_Lighting = (Lighting) info.GetValue("m_Lighting", typeof (Lighting));
      m_Scents = (List<OdorScent>) info.GetValue("m_Scents", typeof (List<OdorScent>));
      m_Timers = (List<TimedTask>) info.GetValue("m_Timers", typeof (List<TimedTask>));
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("m_Seed", Seed);
      info.AddValue("m_District", (object)m_District);
      info.AddValue("m_Name", (object)Name);
      info.AddValue("m_LocalTime", (object)LocalTime);
      info.AddValue("m_Width", Width);
      info.AddValue("m_Height", Height);
      info.AddValue("m_Rectangle", (object)Rect);
      info.AddValue("m_Tiles", (object)m_Tiles);
      info.AddValue("m_Exits", (object)m_Exits);
      info.AddValue("m_Zones", (object)m_Zones);
      info.AddValue("m_ActorsList", (object)m_ActorsList);
      info.AddValue("m_MapObjectsList", (object)m_MapObjectsList);
      info.AddValue("m_GroundItemsByPosition", (object)m_GroundItemsByPosition);
      info.AddValue("m_CorpsesList", (object)m_CorpsesList);
      info.AddValue("m_Lighting", (object)m_Lighting);
      info.AddValue("m_Scents", (object)m_Scents);
      info.AddValue("m_Timers", (object)m_Timers);
    }


    public bool IsInBounds(int x, int y)
    {
      if (x >= 0 && x < Width && y >= 0)
        return y < Height;
      return false;
    }

    public bool IsInBounds(Point p)
    {
      return IsInBounds(p.X, p.Y);
    }

    public void TrimToBounds(ref int x, ref int y)
    {
      if (x < 0)
        x = 0;
      else if (x > Width - 1)
        x = Width - 1;
      if (y < 0)
        y = 0;
      else
      {
        if (y <= Height - 1)
          return;
        y = Height - 1;
      }
    }

    public void TrimToBounds(ref Point p)
    {
      if (p.X < 0)
        p.X = 0;
      else if (p.X > Width - 1)
        p.X = Width - 1;
      if (p.Y < 0)
        p.Y = 0;
      else
      {
        if (p.Y <= Height - 1)
          return;
        p.Y = Height - 1;
      }
    }

    public bool IsMapBoundary(int x, int y)
    {
      if (x != -1 && x != Width && y != -1)
        return y == Height;
      return true;
    }

    public bool IsOnMapBorder(int x, int y)
    {
      if (x != 0 && x != Width - 1 && y != 0)
        return y == Height - 1;
      return true;
    }

    public Tile GetTileAt(int x, int y)
    {
      return m_Tiles[x, y];
    }

    public Tile GetTileAt(Point p)
    {
      return m_Tiles[p.X, p.Y];
    }

    public void SetTileModelAt(int x, int y, TileModel model)
    {
      if (!IsInBounds(x, y)) throw new ArgumentOutOfRangeException("position out of map bounds");
      if (model == null) throw new ArgumentNullException("model");
      m_Tiles[x, y].Model = model;
    }

    public Exit GetExitAt(Point pos)
    {
      Exit exit;
      if (m_Exits.TryGetValue(pos, out exit))
        return exit;
      return null;
    }

    public Exit GetExitAt(int x, int y)
    {
      return GetExitAt(new Point(x, y));
    }

    public void SetExitAt(Point pos, Exit exit)
    {
      m_Exits.Add(pos, exit);
    }

    public void RemoveExitAt(Point pos)
    {
      m_Exits.Remove(pos);
    }

    public bool HasAnExitIn(Rectangle rect)
    {
      for (int left = rect.Left; left < rect.Right; ++left) {
        for (int top = rect.Top; top < rect.Bottom; ++top) {
          if (GetExitAt(left, top) != null) return true;
        }
      }
      return false;
    }

    public Point? GetExitPos(Exit exit)
    {
      if (exit == null) return new Point?();
      foreach (KeyValuePair<Point, Exit> mExit in m_Exits) {
        if (mExit.Value == exit) return new Point?(mExit.Key);
      }
      return new Point?();
    }

    public void AddZone(Zone zone)
    {
      m_Zones.Add(zone);
    }

    public void RemoveZone(Zone zone)
    {
      m_Zones.Remove(zone);
    }

    public void RemoveAllZonesAt(int x, int y)
    {
      List<Zone> zonesAt = GetZonesAt(x, y);
      if (zonesAt == null) return;
      foreach (Zone zone in zonesAt)
        RemoveZone(zone);
    }

    public List<Zone> GetZonesAt(int x, int y)
    {
      List<Zone> zoneList = (List<Zone>) null;
      foreach (Zone mZone in m_Zones) {
        if (mZone.Bounds.Contains(x, y)) {
          if (zoneList == null)
            zoneList = new List<Zone>(m_Zones.Count / 4);
          zoneList.Add(mZone);
        }
      }
      return zoneList;
    }

    public Zone GetZoneByName(string name)
    {
      foreach (Zone mZone in m_Zones) {
        if (mZone.Name == name) return mZone;
      }
      return null;
    }

    public Zone GetZoneByPartialName(string partOfname)
    {
      foreach (Zone mZone in m_Zones) {
        if (mZone.Name.Contains(partOfname)) return mZone;
      }
      return null;
    }

    public bool HasZonePartiallyNamedAt(Point pos, string partOfName)
    {
      List<Zone> zonesAt = GetZonesAt(pos.X, pos.Y);
      if (zonesAt == null) return false;
      foreach (Zone zone in zonesAt) {
        if (zone.Name.Contains(partOfName)) return true;
      }
      return false;
    }

    // Actor manipulation functions
    public bool HasActor(Actor actor)
    {
      return m_ActorsList.Contains(actor);
    }

    public Actor GetActor(int index)
    {
      return m_ActorsList[index];
    }

    public Actor GetActorAt(Point position)
    {
      Actor actor;
      if (m_aux_ActorsByPosition.TryGetValue(position, out actor)) return actor;
      return null;
    }

    public Actor GetActorAt(int x, int y)
    {
      return GetActorAt(new Point(x, y));
    }

    public void PlaceActorAt(Actor actor, Point position)
    {
      if (actor == null)
        throw new ArgumentNullException("actor");
      Actor actorAt = GetActorAt(position);
      if (actorAt == actor)
        throw new InvalidOperationException("actor already at position");
      if (actorAt != null)
        throw new InvalidOperationException("another actor already at position");
      if (!IsInBounds(position.X, position.Y))
        throw new ArgumentOutOfRangeException("position out of map bounds");
      if (HasActor(actor))
                m_aux_ActorsByPosition.Remove(actor.Location.Position);
      else
                m_ActorsList.Add(actor);
            m_aux_ActorsByPosition.Add(position, actor);
      actor.Location = new Location(this, position);
            m_iCheckNextActorIndex = 0;
    }

    public void MoveActorToFirstPosition(Actor actor)
    {
      if (!m_ActorsList.Contains(actor))
        throw new ArgumentException("actor not in map");
            m_ActorsList.Remove(actor);
      if (m_ActorsList.Count == 0)
                m_ActorsList.Add(actor);
      else
                m_ActorsList.Insert(0, actor);
            m_iCheckNextActorIndex = 0;
    }

    public void RemoveActor(Actor actor)
    {
      if (!m_ActorsList.Contains(actor))
        return;
            m_ActorsList.Remove(actor);
            m_aux_ActorsByPosition.Remove(actor.Location.Position);
            m_iCheckNextActorIndex = 0;
    }

    public Actor NextActorToAct { 
      get {
        int countActors = m_ActorsList.Count;
        int turnCounter = LocalTime.TurnCounter;
        for (int checkNextActorIndex = m_iCheckNextActorIndex; checkNextActorIndex < countActors; ++checkNextActorIndex) {
          Actor actor = m_ActorsList[checkNextActorIndex];
          if (actor.ActionPoints > 0 && !actor.IsSleeping) {
            m_iCheckNextActorIndex = checkNextActorIndex;
            return actor;
          }
        }
        return null;
      }
    }

    // map object manipulation functions
    public bool HasMapObject(MapObject mapObj)
    {
      return m_MapObjectsList.Contains(mapObj);
    }

    public MapObject GetMapObjectAt(Point position)
    {
      MapObject mapObject;
      if (m_aux_MapObjectsByPosition.TryGetValue(position, out mapObject))
        return mapObject;
      return (MapObject) null;
    }

    public MapObject GetMapObjectAt(int x, int y)
    {
      return GetMapObjectAt(new Point(x, y));
    }

    public void PlaceMapObjectAt(MapObject mapObj, Point position)
    {
      if (mapObj == null)
        throw new ArgumentNullException("actor");
      MapObject mapObjectAt = GetMapObjectAt(position);
      if (mapObjectAt == mapObj)
        return;
      if (mapObjectAt == mapObj)
        throw new InvalidOperationException("mapObject already at position");
      if (mapObjectAt != null)
        throw new InvalidOperationException("another mapObject already at position");
      if (!IsInBounds(position.X, position.Y))
        throw new ArgumentOutOfRangeException("position out of map bounds");
      if (!GetTileAt(position.X, position.Y).Model.IsWalkable)
        throw new InvalidOperationException("cannot place map objects on unwalkable tiles");
      if (HasMapObject(mapObj))
                m_aux_MapObjectsByPosition.Remove(mapObj.Location.Position);
      else
                m_MapObjectsList.Add(mapObj);
            m_aux_MapObjectsByPosition.Add(position, mapObj);
      mapObj.Location = new Location(this, position);
    }

    public void RemoveMapObjectAt(int x, int y)
    {
      MapObject mapObjectAt = GetMapObjectAt(x, y);
      if (mapObjectAt == null)
        return;
            m_MapObjectsList.Remove(mapObjectAt);
            m_aux_MapObjectsByPosition.Remove(new Point(x, y));
    }

    public Inventory GetItemsAt(Point position)
    {
      if (!IsInBounds(position))
        return (Inventory) null;
      Inventory inventory;
      if (m_GroundItemsByPosition.TryGetValue(position, out inventory))
        return inventory;
      return (Inventory) null;
    }

    public Inventory GetItemsAt(int x, int y)
    {
      return GetItemsAt(new Point(x, y));
    }

    public djack.RogueSurvivor.Engine.Items.ItemTrap GetActivatedTrapAt(Point pos)
    {
      Inventory itemsAt = GetItemsAt(pos);
      if (itemsAt == null || itemsAt.IsEmpty) return null;
      Item tmp = itemsAt.GetFirstMatching((Predicate<Item>) (it =>
      {
        djack.RogueSurvivor.Engine.Items.ItemTrap itemTrap = it as djack.RogueSurvivor.Engine.Items.ItemTrap;
        if (itemTrap != null) return itemTrap.IsActivated;
        return false;
      }));
      return tmp as djack.RogueSurvivor.Engine.Items.ItemTrap;
    }

    public Point? GetGroundInventoryPosition(Inventory groundInv)
    {
      foreach (KeyValuePair<Point, Inventory> keyValuePair in m_GroundItemsByPosition)
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
      if (!IsInBounds(position))
        throw new ArgumentOutOfRangeException("position out of map bounds");
      Inventory itemsAt = GetItemsAt(position);
      if (itemsAt == null)
      {
        Inventory inventory = new Inventory(GROUND_INVENTORY_SLOTS);
                m_aux_GroundItemsList.Add(inventory);
                m_GroundItemsByPosition.Add(position, inventory);
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
            DropItemAt(it, new Point(x, y));
    }

    public void RemoveItemAt(Item it, Point position)
    {
      if (it == null)
        throw new ArgumentNullException("item");
      if (!IsInBounds(position))
        throw new ArgumentOutOfRangeException("position out of map bounds");
      Inventory itemsAt = GetItemsAt(position);
      if (itemsAt == null)
        throw new ArgumentException("no items at this position");
      if (!itemsAt.Contains(it))
        throw new ArgumentException("item not at this position");
      itemsAt.RemoveAllQuantity(it);
      if (!itemsAt.IsEmpty)
        return;
            m_GroundItemsByPosition.Remove(position);
            m_aux_GroundItemsList.Remove(itemsAt);
    }

    public void RemoveItemAt(Item it, int x, int y)
    {
            RemoveItemAt(it, new Point(x, y));
    }

    public void RemoveAllItemsAt(Point position)
    {
      Inventory itemsAt = GetItemsAt(position);
      if (itemsAt == null)
        return;
            m_GroundItemsByPosition.Remove(position);
            m_aux_GroundItemsList.Remove(itemsAt);
    }

    public List<Corpse> GetCorpsesAt(Point p)
    {
      List<Corpse> corpseList;
      if (m_aux_CorpsesByPosition.TryGetValue(p, out corpseList))
        return corpseList;
      return (List<Corpse>) null;
    }

    public List<Corpse> GetCorpsesAt(int x, int y)
    {
      return GetCorpsesAt(new Point(x, y));
    }

    public bool HasCorpse(Corpse c)
    {
      return m_CorpsesList.Contains(c);
    }

    public void AddCorpseAt(Corpse c, Point p)
    {
      if (m_CorpsesList.Contains(c))
        throw new ArgumentException("corpse already in this map");
      c.Position = p;
            m_CorpsesList.Add(c);
            InsertCorpseAtPos(c);
      c.DeadGuy.Location = new Location(this, p);
    }

    public void MoveCorpseTo(Corpse c, Point newPos)
    {
      if (!m_CorpsesList.Contains(c))
        throw new ArgumentException("corpse not in this map");
            RemoveCorpseFromPos(c);
      c.Position = newPos;
            InsertCorpseAtPos(c);
      c.DeadGuy.Location = new Location(this, newPos);
    }

    public void RemoveCorpse(Corpse c)
    {
      if (!m_CorpsesList.Contains(c))
        throw new ArgumentException("corpse not in this map");
            m_CorpsesList.Remove(c);
            RemoveCorpseFromPos(c);
    }

    public bool TryRemoveCorpseOf(Actor a)
    {
      foreach (Corpse mCorpses in m_CorpsesList)
      {
        if (mCorpses.DeadGuy == a)
        {
                    RemoveCorpse(mCorpses);
          return true;
        }
      }
      return false;
    }

    private void RemoveCorpseFromPos(Corpse c)
    {
      List<Corpse> corpseList;
      if (!m_aux_CorpsesByPosition.TryGetValue(c.Position, out corpseList))
        return;
      corpseList.Remove(c);
      if (corpseList.Count != 0)
        return;
            m_aux_CorpsesByPosition.Remove(c.Position);
    }

    private void InsertCorpseAtPos(Corpse c)
    {
      List<Corpse> corpseList;
      if (m_aux_CorpsesByPosition.TryGetValue(c.Position, out corpseList))
        corpseList.Insert(0, c);
      else
                m_aux_CorpsesByPosition.Add(c.Position, new List<Corpse>(1) { c });
    }

    public void AddTimer(TimedTask t)
    {
      if (m_Timers == null)
                m_Timers = new List<TimedTask>(5);
            m_Timers.Add(t);
    }

    public void RemoveTimer(TimedTask t)
    {
            m_Timers.Remove(t);
    }

    public int GetScentByOdorAt(Odor odor, Point position)
    {
      if (!IsInBounds(position))
        return 0;
      OdorScent scentByOdor = GetScentByOdor(odor, position);
      if (scentByOdor != null)
        return scentByOdor.Strength;
      return 0;
    }

    private OdorScent GetScentByOdor(Odor odor, Point p)
    {
      List<OdorScent> odorScentList;
      if (!m_aux_ScentsByPosition.TryGetValue(p, out odorScentList))
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
      if (!m_Scents.Contains(scent))
                m_Scents.Add(scent);
      List<OdorScent> odorScentList;
      if (m_aux_ScentsByPosition.TryGetValue(scent.Position, out odorScentList))
      {
        odorScentList.Add(scent);
      }
      else
      {
        odorScentList = new List<OdorScent>(2);
        odorScentList.Add(scent);
                m_aux_ScentsByPosition.Add(scent.Position, odorScentList);
      }
    }

    public void ModifyScentAt(Odor odor, int strengthChange, Point position)
    {
      if (!IsInBounds(position))
        throw new ArgumentOutOfRangeException("position");
      OdorScent scentByOdor = GetScentByOdor(odor, position);
      if (scentByOdor == null)
                AddNewScent(new OdorScent(odor, strengthChange, position));
      else
        scentByOdor.Change(strengthChange);
    }

    public void RefreshScentAt(Odor odor, int freshStrength, Point position)
    {
      if (!IsInBounds(position))
        throw new ArgumentOutOfRangeException(string.Format("position; ({0},{1}) map {2} odor {3}", (object) position.X, (object) position.Y, (object) Name, (object) odor.ToString()));
      OdorScent scentByOdor = GetScentByOdor(odor, position);
      if (scentByOdor == null)
      {
        AddNewScent(new OdorScent(odor, freshStrength, position));
      }
      else
      {
        if (scentByOdor.Strength >= freshStrength) return;
        scentByOdor.Set(freshStrength);
      }
    }

    public void RemoveScent(OdorScent scent)
    {
            m_Scents.Remove(scent);
      List<OdorScent> odorScentList;
      if (!m_aux_ScentsByPosition.TryGetValue(scent.Position, out odorScentList))
        return;
      odorScentList.Remove(scent);
      if (odorScentList.Count != 0)
        return;
            m_aux_ScentsByPosition.Remove(scent.Position);
    }

    public void ClearView()
    {
      for (int index1 = 0; index1 < Width; ++index1)
      {
        for (int index2 = 0; index2 < Height; ++index2)
          m_Tiles[index1, index2].IsInView = false;
      }
    }

    public void SetView(IEnumerable<Point> visiblePositions)
    {
            ClearView();
      foreach (Point visiblePosition in visiblePositions)
      {
        if (!IsInBounds(visiblePosition.X, visiblePosition.Y))
          throw new ArgumentOutOfRangeException("point " + (object) visiblePosition + " not in map bounds");
                m_Tiles[visiblePosition.X, visiblePosition.Y].IsInView = true;
      }
    }

    public void MarkAsVisited(IEnumerable<Point> positions)
    {
      foreach (Point position in positions)
      {
        if (!IsInBounds(position.X, position.Y))
          throw new ArgumentOutOfRangeException("point " + (object) position + " not in map bounds");
                m_Tiles[position.X, position.Y].IsVisited = true;
      }
    }

    public void SetViewAndMarkVisited(IEnumerable<Point> visiblePositions)
    {
            ClearView();
      foreach (Point visiblePosition in visiblePositions)
      {
        if (!IsInBounds(visiblePosition.X, visiblePosition.Y))
          throw new ArgumentOutOfRangeException("point " + (object) visiblePosition + " not in map bounds");
                m_Tiles[visiblePosition.X, visiblePosition.Y].IsInView = true;
                m_Tiles[visiblePosition.X, visiblePosition.Y].IsVisited = true;
      }
    }

    public void SetAllAsVisited()
    {
      for (int index1 = 0; index1 < Width; ++index1)
      {
        for (int index2 = 0; index2 < Height; ++index2)
          m_Tiles[index1, index2].IsVisited = true;
      }
    }

    public void SetAllAsUnvisited()
    {
      for (int index1 = 0; index1 < Width; ++index1)
      {
        for (int index2 = 0; index2 < Height; ++index2)
          m_Tiles[index1, index2].IsVisited = false;
      }
    }

    public bool IsTransparent(int x, int y)
    {
      if (!IsInBounds(x, y) || !m_Tiles[x, y].Model.IsTransparent)
        return false;
      MapObject mapObjectAt = GetMapObjectAt(x, y);
      if (mapObjectAt == null)
        return true;
      return mapObjectAt.IsTransparent;
    }

    public bool IsWalkable(int x, int y)
    {
      if (!IsInBounds(x, y) || !m_Tiles[x, y].Model.IsWalkable)
        return false;
      MapObject mapObjectAt = GetMapObjectAt(x, y);
      if (mapObjectAt == null)
        return true;
      return mapObjectAt.IsWalkable;
    }

    public bool IsWalkable(Point p)
    {
      return IsWalkable(p.X, p.Y);
    }

    public bool IsBlockingFire(int x, int y)
    {
      if (!IsInBounds(x, y) || !m_Tiles[x, y].Model.IsTransparent)
        return true;
      MapObject mapObjectAt = GetMapObjectAt(x, y);
      return mapObjectAt != null && !mapObjectAt.IsTransparent || GetActorAt(x, y) != null;
    }

    public bool IsBlockingThrow(int x, int y)
    {
      if (!IsInBounds(x, y) || !m_Tiles[x, y].Model.IsWalkable)
        return true;
      MapObject mapObjectAt = GetMapObjectAt(x, y);
      return mapObjectAt != null && !mapObjectAt.IsWalkable && !mapObjectAt.IsJumpable;
    }

    public List<Point> FilterAdjacentInMap(Point position, Predicate<Point> predicateFn)
    {
      if (!IsInBounds(position))
        return (List<Point>) null;
      List<Point> pointList = (List<Point>) null;
      foreach (Direction direction in Direction.COMPASS)
      {
        Point p = position + direction;
        if (IsInBounds(p) && predicateFn(p))
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
      if (!IsInBounds(position))
        return false;
      foreach (Direction direction in Direction.COMPASS)
      {
        Point p = position + direction;
        if (IsInBounds(p) && predicateFn(p))
          return true;
      }
      return false;
    }

    public int CountAdjacentInMap(Point position, Predicate<Point> predicateFn)
    {
      if (!IsInBounds(position))
        return 0;
      int num = 0;
      foreach (Direction direction in Direction.COMPASS)
      {
        Point p = position + direction;
        if (IsInBounds(p) && predicateFn(p))
          ++num;
      }
      return num;
    }

    public void ForEachAdjacentInMap(Point position, Action<Point> fn)
    {
      if (!IsInBounds(position)) return;
#if DEBUG
      if (null == fn) throw new ArgumentNullException("fn");
#endif
      foreach (Direction direction in Direction.COMPASS) {
        Point p = position + direction;
        if (IsInBounds(p)) fn(p);
      }
    }

    public Point? FindFirstInMap(Predicate<Point> predicateFn)
    {
      Point point = new Point();
      for (int index1 = 0; index1 < Width; ++index1)
      {
        point.X = index1;
        for (int index2 = 0; index2 < Height; ++index2)
        {
          point.Y = index2;
          if (predicateFn(point)) return new Point?(point);
        }
      }
      return new Point?();
    }

    public void ReconstructAuxiliaryFields()
    {
            m_aux_ActorsByPosition = new Dictionary<Point, Actor>();
      foreach (Actor mActors in m_ActorsList)
                m_aux_ActorsByPosition.Add(mActors.Location.Position, mActors);
            m_aux_GroundItemsList = new List<Inventory>();
      foreach (Inventory inventory in m_GroundItemsByPosition.Values)
                m_aux_GroundItemsList.Add(inventory);
            m_aux_MapObjectsByPosition = new Dictionary<Point, MapObject>();
      foreach (MapObject mMapObjects in m_MapObjectsList)
                m_aux_MapObjectsByPosition.Add(mMapObjects.Location.Position, mMapObjects);
            m_aux_ScentsByPosition = new Dictionary<Point, List<OdorScent>>();
      foreach (OdorScent mScent in m_Scents)
      {
        List<OdorScent> odorScentList;
        if (m_aux_ScentsByPosition.TryGetValue(mScent.Position, out odorScentList))
        {
          odorScentList.Add(mScent);
        }
        else
        {
          odorScentList = new List<OdorScent>()
          {
            mScent
          };
                    m_aux_ScentsByPosition.Add(mScent.Position, odorScentList);
        }
      }
            m_aux_CorpsesByPosition = new Dictionary<Point, List<Corpse>>();
      foreach (Corpse mCorpses in m_CorpsesList)
      {
        List<Corpse> corpseList;
        if (m_aux_CorpsesByPosition.TryGetValue(mCorpses.Position, out corpseList))
          corpseList.Add(mCorpses);
        else
                    m_aux_CorpsesByPosition.Add(mCorpses.Position, new List<Corpse>(1)
          {
            mCorpses
          });
      }
      // Support savefile hacking.
      // Check the actors.  If any have null controllers, intent was to hand control from the player to the AI.
      // Give them AI controllers here.
      foreach(Actor tmp in m_ActorsList)
      {
        if (null != tmp.Controller) continue;
        tmp.Controller = tmp.Model.InstanciateController();
      }
    }

    public void OptimizeBeforeSaving()
    {
      for (int index1 = 0; index1 < Width; ++index1)
      {
        for (int index2 = 0; index2 < Height; ++index2)
          m_Tiles[index1, index2].OptimizeBeforeSaving();
      }

      int i = 0;
      if (null != m_ActorsList)
      {
        i = m_ActorsList.Count;
        while (0 < i--) {
          if (m_ActorsList[i].IsDead) m_ActorsList.RemoveAt(i);
        }
      }

      foreach (Actor mActors in m_ActorsList)
        mActors.OptimizeBeforeSaving();
      m_ActorsList.TrimExcess();
      m_MapObjectsList.TrimExcess();
      m_Scents.TrimExcess();
      m_Zones.TrimExcess();
      m_CorpsesList.TrimExcess();
      m_Timers.TrimExcess();
    }

    public override int GetHashCode()
    {
      return Name.GetHashCode() ^ m_District.GetHashCode();
    }
  }
}
