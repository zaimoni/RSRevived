// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.UniqueActors
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using djack.RogueSurvivor.Gameplay;

using Actor = djack.RogueSurvivor.Data.Actor;
using BaseMapGenerator = djack.RogueSurvivor.Gameplay.Generators.BaseMapGenerator;
using BaseTownGenerator = djack.RogueSurvivor.Gameplay.Generators.BaseTownGenerator;
using DollPart = djack.RogueSurvivor.Data.DollPart;
using ItemMeleeWeapon = djack.RogueSurvivor.Engine.Items.ItemMeleeWeapon;
using ItemRangedWeapon = djack.RogueSurvivor.Engine.Items.ItemRangedWeapon;

// Note that game-specific content was already here in RS alpha 9 (the identities of the unique actors)
namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class UniqueActors
  {
    public UniqueActor BigBear { get; set; }
    public UniqueActor Duckman { get; private set; }
    public UniqueActor FamuFataru { get; private set; }
    public UniqueActor HansVonHanz { get; private set; }
    public UniqueActor JasonMyers { get; set; }
    public UniqueActor PoliceStationPrisonner { get; private set; }
    public UniqueActor Roguedjack { get; private set; }
    public UniqueActor Santaman { get; private set; }
    public UniqueActor TheSewersThing { get; set; }

    // \todo NEXT SAVEFILE BREAK: Father Time, with a legendary scythe.
    // This must be a value copy anyway (i.e. we gain almost nothing from having the authoritative storage be an array rather than auto properties)
    public UniqueActor[] ToArray()
    {
      return new UniqueActor[] {
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
#if DEBUG
      if (null != PoliceStationPrisonner) throw new InvalidOperationException("only call UniqueActors::init_Prisoner once");
#endif
      newCivilian.Name = "The Prisoner Who Should Not Be";
      for (int index = 0; index < newCivilian.Inventory.MaxCapacity; ++index)
        newCivilian.Inventory.AddAll(Gameplay.Generators.BaseMapGenerator.MakeItemArmyRation());
      PoliceStationPrisonner = new UniqueActor(newCivilian,true);
    }

    public void init_FamuFataru(BaseTownGenerator tgen)
    {
#if DEBUG
      if (null != FamuFataru) throw new InvalidOperationException("only call UniqueActors::init_Santaman once");
#endif
      Actor named = GameActors.FemaleCivilian.CreateNamed(GameFactions.TheCivilians, "Famu Fataru", false, 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_FAMU_FATARU);
      named.StartingSkill(Skills.IDs.HAULER,3);
      named.StartingSkill(Skills.IDs.HARDY,5);
      named.StartingSkill(Skills.IDs._FIRST,5);
      named.StartingSkill(Skills.IDs.HIGH_STAMINA,5);
      named.Inventory.AddAll(new ItemMeleeWeapon(GameItems.UNIQUE_FAMU_FATARU_KATANA));
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      FamuFataru = new UniqueActor(named,false,true, GameMusics.FAMU_FATARU_THEME_SONG, "You hear a woman laughing.");
    }

    public void init_Santaman(BaseTownGenerator tgen)
    {
#if DEBUG
      if (null != Santaman) throw new InvalidOperationException("only call UniqueActors::init_Santaman once");
#endif
      Actor named = GameActors.MaleCivilian.CreateNamed(GameFactions.TheCivilians, "Santaman", false, 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_SANTAMAN);
      named.StartingSkill(Skills.IDs.HAULER,3);
      named.StartingSkill(Skills.IDs.HARDY,5);
      named.StartingSkill(Skills.IDs.AGILE,5);
      named.StartingSkill(Skills.IDs.FIREARMS,5);
      named.Inventory.AddAll(new ItemRangedWeapon(GameItems.UNIQUE_SANTAMAN_SHOTGUN));
      named.Inventory.AddAll(BaseMapGenerator.MakeItemShotgunAmmo());
      named.Inventory.AddAll(BaseMapGenerator.MakeItemShotgunAmmo());
      named.Inventory.AddAll(BaseMapGenerator.MakeItemShotgunAmmo());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      Santaman = new UniqueActor(named,false,true, GameMusics.SANTAMAN_THEME_SONG, "You hear christmas music and drunken vomiting.");
    }

    public void init_Roguedjack(BaseTownGenerator tgen)
    {
#if DEBUG
      if (null != Roguedjack) throw new InvalidOperationException("only call UniqueActors::init_Roguedjack once");
#endif
      Actor named = GameActors.MaleCivilian.CreateNamed(GameFactions.TheCivilians, "Roguedjack", false, 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_ROGUEDJACK);
      named.StartingSkill(Skills.IDs.HAULER,3);
      named.StartingSkill(Skills.IDs.HARDY,5);
      named.StartingSkill(Skills.IDs.LEADERSHIP,5);
      named.StartingSkill(Skills.IDs.CHARISMATIC,5);
      named.Inventory.AddAll(new ItemMeleeWeapon(GameItems.UNIQUE_ROGUEDJACK_KEYBOARD));
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      Roguedjack = new UniqueActor(named,false,true, GameMusics.ROGUEDJACK_THEME_SONG, "You hear a man shouting in French.");
    }


    public void init_Duckman(BaseTownGenerator tgen)
    {
#if DEBUG
      if (null != Duckman) throw new InvalidOperationException("only call UniqueActors::init_Duckman once");
#endif
      Actor named = GameActors.MaleCivilian.CreateNamed(GameFactions.TheCivilians, "Duckman", false, 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_DUCKMAN);
      named.StartingSkill(Skills.IDs.CHARISMATIC,5);
      named.StartingSkill(Skills.IDs.LEADERSHIP);
      named.StartingSkill(Skills.IDs.STRONG,5);
      named.StartingSkill(Skills.IDs.HIGH_STAMINA,5);
      named.StartingSkill(Skills.IDs.MARTIAL_ARTS,5);
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      Duckman = new UniqueActor(named,false,true, GameMusics.DUCKMAN_THEME_SONG, "You hear loud demented QUACKS.");
    }

    public void init_HansVonHanz(BaseTownGenerator tgen)
    {
#if DEBUG
      if (null != HansVonHanz) throw new InvalidOperationException("only call UniqueActors::init_HanzVonHanz once");
#endif
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
