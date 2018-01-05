// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.Generators.BaseMapGenerator
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Engine.MapObjects;
using System;
using System.Drawing;

namespace djack.RogueSurvivor.Gameplay.Generators
{
  internal abstract class BaseMapGenerator : MapGenerator
  {
    private static readonly string[] MALE_SKINS = new string[5]
    {
      GameImages.MALE_SKIN1,
      GameImages.MALE_SKIN2,
      GameImages.MALE_SKIN3,
      GameImages.MALE_SKIN4,
      GameImages.MALE_SKIN5
    };
    private static readonly string[] MALE_HEADS = new string[8]
    {
      GameImages.MALE_HAIR1,
      GameImages.MALE_HAIR2,
      GameImages.MALE_HAIR3,
      GameImages.MALE_HAIR4,
      GameImages.MALE_HAIR5,
      GameImages.MALE_HAIR6,
      GameImages.MALE_HAIR7,
      GameImages.MALE_HAIR8
    };
    private static readonly string[] MALE_TORSOS = new string[5]
    {
      GameImages.MALE_SHIRT1,
      GameImages.MALE_SHIRT2,
      GameImages.MALE_SHIRT3,
      GameImages.MALE_SHIRT4,
      GameImages.MALE_SHIRT5
    };
    private static readonly string[] MALE_LEGS = new string[5]
    {
      GameImages.MALE_PANTS1,
      GameImages.MALE_PANTS2,
      GameImages.MALE_PANTS3,
      GameImages.MALE_PANTS4,
      GameImages.MALE_PANTS5
    };
    private static readonly string[] MALE_SHOES = new string[3]
    {
      GameImages.MALE_SHOES1,
      GameImages.MALE_SHOES2,
      GameImages.MALE_SHOES3
    };
    private static readonly string[] MALE_EYES = new string[6]
    {
      GameImages.MALE_EYES1,
      GameImages.MALE_EYES2,
      GameImages.MALE_EYES3,
      GameImages.MALE_EYES4,
      GameImages.MALE_EYES5,
      GameImages.MALE_EYES6
    };
    private static readonly string[] FEMALE_SKINS = new string[5]
    {
      GameImages.FEMALE_SKIN1,
      GameImages.FEMALE_SKIN2,
      GameImages.FEMALE_SKIN3,
      GameImages.FEMALE_SKIN4,
      GameImages.FEMALE_SKIN5
    };
    private static readonly string[] FEMALE_HEADS = new string[7]
    {
      GameImages.FEMALE_HAIR1,
      GameImages.FEMALE_HAIR2,
      GameImages.FEMALE_HAIR3,
      GameImages.FEMALE_HAIR4,
      GameImages.FEMALE_HAIR5,
      GameImages.FEMALE_HAIR6,
      GameImages.FEMALE_HAIR7
    };
    private static readonly string[] FEMALE_TORSOS = new string[4]
    {
      GameImages.FEMALE_SHIRT1,
      GameImages.FEMALE_SHIRT2,
      GameImages.FEMALE_SHIRT3,
      GameImages.FEMALE_SHIRT4
    };
    private static readonly string[] FEMALE_LEGS = new string[5]
    {
      GameImages.FEMALE_PANTS1,
      GameImages.FEMALE_PANTS2,
      GameImages.FEMALE_PANTS3,
      GameImages.FEMALE_PANTS4,
      GameImages.FEMALE_PANTS5
    };
    private static readonly string[] FEMALE_SHOES = new string[3]
    {
      GameImages.FEMALE_SHOES1,
      GameImages.FEMALE_SHOES2,
      GameImages.FEMALE_SHOES3
    };
    private static readonly string[] FEMALE_EYES = new string[6]
    {
      GameImages.FEMALE_EYES1,
      GameImages.FEMALE_EYES2,
      GameImages.FEMALE_EYES3,
      GameImages.FEMALE_EYES4,
      GameImages.FEMALE_EYES5,
      GameImages.FEMALE_EYES6
    };
    private static readonly string[] BIKER_HEADS = new string[3]
    {
      GameImages.BIKER_HAIR1,
      GameImages.BIKER_HAIR2,
      GameImages.BIKER_HAIR3
    };
    private static readonly string[] BIKER_LEGS = new string[1]
    {
      GameImages.BIKER_PANTS
    };
    private static readonly string[] BIKER_SHOES = new string[1]
    {
      GameImages.BIKER_SHOES
    };
    private static readonly string[] CHARGUARD_HEADS = new string[1]
    {
      GameImages.CHARGUARD_HAIR
    };
    private static readonly string[] CHARGUARD_LEGS = new string[1]
    {
      GameImages.CHARGUARD_PANTS
    };
    private static readonly string[] DOG_SKINS = new string[3]
    {
      GameImages.DOG_SKIN1,
      GameImages.DOG_SKIN2,
      GameImages.DOG_SKIN3
    };
    private static readonly string[] MALE_FIRST_NAMES = new string[139]
    {
      "Alan",
      "Albert",
      "Alex",
      "Alexander",
      "Andrew",
      "Andy",
      "Anton",
      "Anthony",
      "Ashley",
      "Axel",
      "Ben",
      "Bill",
      "Bob",
      "Brad",
      "Brandon",
      "Brian",
      "Bruce",
      "Caine",
      "Carl",
      "Carlton",
      "Charlie",
      "Clark",
      "Cody",
      "Cris",
      "Cristobal",
      "Dan",
      "Danny",
      "Dave",
      "David",
      "Dirk",
      "Don",
      "Donovan",
      "Doug",
      "Dustin",
      "Ed",
      "Eddy",
      "Edward",
      "Elias",
      "Eli",
      "Elmer",
      "Elton",
      "Eric",
      "Eugene",
      "Francis",
      "Frank",
      "Fred",
      "Garry",
      "George",
      "Greg",
      "Guy",
      "Gordon",
      "Hank",
      "Harold",
      "Harvey",
      "Henry",
      "Hubert",
      "Indy",
      "Jack",
      "Jake",
      "James",
      "Jarvis",
      "Jason",
      "Jeff",
      "Jeffrey",
      "Jeremy",
      "Jesse",
      "Jesus",
      "Jim",
      "John",
      "Johnny",
      "Jonas",
      "Joseph",
      "Julian",
      "Karl",
      "Keith",
      "Ken",
      "Larry",
      "Lars",
      "Lee",
      "Lennie",
      "Lewis",
      "Mark",
      "Mathew",
      "Max",
      "Michael",
      "Mickey",
      "Mike",
      "Mitch",
      "Ned",
      "Neil",
      "Nick",
      "Norman",
      "Oliver",
      "Orlando",
      "Oscar",
      "Pablo",
      "Patrick",
      "Pete",
      "Peter",
      "Phil",
      "Philip",
      "Preston",
      "Quentin",
      "Randy",
      "Rick",
      "Rob",
      "Ron",
      "Ross",
      "Robert",
      "Roberto",
      "Rudy",
      "Ryan",
      "Sam",
      "Samuel",
      "Saul",
      "Scott",
      "Shane",
      "Shaun",
      "Stan",
      "Stanley",
      "Stephen",
      "Steve",
      "Stuart",
      "Ted",
      "Tim",
      "Toby",
      "Tom",
      "Tommy",
      "Tony",
      "Travis",
      "Trevor",
      "Ulrich",
      "Val",
      "Vince",
      "Vincent",
      "Vinnie",
      "Walter",
      "Wayne",
      "Xavier"
    };
    private static readonly string[] FEMALE_FIRST_NAMES = new string[113]
    {
      "Abigail",
      "Amanda",
      "Ali",
      "Alice",
      "Alicia",
      "Alison",
      "Amy",
      "Angela",
      "Ann",
      "Annie",
      "Audrey",
      "Belinda",
      "Beth",
      "Brenda",
      "Carla",
      "Carolin",
      "Carrie",
      "Cassie",
      "Cherie",
      "Cheryl",
      "Claire",
      "Connie",
      "Cris",
      "Crissie",
      "Christina",
      "Dana",
      "Debbie",
      "Deborah",
      "Debrah",
      "Diana",
      "Dona",
      "Elayne",
      "Eleonor",
      "Elie",
      "Elizabeth",
      "Ester",
      "Felicia",
      "Fiona",
      "Fran",
      "Frances",
      "Georges",
      "Gina",
      "Ginger",
      "Gloria",
      "Grace",
      "Helen",
      "Helena",
      "Hilary",
      "Holy",
      "Ingrid",
      "Isabela",
      "Jackie",
      "Jennifer",
      "Jess",
      "Jessie",
      "Jill",
      "Joana",
      "Kate",
      "Kathleen",
      "Kathy",
      "Katrin",
      "Kim",
      "Kira",
      "Leonor",
      "Leslie",
      "Linda",
      "Lindsay",
      "Lisa",
      "Liz",
      "Lorraine",
      "Lucia",
      "Lucy",
      "Maggie",
      "Margareth",
      "Maria",
      "Mary",
      "Mary-Ann",
      "Marylin",
      "Michelle",
      "Millie",
      "Molly",
      "Monica",
      "Nancy",
      "Ophelia",
      "Paquita",
      "Page",
      "Patricia",
      "Patty",
      "Paula",
      "Rachel",
      "Raquel",
      "Regina",
      "Roberta",
      "Ruth",
      "Sabrina",
      "Samantha",
      "Sandra",
      "Sarah",
      "Sofia",
      "Sue",
      "Susan",
      "Tabatha",
      "Tanya",
      "Teresa",
      "Tess",
      "Tifany",
      "Tori",
      "Veronica",
      "Victoria",
      "Vivian",
      "Wendy",
      "Winona",
      "Zora"
    };
    private static readonly string[] LAST_NAMES = new string[52]
    {
      "Anderson",
      "Austin",
      "Bent",
      "Black",
      "Bradley",
      "Brown",
      "Bush",
      "Carpenter",
      "Carter",
      "Collins",
      "Cordell",
      "Dobbs",
      "Engels",
      "Finch",
      "Ford",
      "Forrester",
      "Gates",
      "Hewlett",
      "Holtz",
      "Irvin",
      "Jones",
      "Kennedy",
      "Lambert",
      "Lesaint",
      "Lee",
      "Lewis",
      "McAllister",
      "Malory",
      "McGready",
      "Norton",
      "O'Brien",
      "Oswald",
      "Patterson",
      "Paul",
      "Pitt",
      "Quinn",
      "Ramirez",
      "Reeves",
      "Rockwell",
      "Rogers",
      "Robertson",
      "Sanchez",
      "Smith",
      "Stevens",
      "Steward",
      "Tarver",
      "Taylor",
      "Ulrich",
      "Vance",
      "Washington",
      "Walters",
      "White"
    };
    private static readonly string[] CARS = new string[4]
    {
      GameImages.OBJ_CAR1,
      GameImages.OBJ_CAR2,
      GameImages.OBJ_CAR3,
      GameImages.OBJ_CAR4
    };
    protected readonly RogueGame m_Game;
    protected DiceRoller m_DiceRoller;

