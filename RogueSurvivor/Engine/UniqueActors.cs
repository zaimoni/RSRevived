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
using GameFactions = djack.RogueSurvivor.Data.GameFactions;
using ItemModel = djack.RogueSurvivor.Data.ItemModel;
using ItemMeleeWeapon = djack.RogueSurvivor.Engine.Items.ItemMeleeWeapon;
using ItemRangedWeapon = djack.RogueSurvivor.Engine.Items.ItemRangedWeapon;
using Map = djack.RogueSurvivor.Data.Map;
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
    public UniqueActor PoliceStationPrisoner { get; private set; }
    public UniqueActor Roguedjack { get; private set; }
    public UniqueActor Santaman { get; private set; }
    public UniqueActor TheSewersThing { get; private set; }

    // This must be a value copy anyway (i.e. we gain almost nothing from having the authoritative storage be an array rather than auto properties)
    public UniqueActor[] ToArray()
    {
      return new UniqueActor[] {
        BigBear,
        Duckman,
        FamuFataru,
        FatherTime,
        HansVonHanz,
        PoliceStationPrisoner,
        Roguedjack,
        Santaman,
        TheSewersThing,
        JasonMyers  // alpha10
      };
    }

    public Zaimoni.Data.Stack<UniqueActor> DraftPool(Predicate<UniqueActor> test) {
      var ret = new Zaimoni.Data.Stack<UniqueActor>(new UniqueActor[10]);
      // \todo this warrants a linear data structure
      if (test(BigBear)) ret.push(BigBear);
      if (test(Duckman)) ret.push(Duckman);
      if (test(FamuFataru)) ret.push(FamuFataru);
      if (test(FatherTime)) ret.push(FatherTime);
      if (test(HansVonHanz)) ret.push(HansVonHanz);
      if (test(PoliceStationPrisoner)) ret.push(PoliceStationPrisoner);
      if (test(Roguedjack)) ret.push(Roguedjack);
      if (test(Santaman)) ret.push(Santaman);
      if (test(TheSewersThing)) ret.push(TheSewersThing);
      if (test(JasonMyers)) ret.push(JasonMyers);
      return ret;
    }

    // Bound uniques.  These uniques are generated at the same time as their map.
    public void init_Prisoner(Actor newCivilian)
    {
#if DEBUG
      if (null != PoliceStationPrisoner) throw new InvalidOperationException("only call UniqueActors::init_Prisoner once");
#endif
      newCivilian.Name = "The Prisoner Who Should Not Be";
      for (int index = 0; index < newCivilian.Inventory.MaxCapacity; ++index)
        newCivilian.Inventory.AddAll(GameItems.ARMY_RATION.instantiate());
      PoliceStationPrisoner = new UniqueActor(newCivilian,true);
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
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_FATHER_TIME);
      named.StartingSkill(Skills.IDs.HAULER,3);
      named.StartingSkill(Skills.IDs.MARTIAL_ARTS, 5);  // to get the most out of his scythe
      named.StartingSkill(Skills.IDs.AGILE, 5);
      named.StartingSkill(Skills.IDs.HIGH_STAMINA,5);
      named.Inventory.AddAll(new ItemMeleeWeapon(GameItems.UNIQUE_FATHER_TIME_SCYTHE));
      named.Inventory.AddAll(GameItems.ARMY_RATION.instantiate());    // Doesn't want to be a target, so only one ration
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
           return map.IsWalkable(pt) && !map.HasActorAt(in pt) && !map.HasItemsAt(pt) && !map.HasExitAt(in pt);
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

    private static readonly string[] BIG_BEAR_EMOTES = new string[Gameplay.AI.BaseAI.MAX_EMOTES] {
      "You fool",
      "I'm fooled!",
      "Be a man"
    };

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
      (named.Controller as Gameplay.AI.CivilianAI)?.InstallUniqueEmotes(BIG_BEAR_EMOTES);
    }

    private static readonly string[] FAMU_FATARU_EMOTES = new string[Gameplay.AI.BaseAI.MAX_EMOTES] {
      "Bakemono",
      "Nani!?",
      "Kawaii"
    };

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
      (named.Controller as Gameplay.AI.CivilianAI)?.InstallUniqueEmotes(FAMU_FATARU_EMOTES);
    }

    private static readonly string[] SANTAMAN_EMOTES = new string[Gameplay.AI.BaseAI.MAX_EMOTES] {
      "DEM BLOODY KIDS!",
      "LEAVE ME ALONE I AIN'T HAVE NO PRESENTS!",
      "MERRY FUCKIN' CHRISTMAS"
    };

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

     ItemModel[] default_inv = { GameItems.UNIQUE_SANTAMAN_SHOTGUN, GameItems.AMMO_SHOTGUN, GameItems.AMMO_SHOTGUN, GameItems.AMMO_SHOTGUN };
      foreach(var x in default_inv) named.Inventory.AddAll(x.create());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      named.Inventory.AddAll(tgen.MakeItemCannedFood());
      Santaman = new UniqueActor(named,false,true, GameMusics.SANTAMAN_THEME_SONG, "You hear christmas music and drunken vomiting.");
      (named.Controller as Gameplay.AI.CivilianAI)?.InstallUniqueEmotes(SANTAMAN_EMOTES);
    }

    private static readonly string[] ROGUEDJACK_EMOTES = new string[Gameplay.AI.BaseAI.MAX_EMOTES] {
      "Sorry but I am le busy,",
      "I should have redone ze AI rootines!",
      "Let me test le something on you"
    };

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
      (named.Controller as Gameplay.AI.CivilianAI)?.InstallUniqueEmotes(ROGUEDJACK_EMOTES);
    }

    private static readonly string[] DUCKMAN_EMOTES = new string[Gameplay.AI.BaseAI.MAX_EMOTES] {
      "I'LL QUACK YOU BACK",
      "THIS IS MY FINAL QUACK",
      "I'M GONNA QUACK YOU"
    };

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
      (named.Controller as Gameplay.AI.CivilianAI)?.InstallUniqueEmotes(DUCKMAN_EMOTES);
    }

    private static readonly string[] HANS_VON_HANZ_EMOTES = new string[Gameplay.AI.BaseAI.MAX_EMOTES] {
      "RAUS",
      "MEIN FUHRER!",
      "KOMM HIER BITE"
    };

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
      (named.Controller as Gameplay.AI.CivilianAI)?.InstallUniqueEmotes(HANS_VON_HANZ_EMOTES);
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
