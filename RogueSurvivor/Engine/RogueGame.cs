// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.RogueGame
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Engine.MapObjects;
using djack.RogueSurvivor.Engine.Tasks;
using djack.RogueSurvivor.Gameplay;
using djack.RogueSurvivor.Gameplay.AI;
using djack.RogueSurvivor.Gameplay.Generators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Security.Permissions;

namespace djack.RogueSurvivor.Engine
{
  internal class RogueGame
  {
    private readonly Color POPUP_FILLCOLOR = Color.FromArgb(192, Color.CornflowerBlue);
    private readonly string[] CLOSE_DOOR_MODE_TEXT = new string[1]
    {
      "CLOSE MODE - directions to close, ESC cancels"
    };
    private readonly string[] BARRICADE_MODE_TEXT = new string[1]
    {
      "BARRICADE/REPAIR MODE - directions to barricade/repair, ESC cancels"
    };
    private readonly string[] BREAK_MODE_TEXT = new string[1]
    {
      "BREAK MODE - directions/wait to break an object, ESC cancels"
    };
    private readonly string[] BUILD_LARGE_FORT_MODE_TEXT = new string[1]
    {
      "BUILD LARGE FORTIFICATION MODE - directions to build, ESC cancels"
    };
    private readonly string[] BUILD_SMALL_FORT_MODE_TEXT = new string[1]
    {
      "BUILD SMALL FORTIFICATION MODE - directions to build, ESC cancels"
    };
    private readonly string[] TRADE_MODE_TEXT = new string[1]
    {
      "TRADE MODE - Y to accept the deal, N to refuse"
    };
    private readonly string[] INITIATE_TRADE_MODE_TEXT = new string[1]
    {
      "INITIATE TRADE MODE - directions to offer item to someone, ESC cancels"
    };
    private readonly string[] UPGRADE_MODE_TEXT = new string[1]
    {
      "UPGRADE MODE - follow instructions in the message panel"
    };
    private readonly string[] FIRE_MODE_TEXT = new string[1]
    {
      "FIRE MODE - F to fire, T next target, M toggle mode, ESC cancels"
    };
    private readonly string[] SWITCH_PLACE_MODE_TEXT = new string[1]
    {
      "SWITCH PLACE MODE - directions to switch place with a follower, ESC cancels"
    };
    private readonly string[] TAKE_LEAD_MODE_TEXT = new string[1]
    {
      "TAKE LEAD MODE - directions to recruit a follower, ESC cancels"
    };
    private readonly string[] PUSH_MODE_TEXT = new string[1]
    {
      "PUSH/SHOVE MODE - directions to push/shove, ESC cancels"
    };
    private readonly string[] TAG_MODE_TEXT = new string[1]
    {
      "TAG MODE - directions to tag a wall or on the floor, ESC cancels"
    };
    private readonly string PUSH_OBJECT_MODE_TEXT = "PUSHING {0} - directions to push, ESC cancels";
    private readonly string SHOVE_ACTOR_MODE_TEXT = "SHOVING {0} - directions to shove, ESC cancels";
    private readonly string[] ORDER_MODE_TEXT = new string[1]
    {
      "ORDER MODE - follow instructions in the message panel, ESC cancels"
    };
    private readonly string[] GIVE_MODE_TEXT = new string[1]
    {
      "GIVE MODE - directions to give item to someone, ESC cancels"
    };
    private readonly string[] THROW_GRENADE_MODE_TEXT = new string[1]
    {
      "THROW GRENADE MODE - directions to select, F to fire,  ESC cancels"
    };
    private readonly string[] MARK_ENEMIES_MODE = new string[1]
    {
      "MARK ENEMIES MODE - E to make enemy, T next actor, ESC cancels"
    };
    private readonly Color MODE_TEXTCOLOR = Color.Yellow;
    private readonly Color MODE_BORDERCOLOR = Color.Yellow;
    private readonly Color MODE_FILLCOLOR = Color.FromArgb(192, Color.Gray);
    private readonly Color PLAYER_ACTION_COLOR = Color.White;
    private readonly Color OTHER_ACTION_COLOR = Color.Gray;
    private readonly Color SAYOREMOTE_COLOR = Color.Brown;
    private readonly Color PLAYER_AUDIO_COLOR = Color.Green;
    private readonly Color NIGHT_COLOR = Color.Cyan;
    private readonly Color DAY_COLOR = Color.Gold;
    private readonly Color TINT_DAY = Color.White;
    private readonly Color TINT_SUNSET = Color.FromArgb(235, 235, 235);
    private readonly Color TINT_EVENING = Color.FromArgb(215, 215, 215);
    private readonly Color TINT_MIDNIGHT = Color.FromArgb(195, 195, 195);
    private readonly Color TINT_NIGHT = Color.FromArgb(205, 205, 205);
    private readonly Color TINT_SUNRISE = Color.FromArgb(225, 225, 225);
    private readonly Verb VERB_ACCEPT_THE_DEAL = new Verb("accept the deal", "accepts the deal");
    private readonly Verb VERB_ACTIVATE = new Verb("activate");
    private readonly Verb VERB_AVOID = new Verb("avoid");
    private readonly Verb VERB_BARRICADE = new Verb("barricade");
    private readonly Verb VERB_BASH = new Verb("bash", "bashes");
    private readonly Verb VERB_BE = new Verb("are", "is");
    private readonly Verb VERB_BUILD = new Verb("build");
    private readonly Verb VERB_BREAK = new Verb("break");
    private readonly Verb VERB_BUTCHER = new Verb("butcher");
    private readonly Verb VERB_CATCH = new Verb("catch", "catches");
    private readonly Verb VERB_CHAT_WITH = new Verb("chat with", "chats with");
    private readonly Verb VERB_CLOSE = new Verb("close");
    private readonly Verb VERB_COLLAPSE = new Verb("collapse");
    private readonly Verb VERB_CRUSH = new Verb("crush", "crushes");
    private readonly Verb VERB_DESACTIVATE = new Verb("desactivate");
    private readonly Verb VERB_DESTROY = new Verb("destroy");
    private readonly Verb VERB_DIE = new Verb("die");
    private readonly Verb VERB_DIE_FROM_STARVATION = new Verb("die from starvation", "dies from starvation");
    private readonly Verb VERB_DISCARD = new Verb("discard");
    private readonly Verb VERB_DRAG = new Verb("drag");
    private readonly Verb VERB_DROP = new Verb("drop");
    private readonly Verb VERB_EAT = new Verb("eat");
    private readonly Verb VERB_ENJOY = new Verb("enjoy");
    private readonly Verb VERB_ENTER = new Verb("enter");
    private readonly Verb VERB_ESCAPE = new Verb("escape");
    private readonly Verb VERB_FAIL = new Verb("fail");
    private readonly Verb VERB_FEAST_ON = new Verb("feast on", "feasts on");
    private readonly Verb VERB_FEEL = new Verb("feel");
    private readonly Verb VERB_GIVE = new Verb("give");
    private readonly Verb VERB_GRAB = new Verb("grab");
    private readonly Verb VERB_EQUIP = new Verb("equip");
    private readonly Verb VERB_HAVE = new Verb("have", "has");
    private readonly Verb VERB_HELP = new Verb("help");
    private readonly Verb VERB_PERSUADE = new Verb("persuade");
    private readonly Verb VERB_HEAL_WITH = new Verb("heal with", "heals with");
    private readonly Verb VERB_JUMP_ON = new Verb("jump on", "jumps on");
    private readonly Verb VERB_KILL = new Verb("kill");
    private readonly Verb VERB_LEAVE = new Verb("leave");
    private readonly Verb VERB_MISS = new Verb("miss", "misses");
    private readonly Verb VERB_MURDER = new Verb("murder");
    private readonly Verb VERB_OFFER = new Verb("offer");
    private readonly Verb VERB_OPEN = new Verb("open");
    private readonly Verb VERB_ORDER = new Verb("order");
    private readonly Verb VERB_PUSH = new Verb("push", "pushes");
    private readonly Verb VERB_RAISE_ALARM = new Verb("raise the alarm", "raises the alarm");
    private readonly Verb VERB_REFUSE_THE_DEAL = new Verb("refuse the deal", "refuses the deal");
    private readonly Verb VERB_RELOAD = new Verb("reload");
    private readonly Verb VERB_RECHARGE = new Verb("recharge");
    private readonly Verb VERB_REPAIR = new Verb("repair");
    private readonly Verb VERB_REVIVE = new Verb("revive");
    private readonly Verb VERB_SEE = new Verb("see");
    private readonly Verb VERB_SHOUT = new Verb("shout");
    private readonly Verb VERB_SHOVE = new Verb("shove");
    private readonly Verb VERB_SNORE = new Verb("snore");
    private readonly Verb VERB_SPRAY = new Verb("spray");
    private readonly Verb VERB_START = new Verb("start");
    private readonly Verb VERB_STOP = new Verb("stop");
    private readonly Verb VERB_STUMBLE = new Verb("stumble");
    private readonly Verb VERB_SWITCH = new Verb("switch", "switches");
    private readonly Verb VERB_SWITCH_PLACE_WITH = new Verb("switch place with", "switches place with");
    private readonly Verb VERB_TAKE = new Verb("take");
    private readonly Verb VERB_THROW = new Verb("throw");
    private readonly Verb VERB_TRANSFORM_INTO = new Verb("transform into", "transforms into");
    private readonly Verb VERB_UNEQUIP = new Verb("unequip");
    private readonly Verb VERB_VOMIT = new Verb("vomit");
    private readonly Verb VERB_WAIT = new Verb("wait");
    private readonly Verb VERB_WAKE_UP = new Verb("wake up", "wakes up");
    private bool m_IsGameRunning = true;
    private List<RogueGame.Overlay> m_Overlays = new List<RogueGame.Overlay>();
    private HashSet<Point> m_PlayerFOV = new HashSet<Point>();
    private object m_SimMutex = new object();
    public const int MAP_MAX_HEIGHT = 100;
    public const int MAP_MAX_WIDTH = 100;
    public const int TILE_SIZE = 32;
    public const int ACTOR_SIZE = 32;
    public const int ACTOR_OFFSET = 0;
    public const int TILE_VIEW_WIDTH = 21;
    public const int TILE_VIEW_HEIGHT = 21;
    private const int HALF_VIEW_WIDTH = 10;
    private const int HALF_VIEW_HEIGHT = 10;
    public const int CANVAS_WIDTH = 1024;
    public const int CANVAS_HEIGHT = 768;
    private const int DAMAGE_DX = 10;
    private const int DAMAGE_DY = 10;
    private const int RIGHTPANEL_X = 676;
    private const int RIGHTPANEL_Y = 0;
    private const int RIGHTPANEL_TEXT_X = 680;
    private const int RIGHTPANEL_TEXT_Y = 4;
    private const int INVENTORYPANEL_X = 680;
    private const int INVENTORYPANEL_Y = 160;
    private const int GROUNDINVENTORYPANEL_Y = 224;
    private const int CORPSESPANEL_Y = 288;
    private const int INVENTORY_SLOTS_PER_LINE = 10;
    private const int SKILLTABLE_Y = 352;
    private const int SKILLTABLE_LINES = 10;
    private const int LOCATIONPANEL_X = 676;
    private const int LOCATIONPANEL_Y = 676;
    private const int LOCATIONPANEL_TEXT_X = 680;
    private const int LOCATIONPANEL_TEXT_Y = 680;
    private const int MESSAGES_X = 4;
    private const int MESSAGES_Y = 676;
    private const int MESSAGES_SPACING = 12;
    private const int MESSAGES_FADEOUT = 25;
    private const int MAX_MESSAGES = 7;
    private const int MESSAGES_HISTORY = 59;
    public const int MINITILE_SIZE = 2;
    private const int MINIMAP_X = 750;
    private const int MINIMAP_Y = 475;
    private const int MINI_TRACKER_OFFSET = 1;
    private const int DELAY_SHORT = 250;
    private const int DELAY_NORMAL = 500;
    private const int DELAY_LONG = 1000;
    private const int LINE_SPACING = 12;
    private const int BOLD_LINE_SPACING = 14;
    private const int CREDIT_CHAR_SPACING = 8;
    private const int CREDIT_LINE_SPACING = 12;
    private const int TEXTFILE_CHARS_PER_LINE = 120;
    private const int TEXTFILE_LINES_PER_PAGE = 50;
    public const string NAME_SUBWAY_STATION = "Subway Station";
    public const string NAME_SEWERS_MAINTENANCE = "Sewers Maintenance";
    public const string NAME_SUBWAY_RAILS = "rails";
    public const string NAME_POLICE_STATION_JAILS_CELL = "jail";
    private const int SPAWN_DISTANCE_TO_PLAYER = 10;
    private const int SEWERS_INVASION_CHANCE = 1;
    public const float SEWERS_UNDEADS_FACTOR = 0.5f;
    private const float REFUGEES_WAVE_SIZE = 0.2f;
    private const int REFUGEES_WAVE_ITEMS = 3;
    private const int REFUGEE_SURFACE_SPAWN_CHANCE = 80;
    private const int UNIQUE_REFUGEE_CHECK_CHANCE = 10;
    public const int NATGUARD_DAY = 3;
    private const int NATGUARD_END_DAY = 10;
    private const int NATGUARD_ZTRACKER_DAY = 6;
    private const int NATGUARD_SQUAD_SIZE = 5;
    private const double NATGUARD_INTERVENTION_FACTOR = 5.0;
    private const int NATGUARD_INTERVENTION_CHANCE = 1;
    private const int ARMY_SUPPLIES_DAY = 4;
    private const float ARMY_SUPPLIES_FACTOR = 288f;
    private const int ARMY_SUPPLIES_CHANCE = 2;
    private const int ARMY_SUPPLIES_SCATTER = 1;
    public const int BIKERS_RAID_DAY = 2;
    private const int BIKERS_END_DAY = 14;
    private const int BIKERS_RAID_SIZE = 6;
    private const int BIKERS_RAID_CHANCE_PER_TURN = 1;
    private const int BIKERS_RAID_DAYS_GAP = 2;
    public const int GANGSTAS_RAID_DAY = 7;
    private const int GANGSTAS_END_DAY = 21;
    private const int GANGSTAS_RAID_SIZE = 6;
    private const int GANGSTAS_RAID_CHANCE_PER_TURN = 1;
    private const int GANGSTAS_RAID_DAYS_GAP = 3;
    private const int BLACKOPS_RAID_DAY = 14;
    private const int BLACKOPS_RAID_SIZE = 3;
    private const int BLACKOPS_RAID_CHANCE_PER_TURN = 1;
    private const int BLACKOPS_RAID_DAY_GAP = 5;
    private const int SURVIVORS_BAND_DAY = 21;
    private const int SURVIVORS_BAND_SIZE = 5;
    private const int SURVIVORS_BAND_CHANCE_PER_TURN = 1;
    private const int SURVIVORS_BAND_DAY_GAP = 5;
    private const int ZOMBIE_LORD_EVOLUTION_MIN_DAY = 7;
    private const int DISCIPLE_EVOLUTION_MIN_DAY = 7;
    private const int PLAYER_HEAR_FIGHT_CHANCE = 25;
    private const int PLAYER_HEAR_SCREAMS_CHANCE = 10;
    private const int PLAYER_HEAR_PUSH_CHANCE = 25;
    private const int PLAYER_HEAR_BASH_CHANCE = 25;
    private const int PLAYER_HEAR_BREAK_CHANCE = 50;
    private const int PLAYER_HEAR_EXPLOSION_CHANCE = 100;
    private const int BLOOD_WALL_SPLAT_CHANCE = 20;
    public const int MESSAGE_NPC_SLEEP_SNORE_CHANCE = 10;
    private const int WEATHER_CHANGE_CHANCE = 33;
    private const int DISTRICT_EXIT_CHANCE_PER_TILE = 15;
    private readonly IRogueUI m_UI;
    private Rules m_Rules;
    private Session m_Session;
    private HiScoreTable m_HiScoreTable;
    private MessageManager m_MessageManager;
    private bool m_HasLoadedGame;
    private Actor m_Player;
    private Rectangle m_MapViewRect;
    private static GameOptions s_Options;
    private static Keybindings s_KeyBindings;
    private static GameHintsStatus s_Hints;
    private BaseTownGenerator m_TownGenerator;
    private bool m_PlayedIntro;
    private ISoundManager m_MusicManager;
    private RogueGame.CharGen m_CharGen;
    private TextFile m_Manual;
    private int m_ManualLine;
    private GameFactions m_GameFactions;
    private GameActors m_GameActors;
    private GameItems m_GameItems;
    private GameTiles m_GameTiles;
    private bool m_IsPlayerLongWait;
    private bool m_IsPlayerLongWaitForcedStop;
    private WorldTime m_PlayerLongWaitEnd;
    private Thread m_SimThread;

    public Session Session
    {
      get
      {
        return this.m_Session;
      }
    }

    public Rules Rules
    {
      get
      {
        return this.m_Rules;
      }
    }

    public IRogueUI UI
    {
      get
      {
        return this.m_UI;
      }
    }

    public bool IsGameRunning
    {
      get
      {
        return this.m_IsGameRunning;
      }
      set
      {
        this.m_IsGameRunning = value;
      }
    }

    public static GameOptions Options
    {
      get
      {
        return RogueGame.s_Options;
      }
    }

    public static Keybindings KeyBindings
    {
      get
      {
        return RogueGame.s_KeyBindings;
      }
    }

    public GameFactions GameFactions
    {
      get
      {
        return this.m_GameFactions;
      }
    }

    public GameActors GameActors
    {
      get
      {
        return this.m_GameActors;
      }
    }

    public GameItems GameItems
    {
      get
      {
        return this.m_GameItems;
      }
    }

    public GameTiles GameTiles
    {
      get
      {
        return this.m_GameTiles;
      }
    }

    public Actor Player
    {
      get
      {
        return this.m_Player;
      }
    }

    public RogueGame(IRogueUI UI)
    {
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "RogueGame()");
      this.m_UI = UI;
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating MusicManager");
      switch (SetupConfig.Sound)
      {
//        case SetupConfig.eSound.SOUND_MANAGED_DIRECTX:
//          this.m_MusicManager = (ISoundManager) new MDXSoundManager();
//          break;
//        case SetupConfig.eSound.SOUND_SFML:
//          this.m_MusicManager = (ISoundManager) new SFMLSoundManager();
//          break;
        default:
          this.m_MusicManager = (ISoundManager) new NullSoundManager();
          break;
      }
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating MessageManager");
      this.m_MessageManager = new MessageManager(12, 25, 59);
      this.m_Session = Session.Get;
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating Rules");
      this.m_Rules = new Rules(new DiceRoller(this.m_Session.Seed));
      BaseTownGenerator.Parameters parameters = BaseTownGenerator.DEFAULT_PARAMS;
      parameters.MapWidth = MAP_MAX_WIDTH;
      parameters.MapHeight = RogueGame.MAP_MAX_HEIGHT;
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating Generator");
      this.m_TownGenerator = (BaseTownGenerator) new StdTownGenerator(this, parameters);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating options, keys, hints.");
      RogueGame.s_Options = new GameOptions();
      RogueGame.s_Options.ResetToDefaultValues();
      RogueGame.s_KeyBindings = new Keybindings();
      RogueGame.s_KeyBindings.ResetToDefaults();
      RogueGame.s_Hints = new GameHintsStatus();
      RogueGame.s_Hints.ResetAllHints();
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating dbs");
      this.m_GameFactions = new GameFactions();
      this.m_GameActors = new GameActors();
      this.m_GameItems = new GameItems();
      this.m_GameTiles = new GameTiles();
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "RogueGame() done.");
    }

    public void AddMessage(djack.RogueSurvivor.Data.Message msg)
    {
      if (msg.Text.Length == 0)
        return;
      if (this.m_MessageManager.Count >= 7)
        this.m_MessageManager.Clear();
      msg.Text = string.Format("{0} {1}", (object) this.m_Session.WorldTime.TurnCounter, (object) this.Capitalize(msg.Text));
      this.m_MessageManager.Add(msg);
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public void AddMessageIfAudibleForPlayer(Location location, djack.RogueSurvivor.Data.Message msg)
    {
      if (msg == null)
        throw new ArgumentNullException("msg");
      if (this.m_Player == null || this.m_Player.IsSleeping || location.Map != this.m_Player.Location.Map || (double) this.m_Rules.StdDistance(this.m_Player.Location.Position, location.Position) > (double) this.m_Player.AudioRange)
        return;
      msg.Color = this.PLAYER_AUDIO_COLOR;
      this.AddMessage(msg);
      if (this.m_IsPlayerLongWait)
        this.m_IsPlayerLongWaitForcedStop = true;
      this.RedrawPlayScreen();
    }

    private djack.RogueSurvivor.Data.Message MakePlayerCentricMessage(string eventText, Point position)
    {
      Point v = new Point(position.X - this.m_Player.Location.Position.X, position.Y - this.m_Player.Location.Position.Y);
      return new djack.RogueSurvivor.Data.Message(string.Format("{0} {1} tiles to the {2}.", (object) eventText, (object) (int) this.m_Rules.StdDistance(v), (object) Direction.ApproximateFromVector(v)), this.m_Session.WorldTime.TurnCounter);
    }

    private djack.RogueSurvivor.Data.Message MakeErrorMessage(string text)
    {
      return new djack.RogueSurvivor.Data.Message(text, this.m_Session.WorldTime.TurnCounter, Color.Red);
    }

    private djack.RogueSurvivor.Data.Message MakeYesNoMessage(string question)
    {
      return new djack.RogueSurvivor.Data.Message(string.Format("{0}? Y to confirm, N to cancel", (object) question), this.m_Session.WorldTime.TurnCounter, Color.Yellow);
    }

    private string ActorVisibleIdentity(Actor actor)
    {
      if (!this.IsVisibleToPlayer(actor))
        return "someone";
      return actor.TheName;
    }

    private string ObjectVisibleIdentity(MapObject mapObj)
    {
      if (!this.IsVisibleToPlayer(mapObj))
        return "something";
      return mapObj.TheName;
    }

    private djack.RogueSurvivor.Data.Message MakeMessage(Actor actor, string doWhat)
    {
      return this.MakeMessage(actor, doWhat, this.OTHER_ACTION_COLOR);
    }

    private djack.RogueSurvivor.Data.Message MakeMessage(Actor actor, string doWhat, Color color)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(this.ActorVisibleIdentity(actor));
      stringBuilder.Append(" ");
      stringBuilder.Append(doWhat);
      return new djack.RogueSurvivor.Data.Message(stringBuilder.ToString(), this.m_Session.WorldTime.TurnCounter)
      {
        Color = !actor.IsPlayer ? color : this.PLAYER_ACTION_COLOR
      };
    }

    private djack.RogueSurvivor.Data.Message MakeMessage(Actor actor, string doWhat, Actor target)
    {
      return this.MakeMessage(actor, doWhat, target, ".");
    }

    private djack.RogueSurvivor.Data.Message MakeMessage(Actor actor, string doWhat, Actor target, string phraseEnd)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(this.ActorVisibleIdentity(actor));
      stringBuilder.Append(" ");
      stringBuilder.Append(doWhat);
      stringBuilder.Append(" ");
      stringBuilder.Append(this.ActorVisibleIdentity(target));
      stringBuilder.Append(phraseEnd);
      return new djack.RogueSurvivor.Data.Message(stringBuilder.ToString(), this.m_Session.WorldTime.TurnCounter)
      {
        Color = actor.IsPlayer || target.IsPlayer ? this.PLAYER_ACTION_COLOR : this.OTHER_ACTION_COLOR
      };
    }

    private djack.RogueSurvivor.Data.Message MakeMessage(Actor actor, string doWhat, MapObject target)
    {
      return this.MakeMessage(actor, doWhat, target, ".");
    }

    private djack.RogueSurvivor.Data.Message MakeMessage(Actor actor, string doWhat, MapObject target, string phraseEnd)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(this.ActorVisibleIdentity(actor));
      stringBuilder.Append(" ");
      stringBuilder.Append(doWhat);
      stringBuilder.Append(" ");
      stringBuilder.Append(this.ObjectVisibleIdentity(target));
      stringBuilder.Append(phraseEnd);
      return new djack.RogueSurvivor.Data.Message(stringBuilder.ToString(), this.m_Session.WorldTime.TurnCounter)
      {
        Color = !actor.IsPlayer ? this.OTHER_ACTION_COLOR : this.PLAYER_ACTION_COLOR
      };
    }

    private djack.RogueSurvivor.Data.Message MakeMessage(Actor actor, string doWhat, Item target)
    {
      return this.MakeMessage(actor, doWhat, target, ".");
    }

    private djack.RogueSurvivor.Data.Message MakeMessage(Actor actor, string doWhat, Item target, string phraseEnd)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(this.ActorVisibleIdentity(actor));
      stringBuilder.Append(" ");
      stringBuilder.Append(doWhat);
      stringBuilder.Append(" ");
      stringBuilder.Append(target.TheName);
      stringBuilder.Append(phraseEnd);
      return new djack.RogueSurvivor.Data.Message(stringBuilder.ToString(), this.m_Session.WorldTime.TurnCounter)
      {
        Color = !actor.IsPlayer ? this.OTHER_ACTION_COLOR : this.PLAYER_ACTION_COLOR
      };
    }

    private void ClearMessages()
    {
      this.m_MessageManager.Clear();
    }

    private void ClearMessagesHistory()
    {
      this.m_MessageManager.ClearHistory();
    }

    private void RemoveLastMessage()
    {
      this.m_MessageManager.RemoveLastMessage();
    }

    private void DrawMessages()
    {
      this.m_MessageManager.Draw(this.m_UI, this.m_Session.LastTurnPlayerActed, 4, 676);
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void AddMessagePressEnter()
    {
      this.AddMessage(new djack.RogueSurvivor.Data.Message("<press ENTER>", this.m_Session.WorldTime.TurnCounter, Color.Yellow));
      this.RedrawPlayScreen();
      this.WaitEnter();
      this.RemoveLastMessage();
      this.RedrawPlayScreen();
    }

    private string Conjugate(Actor actor, string verb)
    {
      if (!actor.IsProperName || actor.IsPluralName)
        return verb;
      return verb + "s";
    }

    private string Conjugate(Actor actor, Verb verb)
    {
      if (!actor.IsProperName || actor.IsPluralName)
        return verb.YouForm;
      return verb.HeForm;
    }

    private string Capitalize(string text)
    {
      if (text == null)
        return "";
      if (text.Length == 1)
        return string.Format("{0}", (object) char.ToUpper(text[0]));
      return string.Format("{0}{1}", (object) char.ToUpper(text[0]), (object) text.Substring(1));
    }

    private string HisOrHer(Actor actor)
    {
      return !actor.Model.DollBody.IsMale ? "her" : "his";
    }

    private string HeOrShe(Actor actor)
    {
      return !actor.Model.DollBody.IsMale ? "she" : "he";
    }

    private string HimOrHer(Actor actor)
    {
      return !actor.Model.DollBody.IsMale ? "her" : "him";
    }

    private string AorAn(string name)
    {
      string str1;
      switch (name[0])
      {
        case 'a':
        case 'e':
        case 'i':
        case 'u':
          str1 = "an ";
          break;
        default:
          str1 = "a ";
          break;
      }
      string str2 = name;
      return str1 + str2;
    }

    private string TruncateString(string s, int maxLength)
    {
      if (s.Length > maxLength)
        return s.Substring(0, maxLength);
      return s;
    }

    private void AnimDelay(int msecs)
    {
      if (!RogueGame.s_Options.IsAnimDelayOn)
        return;
      this.m_UI.UI_Wait(msecs);
    }

    public void Run()
    {
      this.InitDirectories();
      this.LoadData();
      this.LoadOptions();
      this.LoadHints();
      this.ApplyOptions(false);
      this.LoadKeybindings();
      this.m_UI.UI_Clear(Color.Black);
      this.m_UI.UI_DrawStringBold(Color.White, "Loading music...", 0, 0, new Color?());
      this.m_UI.UI_Repaint();
      this.m_MusicManager.Load(GameMusics.ARMY, GameMusics.ARMY_FILE);
      this.m_MusicManager.Load(GameMusics.BIGBEAR_THEME_SONG, GameMusics.BIGBEAR_THEME_SONG_FILE);
      this.m_MusicManager.Load(GameMusics.BIKER, GameMusics.BIKER_FILE);
      this.m_MusicManager.Load(GameMusics.CHAR_UNDERGROUND_FACILITY, GameMusics.CHAR_UNDERGROUND_FACILITY_FILE);
      this.m_MusicManager.Load(GameMusics.DUCKMAN_THEME_SONG, GameMusics.DUCKMAN_THEME_SONG_FILE);
      this.m_MusicManager.Load(GameMusics.FAMU_FATARU_THEME_SONG, GameMusics.FAMU_FATARU_THEME_SONG_FILE);
      this.m_MusicManager.Load(GameMusics.FIGHT, GameMusics.FIGHT_FILE);
      this.m_MusicManager.Load(GameMusics.GANGSTA, GameMusics.GANGSTA_FILE);
      this.m_MusicManager.Load(GameMusics.HANS_VON_HANZ_THEME_SONG, GameMusics.HANS_VON_HANZ_THEME_SONG_FILE);
      this.m_MusicManager.Load(GameMusics.HEYTHERE, GameMusics.HEYTHERE_FILE);
      this.m_MusicManager.Load(GameMusics.HOSPITAL, GameMusics.HOSPITAL_FILE);
      this.m_MusicManager.Load(GameMusics.INSANE, GameMusics.INSANE_FILE);
      this.m_MusicManager.Load(GameMusics.INTERLUDE, GameMusics.INTERLUDE_FILE);
      this.m_MusicManager.Load(GameMusics.INTRO, GameMusics.INTRO_FILE);
      this.m_MusicManager.Load(GameMusics.LIMBO, GameMusics.LIMBO_FILE);
      this.m_MusicManager.Load(GameMusics.PLAYER_DEATH, GameMusics.PLAYER_DEATH_FILE);
      this.m_MusicManager.Load(GameMusics.REINCARNATE, GameMusics.REINCARNATE_FILE);
      this.m_MusicManager.Load(GameMusics.ROGUEDJACK_THEME_SONG, GameMusics.ROGUEDJACK_THEME_SONG_FILE);
      this.m_MusicManager.Load(GameMusics.SANTAMAN_THEME_SONG, GameMusics.SANTAMAN_THEME_SONG_FILE);
      this.m_MusicManager.Load(GameMusics.SEWERS, GameMusics.SEWERS_FILE);
      this.m_MusicManager.Load(GameMusics.SLEEP, GameMusics.SLEEP_FILE);
      this.m_MusicManager.Load(GameMusics.SUBWAY, GameMusics.SUBWAY_FILE);
      this.m_MusicManager.Load(GameMusics.SURVIVORS, GameMusics.SURVIVORS_FILE);
      this.m_UI.UI_Clear(Color.Black);
      this.m_UI.UI_DrawStringBold(Color.White, "Loading music... done!", 0, 0, new Color?());
      this.m_UI.UI_Repaint();
      this.m_UI.UI_Clear(Color.Black);
      this.m_UI.UI_DrawStringBold(Color.White, "Loading sfxs...", 0, 0, new Color?());
      this.m_UI.UI_Repaint();
      this.m_MusicManager.Load(GameSounds.UNDEAD_EAT, GameSounds.UNDEAD_EAT_FILE);
      this.m_MusicManager.Load(GameSounds.UNDEAD_RISE, GameSounds.UNDEAD_RISE_FILE);
      this.m_MusicManager.Load(GameSounds.NIGHTMARE, GameSounds.NIGHTMARE_FILE);
      this.m_UI.UI_Clear(Color.Black);
      this.m_UI.UI_DrawStringBold(Color.White, "Loading sfxs... done!", 0, 0, new Color?());
      this.m_UI.UI_Repaint();
      this.LoadManual();
      this.LoadHiScoreTable();
      while (this.m_IsGameRunning)
        this.GameLoop();
      this.m_MusicManager.StopAll();
      this.m_MusicManager.Dispose();
      this.m_UI.UI_DoQuit();
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void GameLoop()
    {
      this.HandleMainMenu();
      while (this.m_Player != null && !this.m_Player.IsDead && this.m_IsGameRunning)
      {
        DateTime now = DateTime.Now;
        this.m_HasLoadedGame = false;
        this.AdvancePlay(this.m_Session.CurrentMap.District, RogueGame.SimFlags.NOT_SIMULATING);
        if (!this.m_IsGameRunning)
          break;
        this.m_Session.Scoring.RealLifePlayingTime = this.m_Session.Scoring.RealLifePlayingTime.Add(DateTime.Now - now);
      }
    }

    private void InitDirectories()
    {
      int gy1 = 0;
      this.m_UI.UI_Clear(Color.Black);
      this.m_UI.UI_DrawStringBold(Color.Yellow, "Checking user game directories...", 0, gy1, new Color?());
      int gy2 = gy1 + 14;
      this.m_UI.UI_Repaint();
      if (!(false | this.CheckDirectory(RogueGame.GetUserBasePath(), "base user", ref gy2) | this.CheckDirectory(RogueGame.GetUserConfigPath(), "config", ref gy2) | this.CheckDirectory(RogueGame.GetUserDocsPath(), "docs", ref gy2) | this.CheckDirectory(RogueGame.GetUserGraveyardPath(), "graveyard", ref gy2) | this.CheckDirectory(RogueGame.GetUserSavesPath(), "saves", ref gy2) | this.CheckDirectory(RogueGame.GetUserScreenshotsPath(), "screenshots", ref gy2) | this.CheckCopyOfManual()))
        return;
      this.m_UI.UI_DrawStringBold(Color.Yellow, "Directories and game manual created.", 0, gy2, new Color?());
      int gy3 = gy2 + 14;
      this.m_UI.UI_DrawStringBold(Color.Yellow, "Your game data directory is in the game folder:", 0, gy3, new Color?());
      int gy4 = gy3 + 14;
      this.m_UI.UI_DrawString(Color.LightGreen, RogueGame.GetUserBasePath(), 0, gy4, new Color?());
      int gy5 = gy4 + 14;
      this.m_UI.UI_DrawStringBold(Color.Yellow, "When you uninstall the game you can delete this directory.", 0, gy5, new Color?());
      int num = gy5 + 14;
      this.DrawFootnote(Color.White, "press ENTER");
      this.m_UI.UI_Repaint();
      this.WaitEnter();
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void HandleMainMenu()
    {
      bool flag1 = true;
      bool flag2 = File.Exists(RogueGame.GetUserSave());
      string[] entries = new string[9]
      {
        "New Game",
        flag2 ? "Load Game" : "(Load Game)",
        "Redefine keys",
        "Options",
        "Game Manual",
        "All Hints",
        "Hi Scores",
        "Credits",
        "Quit Game"
      };
      int currentChoice = 0;
      do
      {
        if (!this.m_MusicManager.IsPlaying(GameMusics.INTRO) && !this.m_PlayedIntro)
        {
          this.m_MusicManager.StopAll();
          this.m_MusicManager.Play(GameMusics.INTRO);
          this.m_PlayedIntro = true;
        }
        int gy1;
        int gx1 = gy1 = 0;
        this.m_UI.UI_Clear(Color.Black);
        this.DrawHeader();
        gy1 += 14;
        this.m_UI.UI_DrawStringBold(Color.Yellow, "Main Menu", 0, gy1, new Color?());
        gy1 += 28;
        this.DrawMenuOrOptions(currentChoice, Color.White, entries, Color.White, (string[]) null, gx1, ref gy1, 256);
        this.DrawFootnote(Color.White, "cursor to move, ENTER to select");
        DateTime now = DateTime.Now;
        if (now.Month == 12 && now.Day >= 24 && now.Day <= 26)
        {
          for (int index = 0; index < 10; ++index)
          {
            int gx2 = this.m_Rules.Roll(0, 1024);
            int gy2 = this.m_Rules.Roll(0, 768);
            this.m_UI.UI_DrawImage("Actors\\santaman", gx2, gy2);
            this.m_UI.UI_DrawStringBold(Color.Snow, "* Merry Christmas *", gx2 - 60, gy2 - 10, new Color?());
          }
        }
        this.m_UI.UI_Repaint();
        switch (this.m_UI.UI_WaitKey().KeyCode)
        {
          case Keys.Return:
            switch (currentChoice)
            {
              case 0:
                if (this.HandleNewCharacter())
                {
                  this.StartNewGame();
                  flag1 = false;
                  break;
                }
                break;
              case 1:
                if (flag2)
                {
                  gy1 += 28;
                  this.m_UI.UI_DrawStringBold(Color.Yellow, "Loading game, please wait...", gx1, gy1, new Color?());
                  this.m_UI.UI_Repaint();
                  this.LoadGame(RogueGame.GetUserSave());
                  flag1 = false;
                  break;
                }
                break;
              case 2:
                this.HandleRedefineKeys();
                break;
              case 3:
                this.HandleOptions(false);
                this.ApplyOptions(false);
                break;
              case 4:
                this.HandleHelpMode();
                break;
              case 5:
                this.HandleHintsScreen();
                break;
              case 6:
                this.HandleHiScores(true);
                break;
              case 7:
                this.HandleCredits();
                break;
              case 8:
                this.m_IsGameRunning = false;
                flag1 = false;
                break;
            }
            break;
          case Keys.Up:
            if (currentChoice > 0)
            {
              --currentChoice;
              break;
            }
            currentChoice = entries.Length - 1;
            break;
          case Keys.Down:
            currentChoice = (currentChoice + 1) % entries.Length;
            break;
        }
      }
      while (flag1);
    }

    private bool HandleNewCharacter()
    {
      DiceRoller roller = new DiceRoller();
      this.m_Session.Reset();
      bool isUndead;
      if (!this.HandleNewGameMode() || !this.HandleNewCharacterRace(roller, out isUndead))
        return false;
      this.m_CharGen.IsUndead = isUndead;
      if (isUndead)
      {
        GameActors.IDs modelID;
        if (!this.HandleNewCharacterUndeadType(roller, out modelID))
          return false;
        this.m_CharGen.UndeadModel = modelID;
      }
      else
      {
        bool isMale;
        if (!this.HandleNewCharacterGender(roller, out isMale))
          return false;
        this.m_CharGen.IsMale = isMale;
      }
      if (!isUndead)
      {
        Skills.IDs skID;
        if (!this.HandleNewCharacterSkill(roller, out skID))
          return false;
        this.m_CharGen.StartingSkill = skID;
        this.m_Session.Scoring.StartingSkill = skID;
      }
      return true;
    }

    private bool HandleNewGameMode()
    {
      string[] entries = new string[3]
      {
        Session.DescGameMode(GameMode.GM_STANDARD),
        Session.DescGameMode(GameMode.GM_CORPSES_INFECTION),
        Session.DescGameMode(GameMode.GM_VINTAGE)
      };
      string[] values = new string[3]
      {
        "Rogue Survivor standard game.",
        "Don't get a cold. Keep an eye on your deceased diseased friends.",
        "The classic zombies next door."
      };
      bool flag1 = true;
      bool flag2 = false;
      int currentChoice = 0;
      do
      {
        this.m_UI.UI_Clear(Color.Black);
        int gy1;
        int gx = gy1 = 0;
        this.m_UI.UI_DrawStringBold(Color.Yellow, "New Game - Choose Game Mode", gx, gy1, new Color?());
        int gy2 = gy1 + 28;
        this.DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGray, values, gx, ref gy2, 256);
        int gy3 = gy2 + 28;
        string[] strArray = new string[0];
        switch (currentChoice)
        {
          case 0:
            strArray = new string[4]
            {
              "This is the standard game setting:",
              "- All the kinds of undeads.",
              "- Undeads can evolve.",
              "- Livings can zombify instantly when dead."
            };
            break;
          case 1:
            strArray = new string[6]
            {
              "This is the standard game setting with corpses and infection: ",
              "- All the kinds of undeads.",
              "- Undeads can evolve.",
              "- Some undeads can infect livings when damaging them.",
              "- Livings become corpses when dead.",
              "- Corpses will slowy rot... but may rise as undead if infected."
            };
            break;
          case 2:
            strArray = new string[10]
            {
              "This is the zombie game for classic hardcore fans: ",
              "- Undeads are only zombified men and women.",
              "- Undeads don't evolve.",
              "- Some undeads can infect livings when damaging them.",
              "- Livings become corpses when dead.",
              "- Corpses will slowy rot... but may rise as undead if infected.",
              "",
              "NOTE:",
              "This mode force some options OFF.",
              "Remember to set them back ON again when you play other modes!"
            };
            break;
        }
        foreach (string text in strArray)
        {
          this.m_UI.UI_DrawStringBold(Color.Gray, text, gx, gy3, new Color?());
          gy3 += 14;
        }
        this.DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel");
        this.m_UI.UI_Repaint();
        switch (this.m_UI.UI_WaitKey().KeyCode)
        {
          case Keys.Return:
            switch (currentChoice)
            {
              case 0:
                this.m_Session.GameMode = GameMode.GM_STANDARD;
                flag2 = true;
                flag1 = false;
                break;
              case 1:
                this.m_Session.GameMode = GameMode.GM_CORPSES_INFECTION;
                flag2 = true;
                flag1 = false;
                break;
              case 2:
                this.m_Session.GameMode = GameMode.GM_VINTAGE;
                RogueGame.s_Options.AllowUndeadsEvolution = false;
                RogueGame.s_Options.ShamblersUpgrade = false;
                RogueGame.s_Options.RatsUpgrade = false;
                RogueGame.s_Options.SkeletonsUpgrade = false;
                this.ApplyOptions(false);
                flag2 = true;
                flag1 = false;
                break;
            }
            break;
          case Keys.Escape:
            flag2 = false;
            flag1 = false;
            break;
          case Keys.Up:
            if (currentChoice > 0)
            {
              --currentChoice;
              break;
            }
            currentChoice = entries.Length - 1;
            break;
          case Keys.Down:
            currentChoice = (currentChoice + 1) % entries.Length;
            break;
        }
      }
      while (flag1);
      return flag2;
    }

    private bool HandleNewCharacterRace(DiceRoller roller, out bool isUndead)
    {
      string[] entries = new string[3]
      {
        "*Random*",
        "Living",
        "Undead"
      };
      string[] values = new string[3]
      {
        "(picks a race at random for you)",
        "Try to survive.",
        "Eat brains and die again."
      };
      isUndead = false;
      bool flag1 = true;
      bool flag2 = false;
      int currentChoice = 0;
      do
      {
        this.m_UI.UI_Clear(Color.Black);
        int gy1;
        int gx = gy1 = 0;
        this.m_UI.UI_DrawStringBold(Color.Yellow, string.Format("[{0}] New Character - Choose Race", (object) Session.DescGameMode(this.m_Session.GameMode)), gx, gy1, new Color?());
        int gy2 = gy1 + 28;
        this.DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGray, values, gx, ref gy2, 256);
        int num = gy2 + 28;
        this.DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel");
        this.m_UI.UI_Repaint();
        switch (this.m_UI.UI_WaitKey().KeyCode)
        {
          case Keys.Return:
            switch (currentChoice)
            {
              case 0:
                isUndead = roller.RollChance(50);
                int gy3 = num + 14;
                this.m_UI.UI_DrawStringBold(Color.White, string.Format("Race : {0}.", isUndead ? (object) "Undead" : (object) "Living"), gx, gy3, new Color?());
                int gy4 = gy3 + 14;
                this.m_UI.UI_DrawStringBold(Color.Yellow, "Is that OK? Y to confirm, N to cancel.", gx, gy4, new Color?());
                this.m_UI.UI_Repaint();
                if (this.WaitYesOrNo())
                {
                  flag2 = true;
                  flag1 = false;
                  break;
                }
                break;
              case 1:
                isUndead = false;
                flag2 = true;
                flag1 = false;
                break;
              case 2:
                isUndead = true;
                flag2 = true;
                flag1 = false;
                break;
            }
            break;
          case Keys.Escape:
            flag2 = false;
            flag1 = false;
            break;
          case Keys.Up:
            if (currentChoice > 0)
            {
              --currentChoice;
              break;
            }
            currentChoice = entries.Length - 1;
            break;
          case Keys.Down:
            currentChoice = (currentChoice + 1) % entries.Length;
            break;
        }
      }
      while (flag1);
      return flag2;
    }

    private bool HandleNewCharacterGender(DiceRoller roller, out bool isMale)
    {
      ActorModel maleCivilian = this.GameActors.MaleCivilian;
      ActorModel femaleCivilian = this.GameActors.FemaleCivilian;
      string[] entries = new string[3]
      {
        "*Random*",
        "Male",
        "Female"
      };
      string[] values = new string[3]
      {
        "(picks a gender at random for you)",
        string.Format("HP:{0:D2}  Def:{1:D2}  Dmg:{2:D1}", (object) maleCivilian.StartingSheet.BaseHitPoints, (object) maleCivilian.StartingSheet.BaseDefence.Value, (object) maleCivilian.StartingSheet.UnarmedAttack.DamageValue),
        string.Format("HP:{0:D2}  Def:{1:D2}  Dmg:{2:D1}", (object) femaleCivilian.StartingSheet.BaseHitPoints, (object) femaleCivilian.StartingSheet.BaseDefence.Value, (object) femaleCivilian.StartingSheet.UnarmedAttack.DamageValue)
      };
      isMale = true;
      bool flag1 = true;
      bool flag2 = false;
      int currentChoice = 0;
      do
      {
        this.m_UI.UI_Clear(Color.Black);
        int gy;
        int gx = gy = 0;
        this.m_UI.UI_DrawStringBold(Color.Yellow, string.Format("[{0}] New Living - Choose Gender", (object) Session.DescGameMode(this.m_Session.GameMode)), gx, gy, new Color?());
        gy += 28;
        this.DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGray, values, gx, ref gy, 256);
        this.DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel");
        this.m_UI.UI_Repaint();
        switch (this.m_UI.UI_WaitKey().KeyCode)
        {
          case Keys.Return:
            switch (currentChoice)
            {
              case 0:
                isMale = roller.RollChance(50);
                gy += 14;
                this.m_UI.UI_DrawStringBold(Color.White, string.Format("Gender : {0}.", isMale ? (object) "Male" : (object) "Female"), gx, gy, new Color?());
                gy += 14;
                this.m_UI.UI_DrawStringBold(Color.Yellow, "Is that OK? Y to confirm, N to cancel.", gx, gy, new Color?());
                this.m_UI.UI_Repaint();
                if (this.WaitYesOrNo())
                {
                  flag2 = true;
                  flag1 = false;
                  break;
                }
                break;
              case 1:
                isMale = true;
                flag2 = true;
                flag1 = false;
                break;
              case 2:
                isMale = false;
                flag2 = true;
                flag1 = false;
                break;
            }
            break;
          case Keys.Escape:
            flag2 = false;
            flag1 = false;
            break;
          case Keys.Up:
            if (currentChoice > 0)
            {
              --currentChoice;
              break;
            }
            currentChoice = entries.Length - 1;
            break;
          case Keys.Down:
            currentChoice = (currentChoice + 1) % entries.Length;
            break;
        }
      }
      while (flag1);
      return flag2;
    }

    private string DescribeUndeadModelStatLine(ActorModel m)
    {
      return string.Format("HP:{0:D3}  Spd:{1:F2}  Atk:{2:D2}  Def:{3:D2}  Dmg:{4:D2}  FoV:{5:D1}  Sml:{6:F2}", (object) m.StartingSheet.BaseHitPoints, (object) (float) ((double) m.DollBody.Speed / 100.0), (object) m.StartingSheet.UnarmedAttack.HitValue, (object) m.StartingSheet.BaseDefence.Value, (object) m.StartingSheet.UnarmedAttack.DamageValue, (object) m.StartingSheet.BaseViewRange, (object) m.StartingSheet.BaseSmellRating);
    }

    private bool HandleNewCharacterUndeadType(DiceRoller roller, out GameActors.IDs modelID)
    {
      ActorModel skeleton = this.GameActors.Skeleton;
      ActorModel zombie = this.GameActors.Zombie;
      ActorModel maleZombified = this.GameActors.MaleZombified;
      ActorModel femaleZombified = this.GameActors.FemaleZombified;
      ActorModel zombieMaster = this.GameActors.ZombieMaster;
      string[] entries = new string[6]
      {
        "*Random*",
        skeleton.Name,
        zombie.Name,
        maleZombified.Name,
        femaleZombified.Name,
        zombieMaster.Name
      };
      string[] values = new string[6]
      {
        "(picks a type at random for you)",
        this.DescribeUndeadModelStatLine(skeleton),
        this.DescribeUndeadModelStatLine(zombie),
        this.DescribeUndeadModelStatLine(maleZombified),
        this.DescribeUndeadModelStatLine(femaleZombified),
        this.DescribeUndeadModelStatLine(zombieMaster)
      };
      modelID = GameActors.IDs.UNDEAD_MALE_ZOMBIFIED;
      bool flag1 = true;
      bool flag2 = false;
      int currentChoice = 0;
      do
      {
        this.m_UI.UI_Clear(Color.Black);
        int gy1;
        int gx = gy1 = 0;
        this.m_UI.UI_DrawStringBold(Color.Yellow, string.Format("[{0}] New Undead - Choose Type", (object) Session.DescGameMode(this.m_Session.GameMode)), gx, gy1, new Color?());
        int gy2 = gy1 + 28;
        this.DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGray, values, gx, ref gy2, 256);
        this.DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel");
        this.m_UI.UI_Repaint();
        switch (this.m_UI.UI_WaitKey().KeyCode)
        {
          case Keys.Return:
            switch (currentChoice)
            {
              case 0:
                currentChoice = roller.Roll(0, 5);
                switch (currentChoice)
                {
                  case 0:
                    modelID = GameActors.IDs._FIRST;
                    break;
                  case 1:
                    modelID = GameActors.IDs.UNDEAD_ZOMBIE;
                    break;
                  case 2:
                    modelID = GameActors.IDs.UNDEAD_MALE_ZOMBIFIED;
                    break;
                  case 3:
                    modelID = GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED;
                    break;
                  case 4:
                    modelID = GameActors.IDs.UNDEAD_ZOMBIE_MASTER;
                    break;
                  default:
                    throw new ArgumentOutOfRangeException("unhandled select " + (object) currentChoice);
                }
                int gy3 = gy2 + 14;
                this.m_UI.UI_DrawStringBold(Color.White, string.Format("Type : {0}.", (object) this.GameActors[modelID].Name), gx, gy3, new Color?());
                int gy4 = gy3 + 14;
                this.m_UI.UI_DrawStringBold(Color.Yellow, "Is that OK? Y to confirm, N to cancel.", gx, gy4, new Color?());
                this.m_UI.UI_Repaint();
                if (this.WaitYesOrNo())
                {
                  flag2 = true;
                  flag1 = false;
                  break;
                }
                break;
              case 1:
                modelID = GameActors.IDs._FIRST;
                flag2 = true;
                flag1 = false;
                break;
              case 2:
                modelID = GameActors.IDs.UNDEAD_ZOMBIE;
                flag2 = true;
                flag1 = false;
                break;
              case 3:
                modelID = GameActors.IDs.UNDEAD_MALE_ZOMBIFIED;
                this.m_CharGen.IsMale = true;
                flag2 = true;
                flag1 = false;
                break;
              case 4:
                modelID = GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED;
                this.m_CharGen.IsMale = false;
                flag2 = true;
                flag1 = false;
                break;
              case 5:
                modelID = GameActors.IDs.UNDEAD_ZOMBIE_MASTER;
                flag2 = true;
                flag1 = false;
                break;
            }
            break;
          case Keys.Escape:
            flag2 = false;
            flag1 = false;
            break;
          case Keys.Up:
            if (currentChoice > 0)
            {
              --currentChoice;
              break;
            }
            currentChoice = entries.Length - 1;
            break;
          case Keys.Down:
            currentChoice = (currentChoice + 1) % entries.Length;
            break;
        }
      }
      while (flag1);
      return flag2;
    }

    private bool HandleNewCharacterSkill(DiceRoller roller, out Skills.IDs skID)
    {
      Skills.IDs[] idsArray = new Skills.IDs[20];
      string[] entries = new string[idsArray.Length + 1];
      string[] values = new string[idsArray.Length + 1];
      entries[0] = "*Random*";
      values[0] = "(picks a skill at random for you)";
      for (int id = 0; id < 20; ++id)
      {
        idsArray[id] = (Skills.IDs) id;
        entries[id + 1] = Skills.Name(idsArray[id]);
        values[id + 1] = string.Format("{0} max - {1}", (object) Skills.MaxSkillLevel(id), (object) this.DescribeSkillShort(idsArray[id]));
      }
      skID = Skills.IDs._FIRST;
      bool flag1 = true;
      bool flag2 = false;
      int currentChoice = 0;
      do
      {
        this.m_UI.UI_Clear(Color.Black);
        int gy1;
        int gx = gy1 = 0;
        this.m_UI.UI_DrawStringBold(Color.Yellow, string.Format("[{0}] New {1} Character - Choose Starting Skill", (object) Session.DescGameMode(this.m_Session.GameMode), this.m_CharGen.IsMale ? (object) "Male" : (object) "Female"), gx, gy1, new Color?());
        int gy2 = gy1 + 28;
        this.DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGray, values, gx, ref gy2, 256);
        this.DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel");
        this.m_UI.UI_Repaint();
        switch (this.m_UI.UI_WaitKey().KeyCode)
        {
          case Keys.Return:
            skID = currentChoice != 0 ? (Skills.IDs) (currentChoice - 1) : Skills.RollLiving(roller);
            int gy3 = gy2 + 14;
            this.m_UI.UI_DrawStringBold(Color.White, string.Format("Skill : {0}.", (object) Skills.Name(skID)), gx, gy3, new Color?());
            int gy4 = gy3 + 14;
            this.m_UI.UI_DrawStringBold(Color.Yellow, "Is that OK? Y to confirm, N to cancel.", gx, gy4, new Color?());
            this.m_UI.UI_Repaint();
            if (this.WaitYesOrNo())
            {
              flag2 = true;
              flag1 = false;
              break;
            }
            break;
          case Keys.Escape:
            flag2 = false;
            flag1 = false;
            break;
          case Keys.Up:
            if (currentChoice > 0)
            {
              --currentChoice;
              break;
            }
            currentChoice = entries.Length - 1;
            break;
          case Keys.Down:
            currentChoice = (currentChoice + 1) % entries.Length;
            break;
        }
      }
      while (flag1);
      return flag2;
    }

    private void LoadManual()
    {
      this.m_UI.UI_Clear(Color.Black);
      int num1 = 0;
      this.m_UI.UI_DrawStringBold(Color.White, "Loading game manual...", 0, 0, new Color?());
      int gy1 = num1 + 14;
      this.m_UI.UI_Repaint();
      this.m_Manual = new TextFile();
      this.m_ManualLine = 0;
      if (!this.m_Manual.Load(this.GetUserManualFilePath()))
      {
        this.m_UI.UI_DrawStringBold(Color.Red, "Error while loading the manual.", 0, gy1, new Color?());
        int gy2 = gy1 + 14;
        this.m_UI.UI_DrawStringBold(Color.Red, "The manual won't be available ingame.", 0, gy2, new Color?());
        int num2 = gy2 + 14;
        this.m_UI.UI_Repaint();
        this.DrawFootnote(Color.White, "press ENTER");
        this.WaitEnter();
        this.m_Manual = (TextFile) null;
      }
      else
      {
        this.m_UI.UI_DrawStringBold(Color.White, "Parsing game manual...", 0, gy1, new Color?());
        int gy2 = gy1 + 14;
        this.m_UI.UI_Repaint();
        m_Manual.FormatLines(TEXTFILE_CHARS_PER_LINE);
        this.m_UI.UI_Clear(Color.Black);
        this.m_UI.UI_DrawStringBold(Color.White, "Game manual... done!", 0, gy2, new Color?());
        this.m_UI.UI_Repaint();
      }
    }

    private void HandleHiScores(bool saveToTextfile)
    {
      TextFile textFile = (TextFile) null;
      if (saveToTextfile)
        textFile = new TextFile();
      this.m_UI.UI_Clear(Color.Black);
      int num1 = 0;
      this.DrawHeader();
      int gy1 = num1 + 14;
      this.m_UI.UI_DrawStringBold(Color.Yellow, "Hi Scores", 0, gy1, new Color?());
      int gy2 = gy1 + 14;
      this.m_UI.UI_DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy2, new Color?());
      int gy3 = gy2 + 14;
      this.m_UI.UI_DrawStringBold(Color.White, "Rank | Name, Skills, Death       |  Score |Difficulty|Survival|  Kills |Achievm.|      Game Time | Playing time", 0, gy3, new Color?());
      int gy4 = gy3 + 14;
      if (saveToTextfile)
      {
        textFile.Append(string.Format("ROGUE SURVIVOR {0}", (object) "alpha 9"));
        textFile.Append("Hi Scores");
        textFile.Append("Rank | Name, Skills, Death       |  Score |Difficulty|Survival|  Kills |Achievm.|      Game Time | Playing time");
      }
      for (int index = 0; index < this.m_HiScoreTable.Count; ++index)
      {
        Color color = index == 0 ? Color.LightYellow : (index == 1 ? Color.LightCyan : (index == 2 ? Color.LightGreen : Color.DimGray));
        this.m_UI.UI_DrawStringBold(color, "------------------------------------------------------------------------------------------------------------------------", 0, gy4, new Color?());
        int gy5 = gy4 + 14;
        HiScore hiScore = this.m_HiScoreTable[index];
        string str = string.Format("{0,3}. | {1,-25} | {2,6} |     {3,3}% | {4,6} | {5,6} | {6,6} | {7,14} | {8}", (object) (index + 1), (object) this.TruncateString(hiScore.Name, 25), (object) hiScore.TotalPoints, (object) hiScore.DifficultyPercent, (object) hiScore.SurvivalPoints, (object) hiScore.KillPoints, (object) hiScore.AchievementPoints, (object) new WorldTime(hiScore.TurnSurvived).ToString(), (object) this.TimeSpanToString(hiScore.PlayingTime));
        this.m_UI.UI_DrawStringBold(color, str, 0, gy5, new Color?());
        int gy6 = gy5 + 14;
        this.m_UI.UI_DrawStringBold(color, string.Format("     | {0}.", (object) hiScore.SkillsDescription), 0, gy6, new Color?());
        int gy7 = gy6 + 14;
        this.m_UI.UI_DrawStringBold(color, string.Format("     | {0}.", (object) hiScore.Death), 0, gy7, new Color?());
        gy4 = gy7 + 14;
        if (saveToTextfile)
        {
          textFile.Append("------------------------------------------------------------------------------------------------------------------------");
          textFile.Append(str);
          textFile.Append(string.Format("     | {0}", (object) hiScore.SkillsDescription));
          textFile.Append(string.Format("     | {0}", (object) hiScore.Death));
        }
      }
      string scoreTextFilePath = this.GetUserHiScoreTextFilePath();
      if (saveToTextfile)
        textFile.Save(scoreTextFilePath);
      this.m_UI.UI_DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy4, new Color?());
      int gy8 = gy4 + 14;
      if (saveToTextfile)
      {
        this.m_UI.UI_DrawStringBold(Color.White, scoreTextFilePath, 0, gy8, new Color?());
        int num2 = gy8 + 14;
      }
      this.DrawFootnote(Color.White, "press ESC to leave");
      this.m_UI.UI_Repaint();
      this.WaitEscape();
    }

    private void LoadHiScoreTable()
    {
      this.m_UI.UI_Clear(Color.Black);
      this.m_UI.UI_DrawStringBold(Color.White, "Loading hiscores table...", 0, 0, new Color?());
      this.m_UI.UI_Repaint();
      this.m_HiScoreTable = HiScoreTable.Load(this.GetUserHiScoreFilePath());
      if (this.m_HiScoreTable == null)
      {
        this.m_HiScoreTable = new HiScoreTable(12);
        this.m_HiScoreTable.Clear();
      }
      this.m_UI.UI_Clear(Color.Black);
      this.m_UI.UI_DrawStringBold(Color.White, "Loading hiscores table... done!", 0, 0, new Color?());
      this.m_UI.UI_Repaint();
    }

    private void SaveHiScoreTable()
    {
      this.m_UI.UI_Clear(Color.Black);
      this.m_UI.UI_DrawStringBold(Color.White, "Saving hiscores table...", 0, 0, new Color?());
      this.m_UI.UI_Repaint();
      HiScoreTable.Save(this.m_HiScoreTable, this.GetUserHiScoreFilePath());
      this.m_UI.UI_Clear(Color.Black);
      this.m_UI.UI_DrawStringBold(Color.White, "Saving hiscores table... done!", 0, 0, new Color?());
      this.m_UI.UI_Repaint();
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void StartNewGame()
    {
      bool isUndead = this.m_CharGen.IsUndead;
      this.GenerateWorld(true, RogueGame.s_Options.CitySize);
      this.m_Session.Scoring.AddVisit(this.m_Session.WorldTime.TurnCounter, this.m_Player.Location.Map);
      this.m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, string.Format(isUndead ? "Rose in {0}." : "Woke up in {0}.", (object) this.m_Player.Location.Map.Name));
      this.m_Session.Scoring.Side = isUndead ? DifficultySide.FOR_UNDEAD : DifficultySide.FOR_SURVIVOR;
      if (RogueGame.s_Options.IsAdvisorEnabled)
      {
        this.ClearMessages();
        this.ClearMessagesHistory();
        this.AddMessage(new djack.RogueSurvivor.Data.Message("The Advisor is enabled and will give you hints during the game.", 0, Color.LightGreen));
        this.AddMessage(new djack.RogueSurvivor.Data.Message("The hints help a beginner learning the basic controls.", 0, Color.LightGreen));
        this.AddMessage(new djack.RogueSurvivor.Data.Message("You can disable the Advisor by going to the Options screen.", 0, Color.LightGreen));
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Press {0} during the game to change the options.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.OPTIONS_MODE)), 0, Color.LightGreen));
        this.AddMessage(new djack.RogueSurvivor.Data.Message("<press ENTER>", 0, Color.Yellow));
        this.RedrawPlayScreen();
        this.WaitEnter();
      }
      this.ClearMessages();
      this.ClearMessagesHistory();
      this.AddMessage(new djack.RogueSurvivor.Data.Message("*****************************", 0, Color.LightGreen));
      this.AddMessage(new djack.RogueSurvivor.Data.Message("* Welcome to Rogue Survivor *", 0, Color.LightGreen));
      this.AddMessage(new djack.RogueSurvivor.Data.Message("* We hope you like Zombies  *", 0, Color.LightGreen));
      this.AddMessage(new djack.RogueSurvivor.Data.Message("*****************************", 0, Color.LightGreen));
      this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Press {0} for help", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.HELP_MODE)), 0, Color.LightGreen));
      this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Press {0} to redefine keys", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.KEYBINDING_MODE)), 0, Color.LightGreen));
      this.AddMessage(new djack.RogueSurvivor.Data.Message("<press ENTER>", 0, Color.Yellow));
      this.RefreshPlayer();
      this.RedrawPlayScreen();
      this.WaitEnter();
      this.ClearMessages();
      this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format(isUndead ? "{0} rises..." : "{0} wakes up.", (object) this.m_Player.Name), 0, Color.White));
      this.RedrawPlayScreen();
      this.RestartSimThread();
    }

    private void HandleCredits()
    {
      int num1 = 0;
      this.m_MusicManager.StopAll();
      this.m_MusicManager.PlayLooping(GameMusics.SLEEP);
      this.m_UI.UI_Clear(Color.Black);
      this.DrawHeader();
      int gy1 = num1 + 14;
      this.m_UI.UI_DrawStringBold(Color.Yellow, "Credits", 0, gy1, new Color?());
      int gy2 = gy1 + 28;
      this.m_UI.UI_DrawStringBold(Color.White, "Programming, Graphics & Music by Jacques Ruiz (roguedjack)", 0, gy2, new Color?());
      int gy3 = gy2 + 28;
      this.m_UI.UI_DrawStringBold(Color.White, "Programming", 0, gy3, new Color?());
      this.m_UI.UI_DrawString(Color.White, "- C# NET 3.5, Microsoft Visual C# 2010 Express", 256, gy3, new Color?());
      int gy4 = gy3 + 14;
      this.m_UI.UI_DrawStringBold(Color.White, "Graphics softwares", 0, gy4, new Color?());
      this.m_UI.UI_DrawString(Color.White, "- Inkscape, Paint.NET", 256, gy4, new Color?());
      int gy5 = gy4 + 14;
      this.m_UI.UI_DrawStringBold(Color.White, "Sound & Music softwares", 0, gy5, new Color?());
      this.m_UI.UI_DrawString(Color.White, "- GuitarPro 6, Audacity", 256, gy5, new Color?());
      int gy6 = gy5 + 14;
      this.m_UI.UI_DrawStringBold(Color.White, "Sound samples", 0, gy6, new Color?());
      this.m_UI.UI_DrawString(Color.White, "- http://www.sound-fishing.net  http://www.soundsnap.com/", 256, gy6, new Color?());
      int gy7 = gy6 + 14;
      this.m_UI.UI_DrawStringBold(Color.White, "Installer", 0, gy7, new Color?());
      this.m_UI.UI_DrawString(Color.White, "- NSIS", 256, gy7, new Color?());
      int gy8 = gy7 + 28;
      this.m_UI.UI_DrawStringBold(Color.White, "Contact", 0, gy8, new Color?());
      int gy9 = gy8 + 14;
      this.m_UI.UI_DrawString(Color.White, "Email      : roguedjack@yahoo.fr", 0, gy9, new Color?());
      int gy10 = gy9 + 14;
      this.m_UI.UI_DrawString(Color.White, "Blog       : http://roguesurvivor.blogspot.com/", 0, gy10, new Color?());
      int gy11 = gy10 + 14;
      this.m_UI.UI_DrawString(Color.White, "Fans Forum : http://roguesurvivor.proboards.com/", 0, gy11, new Color?());
      int gy12 = gy11 + 14;
      this.m_UI.UI_DrawStringBold(Color.White, "Thanks to the players for their feedback and eagerness to die!", 0, gy12, new Color?());
      int num2 = gy12 + 14;
      this.DrawFootnote(Color.White, "ESC to leave");
      this.m_UI.UI_Repaint();
      this.WaitEscape();
    }

    private void HandleOptions(bool ingame)
    {
      GameOptions gameOptions = RogueGame.s_Options;
      GameOptions.IDs[] idsArray = new GameOptions.IDs[36]
      {
        GameOptions.IDs.UI_MUSIC,
        GameOptions.IDs.UI_MUSIC_VOLUME,
        GameOptions.IDs.UI_ANIM_DELAY,
        GameOptions.IDs.UI_SHOW_MINIMAP,
        GameOptions.IDs.UI_SHOW_PLAYER_TAG_ON_MINIMAP,
        GameOptions.IDs.UI_ADVISOR,
        GameOptions.IDs.UI_COMBAT_ASSISTANT,
        GameOptions.IDs.UI_SHOW_PLAYER_TARGETS,
        GameOptions.IDs.UI_SHOW_TARGETS,
        GameOptions.IDs.GAME_SIM_THREAD,
        GameOptions.IDs.GAME_SIMULATE_DISTRICTS,
        GameOptions.IDs.GAME_SIMULATE_SLEEP,
        GameOptions.IDs.GAME_DEATH_SCREENSHOT,
        GameOptions.IDs.GAME_PERMADEATH,
        GameOptions.IDs.GAME_CITY_SIZE,
        GameOptions.IDs.GAME_DISTRICT_SIZE,
        GameOptions.IDs.GAME_REVEAL_STARTING_DISTRICT,
        GameOptions.IDs.GAME_MAX_CIVILIANS,
        GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE,
        GameOptions.IDs.GAME_AGGRESSIVE_HUNGRY_CIVILIANS,
        GameOptions.IDs.GAME_NPC_CAN_STARVE_TO_DEATH,
        GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE,
        GameOptions.IDs.GAME_MAX_UNDEADS,
        GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION,
        GameOptions.IDs.GAME_DAY_ZERO_UNDEADS_PERCENT,
        GameOptions.IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE,
        GameOptions.IDs.GAME_UNDEADS_UPGRADE_DAYS,
        GameOptions.IDs.GAME_SHAMBLERS_UPGRADE,
        GameOptions.IDs.GAME_SKELETONS_UPGRADE,
        GameOptions.IDs.GAME_RATS_UPGRADE,
        GameOptions.IDs.GAME_NATGUARD_FACTOR,
        GameOptions.IDs.GAME_SUPPLIESDROP_FACTOR,
        GameOptions.IDs.GAME_MAX_REINCARNATIONS,
        GameOptions.IDs.GAME_REINC_LIVING_RESTRICTED,
        GameOptions.IDs.GAME_REINCARNATE_AS_RAT,
        GameOptions.IDs.GAME_REINCARNATE_TO_SEWERS
      };
      string[] entries = new string[idsArray.Length];
      string[] values = new string[idsArray.Length];
      for (int index = 0; index < idsArray.Length; ++index)
        entries[index] = GameOptions.Name(idsArray[index]);
      bool flag = true;
      int currentChoice = 0;
      do
      {
        for (int index = 0; index < idsArray.Length; ++index)
          values[index] = RogueGame.s_Options.DescribeValue(this.m_Session.GameMode, idsArray[index]);
        int gy;
        int gx = gy = 0;
        this.m_UI.UI_Clear(Color.Black);
        this.DrawHeader();
        gy += 14;
        this.m_UI.UI_DrawStringBold(Color.Yellow, string.Format("[{0}] - Options", (object) Session.DescGameMode(this.m_Session.GameMode)), 0, gy, new Color?());
        gy += 28;
        this.DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGreen, values, gx, ref gy, 400);
        gy += 14;
        this.m_UI.UI_DrawStringBold(Color.Red, "* Caution : increasing these values makes the game runs slower and saving/loading longer.", gx, gy, new Color?());
        gy += 14;
        gy += 14;
        this.m_UI.UI_DrawStringBold(Color.Yellow, string.Format("Difficulty Rating : {0}% as survivor / {1}% as undead.", (object) (int) (100.0 * (double) Scoring.ComputeDifficultyRating(RogueGame.s_Options, DifficultySide.FOR_SURVIVOR, 0)), (object) (int) (100.0 * (double) Scoring.ComputeDifficultyRating(RogueGame.s_Options, DifficultySide.FOR_UNDEAD, 0))), gx, gy, new Color?());
        gy += 14;
        this.m_UI.UI_DrawStringBold(Color.White, "Difficulty used for scoring automatically decrease with each reincarnation.", gx, gy, new Color?());
        gy += 28;
        this.DrawFootnote(Color.White, "cursor to move and change values, R to restore previous values, ESC to save and leave");
        this.m_UI.UI_Repaint();
        switch (this.m_UI.UI_WaitKey().KeyCode)
        {
          case Keys.Escape:
            flag = false;
            break;
          case Keys.Left:
            switch (idsArray[currentChoice])
            {
              case GameOptions.IDs.UI_MUSIC:
                RogueGame.s_Options.PlayMusic = !RogueGame.s_Options.PlayMusic;
                break;
              case GameOptions.IDs.UI_MUSIC_VOLUME:
                RogueGame.s_Options.MusicVolume -= 5;
                break;
              case GameOptions.IDs.UI_SHOW_PLAYER_TAG_ON_MINIMAP:
                RogueGame.s_Options.ShowPlayerTagsOnMinimap = !RogueGame.s_Options.ShowPlayerTagsOnMinimap;
                break;
              case GameOptions.IDs.UI_ANIM_DELAY:
                RogueGame.s_Options.IsAnimDelayOn = !RogueGame.s_Options.IsAnimDelayOn;
                break;
              case GameOptions.IDs.UI_SHOW_MINIMAP:
                RogueGame.s_Options.IsMinimapOn = !RogueGame.s_Options.IsMinimapOn;
                break;
              case GameOptions.IDs.UI_ADVISOR:
                RogueGame.s_Options.IsAdvisorEnabled = !RogueGame.s_Options.IsAdvisorEnabled;
                break;
              case GameOptions.IDs.UI_COMBAT_ASSISTANT:
                RogueGame.s_Options.IsCombatAssistantOn = !RogueGame.s_Options.IsCombatAssistantOn;
                break;
              case GameOptions.IDs.UI_SHOW_TARGETS:
                RogueGame.s_Options.ShowTargets = !RogueGame.s_Options.ShowTargets;
                break;
              case GameOptions.IDs.UI_SHOW_PLAYER_TARGETS:
                RogueGame.s_Options.ShowPlayerTargets = !RogueGame.s_Options.ShowPlayerTargets;
                break;
              case GameOptions.IDs.GAME_DISTRICT_SIZE:
                RogueGame.s_Options.DistrictSize -= 5;
                break;
              case GameOptions.IDs.GAME_MAX_CIVILIANS:
                RogueGame.s_Options.MaxCivilians -= 5;
                break;
              case GameOptions.IDs.GAME_MAX_DOGS:
                --RogueGame.s_Options.MaxDogs;
                break;
              case GameOptions.IDs.GAME_MAX_UNDEADS:
                RogueGame.s_Options.MaxUndeads -= 10;
                break;
              case GameOptions.IDs.GAME_SIMULATE_DISTRICTS:
                if (RogueGame.s_Options.SimulateDistricts != GameOptions.SimRatio._FIRST)
                {
                  RogueGame.s_Options.SimulateDistricts = RogueGame.s_Options.SimulateDistricts - 1;
                  break;
                }
                break;
              case GameOptions.IDs.GAME_SIMULATE_SLEEP:
                RogueGame.s_Options.SimulateWhenSleeping = !RogueGame.s_Options.SimulateWhenSleeping;
                break;
              case GameOptions.IDs.GAME_SIM_THREAD:
                RogueGame.s_Options.SimThread = !RogueGame.s_Options.SimThread;
                break;
              case GameOptions.IDs.GAME_CITY_SIZE:
                --RogueGame.s_Options.CitySize;
                break;
              case GameOptions.IDs.GAME_NPC_CAN_STARVE_TO_DEATH:
                RogueGame.s_Options.NPCCanStarveToDeath = !RogueGame.s_Options.NPCCanStarveToDeath;
                break;
              case GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE:
                RogueGame.s_Options.ZombificationChance -= 5;
                break;
              case GameOptions.IDs.GAME_REVEAL_STARTING_DISTRICT:
                RogueGame.s_Options.RevealStartingDistrict = !RogueGame.s_Options.RevealStartingDistrict;
                break;
              case GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION:
                RogueGame.s_Options.AllowUndeadsEvolution = this.m_Session.GameMode != GameMode.GM_VINTAGE && !RogueGame.s_Options.AllowUndeadsEvolution;
                break;
              case GameOptions.IDs.GAME_DAY_ZERO_UNDEADS_PERCENT:
                RogueGame.s_Options.DayZeroUndeadsPercent -= 5;
                break;
              case GameOptions.IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE:
                --RogueGame.s_Options.ZombieInvasionDailyIncrease;
                break;
              case GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE:
                RogueGame.s_Options.StarvedZombificationChance -= 5;
                break;
              case GameOptions.IDs.GAME_MAX_REINCARNATIONS:
                --RogueGame.s_Options.MaxReincarnations;
                break;
              case GameOptions.IDs.GAME_REINCARNATE_AS_RAT:
                RogueGame.s_Options.CanReincarnateAsRat = !RogueGame.s_Options.CanReincarnateAsRat;
                break;
              case GameOptions.IDs.GAME_REINCARNATE_TO_SEWERS:
                RogueGame.s_Options.CanReincarnateToSewers = !RogueGame.s_Options.CanReincarnateToSewers;
                break;
              case GameOptions.IDs.GAME_REINC_LIVING_RESTRICTED:
                RogueGame.s_Options.IsLivingReincRestricted = !RogueGame.s_Options.IsLivingReincRestricted;
                break;
              case GameOptions.IDs.GAME_PERMADEATH:
                RogueGame.s_Options.IsPermadeathOn = !RogueGame.s_Options.IsPermadeathOn;
                break;
              case GameOptions.IDs.GAME_DEATH_SCREENSHOT:
                RogueGame.s_Options.IsDeathScreenshotOn = !RogueGame.s_Options.IsDeathScreenshotOn;
                break;
              case GameOptions.IDs.GAME_AGGRESSIVE_HUNGRY_CIVILIANS:
                RogueGame.s_Options.IsAggressiveHungryCiviliansOn = !RogueGame.s_Options.IsAggressiveHungryCiviliansOn;
                break;
              case GameOptions.IDs.GAME_NATGUARD_FACTOR:
                RogueGame.s_Options.NatGuardFactor -= 10;
                break;
              case GameOptions.IDs.GAME_SUPPLIESDROP_FACTOR:
                RogueGame.s_Options.SuppliesDropFactor -= 10;
                break;
              case GameOptions.IDs.GAME_UNDEADS_UPGRADE_DAYS:
                if (RogueGame.s_Options.ZombifiedsUpgradeDays != GameOptions.ZupDays._FIRST)
                {
                  RogueGame.s_Options.ZombifiedsUpgradeDays = RogueGame.s_Options.ZombifiedsUpgradeDays - 1;
                  break;
                }
                break;
              case GameOptions.IDs.GAME_RATS_UPGRADE:
                RogueGame.s_Options.RatsUpgrade = this.m_Session.GameMode != GameMode.GM_VINTAGE && !RogueGame.s_Options.RatsUpgrade;
                break;
              case GameOptions.IDs.GAME_SKELETONS_UPGRADE:
                RogueGame.s_Options.SkeletonsUpgrade = this.m_Session.GameMode != GameMode.GM_VINTAGE && !RogueGame.s_Options.SkeletonsUpgrade;
                break;
              case GameOptions.IDs.GAME_SHAMBLERS_UPGRADE:
                RogueGame.s_Options.ShamblersUpgrade = this.m_Session.GameMode != GameMode.GM_VINTAGE && !RogueGame.s_Options.ShamblersUpgrade;
                break;
            }
            break;
          case Keys.Up:
            if (currentChoice > 0)
            {
              --currentChoice;
              break;
            }
            currentChoice = entries.Length - 1;
            break;
          case Keys.Right:
            switch (idsArray[currentChoice])
            {
              case GameOptions.IDs.UI_MUSIC:
                RogueGame.s_Options.PlayMusic = !RogueGame.s_Options.PlayMusic;
                break;
              case GameOptions.IDs.UI_MUSIC_VOLUME:
                RogueGame.s_Options.MusicVolume += 5;
                break;
              case GameOptions.IDs.UI_SHOW_PLAYER_TAG_ON_MINIMAP:
                RogueGame.s_Options.ShowPlayerTagsOnMinimap = !RogueGame.s_Options.ShowPlayerTagsOnMinimap;
                break;
              case GameOptions.IDs.UI_ANIM_DELAY:
                RogueGame.s_Options.IsAnimDelayOn = !RogueGame.s_Options.IsAnimDelayOn;
                break;
              case GameOptions.IDs.UI_SHOW_MINIMAP:
                RogueGame.s_Options.IsMinimapOn = !RogueGame.s_Options.IsMinimapOn;
                break;
              case GameOptions.IDs.UI_ADVISOR:
                RogueGame.s_Options.IsAdvisorEnabled = !RogueGame.s_Options.IsAdvisorEnabled;
                break;
              case GameOptions.IDs.UI_COMBAT_ASSISTANT:
                RogueGame.s_Options.IsCombatAssistantOn = !RogueGame.s_Options.IsCombatAssistantOn;
                break;
              case GameOptions.IDs.UI_SHOW_TARGETS:
                RogueGame.s_Options.ShowTargets = !RogueGame.s_Options.ShowTargets;
                break;
              case GameOptions.IDs.UI_SHOW_PLAYER_TARGETS:
                RogueGame.s_Options.ShowPlayerTargets = !RogueGame.s_Options.ShowPlayerTargets;
                break;
              case GameOptions.IDs.GAME_DISTRICT_SIZE:
                RogueGame.s_Options.DistrictSize += 5;
                break;
              case GameOptions.IDs.GAME_MAX_CIVILIANS:
                RogueGame.s_Options.MaxCivilians += 5;
                break;
              case GameOptions.IDs.GAME_MAX_DOGS:
                ++RogueGame.s_Options.MaxDogs;
                break;
              case GameOptions.IDs.GAME_MAX_UNDEADS:
                RogueGame.s_Options.MaxUndeads += 10;
                break;
              case GameOptions.IDs.GAME_SIMULATE_DISTRICTS:
                if (RogueGame.s_Options.SimulateDistricts != GameOptions.SimRatio.FULL)
                {
                  RogueGame.s_Options.SimulateDistricts = RogueGame.s_Options.SimulateDistricts + 1;
                  break;
                }
                break;
              case GameOptions.IDs.GAME_SIMULATE_SLEEP:
                RogueGame.s_Options.SimulateWhenSleeping = !RogueGame.s_Options.SimulateWhenSleeping;
                break;
              case GameOptions.IDs.GAME_SIM_THREAD:
                RogueGame.s_Options.SimThread = !RogueGame.s_Options.SimThread;
                break;
              case GameOptions.IDs.GAME_CITY_SIZE:
                ++RogueGame.s_Options.CitySize;
                break;
              case GameOptions.IDs.GAME_NPC_CAN_STARVE_TO_DEATH:
                RogueGame.s_Options.NPCCanStarveToDeath = !RogueGame.s_Options.NPCCanStarveToDeath;
                break;
              case GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE:
                RogueGame.s_Options.ZombificationChance += 5;
                break;
              case GameOptions.IDs.GAME_REVEAL_STARTING_DISTRICT:
                RogueGame.s_Options.RevealStartingDistrict = !RogueGame.s_Options.RevealStartingDistrict;
                break;
              case GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION:
                RogueGame.s_Options.AllowUndeadsEvolution = this.m_Session.GameMode != GameMode.GM_VINTAGE && !RogueGame.s_Options.AllowUndeadsEvolution;
                break;
              case GameOptions.IDs.GAME_DAY_ZERO_UNDEADS_PERCENT:
                RogueGame.s_Options.DayZeroUndeadsPercent += 5;
                break;
              case GameOptions.IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE:
                ++RogueGame.s_Options.ZombieInvasionDailyIncrease;
                break;
              case GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE:
                RogueGame.s_Options.StarvedZombificationChance += 5;
                break;
              case GameOptions.IDs.GAME_MAX_REINCARNATIONS:
                ++RogueGame.s_Options.MaxReincarnations;
                break;
              case GameOptions.IDs.GAME_REINCARNATE_AS_RAT:
                RogueGame.s_Options.CanReincarnateAsRat = !RogueGame.s_Options.CanReincarnateAsRat;
                break;
              case GameOptions.IDs.GAME_REINCARNATE_TO_SEWERS:
                RogueGame.s_Options.CanReincarnateToSewers = !RogueGame.s_Options.CanReincarnateToSewers;
                break;
              case GameOptions.IDs.GAME_REINC_LIVING_RESTRICTED:
                RogueGame.s_Options.IsLivingReincRestricted = !RogueGame.s_Options.IsLivingReincRestricted;
                break;
              case GameOptions.IDs.GAME_PERMADEATH:
                RogueGame.s_Options.IsPermadeathOn = !RogueGame.s_Options.IsPermadeathOn;
                break;
              case GameOptions.IDs.GAME_DEATH_SCREENSHOT:
                RogueGame.s_Options.IsDeathScreenshotOn = !RogueGame.s_Options.IsDeathScreenshotOn;
                break;
              case GameOptions.IDs.GAME_AGGRESSIVE_HUNGRY_CIVILIANS:
                RogueGame.s_Options.IsAggressiveHungryCiviliansOn = !RogueGame.s_Options.IsAggressiveHungryCiviliansOn;
                break;
              case GameOptions.IDs.GAME_NATGUARD_FACTOR:
                RogueGame.s_Options.NatGuardFactor += 10;
                break;
              case GameOptions.IDs.GAME_SUPPLIESDROP_FACTOR:
                RogueGame.s_Options.SuppliesDropFactor += 10;
                break;
              case GameOptions.IDs.GAME_UNDEADS_UPGRADE_DAYS:
                if (RogueGame.s_Options.ZombifiedsUpgradeDays != GameOptions.ZupDays.OFF)
                {
                  RogueGame.s_Options.ZombifiedsUpgradeDays = RogueGame.s_Options.ZombifiedsUpgradeDays + 1;
                  break;
                }
                break;
              case GameOptions.IDs.GAME_RATS_UPGRADE:
                RogueGame.s_Options.RatsUpgrade = this.m_Session.GameMode != GameMode.GM_VINTAGE && !RogueGame.s_Options.RatsUpgrade;
                break;
              case GameOptions.IDs.GAME_SKELETONS_UPGRADE:
                if (this.m_Session.GameMode == GameMode.GM_VINTAGE)
                  RogueGame.s_Options.SkeletonsUpgrade = false;
                RogueGame.s_Options.SkeletonsUpgrade = !RogueGame.s_Options.SkeletonsUpgrade;
                break;
              case GameOptions.IDs.GAME_SHAMBLERS_UPGRADE:
                RogueGame.s_Options.ShamblersUpgrade = this.m_Session.GameMode != GameMode.GM_VINTAGE && !RogueGame.s_Options.ShamblersUpgrade;
                break;
            }
            break;
          case Keys.Down:
            currentChoice = (currentChoice + 1) % entries.Length;
            break;
          case Keys.R:
            RogueGame.s_Options = gameOptions;
            break;
        }
        if (RogueGame.s_Options.SimThread)
          RogueGame.s_Options.SimulateWhenSleeping = false;
        this.ApplyOptions(false);
      }
      while (flag);
      this.SaveOptions();
    }

    private void HandleRedefineKeys()
    {
      bool flag1 = true;
      int currentChoice = 0;
      do
      {
        bool flag2 = RogueGame.s_KeyBindings.CheckForConflict();
        string[] entries = new string[50]
        {
          "Move N",
          "Move NE",
          "Move E",
          "Move SE",
          "Move S",
          "Move SW",
          "Move W",
          "Move NW",
          "Wait",
          "Wait 1 hour",
          "Abandon Game",
          "Advisor Hint",
          "Barricade",
          "Break",
          "Build Large Fortification",
          "Build Small Fortification",
          "City Info",
          "Close",
          "Fire",
          "Give",
          "Help",
          "Hints screen",
          "Initiate Trade",
          "Item 1 slot",
          "Item 2 slot",
          "Item 3 slot",
          "Item 4 slot",
          "Item 5 slot",
          "Item 6 slot",
          "Item 7 slot",
          "Item 8 slot",
          "Item 9 slot",
          "Item 10 slot",
          "Lead",
          "Load Game",
          "Mark Enemies",
          "Messages Log",
          "Options",
          "Order",
          "Push",
          "Quit Game",
          "Redefine Keys",
          "Run",
          "Save Game",
          "Screenshot",
          "Shout",
          "Sleep",
          "Switch Place",
          "Use Exit",
          "Use Spray"
        };
        string[] values = new string[50]
        {
          RogueGame.s_KeyBindings.Get(PlayerCommand.MOVE_N).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.MOVE_NE).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.MOVE_E).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.MOVE_SE).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.MOVE_S).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.MOVE_SW).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.MOVE_W).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.MOVE_NW).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.WAIT_OR_SELF).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.WAIT_LONG).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.ABANDON_GAME).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.ADVISOR).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.BARRICADE_MODE).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.BREAK_MODE).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.BUILD_LARGE_FORTIFICATION).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.BUILD_SMALL_FORTIFICATION).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.CITY_INFO).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.CLOSE_DOOR).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.FIRE_MODE).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.GIVE_ITEM).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.HELP_MODE).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.HINTS_SCREEN_MODE).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.INITIATE_TRADE).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.ITEM_SLOT_0).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.ITEM_SLOT_1).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.ITEM_SLOT_2).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.ITEM_SLOT_3).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.ITEM_SLOT_4).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.ITEM_SLOT_5).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.ITEM_SLOT_6).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.ITEM_SLOT_7).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.ITEM_SLOT_8).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.ITEM_SLOT_9).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.LEAD_MODE).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.LOAD_GAME).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.MARK_ENEMIES_MODE).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.MESSAGE_LOG).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.OPTIONS_MODE).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.ORDER_MODE).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.PUSH_MODE).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.QUIT_GAME).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.KEYBINDING_MODE).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.RUN_TOGGLE).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.SAVE_GAME).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.SCREENSHOT).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.SHOUT).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.SLEEP).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.SWITCH_PLACE).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.USE_EXIT).ToString(),
          RogueGame.s_KeyBindings.Get(PlayerCommand.USE_SPRAY).ToString()
        };
        int gy;
        int gx = gy = 0;
        this.m_UI.UI_Clear(Color.Black);
        this.DrawHeader();
        gy += 14;
        this.m_UI.UI_DrawStringBold(Color.Yellow, "Redefine keys", 0, gy, new Color?());
        gy += 14;
        this.DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGreen, values, gx, ref gy, 256);
        if (flag2)
        {
          this.m_UI.UI_DrawStringBold(Color.Red, "Conflicting keys. Please redefine the keys so the commands don't overlap.", gx, gy, new Color?());
          gy += 14;
        }
        this.DrawFootnote(Color.White, "cursor to move, ENTER to rebind a key, ESC to save and leave");
        this.m_UI.UI_Repaint();
        switch (this.m_UI.UI_WaitKey().KeyCode)
        {
          case Keys.Return:
            this.m_UI.UI_DrawStringBold(Color.Yellow, string.Format("rebinding {0}, press the new key.", (object) entries[currentChoice]), gx, gy, new Color?());
            this.m_UI.UI_Repaint();
            bool flag3 = true;
            Keys key = Keys.None;
            do
            {
              KeyEventArgs keyEventArgs = this.m_UI.UI_WaitKey();
              if (keyEventArgs.KeyCode != Keys.ShiftKey && keyEventArgs.KeyCode != Keys.ControlKey && !keyEventArgs.Alt)
              {
                key = keyEventArgs.KeyData;
                flag3 = false;
              }
            }
            while (flag3);
            PlayerCommand command;
            switch (currentChoice)
            {
              case 0:
                command = PlayerCommand.MOVE_N;
                break;
              case 1:
                command = PlayerCommand.MOVE_NE;
                break;
              case 2:
                command = PlayerCommand.MOVE_E;
                break;
              case 3:
                command = PlayerCommand.MOVE_SE;
                break;
              case 4:
                command = PlayerCommand.MOVE_S;
                break;
              case 5:
                command = PlayerCommand.MOVE_SW;
                break;
              case 6:
                command = PlayerCommand.MOVE_W;
                break;
              case 7:
                command = PlayerCommand.MOVE_NW;
                break;
              case 8:
                command = PlayerCommand.WAIT_OR_SELF;
                break;
              case 9:
                command = PlayerCommand.WAIT_LONG;
                break;
              case 10:
                command = PlayerCommand.ABANDON_GAME;
                break;
              case 11:
                command = PlayerCommand.ADVISOR;
                break;
              case 12:
                command = PlayerCommand.BARRICADE_MODE;
                break;
              case 13:
                command = PlayerCommand.BREAK_MODE;
                break;
              case 14:
                command = PlayerCommand.BUILD_LARGE_FORTIFICATION;
                break;
              case 15:
                command = PlayerCommand.BUILD_SMALL_FORTIFICATION;
                break;
              case 16:
                command = PlayerCommand.CITY_INFO;
                break;
              case 17:
                command = PlayerCommand.CLOSE_DOOR;
                break;
              case 18:
                command = PlayerCommand.FIRE_MODE;
                break;
              case 19:
                command = PlayerCommand.GIVE_ITEM;
                break;
              case 20:
                command = PlayerCommand.HELP_MODE;
                break;
              case 21:
                command = PlayerCommand.HINTS_SCREEN_MODE;
                break;
              case 22:
                command = PlayerCommand.INITIATE_TRADE;
                break;
              case 23:
                command = PlayerCommand.ITEM_SLOT_0;
                break;
              case 24:
                command = PlayerCommand.ITEM_SLOT_1;
                break;
              case 25:
                command = PlayerCommand.ITEM_SLOT_2;
                break;
              case 26:
                command = PlayerCommand.ITEM_SLOT_3;
                break;
              case 27:
                command = PlayerCommand.ITEM_SLOT_4;
                break;
              case 28:
                command = PlayerCommand.ITEM_SLOT_5;
                break;
              case 29:
                command = PlayerCommand.ITEM_SLOT_6;
                break;
              case 30:
                command = PlayerCommand.ITEM_SLOT_7;
                break;
              case 31:
                command = PlayerCommand.ITEM_SLOT_8;
                break;
              case 32:
                command = PlayerCommand.ITEM_SLOT_9;
                break;
              case 33:
                command = PlayerCommand.LEAD_MODE;
                break;
              case 34:
                command = PlayerCommand.LOAD_GAME;
                break;
              case 35:
                command = PlayerCommand.MARK_ENEMIES_MODE;
                break;
              case 36:
                command = PlayerCommand.MESSAGE_LOG;
                break;
              case 37:
                command = PlayerCommand.OPTIONS_MODE;
                break;
              case 38:
                command = PlayerCommand.ORDER_MODE;
                break;
              case 39:
                command = PlayerCommand.PUSH_MODE;
                break;
              case 40:
                command = PlayerCommand.QUIT_GAME;
                break;
              case 41:
                command = PlayerCommand.KEYBINDING_MODE;
                break;
              case 42:
                command = PlayerCommand.RUN_TOGGLE;
                break;
              case 43:
                command = PlayerCommand.SAVE_GAME;
                break;
              case 44:
                command = PlayerCommand.SCREENSHOT;
                break;
              case 45:
                command = PlayerCommand.SHOUT;
                break;
              case 46:
                command = PlayerCommand.SLEEP;
                break;
              case 47:
                command = PlayerCommand.SWITCH_PLACE;
                break;
              case 48:
                command = PlayerCommand.USE_EXIT;
                break;
              case 49:
                command = PlayerCommand.USE_SPRAY;
                break;
              default:
                throw new InvalidOperationException("unhandled selected");
            }
            RogueGame.s_KeyBindings.Set(command, key);
            break;
          case Keys.Escape:
            if (!flag2)
            {
              flag1 = false;
              break;
            }
            break;
          case Keys.Up:
            if (currentChoice > 0)
            {
              --currentChoice;
              break;
            }
            currentChoice = entries.Length - 1;
            break;
          case Keys.Down:
            currentChoice = (currentChoice + 1) % entries.Length;
            break;
        }
      }
      while (flag1);
      this.SaveKeybindings();
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void AdvancePlay(District district, RogueGame.SimFlags sim)
    {
      bool isNight1 = this.m_Session.WorldTime.IsNight;
      DayPhase phase1 = this.m_Session.WorldTime.Phase;
      using (IEnumerator<Map> enumerator = district.Maps.GetEnumerator()) {
        while(enumerator.MoveNext()) {
          Map current = enumerator.Current;
          int turnCounter = current.LocalTime.TurnCounter;
          do {
             AdvancePlay(current, sim);
             if (m_Player.IsDead) HandleReincarnation();
             if (!m_IsGameRunning || m_HasLoadedGame || m_Player.IsDead) return;
          }
          while (current.LocalTime.TurnCounter == turnCounter);
        }
      }
      if (district == this.m_Session.CurrentMap.District)
      {
        ++this.m_Session.WorldTime.TurnCounter;
        bool isNight2 = this.m_Session.WorldTime.IsNight;
        DayPhase phase2 = this.m_Session.WorldTime.Phase;
        if (isNight1 && !isNight2)
        {
          this.AddMessage(new djack.RogueSurvivor.Data.Message("The sun is rising again for you...", this.m_Session.WorldTime.TurnCounter, this.DAY_COLOR));
          this.OnNewDay();
        }
        else if (!isNight1 && isNight2)
        {
          this.AddMessage(new djack.RogueSurvivor.Data.Message("Night is falling upon you...", this.m_Session.WorldTime.TurnCounter, this.NIGHT_COLOR));
          this.OnNewNight();
        }
        else if (phase1 != phase2)
          this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Time passes, it is now {0}...", (object) this.DescribeDayPhase(phase2)), this.m_Session.WorldTime.TurnCounter, isNight2 ? this.NIGHT_COLOR : this.DAY_COLOR));
      }
      if (this.CheckForEvent_ZombieInvasion(district.EntryMap))
        this.FireEvent_ZombieInvasion(district.EntryMap);
      if (this.CheckForEvent_RefugeesWave(district.EntryMap))
        this.FireEvent_RefugeesWave(district);
      if (this.CheckForEvent_NationalGuard(district.EntryMap))
        this.FireEvent_NationalGuard(district.EntryMap);
      if (this.CheckForEvent_ArmySupplies(district.EntryMap))
        this.FireEvent_ArmySupplies(district.EntryMap);
      if (this.CheckForEvent_BikersRaid(district.EntryMap))
        this.FireEvent_BikersRaid(district.EntryMap);
      if (this.CheckForEvent_GangstasRaid(district.EntryMap))
        this.FireEvent_GangstasRaid(district.EntryMap);
      if (this.CheckForEvent_BlackOpsRaid(district.EntryMap))
        this.FireEvent_BlackOpsRaid(district.EntryMap);
      if (this.CheckForEvent_BandOfSurvivors(district.EntryMap))
        this.FireEvent_BandOfSurvivors(district.EntryMap);
      if (this.CheckForEvent_SewersInvasion(district.SewersMap))
        this.FireEvent_SewersInvasion(district.SewersMap);
      if (!RogueGame.s_Options.IsSimON || this.m_Player == null || (!this.m_Player.IsSleeping || !RogueGame.s_Options.SimulateWhenSleeping) || this.m_Player.Location.Map.District != district)
        return;
      this.SimulateNearbyDistricts(district);
    }

    private void NotifyOrderablesAI(Map map, RaidType raid, Point position)
    {
      foreach (Actor actor in map.Actors)
      {
        OrderableAI orderableAi = actor.Controller as OrderableAI;
        if (orderableAi != null)
          orderableAi.OnRaid(raid, new Location(map, position), map.LocalTime.TurnCounter);
      }
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void AdvancePlay(Map map, RogueGame.SimFlags sim)
    {
      if (map.IsSecret)
      {
        ++map.LocalTime.TurnCounter;
      }
      else
      {
        Actor nextActorToAct = this.m_Rules.GetNextActorToAct(map, map.LocalTime.TurnCounter);
        if (nextActorToAct == null)
        {
          this.NextMapTurn(map, sim);
        }
        else
        {
          nextActorToAct.PreviousStaminaPoints = nextActorToAct.StaminaPoints;
          if (nextActorToAct.Controller == null)
            this.SpendActorActionPoints(nextActorToAct, Rules.BASE_ACTION_COST);
          else if (nextActorToAct.IsPlayer)
          {
            this.HandlePlayerActor(nextActorToAct);
            if (!this.m_IsGameRunning || this.m_HasLoadedGame || this.m_Player.IsDead)
              return;
            this.CheckSpecialPlayerEventsAfterAction(nextActorToAct);
          }
          else
            this.HandleAiActor(nextActorToAct);
          nextActorToAct.PreviousHitPoints = nextActorToAct.HitPoints;
          nextActorToAct.PreviousFoodPoints = nextActorToAct.FoodPoints;
          nextActorToAct.PreviousSleepPoints = nextActorToAct.SleepPoints;
          nextActorToAct.PreviousSanity = nextActorToAct.Sanity;
        }
      }
    }

    private void SpendActorActionPoints(Actor actor, int actionCost)
    {
      actor.ActionPoints -= actionCost;
      actor.LastActionTurn = actor.Location.Map.LocalTime.TurnCounter;
    }

    private void SpendActorStaminaPoints(Actor actor, int staminaCost)
    {
      if (actor.Model.Abilities.CanTire)
      {
        if (actor.Location.Map.LocalTime.IsNight && staminaCost > 0)
          staminaCost += this.m_Rules.NightStaminaPenalty(actor);
        if (this.m_Rules.IsActorExhausted(actor))
          staminaCost *= 2;
        actor.StaminaPoints -= staminaCost;
      }
      else
        actor.StaminaPoints = 99;
    }

    private void RegenActorStaminaPoints(Actor actor, int staminaRegen)
    {
      if (actor.Model.Abilities.CanTire)
        actor.StaminaPoints = Math.Min(this.m_Rules.ActorMaxSTA(actor), actor.StaminaPoints + staminaRegen);
      else
        actor.StaminaPoints = 99;
    }

    private void RegenActorHitPoints(Actor actor, int hpRegen)
    {
      actor.HitPoints = Math.Min(this.m_Rules.ActorMaxHPs(actor), actor.HitPoints + hpRegen);
    }

    private void RegenActorSleep(Actor actor, int sleepRegen)
    {
      actor.SleepPoints = Math.Min(this.m_Rules.ActorMaxSleep(actor), actor.SleepPoints + sleepRegen);
    }

    private void SpendActorSanity(Actor actor, int sanCost)
    {
      actor.Sanity -= sanCost;
      if (actor.Sanity >= 0)
        return;
      actor.Sanity = 0;
    }

    private void RegenActorSanity(Actor actor, int sanRegen)
    {
      actor.Sanity = Math.Min(this.m_Rules.ActorMaxSanity(actor), actor.Sanity + sanRegen);
    }

    private void NextMapTurn(Map map, RogueGame.SimFlags sim)
    {
      if ((sim & RogueGame.SimFlags.LODETAIL_TURN) == RogueGame.SimFlags.NOT_SIMULATING)
      {
        bool flag1 = Rules.HasCorpses(this.m_Session.GameMode);
        bool flag2 = Rules.HasInfection(this.m_Session.GameMode);
        if (flag1 || flag2)
        {
          if (flag1 && map.CountCorpses > 0)
          {
            List<Corpse> corpseList1 = new List<Corpse>(map.CountCorpses);
            List<Corpse> corpseList2 = new List<Corpse>(map.CountCorpses);
            foreach (Corpse corpse in map.Corpses)
            {
              if (this.m_Rules.RollChance(this.m_Rules.CorpseZombifyChance(corpse, map.LocalTime, true)))
              {
                corpseList1.Add(corpse);
              }
              else
              {
                this.InflictDamageToCorpse(corpse, Rules.CorpseDecayPerTurn(corpse));
                if ((double) corpse.HitPoints <= 0.0)
                  corpseList2.Add(corpse);
              }
            }
            if (corpseList1.Count > 0)
            {
              List<Corpse> corpseList3 = new List<Corpse>(corpseList1.Count);
              foreach (Corpse corpse in corpseList1)
              {
                if (map.GetActorAt(corpse.Position) == null)
                {
                  float num = corpse.HitPoints / (float) corpse.MaxHitPoints;
                  this.m_Rules.ActorMaxHPs(corpse.DeadGuy);
                  corpseList3.Add(corpse);
                  this.Zombify((Actor) null, corpse.DeadGuy, false);
                  if (this.IsVisibleToPlayer(map, corpse.Position))
                  {
                    this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("The corpse of {0} rise again!!", (object) corpse.DeadGuy.Name), map.LocalTime.TurnCounter, Color.Red));
                    this.m_MusicManager.Play(GameSounds.UNDEAD_RISE);
                  }
                }
              }
              foreach (Corpse c in corpseList3)
                this.DestroyCorpse(c, map);
            }
            if (this.m_Player != null && this.m_Player.Location.Map == map)
            {
              foreach (Corpse c in corpseList2)
              {
                this.DestroyCorpse(c, map);
                if (this.IsVisibleToPlayer(map, c.Position))
                  this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("The corpse of {0} turns into dust.", (object) c.DeadGuy.Name), map.LocalTime.TurnCounter, Color.Purple));
              }
            }
          }
          if (flag2)
          {
            List<Actor> actorList = (List<Actor>) null;
            foreach (Actor actor in map.Actors)
            {
              if (actor.Infection >= Rules.INFECTION_LEVEL_1_WEAK && !actor.Model.Abilities.IsUndead)
              {
                int infectionPercent = this.m_Rules.ActorInfectionPercent(actor);
                if (this.m_Rules.Roll(0, 1000) < this.m_Rules.InfectionEffectTriggerChance1000(infectionPercent))
                {
                  bool player = this.IsVisibleToPlayer(actor);
                  bool flag3 = actor == this.m_Player;
                  if (actor.IsSleeping)
                    this.DoWakeUp(actor);
                  bool flag4 = false;
                  if (infectionPercent >= Rules.INFECTION_LEVEL_5_DEATH)
                    flag4 = true;
                  else if (infectionPercent >= Rules.INFECTION_LEVEL_4_BLEED)
                  {
                    this.DoVomit(actor);
                    actor.HitPoints -= Rules.INFECTION_LEVEL_4_BLEED_HP;
                    if (player)
                    {
                      if (flag3)
                        this.ClearMessages();
                      this.AddMessage(this.MakeMessage(actor, string.Format("{0} blood.", (object) this.Conjugate(actor, this.VERB_VOMIT)), Color.Purple));
                      if (flag3)
                      {
                        this.AddMessagePressEnter();
                        this.ClearMessages();
                      }
                    }
                    if (actor.HitPoints <= 0)
                      flag4 = true;
                  }
                  else if (infectionPercent >= Rules.INFECTION_LEVEL_3_VOMIT)
                  {
                    this.DoVomit(actor);
                    if (player)
                    {
                      if (flag3)
                        this.ClearMessages();
                      this.AddMessage(this.MakeMessage(actor, string.Format("{0}.", (object) this.Conjugate(actor, this.VERB_VOMIT)), Color.Purple));
                      if (flag3)
                      {
                        this.AddMessagePressEnter();
                        this.ClearMessages();
                      }
                    }
                  }
                  else if (infectionPercent >= Rules.INFECTION_LEVEL_2_TIRED)
                  {
                    this.SpendActorStaminaPoints(actor, Rules.INFECTION_LEVEL_2_TIRED_STA);
                    actor.SleepPoints -= Rules.INFECTION_LEVEL_2_TIRED_SLP;
                    if (actor.SleepPoints < 0)
                      actor.SleepPoints = 0;
                    if (player)
                    {
                      if (flag3)
                        this.ClearMessages();
                      this.AddMessage(this.MakeMessage(actor, string.Format("{0} sick and tired.", (object) this.Conjugate(actor, this.VERB_FEEL)), Color.Purple));
                      if (flag3)
                      {
                        this.AddMessagePressEnter();
                        this.ClearMessages();
                      }
                    }
                  }
                  else if (infectionPercent >= Rules.INFECTION_LEVEL_1_WEAK)
                  {
                    this.SpendActorStaminaPoints(actor, Rules.INFECTION_LEVEL_1_WEAK_STA);
                    if (player)
                    {
                      if (flag3)
                        this.ClearMessages();
                      this.AddMessage(this.MakeMessage(actor, string.Format("{0} sick and weak.", (object) this.Conjugate(actor, this.VERB_FEEL)), Color.Purple));
                      if (flag3)
                      {
                        this.AddMessagePressEnter();
                        this.ClearMessages();
                      }
                    }
                  }
                  if (flag4)
                  {
                    if (actorList == null)
                      actorList = new List<Actor>(map.CountActors);
                    actorList.Add(actor);
                  }
                }
              }
            }
            if (actorList != null)
            {
              foreach (Actor actor in actorList)
              {
                if (this.IsVisibleToPlayer(actor))
                  this.AddMessage(this.MakeMessage(actor, string.Format("{0} of infection!", (object) this.Conjugate(actor, this.VERB_DIE))));
                this.KillActor((Actor) null, actor, "infection");
                if (actor.IsPlayer)
                {
                  map.TryRemoveCorpseOf(actor);
                  this.Zombify((Actor) null, actor, false);
                  this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, "turn") + " into a Zombie!"));
                  this.RedrawPlayScreen();
                  AnimDelay(DELAY_LONG);
                }
              }
            }
          }
        }
        List<OdorScent> odorScentList1 = new List<OdorScent>();
        foreach (OdorScent scent in map.Scents)
        {
          switch (scent.Odor)
          {
            case Odor.PERFUME_LIVING_SUPRESSOR:
              odorScentList1.Add(new OdorScent(Odor.LIVING, -scent.Strength, scent.Position));
              continue;
            case Odor.PERFUME_LIVING_GENERATOR:
              odorScentList1.Add(new OdorScent(Odor.LIVING, scent.Strength, scent.Position));
              continue;
            default:
              continue;
          }
        }
        foreach (OdorScent odorScent in odorScentList1)
          map.ModifyScentAt(odorScent.Odor, odorScent.Strength, odorScent.Position);
        List<OdorScent> odorScentList2 = (List<OdorScent>) null;
        foreach (OdorScent scent in map.Scents)
        {
          int num = 1;
          if (map == map.District.SewersMap)
            num += 2;
          else if (map.Lighting == Lighting.OUTSIDE)
          {
            switch (this.m_Session.World.Weather)
            {
              case Weather._FIRST:
              case Weather.CLOUDY:
                break;
              case Weather.RAIN:
                ++num;
                break;
              case Weather.HEAVY_RAIN:
                num += 2;
                break;
              default:
                throw new ArgumentOutOfRangeException("unhandled weather");
            }
          }
          map.ModifyScentAt(scent.Odor, -num, scent.Position);
          if (scent.Strength < 1)
          {
            if (odorScentList2 == null)
              odorScentList2 = new List<OdorScent>(1);
            odorScentList2.Add(scent);
          }
        }
        if (odorScentList2 != null)
        {
          foreach (OdorScent scent in odorScentList2)
            map.RemoveScent(scent);
        }
        foreach (Actor actor in map.Actors)
          this.DropActorScent(actor);
        foreach (Actor actor in map.Actors)
        {
          if (!actor.IsSleeping)
            actor.ActionPoints += this.m_Rules.ActorSpeed(actor);
          if (actor.StaminaPoints < this.m_Rules.ActorMaxSTA(actor))
            this.RegenActorStaminaPoints(actor, 2);
        }
        map.CheckNextActorIndex = 0;
        foreach (Actor actor in map.Actors)
        {
          if (actor.IsRunning && actor.StaminaPoints < 10)
          {
            actor.IsRunning = false;
            if (actor == this.m_Player)
            {
              this.AddMessage(this.MakeMessage(actor, string.Format("{0} too tired to continue running!", (object) this.Conjugate(actor, this.VERB_BE))));
              this.RedrawPlayScreen();
            }
          }
        }
        List<Actor> actorList1 = (List<Actor>) null;
        foreach (Actor actor in map.Actors)
        {
          if (actor.Model.Abilities.HasToEat)
          {
            --actor.FoodPoints;
            if (actor.FoodPoints < 0)
              actor.FoodPoints = 0;
            if (this.m_Rules.IsActorStarving(actor) && this.m_Rules.RollChance(5) && (actor.IsPlayer || RogueGame.s_Options.NPCCanStarveToDeath))
            {
              if (actorList1 == null)
                actorList1 = new List<Actor>();
              actorList1.Add(actor);
            }
          }
          else if (actor.Model.Abilities.IsRotting)
          {
            --actor.FoodPoints;
            if (actor.FoodPoints < 0)
              actor.FoodPoints = 0;
            if (this.m_Rules.IsRottingActorStarving(actor))
            {
              if (m_Rules.Roll(0, 1000) < Rules.ROT_STARVING_HP_CHANCE)
              {
                if (this.IsVisibleToPlayer(actor))
                  this.AddMessage(this.MakeMessage(actor, "is rotting away."));
                if (--actor.HitPoints <= 0)
                {
                  if (actorList1 == null)
                    actorList1 = new List<Actor>();
                  actorList1.Add(actor);
                }
              }
            }
            else if (m_Rules.IsRottingActorHungry(actor) && m_Rules.Roll(0, 1000) < Rules.ROT_HUNGRY_SKILL_CHANCE)
              this.DoLooseRandomSkill(actor);
          }
          if (actor.Model.Abilities.HasToSleep)
          {
            if (actor.IsSleeping)
            {
              if (m_Rules.IsActorDisturbed(actor) && m_Rules.RollChance(Rules.SANITY_NIGHTMARE_CHANCE))
              {
                DoWakeUp(actor);
                DoShout(actor, "NO! LEAVE ME ALONE!");
                actor.SleepPoints -= Rules.SANITY_NIGHTMARE_SLP_LOSS;
                if (actor.SleepPoints < 0)
                  actor.SleepPoints = 0;
                SpendActorSanity(actor, Rules.SANITY_NIGHTMARE_SAN_LOSS);
                if (this.IsVisibleToPlayer(actor))
                  this.AddMessage(this.MakeMessage(actor, string.Format("{0} from a horrible nightmare!", (object) this.Conjugate(actor, this.VERB_WAKE_UP))));
                if (actor.IsPlayer)
                {
                  this.m_MusicManager.StopAll();
                  this.m_MusicManager.Play(GameSounds.NIGHTMARE);
                }
              }
            }
            else
            {
              --actor.SleepPoints;
              if (map.LocalTime.IsNight)
                --actor.SleepPoints;
              if (actor.SleepPoints < 0)
                actor.SleepPoints = 0;
            }
            if (actor.IsSleeping)
            {
              bool isOnCouch = this.m_Rules.IsOnCouch(actor);
              actor.Activity = Activity.SLEEPING;
              int num = this.m_Rules.ActorSleepRegen(actor, isOnCouch);
              actor.SleepPoints += num;
              actor.SleepPoints = Math.Min(actor.SleepPoints, this.m_Rules.ActorMaxSleep(actor));
              if (actor.HitPoints < this.m_Rules.ActorMaxHPs(actor) && this.m_Rules.RollChance((isOnCouch ? Rules.SLEEP_ON_COUCH_HEAL_CHANCE : 0) + this.m_Rules.ActorHealChanceBonus(actor)))
                this.RegenActorHitPoints(actor, 2);
              if (this.m_Rules.IsActorHungry(actor) || actor.SleepPoints >= this.m_Rules.ActorMaxSleep(actor))
                this.DoWakeUp(actor);
              else if (actor.IsPlayer)
              {
                if (this.m_MusicManager.IsPaused(GameMusics.SLEEP))
                  this.m_MusicManager.ResumeLooping(GameMusics.SLEEP);
                this.AddMessage(new djack.RogueSurvivor.Data.Message("...zzZZZzzZ...", map.LocalTime.TurnCounter, Color.DarkCyan));
                this.RedrawPlayScreen();
                if (RogueGame.s_Options.SimThread)
                  Thread.Sleep(10);
              }
              else if (m_Rules.RollChance(MESSAGE_NPC_SLEEP_SNORE_CHANCE) && this.IsVisibleToPlayer(actor))
              {
                this.AddMessage(this.MakeMessage(actor, string.Format("{0}.", (object) this.Conjugate(actor, this.VERB_SNORE))));
                this.RedrawPlayScreen();
              }
            }
            if (m_Rules.IsActorExhausted(actor) && this.m_Rules.RollChance(Rules.SLEEP_EXHAUSTION_COLLAPSE_CHANCE))
            {
              this.DoStartSleeping(actor);
              if (this.IsVisibleToPlayer(actor))
              {
                this.AddMessage(this.MakeMessage(actor, string.Format("{0} from exhaustion !!", (object) this.Conjugate(actor, this.VERB_COLLAPSE))));
                this.RedrawPlayScreen();
              }
              if (actor == this.m_Player)
              {
                this.UpdatePlayerFOV(this.m_Player);
                this.ComputeViewRect(this.m_Player.Location.Position);
                this.RedrawPlayScreen();
              }
            }
          }
          if (actor.Model.Abilities.HasSanity && --actor.Sanity <= 0)
            actor.Sanity = 0;
          if (actor.HasLeader)
          {
            this.ModifyActorTrustInLeader(actor, this.m_Rules.ActorTrustIncrease(actor.Leader), false);
            if (m_Rules.HasActorBondWith(actor, actor.Leader) && m_Rules.RollChance(Rules.SANITY_RECOVER_BOND_CHANCE))
            {
              RegenActorSanity(actor, m_Rules.ActorSanRegenValue(actor, Rules.SANITY_RECOVER_BOND));
              RegenActorSanity(actor.Leader, m_Rules.ActorSanRegenValue(actor.Leader, Rules.SANITY_RECOVER_BOND));
              if (this.IsVisibleToPlayer(actor))
                this.AddMessage(this.MakeMessage(actor, string.Format("{0} reassured knowing {1} is with {2}.", (object) this.Conjugate(actor, this.VERB_FEEL), (object) actor.Leader.Name, (object) this.HimOrHer(actor))));
              if (this.IsVisibleToPlayer(actor.Leader))
                this.AddMessage(this.MakeMessage(actor.Leader, string.Format("{0} reassured knowing {1} is with {2}.", (object) this.Conjugate(actor.Leader, this.VERB_FEEL), (object) actor.Name, (object) this.HimOrHer(actor.Leader))));
            }
          }
        }
        if (actorList1 != null)
        {
          foreach (Actor actor in actorList1)
          {
            if (this.IsVisibleToPlayer(actor))
            {
              this.AddMessage(this.MakeMessage(actor, string.Format("{0} !!", (object) this.Conjugate(actor, this.VERB_DIE_FROM_STARVATION))));
              this.RedrawPlayScreen();
            }
            this.KillActor((Actor) null, actor, "starvation");
            if (!actor.Model.Abilities.IsUndead && Rules.HasImmediateZombification(this.m_Session.GameMode) && this.m_Rules.RollChance(RogueGame.s_Options.StarvedZombificationChance))
            {
              map.TryRemoveCorpseOf(actor);
              this.Zombify((Actor) null, actor, false);
              if (this.IsVisibleToPlayer(actor))
              {
                this.AddMessage(this.MakeMessage(actor, string.Format("{0} into a Zombie!", (object) this.Conjugate(actor, "turn"))));
                this.RedrawPlayScreen();
                AnimDelay(DELAY_LONG);
              }
            }
          }
        }
        foreach (Actor actor in map.Actors)
        {
          Item equippedItem = actor.GetEquippedItem(DollPart.LEFT_HAND);
          if (equippedItem != null)
          {
            ItemLight itemLight = equippedItem as ItemLight;
            if (itemLight != null)
            {
              if (itemLight.Batteries > 0)
              {
                --itemLight.Batteries;
                if (itemLight.Batteries <= 0 && this.IsVisibleToPlayer(actor))
                  this.AddMessage(this.MakeMessage(actor, string.Format(": {0} light goes off.", (object) itemLight.TheName)));
              }
            }
            else
            {
              ItemTracker itemTracker = equippedItem as ItemTracker;
              if (itemTracker != null && itemTracker.Batteries > 0)
              {
                --itemTracker.Batteries;
                if (itemTracker.Batteries <= 0 && this.IsVisibleToPlayer(actor))
                  this.AddMessage(this.MakeMessage(actor, string.Format(": {0} goes off.", (object) itemTracker.TheName)));
              }
            }
          }
        }
        bool flag5 = false;
        foreach (Inventory groundInventory in map.GroundInventories)
        {
          foreach (Item obj in groundInventory.Items)
          {
            ItemPrimedExplosive itemPrimedExplosive = obj as ItemPrimedExplosive;
            if (itemPrimedExplosive != null)
            {
              --itemPrimedExplosive.FuseTimeLeft;
              if (itemPrimedExplosive.FuseTimeLeft <= 0)
                flag5 = true;
            }
          }
        }
        foreach (Actor actor in map.Actors)
        {
          Inventory inventory = actor.Inventory;
          if (inventory != null && !inventory.IsEmpty)
          {
            foreach (Item obj in inventory.Items)
            {
              ItemPrimedExplosive itemPrimedExplosive = obj as ItemPrimedExplosive;
              if (itemPrimedExplosive != null)
              {
                --itemPrimedExplosive.FuseTimeLeft;
                if (itemPrimedExplosive.FuseTimeLeft <= 0)
                  flag5 = true;
              }
            }
          }
        }
        if (flag5)
        {
          bool flag3;
          do
          {
            flag3 = false;
            if (!flag3)
            {
              foreach (Inventory groundInventory in map.GroundInventories)
              {
                Point? inventoryPosition = map.GetGroundInventoryPosition(groundInventory);
                if (!inventoryPosition.HasValue)
                  throw new InvalidOperationException("explosives : GetGroundInventoryPosition returned null point");
                foreach (Item obj in groundInventory.Items)
                {
                  ItemPrimedExplosive itemPrimedExplosive = obj as ItemPrimedExplosive;
                  if (itemPrimedExplosive != null && itemPrimedExplosive.FuseTimeLeft <= 0)
                  {
                    map.RemoveItemAt((Item) itemPrimedExplosive, inventoryPosition.Value);
                    this.DoBlast(new Location(map, inventoryPosition.Value), (itemPrimedExplosive.Model as ItemExplosiveModel).BlastAttack);
                    flag3 = true;
                    break;
                  }
                }
                if (flag3)
                  break;
              }
            }
            if (!flag3)
            {
              foreach (Actor actor in map.Actors)
              {
                Inventory inventory = actor.Inventory;
                if (inventory != null && !inventory.IsEmpty)
                {
                  foreach (Item obj in inventory.Items)
                  {
                    ItemPrimedExplosive itemPrimedExplosive = obj as ItemPrimedExplosive;
                    if (itemPrimedExplosive != null && itemPrimedExplosive.FuseTimeLeft <= 0)
                    {
                      actor.Inventory.RemoveAllQuantity((Item) itemPrimedExplosive);
                      this.DoBlast(new Location(map, actor.Location.Position), (itemPrimedExplosive.Model as ItemExplosiveModel).BlastAttack);
                      flag3 = true;
                      break;
                    }
                  }
                }
              }
            }
          }
          while (flag3);
        }
        if (this.m_Rules.IsWeatherRain(this.m_Session.World.Weather) && this.m_Rules.RollChance(1))
        {
          foreach (MapObject mapObject in map.MapObjects)
          {
            if (mapObject.IsOnFire && this.m_Rules.RollChance(10))
            {
              this.UnapplyOnFire(mapObject);
              if (this.IsVisibleToPlayer(mapObject))
                this.AddMessage(new djack.RogueSurvivor.Data.Message("The rain has put out a fire.", map.LocalTime.TurnCounter));
            }
          }
        }
      }
      if (map.CountTimers > 0)
      {
        List<TimedTask> timedTaskList = (List<TimedTask>) null;
        foreach (TimedTask timer in map.Timers)
        {
          timer.Tick(map);
          if (timer.IsCompleted)
          {
            if (timedTaskList == null)
              timedTaskList = new List<TimedTask>(map.CountTimers);
            timedTaskList.Add(timer);
          }
        }
        if (timedTaskList != null)
        {
          foreach (TimedTask t in timedTaskList)
            map.RemoveTimer(t);
        }
      }
      bool isNight = map.LocalTime.IsNight;
      ++map.LocalTime.TurnCounter;
      bool flag = !map.LocalTime.IsNight;
      if (isNight && flag)
      {
        this.HandleLivingNPCsUpgrade(map);
      }
      else
      {
        if (RogueGame.s_Options.ZombifiedsUpgradeDays == GameOptions.ZupDays.OFF || isNight || (flag || !GameOptions.IsZupDay(RogueGame.s_Options.ZombifiedsUpgradeDays, map.LocalTime.Day)))
          return;
        this.HandleUndeadNPCsUpgrade(map);
      }
    }

    private void DropActorScent(Actor actor)
    {
      if (actor.Model.Abilities.IsUndead)
      {
        if (!actor.Model.Abilities.IsUndeadMaster)
          return;
        actor.Location.Map.RefreshScentAt(Odor.UNDEAD_MASTER, 270, actor.Location.Position);
      }
      else
        actor.Location.Map.RefreshScentAt(Odor.LIVING, 270, actor.Location.Position);
    }

    private void ModifyActorTrustInLeader(Actor a, int mod, bool addMessage)
    {
      a.TrustInLeader += mod;
      if (a.TrustInLeader > Rules.TRUST_MAX)
        a.TrustInLeader = Rules.TRUST_MAX;
      else if (a.TrustInLeader < Rules.TRUST_MIN)
        a.TrustInLeader = Rules.TRUST_MIN;
      if (!addMessage || !a.Leader.IsPlayer)
        return;
      this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("({0} trust with {1})", (object) mod, (object) a.TheName), this.m_Session.WorldTime.TurnCounter, Color.White));
    }

    private int CountLivings(Map map)
    {
      if (map == null)
        throw new ArgumentNullException("map");
      int num = 0;
      foreach (Actor actor in map.Actors)
      {
        if (!actor.Model.Abilities.IsUndead)
          ++num;
      }
      return num;
    }

    private int CountActors(Map map, Predicate<Actor> predFn)
    {
      if (map == null)
        throw new ArgumentNullException("map");
      int num = 0;
      foreach (Actor actor in map.Actors)
      {
        if (predFn(actor))
          ++num;
      }
      return num;
    }

    private int CountFaction(Map map, Faction f)
    {
      if (map == null)
        throw new ArgumentNullException("map");
      int num = 0;
      foreach (Actor actor in map.Actors)
      {
        if (actor.Faction == f)
          ++num;
      }
      return num;
    }

    private int CountUndeads(Map map)
    {
      if (map == null)
        throw new ArgumentNullException("map");
      int num = 0;
      foreach (Actor actor in map.Actors)
      {
        if (actor.Model.Abilities.IsUndead)
          ++num;
      }
      return num;
    }

    private int CountFoodItemsNutrition(Map map)
    {
      if (map == null)
        throw new ArgumentNullException("map");
      int num1 = 0;
      foreach (Inventory groundInventory in map.GroundInventories)
      {
        if (!groundInventory.IsEmpty)
        {
          foreach (Item obj in groundInventory.Items)
          {
            if (obj is ItemFood)
              num1 += this.m_Rules.FoodItemNutrition(obj as ItemFood, map.LocalTime.TurnCounter);
          }
        }
      }
      int num2 = 0;
      foreach (Actor actor in map.Actors)
      {
        Inventory inventory = actor.Inventory;
        if (inventory != null && !inventory.IsEmpty)
        {
          foreach (Item obj in inventory.Items)
          {
            if (obj is ItemFood)
              num2 += this.m_Rules.FoodItemNutrition(obj as ItemFood, map.LocalTime.TurnCounter);
          }
        }
      }
      return num1 + num2;
    }

    private bool HasActorOfModelID(Map map, GameActors.IDs actorModelID)
    {
      if (map == null)
        throw new ArgumentNullException("map");
      foreach (Actor actor in map.Actors)
      {
        if ((GameActors.IDs) actor.Model.ID == actorModelID)
          return true;
      }
      return false;
    }

    private bool CheckForEvent_ZombieInvasion(Map map)
    {
      return map.LocalTime.IsStrikeOfMidnight && this.CountUndeads(map) < RogueGame.s_Options.MaxUndeads;
    }

    private void FireEvent_ZombieInvasion(Map map)
    {
      if (map == this.m_Player.Location.Map && !this.m_Player.IsSleeping && !this.m_Player.Model.Abilities.IsUndead)
      {
        this.AddMessage(new djack.RogueSurvivor.Data.Message("It is Midnight! Zombies are invading!", this.m_Session.WorldTime.TurnCounter, Color.Crimson));
        this.RedrawPlayScreen();
      }
      int num1 = this.CountUndeads(map);
      int num2 = 1 + (int) ((double) Math.Min(1f, (float) (map.LocalTime.Day * RogueGame.s_Options.ZombieInvasionDailyIncrease + RogueGame.s_Options.DayZeroUndeadsPercent) / 100f) * (double) RogueGame.s_Options.MaxUndeads) - num1;
      for (int index = 0; index < num2; ++index)
        this.SpawnNewUndead(map, map.LocalTime.Day);
    }

    private bool CheckForEvent_SewersInvasion(Map map)
    {
      return Rules.HasZombiesInSewers(m_Session.GameMode) && m_Rules.RollChance(SEWERS_INVASION_CHANCE) && CountUndeads(map) < RogueGame.s_Options.MaxUndeads/2;
    }

    private void FireEvent_SewersInvasion(Map map)
    {
      int num1 = this.CountUndeads(map);
      int num2 = 1 + (int) ((double) Math.Min(1f, (float) (map.LocalTime.Day * RogueGame.s_Options.ZombieInvasionDailyIncrease + RogueGame.s_Options.DayZeroUndeadsPercent) / 100f) * (double)(RogueGame.s_Options.MaxUndeads/2)) - num1;
      for (int index = 0; index < num2; ++index)
        this.SpawnNewSewersUndead(map, map.LocalTime.Day);
    }

    private bool CheckForEvent_RefugeesWave(Map map)
    {
      return map.LocalTime.IsStrikeOfMidday;
    }

    private float RefugeesEventDistrictFactor(District d)
    {
      int x = d.WorldPosition.X;
      int y = d.WorldPosition.Y;
      int num1 = this.m_Session.World.Size - 1;
      int num2 = num1 / 2;
      if (x == 0 || y == 0 || (x == num1 || y == num1))
        return 2f;
      return x != num2 || y != num2 ? 1f : 0.5f;
    }

    private void FireEvent_RefugeesWave(District district)
    {
      if (district == this.m_Player.Location.Map.District && !this.m_Player.IsSleeping && !this.m_Player.Model.Abilities.IsUndead)
      {
        this.AddMessage(new djack.RogueSurvivor.Data.Message("A new wave of refugees has arrived!", this.m_Session.WorldTime.TurnCounter, Color.Pink));
        this.RedrawPlayScreen();
      }
      int num1 = this.CountActors(district.EntryMap, (Predicate<Actor>) (a =>
      {
        if (a.Faction != this.GameFactions.TheCivilians)
          return a.Faction == this.GameFactions.ThePolice;
        return true;
      }));
      int num2 = Math.Min(1 + (int)( (RefugeesEventDistrictFactor(district) * (float)RogueGame.s_Options.MaxCivilians)/5f ), RogueGame.s_Options.MaxCivilians - num1);
      for (int index = 0; index < num2; ++index)
        SpawnNewRefugee(!this.m_Rules.RollChance(REFUGEE_SURFACE_SPAWN_CHANCE) ? (!district.HasSubway ? district.SewersMap : (this.m_Rules.RollChance(50) ? district.SubwayMap : district.SewersMap)) : district.EntryMap);
      if (!m_Rules.RollChance(UNIQUE_REFUGEE_CHECK_CHANCE)) return;
      lock (this.m_Session)
      {
        UniqueActor[] local_6 = Array.FindAll<UniqueActor>(this.m_Session.UniqueActors.ToArray(), (Predicate<UniqueActor>) (a =>
        {
          if (a.IsWithRefugees && !a.IsSpawned)
            return !a.TheActor.IsDead;
          return false;
        }));
        if (local_6 == null || local_6.Length <= 0)
          return;
        int local_7 = this.m_Rules.Roll(0, local_6.Length);
        this.FireEvent_UniqueActorArrive(district.EntryMap, local_6[local_7]);
      }
    }

    private void FireEvent_UniqueActorArrive(Map map, UniqueActor unique)
    {
      if (!this.SpawnActorOnMapBorder(map, unique.TheActor, 10, true))
        return;
      unique.IsSpawned = true;
      if (map != this.m_Player.Location.Map || this.m_Player.IsSleeping || this.m_Player.Model.Abilities.IsUndead)
        return;
      if (unique.EventMessage != null)
      {
        if (unique.EventThemeMusic != null)
        {
          this.m_MusicManager.StopAll();
          this.m_MusicManager.Play(unique.EventThemeMusic);
        }
        this.ClearMessages();
        this.AddMessage(new djack.RogueSurvivor.Data.Message(unique.EventMessage, this.m_Session.WorldTime.TurnCounter, Color.Pink));
        this.AddMessage(this.MakePlayerCentricMessage("Seems to come from", unique.TheActor.Location.Position));
        this.AddMessagePressEnter();
        this.ClearMessages();
      }
      this.m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, unique.TheActor.Name + " arrived.");
    }

    private bool CheckForEvent_NationalGuard(Map map)
    {
      if (RogueGame.s_Options.NatGuardFactor == 0 || map.LocalTime.IsNight || (map.LocalTime.Day < NATGUARD_DAY || map.LocalTime.Day >= NATGUARD_END_DAY) || !m_Rules.RollChance(NATGUARD_INTERVENTION_CHANCE))
        return false;
      int num = this.CountLivings(map) + this.CountFaction(map, this.GameFactions.TheArmy);
      return (double) ((float) this.CountUndeads(map) / (float) num) * ((double) RogueGame.s_Options.NatGuardFactor / 100.0) >= NATGUARD_INTERVENTION_FACTOR;
    }

    private void FireEvent_NationalGuard(Map map)
    {
      Actor actor = this.SpawnNewNatGuardLeader(map);
      if (actor == null) return;

      for (int index = 0; index < NATGUARD_SQUAD_SIZE-1; ++index)
        {
        Actor other = SpawnNewNatGuardTrooper(map, actor.Location.Position);
        if (other != null) actor.AddFollower(other);
        }

      NotifyOrderablesAI(map, RaidType.NATGUARD, actor.Location.Position);
      if (map != m_Player.Location.Map) return;
      if (!m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead)
      {
        this.m_MusicManager.StopAll();
        this.m_MusicManager.Play(GameMusics.ARMY);
        this.ClearMessages();
        this.AddMessage(new djack.RogueSurvivor.Data.Message("A National Guard squad has arrived!", this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.AddMessage(this.MakePlayerCentricMessage("Soldiers seem to come from", actor.Location.Position));
        this.AddMessagePressEnter();
        this.ClearMessages();
      }
      m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, "A National Guard squad arrived.");
    }

    private bool CheckForEvent_ArmySupplies(Map map)
    {
      if (RogueGame.s_Options.SuppliesDropFactor == 0 || map.LocalTime.IsNight || (map.LocalTime.Day < ARMY_SUPPLIES_DAY || !m_Rules.RollChance(ARMY_SUPPLIES_CHANCE)))
        return false;
      int num = 1 + this.CountActors(map, (Predicate<Actor>) (a =>
      {
        if (!a.Model.Abilities.IsUndead && a.Model.Abilities.HasToEat)
          return a.Faction == this.GameFactions.TheCivilians;
        return false;
      }));
      return (double) ((float) (1 + this.CountFoodItemsNutrition(map)) / (float) num) < (double) RogueGame.s_Options.SuppliesDropFactor / 100.0 * ARMY_SUPPLIES_FACTOR;
    }

    private void FireEvent_ArmySupplies(Map map)
    {
      Point dropPoint;
      if (!this.FindDropSuppliesPoint(map, out dropPoint))
        return;
      int x1 = dropPoint.X - 1;
      int x2 = dropPoint.X + 1;
      int y1 = dropPoint.Y - 1;
      int y2 = dropPoint.Y + 1;
      map.TrimToBounds(ref x1, ref y1);
      map.TrimToBounds(ref x2, ref y2);
      for (int x3 = x1; x3 <= x2; ++x3)
      {
        for (int y3 = y1; y3 <= y2; ++y3)
        {
          if (this.IsSuitableDropSuppliesPoint(map, x3, y3))
          {
            Item it = this.m_Rules.RollChance(80) ? this.m_TownGenerator.MakeItemArmyRation() : this.m_TownGenerator.MakeItemMedikit();
            map.DropItemAt(it, x3, y3);
          }
        }
      }
      this.NotifyOrderablesAI(map, RaidType.ARMY_SUPLLIES, dropPoint);
      if (map != m_Player.Location.Map) return;
      if (!m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead)
      {
        this.m_MusicManager.StopAll();
        this.m_MusicManager.Play(GameMusics.ARMY);
        this.ClearMessages();
        this.AddMessage(new djack.RogueSurvivor.Data.Message("An Army chopper has dropped supplies!", this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.AddMessage(this.MakePlayerCentricMessage("The drop point seems to be", dropPoint));
        this.AddMessagePressEnter();
        this.ClearMessages();
      }
      m_Session.Scoring.AddEvent(m_Session.WorldTime.TurnCounter, "An army chopper dropped supplies.");
    }

    private bool IsSuitableDropSuppliesPoint(Map map, int x, int y)
    {
      if (!map.IsInBounds(x, y))
        return false;
      Tile tileAt = map.GetTileAt(x, y);
      return !tileAt.IsInside && tileAt.Model.IsWalkable && (map.GetActorAt(x, y) == null && map.GetMapObjectAt(x, y) == null) && this.DistanceToPlayer(map, x, y) >= 10;
    }

    private bool FindDropSuppliesPoint(Map map, out Point dropPoint)
    {
      dropPoint = new Point();
      int num = 4 * map.Width;
      for (int index = 0; index < num; ++index)
      {
        dropPoint.X = this.m_Rules.RollX(map);
        dropPoint.Y = this.m_Rules.RollY(map);
        if (this.IsSuitableDropSuppliesPoint(map, dropPoint.X, dropPoint.Y))
          return true;
      }
      return false;
    }

    private bool HasRaidHappenedSince(RaidType raid, District district, WorldTime mapTime, int sinceNTurns)
    {
      if (this.m_Session.HasRaidHappened(raid, district))
        return mapTime.TurnCounter - this.m_Session.LastRaidTime(raid, district) < sinceNTurns;
      return false;
    }

    private bool CheckForEvent_BikersRaid(Map map)
    {
      return map.LocalTime.Day >= BIKERS_RAID_DAY && map.LocalTime.Day < BIKERS_END_DAY && (!HasRaidHappenedSince(RaidType.BIKERS, map.District, map.LocalTime, BIKERS_RAID_DAYS_GAP * WorldTime.TURNS_PER_DAY) && m_Rules.RollChance(BIKERS_RAID_CHANCE_PER_TURN));
    }

    private void FireEvent_BikersRaid(Map map)
    {
      this.m_Session.SetLastRaidTime(RaidType.BIKERS, map.District, map.LocalTime.TurnCounter);
      GameGangs.IDs gangId = GameGangs.BIKERS[this.m_Rules.Roll(0, GameGangs.BIKERS.Length)];
      Actor actor = this.SpawnNewBikerLeader(map, gangId);
      if (actor == null) return;
      for (int index = 0; index < BIKERS_RAID_SIZE-1; ++index)
      {
        Actor other = SpawnNewBiker(map, gangId, actor.Location.Position);
        if (other != null) actor.AddFollower(other);
      }
      this.NotifyOrderablesAI(map, RaidType.BIKERS, actor.Location.Position);
      if (map != m_Player.Location.Map) return;
      if (!m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead)
      {
        this.m_MusicManager.StopAll();
        this.m_MusicManager.Play(GameMusics.BIKER);
        this.ClearMessages();
        this.AddMessage(new djack.RogueSurvivor.Data.Message("You hear the sound of roaring engines!", this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.AddMessage(this.MakePlayerCentricMessage("Motorbikes seem to come from", actor.Location.Position));
        this.AddMessagePressEnter();
        this.ClearMessages();
      }
      m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, "Bikers raided the district.");
    }

    private bool CheckForEvent_GangstasRaid(Map map)
    {
      return map.LocalTime.Day >= GANGSTAS_RAID_DAY && map.LocalTime.Day < GANGSTAS_END_DAY && (!HasRaidHappenedSince(RaidType.GANGSTA, map.District, map.LocalTime, GANGSTAS_RAID_DAYS_GAP*WorldTime.TURNS_PER_DAY) && m_Rules.RollChance(GANGSTAS_RAID_CHANCE_PER_TURN));
    }

    private void FireEvent_GangstasRaid(Map map)
    {
      this.m_Session.SetLastRaidTime(RaidType.GANGSTA, map.District, map.LocalTime.TurnCounter);
      GameGangs.IDs gangId = GameGangs.GANGSTAS[this.m_Rules.Roll(0, GameGangs.GANGSTAS.Length)];
      Actor actor = this.SpawnNewGangstaLeader(map, gangId);
      if (actor == null) return;
      for (int index = 0; index < GANGSTAS_RAID_SIZE-1; ++index)
      {
        Actor other = this.SpawnNewGangsta(map, gangId, actor.Location.Position);
        if (other != null) actor.AddFollower(other);
      }
      NotifyOrderablesAI(map, RaidType.GANGSTA, actor.Location.Position);
      if (map != m_Player.Location.Map) return;
      if (!m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead)
      {
        this.m_MusicManager.StopAll();
        this.m_MusicManager.Play(GameMusics.GANGSTA);
        this.ClearMessages();
        this.AddMessage(new djack.RogueSurvivor.Data.Message("You hear obnoxious loud music!", this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.AddMessage(this.MakePlayerCentricMessage("Cars seem to come from", actor.Location.Position));
        this.AddMessagePressEnter();
        this.ClearMessages();
      }
      m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, "Gangstas raided the district.");
    }

    private bool CheckForEvent_BlackOpsRaid(Map map)
    {
      return map.LocalTime.Day >= BLACKOPS_RAID_DAY && !HasRaidHappenedSince(RaidType.BLACKOPS, map.District, map.LocalTime, BLACKOPS_RAID_DAY_GAP*WorldTime.TURNS_PER_DAY) && this.m_Rules.RollChance(BLACKOPS_RAID_CHANCE_PER_TURN);
    }

    private void FireEvent_BlackOpsRaid(Map map)
    {
      this.m_Session.SetLastRaidTime(RaidType.BLACKOPS, map.District, map.LocalTime.TurnCounter);
      Actor actor = this.SpawnNewBlackOpsLeader(map);
      if (actor == null) return;
      for (int index = 0; index < BLACKOPS_RAID_SIZE-1; ++index)
      {
        Actor other = this.SpawnNewBlackOpsTrooper(map, actor.Location.Position);
        if (other != null) actor.AddFollower(other);
      }
      NotifyOrderablesAI(map, RaidType.BLACKOPS, actor.Location.Position);
      if (map != m_Player.Location.Map) return;
      if (!m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead)
      {
        this.m_MusicManager.StopAll();
        this.m_MusicManager.Play(GameMusics.ARMY);
        this.ClearMessages();
        this.AddMessage(new djack.RogueSurvivor.Data.Message("You hear a chopper flying over the city!", this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.AddMessage(this.MakePlayerCentricMessage("The chopper has dropped something", actor.Location.Position));
        this.AddMessagePressEnter();
        this.ClearMessages();
      }
      m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, "BlackOps raided the district.");
    }

    private bool CheckForEvent_BandOfSurvivors(Map map)
    {
      return map.LocalTime.Day >= SURVIVORS_BAND_DAY && !HasRaidHappenedSince(RaidType.SURVIVORS, map.District, map.LocalTime, SURVIVORS_BAND_DAY_GAP*WorldTime.TURNS_PER_DAY) && m_Rules.RollChance(SURVIVORS_BAND_CHANCE_PER_TURN);
    }

    private void FireEvent_BandOfSurvivors(Map map)
    {
      this.m_Session.SetLastRaidTime(RaidType.SURVIVORS, map.District, map.LocalTime.TurnCounter);
      Actor actor = this.SpawnNewSurvivor(map);
      if (actor == null) return;
      for (int index = 0; index < SURVIVORS_BAND_SIZE-1; ++index)
        SpawnNewSurvivor(map, actor.Location.Position);
      NotifyOrderablesAI(map, RaidType.SURVIVORS, actor.Location.Position);
      if (map != m_Player.Location.Map) return;
      if (!m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead)
      {
        this.m_MusicManager.StopAll();
        this.m_MusicManager.Play(GameMusics.SURVIVORS);
        this.ClearMessages();
        this.AddMessage(new djack.RogueSurvivor.Data.Message("You hear shooting and honking in the distance.", this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.AddMessage(this.MakePlayerCentricMessage("A van has stopped", actor.Location.Position));
        this.AddMessagePressEnter();
        this.ClearMessages();
      }
      m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, "A Band of Survivors entered the district.");
    }

    private int DistanceToPlayer(Map map, int x, int y)
    {
      if (this.m_Player == null || this.m_Player.Location.Map != map)
        return int.MaxValue;
      return this.m_Rules.GridDistance(this.m_Player.Location.Position, x, y);
    }

    private int DistanceToPlayer(Map map, Point pos)
    {
      return this.DistanceToPlayer(map, pos.X, pos.Y);
    }

    private bool IsAdjacentToEnemy(Map map, Point pos, Actor actor)
    {
      int x1 = pos.X - 1;
      int x2 = pos.X + 1;
      int y1 = pos.Y - 1;
      int y2 = pos.Y + 1;
      map.TrimToBounds(ref x1, ref y1);
      map.TrimToBounds(ref x2, ref y2);
      for (int x3 = x1; x3 <= x2; ++x3)
      {
        for (int y3 = y1; y3 <= y2; ++y3)
        {
          if (x3 != pos.X || y3 != pos.Y)
          {
            Actor actorAt = map.GetActorAt(x3, y3);
            if (actorAt != null && this.m_Rules.IsEnemyOf(actor, actorAt))
              return true;
          }
        }
      }
      return false;
    }

    private bool SpawnActorOnMapBorder(Map map, Actor actorToSpawn, int minDistToPlayer, bool mustBeOutside)
    {
      int num1 = 4 * (map.Width + map.Height);
      int num2 = 0;
      Point point = new Point();
      do
      {
        ++num2;
        int num3 = this.m_Rules.RollChance(50) ? 0 : map.Width - 1;
        int num4 = this.m_Rules.RollChance(50) ? 0 : map.Height - 1;
        if (this.m_Rules.RollChance(50))
          num3 = this.m_Rules.RollX(map);
        else
          num4 = this.m_Rules.RollY(map);
        point.X = num3;
        point.Y = num4;
        if ((!mustBeOutside || !map.GetTileAt(point.X, point.Y).IsInside) && (this.m_Rules.IsWalkableFor(actorToSpawn, map, point.X, point.Y) && this.DistanceToPlayer(map, point) >= minDistToPlayer) && !this.IsAdjacentToEnemy(map, point, actorToSpawn))
        {
          map.PlaceActorAt(actorToSpawn, point);
          this.OnActorEnterTile(actorToSpawn);
          return true;
        }
      }
      while (num2 <= num1);
      return false;
    }

    private bool SpawnActorNear(Map map, Actor actorToSpawn, int minDistToPlayer, Point nearPoint, int maxDistToPoint)
    {
      int num1 = 4 * (map.Width + map.Height);
      int num2 = 0;
      Point p = new Point();
      do
      {
        ++num2;
        int num3 = nearPoint.X + this.m_Rules.Roll(1, maxDistToPoint + 1) - this.m_Rules.Roll(1, maxDistToPoint + 1);
        int num4 = nearPoint.Y + this.m_Rules.Roll(1, maxDistToPoint + 1) - this.m_Rules.Roll(1, maxDistToPoint + 1);
        p.X = num3;
        p.Y = num4;
        map.TrimToBounds(ref p);
        if (!map.GetTileAt(p.X, p.Y).IsInside && this.m_Rules.IsWalkableFor(actorToSpawn, map, p.X, p.Y) && (this.DistanceToPlayer(map, p) >= minDistToPlayer && !this.IsAdjacentToEnemy(map, p, actorToSpawn)))
        {
          map.PlaceActorAt(actorToSpawn, p);
          return true;
        }
      }
      while (num2 <= num1);
      return false;
    }

    private void SpawnNewUndead(Map map, int day)
    {
      Actor newUndead = this.m_TownGenerator.CreateNewUndead(map.LocalTime.TurnCounter);
      if (RogueGame.s_Options.AllowUndeadsEvolution && Rules.HasEvolution(this.m_Session.GameMode))
      {
        int chance = Math.Min(75, day * 2);
        bool flag = false;
        GameActors.IDs fromModelID = (GameActors.IDs) newUndead.Model.ID;
        if (this.m_Rules.RollChance(chance))
        {
          flag = true;
          fromModelID = this.NextUndeadEvolution((GameActors.IDs) newUndead.Model.ID);
          if (this.m_Rules.RollChance(chance))
            fromModelID = this.NextUndeadEvolution(fromModelID);
        }
        if (fromModelID == GameActors.IDs.UNDEAD_ZOMBIE_LORD && day < 7)
          flag = false;
        if (flag)
          newUndead.Model = this.GameActors[fromModelID];
      }
      this.SpawnActorOnMapBorder(map, newUndead, 10, true);
    }

    private void SpawnNewSewersUndead(Map map, int day)
    {
      Actor newSewersUndead = this.m_TownGenerator.CreateNewSewersUndead(map.LocalTime.TurnCounter);
      this.SpawnActorOnMapBorder(map, newSewersUndead, 10, false);
    }

    private void SpawnNewSubwayUndead(Map map, int day)
    {
      Actor newSubwayUndead = this.m_TownGenerator.CreateNewSubwayUndead(map.LocalTime.TurnCounter);
      this.SpawnActorOnMapBorder(map, newSubwayUndead, 10, false);
    }

    private void SpawnNewRefugee(Map map)
    {
      Actor newRefugee = this.m_TownGenerator.CreateNewRefugee(map.LocalTime.TurnCounter, 3);
      this.SpawnActorOnMapBorder(map, newRefugee, 10, true);
    }

    private Actor SpawnNewSurvivor(Map map)
    {
      Actor newSurvivor = this.m_TownGenerator.CreateNewSurvivor(map.LocalTime.TurnCounter);
      if (this.SpawnActorOnMapBorder(map, newSurvivor, 10, true))
        return newSurvivor;
      return (Actor) null;
    }

    private Actor SpawnNewSurvivor(Map map, Point bandPos)
    {
      Actor newSurvivor = this.m_TownGenerator.CreateNewSurvivor(map.LocalTime.TurnCounter);
      if (this.SpawnActorNear(map, newSurvivor, 10, bandPos, 3))
        return newSurvivor;
      return (Actor) null;
    }

    private Actor SpawnNewNatGuardLeader(Map map)
    {
      Actor armyNationalGuard = this.m_TownGenerator.CreateNewArmyNationalGuard(map.LocalTime.TurnCounter, "Sgt");
      this.m_TownGenerator.GiveStartingSkillToActor(armyNationalGuard, Skills.IDs.LEADERSHIP);
      if (map.LocalTime.Day > NATGUARD_ZTRACKER_DAY)
        armyNationalGuard.Inventory.AddAll(this.m_TownGenerator.MakeItemZTracker());
      if (!this.SpawnActorOnMapBorder(map, armyNationalGuard, 10, true))
        return (Actor) null;
      return armyNationalGuard;
    }

    private Actor SpawnNewNatGuardTrooper(Map map, Point leaderPos)
    {
      Actor armyNationalGuard = this.m_TownGenerator.CreateNewArmyNationalGuard(map.LocalTime.TurnCounter, "Pvt");
      if (this.m_Rules.RollChance(50))
        armyNationalGuard.Inventory.AddAll(this.m_TownGenerator.MakeItemCombatKnife());
      else
        armyNationalGuard.Inventory.AddAll(this.m_TownGenerator.MakeItemGrenade());
      if (!this.SpawnActorNear(map, armyNationalGuard, 10, leaderPos, 3))
        return (Actor) null;
      return armyNationalGuard;
    }

    private Actor SpawnNewBikerLeader(Map map, GameGangs.IDs gangId)
    {
      Actor newBikerMan = this.m_TownGenerator.CreateNewBikerMan(map.LocalTime.TurnCounter, gangId);
      this.m_TownGenerator.GiveStartingSkillToActor(newBikerMan, Skills.IDs.LEADERSHIP);
      this.m_TownGenerator.GiveStartingSkillToActor(newBikerMan, Skills.IDs.TOUGH);
      this.m_TownGenerator.GiveStartingSkillToActor(newBikerMan, Skills.IDs.TOUGH);
      this.m_TownGenerator.GiveStartingSkillToActor(newBikerMan, Skills.IDs.TOUGH);
      this.m_TownGenerator.GiveStartingSkillToActor(newBikerMan, Skills.IDs.STRONG);
      this.m_TownGenerator.GiveStartingSkillToActor(newBikerMan, Skills.IDs.STRONG);
      this.m_TownGenerator.GiveStartingSkillToActor(newBikerMan, Skills.IDs.STRONG);
      if (!this.SpawnActorOnMapBorder(map, newBikerMan, 10, true))
        return (Actor) null;
      return newBikerMan;
    }

    private Actor SpawnNewBiker(Map map, GameGangs.IDs gangId, Point leaderPos)
    {
      Actor newBikerMan = this.m_TownGenerator.CreateNewBikerMan(map.LocalTime.TurnCounter, gangId);
      this.m_TownGenerator.GiveStartingSkillToActor(newBikerMan, Skills.IDs.TOUGH);
      this.m_TownGenerator.GiveStartingSkillToActor(newBikerMan, Skills.IDs.STRONG);
      if (!this.SpawnActorNear(map, newBikerMan, 10, leaderPos, 3))
        return (Actor) null;
      return newBikerMan;
    }

    private Actor SpawnNewGangstaLeader(Map map, GameGangs.IDs gangId)
    {
      Actor newGangstaMan = this.m_TownGenerator.CreateNewGangstaMan(map.LocalTime.TurnCounter, gangId);
      this.m_TownGenerator.GiveStartingSkillToActor(newGangstaMan, Skills.IDs.LEADERSHIP);
      this.m_TownGenerator.GiveStartingSkillToActor(newGangstaMan, Skills.IDs._FIRST);
      this.m_TownGenerator.GiveStartingSkillToActor(newGangstaMan, Skills.IDs._FIRST);
      this.m_TownGenerator.GiveStartingSkillToActor(newGangstaMan, Skills.IDs._FIRST);
      this.m_TownGenerator.GiveStartingSkillToActor(newGangstaMan, Skills.IDs.FIREARMS);
      if (!this.SpawnActorOnMapBorder(map, newGangstaMan, 10, true))
        return (Actor) null;
      return newGangstaMan;
    }

    private Actor SpawnNewGangsta(Map map, GameGangs.IDs gangId, Point leaderPos)
    {
      Actor newGangstaMan = this.m_TownGenerator.CreateNewGangstaMan(map.LocalTime.TurnCounter, gangId);
      this.m_TownGenerator.GiveStartingSkillToActor(newGangstaMan, Skills.IDs._FIRST);
      if (!this.SpawnActorNear(map, newGangstaMan, 10, leaderPos, 3))
        return (Actor) null;
      return newGangstaMan;
    }

    private Actor SpawnNewBlackOpsLeader(Map map)
    {
      Actor newBlackOps = this.m_TownGenerator.CreateNewBlackOps(map.LocalTime.TurnCounter, "Officer");
      this.m_TownGenerator.GiveStartingSkillToActor(newBlackOps, Skills.IDs.LEADERSHIP);
      this.m_TownGenerator.GiveStartingSkillToActor(newBlackOps, Skills.IDs._FIRST);
      this.m_TownGenerator.GiveStartingSkillToActor(newBlackOps, Skills.IDs._FIRST);
      this.m_TownGenerator.GiveStartingSkillToActor(newBlackOps, Skills.IDs._FIRST);
      this.m_TownGenerator.GiveStartingSkillToActor(newBlackOps, Skills.IDs.FIREARMS);
      this.m_TownGenerator.GiveStartingSkillToActor(newBlackOps, Skills.IDs.FIREARMS);
      this.m_TownGenerator.GiveStartingSkillToActor(newBlackOps, Skills.IDs.FIREARMS);
      this.m_TownGenerator.GiveStartingSkillToActor(newBlackOps, Skills.IDs.TOUGH);
      this.m_TownGenerator.GiveStartingSkillToActor(newBlackOps, Skills.IDs.TOUGH);
      this.m_TownGenerator.GiveStartingSkillToActor(newBlackOps, Skills.IDs.TOUGH);
      if (!this.SpawnActorOnMapBorder(map, newBlackOps, 10, true))
        return (Actor) null;
      return newBlackOps;
    }

    private Actor SpawnNewBlackOpsTrooper(Map map, Point leaderPos)
    {
      Actor newBlackOps = this.m_TownGenerator.CreateNewBlackOps(map.LocalTime.TurnCounter, "Agent");
      this.m_TownGenerator.GiveStartingSkillToActor(newBlackOps, Skills.IDs._FIRST);
      this.m_TownGenerator.GiveStartingSkillToActor(newBlackOps, Skills.IDs.FIREARMS);
      this.m_TownGenerator.GiveStartingSkillToActor(newBlackOps, Skills.IDs.TOUGH);
      if (!this.SpawnActorNear(map, newBlackOps, 10, leaderPos, 3))
        return (Actor) null;
      return newBlackOps;
    }

    private void UpdatePlayerFOV(Actor player)
    {
      if (player == null)
        return;
      this.m_PlayerFOV = LOS.ComputeFOVFor(this.m_Rules, player, this.m_Session.WorldTime, this.m_Session.World.Weather);
      player.Location.Map.SetViewAndMarkVisited((IEnumerable<Point>) this.m_PlayerFOV);
    }

    private void HandlePlayerActor(Actor player)
    {
      this.UpdatePlayerFOV(player);
      this.m_Player = player;
      this.ComputeViewRect(player.Location.Position);
      this.m_Session.Scoring.TurnsSurvived = this.m_Session.WorldTime.TurnCounter;
      if (this.m_IsPlayerLongWait)
      {
        if (this.CheckPlayerWaitLong(player))
        {
          this.DoWait(player);
          return;
        }
        this.m_IsPlayerLongWait = false;
        this.m_IsPlayerLongWaitForcedStop = false;
        if (this.m_Session.WorldTime.TurnCounter >= this.m_PlayerLongWaitEnd.TurnCounter)
          this.AddMessage(new djack.RogueSurvivor.Data.Message("Wait ended.", this.m_Session.WorldTime.TurnCounter, Color.Yellow));
        else
          this.AddMessage(new djack.RogueSurvivor.Data.Message("Wait interrupted!", this.m_Session.WorldTime.TurnCounter, Color.Red));
      }

      GC.Collect(); // force garbage collection when things should be slow anyway

      bool flag1 = true;
      do
      {
        this.m_UI.UI_SetCursor((Cursor) null);
        if (RogueGame.s_Options.IsAdvisorEnabled && this.HasAdvisorAnyHintToGive())
          this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(new string[1]
          {
            string.Format("HINT AVAILABLE PRESS <{0}>", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.ADVISOR).ToString())
          }, Color.White, Color.White, Color.Black, this.MapToScreen(this.m_Player.Location.Position.X - 3, this.m_Player.Location.Position.Y - 1)));
        this.RedrawPlayScreen();
        this.m_UI.UI_PeekKey();
        bool flag2 = true;
        bool flag3 = false;
        Point mousePosition = this.m_UI.UI_GetMousePosition();
        Point point = new Point(-1, -1);
        MouseButtons? mouseButtons = new MouseButtons?();
        KeyEventArgs key;
        do
        {
          key = this.m_UI.UI_PeekKey();
          if (key != null)
          {
            flag3 = true;
            flag2 = false;
          }
          else
          {
            point = this.m_UI.UI_GetMousePosition();
            mouseButtons = this.m_UI.UI_PeekMouseButtons();
            if (point != mousePosition || mouseButtons.HasValue)
              flag2 = false;
          }
        }
        while (flag2);
        if (flag3)
        {
          PlayerCommand command = InputTranslator.KeyToCommand(key);
          if (command == PlayerCommand.QUIT_GAME)
          {
            if (this.HandleQuitGame())
            {
              this.StopSimThread();
              this.RedrawPlayScreen();
              this.m_IsGameRunning = false;
              return;
            }
          }
          else
          {
            switch (command)
            {
              case PlayerCommand.NONE:
                break;
              case PlayerCommand.HELP_MODE:
                this.HandleHelpMode();
                break;
              case PlayerCommand.ADVISOR:
                this.HandleAdvisor(player);
                break;
              case PlayerCommand.OPTIONS_MODE:
                this.HandleOptions(true);
                this.ApplyOptions(true);
                break;
              case PlayerCommand.KEYBINDING_MODE:
                this.HandleRedefineKeys();
                break;
              case PlayerCommand.HINTS_SCREEN_MODE:
                this.HandleHintsScreen();
                break;
              case PlayerCommand.SCREENSHOT:
                this.HandleScreenshot();
                break;
              case PlayerCommand.SAVE_GAME:
                this.StopSimThread();
                this.HandleSaveGame();
                this.RestartSimThread();
                break;
              case PlayerCommand.LOAD_GAME:
                this.StopSimThread();
                this.HandleLoadGame();
                this.RestartSimThread();
                player = this.m_Player;
                flag1 = false;
                this.m_HasLoadedGame = true;
                break;
              case PlayerCommand.ABANDON_GAME:
                if (this.HandleAbandonGame())
                {
                  this.StopSimThread();
                  flag1 = false;
                  this.KillActor((Actor) null, this.m_Player, "suicide");
                  break;
                }
                break;
              case PlayerCommand.MOVE_N:
                flag1 = !this.TryPlayerInsanity() && !this.DoPlayerBump(player, Direction.N);
                break;
              case PlayerCommand.MOVE_NE:
                flag1 = !this.TryPlayerInsanity() && !this.DoPlayerBump(player, Direction.NE);
                break;
              case PlayerCommand.MOVE_E:
                flag1 = !this.TryPlayerInsanity() && !this.DoPlayerBump(player, Direction.E);
                break;
              case PlayerCommand.MOVE_SE:
                flag1 = !this.TryPlayerInsanity() && !this.DoPlayerBump(player, Direction.SE);
                break;
              case PlayerCommand.MOVE_S:
                flag1 = !this.TryPlayerInsanity() && !this.DoPlayerBump(player, Direction.S);
                break;
              case PlayerCommand.MOVE_SW:
                flag1 = !this.TryPlayerInsanity() && !this.DoPlayerBump(player, Direction.SW);
                break;
              case PlayerCommand.MOVE_W:
                flag1 = !this.TryPlayerInsanity() && !this.DoPlayerBump(player, Direction.W);
                break;
              case PlayerCommand.MOVE_NW:
                flag1 = !this.TryPlayerInsanity() && !this.DoPlayerBump(player, Direction.NW);
                break;
              case PlayerCommand.RUN_TOGGLE:
                if (this.TryPlayerInsanity())
                {
                  flag1 = false;
                  break;
                }
                this.HandlePlayerRunToggle(player);
                break;
              case PlayerCommand.WAIT_OR_SELF:
                if (this.TryPlayerInsanity())
                {
                  flag1 = false;
                  break;
                }
                flag1 = false;
                this.DoWait(player);
                break;
              case PlayerCommand.WAIT_LONG:
                if (this.TryPlayerInsanity())
                {
                  flag1 = false;
                  break;
                }
                flag1 = false;
                this.StartPlayerWaitLong(player);
                break;
              case PlayerCommand.BARRICADE_MODE:
                flag1 = !this.TryPlayerInsanity() && !this.HandlePlayerBarricade(player);
                break;
              case PlayerCommand.BREAK_MODE:
                flag1 = !this.TryPlayerInsanity() && !this.HandlePlayerBreak(player);
                break;
              case PlayerCommand.BUILD_LARGE_FORTIFICATION:
                flag1 = !this.TryPlayerInsanity() && !this.HandlePlayerBuildFortification(player, true);
                break;
              case PlayerCommand.BUILD_SMALL_FORTIFICATION:
                flag1 = !this.TryPlayerInsanity() && !this.HandlePlayerBuildFortification(player, false);
                break;
              case PlayerCommand.CLOSE_DOOR:
                flag1 = !this.TryPlayerInsanity() && !this.HandlePlayerCloseDoor(player);
                break;
              case PlayerCommand.EAT_CORPSE:
                flag1 = !this.TryPlayerInsanity() && !this.HandlePlayerEatCorpse(player, point);
                break;
              case PlayerCommand.FIRE_MODE:
                flag1 = !this.TryPlayerInsanity() && !this.HandlePlayerFireMode(player);
                break;
              case PlayerCommand.GIVE_ITEM:
                flag1 = !this.TryPlayerInsanity() && !this.HandlePlayerGiveItem(player, point);
                break;
              case PlayerCommand.INITIATE_TRADE:
                flag1 = !this.TryPlayerInsanity() && !this.HandlePlayerInitiateTrade(player, point);
                break;
              case PlayerCommand.LEAD_MODE:
                flag1 = !this.TryPlayerInsanity() && !this.HandlePlayerTakeLead(player);
                break;
              case PlayerCommand.MARK_ENEMIES_MODE:
                if (this.TryPlayerInsanity())
                {
                  flag1 = false;
                  break;
                }
                this.HandlePlayerMarkEnemies(player);
                break;
              case PlayerCommand.ORDER_MODE:
                flag1 = !this.TryPlayerInsanity() && !this.HandlePlayerOrderMode(player);
                break;
              case PlayerCommand.PUSH_MODE:
                flag1 = !this.TryPlayerInsanity() && !this.HandlePlayerPush(player);
                break;
              case PlayerCommand.REVIVE_CORPSE:
                flag1 = !this.TryPlayerInsanity() && !this.HandlePlayerReviveCorpse(player, point);
                break;
              case PlayerCommand.SHOUT:
                flag1 = !this.TryPlayerInsanity() && !this.HandlePlayerShout(player, (string) null);
                break;
              case PlayerCommand.SLEEP:
                flag1 = !this.TryPlayerInsanity() && !this.HandlePlayerSleep(player);
                break;
              case PlayerCommand.SWITCH_PLACE:
                flag1 = !this.TryPlayerInsanity() && !this.HandlePlayerSwitchPlace(player);
                break;
              case PlayerCommand.USE_EXIT:
                flag1 = !this.TryPlayerInsanity() && !this.DoUseExit(player, player.Location.Position);
                break;
              case PlayerCommand.USE_SPRAY:
                flag1 = !this.TryPlayerInsanity() && !this.HandlePlayerUseSpray(player);
                break;
              case PlayerCommand.CITY_INFO:
                this.HandleCityInfo();
                break;
              case PlayerCommand.MESSAGE_LOG:
                this.HandleMessageLog();
                break;
              case PlayerCommand.ITEM_SLOT_0:
                flag1 = !this.TryPlayerInsanity() && !this.DoPlayerItemSlot(player, 0, key);
                break;
              case PlayerCommand.ITEM_SLOT_1:
                flag1 = !this.TryPlayerInsanity() && !this.DoPlayerItemSlot(player, 1, key);
                break;
              case PlayerCommand.ITEM_SLOT_2:
                flag1 = !this.TryPlayerInsanity() && !this.DoPlayerItemSlot(player, 2, key);
                break;
              case PlayerCommand.ITEM_SLOT_3:
                flag1 = !this.TryPlayerInsanity() && !this.DoPlayerItemSlot(player, 3, key);
                break;
              case PlayerCommand.ITEM_SLOT_4:
                flag1 = !this.TryPlayerInsanity() && !this.DoPlayerItemSlot(player, 4, key);
                break;
              case PlayerCommand.ITEM_SLOT_5:
                flag1 = !this.TryPlayerInsanity() && !this.DoPlayerItemSlot(player, 5, key);
                break;
              case PlayerCommand.ITEM_SLOT_6:
                flag1 = !this.TryPlayerInsanity() && !this.DoPlayerItemSlot(player, 6, key);
                break;
              case PlayerCommand.ITEM_SLOT_7:
                flag1 = !this.TryPlayerInsanity() && !this.DoPlayerItemSlot(player, 7, key);
                break;
              case PlayerCommand.ITEM_SLOT_8:
                flag1 = !this.TryPlayerInsanity() && !this.DoPlayerItemSlot(player, 8, key);
                break;
              case PlayerCommand.ITEM_SLOT_9:
                flag1 = !this.TryPlayerInsanity() && !this.DoPlayerItemSlot(player, 9, key);
                break;
              default:
                throw new ArgumentException("command unhandled");
            }
          }
        }
        else if (!this.HandleMouseLook(point))
        {
          bool hasDoneAction1;
          if (HandleMouseInventory(point, mouseButtons, out hasDoneAction1))
          {
            if (!hasDoneAction1) continue;
            flag1 = false;
          }
          bool hasDoneAction2;
          if (HandleMouseOverCorpses(point, mouseButtons, out hasDoneAction2))
          {
            if (!hasDoneAction2) continue;
            flag1 = false;
          }
          ClearOverlays();
        }
      }
      while (flag1);
      this.UpdatePlayerFOV(player);
      this.ComputeViewRect(player.Location.Position);
      this.m_Session.LastTurnPlayerActed = this.m_Session.WorldTime.TurnCounter;
    }

    private bool TryPlayerInsanity()
    {
      if (!this.m_Rules.IsActorInsane(this.m_Player) || !this.m_Rules.RollChance(5))
        return false;
      ActorAction insaneAction = this.GenerateInsaneAction(this.m_Player);
      if (insaneAction == null || !insaneAction.IsLegal())
        return false;
      this.ClearMessages();
      this.AddMessage(new djack.RogueSurvivor.Data.Message("(your insanity takes over)", this.m_Player.Location.Map.LocalTime.TurnCounter, Color.Orange));
      this.AddMessagePressEnter();
      insaneAction.Perform();
      return true;
    }

    private bool HandleQuitGame()
    {
      this.AddMessage(this.MakeYesNoMessage("REALLY QUIT GAME"));
      this.RedrawPlayScreen();
      bool flag = this.WaitYesOrNo();
      if (!flag)
        this.AddMessage(new djack.RogueSurvivor.Data.Message("Good. Keep roguing!", this.m_Session.WorldTime.TurnCounter, Color.Yellow));
      else
        this.AddMessage(new djack.RogueSurvivor.Data.Message("Bye!", this.m_Session.WorldTime.TurnCounter, Color.Yellow));
      return flag;
    }

    private bool HandleAbandonGame()
    {
      this.AddMessage(this.MakeYesNoMessage("REALLY KILL YOURSELF"));
      this.RedrawPlayScreen();
      bool flag = this.WaitYesOrNo();
      if (!flag)
        this.AddMessage(new djack.RogueSurvivor.Data.Message("Good. No reason to make the undeads life easier by removing yours!", this.m_Session.WorldTime.TurnCounter, Color.Yellow));
      else
        this.AddMessage(new djack.RogueSurvivor.Data.Message("You can't bear the horror anymore...", this.m_Session.WorldTime.TurnCounter, Color.Yellow));
      return flag;
    }

    private void HandleScreenshot()
    {
      this.AddMessage(new djack.RogueSurvivor.Data.Message("Taking screenshot...", this.m_Session.WorldTime.TurnCounter, Color.Yellow));
      this.RedrawPlayScreen();
      string screenshot = this.DoTakeScreenshot();
      if (screenshot == null)
        this.AddMessage(new djack.RogueSurvivor.Data.Message("Could not save screenshot.", this.m_Session.WorldTime.TurnCounter, Color.Red));
      else
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("screenshot {0} saved.", (object) screenshot), this.m_Session.WorldTime.TurnCounter, Color.Yellow));
      this.RedrawPlayScreen();
    }

    private string DoTakeScreenshot()
    {
      string newScreenshotName = this.GetUserNewScreenshotName();
      if (this.m_UI.UI_SaveScreenshot(this.ScreenshotFilePath(newScreenshotName)) != null)
        return newScreenshotName;
      return (string) null;
    }

    private void HandleHelpMode()
    {
      if (this.m_Manual == null)
      {
        this.m_UI.UI_Clear(Color.Black);
        int gy = 0;
        this.m_UI.UI_DrawStringBold(Color.Red, "Game manual not available ingame.", 0, gy, new Color?());
        int num = gy + 14;
        this.DrawFootnote(Color.White, "press ENTER");
        this.m_UI.UI_Repaint();
        this.WaitEnter();
      }
      else
      {
        bool flag = true;
        List<string> formatedLines = this.m_Manual.FormatedLines;
        do
        {
          this.m_UI.UI_Clear(Color.Black);
          int num1 = 0;
          this.DrawHeader();
          int gy1 = num1 + 14;
          this.m_UI.UI_DrawStringBold(Color.Yellow, "Game Manual", 0, gy1, new Color?());
          int gy2 = gy1 + 14;
          this.m_UI.UI_DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy2, new Color?());
          int gy3 = gy2 + 14;
          int index = this.m_ManualLine;
          do
          {
            if (!(formatedLines[index] == "<SECTION>"))
            {
              this.m_UI.UI_DrawStringBold(Color.LightGray, formatedLines[index], 0, gy3, new Color?());
              gy3 += 14;
            }
            ++index;
          }
          while (index < formatedLines.Count && gy3 < 740);
          this.m_UI.UI_DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy3, new Color?());
          int num2 = gy3 + 14;
          this.DrawFootnote(Color.White, "cursor and PgUp/PgDn to move, numbers to jump to section, ESC to leave");
          this.m_UI.UI_Repaint();
          KeyEventArgs keyEventArgs = this.m_UI.UI_WaitKey();
          int choiceNumber = this.KeyToChoiceNumber(keyEventArgs.KeyCode);
          if (choiceNumber >= 0)
          {
            if (choiceNumber == 0)
            {
              this.m_ManualLine = 0;
            }
            else
            {
              int num3 = this.m_ManualLine;
              int num4 = 0;
              for (this.m_ManualLine = 0; num4 < choiceNumber && this.m_ManualLine < formatedLines.Count; ++this.m_ManualLine)
              {
                if (formatedLines[this.m_ManualLine] == "<SECTION>")
                  ++num4;
              }
              if (this.m_ManualLine >= formatedLines.Count)
                this.m_ManualLine = num3;
            }
          }
          else
          {
            switch (keyEventArgs.KeyCode)
            {
              case Keys.Escape:
                flag = false;
                break;
              case Keys.Prior:
                this.m_ManualLine -= 50;
                break;
              case Keys.Next:
                this.m_ManualLine += 50;
                break;
              case Keys.Up:
                --this.m_ManualLine;
                break;
              case Keys.Down:
                ++this.m_ManualLine;
                break;
            }
          }
          if (this.m_ManualLine < 0)
            this.m_ManualLine = 0;
          if (this.m_ManualLine + 50 >= formatedLines.Count)
            this.m_ManualLine = Math.Max(0, formatedLines.Count - 50);
        }
        while (flag);
      }
    }

    private void HandleHintsScreen()
    {
      this.m_UI.UI_Clear(Color.Black);
      int num1 = 0;
      this.DrawHeader();
      int gy1 = num1 + 14;
      this.m_UI.UI_DrawStringBold(Color.Yellow, "Advisor Hints", 0, gy1, new Color?());
      int gy2 = gy1 + 14;
      this.m_UI.UI_DrawStringBold(Color.White, "preparing...", 0, gy2, new Color?());
      int num2 = gy2 + 14;
      this.m_UI.UI_Repaint();
      List<string> stringList = new List<string>();
      for (int index = 0; index < 46; ++index)
      {
        string title;
        string[] body;
        this.GetAdvisorHintText((AdvisorHint) index, out title, out body);
        stringList.Add(string.Format("HINT {0} : {1}", (object) index, (object) title));
        stringList.AddRange((IEnumerable<string>) body);
        stringList.Add("~~~~");
        stringList.Add("");
      }
      int num3 = 0;
      bool flag = true;
      do
      {
        this.m_UI.UI_Clear(Color.Black);
        int num4 = 0;
        this.DrawHeader();
        int gy3 = num4 + 14;
        this.m_UI.UI_DrawStringBold(Color.Yellow, "Advisor Hints", 0, gy3, new Color?());
        int gy4 = gy3 + 14;
        this.m_UI.UI_DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy4, new Color?());
        int gy5 = gy4 + 14;
        int index = num3;
        do
        {
          this.m_UI.UI_DrawStringBold(Color.LightGray, stringList[index], 0, gy5, new Color?());
          gy5 += 14;
          ++index;
        }
        while (index < stringList.Count && gy5 < 740);
        this.m_UI.UI_DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy5, new Color?());
        num2 = gy5 + 14;
        this.DrawFootnote(Color.White, "cursor and PgUp/PgDn to move, R to reset hints, ESC to leave");
        this.m_UI.UI_Repaint();
        switch (this.m_UI.UI_WaitKey().KeyCode)
        {
          case Keys.Up:
            --num3;
            break;
          case Keys.Down:
            ++num3;
            break;
          case Keys.R:
            RogueGame.s_Hints.ResetAllHints();
            this.m_UI.UI_Clear(Color.Black);
            int num5 = 0;
            this.DrawHeader();
            int gy6 = num5 + 14;
            this.m_UI.UI_DrawStringBold(Color.Yellow, "Advisor Hints", 0, gy6, new Color?());
            this.m_UI.UI_DrawStringBold(Color.White, "Hints reset done.", 0, gy6 + 14, new Color?());
            this.m_UI.UI_Repaint();
            m_UI.UI_Wait(DELAY_LONG);
            break;
          case Keys.Escape:
            flag = false;
            break;
          case Keys.Prior:
            num3 -= 50;
            break;
          case Keys.Next:
            num3 += 50;
            break;
        }
        if (num3 < 0)
          num3 = 0;
        if (num3 + 50 >= stringList.Count)
          num3 = Math.Max(0, stringList.Count - 50);
      }
      while (flag);
    }

    private void HandleMessageLog()
    {
      this.m_UI.UI_Clear(Color.Black);
      int num = 0;
      this.DrawHeader();
      int gy1 = num + 14;
      this.m_UI.UI_DrawStringBold(Color.Yellow, "Message Log", 0, gy1, new Color?());
      int gy2 = gy1 + 14;
      this.m_UI.UI_DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy2, new Color?());
      int gy3 = gy2 + 14;
      foreach (djack.RogueSurvivor.Data.Message message in this.m_MessageManager.History)
      {
        this.m_UI.UI_DrawString(message.Color, message.Text, 0, gy3, new Color?());
        gy3 += 12;
      }
      this.DrawFootnote(Color.White, "press ESC to leave");
      this.m_UI.UI_Repaint();
      this.WaitEscape();
    }

    private void HandleCityInfo()
    {
      int num1;
      int gx = num1 = 0;
      this.m_UI.UI_Clear(Color.Black);
      this.m_UI.UI_DrawStringBold(Color.White, "CITY INFORMATION", num1, num1, new Color?());
      int gy1 = num1 + 28;
      if (this.m_Player.Model.Abilities.IsUndead)
      {
        this.m_UI.UI_DrawStringBold(Color.Red, "You can't remember where you are...", gx, gy1, new Color?());
        int gy2 = gy1 + 14;
        this.m_UI.UI_DrawStringBold(Color.Red, "Must be that rotting brain of yours...", gx, gy2, new Color?());
        int num2 = gy2 + 28;
      }
      else
      {
        this.m_UI.UI_DrawStringBold(Color.White, "> DISTRICTS LAYOUT", gx, gy1, new Color?());
        int num2 = gy1 + 14 + 14;
        for (int index = 0; index < this.m_Session.World.Size; ++index)
        {
          Color color = index == this.m_Player.Location.Map.District.WorldPosition.Y ? Color.LightGreen : Color.White;
          this.m_UI.UI_DrawStringBold(color, index.ToString(), 20, num2 + index * 3 * 14 + 14, new Color?());
          this.m_UI.UI_DrawStringBold(color, ".", 20, num2 + index * 3 * 14, new Color?());
          this.m_UI.UI_DrawStringBold(color, ".", 20, num2 + index * 3 * 14 + 28, new Color?());
        }
        int gy2 = num2 - 14;
        for (int index = 0; index < this.m_Session.World.Size; ++index)
          this.m_UI.UI_DrawStringBold(index == this.m_Player.Location.Map.District.WorldPosition.X ? Color.LightGreen : Color.White, string.Format("..{0}..", (object) (char) (65 + index)), 32 + index * 48, gy2, new Color?());
        int num3 = gy2 + 14;
        int num4 = 32;
        int num5 = num3;
        for (int index1 = 0; index1 < this.m_Session.World.Size; ++index1)
        {
          for (int index2 = 0; index2 < this.m_Session.World.Size; ++index2)
          {
            District district = this.m_Session.World[index2, index1];
            char ch = district == this.m_Session.CurrentMap.District ? '*' : (this.m_Session.Scoring.HasVisited(district.EntryMap) ? '-' : '?');
            Color color;
            string str;
            switch (district.Kind)
            {
              case DistrictKind._FIRST:
                color = Color.Gray;
                str = "Gen";
                break;
              case DistrictKind.RESIDENTIAL:
                color = Color.Orange;
                str = "Res";
                break;
              case DistrictKind.SHOPPING:
                color = Color.White;
                str = "Sho";
                break;
              case DistrictKind.GREEN:
                color = Color.Green;
                str = "Gre";
                break;
              case DistrictKind.BUSINESS:
                color = Color.Red;
                str = "Bus";
                break;
              default:
                throw new ArgumentOutOfRangeException("unhandled district kind");
            }
            string text = "";
            for (int index3 = 0; index3 < 5; ++index3)
              text += ch;
            this.m_UI.UI_DrawStringBold(color, text, num4 + index2 * 48, num5 + index1 * 3 * 14, new Color?());
            this.m_UI.UI_DrawStringBold(color, string.Format("{0}{1}{2}", (object) ch, (object) str, (object) ch), num4 + index2 * 48, num5 + (index1 * 3 + 1) * 14, new Color?());
            this.m_UI.UI_DrawStringBold(color, text, num4 + index2 * 48, num5 + (index1 * 3 + 2) * 14, new Color?());
          }
        }
        int num6 = this.m_Session.World.Size / 2;
        for (int index = 1; index < this.m_Session.World.Size; ++index)
          this.m_UI.UI_DrawStringBold(Color.White, "=", num4 + index * 48 - 8, num5 + num6 * 3 * 14 + 14, new Color?());
        int gy3 = num3 + (this.m_Session.World.Size * 3 + 1) * 14;
        this.m_UI.UI_DrawStringBold(Color.White, "Legend", gx, gy3, new Color?());
        int gy4 = gy3 + 14;
        this.m_UI.UI_DrawString(Color.White, "  *   - current     ?   - unvisited", gx, gy4, new Color?());
        int gy5 = gy4 + 12;
        this.m_UI.UI_DrawString(Color.White, "  Bus - Business    Gen - General    Gre - Green", gx, gy5, new Color?());
        int gy6 = gy5 + 12;
        this.m_UI.UI_DrawString(Color.White, "  Res - Residential Sho - Shopping", gx, gy6, new Color?());
        int gy7 = gy6 + 12;
        this.m_UI.UI_DrawString(Color.White, "  =   - Subway Line", gx, gy7, new Color?());
        int gy8 = gy7 + 12 + 14;
        this.m_UI.UI_DrawStringBold(Color.White, "> NOTABLE LOCATIONS", gx, gy8, new Color?());
        int gy9 = gy8 + 14;
        int num7 = gy9;
        for (int y = 0; y < this.m_Session.World.Size; ++y)
        {
          for (int x = 0; x < this.m_Session.World.Size; ++x)
          {
            Map entryMap = this.m_Session.World[x, y].EntryMap;
            Zone zoneByPartialName1;
            if ((zoneByPartialName1 = entryMap.GetZoneByPartialName("Subway Station")) != null)
            {
              this.m_UI.UI_DrawStringBold(Color.Blue, string.Format("at {0} : {1}.", (object) World.CoordToString(x, y), (object) zoneByPartialName1.Name), gx, gy9, new Color?());
              gy9 += 14;
              if (gy9 >= 740)
              {
                gy9 = num7;
                gx += 350;
              }
            }
            Zone zoneByPartialName2;
            if ((zoneByPartialName2 = entryMap.GetZoneByPartialName("Sewers Maintenance")) != null)
            {
              this.m_UI.UI_DrawStringBold(Color.Green, string.Format("at {0} : {1}.", (object) World.CoordToString(x, y), (object) zoneByPartialName2.Name), gx, gy9, new Color?());
              gy9 += 14;
              if (gy9 >= 740)
              {
                gy9 = num7;
                gx += 350;
              }
            }
            if (entryMap == this.m_Session.UniqueMaps.PoliceStation_OfficesLevel.TheMap.District.EntryMap)
            {
              this.m_UI.UI_DrawStringBold(Color.CadetBlue, string.Format("at {0} : Police Station.", (object) World.CoordToString(x, y)), gx, gy9, new Color?());
              gy9 += 14;
              if (gy9 >= 740)
              {
                gy9 = num7;
                gx += 350;
              }
            }
            if (entryMap == this.m_Session.UniqueMaps.Hospital_Admissions.TheMap.District.EntryMap)
            {
              this.m_UI.UI_DrawStringBold(Color.White, string.Format("at {0} : Hospital.", (object) World.CoordToString(x, y)), gx, gy9, new Color?());
              gy9 += 14;
              if (gy9 >= 740)
              {
                gy9 = num7;
                gx += 350;
              }
            }
            if (this.m_Session.PlayerKnows_CHARUndergroundFacilityLocation && entryMap == this.m_Session.UniqueMaps.CHARUndergroundFacility.TheMap.District.EntryMap)
            {
              this.m_UI.UI_DrawStringBold(Color.Red, string.Format("at {0} : {1}.", (object) World.CoordToString(x, y), (object) this.m_Session.UniqueMaps.CHARUndergroundFacility.TheMap.Name), gx, gy9, new Color?());
              gy9 += 14;
              if (gy9 >= 740)
              {
                gy9 = num7;
                gx += 350;
              }
            }
            if (this.m_Session.PlayerKnows_TheSewersThingLocation && (entryMap == this.m_Session.UniqueActors.TheSewersThing.TheActor.Location.Map.District.EntryMap && !this.m_Session.UniqueActors.TheSewersThing.TheActor.IsDead))
            {
              this.m_UI.UI_DrawStringBold(Color.Red, string.Format("at {0} : The Sewers Thing lives down there.", (object) World.CoordToString(x, y)), gx, gy9, new Color?());
              gy9 += 14;
              if (gy9 >= 740)
              {
                gy9 = num7;
                gx += 350;
              }
            }
          }
        }
      }
      this.DrawFootnote(Color.White, "press ESC to leave");
      this.m_UI.UI_Repaint();
      this.WaitEscape();
    }

    private bool HandleMouseLook(Point mousePos)
    {
      Point map = this.MouseToMap(mousePos);
      if (!this.IsInViewRect(map))
        return false;
      if (!this.m_Session.CurrentMap.IsInBounds(map))
        return true;
      this.ClearOverlays();
      if (this.IsVisibleToPlayer(this.m_Session.CurrentMap, map))
      {
        Point screen = this.MapToScreen(map);
        string[] lines = this.DescribeStuffAt(this.m_Session.CurrentMap, map);
        if (lines != null)
        {
          Point screenPos = new Point(screen.X + 32, screen.Y);
          this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(lines, Color.White, Color.White, this.POPUP_FILLCOLOR, screenPos));
          if (RogueGame.s_Options.ShowTargets)
          {
            Actor actorAt = this.m_Session.CurrentMap.GetActorAt(map);
            if (actorAt != null)
              this.DrawActorTargets(actorAt);
          }
        }
      }
      return true;
    }

    private bool HandleMouseInventory(Point mousePos, MouseButtons? mouseButtons, out bool hasDoneAction)
    {
      Inventory inv;
      Point itemPos;
      Item inventoryItem = this.MouseToInventoryItem(mousePos, out inv, out itemPos);
      if (inv == null)
      {
        hasDoneAction = false;
        return false;
      }
      bool isPlayerInventory = inv == this.m_Player.Inventory;
      hasDoneAction = false;
      this.ClearOverlays();
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Cyan, new Rectangle(itemPos.X, itemPos.Y, 32, 32)));
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Cyan, new Rectangle(itemPos.X + 1, itemPos.Y + 1, 30, 30)));
      if (inventoryItem != null)
      {
        string[] lines = this.DescribeItemLong(inventoryItem, isPlayerInventory);
        int num = 1 + this.FindLongestLine(lines);
        int x = itemPos.X - 7 * num;
        int y = itemPos.Y + 32;
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(lines, Color.White, Color.White, this.POPUP_FILLCOLOR, new Point(x, y)));
        if (mouseButtons.HasValue)
        {
          MouseButtons? nullable1 = mouseButtons;
          if ((nullable1.GetValueOrDefault() != MouseButtons.Left ? 0 : (nullable1.HasValue ? 1 : 0)) != 0)
          {
            hasDoneAction = this.OnLMBItem(inv, inventoryItem);
          }
          else
          {
            MouseButtons? nullable2 = mouseButtons;
            if ((nullable2.GetValueOrDefault() != MouseButtons.Right ? 0 : (nullable2.HasValue ? 1 : 0)) != 0)
              hasDoneAction = this.OnRMBItem(inv, inventoryItem);
          }
        }
      }
      return true;
    }

    private Item MouseToInventoryItem(Point screen, out Inventory inv, out Point itemPos)
    {
      inv = (Inventory) null;
      itemPos = Point.Empty;
      if (this.m_Player == null)
        return (Item) null;
      Inventory inventory = this.m_Player.Inventory;
      Point inventorySlot1 = MouseToInventorySlot(INVENTORYPANEL_X, INVENTORYPANEL_Y, screen.X, screen.Y);
      int index1 = inventorySlot1.X + inventorySlot1.Y * 10;
      if (index1 >= 0 && index1 < inventory.MaxCapacity)
      {
        inv = inventory;
        itemPos = this.InventorySlotToScreen(INVENTORYPANEL_X, INVENTORYPANEL_Y, inventorySlot1.X, inventorySlot1.Y);
        return inventory[index1];
      }
      Inventory itemsAt = this.m_Player.Location.Map.GetItemsAt(this.m_Player.Location.Position);
      Point inventorySlot2 = MouseToInventorySlot(INVENTORYPANEL_X, GROUNDINVENTORYPANEL_Y, screen.X, screen.Y);
      itemPos = InventorySlotToScreen(INVENTORYPANEL_X, GROUNDINVENTORYPANEL_Y, inventorySlot2.X, inventorySlot2.Y);
      if (itemsAt == null)
        return (Item) null;
      int index2 = inventorySlot2.X + inventorySlot2.Y * 10;
      if (index2 < 0 || index2 >= itemsAt.MaxCapacity)
        return (Item) null;
      inv = itemsAt;
      return itemsAt[index2];
    }

    private bool OnLMBItem(Inventory inv, Item it)
    {
      if (inv == this.m_Player.Inventory)
      {
        if (it.IsEquipped)
        {
          string reason;
          if (Rules.CanActorUnequipItem(this.m_Player, it, out reason))
          {
            this.DoUnequipItem(this.m_Player, it);
            return false;
          }
          this.AddMessage(this.MakeErrorMessage(string.Format("Cannot unequip {0} : {1}.", (object) it.TheName, (object) reason)));
          return false;
        }
        if (it.Model.IsEquipable)
        {
          string reason;
          if (this.m_Rules.CanActorEquipItem(this.m_Player, it, out reason))
          {
            this.DoEquipItem(this.m_Player, it);
            return false;
          }
          this.AddMessage(this.MakeErrorMessage(string.Format("Cannot equip {0} : {1}.", (object) it.TheName, (object) reason)));
          return false;
        }
        string reason1;
        if (this.m_Rules.CanActorUseItem(this.m_Player, it, out reason1))
        {
          this.DoUseItem(this.m_Player, it);
          return true;
        }
        this.AddMessage(this.MakeErrorMessage(string.Format("Cannot use {0} : {1}.", (object) it.TheName, (object) reason1)));
        return false;
      }
      string reason2;
      if (this.m_Rules.CanActorGetItem(this.m_Player, it, out reason2))
      {
        this.DoTakeItem(this.m_Player, this.m_Player.Location.Position, it);
        return true;
      }
      this.AddMessage(this.MakeErrorMessage(string.Format("Cannot take {0} : {1}.", (object) it.TheName, (object) reason2)));
      return false;
    }

    private bool OnRMBItem(Inventory inv, Item it)
    {
      if (inv != this.m_Player.Inventory)
        return false;
      string reason;
      if (this.m_Rules.CanActorDropItem(this.m_Player, it, out reason))
      {
        this.DoDropItem(this.m_Player, it);
        return true;
      }
      this.AddMessage(this.MakeErrorMessage(string.Format("Cannot drop {0} : {1}.", (object) it.TheName, (object) reason)));
      return false;
    }

    private bool HandleMouseOverCorpses(Point mousePos, MouseButtons? mouseButtons, out bool hasDoneAction)
    {
      Point corpsePos;
      Corpse corpse = this.MouseToCorpse(mousePos, out corpsePos);
      if (corpse == null)
      {
        hasDoneAction = false;
        return false;
      }
      hasDoneAction = false;
      this.ClearOverlays();
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Cyan, new Rectangle(corpsePos.X, corpsePos.Y, 32, 32)));
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Cyan, new Rectangle(corpsePos.X + 1, corpsePos.Y + 1, 30, 30)));
      if (corpse != null)
      {
        string[] lines = this.DescribeCorpseLong(corpse, true);
        int num = 1 + this.FindLongestLine(lines);
        int x = corpsePos.X - 7 * num;
        int y = corpsePos.Y + 32;
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(lines, Color.White, Color.White, this.POPUP_FILLCOLOR, new Point(x, y)));
        if (mouseButtons.HasValue)
        {
          MouseButtons? nullable1 = mouseButtons;
          if ((nullable1.GetValueOrDefault() != MouseButtons.Left ? 0 : (nullable1.HasValue ? 1 : 0)) != 0)
          {
            hasDoneAction = this.OnLMBCorpse(corpse);
          }
          else
          {
            MouseButtons? nullable2 = mouseButtons;
            if ((nullable2.GetValueOrDefault() != MouseButtons.Right ? 0 : (nullable2.HasValue ? 1 : 0)) != 0)
              hasDoneAction = this.OnRMBCorpse(corpse);
          }
        }
      }
      return true;
    }

    private Corpse MouseToCorpse(Point screen, out Point corpsePos)
    {
      corpsePos = Point.Empty;
      if (this.m_Player == null)
        return (Corpse) null;
      List<Corpse> corpsesAt = this.m_Player.Location.Map.GetCorpsesAt(this.m_Player.Location.Position);
      if (corpsesAt == null)
        return (Corpse) null;
      Point inventorySlot = MouseToInventorySlot(INVENTORYPANEL_X, CORPSESPANEL_Y, screen.X, screen.Y);
      corpsePos = InventorySlotToScreen(INVENTORYPANEL_X, CORPSESPANEL_Y, inventorySlot.X, inventorySlot.Y);
      int index = inventorySlot.X + inventorySlot.Y * 10;
      if (index >= 0 && index < corpsesAt.Count)
        return corpsesAt[index];
      return (Corpse) null;
    }

    private bool OnLMBCorpse(Corpse c)
    {
      if (c.IsDragged)
      {
        string reason;
        if (this.m_Rules.CanActorStopDragCorpse(this.m_Player, c, out reason))
        {
          this.DoStopDragCorpse(this.m_Player, c);
          return false;
        }
        this.AddMessage(this.MakeErrorMessage(string.Format("Cannot stop dragging {0} corpse : {1}.", (object) c.DeadGuy.Name, (object) reason)));
        return false;
      }
      string reason1;
      if (this.m_Rules.CanActorStartDragCorpse(this.m_Player, c, out reason1))
      {
        this.DoStartDragCorpse(this.m_Player, c);
        return false;
      }
      this.AddMessage(this.MakeErrorMessage(string.Format("Cannot start dragging {0} corpse : {1}.", (object) c.DeadGuy.Name, (object) reason1)));
      return false;
    }

    private bool OnRMBCorpse(Corpse c)
    {
      if (this.m_Player.Model.Abilities.IsUndead)
      {
        string reason;
        if (this.m_Rules.CanActorEatCorpse(this.m_Player, c, out reason))
        {
          this.DoEatCorpse(this.m_Player, c);
          return true;
        }
        this.AddMessage(this.MakeErrorMessage(string.Format("Cannot eat {0} corpse : {1}.", (object) c.DeadGuy.Name, (object) reason)));
        return false;
      }
      string reason1;
      if (this.m_Rules.CanActorButcherCorpse(this.m_Player, c, out reason1))
      {
        this.DoButcherCorpse(this.m_Player, c);
        return true;
      }
      this.AddMessage(this.MakeErrorMessage(string.Format("Cannot butcher {0} corpse : {1}.", (object) c.DeadGuy.Name, (object) reason1)));
      return false;
    }

    private bool HandlePlayerEatCorpse(Actor player, Point mousePos)
    {
      Point corpsePos;
      Corpse corpse = this.MouseToCorpse(mousePos, out corpsePos);
      if (corpse == null)
        return false;
      string reason;
      if (!this.m_Rules.CanActorEatCorpse(player, corpse, out reason))
      {
        this.AddMessage(this.MakeErrorMessage(string.Format("Cannot eat {0} corpse : {1}.", (object) corpse.DeadGuy.Name, (object) reason)));
        return false;
      }
      this.DoEatCorpse(player, corpse);
      return true;
    }

    private bool HandlePlayerReviveCorpse(Actor player, Point mousePos)
    {
      Point corpsePos;
      Corpse corpse = this.MouseToCorpse(mousePos, out corpsePos);
      if (corpse == null)
        return false;
      string reason;
      if (!this.m_Rules.CanActorReviveCorpse(player, corpse, out reason))
      {
        this.AddMessage(this.MakeErrorMessage(string.Format("Cannot revive {0} : {1}.", (object) corpse.DeadGuy.Name, (object) reason)));
        return false;
      }
      this.DoReviveCorpse(player, corpse);
      return true;
    }

    public void DoStartDragCorpse(Actor a, Corpse c)
    {
      c.DraggedBy = a;
      a.DraggedCorpse = c;
      if (!this.IsVisibleToPlayer(a))
        return;
      this.AddMessage(this.MakeMessage(a, string.Format("{0} dragging {1} corpse.", (object) this.Conjugate(a, this.VERB_START), (object) c.DeadGuy.Name)));
    }

    public void DoStopDragCorpse(Actor a, Corpse c)
    {
      c.DraggedBy = (Actor) null;
      a.DraggedCorpse = (Corpse) null;
      if (!this.IsVisibleToPlayer(a))
        return;
      this.AddMessage(this.MakeMessage(a, string.Format("{0} dragging {1} corpse.", (object) this.Conjugate(a, this.VERB_STOP), (object) c.DeadGuy.Name)));
    }

    public void DoStopDraggingCorpses(Actor a)
    {
      if (a.DraggedCorpse == null)
        return;
      this.DoStopDragCorpse(a, a.DraggedCorpse);
    }

    public void DoButcherCorpse(Actor a, Corpse c)
    {
      bool player = this.IsVisibleToPlayer(a);
      SpendActorActionPoints(a, Rules.BASE_ACTION_COST);
      SeeingCauseInsanity(a, a.Location, Rules.SANITY_HIT_BUTCHERING_CORPSE, string.Format("{0} butchering {1}", (object) a.Name, (object) c.DeadGuy.Name));
      int num = this.m_Rules.ActorDamageVsCorpses(a);
      if (player)
        this.AddMessage(this.MakeMessage(a, string.Format("{0} {1} corpse for {2} damage.", (object) this.Conjugate(a, this.VERB_BUTCHER), (object) c.DeadGuy.Name, (object) num)));
      this.InflictDamageToCorpse(c, (float) num);
      if ((double) c.HitPoints > 0.0)
        return;
      this.DestroyCorpse(c, a.Location.Map);
      if (!player)
        return;
      this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0} corpse is no more.", (object) c.DeadGuy.Name), a.Location.Map.LocalTime.TurnCounter, Color.Purple));
    }

    public void DoEatCorpse(Actor a, Corpse c)
    {
      bool player = this.IsVisibleToPlayer(a);
      SpendActorActionPoints(a, Rules.BASE_ACTION_COST);
      int num = this.m_Rules.ActorDamageVsCorpses(a);
      if (player)
      {
        this.AddMessage(this.MakeMessage(a, string.Format("{0} {1} corpse.", (object) this.Conjugate(a, this.VERB_FEAST_ON), (object) c.DeadGuy.Name)));
        this.m_MusicManager.Play(GameSounds.UNDEAD_EAT);
      }
      this.InflictDamageToCorpse(c, (float) num);
      if ((double) c.HitPoints <= 0.0)
      {
        this.DestroyCorpse(c, a.Location.Map);
        if (player)
          this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0} corpse is no more.", (object) c.DeadGuy.Name), a.Location.Map.LocalTime.TurnCounter, Color.Purple));
      }
      if (a.Model.Abilities.IsUndead)
      {
        this.RegenActorHitPoints(a, this.Rules.ActorBiteHpRegen(a, num));
        a.FoodPoints = Math.Min(a.FoodPoints + this.m_Rules.ActorBiteNutritionValue(a, num), this.m_Rules.ActorMaxRot(a));
      }
      else
      {
        a.FoodPoints = Math.Min(a.FoodPoints + this.m_Rules.ActorBiteNutritionValue(a, num), this.m_Rules.ActorMaxFood(a));
        this.InfectActor(a, this.m_Rules.CorpseEeatingInfectionTransmission(c.DeadGuy.Infection));
      }
      this.SeeingCauseInsanity(a, a.Location, a.Model.Abilities.IsUndead ? Rules.SANITY_HIT_UNDEAD_EATING_CORPSE : Rules.SANITY_HIT_LIVING_EATING_CORPSE, string.Format("{0} eating {1}", (object) a.Name, (object) c.DeadGuy.Name));
    }

    public void DoReviveCorpse(Actor actor, Corpse corpse)
    {
      bool player = this.IsVisibleToPlayer(actor);
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      Map map = actor.Location.Map;
      List<Point> pointList = actor.Location.Map.FilterAdjacentInMap(actor.Location.Position, (Predicate<Point>) (pt => map.GetActorAt(pt) == null && map.GetMapObjectAt(pt) == null));
      if (pointList == null)
      {
        if (!player)
          return;
        this.AddMessage(this.MakeMessage(actor, string.Format("{0} not enough room for reviving {1}.", (object) this.Conjugate(actor, this.VERB_HAVE), (object) corpse.DeadGuy.Name)));
      }
      else
      {
        Point position = pointList[this.m_Rules.Roll(0, pointList.Count)];
        int chance = this.m_Rules.CorpseReviveChance(actor, corpse);
        Item firstMatching = actor.Inventory.GetFirstMatching((Predicate<Item>) (it => it.Model == this.GameItems.MEDIKIT));
        actor.Inventory.Consume(firstMatching);
        if (this.m_Rules.RollChance(chance))
        {
          corpse.DeadGuy.IsDead = false;
          corpse.DeadGuy.HitPoints = this.m_Rules.CorpseReviveHPs(actor, corpse);
          corpse.DeadGuy.Doll.RemoveDecoration("Actors\\Decoration\\bloodied");
          corpse.DeadGuy.Activity = Activity.IDLE;
          corpse.DeadGuy.TargetActor = (Actor) null;
          map.RemoveCorpse(corpse);
          map.PlaceActorAt(corpse.DeadGuy, position);
          if (player)
            this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_REVIVE), corpse.DeadGuy));
          if (!this.m_Rules.IsEnemyOf(actor, corpse.DeadGuy))
            this.DoSay(corpse.DeadGuy, actor, "Thank you, you saved my life!", RogueGame.Sayflags.NONE);
          if (this.m_Rules.IsEnemyOf(actor, corpse.DeadGuy))
            return;
          corpse.DeadGuy.AddTrustIn(actor, Rules.TRUST_REVIVE_BONUS);
        }
        else
        {
          if (!player)
            return;
          this.AddMessage(this.MakeMessage(actor, string.Format("{0} to revive", (object) this.Conjugate(actor, this.VERB_FAIL)), corpse.DeadGuy));
        }
      }
    }

    private void InflictDamageToCorpse(Corpse c, float dmg)
    {
      c.HitPoints -= dmg;
    }

    private void DestroyCorpse(Corpse c, Map m)
    {
      if (c.DraggedBy != null)
      {
        c.DraggedBy.DraggedCorpse = (Corpse) null;
        c.DraggedBy = (Actor) null;
      }
      m.RemoveCorpse(c);
    }

    private bool DoPlayerItemSlot(Actor player, int slot, KeyEventArgs key)
    {
      if ((key.Modifiers & Keys.Control) != Keys.None)
        return this.DoPlayerItemSlotUse(player, slot);
      if (key.Shift)
        return this.DoPlayerItemSlotTake(player, slot);
      if (key.Alt)
        return this.DoPlayerItemSlotDrop(player, slot);
      return false;
    }

    private bool DoPlayerItemSlotUse(Actor player, int slot)
    {
      Item it = player.Inventory[slot];
      if (it == null)
      {
        this.AddMessage(this.MakeErrorMessage(string.Format("No item at inventory slot {0}.", (object) (slot + 1))));
        return false;
      }
      if (it.IsEquipped)
      {
        string reason;
        if (Rules.CanActorUnequipItem(player, it, out reason))
        {
          this.DoUnequipItem(player, it);
          return false;
        }
        this.AddMessage(this.MakeErrorMessage(string.Format("Cannot unequip {0} : {1}.", (object) it.TheName, (object) reason)));
        return false;
      }
      if (it.Model.IsEquipable)
      {
        string reason;
        if (this.m_Rules.CanActorEquipItem(player, it, out reason))
        {
          this.DoEquipItem(player, it);
          return false;
        }
        this.AddMessage(this.MakeErrorMessage(string.Format("Cannot equip {0} : {1}.", (object) it.TheName, (object) reason)));
        return false;
      }
      string reason1;
      if (this.m_Rules.CanActorUseItem(player, it, out reason1))
      {
        this.DoUseItem(player, it);
        return true;
      }
      this.AddMessage(this.MakeErrorMessage(string.Format("Cannot use {0} : {1}.", (object) it.TheName, (object) reason1)));
      return false;
    }

    private bool DoPlayerItemSlotTake(Actor player, int slot)
    {
      Inventory itemsAt = player.Location.Map.GetItemsAt(player.Location.Position);
      if (itemsAt == null || itemsAt.IsEmpty)
      {
        this.AddMessage(this.MakeErrorMessage("No items on ground."));
        return false;
      }
      Item it = itemsAt[slot];
      if (it == null)
      {
        this.AddMessage(this.MakeErrorMessage(string.Format("No item at ground slot {0}.", (object) (slot + 1))));
        return false;
      }
      string reason;
      if (this.m_Rules.CanActorGetItem(player, it, out reason))
      {
        this.DoTakeItem(player, player.Location.Position, it);
        return true;
      }
      this.AddMessage(this.MakeErrorMessage(string.Format("Cannot take {0} : {1}.", (object) it.TheName, (object) reason)));
      return false;
    }

    private bool DoPlayerItemSlotDrop(Actor player, int slot)
    {
      Item it = player.Inventory[slot];
      if (it == null)
      {
        this.AddMessage(this.MakeErrorMessage(string.Format("No item at inventory slot {0}.", (object) (slot + 1))));
        return false;
      }
      string reason;
      if (this.m_Rules.CanActorDropItem(player, it, out reason))
      {
        this.DoDropItem(player, it);
        return true;
      }
      this.AddMessage(this.MakeErrorMessage(string.Format("Cannot drop {0} : {1}.", (object) it.TheName, (object) reason)));
      return false;
    }

    private bool HandlePlayerShout(Actor player, string text)
    {
      string reason;
      if (!this.m_Rules.CanActorShout(player, out reason))
      {
        this.AddMessage(this.MakeErrorMessage(string.Format("Can't shout : {0}.", (object) reason)));
        return false;
      }
      this.DoShout(player, text);
      return true;
    }

    private bool HandlePlayerGiveItem(Actor player, Point screen)
    {
      Inventory inv;
      Point itemPos;
      Item inventoryItem = this.MouseToInventoryItem(screen, out inv, out itemPos);
      if (inv == null || inv != player.Inventory || inventoryItem == null) return false;
      bool flag1 = true;
      bool flag2 = false;
      this.ClearOverlays();
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.GIVE_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
      do
      {
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Giving {0} to...", (object) inventoryItem.TheName), this.m_Session.WorldTime.TurnCounter, Color.Yellow));
        this.RedrawPlayScreen();
        Direction direction = this.WaitDirectionOrCancel();
        if (direction == null)
          flag1 = false;
        else if (direction != Direction.NEUTRAL)
        {
          Point point = player.Location.Position + direction;
          if (player.Location.Map.IsInBounds(point))
          {
            Actor actorAt = player.Location.Map.GetActorAt(point);
            if (actorAt != null)
            {
              string reason;
              if (this.m_Rules.CanActorGiveItemTo(player, actorAt, inventoryItem, out reason))
              {
                flag2 = true;
                flag1 = false;
                this.DoGiveItemTo(player, actorAt, inventoryItem);
              }
              else
                this.AddMessage(this.MakeErrorMessage(string.Format("Can't give {0} to {1} : {2}.", (object) inventoryItem.TheName, (object) actorAt.TheName, (object) reason)));
            }
            else
              this.AddMessage(this.MakeErrorMessage("Noone there."));
          }
        }
      }
      while (flag1);
      this.ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerInitiateTrade(Actor player, Point screen)
    {
      Inventory inv;
      Point itemPos;
      Item inventoryItem = this.MouseToInventoryItem(screen, out inv, out itemPos);
      if (inv == null || inv != player.Inventory || inventoryItem == null) return false;
      bool flag1 = true;
      bool flag2 = false;
      this.ClearOverlays();
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.INITIATE_TRADE_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
      do
      {
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Trading {0} with...", (object) inventoryItem.TheName), this.m_Session.WorldTime.TurnCounter, Color.Yellow));
        this.RedrawPlayScreen();
        Direction direction = this.WaitDirectionOrCancel();
        if (direction == null)
          flag1 = false;
        else if (direction != Direction.NEUTRAL)
        {
          Point point = player.Location.Position + direction;
          if (player.Location.Map.IsInBounds(point))
          {
            Actor actorAt = player.Location.Map.GetActorAt(point);
            if (actorAt != null)
            {
              string reason;
              if (this.m_Rules.CanActorInitiateTradeWith(player, actorAt, out reason))
              {
                flag2 = true;
                flag1 = false;
                this.ClearOverlays();
                this.RedrawPlayScreen();
                this.DoTrade(player, inventoryItem, actorAt, true);
              }
              else
                this.AddMessage(this.MakeErrorMessage(string.Format("Can't trade with {0} : {1}.", (object) actorAt.TheName, (object) reason)));
            }
            else
              this.AddMessage(this.MakeErrorMessage("Noone there."));
          }
        }
      }
      while (flag1);
      this.ClearOverlays();
      return flag2;
    }

    private void HandlePlayerRunToggle(Actor player)
    {
      string reason;
      if (!this.m_Rules.CanActorRun(player, out reason))
      {
        this.AddMessage(this.MakeErrorMessage(string.Format("Cannot run now : {0}.", (object) reason)));
      }
      else
      {
        player.IsRunning = !player.IsRunning;
        this.AddMessage(this.MakeMessage(player, string.Format("{0} running.", (object) this.Conjugate(player, player.IsRunning ? this.VERB_START : this.VERB_STOP))));
      }
    }

    private bool HandlePlayerCloseDoor(Actor player)
    {
      bool flag1 = true;
      bool flag2 = false;
      this.ClearOverlays();
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.CLOSE_DOOR_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
      do
      {
        this.RedrawPlayScreen();
        Direction direction = this.WaitDirectionOrCancel();
        if (direction == null)
          flag1 = false;
        else if (direction != Direction.NEUTRAL)
        {
          Point point = player.Location.Position + direction;
          if (player.Location.Map.IsInBounds(point))
          {
            MapObject mapObjectAt = player.Location.Map.GetMapObjectAt(point);
            if (mapObjectAt != null && mapObjectAt is DoorWindow)
            {
              DoorWindow door = mapObjectAt as DoorWindow;
              string reason;
              if (this.m_Rules.IsClosableFor(player, door, out reason))
              {
                this.DoCloseDoor(player, door);
                this.RedrawPlayScreen();
                flag1 = false;
                flag2 = true;
              }
              else
                this.AddMessage(this.MakeErrorMessage(string.Format("Can't close {0} : {1}.", (object) door.TheName, (object) reason)));
            }
            else
              this.AddMessage(this.MakeErrorMessage("Nothing to close there."));
          }
        }
      }
      while (flag1);
      this.ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerBarricade(Actor player)
    {
      bool flag1 = true;
      bool flag2 = false;
      this.ClearOverlays();
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.BARRICADE_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
      do
      {
        this.RedrawPlayScreen();
        Direction direction = this.WaitDirectionOrCancel();
        if (direction == null)
          flag1 = false;
        else if (direction != Direction.NEUTRAL)
        {
          Point point = player.Location.Position + direction;
          if (player.Location.Map.IsInBounds(point))
          {
            MapObject mapObjectAt = player.Location.Map.GetMapObjectAt(point);
            if (mapObjectAt != null)
            {
              if (mapObjectAt is DoorWindow)
              {
                DoorWindow door = mapObjectAt as DoorWindow;
                string reason;
                if (this.m_Rules.CanActorBarricadeDoor(player, door, out reason))
                {
                  this.DoBarricadeDoor(player, door);
                  this.RedrawPlayScreen();
                  flag1 = false;
                  flag2 = true;
                }
                else
                  this.AddMessage(this.MakeErrorMessage(string.Format("Cannot barricade {0} : {1}.", (object) door.TheName, (object) reason)));
              }
              else if (mapObjectAt is Fortification)
              {
                Fortification fort = mapObjectAt as Fortification;
                string reason;
                if (this.m_Rules.CanActorRepairFortification(player, fort, out reason))
                {
                  this.DoRepairFortification(player, fort);
                  this.RedrawPlayScreen();
                  flag1 = false;
                  flag2 = true;
                }
                else
                  this.AddMessage(this.MakeErrorMessage(string.Format("Cannot repair {0} : {1}.", (object) fort.TheName, (object) reason)));
              }
              else
                this.AddMessage(this.MakeErrorMessage(string.Format("{0} cannot be repaired or barricaded.", (object) mapObjectAt.TheName)));
            }
            else
              this.AddMessage(this.MakeErrorMessage("Nothing to barricade there."));
          }
        }
      }
      while (flag1);
      this.ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerBreak(Actor player)
    {
      bool flag1 = true;
      bool flag2 = false;
      this.ClearOverlays();
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.BREAK_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
      do
      {
        this.RedrawPlayScreen();
        Direction direction = this.WaitDirectionOrCancel();
        if (direction == null)
          flag1 = false;
        else if (direction == Direction.NEUTRAL)
        {
          Exit exitAt = player.Location.Map.GetExitAt(player.Location.Position);
          if (exitAt == null)
          {
            this.AddMessage(this.MakeErrorMessage("No exit there."));
          }
          else
          {
            Map toMap = exitAt.ToMap;
            Actor actorAt = toMap.GetActorAt(exitAt.ToPosition);
            string reason;
            if (actorAt != null)
            {
              if (this.m_Rules.IsEnemyOf(player, actorAt))
              {
                if (this.m_Rules.CanActorMeleeAttack(player, actorAt, out reason))
                {
                  this.DoMeleeAttack(player, actorAt);
                  flag1 = false;
                  flag2 = true;
                }
                else
                  this.AddMessage(this.MakeErrorMessage(string.Format("Cannot attack {0} : {1}.", (object) actorAt.Name, (object) reason)));
              }
              else
                this.AddMessage(this.MakeErrorMessage(string.Format("{0} is not your enemy.", (object) actorAt.Name)));
            }
            else
            {
              MapObject mapObjectAt = toMap.GetMapObjectAt(exitAt.ToPosition);
              if (mapObjectAt != null)
              {
                if (this.m_Rules.IsBreakableFor(player, mapObjectAt, out reason))
                {
                  this.DoBreak(player, mapObjectAt);
                  flag1 = false;
                  flag2 = true;
                }
                else
                  this.AddMessage(this.MakeErrorMessage(string.Format("Cannot break {0} : {1}.", (object) mapObjectAt.TheName, (object) reason)));
              }
              else
                this.AddMessage(this.MakeErrorMessage("Nothing to break or attack on the other side."));
            }
          }
        }
        else
        {
          Point point = player.Location.Position + direction;
          if (player.Location.Map.IsInBounds(point))
          {
            MapObject mapObjectAt = player.Location.Map.GetMapObjectAt(point);
            if (mapObjectAt != null)
            {
              string reason;
              if (this.m_Rules.IsBreakableFor(player, mapObjectAt, out reason))
              {
                this.DoBreak(player, mapObjectAt);
                this.RedrawPlayScreen();
                flag1 = false;
                flag2 = true;
              }
              else
                this.AddMessage(this.MakeErrorMessage(string.Format("Cannot break {0} : {1}.", (object) mapObjectAt.TheName, (object) reason)));
            }
            else
              this.AddMessage(this.MakeErrorMessage("Nothing to break there."));
          }
        }
      }
      while (flag1);
      this.ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerBuildFortification(Actor player, bool isLarge)
    {
      if (player.Sheet.SkillTable.GetSkillLevel(3) == 0)
      {
        this.AddMessage(this.MakeErrorMessage("need carpentry skill."));
        return false;
      }
      int num = this.m_Rules.ActorBarricadingMaterialNeedForFortification(player, isLarge);
      if (this.m_Rules.CountBarricadingMaterial(player) < num)
      {
        this.AddMessage(this.MakeErrorMessage(string.Format("not enough barricading material, need {0}.", (object) num)));
        return false;
      }
      bool flag1 = true;
      bool flag2 = false;
      this.ClearOverlays();
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(isLarge ? this.BUILD_LARGE_FORT_MODE_TEXT : this.BUILD_SMALL_FORT_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
      do
      {
        this.RedrawPlayScreen();
        Direction direction = this.WaitDirectionOrCancel();
        if (direction == null)
          flag1 = false;
        else if (direction != Direction.NEUTRAL)
        {
          Point point = player.Location.Position + direction;
          if (player.Location.Map.IsInBounds(point))
          {
            string reason;
            if (this.m_Rules.CanActorBuildFortification(player, point, isLarge, out reason))
            {
              this.DoBuildFortification(player, point, isLarge);
              this.RedrawPlayScreen();
              flag1 = false;
              flag2 = true;
            }
            else
              this.AddMessage(this.MakeErrorMessage(string.Format("Cannot build here : {0}.", (object) reason)));
          }
        }
      }
      while (flag1);
      this.ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerFireMode(Actor player)
    {
      bool flag1 = true;
      bool flag2 = false;
      if (player.GetEquippedWeapon() is ItemGrenade || player.GetEquippedWeapon() is ItemGrenadePrimed)
        return this.HandlePlayerThrowGrenade(player);
      ItemRangedWeapon itemRangedWeapon = player.GetEquippedWeapon() as ItemRangedWeapon;
      if (itemRangedWeapon == null)
      {
        this.AddMessage(this.MakeErrorMessage("No weapon ready to fire."));
        this.RedrawPlayScreen();
        return false;
      }
      if (itemRangedWeapon.Ammo <= 0)
      {
        this.AddMessage(this.MakeErrorMessage("No ammo left."));
        this.RedrawPlayScreen();
        return false;
      }
      HashSet<Point> fovFor = LOS.ComputeFOVFor(this.m_Rules, player, this.m_Session.WorldTime, this.m_Session.World.Weather);
      List<Actor> enemiesInFov = this.m_Rules.GetEnemiesInFov(player, fovFor);
      if (enemiesInFov == null || enemiesInFov.Count == 0)
      {
        this.AddMessage(this.MakeErrorMessage("No targets to fire at."));
        this.RedrawPlayScreen();
        return false;
      }
      Attack attack = this.m_Rules.ActorRangedAttack(player, player.CurrentRangedAttack, 0, (Actor) null);
      int index = 0;
      List<Point> LoF = new List<Point>(attack.Range);
      FireMode mode = FireMode.DEFAULT;
      do
      {
        Actor actor = enemiesInFov[index];
        LoF.Clear();
        string reason;
        bool flag3 = this.m_Rules.CanActorFireAt(player, actor, LoF, out reason);
        int num1 = this.m_Rules.GridDistance(player.Location.Position, actor.Location.Position);
        this.ClearOverlays();
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.FIRE_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(actor.Location.Position), "Icons\\target"));
        string imageID = flag3 ? (num1 <= attack.EfficientRange ? "Icons\\line_clear" : "Icons\\line_bad") : "Icons\\line_blocked";
        foreach (Point mapPosition in LoF)
          this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(mapPosition), imageID));
        this.RedrawPlayScreen();
        KeyEventArgs key = this.m_UI.UI_WaitKey();
        int num2 = (int) InputTranslator.KeyToCommand(key);
        if (key.KeyCode == Keys.Escape)
          flag1 = false;
        else if (key.KeyCode == Keys.T)
          index = (index + 1) % enemiesInFov.Count;
        else if (key.KeyCode == Keys.M)
        {
          mode = (FireMode) ((int) (mode + 1) % 2);
          this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Switched to {0} fire mode.", (object) mode.ToString()), this.m_Session.WorldTime.TurnCounter, Color.Yellow));
        }
        else if (key.KeyCode == Keys.F)
        {
          if (flag3)
          {
            this.DoSingleRangedAttack(player, actor, LoF, mode);
            this.RedrawPlayScreen();
            flag1 = false;
            flag2 = true;
          }
          else
            this.AddMessage(this.MakeErrorMessage(string.Format("Can't fire at {0} : {1}.", (object) actor.TheName, (object) reason)));
        }
      }
      while (flag1);
      this.ClearOverlays();
      return flag2;
    }

    private void HandlePlayerMarkEnemies(Actor player)
    {
      if (player.Model.Abilities.IsUndead)
      {
        this.AddMessage(this.MakeErrorMessage("Undeads can't have personal enemies."));
      }
      else
      {
        Map map = player.Location.Map;
        List<Actor> actorList = new List<Actor>();
        foreach (Point position in this.m_PlayerFOV)
        {
          Actor actorAt = map.GetActorAt(position);
          if (actorAt != null && !actorAt.IsPlayer)
            actorList.Add(actorAt);
        }
        if (actorList.Count == 0)
        {
          this.AddMessage(this.MakeErrorMessage("No visible actors to mark."));
          this.RedrawPlayScreen();
        }
        else
        {
          bool flag1 = true;
          int index = 0;
          do
          {
            Actor target = actorList[index];
            this.ClearOverlays();
            this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.MARK_ENEMIES_MODE, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
            this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(target.Location.Position), "Icons\\target"));
            this.RedrawPlayScreen();
            KeyEventArgs key = this.m_UI.UI_WaitKey();
            int num = (int) InputTranslator.KeyToCommand(key);
            if (key.KeyCode == Keys.Escape)
              flag1 = false;
            else if (key.KeyCode == Keys.T)
              index = (index + 1) % actorList.Count;
            else if (key.KeyCode == Keys.E)
            {
              bool flag2 = true;
              if (target.Leader == player)
              {
                this.AddMessage(this.MakeErrorMessage("Can't make a follower your enemy."));
                flag2 = false;
              }
              else if (player.Leader == target)
              {
                this.AddMessage(this.MakeErrorMessage("Can't make your leader your enemy."));
                flag2 = false;
              }
              else if (this.m_Rules.IsEnemyOf(this.m_Player, target))
              {
                this.AddMessage(this.MakeErrorMessage("Already enemies."));
                flag2 = false;
              }
              if (flag2)
              {
                this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0} is now a personal enemy.", (object) target.TheName), this.m_Session.WorldTime.TurnCounter, Color.Orange));
                this.DoMakeAggression(player, target);
              }
            }
          }
          while (flag1);
          this.ClearOverlays();
        }
      }
    }

    private bool HandlePlayerThrowGrenade(Actor player)
    {
      bool flag1 = true;
      bool flag2 = false;
      ItemGrenade itemGrenade = player.GetEquippedWeapon() as ItemGrenade;
      ItemGrenadePrimed itemGrenadePrimed = player.GetEquippedWeapon() as ItemGrenadePrimed;
      if (itemGrenade == null && itemGrenadePrimed == null)
      {
        this.AddMessage(this.MakeErrorMessage("No grenade to throw."));
        this.RedrawPlayScreen();
        return false;
      }
      ItemGrenadeModel itemGrenadeModel = itemGrenade == null ? (itemGrenadePrimed.Model as ItemGrenadePrimedModel).GrenadeModel : itemGrenade.Model as ItemGrenadeModel;
      Map map = player.Location.Map;
      Point point1 = player.Location.Position;
      int num = this.m_Rules.ActorMaxThrowRange(player, itemGrenadeModel.MaxThrowDistance);
      List<Point> LoF = new List<Point>();
      do
      {
        LoF.Clear();
        string reason;
        bool flag3 = this.m_Rules.CanActorThrowTo(player, point1, LoF, out reason);
        this.ClearOverlays();
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.THROW_GRENADE_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
        string imageID = flag3 ? "Icons\\line_clear" : "Icons\\line_blocked";
        foreach (Point mapPosition in LoF)
          this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(mapPosition), imageID));
        this.RedrawPlayScreen();
        KeyEventArgs key = this.m_UI.UI_WaitKey();
        PlayerCommand command = InputTranslator.KeyToCommand(key);
        if (key.KeyCode == Keys.Escape)
          flag1 = false;
        else if (key.KeyCode == Keys.F)
        {
          if (flag3)
          {
            bool flag4 = true;
            if (this.m_Rules.GridDistance(player.Location.Position, point1) <= itemGrenadeModel.BlastAttack.Radius)
            {
              this.ClearMessages();
              this.AddMessage(new djack.RogueSurvivor.Data.Message("You are in the blast radius!", this.m_Session.WorldTime.TurnCounter, Color.Yellow));
              this.AddMessage(this.MakeYesNoMessage("Really throw there"));
              this.RedrawPlayScreen();
              flag4 = this.WaitYesOrNo();
              this.ClearMessages();
              this.RedrawPlayScreen();
            }
            if (flag4)
            {
              if (itemGrenade != null)
                this.DoThrowGrenadeUnprimed(player, point1);
              else
                this.DoThrowGrenadePrimed(player, point1);
              this.RedrawPlayScreen();
              flag1 = false;
              flag2 = true;
            }
          }
          else
            this.AddMessage(this.MakeErrorMessage(string.Format("Can't throw there : {0}.", (object) reason)));
        }
        else
        {
          Direction direction = RogueGame.CommandToDirection(command);
          if (direction != null)
          {
            Point point2 = point1 + direction;
            if (map.IsInBounds(point2) && this.m_Rules.GridDistance(player.Location.Position, point2) <= num)
              point1 = point2;
          }
        }
      }
      while (flag1);
      this.ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerSleep(Actor player)
    {
      string reason;
      if (!this.m_Rules.CanActorSleep(player, out reason))
      {
        this.AddMessage(this.MakeErrorMessage(string.Format("Cannot sleep now : {0}.", (object) reason)));
        return false;
      }
      this.AddMessage(this.MakeYesNoMessage("Really sleep there"));
      this.RedrawPlayScreen();
      if (!this.WaitYesOrNo())
      {
        this.AddMessage(new djack.RogueSurvivor.Data.Message("Good, keep those eyes wide open.", this.m_Session.WorldTime.TurnCounter, Color.Yellow));
        return false;
      }
      this.AddMessage(new djack.RogueSurvivor.Data.Message("Goodnight, happy nightmares!", this.m_Session.WorldTime.TurnCounter, Color.Yellow));
      this.DoStartSleeping(player);
      this.RedrawPlayScreen();
      this.m_MusicManager.StopAll();
      this.m_MusicManager.PlayLooping(GameMusics.SLEEP);
      return true;
    }

    private bool HandlePlayerSwitchPlace(Actor player)
    {
      bool flag1 = true;
      bool flag2 = false;
      this.ClearOverlays();
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.SWITCH_PLACE_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
      do
      {
        this.RedrawPlayScreen();
        Direction direction = this.WaitDirectionOrCancel();
        if (direction == null)
          flag1 = false;
        else if (direction != Direction.NEUTRAL)
        {
          Point point = player.Location.Position + direction;
          if (player.Location.Map.IsInBounds(point))
          {
            Actor actorAt = player.Location.Map.GetActorAt(point);
            if (actorAt != null)
            {
              string reason;
              if (this.m_Rules.CanActorSwitchPlaceWith(player, actorAt, out reason))
              {
                flag2 = true;
                flag1 = false;
                this.DoSwitchPlace(player, actorAt);
              }
              else
                this.AddMessage(this.MakeErrorMessage(string.Format("Can't switch place : {0}", (object) reason)));
            }
            else
              this.AddMessage(this.MakeErrorMessage("Noone there."));
          }
        }
      }
      while (flag1);
      this.ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerTakeLead(Actor player)
    {
      bool flag1 = true;
      bool flag2 = false;
      this.ClearOverlays();
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.TAKE_LEAD_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
      do
      {
        this.RedrawPlayScreen();
        Direction direction = this.WaitDirectionOrCancel();
        if (direction == null)
          flag1 = false;
        else if (direction != Direction.NEUTRAL)
        {
          Point point = player.Location.Position + direction;
          if (player.Location.Map.IsInBounds(point))
          {
            Actor actorAt = player.Location.Map.GetActorAt(point);
            if (actorAt != null)
            {
              string reason;
              if (this.m_Rules.CanActorTakeLead(player, actorAt, out reason))
              {
                flag2 = true;
                flag1 = false;
                this.DoTakeLead(player, actorAt);
                this.m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, string.Format("Recruited {0}.", (object) actorAt.TheName));
                this.AddMessage(new djack.RogueSurvivor.Data.Message("(you can now set directives and orders for your new follower).", this.m_Session.WorldTime.TurnCounter, Color.White));
                this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("(to give order : press <{0}>).", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.ORDER_MODE).ToString()), this.m_Session.WorldTime.TurnCounter, Color.White));
              }
              else if (actorAt.Leader == player)
              {
                if (this.m_Rules.CanActorCancelLead(player, actorAt, out reason))
                {
                  this.AddMessage(this.MakeYesNoMessage(string.Format("Really ask {0} to leave", (object) actorAt.TheName)));
                  this.RedrawPlayScreen();
                  if (this.WaitYesOrNo())
                  {
                    flag2 = true;
                    flag1 = false;
                    this.DoCancelLead(player, actorAt);
                    this.m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, string.Format("Fired {0}.", (object) actorAt.TheName));
                  }
                  else
                    this.AddMessage(new djack.RogueSurvivor.Data.Message("Good, together you are strong.", this.m_Session.WorldTime.TurnCounter, Color.Yellow));
                }
                else
                  this.AddMessage(this.MakeErrorMessage(string.Format("{0} can't leave : {1}.", (object) actorAt.TheName, (object) reason)));
              }
              else
                this.AddMessage(this.MakeErrorMessage(string.Format("Can't lead {0} : {1}.", (object) actorAt.TheName, (object) reason)));
            }
            else
              this.AddMessage(this.MakeErrorMessage("Noone there."));
          }
        }
      }
      while (flag1);
      this.ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerPush(Actor player)
    {
      if (!this.m_Rules.HasActorPushAbility(player))
      {
        this.AddMessage(this.MakeErrorMessage("Cannot push objects."));
        return false;
      }
      if (this.m_Rules.IsActorTired(player))
      {
        this.AddMessage(this.MakeErrorMessage("Too tired to push."));
        return false;
      }
      bool flag1 = true;
      bool flag2 = false;
      this.ClearOverlays();
      do
      {
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.PUSH_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
        this.RedrawPlayScreen();
        Direction direction = this.WaitDirectionOrCancel();
        if (direction == null)
          flag1 = false;
        else if (direction != Direction.NEUTRAL)
        {
          Point point = player.Location.Position + direction;
          if (player.Location.Map.IsInBounds(point))
          {
            Actor actorAt = player.Location.Map.GetActorAt(point);
            MapObject mapObjectAt = player.Location.Map.GetMapObjectAt(point);
            string reason;
            if (actorAt != null)
            {
              if (this.m_Rules.CanActorShove(player, actorAt, out reason))
              {
                if (this.HandlePlayerShoveActor(player, actorAt))
                {
                  flag1 = false;
                  flag2 = true;
                }
              }
              else
                this.AddMessage(this.MakeErrorMessage(string.Format("Cannot shove {0} : {1}.", (object) actorAt.TheName, (object) reason)));
            }
            else if (mapObjectAt != null)
            {
              if (this.m_Rules.CanActorPush(player, mapObjectAt, out reason))
              {
                if (this.HandlePlayerPushObject(player, mapObjectAt))
                {
                  flag1 = false;
                  flag2 = true;
                }
              }
              else
                this.AddMessage(this.MakeErrorMessage(string.Format("Cannot move {0} : {1}.", (object) mapObjectAt.TheName, (object) reason)));
            }
            else
              this.AddMessage(this.MakeErrorMessage("Nothing to push there."));
          }
        }
      }
      while (flag1);
      this.ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerPushObject(Actor player, MapObject mapObj)
    {
      bool flag1 = true;
      bool flag2 = false;
      this.ClearOverlays();
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(new string[1]
      {
        string.Format(this.PUSH_OBJECT_MODE_TEXT, (object) mapObj.TheName)
      }, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Yellow, new Rectangle(this.MapToScreen(mapObj.Location.Position), new Size(32, 32))));
      do
      {
        this.RedrawPlayScreen();
        Direction direction = this.WaitDirectionOrCancel();
        if (direction == null)
          flag1 = false;
        else if (direction != Direction.NEUTRAL)
        {
          Point point = mapObj.Location.Position + direction;
          if (player.Location.Map.IsInBounds(point))
          {
            string reason;
            if (this.m_Rules.CanPushObjectTo(mapObj, point, out reason))
            {
              this.DoPush(player, mapObj, point);
              flag1 = false;
              flag2 = true;
            }
            else
              this.AddMessage(this.MakeErrorMessage(string.Format("Cannot move {0} there : {1}.", (object) mapObj.TheName, (object) reason)));
          }
        }
      }
      while (flag1);
      this.ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerShoveActor(Actor player, Actor other)
    {
      bool flag1 = true;
      bool flag2 = false;
      this.ClearOverlays();
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(new string[1]
      {
        string.Format(this.SHOVE_ACTOR_MODE_TEXT, (object) other.TheName)
      }, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Yellow, new Rectangle(this.MapToScreen(other.Location.Position), new Size(32, 32))));
      do
      {
        this.RedrawPlayScreen();
        Direction direction = this.WaitDirectionOrCancel();
        if (direction == null)
          flag1 = false;
        else if (direction != Direction.NEUTRAL)
        {
          Point point = other.Location.Position + direction;
          if (player.Location.Map.IsInBounds(point))
          {
            string reason;
            if (this.m_Rules.CanShoveActorTo(other, point, out reason))
            {
              this.DoShove(player, other, point);
              flag1 = false;
              flag2 = true;
            }
            else
              this.AddMessage(this.MakeErrorMessage(string.Format("Cannot shove {0} there : {1}.", (object) other.TheName, (object) reason)));
          }
        }
      }
      while (flag1);
      this.ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerUseSpray(Actor player)
    {
      Item equippedItem = player.GetEquippedItem(DollPart.LEFT_HAND);
      if (equippedItem == null)
      {
        this.AddMessage(this.MakeErrorMessage("No spray equipped."));
        this.RedrawPlayScreen();
        return false;
      }
      if (equippedItem is ItemSprayPaint)
        return this.HandlePlayerTag(player);
      ItemSprayScent spray = equippedItem as ItemSprayScent;
      if (spray != null)
      {
        string reason;
        if (!this.m_Rules.CanActorUseItem(player, (Item) spray, out reason))
        {
          this.AddMessage(this.MakeErrorMessage(string.Format("Can't use the spray : {0}.", (object) reason)));
          this.RedrawPlayScreen();
          return false;
        }
        this.DoUseSprayScentItem(player, spray);
        return true;
      }
      this.AddMessage(this.MakeErrorMessage("No spray equipped."));
      this.RedrawPlayScreen();
      return false;
    }

    private bool HandlePlayerTag(Actor player)
    {
      bool flag1 = true;
      bool flag2 = false;
      ItemSprayPaint spray = player.GetEquippedItem(DollPart.LEFT_HAND) as ItemSprayPaint;
      if (spray == null)
      {
        this.AddMessage(this.MakeErrorMessage("No spray paint equipped."));
        this.RedrawPlayScreen();
        return false;
      }
      if (spray.PaintQuantity <= 0)
      {
        this.AddMessage(this.MakeErrorMessage("No paint left."));
        this.RedrawPlayScreen();
        return false;
      }
      this.ClearOverlays();
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.TAG_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
      do
      {
        this.RedrawPlayScreen();
        Direction direction = this.WaitDirectionOrCancel();
        if (direction == null)
          flag1 = false;
        else if (direction != Direction.NEUTRAL)
        {
          Point point = player.Location.Position + direction;
          if (player.Location.Map.IsInBounds(point))
          {
            string reason;
            if (this.CanTag(player.Location.Map, point, out reason))
            {
              this.DoTag(player, spray, point);
              flag1 = false;
              flag2 = true;
            }
            else
            {
              this.AddMessage(this.MakeErrorMessage(string.Format("Can't tag there : {0}.", (object) reason)));
              this.RedrawPlayScreen();
            }
          }
        }
      }
      while (flag1);
      this.ClearOverlays();
      return flag2;
    }

    private bool CanTag(Map map, Point pos, out string reason)
    {
      if (!map.IsInBounds(pos))
      {
        reason = "out of map";
        return false;
      }
      if (map.GetActorAt(pos) != null)
      {
        reason = "someone there";
        return false;
      }
      if (map.GetMapObjectAt(pos) != null)
      {
        reason = "something there";
        return false;
      }
      reason = "";
      return true;
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void StartPlayerWaitLong(Actor player)
    {
      this.m_IsPlayerLongWait = true;
      this.m_IsPlayerLongWaitForcedStop = false;
      this.m_PlayerLongWaitEnd = new WorldTime(this.m_Session.WorldTime.TurnCounter + WorldTime.TURNS_PER_HOUR);
      this.AddMessage(this.MakeMessage(player, string.Format("{0} waiting.", (object) this.Conjugate(player, this.VERB_START))));
      this.RedrawPlayScreen();
    }

    private bool CheckPlayerWaitLong(Actor player)
    {
      if (this.m_IsPlayerLongWaitForcedStop || this.m_Session.WorldTime.TurnCounter >= this.m_PlayerLongWaitEnd.TurnCounter || (this.m_Rules.IsActorHungry(player) || this.m_Rules.IsActorStarving(player)) || (this.m_Rules.IsActorSleepy(player) || this.m_Rules.IsActorExhausted(player)))
        return false;
      foreach (Point position in this.m_PlayerFOV)
      {
        Actor actorAt = player.Location.Map.GetActorAt(position);
        if (actorAt != null && this.m_Rules.IsEnemyOf(player, actorAt))
          return false;
      }
      return !this.TryPlayerInsanity();
    }

    private bool HandlePlayerOrderMode(Actor player)
    {
      if (player.CountFollowers == 0)
      {
        this.AddMessage(this.MakeErrorMessage("No followers to give orders to."));
        return false;
      }
      Actor[] actorArray = new Actor[player.CountFollowers];
      HashSet<Point>[] pointSetArray = new HashSet<Point>[player.CountFollowers];
      bool[] flagArray = new bool[player.CountFollowers];
      int index1 = 0;
      foreach (Actor follower in player.Followers)
      {
        actorArray[index1] = follower;
        pointSetArray[index1] = LOS.ComputeFOVFor(this.m_Rules, follower, this.m_Session.WorldTime, this.m_Session.World.Weather);
        bool flag1 = pointSetArray[index1].Contains(player.Location.Position) && this.m_PlayerFOV.Contains(follower.Location.Position);
        bool flag2 = this.AreLinkedByPhone(player, follower);
        flagArray[index1] = flag1 || flag2;
        ++index1;
      }
      if (player.CountFollowers == 1 && flagArray[0])
      {
        bool flag = this.HandlePlayerOrderFollower(player, actorArray[0]);
        this.ClearOverlays();
        this.ClearMessages();
        return flag;
      }
      bool flag3 = true;
      bool flag4 = false;
      int num1 = 0;
      do
      {
        this.ClearOverlays();
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.ORDER_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
        this.ClearMessages();
        this.AddMessage(new djack.RogueSurvivor.Data.Message("Choose a follower.", this.m_Session.WorldTime.TurnCounter, Color.Yellow));
        int num2;
        for (num2 = 0; num2 < 5 && num1 + num2 < actorArray.Length; ++num2)
        {
          int index2 = num2 + num1;
          Actor follower = actorArray[index2];
          string str = this.DescribePlayerFollowerStatus(follower);
          if (flagArray[index2])
            this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0}. {1}/{2} {3}... {4}.", (object) (1 + num2), (object) (index2 + 1), (object) actorArray.Length, (object) follower.Name, (object) str), this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
          else
            this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0}. {1}/{2} ({3}) {4}.", (object) (1 + num2), (object) (index2 + 1), (object) actorArray.Length, (object) follower.Name, (object) str), this.m_Session.WorldTime.TurnCounter, Color.DarkGray));
        }
        if (num2 < actorArray.Length)
          this.AddMessage(new djack.RogueSurvivor.Data.Message("9. next", this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.RedrawPlayScreen();
        KeyEventArgs keyEventArgs = this.m_UI.UI_WaitKey();
        int choiceNumber = this.KeyToChoiceNumber(keyEventArgs.KeyCode);
        if (keyEventArgs.KeyCode == Keys.Escape)
          flag3 = false;
        else if (choiceNumber == 9)
        {
          num1 += 5;
          if (num1 >= actorArray.Length)
            num1 = 0;
        }
        else if (choiceNumber >= 1 && choiceNumber <= num2)
        {
          int index2 = num1 + choiceNumber - 1;
          if (flagArray[index2])
          {
            Actor follower = actorArray[index2];
            if (this.HandlePlayerOrderFollower(player, follower))
            {
              flag3 = false;
              flag4 = true;
            }
          }
        }
      }
      while (flag3);
      this.ClearOverlays();
      this.ClearMessages();
      return flag4;
    }

    private bool HandlePlayerDirectiveFollower(Actor player, Actor follower)
    {
      bool flag1 = true;
      bool flag2 = false;
      do
      {
        ActorDirective directives = (follower.Controller as AIController).Directives;
        this.ClearOverlays();
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.ORDER_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
        this.ClearMessages();
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0} directives...", (object) follower.Name), this.m_Session.WorldTime.TurnCounter, Color.Yellow));
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("1. {0} items.", directives.CanTakeItems ? (object) "Take" : (object) "Don't take"), this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("2. {0} weapons.", directives.CanFireWeapons ? (object) "Fire" : (object) "Don't fire"), this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("3. {0} grenades.", directives.CanThrowGrenades ? (object) "Throw" : (object) "Don't throw"), this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("4. {0}.", directives.CanSleep ? (object) "Sleep" : (object) "Don't sleep"), this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("5. {0}.", directives.CanTrade ? (object) "Trade" : (object) "Don't trade"), this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("6. {0}.", (object) ActorDirective.CourageString(directives.Courage)), this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.RedrawPlayScreen();
        KeyEventArgs keyEventArgs = this.m_UI.UI_WaitKey();
        int choiceNumber = this.KeyToChoiceNumber(keyEventArgs.KeyCode);
        if (keyEventArgs.KeyCode == Keys.Escape)
        {
          flag1 = false;
        }
        else
        {
          switch (choiceNumber)
          {
            case 1:
              directives.CanTakeItems = !directives.CanTakeItems;
              break;
            case 2:
              directives.CanFireWeapons = !directives.CanFireWeapons;
              break;
            case 3:
              directives.CanThrowGrenades = !directives.CanThrowGrenades;
              break;
            case 4:
              directives.CanSleep = !directives.CanSleep;
              break;
            case 5:
              directives.CanTrade = !directives.CanTrade;
              break;
            case 6:
              switch (directives.Courage)
              {
                case ActorCourage.COWARD:
                  directives.Courage = ActorCourage.CAUTIOUS;
                  break;
                case ActorCourage.CAUTIOUS:
                  directives.Courage = ActorCourage.COURAGEOUS;
                  break;
                case ActorCourage.COURAGEOUS:
                  directives.Courage = ActorCourage.COWARD;
                  break;
              }
              break;
          }
        }
      }
      while (flag1);
      return flag2;
    }

    private bool HandlePlayerOrderFollower(Actor player, Actor follower)
    {
      if (!this.m_Rules.IsActorTrustingLeader(follower))
      {
        if (this.IsVisibleToPlayer(follower))
          this.DoSay(follower, player, "Sorry, I don't trust you enough yet.", RogueGame.Sayflags.IS_IMPORTANT | RogueGame.Sayflags.IS_FREE_ACTION);
        else if (this.AreLinkedByPhone(follower, player))
        {
          this.ClearMessages();
          this.AddMessage(this.MakeMessage(follower, "Sorry, I don't trust you enough yet."));
          this.AddMessagePressEnter();
        }
        return false;
      }
      string str1 = this.DescribePlayerFollowerStatus(follower);
      HashSet<Point> fovFor = LOS.ComputeFOVFor(this.m_Rules, follower, this.m_Session.WorldTime, this.m_Session.World.Weather);
      bool flag1 = true;
      bool flag2 = false;
      do
      {
        string str2 = (follower.Controller as OrderableAI).DontFollowLeader ? "Start" : "Stop";
        this.ClearOverlays();
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.ORDER_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
        this.ClearMessages();
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Order {0} to...", (object) follower.Name), this.m_Session.WorldTime.TurnCounter, Color.Yellow));
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("0. Cancel current order {0}.", (object) str1), this.m_Session.WorldTime.TurnCounter, Color.Green));
        this.AddMessage(new djack.RogueSurvivor.Data.Message("1. Set directives...", this.m_Session.WorldTime.TurnCounter, Color.Cyan));
        this.AddMessage(new djack.RogueSurvivor.Data.Message("2. Barricade (one)...    6. Drop all items.      A. Give me...", this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.AddMessage(new djack.RogueSurvivor.Data.Message("3. Barricade (max)...    7. Build small fort.    B. Sleep now.", this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("4. Guard...              8. Build large fort.    C. {0} following me.   ", (object) str2), this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.AddMessage(new djack.RogueSurvivor.Data.Message("5. Patrol...             9. Report events.       D. Where are you?", this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.RedrawPlayScreen();
        KeyEventArgs keyEventArgs = this.m_UI.UI_WaitKey();
        int choiceNumber = this.KeyToChoiceNumber(keyEventArgs.KeyCode);
        if (keyEventArgs.KeyCode == Keys.Escape)
          flag1 = false;
        else if (choiceNumber >= 0 && choiceNumber <= 9)
        {
          switch (choiceNumber)
          {
            case 0:
              this.DoCancelOrder(player, follower);
              flag1 = false;
              flag2 = true;
              break;
            case 1:
              this.HandlePlayerDirectiveFollower(player, follower);
              break;
            case 2:
              if (this.HandlePlayerOrderFollowerToBarricade(player, follower, fovFor, false))
              {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
            case 3:
              if (this.HandlePlayerOrderFollowerToBarricade(player, follower, fovFor, true))
              {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
            case 4:
              if (this.HandlePlayerOrderFollowerToGuard(player, follower, fovFor))
              {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
            case 5:
              if (this.HandlePlayerOrderFollowerToPatrol(player, follower, fovFor))
              {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
            case 6:
              if (this.HandlePlayerOrderFollowerToDropAllItems(player, follower))
              {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
            case 7:
              if (this.HandlePlayerOrderFollowerToBuildFortification(player, follower, fovFor, false))
              {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
            case 8:
              if (this.HandlePlayerOrderFollowerToBuildFortification(player, follower, fovFor, true))
              {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
            case 9:
              if (this.HandlePlayerOrderFollowerToReport(player, follower))
              {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
          }
        }
        else
        {
          switch (keyEventArgs.KeyCode)
          {
            case Keys.A:
              if (this.HandlePlayerOrderFollowerToGiveItems(player, follower))
              {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
            case Keys.B:
              if (this.HandlePlayerOrderFollowerToSleep(player, follower))
              {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
            case Keys.C:
              if (this.HandlePlayerOrderFollowerToToggleFollow(player, follower))
              {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
            case Keys.D:
              if (this.HandlePlayerOrderFollowerToReportPosition(player, follower))
              {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
          }
        }
      }
      while (flag1);
      return flag2;
    }

    private bool HandlePlayerOrderFollowerToBuildFortification(Actor player, Actor follower, HashSet<Point> followerFOV, bool isLarge)
    {
      bool flag1 = true;
      bool flag2 = false;
      Map map1 = player.Location.Map;
      Point? nullable = new Point?();
      Color color = Color.White;
      do
      {
        this.ClearOverlays();
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.ORDER_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
        if (nullable.HasValue)
          this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(color, new Rectangle(this.MapToScreen(nullable.Value.X, nullable.Value.Y), new Size(32, 32))));
        this.ClearMessages();
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Ordering {0} to build {1} fortification...", (object) follower.Name, isLarge ? (object) "large" : (object) "small"), this.m_Session.WorldTime.TurnCounter, Color.Yellow));
        this.AddMessage(new djack.RogueSurvivor.Data.Message("Left-Click on a map object.", this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.RedrawPlayScreen();
        KeyEventArgs key;
        Point mousePos;
        MouseButtons? mouseButtons;
        this.WaitKeyOrMouse(out key, out mousePos, out mouseButtons);
        if (key != null)
        {
          if (key.KeyCode == Keys.Escape)
            flag1 = false;
        }
        else
        {
          Point map2 = this.MouseToMap(mousePos);
          if (map1.IsInBounds(map2) && this.IsInViewRect(map2))
          {
            if (this.IsVisibleToPlayer(map1, map2) && followerFOV.Contains(map2))
            {
              string reason;
              if (this.m_Rules.CanActorBuildFortification(follower, map2, isLarge, out reason))
              {
                nullable = new Point?(map2);
                color = Color.LightGreen;
                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left)
                {
                  this.DoGiveOrderTo(player, follower, new ActorOrder(isLarge ? ActorTasks.BUILD_LARGE_FORTIFICATION : ActorTasks.BUILD_SMALL_FORTIFICATION, new Location(player.Location.Map, map2)));
                  flag1 = false;
                  flag2 = true;
                }
              }
              else
              {
                nullable = new Point?(map2);
                color = Color.Red;
                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left)
                {
                  this.AddMessage(this.MakeErrorMessage(string.Format("Can't build {0} fortification : {1}.", isLarge ? (object) "large" : (object) "small", (object) reason)));
                  this.AddMessagePressEnter();
                }
              }
            }
            else
            {
              nullable = new Point?(map2);
              color = Color.Red;
            }
          }
        }
      }
      while (flag1);
      return flag2;
    }

    private bool HandlePlayerOrderFollowerToBarricade(Actor player, Actor follower, HashSet<Point> followerFOV, bool toTheMax)
    {
      bool flag1 = true;
      bool flag2 = false;
      Map map1 = player.Location.Map;
      Point? nullable = new Point?();
      Color color = Color.White;
      do
      {
        this.ClearOverlays();
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.ORDER_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
        if (nullable.HasValue)
          this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(color, new Rectangle(this.MapToScreen(nullable.Value.X, nullable.Value.Y), new Size(32, 32))));
        this.ClearMessages();
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Ordering {0} to barricade...", (object) follower.Name), this.m_Session.WorldTime.TurnCounter, Color.Yellow));
        this.AddMessage(new djack.RogueSurvivor.Data.Message("Left-Click on a map object.", this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.RedrawPlayScreen();
        KeyEventArgs key;
        Point mousePos;
        MouseButtons? mouseButtons;
        this.WaitKeyOrMouse(out key, out mousePos, out mouseButtons);
        if (key != null)
        {
          if (key.KeyCode == Keys.Escape)
            flag1 = false;
        }
        else
        {
          Point map2 = this.MouseToMap(mousePos);
          if (map1.IsInBounds(map2) && this.IsInViewRect(map2))
          {
            if (this.IsVisibleToPlayer(map1, map2) && followerFOV.Contains(map2))
            {
              DoorWindow door = map1.GetMapObjectAt(map2) as DoorWindow;
              if (door != null)
              {
                string reason;
                if (this.m_Rules.CanActorBarricadeDoor(follower, door, out reason))
                {
                  nullable = new Point?(map2);
                  color = Color.LightGreen;
                  if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left)
                  {
                    this.DoGiveOrderTo(player, follower, new ActorOrder(toTheMax ? ActorTasks.BARRICADE_MAX : ActorTasks.BARRICADE_ONE, door.Location));
                    flag1 = false;
                    flag2 = true;
                  }
                }
                else
                {
                  nullable = new Point?(map2);
                  color = Color.Red;
                  if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left)
                  {
                    this.AddMessage(this.MakeErrorMessage(string.Format("Can't barricade {0} : {1}.", (object) door.TheName, (object) reason)));
                    this.AddMessagePressEnter();
                  }
                }
              }
              else
              {
                nullable = new Point?(map2);
                color = Color.Red;
              }
            }
            else
            {
              nullable = new Point?(map2);
              color = Color.Red;
            }
          }
        }
      }
      while (flag1);
      return flag2;
    }

    private bool HandlePlayerOrderFollowerToGuard(Actor player, Actor follower, HashSet<Point> followerFOV)
    {
      bool flag1 = true;
      bool flag2 = false;
      Map map1 = player.Location.Map;
      Point? nullable = new Point?();
      Color color = Color.White;
      do
      {
        this.ClearOverlays();
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.ORDER_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
        if (nullable.HasValue)
          this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(color, new Rectangle(this.MapToScreen(nullable.Value.X, nullable.Value.Y), new Size(32, 32))));
        this.ClearMessages();
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Ordering {0} to guard...", (object) follower.Name), this.m_Session.WorldTime.TurnCounter, Color.Yellow));
        this.AddMessage(new djack.RogueSurvivor.Data.Message("Left-Click on a map position.", this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.RedrawPlayScreen();
        KeyEventArgs key;
        Point mousePos;
        MouseButtons? mouseButtons;
        this.WaitKeyOrMouse(out key, out mousePos, out mouseButtons);
        if (key != null)
        {
          if (key.KeyCode == Keys.Escape)
            flag1 = false;
        }
        else
        {
          Point map2 = this.MouseToMap(mousePos);
          if (map1.IsInBounds(map2) && this.IsInViewRect(map2))
          {
            if (this.IsVisibleToPlayer(map1, map2) && followerFOV.Contains(map2))
            {
              string reason;
              if (map2 == follower.Location.Position || this.m_Rules.IsWalkableFor(follower, map1, map2.X, map2.Y, out reason))
              {
                nullable = new Point?(map2);
                color = Color.LightGreen;
                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left)
                {
                  this.DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.GUARD, new Location(map1, map2)));
                  flag1 = false;
                  flag2 = true;
                }
              }
              else
              {
                nullable = new Point?(map2);
                color = Color.Red;
                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left)
                {
                  this.AddMessage(this.MakeErrorMessage(string.Format("Can't guard here : {0}", (object) reason)));
                  this.AddMessagePressEnter();
                }
              }
            }
            else
            {
              nullable = new Point?(map2);
              color = Color.Red;
            }
          }
        }
      }
      while (flag1);
      return flag2;
    }

    private bool HandlePlayerOrderFollowerToPatrol(Actor player, Actor follower, HashSet<Point> followerFOV)
    {
      bool flag1 = true;
      bool flag2 = false;
      Map map1 = player.Location.Map;
      Point? nullable = new Point?();
      Color color = Color.White;
      do
      {
        this.ClearOverlays();
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.ORDER_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
        if (nullable.HasValue)
        {
          this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(color, new Rectangle(this.MapToScreen(nullable.Value.X, nullable.Value.Y), new Size(32, 32))));
          List<Zone> zonesAt = map1.GetZonesAt(nullable.Value.X, nullable.Value.Y);
          if (zonesAt != null && zonesAt.Count > 0)
          {
            string[] lines = new string[zonesAt.Count + 1];
            lines[0] = "Zone(s) here :";
            for (int index = 0; index < zonesAt.Count; ++index)
              lines[index + 1] = string.Format("- {0}", (object) zonesAt[index].Name);
            this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(lines, Color.White, Color.White, this.POPUP_FILLCOLOR, this.MapToScreen(nullable.Value.X + 1, nullable.Value.Y + 1)));
          }
        }
        this.ClearMessages();
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Ordering {0} to patrol...", (object) follower.Name), this.m_Session.WorldTime.TurnCounter, Color.Yellow));
        this.AddMessage(new djack.RogueSurvivor.Data.Message("Left-Click on a map position.", this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.RedrawPlayScreen();
        KeyEventArgs key;
        Point mousePos;
        MouseButtons? mouseButtons;
        this.WaitKeyOrMouse(out key, out mousePos, out mouseButtons);
        if (key != null)
        {
          if (key.KeyCode == Keys.Escape)
            flag1 = false;
        }
        else
        {
          Point map2 = this.MouseToMap(mousePos);
          if (map1.IsInBounds(map2) && this.IsInViewRect(map2))
          {
            if (this.IsVisibleToPlayer(map1, map2) && followerFOV.Contains(map2))
            {
              bool flag3 = true;
              string reason = "";
              if (map1.GetZonesAt(map2.X, map2.Y) == null)
              {
                flag3 = false;
                reason = "no zone here";
              }
              else if (!(map2 == follower.Location.Position) && !this.m_Rules.IsWalkableFor(follower, map1, map2.X, map2.Y, out reason))
                flag3 = false;
              if (flag3)
              {
                nullable = new Point?(map2);
                color = Color.LightGreen;
                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left)
                {
                  this.DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.PATROL, new Location(map1, map2)));
                  flag1 = false;
                  flag2 = true;
                }
              }
              else
              {
                nullable = new Point?(map2);
                color = Color.Red;
                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left)
                {
                  this.AddMessage(this.MakeErrorMessage(string.Format("Can't patrol here : {0}", (object) reason)));
                  this.AddMessagePressEnter();
                }
              }
            }
            else
            {
              nullable = new Point?(map2);
              color = Color.Red;
            }
          }
        }
      }
      while (flag1);
      return flag2;
    }

    private bool HandlePlayerOrderFollowerToDropAllItems(Actor player, Actor follower)
    {
      if (follower.Inventory.IsEmpty)
        return false;
      this.DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.DROP_ALL_ITEMS, follower.Location));
      this.DoSay(follower, player, "Well ok...", RogueGame.Sayflags.IS_FREE_ACTION);
      ModifyActorTrustInLeader(follower, follower.Inventory.CountItems * Rules.TRUST_GIVE_ITEM_ORDER_PENALTY, true);
      return true;
    }

    private bool HandlePlayerOrderFollowerToReport(Actor player, Actor follower)
    {
      this.DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.REPORT_EVENTS, follower.Location));
      return true;
    }

    private bool HandlePlayerOrderFollowerToSleep(Actor player, Actor follower)
    {
      this.DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.SLEEP_NOW, follower.Location));
      return true;
    }

    private bool HandlePlayerOrderFollowerToToggleFollow(Actor player, Actor follower)
    {
      this.DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.FOLLOW_TOGGLE, follower.Location));
      return true;
    }

    private bool HandlePlayerOrderFollowerToReportPosition(Actor player, Actor follower)
    {
      this.DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.WHERE_ARE_YOU, follower.Location));
      return true;
    }

    private bool HandlePlayerOrderFollowerToGiveItems(Actor player, Actor follower)
    {
      if (follower.Inventory == null || follower.Inventory.IsEmpty)
      {
        this.ClearMessages();
        this.AddMessage(this.MakeErrorMessage(string.Format("{0} has no items to give.", (object) follower.TheName)));
        this.AddMessagePressEnter();
        return false;
      }
      if (player.Location.Map != follower.Location.Map || !this.m_Rules.IsAdjacent(player.Location.Position, follower.Location.Position))
      {
        this.ClearMessages();
        this.AddMessage(this.MakeErrorMessage(string.Format("{0} is not next to you.", (object) follower.TheName)));
        this.AddMessagePressEnter();
        return false;
      }
      bool flag1 = true;
      bool flag2 = false;
      int num1 = 0;
      Inventory inventory = follower.Inventory;
      do
      {
        this.ClearOverlays();
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.ORDER_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, new Point(0, 0)));
        this.ClearMessages();
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Ordering {0} to give...", (object) follower.Name), this.m_Session.WorldTime.TurnCounter, Color.Yellow));
        int num2;
        for (num2 = 0; num2 < 5 && num1 + num2 < inventory.CountItems; ++num2)
        {
          int index = num1 + num2;
          this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0}. {1}/{2} {3}.", (object) (1 + num2), (object) (index + 1), (object) inventory.CountItems, (object) this.DescribeItemShort(inventory[index])), this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        }
        if (num2 < inventory.CountItems)
          this.AddMessage(new djack.RogueSurvivor.Data.Message("9. next", this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
        this.RedrawPlayScreen();
        KeyEventArgs keyEventArgs = this.m_UI.UI_WaitKey();
        int choiceNumber = this.KeyToChoiceNumber(keyEventArgs.KeyCode);
        if (keyEventArgs.KeyCode == Keys.Escape)
          flag1 = false;
        else if (choiceNumber == 9)
        {
          num1 += 5;
          if (num1 >= inventory.CountItems)
            num1 = 0;
        }
        else if (choiceNumber >= 1 && choiceNumber <= num2)
        {
          int index = num1 + choiceNumber - 1;
          Item obj = inventory[index];
          string reason;
          if (this.m_Rules.CanActorGiveItemTo(follower, player, obj, out reason))
          {
            this.DoGiveItemTo(follower, this.m_Player, obj);
            flag1 = false;
            flag2 = true;
          }
          else
          {
            this.ClearMessages();
            this.AddMessage(this.MakeErrorMessage(string.Format("{0} cannot give {1} : {2}.", (object) follower.TheName, (object) this.DescribeItemShort(obj), (object) reason)));
            this.AddMessagePressEnter();
          }
        }
      }
      while (flag1);
      return flag2;
    }

    private void HandleAiActor(Actor aiActor)
    {
      ActorAction actorAction = aiActor.Controller.GetAction(this);
      if (this.m_Rules.IsActorInsane(aiActor) && this.m_Rules.RollChance(5))
      {
        ActorAction insaneAction = this.GenerateInsaneAction(aiActor);
        if (insaneAction != null && insaneAction.IsLegal())
          actorAction = insaneAction;
      }
      if (actorAction == null)
        throw new InvalidOperationException("AI returned null action.");
      if (actorAction.IsLegal())
      {
        actorAction.Perform();
      }
      else
      {
        this.SpendActorActionPoints(aiActor, Rules.BASE_ACTION_COST);
        throw new InvalidOperationException(string.Format("AI attempted illegal action {0}; actorAI: {1}; fail reason : {2}.", (object) actorAction.GetType().ToString(), (object) aiActor.Controller.GetType().ToString(), (object) actorAction.FailReason));
      }
    }

    private void HandleAdvisor(Actor player)
    {
      if (RogueGame.s_Hints.HasAdvisorGivenAllHints())
      {
        this.ShowAdvisorMessage("YOU KNOW THE BASICS!", new string[7]
        {
          "The Advisor has given you all the hints.",
          "You can disable the advisor in the options.",
          "Read the manual or discover the rest of the game by yourself.",
          "Good luck and have fun!",
          string.Format("To REDEFINE THE KEYS : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.KEYBINDING_MODE).ToString()),
          string.Format("To CHANGE OPTIONS    : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.OPTIONS_MODE).ToString()),
          string.Format("To READ THE MANUAL   : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.HELP_MODE).ToString())
        });
      }
      else
      {
        for (int index = 0; index < 46; ++index)
        {
          if (!RogueGame.s_Hints.IsAdvisorHintGiven((AdvisorHint) index) && this.IsAdvisorHintAppliable((AdvisorHint) index))
          {
            this.AdvisorGiveHint((AdvisorHint) index);
            return;
          }
        }
        this.ShowAdvisorMessage("No hint available.", new string[5]
        {
          "The Advisor has now new hint for you in this situation.",
          "You will see a popup when he has something to say.",
          string.Format("To REDEFINE THE KEYS : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.KEYBINDING_MODE).ToString()),
          string.Format("To CHANGE OPTIONS    : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.OPTIONS_MODE).ToString()),
          string.Format("To READ THE MANUAL   : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.HELP_MODE).ToString())
        });
      }
    }

    private bool HasAdvisorAnyHintToGive()
    {
      for (int index = 0; index < 46; ++index)
      {
        if (!RogueGame.s_Hints.IsAdvisorHintGiven((AdvisorHint) index) && this.IsAdvisorHintAppliable((AdvisorHint) index))
          return true;
      }
      return false;
    }

    private void AdvisorGiveHint(AdvisorHint hint)
    {
      RogueGame.s_Hints.SetAdvisorHintAsGiven(hint);
      this.SaveHints();
      this.ShowAdvisorHint(hint);
    }

    private bool IsAdvisorHintAppliable(AdvisorHint hint)
    {
      Map map = this.m_Player.Location.Map;
      Point position = this.m_Player.Location.Position;
      switch (hint)
      {
        case AdvisorHint._FIRST:
          return true;
        case AdvisorHint.MOUSE_LOOK:
          return map.LocalTime.TurnCounter >= 2;
        case AdvisorHint.KEYS_OPTIONS:
          return true;
        case AdvisorHint.NIGHT:
          return map.LocalTime.TurnCounter >= WorldTime.TURNS_PER_HOUR;
        case AdvisorHint.RAIN:
          if (m_Rules.IsWeatherRain(m_Session.World.Weather))
            return map.LocalTime.TurnCounter >= 2*WorldTime.TURNS_PER_HOUR;
          return false;
        case AdvisorHint.ACTOR_MELEE:
          return this.IsAdjacentToEnemy(map, position, this.m_Player);
        case AdvisorHint.MOVE_RUN:
          if (map.LocalTime.TurnCounter >= 5)
            return this.m_Rules.CanActorRun(this.m_Player);
          return false;
        case AdvisorHint.MOVE_RESTING:
          return this.m_Rules.IsActorTired(this.m_Player);
        case AdvisorHint.MOVE_JUMP:
          if (!this.m_Rules.IsActorTired(this.m_Player))
            return map.HasAnyAdjacentInMap(position, (Predicate<Point>) (pt =>
            {
              MapObject mapObjectAt = map.GetMapObjectAt(pt);
              if (mapObjectAt == null)
                return false;
              return mapObjectAt.IsJumpable;
            }));
          return false;
        case AdvisorHint.ITEM_GRAB_CONTAINER:
          return map.HasAnyAdjacentInMap(position, (Predicate<Point>) (pt => this.m_Rules.CanActorGetItemFromContainer(this.m_Player, pt)));
        case AdvisorHint.ITEM_GRAB_FLOOR:
          Inventory itemsAt = map.GetItemsAt(position);
          if (itemsAt == null)
            return false;
          foreach (Item it in itemsAt.Items)
          {
            if (this.m_Rules.CanActorGetItem(this.m_Player, it))
              return true;
          }
          return false;
        case AdvisorHint.ITEM_UNEQUIP:
          Inventory inventory1 = this.m_Player.Inventory;
          if (inventory1 == null || inventory1.IsEmpty)
            return false;
          foreach (Item it in inventory1.Items)
          {
            if (Rules.CanActorUnequipItem(m_Player, it)) return true;
          }
          return false;
        case AdvisorHint.ITEM_EQUIP:
          Inventory inventory2 = this.m_Player.Inventory;
          if (inventory2 == null || inventory2.IsEmpty)
            return false;
          foreach (Item it in inventory2.Items)
          {
            if (!it.IsEquipped && this.m_Rules.CanActorEquipItem(this.m_Player, it))
              return true;
          }
          return false;
        case AdvisorHint.ITEM_TYPE_BARRICADING:
          Inventory inventory3 = this.m_Player.Inventory;
          if (inventory3 == null || inventory3.IsEmpty)
            return false;
          return inventory3.HasItemOfType(typeof (ItemBarricadeMaterial));
        case AdvisorHint.ITEM_DROP:
          Inventory inventory4 = this.m_Player.Inventory;
          if (inventory4 == null || inventory4.IsEmpty)
            return false;
          foreach (Item it in inventory4.Items)
          {
            if (this.m_Rules.CanActorDropItem(this.m_Player, it))
              return true;
          }
          return false;
        case AdvisorHint.ITEM_USE:
          Inventory inventory5 = this.m_Player.Inventory;
          if (inventory5 == null || inventory5.IsEmpty)
            return false;
          foreach (Item it in inventory5.Items)
          {
            if (this.m_Rules.CanActorUseItem(this.m_Player, it))
              return true;
          }
          return false;
        case AdvisorHint.FLASHLIGHT:
          return this.m_Player.Inventory.HasItemOfType(typeof (ItemLight));
        case AdvisorHint.CELLPHONES:
          return this.m_Player.Inventory.GetFirstByModel((ItemModel) this.GameItems.CELL_PHONE) != null;
        case AdvisorHint.SPRAYS_PAINT:
          return this.m_Player.Inventory.HasItemOfType(typeof (ItemSprayPaint));
        case AdvisorHint.SPRAYS_SCENT:
          return this.m_Player.Inventory.HasItemOfType(typeof (ItemSprayScent));
        case AdvisorHint.WEAPON_FIRE:
          ItemRangedWeapon itemRangedWeapon = this.m_Player.GetEquippedWeapon() as ItemRangedWeapon;
          if (itemRangedWeapon == null)
            return false;
          return itemRangedWeapon.Ammo >= 0;
        case AdvisorHint.WEAPON_RELOAD:
          if (!(this.m_Player.GetEquippedWeapon() is ItemRangedWeapon))
            return false;
          Inventory inventory6 = this.m_Player.Inventory;
          if (inventory6 == null || inventory6.IsEmpty)
            return false;
          foreach (Item it in inventory6.Items)
          {
            if (it is ItemAmmo && this.m_Rules.CanActorUseItem(this.m_Player, it))
              return true;
          }
          return false;
        case AdvisorHint.GRENADE:
          Inventory inventory7 = this.m_Player.Inventory;
          if (inventory7 == null || inventory7.IsEmpty)
            return false;
          return inventory7.HasItemOfType(typeof (ItemGrenade));
        case AdvisorHint.DOORWINDOW_OPEN:
          return map.HasAnyAdjacentInMap(position, (Predicate<Point>) (pt =>
          {
            DoorWindow door = map.GetMapObjectAt(pt) as DoorWindow;
            if (door == null)
              return false;
            return this.m_Rules.IsOpenableFor(this.m_Player, door);
          }));
        case AdvisorHint.DOORWINDOW_CLOSE:
          return map.HasAnyAdjacentInMap(position, (Predicate<Point>) (pt =>
          {
            DoorWindow door = map.GetMapObjectAt(pt) as DoorWindow;
            if (door == null)
              return false;
            return this.m_Rules.IsClosableFor(this.m_Player, door);
          }));
        case AdvisorHint.OBJECT_PUSH:
          return map.HasAnyAdjacentInMap(position, (Predicate<Point>) (pt =>
          {
            MapObject mapObjectAt = map.GetMapObjectAt(pt);
            if (mapObjectAt == null)
              return false;
            return this.m_Rules.CanActorPush(this.m_Player, mapObjectAt);
          }));
        case AdvisorHint.OBJECT_BREAK:
          return map.HasAnyAdjacentInMap(position, (Predicate<Point>) (pt =>
          {
            MapObject mapObjectAt = map.GetMapObjectAt(pt);
            if (mapObjectAt == null)
              return false;
            return this.m_Rules.IsBreakableFor(this.m_Player, mapObjectAt);
          }));
        case AdvisorHint.BARRICADE:
          return map.HasAnyAdjacentInMap(position, (Predicate<Point>) (pt =>
          {
            DoorWindow door = map.GetMapObjectAt(pt) as DoorWindow;
            if (door == null)
              return false;
            return this.m_Rules.CanActorBarricadeDoor(this.m_Player, door);
          }));
        case AdvisorHint.EXIT_STAIRS_LADDERS:
          return map.GetExitAt(position) != null;
        case AdvisorHint.EXIT_LEAVING_DISTRICT:
          foreach (Direction direction in Direction.COMPASS)
          {
            Point point = position + direction;
            if (!map.IsInBounds(point) && map.GetExitAt(point) != null)
              return true;
          }
          return false;
        case AdvisorHint.STATE_SLEEPY:
          return this.m_Rules.IsActorSleepy(this.m_Player);
        case AdvisorHint.STATE_HUNGRY:
          return this.m_Rules.IsActorHungry(this.m_Player);
        case AdvisorHint.NPC_TRADE:
          return map.HasAnyAdjacentInMap(position, (Predicate<Point>) (pt =>
          {
            Actor actorAt = map.GetActorAt(pt);
            if (actorAt == null)
              return false;
            return this.m_Rules.CanActorInitiateTradeWith(this.m_Player, actorAt);
          }));
        case AdvisorHint.NPC_GIVING_ITEM:
          Inventory inventory8 = this.m_Player.Inventory;
          if (inventory8 == null || inventory8.IsEmpty)
            return false;
          return map.HasAnyAdjacentInMap(position, (Predicate<Point>) (pt =>
          {
            Actor actorAt = map.GetActorAt(pt);
            if (actorAt == null)
              return false;
            return !this.m_Rules.IsEnemyOf(this.m_Player, actorAt);
          }));
        case AdvisorHint.NPC_SHOUTING:
          return map.HasAnyAdjacentInMap(position, (Predicate<Point>) (pt =>
          {
            Actor actorAt = map.GetActorAt(pt);
            if (actorAt == null || !actorAt.IsSleeping)
              return false;
            return !this.m_Rules.IsEnemyOf(this.m_Player, actorAt);
          }));
        case AdvisorHint.BUILD_FORTIFICATION:
          return map.HasAnyAdjacentInMap(position, (Predicate<Point>) (pt => this.m_Rules.CanActorBuildFortification(this.m_Player, pt, false)));
        case AdvisorHint.LEADING_NEED_SKILL:
          return map.HasAnyAdjacentInMap(position, (Predicate<Point>) (pt =>
          {
            Actor actorAt = map.GetActorAt(pt);
            if (actorAt == null)
              return false;
            return !this.m_Rules.IsEnemyOf(this.m_Player, actorAt);
          }));
        case AdvisorHint.LEADING_CAN_RECRUIT:
          return map.HasAnyAdjacentInMap(position, (Predicate<Point>) (pt =>
          {
            Actor actorAt = map.GetActorAt(pt);
            if (actorAt == null)
              return false;
            return this.m_Rules.CanActorTakeLead(this.m_Player, actorAt);
          }));
        case AdvisorHint.LEADING_GIVE_ORDERS:
          return this.m_Player.CountFollowers > 0;
        case AdvisorHint.LEADING_SWITCH_PLACE:
          return map.HasAnyAdjacentInMap(position, (Predicate<Point>) (pt =>
          {
            Actor actorAt = map.GetActorAt(pt);
            if (actorAt == null)
              return false;
            return this.m_Rules.CanActorSwitchPlaceWith(this.m_Player, actorAt);
          }));
        case AdvisorHint.GAME_SAVE_LOAD:
          return map.LocalTime.Hour >= 7;
        case AdvisorHint.CITY_INFORMATION:
          return map.LocalTime.Hour >= 12;
        case AdvisorHint.CORPSE_BUTCHER:
          if (!this.m_Player.Model.Abilities.IsUndead)
            return map.GetCorpsesAt(position) != null;
          return false;
        case AdvisorHint.CORPSE_EAT:
          if (this.m_Player.Model.Abilities.IsUndead)
            return map.GetCorpsesAt(position) != null;
          return false;
        case AdvisorHint.CORPSE_DRAG_START:
          if (this.m_Player.DraggedCorpse == null)
            return map.GetCorpsesAt(position) != null;
          return false;
        case AdvisorHint.CORPSE_DRAG_MOVE:
          return this.m_Player.DraggedCorpse != null;
        default:
          throw new ArgumentOutOfRangeException("unhandled hint");
      }
    }

    private void GetAdvisorHintText(AdvisorHint hint, out string title, out string[] body)
    {
      switch (hint)
      {
        case AdvisorHint._FIRST:
          title = "MOVEMENT - DIRECTIONS";
          body = new string[12]
          {
            "MOVE your character around with the movements keys.",
            "The default keys are your NUMPAD numbers.",
            "",
            "7 8 9",
            "4 - 6",
            "1 2 3",
            "",
            "5 makes you WAIT one turn.",
            "The move keys are the most important ones.",
            "When asked for a DIRECTION, press a MOVE key.",
            "Be sure to remember that!",
            "...and remember to keep NumLock on!"
          };
          break;
        case AdvisorHint.MOUSE_LOOK:
          title = "LOOKING WITH THE MOUSE";
          body = new string[4]
          {
            "You can LOOK at actors and objects on the map.",
            "Move the MOUSE over something interesting.",
            "You will get a detailed description of the actor or object.",
            "This is useful to learn the game or assessing the tactical situation."
          };
          break;
        case AdvisorHint.KEYS_OPTIONS:
          title = "KEYS & OPTIONS";
          body = new string[4]
          {
            string.Format("You can view and redefine the KEYS by pressing <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.KEYBINDING_MODE).ToString()),
            string.Format("You can change OPTIONS by pressing <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.OPTIONS_MODE).ToString()),
            "Some option changes will only take effect when starting a new game.",
            "Keys and Options are saved."
          };
          break;
        case AdvisorHint.NIGHT:
          title = "NIGHT TIME";
          body = new string[3]
          {
            "It is night. Night time is penalizing for livings.",
            "They tire faster (stamina and sleep) and don't see very far.",
            "Undeads are not penalized by night at all."
          };
          break;
        case AdvisorHint.RAIN:
          title = "RAIN";
          body = new string[4]
          {
            "It is raining. Rain has various effects.",
            "Livings vision is reduced.",
            "Firearms have more chance to jam.",
            "Scents evaporate faster."
          };
          break;
        case AdvisorHint.ACTOR_MELEE:
          title = "ATTACK AN ENEMY IN MELEE";
          body = new string[2]
          {
            "You are next to an enemy.",
            "To ATTACK him, try to MOVE on him."
          };
          break;
        case AdvisorHint.MOVE_RUN:
          title = "MOVEMENT - RUNNING";
          body = new string[3]
          {
            "You can RUN to move faster.",
            "Running is tiring and spend stamina.",
            string.Format("To TOGGLE RUNNING : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.RUN_TOGGLE).ToString())
          };
          break;
        case AdvisorHint.MOVE_RESTING:
          title = "MOVEMENT - RESTING";
          body = new string[7]
          {
            "You are TIRED because you lost too much STAMINA.",
            "Being tired is bad for you!",
            "You move slowly.",
            "You can't do tiring activities such as running, fighting and jumping.",
            "You always recover a bit of stamina each turn.",
            "But you can REST to recover stamina faster.",
            string.Format("To REST/WAIT : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.WAIT_OR_SELF).ToString())
          };
          break;
        case AdvisorHint.MOVE_JUMP:
          title = "MOVEMENT - JUMPING";
          body = new string[6]
          {
            "You can JUMP on or over an obstacle next to you.",
            "Typical jumpable objects are cars, fences and furnitures.",
            "The object is described with 'Can be jumped on'.",
            "Some enemies can't jump and won't be able to follow you.",
            "Jumping is tiring and spend stamina.",
            "To jump, just MOVE on the obstacle."
          };
          break;
        case AdvisorHint.ITEM_GRAB_CONTAINER:
          title = "TAKING AN ITEM FROM A CONTAINER";
          body = new string[2]
          {
            "You are next to a container, such as a warbrobe or a shelf.",
            "You can TAKE the item there by MOVING into the object."
          };
          break;
        case AdvisorHint.ITEM_GRAB_FLOOR:
          title = "TAKING AN ITEM FROM THE FLOOR";
          body = new string[3]
          {
            "You are standing on a stack of items.",
            "The items are listed on the right panel in the ground inventory.",
            "To TAKE an item, move your mouse on the item on the ground inventory and LEFT CLICK."
          };
          break;
        case AdvisorHint.ITEM_UNEQUIP:
          title = "UNEQUIPING AN ITEM";
          body = new string[3]
          {
            "You have equiped an item.",
            "The item is displayed with a green background.",
            "To UNEQUIP the item, LEFT CLICK on it in your inventory."
          };
          break;
        case AdvisorHint.ITEM_EQUIP:
          title = "EQUIPING AN ITEM";
          body = new string[3]
          {
            "You have an equipable item in your inventory.",
            "Typical equipable items are weapons, lights and phones.",
            "To EQUIP the item, LEFT CLICK on it in your inventory."
          };
          break;
        case AdvisorHint.ITEM_TYPE_BARRICADING:
          title = "ITEM - BARRICADING MATERIAL";
          body = new string[3]
          {
            "You have some barricading materials, such as planks.",
            "Barricading material is used when you barricade doors/windows or build fortifications.",
            "To build fortifications you need the CARPENTRY skill."
          };
          break;
        case AdvisorHint.ITEM_DROP:
          title = "DROPPING AN ITEM";
          body = new string[3]
          {
            "You can drop items from your inventory.",
            "To DROP an item, RIGHT CLICK on it.",
            "The item must be unequiped first."
          };
          break;
        case AdvisorHint.ITEM_USE:
          title = "USING AN ITEM";
          body = new string[3]
          {
            "You can use one of your item.",
            "Typical usable items are food, medecine and ammunition.",
            "To USE the item, LEFT CLICK on it in your inventory."
          };
          break;
        case AdvisorHint.FLASHLIGHT:
          title = "LIGHTING";
          body = new string[3]
          {
            "You have found a lighting item, such as a flashlight.",
            "Equip the item to increase your view distance (FoV).",
            "Standing next to someone with a light on has the same effect."
          };
          break;
        case AdvisorHint.CELLPHONES:
          title = "CELLPHONES";
          body = new string[3]
          {
            "You have found a cellphone.",
            "Cellphones are used to keep contact with your follower(s).",
            "You and your follower(s) must have a cellphone equipped."
          };
          break;
        case AdvisorHint.SPRAYS_PAINT:
          title = "SPRAYS - SPRAYPAINT";
          body = new string[4]
          {
            "You have found a can of spraypaint.",
            "You can tag a symbol on walls and floors.",
            "This is useful to mark some places and locations.",
            string.Format("To USE THE SPRAY : move the mouse over the item and press <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.USE_SPRAY).ToString())
          };
          break;
        case AdvisorHint.SPRAYS_SCENT:
          title = "SPRAYS - SCENT SPRAY";
          body = new string[4]
          {
            "You have found a scent spray.",
            "You can spray some perfume on the tile you are standing.",
            "This is useful to confuse the undeads that chase using their smell.",
            string.Format("To USE THE SPRAY : move the mouse over the item and press <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.USE_SPRAY).ToString())
          };
          break;
        case AdvisorHint.WEAPON_FIRE:
          title = "FIRING A WEAPON";
          body = new string[8]
          {
            "You can fire your equiped ranged weapon.",
            "You need to have valid targets.",
            "To fire on a target you need ammunitions and a clear line of fine.",
            "The target must be within the weapon range.",
            "The closer the target is, the easier it is to hit and it does slightly more damage.",
            string.Format("To FIRE : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.FIRE_MODE).ToString()),
            "Remember you need to have visible enemies to fire at.",
            "Read the manual for more explanation about firing and ranged weapons."
          };
          break;
        case AdvisorHint.WEAPON_RELOAD:
          title = "RELOADING A WEAPON";
          body = new string[2]
          {
            "You can reload your equiped ranged weapon.",
            "To RELOAD, just USE a compatible ammo item."
          };
          break;
        case AdvisorHint.GRENADE:
          title = "GRENADES";
          body = new string[3]
          {
            "You have found a grenade.",
            "To THROW a GRENADE, EQUIP it and FIRE it.",
            string.Format("To FIRE : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.FIRE_MODE).ToString())
          };
          break;
        case AdvisorHint.DOORWINDOW_OPEN:
          title = "OPENING A DOOR/WINDOW";
          body = new string[2]
          {
            "You are next to a closed door or window.",
            "To OPEN it, try to MOVE on it."
          };
          break;
        case AdvisorHint.DOORWINDOW_CLOSE:
          title = "CLOSING A DOOR/WINDOW";
          body = new string[2]
          {
            "You are next to an open door or window.",
            string.Format("To CLOSE : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.CLOSE_DOOR).ToString())
          };
          break;
        case AdvisorHint.OBJECT_PUSH:
          title = "PUSHING OBJECTS";
          body = new string[4]
          {
            "You can PUSH an object around you.",
            "Only MOVABLE objects can be pushed.",
            "Movable objects will be described as 'Can be moved'",
            string.Format("To PUSH : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.PUSH_MODE).ToString())
          };
          break;
        case AdvisorHint.OBJECT_BREAK:
          title = "BREAKING OBJECTS";
          body = new string[3]
          {
            "You can try to BREAK an object around you.",
            "Typical breakable objects are furnitures, doors and windows.",
            string.Format("To BREAK : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.BREAK_MODE).ToString())
          };
          break;
        case AdvisorHint.BARRICADE:
          title = "BARRICADING A DOOR/WINDOW";
          body = new string[3]
          {
            "You can barricade an adjacent door or window.",
            "Barricading uses material such as planks.",
            string.Format("To BARRICADE : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.BARRICADE_MODE).ToString())
          };
          break;
        case AdvisorHint.EXIT_STAIRS_LADDERS:
          title = "USING STAIRS & LADDERS";
          body = new string[3]
          {
            "You are standing on stairs or a ladder.",
            "You can use this exit to go on another map.",
            string.Format("To USE THE EXIT : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.USE_EXIT).ToString())
          };
          break;
        case AdvisorHint.EXIT_LEAVING_DISTRICT:
          title = "LEAVING THE DISTRICT";
          body = new string[2]
          {
            "You are next to a district EXIT.",
            "You can leave this district by MOVING into the exit."
          };
          break;
        case AdvisorHint.STATE_SLEEPY:
          title = "STATE - SLEEPY";
          body = new string[7]
          {
            "You are SLEEPY.",
            "This is bad for you!",
            "You have a number of penalties.",
            "You should find a place to SLEEP.",
            "Couches are good places to sleep.",
            string.Format("To SLEEP : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.SLEEP).ToString()),
            "Read the manual for more explanations on sleep."
          };
          break;
        case AdvisorHint.STATE_HUNGRY:
          title = "STATE - HUNGRY";
          body = new string[5]
          {
            "You are HUNGRY.",
            "If you become starved you can die!",
            "You should EAT soon.",
            "To eat, just USE a food item, such as groceries.",
            "Read the manual for more explanations on hunger."
          };
          break;
        case AdvisorHint.NPC_TRADE:
          title = "TRADING";
          body = new string[6]
          {
            "You can TRADE with an actor next to you.",
            "Actor that can trade with you have a $ icon on the map.",
            "Trading means exhanging items.",
            "To ask for a TRADE offer, just try to MOVE into the actor.",
            "You can also initiate the trade by offering an item you possess.",
            string.Format("To INITIATE THE TRADE : move the mouse over your item and press <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.INITIATE_TRADE).ToString())
          };
          break;
        case AdvisorHint.NPC_GIVING_ITEM:
          title = "GIVING ITEMS";
          body = new string[2]
          {
            "You can GIVE ITEMS to other actors.",
            string.Format("To GIVE AN ITEM : move the mouse over your item and press <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.GIVE_ITEM).ToString())
          };
          break;
        case AdvisorHint.NPC_SHOUTING:
          title = "SHOUTING";
          body = new string[4]
          {
            "Someone is sleeping near you.",
            "You can SHOUT to try to wake him or her up.",
            "Other actors can also shout to wake their friends up when they see danger.",
            string.Format("To SHOUT : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.SHOUT).ToString())
          };
          break;
        case AdvisorHint.BUILD_FORTIFICATION:
          title = "BUILDING FORTIFICATIONS";
          body = new string[4]
          {
            "You can now build fortifications thanks to the carpentry skill.",
            "You need enough barricading materials.",
            string.Format("To BUILD SMALL FORTIFICATIONS : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.BUILD_SMALL_FORTIFICATION).ToString()),
            string.Format("To BUILD LARGE FORTIFICATIONS : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.BUILD_LARGE_FORTIFICATION).ToString())
          };
          break;
        case AdvisorHint.LEADING_NEED_SKILL:
          title = "LEADING - LEADERSHIP SKILL";
          body = new string[2]
          {
            "You can try to recruit a follower if you have the LEADERSHIP skill.",
            "The higher the skill, the more followers you can recruit."
          };
          break;
        case AdvisorHint.LEADING_CAN_RECRUIT:
          title = "LEADING - RECRUITING";
          body = new string[2]
          {
            "You can recruit a follower next to you!",
            string.Format("To RECRUIT : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.LEAD_MODE).ToString())
          };
          break;
        case AdvisorHint.LEADING_GIVE_ORDERS:
          title = "LEADING - GIVING ORDERS";
          body = new string[4]
          {
            "You can give orders and directives to your follower.",
            "You can also fire your followers.",
            string.Format("To GIVE ORDERS : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.ORDER_MODE).ToString()),
            string.Format("To FIRE YOUR FOLLOWER : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.LEAD_MODE).ToString())
          };
          break;
        case AdvisorHint.LEADING_SWITCH_PLACE:
          title = "LEADING - SWITCHING PLACE";
          body = new string[2]
          {
            "You can switch place with followers next to you.",
            string.Format("To SWITCH PLACE : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.SWITCH_PLACE).ToString())
          };
          break;
        case AdvisorHint.GAME_SAVE_LOAD:
          title = "SAVING AND LOADING GAME";
          body = new string[7]
          {
            "Now could be a good time to save your game.",
            "You can have only one save game active.",
            string.Format("To SAVE THE GAME : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.SAVE_GAME).ToString()),
            string.Format("To LOAD THE GAME : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.LOAD_GAME).ToString()),
            "You can also load the game from the main menu.",
            "Saving or loading can take a bit of time, please be patient.",
            "Or consider turning some game options to lower settings."
          };
          break;
        case AdvisorHint.CITY_INFORMATION:
          title = "CITY INFORMATION";
          body = new string[3]
          {
            "You know the layout of your town.",
            "You aso know the most notable locations.",
            string.Format("To VIEW THE CITY INFORMATION : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.CITY_INFO).ToString())
          };
          break;
        case AdvisorHint.CORPSE_BUTCHER:
          title = "BUTCHERING CORPSES";
          body = new string[2]
          {
            "You can butcher a corpse.",
            string.Format("TO BUTCHER A CORPSE : RIGHT CLICK on it in the corpse list.")
          };
          break;
        case AdvisorHint.CORPSE_EAT:
          title = "EATING CORPSES";
          body = new string[2]
          {
            "You can eat a corpse to regain health.",
            string.Format("TO EAT A CORPSE : RIGHT CLICK on it in the corpse list.")
          };
          break;
        case AdvisorHint.CORPSE_DRAG_START:
          title = "DRAGGING CORPSES";
          body = new string[2]
          {
            "You can drag corpses.",
            string.Format("TO DRAG A CORPSE : LEFT CLICK on it in the corpse list.")
          };
          break;
        case AdvisorHint.CORPSE_DRAG_MOVE:
          title = "DRAGGING CORPSES";
          body = new string[2]
          {
            "You can move the dragged corpse with you.",
            string.Format("TO STOP DRAGGING THE CORPSE : LEFT CLICK on it in the corpse list.")
          };
          break;
        default:
          throw new ArgumentOutOfRangeException("unhandled hint");
      }
    }

    private void ShowAdvisorHint(AdvisorHint hint)
    {
      string title;
      string[] body;
      this.GetAdvisorHintText(hint, out title, out body);
      this.ShowAdvisorMessage(title, body);
    }

    private void ShowAdvisorMessage(string title, string[] lines)
    {
      this.ClearMessages();
      this.ClearOverlays();
      string[] lines1 = new string[lines.Length + 2];
      lines1[0] = "HINT : " + title;
      Array.Copy((Array) lines, 0, (Array) lines1, 1, lines.Length);
      lines1[lines.Length + 1] = string.Format("(hint {0}/{1})", (object) RogueGame.s_Hints.CountAdvisorHintsGiven(), (object) 46);
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(lines1, Color.White, Color.White, Color.Black, new Point(0, 0)));
      this.ClearMessages();
      this.AddMessage(new djack.RogueSurvivor.Data.Message("You can disable the advisor in the options screen.", this.m_Session.WorldTime.TurnCounter, Color.White));
      this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("To show the options screen : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.OPTIONS_MODE).ToString()), this.m_Session.WorldTime.TurnCounter, Color.White));
      this.AddMessagePressEnter();
      this.ClearMessages();
      this.ClearOverlays();
      this.RedrawPlayScreen();
    }

    private void WaitKeyOrMouse(out KeyEventArgs key, out Point mousePos, out MouseButtons? mouseButtons)
    {
      this.m_UI.UI_PeekKey();
      Point mousePosition = this.m_UI.UI_GetMousePosition();
      mousePos = new Point(-1, -1);
      mouseButtons = new MouseButtons?();
      do
      {
        KeyEventArgs keyEventArgs = this.m_UI.UI_PeekKey();
        if (keyEventArgs != null)
        {
          key = keyEventArgs;
          return;
        }
        mousePos = this.m_UI.UI_GetMousePosition();
        mouseButtons = this.m_UI.UI_PeekMouseButtons();
      }
      while (!(mousePos != mousePosition) && !mouseButtons.HasValue);
      key = (KeyEventArgs) null;
    }

    private Direction WaitDirectionOrCancel()
    {
      Direction direction;
      do
      {
        KeyEventArgs key = this.m_UI.UI_WaitKey();
        PlayerCommand command = InputTranslator.KeyToCommand(key);
        if (key.KeyCode == Keys.Escape)
          return (Direction) null;
        direction = RogueGame.CommandToDirection(command);
      }
      while (direction == null);
      return direction;
    }

    private void WaitEnter()
    {
      do
        ;
      while (this.m_UI.UI_WaitKey().KeyCode != Keys.Return);
    }

    private void WaitEscape()
    {
      do
        ;
      while (this.m_UI.UI_WaitKey().KeyCode != Keys.Escape);
    }

    private int KeyToChoiceNumber(Keys key)
    {
      switch (key)
      {
        case Keys.D0:
        case Keys.NumPad0:
          return 0;
        case Keys.D1:
        case Keys.NumPad1:
          return 1;
        case Keys.D2:
        case Keys.NumPad2:
          return 2;
        case Keys.D3:
        case Keys.NumPad3:
          return 3;
        case Keys.D4:
        case Keys.NumPad4:
          return 4;
        case Keys.D5:
        case Keys.NumPad5:
          return 5;
        case Keys.D6:
        case Keys.NumPad6:
          return 6;
        case Keys.D7:
        case Keys.NumPad7:
          return 7;
        case Keys.D8:
        case Keys.NumPad8:
          return 8;
        case Keys.D9:
        case Keys.NumPad9:
          return 9;
        default:
          return -1;
      }
    }

    private bool WaitYesOrNo()
    {
      KeyEventArgs keyEventArgs;
      do
      {
        keyEventArgs = this.m_UI.UI_WaitKey();
        if (keyEventArgs.KeyCode == Keys.Y)
          return true;
      }
      while (keyEventArgs.KeyCode != Keys.N && keyEventArgs.KeyCode != Keys.Escape);
      return false;
    }

    private string[] DescribeStuffAt(Map map, Point mapPos)
    {
      Actor actorAt = map.GetActorAt(mapPos);
      if (actorAt != null)
        return this.DescribeActor(actorAt);
      MapObject mapObjectAt = map.GetMapObjectAt(mapPos);
      if (mapObjectAt != null)
        return this.DescribeMapObject(mapObjectAt, map, mapPos);
      Inventory itemsAt = map.GetItemsAt(mapPos);
      if (itemsAt != null && !itemsAt.IsEmpty)
        return this.DescribeInventory(itemsAt);
      List<Corpse> corpsesAt = map.GetCorpsesAt(mapPos);
      if (corpsesAt != null)
        return this.DescribeCorpses(corpsesAt);
      return (string[]) null;
    }

    private string[] DescribeActor(Actor actor)
    {
      List<string> stringList = new List<string>(10);
      if (actor.Faction != null)
      {
        if (actor.IsInAGang)
          stringList.Add(string.Format("{0}, {1}-{2}.", (object) this.Capitalize(actor.Name), (object) actor.Faction.MemberName, (object) GameGangs.NAMES[actor.GangID]));
        else
          stringList.Add(string.Format("{0}, {1}.", (object) this.Capitalize(actor.Name), (object) actor.Faction.MemberName));
      }
      else
        stringList.Add(string.Format("{0}.", (object) this.Capitalize(actor.Name)));
      stringList.Add(string.Format("{0}.", (object) this.Capitalize(actor.Model.Name)));
      stringList.Add(string.Format("{0} since {1}.", actor.Model.Abilities.IsUndead ? (object) "Undead" : (object) "Staying alive", (object) new WorldTime(actor.SpawnTime).ToString()));
      AIController aiController = actor.Controller as AIController;
      if (aiController != null && aiController.Order != null)
        stringList.Add(string.Format("Order : {0}.", (object) aiController.Order.ToString()));
      if (actor.HasLeader)
      {
        if (actor.Leader.IsPlayer)
        {
          if (actor.TrustInLeader >= Rules.TRUST_BOND_THRESHOLD)
            stringList.Add(string.Format("Trust : BOND."));
          else if (actor.TrustInLeader >= Rules.TRUST_MAX)
            stringList.Add("Trust : MAX.");
          else
            stringList.Add(string.Format("Trust : {0}/T:{1}-B:{2}.", (object) actor.TrustInLeader, (object) Rules.TRUST_TRUSTING_THRESHOLD, (object) Rules.TRUST_MAX));
          OrderableAI orderableAi = aiController as OrderableAI;
          if (orderableAi != null && orderableAi.DontFollowLeader)
            stringList.Add("Ordered to not follow you.");
          stringList.Add(string.Format("Foo : {0} {1}h", (object) actor.FoodPoints, (object) this.FoodToHoursUntilHungry(actor.FoodPoints)));
          stringList.Add(string.Format("Slp : {0} {1}h", (object) actor.SleepPoints, (object) this.m_Rules.SleepToHoursUntilSleepy(actor.SleepPoints, actor.Location.Map.LocalTime.IsNight)));
          stringList.Add(string.Format("San : {0} {1}h", (object) actor.Sanity, (object) this.m_Rules.SanityToHoursUntilUnstable(actor)));
          stringList.Add(string.Format("Inf : {0} {1}%", (object) actor.Infection, (object) this.m_Rules.ActorInfectionPercent(actor)));
        }
        else
          stringList.Add(string.Format("Leader : {0}.", (object) this.Capitalize(actor.Leader.Name)));
      }
      if (actor.MurdersCounter > 0 && this.m_Player.Model.Abilities.IsLawEnforcer)
      {
        stringList.Add("WANTED FOR MURDER!");
        stringList.Add(string.Format("{0} murder{1}!", (object) actor.MurdersCounter, actor.MurdersCounter > 1 ? (object) "s" : (object) ""));
      }
      else if (actor.HasLeader && actor.Leader.IsPlayer && this.m_Rules.IsActorTrustingLeader(actor))
      {
        if (actor.MurdersCounter > 0)
          stringList.Add(string.Format("* Confess {0} murder{1}! *", (object) actor.MurdersCounter, actor.MurdersCounter > 1 ? (object) "s" : (object) ""));
        else
          stringList.Add("Has committed no murders.");
      }
      if (actor.IsAggressorOf(this.m_Player))
        stringList.Add("Aggressed you.");
      if (this.m_Player.IsSelfDefenceFrom(actor))
        stringList.Add(string.Format("You can kill {0} in self-defence.", (object) this.HimOrHer(actor)));
      if (this.m_Player.IsAggressorOf(actor))
        stringList.Add(string.Format("You aggressed {0}.", (object) this.HimOrHer(actor)));
      if (actor.IsSelfDefenceFrom(this.m_Player))
        stringList.Add("Killing you would be self-defence.");
      if (this.m_Player.AreIndirectEnemies(actor))
        stringList.Add("You are enemies through relationships.");
      stringList.Add("");
      string str = this.DescribeActorActivity(actor);
      if (str != null)
        stringList.Add(str);
      else
        stringList.Add(" ");
      if (actor.Model.Abilities.HasToSleep)
      {
        if (this.m_Rules.IsActorExhausted(actor))
          stringList.Add("Exhausted!");
        else if (this.m_Rules.IsActorSleepy(actor))
          stringList.Add("Sleepy.");
      }
      if (actor.Model.Abilities.HasToEat)
      {
        if (this.m_Rules.IsActorStarving(actor))
          stringList.Add("Starving!");
        else if (this.m_Rules.IsActorHungry(actor))
          stringList.Add("Hungry.");
      }
      else if (actor.Model.Abilities.IsRotting)
      {
        if (this.m_Rules.IsRottingActorStarving(actor))
          stringList.Add("Starving!");
        else if (this.m_Rules.IsRottingActorHungry(actor))
          stringList.Add("Hungry.");
      }
      if (actor.Model.Abilities.HasSanity)
      {
        if (this.m_Rules.IsActorInsane(actor))
          stringList.Add("Insane!");
        else if (this.m_Rules.IsActorDisturbed(actor))
          stringList.Add("Disturbed.");
      }
      stringList.Add(string.Format("Spd : {0:F2}", (object) (float) ((double) this.m_Rules.ActorSpeed(actor) / 100.0)));
      StringBuilder stringBuilder = new StringBuilder();
      int num1 = this.m_Rules.ActorMaxHPs(actor);
      if (actor.HitPoints != num1)
        stringBuilder.Append(string.Format("HP  : {0:D2}/{1:D2}", (object) actor.HitPoints, (object) num1));
      else
        stringBuilder.Append(string.Format("HP  : {0:D2} MAX", (object) actor.HitPoints));
      if (actor.Model.Abilities.CanTire)
      {
        int num2 = this.m_Rules.ActorMaxSTA(actor);
        if (actor.StaminaPoints != num2)
          stringBuilder.Append(string.Format("   STA : {0}/{1}", (object) actor.StaminaPoints, (object) num2));
        else
          stringBuilder.Append(string.Format("   STA : {0} MAX", (object) actor.StaminaPoints));
      }
      stringList.Add(stringBuilder.ToString());
      Attack attack = this.m_Rules.ActorMeleeAttack(actor, actor.CurrentMeleeAttack, (Actor) null);
      stringList.Add(string.Format("Atk : {0:D2} Dmg : {1:D2}", (object) attack.HitValue, (object) attack.DamageValue));
      Defence defence = this.m_Rules.ActorDefence(actor, actor.CurrentDefence);
      stringList.Add(string.Format("Def : {0:D2}", (object) defence.Value));
      stringList.Add(string.Format("Arm : {0}/{1}", (object) defence.Protection_Hit, (object) defence.Protection_Shot));
      stringList.Add(" ");
      stringList.Add(actor.Model.FlavorDescription);
      stringList.Add(" ");
      if (actor.Sheet.SkillTable != null && actor.Sheet.SkillTable.CountSkills > 0)
      {
        foreach (Skill skill in actor.Sheet.SkillTable.Skills)
          stringList.Add(string.Format("{0}-{1}", (object) skill.Level, (object) Skills.Name(skill.ID)));
        stringList.Add(" ");
      }
      if (actor.Inventory != null && !actor.Inventory.IsEmpty)
      {
        stringList.Add(string.Format("Items {0}/{1} : ", (object) actor.Inventory.CountItems, (object) this.m_Rules.ActorMaxInv(actor)));
        stringList.AddRange((IEnumerable<string>) this.DescribeInventory(actor.Inventory));
      }
      return stringList.ToArray();
    }

    private string DescribeActorActivity(Actor actor)
    {
      if (actor.IsPlayer)
        return (string) null;
      switch (actor.Activity)
      {
        case Activity.IDLE:
          return (string) null;
        case Activity.CHASING:
          if (actor.TargetActor == null)
            return "Chasing!";
          return string.Format("Chasing {0}!", (object) actor.TargetActor.Name);
        case Activity.FIGHTING:
          if (actor.TargetActor == null)
            return "Fighting!";
          return string.Format("Fighting {0}!", (object) actor.TargetActor.Name);
        case Activity.TRACKING:
          return "Tracking!";
        case Activity.FLEEING:
          return "Fleeing!";
        case Activity.FOLLOWING:
          if (actor.TargetActor == null)
            return "Following.";
          return string.Format("Following {0}.", (object) actor.TargetActor.Name);
        case Activity.SLEEPING:
          return "Sleeping.";
        case Activity.FOLLOWING_ORDER:
          return "Following orders.";
        case Activity.FLEEING_FROM_EXPLOSIVE:
          return "Fleeing from explosives!";
        default:
          throw new ArgumentException("unhandled activity " + (object) actor.Activity);
      }
    }

    private string DescribePlayerFollowerStatus(Actor follower)
    {
      BaseAI baseAi = follower.Controller as BaseAI;
      return (baseAi.Order != null ? baseAi.Order.ToString() : "(no orders)") + string.Format("(trust:{0})", (object) follower.TrustInLeader);
    }

    private string[] DescribeMapObject(MapObject obj, Map map, Point mapPos)
    {
      List<string> stringList = new List<string>(4);
      stringList.Add(string.Format("{0}.", (object) obj.AName));
      if (obj.IsJumpable)
        stringList.Add("Can be jumped on.");
      if (obj.IsCouch)
        stringList.Add("Is a couch.");
      if (obj.GivesWood)
        stringList.Add("Can be dismantled for wood.");
      if (obj.IsMovable)
        stringList.Add("Can be moved.");
      if (obj.StandOnFovBonus)
        stringList.Add("Increases view range.");
      StringBuilder stringBuilder = new StringBuilder();
      if (obj.BreakState == MapObject.Break.BROKEN)
        stringBuilder.Append("Broken! ");
      if (obj.FireState == MapObject.Fire.ONFIRE)
        stringBuilder.Append("On fire! ");
      else if (obj.FireState == MapObject.Fire.ASHES)
        stringBuilder.Append("Burnt to ashes! ");
      stringList.Add(stringBuilder.ToString());
      if (obj is PowerGenerator)
      {
        if ((obj as PowerGenerator).IsOn)
          stringList.Add("Currently ON.");
        else
          stringList.Add("Currently OFF.");
        float mapPowerRatio = this.m_Rules.ComputeMapPowerRatio(obj.Location.Map);
        stringList.Add(string.Format("The power gauge reads {0}%.", (object) (int) (100.0 * (double) mapPowerRatio)));
      }
      else if (obj is Board)
      {
        stringList.Add("The text reads : ");
        stringList.AddRange((IEnumerable<string>) (obj as Board).Text);
      }
      if (obj.MaxHitPoints > 0)
      {
        if (obj.HitPoints < obj.MaxHitPoints)
          stringList.Add(string.Format("HP        : {0}/{1}", (object) obj.HitPoints, (object) obj.MaxHitPoints));
        else
          stringList.Add(string.Format("HP        : {0} MAX", (object) obj.HitPoints));
        DoorWindow doorWindow = obj as DoorWindow;
        if (doorWindow != null)
        {
          if (doorWindow.BarricadePoints < 80)
            stringList.Add(string.Format("Barricades: {0}/{1}", (object) doorWindow.BarricadePoints, (object) 80));
          else
            stringList.Add(string.Format("Barricades: {0} MAX", (object) doorWindow.BarricadePoints));
        }
      }
      if (obj.Weight > 0)
        stringList.Add(string.Format("Weight    : {0}", (object) obj.Weight));
      Inventory itemsAt = map.GetItemsAt(mapPos);
      if (itemsAt != null && !itemsAt.IsEmpty)
        stringList.AddRange((IEnumerable<string>) this.DescribeInventory(itemsAt));
      return stringList.ToArray();
    }

    private string[] DescribeInventory(Inventory inv)
    {
      List<string> stringList = new List<string>(inv.CountItems);
      foreach (Item it in inv.Items)
      {
        if (it.IsEquipped)
          stringList.Add(string.Format("- {0} (equipped)", (object) this.DescribeItemShort(it)));
        else
          stringList.Add(string.Format("- {0}", (object) this.DescribeItemShort(it)));
      }
      return stringList.ToArray();
    }

    private string[] DescribeCorpses(List<Corpse> corpses)
    {
      List<string> stringList = new List<string>(corpses.Count + 2);
      if (corpses.Count > 1)
        stringList.Add("There are corpses there...");
      else
        stringList.Add("There is a corpse here.");
      stringList.Add(" ");
      foreach (Corpse corpse in corpses)
        stringList.Add(string.Format("- Corpse of {0}.", (object) corpse.DeadGuy.Name));
      return stringList.ToArray();
    }

    private string[] DescribeCorpseLong(Corpse c, bool isInPlayerTile)
    {
      List<string> stringList = new List<string>(10);
      stringList.Add(string.Format("Corpse of {0}.", (object) c.DeadGuy.Name));
      stringList.Add(" ");
      int skillLevel = this.m_Player.Sheet.SkillTable.GetSkillLevel(15);
      string str1 = "???";
      if (skillLevel > 0)
        str1 = WorldTime.MakeTimeDurationMessage(this.m_Session.WorldTime.TurnCounter - c.Turn);
      stringList.Add(string.Format("Death     : {0}.", (object) str1));
      string str2 = "???";
      if (skillLevel >= Rules.SKILL_NECROLOGY_LEVEL_FOR_INFECTION)
      {
        int num = this.m_Rules.ActorInfectionPercent(c.DeadGuy);
        str2 = num != 0 ? (num >= 5 ? (num >= 15 ? (num >= 30 ? (num >= 55 ? (num >= 70 ? (num >= 99 ? "7/7 - total" : "6/7 - great") : "5/7 - important") : "4/7 - average") : "3/7 - low") : "2/7 - minor") : "1/7 - traces") : "0/7 - none";
      }
      stringList.Add(string.Format("Infection : {0}.", (object) str2));
      string str3 = "???";
      if (skillLevel >= Rules.SKILL_NECROLOGY_LEVEL_FOR_RISE)
      {
        int num = 2 * this.m_Rules.CorpseZombifyChance(c, c.DeadGuy.Location.Map.LocalTime, false);
        str3 = num >= 5 ? (num >= 20 ? (num >= 40 ? (num >= 60 ? (num >= 80 ? (num >= 99 ? "6/6 - certain" : "5/6 - most likely") : "4/6 - very likely") : "3/6 - likely") : "2/6 - possible") : "1/6 - unlikely") : "0/6 - extremely unlikely";
      }
      stringList.Add(string.Format("Rise      : {0}.", (object) str3));
      stringList.Add(" ");
      switch (this.Rules.CorpseRotLevel(c))
      {
        case 0:
          stringList.Add("The corpse looks fresh.");
          break;
        case 1:
          stringList.Add("The corpse is bruised and smells.");
          break;
        case 2:
          stringList.Add("The corpse is damaged.");
          break;
        case 3:
          stringList.Add("The corpse is badly damaged.");
          break;
        case 4:
          stringList.Add("The corpse is almost entirely rotten.");
          break;
        case 5:
          stringList.Add("The corpse is about to crumble to dust.");
          break;
        default:
          throw new Exception("unhandled rot level");
      }
      string str4 = "???";
      if (this.m_Player.Sheet.SkillTable.GetSkillLevel(14) >= Rules.SKILL_MEDIC_LEVEL_FOR_REVIVE_EST)
      {
        int num = this.m_Rules.CorpseReviveChance(this.m_Player, c);
        str4 = num != 0 ? (num >= 5 ? (num >= 20 ? (num >= 40 ? (num >= 60 ? (num >= 80 ? (num >= 99 ? "6/6 - certain" : "5/6 - most likely") : "4/6 - very likely") : "3/6 - likely") : "2/6 - possible") : "1/6 - unlikely") : "0/6 - extremely unlikely") : "impossible";
      }
      stringList.Add(string.Format("Revive    : {0}.", (object) str4));
      if (isInPlayerTile)
      {
        stringList.Add(" ");
        stringList.Add("----");
        stringList.Add("LBM to start/stop dragging.");
        stringList.Add(string.Format("RBM to {0}.", this.m_Player.Model.Abilities.IsUndead ? (object) "eat" : (object) "butcher"));
        if (!this.m_Player.Model.Abilities.IsUndead)
        {
          stringList.Add(string.Format("to eat: <{0}>", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.EAT_CORPSE).ToString()));
          stringList.Add(string.Format("to revive : <{0}>", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.REVIVE_CORPSE).ToString()));
        }
      }
      return stringList.ToArray();
    }

    private string DescribeItemShort(Item it)
    {
      string str = it.Quantity > 1 ? it.Model.PluralName : it.AName;
      if (it is ItemFood)
      {
        ItemFood food = it as ItemFood;
        if (this.m_Rules.IsFoodSpoiled(food, this.m_Session.WorldTime.TurnCounter))
          str += " (spoiled)";
        else if (this.m_Rules.IsFoodExpired(food, this.m_Session.WorldTime.TurnCounter))
          str += " (expired)";
      }
      else if (it is ItemRangedWeapon)
      {
        ItemRangedWeapon itemRangedWeapon = it as ItemRangedWeapon;
        str += string.Format(" ({0}/{1})", (object) itemRangedWeapon.Ammo, (object) (itemRangedWeapon.Model as ItemRangedWeaponModel).MaxAmmo);
      }
      else if (it is ItemTrap)
      {
        ItemTrap itemTrap = it as ItemTrap;
        if (itemTrap.IsActivated)
          str += "(activated)";
        if (itemTrap.IsTriggered)
          str += "(triggered)";
      }
      if (it.Quantity > 1)
        return string.Format("{0} {1}", (object) it.Quantity, (object) str);
      return str;
    }

    private string[] DescribeItemLong(Item it, bool isPlayerInventory)
    {
      List<string> stringList = new List<string>();
      if (it.Model.IsStackable)
        stringList.Add(string.Format("{0} {1}/{2}", (object) this.DescribeItemShort(it), (object) it.Quantity, (object) it.Model.StackingLimit));
      else
        stringList.Add(this.DescribeItemShort(it));
      if (it.Model.IsUnbreakable)
        stringList.Add("Unbreakable.");
      string str = (string) null;
      if (it is ItemWeapon)
      {
        stringList.AddRange((IEnumerable<string>) this.DescribeItemWeapon(it as ItemWeapon));
        if (it is ItemRangedWeapon)
          str = string.Format("to fire : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.FIRE_MODE).ToString());
      }
      else if (it is ItemFood)
        stringList.AddRange((IEnumerable<string>) this.DescribeItemFood(it as ItemFood));
      else if (it is ItemMedicine)
        stringList.AddRange((IEnumerable<string>) this.DescribeItemMedicine(it as ItemMedicine));
      else if (it is ItemBarricadeMaterial)
      {
        stringList.AddRange((IEnumerable<string>) this.DescribeItemBarricadeMaterial(it as ItemBarricadeMaterial));
        str = string.Format("to use : <{0}>/<{1}>/<{2}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.BARRICADE_MODE).ToString(), (object) RogueGame.s_KeyBindings.Get(PlayerCommand.BUILD_SMALL_FORTIFICATION).ToString(), (object) RogueGame.s_KeyBindings.Get(PlayerCommand.BUILD_LARGE_FORTIFICATION).ToString());
      }
      else if (it is ItemBodyArmor)
        stringList.AddRange((IEnumerable<string>) this.DescribeItemBodyArmor(it as ItemBodyArmor));
      else if (it is ItemSprayPaint)
      {
        stringList.AddRange((IEnumerable<string>) this.DescribeItemSprayPaint(it as ItemSprayPaint));
        str = string.Format("to spray : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.USE_SPRAY).ToString());
      }
      else if (it is ItemSprayScent)
      {
        stringList.AddRange((IEnumerable<string>) this.DescribeItemSprayScent(it as ItemSprayScent));
        str = string.Format("to spray : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.USE_SPRAY).ToString());
      }
      else if (it is ItemLight)
        stringList.AddRange((IEnumerable<string>) this.DescribeItemLight(it as ItemLight));
      else if (it is ItemTracker)
        stringList.AddRange((IEnumerable<string>) this.DescribeItemTracker(it as ItemTracker));
      else if (it is ItemAmmo)
      {
        stringList.AddRange((IEnumerable<string>) this.DescribeItemAmmo(it as ItemAmmo));
        str = "to reload : left-click.";
      }
      else if (it is ItemExplosive)
      {
        stringList.AddRange((IEnumerable<string>) this.DescribeItemExplosive(it as ItemExplosive));
        str = string.Format("to throw : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.FIRE_MODE).ToString());
      }
      else if (it is ItemTrap)
        stringList.AddRange((IEnumerable<string>) this.DescribeItemTrap(it as ItemTrap));
      else if (it is ItemEntertainment)
        stringList.AddRange((IEnumerable<string>) this.DescribeItemEntertainment(it as ItemEntertainment));
      stringList.Add(" ");
      stringList.Add(it.Model.FlavorDescription);
      if (isPlayerInventory)
      {
        stringList.Add(" ");
        stringList.Add("----");
        stringList.Add(string.Format("to give : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.GIVE_ITEM).ToString()));
        stringList.Add(string.Format("to trade : <{0}>.", (object) RogueGame.s_KeyBindings.Get(PlayerCommand.INITIATE_TRADE).ToString()));
        if (str != null)
          stringList.Add(str);
      }
      return stringList.ToArray();
    }

    private string[] DescribeItemExplosive(ItemExplosive ex)
    {
      List<string> stringList = new List<string>();
      ItemExplosiveModel itemExplosiveModel = ex.Model as ItemExplosiveModel;
      ItemPrimedExplosive itemPrimedExplosive = ex as ItemPrimedExplosive;
      stringList.Add("> explosive");
      if (itemExplosiveModel.BlastAttack.CanDamageObjects)
        stringList.Add("Can damage objects.");
      if (itemExplosiveModel.BlastAttack.CanDestroyWalls)
        stringList.Add("Can destroy walls.");
      if (itemPrimedExplosive != null)
        stringList.Add(string.Format("Fuse          : {0} turn(s) left!", (object) itemPrimedExplosive.FuseTimeLeft));
      else
        stringList.Add(string.Format("Fuse          : {0} turn(s)", (object) itemExplosiveModel.FuseDelay));
      stringList.Add(string.Format("Blast radius  : {0}", (object) itemExplosiveModel.BlastAttack.Radius));
      StringBuilder stringBuilder = new StringBuilder();
      for (int distance = 0; distance <= itemExplosiveModel.BlastAttack.Radius; ++distance)
        stringBuilder.Append(string.Format("{0};", (object) this.m_Rules.BlastDamage(distance, itemExplosiveModel.BlastAttack)));
      stringList.Add(string.Format("Blast damages : {0}", (object) stringBuilder.ToString()));
      ItemGrenade itemGrenade = ex as ItemGrenade;
      if (itemGrenade != null)
      {
        stringList.Add("> grenade");
        ItemGrenadeModel itemGrenadeModel = itemGrenade.Model as ItemGrenadeModel;
        int num = this.m_Rules.ActorMaxThrowRange(this.m_Player, itemGrenadeModel.MaxThrowDistance);
        if (num != itemGrenadeModel.MaxThrowDistance)
          stringList.Add(string.Format("Throwing rng  : {0} ({1})", (object) num, (object) itemGrenadeModel.MaxThrowDistance));
        else
          stringList.Add(string.Format("Throwing rng  : {0}", (object) num));
      }
      if (itemPrimedExplosive != null)
        stringList.Add("PRIMED AND READY TO EXPLODE!");
      return stringList.ToArray();
    }

    private string[] DescribeItemWeapon(ItemWeapon w)
    {
      List<string> stringList = new List<string>();
      ItemWeaponModel itemWeaponModel = w.Model as ItemWeaponModel;
      stringList.Add("> weapon");
      stringList.Add(string.Format("Atk : +{0}", (object) itemWeaponModel.Attack.HitValue));
      stringList.Add(string.Format("Dmg : +{0}", (object) itemWeaponModel.Attack.DamageValue));
      stringList.Add(string.Format("Sta : -{0}", (object) itemWeaponModel.Attack.StaminaPenalty));
      ItemMeleeWeapon itemMeleeWeapon = w as ItemMeleeWeapon;
      if (itemMeleeWeapon != null)
      {
        if (itemMeleeWeapon.IsFragile)
          stringList.Add("Breaks easily.");
      }
      else
      {
        ItemRangedWeapon itemRangedWeapon = w as ItemRangedWeapon;
        if (itemRangedWeapon != null)
        {
          ItemRangedWeaponModel rangedWeaponModel = w.Model as ItemRangedWeaponModel;
          if (rangedWeaponModel.IsFireArm)
            stringList.Add("> firearm");
          else if (rangedWeaponModel.IsBow)
            stringList.Add("> bow");
          else
            stringList.Add("> ranged weapon");
          stringList.Add(string.Format("Rng  : {0}-{1}", (object) rangedWeaponModel.Attack.Range, (object) rangedWeaponModel.Attack.EfficientRange));
          if (itemRangedWeapon.Ammo < rangedWeaponModel.MaxAmmo)
            stringList.Add(string.Format("Amo  : {0}/{1}", (object) itemRangedWeapon.Ammo, (object) rangedWeaponModel.MaxAmmo));
          else
            stringList.Add(string.Format("Amo  : {0} MAX", (object) itemRangedWeapon.Ammo));
          stringList.Add(string.Format("Type : {0}", (object) this.DescribeAmmoType(rangedWeaponModel.AmmoType)));
        }
      }
      return stringList.ToArray();
    }

    private string DescribeAmmoType(AmmoType at)
    {
      switch (at)
      {
        case AmmoType._FIRST:
          return "light pistol bullets";
        case AmmoType.HEAVY_PISTOL:
          return "heavy pistol bullets";
        case AmmoType.SHOTGUN:
          return "shotgun cartridge";
        case AmmoType.LIGHT_RIFLE:
          return "light rifle bullets";
        case AmmoType.HEAVY_RIFLE:
          return "heavy rifle bullets";
        case AmmoType.BOLT:
          return "bolts";
        default:
          throw new ArgumentOutOfRangeException("unhandled ammo type");
      }
    }

    private string[] DescribeItemAmmo(ItemAmmo am)
    {
      return new List<string>()
      {
        "> ammo",
        string.Format("Type : {0}", (object) this.DescribeAmmoType(am.AmmoType))
      }.ToArray();
    }

    private string[] DescribeItemFood(ItemFood f)
    {
      List<string> stringList = new List<string>();
      ItemFoodModel itemFoodModel = f.Model as ItemFoodModel;
      stringList.Add("> food");
      if (f.IsPerishable)
      {
        if (this.m_Rules.IsFoodStillFresh(f, this.m_Session.WorldTime.TurnCounter))
          stringList.Add("Fresh.");
        else if (this.m_Rules.IsFoodExpired(f, this.m_Session.WorldTime.TurnCounter))
          stringList.Add("*Expired*");
        else if (this.m_Rules.IsFoodSpoiled(f, this.m_Session.WorldTime.TurnCounter))
          stringList.Add("**SPOILED**");
        stringList.Add(string.Format("Best-Before : {0}", (object) f.BestBefore.ToString()));
      }
      else
        stringList.Add("Always fresh.");
      int baseValue = this.m_Rules.FoodItemNutrition(f, this.m_Session.WorldTime.TurnCounter);
      int num = this.m_Player == null ? baseValue : this.m_Rules.ActorItemNutritionValue(this.m_Player, baseValue);
      if (num == itemFoodModel.Nutrition)
        stringList.Add(string.Format("Nutrition   : +{0}", (object) baseValue));
      else
        stringList.Add(string.Format("Nutrition   : +{0} (+{1})", (object) num, (object) baseValue));
      return stringList.ToArray();
    }

    private string[] DescribeItemMedicine(ItemMedicine med)
    {
      List<string> stringList = new List<string>();
      ItemMedicineModel itemMedicineModel = med.Model as ItemMedicineModel;
      stringList.Add("> medicine");
      int num1 = this.m_Player == null ? itemMedicineModel.Healing : this.m_Rules.ActorMedicineEffect(this.m_Player, itemMedicineModel.Healing);
      if (num1 == itemMedicineModel.Healing)
        stringList.Add(string.Format("Healing : +{0}", (object) itemMedicineModel.Healing));
      else
        stringList.Add(string.Format("Healing : +{0} (+{1})", (object) num1, (object) itemMedicineModel.Healing));
      int num2 = this.m_Player == null ? itemMedicineModel.StaminaBoost : this.m_Rules.ActorMedicineEffect(this.m_Player, itemMedicineModel.StaminaBoost);
      if (num2 == itemMedicineModel.StaminaBoost)
        stringList.Add(string.Format("Stamina : +{0}", (object) itemMedicineModel.StaminaBoost));
      else
        stringList.Add(string.Format("Stamina : +{0} (+{1})", (object) num2, (object) itemMedicineModel.StaminaBoost));
      int num3 = this.m_Player == null ? itemMedicineModel.SleepBoost : this.m_Rules.ActorMedicineEffect(this.m_Player, itemMedicineModel.SleepBoost);
      if (num3 == itemMedicineModel.SleepBoost)
        stringList.Add(string.Format("Sleep   : +{0}", (object) itemMedicineModel.SleepBoost));
      else
        stringList.Add(string.Format("Sleep   : +{0} (+{1})", (object) num3, (object) itemMedicineModel.SleepBoost));
      int num4 = this.m_Player == null ? itemMedicineModel.SanityCure : this.m_Rules.ActorMedicineEffect(this.m_Player, itemMedicineModel.SanityCure);
      if (num4 == itemMedicineModel.SanityCure)
        stringList.Add(string.Format("Sanity  : +{0}", (object) itemMedicineModel.SanityCure));
      else
        stringList.Add(string.Format("Sanity  : +{0} (+{1})", (object) num4, (object) itemMedicineModel.SanityCure));
      if (Rules.HasInfection(this.m_Session.GameMode))
      {
        int num5 = this.m_Player == null ? itemMedicineModel.InfectionCure : this.m_Rules.ActorMedicineEffect(this.m_Player, itemMedicineModel.InfectionCure);
        if (num5 == itemMedicineModel.InfectionCure)
          stringList.Add(string.Format("Cure    : +{0}", (object) itemMedicineModel.InfectionCure));
        else
          stringList.Add(string.Format("Cure    : +{0} (+{1})", (object) num5, (object) itemMedicineModel.InfectionCure));
      }
      return stringList.ToArray();
    }

    private string[] DescribeItemBarricadeMaterial(ItemBarricadeMaterial bm)
    {
      List<string> stringList = new List<string>();
      ItemBarricadeMaterialModel barricadeMaterialModel = bm.Model as ItemBarricadeMaterialModel;
      stringList.Add("> barricade material");
      int num = this.m_Player == null ? barricadeMaterialModel.BarricadingValue : this.m_Rules.ActorBarricadingPoints(this.m_Player, barricadeMaterialModel.BarricadingValue);
      if (num == barricadeMaterialModel.BarricadingValue)
        stringList.Add(string.Format("Barricading : +{0}", (object) barricadeMaterialModel.BarricadingValue));
      else
        stringList.Add(string.Format("Barricading : +{0} (+{1})", (object) num, (object) barricadeMaterialModel.BarricadingValue));
      return stringList.ToArray();
    }

    private string[] DescribeItemBodyArmor(ItemBodyArmor b)
    {
      List<string> stringList1 = new List<string>();
      stringList1.Add("> body armor");
      stringList1.Add(string.Format("Protection vs Hits  : +{0}", (object) b.Protection_Hit));
      stringList1.Add(string.Format("Protection vs Shots : +{0}", (object) b.Protection_Shot));
      stringList1.Add(string.Format("Encumbrance         : -{0} DEF", (object) b.Encumbrance));
      stringList1.Add(string.Format("Weight              : -{0:F2} SPD", (object) (float) (0.00999999977648258 * (double) b.Weight)));
      List<string> stringList2 = new List<string>();
      List<string> stringList3 = new List<string>();
      if (b.IsFriendlyForCops())
        stringList2.Add("Cops");
      if (b.IsHostileForCops())
        stringList3.Add("Cops");
      foreach (GameGangs.IDs gangID in GameGangs.BIKERS)
      {
        if (b.IsHostileForBiker(gangID))
          stringList3.Add(GameGangs.NAMES[(int) gangID]);
        if (b.IsFriendlyForBiker(gangID))
          stringList2.Add(GameGangs.NAMES[(int) gangID]);
      }
      foreach (GameGangs.IDs gangID in GameGangs.GANGSTAS)
      {
        if (b.IsHostileForBiker(gangID))
          stringList3.Add(GameGangs.NAMES[(int) gangID]);
        if (b.IsFriendlyForBiker(gangID))
          stringList2.Add(GameGangs.NAMES[(int) gangID]);
      }
      if (stringList2.Count > 0)
      {
        stringList1.Add("Unsuspicious to:");
        foreach (string str in stringList2)
          stringList1.Add("- " + str);
      }
      if (stringList3.Count > 0)
      {
        stringList1.Add("Suspicious to:");
        foreach (string str in stringList3)
          stringList1.Add("- " + str);
      }
      return stringList1.ToArray();
    }

    private string[] DescribeItemSprayPaint(ItemSprayPaint sp)
    {
      List<string> stringList = new List<string>();
      ItemSprayPaintModel itemSprayPaintModel = sp.Model as ItemSprayPaintModel;
      stringList.Add("> spray paint");
      if (sp.PaintQuantity < itemSprayPaintModel.MaxPaintQuantity)
        stringList.Add(string.Format("Paint : {0}/{1}", (object) sp.PaintQuantity, (object) itemSprayPaintModel.MaxPaintQuantity));
      else
        stringList.Add(string.Format("Paint : {0} MAX", (object) sp.PaintQuantity));
      return stringList.ToArray();
    }

    private string[] DescribeItemSprayScent(ItemSprayScent sp)
    {
      List<string> stringList = new List<string>();
      ItemSprayScentModel itemSprayScentModel = sp.Model as ItemSprayScentModel;
      stringList.Add("> spray scent");
      if (sp.SprayQuantity < itemSprayScentModel.MaxSprayQuantity)
        stringList.Add(string.Format("Spray : {0}/{1}", (object) sp.SprayQuantity, (object) itemSprayScentModel.MaxSprayQuantity));
      else
        stringList.Add(string.Format("Spray : {0} MAX", (object) sp.SprayQuantity));
      return stringList.ToArray();
    }

    private string[] DescribeItemLight(ItemLight lt)
    {
      List<string> stringList = new List<string>();
      ItemLightModel itemLightModel = lt.Model as ItemLightModel;
      stringList.Add("> light");
      stringList.Add(this.DescribeBatteries(lt.Batteries, itemLightModel.MaxBatteries));
      stringList.Add(string.Format("FOV       : +{0}", (object) lt.FovBonus));
      return stringList.ToArray();
    }

    private string[] DescribeItemTracker(ItemTracker tr)
    {
      List<string> stringList = new List<string>();
      ItemTrackerModel itemTrackerModel = tr.Model as ItemTrackerModel;
      stringList.Add("> tracker");
      stringList.Add(this.DescribeBatteries(tr.Batteries, itemTrackerModel.MaxBatteries));
      return stringList.ToArray();
    }

    private string[] DescribeItemTrap(ItemTrap tr)
    {
      List<string> stringList = new List<string>();
      ItemTrapModel itemTrapModel = tr.Model as ItemTrapModel;
      stringList.Add("> trap");
      if (tr.IsActivated)
        stringList.Add("** Activated! **");
      if (itemTrapModel.IsOneTimeUse)
        stringList.Add("Desactives when triggered.");
      if (itemTrapModel.IsNoisy)
        stringList.Add(string.Format("Makes {0} noise.", (object) itemTrapModel.NoiseName));
      if (itemTrapModel.UseToActivate)
        stringList.Add("Use to activate.");
      stringList.Add(string.Format("Damage  : {0}", (object) itemTrapModel.Damage));
      stringList.Add(string.Format("Trigger : {0}%", (object) itemTrapModel.TriggerChance));
      stringList.Add(string.Format("Break   : {0}%", (object) itemTrapModel.BreakChance));
      if (itemTrapModel.BlockChance > 0)
        stringList.Add(string.Format("Block   : {0}%", (object) itemTrapModel.BlockChance));
      if (itemTrapModel.BreakChanceWhenEscape > 0)
        stringList.Add(string.Format("{0}% to break on escape", (object) itemTrapModel.BreakChanceWhenEscape));
      return stringList.ToArray();
    }

    private string[] DescribeItemEntertainment(ItemEntertainment ent)
    {
      List<string> stringList = new List<string>();
      ItemEntertainmentModel entertainmentModel = ent.EntertainmentModel;
      stringList.Add("> entertainment");
      if (this.m_Player != null && this.m_Player.IsBoredOf((Item) ent))
        stringList.Add("* BORED OF IT! *");
      int num = this.m_Player == null ? entertainmentModel.Value : this.m_Rules.ActorSanRegenValue(this.m_Player, entertainmentModel.Value);
      if (num != entertainmentModel.Value)
        stringList.Add(string.Format("Sanity : +{0} (+{1})", (object) num, (object) entertainmentModel.Value));
      else
        stringList.Add(string.Format("Sanity : +{0}", (object) entertainmentModel.Value));
      stringList.Add(string.Format("Boring : {0}%", (object) entertainmentModel.BoreChance));
      return stringList.ToArray();
    }

    private string DescribeBatteries(int batteries, int maxBatteries)
    {
      int hours = this.BatteriesToHours(batteries);
      if (batteries < maxBatteries)
        return string.Format("Batteries : {0}/{1} ({2}h)", (object) batteries, (object) maxBatteries, (object) hours);
      return string.Format("Batteries : {0} MAX ({1}h)", (object) batteries, (object) hours);
    }

    private string DescribeSkillShort(Skills.IDs id)
    {
      switch (id)
      {
        case Skills.IDs._FIRST:
          return string.Format("+{0} melee ATK, +{1} DEF", (object) Rules.SKILL_AGILE_ATK_BONUS, (object) Rules.SKILL_AGILE_DEF_BONUS);
        case Skills.IDs.AWAKE:
          return string.Format("+{0}% max SLP, +{1}% SLP sleeping regen ", (object) (int) (100.0 * (double) Rules.SKILL_AWAKE_SLEEP_BONUS), (object) (int) (100.0 * (double) Rules.SKILL_AWAKE_SLEEP_REGEN_BONUS));
        case Skills.IDs.BOWS:
          return string.Format("bows +{0} Atk, +{1} Dmg", (object) Rules.SKILL_BOWS_ATK_BONUS, (object) Rules.SKILL_BOWS_DMG_BONUS);
        case Skills.IDs.CARPENTRY:
          return string.Format("build, -{0} mat. at lvl 3, +{1}% barricading", (object) Rules.SKILL_CARPENTRY_LEVEL3_BUILD_BONUS, (object) (int) (100.0 * (double) Rules.SKILL_CARPENTRY_BARRICADING_BONUS));
        case Skills.IDs.CHARISMATIC:
          return string.Format("+{0} trust per turn, +{1}% trade offers", (object) Rules.SKILL_CHARISMATIC_TRUST_BONUS, (object) Rules.SKILL_CHARISMATIC_TRADE_BONUS);
        case Skills.IDs.FIREARMS:
          return string.Format("firearms +{0} Atk, +{1} Dmg", (object) Rules.SKILL_FIREARMS_ATK_BONUS, (object) Rules.SKILL_FIREARMS_DMG_BONUS);
        case Skills.IDs.HARDY:
          return string.Format("sleep heals anywhere, +{0}% chance to heal", (object) Rules.SKILL_HARDY_HEAL_CHANCE_BONUS);
        case Skills.IDs.HAULER:
          return string.Format("+{0} inventory capacity", (object) Rules.SKILL_HAULER_INV_BONUS);
        case Skills.IDs.HIGH_STAMINA:
          return string.Format("+{0} STA", (object) Rules.SKILL_HIGH_STAMINA_STA_BONUS);
        case Skills.IDs.LEADERSHIP:
          return string.Format("+{0} max Followers", (object) Rules.SKILL_LEADERSHIP_FOLLOWER_BONUS);
        case Skills.IDs.LIGHT_EATER:
          return string.Format("+{0}% max FOO, +{1}% item food points", (object) (int) (100.0 * (double) Rules.SKILL_LIGHT_EATER_MAXFOOD_BONUS), (object) (int) (100.0 * (double) Rules.SKILL_LIGHT_EATER_FOOD_BONUS));
        case Skills.IDs.LIGHT_FEET:
          return string.Format("+{0}% to avoid and escape traps", (object) Rules.SKILL_LIGHT_FEET_TRAP_BONUS);
        case Skills.IDs.LIGHT_SLEEPER:
          return string.Format("+{0}% noise wake up chance", (object) Rules.SKILL_LIGHT_SLEEPER_WAKEUP_CHANCE_BONUS);
        case Skills.IDs.MARTIAL_ARTS:
          return string.Format("unarmed only melee +{0} Atk, +{1} Dmg", (object) Rules.SKILL_MARTIAL_ARTS_ATK_BONUS, (object) Rules.SKILL_MARTIAL_ARTS_DMG_BONUS);
        case Skills.IDs.MEDIC:
          return string.Format("+{0}% medicine effects, +{1}% revive ", (object) (int) (100.0 * (double) Rules.SKILL_MEDIC_BONUS), (object) Rules.SKILL_MEDIC_REVIVE_BONUS);
        case Skills.IDs.NECROLOGY:
          return string.Format("+{0}/+{1} Dmg vs undeads/corpses, data on corpses", (object) Rules.SKILL_NECROLOGY_UNDEAD_BONUS, (object) Rules.SKILL_NECROLOGY_CORPSE_BONUS);
        case Skills.IDs.STRONG:
          return string.Format("+{0} melee DMG, +{1} throw range", (object) Rules.SKILL_STRONG_DMG_BONUS, (object) Rules.SKILL_STRONG_THROW_BONUS);
        case Skills.IDs.STRONG_PSYCHE:
          return string.Format("+{0}% SAN threshold, +{1}% regen", (object) (int) (100.0 * (double) Rules.SKILL_STRONG_PSYCHE_LEVEL_BONUS), (object) (int) (100.0 * (double) Rules.SKILL_STRONG_PSYCHE_ENT_BONUS));
        case Skills.IDs.TOUGH:
          return string.Format("+{0} HP", (object) Rules.SKILL_TOUGH_HP_BONUS);
        case Skills.IDs.UNSUSPICIOUS:
          return string.Format("+{0}% unnoticed by law enforcers and gangs", (object) Rules.SKILL_UNSUSPICIOUS_BONUS);
        case Skills.IDs._FIRST_UNDEAD:
          return string.Format("+{0} melee ATK, +{1} DEF, can jump", (object) Rules.SKILL_ZAGILE_ATK_BONUS, (object) Rules.SKILL_ZAGILE_DEF_BONUS);
        case Skills.IDs.Z_EATER:
          return string.Format("+{0}% hp regen", (object) (int) (100.0 * (double) Rules.SKILL_ZEATER_REGEN_BONUS));
        case Skills.IDs.Z_GRAB:
          return string.Format("can grab enemies, +{0}% per level", (object) Rules.SKILL_ZGRAB_CHANCE);
        case Skills.IDs.Z_INFECTOR:
          return string.Format("+{0}% infection damage", (object) (int) (100.0 * (double) Rules.SKILL_ZINFECTOR_BONUS));
        case Skills.IDs.Z_LIGHT_EATER:
          return string.Format("+{0}% max ROT, +{1}% from eating", (object) (int) (100.0 * (double) Rules.SKILL_ZLIGHT_EATER_MAXFOOD_BONUS), (object) (int) (100.0 * (double) Rules.SKILL_ZLIGHT_EATER_FOOD_BONUS));
        case Skills.IDs.Z_LIGHT_FEET:
          return string.Format("+{0}% to avoid traps", (object) Rules.SKILL_ZLIGHT_FEET_TRAP_BONUS);
        case Skills.IDs.Z_STRONG:
          return string.Format("+{0} melee DMG, can push", (object) Rules.SKILL_ZSTRONG_DMG_BONUS);
        case Skills.IDs.Z_TOUGH:
          return string.Format("+{0} HP", (object) Rules.SKILL_ZTOUGH_HP_BONUS);
        case Skills.IDs.Z_TRACKER:
          return string.Format("+{0}% smell", (object) (int) (100.0 * (double) Rules.SKILL_ZTRACKER_SMELL_BONUS));
        default:
          throw new ArgumentOutOfRangeException("unhandled skill id");
      }
    }

    private string DescribeDayPhase(DayPhase phase)
    {
      switch (phase)
      {
        case DayPhase.SUNSET:
          return "Sunset";
        case DayPhase.EVENING:
          return "Evening";
        case DayPhase.MIDNIGHT:
          return "Midnight";
        case DayPhase.DEEP_NIGHT:
          return "Deep Night";
        case DayPhase.SUNRISE:
          return "Sunrise";
        case DayPhase.MORNING:
          return "Morning";
        case DayPhase.MIDDAY:
          return "Midday";
        case DayPhase.AFTERNOON:
          return "Afternoon";
        default:
          throw new ArgumentOutOfRangeException("unhandled dayphase");
      }
    }

    private string DescribeWeather(Weather weather)
    {
      switch (weather)
      {
        case Weather._FIRST:
          return "Clear";
        case Weather.CLOUDY:
          return "Cloudy";
        case Weather.RAIN:
          return "Rain";
        case Weather.HEAVY_RAIN:
          return "Heavy rain";
        default:
          throw new ArgumentOutOfRangeException("unhandled weather");
      }
    }

    private Color WeatherColor(Weather weather)
    {
      switch (weather)
      {
        case Weather._FIRST:
          return Color.Yellow;
        case Weather.CLOUDY:
          return Color.Gray;
        case Weather.RAIN:
          return Color.LightBlue;
        case Weather.HEAVY_RAIN:
          return Color.Blue;
        default:
          throw new ArgumentOutOfRangeException("unhandled weather");
      }
    }

    private int BatteriesToHours(int batteries)
    {
      return batteries / WorldTime.TURNS_PER_HOUR;
    }

    private int FoodToHoursUntilHungry(int food)
    {
      int num = food - Rules.FOOD_HUNGRY_LEVEL;
      if (num <= 0)
        return 0;
      return num / WorldTime.TURNS_PER_HOUR;
    }

    private int FoodToHoursUntilRotHungry(int food)
    {
      int num = food - Rules.ROT_HUNGRY_LEVEL;
      if (num <= 0)
        return 0;
      return num / WorldTime.TURNS_PER_HOUR;
    }

    public bool IsAlmostHungry(Actor actor)
    {
      if (!actor.Model.Abilities.HasToEat)
        return false;
      return this.FoodToHoursUntilHungry(actor.FoodPoints) <= 3;
    }

    public bool IsAlmostRotHungry(Actor actor)
    {
      if (!actor.Model.Abilities.IsRotting)
        return false;
      return this.FoodToHoursUntilRotHungry(actor.FoodPoints) <= 3;
    }

    public static Direction CommandToDirection(PlayerCommand cmd)
    {
      switch (cmd)
      {
        case PlayerCommand.MOVE_N:
          return Direction.N;
        case PlayerCommand.MOVE_NE:
          return Direction.NE;
        case PlayerCommand.MOVE_E:
          return Direction.E;
        case PlayerCommand.MOVE_SE:
          return Direction.SE;
        case PlayerCommand.MOVE_S:
          return Direction.S;
        case PlayerCommand.MOVE_SW:
          return Direction.SW;
        case PlayerCommand.MOVE_W:
          return Direction.W;
        case PlayerCommand.MOVE_NW:
          return Direction.NW;
        case PlayerCommand.WAIT_OR_SELF:
          return Direction.NEUTRAL;
        default:
          return (Direction) null;
      }
    }

    public void DoMoveActor(Actor actor, Location newLocation)
    {
      Location location = actor.Location;
      if (!this.TryActorLeaveTile(actor))
      {
        this.SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      }
      else
      {
        if (location.Map != newLocation.Map)
          throw new NotImplementedException("DoMoveActor : illegal to change map.");
        newLocation.Map.PlaceActorAt(actor, newLocation.Position);
        Corpse draggedCorpse = actor.DraggedCorpse;
        if (draggedCorpse != null)
        {
          location.Map.MoveCorpseTo(draggedCorpse, newLocation.Position);
          if (this.IsVisibleToPlayer(newLocation) || this.IsVisibleToPlayer(location))
            this.AddMessage(this.MakeMessage(actor, string.Format("{0} {1} corpse.", (object) this.Conjugate(actor, this.VERB_DRAG), (object) draggedCorpse.DeadGuy.TheName)));
        }
        int actionCost = Rules.BASE_ACTION_COST;
        if (actor.IsRunning)
        {
          actionCost /= 2;
          this.SpendActorStaminaPoints(actor, Rules.STAMINA_COST_RUNNING);
        }
        bool flag = false;
        MapObject mapObjectAt = newLocation.Map.GetMapObjectAt(newLocation.Position.X, newLocation.Position.Y);
        if (mapObjectAt != null && !mapObjectAt.IsWalkable && mapObjectAt.IsJumpable)
          flag = true;
        if (flag)
        {
          this.SpendActorStaminaPoints(actor, Rules.STAMINA_COST_JUMP);
          if (this.IsVisibleToPlayer(actor))
            this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_JUMP_ON), mapObjectAt));
          if (actor.Model.Abilities.CanJumpStumble && this.m_Rules.RollChance(Rules.JUMP_STUMBLE_CHANCE))
          {
            actionCost += Rules.JUMP_STUMBLE_ACTION_COST;
            if (this.IsVisibleToPlayer(actor))
              this.AddMessage(this.MakeMessage(actor, string.Format("{0}!", (object) this.Conjugate(actor, this.VERB_STUMBLE))));
          }
        }
        if (draggedCorpse != null)
          this.SpendActorStaminaPoints(actor, Rules.STAMINA_COST_JUMP);
        this.SpendActorActionPoints(actor, actionCost);
        if (actor.ActionPoints >= Rules.BASE_ACTION_COST)
          this.DropActorScent(actor);
        if (!actor.IsPlayer && (actor.Activity == Activity.FLEEING || actor.Activity == Activity.FLEEING_FROM_EXPLOSIVE) && (!actor.Model.Abilities.IsUndead && actor.Model.Abilities.CanTalk))
        {
          this.OnLoudNoise(newLocation.Map, newLocation.Position, "A loud SCREAM");
          if (this.m_Rules.RollChance(PLAYER_HEAR_SCREAMS_CHANCE) && !this.IsVisibleToPlayer(actor))
            this.AddMessageIfAudibleForPlayer(actor.Location, this.MakePlayerCentricMessage("You hear screams of terror", actor.Location.Position));
        }
        this.OnActorEnterTile(actor);
      }
    }

    public void DoMoveActor(Actor actor, Direction direction)
    {
      this.DoMoveActor(actor, actor.Location + direction);
    }

    public void OnActorEnterTile(Actor actor)
    {
      Map map = actor.Location.Map;
      Point position = actor.Location.Position;
      if (this.m_Rules.IsTrapCoveringMapObjectThere(map, position))
        return;
      Inventory itemsAt = map.GetItemsAt(position);
      if (itemsAt == null)
        return;
      List<Item> objList = (List<Item>) null;
      foreach (Item obj in itemsAt.Items)
      {
        ItemTrap trap = obj as ItemTrap;
        if (trap != null && trap.IsActivated && this.TryTriggerTrap(trap, actor))
        {
          if (objList == null)
            objList = new List<Item>(itemsAt.CountItems);
          objList.Add(obj);
        }
      }
      if (objList != null)
      {
        foreach (Item it in objList)
          map.RemoveItemAt(it, position);
      }
      if (actor.HitPoints > 0)
        return;
      this.KillActor((Actor) null, actor, "trap");
    }

    private bool TryActorLeaveTile(Actor actor)
    {
      Map map = actor.Location.Map;
      Point position = actor.Location.Position;
      bool canLeave = true;
      if (!this.m_Rules.IsTrapCoveringMapObjectThere(map, position))
      {
        Inventory itemsAt = map.GetItemsAt(position);
        if (itemsAt != null)
        {
          List<Item> objList = (List<Item>) null;
          bool flag = false;
          foreach (Item obj in itemsAt.Items)
          {
            ItemTrap trap = obj as ItemTrap;
            if (trap != null && trap.IsTriggered)
            {
              flag = true;
              bool isDestroyed = false;
              if (!this.TryEscapeTrap(trap, actor, out isDestroyed))
                canLeave = false;
              else if (isDestroyed)
              {
                if (objList == null)
                  objList = new List<Item>(itemsAt.CountItems);
                objList.Add(obj);
              }
            }
          }
          if (objList != null)
          {
            foreach (Item it in objList)
              map.RemoveItemAt(it, position);
          }
          if (canLeave && flag)
            this.UntriggerAllTrapsHere(actor.Location);
        }
      }
      bool visible = this.IsVisibleToPlayer(actor);
      map.ForEachAdjacentInMap(position, (Action<Point>) (adj =>
      {
        Actor actorAt = map.GetActorAt(adj);
        if (actorAt == null || !actorAt.Model.Abilities.IsUndead || (!this.m_Rules.IsEnemyOf(actorAt, actor) || this.m_Rules.ZGrabChance(actorAt, actor) == 0) || !this.m_Rules.RollChance(this.m_Rules.ZGrabChance(actorAt, actor)))
          return;
        if (visible)
          this.AddMessage(this.MakeMessage(actorAt, this.Conjugate(actorAt, this.VERB_GRAB), actor));
        canLeave = false;
      }));
      return canLeave;
    }

    private bool TryTriggerTrap(ItemTrap trap, Actor victim)
    {
      if (this.m_Rules.CheckTrapTriggers(trap, victim))
        this.DoTriggerTrap(trap, victim.Location.Map, victim.Location.Position, victim, (MapObject) null);
      else if (this.IsVisibleToPlayer(victim))
        this.AddMessage(this.MakeMessage(victim, string.Format("safely {0} {1}.", (object) this.Conjugate(victim, this.VERB_AVOID), (object) trap.TheName)));
      return trap.Quantity == 0;
    }

    private bool TryEscapeTrap(ItemTrap trap, Actor victim, out bool isDestroyed)
    {
      isDestroyed = false;
      if (trap.TrapModel.BlockChance <= 0)
        return true;
      bool player = this.IsVisibleToPlayer(victim);
      bool flag = false;
      if (this.m_Rules.CheckTrapEscape(trap, victim))
      {
        trap.IsTriggered = false;
        flag = true;
        if (player)
          this.AddMessage(this.MakeMessage(victim, string.Format("{0} {1}.", (object) this.Conjugate(victim, this.VERB_ESCAPE), (object) trap.TheName)));
        if (this.m_Rules.CheckTrapEscapeBreaks(trap, victim))
        {
          if (player)
            this.AddMessage(this.MakeMessage(victim, string.Format("{0} {1}.", (object) this.Conjugate(victim, this.VERB_BREAK), (object) trap.TheName)));
          --trap.Quantity;
          isDestroyed = trap.Quantity <= 0;
        }
      }
      else if (player)
        this.AddMessage(this.MakeMessage(victim, string.Format("is trapped by {0}!", (object) trap.TheName)));
      return flag;
    }

    private void UntriggerAllTrapsHere(Location loc)
    {
      Inventory itemsAt = loc.Map.GetItemsAt(loc.Position);
      if (itemsAt == null)
        return;
      foreach (Item obj in itemsAt.Items)
      {
        ItemTrap itemTrap = obj as ItemTrap;
        if (itemTrap != null && itemTrap.IsTriggered)
          itemTrap.IsTriggered = false;
      }
    }

    private void CheckMapObjectTriggersTraps(Map map, Point pos)
    {
      if (!this.m_Rules.IsTrapTriggeringMapObjectThere(map, pos))
        return;
      MapObject mapObjectAt = map.GetMapObjectAt(pos);
      Inventory itemsAt = map.GetItemsAt(pos);
      if (itemsAt == null)
        return;
      List<Item> objList = (List<Item>) null;
      foreach (Item obj in itemsAt.Items)
      {
        ItemTrap trap = obj as ItemTrap;
        if (trap != null && trap.IsActivated)
        {
          this.DoTriggerTrap(trap, map, pos, (Actor) null, mapObjectAt);
          if (trap.Quantity <= 0)
          {
            if (objList == null)
              objList = new List<Item>(itemsAt.CountItems);
            objList.Add(obj);
          }
        }
      }
      if (objList == null)
        return;
      foreach (Item it in objList)
        map.RemoveItemAt(it, pos);
    }

    private void DoTriggerTrap(ItemTrap trap, Map map, Point pos, Actor victim, MapObject mobj)
    {
      ItemTrapModel trapModel = trap.TrapModel;
      bool player = this.IsVisibleToPlayer(map, pos);
      trap.IsTriggered = true;
      int dmg = trapModel.Damage * trap.Quantity;
      if (dmg > 0 && victim != null)
      {
        this.InflictDamage(victim, dmg);
        if (player)
        {
          this.AddMessage(this.MakeMessage(victim, string.Format("is hurt by {0} for {1} damage!", (object) trap.AName, (object) dmg)));
          this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(victim.Location.Position), "Icons\\melee_damage"));
          this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayText(this.MapToScreen(victim.Location.Position).Add(10, 10), Color.White, dmg.ToString(), new Color?(Color.Black)));
          this.RedrawPlayScreen();
          AnimDelay(victim.IsPlayer ? DELAY_NORMAL : DELAY_SHORT);
          this.ClearOverlays();
          this.RedrawPlayScreen();
        }
      }
      if (trapModel.IsNoisy)
      {
        if (player)
        {
          if (victim != null)
            this.AddMessage(this.MakeMessage(victim, string.Format("stepping on {0} makes a bunch of noise!", (object) trap.AName)));
          else if (mobj != null)
            this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0} makes a lot of noise!", (object) this.Capitalize(trap.TheName)), map.LocalTime.TurnCounter));
        }
        this.OnLoudNoise(map, pos, trapModel.NoiseName);
      }
      if (trapModel.IsOneTimeUse)
        trap.IsActivated = false;
      if (!this.m_Rules.CheckTrapStepOnBreaks(trap, mobj))
        return;
      if (player)
      {
        if (victim != null)
          this.AddMessage(this.MakeMessage(victim, string.Format("{0} {1}.", (object) this.Conjugate(victim, this.VERB_CRUSH), (object) trap.TheName)));
        else if (mobj != null)
          this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0} breaks the {1}.", (object) this.Capitalize(mobj.TheName), (object) trap.TheName), map.LocalTime.TurnCounter));
      }
      --trap.Quantity;
    }

    public bool DoLeaveMap(Actor actor, Point exitPoint, bool askForConfirmation)
    {
      bool isPlayer = actor.IsPlayer;
      Map map = actor.Location.Map;
      Point position = actor.Location.Position;
      Exit exitAt = map.GetExitAt(exitPoint);
      if (exitAt == null)
      {
        if (isPlayer)
          this.AddMessage(this.MakeErrorMessage("There is nowhere to go there."));
        return true;
      }
      if (isPlayer && askForConfirmation)
      {
        this.ClearMessages();
        this.AddMessage(this.MakeYesNoMessage(string.Format("REALLY LEAVE {0}", (object) map.Name)));
        this.RedrawPlayScreen();
        if (!this.WaitYesOrNo())
        {
          this.AddMessage(new djack.RogueSurvivor.Data.Message("Let's stay here a bit longer...", this.m_Session.WorldTime.TurnCounter, Color.Yellow));
          this.RedrawPlayScreen();
          return false;
        }
      }
      if (!this.TryActorLeaveTile(actor))
      {
        this.SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
        return false;
      }
      if (!actor.IsPlayer)
        this.SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      if (isPlayer && exitAt.ToMap.District != map.District)
        this.BeforePlayerEnterDistrict(exitAt.ToMap.District);
      Actor actorAt = exitAt.ToMap.GetActorAt(exitAt.ToPosition);
      if (actorAt != null)
      {
        if (isPlayer)
          this.AddMessage(this.MakeErrorMessage(string.Format("{0} is blocking your way.", (object) actorAt.Name)));
        return true;
      }
      MapObject mapObjectAt = exitAt.ToMap.GetMapObjectAt(exitAt.ToPosition);
      if (mapObjectAt != null && ((!mapObjectAt.IsJumpable || !this.m_Rules.HasActorJumpAbility(actor)) && !mapObjectAt.IsCouch))
      {
        if (isPlayer)
          this.AddMessage(this.MakeErrorMessage(string.Format("{0} is blocking your way.", (object) mapObjectAt.AName)));
        return true;
      }
      if (this.IsVisibleToPlayer(actor))
        this.AddMessage(this.MakeMessage(actor, string.Format("{0} {1}.", (object) this.Conjugate(actor, this.VERB_LEAVE), (object) map.Name)));
      map.RemoveActor(actor);
      if (actor.DraggedCorpse != null)
        map.RemoveCorpse(actor.DraggedCorpse);
      if (isPlayer && exitAt.ToMap.District != map.District)
        this.OnPlayerLeaveDistrict();
      exitAt.ToMap.PlaceActorAt(actor, exitAt.ToPosition);
      exitAt.ToMap.MoveActorToFirstPosition(actor);
      if (actor.DraggedCorpse != null)
        exitAt.ToMap.AddCorpseAt(actor.DraggedCorpse, exitAt.ToPosition);
      if (this.IsVisibleToPlayer(actor) || isPlayer)
        this.AddMessage(this.MakeMessage(actor, string.Format("{0} {1}.", (object) this.Conjugate(actor, this.VERB_ENTER), (object) exitAt.ToMap.Name)));
      if (isPlayer)
      {
        if (map.District != exitAt.ToMap.District)
          this.m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, string.Format("Entered district {0}.", (object) exitAt.ToMap.District.Name));
        this.SetCurrentMap(exitAt.ToMap);
      }
      this.OnActorEnterTile(actor);
      if (actor.CountFollowers > 0)
        this.DoFollowersEnterMap(actor, map, position, exitAt.ToMap, exitAt.ToPosition);
      return true;
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void DoFollowersEnterMap(Actor leader, Map fromMap, Point fromPos, Map toMap, Point toPos)
    {
      bool flag1 = toMap.District != fromMap.District;
      bool flag2 = this.m_Player == leader;
      List<Actor> actorList = (List<Actor>) null;
      using (IEnumerator<Actor> enumerator = leader.Followers.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          Actor fo = enumerator.Current;
          bool flag3 = false;
          List<Point> pointList = (List<Point>) null;
          if (this.m_Rules.IsAdjacent(fromPos, fo.Location.Position))
          {
            pointList = toMap.FilterAdjacentInMap(toPos, (Predicate<Point>) (pt => this.m_Rules.IsWalkableFor(fo, toMap, pt.X, pt.Y)));
            flag3 = pointList != null && pointList.Count != 0;
          }
          if (!flag3)
          {
            if (actorList == null)
              actorList = new List<Actor>(3);
            actorList.Add(fo);
          }
          else if (this.TryActorLeaveTile(fo))
          {
            Point position = pointList[this.m_Rules.Roll(0, pointList.Count)];
            fromMap.RemoveActor(fo);
            toMap.PlaceActorAt(fo, position);
            toMap.MoveActorToFirstPosition(fo);
            this.OnActorEnterTile(fo);
          }
        }
      }
      if (actorList == null)
        return;
      foreach (Actor other in actorList)
      {
        if (flag1)
        {
          leader.RemoveFollower(other);
          if (flag2)
          {
            this.m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, string.Format("{0} was left behind.", (object) other.TheName));
            this.ClearMessages();
            this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0} could not follow you out of the district and left you!", (object) other.TheName), this.m_Session.WorldTime.TurnCounter, Color.Red));
            this.AddMessagePressEnter();
            this.ClearMessages();
          }
        }
        else if (other.Location.Map == fromMap && flag2)
        {
          this.ClearMessages();
          this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0} could not follow and is still in {1}.", (object) other.TheName, (object) fromMap.Name), this.m_Session.WorldTime.TurnCounter, Color.Yellow));
          this.AddMessagePressEnter();
          this.ClearMessages();
        }
      }
    }

    public bool DoUseExit(Actor actor, Point exitPoint)
    {
      return this.DoLeaveMap(actor, exitPoint, false);
    }

    public void DoSwitchPlace(Actor actor, Actor other)
    {
      this.SpendActorActionPoints(actor, 200);
      Map map = other.Location.Map;
      Point position = actor.Location.Position;
      map.RemoveActor(other);
      map.PlaceActorAt(actor, other.Location.Position);
      map.PlaceActorAt(other, position);
      if (!this.IsVisibleToPlayer(actor) && !this.IsVisibleToPlayer(other))
        return;
      this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_SWITCH_PLACE_WITH), other));
    }

    public void DoTakeLead(Actor actor, Actor other)
    {
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      actor.AddFollower(other);
      int trustIn = other.GetTrustIn(actor);
      other.TrustInLeader = trustIn;
      if (!this.IsVisibleToPlayer(actor) && !this.IsVisibleToPlayer(other))
        return;
      if (actor == this.m_Player)
        this.ClearMessages();
      this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_PERSUADE), other, " to join."));
      if (trustIn == 0)
        return;
      this.DoSay(other, actor, "Ah yes I remember you.", RogueGame.Sayflags.IS_FREE_ACTION);
    }

    public void DoCancelLead(Actor actor, Actor follower)
    {
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      actor.RemoveFollower(follower);
      follower.SetTrustIn(actor, follower.TrustInLeader);
      follower.TrustInLeader = 0;
      if (!this.IsVisibleToPlayer(actor) && !this.IsVisibleToPlayer(follower))
        return;
      if (actor == this.m_Player)
        this.ClearMessages();
      this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_PERSUADE), follower, " to leave."));
    }

    public void DoWait(Actor actor)
    {
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      if (this.IsVisibleToPlayer(actor))
      {
        if (actor.StaminaPoints < this.m_Rules.ActorMaxSTA(actor))
          this.AddMessage(this.MakeMessage(actor, string.Format("{0} {1} breath.", (object) this.Conjugate(actor, this.VERB_CATCH), (object) this.HisOrHer(actor))));
        else
          this.AddMessage(this.MakeMessage(actor, string.Format("{0}.", (object) this.Conjugate(actor, this.VERB_WAIT))));
      }
      this.RegenActorStaminaPoints(actor, 2);
    }

    public bool DoPlayerBump(Actor player, Direction direction)
    {
      ActionBump actionBump = new ActionBump(player, this, direction);
      if (actionBump == null)
        return false;
      if (actionBump.IsLegal())
      {
        actionBump.Perform();
        return true;
      }
      DoorWindow doorWindow = player.Location.Map.GetMapObjectAt(player.Location.Position + direction) as DoorWindow;
      if (doorWindow != null && doorWindow.IsBarricaded && !player.Model.Abilities.IsUndead)
      {
        if (!this.m_Rules.IsActorTired(player))
        {
          this.AddMessage(this.MakeYesNoMessage("Really tear down the barricade"));
          this.RedrawPlayScreen();
          if (this.WaitYesOrNo())
          {
            this.DoBreak(player, (MapObject) doorWindow);
            return true;
          }
          this.AddMessage(new djack.RogueSurvivor.Data.Message("Good, keep everything secure.", this.m_Session.WorldTime.TurnCounter, Color.Yellow));
          return false;
        }
        this.AddMessage(this.MakeErrorMessage("Too tired to tear down the barricade."));
        this.RedrawPlayScreen();
        return false;
      }
      this.AddMessage(this.MakeErrorMessage(string.Format("Cannot do that : {0}.", (object) actionBump.FailReason)));
      return false;
    }

    public void DoMakeAggression(Actor aggressor, Actor target)
    {
      if (aggressor.Faction.IsEnemyOf(target.Faction))
        return;
      bool wasAlreadyEnemy = aggressor.IsAggressorOf(target) || target.IsAggressorOf(aggressor);
      if (!target.IsPlayer && !target.IsSleeping && (!aggressor.IsAggressorOf(target) && !target.IsAggressorOf(aggressor)))
        this.DoSay(target, aggressor, "BASTARD! TRAITOR!", RogueGame.Sayflags.IS_FREE_ACTION);
      aggressor.MarkAsAgressorOf(target);
      target.MarkAsSelfDefenceFrom(aggressor);
      if (target.IsSleeping)
        return;
      Faction faction = target.Faction;
      if (faction == this.GameFactions.ThePolice)
      {
        if (aggressor.Model.Abilities.IsLawEnforcer && !this.m_Rules.IsMurder(aggressor, target))
          return;
        this.OnMakeEnemyOfCop(aggressor, target, wasAlreadyEnemy);
      }
      else
      {
        if (faction != this.GameFactions.TheArmy)
          return;
        this.OnMakeEnemyOfSoldier(aggressor, target, wasAlreadyEnemy);
      }
    }

    private void OnMakeEnemyOfCop(Actor aggressor, Actor cop, bool wasAlreadyEnemy)
    {
      if (!wasAlreadyEnemy)
        this.DoSay(cop, aggressor, string.Format("TO DISTRICT PATROLS : {0} MUST DIE!", (object) aggressor.TheName), RogueGame.Sayflags.IS_FREE_ACTION);
      this.MakeEnemyOfTargetFactionInDistrict(aggressor, cop, (Action<Actor>) (a =>
      {
        if (!a.IsPlayer || a == cop || (a.IsSleeping || this.m_Rules.IsEnemyOf(a, aggressor)))
          return;
        int turnCounter = this.m_Session.WorldTime.TurnCounter;
        this.ClearMessages();
        this.AddMessage(new djack.RogueSurvivor.Data.Message("You get a message from your police radio.", turnCounter, Color.White));
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0} is armed and dangerous. Shoot on sight!", (object) aggressor.TheName), turnCounter, Color.White));
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Current location : {0}@{1},{2}", (object) aggressor.Location.Map.Name, (object) aggressor.Location.Position.X, (object) aggressor.Location.Position.Y), turnCounter, Color.White));
        this.AddMessagePressEnter();
      }));
    }

    private void OnMakeEnemyOfSoldier(Actor aggressor, Actor soldier, bool wasAlreadyEnemy)
    {
      if (!wasAlreadyEnemy)
        this.DoSay(soldier, aggressor, string.Format("TO DISTRICT SQUADS : {0} MUST DIE!", (object) aggressor.TheName), RogueGame.Sayflags.IS_FREE_ACTION);
      this.MakeEnemyOfTargetFactionInDistrict(aggressor, soldier, (Action<Actor>) (a =>
      {
        if (!a.IsPlayer || a == soldier || (a.IsSleeping || this.m_Rules.IsEnemyOf(a, aggressor)))
          return;
        int turnCounter = this.m_Session.WorldTime.TurnCounter;
        this.ClearMessages();
        this.AddMessage(new djack.RogueSurvivor.Data.Message("You get a message from your army radio.", turnCounter, Color.White));
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0} is armed and dangerous. Shoot on sight!", (object) aggressor.Name), turnCounter, Color.White));
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Current location : {0}@{1},{2}", (object) aggressor.Location.Map.Name, (object) aggressor.Location.Position.X, (object) aggressor.Location.Position.Y), turnCounter, Color.White));
        this.AddMessagePressEnter();
      }));
    }

    private void MakeEnemyOfTargetFactionInDistrict(Actor aggressor, Actor target, Action<Actor> fn)
    {
      Faction faction = target.Faction;
      foreach (Map map in target.Location.Map.District.Maps)
      {
        foreach (Actor actor in map.Actors)
        {
          if (actor != aggressor && actor != target && (actor.Faction == faction && actor.Leader != aggressor))
          {
            if (fn != null)
              fn(actor);
            aggressor.MarkAsAgressorOf(actor);
            actor.MarkAsSelfDefenceFrom(aggressor);
          }
        }
      }
    }

    public void DoMeleeAttack(Actor attacker, Actor defender)
    {
      attacker.Activity = Activity.FIGHTING;
      attacker.TargetActor = defender;
      if (!this.m_Rules.IsEnemyOf(attacker, defender))
        this.DoMakeAggression(attacker, defender);
      Attack attack = this.m_Rules.ActorMeleeAttack(attacker, attacker.CurrentMeleeAttack, defender);
      Defence defence = this.m_Rules.ActorDefence(defender, defender.CurrentDefence);
      SpendActorActionPoints(attacker, Rules.BASE_ACTION_COST);
      this.SpendActorStaminaPoints(attacker, 8 + attack.StaminaPenalty);
      int num1 = this.m_Rules.RollSkill(attack.HitValue);
      int num2 = this.m_Rules.RollSkill(defence.Value);
      this.OnLoudNoise(attacker.Location.Map, attacker.Location.Position, "Nearby fighting");
      if (this.m_IsPlayerLongWait && defender.IsPlayer)
        this.m_IsPlayerLongWaitForcedStop = true;
      bool player1 = this.IsVisibleToPlayer(defender);
      bool player2 = this.IsVisibleToPlayer(attacker);
      bool flag = attacker.IsPlayer || defender.IsPlayer;
      if (!player1 && !player2 && (!flag && this.m_Rules.RollChance(25)))
        this.AddMessageIfAudibleForPlayer(attacker.Location, this.MakePlayerCentricMessage("You hear fighting", attacker.Location.Position));
      if (player2)
      {
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Yellow, new Rectangle(this.MapToScreen(attacker.Location.Position), new Size(32, 32))));
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Red, new Rectangle(this.MapToScreen(defender.Location.Position), new Size(32, 32))));
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(attacker.Location.Position), "Icons\\melee_attack"));
      }
      if (num1 > num2)
      {
        int num3 = this.m_Rules.RollDamage(defender.IsSleeping ? attack.DamageValue * 2 : attack.DamageValue) - defence.Protection_Hit;
        if (num3 > 0)
        {
          this.InflictDamage(defender, num3);
          if (attacker.Model.Abilities.CanZombifyKilled && !defender.Model.Abilities.IsUndead)
          {
            this.RegenActorHitPoints(attacker, this.Rules.ActorBiteHpRegen(attacker, num3));
            attacker.FoodPoints = Math.Min(attacker.FoodPoints + this.m_Rules.ActorBiteNutritionValue(attacker, num3), this.m_Rules.ActorMaxRot(attacker));
            if (player2)
              this.AddMessage(this.MakeMessage(attacker, this.Conjugate(attacker, this.VERB_FEAST_ON), defender, " flesh !"));
            this.InfectActor(defender, Rules.InfectionForDamage(attacker, num3));
          }
          if (defender.HitPoints <= 0)
          {
            if (player2 || player1)
            {
              this.AddMessage(this.MakeMessage(attacker, this.Conjugate(attacker, defender.Model.Abilities.IsUndead ? this.VERB_DESTROY : (this.m_Rules.IsMurder(attacker, defender) ? this.VERB_MURDER : this.VERB_KILL)), defender, " !"));
              this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(defender.Location.Position), "Icons\\killed"));
              this.RedrawPlayScreen();
              AnimDelay(DELAY_LONG);
            }
            this.KillActor(attacker, defender, "hit");
            if (attacker.Model.Abilities.IsUndead && !defender.Model.Abilities.IsUndead)
              SeeingCauseInsanity(attacker, attacker.Location, Rules.SANITY_HIT_EATEN_ALIVE, string.Format("{0} eaten alive", (object) defender.Name));
            if (Rules.HasImmediateZombification(this.m_Session.GameMode) || defender == this.m_Player)
            {
              if (attacker.Model.Abilities.CanZombifyKilled && !defender.Model.Abilities.IsUndead && this.m_Rules.RollChance(RogueGame.s_Options.ZombificationChance))
              {
                if (defender.IsPlayer)
                  defender.Location.Map.TryRemoveCorpseOf(defender);
                this.Zombify(attacker, defender, false);
                if (player1)
                {
                  this.AddMessage(this.MakeMessage(attacker, this.Conjugate(attacker, "turn"), defender, " into a Zombie!"));
                  this.RedrawPlayScreen();
                  AnimDelay(DELAY_LONG);
                }
              }
              else if (defender == this.m_Player && !defender.Model.Abilities.IsUndead && defender.Infection > 0)
              {
                defender.Location.Map.TryRemoveCorpseOf(defender);
                this.Zombify((Actor) null, defender, false);
                this.AddMessage(this.MakeMessage(defender, this.Conjugate(defender, "turn") + " into a Zombie!"));
                this.RedrawPlayScreen();
                AnimDelay(DELAY_LONG);
              }
            }
          }
          else if (player2 || player1)
          {
            this.AddMessage(this.MakeMessage(attacker, this.Conjugate(attacker, attack.Verb), defender, string.Format(" for {0} damage.", (object) num3)));
            this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(defender.Location.Position), "Icons\\melee_damage"));
            this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayText(this.MapToScreen(defender.Location.Position).Add(10, 10), Color.White, num3.ToString(), new Color?(Color.Black)));
            this.RedrawPlayScreen();
            AnimDelay(flag ? DELAY_NORMAL : DELAY_SHORT);
          }
        }
        else if (player2 || player1)
        {
          this.AddMessage(this.MakeMessage(attacker, this.Conjugate(attacker, attack.Verb), defender, " for no effect."));
          this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(defender.Location.Position), "Icons\\melee_miss"));
          this.RedrawPlayScreen();
          AnimDelay(flag ? DELAY_NORMAL : DELAY_SHORT);
        }
      }
      else if (player2 || player1)
      {
        this.AddMessage(this.MakeMessage(attacker, this.Conjugate(attacker, this.VERB_MISS), defender));
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(defender.Location.Position), "Icons\\melee_miss"));
        this.RedrawPlayScreen();
        AnimDelay(flag ? DELAY_NORMAL : DELAY_SHORT);
      }
      ItemMeleeWeapon itemMeleeWeapon = attacker.GetEquippedWeapon() as ItemMeleeWeapon;
      if (itemMeleeWeapon != null && !(itemMeleeWeapon.Model as ItemMeleeWeaponModel).IsUnbreakable && this.m_Rules.RollChance(itemMeleeWeapon.IsFragile ? 3 : 1))
      {
        this.OnUnequipItem(attacker, (Item) itemMeleeWeapon);
        if (itemMeleeWeapon.Quantity > 1)
          --itemMeleeWeapon.Quantity;
        else
          attacker.Inventory.RemoveAllQuantity((Item) itemMeleeWeapon);
        if (player2)
        {
          this.AddMessage(this.MakeMessage(attacker, string.Format(": {0} breaks and is now useless!", (object) itemMeleeWeapon.TheName)));
          this.RedrawPlayScreen();
          AnimDelay(flag ? DELAY_NORMAL : DELAY_SHORT);
        }
      }
      this.ClearOverlays();
    }

    public void DoSingleRangedAttack(Actor attacker, Actor defender, List<Point> LoF, FireMode mode)
    {
      if (!this.m_Rules.IsEnemyOf(attacker, defender))
        this.DoMakeAggression(attacker, defender);
      switch (mode)
      {
        case FireMode.DEFAULT:
          SpendActorActionPoints(attacker, Rules.BASE_ACTION_COST);
          DoSingleRangedAttack(attacker, defender, LoF, 1f);
          break;
        case FireMode.RAPID:
          SpendActorActionPoints(attacker, Rules.BASE_ACTION_COST);
          this.DoSingleRangedAttack(attacker, defender, LoF, Rules.RAPID_FIRE_FIRST_SHOT_ACCURACY);
          ItemRangedWeapon itemRangedWeapon = attacker.GetEquippedWeapon() as ItemRangedWeapon;
          if (defender.IsDead)
          {
            --itemRangedWeapon.Ammo;
            Attack currentRangedAttack = attacker.CurrentRangedAttack;
            this.AddMessage(this.MakeMessage(attacker, string.Format("{0} at nothing.", (object) this.Conjugate(attacker, currentRangedAttack.Verb))));
            break;
          }
          if (itemRangedWeapon.Ammo <= 0)
            break;
          this.DoSingleRangedAttack(attacker, defender, LoF, Rules.RAPID_FIRE_SECOND_SHOT_ACCURACY);
          break;
        default:
          throw new ArgumentOutOfRangeException("unhandled mode");
      }
    }

    private void DoSingleRangedAttack(Actor attacker, Actor defender, List<Point> LoF, float accuracyFactor)
    {
      attacker.Activity = Activity.FIGHTING;
      attacker.TargetActor = defender;
      int distance = this.m_Rules.GridDistance(attacker.Location.Position, defender.Location.Position);
      Attack attack = this.m_Rules.ActorRangedAttack(attacker, attacker.CurrentRangedAttack, distance, defender);
      Defence defence = this.m_Rules.ActorDefence(defender, defender.CurrentDefence);
      this.SpendActorStaminaPoints(attacker, attack.StaminaPenalty);
      if (attack.Kind == AttackKind.FIREARM && (this.m_Rules.RollChance(this.m_Rules.IsWeatherRain(this.m_Session.World.Weather) ? 3 : 1) && this.IsVisibleToPlayer(attacker)))
      {
        this.AddMessage(this.MakeMessage(attacker, " : weapon jam!"));
      }
      else
      {
        ItemRangedWeapon itemRangedWeapon = attacker.GetEquippedWeapon() as ItemRangedWeapon;
        if (itemRangedWeapon == null)
          throw new InvalidOperationException("DoSingleRangedAttack but no equipped ranged weapon");
        --itemRangedWeapon.Ammo;
        if (this.DoCheckFireThrough(attacker, LoF))
          return;
        if (this.m_IsPlayerLongWait && defender.IsPlayer)
          this.m_IsPlayerLongWaitForcedStop = true;
        int num1 = (int) ((double) accuracyFactor * (double) this.m_Rules.RollSkill(attack.HitValue));
        int num2 = this.m_Rules.RollSkill(defence.Value);
        bool player1 = this.IsVisibleToPlayer(defender.Location);
        bool player2 = this.IsVisibleToPlayer(attacker.Location);
        bool flag = attacker.IsPlayer || defender.IsPlayer;
        if (!player1 && !player2 && (!flag && this.m_Rules.RollChance(25)))
          this.AddMessageIfAudibleForPlayer(attacker.Location, this.MakePlayerCentricMessage("You hear firing", attacker.Location.Position));
        if (player2)
        {
          this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Yellow, new Rectangle(this.MapToScreen(attacker.Location.Position), new Size(32, 32))));
          this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Red, new Rectangle(this.MapToScreen(defender.Location.Position), new Size(32, 32))));
          this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(attacker.Location.Position), "Icons\\ranged_attack"));
        }
        if (num1 > num2)
        {
          int dmg = this.m_Rules.RollDamage(defender.IsSleeping ? attack.DamageValue * 2 : attack.DamageValue) - defence.Protection_Shot;
          if (dmg > 0)
          {
            this.InflictDamage(defender, dmg);
            if (defender.HitPoints <= 0)
            {
              if (player1)
              {
                this.AddMessage(this.MakeMessage(attacker, this.Conjugate(attacker, defender.Model.Abilities.IsUndead ? this.VERB_DESTROY : (this.m_Rules.IsMurder(attacker, defender) ? this.VERB_MURDER : this.VERB_KILL)), defender, " !"));
                this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(defender.Location.Position), "Icons\\killed"));
                this.RedrawPlayScreen();
                AnimDelay(DELAY_LONG);
              }
              this.KillActor(attacker, defender, "shot");
            }
            else if (player1)
            {
              this.AddMessage(this.MakeMessage(attacker, this.Conjugate(attacker, attack.Verb), defender, string.Format(" for {0} damage.", (object) dmg)));
              this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(defender.Location.Position), "Icons\\ranged_damage"));
              this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayText(this.MapToScreen(defender.Location.Position).Add(10, 10), Color.White, dmg.ToString(), new Color?(Color.Black)));
              this.RedrawPlayScreen();
              AnimDelay(flag ? DELAY_NORMAL : DELAY_SHORT);
            }
          }
          else if (player1)
          {
            this.AddMessage(this.MakeMessage(attacker, this.Conjugate(attacker, attack.Verb), defender, " for no effect."));
            this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(defender.Location.Position), "Icons\\ranged_miss"));
            this.RedrawPlayScreen();
            AnimDelay(flag ? DELAY_NORMAL : DELAY_SHORT);
          }
        }
        else if (player1)
        {
          this.AddMessage(this.MakeMessage(attacker, this.Conjugate(attacker, this.VERB_MISS), defender));
          this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(defender.Location.Position), "Icons\\ranged_miss"));
          this.RedrawPlayScreen();
          AnimDelay(flag ? DELAY_NORMAL : DELAY_SHORT);
        }
        this.ClearOverlays();
      }
    }

    private bool DoCheckFireThrough(Actor attacker, List<Point> LoF)
    {
      foreach (Point point in LoF)
      {
        MapObject mapObjectAt = attacker.Location.Map.GetMapObjectAt(point);
        if (mapObjectAt != null && mapObjectAt.BreaksWhenFiredThrough && (mapObjectAt.BreakState != MapObject.Break.BROKEN && !mapObjectAt.IsWalkable))
        {
          bool player1 = this.IsVisibleToPlayer(attacker);
          bool player2 = this.IsVisibleToPlayer(mapObjectAt);
          if (player1 || player2)
          {
            if (player1)
            {
              this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Yellow, new Rectangle(this.MapToScreen(attacker.Location.Position), new Size(32, 32))));
              this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(attacker.Location.Position), "Icons\\ranged_attack"));
            }
            if (player2)
              this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Red, new Rectangle(this.MapToScreen(point), new Size(32, 32))));
            AnimDelay(attacker.IsPlayer ? DELAY_NORMAL : DELAY_SHORT);
          }
          this.DoDestroyObject(mapObjectAt);
          return true;
        }
      }
      return false;
    }

    public void DoThrowGrenadeUnprimed(Actor actor, Point targetPos)
    {
      ItemGrenade itemGrenade = actor.GetEquippedWeapon() as ItemGrenade;
      if (itemGrenade == null)
        throw new InvalidOperationException("throwing grenade but no grenade equiped ");
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      actor.Inventory.Consume((Item) itemGrenade);
      actor.Location.Map.DropItemAt((Item) new ItemGrenadePrimed(this.m_GameItems[itemGrenade.PrimedModelID]), targetPos);
      if (!this.IsVisibleToPlayer(actor) && !this.IsVisibleToPlayer(actor.Location.Map, targetPos))
        return;
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Yellow, new Rectangle(this.MapToScreen(actor.Location.Position), new Size(32, 32))));
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Red, new Rectangle(this.MapToScreen(targetPos), new Size(32, 32))));
      this.AddMessage(this.MakeMessage(actor, string.Format("{0} a {1}!", (object) this.Conjugate(actor, this.VERB_THROW), (object) itemGrenade.Model.SingleName)));
      this.RedrawPlayScreen();
      AnimDelay(DELAY_LONG);
      this.ClearOverlays();
      this.RedrawPlayScreen();
    }

    public void DoThrowGrenadePrimed(Actor actor, Point targetPos)
    {
      ItemGrenadePrimed itemGrenadePrimed = actor.GetEquippedWeapon() as ItemGrenadePrimed;
      if (itemGrenadePrimed == null)
        throw new InvalidOperationException("throwing primed grenade but no primed grenade equiped ");
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      actor.Inventory.RemoveAllQuantity((Item) itemGrenadePrimed);
      actor.Location.Map.DropItemAt((Item) itemGrenadePrimed, targetPos);
      if (!this.IsVisibleToPlayer(actor) && !this.IsVisibleToPlayer(actor.Location.Map, targetPos))
        return;
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Yellow, new Rectangle(this.MapToScreen(actor.Location.Position), new Size(32, 32))));
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Red, new Rectangle(this.MapToScreen(targetPos), new Size(32, 32))));
      this.AddMessage(this.MakeMessage(actor, string.Format("{0} back a {1}!", (object) this.Conjugate(actor, this.VERB_THROW), (object) itemGrenadePrimed.Model.SingleName)));
      this.RedrawPlayScreen();
      AnimDelay(DELAY_LONG);
      this.ClearOverlays();
      this.RedrawPlayScreen();
    }

    private void ShowBlastImage(Point screenPos, BlastAttack attack, int damage)
    {
      float alpha = (float) (0.1 + (double) damage / (double) attack.Damage[0]);
      if ((double) alpha > 1.0)
        alpha = 1f;
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayTransparentImage(alpha, screenPos, "Icons\\blast"));
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayText(screenPos, Color.Red, damage.ToString(), new Color?(Color.Black)));
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void DoBlast(Location location, BlastAttack blastAttack)
    {
      this.OnLoudNoise(location.Map, location.Position, "A loud EXPLOSION");
      if (this.IsVisibleToPlayer(location))
      {
        this.ShowBlastImage(this.MapToScreen(location.Position), blastAttack, blastAttack.Damage[0]);
        this.RedrawPlayScreen();
        AnimDelay(DELAY_LONG);
        this.RedrawPlayScreen();
      }
      else if (this.m_Rules.RollChance(PLAYER_HEAR_EXPLOSION_CHANCE))
        this.AddMessageIfAudibleForPlayer(location, this.MakePlayerCentricMessage("You hear an explosion", location.Position));
      this.ApplyExplosionDamage(location, 0, blastAttack);
      for (int waveDistance = 1; waveDistance <= blastAttack.Radius; ++waveDistance)
      {
        if (this.ApplyExplosionWave(location, waveDistance, blastAttack))
        {
          this.RedrawPlayScreen();
          this.AnimDelay(DELAY_NORMAL);
        }
      }
      this.ClearOverlays();
    }

    private bool ApplyExplosionWave(Location center, int waveDistance, BlastAttack blast)
    {
      bool flag = false;
      Map map = center.Map;
      Point pt = new Point();
      int num1 = center.Position.X - waveDistance;
      int num2 = center.Position.X + waveDistance;
      int num3 = center.Position.Y - waveDistance;
      int num4 = center.Position.Y + waveDistance;
      if (num3 >= 0)
      {
        pt.Y = num3;
        for (int index = num1; index <= num2; ++index)
        {
          pt.X = index;
          flag |= this.ApplyExplosionWaveSub(center, pt, waveDistance, blast);
        }
      }
      if (num4 < map.Height)
      {
        pt.Y = num4;
        for (int index = num1; index <= num2; ++index)
        {
          pt.X = index;
          flag |= this.ApplyExplosionWaveSub(center, pt, waveDistance, blast);
        }
      }
      if (num1 >= 0)
      {
        pt.X = num1;
        for (int index = num3 + 1; index < num4; ++index)
        {
          pt.Y = index;
          flag |= this.ApplyExplosionWaveSub(center, pt, waveDistance, blast);
        }
      }
      if (num2 < map.Width)
      {
        pt.X = num2;
        for (int index = num3 + 1; index < num4; ++index)
        {
          pt.Y = index;
          flag |= this.ApplyExplosionWaveSub(center, pt, waveDistance, blast);
        }
      }
      return flag;
    }

    private bool ApplyExplosionWaveSub(Location blastCenter, Point pt, int waveDistance, BlastAttack blast)
    {
      if (!blastCenter.Map.IsInBounds(pt) || !LOS.CanTraceFireLine(blastCenter, pt, waveDistance, (List<Point>) null))
        return false;
      int damage = this.ApplyExplosionDamage(new Location(blastCenter.Map, pt), waveDistance, blast);
      if (!this.IsVisibleToPlayer(blastCenter.Map, pt))
        return false;
      this.ShowBlastImage(this.MapToScreen(pt), blast, damage);
      return true;
    }

    private int ApplyExplosionDamage(Location location, int distanceFromBlast, BlastAttack blast)
    {
      Map map = location.Map;
      int num1 = this.m_Rules.BlastDamage(distanceFromBlast, blast);
      if (num1 <= 0)
        return 0;
      Actor actorAt = map.GetActorAt(location.Position);
      if (actorAt != null)
      {
        this.ExplosionChainReaction(actorAt.Inventory, location);
        int dmg = num1 - (actorAt.CurrentDefence.Protection_Hit + actorAt.CurrentDefence.Protection_Shot) / 2;
        if (dmg > 0)
        {
          this.InflictDamage(actorAt, dmg);
          if (this.IsVisibleToPlayer(actorAt))
            this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0} is hit for {1} damage!", (object) actorAt.Name, (object) dmg), map.LocalTime.TurnCounter, Color.Crimson));
          if (actorAt.HitPoints <= 0 && !actorAt.IsDead)
          {
            this.KillActor((Actor) null, actorAt, string.Format("explosion {0} damage", (object) dmg));
            if (this.IsVisibleToPlayer(actorAt))
              this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0} dies in the explosion!", (object) actorAt.Name), map.LocalTime.TurnCounter, Color.Crimson));
          }
        }
        else
          this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0} is hit for no damage.", (object) actorAt.Name), map.LocalTime.TurnCounter, Color.White));
      }
      Inventory itemsAt = map.GetItemsAt(location.Position);
      if (itemsAt != null)
      {
        this.ExplosionChainReaction(itemsAt, location);
        int chance = num1;
        List<Item> objList = new List<Item>(itemsAt.CountItems);
        foreach (Item obj in itemsAt.Items)
        {
          if (!obj.IsUnique && !obj.Model.IsUnbreakable && (!(obj is ItemPrimedExplosive) || (obj as ItemPrimedExplosive).FuseTimeLeft > 0) && this.m_Rules.RollChance(chance))
            objList.Add(obj);
        }
        foreach (Item it in objList)
          map.RemoveItemAt(it, location.Position);
      }
      if (blast.CanDamageObjects)
      {
        MapObject mapObjectAt = map.GetMapObjectAt(location.Position);
        if (mapObjectAt != null)
        {
          DoorWindow doorWindow = mapObjectAt as DoorWindow;
          if (mapObjectAt.IsBreakable || doorWindow != null && doorWindow.IsBarricaded)
          {
            int val2 = num1;
            if (doorWindow != null && doorWindow.IsBarricaded)
            {
              int num2 = Math.Min(doorWindow.BarricadePoints, val2);
              doorWindow.BarricadePoints -= num2;
              val2 -= num2;
            }
            if (val2 >= 0)
            {
              mapObjectAt.HitPoints -= val2;
              if (mapObjectAt.HitPoints <= 0)
                this.DoDestroyObject(mapObjectAt);
            }
          }
        }
      }
      List<Corpse> corpsesAt = map.GetCorpsesAt(location.Position);
      if (corpsesAt != null)
      {
        foreach (Corpse c in corpsesAt)
          this.InflictDamageToCorpse(c, (float) num1);
      }
      if (blast.CanDestroyWalls)
        throw new NotImplementedException("blast.destroyWalls");
      return num1;
    }

    private void ExplosionChainReaction(Inventory inv, Location location)
    {
      if (inv == null || inv.IsEmpty) return;
      List<ItemExplosive> itemExplosiveList = (List<ItemExplosive>) null;
      List<ItemPrimedExplosive> itemPrimedExplosiveList = (List<ItemPrimedExplosive>) null;
      foreach (Item obj in inv.Items)
      {
        ItemExplosive itemExplosive = obj as ItemExplosive;
        if (itemExplosive != null)
        {
          ItemPrimedExplosive itemPrimedExplosive = itemExplosive as ItemPrimedExplosive;
          if (itemPrimedExplosive != null)
          {
            itemPrimedExplosive.FuseTimeLeft = 0;
          }
          else
          {
            if (itemExplosiveList == null)
              itemExplosiveList = new List<ItemExplosive>();
            if (itemPrimedExplosiveList == null)
              itemPrimedExplosiveList = new List<ItemPrimedExplosive>();
            itemExplosiveList.Add(itemExplosive);
            for (int index = 0; index < obj.Quantity; ++index)
              itemPrimedExplosiveList.Add(new ItemPrimedExplosive(this.m_GameItems[itemExplosive.PrimedModelID])
              {
                FuseTimeLeft = 0
              });
          }
        }
      }
      if (itemExplosiveList != null)
      {
        foreach (Item it in itemExplosiveList)
          inv.RemoveAllQuantity(it);
      }
      if (itemPrimedExplosiveList == null)
        return;
      foreach (Item it in itemPrimedExplosiveList)
        location.Map.DropItemAt(it, location.Position);
    }

    public void DoChat(Actor speaker, Actor target)
    {
      SpendActorActionPoints(speaker, Rules.BASE_ACTION_COST);
      if (this.IsVisibleToPlayer(speaker) || this.IsVisibleToPlayer(target))
        this.AddMessage(this.MakeMessage(speaker, this.Conjugate(speaker, this.VERB_CHAT_WITH), target));
      if (!this.m_Rules.CanActorInitiateTradeWith(speaker, target))
        return;
      this.DoTrade(speaker, target);
    }

    private bool IsInterestingTradeItem(Actor speaker, Item offeredItem, Actor target)
    {
      if (this.m_Rules.RollChance(this.m_Rules.ActorCharismaticTradeChance(speaker)))
        return true;
      return (target.Controller as BaseAI).IsInterestingItem(this, offeredItem);
    }

    private bool IsTradeableItem(Actor speaker, Item offeredItem)
    {
       return (speaker.Controller as BaseAI).IsTradeableItem(this, offeredItem);
    }

    private void DoTrade(Actor speaker, Item itSpeaker, Actor target, bool doesTargetCheckForInterestInOffer)
    {
      bool flag1 = this.IsVisibleToPlayer(speaker) || this.IsVisibleToPlayer(target);
      bool flag2 = true;
      bool flag3 = itSpeaker != null && this.IsInterestingTradeItem(speaker, itSpeaker, target);
      if (target.Leader == speaker)
        flag2 = true;
      else if (doesTargetCheckForInterestInOffer)
        flag2 = flag3;
      Item trade = this.PickItemToTrade(target, speaker);
      if (itSpeaker == null || !flag2)
      {
        if (!flag1)
          return;
        if (itSpeaker == null)
          this.AddMessage(this.MakeMessage(target, string.Format("is not interested in {0} items.", (object) speaker.Name)));
        else
          this.AddMessage(this.MakeMessage(target, string.Format("is not interested in {0}.", (object) itSpeaker.TheName)));
      }
      else if (trade == null)
      {
        if (!flag1)
          return;
        this.AddMessage(this.MakeMessage(speaker, string.Format("is not interested in {0} items.", (object) target.Name)));
      }
      else
      {
        if (target.Leader != speaker)
        {
          if (itSpeaker.Model.ID == trade.Model.ID)
          {
            if (!flag1)
              return;
            this.AddMessage(this.MakeMessage(target, "has no interesting deal to offer."));
            return;
          }
          bool flag4 = false;
          if (itSpeaker is ItemRangedWeapon && trade is ItemAmmo)
          {
            if ((itSpeaker as ItemRangedWeapon).AmmoType == (trade as ItemAmmo).AmmoType)
              flag4 = true;
          }
          else if (itSpeaker is ItemAmmo && trade is ItemRangedWeapon && (itSpeaker as ItemAmmo).AmmoType == (trade as ItemRangedWeapon).AmmoType)
            flag4 = true;
          if (flag4)
          {
            if (!flag1)
              return;
            this.AddMessage(this.MakeMessage(target, "has no interesting deal to offer."));
            return;
          }
        }
        bool isPlayer = speaker.IsPlayer;
        if (flag1)
          this.AddMessage(this.MakeMessage(target, string.Format("{0} {1} for {2}.", (object) this.Conjugate(target, this.VERB_OFFER), (object) trade.AName, (object) itSpeaker.AName)));
        bool flag5 = true;
        if (!speaker.IsPlayer)
          flag5 = !target.HasLeader || (target.Controller as AIController).Directives.CanTrade;
        if (speaker.IsPlayer)
        {
          this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.TRADE_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, Point.Empty));
          this.RedrawPlayScreen();
          flag5 = this.WaitYesOrNo();
          this.ClearOverlays();
          this.RedrawPlayScreen();
        }
        if (flag5)
        {
          if (flag1)
          {
            this.AddMessage(this.MakeMessage(speaker, string.Format("{0}.", (object) this.Conjugate(speaker, this.VERB_ACCEPT_THE_DEAL))));
            if (isPlayer)
              this.RedrawPlayScreen();
          }
          if (target.Leader == speaker && flag3)
            this.DoSay(target, speaker, "Thank you for this good deal.", RogueGame.Sayflags.IS_FREE_ACTION);
          if (itSpeaker.IsEquipped)
            this.DoUnequipItem(speaker, itSpeaker);
          if (trade.IsEquipped)
            this.DoUnequipItem(target, trade);
          speaker.Inventory.RemoveAllQuantity(itSpeaker);
          target.Inventory.RemoveAllQuantity(trade);
          speaker.Inventory.AddAll(trade);
          target.Inventory.AddAll(itSpeaker);
        }
        else
        {
          if (!flag1)
            return;
          this.AddMessage(this.MakeMessage(speaker, string.Format("{0}.", (object) this.Conjugate(speaker, this.VERB_REFUSE_THE_DEAL))));
          if (!isPlayer)
            return;
          this.RedrawPlayScreen();
        }
      }
    }

    public void DoTrade(Actor speaker, Actor target)
    {
      Item trade = this.PickItemToTrade(speaker, target);
      this.DoTrade(speaker, trade, target, false);
    }

    private Item PickItemToTrade(Actor speaker, Actor buyer)
    {
      Inventory inventory = speaker.Inventory;
      // player used to get a break
/*
      if (buyer.IsPlayer)
        return inventory[this.m_Rules.Roll(0, inventory.CountItems)];
*/
      List<Item> objList = null;
      foreach (Item offeredItem in inventory.Items)
      {
        if (   IsInterestingTradeItem(speaker, offeredItem, buyer)
            && IsTradeableItem(speaker,offeredItem))
        {
          if (objList == null)
            objList = new List<Item>(inventory.CountItems);
          objList.Add(offeredItem);
        }
      }
      if (objList == null) return null;
      return objList[this.m_Rules.Roll(0, objList.Count)];
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public void DoSay(Actor speaker, Actor target, string text, RogueGame.Sayflags flags)
    {
      if ((flags & RogueGame.Sayflags.IS_FREE_ACTION) == RogueGame.Sayflags.NONE)
        SpendActorActionPoints(speaker, Rules.BASE_ACTION_COST);
      if (!this.IsVisibleToPlayer(speaker) && (!this.IsVisibleToPlayer(target) || this.m_Player.IsSleeping && target == this.m_Player))
        return;
      bool isPlayer = target.IsPlayer;
      bool flag = (flags & RogueGame.Sayflags.IS_IMPORTANT) != RogueGame.Sayflags.NONE;
      if (isPlayer && flag)
        this.ClearMessages();
      this.AddMessage(this.MakeMessage(speaker, string.Format("to {0} : ", (object) target.TheName), this.SAYOREMOTE_COLOR));
      this.AddMessage(this.MakeMessage(speaker, string.Format("\"{0}\"", (object) text), this.SAYOREMOTE_COLOR));
      if (!isPlayer || !flag)
        return;
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Yellow, new Rectangle(this.MapToScreen(speaker.Location.Position), new Size(32, 32))));
      this.AddMessagePressEnter();
      this.ClearOverlays();
      this.RemoveLastMessage();
      this.RedrawPlayScreen();
    }

    public void DoShout(Actor speaker, string text)
    {
      SpendActorActionPoints(speaker, Rules.BASE_ACTION_COST);
      this.OnLoudNoise(speaker.Location.Map, speaker.Location.Position, "A SHOUT");
      if (!this.IsVisibleToPlayer(speaker) && !this.AreLinkedByPhone(speaker, this.m_Player))
        return;
      if (speaker.Leader == this.m_Player)
      {
        this.ClearMessages();
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Yellow, new Rectangle(this.MapToScreen(speaker.Location.Position), new Size(32, 32))));
        this.AddMessage(this.MakeMessage(speaker, string.Format("{0}!!", (object) this.Conjugate(speaker, this.VERB_RAISE_ALARM))));
        if (text != null)
          this.DoEmote(speaker, text);
        this.AddMessagePressEnter();
        this.ClearOverlays();
        this.RemoveLastMessage();
      }
      else if (text == null)
        this.AddMessage(this.MakeMessage(speaker, string.Format("{0}!", (object) this.Conjugate(speaker, this.VERB_SHOUT))));
      else
        this.DoEmote(speaker, string.Format("{0} \"{1}\"", (object) this.Conjugate(speaker, this.VERB_SHOUT), (object) text));
    }

    public void DoEmote(Actor actor, string text)
    {
      if (!this.IsVisibleToPlayer(actor))
        return;
      this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0} : {1}", (object) actor.Name, (object) text), actor.Location.Map.LocalTime.TurnCounter, this.SAYOREMOTE_COLOR));
    }

    public void DoTakeFromContainer(Actor actor, Point position)
    {
      Item topItem = actor.Location.Map.GetItemsAt(position).TopItem;
      this.DoTakeItem(actor, position, topItem);
    }

    public void DoTakeItem(Actor actor, Point position, Item it)
    {
      Map map = actor.Location.Map;
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      if (it is ItemTrap)
        (it as ItemTrap).IsActivated = false;
      int quantity = it.Quantity;
      int quantityAdded;
      actor.Inventory.AddAsMuchAsPossible(it, out quantityAdded);
      if (quantityAdded == quantity)
      {
        Inventory itemsAt = map.GetItemsAt(position);
        if (itemsAt != null && itemsAt.Contains(it))
          map.RemoveItemAt(it, position);
      }
      if (this.IsVisibleToPlayer(actor) || this.IsVisibleToPlayer(new Location(map, position)))
        this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_TAKE), it));
      if (it.Model.DontAutoEquip || !this.m_Rules.CanActorEquipItem(actor, it) || actor.GetEquippedItem(it.Model.EquipmentPart) != null)
        return;
      this.DoEquipItem(actor, it);
    }

    public void DoGiveItemTo(Actor actor, Actor target, Item gift)
    {
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      if (target.Leader == actor)
      {
        BaseAI baseAi = target.Controller as BaseAI;
        bool flag = baseAi != null && baseAi.IsInterestingItem(this, gift);
        if (flag)
          this.DoSay(target, actor, "Thank you, I really needed that!", RogueGame.Sayflags.IS_FREE_ACTION);
        else
          this.DoSay(target, actor, "Thanks I guess...", RogueGame.Sayflags.IS_FREE_ACTION);
        ModifyActorTrustInLeader(target, flag ? Rules.TRUST_GOOD_GIFT_INCREASE : Rules.TRUST_MISC_GIFT_INCREASE, true);
      }
      else if (actor.Leader == target)
      {
        DoSay(target, actor, "Well, here it is...", RogueGame.Sayflags.IS_FREE_ACTION);
        ModifyActorTrustInLeader(actor, Rules.TRUST_GIVE_ITEM_ORDER_PENALTY, true);
      }
      // XXX If the ground inventory is not full and item merging happens, gift still exists to C#
      // so the result is the follower gets the gift while the pile on the ground increases.
      DropItem(actor, gift);
      DoTakeItem(target, actor.Location.Position, gift);
      if (!this.IsVisibleToPlayer(actor) && !this.IsVisibleToPlayer(target))
        return;
      this.AddMessage(this.MakeMessage(actor, string.Format("{0} {1} to", (object) this.Conjugate(actor, this.VERB_GIVE), (object) gift.TheName), target));
    }

    public void DoEquipItem(Actor actor, Item it)
    {
      Item equippedItem = actor.GetEquippedItem(it.Model.EquipmentPart);
      if (equippedItem != null) DoUnequipItem(actor, equippedItem);
      it.EquippedPart = it.Model.EquipmentPart;
      OnEquipItem(actor, it);
#if DEBUG
      // postcondition: item is unequippable
      if (!Rules.CanActorUnequipItem(actor,it)) throw new ArgumentOutOfRangeException("equipped item cannot be unequipped","item type value: "+it.Model.ID.ToString());
#endif
      if (!this.IsVisibleToPlayer(actor)) return;
      this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_EQUIP), it));
    }

    public void DoUnequipItem(Actor actor, Item it)
    {
      it.EquippedPart = DollPart.NONE;
      this.OnUnequipItem(actor, it);
      if (!this.IsVisibleToPlayer(actor))
        return;
      this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_UNEQUIP), it));
    }

    private void OnEquipItem(Actor actor, Item it)
    {
      if (it.Model is ItemWeaponModel)
      {
        if (it.Model is ItemMeleeWeaponModel)
        {
          ItemMeleeWeaponModel meleeWeaponModel = it.Model as ItemMeleeWeaponModel;
          actor.CurrentMeleeAttack = new Attack(meleeWeaponModel.Attack.Kind, meleeWeaponModel.Attack.Verb, meleeWeaponModel.Attack.HitValue + actor.Sheet.UnarmedAttack.HitValue, meleeWeaponModel.Attack.DamageValue + actor.Sheet.UnarmedAttack.DamageValue, meleeWeaponModel.Attack.StaminaPenalty);
        }
        else
        {
          if (!(it.Model is ItemRangedWeaponModel))
            return;
          ItemRangedWeaponModel rangedWeaponModel = it.Model as ItemRangedWeaponModel;
          actor.CurrentRangedAttack = new Attack(rangedWeaponModel.Attack.Kind, rangedWeaponModel.Attack.Verb, rangedWeaponModel.Attack.HitValue, rangedWeaponModel.Attack.DamageValue, rangedWeaponModel.Attack.StaminaPenalty, rangedWeaponModel.Attack.Range);
        }
      }
      else if (it.Model is ItemBodyArmorModel)
      {
        ItemBodyArmorModel itemBodyArmorModel = it.Model as ItemBodyArmorModel;
        actor.CurrentDefence += itemBodyArmorModel.ToDefence();
      }
      else if (it.Model is ItemTrackerModel)
      {
        --(it as ItemTracker).Batteries;
      }
      else
      {
        if (!(it.Model is ItemLightModel))
          return;
        --(it as ItemLight).Batteries;
      }
    }

    private void OnUnequipItem(Actor actor, Item it)
    {
      if (it.Model is ItemWeaponModel)
      {
        if (it.Model is ItemMeleeWeaponModel)
        {
          actor.CurrentMeleeAttack = actor.Sheet.UnarmedAttack;
        }
        else
        {
          if (!(it.Model is ItemRangedWeaponModel))
            return;
          actor.CurrentRangedAttack = Attack.BLANK;
        }
      }
      else
      {
        if (!(it.Model is ItemBodyArmorModel))
          return;
        ItemBodyArmorModel itemBodyArmorModel = it.Model as ItemBodyArmorModel;
        actor.CurrentDefence -= itemBodyArmorModel.ToDefence();
      }
    }

    public void DoDropItem(Actor actor, Item it)
    {
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      Item obj = it;
      bool flag = false;
      if (it is ItemTrap)
      {
        ItemTrap itemTrap1 = it as ItemTrap;
        ItemTrap itemTrap2 = itemTrap1.Clone();
        itemTrap2.IsActivated = itemTrap1.IsActivated;
        obj = (Item) itemTrap2;
        if (itemTrap2.TrapModel.ActivatesWhenDropped)
          itemTrap2.IsActivated = true;
        itemTrap1.IsActivated = false;
      }
      else if (it is ItemTracker)
        flag = (it as ItemTracker).Batteries <= 0;
      else if (it is ItemLight)
        flag = (it as ItemLight).Batteries <= 0;
      else if (it is ItemSprayPaint)
        flag = (it as ItemSprayPaint).PaintQuantity <= 0;
      else if (it is ItemSprayScent)
        flag = (it as ItemSprayScent).SprayQuantity <= 0;
      if (flag)
      {
        this.DiscardItem(actor, it);
        if (!this.IsVisibleToPlayer(actor))
          return;
        this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_DISCARD), it));
      }
      else
      {
        if (obj == it)
          this.DropItem(actor, it);
        else
          this.DropCloneItem(actor, it, obj);
        if (!this.IsVisibleToPlayer(actor))
          return;
        this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_DROP), obj));
      }
    }

    private void DiscardItem(Actor actor, Item it)
    {
      actor.Inventory.RemoveAllQuantity(it);
      it.EquippedPart = DollPart.NONE;
    }

    private void DropItem(Actor actor, Item it)
    {
      actor.Inventory.RemoveAllQuantity(it);
      actor.Location.Map.DropItemAt(it, actor.Location.Position);
      it.EquippedPart = DollPart.NONE;
    }

    private void DropCloneItem(Actor actor, Item it, Item clone)
    {
      if (--it.Quantity <= 0)
        actor.Inventory.RemoveAllQuantity(it);
      actor.Location.Map.DropItemAt(clone, actor.Location.Position);
      clone.EquippedPart = DollPart.NONE;
    }

    public void DoUseItem(Actor actor, Item it)
    {
      if (it is ItemFood)
        this.DoUseFoodItem(actor, it as ItemFood);
      else if (it is ItemMedicine)
        this.DoUseMedicineItem(actor, it as ItemMedicine);
      else if (it is ItemAmmo)
        this.DoUseAmmoItem(actor, it as ItemAmmo);
      else if (it is ItemSprayScent)
        this.DoUseSprayScentItem(actor, it as ItemSprayScent);
      else if (it is ItemTrap)
      {
        this.DoUseTrapItem(actor, it as ItemTrap);
      }
      else
      {
        if (!(it is ItemEntertainment))
          return;
        this.DoUseEntertainmentItem(actor, it as ItemEntertainment);
      }
    }

    public void DoEatFoodFromGround(Actor actor, Item it)
    {
      ItemFood food = it as ItemFood;
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      int baseValue = this.m_Rules.FoodItemNutrition(food, actor.Location.Map.LocalTime.TurnCounter);
      actor.FoodPoints = Math.Min(actor.FoodPoints + this.m_Rules.ActorItemNutritionValue(actor, baseValue), this.m_Rules.ActorMaxFood(actor));
      actor.Location.Map.GetItemsAt(actor.Location.Position).Consume((Item) food);
      bool player = this.IsVisibleToPlayer(actor);
      if (player)
        this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_EAT), (Item) food));
      if (!this.m_Rules.IsFoodSpoiled(food, actor.Location.Map.LocalTime.TurnCounter) || !this.m_Rules.RollChance(Rules.FOOD_EXPIRED_VOMIT_CHANCE))
        return;
      this.DoVomit(actor);
      if (!player)
        return;
      this.AddMessage(this.MakeMessage(actor, string.Format("{0} from eating spoiled food!", (object) this.Conjugate(actor, this.VERB_VOMIT))));
    }

    private void DoUseFoodItem(Actor actor, ItemFood food)
    {
      if (actor == this.m_Player && actor.FoodPoints >= this.m_Rules.ActorMaxFood(actor) - 1)
      {
        this.AddMessage(this.MakeErrorMessage("Don't waste food!"));
      }
      else
      {
        SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
        int baseValue = this.m_Rules.FoodItemNutrition(food, actor.Location.Map.LocalTime.TurnCounter);
        actor.FoodPoints = Math.Min(actor.FoodPoints + this.m_Rules.ActorItemNutritionValue(actor, baseValue), this.m_Rules.ActorMaxFood(actor));
        actor.Inventory.Consume((Item) food);
        if (food.Model == this.GameItems.CANNED_FOOD)
        {
          ItemTrap itemTrap = new ItemTrap(this.GameItems.EMPTY_CAN)
          {
            IsActivated = true
          };
          actor.Location.Map.DropItemAt((Item) itemTrap, actor.Location.Position);
        }
        bool player = this.IsVisibleToPlayer(actor);
        if (player)
          this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_EAT), (Item) food));
        if (!m_Rules.IsFoodSpoiled(food, actor.Location.Map.LocalTime.TurnCounter) || !m_Rules.RollChance(Rules.FOOD_EXPIRED_VOMIT_CHANCE))
          return;
        this.DoVomit(actor);
        if (!player)
          return;
        this.AddMessage(this.MakeMessage(actor, string.Format("{0} from eating spoiled food!", (object) this.Conjugate(actor, this.VERB_VOMIT))));
      }
    }

    private void DoVomit(Actor actor)
    {
      actor.StaminaPoints -= Rules.FOOD_VOMIT_STA_COST;
      actor.SleepPoints = Math.Max(0, actor.SleepPoints - WorldTime.TURNS_PER_HOUR);
      actor.FoodPoints = Math.Max(0, actor.FoodPoints - WorldTime.TURNS_PER_HOUR);
      Location location = actor.Location;
      location.Map.GetTileAt(location.Position.X, location.Position.Y).AddDecoration("Tiles\\Decoration\\vomit");
    }

    private void DoUseMedicineItem(Actor actor, ItemMedicine med)
    {
      if (actor == this.m_Player)
      {
        int num1 = this.m_Rules.ActorMaxHPs(actor) - actor.HitPoints;
        int num2 = this.m_Rules.ActorMaxSTA(actor) - actor.StaminaPoints;
        int num3 = this.m_Rules.ActorMaxSleep(actor) - 2 - actor.SleepPoints;
        int infection = actor.Infection;
        int num4 = this.m_Rules.ActorMaxSanity(actor) - actor.Sanity;
        if ((num1 <= 0 || med.Healing <= 0) && (num2 <= 0 || med.StaminaBoost <= 0) && ((num3 <= 0 || med.SleepBoost <= 0) && (infection <= 0 || med.InfectionCure <= 0)) && (num4 <= 0 || med.SanityCure <= 0))
        {
          this.AddMessage(this.MakeErrorMessage("Don't waste medicine!"));
          return;
        }
      }
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      actor.HitPoints = Math.Min(actor.HitPoints + this.m_Rules.ActorMedicineEffect(actor, med.Healing), this.m_Rules.ActorMaxHPs(actor));
      actor.StaminaPoints = Math.Min(actor.StaminaPoints + this.m_Rules.ActorMedicineEffect(actor, med.StaminaBoost), this.m_Rules.ActorMaxSTA(actor));
      actor.SleepPoints = Math.Min(actor.SleepPoints + this.m_Rules.ActorMedicineEffect(actor, med.SleepBoost), this.m_Rules.ActorMaxSleep(actor));
      actor.Infection = Math.Max(0, actor.Infection - this.m_Rules.ActorMedicineEffect(actor, med.InfectionCure));
      actor.Sanity = Math.Min(actor.Sanity + this.m_Rules.ActorMedicineEffect(actor, med.SanityCure), this.m_Rules.ActorMaxSanity(actor));
      actor.Inventory.Consume((Item) med);
      if (!this.IsVisibleToPlayer(actor))
        return;
      this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_HEAL_WITH), (Item) med));
    }

    private void DoUseAmmoItem(Actor actor, ItemAmmo ammoItem)
    {
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      ItemRangedWeapon itemRangedWeapon = actor.GetEquippedWeapon() as ItemRangedWeapon;
      int num = Math.Min((itemRangedWeapon.Model as ItemRangedWeaponModel).MaxAmmo - itemRangedWeapon.Ammo, ammoItem.Quantity);
      itemRangedWeapon.Ammo += num;
      ammoItem.Quantity -= num;
      if (ammoItem.Quantity <= 0)
        actor.Inventory.RemoveAllQuantity((Item) ammoItem);
      if (!this.IsVisibleToPlayer(actor))
        return;
      this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_RELOAD), (Item) itemRangedWeapon));
    }

    private void DoUseSprayScentItem(Actor actor, ItemSprayScent spray)
    {
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      --spray.SprayQuantity;
      Map map = actor.Location.Map;
      ItemSprayScentModel itemSprayScentModel = spray.Model as ItemSprayScentModel;
      map.ModifyScentAt(itemSprayScentModel.Odor, itemSprayScentModel.Strength, actor.Location.Position);
      if (!this.IsVisibleToPlayer(actor))
        return;
      this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_SPRAY), (Item) spray));
    }

    private void DoUseTrapItem(Actor actor, ItemTrap trap)
    {
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      trap.IsActivated = !trap.IsActivated;
      if (!this.IsVisibleToPlayer(actor))
        return;
      this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, trap.IsActivated ? this.VERB_ACTIVATE : this.VERB_DESACTIVATE), (Item) trap));
    }

    private void DoUseEntertainmentItem(Actor actor, ItemEntertainment ent)
    {
      bool player = this.IsVisibleToPlayer(actor);
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      this.RegenActorSanity(actor, this.Rules.ActorSanRegenValue(actor, ent.EntertainmentModel.Value));
      if (player)
        this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_ENJOY), (Item) ent));
      int boreChance = ent.EntertainmentModel.BoreChance;
      bool flag1 = false;
      bool flag2 = false;
      if (boreChance == 100)
      {
        actor.Inventory.Consume((Item) ent);
        flag2 = true;
      }
      else if (boreChance > 0 && this.m_Rules.RollChance(boreChance))
        flag1 = true;
      if (flag1)
        actor.AddBoringItem((Item) ent);
      if (!player)
        return;
      if (flag1)
        this.AddMessage(this.MakeMessage(actor, string.Format("{0} now bored of {1}.", (object) this.Conjugate(actor, this.VERB_BE), (object) ent.TheName)));
      if (!flag2)
        return;
      this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_DISCARD), (Item) ent));
    }

    public void DoRechargeItemBattery(Actor actor, Item it)
    {
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      if (it is ItemLight) (it as ItemLight).Recharge();
      else if (it is ItemTracker) (it as ItemTracker).Recharge();
      if (!this.IsVisibleToPlayer(actor)) return;
      AddMessage(MakeMessage(actor, Conjugate(actor, VERB_RECHARGE), it, " batteries."));
    }

    public void DoOpenDoor(Actor actor, DoorWindow door)
    {
      door.SetState(2);
      if (this.IsVisibleToPlayer(actor) || this.IsVisibleToPlayer((MapObject) door))
      {
        this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_OPEN), (MapObject) door));
        this.RedrawPlayScreen();
      }
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public void DoCloseDoor(Actor actor, DoorWindow door)
    {
      door.SetState(1);
      if (this.IsVisibleToPlayer(actor) || this.IsVisibleToPlayer((MapObject) door))
      {
        this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_CLOSE), (MapObject) door));
        this.RedrawPlayScreen();
      }
      this.SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
    }

    public void DoBarricadeDoor(Actor actor, DoorWindow door)
    {
      ItemBarricadeMaterial barricadeMaterial = actor.Inventory.GetFirstByType(typeof (ItemBarricadeMaterial)) as ItemBarricadeMaterial;
      ItemBarricadeMaterialModel barricadeMaterialModel = barricadeMaterial.Model as ItemBarricadeMaterialModel;
      actor.Inventory.Consume((Item) barricadeMaterial);
      door.BarricadePoints = Math.Min(door.BarricadePoints + this.m_Rules.ActorBarricadingPoints(actor, barricadeMaterialModel.BarricadingValue), 80);
      if (this.IsVisibleToPlayer(actor) || this.IsVisibleToPlayer((MapObject) door))
        this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_BARRICADE), (MapObject) door));
      this.SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
    }

    public void DoBuildFortification(Actor actor, Point buildPos, bool isLarge)
    {
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      int num = this.m_Rules.ActorBarricadingMaterialNeedForFortification(actor, isLarge);
      for (int index = 0; index < num; ++index)
      {
        Item firstByType = actor.Inventory.GetFirstByType(typeof (ItemBarricadeMaterial));
        actor.Inventory.Consume(firstByType);
      }
      Fortification fortification = isLarge ? this.m_TownGenerator.MakeObjLargeFortification("MapObjects\\wooden_large_fortification") : this.m_TownGenerator.MakeObjSmallFortification("MapObjects\\wooden_small_fortification");
      actor.Location.Map.PlaceMapObjectAt((MapObject) fortification, buildPos);
      if (this.IsVisibleToPlayer(actor) || this.IsVisibleToPlayer(new Location(actor.Location.Map, buildPos)))
        this.AddMessage(this.MakeMessage(actor, string.Format("{0} a {1} fortification.", (object) this.Conjugate(actor, this.VERB_BUILD), isLarge ? (object) "large" : (object) "small")));
      this.CheckMapObjectTriggersTraps(actor.Location.Map, buildPos);
    }

    public void DoRepairFortification(Actor actor, Fortification fort)
    {
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      ItemBarricadeMaterial barricadeMaterial = actor.Inventory.GetFirstByType(typeof (ItemBarricadeMaterial)) as ItemBarricadeMaterial;
      if (barricadeMaterial == null)
        throw new InvalidOperationException("no material");
      actor.Inventory.Consume((Item) barricadeMaterial);
      fort.HitPoints = Math.Min(fort.MaxHitPoints, fort.HitPoints + this.m_Rules.ActorBarricadingPoints(actor, (barricadeMaterial.Model as ItemBarricadeMaterialModel).BarricadingValue));
      if (!this.IsVisibleToPlayer(actor) && !this.IsVisibleToPlayer((MapObject) fort))
        return;
      this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_REPAIR), (MapObject) fort));
    }

    public void DoSwitchPowerGenerator(Actor actor, PowerGenerator powGen)
    {
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      powGen.TogglePower();
      if (this.IsVisibleToPlayer(actor) || this.IsVisibleToPlayer((MapObject) powGen))
        this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_SWITCH), (MapObject) powGen, powGen.IsOn ? " on." : " off."));
      this.OnMapPowerGeneratorSwitch(actor.Location, powGen);
    }

    private void DoDestroyObject(MapObject mapObj)
    {
      DoorWindow doorWindow = mapObj as DoorWindow;
      bool flag = doorWindow != null && doorWindow.IsWindow;
      mapObj.HitPoints = 0;
      if (mapObj.GivesWood)
      {
        int val2 = 1 + mapObj.MaxHitPoints / 40;
        while (val2 > 0)
        {
          ItemBarricadeMaterial barricadeMaterial = new ItemBarricadeMaterial((ItemModel) this.m_GameItems.WOODENPLANK);
          barricadeMaterial.Quantity = Math.Min(this.m_GameItems.WOODENPLANK.StackingLimit, val2);
          Item it = (Item) barricadeMaterial;
          if (it.Quantity < 1)
            it.Quantity = 1;
          mapObj.Location.Map.DropItemAt(it, mapObj.Location.Position);
          val2 -= it.Quantity;
        }
        if (this.m_Rules.RollChance(Rules.IMPROVED_WEAPONS_FROM_BROKEN_WOOD_CHANCE))
        {
          ItemMeleeWeapon itemMeleeWeapon = !this.m_Rules.RollChance(50) ? new ItemMeleeWeapon((ItemModel) this.m_GameItems.IMPROVISED_SPEAR) : new ItemMeleeWeapon((ItemModel) this.m_GameItems.IMPROVISED_CLUB);
          mapObj.Location.Map.DropItemAt((Item) itemMeleeWeapon, mapObj.Location.Position);
        }
      }
      if (flag)
        doorWindow.SetState(3);
      else
        mapObj.Location.Map.RemoveMapObjectAt(mapObj.Location.Position.X, mapObj.Location.Position.Y);
      this.OnLoudNoise(mapObj.Location.Map, mapObj.Location.Position, "A loud *CRASH*");
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public void DoBreak(Actor actor, MapObject mapObj)
    {
      Attack attack = this.m_Rules.ActorMeleeAttack(actor, actor.CurrentMeleeAttack, (Actor) null);
      DoorWindow doorWindow = mapObj as DoorWindow;
      if (doorWindow != null && doorWindow.IsBarricaded)
      {
        this.SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
        this.SpendActorStaminaPoints(actor, Rules.STAMINA_COST_MELEE_ATTACK);
        doorWindow.BarricadePoints -= attack.DamageValue;
        this.OnLoudNoise(doorWindow.Location.Map, doorWindow.Location.Position, "A loud *BASH*");
        if (this.IsVisibleToPlayer(actor) || this.IsVisibleToPlayer((MapObject) doorWindow))
        {
          this.AddMessage(this.MakeMessage(actor, string.Format("{0} the barricade.", (object) this.Conjugate(actor, this.VERB_BASH))));
        }
        else
        {
          if (!m_Rules.RollChance(PLAYER_HEAR_BASH_CHANCE))
            return;
          this.AddMessageIfAudibleForPlayer(doorWindow.Location, this.MakePlayerCentricMessage("You hear someone bashing barricades", doorWindow.Location.Position));
        }
      }
      else
      {
        mapObj.HitPoints -= attack.DamageValue;
        this.SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
        this.SpendActorStaminaPoints(actor, Rules.STAMINA_COST_MELEE_ATTACK);
        bool flag = false;
        if (mapObj.HitPoints <= 0)
        {
          this.DoDestroyObject(mapObj);
          flag = true;
        }
        this.OnLoudNoise(mapObj.Location.Map, mapObj.Location.Position, "A loud *CRASH*");
        bool player1 = this.IsVisibleToPlayer(actor);
        bool player2 = this.IsVisibleToPlayer(mapObj);
        bool isPlayer = actor.IsPlayer;
        if (player1 || player2)
        {
          if (player1)
            this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Yellow, new Rectangle(this.MapToScreen(actor.Location.Position), new Size(32, 32))));
          if (player2)
            this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Red, new Rectangle(this.MapToScreen(mapObj.Location.Position), new Size(32, 32))));
          if (flag)
          {
            this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_BREAK), mapObj));
            if (player1)
              this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(actor.Location.Position), "Icons\\melee_attack"));
            if (player2)
              this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(mapObj.Location.Position), "Icons\\killed"));
            this.RedrawPlayScreen();
            AnimDelay(DELAY_LONG);
          }
          else
          {
            this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_BASH), mapObj));
            if (player1)
              this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(actor.Location.Position), "Icons\\melee_attack"));
            if (player2)
              this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(mapObj.Location.Position), "Icons\\melee_damage"));
            this.RedrawPlayScreen();
            AnimDelay(isPlayer ? DELAY_NORMAL : DELAY_SHORT);
          }
        }
        else if (flag)
        {
          if (this.m_Rules.RollChance(PLAYER_HEAR_BREAK_CHANCE))
            this.AddMessageIfAudibleForPlayer(mapObj.Location, this.MakePlayerCentricMessage("You hear someone breaking furniture", mapObj.Location.Position));
        }
        else if (this.m_Rules.RollChance(PLAYER_HEAR_BASH_CHANCE))
          this.AddMessageIfAudibleForPlayer(mapObj.Location, this.MakePlayerCentricMessage("You hear someone bashing furniture", mapObj.Location.Position));
        this.ClearOverlays();
      }
    }

    public void DoPush(Actor actor, MapObject mapObj, Point toPos)
    {
      bool flag = this.IsVisibleToPlayer(actor) || this.IsVisibleToPlayer(mapObj);
      int staminaCost = mapObj.Weight;
      if (actor.CountFollowers > 0)
      {
        Location location = new Location(actor.Location.Map, mapObj.Location.Position);
        List<Actor> actorList = (List<Actor>) null;
        foreach (Actor follower in actor.Followers)
        {
          if (!follower.IsSleeping && (follower.Activity == Activity.IDLE || follower.Activity == Activity.FOLLOWING) && this.m_Rules.IsAdjacent(follower.Location, mapObj.Location))
          {
            if (actorList == null)
              actorList = new List<Actor>(actor.CountFollowers);
            actorList.Add(follower);
          }
        }
        if (actorList != null)
        {
          staminaCost = mapObj.Weight / (1 + actorList.Count);
          foreach (Actor actor1 in actorList)
          {
            SpendActorActionPoints(actor1, Rules.BASE_ACTION_COST);
            this.SpendActorStaminaPoints(actor1, staminaCost);
            if (flag)
              this.AddMessage(this.MakeMessage(actor1, string.Format("{0} {1} pushing {2}.", (object) this.Conjugate(actor1, this.VERB_HELP), (object) actor.Name, (object) mapObj.TheName)));
          }
        }
      }
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      this.SpendActorStaminaPoints(actor, staminaCost);
      Map map = mapObj.Location.Map;
      Point position = mapObj.Location.Position;
      map.RemoveMapObjectAt(mapObj.Location.Position.X, mapObj.Location.Position.Y);
      map.PlaceMapObjectAt(mapObj, toPos);
      if (!this.m_Rules.IsAdjacent(toPos, actor.Location.Position) && this.m_Rules.IsWalkableFor(actor, map, position.X, position.Y))
      {
        map.RemoveActor(actor);
        map.PlaceActorAt(actor, position);
      }
      if (flag)
      {
        this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_PUSH), mapObj));
        this.RedrawPlayScreen();
      }
      else
      {
        this.OnLoudNoise(map, toPos, "Something being pushed");
        if (m_Rules.RollChance(PLAYER_HEAR_PUSH_CHANCE))
          this.AddMessageIfAudibleForPlayer(mapObj.Location, this.MakePlayerCentricMessage("You hear something being pushed", toPos));
      }
      this.CheckMapObjectTriggersTraps(map, toPos);
    }

    public void DoShove(Actor actor, Actor target, Point toPos)
    {
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      if (this.TryActorLeaveTile(target))
      {
        this.SpendActorStaminaPoints(actor, 10);
        this.DoStopDraggingCorpses(target);
        Map map = target.Location.Map;
        Point position = target.Location.Position;
        map.PlaceActorAt(target, toPos);
        if (!this.m_Rules.IsAdjacent(toPos, actor.Location.Position) && this.m_Rules.IsWalkableFor(actor, map, position.X, position.Y))
        {
          if (!this.TryActorLeaveTile(actor))
            return;
          map.RemoveActor(actor);
          map.PlaceActorAt(actor, position);
          this.OnActorEnterTile(actor);
        }
        if (this.IsVisibleToPlayer(actor) || this.IsVisibleToPlayer(target) || this.IsVisibleToPlayer(map, toPos))
        {
          this.AddMessage(this.MakeMessage(actor, this.Conjugate(actor, this.VERB_SHOVE), target));
          this.RedrawPlayScreen();
        }
        if (target.IsSleeping)
          this.DoWakeUp(target);
        this.OnActorEnterTile(target);
      }
    }

    public void DoStartSleeping(Actor actor)
    {
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      this.DoStopDraggingCorpses(actor);
      actor.Activity = Activity.SLEEPING;
      actor.IsSleeping = true;
    }

    public void DoWakeUp(Actor actor)
    {
      actor.Activity = Activity.IDLE;
      actor.IsSleeping = false;
      if (this.IsVisibleToPlayer(actor))
        this.AddMessage(this.MakeMessage(actor, string.Format("{0}.", (object) this.Conjugate(actor, this.VERB_WAKE_UP))));
      if (!actor.IsPlayer)
        return;
      this.m_MusicManager.StopAll();
    }

    private void DoTag(Actor actor, ItemSprayPaint spray, Point pos)
    {
      SpendActorActionPoints(actor, Rules.BASE_ACTION_COST);
      --spray.PaintQuantity;
      actor.Location.Map.GetTileAt(pos.X, pos.Y).AddDecoration((spray.Model as ItemSprayPaintModel).TagImageID);
      if (!this.IsVisibleToPlayer(actor))
        return;
      this.AddMessage(this.MakeMessage(actor, string.Format("{0} a tag.", (object) this.Conjugate(actor, this.VERB_SPRAY))));
    }

    private void DoGiveOrderTo(Actor master, Actor slave, ActorOrder order)
    {
      SpendActorActionPoints(master, Rules.BASE_ACTION_COST);
      if (master != slave.Leader)
        this.DoSay(slave, master, "Who are you to give me orders?", RogueGame.Sayflags.IS_FREE_ACTION);
      else if (!this.m_Rules.IsActorTrustingLeader(slave))
      {
        this.DoSay(slave, master, "Sorry, I don't trust you enough yet.", RogueGame.Sayflags.IS_IMPORTANT | RogueGame.Sayflags.IS_FREE_ACTION);
      }
      else
      {
        AIController aiController = slave.Controller as AIController;
        if (aiController == null)
          return;
        aiController.SetOrder(order);
        if (!this.IsVisibleToPlayer(master) && !this.IsVisibleToPlayer(slave))
          return;
        this.AddMessage(this.MakeMessage(master, this.Conjugate(master, this.VERB_ORDER), slave, string.Format(" to {0}.", (object) order.ToString())));
      }
    }

    private void DoCancelOrder(Actor master, Actor slave)
    {
      SpendActorActionPoints(master, Rules.BASE_ACTION_COST);
      AIController aiController = slave.Controller as AIController;
      if (aiController == null)
        return;
      aiController.SetOrder((ActorOrder) null);
      if (!this.IsVisibleToPlayer(master) && !this.IsVisibleToPlayer(slave))
        return;
      this.AddMessage(this.MakeMessage(master, this.Conjugate(master, this.VERB_ORDER), slave, " to forget its orders."));
    }

    private void OnLoudNoise(Map map, Point noisePosition, string noiseName)
    {
      int x1 = noisePosition.X - 5;
      int x2 = noisePosition.X + 5;
      int y1 = noisePosition.Y - 5;
      int y2 = noisePosition.Y + 5;
      map.TrimToBounds(ref x1, ref y1);
      map.TrimToBounds(ref x2, ref y2);
      for (int index1 = x1; index1 <= x2; ++index1)
      {
        for (int index2 = y1; index2 <= y2; ++index2)
        {
          Actor actorAt = map.GetActorAt(index1, index2);
          if (actorAt != null && actorAt.IsSleeping)
          {
            int noiseDistance = this.m_Rules.GridDistance(noisePosition, index1, index2);
            if (noiseDistance <= Rules.LOUD_NOISE_RADIUS && m_Rules.RollChance(this.m_Rules.ActorLoudNoiseWakeupChance(actorAt, noiseDistance)))
            {
              this.DoWakeUp(actorAt);
              if (this.IsVisibleToPlayer(actorAt))
              {
                this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0} wakes {1} up!", (object) noiseName, (object) actorAt.TheName), map.LocalTime.TurnCounter, actorAt == this.m_Player ? Color.Red : Color.White));
                this.RedrawPlayScreen();
              }
            }
          }
        }
      }
      if (!this.m_IsPlayerLongWait || (map != this.m_Player.Location.Map || !this.IsVisibleToPlayer(map, noisePosition)))
        return;
      this.m_IsPlayerLongWaitForcedStop = true;
    }

    private void InflictDamage(Actor actor, int dmg)
    {
      actor.HitPoints -= dmg;
      if (actor.Model.Abilities.CanTire)
        actor.StaminaPoints -= dmg;
      Item equippedItem = actor.GetEquippedItem(DollPart.TORSO);
      if (equippedItem != null && equippedItem is ItemBodyArmor && m_Rules.RollChance(Rules.BODY_ARMOR_BREAK_CHANCE))
      {
        this.OnUnequipItem(actor, equippedItem);
        actor.Inventory.RemoveAllQuantity(equippedItem);
        if (this.IsVisibleToPlayer(actor))
        {
          this.AddMessage(this.MakeMessage(actor, string.Format(": {0} breaks and is now useless!", (object) equippedItem.TheName)));
          this.RedrawPlayScreen();
          this.AnimDelay(actor.IsPlayer ? DELAY_NORMAL : DELAY_SHORT);
        }
      }
      if (!actor.IsSleeping)
        return;
      this.DoWakeUp(actor);
    }

    public void KillActor(Actor killer, Actor deadGuy, string reason)
    {
      deadGuy.IsDead = true;
      this.DoStopDraggingCorpses(deadGuy);
      this.UntriggerAllTrapsHere(deadGuy.Location);
      if (killer != null && !killer.Model.Abilities.IsUndead && (killer.Model.Abilities.HasSanity && deadGuy.Model.Abilities.IsUndead))
        this.RegenActorSanity(killer, this.m_Rules.ActorSanRegenValue(killer, Rules.SANITY_RECOVER_KILL_UNDEAD));
      if (deadGuy.HasLeader)
      {
        if (this.m_Rules.HasActorBondWith(deadGuy.Leader, deadGuy))
        {
          this.SpendActorSanity(deadGuy.Leader, Rules.SANITY_HIT_BOND_DEATH);
          if (this.IsVisibleToPlayer(deadGuy.Leader))
          {
            if (deadGuy.Leader.IsPlayer)
              this.ClearMessages();
            this.AddMessage(this.MakeMessage(deadGuy.Leader, string.Format("{0} deeply disturbed by {1} sudden death!", (object) this.Conjugate(deadGuy.Leader, this.VERB_BE), (object) deadGuy.Name)));
            if (deadGuy.Leader.IsPlayer)
              this.AddMessagePressEnter();
          }
        }
      }
      else if (deadGuy.CountFollowers > 0)
      {
        foreach (Actor follower in deadGuy.Followers)
        {
          if (this.m_Rules.HasActorBondWith(follower, deadGuy))
          {
            this.SpendActorSanity(follower, Rules.SANITY_HIT_BOND_DEATH);
            if (this.IsVisibleToPlayer(follower))
            {
              if (follower.IsPlayer)
                this.ClearMessages();
              this.AddMessage(this.MakeMessage(follower, string.Format("{0} deeply disturbed by {1} sudden death!", (object) this.Conjugate(follower, this.VERB_BE), (object) deadGuy.Name)));
              if (follower.IsPlayer)
                this.AddMessagePressEnter();
            }
          }
        }
      }
      if (deadGuy.IsUnique)
      {
        if (killer != null)
          this.m_Session.Scoring.AddEvent(deadGuy.Location.Map.LocalTime.TurnCounter, string.Format("* {0} was killed by {1} {2}! *", (object) deadGuy.TheName, (object) killer.Model.Name, (object) killer.TheName));
        else
          this.m_Session.Scoring.AddEvent(deadGuy.Location.Map.LocalTime.TurnCounter, string.Format("* {0} died by {1}! *", (object) deadGuy.TheName, (object) reason));
      }
      if (deadGuy == this.m_Player)
        this.PlayerDied(killer, reason);
      deadGuy.RemoveAllFollowers();
      if (deadGuy.Leader != null)
      {
        if (deadGuy.Leader.IsPlayer)
        {
          string text = killer == null ? string.Format("Follower {0} died by {1}!", (object) deadGuy.TheName, (object) reason) : string.Format("Follower {0} was killed by {1} {2}!", (object) deadGuy.TheName, (object) killer.Model.Name, (object) killer.TheName);
          this.m_Session.Scoring.AddEvent(deadGuy.Location.Map.LocalTime.TurnCounter, text);
        }
        deadGuy.Leader.RemoveFollower(deadGuy);
      }
      bool flag1 = killer != null && this.m_Rules.IsMurder(killer, deadGuy);
      deadGuy.RemoveAllAgressorSelfDefenceRelations();
      deadGuy.Location.Map.RemoveActor(deadGuy);
      if (deadGuy.Inventory != null && !deadGuy.Inventory.IsEmpty)
      {
        Item[] objArray = new Item[deadGuy.Inventory.CountItems];
        for (int index = 0; index < objArray.Length; ++index)
          objArray[index] = deadGuy.Inventory[index];
        for (int index = 0; index < objArray.Length; ++index)
        {
          Item it = objArray[index];
          int chance = it is ItemAmmo || it is ItemFood ? 100 : 50;
          if (it.Model.IsUnbreakable || it.IsUnique || this.m_Rules.RollChance(chance))
            this.DropItem(deadGuy, it);
        }
      }
      if (!deadGuy.Model.Abilities.IsUndead)
        this.SplatterBlood(deadGuy.Location.Map, deadGuy.Location.Position);
      if (Rules.HasCorpses(this.m_Session.GameMode) && !deadGuy.Model.Abilities.IsUndead)
        this.DropCorpse(deadGuy);
      if (killer != null)
        ++killer.KillsCount;
      if (killer == this.m_Player)
        this.PlayerKill(deadGuy);
      if (killer != null && Rules.HasEvolution(this.m_Session.GameMode) && killer.Model.Abilities.IsUndead)
      {
        ActorModel actorModel = this.CheckUndeadEvolution(killer);
        if (actorModel != null)
        {
          SkillTable skillTable = (SkillTable) null;
          if (killer.Sheet.SkillTable != null && killer.Sheet.SkillTable.Skills != null)
            skillTable = new SkillTable(killer.Sheet.SkillTable.Skills);
          killer.Model = actorModel;
          if (killer.IsPlayer)
            this.PrepareActorForPlayerControl(killer);
          if (skillTable != null)
          {
            foreach (Skill skill in skillTable.Skills)
            {
              for (int index = 0; index < skill.Level; ++index)
              {
                killer.Sheet.SkillTable.AddOrIncreaseSkill(skill.ID);
                this.OnSkillUpgrade(killer, (Skills.IDs) skill.ID);
              }
            }
            this.m_TownGenerator.RecomputeActorStartingStats(killer);
          }
          if (this.IsVisibleToPlayer(killer))
          {
            this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Yellow, new Rectangle(this.MapToScreen(killer.Location.Position), new Size(32, 32))));
            this.AddMessage(this.MakeMessage(killer, string.Format("{0} a {1} horror!", (object) this.Conjugate(killer, this.VERB_TRANSFORM_INTO), (object) actorModel.Name)));
            this.RedrawPlayScreen();
            AnimDelay(DELAY_LONG);
            this.ClearOverlays();
          }
        }
      }
      if (killer != null && killer.CountFollowers > 0)
      {
        foreach (Actor follower in killer.Followers)
        {
          bool flag2 = false;
          if (follower.TargetActor == deadGuy || this.m_Rules.IsEnemyOf(follower, deadGuy) && this.m_Rules.IsAdjacent(follower.Location, deadGuy.Location))
            flag2 = true;
          if (flag2)
          {
            this.DoSay(follower, killer, "That was close! Thanks for the help!!", RogueGame.Sayflags.IS_FREE_ACTION);
            this.ModifyActorTrustInLeader(follower, 90, true);
          }
        }
      }
      if (flag1)
      {
        ++killer.MurdersCounter;
        if (killer.IsPlayer)
          this.m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, string.Format("Murdered {0} a {1}!", (object) deadGuy.TheName, (object) deadGuy.Model.Name));
        if (this.IsVisibleToPlayer(killer))
          this.AddMessage(this.MakeMessage(killer, string.Format("murdered {0}!!", (object) deadGuy.Name)));
        Map map = killer.Location.Map;
        Point position = killer.Location.Position;
        foreach (Actor actor in map.Actors)
        {
          if (actor.Model.Abilities.IsLawEnforcer && !actor.IsDead && (!actor.IsSleeping && !actor.IsPlayer) && (actor != killer && actor != deadGuy && (actor.Leader != killer && killer.Leader != actor)) && (this.m_Rules.GridDistance(actor.Location.Position, position) <= this.m_Rules.ActorFOV(actor, map.LocalTime, this.m_Session.World.Weather) && LOS.CanTraceViewLine(actor.Location, position)))
          {
            this.DoSay(actor, killer, string.Format("MURDER! {0} HAS KILLED {1}!", (object) killer.TheName, (object) deadGuy.TheName), RogueGame.Sayflags.IS_IMPORTANT | RogueGame.Sayflags.IS_FREE_ACTION);
            this.DoMakeAggression(actor, killer);
          }
        }
      }
      if (killer != null && deadGuy.MurdersCounter > 0 && (killer.Model.Abilities.IsLawEnforcer && !killer.Faction.IsEnemyOf(deadGuy.Faction)))
      {
        if (killer.IsPlayer)
          this.AddMessage(new djack.RogueSurvivor.Data.Message("You feel like you did your duty with killing a murderer.", this.m_Session.WorldTime.TurnCounter, Color.White));
        else
          this.DoSay(killer, deadGuy, "Good riddance, murderer!", RogueGame.Sayflags.IS_FREE_ACTION);
      }
      if (deadGuy != this.m_Session.UniqueActors.TheSewersThing.TheActor || killer != this.m_Player && killer.Leader != this.m_Player)
        return;
      this.m_Session.Scoring.SetCompletedAchievement(Achievement.IDs.KILLED_THE_SEWERS_THING);
      this.ShowNewAchievement(Achievement.IDs.KILLED_THE_SEWERS_THING);
    }

    private ActorModel CheckUndeadEvolution(Actor undead)
    {
      if (!RogueGame.s_Options.AllowUndeadsEvolution || !Rules.HasEvolution(this.m_Session.GameMode))
        return (ActorModel) null;
      bool flag;
      switch (undead.Model.ID)
      {
        case 0:
          if (undead.KillsCount < 2)
            return (ActorModel) null;
          flag = true;
          break;
        case 1:
          if (undead.KillsCount < 4)
            return (ActorModel) null;
          flag = true;
          break;
        case 3:
          flag = true;
          break;
        case 4:
          flag = true;
          break;
        case 6:
          if (undead.KillsCount < 4)
            return (ActorModel) null;
          if (undead.Location.Map.LocalTime.Day < 7 && !undead.IsPlayer)
            return (ActorModel) null;
          flag = true;
          break;
        case 7:
          if (undead.KillsCount < 8)
            return (ActorModel) null;
          flag = true;
          break;
        case 9:
        case 10:
          if (undead.KillsCount < 2)
            return (ActorModel) null;
          flag = true;
          break;
        case 11:
        case 12:
          if (undead.KillsCount < 4)
            return (ActorModel) null;
          if (undead.Location.Map.LocalTime.Day < 7 && !undead.IsPlayer)
            return (ActorModel) null;
          flag = true;
          break;
        default:
          flag = false;
          break;
      }
      if (!flag)
        return (ActorModel) null;
      GameActors.IDs index = this.NextUndeadEvolution((GameActors.IDs) undead.Model.ID);
      if (index == (GameActors.IDs) undead.Model.ID)
        return (ActorModel) null;
      return this.GameActors[index];
    }

    public GameActors.IDs NextUndeadEvolution(GameActors.IDs fromModelID)
    {
      switch (fromModelID)
      {
        case GameActors.IDs._FIRST:
          return GameActors.IDs.UNDEAD_RED_EYED_SKELETON;
        case GameActors.IDs.UNDEAD_RED_EYED_SKELETON:
          return GameActors.IDs.UNDEAD_RED_SKELETON;
        case GameActors.IDs.UNDEAD_ZOMBIE:
          return GameActors.IDs.UNDEAD_DARK_EYED_ZOMBIE;
        case GameActors.IDs.UNDEAD_DARK_EYED_ZOMBIE:
          return GameActors.IDs.UNDEAD_DARK_ZOMBIE;
        case GameActors.IDs.UNDEAD_ZOMBIE_MASTER:
          return GameActors.IDs.UNDEAD_ZOMBIE_LORD;
        case GameActors.IDs.UNDEAD_ZOMBIE_LORD:
          return GameActors.IDs.UNDEAD_ZOMBIE_PRINCE;
        case GameActors.IDs.UNDEAD_MALE_ZOMBIFIED:
          return GameActors.IDs.UNDEAD_MALE_NEOPHYTE;
        case GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED:
          return GameActors.IDs.UNDEAD_FEMALE_NEOPHYTE;
        case GameActors.IDs.UNDEAD_MALE_NEOPHYTE:
          return GameActors.IDs.UNDEAD_MALE_DISCIPLE;
        case GameActors.IDs.UNDEAD_FEMALE_NEOPHYTE:
          return GameActors.IDs.UNDEAD_FEMALE_DISCIPLE;
        default:
          return fromModelID;
      }
    }

    public void SplatterBlood(Map map, Point position)
    {
      Tile tileAt1 = map.GetTileAt(position.X, position.Y);
      if (map.IsWalkable(position.X, position.Y) && !tileAt1.HasDecoration("Tiles\\Decoration\\bloodied_floor"))
      {
        tileAt1.AddDecoration("Tiles\\Decoration\\bloodied_floor");
        map.AddTimer((TimedTask) new TaskRemoveDecoration(WorldTime.TURNS_PER_DAY, position.X, position.Y, "Tiles\\Decoration\\bloodied_floor"));
      }
      foreach (Direction direction in Direction.COMPASS)
      {
        if (this.m_Rules.RollChance(20))
        {
          Point p = position + direction;
          if (map.IsInBounds(p))
          {
            Tile tileAt2 = map.GetTileAt(p.X, p.Y);
            if (!tileAt2.Model.IsWalkable && !tileAt2.HasDecoration("Tiles\\Decoration\\bloodied_wall"))
            {
              tileAt2.AddDecoration("Tiles\\Decoration\\bloodied_wall");
              map.AddTimer((TimedTask) new TaskRemoveDecoration(WorldTime.TURNS_PER_DAY, p.X, p.Y, "Tiles\\Decoration\\bloodied_wall"));
            }
          }
        }
      }
    }

    public void UndeadRemains(Map map, Point position)
    {
      Tile tileAt = map.GetTileAt(position.X, position.Y);
      if (!map.IsWalkable(position.X, position.Y) || tileAt.HasDecoration("Tiles\\Decoration\\zombie_remains"))
        return;
      tileAt.AddDecoration("Tiles\\Decoration\\zombie_remains");
    }

    public void DropCorpse(Actor deadGuy)
    {
      deadGuy.Doll.AddDecoration(DollPart.TORSO, "Actors\\Decoration\\bloodied");
      int num = this.m_Rules.ActorMaxHPs(deadGuy);
      float rotation = (float) this.m_Rules.Roll(30, 60);
      if (this.m_Rules.RollChance(50))
        rotation = -rotation;
      float scale = 1f;
      deadGuy.Location.Map.AddCorpseAt(new Corpse(deadGuy, num, num, deadGuy.Location.Map.LocalTime.TurnCounter, rotation, scale), deadGuy.Location.Position);
    }

    private void PlayerDied(Actor killer, string reason)
    {
      this.StopSimThread();
      this.m_UI.UI_SetCursor((Cursor) null);
      this.m_MusicManager.StopAll();
      this.m_MusicManager.Play(GameMusics.PLAYER_DEATH);
      this.m_Session.Scoring.TurnsSurvived = this.m_Session.WorldTime.TurnCounter;
      this.m_Session.Scoring.SetKiller(killer);
      if (this.m_Player.CountFollowers > 0)
      {
        foreach (Actor follower in this.m_Player.Followers)
          this.m_Session.Scoring.AddFollowerWhenDied(follower);
      }
      List<Zone> zonesAt = this.m_Player.Location.Map.GetZonesAt(this.m_Player.Location.Position.X, this.m_Player.Location.Position.Y);
      this.m_Session.Scoring.DeathPlace = zonesAt != null ? string.Format("{0} at {1}", (object) this.m_Player.Location.Map.Name, (object) zonesAt[0].Name) : this.m_Player.Location.Map.Name;
      this.m_Session.Scoring.DeathReason = killer == null ? string.Format("Death by {0}", (object) reason) : string.Format("{0} by {1} {2}", this.m_Rules.IsMurder(killer, this.m_Player) ? (object) "Murdered" : (object) "Killed", (object) killer.Model.Name, (object) killer.TheName);
      this.m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, "Died.");
      int index = this.m_Rules.Roll(0, GameTips.TIPS.Length);
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(new string[3]
      {
        "TIP OF THE DEAD",
        "Did you know that...",
        GameTips.TIPS[index]
      }, Color.White, Color.White, this.POPUP_FILLCOLOR, new Point(0, 0)));
      this.ClearMessages();
      this.AddMessage(new djack.RogueSurvivor.Data.Message("**** YOU DIED! ****", this.m_Session.WorldTime.TurnCounter, Color.Red));
      if (killer != null)
        this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Killer : {0}.", (object) killer.TheName), this.m_Session.WorldTime.TurnCounter, Color.Red));
      this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Reason : {0}.", (object) reason), this.m_Session.WorldTime.TurnCounter, Color.Red));
      if (this.m_Player.Model.Abilities.IsUndead)
        this.AddMessage(new djack.RogueSurvivor.Data.Message("You die one last time... Game over!", this.m_Session.WorldTime.TurnCounter, Color.Red));
      else
        this.AddMessage(new djack.RogueSurvivor.Data.Message("You join the realm of the undeads... Game over!", this.m_Session.WorldTime.TurnCounter, Color.Red));
      if (RogueGame.s_Options.IsPermadeathOn)
        this.DeleteSavedGame(RogueGame.GetUserSave());
      if (RogueGame.s_Options.IsDeathScreenshotOn)
      {
        this.RedrawPlayScreen();
        string screenshot = this.DoTakeScreenshot();
        if (screenshot == null)
          this.AddMessage(this.MakeErrorMessage("could not save death screenshot."));
        else
          this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Death screenshot saved : {0}.", (object) screenshot), this.m_Session.WorldTime.TurnCounter, Color.Red));
      }
      this.AddMessagePressEnter();
      this.HandlePostMortem();
      this.m_MusicManager.StopAll();
    }

    private string TimeSpanToString(TimeSpan rt)
    {
      return string.Format("{0}{1}{2}{3}", (object) (rt.Days == 0 ? "" : string.Format("{0} days ", (object) rt.Days)), (object) (rt.Hours == 0 ? "" : string.Format("{0:D2} hours ", (object) rt.Hours)), (object) (rt.Minutes == 0 ? "" : string.Format("{0:D2} minutes ", (object) rt.Minutes)), (object) (rt.Seconds == 0 ? "" : string.Format("{0:D2} seconds", (object) rt.Seconds)));
    }

    private void HandlePostMortem()
    {
      WorldTime worldTime = new WorldTime();
      worldTime.TurnCounter = this.m_Session.Scoring.TurnsSurvived;
      bool isMale = this.m_Player.Model.DollBody.IsMale;
      string str1 = isMale ? "He" : "She";
      this.HisOrHer(this.m_Player);
      string str2 = isMale ? "him" : "her";
      string name = this.m_Player.TheName.Replace("(YOU) ", "");
      string @string = this.TimeSpanToString(this.m_Session.Scoring.RealLifePlayingTime);
      this.m_Session.Scoring.Side = this.m_Player.Model.Abilities.IsUndead ? DifficultySide.FOR_UNDEAD : DifficultySide.FOR_SURVIVOR;
      this.m_Session.Scoring.DifficultyRating = Scoring.ComputeDifficultyRating(RogueGame.s_Options, this.m_Session.Scoring.Side, this.m_Session.Scoring.ReincarnationNumber);
      TextFile textFile = new TextFile();
      textFile.Append(string.Format("ROGUE SURVIVOR {0}", (object) "alpha 9"));
      textFile.Append("POST MORTEM");
      textFile.Append(string.Format("{0} was {1} and {2}.", (object) name, (object) this.AorAn(this.m_Player.Model.Name), (object) this.AorAn(this.m_Player.Faction.MemberName)));
      textFile.Append(string.Format("{0} survived to see {1}.", (object) str1, (object) worldTime.ToString()));
      textFile.Append(string.Format("{0}'s spirit guided {1} for {2}.", (object) name, (object) str2, (object) @string));
      if (this.m_Session.Scoring.ReincarnationNumber > 0)
        textFile.Append(string.Format("{0} was reincarnation {1}.", (object) str1, (object) this.m_Session.Scoring.ReincarnationNumber));
      textFile.Append(" ");
      textFile.Append("> SCORING");
      textFile.Append(string.Format("{0} scored a total of {1} points.", (object) str1, (object) this.m_Session.Scoring.TotalPoints));
      textFile.Append(string.Format("- difficulty rating of {0}%.", (object) (int) (100.0 * (double) this.m_Session.Scoring.DifficultyRating)));
      textFile.Append(string.Format("- {0} base points for survival.", (object) this.m_Session.Scoring.SurvivalPoints));
      textFile.Append(string.Format("- {0} base points for kills.", (object) this.m_Session.Scoring.KillPoints));
      textFile.Append(string.Format("- {0} base points for achievements.", (object) this.m_Session.Scoring.AchievementPoints));
      textFile.Append(" ");
      textFile.Append("> ACHIEVEMENTS");
      foreach (Achievement achievement in this.m_Session.Scoring.Achievements)
      {
        if (achievement.IsDone)
          textFile.Append(string.Format("- {0} for {1} points!", (object) achievement.Name, (object) achievement.ScoreValue));
        else
          textFile.Append(string.Format("- Fail : {0}.", (object) achievement.TeaseName));
      }
      if (this.m_Session.Scoring.CompletedAchievementsCount == 0)
      {
        textFile.Append("Didn't achieve anything notable. And then died.");
        textFile.Append(string.Format("(unlock all the {0} achievements to win this game version)", (object) 8));
      }
      else
      {
        textFile.Append(string.Format("Total : {0}/{1}.", (object) this.m_Session.Scoring.CompletedAchievementsCount, (object) 8));
        if (this.m_Session.Scoring.CompletedAchievementsCount >= 8)
          textFile.Append("*** You achieved everything! You can consider having won this version of the game! CONGRATULATIONS! ***");
        else
          textFile.Append("(unlock all the achievements to win this game version)");
        textFile.Append("(later versions of the game will feature real winning conditions and multiple endings...)");
      }
      textFile.Append(" ");
      textFile.Append("> DEATH");
      textFile.Append(string.Format("{0} in {1}.", (object) this.m_Session.Scoring.DeathReason, (object) this.m_Session.Scoring.DeathPlace));
      textFile.Append(" ");
      textFile.Append("> KILLS");
      if (this.m_Session.Scoring.HasNoKills)
      {
        textFile.Append(string.Format("{0} was a pacifist. Or too scared to fight.", (object) str1));
      }
      else
      {
        foreach (Scoring.KillData kill in this.m_Session.Scoring.Kills)
        {
          string str3 = kill.Amount > 1 ? Models.Actors[kill.ActorModelID].PluralName : Models.Actors[kill.ActorModelID].Name;
          textFile.Append(string.Format("{0,4} {1}.", (object) kill.Amount, (object) str3));
        }
      }
      if (!this.m_Player.Model.Abilities.IsUndead && this.m_Player.MurdersCounter > 0)
        textFile.Append(string.Format("{0} committed {1} murder{2}!", (object) str1, (object) this.m_Player.MurdersCounter, this.m_Player.MurdersCounter > 1 ? (object) "s" : (object) ""));
      textFile.Append(" ");
      textFile.Append("> FUN FACTS!");
      textFile.Append(string.Format("While {0} has died, others are still having fun!", (object) name));
      foreach (string compileDistrictFunFact in this.CompileDistrictFunFacts(this.m_Player.Location.Map.District))
        textFile.Append(compileDistrictFunFact);
      textFile.Append("");
      textFile.Append("> SKILLS");
      if (this.m_Player.Sheet.SkillTable.Skills == null)
      {
        textFile.Append(string.Format("{0} was a jack of all trades. Or an incompetent.", (object) str1));
      }
      else
      {
        foreach (Skill skill in this.m_Player.Sheet.SkillTable.Skills)
          textFile.Append(string.Format("{0}-{1}.", (object) skill.Level, (object) Skills.Name(skill.ID)));
      }
      textFile.Append(" ");
      textFile.Append("> INVENTORY");
      if (this.m_Player.Inventory.IsEmpty)
      {
        textFile.Append(string.Format("{0} was humble. Or dirt poor.", (object) str1));
      }
      else
      {
        foreach (Item it in this.m_Player.Inventory.Items)
        {
          string str3 = this.DescribeItemShort(it);
          if (it.IsEquipped)
            textFile.Append(string.Format("- {0} (equipped).", (object) str3));
          else
            textFile.Append(string.Format("- {0}.", (object) str3));
        }
      }
      textFile.Append(" ");
      textFile.Append("> FOLLOWERS");
      if (this.m_Session.Scoring.FollowersWhendDied == null || this.m_Session.Scoring.FollowersWhendDied.Count == 0)
      {
        textFile.Append(string.Format("{0} was doing fine alone. Or everyone else was dead.", (object) str1));
      }
      else
      {
        StringBuilder stringBuilder = new StringBuilder(string.Format("{0} was leading", (object) str1));
        bool flag = true;
        int num = 0;
        int count = this.m_Session.Scoring.FollowersWhendDied.Count;
        foreach (Actor actor in this.m_Session.Scoring.FollowersWhendDied)
        {
          if (flag)
            stringBuilder.Append(" ");
          else if (num == count)
            stringBuilder.Append(".");
          else if (num == count - 1)
            stringBuilder.Append(" and ");
          else
            stringBuilder.Append(", ");
          stringBuilder.Append(actor.TheName);
          ++num;
          flag = false;
        }
        stringBuilder.Append(".");
        textFile.Append(stringBuilder.ToString());
        foreach (Actor actor in this.m_Session.Scoring.FollowersWhendDied)
        {
          textFile.Append(string.Format("{0} skills : ", (object) actor.Name));
          if (actor.Sheet.SkillTable != null && actor.Sheet.SkillTable.Skills != null)
          {
            foreach (Skill skill in actor.Sheet.SkillTable.Skills)
              textFile.Append(string.Format("{0}-{1}.", (object) skill.Level, (object) Skills.Name(skill.ID)));
          }
        }
      }
      textFile.Append(" ");
      textFile.Append("> EVENTS");
      if (this.m_Session.Scoring.HasNoEvents)
      {
        textFile.Append(string.Format("{0} had a quiet life. Or dull and boring.", (object) str1));
      }
      else
      {
        foreach (Scoring.GameEventData @event in this.m_Session.Scoring.Events)
          textFile.Append(string.Format("- {0,13} : {1}", (object) new WorldTime()
          {
            TurnCounter = @event.Turn
          }.ToString(), (object) @event.Text));
      }
      textFile.Append(" ");
      textFile.Append("> CUSTOM OPTIONS");
      textFile.Append(string.Format("- difficulty rating of {0}%.", (object) (int) (100.0 * (double) this.m_Session.Scoring.DifficultyRating)));
      if (RogueGame.s_Options.IsPermadeathOn)
        textFile.Append(string.Format("- {0} : yes.", (object) GameOptions.Name(GameOptions.IDs.GAME_PERMADEATH)));
      if (!RogueGame.s_Options.AllowUndeadsEvolution)
        textFile.Append(string.Format("- {0} : {1}.", (object) GameOptions.Name(GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION), RogueGame.s_Options.AllowUndeadsEvolution ? (object) "yes" : (object) "no"));
      if (RogueGame.s_Options.CitySize != GameOptions.DEFAULT_CITY_SIZE)
        textFile.Append(string.Format("- {0} : {1}.", (object) GameOptions.Name(GameOptions.IDs.GAME_CITY_SIZE), (object) RogueGame.s_Options.CitySize));
      if (RogueGame.s_Options.DayZeroUndeadsPercent != GameOptions.DEFAULT_DAY_ZERO_UNDEADS_PERCENT)
        textFile.Append(string.Format("- {0} : {1}%.", (object) GameOptions.Name(GameOptions.IDs.GAME_DAY_ZERO_UNDEADS_PERCENT), (object) RogueGame.s_Options.DayZeroUndeadsPercent));
      if (RogueGame.s_Options.DistrictSize != GameOptions.DEFAULT_DISTRICT_SIZE)
        textFile.Append(string.Format("- {0} : {1}.", (object) GameOptions.Name(GameOptions.IDs.GAME_DISTRICT_SIZE), (object) RogueGame.s_Options.DistrictSize));
      if (RogueGame.s_Options.MaxCivilians != GameOptions.DEFAULT_MAX_CIVILIANS)
        textFile.Append(string.Format("- {0} : {1}.", (object) GameOptions.Name(GameOptions.IDs.GAME_MAX_CIVILIANS), (object) RogueGame.s_Options.MaxCivilians));
      if (RogueGame.s_Options.MaxUndeads != GameOptions.DEFAULT_MAX_UNDEADS)
        textFile.Append(string.Format("- {0} : {1}.", (object) GameOptions.Name(GameOptions.IDs.GAME_MAX_UNDEADS), (object) RogueGame.s_Options.MaxUndeads));
      if (!RogueGame.s_Options.NPCCanStarveToDeath)
        textFile.Append(string.Format("- {0} : {1}.", (object) GameOptions.Name(GameOptions.IDs.GAME_NPC_CAN_STARVE_TO_DEATH), RogueGame.s_Options.NPCCanStarveToDeath ? (object) "yes" : (object) "no"));
      if (RogueGame.s_Options.StarvedZombificationChance != GameOptions.DEFAULT_STARVED_ZOMBIFICATION_CHANCE)
        textFile.Append(string.Format("- {0} : {1}%.", (object) GameOptions.Name(GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE), (object) RogueGame.s_Options.StarvedZombificationChance));
      if (!RogueGame.s_Options.RevealStartingDistrict)
        textFile.Append(string.Format("- {0} : {1}.", (object) GameOptions.Name(GameOptions.IDs.GAME_REVEAL_STARTING_DISTRICT), RogueGame.s_Options.RevealStartingDistrict ? (object) "yes" : (object) "no"));
      if (RogueGame.s_Options.SimulateDistricts != GameOptions.SimRatio.FULL)
        textFile.Append(string.Format("- {0} : {1}.", (object) GameOptions.Name(GameOptions.IDs.GAME_SIMULATE_DISTRICTS), (object) GameOptions.Name(RogueGame.s_Options.SimulateDistricts)));
      if (RogueGame.s_Options.SimulateWhenSleeping)
        textFile.Append(string.Format("- {0} : {1}.", (object) GameOptions.Name(GameOptions.IDs.GAME_SIMULATE_SLEEP), RogueGame.s_Options.SimulateWhenSleeping ? (object) "yes" : (object) "no"));
      if (RogueGame.s_Options.ZombieInvasionDailyIncrease != GameOptions.DEFAULT_ZOMBIE_INVASION_DAILY_INCREASE)
        textFile.Append(string.Format("- {0} : {1}%.", (object) GameOptions.Name(GameOptions.IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE), (object) RogueGame.s_Options.ZombieInvasionDailyIncrease));
      if (RogueGame.s_Options.ZombificationChance != GameOptions.DEFAULT_ZOMBIFICATION_CHANCE)
        textFile.Append(string.Format("- {0} : {1}%.", (object) GameOptions.Name(GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE), (object) RogueGame.s_Options.ZombificationChance));
      if (RogueGame.s_Options.MaxReincarnations != GameOptions.DEFAULT_MAX_REINCARNATIONS)
        textFile.Append(string.Format("- {0} : {1}.", (object) GameOptions.Name(GameOptions.IDs.GAME_MAX_REINCARNATIONS), (object) RogueGame.s_Options.MaxReincarnations));
      textFile.Append(" ");
      textFile.Append("> R.I.P");
      textFile.Append(string.Format("May {0} soul rest in peace.", (object) this.HisOrHer(this.m_Player)));
      textFile.Append(string.Format("For {0} body is now a meal for evil.", (object) this.HisOrHer(this.m_Player)));
      textFile.Append("The End.");
      int num1;
      int num2 = num1 = 0;
      this.m_UI.UI_Clear(Color.Black);
      this.m_UI.UI_DrawStringBold(Color.Yellow, "Saving post mortem to graveyard...", 0, 0, new Color?());
      int gy1 = num1 + 14;
      this.m_UI.UI_Repaint();
      string str4 = RogueGame.GraveFilePath(this.GetUserNewGraveyardName());
      int num3;
      if (!textFile.Save(str4))
      {
        this.m_UI.UI_DrawStringBold(Color.Red, "Could not save to graveyard.", 0, gy1, new Color?());
        num3 = gy1 + 14;
      }
      else
      {
        this.m_UI.UI_DrawStringBold(Color.Yellow, "Grave saved to :", 0, gy1, new Color?());
        int gy2 = gy1 + 14;
        this.m_UI.UI_DrawString(Color.White, str4, 0, gy2, new Color?());
        num3 = gy2 + 14;
      }
      this.DrawFootnote(Color.White, "press ENTER");
      this.m_UI.UI_Repaint();
      this.WaitEnter();
      textFile.FormatLines(TEXTFILE_CHARS_PER_LINE);
      int index = 0;
      do
      {
        this.m_UI.UI_Clear(Color.Black);
        int num4;
        int gx = num4 = 0;
        this.DrawHeader();
        int gy2 = num4 + 14;
        int num5 = 0;
        this.m_UI.UI_DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, gy2, new Color?());
        int gy3 = gy2 + 14;
        for (; num5 < 50 && index < textFile.FormatedLines.Count; ++num5)
        {
          this.m_UI.UI_DrawStringBold(Color.White, textFile.FormatedLines[index], gx, gy3, new Color?());
          gy3 += 14;
          ++index;
        }
        this.m_UI.UI_DrawStringBold(Color.White, "---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+---------+", 0, 740, new Color?());
        if (index < textFile.FormatedLines.Count)
          this.DrawFootnote(Color.White, "press ENTER for more");
        else
          this.DrawFootnote(Color.White, "press ENTER to leave");
        this.m_UI.UI_Repaint();
        this.WaitEnter();
      }
      while (index < textFile.FormatedLines.Count);
      StringBuilder stringBuilder1 = new StringBuilder();
      if (this.m_Player.Sheet.SkillTable.Skills != null)
      {
        foreach (Skill skill in this.m_Player.Sheet.SkillTable.Skills)
          stringBuilder1.AppendFormat("{0}-{1} ", (object) skill.Level, (object) Skills.Name(skill.ID));
      }
      if (!this.m_HiScoreTable.Register(HiScore.FromScoring(name, this.m_Session.Scoring, stringBuilder1.ToString())))
        return;
      this.SaveHiScoreTable();
      this.HandleHiScores(true);
    }

    private void OnNewNight()
    {
      this.UpdatePlayerFOV(this.m_Player);
      if (!this.m_Player.Model.Abilities.IsUndead || this.m_Player.Location.Map.LocalTime.Day % 2 != 1)
        return;
      this.ClearOverlays();
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.UPGRADE_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, Point.Empty));
      this.m_MusicManager.StopAll();
      this.m_MusicManager.Play(GameMusics.INTERLUDE);
      this.ClearMessages();
      this.AddMessage(new djack.RogueSurvivor.Data.Message("You will hunt another day!", this.m_Session.WorldTime.TurnCounter, Color.Green));
      this.UpdatePlayerFOV(this.m_Player);
      this.AddMessagePressEnter();
      this.HandlePlayerDecideUpgrade(this.m_Player);
      this.HandlePlayerFollowersUpgrade();
      this.ClearMessages();
      this.AddMessage(new djack.RogueSurvivor.Data.Message("Welcome to the night.", this.m_Session.WorldTime.TurnCounter, Color.White));
      this.ClearOverlays();
      this.RedrawPlayScreen();
      this.m_MusicManager.StopAll();
    }

    private void OnNewDay()
    {
      if (!this.m_Player.Model.Abilities.IsUndead)
      {
        this.ClearOverlays();
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(this.UPGRADE_MODE_TEXT, this.MODE_TEXTCOLOR, this.MODE_BORDERCOLOR, this.MODE_FILLCOLOR, Point.Empty));
        this.m_MusicManager.StopAll();
        this.m_MusicManager.Play(GameMusics.INTERLUDE);
        this.ClearMessages();
        this.AddMessage(new djack.RogueSurvivor.Data.Message("You survived another night!", this.m_Session.WorldTime.TurnCounter, Color.Green));
        this.UpdatePlayerFOV(this.m_Player);
        this.AddMessagePressEnter();
        this.HandlePlayerDecideUpgrade(this.m_Player);
        this.HandlePlayerFollowersUpgrade();
        this.ClearMessages();
        this.AddMessage(new djack.RogueSurvivor.Data.Message("Welcome to tomorrow.", this.m_Session.WorldTime.TurnCounter, Color.White));
        this.ClearOverlays();
        this.RedrawPlayScreen();
        this.m_MusicManager.StopAll();
      }
      this.CheckWeatherChange();
      if (this.m_Player.Model.Abilities.IsUndead)
        return;
      if (this.m_Session.WorldTime.Day == 7)
      {
        this.m_Session.Scoring.SetCompletedAchievement(Achievement.IDs._FIRST);
        this.ShowNewAchievement(Achievement.IDs._FIRST);
      }
      else if (this.m_Session.WorldTime.Day == 14)
      {
        this.m_Session.Scoring.SetCompletedAchievement(Achievement.IDs.REACHED_DAY_14);
        this.ShowNewAchievement(Achievement.IDs.REACHED_DAY_14);
      }
      else if (this.m_Session.WorldTime.Day == 21)
      {
        this.m_Session.Scoring.SetCompletedAchievement(Achievement.IDs.REACHED_DAY_21);
        this.ShowNewAchievement(Achievement.IDs.REACHED_DAY_21);
      }
      else
      {
        if (this.m_Session.WorldTime.Day != 28)
          return;
        this.m_Session.Scoring.SetCompletedAchievement(Achievement.IDs.REACHED_DAY_28);
        this.ShowNewAchievement(Achievement.IDs.REACHED_DAY_28);
      }
    }

    private void HandlePlayerDecideUpgrade(Actor upgradeActor)
    {
      List<Skills.IDs> upgrade = this.RollSkillsToUpgrade(upgradeActor, 300);
      string str = upgradeActor == this.m_Player ? "You" : upgradeActor.Name;
      bool flag = true;
      do
      {
        this.ClearMessages();
        this.AddMessage(new djack.RogueSurvivor.Data.Message(str + " can improve or learn one of these skills. Choose wisely.", this.m_Session.WorldTime.TurnCounter, Color.Green));
        if (upgrade.Count == 0)
        {
          this.AddMessage(this.MakeErrorMessage(str + " can't learn anything new!"));
        }
        else
        {
          for (int index = 0; index < upgrade.Count; ++index)
          {
            Skills.IDs id = upgrade[index];
            int skillLevel = upgradeActor.Sheet.SkillTable.GetSkillLevel((int) id);
            this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("choice {0} : {1} from {2} to {3} - {4}", (object) (index + 1), (object) Skills.Name(id), (object) skillLevel, (object) (skillLevel + 1), (object) this.DescribeSkillShort(id)), this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
          }
        }
        this.AddMessage(new djack.RogueSurvivor.Data.Message("ESC if you don't want to upgrade.", this.m_Session.WorldTime.TurnCounter, Color.White));
        this.RedrawPlayScreen();
        KeyEventArgs key = this.m_UI.UI_WaitKey();
        int num = (int) InputTranslator.KeyToCommand(key);
        if (key.KeyCode == Keys.Escape)
        {
          flag = false;
        }
        else
        {
          int choiceNumber = this.KeyToChoiceNumber(key.KeyCode);
          if (choiceNumber >= 1 && choiceNumber <= upgrade.Count)
          {
            Skills.IDs id = upgrade[choiceNumber - 1];
            Skill skill = this.SkillUpgrade(upgradeActor, id);
            if (skill.Level == 1)
            {
              this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0} learned skill {1}.", (object) upgradeActor.Name, (object) Skills.Name(skill.ID)), this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
              this.m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, string.Format("{0} learned skill {1}.", (object) upgradeActor.Name, (object) Skills.Name(skill.ID)));
            }
            else
            {
              this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0} improved skill {1} to level {2}.", (object) upgradeActor.Name, (object) Skills.Name(skill.ID), (object) skill.Level), this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
              this.m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, string.Format("{0} improved skill {1} to level {2}.", (object) upgradeActor.Name, (object) Skills.Name(skill.ID), (object) skill.Level));
            }
            this.AddMessagePressEnter();
            flag = false;
          }
        }
      }
      while (flag);
    }

    private void HandlePlayerFollowersUpgrade()
    {
      if (this.m_Player.CountFollowers == 0)
        return;
      this.ClearMessages();
      this.AddMessage(new djack.RogueSurvivor.Data.Message("Your followers learned new skills at your side!", this.m_Session.WorldTime.TurnCounter, Color.Green));
      this.AddMessagePressEnter();
      foreach (Actor follower in this.m_Player.Followers)
        this.HandlePlayerDecideUpgrade(follower);
    }

    private void HandleLivingNPCsUpgrade(Map map)
    {
      foreach (Actor actor in map.Actors)
      {
        if (actor != this.m_Player && actor.Leader != this.m_Player && !actor.Model.Abilities.IsUndead)
        {
          List<Skills.IDs> upgrade1 = this.RollSkillsToUpgrade(actor, 300);
          Skills.IDs? upgrade2 = this.NPCPickSkillToUpgrade(actor, upgrade1);
          if (upgrade2.HasValue)
            this.SkillUpgrade(actor, upgrade2.Value);
        }
      }
    }

    private void HandleUndeadNPCsUpgrade(Map map)
    {
      foreach (Actor actor in map.Actors)
      {
        if (actor != this.m_Player && actor.Leader != this.m_Player && actor.Model.Abilities.IsUndead && ((RogueGame.s_Options.SkeletonsUpgrade || !this.GameActors.IsSkeletonBranch(actor.Model)) && (RogueGame.s_Options.RatsUpgrade || !this.GameActors.IsRatBranch(actor.Model))) && (RogueGame.s_Options.ShamblersUpgrade || !this.GameActors.IsShamblerBranch(actor.Model)))
        {
          List<Skills.IDs> upgrade1 = this.RollSkillsToUpgrade(actor, 300);
          Skills.IDs? upgrade2 = this.NPCPickSkillToUpgrade(actor, upgrade1);
          if (upgrade2.HasValue)
            this.SkillUpgrade(actor, upgrade2.Value);
        }
      }
    }

    private List<Skills.IDs> RollSkillsToUpgrade(Actor actor, int maxTries)
    {
      int capacity = actor.Model.Abilities.IsUndead ? Rules.UNDEAD_UPGRADE_SKILLS_TO_CHOOSE_FROM : Rules.UPGRADE_SKILLS_TO_CHOOSE_FROM;
      List<Skills.IDs> idsList = new List<Skills.IDs>(capacity);
      for (int index = 0; index < capacity; ++index)
      {
        int num = 0;
        Skills.IDs? upgrade;
        do
        {
          ++num;
          upgrade = this.RollRandomSkillToUpgrade(actor, maxTries);
          if (!upgrade.HasValue)
            return idsList;
        }
        while (idsList.Contains(upgrade.Value) && num < maxTries);
        idsList.Add(upgrade.Value);
      }
      return idsList;
    }

    private Skills.IDs? NPCPickSkillToUpgrade(Actor npc, List<Skills.IDs> chooseFrom)
    {
      if (chooseFrom == null || chooseFrom.Count == 0)
        return new Skills.IDs?();
      int count = chooseFrom.Count;
      int[] numArray = new int[count];
      int num = -1;
      for (int index = 0; index < count; ++index)
      {
        numArray[index] = this.NPCSkillUtility(npc, chooseFrom[index]);
        if (numArray[index] > num)
          num = numArray[index];
      }
      List<Skills.IDs> idsList = new List<Skills.IDs>(count);
      for (int index = 0; index < count; ++index)
      {
        if (numArray[index] == num)
          idsList.Add(chooseFrom[index]);
      }
      return new Skills.IDs?(idsList[this.m_Rules.Roll(0, idsList.Count)]);
    }

    private int NPCSkillUtility(Actor actor, Skills.IDs skID)
    {
      if (actor.Model.Abilities.IsUndead)
      {
        switch (skID)
        {
          case Skills.IDs._FIRST_UNDEAD:
          case Skills.IDs.Z_STRONG:
          case Skills.IDs.Z_TOUGH:
          case Skills.IDs.Z_TRACKER:
            return 2;
          case Skills.IDs.Z_EATER:
          case Skills.IDs.Z_LIGHT_FEET:
            return 1;
          case Skills.IDs.Z_GRAB:
          case Skills.IDs.Z_INFECTOR:
          case Skills.IDs.Z_LIGHT_EATER:
            return 3;
          default:
            return 0;
        }
      }
      else
      {
        switch (skID)
        {
          case Skills.IDs._FIRST:
            return 2;
          case Skills.IDs.AWAKE:
            return !actor.Model.Abilities.HasToSleep ? 0 : 3;
          case Skills.IDs.BOWS:
            if (actor.Inventory != null)
            {
              foreach (Item obj in actor.Inventory.Items)
              {
                if (obj is ItemRangedWeapon && (obj.Model as ItemRangedWeaponModel).IsBow)
                  return 3;
              }
            }
            return 0;
          case Skills.IDs.CARPENTRY:
            return 1;
          case Skills.IDs.CHARISMATIC:
            return actor.CountFollowers <= 0 ? 0 : 1;
          case Skills.IDs.FIREARMS:
            if (actor.Inventory != null)
            {
              foreach (Item obj in actor.Inventory.Items)
              {
                if (obj is ItemRangedWeapon && (obj.Model as ItemRangedWeaponModel).IsFireArm)
                  return 3;
              }
            }
            return 0;
          case Skills.IDs.HARDY:
            return !actor.Model.Abilities.HasToSleep ? 0 : 3;
          case Skills.IDs.HAULER:
            return 3;
          case Skills.IDs.HIGH_STAMINA:
            return 2;
          case Skills.IDs.LEADERSHIP:
            return !actor.HasLeader ? 1 : 0;
          case Skills.IDs.LIGHT_EATER:
            return !actor.Model.Abilities.HasToEat ? 0 : 3;
          case Skills.IDs.LIGHT_FEET:
            return 2;
          case Skills.IDs.LIGHT_SLEEPER:
            return !actor.Model.Abilities.HasToSleep ? 0 : 2;
          case Skills.IDs.MARTIAL_ARTS:
            if (actor.Inventory != null)
            {
              foreach (Item obj in actor.Inventory.Items)
              {
                if (obj is ItemWeapon)
                  return 1;
              }
            }
            return 2;
          case Skills.IDs.MEDIC:
            return 1;
          case Skills.IDs.NECROLOGY:
            return 0;
          case Skills.IDs.STRONG:
            return 2;
          case Skills.IDs.STRONG_PSYCHE:
            return !actor.Model.Abilities.HasSanity ? 0 : 3;
          case Skills.IDs.TOUGH:
            return 3;
          case Skills.IDs.UNSUSPICIOUS:
            return actor.MurdersCounter <= 0 || actor.Model.Abilities.IsLawEnforcer ? 0 : 1;
          default:
            return 0;
        }
      }
    }

    private Skills.IDs? RollRandomSkillToUpgrade(Actor actor, int maxTries)
    {
      int num = 0;
      bool isUndead = actor.Model.Abilities.IsUndead;
      int id;
      do
      {
        ++num;
        id = isUndead ? (int) Skills.RollUndead(this.Rules.DiceRoller) : (int) Skills.RollLiving(this.Rules.DiceRoller);
      }
      while (actor.Sheet.SkillTable.GetSkillLevel(id) >= Skills.MaxSkillLevel(id) && num < maxTries);
      if (num >= maxTries)
        return new Skills.IDs?();
      return new Skills.IDs?((Skills.IDs) id);
    }

    private void DoLooseRandomSkill(Actor actor)
    {
      int[] skillsList = actor.Sheet.SkillTable.SkillsList;
      if (skillsList == null)
        return;
      int index = this.m_Rules.Roll(0, skillsList.Length);
      Skills.IDs id = (Skills.IDs) skillsList[index];
      actor.Sheet.SkillTable.DecOrRemoveSkill((int) id);
      if (!this.IsVisibleToPlayer(actor))
        return;
      this.AddMessage(this.MakeMessage(actor, string.Format("regressed in {0}!", (object) Skills.Name(id))));
    }

    public Skill SkillUpgrade(Actor actor, Skills.IDs id)
    {
      actor.Sheet.SkillTable.AddOrIncreaseSkill((int) id);
      Skill skill = actor.Sheet.SkillTable.GetSkill((int) id);
      this.OnSkillUpgrade(actor, id);
      return skill;
    }

    public void OnSkillUpgrade(Actor actor, Skills.IDs id)
    {
      if (id != Skills.IDs.HAULER || actor.Inventory == null)
        return;
      actor.Inventory.MaxCapacity = this.m_Rules.ActorMaxInv(actor);
    }

    private void CheckWeatherChange()
    {
      if (this.m_Rules.RollChance(33))
      {
        Weather weather;
        string text;
        switch (this.m_Session.World.Weather)
        {
          case Weather._FIRST:
            weather = Weather.CLOUDY;
            text = "Clouds are hiding the sky.";
            break;
          case Weather.CLOUDY:
            if (this.m_Rules.RollChance(50))
            {
              weather = Weather._FIRST;
              text = "The sky is clear again.";
              break;
            }
            weather = Weather.RAIN;
            text = "Rain is starting to fall.";
            break;
          case Weather.RAIN:
            if (this.m_Rules.RollChance(50))
            {
              weather = Weather.CLOUDY;
              text = "The rain has stopped.";
              break;
            }
            weather = Weather.HEAVY_RAIN;
            text = "The weather is getting worse!";
            break;
          case Weather.HEAVY_RAIN:
            weather = Weather.RAIN;
            text = "The rain is less heavy.";
            break;
          default:
            throw new ArgumentOutOfRangeException("unhandled weather");
        }
        this.m_Session.World.Weather = weather;
        this.AddMessage(new djack.RogueSurvivor.Data.Message(text, this.m_Session.WorldTime.TurnCounter, Color.White));
        this.m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, string.Format("The weather changed to {0}.", (object) this.DescribeWeather(this.m_Session.World.Weather)));
      }
      else
        this.AddMessage(new djack.RogueSurvivor.Data.Message("The weather stays the same.", this.m_Session.WorldTime.TurnCounter, Color.White));
    }

    private void PlayerKill(Actor victim)
    {
      this.m_Session.Scoring.AddKill(this.m_Player, victim, this.m_Session.WorldTime.TurnCounter);
    }

    private void InfectActor(Actor actor, int addInfection)
    {
      actor.Infection = Math.Min(this.m_Rules.ActorInfectionHPs(actor), actor.Infection + addInfection);
    }

    private Actor Zombify(Actor zombifier, Actor deadVictim, bool isStartingGame)
    {
      Actor actor = this.m_TownGenerator.MakeZombified(zombifier, deadVictim, isStartingGame ? 0 : deadVictim.Location.Map.LocalTime.TurnCounter);
      if (!isStartingGame)
        deadVictim.Location.Map.PlaceActorAt(actor, deadVictim.Location.Position);
      actor.ActionPoints = 0;
      if (deadVictim == this.m_Player || deadVictim.IsPlayer)
        this.m_Session.Scoring.SetZombifiedPlayer(actor);
      SkillTable skillTable = deadVictim.Sheet.SkillTable;
      if (skillTable != null && skillTable.CountSkills > 0)
      {
        if (actor.Sheet.SkillTable == null)
          actor.Sheet.SkillTable = new SkillTable();
        int countSkills = skillTable.CountSkills;
        int num = skillTable.CountTotalSkillLevels / 2;
        for (int index = 0; index < num; ++index)
        {
          Skills.IDs? nullable = this.ZombifySkill((Skills.IDs) skillTable.SkillsList[this.m_Rules.Roll(0, countSkills)]);
          if (nullable.HasValue)
            this.SkillUpgrade(actor, nullable.Value);
        }
        this.m_TownGenerator.RecomputeActorStartingStats(actor);
      }
      if (!isStartingGame)
        this.SeeingCauseInsanity(actor, actor.Location, Rules.SANITY_HIT_ZOMBIFY, string.Format("{0} turning into a zombie", (object) deadVictim.Name));
      return actor;
    }

    public Skills.IDs? ZombifySkill(Skills.IDs skill)
    {
      switch (skill)
      {
        case Skills.IDs._FIRST:
          return new Skills.IDs?(Skills.IDs._FIRST_UNDEAD);
        case Skills.IDs.LIGHT_EATER:
          return new Skills.IDs?(Skills.IDs.Z_LIGHT_EATER);
        case Skills.IDs.LIGHT_FEET:
          return new Skills.IDs?(Skills.IDs.Z_LIGHT_FEET);
        case Skills.IDs.MEDIC:
          return new Skills.IDs?(Skills.IDs.Z_INFECTOR);
        case Skills.IDs.STRONG:
          return new Skills.IDs?(Skills.IDs.Z_STRONG);
        case Skills.IDs.TOUGH:
          return new Skills.IDs?(Skills.IDs.Z_TOUGH);
        default:
          return new Skills.IDs?();
      }
    }

    public void ApplyOnFire(MapObject mapObj)
    {
      mapObj.FireState = MapObject.Fire.ONFIRE;
      --mapObj.JumpLevel;
    }

    public void UnapplyOnFire(MapObject mapObj)
    {
      ++mapObj.JumpLevel;
      mapObj.FireState = MapObject.Fire.BURNABLE;
    }

    public void ComputeViewRect(Point mapCenter)
    {
      int x = mapCenter.X - 10;
      int num1 = mapCenter.X + 10;
      int y = mapCenter.Y - 10;
      int num2 = mapCenter.Y + 10;
      this.m_MapViewRect = new Rectangle(x, y, 1 + num1 - x, 1 + num2 - y);
    }

    public bool IsInViewRect(Point mapPosition)
    {
      return this.m_MapViewRect.Contains(mapPosition);
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public void RedrawPlayScreen()
    {
            lock (m_UI) {
                m_UI.UI_Clear(Color.Black);
                Color white = Color.White;
                m_UI.UI_DrawLine(Color.DarkGray, 676, 0, 676, 676);
                DrawMap(m_Session.CurrentMap, white);
                m_UI.UI_DrawLine(Color.DarkGray, 676, 471, 1024, 471);
                DrawMiniMap(m_Session.CurrentMap);
                m_UI.UI_DrawLine(Color.DarkGray, 4, 675, 1024, 675);
                DrawMessages();
                m_UI.UI_DrawLine(Color.DarkGray, 676, 676, 676, 768);
                m_UI.UI_DrawString(Color.White, m_Session.CurrentMap.Name, 680, 680, new Color?());
                m_UI.UI_DrawString(Color.White, LocationText(m_Session.CurrentMap, m_Player), 680, 692, new Color?());
                m_UI.UI_DrawString(Color.White, string.Format("Day  {0}", (object)m_Session.WorldTime.Day), 680, 704, new Color?());
                m_UI.UI_DrawString(Color.White, string.Format("Hour {0}", (object)m_Session.WorldTime.Hour), 680, 716, new Color?());
                m_UI.UI_DrawString(m_Session.WorldTime.IsNight ? NIGHT_COLOR : DAY_COLOR, DescribeDayPhase(m_Session.WorldTime.Phase), 808, 704, new Color?());
                Color color;
                string text;
                switch (m_Session.CurrentMap.Lighting)
                {
                    case Lighting._FIRST:
                        color = Color.Blue;
                        text = "Darkness";
                        break;
                    case Lighting.OUTSIDE:
                        color = WeatherColor(m_Session.World.Weather);
                        text = DescribeWeather(m_Session.World.Weather);
                        break;
                    case Lighting.LIT:
                        color = Color.Yellow;
                        text = "Lit";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("unhandled lighting");
                }
                m_UI.UI_DrawString(color, text, 808, 716, new Color?());
                m_UI.UI_DrawString(Color.White, string.Format("Turn {0}", (object)m_Session.WorldTime.TurnCounter), 680, 728, new Color?());
                m_UI.UI_DrawString(Color.White, string.Format("Score   {0}@{1}% {2}", (object)m_Session.Scoring.TotalPoints, (object)(int)(100.0 * (double)Scoring.ComputeDifficultyRating(RogueGame.s_Options, m_Session.Scoring.Side, m_Session.Scoring.ReincarnationNumber)), (object)Session.DescShortGameMode(m_Session.GameMode)), 808, 728, new Color?());
                m_UI.UI_DrawString(Color.White, string.Format("Avatar  {0}/{1}", (object)(1 + m_Session.Scoring.ReincarnationNumber), (object)(1 + RogueGame.s_Options.MaxReincarnations)), 808, 740, new Color?());
                if (m_Player.MurdersCounter > 0)
                    m_UI.UI_DrawString(Color.White, string.Format("Murders {0}", (object)m_Player.MurdersCounter), 808, 752, new Color?());
                if (m_Player != null)
                    DrawActorStatus(m_Player, 680, 4);
                if (m_Player != null)
                {
                    if (m_Player.Inventory != null && m_Player.Model.Abilities.HasInventory)
                        DrawInventory(m_Player.Inventory, "Inventory", true, 10, m_Player.Inventory.MaxCapacity, INVENTORYPANEL_X, INVENTORYPANEL_Y);
                    DrawInventory(m_Player.Location.Map.GetItemsAt(m_Player.Location.Position), "Items on ground", true, 10, 10, INVENTORYPANEL_X, GROUNDINVENTORYPANEL_Y);
                    DrawCorpsesList(m_Player.Location.Map.GetCorpsesAt(m_Player.Location.Position), "Corpses on ground", 10, INVENTORYPANEL_X, CORPSESPANEL_Y);
                }
                if (m_Player != null && m_Player.Sheet.SkillTable != null && m_Player.Sheet.SkillTable.CountSkills > 0)
                    DrawActorSkillTable(m_Player, 680, 352);
                lock (m_Overlays) {
                    foreach (RogueGame.Overlay mOverlay in m_Overlays)
                        mOverlay.Draw(m_UI);
                }
                m_UI.UI_Repaint();
            };  // lock(m_UI)
    }

    private string LocationText(Map map, Actor actor)
    {
      if (map == null || actor == null)
        return "";
      StringBuilder stringBuilder = new StringBuilder(string.Format("({0},{1}) ", (object) actor.Location.Position.X, (object) actor.Location.Position.Y));
      List<Zone> zonesAt = map.GetZonesAt(actor.Location.Position.X, actor.Location.Position.Y);
      if (zonesAt == null || zonesAt.Count == 0)
        return stringBuilder.ToString();
      foreach (Zone zone in zonesAt)
        stringBuilder.Append(string.Format("{0} ", (object) zone.Name));
      return stringBuilder.ToString();
    }

    private Color TintForDayPhase(DayPhase phase)
    {
      switch (phase)
      {
        case DayPhase.SUNSET:
          return this.TINT_SUNSET;
        case DayPhase.EVENING:
          return this.TINT_EVENING;
        case DayPhase.MIDNIGHT:
          return this.TINT_MIDNIGHT;
        case DayPhase.DEEP_NIGHT:
          return this.TINT_NIGHT;
        case DayPhase.SUNRISE:
          return this.TINT_SUNRISE;
        case DayPhase.MORNING:
        case DayPhase.MIDDAY:
        case DayPhase.AFTERNOON:
          return this.TINT_DAY;
        default:
          throw new ArgumentOutOfRangeException("unhandled dayphase");
      }
    }

    public void DrawMap(Map map, Color tint)
    {
      int num1 = Math.Max(-1, this.m_MapViewRect.Left);
      int num2 = Math.Min(map.Width + 1, this.m_MapViewRect.Right);
      int num3 = Math.Max(-1, this.m_MapViewRect.Top);
      int num4 = Math.Min(map.Height + 1, this.m_MapViewRect.Bottom);
      string imageID;
      switch (this.m_Session.World.Weather)
      {
        case Weather.RAIN:
          imageID = this.m_Session.WorldTime.TurnCounter % 2 == 0 ? "weather_rain1" : "weather_rain2";
          break;
        case Weather.HEAVY_RAIN:
          imageID = this.m_Session.WorldTime.TurnCounter % 2 == 0 ? "weather_heavy_rain1" : "weather_heavy_rain2";
          break;
        default:
          imageID = (string) null;
          break;
      }
      Point point = new Point();
      bool isUndead = this.m_Player.Model.Abilities.IsUndead;
      bool flag1 = (double) this.m_Player.Model.StartingSheet.BaseSmellRating > 0.0;
      int num5 = this.m_Rules.ActorSmellThreshold(this.m_Player);
      for (int x = num1; x < num2; ++x)
      {
        point.X = x;
        for (int y = num3; y < num4; ++y)
        {
          point.Y = y;
          Point screen = this.MapToScreen(x, y);
          bool player = this.IsVisibleToPlayer(map, point);
          bool flag2 = false;
          Tile tile = map.IsInBounds(x, y) ? map.GetTileAt(x, y) : (Tile) null;
          if (map.IsInBounds(x, y))
            this.DrawTile(tile, screen, tint);
          else if (map.IsMapBoundary(x, y) && map.GetExitAt(point) != null)
            this.DrawExit(screen);
          if (player)
          {
            List<Corpse> corpsesAt = map.GetCorpsesAt(x, y);
            if (corpsesAt != null)
            {
              foreach (Corpse c in corpsesAt)
                this.DrawCorpse(c, screen.X, screen.Y, tint);
            }
          }
          if (RogueGame.s_Options.ShowPlayerTargets && !this.m_Player.IsSleeping && this.m_Player.Location.Position == point)
            this.DrawPlayerActorTargets(this.m_Player);
          MapObject mapObjectAt = map.GetMapObjectAt(x, y);
          if (mapObjectAt != null)
          {
            this.DrawMapObject(mapObjectAt, screen, tint);
            flag2 = true;
          }
          if (!this.m_Player.IsSleeping && map.IsInBounds(x, y) && this.m_Rules.GridDistance(this.m_Player.Location.Position, point) <= 1)
          {
            if (isUndead)
            {
              if (flag1)
              {
                int scentByOdorAt1 = map.GetScentByOdorAt(Odor.LIVING, point);
                if (scentByOdorAt1 >= num5)
                {
                  float num6 = (float) (0.899999976158142 * (double) scentByOdorAt1 / 270.0);
                  this.m_UI.UI_DrawTransparentImage(num6 * num6, "Icons\\scent_living", screen.X, screen.Y);
                }
                int scentByOdorAt2 = map.GetScentByOdorAt(Odor.UNDEAD_MASTER, point);
                if (scentByOdorAt2 >= num5)
                {
                  float num6 = (float) (0.899999976158142 * (double) scentByOdorAt2 / 270.0);
                  this.m_UI.UI_DrawTransparentImage(num6 * num6, "Icons\\scent_zm", screen.X, screen.Y);
                }
              }
            }
            else
            {
              int scentByOdorAt = map.GetScentByOdorAt(Odor.PERFUME_LIVING_SUPRESSOR, point);
              if (scentByOdorAt > 0)
                this.m_UI.UI_DrawTransparentImage((float) (0.899999976158142 * (double) scentByOdorAt / 270.0), "Icons\\scent_living_supressor", screen.X, screen.Y);
            }
          }
          if (player)
          {
            Inventory itemsAt = map.GetItemsAt(x, y);
            if (itemsAt != null)
            {
              this.DrawItemsStack(itemsAt, screen.X, screen.Y, tint);
              flag2 = true;
            }
            Actor actorAt = map.GetActorAt(x, y);
            if (actorAt != null)
            {
              this.DrawActorSprite(actorAt, screen, tint);
              flag2 = true;
            }
          }
          if (tile != null && tile.HasDecorations)
            flag2 = true;
          if (flag2 && tile.Model.IsWater)
            this.DrawTileWaterCover(tile, screen, tint);
          if (player && imageID != null && (tile != null && !tile.IsInside))
            this.m_UI.UI_DrawImage(imageID, screen.X, screen.Y);
        }
      }
    }

    private string MovingWaterImage(TileModel model, int turnCount)
    {
      if (model != this.m_GameTiles.FLOOR_SEWER_WATER)
        return (string) null;
      switch (turnCount % 3)
      {
        case 0:
          return "Tiles\\floor_sewer_water_anim1";
        case 1:
          return "Tiles\\floor_sewer_water_anim2";
        default:
          return "Tiles\\floor_sewer_water_anim3";
      }
    }

    public void DrawTile(Tile tile, Point screen, Color tint)
    {
      if (tile.IsInView)
      {
        this.m_UI.UI_DrawImage(tile.Model.ImageID, screen.X, screen.Y, tint);
        string imageID = this.MovingWaterImage(tile.Model, this.m_Session.WorldTime.TurnCounter);
        if (imageID != null)
          this.m_UI.UI_DrawImage(imageID, screen.X, screen.Y, tint);
        if (!tile.HasDecorations)
          return;
        foreach (string decoration in tile.Decorations)
          this.m_UI.UI_DrawImage(decoration, screen.X, screen.Y, tint);
      }
      else
      {
        if (!tile.IsVisited || this.IsPlayerSleeping())
          return;
        this.m_UI.UI_DrawGrayLevelImage(tile.Model.ImageID, screen.X, screen.Y);
        string imageID = this.MovingWaterImage(tile.Model, this.m_Session.WorldTime.TurnCounter);
        if (imageID != null)
          this.m_UI.UI_DrawGrayLevelImage(imageID, screen.X, screen.Y);
        if (!tile.HasDecorations)
          return;
        foreach (string decoration in tile.Decorations)
          this.m_UI.UI_DrawGrayLevelImage(decoration, screen.X, screen.Y);
      }
    }

    public void DrawTileWaterCover(Tile tile, Point screen, Color tint)
    {
      if (tile.IsInView)
      {
        this.m_UI.UI_DrawImage(tile.Model.WaterCoverImageID, screen.X, screen.Y, tint);
      }
      else
      {
        if (!tile.IsVisited || this.IsPlayerSleeping())
          return;
        this.m_UI.UI_DrawGrayLevelImage(tile.Model.WaterCoverImageID, screen.X, screen.Y);
      }
    }

    public void DrawExit(Point screen)
    {
      this.m_UI.UI_DrawImage("map_exit", screen.X, screen.Y);
    }

    public void DrawTileRectangle(Point mapPosition, Color color)
    {
      this.m_UI.UI_DrawRect(color, new Rectangle(this.MapToScreen(mapPosition), new Size(32, 32)));
    }

    public void DrawMapObject(MapObject mapObj, Point screen, Color tint)
    {
      if (mapObj.IsMovable && mapObj.Location.Map.GetTileAt(mapObj.Location.Position.X, mapObj.Location.Position.Y).Model.IsWater)
      {
        int num = (mapObj.Location.Position.X + this.m_Session.WorldTime.TurnCounter) % 2 == 0 ? -2 : 0;
        screen.Y -= num;
      }
      if (this.IsVisibleToPlayer(mapObj))
      {
        this.DrawMapObject(mapObj, screen, mapObj.ImageID, (Action<string, int, int>) ((imageID, gx, gy) => this.m_UI.UI_DrawImage(imageID, gx, gy, tint)));
        if (mapObj.HitPoints < mapObj.MaxHitPoints && mapObj.HitPoints > 0)
          this.DrawMapHealthBar(mapObj.HitPoints, mapObj.MaxHitPoints, screen.X, screen.Y);
        DoorWindow doorWindow = mapObj as DoorWindow;
        if (doorWindow == null || doorWindow.BarricadePoints <= 0)
          return;
        this.DrawMapHealthBar(doorWindow.BarricadePoints, 80, screen.X, screen.Y, Color.Green);
        this.m_UI.UI_DrawImage("Effects\\barricaded", screen.X, screen.Y, tint);
      }
      else
      {
        if (!this.IsKnownToPlayer(mapObj) || this.IsPlayerSleeping())
          return;
        this.DrawMapObject(mapObj, screen, mapObj.HiddenImageID, (Action<string, int, int>) ((imageID, gx, gy) => this.m_UI.UI_DrawGrayLevelImage(imageID, gx, gy)));
      }
    }

    private void DrawMapObject(MapObject mapObj, Point screen, string imageID, Action<string, int, int> drawFn)
    {
      drawFn(imageID, screen.X, screen.Y);
      if (!mapObj.IsOnFire)
        return;
      drawFn("Effects\\onFire", screen.X, screen.Y);
    }

    public void DrawActorSprite(Actor actor, Point screen, Color tint)
    {
      int x = screen.X;
      int y = screen.Y;
      if (actor.Leader != null && actor.Leader == this.m_Player)
      {
        if (this.m_Rules.HasActorBondWith(actor, this.m_Player))
          this.m_UI.UI_DrawImage("Actors\\player_follower_bond", x, y, tint);
        else if (this.m_Rules.IsActorTrustingLeader(actor))
          this.m_UI.UI_DrawImage("Actors\\player_follower_trust", x, y, tint);
        else
          this.m_UI.UI_DrawImage("Actors\\player_follower", x, y, tint);
      }
      int gx1 = x;
      int gy1 = y;
      if (actor.Model.ImageID != null)
        this.m_UI.UI_DrawImage(actor.Model.ImageID, gx1, gy1, tint);
      this.DrawActorDecoration(actor, gx1, gy1, DollPart.SKIN, tint);
      this.DrawActorDecoration(actor, gx1, gy1, DollPart.FEET, tint);
      this.DrawActorDecoration(actor, gx1, gy1, DollPart.LEGS, tint);
      this.DrawActorDecoration(actor, gx1, gy1, DollPart.TORSO, tint);
      this.DrawActorDecoration(actor, gx1, gy1, DollPart.TORSO, tint);
      if (actor.GetEquippedItem(DollPart.TORSO) != null)
        this.DrawActorEquipment(actor, gx1, gy1, DollPart.TORSO, tint);
      this.DrawActorDecoration(actor, gx1, gy1, DollPart.EYES, tint);
      this.DrawActorDecoration(actor, gx1, gy1, DollPart.HEAD, tint);
      this.DrawActorEquipment(actor, gx1, gy1, DollPart.LEFT_HAND, tint);
      this.DrawActorEquipment(actor, gx1, gy1, DollPart._FIRST, tint);
      int gx2 = gx1;
      int gy2 = gy1;
      if (this.m_Player != null)
      {
        bool flag1 = this.m_Player.IsSelfDefenceFrom(actor);
        bool flag2 = this.m_Player.IsAggressorOf(actor);
        bool flag3 = this.m_Player.AreIndirectEnemies(actor);
        if (flag1)
          this.m_UI.UI_DrawImage("Icons\\enemy_you_self_defence", gx2, gy2, tint);
        else if (flag2)
          this.m_UI.UI_DrawImage("Icons\\enemy_you_aggressor", gx2, gy2, tint);
        else if (flag3)
          this.m_UI.UI_DrawImage("Icons\\enemy_indirect", gx2, gy2, tint);
      }
      switch (actor.Activity)
      {
        case Activity.IDLE:
          int maxHitPoints = this.m_Rules.ActorMaxHPs(actor);
          if (actor.HitPoints < maxHitPoints)
            this.DrawMapHealthBar(actor.HitPoints, maxHitPoints, gx2, gy2);
          if (actor.IsRunning)
            this.m_UI.UI_DrawImage("Icons\\running", gx2, gy2, tint);
          else if (actor.Model.Abilities.CanRun && !this.m_Rules.CanActorRun(actor))
            this.m_UI.UI_DrawImage("Icons\\cant_run", gx2, gy2, tint);
          if (actor.Model.Abilities.HasToSleep)
          {
            if (this.m_Rules.IsActorExhausted(actor))
              this.m_UI.UI_DrawImage("Icons\\sleep_exhausted", gx2, gy2, tint);
            else if (this.m_Rules.IsActorSleepy(actor))
              this.m_UI.UI_DrawImage("Icons\\sleep_sleepy", gx2, gy2, tint);
            else if (this.m_Rules.IsAlmostSleepy(actor))
              this.m_UI.UI_DrawImage("Icons\\sleep_almost_sleepy", gx2, gy2, tint);
          }
          if (actor.Model.Abilities.HasToEat)
          {
            if (this.m_Rules.IsActorStarving(actor))
              this.m_UI.UI_DrawImage("Icons\\food_starving", gx2, gy2, tint);
            else if (this.m_Rules.IsActorHungry(actor))
              this.m_UI.UI_DrawImage("Icons\\food_hungry", gx2, gy2, tint);
            else if (this.IsAlmostHungry(actor))
              this.m_UI.UI_DrawImage("Icons\\food_almost_hungry", gx2, gy2, tint);
          }
          else if (actor.Model.Abilities.IsRotting)
          {
            if (this.m_Rules.IsRottingActorStarving(actor))
              this.m_UI.UI_DrawImage("Icons\\rot_starving", gx2, gy2, tint);
            else if (this.m_Rules.IsRottingActorHungry(actor))
              this.m_UI.UI_DrawImage("Icons\\rot_hungry", gx2, gy2, tint);
            else if (this.IsAlmostRotHungry(actor))
              this.m_UI.UI_DrawImage("Icons\\rot_almost_hungry", gx2, gy2, tint);
          }
          if (actor.Model.Abilities.HasSanity)
          {
            if (this.m_Rules.IsActorInsane(actor))
              this.m_UI.UI_DrawImage("Icons\\sanity_insane", gx2, gy2, tint);
            else if (this.m_Rules.IsActorDisturbed(actor))
              this.m_UI.UI_DrawImage("Icons\\sanity_disturbed", gx2, gy2, tint);
          }
          if (this.m_Player != null && this.m_Rules.CanActorInitiateTradeWith(this.m_Player, actor))
            this.m_UI.UI_DrawImage("Icons\\can_trade", gx2, gy2, tint);
          if (actor.IsSleeping && (this.m_Rules.IsOnCouch(actor) || this.m_Rules.ActorHealChanceBonus(actor) > 0))
            this.m_UI.UI_DrawImage("Icons\\healing", gx2, gy2, tint);
          if (actor.CountFollowers > 0)
            this.m_UI.UI_DrawImage("Icons\\leader", gx2, gy2, tint);
          if (!RogueGame.s_Options.IsCombatAssistantOn || actor == this.m_Player || (this.m_Player == null || !this.m_Rules.IsEnemyOf(actor, this.m_Player)))
            break;
          if (this.m_Rules.WillActorActAgainBefore(this.m_Player, actor))
          {
            this.m_UI.UI_DrawImage("Icons\\threat_safe", gx2, gy2, tint);
            break;
          }
          if (this.m_Rules.WillOtherActTwiceBefore(this.m_Player, actor))
          {
            this.m_UI.UI_DrawImage("Icons\\threat_high_danger", gx2, gy2, tint);
            break;
          }
          this.m_UI.UI_DrawImage("Icons\\threat_danger", gx2, gy2, tint);
          break;
        case Activity.CHASING:
        case Activity.FIGHTING:
          if (!actor.IsPlayer && actor.TargetActor != null)
          {
            if (actor.TargetActor != null && actor.TargetActor == this.m_Player)
            {
              this.m_UI.UI_DrawImage("Activities\\chasing_player", gx2, gy2, tint);
              goto case Activity.IDLE;
            }
            else
            {
              this.m_UI.UI_DrawImage("Activities\\chasing", gx2, gy2, tint);
              goto case Activity.IDLE;
            }
          }
          else
            goto case Activity.IDLE;
        case Activity.TRACKING:
          if (!actor.IsPlayer)
          {
            this.m_UI.UI_DrawImage("Activities\\tracking", gx2, gy2, tint);
            goto case Activity.IDLE;
          }
          else
            goto case Activity.IDLE;
        case Activity.FLEEING:
          if (!actor.IsPlayer)
          {
            this.m_UI.UI_DrawImage("Activities\\fleeing", gx2, gy2, tint);
            goto case Activity.IDLE;
          }
          else
            goto case Activity.IDLE;
        case Activity.FOLLOWING:
          if (!actor.IsPlayer && actor.TargetActor != null)
          {
            if (actor.TargetActor.IsPlayer)
            {
              this.m_UI.UI_DrawImage("Activities\\following_player", gx2, gy2);
              goto case Activity.IDLE;
            }
            else
            {
              this.m_UI.UI_DrawImage("Activities\\following", gx2, gy2);
              goto case Activity.IDLE;
            }
          }
          else
            goto case Activity.IDLE;
        case Activity.SLEEPING:
          this.m_UI.UI_DrawImage("Activities\\sleeping", gx2, gy2);
          goto case Activity.IDLE;
        case Activity.FOLLOWING_ORDER:
          this.m_UI.UI_DrawImage("Activities\\following_order", gx2, gy2);
          goto case Activity.IDLE;
        case Activity.FLEEING_FROM_EXPLOSIVE:
          if (!actor.IsPlayer)
          {
            this.m_UI.UI_DrawImage("Activities\\fleeing_explosive", gx2, gy2, tint);
            goto case Activity.IDLE;
          }
          else
            goto case Activity.IDLE;
        default:
          throw new InvalidOperationException("unhandled activity " + (object) actor.Activity);
      }
    }

    public void DrawActorDecoration(Actor actor, int gx, int gy, DollPart part, Color tint)
    {
      List<string> decorations = actor.Doll.GetDecorations(part);
      if (decorations == null)
        return;
      foreach (string imageID in decorations)
        this.m_UI.UI_DrawImage(imageID, gx, gy, tint);
    }

    public void DrawActorDecoration(Actor actor, int gx, int gy, DollPart part, float rotation, float scale)
    {
      List<string> decorations = actor.Doll.GetDecorations(part);
      if (decorations == null)
        return;
      foreach (string imageID in decorations)
        this.m_UI.UI_DrawImageTransform(imageID, gx, gy, rotation, scale);
    }

    public void DrawActorEquipment(Actor actor, int gx, int gy, DollPart part, Color tint)
    {
      Item equippedItem = actor.GetEquippedItem(part);
      if (equippedItem == null)
        return;
      this.m_UI.UI_DrawImage(equippedItem.ImageID, gx, gy, tint);
    }

    public void DrawCorpse(Corpse c, int gx, int gy, Color tint)
    {
      float rotation = c.Rotation;
      float scale = c.Scale;
      int num1 = 0;
      Actor deadGuy = c.DeadGuy;
      gx += num1;
      gy += num1;
      if (deadGuy.Model.ImageID != null)
        this.m_UI.UI_DrawImageTransform(deadGuy.Model.ImageID, gx, gy, rotation, scale);
      this.DrawActorDecoration(deadGuy, gx, gy, DollPart.SKIN, rotation, scale);
      this.DrawActorDecoration(deadGuy, gx, gy, DollPart.FEET, rotation, scale);
      this.DrawActorDecoration(deadGuy, gx, gy, DollPart.LEGS, rotation, scale);
      this.DrawActorDecoration(deadGuy, gx, gy, DollPart.TORSO, rotation, scale);
      this.DrawActorDecoration(deadGuy, gx, gy, DollPart.TORSO, rotation, scale);
      this.DrawActorDecoration(deadGuy, gx, gy, DollPart.EYES, rotation, scale);
      this.DrawActorDecoration(deadGuy, gx, gy, DollPart.HEAD, rotation, scale);
      gx -= num1;
      gy -= num1;
      int num2 = this.Rules.CorpseRotLevel(c);
      string str = (string) null;
      switch (num2)
      {
        case 0:
          if (str == null)
            break;
          string imageID = str + (object) (1 + this.m_Session.WorldTime.TurnCounter % 2);
          int num3 = this.m_Session.WorldTime.TurnCounter % 5 - 2;
          int num4 = this.m_Session.WorldTime.TurnCounter / 3 % 5 - 2;
          this.m_UI.UI_DrawImage(imageID, gx + num3, gy + num4);
          break;
        case 1:
        case 2:
        case 3:
        case 4:
        case 5:
          str = "rot" + (object) num2 + "_";
          goto case 0;
        default:
          throw new Exception("unhandled rot level");
      }
    }

    public void DrawCorpsesList(List<Corpse> list, string title, int slots, int gx, int gy)
    {
      int num2 = list == null ? 0 : list.Count;
      if (num2 > 0)
        title = title + " : " + (object) num2;
      gy -= 14;
      this.m_UI.UI_DrawStringBold(Color.White, title, gx, gy, new Color?());
      gy += 14;
      int gx1 = gx;
      int gy1 = gy;
      for (int index = 0; index < slots; ++index)
      {
        this.m_UI.UI_DrawImage("Items\\itemslot", gx1, gy1);
        gx1 += 32;
      }
      if (list == null)
        return;
      int gx2 = gx;
      int gy2 = gy;
      int num3 = 0;
      foreach (Corpse c in list)
      {
        if (c.IsDragged)
          this.m_UI.UI_DrawImage("corpse_dragged", gx2, gy2);
        this.DrawCorpse(c, gx2, gy2, Color.White);
        if (++num3 >= slots)
          break;
        gx2 += 32;
      }
    }

    public void DrawActorTargets(Actor actor)
    {
      Point point = new Point(16, 16);
      if (actor.TargetActor != null && !actor.TargetActor.IsDead && this.IsVisibleToPlayer(actor.TargetActor))
        this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(actor.TargetActor.Location.Position), "Icons\\is_target"));
      foreach (Actor actor1 in actor.Location.Map.Actors)
      {
        if (actor1 != actor && !actor1.IsDead && (this.IsVisibleToPlayer(actor1) && actor1.TargetActor == actor) && (actor1.Activity == Activity.CHASING || actor1.Activity == Activity.FIGHTING))
        {
          this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayImage(this.MapToScreen(actor.Location.Position), "Icons\\is_targetted"));
          break;
        }
      }
    }

    public void DrawPlayerActorTargets(Actor player)
    {
      Point point = new Point(16, 16);
      if (player.TargetActor != null && !player.TargetActor.IsDead && this.IsVisibleToPlayer(player.TargetActor))
      {
        Point screen = this.MapToScreen(player.TargetActor.Location.Position);
        this.m_UI.UI_DrawImage("Icons\\is_target", screen.X, screen.Y);
      }
      foreach (Actor actor in player.Location.Map.Actors)
      {
        if (actor != player && !actor.IsDead && (this.IsVisibleToPlayer(actor) && actor.TargetActor == player) && (actor.Activity == Activity.CHASING || actor.Activity == Activity.FIGHTING))
        {
          Point screen = this.MapToScreen(player.Location.Position);
          this.m_UI.UI_DrawImage("Icons\\is_targetted", screen.X, screen.Y);
          break;
        }
      }
    }

    public void DrawItemsStack(Inventory inventory, int gx, int gy, Color tint)
    {
      if (inventory == null)
        return;
      foreach (Item it in inventory.Items)
        this.DrawItem(it, gx, gy, tint);
    }

    public void DrawMapIcon(Point position, string imageID)
    {
      this.m_UI.UI_DrawImage(imageID, position.X * 32, position.Y * 32);
    }

    public void DrawMapHealthBar(int hitPoints, int maxHitPoints, int gx, int gy)
    {
      this.DrawMapHealthBar(hitPoints, maxHitPoints, gx, gy, Color.Red);
    }

    public void DrawMapHealthBar(int hitPoints, int maxHitPoints, int gx, int gy, Color barColor)
    {
      int x = gx + 4;
      int y = gy + 32 - 4;
      int width = (int) (20.0 * (double) hitPoints / (double) maxHitPoints);
      this.m_UI.UI_FillRect(Color.Black, new Rectangle(x, y, 20, 4));
      if (width <= 0)
        return;
      this.m_UI.UI_FillRect(barColor, new Rectangle(x + 1, y + 1, width, 2));
    }

    public void DrawBar(int value, int previousValue, int maxValue, int refValue, int maxWidth, int height, int gx, int gy, Color fillColor, Color lossFillColor, Color gainFillColor, Color emptyColor)
    {
      this.m_UI.UI_FillRect(emptyColor, new Rectangle(gx, gy, maxWidth, height));
      int width1 = (int) ((double) maxWidth * (double) previousValue / (double) maxValue);
      int width2 = (int) ((double) maxWidth * (double) value / (double) maxValue);
      if (value > previousValue)
      {
        if (width2 > 0)
          this.m_UI.UI_FillRect(gainFillColor, new Rectangle(gx, gy, width2, height));
        if (width1 > 0)
          this.m_UI.UI_FillRect(fillColor, new Rectangle(gx, gy, width1, height));
      }
      else if (value < previousValue)
      {
        if (width1 > 0)
          this.m_UI.UI_FillRect(lossFillColor, new Rectangle(gx, gy, width1, height));
        if (width2 > 0)
          this.m_UI.UI_FillRect(fillColor, new Rectangle(gx, gy, width2, height));
      }
      else if (width2 > 0)
        this.m_UI.UI_FillRect(fillColor, new Rectangle(gx, gy, width2, height));
      int num = (int) ((double) maxWidth * (double) refValue / (double) maxValue);
      this.m_UI.UI_DrawLine(Color.White, gx + num, gy, gx + num, gy + height);
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public void DrawMiniMap(Map map)
    {
      if (RogueGame.s_Options.IsMinimapOn)
        this.m_UI.UI_ClearMinimap(Color.Black);
      if (RogueGame.s_Options.IsMinimapOn)
      {
        Point pos = new Point();
        for (int x = 0; x < map.Width; ++x)
        {
          pos.X = x;
          for (int y = 0; y < map.Height; ++y)
          {
            pos.Y = y;
            Tile tileAt = map.GetTileAt(x, y);
            if (tileAt.IsVisited)
            {
              if (map.GetExitAt(pos) != null)
                this.m_UI.UI_SetMinimapColor(x, y, Color.HotPink);
              else
                this.m_UI.UI_SetMinimapColor(x, y, tileAt.Model.MinimapColor);
            }
          }
        }
      }
      if (RogueGame.s_Options.IsMinimapOn)
        m_UI.UI_DrawMinimap(MINIMAP_X, MINIMAP_Y);
      m_UI.UI_DrawRect(Color.White, new Rectangle(MINIMAP_X + m_MapViewRect.Left * 2, MINIMAP_Y + m_MapViewRect.Top * 2, m_MapViewRect.Width * 2, m_MapViewRect.Height * 2));
      if (RogueGame.s_Options.ShowPlayerTagsOnMinimap)
      {
        for (int x = 0; x < map.Width; ++x)
        {
          for (int y = 0; y < map.Height; ++y)
          {
            Tile tileAt = map.GetTileAt(x, y);
            if (tileAt.IsVisited)
            {
              string imageID = (string) null;
              if (tileAt.HasDecoration("Tiles\\Decoration\\player_tag"))
                imageID = "mini_player_tag";
              else if (tileAt.HasDecoration("Tiles\\Decoration\\player_tag2"))
                imageID = "mini_player_tag2";
              else if (tileAt.HasDecoration("Tiles\\Decoration\\player_tag3"))
                imageID = "mini_player_tag3";
              else if (tileAt.HasDecoration("Tiles\\Decoration\\player_tag4"))
                imageID = "mini_player_tag4";
              if (imageID != null)
              {
                Point point = new Point(MINIMAP_X + x * 2, MINIMAP_Y + y * 2);
                this.m_UI.UI_DrawImage(imageID, point.X - 1, point.Y - 1);
              }
            }
          }
        }
      }
      if (this.m_Player == null)
        return;
      if (!this.m_Player.IsSleeping)
      {
        ItemTracker itemTracker1 = this.m_Player.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;
        if (itemTracker1 != null && itemTracker1.Batteries > 0)
        {
          if (this.m_Player.CountFollowers > 0 && itemTracker1.CanTrackFollowersOrLeader)
          {
            foreach (Actor follower in this.m_Player.Followers)
            {
              if (follower.Location.Map == this.m_Player.Location.Map)
              {
                ItemTracker itemTracker2 = follower.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;
                if (itemTracker2 != null && itemTracker2.CanTrackFollowersOrLeader)
                {
                  Point point = new Point(MINIMAP_X + follower.Location.Position.X * 2, MINIMAP_Y + follower.Location.Position.Y * 2);
                  this.m_UI.UI_DrawImage("mini_follower_position", point.X - 1, point.Y - 1);
                  if (this.IsInViewRect(follower.Location.Position) && !this.IsVisibleToPlayer(follower))
                  {
                    Point screen = this.MapToScreen(follower.Location.Position);
                    this.m_UI.UI_DrawImage("track_follower_position", screen.X, screen.Y);
                  }
                }
              }
            }
          }
          if (itemTracker1.CanTrackUndeads)
          {
            foreach (Actor actor in map.Actors)
            {
              if (actor != this.m_Player && actor.Model.Abilities.IsUndead && actor.Location.Map == this.m_Player.Location.Map && this.m_Rules.GridDistance(actor.Location.Position, this.m_Player.Location.Position) <= 6)
              {
                Point point = new Point(MINIMAP_X + actor.Location.Position.X * 2, MINIMAP_Y + actor.Location.Position.Y * 2);
                this.m_UI.UI_DrawImage("mini_undead_position", point.X - 1, point.Y - 1);
                if (this.IsInViewRect(actor.Location.Position) && !this.IsVisibleToPlayer(actor))
                {
                  Point screen = this.MapToScreen(actor.Location.Position);
                  this.m_UI.UI_DrawImage("track_undead_position", screen.X, screen.Y);
                }
              }
            }
          }
          if (itemTracker1.CanTrackBlackOps)
          {
            foreach (Actor actor in map.Actors)
            {
              if (actor != this.m_Player && actor.Faction == this.GameFactions.TheBlackOps && actor.Location.Map == this.m_Player.Location.Map)
              {
                Point point = new Point(MINIMAP_X + actor.Location.Position.X * 2, MINIMAP_Y + actor.Location.Position.Y * 2);
                this.m_UI.UI_DrawImage("mini_blackops_position", point.X - 1, point.Y - 1);
                if (this.IsInViewRect(actor.Location.Position) && !this.IsVisibleToPlayer(actor))
                {
                  Point screen = this.MapToScreen(actor.Location.Position);
                  this.m_UI.UI_DrawImage("track_blackops_position", screen.X, screen.Y);
                }
              }
            }
          }
          if (itemTracker1.CanTrackPolice)
          {
            foreach (Actor actor in map.Actors)
            {
              if (actor != this.m_Player && actor.Faction == this.GameFactions.ThePolice && actor.Location.Map == this.m_Player.Location.Map)
              {
                Point point = new Point(MINIMAP_X + actor.Location.Position.X * 2, MINIMAP_Y + actor.Location.Position.Y * 2);
                this.m_UI.UI_DrawImage("mini_police_position", point.X - 1, point.Y - 1);
                if (this.IsInViewRect(actor.Location.Position) && !this.IsVisibleToPlayer(actor))
                {
                  Point screen = this.MapToScreen(actor.Location.Position);
                  this.m_UI.UI_DrawImage("track_police_position", screen.X, screen.Y);
                }
              }
            }
          }
        }
      }
      Point position = m_Player.Location.Position;
      int x1 = MINIMAP_X + position.X * 2;
      int y1 = MINIMAP_Y + position.Y * 2;
      m_UI.UI_DrawImage("mini_player_position", x1 - 1, y1 - 1);
    }

    public void DrawActorStatus(Actor actor, int gx, int gy)
    {
      this.m_UI.UI_DrawStringBold(Color.White, string.Format("{0}, {1}", (object) actor.Name, (object) actor.Faction.MemberName), gx, gy, new Color?());
      gy += 14;
      int maxValue1 = this.m_Rules.ActorMaxHPs(actor);
      this.m_UI.UI_DrawStringBold(Color.White, string.Format("HP  {0}", (object) actor.HitPoints), gx, gy, new Color?());
      this.DrawBar(actor.HitPoints, actor.PreviousHitPoints, maxValue1, 0, 100, 14, gx + 70, gy, Color.Red, Color.DarkRed, Color.OrangeRed, Color.Gray);
      this.m_UI.UI_DrawStringBold(Color.White, string.Format("{0}", (object) maxValue1), gx + 84 + 100, gy, new Color?());
      gy += 14;
      if (actor.Model.Abilities.CanTire)
      {
        int maxValue2 = this.m_Rules.ActorMaxSTA(actor);
        this.m_UI.UI_DrawStringBold(Color.White, string.Format("STA {0}", (object) actor.StaminaPoints), gx, gy, new Color?());
        this.DrawBar(actor.StaminaPoints, actor.PreviousStaminaPoints, maxValue2, 10, 100, 14, gx + 70, gy, Color.Green, Color.DarkGreen, Color.LightGreen, Color.Gray);
        this.m_UI.UI_DrawStringBold(Color.White, string.Format("{0}", (object) maxValue2), gx + 84 + 100, gy, new Color?());
        if (actor.IsRunning)
          this.m_UI.UI_DrawStringBold(Color.LightGreen, "RUNNING!", gx + 126 + 100, gy, new Color?());
        else if (this.m_Rules.CanActorRun(actor))
          this.m_UI.UI_DrawStringBold(Color.Green, "can run", gx + 126 + 100, gy, new Color?());
        else if (this.m_Rules.IsActorTired(actor))
          this.m_UI.UI_DrawStringBold(Color.Gray, "TIRED", gx + 126 + 100, gy, new Color?());
      }
      gy += 14;
      if (actor.Model.Abilities.HasToEat)
      {
        int maxValue2 = this.m_Rules.ActorMaxFood(actor);
        this.m_UI.UI_DrawStringBold(Color.White, string.Format("FOO {0}", (object) actor.FoodPoints), gx, gy, new Color?());
        this.DrawBar(actor.FoodPoints, actor.PreviousFoodPoints, maxValue2, Rules.FOOD_HUNGRY_LEVEL, 100, 14, gx + 70, gy, Color.Chocolate, Color.Brown, Color.Beige, Color.Gray);
        this.m_UI.UI_DrawStringBold(Color.White, string.Format("{0}", (object) maxValue2), gx + 84 + 100, gy, new Color?());
        if (this.m_Rules.IsActorHungry(actor))
        {
          if (this.m_Rules.IsActorStarving(actor))
            this.m_UI.UI_DrawStringBold(Color.Red, "STARVING!", gx + 126 + 100, gy, new Color?());
          else
            this.m_UI.UI_DrawStringBold(Color.Yellow, "Hungry", gx + 126 + 100, gy, new Color?());
        }
        else
          this.m_UI.UI_DrawStringBold(Color.White, string.Format("{0}h", (object) this.FoodToHoursUntilHungry(actor.FoodPoints)), gx + 126 + 100, gy, new Color?());
      }
      else if (actor.Model.Abilities.IsRotting)
      {
        int maxValue2 = this.m_Rules.ActorMaxRot(actor);
        this.m_UI.UI_DrawStringBold(Color.White, string.Format("ROT {0}", (object) actor.FoodPoints), gx, gy, new Color?());
        this.DrawBar(actor.FoodPoints, actor.PreviousFoodPoints, maxValue2, Rules.ROT_HUNGRY_LEVEL, 100, 14, gx + 70, gy, Color.Chocolate, Color.Brown, Color.Beige, Color.Gray);
        this.m_UI.UI_DrawStringBold(Color.White, string.Format("{0}", (object) maxValue2), gx + 84 + 100, gy, new Color?());
        if (this.m_Rules.IsRottingActorHungry(actor))
        {
          if (this.m_Rules.IsRottingActorStarving(actor))
            this.m_UI.UI_DrawStringBold(Color.Red, "STARVING!", gx + 126 + 100, gy, new Color?());
          else
            this.m_UI.UI_DrawStringBold(Color.Yellow, "Hungry", gx + 126 + 100, gy, new Color?());
        }
        else
          this.m_UI.UI_DrawStringBold(Color.White, string.Format("{0}h", (object) this.FoodToHoursUntilRotHungry(actor.FoodPoints)), gx + 126 + 100, gy, new Color?());
      }
      gy += 14;
      if (actor.Model.Abilities.HasToSleep)
      {
        int maxValue2 = this.m_Rules.ActorMaxSleep(actor);
        this.m_UI.UI_DrawStringBold(Color.White, string.Format("SLP {0}", (object) actor.SleepPoints), gx, gy, new Color?());
        DrawBar(actor.SleepPoints, actor.PreviousSleepPoints, maxValue2, Rules.SLEEP_SLEEPY_LEVEL, 100, 14, gx + 70, gy, Color.Blue, Color.DarkBlue, Color.LightBlue, Color.Gray);
        this.m_UI.UI_DrawStringBold(Color.White, string.Format("{0}", (object) maxValue2), gx + 84 + 100, gy, new Color?());
        if (this.m_Rules.IsActorSleepy(actor))
        {
          if (this.m_Rules.IsActorExhausted(actor))
            this.m_UI.UI_DrawStringBold(Color.Red, "EXHAUSTED!", gx + 126 + 100, gy, new Color?());
          else
            this.m_UI.UI_DrawStringBold(Color.Yellow, "Sleepy", gx + 126 + 100, gy, new Color?());
        }
        else
          this.m_UI.UI_DrawStringBold(Color.White, string.Format("{0}h", (object) this.m_Rules.SleepToHoursUntilSleepy(actor.SleepPoints, this.m_Session.WorldTime.IsNight)), gx + 126 + 100, gy, new Color?());
      }
      gy += 14;
      if (actor.Model.Abilities.HasSanity)
      {
        int maxValue2 = this.m_Rules.ActorMaxSanity(actor);
        this.m_UI.UI_DrawStringBold(Color.White, string.Format("SAN {0}", (object) actor.Sanity), gx, gy, new Color?());
        this.DrawBar(actor.Sanity, actor.PreviousSanity, maxValue2, this.m_Rules.ActorDisturbedLevel(actor), 100, 14, gx + 70, gy, Color.Orange, Color.DarkOrange, Color.OrangeRed, Color.Gray);
        this.m_UI.UI_DrawStringBold(Color.White, string.Format("{0}", (object) maxValue2), gx + 84 + 100, gy, new Color?());
        if (this.m_Rules.IsActorDisturbed(actor))
        {
          if (this.m_Rules.IsActorInsane(actor))
            this.m_UI.UI_DrawStringBold(Color.Red, "INSANE!", gx + 126 + 100, gy, new Color?());
          else
            this.m_UI.UI_DrawStringBold(Color.Yellow, "Disturbed", gx + 126 + 100, gy, new Color?());
        }
        else
          this.m_UI.UI_DrawStringBold(Color.White, string.Format("{0}h", (object) this.m_Rules.SanityToHoursUntilUnstable(actor)), gx + 126 + 100, gy, new Color?());
      }
      if (Rules.HasInfection(this.m_Session.GameMode) && !actor.Model.Abilities.IsUndead)
      {
        int maxValue2 = this.m_Rules.ActorInfectionHPs(actor);
        int refValue = Rules.INFECTION_LEVEL_1_WEAK * maxValue2 / 100;
        gy += 14;
        this.m_UI.UI_DrawStringBold(Color.White, string.Format("INF {0}", (object) actor.Infection), gx, gy, new Color?());
        this.DrawBar(actor.Infection, actor.Infection, maxValue2, refValue, 100, 14, gx + 70, gy, Color.Purple, Color.Black, Color.Black, Color.Gray);
        this.m_UI.UI_DrawStringBold(Color.White, string.Format("{0}%", (object) this.m_Rules.ActorInfectionPercent(actor)), gx + 84 + 100, gy, new Color?());
      }
      gy += 14;
      Attack attack1 = this.m_Rules.ActorMeleeAttack(actor, actor.CurrentMeleeAttack, (Actor) null);
      int num1 = this.m_Rules.ActorDamageBonusVsUndeads(actor);
      this.m_UI.UI_DrawStringBold(Color.White, string.Format("Melee  Atk {0:D2}  Dmg {1:D2}/{2:D2}", (object) attack1.HitValue, (object) attack1.DamageValue, (object) (attack1.DamageValue + num1)), gx, gy, new Color?());
      gy += 14;
      Attack attack2 = this.m_Rules.ActorRangedAttack(actor, actor.CurrentRangedAttack, actor.CurrentRangedAttack.EfficientRange, (Actor) null);
      ItemRangedWeapon itemRangedWeapon = actor.GetEquippedWeapon() as ItemRangedWeapon;
      if (itemRangedWeapon != null)
      {
        int ammo = itemRangedWeapon.Ammo;
        int maxAmmo = (itemRangedWeapon.Model as ItemRangedWeaponModel).MaxAmmo;
        this.m_UI.UI_DrawStringBold(Color.White, string.Format("Ranged Atk {0:D2}  Dmg {1:D2}/{2:D2} Rng {3}-{4} Amo {5}/{6}", (object) attack2.HitValue, (object) attack2.DamageValue, (object) (attack2.DamageValue + num1), (object) attack2.Range, (object) attack2.EfficientRange, (object) ammo, (object) maxAmmo), gx, gy, new Color?());
      }
      gy += 14;
      Defence defence = this.m_Rules.ActorDefence(actor, actor.CurrentDefence);
      if (actor.Model.Abilities.IsUndead)
        this.m_UI.UI_DrawStringBold(Color.White, string.Format("Def {0:D2} Spd {1:F2} FoV {2} Sml {3:F2} Kills {4}", (object) defence.Value, (object) (float) ((double) this.m_Rules.ActorSpeed(actor) / 100.0), (object) this.m_Rules.ActorFOV(actor, this.m_Session.WorldTime, this.m_Session.World.Weather), (object) this.m_Rules.ActorSmell(actor), (object) actor.KillsCount), gx, gy, new Color?());
      else
        this.m_UI.UI_DrawStringBold(Color.White, string.Format("Def {0:D2} Arm {1:D1}/{2:D1} Spd {3:F2} FoV {4} Fol {5}/{6}", (object) defence.Value, (object) defence.Protection_Hit, (object) defence.Protection_Shot, (object) (float) ((double) this.m_Rules.ActorSpeed(actor) / 100.0), (object) this.m_Rules.ActorFOV(actor, this.m_Session.WorldTime, this.m_Session.World.Weather), (object) actor.CountFollowers, (object) this.m_Rules.ActorMaxFollowers(actor)), gx, gy, new Color?());
    }

    public void DrawInventory(Inventory inventory, string title, bool drawSlotsNumbers, int slotsPerLine, int maxSlots, int gx, int gy)
    {
      gy -= 14;
      this.m_UI.UI_DrawStringBold(Color.White, title, gx, gy, new Color?());
      gy += 14;
      int gx1 = gx;
      int gy1 = gy;
      int num1 = 0;
      for (int index = 0; index < maxSlots; ++index)
      {
        this.m_UI.UI_DrawImage("Items\\itemslot", gx1, gy1);
        if (++num1 >= slotsPerLine)
        {
          num1 = 0;
          gy1 += 32;
          gx1 = gx;
        }
        else
          gx1 += 32;
      }
      if (inventory == null)
        return;
      int gx2 = gx;
      int gy2 = gy;
      int num2 = 0;
      foreach (Item it in inventory.Items)
      {
        if (it.IsEquipped)
          this.m_UI.UI_DrawImage("Items\\itemequipped", gx2, gy2);
        if (it is ItemRangedWeapon)
        {
          ItemRangedWeapon itemRangedWeapon = it as ItemRangedWeapon;
          if (itemRangedWeapon.Ammo <= 0)
            this.m_UI.UI_DrawImage("Icons\\out_of_ammo", gx2, gy2);
          this.DrawBar(itemRangedWeapon.Ammo, itemRangedWeapon.Ammo, (itemRangedWeapon.Model as ItemRangedWeaponModel).MaxAmmo, 0, 28, 3, gx2 + 2, gy2 + 27, Color.Blue, Color.Blue, Color.Blue, Color.DarkGray);
        }
        else if (it is ItemSprayPaint)
        {
          ItemSprayPaint itemSprayPaint = it as ItemSprayPaint;
          this.DrawBar(itemSprayPaint.PaintQuantity, itemSprayPaint.PaintQuantity, (itemSprayPaint.Model as ItemSprayPaintModel).MaxPaintQuantity, 0, 28, 3, gx2 + 2, gy2 + 27, Color.Gold, Color.Gold, Color.Gold, Color.DarkGray);
        }
        else if (it is ItemSprayScent)
        {
          ItemSprayScent itemSprayScent = it as ItemSprayScent;
          this.DrawBar(itemSprayScent.SprayQuantity, itemSprayScent.SprayQuantity, (itemSprayScent.Model as ItemSprayScentModel).MaxSprayQuantity, 0, 28, 3, gx2 + 2, gy2 + 27, Color.Cyan, Color.Cyan, Color.Cyan, Color.DarkGray);
        }
        else if (it is ItemLight)
        {
          ItemLight itemLight = it as ItemLight;
          if (itemLight.Batteries <= 0)
            this.m_UI.UI_DrawImage("Icons\\out_of_batteries", gx2, gy2);
          this.DrawBar(itemLight.Batteries, itemLight.Batteries, (itemLight.Model as ItemLightModel).MaxBatteries, 0, 28, 3, gx2 + 2, gy2 + 27, Color.Yellow, Color.Yellow, Color.Yellow, Color.DarkGray);
        }
        else if (it is ItemTracker)
        {
          ItemTracker itemTracker = it as ItemTracker;
          if (itemTracker.Batteries <= 0)
            this.m_UI.UI_DrawImage("Icons\\out_of_batteries", gx2, gy2);
          this.DrawBar(itemTracker.Batteries, itemTracker.Batteries, (itemTracker.Model as ItemTrackerModel).MaxBatteries, 0, 28, 3, gx2 + 2, gy2 + 27, Color.Pink, Color.Pink, Color.Pink, Color.DarkGray);
        }
        else if (it is ItemFood)
        {
          ItemFood food = it as ItemFood;
          if (this.m_Rules.IsFoodExpired(food, this.m_Session.WorldTime.TurnCounter))
            this.m_UI.UI_DrawImage("Icons\\expired_food", gx2, gy2);
          else if (this.m_Rules.IsFoodSpoiled(food, this.m_Session.WorldTime.TurnCounter))
            this.m_UI.UI_DrawImage("Icons\\spoiled_food", gx2, gy2);
        }
        else if (it is ItemTrap)
        {
          ItemTrap itemTrap = it as ItemTrap;
          if (itemTrap.IsTriggered)
            this.m_UI.UI_DrawImage("Icons\\trap_triggered", gx2, gy2);
          else if (itemTrap.IsActivated)
            this.m_UI.UI_DrawImage("Icons\\trap_activated", gx2, gy2);
        }
        else if (it is ItemEntertainment && this.m_Player != null && this.m_Player.IsBoredOf(it))
          this.m_UI.UI_DrawImage("Icons\\boring_item", gx2, gy2);
        this.DrawItem(it, gx2, gy2);
        if (++num2 >= slotsPerLine)
        {
          num2 = 0;
          gy2 += 32;
          gx2 = gx;
        }
        else
          gx2 += 32;
      }
      if (!drawSlotsNumbers)
        return;
      int gx3 = gx + 4;
      int gy3 = gy + 32;
      for (int index = 0; index < inventory.MaxCapacity; ++index)
      {
        this.m_UI.UI_DrawString(Color.White, (index + 1).ToString(), gx3, gy3, new Color?());
        gx3 += 32;
      }
    }

    public void DrawItem(Item it, int gx, int gy)
    {
      this.DrawItem(it, gx, gy, Color.White);
    }

    public void DrawItem(Item it, int gx, int gy, Color tint)
    {
      this.m_UI.UI_DrawImage(it.ImageID, gx, gy, tint);
      if (it.Model.IsStackable)
      {
        string text = string.Format("{0}", (object) it.Quantity);
        int gx1 = gx + 32 - 10;
        if (it.Quantity > 100)
          gx1 -= 10;
        else if (it.Quantity > 10)
          gx1 -= 4;
        this.m_UI.UI_DrawString(Color.DarkGray, text, gx1 + 1, gy + 1, new Color?());
        this.m_UI.UI_DrawString(Color.White, text, gx1, gy, new Color?());
      }
      if (!(it is ItemTrap))
        return;
      ItemTrap itemTrap = it as ItemTrap;
      if (itemTrap.IsTriggered)
      {
        this.m_UI.UI_DrawImage("Icons\\trap_triggered", gx, gy);
      }
      else
      {
        if (!itemTrap.IsActivated)
          return;
        this.m_UI.UI_DrawImage("Icons\\trap_activated", gx, gy);
      }
    }

    public void DrawActorSkillTable(Actor actor, int gx, int gy)
    {
      gy -= 14;
      this.m_UI.UI_DrawStringBold(Color.White, "Skills", gx, gy, new Color?());
      gy += 14;
      IEnumerable<Skill> skills = actor.Sheet.SkillTable.Skills;
      if (skills == null)
        return;
      int num = 0;
      int gx1 = gx;
      int gy1 = gy;
      foreach (Skill skill in skills)
      {
        this.m_UI.UI_DrawString(Color.White, string.Format("{0}-", (object) skill.Level), gx1, gy1, new Color?());
        int gx2 = gx1 + 16;
        this.m_UI.UI_DrawString(Color.White, Skills.Name(skill.ID), gx2, gy1, new Color?());
        gx1 = gx2 - 16;
        if (++num >= 10)
        {
          num = 0;
          gy1 = gy;
          gx1 += TEXTFILE_CHARS_PER_LINE;
        }
        else
          gy1 += 12;
      }
    }

    private void AddOverlay(RogueGame.Overlay o)
    {
      lock(m_Overlays) { m_Overlays.Add(o); }
    }

    private void ClearOverlays()
    {
      lock(m_Overlays) { m_Overlays.Clear(); }
    }

    private Point MapToScreen(Point mapPosition)
    {
      return this.MapToScreen(mapPosition.X, mapPosition.Y);
    }

    private Point MapToScreen(int x, int y)
    {
      return new Point((x - this.m_MapViewRect.Left) * 32, (y - this.m_MapViewRect.Top) * 32);
    }

    private Point ScreenToMap(Point screenPosition)
    {
      return this.ScreenToMap(screenPosition.X, screenPosition.Y);
    }

    private Point ScreenToMap(int gx, int gy)
    {
      return new Point(this.m_MapViewRect.Left + gx / 32, this.m_MapViewRect.Top + gy / 32);
    }

    private Point MouseToMap(Point mousePosition)
    {
      return this.MouseToMap(mousePosition.X, mousePosition.Y);
    }

    private Point MouseToMap(int mouseX, int mouseY)
    {
      mouseX = (int) ((double) mouseX / (double) this.m_UI.UI_GetCanvasScaleX());
      mouseY = (int) ((double) mouseY / (double) this.m_UI.UI_GetCanvasScaleY());
      return this.ScreenToMap(mouseX, mouseY);
    }

    private Point MouseToInventorySlot(int invX, int invY, int mouseX, int mouseY)
    {
      mouseX = (int) ((double) mouseX / (double) this.m_UI.UI_GetCanvasScaleX());
      mouseY = (int) ((double) mouseY / (double) this.m_UI.UI_GetCanvasScaleY());
      return new Point((mouseX - invX) / 32, (mouseY - invY) / 32);
    }

    private Point InventorySlotToScreen(int invX, int invY, int slotX, int slotY)
    {
      return new Point(invX + slotX * 32, invY + slotY * 32);
    }

    private bool IsVisibleToPlayer(Location location)
    {
      return this.IsVisibleToPlayer(location.Map, location.Position);
    }

    private bool IsVisibleToPlayer(Map map, Point position)
    {
      if (this.m_Player != null && (map == this.m_Player.Location.Map && map.IsInBounds(position.X, position.Y)))
        return map.GetTileAt(position.X, position.Y).IsInView;
      return false;
    }

    private bool IsVisibleToPlayer(Actor actor)
    {
      if (actor != this.m_Player)
        return this.IsVisibleToPlayer(actor.Location);
      return true;
    }

    private bool IsVisibleToPlayer(MapObject mapObj)
    {
      return this.IsVisibleToPlayer(mapObj.Location);
    }

    private bool IsKnownToPlayer(Map map, Point position)
    {
      if (map.IsInBounds(position.X, position.Y))
        return map.GetTileAt(position.X, position.Y).IsVisited;
      return false;
    }

    private bool IsKnownToPlayer(Location location)
    {
      return this.IsKnownToPlayer(location.Map, location.Position);
    }

    private bool IsKnownToPlayer(MapObject mapObj)
    {
      return this.IsKnownToPlayer(mapObj.Location);
    }

    private bool IsPlayerSleeping()
    {
      if (this.m_Player != null)
        return this.m_Player.IsSleeping;
      return false;
    }

    private int FindLongestLine(string[] lines)
    {
      if (lines == null || lines.Length == 0)
        return 0;
      int num = int.MinValue;
      foreach (string line in lines)
      {
        if (line != null && line.Length > num)
          num = line.Length;
      }
      return num;
    }

    private void HandleSaveGame()
    {
      this.DoSaveGame(RogueGame.GetUserSave());
    }

    private void HandleLoadGame()
    {
      this.DoLoadGame(RogueGame.GetUserSave());
    }

    private void DoSaveGame(string saveName)
    {
      this.ClearMessages();
      this.AddMessage(new djack.RogueSurvivor.Data.Message("SAVING GAME, PLEASE WAIT...", this.m_Session.WorldTime.TurnCounter, Color.Yellow));
      this.RedrawPlayScreen();
      this.m_UI.UI_Repaint();
      Session.Save(this.m_Session, saveName, Session.SaveFormat.FORMAT_BIN);
      this.AddMessage(new djack.RogueSurvivor.Data.Message("SAVING DONE.", this.m_Session.WorldTime.TurnCounter, Color.Yellow));
      this.RedrawPlayScreen();
      this.m_UI.UI_Repaint();
    }

    private void DoLoadGame(string saveName)
    {
      this.ClearMessages();
      this.AddMessage(new djack.RogueSurvivor.Data.Message("LOADING GAME, PLEASE WAIT...", this.m_Session.WorldTime.TurnCounter, Color.Yellow));
      this.RedrawPlayScreen();
      this.m_UI.UI_Repaint();
      if (this.LoadGame(saveName))
        return;
      this.AddMessage(new djack.RogueSurvivor.Data.Message("LOADING FAILED, NO GAME SAVED OR VERSION NOT COMPATIBLE.", this.m_Session.WorldTime.TurnCounter, Color.Red));
    }

    private void DeleteSavedGame(string saveName)
    {
      if (!Session.Delete(saveName))
        return;
      this.AddMessage(new djack.RogueSurvivor.Data.Message("PERMADEATH : SAVE GAME DELETED!", this.m_Session.WorldTime.TurnCounter, Color.Red));
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private bool LoadGame(string saveName)
    {
      if (!Session.Load(saveName, Session.SaveFormat.FORMAT_BIN))
        return false;
      this.m_Session = Session.Get;
      this.m_Rules = new Rules(new DiceRoller(this.m_Session.Seed));
      this.RefreshPlayer();
      this.AddMessage(new djack.RogueSurvivor.Data.Message("LOADING DONE.", this.m_Session.WorldTime.TurnCounter, Color.Yellow));
      this.AddMessage(new djack.RogueSurvivor.Data.Message("Welcome back to Rogue Survivor!", this.m_Session.WorldTime.TurnCounter, Color.LightGreen));
      this.RedrawPlayScreen();
      this.m_UI.UI_Repaint();
      this.m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, "<Loaded game>");
      return true;
    }

    private void LoadOptions()
    {
      RogueGame.s_Options = GameOptions.Load(RogueGame.GetUserOptionsFilePath());
    }

    private void SaveOptions()
    {
      GameOptions.Save(RogueGame.s_Options, RogueGame.GetUserOptionsFilePath());
    }

    private void ApplyOptions(bool ingame)
    {
      this.m_MusicManager.IsMusicEnabled = RogueGame.Options.PlayMusic;
      this.m_MusicManager.Volume = RogueGame.Options.MusicVolume;
      if (this.m_Session != null && this.m_Session.Scoring != null)
      {
        this.m_Session.Scoring.Side = this.m_Player == null || !this.m_Player.Model.Abilities.IsUndead ? DifficultySide.FOR_SURVIVOR : DifficultySide.FOR_UNDEAD;
        this.m_Session.Scoring.DifficultyRating = Scoring.ComputeDifficultyRating(RogueGame.s_Options, this.m_Session.Scoring.Side, this.m_Session.Scoring.ReincarnationNumber);
      }
      if (this.m_MusicManager.IsMusicEnabled)
        return;
      this.m_MusicManager.StopAll();
    }

    private void LoadKeybindings()
    {
      this.m_UI.UI_Clear(Color.Black);
      this.m_UI.UI_DrawStringBold(Color.White, "Loading keybindings...", 0, 0, new Color?());
      this.m_UI.UI_Repaint();
      RogueGame.s_KeyBindings = Keybindings.Load(RogueGame.GetUserConfigPath() + "keys.dat");
      this.m_UI.UI_Clear(Color.Black);
      this.m_UI.UI_DrawStringBold(Color.White, "Loading keybindings... done!", 0, 0, new Color?());
      this.m_UI.UI_Repaint();
    }

    private void SaveKeybindings()
    {
      this.m_UI.UI_Clear(Color.Black);
      this.m_UI.UI_DrawStringBold(Color.White, "Saving keybindings...", 0, 0, new Color?());
      this.m_UI.UI_Repaint();
      Keybindings.Save(RogueGame.s_KeyBindings, RogueGame.GetUserConfigPath() + "keys.dat");
      this.m_UI.UI_Clear(Color.Black);
      this.m_UI.UI_DrawStringBold(Color.White, "Saving keybindings... done!", 0, 0, new Color?());
      this.m_UI.UI_Repaint();
    }

    private void LoadHints()
    {
      this.m_UI.UI_Clear(Color.Black);
      this.m_UI.UI_DrawStringBold(Color.White, "Loading hints...", 0, 0, new Color?());
      this.m_UI.UI_Repaint();
      RogueGame.s_Hints = GameHintsStatus.Load(RogueGame.GetUserConfigPath() + "hints.dat");
      this.m_UI.UI_Clear(Color.Black);
      this.m_UI.UI_DrawStringBold(Color.White, "Loading hints... done!", 0, 0, new Color?());
      this.m_UI.UI_Repaint();
    }

    private void SaveHints()
    {
      GameHintsStatus.Save(RogueGame.s_Hints, RogueGame.GetUserConfigPath() + "hints.dat");
    }

    private void DrawMenuOrOptions(int currentChoice, Color entriesColor, string[] entries, Color valuesColor, string[] values, int gx, ref int gy, int rightPadding = 256)
    {
      int gx1 = gx + rightPadding;
      if (values != null && entries.Length != values.Length)
        throw new ArgumentException("values length!= choices length");
      Color color = Color.FromArgb((int) entriesColor.A, (int) entriesColor.R / 2, (int) entriesColor.G / 2, (int) entriesColor.B / 2);
      for (int index = 0; index < entries.Length; ++index)
      {
        string text1 = index != currentChoice ? string.Format("     {0}", (object) entries[index]) : string.Format("---> {0}", (object) entries[index]);
        this.m_UI.UI_DrawStringBold(entriesColor, text1, gx, gy, new Color?(color));
        if (values != null)
        {
          string text2 = index != currentChoice ? values[index] : string.Format("{0} <---", (object) values[index]);
          this.m_UI.UI_DrawStringBold(valuesColor, text2, gx1, gy, new Color?());
        }
        gy += 14;
      }
    }

    private void DrawHeader()
    {
      this.m_UI.UI_DrawStringBold(Color.Red, "ROGUE SURVIVOR - alpha 9", 0, 0, new Color?(Color.DarkRed));
    }

    private void DrawFootnote(Color color, string text)
    {
      Color color1 = Color.FromArgb((int) color.A, (int) color.R / 2, (int) color.G / 2, (int) color.B / 2);
      this.m_UI.UI_DrawStringBold(color, string.Format("<{0}>", (object) text), 0, 754, new Color?(color1));
    }

    public static string GetUserBasePath()
    {
      return SetupConfig.DirPath;
    }

    public static string GetUserSavesPath()
    {
      return RogueGame.GetUserBasePath() + "Saves\\";
    }

    public static string GetUserSave()
    {
      return RogueGame.GetUserSavesPath() + "save.dat";
    }

    public static string GetUserDocsPath()
    {
      return RogueGame.GetUserBasePath() + "Docs\\";
    }

    public static string GetUserGraveyardPath()
    {
      return RogueGame.GetUserBasePath() + "Graveyard\\";
    }

    public string GetUserNewGraveyardName()
    {
      int num = 0;
      string graveName;
      bool flag;
      do
      {
        graveName = string.Format("grave_{0:D3}", (object) num);
        flag = !File.Exists(RogueGame.GraveFilePath(graveName));
        ++num;
      }
      while (!flag);
      return graveName;
    }

    public static string GraveFilePath(string graveName)
    {
      return RogueGame.GetUserGraveyardPath() + graveName + ".txt";
    }

    public static string GetUserConfigPath()
    {
      return RogueGame.GetUserBasePath() + "Config\\";
    }

    public static string GetUserOptionsFilePath()
    {
      return RogueGame.GetUserConfigPath() + "options.dat";
    }

    public static string GetUserScreenshotsPath()
    {
      return RogueGame.GetUserBasePath() + "Screenshots\\";
    }

    public string GetUserNewScreenshotName()
    {
      int num = 0;
      string shotname;
      bool flag;
      do
      {
        shotname = string.Format("screenshot_{0:D3}", (object) num);
        flag = !File.Exists(this.ScreenshotFilePath(shotname));
        ++num;
      }
      while (!flag);
      return shotname;
    }

    public string ScreenshotFilePath(string shotname)
    {
      return RogueGame.GetUserScreenshotsPath() + shotname + "." + this.m_UI.UI_ScreenshotExtension();
    }

    private bool CreateDirectory(string path)
    {
      if (Directory.Exists(path))
        return false;
      Directory.CreateDirectory(path);
      return true;
    }

    private bool CheckDirectory(string path, string description, ref int gy)
    {
      this.m_UI.UI_DrawString(Color.White, string.Format("{0} : {1}...", (object) description, (object) path), 0, gy, new Color?());
      gy += 14;
      this.m_UI.UI_Repaint();
      bool directory = this.CreateDirectory(path);
      this.m_UI.UI_DrawString(Color.White, "ok.", 0, gy, new Color?());
      gy += 14;
      this.m_UI.UI_Repaint();
      return directory;
    }

    private bool CheckCopyOfManual()
    {
      string str1 = "Resources\\Manual\\";
      string userDocsPath = RogueGame.GetUserDocsPath();
      string str2 = "RS Manual.txt";
      bool flag = false;
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "checking for manual...");
      if (!File.Exists(userDocsPath + str2))
      {
        Logger.WriteLine(Logger.Stage.INIT_MAIN, "copying manual...");
        flag = true;
        File.Copy(str1 + str2, userDocsPath + str2);
        Logger.WriteLine(Logger.Stage.INIT_MAIN, "copying manual... done!");
      }
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "checking for manual... done!");
      return flag;
    }

    private string GetUserManualFilePath()
    {
      return RogueGame.GetUserDocsPath() + "RS Manual.txt";
    }

    private string GetUserHiScorePath()
    {
      return RogueGame.GetUserSavesPath();
    }

    private string GetUserHiScoreFilePath()
    {
      return this.GetUserHiScorePath() + "hiscores.dat";
    }

    private string GetUserHiScoreTextFilePath()
    {
      return this.GetUserHiScorePath() + "hiscores.txt";
    }

    private void GenerateWorld(bool isVerbose, int size)
    {
      if (isVerbose)
      {
        this.m_UI.UI_Clear(Color.Black);
        this.m_UI.UI_DrawStringBold(Color.White, "Generating game world...", 0, 0, new Color?());
        this.m_UI.UI_Repaint();
      }
      if (isVerbose)
      {
        this.m_UI.UI_Clear(Color.Black);
        this.m_UI.UI_DrawStringBold(Color.White, "Creating empty world...", 0, 0, new Color?());
        this.m_UI.UI_Repaint();
      }
      this.m_Session.World = new World(size);
      World world = this.m_Session.World;
      world.Weather = (Weather) this.m_Rules.Roll(0, 4);
      List<Point> pointList = new List<Point>();
      for (int x = 0; x < world.Size; ++x)
      {
        for (int y = 0; y < world.Size; ++y)
          pointList.Add(new Point(x, y));
      }
      Point policeStationDistrictPos = pointList[this.m_Rules.Roll(0, pointList.Count)];
      pointList.Remove(policeStationDistrictPos);
      Point hospitalDistrictPos = pointList[this.m_Rules.Roll(0, pointList.Count)];
      pointList.Remove(hospitalDistrictPos);
      for (int index1 = 0; index1 < world.Size; ++index1)
      {
        for (int index2 = 0; index2 < world.Size; ++index2)
        {
          if (isVerbose)
          {
            this.m_UI.UI_Clear(Color.Black);
            this.m_UI.UI_DrawStringBold(Color.White, string.Format("Creating District@{0}...", (object) World.CoordToString(index1, index2)), 0, 0, new Color?());
            this.m_UI.UI_Repaint();
          }
          District district = new District(new Point(index1, index2), this.GenerateDistrictKind(world, index1, index2));
          world[index1, index2] = district;
          district.EntryMap = this.GenerateDistrictEntryMap(world, district, policeStationDistrictPos, hospitalDistrictPos);
          district.Name = district.EntryMap.Name;
          Map districtSewersMap = this.GenerateDistrictSewersMap(district);
          district.SewersMap = districtSewersMap;
          if (index2 == world.Size / 2)
          {
            Map districtSubwayMap = this.GenerateDistrictSubwayMap(district);
            district.SubwayMap = districtSubwayMap;
          }
        }
      }
      if (isVerbose)
      {
        this.m_UI.UI_Clear(Color.Black);
        this.m_UI.UI_DrawStringBold(Color.White, "Generating unique maps...", 0, 0, new Color?());
        this.m_UI.UI_Repaint();
      }
      this.m_Session.UniqueMaps.CHARUndergroundFacility = this.CreateUniqueMap_CHARUndegroundFacility(world);
      if (isVerbose)
      {
        this.m_UI.UI_Clear(Color.Black);
        this.m_UI.UI_DrawStringBold(Color.White, "Generating unique actors...", 0, 0, new Color?());
        this.m_UI.UI_Repaint();
      }
      this.m_Session.UniqueActors.TheSewersThing = this.SpawnUniqueSewersThing(world);
      this.m_Session.UniqueActors.BigBear = this.CreateUniqueBigBear(world);
      this.m_Session.UniqueActors.FamuFataru = this.CreateUniqueFamuFataru(world);
      this.m_Session.UniqueActors.Santaman = this.CreateUniqueSantaman(world);
      this.m_Session.UniqueActors.Roguedjack = this.CreateUniqueRoguedjack(world);
      this.m_Session.UniqueActors.Duckman = this.CreateUniqueDuckman(world);
      this.m_Session.UniqueActors.HansVonHanz = this.CreateUniqueHansVonHanz(world);
      this.m_Session.UniqueItems.TheSubwayWorkerBadge = this.SpawnUniqueSubwayWorkerBadge(world);
      for (int x1 = 0; x1 < world.Size; ++x1)
      {
        for (int y1 = 0; y1 < world.Size; ++y1)
        {
          if (isVerbose)
          {
            this.m_UI.UI_Clear(Color.Black);
            this.m_UI.UI_DrawStringBold(Color.White, string.Format("Linking District@{0}...", (object) World.CoordToString(x1, y1)), 0, 0, new Color?());
            this.m_UI.UI_Repaint();
          }
          Map entryMap1 = world[x1, y1].EntryMap;
          if (y1 > 0)
          {
            Map entryMap2 = world[x1, y1 - 1].EntryMap;
            for (int x2 = 0; x2 < entryMap1.Width; ++x2)
            {
              if (x2 < entryMap2.Width && this.m_Rules.RollChance(15))
              {
                Point from1 = new Point(x2, -1);
                Point to1 = new Point(x2, entryMap2.Height - 1);
                Point from2 = new Point(x2, entryMap2.Height);
                Point to2 = new Point(x2, 0);
                if (this.CheckIfExitIsGood(entryMap1, from1, entryMap2, to1) && this.CheckIfExitIsGood(entryMap2, from2, entryMap1, to2))
                {
                  this.GenerateExit(entryMap1, from1, entryMap2, to1);
                  this.GenerateExit(entryMap2, from2, entryMap1, to2);
                }
              }
            }
          }
          if (x1 > 0)
          {
            Map entryMap2 = world[x1 - 1, y1].EntryMap;
            for (int y2 = 0; y2 < entryMap1.Height; ++y2)
            {
              if (y2 < entryMap2.Height && this.m_Rules.RollChance(15))
              {
                Point from1 = new Point(-1, y2);
                Point to1 = new Point(entryMap2.Width - 1, y2);
                Point from2 = new Point(entryMap2.Width, y2);
                Point to2 = new Point(0, y2);
                if (this.CheckIfExitIsGood(entryMap1, from1, entryMap2, to1) && this.CheckIfExitIsGood(entryMap2, from2, entryMap1, to2))
                {
                  this.GenerateExit(entryMap1, from1, entryMap2, to1);
                  this.GenerateExit(entryMap2, from2, entryMap1, to2);
                }
              }
            }
          }
          Map sewersMap1 = world[x1, y1].SewersMap;
          if (y1 > 0)
          {
            Map sewersMap2 = world[x1, y1 - 1].SewersMap;
            for (int x2 = 0; x2 < sewersMap1.Width; ++x2)
            {
              if (x2 < sewersMap2.Width)
              {
                Point from1 = new Point(x2, -1);
                Point to1 = new Point(x2, sewersMap2.Height - 1);
                Point from2 = new Point(x2, sewersMap2.Height);
                Point to2 = new Point(x2, 0);
                this.GenerateExit(sewersMap1, from1, sewersMap2, to1);
                this.GenerateExit(sewersMap2, from2, sewersMap1, to2);
              }
            }
          }
          if (x1 > 0)
          {
            Map sewersMap2 = world[x1 - 1, y1].SewersMap;
            for (int y2 = 0; y2 < sewersMap1.Height; ++y2)
            {
              if (y2 < sewersMap2.Height)
              {
                Point from1 = new Point(-1, y2);
                Point to1 = new Point(sewersMap2.Width - 1, y2);
                Point from2 = new Point(sewersMap2.Width, y2);
                Point to2 = new Point(0, y2);
                this.GenerateExit(sewersMap1, from1, sewersMap2, to1);
                this.GenerateExit(sewersMap2, from2, sewersMap1, to2);
              }
            }
          }
          Map subwayMap1 = world[x1, y1].SubwayMap;
          if (subwayMap1 != null && x1 > 0)
          {
            Map subwayMap2 = world[x1 - 1, y1].SubwayMap;
            for (int y2 = 0; y2 < subwayMap1.Height; ++y2)
            {
              if (y2 < subwayMap2.Height)
              {
                Point from1 = new Point(-1, y2);
                Point to1 = new Point(subwayMap2.Width - 1, y2);
                Point from2 = new Point(subwayMap2.Width, y2);
                Point to2 = new Point(0, y2);
                if (subwayMap1.IsWalkable(subwayMap1.Width - 1, y2) && subwayMap2.IsWalkable(0, y2))
                {
                  this.GenerateExit(subwayMap1, from1, subwayMap2, to1);
                  this.GenerateExit(subwayMap2, from2, subwayMap1, to2);
                }
              }
            }
          }
        }
      }
      Map sewersMap = world[0, 0].SewersMap;
      sewersMap.RemoveMapObjectAt(1, 1);
      sewersMap.GetTileAt(1, 1).RemoveAllDecorations();
      sewersMap.GetTileAt(1, 1).AddDecoration("Tiles\\Decoration\\roguedjack");
      if (isVerbose)
      {
        this.m_UI.UI_Clear(Color.Black);
        this.m_UI.UI_DrawStringBold(Color.White, "Spawning player...", 0, 0, new Color?());
        this.m_UI.UI_Repaint();
      }
      int index = world.Size / 2;
      Map entryMap = world[index, index].EntryMap;
      this.GeneratePlayerOnMap(entryMap, this.m_TownGenerator);
      this.SetCurrentMap(entryMap);
      this.RefreshPlayer();
      this.UpdatePlayerFOV(this.m_Player);
      if (RogueGame.s_Options.RevealStartingDistrict)
      {
        Map map = entryMap;
        Location location = this.m_Player.Location;
        int x1 = location.Position.X;
        location = this.m_Player.Location;
        int y1 = location.Position.Y;
        List<Zone> zonesAt1 = map.GetZonesAt(x1, y1);
        if (zonesAt1 != null)
        {
          Zone zone = zonesAt1[0];
          for (int x2 = 0; x2 < entryMap.Width; ++x2)
          {
            for (int y2 = 0; y2 < entryMap.Height; ++y2)
            {
              bool flag = false;
              List<Zone> zonesAt2 = entryMap.GetZonesAt(x2, y2);
              if (zonesAt2 != null && zonesAt2[0] == zone)
                flag = true;
              else if (!entryMap.GetTileAt(x2, y2).IsInside)
                flag = true;
              if (flag)
                entryMap.GetTileAt(x2, y2).IsVisited = true;
            }
          }
        }
      }
      if (!isVerbose)
        return;
      this.m_UI.UI_Clear(Color.Black);
      this.m_UI.UI_DrawStringBold(Color.White, "Generating game world... done!", 0, 0, new Color?());
      this.m_UI.UI_Repaint();
    }

    private bool CheckIfExitIsGood(Map fromMap, Point from, Map toMap, Point to)
    {
      return toMap.GetTileAt(to.X, to.Y).Model.IsWalkable && toMap.GetMapObjectAt(to.X, to.Y) == null;
    }

    private void GenerateExit(Map fromMap, Point from, Map toMap, Point to)
    {
      fromMap.SetExitAt(from, new Exit(toMap, to));
    }

    private UniqueActor SpawnUniqueSewersThing(World world)
    {
      Map map = world[this.m_Rules.Roll(0, world.Size), this.m_Rules.Roll(0, world.Size)].SewersMap;
      Actor named = this.GameActors.SewersThing.CreateNamed(this.GameFactions.TheUndeads, "The Sewers Thing", false, 0);
      DiceRoller roller = new DiceRoller(map.Seed);
      if (!this.m_TownGenerator.ActorPlace(roller, 10000, map, named))
        throw new InvalidOperationException("could not spawn unique The Sewers Thing");
      Zone zoneByPartialName = map.GetZoneByPartialName("Sewers Maintenance");
      if (zoneByPartialName != null)
        this.m_TownGenerator.MapObjectPlaceInGoodPosition(map, zoneByPartialName.Bounds, (Func<Point, bool>) (pt =>
        {
          if (map.IsWalkable(pt.X, pt.Y) && map.GetActorAt(pt) == null)
            return map.GetItemsAt(pt) == null;
          return false;
        }), roller, (Func<Point, MapObject>) (pt => this.m_TownGenerator.MakeObjBoard("MapObjects\\announcement_board", new string[7]
        {
          "TO SEWER WORKERS :",
          "- It lives here.",
          "- Do not disturb.",
          "- Approach with caution.",
          "- Watch your back.",
          "- In case of emergency, take refuge here.",
          "- Do not let other people interact with it!"
        })));
      return new UniqueActor()
      {
        TheActor = named,
        IsSpawned = true
      };
    }

    private UniqueActor CreateUniqueBigBear(World world)
    {
      Actor named = this.GameActors.MaleCivilian.CreateNamed(this.GameFactions.TheCivilians, "Big Bear", false, 0);
      named.IsUnique = true;
      named.Controller = (ActorController) new CivilianAI();
      named.Doll.AddDecoration(DollPart.SKIN, "Actors\\big_bear");
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HAULER);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HAULER);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HAULER);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HARDY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HARDY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HARDY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HARDY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HARDY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.STRONG);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.STRONG);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.STRONG);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.STRONG);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.STRONG);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.TOUGH);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.TOUGH);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.TOUGH);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.TOUGH);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.TOUGH);
      ItemMeleeWeapon itemMeleeWeapon = new ItemMeleeWeapon((ItemModel) this.GameItems.UNIQUE_BIGBEAR_BAT);
      itemMeleeWeapon.IsUnique = true;
      Item it = (Item) itemMeleeWeapon;
      named.Inventory.AddAll(it);
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      return new UniqueActor()
      {
        TheActor = named,
        IsSpawned = false,
        IsWithRefugees = true,
        EventMessage = "You hear an angry man shouting 'FOOLS!'",
        EventThemeMusic = GameMusics.BIGBEAR_THEME_SONG
      };
    }

    private UniqueActor CreateUniqueFamuFataru(World world)
    {
      Actor named = this.GameActors.FemaleCivilian.CreateNamed(this.GameFactions.TheCivilians, "Famu Fataru", false, 0);
      named.IsUnique = true;
      named.Controller = (ActorController) new CivilianAI();
      named.Doll.AddDecoration(DollPart.SKIN, "Actors\\famu_fataru");
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HAULER);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HAULER);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HAULER);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HARDY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HARDY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HARDY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HARDY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HARDY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs._FIRST);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs._FIRST);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs._FIRST);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs._FIRST);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs._FIRST);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HIGH_STAMINA);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HIGH_STAMINA);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HIGH_STAMINA);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HIGH_STAMINA);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HIGH_STAMINA);
      ItemMeleeWeapon itemMeleeWeapon = new ItemMeleeWeapon((ItemModel) this.GameItems.UNIQUE_FAMU_FATARU_KATANA);
      itemMeleeWeapon.IsUnique = true;
      Item it = (Item) itemMeleeWeapon;
      named.Inventory.AddAll(it);
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      return new UniqueActor()
      {
        TheActor = named,
        IsSpawned = false,
        IsWithRefugees = true,
        EventMessage = "You hear a woman laughing.",
        EventThemeMusic = GameMusics.FAMU_FATARU_THEME_SONG
      };
    }

    private UniqueActor CreateUniqueSantaman(World world)
    {
      Actor named = this.GameActors.MaleCivilian.CreateNamed(this.GameFactions.TheCivilians, "Santaman", false, 0);
      named.IsUnique = true;
      named.Controller = (ActorController) new CivilianAI();
      named.Doll.AddDecoration(DollPart.SKIN, "Actors\\santaman");
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HAULER);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HAULER);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HAULER);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HARDY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HARDY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HARDY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HARDY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HARDY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.AWAKE);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.AWAKE);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.AWAKE);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.AWAKE);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.AWAKE);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.FIREARMS);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.FIREARMS);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.FIREARMS);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.FIREARMS);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.FIREARMS);
      ItemRangedWeapon itemRangedWeapon = new ItemRangedWeapon((ItemModel) this.GameItems.UNIQUE_SANTAMAN_SHOTGUN);
      itemRangedWeapon.IsUnique = true;
      Item it = (Item) itemRangedWeapon;
      named.Inventory.AddAll(it);
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemShotgunAmmo());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemShotgunAmmo());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemShotgunAmmo());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      return new UniqueActor()
      {
        TheActor = named,
        IsSpawned = false,
        IsWithRefugees = true,
        EventMessage = "You hear christmas music and drunken vomitting.",
        EventThemeMusic = GameMusics.SANTAMAN_THEME_SONG
      };
    }

    private UniqueActor CreateUniqueRoguedjack(World world)
    {
      Actor named = this.GameActors.MaleCivilian.CreateNamed(this.GameFactions.TheCivilians, "Roguedjack", false, 0);
      named.IsUnique = true;
      named.Controller = (ActorController) new CivilianAI();
      named.Doll.AddDecoration(DollPart.SKIN, "Actors\\roguedjack");
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HAULER);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HAULER);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HAULER);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HARDY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HARDY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HARDY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HARDY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HARDY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.LEADERSHIP);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.LEADERSHIP);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.LEADERSHIP);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.LEADERSHIP);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.LEADERSHIP);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.CHARISMATIC);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.CHARISMATIC);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.CHARISMATIC);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.CHARISMATIC);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.CHARISMATIC);
      ItemMeleeWeapon itemMeleeWeapon = new ItemMeleeWeapon((ItemModel) this.GameItems.UNIQUE_ROGUEDJACK_KEYBOARD);
      itemMeleeWeapon.IsUnique = true;
      Item it = (Item) itemMeleeWeapon;
      named.Inventory.AddAll(it);
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      return new UniqueActor()
      {
        TheActor = named,
        IsSpawned = false,
        IsWithRefugees = true,
        EventMessage = "You hear a man shouting in French.",
        EventThemeMusic = GameMusics.ROGUEDJACK_THEME_SONG
      };
    }

    private UniqueActor CreateUniqueDuckman(World world)
    {
      Actor named = this.GameActors.MaleCivilian.CreateNamed(this.GameFactions.TheCivilians, "Duckman", false, 0);
      named.IsUnique = true;
      named.Controller = (ActorController) new CivilianAI();
      named.Doll.AddDecoration(DollPart.SKIN, "Actors\\duckman");
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.CHARISMATIC);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.CHARISMATIC);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.CHARISMATIC);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.CHARISMATIC);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.CHARISMATIC);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.LEADERSHIP);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.STRONG);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.STRONG);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.STRONG);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.STRONG);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.STRONG);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HIGH_STAMINA);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HIGH_STAMINA);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HIGH_STAMINA);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HIGH_STAMINA);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HIGH_STAMINA);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.MARTIAL_ARTS);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.MARTIAL_ARTS);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.MARTIAL_ARTS);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.MARTIAL_ARTS);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.MARTIAL_ARTS);
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      return new UniqueActor()
      {
        TheActor = named,
        IsSpawned = false,
        IsWithRefugees = true,
        EventMessage = "You hear loud demented QUACKS.",
        EventThemeMusic = GameMusics.DUCKMAN_THEME_SONG
      };
    }

    private UniqueActor CreateUniqueHansVonHanz(World world)
    {
      Actor named = this.GameActors.MaleCivilian.CreateNamed(this.GameFactions.TheCivilians, "Hans von Hanz", false, 0);
      named.IsUnique = true;
      named.Controller = (ActorController) new CivilianAI();
      named.Doll.AddDecoration(DollPart.SKIN, "Actors\\hans_von_hanz");
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HAULER);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HAULER);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.HAULER);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.FIREARMS);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.FIREARMS);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.FIREARMS);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.FIREARMS);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.FIREARMS);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.LEADERSHIP);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.LEADERSHIP);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.LEADERSHIP);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.LEADERSHIP);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.LEADERSHIP);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.NECROLOGY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.NECROLOGY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.NECROLOGY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.NECROLOGY);
      this.m_TownGenerator.GiveStartingSkillToActor(named, Skills.IDs.NECROLOGY);
      ItemRangedWeapon itemRangedWeapon = new ItemRangedWeapon((ItemModel) this.GameItems.UNIQUE_HANS_VON_HANZ_PISTOL);
      itemRangedWeapon.IsUnique = true;
      Item it = (Item) itemRangedWeapon;
      named.Inventory.AddAll(it);
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(this.m_TownGenerator.MakeItemCannedFood());
      return new UniqueActor()
      {
        TheActor = named,
        IsSpawned = false,
        IsWithRefugees = true,
        EventMessage = "You hear a man barking orders in German.",
        EventThemeMusic = GameMusics.HANS_VON_HANZ_THEME_SONG
      };
    }

    private UniqueItem SpawnUniqueSubwayWorkerBadge(World world)
    {
      Item it = new Item(this.GameItems.UNIQUE_SUBWAY_BADGE)
      {
        IsUnique = true,
        IsForbiddenToAI = true
      };
      List<Map> mapList = new List<Map>();
      for (int index1 = 0; index1 < world.Size; ++index1)
      {
        for (int index2 = 0; index2 < world.Size; ++index2)
        {
          if (world[index1, index2].HasSubway)
            mapList.Add(world[index1, index2].SubwayMap);
        }
      }
      if (mapList.Count == 0)
        return new UniqueItem()
        {
          TheItem = it,
          IsSpawned = false
        };
      Map map = mapList[this.m_Rules.Roll(0, mapList.Count)];
      Rectangle bounds = map.GetZoneByPartialName("rails").Bounds;
      Point point = new Point(this.m_Rules.Roll(bounds.Left, bounds.Right), this.m_Rules.Roll(bounds.Top, bounds.Bottom));
      map.DropItemAt(it, point);
      map.GetTileAt(point).AddDecoration("Tiles\\Decoration\\bloodied_floor");
      return new UniqueItem()
      {
        TheItem = it,
        IsSpawned = true
      };
    }

    private UniqueMap CreateUniqueMap_CHARUndegroundFacility(World world)
    {
      List<District> districtList = (List<District>) null;
      for (int index1 = 0; index1 < world.Size; ++index1)
      {
        for (int index2 = 0; index2 < world.Size; ++index2)
        {
          if (world[index1, index2].Kind == DistrictKind.BUSINESS)
          {
            bool flag = false;
            foreach (Zone zone in world[index1, index2].EntryMap.Zones)
            {
              if (zone.HasGameAttribute("CHAR Office"))
              {
                flag = true;
                break;
              }
            }
            if (flag)
            {
              if (districtList == null)
                districtList = new List<District>();
              districtList.Add(world[index1, index2]);
            }
          }
        }
      }
      if (districtList == null)
        throw new InvalidOperationException("world has no business districts with offices");
      District district = districtList[this.m_Rules.Roll(0, districtList.Count)];
      List<Zone> zoneList = new List<Zone>();
      foreach (Zone zone in district.EntryMap.Zones)
      {
        if (zone.HasGameAttribute("CHAR Office"))
          zoneList.Add(zone);
      }
      Zone officeZone = zoneList[this.m_Rules.Roll(0, zoneList.Count)];
      Map mapCharUnderground = this.m_TownGenerator.GenerateUniqueMap_CHARUnderground(district.EntryMap, officeZone);
      mapCharUnderground.District = district;
      mapCharUnderground.Name = string.Format("CHAR Underground Facility @{0}-{1}", (object) district.WorldPosition.X, (object) district.WorldPosition.Y);
      district.AddUniqueMap(mapCharUnderground);
      return new UniqueMap()
      {
        TheMap = mapCharUnderground
      };
    }

    private DistrictKind GenerateDistrictKind(World world, int gridX, int gridY)
    {
      if (gridX == 0 && gridY == 0)
        return DistrictKind.BUSINESS;
      return (DistrictKind) this.m_Rules.Roll(0, 5);
    }

    private Map GenerateDistrictEntryMap(World world, District district, Point policeStationDistrictPos, Point hospitalDistrictPos)
    {
      int x = district.WorldPosition.X;
      int y = district.WorldPosition.Y;
      int seed = this.m_Session.Seed + y * world.Size + x;
      BaseTownGenerator.Parameters parameters = BaseTownGenerator.DEFAULT_PARAMS;
      parameters.MapWidth = parameters.MapHeight = RogueGame.s_Options.DistrictSize;
      parameters.District = district;
      int num = 8;
      string str;
      switch (district.Kind)
      {
        case DistrictKind._FIRST:
          str = "District";
          break;
        case DistrictKind.RESIDENTIAL:
          str = "Residential District";
          parameters.CHARBuildingChance /= num;
          parameters.ParkBuildingChance /= num;
          parameters.ShopBuildingChance /= num;
          break;
        case DistrictKind.SHOPPING:
          str = "Shopping District";
          parameters.CHARBuildingChance /= num;
          parameters.ShopBuildingChance *= num;
          parameters.ParkBuildingChance /= num;
          break;
        case DistrictKind.GREEN:
          str = "Green District";
          parameters.CHARBuildingChance /= num;
          parameters.ParkBuildingChance *= num;
          parameters.ShopBuildingChance /= num;
          break;
        case DistrictKind.BUSINESS:
          str = "Business District";
          parameters.CHARBuildingChance *= num;
          parameters.ParkBuildingChance /= num;
          parameters.ShopBuildingChance /= num;
          break;
        default:
          throw new ArgumentOutOfRangeException("unhandled district kind");
      }
      parameters.GeneratePoliceStation = district.WorldPosition == policeStationDistrictPos;
      parameters.GenerateHospital = district.WorldPosition == hospitalDistrictPos;
      BaseTownGenerator.Parameters @params = this.m_TownGenerator.Params;
      this.m_TownGenerator.Params = parameters;
      Map map = this.m_TownGenerator.Generate(seed);
      map.Name = string.Format("{0}@{1}", (object) str, (object) World.CoordToString(x, y));
      this.m_TownGenerator.Params = @params;
      return map;
    }

    private Map GenerateDistrictSewersMap(District district)
    {
      Map sewersMap = this.m_TownGenerator.GenerateSewersMap(district.EntryMap.Seed << 1 ^ district.EntryMap.Seed, district);
      sewersMap.Name = string.Format("Sewers@{0}-{1}", (object) district.WorldPosition.X, (object) district.WorldPosition.Y);
      return sewersMap;
    }

    private Map GenerateDistrictSubwayMap(District district)
    {
      Map subwayMap = this.m_TownGenerator.GenerateSubwayMap(district.EntryMap.Seed << 2 ^ district.EntryMap.Seed, district);
      subwayMap.Name = string.Format("Subway@{0}-{1}", (object) district.WorldPosition.X, (object) district.WorldPosition.Y);
      return subwayMap;
    }

    private void GeneratePlayerOnMap(Map map, BaseTownGenerator townGen)
    {
      DiceRoller roller = new DiceRoller(map.Seed);
      Actor actor;
      if (this.m_CharGen.IsUndead)
      {
        switch (this.m_CharGen.UndeadModel)
        {
          case GameActors.IDs._FIRST:
            actor = this.m_GameActors.Skeleton.CreateNumberedName(this.m_GameFactions.TheUndeads, 0);
            break;
          case GameActors.IDs.UNDEAD_ZOMBIE:
            actor = this.m_GameActors.Zombie.CreateNumberedName(this.m_GameFactions.TheUndeads, 0);
            break;
          case GameActors.IDs.UNDEAD_ZOMBIE_MASTER:
            actor = this.m_GameActors.ZombieMaster.CreateNumberedName(this.m_GameFactions.TheUndeads, 0);
            break;
          case GameActors.IDs.UNDEAD_MALE_ZOMBIFIED:
          case GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED:
            Actor anonymous = (this.m_CharGen.IsMale ? this.m_GameActors.MaleCivilian : this.m_GameActors.FemaleCivilian).CreateAnonymous(this.m_GameFactions.TheCivilians, 0);
            townGen.DressCivilian(roller, anonymous);
            townGen.GiveNameToActor(roller, anonymous);
            actor = this.Zombify((Actor) null, anonymous, true);
            break;
          default:
            throw new ArgumentOutOfRangeException("unhandled undeadModel");
        }
        this.PrepareActorForPlayerControl(actor);
      }
      else
      {
        actor = (this.m_CharGen.IsMale ? this.m_GameActors.MaleCivilian : this.m_GameActors.FemaleCivilian).CreateAnonymous(this.m_GameFactions.TheCivilians, 0);
        townGen.DressCivilian(roller, actor);
        townGen.GiveNameToActor(roller, actor);
        actor.Sheet.SkillTable.AddOrIncreaseSkill((int) this.m_CharGen.StartingSkill);
        townGen.RecomputeActorStartingStats(actor);
        this.OnSkillUpgrade(actor, this.m_CharGen.StartingSkill);
        int max1 = (int) (0.25 * (double) actor.FoodPoints);
        actor.FoodPoints = actor.FoodPoints - this.m_Rules.Roll(0, max1);
        int max2 = (int) (0.25 * (double) actor.SleepPoints);
        actor.SleepPoints = actor.SleepPoints - this.m_Rules.Roll(0, max2);
      }
      actor.Controller = (ActorController) new PlayerController();
      if (townGen.ActorPlace(roller, 10 * map.Width * map.Height, map, actor, (Predicate<Point>) (pt =>
      {
        bool isInside = map.GetTileAt(pt.X, pt.Y).IsInside;
        if (this.m_CharGen.IsUndead && isInside || !this.m_CharGen.IsUndead && !isInside || RogueGame.IsInCHAROffice(new Location(map, pt)))
          return false;
        MapObject mapObjectAt = map.GetMapObjectAt(pt);
        if (this.m_CharGen.IsUndead)
          return mapObjectAt == null;
        if (mapObjectAt != null)
          return mapObjectAt.IsCouch;
        return false;
      })) || townGen.ActorPlace(roller, map.Width * map.Height, map, actor, (Predicate<Point>) (pt =>
      {
        if (map.GetTileAt(pt.X, pt.Y).IsInside)
          return !RogueGame.IsInCHAROffice(new Location(map, pt));
        return false;
      })))
        return;
      do
        ;
      while (!townGen.ActorPlace(roller, int.MaxValue, map, actor, (Predicate<Point>) (pt => !RogueGame.IsInCHAROffice(new Location(map, pt)))));
    }

    private void RefreshPlayer()
    {
      foreach (Actor actor in this.m_Session.CurrentMap.Actors)
      {
        if (actor.IsPlayer)
        {
          this.m_Player = actor;
          break;
        }
      }
      if (this.m_Player == null)
        return;
      this.ComputeViewRect(this.m_Player.Location.Position);
    }

    private void PrepareActorForPlayerControl(Actor newPlayerAvatar)
    {
      if (newPlayerAvatar.Inventory == null)
        newPlayerAvatar.Inventory = new Inventory(1);
      if (newPlayerAvatar.Sheet.SkillTable == null)
        newPlayerAvatar.Sheet.SkillTable = new SkillTable();
      if (newPlayerAvatar.Leader == null)
        return;
      newPlayerAvatar.Leader.RemoveFollower(newPlayerAvatar);
    }

    private void SetCurrentMap(Map map)
    {
      this.m_Session.CurrentMap = map;
      if (map == map.District.SewersMap)
      {
        this.m_MusicManager.StopAll();
        this.m_MusicManager.PlayLooping(GameMusics.SEWERS);
      }
      else if (this.m_MusicManager.IsPlaying(GameMusics.SEWERS))
        this.m_MusicManager.Stop(GameMusics.SEWERS);
      if (map == map.District.SubwayMap)
      {
        this.m_MusicManager.StopAll();
        this.m_MusicManager.PlayLooping(GameMusics.SUBWAY);
      }
      else if (this.m_MusicManager.IsPlaying(GameMusics.SUBWAY))
        this.m_MusicManager.Stop(GameMusics.SUBWAY);
      if (map == this.m_Session.UniqueMaps.Hospital_Admissions.TheMap || map == this.m_Session.UniqueMaps.Hospital_Offices.TheMap || (map == this.m_Session.UniqueMaps.Hospital_Patients.TheMap || map == this.m_Session.UniqueMaps.Hospital_Power.TheMap) || map == this.m_Session.UniqueMaps.Hospital_Storage.TheMap)
      {
        if (this.m_MusicManager.IsPlaying(GameMusics.HOSPITAL))
          return;
        this.m_MusicManager.StopAll();
        this.m_MusicManager.PlayLooping(GameMusics.HOSPITAL);
      }
      else
      {
        if (!this.m_MusicManager.IsPlaying(GameMusics.HOSPITAL))
          return;
        this.m_MusicManager.Stop(GameMusics.HOSPITAL);
      }
    }

    private void OnPlayerLeaveDistrict()
    {
      this.m_Session.CurrentMap.LocalTime.TurnCounter = this.m_Session.WorldTime.TurnCounter;
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void BeforePlayerEnterDistrict(District district)
    {
      Map entryMap = district.EntryMap;
      int turnCounter = entryMap.LocalTime.TurnCounter;
      if (RogueGame.s_Options.IsSimON)
      {
        m_MusicManager.StopAll();
        m_MusicManager.Play(GameMusics.INTERLUDE);
        if (m_Player != null)
        {
          m_Player.Location.Map.ClearView();
          entryMap.ClearView();
        }
        StopSimThread();
                lock (m_SimMutex)
                {
                    double totalMilliseconds1 = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
                    double num1 = 0.0;
                    bool flag = false;
                    while (entryMap.LocalTime.TurnCounter <= m_Session.WorldTime.TurnCounter)
                    {
                        double totalMilliseconds2 = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
                        if (entryMap.LocalTime.TurnCounter == m_Session.WorldTime.TurnCounter || entryMap.LocalTime.TurnCounter == turnCounter || totalMilliseconds2 >= num1 + 1000.0)
                        {
                            num1 = totalMilliseconds2;
                            ClearMessages();
                            AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Simulating district, please wait {0}/{1}...", (object)entryMap.LocalTime.TurnCounter, (object)m_Session.WorldTime.TurnCounter), m_Session.WorldTime.TurnCounter, Color.White));
                            AddMessage(new djack.RogueSurvivor.Data.Message("(this is an option you can tune)", m_Session.WorldTime.TurnCounter, Color.White));
                            int num2 = entryMap.LocalTime.TurnCounter - turnCounter;
                            if (num2 > 1)
                            {
                                int num3 = m_Session.WorldTime.TurnCounter - entryMap.LocalTime.TurnCounter;
                                double num4 = 1000.0 * (double)num2 / (1.0 + totalMilliseconds2 - totalMilliseconds1);
                                AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Turns per second    : {0:F2}.", (object)num4), m_Session.WorldTime.TurnCounter, Color.White));
                                int num5 = (int)((double)num3 / num4);
                                int num6 = num5 / 60;
                                int num7 = num5 % 60;
                                AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Estimated time left : {0}.", num6 > 0 ? (object)string.Format("{0} min {1:D2} secs", (object)num6, (object)num7) : (object)string.Format("{0} secs", (object)num7)), m_Session.WorldTime.TurnCounter, Color.White));
                            }
                            if (flag)
                                AddMessage(new djack.RogueSurvivor.Data.Message("Simulation aborted!", m_Session.WorldTime.TurnCounter, Color.Red));
                            else
                                AddMessage(new djack.RogueSurvivor.Data.Message("<keep ESC pressed to abort the simulation>", m_Session.WorldTime.TurnCounter, Color.Yellow));
                            RedrawPlayScreen();
                            if (!m_MusicManager.IsPlaying(GameMusics.INTERLUDE))
                                m_MusicManager.Play(GameMusics.INTERLUDE);
                        }
                        if (flag) break;

                        KeyEventArgs keyEventArgs = m_UI.UI_PeekKey();
                        if (keyEventArgs != null && keyEventArgs.KeyCode == Keys.Escape)
                        {
                             foreach (Map map in district.Maps)
                                 map.LocalTime.TurnCounter = m_Session.WorldTime.TurnCounter;
                             flag = true;
                        }
                        if (!flag) SimulateDistrict(district);
                    }
                }
        RestartSimThread();
        RemoveLastMessage();
        foreach (Map map in district.Maps)
        {
          foreach (Actor actor in map.Actors)
          {
            if (!actor.IsSleeping)
              actor.ActionPoints = 0;
          }
        }
        m_MusicManager.StopAll();
      }
      else
      {
        foreach (Map map in district.Maps)
          map.LocalTime.TurnCounter = m_Session.WorldTime.TurnCounter;
      }
    }

    private void OnPlayerChangeMap()
    {
      this.RefreshPlayer();
    }

    private RogueGame.SimFlags ComputeSimFlagsForTurn(int turn)
    {
      bool flag;
      switch (RogueGame.s_Options.SimulateDistricts)
      {
        case GameOptions.SimRatio._FIRST:
          flag = true;
          break;
        case GameOptions.SimRatio.ONE_QUARTER:
          flag = turn % 4 != 0;
          break;
        case GameOptions.SimRatio.ONE_THIRD:
          flag = turn % 3 != 0;
          break;
        case GameOptions.SimRatio.HALF:
          flag = turn % 2 == 1;
          break;
        case GameOptions.SimRatio.TWO_THIRDS:
          flag = turn % 3 == 2;
          break;
        case GameOptions.SimRatio.THREE_QUARTER:
          flag = turn % 4 == 3;
          break;
        case GameOptions.SimRatio.FULL:
          flag = false;
          break;
        default:
          throw new ArgumentOutOfRangeException("unhandled simRatio");
      }
      return !flag ? RogueGame.SimFlags.HIDETAIL_TURN : RogueGame.SimFlags.LODETAIL_TURN;
    }

    private void SimulateDistrict(District d)
    {
      this.AdvancePlay(d, this.ComputeSimFlagsForTurn(d.EntryMap.LocalTime.TurnCounter));
    }

    private bool SimulateNearbyDistricts(District d)
    {
      bool flag = false;
      int x1 = d.WorldPosition.X - 1;
      int x2 = d.WorldPosition.X + 1;
      int y1 = d.WorldPosition.Y - 1;
      int y2 = d.WorldPosition.Y + 1;
      this.m_Session.World.TrimToBounds(ref x1, ref y1);
      this.m_Session.World.TrimToBounds(ref x2, ref y2);
      for (int index1 = x1; index1 <= x2; ++index1)
      {
        for (int index2 = y1; index2 <= y2; ++index2)
        {
          if (index1 != d.WorldPosition.X || index2 != d.WorldPosition.Y)
          {
            District d1 = this.m_Session.World[index1, index2];
            if (d.EntryMap.LocalTime.TurnCounter - d1.EntryMap.LocalTime.TurnCounter > 0)
            {
              flag = true;
              this.SimulateDistrict(d1);
            }
          }
        }
      }
      return flag;
    }

    private void RestartSimThread()
    {
      StopSimThread();
      StartSimThread();
    }

    private void StartSimThread()
    {
      if (!RogueGame.s_Options.IsSimON || !RogueGame.s_Options.SimThread) return;
      if (m_SimThread == null) {
        m_SimThread = new Thread(new ThreadStart(SimThreadProc));
        m_SimThread.Name = "Simulation Thread";
      }
      m_SimThread.Start();
    }

    private void StopSimThread()
    {
      if (m_SimThread == null) return;
      m_SimThread.Abort();
      m_SimThread = (Thread) null;
    }

    private void SimThreadProc()
    {
      while (true) {
        Thread.Sleep(10);
        lock (m_SimMutex) {
          if (m_Player != null)
            SimulateNearbyDistricts(m_Player.Location.Map.District);
        }
      }
    }

    private void ShowNewAchievement(Achievement.IDs id)
    {
      ++this.m_Session.Scoring.CompletedAchievementsCount;
      Achievement achievement = this.m_Session.Scoring.GetAchievement(id);
      string musicId = achievement.MusicID;
      string name = achievement.Name;
      string[] text = achievement.Text;
      this.m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, string.Format("** Achievement : {0} for {1} points. **", (object) name, (object) achievement.ScoreValue));
      this.m_MusicManager.StopAll();
      this.m_MusicManager.Play(musicId);
      string str = new string('*', Math.Max(this.FindLongestLine(text), 50));
      List<string> stringList = new List<string>(text.Length + 3 + 2);
      stringList.Add(str);
      stringList.Add(string.Format("ACHIEVEMENT : {0}", (object) name));
      stringList.Add("CONGRATULATIONS!");
      for (int index = 0; index < text.Length; ++index)
        stringList.Add(text[index]);
      stringList.Add(string.Format("Achievements : {0}/{1}.", (object) this.m_Session.Scoring.CompletedAchievementsCount, (object) 8));
      stringList.Add(str);
      Point screenPos = new Point(0, 0);
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(stringList.ToArray(), Color.Gold, Color.Gold, Color.DimGray, screenPos));
      this.ClearMessages();
      this.AddMessagePressEnter();
      this.ClearOverlays();
    }

    private void ShowSpecialDialogue(Actor speaker, string[] text)
    {
      this.m_MusicManager.StopAll();
      this.m_MusicManager.Play(GameMusics.INTERLUDE);
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayPopup(text, Color.Gold, Color.Gold, Color.DimGray, new Point(0, 0)));
      this.AddOverlay((RogueGame.Overlay) new RogueGame.OverlayRect(Color.Yellow, new Rectangle(this.MapToScreen(speaker.Location.Position), new Size(32, 32))));
      this.ClearMessages();
      this.AddMessagePressEnter();
      this.m_MusicManager.StopAll();
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void CheckSpecialPlayerEventsAfterAction(Actor player)
    {
      if (!player.Model.Abilities.IsUndead && player.Faction != this.GameFactions.TheCHARCorporation && (!this.m_Session.Scoring.HasCompletedAchievement(Achievement.IDs.CHAR_BROKE_INTO_OFFICE) && RogueGame.IsInCHAROffice(player.Location)))
      {
        this.m_Session.Scoring.SetCompletedAchievement(Achievement.IDs.CHAR_BROKE_INTO_OFFICE);
        this.ShowNewAchievement(Achievement.IDs.CHAR_BROKE_INTO_OFFICE);
      }
      if (!this.m_Session.Scoring.HasCompletedAchievement(Achievement.IDs.CHAR_FOUND_UNDERGROUND_FACILITY) && player.Location.Map == this.m_Session.UniqueMaps.CHARUndergroundFacility.TheMap)
      {
        lock (this.m_Session)
        {
          this.m_Session.Scoring.SetCompletedAchievement(Achievement.IDs.CHAR_FOUND_UNDERGROUND_FACILITY);
          this.ShowNewAchievement(Achievement.IDs.CHAR_FOUND_UNDERGROUND_FACILITY);
          this.m_Session.PlayerKnows_CHARUndergroundFacilityLocation = true;
          this.m_Session.CHARUndergroundFacility_Activated = true;
          this.m_Session.UniqueMaps.CHARUndergroundFacility.TheMap.IsSecret = false;
          Map surfaceMap = this.m_Session.UniqueMaps.CHARUndergroundFacility.TheMap.District.EntryMap;
          Point? local_0 = surfaceMap.FindFirstInMap((Predicate<Point>) (pt =>
          {
            Exit exitAt = surfaceMap.GetExitAt(pt);
            if (exitAt == null)
              return false;
            return exitAt.ToMap == this.m_Session.UniqueMaps.CHARUndergroundFacility.TheMap;
          }));
          if (!local_0.HasValue)
            throw new InvalidOperationException("could not find exit to CUF in surface map");
          surfaceMap.GetExitAt(local_0.Value).IsAnAIExit = true;
          Point? local_2 = this.m_Session.UniqueMaps.CHARUndergroundFacility.TheMap.FindFirstInMap((Predicate<Point>) (pt =>
          {
            Exit exitAt = this.m_Session.UniqueMaps.CHARUndergroundFacility.TheMap.GetExitAt(pt);
            if (exitAt == null)
              return false;
            return exitAt.ToMap == surfaceMap;
          }));
          if (!local_2.HasValue)
            throw new InvalidOperationException("could not find exit to surface in CUF map");
          this.m_Session.UniqueMaps.CHARUndergroundFacility.TheMap.GetExitAt(local_2.Value).IsAnAIExit = true;
        }
      }
      if (player != this.m_Session.UniqueActors.TheSewersThing.TheActor && !this.m_Session.PlayerKnows_TheSewersThingLocation && (player.Location.Map == this.m_Session.UniqueActors.TheSewersThing.TheActor.Location.Map && !this.m_Session.UniqueActors.TheSewersThing.TheActor.IsDead && this.IsVisibleToPlayer(this.m_Session.UniqueActors.TheSewersThing.TheActor)))
      {
        lock (this.m_Session)
        {
          this.m_Session.PlayerKnows_TheSewersThingLocation = true;
          this.m_MusicManager.StopAll();
          this.m_MusicManager.Play(GameMusics.FIGHT);
          this.ClearMessages();
          this.AddMessage(new djack.RogueSurvivor.Data.Message("Hey! What's that THING!?", this.m_Session.WorldTime.TurnCounter, Color.Yellow));
          this.AddMessagePressEnter();
        }
      }
      if (player.Location.Map == this.m_Session.UniqueMaps.PoliceStation_JailsLevel.TheMap && !this.m_Session.UniqueActors.PoliceStationPrisonner.TheActor.IsDead)
      {
        Actor theActor = this.m_Session.UniqueActors.PoliceStationPrisonner.TheActor;
        Map map = player.Location.Map;
        switch (this.m_Session.ScriptStage_PoliceStationPrisonner)
        {
          case ScriptStage.STAGE_0:
            if (map.HasAnyAdjacentInMap(player.Location.Position, (Predicate<Point>) (pt => map.GetMapObjectAt(pt) is PowerGenerator)) && !theActor.IsSleeping)
            {
              lock (this.m_Session)
              {
                string[] local_6 = new string[13]
                {
                  "\" Psssst! Hey! You over there! \"",
                  string.Format("{0} is discretly calling you from {1} cell. You listen closely...", (object) theActor.Name, (object) this.HisOrHer(theActor)),
                  "\" Listen! I shouldn't be here! Just drived a bit too fast!",
                  "  Look, I know what's happening! I worked down there! At the CHAR facility!",
                  "  They didn't want me to leave but I did! Like I'm stupid enough to stay down there uh?",
                  "  Now listen! Let's make a deal...",
                  "  Stupid cops won't listen to me. You look clever...",
                  "  You just have to push this button to open my cell.",
                  "  The cops are too busy to care about small fish like me!",
                  "  Then I'll tell you where is the underground facility and just get the hell out of here.",
                  "  I don't give a fuck about CHAR anymore, you can do what you want with that!",
                  "  Do it PLEASE! I REALLY shoudn't be there! \"",
                  string.Format("Looks like {0} wants you to turn the generator on to open the cells...", (object) this.HeOrShe(theActor))
                };
                this.ShowSpecialDialogue(theActor, local_6);
                this.m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, string.Format("{0} offered a deal.", (object) theActor.Name));
                this.m_Session.ScriptStage_PoliceStationPrisonner = ScriptStage.STAGE_1;
                break;
              }
            }
            else
              break;
          case ScriptStage.STAGE_1:
            if (!map.HasZonePartiallyNamedAt(theActor.Location.Position, "jail") && (this.m_Rules.IsAdjacent(player.Location.Position, theActor.Location.Position) && !theActor.IsSleeping))
            {
              lock (this.m_Session)
              {
                string[] local_7 = new string[8]
                {
                  "\" Thank you! Thank you so much!",
                  "  As promised, I'll tell you the big secret!",
                  string.Format("  The CHAR Underground Facility is in district {0}.", (object) World.CoordToString(this.m_Session.UniqueMaps.CHARUndergroundFacility.TheMap.District.WorldPosition.X, this.m_Session.UniqueMaps.CHARUndergroundFacility.TheMap.District.WorldPosition.Y)),
                  "  Look for a CHAR Office, a room with an iron door.",
                  "  Now I must hurry! Thanks a lot for saving me!",
                  "  I don't want them to... UGGH...",
                  "  What's happening? NO!",
                  "  NO NOT ME! aAAAAAaaaa! NOT NOW! AAAGGGGGGGRRR \""
                };
                this.ShowSpecialDialogue(theActor, local_7);
                this.m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, string.Format("Freed {0}.", (object) theActor.Name));
                this.m_Session.PlayerKnows_CHARUndergroundFacilityLocation = true;
                this.m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, "Learned the location of the CHAR Underground Facility.");
                this.KillActor((Actor) null, theActor, "transformation");
                Actor local_8 = this.Zombify((Actor) null, theActor, false);
                local_8.Model = this.m_GameActors.ZombiePrince;
                local_8.ActionPoints = 0;
                this.m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, string.Format("{0} turned into a {1}!", (object) theActor.Name, (object) local_8.Model.Name));
                this.m_MusicManager.Play(GameMusics.FIGHT);
                this.m_Session.ScriptStage_PoliceStationPrisonner = ScriptStage.STAGE_2;
                break;
              }
            }
            else
              break;
          case ScriptStage.STAGE_2:
            break;
          default:
            throw new ArgumentOutOfRangeException("unhandled script stage " + (object) this.m_Session.ScriptStage_PoliceStationPrisonner);
        }
      }
      if (player != this.m_Session.UniqueActors.JasonMyers.TheActor && !this.m_Session.UniqueActors.JasonMyers.TheActor.IsDead && this.IsVisibleToPlayer(this.m_Session.UniqueActors.JasonMyers.TheActor))
      {
        lock (this.m_Session)
        {
          if (!this.m_MusicManager.IsPlaying(GameMusics.INSANE))
          {
            this.m_MusicManager.StopAll();
            this.m_MusicManager.Play(GameMusics.INSANE);
          }
          if (!this.m_Session.Scoring.HasSighted(this.m_Session.UniqueActors.JasonMyers.TheActor.Model.ID))
          {
            this.ClearMessages();
            this.AddMessage(new djack.RogueSurvivor.Data.Message("Nice axe you have there!", this.m_Session.WorldTime.TurnCounter, Color.Yellow));
            this.AddMessagePressEnter();
          }
        }
      }
      if (player != this.m_Session.UniqueActors.Duckman.TheActor)
      {
        if (!this.m_Session.UniqueActors.Duckman.TheActor.IsDead && this.IsVisibleToPlayer(this.m_Session.UniqueActors.Duckman.TheActor))
        {
          if (!this.m_MusicManager.IsPlaying(GameMusics.DUCKMAN_THEME_SONG))
            this.m_MusicManager.Play(GameMusics.DUCKMAN_THEME_SONG);
        }
        else if (this.m_MusicManager.IsPlaying(GameMusics.DUCKMAN_THEME_SONG))
          this.m_MusicManager.Stop(GameMusics.DUCKMAN_THEME_SONG);
      }
      if (this.m_Session.UniqueItems.TheSubwayWorkerBadge.TheItem.IsEquipped && (player.Location.Map == player.Location.Map.District.SubwayMap && player.Inventory.Contains(this.m_Session.UniqueItems.TheSubwayWorkerBadge.TheItem)))
      {
        Map map = player.Location.Map;
        if (map.HasAnyAdjacentInMap(player.Location.Position, (Predicate<Point>) (pt =>
        {
          MapObject mapObjectAt = map.GetMapObjectAt(pt);
          if (mapObjectAt == null)
            return false;
          return mapObjectAt.ImageID == "MapObjects\\gate_closed";
        })))
        {
          this.DoTurnAllGeneratorsOn(map);
          this.AddMessage(new djack.RogueSurvivor.Data.Message("The gate system scanned your badge and turned the power on!", this.m_Session.WorldTime.TurnCounter, Color.Green));
        }
      }
      if (!this.m_Session.Scoring.HasVisited(player.Location.Map))
      {
        this.m_Session.Scoring.AddVisit(this.m_Session.WorldTime.TurnCounter, player.Location.Map);
        this.m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, string.Format("Visited {0}.", (object) player.Location.Map.Name));
      }
      foreach (Point position in this.m_PlayerFOV)
      {
        Actor actorAt = player.Location.Map.GetActorAt(position);
        if (actorAt != null && actorAt != player)
          this.m_Session.Scoring.AddSighting(actorAt.Model.ID, this.m_Session.WorldTime.TurnCounter);
      }
    }

    private void HandleReincarnation()
    {
      this.m_MusicManager.Play(GameMusics.LIMBO);
      if (RogueGame.s_Options.MaxReincarnations <= 0 || !this.AskForReincarnation())
      {
        this.m_MusicManager.StopAll();
      }
      else
      {
        this.m_UI.UI_Clear(Color.Black);
        this.m_UI.UI_DrawStringBold(Color.Yellow, "Reincarnation - Purgatory", 0, 0, new Color?());
        this.m_UI.UI_DrawStringBold(Color.White, "(preparing reincarnations, please wait...)", 0, 28, new Color?());
        this.m_UI.UI_Repaint();
        int matchingActors1;
        Actor reincarnationAvatar1 = this.FindReincarnationAvatar(GameOptions.ReincMode.RANDOM_ACTOR, out matchingActors1);
        int matchingActors2;
        Actor reincarnationAvatar2 = this.FindReincarnationAvatar(GameOptions.ReincMode.RANDOM_LIVING, out matchingActors2);
        int matchingActors3;
        Actor reincarnationAvatar3 = this.FindReincarnationAvatar(GameOptions.ReincMode.RANDOM_UNDEAD, out matchingActors3);
        int matchingActors4;
        Actor reincarnationAvatar4 = this.FindReincarnationAvatar(GameOptions.ReincMode._FIRST, out matchingActors4);
        Actor reincarnationAvatar5 = this.FindReincarnationAvatar(GameOptions.ReincMode.KILLER, out matchingActors1);
        Actor reincarnationAvatar6 = this.FindReincarnationAvatar(GameOptions.ReincMode.ZOMBIFIED, out matchingActors1);
        string[] strArray = this.CompileDistrictFunFacts(this.m_Player.Location.Map.District);
        bool flag = false;
        string[] entries = new string[6]
        {
          GameOptions.Name(GameOptions.ReincMode.RANDOM_ACTOR),
          GameOptions.Name(GameOptions.ReincMode.RANDOM_LIVING),
          GameOptions.Name(GameOptions.ReincMode.RANDOM_UNDEAD),
          GameOptions.Name(GameOptions.ReincMode._FIRST),
          GameOptions.Name(GameOptions.ReincMode.KILLER),
          GameOptions.Name(GameOptions.ReincMode.ZOMBIFIED)
        };
        string[] values = new string[6]
        {
          this.DescribeAvatar(reincarnationAvatar1),
          string.Format("{0}   (out of {1} possibilities)", (object) this.DescribeAvatar(reincarnationAvatar2), (object) matchingActors2),
          string.Format("{0}   (out of {1} possibilities)", (object) this.DescribeAvatar(reincarnationAvatar3), (object) matchingActors3),
          string.Format("{0}   (out of {1} possibilities)", (object) this.DescribeAvatar(reincarnationAvatar4), (object) matchingActors4),
          this.DescribeAvatar(reincarnationAvatar5),
          this.DescribeAvatar(reincarnationAvatar6)
        };
        int currentChoice = 0;
        Actor newPlayerAvatar = (Actor) null;
        do
        {
          int gy1;
          int gx = gy1 = 0;
          this.m_UI.UI_Clear(Color.Black);
          this.m_UI.UI_DrawStringBold(Color.Yellow, "Reincarnation - Choose Avatar", gx, gy1, new Color?());
          int gy2 = gy1 + 28;
          this.DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGreen, values, gx, ref gy2, 256);
          gy2 += 28;
          this.m_UI.UI_DrawStringBold(Color.Pink, ".-* District Fun Facts! *-.", gx, gy2, new Color?());
          int gy3 = gy2 + 14;
          this.m_UI.UI_DrawStringBold(Color.Pink, string.Format("at current date : {0}.", (object) new WorldTime(this.m_Session.WorldTime.TurnCounter).ToString()), gx, gy3, new Color?());
          int gy4 = gy3 + 28;
          for (int index = 0; index < strArray.Length; ++index)
          {
            this.m_UI.UI_DrawStringBold(Color.Pink, strArray[index], gx, gy4, new Color?());
            gy4 += 14;
          }
          this.DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel and end game");
          this.m_UI.UI_Repaint();
          switch (this.m_UI.UI_WaitKey().KeyCode)
          {
            case Keys.Return:
              switch (currentChoice)
              {
                case 0:
                  newPlayerAvatar = reincarnationAvatar1;
                  break;
                case 1:
                  newPlayerAvatar = reincarnationAvatar2;
                  break;
                case 2:
                  newPlayerAvatar = reincarnationAvatar3;
                  break;
                case 3:
                  newPlayerAvatar = reincarnationAvatar4;
                  break;
                case 4:
                  newPlayerAvatar = reincarnationAvatar5;
                  break;
                case 5:
                  newPlayerAvatar = reincarnationAvatar6;
                  break;
              }
              flag = newPlayerAvatar != null;
              break;
            case Keys.Escape:
              flag = true;
              newPlayerAvatar = (Actor) null;
              break;
            case Keys.Up:
              if (currentChoice > 0)
              {
                --currentChoice;
                break;
              }
              currentChoice = entries.Length - 1;
              break;
            case Keys.Down:
              currentChoice = (currentChoice + 1) % entries.Length;
              break;
          }
        }
        while (!flag);
        if (newPlayerAvatar == null)
        {
          this.m_MusicManager.StopAll();
        }
        else
        {
          newPlayerAvatar.Controller = (ActorController) new PlayerController();
          if (newPlayerAvatar.Activity != Activity.SLEEPING)
            newPlayerAvatar.Activity = Activity.IDLE;
          this.PrepareActorForPlayerControl(newPlayerAvatar);
          this.m_Player = newPlayerAvatar;
          this.m_Session.CurrentMap = newPlayerAvatar.Location.Map;
          this.m_Session.Scoring.StartNewLife(this.m_Session.WorldTime.TurnCounter);
          this.m_Session.Scoring.AddEvent(this.m_Session.WorldTime.TurnCounter, string.Format("(reincarnation {0})", (object) this.m_Session.Scoring.ReincarnationNumber));
          this.m_Session.Scoring.Side = this.m_Player.Model.Abilities.IsUndead ? DifficultySide.FOR_UNDEAD : DifficultySide.FOR_SURVIVOR;
          this.m_Session.Scoring.DifficultyRating = Scoring.ComputeDifficultyRating(RogueGame.s_Options, this.m_Session.Scoring.Side, this.m_Session.Scoring.ReincarnationNumber);
          for (int index1 = 0; index1 < this.m_Session.World.Size; ++index1)
          {
            for (int index2 = 0; index2 < this.m_Session.World.Size; ++index2)
            {
              foreach (Map map in this.m_Session.World[index1, index2].Maps)
                map.SetAllAsUnvisited();
            }
          }
          this.m_MusicManager.StopAll();
          this.UpdatePlayerFOV(this.m_Player);
          this.ComputeViewRect(this.m_Player.Location.Position);
          this.ClearMessages();
          this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("{0} feels disoriented for a second...", (object) this.m_Player.Name), this.m_Session.WorldTime.TurnCounter, Color.Yellow));
          this.RedrawPlayScreen();
          string musicname = GameMusics.REINCARNATE;
          if (this.m_Player == this.m_Session.UniqueActors.JasonMyers.TheActor)
            musicname = GameMusics.INSANE;
          this.m_MusicManager.Play(musicname);
          this.RestartSimThread();
        }
      }
    }

    private string DescribeAvatar(Actor a)
    {
      if (a == null)
        return "(N/A)";
      bool flag = a.CountFollowers > 0;
      bool hasLeader = a.HasLeader;
      return string.Format("{0}, a {1}{2}", (object) a.Name, (object) a.Model.Name, flag ? (object) ", leader" : (hasLeader ? (object) ", follower" : (object) ""));
    }

    private bool AskForReincarnation()
    {
      int gy1;
      int gx = gy1 = 0;
      this.m_UI.UI_Clear(Color.Black);
      this.m_UI.UI_DrawStringBold(Color.Yellow, "Limbo", gx, gy1, new Color?());
      int gy2 = gy1 + 28;
      this.m_UI.UI_DrawStringBold(Color.White, string.Format("Leave body {0}/{1}.", (object) (1 + this.m_Session.Scoring.ReincarnationNumber), (object) (1 + RogueGame.s_Options.MaxReincarnations)), gx, gy2, new Color?());
      int gy3 = gy2 + 14;
      this.m_UI.UI_DrawStringBold(Color.White, "Remember lives.", gx, gy3, new Color?());
      int gy4 = gy3 + 14;
      this.m_UI.UI_DrawStringBold(Color.White, "Remember purpose.", gx, gy4, new Color?());
      int gy5 = gy4 + 14;
      this.m_UI.UI_DrawStringBold(Color.White, "Clear again.", gx, gy5, new Color?());
      int gy6 = gy5 + 14;
      if (this.m_Session.Scoring.ReincarnationNumber >= RogueGame.s_Options.MaxReincarnations)
      {
        this.m_UI.UI_DrawStringBold(Color.LightGreen, "Humans interesting.", gx, gy6, new Color?());
        int gy7 = gy6 + 14;
        this.m_UI.UI_DrawStringBold(Color.LightGreen, "Time to leave.", gx, gy7, new Color?());
        int gy8 = gy7 + 14 + 28;
        this.m_UI.UI_DrawStringBold(Color.Yellow, "No more reincarnations left.", gx, gy8, new Color?());
        this.DrawFootnote(Color.White, "press ENTER");
        this.m_UI.UI_Repaint();
        this.WaitEnter();
        return false;
      }
      this.m_UI.UI_DrawStringBold(Color.White, "Leave?", gx, gy6, new Color?());
      int gy9 = gy6 + 14;
      this.m_UI.UI_DrawStringBold(Color.White, "Live?", gx, gy9, new Color?());
      int gy10 = gy9 + 28;
      this.m_UI.UI_DrawStringBold(Color.Yellow, "Reincarnate? Y to confirm, N to cancel.", gx, gy10, new Color?());
      this.m_UI.UI_Repaint();
      return this.WaitYesOrNo();
    }

    private bool IsSuitableReincarnation(Actor a, bool asLiving)
    {
      if (a == null || a.IsDead || a.IsPlayer || a.Location.Map.District != this.m_Session.CurrentMap.District || (a.Location.Map == this.m_Session.UniqueMaps.CHARUndergroundFacility.TheMap || a == this.m_Session.UniqueActors.PoliceStationPrisonner.TheActor || a.Location.Map == a.Location.Map.District.SewersMap))
        return false;
      if (asLiving)
        return !a.Model.Abilities.IsUndead && (!RogueGame.s_Options.IsLivingReincRestricted || a.Faction == this.GameFactions.TheCivilians);
      return a.Model.Abilities.IsUndead && (RogueGame.s_Options.CanReincarnateAsRat || a.Model != this.GameActors.RatZombie);
    }

    private Actor FindReincarnationAvatar(GameOptions.ReincMode reincMode, out int matchingActors)
    {
      switch (reincMode)
      {
        case GameOptions.ReincMode._FIRST:
          if (this.m_Session.Scoring.FollowersWhendDied == null)
          {
            matchingActors = 0;
            return (Actor) null;
          }
          List<Actor> actorList1 = new List<Actor>(this.m_Session.Scoring.FollowersWhendDied.Count);
          foreach (Actor a in this.m_Session.Scoring.FollowersWhendDied)
          {
            if (this.IsSuitableReincarnation(a, true))
              actorList1.Add(a);
          }
          matchingActors = actorList1.Count;
          if (actorList1.Count == 0)
            return (Actor) null;
          return actorList1[this.m_Rules.Roll(0, actorList1.Count)];
        case GameOptions.ReincMode.KILLER:
          Actor killer = this.m_Session.Scoring.Killer;
          if (this.IsSuitableReincarnation(killer, true) || this.IsSuitableReincarnation(killer, false))
          {
            matchingActors = 1;
            return killer;
          }
          matchingActors = 0;
          return (Actor) null;
        case GameOptions.ReincMode.ZOMBIFIED:
          Actor zombifiedPlayer = this.m_Session.Scoring.ZombifiedPlayer;
          if (this.IsSuitableReincarnation(zombifiedPlayer, false))
          {
            matchingActors = 1;
            return zombifiedPlayer;
          }
          matchingActors = 0;
          return (Actor) null;
        case GameOptions.ReincMode.RANDOM_LIVING:
        case GameOptions.ReincMode.RANDOM_UNDEAD:
        case GameOptions.ReincMode.RANDOM_ACTOR:
          bool asLiving = reincMode == GameOptions.ReincMode.RANDOM_LIVING || reincMode == GameOptions.ReincMode.RANDOM_ACTOR && this.m_Rules.RollChance(50);
          List<Actor> actorList2 = new List<Actor>();
          for (int index1 = 0; index1 < this.m_Session.World.Size; ++index1)
          {
            for (int index2 = 0; index2 < this.m_Session.World.Size; ++index2)
            {
              foreach (Map map in this.m_Session.World[index1, index2].Maps)
              {
                foreach (Actor actor in map.Actors)
                {
                  if (this.IsSuitableReincarnation(actor, asLiving))
                    actorList2.Add(actor);
                }
              }
            }
          }
          matchingActors = actorList2.Count;
          if (actorList2.Count == 0)
            return (Actor) null;
          return actorList2[this.m_Rules.Roll(0, actorList2.Count)];
        default:
          throw new ArgumentOutOfRangeException("unhandled reincarnation mode " + reincMode.ToString());
      }
    }

    private ActorAction GenerateInsaneAction(Actor actor)
    {
      switch (this.m_Rules.Roll(0, 5))
      {
        case 0:
          return (ActorAction) new ActionShout(actor, this, "AAAAAAAAAAA!!!");
        case 1:
          return (ActorAction) new ActionBump(actor, this, this.m_Rules.RollDirection());
        case 2:
          Direction direction = this.m_Rules.RollDirection();
          MapObject mapObjectAt = actor.Location.Map.GetMapObjectAt(actor.Location.Position + direction);
          if (mapObjectAt == null)
            return (ActorAction) null;
          return (ActorAction) new ActionBreak(actor, this, mapObjectAt);
        case 3:
          Inventory inventory = actor.Inventory;
          if (inventory == null || inventory.CountItems == 0)
            return (ActorAction) null;
          Item it = inventory[this.m_Rules.Roll(0, inventory.CountItems)];
          ActionUseItem actionUseItem = new ActionUseItem(actor, this, it);
          if (actionUseItem.IsLegal())
            return (ActorAction) actionUseItem;
          if (it.IsEquipped)
            return (ActorAction) new ActionUnequipItem(actor, this, it);
          return (ActorAction) new ActionDropItem(actor, this, it);
        case 4:
          int maxRange = this.m_Rules.ActorFOV(actor, actor.Location.Map.LocalTime, this.m_Session.World.Weather);
          foreach (Actor actor1 in actor.Location.Map.Actors)
          {
            if (actor1 != actor && !this.m_Rules.IsEnemyOf(actor, actor1) && (LOS.CanTraceViewLine(actor.Location, actor1.Location.Position, maxRange) && this.m_Rules.RollChance(50)))
            {
              if (actor.HasLeader)
              {
                actor.Leader.RemoveFollower(actor);
                actor.TrustInLeader = 0;
              }
              this.DoMakeAggression(actor, actor1);
              return (ActorAction) new ActionSay(actor, this, actor1, "YOU ARE ONE OF THEM!!", RogueGame.Sayflags.IS_IMPORTANT);
            }
          }
          return (ActorAction) null;
        default:
          return (ActorAction) null;
      }
    }

    private void SeeingCauseInsanity(Actor whoDoesTheAction, Location loc, int sanCost, string what)
    {
      foreach (Actor actor in loc.Map.Actors)
      {
        if (actor.Model.Abilities.HasSanity && !actor.IsSleeping)
        {
          int maxRange = this.m_Rules.ActorFOV(actor, loc.Map.LocalTime, this.m_Session.World.Weather);
          if (LOS.CanTraceViewLine(loc, actor.Location.Position, maxRange))
          {
            this.SpendActorSanity(actor, sanCost);
            if (whoDoesTheAction == actor)
            {
              if (actor.IsPlayer)
                this.AddMessage(new djack.RogueSurvivor.Data.Message("That was a very disturbing thing to do...", loc.Map.LocalTime.TurnCounter, Color.Orange));
              else if (this.IsVisibleToPlayer(actor))
                this.AddMessage(this.MakeMessage(actor, string.Format("{0} done something very disturbing...", (object) this.Conjugate(actor, this.VERB_HAVE))));
            }
            else if (actor.IsPlayer)
              this.AddMessage(new djack.RogueSurvivor.Data.Message(string.Format("Seeing {0} is very disturbing...", (object) what), loc.Map.LocalTime.TurnCounter, Color.Orange));
            else if (this.IsVisibleToPlayer(actor))
              this.AddMessage(this.MakeMessage(actor, string.Format("{0} something very disturbing...", (object) this.Conjugate(actor, this.VERB_SEE))));
          }
        }
      }
    }

    private void OnMapPowerGeneratorSwitch(Location location, PowerGenerator powGen)
    {
      Map map = location.Map;
      if (map == this.m_Session.UniqueMaps.CHARUndergroundFacility.TheMap)
      {
        lock (this.m_Session)
        {
          if ((double) this.m_Rules.ComputeMapPowerRatio(map) >= 1.0)
          {
            if (map.Lighting != Lighting.LIT)
            {
              map.Lighting = Lighting.LIT;
              if (this.m_Player.Location.Map == map)
              {
                this.ClearMessages();
                this.AddMessage(new djack.RogueSurvivor.Data.Message("The Facility lights turn on!", map.LocalTime.TurnCounter, Color.Green));
                this.RedrawPlayScreen();
              }
              if (!this.m_Session.Scoring.HasCompletedAchievement(Achievement.IDs.CHAR_POWER_UNDERGROUND_FACILITY))
              {
                this.m_Session.Scoring.SetCompletedAchievement(Achievement.IDs.CHAR_POWER_UNDERGROUND_FACILITY);
                this.ShowNewAchievement(Achievement.IDs.CHAR_POWER_UNDERGROUND_FACILITY);
              }
            }
          }
          else if (map.Lighting != Lighting._FIRST)
          {
            map.Lighting = Lighting._FIRST;
            if (this.m_Player.Location.Map == map)
            {
              this.ClearMessages();
              this.AddMessage(new djack.RogueSurvivor.Data.Message("The Facility lights turn off!", map.LocalTime.TurnCounter, Color.Red));
              this.RedrawPlayScreen();
            }
          }
        }
      }
      if (map == map.District.SubwayMap)
      {
        lock (this.m_Session)
        {
          if ((double) this.m_Rules.ComputeMapPowerRatio(map) >= 1.0)
          {
            if (map.Lighting != Lighting.LIT)
            {
              map.Lighting = Lighting.LIT;
              if (this.m_Player.Location.Map == map)
              {
                this.ClearMessages();
                this.AddMessage(new djack.RogueSurvivor.Data.Message("The station power turns on!", map.LocalTime.TurnCounter, Color.Green));
                this.AddMessage(new djack.RogueSurvivor.Data.Message("You hear the gates opening.", map.LocalTime.TurnCounter, Color.Green));
                this.RedrawPlayScreen();
              }
              this.DoOpenSubwayGates(map);
            }
          }
          else if (map.Lighting != Lighting._FIRST)
          {
            if (this.m_Player.Location.Map == map)
            {
              this.ClearMessages();
              this.AddMessage(new djack.RogueSurvivor.Data.Message("The station power turns off!", map.LocalTime.TurnCounter, Color.Red));
              this.AddMessage(new djack.RogueSurvivor.Data.Message("You hear the gates closing.", map.LocalTime.TurnCounter, Color.Red));
              this.RedrawPlayScreen();
            }
            map.Lighting = Lighting._FIRST;
            this.DoCloseSubwayGates(map);
          }
        }
      }
      if (map == this.m_Session.UniqueMaps.PoliceStation_JailsLevel.TheMap)
      {
        lock (this.m_Session)
        {
          if ((double) this.m_Rules.ComputeMapPowerRatio(map) >= 1.0)
          {
            if (this.m_Player.Location.Map == map)
            {
              this.ClearMessages();
              this.AddMessage(new djack.RogueSurvivor.Data.Message("The cells are opening.", map.LocalTime.TurnCounter, Color.Green));
              this.RedrawPlayScreen();
            }
            this.DoOpenPoliceJailCells(map);
          }
          else
          {
            if (this.m_Player.Location.Map == map)
            {
              this.ClearMessages();
              this.AddMessage(new djack.RogueSurvivor.Data.Message("The cells are closing.", map.LocalTime.TurnCounter, Color.Green));
              this.RedrawPlayScreen();
            }
            this.DoClosePoliceJailCells(map);
          }
        }
      }
      if (map != this.m_Session.UniqueMaps.Hospital_Power.TheMap)
        return;
      lock (this.m_Session)
      {
        if ((double) this.m_Rules.ComputeMapPowerRatio(map) >= 1.0)
        {
          if (this.m_Player.Location.Map == map)
          {
            this.ClearMessages();
            this.AddMessage(new djack.RogueSurvivor.Data.Message("The lights turn on and you hear something opening upstairs.", map.LocalTime.TurnCounter, Color.Green));
            this.RedrawPlayScreen();
          }
          this.DoHospitalPowerOn();
        }
        else
        {
          if (map.Lighting == Lighting._FIRST)
            return;
          if (this.m_Player.Location.Map == map)
          {
            this.ClearMessages();
            this.AddMessage(new djack.RogueSurvivor.Data.Message("The lights turn off and you hear something closing upstairs.", map.LocalTime.TurnCounter, Color.Green));
            this.RedrawPlayScreen();
          }
          this.DoHospitalPowerOff();
        }
      }
    }

    private void DoOpenSubwayGates(Map map)
    {
      foreach (MapObject mapObject in map.MapObjects)
      {
        if (mapObject.ImageID == "MapObjects\\gate_closed")
        {
          mapObject.IsWalkable = true;
          mapObject.ImageID = "MapObjects\\gate_open";
        }
      }
    }

    private void DoCloseSubwayGates(Map map)
    {
      foreach (MapObject mapObject in map.MapObjects)
      {
        if (mapObject.ImageID == "MapObjects\\gate_open")
        {
          mapObject.IsWalkable = false;
          mapObject.ImageID = "MapObjects\\gate_closed";
          Actor actorAt = map.GetActorAt(mapObject.Location.Position);
          if (actorAt != null)
          {
            this.KillActor((Actor) null, actorAt, "crushed");
            if (this.m_Player.Location.Map == map)
            {
              this.AddMessage(new djack.RogueSurvivor.Data.Message("Someone got crushed between the closing gates!", map.LocalTime.TurnCounter, Color.Red));
              this.RedrawPlayScreen();
            }
          }
        }
      }
    }

    private void DoOpenPoliceJailCells(Map map)
    {
      foreach (MapObject mapObject in map.MapObjects)
      {
        if (mapObject.ImageID == "MapObjects\\gate_closed")
        {
          mapObject.IsWalkable = true;
          mapObject.ImageID = "MapObjects\\gate_open";
        }
      }
    }

    private void DoClosePoliceJailCells(Map map)
    {
      foreach (MapObject mapObject in map.MapObjects)
      {
        if (mapObject.ImageID == "MapObjects\\gate_open")
        {
          mapObject.IsWalkable = false;
          mapObject.ImageID = "MapObjects\\gate_closed";
          Actor actorAt = map.GetActorAt(mapObject.Location.Position);
          if (actorAt != null)
          {
            this.KillActor((Actor) null, actorAt, "crushed");
            if (this.m_Player.Location.Map == map)
            {
              this.AddMessage(new djack.RogueSurvivor.Data.Message("Someone got crushed between the closing cells!", map.LocalTime.TurnCounter, Color.Red));
              this.RedrawPlayScreen();
            }
          }
        }
      }
    }

    private void DoHospitalPowerOn()
    {
      this.m_Session.UniqueMaps.Hospital_Admissions.TheMap.Lighting = Lighting.LIT;
      this.m_Session.UniqueMaps.Hospital_Offices.TheMap.Lighting = Lighting.LIT;
      this.m_Session.UniqueMaps.Hospital_Patients.TheMap.Lighting = Lighting.LIT;
      this.m_Session.UniqueMaps.Hospital_Power.TheMap.Lighting = Lighting.LIT;
      this.m_Session.UniqueMaps.Hospital_Storage.TheMap.Lighting = Lighting.LIT;
      foreach (MapObject mapObject in this.m_Session.UniqueMaps.Hospital_Storage.TheMap.MapObjects)
      {
        if (mapObject.ImageID == "MapObjects\\gate_closed")
        {
          mapObject.IsWalkable = true;
          mapObject.ImageID = "MapObjects\\gate_open";
        }
      }
    }

    private void DoHospitalPowerOff()
    {
      this.m_Session.UniqueMaps.Hospital_Admissions.TheMap.Lighting = Lighting._FIRST;
      this.m_Session.UniqueMaps.Hospital_Offices.TheMap.Lighting = Lighting._FIRST;
      this.m_Session.UniqueMaps.Hospital_Patients.TheMap.Lighting = Lighting._FIRST;
      this.m_Session.UniqueMaps.Hospital_Power.TheMap.Lighting = Lighting._FIRST;
      this.m_Session.UniqueMaps.Hospital_Storage.TheMap.Lighting = Lighting._FIRST;
      Map theMap = this.m_Session.UniqueMaps.Hospital_Storage.TheMap;
      foreach (MapObject mapObject in theMap.MapObjects)
      {
        if (mapObject.ImageID == "MapObjects\\gate_open")
        {
          mapObject.IsWalkable = false;
          mapObject.ImageID = "MapObjects\\gate_closed";
          Actor actorAt = theMap.GetActorAt(mapObject.Location.Position);
          if (actorAt != null)
          {
            this.KillActor((Actor) null, actorAt, "crushed");
            if (this.m_Player.Location.Map == theMap)
            {
              this.AddMessage(new djack.RogueSurvivor.Data.Message("Someone got crushed between the closing gate!", theMap.LocalTime.TurnCounter, Color.Red));
              this.RedrawPlayScreen();
            }
          }
        }
      }
    }

    private void DoTurnAllGeneratorsOn(Map map)
    {
      foreach (MapObject mapObject in map.MapObjects)
      {
        PowerGenerator powGen = mapObject as PowerGenerator;
        if (powGen != null && !powGen.IsOn)
        {
          powGen.TogglePower();
          this.OnMapPowerGeneratorSwitch(powGen.Location, powGen);
        }
      }
    }

    public static bool IsInCHAROffice(Location location)
    {
      List<Zone> zonesAt = location.Map.GetZonesAt(location.Position.X, location.Position.Y);
      if (zonesAt == null)
        return false;
      foreach (Zone zone in zonesAt)
      {
        if (zone.HasGameAttribute("CHAR Office"))
          return true;
      }
      return false;
    }

    public bool IsInCHARProperty(Location location)
    {
      if (location.Map != this.Session.UniqueMaps.CHARUndergroundFacility.TheMap)
        return RogueGame.IsInCHAROffice(location);
      return true;
    }

    private bool AreLinkedByPhone(Actor speaker, Actor target)
    {
      if (speaker.Leader != target && target.Leader != speaker)
        return false;
      ItemTracker itemTracker1 = speaker.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;
      if (itemTracker1 == null || !itemTracker1.CanTrackFollowersOrLeader)
        return false;
      ItemTracker itemTracker2 = target.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;
      return itemTracker2 != null && itemTracker2.CanTrackFollowersOrLeader;
    }

    private List<Actor> ListWorldActors(Predicate<Actor> pred, RogueGame.MapListFlags flags)
    {
      List<Actor> actorList = new List<Actor>();
      for (int index1 = 0; index1 < this.m_Session.World.Size; ++index1)
      {
        for (int index2 = 0; index2 < this.m_Session.World.Size; ++index2)
          actorList.AddRange((IEnumerable<Actor>) this.ListDistrictActors(this.m_Session.World[index1, index2], flags, pred));
      }
      return actorList;
    }

    private List<Actor> ListDistrictActors(District d, RogueGame.MapListFlags flags, Predicate<Actor> pred)
    {
      List<Actor> actorList = new List<Actor>();
      foreach (Map map in d.Maps)
      {
        if ((flags & RogueGame.MapListFlags.EXCLUDE_SECRET_MAPS) == RogueGame.MapListFlags.NONE || !map.IsSecret)
        {
          foreach (Actor actor in map.Actors)
          {
            if (pred == null || pred(actor))
              actorList.Add(actor);
          }
        }
      }
      return actorList;
    }

    private string FunFactActorResume(Actor a, string info)
    {
      if (a == null)
        return "(N/A)";
      return string.Format("{0} - {1}, a {2} - {3}", (object) info, (object) a.TheName, (object) a.Model.Name, (object) a.Location.Map.Name);
    }

    private string[] CompileDistrictFunFacts(District d)
    {
      List<string> stringList = new List<string>();
      List<Actor> actorList1 = this.ListDistrictActors(d, RogueGame.MapListFlags.EXCLUDE_SECRET_MAPS, (Predicate<Actor>) (a =>
      {
        if (!a.IsDead)
          return !a.Model.Abilities.IsUndead;
        return false;
      }));
      List<Actor> actorList2 = this.ListDistrictActors(d, RogueGame.MapListFlags.EXCLUDE_SECRET_MAPS, (Predicate<Actor>) (a =>
      {
        if (!a.IsDead)
          return a.Model.Abilities.IsUndead;
        return false;
      }));
      List<Actor> actorList3 = this.ListDistrictActors(d, RogueGame.MapListFlags.EXCLUDE_SECRET_MAPS, (Predicate<Actor>) null);
      if (this.m_Player.Model.Abilities.IsUndead)
        actorList2.Add(this.m_Player);
      else
        actorList1.Add(this.m_Player);
      actorList3.Add(this.m_Player);
      if (actorList1.Count > 0)
      {
        actorList1.Sort((Comparison<Actor>) ((a, b) =>
        {
          if (a.SpawnTime < b.SpawnTime)
            return -1;
          return a.SpawnTime != b.SpawnTime ? 1 : 0;
        }));
        stringList.Add("- Oldest Livings Surviving");
        stringList.Add(string.Format("    1st {0}.", (object) this.FunFactActorResume(actorList1[0], new WorldTime(actorList1[0].SpawnTime).ToString())));
        if (actorList1.Count > 1)
          stringList.Add(string.Format("    2nd {0}.", (object) this.FunFactActorResume(actorList1[1], new WorldTime(actorList1[1].SpawnTime).ToString())));
      }
      else
        stringList.Add("    No living actors alive!");
      if (actorList2.Count > 0)
      {
        actorList2.Sort((Comparison<Actor>) ((a, b) =>
        {
          if (a.SpawnTime < b.SpawnTime)
            return -1;
          return a.SpawnTime != b.SpawnTime ? 1 : 0;
        }));
        stringList.Add("- Oldest Undeads Rotting Around");
        stringList.Add(string.Format("    1st {0}.", (object) this.FunFactActorResume(actorList2[0], new WorldTime(actorList2[0].SpawnTime).ToString())));
        if (actorList2.Count > 1)
          stringList.Add(string.Format("    2nd {0}.", (object) this.FunFactActorResume(actorList2[1], new WorldTime(actorList2[1].SpawnTime).ToString())));
      }
      else
        stringList.Add("    No undeads shambling around!");
      if (actorList1.Count > 0)
      {
        actorList1.Sort((Comparison<Actor>) ((a, b) =>
        {
          if (a.KillsCount > b.KillsCount)
            return -1;
          return a.KillsCount != b.KillsCount ? 1 : 0;
        }));
        stringList.Add("- Deadliest Livings Kicking ass");
        if (actorList1[0].KillsCount > 0)
        {
          stringList.Add(string.Format("    1st {0}.", (object) this.FunFactActorResume(actorList1[0], actorList1[0].KillsCount.ToString())));
          if (actorList1.Count > 1 && actorList1[1].KillsCount > 0)
            stringList.Add(string.Format("    2nd {0}.", (object) this.FunFactActorResume(actorList1[1], actorList1[1].KillsCount.ToString())));
        }
        else
          stringList.Add("    Livings can't fight for their lives apparently.");
      }
      if (actorList2.Count > 0)
      {
        actorList2.Sort((Comparison<Actor>) ((a, b) =>
        {
          if (a.KillsCount > b.KillsCount)
            return -1;
          return a.KillsCount != b.KillsCount ? 1 : 0;
        }));
        stringList.Add("- Deadliest Undeads Chewing Brains");
        if (actorList2[0].KillsCount > 0)
        {
          stringList.Add(string.Format("    1st {0}.", (object) this.FunFactActorResume(actorList2[0], actorList2[0].KillsCount.ToString())));
          if (actorList2.Count > 1 && actorList2[1].KillsCount > 0)
            stringList.Add(string.Format("    2nd {0}.", (object) this.FunFactActorResume(actorList2[1], actorList2[1].KillsCount.ToString())));
        }
        else
          stringList.Add("    Undeads don't care for brains apparently.");
      }
      if (actorList1.Count > 0)
      {
        actorList1.Sort((Comparison<Actor>) ((a, b) =>
        {
          if (a.MurdersCounter > b.MurdersCounter)
            return -1;
          return a.MurdersCounter != b.MurdersCounter ? 1 : 0;
        }));
        stringList.Add("- Most Murderous Murderer Murdering");
        if (actorList1[0].MurdersCounter > 0)
        {
          stringList.Add(string.Format("    1st {0}.", (object) this.FunFactActorResume(actorList1[0], actorList1[0].MurdersCounter.ToString())));
          if (actorList1.Count > 1 && actorList1[1].MurdersCounter > 0)
            stringList.Add(string.Format("    2nd {0}.", (object) this.FunFactActorResume(actorList1[1], actorList1[1].MurdersCounter.ToString())));
        }
        else
          stringList.Add("    No murders committed!");
      }
      return stringList.ToArray();
    }

    public void DEV_ToggleShowActorsStats()
    {
      RogueGame.s_Options.DEV_ShowActorsStats = !RogueGame.s_Options.DEV_ShowActorsStats;
    }

    private void LoadData()
    {
      this.LoadDataSkills();
      this.LoadDataItems();
      this.LoadDataActors();
    }

    private void LoadDataActors()
    {
      this.m_GameActors.LoadFromCSV(this.m_UI, "Resources\\Data\\Actors.csv");
    }

    private void LoadDataItems()
    {
      this.m_GameItems.LoadMedicineFromCSV(this.m_UI, "Resources\\Data\\Items_Medicine.csv");
      this.m_GameItems.LoadFoodFromCSV(this.m_UI, "Resources\\Data\\Items_Food.csv");
      this.m_GameItems.LoadMeleeWeaponsFromCSV(this.m_UI, "Resources\\Data\\Items_MeleeWeapons.csv");
      this.m_GameItems.LoadRangedWeaponsFromCSV(this.m_UI, "Resources\\Data\\Items_RangedWeapons.csv");
      this.m_GameItems.LoadExplosivesFromCSV(this.m_UI, "Resources\\Data\\Items_Explosives.csv");
      this.m_GameItems.LoadBarricadingMaterialFromCSV(this.m_UI, "Resources\\Data\\Items_Barricading.csv");
      this.m_GameItems.LoadArmorsFromCSV(this.m_UI, "Resources\\Data\\Items_Armors.csv");
      this.m_GameItems.LoadTrackersFromCSV(this.m_UI, "Resources\\Data\\Items_Trackers.csv");
      this.m_GameItems.LoadSpraypaintsFromCSV(this.m_UI, "Resources\\Data\\Items_Spraypaints.csv");
      this.m_GameItems.LoadLightsFromCSV(this.m_UI, "Resources\\Data\\Items_Lights.csv");
      this.m_GameItems.LoadScentspraysFromCSV(this.m_UI, "Resources\\Data\\Items_Scentsprays.csv");
      this.m_GameItems.LoadTrapsFromCSV(this.m_UI, "Resources\\Data\\Items_Traps.csv");
      this.m_GameItems.LoadEntertainmentFromCSV(this.m_UI, "Resources\\Data\\Items_Entertainment.csv");
      this.m_GameItems.CreateModels();
    }

    private void LoadDataSkills()
    {
      Skills.LoadSkillsFromCSV(this.m_UI, "Resources\\Data\\Skills.csv");
    }

    private abstract class Overlay
    {
      public abstract void Draw(IRogueUI ui);
    }

    private class OverlayImage : RogueGame.Overlay
    {
      public Point ScreenPosition { get; set; }

      public string ImageID { get; set; }

      public OverlayImage(Point screenPosition, string imageID)
      {
        this.ScreenPosition = screenPosition;
        this.ImageID = imageID;
      }

      public override void Draw(IRogueUI ui)
      {
        ui.UI_DrawImage(this.ImageID, this.ScreenPosition.X, this.ScreenPosition.Y);
      }
    }

    private class OverlayTransparentImage : RogueGame.Overlay
    {
      public float Alpha { get; set; }

      public Point ScreenPosition { get; set; }

      public string ImageID { get; set; }

      public OverlayTransparentImage(float alpha, Point screenPosition, string imageID)
      {
        this.Alpha = alpha;
        this.ScreenPosition = screenPosition;
        this.ImageID = imageID;
      }

      public override void Draw(IRogueUI ui)
      {
        ui.UI_DrawTransparentImage(this.Alpha, this.ImageID, this.ScreenPosition.X, this.ScreenPosition.Y);
      }
    }

    private class OverlayText : RogueGame.Overlay
    {
      public Point ScreenPosition { get; set; }

      public string Text { get; set; }

      public Color Color { get; set; }

      public Color? ShadowColor { get; set; }

      public OverlayText(Point screenPosition, Color color, string text)
        : this(screenPosition, color, text, new Color?())
      {
      }

      public OverlayText(Point screenPosition, Color color, string text, Color? shadowColor)
      {
        this.ScreenPosition = screenPosition;
        this.Color = color;
        this.ShadowColor = shadowColor;
        this.Text = text;
      }

      public override void Draw(IRogueUI ui)
      {
        if (this.ShadowColor.HasValue)
          ui.UI_DrawString(this.ShadowColor.Value, this.Text, this.ScreenPosition.X + 1, this.ScreenPosition.Y + 1, new Color?());
        ui.UI_DrawString(this.Color, this.Text, this.ScreenPosition.X, this.ScreenPosition.Y, new Color?());
      }
    }

    private class OverlayLine : RogueGame.Overlay
    {
      public Point ScreenFrom { get; set; }

      public Point ScreenTo { get; set; }

      public Color Color { get; set; }

      public OverlayLine(Point screenFrom, Color color, Point screenTo)
      {
        this.ScreenFrom = screenFrom;
        this.ScreenTo = screenTo;
        this.Color = color;
      }

      public override void Draw(IRogueUI ui)
      {
        ui.UI_DrawLine(this.Color, this.ScreenFrom.X, this.ScreenFrom.Y, this.ScreenTo.X, this.ScreenTo.Y);
      }
    }

    private class OverlayRect : RogueGame.Overlay
    {
      public Rectangle Rectangle { get; set; }

      public Color Color { get; set; }

      public OverlayRect(Color color, Rectangle rect)
      {
        this.Rectangle = rect;
        this.Color = color;
      }

      public override void Draw(IRogueUI ui)
      {
        ui.UI_DrawRect(this.Color, this.Rectangle);
      }
    }

    private class OverlayPopup : RogueGame.Overlay
    {
      public Point ScreenPosition { get; set; }

      public Color TextColor { get; set; }

      public Color BoxBorderColor { get; set; }

      public Color BoxFillColor { get; set; }

      public string[] Lines { get; set; }

      public OverlayPopup(string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, Point screenPos)
      {
        this.ScreenPosition = screenPos;
        this.TextColor = textColor;
        this.BoxBorderColor = boxBorderColor;
        this.BoxFillColor = boxFillColor;
        this.Lines = lines;
      }

      public override void Draw(IRogueUI ui)
      {
        ui.UI_DrawPopup(this.Lines, this.TextColor, this.BoxBorderColor, this.BoxFillColor, this.ScreenPosition.X, this.ScreenPosition.Y);
      }
    }

    private struct CharGen
    {
      public bool IsUndead { get; set; }

      public GameActors.IDs UndeadModel { get; set; }

      public bool IsMale { get; set; }

      public Skills.IDs StartingSkill { get; set; }
    }

    [System.Flags]
    private enum SimFlags
    {
      NOT_SIMULATING = 0,
      HIDETAIL_TURN = 1,
      LODETAIL_TURN = 2,
    }

    [System.Flags]
    public enum Sayflags
    {
      NONE = 0,
      IS_IMPORTANT = 1,
      IS_FREE_ACTION = 2,
    }

    [System.Flags]
    private enum MapListFlags
    {
      NONE = 0,
      EXCLUDE_SECRET_MAPS = 1,
    }
  }
}