    protected BaseMapGenerator(RogueGame game)
      : base(game.Rules)
    {
      m_Game = game;
      m_DiceRoller = new DiceRoller();
    }

    static public void DressCivilian(DiceRoller roller, Actor actor)
    {
      if (actor.Model.DollBody.IsMale)
        DressCivilian(roller, actor, MALE_EYES, MALE_SKINS, MALE_HEADS, MALE_TORSOS, MALE_LEGS, MALE_SHOES);
      else
        DressCivilian(roller, actor, FEMALE_EYES, FEMALE_SKINS, FEMALE_HEADS, FEMALE_TORSOS, FEMALE_LEGS, FEMALE_SHOES);
    }

    static protected void SkinNakedHuman(DiceRoller roller, Actor actor)
    {
      if (actor.Model.DollBody.IsMale)
        SkinNakedHuman(roller, actor, MALE_EYES, MALE_SKINS, MALE_HEADS);
      else
        SkinNakedHuman(roller, actor, FEMALE_EYES, FEMALE_SKINS, FEMALE_HEADS);
    }

    static protected void DressCivilian(DiceRoller roller, Actor actor, string[] eyes, string[] skins, string[] heads, string[] torsos, string[] legs, string[] shoes)
    {
      actor.Doll.RemoveAllDecorations();
      actor.Doll.AddDecoration(DollPart.EYES, roller.Choose(eyes));
      actor.Doll.AddDecoration(DollPart.SKIN, roller.Choose(skins));
      actor.Doll.AddDecoration(DollPart.HEAD, roller.Choose(heads));
      actor.Doll.AddDecoration(DollPart.TORSO, roller.Choose(torsos));
      actor.Doll.AddDecoration(DollPart.LEGS, roller.Choose(legs));
      actor.Doll.AddDecoration(DollPart.FEET, roller.Choose(shoes));
    }

