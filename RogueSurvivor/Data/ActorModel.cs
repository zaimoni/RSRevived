// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  internal class ActorModel
  {
    public Gameplay.GameActors.IDs ID { get; set; }
    public readonly string ImageID;
    public readonly DollBody DollBody;
    public readonly string Name;
    public readonly string PluralName;
    public readonly ActorSheet StartingSheet;
    public readonly Abilities Abilities;
    public readonly Type DefaultController;
    public int CreatedCount { get; private set; }   // XXX resets on restart; needs to be where it can reach the savefile
    public readonly int ScoreValue;
    public readonly string FlavorDescription;

    public ActorModel(string imageID, string name, string pluralName, int scoreValue, string flavor, DollBody body, Abilities abilities, ActorSheet startingSheet, Type defaultController)
    {
#if DEBUG
      // using logical XOR to restate IFF, logical AND to restate logical implication
      if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
      if (string.IsNullOrEmpty(flavor)) throw new ArgumentNullException(nameof(flavor));
      if (null == body) throw new ArgumentNullException(nameof(body));
      if (null == abilities) throw new ArgumentNullException(nameof(abilities));
      if (null == startingSheet) throw new ArgumentNullException(nameof(startingSheet));
      if (null == defaultController) throw new ArgumentNullException(nameof(defaultController));
      if (!defaultController.IsSubclassOf(typeof(ActorController))) throw new ArgumentOutOfRangeException(nameof(defaultController), "!defaultController.IsSubclassOf(typeof(ActorController))");
      if (abilities.HasInventory ^ (0 < startingSheet.BaseInventoryCapacity)) throw new ArgumentOutOfRangeException(nameof(abilities)+","+nameof(startingSheet), "abilities.HasInventory ^ (0 < startingSheet.BaseInventoryCapacity)");
      if ((abilities.HasToEat || abilities.IsRotting) ^ (0 < startingSheet.BaseFoodPoints)) throw new ArgumentOutOfRangeException(nameof(abilities) + "," + nameof(startingSheet), "(abilities.HasToEat || abilities.IsRotting) ^ (0 < startingSheet.BaseFoodPoints)");
      if (abilities.HasToSleep ^ (0 < startingSheet.BaseSleepPoints)) throw new ArgumentOutOfRangeException(nameof(abilities) + "," + nameof(startingSheet), "abilities.HasToSleep ^ (0 < startingSheet.BaseSleepPoints)");
      if (abilities.HasSanity ^ (0 < startingSheet.BaseSanity)) throw new ArgumentOutOfRangeException(nameof(abilities) + "," + nameof(startingSheet), "abilities.HasSanity ^ (0 < startingSheet.BaseSanity)");
      if (abilities.CanTrade && !defaultController.IsSubclassOf(typeof(Gameplay.AI.OrderableAI))) throw new ArgumentOutOfRangeException(nameof(abilities) + "," + nameof(defaultController), "abilities.CanTrade && !defaultController.IsSubclassOf(typeof(Gameplay.AI.OrderableAI))");
      if (!abilities.CanBarricade && defaultController.IsSubclassOf(typeof(Gameplay.AI.OrderableAI))) throw new ArgumentOutOfRangeException(nameof(abilities) + "," + nameof(defaultController), "!abilities.CanBarricade && defaultController.IsSubclassOf(typeof(Gameplay.AI.OrderableAI))");
      if (!abilities.HasInventory && defaultController.IsSubclassOf(typeof(Gameplay.AI.OrderableAI))) throw new ArgumentOutOfRangeException(nameof(abilities) + "," + nameof(defaultController), "!abilities.HasInventory && defaultController.IsSubclassOf(typeof(Gameplay.AI.OrderableAI))");
#endif
      ImageID = imageID;
      DollBody = body;
      Name = name;
      PluralName = pluralName;
      StartingSheet = startingSheet;
      Abilities = abilities;
      DefaultController = defaultController;
      ScoreValue = scoreValue;
      FlavorDescription = flavor;
      CreatedCount = 0;
    }

    private Actor Create(Faction faction, int spawnTime, string properName="")
    {
      ++CreatedCount;
      return new Actor(this, faction, spawnTime, properName)
      {
        Controller = InstanciateController()
      };
    }

    // should be private, but savefile auto-repair contraindicates
    public ActorController InstanciateController()
    {
      return DefaultController.GetConstructor(Type.EmptyTypes).Invoke((object[]) null) as ActorController;
    }

    public Actor CreateAnonymous(Faction faction, int spawnTime)
    {
      return Create(faction, spawnTime);
    }

    public Actor CreateNumberedName(Faction faction, int spawnTime)
    {
      Actor actor = Create(faction, spawnTime);
      actor.Name += CreatedCount.ToString();
      actor.IsProperName = true;
      return actor;
    }

    public Actor CreateNamed(Faction faction, string properName, bool isPluralName, int spawnTime)
    {
      Actor actor = Create(faction, spawnTime, properName);
      actor.IsProperName = true;
      actor.IsPluralName = isPluralName;
      return actor;
    }
  }
}
