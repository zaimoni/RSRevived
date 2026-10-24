// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.UniqueActors
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.Text.Json;
using djack.RogueSurvivor.Gameplay;
using Zaimoni.JSON;

using Actor = djack.RogueSurvivor.Data.Actor;
using LivingActor = djack.RogueSurvivor.Data.LivingActor;
using BaseMapGenerator = djack.RogueSurvivor.Gameplay.Generators.BaseMapGenerator;
using BaseTownGenerator = djack.RogueSurvivor.Gameplay.Generators.BaseTownGenerator;
using DollPart = djack.RogueSurvivor.Data.DollPart;
using GameFactions = djack.RogueSurvivor.Data.GameFactions;
using ItemMeleeWeapon = djack.RogueSurvivor.Engine.Items.ItemMeleeWeapon;
using ItemRangedWeapon = djack.RogueSurvivor.Engine.Items.ItemRangedWeapon;
using Map = djack.RogueSurvivor.Data.Map;
using Zone = djack.RogueSurvivor.Data.Zone;

// Note that game-specific content was already here in RS alpha 9 (the identities of the unique actors)
namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  public class UniqueActors
  {
    public LivingActor BigBear { get; private set; }
    public LivingActor Duckman { get; private set; }
    public LivingActor FamuFataru { get; private set; }
    public LivingActor FatherTime { get; private set; }
    public LivingActor HansVonHanz { get; private set; }
    public LivingActor JasonMyers { get; private set; }
    public LivingActor PoliceStationPrisoner { get; private set; }
    public LivingActor Roguedjack { get; private set; }
    public LivingActor Santaman { get; private set; }
    public Actor TheSewersThing { get; private set; }

    /// <summary>The free uniques: not placed when the town is generated, they arrive with a refugee wave.
    /// Arrival flavor text, from the retired FireEvent_UniqueActorArrive, for when arrivals are re-wired:
    /// Big Bear "You hear an angry man shouting 'FOOLS!'"; Duckman "You hear loud demented QUACKS.";
    /// Famu Fataru "You hear a woman laughing."; Father Time "You hear new year's music.";
    /// Hans von Hanz "You hear a man barking orders in German."; Roguedjack "You hear a man shouting in French.";
    /// Santaman "You hear christmas music and drunken vomiting."</summary>
    public IEnumerable<LivingActor> Refugees {
      get {
        yield return BigBear;
        yield return Duckman;
        yield return FamuFataru;
        yield return FatherTime;
        yield return HansVonHanz;
        yield return Roguedjack;
        yield return Santaman;
      }
    }

    /// <summary>The uniques whose presence has theme music, in the historical priority order.</summary>
    public IEnumerable<KeyValuePair<Actor, string>> ThemeMusic {
      get {
        yield return new(BigBear, GameMusics.BIGBEAR_THEME_SONG);
        yield return new(Duckman, GameMusics.DUCKMAN_THEME_SONG);
        yield return new(FamuFataru, GameMusics.FAMU_FATARU_THEME_SONG);
        // \todo Father Time: GameMusics.FATHER_TIME_THEME_SONG (Auld Lang Syne?)
        yield return new(HansVonHanz, GameMusics.HANS_VON_HANZ_THEME_SONG);
        yield return new(Roguedjack, GameMusics.ROGUEDJACK_THEME_SONG);
        yield return new(Santaman, GameMusics.SANTAMAN_THEME_SONG);
        yield return new(JasonMyers, GameMusics.INSANE);  // alpha10
      }
    }

    // Bound uniques.  These uniques are generated at the same time as their map.
    public void init_Prisoner(LivingActor newCivilian)
    {
#if DEBUG
      if (PoliceStationPrisoner is not null) throw new InvalidOperationException("only call UniqueActors::init_Prisoner once");
#endif
      newCivilian.Name = "The Prisoner Who Should Not Be";
      for (int index = 0; index < newCivilian.Inventory.MaxCapacity; ++index)
        newCivilian.Take(GameItems.ARMY_RATION.instantiate());
      PoliceStationPrisoner = newCivilian;
    }

    public void init_JasonMyers()
    {
#if DEBUG
      if (JasonMyers is not null) throw new InvalidOperationException("only call UniqueActors::init_JasonMyers once");
#endif
      var named = GameActors.JasonMyers.CreateLivingNamed(GameFactions.ThePsychopaths, "Jason Myers", 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_JASON_MYERS);
      named.StartingSkill(Skills.IDs.TOUGH,3);
      named.StartingSkill(Skills.IDs.STRONG,3);
      named.StartingSkill(Skills.IDs.AGILE,3);
      named.StartingSkill(Skills.IDs.HIGH_STAMINA,3);
      named.Take(new ItemMeleeWeapon(GameItems.UNIQUE_JASON_MYERS_AXE));
      JasonMyers = named;
    }

    // Free/unbound uniques.  These are not assigned to a specific map at the time the map is generated.

    // VAPORWARE: Father Time's day job was running a martial arts dojo.  Make him a shop owner for one; would change his status to a bound unique.
    // VAPORWARE: Father Time is elderly.  However, he's not deconditioned so shouldn't be using a standard elderly civilian model
    private void init_FatherTime(BaseTownGenerator tgen)    // unused parameter...haven't decided on full item complement yet.
    {
#if DEBUG
      if (FatherTime is not null) throw new InvalidOperationException("only call UniqueActors::init_FatherTime once");
#endif
      var named = GameActors.MaleCivilian.CreateLivingNamed(GameFactions.TheCivilians, "Father Time", 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_FATHER_TIME);
      named.StartingSkill(Skills.IDs.HAULER,3);
      named.StartingSkill(Skills.IDs.MARTIAL_ARTS, 5);  // to get the most out of his scythe
      named.StartingSkill(Skills.IDs.AGILE, 5);
      named.StartingSkill(Skills.IDs.HIGH_STAMINA,5);
      named.Take(new ItemMeleeWeapon(GameItems.UNIQUE_FATHER_TIME_SCYTHE));
      named.Take(GameItems.ARMY_RATION.instantiate());    // Doesn't want to be a target, so only one ration
      FatherTime = named;  // XXX \todo GameMusics.FATHER_TIME_THEME_SONG (Auld Lang Syne?)
    }

    private void init_SewersThing(BaseTownGenerator tgen)
    {
#if DEBUG
      if (TheSewersThing is not null) throw new InvalidOperationException("only call UniqueActors::init_SewersThing once");
#endif
      Map map = tgen.RandomDistrictInCity().SewersMap;
      Actor named = GameActors.SewersThing.CreateNamed(GameFactions.TheUndeads, "The Sewers Thing", 0);
      DiceRoller roller = new DiceRoller(map.Seed);
      if (!MapGenerator.ActorPlace(roller, map, named)) throw new InvalidOperationException("could not spawn unique The Sewers Thing");
      Zone? zoneByPartialName = map.GetZoneByPartialName("Sewers Maintenance");
      if (zoneByPartialName is not null)
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
      TheSewersThing = named;
    }

    private static readonly string[] BIG_BEAR_EMOTES = new string[Gameplay.AI.BaseAI.MAX_EMOTES] {
      "You fool",
      "I'm fooled!",
      "Be a man"
    };

    private void init_BigBear(BaseTownGenerator tgen)
    {
#if DEBUG
      if (BigBear is not null) throw new InvalidOperationException("only call UniqueActors::init_BigBear once");
#endif
      var named = GameActors.MaleCivilian.CreateLivingNamed(GameFactions.TheCivilians, "Big Bear", 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_BIG_BEAR);
      named.StartingSkill(Skills.IDs.HAULER,3);
      named.StartingSkill(Skills.IDs.HARDY,5);
      named.StartingSkill(Skills.IDs.STRONG,5);
      named.StartingSkill(Skills.IDs.TOUGH,5);
      named.Take(new ItemMeleeWeapon(GameItems.UNIQUE_BIGBEAR_BAT));
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      BigBear = named;
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
      if (FamuFataru is not null) throw new InvalidOperationException("only call UniqueActors::init_FamuFataru once");
#endif
      var named = GameActors.FemaleCivilian.CreateLivingNamed(GameFactions.TheCivilians, "Famu Fataru", 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_FAMU_FATARU);
      named.StartingSkill(Skills.IDs.HAULER,3);
      named.StartingSkill(Skills.IDs.HARDY,5);
      named.StartingSkill(Skills.IDs.AGILE,5);
      named.StartingSkill(Skills.IDs.HIGH_STAMINA,5);
      named.StartingSkill(Skills.IDs.MARTIAL_ARTS,5);   // otherwise katana isn't awesome
      named.Take(new ItemMeleeWeapon(GameItems.UNIQUE_FAMU_FATARU_KATANA));
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      FamuFataru = named;
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
      if (Santaman is not null) throw new InvalidOperationException("only call UniqueActors::init_Santaman once");
#endif
      var named = GameActors.MaleCivilian.CreateLivingNamed(GameFactions.TheCivilians, "Santaman", 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_SANTAMAN);
      named.StartingSkill(Skills.IDs.HAULER,3);
      named.StartingSkill(Skills.IDs.HARDY,5);
      named.StartingSkill(Skills.IDs.AGILE,5);
      named.StartingSkill(Skills.IDs.FIREARMS,5);

     Data.Model.Item[] default_inv = { GameItems.UNIQUE_SANTAMAN_SHOTGUN, GameItems.AMMO_SHOTGUN, GameItems.AMMO_SHOTGUN, GameItems.AMMO_SHOTGUN };
      foreach(var x in default_inv) named.Take(x.create());
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      Santaman = named;
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
      if (Roguedjack is not null) throw new InvalidOperationException("only call UniqueActors::init_Roguedjack once");
#endif
      var named = GameActors.MaleCivilian.CreateLivingNamed(GameFactions.TheCivilians, "Roguedjack", 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_ROGUEDJACK);
      named.StartingSkill(Skills.IDs.HAULER,3);
      named.StartingSkill(Skills.IDs.HARDY,5);
      named.StartingSkill(Skills.IDs.LEADERSHIP,5);
      named.StartingSkill(Skills.IDs.CHARISMATIC,5);
      named.Take(new ItemMeleeWeapon(GameItems.UNIQUE_ROGUEDJACK_KEYBOARD));
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      Roguedjack = named;
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
      if (Duckman is not null) throw new InvalidOperationException("only call UniqueActors::init_Duckman once");
#endif
      var named = GameActors.MaleCivilian.CreateLivingNamed(GameFactions.TheCivilians, "Duckman", 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_DUCKMAN);
      named.StartingSkill(Skills.IDs.CHARISMATIC,5);
      named.StartingSkill(Skills.IDs.LEADERSHIP);
      named.StartingSkill(Skills.IDs.STRONG,5);
      named.StartingSkill(Skills.IDs.HIGH_STAMINA,5);
      named.StartingSkill(Skills.IDs.MARTIAL_ARTS,5);
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      Duckman = named;
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
      if (HansVonHanz is not null) throw new InvalidOperationException("only call UniqueActors::init_HanzVonHanz once");
#endif
      var named = GameActors.MaleCivilian.CreateLivingNamed(GameFactions.TheCivilians, "Hans von Hanz", 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_HANS_VON_HANZ);
      named.StartingSkill(Skills.IDs.HAULER,3);
      named.StartingSkill(Skills.IDs.FIREARMS,5);
      named.StartingSkill(Skills.IDs.LEADERSHIP,5);
      named.StartingSkill(Skills.IDs.NECROLOGY,5);
      named.Take(new ItemRangedWeapon(GameItems.UNIQUE_HANS_VON_HANZ_PISTOL));
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      named.Take(tgen.MakeItemCannedFood());
      HansVonHanz = named;
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

#region JSON save/load
    // We must be saved after the World.  A unique that is on a map, or has a corpse, was saved there and is a reference here.
    // Any other (not spawned yet; or dead, with its corpse gone) is saved here in full, and so loads with no map.
    static private void save(Utf8JsonWriter writer, string name, Actor src, JsonSerializerOptions options)
    {
      writer.WritePropertyName(name);
      JsonSerializer.Serialize(writer, src, options);
    }

    /// <remarks>Before 2026-10-24 the actor was wrapped in an object with its event flags.  Those were constants per identity, or dead.</remarks>
    static private T load<T>(JsonElement src, string name, JsonSerializerOptions options) where T : Actor
    {
      var relay = src.Require(name);
      if (relay.TryGetProperty("Actor", out var legacy)) relay = legacy;
      return relay.Deserialize<T>(options) ?? throw new JsonException("null unique actor: " + name);
    }

    public void toJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      writer.WriteStartObject();
      save(writer, nameof(BigBear), BigBear, options);
      save(writer, nameof(Duckman), Duckman, options);
      save(writer, nameof(FamuFataru), FamuFataru, options);
      save(writer, nameof(FatherTime), FatherTime, options);
      save(writer, nameof(HansVonHanz), HansVonHanz, options);
      save(writer, nameof(JasonMyers), JasonMyers, options);
      save(writer, nameof(PoliceStationPrisoner), PoliceStationPrisoner, options);
      save(writer, nameof(Roguedjack), Roguedjack, options);
      save(writer, nameof(Santaman), Santaman, options);
      save(writer, nameof(TheSewersThing), TheSewersThing, options);
      writer.WriteEndObject();
    }

    public UniqueActors() { }   // was implicit, until the constructor below was added

    private UniqueActors(JsonElement src, JsonSerializerOptions options)
    {
      BigBear = load<LivingActor>(src, nameof(BigBear), options);
      Duckman = load<LivingActor>(src, nameof(Duckman), options);
      FamuFataru = load<LivingActor>(src, nameof(FamuFataru), options);
      FatherTime = load<LivingActor>(src, nameof(FatherTime), options);
      HansVonHanz = load<LivingActor>(src, nameof(HansVonHanz), options);
      JasonMyers = load<LivingActor>(src, nameof(JasonMyers), options);
      PoliceStationPrisoner = load<LivingActor>(src, nameof(PoliceStationPrisoner), options);
      Roguedjack = load<LivingActor>(src, nameof(Roguedjack), options);
      Santaman = load<LivingActor>(src, nameof(Santaman), options);
      TheSewersThing = load<Actor>(src, nameof(TheSewersThing), options);
    }

    public static UniqueActors fromJson(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
      var src = JsonElement.ParseValue(ref reader);
      if (JsonValueKind.Object != src.ValueKind) throw new JsonException("expected object for UniqueActors");
      return new(src, options);
    }
#endregion
  }
}

namespace Zaimoni.JsonConvert
{
    public class UniqueActors : System.Text.Json.Serialization.JsonConverter<djack.RogueSurvivor.Engine.UniqueActors>
    {
        public override djack.RogueSurvivor.Engine.UniqueActors Read(ref Utf8JsonReader reader, Type src, JsonSerializerOptions options)
        {
            return djack.RogueSurvivor.Engine.UniqueActors.fromJson(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, djack.RogueSurvivor.Engine.UniqueActors src, JsonSerializerOptions options)
        {
            src.toJson(writer, options);
        }
    }
}
