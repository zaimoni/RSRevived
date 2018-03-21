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
using Map = djack.RogueSurvivor.Data.Map;
using World = djack.RogueSurvivor.Data.World;
using Zone = djack.RogueSurvivor.Data.Zone;

// Note that game-specific content was already here in RS alpha 9 (the identities of the unique actors)
namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  internal class UniqueActors
  {
    public UniqueActor BigBear { get; private set; }
    public UniqueActor Duckman { get; private set; }
    public UniqueActor FamuFataru { get; private set; }
    public UniqueActor FatherTime { get; private set; }
    public UniqueActor HansVonHanz { get; private set; }
    public UniqueActor JasonMyers { get; private set; }
    public UniqueActor PoliceStationPrisonner { get; private set; }
    public UniqueActor Roguedjack { get; private set; }
    public UniqueActor Santaman { get; private set; }
    public UniqueActor TheSewersThing { get; private set; }

    // \todo NEXT SAVEFILE BREAK: Father Time, with a legendary scythe.
    // This must be a value copy anyway (i.e. we gain almost nothing from having the authoritative storage be an array rather than auto properties)
    public UniqueActor[] ToArray()
    {
      return new UniqueActor[] {
        BigBear,
        Duckman,
        FamuFataru,
        FatherTime,
        HansVonHanz,
        PoliceStationPrisonner,
        Roguedjack,
        Santaman,
        TheSewersThing
      };
    }

    // Bound uniques.  These uniques are generated at the same time as their map.
    public void init_Prisoner(Actor newCivilian)
    {
#if DEBUG
      if (null != PoliceStationPrisonner) throw new InvalidOperationException("only call UniqueActors::init_Prisoner once");
#endif
      newCivilian.Name = "The Prisoner Who Should Not Be";
      for (int index = 0; index < newCivilian.Inventory.MaxCapacity; ++index)
        newCivilian.Inventory.AddAll(BaseMapGenerator.MakeItemArmyRation());
      PoliceStationPrisonner = new UniqueActor(newCivilian,true);
    }

    public void init_JasonMyers()
    {
#if DEBUG
      if (null != JasonMyers) throw new InvalidOperationException("only call UniqueActors::init_JasonMyers once");
#endif
      Actor named = GameActors.JasonMyers.CreateNamed(GameFactions.ThePsychopaths, "Jason Myers", 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_JASON_MYERS);
      named.StartingSkill(Skills.IDs.TOUGH,3);
      named.StartingSkill(Skills.IDs.STRONG,3);
      named.StartingSkill(Skills.IDs.AGILE,3);
      named.StartingSkill(Skills.IDs.HIGH_STAMINA,3);
      named.Inventory.AddAll(new ItemMeleeWeapon(GameItems.UNIQUE_JASON_MYERS_AXE));
      JasonMyers = new UniqueActor(named, true, false, GameMusics.INSANE);
    }

    // Free/unbound uniques.  These are not assigned to a specific map at the time the map is generated.

    // VAPORWARE: Father Time's day job was running a martial arts dojo.  Make him a shop owner for one; would change his status to a bound unique.
    // VAPORWARE: Father Time is elderly.  However, he's not deconditioned so shouldn't be using a standard elderly civilian model
    private void init_FatherTime(BaseTownGenerator tgen)    // unused parameter...haven't decided on full item complement yet.
    {
#if DEBUG
      if (null != FatherTime) throw new InvalidOperationException("only call UniqueActors::init_FatherTime once");
#endif
      Actor named = GameActors.MaleCivilian.CreateNamed(GameFactions.TheCivilians, "Father Time", 0);
      named.IsUnique = true;
      tgen.DressCivilian(named);
//    named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_FAMU_FATARU);    // XXX \todo GameImages.ACTOR_FATHER_TIME
      named.StartingSkill(Skills.IDs.HAULER,3);
      named.StartingSkill(Skills.IDs.MARTIAL_ARTS, 5);  // to get the most out of his scythe
      named.StartingSkill(Skills.IDs.AGILE, 5);
      named.StartingSkill(Skills.IDs.HIGH_STAMINA,5);
      named.Inventory.AddAll(new ItemMeleeWeapon(GameItems.UNIQUE_FATHER_TIME_SCYTHE));
      named.Inventory.AddAll(BaseMapGenerator.MakeItemArmyRation());    // Doesn't want to be a target, so only one ration
      FatherTime = new UniqueActor(named,false,true,null, "You hear new year's music.");  // XXX \todo GameMusics.FATHER_TIME_THEME_SONG (Auld Lang Syne?)
    }

    private void init_SewersThing(BaseTownGenerator tgen)
    {
#if DEBUG
      if (null != TheSewersThing) throw new InvalidOperationException("only call UniqueActors::init_SewersThing once");
#endif
      Map map = tgen.RandomDistrictInCity().SewersMap;
      Actor named = GameActors.SewersThing.CreateNamed(GameFactions.TheUndeads, "The Sewers Thing", 0);
      DiceRoller roller = new DiceRoller(map.Seed);
      if (!MapGenerator.ActorPlace(roller, map, named)) throw new InvalidOperationException("could not spawn unique The Sewers Thing");
      Zone zoneByPartialName = map.GetZoneByPartialName("Sewers Maintenance");
      if (zoneByPartialName != null)
        MapGenerator.MapObjectPlaceInGoodPosition(map, zoneByPartialName.Bounds, pt => {
           return map.IsWalkable(pt.X, pt.Y) && !map.HasActorAt(pt) && !map.HasItemsAt(pt) && !map.HasExitAt(pt);
        }, roller, pt => BaseMapGenerator.MakeObjBoard(GameImages.OBJ_BOARD, new string[] {
          "TO SEWER WORKERS :",
          "- It lives here.",
          "- Do not disturb.",
          "- Approach with caution.",
          "- Watch your back.",
          "- In case of emergency, take refuge here.",
          "- Do not let other people interact with it!"
        }));
      TheSewersThing = new UniqueActor(named,true);
    }

    private void init_BigBear(BaseTownGenerator tgen)
    {
#if DEBUG
      if (null != BigBear) throw new InvalidOperationException("only call UniqueActors::init_BigBear once");
#endif
      Actor named = GameActors.MaleCivilian.CreateNamed(GameFactions.TheCivilians, "Big Bear", 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_BIG_BEAR);
      named.StartingSkill(Skills.IDs.HAULER,3);
      named.StartingSkill(Skills.IDs.HARDY,5);
      named.StartingSkill(Skills.IDs.STRONG,5);
      named.StartingSkill(Skills.IDs.TOUGH,5);
      named.Inventory.AddAll(new ItemMeleeWeapon(GameItems.UNIQUE_BIGBEAR_BAT));
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      BigBear = new UniqueActor(named,false,true, GameMusics.BIGBEAR_THEME_SONG, "You hear an angry man shouting 'FOOLS!'");
    }

    private void init_FamuFataru(BaseTownGenerator tgen)
    {
#if DEBUG
      if (null != FamuFataru) throw new InvalidOperationException("only call UniqueActors::init_FamuFataru once");
#endif
      Actor named = GameActors.FemaleCivilian.CreateNamed(GameFactions.TheCivilians, "Famu Fataru", 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_FAMU_FATARU);
      named.StartingSkill(Skills.IDs.HAULER,3);
      named.StartingSkill(Skills.IDs.HARDY,5);
      named.StartingSkill(Skills.IDs.AGILE,5);
      named.StartingSkill(Skills.IDs.HIGH_STAMINA,5);
      named.StartingSkill(Skills.IDs.MARTIAL_ARTS,5);   // otherwise katana isn't awesome
      named.Inventory.AddAll(new ItemMeleeWeapon(GameItems.UNIQUE_FAMU_FATARU_KATANA));
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      FamuFataru = new UniqueActor(named,false,true, GameMusics.FAMU_FATARU_THEME_SONG, "You hear a woman laughing.");
    }

    private void init_Santaman(BaseTownGenerator tgen)
    {
#if DEBUG
      if (null != Santaman) throw new InvalidOperationException("only call UniqueActors::init_Santaman once");
#endif
      Actor named = GameActors.MaleCivilian.CreateNamed(GameFactions.TheCivilians, "Santaman", 0);
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

    private void init_Roguedjack(BaseTownGenerator tgen)
    {
#if DEBUG
      if (null != Roguedjack) throw new InvalidOperationException("only call UniqueActors::init_Roguedjack once");
#endif
      Actor named = GameActors.MaleCivilian.CreateNamed(GameFactions.TheCivilians, "Roguedjack", 0);
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


    private void init_Duckman(BaseTownGenerator tgen)
    {
#if DEBUG
      if (null != Duckman) throw new InvalidOperationException("only call UniqueActors::init_Duckman once");
#endif
      Actor named = GameActors.MaleCivilian.CreateNamed(GameFactions.TheCivilians, "Duckman", 0);
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

    private void init_HansVonHanz(BaseTownGenerator tgen)
    {
#if DEBUG
      if (null != HansVonHanz) throw new InvalidOperationException("only call UniqueActors::init_HanzVonHanz once");
#endif
      Actor named = GameActors.MaleCivilian.CreateNamed(GameFactions.TheCivilians, "Hans von Hanz", 0);
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

    public void init_UnboundUniques(BaseTownGenerator tgen)
    {
      init_SewersThing(tgen);
      init_BigBear(tgen);
      init_FamuFataru(tgen);
      init_Santaman(tgen);
      init_Roguedjack(tgen);
      init_Duckman(tgen);
      init_HansVonHanz(tgen);
      init_FatherTime(tgen);
    }
  }
}