    static protected void SkinNakedHuman(DiceRoller roller, Actor actor, string[] eyes, string[] skins, string[] heads)
    {
      actor.Doll.RemoveAllDecorations();
      actor.Doll.AddDecoration(DollPart.EYES, roller.Choose(eyes));
      actor.Doll.AddDecoration(DollPart.SKIN, roller.Choose(skins));
      actor.Doll.AddDecoration(DollPart.HEAD, roller.Choose(heads));
    }

    static protected void SkinDog(DiceRoller roller, Actor actor)
    {
      actor.Doll.RemoveAllDecorations();
      actor.Doll.AddDecoration(DollPart.SKIN, roller.Choose(DOG_SKINS));
    }

    static protected void DressArmy(DiceRoller roller, Actor actor)
    {
      actor.Doll.RemoveAllDecorations();
//    actor.Doll.AddDecoration(DollPart.EYES, roller.Choose(MALE_EYES));    // XXX helmet obscures eyes? XXX
      actor.Doll.AddDecoration(DollPart.SKIN, roller.Choose(MALE_SKINS));
      actor.Doll.AddDecoration(DollPart.HEAD, GameImages.ARMY_HELMET);
      actor.Doll.AddDecoration(DollPart.TORSO, GameImages.ARMY_SHIRT);
      actor.Doll.AddDecoration(DollPart.LEGS, GameImages.ARMY_PANTS);
      actor.Doll.AddDecoration(DollPart.FEET, GameImages.ARMY_SHOES);
    }

