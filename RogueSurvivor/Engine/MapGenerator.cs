// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.MapGenerator
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;
using System;
using System.Collections.Generic;

using Point = Zaimoni.Data.Vector2D<short>;
using Rectangle = Zaimoni.Data.Box2D<short>;

namespace djack.RogueSurvivor.Engine
{
  public abstract class MapGenerator
  {
    protected MapGenerator() {}

    public abstract Map Generate(int seed, string name, District d);

#region Tile filling
    protected static void TileFill(Map map, TileModel model, bool inside)
    {
      TileFill(map, model, 0, 0, map.Width, map.Height, inside);
    }

    protected static void TileFill(Map map, TileModel model, Action<Tile, TileModel, short, short> decoratorFn=null)
    {
      TileFill(map, model, 0, 0, map.Width, map.Height, decoratorFn);
    }

    protected static void TileFill(Map map, TileModel model, Rectangle rect, bool inside)
    {
      TileFill(map, model, rect.Left, rect.Top, rect.Width, rect.Height, inside);
    }

    protected static void TileFill(Map map, TileModel model, Rectangle rect, Action<Tile, TileModel, short, short> decoratorFn=null)
    {
      TileFill(map, model, rect.Left, rect.Top, rect.Width, rect.Height, decoratorFn);
    }

    protected static void TileFill(Map map, TileModel model, short left, short top, int width, int height, Action<Tile, TileModel, short, short> decoratorFn=null)
    {
#if DEBUG
      if (null == map) throw new ArgumentNullException(nameof(map));
      if (null == model) throw new ArgumentNullException(nameof(model));
#endif
      for (short x = left; x < left + width; ++x) {
        for (short y = top; y < top + height; ++y) {
          TileModel model1 = map.GetTileModelAt(x, y);
          map.SetTileModelAt(x, y, model);
          decoratorFn?.Invoke(map.GetTileAt(x, y), model1, x, y);
        }
      }
    }

    protected static void TileFill(Map map, TileModel model, short left, short top, int width, int height, bool inside)
    {
#if DEBUG
      if (null == map) throw new ArgumentNullException(nameof(map));
      if (null == model) throw new ArgumentNullException(nameof(model));
#endif
      for (short x = left; x < left + width; ++x) {
        for (short y = top; y < top + height; ++y) {
          map.SetTileModelAt(x, y, model);
          map.SetIsInsideAt(x, y, inside);
        }
      }
    }

    protected static void TileHLine(Map map, TileModel model, int left, int top, int width, Action<Tile, TileModel, Point> decoratorFn=null)
    {
#if DEBUG
      if (null == map) throw new ArgumentNullException(nameof(map));
      if (null == model) throw new ArgumentNullException(nameof(model));
#endif
      for (short x = (short)left; x < left + width; ++x) {
        Point pt = new Point(x, (short)top);
        TileModel model1 = map.GetTileModelAt(pt);
        map.SetTileModelAt(pt, model);
        decoratorFn?.Invoke(map.GetTileAt(pt), model1, pt);
      }
    }

    protected static void TileVLine(Map map, TileModel model, int left, int top, int height, Action<Tile, TileModel, Point> decoratorFn=null)
    {
#if DEBUG
      if (null == map) throw new ArgumentNullException(nameof(map));
      if (null == model) throw new ArgumentNullException(nameof(model));
#endif
      for (short y = (short)top; y < top + height; ++y) {
        Point pt = new Point((short)left, y);
        TileModel model1 = map.GetTileModelAt(pt);
        map.SetTileModelAt(pt, model);
        decoratorFn?.Invoke(map.GetTileAt(pt), model1, pt);
      }
    }

    protected static void TileRectangle(Map map, TileModel model, Rectangle rect)
    {
      TileRectangle(map, model, rect.Left, rect.Top, rect.Width, rect.Height);
    }

    protected static void TileRectangle(Map map, TileModel model, short left, short top, int width, int height, Action<Tile, TileModel, Point> decoratorFn=null)
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

    protected static void DoForEachTile(Rectangle rect, Map m, Action<Location> doFn)
    {
#if DEBUG
      if (null == doFn) throw new ArgumentNullException(nameof(doFn));
#endif
      Point point = new Point();
      for (point.X = rect.Left; point.X < rect.Right; ++point.X) {
        for (point.Y = rect.Top; point.Y < rect.Bottom; ++point.Y) {
          doFn(new Location(m, point));
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
      map.PlaceAt(actor, roller.Choose(valid_spawn));
      if (Session.Get.Police.IsEnemy(actor)) Session.Get.Police.Threats.RecordSpawn(actor, map, valid_spawn);
      return true;
    }

    public static bool ActorPlace(Actor actor, Location dest)
    {
#if DEBUG
      if (null == dest.Map) throw new ArgumentNullException(nameof(dest.Map));
      if (null == actor) throw new ArgumentNullException(nameof(actor));
      if (!Map.Canonical(ref dest)) throw new InvalidOperationException("location cannot be made canonical");
#endif
      dest.Map.PlaceAt(actor, dest.Position);
      if (Session.Get.Police.IsEnemy(actor)) Session.Get.Police.Threats.RecordTaint(actor, dest);
      return true;
    }
#endregion

#region Map Objects
    protected static void MapObjectPlace(Map map, Point pt, MapObject mapObj)
    {
      if (!map.HasMapObjectAt(pt)) map.PlaceAt(mapObj, pt);
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
        createFn(pt)?.PlaceAt(map, in pt);   // XXX RNG potentially involved
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
      createFn(pt2)?.PlaceAt(map, in pt2);
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
          if (clearZones) map.RemoveAllZonesAt(pt);
          map.GetActorAt(pt)?.RemoveFromMap();
        }
      }
    }

#region Predicates and Actions
    protected static int CountAdjWalls(Map map, Point p)
    {
      return map.CountAdjacentTo(p, pt => !map.GetTileModelAt(pt).IsWalkable);
    }

    protected static int CountAdjWalkables(Map map, in Point p)
    {
      return map.CountAdjacentTo(p, pt => map.GetTileModelAt(pt).IsWalkable);
    }

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

    protected static bool IsAccessible(Map map, in Point pos)
    {
      return map.CountAdjacentTo(pos, pt => map.IsWalkable(pt)) >= 6;
    }
#endregion
  }
}
