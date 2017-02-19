// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Data
{
  internal class ActorModel
  {
    public Gameplay.GameActors.IDs ID { get; set; }
    public string ImageID { get; private set; }
    public DollBody DollBody { get; private set;  }
    public string Name { get; private set; }
    public string PluralName { get; private set; }
    public ActorSheet StartingSheet { get; private set; }
    public Abilities Abilities { get; private set; }
    public Type DefaultController { get; private set; }
    public int CreatedCount { get; private set; }
    public int ScoreValue { get; private set; }
    public string FlavorDescription { get; private set; }

    public ActorModel(string imageID, string name, string pluralName, int scoreValue, string flavor, DollBody body, Abilities abilities, ActorSheet startingSheet, Type defaultController)
    {
      Contract.Requires(!string.IsNullOrEmpty(name));
      Contract.Requires(!string.IsNullOrEmpty(flavor));
      Contract.Requires(null != body);
      Contract.Requires(null != abilities);
      Contract.Requires(null != startingSheet);
      Contract.Requires(null != defaultController);
      Contract.Requires(defaultController.IsSubclassOf(typeof(ActorController)));
      // using logical XOR to restate IFF, logical OR to restate logical implication
      Contract.Requires(abilities.HasInventory ^ (0 >= startingSheet.BaseInventoryCapacity));
      Contract.Requires((abilities.HasToEat || abilities.IsRotting) ^ (0 >= startingSheet.BaseFoodPoints));
      Contract.Requires(abilities.HasToSleep ^ (0 >= startingSheet.BaseSleepPoints));
      Contract.Requires(abilities.HasSanity ^ (0 >= startingSheet.BaseSanity));
      Contract.Requires(!abilities.CanTrade || defaultController.IsSubclassOf(typeof(Gameplay.AI.OrderableAI)));
      Contract.Requires(!defaultController.IsSubclassOf(typeof(Gameplay.AI.OrderableAI)) || abilities.HasInventory);

      ImageID = imageID;
      DollBody = body;
      Name = name;
      PluralName = pluralName;
      StartingSheet = startingSheet;
      Abilities = abilities;
      DefaultController = defaultController;
      ScoreValue = scoreValue;
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
