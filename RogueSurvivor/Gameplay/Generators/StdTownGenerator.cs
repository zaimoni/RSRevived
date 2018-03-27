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

    public override Map Generate(int seed, string name)
    {
      Map map = base.Generate(seed, name);
      bool outside_test(Point pt) { return !map.IsInsideAt(pt); };
      // XXX with default 15% policeman probability and default 25 max civilians, 0 police in district is ~1.7% chance and happens noticeably often in testing
      for (int index = 0; index < RogueGame.Options.MaxCivilians; ++index) {
        if (m_DiceRoller.RollChance(Params.PolicemanChance))
          ActorPlace(m_DiceRoller, map, CreateNewPoliceman(0), outside_test);
        else {
          Actor newCivilian = CreateNewCivilian(0, 0, 1);
          ActorPlace(m_DiceRoller, map, newCivilian, pt => map.IsInsideAt(pt) && !map.HasZonePartiallyNamedAt(pt, "NoCivSpawn"));
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