    static protected void DressPolice(DiceRoller roller, Actor actor)
    {
      actor.Doll.RemoveAllDecorations();
      actor.Doll.AddDecoration(DollPart.EYES, roller.Choose(MALE_EYES));
      actor.Doll.AddDecoration(DollPart.SKIN, roller.Choose(MALE_SKINS));
      actor.Doll.AddDecoration(DollPart.HEAD, roller.Choose(MALE_HEADS));
      actor.Doll.AddDecoration(DollPart.HEAD, GameImages.POLICE_HAT);
      actor.Doll.AddDecoration(DollPart.TORSO, GameImages.POLICE_UNIFORM);
      actor.Doll.AddDecoration(DollPart.LEGS, GameImages.POLICE_PANTS);
      actor.Doll.AddDecoration(DollPart.FEET, GameImages.POLICE_SHOES);
    }

    static protected void DressBiker(DiceRoller roller, Actor actor)
    {
      actor.Doll.RemoveAllDecorations();
      actor.Doll.AddDecoration(DollPart.EYES, roller.Choose(MALE_EYES));
      actor.Doll.AddDecoration(DollPart.SKIN, roller.Choose(MALE_SKINS));
      actor.Doll.AddDecoration(DollPart.HEAD, roller.Choose(BIKER_HEADS));
      actor.Doll.AddDecoration(DollPart.LEGS, roller.Choose(BIKER_LEGS));
      actor.Doll.AddDecoration(DollPart.FEET, roller.Choose(BIKER_SHOES));
    }

    static protected void DressGangsta(DiceRoller roller, Actor actor)
    {
      actor.Doll.RemoveAllDecorations();
      actor.Doll.AddDecoration(DollPart.EYES, roller.Choose(MALE_EYES));
      actor.Doll.AddDecoration(DollPart.SKIN, roller.Choose(MALE_SKINS));
      actor.Doll.AddDecoration(DollPart.TORSO, GameImages.GANGSTA_SHIRT);
      actor.Doll.AddDecoration(DollPart.HEAD, roller.Choose(MALE_HEADS));
      actor.Doll.AddDecoration(DollPart.HEAD, GameImages.GANGSTA_HAT);
      actor.Doll.AddDecoration(DollPart.LEGS, GameImages.GANGSTA_PANTS);
      actor.Doll.AddDecoration(DollPart.FEET, roller.Choose(MALE_SHOES));
    }

    static protected void DressCHARGuard(DiceRoller roller, Actor actor)
    {
      actor.Doll.RemoveAllDecorations();
      actor.Doll.AddDecoration(DollPart.EYES, roller.Choose(MALE_EYES));
      actor.Doll.AddDecoration(DollPart.SKIN, roller.Choose(MALE_SKINS));
      actor.Doll.AddDecoration(DollPart.HEAD, roller.Choose(CHARGUARD_HEADS));
      actor.Doll.AddDecoration(DollPart.LEGS, roller.Choose(CHARGUARD_LEGS));
    }

    static protected void DressBlackOps(DiceRoller roller, Actor actor)
    {
      actor.Doll.RemoveAllDecorations();
      actor.Doll.AddDecoration(DollPart.EYES, roller.Choose(MALE_EYES));
      actor.Doll.AddDecoration(DollPart.SKIN, roller.Choose(MALE_SKINS));
      actor.Doll.AddDecoration(DollPart.TORSO, GameImages.BLACKOP_SUIT);
    }

#if DEAD_FUNC
    static protected string RandomSkin(DiceRoller roller, bool isMale)
    {
      string[] strArray = isMale ? MALE_SKINS : BaseMapGenerator.FEMALE_SKINS;
      return strArray[roller.Roll(0, strArray.Length)];
    }
#endif

    static public void GiveNameToActor(DiceRoller roller, Actor actor, string prefix=null)
    {
      if (actor.Model.DollBody.IsMale)
        GiveNameToActor(roller, actor, MALE_FIRST_NAMES, LAST_NAMES, prefix);
      else
        GiveNameToActor(roller, actor, FEMALE_FIRST_NAMES, LAST_NAMES, prefix);
    }

