// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.Generators.StdTownGenerator
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using System;
using System.Collections.Generic;
using Zaimoni.Data;

using Point = Zaimoni.Data.Vector2D<short>;

namespace djack.RogueSurvivor.Gameplay.Generators
{
  internal class StdTownGenerator : BaseTownGenerator
  {
    public StdTownGenerator(Parameters parameters) : base(parameters) {}

    public override Map Generate(int seed, string name, District d)
    {
      Map map = base.Generate(seed, name, d);
      bool outside_test(Point pt) { return !map.IsInsideAt(pt); }
      // XXX with default 15% policeman probability and default 25 max civilians, 0 police in district is ~1.7% chance and happens noticeably often in testing
      bool inside_test(Point pt) { return map.IsInsideAt(pt) && !map.HasZoneNamedAt(pt, "NoCivSpawn"); }
      bool has_subway = null!=map.GetZoneByPartialName("Subway Station");
      bool has_CHAR_office = null!=map.GetZoneByPartialName("CHAR Office");
      var unclaimed_sheds = map.GetZonesByPartialName("Shed");
      List<Zone>? claimed_sheds = null;
      List<Zone>? full_sheds = null;

      for (int index = 0; index < RogueGame.Options.MaxCivilians; ++index) {
        if (m_DiceRoller.RollChance(Parameters.PolicemanChance)) {
          Predicate<Point> ok = outside_test;
          // 2022-02-17: Require starting next to a wrecked car.  We'd prefer that it be a police car, but the data
          // for that isn't (yet) in-game.
          // This prevents some outlandish spacings in the interstate districts.
          ok = ok.And(pt => {
            var could_be_policecars = map.FindAdjacent<MapObject>(pt, (m,pos) => {
                var obj = m.GetMapObjectAtExt(pos);
                // avoiding exact-id test for now
                if (null != obj && MapObject.CAR_WEIGHT == obj.Weight) return obj;
                return null;
            });
            return 0 < could_be_policecars.Count;
          });

          ActorPlace(m_DiceRoller, map, CreateNewPoliceman(0), ok);
        } else {
          // no unusual handling for residences or CHAR Agencies.
          // \todo: policy decision re shop owners (would be ok to have staff and immediate relatives of owner and staff).
          // Consider forcing shop owners to be present at their shops
          // \todo: when lifting sewers to early-generation, strongly restrict extras.  Allow one significant other.
          Actor newCivilian = CreateNewCivilian(0, 0, 1);
          var zzz = new HashSet<Point>();
          var shed_claimed = new HashSet<Point>();
          Predicate<Point> ok = inside_test;
          // Park sheds can get crowded fast.  #1 takes the central square (5 in reach).
          // VAPORWARE #2 takes the square between the central square and the door.
          // disallow more than 2.
          if (null != unclaimed_sheds) {
            if (null != claimed_sheds) {
              ok = ok.And(pt => {
                if (null != claimed_sheds.Find(z => z.Bounds.Contains(pt))) return false;   // XXX \todo extend
                var z2 = unclaimed_sheds.Find(z => z.Bounds.Contains(pt));
                if (null == z2) return true;
                var center = z2.Bounds.Location + z2.Bounds.Size/2;
                if (pt == center) { // grab center slot
                  shed_claimed.Add(pt);
                  return true;
                } else return false;
              });
            } else {
              ok = ok.And(pt => {
                var z2 = unclaimed_sheds.Find(z => z.Bounds.Contains(pt));
                if (null == z2) return true;
                var center = z2.Bounds.Location + z2.Bounds.Size/2;
                if (pt == center) { // grab center slot
                  shed_claimed.Add(pt);
                  return true;
                } else return false;
              });
            }
          } else if (null != claimed_sheds) {
            if (null != full_sheds) {
              ok = ok.And(pt => {
                if (null != full_sheds.Find(z => z.Bounds.Contains(pt))) return false;   // XXX \todo extend
                var z2 = claimed_sheds.Find(z => z.Bounds.Contains(pt));
                if (null == z2) return true;
                var center = z2.Bounds.Location + z2.Bounds.Size/2;
                foreach(var dir in Direction.COMPASS_4) {
                  var pt2 = center+dir;
                  if (null != map.GetMapObjectAt(pt2)) continue;
                  if (pt == pt2) {
                    shed_claimed.Add(pt);
                    return true;
                  } else return false;
                }
                return false;
              });
            } else {
              ok = ok.And(pt => {
                var z2 = claimed_sheds.Find(z => z.Bounds.Contains(pt));
                if (null == z2) return true;
                var center = z2.Bounds.Location + z2.Bounds.Size/2;
                foreach(var dir in Direction.COMPASS_4) {
                  var pt2 = center+dir;
                  if (null != map.GetMapObjectAt(pt2)) continue;
                  if (pt == pt2) {
                    shed_claimed.Add(pt);
                    return true;
                  } else return false;
                }
                return false;
              });
            }
          } else if (null != full_sheds) {
            ok = ok.And(pt => null == full_sheds.Find(z => z.Bounds.Contains(pt)));
          }

          // Unfortunately, CHAR did represent to the police that the offices were ok as curfew refuges.
          // The CHAR Guard Manuals only approve of this for lesser disasters than a z apocalypse.
          // Fortunately, airliners that crash have 1/3rd fewer passengers than those that don't!
          // There may also be some "prison barge" rumors circulating (cf. the 2018 Hawaii volcanic eruption, and graffiti at game start)
          if (has_CHAR_office && m_DiceRoller.RollChance(33)) ok= ok.And(pt => !map.HasZonePrefixNamedAt(pt, "CHAR Office@"));
          // subways are now early-spawn.  Since they're a legal area for the PC to spawn we have to allow them, but 
          // they're not really emergency shelters (the trains are in storage).
          if (has_subway) {
            if (m_DiceRoller.RollChance(50)) { // \todo tune this empirical constant.  Ideally would respond to charisma skill, but that's not player-visible
              ok = ok.And(pt => {
                if (!map.HasZonePrefixNamedAt(pt, "Subway Station@")) return true;
                if (map.GetMapObjectAt(pt)?.IsCouch ?? false) {
                  zzz.Add(pt);  // side-effecting, so prefers to be last
                  return true;
                } else return false;
              });
            } else {    // not convincing: bounced
              ok = ok.And(pt => !map.HasZonePrefixNamedAt(pt, "Subway Station@"));
            }
          }

          if (!ActorPlace(m_DiceRoller, map, newCivilian, ok)) continue;    // XXX \todo record and use the non-spawn elsewhere
          if (zzz.Contains(newCivilian.Location.Position)) {    // start the game sleeping
            newCivilian.Activity = Activity.SLEEPING;
            newCivilian.IsSleeping = true;
          } else if (shed_claimed.Contains(newCivilian.Location.Position)) {
            int i = unclaimed_sheds.FindIndex(z => z.Bounds.Contains(newCivilian.Location.Position));
            if (0 <= i) {
              (claimed_sheds ??= new()).Add(unclaimed_sheds[i]);
              unclaimed_sheds.RemoveAt(i);
            } else {
              i = claimed_sheds.FindIndex(z => z.Bounds.Contains(newCivilian.Location.Position));
              if (0 <= i) {
                (full_sheds ??= new()).Add(claimed_sheds[i]);
                claimed_sheds.RemoveAt(i);
              }
            }
          }
        }
      }
      int num = RogueGame.Options.MaxDogs;
      while(0 < num--) ActorPlace(m_DiceRoller, map, CreateNewFeralDog(0), outside_test);
      num = RogueGame.Options.MaxUndeads * RogueGame.Options.DayZeroUndeadsPercent / 100;
      while(0 < num--) ActorPlace(m_DiceRoller, map, CreateNewUndead(0), outside_test);
      Session.Get.Police.Threats.Rebuild(map);   // prune back RAM cost
      map.OnMapGenerated(); // remove deadwood that should not hit the savefile
      return map;
    }

    public override Map GenerateSewersMap(int seed, District district)
    {
      Map sewersMap = base.GenerateSewersMap(seed, district);
      if (Session.Get.HasZombiesInSewers) {
        int num = (int) (0.5 * (double) (RogueGame.Options.MaxUndeads * RogueGame.Options.DayZeroUndeadsPercent) / 100.0);
        while(0 < num--) ActorPlace(m_DiceRoller, sewersMap, CreateNewSewersUndead(0));
      }
      Session.Get.Police.Threats.Rebuild(sewersMap);   // prune back RAM cost
      return sewersMap;
    }
  }
}
