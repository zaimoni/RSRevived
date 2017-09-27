// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.RogueGame
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define DATAFLOW_TRACE

// #define STABLE_SIM_OPTIONAL
#define ENABLE_THREAT_TRACKING
// #define SUICIDE_BY_LONG_WAIT
#define NO_PEACE_WALLS
// #define SPEEDY_GONZALES
#define FRAGILE_RENDERING

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
using System.Linq;
using System.Diagnostics.Contracts;
using ColorString = System.Collections.Generic.KeyValuePair<System.Drawing.Color, string>;
using Zaimoni.Data;

namespace djack.RogueSurvivor.Engine
{
  internal class RogueGame
  {
    private readonly Color POPUP_FILLCOLOR = Color.FromArgb(192, Color.CornflowerBlue);
    // these are used by OverlayPopup, so are used as arrays of strings anyway
    // string.Format requires a string, but otherwise these should remain arrays of strings
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
    // end string arrays used by OverlayPopup

    // report formatting; design width 120 characters, design height 51-ish lines (depends on bold/normal)
    private readonly string hr = "".PadLeft(120,'-');
    private readonly string hr_plus = string.Join("",Enumerable.Range(0,11).Select(x => "---------+").ToArray());

    private readonly Color MODE_TEXTCOLOR = Color.Yellow;
    private readonly Color MODE_BORDERCOLOR = Color.Yellow;
    private readonly Color MODE_FILLCOLOR = Color.FromArgb(192, Color.Gray);
    private readonly Color PLAYER_ACTION_COLOR = Color.White;
    private readonly Color OTHER_ACTION_COLOR = Color.Gray;
    public readonly Color SAYOREMOTE_COLOR = Color.Brown;
    private readonly Color PLAYER_AUDIO_COLOR = Color.Green;
    private readonly Color NIGHT_COLOR = Color.Cyan;
    private readonly Color DAY_COLOR = Color.Gold;
/*
 * Used only by intentionally disabled function TintForDayPhase
    private readonly Color TINT_DAY = Color.White;
    private readonly Color TINT_SUNSET = Color.FromArgb(235, 235, 235);
    private readonly Color TINT_EVENING = Color.FromArgb(215, 215, 215);
    private readonly Color TINT_MIDNIGHT = Color.FromArgb(195, 195, 195);
    private readonly Color TINT_NIGHT = Color.FromArgb(205, 205, 205);
    private readonly Color TINT_SUNRISE = Color.FromArgb(225, 225, 225);
*/
    private readonly Verb VERB_ACCEPT_THE_DEAL = new Verb("accept the deal", "accepts the deal");
    private readonly Verb VERB_ACTIVATE = new Verb("activate");
    private readonly Verb VERB_AVOID = new Verb("avoid");
    private readonly Verb VERB_BARRICADE = new Verb("barricade");
    public static readonly Verb VERB_BASH = new Verb("bash", "bashes");
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
    private readonly Verb VERB_PUT = new Verb("put", "puts");
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
    private readonly List<Overlay> m_Overlays = new List<Overlay>();
    private readonly object m_SimMutex = new object();
    public const int MAP_MAX_HEIGHT = 100;
    public const int MAP_MAX_WIDTH = 100;

    public const int TILE_SIZE = 32;    // ACTOR_SIZE+ACTOR_OFFSET <= TILE_SIZE
    public const int ACTOR_SIZE = 32;
    public const int ACTOR_OFFSET = 0;
    public static readonly Size SIZE_OF_TILE = new Size(TILE_SIZE, TILE_SIZE);
    public static readonly Size SIZE_OF_ACTOR = new Size(ACTOR_SIZE, ACTOR_SIZE);

    public const int TILE_VIEW_WIDTH = 21;
    public const int TILE_VIEW_HEIGHT = 21;
    public const int HALF_VIEW_WIDTH = 10;
    public const int HALF_VIEW_HEIGHT = 10;
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
    private const int SPAWN_DISTANCE_TO_PLAYER = WorldTime.TURNS_PER_HOUR/3;
    private const int SEWERS_INVASION_CHANCE = 1;
    public const float SEWERS_UNDEADS_FACTOR = 0.5f;
    private const float REFUGEES_WAVE_SIZE = 0.2f;
    public const int REFUGEES_WAVE_ITEMS = 3;
    private const int REFUGEE_SURFACE_SPAWN_CHANCE = 80;
    private const int UNIQUE_REFUGEE_CHECK_CHANCE = 10;
    public const int NATGUARD_DAY = 3;
    private const int NATGUARD_END_DAY = 10;
    private const int NATGUARD_ZTRACKER_DAY = NATGUARD_DAY + 3;
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
    private const int DISTRICT_EXIT_CHANCE_PER_TILE = 15;   // XXX dead now that exit generation is on NO_PEACE_WALLS
    private readonly IRogueUI m_UI; // this cannot be static.
    private Rules m_Rules;
    private HiScoreTable m_HiScoreTable;
    private readonly MessageManager m_MessageManager;
    private bool m_HasLoadedGame;
    private Actor m_Player;
    private Rectangle m_MapViewRect;    // morally anchored to Session.Get.CurrentMap
    private static GameOptions s_Options;
    private static Keybindings s_KeyBindings;
    private static GameHintsStatus s_Hints;
    private readonly BaseTownGenerator m_TownGenerator;
    private bool m_PlayedIntro;
    private readonly ISoundManager m_MusicManager;
    private RogueGame.CharGen m_CharGen;
    private TextFile m_Manual;
    private int m_ManualLine;
    private readonly GameActors m_GameActors;
    private readonly GameItems m_GameItems;
#if SUICIDE_BY_LONG_WAIT
    private bool m_IsPlayerLongWait;
    private bool m_IsPlayerLongWaitForcedStop;
    private WorldTime m_PlayerLongWaitEnd;
#endif
    private Thread m_SimThread;

    public Rules Rules {
      get {
        return m_Rules;
      }
    }

    public IRogueUI UI {
      get {
        return m_UI;
      }
    }

    public bool IsGameRunning {
      get {
        return m_IsGameRunning;
      }
    }

    public static GameOptions Options
    {
      get
      {
        return s_Options;
      }
    }

    public static Keybindings KeyBindings
    {
      get
      {
        return RogueGame.s_KeyBindings;
      }
    }

    public GameActors GameActors
    {
      get
      {
        return m_GameActors;
      }
    }

    public GameItems GameItems
    {
      get
      {
        return m_GameItems;
      }
    }

    public Actor Player
    {
      get
      {
        return m_Player;
      }
    }

    public RogueGame(IRogueUI UI)
    {
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "RogueGame()");
      m_UI = UI;
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating MusicManager");
      switch (SetupConfig.Sound)
      {
#if FAIL
        case SetupConfig.eSound.SOUND_MANAGED_DIRECTX:
          this.m_MusicManager = (ISoundManager) new MDXSoundManager();
          break;
#endif
        case SetupConfig.eSound.SOUND_WAV:
          m_MusicManager = new WAVSoundManager();
          break;
        default:
          m_MusicManager = new NullSoundManager();
          break;
      }
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating MessageManager");
      m_MessageManager = new MessageManager(12, 25, 59);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating Rules");
      m_Rules = new Rules(new DiceRoller(Session.Get.Seed));  // possibly no-op
      BaseTownGenerator.Parameters parameters = BaseTownGenerator.DEFAULT_PARAMS;
      parameters.MapWidth = MAP_MAX_WIDTH;
      parameters.MapHeight = RogueGame.MAP_MAX_HEIGHT;
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating Generator");
      m_TownGenerator = new StdTownGenerator(this, parameters);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating options, keys, hints.");
      s_Options = new GameOptions();
      s_Options.ResetToDefaultValues();
      s_KeyBindings = new Keybindings();
      s_Hints = new GameHintsStatus();
      s_Hints.ResetAllHints();
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating dbs");
      m_GameActors = new GameActors(m_UI);
      m_GameItems = new GameItems(m_UI);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "RogueGame() done.");
    }

    public void AddMessage(Data.Message msg)
    {
      if (msg.Text.Length == 0) return;
      if (m_MessageManager.Count >= MAX_MESSAGES) m_MessageManager.Clear();
      msg.Text = string.Format("{0} {1}", Session.Get.WorldTime.TurnCounter, Capitalize(msg.Text));
      m_MessageManager.Add(msg);
    }

    // XXX just about everything that rates this is probable cause for police investigation
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public void AddMessageIfAudibleForPlayer(Location loc, string text)
    {
      if (  m_Player == null
         || m_Player.IsSleeping
         || loc.Map != m_Player.Location.Map
         || Rules.StdDistance(m_Player.Location.Position, loc.Position) > m_Player.AudioRange)
      {
        List<Actor> tmp = loc.Map.Players;
        if (0 >= tmp.Count) return;
        tmp = tmp.Where(a => !a.IsSleeping && Rules.StdDistance(a.Location.Position, loc.Position) <= a.AudioRange).ToList();
        if (0 >= tmp.Count) return;
        PanViewportTo(tmp[0]);
      }
      // Data.Message msg = MakePlayerCentricMessage(string eventText, location.Position);
      Data.Message msg = MakePlayerCentricMessage(text, loc.Position);
      msg.Color = PLAYER_AUDIO_COLOR;
      AddMessage(msg);
#if SUICIDE_BY_LONG_WAIT
      if (m_IsPlayerLongWait) m_IsPlayerLongWaitForcedStop = true;
#endif
      RedrawPlayScreen();
    }

    private Data.Message MakePlayerCentricMessage(string eventText, Point position)
    {
      Point v = new Point(position.X - m_Player.Location.Position.X, position.Y - m_Player.Location.Position.Y);
      return new Data.Message(string.Format("{0} {1} tiles to the {2}.", eventText, (int)Rules.StdDistance(v), Direction.ApproximateFromVector(v)), Session.Get.WorldTime.TurnCounter);
    }

    private Data.Message MakeErrorMessage(string text)
    {
      return new Data.Message(text, Session.Get.WorldTime.TurnCounter, Color.Red);
    }

    private Data.Message MakeYesNoMessage(string question)
    {
      return new Data.Message(string.Format("{0}? Y to confirm, N to cancel", question), Session.Get.WorldTime.TurnCounter, Color.Yellow);
    }

    private string ActorVisibleIdentity(Actor actor)
    {
      if (!IsVisibleToPlayer(actor))
        return "someone";
      return actor.TheName;
    }

    private string ObjectVisibleIdentity(MapObject mapObj)
    {
      if (!IsVisibleToPlayer(mapObj))
        return "something";
      return mapObj.TheName;
    }

    private Data.Message MakeMessage(Actor actor, string doWhat)
    {
      return MakeMessage(actor, doWhat, OTHER_ACTION_COLOR);
    }

    public Data.Message MakeMessage(Actor actor, string doWhat, Color color)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(ActorVisibleIdentity(actor));
      stringBuilder.Append(" ");
      stringBuilder.Append(doWhat);
      return new Data.Message(stringBuilder.ToString(), Session.Get.WorldTime.TurnCounter)
      {
        Color = !actor.IsPlayer ? color : PLAYER_ACTION_COLOR
      };
    }

    private Data.Message MakeMessage(Actor actor, string doWhat, Actor target)
    {
      return MakeMessage(actor, doWhat, target, ".");
    }

    private Data.Message MakeMessage(Actor actor, string doWhat, Actor target, string phraseEnd)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(ActorVisibleIdentity(actor));
      stringBuilder.Append(" ");
      stringBuilder.Append(doWhat);
      stringBuilder.Append(" ");
      stringBuilder.Append(ActorVisibleIdentity(target));
      stringBuilder.Append(phraseEnd);
      return new Data.Message(stringBuilder.ToString(), Session.Get.WorldTime.TurnCounter)
      {
        Color = actor.IsPlayer || target.IsPlayer ? PLAYER_ACTION_COLOR : OTHER_ACTION_COLOR
      };
    }

    private Data.Message MakeMessage(Actor actor, string doWhat, MapObject target)
    {
      return MakeMessage(actor, doWhat, target, ".");
    }

    private Data.Message MakeMessage(Actor actor, string doWhat, MapObject target, string phraseEnd)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(ActorVisibleIdentity(actor));
      stringBuilder.Append(" ");
      stringBuilder.Append(doWhat);
      stringBuilder.Append(" ");
      stringBuilder.Append(ObjectVisibleIdentity(target));
      stringBuilder.Append(phraseEnd);
      return new Data.Message(stringBuilder.ToString(), Session.Get.WorldTime.TurnCounter)
      {
        Color = !actor.IsPlayer ? OTHER_ACTION_COLOR : PLAYER_ACTION_COLOR
      };
    }

    private Data.Message MakeMessage(Actor actor, string doWhat, Item target)
    {
      return MakeMessage(actor, doWhat, target, ".");
    }

    private Data.Message MakeMessage(Actor actor, string doWhat, Item target, string phraseEnd)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(ActorVisibleIdentity(actor));
      stringBuilder.Append(" ");
      stringBuilder.Append(doWhat);
      stringBuilder.Append(" ");
      stringBuilder.Append(target.TheName);
      stringBuilder.Append(phraseEnd);
      return new Data.Message(stringBuilder.ToString(), Session.Get.WorldTime.TurnCounter)
      {
        Color = !actor.IsPlayer ? OTHER_ACTION_COLOR : PLAYER_ACTION_COLOR
      };
    }

    public void ClearMessages()
    {
      m_MessageManager.Clear();
    }

    private void ClearMessagesHistory()
    {
      m_MessageManager.ClearHistory();
    }

    public void RemoveLastMessage()
    {
      m_MessageManager.RemoveLastMessage();
    }

    private void DrawMessages()
    {
      m_MessageManager.Draw(m_UI, Session.Get.LastTurnPlayerActed, 4, 676);
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public void AddMessagePressEnter()
    {
      AddMessage(new Data.Message("<press ENTER>", Session.Get.WorldTime.TurnCounter, Color.Yellow));
      RedrawPlayScreen();
      WaitEnter();
      RemoveLastMessage();
      RedrawPlayScreen();
    }

    private string Conjugate(Actor actor, string verb)
    {
      if (!actor.IsProperName || actor.IsPluralName) return verb;
      return verb + "s";
    }

    private string Conjugate(Actor actor, Verb verb)
    {
      if (!actor.IsProperName || actor.IsPluralName) return verb.YouForm;
      return verb.HeForm;
    }

    private string Capitalize(string text)
    {
      if (text == null) return "";
      if (text.Length == 1)
        return string.Format("{0}", char.ToUpper(text[0]));
      return string.Format("{0}{1}", char.ToUpper(text[0]), text.Substring(1));
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

    private string TruncateString(string s, int maxLength)
    {
      if (s.Length > maxLength) return s.Substring(0, maxLength);
      return s;
    }

    private void AnimDelay(int msecs)
    {
      if (s_Options.IsAnimDelayOn) m_UI.UI_Wait(msecs);
    }

    public void Run()
    {
            InitDirectories();
            LoadData();
            LoadOptions();
            LoadHints();
            ApplyOptions(false);
            LoadKeybindings();
            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.White, "Loading music...", 0, 0, new Color?());
            m_UI.UI_Repaint();
            m_MusicManager.Load(GameMusics.ARMY, GameMusics.ARMY_FILE);
            m_MusicManager.Load(GameMusics.BIGBEAR_THEME_SONG, GameMusics.BIGBEAR_THEME_SONG_FILE);
            m_MusicManager.Load(GameMusics.BIKER, GameMusics.BIKER_FILE);
            m_MusicManager.Load(GameMusics.CHAR_UNDERGROUND_FACILITY, GameMusics.CHAR_UNDERGROUND_FACILITY_FILE);
            m_MusicManager.Load(GameMusics.DUCKMAN_THEME_SONG, GameMusics.DUCKMAN_THEME_SONG_FILE);
            m_MusicManager.Load(GameMusics.FAMU_FATARU_THEME_SONG, GameMusics.FAMU_FATARU_THEME_SONG_FILE);
            m_MusicManager.Load(GameMusics.FIGHT, GameMusics.FIGHT_FILE);
            m_MusicManager.Load(GameMusics.GANGSTA, GameMusics.GANGSTA_FILE);
            m_MusicManager.Load(GameMusics.HANS_VON_HANZ_THEME_SONG, GameMusics.HANS_VON_HANZ_THEME_SONG_FILE);
            m_MusicManager.Load(GameMusics.HEYTHERE, GameMusics.HEYTHERE_FILE);
            m_MusicManager.Load(GameMusics.HOSPITAL, GameMusics.HOSPITAL_FILE);
            m_MusicManager.Load(GameMusics.INSANE, GameMusics.INSANE_FILE);
            m_MusicManager.Load(GameMusics.INTERLUDE, GameMusics.INTERLUDE_FILE);
            m_MusicManager.Load(GameMusics.INTRO, GameMusics.INTRO_FILE);
            m_MusicManager.Load(GameMusics.LIMBO, GameMusics.LIMBO_FILE);
            m_MusicManager.Load(GameMusics.PLAYER_DEATH, GameMusics.PLAYER_DEATH_FILE);
            m_MusicManager.Load(GameMusics.REINCARNATE, GameMusics.REINCARNATE_FILE);
            m_MusicManager.Load(GameMusics.ROGUEDJACK_THEME_SONG, GameMusics.ROGUEDJACK_THEME_SONG_FILE);
            m_MusicManager.Load(GameMusics.SANTAMAN_THEME_SONG, GameMusics.SANTAMAN_THEME_SONG_FILE);
            m_MusicManager.Load(GameMusics.SEWERS, GameMusics.SEWERS_FILE);
            m_MusicManager.Load(GameMusics.SLEEP, GameMusics.SLEEP_FILE);
            m_MusicManager.Load(GameMusics.SUBWAY, GameMusics.SUBWAY_FILE);
      m_MusicManager.Load(GameMusics.SURVIVORS, GameMusics.SURVIVORS_FILE);
      m_UI.UI_Clear(Color.Black);
      m_UI.UI_DrawStringBold(Color.White, "Loading music... done!", 0, 0, new Color?());
      m_UI.UI_Repaint();
      m_UI.UI_Clear(Color.Black);
      m_UI.UI_DrawStringBold(Color.White, "Loading sfxs...", 0, 0, new Color?());
      m_UI.UI_Repaint();
      m_MusicManager.Load(GameSounds.UNDEAD_EAT, GameSounds.UNDEAD_EAT_FILE);
      m_MusicManager.Load(GameSounds.UNDEAD_RISE, GameSounds.UNDEAD_RISE_FILE);
      m_MusicManager.Load(GameSounds.NIGHTMARE, GameSounds.NIGHTMARE_FILE);
      m_UI.UI_Clear(Color.Black);
      m_UI.UI_DrawStringBold(Color.White, "Loading sfxs... done!", 0, 0, new Color?());
      m_UI.UI_Repaint();
      LoadManual();
      LoadHiScoreTable();
      while (m_IsGameRunning)
        GameLoop();
      m_MusicManager.StopAll();
      m_MusicManager.Dispose();
      m_UI.UI_DoQuit();
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void GameLoop()
    {
      HandleMainMenu();
      while (m_IsGameRunning && 0 < Session.Get.World.PlayerCount) {
        District d = Session.Get.World.CurrentPlayerDistrict();
        if (null == d) {
          if (null == Session.Get.World.CurrentSimulationDistrict()) throw new InvalidOperationException("no districts available to simulate");
          if (null == m_SimThread) throw new InvalidOperationException("no simulation thread");
          Thread.Sleep(100);
          continue;
        }
        m_HasLoadedGame = false;
        DateTime now = DateTime.Now;
        AdvancePlay(d, RogueGame.SimFlags.NOT_SIMULATING);
        if (!m_IsGameRunning) break;
        Session.Get.Scoring.RealLifePlayingTime = Session.Get.Scoring.RealLifePlayingTime.Add(DateTime.Now - now);
        Session.Get.World.ScheduleAdjacentForAdvancePlay(d);
      }
    }

    private void InitDirectories()
    {
      int gy1 = 0;
      m_UI.UI_Clear(Color.Black);
      m_UI.UI_DrawStringBold(Color.Yellow, "Checking user game directories...", 0, gy1, new Color?());
      gy1 += 14;
      m_UI.UI_Repaint();
      if (!(  CheckDirectory(GetUserBasePath(), "base user", ref gy1) | CheckDirectory(GetUserConfigPath(), "config", ref gy1)
            | CheckDirectory(GetUserDocsPath(), "docs", ref gy1) | CheckDirectory(GetUserGraveyardPath(), "graveyard", ref gy1)
            | CheckDirectory(GetUserSavesPath(), "saves", ref gy1) | CheckDirectory(GetUserScreenshotsPath(), "screenshots", ref gy1) | CheckCopyOfManual()))
        return;
      m_UI.UI_DrawStringBold(Color.Yellow, "Directories and game manual created.", 0, gy1, new Color?());
      gy1 += 14;
      m_UI.UI_DrawStringBold(Color.Yellow, "Your game data directory is in the game folder:", 0, gy1, new Color?());
      gy1 += 14;
      m_UI.UI_DrawString(Color.LightGreen, GetUserBasePath(), 0, gy1, new Color?());
      gy1 += 14;
      m_UI.UI_DrawStringBold(Color.Yellow, "When you uninstall the game you can delete this directory.", 0, gy1, new Color?());
      DrawFootnote(Color.White, "press ENTER");
      m_UI.UI_Repaint();
      WaitEnter();
    }

    static private void LogSaveScumStats() {
      DiceRoller tmp = new DiceRoller(Session.Get.Seed);
      string msg = "first seven RNG d100 values:";
      List<int> tmp2 = new List<int>(7);
      int i = 7;
      do tmp2.Add(tmp.Roll(0, 100));
      while(0 < --i);
      msg += String.Join<int>(" ",tmp2);
      Logger.WriteLine(Logger.Stage.RUN_MAIN, msg);
      // report on particularly noxious consequences
      if (Rules.FIREARM_JAM_CHANCE_RAIN > tmp2[0]) Logger.WriteLine(Logger.Stage.RUN_MAIN, "firearms jam in rain on save-load");
      else if (Rules.FIREARM_JAM_CHANCE_NO_RAIN > tmp2[0]) Logger.WriteLine(Logger.Stage.RUN_MAIN, "firearms jam on save-load");
      if (Rules.MELEE_WEAPON_FRAGILE_BREAK_CHANCE > tmp2[6]) Logger.WriteLine(Logger.Stage.RUN_MAIN, "fragile melee weapons break on save-load");
      else if (Rules.MELEE_WEAPON_BREAK_CHANCE > tmp2[6]) Logger.WriteLine(Logger.Stage.RUN_MAIN, "melee weapons break on save-load");
    }

#if FAIL
// minimum demo code fragment
      Func<int,bool?> setup_handler = (currentChoice => {
        return null;
      });
      Func<int, bool?> choice_handler = (currentChoice => {
        return null;
      });
      Func<Keys,int,bool?> failover_handler = ((k,currentChoice) => {
        return null;
      });
      m_IsGameRunning = ChoiceMenu(choice_handler, setup_handler, entries.Length);
#endif
    // this is a UI function so we can afford to be an inefficient monolithic function
    // return values of handlers: null is continue, true/false are the return values
    // Compiler error to mix this with out/ref parameters
    private bool ChoiceMenu(Func<int, bool?> choice_handler, Func<int, bool?> setup_handler, int choice_length, Func<Keys,int,bool?> failover_handler=null)
    {
#if DEBUG
      if (null == choice_handler) throw new ArgumentNullException(nameof(choice_handler));
      if (null == setup_handler) throw new ArgumentNullException(nameof(setup_handler));
#endif
      int currentChoice = 0;
      do {
        bool? ret = setup_handler(currentChoice);
        if (null != ret) return ret.Value;
        m_UI.UI_Repaint();

        KeyEventArgs key = m_UI.UI_WaitKey();

        switch(key.KeyCode) {
          case Keys.Return:
            ret = choice_handler(currentChoice);
            if (null == ret) break;
            return ret.Value;
          case Keys.Escape: return false;
          case Keys.Up:
            if (currentChoice > 0) {
              --currentChoice;
              break;
            }
            currentChoice = choice_length - 1;
            break;
          case Keys.Down:
            currentChoice = (currentChoice + 1) % choice_length;
            break;
          default:
            if (null == failover_handler) break;
            ret = failover_handler(key.KeyCode,currentChoice);
            if (null == ret) break;
            return ret.Value;
        }
      } while(true);
    }

    // boxed value type return value version of ChoiceMenu
    private T? ChoiceMenuNN<T>(Func<int, T?> choice_handler, Func<int, T?> setup_handler, int choice_length, Func<Keys,int,T?> failover_handler=null) where T:struct
    {
#if DEBUG
      if (null == choice_handler) throw new ArgumentNullException(nameof(choice_handler));
      if (null == setup_handler) throw new ArgumentNullException(nameof(setup_handler));
#endif
      int currentChoice = 0;
      do {
        T? ret = setup_handler(currentChoice);
        if (null != ret) return ret.Value;
        m_UI.UI_Repaint();

        KeyEventArgs key = m_UI.UI_WaitKey();

        switch(key.KeyCode) {
          case Keys.Return:
            ret = choice_handler(currentChoice);
            if (null == ret) break;
            return ret.Value;
          case Keys.Escape: return null;
          case Keys.Up:
            if (currentChoice > 0) {
              --currentChoice;
              break;
            }
            currentChoice = choice_length - 1;
            break;
          case Keys.Down:
            currentChoice = (currentChoice + 1) % choice_length;
            break;
          default:
            if (null == failover_handler) break;
            ret = failover_handler(key.KeyCode,currentChoice);
            if (null == ret) break;
            return ret.Value;
        }
      } while(true);
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void HandleMainMenu()
    {
      bool flag2 = File.Exists(GetUserSave());
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

      int gy1 = 0;
      const int gx1 = 0;

      Func<int,bool?> setup_handler = (c => {
        if (!m_MusicManager.IsPlaying(GameMusics.INTRO) && !m_PlayedIntro) {
          m_MusicManager.StopAll();
          m_MusicManager.Play(GameMusics.INTRO);
          m_PlayedIntro = true;
        }
        gy1 = 0;
        m_UI.UI_Clear(Color.Black);
        DrawHeader();
        gy1 += 14;
        m_UI.UI_DrawStringBold(Color.Yellow, "Main Menu", 0, gy1, new Color?());
        gy1 += 28;
        DrawMenuOrOptions(c, Color.White, entries, Color.White, null, gx1, ref gy1, 256);
        DrawFootnote(Color.White, "cursor to move, ENTER to select");
        DateTime now = DateTime.Now;
        if (now.Month == 12 && now.Day >= 24 && now.Day <= 26) {
          for (int index = 0; index < 10; ++index) {
            int gx2 = m_Rules.Roll(0, CANVAS_WIDTH-ACTOR_SIZE+ACTOR_OFFSET);
            int gy2 = m_Rules.Roll(0, CANVAS_HEIGHT-ACTOR_SIZE+ACTOR_OFFSET);
            m_UI.UI_DrawImage(GameImages.ACTOR_SANTAMAN, gx2, gy2);
            m_UI.UI_DrawStringBold(Color.Snow, "* Merry Christmas *", gx2 - 60, gy2 - 10, new Color?());
          }
        }
        return null;
      });
      Func<int, bool?> choice_handler = (c => {
          switch (c) {
          case 0:
            if (HandleNewCharacter()) {
              StartNewGame();
              LogSaveScumStats();
              return true;
            }
            break;
          case 1:
            if (flag2) {
              gy1 += 28;
              m_UI.UI_DrawStringBold(Color.Yellow, "Loading game, please wait...", gx1, gy1, new Color?());
              m_UI.UI_Repaint();
              LoadGame(GetUserSave());
              LogSaveScumStats();
              RestartSimThread();
              return true;
            }
            break;
          case 2: HandleRedefineKeys(); break;
          case 3:
            HandleOptions(false);
            ApplyOptions(false);
            break;
          case 4: HandleHelpMode(); break;
          case 5: HandleHintsScreen(); break;
          case 6: HandleHiScores(true); break;
          case 7: HandleCredits(); break;
          case 8: return false;
        }
        return null;
      });

      m_IsGameRunning = ChoiceMenu(choice_handler, setup_handler, entries.Length);
    }

    private bool HandleNewCharacter()
    {
      DiceRoller roller = new DiceRoller();
      if (!HandleNewGameMode()) return false;
      bool? isUndead = HandleNewCharacterRace(roller);
      if (null == isUndead) return false;
      m_CharGen.IsUndead = isUndead.Value;
      if (isUndead.Value) {
        GameActors.IDs? undeadModel = HandleNewCharacterUndeadType(roller);
        if (null == undeadModel) return false;
        m_CharGen.UndeadModel = undeadModel.Value;
      } else {
        bool? isMale = HandleNewCharacterGender(roller);
        if (null == isMale) return false;
        m_CharGen.IsMale = isMale.Value;

        Skills.IDs? skID = HandleNewCharacterSkill(roller);
        if (null == skID) return false;
        m_CharGen.StartingSkill = skID.Value;
        Session.Get.Scoring.StartingSkill = skID.Value;
      }
      return true;
    }

    private bool HandleNewGameMode()
    {
      string[] entries = new string[(int)GameMode_Bounds._COUNT]
      {
        Session.DescGameMode(GameMode.GM_STANDARD),
        Session.DescGameMode(GameMode.GM_CORPSES_INFECTION),
        Session.DescGameMode(GameMode.GM_VINTAGE)
      };
      string[] values = new string[(int)GameMode_Bounds._COUNT]
      {
        SetupConfig.GAME_NAME+" standard game.",
        "Don't get a cold. Keep an eye on your deceased diseased friends.",
        "The classic zombies next door."
      };
      string[][] summaries = new string[(int)GameMode_Bounds._COUNT][]
      {
        new string[] {
          "This is the standard game setting:",
          "- All the kinds of undeads.",
          "- Undeads can evolve.",
          "- Livings can zombify instantly when dead."
        },
        new string[] {
          "This is the standard game setting with corpses and infection: ",
          "- All the kinds of undeads.",
          "- Undeads can evolve.",
          "- Some undeads can infect livings when damaging them.",
          "- Livings become corpses when dead.",
          "- Corpses will slowy rot... but may rise as undead if infected."
        },
        new string[] {
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
        }
      };
      const int gx = 0;

      Func<int,bool?> setup_handler = (currentChoice => {
        m_UI.UI_Clear(Color.Black);
        int gy1 = 0;
        m_UI.UI_DrawStringBold(Color.Yellow, "New Game - Choose Game Mode", gx, gy1, new Color?());
        gy1 += 28;
        DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGray, values, gx, ref gy1, 256);
        gy1 += 28;
        foreach (string text in summaries[currentChoice]) {
          m_UI.UI_DrawStringBold(Color.Gray, text, gx, gy1, new Color?());
          gy1 += 14;
        }
        DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel");
        return null;
      });
      Func<int, bool?> choice_handler = (currentChoice => {
        switch (currentChoice) {
          case 0:
            Session.Get.GameMode = GameMode.GM_STANDARD;
            return true;
          case 1:
            Session.Get.GameMode = GameMode.GM_CORPSES_INFECTION;
            return true;
          case 2:
            Session.Get.GameMode = GameMode.GM_VINTAGE;
            ApplyOptions(false);
            return true;
        }
        return null;
      });
      return ChoiceMenu(choice_handler, setup_handler, entries.Length);
    }

    private bool? HandleNewCharacterRace(DiceRoller roller)
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
      const int gx = 0;
      int gy1 = 0;

      Func<int,bool?> setup_handler = (currentChoice => {
        m_UI.UI_Clear(Color.Black);
        gy1 = 0;
        m_UI.UI_DrawStringBold(Color.Yellow, string.Format("[{0}] New Character - Choose Race", Session.DescGameMode(Session.Get.GameMode)), gx, gy1, new Color?());
        gy1 += 28;
        DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGray, values, gx, ref gy1, 256);
        DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel");
        return null;
      });
      Func<int, bool?> choice_handler = (currentChoice => {
        switch (currentChoice) {
        case 0:
          bool isUndead = roller.RollChance(50);
          int gy3 = gy1 + 42;
          m_UI.UI_DrawStringBold(Color.White, string.Format("Race : {0}.", isUndead ? "Undead" : "Living"), gx, gy3, new Color?());
          int gy4 = gy3 + 14;
          m_UI.UI_DrawStringBold(Color.Yellow, "Is that OK? Y to confirm, N to cancel.", gx, gy4, new Color?());
          m_UI.UI_Repaint();
          if (WaitYesOrNo()) return isUndead;
          break;
        case 1: return false;
        case 2: return true;
        }
        return null;
      });
      return ChoiceMenuNN(choice_handler, setup_handler, entries.Length);
    }

    private bool? HandleNewCharacterGender(DiceRoller roller)
    {
      ActorModel maleCivilian = GameActors.MaleCivilian;
      ActorModel femaleCivilian = GameActors.FemaleCivilian;
      string[] entries = new string[3] {
        "*Random*",
        "Male",
        "Female"
      };
      string[] values = new string[3] {
        "(picks a gender at random for you)",
        string.Format("HP:{0:D2}  Def:{1:D2}  Dmg:{2:D1}",  maleCivilian.StartingSheet.BaseHitPoints,  maleCivilian.StartingSheet.BaseDefence.Value,  maleCivilian.StartingSheet.UnarmedAttack.DamageValue),
        string.Format("HP:{0:D2}  Def:{1:D2}  Dmg:{2:D1}",  femaleCivilian.StartingSheet.BaseHitPoints,  femaleCivilian.StartingSheet.BaseDefence.Value,  femaleCivilian.StartingSheet.UnarmedAttack.DamageValue)
      };
      const int gx = 0;
      int gy = 0;

      Func<int,bool?> setup_handler = (currentChoice => {
        m_UI.UI_Clear(Color.Black);
        gy = 0;
        m_UI.UI_DrawStringBold(Color.Yellow, string.Format("[{0}] New Living - Choose Gender", Session.DescGameMode(Session.Get.GameMode)), gx, gy, new Color?());
        gy += 28;
        DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGray, values, gx, ref gy, 256);
        DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel");
        return null;
      });
      Func<int, bool?> choice_handler = (currentChoice => {
        switch (currentChoice) {
          case 0:
            bool isMale = roller.RollChance(50);
            gy += 14;
            m_UI.UI_DrawStringBold(Color.White, string.Format("Gender : {0}.", isMale ? "Male" : "Female"), gx, gy, new Color?());
            gy += 14;
            m_UI.UI_DrawStringBold(Color.Yellow, "Is that OK? Y to confirm, N to cancel.", gx, gy, new Color?());
            m_UI.UI_Repaint();
            if (WaitYesOrNo()) return isMale;
            break;
          case 1: return true;
          case 2: return false;
        }
        return null;
      });
      return ChoiceMenuNN(choice_handler, setup_handler, entries.Length);
    }

    static private string DescribeUndeadModelStatLine(ActorModel m)
    {
      return string.Format("HP:{0:D3}  Spd:{1:F2}  Atk:{2:D2}  Def:{3:D2}  Dmg:{4:D2}  FoV:{5:D1}  Sml:{6:F2}", m.StartingSheet.BaseHitPoints, (float)((double)m.DollBody.Speed / 100.0), m.StartingSheet.UnarmedAttack.HitValue, m.StartingSheet.BaseDefence.Value, m.StartingSheet.UnarmedAttack.DamageValue, m.StartingSheet.BaseViewRange, m.StartingSheet.BaseSmellRating);
    }

    private GameActors.IDs? HandleNewCharacterUndeadType(DiceRoller roller)
    {
      ActorModel[] undead = {
        GameActors.Skeleton,
        GameActors.Zombie,
        GameActors.MaleZombified,
        GameActors.FemaleZombified,
        GameActors.ZombieMaster
      };

      string[] entries = (new string[] { "*Random*" }).Concat(undead.Select(x => x.Name)).ToArray();
      string[] values = (new string[] { "(picks a type at random for you)" }).Concat(undead.Select(x => DescribeUndeadModelStatLine(x))).ToArray();

      const int gx = 0;
      int gy1 = 0;

      Func<int,GameActors.IDs?> setup_handler = (currentChoice => {
        m_UI.UI_Clear(Color.Black);
        gy1 = 0;
        m_UI.UI_DrawStringBold(Color.Yellow, string.Format("[{0}] New Undead - Choose Type", Session.DescGameMode(Session.Get.GameMode)), gx, gy1, new Color?());
        gy1 += 28;
        DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGray, values, gx, ref gy1, 256);
        DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel");
        return null;
      });
      Func<int, GameActors.IDs?> choice_handler = (currentChoice => {
        switch (currentChoice) {
          case 0:
            GameActors.IDs modelID = undead[roller.Roll(0, 5)].ID;
            int gy3 = gy1 + 14;
            m_UI.UI_DrawStringBold(Color.White, string.Format("Type : {0}.", GameActors[modelID].Name), gx, gy3);
            int gy4 = gy3 + 14;
            m_UI.UI_DrawStringBold(Color.Yellow, "Is that OK? Y to confirm, N to cancel.", gx, gy4);
            m_UI.UI_Repaint();
            if (WaitYesOrNo()) return modelID;
            break;
          default: return undead[currentChoice-1].ID;
        }
        return null;
      });
      return ChoiceMenuNN(choice_handler, setup_handler, entries.Length);
    }

    private Skills.IDs? HandleNewCharacterSkill(DiceRoller roller)
    {
      Skills.IDs[] idsArray = Enumerable.Range(0, 1 + (int)Skills.IDs._LAST_LIVING).Select(id => (Skills.IDs)id).ToArray();
      string[] entries = (new string[] { "*Random*" }).Concat(idsArray.Select(id => Skills.Name(id))).ToArray();
      string[] values = (new string[] { "(picks a skill at random for you)" }).Concat(idsArray.Select(id => string.Format("{0} max - {1}", Skills.MaxSkillLevel(id), DescribeSkillShort(id)))).ToArray();

      const int gx = 0;
      int gy1 = 0;

      Func<int,Skills.IDs?> setup_handler = (currentChoice => {
        gy1 = 0;
        m_UI.UI_Clear(Color.Black);
        m_UI.UI_DrawStringBold(Color.Yellow, string.Format("[{0}] New {1} Character - Choose Starting Skill", Session.DescGameMode(Session.Get.GameMode), m_CharGen.IsMale ? "Male" : "Female"), gx, gy1, new Color?());
        gy1 += 28;
        DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGray, values, gx, ref gy1, 256);
        DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel");
        return null;
      });
      Func<int, Skills.IDs?> choice_handler = (currentChoice => {
        Skills.IDs skID = currentChoice != 0 ? (Skills.IDs) (currentChoice - 1) : Skills.RollLiving(roller);
        int gy3 = gy1 + 14;
        m_UI.UI_DrawStringBold(Color.White, string.Format("Skill : {0}.", Skills.Name(skID)), gx, gy3, new Color?());
        int gy4 = gy3 + 14;
        m_UI.UI_DrawStringBold(Color.Yellow, "Is that OK? Y to confirm, N to cancel.", gx, gy4, new Color?());
        m_UI.UI_Repaint();
        if (WaitYesOrNo()) return skID;
        return null;
      });
      return ChoiceMenuNN(choice_handler, setup_handler, entries.Length);
    }

    private void LoadManual()
    {
      m_UI.UI_Clear(Color.Black);
      m_UI.UI_DrawStringBold(Color.White, "Loading game manual...", 0, 0, new Color?());
      int gy1 = 14;
      m_UI.UI_Repaint();
      m_Manual = new TextFile();
      m_ManualLine = 0;
      if (!m_Manual.Load(GetUserManualFilePath())) {
        m_UI.UI_DrawStringBold(Color.Red, "Error while loading the manual.", 0, gy1, new Color?());
        gy1 += 14;
        m_UI.UI_DrawStringBold(Color.Red, "The manual won't be available ingame.", 0, gy1, new Color?());
        m_UI.UI_Repaint();
        DrawFootnote(Color.White, "press ENTER");
        WaitEnter();
        m_Manual = null;
      } else {
        m_UI.UI_DrawStringBold(Color.White, "Parsing game manual...", 0, gy1, new Color?());
        gy1 += 14;
        m_UI.UI_Repaint();
        m_Manual.FormatLines(TEXTFILE_CHARS_PER_LINE);
        m_UI.UI_Clear(Color.Black);
        m_UI.UI_DrawStringBold(Color.White, "Game manual... done!", 0, gy1, new Color?());
        m_UI.UI_Repaint();
      }
    }

    private void HandleHiScores(bool saveToTextfile)
    {
      TextFile textFile = (saveToTextfile ? new TextFile() : null);
      m_UI.UI_Clear(Color.Black);
      DrawHeader();
      int gy1 = 14;
      m_UI.UI_DrawStringBold(Color.Yellow, "Hi Scores", 0, gy1, new Color?());
      gy1 += 14;
      m_UI.UI_DrawStringBold(Color.White, hr_plus, 0, gy1, new Color?());
      gy1 += 14;
      m_UI.UI_DrawStringBold(Color.White, "Rank | Name, Skills, Death       |  Score |Difficulty|Survival|  Kills |Achievm.|      Game Time | Playing time", 0, gy1, new Color?());
      gy1 += 14;
      if (saveToTextfile) {
        textFile.Append(SetupConfig.GAME_NAME_CAPS+" "+SetupConfig.GAME_VERSION);
        textFile.Append("Hi Scores");
        textFile.Append("Rank | Name, Skills, Death       |  Score |Difficulty|Survival|  Kills |Achievm.|      Game Time | Playing time");
      }
      for (int index = 0; index < m_HiScoreTable.Count; ++index) {
        Color color = index == 0 ? Color.LightYellow : (index == 1 ? Color.LightCyan : (index == 2 ? Color.LightGreen : Color.DimGray));
        m_UI.UI_DrawStringBold(color, hr, 0, gy1, new Color?());
        gy1 += 14;
        HiScore hiScore = m_HiScoreTable[index];
        string str = string.Format("{0,3}. | {1,-25} | {2,6} |     {3,3}% | {4,6} | {5,6} | {6,6} | {7,14} | {8}", index + 1, TruncateString(hiScore.Name, 25), hiScore.TotalPoints, hiScore.DifficultyPercent, hiScore.SurvivalPoints, hiScore.KillPoints, hiScore.AchievementPoints, new WorldTime(hiScore.TurnSurvived).ToString(), TimeSpanToString(hiScore.PlayingTime));
        m_UI.UI_DrawStringBold(color, str, 0, gy1, new Color?());
        gy1 += 14;
        m_UI.UI_DrawStringBold(color, string.Format("     | {0}.", hiScore.SkillsDescription), 0, gy1, new Color?());
        gy1 += 14;
        m_UI.UI_DrawStringBold(color, string.Format("     | {0}.", hiScore.Death), 0, gy1, new Color?());
        gy1 += 14;
        if (saveToTextfile) {
          textFile.Append(hr);
          textFile.Append(str);
          textFile.Append(string.Format("     | {0}", hiScore.SkillsDescription));
          textFile.Append(string.Format("     | {0}", hiScore.Death));
        }
      }
      string scoreTextFilePath = GetUserHiScoreTextFilePath();
      if (saveToTextfile) textFile.Save(scoreTextFilePath);
      m_UI.UI_DrawStringBold(Color.White, hr_plus, 0, gy1, new Color?());
      gy1 += 14;
      if (saveToTextfile) m_UI.UI_DrawStringBold(Color.White, scoreTextFilePath, 0, gy1, new Color?());
      DrawFootnote(Color.White, "press ESC to leave");
      m_UI.UI_Repaint();
      WaitEscape();
    }

    private void LoadHiScoreTable()
    {
      m_UI.UI_Clear(Color.Black);
      m_UI.UI_DrawStringBold(Color.White, "Loading hiscores table...", 0, 0, new Color?());
      m_UI.UI_Repaint();
      m_HiScoreTable = HiScoreTable.Load(GetUserHiScoreFilePath());
      if (m_HiScoreTable == null) {
        m_HiScoreTable = new HiScoreTable(12);
        m_HiScoreTable.Clear();
      }
      m_UI.UI_Clear(Color.Black);
      m_UI.UI_DrawStringBold(Color.White, "Loading hiscores table... done!", 0, 0, new Color?());
      m_UI.UI_Repaint();
    }

    private void SaveHiScoreTable()
    {
      m_UI.UI_Clear(Color.Black);
      m_UI.UI_DrawStringBold(Color.White, "Saving hiscores table...", 0, 0, new Color?());
      m_UI.UI_Repaint();
      HiScoreTable.Save(m_HiScoreTable, GetUserHiScoreFilePath());
      m_UI.UI_Clear(Color.Black);
      m_UI.UI_DrawStringBold(Color.White, "Saving hiscores table... done!", 0, 0, new Color?());
      m_UI.UI_Repaint();
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void StartNewGame()
    {
      bool isUndead = m_CharGen.IsUndead;
      GenerateWorld(true);
      Session.Get.Scoring.AddVisit(Session.Get.WorldTime.TurnCounter, m_Player.Location.Map);
      Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format(isUndead ? "Rose in {0}." : "Woke up in {0}.", m_Player.Location.Map.Name));
      Session.Get.Scoring.Side = isUndead ? DifficultySide.FOR_UNDEAD : DifficultySide.FOR_SURVIVOR;
      if (s_Options.IsAdvisorEnabled) {
        ClearMessages();
        ClearMessagesHistory();
        AddMessage(new Data.Message("The Advisor is enabled and will give you hints during the game.", 0, Color.LightGreen));
        AddMessage(new Data.Message("The hints help a beginner learning the basic controls.", 0, Color.LightGreen));
        AddMessage(new Data.Message("You can disable the Advisor by going to the Options screen.", 0, Color.LightGreen));
        AddMessage(new Data.Message(string.Format("Press {0} during the game to change the options.", RogueGame.s_KeyBindings.Get(PlayerCommand.OPTIONS_MODE)), 0, Color.LightGreen));
        AddMessage(new Data.Message("<press ENTER>", 0, Color.Yellow));
        RedrawPlayScreen();
        WaitEnter();
      }
      ClearMessages();
      ClearMessagesHistory();
      string msg1 = "Welcome to " + SetupConfig.GAME_NAME;
      string msg2 = "We hope you like Zombies";
      int len = (msg1.Length < msg2.Length ? msg2.Length : msg1.Length);
      int delta = msg2.Length-msg1.Length;
      string header = "".PadRight(len+4, '*');
      if (0<delta) {  // msg2 longer
        int half_delta = delta / 2;
        msg1 = msg1.PadLeft(msg1.Length+half_delta).PadRight(len);
      } else if (0>delta) { // msg1 longer
        delta = -delta;
        int half_delta = delta / 2;
        msg2 = msg2.PadLeft(msg2.Length + half_delta).PadRight(len);
      }
      AddMessage(new Data.Message(header, 0, Color.LightGreen));
      AddMessage(new Data.Message("* " + msg1 + " *", 0, Color.LightGreen));
      AddMessage(new Data.Message("* " + msg2 + " *", 0, Color.LightGreen));
      AddMessage(new Data.Message(header, 0, Color.LightGreen));
      AddMessage(new Data.Message(string.Format("Press {0} for help", RogueGame.s_KeyBindings.Get(PlayerCommand.HELP_MODE)), 0, Color.LightGreen));
      AddMessage(new Data.Message(string.Format("Press {0} to redefine keys", RogueGame.s_KeyBindings.Get(PlayerCommand.KEYBINDING_MODE)), 0, Color.LightGreen));
      AddMessage(new Data.Message("<press ENTER>", 0, Color.Yellow));
      RefreshPlayer();
      RedrawPlayScreen();
      WaitEnter();
      ClearMessages();
      AddMessage(new Data.Message(string.Format(isUndead ? "{0} rises..." : "{0} wakes up.", m_Player.Name), 0, Color.White));
      RedrawPlayScreen();
      Session.Get.World.ScheduleForAdvancePlay();   // simulation starts at district A1
      RestartSimThread();
    }

    private void HandleCredits()
    {
      m_MusicManager.StopAll();
      m_MusicManager.PlayLooping(GameMusics.SLEEP);
      m_UI.UI_Clear(Color.Black);
      DrawHeader();
      int gy1 = BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.Yellow, "Credits", 0, gy1, new Color?());
      gy1 += 2*BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.White, "Programming, Graphics & Music by Jacques Ruiz (roguedjack)", 0, gy1, new Color?());
      gy1 += 2*BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.White, "Programming", 0, gy1, new Color?());
      m_UI.UI_DrawString(Color.White, "- C# NET 3.5, Microsoft Visual C# 2010 Express", 256, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.White, "(zaimoni)", 0, gy1, new Color?());
      m_UI.UI_DrawString(Color.White, "- C# NET 4.6, Microsoft Visual C# 2015 Community", 256, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.White, "Graphics softwares", 0, gy1, new Color?());
      m_UI.UI_DrawString(Color.White, "- Inkscape, Paint.NET", 256, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.White, "Sound & Music softwares", 0, gy1, new Color?());
      m_UI.UI_DrawString(Color.White, "- GuitarPro 6, Audacity", 256, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.White, "Sound samples", 0, gy1, new Color?());
      m_UI.UI_DrawString(Color.White, "- http://www.sound-fishing.net  http://www.soundsnap.com/", 256, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.White, "Installer", 0, gy1, new Color?());
      m_UI.UI_DrawString(Color.White, "- NSIS", 256, gy1, new Color?());
      gy1 += 2*BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.White, "Contact", 0, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawString(Color.White, "Email      : roguedjack@yahoo.fr", 0, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawString(Color.White, "Blog       : http://roguesurvivor.blogspot.com/", 0, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawString(Color.White, "Fans Forum : http://roguesurvivor.proboards.com/", 0, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawString(Color.White, "(zaimoni)", 0, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawString(Color.White, "forum post : http://www.bay12forums.com/smf/index.php?topic=157701.0", 0, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawString(Color.White, "forum post : http://smf.cataclysmdda.com/index.php?topic=12463.0", 0, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawString(Color.White, "source code: https://bitbucket.org/zaimoni/roguesurvivor-revived", 0, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.White, "Thanks to the players for their feedback and eagerness to die!", 0, gy1, new Color?());
      DrawFootnote(Color.White, "ESC to leave");
      m_UI.UI_Repaint();
      WaitEscape();
    }

    private void HandleOptions(bool ingame)
    {
      GameOptions gameOptions = s_Options;
      GameOptions.IDs[] idsArray =  {
        GameOptions.IDs.UI_MUSIC,
        GameOptions.IDs.UI_MUSIC_VOLUME,
        GameOptions.IDs.UI_ANIM_DELAY,
        GameOptions.IDs.UI_SHOW_MINIMAP,
        GameOptions.IDs.UI_SHOW_PLAYER_TAG_ON_MINIMAP,
        GameOptions.IDs.UI_ADVISOR,
        GameOptions.IDs.UI_COMBAT_ASSISTANT,
        GameOptions.IDs.UI_SHOW_PLAYER_TARGETS,
        GameOptions.IDs.UI_SHOW_TARGETS,
//      GameOptions.IDs.GAME_SIM_THREAD,
//      GameOptions.IDs.GAME_SIMULATE_DISTRICTS,
//      GameOptions.IDs.GAME_SIMULATE_SLEEP,
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
      string[] entries = idsArray.Select(x => GameOptions.Name(x)).ToArray();

      Func<int,bool?> setup_handler = (currentChoice => {
        ApplyOptions(false);
        string[] values = idsArray.Select(x => s_Options.DescribeValue(Session.Get.GameMode, x)).ToArray();
        int gy;
        int gx = gy = 0;
        m_UI.UI_Clear(Color.Black);
        DrawHeader();
        gy += 14;
        m_UI.UI_DrawStringBold(Color.Yellow, string.Format("[{0}] - Options", Session.DescGameMode(Session.Get.GameMode)), 0, gy, new Color?());
        gy += 28;
        DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGreen, values, gx, ref gy, 400);
        gy += 14;
        m_UI.UI_DrawStringBold(Color.Red, "* Caution : increasing these values makes the game runs slower and saving/loading longer.", gx, gy, new Color?());
        gy += 14;
        gy += 14;
        m_UI.UI_DrawStringBold(Color.Yellow, string.Format("Difficulty Rating : {0}% as survivor / {1}% as undead.", (int)(100.0 * (double)Scoring.ComputeDifficultyRating(s_Options, DifficultySide.FOR_SURVIVOR, 0)), (int)(100.0 * (double)Scoring.ComputeDifficultyRating(s_Options, DifficultySide.FOR_UNDEAD, 0))), gx, gy, new Color?());
        gy += 14;
        m_UI.UI_DrawStringBold(Color.White, "Difficulty used for scoring automatically decrease with each reincarnation.", gx, gy, new Color?());
        gy += 28;
        DrawFootnote(Color.White, "cursor to move and change values, R to restore previous values, ESC to save and leave");
        return null;
      });
      Func<int, bool?> choice_handler = (currentChoice => {
        return null;
      });
      Func<Keys,int,bool?> failover_handler = ((k,currentChoice) => {
        switch (k) {
          case Keys.Left:
            switch (idsArray[currentChoice])
            {
              case GameOptions.IDs.UI_MUSIC:
                          s_Options.PlayMusic = !s_Options.PlayMusic;
                break;
              case GameOptions.IDs.UI_MUSIC_VOLUME:
                          s_Options.MusicVolume -= 5;
                break;
              case GameOptions.IDs.UI_SHOW_PLAYER_TAG_ON_MINIMAP:
                          s_Options.ShowPlayerTagsOnMinimap = !s_Options.ShowPlayerTagsOnMinimap;
                break;
              case GameOptions.IDs.UI_ANIM_DELAY:
                          s_Options.IsAnimDelayOn = !s_Options.IsAnimDelayOn;
                break;
              case GameOptions.IDs.UI_SHOW_MINIMAP:
                          s_Options.IsMinimapOn = !s_Options.IsMinimapOn;
                break;
              case GameOptions.IDs.UI_ADVISOR:
                          s_Options.IsAdvisorEnabled = !s_Options.IsAdvisorEnabled;
                break;
              case GameOptions.IDs.UI_COMBAT_ASSISTANT:
                          s_Options.IsCombatAssistantOn = !s_Options.IsCombatAssistantOn;
                break;
              case GameOptions.IDs.UI_SHOW_TARGETS:
                          s_Options.ShowTargets = !s_Options.ShowTargets;
                break;
              case GameOptions.IDs.UI_SHOW_PLAYER_TARGETS:
                          s_Options.ShowPlayerTargets = !s_Options.ShowPlayerTargets;
                break;
              case GameOptions.IDs.GAME_DISTRICT_SIZE:
                          s_Options.DistrictSize -= 5;
                break;
              case GameOptions.IDs.GAME_MAX_CIVILIANS:
                          s_Options.MaxCivilians -= 5;
                break;
              case GameOptions.IDs.GAME_MAX_DOGS:
                --s_Options.MaxDogs;
                break;
              case GameOptions.IDs.GAME_MAX_UNDEADS:
                          s_Options.MaxUndeads -= 10;
                break;
              case GameOptions.IDs.GAME_SIMULATE_DISTRICTS:
                break;
              case GameOptions.IDs.GAME_SIMULATE_SLEEP:
                break;
              case GameOptions.IDs.GAME_SIM_THREAD:
                break;
              case GameOptions.IDs.GAME_CITY_SIZE:
                --s_Options.CitySize;
                break;
              case GameOptions.IDs.GAME_NPC_CAN_STARVE_TO_DEATH:
                          s_Options.NPCCanStarveToDeath = !s_Options.NPCCanStarveToDeath;
                break;
              case GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE:
                          s_Options.ZombificationChance -= 5;
                break;
              case GameOptions.IDs.GAME_REVEAL_STARTING_DISTRICT:
                          s_Options.RevealStartingDistrict = !s_Options.RevealStartingDistrict;
                break;
              case GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION:
                if (Session.Get.GameMode != GameMode.GM_VINTAGE) s_Options.AllowUndeadsEvolution = !s_Options.AllowUndeadsEvolution;
                break;
              case GameOptions.IDs.GAME_DAY_ZERO_UNDEADS_PERCENT:
                          s_Options.DayZeroUndeadsPercent -= 5;
                break;
              case GameOptions.IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE:
                --s_Options.ZombieInvasionDailyIncrease;
                break;
              case GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE:
                          s_Options.StarvedZombificationChance -= 5;
                break;
              case GameOptions.IDs.GAME_MAX_REINCARNATIONS:
                --s_Options.MaxReincarnations;
                break;
              case GameOptions.IDs.GAME_REINCARNATE_AS_RAT:
                          s_Options.CanReincarnateAsRat = !s_Options.CanReincarnateAsRat;
                break;
              case GameOptions.IDs.GAME_REINCARNATE_TO_SEWERS:
                          s_Options.CanReincarnateToSewers = !s_Options.CanReincarnateToSewers;
                break;
              case GameOptions.IDs.GAME_REINC_LIVING_RESTRICTED:
                          s_Options.IsLivingReincRestricted = !s_Options.IsLivingReincRestricted;
                break;
              case GameOptions.IDs.GAME_PERMADEATH:
                          s_Options.IsPermadeathOn = !s_Options.IsPermadeathOn;
                break;
              case GameOptions.IDs.GAME_DEATH_SCREENSHOT:
                          s_Options.IsDeathScreenshotOn = !s_Options.IsDeathScreenshotOn;
                break;
              case GameOptions.IDs.GAME_AGGRESSIVE_HUNGRY_CIVILIANS:
                          s_Options.IsAggressiveHungryCiviliansOn = !s_Options.IsAggressiveHungryCiviliansOn;
                break;
              case GameOptions.IDs.GAME_NATGUARD_FACTOR:
                          s_Options.NatGuardFactor -= 10;
                break;
              case GameOptions.IDs.GAME_SUPPLIESDROP_FACTOR:
                          s_Options.SuppliesDropFactor -= 10;
                break;
              case GameOptions.IDs.GAME_UNDEADS_UPGRADE_DAYS:
                if (s_Options.ZombifiedsUpgradeDays != GameOptions.ZupDays.ONE) {
                              s_Options.ZombifiedsUpgradeDays = s_Options.ZombifiedsUpgradeDays - 1;
                  break;
                }
                break;
              case GameOptions.IDs.GAME_RATS_UPGRADE:
                if (Session.Get.GameMode != GameMode.GM_VINTAGE) s_Options.RatsUpgrade = !s_Options.RatsUpgrade;
                break;
              case GameOptions.IDs.GAME_SKELETONS_UPGRADE:
                if (Session.Get.GameMode != GameMode.GM_VINTAGE) s_Options.SkeletonsUpgrade = !s_Options.SkeletonsUpgrade;
                break;
              case GameOptions.IDs.GAME_SHAMBLERS_UPGRADE:
                if (Session.Get.GameMode != GameMode.GM_VINTAGE) s_Options.ShamblersUpgrade = !s_Options.ShamblersUpgrade;
                break;
            }
            break;
          case Keys.Right:
            switch (idsArray[currentChoice]) {
              case GameOptions.IDs.UI_MUSIC:
                          s_Options.PlayMusic = !s_Options.PlayMusic;
                break;
              case GameOptions.IDs.UI_MUSIC_VOLUME:
                          s_Options.MusicVolume += 5;
                break;
              case GameOptions.IDs.UI_SHOW_PLAYER_TAG_ON_MINIMAP:
                          s_Options.ShowPlayerTagsOnMinimap = !s_Options.ShowPlayerTagsOnMinimap;
                break;
              case GameOptions.IDs.UI_ANIM_DELAY:
                          s_Options.IsAnimDelayOn = !s_Options.IsAnimDelayOn;
                break;
              case GameOptions.IDs.UI_SHOW_MINIMAP:
                          s_Options.IsMinimapOn = !s_Options.IsMinimapOn;
                break;
              case GameOptions.IDs.UI_ADVISOR:
                          s_Options.IsAdvisorEnabled = !s_Options.IsAdvisorEnabled;
                break;
              case GameOptions.IDs.UI_COMBAT_ASSISTANT:
                          s_Options.IsCombatAssistantOn = !s_Options.IsCombatAssistantOn;
                break;
              case GameOptions.IDs.UI_SHOW_TARGETS:
                          s_Options.ShowTargets = !s_Options.ShowTargets;
                break;
              case GameOptions.IDs.UI_SHOW_PLAYER_TARGETS:
                          s_Options.ShowPlayerTargets = !s_Options.ShowPlayerTargets;
                break;
              case GameOptions.IDs.GAME_DISTRICT_SIZE:
                          s_Options.DistrictSize += 5;
                break;
              case GameOptions.IDs.GAME_MAX_CIVILIANS:
                          s_Options.MaxCivilians += 5;
                break;
              case GameOptions.IDs.GAME_MAX_DOGS:
                ++s_Options.MaxDogs;
                break;
              case GameOptions.IDs.GAME_MAX_UNDEADS:
                          s_Options.MaxUndeads += 10;
                break;
              case GameOptions.IDs.GAME_SIMULATE_DISTRICTS:
                break;
              case GameOptions.IDs.GAME_SIMULATE_SLEEP:
                break;
              case GameOptions.IDs.GAME_SIM_THREAD:
                break;
              case GameOptions.IDs.GAME_CITY_SIZE:
                ++s_Options.CitySize;
                break;
              case GameOptions.IDs.GAME_NPC_CAN_STARVE_TO_DEATH:
                          s_Options.NPCCanStarveToDeath = !s_Options.NPCCanStarveToDeath;
                break;
              case GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE:
                          s_Options.ZombificationChance += 5;
                break;
              case GameOptions.IDs.GAME_REVEAL_STARTING_DISTRICT:
                          s_Options.RevealStartingDistrict = !s_Options.RevealStartingDistrict;
                break;
              case GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION:
                if (Session.Get.GameMode != GameMode.GM_VINTAGE) s_Options.AllowUndeadsEvolution = !s_Options.AllowUndeadsEvolution;
                break;
              case GameOptions.IDs.GAME_DAY_ZERO_UNDEADS_PERCENT:
                          s_Options.DayZeroUndeadsPercent += 5;
                break;
              case GameOptions.IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE:
                ++s_Options.ZombieInvasionDailyIncrease;
                break;
              case GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE:
                          s_Options.StarvedZombificationChance += 5;
                break;
              case GameOptions.IDs.GAME_MAX_REINCARNATIONS:
                ++s_Options.MaxReincarnations;
                break;
              case GameOptions.IDs.GAME_REINCARNATE_AS_RAT:
                          s_Options.CanReincarnateAsRat = !s_Options.CanReincarnateAsRat;
                break;
              case GameOptions.IDs.GAME_REINCARNATE_TO_SEWERS:
                          s_Options.CanReincarnateToSewers = !s_Options.CanReincarnateToSewers;
                break;
              case GameOptions.IDs.GAME_REINC_LIVING_RESTRICTED:
                          s_Options.IsLivingReincRestricted = !s_Options.IsLivingReincRestricted;
                break;
              case GameOptions.IDs.GAME_PERMADEATH:
                          s_Options.IsPermadeathOn = !s_Options.IsPermadeathOn;
                break;
              case GameOptions.IDs.GAME_DEATH_SCREENSHOT:
                          s_Options.IsDeathScreenshotOn = !s_Options.IsDeathScreenshotOn;
                break;
              case GameOptions.IDs.GAME_AGGRESSIVE_HUNGRY_CIVILIANS:
                          s_Options.IsAggressiveHungryCiviliansOn = !s_Options.IsAggressiveHungryCiviliansOn;
                break;
              case GameOptions.IDs.GAME_NATGUARD_FACTOR:
                          s_Options.NatGuardFactor += 10;
                break;
              case GameOptions.IDs.GAME_SUPPLIESDROP_FACTOR:
                          s_Options.SuppliesDropFactor += 10;
                break;
              case GameOptions.IDs.GAME_UNDEADS_UPGRADE_DAYS:
                if (s_Options.ZombifiedsUpgradeDays != GameOptions.ZupDays.OFF)
                {
                              s_Options.ZombifiedsUpgradeDays = s_Options.ZombifiedsUpgradeDays + 1;
                  break;
                }
                break;
              case GameOptions.IDs.GAME_RATS_UPGRADE:
				if (Session.Get.GameMode != GameMode.GM_VINTAGE) s_Options.RatsUpgrade = !s_Options.RatsUpgrade;
                break;
              case GameOptions.IDs.GAME_SKELETONS_UPGRADE:
			    if (Session.Get.GameMode != GameMode.GM_VINTAGE) s_Options.SkeletonsUpgrade = !s_Options.SkeletonsUpgrade;
                break;
              case GameOptions.IDs.GAME_SHAMBLERS_UPGRADE:
				if (Session.Get.GameMode != GameMode.GM_VINTAGE) s_Options.ShamblersUpgrade = !s_Options.ShamblersUpgrade;
                break;
            }
            break;
          case Keys.R:
                  s_Options = gameOptions;
            break;
        }
        return null;
      });
      ChoiceMenu(choice_handler, setup_handler, entries.Length, failover_handler);
      ApplyOptions(false);
      SaveOptions();
    }

    private void HandleRedefineKeys()
    {
      // need to maintain: label to command mapping
      // then generate current keybindings
      // then read off position from reference array
      // screen layout may fail with more than 51 entries
      KeyValuePair<string, PlayerCommand>[] command_labels = new KeyValuePair<string, PlayerCommand>[] {
          new KeyValuePair< string,PlayerCommand >("Move N", PlayerCommand.MOVE_N),
          new KeyValuePair< string,PlayerCommand >("Move NE", PlayerCommand.MOVE_NE),
          new KeyValuePair< string,PlayerCommand >("Move E", PlayerCommand.MOVE_E),
          new KeyValuePair< string,PlayerCommand >("Move SE", PlayerCommand.MOVE_SE),
          new KeyValuePair< string,PlayerCommand >("Move S", PlayerCommand.MOVE_S),
          new KeyValuePair< string,PlayerCommand >("Move SW", PlayerCommand.MOVE_SW),
          new KeyValuePair< string,PlayerCommand >("Move W", PlayerCommand.MOVE_W),
          new KeyValuePair< string,PlayerCommand >("Move NW", PlayerCommand.MOVE_NW),
          new KeyValuePair< string,PlayerCommand >("Wait", PlayerCommand.WAIT_OR_SELF),
#if SUICIDE_BY_LONG_WAIT
          new KeyValuePair< string,PlayerCommand >("Wait 1 hour", PlayerCommand.WAIT_LONG),
#endif
          new KeyValuePair< string,PlayerCommand >("Abandon Game", PlayerCommand.ABANDON_GAME),
          new KeyValuePair< string,PlayerCommand >("Advisor Hint", PlayerCommand.ADVISOR),
          new KeyValuePair< string,PlayerCommand >("Barricade", PlayerCommand.BARRICADE_MODE),
          new KeyValuePair< string,PlayerCommand >("Break", PlayerCommand.BREAK_MODE),
          new KeyValuePair< string,PlayerCommand >("Build Large Fortification", PlayerCommand.BUILD_LARGE_FORTIFICATION),
          new KeyValuePair< string,PlayerCommand >("Build Small Fortification", PlayerCommand.BUILD_SMALL_FORTIFICATION),
          new KeyValuePair< string,PlayerCommand >("City Info", PlayerCommand.CITY_INFO),
          new KeyValuePair< string,PlayerCommand >("Item Info", PlayerCommand.ITEM_INFO),
          new KeyValuePair< string,PlayerCommand >("Close", PlayerCommand.CLOSE_DOOR),
          new KeyValuePair< string,PlayerCommand >("Fire", PlayerCommand.FIRE_MODE),
          new KeyValuePair< string,PlayerCommand >("Give", PlayerCommand.GIVE_ITEM),
          new KeyValuePair< string,PlayerCommand >("Help", PlayerCommand.HELP_MODE),
          new KeyValuePair< string,PlayerCommand >("Hints screen", PlayerCommand.HINTS_SCREEN_MODE),
          new KeyValuePair< string,PlayerCommand >("Initiate Trade", PlayerCommand.INITIATE_TRADE),
          new KeyValuePair< string,PlayerCommand >("Item 1 slot", PlayerCommand.ITEM_SLOT_0),
          new KeyValuePair< string,PlayerCommand >("Item 2 slot", PlayerCommand.ITEM_SLOT_1),
          new KeyValuePair< string,PlayerCommand >("Item 3 slot", PlayerCommand.ITEM_SLOT_2),
          new KeyValuePair< string,PlayerCommand >("Item 4 slot", PlayerCommand.ITEM_SLOT_3),
          new KeyValuePair< string,PlayerCommand >("Item 5 slot", PlayerCommand.ITEM_SLOT_4),
          new KeyValuePair< string,PlayerCommand >("Item 6 slot", PlayerCommand.ITEM_SLOT_5),
          new KeyValuePair< string,PlayerCommand >("Item 7 slot", PlayerCommand.ITEM_SLOT_6),
          new KeyValuePair< string,PlayerCommand >("Item 8 slot", PlayerCommand.ITEM_SLOT_7),
          new KeyValuePair< string,PlayerCommand >("Item 9 slot", PlayerCommand.ITEM_SLOT_8),
          new KeyValuePair< string,PlayerCommand >("Item 10 slot", PlayerCommand.ITEM_SLOT_9),
          new KeyValuePair< string,PlayerCommand >("Lead", PlayerCommand.LEAD_MODE),
          new KeyValuePair< string,PlayerCommand >("Load Game", PlayerCommand.LOAD_GAME),
          new KeyValuePair< string,PlayerCommand >("Mark Enemies", PlayerCommand.MARK_ENEMIES_MODE),
          new KeyValuePair< string,PlayerCommand >("Messages Log", PlayerCommand.MESSAGE_LOG),
          new KeyValuePair< string,PlayerCommand >("Options", PlayerCommand.OPTIONS_MODE),
          new KeyValuePair< string,PlayerCommand >("Order", PlayerCommand.ORDER_MODE),
          new KeyValuePair< string,PlayerCommand >("Push", PlayerCommand.PUSH_MODE),
          new KeyValuePair< string,PlayerCommand >("Quit Game", PlayerCommand.QUIT_GAME),
          new KeyValuePair< string,PlayerCommand >("Redefine Keys", PlayerCommand.KEYBINDING_MODE),
          new KeyValuePair< string,PlayerCommand >("Run", PlayerCommand.RUN_TOGGLE),
          new KeyValuePair< string,PlayerCommand >("Save Game", PlayerCommand.SAVE_GAME),
          new KeyValuePair< string,PlayerCommand >("Screenshot", PlayerCommand.SCREENSHOT),
          new KeyValuePair< string,PlayerCommand >("Shout", PlayerCommand.SHOUT),
          new KeyValuePair< string,PlayerCommand >("Sleep", PlayerCommand.SLEEP),
          new KeyValuePair< string,PlayerCommand >("Switch Place", PlayerCommand.SWITCH_PLACE),
          new KeyValuePair< string,PlayerCommand >("Use Exit", PlayerCommand.USE_EXIT),
          new KeyValuePair< string,PlayerCommand >("Use Spray", PlayerCommand.USE_SPRAY),
        };

      string[] entries = command_labels.Select(x => x.Key).ToArray();
      const int gx = 0;
      int gy = 0;

      Func<int,bool?> setup_handler = (currentChoice => {
        string[] values = command_labels.Select(x => RogueGame.s_KeyBindings.Get(x.Value).ToString()).ToArray();

        gy = 0;
        m_UI.UI_Clear(Color.Black);
        DrawHeader();
        gy += 14;
        m_UI.UI_DrawStringBold(Color.Yellow, "Redefine keys", 0, gy, new Color?());
        gy += 14;
        DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGreen, values, gx, ref gy, 256);
        if (s_KeyBindings.CheckForConflict()) {
          m_UI.UI_DrawStringBold(Color.Red, "Conflicting keys. Please redefine the keys so the commands don't overlap.", gx, gy, new Color?());
          gy += 14;
        }
        DrawFootnote(Color.White, "cursor to move, ENTER to rebind a key, ESC to save and leave");
        return null;
      });
      Func<int, bool?> choice_handler = (currentChoice => {
        m_UI.UI_DrawStringBold(Color.Yellow, string.Format("rebinding {0}, press the new key.", command_labels[currentChoice].Key), gx, gy, new Color?());
        m_UI.UI_Repaint();
        Keys key = Keys.None;
        while(true) {
          KeyEventArgs keyEventArgs = m_UI.UI_WaitKey();
          if (keyEventArgs.KeyCode != Keys.ShiftKey && keyEventArgs.KeyCode != Keys.ControlKey && !keyEventArgs.Alt) {
            key = keyEventArgs.KeyData;
            break;
          }
        };
        if (0>currentChoice || command_labels.Length<=currentChoice) throw new InvalidOperationException("unhandled selected");
        PlayerCommand command = command_labels[currentChoice].Value;
        RogueGame.s_KeyBindings.Set(command, key);
        return null;
      });
      do ChoiceMenu(choice_handler, setup_handler, entries.Length);
      while(s_KeyBindings.CheckForConflict());
      SaveKeybindings();
    }

    private void FindPlayer()
    {
       Actor tmp = Session.Get.CurrentMap.FindPlayer;
       if (null != tmp) {
         m_Player = tmp;
         m_Player.Controller.UpdateSensors();
         ComputeViewRect(m_Player.Location.Position);
         RedrawPlayScreen();
         return;
       }
       // check all maps in current district
       tmp = Session.Get.CurrentMap.District.FindPlayer(Session.Get.CurrentMap);
       if (null != tmp) {
         m_Player = tmp;
         return;
       }

       // check all other districts
       foreach(District tmp2 in Session.Get.World.PlayerDistricts) {
         tmp = tmp2.FindPlayer(null);
         if (null != tmp) {
           m_Player = tmp;
           return;
         }
       }
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void AdvancePlay(District district, RogueGame.SimFlags sim)
    {
      bool isNight1 = Session.Get.WorldTime.IsNight;
      DayPhase phase1 = Session.Get.WorldTime.Phase;
#if DATAFLOW_TRACE
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "District: "+district.Name);
#endif

      lock(district) {
      bool IsPCdistrict = (0 < district.PlayerCount);
      foreach(Map current in district.Maps) {
        // not processing secret maps used to be a micro-optimization; now a hang bug
        while(!current.IsSecret && null != current.NextActorToAct) {
          AdvancePlay(current, sim);
          if (district == Session.Get.CurrentMap.District) { // Bay12/jorgene0: do not let simulation thread process reincarnation
            if (0>=Session.Get.World.PlayerCount) HandleReincarnation();
          }
          if (!m_IsGameRunning || m_HasLoadedGame || (IsPCdistrict && 0>=Session.Get.World.PlayerCount)) return;
        }
      }

      if (district.ReadyForNextTurn) {
        foreach(Map current in district.Maps) {
          NextMapTurn(current, sim);
        }
      }

      // XXX message generation wrappers do not have access to map time, only world time
      // XXX this set of messages must execute only once
      // XXX the displayed turn on the message must agree with the displayed turn on the screen
      // XXX thus, displayed turn on screen for non-first PC districts is greater than the map time by one
      if (district == Session.Get.CurrentMap.District && Session.Get.CurrentMap.LocalTime.TurnCounter > Session.Get.WorldTime.TurnCounter) {
        ++Session.Get.WorldTime.TurnCounter;
        bool isNight2 = Session.Get.WorldTime.IsNight;
        DayPhase phase2 = Session.Get.WorldTime.Phase;
        if (isNight1 && !isNight2) {
          AddMessage(new Data.Message("The sun is rising again for you...", Session.Get.WorldTime.TurnCounter, DAY_COLOR));
          OnNewDay();
        } else if (!isNight1 && isNight2) {
          AddMessage(new Data.Message("Night is falling upon you...", Session.Get.WorldTime.TurnCounter, NIGHT_COLOR));
          OnNewNight();
        } else if (phase1 != phase2)
          AddMessage(new Data.Message(string.Format("Time passes, it is now {0}...", DescribeDayPhase(phase2)), Session.Get.WorldTime.TurnCounter, isNight2 ? NIGHT_COLOR : DAY_COLOR));
      }

      if (CheckForEvent_ZombieInvasion(district.EntryMap)) FireEvent_ZombieInvasion(district.EntryMap);
      if (CheckForEvent_RefugeesWave(district.EntryMap)) FireEvent_RefugeesWave(district);
      if (CheckForEvent_NationalGuard(district.EntryMap)) FireEvent_NationalGuard(district.EntryMap);
      if (CheckForEvent_ArmySupplies(district.EntryMap)) FireEvent_ArmySupplies(district.EntryMap);
      if (CheckForEvent_BikersRaid(district.EntryMap)) FireEvent_BikersRaid(district.EntryMap);
      if (CheckForEvent_GangstasRaid(district.EntryMap)) FireEvent_GangstasRaid(district.EntryMap);
      if (CheckForEvent_BlackOpsRaid(district.EntryMap)) FireEvent_BlackOpsRaid(district.EntryMap);
      if (CheckForEvent_BandOfSurvivors(district.EntryMap)) FireEvent_BandOfSurvivors(district.EntryMap);
      if (CheckForEvent_SewersInvasion(district.SewersMap)) FireEvent_SewersInvasion(district.SewersMap);
      } // end lock(district)

#if DATAFLOW_TRACE
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "District finished: "+district.Name);
#endif
    }

    static private void NotifyOrderablesAI(Map map, RaidType raid, Point position)
    {
      foreach (Actor actor in map.Actors) {
        (actor.Controller as OrderableAI)?.OnRaid(raid, new Location(map, position), map.LocalTime.TurnCounter);
      }
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void AdvancePlay(Map map, RogueGame.SimFlags sim)
    {
#if DATAFLOW_TRACE
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "Map: "+map.Name);
#endif
      if (map.IsSecret) return; // undiscovered CHAR base is in stasis
//    if (map.IsSecret) { map.LocalTime.TurnCounter++; return; } // use turn overflow crash to test multi-threading error handling

      Actor nextActorToAct = map.NextActorToAct;
      if (nextActorToAct == null) return;

      // We actually may do something.  Do a partial solution to dropped messages here in the multi-PC case
      if (map != m_Player.Location.Map && 0 < map.PlayerCount && !nextActorToAct.IsPlayer) {
        Actor tmp = map.FindPlayer;
        if (null != tmp)
          {
          m_Player = tmp;
          Session.Get.CurrentMap = map;  // multi-PC support
          m_Player.Controller.UpdateSensors();
          ComputeViewRect(m_Player.Location.Position);
          RedrawPlayScreen();
          }
      }

      if (nextActorToAct.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "Actor: "+ nextActorToAct.Name);
      nextActorToAct.PreviousStaminaPoints = nextActorToAct.StaminaPoints;
      if (nextActorToAct.Controller == null)
#if DEBUG
        throw new InvalidOperationException("nextActorToAct.Controller == null");
#else
        nextActorToAct.SpendActionPoints(Rules.BASE_ACTION_COST);
#endif
      else if (nextActorToAct.IsPlayer) {
        HandlePlayerActor(nextActorToAct);
        if (!m_IsGameRunning || m_HasLoadedGame || 0>=Session.Get.World.PlayerCount) return;
        CheckSpecialPlayerEventsAfterAction(nextActorToAct);
      }
      else HandleAiActor(nextActorToAct);
      nextActorToAct.AfterAction();
    }

    private void NextMapTurn(Map map, RogueGame.SimFlags sim)
    {
#if DATAFLOW_TRACE
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "Next turn, Map: "+map.Name);
#endif
      if (map.IsSecret) {   // undiscovered CHAR base is in stasis
        ++map.LocalTime.TurnCounter;
        return;
      }

            ////////////////////////////////////////
            // (the following are skipped in lodetail turns)
            // 0. Raise the deads; Check infections (non STD)
            // 1. Update odors.
            //      1.1 Odor suppression/generation.
            //      1.2 Odors decay.
            //      1.3 Drop new scents.
            // 2. Regen actors AP & STA
            // 3. Stop tired actors from running.
            // 4. Actor gauges & states :
            //    Hunger, Sleep, Sanity, Leader Trust.
            //      4.1 May kill starved actors.
            //      4.2 Handle sleeping actors.
            //      4.3 Exhausted actors might collapse.
            // 5. Check batteries : lights, trackers.
            // 6. Check explosives.
            // 7. Check fires.
            // (the following are always performed)
            // - Check timers.
            // - Advance local time.
            // - Check for NPC upgrade.
            ////////////////////////////////////////

      if ((sim & RogueGame.SimFlags.LODETAIL_TURN) == RogueGame.SimFlags.NOT_SIMULATING) {
        if (Session.Get.HasCorpses && map.CountCorpses > 0) {
#region corpses: decide who zombify or rots.
          List<Corpse> corpseList1 = new List<Corpse>(map.CountCorpses);
          List<Corpse> corpseList2 = new List<Corpse>(map.CountCorpses);
          foreach (Corpse corpse in map.Corpses) {
            if (m_Rules.RollChance(Rules.CorpseZombifyChance(corpse, map.LocalTime, true))) {
              corpseList1.Add(corpse);
            } else if (corpse.TakeDamage(Rules.CorpseDecayPerTurn(corpse))) {
              corpseList2.Add(corpse);
            }
          }
          if (corpseList1.Count > 0) {
            List<Corpse> corpseList3 = new List<Corpse>(corpseList1.Count);
            foreach (Corpse corpse in corpseList1) {
              if (!map.HasActorAt(corpse.Position)) {
                corpseList3.Add(corpse);
                Zombify(null, corpse.DeadGuy, false);
                if (ForceVisibleToPlayer(map, corpse.Position)) {
                  AddMessage(new Data.Message(string.Format("The corpse of {0} rise again!!", corpse.DeadGuy.Name), map.LocalTime.TurnCounter, Color.Red));
                  m_MusicManager.Play(GameSounds.UNDEAD_RISE);
                }
              }
            }
            foreach (Corpse c in corpseList3) map.Destroy(c);
          }
          if (m_Player != null && m_Player.Location.Map == map) {
            foreach (Corpse c in corpseList2) {
              map.Destroy(c);
              if (ForceVisibleToPlayer(map, c.Position))
                AddMessage(new Data.Message(string.Format("The corpse of {0} turns into dust.", c.DeadGuy.Name), map.LocalTime.TurnCounter, Color.Purple));
            }
          }
#endregion
        }
        if (Session.Get.HasInfection) {
#region Infection effects
          List<Actor> actorList = null;
          foreach (Actor actor in map.Actors) {
            if (actor.Infection >= Rules.INFECTION_LEVEL_1_WEAK && !actor.Model.Abilities.IsUndead) {
              int infectionPercent = actor.InfectionPercent;
              if (m_Rules.Roll(0, 1000) < Rules.InfectionEffectTriggerChance1000(infectionPercent)) {
                bool player = ForceVisibleToPlayer(actor);
                bool flag3 = actor == m_Player;
                if (actor.IsSleeping) DoWakeUp(actor);
                bool flag4 = false;
                if (infectionPercent >= Rules.INFECTION_LEVEL_5_DEATH) flag4 = true;
                else if (infectionPercent >= Rules.INFECTION_LEVEL_4_BLEED) {
                  DoVomit(actor);
                  actor.HitPoints -= Rules.INFECTION_LEVEL_4_BLEED_HP;
                  if (player) {
                    if (flag3) ClearMessages();
                    AddMessage(MakeMessage(actor, string.Format("{0} blood.", Conjugate(actor, VERB_VOMIT)), Color.Purple));
                    if (flag3) {
                      AddMessagePressEnter();
                      ClearMessages();
                    }
                  }
                  if (actor.HitPoints <= 0) flag4 = true;
                } else if (infectionPercent >= Rules.INFECTION_LEVEL_3_VOMIT) {
                  DoVomit(actor);
                  if (player) {
                    if (flag3) ClearMessages();
                    AddMessage(MakeMessage(actor, string.Format("{0}.", Conjugate(actor, VERB_VOMIT)), Color.Purple));
                    if (flag3) {
                      AddMessagePressEnter();
                      ClearMessages();
                    }
                  }
                } else if (infectionPercent >= Rules.INFECTION_LEVEL_2_TIRED) {
                  actor.SpendStaminaPoints(Rules.INFECTION_LEVEL_2_TIRED_STA);
                  actor.Drowse(Rules.INFECTION_LEVEL_2_TIRED_SLP);
                  if (player) {
                    if (flag3) ClearMessages();
                    AddMessage(MakeMessage(actor, string.Format("{0} sick and tired.", Conjugate(actor, VERB_FEEL)), Color.Purple));
                    if (flag3) {
                      AddMessagePressEnter();
                      ClearMessages();
                    }
                  }
                } else if (infectionPercent >= Rules.INFECTION_LEVEL_1_WEAK) {
                  actor.SpendStaminaPoints(Rules.INFECTION_LEVEL_1_WEAK_STA);
                  if (player) {
                    if (flag3) ClearMessages();
                    AddMessage(MakeMessage(actor, string.Format("{0} sick and weak.", Conjugate(actor, VERB_FEEL)), Color.Purple));
                    if (flag3) {
                      AddMessagePressEnter();
                      ClearMessages();
                    }
                  }
                }
                if (flag4) {
                  if (actorList == null) actorList = new List<Actor>(map.CountActors);
                  actorList.Add(actor);
                }
              }
            }
          }
          if (actorList != null) {
            foreach (Actor actor in actorList) {
              if (ForceVisibleToPlayer(actor))
                AddMessage(MakeMessage(actor, string.Format("{0} of infection!", Conjugate(actor, VERB_DIE))));
              KillActor(null, actor, "infection");
              if (actor.IsPlayer) {
                map.TryRemoveCorpseOf(actor);
                Zombify(null, actor, false);
                AddMessage(MakeMessage(actor, Conjugate(actor, "turn") + " into a Zombie!"));
                RedrawPlayScreen();
                AnimDelay(DELAY_LONG);
              }
            }
          }
#endregion
        }

#region 1. Update odors.
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

        int odorDecayRate = 1;
        if (map == map.District.SewersMap)
          odorDecayRate += 2;
        else if (map.Lighting == Lighting.OUTSIDE) {
          switch (Session.Get.World.Weather)
          {
          case Weather.CLEAR:
          case Weather.CLOUDY: break;
          case Weather.RAIN:
            ++odorDecayRate;
            break;
          case Weather.HEAVY_RAIN:
            odorDecayRate += 2;
            break;
          default: throw new ArgumentOutOfRangeException("unhandled weather");
          }
        }

        foreach (OdorScent scent in new List<OdorScent>(map.Scents)) {
          map.ModifyScentAt(scent.Odor, -odorDecayRate, scent.Position);
          if (scent.Strength < 1) map.RemoveScent(scent);
        }
#endregion
        // 2. Regen actors AP & STA
        foreach (Actor actor in map.Actors) actor.PreTurnStart();
        map.CheckNextActorIndex = 0;
#region 3. Stop tired actors from running.
        foreach (Actor actor in map.Actors) {
          if (actor.IsRunning && actor.StaminaPoints < Actor.STAMINA_MIN_FOR_ACTIVITY) {
            actor.IsRunning = false;
            if (actor == m_Player) {
              AddMessage(MakeMessage(actor, string.Format("{0} too tired to continue running!", Conjugate(actor, VERB_BE))));
              RedrawPlayScreen();
            }
          }
        }
#endregion
#region 4. Actor gauges & states
        List<Actor> actorList1 = null;
        foreach (Actor actor in map.Actors)
        {
#region hunger & rot
          if (actor.Model.Abilities.HasToEat)
          {
            actor.Appetite(1);
            if (actor.IsStarving && m_Rules.RollChance(Rules.FOOD_STARVING_DEATH_CHANCE) && (actor.IsPlayer || s_Options.NPCCanStarveToDeath))
            {
              if (actorList1 == null)
                actorList1 = new List<Actor>();
              actorList1.Add(actor);
            }
          }
          else if (actor.Model.Abilities.IsRotting)
          {
            actor.Appetite(1);
            if (actor.IsRotStarving && m_Rules.Roll(0, 1000) < Rules.ROT_STARVING_HP_CHANCE)
            {
              if (ForceVisibleToPlayer(actor))
                AddMessage(MakeMessage(actor, "is rotting away."));
              if (--actor.HitPoints <= 0) {
                  if (actorList1 == null) actorList1 = new List<Actor>();
                  actorList1.Add(actor);
              }
            }
            else if (actor.IsRotHungry && m_Rules.Roll(0, 1000) < Rules.ROT_HUNGRY_SKILL_CHANCE)
              DoLooseRandomSkill(actor);
          }
#endregion
          if (actor.Model.Abilities.HasToSleep) {
#region sleep.
            if (actor.IsSleeping) {
              if (actor.IsDisturbed && m_Rules.RollChance(Rules.SANITY_NIGHTMARE_CHANCE)) {
                DoWakeUp(actor);
                DoShout(actor, "NO! LEAVE ME ALONE!");
                actor.Drowse(Rules.SANITY_NIGHTMARE_SLP_LOSS);
                actor.SpendSanity(Rules.SANITY_NIGHTMARE_SAN_LOSS);
                if (ForceVisibleToPlayer(actor))
                  AddMessage(MakeMessage(actor, string.Format("{0} from a horrible nightmare!", Conjugate(actor, VERB_WAKE_UP))));
                if (actor.IsPlayer) {
                  m_MusicManager.StopAll();
                  m_MusicManager.Play(GameSounds.NIGHTMARE);
                }
              }
            } else {
              actor.Drowse(map.LocalTime.IsNight ? 2 : 1);
            }
            if (actor.IsSleeping) {
#region 4.2 Handle sleeping actors.
              bool isOnCouch = actor.IsOnCouch;
              actor.Activity = Activity.SLEEPING;
              actor.Rest(Rules.ActorSleepRegen(actor, isOnCouch));
              if (actor.HitPoints < actor.MaxHPs && m_Rules.RollChance((isOnCouch ? Rules.SLEEP_ON_COUCH_HEAL_CHANCE : 0) + Rules.ActorHealChanceBonus(actor)))
                actor.RegenHitPoints(Rules.SLEEP_HEAL_HITPOINTS);
              if (actor.IsHungry || actor.SleepPoints >= actor.MaxSleep)
                DoWakeUp(actor);
              else if (actor.IsPlayer) {
                if (m_MusicManager.IsPaused(GameMusics.SLEEP))
                  m_MusicManager.ResumeLooping(GameMusics.SLEEP);
                AddMessage(new Data.Message("...zzZZZzzZ...", map.LocalTime.TurnCounter, Color.DarkCyan));
                RedrawPlayScreen();
                Thread.Sleep(10);
              } else if (m_Rules.RollChance(MESSAGE_NPC_SLEEP_SNORE_CHANCE) && ForceVisibleToPlayer(actor)) {
                AddMessage(MakeMessage(actor, string.Format("{0}.", Conjugate(actor, VERB_SNORE))));
                RedrawPlayScreen();
              }
#endregion
            }
            if (actor.IsExhausted && m_Rules.RollChance(Rules.SLEEP_EXHAUSTION_COLLAPSE_CHANCE)) {
#region 4.3 Exhausted actors might collapse.
              DoStartSleeping(actor);
              if (ForceVisibleToPlayer(actor)) {
                AddMessage(MakeMessage(actor, string.Format("{0} from exhaustion !!", Conjugate(actor, VERB_COLLAPSE))));
                RedrawPlayScreen();
              }
              if (actor == m_Player) {
                m_Player.Controller.UpdateSensors();
                ComputeViewRect(m_Player.Location.Position);
                RedrawPlayScreen();
              }
#endregion
            }
#endregion
          }
          actor.SpendSanity(1);
          if (actor.HasLeader) {
#region leader trust & leader/follower bond.
            ModifyActorTrustInLeader(actor, Rules.ActorTrustIncrease(actor.Leader), false);
            if (actor.HasBondWith(actor.Leader) && m_Rules.RollChance(Rules.SANITY_RECOVER_BOND_CHANCE)) {
              actor.RegenSanity(Rules.ActorSanRegenValue(actor, Rules.SANITY_RECOVER_BOND));
              actor.Leader.RegenSanity(Rules.ActorSanRegenValue(actor.Leader, Rules.SANITY_RECOVER_BOND));
              if (ForceVisibleToPlayer(actor))
                AddMessage(MakeMessage(actor, string.Format("{0} reassured knowing {1} is with {2}.", Conjugate(actor, VERB_FEEL), actor.Leader.Name, HimOrHer(actor))));
              if (ForceVisibleToPlayer(actor.Leader))
                AddMessage(MakeMessage(actor.Leader, string.Format("{0} reassured knowing {1} is with {2}.", Conjugate(actor.Leader, VERB_FEEL), actor.Name, HimOrHer(actor.Leader))));
            }
#endregion
          }
        }
#region Kill (zombify) starved actors.
        if (actorList1 != null) {
          foreach (Actor actor in actorList1) {
            if (ForceVisibleToPlayer(actor)) {
              AddMessage(MakeMessage(actor, string.Format("{0} !!", Conjugate(actor, VERB_DIE_FROM_STARVATION))));
              RedrawPlayScreen();
            }
            KillActor(null, actor, "starvation");
            if (!actor.Model.Abilities.IsUndead && Session.Get.HasImmediateZombification && m_Rules.RollChance(s_Options.StarvedZombificationChance)) {
              map.TryRemoveCorpseOf(actor);
              Zombify(null, actor, false);
              if (ForceVisibleToPlayer(actor)) {
                AddMessage(MakeMessage(actor, string.Format("{0} into a Zombie!", Conjugate(actor, "turn"))));
                RedrawPlayScreen();
                AnimDelay(DELAY_LONG);
              }
            }
          }
        }
#endregion
#endregion
#region 5. Check batteries : lights, trackers.
        // lights and normal trackers
        foreach (Actor actor in map.Actors) {
          Item equippedItem = actor.GetEquippedItem(DollPart.LEFT_HAND);
          if (null != equippedItem && equippedItem is BatteryPowered tmp) {
            if (0 < tmp.Batteries) {
              --tmp.Batteries;
               if (tmp.Batteries <= 0 && ForceVisibleToPlayer(actor))
                 AddMessage(MakeMessage(actor, string.Format((equippedItem is ItemLight ? ": {0} light goes off." : ": {0} goes off."), equippedItem.TheName)));
            }
          }
        }
        // police radios
        foreach (Actor actor in map.Actors) {
          if (actor.GetEquippedItem(DollPart.HIP_HOLSTER) is ItemTracker tracker && tracker.Batteries > 0) {
            --tracker.Batteries;
            if (tracker.Batteries <= 0 && ForceVisibleToPlayer(actor))
              AddMessage(MakeMessage(actor, string.Format(": {0} goes off.", tracker.TheName)));
          }
        }
#endregion

#region 6. Check explosives.
        bool hasExplosivesToExplode = false;
        Action<ItemPrimedExplosive> expire_exp = (exp => {
          if (0 >= --exp.FuseTimeLeft) hasExplosivesToExplode = true;
        });
#region 6.1 Update fuses.
        foreach (Inventory inv in map.GroundInventories) {
          inv.GetItemsByType<ItemPrimedExplosive>()?.ForEach(expire_exp);
        }
        foreach (Actor actor in map.Actors) {
          actor.Inventory?.GetItemsByType<ItemPrimedExplosive>()?.ForEach(expire_exp);
        }
#endregion
        if (hasExplosivesToExplode) {
#region 6.2 Explode.
          bool hasExplodedSomething;
          do {
            hasExplodedSomething = false;
            if (!hasExplodedSomething) {
              foreach (Inventory groundInventory in map.GroundInventories) {
                List<ItemPrimedExplosive> tmp = groundInventory.GetItemsByType<ItemPrimedExplosive>();
                if (null == tmp) continue;

                // leave in for formal correctness.
                Point? inventoryPosition = map.GetGroundInventoryPosition(groundInventory);
                if (!inventoryPosition.HasValue)
                  throw new InvalidOperationException("explosives : GetGroundInventoryPosition returned null point");

                foreach (ItemPrimedExplosive exp in tmp) {
                  if (0 >= exp.FuseTimeLeft) {
                    map.RemoveItemAt(exp, inventoryPosition.Value);
                    DoBlast(new Location(map, inventoryPosition.Value), exp.Model.BlastAttack);
                    hasExplodedSomething = true;
                    break;
                  }
                }
                if (hasExplodedSomething) break;
              }
            }
            if (!hasExplodedSomething) {
              foreach (Actor actor in map.Actors) {
                List<ItemPrimedExplosive> tmp = actor.Inventory?.GetItemsByType<ItemPrimedExplosive>();
                if (null == tmp) continue;
                foreach (ItemPrimedExplosive exp in tmp) {
                  if (0 >= exp.FuseTimeLeft) {
                    actor.Inventory.RemoveAllQuantity(exp);
                    DoBlast(new Location(map, actor.Location.Position), exp.Model.BlastAttack);
                    hasExplodedSomething = true;
                    break;
                  }
                }
              }
            }
          }
          while (hasExplodedSomething);
#endregion
        }
#endregion
#region 7. Check fires.
        // \todo implement per-map weather, then use it here
        if (Session.Get.World.Weather.IsRain() && m_Rules.RollChance(Rules.FIRE_RAIN_TEST_CHANCE)) {
          foreach (MapObject mapObject in map.MapObjects) {
            if (mapObject.IsOnFire && m_Rules.RollChance(Rules.FIRE_RAIN_PUT_OUT_CHANCE)) {
              mapObject.Extinguish();;
              if (ForceVisibleToPlayer(mapObject))
                AddMessage(new Data.Message("The rain has put out a fire.", map.LocalTime.TurnCounter));
            }
          }
        }
#endregion
      } // skipped in lodetail turns.

#region Check timers
      if (map.CountTimers > 0) {
        foreach (TimedTask timer in new List<TimedTask>(map.Timers)) {
          timer.Tick(map);
          if (timer.IsCompleted) map.RemoveTimer(timer);
        }
      }
#endregion
#if DATAFLOW_TRACE
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "considering NPC upgrade, Map: "+map.Name);
#endif
#region Advance local time
      bool wasNight = map.LocalTime.IsNight;
      ++map.LocalTime.TurnCounter;
      bool isDay = !map.LocalTime.IsNight;

      if (0 < map.CountActors) LOS.Now(map);
      LOS.Expire(map);
#endregion
#region Check for NPC upgrade
	  if (wasNight != isDay) return;	// night/day did not end, do not upgrade skills
      if (isDay) {
        HandleLivingNPCsUpgrade(map);
      } else {
        if (s_Options.ZombifiedsUpgradeDays == GameOptions.ZupDays.OFF || !GameOptions.IsZupDay(s_Options.ZombifiedsUpgradeDays, map.LocalTime.Day))
          return;
        HandleUndeadNPCsUpgrade(map);
      }
#endregion
    }

    private void ModifyActorTrustInLeader(Actor a, int mod, bool addMessage)
    {
      a.TrustInLeader += mod;
      if (a.TrustInLeader > Rules.TRUST_MAX) a.TrustInLeader = Rules.TRUST_MAX;
      else if (a.TrustInLeader < Rules.TRUST_MIN) a.TrustInLeader = Rules.TRUST_MIN;
      if (!addMessage || !a.Leader.IsPlayer) return;
      AddMessage(new Data.Message(string.Format("({0} trust with {1})", mod, a.TheName), Session.Get.WorldTime.TurnCounter, Color.White));
    }

    static private int CountLivings(Map map)
    {
      Contract.Requires(null != map);
      return map.Actors.Count(a => !a.Model.Abilities.IsUndead);
    }

    static private int CountFaction(Map map, Faction f)
    {
      Contract.Requires(null != map);
      return map.Actors.Count(a => a.Faction == f);
    }

    static private int CountUndeads(Map map)
    {
      Contract.Requires(null != map);
      return map.Actors.Count(a => a.Model.Abilities.IsUndead);
    }

    static private int CountFoodItemsNutrition(Map map)
    {
      Contract.Requires(null != map);
      int num1 = 0;
      Func<ItemFood,int> nutrition = (food => food.NutritionAt(map.LocalTime.TurnCounter));
      foreach (Inventory groundInventory in map.GroundInventories) {
        List<ItemFood> tmp = groundInventory.GetItemsByType<ItemFood>();
        if (null == tmp) continue;
        num1 += tmp.Sum(nutrition);
      }
      foreach (Actor actor in map.Actors) {
        List<ItemFood> tmp = actor.Inventory?.GetItemsByType<ItemFood>();
        if (null == tmp) continue;
        num1 += tmp.Sum(nutrition);
      }
      return num1;
    }

    static private bool CheckForEvent_ZombieInvasion(Map map)
    {
      return map.LocalTime.IsStrikeOfMidnight && CountUndeads(map) < s_Options.MaxUndeads;
    }

    private void FireEvent_ZombieInvasion(Map map)
    {
      if (map == m_Player.Location.Map && !m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead) {
        AddMessage(new Data.Message("It is Midnight! Zombies are invading!", Session.Get.WorldTime.TurnCounter, Color.Crimson));
        RedrawPlayScreen();
      }
      int num1 = CountUndeads(map);
      int num2 = 1 + (int)(Math.Min(1f, (float)(map.LocalTime.Day * s_Options.ZombieInvasionDailyIncrease + s_Options.DayZeroUndeadsPercent) / 100f) * (double)s_Options.MaxUndeads) - num1;
      for (int index = 0; index < num2; ++index)
        SpawnNewUndead(map, map.LocalTime.Day);
    }

    private bool CheckForEvent_SewersInvasion(Map map)
    {
      return Session.Get.HasZombiesInSewers && m_Rules.RollChance(SEWERS_INVASION_CHANCE) && CountUndeads(map) < s_Options.MaxUndeads/2;
    }

    private void FireEvent_SewersInvasion(Map map)
    {
      int num1 = CountUndeads(map);
      int num2 = 1 + (int)(Math.Min(1f, (float)(map.LocalTime.Day * s_Options.ZombieInvasionDailyIncrease + s_Options.DayZeroUndeadsPercent) / 100f) * (double)(s_Options.MaxUndeads / 2)) - num1;
      for (int index = 0; index < num2; ++index)
        SpawnNewSewersUndead(map);
    }

    static private bool CheckForEvent_RefugeesWave(Map map)
    {
      return map.LocalTime.IsStrikeOfMidday;
    }

    static private float RefugeesEventDistrictFactor(District d)
    {
      int x = d.WorldPosition.X;
      int y = d.WorldPosition.Y;
      int num1 = Session.Get.World.Size - 1;
      int num2 = num1 / 2;
      if (x == 0 || y == 0 || (x == num1 || y == num1)) return 2f;
      return x != num2 || y != num2 ? 1f : 0.5f;
    }

    private void FireEvent_RefugeesWave(District district)
    {
      if (district == m_Player.Location.Map.District && !m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead) {
        AddMessage(new Data.Message("A new wave of refugees has arrived!", Session.Get.WorldTime.TurnCounter, Color.Pink));
        RedrawPlayScreen();
      }
      int num1 = district.EntryMap.Actors.Count(a => a.Faction == GameFactions.TheCivilians || a.Faction == GameFactions.ThePolice);
      int num2 = Math.Min(1 + (int)( (RefugeesEventDistrictFactor(district) * s_Options.MaxCivilians) * REFUGEES_WAVE_SIZE), s_Options.MaxCivilians - num1);
      for (int index = 0; index < num2; ++index)
        SpawnNewRefugee(!m_Rules.RollChance(REFUGEE_SURFACE_SPAWN_CHANCE) ? (!district.HasSubway ? district.SewersMap : (m_Rules.RollChance(50) ? district.SubwayMap : district.SewersMap)) : district.EntryMap);
      if (!m_Rules.RollChance(UNIQUE_REFUGEE_CHECK_CHANCE)) return;
      lock (Session.Get) {
        UniqueActor[] local_6 = Array.FindAll<UniqueActor>(Session.Get.UniqueActors.ToArray(), a => {
          if (a.IsWithRefugees && !a.IsSpawned) return !a.TheActor.IsDead;
          return false;
        });
        if (local_6 == null || local_6.Length <= 0) return;
        int local_7 = m_Rules.Roll(0, local_6.Length);
        FireEvent_UniqueActorArrive(district.EntryMap, local_6[local_7]);
      }
    }

    private void FireEvent_UniqueActorArrive(Map map, UniqueActor unique)
    {
      if (!SpawnActorOnMapBorder(map, unique.TheActor, SPAWN_DISTANCE_TO_PLAYER)) return;
      unique.IsSpawned = true;
      if (map != m_Player.Location.Map || m_Player.IsSleeping || m_Player.Model.Abilities.IsUndead) return;
      if (unique.EventMessage != null) {
        if (unique.EventThemeMusic != null) {
          m_MusicManager.StopAll();
          m_MusicManager.Play(unique.EventThemeMusic);
        }
        ClearMessages();
        AddMessage(new Data.Message(unique.EventMessage, Session.Get.WorldTime.TurnCounter, Color.Pink));
        AddMessage(MakePlayerCentricMessage("Seems to come from", unique.TheActor.Location.Position));
        AddMessagePressEnter();
        ClearMessages();
      }
      Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, unique.TheActor.Name + " arrived.");
    }

    private bool CheckForEvent_NationalGuard(Map map)
    {
      if (s_Options.NatGuardFactor == 0 || map.LocalTime.IsNight || (map.LocalTime.Day < NATGUARD_DAY || map.LocalTime.Day >= NATGUARD_END_DAY) || !m_Rules.RollChance(NATGUARD_INTERVENTION_CHANCE))
        return false;
      int num = CountLivings(map) + CountFaction(map, GameFactions.TheArmy);
      return (float)CountUndeads(map) / (float)num * (s_Options.NatGuardFactor / 100.0) >= NATGUARD_INTERVENTION_FACTOR;
    }

    private void FireEvent_NationalGuard(Map map)
    {
      Actor actor = SpawnNewNatGuardLeader(map);
      if (actor == null) return;

      for (int index = 0; index < NATGUARD_SQUAD_SIZE-1; ++index) {
        Actor other = SpawnNewNatGuardTrooper(map, actor.Location.Position);
        if (other != null) actor.AddFollower(other);
      }

      NotifyOrderablesAI(map, RaidType.NATGUARD, actor.Location.Position);
      if (map != m_Player.Location.Map) return;
      if (!m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead) {
        m_MusicManager.StopAll();
        m_MusicManager.Play(GameMusics.ARMY);
        ClearMessages();
        AddMessage(new Data.Message("A National Guard squad has arrived!", Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        AddMessage(MakePlayerCentricMessage("Soldiers seem to come from", actor.Location.Position));
        AddMessagePressEnter();
        ClearMessages();
      }
      Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, "A National Guard squad arrived.");
    }

    private bool CheckForEvent_ArmySupplies(Map map)
    {
      if (s_Options.SuppliesDropFactor == 0 || map.LocalTime.IsNight || (map.LocalTime.Day < ARMY_SUPPLIES_DAY || !m_Rules.RollChance(ARMY_SUPPLIES_CHANCE)))
        return false;
      int num = 1 + map.Actors.Count(a =>
      {
        if (!a.Model.Abilities.IsUndead && a.Model.Abilities.HasToEat)
          return a.Faction == GameFactions.TheCivilians;
        return false;
      });
      return (float)(1 + CountFoodItemsNutrition(map)) / (float)num < s_Options.SuppliesDropFactor / 100.0 * ARMY_SUPPLIES_FACTOR;
    }

    private void FireEvent_ArmySupplies(Map map)
    {
      if (!FindDropSuppliesPoint(map, out Point dropPoint)) return;
      Rectangle survey = new Rectangle(dropPoint.X-ARMY_SUPPLIES_SCATTER, dropPoint.Y-ARMY_SUPPLIES_SCATTER, 2*ARMY_SUPPLIES_SCATTER+1, 2*ARMY_SUPPLIES_SCATTER+1);
      map.TrimToBounds(ref survey);
      survey.DoForEach(pt => map.DropItemAt((m_Rules.RollChance(80) ? BaseMapGenerator.MakeItemArmyRation() : (Item)BaseMapGenerator.MakeItemMedikit()), pt),
        pt => IsSuitableDropSuppliesPoint(map, pt));

      NotifyOrderablesAI(map, RaidType.ARMY_SUPLLIES, dropPoint);
      if (map != m_Player.Location.Map) return;
      if (!m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead) {
        m_MusicManager.StopAll();
        m_MusicManager.Play(GameMusics.ARMY);
        ClearMessages();
        AddMessage(new Data.Message("An Army chopper has dropped supplies!", Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        AddMessage(MakePlayerCentricMessage("The drop point seems to be", dropPoint));
        AddMessagePressEnter();
        ClearMessages();
      }
      Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, "An army chopper dropped supplies.");
    }

    static private bool IsSuitableDropSuppliesPoint(Map map, Point pt)
    {
      if (!map.IsValid(pt)) return false;
      Tile tileAt = map.GetTileAtExt(pt);
      return !tileAt.IsInside && tileAt.Model.IsWalkable && !map.HasActorAt(pt) && !map.HasMapObjectAt(pt) && DistanceToPlayer(map, pt) >= SPAWN_DISTANCE_TO_PLAYER;
    }

    private bool FindDropSuppliesPoint(Map map, out Point dropPoint)
    {
      dropPoint = new Point();
      int num = 4 * map.Width;
      for (int index = 0; index < num; ++index) {
        dropPoint.X = m_Rules.RollX(map);
        dropPoint.Y = m_Rules.RollY(map);
        if (IsSuitableDropSuppliesPoint(map, dropPoint)) return true;
      }
      return false;
    }

    static private bool HasRaidHappenedSince(RaidType raid, District district, WorldTime mapTime, int sinceNTurns)
    {
      if (Session.Get.HasRaidHappened(raid, district))
        return mapTime.TurnCounter - Session.Get.LastRaidTime(raid, district) < sinceNTurns;
      return false;
    }

    private bool CheckForEvent_BikersRaid(Map map)
    {
      return map.LocalTime.Day >= BIKERS_RAID_DAY && map.LocalTime.Day < BIKERS_END_DAY && (!HasRaidHappenedSince(RaidType.BIKERS, map.District, map.LocalTime, BIKERS_RAID_DAYS_GAP * WorldTime.TURNS_PER_DAY) && m_Rules.RollChance(BIKERS_RAID_CHANCE_PER_TURN));
    }

    private void FireEvent_BikersRaid(Map map)
    {
      Session.Get.SetLastRaidTime(RaidType.BIKERS, map.District, map.LocalTime.TurnCounter);
      GameGangs.IDs gangId = GameGangs.BIKERS[m_Rules.Roll(0, GameGangs.BIKERS.Length)];
      Actor actor = SpawnNewBikerLeader(map, gangId);
      if (actor == null) return;
      for (int index = 0; index < BIKERS_RAID_SIZE-1; ++index) {
        Actor other = SpawnNewBiker(map, gangId, actor.Location.Position);
        if (other != null) actor.AddFollower(other);
      }
      NotifyOrderablesAI(map, RaidType.BIKERS, actor.Location.Position);
      if (map != m_Player.Location.Map) return;
      if (!m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead) {
        m_MusicManager.StopAll();
        m_MusicManager.Play(GameMusics.BIKER);
        ClearMessages();
        AddMessage(new Data.Message("You hear the sound of roaring engines!", Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        AddMessage(MakePlayerCentricMessage("Motorbikes seem to come from", actor.Location.Position));
        AddMessagePressEnter();
        ClearMessages();
      }
      Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, "Bikers raided the district.");
    }

    private bool CheckForEvent_GangstasRaid(Map map)
    {
      return map.LocalTime.Day >= GANGSTAS_RAID_DAY && map.LocalTime.Day < GANGSTAS_END_DAY && (!HasRaidHappenedSince(RaidType.GANGSTA, map.District, map.LocalTime, GANGSTAS_RAID_DAYS_GAP*WorldTime.TURNS_PER_DAY) && m_Rules.RollChance(GANGSTAS_RAID_CHANCE_PER_TURN));
    }

    private void FireEvent_GangstasRaid(Map map)
    {
      Session.Get.SetLastRaidTime(RaidType.GANGSTA, map.District, map.LocalTime.TurnCounter);
      GameGangs.IDs gangId = GameGangs.GANGSTAS[m_Rules.Roll(0, GameGangs.GANGSTAS.Length)];
      Actor actor = SpawnNewGangstaLeader(map, gangId);
      if (actor == null) return;
      for (int index = 0; index < GANGSTAS_RAID_SIZE-1; ++index) {
        Actor other = SpawnNewGangsta(map, gangId, actor.Location.Position);
        if (other != null) actor.AddFollower(other);
      }
      NotifyOrderablesAI(map, RaidType.GANGSTA, actor.Location.Position);
      if (map != m_Player.Location.Map) return;
      if (!m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead) {
        m_MusicManager.StopAll();
        m_MusicManager.Play(GameMusics.GANGSTA);
        ClearMessages();
        AddMessage(new Data.Message("You hear obnoxious loud music!", Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        AddMessage(MakePlayerCentricMessage("Cars seem to come from", actor.Location.Position));
        AddMessagePressEnter();
        ClearMessages();
      }
      Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, "Gangstas raided the district.");
    }

    private bool CheckForEvent_BlackOpsRaid(Map map)
    {
      return map.LocalTime.Day >= BLACKOPS_RAID_DAY && !HasRaidHappenedSince(RaidType.BLACKOPS, map.District, map.LocalTime, BLACKOPS_RAID_DAY_GAP*WorldTime.TURNS_PER_DAY) && m_Rules.RollChance(BLACKOPS_RAID_CHANCE_PER_TURN);
    }

    private void FireEvent_BlackOpsRaid(Map map)
    {
      Session.Get.SetLastRaidTime(RaidType.BLACKOPS, map.District, map.LocalTime.TurnCounter);
      Actor actor = SpawnNewBlackOpsLeader(map);
      if (actor == null) return;
      for (int index = 0; index < BLACKOPS_RAID_SIZE-1; ++index) {
        Actor other = SpawnNewBlackOpsTrooper(map, actor.Location.Position);
        if (other != null) actor.AddFollower(other);
      }
      NotifyOrderablesAI(map, RaidType.BLACKOPS, actor.Location.Position);
      if (map != m_Player.Location.Map) return;
      if (!m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead) {
        m_MusicManager.StopAll();
        m_MusicManager.Play(GameMusics.ARMY);
        ClearMessages();
        AddMessage(new Data.Message("You hear a chopper flying over the city!", Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        AddMessage(MakePlayerCentricMessage("The chopper has dropped something", actor.Location.Position));
        AddMessagePressEnter();
        ClearMessages();
      }
      Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, "BlackOps raided the district.");
    }

    private bool CheckForEvent_BandOfSurvivors(Map map)
    {
      return map.LocalTime.Day >= SURVIVORS_BAND_DAY && !HasRaidHappenedSince(RaidType.SURVIVORS, map.District, map.LocalTime, SURVIVORS_BAND_DAY_GAP*WorldTime.TURNS_PER_DAY) && m_Rules.RollChance(SURVIVORS_BAND_CHANCE_PER_TURN);
    }

    private void FireEvent_BandOfSurvivors(Map map)
    {
      Session.Get.SetLastRaidTime(RaidType.SURVIVORS, map.District, map.LocalTime.TurnCounter);
      Actor actor = SpawnNewSurvivor(map);
      if (actor == null) return;
      for (int index = 0; index < SURVIVORS_BAND_SIZE-1; ++index)
        SpawnNewSurvivor(map, actor.Location.Position);
      NotifyOrderablesAI(map, RaidType.SURVIVORS, actor.Location.Position);
      if (map != m_Player.Location.Map) return;
      if (!m_Player.IsSleeping && !m_Player.Model.Abilities.IsUndead) {
        m_MusicManager.StopAll();
        m_MusicManager.Play(GameMusics.SURVIVORS);
        ClearMessages();
        AddMessage(new Data.Message("You hear shooting and honking in the distance.", Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        AddMessage(MakePlayerCentricMessage("A van has stopped", actor.Location.Position));
        AddMessagePressEnter();
        ClearMessages();
      }
      Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, "A Band of Survivors entered the district.");
    }

    static private int DistanceToPlayer(Map map, int x, int y)
    {
	  List<Actor> players = map.Players;
	  if (0 >= players.Count) return int.MaxValue;
	  return players.Select(p=> Rules.GridDistance(p.Location.Position, x, y)).Min();
    }

    static private int DistanceToPlayer(Map map, Point pos)
    {
      return DistanceToPlayer(map, pos.X, pos.Y);
    }

    private bool SpawnActorOnMapBorder(Map map, Actor actorToSpawn, int minDistToPlayer)
    {
      int num1 = 4 * (map.Width + map.Height);
      int num2 = 0;
      Point point = new Point();
      do {
        if (m_Rules.RollChance(50)) {
          point.X = m_Rules.RollX(map);
          point.Y = m_Rules.RollChance(50) ? 0 : map.Height - 1;
        } else{
          point.X = m_Rules.RollChance(50) ? 0 : map.Width - 1;
          point.Y = m_Rules.RollY(map);
        }
        if (!map.IsWalkableFor(point, actorToSpawn)) continue;
        if (DistanceToPlayer(map, point) < minDistToPlayer) continue;
        if (actorToSpawn.WouldBeAdjacentToEnemy(map, point)) continue;
        map.PlaceAt(actorToSpawn, point);
        OnActorEnterTile(actorToSpawn);
        return true;
      }
      while (++num2 <= num1);
      return false;
    }

    private bool SpawnActorNear(Map map, Actor actorToSpawn, int minDistToPlayer, Point nearPoint, int maxDistToPoint)
    {
      int num1 = 4 * (map.Width + map.Height);
      int num2 = 0;
      Point p = new Point();
      do {
        ++num2;
        int num3 = nearPoint.X + m_Rules.Roll(1, maxDistToPoint + 1) - m_Rules.Roll(1, maxDistToPoint + 1);
        int num4 = nearPoint.Y + m_Rules.Roll(1, maxDistToPoint + 1) - m_Rules.Roll(1, maxDistToPoint + 1);
        p.X = num3;
        p.Y = num4;
        map.TrimToBounds(ref p);
        if (!map.IsInsideAt(p) && map.IsWalkableFor(p, actorToSpawn) && (DistanceToPlayer(map, p) >= minDistToPlayer && !actorToSpawn.WouldBeAdjacentToEnemy(map, p))) {
          map.PlaceAt(actorToSpawn, p);
          return true;
        }
      }
      while (num2 <= num1);
      return false;
    }

    private void SpawnNewUndead(Map map, int day)
    {
      Actor newUndead = m_TownGenerator.CreateNewUndead(map.LocalTime.TurnCounter);
      if (s_Options.AllowUndeadsEvolution && Session.Get.HasEvolution) {
        GameActors.IDs fromModelID = newUndead.Model.ID;
        if (fromModelID != GameActors.IDs.UNDEAD_ZOMBIE_LORD || ZOMBIE_LORD_EVOLUTION_MIN_DAY <= day) {
          int chance = Math.Min(75, day * 2);
          if (m_Rules.RollChance(chance)) {
            fromModelID = fromModelID.NextUndeadEvolution();
            if (m_Rules.RollChance(chance))
              fromModelID = fromModelID.NextUndeadEvolution();
            newUndead.Model = GameActors[fromModelID];
          }
        }
      }
//    SpawnActorOnMapBorder(map, newUndead, SPAWN_DISTANCE_TO_PLAYER, true);    // allows cheesy metagaming
      SpawnActorOnMapBorder(map, newUndead, 1);
    }

    private void SpawnNewSewersUndead(Map map)
    {
      Actor newSewersUndead = m_TownGenerator.CreateNewSewersUndead(map.LocalTime.TurnCounter);
//    SpawnActorOnMapBorder(map, newSewersUndead, SPAWN_DISTANCE_TO_PLAYER, false); // allows cheesy metagaming
      SpawnActorOnMapBorder(map, newSewersUndead, 1);
    }

    // Balance issue is problematic here (cop spawning at distance 2 can severely damage most undead PCs)
    // eliminating PC spawn radius shielding to match undead

    // The ley line backstory doesn't work for livings.
    private void SpawnNewRefugee(Map map)
    {
      Actor newRefugee = m_TownGenerator.CreateNewRefugee(map.LocalTime.TurnCounter, REFUGEES_WAVE_ITEMS);
//    SpawnActorOnMapBorder(map, newRefugee, SPAWN_DISTANCE_TO_PLAYER, true); // allows cheesy metagaming
      SpawnActorOnMapBorder(map, newRefugee, 1);
    }

    // The bands remain PC spawn radius shielded, for now.
    private Actor SpawnNewSurvivor(Map map)
    {
      Actor newSurvivor = m_TownGenerator.CreateNewSurvivor(map.LocalTime.TurnCounter);
      return (SpawnActorOnMapBorder(map, newSurvivor, SPAWN_DISTANCE_TO_PLAYER) ? newSurvivor : null);
    }

    private Actor SpawnNewSurvivor(Map map, Point bandPos)
    {
      Actor newSurvivor = m_TownGenerator.CreateNewSurvivor(map.LocalTime.TurnCounter);
      return (SpawnActorNear(map, newSurvivor, SPAWN_DISTANCE_TO_PLAYER, bandPos, 3) ? newSurvivor : null);
    }

    private Actor SpawnNewNatGuardLeader(Map map)
    {
      Actor armyNationalGuard = m_TownGenerator.CreateNewArmyNationalGuard(map.LocalTime.TurnCounter, "Sgt");
      armyNationalGuard.StartingSkill(Skills.IDs.LEADERSHIP);
      if (map.LocalTime.Day > NATGUARD_ZTRACKER_DAY)
        armyNationalGuard.Inventory.AddAll(BaseMapGenerator.MakeItemZTracker());
      return (SpawnActorOnMapBorder(map, armyNationalGuard, SPAWN_DISTANCE_TO_PLAYER) ? armyNationalGuard : null);
    }

    private Actor SpawnNewNatGuardTrooper(Map map, Point leaderPos)
    {
      Actor armyNationalGuard = m_TownGenerator.CreateNewArmyNationalGuard(map.LocalTime.TurnCounter, "Pvt");
      if (m_Rules.RollChance(50))
        armyNationalGuard.Inventory.AddAll(BaseMapGenerator.MakeItemCombatKnife());
      else
        armyNationalGuard.Inventory.AddAll(m_TownGenerator.MakeItemGrenade());
      return (SpawnActorNear(map, armyNationalGuard, SPAWN_DISTANCE_TO_PLAYER, leaderPos, 3) ? armyNationalGuard : null);
    }

    private Actor SpawnNewBikerLeader(Map map, GameGangs.IDs gangId)
    {
      Actor newBikerMan = m_TownGenerator.CreateNewBikerMan(map.LocalTime.TurnCounter, gangId);
      newBikerMan.StartingSkill(Skills.IDs.LEADERSHIP);
      newBikerMan.StartingSkill(Skills.IDs.TOUGH,3);
      newBikerMan.StartingSkill(Skills.IDs.STRONG,3);
      return (SpawnActorOnMapBorder(map, newBikerMan, SPAWN_DISTANCE_TO_PLAYER) ? newBikerMan : null);
    }

    private Actor SpawnNewBiker(Map map, GameGangs.IDs gangId, Point leaderPos)
    {
      Actor newBikerMan = m_TownGenerator.CreateNewBikerMan(map.LocalTime.TurnCounter, gangId);
      newBikerMan.StartingSkill(Skills.IDs.TOUGH);
      newBikerMan.StartingSkill(Skills.IDs.STRONG);
      return (SpawnActorNear(map, newBikerMan, SPAWN_DISTANCE_TO_PLAYER, leaderPos, 3) ? newBikerMan : null);
    }

    private Actor SpawnNewGangstaLeader(Map map, GameGangs.IDs gangId)
    {
      Actor newGangstaMan = m_TownGenerator.CreateNewGangstaMan(map.LocalTime.TurnCounter, gangId);
      newGangstaMan.StartingSkill(Skills.IDs.LEADERSHIP);
      newGangstaMan.StartingSkill(Skills.IDs._FIRST,3);
      newGangstaMan.StartingSkill(Skills.IDs.FIREARMS);
      return (SpawnActorOnMapBorder(map, newGangstaMan, SPAWN_DISTANCE_TO_PLAYER) ? newGangstaMan : null);
    }

    private Actor SpawnNewGangsta(Map map, GameGangs.IDs gangId, Point leaderPos)
    {
      Actor newGangstaMan = m_TownGenerator.CreateNewGangstaMan(map.LocalTime.TurnCounter, gangId);
      newGangstaMan.StartingSkill(Skills.IDs._FIRST);
      return (SpawnActorNear(map, newGangstaMan, SPAWN_DISTANCE_TO_PLAYER, leaderPos, 3) ? newGangstaMan : null);
    }

    private Actor SpawnNewBlackOpsLeader(Map map)
    {
      Actor newBlackOps = m_TownGenerator.CreateNewBlackOps(map.LocalTime.TurnCounter, "Officer");
      newBlackOps.StartingSkill(Skills.IDs.LEADERSHIP);
      newBlackOps.StartingSkill(Skills.IDs._FIRST,3);
      newBlackOps.StartingSkill(Skills.IDs.FIREARMS,3);
      newBlackOps.StartingSkill(Skills.IDs.TOUGH);
      return (SpawnActorOnMapBorder(map, newBlackOps, SPAWN_DISTANCE_TO_PLAYER) ? newBlackOps : null);
    }

    private Actor SpawnNewBlackOpsTrooper(Map map, Point leaderPos)
    {
      Actor newBlackOps = m_TownGenerator.CreateNewBlackOps(map.LocalTime.TurnCounter, "Agent");
      newBlackOps.StartingSkill(Skills.IDs._FIRST);
      newBlackOps.StartingSkill(Skills.IDs.FIREARMS);
      newBlackOps.StartingSkill(Skills.IDs.TOUGH);
      return (SpawnActorNear(map, newBlackOps, SPAWN_DISTANCE_TO_PLAYER, leaderPos, 3) ? newBlackOps : null);
    }

    public void StopTheWorld()
    {
      StopSimThread();
      m_IsGameRunning = false;
      m_MusicManager.StopAll();
    }

    private void HandlePlayerActor(Actor player)
    {
      Contract.Requires(null != player);
      Contract.Requires(!player.IsSleeping);
      player.Controller.UpdateSensors();
      m_Player = player;
      Session.Get.CurrentMap = player.Location.Map;  // multi-PC support
      ComputeViewRect(player.Location.Position);
      Session.Get.Scoring.TurnsSurvived = Session.Get.WorldTime.TurnCounter;
#if SUICIDE_BY_LONG_WAIT
      if (m_IsPlayerLongWait) {
        if (CheckPlayerWaitLong(player)) {
          DoWait(player);
          return;
        }
        m_IsPlayerLongWait = false;
        m_IsPlayerLongWaitForcedStop = false;
        if (Session.Get.WorldTime.TurnCounter >= m_PlayerLongWaitEnd.TurnCounter)
          AddMessage(new Data.Message("Wait ended.", Session.Get.WorldTime.TurnCounter, Color.Yellow));
        else
          AddMessage(new Data.Message("Wait interrupted!", Session.Get.WorldTime.TurnCounter, Color.Red));
      }
#endif

      GC.Collect(); // force garbage collection when things should be slow anyway

      bool flag1 = true;
      do {
        m_UI.UI_SetCursor(null);
        if (s_Options.IsAdvisorEnabled && HasAdvisorAnyHintToGive())
          AddOverlay(new OverlayPopup(new string[1] { string.Format("HINT AVAILABLE PRESS <{0}>", s_KeyBindings.Get(PlayerCommand.ADVISOR).ToString()) }, Color.White, Color.White, Color.Black, MapToScreen(m_Player.Location.Position.X - 3, m_Player.Location.Position.Y - 1)));
        RedrawPlayScreen();
        m_UI.UI_PeekKey();
        bool flag2 = true;
        bool flag3 = false;
        Point mousePosition = m_UI.UI_GetMousePosition();
        Point point = new Point(-1, -1);
        MouseButtons? mouseButtons = new MouseButtons?();
        KeyEventArgs key;
        do {
          key = m_UI.UI_PeekKey();
          if (key != null) {
            flag3 = true;
            flag2 = false;
          } else {
            point = m_UI.UI_GetMousePosition();
            mouseButtons = m_UI.UI_PeekMouseButtons();
            if (point != mousePosition || mouseButtons.HasValue)
              flag2 = false;
          }
        }
        while (flag2);
        if (flag3)
        {
          PlayerCommand command = InputTranslator.KeyToCommand(key);
          if (command == PlayerCommand.QUIT_GAME) {
            if (HandleQuitGame()) {
              StopTheWorld();
              RedrawPlayScreen();
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
                HandleHelpMode();
                break;
              case PlayerCommand.ADVISOR:
                HandleAdvisor(player);
                break;
              case PlayerCommand.OPTIONS_MODE:
                HandleOptions(true);
                ApplyOptions(true);
                break;
              case PlayerCommand.KEYBINDING_MODE:
                                HandleRedefineKeys();
                break;
              case PlayerCommand.HINTS_SCREEN_MODE:
                                HandleHintsScreen();
                break;
              case PlayerCommand.SCREENSHOT:
                                HandleScreenshot();
                break;
              case PlayerCommand.SAVE_GAME:
                StopSimThread();
                HandleSaveGame();
                RestartSimThread();
                break;
              case PlayerCommand.LOAD_GAME:
                StopSimThread();
                HandleLoadGame();
                RestartSimThread();
                player = m_Player;
                flag1 = false;
                m_HasLoadedGame = true;
                break;
              case PlayerCommand.ABANDON_GAME:
                if (HandleAbandonGame()) {
                  StopSimThread();
                  flag1 = false;
                  KillActor(null, m_Player, "suicide");
                  break;
                }
                break;
              case PlayerCommand.ABANDON_PC:
                if (HandleAbandonPC(m_Player)) {
                  StopSimThread();
                  flag1 = false;
                  break;
                }
                flag1 = false;
                break;
              case PlayerCommand.MOVE_N:
                flag1 = !TryPlayerInsanity() && !DoPlayerBump(player, Direction.N);
                break;
              case PlayerCommand.MOVE_NE:
                flag1 = !TryPlayerInsanity() && !DoPlayerBump(player, Direction.NE);
                break;
              case PlayerCommand.MOVE_E:
                flag1 = !TryPlayerInsanity() && !DoPlayerBump(player, Direction.E);
                break;
              case PlayerCommand.MOVE_SE:
                flag1 = !TryPlayerInsanity() && !DoPlayerBump(player, Direction.SE);
                break;
              case PlayerCommand.MOVE_S:
                flag1 = !TryPlayerInsanity() && !DoPlayerBump(player, Direction.S);
                break;
              case PlayerCommand.MOVE_SW:
                flag1 = !TryPlayerInsanity() && !DoPlayerBump(player, Direction.SW);
                break;
              case PlayerCommand.MOVE_W:
                flag1 = !TryPlayerInsanity() && !DoPlayerBump(player, Direction.W);
                break;
              case PlayerCommand.MOVE_NW:
                flag1 = !TryPlayerInsanity() && !DoPlayerBump(player, Direction.NW);
                break;
              case PlayerCommand.RUN_TOGGLE:
                if (TryPlayerInsanity()) {
                  flag1 = false;
                  break;
                }
                HandlePlayerRunToggle(player);
                break;
              case PlayerCommand.WAIT_OR_SELF:
                if (TryPlayerInsanity()) {
                  flag1 = false;
                  break;
                }
                flag1 = false;
                DoWait(player);
                break;
#if SUICIDE_BY_LONG_WAIT
              case PlayerCommand.WAIT_LONG:
                if (TryPlayerInsanity()) {
                  flag1 = false;
                  break;
                }
                flag1 = false;
                StartPlayerWaitLong(player);
                break;
#endif
              case PlayerCommand.BARRICADE_MODE:
                flag1 = !TryPlayerInsanity() && !HandlePlayerBarricade(player);
                break;
              case PlayerCommand.BREAK_MODE:
                flag1 = !TryPlayerInsanity() && !HandlePlayerBreak(player);
                break;
              case PlayerCommand.BUILD_LARGE_FORTIFICATION:
                flag1 = !TryPlayerInsanity() && !HandlePlayerBuildFortification(player, true);
                break;
              case PlayerCommand.BUILD_SMALL_FORTIFICATION:
                flag1 = !TryPlayerInsanity() && !HandlePlayerBuildFortification(player, false);
                break;
              case PlayerCommand.CLOSE_DOOR:
                flag1 = !TryPlayerInsanity() && !HandlePlayerCloseDoor(player);
                break;
              case PlayerCommand.EAT_CORPSE:
                flag1 = !TryPlayerInsanity() && !HandlePlayerEatCorpse(player, point);
                break;
              case PlayerCommand.FIRE_MODE:
                flag1 = !TryPlayerInsanity() && !HandlePlayerFireMode(player);
                break;
              case PlayerCommand.GIVE_ITEM:
                flag1 = !TryPlayerInsanity() && !HandlePlayerGiveItem(player, point);
                break;
              case PlayerCommand.INITIATE_TRADE:
                flag1 = !TryPlayerInsanity() && !HandlePlayerInitiateTrade(player, point);
                break;
              case PlayerCommand.LEAD_MODE:
                flag1 = !TryPlayerInsanity() && !HandlePlayerTakeLead(player);
                break;
              case PlayerCommand.MARK_ENEMIES_MODE:
                if (TryPlayerInsanity())
                {
                  flag1 = false;
                  break;
                }
                HandlePlayerMarkEnemies(player);
                break;
              case PlayerCommand.ORDER_MODE:
                flag1 = !TryPlayerInsanity() && !HandlePlayerOrderMode(player);
                break;
              case PlayerCommand.PUSH_MODE:
                flag1 = !TryPlayerInsanity() && !HandlePlayerPush(player);
                break;
              case PlayerCommand.REVIVE_CORPSE:
                flag1 = !TryPlayerInsanity() && !HandlePlayerReviveCorpse(player, point);
                break;
              case PlayerCommand.SHOUT:
                flag1 = !TryPlayerInsanity() && !HandlePlayerShout(player, null);
                break;
              case PlayerCommand.SLEEP:
                flag1 = !TryPlayerInsanity() && !HandlePlayerSleep(player);
                break;
              case PlayerCommand.SWITCH_PLACE:
                flag1 = !TryPlayerInsanity() && !HandlePlayerSwitchPlace(player);
                break;
              case PlayerCommand.USE_EXIT:
                flag1 = !TryPlayerInsanity() && !DoUseExit(player, player.Location.Position);
                break;
              case PlayerCommand.USE_SPRAY:
                flag1 = !TryPlayerInsanity() && !HandlePlayerUseSpray(player);
                break;
              case PlayerCommand.CITY_INFO:
                HandleCityInfo();
                break;
              case PlayerCommand.ITEM_INFO:
                HandleItemInfo();
                break;
              case PlayerCommand.ALLIES_INFO:
                HandleAlliesInfo();
                break;
              case PlayerCommand.DAIMON_MAP:    // cheat command
                HandleDaimonMap();
                break;
              case PlayerCommand.MESSAGE_LOG:
                HandleMessageLog();
                break;
              case PlayerCommand.ITEM_SLOT_0:
                flag1 = !TryPlayerInsanity() && !DoPlayerItemSlot(player, 0, key);
                break;
              case PlayerCommand.ITEM_SLOT_1:
                flag1 = !TryPlayerInsanity() && !DoPlayerItemSlot(player, 1, key);
                break;
              case PlayerCommand.ITEM_SLOT_2:
                flag1 = !TryPlayerInsanity() && !DoPlayerItemSlot(player, 2, key);
                break;
              case PlayerCommand.ITEM_SLOT_3:
                flag1 = !TryPlayerInsanity() && !DoPlayerItemSlot(player, 3, key);
                break;
              case PlayerCommand.ITEM_SLOT_4:
                flag1 = !TryPlayerInsanity() && !DoPlayerItemSlot(player, 4, key);
                break;
              case PlayerCommand.ITEM_SLOT_5:
                flag1 = !TryPlayerInsanity() && !DoPlayerItemSlot(player, 5, key);
                break;
              case PlayerCommand.ITEM_SLOT_6:
                flag1 = !TryPlayerInsanity() && !DoPlayerItemSlot(player, 6, key);
                break;
              case PlayerCommand.ITEM_SLOT_7:
                flag1 = !TryPlayerInsanity() && !DoPlayerItemSlot(player, 7, key);
                break;
              case PlayerCommand.ITEM_SLOT_8:
                flag1 = !TryPlayerInsanity() && !DoPlayerItemSlot(player, 8, key);
                break;
              case PlayerCommand.ITEM_SLOT_9:
                flag1 = !TryPlayerInsanity() && !DoPlayerItemSlot(player, 9, key);
                break;
              default:
                throw new ArgumentException("command unhandled");
            }
          }
        } else if (!HandleMouseLook(point)) {
          if (HandleMouseInventory(point, mouseButtons, out bool hasDoneAction)) {
            if (!hasDoneAction) continue;
            flag1 = false;
          }
          if (HandleMouseOverCorpses(point, mouseButtons, out hasDoneAction)) {
            if (!hasDoneAction) continue;
            flag1 = false;
          }
          ClearOverlays();
        }
      }
      while (flag1);
      player.Controller.UpdateSensors();
      ComputeViewRect(player.Location.Position);
      Session.Get.LastTurnPlayerActed = Session.Get.WorldTime.TurnCounter;
    }

    private bool TryPlayerInsanity()
    {
      if (!m_Player.IsInsane || !m_Rules.RollChance(Rules.SANITY_INSANE_ACTION_CHANCE)) return false;
      ActorAction insaneAction = GenerateInsaneAction(m_Player);
      if (insaneAction == null || !insaneAction.IsLegal()) return false;
      ClearMessages();
      AddMessage(new Data.Message("(your insanity takes over)", m_Player.Location.Map.LocalTime.TurnCounter, Color.Orange));
      AddMessagePressEnter();
      insaneAction.Perform();
      return true;
    }

    private bool HandleQuitGame()
    {
      AddMessage(MakeYesNoMessage("REALLY QUIT GAME"));
      RedrawPlayScreen();
      bool flag = WaitYesOrNo();
      if (!flag)
        AddMessage(new Data.Message("Good. Keep roguing!", Session.Get.WorldTime.TurnCounter, Color.Yellow));
      else
        AddMessage(new Data.Message("Bye!", Session.Get.WorldTime.TurnCounter, Color.Yellow));
      return flag;
    }

    private bool HandleAbandonGame()
    {
      AddMessage(MakeYesNoMessage("REALLY KILL YOURSELF"));
      RedrawPlayScreen();
      bool flag = WaitYesOrNo();
      if (!flag)
        AddMessage(new Data.Message("Good. No reason to make the undeads life easier by removing yours!", Session.Get.WorldTime.TurnCounter, Color.Yellow));
      else
        AddMessage(new Data.Message("You can't bear the horror anymore...", Session.Get.WorldTime.TurnCounter, Color.Yellow));
      return flag;
    }

    private bool HandleAbandonPC(Actor player)
    {
      Contract.Requires(null!=player);
      Contract.Requires(player.IsPlayer);
      AddMessage(MakeYesNoMessage("REALLY ABANDON "+player.UnmodifiedName+" TO FATE"));
      RedrawPlayScreen();
      bool flag = WaitYesOrNo();
      if (!flag) {
        AddMessage(new Data.Message("Good. No reason to make the undeads life easier by removing yours!", Session.Get.WorldTime.TurnCounter, Color.Yellow));
        return false;
      }
      player.Controller = player.Model.InstanciateController();
      AddMessage(new Data.Message("You can't bear the horror anymore...", Session.Get.WorldTime.TurnCounter, Color.Yellow));
      if (0<player.CountFollowers) {
        foreach(Actor fo in player.Followers.Where(a=>a.IsPlayer)) {
          HandleAbandonPC(fo);
          if (fo.IsPlayer) player.RemoveFollower(fo);  // NPCs cannot lead PCs; cf Actor::PrepareForPlayerControl
        }
      }
      return 0>=Session.Get.World.PlayerCount;
    }


    private void HandleScreenshot()
    {
      AddMessage(new Data.Message("Taking screenshot...", Session.Get.WorldTime.TurnCounter, Color.Yellow));
      RedrawPlayScreen();
      string screenshot = DoTakeScreenshot();
      if (screenshot == null)
        AddMessage(new Data.Message("Could not save screenshot.", Session.Get.WorldTime.TurnCounter, Color.Red));
      else
        AddMessage(new Data.Message(string.Format("screenshot {0} saved.", screenshot), Session.Get.WorldTime.TurnCounter, Color.Yellow));
      RedrawPlayScreen();
    }

    private string DoTakeScreenshot()
    {
      string newScreenshotName = GetUserNewScreenshotName();
      if (m_UI.UI_SaveScreenshot(ScreenshotFilePath(newScreenshotName)) != null)
        return newScreenshotName;
      return null;
    }

    private void HandleHelpMode()
    {
      if (m_Manual == null) {
        m_UI.UI_Clear(Color.Black);
        m_UI.UI_DrawStringBold(Color.Red, "Game manual not available ingame.", 0, 0, new Color?());
        DrawFootnote(Color.White, "press ENTER");
        m_UI.UI_Repaint();
        WaitEnter();
      } else {
        bool flag = true;
        List<string> formatedLines = m_Manual.FormatedLines;
        do {
          m_UI.UI_Clear(Color.Black);
          DrawHeader();
          int gy1 = 14;
          m_UI.UI_DrawStringBold(Color.Yellow, "Game Manual", 0, gy1, new Color?());
          gy1 += 14;
          m_UI.UI_DrawStringBold(Color.White, hr_plus, 0, gy1, new Color?());
          gy1 += 14;
          int index = m_ManualLine;
          do {
            if (!(formatedLines[index] == "<SECTION>")) {
              m_UI.UI_DrawStringBold(Color.LightGray, formatedLines[index], 0, gy1, new Color?());
              gy1 += 14;
            }
            ++index;
          }
          while (index < formatedLines.Count && gy1 < 740);
            m_UI.UI_DrawStringBold(Color.White, hr_plus, 0, gy1, new Color?());
          DrawFootnote(Color.White, "cursor and PgUp/PgDn to move, numbers to jump to section, ESC to leave");
          m_UI.UI_Repaint();
          KeyEventArgs keyEventArgs = m_UI.UI_WaitKey();
          int choiceNumber = KeyToChoiceNumber(keyEventArgs.KeyCode);
          if (choiceNumber >= 0) {
            if (choiceNumber == 0) {
              m_ManualLine = 0;
            } else {
              int num3 = m_ManualLine;
              int num4 = 0;
              for (m_ManualLine = 0; num4 < choiceNumber && m_ManualLine < formatedLines.Count; ++m_ManualLine) {
                if (formatedLines[m_ManualLine] == "<SECTION>") ++num4;
              }
              if (m_ManualLine >= formatedLines.Count) m_ManualLine = num3;
            }
          } else {
            switch (keyEventArgs.KeyCode) {
              case Keys.Escape:
                flag = false;
                break;
              case Keys.Prior:
                m_ManualLine -= 50;
                break;
              case Keys.Next:
                m_ManualLine += 50;
                break;
              case Keys.Up:
                --m_ManualLine;
                break;
              case Keys.Down:
                ++m_ManualLine;
                break;
            }
          }
          if (m_ManualLine < 0) m_ManualLine = 0;
          if (m_ManualLine + 50 >= formatedLines.Count) m_ManualLine = Math.Max(0, formatedLines.Count - 50);
        }
        while (flag);
      }
    }

    private void HandleHintsScreen()
    {
      m_UI.UI_Clear(Color.Black);
      DrawHeader();
      int gy1 = 14;
      m_UI.UI_DrawStringBold(Color.Yellow, "Advisor Hints", 0, gy1, new Color?());
      gy1 += 14;
      m_UI.UI_DrawStringBold(Color.White, "preparing...", 0, gy1, new Color?());
      m_UI.UI_Repaint();
      List<string> stringList = new List<string>();
      for (int index = 0; index < (int)AdvisorHint._COUNT; ++index) {
        GetAdvisorHintText((AdvisorHint) index, out string title, out string[] body);
        stringList.Add(string.Format("HINT {0} : {1}", index, title));
        stringList.AddRange(body);
        stringList.Add("~~~~");
        stringList.Add("");
      }
      int num3 = 0;
      bool flag = true;
      do {
        m_UI.UI_Clear(Color.Black);
        DrawHeader();
        int gy3 = 14;
        m_UI.UI_DrawStringBold(Color.Yellow, "Advisor Hints", 0, gy3, new Color?());
        gy3 += 14;
        m_UI.UI_DrawStringBold(Color.White, hr_plus, 0, gy3, new Color?());
        gy3 += 14;
        int index = num3;
        do {
          m_UI.UI_DrawStringBold(Color.LightGray, stringList[index], 0, gy3, new Color?());
          gy3 += 14;
          ++index;
        }
        while (index < stringList.Count && gy3 < 740);
        m_UI.UI_DrawStringBold(Color.White, hr_plus, 0, gy3, new Color?());
        DrawFootnote(Color.White, "cursor and PgUp/PgDn to move, R to reset hints, ESC to leave");
        m_UI.UI_Repaint();
        switch (m_UI.UI_WaitKey().KeyCode) {
          case Keys.Up:
            --num3;
            break;
          case Keys.Down:
            ++num3;
            break;
          case Keys.R:
            s_Hints.ResetAllHints();
            m_UI.UI_Clear(Color.Black);
            DrawHeader();
            int gy6 = 14;
            m_UI.UI_DrawStringBold(Color.Yellow, "Advisor Hints", 0, gy6, new Color?());
            m_UI.UI_DrawStringBold(Color.White, "Hints reset done.", 0, gy6 + 14, new Color?());
            m_UI.UI_Repaint();
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
        if (num3 < 0) num3 = 0;
        if (num3 + 50 >= stringList.Count)
          num3 = Math.Max(0, stringList.Count - 50);
      }
      while (flag);
    }

    private void HandleMessageLog()
    {
      m_UI.UI_Clear(Color.Black);
      DrawHeader();
      int gy1 = 14;
      m_UI.UI_DrawStringBold(Color.Yellow, "Message Log", 0, gy1, new Color?());
      gy1 += 14;
      m_UI.UI_DrawStringBold(Color.White, hr_plus, 0, gy1, new Color?());
      gy1 += 14;
      foreach (Data.Message message in m_MessageManager.History) {
        m_UI.UI_DrawString(message.Color, message.Text, 0, gy1, new Color?());
        gy1 += 12;
      }
      DrawFootnote(Color.White, "press ESC to leave");
      m_UI.UI_Repaint();
      WaitEscape();
    }

	static private string HandleCityInfo_DistrictToCode(DistrictKind d)
	{
       switch (d) {
         case DistrictKind.GENERAL: return "Gen";
         case DistrictKind.RESIDENTIAL: return "Res";
         case DistrictKind.SHOPPING: return "Sho";
         case DistrictKind.GREEN: return "Gre";
         case DistrictKind.BUSINESS: return "Bus";
         default:
#if DEBUG
           throw new ArgumentOutOfRangeException("unhandled district kind");
#else
		   return "BUG";
#endif
       }
	}

	static private Color HandleCityInfo_DistrictToColor(DistrictKind d)
	{
       switch (d) {
         case DistrictKind.GENERAL: return Color.Gray;
         case DistrictKind.RESIDENTIAL: return Color.Orange;
         case DistrictKind.SHOPPING: return Color.White;
         case DistrictKind.GREEN: return Color.Green;
         case DistrictKind.BUSINESS: return Color.Red;
         default:
#if DEBUG
           throw new ArgumentOutOfRangeException("unhandled district kind");
#else
		   return Color.Blue;
#endif
	  }
	}

    private void HandleCityInfo()
    {
      int gx = 0;
      int gy = 0;
      m_UI.UI_Clear(Color.Black);
      m_UI.UI_DrawStringBold(Color.White, "CITY INFORMATION -- "+Session.Get.Seed.ToString(), gx, gy, new Color?());
      gy = 2* BOLD_LINE_SPACING;
      if (m_Player.Model.Abilities.IsUndead) {
        m_UI.UI_DrawStringBold(Color.Red, "You can't remember where you are...", gx, gy, new Color?());
        gy += BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.Red, "Must be that rotting brain of yours...", gx, gy, new Color?());
        DrawFootnote(Color.White, "press ESC to leave");
        m_UI.UI_Repaint();
        WaitEscape();
        return;
      }

      m_UI.UI_DrawStringBold(Color.White, "> DISTRICTS LAYOUT", gx, gy, new Color?());
      gy += BOLD_LINE_SPACING;
      int num2 = gy + BOLD_LINE_SPACING;
      for (int index = 0; index < Session.Get.World.Size; ++index) {
        Color color = index == m_Player.Location.Map.District.WorldPosition.Y ? Color.LightGreen : Color.White;
        m_UI.UI_DrawStringBold(color, index.ToString(), 20, num2 + index * 3 * BOLD_LINE_SPACING + BOLD_LINE_SPACING, new Color?());
        m_UI.UI_DrawStringBold(color, ".", 20, num2 + index * 3 * BOLD_LINE_SPACING, new Color?());
        m_UI.UI_DrawStringBold(color, ".", 20, num2 + index * 3 * BOLD_LINE_SPACING + 2* BOLD_LINE_SPACING, new Color?());
      }
      for (int index = 0; index < Session.Get.World.Size; ++index)
        m_UI.UI_DrawStringBold(index == m_Player.Location.Map.District.WorldPosition.X ? Color.LightGreen : Color.White, string.Format("..{0}..", (char)(65 + index)), 32 + index * 48, gy, new Color?());
      int num3 = gy + BOLD_LINE_SPACING;
      const int num4 = 32;
      int num5 = num3;
      for (int index1 = 0; index1 < Session.Get.World.Size; ++index1) {
        for (int index2 = 0; index2 < Session.Get.World.Size; ++index2) {
          District district = Session.Get.World[index2, index1];
          char ch = district == Session.Get.CurrentMap.District ? '*' : (Session.Get.Scoring.HasVisited(district.EntryMap) ? '-' : '?');
          Color color = HandleCityInfo_DistrictToColor(district.Kind);
          string str = HandleCityInfo_DistrictToCode(district.Kind);
          string text = "".PadLeft(5,ch);
          m_UI.UI_DrawStringBold(color, text, num4 + index2 * 48, num5 + index1 * 3 * BOLD_LINE_SPACING, new Color?());
          m_UI.UI_DrawStringBold(color, string.Format("{0}{1}{2}", ch, str, ch), num4 + index2 * 48, num5 + (index1 * 3 + 1) * BOLD_LINE_SPACING, new Color?());
          m_UI.UI_DrawStringBold(color, text, num4 + index2 * 48, num5 + (index1 * 3 + 2) * BOLD_LINE_SPACING, new Color?());
        }

        int num6 = Session.Get.World.Size / 2;
        for (int index = 1; index < Session.Get.World.Size; ++index)
          m_UI.UI_DrawStringBold(Color.White, "=", num4 + index * 48 - 8, num5 + num6 * 3 * BOLD_LINE_SPACING + BOLD_LINE_SPACING, new Color?());
        int gy3 = num3 + (Session.Get.World.Size * 3 + 1) * BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.White, "Legend", gx, gy3, new Color?());
        int gy4 = gy3 + BOLD_LINE_SPACING;
        m_UI.UI_DrawString(Color.White, "  *   - current     ?   - unvisited", gx, gy4, new Color?());
        int gy5 = gy4 + LINE_SPACING;
        m_UI.UI_DrawString(Color.White, "  Bus - Business    Gen - General    Gre - Green", gx, gy5, new Color?());
        int gy6 = gy5 + LINE_SPACING;
        m_UI.UI_DrawString(Color.White, "  Res - Residential Sho - Shopping", gx, gy6, new Color?());
        int gy7 = gy6 + LINE_SPACING;
        m_UI.UI_DrawString(Color.White, "  =   - Subway Line", gx, gy7, new Color?());
        int gy8 = gy7 + LINE_SPACING + BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.White, "> NOTABLE LOCATIONS", gx, gy8, new Color?());
        int gy9 = gy8 + BOLD_LINE_SPACING;
        int num7 = gy9;
        for (int y = 0; y < Session.Get.World.Size; ++y) {
          for (int x = 0; x < Session.Get.World.Size; ++x) {
            Map entryMap = Session.Get.World[x, y].EntryMap;
            Zone zoneByPartialName1;
            if ((zoneByPartialName1 = entryMap.GetZoneByPartialName("Subway Station")) != null) {
              m_UI.UI_DrawStringBold(Color.Blue, string.Format("at {0} : {1}.", World.CoordToString(x, y), zoneByPartialName1.Name), gx, gy9, new Color?());
              gy9 += BOLD_LINE_SPACING;
              if (gy9 >= 740) {
                gy9 = num7;
                gx += 350;
              }
            }
            Zone zoneByPartialName2;
            if ((zoneByPartialName2 = entryMap.GetZoneByPartialName("Sewers Maintenance")) != null) {
              m_UI.UI_DrawStringBold(Color.Green, string.Format("at {0} : {1}.", World.CoordToString(x, y), zoneByPartialName2.Name), gx, gy9, new Color?());
              gy9 += BOLD_LINE_SPACING;
              if (gy9 >= 740) {
                gy9 = num7;
                gx += 350;
              }
            }
            if (entryMap == Session.Get.UniqueMaps.PoliceStation_OfficesLevel.TheMap.District.EntryMap) {
              m_UI.UI_DrawStringBold(Color.CadetBlue, string.Format("at {0} : Police Station.", World.CoordToString(x, y)), gx, gy9, new Color?());
              gy9 += BOLD_LINE_SPACING;
              if (gy9 >= 740) {
                gy9 = num7;
                gx += 350;
              }
            }
            if (entryMap == Session.Get.UniqueMaps.Hospital_Admissions.TheMap.District.EntryMap) {
              m_UI.UI_DrawStringBold(Color.White, string.Format("at {0} : Hospital.", World.CoordToString(x, y)), gx, gy9, new Color?());
              gy9 += BOLD_LINE_SPACING;
              if (gy9 >= 740) {
                gy9 = num7;
                gx += 350;
              }
            }
            if (Session.Get.PlayerKnows_CHARUndergroundFacilityLocation && entryMap == Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap.District.EntryMap) {
              m_UI.UI_DrawStringBold(Color.Red, string.Format("at {0} : {1}.", World.CoordToString(x, y), Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap.Name), gx, gy9, new Color?());
              gy9 += BOLD_LINE_SPACING;
              if (gy9 >= 740) {
                gy9 = num7;
                gx += 350;
              }
            }
            if (Session.Get.PlayerKnows_TheSewersThingLocation && (entryMap == Session.Get.UniqueActors.TheSewersThing.TheActor.Location.Map.District.EntryMap && !Session.Get.UniqueActors.TheSewersThing.TheActor.IsDead))
            {
              m_UI.UI_DrawStringBold(Color.Red, string.Format("at {0} : The Sewers Thing lives down there.", World.CoordToString(x, y)), gx, gy9, new Color?());
              gy9 += BOLD_LINE_SPACING;
              if (gy9 >= 740) {
                gy9 = num7;
                gx += 350;
              }
            }
          }
        }
      }
      DrawFootnote(Color.White, "press ESC to leave");
      m_UI.UI_Repaint();
      WaitEscape();
    }

    private void PagedMenu(string header,int strict_ub, Func<int,string> label, Predicate<int> details)    // breaks down if MAX_MESSAGES exceeds 10
    {
#if DEBUG
      if (null == label) throw new ArgumentNullException(nameof(label));
      if (null == details) throw new ArgumentNullException(nameof(details));
#endif
      bool flag1 = true;
      int num1 = 0;
      do {
        ClearOverlays();
        AddOverlay(new RogueGame.OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
        ClearMessages();
        AddMessage(new Data.Message(header, Session.Get.WorldTime.TurnCounter, Color.Yellow));
        int num2;
        for (num2 = 0; num2 < MAX_MESSAGES-2 && num1 + num2 < strict_ub; ++num2) {
          int index = num1 + num2;
          AddMessage(new Data.Message((1+num2).ToString()+" "+label(index), Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        }
        if (num2 < strict_ub)
          AddMessage(new Data.Message("9. next", Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        RedrawPlayScreen();
        KeyEventArgs keyEventArgs = m_UI.UI_WaitKey();
        int choiceNumber = KeyToChoiceNumber(keyEventArgs.KeyCode);
        if (keyEventArgs.KeyCode == Keys.Escape) flag1 = false;
        else if (choiceNumber == 9) {
          num1 += MAX_MESSAGES-2;
          if (num1 >= strict_ub) num1 = 0;
        } else if (choiceNumber >= 1 && choiceNumber <= num2) {
          int index = num1 + choiceNumber - 1;
          if (details(index)) flag1 = false;
        }
      }
      while (flag1);
      ClearOverlays();
    }

    private void HandleItemInfo()
    {
      List<Gameplay.GameItems.IDs> item_classes = m_Player.Controller.WhatHaveISeen();
      if (null == item_classes || 0>=item_classes.Count) {
        AddMessage(new Data.Message("You have seen no memorable items.", Session.Get.WorldTime.TurnCounter, Color.Yellow));
        return;
      }
      item_classes.Sort();

      Func<int,string> label = index => string.Format("{0}/{1} {2}.", index + 1, item_classes.Count, item_classes[index].ToString());
      Predicate<int> details = index => {
        Gameplay.GameItems.IDs item_type = item_classes[index];
        Dictionary<Location, int> catalog = m_Player.Controller.WhereIs(item_type);
        List<string> tmp = new List<string>();
        foreach(var loc_qty in catalog) {
          tmp.Add(loc_qty.Key.ToString()+": "+loc_qty.Value.ToString());
          if (20<tmp.Count) break;
        }
        ShowSpecialDialogue(m_Player,tmp.ToArray());
        return false;
      };

      PagedMenu("Reviewing...", item_classes.Count, label, details);
    }

    private void HandleAlliesInfo()
    {
      HashSet<Actor> player_allies = m_Player.Allies;
      if (null == player_allies) {
        AddMessage(new Data.Message("You have no nearby allies.", Session.Get.WorldTime.TurnCounter, Color.Yellow));
        return;
      }
      List<Actor> allies = player_allies.ToList();
      allies.Sort((a,b)=> string.Compare(a.Name,b.Name));

      Func<int,string> label = index => allies[index].Name+(allies[index].HasLeader ? "(leader "+allies[index].Leader.Name+")"  : "");
      Predicate<int> details = index => {
        Actor a = allies[index];
        List<string> tmp = new List<string>{a.Name};
        ItemMeleeWeapon best_melee = a.GetBestMeleeWeapon();
        tmp.Add("melee: "+(null == best_melee ? "unarmed" : best_melee.Model.ID.ToString()));
        List<ItemRangedWeapon> ranged = a.Inventory.GetItemsByType<ItemRangedWeapon>();
        if (null != ranged) {
          string msg = "ranged:";
          foreach(ItemRangedWeapon rw in ranged) {
            msg += " "+rw.Model.ID.ToString();
          }
          tmp.Add(msg);
        }
        List<Gameplay.GameItems.IDs> items = (a.Controller as ObjectiveAI).WhatHaveISeen();

        HashSet<GameItems.IDs> critical = (a.Controller as ObjectiveAI).WhatDoINeedNow();
        critical.IntersectWith(items);
;       if (0<critical.Count) {
          string msg = "need now:";
          foreach(GameItems.IDs x in critical) {
            if (60<msg.Length) {
              tmp.Add(msg);
              msg = "need now:";
            }
            msg += " "+x.ToString();
          }
          tmp.Add(msg);
        } else {
          critical = (a.Controller as ObjectiveAI).WhatDoIWantNow();
          critical.IntersectWith(items);
          if (0<critical.Count) {
            string msg = "want now:";
            foreach(GameItems.IDs x in critical) {
              if (60<msg.Length) {
                tmp.Add(msg);
                msg = "want now:";
              }
              msg += " "+x.ToString();
            }
            tmp.Add(msg);
          }
        }

        ShowSpecialDialogue(m_Player,tmp.ToArray());
        return false;
      };

      PagedMenu("Reviewing...", allies.Count, label, details);
    }

    private void HandleDaimonMap()
    {
      if (!Session.Get.CMDoptionExists("socrates-daimon")) return;
      AddMessage(new Data.Message("You pray for wisdom.", Session.Get.WorldTime.TurnCounter, Color.Green));
      Session.Get.World.DaimonMap();
      AddMessage(new Data.Message("Your prayers are unclearly answered.", Session.Get.WorldTime.TurnCounter, Color.Yellow));
    }

    private bool HandleMouseLook(Point mousePos)
    {
      Point pt = MouseToMap(mousePos);
      if (!IsInViewRect(pt)) return false;
      if (!Session.Get.CurrentMap.IsValid(pt)) return true;
      ClearOverlays();
      if (IsVisibleToPlayer(Session.Get.CurrentMap, pt)) {
        Point screen = MapToScreen(pt);
        string[] lines = DescribeStuffAt(Session.Get.CurrentMap, pt);
        if (null == lines) {
          Location? test = Session.Get.CurrentMap.Normalize(pt);
          if (null != test) lines = DescribeStuffAt(test.Value.Map, test.Value.Position);
        }
        if (lines != null) {
          Point screenPos = new Point(screen.X + 32, screen.Y);
          AddOverlay(new RogueGame.OverlayPopup(lines, Color.White, Color.White, POPUP_FILLCOLOR, screenPos));
          if (s_Options.ShowTargets) {
            Actor actorAt = Session.Get.CurrentMap.GetActorAt(pt);
            if (actorAt != null)
              DrawActorTargets(actorAt);
          }
        }
      }
      return true;
    }

    private bool HandleMouseInventory(Point mousePos, MouseButtons? mouseButtons, out bool hasDoneAction)
    {
      Item inventoryItem = MouseToInventoryItem(mousePos, out Inventory inv, out Point itemPos);
      if (inv == null) {
        hasDoneAction = false;
        return false;
      }
      bool isPlayerInventory = inv == m_Player.Inventory;
      hasDoneAction = false;
      ClearOverlays();
      AddOverlay(new OverlayRect(Color.Cyan, new Rectangle(itemPos.X, itemPos.Y, 32, 32)));
      AddOverlay(new OverlayRect(Color.Cyan, new Rectangle(itemPos.X + 1, itemPos.Y + 1, 30, 30)));
      if (inventoryItem != null) {
        string[] lines = DescribeItemLong(inventoryItem, isPlayerInventory);
        int num = 1 + FindLongestLine(lines);
        int x = itemPos.X - 7 * num;
        int y = itemPos.Y + TILE_SIZE;
        AddOverlay(new OverlayPopup(lines, Color.White, Color.White, POPUP_FILLCOLOR, new Point(x, y)));
        if (mouseButtons.HasValue) {
          if (mouseButtons.GetValueOrDefault() == MouseButtons.Left) hasDoneAction = OnLMBItem(inv, inventoryItem);
          else if (mouseButtons.GetValueOrDefault() == MouseButtons.Right) hasDoneAction = OnRMBItem(inv, inventoryItem);
        }
      }
      return true;
    }

    private Item MouseToInventoryItem(Point screen, out Inventory inv, out Point itemPos)
    {
      inv = null;
      itemPos = Point.Empty;
      if (m_Player == null) return null;
      Inventory inventory = m_Player.Inventory;
      if (null == inventory) return null;
      Point inventorySlot1 = MouseToInventorySlot(INVENTORYPANEL_X, INVENTORYPANEL_Y, screen.X, screen.Y);
      int index1 = inventorySlot1.X + inventorySlot1.Y * 10;
      if (index1 >= 0 && index1 < inventory.MaxCapacity) {
        inv = inventory;
        itemPos = InventorySlotToScreen(INVENTORYPANEL_X, INVENTORYPANEL_Y, inventorySlot1.X, inventorySlot1.Y);
        return inventory[index1];
      }
      Inventory itemsAt = m_Player.Location.Items;
      Point inventorySlot2 = MouseToInventorySlot(INVENTORYPANEL_X, GROUNDINVENTORYPANEL_Y, screen.X, screen.Y);
      itemPos = InventorySlotToScreen(INVENTORYPANEL_X, GROUNDINVENTORYPANEL_Y, inventorySlot2.X, inventorySlot2.Y);
      if (itemsAt == null) return null;
      int index2 = inventorySlot2.X + inventorySlot2.Y * 10;
      if (index2 < 0 || index2 >= itemsAt.MaxCapacity) return null;
      inv = itemsAt;
      return itemsAt[index2];
    }

    private bool OnLMBItem(Inventory inv, Item it)
    {
      if (inv == m_Player.Inventory) {
        if (it.IsEquipped) {
          if (m_Player.CanUnequip(it, out string reason)) {
            DoUnequipItem(m_Player, it);
            return false;
          }
          AddMessage(MakeErrorMessage(string.Format("Cannot unequip {0} : {1}.", it.TheName, reason)));
          return false;
        }
        if (it.Model.IsEquipable) {
          if (m_Player.CanEquip(it, out string reason)) {
            DoEquipItem(m_Player, it);
            return false;
          }
          AddMessage(MakeErrorMessage(string.Format("Cannot equip {0} : {1}.", it.TheName, reason)));
          return false;
        }
        if (m_Player.CanUse(it, out string reason1)) {
          DoUseItem(m_Player, it);
          return true;
        }
        AddMessage(MakeErrorMessage(string.Format("Cannot use {0} : {1}.", it.TheName, reason1)));
        return false;
      }
      if (m_Player.CanGet(it, out string reason2)) {
        DoTakeItem(m_Player, m_Player.Location.Position, it);
        return true;
      }
      AddMessage(MakeErrorMessage(string.Format("Cannot take {0} : {1}.", it.TheName, reason2)));
      return false;
    }

    private bool OnRMBItem(Inventory inv, Item it)
    {
      if (inv != m_Player.Inventory) return false;
      if (m_Player.CanDrop(it, out string reason)) {
        DoDropItem(m_Player, it);
        return true;
      }
      AddMessage(MakeErrorMessage(string.Format("Cannot drop {0} : {1}.", it.TheName, reason)));
      return false;
    }

    private bool HandleMouseOverCorpses(Point mousePos, MouseButtons? mouseButtons, out bool hasDoneAction)
    {
      Corpse corpse = MouseToCorpse(mousePos, out Point corpsePos);
      hasDoneAction = false;
      if (corpse == null)  return false;
      ClearOverlays();
      AddOverlay(new RogueGame.OverlayRect(Color.Cyan, new Rectangle(corpsePos.X, corpsePos.Y, 32, 32)));
      AddOverlay(new RogueGame.OverlayRect(Color.Cyan, new Rectangle(corpsePos.X + 1, corpsePos.Y + 1, 30, 30)));

      string[] lines = DescribeCorpseLong(corpse, true);
      int num = 1 + FindLongestLine(lines);
      int x = corpsePos.X - 7 * num;
      int y = corpsePos.Y + TILE_SIZE;
      AddOverlay(new RogueGame.OverlayPopup(lines, Color.White, Color.White, POPUP_FILLCOLOR, new Point(x, y)));
      if (mouseButtons.HasValue) {
        if (mouseButtons.GetValueOrDefault() == MouseButtons.Left) {
          hasDoneAction = OnLMBCorpse(corpse);
        } else if (mouseButtons.GetValueOrDefault() == MouseButtons.Right) {
          hasDoneAction = OnRMBCorpse(corpse);
        }
      }
      return true;
    }

    private Corpse MouseToCorpse(Point screen, out Point corpsePos)
    {
      corpsePos = Point.Empty;
      if (m_Player == null) return null;
      List<Corpse> corpsesAt = m_Player.Location.Map.GetCorpsesAt(m_Player.Location.Position);
      if (corpsesAt == null) return null;
      Point inventorySlot = MouseToInventorySlot(INVENTORYPANEL_X, CORPSESPANEL_Y, screen.X, screen.Y);
      corpsePos = InventorySlotToScreen(INVENTORYPANEL_X, CORPSESPANEL_Y, inventorySlot.X, inventorySlot.Y);
      int index = inventorySlot.X + inventorySlot.Y * 10;
      if (index >= 0 && index < corpsesAt.Count) return corpsesAt[index];
      return null;
    }

    private bool OnLMBCorpse(Corpse c)
    {
      if (c.IsDragged) {
        if (m_Rules.CanActorStopDragCorpse(m_Player, c, out string reason)) {
          DoStopDragCorpse(m_Player);
          return false;
        }
        AddMessage(MakeErrorMessage(string.Format("Cannot stop dragging {0} corpse : {1}.", c.DeadGuy.Name, reason)));
        return false;
      }
      if (m_Rules.CanActorStartDragCorpse(m_Player, c, out string reason1)) {
        DoStartDragCorpse(m_Player, c);
        return false;
      }
      AddMessage(MakeErrorMessage(string.Format("Cannot start dragging {0} corpse : {1}.", c.DeadGuy.Name, reason1)));
      return false;
    }

    private bool OnRMBCorpse(Corpse c)
    {
      if (m_Player.Model.Abilities.IsUndead) {
        if (m_Rules.CanActorEatCorpse(m_Player, c, out string reason)) { // currently automatically succeeds, other than null checks
          DoEatCorpse(m_Player, c);
          return true;
        }
        AddMessage(MakeErrorMessage(string.Format("Cannot eat {0} corpse : {1}.", c.DeadGuy.Name, reason)));
        return false;
      }
      if (m_Rules.CanActorButcherCorpse(m_Player, c, out string reason1)) {
        DoButcherCorpse(m_Player, c);
        return true;
      }
      AddMessage(MakeErrorMessage(string.Format("Cannot butcher {0} corpse : {1}.", c.DeadGuy.Name, reason1)));
      return false;
    }

    private bool HandlePlayerEatCorpse(Actor player, Point mousePos)
    {
      Corpse corpse = MouseToCorpse(mousePos, out Point corpsePos);
      if (corpse == null) return false;
      if (!m_Rules.CanActorEatCorpse(player, corpse, out string reason)) {
        AddMessage(MakeErrorMessage(string.Format("Cannot eat {0} corpse : {1}.", corpse.DeadGuy.Name, reason)));
        return false;
      }
      DoEatCorpse(player, corpse);
      return true;
    }

    private bool HandlePlayerReviveCorpse(Actor player, Point mousePos)
    {
      Corpse corpse = MouseToCorpse(mousePos, out Point corpsePos);
      if (corpse == null) return false;
      if (!m_Rules.CanActorReviveCorpse(player, corpse, out string reason)) {
        AddMessage(MakeErrorMessage(string.Format("Cannot revive {0} : {1}.", corpse.DeadGuy.Name, reason)));
        return false;
      }
      DoReviveCorpse(player, corpse);
      return true;
    }

    public void DoStartDragCorpse(Actor a, Corpse c)
    {
      a.Drag(c);
      if (!ForceVisibleToPlayer(a)) return;
      AddMessage(MakeMessage(a, string.Format("{0} dragging {1} corpse.", Conjugate(a, VERB_START), c.DeadGuy.Name)));
    }

    public void DoStopDragCorpse(Actor a)   // also aliasing former DoStopDraggingCorpses
    {
      Corpse c = a.StopDraggingCorpse();
      if (null == c) return;
      if (!ForceVisibleToPlayer(a)) return;
      AddMessage(MakeMessage(a, string.Format("{0} dragging {1} corpse.", Conjugate(a, VERB_STOP), c.DeadGuy.Name)));
    }

    public void DoButcherCorpse(Actor a, Corpse c)
    {
      bool player = ForceVisibleToPlayer(a);
      a.SpendActionPoints(Rules.BASE_ACTION_COST);
      SeeingCauseInsanity(a, a.Location, Rules.SANITY_HIT_BUTCHERING_CORPSE, string.Format("{0} butchering {1}", a.Name, c.DeadGuy.Name));
      int num = m_Rules.ActorDamageVsCorpses(a);
      if (player) AddMessage(MakeMessage(a, string.Format("{0} {1} corpse for {2} damage.", Conjugate(a, VERB_BUTCHER), c.DeadGuy.Name, num)));
      if (!c.TakeDamage(num)) return;
      a.Location.Map.Destroy(c);
      if (player) AddMessage(new Data.Message(string.Format("{0} corpse is no more.", c.DeadGuy.Name), a.Location.Map.LocalTime.TurnCounter, Color.Purple));
    }

    public void DoEatCorpse(Actor a, Corpse c)
    {
      bool player = ForceVisibleToPlayer(a);
      a.SpendActionPoints(Rules.BASE_ACTION_COST);
      int num = m_Rules.ActorDamageVsCorpses(a);
      if (player) {
        AddMessage(MakeMessage(a, string.Format("{0} {1} corpse.", Conjugate(a, VERB_FEAST_ON), c.DeadGuy.Name)));
        m_MusicManager.Play(GameSounds.UNDEAD_EAT);
      }
      if (c.TakeDamage(num)) {
        a.Location.Map.Destroy(c);
        if (player)
          AddMessage(new Data.Message(string.Format("{0} corpse is no more.", c.DeadGuy.Name), a.Location.Map.LocalTime.TurnCounter, Color.Purple));
      }
      if (a.Model.Abilities.IsUndead) {
        a.RegenHitPoints(Rules.ActorBiteHpRegen(a, num));
        a.RottingEat(a.BiteNutritionValue(num));
      } else {
        a.LivingEat(a.BiteNutritionValue(num));
        a.Infect(Rules.CorpseEatingInfectionTransmission(c.DeadGuy.Infection));
      }
      SeeingCauseInsanity(a, a.Location, a.Model.Abilities.IsUndead ? Rules.SANITY_HIT_UNDEAD_EATING_CORPSE : Rules.SANITY_HIT_LIVING_EATING_CORPSE, string.Format("{0} eating {1}", a.Name, c.DeadGuy.Name));
    }

    public void DoReviveCorpse(Actor actor, Corpse corpse)
    {
      bool player = ForceVisibleToPlayer(actor);
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      Map map = actor.Location.Map;
      List<Point> pointList = actor.Location.Map.FilterAdjacentInMap(actor.Location.Position, pt => !map.HasActorAt(pt) && !map.HasMapObjectAt(pt));
      if (pointList == null) {
        if (!player) return;
        AddMessage(MakeMessage(actor, string.Format("{0} not enough room for reviving {1}.", Conjugate(actor, VERB_HAVE), corpse.DeadGuy.Name)));
      } else {
        Point position = pointList[m_Rules.Roll(0, pointList.Count)];
        int chance = m_Rules.CorpseReviveChance(actor, corpse);
        Item firstMatching = actor.Inventory.GetFirstByModel(GameItems.MEDIKIT);
        actor.Inventory.Consume(firstMatching);
        if (m_Rules.RollChance(chance)) {
          corpse.DeadGuy.IsDead = false;
          corpse.DeadGuy.HitPoints = Rules.CorpseReviveHPs(actor, corpse);
          corpse.DeadGuy.Doll.RemoveDecoration("Actors\\Decoration\\bloodied");
          corpse.DeadGuy.Activity = Activity.IDLE;
          corpse.DeadGuy.TargetActor = null;
          map.Remove(corpse);
          map.PlaceAt(corpse.DeadGuy, position);
          if (player)
            AddMessage(MakeMessage(actor, Conjugate(actor, VERB_REVIVE), corpse.DeadGuy));
          if (actor.IsEnemyOf(corpse.DeadGuy)) return;
          DoSay(corpse.DeadGuy, actor, "Thank you, you saved my life!", RogueGame.Sayflags.NONE);
          corpse.DeadGuy.AddTrustIn(actor, Rules.TRUST_REVIVE_BONUS);
        } else {
          if (!player) return;
          AddMessage(MakeMessage(actor, string.Format("{0} to revive", Conjugate(actor, VERB_FAIL)), corpse.DeadGuy));
        }
      }
    }

    private bool DoPlayerItemSlot(Actor player, int slot, KeyEventArgs key)
    {
      if ((key.Modifiers & Keys.Control) != Keys.None) return DoPlayerItemSlotUse(player, slot);
      if (key.Shift) return DoPlayerItemSlotTake(player, slot);
      if (key.Alt) return DoPlayerItemSlotDrop(player, slot);
      return false;
    }

    private bool DoPlayerItemSlotUse(Actor player, int slot)
    {
      Item it = player.Inventory[slot];
      if (it == null) {
        AddMessage(MakeErrorMessage(string.Format("No item at inventory slot {0}.", slot + 1)));
        return false;
      }
      if (it.IsEquipped) {
        if (player.CanUnequip(it, out string reason)) {
          DoUnequipItem(player, it);
          return false;
        }
        AddMessage(MakeErrorMessage(string.Format("Cannot unequip {0} : {1}.", it.TheName, reason)));
        return false;
      }
      if (it.Model.IsEquipable) {
        if (player.CanEquip(it, out string reason)) {
          DoEquipItem(player, it);
          return false;
        }
        AddMessage(MakeErrorMessage(string.Format("Cannot equip {0} : {1}.", it.TheName, reason)));
        return false;
      }
      if (player.CanUse(it, out string reason1)) {
        DoUseItem(player, it);
        return true;
      }
      AddMessage(MakeErrorMessage(string.Format("Cannot use {0} : {1}.", it.TheName, reason1)));
      return false;
    }

    private bool DoPlayerItemSlotTake(Actor player, int slot)
    {
      Inventory itemsAt = player.Location.Items;
      if (itemsAt == null) {
        AddMessage(MakeErrorMessage("No items on ground."));
        return false;
      }
      Item it = itemsAt[slot];
      if (it == null) {
        AddMessage(MakeErrorMessage(string.Format("No item at ground slot {0}.", slot + 1)));
        return false;
      }
      if (player.CanGet(it, out string reason)) {
        DoTakeItem(player, player.Location.Position, it);
        return true;
      }
      AddMessage(MakeErrorMessage(string.Format("Cannot take {0} : {1}.", it.TheName, reason)));
      return false;
    }

    private bool DoPlayerItemSlotDrop(Actor player, int slot)
    {
      Item it = player.Inventory[slot];
      if (it == null) {
        AddMessage(MakeErrorMessage(string.Format("No item at inventory slot {0}.", slot + 1)));
        return false;
      }
      if (player.CanDrop(it, out string reason)) {
        DoDropItem(player, it);
        return true;
      }
      AddMessage(MakeErrorMessage(string.Format("Cannot drop {0} : {1}.", it.TheName, reason)));
      return false;
    }

    private bool HandlePlayerShout(Actor player, string text)
    {
      if (!player.CanShout(out string reason)) {
        AddMessage(MakeErrorMessage(string.Format("Can't shout : {0}.", reason)));
        return false;
      }
      DoShout(player, text);
      return true;
    }

    private bool HandlePlayerGiveItem(Actor player, Point screen)
    {
      Item inventoryItem = MouseToInventoryItem(screen, out Inventory inv, out Point itemPos);
      if (inv == null || inv != player.Inventory || inventoryItem == null) return false;
      bool flag2 = false;
      ClearOverlays();
      AddOverlay(new RogueGame.OverlayPopup(GIVE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
      do {
        AddMessage(new Data.Message(string.Format("Giving {0} to...", inventoryItem.TheName), Session.Get.WorldTime.TurnCounter, Color.Yellow));
        RedrawPlayScreen();
        Direction direction = WaitDirectionOrCancel();
        if (null == direction) break;
        if (Direction.NEUTRAL == direction) continue;
        Point point = player.Location.Position + direction;
        if (!player.Location.Map.IsValid(point)) continue;
        Actor actorAt = player.Location.Map.GetActorAt(point);
        if (actorAt != null) {
          if (player.CanGiveTo(actorAt, inventoryItem, out string reason)) {
            flag2 = true;
            DoGiveItemTo(player, actorAt, inventoryItem);
            break;
          }
          AddMessage(MakeErrorMessage(string.Format("Can't give {0} to {1} : {2}.", inventoryItem.TheName, actorAt.TheName, reason)));
          continue;
        } else if (m_Rules.CanActorPutItemIntoContainer(player, point)) {
          flag2 = true;
          DoPutItemInContainer(player, point, inventoryItem);
          break;
        }
        AddMessage(MakeErrorMessage("Noone there."));
      }
      while (true);
      ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerInitiateTrade(Actor player, Point screen)
    {
      Item inventoryItem = MouseToInventoryItem(screen, out Inventory inv, out Point itemPos);
      if (inv == null || inv != player.Inventory || inventoryItem == null) return false;
      bool flag1 = true;
      bool flag2 = false;
      ClearOverlays();
      AddOverlay(new RogueGame.OverlayPopup(INITIATE_TRADE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
      do {
        AddMessage(new Data.Message(string.Format("Trading {0} with...", inventoryItem.TheName), Session.Get.WorldTime.TurnCounter, Color.Yellow));
        RedrawPlayScreen();
        Direction direction = WaitDirectionOrCancel();
        if (direction == null) flag1 = false;
        else if (direction != Direction.NEUTRAL) {
          Point point = player.Location.Position + direction;
          if (player.Location.Map.IsValid(point)) {
            Actor actorAt = player.Location.Map.GetActorAt(point);
            if (actorAt != null) {
              if (player.CanTradeWith(actorAt, out string reason)) {
                flag2 = true;
                flag1 = false;
                ClearOverlays();
                RedrawPlayScreen();
                DoTrade(player, inventoryItem, actorAt, true);
              } else
                AddMessage(MakeErrorMessage(string.Format("Can't trade with {0} : {1}.", actorAt.TheName, reason)));
            } else
              AddMessage(MakeErrorMessage("Noone there."));
          }
        }
      }
      while (flag1);
      ClearOverlays();
      return flag2;
    }

    private void HandlePlayerRunToggle(Actor player)
    {
      if (!player.CanRun(out string reason) && !player.IsRunning) {
        AddMessage(MakeErrorMessage(string.Format("Cannot run now : {0}.", reason)));
      } else {
        player.IsRunning = !player.IsRunning;
        AddMessage(MakeMessage(player, string.Format("{0} running.", Conjugate(player, player.IsRunning ? VERB_START : VERB_STOP))));
      }
    }

    private bool HandlePlayerCloseDoor(Actor player)
    {
      bool flag1 = true;
      bool flag2 = false;
      ClearOverlays();
      AddOverlay(new RogueGame.OverlayPopup(CLOSE_DOOR_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
      do {
        RedrawPlayScreen();
        Direction direction = WaitDirectionOrCancel();
        if (direction == null) flag1 = false;
        else if (direction != Direction.NEUTRAL) {
          Point point = player.Location.Position + direction;
          if (player.Location.Map.IsInBounds(point)) {  // doors never generate on map edges so IsInBounds ok
            MapObject mapObjectAt = player.Location.Map.GetMapObjectAt(point);
            if (mapObjectAt is DoorWindow door) {
              if (player.CanClose(door, out string reason)) {
                DoCloseDoor(player, door);
                RedrawPlayScreen();
                flag1 = false;
                flag2 = true;
              } else
                AddMessage(MakeErrorMessage(string.Format("Can't close {0} : {1}.", door.TheName, reason)));
            } else
              AddMessage(MakeErrorMessage("Nothing to close there."));
          }
        }
      }
      while (flag1);
      ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerBarricade(Actor player)
    {
      bool flag1 = true;
      bool flag2 = false;
      ClearOverlays();
      AddOverlay(new OverlayPopup(BARRICADE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
      do {
        RedrawPlayScreen();
        Direction direction = WaitDirectionOrCancel();
        if (direction == null) flag1 = false;
        else if (direction != Direction.NEUTRAL) {
          Point point = player.Location.Position + direction;
          if (player.Location.Map.IsValid(point)) {
            MapObject mapObjectAt = player.Location.Map.GetMapObjectAt(point);
            if (mapObjectAt != null) {
              if (mapObjectAt is DoorWindow door) {
                if (player.CanBarricade(door, out string reason)) {
                  DoBarricadeDoor(player, door);
                  RedrawPlayScreen();
                  flag1 = false;
                  flag2 = true;
                } else
                  AddMessage(MakeErrorMessage(string.Format("Cannot barricade {0} : {1}.", door.TheName, reason)));
              } else if (mapObjectAt is Fortification fort) {
                if (m_Rules.CanActorRepairFortification(player, fort, out string reason)) {
                  DoRepairFortification(player, fort);
                  RedrawPlayScreen();
                  flag1 = false;
                  flag2 = true;
                } else
                  AddMessage(MakeErrorMessage(string.Format("Cannot repair {0} : {1}.", fort.TheName, reason)));
              } else
                AddMessage(MakeErrorMessage(string.Format("{0} cannot be repaired or barricaded.", mapObjectAt.TheName)));
            } else
              AddMessage(MakeErrorMessage("Nothing to barricade there."));
          }
        }
      }
      while (flag1);
      ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerBreak(Actor player)
    {
      bool flag1 = true;
      bool flag2 = false;
      ClearOverlays();
      AddOverlay(new RogueGame.OverlayPopup(BREAK_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
      do {
        RedrawPlayScreen();
        Direction direction = WaitDirectionOrCancel();
        if (direction == null) flag1 = false;
        else if (direction == Direction.NEUTRAL) {
          Exit exitAt = player.Location.Exit;
          if (exitAt == null) AddMessage(MakeErrorMessage("No exit there."));
          else {
            Actor actorAt = exitAt.Location.Actor;
            string reason;
            if (actorAt != null) {
              if (player.IsEnemyOf(actorAt)) {
                if (player.CanMeleeAttack(actorAt, out reason)) {
                  DoMeleeAttack(player, actorAt);
                  flag1 = false;
                  flag2 = true;
                } else
                  AddMessage(MakeErrorMessage(string.Format("Cannot attack {0} : {1}.", actorAt.Name, reason)));
              } else
                AddMessage(MakeErrorMessage(string.Format("{0} is not your enemy.", actorAt.Name)));
            } else {
              MapObject mapObjectAt = exitAt.Location.MapObject;
              if (mapObjectAt != null) {
                if (player.CanBreak(mapObjectAt, out reason)) {
                  DoBreak(player, mapObjectAt);
                  flag1 = false;
                  flag2 = true;
                } else
                  AddMessage(MakeErrorMessage(string.Format("Cannot break {0} : {1}.", mapObjectAt.TheName, reason)));
              } else
                AddMessage(MakeErrorMessage("Nothing to break or attack on the other side."));
            }
          }
        } else {
          Point point = player.Location.Position + direction;
          if (player.Location.Map.IsValid(point)) {
            MapObject mapObjectAt = player.Location.Map.GetMapObjectAt(point);
            if (mapObjectAt != null) {
              if (player.CanBreak(mapObjectAt, out string reason)) {
                DoBreak(player, mapObjectAt);
                RedrawPlayScreen();
                flag1 = false;
                flag2 = true;
              } else
                AddMessage(MakeErrorMessage(string.Format("Cannot break {0} : {1}.", mapObjectAt.TheName, reason)));
            } else
              AddMessage(MakeErrorMessage("Nothing to break there."));
          }
        }
      }
      while (flag1);
      ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerBuildFortification(Actor player, bool isLarge)
    {
      if (player.Sheet.SkillTable.GetSkillLevel(Skills.IDs.CARPENTRY) == 0) {
        AddMessage(MakeErrorMessage("need carpentry skill."));
        return false;
      }
      int num = Rules.ActorBarricadingMaterialNeedForFortification(player, isLarge);
      if (player.CountItems<ItemBarricadeMaterial>() < num) {
        AddMessage(MakeErrorMessage(string.Format("not enough barricading material, need {0}.", num)));
        return false;
      }
      bool flag1 = true;
      bool flag2 = false;
      ClearOverlays();
      AddOverlay(new RogueGame.OverlayPopup(isLarge ? BUILD_LARGE_FORT_MODE_TEXT : BUILD_SMALL_FORT_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
      do {
        RedrawPlayScreen();
        Direction direction = WaitDirectionOrCancel();
        if (direction == null) flag1 = false;
        else if (direction != Direction.NEUTRAL) {
          Point point = player.Location.Position + direction;
          if (player.Location.Map.IsValid(point)) {
            if (m_Rules.CanActorBuildFortification(player, point, isLarge, out string reason)) {
              DoBuildFortification(player, point, isLarge);
              RedrawPlayScreen();
              flag1 = false;
              flag2 = true;
            } else
              AddMessage(MakeErrorMessage(string.Format("Cannot build here : {0}.", reason)));
          }
        }
      }
      while (flag1);
      ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerFireMode(Actor player)
    {
      bool flag1 = true;
      bool flag2 = false;
      if (player.GetEquippedWeapon() is ItemGrenade || player.GetEquippedWeapon() is ItemGrenadePrimed)
        return HandlePlayerThrowGrenade(player);
      ItemRangedWeapon itemRangedWeapon = player.GetEquippedWeapon() as ItemRangedWeapon;
      if (itemRangedWeapon == null) {
        AddMessage(MakeErrorMessage("No weapon ready to fire."));
        RedrawPlayScreen();
        return false;
      }
      if (itemRangedWeapon.Ammo <= 0) {
        AddMessage(MakeErrorMessage("No ammo left."));
        RedrawPlayScreen();
        return false;
      }
      HashSet<Point> fovFor = LOS.ComputeFOVFor(player);
      List<Actor> enemiesInFov = player.GetEnemiesInFov(fovFor);
      if (enemiesInFov == null || enemiesInFov.Count == 0) {
        AddMessage(MakeErrorMessage("No targets to fire at."));
        RedrawPlayScreen();
        return false;
      }
      Attack attack = player.RangedAttack(0);
      int index = 0;
      List<Point> LoF = new List<Point>(attack.Range);
      FireMode mode = FireMode.DEFAULT;
      do {
        Actor actor = enemiesInFov[index];
        LoF.Clear();
        bool flag3 = player.CanFireAt(actor, LoF, out string reason);
        int num1 = Rules.GridDistance(player.Location, actor.Location);
        ClearOverlays();
        AddOverlay(new RogueGame.OverlayPopup(FIRE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
        AddOverlay(new OverlayImage(MapToScreen(actor.Location), GameImages.ICON_TARGET));
        string imageID = flag3 ? (num1 <= attack.EfficientRange ? GameImages.ICON_LINE_CLEAR : GameImages.ICON_LINE_BAD) : GameImages.ICON_LINE_BLOCKED;
        foreach (Point mapPosition in LoF)
          AddOverlay(new OverlayImage(MapToScreen(mapPosition), imageID));
        RedrawPlayScreen();
        KeyEventArgs key = m_UI.UI_WaitKey();
        int num2 = (int) InputTranslator.KeyToCommand(key);
        if (key.KeyCode == Keys.Escape) flag1 = false;
        else if (key.KeyCode == Keys.T) index = (index + 1) % enemiesInFov.Count;
        else if (key.KeyCode == Keys.M) {
          mode = (FireMode) ((int) (mode + 1) % 2);
          AddMessage(new Data.Message(string.Format("Switched to {0} fire mode.", mode.ToString()), Session.Get.WorldTime.TurnCounter, Color.Yellow));
        } else if (key.KeyCode == Keys.F) {
          if (flag3) {
            DoSingleRangedAttack(player, actor, LoF, mode);
            RedrawPlayScreen();
            flag1 = false;
            flag2 = true;
          } else
            AddMessage(MakeErrorMessage(string.Format("Can't fire at {0} : {1}.", actor.TheName, reason)));
        }
      }
      while (flag1);
      ClearOverlays();
      return flag2;
    }

    private void HandlePlayerMarkEnemies(Actor player)
    {
      if (player.Model.Abilities.IsUndead) {
        AddMessage(MakeErrorMessage("Undeads can't have personal enemies."));
      } else {
        Map map = player.Location.Map;
        List<Actor> actorList = new List<Actor>();
        foreach (Point position in m_Player.Controller.FOV) {
          Actor actorAt = map.GetActorAt(position);
          if (actorAt != null && actorAt!=player)
            actorList.Add(actorAt);
        }
        if (actorList.Count == 0) {
          AddMessage(MakeErrorMessage("No visible actors to mark."));
          RedrawPlayScreen();
        } else {
          bool flag1 = true;
          int index = 0;
          do {
            Actor target = actorList[index];
            ClearOverlays();
            AddOverlay(new RogueGame.OverlayPopup(MARK_ENEMIES_MODE, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
            AddOverlay(new OverlayImage(MapToScreen(target.Location), GameImages.ICON_TARGET));
            RedrawPlayScreen();
            KeyEventArgs key = m_UI.UI_WaitKey();
            int num = (int) InputTranslator.KeyToCommand(key);
            if (key.KeyCode == Keys.Escape) flag1 = false;
            else if (key.KeyCode == Keys.T) index = (index + 1) % actorList.Count;
            else if (key.KeyCode == Keys.E) {
              bool flag2 = true;
              if (target.Leader == player) {
                AddMessage(MakeErrorMessage("Can't make a follower your enemy."));
                flag2 = false;
              } else if (player.Leader == target) {
                AddMessage(MakeErrorMessage("Can't make your leader your enemy."));
                flag2 = false;
              } else if (m_Player.IsEnemyOf(target)) {
                AddMessage(MakeErrorMessage("Already enemies."));
                flag2 = false;
              }
              if (flag2) {
                AddMessage(new Data.Message(string.Format("{0} is now a personal enemy.", target.TheName), Session.Get.WorldTime.TurnCounter, Color.Orange));
                DoMakeAggression(player, target);
              }
            }
          }
          while (flag1);
          ClearOverlays();
        }
      }
    }

    private bool HandlePlayerThrowGrenade(Actor player)
    {
      ItemGrenade itemGrenade = player.GetEquippedWeapon() as ItemGrenade;
      ItemGrenadePrimed itemGrenadePrimed = player.GetEquippedWeapon() as ItemGrenadePrimed;
      if (itemGrenade == null && itemGrenadePrimed == null) {
        AddMessage(MakeErrorMessage("No grenade to throw."));
        RedrawPlayScreen();
        return false;
      }
      bool flag1 = true;
      bool flag2 = false;
      ItemGrenadeModel itemGrenadeModel = itemGrenade == null ? itemGrenadePrimed.Model.GrenadeModel : itemGrenade.Model;
      Map map = player.Location.Map;
      Point point1 = player.Location.Position;
      int num = player.MaxThrowRange(itemGrenadeModel.MaxThrowDistance);
      List<Point> LoF = new List<Point>();
      do {
        LoF.Clear();
        bool flag3 = player.CanThrowTo(point1, out string reason, LoF);
        ClearOverlays();
        AddOverlay(new RogueGame.OverlayPopup(THROW_GRENADE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
        string imageID = flag3 ? GameImages.ICON_LINE_CLEAR : GameImages.ICON_LINE_BLOCKED;
        foreach (Point mapPosition in LoF)
          AddOverlay(new OverlayImage(MapToScreen(mapPosition), imageID));
        RedrawPlayScreen();
        KeyEventArgs key = m_UI.UI_WaitKey();
        PlayerCommand command = InputTranslator.KeyToCommand(key);
        if (key.KeyCode == Keys.Escape) flag1 = false;
        else if (key.KeyCode == Keys.F) {
          if (flag3) {
            bool flag4 = true;
            if (Rules.GridDistance(player.Location.Position, point1) <= itemGrenadeModel.BlastAttack.Radius) {
              ClearMessages();
              AddMessage(new Data.Message("You are in the blast radius!", Session.Get.WorldTime.TurnCounter, Color.Yellow));
              AddMessage(MakeYesNoMessage("Really throw there"));
              RedrawPlayScreen();
              flag4 = WaitYesOrNo();
              ClearMessages();
              RedrawPlayScreen();
            }
            if (flag4) {
              if (itemGrenade != null)
                DoThrowGrenadeUnprimed(player, point1);
              else
                DoThrowGrenadePrimed(player, point1);
              RedrawPlayScreen();
              flag1 = false;
              flag2 = true;
            }
          } else
            AddMessage(MakeErrorMessage(string.Format("Can't throw there : {0}.", reason)));
        } else {
          Direction direction = RogueGame.CommandToDirection(command);
          if (direction != null) {
            Point point2 = point1 + direction;
            if (map.IsValid(point2) && Rules.GridDistance(player.Location.Position, point2) <= num)
              point1 = point2;
          }
        }
      }
      while (flag1);
      ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerSleep(Actor player)
    {
      if (!player.CanSleep(out string reason)) {
        AddMessage(MakeErrorMessage(string.Format("Cannot sleep now : {0}.", reason)));
        return false;
      }
      AddMessage(MakeYesNoMessage("Really sleep there"));
      RedrawPlayScreen();
      if (!WaitYesOrNo()) {
        AddMessage(new Data.Message("Good, keep those eyes wide open.", Session.Get.WorldTime.TurnCounter, Color.Yellow));
        return false;
      }
      AddMessage(new Data.Message("Goodnight, happy nightmares!", Session.Get.WorldTime.TurnCounter, Color.Yellow));
      DoStartSleeping(player);
      RedrawPlayScreen();
      m_MusicManager.StopAll();
      m_MusicManager.PlayLooping(GameMusics.SLEEP);
      return true;
    }

    private bool HandlePlayerSwitchPlace(Actor player)
    {
      ClearOverlays();
      AddOverlay(new OverlayPopup(SWITCH_PLACE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));

      bool flag2 = false;
      do {
        RedrawPlayScreen();
        Direction direction = WaitDirectionOrCancel();
        if (direction == null) break;
        else if (direction != Direction.NEUTRAL) {
          Point point = player.Location.Position + direction;
          if (player.Location.Map.IsValid(point)) {
            Actor actorAt = player.Location.Map.GetActorAt(point);
            if (actorAt != null) {
              if (player.CanSwitchPlaceWith(actorAt, out string reason)) {
                flag2 = true;
                DoSwitchPlace(player, actorAt);
                break;
              }
              else
                AddMessage(MakeErrorMessage(string.Format("Can't switch place : {0}", reason)));
            } else
              AddMessage(MakeErrorMessage("Noone there."));
          }
        }
      }
      while(true);
      ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerTakeLead(Actor player)
    {
      ClearOverlays();
      AddOverlay(new OverlayPopup(TAKE_LEAD_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));

      bool flag2 = false;
      do {
        RedrawPlayScreen();
        Direction direction = WaitDirectionOrCancel();
        if (direction == null) break;
        else if (direction != Direction.NEUTRAL) {
          Point point = player.Location.Position + direction;
          if (player.Location.Map.IsValid(point)) {
            Actor actorAt = player.Location.Map.GetActorAt(point);
            if (actorAt != null) {
              if (player.CanTakeLeadOf(actorAt, out string reason)) {
                flag2 = true;
                DoTakeLead(player, actorAt);
                Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format("Recruited {0}.", actorAt.TheName));
                AddMessage(new Data.Message("(you can now set directives and orders for your new follower).", Session.Get.WorldTime.TurnCounter, Color.White));
                AddMessage(new Data.Message(string.Format("(to give order : press <{0}>).", RogueGame.s_KeyBindings.Get(PlayerCommand.ORDER_MODE).ToString()), Session.Get.WorldTime.TurnCounter, Color.White));
                break;
              } else if (actorAt.Leader == player) {
                if (m_Rules.CanActorCancelLead(player, actorAt, out reason)) {
                  AddMessage(MakeYesNoMessage(string.Format("Really ask {0} to leave", actorAt.TheName)));
                  RedrawPlayScreen();
                  if (WaitYesOrNo()) {
                    flag2 = true;
                    DoCancelLead(player, actorAt);
                    Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format("Fired {0}.", actorAt.TheName));
                    break;
                  } else
                    AddMessage(new Data.Message("Good, together you are strong.", Session.Get.WorldTime.TurnCounter, Color.Yellow));
                } else
                  AddMessage(MakeErrorMessage(string.Format("{0} can't leave : {1}.", actorAt.TheName, reason)));
              } else
                AddMessage(MakeErrorMessage(string.Format("Can't lead {0} : {1}.", actorAt.TheName, reason)));
            } else
              AddMessage(MakeErrorMessage("Noone there."));
          }
        }
      }
      while(true);
      ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerPush(Actor player)
    {
      if (!player.AbleToPush) {
        AddMessage(MakeErrorMessage("Cannot push objects."));
        return false;
      }
      if (player.IsTired) {
        AddMessage(MakeErrorMessage("Too tired to push."));
        return false;
      }
      ClearOverlays();

      bool flag2 = false;
      do {
        AddOverlay(new RogueGame.OverlayPopup(PUSH_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
        RedrawPlayScreen();
        Direction direction = WaitDirectionOrCancel();
        if (direction == null) break;
        else if (direction != Direction.NEUTRAL) {
          Point point = player.Location.Position + direction;
          if (player.Location.Map.IsValid(point)) {
            Actor actorAt = player.Location.Map.GetActorAt(point);
            MapObject mapObjectAt = player.Location.Map.GetMapObjectAt(point);
            string reason;
            if (actorAt != null) {
              if (player.CanShove(actorAt, out reason)) {
                if (HandlePlayerShoveActor(player, actorAt)) {
                  flag2 = true;
                  break;
                }
              } else
                AddMessage(MakeErrorMessage(string.Format("Cannot shove {0} : {1}.", actorAt.TheName, reason)));
            } else if (mapObjectAt != null) {
              if (player.CanPush(mapObjectAt, out reason)) {
                if (HandlePlayerPushObject(player, mapObjectAt)) {
                  flag2 = true;
                  break;
                }
              } else
                AddMessage(MakeErrorMessage(string.Format("Cannot move {0} : {1}.", mapObjectAt.TheName, reason)));
            } else
              AddMessage(MakeErrorMessage("Nothing to push there."));
          }
        }
      }
      while(true);
      ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerPushObject(Actor player, MapObject mapObj)
    {
      ClearOverlays();
      AddOverlay(new OverlayPopup(new string[1] { string.Format(PUSH_OBJECT_MODE_TEXT,  mapObj.TheName) }, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
      AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(mapObj.Location), SIZE_OF_TILE)));

      bool flag2 = false;
      do {
        RedrawPlayScreen();
        Direction direction = WaitDirectionOrCancel();
        if (direction == null) break;
        else if (direction != Direction.NEUTRAL) {
          Point point = mapObj.Location.Position + direction;
          if (player.Location.Map.IsValid(point)) {
            if (mapObj.CanPushTo(point, out string reason)) {
              DoPush(player, mapObj, point);
              flag2 = true;
              break;
            } else
              AddMessage(MakeErrorMessage(string.Format("Cannot move {0} there : {1}.", mapObj.TheName, reason)));
          }
        }
      }
      while(true);
      ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerShoveActor(Actor player, Actor other)
    {
      ClearOverlays();
      AddOverlay(new OverlayPopup(new string[1] { string.Format(SHOVE_ACTOR_MODE_TEXT,  other.TheName) }, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
      AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(other.Location), SIZE_OF_ACTOR)));

      bool flag2 = false;
      do {
        RedrawPlayScreen();
        Direction direction = WaitDirectionOrCancel();
        if (direction == null) break;
        else if (direction != Direction.NEUTRAL) {
          Point point = other.Location.Position + direction;
          if (player.Location.Map.IsValid(point)) {
            if (other.CanBeShovedTo(point, out string reason)) {
              DoShove(player, other, point);
              flag2 = true;
              break;
            } else
              AddMessage(MakeErrorMessage(string.Format("Cannot shove {0} there : {1}.", other.TheName, reason)));
          }
        }
      }
      while(true);
      ClearOverlays();
      return flag2;
    }

    private bool HandlePlayerUseSpray(Actor player)
    {
      Item equippedItem = player.GetEquippedItem(DollPart.LEFT_HAND);
      if (equippedItem == null) {
        AddMessage(MakeErrorMessage("No spray equipped."));
        RedrawPlayScreen();
        return false;
      }
      if (equippedItem is ItemSprayPaint) return HandlePlayerTag(player);
      if (equippedItem is ItemSprayScent spray) {
        if (!player.CanUse(spray, out string reason)) {
          AddMessage(MakeErrorMessage(string.Format("Can't use the spray : {0}.", reason)));
          RedrawPlayScreen();
          return false;
        }
        DoUseSprayScentItem(player, spray);
        return true;
      }
      AddMessage(MakeErrorMessage("No spray equipped."));
      RedrawPlayScreen();
      return false;
    }

    private bool HandlePlayerTag(Actor player)
    {
      ItemSprayPaint spray = player.GetEquippedItem(DollPart.LEFT_HAND) as ItemSprayPaint;
      if (spray == null) {
        AddMessage(MakeErrorMessage("No spray paint equipped."));
        RedrawPlayScreen();
        return false;
      }
      if (spray.PaintQuantity <= 0) {
        AddMessage(MakeErrorMessage("No paint left."));
        RedrawPlayScreen();
        return false;
      }
      ClearOverlays();
      AddOverlay(new OverlayPopup(TAG_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));

      bool flag2 = false;
      do {
        RedrawPlayScreen();
        Direction direction = WaitDirectionOrCancel();
        if (direction == null) break;
        else if (direction != Direction.NEUTRAL) {
          Point point = player.Location.Position + direction;
          if (player.Location.Map.IsInBounds(point)) {
            if (CanTag(player.Location.Map, point, out string reason)) {
              DoTag(player, spray, point);
              flag2 = true;
              break;
            } else {
              AddMessage(MakeErrorMessage(string.Format("Can't tag there : {0}.", reason)));
              RedrawPlayScreen();
            }
          }
        }
      }
      while(true);
      ClearOverlays();
      return flag2;
    }

    private bool CanTag(Map map, Point pos, out string reason)
    {
      if (!map.IsInBounds(pos)) {
        reason = "out of map";
        return false;
      }
      if (map.HasActorAt(pos)) {
        reason = "someone there";
        return false;
      }
      if (map.HasMapObjectAt(pos)) {
        reason = "something there";
        return false;
      }
      reason = "";
      return true;
    }

#if SUICIDE_BY_LONG_WAIT
    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void StartPlayerWaitLong(Actor player)
    {
      m_IsPlayerLongWait = true;
      m_IsPlayerLongWaitForcedStop = false;
      m_PlayerLongWaitEnd = new WorldTime(Session.Get.WorldTime.TurnCounter + WorldTime.TURNS_PER_HOUR);
      AddMessage(MakeMessage(player, string.Format("{0} waiting.", (object) Conjugate(player, VERB_START))));
      RedrawPlayScreen();
    }

    private bool CheckPlayerWaitLong(Actor player)
    {
      if (m_IsPlayerLongWaitForcedStop || Session.Get.WorldTime.TurnCounter >= m_PlayerLongWaitEnd.TurnCounter || (player.IsHungry || player.IsStarving) || (player.IsSleepy || player.IsExhausted))
        return false;
      foreach (Point position in m_Player.Controller.FOV) {
        Actor actorAt = player.Location.Map.GetActorAt(position);
        if (actorAt != null && player.IsEnemyOf(actorAt)) return false;
      }
      return !TryPlayerInsanity();
    }
#endif

    private bool HandlePlayerOrderMode(Actor player)
    {
      if (player.CountFollowers == 0) {
        AddMessage(MakeErrorMessage("No followers to give orders to."));
        return false;
      }
      Actor[] actorArray = new Actor[player.CountFollowers];
      HashSet<Point>[] pointSetArray = new HashSet<Point>[player.CountFollowers];
      bool[] flagArray = new bool[player.CountFollowers];
      int index1 = 0;
      foreach (Actor follower in player.Followers) {
        actorArray[index1] = follower;
        pointSetArray[index1] = LOS.ComputeFOVFor(follower);
        bool flag1 = pointSetArray[index1].Contains(player.Location.Position) && m_Player.Controller.FOV.Contains(follower.Location.Position);
        bool flag2 = AreLinkedByPhone(player, follower);
        flagArray[index1] = flag1 || flag2;
        ++index1;
      }
      if (player.CountFollowers == 1 && flagArray[0]) {
        bool flag = HandlePlayerOrderFollower(player, actorArray[0]);
        ClearOverlays();
        ClearMessages();
        return flag;
      }
      bool flag3 = true;
      bool flag4 = false;
      int num1 = 0;
      do {
        ClearOverlays();
        AddOverlay(new RogueGame.OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
        ClearMessages();
        AddMessage(new Data.Message("Choose a follower.", Session.Get.WorldTime.TurnCounter, Color.Yellow));
        int num2;
        for (num2 = 0; num2 < 5 && num1 + num2 < actorArray.Length; ++num2) {
          int index2 = num2 + num1;
          Actor follower = actorArray[index2];
          string str = DescribePlayerFollowerStatus(follower);
          if (flagArray[index2])
            AddMessage(new Data.Message(string.Format("{0}. {1}/{2} {3}... {4}.", 1 + num2, index2 + 1, actorArray.Length, follower.Name, str), Session.Get.WorldTime.TurnCounter, Color.LightGreen));
          else
            AddMessage(new Data.Message(string.Format("{0}. {1}/{2} ({3}) {4}.", 1 + num2, index2 + 1, actorArray.Length, follower.Name, str), Session.Get.WorldTime.TurnCounter, Color.DarkGray));
        }
        if (num2 < actorArray.Length)
          AddMessage(new Data.Message("9. next", Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        RedrawPlayScreen();
        KeyEventArgs keyEventArgs = m_UI.UI_WaitKey();
        int choiceNumber = KeyToChoiceNumber(keyEventArgs.KeyCode);
        if (keyEventArgs.KeyCode == Keys.Escape) flag3 = false;
        else if (choiceNumber == 9) {
          num1 += 5;
          if (num1 >= actorArray.Length) num1 = 0;
        } else if (choiceNumber >= 1 && choiceNumber <= num2) {
          int index2 = num1 + choiceNumber - 1;
          if (flagArray[index2]) {
            Actor follower = actorArray[index2];
            if (HandlePlayerOrderFollower(player, follower)) {
              flag3 = false;
              flag4 = true;
            }
          }
        }
      }
      while (flag3);
      ClearOverlays();
      ClearMessages();
      return flag4;
    }

    private bool HandlePlayerDirectiveFollower(Actor player, Actor follower)    // XXX could be void return but other HandlePlayer... return bool
    {
      bool flag1 = true;
      do {
        ActorDirective directives = (follower.Controller as OrderableAI).Directives;
        ClearOverlays();
        AddOverlay(new RogueGame.OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
        ClearMessages();
        AddMessage(new Data.Message(string.Format("{0} directives...", follower.Name), Session.Get.WorldTime.TurnCounter, Color.Yellow));
        AddMessage(new Data.Message(string.Format("1. {0} items.", directives.CanTakeItems ? "Take" : "Don't take"), Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        AddMessage(new Data.Message(string.Format("2. {0} weapons.", directives.CanFireWeapons ? "Fire" : "Don't fire"), Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        AddMessage(new Data.Message(string.Format("3. {0} grenades.", directives.CanThrowGrenades ? "Throw" : "Don't throw"), Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        AddMessage(new Data.Message(string.Format("4. {0}.", directives.CanSleep ? "Sleep" : "Don't sleep"), Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        AddMessage(new Data.Message(string.Format("5. {0}.", directives.CanTrade ? "Trade" : "Don't trade"), Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        AddMessage(new Data.Message(string.Format("6. {0}.", ActorDirective.CourageString(directives.Courage)), Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        RedrawPlayScreen();
        KeyEventArgs keyEventArgs = m_UI.UI_WaitKey();
        int choiceNumber = KeyToChoiceNumber(keyEventArgs.KeyCode);
        if (keyEventArgs.KeyCode == Keys.Escape) flag1 = false;
        else {
          switch (choiceNumber) {
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
              switch (directives.Courage) {
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
      return false;
    }

    private bool HandlePlayerOrderFollower(Actor player, Actor follower)
    {
      if (!follower.IsTrustingLeader) {
        if (IsVisibleToPlayer(follower))
          DoSay(follower, player, "Sorry, I don't trust you enough yet.", RogueGame.Sayflags.IS_IMPORTANT | RogueGame.Sayflags.IS_FREE_ACTION);
        else if (AreLinkedByPhone(follower, player)) {
          ClearMessages();
          AddMessage(MakeMessage(follower, "Sorry, I don't trust you enough yet."));
          AddMessagePressEnter();
        }
        return false;
      }
      string str1 = DescribePlayerFollowerStatus(follower);
      HashSet<Point> fovFor = LOS.ComputeFOVFor(follower);
      bool flag1 = true;
      bool flag2 = false;
      do {
        string str2 = (follower.Controller as OrderableAI).DontFollowLeader ? "Start" : "Stop";
        ClearOverlays();
        AddOverlay(new RogueGame.OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
        ClearMessages();
        AddMessage(new Data.Message(string.Format("Order {0} to...", follower.Name), Session.Get.WorldTime.TurnCounter, Color.Yellow));
        AddMessage(new Data.Message(string.Format("0. Cancel current order {0}.", str1), Session.Get.WorldTime.TurnCounter, Color.Green));
        AddMessage(new Data.Message("1. Set directives...", Session.Get.WorldTime.TurnCounter, Color.Cyan));
        AddMessage(new Data.Message("2. Barricade (one)...    6. Drop all items.      A. Give me...", Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        AddMessage(new Data.Message("3. Barricade (max)...    7. Build small fort.    B. Sleep now.", Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        AddMessage(new Data.Message(string.Format("4. Guard...              8. Build large fort.    C. {0} following me.   ", str2), Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        AddMessage(new Data.Message("5. Patrol...             9. Report events.       D. Where are you?", Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        RedrawPlayScreen();
        KeyEventArgs keyEventArgs = m_UI.UI_WaitKey();
        int choiceNumber = KeyToChoiceNumber(keyEventArgs.KeyCode);
        if (keyEventArgs.KeyCode == Keys.Escape) flag1 = false;
        else if (choiceNumber >= 0 && choiceNumber <= 9) {
          switch (choiceNumber) {
            case 0:
              DoCancelOrder(player, follower);
              flag1 = false;
              flag2 = true;
              break;
            case 1:
              HandlePlayerDirectiveFollower(player, follower);
              break;
            case 2:
              if (HandlePlayerOrderFollowerToBarricade(player, follower, fovFor, false)) {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
            case 3:
              if (HandlePlayerOrderFollowerToBarricade(player, follower, fovFor, true)) {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
            case 4:
              if (HandlePlayerOrderFollowerToGuard(player, follower, fovFor))
              {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
            case 5:
              if (HandlePlayerOrderFollowerToPatrol(player, follower, fovFor))
              {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
            case 6:
              if (HandlePlayerOrderFollowerToDropAllItems(player, follower))
              {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
            case 7:
              if (HandlePlayerOrderFollowerToBuildFortification(player, follower, fovFor, false))
              {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
            case 8:
              if (HandlePlayerOrderFollowerToBuildFortification(player, follower, fovFor, true))
              {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
            case 9:
              if (HandlePlayerOrderFollowerToReport(player, follower))
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
              if (HandlePlayerOrderFollowerToGiveItems(player, follower))
              {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
            case Keys.B:
              if (HandlePlayerOrderFollowerToSleep(player, follower))
              {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
            case Keys.C:
              if (HandlePlayerOrderFollowerToToggleFollow(player, follower))
              {
                flag1 = false;
                flag2 = true;
                break;
              }
              break;
            case Keys.D:
              if (HandlePlayerOrderFollowerToReportPosition(player, follower)) {
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
      do {
        ClearOverlays();
        AddOverlay(new OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
        if (nullable.HasValue)
          AddOverlay(new OverlayRect(color, new Rectangle(MapToScreen(nullable.Value), SIZE_OF_TILE)));
        ClearMessages();
        AddMessage(new Data.Message(string.Format("Ordering {0} to build {1} fortification...", follower.Name, isLarge ? "large" : "small"), Session.Get.WorldTime.TurnCounter, Color.Yellow));
        AddMessage(new Data.Message("Left-Click on a map object.", Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        RedrawPlayScreen();
        WaitKeyOrMouse(out KeyEventArgs key, out Point mousePos, out MouseButtons? mouseButtons);
        if (key != null) {
          if (key.KeyCode == Keys.Escape) flag1 = false;
        } else {
          Point map2 = MouseToMap(mousePos);
          if (map1.IsValid(map2) && IsInViewRect(map2)) {
            if (IsVisibleToPlayer(map1, map2) && followerFOV.Contains(map2)) {
              if (m_Rules.CanActorBuildFortification(follower, map2, isLarge, out string reason)) {
                nullable = map2;
                color = Color.LightGreen;
                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left) {
                  DoGiveOrderTo(player, follower, new ActorOrder(isLarge ? ActorTasks.BUILD_LARGE_FORTIFICATION : ActorTasks.BUILD_SMALL_FORTIFICATION, new Location(player.Location.Map, map2)));
                  flag1 = false;
                  flag2 = true;
                }
              } else {
                nullable = map2;
                color = Color.Red;
                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left) {
                  AddMessage(MakeErrorMessage(string.Format("Can't build {0} fortification : {1}.", isLarge ? "large" : "small", reason)));
                  AddMessagePressEnter();
                }
              }
            } else {
              nullable = map2;
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
      Point? nullable = null;
      Color color = Color.White;
      do {
        ClearOverlays();
        AddOverlay(new OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
        if (nullable.HasValue)
          AddOverlay(new OverlayRect(color, new Rectangle(MapToScreen(nullable.Value), SIZE_OF_TILE)));
        ClearMessages();
        AddMessage(new Data.Message(string.Format("Ordering {0} to barricade...", follower.Name), Session.Get.WorldTime.TurnCounter, Color.Yellow));
        AddMessage(new Data.Message("Left-Click on a map object.", Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        RedrawPlayScreen();
        WaitKeyOrMouse(out KeyEventArgs key, out Point mousePos, out MouseButtons? mouseButtons);
        if (key != null) {
          if (key.KeyCode == Keys.Escape) flag1 = false;
        } else {
          Point map2 = MouseToMap(mousePos);
          if (map1.IsValid(map2) && IsInViewRect(map2)) {
            nullable = map2;
            if (IsVisibleToPlayer(map1, map2) && followerFOV.Contains(map2)) {
              if (map1.GetMapObjectAt(map2) is DoorWindow door) {
                if (follower.CanBarricade(door, out string reason)) {
                  color = Color.LightGreen;
                  if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left) {
                    DoGiveOrderTo(player, follower, new ActorOrder(toTheMax ? ActorTasks.BARRICADE_MAX : ActorTasks.BARRICADE_ONE, door.Location));
                    flag1 = false;
                    flag2 = true;
                  }
                } else {
                  color = Color.Red;
                  if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left) {
                    AddMessage(MakeErrorMessage(string.Format("Can't barricade {0} : {1}.", door.TheName, reason)));
                    AddMessagePressEnter();
                  }
                }
              } else color = Color.Red;
            } else color = Color.Red;
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
      Point? nullable = null;
      Color color = Color.White;
      do {
        ClearOverlays();
        AddOverlay(new OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
        if (nullable.HasValue)
          AddOverlay(new OverlayRect(color, new Rectangle(MapToScreen(nullable.Value), SIZE_OF_TILE)));
        ClearMessages();
        AddMessage(new Data.Message(string.Format("Ordering {0} to guard...", follower.Name), Session.Get.WorldTime.TurnCounter, Color.Yellow));
        AddMessage(new Data.Message("Left-Click on a map position.", Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        RedrawPlayScreen();
        WaitKeyOrMouse(out KeyEventArgs key, out Point mousePos, out MouseButtons? mouseButtons);
        if (key != null) {
          if (key.KeyCode == Keys.Escape) flag1 = false;
        } else {
          Point map2 = MouseToMap(mousePos);
          if (map1.IsValid(map2) && IsInViewRect(map2)) {
            if (IsVisibleToPlayer(map1, map2) && followerFOV.Contains(map2)) {
              if (map2 == follower.Location.Position || map1.IsWalkableFor(map2, follower, out string reason)) {
                nullable = map2;
                color = Color.LightGreen;
                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left) {
                  DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.GUARD, new Location(map1, map2)));
                  flag1 = false;
                  flag2 = true;
                }
              } else {
                nullable = map2;
                color = Color.Red;
                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left) {
                  AddMessage(MakeErrorMessage(string.Format("Can't guard here : {0}", reason)));
                  AddMessagePressEnter();
                }
              }
            } else {
              nullable = map2;
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
      do {
        ClearOverlays();
        AddOverlay(new OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
        if (nullable.HasValue) {
          AddOverlay(new OverlayRect(color, new Rectangle(MapToScreen(nullable.Value), SIZE_OF_TILE)));
          List<Zone> zonesAt = map1.GetZonesAt(nullable.Value);
          if (null != zonesAt) {
            string[] lines = new string[zonesAt.Count + 1];
            lines[0] = "Zone(s) here :";
            for (int index = 0; index < zonesAt.Count; ++index)
              lines[index + 1] = string.Format("- {0}", zonesAt[index].Name);
            AddOverlay(new OverlayPopup(lines, Color.White, Color.White, POPUP_FILLCOLOR, MapToScreen(nullable.Value.X + 1, nullable.Value.Y + 1)));
          }
        }
        ClearMessages();
        AddMessage(new Data.Message(string.Format("Ordering {0} to patrol...", follower.Name), Session.Get.WorldTime.TurnCounter, Color.Yellow));
        AddMessage(new Data.Message("Left-Click on a map position.", Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        RedrawPlayScreen();
        WaitKeyOrMouse(out KeyEventArgs key, out Point mousePos, out MouseButtons? mouseButtons);
        if (key != null) {
          if (key.KeyCode == Keys.Escape) flag1 = false;
        } else {
          Point map2 = MouseToMap(mousePos);
          if (map1.IsValid(map2) && IsInViewRect(map2)) {
            if (IsVisibleToPlayer(map1, map2) && followerFOV.Contains(map2)) {
              bool flag3 = true;
              string reason = "";
              if (map1.GetZonesAt(map2) == null) {
                flag3 = false;
                reason = "no zone here";
              } else if (!(map2 == follower.Location.Position) && !map1.IsWalkableFor(map2, follower, out reason))
                flag3 = false;
              nullable = map2;
              if (flag3) {
                color = Color.LightGreen;
                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left) {
                  DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.PATROL, new Location(map1, map2)));
                  flag1 = false;
                  flag2 = true;
                }
              } else {
                color = Color.Red;
                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left) {
                  AddMessage(MakeErrorMessage(string.Format("Can't patrol here : {0}", reason)));
                  AddMessagePressEnter();
                }
              }
            } else {
              nullable = map2;
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
            DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.DROP_ALL_ITEMS, follower.Location));
            DoSay(follower, player, "Well ok...", RogueGame.Sayflags.IS_FREE_ACTION);
      ModifyActorTrustInLeader(follower, follower.Inventory.CountItems * Rules.TRUST_GIVE_ITEM_ORDER_PENALTY, true);
      return true;
    }

    private bool HandlePlayerOrderFollowerToReport(Actor player, Actor follower)
    {
            DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.REPORT_EVENTS, follower.Location));
      return true;
    }

    private bool HandlePlayerOrderFollowerToSleep(Actor player, Actor follower)
    {
            DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.SLEEP_NOW, follower.Location));
      return true;
    }

    private bool HandlePlayerOrderFollowerToToggleFollow(Actor player, Actor follower)
    {
            DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.FOLLOW_TOGGLE, follower.Location));
      return true;
    }

    private bool HandlePlayerOrderFollowerToReportPosition(Actor player, Actor follower)
    {
      DoGiveOrderTo(player, follower, new ActorOrder(ActorTasks.WHERE_ARE_YOU, follower.Location));
      return true;
    }

    private bool HandlePlayerOrderFollowerToGiveItems(Actor player, Actor follower)
    {
      if (follower.Inventory == null || follower.Inventory.IsEmpty) {
        ClearMessages();
        AddMessage(MakeErrorMessage(string.Format("{0} has no items to give.", follower.TheName)));
        AddMessagePressEnter();
        return false;
      }
      if (!Rules.IsAdjacent(player.Location, follower.Location)) {
        ClearMessages();
        AddMessage(MakeErrorMessage(string.Format("{0} is not next to you.", follower.TheName)));
        AddMessagePressEnter();
        return false;
      }
      bool flag1 = true;
      bool flag2 = false;
      int num1 = 0;
      Inventory inventory = follower.Inventory;
      do {
        ClearOverlays();
        AddOverlay(new RogueGame.OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, new Point(0, 0)));
        ClearMessages();
        AddMessage(new Data.Message(string.Format("Ordering {0} to give...", follower.Name), Session.Get.WorldTime.TurnCounter, Color.Yellow));
        int num2;
        for (num2 = 0; num2 < 5 && num1 + num2 < inventory.CountItems; ++num2) {
          int index = num1 + num2;
          AddMessage(new Data.Message(string.Format("{0}. {1}/{2} {3}.", 1 + num2, index + 1, inventory.CountItems, DescribeItemShort(inventory[index])), Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        }
        if (num2 < inventory.CountItems)
          AddMessage(new Data.Message("9. next", Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        RedrawPlayScreen();
        KeyEventArgs keyEventArgs = m_UI.UI_WaitKey();
        int choiceNumber = KeyToChoiceNumber(keyEventArgs.KeyCode);
        if (keyEventArgs.KeyCode == Keys.Escape) flag1 = false;
        else if (choiceNumber == 9) {
          num1 += 5;
          if (num1 >= inventory.CountItems) num1 = 0;
        } else if (choiceNumber >= 1 && choiceNumber <= num2) {
          int index = num1 + choiceNumber - 1;
          Item obj = inventory[index];
          if (follower.CanGiveTo(player, obj, out string reason)) {
            DoGiveItemTo(follower, player, obj);
            flag1 = false;
            flag2 = true;
          } else {
            ClearMessages();
            AddMessage(MakeErrorMessage(string.Format("{0} cannot give {1} : {2}.", follower.TheName, DescribeItemShort(obj), reason)));
            AddMessagePressEnter();
          }
        }
      }
      while (flag1);
      return flag2;
    }

    private void HandlePlayerTakeItemFromContainer(Actor player, Point src)
    {
      Inventory inv = player.Location.Map.GetItemsAt(src);
      if (null == inv) throw new ArgumentNullException(nameof(src),"no inventory at ("+src.X.ToString()+","+src.Y.ToString()+")");
      if (2 > inv.CountItems) throw new ArgumentOutOfRangeException(nameof(inv),"inventory was not a stack");
      if (1 != Rules.GridDistance(player.Location.Position,src)) throw new ArgumentOutOfRangeException(nameof(src), "("+src.X.ToString()+", "+src.Y.ToString()+") not adjacent");

      Func<int,string> label = index => string.Format("{0}/{1} {2}.", index + 1, inv.CountItems, DescribeItemShort(inv[index]));
      Predicate<int> details = index => {
        Item obj = inv[index];
        if (player.CanGet(obj, out string reason)) {
          DoTakeItem(player, src, obj);
          return true;
        }
        ClearMessages();
        AddMessage(MakeErrorMessage(string.Format("{0} take {1} : {2}.", player.TheName, DescribeItemShort(obj), reason)));
        AddMessagePressEnter();
        return false;
      };

      PagedMenu("Taking...", inv.CountItems, label, details);
    }

    private void HandleAiActor(Actor aiActor)
    {
      Contract.Requires(null != aiActor);
      Contract.Requires(!aiActor.IsSleeping);
      ActorAction actorAction = aiActor.Controller.GetAction(this);
      if (aiActor.IsInsane && m_Rules.RollChance(Rules.SANITY_INSANE_ACTION_CHANCE)) {
        ActorAction insaneAction = GenerateInsaneAction(aiActor);
        if (insaneAction != null && insaneAction.IsLegal())
          actorAction = insaneAction;
      }
      // we need to know if this got past internal testing.
#if DEBUG
#else
      if (actorAction == null) throw new InvalidOperationException("AI returned null action.");
      if (!actorAction.IsLegal()) throw new InvalidOperationException(string.Format("AI attempted illegal action {0}; actorAI: {1}; fail reason : {2}.", actorAction.GetType().ToString(), aiActor.Controller.GetType().ToString(), actorAction.FailReason));
#endif
      if (aiActor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "action: "+actorAction.ToString());
      actorAction.Perform();
    }

    private void HandleAdvisor(Actor player)
    {
      if (s_Hints.HasAdvisorGivenAllHints()) {
        ShowAdvisorMessage("YOU KNOW THE BASICS!", new string[7]
        {
          "The Advisor has given you all the hints.",
          "You can disable the advisor in the options.",
          "Read the manual or discover the rest of the game by yourself.",
          "Good luck and have fun!",
          string.Format("To REDEFINE THE KEYS : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.KEYBINDING_MODE).ToString()),
          string.Format("To CHANGE OPTIONS    : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.OPTIONS_MODE).ToString()),
          string.Format("To READ THE MANUAL   : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.HELP_MODE).ToString())
        });
      } else {
        for (int index = 0; index < (int)AdvisorHint._COUNT; ++index) {
          if (!s_Hints.IsAdvisorHintGiven((AdvisorHint) index) && IsAdvisorHintAppliable((AdvisorHint) index)) {
            AdvisorGiveHint((AdvisorHint) index);
            return;
          }
        }
        ShowAdvisorMessage("No hint available.", new string[5]
        {
          "The Advisor has now new hint for you in this situation.",
          "You will see a popup when he has something to say.",
          string.Format("To REDEFINE THE KEYS : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.KEYBINDING_MODE).ToString()),
          string.Format("To CHANGE OPTIONS    : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.OPTIONS_MODE).ToString()),
          string.Format("To READ THE MANUAL   : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.HELP_MODE).ToString())
        });
      }
    }

    private bool HasAdvisorAnyHintToGive()
    {
      for (int index = 0; index < (int) AdvisorHint._COUNT; ++index)
      {
        if (!s_Hints.IsAdvisorHintGiven((AdvisorHint) index) && IsAdvisorHintAppliable((AdvisorHint) index))
          return true;
      }
      return false;
    }

    private void AdvisorGiveHint(AdvisorHint hint)
    {
      s_Hints.SetAdvisorHintAsGiven(hint);
      SaveHints();
      ShowAdvisorHint(hint);
    }

    private bool IsAdvisorHintAppliable(AdvisorHint hint)
    {
      Map map = m_Player.Location.Map;
      Point position = m_Player.Location.Position;
      switch (hint)
      {
        case AdvisorHint.MOVE_BASIC:
          return true;
        case AdvisorHint.MOUSE_LOOK:
          return map.LocalTime.TurnCounter >= 2;
        case AdvisorHint.KEYS_OPTIONS:
          return true;
        case AdvisorHint.NIGHT:
          return map.LocalTime.TurnCounter >= WorldTime.TURNS_PER_HOUR;
        case AdvisorHint.RAIN:
          if (Session.Get.World.Weather.IsRain())
            return map.LocalTime.TurnCounter >= 2*WorldTime.TURNS_PER_HOUR;
          return false;
        case AdvisorHint.ACTOR_MELEE:
          return m_Player.IsAdjacentToEnemy;
        case AdvisorHint.MOVE_RUN:
          if (map.LocalTime.TurnCounter >= 5)
            return m_Player.CanRun();
          return false;
        case AdvisorHint.MOVE_RESTING:
          return m_Player.IsTired;
        case AdvisorHint.MOVE_JUMP:
          if (!m_Player.IsTired)
            return map.HasAnyAdjacentInMap(position, pt =>
           {
               MapObject mapObjectAt = map.GetMapObjectAt(pt);
               if (mapObjectAt == null) return false;
               return mapObjectAt.IsJumpable;
           });
          return false;
        case AdvisorHint.ITEM_GRAB_CONTAINER:
          return map.HasAnyAdjacentInMap(position, pt => m_Player.CanGetFromContainer(pt));
        case AdvisorHint.ITEM_GRAB_FLOOR:
          Inventory itemsAt = map.GetItemsAt(position);
          if (itemsAt == null) return false;
          foreach (Item it in itemsAt.Items) {
            if (m_Player.CanGet(it)) return true;
          }
          return false;
        case AdvisorHint.ITEM_UNEQUIP:
          Inventory inventory1 = m_Player.Inventory;
          if (inventory1 == null || inventory1.IsEmpty) return false;
          foreach (Item it in inventory1.Items) {
            if (m_Player.CanUnequip(it)) return true;
          }
          return false;
        case AdvisorHint.ITEM_EQUIP:
          Inventory inventory2 = m_Player.Inventory;
          if (inventory2 == null || inventory2.IsEmpty) return false;
          foreach (Item it in inventory2.Items) {
            if (!it.IsEquipped && m_Player.CanEquip(it)) return true;
          }
          return false;
        case AdvisorHint.ITEM_TYPE_BARRICADING:
          Inventory inventory3 = m_Player.Inventory;
          if (inventory3 == null || inventory3.IsEmpty) return false;
          return inventory3.Has<ItemBarricadeMaterial>();
        case AdvisorHint.ITEM_DROP:
          Inventory inventory4 = m_Player.Inventory;
          if (inventory4 == null || inventory4.IsEmpty)
            return false;
          foreach (Item it in inventory4.Items) {
            if (m_Player.CanDrop(it)) return true;
          }
          return false;
        case AdvisorHint.ITEM_USE:
          Inventory inventory5 = m_Player.Inventory;
          if (inventory5 == null || inventory5.IsEmpty) return false;
          foreach (Item it in inventory5.Items) {
            if (m_Player.CanUse(it)) return true;
          }
          return false;
        case AdvisorHint.FLASHLIGHT:
          return m_Player.Inventory.Has<ItemLight>();
        case AdvisorHint.CELLPHONES:
          return m_Player.Inventory.GetFirstByModel(GameItems.CELL_PHONE) != null;
        case AdvisorHint.SPRAYS_PAINT:
          return m_Player.Inventory.Has<ItemSprayPaint>();
        case AdvisorHint.SPRAYS_SCENT:
          return m_Player.Inventory.Has<ItemSprayScent>();
        case AdvisorHint.WEAPON_FIRE:
          ItemRangedWeapon itemRangedWeapon = m_Player.GetEquippedWeapon() as ItemRangedWeapon;
          if (itemRangedWeapon == null)
            return false;
          return itemRangedWeapon.Ammo >= 0;
        case AdvisorHint.WEAPON_RELOAD:
          if (!(m_Player.GetEquippedWeapon() is ItemRangedWeapon)) return false;
          Inventory inventory6 = m_Player.Inventory;
          if (inventory6 == null || inventory6.IsEmpty) return false;
          foreach (Item it in inventory6.Items) {
            if (it is ItemAmmo && m_Player.CanUse(it)) return true;
          }
          return false;
        case AdvisorHint.GRENADE:
          return m_Player.Has<ItemGrenade>();
        case AdvisorHint.DOORWINDOW_OPEN:
          return map.HasAnyAdjacentInMap(position, pt =>
         {
             DoorWindow door = map.GetMapObjectAt(pt) as DoorWindow;
             return null != door && m_Player.CanOpen(door);
         });
        case AdvisorHint.DOORWINDOW_CLOSE:
          return map.HasAnyAdjacentInMap(position, pt =>
         {
             DoorWindow door = map.GetMapObjectAt(pt) as DoorWindow;
             return null != door && m_Player.CanClose(door);
         });
        case AdvisorHint.OBJECT_PUSH:
          return map.HasAnyAdjacentInMap(position, pt =>
         {
             MapObject mapObjectAt = map.GetMapObjectAt(pt);
             return null != mapObjectAt && m_Player.CanPush(mapObjectAt);
         });
        case AdvisorHint.OBJECT_BREAK:
          return map.HasAnyAdjacentInMap(position, pt =>
         {
             MapObject mapObjectAt = map.GetMapObjectAt(pt);
             return null != mapObjectAt && m_Player.CanBreak(mapObjectAt);
         });
        case AdvisorHint.BARRICADE:
          return map.HasAnyAdjacentInMap(position, pt =>
         {
             DoorWindow door = map.GetMapObjectAt(pt) as DoorWindow;
             return null != door && m_Player.CanBarricade(door);
         });
        case AdvisorHint.EXIT_STAIRS_LADDERS: return map.HasExitAt(position);
        case AdvisorHint.EXIT_LEAVING_DISTRICT:
          foreach (Direction direction in Direction.COMPASS) {
            Point point = position + direction;
            if (!map.IsInBounds(point) && map.HasExitAt(point)) return true;
          }
          return false;
        case AdvisorHint.STATE_SLEEPY: return m_Player.IsSleepy;
        case AdvisorHint.STATE_HUNGRY: return m_Player.IsHungry;
        case AdvisorHint.NPC_TRADE:
          return map.HasAnyAdjacentInMap(position, pt =>
         {
             Actor actorAt = map.GetActorAt(pt);
             if (actorAt == null) return false;
             return m_Player.CanTradeWith(actorAt);
         });
        case AdvisorHint.NPC_GIVING_ITEM:
          Inventory inventory8 = m_Player.Inventory;
          if (inventory8 == null || inventory8.IsEmpty) return false;
          return map.HasAnyAdjacentInMap(position, pt =>
         {
             Actor actorAt = map.GetActorAt(pt);
             if (actorAt == null) return false;
             return !m_Player.IsEnemyOf(actorAt);
         });
        case AdvisorHint.NPC_SHOUTING:
          return map.HasAnyAdjacentInMap(position, pt =>
         {
             Actor actorAt = map.GetActorAt(pt);
             if (actorAt == null || !actorAt.IsSleeping) return false;
             return !m_Player.IsEnemyOf(actorAt);
         });
        case AdvisorHint.BUILD_FORTIFICATION:
          return map.HasAnyAdjacentInMap(position, pt => m_Rules.CanActorBuildFortification(m_Player, pt, false));
        case AdvisorHint.LEADING_NEED_SKILL:
          return map.HasAnyAdjacentInMap(position, pt =>
         {
             Actor actorAt = map.GetActorAt(pt);
             if (actorAt == null) return false;
             return !m_Player.IsEnemyOf(actorAt);
         });
        case AdvisorHint.LEADING_CAN_RECRUIT:
          return map.HasAnyAdjacentInMap(position, pt =>
         {
             Actor actorAt = map.GetActorAt(pt);
             if (actorAt == null) return false;
             return m_Player.CanTakeLeadOf(actorAt);
         });
        case AdvisorHint.LEADING_GIVE_ORDERS:
          return m_Player.CountFollowers > 0;
        case AdvisorHint.LEADING_SWITCH_PLACE:
          return map.HasAnyAdjacentInMap(position, pt =>
         {
             Actor actorAt = map.GetActorAt(pt);
             return (null != actorAt && m_Player.CanSwitchPlaceWith(actorAt));
         });
        case AdvisorHint.GAME_SAVE_LOAD:
          return map.LocalTime.Hour >= 7;
        case AdvisorHint.CITY_INFORMATION:
          return map.LocalTime.Hour >= 12;
        case AdvisorHint.CORPSE_BUTCHER:
          if (!m_Player.Model.Abilities.IsUndead)
            return map.GetCorpsesAt(position) != null;
          return false;
        case AdvisorHint.CORPSE_EAT:
          if (m_Player.Model.Abilities.IsUndead)
            return map.GetCorpsesAt(position) != null;
          return false;
        case AdvisorHint.CORPSE_DRAG_START:
          if (m_Player.DraggedCorpse == null)
            return map.GetCorpsesAt(position) != null;
          return false;
        case AdvisorHint.CORPSE_DRAG_MOVE:
          return m_Player.DraggedCorpse != null;
        default:
          throw new ArgumentOutOfRangeException("unhandled hint");
      }
    }

    static private void GetAdvisorHintText(AdvisorHint hint, out string title, out string[] body)
    {
      switch (hint)
      {
        case AdvisorHint.MOVE_BASIC:
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
            string.Format("You can view and redefine the KEYS by pressing <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.KEYBINDING_MODE).ToString()),
            string.Format("You can change OPTIONS by pressing <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.OPTIONS_MODE).ToString()),
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
            string.Format("To TOGGLE RUNNING : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.RUN_TOGGLE).ToString())
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
            string.Format("To REST/WAIT : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.WAIT_OR_SELF).ToString())
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
            string.Format("To USE THE SPRAY : move the mouse over the item and press <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.USE_SPRAY).ToString())
          };
          break;
        case AdvisorHint.SPRAYS_SCENT:
          title = "SPRAYS - SCENT SPRAY";
          body = new string[4]
          {
            "You have found a scent spray.",
            "You can spray some perfume on the tile you are standing.",
            "This is useful to confuse the undeads that chase using their smell.",
            string.Format("To USE THE SPRAY : move the mouse over the item and press <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.USE_SPRAY).ToString())
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
            string.Format("To FIRE : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.FIRE_MODE).ToString()),
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
            string.Format("To FIRE : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.FIRE_MODE).ToString())
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
            string.Format("To CLOSE : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.CLOSE_DOOR).ToString())
          };
          break;
        case AdvisorHint.OBJECT_PUSH:
          title = "PUSHING OBJECTS";
          body = new string[4]
          {
            "You can PUSH an object around you.",
            "Only MOVABLE objects can be pushed.",
            "Movable objects will be described as 'Can be moved'",
            string.Format("To PUSH : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.PUSH_MODE).ToString())
          };
          break;
        case AdvisorHint.OBJECT_BREAK:
          title = "BREAKING OBJECTS";
          body = new string[3]
          {
            "You can try to BREAK an object around you.",
            "Typical breakable objects are furnitures, doors and windows.",
            string.Format("To BREAK : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.BREAK_MODE).ToString())
          };
          break;
        case AdvisorHint.BARRICADE:
          title = "BARRICADING A DOOR/WINDOW";
          body = new string[3]
          {
            "You can barricade an adjacent door or window.",
            "Barricading uses material such as planks.",
            string.Format("To BARRICADE : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.BARRICADE_MODE).ToString())
          };
          break;
        case AdvisorHint.EXIT_STAIRS_LADDERS:
          title = "USING STAIRS & LADDERS";
          body = new string[3]
          {
            "You are standing on stairs or a ladder.",
            "You can use this exit to go on another map.",
            string.Format("To USE THE EXIT : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.USE_EXIT).ToString())
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
            string.Format("To SLEEP : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.SLEEP).ToString()),
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
            string.Format("To INITIATE THE TRADE : move the mouse over your item and press <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.INITIATE_TRADE).ToString())
          };
          break;
        case AdvisorHint.NPC_GIVING_ITEM:
          title = "GIVING ITEMS";
          body = new string[2]
          {
            "You can GIVE ITEMS to other actors.",
            string.Format("To GIVE AN ITEM : move the mouse over your item and press <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.GIVE_ITEM).ToString())
          };
          break;
        case AdvisorHint.NPC_SHOUTING:
          title = "SHOUTING";
          body = new string[4]
          {
            "Someone is sleeping near you.",
            "You can SHOUT to try to wake him or her up.",
            "Other actors can also shout to wake their friends up when they see danger.",
            string.Format("To SHOUT : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.SHOUT).ToString())
          };
          break;
        case AdvisorHint.BUILD_FORTIFICATION:
          title = "BUILDING FORTIFICATIONS";
          body = new string[4]
          {
            "You can now build fortifications thanks to the carpentry skill.",
            "You need enough barricading materials.",
            string.Format("To BUILD SMALL FORTIFICATIONS : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.BUILD_SMALL_FORTIFICATION).ToString()),
            string.Format("To BUILD LARGE FORTIFICATIONS : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.BUILD_LARGE_FORTIFICATION).ToString())
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
            string.Format("To RECRUIT : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.LEAD_MODE).ToString())
          };
          break;
        case AdvisorHint.LEADING_GIVE_ORDERS:
          title = "LEADING - GIVING ORDERS";
          body = new string[4]
          {
            "You can give orders and directives to your follower.",
            "You can also fire your followers.",
            string.Format("To GIVE ORDERS : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.ORDER_MODE).ToString()),
            string.Format("To FIRE YOUR FOLLOWER : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.LEAD_MODE).ToString())
          };
          break;
        case AdvisorHint.LEADING_SWITCH_PLACE:
          title = "LEADING - SWITCHING PLACE";
          body = new string[2]
          {
            "You can switch place with followers next to you.",
            string.Format("To SWITCH PLACE : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.SWITCH_PLACE).ToString())
          };
          break;
        case AdvisorHint.GAME_SAVE_LOAD:
          title = "SAVING AND LOADING GAME";
          body = new string[7]
          {
            "Now could be a good time to save your game.",
            "You can have only one save game active.",
            string.Format("To SAVE THE GAME : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.SAVE_GAME).ToString()),
            string.Format("To LOAD THE GAME : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.LOAD_GAME).ToString()),
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
            string.Format("To VIEW THE CITY INFORMATION : <{0}>.",  RogueGame.s_KeyBindings.Get(PlayerCommand.CITY_INFO).ToString())
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
      GetAdvisorHintText(hint, out string title, out string[] body);
      ShowAdvisorMessage(title, body);
    }

    private void ShowAdvisorMessage(string title, string[] lines)
    {
      ClearMessages();
      ClearOverlays();
      string[] lines1 = new string[lines.Length + 2];
      lines1[0] = "HINT : " + title;
      Array.Copy(lines, 0, lines1, 1, lines.Length);
      lines1[lines.Length + 1] = string.Format("(hint {0}/{1})", s_Hints.CountAdvisorHintsGiven(), (int)AdvisorHint._COUNT);
      AddOverlay(new RogueGame.OverlayPopup(lines1, Color.White, Color.White, Color.Black, new Point(0, 0)));
      ClearMessages();
      AddMessage(new Data.Message("You can disable the advisor in the options screen.", Session.Get.WorldTime.TurnCounter, Color.White));
      AddMessage(new Data.Message(string.Format("To show the options screen : <{0}>.", RogueGame.s_KeyBindings.Get(PlayerCommand.OPTIONS_MODE).ToString()), Session.Get.WorldTime.TurnCounter, Color.White));
      AddMessagePressEnter();
      ClearMessages();
      ClearOverlays();
      RedrawPlayScreen();
    }

    private void WaitKeyOrMouse(out KeyEventArgs key, out Point mousePos, out MouseButtons? mouseButtons)
    {
      m_UI.UI_PeekKey();
      Point mousePosition = m_UI.UI_GetMousePosition();
      mousePos = new Point(-1, -1);
      mouseButtons = new MouseButtons?();
      do {
        KeyEventArgs keyEventArgs = m_UI.UI_PeekKey();
        if (keyEventArgs != null) {
          key = keyEventArgs;
          return;
        }
        mousePos = m_UI.UI_GetMousePosition();
        mouseButtons = m_UI.UI_PeekMouseButtons();
      }
      while (!(mousePos != mousePosition) && !mouseButtons.HasValue);
      key = null;
    }

    private Direction WaitDirectionOrCancel()
    {
      Direction direction;
      do
      {
        KeyEventArgs key = m_UI.UI_WaitKey();
        PlayerCommand command = InputTranslator.KeyToCommand(key);
        if (key.KeyCode == Keys.Escape)
          return null;
        direction = RogueGame.CommandToDirection(command);
      }
      while (direction == null);
      return direction;
    }

    private void WaitEnter()
    {
      do
        ;
      while (m_UI.UI_WaitKey().KeyCode != Keys.Return);
    }

    private void WaitEscape()
    {
      do
        ;
      while (m_UI.UI_WaitKey().KeyCode != Keys.Escape);
    }

    static private int KeyToChoiceNumber(Keys key)
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
        keyEventArgs = m_UI.UI_WaitKey();
        if (keyEventArgs.KeyCode == Keys.Y)
          return true;
      }
      while (keyEventArgs.KeyCode != Keys.N && keyEventArgs.KeyCode != Keys.Escape);
      return false;
    }

    private string[] DescribeStuffAt(Map map, Point mapPos)
    {
      Actor actorAt = map.GetActorAt(mapPos);
      if (actorAt != null) return DescribeActor(actorAt);
      MapObject mapObjectAt = map.GetMapObjectAt(mapPos);
      if (mapObjectAt != null) return DescribeMapObject(mapObjectAt, map, mapPos);
      Inventory itemsAt = map.GetItemsAt(mapPos);
      if (itemsAt != null) return DescribeInventory(itemsAt);
      List<Corpse> corpsesAt = map.GetCorpsesAt(mapPos);
      if (corpsesAt != null) return DescribeCorpses(corpsesAt);
      return null;
    }

    private string[] DescribeActor(Actor actor)
    {
      List<string> stringList = new List<string>(10);
      if (actor.Faction != null) {
        if (actor.IsInAGang)
          stringList.Add(string.Format("{0}, {1}-{2}.", Capitalize(actor.Name), actor.Faction.MemberName, GameGangs.NAMES[(int)actor.GangID]));
        else
          stringList.Add(string.Format("{0}, {1}.", Capitalize(actor.Name), actor.Faction.MemberName));
      } else
        stringList.Add(string.Format("{0}.", Capitalize(actor.Name)));
      stringList.Add(string.Format("{0}.", Capitalize(actor.Model.Name)));
      stringList.Add(string.Format("{0} since {1}.", actor.Model.Abilities.IsUndead ? "Undead" : "Staying alive", new WorldTime(actor.SpawnTime).ToString()));
      OrderableAI aiController = actor.Controller as OrderableAI;
      if (aiController?.Order != null) stringList.Add(string.Format("Order : {0}.", aiController.Order.ToString()));
      if (actor.HasLeader) {
        if (actor.Leader.IsPlayer) {
          if (actor.TrustInLeader >= Actor.TRUST_BOND_THRESHOLD)
            stringList.Add(string.Format("Trust : BOND."));
          else if (actor.TrustInLeader >= Rules.TRUST_MAX)
            stringList.Add("Trust : MAX.");
          else
            stringList.Add(string.Format("Trust : {0}/T:{1}-B:{2}.", actor.TrustInLeader, Actor.TRUST_TRUSTING_THRESHOLD, Rules.TRUST_MAX));
          if (aiController is OrderableAI orderableAi && orderableAi.DontFollowLeader)
            stringList.Add("Ordered to not follow you.");
          stringList.Add(string.Format("Foo : {0} {1}h", actor.FoodPoints, actor.HoursUntilHungry));
          stringList.Add(string.Format("Slp : {0} {1}h", actor.SleepPoints, actor.HoursUntilSleepy));
          stringList.Add(string.Format("San : {0} {1}h", actor.Sanity, actor.HoursUntilUnstable));
          stringList.Add(string.Format("Inf : {0} {1}%", actor.Infection, actor.InfectionPercent));
        } else
          stringList.Add(string.Format("Leader : {0}.", Capitalize(actor.Leader.Name)));
      }
      if (actor.MurdersCounter > 0 && m_Player.Model.Abilities.IsLawEnforcer) {
        stringList.Add("WANTED FOR MURDER!");
        stringList.Add(string.Format("{0}!", "murder".QtyDesc(actor.MurdersCounter)));
      } else if (actor.HasLeader && actor.Leader.IsPlayer && actor.IsTrustingLeader) {
        stringList.Add(actor.MurdersCounter > 0
                     ? string.Format("* Confess {0}! *", "murder".QtyDesc(actor.MurdersCounter))
                     : "Has committed no murders.");
      }
      if (actor.IsAggressorOf(m_Player)) stringList.Add("Aggressed you.");
      if (m_Player.IsSelfDefenceFrom(actor)) stringList.Add(string.Format("You can kill {0} in self-defence.", HimOrHer(actor)));
      if (m_Player.IsAggressorOf(actor)) stringList.Add(string.Format("You aggressed {0}.", HimOrHer(actor)));
      if (actor.IsSelfDefenceFrom(m_Player)) stringList.Add("Killing you would be self-defence.");
      if (m_Player.AreIndirectEnemies(actor)) stringList.Add("You are enemies through relationships.");
      stringList.Add("");
      string str = DescribeActorActivity(actor);
      stringList.Add(str ?? " ");
      if (actor.Model.Abilities.HasToSleep) {
        if (actor.IsExhausted) stringList.Add("Exhausted!");
        else if (actor.IsSleepy) stringList.Add("Sleepy.");
      }
      if (actor.Model.Abilities.HasToEat) {
        if (actor.IsStarving) stringList.Add("Starving!");
        else if (actor.IsHungry) stringList.Add("Hungry.");
      }
      else if (actor.Model.Abilities.IsRotting) {
        if (actor.IsRotStarving) stringList.Add("Starving!");
        else if (actor.IsRotHungry) stringList.Add("Hungry.");
      }
      if (actor.Model.Abilities.HasSanity) {
        if (actor.IsInsane) stringList.Add("Insane!");
        else if (actor.IsDisturbed) stringList.Add("Disturbed.");
      }
      stringList.Add(string.Format("Spd : {0:F2}", (double)actor.Speed / Rules.BASE_SPEED));
      StringBuilder stringBuilder = new StringBuilder();
      int num1 = actor.MaxHPs;
      stringBuilder.Append(actor.HitPoints != num1
                         ? string.Format("HP  : {0:D2}/{1:D2}", actor.HitPoints, num1)
                         : string.Format("HP  : {0:D2} MAX", actor.HitPoints));
      if (actor.Model.Abilities.CanTire) {
        int num2 = actor.MaxSTA;
        stringBuilder.Append(actor.StaminaPoints != num2
                           ? string.Format("   STA : {0}/{1}", actor.StaminaPoints, num2)
                           : string.Format("   STA : {0} MAX", actor.StaminaPoints));
      }
      stringList.Add(stringBuilder.ToString());
      Attack attack = actor.MeleeAttack();
      stringList.Add(string.Format("Atk : {0:D2} Dmg : {1:D2}", attack.HitValue, attack.DamageValue));
      Defence defence = Rules.ActorDefence(actor, actor.CurrentDefence);
      stringList.Add(string.Format("Def : {0:D2}", defence.Value));
      stringList.Add(string.Format("Arm : {0}/{1}", defence.Protection_Hit, defence.Protection_Shot));
      stringList.Add(" ");
      stringList.Add(actor.Model.FlavorDescription);
      stringList.Add(" ");
      if (actor.Sheet.SkillTable != null && actor.Sheet.SkillTable.CountSkills > 0) {
        foreach (Skill skill in actor.Sheet.SkillTable.Skills)
          stringList.Add(string.Format("{0}-{1}", skill.Level, Skills.Name(skill.ID)));
        stringList.Add(" ");
      }
      if (actor.Inventory != null && !actor.Inventory.IsEmpty) {
        stringList.Add(string.Format("Items {0}/{1} : ", actor.Inventory.CountItems, actor.MaxInv));
        stringList.AddRange(DescribeInventory(actor.Inventory));
      }
      return stringList.ToArray();
    }

    static private string DescribeActorActivity(Actor actor)
    {
      if (actor.IsPlayer) return null;
      switch (actor.Activity) {
        case Activity.IDLE:
          return null;
        case Activity.CHASING:
          if (actor.TargetActor == null)
            return "Chasing!";
          return string.Format("Chasing {0}!", actor.TargetActor.Name);
        case Activity.FIGHTING:
          if (actor.TargetActor == null)
            return "Fighting!";
          return string.Format("Fighting {0}!", actor.TargetActor.Name);
        case Activity.TRACKING:
          return "Tracking!";
        case Activity.FLEEING:
          return "Fleeing!";
        case Activity.FOLLOWING:
          if (actor.TargetActor == null)
            return "Following.";
          return string.Format("Following {0}.", actor.TargetActor.Name);
        case Activity.SLEEPING:
          return "Sleeping.";
        case Activity.FOLLOWING_ORDER:
          return "Following orders.";
        case Activity.FLEEING_FROM_EXPLOSIVE:
          return "Fleeing from explosives!";
        default:
          throw new ArgumentException("unhandled activity " + actor.Activity);
      }
    }

    static private string DescribePlayerFollowerStatus(Actor follower)
    {
      OrderableAI baseAi = follower.Controller as OrderableAI;
      return (baseAi.Order != null ? baseAi.Order.ToString() : "(no orders)") + string.Format("(trust:{0})", follower.TrustInLeader);
    }

    static private string[] DescribeMapObject(MapObject obj, Map map, Point mapPos)
    {
      List<string> stringList = new List<string>(4) { string.Format("{0}.", obj.AName) };
      if (obj.IsJumpable) stringList.Add("Can be jumped on.");
      if (obj.IsCouch) stringList.Add("Is a couch.");
      if (obj.GivesWood) stringList.Add("Can be dismantled for wood.");
      if (obj.IsMovable) stringList.Add("Can be moved.");
      if (obj.StandOnFovBonus) stringList.Add("Increases view range.");
      StringBuilder stringBuilder = new StringBuilder();
      if (obj.BreakState == MapObject.Break.BROKEN) stringBuilder.Append("Broken! ");
      if (obj.FireState == MapObject.Fire.ONFIRE) stringBuilder.Append("On fire! ");
      else if (obj.FireState == MapObject.Fire.ASHES) stringBuilder.Append("Burnt to ashes! ");
      stringList.Add(stringBuilder.ToString());
      if (obj is PowerGenerator) {
        stringList.Add((obj as PowerGenerator).IsOn ? "Currently ON." : "Currently OFF.");
        stringList.Add(string.Format("The power gauge reads {0}%.", (int)(100.0 * obj.Location.Map.PowerRatio)));
      } else if (obj is Board) {
        stringList.Add("The text reads : ");
        stringList.AddRange((obj as Board).Text);
      }
      if (obj.MaxHitPoints > 0) {
        stringList.Add(obj.HitPoints < obj.MaxHitPoints
                     ? string.Format("HP        : {0}/{1}", obj.HitPoints, obj.MaxHitPoints)
                     : string.Format("HP        : {0} MAX", obj.HitPoints));
        if (obj is DoorWindow doorWindow) {
          stringList.Add(doorWindow.BarricadePoints < Rules.BARRICADING_MAX
                       ? string.Format("Barricades: {0}/{1}", doorWindow.BarricadePoints, Rules.BARRICADING_MAX)
                       : string.Format("Barricades: {0} MAX", doorWindow.BarricadePoints));
        }
      }
      if (obj.Weight > 0) stringList.Add(string.Format("Weight    : {0}", obj.Weight));
      Inventory itemsAt = map.GetItemsAt(mapPos);
      if (itemsAt != null) stringList.AddRange(DescribeInventory(itemsAt));
      return stringList.ToArray();
    }

    static private string[] DescribeInventory(Inventory inv)
    {
      List<string> stringList = new List<string>(inv.CountItems);
      foreach (Item it in inv.Items) {
        if (it.IsEquipped)
          stringList.Add(string.Format("- {0} (equipped)", DescribeItemShort(it)));
        else
          stringList.Add(string.Format("- {0}", DescribeItemShort(it)));
      }
      return stringList.ToArray();
    }

    static private string[] DescribeCorpses(List<Corpse> corpses)
    {
      List<string> stringList = new List<string>(corpses.Count + 2);
      if (corpses.Count > 1)
        stringList.Add("There are corpses there...");
      else
        stringList.Add("There is a corpse here.");
      stringList.Add(" ");
      foreach (Corpse corpse in corpses)
        stringList.Add(string.Format("- Corpse of {0}.", corpse.DeadGuy.Name));
      return stringList.ToArray();
    }

	// UI functions ... do not belong in Corpse class for now
	static private string DescribeCorpseLong_DescInfectionPercent(int num)
	{
	  return num != 0 ? (num >= 5 ? (num >= 15 ? (num >= 30 ? (num >= 55 ? (num >= 70 ? (num >= 99 ? "7/7 - total" : "6/7 - great") : "5/7 - important") : "4/7 - average") : "3/7 - low") : "2/7 - minor") : "1/7 - traces") : "0/7 - none";
	}

	static private string DescribeCorpseLong_DescRiseProbability(int num)
	{
	  return num >= 5 ? (num >= 20 ? (num >= 40 ? (num >= 60 ? (num >= 80 ? (num >= 99 ? "6/6 - certain" : "5/6 - most likely") : "4/6 - very likely") : "3/6 - likely") : "2/6 - possible") : "1/6 - unlikely") : "0/6 - extremely unlikely";
	}

	static private string DescribeCorpseLong_DescRotLevel(int num)
	{
      switch (num)
      {
        case 0: return "The corpse looks fresh.";
        case 1: return "The corpse is bruised and smells.";
        case 2: return "The corpse is damaged.";
        case 3: return "The corpse is badly damaged.";
        case 4: return "The corpse is almost entirely rotten.";
        case 5: return "The corpse is about to crumble to dust.";
        default:
#if DEBUG
          throw new Exception("unhandled rot level");
#else
		  return "The corpse is infested with software bugs.";
#endif
      }
	}

	static private string DescribeCorpseLong_DescReviveChance(int num)
	{
			return num != 0 ? (num >= 5 ? (num >= 20 ? (num >= 40 ? (num >= 60 ? (num >= 80 ? (num >= 99 ? "6/6 - certain" : "5/6 - most likely") : "4/6 - very likely") : "3/6 - likely") : "2/6 - possible") : "1/6 - unlikely") : "0/6 - extremely unlikely") : "impossible";
	}

    private string[] DescribeCorpseLong(Corpse c, bool isInPlayerTile)
    {
      int skillLevel = m_Player.Sheet.SkillTable.GetSkillLevel(Skills.IDs.NECROLOGY);
      List<string> stringList = new List<string>(10){
        string.Format("Corpse of {0}.", c.DeadGuy.Name),
        " ",
        string.Format("Death     : {0}.", (skillLevel > 0 ? WorldTime.MakeTimeDurationMessage(Session.Get.WorldTime.TurnCounter - c.Turn) : "???")),
        string.Format("Infection : {0}.", (skillLevel >= Rules.SKILL_NECROLOGY_LEVEL_FOR_INFECTION ? DescribeCorpseLong_DescInfectionPercent(c.DeadGuy.InfectionPercent) : "???")),
        string.Format("Rise      : {0}.", (skillLevel >= Rules.SKILL_NECROLOGY_LEVEL_FOR_RISE ? DescribeCorpseLong_DescRiseProbability(2 * Rules.CorpseZombifyChance(c, c.DeadGuy.Location.Map.LocalTime, false)) : "???")),
        " ",
	    DescribeCorpseLong_DescRotLevel(c.RotLevel),
        string.Format("Revive    : {0}.", (m_Player.Sheet.SkillTable.GetSkillLevel(Skills.IDs.MEDIC) >= Rules.SKILL_MEDIC_LEVEL_FOR_REVIVE_EST ? DescribeCorpseLong_DescReviveChance(m_Rules.CorpseReviveChance(m_Player, c)) : "???"))
      };
      if (isInPlayerTile) {
        stringList.Add(" ");
        stringList.Add("----");
        stringList.Add("LBM to start/stop dragging.");
        stringList.Add(string.Format("RBM to {0}.", m_Player.Model.Abilities.IsUndead ? "eat" : "butcher"));
        if (!m_Player.Model.Abilities.IsUndead) {
          stringList.Add(string.Format("to eat: <{0}>", RogueGame.s_KeyBindings.Get(PlayerCommand.EAT_CORPSE).ToString()));
          stringList.Add(string.Format("to revive : <{0}>", RogueGame.s_KeyBindings.Get(PlayerCommand.REVIVE_CORPSE).ToString()));
        }
      }
      return stringList.ToArray();
    }

    static private string DescribeItemShort(Item it)
    {
      string str = it.Quantity > 1 ? it.Model.PluralName : it.AName;
      if (it is ItemFood) {
        ItemFood food = it as ItemFood;
        if (food.IsSpoiledAt(Session.Get.WorldTime.TurnCounter))
          str += " (spoiled)";
        else if (food.IsExpiredAt(Session.Get.WorldTime.TurnCounter))
          str += " (expired)";
      } else if (it is ItemRangedWeapon) {
        ItemRangedWeapon itemRangedWeapon = it as ItemRangedWeapon;
        str += string.Format(" ({0}/{1})", itemRangedWeapon.Ammo, itemRangedWeapon.Model.MaxAmmo);
      } else if (it is ItemTrap) {
        ItemTrap itemTrap = it as ItemTrap;
        if (itemTrap.IsActivated)
          str += "(activated)";
        if (itemTrap.IsTriggered)
          str += "(triggered)";
      }
      if (it.Quantity > 1) return string.Format("{0} {1}", it.Quantity, str);
      return str;
    }

    private string[] DescribeItemLong(Item it, bool isPlayerInventory)
    {
      List<string> stringList = new List<string>();
      if (it.Model.IsStackable)
        stringList.Add(string.Format("{0} {1}/{2}", DescribeItemShort(it), it.Quantity, it.Model.StackingLimit));
      else
        stringList.Add(DescribeItemShort(it));
      if (it.Model.IsUnbreakable)
        stringList.Add("Unbreakable.");
      string str = null;
      if (it is ItemWeapon)
      {
        stringList.AddRange(DescribeItemWeapon(it as ItemWeapon));
        if (it is ItemRangedWeapon)
          str = string.Format("to fire : <{0}>.", RogueGame.s_KeyBindings.Get(PlayerCommand.FIRE_MODE).ToString());
      }
      else if (it is ItemFood)
        stringList.AddRange(DescribeItemFood(it as ItemFood));
      else if (it is ItemMedicine)
        stringList.AddRange(DescribeItemMedicine(it as ItemMedicine));
      else if (it is ItemBarricadeMaterial)
      {
        stringList.AddRange(DescribeItemBarricadeMaterial(it as ItemBarricadeMaterial));
        str = string.Format("to use : <{0}>/<{1}>/<{2}>.", RogueGame.s_KeyBindings.Get(PlayerCommand.BARRICADE_MODE).ToString(), RogueGame.s_KeyBindings.Get(PlayerCommand.BUILD_SMALL_FORTIFICATION).ToString(), RogueGame.s_KeyBindings.Get(PlayerCommand.BUILD_LARGE_FORTIFICATION).ToString());
      }
      else if (it is ItemBodyArmor)
        stringList.AddRange(DescribeItemBodyArmor(it as ItemBodyArmor));
      else if (it is ItemSprayPaint)
      {
        stringList.AddRange(DescribeItemSprayPaint(it as ItemSprayPaint));
        str = string.Format("to spray : <{0}>.", RogueGame.s_KeyBindings.Get(PlayerCommand.USE_SPRAY).ToString());
      }
      else if (it is ItemSprayScent)
      {
        stringList.AddRange(DescribeItemSprayScent(it as ItemSprayScent));
        str = string.Format("to spray : <{0}>.", RogueGame.s_KeyBindings.Get(PlayerCommand.USE_SPRAY).ToString());
      }
      else if (it is ItemLight)
        stringList.AddRange(DescribeItemLight(it as ItemLight));
      else if (it is ItemTracker)
        stringList.AddRange(DescribeItemTracker(it as ItemTracker));
      else if (it is ItemAmmo)
      {
        stringList.AddRange(DescribeItemAmmo(it as ItemAmmo));
        str = "to reload : left-click.";
      }
      else if (it is ItemExplosive)
      {
        stringList.AddRange(DescribeItemExplosive(it as ItemExplosive));
        str = string.Format("to throw : <{0}>.", RogueGame.s_KeyBindings.Get(PlayerCommand.FIRE_MODE).ToString());
      }
      else if (it is ItemTrap)
        stringList.AddRange(DescribeItemTrap(it as ItemTrap));
      else if (it is ItemEntertainment)
        stringList.AddRange(DescribeItemEntertainment(it as ItemEntertainment));
      stringList.Add(" ");
      stringList.Add(it.Model.FlavorDescription);
      if (isPlayerInventory)
      {
        stringList.Add(" ");
        stringList.Add("----");
        stringList.Add(string.Format("to give : <{0}>.", RogueGame.s_KeyBindings.Get(PlayerCommand.GIVE_ITEM).ToString()));
        stringList.Add(string.Format("to trade : <{0}>.", RogueGame.s_KeyBindings.Get(PlayerCommand.INITIATE_TRADE).ToString()));
        if (str != null)
          stringList.Add(str);
      }
      return stringList.ToArray();
    }

    private string[] DescribeItemExplosive(ItemExplosive ex)
    {
      ItemExplosiveModel itemExplosiveModel = ex.Model;
      List<string> stringList = new List<string>{ "> explosive" };
      if (itemExplosiveModel.BlastAttack.CanDamageObjects) stringList.Add("Can damage objects.");
      if (itemExplosiveModel.BlastAttack.CanDestroyWalls) stringList.Add("Can destroy walls.");
      ItemPrimedExplosive primed = ex as ItemPrimedExplosive;
      if (null == primed)
        stringList.Add(string.Format("Fuse          : {0} turn(s) left!", primed.FuseTimeLeft));
      else
        stringList.Add(string.Format("Fuse          : {0} turn(s)", itemExplosiveModel.FuseDelay));
      stringList.Add(string.Format("Blast radius  : {0}", itemExplosiveModel.BlastAttack.Radius));
      StringBuilder stringBuilder = new StringBuilder();
      for (int distance = 0; distance <= itemExplosiveModel.BlastAttack.Radius; ++distance)
        stringBuilder.Append(string.Format("{0};", itemExplosiveModel.BlastAttack.DamageAt(distance)));
      stringList.Add(string.Format("Blast damages : {0}", stringBuilder.ToString()));
      if (ex is ItemGrenade grenade) {
        stringList.Add("> grenade");
        int max_throw_distance = grenade.Model.MaxThrowDistance;
        int num = m_Player.MaxThrowRange(max_throw_distance);
        if (num != max_throw_distance)
          stringList.Add(string.Format("Throwing rng  : {0} ({1})", num, max_throw_distance));
        else
          stringList.Add(string.Format("Throwing rng  : {0}", num));
      }
      if (null != primed) stringList.Add("PRIMED AND READY TO EXPLODE!");
      return stringList.ToArray();
    }

    static private string[] DescribeItemWeapon(ItemWeapon w)
    {
      ItemWeaponModel itemWeaponModel = w.Model;
      List<string> stringList = new List<string>{
        "> weapon",
        string.Format("Atk : +{0}", itemWeaponModel.Attack.HitValue),
        string.Format("Dmg : +{0}", itemWeaponModel.Attack.DamageValue),
        string.Format("Sta : -{0}", itemWeaponModel.Attack.StaminaPenalty)
      };
      if (w is ItemMeleeWeapon melee) {
        if (melee.IsFragile) stringList.Add("Breaks easily.");
      } else if (w is ItemRangedWeapon rw) {
        ItemRangedWeaponModel rangedWeaponModel = w.Model as ItemRangedWeaponModel;
        if (rangedWeaponModel.IsFireArm)
          stringList.Add("> firearm");
        else if (rangedWeaponModel.IsBow)
          stringList.Add("> bow");
        else
          stringList.Add("> ranged weapon");
        stringList.Add(string.Format("Rng  : {0}-{1}", rangedWeaponModel.Attack.Range, rangedWeaponModel.Attack.EfficientRange));
        if (rw.Ammo < rangedWeaponModel.MaxAmmo)
          stringList.Add(string.Format("Amo  : {0}/{1}", rw.Ammo, rangedWeaponModel.MaxAmmo));
        else
          stringList.Add(string.Format("Amo  : {0} MAX", rw.Ammo));
        stringList.Add(string.Format("Type : {0}", rangedWeaponModel.AmmoType.Describe(true)));
      }
      return stringList.ToArray();
    }

    static private string[] DescribeItemAmmo(ItemAmmo am)
    {
      return new List<string>{
        "> ammo",
        string.Format("Type : {0}", am.AmmoType.Describe(true))
      }.ToArray();
    }

    private string[] DescribeItemFood(ItemFood f)
    {
      List<string> stringList = new List<string>{ "> food" };
      if (f.IsPerishable) {
        if (f.IsStillFreshAt(Session.Get.WorldTime.TurnCounter)) stringList.Add("Fresh.");
        else if (f.IsExpiredAt(Session.Get.WorldTime.TurnCounter)) stringList.Add("*Expired*");
        else if (f.IsSpoiledAt(Session.Get.WorldTime.TurnCounter)) stringList.Add("**SPOILED**");
        stringList.Add(string.Format("Best-Before : {0}", f.BestBefore.ToString()));
      } else
        stringList.Add("Always fresh.");
      int baseValue = f.NutritionAt(Session.Get.WorldTime.TurnCounter);
      int num = m_Player == null ? baseValue : m_Player.ItemNutritionValue(baseValue);
      if (num == f.Model.Nutrition)
        stringList.Add(string.Format("Nutrition   : +{0}", baseValue));
      else
        stringList.Add(string.Format("Nutrition   : +{0} (+{1})", num, baseValue));
      return stringList.ToArray();
    }

    private string[] DescribeItemMedicine(ItemMedicine med)
    {
      List<string> stringList = new List<string>{ "> medicine" };
      ItemMedicineModel itemMedicineModel = med.Model;
      int num1 = m_Player == null ? itemMedicineModel.Healing : Rules.ActorMedicineEffect(m_Player, itemMedicineModel.Healing);
      if (num1 == itemMedicineModel.Healing)
        stringList.Add(string.Format("Healing : +{0}", itemMedicineModel.Healing));
      else
        stringList.Add(string.Format("Healing : +{0} (+{1})", num1, itemMedicineModel.Healing));
      int num2 = m_Player == null ? itemMedicineModel.StaminaBoost : Rules.ActorMedicineEffect(m_Player, itemMedicineModel.StaminaBoost);
      if (num2 == itemMedicineModel.StaminaBoost)
        stringList.Add(string.Format("Stamina : +{0}", itemMedicineModel.StaminaBoost));
      else
        stringList.Add(string.Format("Stamina : +{0} (+{1})", num2, itemMedicineModel.StaminaBoost));
      int num3 = m_Player == null ? itemMedicineModel.SleepBoost : Rules.ActorMedicineEffect(m_Player, itemMedicineModel.SleepBoost);
      if (num3 == itemMedicineModel.SleepBoost)
        stringList.Add(string.Format("Sleep   : +{0}", itemMedicineModel.SleepBoost));
      else
        stringList.Add(string.Format("Sleep   : +{0} (+{1})", num3, itemMedicineModel.SleepBoost));
      int num4 = m_Player == null ? itemMedicineModel.SanityCure : Rules.ActorMedicineEffect(m_Player, itemMedicineModel.SanityCure);
      if (num4 == itemMedicineModel.SanityCure)
        stringList.Add(string.Format("Sanity  : +{0}", itemMedicineModel.SanityCure));
      else
        stringList.Add(string.Format("Sanity  : +{0} (+{1})", num4, itemMedicineModel.SanityCure));
      if (Session.Get.HasInfection) {
        int num5 = m_Player == null ? itemMedicineModel.InfectionCure : Rules.ActorMedicineEffect(m_Player, itemMedicineModel.InfectionCure);
        if (num5 == itemMedicineModel.InfectionCure)
          stringList.Add(string.Format("Cure    : +{0}", itemMedicineModel.InfectionCure));
        else
          stringList.Add(string.Format("Cure    : +{0} (+{1})", num5, itemMedicineModel.InfectionCure));
      }
      return stringList.ToArray();
    }

    private string[] DescribeItemBarricadeMaterial(ItemBarricadeMaterial bm)
    {
      List<string> stringList = new List<string>{ "> barricade material" };
      int barricade_value = bm.Model.BarricadingValue;
      int num = m_Player == null ? barricade_value : Rules.ActorBarricadingPoints(m_Player, barricade_value);
      if (num == barricade_value)
        stringList.Add(string.Format("Barricading : +{0}", barricade_value));
      else
        stringList.Add(string.Format("Barricading : +{0} (+{1})", num, barricade_value));
      return stringList.ToArray();
    }

    static private string[] DescribeItemBodyArmor(ItemBodyArmor b)
    {
      List<string> stringList1 = new List<string>{
        "> body armor",
        string.Format("Protection vs Hits  : +{0}", b.Protection_Hit),
        string.Format("Protection vs Shots : +{0}", b.Protection_Shot),
        string.Format("Encumbrance         : -{0} DEF", b.Encumbrance),
        string.Format("Weight              : -{0:F2} SPD", b.Weight/100.0f)
      };
      List<string> stringList2 = new List<string>();
      List<string> stringList3 = new List<string>();
      if (b.IsFriendlyForCops()) stringList2.Add("Cops");
      if (b.IsHostileForCops()) stringList3.Add("Cops");
      foreach (GameGangs.IDs gangID in GameGangs.BIKERS) {
        if (b.IsHostileForBiker(gangID)) stringList3.Add(GameGangs.NAMES[(int) gangID]);
        if (b.IsFriendlyForBiker(gangID)) stringList2.Add(GameGangs.NAMES[(int) gangID]);
      }
      foreach (GameGangs.IDs gangID in GameGangs.GANGSTAS) {
        if (b.IsHostileForBiker(gangID)) stringList3.Add(GameGangs.NAMES[(int) gangID]);
        if (b.IsFriendlyForBiker(gangID)) stringList2.Add(GameGangs.NAMES[(int) gangID]);
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

    static private string[] DescribeItemSprayPaint(ItemSprayPaint sp)
    {
      List<string> stringList = new List<string>{ "> spray paint" };
      int max_paint = sp.Model.MaxPaintQuantity;
      if (sp.PaintQuantity < max_paint)
        stringList.Add(string.Format("Paint : {0}/{1}", sp.PaintQuantity, max_paint));
      else
        stringList.Add(string.Format("Paint : {0} MAX", sp.PaintQuantity));
      return stringList.ToArray();
    }

    static private string[] DescribeItemSprayScent(ItemSprayScent sp)
    {
      List<string> stringList = new List<string>{ "> spray scent" };
      int max_spray = sp.Model.MaxSprayQuantity;
      if (sp.SprayQuantity < max_spray)
        stringList.Add(string.Format("Spray : {0}/{1}", sp.SprayQuantity, max_spray));
      else
        stringList.Add(string.Format("Spray : {0} MAX", sp.SprayQuantity));
      return stringList.ToArray();
    }

    static private string[] DescribeItemLight(ItemLight lt)
    {
      List<string> stringList = new List<string>{ "> light" };
      stringList.Add(DescribeBatteries(lt));
      stringList.Add(string.Format("FOV       : +{0}", lt.FovBonus));
      return stringList.ToArray();
    }

    static private string[] DescribeItemTracker(ItemTracker tr)
    {
      List<string> stringList = new List<string>{ "> tracker" };
      stringList.Add(DescribeBatteries(tr));
      return stringList.ToArray();
    }

    static private string[] DescribeItemTrap(ItemTrap tr)
    {
      List<string> stringList = new List<string>();
      ItemTrapModel itemTrapModel = tr.Model as ItemTrapModel;
      stringList.Add("> trap");
      if (tr.IsActivated)
        stringList.Add("** Activated! **");
      if (itemTrapModel.IsOneTimeUse)
        stringList.Add("Desactives when triggered.");
      if (itemTrapModel.IsNoisy)
        stringList.Add(string.Format("Makes {0} noise.", itemTrapModel.NoiseName));
      if (itemTrapModel.UseToActivate)
        stringList.Add("Use to activate.");
      stringList.Add(string.Format("Damage  : {0}", itemTrapModel.Damage));
      stringList.Add(string.Format("Trigger : {0}%", itemTrapModel.TriggerChance));
      stringList.Add(string.Format("Break   : {0}%", itemTrapModel.BreakChance));
      if (itemTrapModel.BlockChance > 0)
        stringList.Add(string.Format("Block   : {0}%", itemTrapModel.BlockChance));
      if (itemTrapModel.BreakChanceWhenEscape > 0)
        stringList.Add(string.Format("{0}% to break on escape", itemTrapModel.BreakChanceWhenEscape));
      return stringList.ToArray();
    }

    private string[] DescribeItemEntertainment(ItemEntertainment ent)
    {
      List<string> stringList = new List<string>();
      ItemEntertainmentModel entertainmentModel = ent.Model;
      stringList.Add("> entertainment");
      if (m_Player != null && m_Player.IsBoredOf(ent))
        stringList.Add("* BORED OF IT! *");
      int ent_value = entertainmentModel.Value;
      int num = m_Player == null ? ent_value : Rules.ActorSanRegenValue(m_Player, ent_value);
      if (num != ent_value)
        stringList.Add(string.Format("Sanity : +{0} (+{1})", num, ent_value));
      else
        stringList.Add(string.Format("Sanity : +{0}", ent_value));
      stringList.Add(string.Format("Boring : {0}%", entertainmentModel.BoreChance));
      return stringList.ToArray();
    }

    static private string DescribeBatteries(BatteryPowered it)
    {
      int hours = BatteriesToHours(it.Batteries);
      if (it.Batteries < it.MaxBatteries)
        return string.Format("Batteries : {0}/{1} ({2}h)", it.Batteries, it.MaxBatteries, hours);
      return string.Format("Batteries : {0} MAX ({1}h)", it.Batteries, hours);
    }

    static private string DescribeSkillShort(Skills.IDs id)
    {
      switch (id) {
        case Skills.IDs._FIRST:
          return string.Format("+{0} melee ATK, +{1} DEF", Actor.SKILL_AGILE_ATK_BONUS, Rules.SKILL_AGILE_DEF_BONUS);
        case Skills.IDs.AWAKE:
          return string.Format("+{0}% max SLP, +{1}% SLP sleeping regen ", (int)(100.0 * (double)Actor.SKILL_AWAKE_SLEEP_BONUS), (int)(100.0 * (double)Rules.SKILL_AWAKE_SLEEP_REGEN_BONUS));
        case Skills.IDs.BOWS:
          return string.Format("bows +{0} Atk, +{1} Dmg", Actor.SKILL_BOWS_ATK_BONUS, Actor.SKILL_BOWS_DMG_BONUS);
        case Skills.IDs.CARPENTRY:
          return string.Format("build, -{0} mat. at lvl 3, +{1}% barricading", Rules.SKILL_CARPENTRY_LEVEL3_BUILD_BONUS, (int)(100.0 * (double)Rules.SKILL_CARPENTRY_BARRICADING_BONUS));
        case Skills.IDs.CHARISMATIC:
          return string.Format("+{0} trust per turn, +{1}% trade offers", Rules.SKILL_CHARISMATIC_TRUST_BONUS, Rules.SKILL_CHARISMATIC_TRADE_BONUS);
        case Skills.IDs.FIREARMS:
          return string.Format("firearms +{0} Atk, +{1} Dmg", Actor.SKILL_FIREARMS_ATK_BONUS, Actor.SKILL_FIREARMS_DMG_BONUS);
        case Skills.IDs.HARDY:
          return string.Format("sleep heals anywhere, +{0}% chance to heal", Rules.SKILL_HARDY_HEAL_CHANCE_BONUS);
        case Skills.IDs.HAULER:
          return string.Format("+{0} inventory capacity", Actor.SKILL_HAULER_INV_BONUS);
        case Skills.IDs.HIGH_STAMINA:
          return string.Format("+{0} STA", Actor.SKILL_HIGH_STAMINA_STA_BONUS);
        case Skills.IDs.LEADERSHIP:
          return string.Format("+{0} max Followers", Actor.SKILL_LEADERSHIP_FOLLOWER_BONUS);
        case Skills.IDs.LIGHT_EATER:
          return string.Format("+{0}% max FOO, +{1}% item food points", (int)(100.0 * (double)Actor.SKILL_LIGHT_EATER_MAXFOOD_BONUS), (int)(100.0 * (double)Actor.SKILL_LIGHT_EATER_FOOD_BONUS));
        case Skills.IDs.LIGHT_FEET:
          return string.Format("+{0}% to avoid and escape traps", Rules.SKILL_LIGHT_FEET_TRAP_BONUS);
        case Skills.IDs.LIGHT_SLEEPER:
          return string.Format("+{0}% noise wake up chance", Rules.SKILL_LIGHT_SLEEPER_WAKEUP_CHANCE_BONUS);
        case Skills.IDs.MARTIAL_ARTS:
          return string.Format("unarmed only melee +{0} Atk, +{1} Dmg", Actor.SKILL_MARTIAL_ARTS_ATK_BONUS, Actor.SKILL_MARTIAL_ARTS_DMG_BONUS);
        case Skills.IDs.MEDIC:
          return string.Format("+{0}% medicine effects, +{1}% revive ", (int)(100.0 * (double)Rules.SKILL_MEDIC_BONUS), Rules.SKILL_MEDIC_REVIVE_BONUS);
        case Skills.IDs.NECROLOGY:
          return string.Format("+{0}/+{1} Dmg vs undeads/corpses, data on corpses", Actor.SKILL_NECROLOGY_UNDEAD_BONUS, Rules.SKILL_NECROLOGY_CORPSE_BONUS);
        case Skills.IDs.STRONG:
          return string.Format("+{0} melee DMG, +{1} throw range", Actor.SKILL_STRONG_DMG_BONUS, Actor.SKILL_STRONG_THROW_BONUS);
        case Skills.IDs.STRONG_PSYCHE:
          return string.Format("+{0}% SAN threshold, +{1}% regen", (int)(100.0 * (double)Rules.SKILL_STRONG_PSYCHE_LEVEL_BONUS), (int)(100.0 * (double)Rules.SKILL_STRONG_PSYCHE_ENT_BONUS));
        case Skills.IDs.TOUGH:
          return string.Format("+{0} HP", Actor.SKILL_TOUGH_HP_BONUS);
        case Skills.IDs.UNSUSPICIOUS:
          return string.Format("+{0}% unnoticed by law enforcers and gangs", Rules.SKILL_UNSUSPICIOUS_BONUS);
        case Skills.IDs._FIRST_UNDEAD:
          return string.Format("+{0} melee ATK, +{1} DEF, can jump", Actor.SKILL_ZAGILE_ATK_BONUS, Rules.SKILL_ZAGILE_DEF_BONUS);
        case Skills.IDs.Z_EATER:
          return string.Format("+{0}% hp regen", (int)(100.0 * (double)Rules.SKILL_ZEATER_REGEN_BONUS));
        case Skills.IDs.Z_GRAB:
          return string.Format("can grab enemies, +{0}% per level", Rules.SKILL_ZGRAB_CHANCE);
        case Skills.IDs.Z_INFECTOR:
          return string.Format("+{0}% infection damage", (int)(100.0 * (double)Rules.SKILL_ZINFECTOR_BONUS));
        case Skills.IDs.Z_LIGHT_EATER:
          return string.Format("+{0}% max ROT, +{1}% from eating", (int)(100.0 * (double)Actor.SKILL_ZLIGHT_EATER_MAXFOOD_BONUS), (int)(100.0 * (double)Actor.SKILL_ZLIGHT_EATER_FOOD_BONUS));
        case Skills.IDs.Z_LIGHT_FEET:
          return string.Format("+{0}% to avoid traps", Rules.SKILL_ZLIGHT_FEET_TRAP_BONUS);
        case Skills.IDs.Z_STRONG:
          return string.Format("+{0} melee DMG, can push", Actor.SKILL_ZSTRONG_DMG_BONUS);
        case Skills.IDs.Z_TOUGH:
          return string.Format("+{0} HP", Actor.SKILL_ZTOUGH_HP_BONUS);
        case Skills.IDs.Z_TRACKER:
          return string.Format("+{0}% smell", (int)(100.0 * (double)Actor.SKILL_ZTRACKER_SMELL_BONUS));
        default:
          throw new ArgumentOutOfRangeException("unhandled skill id");
      }
    }

    static private string DescribeDayPhase(DayPhase phase)
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

    static private string DescribeWeather(Weather weather)
    {
      switch (weather)
      {
        case Weather.CLEAR:
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

    static private Color WeatherColor(Weather weather)
    {
      switch (weather)
      {
        case Weather.CLEAR:
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

    static private int BatteriesToHours(int batteries)
    {
      return batteries / WorldTime.TURNS_PER_HOUR;
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
          return null;
      }
    }

    public void DoMoveActor(Actor actor, Location newLocation)
    {
      if (!TryActorLeaveTile(actor)) {
        actor.SpendActionPoints(Rules.BASE_ACTION_COST);
        return;
      }
      Location location = actor.Location;
      if (location.Map != newLocation.Map) throw new NotImplementedException("DoMoveActor : illegal to change map.");
      newLocation.Place(actor);
      Corpse draggedCorpse = actor.DraggedCorpse;
      if (draggedCorpse != null) {
        location.Map.MoveTo(draggedCorpse, newLocation.Position);
        if (ForceVisibleToPlayer(newLocation) || ForceVisibleToPlayer(location))
          AddMessage(MakeMessage(actor, string.Format("{0} {1} corpse.", Conjugate(actor, VERB_DRAG), draggedCorpse.DeadGuy.TheName)));
      }
      int actionCost = Rules.BASE_ACTION_COST;
      if (actor.IsRunning) {
        actionCost /= 2;
        actor.SpendStaminaPoints(Rules.STAMINA_COST_RUNNING);
      }
      bool flag = false;
      MapObject mapObjectAt = newLocation.Map.GetMapObjectAt(newLocation.Position);
      if (mapObjectAt != null && !mapObjectAt.IsWalkable && mapObjectAt.IsJumpable)
        flag = true;
      if (flag) {
        actor.SpendStaminaPoints(Rules.STAMINA_COST_JUMP);
        if (ForceVisibleToPlayer(actor))
          AddMessage(MakeMessage(actor, Conjugate(actor, VERB_JUMP_ON), mapObjectAt));
        if (actor.Model.Abilities.CanJumpStumble && m_Rules.RollChance(Rules.JUMP_STUMBLE_CHANCE)) {
          actionCost += Rules.JUMP_STUMBLE_ACTION_COST;
          if (IsVisibleToPlayer(actor))
            AddMessage(MakeMessage(actor, string.Format("{0}!", Conjugate(actor, VERB_STUMBLE))));
        }
      }
      if (draggedCorpse != null)
        actor.SpendStaminaPoints(Rules.STAMINA_COST_MOVE_DRAGGED_CORPSE);
      actor.SpendActionPoints(actionCost);

	  // committed to move now
#if ENABLE_THREAT_TRACKING
	  actor.Moved();
#endif
      if (actor.GetEquippedItem(DollPart.HIP_HOLSTER) is ItemTracker tracker) tracker.Batteries += 2;  // police radio recharge

      if (actor.ActionPoints >= Rules.BASE_ACTION_COST) actor.DropScent();
      if (!actor.IsPlayer && (actor.Activity == Activity.FLEEING || actor.Activity == Activity.FLEEING_FROM_EXPLOSIVE) && (!actor.Model.Abilities.IsUndead && actor.Model.Abilities.CanTalk))
      {
        OnLoudNoise(newLocation.Map, newLocation.Position, "A loud SCREAM");
        if (!ForceVisibleToPlayer(actor) && m_Rules.RollChance(PLAYER_HEAR_SCREAMS_CHANCE))
          AddMessageIfAudibleForPlayer(actor.Location, "You hear screams of terror");
      }
      OnActorEnterTile(actor);
    }

    public void DoMoveActor(Actor actor, Direction direction)
    {
      DoMoveActor(actor, actor.Location + direction);
    }

    public void OnActorEnterTile(Actor actor)
    {
      Map map = actor.Location.Map;
      Point position = actor.Location.Position;
#if ENABLE_THREAT_TRACKING
	  Session.Get.PoliceTrackingThroughExitSpawn(actor);
#endif
      if (map.IsTrapCoveringMapObjectAt(position)) return;
      List<ItemTrap> trapsAt = actor.Location.Items?.GetItemsByType<ItemTrap>();
      if (null == trapsAt) return;
      List<ItemTrap> trapList = null;
      foreach (ItemTrap trap in trapsAt) {
        if (trap.IsActivated && TryTriggerTrap(trap, actor)) {
          (trapList ?? (trapList = new List<ItemTrap>())).Add(trap);
        }
      }
      if (null != trapList) {
        foreach (ItemTrap it in trapList)
          map.RemoveItemAt(it, position);
      }
      if (0 >= actor.HitPoints) KillActor(null, actor, "trap");
    }

    private bool TryActorLeaveTile(Actor actor)
    {
      Map map = actor.Location.Map;
      Point position = actor.Location.Position;
      bool canLeave = true;
      if (!map.IsTrapCoveringMapObjectAt(position)) {
        Inventory itemsAt = map.GetItemsAt(position);
        if (itemsAt != null) {
          List<Item> objList = null;
          bool flag = false;
          foreach (Item obj in itemsAt.Items) {
            if (obj is ItemTrap trap && trap.IsTriggered) {
              flag = true;
              if (!TryEscapeTrap(trap, actor, out bool isDestroyed)) canLeave = false;
              else if (isDestroyed) {
                if (objList == null) objList = new List<Item>(itemsAt.CountItems);
                objList.Add(obj);
              }
            }
          }
          if (objList != null) {
            foreach (Item it in objList)
              map.RemoveItemAt(it, position);
          }
          if (canLeave && flag)
            UntriggerAllTrapsHere(actor.Location);
        }
      }
      bool visible = ForceVisibleToPlayer(actor);
      map.ForEachAdjacent(position, adj => {
        Actor actorAt = map.GetActorAt(adj);
        if (actorAt == null || !actorAt.Model.Abilities.IsUndead || !actorAt.IsEnemyOf(actor) || !m_Rules.RollChance(Rules.ZGrabChance(actorAt, actor))) return;
        if (visible) AddMessage(MakeMessage(actorAt, Conjugate(actorAt, VERB_GRAB), actor));
        canLeave = false;
      });
      return canLeave;
    }

    private bool TryTriggerTrap(ItemTrap trap, Actor victim)
    {
      if (m_Rules.CheckTrapTriggers(trap, victim))
        DoTriggerTrap(trap, victim.Location.Map, victim.Location.Position, victim, null);
      else if (IsVisibleToPlayer(victim))
        AddMessage(MakeMessage(victim, string.Format("safely {0} {1}.", Conjugate(victim, VERB_AVOID), trap.TheName)));
      return trap.Quantity == 0;
    }

    private bool TryEscapeTrap(ItemTrap trap, Actor victim, out bool isDestroyed)
    {
      isDestroyed = false;
      if (trap.Model.BlockChance <= 0) return true;
      bool player = ForceVisibleToPlayer(victim);
      bool flag = false;
      if (m_Rules.CheckTrapEscape(trap, victim)) {
        trap.IsTriggered = false;
        flag = true;
        if (player)
          AddMessage(MakeMessage(victim, string.Format("{0} {1}.", Conjugate(victim, VERB_ESCAPE), trap.TheName)));
        if (m_Rules.CheckTrapEscapeBreaks(trap, victim)) {
          if (player)
            AddMessage(MakeMessage(victim, string.Format("{0} {1}.", Conjugate(victim, VERB_BREAK), trap.TheName)));
          --trap.Quantity;
          isDestroyed = trap.Quantity <= 0;
        }
      }
      else if (player)
        AddMessage(MakeMessage(victim, string.Format("is trapped by {0}!", trap.TheName)));
      return flag;
    }

    static private void UntriggerAllTrapsHere(Location loc)
    {
      Inventory itemsAt = loc.Map.GetItemsAt(loc.Position);
      if (itemsAt == null) return;
      foreach (Item obj in itemsAt.Items) {
        if (obj is ItemTrap trap && trap.IsTriggered) trap.IsTriggered = false;
      }
    }

    private void CheckMapObjectTriggersTraps(Map map, Point pos)
    {
      MapObject mapObjectAt = map.GetTrapTriggeringMapObjectAt(pos);
      if (null == mapObjectAt) return;
      Inventory itemsAt = map.GetItemsAt(pos);
      if (itemsAt == null) return;
      List<Item> objList = null;
      foreach (Item obj in itemsAt.Items) {
        if (obj is ItemTrap trap && trap.IsActivated) {
          DoTriggerTrap(trap, map, pos, null, mapObjectAt);
          if (trap.Quantity <= 0) {
            (objList ?? (objList = new List<Item>(itemsAt.CountItems))).Add(obj);
          }
        }
      }
      if (objList == null) return;
      foreach (Item it in objList)
        map.RemoveItemAt(it, pos);
    }

    private void DoTriggerTrap(ItemTrap trap, Map map, Point pos, Actor victim, MapObject mobj)
    {
      ItemTrapModel trapModel = trap.Model;
      bool player = ForceVisibleToPlayer(map, pos);
      trap.IsTriggered = true;
      int dmg = trapModel.Damage * trap.Quantity;
      if (dmg > 0 && victim != null) {
        InflictDamage(victim, dmg);
        if (player) {
          AddMessage(MakeMessage(victim, string.Format("is hurt by {0} for {1} damage!", trap.AName, dmg)));
          AddOverlay(new OverlayImage(MapToScreen(victim.Location), GameImages.ICON_MELEE_DAMAGE));
          AddOverlay(new RogueGame.OverlayText(MapToScreen(victim.Location).Add(10, 10), Color.White, dmg.ToString(), Color.Black));
          RedrawPlayScreen();
          AnimDelay(victim.IsPlayer ? DELAY_NORMAL : DELAY_SHORT);
          ClearOverlays();
          RedrawPlayScreen();
        }
      }
      if (trapModel.IsNoisy) {
        if (player) {
          if (victim != null)
            AddMessage(MakeMessage(victim, string.Format("stepping on {0} makes a bunch of noise!", trap.AName)));
          else if (mobj != null)
            AddMessage(new Data.Message(string.Format("{0} makes a lot of noise!", Capitalize(trap.TheName)), map.LocalTime.TurnCounter));
        }
        OnLoudNoise(map, pos, trapModel.NoiseName);
      }
      if (trapModel.IsOneTimeUse) trap.IsActivated = false;
      if (!m_Rules.CheckTrapStepOnBreaks(trap, mobj)) return;
      if (player) {
        if (victim != null)
          AddMessage(MakeMessage(victim, string.Format("{0} {1}.", Conjugate(victim, VERB_CRUSH), trap.TheName)));
        else if (mobj != null)
          AddMessage(new Data.Message(string.Format("{0} breaks the {1}.", Capitalize(mobj.TheName), trap.TheName), map.LocalTime.TurnCounter));
      }
      --trap.Quantity;
    }

    public bool DoLeaveMap(Actor actor, Point exitPoint, bool askForConfirmation)
    {
      bool isPlayer = actor.IsPlayer;
      Exit exitAt = actor.Location.Map.GetExitAt(exitPoint);
      if (exitAt == null) {
        if (isPlayer) AddMessage(MakeErrorMessage("There is nowhere to go there."));
        return true;
      }
      Map map = actor.Location.Map;
      Point position = actor.Location.Position;
      if (isPlayer && askForConfirmation) {
        ClearMessages();
        AddMessage(MakeYesNoMessage(string.Format("REALLY LEAVE {0}", map.Name)));
        RedrawPlayScreen();
        if (!WaitYesOrNo()) {
          AddMessage(new Data.Message("Let's stay here a bit longer...", Session.Get.WorldTime.TurnCounter, Color.Yellow));
          RedrawPlayScreen();
          return false;
        }
      }
      if (!TryActorLeaveTile(actor)) {
        actor.SpendActionPoints(Rules.BASE_ACTION_COST);
        return false;
      }
#if SPEEDY_GONZALES
      if (!actor.IsPlayer) actor.SpendActionPoints(Rules.BASE_ACTION_COST);
#else
      if (null == exitAt.ToMap.NextActorToAct || actor.Location.Map.District!=exitAt.ToMap.District) actor.SpendActionPoints(Rules.BASE_ACTION_COST);
#endif
      if (isPlayer && exitAt.ToMap.District != map.District)
        BeforePlayerEnterDistrict(exitAt.ToMap.District);
      string reason = exitAt.ReasonIsBlocked(actor);
      if (!string.IsNullOrEmpty(reason)) {
        if (isPlayer) AddMessage(MakeErrorMessage(reason));
        return true;
      }
      if (ForceVisibleToPlayer(actor))
        AddMessage(MakeMessage(actor, string.Format("{0} {1}.", Conjugate(actor, VERB_LEAVE), map.Name)));
      map.Remove(actor);
      if (actor.DraggedCorpse != null) map.Remove(actor.DraggedCorpse);
      if (isPlayer && exitAt.ToMap.District != map.District) OnPlayerLeaveDistrict();
      exitAt.Location.Place(actor);
      exitAt.ToMap.MoveActorToFirstPosition(actor); // XXX change target for NO_PEACE_WALLS; when entering a district that executes before ours we should be *last* -- if we can see what we're getting into
      if (actor.DraggedCorpse != null) exitAt.Location.Add(actor.DraggedCorpse);
      if (ForceVisibleToPlayer(actor) || isPlayer)
      AddMessage(MakeMessage(actor, string.Format("{0} {1}.", Conjugate(actor, VERB_ENTER), exitAt.ToMap.Name)));
      if (isPlayer) {
        if (map.District != exitAt.ToMap.District) {
          Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format("Entered district {0}.", exitAt.ToMap.District.Name));
          actor.ActionPoints += actor.Speed;
        }
        SetCurrentMap(exitAt.ToMap);
      }
      OnActorEnterTile(actor);
      if (actor.CountFollowers > 0) DoFollowersEnterMap(actor, exitAt.Location);
      return true;
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void DoFollowersEnterMap(Actor leader, Location to)
    {
      Map fromMap = leader.Location.Map;
      Point fromPos = leader.Location.Position;
      List<Actor> actorList = null;
      foreach(Actor fo in leader.Followers) {
        bool flag3 = false;
        List<Point> pointList = null;
        if (Rules.IsAdjacent(leader.Location, fo.Location)) {
          pointList = to.Map.FilterAdjacentInMap(to.Position, pt => to.Map.IsWalkableFor(pt, fo));
          flag3 = pointList != null && pointList.Count != 0;
        }
        if (!flag3) {
          (actorList ?? (actorList = new List<Actor>())).Add(fo);
        } else if (TryActorLeaveTile(fo)) {
          Point position = pointList[m_Rules.Roll(0, pointList.Count)];
          to.Map.PlaceAt(fo, position);
          to.Map.MoveActorToFirstPosition(fo);
          OnActorEnterTile(fo);
#if SPEEDY_GONZALES
          if (fromMap.District != to.Map.District) fo.ActionPoints += fo.Speed; // Yes, run *four* squares on the first turn in a new district!
#endif
        }
      }
      if (actorList == null) return;

      bool flag2 = m_Player == leader;
      if (to.Map.District != fromMap.District) {
        foreach (Actor other in actorList) {
          if (!other.IsPlayer) leader.RemoveFollower(other);
          if (flag2) {
            Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format("{0} was left behind.", other.TheName));
            ClearMessages();
            if (other.IsPlayer) {
              AddMessage(new Data.Message(string.Format("{0} could not follow and is still in {1}.", other.TheName, other.Location.Map.Name), Session.Get.WorldTime.TurnCounter, Color.Yellow));
            } else {
              AddMessage(new Data.Message(string.Format("{0} could not follow you out of the district and left you!", other.TheName), Session.Get.WorldTime.TurnCounter, Color.Red));
            }
            AddMessagePressEnter();
            ClearMessages();
          }
        }
      } else if (flag2) {
        foreach (Actor other in actorList) {
          if (other.Location.Map == fromMap) {
            ClearMessages();
            AddMessage(new Data.Message(string.Format("{0} could not follow and is still in {1}.", other.TheName, fromMap.Name), Session.Get.WorldTime.TurnCounter, Color.Yellow));
            AddMessagePressEnter();
            ClearMessages();
          }
        }
      }
    }

    public bool DoUseExit(Actor actor, Point exitPoint)
    {
      return DoLeaveMap(actor, exitPoint, false);
    }

    public void DoSwitchPlace(Actor actor, Actor other)
    {
      actor.SpendActionPoints(2*Rules.BASE_ACTION_COST);
      Location a_loc = actor.Location;
      Location o_loc = other.Location;
      o_loc.Map.Remove(other);
      o_loc.Place(actor);
      a_loc.Place(other);
      if (!IsVisibleToPlayer(actor) && !IsVisibleToPlayer(other)) return;
      AddMessage(MakeMessage(actor, Conjugate(actor, VERB_SWITCH_PLACE_WITH), other));
    }

    public void DoTakeLead(Actor actor, Actor other)
    {
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      actor.AddFollower(other);
      int trustIn = other.GetTrustIn(actor);
      other.TrustInLeader = trustIn;
      if (!ForceVisibleToPlayer(actor) && !ForceVisibleToPlayer(other)) return;
      if (actor == m_Player) ClearMessages();
      AddMessage(MakeMessage(actor, Conjugate(actor, VERB_PERSUADE), other, " to join."));
      if (trustIn == 0) return;
      DoSay(other, actor, "Ah yes I remember you.", RogueGame.Sayflags.IS_FREE_ACTION);
    }

    public void DoCancelLead(Actor actor, Actor follower)
    {
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      actor.RemoveFollower(follower);
      follower.SetTrustIn(actor, follower.TrustInLeader);
      follower.TrustInLeader = 0;
      if (!ForceVisibleToPlayer(actor) && !ForceVisibleToPlayer(follower)) return;
      if (actor == m_Player) ClearMessages();
      AddMessage(MakeMessage(actor, Conjugate(actor, VERB_PERSUADE), follower, " to leave."));
    }

    // It breaks immersion to have to run, rather than wait, to adjust the energy level above 50
    public void DoWait(Actor actor)
    {
      actor.Wait();
      if (ForceVisibleToPlayer(actor)) {
        if (actor.StaminaPoints < actor.MaxSTA)
          AddMessage(MakeMessage(actor, string.Format("{0} {1} breath.", Conjugate(actor, VERB_CATCH), HisOrHer(actor))));
        else
          AddMessage(MakeMessage(actor, string.Format("{0}.", Conjugate(actor, VERB_WAIT))));
      }
      actor.RegenStaminaPoints(Actor.STAMINA_REGEN_WAIT);
    }

    public bool DoPlayerBump(Actor player, Direction direction)
    {
      ActionBump actionBump = new ActionBump(player, direction);
      if (actionBump.IsLegal()) {
        actionBump.Perform();
        return true;
      }
      if (player.Location.Map.GetMapObjectAt(player.Location.Position + direction) is DoorWindow doorWindow && doorWindow.IsBarricaded && !player.Model.Abilities.IsUndead) {
        if (!player.IsTired) {
          AddMessage(MakeYesNoMessage("Really tear down the barricade"));
          RedrawPlayScreen();
          if (WaitYesOrNo()) {
            DoBreak(player, doorWindow);
            return true;
          }
          AddMessage(new Data.Message("Good, keep everything secure.", Session.Get.WorldTime.TurnCounter, Color.Yellow));
          return false;
        }
        AddMessage(MakeErrorMessage("Too tired to tear down the barricade."));
        RedrawPlayScreen();
        return false;
      }
      AddMessage(MakeErrorMessage(string.Format("Cannot do that : {0}.", actionBump.FailReason)));
      return false;
    }

    public void DoMakeAggression(Actor aggressor, Actor target)
    {
      if (aggressor.Faction.IsEnemyOf(target.Faction)) return;
      bool wasAlreadyEnemy = aggressor.IsAggressorOf(target) || target.IsAggressorOf(aggressor);
      if (!target.IsPlayer && !target.IsSleeping && (!aggressor.IsAggressorOf(target) && !target.IsAggressorOf(aggressor)))
        DoSay(target, aggressor, "BASTARD! TRAITOR!", RogueGame.Sayflags.IS_FREE_ACTION);
      aggressor.MarkAsAgressorOf(target);
      target.MarkAsSelfDefenceFrom(aggressor);
      if (target.IsSleeping) return;
      Faction faction = target.Faction;
      if (faction == GameFactions.ThePolice) {
        if (aggressor.Model.Abilities.IsLawEnforcer && !Rules.IsMurder(aggressor, target)) return;
        OnMakeEnemyOfCop(aggressor, target, wasAlreadyEnemy);
      } else {
        if (faction != GameFactions.TheArmy) return;
        OnMakeEnemyOfSoldier(aggressor, target, wasAlreadyEnemy);
      }
    }

    private void OnMakeEnemyOfCop(Actor aggressor, Actor cop, bool wasAlreadyEnemy)
    {
      if (!wasAlreadyEnemy)
        DoSay(cop, aggressor, string.Format("TO DISTRICT PATROLS : {0} MUST DIE!", aggressor.TheName), RogueGame.Sayflags.IS_FREE_ACTION);
      MakeEnemyOfTargetFactionInDistrict(aggressor, cop, a => {
        int turnCounter = Session.Get.WorldTime.TurnCounter;
        ClearMessages();
        AddMessage(new Data.Message("You get a message from your police radio.", turnCounter, Color.White));
        AddMessage(new Data.Message(string.Format("{0} is armed and dangerous. Shoot on sight!", (object)aggressor.TheName), turnCounter, Color.White));
        AddMessage(new Data.Message(string.Format("Current location : {0}@{1},{2}", (object)aggressor.Location.Map.Name, (object)aggressor.Location.Position.X, (object)aggressor.Location.Position.Y), turnCounter, Color.White));
        AddMessagePressEnter();
      }, a => {
        if (a == cop) return false;    // target already knows
        if (a.IsSleeping) return false;   // can't hear when sleeping
        if (a == aggressor || a.Leader == aggressor) return false;  // aggressor doesn't find this message informative
        if (!a.HasActivePoliceRadio) return false;  // not in communication (police have implicit radios)
        if (a.IsEnemyOf(aggressor)) return false; // already an enemy...presumed informed
        return true;
      });
    }

    private void OnMakeEnemyOfSoldier(Actor aggressor, Actor soldier, bool wasAlreadyEnemy)
    {
      if (!wasAlreadyEnemy)
        DoSay(soldier, aggressor, string.Format("TO DISTRICT SQUADS : {0} MUST DIE!", aggressor.TheName), RogueGame.Sayflags.IS_FREE_ACTION);
      MakeEnemyOfTargetFactionInDistrict(aggressor, soldier, a => {
        int turnCounter = Session.Get.WorldTime.TurnCounter;
        ClearMessages();
        AddMessage(new Data.Message("You get a message from your army radio.", turnCounter, Color.White));
        AddMessage(new Data.Message(string.Format("{0} is armed and dangerous. Shoot on sight!", (object)aggressor.Name), turnCounter, Color.White));
        AddMessage(new Data.Message(string.Format("Current location : {0}@{1},{2}", (object)aggressor.Location.Map.Name, (object)aggressor.Location.Position.X, (object)aggressor.Location.Position.Y), turnCounter, Color.White));
        AddMessagePressEnter();
      }, a => {
        if (a == soldier) return false;    // target already knows
        if (a.IsSleeping) return false;   // can't hear when sleeping
        if (a == aggressor || a.Leader == aggressor) return false;  // aggressor doesn't find this message informative
        if (a.Faction != soldier.Faction) return false;  // not in communication
        if (a.IsEnemyOf(aggressor)) return false; // already an enemy...presumed informed
        return true;
      });
    }

    // fn is the UI message
    private void MakeEnemyOfTargetFactionInDistrict(Actor aggressor, Actor target, Action<Actor> fn, Predicate<Actor> pred)
    {
      if (null!=fn) {
        if (m_Player.Location.Map.District==target.Location.Map.District) {
          m_Player.MessagePlayerOnce(fn,pred);
        } else {
          target.MessagePlayerOnce(fn, pred);
        }
      }
      Faction faction = target.Faction;
      foreach (Map map in target.Location.Map.District.Maps) {
        foreach (Actor actor in map.Actors) {
          if (actor != aggressor && actor != target && (actor.Faction == faction && actor.Leader != aggressor)) {
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
      if (!attacker.IsEnemyOf(defender)) DoMakeAggression(attacker, defender);
      Attack attack = attacker.MeleeAttack(defender);
      Defence defence = Rules.ActorDefence(defender, defender.CurrentDefence);
      attacker.SpendActionPoints(Rules.BASE_ACTION_COST);
      attacker.SpendStaminaPoints(Rules.STAMINA_COST_MELEE_ATTACK + attack.StaminaPenalty);
      int num1 = m_Rules.RollSkill(attack.HitValue);
      int num2 = m_Rules.RollSkill(defence.Value);
      // weird to have the presence of sleeping livings influence damage inflicted
      int num3 = (num1 > num2 ? m_Rules.RollDamage(defender.IsSleeping ? attack.DamageValue * 2 : attack.DamageValue) - defence.Protection_Hit : 0);

      OnLoudNoise(attacker.Location.Map, attacker.Location.Position, "Nearby fighting");
#if SUICIDE_BY_LONG_WAIT
      if (m_IsPlayerLongWait && defender.IsPlayer) m_IsPlayerLongWaitForcedStop = true;
#endif
      bool player1 = ForceVisibleToPlayer(defender);
      bool player2 = player1 ? IsVisibleToPlayer(attacker) : ForceVisibleToPlayer(attacker);
      bool flag = attacker.IsPlayer || defender.IsPlayer;
      if (!player1 && !player2 && (!flag && m_Rules.RollChance(PLAYER_HEAR_FIGHT_CHANCE)))
        AddMessageIfAudibleForPlayer(attacker.Location, "You hear fighting");
      if (player2) {
        AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(attacker.Location), SIZE_OF_ACTOR)));
        AddOverlay(new OverlayRect(Color.Red, new Rectangle(MapToScreen(defender.Location), SIZE_OF_ACTOR)));
        AddOverlay(new OverlayImage(MapToScreen(attacker.Location), GameImages.ICON_MELEE_ATTACK));
      }
      if (num1 > num2) {
        if (num3 > 0) {
          InflictDamage(defender, num3);
          if (attacker.Model.Abilities.CanZombifyKilled && !defender.Model.Abilities.IsUndead) {
            attacker.RegenHitPoints(Rules.ActorBiteHpRegen(attacker, num3));
            attacker.RottingEat(attacker.BiteNutritionValue(num3));
            if (player2)
              AddMessage(MakeMessage(attacker, Conjugate(attacker, VERB_FEAST_ON), defender, " flesh !"));
            defender.Infect(Rules.InfectionForDamage(attacker, num3));
          }
          if (defender.HitPoints <= 0) {
            if (player2 || player1) {
              AddMessage(MakeMessage(attacker, Conjugate(attacker, defender.Model.Abilities.IsUndead ? VERB_DESTROY : (Rules.IsMurder(attacker, defender) ? VERB_MURDER : VERB_KILL)), defender, " !"));
              AddOverlay(new OverlayImage(MapToScreen(defender.Location), GameImages.ICON_KILLED));
              RedrawPlayScreen();
              AnimDelay(DELAY_LONG);
            }
            KillActor(attacker, defender, "hit");
            if (attacker.Model.Abilities.IsUndead && !defender.Model.Abilities.IsUndead)
              SeeingCauseInsanity(attacker, attacker.Location, Rules.SANITY_HIT_EATEN_ALIVE, string.Format("{0} eaten alive", defender.Name));
            if (Session.Get.HasImmediateZombification || defender == m_Player) {
              if (attacker.Model.Abilities.CanZombifyKilled && !defender.Model.Abilities.IsUndead && m_Rules.RollChance(s_Options.ZombificationChance)) {
                if (defender.IsPlayer)
                  defender.Location.Map.TryRemoveCorpseOf(defender);
                Zombify(attacker, defender, false);
                if (player1) {
                  AddMessage(MakeMessage(attacker, Conjugate(attacker, "turn"), defender, " into a Zombie!"));
                  RedrawPlayScreen();
                  AnimDelay(DELAY_LONG);
                }
              } else if (defender == m_Player && !defender.Model.Abilities.IsUndead && defender.Infection > 0) {
                defender.Location.Map.TryRemoveCorpseOf(defender);
                Zombify(null, defender, false);
                AddMessage(MakeMessage(defender, Conjugate(defender, "turn") + " into a Zombie!"));
                RedrawPlayScreen();
                AnimDelay(DELAY_LONG);
              }
            }
          } else if (player2 || player1) {
            AddMessage(MakeMessage(attacker, Conjugate(attacker, attack.Verb), defender, string.Format(" for {0} damage.", num3)));
            AddOverlay(new OverlayImage(MapToScreen(defender.Location), GameImages.ICON_MELEE_DAMAGE));
            AddOverlay(new RogueGame.OverlayText(MapToScreen(defender.Location).Add(10, 10), Color.White, num3.ToString(), new Color?(Color.Black)));
            RedrawPlayScreen();
            AnimDelay(flag ? DELAY_NORMAL : DELAY_SHORT);
          }
        } else if (player2 || player1) {
          AddMessage(MakeMessage(attacker, Conjugate(attacker, attack.Verb), defender, " for no effect."));
          AddOverlay(new OverlayImage(MapToScreen(defender.Location), GameImages.ICON_MELEE_MISS));
          RedrawPlayScreen();
          AnimDelay(flag ? DELAY_NORMAL : DELAY_SHORT);
        }
      }
      else if (player2 || player1) {
        AddMessage(MakeMessage(attacker, Conjugate(attacker, VERB_MISS), defender));
        AddOverlay(new OverlayImage(MapToScreen(defender.Location), GameImages.ICON_MELEE_MISS));
        RedrawPlayScreen();
        AnimDelay(flag ? DELAY_NORMAL : DELAY_SHORT);
      }
      if (attacker.GetEquippedWeapon() is ItemMeleeWeapon itemMeleeWeapon && !itemMeleeWeapon.Model.IsUnbreakable && m_Rules.RollChance(itemMeleeWeapon.IsFragile ? Rules.MELEE_WEAPON_FRAGILE_BREAK_CHANCE : Rules.MELEE_WEAPON_BREAK_CHANCE))
      {
        attacker.OnUnequipItem(itemMeleeWeapon);
        if (itemMeleeWeapon.Quantity > 1)
          --itemMeleeWeapon.Quantity;
        else
          attacker.Inventory.RemoveAllQuantity(itemMeleeWeapon);
        if (player2) {
          AddMessage(MakeMessage(attacker, string.Format(": {0} breaks and is now useless!", itemMeleeWeapon.TheName)));
          RedrawPlayScreen();
          AnimDelay(flag ? DELAY_NORMAL : DELAY_SHORT);
        }
      }
      ClearOverlays();
    }

    public void DoSingleRangedAttack(Actor attacker, Actor defender, List<Point> LoF, FireMode mode)
    {
      if (!attacker.IsEnemyOf(defender)) DoMakeAggression(attacker, defender);
      switch (mode) {
        case FireMode.DEFAULT:
          attacker.SpendActionPoints(Rules.BASE_ACTION_COST);
          DoSingleRangedAttack(attacker, defender, LoF, 1f);
          break;
        case FireMode.RAPID:
          attacker.SpendActionPoints(Rules.BASE_ACTION_COST);
          DoSingleRangedAttack(attacker, defender, LoF, Rules.RAPID_FIRE_FIRST_SHOT_ACCURACY);
          ItemRangedWeapon itemRangedWeapon = attacker.GetEquippedWeapon() as ItemRangedWeapon;
          if (defender.IsDead) {
            --itemRangedWeapon.Ammo;
            Attack currentRangedAttack = attacker.CurrentRangedAttack;
            AddMessage(MakeMessage(attacker, string.Format("{0} at nothing.", Conjugate(attacker, currentRangedAttack.Verb))));
            break;
          }
          if (itemRangedWeapon.Ammo <= 0) break;
          DoSingleRangedAttack(attacker, defender, LoF, Rules.RAPID_FIRE_SECOND_SHOT_ACCURACY);
          break;
        default:
          throw new ArgumentOutOfRangeException("unhandled mode");
      }
    }

    private void DoSingleRangedAttack(Actor attacker, Actor defender, List<Point> LoF, float accuracyFactor)
    {
      attacker.Activity = Activity.FIGHTING;
      attacker.TargetActor = defender;
      int distance = Rules.GridDistance(attacker.Location, defender.Location);
      Attack attack = attacker.RangedAttack(distance, defender);
      Defence defence = Rules.ActorDefence(defender, defender.CurrentDefence);
      attacker.SpendStaminaPoints(attack.StaminaPenalty);
      if (attack.Kind == AttackKind.FIREARM && (m_Rules.RollChance(Session.Get.World.Weather.IsRain() ? Rules.FIREARM_JAM_CHANCE_RAIN : Rules.FIREARM_JAM_CHANCE_NO_RAIN) && ForceVisibleToPlayer(attacker)))
      {
        AddMessage(MakeMessage(attacker, " : weapon jam!"));
      } else {
        ItemRangedWeapon itemRangedWeapon = attacker.GetEquippedWeapon() as ItemRangedWeapon;
        if (itemRangedWeapon == null) throw new InvalidOperationException("DoSingleRangedAttack but no equipped ranged weapon");
        --itemRangedWeapon.Ammo;
        if (DoCheckFireThrough(attacker, LoF)) return;
#if SUICIDE_BY_LONG_WAIT
        if (m_IsPlayerLongWait && defender.IsPlayer) m_IsPlayerLongWaitForcedStop = true;
#endif
        int num1 = (int)(accuracyFactor * (double)m_Rules.RollSkill(attack.HitValue));
        int num2 = m_Rules.RollSkill(defence.Value);
        bool player1 = ForceVisibleToPlayer(defender.Location);
        bool player2 = player1 ? IsVisibleToPlayer(attacker.Location) : ForceVisibleToPlayer(attacker.Location);
        bool flag = attacker.IsPlayer || defender.IsPlayer;
        if (!player1 && !player2 && (!flag && m_Rules.RollChance(PLAYER_HEAR_FIGHT_CHANCE)))
          AddMessageIfAudibleForPlayer(attacker.Location, "You hear firing");
        if (player2) {
          AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(attacker.Location), SIZE_OF_ACTOR)));
          AddOverlay(new OverlayRect(Color.Red, new Rectangle(MapToScreen(defender.Location), SIZE_OF_ACTOR)));
          AddOverlay(new OverlayImage(MapToScreen(attacker.Location), GameImages.ICON_RANGED_ATTACK));
        }
        if (num1 > num2) {
          int dmg = m_Rules.RollDamage(defender.IsSleeping ? attack.DamageValue * 2 : attack.DamageValue) - defence.Protection_Shot;
          if (dmg > 0) {
            InflictDamage(defender, dmg);
            if (defender.HitPoints <= 0) {
              if (player1) {
                AddMessage(MakeMessage(attacker, Conjugate(attacker, defender.Model.Abilities.IsUndead ? VERB_DESTROY : (Rules.IsMurder(attacker, defender) ? VERB_MURDER : VERB_KILL)), defender, " !"));
                AddOverlay(new OverlayImage(MapToScreen(defender.Location), GameImages.ICON_KILLED));
                RedrawPlayScreen();
                AnimDelay(DELAY_LONG);
              }
              KillActor(attacker, defender, "shot");
            } else if (player1) {
              AddMessage(MakeMessage(attacker, Conjugate(attacker, attack.Verb), defender, string.Format(" for {0} damage.", dmg)));
              AddOverlay(new OverlayImage(MapToScreen(defender.Location), GameImages.ICON_RANGED_DAMAGE));
              AddOverlay(new OverlayText(MapToScreen(defender.Location).Add(10, 10), Color.White, dmg.ToString(), Color.Black));
              RedrawPlayScreen();
              AnimDelay(flag ? DELAY_NORMAL : DELAY_SHORT);
            }
          } else if (player1) {
            AddMessage(MakeMessage(attacker, Conjugate(attacker, attack.Verb), defender, " for no effect."));
            AddOverlay(new OverlayImage(MapToScreen(defender.Location), GameImages.ICON_RANGED_MISS));
            RedrawPlayScreen();
            AnimDelay(flag ? DELAY_NORMAL : DELAY_SHORT);
          }
        } else if (player1) {
          AddMessage(MakeMessage(attacker, Conjugate(attacker, VERB_MISS), defender));
          AddOverlay(new OverlayImage(MapToScreen(defender.Location), GameImages.ICON_RANGED_MISS));
          RedrawPlayScreen();
          AnimDelay(flag ? DELAY_NORMAL : DELAY_SHORT);
        }
        ClearOverlays();
      }
    }

    private bool DoCheckFireThrough(Actor attacker, List<Point> LoF)
    {
      foreach (Point point in LoF) {
        MapObject mapObjectAt = attacker.Location.Map.GetMapObjectAt(point);
        if (mapObjectAt != null && mapObjectAt.BreaksWhenFiredThrough && (mapObjectAt.BreakState != MapObject.Break.BROKEN && !mapObjectAt.IsWalkable)) {
          bool player1 = ForceVisibleToPlayer(attacker);
          bool player2 = player1 ? IsVisibleToPlayer(mapObjectAt) : ForceVisibleToPlayer(mapObjectAt);
          if (player1 || player2) {
            if (player1) {
              AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(attacker.Location), SIZE_OF_ACTOR)));
              AddOverlay(new OverlayImage(MapToScreen(attacker.Location), GameImages.ICON_RANGED_ATTACK));
            }
            if (player2)
              AddOverlay(new OverlayRect(Color.Red, new Rectangle(MapToScreen(point), SIZE_OF_TILE)));
            AnimDelay(attacker.IsPlayer ? DELAY_NORMAL : DELAY_SHORT);
          }
          DoDestroyObject(mapObjectAt);
          return true;
        }
      }
      return false;
    }

    public void DoThrowGrenadeUnprimed(Actor actor, Point targetPos)
    {
      ItemGrenade itemGrenade = actor.GetEquippedWeapon() as ItemGrenade;
      if (itemGrenade == null) throw new InvalidOperationException("throwing grenade but no grenade equiped ");
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      actor.Inventory.Consume(itemGrenade);
      actor.Location.Map.DropItemAtExt(new ItemGrenadePrimed(GameItems.Cast<ItemGrenadePrimedModel>(itemGrenade.PrimedModelID)), targetPos);
      if (!ForceVisibleToPlayer(actor) && !ForceVisibleToPlayer(actor.Location.Map, targetPos)) return;
      AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(actor.Location), SIZE_OF_ACTOR)));
      AddOverlay(new OverlayRect(Color.Red, new Rectangle(MapToScreen(targetPos), SIZE_OF_TILE)));
      AddMessage(MakeMessage(actor, string.Format("{0} a {1}!", Conjugate(actor, VERB_THROW), itemGrenade.Model.SingleName)));
      RedrawPlayScreen();
      AnimDelay(DELAY_LONG);
      ClearOverlays();
      RedrawPlayScreen();
    }

    public void DoThrowGrenadePrimed(Actor actor, Point targetPos)
    {
      ItemGrenadePrimed itemGrenadePrimed = actor.GetEquippedWeapon() as ItemGrenadePrimed;
      if (itemGrenadePrimed == null)
        throw new InvalidOperationException("throwing primed grenade but no primed grenade equiped ");
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      actor.Inventory.RemoveAllQuantity(itemGrenadePrimed);
      actor.Location.Map.DropItemAtExt(itemGrenadePrimed, targetPos);
      if (!ForceVisibleToPlayer(actor) && !ForceVisibleToPlayer(actor.Location.Map, targetPos)) return;
      AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(actor.Location), SIZE_OF_ACTOR)));
      AddOverlay(new OverlayRect(Color.Red, new Rectangle(MapToScreen(targetPos), SIZE_OF_TILE)));
      AddMessage(MakeMessage(actor, string.Format("{0} back a {1}!", Conjugate(actor, VERB_THROW), itemGrenadePrimed.Model.SingleName)));
      RedrawPlayScreen();
      AnimDelay(DELAY_LONG);
      ClearOverlays();
      RedrawPlayScreen();
    }

    private void ShowBlastImage(Point screenPos, BlastAttack attack, int damage)
    {
      float alpha = (float) (0.1 + damage / (double)attack.Damage[0]);
      if (alpha > 1.0) alpha = 1f;
      AddOverlay(new OverlayTransparentImage(alpha, screenPos, GameImages.ICON_BLAST));
      AddOverlay(new OverlayText(screenPos, Color.Red, damage.ToString(), Color.Black));
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void DoBlast(Location location, BlastAttack blastAttack)
    {
      OnLoudNoise(location.Map, location.Position, "A loud EXPLOSION");
      if (ForceVisibleToPlayer(location)) {
        ShowBlastImage(MapToScreen(location), blastAttack, blastAttack.Damage[0]);
        RedrawPlayScreen();
        AnimDelay(DELAY_LONG);
        RedrawPlayScreen();
      } else if (m_Rules.RollChance(PLAYER_HEAR_EXPLOSION_CHANCE))
        AddMessageIfAudibleForPlayer(location, "You hear an explosion");
      ApplyExplosionDamage(location, 0, blastAttack);
      for (int waveDistance = 1; waveDistance <= blastAttack.Radius; ++waveDistance) {
        if (ApplyExplosionWave(location, waveDistance, blastAttack)) {
          RedrawPlayScreen();
          AnimDelay(DELAY_NORMAL);
        }
      }
      ClearOverlays();
    }

    private bool ApplyExplosionWave(Location center, int waveDistance, BlastAttack blast)
    {
      bool flag = false;
      foreach(Point pt in Enumerable.Range(0, 8*waveDistance).Select(i => center.Position.RadarSweep(waveDistance,i))) {
          flag |= ApplyExplosionWaveSub(center, pt, waveDistance, blast);
      }
      return flag;
    }

    private bool ApplyExplosionWaveSub(Location blastCenter, Point pt, int waveDistance, BlastAttack blast)
    {
      if (!blastCenter.Map.IsValid(pt) || !LOS.CanTraceFireLine(blastCenter, pt, waveDistance))
        return false;
      int damage = ApplyExplosionDamage(new Location(blastCenter.Map, pt), waveDistance, blast);
      if (!ForceVisibleToPlayer(blastCenter.Map, pt)) return false;
      ShowBlastImage(MapToScreen(pt), blast, damage);
      return true;
    }

    private int ApplyExplosionDamage(Location location, int distanceFromBlast, BlastAttack blast)
    {
      Contract.Requires(!blast.CanDestroyWalls);    // not implemented
      int num1 = blast.DamageAt(distanceFromBlast);
      if (num1 <= 0) return 0;
      Map map = location.Map;
      Actor actorAt =  location.Map.GetActorAtExt(location.Position.X,location.Position.Y);
      if (actorAt != null) {
        ExplosionChainReaction(actorAt.Inventory, location);
        int dmg = num1 - (actorAt.CurrentDefence.Protection_Hit + actorAt.CurrentDefence.Protection_Shot) / 2;
        if (dmg > 0) {
          InflictDamage(actorAt, dmg);
          if (ForceVisibleToPlayer(actorAt))
            AddMessage(new Data.Message(string.Format("{0} is hit for {1} damage!", actorAt.Name, dmg), map.LocalTime.TurnCounter, Color.Crimson));
          if (actorAt.HitPoints <= 0 && !actorAt.IsDead) {
            KillActor(null, actorAt, string.Format("explosion {0} damage", dmg));
            if (ForceVisibleToPlayer(actorAt))
              AddMessage(new Data.Message(string.Format("{0} dies in the explosion!", actorAt.Name), map.LocalTime.TurnCounter, Color.Crimson));
          }
        } else
          AddMessage(new Data.Message(string.Format("{0} is hit for no damage.", actorAt.Name), map.LocalTime.TurnCounter, Color.White));
      }
      Inventory itemsAt = location.Map.GetItemsAtExt(location.Position.X,location.Position.Y);
      if (itemsAt != null) {
        ExplosionChainReaction(itemsAt, location);
        int chance = num1;
        List<Item> objList = new List<Item>(itemsAt.CountItems);
        foreach (Item obj in itemsAt.Items) {
          if (!obj.IsUnique && !obj.Model.IsUnbreakable && (!(obj is ItemPrimedExplosive) || (obj as ItemPrimedExplosive).FuseTimeLeft > 0) && m_Rules.RollChance(chance))
            objList.Add(obj);
        }
        foreach (Item it in objList)
          map.RemoveItemAt(it, location.Position);
      }
      if (blast.CanDamageObjects) {
        MapObject mapObjectAt = map.GetMapObjectAt(location.Position);
        if (mapObjectAt != null) {
          DoorWindow doorWindow = mapObjectAt as DoorWindow;
          if (mapObjectAt.IsBreakable || doorWindow != null && doorWindow.IsBarricaded) {
            int val2 = num1;
            if (doorWindow != null && doorWindow.IsBarricaded) {
              int num2 = Math.Min(doorWindow.BarricadePoints, val2);
              doorWindow.Barricade(-num2);
              val2 -= num2;
            }
            if (val2 > 0) {
              mapObjectAt.HitPoints -= val2;
              if (mapObjectAt.HitPoints <= 0)
                DoDestroyObject(mapObjectAt);
            }
          }
        }
      }
      List<Corpse> corpsesAt = map.GetCorpsesAt(location.Position);
      if (corpsesAt != null) {
        foreach (Corpse c in corpsesAt)
           c.TakeDamage(num1);
      }
      // XXX implementation of blast.CanDestroyWalls goes here
      return num1;
    }

    static private void ExplosionChainReaction(Inventory inv, Location location)
    {
      if (inv == null || inv.IsEmpty) return;
      List<ItemExplosive> itemExplosiveList = null;
      List<ItemPrimedExplosive> itemPrimedExplosiveList = null;
      foreach (Item obj in inv.Items) {
        ItemExplosive itemExplosive = obj as ItemExplosive;
		if (null == itemExplosive) continue;
        if (itemExplosive is ItemPrimedExplosive primed) {
          primed.FuseTimeLeft = 0;
        } else {
          if (itemExplosiveList == null) itemExplosiveList = new List<ItemExplosive>();
          if (itemPrimedExplosiveList == null) itemPrimedExplosiveList = new List<ItemPrimedExplosive>();
          itemExplosiveList.Add(itemExplosive);
          for (int index = 0; index < obj.Quantity; ++index)
            itemPrimedExplosiveList.Add(new ItemPrimedExplosive(GameItems.Cast<ItemExplosiveModel>(itemExplosive.PrimedModelID))
            {
              FuseTimeLeft = 0
            });
        }
      }
      if (itemExplosiveList != null) {
        foreach (Item it in itemExplosiveList)
          inv.RemoveAllQuantity(it);
      }
      if (itemPrimedExplosiveList == null) return;
      foreach (Item it in itemPrimedExplosiveList)
        location.Map.DropItemAt(it, location.Position);
    }

    public void DoChat(Actor speaker, Actor target)
    {
      Contract.Requires(!speaker.IsPlayer);
      speaker.SpendActionPoints(Rules.BASE_ACTION_COST);
      if (ForceVisibleToPlayer(speaker) || ForceVisibleToPlayer(target))
        AddMessage(MakeMessage(speaker, Conjugate(speaker, VERB_CHAT_WITH), target));
      if (!speaker.CanTradeWith(target)) return;
      DoTrade(speaker, target);
    }

    private void DoTrade(Actor speaker, Item itSpeaker, Actor target, bool doesTargetCheckForInterestInOffer)
    {
      Contract.Requires(!target.IsPlayer);
      bool flag1 = ForceVisibleToPlayer(speaker) || ForceVisibleToPlayer(target);
      // bail on null item from speaker early
      if (null == itSpeaker) {
        if (flag1) AddMessage(MakeMessage(target, string.Format("is not interested in {0} items.", speaker.Name)));
        return;
      }

      bool wantedItem = true;
      bool flag3 = (target.Controller as ObjectiveAI).IsInterestingTradeItem(speaker, itSpeaker);
      if (target.Leader == speaker)
        wantedItem = true;
      else if (doesTargetCheckForInterestInOffer)
        wantedItem = flag3;

      if (!wantedItem)
      { // offered item is not of perceived use
        if (flag1) AddMessage(MakeMessage(target, string.Format("is not interested in {0}.", itSpeaker.TheName)));
        return;
      };

      Item trade = PickItemToTrade(target, speaker, itSpeaker);
      if (null == trade) {
        if (flag1) AddMessage(MakeMessage(speaker, string.Format("is not interested in {0} items.", target.Name)));
        return;
      };

      bool isPlayer = speaker.IsPlayer;
      if (flag1)
        AddMessage(MakeMessage(target, string.Format("{0} {1} for {2}.", Conjugate(target, VERB_OFFER), trade.AName, itSpeaker.AName)));

      bool acceptDeal = true;
      if (speaker.IsPlayer) {
        AddOverlay(new RogueGame.OverlayPopup(TRADE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, Point.Empty));
        RedrawPlayScreen();
        acceptDeal = WaitYesOrNo();
        ClearOverlays();
        RedrawPlayScreen();
      }
      else
        acceptDeal = !target.HasLeader || (target.Controller as OrderableAI).Directives.CanTrade;

      if (!acceptDeal) {
        if (!flag1) return;
        AddMessage(MakeMessage(speaker, string.Format("{0}.", Conjugate(speaker, VERB_REFUSE_THE_DEAL))));
        if (!isPlayer) return;
        RedrawPlayScreen();
        return;
      }

      if (flag1) {
        AddMessage(MakeMessage(speaker, string.Format("{0}.", Conjugate(speaker, VERB_ACCEPT_THE_DEAL))));
        if (isPlayer) RedrawPlayScreen();
      }
      if (target.Leader == speaker && flag3)
        DoSay(target, speaker, "Thank you for this good deal.", RogueGame.Sayflags.IS_FREE_ACTION);
      if (itSpeaker.IsEquipped) DoUnequipItem(speaker, itSpeaker);
      if (trade.IsEquipped) DoUnequipItem(target, trade);
      speaker.Inventory.RemoveAllQuantity(itSpeaker);
      target.Inventory.RemoveAllQuantity(trade);
      speaker.Inventory.AddAll(trade);
      target.Inventory.AddAll(itSpeaker);
    }

    public void DoTrade(Actor speaker, Actor target)
    {   // precondition: !speaker.IsPlayer (need different implementation)
      Contract.Requires(!speaker.IsPlayer);
      Contract.Requires(!target.IsPlayer);  // strictly implied by Actor::CanTradeWith
#if DEBUG
      if (!speaker.CanTradeWith(target, out string reason)) throw new ArgumentOutOfRangeException("Trading not supported",reason);
#endif
      Item trade = PickItemToTrade(speaker, target);
      DoTrade(speaker, trade, target, false);
    }

    private Item PickItemToTrade(Actor speaker, Actor buyer)
    {
      List<Item> objList = speaker.GetInterestingTradeableItems(buyer);
      if (objList == null || 0>=objList.Count) return null;
      return objList[m_Rules.Roll(0, objList.Count)];
    }

    private Item PickItemToTrade(Actor speaker, Actor buyer, Item itSpeaker)
    {
      List<Item> objList = speaker.GetInterestingTradeableItems(buyer);
      if (objList == null || 0>=objList.Count) return null;
      IEnumerable<Item> tmp = objList.Where(it=>itSpeaker.Model.ID != it.Model.ID);
      // XXX disallow clearly non-mutual advantage trades
      switch(itSpeaker.Model.ID)
      {
      // two weapons for the ammo
      case GameItems.IDs.RANGED_PRECISION_RIFLE:
      case GameItems.IDs.RANGED_ARMY_RIFLE:
        tmp = tmp.Where(it=> it.Model.ID!= GameItems.IDs.AMMO_HEAVY_RIFLE);
        break;
      case GameItems.IDs.AMMO_HEAVY_RIFLE:
        tmp = tmp.Where(it=> it.Model.ID!= GameItems.IDs.RANGED_PRECISION_RIFLE && it.Model.ID!= GameItems.IDs.RANGED_ARMY_RIFLE);
        break;
      case GameItems.IDs.RANGED_PISTOL:
      case GameItems.IDs.RANGED_KOLT_REVOLVER:
        tmp = tmp.Where(it=> it.Model.ID!= GameItems.IDs.AMMO_LIGHT_PISTOL);
        break;
      case GameItems.IDs.AMMO_LIGHT_PISTOL:
        tmp = tmp.Where(it=> it.Model.ID!= GameItems.IDs.RANGED_PISTOL && it.Model.ID!= GameItems.IDs.RANGED_KOLT_REVOLVER);
        break;
      // one weapon for the ammo
      case GameItems.IDs.RANGED_ARMY_PISTOL:
        tmp = tmp.Where(it=> it.Model.ID!= GameItems.IDs.AMMO_HEAVY_PISTOL);
        break;
      case GameItems.IDs.AMMO_HEAVY_PISTOL:
        tmp = tmp.Where(it=> it.Model.ID!= GameItems.IDs.RANGED_ARMY_PISTOL);
        break;
      case GameItems.IDs.RANGED_HUNTING_CROSSBOW:
        tmp = tmp.Where(it=> it.Model.ID!= GameItems.IDs.AMMO_BOLTS);
        break;
      case GameItems.IDs.AMMO_BOLTS:
        tmp = tmp.Where(it=> it.Model.ID!= GameItems.IDs.RANGED_HUNTING_CROSSBOW);
        break;
      case GameItems.IDs.RANGED_HUNTING_RIFLE:
        tmp = tmp.Where(it=> it.Model.ID!= GameItems.IDs.AMMO_LIGHT_RIFLE);
        break;
      case GameItems.IDs.AMMO_LIGHT_RIFLE:
        tmp = tmp.Where(it=> it.Model.ID!= GameItems.IDs.RANGED_HUNTING_RIFLE);
        break;
      case GameItems.IDs.RANGED_SHOTGUN:
        tmp = tmp.Where(it=> it.Model.ID!= GameItems.IDs.AMMO_SHOTGUN);
        break;
      case GameItems.IDs.AMMO_SHOTGUN:
        tmp = tmp.Where(it=> it.Model.ID!= GameItems.IDs.RANGED_SHOTGUN);
        break;
      // flashlights.  larger radius and longer duration are independently better...do not trade if both are worse
      case GameItems.IDs.LIGHT_FLASHLIGHT:
        tmp = tmp.Where(it=> it.Model.ID!= GameItems.IDs.LIGHT_BIG_FLASHLIGHT || (itSpeaker as BatteryPowered).Batteries<=(it as BatteryPowered).Batteries);
        break;
      case GameItems.IDs.LIGHT_BIG_FLASHLIGHT:
        tmp = tmp.Where(it=> it.Model.ID!= GameItems.IDs.LIGHT_FLASHLIGHT || (itSpeaker as BatteryPowered).Batteries>=(it as BatteryPowered).Batteries);
        break;
      }
      if (!tmp.Any()) return null;
      objList = tmp.ToList();
      return objList[m_Rules.Roll(0, objList.Count)];
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    static public void DoSay(Actor speaker, Actor target, string text, RogueGame.Sayflags flags)
    {
      speaker.Say(target,text,flags);
    }

    public void DoShout(Actor speaker, string text)
    {
      speaker.SpendActionPoints(Rules.BASE_ACTION_COST);
      OnLoudNoise(speaker.Location.Map, speaker.Location.Position, "A SHOUT");
      if (!AreLinkedByPhone(speaker, m_Player) && !ForceVisibleToPlayer(speaker)) return;
      if (speaker.Leader == m_Player) {
        ClearMessages();
        AddOverlay((new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(speaker.Location), SIZE_OF_ACTOR))));
        AddMessage(MakeMessage(speaker, string.Format("{0}!!", Conjugate(speaker, VERB_RAISE_ALARM))));
        if (text != null) DoEmote(speaker, text);
        AddMessagePressEnter();
        ClearOverlays();
        RemoveLastMessage();
      }
      else if (text == null)
        AddMessage(MakeMessage(speaker, string.Format("{0}!", Conjugate(speaker, VERB_SHOUT))));
      else
        DoEmote(speaker, string.Format("{0} \"{1}\"", Conjugate(speaker, VERB_SHOUT), text));
    }

    public void DoEmote(Actor actor, string text)
    {
      if (!ForceVisibleToPlayer(actor)) return;
      AddMessage(new Data.Message(string.Format("{0} : {1}", actor.Name, text), actor.Location.Map.LocalTime.TurnCounter, SAYOREMOTE_COLOR));
    }

    public void DoTakeFromContainer(Actor actor, Point position)
    {
      Inventory inv = actor.Location.Map.GetItemsAt(position);
      if (actor.IsPlayer && 2 <= inv.CountItems) {
        HandlePlayerTakeItemFromContainer(actor, position);
        return;
      }

      Item topItem = actor.Location.Map.GetItemsAt(position).TopItem;
      DoTakeItem(actor, position, topItem);
    }

    public void DoTakeItem(Actor actor, Point position, Item it)
    {
      Contract.Requires(actor.Location.Map.GetItemsAt(position)?.Contains(it) ?? false);
      Map map = actor.Location.Map;
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      if (it is ItemTrap) (it as ItemTrap).IsActivated = false;
      int quantity = it.Quantity;
      int quantityAdded = actor.Inventory.AddAsMuchAsPossible(it);
      if (quantityAdded == quantity) map.RemoveItemAt(it, position);
      if (ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(new Location(map, position)))
        AddMessage(MakeMessage(actor, Conjugate(actor, VERB_TAKE), it));
      if (!it.Model.DontAutoEquip && actor.CanEquip(it) && actor.GetEquippedItem(it.Model.EquipmentPart) == null)
        DoEquipItem(actor, it);
#if DEBUG
      if (0< (map.GetItemsAt(position)?.Items.Intersect(actor.Inventory.Items).Count() ?? 0)) throw new InvalidOperationException("inventories not disjoint after:\n"+actor.Name + "'s inventory: " + actor.Inventory.ToString() + "\nstack inventory: " + map.GetItemsAt(position).ToString());
#endif
    }

    public void DoGiveItemTo(Actor actor, Actor target, Item gift)
    {
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      if (target.Leader == actor) {
        bool flag = (target.Controller as ObjectiveAI).IsInterestingItem(gift);
        if (flag)
          DoSay(target, actor, "Thank you, I really needed that!", RogueGame.Sayflags.IS_FREE_ACTION);
        else
          DoSay(target, actor, "Thanks I guess...", RogueGame.Sayflags.IS_FREE_ACTION);
        ModifyActorTrustInLeader(target, flag ? Rules.TRUST_GOOD_GIFT_INCREASE : Rules.TRUST_MISC_GIFT_INCREASE, true);
      } else if (actor.Leader == target) {
        DoSay(target, actor, "Well, here it is...", RogueGame.Sayflags.IS_FREE_ACTION);
        ModifyActorTrustInLeader(actor, Rules.TRUST_GIVE_ITEM_ORDER_PENALTY, true);
      }

      if (gift is ItemTrap) (gift as ItemTrap).IsActivated = false;
      int quantity = gift.Quantity;
      int quantityAdded = target.Inventory.AddAsMuchAsPossible(gift);
      if (quantityAdded==quantity)
        actor.Inventory.RemoveAllQuantity(gift);

      target.SpendActionPoints(Rules.BASE_ACTION_COST);
      if (!gift.Model.DontAutoEquip && target.CanEquip(gift) && target.GetEquippedItem(gift.Model.EquipmentPart) != null)
        DoEquipItem(target, gift);

#if DEBUG
      if (0< target.Inventory.Items.Intersect(actor.Inventory.Items).Count()) throw new InvalidOperationException("inventories not disjoint after:\n"+actor.Name + "'s inventory: " + actor.Inventory.ToString() + target.Name + "'s inventory: " + target.Inventory.ToString());
#endif
      if (!ForceVisibleToPlayer(actor) && !ForceVisibleToPlayer(target)) return;
      AddMessage(MakeMessage(actor, string.Format("{0} {1} to", Conjugate(actor, VERB_GIVE), gift.TheName), target));
    }

    public void DoPutItemInContainer(Actor actor, Point dest, Item gift)
    {
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      if (gift is ItemTrap) (gift as ItemTrap).IsActivated = false;
      actor.Location.Map.DropItemAt(gift, dest);
      actor.Inventory.RemoveAllQuantity(gift);

#if DEBUG
      if (0< (actor.Location.Map.GetItemsAt(actor.Location.Position)?.Items.Intersect(actor.Inventory.Items).Count() ?? 0)) throw new InvalidOperationException("inventories not disjoint after:\n"+actor.Name + "'s inventory: " + actor.Inventory.ToString() + "\nstack inventory: " + actor.Location.Map.GetItemsAt(actor.Location.Position).ToString());
#endif
      if (!ForceVisibleToPlayer(actor) && !ForceVisibleToPlayer(actor.Location.Map, dest)) return;
      AddMessage(MakeMessage(actor, string.Format("{0} {1} away", Conjugate(actor, VERB_PUT), gift.TheName)));
    }


    public void DoEquipItem(Actor actor, Item it)
    {
      Item equippedItem = actor.GetEquippedItem(it.Model.EquipmentPart);
      if (equippedItem != null) DoUnequipItem(actor, equippedItem);
      it.Equip();
      actor.OnEquipItem(it);
#if FAIL
      // postcondition: item is unequippable (but this breaks on merge)
      if (!Rules.CanActorUnequipItem(actor,it)) throw new ArgumentOutOfRangeException("equipped item cannot be unequipped","item type value: "+it.Model.ID.ToString());
#endif
      if (!ForceVisibleToPlayer(actor)) return;
      AddMessage(MakeMessage(actor, Conjugate(actor, VERB_EQUIP), it));
    }

    public void DoUnequipItem(Actor actor, Item it)
    {
      it.Unequip();
      actor.OnUnequipItem(it);
      if (!ForceVisibleToPlayer(actor)) return;
      AddMessage(MakeMessage(actor, Conjugate(actor, VERB_UNEQUIP), it));
    }

    public void DoDropItem(Actor actor, Item it)
    {
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      Item obj = it;
      if (it is ItemTrap) {
        ItemTrap itemTrap1 = it as ItemTrap;
        ItemTrap itemTrap2 = itemTrap1.Clone();
        itemTrap2.IsActivated = itemTrap1.IsActivated;
        obj = itemTrap2;
        if (itemTrap2.Model.ActivatesWhenDropped) itemTrap2.IsActivated = true;
        itemTrap1.IsActivated = false;
#if DEBUG
        if (!itemTrap2.IsActivated) throw new ArgumentOutOfRangeException(nameof(it)," trap being dropped intentionally must be activated");
#endif
      };
      if (it.IsUseless) {
        DiscardItem(actor, it);
        if (ForceVisibleToPlayer(actor)) AddMessage(MakeMessage(actor, Conjugate(actor, VERB_DISCARD), it));
        return;
      }
      // XXX using containers can go here, but we may want a different action anyway
      if (obj == it) DropItem(actor, it);
      else DropCloneItem(actor, it, obj);
      if (ForceVisibleToPlayer(actor)) AddMessage(MakeMessage(actor, Conjugate(actor, VERB_DROP), obj));
#if DEBUG
      if (0< (actor.Location.Map.GetItemsAt(actor.Location.Position)?.Items.Intersect(actor.Inventory.Items).Count() ?? 0)) throw new InvalidOperationException("inventories not disjoint after:\n"+actor.Name + "'s inventory: " + actor.Inventory.ToString() + "\nstack inventory: " + actor.Location.Map.GetItemsAt(actor.Location.Position).ToString());
#endif
    }

    static private void DiscardItem(Actor actor, Item it)
    {
      actor.Inventory.RemoveAllQuantity(it);
      it.Unequip();
    }

    static private void DropItem(Actor actor, Item it)
    {
      actor.Inventory.RemoveAllQuantity(it);
      actor.Location.Map.DropItemAt(it, actor.Location.Position);
      it.Unequip();
    }

    static private void DropCloneItem(Actor actor, Item it, Item clone)
    {
      if (--it.Quantity <= 0)
        actor.Inventory.RemoveAllQuantity(it);
      actor.Location.Map.DropItemAt(clone, actor.Location.Position);
      clone.Unequip();
    }

    public void DoUseItem(Actor actor, Item it)
    {
      if (it is ItemFood) DoUseFoodItem(actor, it as ItemFood);
      else if (it is ItemMedicine) DoUseMedicineItem(actor, it as ItemMedicine);
      else if (it is ItemAmmo) DoUseAmmoItem(actor, it as ItemAmmo);
      else if (it is ItemSprayScent) DoUseSprayScentItem(actor, it as ItemSprayScent);
      else if (it is ItemTrap) DoUseTrapItem(actor, it as ItemTrap);
      else if (it is ItemEntertainment) DoUseEntertainmentItem(actor, it as ItemEntertainment);
    }

    public void DoEatFoodFromGround(Actor actor, ItemFood food)
    {
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      actor.LivingEat(actor.CurrentNutritionOf(food));
      actor.Location.Items.Consume(food);
      bool player = ForceVisibleToPlayer(actor);
      if (player) AddMessage(MakeMessage(actor, Conjugate(actor, VERB_EAT), food));
      if (!food.IsSpoiledAt(actor.Location.Map.LocalTime.TurnCounter) || !m_Rules.RollChance(Rules.FOOD_EXPIRED_VOMIT_CHANCE))
        return;
      DoVomit(actor);
      if (!player) return;
      AddMessage(MakeMessage(actor, string.Format("{0} from eating spoiled food!", Conjugate(actor, VERB_VOMIT))));
    }

    private void DoUseFoodItem(Actor actor, ItemFood food)
    {
      if (actor == m_Player && actor.FoodPoints >= actor.MaxFood - 1) {
        AddMessage(MakeErrorMessage("Don't waste food!"));
      } else {
        actor.SpendActionPoints(Rules.BASE_ACTION_COST);
        actor.LivingEat(actor.CurrentNutritionOf(food));
        actor.Inventory.Consume(food);
        if (food.Model == GameItems.CANNED_FOOD) {
          ItemTrap itemTrap = new ItemTrap(GameItems.EMPTY_CAN)
          {
            IsActivated = true
          };
          actor.Location.Map.DropItemAt(itemTrap, actor.Location.Position);
        }
        bool player = ForceVisibleToPlayer(actor);
        if (player) AddMessage(MakeMessage(actor, Conjugate(actor, VERB_EAT), food));
        if (!food.IsSpoiledAt(actor.Location.Map.LocalTime.TurnCounter) || !m_Rules.RollChance(Rules.FOOD_EXPIRED_VOMIT_CHANCE))
          return;
        DoVomit(actor);
        if (!player) return;
        AddMessage(MakeMessage(actor, string.Format("{0} from eating spoiled food!", Conjugate(actor, VERB_VOMIT))));
      }
    }

    static private void DoVomit(Actor actor)
    {
      actor.StaminaPoints -= Rules.FOOD_VOMIT_STA_COST;
      actor.Drowse(WorldTime.TURNS_PER_HOUR);
      actor.Appetite(WorldTime.TURNS_PER_HOUR);
      Location location = actor.Location;
      location.Map.AddDecorationAt("Tiles\\Decoration\\vomit", location.Position);
    }

    private void DoUseMedicineItem(Actor actor, ItemMedicine med)
    {
      if (actor == m_Player)
      {
        int num1 = actor.MaxHPs - actor.HitPoints;
        int num2 = actor.MaxSTA - actor.StaminaPoints;
        int num3 = actor.MaxSleep - 2 - actor.SleepPoints;
        int infection = actor.Infection;
        int num4 = actor.MaxSanity - actor.Sanity;
        if ((num1 <= 0 || med.Healing <= 0) && (num2 <= 0 || med.StaminaBoost <= 0) && ((num3 <= 0 || med.SleepBoost <= 0) && (infection <= 0 || med.InfectionCure <= 0)) && (num4 <= 0 || med.SanityCure <= 0))
        {
          AddMessage(MakeErrorMessage("Don't waste medicine!"));
          return;
        }
      }
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      actor.RegenHitPoints(Rules.ActorMedicineEffect(actor, med.Healing));
      actor.RegenStaminaPoints(Rules.ActorMedicineEffect(actor, med.StaminaBoost));
      actor.Rest(Rules.ActorMedicineEffect(actor, med.SleepBoost));
      actor.Cure(Rules.ActorMedicineEffect(actor, med.InfectionCure));
      actor.RegenSanity(Rules.ActorMedicineEffect(actor, med.SanityCure));
      actor.Inventory.Consume(med);
      if (!ForceVisibleToPlayer(actor)) return;
      AddMessage(MakeMessage(actor, Conjugate(actor, VERB_HEAL_WITH), med));
    }

    private void DoUseAmmoItem(Actor actor, ItemAmmo ammoItem)
    {
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      ItemRangedWeapon itemRangedWeapon = actor.GetEquippedWeapon() as ItemRangedWeapon;
      int num = Math.Min(itemRangedWeapon.Model.MaxAmmo - itemRangedWeapon.Ammo, ammoItem.Quantity);
      itemRangedWeapon.Ammo += num;
      ammoItem.Quantity -= num;
      if (ammoItem.Quantity <= 0) actor.Inventory.RemoveAllQuantity(ammoItem);
      if (!ForceVisibleToPlayer(actor)) return;
      AddMessage(MakeMessage(actor, Conjugate(actor, VERB_RELOAD), itemRangedWeapon));
    }

    private void DoUseSprayScentItem(Actor actor, ItemSprayScent spray)
    {
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      --spray.SprayQuantity;
      Map map = actor.Location.Map;
      ItemSprayScentModel itemSprayScentModel = spray.Model;
      map.ModifyScentAt(itemSprayScentModel.Odor, itemSprayScentModel.Strength, actor.Location.Position);
      if (!ForceVisibleToPlayer(actor)) return;
      AddMessage(MakeMessage(actor, Conjugate(actor, VERB_SPRAY), spray));
    }

    private void DoUseTrapItem(Actor actor, ItemTrap trap)
    {
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      trap.IsActivated = !trap.IsActivated;
      if (!ForceVisibleToPlayer(actor)) return;
      AddMessage(MakeMessage(actor, Conjugate(actor, trap.IsActivated ? VERB_ACTIVATE : VERB_DESACTIVATE), trap));
    }

    private void DoUseEntertainmentItem(Actor actor, ItemEntertainment ent)
    {
      bool player = ForceVisibleToPlayer(actor);
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      actor.RegenSanity(Rules.ActorSanRegenValue(actor, ent.Model.Value));
      if (player) AddMessage(MakeMessage(actor, Conjugate(actor, VERB_ENJOY), ent));
      int boreChance = ent.Model.BoreChance;
      if (boreChance == 100) {
        actor.Inventory.Consume(ent);
        if (player) AddMessage(MakeMessage(actor, Conjugate(actor, VERB_DISCARD), ent));
      } else if (m_Rules.RollChance(boreChance)) {
        actor.AddBoringItem(ent);
        if (player) AddMessage(MakeMessage(actor, string.Format("{0} now bored of {1}.", Conjugate(actor, VERB_BE), ent.TheName)));
      }
    }

    public void DoRechargeItemBattery(Actor actor, Item it)
    {
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      (it as BatteryPowered).Recharge();
      if (!ForceVisibleToPlayer(actor)) return;
      AddMessage(MakeMessage(actor, Conjugate(actor, VERB_RECHARGE), it, " batteries."));
    }

    public void DoOpenDoor(Actor actor, DoorWindow door)
    {
      door.SetState(DoorWindow.STATE_OPEN);
      if (ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(door)) {
        AddMessage(MakeMessage(actor, Conjugate(actor, VERB_OPEN), door));
        RedrawPlayScreen();
      }
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public void DoCloseDoor(Actor actor, DoorWindow door)
    {
      door.SetState(DoorWindow.STATE_CLOSED);
      if (ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(door)) {
        AddMessage(MakeMessage(actor, Conjugate(actor, VERB_CLOSE), door));
        RedrawPlayScreen();
      }
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
    }

    public void DoBarricadeDoor(Actor actor, DoorWindow door)
    {
      ItemBarricadeMaterial barricadeMaterial = actor.Inventory.GetFirst<ItemBarricadeMaterial>();
      actor.Inventory.Consume(barricadeMaterial);
      door.Barricade(Rules.ActorBarricadingPoints(actor, barricadeMaterial.Model.BarricadingValue));
      if (ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(door))
        AddMessage(MakeMessage(actor, Conjugate(actor, VERB_BARRICADE), door));
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
    }

    public void DoBuildFortification(Actor actor, Point buildPos, bool isLarge)
    {
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      int num = Rules.ActorBarricadingMaterialNeedForFortification(actor, isLarge);
      for (int index = 0; index < num; ++index) {
        ItemBarricadeMaterial firstByType = actor.Inventory.GetFirst<ItemBarricadeMaterial>();
        actor.Inventory.Consume(firstByType);
      }
      Fortification fortification = isLarge ? BaseMapGenerator.MakeObjLargeFortification() : BaseMapGenerator.MakeObjSmallFortification();
      actor.Location.Map.PlaceAt(fortification, buildPos);  // XXX cross-map fortification change target
      if (ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(new Location(actor.Location.Map, buildPos)))
        AddMessage(MakeMessage(actor, string.Format("{0} {1}.", Conjugate(actor, VERB_BUILD), fortification.AName)));
      CheckMapObjectTriggersTraps(actor.Location.Map, buildPos);
    }

    public void DoRepairFortification(Actor actor, Fortification fort)
    {
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      ItemBarricadeMaterial barricadeMaterial = actor.Inventory.GetFirst<ItemBarricadeMaterial>();
      if (barricadeMaterial == null) throw new InvalidOperationException("no material");
      actor.Inventory.Consume(barricadeMaterial);
      fort.HitPoints = Math.Min(fort.MaxHitPoints, fort.HitPoints + Rules.ActorBarricadingPoints(actor, barricadeMaterial.Model.BarricadingValue));
      if (!ForceVisibleToPlayer(actor) && !ForceVisibleToPlayer(fort)) return;
      AddMessage(MakeMessage(actor, Conjugate(actor, VERB_REPAIR), fort));
    }

    public void DoSwitchPowerGenerator(Actor actor, PowerGenerator powGen)
    {
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      powGen.TogglePower();
      if (ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(powGen))
        AddMessage(MakeMessage(actor, Conjugate(actor, VERB_SWITCH), powGen, powGen.IsOn ? " on." : " off."));
      OnMapPowerGeneratorSwitch(actor.Location);
    }

    private void DoDestroyObject(MapObject mapObj)
    {
      DoorWindow doorWindow = mapObj as DoorWindow;
      bool flag = doorWindow != null && doorWindow.IsWindow;
      mapObj.HitPoints = 0;
      if (mapObj.GivesWood) {
        int val2 = 1 + mapObj.MaxHitPoints / 40;
        while (val2 > 0) {
          ItemBarricadeMaterial barricadeMaterial = new ItemBarricadeMaterial(GameItems.WOODENPLANK) {
            Quantity = Math.Min(GameItems.WOODENPLANK.StackingLimit, val2)
          };
          val2 -= barricadeMaterial.Quantity;
          mapObj.Location.Map.DropItemAt(barricadeMaterial, mapObj.Location.Position);
        }
        if (m_Rules.RollChance(Rules.IMPROVED_WEAPONS_FROM_BROKEN_WOOD_CHANCE)) {
          ItemMeleeWeapon itemMeleeWeapon = !m_Rules.RollChance(50) ? new ItemMeleeWeapon(GameItems.IMPROVISED_SPEAR) : new ItemMeleeWeapon(GameItems.IMPROVISED_CLUB);
          mapObj.Location.Map.DropItemAt(itemMeleeWeapon, mapObj.Location.Position);
        }
      }
      if (flag)
        doorWindow.SetState(DoorWindow.STATE_BROKEN);
      else
        mapObj.Remove();
      OnLoudNoise(mapObj.Location.Map, mapObj.Location.Position, "A loud *CRASH*");
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public void DoBreak(Actor actor, MapObject mapObj)
    {
      // XXX NPCs know to use their best melee weapon
      // this doesn't handle martial artists properly
      if (!actor.IsPlayer) {
        ItemMeleeWeapon bestMeleeWeapon = actor.GetBestMeleeWeapon();
        if (null!=bestMeleeWeapon) {
          ItemMeleeWeapon equippedWeapon = actor.GetEquippedWeapon() as ItemMeleeWeapon;
          if (equippedWeapon != bestMeleeWeapon) DoEquipItem(actor, bestMeleeWeapon);
        }
      }
      Attack attack = actor.MeleeAttack();
      if (mapObj is DoorWindow doorWindow && doorWindow.IsBarricaded) {
        actor.SpendActionPoints(Rules.BASE_ACTION_COST);
        actor.SpendStaminaPoints(Rules.STAMINA_COST_MELEE_ATTACK);
        doorWindow.Barricade(-attack.DamageValue);
        OnLoudNoise(doorWindow.Location.Map, doorWindow.Location.Position, "A loud *BASH*");
        if (ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(doorWindow)) {
          AddMessage(MakeMessage(actor, string.Format("{0} the barricade.", Conjugate(actor, VERB_BASH))));
        } else {
          if (!m_Rules.RollChance(PLAYER_HEAR_BASH_CHANCE)) return;
          AddMessageIfAudibleForPlayer(doorWindow.Location, "You hear someone bashing barricades");
        }
      } else {
        mapObj.HitPoints -= attack.DamageValue;
        actor.SpendActionPoints(Rules.BASE_ACTION_COST);
        actor.SpendStaminaPoints(Rules.STAMINA_COST_MELEE_ATTACK);
        bool flag = false;
        if (mapObj.HitPoints <= 0) {
          DoDestroyObject(mapObj);
          flag = true;
        }
        OnLoudNoise(mapObj.Location.Map, mapObj.Location.Position, "A loud *CRASH*");
        bool player1 = ForceVisibleToPlayer(actor);
        bool player2 = player1 ? IsVisibleToPlayer(mapObj) : ForceVisibleToPlayer(mapObj);
        bool isPlayer = actor.IsPlayer;
        if (player1 || player2) {
          if (player1) AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(actor.Location), SIZE_OF_ACTOR)));
          if (player2) AddOverlay(new OverlayRect(Color.Red, new Rectangle(MapToScreen(mapObj.Location), SIZE_OF_TILE)));
          if (flag) {
            AddMessage(MakeMessage(actor, Conjugate(actor, VERB_BREAK), mapObj));
            if (player1) AddOverlay(new OverlayImage(MapToScreen(actor.Location), GameImages.ICON_MELEE_ATTACK));
            if (player2) AddOverlay(new OverlayImage(MapToScreen(mapObj.Location), GameImages.ICON_KILLED));
            RedrawPlayScreen();
            AnimDelay(DELAY_LONG);
          } else {
            AddMessage(MakeMessage(actor, Conjugate(actor, VERB_BASH), mapObj));
            if (player1)
              AddOverlay(new OverlayImage(MapToScreen(actor.Location), GameImages.ICON_MELEE_ATTACK));
            if (player2)
              AddOverlay(new OverlayImage(MapToScreen(mapObj.Location), GameImages.ICON_MELEE_DAMAGE));
            RedrawPlayScreen();
            AnimDelay(isPlayer ? DELAY_NORMAL : DELAY_SHORT);
          }
        } else if (flag) {
          if (m_Rules.RollChance(PLAYER_HEAR_BREAK_CHANCE))
            AddMessageIfAudibleForPlayer(mapObj.Location, "You hear someone breaking furniture");
        } else if (m_Rules.RollChance(PLAYER_HEAR_BASH_CHANCE))
          AddMessageIfAudibleForPlayer(mapObj.Location, "You hear someone bashing furniture");
        ClearOverlays();
      }
    }

    public void DoPush(Actor actor, MapObject mapObj, Point toPos)
    {
      bool flag = ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(mapObj);
      int staminaCost = mapObj.Weight;
      if (actor.CountFollowers > 0) {
        Location location = new Location(actor.Location.Map, mapObj.Location.Position);
        IEnumerable<Actor> tmp = actor.Followers.Where(follower=>!follower.IsSleeping && (follower.Activity == Activity.IDLE || follower.Activity == Activity.FOLLOWING) && Rules.IsAdjacent(follower.Location, mapObj.Location));
        if (tmp.Any()) {
          staminaCost = mapObj.Weight / (1 + tmp.Count());
          foreach(Actor actor1 in tmp) {
            actor1.SpendActionPoints(Rules.BASE_ACTION_COST);
            actor1.SpendStaminaPoints(staminaCost);
            if (flag)
              AddMessage(MakeMessage(actor1, string.Format("{0} {1} pushing {2}.", Conjugate(actor1, VERB_HELP), actor.Name, mapObj.TheName)));
          }
        }
      }
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      actor.SpendStaminaPoints(staminaCost);
      Location o_loc = mapObj.Location;
      o_loc.Map.PlaceAt(mapObj, toPos);  // XXX cross-map push target
      if (!Rules.IsAdjacent(o_loc, actor.Location) && o_loc.IsWalkableFor(actor)) {
        o_loc.Place(actor);
      }
      if (flag) {
        AddMessage(MakeMessage(actor, Conjugate(actor, VERB_PUSH), mapObj));
        RedrawPlayScreen();
      } else {
        OnLoudNoise(o_loc.Map, toPos, "Something being pushed");
        if (m_Rules.RollChance(PLAYER_HEAR_PUSH_CHANCE))
          AddMessageIfAudibleForPlayer(mapObj.Location, "You hear something being pushed");
      }
      CheckMapObjectTriggersTraps(o_loc.Map, toPos);
    }

    public void DoShove(Actor actor, Actor target, Point toPos)
    {
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      if (TryActorLeaveTile(target)) {
        actor.SpendStaminaPoints(Rules.DEFAULT_ACTOR_WEIGHT);
        DoStopDragCorpse(target);
        Location t_loc = target.Location;
        t_loc.Map.PlaceAt(target, toPos);    // XXX cross-map shove change target
        if (!Rules.IsAdjacent(t_loc, actor.Location) && t_loc.IsWalkableFor(actor)) {
          if (!TryActorLeaveTile(actor)) return;
          t_loc.Place(actor);
          OnActorEnterTile(actor);
        }
        if (ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(target) || ForceVisibleToPlayer(t_loc.Map, toPos)) {
          AddMessage(MakeMessage(actor, Conjugate(actor, VERB_SHOVE), target));
          RedrawPlayScreen();
        }
        if (target.IsSleeping) DoWakeUp(target);
        OnActorEnterTile(target);
      }
    }

    public void DoStartSleeping(Actor actor)
    {
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      DoStopDragCorpse(actor);
      actor.Activity = Activity.SLEEPING;
      actor.IsSleeping = true;
    }

    public void DoWakeUp(Actor actor)
    {
      actor.Activity = Activity.IDLE;
      actor.IsSleeping = false;
      if (ForceVisibleToPlayer(actor))
      AddMessage(MakeMessage(actor, string.Format("{0}.", Conjugate(actor, VERB_WAKE_UP))));
      if (!actor.IsPlayer) return;
      m_MusicManager.StopAll();
    }

    private void DoTag(Actor actor, ItemSprayPaint spray, Point pos)
    {
      actor.SpendActionPoints(Rules.BASE_ACTION_COST);
      --spray.PaintQuantity;
      actor.Location.Map.AddDecorationAt(spray.Model.TagImageID,pos);
      if (!ForceVisibleToPlayer(actor)) return;
      AddMessage(MakeMessage(actor, string.Format("{0} a tag.", Conjugate(actor, VERB_SPRAY))));
    }

    private void DoGiveOrderTo(Actor master, Actor slave, ActorOrder order)
    {
      master.SpendActionPoints(Rules.BASE_ACTION_COST);
      if (master != slave.Leader)
        DoSay(slave, master, "Who are you to give me orders?", RogueGame.Sayflags.IS_FREE_ACTION);
      else if (!slave.IsTrustingLeader) {
        DoSay(slave, master, "Sorry, I don't trust you enough yet.", RogueGame.Sayflags.IS_IMPORTANT | RogueGame.Sayflags.IS_FREE_ACTION);
      } else {
        OrderableAI aiController = slave.Controller as OrderableAI;
        if (aiController == null) return;
        aiController.SetOrder(order);
        if (!ForceVisibleToPlayer(master) && !ForceVisibleToPlayer(slave)) return;
        AddMessage(MakeMessage(master, Conjugate(master, VERB_ORDER), slave, string.Format(" to {0}.", order.ToString())));
      }
    }

    private void DoCancelOrder(Actor master, Actor slave)
    {
      master.SpendActionPoints(Rules.BASE_ACTION_COST);
      OrderableAI aiController = slave.Controller as OrderableAI;
      if (aiController == null) return;
      aiController.SetOrder(null);
      if (!ForceVisibleToPlayer(master) && !ForceVisibleToPlayer(slave)) return;
      AddMessage(MakeMessage(master, Conjugate(master, VERB_ORDER), slave, " to forget its orders."));
    }

    private void OnLoudNoise(Map map, Point noisePosition, string noiseName)
    {   // Note: Loud noise radius is hard-coded as 5 grid distance; empirically audio range is 0/16 Euclidean distance
      Rectangle survey = new Rectangle(noisePosition.X - Rules.LOUD_NOISE_RADIUS, noisePosition.Y - Rules.LOUD_NOISE_RADIUS, 2* Rules.LOUD_NOISE_RADIUS + 1, 2 * Rules.LOUD_NOISE_RADIUS + 1);
      map.TrimToBounds(ref survey);

      Actor actorAt = null;
      survey.DoForEach(pt => {
        DoWakeUp(actorAt);
        if (ForceVisibleToPlayer(actorAt)) {
          AddMessage(new Data.Message(string.Format("{0} wakes {1} up!", noiseName, actorAt.TheName), map.LocalTime.TurnCounter, actorAt == m_Player ? Color.Red : Color.White));
          RedrawPlayScreen();
        }
      }, pt => {
        actorAt = map.GetActorAt(pt);
        if (!actorAt?.IsSleeping ?? true) return false;
        int noiseDistance = Rules.GridDistance(noisePosition, pt);
        return /* noiseDistance <= Rules.LOUD_NOISE_RADIUS && */ m_Rules.RollChance(Rules.ActorLoudNoiseWakeupChance(actorAt, noiseDistance));  // would need to test for other kinds of distance
      });
#if SUICIDE_BY_LONG_WAIT
      if (!m_IsPlayerLongWait || (map != m_Player.Location.Map || !ForceVisibleToPlayer(map, noisePosition))) return;
      m_IsPlayerLongWaitForcedStop = true;
#endif
    }

    private void InflictDamage(Actor actor, int dmg)
    {
      actor.HitPoints -= dmg;
      if (actor.Model.Abilities.CanTire) actor.StaminaPoints -= dmg;
      Item equippedItem = actor.GetEquippedItem(DollPart.TORSO);
      if (equippedItem != null && equippedItem is ItemBodyArmor && m_Rules.RollChance(Rules.BODY_ARMOR_BREAK_CHANCE)) {
        actor.OnUnequipItem(equippedItem);
        actor.Inventory.RemoveAllQuantity(equippedItem);
        if (ForceVisibleToPlayer(actor)) {
          AddMessage(MakeMessage(actor, string.Format(": {0} breaks and is now useless!", equippedItem.TheName)));
          RedrawPlayScreen();
          AnimDelay(actor.IsPlayer ? DELAY_NORMAL : DELAY_SHORT);
        }
      }
      if (actor.IsSleeping) DoWakeUp(actor);
    }

    public void KillActor(Actor killer, Actor deadGuy, string reason)
    {
#if DEBUG
      // for some reason, this can happen with starved actors (cf. alpha 8)
      if (deadGuy.IsDead)
        throw new InvalidOperationException(String.Format("killing deadGuy that is already dead : killer={0} deadGuy={1} reason={2}", (killer == null ? "N/A" : killer.TheName), deadGuy.TheName, reason));
#endif
      Actor m_Player_bak = m_Player;    // ForceVisibleToPlayer calls below can change this

      deadGuy.IsDead = true;
	  deadGuy.Killed(reason);
      DoStopDragCorpse(deadGuy);
      UntriggerAllTrapsHere(deadGuy.Location);
      if (killer != null && !killer.Model.Abilities.IsUndead && (killer.Model.Abilities.HasSanity && deadGuy.Model.Abilities.IsUndead))
        killer.RegenSanity(Rules.ActorSanRegenValue(killer, Rules.SANITY_RECOVER_KILL_UNDEAD));
      if (deadGuy.HasLeader) {
        if (deadGuy.Leader.HasBondWith(deadGuy)) {
          deadGuy.Leader.SpendSanity(Rules.SANITY_HIT_BOND_DEATH);
          if (ForceVisibleToPlayer(deadGuy.Leader)) {
            if (deadGuy.Leader.IsPlayer) ClearMessages();
            AddMessage(MakeMessage(deadGuy.Leader, string.Format("{0} deeply disturbed by {1} sudden death!", Conjugate(deadGuy.Leader, VERB_BE), deadGuy.Name)));
            if (deadGuy.Leader.IsPlayer) AddMessagePressEnter();
          }
        }
      } else if (deadGuy.CountFollowers > 0) {
        foreach (Actor follower in deadGuy.Followers) {
          if (follower.HasBondWith(deadGuy)) {
            follower.SpendSanity(Rules.SANITY_HIT_BOND_DEATH);
            if (ForceVisibleToPlayer(follower)) {
              if (follower.IsPlayer) ClearMessages();
              AddMessage(MakeMessage(follower, string.Format("{0} deeply disturbed by {1} sudden death!", Conjugate(follower, VERB_BE), deadGuy.Name)));
              if (follower.IsPlayer) AddMessagePressEnter();
            }
          }
        }
      }
      if (deadGuy.IsUnique) {
        Session.Get.Scoring.AddEvent(deadGuy.Location.Map.LocalTime.TurnCounter,
            (killer != null
           ? string.Format("* {0} was killed by {1} {2}! *", deadGuy.TheName, killer.Model.Name, killer.TheName)
           : string.Format("* {0} died by {1}! *", deadGuy.TheName, reason)));
      }
      if (deadGuy == m_Player_bak) {
        m_Player = m_Player_bak;
        m_Player.Location.Map.RecalcPlayers();
        if (0 >= Session.Get.World.PlayerCount) PlayerDied(killer, reason);
      }
      deadGuy.RemoveAllFollowers();
      if (deadGuy.Leader != null) {
        if (deadGuy.Leader.IsPlayer) {
          string text = killer == null ? string.Format("Follower {0} died by {1}!", deadGuy.TheName, reason) : string.Format("Follower {0} was killed by {1} {2}!", deadGuy.TheName, killer.Model.Name, killer.TheName);
          Session.Get.Scoring.AddEvent(deadGuy.Location.Map.LocalTime.TurnCounter, text);
        }
        deadGuy.Leader.RemoveFollower(deadGuy);
      }
      deadGuy.RemoveAllAgressorSelfDefenceRelations();
      deadGuy.Location.Map.Remove(deadGuy);
      if (deadGuy.Inventory != null && !deadGuy.Inventory.IsEmpty) {
        // the implicit police radio goes explicit on death, as a generic item
        if ((int) GameFactions.IDs.ThePolice == deadGuy.Faction.ID && m_Rules.RollChance(Rules.VICTIM_DROP_GENERIC_ITEM_CHANCE)) {
          deadGuy.Location.Map.DropItemAt(BaseMapGenerator.MakeItemPoliceRadio(), deadGuy.Location.Position);
        }
        Item[] objArray = deadGuy.Inventory.Items.ToArray();
        foreach (Item it in objArray) {
          int chance = (it is ItemAmmo || it is ItemFood) ? Rules.VICTIM_DROP_AMMOFOOD_ITEM_CHANCE : Rules.VICTIM_DROP_GENERIC_ITEM_CHANCE;
          if (it.Model.IsUnbreakable || it.IsUnique || m_Rules.RollChance(chance))
            DropItem(deadGuy, it);
        }
        Session.Get.PoliceInvestigate.Record(deadGuy.Location.Map,deadGuy.Location.Position);  // cheating ai: police consider death drops tourism targets
      }

      if (!deadGuy.Model.Abilities.IsUndead) SplatterBlood(deadGuy.Location.Map, deadGuy.Location.Position);
#if FAIL
      else UndeadRemains(deadGuy.Location.Map, deadGuy.Location.Position);
#endif
      if (Session.Get.HasCorpses && !deadGuy.Model.Abilities.IsUndead) DropCorpse(deadGuy);
      if (killer != null) ++killer.KillsCount;
      if (killer == m_Player) PlayerKill(deadGuy);
      if (killer != null && Session.Get.HasEvolution && killer.Model.Abilities.IsUndead) {
        ActorModel actorModel = CheckUndeadEvolution(killer);
        if (actorModel != null) {
          SkillTable skillTable = null;
          if (killer.Sheet.SkillTable != null && killer.Sheet.SkillTable.Skills != null)
            skillTable = new SkillTable(killer.Sheet.SkillTable.Skills);
          killer.Model = actorModel;
          if (killer.IsPlayer) killer.PrepareForPlayerControl();
          if (skillTable != null) {
            foreach (Skill skill in skillTable.Skills) {
              for (int index = 0; index < skill.Level; ++index) {
                killer.SkillUpgrade(skill.ID);
              }
            }
            killer.RecomputeStartingStats();
          }
          if (ForceVisibleToPlayer(killer)) {
            AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(killer.Location), SIZE_OF_ACTOR)));
            AddMessage(MakeMessage(killer, string.Format("{0} a {1} horror!", Conjugate(killer, VERB_TRANSFORM_INTO), actorModel.Name)));
            RedrawPlayScreen();
            AnimDelay(DELAY_LONG);
            ClearOverlays();
          }
        }
      }
      if (killer != null && killer.CountFollowers > 0) {
        foreach (Actor follower in killer.Followers) {
          if (follower.TargetActor == deadGuy || follower.IsEnemyOf(deadGuy) && Rules.IsAdjacent(follower.Location, deadGuy.Location)) {
            DoSay(follower, killer, "That was close! Thanks for the help!!", RogueGame.Sayflags.IS_FREE_ACTION);
            ModifyActorTrustInLeader(follower, Rules.TRUST_LEADER_KILL_ENEMY, true);
          }
        }
      }
      if (killer != null && Rules.IsMurder(killer, deadGuy)) {
        ++killer.MurdersCounter;
        if (killer.IsPlayer)
          Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format("Murdered {0} a {1}!", deadGuy.TheName, deadGuy.Model.Name));
        if (IsVisibleToPlayer(killer))
          AddMessage(MakeMessage(killer, string.Format("murdered {0}!!", deadGuy.Name)));
        Map map = killer.Location.Map;
        Point position = killer.Location.Position;
        foreach (Actor actor in map.Actors) {
          if (actor.Model.Abilities.IsLawEnforcer && !actor.IsDead && (!actor.IsSleeping && !actor.IsPlayer) && (actor != killer && actor != deadGuy && (actor.Leader != killer && killer.Leader != actor)) && (Rules.GridDistance(actor.Location.Position, position) <= actor.FOVrange(map.LocalTime, Session.Get.World.Weather) && LOS.CanTraceViewLine(actor.Location, position)))
          {
            DoSay(actor, killer, string.Format("MURDER! {0} HAS KILLED {1}!", killer.TheName, deadGuy.TheName), RogueGame.Sayflags.IS_IMPORTANT | RogueGame.Sayflags.IS_FREE_ACTION);
            DoMakeAggression(actor, killer);
          }
        }
      }
      // XXX also a model for army radio, etc. usage
      if (killer != null && killer.Model.Abilities.IsLawEnforcer && (killer.Faction.IsEnemyOf(deadGuy.Faction) || deadGuy.MurdersCounter > 0)) {
        if (!killer.Faction.IsEnemyOf(deadGuy.Faction) /* &&  deadGuy.MurdersCounter > 0 */) {
          if (killer.IsPlayer)
            AddMessage(new Data.Message("You feel like you did your duty with killing a murderer.", Session.Get.WorldTime.TurnCounter, Color.White));
          else
            DoSay(killer, deadGuy, "Good riddance, murderer!", RogueGame.Sayflags.IS_FREE_ACTION);
        }

        if (!killer.IsPlayer)
          killer.MessagePlayerOnce(a => {
            int turnCounter = Session.Get.WorldTime.TurnCounter;
            // possible verbs: killed, terminated, erased, downed, wasted.
            AddMessage(new Data.Message(string.Format("(police radio, {0}) {1} killed.", killer.Name, deadGuy.Name), turnCounter, Color.White));
          }, a => {
            if (a.IsSleeping) return false;
            if (!a.HasActivePoliceRadio) return false;
            return true;
          });
      }

      deadGuy.TargetActor = null; // savefile scanner said this wasn't covered.  Other fields targeted by Actor::OptimizeBeforeSaving are covered.
      // this doesn't *look* safe, but it turns out we never have a foreach loop on the actors list when calling KillActor.
      deadGuy.Location.Map.Remove(deadGuy);

      // achievement: killing the Sewers Thing
      if (killer == m_Player && killer.Leader == m_Player) {
        if (deadGuy == Session.Get.UniqueActors.TheSewersThing.TheActor) ShowNewAchievement(Achievement.IDs.KILLED_THE_SEWERS_THING);
      }

      // If m_Player has just died, then we should be in the current district and thus clear to find a player
      // furthermore, the viewport didn't pan away to another player
      if (deadGuy == m_Player) FindPlayer();
    }

    private ActorModel CheckUndeadEvolution(Actor undead)
    {
      if (!s_Options.AllowUndeadsEvolution || !Session.Get.HasEvolution) return null;
	  // anything not whitelisted to evolve, doesn't
      switch (undead.Model.ID)
      {
        case GameActors.IDs.UNDEAD_SKELETON:
          if (undead.KillsCount < 2) return null;
          break;
        case GameActors.IDs.UNDEAD_RED_EYED_SKELETON:
          if (undead.KillsCount < 4) return null;
          break;
        case GameActors.IDs.UNDEAD_ZOMBIE:
        case GameActors.IDs.UNDEAD_DARK_EYED_ZOMBIE:
          break;
        case GameActors.IDs.UNDEAD_ZOMBIE_MASTER:
          if (undead.KillsCount < 4) return null;
          if (undead.Location.Map.LocalTime.Day < ZOMBIE_LORD_EVOLUTION_MIN_DAY && !undead.IsPlayer)
            return null;
          break;
        case GameActors.IDs.UNDEAD_ZOMBIE_LORD:
          if (undead.KillsCount < 8) return null;
          break;
        case GameActors.IDs.UNDEAD_MALE_ZOMBIFIED:
        case GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED:
          if (undead.KillsCount < 2) return null;
          break;
        case GameActors.IDs.UNDEAD_MALE_NEOPHYTE:
        case GameActors.IDs.UNDEAD_FEMALE_NEOPHYTE:
          if (undead.KillsCount < 4) return null;
          if (undead.Location.Map.LocalTime.Day < DISCIPLE_EVOLUTION_MIN_DAY && !undead.IsPlayer)
            return null;
          break;
        default:
          return null;
      }
      GameActors.IDs index = undead.Model.ID.NextUndeadEvolution();
	  return (index != undead.Model.ID ? GameActors[index] : null);
    }

    public void SplatterBlood(Map map, Point position)
    {
      Tile tileAt1 = map.GetTileAt(position);
      if (tileAt1.Model.IsWalkable && !tileAt1.HasDecoration(GameImages.DECO_BLOODIED_FLOOR)) {
        tileAt1.AddDecoration(GameImages.DECO_BLOODIED_FLOOR);
        map.AddTimer(new TaskRemoveDecoration(WorldTime.TURNS_PER_DAY, position.X, position.Y, GameImages.DECO_BLOODIED_FLOOR));
      }
      map.ForEachAdjacent(position,(p => {
        if (!m_Rules.RollChance(20)) return;
        Tile tileAt2 = map.GetTileAt(p);
        if (!tileAt2.Model.IsWalkable && !tileAt2.HasDecoration(GameImages.DECO_BLOODIED_WALL)) {
          tileAt2.AddDecoration(GameImages.DECO_BLOODIED_WALL);
          map.AddTimer(new TaskRemoveDecoration(WorldTime.TURNS_PER_DAY, p.X, p.Y, GameImages.DECO_BLOODIED_WALL));
        }
      }));
    }

#if DEAD_FUNC
    public void UndeadRemains(Map map, Point position)
    {
      Tile tileAt = map.GetTileAt(position);
//    if (!map.IsWalkable(position.X, position.Y) || tileAt.HasDecoration(GameImages.DECO_ZOMBIE_REMAINS)) return;
      tileAt.AddDecoration(GameImages.DECO_ZOMBIE_REMAINS);
      map.AddTimer(new TaskRemoveDecoration(WorldTime.TURNS_PER_DAY, position.X, position.Y, GameImages.DECO_ZOMBIE_REMAINS));
    }
#endif

    public void DropCorpse(Actor deadGuy)
    {
      deadGuy.Doll.AddDecoration(DollPart.TORSO, GameImages.BLOODIED);
      float rotation = m_Rules.Roll(30, 60);
      if (m_Rules.RollChance(50)) rotation = -rotation;
      deadGuy.Location.Map.AddAt(new Corpse(deadGuy, rotation), deadGuy.Location.Position);
    }

    private void PlayerDied(Actor killer, string reason)
    {
      StopSimThread();
      m_UI.UI_SetCursor(null);
      m_MusicManager.StopAll();
      m_MusicManager.Play(GameMusics.PLAYER_DEATH);
      Session.Get.Scoring.TurnsSurvived = Session.Get.WorldTime.TurnCounter;
      Session.Get.Scoring.SetKiller(killer);
      if (m_Player.CountFollowers > 0) {
        foreach (Actor follower in m_Player.Followers)
          Session.Get.Scoring.AddFollowerWhenDied(follower);
      }
      List<Zone> zonesAt = m_Player.Location.Map.GetZonesAt(m_Player.Location.Position);
      Session.Get.Scoring.DeathPlace = zonesAt != null ? string.Format("{0} at {1}", m_Player.Location.Map.Name, zonesAt[0].Name) : m_Player.Location.Map.Name;
      Session.Get.Scoring.DeathReason = killer == null ? string.Format("Death by {0}", reason) : string.Format("{0} by {1} {2}", Rules.IsMurder(killer, m_Player) ? "Murdered" : "Killed", killer.Model.Name, killer.TheName);
      Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, "Died.");
      int index = m_Rules.Roll(0, GameTips.TIPS.Length);
      AddOverlay(new OverlayPopup(new string[3]
      {
        "TIP OF THE DEAD",
        "Did you know that...",
        GameTips.TIPS[index]
      }, Color.White, Color.White, POPUP_FILLCOLOR, new Point(0, 0)));
      ClearMessages();
      AddMessage(new Data.Message("**** YOU DIED! ****", Session.Get.WorldTime.TurnCounter, Color.Red));
      if (killer != null)
        AddMessage(new Data.Message(string.Format("Killer : {0}.", killer.TheName), Session.Get.WorldTime.TurnCounter, Color.Red));
      AddMessage(new Data.Message(string.Format("Reason : {0}.", reason), Session.Get.WorldTime.TurnCounter, Color.Red));
      if (m_Player.Model.Abilities.IsUndead)
        AddMessage(new Data.Message("You die one last time... Game over!", Session.Get.WorldTime.TurnCounter, Color.Red));
      else
        AddMessage(new Data.Message("You join the realm of the undeads... Game over!", Session.Get.WorldTime.TurnCounter, Color.Red));
      if (s_Options.IsPermadeathOn)
        DeleteSavedGame(RogueGame.GetUserSave());
      if (s_Options.IsDeathScreenshotOn) {
        RedrawPlayScreen();
        string screenshot = DoTakeScreenshot();
        if (screenshot == null)
          AddMessage(MakeErrorMessage("could not save death screenshot."));
        else
          AddMessage(new Data.Message(string.Format("Death screenshot saved : {0}.", screenshot), Session.Get.WorldTime.TurnCounter, Color.Red));
      }
      AddMessagePressEnter();
      HandlePostMortem();
      m_MusicManager.StopAll();
    }

    static private string TimeSpanToString(TimeSpan rt)
    {
      return string.Format("{0}{1}{2}{3}", rt.Days == 0 ? "" : string.Format("{0} days ", (object)rt.Days), rt.Hours == 0 ? "" : string.Format("{0:D2} hours ", (object)rt.Hours), rt.Minutes == 0 ? "" : string.Format("{0:D2} minutes ", (object)rt.Minutes), rt.Seconds == 0 ? "" : string.Format("{0:D2} seconds", (object)rt.Seconds));
    }

    private void HandlePostMortem()
    {
      WorldTime worldTime = new WorldTime(Session.Get.Scoring.TurnsSurvived);
      string str1 = HisOrHer(m_Player);
      string str2 = HimOrHer(m_Player);
      string name = m_Player.TheName.Replace("(YOU) ", "");
      string @string = TimeSpanToString(Session.Get.Scoring.RealLifePlayingTime);
      Session.Get.Scoring.Side = m_Player.Model.Abilities.IsUndead ? DifficultySide.FOR_UNDEAD : DifficultySide.FOR_SURVIVOR;
      Session.Get.Scoring.DifficultyRating = Scoring.ComputeDifficultyRating(s_Options, Session.Get.Scoring.Side, Session.Get.Scoring.ReincarnationNumber);
      TextFile textFile = new TextFile();
      textFile.Append(SetupConfig.GAME_NAME_CAPS+" "+SetupConfig.GAME_VERSION);
      textFile.Append("POST MORTEM");
      textFile.Append(string.Format("{0} was {1} and {2}.", name, m_Player.Model.Name.PrefixIndefiniteSingularArticle(), m_Player.Faction.MemberName.PrefixIndefiniteSingularArticle()));
      textFile.Append(string.Format("{0} survived to see {1}.", str1, worldTime.ToString()));
      textFile.Append(string.Format("{0}'s spirit guided {1} for {2}.", name, str2, @string));
      if (Session.Get.Scoring.ReincarnationNumber > 0)
        textFile.Append(string.Format("{0} was reincarnation {1}.", str1, Session.Get.Scoring.ReincarnationNumber));
      textFile.Append(" ");
      textFile.Append("> SCORING");
      textFile.Append(string.Format("{0} scored a total of {1} points.", str1, Session.Get.Scoring.TotalPoints));
      textFile.Append(string.Format("- difficulty rating of {0}%.", (int)(100.0 * (double)Session.Get.Scoring.DifficultyRating)));
      textFile.Append(string.Format("- {0} base points for survival.", Session.Get.Scoring.SurvivalPoints));
      textFile.Append(string.Format("- {0} base points for kills.", Session.Get.Scoring.KillPoints));
      textFile.Append(string.Format("- {0} base points for achievements.", Session.Get.Scoring.AchievementPoints));
      textFile.Append(" ");
      textFile.Append("> ACHIEVEMENTS");
      foreach (Achievement achievement in Session.Get.Scoring.Achievements) {
        if (achievement.IsDone)
          textFile.Append(string.Format("- {0} for {1} points!", achievement.Name, achievement.ScoreValue));
        else
          textFile.Append(string.Format("- Fail : {0}.", achievement.TeaseName));
      }
      if (Session.Get.Scoring.CompletedAchievementsCount == 0) {
        textFile.Append("Didn't achieve anything notable. And then died.");
        textFile.Append(string.Format("(unlock all the {0} achievements to win this game version)", 8));
      } else {
        textFile.Append(string.Format("Total : {0}/{1}.", Session.Get.Scoring.CompletedAchievementsCount, (int)Achievement.IDs._COUNT));
        if (Session.Get.Scoring.CompletedAchievementsCount >= (int)Achievement.IDs._COUNT)
          textFile.Append("*** You achieved everything! You can consider having won this version of the game! CONGRATULATIONS! ***");
        else
          textFile.Append("(unlock all the achievements to win this game version)");
        textFile.Append("(later versions of the game will feature real winning conditions and multiple endings...)");
      }
      textFile.Append(" ");
      textFile.Append("> DEATH");
      textFile.Append(string.Format("{0} in {1}.", Session.Get.Scoring.DeathReason, Session.Get.Scoring.DeathPlace));
      textFile.Append(" ");
      textFile.Append("> KILLS");
      if (Session.Get.Scoring.HasNoKills) {
        textFile.Append(string.Format("{0} was a pacifist. Or too scared to fight.", str1));
      } else {
        foreach (Scoring.KillData kill in Session.Get.Scoring.Kills) {
          string str3 = kill.Amount > 1 ? Models.Actors[(int)kill.ActorModelID].PluralName : Models.Actors[(int)kill.ActorModelID].Name;
          textFile.Append(string.Format("{0,4} {1}.", kill.Amount, str3));
        }
      }
      if (!m_Player.Model.Abilities.IsUndead && m_Player.MurdersCounter > 0)
        textFile.Append(string.Format("{0} committed {1}!", str1, "murder".QtyDesc(m_Player.MurdersCounter)));
      textFile.Append(" ");
      textFile.Append("> FUN FACTS!");
      textFile.Append(string.Format("While {0} has died, others are still having fun!", name));
      foreach (string compileDistrictFunFact in CompileDistrictFunFacts(m_Player.Location.Map.District))
        textFile.Append(compileDistrictFunFact);
      textFile.Append("");
      textFile.Append("> SKILLS");
      if (m_Player.Sheet.SkillTable.Skills == null) {
        textFile.Append(string.Format("{0} was a jack of all trades. Or an incompetent.", str1));
      } else {
        foreach (Skill skill in m_Player.Sheet.SkillTable.Skills)
          textFile.Append(string.Format("{0}-{1}.", skill.Level, Skills.Name(skill.ID)));
      }
      textFile.Append(" ");
      textFile.Append("> INVENTORY");
      if (null==m_Player.Inventory || m_Player.Inventory.IsEmpty) {
        textFile.Append(string.Format("{0} was humble. Or dirt poor.", str1));
      } else {
        foreach (Item it in m_Player.Inventory.Items) {
          string str3 = DescribeItemShort(it);
          if (it.IsEquipped)
            textFile.Append(string.Format("- {0} (equipped).", str3));
          else
            textFile.Append(string.Format("- {0}.", str3));
        }
      }
      textFile.Append(" ");
      textFile.Append("> FOLLOWERS");
      if (Session.Get.Scoring.FollowersWhendDied == null || Session.Get.Scoring.FollowersWhendDied.Count == 0) {
        textFile.Append(string.Format("{0} was doing fine alone. Or everyone else was dead.", str1));
      } else {
        StringBuilder stringBuilder = new StringBuilder(string.Format("{0} was leading", str1));
        bool flag = true;
        int num = 0;
        int count = Session.Get.Scoring.FollowersWhendDied.Count;
        foreach (Actor actor in Session.Get.Scoring.FollowersWhendDied) {
          if (flag) stringBuilder.Append(" ");
          else if (num == count) stringBuilder.Append(".");
          else if (num == count - 1) stringBuilder.Append(" and ");
          else stringBuilder.Append(", ");
          stringBuilder.Append(actor.TheName);
          ++num;
          flag = false;
        }
        stringBuilder.Append(".");
        textFile.Append(stringBuilder.ToString());
        foreach (Actor actor in Session.Get.Scoring.FollowersWhendDied) {
          textFile.Append(string.Format("{0} skills : ", actor.Name));
          if (actor.Sheet.SkillTable != null && actor.Sheet.SkillTable.Skills != null) {
            foreach (Skill skill in actor.Sheet.SkillTable.Skills)
              textFile.Append(string.Format("{0}-{1}.", skill.Level, Skills.Name(skill.ID)));
          }
        }
      }
      textFile.Append(" ");
      textFile.Append("> EVENTS");
      if (Session.Get.Scoring.HasNoEvents) {
        textFile.Append(string.Format("{0} had a quiet life. Or dull and boring.", str1));
      } else {
        foreach (Scoring.GameEventData @event in Session.Get.Scoring.Events)
          textFile.Append(string.Format("- {0,13} : {1}", new WorldTime(@event.Turn).ToString(), @event.Text));
      }
      textFile.Append(" ");
      textFile.Append("> CUSTOM OPTIONS");
      textFile.Append(string.Format("- difficulty rating of {0}%.", (int)(100.0 * (double)Session.Get.Scoring.DifficultyRating)));
      if (s_Options.IsPermadeathOn)
        textFile.Append(string.Format("- {0} : yes.", GameOptions.Name(GameOptions.IDs.GAME_PERMADEATH)));
      if (!s_Options.AllowUndeadsEvolution && Session.Get.HasEvolution)
        textFile.Append(string.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION), s_Options.AllowUndeadsEvolution ? "yes" : "no"));
      if (s_Options.CitySize != GameOptions.DEFAULT_CITY_SIZE)
        textFile.Append(string.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_CITY_SIZE), s_Options.CitySize));
      if (s_Options.DayZeroUndeadsPercent != GameOptions.DEFAULT_DAY_ZERO_UNDEADS_PERCENT)
        textFile.Append(string.Format("- {0} : {1}%.", GameOptions.Name(GameOptions.IDs.GAME_DAY_ZERO_UNDEADS_PERCENT), s_Options.DayZeroUndeadsPercent));
      if (s_Options.DistrictSize != GameOptions.DEFAULT_DISTRICT_SIZE)
        textFile.Append(string.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_DISTRICT_SIZE), s_Options.DistrictSize));
      if (s_Options.MaxCivilians != GameOptions.DEFAULT_MAX_CIVILIANS)
        textFile.Append(string.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_MAX_CIVILIANS), s_Options.MaxCivilians));
      if (s_Options.MaxUndeads != GameOptions.DEFAULT_MAX_UNDEADS)
        textFile.Append(string.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_MAX_UNDEADS), s_Options.MaxUndeads));
      if (!s_Options.NPCCanStarveToDeath)
        textFile.Append(string.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_NPC_CAN_STARVE_TO_DEATH), s_Options.NPCCanStarveToDeath ? "yes" : "no"));
      if (s_Options.StarvedZombificationChance != GameOptions.DEFAULT_STARVED_ZOMBIFICATION_CHANCE)
        textFile.Append(string.Format("- {0} : {1}%.", GameOptions.Name(GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE), s_Options.StarvedZombificationChance));
      if (!s_Options.RevealStartingDistrict)
        textFile.Append(string.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_REVEAL_STARTING_DISTRICT), s_Options.RevealStartingDistrict ? "yes" : "no"));
      textFile.Append(string.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_SIMULATE_SLEEP), "no [hardcoded]"));
      if (s_Options.ZombieInvasionDailyIncrease != GameOptions.DEFAULT_ZOMBIE_INVASION_DAILY_INCREASE)
        textFile.Append(string.Format("- {0} : {1}%.", GameOptions.Name(GameOptions.IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE), s_Options.ZombieInvasionDailyIncrease));
      if (s_Options.ZombificationChance != GameOptions.DEFAULT_ZOMBIFICATION_CHANCE)
        textFile.Append(string.Format("- {0} : {1}%.", GameOptions.Name(GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE), s_Options.ZombificationChance));
      if (s_Options.MaxReincarnations != GameOptions.DEFAULT_MAX_REINCARNATIONS)
        textFile.Append(string.Format("- {0} : {1}.", GameOptions.Name(GameOptions.IDs.GAME_MAX_REINCARNATIONS), s_Options.MaxReincarnations));
      textFile.Append(" ");
      textFile.Append("> R.I.P");
      textFile.Append(string.Format("May {0} soul rest in peace.", HisOrHer(m_Player)));
      textFile.Append(string.Format("For {0} body is now a meal for evil.", HisOrHer(m_Player)));
      textFile.Append("The End.");
      int num1;
      int num2 = num1 = 0;
      m_UI.UI_Clear(Color.Black);
      m_UI.UI_DrawStringBold(Color.Yellow, "Saving post mortem to graveyard...", 0, 0, new Color?());
      int gy1 = num1 + 14;
      m_UI.UI_Repaint();
      string str4 = RogueGame.GraveFilePath(GetUserNewGraveyardName());
      if (!textFile.Save(str4)) {
        m_UI.UI_DrawStringBold(Color.Red, "Could not save to graveyard.", 0, gy1, new Color?());
      } else {
        m_UI.UI_DrawStringBold(Color.Yellow, "Grave saved to :", 0, gy1, new Color?());
        int gy2 = gy1 + 14;
        m_UI.UI_DrawString(Color.White, str4, 0, gy2, new Color?());
      }
      DrawFootnote(Color.White, "press ENTER");
      m_UI.UI_Repaint();
      WaitEnter();
      textFile.FormatLines(TEXTFILE_CHARS_PER_LINE);
      int index = 0;
      do {
        m_UI.UI_Clear(Color.Black);
        DrawHeader();
        int gy2 = 14;
        int num5 = 0;
        m_UI.UI_DrawStringBold(Color.White, hr_plus, 0, gy2, new Color?());
        gy2 += 14;
        for (; num5 < 50 && index < textFile.FormatedLines.Count; ++num5) {
          m_UI.UI_DrawStringBold(Color.White, textFile.FormatedLines[index], 0, gy2, new Color?());
          gy2 += 14;
          ++index;
        }
        m_UI.UI_DrawStringBold(Color.White, hr_plus, 0, 740, new Color?());
        if (index < textFile.FormatedLines.Count)
          DrawFootnote(Color.White, "press ENTER for more");
        else
          DrawFootnote(Color.White, "press ENTER to leave");
        m_UI.UI_Repaint();
        WaitEnter();
      }
      while (index < textFile.FormatedLines.Count);
      StringBuilder stringBuilder1 = new StringBuilder();
      if (m_Player.Sheet.SkillTable.Skills != null) {
        foreach (Skill skill in m_Player.Sheet.SkillTable.Skills)
          stringBuilder1.AppendFormat("{0}-{1} ", skill.Level, Skills.Name(skill.ID));
      }
      if (!m_HiScoreTable.Register(HiScore.FromScoring(name, Session.Get.Scoring, stringBuilder1.ToString()))) return;
      SaveHiScoreTable();
      HandleHiScores(true);
    }

    private void OnNewNight()
    {
      m_Player.Controller.UpdateSensors();
      if (!m_Player.Model.Abilities.IsUndead) return;
	  // Proboards leonelhenry: PC should be on the same options as undead
	  if (s_Options.ZombifiedsUpgradeDays == GameOptions.ZupDays.OFF || !GameOptions.IsZupDay(s_Options.ZombifiedsUpgradeDays, m_Player.Location.Map.LocalTime.Day)) return;
      if ((GameMode.GM_VINTAGE == Session.Get.GameMode || !s_Options.SkeletonsUpgrade) && GameActors.IsSkeletonBranch(m_Player.Model)) return;
      if ((GameMode.GM_VINTAGE == Session.Get.GameMode || !s_Options.RatsUpgrade) && GameActors.IsRatBranch(m_Player.Model)) return;
      if ((GameMode.GM_VINTAGE == Session.Get.GameMode || !s_Options.ShamblersUpgrade) && GameActors.IsShamblerBranch(m_Player.Model)) return;
      ClearOverlays();
      AddOverlay(new RogueGame.OverlayPopup(UPGRADE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, Point.Empty));
      m_MusicManager.StopAll();
      m_MusicManager.Play(GameMusics.INTERLUDE);
      ClearMessages();
      AddMessage(new Data.Message("You will hunt another day!", Session.Get.WorldTime.TurnCounter, Color.Green));
      m_Player.Controller.UpdateSensors();
      AddMessagePressEnter();
//    HandlePlayerDecideUpgrade(m_Player);    // XXX skill upgrade timing problems with non-following PCs
      ClearMessages();
      AddMessage(new Data.Message("Welcome to the night.", Session.Get.WorldTime.TurnCounter, Color.White));
      ClearOverlays();
      RedrawPlayScreen();
      m_MusicManager.StopAll();
    }

    private void OnNewDay()
    {
      if (!m_Player.Model.Abilities.IsUndead) {
        ClearOverlays();
        AddOverlay(new RogueGame.OverlayPopup(UPGRADE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, Point.Empty));
        m_MusicManager.StopAll();
        m_MusicManager.Play(GameMusics.INTERLUDE);
        ClearMessages();
        AddMessage(new Data.Message("You survived another night!", Session.Get.WorldTime.TurnCounter, Color.Green));
        m_Player.Controller.UpdateSensors();
        AddMessagePressEnter();
//      HandlePlayerDecideUpgrade(m_Player);    // XXX skill upgrade timing problems with non-following PCs
        ClearMessages();
        AddMessage(new Data.Message("Welcome to tomorrow.", Session.Get.WorldTime.TurnCounter, Color.White));
        ClearOverlays();
        RedrawPlayScreen();
        m_MusicManager.StopAll();
      }
      CheckWeatherChange();
      if (m_Player.Model.Abilities.IsUndead) return;
      if (Session.Get.WorldTime.Day == 7) ShowNewAchievement(Achievement.IDs.REACHED_DAY_07);
      else if (Session.Get.WorldTime.Day == 14) ShowNewAchievement(Achievement.IDs.REACHED_DAY_14);
      else if (Session.Get.WorldTime.Day == 21) ShowNewAchievement(Achievement.IDs.REACHED_DAY_21);
      else if (Session.Get.WorldTime.Day == 28) ShowNewAchievement(Achievement.IDs.REACHED_DAY_28);
    }

    private void HandlePlayerDecideUpgrade(Actor upgradeActor)
    {
      List<Skills.IDs> upgrade = RollSkillsToUpgrade(upgradeActor, 300);
      string str = upgradeActor == m_Player ? "You" : upgradeActor.Name;
      bool flag = true;
      do {
        ClearMessages();
        AddMessage(new Data.Message(str + " can improve or learn one of these skills. Choose wisely.", Session.Get.WorldTime.TurnCounter, Color.Green));
        if (upgrade.Count == 0) {
          AddMessage(MakeErrorMessage(str + " can't learn anything new!"));
        } else {
          for (int index = 0; index < upgrade.Count; ++index) {
            Skills.IDs id = upgrade[index];
            int skillLevel = upgradeActor.Sheet.SkillTable.GetSkillLevel(id);
            AddMessage(new Data.Message(string.Format("choice {0} : {1} from {2} to {3} - {4}", index + 1, Skills.Name(id), skillLevel, skillLevel + 1, DescribeSkillShort(id)), Session.Get.WorldTime.TurnCounter, Color.LightGreen));
          }
        }
        AddMessage(new Data.Message("ESC if you don't want to upgrade; SPACE to get wiser skills.", Session.Get.WorldTime.TurnCounter, Color.White));
        RedrawPlayScreen();
        KeyEventArgs key = m_UI.UI_WaitKey();
        int num = (int) InputTranslator.KeyToCommand(key);
        if (key.KeyCode == Keys.Escape) break;
        if (key.KeyCode == Keys.Space) {
          upgrade = RollSkillsToUpgrade(upgradeActor, 300);
          continue;
        }

        int choiceNumber = KeyToChoiceNumber(key.KeyCode);
        if (choiceNumber >= 1 && choiceNumber <= upgrade.Count) {
          Skill skill = upgradeActor.SkillUpgrade(upgrade[choiceNumber - 1]);
		  string msg = (1 == skill.Level ? string.Format("{0} learned skill {1}.", upgradeActor.Name, Skills.Name(skill.ID))
					 : string.Format("{0} improved skill {1} to level {2}.", upgradeActor.Name, Skills.Name(skill.ID), skill.Level));
          AddMessage(new Data.Message(msg, Session.Get.WorldTime.TurnCounter, Color.LightGreen));
          Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, msg);
          AddMessagePressEnter();
          flag = false;
        }
      }
      while (flag);
      // this is the change target for becoming a cop.  The test may need extracting to an ImpersonateCop function
      // 0) must be civilian or survivor with 0 murders
      if ((GameFactions.TheCivilians == upgradeActor.Faction || GameFactions.TheSurvivors == upgradeActor.Faction)
           // 1) required skills: Firearms 1, Leadership 1
           && 1 <= upgradeActor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.FIREARMS) && 1 <= upgradeActor.Sheet.SkillTable.GetSkillLevel(Skills.IDs.LEADERSHIP)
           // 2) must have equipped: police radio, police armor
           && null != upgradeActor.GetEquippedItem(GameItems.IDs.TRACKER_POLICE_RADIO)
           && (null != upgradeActor.GetEquippedItem(GameItems.IDs.ARMOR_POLICE_JACKET) || null != upgradeActor.GetEquippedItem(GameItems.IDs.ARMOR_POLICE_RIOT))    // XXX should just check good police armors list
           // 3) must have in inventory: one of pistol or shotgun
           && (null != upgradeActor.GetItem(GameItems.IDs.RANGED_PISTOL) || null != upgradeActor.GetItem(GameItems.IDs.RANGED_SHOTGUN))
           // 4) must have committed no murders
           && 0 >= upgradeActor.MurdersCounter) {
        // then: y/n prompt, if y become cop
        AddMessage(MakeYesNoMessage("Become a cop"));
        RedrawPlayScreen();
        if (WaitYesOrNo()) {
          upgradeActor.Faction = GameFactions.ThePolice;
          DiscardItem(upgradeActor, upgradeActor.GetEquippedItem(GameItems.IDs.TRACKER_POLICE_RADIO));    // now implicit; don't worry about efficiency here
          upgradeActor.PrefixName("Cop"); // adjust job title
          upgradeActor.Doll.AddDecoration(DollPart.HEAD, GameImages.POLICE_HAT); // XXX should selectively remove clothes when re-clothing
          upgradeActor.Doll.AddDecoration(DollPart.TORSO, GameImages.POLICE_UNIFORM);
          upgradeActor.Doll.AddDecoration(DollPart.LEGS, GameImages.POLICE_PANTS);
          upgradeActor.Doll.AddDecoration(DollPart.FEET, GameImages.POLICE_SHOES);
          upgradeActor.Model = Models.Actors[(int)(upgradeActor.Model.ID.IsFemale() ? GameActors.IDs.POLICEWOMAN : GameActors.IDs.POLICEMAN)];
          upgradeActor.Controller = new PlayerController();
          AddMessage(new Data.Message("Welcome to the force.", Session.Get.WorldTime.TurnCounter, Color.Yellow));
        } else
          AddMessage(new Data.Message("Acknowledged.", Session.Get.WorldTime.TurnCounter, Color.Yellow));
      }
      if (upgradeActor.IsPlayer) HandlePlayerFollowersUpgrade(upgradeActor);
    }

    private void HandlePlayerFollowersUpgrade(Actor player)
    {
      if (player.CountFollowers == 0) return;
      ClearMessages();
      AddMessage(new Data.Message("Your followers learned new skills at your side!", Session.Get.WorldTime.TurnCounter, Color.Green));
      AddMessagePressEnter();
      foreach (Actor follower in player.Followers)
        HandlePlayerDecideUpgrade(follower);
    }

    private void HandleLivingNPCsUpgrade(Map map)
    {
      foreach (Actor actor in map.Actors) {
        if (actor.Model.Abilities.IsUndead) continue;
        if (actor.HasLeader && actor.Leader.IsPlayer) continue; // leader triggers upgrade
        if (actor.IsPlayer) {
          HandlePlayerDecideUpgrade(actor);
          continue;
        }
        List<Skills.IDs> upgrade1 = RollSkillsToUpgrade(actor, 300);
        Skills.IDs? upgrade2 = NPCPickSkillToUpgrade(actor, upgrade1);
        if (upgrade2.HasValue)
          actor.SkillUpgrade(upgrade2.Value);
      }
    }

    private void HandleUndeadNPCsUpgrade(Map map)
    {
      foreach (Actor actor in map.Actors) {
        if (!actor.Model.Abilities.IsUndead) continue;
        if ((GameMode.GM_VINTAGE == Session.Get.GameMode || !s_Options.SkeletonsUpgrade) && GameActors.IsSkeletonBranch(actor.Model)) continue;
        if ((GameMode.GM_VINTAGE == Session.Get.GameMode || !s_Options.RatsUpgrade) && GameActors.IsRatBranch(actor.Model)) continue;
        if ((GameMode.GM_VINTAGE == Session.Get.GameMode || !s_Options.ShamblersUpgrade) && GameActors.IsShamblerBranch(actor.Model)) continue;
        if (actor.HasLeader && actor.Leader.IsPlayer) continue; // leader triggers upgrade
        if (actor.IsPlayer) {
          HandlePlayerDecideUpgrade(actor);
          continue;
        }
        List<Skills.IDs> upgrade1 = RollSkillsToUpgrade(actor, 300);
        Skills.IDs? upgrade2 = NPCPickSkillToUpgrade(actor, upgrade1);
        if (upgrade2.HasValue)
          actor.SkillUpgrade(upgrade2.Value);
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
          upgrade = RollRandomSkillToUpgrade(actor, maxTries);
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
        numArray[index] = NPCSkillUtility(npc, chooseFrom[index]);
        if (numArray[index] > num)
          num = numArray[index];
      }
      List<Skills.IDs> idsList = new List<Skills.IDs>(count);
      for (int index = 0; index < count; ++index)
      {
        if (numArray[index] == num)
          idsList.Add(chooseFrom[index]);
      }
      return new Skills.IDs?(idsList[m_Rules.Roll(0, idsList.Count)]);
    }

    static private int NPCSkillUtility(Actor actor, Skills.IDs skID)
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
      Skills.IDs id;
      do {
        ++num;
        id = isUndead ? Skills.RollUndead(Rules.DiceRoller) : Skills.RollLiving(Rules.DiceRoller);
      }
      while (actor.Sheet.SkillTable.GetSkillLevel(id) >= Skills.MaxSkillLevel(id) && num < maxTries);
      if (num >= maxTries) return new Skills.IDs?();
      return new Skills.IDs?(id);
    }

    private void DoLooseRandomSkill(Actor actor)
    {
      int[] skillsList = actor.Sheet.SkillTable.SkillsList;
      if (skillsList == null) return;
      int index = m_Rules.Roll(0, skillsList.Length);
      Skills.IDs id = (Skills.IDs) skillsList[index];
      actor.Sheet.SkillTable.DecOrRemoveSkill(id);
      if (!ForceVisibleToPlayer(actor)) return;
      AddMessage(MakeMessage(actor, string.Format("regressed in {0}!", Skills.Name(id))));
    }

    private void CheckWeatherChange()
    {
      if (m_Rules.RollChance(33)) {
        AddMessage(new Data.Message(Session.Get.World.WeatherChanges(), Session.Get.WorldTime.TurnCounter, Color.White));
        Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format("The weather changed to {0}.", DescribeWeather(Session.Get.World.Weather)));
      } else
        AddMessage(new Data.Message("The weather stays the same.", Session.Get.WorldTime.TurnCounter, Color.White));
    }

    private void PlayerKill(Actor victim)
    {
      Session.Get.Scoring.AddKill(m_Player, victim, Session.Get.WorldTime.TurnCounter);
    }

    private Actor Zombify(Actor zombifier, Actor deadVictim, bool isStartingGame)
    {
      Actor actor = BaseTownGenerator.MakeZombified(zombifier, deadVictim, isStartingGame ? 0 : deadVictim.Location.Map.LocalTime.TurnCounter);
      if (!isStartingGame) deadVictim.Location.Place(actor);
      if (deadVictim == m_Player || deadVictim.IsPlayer) Session.Get.Scoring.SetZombifiedPlayer(actor);
      SkillTable skillTable = deadVictim.Sheet.SkillTable;
      if (skillTable != null && skillTable.CountSkills > 0) {
        int countSkills = skillTable.CountSkills;
        int num = skillTable.CountTotalSkillLevels / 2;
        for (int index = 0; index < num; ++index) {
          Skills.IDs? nullable = ((Skills.IDs) skillTable.SkillsList[m_Rules.Roll(0, countSkills)]).Zombify();
          if (nullable.HasValue)
            actor.SkillUpgrade(nullable.Value);
        }
        actor.RecomputeStartingStats();
      }
      if (!isStartingGame)
        SeeingCauseInsanity(actor, actor.Location, Rules.SANITY_HIT_ZOMBIFY, string.Format("{0} turning into a zombie", deadVictim.Name));
      return actor;
    }

    // These are closely coordinated with Session.Get.CurrentMap
    private void ComputeViewRect(Point mapCenter)
    {
      int x = mapCenter.X - HALF_VIEW_WIDTH;
      int y = mapCenter.Y - HALF_VIEW_HEIGHT;
      m_MapViewRect = new Rectangle(x, y, 1+2* HALF_VIEW_WIDTH, 1 + 2 * HALF_VIEW_HEIGHT);
    }

    private bool IsInViewRect(Point mapPosition)
    {
      return m_MapViewRect.Contains(mapPosition);
    }

    private bool IsInViewRect(Location loc)
    {
      if (loc.Map == Session.Get.CurrentMap) return m_MapViewRect.Contains(loc.Position);
      Location? tmp = Session.Get.CurrentMap.Denormalize(loc);
      if (null == tmp) return false;
      return m_MapViewRect.Contains(tmp.Value.Position);
    }

    static private ColorString WeatherStatusText()
    {
      switch(Session.Get.CurrentMap.Lighting)
      {
        case Lighting.DARKNESS: return new ColorString(Color.Blue,"Darkness");
        case Lighting.OUTSIDE: return new ColorString(WeatherColor(Session.Get.World.Weather),DescribeWeather(Session.Get.World.Weather));
        case Lighting.LIT: return new ColorString(Color.Yellow,"Lit");
        default: throw new ArgumentOutOfRangeException("unhandled lighting");
      }
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public void RedrawPlayScreen()
    {
            lock (m_UI) {
                m_UI.UI_Clear(Color.Black);
                m_UI.UI_DrawLine(Color.DarkGray, 676, 0, 676, 676);
                DrawMap(Session.Get.CurrentMap);
                m_UI.UI_DrawLine(Color.DarkGray, 676, 471, CANVAS_WIDTH, 471);
                DrawMiniMap(Session.Get.CurrentMap);
                m_UI.UI_DrawLine(Color.DarkGray, 4, 675, CANVAS_WIDTH, 675);
                DrawMessages();
                m_UI.UI_DrawLine(Color.DarkGray, 676, 676, 676, 768);
                m_UI.UI_DrawString(Color.White, Session.Get.CurrentMap.Name, 680, 680, new Color?());
                m_UI.UI_DrawString(Color.White, LocationText(Session.Get.CurrentMap, m_Player), 680, 692, new Color?());
                m_UI.UI_DrawString(Color.White, string.Format("Day  {0}", Session.Get.WorldTime.Day), 680, 704, new Color?());
                m_UI.UI_DrawString(Color.White, string.Format("Hour {0}", Session.Get.WorldTime.Hour), 680, 716, new Color?());
                m_UI.UI_DrawString(Session.Get.WorldTime.IsNight ? NIGHT_COLOR : DAY_COLOR, DescribeDayPhase(Session.Get.WorldTime.Phase), 808, 704, new Color?());
                m_UI.UI_DrawString(WeatherStatusText(), 808, 716);
                m_UI.UI_DrawString(Color.White, string.Format("Turn {0}", Session.Get.WorldTime.TurnCounter), 680, 728);
                m_UI.UI_DrawString(Color.White, string.Format("Score   {0}@{1}% {2}", Session.Get.Scoring.TotalPoints, (int)(100.0 * (double)Scoring.ComputeDifficultyRating(s_Options, Session.Get.Scoring.Side, Session.Get.Scoring.ReincarnationNumber)), Session.DescShortGameMode(Session.Get.GameMode)), 808, 728);
                m_UI.UI_DrawString(Color.White, string.Format("Avatar  {0}/{1}", 1 + Session.Get.Scoring.ReincarnationNumber, 1 + s_Options.MaxReincarnations), 808, 740);
                if (null != m_Player) {
                  if (m_Player.MurdersCounter > 0)
                    m_UI.UI_DrawString(Color.White, string.Format("Murders {0}", m_Player.MurdersCounter), 808, 752);
                  DrawActorStatus(m_Player, 680, 4);
                  if (m_Player.Inventory != null && m_Player.Model.Abilities.HasInventory)
                    DrawInventory(m_Player.Inventory, "Inventory", true, Map.GROUND_INVENTORY_SLOTS, m_Player.Inventory.MaxCapacity, INVENTORYPANEL_X, INVENTORYPANEL_Y);
                  DrawInventory(m_Player.Location.Items, "Items on ground", true, Map.GROUND_INVENTORY_SLOTS, Map.GROUND_INVENTORY_SLOTS, INVENTORYPANEL_X, GROUNDINVENTORYPANEL_Y);
                  DrawCorpsesList(m_Player.Location.Map.GetCorpsesAt(m_Player.Location.Position), "Corpses on ground", Map.GROUND_INVENTORY_SLOTS, INVENTORYPANEL_X, CORPSESPANEL_Y);
                  if (m_Player.Sheet.SkillTable != null && m_Player.Sheet.SkillTable.CountSkills > 0)
                    DrawActorSkillTable(m_Player, 680, 352);
                }
                lock (m_Overlays) {
                  foreach (Overlay mOverlay in m_Overlays)
                    mOverlay.Draw(m_UI);
                }
                m_UI.UI_Repaint();
            };  // lock(m_UI)
    }

    static private string LocationText(Map map, Actor actor)
    {
      if (map == null || actor == null) return "";
      StringBuilder stringBuilder = new StringBuilder(string.Format("({0},{1}) ", actor.Location.Position.X, actor.Location.Position.Y));
      List<Zone> zonesAt = map.GetZonesAt(actor.Location.Position);
      if (null == zonesAt) return stringBuilder.ToString();
      foreach (Zone zone in zonesAt)
        stringBuilder.Append(string.Format("{0} ", zone.Name));
      return stringBuilder.ToString();
    }

 /*
  * Intentionally disabled function in RS alpha 9
    private Color TintForDayPhase(DayPhase phase)
    {
      switch (phase)
      {
        case DayPhase.SUNSET:
          return TINT_SUNSET;
        case DayPhase.EVENING:
          return TINT_EVENING;
        case DayPhase.MIDNIGHT:
          return TINT_MIDNIGHT;
        case DayPhase.DEEP_NIGHT:
          return TINT_NIGHT;
        case DayPhase.SUNRISE:
          return TINT_SUNRISE;
        case DayPhase.MORNING:
        case DayPhase.MIDDAY:
        case DayPhase.AFTERNOON:
          return TINT_DAY;
        default:
          throw new ArgumentOutOfRangeException("unhandled dayphase");
      }
    }

    public void DrawMap(Map map, Color tint)
    {
*/
    public void DrawMap(Map map)    // XXX not at all clear why this and the functions it controls are public
    {
      Color tint = Color.White; // disabled changing brightness bad for the eyes TintForDayPhase(m_Session.WorldTime.Phase);
#if NO_PEACE_WALLS
      int num1 = m_MapViewRect.Left;
      int num2 = m_MapViewRect.Right;
      int num3 = m_MapViewRect.Top;
      int num4 = m_MapViewRect.Bottom;
#else
      int num1 = Math.Max(-1, m_MapViewRect.Left);
      int num2 = Math.Min(map.Width + 1, m_MapViewRect.Right);
      int num3 = Math.Max(-1, m_MapViewRect.Top);
      int num4 = Math.Min(map.Height + 1, m_MapViewRect.Bottom);
#endif
      string imageID;
      switch (Session.Get.World.Weather) {
        case Weather.RAIN:
          imageID = Session.Get.WorldTime.TurnCounter % 2 == 0 ? GameImages.WEATHER_RAIN1 : GameImages.WEATHER_RAIN2;
          break;
        case Weather.HEAVY_RAIN:
          imageID = Session.Get.WorldTime.TurnCounter % 2 == 0 ? GameImages.WEATHER_HEAVY_RAIN1 : GameImages.WEATHER_HEAVY_RAIN2;
          break;
        default:
          imageID = null;
          break;
      }

	  ThreatTracking threats = m_Player.Threats;    // these two should agree on whether they're null or not
      LocationSet sights_to_see = m_Player.InterestingLocs;

      // as drawing is slow, we should be able to get away with thrashing the garbage collector here
      // XXX these two aren't cross-district
      HashSet<Point> tainted = threats?.ThreatWhere(map) ?? new HashSet<Point>();
      HashSet<Point> tourism = sights_to_see?.In(map) ?? new HashSet<Point>();

      Point point = new Point();
      bool isUndead = m_Player.Model.Abilities.IsUndead;
      bool flag1 = m_Player.Model.StartingSheet.BaseSmellRating > 0;
      for (int x = num1; x < num2; ++x) {
        point.X = x;
        for (int y = num3; y < num4; ++y) {
          point.Y = y;
#if NO_PEACE_WALLS
          if (!map.IsValid(x, y)) continue;
#endif
          Point screen = MapToScreen(x, y);
          bool player = IsVisibleToPlayer(map, point);
          bool flag2 = false;
#if NO_PEACE_WALLS
          Tile tile = map.GetTileAtExt(x, y);   // non-null for valid coordinates by construction
#else
          Tile tile = map.IsValid(x, y) ? map.GetTileAtExt(x, y) : null;
          if (null != tile) {
#endif
            tile.IsInView = player;
            tile.IsVisited = m_Player.Controller.IsKnown(new Location(map,point));
            DrawTile(tile, screen, tint);
            if (tainted.Contains(point)) {
              if (tourism.Contains(point)) {
                m_UI.UI_DrawImage(GameImages.THREAT_AND_TOURISM_OVERLAY, screen.X, screen.Y, tint);
              } else {
                m_UI.UI_DrawImage(GameImages.THREAT_OVERLAY, screen.X, screen.Y, tint);
              }
            } else if (tourism.Contains(point)) {
              m_UI.UI_DrawImage(GameImages.TOURISM_OVERLAY, screen.X, screen.Y, tint);
            }
#if NO_PEACE_WALLS
#else
          } else if (map.IsMapBoundary(x, y)) {
            Exit tmp = map.GetExitAt(point);
            if (null!=tmp && string.IsNullOrEmpty(tmp.ReasonIsBlocked(m_Player)))
              DrawExit(screen);
          }
#endif
          if (player) {
            // XXX should be visible only if underlying AI sees corpses
            List<Corpse> corpsesAt = map.GetCorpsesAtExt(x, y);
            if (corpsesAt != null) {
              foreach (Corpse c in corpsesAt)
                DrawCorpse(c, screen.X, screen.Y, tint);
            }
          }
          if (s_Options.ShowPlayerTargets && !m_Player.IsSleeping && m_Player.Location.Position == point)
            DrawPlayerActorTargets(m_Player);
          MapObject mapObjectAt = map.GetMapObjectAtExt(x, y);
          if (mapObjectAt != null) {
            DrawMapObject(mapObjectAt, screen, tile, tint);
            flag2 = true;
          }
#if NO_PEACE_WALLS
          if (!m_Player.IsSleeping && Rules.GridDistance(m_Player.Location.Position, point) <= 1) {    // grid distance 1 is always valid with cross-district visibility
#else
          if (!m_Player.IsSleeping && map.IsValid(x, y) && Rules.GridDistance(m_Player.Location.Position, point) <= 1) {    // XXX optimize when no peace walls
#endif
            if (isUndead) {
              if (flag1) {
                int num5 = m_Player.SmellThreshold;
                int scentByOdorAt1 = map.GetScentByOdorAt(Odor.LIVING, point);
                if (scentByOdorAt1 >= num5) {
                  float num6 = (float) (0.9 * scentByOdorAt1 / OdorScent.MAX_STRENGTH);
                  m_UI.UI_DrawTransparentImage(num6 * num6, GameImages.ICON_SCENT_LIVING, screen.X, screen.Y);
                }
                int scentByOdorAt2 = map.GetScentByOdorAt(Odor.UNDEAD_MASTER, point);
                if (scentByOdorAt2 >= num5) {
                  float num6 = (float) (0.9 * scentByOdorAt2 / OdorScent.MAX_STRENGTH);
                  m_UI.UI_DrawTransparentImage(num6 * num6, GameImages.ICON_SCENT_ZOMBIEMASTER, screen.X, screen.Y);
                }
              }
            } else {
              int scentByOdorAt = map.GetScentByOdorAt(Odor.PERFUME_LIVING_SUPRESSOR, point);
              if (scentByOdorAt > 0)
                m_UI.UI_DrawTransparentImage((float) (0.9 * scentByOdorAt / OdorScent.MAX_STRENGTH), GameImages.ICON_SCENT_LIVING_SUPRESSOR, screen.X, screen.Y);
            }
          }
          if (player) {
            // XXX the two AIs that don't see items but do have inventory, are feral dogs and the insane human ai.
            Inventory itemsAt = map.GetItemsAtExt(x, y);
            if (itemsAt != null) {
              DrawItemsStack(itemsAt, screen, tint);
              flag2 = true;
            }
            Actor actorAt = map.GetActorAtExt(x, y);
            if (actorAt != null) {
              DrawActorSprite(actorAt, screen, tint);
              flag2 = true;
            }
          }
#if NO_PEACE_WALLS
          if (tile.HasDecorations) flag2 = true;
#else
          if (tile != null && tile.HasDecorations) flag2 = true;
#endif
          if (flag2 && tile.Model.IsWater) DrawTileWaterCover(tile, screen, tint);
#if NO_PEACE_WALLS
          if (player && imageID != null && !tile.IsInside)
#else
          if (player && imageID != null && (tile != null && !tile.IsInside))
#endif
            m_UI.UI_DrawImage(imageID, screen.X, screen.Y);
        }
      }
    }

    static private string MovingWaterImage(TileModel model, int turnCount)
    {
      if (model != GameTiles.FLOOR_SEWER_WATER) return null;
      switch (turnCount % 3) {
        case 0: return GameImages.TILE_FLOOR_SEWER_WATER_ANIM1;
        case 1: return GameImages.TILE_FLOOR_SEWER_WATER_ANIM2;
        default: return GameImages.TILE_FLOOR_SEWER_WATER_ANIM3;
      }
    }

    public void DrawTile(Tile tile, Point screen, Color tint)
    {
      if (tile.IsInView) {
        m_UI.UI_DrawImage(tile.Model.ImageID, screen.X, screen.Y, tint);
        string imageID = MovingWaterImage(tile.Model, Session.Get.WorldTime.TurnCounter);
        if (imageID != null)
          m_UI.UI_DrawImage(imageID, screen.X, screen.Y, tint);
        if (!tile.HasDecorations) return;
        foreach (string decoration in tile.Decorations)
          m_UI.UI_DrawImage(decoration, screen.X, screen.Y, tint);
      } else {
        if (!tile.IsVisited || IsPlayerSleeping()) return;
        m_UI.UI_DrawGrayLevelImage(tile.Model.ImageID, screen.X, screen.Y);
        string imageID = MovingWaterImage(tile.Model, Session.Get.WorldTime.TurnCounter);
        if (imageID != null)
          m_UI.UI_DrawGrayLevelImage(imageID, screen.X, screen.Y);
        if (!tile.HasDecorations) return;
        foreach (string decoration in tile.Decorations)
          m_UI.UI_DrawGrayLevelImage(decoration, screen.X, screen.Y);
      }
    }

    public void DrawTileWaterCover(Tile tile, Point screen, Color tint)
    {
      if (tile.IsInView) {
        m_UI.UI_DrawImage(tile.Model.WaterCoverImageID, screen.X, screen.Y, tint);
      } else if (tile.IsVisited && !IsPlayerSleeping()) {
        m_UI.UI_DrawGrayLevelImage(tile.Model.WaterCoverImageID, screen.X, screen.Y);
      }
    }

    public void DrawExit(Point screen)
    {
      m_UI.UI_DrawImage(GameImages.MAP_EXIT, screen.X, screen.Y);
    }

    public void DrawTileRectangle(Point mapPosition, Color color)
    {
      m_UI.UI_DrawRect(color, new Rectangle(MapToScreen(mapPosition), new Size(TILE_SIZE, TILE_SIZE)));
    }

    public void DrawMapObject(MapObject mapObj, Point screen, Tile tile, Color tint)    // tile is the one that the map object is on.
    {
      if (mapObj.IsMovable && tile.Model.IsWater) {
        int num = (mapObj.Location.Position.X + Session.Get.WorldTime.TurnCounter) % 2 == 0 ? -2 : 0;
        screen.Y -= num;
      }
      if (tile.IsInView) {
        DrawMapObject(mapObj, screen, mapObj.ImageID, (imageID, gx, gy) => m_UI.UI_DrawImage(imageID, gx, gy, tint));
        if (mapObj.HitPoints < mapObj.MaxHitPoints && mapObj.HitPoints > 0)
          DrawMapHealthBar(mapObj.HitPoints, mapObj.MaxHitPoints, screen.X, screen.Y);
        DoorWindow doorWindow = mapObj as DoorWindow;
        if (doorWindow == null || doorWindow.BarricadePoints <= 0) return;
        DrawMapHealthBar(doorWindow.BarricadePoints, Rules.BARRICADING_MAX, screen.X, screen.Y, Color.Green);
        m_UI.UI_DrawImage(GameImages.EFFECT_BARRICADED, screen.X, screen.Y, tint);
      } else if (tile.IsVisited && !IsPlayerSleeping()) {
        DrawMapObject(mapObj, screen, mapObj.HiddenImageID, (imageID, gx, gy) => m_UI.UI_DrawGrayLevelImage(imageID, gx, gy));
      }
    }

    static private void DrawMapObject(MapObject mapObj, Point screen, string imageID, Action<string, int, int> drawFn)
    {
#if DEBUG
      if (null == drawFn) throw new ArgumentNullException(nameof(drawFn));
#endif
      drawFn(imageID, screen.X, screen.Y);
      if (mapObj.IsOnFire) drawFn(GameImages.EFFECT_ONFIRE, screen.X, screen.Y);
    }

    static private string FollowerIcon(Actor actor)
    {
      if (actor.HasBondWith(actor.Leader)) return GameImages.PLAYER_FOLLOWER_BOND;
      if (actor.IsTrustingLeader) return GameImages.PLAYER_FOLLOWER_TRUST;
      return GameImages.PLAYER_FOLLOWER;
    }

    private string ThreatIcon(Actor actor)
    {
      int threat_actions = m_Player.HowManyTimesOtherActs(1, actor);
      if (1 > threat_actions) return GameImages.ICON_THREAT_SAFE;
      if (1 < threat_actions) return GameImages.ICON_THREAT_HIGH_DANGER;
      return GameImages.ICON_THREAT_DANGER;
    }

    public void DrawActorSprite(Actor actor, Point screen, Color tint)
    {
      int x = screen.X;
      int y = screen.Y;
      if (actor.Leader != null && actor.Leader == m_Player) {
        m_UI.UI_DrawImage(FollowerIcon(actor), x, y, tint);
      }
      int gx1 = x;
      int gy1 = y;
      if (actor.Model.ImageID != null) m_UI.UI_DrawImage(actor.Model.ImageID, gx1, gy1, tint);
      else {
        // XXX would check sprite cache here
        DrawActorDecoration(actor, DollPart.SKIN, tint);
        DrawActorDecoration(actor, DollPart.FEET, tint);
        DrawActorDecoration(actor, DollPart.LEGS, tint);
        DrawActorDecoration(actor, DollPart.TORSO, tint);
        DrawActorEquipment(actor, DollPart.TORSO, tint);
        DrawActorDecoration(actor, DollPart.EYES, tint);
        DrawActorDecoration(actor, DollPart.HEAD, tint);
        DrawActorEquipment(actor, DollPart.LEFT_HAND, tint);
        DrawActorEquipment(actor, DollPart.RIGHT_HAND, tint);
        m_UI.DrawTile(gx1, gy1);    // would hand off to sprite cache here
      }
      int gx2 = gx1;
      int gy2 = gy1;
      if (m_Player != null) {
        if (m_Player.IsSelfDefenceFrom(actor))
          m_UI.UI_DrawImage(GameImages.ICON_SELF_DEFENCE, gx2, gy2, tint);
        else if (m_Player.IsAggressorOf(actor))
          m_UI.UI_DrawImage(GameImages.ICON_AGGRESSOR, gx2, gy2, tint);
        else if (m_Player.AreIndirectEnemies(actor))
          m_UI.UI_DrawImage(GameImages.ICON_INDIRECT_ENEMIES, gx2, gy2, tint);
      }
      switch (actor.Activity) {
        case Activity.IDLE:
          int maxHitPoints = actor.MaxHPs;
          if (actor.HitPoints < maxHitPoints) DrawMapHealthBar(actor.HitPoints, maxHitPoints, gx2, gy2);
          if (actor.IsRunning) m_UI.UI_DrawImage(GameImages.ICON_RUNNING, gx2, gy2, tint);
          else if (actor.Model.Abilities.CanRun && !actor.CanRun()) m_UI.UI_DrawImage(GameImages.ICON_CANT_RUN, gx2, gy2, tint);
          if (actor.Model.Abilities.HasToSleep) {
            if (actor.IsExhausted) m_UI.UI_DrawImage(GameImages.ICON_SLEEP_EXHAUSTED, gx2, gy2, tint);
            else if (actor.IsSleepy) m_UI.UI_DrawImage(GameImages.ICON_SLEEP_SLEEPY, gx2, gy2, tint);
            else if (actor.IsAlmostSleepy) m_UI.UI_DrawImage(GameImages.ICON_SLEEP_ALMOST_SLEEPY, gx2, gy2, tint);
          }
          if (actor.Model.Abilities.HasToEat) {
            if (actor.IsStarving) m_UI.UI_DrawImage(GameImages.ICON_FOOD_STARVING, gx2, gy2, tint);
            else if (actor.IsHungry) m_UI.UI_DrawImage(GameImages.ICON_FOOD_HUNGRY, gx2, gy2, tint);
            else if (actor.IsAlmostHungry) m_UI.UI_DrawImage(GameImages.ICON_FOOD_ALMOST_HUNGRY, gx2, gy2, tint);
          }
          else if (actor.Model.Abilities.IsRotting) {
            if (actor.IsRotStarving) m_UI.UI_DrawImage(GameImages.ICON_ROT_STARVING, gx2, gy2, tint);
            else if (actor.IsRotHungry) m_UI.UI_DrawImage(GameImages.ICON_ROT_HUNGRY, gx2, gy2, tint);
            else if (actor.IsAlmostRotHungry) m_UI.UI_DrawImage(GameImages.ICON_ROT_ALMOST_HUNGRY, gx2, gy2, tint);
          }
          if (actor.Model.Abilities.HasSanity) {
            if (actor.IsInsane) m_UI.UI_DrawImage(GameImages.ICON_SANITY_INSANE, gx2, gy2, tint);
            else if (actor.IsDisturbed) m_UI.UI_DrawImage(GameImages.ICON_SANITY_DISTURBED, gx2, gy2, tint);
          }
          if (m_Player != null && m_Player.CanTradeWith(actor)) m_UI.UI_DrawImage(GameImages.ICON_CAN_TRADE, gx2, gy2, tint);
          if (actor.IsSleeping && (actor.IsOnCouch || Rules.ActorHealChanceBonus(actor) > 0)) m_UI.UI_DrawImage(GameImages.ICON_HEALING, gx2, gy2, tint);
          if (actor.CountFollowers > 0) m_UI.UI_DrawImage(GameImages.ICON_LEADER, gx2, gy2, tint);
          if (!s_Options.IsCombatAssistantOn || actor == m_Player || (m_Player == null || !actor.IsEnemyOf(m_Player))) break;
          m_UI.UI_DrawImage(ThreatIcon(actor), gx2, gy2, tint);
          break;
        case Activity.CHASING:
        case Activity.FIGHTING:
          if (!actor.IsPlayer && null != actor.TargetActor) {
            m_UI.UI_DrawImage(((actor.TargetActor == m_Player) ? GameImages.ACTIVITY_CHASING_PLAYER : GameImages.ACTIVITY_CHASING), gx2, gy2, tint);
          }
          goto case Activity.IDLE;
        case Activity.TRACKING:
          if (!actor.IsPlayer) {
            m_UI.UI_DrawImage(GameImages.ACTIVITY_TRACKING, gx2, gy2, tint);
          }
          goto case Activity.IDLE;
        case Activity.FLEEING:
          if (!actor.IsPlayer) {
            m_UI.UI_DrawImage(GameImages.ACTIVITY_FLEEING, gx2, gy2, tint);
          }
          goto case Activity.IDLE;
        case Activity.FOLLOWING:
          if (!actor.IsPlayer && null != actor.TargetActor) {
            m_UI.UI_DrawImage((actor.TargetActor.IsPlayer ? GameImages.ACTIVITY_FOLLOWING_PLAYER : GameImages.ACTIVITY_FOLLOWING), gx2, gy2);
          }
          goto case Activity.IDLE;
        case Activity.SLEEPING:
          m_UI.UI_DrawImage(GameImages.ACTIVITY_SLEEPING, gx2, gy2);
          goto case Activity.IDLE;
        case Activity.FOLLOWING_ORDER:
          m_UI.UI_DrawImage(GameImages.ACTIVITY_FOLLOWING_ORDER, gx2, gy2);
          goto case Activity.IDLE;
        case Activity.FLEEING_FROM_EXPLOSIVE:
          if (!actor.IsPlayer) {
            m_UI.UI_DrawImage(GameImages.ACTIVITY_FLEEING_FROM_EXPLOSIVE, gx2, gy2, tint);
          }
          goto case Activity.IDLE;
        default:
          throw new InvalidOperationException("unhandled activity " + actor.Activity);
      }
    }

    public void DrawActorDecoration(Actor actor, DollPart part, Color tint)
    {
      List<string> decorations = actor.Doll.GetDecorations(part);
      if (decorations == null) return;
      foreach (string imageID in decorations)
      {// the skin is both guaranteed to be present when any decorations are present, and be unique
        if (DollPart.SKIN==part) {
          m_UI.AddTile(imageID, tint);
        } else {
          m_UI.AppendTile(imageID, tint);
        }
      }
    }

    public void DrawActorDecoration(Actor actor, int gx, int gy, DollPart part, float rotation, float scale)
    {
      List<string> decorations = actor.Doll.GetDecorations(part);
      if (decorations == null) return;
      foreach (string imageID in decorations)
        m_UI.UI_DrawImageTransform(imageID, gx, gy, rotation, scale);
    }

    public void DrawActorEquipment(Actor actor, DollPart part, Color tint)
    {
      Item equippedItem = actor.GetEquippedItem(part);
      if (equippedItem == null) return;
      m_UI.AppendTile(equippedItem.ImageID, tint);
    }

    public void DrawCorpse(Corpse c, int gx, int gy, Color tint)
    {
      float rotation = c.Rotation;
      float scale = c.Scale;
      Actor deadGuy = c.DeadGuy;
      if (deadGuy.Model.ImageID != null)
        m_UI.UI_DrawImageTransform(deadGuy.Model.ImageID, gx, gy, rotation, scale);
      else {
        DrawActorDecoration(deadGuy, gx, gy, DollPart.SKIN, rotation, scale);
        DrawActorDecoration(deadGuy, gx, gy, DollPart.FEET, rotation, scale);
        DrawActorDecoration(deadGuy, gx, gy, DollPart.LEGS, rotation, scale);
        DrawActorDecoration(deadGuy, gx, gy, DollPart.TORSO, rotation, scale);
        DrawActorDecoration(deadGuy, gx, gy, DollPart.TORSO, rotation, scale);
        DrawActorDecoration(deadGuy, gx, gy, DollPart.EYES, rotation, scale);
        DrawActorDecoration(deadGuy, gx, gy, DollPart.HEAD, rotation, scale);
      }
      int num2 = c.RotLevel;
      string str = null;
      switch (num2) {
        case 0:
          if (str == null) break;
          string imageID = str + 1 + Session.Get.WorldTime.TurnCounter % 2;
          int num3 = Session.Get.WorldTime.TurnCounter % 5 - 2;
          int num4 = Session.Get.WorldTime.TurnCounter / 3 % 5 - 2;
          m_UI.UI_DrawImage(imageID, gx + num3, gy + num4);
          break;
        case 1:
        case 2:
        case 3:
        case 4:
        case 5:
          str = "rot" + num2 + "_";
          goto case 0;
        default:
          throw new Exception("unhandled rot level");
      }
    }

    public void DrawCorpsesList(List<Corpse> list, string title, int slots, int gx, int gy)
    {
      int num2 = list == null ? 0 : list.Count;
      if (num2 > 0) title = title + " : " + num2;
      gy -= 14;
      m_UI.UI_DrawStringBold(Color.White, title, gx, gy, new Color?());
      gy += 14;
      int gx1 = gx;
      int gy1 = gy;
      for (int index = 0; index < slots; ++index) {
        m_UI.UI_DrawImage(GameImages.ITEM_SLOT, gx1, gy1);
        gx1 += TILE_SIZE;
      }
      if (list == null) return;
      int gx2 = gx;
      int gy2 = gy;
      int num3 = 0;
      foreach (Corpse c in list) {
        if (c.IsDragged) m_UI.UI_DrawImage(GameImages.CORPSE_DRAGGED, gx2, gy2);
        DrawCorpse(c, gx2, gy2, Color.White);
        if (++num3 >= slots) break;
        gx2 += TILE_SIZE;
      }
    }

    public void DrawActorTargets(Actor actor)
    {
      if (actor.TargetActor != null && !actor.TargetActor.IsDead && IsVisibleToPlayer(actor.TargetActor))
        AddOverlay(new OverlayImage(MapToScreen(actor.TargetActor.Location), GameImages.ICON_IS_TARGET));
      foreach (Actor actor1 in actor.Location.Map.Actors) {
        if (actor1 != actor && !actor1.IsDead && (IsVisibleToPlayer(actor1) && actor1.TargetActor == actor) && (actor1.Activity == Activity.CHASING || actor1.Activity == Activity.FIGHTING))
        {
          AddOverlay(new OverlayImage(MapToScreen(actor.Location), GameImages.ICON_IS_TARGETTED));
          break;
        }
      }
    }

    public void DrawPlayerActorTargets(Actor player)
    {
      if (null != player.Sees(player.TargetActor)) {
        Point screen = MapToScreen(player.TargetActor.Location);
        m_UI.UI_DrawImage(GameImages.ICON_IS_TARGET, screen.X, screen.Y);
      }
      foreach (Actor actor in player.Location.Map.Actors) {
        if (null != player.Sees(actor) && actor.TargetActor == player && (actor.Activity == Activity.CHASING || actor.Activity == Activity.FIGHTING)) {
          Point screen = MapToScreen(player.Location);
          m_UI.UI_DrawImage(GameImages.ICON_IS_TARGETTED, screen.X, screen.Y);
          break;
        }
      }
    }

    public void DrawItemsStack(Inventory inventory, Point screen, Color tint)
    {
      if (inventory == null) return;
      foreach (Item it in inventory.Items)
        DrawItem(it, screen.X, screen.Y, tint);
    }

    public void DrawMapIcon(Point position, string imageID)
    {
      m_UI.UI_DrawImage(imageID, position.X * TILE_SIZE, position.Y * TILE_SIZE);
    }

    public void DrawMapHealthBar(int hitPoints, int maxHitPoints, int gx, int gy)
    {
      DrawMapHealthBar(hitPoints, maxHitPoints, gx, gy, Color.Red);
    }

    public void DrawMapHealthBar(int hitPoints, int maxHitPoints, int gx, int gy, Color barColor)
    {
      int x = gx + 4;
      int y = gy + TILE_SIZE - 4;
      int width = (int) (20.0 * hitPoints / maxHitPoints);
      m_UI.UI_FillRect(Color.Black, new Rectangle(x, y, 20, 4));
      if (width <= 0) return;
      m_UI.UI_FillRect(barColor, new Rectangle(x + 1, y + 1, width, 2));
    }

    public void DrawBar(int value, int previousValue, int maxValue, int refValue, int maxWidth, int height, int gx, int gy, Color fillColor, Color lossFillColor, Color gainFillColor, Color emptyColor)
    {
      m_UI.UI_FillRect(emptyColor, new Rectangle(gx, gy, maxWidth, height));
      int width1 = (int)(maxWidth * (double)previousValue / maxValue);
      int width2 = (int)(maxWidth * (double)value / maxValue);
      if (value > previousValue) {
        if (width2 > 0)
          m_UI.UI_FillRect(gainFillColor, new Rectangle(gx, gy, width2, height));
        if (width1 > 0)
          m_UI.UI_FillRect(fillColor, new Rectangle(gx, gy, width1, height));
      } else if (value < previousValue) {
        if (width1 > 0)
          m_UI.UI_FillRect(lossFillColor, new Rectangle(gx, gy, width1, height));
        if (width2 > 0)
          m_UI.UI_FillRect(fillColor, new Rectangle(gx, gy, width2, height));
      }
      else if (width2 > 0)
        m_UI.UI_FillRect(fillColor, new Rectangle(gx, gy, width2, height));

      int num = (int)(maxWidth * (double)refValue / maxValue);
      m_UI.UI_DrawLine(Color.White, gx + num, gy, gx + num, gy + height);
    }

    private void DrawDetected(Actor actor, string minimap_img, string map_img)
    {
      Point point = new Point(MINIMAP_X + actor.Location.Position.X * MINITILE_SIZE, MINIMAP_Y + actor.Location.Position.Y * MINITILE_SIZE);
      m_UI.UI_DrawImage(minimap_img, point.X - 1, point.Y - 1);
      if (IsInViewRect(actor.Location) && !IsVisibleToPlayer(actor)) {
        Point screen = MapToScreen(actor.Location);
        m_UI.UI_DrawImage(map_img, screen.X, screen.Y);
      }
    }

    private void DrawDetected(Actor actor, Color minimap_color, string map_img)
    {
	  m_UI.UI_SetMinimapColor(actor.Location.Position.X, actor.Location.Position.Y, minimap_color);
      if (IsInViewRect(actor.Location) && !IsVisibleToPlayer(actor)) {
        Point screen = MapToScreen(actor.Location);
        m_UI.UI_DrawImage(map_img, screen.X, screen.Y);
      }
      m_UI.UI_DrawMinimap(MINIMAP_X, MINIMAP_Y);
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public void DrawMiniMap(Map map)
    {
      if (null == m_Player) return;   // fail-safe.
	  ThreatTracking threats = m_Player.Threats;    // these two should agree on whether they're null or not
      LocationSet sights_to_see = m_Player.InterestingLocs;

	  if (s_Options.IsMinimapOn) {
        m_UI.UI_ClearMinimap(Color.Black);
#region set visited tiles color.
        Point pos = new Point();
		if (null == threats) {
          map.Rect.DoForEach(pt => m_UI.UI_SetMinimapColor(pos.X, pos.Y, (map.HasExitAt(pos) ? Color.HotPink : map.GetTileModelAt(pos).MinimapColor)), pt => m_Player.Controller.IsKnown(new Location(map, pos)));
		} else {
          HashSet<Point> tainted = threats.ThreatWhere(map);
          HashSet<Point> tourism = sights_to_see.In(map);
          for (pos.X = 0; pos.X < map.Width; ++pos.X) {
            for (pos.Y = 0; pos.Y < map.Height; ++pos.Y) {
              if (tainted.Contains(pos)) {
                m_UI.UI_SetMinimapColor(pos.X, pos.Y, Color.Maroon);
                continue;
              }
              if (tourism.Contains(pos)) {
                m_UI.UI_SetMinimapColor(pos.X, pos.Y, Color.Magenta);
                continue;
              }
              if (!m_Player.Controller.IsKnown(new Location(map, pos))) continue;
              m_UI.UI_SetMinimapColor(pos.X, pos.Y, (map.HasExitAt(pos) ? Color.HotPink : map.GetTileModelAt(pos).MinimapColor));
            }
          }
		}
#endregion
        m_UI.UI_DrawMinimap(MINIMAP_X, MINIMAP_Y);
      }
      m_UI.UI_DrawRect(Color.White, new Rectangle(MINIMAP_X + m_MapViewRect.Left * MINITILE_SIZE, MINIMAP_Y + m_MapViewRect.Top * MINITILE_SIZE, m_MapViewRect.Width * MINITILE_SIZE, m_MapViewRect.Height * MINITILE_SIZE));
      if (s_Options.ShowPlayerTagsOnMinimap) {
        Point pos = new Point();
        for (pos.X = 0; pos.X < map.Width; ++pos.X) {
          for (pos.Y = 0; pos.Y < map.Height; ++pos.Y) {
            if (!m_Player.Controller.IsKnown(new Location(map, pos))) continue;
            Tile tileAt = map.GetTileAt(pos);
            string imageID = null;
            if (tileAt.HasDecoration(GameImages.DECO_PLAYER_TAG1)) imageID = GameImages.MINI_PLAYER_TAG1;
            else if (tileAt.HasDecoration(GameImages.DECO_PLAYER_TAG2)) imageID = GameImages.MINI_PLAYER_TAG2;
            else if (tileAt.HasDecoration(GameImages.DECO_PLAYER_TAG3)) imageID = GameImages.MINI_PLAYER_TAG3;
            else if (tileAt.HasDecoration(GameImages.DECO_PLAYER_TAG4)) imageID = GameImages.MINI_PLAYER_TAG4;
            if (imageID != null) {
              Point point = new Point(MINIMAP_X + pos.X * MINITILE_SIZE, MINIMAP_Y + pos.Y * MINITILE_SIZE);
              m_UI.UI_DrawImage(imageID, point.X - 1, point.Y - 1);
            }
          }
        }
      }
      if (!m_Player.IsSleeping) {
	    // normal detectors/lights
        ItemTracker itemTracker1 = m_Player.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;
        if (null!=itemTracker1 && itemTracker1.IsUseless) itemTracker1 = null;    // require batteries > 0
        bool find_followers = (null != itemTracker1 && m_Player.CountFollowers > 0 && itemTracker1.CanTrackFollowersOrLeader);
//      bool find_leader = (null != itemTracker1 && m_Player.HasLeader && itemTracker1.CanTrackFollowersOrLeader); // may need this, but not for single PC
        bool find_undead = (null != itemTracker1 && itemTracker1.CanTrackUndeads);
        bool find_blackops = (null != itemTracker1 && itemTracker1.CanTrackBlackOps);
        bool find_police = (null != itemTracker1 && itemTracker1.CanTrackPolice) || GameFactions.ThePolice == m_Player.Faction;
        // the police radio
        itemTracker1 = m_Player.GetEquippedItem(DollPart.HIP_HOLSTER) as ItemTracker;
        if (null!=itemTracker1 && itemTracker1.IsUseless) itemTracker1 = null;    // require batteries > 0
        if (null != itemTracker1) {
          if (!find_followers) find_followers = (m_Player.CountFollowers > 0 && itemTracker1.CanTrackFollowersOrLeader);
//        if (!find_leader) find_leader = (m_Player.HasLeader && itemTracker1.CanTrackFollowersOrLeader); // may need this, but not for single PC
          if (!find_undead) find_undead = itemTracker1.CanTrackUndeads;
          if (!find_blackops) find_blackops = itemTracker1.CanTrackBlackOps;
          if (!find_police) find_police = itemTracker1.CanTrackPolice;
        }

        // do not assume tracker capabilities are mutually exclusive.
        if (find_followers) {
          foreach (Actor follower in m_Player.Followers) {
            if (follower.Location.Map == m_Player.Location.Map) {
              if (follower.GetEquippedItem(DollPart.LEFT_HAND) is ItemTracker tracker && tracker.CanTrackFollowersOrLeader) {
                DrawDetected(follower, GameImages.MINI_FOLLOWER_POSITION, GameImages.TRACK_FOLLOWER_POSITION);
              }
            }
          }
        }
        if (find_undead) {
          foreach (Actor actor in map.Actors) {
            if (actor != m_Player && actor.Model.Abilities.IsUndead && Rules.GridDistance(actor.Location, m_Player.Location) <= Rules.ZTRACKINGRADIUS)
            {
              DrawDetected(actor, GameImages.MINI_UNDEAD_POSITION, GameImages.TRACK_UNDEAD_POSITION);
            }
          }
        }
        if (find_blackops) {
          foreach (Actor actor in map.Actors) {
            if (actor != m_Player && actor.Faction == GameFactions.TheBlackOps && actor.Location.Map == m_Player.Location.Map) {
              DrawDetected(actor, GameImages.MINI_BLACKOPS_POSITION, GameImages.TRACK_BLACKOPS_POSITION);
            }
          }
        }
        if (find_police) {
          foreach (Actor actor in map.Actors) {
            if (actor != m_Player && actor.Faction == GameFactions.ThePolice && actor.Location.Map == m_Player.Location.Map) {
              DrawDetected(actor, GameImages.MINI_POLICE_POSITION, GameImages.TRACK_POLICE_POSITION);
//            DrawDetected(actor, Color.Blue, GameImages.TRACK_POLICE_POSITION);
            }
          }
        }
      }	// end if (!m_Player.IsSleeping)
      Point position = m_Player.Location.Position;
      int x1 = MINIMAP_X + position.X * 2;
      int y1 = MINIMAP_Y + position.Y * 2;
      m_UI.UI_DrawImage(GameImages.MINI_PLAYER_POSITION, x1 - 1, y1 - 1);
    }

    static private ColorString ActorHungerStatus(Actor actor)
    {
      if (actor.IsStarving) return new ColorString(Color.Red,"STARVING!");
      if (actor.IsHungry) return new ColorString(Color.Yellow,"Hungry");
      return new ColorString(Color.White,string.Format("{0}h", actor.HoursUntilHungry));
    }

    static private ColorString ActorRotHungerStatus(Actor actor)
    {
      if (actor.IsRotStarving) return new ColorString(Color.Red,"STARVING!");
      if (actor.IsRotHungry) return new ColorString(Color.Yellow,"Hungry");
      return new ColorString(Color.White,string.Format("{0}h", actor.HoursUntilRotHungry));
    }

    static private ColorString ActorRunningStatus(Actor actor)
    {
      if (actor.IsRunning) return new ColorString(Color.LightGreen, "RUNNING!");
      if (actor.CanRun()) return new ColorString(Color.Green, "can run");
      if (actor.IsTired) return new ColorString(Color.Gray, "TIRED");
      return new ColorString(Color.Red,string.Empty);
    }

    static private ColorString ActorSleepStatus(Actor actor)
    {
      if (actor.IsExhausted) return new ColorString(Color.Red, "EXHAUSTED!");
      if (actor.IsSleepy) return new ColorString(Color.Yellow, "Sleepy");
      return new ColorString(Color.White, string.Format("{0}h", actor.HoursUntilSleepy));
    }

    static private ColorString ActorSanityStatus(Actor actor)
    {
      if (actor.IsInsane) return new ColorString(Color.Red, "INSANE!");
      if (actor.IsDisturbed) return new ColorString(Color.Yellow, "Disturbed");
      return new ColorString(Color.White, string.Format("{0}h", actor.HoursUntilUnstable));
    }

    static private string ActorStatString(Actor actor)
    {
      Defence defence = Rules.ActorDefence(actor, actor.CurrentDefence);
      return (actor.Model.Abilities.IsUndead
            ? string.Format("Def {0:D2} Spd {1:F2} En {2} FoV {3} Sml {4:F2} Kills {5}", defence.Value, ((double)actor.Speed / Rules.BASE_SPEED), actor.ActionPoints, actor.FOVrange(Session.Get.WorldTime, Session.Get.World.Weather), actor.Smell, actor.KillsCount)
            : string.Format("Def {0:D2} Arm {1:D1}/{2:D1} Spd {3:F2} En {4} FoV {5} Fol {6}/{7}", defence.Value, defence.Protection_Hit, defence.Protection_Shot, ((double)actor.Speed / Rules.BASE_SPEED), actor.ActionPoints, actor.FOVrange(Session.Get.WorldTime, Session.Get.World.Weather), actor.CountFollowers, actor.MaxFollowers));
    }

    public void DrawActorStatus(Actor actor, int gx, int gy)
    {
      m_UI.UI_DrawStringBold(Color.White, string.Format("{0}, {1}", actor.Name, actor.Faction.MemberName), gx, gy, new Color?());
      gy += 14;
      int maxValue1 = actor.MaxHPs;
      m_UI.UI_DrawStringBold(Color.White, string.Format("HP  {0}", actor.HitPoints), gx, gy, new Color?());
      DrawBar(actor.HitPoints, actor.PreviousHitPoints, maxValue1, 0, 100, 14, gx + 70, gy, Color.Red, Color.DarkRed, Color.OrangeRed, Color.Gray);
      m_UI.UI_DrawStringBold(Color.White, string.Format("{0}", maxValue1), gx + 84 + 100, gy, new Color?());
      gy += 14;
      if (actor.Model.Abilities.CanTire) {
        int maxValue2 = actor.MaxSTA;
        m_UI.UI_DrawStringBold(Color.White, string.Format("STA {0}", actor.StaminaPoints), gx, gy, new Color?());
        DrawBar(actor.StaminaPoints, actor.PreviousStaminaPoints, maxValue2, 10, 100, 14, gx + 70, gy, Color.Green, Color.DarkGreen, Color.LightGreen, Color.Gray);
        m_UI.UI_DrawStringBold(Color.White, string.Format("{0}", maxValue2), gx + 84 + 100, gy, new Color?());
        m_UI.UI_DrawStringBold(ActorRunningStatus(actor), gx + 126 + 100, gy);
      }
      gy += 14;
      if (actor.Model.Abilities.HasToEat) {
        int maxValue2 = actor.MaxFood;
        m_UI.UI_DrawStringBold(Color.White, string.Format("FOO {0}", actor.FoodPoints), gx, gy, new Color?());
        DrawBar(actor.FoodPoints, actor.PreviousFoodPoints, maxValue2, Actor.FOOD_HUNGRY_LEVEL, 100, 14, gx + 70, gy, Color.Chocolate, Color.Brown, Color.Beige, Color.Gray);
        m_UI.UI_DrawStringBold(Color.White, string.Format("{0}", maxValue2), gx + 84 + 100, gy, new Color?());
        m_UI.UI_DrawStringBold(ActorHungerStatus(actor), gx + 126 + 100, gy);
      } else if (actor.Model.Abilities.IsRotting) {
        int maxValue2 = actor.MaxRot;
        m_UI.UI_DrawStringBold(Color.White, string.Format("ROT {0}", actor.FoodPoints), gx, gy, new Color?());
        DrawBar(actor.FoodPoints, actor.PreviousFoodPoints, maxValue2, Actor.ROT_HUNGRY_LEVEL, 100, 14, gx + 70, gy, Color.Chocolate, Color.Brown, Color.Beige, Color.Gray);
        m_UI.UI_DrawStringBold(Color.White, string.Format("{0}", maxValue2), gx + 84 + 100, gy, new Color?());
        m_UI.UI_DrawStringBold(ActorRotHungerStatus(actor), gx + 126 + 100, gy);
      }
      gy += 14;
      if (actor.Model.Abilities.HasToSleep) {
        int maxValue2 = actor.MaxSleep;
        m_UI.UI_DrawStringBold(Color.White, string.Format("SLP {0}", actor.SleepPoints), gx, gy, new Color?());
        DrawBar(actor.SleepPoints, actor.PreviousSleepPoints, maxValue2, Actor.SLEEP_SLEEPY_LEVEL, 100, 14, gx + 70, gy, Color.Blue, Color.DarkBlue, Color.LightBlue, Color.Gray);
        m_UI.UI_DrawStringBold(Color.White, string.Format("{0}", maxValue2), gx + 84 + 100, gy, new Color?());
        m_UI.UI_DrawStringBold(ActorSleepStatus(actor), gx + 126 + 100, gy);
      }
      gy += 14;
      if (actor.Model.Abilities.HasSanity) {
        int maxValue2 = actor.MaxSanity;
        m_UI.UI_DrawStringBold(Color.White, string.Format("SAN {0}", actor.Sanity), gx, gy, new Color?());
        DrawBar(actor.Sanity, actor.PreviousSanity, maxValue2, Rules.ActorDisturbedLevel(actor), 100, 14, gx + 70, gy, Color.Orange, Color.DarkOrange, Color.OrangeRed, Color.Gray);
        m_UI.UI_DrawStringBold(Color.White, string.Format("{0}", maxValue2), gx + 84 + 100, gy, new Color?());
        m_UI.UI_DrawStringBold(ActorSanityStatus(actor), gx + 126 + 100, gy);
      }
      if (Session.Get.HasInfection && !actor.Model.Abilities.IsUndead) {
        int maxValue2 = actor.InfectionHPs;
        int refValue = Rules.INFECTION_LEVEL_1_WEAK * maxValue2 / 100;
        gy += 14;
        m_UI.UI_DrawStringBold(Color.White, string.Format("INF {0}", actor.Infection), gx, gy, new Color?());
        DrawBar(actor.Infection, actor.Infection, maxValue2, refValue, 100, 14, gx + 70, gy, Color.Purple, Color.Black, Color.Black, Color.Gray);
        m_UI.UI_DrawStringBold(Color.White, string.Format("{0}%", actor.InfectionPercent), gx + 84 + 100, gy, new Color?());
      }
      gy += 14;
      Attack attack1 = actor.MeleeAttack();
      int num1 = actor.DamageBonusVsUndeads;
      m_UI.UI_DrawStringBold(Color.White, string.Format("Melee  Atk {0:D2}  Dmg {1:D2}/{2:D2}", attack1.HitValue, attack1.DamageValue, attack1.DamageValue + num1), gx, gy, new Color?());
      gy += 14;
      Attack attack2 = actor.RangedAttack(actor.CurrentRangedAttack.EfficientRange);
      if (actor.GetEquippedWeapon() is ItemRangedWeapon rw) {
        int ammo = rw.Ammo;
        int maxAmmo = rw.Model.MaxAmmo;
        m_UI.UI_DrawStringBold(Color.White, string.Format("Ranged Atk {0:D2}  Dmg {1:D2}/{2:D2} Rng {3}-{4} Amo {5}/{6}", attack2.HitValue, attack2.DamageValue, attack2.DamageValue + num1, attack2.Range, attack2.EfficientRange, ammo, maxAmmo), gx, gy, new Color?());
      }
      gy += 14;
      m_UI.UI_DrawStringBold(Color.White, ActorStatString(actor), gx, gy, new Color?());
    }

    static private string TrapStatusIcon(ItemTrap it)
    {
      if (null == it) return "";
      if (it.IsTriggered) return GameImages.ICON_TRAP_TRIGGERED;
      if (it.IsActivated) return GameImages.ICON_TRAP_ACTIVATED;
      return "";
    }

    public void DrawInventory(Inventory inventory, string title, bool drawSlotsNumbers, int slotsPerLine, int maxSlots, int gx, int gy)
    {
      gy -= 14;
      m_UI.UI_DrawStringBold(Color.White, title, gx, gy, new Color?());
      gy += 14;
      int gx1 = gx;
      int gy1 = gy;
      int num1 = 0;
      for (int index = 0; index < maxSlots; ++index) {
        m_UI.UI_DrawImage(GameImages.ITEM_SLOT, gx1, gy1);
        if (++num1 >= slotsPerLine) {
          num1 = 0;
          gy1 += TILE_SIZE;
          gx1 = gx;
        } else
          gx1 += TILE_SIZE;
      }
      if (inventory == null) return;
      int gx2 = gx;
      int gy2 = gy;
      int num2 = 0;
      foreach (Item it in inventory.Items) {
        if (it.IsEquipped)
          m_UI.UI_DrawImage(GameImages.ITEM_EQUIPPED, gx2, gy2);
        if (it is ItemRangedWeapon) {
          ItemRangedWeapon itemRangedWeapon = it as ItemRangedWeapon;
          if (itemRangedWeapon.Ammo <= 0)
            m_UI.UI_DrawImage(GameImages.ICON_OUT_OF_AMMO, gx2, gy2);
          DrawBar(itemRangedWeapon.Ammo, itemRangedWeapon.Ammo, itemRangedWeapon.Model.MaxAmmo, 0, 28, 3, gx2 + 2, gy2 + 27, Color.Blue, Color.Blue, Color.Blue, Color.DarkGray);
        } else if (it is ItemSprayPaint) {
          ItemSprayPaint itemSprayPaint = it as ItemSprayPaint;
          DrawBar(itemSprayPaint.PaintQuantity, itemSprayPaint.PaintQuantity, itemSprayPaint.Model.MaxPaintQuantity, 0, 28, 3, gx2 + 2, gy2 + 27, Color.Gold, Color.Gold, Color.Gold, Color.DarkGray);
        } else if (it is ItemSprayScent) {
          ItemSprayScent itemSprayScent = it as ItemSprayScent;
          DrawBar(itemSprayScent.SprayQuantity, itemSprayScent.SprayQuantity, itemSprayScent.Model.MaxSprayQuantity, 0, 28, 3, gx2 + 2, gy2 + 27, Color.Cyan, Color.Cyan, Color.Cyan, Color.DarkGray);
        }
        else if (it is BatteryPowered) {
          BatteryPowered tmp = it as BatteryPowered;
          Color bar_color = (it is ItemLight ? Color.Yellow : Color.Pink);
          if (0 >= tmp.Batteries) m_UI.UI_DrawImage(GameImages.ICON_OUT_OF_BATTERIES, gx2, gy2);
          DrawBar(tmp.Batteries, tmp.Batteries, tmp.MaxBatteries, 0, 28, 3, gx2 + 2, gy2 + 27, bar_color, bar_color, bar_color, Color.DarkGray);
        } else if (it is ItemFood) {
          ItemFood food = it as ItemFood;
          if (food.IsExpiredAt(Session.Get.WorldTime.TurnCounter))
            m_UI.UI_DrawImage(GameImages.ICON_EXPIRED_FOOD, gx2, gy2);
          else if (food.IsSpoiledAt(Session.Get.WorldTime.TurnCounter))
            m_UI.UI_DrawImage(GameImages.ICON_SPOILED_FOOD, gx2, gy2);
        } else if (it is ItemTrap) {
          string trap_status = TrapStatusIcon(it as ItemTrap);
          if (!string.IsNullOrEmpty(trap_status)) m_UI.UI_DrawImage(trap_status, gx2, gy2);
        }
        else if (it is ItemEntertainment && m_Player != null && m_Player.IsBoredOf(it))
          m_UI.UI_DrawImage(GameImages.ICON_BORING_ITEM, gx2, gy2);
        DrawItem(it, gx2, gy2);
        if (++num2 >= slotsPerLine) {
          num2 = 0;
          gy2 += TILE_SIZE;
          gx2 = gx;
        } else
          gx2 += TILE_SIZE;
      }
      if (!drawSlotsNumbers) return;
      int gx3 = gx + 4;
      int gy3 = gy + TILE_SIZE;
      for (int index = 0; index < inventory.MaxCapacity; ++index) {
        m_UI.UI_DrawString(Color.White, (index + 1).ToString(), gx3, gy3, new Color?());
        gx3 += TILE_SIZE;
      }
    }

    public void DrawItem(Item it, int gx, int gy)
    {
      DrawItem(it, gx, gy, Color.White);
    }

    public void DrawItem(Item it, int gx, int gy, Color tint)
    {
      m_UI.UI_DrawImage(it.ImageID, gx, gy, tint);
      if (it.Model.IsStackable) {
        string text = string.Format("{0}", it.Quantity);
        int gx1 = gx + TILE_SIZE - 10;
        if (it.Quantity > 100) gx1 -= 10;
        else if (it.Quantity > 10) gx1 -= 4;
        m_UI.UI_DrawString(Color.DarkGray, text, gx1 + 1, gy + 1, new Color?());
        m_UI.UI_DrawString(Color.White, text, gx1, gy, new Color?());
      }
      string trap_status = TrapStatusIcon(it as ItemTrap);
      if (!string.IsNullOrEmpty(trap_status)) m_UI.UI_DrawImage(trap_status, gx, gy);
    }

    public void DrawActorSkillTable(Actor actor, int gx, int gy)
    {
      gy -= 14;
      m_UI.UI_DrawStringBold(Color.White, "Skills", gx, gy, new Color?());
      gy += 14;
      IEnumerable<Skill> skills = actor.Sheet.SkillTable.Skills;
      if (skills == null) return;
      int num = 0;
      int gx1 = gx;
      int gy1 = gy;
      foreach (Skill skill in skills) {
        m_UI.UI_DrawString(Color.White, string.Format("{0}-", skill.Level), gx1, gy1, new Color?());
        int gx2 = gx1 + 16;
        m_UI.UI_DrawString(Color.White, Skills.Name(skill.ID), gx2, gy1, new Color?());
        gx1 = gx2 - 16;
        if (++num >= 10) {
          num = 0;
          gy1 = gy;
          gx1 += TEXTFILE_CHARS_PER_LINE;
        } else gy1 += 12;
      }
    }

    public void AddOverlay(Overlay o)
    {
      lock(m_Overlays) { m_Overlays.Add(o); }
    }

    public void ClearOverlays()
    {
      lock(m_Overlays) { m_Overlays.Clear(); }
    }

    private Point MapToScreen(int x, int y)
    {
      return new Point((x - m_MapViewRect.Left) * TILE_SIZE, (y - m_MapViewRect.Top) * TILE_SIZE);
    }

    private Point MapToScreen(Point mapPosition)
    {
      return MapToScreen(mapPosition.X, mapPosition.Y);
    }

    public Point MapToScreen(Location loc)
    {
      if (loc.Map == Session.Get.CurrentMap) return MapToScreen(loc.Position);
      Location? tmp = Session.Get.CurrentMap.Denormalize(loc);
#if DEBUG
      if (null == tmp) throw new ArgumentNullException(nameof(tmp));
#endif
      return MapToScreen(tmp.Value.Position);
    }

    private Point ScreenToMap(Point screenPosition)
    {
      return ScreenToMap(screenPosition.X, screenPosition.Y);
    }

    private Point ScreenToMap(int gx, int gy)
    {
      return new Point(m_MapViewRect.Left + gx / TILE_SIZE, m_MapViewRect.Top + gy / TILE_SIZE);
    }

    private Point MouseToMap(Point mousePosition)
    {
      return MouseToMap(mousePosition.X, mousePosition.Y);
    }

    private Point MouseToMap(int mouseX, int mouseY)
    {
      mouseX = (int)(mouseX / (double)m_UI.UI_GetCanvasScaleX());
      mouseY = (int)(mouseY / (double)m_UI.UI_GetCanvasScaleY());
      return ScreenToMap(mouseX, mouseY);
    }

    private Point MouseToInventorySlot(int invX, int invY, int mouseX, int mouseY)
    {
      mouseX = (int)(mouseX / (double)m_UI.UI_GetCanvasScaleX());
      mouseY = (int)(mouseY / (double)m_UI.UI_GetCanvasScaleY());
      return new Point((mouseX - invX) / TILE_SIZE, (mouseY - invY) / TILE_SIZE);
    }

    static private Point InventorySlotToScreen(int invX, int invY, int slotX, int slotY)
    {
      return new Point(invX + slotX * TILE_SIZE, invY + slotY * TILE_SIZE);
    }

    private bool IsVisibleToPlayer(Location location)
    {
      return m_Player?.Controller.IsVisibleTo(location) ?? false;
    }

    private bool IsVisibleToPlayer(Map map, Point position)
    {
      return m_Player?.Controller.IsVisibleTo(map,position) ?? false;
    }

    private bool IsVisibleToPlayer(Actor actor)
    {
      return m_Player?.Controller.IsVisibleTo(actor) ?? false;
    }

    private bool IsVisibleToPlayer(MapObject mapObj)
    {
      return m_Player?.Controller.IsVisibleTo(mapObj.Location) ?? false;
    }

    public void PanViewportTo(Actor player)
    {
      m_Player = player;
      Session.Get.CurrentMap = player.Location.Map;
      ComputeViewRect(m_Player.Location.Position);
      RedrawPlayScreen();
    }

    private bool ForceVisibleToPlayer(Map map, Point position)
    {
      if (null == map) return false;    // convince Duckman to not superheroically crash many games on turn 0
      if (!map.IsValid(position)) return false;
      if (Session.Get.CurrentMap != map) {
        Location? tmp = Session.Get.CurrentMap.Denormalize(new Location(map,position));
        if (null == tmp) return false;
        return ForceVisibleToPlayer(tmp.Value);
      }

      if (null != m_Player && map == m_Player.Location.Map && m_Player.Controller.FOV.Contains(position)) return true;

      Actor who = map.FindPlayerWithFOV(position);
      if (null == who) return false;
      PanViewportTo(who);
      return true;
    }

    private bool ForceVisibleToPlayer(Actor actor)
    {
      if (actor == m_Player) return true;
      if (IsVisibleToPlayer(actor.Location)) return true;
      if (actor.IsPlayer) {
        PanViewportTo(actor);
        return true;
      }
      return ForceVisibleToPlayer(actor.Location);
    }

    private bool ForceVisibleToPlayer(MapObject mapObj)
    {
      return ForceVisibleToPlayer(mapObj.Location);
    }

    private bool ForceVisibleToPlayer(Location location)
    {
      return ForceVisibleToPlayer(location.Map, location.Position);
    }

    private bool IsPlayerSleeping()
    {
      if (m_Player != null)
        return m_Player.IsSleeping;
      return false;
    }

    static private int FindLongestLine(string[] lines)
    {
      if (lines == null || lines.Length == 0) return 0;
      int num = int.MinValue;
      foreach (string line in lines) {
        if (line != null && line.Length > num)
          num = line.Length;
      }
      return num;
    }

    private void HandleSaveGame()
    {
      DoSaveGame(RogueGame.GetUserSave());
    }

    private void HandleLoadGame()
    {
      DoLoadGame(RogueGame.GetUserSave());
    }

    private void DoSaveGame(string saveName)
    {
	  Contract.Requires(!string.IsNullOrEmpty(saveName));
      ClearMessages();
      AddMessage(new Data.Message("SAVING GAME, PLEASE WAIT...", Session.Get.WorldTime.TurnCounter, Color.Yellow));
      RedrawPlayScreen();
      m_UI.UI_Repaint();
      lock (m_SimMutex) {
        Session.Save(Session.Get, saveName, Session.SaveFormat.FORMAT_BIN);
      }
#if DEBUG
      File.Copy(saveName, RogueGame.GetUserSaveBackup(),true);
#endif
      AddMessage(new Data.Message("SAVING DONE.", Session.Get.WorldTime.TurnCounter, Color.Yellow));
      RedrawPlayScreen();
      m_UI.UI_Repaint();
    }

    private void DoLoadGame(string saveName)
    {
	  Contract.Requires(!string.IsNullOrEmpty(saveName));
      ClearMessages();
      AddMessage(new Data.Message("LOADING GAME, PLEASE WAIT...", Session.Get.WorldTime.TurnCounter, Color.Yellow));
      RedrawPlayScreen();
      m_UI.UI_Repaint();
      lock (m_SimMutex) {
        if (LoadGame(saveName)) return;
      }
      AddMessage(new Data.Message("LOADING FAILED, NO GAME SAVED OR VERSION NOT COMPATIBLE.", Session.Get.WorldTime.TurnCounter, Color.Red));
    }

    private void DeleteSavedGame(string saveName)
    {
	  Contract.Requires(!string.IsNullOrEmpty(saveName));
      if (!Session.Delete(saveName)) return;
      AddMessage(new Data.Message("PERMADEATH : SAVE GAME DELETED!", Session.Get.WorldTime.TurnCounter, Color.Red));
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private bool LoadGame(string saveName)
    {
	  Contract.Requires(!string.IsNullOrEmpty(saveName));
      if (!Session.Load(saveName, Session.SaveFormat.FORMAT_BIN)) return false;
      // command line option --PC requests converting an NPC to a PC
      if (Session.CommandLineOptions.ContainsKey("PC")) Session.Get.World.MakePC();
      m_Rules = new Rules(new DiceRoller(Session.Get.Seed));
      m_Player = null;
      RefreshPlayer();
      AddMessage(new Data.Message("LOADING DONE.", Session.Get.WorldTime.TurnCounter, Color.Yellow));
      AddMessage(new Data.Message("Welcome back to "+SetupConfig.GAME_NAME+"!", Session.Get.WorldTime.TurnCounter, Color.LightGreen));
      RedrawPlayScreen();
      m_UI.UI_Repaint();
      Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, "<Loaded game>");
      return true;
    }

    static private void LoadOptions()
    {
      s_Options = GameOptions.Load(GetUserOptionsFilePath());
    }

    static private void SaveOptions()
    {
      GameOptions.Save(s_Options, GetUserOptionsFilePath());
    }

    private void ApplyOptions(bool ingame)
    {
      m_MusicManager.IsMusicEnabled = RogueGame.Options.PlayMusic;
      m_MusicManager.Volume = RogueGame.Options.MusicVolume;
      Session.Get.Scoring.Side = m_Player == null || !m_Player.Model.Abilities.IsUndead ? DifficultySide.FOR_SURVIVOR : DifficultySide.FOR_UNDEAD;
      Session.Get.Scoring.DifficultyRating = Scoring.ComputeDifficultyRating(s_Options, Session.Get.Scoring.Side, Session.Get.Scoring.ReincarnationNumber);
      if (m_MusicManager.IsMusicEnabled) return;
      m_MusicManager.StopAll();
    }

    private void LoadKeybindings()
    {
      m_UI.UI_Clear(Color.Black);
      m_UI.UI_DrawStringBold(Color.White, "Loading keybindings...", 0, 0, new Color?());
      m_UI.UI_Repaint();
      RogueGame.s_KeyBindings = Keybindings.Load(RogueGame.GetUserConfigPath() + "keys.dat");
      m_UI.UI_Clear(Color.Black);
      m_UI.UI_DrawStringBold(Color.White, "Loading keybindings... done!", 0, 0, new Color?());
      m_UI.UI_Repaint();
    }

    private void SaveKeybindings()
    {
      m_UI.UI_Clear(Color.Black);
      m_UI.UI_DrawStringBold(Color.White, "Saving keybindings...", 0, 0, new Color?());
      m_UI.UI_Repaint();
      Keybindings.Save(RogueGame.s_KeyBindings, RogueGame.GetUserConfigPath() + "keys.dat");
      m_UI.UI_Clear(Color.Black);
      m_UI.UI_DrawStringBold(Color.White, "Saving keybindings... done!", 0, 0, new Color?());
      m_UI.UI_Repaint();
    }

    private void LoadHints()
    {
      m_UI.UI_Clear(Color.Black);
      m_UI.UI_DrawStringBold(Color.White, "Loading hints...", 0, 0, new Color?());
      m_UI.UI_Repaint();
      RogueGame.s_Hints = GameHintsStatus.Load(RogueGame.GetUserConfigPath() + "hints.dat");
      m_UI.UI_Clear(Color.Black);
      m_UI.UI_DrawStringBold(Color.White, "Loading hints... done!", 0, 0, new Color?());
      m_UI.UI_Repaint();
    }

    static private void SaveHints()
    {
      GameHintsStatus.Save(RogueGame.s_Hints, RogueGame.GetUserConfigPath() + "hints.dat");
    }

    private void DrawMenuOrOptions(int currentChoice, Color entriesColor, string[] entries, Color valuesColor, string[] values, int gx, ref int gy, int rightPadding = 256)
    {
      Contract.Requires(null != entries);
      Contract.Requires(null == values || entries.Length == values.Length);
      int gx1 = gx + rightPadding;
      Color color = Color.FromArgb(entriesColor.A, entriesColor.R / 2, entriesColor.G / 2, entriesColor.B / 2);
      for (int index = 0; index < entries.Length; ++index) {
        string text1 = index != currentChoice ? string.Format("     {0}", entries[index]) : string.Format("---> {0}", entries[index]);
        m_UI.UI_DrawStringBold(entriesColor, text1, gx, gy, new Color?(color));
        if (values != null) {
          string text2 = index != currentChoice ? values[index] : string.Format("{0} <---", values[index]);
          m_UI.UI_DrawStringBold(valuesColor, text2, gx1, gy, new Color?());
        }
        gy += 14;
      }
    }

    private void DrawHeader()
    {
      m_UI.UI_DrawStringBold(Color.Red, SetupConfig.GAME_NAME_CAPS+" - " + SetupConfig.GAME_VERSION, 0, 0, new Color?(Color.DarkRed));
    }

    private void DrawFootnote(Color color, string text)
    {
      Color color1 = Color.FromArgb(color.A, color.R / 2, color.G / 2, color.B / 2);
      m_UI.UI_DrawStringBold(color, string.Format("<{0}>", text), 0, 754, new Color?(color1));
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

#if DEBUG
    public static string GetUserSaveBackup()
    {
      return RogueGame.GetUserSavesPath() + "save." + Session.Get.WorldTime.TurnCounter.ToString();
    }
#endif

    public static string GetUserDocsPath()
    {
      return RogueGame.GetUserBasePath() + "Docs\\";
    }

    public static string GetUserGraveyardPath()
    {
      return RogueGame.GetUserBasePath() + "Graveyard\\";
    }

    static private string GetUserNewGraveyardName()
    {
      int num = 0;
      string graveName;
      bool flag;
      do
      {
        graveName = string.Format("grave_{0:D3}", num);
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
      do {
        shotname = string.Format("screenshot_{0:D3}", num);
        flag = !File.Exists(ScreenshotFilePath(shotname));
        ++num;
      }
      while (!flag);
      return shotname;
    }

    public string ScreenshotFilePath(string shotname)
    {
      return RogueGame.GetUserScreenshotsPath() + shotname + "." + m_UI.UI_ScreenshotExtension();
    }

    private bool CreateDirectory(string path)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      if (Directory.Exists(path)) return false;
      Directory.CreateDirectory(path);
      return true;
    }

    private bool CheckDirectory(string path, string description, ref int gy)
    {
#if DEBUG
      if (string.IsNullOrEmpty(path)) throw new ArgumentOutOfRangeException(nameof(path),path, "string.IsNullOrEmpty(path)");
#endif
      m_UI.UI_DrawString(Color.White, string.Format("{0} : {1}...", description, path), 0, gy, new Color?());
      gy += 14;
      m_UI.UI_Repaint();
      bool directory = CreateDirectory(path);
      m_UI.UI_DrawString(Color.White, "ok.", 0, gy, new Color?());
      gy += 14;
      m_UI.UI_Repaint();
      return directory;
    }

    static private bool CheckCopyOfManual()
    {
      string str1 = "Resources\\Manual\\";
      string userDocsPath = RogueGame.GetUserDocsPath();
      const string str2 = "RS Manual.txt";
      bool flag = false;
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "checking for manual...");
      if (!File.Exists(userDocsPath + str2)) {
        Logger.WriteLine(Logger.Stage.INIT_MAIN, "copying manual...");
        flag = true;
        File.Copy(str1 + str2, userDocsPath + str2);
        Logger.WriteLine(Logger.Stage.INIT_MAIN, "copying manual... done!");
      }
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "checking for manual... done!");
      return flag;
    }

    static private string GetUserManualFilePath()
    {
      return GetUserDocsPath() + "RS Manual.txt";
    }

    static private string GetUserHiScorePath()
    {
      return GetUserSavesPath();
    }

    static private string GetUserHiScoreFilePath()
    {
      return GetUserHiScorePath() + "hiscores.dat";
    }

    static private string GetUserHiScoreTextFilePath()
    {
      return GetUserHiScorePath() + "hiscores.txt";
    }

    private void GenerateWorld(bool isVerbose)
    {
      if (isVerbose) {
        m_UI.UI_Clear(Color.Black);
        m_UI.UI_DrawStringBold(Color.White, "Generating game world...", 0, 0, new Color?());
        m_UI.UI_Repaint();
      }
      if (isVerbose) {
        m_UI.UI_Clear(Color.Black);
        m_UI.UI_DrawStringBold(Color.White, "Creating empty world...", 0, 0, new Color?());
        m_UI.UI_Repaint();
      }
      Session.Get.Reset();
      m_Rules = new Rules(new DiceRoller(Session.Get.Seed));
      World world = Session.Get.World;
      List<Point> pointList = new List<Point>();
      for (int x = 0; x < world.Size; ++x) {
        for (int y = 0; y < world.Size; ++y)
          pointList.Add(new Point(x, y));
      }
      Point policeStationDistrictPos = pointList[m_Rules.Roll(0, pointList.Count)];
      pointList.Remove(policeStationDistrictPos);
      Point hospitalDistrictPos = pointList[m_Rules.Roll(0, pointList.Count)];
      pointList.Remove(hospitalDistrictPos);
      for (int index1 = 0; index1 < world.Size; ++index1) {
        for (int index2 = 0; index2 < world.Size; ++index2) {
          if (isVerbose) {
            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.White, string.Format("Creating District@{0}...", World.CoordToString(index1, index2)), 0, 0, new Color?());
            m_UI.UI_Repaint();
          }
          District district = new District(new Point(index1, index2), GenerateDistrictKind(index1, index2));
          world[index1, index2] = district;
          // All map generation types should have an entry map so that is sort-of-ok to have as a member function of District
          district.GenerateEntryMap(world, policeStationDistrictPos, hospitalDistrictPos, s_Options.DistrictSize, m_TownGenerator);
          // other (hypothetical) map generation types are not guaranteed to have sewers or subways so leave those where they are
          GenerateDistrictSewersMap(district);
          if (index2 == world.Size / 2) GenerateDistrictSubwayMap(district);
        }
      }
      if (isVerbose) {
        m_UI.UI_Clear(Color.Black);
        m_UI.UI_DrawStringBold(Color.White, "Generating unique maps...", 0, 0, new Color?());
        m_UI.UI_Repaint();
      }
      Session.Get.UniqueMaps.CHARUndergroundFacility = CreateUniqueMap_CHARUndegroundFacility(world);
      if (isVerbose) {
        m_UI.UI_Clear(Color.Black);
        m_UI.UI_DrawStringBold(Color.White, "Generating unique actors...", 0, 0, new Color?());
        m_UI.UI_Repaint();
      }
      Session.Get.UniqueActors.TheSewersThing = SpawnUniqueSewersThing(world);
      Session.Get.UniqueActors.BigBear = CreateUniqueBigBear();
      Session.Get.UniqueActors.FamuFataru = CreateUniqueFamuFataru();
      Session.Get.UniqueActors.Santaman = CreateUniqueSantaman();
      Session.Get.UniqueActors.Roguedjack = CreateUniqueRoguedjack();
      Session.Get.UniqueActors.Duckman = CreateUniqueDuckman();
      Session.Get.UniqueActors.HansVonHanz = CreateUniqueHansVonHanz();
      Session.Get.UniqueItems.TheSubwayWorkerBadge = SpawnUniqueSubwayWorkerBadge(world);
      for (int x1 = 0; x1 < world.Size; ++x1) {
        for (int y1 = 0; y1 < world.Size; ++y1) {
          if (isVerbose) {
            m_UI.UI_Clear(Color.Black);
            m_UI.UI_DrawStringBold(Color.White, string.Format("Linking District@{0}...", World.CoordToString(x1, y1)), 0, 0, new Color?());
            m_UI.UI_Repaint();
          }
          Map entryMap1 = world[x1, y1].EntryMap;
          if (y1 > 0) {
            Map entryMap2 = world[x1, y1 - 1].EntryMap;
            for (int x2 = 0; x2 < entryMap1.Width; ++x2) {
              if (x2 < entryMap2.Width) {
                Point from1 = new Point(x2, -1);
                Point to1 = new Point(x2, entryMap2.Height - 1);
                Point from2 = new Point(x2, entryMap2.Height);
                Point to2 = new Point(x2, 0);
                if (CheckIfExitIsGood(entryMap2, to1) && CheckIfExitIsGood(entryMap1, to2)) {
                  GenerateExit(entryMap1, from1, entryMap2, to1);
                  GenerateExit(entryMap2, from2, entryMap1, to2);
                }
              }
            }
          }
          if (x1 > 0) {
            Map entryMap2 = world[x1 - 1, y1].EntryMap;
            for (int y2 = 0; y2 < entryMap1.Height; ++y2) {
              if (y2 < entryMap2.Height) {
                Point from1 = new Point(-1, y2);
                Point to1 = new Point(entryMap2.Width - 1, y2);
                Point from2 = new Point(entryMap2.Width, y2);
                Point to2 = new Point(0, y2);
                if (CheckIfExitIsGood(entryMap2, to1) && CheckIfExitIsGood(entryMap1, to2)) {
                  GenerateExit(entryMap1, from1, entryMap2, to1);
                  GenerateExit(entryMap2, from2, entryMap1, to2);
                }
              }
            }
          }
          Map sewersMap1 = world[x1, y1].SewersMap;
          if (y1 > 0) {
            Map sewersMap2 = world[x1, y1 - 1].SewersMap;
            for (int x2 = 0; x2 < sewersMap1.Width; ++x2) {
              if (x2 < sewersMap2.Width) {
                Point from1 = new Point(x2, -1);
                Point to1 = new Point(x2, sewersMap2.Height - 1);
                Point from2 = new Point(x2, sewersMap2.Height);
                Point to2 = new Point(x2, 0);
                GenerateExit(sewersMap1, from1, sewersMap2, to1);
                GenerateExit(sewersMap2, from2, sewersMap1, to2);
              }
            }
          }
          if (x1 > 0) {
            Map sewersMap2 = world[x1 - 1, y1].SewersMap;
            for (int y2 = 0; y2 < sewersMap1.Height; ++y2) {
              if (y2 < sewersMap2.Height) {
                Point from1 = new Point(-1, y2);
                Point to1 = new Point(sewersMap2.Width - 1, y2);
                Point from2 = new Point(sewersMap2.Width, y2);
                Point to2 = new Point(0, y2);
                GenerateExit(sewersMap1, from1, sewersMap2, to1);
                GenerateExit(sewersMap2, from2, sewersMap1, to2);
              }
            }
          }
          Map subwayMap1 = world[x1, y1].SubwayMap;
          if (subwayMap1 != null && x1 > 0) {
            Map subwayMap2 = world[x1 - 1, y1].SubwayMap;
            for (int y2 = 0; y2 < subwayMap1.Height; ++y2) {
              if (y2 < subwayMap2.Height) {
                Point from1 = new Point(-1, y2);
                Point to1 = new Point(subwayMap2.Width - 1, y2);
                Point from2 = new Point(subwayMap2.Width, y2);
                Point to2 = new Point(0, y2);
                if (subwayMap1.IsWalkable(subwayMap1.Width - 1, y2) && subwayMap2.IsWalkable(0, y2)) {
                  GenerateExit(subwayMap1, from1, subwayMap2, to1);
                  GenerateExit(subwayMap2, from2, subwayMap1, to2);
                }
              }
            }
          }
        }
      }
      Map sewersMap = world[0, 0].SewersMap;
      sewersMap.RemoveMapObjectAt(1, 1);
      sewersMap.GetTileAt(1, 1).RemoveAllDecorations();
      sewersMap.GetTileAt(1, 1).AddDecoration(GameImages.DECO_ROGUEDJACK_TAG);
      if (isVerbose) {
        m_UI.UI_Clear(Color.Black);
        m_UI.UI_DrawStringBold(Color.White, "Spawning player...", 0, 0, new Color?());
        m_UI.UI_Repaint();
      }

      if (!Session.Get.CMDoptionExists("no-spawn")) {
        int index = world.Size / 2;
        Map entryMap = world[index, index].EntryMap;
        GeneratePlayerOnMap(entryMap, m_TownGenerator);
        SetCurrentMap(entryMap);
        RefreshPlayer();
        foreach(Actor player in entryMap.Players) {
          player.Controller.UpdateSensors();
        }
        if (s_Options.RevealStartingDistrict) {
          foreach(Actor player in entryMap.Players) {
            Point pos = player.Location.Position;
            List<Zone> zonesAt1 = entryMap.GetZonesAt(pos);
            if (null == zonesAt1) continue;
            Zone zone = zonesAt1[0];
            entryMap.Rect.DoForEach((pt => {
              player.Controller.ForceKnown(pt);
            }), (pt => {
              if (!entryMap.IsInsideAt(pt)) return true;
              List<Zone> zonesAt2 = entryMap.GetZonesAt(pt);
              return zonesAt2 != null && zonesAt2[0] == zone;
            }));
          }
        }
      }
      if (!isVerbose) return;
      m_UI.UI_Clear(Color.Black);
      m_UI.UI_DrawStringBold(Color.White, "Generating game world... done!", 0, 0, new Color?());
      m_UI.UI_Repaint();
    }

    static private bool CheckIfExitIsGood(Map toMap, Point to)
    {
      return toMap.GetTileModelAt(to).IsWalkable;
    }

    static private void GenerateExit(Map fromMap, Point from, Map toMap, Point to)
    {
      fromMap.SetExitAt(from, new Exit(toMap, to));
    }

    private UniqueActor SpawnUniqueSewersThing(World world)
    {
      Map map = world[m_Rules.Roll(0, world.Size), m_Rules.Roll(0, world.Size)].SewersMap;
      Actor named = GameActors.SewersThing.CreateNamed(GameFactions.TheUndeads, "The Sewers Thing", false, 0);
      DiceRoller roller = new DiceRoller(map.Seed);
      if (!MapGenerator.ActorPlace(roller, map, named)) throw new InvalidOperationException("could not spawn unique The Sewers Thing");
      Zone zoneByPartialName = map.GetZoneByPartialName("Sewers Maintenance");
      if (zoneByPartialName != null)
        MapGenerator.MapObjectPlaceInGoodPosition(map, zoneByPartialName.Bounds, pt => {
           return map.IsWalkable(pt.X, pt.Y) && !map.HasActorAt(pt) && !map.HasItemsAt(pt);
        }, roller, pt => BaseMapGenerator.MakeObjBoard(GameImages.OBJ_BOARD, new string[7] {
          "TO SEWER WORKERS :",
          "- It lives here.",
          "- Do not disturb.",
          "- Approach with caution.",
          "- Watch your back.",
          "- In case of emergency, take refuge here.",
          "- Do not let other people interact with it!"
        }));
      return new UniqueActor(named,true);
    }

    private UniqueActor CreateUniqueBigBear()
    {
      Actor named = GameActors.MaleCivilian.CreateNamed(GameFactions.TheCivilians, "Big Bear", false, 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_BIG_BEAR);
      named.StartingSkill(Skills.IDs.HAULER,3);
      named.StartingSkill(Skills.IDs.HARDY,5);
      named.StartingSkill(Skills.IDs.STRONG,5);
      named.StartingSkill(Skills.IDs.TOUGH,5);
      named.Inventory.AddAll(new ItemMeleeWeapon(GameItems.UNIQUE_BIGBEAR_BAT));
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      return new UniqueActor(named,false,true, "You hear an angry man shouting 'FOOLS!'", GameMusics.BIGBEAR_THEME_SONG);
    }

    private UniqueActor CreateUniqueFamuFataru()
    {
      Actor named = GameActors.FemaleCivilian.CreateNamed(GameFactions.TheCivilians, "Famu Fataru", false, 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_FAMU_FATARU);
      named.StartingSkill(Skills.IDs.HAULER,3);
      named.StartingSkill(Skills.IDs.HARDY,5);
      named.StartingSkill(Skills.IDs._FIRST,5);
      named.StartingSkill(Skills.IDs.HIGH_STAMINA,5);
      named.Inventory.AddAll(new ItemMeleeWeapon(GameItems.UNIQUE_FAMU_FATARU_KATANA));
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      return new UniqueActor(named,false,true, "You hear a woman laughing.", GameMusics.FAMU_FATARU_THEME_SONG);
    }

    private UniqueActor CreateUniqueSantaman()
    {
      Actor named = GameActors.MaleCivilian.CreateNamed(GameFactions.TheCivilians, "Santaman", false, 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_SANTAMAN);
      named.StartingSkill(Skills.IDs.HAULER,3);
      named.StartingSkill(Skills.IDs.HARDY,5);
      named.StartingSkill(Skills.IDs.AWAKE,5);
      named.StartingSkill(Skills.IDs.FIREARMS,5);
      named.Inventory.AddAll(new ItemRangedWeapon(GameItems.UNIQUE_SANTAMAN_SHOTGUN));
      named.Inventory.AddAll(BaseMapGenerator.MakeItemShotgunAmmo());
      named.Inventory.AddAll(BaseMapGenerator.MakeItemShotgunAmmo());
      named.Inventory.AddAll(BaseMapGenerator.MakeItemShotgunAmmo());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      return new UniqueActor(named,false,true, "You hear christmas music and drunken vomiting.", GameMusics.SANTAMAN_THEME_SONG);
    }

    private UniqueActor CreateUniqueRoguedjack()
    {
      Actor named = GameActors.MaleCivilian.CreateNamed(GameFactions.TheCivilians, "Roguedjack", false, 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_ROGUEDJACK);
      named.StartingSkill(Skills.IDs.HAULER,3);
      named.StartingSkill(Skills.IDs.HARDY,5);
      named.StartingSkill(Skills.IDs.LEADERSHIP,5);
      named.StartingSkill(Skills.IDs.CHARISMATIC,5);
      named.Inventory.AddAll(new ItemMeleeWeapon(GameItems.UNIQUE_ROGUEDJACK_KEYBOARD));
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      return new UniqueActor(named,false,true, "You hear a man shouting in French.", GameMusics.ROGUEDJACK_THEME_SONG);
    }

    private UniqueActor CreateUniqueDuckman()
    {
      Actor named = GameActors.MaleCivilian.CreateNamed(GameFactions.TheCivilians, "Duckman", false, 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_DUCKMAN);
      named.StartingSkill(Skills.IDs.CHARISMATIC,5);
      named.StartingSkill(Skills.IDs.LEADERSHIP);
      named.StartingSkill(Skills.IDs.STRONG,5);
      named.StartingSkill(Skills.IDs.HIGH_STAMINA,5);
      named.StartingSkill(Skills.IDs.MARTIAL_ARTS,5);
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      return new UniqueActor(named,false,true, "You hear loud demented QUACKS.", GameMusics.DUCKMAN_THEME_SONG);
    }

    private UniqueActor CreateUniqueHansVonHanz()
    {
      Actor named = GameActors.MaleCivilian.CreateNamed(GameFactions.TheCivilians, "Hans von Hanz", false, 0);
      named.IsUnique = true;
      named.Doll.AddDecoration(DollPart.SKIN, GameImages.ACTOR_HANS_VON_HANZ);
      named.StartingSkill(Skills.IDs.HAULER,3);
      named.StartingSkill(Skills.IDs.FIREARMS,5);
      named.StartingSkill(Skills.IDs.LEADERSHIP,5);
      named.StartingSkill(Skills.IDs.NECROLOGY,5);
      named.Inventory.AddAll(new ItemRangedWeapon(GameItems.UNIQUE_HANS_VON_HANZ_PISTOL));
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      named.Inventory.AddAll(m_TownGenerator.MakeItemCannedFood());
      return new UniqueActor(named,false,true, "You hear a man barking orders in German.",GameMusics.HANS_VON_HANZ_THEME_SONG);
    }

    private UniqueItem SpawnUniqueSubwayWorkerBadge(World world)
    {
      Item it = new Item(GameItems.UNIQUE_SUBWAY_BADGE);
      // we intentionally do not take advantage of the current subway layout algorithm
      List<Map> mapList = new List<Map>();
      for (int index1 = 0; index1 < world.Size; ++index1) {
        for (int index2 = 0; index2 < world.Size; ++index2) {
          if (world[index1, index2].HasSubway)
            mapList.Add(world[index1, index2].SubwayMap);
        }
      }
      if (mapList.Count == 0)
        return new UniqueItem{
          TheItem = it,
          IsSpawned = false
        };
      Map map = mapList[m_Rules.Roll(0, mapList.Count)];
      Rectangle bounds = map.GetZoneByPartialName("rails").Bounds;
      Point point = new Point(m_Rules.Roll(bounds.Left, bounds.Right), m_Rules.Roll(bounds.Top, bounds.Bottom));
      map.DropItemAt(it, point);
      map.AddDecorationAt("Tiles\\Decoration\\bloodied_floor", point);
      return new UniqueItem{
        TheItem = it,
        IsSpawned = true
      };
    }

    private UniqueMap CreateUniqueMap_CHARUndegroundFacility(World world)
    {
      List<District> districtList = null;
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
      District district = districtList[m_Rules.Roll(0, districtList.Count)];
      List<Zone> zoneList = new List<Zone>();
      foreach (Zone zone in district.EntryMap.Zones)
      {
        if (zone.HasGameAttribute("CHAR Office"))
          zoneList.Add(zone);
      }
      Zone officeZone = zoneList[m_Rules.Roll(0, zoneList.Count)];
      Map mapCharUnderground = m_TownGenerator.GenerateUniqueMap_CHARUnderground(district.EntryMap, officeZone);
      district.AddUniqueMap(mapCharUnderground);
      return new UniqueMap(mapCharUnderground);
    }

    private DistrictKind GenerateDistrictKind(int gridX, int gridY)
    {
      if (gridX == 0 && gridY == 0) return DistrictKind.BUSINESS;
      return (DistrictKind) m_Rules.Roll(0, (int) DistrictKind._COUNT);
    }

    private void GenerateDistrictSewersMap(District district)
    {
      m_TownGenerator.GenerateSewersMap(district.EntryMap.Seed << 1 ^ district.EntryMap.Seed, district);
    }

    private void GenerateDistrictSubwayMap(District district)
    {
      m_TownGenerator.GenerateSubwayMap(district.EntryMap.Seed << 2 ^ district.EntryMap.Seed, district);
    }

    private void GeneratePlayerOnMap(Map map, BaseTownGenerator townGen)
    {
      DiceRoller roller = new DiceRoller(map.Seed);
      Actor actor;
      if (m_CharGen.IsUndead)
      {
        switch (m_CharGen.UndeadModel)
        {
          case GameActors.IDs.UNDEAD_SKELETON:
            actor = GameActors.Skeleton.CreateNumberedName(GameFactions.TheUndeads, 0);
            break;
          case GameActors.IDs.UNDEAD_ZOMBIE:
            actor = GameActors.Zombie.CreateNumberedName(GameFactions.TheUndeads, 0);
            break;
          case GameActors.IDs.UNDEAD_ZOMBIE_MASTER:
            actor = GameActors.ZombieMaster.CreateNumberedName(GameFactions.TheUndeads, 0);
            break;
          case GameActors.IDs.UNDEAD_MALE_ZOMBIFIED:
          case GameActors.IDs.UNDEAD_FEMALE_ZOMBIFIED:
            Actor anonymous = (m_CharGen.UndeadModel.IsFemale() ? GameActors.FemaleCivilian : GameActors.MaleCivilian).CreateAnonymous(GameFactions.TheCivilians, 0);
            BaseMapGenerator.DressCivilian(roller, anonymous);
            BaseMapGenerator.GiveNameToActor(roller, anonymous);
            actor = Zombify(null, anonymous, true);
            break;
          default:
            throw new ArgumentOutOfRangeException("unhandled undeadModel");
        }
        actor.PrepareForPlayerControl();
      } else {
        actor = (m_CharGen.IsMale ? GameActors.MaleCivilian : GameActors.FemaleCivilian).CreateAnonymous(GameFactions.TheCivilians, 0);
        BaseMapGenerator.DressCivilian(roller, actor);
        BaseMapGenerator.GiveNameToActor(roller, actor);
        actor.SkillUpgrade(m_CharGen.StartingSkill);
        actor.RecomputeStartingStats();
        actor.CreateCivilianDeductFoodSleep(m_Rules);
      }
      actor.Controller = new PlayerController();
      if (   MapGenerator.ActorPlace(roller, map, actor, pt => {
               if (map.IsInsideAt(pt)) {
                 if (m_CharGen.IsUndead) return false;
               } else {
                 if (!m_CharGen.IsUndead) return false;
               }
               if (IsInCHAROffice(new Location(map, pt))) return false;
               MapObject mapObjectAt = map.GetMapObjectAt(pt);
               if (m_CharGen.IsUndead) return mapObjectAt == null;
               if (mapObjectAt != null) return mapObjectAt.IsCouch;
               return false;
             })
          || MapGenerator.ActorPlace(roller, map, actor, pt => {
              if (map.IsInsideAt(pt)) return !IsInCHAROffice(new Location(map, pt));
              return false;
             }))
        return;
      do;
      while (!MapGenerator.ActorPlace(roller, map, actor, pt => !RogueGame.IsInCHAROffice(new Location(map, pt))));
    }

    private void RefreshPlayer()
    {
      foreach (Actor actor in Session.Get.CurrentMap.Actors) {
        if (actor.IsPlayer) {
          m_Player = actor;
          break;
        }
      }
      if (m_Player == null) return;
      ComputeViewRect(m_Player.Location.Position);
    }

    private void SetCurrentMap(Map map)
    {
      Session.Get.CurrentMap = map;
      if (map == map.District.SewersMap) {
        m_MusicManager.StopAll();
        m_MusicManager.PlayLooping(GameMusics.SEWERS);
      } else if (map == map.District.SubwayMap) {
        m_MusicManager.StopAll();
        m_MusicManager.PlayLooping(GameMusics.SUBWAY);
      } else if (map == Session.Get.UniqueMaps.Hospital_Admissions.TheMap || map == Session.Get.UniqueMaps.Hospital_Offices.TheMap || (map == Session.Get.UniqueMaps.Hospital_Patients.TheMap || map == Session.Get.UniqueMaps.Hospital_Power.TheMap) || map == Session.Get.UniqueMaps.Hospital_Storage.TheMap) {
        m_MusicManager.StopAll();
        m_MusicManager.PlayLooping(GameMusics.HOSPITAL);
      } else {
        m_MusicManager.Stop(GameMusics.SEWERS);
        m_MusicManager.Stop(GameMusics.SUBWAY);
        m_MusicManager.Stop(GameMusics.HOSPITAL);
      }
    }

    // looks good in single-player but not really honest with the no-skew scheduler (and possibly can mess it up)
    // problems with turn skew should be handled in simulation (BeforePlayerEnterDistrict)
    private void OnPlayerLeaveDistrict()
    {
//    Session.Get.CurrentMap.LocalTime.TurnCounter = Session.Get.WorldTime.TurnCounter;
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void BeforePlayerEnterDistrict(District district)
    {
      if (Session.Get.World.PlayerDistricts.Contains(district)) return; // do not simulate districts with PCs
      Map entryMap = district.EntryMap;
      int turnCounter = entryMap.LocalTime.TurnCounter;
        m_MusicManager.StopAll();
        m_MusicManager.Play(GameMusics.INTERLUDE);
        StopSimThread();
        lock (m_SimMutex) {
          double totalMilliseconds1 = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
          // The no-skew scheduling should mean the following is not necessary.
          // a district after the PC's will run in order.  It's ok for the turn counter to not increment.
          // a district before the PCs will already be "current"
#if FAIL
          double num1 = 0.0;
          bool flag = false;
          while (entryMap.LocalTime.TurnCounter <= Session.Get.WorldTime.TurnCounter) {
            double totalMilliseconds2 = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
            if (entryMap.LocalTime.TurnCounter == Session.Get.WorldTime.TurnCounter || entryMap.LocalTime.TurnCounter == turnCounter || totalMilliseconds2 >= num1 + 1000.0) {
              num1 = totalMilliseconds2;
              ClearMessages();
              AddMessage(new Data.Message(string.Format("Simulating district, please wait {0}/{1}...", (object)entryMap.LocalTime.TurnCounter, (object)Session.Get.WorldTime.TurnCounter), Session.Get.WorldTime.TurnCounter, Color.White));
              AddMessage(new Data.Message("(this is an option you can tune)", Session.Get.WorldTime.TurnCounter, Color.White));
              int num2 = entryMap.LocalTime.TurnCounter - turnCounter;
              if (num2 > 1) {
                int num3 = Session.Get.WorldTime.TurnCounter - entryMap.LocalTime.TurnCounter;
                double num4 = 1000.0 * (double)num2 / (1.0 + totalMilliseconds2 - totalMilliseconds1);
                AddMessage(new Data.Message(string.Format("Turns per second    : {0:F2}.", (object)num4), Session.Get.WorldTime.TurnCounter, Color.White));
                int num5 = (int)((double)num3 / num4);
                int num6 = num5 / 60;
                int num7 = num5 % 60;
                AddMessage(new Data.Message(string.Format("Estimated time left : {0}.", num6 > 0 ? (object)string.Format("{0} min {1:D2} secs", (object)num6, (object)num7) : (object)string.Format("{0} secs", (object)num7)), Session.Get.WorldTime.TurnCounter, Color.White));
              }
              if (flag)
                AddMessage(new Data.Message("Simulation aborted!", Session.Get.WorldTime.TurnCounter, Color.Red));
              else
                AddMessage(new Data.Message("<keep ESC pressed to abort the simulation>", Session.Get.WorldTime.TurnCounter, Color.Yellow));
              RedrawPlayScreen();
              if (!m_MusicManager.IsPlaying(GameMusics.INTERLUDE))
                m_MusicManager.Play(GameMusics.INTERLUDE);
            }
            if (flag) break;

            KeyEventArgs keyEventArgs = m_UI.UI_PeekKey();
            if (keyEventArgs != null && keyEventArgs.KeyCode == Keys.Escape) {
              foreach (Map map in district.Maps)
                map.LocalTime.TurnCounter = Session.Get.WorldTime.TurnCounter;
              flag = true;
            }
            if (!flag) SimulateDistrict(district);
          }
#endif
        }
        RestartSimThread();
        RemoveLastMessage();
        m_MusicManager.StopAll();
    }

    private SimFlags ComputeSimFlagsForTurn(int turn)
    {
      return SimFlags.HIDETAIL_TURN;
    }

    private bool SimulateNearbyDistricts(District d)
    {
      District d1 = Session.Get.World.CurrentSimulationDistrict();
      if (null == d1) return false;
      AdvancePlay(d1, ComputeSimFlagsForTurn(d.EntryMap.LocalTime.TurnCounter));    // void SimulateDistrict(d1) if becomes complicated again
      Session.Get.World.ScheduleAdjacentForAdvancePlay(d1);
      return true;
    }

    private void RestartSimThread()
    {
      StopSimThread();
      StartSimThread();
    }

    private void StartSimThread()
    {
      if (m_SimThread == null) {
        m_SimThread = new Thread(new ThreadStart(SimThreadProc)) {
          Name = "Simulation Thread"
        };
      }
      m_SimThread.Start();
    }

    private void StopSimThread()
    {
      if (m_SimThread == null) return;
      m_SimThread.Abort();
      m_SimThread = null;
    }

    private void SimThreadProc()
    {
#if DEBUG
        bool have_simulated = false;
        while (m_SimThread.IsAlive) {
          lock (m_SimMutex) {
            have_simulated = (m_Player != null ? SimulateNearbyDistricts(m_Player.Location.Map.District) : false);
          }
          if (!have_simulated) Thread.Sleep(200);
        }
#else
      try {
        bool have_simulated = false;
        while (m_SimThread.IsAlive) {
          lock (m_SimMutex) {
            have_simulated = (m_Player != null ? SimulateNearbyDistricts(m_Player.Location.Map.District) : false);
          }
          if (!have_simulated) Thread.Sleep(200);
        }
      } catch (Exception ex) {
        if (ex is ThreadAbortException) return; // this is from the Abort() call
        using (Bugreport bugreport = new Bugreport(ex)) {
          int num = (int) bugreport.ShowDialog();
        }
        // It is exceptionally difficult to completely shut down Rogue Survivor Revived at this point.
        // RogueSurvivor.vshost.exe doesn't completely go away.
        Application.Exit();
        // Thread.CurrentThread.Abort();    // makes RogueSurvivor.exe also stay around
        // return;  // no-op for not-so-obvious reasons
      }
#endif
    }

    private void ShowNewAchievement(Achievement.IDs id)
    {
      Session.Get.Scoring.SetCompletedAchievement(id);
      ++Session.Get.Scoring.CompletedAchievementsCount;
      Achievement achievement = Session.Get.Scoring.GetAchievement(id);
      string musicId = achievement.MusicID;
      string name = achievement.Name;
      string[] text = achievement.Text;
      Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format("** Achievement : {0} for {1} points. **", name, achievement.ScoreValue));
      m_MusicManager.StopAll();
      m_MusicManager.Play(musicId);
      string str = new string('*', Math.Max(FindLongestLine(text), 50));
      List<string> stringList = new List<string>(text.Length + 3 + 2){
        str,
        string.Format("ACHIEVEMENT : {0}", name),
        "CONGRATULATIONS!"
      };
      for (int index = 0; index < text.Length; ++index)
        stringList.Add(text[index]);
      stringList.Add(string.Format("Achievements : {0}/{1}.", Session.Get.Scoring.CompletedAchievementsCount, (int)Achievement.IDs._COUNT));
      stringList.Add(str);
      Point screenPos = new Point(0, 0);
      AddOverlay(new RogueGame.OverlayPopup(stringList.ToArray(), Color.Gold, Color.Gold, Color.DimGray, screenPos));
      ClearMessages();
      AddMessagePressEnter();
      ClearOverlays();
    }

    private void ShowSpecialDialogue(Actor speaker, string[] text)
    {
      m_MusicManager.StopAll();
      m_MusicManager.Play(GameMusics.INTERLUDE);
      AddOverlay(new OverlayPopup(text, Color.Gold, Color.Gold, Color.DimGray, new Point(0, 0)));
      AddOverlay(new OverlayRect(Color.Yellow, new Rectangle(MapToScreen(speaker.Location), SIZE_OF_ACTOR)));
      ClearMessages();
      AddMessagePressEnter();
      m_MusicManager.StopAll();
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    private void CheckSpecialPlayerEventsAfterAction(Actor player)
    { // XXX player is always m_Player here.
      if (!player.Model.Abilities.IsUndead && player.Faction != GameFactions.TheCHARCorporation && (!Session.Get.Scoring.HasCompletedAchievement(Achievement.IDs.CHAR_BROKE_INTO_OFFICE) && RogueGame.IsInCHAROffice(player.Location)))
        ShowNewAchievement(Achievement.IDs.CHAR_BROKE_INTO_OFFICE);
      if (!Session.Get.Scoring.HasCompletedAchievement(Achievement.IDs.CHAR_FOUND_UNDERGROUND_FACILITY) && player.Location.Map == Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap) {
        lock (Session.Get) {
          ShowNewAchievement(Achievement.IDs.CHAR_FOUND_UNDERGROUND_FACILITY);
          Session.Get.PlayerKnows_CHARUndergroundFacilityLocation = true;
          Session.Get.CHARUndergroundFacility_Activated = true;
          Map CHARmap = Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap;

          CHARmap.Expose();
          Map surfaceMap = Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap.District.EntryMap;
		  // XXX reduced to integrity checking by Exit constructor adjustment
          if (!surfaceMap.Rect.Any(pt => {
              Exit exitAt = surfaceMap.GetExitAt(pt);
              if (exitAt == null) return false;
              return exitAt.ToMap == CHARmap;
            }))
            throw new InvalidOperationException("could not find exit to CUF in surface map");
          if (!Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap.Rect.Any(pt =>
            {
              Exit exitAt = CHARmap.GetExitAt(pt);
              if (exitAt == null) return false;
              return exitAt.ToMap == surfaceMap;
            }))
            throw new InvalidOperationException("could not find exit to surface in CUF map");
        }
      }
      Actor vip = player.Sees(Session.Get.UniqueActors.TheSewersThing.TheActor);
      if (null != vip && !Session.Get.PlayerKnows_TheSewersThingLocation) {
        lock (Session.Get) {
          Session.Get.PlayerKnows_TheSewersThingLocation = true;
          m_MusicManager.StopAll();
          m_MusicManager.Play(GameMusics.FIGHT);
          ClearMessages();
          AddMessage(new Data.Message("Hey! What's that THING!?", Session.Get.WorldTime.TurnCounter, Color.Yellow));
          AddMessagePressEnter();
        }
      }
      // The Prisoner Who Should Not Be should only respond to civilian players; other factions should either be hostile, or colluding on
      // the fake charges used to frame him (CHAR, possibly police), or conned (possibly police)
      // Acceptable factions are civilians and survivors.
      // Even if the player is of an unacceptable faction, he will be thanked if not an enemy.
      if (player.Location.Map == Session.Get.UniqueMaps.PoliceStation_JailsLevel.TheMap && !Session.Get.UniqueActors.PoliceStationPrisonner.TheActor.IsDead)
      {
        Actor theActor = Session.Get.UniqueActors.PoliceStationPrisonner.TheActor;
        Map map = player.Location.Map;
        switch (Session.Get.ScriptStage_PoliceStationPrisoner)
        {
          case 0:
            if (map.HasAnyAdjacentInMap(player.Location.Position, pt => map.GetMapObjectAt(pt) is PowerGenerator) && !theActor.IsSleeping)
            {
              lock (Session.Get)
              {
                string[] local_6 = null;
                if (player.Faction == GameFactions.TheCivilians || player.Faction == GameFactions.TheSurvivors) {
                  local_6 = new string[13] {    // standard message
                    "\" Psssst! Hey! You over there! \"",
                    string.Format("{0} is discreetly calling you from {1} cell. You listen closely...",  theActor.Name,  HisOrHer(theActor)),
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
                    string.Format("Looks like {0} wants you to turn the generator on to open the cells...",  HeOrShe(theActor))
                  };
                }
#if FAIL
                if (player.Faction == GameFactions.ThePolice) {
                }
                if (player.Faction == GameFactions.TheCHARCorporation) {
                }
#endif
                if (null != local_6) {
                  ShowSpecialDialogue(theActor, local_6);
                  Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format("{0} offered a deal.", theActor.Name));
                }
                Session.Get.ScriptStage_PoliceStationPrisoner = 1;
                break;
              }
            }
            else
              break;
          case 1:
            if (!map.HasZonePartiallyNamedAt(theActor.Location.Position, "jail") && Rules.IsAdjacent(player.Location.Position, theActor.Location.Position) && !theActor.IsSleeping && !theActor.IsEnemyOf(player)) {
              lock (Session.Get) {
                string[] local_7 = new string[8]
                {
                  "\" Thank you! Thank you so much!",
                  "  As promised, I'll tell you the big secret!",
                  string.Format("  The CHAR Underground Facility is in district {0}.",  World.CoordToString(Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap.District.WorldPosition.X, Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap.District.WorldPosition.Y)),
                  "  Look for a CHAR Office, a room with an iron door.",
                  "  Now I must hurry! Thanks a lot for saving me!",
                  "  I don't want them to... UGGH...",
                  "  What's happening? NO!",
                  "  NO NOT ME! aAAAAAaaaa! NOT NOW! AAAGGGGGGGRRR \""
                };
                ShowSpecialDialogue(theActor, local_7);
                Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format("Freed {0}.", theActor.Name));
                Session.Get.PlayerKnows_CHARUndergroundFacilityLocation = true;
                Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, "Learned the location of the CHAR Underground Facility.");
                KillActor(null, theActor, "transformation");
                Actor local_8 = Zombify(null, theActor, false);
                local_8.Model = GameActors.ZombiePrince;
                local_8.ActionPoints = 0;   // this was warned, player should get the first move
                Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format("{0} turned into a {1}!", theActor.Name, local_8.Model.Name));
                m_MusicManager.Play(GameMusics.FIGHT);
                Session.Get.ScriptStage_PoliceStationPrisoner = 2;
                break;
              }
            }
            else
              break;
          case 2: break;
          default:
            throw new ArgumentOutOfRangeException("unhandled script stage " + Session.Get.ScriptStage_PoliceStationPrisoner.ToString());
        }
      }
      vip = player.Sees(Session.Get.UniqueActors.JasonMyers.TheActor);
      if (null != vip) {
        if (!m_MusicManager.IsPlaying(GameMusics.INSANE)) {
          m_MusicManager.StopAll();
          m_MusicManager.Play(GameMusics.INSANE);
        }
        lock (Session.Get) {
          if (!Session.Get.Scoring.HasSighted(Session.Get.UniqueActors.JasonMyers.TheActor.Model.ID)) {
            ClearMessages();
            AddMessage(new Data.Message("Nice axe you have there!", Session.Get.WorldTime.TurnCounter, Color.Yellow));
            AddMessagePressEnter();
          }
        }
      }
      if (player != Session.Get.UniqueActors.Duckman.TheActor) {
        if (!Session.Get.UniqueActors.Duckman.TheActor.IsDead && IsVisibleToPlayer(Session.Get.UniqueActors.Duckman.TheActor)) {
          if (!m_MusicManager.IsPlaying(GameMusics.DUCKMAN_THEME_SONG))
            m_MusicManager.Play(GameMusics.DUCKMAN_THEME_SONG);
        } else if (m_MusicManager.IsPlaying(GameMusics.DUCKMAN_THEME_SONG))
          m_MusicManager.Stop(GameMusics.DUCKMAN_THEME_SONG);
      }
      if (Session.Get.UniqueItems.TheSubwayWorkerBadge.TheItem.IsEquipped && (player.Location.Map == player.Location.Map.District.SubwayMap && player.Inventory.Contains(Session.Get.UniqueItems.TheSubwayWorkerBadge.TheItem)))
      {
        Map map = player.Location.Map;
        if (map.HasAnyAdjacentInMap(player.Location.Position, pt => {
          MapObject mapObjectAt = map.GetMapObjectAt(pt);
          if (mapObjectAt == null) return false;
          return mapObjectAt.ImageID == GameImages.OBJ_GATE_CLOSED;
        })) {
          DoTurnAllGeneratorsOn(map);
          AddMessage(new Data.Message("The gate system scanned your badge and turned the power on!", Session.Get.WorldTime.TurnCounter, Color.Green));
        }
      }
      if (!Session.Get.Scoring.HasVisited(player.Location.Map)) {
        Session.Get.Scoring.AddVisit(Session.Get.WorldTime.TurnCounter, player.Location.Map);
        Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format("Visited {0}.", player.Location.Map.Name));
      }
      foreach (Point position in m_Player.Controller.FOV) {
        Actor actorAt = player.Location.Map.GetActorAt(position);
        if (actorAt != null && actorAt != player)
          Session.Get.Scoring.AddSighting(actorAt.Model.ID, Session.Get.WorldTime.TurnCounter);
      }
    }

    private void HandleReincarnation()
    {
      if (s_Options.MaxReincarnations <= 0 || !AskForReincarnation()) {
        m_MusicManager.StopAll();
        return;
      }

      m_MusicManager.Play(GameMusics.LIMBO);
      m_UI.UI_Clear(Color.Black);
      m_UI.UI_DrawStringBold(Color.Yellow, "Reincarnation - Purgatory", 0, 0, new Color?());
      m_UI.UI_DrawStringBold(Color.White, "(preparing reincarnations, please wait...)", 0, 28, new Color?());
      m_UI.UI_Repaint();
      Actor[] reincarnationAvatars = {
        FindReincarnationAvatar(GameOptions.ReincMode.RANDOM_ACTOR, out int matchingActors1),
        FindReincarnationAvatar(GameOptions.ReincMode.RANDOM_LIVING, out int matchingActors2),
        FindReincarnationAvatar(GameOptions.ReincMode.RANDOM_UNDEAD, out int matchingActors3),
        FindReincarnationAvatar(GameOptions.ReincMode.RANDOM_FOLLOWER, out int matchingActors4),
        FindReincarnationAvatar(GameOptions.ReincMode.KILLER, out matchingActors1),
        FindReincarnationAvatar(GameOptions.ReincMode.ZOMBIFIED, out matchingActors1)
      };
      string[] strArray = CompileDistrictFunFacts(m_Player.Location.Map.District);
      string[] entries = new string[(int)GameOptions.ReincMode._COUNT] {
        GameOptions.Name(GameOptions.ReincMode.RANDOM_ACTOR),
        GameOptions.Name(GameOptions.ReincMode.RANDOM_LIVING),
        GameOptions.Name(GameOptions.ReincMode.RANDOM_UNDEAD),
        GameOptions.Name(GameOptions.ReincMode.RANDOM_FOLLOWER),
        GameOptions.Name(GameOptions.ReincMode.KILLER),
        GameOptions.Name(GameOptions.ReincMode.ZOMBIFIED)
      };
      string[] values = reincarnationAvatars.Select(a => DescribeAvatar(a)).ToArray();
      values[1] = string.Format("{0}   (out of {1} possibilities)", values[1], matchingActors2);
      values[2] = string.Format("{0}   (out of {1} possibilities)", values[2], matchingActors3);
      values[3] = string.Format("{0}   (out of {1} possibilities)", values[3], matchingActors4);
      const int gx = 0;

      Actor newPlayerAvatar = null;
      Func<int,bool?> setup_handler = (currentChoice => {
        int gy1 = 0;
        m_UI.UI_Clear(Color.Black);
        m_UI.UI_DrawStringBold(Color.Yellow, "Reincarnation - Choose Avatar", gx, gy1, new Color?());
        gy1 += 28;
        DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGreen, values, gx, ref gy1, 256);
        gy1 += 28;
        m_UI.UI_DrawStringBold(Color.Pink, ".-* District Fun Facts! *-.", gx, gy1, new Color?());
        gy1 += 14;
        m_UI.UI_DrawStringBold(Color.Pink, string.Format("at current date : {0}.", new WorldTime(Session.Get.WorldTime.TurnCounter).ToString()), gx, gy1, new Color?());
        int gy4 = gy1 + 28;
        for (int index = 0; index < strArray.Length; ++index) {
          m_UI.UI_DrawStringBold(Color.Pink, strArray[index], gx, gy4, new Color?());
          gy4 += 14;
        }
        DrawFootnote(Color.White, "cursor to move, ENTER to select, ESC to cancel and end game");
        return null;
      });
      Func<int, bool?> choice_handler = (currentChoice => {
        newPlayerAvatar = reincarnationAvatars[currentChoice];
        if (null != newPlayerAvatar) return true;
        return null;
      });
      if (!ChoiceMenu(choice_handler, setup_handler, entries.Length)) newPlayerAvatar = null;
      if (newPlayerAvatar == null) {
        m_MusicManager.StopAll();
        return;
      }

      newPlayerAvatar.Controller = new PlayerController();
      if (newPlayerAvatar.Activity != Activity.SLEEPING) newPlayerAvatar.Activity = Activity.IDLE;
      newPlayerAvatar.PrepareForPlayerControl();
      m_Player = newPlayerAvatar;
      Session.Get.CurrentMap = newPlayerAvatar.Location.Map;
      Session.Get.Scoring.StartNewLife(Session.Get.WorldTime.TurnCounter);
      Session.Get.Scoring.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format("(reincarnation {0})", Session.Get.Scoring.ReincarnationNumber));
      Session.Get.Scoring.Side = m_Player.Model.Abilities.IsUndead ? DifficultySide.FOR_UNDEAD : DifficultySide.FOR_SURVIVOR;
      Session.Get.Scoring.DifficultyRating = Scoring.ComputeDifficultyRating(s_Options, Session.Get.Scoring.Side, Session.Get.Scoring.ReincarnationNumber);
      // Historically, reincarnation completely wiped the is-visited memory.  We get that for free by constructing a new PlayerController.
      // This may not be a useful idea, however.
      m_MusicManager.StopAll();
      m_Player.Controller.UpdateSensors();
      ComputeViewRect(m_Player.Location.Position);
      ClearMessages();
      AddMessage(new Data.Message(string.Format("{0} feels disoriented for a second...", m_Player.Name), Session.Get.WorldTime.TurnCounter, Color.Yellow));
      RedrawPlayScreen();
      string musicname = GameMusics.REINCARNATE;
      // XXX feels gappy ... there are other uniques with music
      if (m_Player == Session.Get.UniqueActors.JasonMyers.TheActor) musicname = GameMusics.INSANE;
      m_MusicManager.Play(musicname);
      RestartSimThread();
    }

    static private string DescribeAvatar(Actor a)
    {
      if (a == null) return "(N/A)";
      bool flag = a.CountFollowers > 0;
      bool hasLeader = a.HasLeader;
      return string.Format("{0}, a {1}{2}", a.Name, a.Model.Name, flag ? ", leader" : (hasLeader ? ", follower" : ""));
    }

    private bool AskForReincarnation()
    {
      int gy1;
      int gx = gy1 = 0;
      m_UI.UI_Clear(Color.Black);
      m_UI.UI_DrawStringBold(Color.Yellow, "Limbo", gx, gy1, new Color?());
      int gy2 = gy1 + 28;
      m_UI.UI_DrawStringBold(Color.White, string.Format("Leave body {0}/{1}.", 1 + Session.Get.Scoring.ReincarnationNumber, 1 + s_Options.MaxReincarnations), gx, gy2, new Color?());
      int gy3 = gy2 + 14;
      m_UI.UI_DrawStringBold(Color.White, "Remember lives.", gx, gy3, new Color?());
      int gy4 = gy3 + 14;
      m_UI.UI_DrawStringBold(Color.White, "Remember purpose.", gx, gy4, new Color?());
      int gy5 = gy4 + 14;
      m_UI.UI_DrawStringBold(Color.White, "Clear again.", gx, gy5, new Color?());
      int gy6 = gy5 + 14;
      if (Session.Get.Scoring.ReincarnationNumber >= s_Options.MaxReincarnations) {
        m_UI.UI_DrawStringBold(Color.LightGreen, "Humans interesting.", gx, gy6, new Color?());
        int gy7 = gy6 + 14;
        m_UI.UI_DrawStringBold(Color.LightGreen, "Time to leave.", gx, gy7, new Color?());
        int gy8 = gy7 + 14 + 28;
        m_UI.UI_DrawStringBold(Color.Yellow, "No more reincarnations left.", gx, gy8, new Color?());
        DrawFootnote(Color.White, "press ENTER");
        m_UI.UI_Repaint();
        WaitEnter();
        return false;
      }
      m_UI.UI_DrawStringBold(Color.White, "Leave?", gx, gy6, new Color?());
      int gy9 = gy6 + 14;
      m_UI.UI_DrawStringBold(Color.White, "Live?", gx, gy9, new Color?());
      int gy10 = gy9 + 28;
      m_UI.UI_DrawStringBold(Color.Yellow, "Reincarnate? Y to confirm, N to cancel.", gx, gy10, new Color?());
      m_UI.UI_Repaint();
      return WaitYesOrNo();
    }

    static private bool IsSuitableReincarnation(Actor a, bool asLiving)
    {
      if (a == null || a.IsDead || a.IsPlayer || a.Location.Map.District != Session.Get.CurrentMap.District || (a.Location.Map == Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap || a == Session.Get.UniqueActors.PoliceStationPrisonner.TheActor || a.Location.Map == a.Location.Map.District.SewersMap))
        return false;
      if (asLiving)
        return !a.Model.Abilities.IsUndead && (!s_Options.IsLivingReincRestricted || a.Faction == GameFactions.TheCivilians);
      return a.Model.Abilities.IsUndead && (s_Options.CanReincarnateAsRat || a.Model != GameActors.RatZombie);
    }

    private Actor FindReincarnationAvatar(GameOptions.ReincMode reincMode, out int matchingActors)
    {
      switch (reincMode) {
        case GameOptions.ReincMode.RANDOM_FOLLOWER:
          if (Session.Get.Scoring.FollowersWhendDied == null) {
            matchingActors = 0;
            return null;
          }
          List<Actor> actorList1 = new List<Actor>(Session.Get.Scoring.FollowersWhendDied.Count);
          foreach (Actor a in Session.Get.Scoring.FollowersWhendDied) {
            if (IsSuitableReincarnation(a, true))
              actorList1.Add(a);
          }
          matchingActors = actorList1.Count;
          if (actorList1.Count == 0) return null;
          return actorList1[m_Rules.Roll(0, actorList1.Count)];
        case GameOptions.ReincMode.KILLER:
          Actor killer = Session.Get.Scoring.Killer;
          if (IsSuitableReincarnation(killer, true) || IsSuitableReincarnation(killer, false)) {
            matchingActors = 1;
            return killer;
          }
          matchingActors = 0;
          return null;
        case GameOptions.ReincMode.ZOMBIFIED:
          Actor zombifiedPlayer = Session.Get.Scoring.ZombifiedPlayer;
          if (IsSuitableReincarnation(zombifiedPlayer, false)) {
            matchingActors = 1;
            return zombifiedPlayer;
          }
          matchingActors = 0;
          return null;
        case GameOptions.ReincMode.RANDOM_LIVING:
        case GameOptions.ReincMode.RANDOM_UNDEAD:
        case GameOptions.ReincMode.RANDOM_ACTOR:
          bool asLiving = reincMode == GameOptions.ReincMode.RANDOM_LIVING || reincMode == GameOptions.ReincMode.RANDOM_ACTOR && m_Rules.RollChance(50);
          List<Actor> actorList2 = new List<Actor>();
          // prior implementation iterated through all districts even though IsSuitableReincarnation requires m_Session.CurrentMap.District
          foreach (Map map in Session.Get.CurrentMap.District.Maps) {
            foreach (Actor actor in map.Actors) {
              if (IsSuitableReincarnation(actor, asLiving))
                actorList2.Add(actor);
            }
          }
          matchingActors = actorList2.Count;
          if (actorList2.Count == 0) return null;
          return actorList2[m_Rules.Roll(0, actorList2.Count)];
        default:
          throw new ArgumentOutOfRangeException("unhandled reincarnation mode " + reincMode.ToString());
      }
    }

    private ActorAction GenerateInsaneAction(Actor actor)
    {
      switch (m_Rules.Roll(0, 5)) {
        case 0:
          return new ActionShout(actor, "AAAAAAAAAAA!!!");
        case 1:
          return new ActionBump(actor, m_Rules.RollDirection());
        case 2:
          Direction direction = m_Rules.RollDirection();
          MapObject mapObjectAt = actor.Location.Map.GetMapObjectAt(actor.Location.Position + direction);
          if (mapObjectAt == null) return null;
          return new ActionBreak(actor, mapObjectAt);
        case 3:
          Inventory inventory = actor.Inventory;
          if (inventory == null || inventory.CountItems == 0) return null;
          Item it = inventory[m_Rules.Roll(0, inventory.CountItems)];
          ActionUseItem actionUseItem = new ActionUseItem(actor, it);
          if (actionUseItem.IsLegal()) return actionUseItem;
          if (it.IsEquipped) return new ActionUnequipItem(actor, it);
          return new ActionDropItem(actor, it);
        case 4:
          int maxRange = actor.FOVrange(actor.Location.Map.LocalTime, Session.Get.World.Weather);
          foreach (Actor actor1 in actor.Location.Map.Actors) {
            if (actor1 != actor && !actor.IsEnemyOf(actor1) && (LOS.CanTraceViewLine(actor.Location, actor1.Location.Position, maxRange) && m_Rules.RollChance(50))) {
              if (actor.HasLeader) {
                actor.Leader.RemoveFollower(actor);
                actor.TrustInLeader = 0;
              }
              DoMakeAggression(actor, actor1);
              return new ActionSay(actor, actor1, "YOU ARE ONE OF THEM!!", RogueGame.Sayflags.IS_IMPORTANT);    // this takes a turn unconditionally for game balance.
            }
          }
          return null;
        default:
          return null;
      }
    }

    private void SeeingCauseInsanity(Actor whoDoesTheAction, Location loc, int sanCost, string what)
    {
      foreach (Actor actor in loc.Map.Actors) {
        if (actor.Model.Abilities.HasSanity && !actor.IsSleeping) {
          int maxRange = actor.FOVrange(loc.Map.LocalTime, Session.Get.World.Weather);
          if (LOS.CanTraceViewLine(loc, actor.Location.Position, maxRange)) {
            actor.SpendSanity(sanCost);
            if (whoDoesTheAction == actor) {
              if (actor.IsPlayer)
                AddMessage(new Data.Message("That was a very disturbing thing to do...", loc.Map.LocalTime.TurnCounter, Color.Orange));
              else if (ForceVisibleToPlayer(actor))
                AddMessage(MakeMessage(actor, string.Format("{0} done something very disturbing...", Conjugate(actor, VERB_HAVE))));
            }
            else if (actor.IsPlayer)
              AddMessage(new Data.Message(string.Format("Seeing {0} is very disturbing...", what), loc.Map.LocalTime.TurnCounter, Color.Orange));
            else if (ForceVisibleToPlayer(actor))
              AddMessage(MakeMessage(actor, string.Format("{0} something very disturbing...", Conjugate(actor, VERB_SEE))));
          }
        }
      }
    }

    private void OnMapPowerGeneratorSwitch(Location location)
    {
      Map map = location.Map;
      if (map == Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap) {
        lock (Session.Get) {
          if (1.0 <= map.PowerRatio) {
            if (map.Illuminate(true)) {
              if (0 < map.PlayerCount) {
                ClearMessages();
                AddMessage(new Data.Message("The Facility lights turn on!", map.LocalTime.TurnCounter, Color.Green));
                RedrawPlayScreen();
              }
              if (!Session.Get.Scoring.HasCompletedAchievement(Achievement.IDs.CHAR_POWER_UNDERGROUND_FACILITY))
                ShowNewAchievement(Achievement.IDs.CHAR_POWER_UNDERGROUND_FACILITY);
            }
          } else if (map.Illuminate(false)) {
            if (0 < map.PlayerCount) {
              ClearMessages();
              AddMessage(new Data.Message("The Facility lights turn off!", map.LocalTime.TurnCounter, Color.Red));
              RedrawPlayScreen();
            }
          }
        }
      }
      if (map == map.District.SubwayMap) {
        lock (Session.Get) {
          if (1.0 <= map.PowerRatio) {
            if (map.Illuminate(true)) {
              if (0 < map.PlayerCount) {
                ClearMessages();
                AddMessage(new Data.Message("The station power turns on!", map.LocalTime.TurnCounter, Color.Green));
                AddMessage(new Data.Message("You hear the gates opening.", map.LocalTime.TurnCounter, Color.Green));
                RedrawPlayScreen();
              }
              map.OpenAllGates();
            }
          } else if (map.Illuminate(false)) {
            if (0 < map.PlayerCount) {
              ClearMessages();
              AddMessage(new Data.Message("The station power turns off!", map.LocalTime.TurnCounter, Color.Red));
              AddMessage(new Data.Message("You hear the gates closing.", map.LocalTime.TurnCounter, Color.Red));
              RedrawPlayScreen();
            }
            CloseAllGates(map,"gates");
          }
        }
      }
      if (map == Session.Get.UniqueMaps.PoliceStation_JailsLevel.TheMap) {
        lock (Session.Get) {
          if (1.0 <= map.PowerRatio) {
            if (0 < map.PlayerCount) {
              ClearMessages();
              AddMessage(new Data.Message("The cells are opening.", map.LocalTime.TurnCounter, Color.Green));
              RedrawPlayScreen();
            }
            map.OpenAllGates();
            // even if we missed talking to the Prisoner Who Should Not Be, make sure he'll think of thanking us if not an enemy
            if (0 == Session.Get.ScriptStage_PoliceStationPrisoner) Session.Get.ScriptStage_PoliceStationPrisoner = 0;
          } else {
            if (0 < map.PlayerCount) {
              ClearMessages();
              AddMessage(new Data.Message("The cells are closing.", map.LocalTime.TurnCounter, Color.Green));
              RedrawPlayScreen();
            }
            CloseAllGates(map,"cells");
          }
        }
      }
      // XXX multi-PC, or NPC operation of the generators, needs more work for the messaging to work here
      if (map != Session.Get.UniqueMaps.Hospital_Power.TheMap) return;
      lock (Session.Get) {
        if (1.0 <= map.PowerRatio) {
          if (0 < map.PlayerCount) {
            ClearMessages();
            AddMessage(new Data.Message("The lights turn on and you hear something opening upstairs.", map.LocalTime.TurnCounter, Color.Green));
            RedrawPlayScreen();
          }
          DoHospitalPowerOn();
        } else {
          if (map.Lighting == Lighting.DARKNESS) return;
          if (0 < map.PlayerCount) {
            ClearMessages();
            AddMessage(new Data.Message("The lights turn off and you hear something closing upstairs.", map.LocalTime.TurnCounter, Color.Green));
            RedrawPlayScreen();
          }
          DoHospitalPowerOff();
        }
      }
    }

    private void CloseAllGates(Map map,string gate_name)
    {
      foreach (MapObject mapObject in map.MapObjects.Where(obj=>GameImages.OBJ_GATE_OPEN==obj.ImageID)) {
        mapObject.IsWalkable = false;
        mapObject.ImageID = GameImages.OBJ_GATE_CLOSED;
        Actor actorAt = map.GetActorAt(mapObject.Location.Position);
        if (null == actorAt) continue;
        KillActor(null, actorAt, "crushed");
        if (0<map.PlayerCount) {
          AddMessage(new Data.Message("Someone got crushed between the closing "+gate_name+"!", map.LocalTime.TurnCounter, Color.Red));
          RedrawPlayScreen();
        }
      }
    }

    static private void DoHospitalPowerOn()
    {
      Session.Get.UniqueMaps.Hospital_Admissions.TheMap.Illuminate(true);
      Session.Get.UniqueMaps.Hospital_Offices.TheMap.Illuminate(true);
      Session.Get.UniqueMaps.Hospital_Patients.TheMap.Illuminate(true);
      Session.Get.UniqueMaps.Hospital_Power.TheMap.Illuminate(true);
      Session.Get.UniqueMaps.Hospital_Storage.TheMap.Illuminate(true);
      Session.Get.UniqueMaps.Hospital_Storage.TheMap.OpenAllGates();    // other hospital maps do not have gates so no-op
    }

    private void DoHospitalPowerOff()
    {
      Session.Get.UniqueMaps.Hospital_Admissions.TheMap.Illuminate(false);
      Session.Get.UniqueMaps.Hospital_Offices.TheMap.Illuminate(false);
      Session.Get.UniqueMaps.Hospital_Patients.TheMap.Illuminate(false);
      Session.Get.UniqueMaps.Hospital_Power.TheMap.Illuminate(false);
      Session.Get.UniqueMaps.Hospital_Storage.TheMap.Illuminate(false);
      Map theMap = Session.Get.UniqueMaps.Hospital_Storage.TheMap;
      CloseAllGates(Session.Get.UniqueMaps.Hospital_Storage.TheMap,"gate");
    }

    private void DoTurnAllGeneratorsOn(Map map)
    {
      foreach (MapObject mapObject in map.MapObjects) {
        if (mapObject is PowerGenerator powGen && !powGen.IsOn) {
          powGen.TogglePower();
          OnMapPowerGeneratorSwitch(powGen.Location);
        }
      }
    }

    public static bool IsInCHAROffice(Location location)
    {
      return location.Map.GetZonesAt(location.Position)?.Any(zone => zone.HasGameAttribute("CHAR Office")) ?? false;
    }

    static public bool IsInCHARProperty(Location location)
    {
      if (location.Map != Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap)
        return IsInCHAROffice(location);
      return true;
    }

    private bool AreLinkedByPhone(Actor speaker, Actor target)
    {
      if (speaker.Leader != target && target.Leader != speaker) return false;
      ItemTracker itemTracker1 = speaker.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;
      if (itemTracker1 == null || !itemTracker1.CanTrackFollowersOrLeader) return false;
      ItemTracker itemTracker2 = target.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;
      return itemTracker2 != null && itemTracker2.CanTrackFollowersOrLeader;
    }

#if DEAD_FUNC
    private List<Actor> ListWorldActors(Predicate<Actor> pred, RogueGame.MapListFlags flags)
    {
      List<Actor> actorList = new List<Actor>();
      for (int index1 = 0; index1 < Session.Get.World.Size; ++index1) {
        for (int index2 = 0; index2 < Session.Get.World.Size; ++index2)
          actorList.AddRange(ListDistrictActors(Session.Get.World[index1, index2], flags, pred));
      }
      return actorList;
    }
#endif

    static private List<Actor> ListDistrictActors(District d, RogueGame.MapListFlags flags, Predicate<Actor> pred=null)
    {
      List<Actor> actorList = new List<Actor>();
      foreach (Map map in d.Maps) {
        if ((flags & RogueGame.MapListFlags.EXCLUDE_SECRET_MAPS) == RogueGame.MapListFlags.NONE || !map.IsSecret) {
          foreach (Actor actor in map.Actors) {
            if (pred == null || pred(actor))
              actorList.Add(actor);
          }
        }
      }
      return actorList;
    }

    static private string FunFactActorResume(Actor a, string info)
    {
      if (a == null) return "(N/A)";
      return string.Format("{0} - {1}, a {2} - {3}", info, a.TheName, a.Model.Name, a.Location.Map.Name);
    }

    private string[] CompileDistrictFunFacts(District d)
    {
      List<string> stringList = new List<string>();
      List<Actor> actorList1 = ListDistrictActors(d, RogueGame.MapListFlags.EXCLUDE_SECRET_MAPS, a => {
        if (!a.IsDead) return !a.Model.Abilities.IsUndead;
        return false;
      });
      List<Actor> actorList2 = ListDistrictActors(d, RogueGame.MapListFlags.EXCLUDE_SECRET_MAPS, a => {
        if (!a.IsDead) return a.Model.Abilities.IsUndead;
        return false;
      });
      List<Actor> actorList3 = ListDistrictActors(d, RogueGame.MapListFlags.EXCLUDE_SECRET_MAPS);
      (m_Player.Model.Abilities.IsUndead ? actorList2 : actorList1).Add(m_Player);
      actorList3.Add(m_Player);
      if (actorList1.Count > 0) {
        actorList1.Sort((a, b) => {
          if (a.SpawnTime < b.SpawnTime) return -1;
          return a.SpawnTime != b.SpawnTime ? 1 : 0;
        });
        stringList.Add("- Oldest Livings Surviving");
        stringList.Add(string.Format("    1st {0}.", FunFactActorResume(actorList1[0], new WorldTime(actorList1[0].SpawnTime).ToString())));
        if (actorList1.Count > 1)
          stringList.Add(string.Format("    2nd {0}.", FunFactActorResume(actorList1[1], new WorldTime(actorList1[1].SpawnTime).ToString())));
      } else stringList.Add("    No living actors alive!");
      if (actorList2.Count > 0) {
        actorList2.Sort((a, b) => {
          if (a.SpawnTime < b.SpawnTime) return -1;
          return a.SpawnTime != b.SpawnTime ? 1 : 0;
        });
        stringList.Add("- Oldest Undeads Rotting Around");
        stringList.Add(string.Format("    1st {0}.", FunFactActorResume(actorList2[0], new WorldTime(actorList2[0].SpawnTime).ToString())));
        if (actorList2.Count > 1)
          stringList.Add(string.Format("    2nd {0}.", FunFactActorResume(actorList2[1], new WorldTime(actorList2[1].SpawnTime).ToString())));
      }
      else stringList.Add("    No undeads shambling around!");
      if (actorList1.Count > 0) {
        actorList1.Sort((a, b) => {
          if (a.KillsCount > b.KillsCount) return -1;
          return a.KillsCount != b.KillsCount ? 1 : 0;
        });
        stringList.Add("- Deadliest Livings Kicking ass");
        if (actorList1[0].KillsCount > 0) {
          stringList.Add(string.Format("    1st {0}.", FunFactActorResume(actorList1[0], actorList1[0].KillsCount.ToString())));
          if (actorList1.Count > 1 && actorList1[1].KillsCount > 0)
            stringList.Add(string.Format("    2nd {0}.", FunFactActorResume(actorList1[1], actorList1[1].KillsCount.ToString())));
        }
        else stringList.Add("    Livings can't fight for their lives apparently.");
      }
      if (actorList2.Count > 0) {
        actorList2.Sort((a, b) => {
          if (a.KillsCount > b.KillsCount) return -1;
          return a.KillsCount != b.KillsCount ? 1 : 0;
        });
        stringList.Add("- Deadliest Undeads Chewing Brains");
        if (actorList2[0].KillsCount > 0) {
          stringList.Add(string.Format("    1st {0}.", FunFactActorResume(actorList2[0], actorList2[0].KillsCount.ToString())));
          if (actorList2.Count > 1 && actorList2[1].KillsCount > 0)
            stringList.Add(string.Format("    2nd {0}.", FunFactActorResume(actorList2[1], actorList2[1].KillsCount.ToString())));
        }
        else stringList.Add("    Undeads don't care for brains apparently.");
      }
      if (actorList1.Count > 0) {
        actorList1.Sort((a, b) => {
          if (a.MurdersCounter > b.MurdersCounter) return -1;
          return a.MurdersCounter != b.MurdersCounter ? 1 : 0;
        });
        stringList.Add("- Most Murderous Murderer Murdering");
        if (actorList1[0].MurdersCounter > 0) {
          stringList.Add(string.Format("    1st {0}.", FunFactActorResume(actorList1[0], actorList1[0].MurdersCounter.ToString())));
          if (actorList1.Count > 1 && actorList1[1].MurdersCounter > 0)
            stringList.Add(string.Format("    2nd {0}.", FunFactActorResume(actorList1[1], actorList1[1].MurdersCounter.ToString())));
        }
        else stringList.Add("    No murders committed!");
      }
      return stringList.ToArray();
    }

#if DEAD_FUNC
    public void DEV_ToggleShowActorsStats()
    {
      s_Options.DEV_ShowActorsStats = !s_Options.DEV_ShowActorsStats;
    }
#endif

    private void LoadData()
    {
      LoadDataSkills();
    }

    private void LoadDataSkills()
    {
      Skills.LoadSkillsFromCSV(m_UI, "Resources\\Data\\Skills.csv");
    }

    public abstract class Overlay   // could be an interface instead
    {
      public abstract void Draw(IRogueUI ui);
    }

    private class OverlayImage : Overlay
    {
      public readonly Point ScreenPosition;
      public readonly string ImageID;

      public OverlayImage(Point screenPosition, string imageID)
      {
        ScreenPosition = screenPosition;
        ImageID = imageID;
      }

      public override void Draw(IRogueUI ui)
      {
        ui.UI_DrawImage(ImageID, ScreenPosition.X, ScreenPosition.Y);
      }
    }

    private class OverlayTransparentImage : Overlay
    {
      public readonly float Alpha;
      public readonly Point ScreenPosition;
      public readonly string ImageID;

      public OverlayTransparentImage(float alpha, Point screenPosition, string imageID)
      {
        Alpha = alpha;
        ScreenPosition = screenPosition;
        ImageID = imageID;
      }

      public override void Draw(IRogueUI ui)
      {
        ui.UI_DrawTransparentImage(Alpha, ImageID, ScreenPosition.X, ScreenPosition.Y);
      }
    }

    private class OverlayText : Overlay
    {
      public readonly Point ScreenPosition;
      public readonly string Text;
      public readonly Color Color;
      public readonly Color? ShadowColor;

      public OverlayText(Point screenPosition, Color color, string text, Color? shadowColor = null)
      {
        ScreenPosition = screenPosition;
        Color = color;
        ShadowColor = shadowColor;
        Text = text;
      }

      public override void Draw(IRogueUI ui)
      {
        if (ShadowColor.HasValue)
          ui.UI_DrawString(ShadowColor.Value, Text, ScreenPosition.X + 1, ScreenPosition.Y + 1, new Color?());
        ui.UI_DrawString(Color, Text, ScreenPosition.X, ScreenPosition.Y, new Color?());
      }
    }

    private class OverlayLine : Overlay   // dead class
        {
      public Point ScreenFrom { get; set; }

      public Point ScreenTo { get; set; }

      public Color Color { get; set; }

      public OverlayLine(Point screenFrom, Color color, Point screenTo)
      {
                ScreenFrom = screenFrom;
                ScreenTo = screenTo;
                Color = color;
      }

      public override void Draw(IRogueUI ui)
      {
        ui.UI_DrawLine(Color, ScreenFrom.X, ScreenFrom.Y, ScreenTo.X, ScreenTo.Y);
      }
    }

    // cf competing implementation : GameImages::MonochromeBorderTile class and image caching
    public class OverlayRect : Overlay
    {
      public readonly Rectangle Rectangle;
      public readonly Color Color;

      public OverlayRect(Color color, Rectangle rect)
      {
        Rectangle = rect;
        Color = color;
      }

      public override void Draw(IRogueUI ui)
      {
        ui.UI_DrawRect(Color, Rectangle);
      }
    }

    private class OverlayPopup : Overlay
    {
      public readonly Point ScreenPosition;
      public readonly Color TextColor;
      public readonly Color BoxBorderColor;
      public readonly Color BoxFillColor;
      public readonly string[] Lines;

      public OverlayPopup(string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, Point screenPos)
      {
        ScreenPosition = screenPos;
        TextColor = textColor;
        BoxBorderColor = boxBorderColor;
        BoxFillColor = boxFillColor;
        Lines = lines;
      }

      public override void Draw(IRogueUI ui)
      {
        ui.UI_DrawPopup(Lines, TextColor, BoxBorderColor, BoxFillColor, ScreenPosition.X, ScreenPosition.Y);
      }
    }

    private struct CharGen
    {
      public bool IsUndead;
      public GameActors.IDs UndeadModel;
      public bool IsMale;
      public Skills.IDs StartingSkill;
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
