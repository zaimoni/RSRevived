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

using Point = Zaimoni.Data.Vector2D_short;
using Rectangle = Zaimoni.Data.Box2D_short;

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

    // alpha10.1 added new names
    private static readonly string[] MALE_FIRST_NAMES = new string[]
    {
      "Aaron", "Adam", "Adrian", "Alan", "Albert", "Alberto", "Alex", "Alexander", "Alfred", "Alfredo", "Allan", "Allen", "Alvin", "Andre", "Andrew", "Andy", "Angel", "Anton", "Antonio", "Anthony", "Armando", "Arnold", "Arthur", "Ashley", "Axel",
      "Barry", "Ben", "Benjamin", "Bernard", "Bill", "Billy", "Bob", "Bobby", "Brad", "Brandon", "Bradley", "Brent", "Brett", "Brian", "Bryan", "Bruce", "Byron",
      "Caine", "Calvin", "Carl", "Carlos", "Carlton", "Casey", "Cecil", "Chad", "Charles", "Charlie", "Chester", "Chris", "Christian", "Christopher", "Clarence", "Clark", "Claude", "Clayton", "Clifford", "Clifton", "Clinton", "Clyde", "Cody", "Corey", "Cory", "Craig", "Cris", "Cristobal", "Curtis",
      "Dan", "Daniel", "Danny", "Dale", "Darrell", "Darren", "Darryl", "Dave", "David", "Dean", "Dennis", "Derek", "Derrick", "Dirk", "Don", "Donald", "Donovan", "Doug", "Douglas", "Duane", "Dustin", "Dwayne", "Dwight",
      "Earl", "Ed", "Eddie", "Eddy", "Edgar", "Eduardo", "Edward", "Edwin", "Elias", "Elie", "Elmer", "Elton", "Enrique", "Eric", "Erik", "Ernest", "Eugene", "Everett",
      "Felix", "Fernando", "Floyd", "Francis", "Francisco", "Frank", "Franklin", "Fred", "Frederick", "Freddie",
      "Gabriel", "Gary", "Gene", "George", "Georges", "Gerald", "Gilbert", "Glenn", "Gordon", "Greg", "Gregory", "Guy",
      "Hank", "Harold", "Harvey", "Harry", "Hector", "Henry", "Herbert", "Herman", "Howard", "Hubert", "Hugh", "Hughie",
      "Ian", "Indy", "Isaac", "Ivan",
      "Jack", "Jacob", "Jaime", "Jake", "James", "Jamie", "Jared", "Jarvis", "Jason", "Javier", "Jay", "Jeff", "Jeffrey", "Jeremy", "Jerome", "Jerry", "Jesse", "Jessie", "Jesus", "Jim", "Jimmie", "Jimmy", "Joe", "Joel", "John", "Johnnie", "Johnny", "Jon", "Jonas", "Jonathan", "Jordan", "Jorge", "Jose", "Joseph", "Joshua", "Juan", "Julian", "Julio", "Justin",
      "Karl", "Keith", "Kelly", "Ken", "Kenneth", "Kent", "Kevin", "Kirk", "Kurt", "Kyle",
      "Lance", "Larry", "Lars", "Lawrence", "Lee", "Lennie", "Leo", "Leon", "Leonard", "Leroy", "Leslie", "Lester", "Lewis", "Lloyd", "Lonnie", "Louis", "Luis",
      "Manuel", "Marc", "Marcus", "Mario", "Mark", "Marshall", "Martin", "Marvin", "Maurice", "Matthew", "Max", "Melvin", "Michael", "Mickey", "Miguel", "Mike", "Milton", "Mitch", "Mitchell", "Morris",
      "Nathan", "Nathaniel", "Ned", "Neil", "Nelson", "Nicholas", "Nick", "Norman",
      "Oliver", "Orlando", "Oscar",
      "Pablo", "Patrick", "Paul", "Pedro", "Perry", "Pete", "Peter", "Phil", "Phillip", "Preston",
      "Quentin",
      "Rafael", "Ralph", "Ramon", "Randall", "Randy", "Raul", "Ray", "Raymond", "Reginald", "Rene", "Ricardo", "Richard", "Rick", "Ricky", "Rob", "Robert", "Roberto", "Rodney", "Roger", "Roland", "Ron", "Ronald", "Ronnie", "Ross", "Roy", "Ruben", "Rudy", "Russell", "Ryan",
      "Salvador", "Sam", "Samuel", "Saul", "Scott", "Sean", "Seth", "Sergio", "Shane", "Shaun", "Shawn", "Sidney", "Stan", "Stanley", "Stephen", "Steve", "Steven", "Stuart",
      "Ted", "Terrance", "Terrence", "Terry", "Theodore", "Thomas", "Tim", "Timothy", "Toby", "Todd", "Tom", "Tommy", "Tony", "Tracy", "Travis", "Trevor", "Troy", "Tyler", "Tyrone",
      "Ulrich",
      "Val", "Vernon", "Vince", "Vincent", "Vinnie", "Victor", "Virgil",
      "Wade", "Wallace", "Walter", "Warren", "Wayne", "Wesley", "Willard", "William", "Willie",
      "Xavier",
      // Y
      "Zachary"
    };

    // alpha10.1 added new names
    private static readonly string[] FEMALE_FIRST_NAMES = new string[]
    {
      "Abigail", "Agnes", "Ali", "Alice", "Alicia", "Allison", "Alma", "Amanda", "Amber", "Amy", "Andrea", "Angela", "Anita", "Ana", "Ann", "Anna", "Anne", "Annette", "Annie", "April", "Arlene", "Ashley", "Audrey",
      "Barbara", "Beatrice", "Becky", "Belinda", "Bernice", "Bertha", "Bessie", "Beth", "Betty", "Beverly", "Billie", "Bobbie", "Bonnie", "Brandy", "Brenda", "Britanny",
      "Carla", "Carmen", "Carol", "Carole", "Caroline", "Carolyn", "Carrie", "Cassandra", "Cassie", "Cathy", "Catherine", "Charlene", "Charlotte", "Cherie", "Cheryl", "Christina", "Christine", "Christy", "Cindy", "Claire", "Clara", "Claudia", "Colleen", "Connie", "Constance", "Courtney", "Cris", "Crissie", "Crystal",  "Cynthia",
      "Daisy", "Dana", "Danielle", "Darlene", "Dawn", "Deanna", "Debbie", "Deborah", "Debrah", "Delores", "Denise", "Diana", "Diane", "Donna", "Dolores", "Dora", "Doris", "Dorothy",
      "Edith", "Edna", "Eileen", "Elaine", "Elayne", "Eleanor", "Eleonor", "Elizabeth", "Ella", "Ellen", "Elsie", "Emily", "Emma", "Erica", "Erika", "Erin", "Esther", "Ethel", "Eva", "Evelyn",
      "Felicia", "Fiona", "Florence", "Fran", "Frances",
      "Gail", "Georgia", "Geraldine", "Gertrude", "Gina", "Ginger", "Gladys", "Glenda", "Gloria", "Grace", "Gwendolyn",
      "Georges",   // RS Revived
      "Hazel", "Heather", "Heidi", "Helen", "Helena", "Hilary", "Hilda", "Holly", "Holy",
      "Ida", "Ingrid", "Irene", "Irma", "Isabela",
      "Jackie", "Jacqueline", "Jamie", "Jane", "Janet", "Janice", "Jean", "Jeanne", "Jeanette", "Jennie", "Jennifer", "Jenny", "Jess", "Jessica", "Jessie", "Jill", "Jo", "Joan", "Joana", "Joanne", "Josephine", "Joy", "Joyce", "Juanita", "Judith", "Judy", "Julia", "Julie", "June",
      "Karen", "Kate", "Katherine", "Kathleen", "Kathy", "Kathryn", "Katie", "Katrina", "Kay", "Kelly", "Kim", "Kimberly", "Kira", "Kristen", "Kristin", "Kristina",
      "Laura", "Lauren", "Laurie", "Lea", "Lena", "Leona", "Leonor", "Leslie", "Lillian", "Lillie", "Linda", "Lindsay", "Lisa", "Liz", "Lois", "Loretta", "Lori", "Lorraine", "Louise", "Lucia", "Lucille", "Lucy", "Lydia", "Lynn",
      "Mabel", "Mae", "Maggie", "Marcia", "Margaret", "Margie", "Maria", "Marian", "Marie", "Marion", "Marjorie", "Marlene", "Marsha", "Martha", "Mary", "Marylin", "Mary-Ann", "Mattie", "Maureen", "Maxine", "Megan", "Melanie", "Melinda", "Melissa", "Michele", "Mildred", "Millie", "Minnie", "Miriam", "Misty", "Molly", "Monica", "Myrtle",
      "Naomi", "Nancy", "Natalie", "Nelly", "Nicole", "Nina", "Nora", "Norma",
      "Olga", "Ophelia",
      "Paquita", "Page", "Pamela", "Patricia", "Patsy", "Patty", "Paula", "Pauline", "Pearl", "Peggy", "Penny", "Phyllis", "Priscilla",
      // Q
      "Rachel", "Ramona", "Raquel", "Rebecca", "Regina", "Renee", "Rhonda", "Rita", "Roberta", "Robin", "Rosa", "Rose", "Rosemary", "Ruby", "Ruth",
      "Sabrina", "Sally", "Samantha", "Sandra", "Sara", "Sarah", "Shannon", "Sharon", "Sheila", "Shelly", "Sherry", "Shirley", "Sofia", "Sonia", "Stacey", "Stacy", "Stella", "Stephanie", "Sue", "Susan", "Suzanne", "Sylvia",
      "Tabatha", "Tamara", "Tammy", "Tanya", "Tara", "Terri", "Terry", "Tess", "Thelma", "Theresa", "Tiffany", "Tina", "Toni", "Tonya", "Tori", "Tracey", "Tracy",
      // U
      "Valerie", "Vanessa", "Velma", "Vera", "Veronica", "Vickie", "Victoria", "Viola", "Violet", "Virginia", "Vivian",
      "Wanda", "Wendy", "Willie", "Wilma", "Winona",
      // X
      "Yolanda", "Yvone",
      "Zora"
    };

    // alpha10.1 added new names
    private static readonly string[] LAST_NAMES = new string[]
    {
      "Adams", "Alexander", "Allen", "Anderson", "Austin",
      "Bailey", "Baker", "Barnes", "Bell", "Bennett", "Bent", "Black", "Bradley", "Brown", "Brooks", "Bryant", "Bush", "Butler",
      "Campbell", "Carpenter", "Carter", "Clark", "Coleman", "Collins", "Cook", "Cooper", "Cordell", "Cox",
      "Davis", "Diaz", "Dobbs",
      "Edwards", "Engels", "Evans",
      "Finch", "Flores", "Ford", "Forrester", "Foster",
      "Garcia", "Gates", "Gonzales", "Gonzalez", "Gray", "Green", "Griffin",
      "Hall", "Harris", "Hayes", "Henderson", "Hernandez", "Hewlett", "Hill", "Holtz", "Howard", "Hughes",
      "Irvin",
      "Jackson", "James", "Jenkins", "Johnson", "Jones",
      "Kelly", "Kennedy", "King",
      "Lambert", "Lesaint", "Lee", "Lewis", "Long", "Lopez",
      "Malory", "Martin", "Martinez", "McAllister", "McGready", "Miller", "Mitchell", "Moore", "Morgan", "Morris", "Murphy",
      "Nelson", "Norton",
      "O'Brien", "Oswald",
      "Parker", "Patterson", "Paul", "Perez", "Perry", "Peterson", "Phillips", "Pitt", "Powell", "Price",
      "Quinn",
      "Ramirez", "Reed", "Reeves", "Richardson", "Rivera", "Roberts", "Robinson", "Rockwell", "Rodriguez", "Rogers", "Robertson", "Ross", "Russell",
      "Sanchez", "Sanders", "Scott", "Simmons", "Smith", "Stevens", "Steward", "Stewart",
      "Tarver", "Taylor", "Thomas", "Thompson", "Torres", "Turner",
      "Ulrich",
      "Vance",
      "Walker", "Ward", "Walters", "Washington", "Watson", "White", "Williams", "Wilson", "Wood", "Wright",
      // X
      "Young"
      // Z
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
      actor.CommandLinePlayer();
    }

    protected void GiveRandomSkillsToActor(Actor actor, int count)
    {
      while(0 < count--) GiveRandomSkillToActor(m_DiceRoller, actor);
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

    static protected MapObject MakeObjGardenFence()  // alpha10
    {
      return new MapObject(GameImages.OBJ_GARDEN_FENCE, DoorWindow.BASE_HITPOINTS/2);
    }

    static protected MapObject MakeObjWireFence()  // alpha10
    {
      return new MapObject(GameImages.OBJ_WIRE_FENCE, DoorWindow.BASE_HITPOINTS);
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

    static protected void DecorateOutsideWalls(Map map, Rectangle rect, Func<Point, string> decoFn)
    {
#if DEBUG
      if (null == decoFn) throw new ArgumentNullException(nameof(decoFn));
#endif
      for (var left = rect.Left; left < rect.Right; ++left) {
        for (var top = rect.Top; top < rect.Bottom; ++top) {
          Point pt = new Point(left,top);
          Tile tileAt = map.GetTileAt(pt);
          if (!tileAt.Model.IsWalkable && !tileAt.IsInside) {
            string imageID = decoFn(pt);
            if (imageID != null)
              tileAt.AddDecoration(imageID);
          }
        }
      }
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

    public ItemRangedWeapon MakeItemRandomPistol()
    {
      return (m_DiceRoller.RollChance(50) ? GameItems.KOLT_REVOLVER : GameItems.PISTOL).instantiate();
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

    public ItemGrenade MakeItemGrenade()
    {
      return new ItemGrenade(GameItems.GRENADE, GameItems.GRENADE_PRIMED) {
        Quantity = m_DiceRoller.Roll(1, GameItems.GRENADE.StackingLimit)
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

    // currently, the world map only has in-city districts.  This is not guaranteed indefinitely for Rogue Survivor Revived; it would be "interesting" for
    // * VAPORWARE: a working gas station to be needed for Bikers/Gangsters to stage raids
    // * VAPORWARE: a working national guard base to be needed for one or both of patrols and supply drops
    public District RandomDistrictInCity() {
      World world = Session.Get.World;
      return world[m_Rules.Roll(0, world.Size), m_Rules.Roll(0, world.Size)];
    }
  }
}
