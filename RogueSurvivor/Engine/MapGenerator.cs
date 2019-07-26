﻿// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.MapGenerator
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;
using System;
using System.Linq;
using System.Collections.Generic;
using Zaimoni.Data;

#if Z_VECTOR
using Point = Zaimoni.Data.Vector2D_short;
using Rectangle = Zaimoni.Data.Box2D_short;
#else
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
#endif


namespace djack.RogueSurvivor.Engine
{
  internal abstract class MapGenerator
  {
    protected readonly Rules m_Rules;   // all uses of this alias of the global Rules object RogueForm.Game.Rules are legitimate

    protected MapGenerator(Rules rules)
    {
#if DEBUG
      if (null == rules) throw new ArgumentNullException(nameof(rules));
#endif
      m_Rules = rules;
    }

    public abstract Map Generate(int seed, string name);

#region Tile filling
    protected static void TileFill(Map map, TileModel model, bool inside)
    {
      TileFill(map, model, 0, 0, map.Width, map.Height, inside);
    }

    protected static void TileFill(Map map, TileModel model, Action<Tile, TileModel, int, int> decoratorFn=null)
    {
      TileFill(map, model, 0, 0, map.Width, map.Height, decoratorFn);
    }

    protected static void TileFill(Map map, TileModel model, Rectangle rect, bool inside)
    {
      TileFill(map, model, rect.Left, rect.Top, rect.Width, rect.Height, inside);
    }

    protected static void TileFill(Map map, TileModel model, Rectangle rect, Action<Tile, TileModel, int, int> decoratorFn=null)
    {
      TileFill(map, model, rect.Left, rect.Top, rect.Width, rect.Height, decoratorFn);
    }

    protected static void TileFill(Map map, TileModel model, int left, int top, int width, int height, Action<Tile, TileModel, int, int> decoratorFn=null)
    {
#if DEBUG
      if (null == map) throw new ArgumentNullException(nameof(map));
      if (null == model) throw new ArgumentNullException(nameof(model));
#endif
      for (int x = left; x < left + width; ++x) {
        for (int y = top; y < top + height; ++y) {
          TileModel model1 = map.GetTileModelAt(x, y);
          map.SetTileModelAt(x, y, model);
          decoratorFn?.Invoke(map.GetTileAt(x, y), model1, x, y);
        }
      }
    }

    protected static void TileFill(Map map, TileModel model, int left, int top, int width, int height, bool inside)
    {
#if DEBUG
      if (null == map) throw new ArgumentNullException(nameof(map));
      if (null == model) throw new ArgumentNullException(nameof(model));
#endif
      for (int x = left; x < left + width; ++x) {
        for (int y = top; y < top + height; ++y) {
          map.SetTileModelAt(x, y, model);
          map.SetIsInsideAt(x, y, inside);
        }
      }
    }

    protected static void TileHLine(Map map, TileModel model, int left, int top, int width, Action<Tile, TileModel, int, int> decoratorFn=null)
    {
#if DEBUG
      if (null == map) throw new ArgumentNullException(nameof(map));
      if (null == model) throw new ArgumentNullException(nameof(model));
#endif
      for (int x = left; x < left + width; ++x) {
        TileModel model1 = map.GetTileModelAt(x, top);
        map.SetTileModelAt(x, top, model);
        decoratorFn?.Invoke(map.GetTileAt(x, top), model1, x, top);
      }
    }

    protected static void TileVLine(Map map, TileModel model, int left, int top, int height, Action<Tile, TileModel, int, int> decoratorFn=null)
    {
#if DEBUG
      if (null == map) throw new ArgumentNullException(nameof(map));
      if (null == model) throw new ArgumentNullException(nameof(model));
#endif
      for (int y = top; y < top + height; ++y) {
        TileModel model1 = map.GetTileModelAt(left, y);
        map.SetTileModelAt(left, y, model);
        decoratorFn?.Invoke(map.GetTileAt(left, y), model1, left, y);
      }
    }

    protected static void TileRectangle(Map map, TileModel model, Rectangle rect)
    {
      TileRectangle(map, model, rect.Left, rect.Top, rect.Width, rect.Height);
    }