    static protected void GiveNameToActor(DiceRoller roller, Actor actor, string[] firstNames, string[] lastNames, string prefix)
    {
      actor.IsProperName = true;
      actor.Name = roller.Choose(firstNames) + " " + roller.Choose(lastNames);
      if (!string.IsNullOrWhiteSpace(prefix)) actor.PrefixName(prefix);
    }

    static protected void GiveRandomSkillsToActor(DiceRoller roller, Actor actor, int count)
    {
      for (int index = 0; index < count; ++index)
        GiveRandomSkillToActor(roller, actor);
    }

    static protected void GiveRandomSkillToActor(DiceRoller roller, Actor actor)
    {
      Skills.IDs skillID = !actor.Model.Abilities.IsUndead ? Skills.RollLiving(roller) : Skills.RollUndead(roller);
      actor.StartingSkill(skillID);
    }

    static protected DoorWindow MakeObjWoodenDoor() { return new DoorWindow(DoorWindow.DW_type.WOODEN, DoorWindow.BASE_HITPOINTS); }
    static protected DoorWindow MakeObjHospitalDoor() { return new DoorWindow(DoorWindow.DW_type.HOSPITAL, DoorWindow.BASE_HITPOINTS); }
    static protected DoorWindow MakeObjCharDoor() { return new DoorWindow(DoorWindow.DW_type.CHAR, 4*DoorWindow.BASE_HITPOINTS); }
    static protected DoorWindow MakeObjGlassDoor() { return new DoorWindow(DoorWindow.DW_type.GLASS, DoorWindow.BASE_HITPOINTS/4); }
    static protected DoorWindow MakeObjIronDoor() { return new DoorWindow(DoorWindow.DW_type.IRON, 8*DoorWindow.BASE_HITPOINTS); }
    static protected DoorWindow MakeObjWindow() { return new DoorWindow(DoorWindow.DW_type.WINDOW, DoorWindow.BASE_HITPOINTS/4); }

    static protected MapObject MakeObjFence()
    {
      return new MapObject(GameImages.OBJ_FENCE, 10* DoorWindow.BASE_HITPOINTS);
    }

    static protected MapObject MakeObjIronFence()
    {
      return new MapObject(GameImages.OBJ_IRON_FENCE);
    }

    static protected MapObject MakeObjIronGate()
    {
      return new MapObject(GameImages.OBJ_GATE_CLOSED, 20* DoorWindow.BASE_HITPOINTS);
    }

    static public Fortification MakeObjSmallFortification()
    {
      return new Fortification(GameImages.OBJ_SMALL_WOODEN_FORTIFICATION, Fortification.SMALL_BASE_HITPOINTS);
    }

    static public Fortification MakeObjLargeFortification()
    {
      return new Fortification(GameImages.OBJ_LARGE_WOODEN_FORTIFICATION, Fortification.LARGE_BASE_HITPOINTS);
    }

    static protected MapObject MakeObjTree()
    {
      return new MapObject(GameImages.OBJ_TREE, 10* DoorWindow.BASE_HITPOINTS);
    }

    static protected MapObject MakeObjWreckedCar(DiceRoller roller)
    {
      return MakeObjWreckedCar(roller.Choose(CARS));
    }

    static protected MapObject MakeObjWreckedCar(string carImageID)
    {
      return new MapObject(carImageID);
    }

    static protected MapObject MakeObjShelf()
    {
      return new MapObject(GameImages.OBJ_SHOP_SHELF, DoorWindow.BASE_HITPOINTS);
    }

    static protected MapObject MakeObjBench()
    {
      return new MapObject(GameImages.OBJ_BENCH, 2* DoorWindow.BASE_HITPOINTS);
    }

    static protected MapObject MakeObjIronBench()
    {
      return new MapObject(GameImages.OBJ_IRON_BENCH);
    }

    static protected MapObject MakeObjBed(string bedImageID)
    {
      return new MapObject(bedImageID, 2* DoorWindow.BASE_HITPOINTS);
    }

    static protected MapObject MakeObjWardrobe(string wardrobeImageID)
    {
      return new MapObject(wardrobeImageID, 6* DoorWindow.BASE_HITPOINTS);
    }

    static protected MapObject MakeObjDrawer()
    {
      return new MapObject(GameImages.OBJ_DRAWER, DoorWindow.BASE_HITPOINTS);
    }

    static protected MapObject MakeObjTable(string tableImageID)
    {
      return new MapObject(tableImageID, DoorWindow.BASE_HITPOINTS);
    }

    static protected MapObject MakeObjChair(string chairImageID)
    {
      return new MapObject(chairImageID, DoorWindow.BASE_HITPOINTS/3);
    }

    static protected MapObject MakeObjNightTable(string nightTableImageID)
    {
      return new MapObject(nightTableImageID, DoorWindow.BASE_HITPOINTS/3);
    }

