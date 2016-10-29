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
      "Elie",
      "Elmer",
      "Elton",
      "Eric",
      "Eugene",
      "Francis",
      "Frank",
      "Fred",
      "Garry",
      "Georges",
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
      "Jessie",
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
    private static readonly string[] FEMALE_FIRST_NAMES = new string[109]
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
      "Elizabeth",
      "Ester",
      "Felicia",
      "Fiona",
      "Fran",
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
    private static string[] CARS = new string[4]
    {
      GameImages.OBJ_CAR1,
      GameImages.OBJ_CAR2,
      GameImages.OBJ_CAR3,
      GameImages.OBJ_CAR4
    };
    protected readonly RogueGame m_Game;

    protected BaseMapGenerator(RogueGame game)
      : base(game.Rules)
    {
            m_Game = game;
    }

    public void DressCivilian(DiceRoller roller, Actor actor)
    {
      if (actor.Model.DollBody.IsMale)
                DressCivilian(roller, actor, BaseMapGenerator.MALE_EYES, BaseMapGenerator.MALE_SKINS, BaseMapGenerator.MALE_HEADS, BaseMapGenerator.MALE_TORSOS, BaseMapGenerator.MALE_LEGS, BaseMapGenerator.MALE_SHOES);
      else
                DressCivilian(roller, actor, BaseMapGenerator.FEMALE_EYES, BaseMapGenerator.FEMALE_SKINS, BaseMapGenerator.FEMALE_HEADS, BaseMapGenerator.FEMALE_TORSOS, BaseMapGenerator.FEMALE_LEGS, BaseMapGenerator.FEMALE_SHOES);
    }

    public void SkinNakedHuman(DiceRoller roller, Actor actor)
    {
      if (actor.Model.DollBody.IsMale)
                SkinNakedHuman(roller, actor, BaseMapGenerator.MALE_EYES, BaseMapGenerator.MALE_SKINS, BaseMapGenerator.MALE_HEADS);
      else
                SkinNakedHuman(roller, actor, BaseMapGenerator.FEMALE_EYES, BaseMapGenerator.FEMALE_SKINS, BaseMapGenerator.FEMALE_HEADS);
    }

    public void DressCivilian(DiceRoller roller, Actor actor, string[] eyes, string[] skins, string[] heads, string[] torsos, string[] legs, string[] shoes)
    {
      actor.Doll.RemoveAllDecorations();
      actor.Doll.AddDecoration(DollPart.EYES, eyes[roller.Roll(0, eyes.Length)]);
      actor.Doll.AddDecoration(DollPart.SKIN, skins[roller.Roll(0, skins.Length)]);
      actor.Doll.AddDecoration(DollPart.HEAD, heads[roller.Roll(0, heads.Length)]);
      actor.Doll.AddDecoration(DollPart.TORSO, torsos[roller.Roll(0, torsos.Length)]);
      actor.Doll.AddDecoration(DollPart.LEGS, legs[roller.Roll(0, legs.Length)]);
      actor.Doll.AddDecoration(DollPart.FEET, shoes[roller.Roll(0, shoes.Length)]);
    }

    public void SkinNakedHuman(DiceRoller roller, Actor actor, string[] eyes, string[] skins, string[] heads)
    {
      actor.Doll.RemoveAllDecorations();
      actor.Doll.AddDecoration(DollPart.EYES, eyes[roller.Roll(0, eyes.Length)]);
      actor.Doll.AddDecoration(DollPart.SKIN, skins[roller.Roll(0, skins.Length)]);
      actor.Doll.AddDecoration(DollPart.HEAD, heads[roller.Roll(0, heads.Length)]);
    }

    public void SkinDog(DiceRoller roller, Actor actor)
    {
      actor.Doll.RemoveAllDecorations();
      actor.Doll.AddDecoration(DollPart.SKIN, BaseMapGenerator.DOG_SKINS[roller.Roll(0, BaseMapGenerator.DOG_SKINS.Length)]);
    }

    public void DressArmy(DiceRoller roller, Actor actor)
    {
      actor.Doll.RemoveAllDecorations();
      actor.Doll.AddDecoration(DollPart.SKIN, BaseMapGenerator.MALE_SKINS[roller.Roll(0, BaseMapGenerator.MALE_SKINS.Length)]);
      actor.Doll.AddDecoration(DollPart.HEAD, "Actors\\Decoration\\army_helmet");
      actor.Doll.AddDecoration(DollPart.TORSO, "Actors\\Decoration\\army_shirt");
      actor.Doll.AddDecoration(DollPart.LEGS, "Actors\\Decoration\\army_pants");
      actor.Doll.AddDecoration(DollPart.FEET, "Actors\\Decoration\\army_shoes");
    }

    public void DressPolice(DiceRoller roller, Actor actor)
    {
      actor.Doll.RemoveAllDecorations();
      actor.Doll.AddDecoration(DollPart.EYES, BaseMapGenerator.MALE_EYES[roller.Roll(0, BaseMapGenerator.MALE_EYES.Length)]);
      actor.Doll.AddDecoration(DollPart.SKIN, BaseMapGenerator.MALE_SKINS[roller.Roll(0, BaseMapGenerator.MALE_SKINS.Length)]);
      actor.Doll.AddDecoration(DollPart.HEAD, BaseMapGenerator.MALE_HEADS[roller.Roll(0, BaseMapGenerator.MALE_HEADS.Length)]);
      actor.Doll.AddDecoration(DollPart.HEAD, "Actors\\Decoration\\police_hat");
      actor.Doll.AddDecoration(DollPart.TORSO, "Actors\\Decoration\\police_uniform");
      actor.Doll.AddDecoration(DollPart.LEGS, "Actors\\Decoration\\police_pants");
      actor.Doll.AddDecoration(DollPart.FEET, "Actors\\Decoration\\police_shoes");
    }

    public void DressBiker(DiceRoller roller, Actor actor)
    {
      actor.Doll.RemoveAllDecorations();
      actor.Doll.AddDecoration(DollPart.EYES, BaseMapGenerator.MALE_EYES[roller.Roll(0, BaseMapGenerator.MALE_EYES.Length)]);
      actor.Doll.AddDecoration(DollPart.SKIN, BaseMapGenerator.MALE_SKINS[roller.Roll(0, BaseMapGenerator.MALE_SKINS.Length)]);
      actor.Doll.AddDecoration(DollPart.HEAD, BaseMapGenerator.BIKER_HEADS[roller.Roll(0, BaseMapGenerator.BIKER_HEADS.Length)]);
      actor.Doll.AddDecoration(DollPart.LEGS, BaseMapGenerator.BIKER_LEGS[roller.Roll(0, BaseMapGenerator.BIKER_LEGS.Length)]);
      actor.Doll.AddDecoration(DollPart.FEET, BaseMapGenerator.BIKER_SHOES[roller.Roll(0, BaseMapGenerator.BIKER_SHOES.Length)]);
    }

    public void DressGangsta(DiceRoller roller, Actor actor)
    {
      actor.Doll.RemoveAllDecorations();
      actor.Doll.AddDecoration(DollPart.EYES, BaseMapGenerator.MALE_EYES[roller.Roll(0, BaseMapGenerator.MALE_EYES.Length)]);
      actor.Doll.AddDecoration(DollPart.SKIN, BaseMapGenerator.MALE_SKINS[roller.Roll(0, BaseMapGenerator.MALE_SKINS.Length)]);
      actor.Doll.AddDecoration(DollPart.TORSO, "Actors\\Decoration\\gangsta_shirt");
      actor.Doll.AddDecoration(DollPart.HEAD, BaseMapGenerator.MALE_HEADS[roller.Roll(0, BaseMapGenerator.MALE_HEADS.Length)]);
      actor.Doll.AddDecoration(DollPart.HEAD, "Actors\\Decoration\\gangsta_hat");
      actor.Doll.AddDecoration(DollPart.LEGS, "Actors\\Decoration\\gangsta_pants");
      actor.Doll.AddDecoration(DollPart.FEET, BaseMapGenerator.MALE_SHOES[roller.Roll(0, BaseMapGenerator.MALE_SHOES.Length)]);
    }

    public void DressCHARGuard(DiceRoller roller, Actor actor)
    {
      actor.Doll.RemoveAllDecorations();
      actor.Doll.AddDecoration(DollPart.EYES, BaseMapGenerator.MALE_EYES[roller.Roll(0, BaseMapGenerator.MALE_EYES.Length)]);
      actor.Doll.AddDecoration(DollPart.SKIN, BaseMapGenerator.MALE_SKINS[roller.Roll(0, BaseMapGenerator.MALE_SKINS.Length)]);
      actor.Doll.AddDecoration(DollPart.HEAD, BaseMapGenerator.CHARGUARD_HEADS[roller.Roll(0, BaseMapGenerator.CHARGUARD_HEADS.Length)]);
      actor.Doll.AddDecoration(DollPart.LEGS, BaseMapGenerator.CHARGUARD_LEGS[roller.Roll(0, BaseMapGenerator.CHARGUARD_LEGS.Length)]);
    }

    public void DressBlackOps(DiceRoller roller, Actor actor)
    {
      actor.Doll.RemoveAllDecorations();
      actor.Doll.AddDecoration(DollPart.EYES, BaseMapGenerator.MALE_EYES[roller.Roll(0, BaseMapGenerator.MALE_EYES.Length)]);
      actor.Doll.AddDecoration(DollPart.SKIN, BaseMapGenerator.MALE_SKINS[roller.Roll(0, BaseMapGenerator.MALE_SKINS.Length)]);
      actor.Doll.AddDecoration(DollPart.TORSO, "Actors\\Decoration\\blackop_suit");
    }

    public string RandomSkin(DiceRoller roller, bool isMale)
    {
      string[] strArray = isMale ? BaseMapGenerator.MALE_SKINS : BaseMapGenerator.FEMALE_SKINS;
      return strArray[roller.Roll(0, strArray.Length)];
    }

    public void GiveNameToActor(DiceRoller roller, Actor actor)
    {
      if (actor.Model.DollBody.IsMale)
                GiveNameToActor(roller, actor, BaseMapGenerator.MALE_FIRST_NAMES, BaseMapGenerator.LAST_NAMES);
      else
                GiveNameToActor(roller, actor, BaseMapGenerator.FEMALE_FIRST_NAMES, BaseMapGenerator.LAST_NAMES);
    }

    public void GiveNameToActor(DiceRoller roller, Actor actor, string[] firstNames, string[] lastNames)
    {
      actor.IsProperName = true;
      string str = firstNames[roller.Roll(0, firstNames.Length)] + " " + lastNames[roller.Roll(0, lastNames.Length)];
      actor.Name = str;
    }

    public void GiveRandomSkillsToActor(DiceRoller roller, Actor actor, int count)
    {
      for (int index = 0; index < count; ++index)
        GiveRandomSkillToActor(roller, actor);
    }

    public void GiveRandomSkillToActor(DiceRoller roller, Actor actor)
    {
      Skills.IDs skillID = !actor.Model.Abilities.IsUndead ? Skills.RollLiving(roller) : Skills.RollUndead(roller);
      actor.StartingSkill(skillID);
    }

    protected DoorWindow MakeObjWoodenDoor()
    {
      return new DoorWindow("wooden door", GameImages.OBJ_WOODEN_DOOR_CLOSED, GameImages.OBJ_WOODEN_DOOR_OPEN, GameImages.OBJ_WOODEN_DOOR_BROKEN, 40) {
        GivesWood = true
      };
    }

    protected DoorWindow MakeObjHospitalDoor()
    {
      return new DoorWindow("door", GameImages.OBJ_HOSPITAL_DOOR_CLOSED, GameImages.OBJ_HOSPITAL_DOOR_OPEN, GameImages.OBJ_HOSPITAL_DOOR_BROKEN, 40) {
        GivesWood = true
      };
    }

    protected DoorWindow MakeObjCharDoor()
    {
      return new DoorWindow("CHAR door", GameImages.OBJ_CHAR_DOOR_CLOSED, GameImages.OBJ_CHAR_DOOR_OPEN, GameImages.OBJ_CHAR_DOOR_BROKEN, 160);
    }

    protected DoorWindow MakeObjGlassDoor()
    {
      return new DoorWindow("glass door", GameImages.OBJ_GLASS_DOOR_CLOSED, GameImages.OBJ_GLASS_DOOR_OPEN, GameImages.OBJ_GLASS_DOOR_BROKEN, 10) {
        IsMaterialTransparent = true,
        BreaksWhenFiredThrough = true
      };
    }

    protected DoorWindow MakeObjIronDoor()
    {
      return new DoorWindow("iron door", GameImages.OBJ_IRON_DOOR_CLOSED, GameImages.OBJ_IRON_DOOR_OPEN, GameImages.OBJ_IRON_DOOR_BROKEN, 320) {
        IsAn = true
      };
    }

    protected DoorWindow MakeObjWindow()
    {
      return new DoorWindow("window", GameImages.OBJ_WINDOW_CLOSED, GameImages.OBJ_WINDOW_OPEN, GameImages.OBJ_WINDOW_BROKEN, 10) {
        IsMaterialTransparent = true,
        GivesWood = true,
        BreaksWhenFiredThrough = true
      };
    }

    protected MapObject MakeObjFence(string fenceImageID)
    {
      return new MapObject("fence", fenceImageID, MapObject.Break.BREAKABLE, MapObject.Fire.UNINFLAMMABLE, 400)
      {
        IsMaterialTransparent = true,
        JumpLevel = 1,
        GivesWood = true,
        StandOnFovBonus = true
      };
    }

    protected MapObject MakeObjIronFence()
    {
      return new MapObject("iron fence", GameImages.OBJ_IRON_FENCE)
      {
        IsMaterialTransparent = true,
        IsAn = true
      };
    }

    protected MapObject MakeObjIronGate()
    {
      return new MapObject("iron gate", GameImages.OBJ_GATE_CLOSED, MapObject.Break.BREAKABLE, MapObject.Fire.UNINFLAMMABLE, 800)
      {
        IsMaterialTransparent = true,
        IsAn = true
      };
    }

    public Fortification MakeObjSmallFortification(string imageID)
    {
      Fortification fortification = new Fortification("small fortification", imageID, 20);
      fortification.IsMaterialTransparent = true;
      fortification.GivesWood = true;
      fortification.IsMovable = true;
      fortification.Weight = 4;
      fortification.JumpLevel = 1;
      return fortification;
    }

    public Fortification MakeObjLargeFortification(string imageID)
    {
      Fortification fortification = new Fortification("large fortification", imageID, 40);
      fortification.GivesWood = true;
      return fortification;
    }

    protected MapObject MakeObjTree(string treeImageID)
    {
      return new MapObject("tree", treeImageID, MapObject.Break.BREAKABLE, MapObject.Fire.BURNABLE, 400)
      {
        GivesWood = true
      };
    }

    protected MapObject MakeObjWreckedCar(DiceRoller roller)
    {
      return MakeObjWreckedCar(BaseMapGenerator.CARS[roller.Roll(0, BaseMapGenerator.CARS.Length)]);
    }

    protected MapObject MakeObjWreckedCar(string carImageID)
    {
      return new MapObject("wrecked car", carImageID)
      {
        BreakState = MapObject.Break.BROKEN,
        IsMaterialTransparent = true,
        JumpLevel = 1,
        IsMovable = true,
        Weight = 100,
        StandOnFovBonus = true
      };
    }

    protected MapObject MakeObjShelf()
    {
      return new MapObject("shelf", GameImages.OBJ_SHOP_SHELF, MapObject.Break.BREAKABLE, MapObject.Fire.UNINFLAMMABLE, 40) {
        IsContainer = true,
        GivesWood = true,
        IsMovable = true,
        Weight = 6
      };
    }

    protected MapObject MakeObjBench()
    {
      return new MapObject("bench", GameImages.OBJ_BENCH, MapObject.Break.BREAKABLE, MapObject.Fire.UNINFLAMMABLE, 80) {
        IsMaterialTransparent = true,
        JumpLevel = 1,
        IsCouch = true,
        GivesWood = true
      };
    }

    protected MapObject MakeObjIronBench()
    {
      return new MapObject("iron bench", GameImages.OBJ_IRON_BENCH) {
        IsMaterialTransparent = true,
        JumpLevel = 1,
        IsCouch = true,
        IsAn = true
      };
    }

    protected MapObject MakeObjBed(string bedImageID)
    {
      return new MapObject("bed", bedImageID, MapObject.Break.BREAKABLE, MapObject.Fire.UNINFLAMMABLE, 80)
      {
        IsMaterialTransparent = true,
        IsWalkable = true,
        IsCouch = true,
        GivesWood = true,
        IsMovable = true,
        Weight = 6
      };
    }

    protected MapObject MakeObjWardrobe(string wardrobeImageID)
    {
      return new MapObject("wardrobe", wardrobeImageID, MapObject.Break.BREAKABLE, MapObject.Fire.UNINFLAMMABLE, 240)
      {
        IsMaterialTransparent = false,
        IsContainer = true,
        GivesWood = true,
        IsMovable = true,
        Weight = 10
      };
    }

    protected MapObject MakeObjDrawer()
    {
      return new MapObject("drawer", GameImages.OBJ_DRAWER, MapObject.Break.BREAKABLE, MapObject.Fire.UNINFLAMMABLE, 40) {
        IsMaterialTransparent = true,
        IsContainer = true,
        GivesWood = true,
        IsMovable = true,
        Weight = 6
      };
    }

    protected MapObject MakeObjTable(string tableImageID)
    {
      return new MapObject("table", tableImageID, MapObject.Break.BREAKABLE, MapObject.Fire.UNINFLAMMABLE, 40)
      {
        IsMaterialTransparent = true,
        JumpLevel = 1,
        GivesWood = true,
        IsMovable = true,
        Weight = 2
      };
    }

    protected MapObject MakeObjChair(string chairImageID)
    {
      return new MapObject("chair", chairImageID, MapObject.Break.BREAKABLE, MapObject.Fire.UNINFLAMMABLE, 13)
      {
        IsMaterialTransparent = true,
        JumpLevel = 1,
        GivesWood = true,
        IsMovable = true,
        Weight = 1
      };
    }

    protected MapObject MakeObjNightTable(string nightTableImageID)
    {
      return new MapObject("night table", nightTableImageID, MapObject.Break.BREAKABLE, MapObject.Fire.UNINFLAMMABLE, 13)
      {
        IsMaterialTransparent = true,
        JumpLevel = 1,
        GivesWood = true,
        IsMovable = true,
        Weight = 1
      };
    }

    protected MapObject MakeObjFridge()
    {
      return new MapObject("fridge", GameImages.OBJ_FRIDGE, MapObject.Break.BREAKABLE, MapObject.Fire.UNINFLAMMABLE, 240)
      {
        IsContainer = true,
        IsMovable = true,
        Weight = 10
      };
    }

    protected MapObject MakeObjJunk()
    {
      return new MapObject("junk", GameImages.OBJ_JUNK, MapObject.Break.BREAKABLE, MapObject.Fire.UNINFLAMMABLE, 40) {
        IsPlural = true,
        IsMaterialTransparent = true,
        IsMovable = true,
        GivesWood = true,
        Weight = 6
      };
    }

    protected MapObject MakeObjBarrels()
    {
      return new MapObject("barrels", GameImages.OBJ_BARRELS, MapObject.Break.BREAKABLE, MapObject.Fire.UNINFLAMMABLE, 80) {
        IsPlural = true,
        IsMaterialTransparent = true,
        IsMovable = true,
        GivesWood = true,
        Weight = 10
      };
    }

    protected PowerGenerator MakeObjPowerGenerator()
    {
      return new PowerGenerator("power generator", GameImages.OBJ_POWERGEN_OFF, GameImages.OBJ_POWERGEN_ON);
    }

    public MapObject MakeObjBoard(string imageID, string[] text)
    {
      return (MapObject) new Board("board", imageID, text);
    }

    public void DecorateOutsideWalls(Map map, Rectangle rect, Func<int, int, string> decoFn)
    {
      for (int left = rect.Left; left < rect.Right; ++left)
      {
        for (int top = rect.Top; top < rect.Bottom; ++top)
        {
          Tile tileAt = map.GetTileAt(left, top);
          if (!tileAt.Model.IsWalkable && !tileAt.IsInside)
          {
            string imageID = decoFn(left, top);
            if (imageID != null)
              tileAt.AddDecoration(imageID);
          }
        }
      }
    }

    public ItemMedicine MakeItemBandages()
    {
      return new ItemMedicine(m_Game.GameItems.BANDAGE) {
        Quantity = m_Rules.Roll(1, m_Game.GameItems.BANDAGE.StackingLimit)
      };
    }

    public ItemMedicine MakeItemMedikit()
    {
      return new ItemMedicine(m_Game.GameItems.MEDIKIT);
    }

    public ItemMedicine MakeItemPillsSTA()
    {
      return new ItemMedicine(m_Game.GameItems.PILLS_STA) {
        Quantity = m_Rules.Roll(1, m_Game.GameItems.PILLS_STA.StackingLimit)
      };
    }

    public ItemMedicine MakeItemPillsSLP()
    {
      return new ItemMedicine(m_Game.GameItems.PILLS_SLP) {
        Quantity = m_Rules.Roll(1, m_Game.GameItems.PILLS_SLP.StackingLimit)
      };
    }

    public ItemMedicine MakeItemPillsSAN()
    {
      return new ItemMedicine(m_Game.GameItems.PILLS_SAN) {
        Quantity = m_Rules.Roll(1, m_Game.GameItems.PILLS_SAN.StackingLimit)
      };
    }

    public ItemMedicine MakeItemPillsAntiviral()
    {
      return new ItemMedicine(m_Game.GameItems.PILLS_ANTIVIRAL) {
        Quantity = m_Rules.Roll(1, m_Game.GameItems.PILLS_ANTIVIRAL.StackingLimit)
      };
    }

    public ItemFood MakeItemGroceries()
    {
      int turnCounter = m_Game.Session.WorldTime.TurnCounter;
      int max = WorldTime.TURNS_PER_DAY * m_Game.GameItems.GROCERIES.BestBeforeDays;
      int min = max / 2;
      return new ItemFood(m_Game.GameItems.GROCERIES, turnCounter + m_Rules.Roll(min, max));
    }

    public ItemFood MakeItemCannedFood()
    {
      return new ItemFood(m_Game.GameItems.CANNED_FOOD) {
        Quantity = m_Rules.Roll(1, m_Game.GameItems.CANNED_FOOD.StackingLimit)
      };
    }

    public ItemMeleeWeapon MakeItemCrowbar()
    {
      return new ItemMeleeWeapon(m_Game.GameItems.CROWBAR) {
        Quantity = m_Rules.Roll(1, m_Game.GameItems.CROWBAR.StackingLimit)
      };
    }

    public ItemMeleeWeapon MakeItemBaseballBat()
    {
      return new ItemMeleeWeapon(m_Game.GameItems.BASEBALLBAT);
    }

    public ItemMeleeWeapon MakeItemCombatKnife()
    {
      return new ItemMeleeWeapon(m_Game.GameItems.COMBAT_KNIFE);
    }

    public ItemMeleeWeapon MakeItemTruncheon()
    {
      return new ItemMeleeWeapon(m_Game.GameItems.TRUNCHEON);
    }

    public ItemMeleeWeapon MakeItemGolfClub()
    {
      return new ItemMeleeWeapon(m_Game.GameItems.GOLFCLUB);
    }

    public ItemMeleeWeapon MakeItemIronGolfClub()
    {
      return new ItemMeleeWeapon(m_Game.GameItems.IRON_GOLFCLUB);
    }

    public ItemMeleeWeapon MakeItemHugeHammer()
    {
      return new ItemMeleeWeapon(m_Game.GameItems.HUGE_HAMMER);
    }

    public ItemMeleeWeapon MakeItemSmallHammer()
    {
      return new ItemMeleeWeapon(m_Game.GameItems.SMALL_HAMMER);
    }

    public ItemMeleeWeapon MakeItemJasonMyersAxe()
    {
      return new ItemMeleeWeapon(m_Game.GameItems.UNIQUE_JASON_MYERS_AXE) {
        IsUnique = true
      };
    }

    public ItemMeleeWeapon MakeItemShovel()
    {
      return new ItemMeleeWeapon(m_Game.GameItems.SHOVEL);
    }

    public ItemMeleeWeapon MakeItemShortShovel()
    {
      return new ItemMeleeWeapon(m_Game.GameItems.SHORT_SHOVEL);
    }

    public ItemBarricadeMaterial MakeItemWoodenPlank()
    {
      return new ItemBarricadeMaterial(m_Game.GameItems.WOODENPLANK);
    }

    public ItemRangedWeapon MakeItemHuntingCrossbow()
    {
      return new ItemRangedWeapon(m_Game.GameItems.HUNTING_CROSSBOW);
    }

    public ItemAmmo MakeItemBoltsAmmo()
    {
      return new ItemAmmo(m_Game.GameItems.AMMO_BOLTS);
    }

    public ItemRangedWeapon MakeItemHuntingRifle()
    {
      return new ItemRangedWeapon(m_Game.GameItems.HUNTING_RIFLE);
    }

    public ItemAmmo MakeItemLightRifleAmmo()
    {
      return new ItemAmmo(m_Game.GameItems.AMMO_LIGHT_RIFLE);
    }

    public ItemRangedWeapon MakeItemPistol()
    {
      return new ItemRangedWeapon(m_Game.GameItems.PISTOL);
    }

    public ItemRangedWeapon MakeItemKoltRevolver()
    {
      return new ItemRangedWeapon(m_Game.GameItems.KOLT_REVOLVER);
    }

    public ItemRangedWeapon MakeItemRandomPistol()
    {
      if (!m_Game.Rules.RollChance(50))
        return MakeItemKoltRevolver();
      return MakeItemPistol();
    }

    public ItemAmmo MakeItemLightPistolAmmo()
    {
      return new ItemAmmo(m_Game.GameItems.AMMO_LIGHT_PISTOL);
    }

    public ItemRangedWeapon MakeItemShotgun()
    {
      return new ItemRangedWeapon(m_Game.GameItems.SHOTGUN);
    }

    public ItemAmmo MakeItemShotgunAmmo()
    {
      return new ItemAmmo(m_Game.GameItems.AMMO_SHOTGUN);
    }

    public ItemBodyArmor MakeItemCHARLightBodyArmor()
    {
      return new ItemBodyArmor(m_Game.GameItems.CHAR_LT_BODYARMOR);
    }

    public ItemBodyArmor MakeItemBikerGangJacket(GameGangs.IDs gangId)
    {
      switch (gangId) {
        case GameGangs.IDs.BIKER_HELLS_SOULS:
          return new ItemBodyArmor(m_Game.GameItems.HELLS_SOULS_JACKET);
        case GameGangs.IDs.BIKER_FREE_ANGELS:
          return new ItemBodyArmor(m_Game.GameItems.FREE_ANGELS_JACKET);
        default:
          throw new ArgumentException("unhandled biker gang");
      }
    }

    public ItemBodyArmor MakeItemPoliceJacket()
    {
      return new ItemBodyArmor(m_Game.GameItems.POLICE_JACKET);
    }

    public ItemBodyArmor MakeItemPoliceRiotArmor()
    {
      return new ItemBodyArmor(m_Game.GameItems.POLICE_RIOT);
    }

    public ItemBodyArmor MakeItemHunterVest()
    {
      return new ItemBodyArmor(m_Game.GameItems.HUNTER_VEST);
    }

    public ItemTracker MakeItemCellPhone()
    {
      return new ItemTracker(m_Game.GameItems.CELL_PHONE);
    }

    public ItemSprayPaint MakeItemSprayPaint()
    {
      ItemSprayPaintModel itemSprayPaintModel;
      switch (m_Game.Rules.Roll(0, 4))
      {
        case 0:
          itemSprayPaintModel = m_Game.GameItems.SPRAY_PAINT1;
          break;
        case 1:
          itemSprayPaintModel = m_Game.GameItems.SPRAY_PAINT2;
          break;
        case 2:
          itemSprayPaintModel = m_Game.GameItems.SPRAY_PAINT3;
          break;
        case 3:
          itemSprayPaintModel = m_Game.GameItems.SPRAY_PAINT4;
          break;
        default:
          throw new ArgumentOutOfRangeException("unhandled roll");
      }
      return new ItemSprayPaint(itemSprayPaintModel);
    }

    public ItemSprayScent MakeItemStenchKiller()
    {
      return new ItemSprayScent(m_Game.GameItems.STENCH_KILLER);
    }

    public ItemRangedWeapon MakeItemArmyRifle()
    {
      return new ItemRangedWeapon(m_Game.GameItems.ARMY_RIFLE);
    }

    public ItemRangedWeapon MakeItemPrecisionRifle()
    {
      return new ItemRangedWeapon(m_Game.GameItems.PRECISION_RIFLE);
    }

    public ItemAmmo MakeItemHeavyRifleAmmo()
    {
      return new ItemAmmo(m_Game.GameItems.AMMO_HEAVY_RIFLE);
    }

    public ItemRangedWeapon MakeItemArmyPistol()
    {
      return new ItemRangedWeapon(m_Game.GameItems.ARMY_PISTOL);
    }

    public ItemAmmo MakeItemHeavyPistolAmmo()
    {
      return new ItemAmmo(m_Game.GameItems.AMMO_HEAVY_PISTOL);
    }

    public ItemBodyArmor MakeItemArmyBodyArmor()
    {
      return new ItemBodyArmor(m_Game.GameItems.ARMY_BODYARMOR);
    }

    public ItemFood MakeItemArmyRation()
    {
      return new ItemFood(m_Game.GameItems.ARMY_RATION, m_Game.Session.WorldTime.TurnCounter + WorldTime.TURNS_PER_DAY * m_Game.GameItems.ARMY_RATION.BestBeforeDays);
    }

    public ItemLight MakeItemFlashlight()
    {
      return new ItemLight(m_Game.GameItems.FLASHLIGHT);
    }

    public ItemLight MakeItemBigFlashlight()
    {
      return new ItemLight(m_Game.GameItems.BIG_FLASHLIGHT);
    }

    public ItemTracker MakeItemZTracker()
    {
      return new ItemTracker(m_Game.GameItems.ZTRACKER);
    }

    public ItemTracker MakeItemBlackOpsGPS()
    {
      return new ItemTracker(m_Game.GameItems.BLACKOPS_GPS);
    }

    public ItemTracker MakeItemPoliceRadio()
    {
      return new ItemTracker(m_Game.GameItems.POLICE_RADIO);
    }

    public ItemGrenade MakeItemGrenade()
    {
      return new ItemGrenade(m_Game.GameItems.GRENADE, m_Game.GameItems.GRENADE_PRIMED) {
        Quantity = m_Rules.Roll(1, m_Game.GameItems.GRENADE.StackingLimit)
      };
    }

    public ItemTrap MakeItemBearTrap()
    {
      return new ItemTrap(m_Game.GameItems.BEAR_TRAP);
    }

    public ItemTrap MakeItemSpikes()
    {
      return new ItemTrap(m_Game.GameItems.SPIKES) {
        Quantity = m_Rules.Roll(1, m_Game.GameItems.BARBED_WIRE.StackingLimit)
      };
    }

    public ItemTrap MakeItemBarbedWire()
    {
      return new ItemTrap(m_Game.GameItems.BARBED_WIRE) {
        Quantity = m_Rules.Roll(1, m_Game.GameItems.BARBED_WIRE.StackingLimit)
      };
    }

    public ItemEntertainment MakeItemBook()
    {
      return new ItemEntertainment(m_Game.GameItems.BOOK);
    }

    public ItemEntertainment MakeItemMagazines()
    {
      return new ItemEntertainment(m_Game.GameItems.MAGAZINE) {
        Quantity = m_Rules.Roll(1, m_Game.GameItems.MAGAZINE.StackingLimit)
      };
    }

    protected void BarricadeDoors(Map map, Rectangle rect, int barricadeLevel)
    {
      for (int left = rect.Left; left < rect.Right; ++left) {
        for (int top = rect.Top; top < rect.Bottom; ++top) {
          DoorWindow doorWindow = map.GetMapObjectAt(left, top) as DoorWindow;
          if (doorWindow != null) doorWindow.Barricade(barricadeLevel);
        }
      }
    }

    protected Zone MakeUniqueZone(string basename, Rectangle rect)
    {
      return new Zone(string.Format("{0}@{1}-{2}", (object) basename, (object) (rect.Left + rect.Width / 2), (object) (rect.Top + rect.Height / 2)), rect);
    }
  }
}
