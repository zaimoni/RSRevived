// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.Generators.StdTownGenerator
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using System;
using System.Drawing;

namespace djack.RogueSurvivor.Gameplay.Generators
{
  internal class StdTownGenerator : BaseTownGenerator
  {
    public StdTownGenerator(RogueGame game, BaseTownGenerator.Parameters parameters)
      : base(game, parameters)
    {
    }

    public override Map Generate(int seed)
    {
      Map map = base.Generate(seed);
      map.Name = "Std City";
      int maxTries = 10 * map.Width * map.Height;
      bool have_placed_cop = false;
      for (int index = 0; index < RogueGame.Options.MaxCivilians; ++index)
      {
        if (m_DiceRoller.RollChance(Params.PolicemanChance))
        {
          Actor newPoliceman = CreateNewPoliceman(0);
          if (ActorPlace(m_DiceRoller, maxTries, map, newPoliceman, (Predicate<Point>) (pt => !map.GetTileAt(pt.X, pt.Y).IsInside))) have_placed_cop = true;
        }
        else
        {
          Actor newCivilian = CreateNewCivilian(0, 0, 1);
          ActorPlace(m_DiceRoller, maxTries, map, newCivilian, (Predicate<Point>) (pt => map.GetTileAt(pt.X, pt.Y).IsInside));
        }
      }
      for (int index = 0; index < RogueGame.Options.MaxDogs; ++index)
      {
        Actor newFeralDog = CreateNewFeralDog(0);
        ActorPlace(m_DiceRoller, maxTries, map, newFeralDog, (Predicate<Point>) (pt => !map.GetTileAt(pt.X, pt.Y).IsInside));
      }
      int num = RogueGame.Options.MaxUndeads * RogueGame.Options.DayZeroUndeadsPercent / 100;
      for (int index = 0; index < num; ++index)
      {
        Actor newUndead = CreateNewUndead(0);
        ActorPlace(m_DiceRoller, maxTries, map, newUndead, (Predicate<Point>) (pt => !map.GetTileAt(pt.X, pt.Y).IsInside));
      }
#if FAIL
      / NOTE: this is too early, the map's district is not set
      // successfully placing a cop means the police faction knows all outside squares (map revealing effect)
      if (have_placed_cop) {
        Point pos = new Point(0);
        for (pos.X = 0; pos.X < map.Width; ++pos.X) {
          for (pos.Y = 0; pos.Y < map.Height; ++pos.Y) {
            if (map.GetTileAt(pos.X, pos.Y).IsInside) continue;
            Session.Get.ForcePoliceKnown(new Location(map,pos));
          }
        }
      }
#endif
      return map;
    }

    public override Map GenerateSewersMap(int seed, District district)
    {
      Map sewersMap = base.GenerateSewersMap(seed, district);
      if (m_Game.Session.HasZombiesInSewers)
      {
        int maxTries = 10 * sewersMap.Width * sewersMap.Height;
        int num = (int) (0.5 * (double) (RogueGame.Options.MaxUndeads * RogueGame.Options.DayZeroUndeadsPercent) / 100.0);
        for (int index = 0; index < num; ++index)
        {
          Actor newSewersUndead = CreateNewSewersUndead(0);
          ActorPlace(m_DiceRoller, maxTries, sewersMap, newSewersUndead);
        }
      }
      return sewersMap;
    }

    public override Map GenerateSubwayMap(int seed, District district)
    {
      return base.GenerateSubwayMap(seed, district);
    }
  }
}
