// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.UniqueActors
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using djack.RogueSurvivor.Gameplay;

using Actor = djack.RogueSurvivor.Data.Actor;
using BaseTownGenerator = djack.RogueSurvivor.Gameplay.Generators.BaseTownGenerator;
using DollPart = djack.RogueSurvivor.Data.DollPart;
using ItemRangedWeapon = djack.RogueSurvivor.Engine.Items.ItemRangedWeapon;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class UniqueActors
  {
    public UniqueActor BigBear { get; set; }
    public UniqueActor Duckman { get; set; }
    public UniqueActor FamuFataru { get; set; }
    public UniqueActor HansVonHanz { get; private set; }
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

    public void init_Prisoner(Actor newCivilian)
    {
      if (null != PoliceStationPrisonner) throw new InvalidOperationException("only call UniqueActors::init_Prisoner once");
      newCivilian.Name = "The Prisoner Who Should Not Be";
      for (int index = 0; index < newCivilian.Inventory.MaxCapacity; ++index)
        newCivilian.Inventory.AddAll(Gameplay.Generators.BaseMapGenerator.MakeItemArmyRation());
      PoliceStationPrisonner = new UniqueActor(newCivilian,true);
    }

    public void init_HansVonHanz(BaseTownGenerator tgen)
    {
      if (null != HansVonHanz) throw new InvalidOperationException("only call UniqueActors::init_HanzVonHanz once");
      Actor named = GameActors.MaleCivilian.CreateNamed(GameFactions.TheCivilians, "Hans von Hanz", false, 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_HANS_VON_HANZ);
      named.StartingSkill(Skills.IDs.HAULER,3);
      named.StartingSkill(Skills.IDs.FIREARMS,5);
      named.StartingSkill(Skills.IDs.LEADERSHIP,5);
      named.StartingSkill(Skills.IDs.NECROLOGY,5);
      named.Inventory.AddAll(new ItemRangedWeapon(GameItems.UNIQUE_HANS_VON_HANZ_PISTOL));
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      HansVonHanz = new UniqueActor(named,false,true,GameMusics.HANS_VON_HANZ_THEME_SONG, "You hear a man barking orders in German.");
    }
  }
}
