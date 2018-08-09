// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.Generators.StdTownGenerator
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using System;
using System.Drawing;
using System.Collections.Generic;
using Zaimoni.Data;

namespace djack.RogueSurvivor.Gameplay.Generators
{
  internal class StdTownGenerator : BaseTownGenerator
  {
    public StdTownGenerator(RogueGame game, BaseTownGenerator.Parameters parameters)
      : base(game, parameters)
    {
    }

    public override Map Generate(int seed, string name)
    {
      Map map = base.Generate(seed, name);
      bool outside_test(Point pt) { return !map.IsInsideAt(pt); };
      // XXX with default 15% policeman probability and default 25 max civilians, 0 police in district is ~1.7% chance and happens noticeably often in testing
      bool inside_test(Point pt) { return map.IsInsideAt(pt) && !map.HasZonePartiallyNamedAt(pt, "NoCivSpawn"); };
      bool has_subway = null!=map.GetZoneByPartialName("Subway Station");
      bool has_CHAR_office = null!=map.GetZoneByPartialName("CHAR Office");

      for (int index = 0; index < RogueGame.Options.MaxCivilians; ++index) {
        if (m_DiceRoller.RollChance(Params.PolicemanChance))
          ActorPlace(m_DiceRoller, map, CreateNewPoliceman(0), outside_test);
        else {
          // no unusual handling for residences or CHAR Agencies.
          // \todo: policy decision re shop owners (would be ok to have staff and immediate relatives of owner and staff).
          // Consider forcing shop owners to be present at their shops
          // \todo: when lifting sewers to early-generation, strongly restrict extras.  Allow one significant other.
          Actor newCivilian = CreateNewCivilian(0, 0, 1);
          var zzz = new HashSet<Point>();
          Predicate<Point> ok_1 = inside_test;
          // Unfortunately, CHAR did represent to the police that the offices were ok as curfew refuges.
          // The CHAR Guard Manuals only approve of this for lesser disasters than a z apocalypse.
          // Fortunately, airliners that crash have 1/3rd fewer passengers than those that don't!
          // There may also be some "prison barge" rumors circulating (cf. the 2018 Hawaii volcanic eruption, and graffiti at game start)
          if (has_CHAR_office && m_DiceRoller.RollChance(33)) ok_1 = ((Predicate < Point >)inside_test).And(pt => !map.HasZonePartiallyNamedAt(pt, "CHAR Office"));
          // subways are now early-spawn.  Since they're a legal area for the PC to spawn we have to allow them, but 
          // they're not really emergency shelters (the trains are in storage).
          Predicate<Point> ok = ok_1;
          if (has_subway) {
            if (m_DiceRoller.RollChance(50)) { // \todo tune this empirical constant.  Ideally would respond to charisma skill, but that's not player-visible
              ok = ok_1.And(pt => {
                if (!map.HasZonePartiallyNamedAt(pt, "Subway Station")) return true;
                if (map.GetMapObjectAt(pt)?.IsCouch ?? false) {
                  zzz.Add(pt);  // side-effecting, so prefers to be last
                  return true;
                } else return false;
              });
            } else {    // not convincing: bounced
              ok = ok_1.And(pt => !map.HasZonePartiallyNamedAt(pt, "Subway Station"));
            }
          }

          if (ActorPlace(m_DiceRoller, map, newCivilian, ok) && zzz.Contains(newCivilian.Location.Position)) {    // start the game sleeping
            newCivilian.Activity = Activity.SLEEPING;
            newCivilian.IsSleeping = true;
          }
        }
      }
      for (int index = 0; index < RogueGame.Options.MaxDogs; ++index) {
        Actor newFeralDog = CreateNewFeralDog(0);
        ActorPlace(m_DiceRoller, map, newFeralDog, outside_test);
      }
      int num = RogueGame.Options.MaxUndeads * RogueGame.Options.DayZeroUndeadsPercent / 100;
      for (int index = 0; index < num; ++index) {
        Actor newUndead = CreateNewUndead(0);
        ActorPlace(m_DiceRoller, map, newUndead, outside_test);
      }
      map.OnMapGenerated(); // remove deadwood that should not hit the savefile
      return map;
    }

    public override Map GenerateSewersMap(int seed, District district)
    {
      Map sewersMap = base.GenerateSewersMap(seed, district);
      if (Session.Get.HasZombiesInSewers) {
        int num = (int) (0.5 * (double) (RogueGame.Options.MaxUndeads * RogueGame.Options.DayZeroUndeadsPercent) / 100.0);
        for (int index = 0; index < num; ++index) {
          ActorPlace(m_DiceRoller, sewersMap, CreateNewSewersUndead(0));
        }
      }
      return sewersMap;
    }
  }
}