    static protected MapObject MakeObjFridge()
    {
      return new MapObject(GameImages.OBJ_FRIDGE, 6* DoorWindow.BASE_HITPOINTS);
    }

    static protected MapObject MakeObjJunk()
    {
      return new MapObject(GameImages.OBJ_JUNK, DoorWindow.BASE_HITPOINTS);
    }

    static protected MapObject MakeObjBarrels()
    {
      return new MapObject(GameImages.OBJ_BARRELS, 2* DoorWindow.BASE_HITPOINTS);
    }

    protected static PowerGenerator MakeObjPowerGenerator() { return new PowerGenerator(); }

    static public MapObject MakeObjBoard(string imageID, string[] text)
    {
      return new Board(imageID, text);
    }

    static protected void DecorateOutsideWalls(Map map, Rectangle rect, Func<int, int, string> decoFn)
    {
#if DEBUG
      if (null == decoFn) throw new ArgumentNullException(nameof(decoFn));
#endif
      for (int left = rect.Left; left < rect.Right; ++left) {
        for (int top = rect.Top; top < rect.Bottom; ++top) {
          Tile tileAt = map.GetTileAt(left, top);
          if (!tileAt.Model.IsWalkable && !tileAt.IsInside) {
            string imageID = decoFn(left, top);
            if (imageID != null)
              tileAt.AddDecoration(imageID);
          }
        }
      }
    }

    public ItemMedicine MakeItemBandages()
    {
      return new ItemMedicine(GameItems.BANDAGE) {
        Quantity = m_DiceRoller.Roll(1, GameItems.BANDAGE.StackingLimit)
      };
    }

    static public ItemMedicine MakeItemMedikit()
    {
      return new ItemMedicine(GameItems.MEDIKIT);
    }

    public ItemMedicine MakeItemPillsSTA()
    {
      return new ItemMedicine(GameItems.PILLS_STA) {
        Quantity = m_DiceRoller.Roll(1, GameItems.PILLS_STA.StackingLimit)
      };
    }

    public ItemMedicine MakeItemPillsSLP()
    {
      return new ItemMedicine(GameItems.PILLS_SLP) {
        Quantity = m_DiceRoller.Roll(1, GameItems.PILLS_SLP.StackingLimit)
      };
    }

    public ItemMedicine MakeItemPillsSAN()
    {
      return new ItemMedicine(GameItems.PILLS_SAN) {
        Quantity = m_DiceRoller.Roll(1, GameItems.PILLS_SAN.StackingLimit)
      };
    }

    public ItemMedicine MakeItemPillsAntiviral()
    {
      return new ItemMedicine(GameItems.PILLS_ANTIVIRAL) {
        Quantity = m_DiceRoller.Roll(1, GameItems.PILLS_ANTIVIRAL.StackingLimit)
      };
    }

    public ItemFood MakeItemGroceries()
    {
      int turnCounter = Session.Get.WorldTime.TurnCounter;
      int max = WorldTime.TURNS_PER_DAY * GameItems.GROCERIES.BestBeforeDays;
      int min = max / 2;
      return new ItemFood(GameItems.GROCERIES, turnCounter + m_DiceRoller.Roll(min, max));
    }

    public ItemFood MakeItemCannedFood()
    {
      return new ItemFood(GameItems.CANNED_FOOD) {
        Quantity = m_DiceRoller.Roll(1, GameItems.CANNED_FOOD.StackingLimit)
      };
    }

    public ItemMeleeWeapon MakeItemCrowbar()
    {
      return new ItemMeleeWeapon(GameItems.CROWBAR) {
        Quantity = m_DiceRoller.Roll(1, GameItems.CROWBAR.StackingLimit)
      };
    }

    static public ItemMeleeWeapon MakeItemBaseballBat()
    {
      return new ItemMeleeWeapon(GameItems.BASEBALLBAT);
    }

    static public ItemMeleeWeapon MakeItemCombatKnife()
    {
      return new ItemMeleeWeapon(GameItems.COMBAT_KNIFE);
    }

    static public ItemMeleeWeapon MakeItemTruncheon()
    {
      return new ItemMeleeWeapon(GameItems.TRUNCHEON);
    }

    static public ItemMeleeWeapon MakeItemGolfClub()
    {
      return new ItemMeleeWeapon(GameItems.GOLFCLUB);
    }

    static public ItemMeleeWeapon MakeItemIronGolfClub()
    {
      return new ItemMeleeWeapon(GameItems.IRON_GOLFCLUB);
    }

    static public ItemMeleeWeapon MakeItemHugeHammer()
    {
      return new ItemMeleeWeapon(GameItems.HUGE_HAMMER);
    }

    static public ItemMeleeWeapon MakeItemSmallHammer()
    {
      return new ItemMeleeWeapon(GameItems.SMALL_HAMMER);
    }

