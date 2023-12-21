// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.Serialization;
using DoorWindow = djack.RogueSurvivor.Engine.MapObjects.DoorWindow;

#nullable enable

namespace djack.RogueSurvivor.Data
{
// problems with prior ActorSheet class, and System.Runtime.Serialization.BinaryFormatter
// trigger was Attack and Defence structs

// Actor
// 2019-10-19: class ok with automatic deserialization, but (readonly) struct with readonly fields is not even
// though it automatically serializes. This is an explicit reversion of
// https://github.com/dotnet/coreclr/pull/21193  (approved merge date 2018-11-26) so even if this is fixed
// in a later version of C#, we can't count on the fix remaining.
// Failure point is before the ISerializable-based constructor is called, so that doesn't work as a bypass.
// this appears related to https://github.com/dotnet/corefx/issues/33655 i.e. anything trying to save/load a dictionary dies.

// ActorSheet
// Oct 19 2019: automatic serialization code makes structs flagged readonly within a class (whether normal or readonly)
// save fine, but hard-crash on loading before an ISerializable constructor can be called.

  internal class ActorModel // looks like a good case for readonly struct, but would need reference return
  {
    static private readonly ulong[] _createdCounts = new ulong[(int)Gameplay.GameActors.IDs._COUNT];
    static private readonly Verb BITE = new Verb("bite");
    static private readonly Verb CLAW = new Verb("claw");
    static private readonly Verb PUNCH = new Verb("punch", "punches");

    public readonly Gameplay.GameActors.IDs ID;
    public readonly string? ImageID;
    public readonly DollBody DollBody;
    public readonly string Name;
    public readonly string PluralName;
    public readonly Abilities Abilities;
    public readonly Type DefaultController;
    public readonly int ScoreValue;
    public readonly string FlavorDescription;
    // migrated from ActorSheet
    public readonly int BaseHitPoints;
    public readonly int BaseStaminaPoints;
    public readonly int BaseFoodPoints;
    public readonly int BaseSleepPoints;
    public readonly int BaseSanity;
    public readonly Attack UnarmedAttack;
    public readonly Defence BaseDefence;
    public readonly int BaseViewRange;
    public readonly int BaseAudioRange;
    public readonly float BaseSmellRating;
    public readonly int BaseInventoryCapacity;

    // backward-compatible aliasing
    private ulong CreatedCount {
      get { return _createdCounts[(int)ID]; }
      set { _createdCounts[(int)ID] = value; }
    }

#region Session save/load assistants
    static public void Load(SerializationInfo info, StreamingContext context)
    {
      var stage = (ulong[]) info.GetValue("GameActors_createdCounts", typeof(ulong[]));
      if ((int)Gameplay.GameActors.IDs._COUNT != stage.Length) throw new InvalidOperationException("need upgrade path for Actor::Load");
      // \todo auto-repair if the incoming count is incorrect
      Array.Copy(stage, _createdCounts, (int)Gameplay.GameActors.IDs._COUNT);
    }

    static public void Save(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("GameActors_createdCounts", _createdCounts);
    }

    // it is a semantic error for the created count to be negative, so we
    // don't need to binary-encode against the possibility of negative values
    static public void Load(Zaimoni.Serialization.DecodeObjects decode)
    {
        var stage = new ulong[(int)Gameplay.GameActors.IDs._COUNT];
        decode.LoadFrom7bit(ref stage);
        if ((int)Gameplay.GameActors.IDs._COUNT != stage.Length) throw new InvalidOperationException("need upgrade path for Actor::Load");
        // \todo auto-repair if the incoming count is incorrect
        Array.Copy(stage, _createdCounts, (int)Gameplay.GameActors.IDs._COUNT);
    }

    static public void Save(Zaimoni.Serialization.EncodeObjects encode) => encode.SaveTo7bit(_createdCounts);
#endregion

    public ActorModel(Gameplay.GameActors.IDs id, string? imageID, string name, string pluralName, int scoreValue, string flavor, DollBody body, Abilities abilities, Gameplay.GameActors.ActorData src, Type defaultController)
    {
#if DEBUG
      // using logical XOR to restate IFF, logical AND to restate logical implication
      if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
      if (string.IsNullOrEmpty(flavor)) throw new ArgumentNullException(nameof(flavor));
      if (!defaultController.IsSubclassOf(typeof(ActorController))) throw new InvalidOperationException("!defaultController.IsSubclassOf(typeof(ActorController))");
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
      Abilities = abilities;
      DefaultController = defaultController;
      ScoreValue = scoreValue;
      FlavorDescription = flavor;
      // migrated from ActorSheet
      BaseHitPoints = src.HP;
      BaseStaminaPoints = src.STA;
      // legacy: both human and dogs are set to this value
      if (abilities.HasToEat) BaseFoodPoints = 2 * Actor.FOOD_HUNGRY_LEVEL;
      else if (abilities.IsRotting) BaseFoodPoints = 2 * Actor.ROT_HUNGRY_LEVEL;
      else BaseFoodPoints = 0;
      // legacy: both human and dogs are set to this value
      if (abilities.HasToSleep) BaseSleepPoints = 2 * Actor.SLEEP_SLEEPY_LEVEL;
      else BaseSleepPoints = 0;

      if (abilities.HasSanity) BaseSanity = 4 * WorldTime.TURNS_PER_DAY;
      else BaseSanity = 0;

      Verb ua() {
        if (typeof(Gameplay.AI.SkeletonAI) == defaultController) return CLAW;
        if (abilities.IsUndead) return BITE;
        if (typeof(Gameplay.AI.FeralDogAI) == defaultController) return BITE;
        return PUNCH;
      }

      UnarmedAttack = new(AttackKind.PHYSICAL, ua(), src.ATK, src.DMG);
      BaseDefence = new(src.DEF, src.PRO_HIT, src.PRO_SHOT);
      BaseViewRange = src.FOV;
      BaseAudioRange = src.AUDIO;
      BaseSmellRating = (float) src.SMELL / 100f;

      if (abilities.HasInventory) {
        if (typeof(Gameplay.AI.FeralDogAI) == defaultController) BaseInventoryCapacity = 1;
        else BaseInventoryCapacity = 7; // humans
      } else BaseInventoryCapacity = 0;
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
