// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Runtime.Serialization;
using DoorWindow = djack.RogueSurvivor.Engine.MapObjects.DoorWindow;

#nullable enable

namespace djack.RogueSurvivor.Data
{
  internal class ActorModel // looks like a good case for readonly struct, but would need reference return
  {
    static private ulong[] _createdCounts = new ulong[(int)Gameplay.GameActors.IDs._COUNT];

    public readonly Gameplay.GameActors.IDs ID;
    public readonly string? ImageID;
    public readonly DollBody DollBody;
    public readonly string Name;
    public readonly string PluralName;
    public readonly ActorSheet StartingSheet;
    public readonly Abilities Abilities;
    public readonly Type DefaultController;
    public readonly int ScoreValue;
    public readonly string FlavorDescription;

    // backward-compatible aliasing
    private ulong CreatedCount {
      get { return _createdCounts[(int)ID]; }
      set { _createdCounts[(int)ID] = value; }
    }

#region Session save/load assistants
    static public void Load(SerializationInfo info, StreamingContext context)
    {
      _createdCounts = (ulong[]) info.GetValue("GameActors_createdCounts", typeof(ulong[]));
    }

    static public void Save(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("GameActors_createdCounts", _createdCounts);
    }

    // it is a semantic error for the created count to be negative, so we
    // don't need to binary-encode against the possibility of negative values
    static public void Load(Zaimoni.Serialization.DecodeObjects decode)
    {
        decode.LoadFrom7bit(ref _createdCounts);
        if ((int)Gameplay.GameActors.IDs._COUNT != _createdCounts.Length) throw new InvalidOperationException("need upgrade path for Actor::Load");
        // \todo auto-repair if the incoming count is incorrect
    }

    static public void Save(Zaimoni.Serialization.EncodeObjects encode) => encode.SaveTo7bit(_createdCounts);
#endregion

    public ActorModel(Gameplay.GameActors.IDs id, string? imageID, string name, string pluralName, int scoreValue, string flavor, DollBody body, Abilities abilities, ActorSheet startingSheet, Type defaultController)
    {
#if DEBUG
      // using logical XOR to restate IFF, logical AND to restate logical implication
      if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
      if (string.IsNullOrEmpty(flavor)) throw new ArgumentNullException(nameof(flavor));
      if (!defaultController.IsSubclassOf(typeof(ActorController))) throw new InvalidOperationException("!defaultController.IsSubclassOf(typeof(ActorController))");
      if (abilities.HasInventory ^ (0 < startingSheet.BaseInventoryCapacity)) throw new InvalidOperationException("abilities.HasInventory ^ (0 < startingSheet.BaseInventoryCapacity)");
      if ((abilities.HasToEat || abilities.IsRotting) ^ (0 < startingSheet.BaseFoodPoints)) throw new InvalidOperationException("(abilities.HasToEat || abilities.IsRotting) ^ (0 < startingSheet.BaseFoodPoints)");
      if (abilities.HasToSleep ^ (0 < startingSheet.BaseSleepPoints)) throw new InvalidOperationException("abilities.HasToSleep ^ (0 < startingSheet.BaseSleepPoints)");
      if (abilities.HasSanity ^ (0 < startingSheet.BaseSanity)) throw new InvalidOperationException("abilities.HasSanity ^ (0 < startingSheet.BaseSanity)");
      // OrderableAI should implement all of STD_LIVING, STD_HUMAN, and STD_SANE flags
      // InsaneHumanAI currently implements all of STD_LIVING and STD_HUMAN flags, but not STD_SANE flags
      if (abilities.CanTrade && !defaultController.IsSubclassOf(typeof(Gameplay.AI.OrderableAI))) throw new InvalidOperationException("abilities.CanTrade && !defaultController.IsSubclassOf(typeof(Gameplay.AI.OrderableAI))");
      if (!abilities.CanBarricade && defaultController.IsSubclassOf(typeof(Gameplay.AI.OrderableAI))) throw new InvalidOperationException("!abilities.CanBarricade && defaultController.IsSubclassOf(typeof(Gameplay.AI.OrderableAI))");
      if (!abilities.CanUseItems && defaultController.IsSubclassOf(typeof(Gameplay.AI.OrderableAI))) throw new InvalidOperationException("!abilities.CanUseItems && defaultController.IsSubclassOf(typeof(Gameplay.AI.OrderableAI))");
      if (!abilities.HasInventory && defaultController.IsSubclassOf(typeof(Gameplay.AI.OrderableAI))) throw new InvalidOperationException("!abilities.HasInventory && defaultController.IsSubclassOf(typeof(Gameplay.AI.OrderableAI))");
      if (!abilities.CanTalk && defaultController.IsSubclassOf(typeof(Gameplay.AI.OrderableAI))) throw new InvalidOperationException("!abilities.CanTalk && defaultController.IsSubclassOf(typeof(Gameplay.AI.OrderableAI))");
#endif
      ID = id;
      ImageID = imageID;
      DollBody = body;
      Name = name;
      PluralName = pluralName;
      StartingSheet = startingSheet;
      Abilities = abilities;
      DefaultController = defaultController;
      ScoreValue = scoreValue;
      FlavorDescription = flavor;
    }

    private Actor Create(Faction faction, int spawnTime, string properName="")
    {
      ++CreatedCount;
      // \todo? subclassing actor (e.g. on inventory existence which is an ActorModel property) would be done here
      return new Actor(this, faction, spawnTime, properName);
    }

    // should be private, but savefile auto-repair contraindicates
    static readonly private Type[] _controller_signature = new Type[] { typeof(Actor) };
    public ActorController InstanciateController(Actor a)
    {
      return (DefaultController.GetConstructor(_controller_signature).Invoke(new object[] { a }) as ActorController)!;
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

    public Actor CreateNamed(Faction faction, string properName, int spawnTime)
    {
      Actor actor = Create(faction, spawnTime, properName);
      actor.IsProperName = true;
      return actor;
    }

    private string ReasonCantBreak(MapObject mapObj)
    {
      if (!Abilities.CanBreakObjects) return "cannot break objects";
      bool flag = (mapObj as DoorWindow)?.IsBarricaded ?? false;
      if (mapObj.BreakState != MapObject.Break.BREAKABLE && !flag) return "can't break this object";
      return "";
    }

    public bool CanBreak(MapObject mapObj, out string reason)
    {
      reason = ReasonCantBreak(mapObj);
      return string.IsNullOrEmpty(reason);
    }

    public bool CanBreak(MapObject mapObj)
    {
      return string.IsNullOrEmpty(ReasonCantBreak(mapObj));
    }
  }
}