    static public ItemMeleeWeapon MakeItemJasonMyersAxe()
    {
      return new ItemMeleeWeapon(GameItems.UNIQUE_JASON_MYERS_AXE);
    }

    static public ItemMeleeWeapon MakeItemShovel()
    {
      return new ItemMeleeWeapon(GameItems.SHOVEL);
    }

    static public ItemMeleeWeapon MakeItemShortShovel()
    {
      return new ItemMeleeWeapon(GameItems.SHORT_SHOVEL);
    }

    static public ItemBarricadeMaterial MakeItemWoodenPlank()
    {
      return new ItemBarricadeMaterial(GameItems.WOODENPLANK);
    }

    // XXX These two arguably should be alternate constructors.
    static public ItemRangedWeapon MakeRangedWeapon(GameItems.IDs x)
    {
      if (Models.Items[(int)x] is ItemRangedWeaponModel rw_model) return new ItemRangedWeapon(rw_model);
      throw new InvalidOperationException(x.ToString()+" not a ranged weapon");
    }

    static public ItemAmmo MakeAmmo(GameItems.IDs x)
    {
      ItemModel tmp = Models.Items[(int)x];
      if (tmp is ItemRangedWeaponModel rw_model) tmp = Models.Items[(int)(rw_model.AmmoType)+(int)(GameItems.IDs.AMMO_LIGHT_PISTOL)];    // use the ammo of the ranged weapon instead
      if (tmp is ItemAmmoModel am_model) return new ItemAmmo(am_model);
      throw new InvalidOperationException(x.ToString()+" not an ammo or ranged weapon");
    }

    static public ItemRangedWeapon MakeItemHuntingCrossbow()
    {
      return new ItemRangedWeapon(GameItems.HUNTING_CROSSBOW);
    }

    static public ItemAmmo MakeItemBoltsAmmo()
    {
      return new ItemAmmo(GameItems.AMMO_BOLTS);
    }

    static public ItemRangedWeapon MakeItemHuntingRifle()
    {
      return new ItemRangedWeapon(GameItems.HUNTING_RIFLE);
    }

    static public ItemAmmo MakeItemLightRifleAmmo()
    {
      return new ItemAmmo(GameItems.AMMO_LIGHT_RIFLE);
    }

    static public ItemRangedWeapon MakeItemPistol()
    {
      return new ItemRangedWeapon(GameItems.PISTOL);
    }

    static public ItemRangedWeapon MakeItemKoltRevolver()
    {
      return new ItemRangedWeapon(GameItems.KOLT_REVOLVER);
    }

    public ItemRangedWeapon MakeItemRandomPistol()
    {
      if (!m_DiceRoller.RollChance(50))
        return MakeItemKoltRevolver();
      return MakeItemPistol();
    }

    static public ItemAmmo MakeItemLightPistolAmmo()
    {
      return new ItemAmmo(GameItems.AMMO_LIGHT_PISTOL);
    }

    static public ItemRangedWeapon MakeItemShotgun()
    {
      return new ItemRangedWeapon(GameItems.SHOTGUN);
    }

    static public ItemAmmo MakeItemShotgunAmmo()
    {
      return new ItemAmmo(GameItems.AMMO_SHOTGUN);
    }

    static public ItemBodyArmor MakeItemCHARLightBodyArmor()
    {
      return new ItemBodyArmor(GameItems.CHAR_LT_BODYARMOR);
    }

    static public ItemBodyArmor MakeItemBikerGangJacket(GameGangs.IDs gangId)
    {
      switch (gangId) {
        case GameGangs.IDs.BIKER_HELLS_SOULS:
          return new ItemBodyArmor(GameItems.HELLS_SOULS_JACKET);
        case GameGangs.IDs.BIKER_FREE_ANGELS:
          return new ItemBodyArmor(GameItems.FREE_ANGELS_JACKET);
        default:
          throw new ArgumentException("unhandled biker gang");
      }
    }

    static public ItemBodyArmor MakeItemPoliceJacket()
    {
      return new ItemBodyArmor(GameItems.POLICE_JACKET);
    }

    static public ItemBodyArmor MakeItemPoliceRiotArmor()
    {
      return new ItemBodyArmor(GameItems.POLICE_RIOT);
    }

    static public ItemBodyArmor MakeItemHunterVest()
    {
      return new ItemBodyArmor(GameItems.HUNTER_VEST);
    }

    static public ItemTracker MakeItemCellPhone()
    {
      return new ItemTracker(GameItems.CELL_PHONE);
    }

