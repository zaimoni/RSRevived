// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.MapGenerator
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics.Contracts;
using Zaimoni.Data;

namespace djack.RogueSurvivor.Engine
{
  internal abstract class MapGenerator
  {
    protected readonly Rules m_Rules;

    public MapGenerator(Rules rules)
    {
	  Contract.Requires(null != rules);
      m_Rules = rules;
    }

    public abstract Map Generate(int seed, string name);

#region Tile filling
    public void TileFill(Map map, TileModel model, bool inside)
    {
      TileFill(map, model, 0, 0, map.Width, map.Height, inside);
    }

    public void TileFill(Map map, TileModel model, Action<Tile, TileModel, int, int> decoratorFn=null)
    {
      TileFill(map, model, 0, 0, map.Width, map.Height, decoratorFn);
    }

    public void TileFill(Map map, TileModel model, Rectangle rect, bool inside)
    {
      TileFill(map, model, rect.Left, rect.Top, rect.Width, rect.Height, inside);
    }

    public void TileFill(Map map, TileModel model, Rectangle rect, Action<Tile, TileModel, int, int> decoratorFn=null)
    {
      TileFill(map, model, rect.Left, rect.Top, rect.Width, rect.Height, decoratorFn);
    }

    public void TileFill(Map map, TileModel model, int left, int top, int width, int height, Action<Tile, TileModel, int, int> decoratorFn=null)
    {
      Contract.Requires(null != map);
      Contract.Requires(null != model);
      for (int x = left; x < left + width; ++x) {
        for (int y = top; y < top + height; ++y) {
          TileModel model1 = map.GetTileModelAt(x, y);
          map.SetTileModelAt(x, y, model);
          if (decoratorFn != null)
            decoratorFn(map.GetTileAt(x, y), model1, x, y);
        }
      }
    }

    public void TileFill(Map map, TileModel model, int left, int top, int width, int height, bool inside)
    {
      Contract.Requires(null != map);
      Contract.Requires(null != model);
      for (int x = left; x < left + width; ++x) {
        for (int y = top; y < top + height; ++y) {
          map.SetTileModelAt(x, y, model);
          map.SetIsInsideAt(x, y, inside);
        }
      }
    }

    public void TileHLine(Map map, TileModel model, int left, int top, int width, Action<Tile, TileModel, int, int> decoratorFn=null)
    {
      Contract.Requires(null != map);
      Contract.Requires(null != model);
      for (int x = left; x < left + width; ++x) {
        TileModel model1 = map.GetTileModelAt(x, top);
        map.SetTileModelAt(x, top, model);
        if (decoratorFn != null)
          decoratorFn(map.GetTileAt(x, top), model1, x, top);
      }
    }

    public void TileVLine(Map map, TileModel model, int left, int top, int height, Action<Tile, TileModel, int, int> decoratorFn=null)
    {
      Contract.Requires(null != map);
      Contract.Requires(null != model);
      for (int y = top; y < top + height; ++y) {
        TileModel model1 = map.GetTileModelAt(left, y);
        map.SetTileModelAt(left, y, model);
        if (decoratorFn != null)
          decoratorFn(map.GetTileAt(left, y), model1, left, y);
      }
    }

    public void TileRectangle(Map map, TileModel model, Rectangle rect)
    {
      TileRectangle(map, model, rect.Left, rect.Top, rect.Width, rect.Height);
    }

    public void TileRectangle(Map map, TileModel model, int left, int top, int width, int height, Action<Tile, TileModel, int, int> decoratorFn=null)
    {
      Contract.Requires(null != map);
      Contract.Requires(null != model);
      TileHLine(map, model, left, top, width, decoratorFn);
      TileHLine(map, model, left, top + height - 1, width, decoratorFn);
      TileVLine(map, model, left, top, height, decoratorFn);
      TileVLine(map, model, left + width - 1, top, height, decoratorFn);
    }

    // dead, but typical for a map generation utility
    public Point DigUntil(Map map, TileModel model, Point startPos, Direction digDirection, Predicate<Point> stopFn)
    {
      Point p = startPos + digDirection;
      while (map.IsInBounds(p) && !stopFn(p))
      {
        map.SetTileModelAt(p.X, p.Y, model);
        p += digDirection;
      }
      return p;
    }

    public void DoForEachTile(Rectangle rect, Action<Point> doFn)
    {
      Contract.Requires(null != doFn);
      Point point = new Point();
      for (point.X = rect.Left; point.X < rect.Right; ++point.X) {
        for (point.Y = rect.Top; point.Y < rect.Bottom; ++point.Y) {
          doFn(point);
        }
      }
    }