    protected static void TileRectangle(Map map, TileModel model, int left, int top, int width, int height, Action<Tile, TileModel, int, int> decoratorFn=null)
    {
#if DEBUG
      if (null == map) throw new ArgumentNullException(nameof(map));
      if (null == model) throw new ArgumentNullException(nameof(model));
#endif
      TileHLine(map, model, left, top, width, decoratorFn);
      TileHLine(map, model, left, top + height - 1, width, decoratorFn);
      TileVLine(map, model, left, top, height, decoratorFn);
      TileVLine(map, model, left + width - 1, top, height, decoratorFn);
    }

#if DEAD_FUNC
    // dead, but typical for a map generation utility
    public Point DigUntil(Map map, TileModel model, Point startPos, Direction digDirection, Predicate<Point> stopFn)
    {
#if DEBUG
      if (null == stopFn) throw new ArgumentNullException(nameof(stopFn));
#endif
      Point p = startPos + digDirection;
      while (map.IsInBounds(p) && !stopFn(p)) {
        map.SetTileModelAt(p.X, p.Y, model);
        p += digDirection;
      }
      return p;
    }
#endif

    protected static void DoForEachTile(Rectangle rect, Action<Point> doFn)
    {
#if DEBUG
      if (null == doFn) throw new ArgumentNullException(nameof(doFn));
#endif
      Point point = new Point();
      for (point.X = rect.Left; point.X < rect.Right; ++point.X) {
        for (point.Y = rect.Top; point.Y < rect.Bottom; ++point.Y) {
          doFn(point);
        }
      }
    }

    protected static bool CheckForEachTile(Rectangle rect, Predicate<Point> predFn)
    {
#if DEBUG
      if (null == predFn) throw new ArgumentNullException(nameof(predFn));
#endif
      Point point = new Point();
      for (point.X = rect.Left; point.X < rect.Right; ++point.X) {
        for (point.Y = rect.Top; point.Y < rect.Bottom; ++point.Y) {
          if (!predFn(point)) return false;
        }
      }
      return true;
    }
#endregion

#region Placing actors
    public static bool ActorPlace(DiceRoller roller, Map map, Actor actor, Predicate<Point> goodPositionFn=null)
    {
      return ActorPlace(roller, map, actor, map.Rect, goodPositionFn);
    }

    // Formerly Las Vegas algorithm.
    protected static bool ActorPlace(DiceRoller roller, Map map, Actor actor, Rectangle rect, Predicate<Point> goodPositionFn=null)
    {
#if DEBUG
      if (null == map) throw new ArgumentNullException(nameof(map));
      if (null == actor) throw new ArgumentNullException(nameof(actor));
#endif
      List<Point> valid_spawn = rect.Where(pt => map.IsWalkableFor(pt, actor) && (goodPositionFn == null || goodPositionFn(pt)));
      if (0>=valid_spawn.Count) return false;
      var position = roller.Choose(valid_spawn);
      map.PlaceAt(actor, position);
      if (actor.Faction.IsEnemyOf(Models.Factions[(int)Gameplay.GameFactions.IDs.ThePolice]))
        Session.Get.PoliceThreatTracking.RecordSpawn(actor, map, valid_spawn);
      return true;
    }
#endregion

#region Map Objects
    protected static void MapObjectPlace(Map map, int x, int y, MapObject mapObj)
    {
      if (!map.HasMapObjectAt(x, y)) map.PlaceAt(mapObj, new Point(x, y));
    }

    protected static void MapObjectPlace(Map map, Point pt, MapObject mapObj)
    {
      if (!map.HasMapObjectAt(pt)) map.PlaceAt(mapObj, pt);
    }

    protected static void PlaceDoor(Map map, int x, int y, TileModel floor, DoorWindow door)
    {
      map.SetTileModelAt(x, y, floor);
      MapObjectPlace(map, x, y, door);
    }

    protected static void PlaceDoor(Map map, Point pt, TileModel floor, DoorWindow door)
    {
      map.SetTileModelAt(pt, floor);
      MapObjectPlace(map, pt, door);
    }

    protected static void PlaceDoorIfNoObject(Map map, Point pt, TileModel floor, DoorWindow door)
    {
      if (!map.HasMapObjectAt(pt)) PlaceDoor(map, pt, floor, door);
    }

#if DEAD_FUNC
    protected static bool PlaceDoorIfAccessible(Map map, Point pt, TileModel floor, int minAccessibility, DoorWindow door)
    {
      int num = Direction.COMPASS.Select(d => pt+d).Count(pt2 => map.IsWalkable(pt2));  // includes IsInBounds check
      if (num < minAccessibility) return false;
      PlaceDoorIfNoObject(map, pt, floor, door);
      return true;
    }
#endif

