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
      for (int index = 0; index < RogueGame.Options.MaxCivilians; ++index)
      {
        if (m_DiceRoller.RollChance(Params.PolicemanChance))
        {
          Actor newPoliceman = CreateNewPoliceman(0);
                    ActorPlace(m_DiceRoller, maxTries, map, newPoliceman, (Predicate<Point>) (pt => !map.GetTileAt(pt.X, pt.Y).IsInside));
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
