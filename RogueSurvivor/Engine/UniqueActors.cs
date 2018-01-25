// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.UniqueActors
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class UniqueActors
  {
    public UniqueActor BigBear { get; set; }

    public UniqueActor Duckman { get; set; }

    public UniqueActor FamuFataru { get; set; }

    public UniqueActor HansVonHanz { get; set; }

    public UniqueActor JasonMyers { get; set; }

    public UniqueActor PoliceStationPrisonner { get; private set; }

    public UniqueActor Roguedjack { get; set; }

    public UniqueActor Santaman { get; set; }

    public UniqueActor TheSewersThing { get; set; }

    // \todo NEXT SAVEFILE BREAK: Father Time, with a legendary scythe.
    public UniqueActor[] ToArray()
    {
      return new UniqueActor[8]
      {
        BigBear,
        Duckman,
        FamuFataru,
        HansVonHanz,
        PoliceStationPrisonner,
        Roguedjack,
        Santaman,
        TheSewersThing
      };
    }

    public void init_Prisoner(Data.Actor newCivilian)
    {
      if (null != PoliceStationPrisonner) throw new InvalidOperationException("only call UniqueActors::init_Prisoner once");
      newCivilian.Name = "The Prisoner Who Should Not Be";
      for (int index = 0; index < newCivilian.Inventory.MaxCapacity; ++index)
        newCivilian.Inventory.AddAll(Gameplay.Generators.BaseMapGenerator.MakeItemArmyRation());
      PoliceStationPrisonner = new UniqueActor(newCivilian,true);
    }
  }
}
