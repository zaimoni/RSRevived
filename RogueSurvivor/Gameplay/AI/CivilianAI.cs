// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.AI.CivilianAI
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace djack.RogueSurvivor.Gameplay.AI
{
  [Serializable]
  internal class CivilianAI : OrderableAI
  {
    private static string[] FIGHT_EMOTES = new string[3]
    {
      "Go away",
      "Damn it I'm trapped!",
      "I'm not afraid"
    };
    private static string[] BIG_BEAR_EMOTES = new string[3]
    {
      "You fool",
      "I'm fooled!",
      "Be a man"
    };
    private static string[] FAMU_FATARU_EMOTES = new string[3]
    {
      "Bakemono",
      "Nani!?",
      "Kawaii"
    };
    private static string[] SANTAMAN_EMOTES = new string[3]
    {
      "DEM BLOODY KIDS!",
      "LEAVE ME ALONE I AIN'T HAVE NO PRESENTS!",
      "MERRY FUCKIN' CHRISTMAS"
    };
    private static string[] ROGUEDJACK_EMOTES = new string[3]
    {
      "Sorry butt I am le busy,",
      "I should have redone ze AI rootines!",
      "Let me test le something on you"
    };
    private static string[] DUCKMAN_EMOTES = new string[3]
    {
      "I'LL QUACK YOU BACK",
      "THIS IS MY FINAL QUACK",
      "I'M GONNA QUACK YOU"
    };
    private static string[] HANS_VON_HANZ_EMOTES = new string[3]
    {
      "RAUS",
      "MEIN FUHRER!",
      "KOMM HIER BITE"
    };
    private const int FOLLOW_NPCLEADER_MAXDIST = 1;
    private const int FOLLOW_PLAYERLEADER_MAXDIST = 1;
    private const int EXPLORATION_MAX_LOCATIONS = WorldTime.TURNS_PER_HOUR;
    private const int EXPLORATION_MAX_ZONES = 3;
    private const int USE_EXIT_CHANCE = 20;
    private const int BUILD_TRAP_CHANCE = 50;
    private const int BUILD_SMALL_FORT_CHANCE = 20;
    private const int BUILD_LARGE_FORT_CHANCE = 50;
    private const int START_FORT_LINE_CHANCE = 1;
    private const int TELL_FRIEND_ABOUT_RAID_CHANCE = 20;
    private const int TELL_FRIEND_ABOUT_ENEMY_CHANCE = 10;
    private const int TELL_FRIEND_ABOUT_ITEMS_CHANCE = 10;
    private const int TELL_FRIEND_ABOUT_SOLDIER_CHANCE = 20;
    private const int MIN_TURNS_SAFE_TO_SLEEP = 10;
    private const int USE_STENCH_KILLER_CHANCE = 75;
    private const int HUNGRY_CHARGE_EMOTE_CHANCE = 50;
    private const int HUNGRY_PUSH_OBJECTS_CHANCE = 25;
    private const int LAW_ENFORCE_CHANCE = 30;
    private const int DONT_LEAVE_BEHIND_EMOTE_CHANCE = 50;
    private LOSSensor m_LOSSensor;
    private int m_SafeTurns;
    private ExplorationData m_Exploration;
    private string[] m_Emotes;

    public override void TakeControl(Actor actor)
    {
      base.TakeControl(actor);
      m_SafeTurns = 0;
      m_Exploration = new ExplorationData(EXPLORATION_MAX_LOCATIONS, EXPLORATION_MAX_ZONES);
      m_LastEnemySaw = null;
      m_LastItemsSaw = null;
      m_LastSoldierSaw = null;
      m_LastRaidHeard = null;
      m_Emotes = null;
    }

    protected override void CreateSensors()
    {
      this.m_LOSSensor = new LOSSensor(LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.ITEMS | LOSSensor.SensingFilter.CORPSES);
    }

    protected override List<Percept> UpdateSensors(RogueGame game)
    {
      if (this.m_Emotes == null)
        this.m_Emotes = !this.m_Actor.IsUnique ? CivilianAI.FIGHT_EMOTES : (this.m_Actor != game.Session.UniqueActors.BigBear.TheActor ? (this.m_Actor != game.Session.UniqueActors.FamuFataru.TheActor ? (this.m_Actor != game.Session.UniqueActors.Santaman.TheActor ? (this.m_Actor != game.Session.UniqueActors.Roguedjack.TheActor ? (this.m_Actor != game.Session.UniqueActors.Duckman.TheActor ? (this.m_Actor != game.Session.UniqueActors.HansVonHanz.TheActor ? CivilianAI.FIGHT_EMOTES : CivilianAI.HANS_VON_HANZ_EMOTES) : CivilianAI.DUCKMAN_EMOTES) : CivilianAI.ROGUEDJACK_EMOTES) : CivilianAI.SANTAMAN_EMOTES) : CivilianAI.FAMU_FATARU_EMOTES) : CivilianAI.BIG_BEAR_EMOTES);
      return this.m_LOSSensor.Sense(game, this.m_Actor);
    }

    protected override ActorAction SelectAction(RogueGame game, List<Percept> percepts)
    {
      List<Percept> percepts1 = FilterSameMap(percepts);
      
      // OrderableAI specific: respond to orders
      if (null != Order)
      {
        ActorAction actorAction = ExecuteOrder(game, Order, percepts1);
        if (null != actorAction)
          {
          m_Actor.Activity = Activity.FOLLOWING_ORDER;
          return actorAction;
          }

        SetOrder(null);
      }
      m_Actor.IsRunning = false;

      m_Exploration.Update(m_Actor.Location);

      List<Percept> perceptList1 = this.FilterEnemies(game, percepts1);
      bool flag1 = perceptList1 != null;
      bool flag2 = this.m_Actor.HasLeader && !this.DontFollowLeader;
      bool hasVisibleLeader = flag2 && this.m_LOSSensor.FOV.Contains(this.m_Actor.Leader.Location.Position);
      bool isLeaderFighting = flag2 && this.IsAdjacentToEnemy(game, this.m_Actor.Leader);
      bool flag3 = flag2 && hasVisibleLeader && isLeaderFighting && !game.Rules.IsActorTired(this.m_Actor);

      // civilians track how long since they've seen trouble
      if (flag1)
        m_SafeTurns = 0;
      else
        ++m_SafeTurns;

      Location location;
      if (m_Actor.Location.Map.LocalTime.TurnCounter % WorldTime.TURNS_PER_HOUR != 0)
      {
        if (PrevLocation.Map == m_Actor.Location.Map)
          goto label_10;
      }
      ClearTabooTiles();
label_10:
      if (m_Actor.Location.Map.LocalTime.TurnCounter % WorldTime.TURNS_PER_DAY == 0)
        this.ClearTabooTrades();

      if (null != perceptList1)
        m_LastEnemySaw = perceptList1[game.Rules.Roll(0, perceptList1.Count)];

      ActorAction tmpAction = this.BehaviorFleeFromExplosives(game, this.FilterStacks(game, percepts1));
      if (null != tmpAction)
      {
        m_Actor.Activity = Activity.FLEEING_FROM_EXPLOSIVE;
        return tmpAction;
      }

      if (!Directives.CanThrowGrenades)
      {
        ItemGrenade itemGrenade = this.m_Actor.GetEquippedWeapon() as ItemGrenade;
        if (itemGrenade != null)
        {
          m_Actor.Activity = Activity.IDLE;
          return (ActorAction) new ActionUnequipItem(this.m_Actor, game, (Item)itemGrenade);
        }
      }
      else if (null != perceptList1)
      {
        tmpAction = BehaviorThrowGrenade(game, m_LOSSensor.FOV, perceptList1);
        if (null != tmpAction) return tmpAction;
      }

      tmpAction = BehaviorEquipWeapon(game);
      if (null != tmpAction)
      {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }
      tmpAction = BehaviorEquipBodyArmor(game);
      if (null != tmpAction)
      {
        m_Actor.Activity = Activity.IDLE;
        return tmpAction;
      }

      // all free actions must be above the enemies check
      if (flag1 && this.Directives.CanFireWeapons && this.m_Actor.GetEquippedWeapon() is ItemRangedWeapon)
      {
        List<Percept> percepts2 = this.FilterFireTargets(game, perceptList1);
        if (percepts2 != null)
        {
          Percept percept = this.FilterNearest(game, percepts2);
          Actor actor = percept.Percepted as Actor;
          Rules rules = game.Rules;
          location = percept.Location;
          Point position1 = location.Position;
          location = this.m_Actor.Location;
          Point position2 = location.Position;
          if (rules.GridDistance(position1, position2) == 1 && !this.HasEquipedRangedWeapon(actor) && this.HasSpeedAdvantage(game, this.m_Actor, actor))
          {
            ActorAction actorAction2 = this.BehaviorWalkAwayFrom(game, percept);
            if (actorAction2 != null)
            {
              this.RunIfPossible(game.Rules);
              this.m_Actor.Activity = Activity.FLEEING;
              return actorAction2;
            }
          }
          ActorAction actorAction5 = this.BehaviorRangedAttack(game, percept);
          if (actorAction5 != null)
          {
            this.m_Actor.Activity = Activity.FIGHTING;
            this.m_Actor.TargetActor = actor;
            return actorAction5;
          }
        }
      }
      if (flag1)
      {
        if (game.Rules.RollChance(50))
        {
          List<Percept> friends = this.FilterNonEnemies(game, percepts1);
          if (friends != null)
          {
            ActorAction actorAction2 = this.BehaviorWarnFriends(game, friends, this.FilterNearest(game, perceptList1).Percepted as Actor);
            if (actorAction2 != null)
            {
              this.m_Actor.Activity = Activity.IDLE;
              return actorAction2;
            }
          }
        }
        ActorAction actorAction5 = this.BehaviorFightOrFlee(game, perceptList1, hasVisibleLeader, isLeaderFighting, this.Directives.Courage, this.m_Emotes);
        if (actorAction5 != null)
          return actorAction5;
      }
      ActorAction actorAction6 = this.BehaviorUseMedecine(game, 2, 1, 2, 4, 2);
      if (actorAction6 != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return actorAction6;
      }
      if (this.BehaviorRestIfTired(game) != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return (ActorAction) new ActionWait(this.m_Actor, game);
      }
      if (flag1 && flag3)
      {
        Percept target = this.FilterNearest(game, perceptList1);
        ActorAction actorAction2 = this.BehaviorChargeEnemy(game, target);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.FIGHTING;
          this.m_Actor.TargetActor = target.Percepted as Actor;
          return actorAction2;
        }
      }
      if (game.Rules.IsActorHungry(this.m_Actor))
      {
        ActorAction actorAction2 = this.BehaviorEat(game);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
        if (game.Rules.IsActorStarving(this.m_Actor) || game.Rules.IsActorInsane(this.m_Actor))
        {
          ActorAction actorAction5 = this.BehaviorGoEatCorpse(game, this.FilterCorpses(game, percepts1));
          if (actorAction5 != null)
          {
            this.m_Actor.Activity = Activity.IDLE;
            return actorAction5;
          }
        }
      }
      if (this.m_SafeTurns >= MIN_TURNS_SAFE_TO_SLEEP && this.Directives.CanSleep && (this.WouldLikeToSleep(game, this.m_Actor) && this.IsInside(this.m_Actor)) && game.Rules.CanActorSleep(this.m_Actor))
      {
        ActorAction actorAction2 = this.BehaviorSecurePerimeter(game, this.m_LOSSensor.FOV);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
        ActorAction actorAction5 = this.BehaviorSleep(game, this.m_LOSSensor.FOV);
        if (actorAction5 != null)
        {
          if (actorAction5 is ActionSleep)
            this.m_Actor.Activity = Activity.SLEEPING;
          return actorAction5;
        }
      }
      ActorAction actorAction7 = this.BehaviorDropUselessItem(game);
      if (actorAction7 != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return actorAction7;
      }
      bool flag4 = this.m_Actor.HasLeader || this.m_Actor.CountFollowers > 0;
      bool flag5 = this.NeedsLight(game);
      RogueGame game1 = game;
      location = this.m_Actor.Location;
      Map map3 = location.Map;
      location = this.m_Actor.Location;
      Point position3 = location.Position;
      bool flag6 = this.IsGoodStenchKillerSpot(game1, map3, position3);
      if (!flag4 && !flag5 && !flag6)
      {
        ActorAction actorAction2 = this.BehaviorUnequipLeftItem(game);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
      }
      if (flag4)
      {
        ActorAction actorAction2 = this.BehaviorEquipCellPhone(game);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
      }
      else if (flag5)
      {
        ActorAction actorAction2 = this.BehaviorEquipLight(game);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
      }
      else if (flag6)
      {
        ActorAction actorAction2 = this.BehaviorEquipStenchKiller(game);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
      }
      if (!flag1 && this.Directives.CanTakeItems)
      {
        location = this.m_Actor.Location;
        Map map = location.Map;
        List<Percept> perceptList2 = this.FilterOut(game, this.FilterStacks(game, percepts1), (Predicate<Percept>) (p =>
        {
          if (p.Turn == map.LocalTime.TurnCounter && !this.IsOccupiedByOther(map, p.Location.Position) && !this.IsTileTaboo(p.Location.Position))
            return !this.HasAnyInterestingItem(game, p.Percepted as Inventory);
          return true;
        }));
        if (perceptList2 != null)
        {
          Percept percept = this.FilterNearest(game, perceptList2);
          this.m_LastItemsSaw = percept;
          ActorAction actorAction2 = this.BehaviorMakeRoomForFood(game, perceptList2);
          if (actorAction2 != null)
          {
            this.m_Actor.Activity = Activity.IDLE;
            return actorAction2;
          }
          RogueGame game2 = game;
          location = percept.Location;
          Point position1 = location.Position;
          Inventory stack = percept.Percepted as Inventory;
          ActorAction actorAction5 = this.BehaviorGrabFromStack(game2, position1, stack);
          if (actorAction5 != null)
          {
            this.m_Actor.Activity = Activity.IDLE;
            return actorAction5;
          }
          location = percept.Location;
          this.MarkTileAsTaboo(location.Position);
          game.DoEmote(this.m_Actor, "Mmmh. Looks like I can't reach what I want.");
        }
        if (this.Directives.CanTrade)
        {
          List<Percept> percepts2 = this.FilterOut(game, this.FilterNonEnemies(game, percepts1), (Predicate<Percept>) (p =>
          {
            if (p.Turn != map.LocalTime.TurnCounter)
              return true;
            Actor actor = p.Percepted as Actor;
            return actor.IsPlayer || !game.Rules.CanActorInitiateTradeWith(this.m_Actor, actor) || (this.IsActorTabooTrade(actor) || !this.HasAnyInterestingItem(game, actor.Inventory)) || !(actor.Controller as BaseAI).HasAnyInterestingItem(game, this.m_Actor.Inventory);
          }));
          if (percepts2 != null)
          {
            Actor actor = this.FilterNearest(game, percepts2).Percepted as Actor;
            if (game.Rules.IsAdjacent(this.m_Actor.Location, actor.Location))
            {
              ActorAction actorAction2 = (ActorAction) new ActionTrade(this.m_Actor, game, actor);
              if (actorAction2.IsLegal())
              {
                this.MarkActorAsRecentTrade(actor);
                game.DoSay(this.m_Actor, actor, string.Format("Hey {0}, let's make a deal!", (object) actor.Name), RogueGame.Sayflags.NONE);
                return actorAction2;
              }
            }
            else
            {
              RogueGame game2 = game;
              location = actor.Location;
              Point position1 = location.Position;
              ActorAction actorAction2 = this.BehaviorIntelligentBumpToward(game2, position1);
              if (actorAction2 != null)
              {
                this.m_Actor.Activity = Activity.FOLLOWING;
                this.m_Actor.TargetActor = actor;
                return actorAction2;
              }
            }
          }
        }
      }
      if (RogueGame.Options.IsAggressiveHungryCiviliansOn && percepts1 != null && (!this.m_Actor.HasLeader && !this.m_Actor.Model.Abilities.IsLawEnforcer) && (game.Rules.IsActorHungry(this.m_Actor) && this.HasNoFoodItems(this.m_Actor)))
      {
        Percept target = this.FilterNearest(game, FilterActors(percepts1, (Predicate<Actor>) (a =>
        {
          if (a == this.m_Actor || a.IsDead || (a.Inventory == null || a.Inventory.IsEmpty) || (a.Leader == this.m_Actor || this.m_Actor.Leader == a))
            return false;
          if (a.Inventory.HasItemOfType(typeof (ItemFood)))
            return true;
          Inventory itemsAt = a.Location.Map.GetItemsAt(a.Location.Position);
          if (itemsAt == null || itemsAt.IsEmpty)
            return false;
          return itemsAt.HasItemOfType(typeof (ItemFood));
        })));
        if (target != null)
        {
          ActorAction actorAction2 = this.BehaviorChargeEnemy(game, target);
          if (actorAction2 != null)
          {
            if (game.Rules.RollChance(HUNGRY_CHARGE_EMOTE_CHANCE))
              game.DoSay(this.m_Actor, target.Percepted as Actor, "HEY! YOU! SHARE SOME FOOD!", RogueGame.Sayflags.IS_FREE_ACTION);
            this.m_Actor.Activity = Activity.FIGHTING;
            this.m_Actor.TargetActor = target.Percepted as Actor;
            return actorAction2;
          }
        }
      }
      if (game.Rules.RollChance(USE_STENCH_KILLER_CHANCE))
      {
        ActorAction actorAction2 = this.BehaviorUseStenchKiller(game);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
      }
      ActorAction actorAction8 = this.BehaviorCloseDoorBehindMe(game, this.PrevLocation);
      if (actorAction8 != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return actorAction8;
      }
      if (this.m_Actor.Model.Abilities.HasSanity)
      {
        if ((double) this.m_Actor.Sanity < 0.75 * (double) game.Rules.ActorMaxSanity(this.m_Actor))
        {
          ActorAction actorAction2 = this.BehaviorUseEntertainment(game);
          if (actorAction2 != null)
          {
            this.m_Actor.Activity = Activity.IDLE;
            return actorAction2;
          }
        }
        ActorAction actorAction5 = this.BehaviorDropBoringEntertainment(game);
        if (actorAction5 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction5;
        }
      }
      if (flag2)
      {
        Point position1 = this.m_Actor.Leader.Location.Position;
        bool isVisible = this.m_LOSSensor.FOV.Contains(position1);
        int maxDist = this.m_Actor.Leader.IsPlayer ? FOLLOW_PLAYERLEADER_MAXDIST : FOLLOW_NPCLEADER_MAXDIST;
        ActorAction actorAction2 = this.BehaviorFollowActor(game, this.m_Actor.Leader, position1, isVisible, maxDist);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.FOLLOWING;
          this.m_Actor.TargetActor = this.m_Actor.Leader;
          return actorAction2;
        }
      }
      if (this.m_Actor.Sheet.SkillTable.GetSkillLevel(9) >= 1 && (!flag2 && this.m_Actor.CountFollowers < game.Rules.ActorMaxFollowers(this.m_Actor)))
      {
        Percept target = this.FilterNearest(game, this.FilterNonEnemies(game, percepts1));
        if (target != null)
        {
          ActorAction actorAction2 = this.BehaviorLeadActor(game, target);
          if (actorAction2 != null)
          {
            this.m_Actor.Activity = Activity.IDLE;
            this.m_Actor.TargetActor = target.Percepted as Actor;
            return actorAction2;
          }
        }
      }
      if (game.Rules.IsActorHungry(this.m_Actor))
      {
        ActorAction actorAction2 = this.BehaviorAttackBarricade(game);
        if (actorAction2 != null)
        {
          game.DoEmote(this.m_Actor, "Open damn it! I know there is food there!");
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
        if (game.Rules.RollChance(HUNGRY_PUSH_OBJECTS_CHANCE))
        {
          ActorAction actorAction5 = this.BehaviorPushNonWalkableObject(game);
          if (actorAction5 != null)
          {
            game.DoEmote(this.m_Actor, "Where is all the damn food?!");
            this.m_Actor.Activity = Activity.IDLE;
            return actorAction5;
          }
        }
      }
      ActorAction actorAction9 = this.BehaviorGoReviveCorpse(game, this.FilterCorpses(game, percepts1));
      if (actorAction9 != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return actorAction9;
      }
      if (game.Rules.RollChance(USE_EXIT_CHANCE))
      {
        ActorAction actorAction2 = this.BehaviorUseExit(game, BaseAI.UseExitFlags.DONT_BACKTRACK);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
      }
      if (game.Rules.RollChance(BUILD_TRAP_CHANCE))
      {
        ActorAction actorAction2 = this.BehaviorBuildTrap(game);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
      }
      if (game.Rules.RollChance(BUILD_LARGE_FORT_CHANCE))
      {
        ActorAction actorAction2 = this.BehaviorBuildLargeFortification(game, 1);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
      }
      if (game.Rules.RollChance(BUILD_SMALL_FORT_CHANCE))
      {
        ActorAction actorAction2 = this.BehaviorBuildSmallFortification(game);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
      }
      if (this.m_LastRaidHeard != null && game.Rules.RollChance(TELL_FRIEND_ABOUT_RAID_CHANCE))
      {
        ActorAction actorAction2 = this.BehaviorTellFriendAboutPercept(game, this.m_LastRaidHeard);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
      }
      Percept percept1 = this.FilterFirst(game, percepts1, (Predicate<Percept>) (p =>
      {
        Actor actor = p.Percepted as Actor;
        if (actor == null || actor == this.m_Actor)
          return false;
        return this.IsSoldier(actor);
      }));
      if (percept1 != null)
        this.m_LastSoldierSaw = percept1;
      if (game.Rules.RollChance(TELL_FRIEND_ABOUT_SOLDIER_CHANCE) && this.m_LastSoldierSaw != null)
      {
        ActorAction actorAction2 = this.BehaviorTellFriendAboutPercept(game, this.m_LastSoldierSaw);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
      }
      if (game.Rules.RollChance(TELL_FRIEND_ABOUT_ENEMY_CHANCE) && this.m_LastEnemySaw != null)
      {
        ActorAction actorAction2 = this.BehaviorTellFriendAboutPercept(game, this.m_LastEnemySaw);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
      }
      if (game.Rules.RollChance(TELL_FRIEND_ABOUT_ITEMS_CHANCE) && this.m_LastItemsSaw != null)
      {
        ActorAction actorAction2 = this.BehaviorTellFriendAboutPercept(game, this.m_LastItemsSaw);
        if (actorAction2 != null)
        {
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
      }
      if (m_Actor.Model.Abilities.IsLawEnforcer && percepts1 != null && game.Rules.RollChance(LAW_ENFORCE_CHANCE))
      {
        Actor target;
        ActorAction actorAction2 = this.BehaviorEnforceLaw(game, percepts1, out target);
        if (actorAction2 != null)
        {
          this.m_Actor.TargetActor = target;
          return actorAction2;
        }
      }
      if (this.m_Actor.CountFollowers > 0)
      {
        Actor target;
        ActorAction actorAction2 = this.BehaviorDontLeaveFollowersBehind(game, 2, out target);
        if (actorAction2 != null)
        {
          if (game.Rules.RollChance(DONT_LEAVE_BEHIND_EMOTE_CHANCE))
          {
            if (target.IsSleeping)
            {
              game.DoEmote(this.m_Actor, string.Format("patiently waits for {0} to wake up.", (object) target.Name));
            }
            else
            {
              if (this.m_LOSSensor.FOV.Contains(target.Location.Position))
                game.DoEmote(this.m_Actor, string.Format("Come on {0}! Hurry up!", (object) target.Name));
              else
                game.DoEmote(this.m_Actor, string.Format("Where the hell is {0}?", (object) target.Name));
            }
          }
          this.m_Actor.Activity = Activity.IDLE;
          return actorAction2;
        }
      }
      ActorAction actorAction10 = this.BehaviorExplore(game, this.m_Exploration);
      if (actorAction10 != null)
      {
        this.m_Actor.Activity = Activity.IDLE;
        return actorAction10;
      }
      this.m_Actor.Activity = Activity.IDLE;
      return this.BehaviorWander(game);
    }
  }
}
