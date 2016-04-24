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
    public int ID { get; set; }
    public string ImageID { get; private set; }
    public DollBody DollBody { get; private set;  }
    public string Name { get; private set; }
    public string PluralName { get; private set; }
    public ActorSheet StartingSheet { get; private set; }
    public Abilities Abilities { get; private set; }
    public Type DefaultController { get; private set; }
    public int CreatedCount { get; private set; }
    public int ScoreValue { get; private set; }
    public string FlavorDescription { get; set; }

    public ActorModel(string imageID, string name, string pluralName, int scoreValue, DollBody body, Abilities abilities, ActorSheet startingSheet, Type defaultController)
    {
#if DEBUG
      if (name == null)
        throw new ArgumentNullException("name");
      if (body == null)
        throw new ArgumentNullException("body");
      if (abilities == null)
        throw new ArgumentNullException("abilities");
      if (startingSheet == null)
        throw new ArgumentNullException("startingSheet");
      if (defaultController == null)
        throw new ArgumentNullException("defaultController");
      if (!defaultController.IsSubclassOf(typeof (ActorController)))
        throw new ArgumentException("defaultController is not a subclass of ActorController");
#endif
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

    private Actor Create(Faction faction, int spawnTime)
    {
      ++CreatedCount;
      return new Actor(this, faction, spawnTime)
      {
        Controller = InstanciateController()
      };
    }

    private ActorController InstanciateController()
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
      Actor actor = Create(faction, spawnTime);
      actor.Name = properName;
      actor.IsProperName = true;
      actor.IsPluralName = isPluralName;
      return actor;
    }
  }
}