    public bool CheckForEachTile(Rectangle rect, Predicate<Point> predFn)
    {
      Contract.Requires(null != predFn);
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
    public bool ActorPlace(DiceRoller roller, Map map, Actor actor, Predicate<Point> goodPositionFn=null)
    {
      return ActorPlace(roller, map, actor, map.Rect, goodPositionFn);
    }

    // Formerly Las Vegas algorithm.
    public bool ActorPlace(DiceRoller roller, Map map, Actor actor, Rectangle rect, Predicate<Point> goodPositionFn=null)
    {
      Contract.Requires(null != map);
      Contract.Requires(null != actor);
      Point position = new Point();
      List<Point> valid_spawn = rect.Where(pt => map.IsWalkableFor(pt, actor) && (goodPositionFn == null || goodPositionFn(pt)));
      if (0>=valid_spawn.Count) return false;
      position = valid_spawn[roller.Roll(0,valid_spawn.Count)];
      map.PlaceActorAt(actor, position);
      if (actor.Faction.IsEnemyOf(Models.Factions[(int)Gameplay.GameFactions.IDs.ThePolice]))
        Session.Get.PoliceThreatTracking.RecordSpawn(actor, map, valid_spawn);
      return true;
    }
#endregion

#region Map Objects
    public void MapObjectPlace(Map map, int x, int y, MapObject mapObj)
    {
      if (!map.HasMapObjectAt(x, y)) map.PlaceMapObjectAt(mapObj, new Point(x, y));
    }

    public void MapObjectFill(Map map, Rectangle rect, Func<Point, MapObject> createFn)
    {
      rect.DoForEach(pt => { 
        createFn(pt)?.PlaceAt(map,pt);   // XXX RNG potentially involved
      }, pt => !map.HasMapObjectAt(pt));
    }

    public void MapObjectPlaceInGoodPosition(Map map, Rectangle rect, Func<Point, bool> isGoodPosFn, DiceRoller roller, Func<Point, MapObject> createFn)
    {
      List<Point> pointList = rect.Where(pt => isGoodPosFn(pt) && !map.HasMapObjectAt(pt));
      if (0 >= pointList.Count) return;
      Point pt2 = pointList[roller.Roll(0, pointList.Count)];
      createFn(pt2)?.PlaceAt(map,pt2);
    }
#endregion

    public void ItemsDrop(Map map, Rectangle rect, Predicate<Point> isGoodPositionFn, Func<Point, Item> createFn)
    {
      rect.DoForEach(pt => createFn(pt)?.DropAt(map, pt), isGoodPositionFn);
    }

    protected void ClearRectangle(Map map, Rectangle rect)
    {
      for (int left = rect.Left; left < rect.Right; ++left) {
        for (int top = rect.Top; top < rect.Bottom; ++top) {
          map.RemoveMapObjectAt(left, top);
          Inventory itemsAt = map.GetItemsAt(left, top);
          if (itemsAt != null) {
            while (!itemsAt.IsEmpty)
              map.RemoveItemAt(itemsAt[0], left, top);
          }
          map.RemoveAllDecorationsAt(left, top);
          map.RemoveAllZonesAt(left, top);
          Actor actorAt = map.GetActorAt(left, top);
          if (actorAt != null) map.RemoveActor(actorAt);
        }
      }
    }

#region Predicates and Actions
    public int CountAdjWalls(Map map, int x, int y)
    {
      return map.CountAdjacentTo(x, y, pt => !map.GetTileModelAt(pt).IsWalkable);
    }

    public int CountAdjWalls(Map map, Point p)
    {
      return CountAdjWalls(map, p.X, p.Y);
    }

    public int CountAdjWalkables(Map map, int x, int y)
    {
      return map.CountAdjacentTo(x, y, pt => map.GetTileModelAt(pt).IsWalkable);
    }

    public int CountAdjDoors(Map map, int x, int y)
    {
      return map.CountAdjacentTo(x, y, pt => map.GetMapObjectAt(pt.X, pt.Y) is DoorWindow);
    }

    public void PlaceIf(Map map, int x, int y, TileModel floor, Func<int, int, bool> predicateFn, Func<int, int, MapObject> createFn)
    {
      Contract.Requires(null != predicateFn);
      Contract.Requires(null != createFn);
      if (!predicateFn(x, y)) return;
      MapObject mapObj = createFn(x, y);
      if (mapObj == null) return;
      map.SetTileModelAt(x, y, floor);
      MapObjectPlace(map, x, y, mapObj);
    }

    public bool IsAccessible(Map map, int x, int y)
    {
      return map.CountAdjacentTo(x, y, pt => map.IsWalkable(pt.X, pt.Y)) >= 6;
    }

    public bool HasNoObjectAt(Map map, int x, int y)
    {
      return map.GetMapObjectAt(x, y) == null;
    }

    public bool IsInside(Map map, int x, int y)
    {
      return map.GetTileAt(x, y).IsInside;
    }

    public bool HasInRange(Map map, Point from, int maxDistance, Predicate<Point> predFn)
    {
      int x1 = from.X - maxDistance;
      int y1 = from.Y - maxDistance;
      int x2 = from.X + maxDistance;
      int y2 = from.Y + maxDistance;
      map.TrimToBounds(ref x1, ref y1);
      map.TrimToBounds(ref x2, ref y2);
      Point point = new Point();
      for (int index1 = x1; index1 <= x2; ++index1)
      {
        point.X = index1;
        for (int index2 = y1; index2 <= y2; ++index2)
        {
          point.Y = index2;
          if ((index1 != from.X || index2 != from.Y) && predFn(point))
            return true;
        }
      }
      return false;
    }
#endregion
  }
}