    protected static bool PlaceDoorIfAccessibleAndNotAdjacent(Map map, Point pt, TileModel floor, int minAccessibility, DoorWindow door)
    {
      int num = 0;
      foreach (var point2 in pt.Adjacent()) {  // micro-optimized: loop combines a reject-any check with a counting operation
        if (map.IsWalkable(point2)) ++num;
        if (map.GetMapObjectAt(point2) is DoorWindow) return false;
      }
      if (num < minAccessibility) return false;
      PlaceDoorIfNoObject(map, pt, floor, door);
      return true;
    }

    protected static void MapObjectFill(Map map, Rectangle rect, Func<Point, MapObject> createFn)
    {
#if DEBUG
      if (null == createFn) throw new ArgumentNullException(nameof(createFn));
#endif
      rect.DoForEach(pt => {
        createFn(pt)?.PlaceAt(map,pt);   // XXX RNG potentially involved
      }, pt => !map.HasMapObjectAt(pt));
    }

    public static void MapObjectPlaceInGoodPosition(Map map, Rectangle rect, Func<Point, bool> isGoodPosFn, DiceRoller roller, Func<Point, MapObject> createFn)
    {
#if DEBUG
      if (null == createFn) throw new ArgumentNullException(nameof(createFn));
      if (null == isGoodPosFn) throw new ArgumentNullException(nameof(isGoodPosFn));
#endif
      List<Point> pointList = rect.Where(pt => isGoodPosFn(pt) && map.GetTileModelAt(pt).IsWalkable && !map.HasMapObjectAt(pt));
      if (0 >= pointList.Count) return;
      Point pt2 = roller.Choose(pointList);
      createFn(pt2)?.PlaceAt(map,pt2);
    }
#endregion

    protected static void ItemsDrop(Map map, Rectangle rect, Predicate<Point> isGoodPositionFn, Func<Point, Item> createFn)
    {
#if DEBUG
      if (null == createFn) throw new ArgumentNullException(nameof(createFn));
#endif
      rect.DoForEach(pt => createFn(pt)?.DropAt(map, pt), isGoodPositionFn);
    }

    protected static void ClearRectangle(Map map, Rectangle rect, bool clearZones = true)
    {
      for (var left = rect.Left; left < rect.Right; ++left) {
        for (var top = rect.Top; top < rect.Bottom; ++top) {
          var pt = new Point(left, top);
          map.RemoveMapObjectAt(pt);
          map.RemoveAllItemsAt(pt);
          map.RemoveAllDecorationsAt(pt);
          if (clearZones) map.RemoveAllZonesAt(left, top);
          map.GetActorAt(pt)?.RemoveFromMap();
        }
      }
    }

#region Predicates and Actions
    protected static int CountAdjWalls(Map map, int x, int y)
    {
      return map.CountAdjacentTo(x, y, pt => !map.GetTileModelAt(pt).IsWalkable);
    }

    protected static int CountAdjWalls(Map map, Point p)
    {
      return CountAdjWalls(map, p.X, p.Y);
    }

    protected static int CountAdjWalkables(Map map, int x, int y)
    {
      return map.CountAdjacentTo(x, y, pt => map.GetTileModelAt(pt).IsWalkable);
    }

#if DEAD_FUNC
    protected static void PlaceIf(Map map, int x, int y, TileModel floor, Func<int, int, bool> predicateFn, Func<int, int, MapObject> createFn)
    {
#if DEBUG
      if (null == predicateFn) throw new ArgumentNullException(nameof(predicateFn));
      if (null == createFn) throw new ArgumentNullException(nameof(createFn));
#endif
      if (!predicateFn(x, y)) return;
      MapObject mapObj = createFn(x, y);
      if (mapObj == null) return;
      map.SetTileModelAt(x, y, floor);
      MapObjectPlace(map, x, y, mapObj);
    }
#endif

    protected static void PlaceIf(Map map, Point pt, TileModel floor, Func<Point, bool> predicateFn, Func<Point, MapObject> createFn)
    {
#if DEBUG
      if (null == predicateFn) throw new ArgumentNullException(nameof(predicateFn));
      if (null == createFn) throw new ArgumentNullException(nameof(createFn));
#endif
      if (!predicateFn(pt)) return;
      MapObject mapObj = createFn(pt);
      if (mapObj == null) return;
      map.SetTileModelAt(pt, floor);
      MapObjectPlace(map, pt, mapObj);
    }

    protected static bool IsAccessible(Map map, int x, int y)
    {
      return map.CountAdjacentTo(x, y, pt => map.IsWalkable(pt.X, pt.Y)) >= 6;
    }
#endregion
  }
}