    public ItemSprayPaint MakeItemSprayPaint()
    {
      ItemSprayPaintModel itemSprayPaintModel;
      switch (m_DiceRoller.Roll(0, 4))
      {
        case 0:
          itemSprayPaintModel = GameItems.SPRAY_PAINT1;
          break;
        case 1:
          itemSprayPaintModel = GameItems.SPRAY_PAINT2;
          break;
        case 2:
          itemSprayPaintModel = GameItems.SPRAY_PAINT3;
          break;
#if DEBUG
        case 3:
#else
        default:
#endif
          itemSprayPaintModel = GameItems.SPRAY_PAINT4;
          break;
#if DEBUG
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
#endif
      }
      return new ItemSprayPaint(itemSprayPaintModel);
    }

    static public ItemSprayScent MakeItemStenchKiller()
    {
      return new ItemSprayScent(GameItems.STENCH_KILLER);
    }

    static public ItemRangedWeapon MakeItemArmyRifle()
    {
      return new ItemRangedWeapon(GameItems.ARMY_RIFLE);
    }

    static public ItemRangedWeapon MakeItemPrecisionRifle()
    {
      return new ItemRangedWeapon(GameItems.PRECISION_RIFLE);
    }

    static public ItemAmmo MakeItemHeavyRifleAmmo()
    {
      return new ItemAmmo(GameItems.AMMO_HEAVY_RIFLE);
    }

    static public ItemRangedWeapon MakeItemArmyPistol()
    {
      return new ItemRangedWeapon(GameItems.ARMY_PISTOL);
    }

    static public ItemAmmo MakeItemHeavyPistolAmmo()
    {
      return new ItemAmmo(GameItems.AMMO_HEAVY_PISTOL);
    }

    static public ItemBodyArmor MakeItemArmyBodyArmor() { return new ItemBodyArmor(GameItems.ARMY_BODYARMOR); }

    static public ItemFood MakeItemArmyRation()
    {
      return new ItemFood(GameItems.ARMY_RATION, Session.Get.WorldTime.TurnCounter + WorldTime.TURNS_PER_DAY * GameItems.ARMY_RATION.BestBeforeDays);
    }

    static public ItemLight MakeItemFlashlight()
    {
      return new ItemLight(GameItems.FLASHLIGHT);
    }

    static public ItemLight MakeItemBigFlashlight()
    {
      return new ItemLight(GameItems.BIG_FLASHLIGHT);
    }

    static public ItemTracker MakeItemZTracker()
    {
      return new ItemTracker(GameItems.ZTRACKER);
    }

    static public ItemTracker MakeItemBlackOpsGPS()
    {
      return new ItemTracker(GameItems.BLACKOPS_GPS);
    }

    static public ItemTracker MakeItemPoliceRadio()
    {
      return new ItemTracker(GameItems.POLICE_RADIO);
    }

    public ItemGrenade MakeItemGrenade()
    {
      return new ItemGrenade(GameItems.GRENADE, GameItems.GRENADE_PRIMED) {
        Quantity = m_DiceRoller.Roll(1, GameItems.GRENADE.StackingLimit)
      };
    }

    static public ItemTrap MakeItemBearTrap()
    {
      return new ItemTrap(GameItems.BEAR_TRAP);
    }

    public ItemTrap MakeItemSpikes()
    {
      return new ItemTrap(GameItems.SPIKES) {
        Quantity = m_DiceRoller.Roll(1, GameItems.BARBED_WIRE.StackingLimit)  // XXX V.0.10.0 align?  RS Alpha 9 has this as well.
      };
    }

    public ItemTrap MakeItemBarbedWire()
    {
      return new ItemTrap(GameItems.BARBED_WIRE) {
        Quantity = m_DiceRoller.Roll(1, GameItems.BARBED_WIRE.StackingLimit)
      };
    }

    static public ItemEntertainment MakeItemBook()
    {
      return new ItemEntertainment(GameItems.BOOK);
    }

    public ItemEntertainment MakeItemMagazines()
    {
      return new ItemEntertainment(GameItems.MAGAZINE) {
        Quantity = m_DiceRoller.Roll(1, GameItems.MAGAZINE.StackingLimit)
      };
    }

    protected static void BarricadeDoors(Map map, Rectangle rect, int barricadeLevel)
    {
      DoForEachTile(rect, pt => (map.GetMapObjectAt(pt) as DoorWindow)?.Barricade(barricadeLevel));
    }

    // this is not a legitimate Zone constructor (signature conflict), it's a preprocessing.
    static protected Zone MakeUniqueZone(string basename, Rectangle rect)
    {
      return new Zone(string.Format("{0}@{1}-{2}", (object) basename, (object) (rect.Left + rect.Width / 2), (object) (rect.Top + rect.Height / 2)), rect);
    }

    // start RNG-dependent map generation utilities
  }
}
