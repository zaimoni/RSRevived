// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.RogueGame
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define DATAFLOW_TRACE

#define FRAGILE_RENDERING
// #define POLICE_NO_QUESTIONS_ASKED

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Engine.MapObjects;
using djack.RogueSurvivor.Gameplay;
using djack.RogueSurvivor.Gameplay.AI;
using djack.RogueSurvivor.Gameplay.Generators;
using djack.RogueSurvivor.UI;
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Zaimoni.Data;

using static Zaimoni.Data.Functor;

using ColorString = System.Collections.Generic.KeyValuePair<System.Drawing.Color, string>;
// game coordinate types
using Point = Zaimoni.Data.Vector2D<short>;
using Rectangle = Zaimoni.Data.Box2D<short>;
// GDI+ types
using Color = System.Drawing.Color;
using GDI_Point = System.Drawing.Point;
using GDI_Rectangle = System.Drawing.Rectangle;
using GDI_Size = System.Drawing.Size;
using System.Numerics;
using System.Runtime;
using djack.RogueSurvivor.Engine._Action;

namespace djack.RogueSurvivor.Engine
{
    internal class RogueGame
  {
    private readonly Color POPUP_FILLCOLOR = Color.FromArgb(192, Color.CornflowerBlue);
    // these are used by OverlayPopup, so are used as arrays of strings anyway
    private const string TRADE_MODE_TEXT = "TRADE MODE - Y to accept the deal, N to refuse";
    private readonly string[] INITIATE_TRADE_MODE_TEXT = new string[1]
    {
      "INITIATE TRADE MODE - directions to offer item to someone, ESC cancels"
    };
    private readonly string[] FIRE_MODE_TEXT = new string[] { "FIRE MODE - F to fire, T next target, M toggle mode, ESC cancels" };
    private readonly string ORDER_MODE_TEXT = "ORDER MODE - follow instructions in the message panel, ESC cancels";
    // end string arrays used by OverlayPopup

    // report formatting; design width 120 characters, design height 51-ish lines (depends on bold/normal)
    private readonly string hr = "".PadLeft(120,'-');
    private readonly string hr_plus = string.Concat(Enumerable.Range(0,11).Select(x => "---------+").ToArray());

    private readonly Color MODE_TEXTCOLOR = Color.Yellow;
    private readonly Color MODE_BORDERCOLOR = Color.Yellow;
    private readonly Color MODE_FILLCOLOR = Color.FromArgb(192, Color.Gray);
    private static readonly Color PLAYER_ACTION_COLOR = Color.White;
    private static readonly Color OTHER_ACTION_COLOR = Color.Gray;
    public static readonly Color SAYOREMOTE_DANGER_COLOR = Color.Brown; // alpha10
    public static readonly Color SAYOREMOTE_NORMAL_COLOR = Color.DarkCyan; // alpha10
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
    public static readonly Verb VERB_ACTIVATE = new Verb("activate");
    private readonly Verb VERB_AVOID = new Verb("avoid");
    private readonly Verb VERB_BARRICADE = new Verb("barricade");
    public static readonly Verb VERB_BASH = new Verb("bash", "bashes");
    public static readonly Verb VERB_BE = new Verb("are", "is");
    private readonly Verb VERB_BUILD = new Verb("build");
    private readonly Verb VERB_BREAK = new Verb("break");
    private readonly Verb VERB_BUTCHER = new Verb("butcher");
    private readonly Verb VERB_CATCH = new Verb("catch", "catches");
    private readonly Verb VERB_CHAT_WITH = new Verb("chat with", "chats with");
    private readonly Verb VERB_CLOSE = new Verb("close");
    private readonly Verb VERB_COLLAPSE = new Verb("collapse");
    private readonly Verb VERB_CRUSH = new Verb("crush", "crushes");
    public static readonly Verb VERB_DESACTIVATE = new Verb("desactivate");
    private readonly Verb VERB_DESTROY = new Verb("destroy");
    private readonly Verb VERB_DIE = new Verb("die");
    private readonly Verb VERB_DIE_FROM_STARVATION = new Verb("die from starvation", "dies from starvation");
    private readonly Verb VERB_DISARM = new Verb("disarm");  // alpha10
    private readonly Verb VERB_DISCARD = new Verb("discard");
    private readonly Verb VERB_DRAG = new Verb("drag");
    private readonly Verb VERB_DROP = new Verb("drop");
    public static readonly Verb VERB_EAT = new Verb("eat");
    private readonly Verb VERB_ENJOY = new Verb("enjoy");
    private readonly Verb VERB_ENTER = new Verb("enter");
    private readonly Verb VERB_ESCAPE = new Verb("escape");
    private readonly Verb VERB_FAIL = new Verb("fail");
    private readonly Verb VERB_FEAST_ON = new Verb("feast on", "feasts on");
    private readonly Verb VERB_FEEL = new Verb("feel");
    private readonly Verb VERB_GIVE = new Verb("give");
    private readonly Verb VERB_GRAB = new Verb("grab");
    public static readonly Verb VERB_EQUIP = new Verb("equip");
    private readonly Verb VERB_HAVE = new Verb("have", "has");
    private readonly Verb VERB_HELP = new Verb("help");
    public static readonly Verb VERB_HEAL_WITH = new Verb("heal with", "heals with");
    private readonly Verb VERB_JUMP_ON = new Verb("jump on", "jumps on");
    private readonly Verb VERB_KILL = new Verb("kill");
    private readonly Verb VERB_LEAVE = new Verb("leave");
    private readonly Verb VERB_MISS = new Verb("miss", "misses");
    private readonly Verb VERB_MURDER = new Verb("murder");
    private readonly Verb VERB_OFFER = new Verb("offer");
    private readonly Verb VERB_OPEN = new Verb("open");
    private readonly Verb VERB_ORDER = new Verb("order");
    private readonly Verb VERB_PERSUADE = new Verb("persuade");
    private readonly Verb VERB_PULL = new Verb("pull");  // alpha10
    private readonly Verb VERB_PUSH = new Verb("push", "pushes");
    private readonly Verb VERB_PUT = new Verb("put", "puts");
    private readonly Verb VERB_RAISE_ALARM = new Verb("raise the alarm", "raises the alarm");
    private readonly Verb VERB_REFUSE_THE_DEAL = new Verb("refuse the deal", "refuses the deal");
    public static readonly Verb VERB_RELOAD = new Verb("reload");
    private readonly Verb VERB_UNLOAD = new Verb("unload");
    private readonly Verb VERB_RECHARGE = new Verb("recharge");
    private readonly Verb VERB_REPAIR = new Verb("repair");
    private readonly Verb VERB_REVIVE = new Verb("revive");
    private readonly Verb VERB_SEE = new Verb("see");
    private readonly Verb VERB_SHOUT = new Verb("shout");
    private readonly Verb VERB_SHOVE = new Verb("shove");
    private readonly Verb VERB_SNORE = new Verb("snore");
    private readonly Verb VERB_SPRAY = new Verb("spray");
    static public readonly Verb VERB_START = new Verb("start");
    static public readonly Verb VERB_STOP = new Verb("stop");
    private readonly Verb VERB_STUMBLE = new Verb("stumble");
    private readonly Verb VERB_SWITCH = new Verb("switch", "switches");
    private readonly Verb VERB_SWITCH_PLACE_WITH = new Verb("switch place with", "switches place with");
    private readonly Verb VERB_TAKE = new Verb("take");
    private readonly Verb VERB_THROW = new Verb("throw");
    private readonly Verb VERB_TRANSFORM_INTO = new Verb("transform into", "transforms into");
    public static readonly Verb VERB_UNEQUIP = new Verb("unequip");
    public static readonly Verb VERB_VOMIT = new Verb("vomit");
    private readonly Verb VERB_WAIT = new Verb("wait");
    private readonly Verb VERB_WAKE_UP = new Verb("wake up", "wakes up");
    private bool m_IsGameRunning = true;
    private readonly List<Overlay> m_Overlays = new List<Overlay>();
    private readonly object m_SimMutex = new object();  // 2018-08-20: almost dead
    private readonly object m_SimStateLock = new object(); // alpha10 lock when reading sim thread state flags
    private CancellationTokenSource? m_CancelSource = null; // migration of alpha10 sim thread state to .NET 5.0

    public const int MAP_MAX_HEIGHT = 100;
    public const int MAP_MAX_WIDTH = 100;
    public const int MINIMAP_RADIUS = 50;
    public const int POLICE_RADIO_RANGE = MINIMAP_RADIUS;   // may need to adjust this when space-time scaling

    public const int TILE_SIZE = 32;    // ACTOR_SIZE+ACTOR_OFFSET <= TILE_SIZE
    public const int ACTOR_SIZE = 32;
    public const int ACTOR_OFFSET = 0;
    public static readonly GDI_Size SIZE_OF_TILE = new GDI_Size(TILE_SIZE, TILE_SIZE);
    public static readonly GDI_Size SIZE_OF_ACTOR = new GDI_Size(ACTOR_SIZE, ACTOR_SIZE);

    public const int VIEW_RADIUS = 10;  // also maximum living sight range; severe UI issues if sight radius exceeds UI view radius, cf. Cataclysm family for what goes wrong
    public const int HALF_VIEW_WIDTH = VIEW_RADIUS; // \todo eliminate these constants in favor of VIEW_RADIUS
    public const int HALF_VIEW_HEIGHT = VIEW_RADIUS;

    // RS Alpha 10.1- define these here.  Backward compatibility constants
    public const int CANVAS_WIDTH = IRogueUI.CANVAS_WIDTH;
    public const int CANVAS_HEIGHT = IRogueUI.CANVAS_HEIGHT;
    public const int LINE_SPACING = IRogueUI.LINE_SPACING;
    public const int BOLD_LINE_SPACING = IRogueUI.BOLD_LINE_SPACING;

    // (2*VIEW_RADIUS+1)*TILE_SIZE == 672
    private const int MESSAGES_X = 4;
    public const int MESSAGES_Y = 676;
    public const int MESSAGES_SPACING = LINE_SPACING;
//  private const int MAX_MESSAGES = 7;
    public const int MAX_MESSAGES = (CANVAS_HEIGHT - LOCATIONPANEL_Y) / MESSAGES_SPACING;
//  private const int MESSAGES_HISTORY = 59;
    public const int MESSAGES_HISTORY = (CANVAS_HEIGHT - 3 * BOLD_LINE_SPACING) / MESSAGES_SPACING;
    private const int MESSAGES_LINE_LENGTH = 91;    // in characters (empirical constant rather than measured)

    private const int RIGHTPANEL_X = 676;
    private const int RIGHTPANEL_Y = 0;
    private const int RIGHTPANEL_TEXT_X = RIGHTPANEL_X+4;
    private const int RIGHTPANEL_TEXT_Y = RIGHTPANEL_Y+4;
    private const int INVENTORYPANEL_X = RIGHTPANEL_X+4;
    // DrawActorStatus uses 11 bold lines: 154 before vertical margins
    private const int INVENTORYPANEL_Y = RIGHTPANEL_TEXT_Y + 170; // alpha10; formerly +156; formerly +142 (responds to maximum bold lines needed, etc.)

    private const int GROUNDINVENTORYPANEL_Y = INVENTORYPANEL_Y + TILE_SIZE + LINE_SPACING + BOLD_LINE_SPACING;
    private const int CORPSESPANEL_Y = GROUNDINVENTORYPANEL_Y + TILE_SIZE + LINE_SPACING + BOLD_LINE_SPACING;
    private const int INVENTORY_SLOTS_PER_LINE = 10;
    private const int SKILLTABLE_X = RIGHTPANEL_X + 4;
    private const int SKILLTABLE_Y = CORPSESPANEL_Y + TILE_SIZE + LINE_SPACING + BOLD_LINE_SPACING;
    // we need enough slots (SKILLTABLE_LINES*3) to display all skills in the endgame
    private const int SKILLTABLE_LINES = 7; // formerly 10, which supports 30 skills; lower bound set by Skills::_LIVING_COUNT
    private const int SKILLTABLE_COL_WIDTH = 120;   // 3 columns (theoretical 118 width)
    private const int SKILL_LINE_SPACING = LINE_SPACING+4; // for x-axis, not y-axis

    private const int LOCATIONPANEL_X = RIGHTPANEL_X;
    public const int LOCATIONPANEL_Y = MESSAGES_Y;
    private const int LOCATIONPANEL_TEXT_X = LOCATIONPANEL_X+4;
    private const int LOCATIONPANEL_TEXT_X_COL2 = LOCATIONPANEL_TEXT_X+(CANVAS_WIDTH - LOCATIONPANEL_TEXT_X)/3;
    private const int LOCATIONPANEL_TEXT_Y = LOCATIONPANEL_Y+4;
    public const int MINITILE_SIZE = 2;
    private const int MINIMAP_X = 750;  // cf. LOCATIONPANEL_X
    private const int MINIMAP_Y = LOCATIONPANEL_Y-MINITILE_SIZE*(2+2*MINIMAP_RADIUS);
    private const int EXIT_SLOTS = MINITILE_SIZE * (2 + 2 * MINIMAP_RADIUS) / TILE_SIZE;
    private const int ENTRYMAP_EXIT_SLOT = EXIT_SLOTS-2;
    private const int EXIT_SLOT_X = RIGHTPANEL_X+4;
    private const int EXIT_SLOT_Y0 = LOCATIONPANEL_Y-TILE_SIZE* EXIT_SLOTS;
    private const int MINI_TRACKER_OFFSET = 1;

    private const int SKILLTABLE_Y_ALT = MINIMAP_Y - SKILLTABLE_LINES * LINE_SPACING - 4;

    public const int DELAY_SHORT = 250;
    public const int DELAY_NORMAL = 500;
    public const int DELAY_LONG = 1000;
    private const int CREDIT_CHAR_SPACING = 8;
    private const int CREDIT_LINE_SPACING = 12;
    private const int TEXTFILE_CHARS_PER_LINE = 120;
    private const int TEXTFILE_LINES_PER_PAGE = 50;
    private const int SPAWN_DISTANCE_TO_PLAYER = WorldTime.TURNS_PER_HOUR/3;
    private const int SEWERS_INVASION_CHANCE = 1;
    public const float SEWERS_UNDEADS_FACTOR = 0.5f;
    public const int REFUGEES_WAVE_ITEMS = 3;
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
    private const int PLAYER_HEAR_PUSHPULL_CHANCE = 25;  // alpha10 also for pulls
    private const int PLAYER_HEAR_BASH_CHANCE = 25;
    private const int PLAYER_HEAR_BREAK_CHANCE = 50;
    private const int PLAYER_HEAR_EXPLOSION_CHANCE = 100;
    public const int MESSAGE_NPC_SLEEP_SNORE_CHANCE = 10;
    private const double BASE_SPEED = Actor.BASE_ACTION_COST; // double triggers automatic promotion from integer to double
    private const int JUMP_STUMBLE_CHANCE = 25;
    private const int JUMP_STUMBLE_ACTION_COST = Actor.BASE_ACTION_COST;

#nullable enable
    // we cannot actually *be* a singleton since we need to guarantee that IRogueUI.UI is set up before we are alive
    static private RogueGame? s_ooao = null;

    static public RogueGame Game {
        get { return s_ooao!; }
    }

    static private KeyValuePair<Map, KeyValuePair<ZoneLoc, HashSet<Point>>>[]? s_Arrival = null;
    static private ZoneLoc[]? s_RefugeeSpawnZones = null;
    static private List<Actor>? s_RefugeePool = null;
#nullable restore

#nullable enable
#if DEBUG
    public static bool IsDebugging;
#endif
    private static readonly Stopwatch play_timer = new Stopwatch();
    private readonly IRogueUI m_UI; // this cannot be static.
#nullable restore
    private HiScoreTable m_HiScoreTable;
#nullable enable
    private static MessageManager? s_MessageManager;
    private static MessageManager Messages { get { return s_MessageManager!; } }
    private bool m_HasLoadedGame;

    // We're a singleton.  Do these three as static to help with loading savefiles. m_Player has warning issues as static, however.
    private static Actor? m_Player;
    private static Actor? m_PlayerInTurn;
    private static string? m_Status = null; // suppressed by m_PlayerInTurn mismatch; AI view of player state
    private static string? m_Status2 = null; // suppressed by m_PlayerInTurn mismatch; UI feedback
#nullable restore
    private static ZoneLoc m_MapView;

#nullable enable
    private static GameOptions s_Options = new GameOptions();
    private static Keybindings s_KeyBindings = new Keybindings();
    private static GameHintsStatus s_Hints = new GameHintsStatus();
    private readonly BaseTownGenerator m_TownGenerator;
    private static bool m_PlayedIntro = false;
    private readonly IMusicManager m_MusicManager;
#nullable restore
    private CharGen m_CharGen;
#nullable enable
    private readonly TextFile? m_Manual;
    private int m_ManualLine = 0;
#nullable restore
    private Thread m_SimThread;

#nullable enable
    public static Actor Player { get { return m_Player!; } }
    public static bool IsPlayer(Actor? a) { return a == m_Player; }
    private static Actor setPlayer(Actor a) {
        // \todo these may need to be atomic updates
        m_Player = a;
        var msgs = (a.Controller as PlayerController)?.Messages;
        if (null != msgs) s_MessageManager = msgs;
        return a;
    }
#nullable restore
    public static Map CurrentMap { get { return m_MapView.m; } }
    private static Rectangle MapViewRect { get { return m_MapView.Rect; } }
    public bool IsGameRunning { get { return m_IsGameRunning; } }
    public static GameOptions Options { get { return s_Options; } }
    public static Keybindings KeyBindings { get { return s_KeyBindings; } }

    static public bool IsSimulating { get { return "Simulation Thread" == Thread.CurrentThread.Name; } }

#region Session save/load assistants
    static public void AfterLoad(Map.ActorCode src)
    {
      m_MapView = setPlayer(Map.decode(src)).Location.View;
    }

    static public void Load(SerializationInfo info, StreamingContext context)
    {
      info.read_nullsafe(ref s_MessageManager, "s_MessageManager");
      info.read_nullsafe(ref s_RefugeePool, "s_RefugeePool");
      info.read_nullsafe(ref s_CountedCommands, "s_CountedCommands");
    }

    static public void Save(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("s_Player",Map.encode(Player));
      info.AddValue("s_MessageManager", s_MessageManager);
      info.AddValue("s_RefugeePool", s_RefugeePool);
      info.AddValue("s_CountedCommands", s_CountedCommands);
    }

    // very severe access control issue; should be called only from Session::Session()
    static public void Reset() { m_MapView = default; }
#endregion

    public static void Init()
    {
#if DEBUG
      if (!IRogueUI.IsConstructed) throw new InvalidOperationException("UI not set up");
      if (null != s_ooao) throw new InvalidOperationException("tried to double-construct a moral singleton");
#endif
      s_ooao = new RogueGame(IRogueUI.UI);
    }

    private RogueGame(IRogueUI UI)
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
      s_MessageManager = new MessageManager(MESSAGES_SPACING, MESSAGES_HISTORY, MAX_MESSAGES);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating Rules, options");
      s_Options.ResetToDefaultValues();
      BaseTownGenerator.Parameters parameters = BaseTownGenerator.DEFAULT_PARAMS;
      parameters.MapWidth = MAP_MAX_WIDTH;
      parameters.MapHeight = MAP_MAX_HEIGHT;
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating Generator");
      m_TownGenerator = new StdTownGenerator(parameters);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating keys, hints.");
      s_Hints.ResetAllHints();
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating dbs");
      GameActors.Init(m_UI);
      GameItems.Init(m_UI);
      m_Manual = LoadManual();
      m_UI.Add(HandleMouseLookAuto);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "RogueGame() done.");
    }

#nullable enable
    public static void AddMessage(UI.Message msg) => Messages.Add(msg);

    public static bool AddMessage(List<PlayerController> witnesses, UI.Message msg)
    {
        bool rendered = false;
        foreach(var pc in witnesses) if (pc.AddMessage(msg)) rendered = true;
        return rendered;
    }

    public static bool AddMessage(List<PlayerController>? a_only_witnesses, List<PlayerController>? ad_witnesses, List<PlayerController>? d_only_witnesses, UI.Message[] msgs)
    {
        bool rendered = false;
        if (null != ad_witnesses && null != msgs[1]) foreach(var pc in ad_witnesses) if (pc.AddMessage(msgs[1])) rendered = true;
        if (null != a_only_witnesses && null != msgs[0]) foreach(var pc in a_only_witnesses) if (pc.AddMessage(msgs[0])) rendered = true;
        if (null != d_only_witnesses && null != msgs[2]) foreach(var pc in d_only_witnesses) if (pc.AddMessage(msgs[2])) rendered = true;
        return rendered;
    }

    public void ImportantMessage(List<PlayerController> witnesses, UI.Message msg, int delay=0)
    {
      RedrawPlayScreen(witnesses, msg);
      if (0 < delay) AnimDelay(delay);
    }

    public void ImportantMessage(List<PlayerController>? a_only_witness, List<PlayerController>? ad_witness, List<PlayerController>? d_only_witness, UI.Message[] msgs, int delay=0)
    {
      bool rendered = false;
      if (null != ad_witness && null != msgs[1]) foreach (var pc in ad_witness) if (pc.AddMessage(msgs[1])) rendered = true;
      if (null != a_only_witness && null != msgs[0]) foreach (var pc in a_only_witness) if (pc.AddMessage(msgs[0])) rendered = true;
      if (null != d_only_witness && null != msgs[2]) foreach (var pc in d_only_witness) if (pc.AddMessage(msgs[2])) rendered = true;
      if (rendered && 0 < delay) AnimDelay(delay);
    }

    // allows propagating sound to NPCs, in theory (API needs extending)
    // should also allow specifying range of sound as parameter (default is very loud), or possibly "energy" so we can model things better
    public void PropagateSound(Location loc, string text, Action<Actor> doFn, Predicate<Actor> player_knows)
    {
      bool hears(Actor a) {
        if (a.IsSleeping) return false;
        if (a.Controller.CanSee(loc)) return false;
        return Rules.StdDistance(a.Location, loc) <= a.AudioRange;
      };

      var actors = loc.ThoseNearby(GameActors.HUMAN_AUDIO, hears);
      if (null == actors) return;
      foreach(var a in actors) {
          if (a.Controller is PlayerController pc) {
            if (player_knows(a)) continue;
            pc.AddMessage(pc.MakeCentricMessage(text, in loc, PLAYER_AUDIO_COLOR));
            if (Player == a) RedrawPlayScreen();
            continue;
          }
          // NPC ai hooks go here
          doFn(a);
      }
    }

    // XXX just about everything that rates this is probable cause for police investigation
    public void AddMessageIfAudibleForPlayer(Location loc, string text)
    {
      bool hears(Actor a) {
        if (a.IsSleeping) return false;
        if (Rules.StdDistance(a.Location, in loc) > a.AudioRange) return false;
        return !a.Controller.CanSee(loc);
      };

      var PCs = PCsNearby(loc, GameActors.HUMAN_AUDIO, hears);
      if (null == PCs) return;

      foreach(var pc in PCs) {
        var msg = pc.MakeCentricMessage(text, in loc, PLAYER_AUDIO_COLOR);
        if (null != msg) pc.AddMessage(msg);
      }
    }

    private void AddMessageIfVisible<S,O>(S subject, List<PlayerController>? s_witnesses, string verb, O direct_object, List<PlayerController>? o_witnesses, string end_sentence, Color msg_color) where S:ILocation,INoun where O:ILocation,INoun
    {
      if (null == o_witnesses && null == o_witnesses) return;
      var both_witnesses = s_witnesses?.Intersect(o_witnesses);
      s_witnesses = s_witnesses?.SetDifference(both_witnesses);
      o_witnesses = o_witnesses?.SetDifference(both_witnesses);
      bool have_rendered = false;
      // based on RogueGame::MakeMessage
      var t0 = Session.Get.WorldTime.TurnCounter;
      var stage_msg = new string[4];
      stage_msg[1] = verb;
      stage_msg[3] = end_sentence;
      if (null != both_witnesses) {
        stage_msg[0] = subject.TheName;
        stage_msg[2] = direct_object.TheName;
        UI.Message stage = new(string.Join(" ", stage_msg), t0, msg_color);
        RedrawPlayScreen(both_witnesses, stage);
        have_rendered = true;
      }
      if (null != s_witnesses) {
        stage_msg[0] = subject.TheName;
        stage_msg[2] = direct_object.UnknownPronoun;
        UI.Message stage = new(string.Join(" ", stage_msg), t0, msg_color);
        if (!have_rendered) {
          RedrawPlayScreen(s_witnesses, stage);
          have_rendered = true;
        } else {
          AddMessage(s_witnesses, stage);
        }
      }
      if (null != o_witnesses) {
        stage_msg[0] = subject.UnknownPronoun.Capitalize();
        stage_msg[2] = direct_object.TheName;
        UI.Message stage = new(string.Join(" ", stage_msg), t0, msg_color);
        if (!have_rendered) {
          RedrawPlayScreen(o_witnesses, stage);
          have_rendered = true;
        } else {
          AddMessage(o_witnesses, stage);
        }
      }
    }

    public void AddMessageIfVisible<S,O>(S subject, string verb, O direct_object, string end_sentence, Color msg_color) where S:ILocation,INoun where O:ILocation,INoun
    {
      AddMessageIfVisible(subject, subject.PlayersInLOS(), verb, direct_object, direct_object.PlayersInLOS(), end_sentence, msg_color);
    }

    // more sophisticated variants would handle player-varying messages
    static public void PropagateSight(Location loc, Action<Actor> doFn)
    {
      bool sees(Actor a) { return a.Controller.CanSee(loc); };

      var actors = loc.ThoseNearby(Actor.MAX_VISION, sees);
      if (null == actors) return;

      foreach(var a in actors) doFn(a);
    }

    private static UI.Message MakeErrorMessage(string text)
    {
      return new(text, Session.Get.WorldTime.TurnCounter, Color.Red);
    }

    private static UI.Message MakePanopticMessage(Actor actor, string verb, INoun direct_object, string ends_with, Color color)
    {
      var msg = new string[] { actor.TheName, verb, direct_object.TheName , ends_with };
      return new(string.Join(" ",msg), Session.Get.WorldTime.TurnCounter, actor.IsPlayer ? PLAYER_ACTION_COLOR : color);
    }
    public static UI.Message MakePanopticMessage(Actor actor, string verb, INoun direct_object, string ends_with=".") => MakePanopticMessage(actor, verb, direct_object, ends_with, OTHER_ACTION_COLOR);

    private static UI.Message MakePanopticMessage(Actor actor, string doWhat, Color color)
    {
      var msg = new string[] { actor.TheName, doWhat };
      return new(string.Join(" ",msg), Session.Get.WorldTime.TurnCounter, actor.IsPlayer ? PLAYER_ACTION_COLOR : color);
    }
    public static UI.Message MakePanopticMessage(Actor actor, string doWhat) => MakePanopticMessage(actor, doWhat, OTHER_ACTION_COLOR);

    private static UI.Message MakeMessage(ActorController viewpoint, Actor actor, string doWhat, Color color)
    {
      var msg = new string[] { viewpoint.VisibleIdentity(actor), doWhat };
      return new(string.Join(" ",msg), Session.Get.WorldTime.TurnCounter, actor.IsPlayer ? PLAYER_ACTION_COLOR : color);
    }

    public static UI.Message MakeMessage(Actor actor, string doWhat, Color color)
    {
      return MakeMessage(actor.IsPlayer ? actor.Controller : Player.Controller, actor, doWhat, color);
    }
    public static UI.Message MakeMessage(Actor actor, string doWhat) => MakeMessage(actor, doWhat, OTHER_ACTION_COLOR);
    public static UI.Message MakeMessage(ActorController viewpoint, Actor actor, string doWhat) => MakeMessage(viewpoint, actor, doWhat, OTHER_ACTION_COLOR);

    private static UI.Message MakeMessage(ActorController viewpoint, Actor actor, string doWhat, Actor target, string phraseEnd = ".")
    {
      var msg = new string[] { viewpoint.VisibleIdentity(actor), doWhat, viewpoint.VisibleIdentity(target)+phraseEnd };
      return new(string.Join(" ", msg), Session.Get.WorldTime.TurnCounter, (actor.IsPlayer || target.IsPlayer) ? PLAYER_ACTION_COLOR : OTHER_ACTION_COLOR);
    }

    private static UI.Message MakeMessage(Actor actor, string doWhat, Actor target, string phraseEnd = ".")
    {
      var viewpoint = actor.Controller;
      if (!actor.IsPlayer) {
        viewpoint = target.IsPlayer ? target.Controller : Player.Controller;
      }
      return MakeMessage(viewpoint, actor, doWhat, target, phraseEnd);
    }

    /// <returns>only actor visible, both visible, only target visible</returns>
    private static UI.Message[] MakeMessages(Actor actor, string doWhat, Actor target, string phraseEnd = ".")
    {
      var ret = new UI.Message[3];
      int turn = Session.Get.WorldTime.TurnCounter;
      var msg_color = (actor.IsPlayer || target.IsPlayer) ? PLAYER_ACTION_COLOR : OTHER_ACTION_COLOR;

      // we inline Actor::VisibleIdentity(Actor) here
      var msg = new string[] { actor.TheName, doWhat, "someone" + phraseEnd };
      ret[0] = new(string.Join(" ", msg), turn, msg_color);
      msg[2] = target.TheName + phraseEnd;
      ret[1] = new(string.Join(" ", msg), turn, msg_color);
      msg[0] = "Someone";
      ret[2] = new(string.Join(" ", msg), turn, msg_color);
      return ret;
    }

    private static UI.Message[] MakeMessages(Actor actor, string doWhat, INoun target, string phraseEnd = ".")
    {
      var ret = new UI.Message[3];
      int turn = Session.Get.WorldTime.TurnCounter;
      var msg_color = actor.IsPlayer ? PLAYER_ACTION_COLOR : OTHER_ACTION_COLOR;

      // we inline Actor::VisibleIdentity(Actor) here
      var msg = new string[] { actor.TheName, doWhat, target.UnknownPronoun + phraseEnd };
      ret[0] = new(string.Join(" ", msg), turn, msg_color);
      msg[2] = target.TheName + phraseEnd;
      ret[1] = new(string.Join(" ", msg), turn, msg_color);
      msg[0] = "Someone";
      ret[2] = new(string.Join(" ", msg), turn, msg_color);
      return ret;
    }

    private static UI.Message MakeMessage(ActorController viewpoint, Actor actor, string doWhat, MapObject target, string phraseEnd)
    {
      var msg = new string[] { viewpoint.VisibleIdentity(actor), doWhat, viewpoint.VisibleIdentity(target)+phraseEnd };
      return new(string.Join(" ", msg), Session.Get.WorldTime.TurnCounter, actor.IsPlayer ? PLAYER_ACTION_COLOR : OTHER_ACTION_COLOR);
    }

    private static UI.Message MakeMessage(Actor actor, string doWhat, MapObject target, string phraseEnd = ".")
    {
      return MakeMessage(actor.IsPlayer ? Player.Controller : actor.Controller, actor, doWhat, target, phraseEnd);
    }

    private static UI.Message MakeMessage(ActorController viewpoint, Actor actor, string doWhat, Item target, string phraseEnd = ".")
    {
      var msg = new string[] { viewpoint.VisibleIdentity(actor), doWhat, target.TheName + phraseEnd };
      return new(string.Join(" ", msg), Session.Get.WorldTime.TurnCounter, actor.IsPlayer ? PLAYER_ACTION_COLOR : OTHER_ACTION_COLOR);
    }

    public static UI.Message MakeMessage(Actor actor, string doWhat, Item target, string phraseEnd = ".")
    {
      return MakeMessage(actor.IsPlayer ? actor.Controller : Player.Controller, actor, doWhat, target, phraseEnd);
    }

    public static UI.Message MakePanopticMessage(Actor actor, string doWhat, Item target, string phraseEnd = ".")
    {
      var msg = new string[] { actor.TheName, doWhat, target.TheName + phraseEnd };
      return new(string.Join(" ", msg), Session.Get.WorldTime.TurnCounter, actor.IsPlayer ? PLAYER_ACTION_COLOR : OTHER_ACTION_COLOR);
    }

    private static void ClearMessages() { Messages.Clear(); }

    static private string Truncate(string s, int maxLength)
    {
      return (s.Length > maxLength) ? s.Substring(0, maxLength) : s;
    }

    private void AnimDelay(int msecs)
    {
      if (IsSimulating) return;   // deadlocks otherwise
      if (s_Options.IsAnimDelayOn) m_UI.UI_Wait(msecs);
    }

    public void PlayEventMusic(string music, bool loop=false)
    {
      if (!string.IsNullOrEmpty(music)) {
        m_MusicManager.Stop();
        if (loop) m_MusicManager.PlayLooping(music, MusicPriority.PRIORITY_EVENT);
        else      m_MusicManager.Play(music, MusicPriority.PRIORITY_EVENT);
      }
    }

    public void Run()
    {
            InitDirectories();
            LoadData();
            LoadOptions();
            LoadHints();
            ApplyOptions(false);
            LoadKeybindings();
            m_UI.DrawHeadNote("Loading music...");
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
      // alpha10
      m_MusicManager.Load(GameMusics.SURFACE, GameMusics.SURFACE_FILE);
      m_UI.DrawHeadNote("Loading music... done!");
      m_UI.DrawHeadNote("Loading sfxs...");
      m_MusicManager.Load(GameSounds.UNDEAD_EAT, GameSounds.UNDEAD_EAT_FILE);
      m_MusicManager.Load(GameSounds.UNDEAD_RISE, GameSounds.UNDEAD_RISE_FILE);
      m_MusicManager.Load(GameSounds.NIGHTMARE, GameSounds.NIGHTMARE_FILE);
      m_UI.DrawHeadNote("Loading sfxs... done!");
      m_ManualLine = 0; // reset line just in case
      LoadHiScoreTable();
      while (m_IsGameRunning)
        GameLoop();
      m_MusicManager.Stop();
      m_MusicManager.Dispose();
      m_UI.UI_DoQuit();
    }

    // called from before game loop starts, so don't need to be too concerned about efficiency
    private void _bootstrap_computable(World world) {
      if (null == s_RefugeeSpawnZones) {
         HashSet<Point> arrival(Rectangle arrived) {
           HashSet<Point> ret = new();
           arrived.DoForEach(pt => ret.Add(pt));
           return ret;
         }

         const int zone_length = 6;
         const int highway_width = 6;   // synchronize w/map generation
         const int C_assert_survivor_spawn = 1/(SURVIVORS_BAND_SIZE + 1 <= highway_width ? 1 : 0);

         Rectangle world_bounds = world.Extent;
         List<ZoneLoc> stage = new();
         var stage_arrivals = new KeyValuePair<Map, KeyValuePair<ZoneLoc, HashSet<Point>>>[4];

         Point ns_arrival_zone_dim = new(zone_length, 1);
         Point ew_arrival_zone_dim = new(1, zone_length);

         // We have to redo stage_arrive if the district entry maps stop being the same size, or
         // if any highway entry points are on the corners of the reality bubble.

         // Estimate where highways enter our reality bubble
         var m = world.At(world_bounds.Anchor(Compass.XCOMlike.N))!.EntryMap;
         var pt = m.Rect.Anchor(Compass.XCOMlike.N);
         var highway_origin = new Location(m, pt + (highway_width / 2 + 1) * Direction.W);
         stage.Add(new ZoneLoc(m, new Zone("refugee-spawn-n-w", new Rectangle(highway_origin.Position + zone_length * Direction.W, ns_arrival_zone_dim))));
         stage.Add(new ZoneLoc(m, new Zone("refugee-spawn-n-e", new Rectangle(highway_origin.Position + highway_width * Direction.E, ns_arrival_zone_dim))));
         stage_arrivals[0] = new(m, new(new ZoneLoc(m, new Zone("highway-spawn-n", new Rectangle(highway_origin.Position, ns_arrival_zone_dim))), arrival(new(m.Rect.Anchor(Compass.XCOMlike.W) + Direction.N, new Point(m.Width, 2)))));

         m = world.At(world_bounds.Anchor(Compass.XCOMlike.E))!.EntryMap;
         pt = m.Rect.Anchor(Compass.XCOMlike.E);
         highway_origin = new Location(m, pt + (highway_width / 2 + 1) * Direction.N);
         stage.Add(new ZoneLoc(m, new Zone("refugee-spawn-e-n", new Rectangle(highway_origin.Position + zone_length * Direction.N, ew_arrival_zone_dim))));
         stage.Add(new ZoneLoc(m, new Zone("refugee-spawn-e-s", new Rectangle(highway_origin.Position + highway_width * Direction.S, ew_arrival_zone_dim))));
         stage_arrivals[1] = new(m, new(new ZoneLoc(m, new Zone("highway-spawn-e", new Rectangle(highway_origin.Position, ew_arrival_zone_dim))), arrival(new(m.Rect.Anchor(Compass.XCOMlike.N) + Direction.W, new Point(2, m.Height)))));

         m = world.At(world_bounds.Anchor(Compass.XCOMlike.S))!.EntryMap;
         pt = m.Rect.Anchor(Compass.XCOMlike.S);
         highway_origin = new Location(m, pt + (highway_width / 2 + 1) * Direction.W);
         stage.Add(new ZoneLoc(m, new Zone("refugee-spawn-s-w", new Rectangle(highway_origin.Position + zone_length * Direction.W, ns_arrival_zone_dim))));
         stage.Add(new ZoneLoc(m, new Zone("refugee-spawn-s-e", new Rectangle(highway_origin.Position + highway_width * Direction.E, ns_arrival_zone_dim))));
         stage_arrivals[2] = new(m, new(new ZoneLoc(m, new Zone("highway-spawn-s", new Rectangle(highway_origin.Position, ns_arrival_zone_dim))), stage_arrivals[0].Value.Value));

         m = world.At(world_bounds.Anchor(Compass.XCOMlike.W))!.EntryMap;
         pt = m.Rect.Anchor(Compass.XCOMlike.W);
         highway_origin = new Location(m, pt + (highway_width / 2 + 1) * Direction.N);
         stage.Add(new ZoneLoc(m, new Zone("refugee-spawn-w-n", new Rectangle(highway_origin.Position + zone_length * Direction.N, ew_arrival_zone_dim))));
         stage.Add(new ZoneLoc(m, new Zone("refugee-spawn-w-s", new Rectangle(highway_origin.Position + highway_width * Direction.S, ew_arrival_zone_dim))));
         stage_arrivals[3] = new(m, new(new ZoneLoc(m, new Zone("highway-spawn-w", new Rectangle(highway_origin.Position, ew_arrival_zone_dim))), stage_arrivals[1].Value.Value));

         Interlocked.CompareExchange(ref s_RefugeeSpawnZones, stage.ToArray(), null);
         Interlocked.CompareExchange(ref s_Arrival, stage_arrivals, null);
      }
    }

    private void GameLoop()
    {
      HandleMainMenu();
      if (!m_IsGameRunning) return;
      var world = World.Get;
      _bootstrap_computable(world);
      while (m_IsGameRunning && 0 < world.PlayerCount) {
        var d = world.CurrentPlayerDistrict();
        if (null == d) {
          if (null == world.CurrentSimulationDistrict()) throw new InvalidOperationException("no districts available to simulate");
          if (null == m_SimThread) throw new InvalidOperationException("no simulation thread");
          Thread.Sleep(100);
          continue;
        }
        m_HasLoadedGame = false;
        DateTime now = DateTime.Now;
        AdvancePlay(d, SimFlags.NOT_SIMULATING);
        if (!m_IsGameRunning) break;
        Session.Get.Scoring.RealLifePlayingTime = Session.Get.Scoring.RealLifePlayingTime.Add(DateTime.Now - now);
        world.ScheduleAdjacentForAdvancePlay(d);
      }
    }

    private void InitDirectories()
    {
      int gy1 = 0;
      m_UI.ClearScreen();
      m_UI.UI_DrawStringBold(Color.Yellow, "Checking user game directories...", 0, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_Repaint();
      if (!(  CheckDirectory(GetUserBasePath(), "base user", ref gy1) | CheckDirectory(GetUserConfigPath(), "config", ref gy1)
            | CheckDirectory(GetUserDocsPath(), "docs", ref gy1) | CheckDirectory(GetUserGraveyardPath(), "graveyard", ref gy1)
            | CheckDirectory(GetUserSavesPath(), "saves", ref gy1) | CheckDirectory(GetUserScreenshotsPath(), "screenshots", ref gy1) | CheckCopyOfManual()))
        return;
      m_UI.UI_DrawStringBold(Color.Yellow, "Directories and game manual created.", 0, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.Yellow, "Your game data directory is in the game folder:", 0, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawString(Color.LightGreen, GetUserBasePath(), 0, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.Yellow, "When you uninstall the game you can delete this directory.", 0, gy1, new Color?());
      m_UI.RenderFootnote("press ENTER");
      m_UI.WaitEnter();
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
    public bool ChoiceMenu(Func<int, bool?> choice_handler, Func<int, bool?> setup_handler, int choice_length, Func<Keys,int,bool?>? failover_handler=null)
    {
      int currentChoice = 0;

      bool? code = setup_handler(currentChoice);
      if (null != code) return code.Value;
      m_UI.UI_Repaint();

      bool? handler(KeyEventArgs key) {
        switch(key.KeyCode) {
          case Keys.Return:
            {
            var ret = choice_handler(currentChoice);
            if (null != ret) return ret.Value;
            }
            break;
          case Keys.Up:
            if (currentChoice > 0) --currentChoice;
            else currentChoice = choice_length - 1;
            break;
          case Keys.Down:
            currentChoice = (currentChoice + 1) % choice_length;
            break;
          default:
            if (null == failover_handler) return null;
            {
            var ret = failover_handler(key.KeyCode,currentChoice);
            if (null != ret) return ret.Value;
            }
            break;
        }

        var code2 = setup_handler(currentChoice);
        if (null != code2) return code2.Value;
        m_UI.UI_Repaint();
        return null;
      };

      return m_UI.Modal(handler);
    }

    // boxed value type return value version of ChoiceMenu
    private T? ChoiceMenuNN<T>(Func<int, T?> choice_handler, Func<int, T?> setup_handler, int choice_length, Func<Keys,int,T?>? failover_handler=null) where T:struct
    {
      int currentChoice = 0;

      T? code = setup_handler(currentChoice);
      if (null != code) return code.Value;
      m_UI.UI_Repaint();

      T? handler(KeyEventArgs key) {
        switch(key.KeyCode) {
          case Keys.Return:
            {
            var ret = choice_handler(currentChoice);
            if (null != ret) return ret.Value;
            }
            break;
          case Keys.Up:
            if (currentChoice > 0) --currentChoice;
            else currentChoice = choice_length - 1;
            break;
          case Keys.Down:
            currentChoice = (currentChoice + 1) % choice_length;
            break;
          default:
            if (null == failover_handler) return null;
            {
            var ret = failover_handler(key.KeyCode,currentChoice);
            if (null != ret) return ret.Value;
            }
            break;
        }

        var code2 = setup_handler(currentChoice);
        if (null != code2) return code2.Value;
        m_UI.UI_Repaint();
        return null;
      };
      return m_UI.Modal(handler);
    }

    private void HandleMainMenu()
    {
      bool flag2 = File.Exists(GetUserSave());
      string[] entries = new string[] {
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
        // music.
        if (!m_PlayedIntro) {
          m_MusicManager.Stop();
          m_MusicManager.Play(GameMusics.INTRO, MusicPriority.PRIORITY_BGM);
          m_PlayedIntro = true;
        }
        gy1 = 0;
        m_UI.ClearScreen();
        DrawHeader();
        gy1 += BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.Yellow, "Main Menu", 0, gy1, new Color?());
        gy1 += 2*BOLD_LINE_SPACING;
        DrawMenuOrOptions(c, Color.White, entries, Color.White, null, gx1, ref gy1);
        m_UI.DrawFootnote("cursor to move, ENTER to select");
        DateTime now = DateTime.Now;
        var rules = Rules.Get;
        if (now.Month == 12 && now.Day >= 24 && now.Day <= 26) {
          for (int index = 0; index < 10; ++index) {
            int gx2 = rules.Roll(0, CANVAS_WIDTH-ACTOR_SIZE+ACTOR_OFFSET);
            int gy2 = rules.Roll(0, CANVAS_HEIGHT-ACTOR_SIZE+ACTOR_OFFSET);
            m_UI.UI_DrawImage(GameImages.ACTOR_SANTAMAN, gx2, gy2);
            m_UI.UI_DrawStringBold(Color.Snow, "* Merry Christmas *", gx2 - 60, gy2 - 10, new Color?());
          }
        }
        if ((now.Month == 12 && now.Day >= 31) || (now.Month==1 && now.Day <= 2)) {
          for (int index = 0; index < 10; ++index) {
            int gx2 = rules.Roll(0, CANVAS_WIDTH-ACTOR_SIZE+ACTOR_OFFSET);
            int gy2 = rules.Roll(0, CANVAS_HEIGHT-ACTOR_SIZE+ACTOR_OFFSET);
            m_UI.UI_DrawImage(GameImages.ACTOR_FATHER_TIME, gx2, gy2);
            m_UI.UI_DrawStringBold(Color.Snow, "* Happy New Year *", gx2 - 60, gy2 - 10, new Color?());
          }
        }
        return null;
      });
      Func<int, bool?> choice_handler = (c => {
          switch (c) {
          case 0:
            if (HandleNewCharacter()) {
              StartNewGame();
              return true;
            }
            break;
          case 1:
            if (flag2) {
              gy1 += 2*BOLD_LINE_SPACING;
              m_UI.UI_DrawStringBold(Color.Yellow, "Loading game, please wait...", gx1, gy1, new Color?());
              m_UI.UI_Repaint();
              LoadGame(GetUserSave());
              StartSimThread();
              return true;
            }
            break;
          case 2: s_KeyBindings.HandleRedefineKeys(); break;
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
      DiceRoller roller = new DiceRoller(); // We must not influence map generation.
      if (!HandleNewGameMode()) return false;
      if (Session.CommandLineOptions.ContainsKey("no-spawn")) return true;
      bool? isUndead = HandleNewCharacterRace(roller);
      if (null == isUndead) return false;
      m_CharGen.IsUndead = isUndead.Value;
        // character 2 is the type
        // Living: M)ale, F)emale
        // Undead: Skeleton, Shambler, Zombie Master, z.man, z.woman
        // Livings then specify a starting skill.

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
      }
      return true;
    }
#nullable restore

    private bool HandleNewGameMode()
    {
      string[] entries = Enumerable.Range(0,(int)GameMode_Bounds._COUNT).Select(i => Session.DescGameMode((GameMode)(i))).ToArray();
      string[] values = new string[(int)GameMode_Bounds._COUNT] {
        SetupConfig.GAME_NAME+" standard game.",
        "Don't get a cold. Keep an eye on your deceased diseased friends.",
        "The classic zombies next door.",
        "Almost standard game, disregarding the 1960's Hayes code."
      };
      // XXX \todo should refactor text to react to game mode (proofreading)
      List<string>[] summaries = new List<string>[(int)GameMode_Bounds._COUNT] {
        new List<string>(){
          "This is the standard game setting.",
          "Recommended for beginners.",
          "- All the kinds of undeads.",
          "- Undeads can evolve to stronger forms.",
          "- Livings can zombify instantly when dead.",
          "- No infection.",
          "- No corpses."
        },
        new List<string>(){
          "This is the standard game setting plus corpses and infection.",
          "Recommended to experience all the features of the game.",
          "- All the kinds of undeads.",
          "- Undeads can evolve to stronger forms.",
          "- Infection:",
          "  - some undeads can infect livings when biting them.",
          "  - infected livings can become ill and die.",
          "  - infected corpses have more chances to rise as zombies.",
          "- Corpses:",
          "  - livings that die drop corpses that will rot away.",
          "  - corpses may rise as zombies.",
          "  - undeads can eat corpses.",
          "  - livings can eat corpses if desperate."
        },
        new List<string>(){
          "This is the classic zombies for hardcore zombie fans.",
          "Recommended if you want classic movies zombies.",
          "- Undeads are only zombified men and women.",
          "- Undeads don't evolve to stronger forms.",
          "- Infection:",
          "  - some undeads can infect livings when biting them.",
          "  - infected livings can become ill and die.",
          "  - infected corpses have more chances to rise as zombies.",
          "- Corpses:",
          "  - livings that die drop corpses that will rot away.",
          "  - corpses may rise as zombies.",
          "  - undeads can eat corpses.",
          "  - livings can eat corpses if desperate.",
        },
        new List<string>(){
          "This is the standard game setting, plus corpses",
          "- All the kinds of undeads.",
          "- Undeads can evolve to stronger forms.",
          "- Livings can zombify instantly when dead.",
          "- No infection.",
          "- Corpses:",
          "  - livings that die drop corpses that will rot away.",
          "  - undeads can eat corpses.",
          "  - livings can eat corpses if desperate.",
        }
      };

      bool command_line()
      {
        if (!Session.CommandLineOptions.TryGetValue("spawn",out string x)) return false;
        switch(x.Substring(0,1))
        {
        case "C":   // Classic
          m_CharGen.GameMode = GameMode.GM_STANDARD;
          return true;
        case "I":   // Infection
          m_CharGen.GameMode = GameMode.GM_CORPSES_INFECTION;
          return true;
        case "V":   // Vintage
          m_CharGen.GameMode = GameMode.GM_VINTAGE;
          ApplyOptions(false);
          return true;
        case "Z":   // World War Z, Resident Evil, etc: instant zombification w/corpses
          m_CharGen.GameMode = GameMode.GM_WORLD_WAR_Z;
          return true;
        default: return false;
        }
        // character 0 is the game mode.  Choice values are 0..2 currently
        // we encode this as: C)lassic, I)nfection, V)intage, Z)-war
      }

      if (command_line()) return true;

      const int gx = 0;

      Func<int,bool?> setup_handler = (currentChoice => {
        m_UI.ClearScreen();
        int gy1 = 0;
        m_UI.UI_DrawStringBold(Color.Yellow, "New Game - Choose Game Mode", gx, gy1, new Color?());
        gy1 += 2*BOLD_LINE_SPACING;
        DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGray, values, gx, ref gy1);
        gy1 += 2*BOLD_LINE_SPACING;
        foreach (string text in summaries[currentChoice]) {
          m_UI.UI_DrawStringBold(Color.Gray, text, gx, gy1, new Color?());
          gy1 += BOLD_LINE_SPACING;
        }
        m_UI.DrawFootnote("cursor to move, ENTER to select, ESC to cancel");
        return null;
      });
      Func<int, bool?> choice_handler = (currentChoice => {
        switch (currentChoice) {
          case 0:
            m_CharGen.GameMode = GameMode.GM_STANDARD;
            return true;
          case 1:
            m_CharGen.GameMode = GameMode.GM_CORPSES_INFECTION;
            return true;
          case 2:
            m_CharGen.GameMode = GameMode.GM_VINTAGE;
            ApplyOptions(false);
            return true;
          case 3:
            m_CharGen.GameMode = GameMode.GM_WORLD_WAR_Z;
            return true;
        }
        return null;
      });
      return ChoiceMenu(choice_handler, setup_handler, entries.Length);
    }

    private bool? HandleNewCharacterRace(DiceRoller roller)
    {
      string[] entries = new string[3] {
        "*Random*",
        "Living",
        "Undead"
      };
      string[] values = new string[3] {
        "(picks a race at random for you)",
        "Try to survive.",
        "Eat brains and die again."
      };
      const int gx = 0;
      int gy1 = 0;

      static bool? command_line()
      {
        if (!Session.CommandLineOptions.TryGetValue("spawn",out string x)) return null;
        switch(x.Substring(1,1))
        {
        case "L": return false;
        case "Z": return true;
        default: return null;
        }
        // character 1 is the race.  Choice values are 1..2
        // we encode this as L)iving, Z)ombie
      }

      bool? ret = command_line();
      if (null != ret) return ret;

      Func<int,bool?> setup_handler = (currentChoice => {
        m_UI.ClearScreen();
        gy1 = 0;
        m_UI.UI_DrawStringBold(Color.Yellow, string.Format("[{0}] New Character - Choose Race", Session.DescGameMode(Session.Get.GameMode)), gx, gy1, new Color?());
        gy1 += 2*BOLD_LINE_SPACING;
        DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGray, values, gx, ref gy1);
        m_UI.DrawFootnote("cursor to move, ENTER to select, ESC to cancel");
        return null;
      });
      Func<int, bool?> choice_handler = (currentChoice => {
        switch (currentChoice) {
        case 0:
          bool isUndead = roller.RollChance(50);
          int gy3 = gy1 + 3*BOLD_LINE_SPACING;
          m_UI.UI_DrawStringBold(Color.White, string.Format("Race : {0}.", isUndead ? "Undead" : "Living"), gx, gy3);
          gy3 += BOLD_LINE_SPACING;
          m_UI.UI_DrawStringBold(Color.Yellow, "Is that OK? Y to confirm, N to cancel.", gx, gy3);
          m_UI.UI_Repaint();
          if (m_UI.WaitYesOrNo()) return isUndead;
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
      var male = GameActors.MaleCivilian;
      var female = GameActors.FemaleCivilian;
      string[] entries = new string[3] {
        "*Random*",
        "Male",
        "Female"
      };
      string[] values = new string[3] {
        "(picks a gender at random for you)",
        string.Format("HP:{0:D2}  Def:{1:D2}  Dmg:{2:D1}",  male.BaseHitPoints,  male.BaseDefence.Value,  male.UnarmedAttack.DamageValue),
        string.Format("HP:{0:D2}  Def:{1:D2}  Dmg:{2:D1}",  female.BaseHitPoints,  female.BaseDefence.Value,  female.UnarmedAttack.DamageValue)
      };

      static bool? command_line()
      {
        if (!Session.CommandLineOptions.TryGetValue("spawn",out string x)) return null;
        switch(x.Substring(2,1))
        {
        case "M": return true;
        case "F": return false;
        default: return null;
        }
      }

      bool? ret = command_line();
      if (null != ret) return ret;

      const int gx = 0;
      int gy = 0;

      Func<int,bool?> setup_handler = (currentChoice => {
        m_UI.ClearScreen();
        gy = 0;
        m_UI.UI_DrawStringBold(Color.Yellow, string.Format("[{0}] New Living - Choose Gender", Session.DescGameMode(Session.Get.GameMode)), gx, gy, new Color?());
        gy += 2* BOLD_LINE_SPACING;
        DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGray, values, gx, ref gy);
        m_UI.DrawFootnote("cursor to move, ENTER to select, ESC to cancel");
        return null;
      });
      Func<int, bool?> choice_handler = (currentChoice => {
        switch (currentChoice) {
          case 0:
            bool isMale = roller.RollChance(50);
            gy += BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.White, string.Format("Gender : {0}.", isMale ? "Male" : "Female"), gx, gy, new Color?());
            gy += BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.Yellow, "Is that OK? Y to confirm, N to cancel.", gx, gy, new Color?());
            m_UI.UI_Repaint();
            if (m_UI.WaitYesOrNo()) return isMale;
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
      return string.Format("HP:{0:D3}  Spd:{1:F2}  Atk:{2:D2}  Def:{3:D2}  Dmg:{4:D2}  FoV:{5:D1}  Sml:{6:F2}", m.BaseHitPoints, (float)((double)m.DollBody.Speed / 100.0), m.UnarmedAttack.HitValue, m.BaseDefence.Value, m.UnarmedAttack.DamageValue, m.BaseViewRange, m.BaseSmellRating);
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

      static GameActors.IDs? command_line()
      {
        if (!Session.CommandLineOptions.TryGetValue("spawn",out string x)) return null;
        switch(x.Substring(2,1))
        {
        case "M": return GameActors.MaleZombified.ID;
        case "F": return GameActors.FemaleZombified.ID;
        case "s": return GameActors.Skeleton.ID;
        case "S": return GameActors.Zombie.ID;
        case "Z": return GameActors.ZombieMaster.ID;
        default: return null;
        }
      }

      GameActors.IDs? ret = command_line();
      if (null != ret) return ret;

      const int gx = 0;
      int gy1 = 0;

      Func<int,GameActors.IDs?> setup_handler = (currentChoice => {
        m_UI.ClearScreen();
        gy1 = 0;
        m_UI.UI_DrawStringBold(Color.Yellow, string.Format("[{0}] New Undead - Choose Type", Session.DescGameMode(Session.Get.GameMode)), gx, gy1, new Color?());
        gy1 += 2* BOLD_LINE_SPACING;
        DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGray, values, gx, ref gy1);
        m_UI.DrawFootnote("cursor to move, ENTER to select, ESC to cancel");
        return null;
      });
      Func<int, GameActors.IDs?> choice_handler = (currentChoice => {
        switch (currentChoice) {
          case 0:
            GameActors.IDs modelID = roller.Choose(undead).ID;
            int gy3 = gy1 + BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.White, string.Format("Type : {0}.", GameActors.From(modelID).Name), gx, gy3);
            int gy4 = gy3 + BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(Color.Yellow, "Is that OK? Y to confirm, N to cancel.", gx, gy4);
            m_UI.UI_Repaint();
            if (m_UI.WaitYesOrNo()) return modelID;
            break;
          default: return undead[currentChoice-1].ID;
        }
        return null;
      });
      return ChoiceMenuNN(choice_handler, setup_handler, entries.Length);
    }

    private Skills.IDs? HandleNewCharacterSkill(DiceRoller roller)
    {
      Skills.IDs[] idsArray = Enumerable.Range(0, Skills._LIVING_COUNT).Select(id => (Skills.IDs)id).ToArray();
      string[] entries = (new string[] { "*Random*" }).Concat(idsArray.Select(id => Skills.Name(id))).ToArray();
      string[] values = (new string[] { "(picks a skill at random for you)" }).Concat(idsArray.Select(id => string.Format("{0} max - {1}", Skills.MaxSkillLevel(id), DescribeSkillShort(id)))).ToArray();

      static Skills.IDs? command_line()
      {
        if (!Session.CommandLineOptions.TryGetValue("spawn",out string x)) return null;
        try {
          return (Skills.IDs)Enum.Parse(typeof(Skills.IDs), x.AsSpan(3));
        } catch (ArgumentException) {
          return null;
        }
      }

      Skills.IDs? ret = command_line();
      if (null != ret) return ret;

      const int gx = 0;
      int gy1 = 0;

      Func<int,Skills.IDs?> setup_handler = (currentChoice => {
        gy1 = 0;
        m_UI.ClearScreen();
        m_UI.UI_DrawStringBold(Color.Yellow, string.Format("[{0}] New {1} Character - Choose Starting Skill", Session.DescGameMode(Session.Get.GameMode), m_CharGen.IsMale ? "Male" : "Female"), gx, gy1, new Color?());
        gy1 += 2* BOLD_LINE_SPACING;
        DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGray, values, gx, ref gy1);
        m_UI.DrawFootnote("cursor to move, ENTER to select, ESC to cancel");
        return null;
      });
      Func<int, Skills.IDs?> choice_handler = (currentChoice => {
        Skills.IDs skID = currentChoice != 0 ? (Skills.IDs) (currentChoice - 1) : Skills.RollLiving(roller);
        int gy3 = gy1 + BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.White, string.Format("Skill : {0}.", Skills.Name(skID)), gx, gy3, new Color?());
        int gy4 = gy3 + BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.Yellow, "Is that OK? Y to confirm, N to cancel.", gx, gy4, new Color?());
        m_UI.UI_Repaint();
        if (m_UI.WaitYesOrNo()) return skID;
        return null;
      });
      return ChoiceMenuNN(choice_handler, setup_handler, entries.Length);
    }

    private TextFile? LoadManual()
    {
      m_UI.ClearScreen();
      m_UI.UI_DrawStringBold(Color.White, "Loading game manual...", 0, 0, new Color?());
      int gy1 = BOLD_LINE_SPACING;
      m_UI.UI_Repaint();
      var ret = new TextFile();
      if (!ret.Load(GetUserManualFilePath())) {
        m_UI.UI_DrawStringBold(Color.Red, "Error while loading the manual.", 0, gy1, new Color?());
        gy1 += BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.Red, "The manual won't be available ingame.", 0, gy1, new Color?());
        m_UI.RenderFootnote("press ENTER");
        m_UI.WaitEnter();
        return null;
      } else {
        m_UI.UI_DrawStringBold(Color.White, "Parsing game manual...", 0, gy1, new Color?());
        gy1 += BOLD_LINE_SPACING;
        m_UI.UI_Repaint();
        ret.FormatLines(TEXTFILE_CHARS_PER_LINE);
        m_UI.ClearScreen();
        m_UI.UI_DrawStringBold(Color.White, "Game manual... done!", 0, gy1, new Color?());
        m_UI.UI_Repaint();
      }
      return ret;
    }

    private void HandleHiScores(bool saveToTextfile)
    {
      var textFile = (saveToTextfile ? new TextFile() : null);
      m_UI.ClearScreen();
      DrawHeader();
      int gy1 = BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.Yellow, "Hi Scores", 0, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.White, hr_plus, 0, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.White, "Rank | Name, Skills, Death       |  Score |Difficulty|Survival|  Kills |Achievm.|      Game Time | Playing time", 0, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      if (saveToTextfile) {
        textFile.Append(SetupConfig.GAME_NAME_CAPS+" "+SetupConfig.GAME_VERSION);
        textFile.Append("Hi Scores");
        textFile.Append("Rank | Name, Skills, Death       |  Score |Difficulty|Survival|  Kills |Achievm.|      Game Time | Playing time");
      }
      for (int index = 0; index < m_HiScoreTable.Count; ++index) {
        Color color = index == 0 ? Color.LightYellow : (index == 1 ? Color.LightCyan : (index == 2 ? Color.LightGreen : Color.DimGray));
        m_UI.UI_DrawStringBold(color, hr, 0, gy1, new Color?());
        gy1 += BOLD_LINE_SPACING;
        HiScore hiScore = m_HiScoreTable[index];
        string str = string.Format("{0,3}. | {1,-25} | {2,6} |     {3,3}% | {4,6} | {5,6} | {6,6} | {7,14} | {8}", index + 1, Truncate(hiScore.Name, 25), hiScore.TotalPoints, hiScore.DifficultyPercent, hiScore.SurvivalPoints, hiScore.KillPoints, hiScore.AchievementPoints, new WorldTime(hiScore.TurnSurvived).ToString(), TimeSpanToString(hiScore.PlayingTime));
        m_UI.UI_DrawStringBold(color, str, 0, gy1, new Color?());
        gy1 += BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(color, string.Format("     | {0}.", hiScore.SkillsDescription), 0, gy1, new Color?());
        gy1 += BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(color, string.Format("     | {0}.", hiScore.Death), 0, gy1, new Color?());
        gy1 += BOLD_LINE_SPACING;
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
      gy1 += BOLD_LINE_SPACING;
      if (saveToTextfile) m_UI.UI_DrawStringBold(Color.White, scoreTextFilePath, 0, gy1, new Color?());
      m_UI.RenderFootnote("press ESC to leave");
      m_UI.WaitEscape();
    }

    private void LoadHiScoreTable()
    {
      m_UI.DrawHeadNote("Loading hiscores table...");
      m_HiScoreTable = HiScoreTable.Load(GetUserHiScoreFilePath());
      bool regen_table = (null == m_HiScoreTable);
      if (!regen_table) {
        for (int index = 0; index < m_HiScoreTable.Count; ++index) {
          HiScore hiScore = m_HiScoreTable[index];
          if (!hiScore.is_valid()) {
            regen_table = true;
            break;
          }
        }
      }
      if (regen_table) {
        m_HiScoreTable = new();
        m_HiScoreTable.Clear();
      }

      m_UI.DrawHeadNote("Loading hiscores table... done!");
    }

    private void SaveHiScoreTable()
    {
      m_UI.DrawHeadNote("Saving hiscores table...");
      HiScoreTable.Save(m_HiScoreTable, GetUserHiScoreFilePath());
      m_UI.DrawHeadNote("Saving hiscores table... done!");
    }

    private void StartNewGame()
    {
      GenerateWorld(true);
      bool isUndead = Player.Model.Abilities.IsUndead;
      // XXX \todo should do this for all actors
      Player.ActorScoring.AddVisit(Session.Get.WorldTime.TurnCounter, Player.Location.Map);
      Player.ActorScoring.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format(isUndead ? "Rose in {0}." : "Woke up in {0}.", Player.Location.Map.Name));
      if (s_Options.IsAdvisorEnabled) {
        Messages.Clear();
        Messages.ClearHistory();
        if (isUndead) {    // alpha10
          AddMessage(new("The Advisor is enabled but you will get no hint when playing undead.", 0, Color.Red));
        } else {
          AddMessage(new("The Advisor is enabled and will give you hints during the game.", 0, Color.LightGreen));
          AddMessage(new("The hints help a beginner learning the basic controls.", 0, Color.LightGreen));
          AddMessage(new("You can disable the Advisor by going to the Options screen.", 0, Color.LightGreen));
        }
        AddMessage(new(string.Format("Press {0} during the game to change the options.", s_KeyBindings.AsString(PlayerCommand.OPTIONS_MODE)), 0, Color.LightGreen));
        RedrawPlayScreen(new UI.Message("<press ENTER>", 0, Color.Yellow));
        m_UI.WaitEnter();
      }
      Messages.Clear();
      Messages.ClearHistory();
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
      AddMessage(new(header, 0, Color.LightGreen));
      AddMessage(new("* " + msg1 + " *", 0, Color.LightGreen));
      AddMessage(new("* " + msg2 + " *", 0, Color.LightGreen));
      AddMessage(new(header, 0, Color.LightGreen));
      AddMessage(new(string.Format("Press {0} for help", s_KeyBindings.AsString(PlayerCommand.HELP_MODE)), 0, Color.LightGreen));
      AddMessage(new(string.Format("Press {0} to redefine keys", s_KeyBindings.AsString(PlayerCommand.KEYBINDING_MODE)), 0, Color.LightGreen));
      AddMessage(new("<press ENTER>", 0, Color.Yellow));
      RefreshPlayer();
      RedrawPlayScreen();
      m_UI.WaitEnter();
      ClearMessages();
      RedrawPlayScreen(new UI.Message(string.Format(isUndead ? "{0} rises..." : "{0} wakes up.", Player.Name), 0, Color.White));
      play_timer.Start();
      World.Get.ScheduleForAdvancePlay();   // simulation starts at district A1
      StopSimThread(false);  // alpha10 stop-start
      StartSimThread();
    }

    private void HandleCredits()
    {
      m_MusicManager.Stop();
      m_MusicManager.PlayLooping(GameMusics.SLEEP, MusicPriority.PRIORITY_BGM);
      m_UI.ClearScreen();
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
      m_UI.UI_DrawString(Color.White, "source code: https://github.com/zaimoni/RSRevived", 0, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.White, "Thanks to the players for their feedback and eagerness to die!", 0, gy1, new Color?());
      m_UI.RenderFootnote("ESC to leave");
      m_UI.WaitEscape();
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
      // alpha10: policy to separate option mode modifiers from main option description
      entries[Array.IndexOf(idsArray, GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION)] += " !V";
      entries[Array.IndexOf(idsArray, GameOptions.IDs.GAME_RATS_UPGRADE)] += " !V";
      entries[Array.IndexOf(idsArray, GameOptions.IDs.GAME_SKELETONS_UPGRADE)] += " !V";
      entries[Array.IndexOf(idsArray, GameOptions.IDs.GAME_SHAMBLERS_UPGRADE)] += " !V";
      entries[Array.IndexOf(idsArray, GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE)] += " =S";  // XXX also World War Z but that's vaporware
      entries[Array.IndexOf(idsArray, GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE)] += " =S";
      char[] newlines = { '\n' };  // alpha10
      char[] spaces = { ' ' }; // alpha10

      Func<int,bool?> setup_handler = (currentChoice => {
        ApplyOptions(false);
        string[] values = idsArray.Select(x => s_Options.DescribeValue(x)).ToArray();
        int gy;
        int gx = gy = 0;
        m_UI.ClearScreen();
        DrawHeader();
        gy += BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.Yellow, string.Format("[{0}] - Options", Session.DescGameMode(Session.Get.GameMode)), 0, gy, new Color?());
        gy += 2*BOLD_LINE_SPACING;
        DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGreen, values, gx, ref gy, false, 400);
        gy += BOLD_LINE_SPACING;

        // alpha10
        // describe current option.
        gy += BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.White, entries[currentChoice].TrimStart(spaces), gx, gy);
        gy += BOLD_LINE_SPACING;
        string desc = GameOptions.Describe(idsArray[currentChoice]);
        string[] descLines = desc.Split(newlines);
        foreach (string d in descLines) {
          m_UI.UI_DrawString(Color.White, "  " + d, gx, gy);
          gy += BOLD_LINE_SPACING;
        }

        m_UI.UI_DrawStringBold(Color.Red, "* Caution : increasing these values makes the game runs slower and saving/loading longer.", gx, gy, new Color?());
        gy += 2*BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.White, "-V : option always OFF when playing VTG-Vintage", gx, gy);
        gy += BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.White, "=S : option used only when playing STD-Standard", gx, gy);
        gy += BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.Yellow, string.Format("Difficulty Rating : {0}% as survivor / {1}% as undead.", (int)(100.0 * (double)s_Options.DifficultyRating(GameFactions.IDs.TheCivilians)), (int)(100.0 * (double)s_Options.DifficultyRating(GameFactions.IDs.TheUndeads))), gx, gy, new Color?());
        gy += BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.White, "Difficulty used for scoring automatically decrease with each reincarnation.", gx, gy, new Color?());
        m_UI.DrawFootnote("cursor to move and change values, R to restore previous values, ESC to save and leave");
        return null;
      });

      static bool? choice_handler(int currentChoice) { return null; }

      Func<Keys,int,bool?> failover_handler = ((k,currentChoice) => {
        switch (k) {
          case Keys.Left:
            switch (idsArray[currentChoice])
            {
              case GameOptions.IDs.UI_MUSIC: s_Options.PlayMusic = !s_Options.PlayMusic; break;
              case GameOptions.IDs.UI_MUSIC_VOLUME: s_Options.MusicVolume -= 5; break;
              case GameOptions.IDs.UI_SHOW_PLAYER_TAG_ON_MINIMAP: s_Options.ShowPlayerTagsOnMinimap = !s_Options.ShowPlayerTagsOnMinimap; break;
              case GameOptions.IDs.UI_ANIM_DELAY: s_Options.IsAnimDelayOn = !s_Options.IsAnimDelayOn; break;
              case GameOptions.IDs.UI_SHOW_MINIMAP: s_Options.IsMinimapOn = !s_Options.IsMinimapOn; break;
              case GameOptions.IDs.UI_ADVISOR: s_Options.IsAdvisorEnabled = !s_Options.IsAdvisorEnabled; break;
              case GameOptions.IDs.UI_COMBAT_ASSISTANT: s_Options.IsCombatAssistantOn = !s_Options.IsCombatAssistantOn; break;
              case GameOptions.IDs.UI_SHOW_TARGETS: s_Options.ShowTargets = !s_Options.ShowTargets; break;
              case GameOptions.IDs.UI_SHOW_PLAYER_TARGETS: s_Options.ShowPlayerTargets = !s_Options.ShowPlayerTargets; break;
              case GameOptions.IDs.GAME_DISTRICT_SIZE: s_Options.DistrictSize -= 5; break;
              case GameOptions.IDs.GAME_MAX_CIVILIANS: s_Options.MaxCivilians -= 5; break;
              case GameOptions.IDs.GAME_MAX_DOGS: --s_Options.MaxDogs; break;
              case GameOptions.IDs.GAME_MAX_UNDEADS: s_Options.MaxUndeads -= 10; break;
              case GameOptions.IDs.GAME_SIMULATE_DISTRICTS: break;
              case GameOptions.IDs.GAME_SIMULATE_SLEEP: break;
              case GameOptions.IDs.GAME_SIM_THREAD: break;
              case GameOptions.IDs.GAME_CITY_SIZE: --s_Options.CitySize; break;
              case GameOptions.IDs.GAME_NPC_CAN_STARVE_TO_DEATH: s_Options.NPCCanStarveToDeath = !s_Options.NPCCanStarveToDeath; break;
              case GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE: s_Options.ZombificationChance -= 5; break;
              case GameOptions.IDs.GAME_REVEAL_STARTING_DISTRICT: s_Options.RevealStartingDistrict = !s_Options.RevealStartingDistrict; break;
              case GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION: s_Options.AllowUndeadsEvolution = !s_Options.AllowUndeadsEvolution; break;
              case GameOptions.IDs.GAME_DAY_ZERO_UNDEADS_PERCENT: s_Options.DayZeroUndeadsPercent -= 5; break;
              case GameOptions.IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE: --s_Options.ZombieInvasionDailyIncrease; break;
              case GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE: s_Options.StarvedZombificationChance -= 5; break;
              case GameOptions.IDs.GAME_MAX_REINCARNATIONS: --s_Options.MaxReincarnations; break;
              case GameOptions.IDs.GAME_REINCARNATE_AS_RAT: s_Options.CanReincarnateAsRat = !s_Options.CanReincarnateAsRat; break;
              case GameOptions.IDs.GAME_REINCARNATE_TO_SEWERS: s_Options.CanReincarnateToSewers = !s_Options.CanReincarnateToSewers; break;
              case GameOptions.IDs.GAME_REINC_LIVING_RESTRICTED: s_Options.IsLivingReincRestricted = !s_Options.IsLivingReincRestricted; break;
              case GameOptions.IDs.GAME_PERMADEATH: s_Options.IsPermadeathOn = !s_Options.IsPermadeathOn; break;
              case GameOptions.IDs.GAME_DEATH_SCREENSHOT: s_Options.IsDeathScreenshotOn = !s_Options.IsDeathScreenshotOn; break;
              case GameOptions.IDs.GAME_AGGRESSIVE_HUNGRY_CIVILIANS: s_Options.IsAggressiveHungryCiviliansOn = !s_Options.IsAggressiveHungryCiviliansOn; break;
              case GameOptions.IDs.GAME_NATGUARD_FACTOR: s_Options.NatGuardFactor -= 10; break;
              case GameOptions.IDs.GAME_SUPPLIESDROP_FACTOR: s_Options.SuppliesDropFactor -= 10; break;
              case GameOptions.IDs.GAME_UNDEADS_UPGRADE_DAYS:
                if (s_Options.ZombifiedsUpgradeDays != GameOptions.ZupDays.ONE) s_Options.ZombifiedsUpgradeDays -= 1;
                break;
              case GameOptions.IDs.GAME_RATS_UPGRADE: s_Options.RatsUpgrade = !s_Options.RatsUpgrade; break;
              case GameOptions.IDs.GAME_SKELETONS_UPGRADE: s_Options.SkeletonsUpgrade = !s_Options.SkeletonsUpgrade; break;
              case GameOptions.IDs.GAME_SHAMBLERS_UPGRADE: s_Options.ShamblersUpgrade = !s_Options.ShamblersUpgrade; break;
            }
            break;
          case Keys.Right:
            switch (idsArray[currentChoice]) {
              case GameOptions.IDs.UI_MUSIC: s_Options.PlayMusic = !s_Options.PlayMusic; break;
              case GameOptions.IDs.UI_MUSIC_VOLUME: s_Options.MusicVolume += 5; break;
              case GameOptions.IDs.UI_SHOW_PLAYER_TAG_ON_MINIMAP: s_Options.ShowPlayerTagsOnMinimap = !s_Options.ShowPlayerTagsOnMinimap; break;
              case GameOptions.IDs.UI_ANIM_DELAY: s_Options.IsAnimDelayOn = !s_Options.IsAnimDelayOn; break;
              case GameOptions.IDs.UI_SHOW_MINIMAP: s_Options.IsMinimapOn = !s_Options.IsMinimapOn; break;
              case GameOptions.IDs.UI_ADVISOR: s_Options.IsAdvisorEnabled = !s_Options.IsAdvisorEnabled; break;
              case GameOptions.IDs.UI_COMBAT_ASSISTANT: s_Options.IsCombatAssistantOn = !s_Options.IsCombatAssistantOn; break;
              case GameOptions.IDs.UI_SHOW_TARGETS: s_Options.ShowTargets = !s_Options.ShowTargets; break;
              case GameOptions.IDs.UI_SHOW_PLAYER_TARGETS: s_Options.ShowPlayerTargets = !s_Options.ShowPlayerTargets; break;
              case GameOptions.IDs.GAME_DISTRICT_SIZE: s_Options.DistrictSize += 5; break;
              case GameOptions.IDs.GAME_MAX_CIVILIANS: s_Options.MaxCivilians += 5; break;
              case GameOptions.IDs.GAME_MAX_DOGS: ++s_Options.MaxDogs; break;
              case GameOptions.IDs.GAME_MAX_UNDEADS: s_Options.MaxUndeads += 10; break;
              case GameOptions.IDs.GAME_SIMULATE_DISTRICTS: break;
              case GameOptions.IDs.GAME_SIMULATE_SLEEP: break;
              case GameOptions.IDs.GAME_SIM_THREAD: break;
              case GameOptions.IDs.GAME_CITY_SIZE: ++s_Options.CitySize; break;
              case GameOptions.IDs.GAME_NPC_CAN_STARVE_TO_DEATH: s_Options.NPCCanStarveToDeath = !s_Options.NPCCanStarveToDeath; break;
              case GameOptions.IDs.GAME_ZOMBIFICATION_CHANCE: s_Options.ZombificationChance += 5; break;
              case GameOptions.IDs.GAME_REVEAL_STARTING_DISTRICT: s_Options.RevealStartingDistrict = !s_Options.RevealStartingDistrict; break;
              case GameOptions.IDs.GAME_ALLOW_UNDEADS_EVOLUTION: s_Options.AllowUndeadsEvolution = !s_Options.AllowUndeadsEvolution; break;
              case GameOptions.IDs.GAME_DAY_ZERO_UNDEADS_PERCENT: s_Options.DayZeroUndeadsPercent += 5; break;
              case GameOptions.IDs.GAME_ZOMBIE_INVASION_DAILY_INCREASE: ++s_Options.ZombieInvasionDailyIncrease; break;
              case GameOptions.IDs.GAME_STARVED_ZOMBIFICATION_CHANCE: s_Options.StarvedZombificationChance += 5; break;
              case GameOptions.IDs.GAME_MAX_REINCARNATIONS: ++s_Options.MaxReincarnations; break;
              case GameOptions.IDs.GAME_REINCARNATE_AS_RAT: s_Options.CanReincarnateAsRat = !s_Options.CanReincarnateAsRat; break;
              case GameOptions.IDs.GAME_REINCARNATE_TO_SEWERS: s_Options.CanReincarnateToSewers = !s_Options.CanReincarnateToSewers; break;
              case GameOptions.IDs.GAME_REINC_LIVING_RESTRICTED: s_Options.IsLivingReincRestricted = !s_Options.IsLivingReincRestricted; break;
              case GameOptions.IDs.GAME_PERMADEATH: s_Options.IsPermadeathOn = !s_Options.IsPermadeathOn; break;
              case GameOptions.IDs.GAME_DEATH_SCREENSHOT: s_Options.IsDeathScreenshotOn = !s_Options.IsDeathScreenshotOn; break;
              case GameOptions.IDs.GAME_AGGRESSIVE_HUNGRY_CIVILIANS: s_Options.IsAggressiveHungryCiviliansOn = !s_Options.IsAggressiveHungryCiviliansOn; break;
              case GameOptions.IDs.GAME_NATGUARD_FACTOR: s_Options.NatGuardFactor += 10; break;
              case GameOptions.IDs.GAME_SUPPLIESDROP_FACTOR: s_Options.SuppliesDropFactor += 10; break;
              case GameOptions.IDs.GAME_UNDEADS_UPGRADE_DAYS:
                if (s_Options.ZombifiedsUpgradeDays != GameOptions.ZupDays.OFF) s_Options.ZombifiedsUpgradeDays += 1;
                break;
              case GameOptions.IDs.GAME_RATS_UPGRADE: s_Options.RatsUpgrade = !s_Options.RatsUpgrade; break;
              case GameOptions.IDs.GAME_SKELETONS_UPGRADE: s_Options.SkeletonsUpgrade = !s_Options.SkeletonsUpgrade; break;
              case GameOptions.IDs.GAME_SHAMBLERS_UPGRADE: s_Options.ShamblersUpgrade = !s_Options.ShamblersUpgrade; break;
            }
            break;
          case Keys.R: s_Options = gameOptions; break;
        }
        return null;
      });
      ChoiceMenu(choice_handler, setup_handler, entries.Length, failover_handler);
      ApplyOptions(false);
      SaveOptions();
    }

    private void FindPlayer()
    {
       var tmp = CurrentMap.FindPlayer;
       if (null != tmp) {
         setPlayer(tmp).Controller.UpdateSensors();
         SetCurrentMap(Player.Location);
         RedrawPlayScreen();
         return;
       }
       // check all maps in current district
       tmp = CurrentMap.District.FindPlayer(CurrentMap);
       if (null != tmp) {
         setPlayer(tmp);
         return;
       }

       // check all other districts
       foreach(District tmp2 in World.Get.PlayerDistricts) {
         tmp = tmp2.FindPlayer(null);
         if (null != tmp) {
           setPlayer(tmp);
           return;
         }
       }
    }

#nullable enable
    private void AdvancePlay(District district, SimFlags sim)
    {
      var sess = Session.Get;
      var world = World.Get;
      DayPhase phase1 = sess.WorldTime.Phase;
#if DATAFLOW_TRACE
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "District: "+district.Name);
#endif

      lock (district) {
      if (!world.WantToAdvancePlay(district)) return;
      bool IsPCdistrict = (0 < district.PlayerCount);

      do {
      foreach(Map current in district.Maps) {
        // not processing secret maps used to be a micro-optimization; now a hang bug
        if (current.IsSecret) continue;
#if IRRATIONAL_CAUTION
        int AP_checkpoint = 0;
        Actor? last = null;
#endif
        Actor? next;
        while(null != (next = current.NextActorToAct)) {
          if (IsSimulating) {
            lock (m_SimStateLock) { if (m_CancelSource.IsCancellationRequested) return; } // .NET 5.0 conversion
          }
#if DEBUG
          // following is a check for a problem in RS Alpha where the AI gets many turns before the player gets one.
          if (next.ActionPoints > next.Model.DollBody.Speed) throw new InvalidOperationException(next.Name+" is hyperactive: energy limit, "+ next.Model.DollBody.Speed.ToString() + " actual "+ next.ActionPoints.ToString());
#endif
#if IRRATIONAL_CAUTION
          if (last != next) AP_checkpoint = next.ActionPoints;
          else if (AP_checkpoint < next.ActionPoints) throw new InvalidOperationException("regained energy outside of next map turn processing");
          Map map_checkpoint = next.Location.Map;
#endif
          AdvancePlay(current);
#if IRRATIONAL_CAUTION
          if (AP_checkpoint < next.ActionPoints && map_checkpoint==next.Location.Map) throw new InvalidOperationException("regained energy outside of next map turn processing");
          else if (0 >= (AP_checkpoint = next.ActionPoints) && next == current.NextActorToAct) throw new InvalidOperationException("actor not rotating properly");
          last = next;
#endif
          if (district == CurrentMap.District) { // Bay12/jorgene0: do not let simulation thread process reincarnation
            if (0>= world.PlayerCount) HandleReincarnation();
          }
          if (!m_IsGameRunning || m_HasLoadedGame || (IsPCdistrict && 0>= world.PlayerCount)) return;
        }
      }
      } while(!district.ReadyForNextTurn);  // do-while prevents time skew at world level

      foreach(Map current in district.Maps) NextMapTurn(current, sim);

      // XXX message generation wrappers do not have access to map time, only world time
      // XXX this set of messages must execute only once
      // XXX the displayed turn on the message must agree with the displayed turn on the screen
      if (world.Last == district) {
        bool canSeeSky = Player.CanSeeSky;  // alpha10 message ony if can see sky
        int turn = sess.WorldTime.TurnCounter;

        DayPhase phase2 = sess.WorldTime.Phase;
        if (sess.WorldTime.IsDawn) {
          if (canSeeSky) AddMessage(new("The sun is rising again for you...", turn, DAY_COLOR));
          OnNewDay();
        } else if (sess.WorldTime.IsDusk) {
          if (canSeeSky) AddMessage(new("Night is falling upon you...", turn, NIGHT_COLOR));
          OnNewNight();
        } else if (phase1 != phase2) {
          if (canSeeSky) AddMessage(new(string.Format("Time passes, it is now {0}...", phase2.to_s()), turn, sess.WorldTime.IsNight ? NIGHT_COLOR : DAY_COLOR));
        }

        // alpha10
        // if time to change weather do it and roll next change time.
        if (turn >= world.NextWeatherCheckTurn) ChangeWeather();

        // handle unconditional time caches here
        Direction_ext.Now();
      }

      // It looks strange to look across the district boundary and see a z-invasion
      // but if the map were large enough for timezones to be significant, trying to do the whole world at once would cause non-midnight invasions
      // vertical slicing doesn't quite work either (midnight is fine but dawn/dusk are latitude-sensitive)

      // with the current scheduler, we could fire the events NW on completing ourselves.
      // * south border: events to W as well
      // * east border: events to N as well
      // * Last (SE corner): self-events

      // Z invasions can remain anchored to the ley lines (district boundaries) indefinitely.
      // VAPORWARE Other events should be logistically sensitive
      // * National Guard, Army Supplies, Black Ops: helicopters.  Do not get along well with trees or power lines, but the CHAR HQ city has no power lines.
      // * Refugees (Civilians (any but likely to favor cars and SUVs, pickup trucks, mopeds, and motorbikes are theoretically possible but rare),
     //    Bikers (motorbikes), Gangstas (cars), Survivors (vans, pickup trucks, or SUVs): 
      //   arrive by road (i.e. arrive on the outer edge of the outer districts)

      // the next district type would be "I-435 freeway" (a road ring encircling the city proper).  We need a low enough CPU/RAM loading to pay for this.
      if (!World.Edge_N_or_W(district)) {
        EndTurnDistrictEvents(world[district.WorldPosition+Direction.NW]);
        if (world.Edge_S(district)) EndTurnDistrictEvents(world[district.WorldPosition + Direction.W]);
        if (world.Edge_E(district)) EndTurnDistrictEvents(world[district.WorldPosition + Direction.N]);
        if (world.Last == district) EndTurnDistrictEvents(district);
      }
      district.EndTurn();
      } // end lock (district)
      RedrawPlayScreen();   // \todo this is to update the time elapsed and minimap (doesn't need to try to run if minimap not in scope)
#if DATAFLOW_TRACE
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "District finished: "+district.Name);
#endif
    }
#nullable restore

    private void EndTurnDistrictEvents(District d)
    { // historically, all districts were within city limits so they all could use the same event specifications.
      CheckFor_Fire_ZombieInvasion(d.EntryMap);
      if (CheckForEvent_ScheduleRefugees(d.EntryMap)) FireEvent_ScheduleRefugees(d);
      CheckFor_Fire_RefugeeParty(d);
      if (CheckForEvent_NationalGuard(d.EntryMap)) FireEvent_NationalGuard(d.EntryMap);
      if (CheckForEvent_ArmySupplies(d.EntryMap)) FireEvent_ArmySupplies(d.EntryMap);
      if (CheckForEvent_BikersRaid(d.EntryMap)) FireEvent_BikersRaid(d.EntryMap);
      if (CheckForEvent_GangstasRaid(d.EntryMap)) FireEvent_GangstasRaid(d.EntryMap);
      if (CheckForEvent_BlackOpsRaid(d.EntryMap)) FireEvent_BlackOpsRaid(d.EntryMap);
      if (CheckForEvent_BandOfSurvivors(d.EntryMap)) FireEvent_BandOfSurvivors(d.EntryMap);
      CheckFor_Fire_SewersInvasion(d.SewersMap);
    }

    // we would prefer to notify in a radius.  Blocked by rewriting raids to not have ley-line behavior
    static private void NotifyOrderablesAI(RaidType raid, Location loc)
    {
      OrderableAI.BeforeRaid(raid, in loc);
      foreach (Actor actor in loc.Map.Actors) {
        (actor.Controller as ObjectiveAI)?.OnRaid(raid, in loc);
      }
      OrderableAI.AfterRaid();
    }

    private void AdvancePlay(Map map)
    {
#if DATAFLOW_TRACE
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "Map: "+map.Name);
#endif
       // undiscovered CHAR base is in stasis
#if DEBUG
      if (map.IsSecret) throw new InvalidProgramException("do not try to process secret maps");
#else
      if (map.IsSecret) return; // undiscovered CHAR base is in stasis
#endif

      Actor? nextActorToAct = map.NextActorToAct;
#if DEBUG
      if (nextActorToAct == null) throw new InvalidProgramException("need actor to handle");
#else
      if (nextActorToAct == null) return; // undiscovered CHAR base is in stasis
#endif

#if DEBUG
      if (nextActorToAct.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "Actor: "+ nextActorToAct.Name);
#endif
      nextActorToAct.PreviousStaminaPoints = nextActorToAct.StaminaPoints;

      var police = Session.Get.Police;
      if (police.IsMine(nextActorToAct)) police.onTurnStart(nextActorToAct);
      else {
        var leader = nextActorToAct.LiveLeader;
        if (null!=leader && police.IsMine(leader) && (nextActorToAct.Controller as ObjectiveAI).InCommunicationWith(leader))
             police.onTurnStart(nextActorToAct);
      }

      if (nextActorToAct.Controller == null)
#if DEBUG
        throw new InvalidOperationException("nextActorToAct.Controller == null");
#else
        nextActorToAct.SpendActionPoints();
#endif
      else if (nextActorToAct.Controller is PlayerController pc) {
        HandlePlayerActor(pc);
        if (!m_IsGameRunning || m_HasLoadedGame || 0>= World.Get.PlayerCount) return;
        if (!nextActorToAct.IsDead) CheckSpecialPlayerEventsAfterAction(nextActorToAct);
      } else {
#if PROTOTYPE
        // alpha10 ai loop bug detection; not multi-thread aware.  \todo fix this
        if (actor == m_DEBUG_prevAiActor) {
          if (++m_DEBUG_sameAiActorCount >= DEBUG_AI_ACTOR_LOOP_COUNT_WARNING) {
            // TO DEVS: you might want to add a debug breakpoint here ->
            Logger.WriteLine(Logger.Stage.RUN_MAIN, "WARNING: AI actor " + actor.Name + " is probably looping!!");
#if DEBUG
            // in debug keep going to let us debug the ai
#else
            // in release throw an exception as infinite loop is a fatal bug
            Exception e = new InvalidOperationException("an AI actor is looping, please report the exception details");
            Logger.WriteLine(Logger.Stage.RUN_MAIN, "AI stacktrace:" + e.StackTrace);
#endif
          }
        } else {
          m_DEBUG_sameAiActorCount = 0;
          m_DEBUG_prevAiActor = actor;
        }
#endif

        HandleAiActor(nextActorToAct.Controller);
      }
      if (nextActorToAct.AfterAction()) map.AfterAction();
    }

#nullable enable
    public void PlayEventMusic(string music)
    {
      m_MusicManager.Stop();
      m_MusicManager.Play(music, MusicPriority.PRIORITY_EVENT);
    }
#nullable restore

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

      var rules = Rules.Get;
      if ((sim & RogueGame.SimFlags.LODETAIL_TURN) == RogueGame.SimFlags.NOT_SIMULATING) {
        if (Session.Get.HasCorpses && map.HasCorpses) {
#region corpses: decide who zombify or rots.
          List<Corpse> corpseList1 = new();
          List<Corpse> corpseList2 = new();

          void classify_corpse(Corpse corpse) {
            if (rules.RollChance(corpse.ZombifyChance(map.LocalTime, true))) {
              corpseList1.Add(corpse);
            } else if (corpse.TakeDamage(Corpse.DecayPerTurn())) {
              corpseList2.Add(corpse);
            }
          }

          map.DoForAll(classify_corpse);

          foreach (var corpse in corpseList1) {
              if (!corpse.Location.StrictHasActorAt) {
                Zombify(null, corpse.DeadGuy, false);
                var witnesses = _ForceVisibleToPlayer(corpse.Location);
                if (null != witnesses) {
                    RedrawPlayScreen(witnesses, new("The " + corpse.ToString() + " rises again!!", map.LocalTime.TurnCounter, Color.Red));
                    m_MusicManager.Play(GameSounds.UNDEAD_RISE, MusicPriority.PRIORITY_EVENT);
                }
                map.Destroy(corpse);
              }
          }
          // RS Alpha 10.1-: players are special, corpses wait for players to turn into dust
          // don't really have the RAM to do anything complex, like inanimate skeletons.
          foreach (Corpse c in corpseList2) {
            map.Destroy(c);
            var witnesses = _ForceVisibleToPlayer(c.Location);
            if (null != witnesses) RedrawPlayScreen(witnesses, new("The " + c.ToString() + " turns into dust.", map.LocalTime.TurnCounter, Color.Purple));
          }
#endregion
        }
        if (Session.Get.HasInfection) {
#region Infection effects
          List<Actor> actorList = null;
          foreach (Actor actor in map.Actors) {
            if (actor.Infection >= Rules.INFECTION_LEVEL_1_WEAK && !actor.Model.Abilities.IsUndead) {
              int infectionPercent = actor.InfectionPercent;
              if (rules.Roll(0, 1000) < Rules.InfectionEffectTriggerChance1000(infectionPercent)) {
                var witnesses = actor.PlayersInLOS();
                if (actor.IsSleeping) DoWakeUp(actor);
                bool flag4 = false;
                if (infectionPercent >= Rules.INFECTION_LEVEL_5_DEATH) flag4 = true;
                else if (infectionPercent >= Rules.INFECTION_LEVEL_4_BLEED) {
                  actor.Vomit();
                  if (null != witnesses) actor.Controller.AddMessageForceReadClear(MakePanopticMessage(actor, string.Format("{0} blood.", VERB_VOMIT.Conjugate(actor)), Color.Purple), witnesses);
                  if (actor.RawDamage(Rules.INFECTION_LEVEL_4_BLEED_HP)) flag4 = true;
                } else if (infectionPercent >= Rules.INFECTION_LEVEL_3_VOMIT) {
                  actor.Vomit();
                  if (null != witnesses) actor.Controller.AddMessageForceReadClear(MakePanopticMessage(actor, string.Format("{0}.", VERB_VOMIT.Conjugate(actor)), Color.Purple), witnesses);
                } else if (infectionPercent >= Rules.INFECTION_LEVEL_2_TIRED) {
                  actor.SpendStaminaPoints(Rules.INFECTION_LEVEL_2_TIRED_STA);
                  actor.Drowse(Rules.INFECTION_LEVEL_2_TIRED_SLP);
                  if (null != witnesses) actor.Controller.AddMessageForceReadClear(MakePanopticMessage(actor, string.Format("{0} sick and tired.", VERB_FEEL.Conjugate(actor)), Color.Purple), witnesses);
                } else if (infectionPercent >= Rules.INFECTION_LEVEL_1_WEAK) {
                  actor.SpendStaminaPoints(Rules.INFECTION_LEVEL_1_WEAK_STA);
                  if (null != witnesses) actor.Controller.AddMessageForceReadClear(MakePanopticMessage(actor, string.Format("{0} sick and weak.", VERB_FEEL.Conjugate(actor)), Color.Purple), witnesses);
                }
                if (flag4) (actorList ??= new List<Actor>()).Add(actor);
              }
            }
          }
          if (actorList != null) {
            foreach (Actor actor in actorList) {
              var witnesses = actor.PlayersInLOS();
              if (null != witnesses) {
                RedrawPlayScreen(witnesses, MakePanopticMessage(actor, string.Format("{0} of infection!", VERB_DIE.Conjugate(actor))));
              }
              KillActor(null, actor, "infection");
            }
          }
#endregion
        }

        map.DecayScents();  // 1. Update odors.

        // 2. Regen actors AP & STA
        map.PreTurnStart();
#region 4. Actor gauges & states
        List<Actor> actorList1 = null;
        foreach (Actor actor in map.Actors) {
#region hunger & rot
          if (actor.Model.Abilities.HasToEat) {
            actor.Appetite(1);
            if (actor.IsStarving && (actor.IsPlayer || s_Options.NPCCanStarveToDeath) && rules.RollChance(Rules.FOOD_STARVING_DEATH_CHANCE)) {
              (actorList1 ??= new()).Add(actor);
            }
          } else if (actor.Model.Abilities.IsRotting) {
            actor.Appetite(1);
            if (actor.IsRotStarving && rules.Roll(0, 1000) < Rules.ROT_STARVING_HP_CHANCE) {
              var witnesses = actor.PlayersInLOS();
              if (null != witnesses) {
                RedrawPlayScreen(witnesses, MakePanopticMessage(actor, "is rotting away."));
              }
              if (actor.RawDamage(1)) (actorList1 ??= new()).Add(actor);
            }
            else if (actor.IsRotHungry && rules.Roll(0, 1000) < Rules.ROT_HUNGRY_SKILL_CHANCE)
              DoLooseRandomSkill(actor);
          }
#endregion
          if (actor.Model.Abilities.HasToSleep) {
#region sleep.
            if (actor.IsSleeping) {
              if (actor.IsDisturbed && rules.RollChance(Rules.SANITY_NIGHTMARE_CHANCE)) {
                DoWakeUp(actor);
                DoShout(actor, "NO! LEAVE ME ALONE!");
                actor.Drowse(Rules.SANITY_NIGHTMARE_SLP_LOSS);
                actor.SpendSanity(Rules.SANITY_NIGHTMARE_SAN_LOSS);
                actor.SpendStaminaPoints(Rules.SANITY_NIGHTMARE_STA_LOSS);
                var witnesses = actor.PlayersInLOS();
                if (null != witnesses) {
                  RedrawPlayScreen(witnesses, MakePanopticMessage(actor, string.Format("{0} from a horrible nightmare!", VERB_WAKE_UP.Conjugate(actor))));
                }
                if (actor.IsPlayer) {
                   // FIXME replace with sfx
                   // alpha10 
                   m_MusicManager.Stop();
                   m_MusicManager.Play(GameSounds.NIGHTMARE, MusicPriority.PRIORITY_EVENT);
                }
              }
            } else {
              actor.Drowse(map.LocalTime.IsNight ? 2 : 1);
            }
            if (actor.IsSleeping) {
#region 4.2 Handle sleeping actors.
              bool isOnCouch = actor.IsOnCouch;
              actor.Activity = Data.Activity.SLEEPING;
              actor.Rest(actor.SleepRegen(isOnCouch));
              if (actor.HitPoints < actor.MaxHPs && rules.RollChance((isOnCouch ? Rules.SLEEP_ON_COUCH_HEAL_CHANCE : 0) + actor.HealChanceBonus))
                actor.RegenHitPoints(Rules.SLEEP_HEAL_HITPOINTS);
              if (actor.IsHungry || actor.SleepPoints >= actor.MaxSleep)
                DoWakeUp(actor);
              else if (actor.IsPlayer) {
                // check music.
                m_MusicManager.PlayLooping(GameMusics.SLEEP, 1 == World.Get.PlayerCount ? MusicPriority.PRIORITY_EVENT : MusicPriority.PRIORITY_BGM);
                // message.
                RedrawPlayScreen(new UI.Message("...zzZZZzzZ...", map.LocalTime.TurnCounter, Color.DarkCyan));
                Thread.Sleep(10);
              } else if (rules.RollChance(MESSAGE_NPC_SLEEP_SNORE_CHANCE)) {
                var witnesses = actor.PlayersInLOS();
                if (null != witnesses) {
                  RedrawPlayScreen(witnesses, MakePanopticMessage(actor, string.Format("{0}.", VERB_SNORE.Conjugate(actor))));
                }
              }
#endregion
            }
            if (actor.IsExhausted && rules.RollChance(Rules.SLEEP_EXHAUSTION_COLLAPSE_CHANCE)) {
#region 4.3 Exhausted actors might collapse.
              actor.StartSleeping();
              var witnesses = actor.PlayersInLOS();
              if (null != witnesses) {
                RedrawPlayScreen(witnesses, MakePanopticMessage(actor, string.Format("{0} from exhaustion !!", VERB_COLLAPSE.Conjugate(actor))));
              }
              if (actor == Player) {
                Player.Controller.UpdateSensors();
                SetCurrentMap(Player.Location);
                RedrawPlayScreen();
              }
#endregion
            }
#endregion
          }
          actor.SpendSanity(1);
          var a_leader = actor.LiveLeader;
          if (null != a_leader) {
#region leader trust & leader/follower bond.
            ModifyActorTrustInLeader(actor, a_leader.TrustIncrease, false);
            if (actor.HasBondWith(a_leader) && rules.RollChance(Rules.SANITY_RECOVER_BOND_CHANCE)) {
              actor.RegenSanity(actor.ScaleSanRegen(Rules.SANITY_RECOVER_BOND));
              actor.Leader.RegenSanity(a_leader.ScaleSanRegen(Rules.SANITY_RECOVER_BOND));
              if (ForceVisibleToPlayer(actor))
                AddMessage(MakeMessage(actor, string.Format("{0} reassured knowing {1} is with {2}.", VERB_FEEL.Conjugate(actor), a_leader.Name, actor.HimOrHer)));
              if (ForceVisibleToPlayer(a_leader))
                AddMessage(MakeMessage(a_leader, string.Format("{0} reassured knowing {1} is with {2}.", VERB_FEEL.Conjugate(a_leader), actor.Name, a_leader.HimOrHer)));
            }
#endregion
          }
        }
#region Kill (zombify) starved actors.
        if (actorList1 != null) {
          foreach (Actor actor in actorList1) {
            var witnesses = actor.PlayersInLOS();
            if (null != witnesses) {
              RedrawPlayScreen(witnesses, MakePanopticMessage(actor, string.Format("{0} !!", VERB_DIE_FROM_STARVATION.Conjugate(actor))));
            }
            KillActor(null, actor, "starvation");
            if (!actor.Model.Abilities.IsUndead && Session.Get.HasImmediateZombification && rules.RollChance(s_Options.StarvedZombificationChance)) {
              map.TryRemoveCorpseOf(actor);
              Zombify(null, actor, false);
              if (null != witnesses) {
                ImportantMessage(witnesses, MakePanopticMessage(actor, string.Format("{0} into a Zombie!", "turn".Conjugate(actor))), DELAY_LONG);
              }
            }
          }
        }
#endregion
#endregion
#region 5. Check batteries : lights, trackers.
        // lights and normal trackers
        static bool is_drained(BatteryPowered it) {
          return 0 < it.Batteries && 0 >= --it.Batteries;
        }
        void drain(Actor actor, Item it) {
          if (it is BatteryPowered batt && is_drained(batt)) {
            var witnesses = actor.PlayersInLOS();
            if (null != witnesses) {
              RedrawPlayScreen(witnesses, MakePanopticMessage(actor, string.Format(": {0} goes off.", it.TheName)));
            }
          };
        }
        map.DoForAllActors(a => {
          drain(a, a.GetEquippedItem(DollPart.LEFT_HAND));    // lights and normal trackers
          drain(a, a.GetEquippedItem(DollPart.HIP_HOLSTER));    // police radios
        });
#endregion

#region 6. Check explosives.
        bool hasExplosivesToExplode = false;
        void expire_exp(ItemPrimedExplosive exp) { if (exp.Expire()) hasExplosivesToExplode = true; }
        void expire_all_exp(Inventory inv) { inv.GetItemsByType<ItemPrimedExplosive>()?.ForEach(expire_exp); }
        map.DoForAllInventory(expire_all_exp);  // 6.1 Update fuses.

        if (hasExplosivesToExplode) {
#region 6.2 Explode.
          bool find_live_grenade(Inventory inv, Location loc) {
            var tmp = inv.GetItemsByType<ItemPrimedExplosive>(ItemPrimedExplosive.IsExpired);
            if (null != tmp) foreach (var exp in tmp) {
                inv.RemoveAllQuantity(exp);
                DoBlast(loc, exp.Model.BlastAttack);
                return true;
            }
            return false;
          }

          while(map.DoForOneInventory(find_live_grenade));
#endregion
        }
#endregion
#region 7. Check fires.
        // \todo implement per-map weather, then use it here
        if (World.Get.Weather.IsRain() && rules.RollChance(Rules.FIRE_RAIN_TEST_CHANCE)) {
          map.DoForAllMapObjects(obj => {
              if (obj.IsOnFire && rules.RollChance(Rules.FIRE_RAIN_PUT_OUT_CHANCE)) {
                  obj.Extinguish();
                  if (ForceVisibleToPlayer(obj))
                      AddMessage(new("The rain has put out a fire.", map.LocalTime.TurnCounter));
              }
          });
        }
#endregion
      } // skipped in lodetail turns.

      map.UpdateTimers();
#if DATAFLOW_TRACE
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "considering NPC upgrade, Map: "+map.Name);
#endif
      var time = map.AdvanceLocalTime();
      if (time.Key) { // night/day ended; upgrade NPC skills
        if (time.Value) {   // day
          HandleLivingNPCsUpgrade(map);
        } else {    // night
          if (s_Options.ZombifiedsUpgradeDays == GameOptions.ZupDays.OFF || !GameOptions.IsZupDay(s_Options.ZombifiedsUpgradeDays, map.LocalTime.Day)) return;
          HandleUndeadNPCsUpgrade(map);
        }
      }
    }

    // object orientation (Actor) and UI lockdown (RogueGame) heuristics are different locations
    private void ModifyActorTrustInLeader(Actor a, int mod, bool addMessage)
    {
      a.TrustInLeader += mod;
      if (a.TrustInLeader > Rules.TRUST_MAX) a.TrustInLeader = Rules.TRUST_MAX;
      else if (a.TrustInLeader < Rules.TRUST_MIN) a.TrustInLeader = Rules.TRUST_MIN;
      if (addMessage && a.Leader.Controller is PlayerController pc)
        pc.AddMessage(new(string.Format("({0} trust with {1})", mod, a.TheName), Session.Get.WorldTime.TurnCounter, Color.White));
    }

#nullable enable
    private void CheckFor_Fire_ZombieInvasion(Map map)
    {
      // lock down the ley-line invasion to city limits, for now.
      if (map.LocalTime.IsStrikeOfMidnight && World.Get.CHAR_CityLimits.Contains(map.District.WorldPosition)) {
        var uc = map.Actors.CountUndead();
        var max_un = s_Options.MaxUndeads;
        if (uc < max_un) {
          if (map == Player.Location.Map && !Player.IsSleeping && !Player.Model.Abilities.IsUndead) {
            RedrawPlayScreen(new UI.Message("It is Midnight! Zombies are invading!", Session.Get.WorldTime.TurnCounter, Color.Crimson));
          }
          var day = map.LocalTime.Day;
          int num2 = 1 + (int)(Math.Min(1f, (float)(day * s_Options.ZombieInvasionDailyIncrease + s_Options.DayZeroUndeadsPercent) / 100f) * max_un) - uc;
          while(0 < num2--) SpawnNewUndead(map, day);
        }
      }
    }

    private void CheckFor_Fire_SewersInvasion(Map? map)
    {
      if (Session.Get.HasZombiesInSewers && null != map) {
        var uc = map.Actors.CountUndead();
        var max_un = s_Options.MaxUndeads / 2;
        if (uc < max_un && Rules.Get.RollChance(SEWERS_INVASION_CHANCE)) {
          int num2 = 1 + (int)(Math.Min(1f, (float)(map.LocalTime.Day * s_Options.ZombieInvasionDailyIncrease + s_Options.DayZeroUndeadsPercent) / 100f) * max_un) - uc;
          while(0 < num2--) SpawnNewSewersUndead(map);
        }
      }
    }

    static private float RefugeesEventDistrictFactor(District d)
    {
        var world = World.Get;
        int x = d.WorldPosition.X;
        int y = d.WorldPosition.Y;
        if (x == world.CHAR_CityLimits.Top || y == world.CHAR_CityLimits.Left) return 2f;
        if (x == world.CHAR_CityLimits.Bottom-1 || y == world.CHAR_CityLimits.Right-1) return 2f;
        int num1 = world.Size - 1;
        num1 /= 2;
        return x != num1 || y != num1 ? 1f : 0.5f;
    }

    // Refugees are up for a rethinking anyway (i.e., how do they get there)
    // They currently use the same ley-line behavior as the undead invasion (indeed, all livings do)
    // Ultimately, we would want to model both helicopters (supply drops, Blackops, possibly National Guard),
    // gas/diesel motorcycles (Bikers), and gas/diesel car-like vehicles (Gangters, Survivors, possibly National Guard)
    // note that there are no gas/diesel stations in town, so there must be one reasonably close outside of
    // the CHAR company town city limits to enable the gas/diesel vehicles to even get here.

    // Subway arrivals were disabled for gameplay reasons. (It was just plain strange for refugees to arrive in a map
    // that was physically disconnected from the surface, from their point of view.)
    // Refugee re-implementation (requires encircling highway)
    // * These arrive "near the main roads", typically
    // * We shall assume arrival on foot, for now.  Anyone arriving by vehicle either is police/military, or has an escape plan
    // ** motorcycle: might be able to carry 2 in a pinch
    // ** motorcycle w/side car
    // ** squad car: 2 police; can evacuate 2 others
    // ** Militarized Infantry Combat Vehicle: capacity 6-8; essential crew 2-3 (\todo: research).  Comes with 30mm-ish machine gun.
    // ** Armored Personnel Carrier: capacity 20-ish; essential crew 2-3 (\todo: research).
    // * keep the same #, but space them out in time ... say 7-17 (incoming @ s_RefugeePool)
    // * on-foot landing zones are 6 on each side of the highway (pre-computed @ s_RefugeeSpawnZones).

    static private bool CheckForEvent_ScheduleRefugees(Map map)
    {
      if (!World.Get.CHAR_CityLimits.Contains(map.District.WorldPosition)) return false;
#if DEBUG
      if (6 == map.LocalTime.Hour && WorldTime.TURNS_PER_HOUR - 1 == map.LocalTime.Tick && null == s_RefugeePool) throw new ArgumentNullException(nameof(s_RefugeePool));
#endif
      return 6 == map.LocalTime.Hour && WorldTime.TURNS_PER_HOUR-2 == map.LocalTime.Tick;
    }

    private readonly object lock_RefugeePool = new();
    private void FireEvent_ScheduleRefugees(District district)
    {
//    const int REFUGEE_SURFACE_SPAWN_CHANCE = 100;  // RS Alpha 80% is appropriate for a true megapolis (city-planet Trantor, for instance)
      const int UNIQUE_REFUGEE_CHECK_CHANCE = 10;
      const float REFUGEES_WAVE_SIZE = 0.2f;

      var rules = Rules.Get;
      var dr = rules.DiceRoller;

      lock(lock_RefugeePool) {
        Interlocked.CompareExchange(ref s_RefugeePool, new(), null); // ok to thrash GC here, this is once/game day
        int num1 = district.EntryMap.Actors.Count(a => a.IsFaction(GameFactions.IDs.TheCivilians) || a.IsFaction(GameFactions.IDs.ThePolice));
        int num2 = Math.Min(1 + (int)( (RefugeesEventDistrictFactor(district) * s_Options.MaxCivilians) * REFUGEES_WAVE_SIZE), s_Options.MaxCivilians - num1);
        while(0 < num2--) {
          var refugee = m_TownGenerator.CreateNewRefugee(district.EntryMap.LocalTime.TurnCounter, REFUGEES_WAVE_ITEMS);
          // Actively de-duplicate these names
          while(null != s_RefugeePool.FirstOrDefault(a  => a.Name == refugee.Name)) BaseMapGenerator.GiveNameToActor(dr, refugee);
          s_RefugeePool.Add(m_TownGenerator.CreateNewRefugee(district.EntryMap.LocalTime.TurnCounter, REFUGEES_WAVE_ITEMS));
        }

        if (!rules.RollChance(UNIQUE_REFUGEE_CHECK_CHANCE)) return;

        var uas = Session.Get.UniqueActors;
        lock (uas) {
          var candidates = uas.DraftPool(a => a.IsWithRefugees && !a.IsSpawned && !s_RefugeePool.Contains(a.TheActor) /* && !a.TheActor.IsDead */);
          if (0 < candidates.Count) s_RefugeePool.Add(rules.DiceRoller.Choose(candidates).TheActor);
        }
      }
    }

    static private bool RefugeeCanTakeLeadOf(Actor leader, Actor fo) {
      // inline Actor::CanTakeLeadOf here.  Most checks vacuously true for unspawned actors
      return leader.Faction == fo.Faction || !fo.Faction.LeadOnlyBySameFaction;
    }

    private List<Actor> GetRefugeeFollowers(Actor leader)
    {
        return s_RefugeePool!.Where(fo => RefugeeCanTakeLeadOf(leader, fo)).ToList();
    }

    private List<Actor> ConstructRefugeeParty()
    {
      var dr = Rules.Get.DiceRoller;
      List<Actor> ret = new();
      var leader = dr.ChooseWithoutReplacement(s_RefugeePool);
      ret.Add(leader);
      if (0 == s_RefugeePool!.Count) return ret;

      var n_followers = leader.MySkills.GetSkillLevel(Skills.IDs.LEADERSHIP);
      if (0 >= n_followers) return ret; // cannot lead.

      var recruits = GetRefugeeFollowers(leader);
      if (0 >= recruits.Count) return ret; // no-one to lead

      // \todo police may be finicky about who they want to lead

      int want = n_followers;
      do {
        var n = dr.Roll(0, recruits.Count);
        var recruit = recruits[n];
        var recruit_followers = recruit.MySkills.GetSkillLevel(Skills.IDs.LEADERSHIP);
        // may want plot armor for uniques
        if (n_followers < recruit_followers && RefugeeCanTakeLeadOf(recruit, leader)) {
          want += (recruit_followers - n_followers);
          leader = recruit;
          ret.Insert(0, leader);
        } else {
          ret.Add(recruit);
        }
        s_RefugeePool.Remove(recruit);
        recruits.RemoveAt(n);
      } while(0 < --want && 0<recruits.Count);
      return ret;
    }

    static private Compass.XCOMlike? entering_from(Rectangle r, Map m) {
        if (0 == r.Top) return Compass.XCOMlike.N;
        if (0 == r.Left) return Compass.XCOMlike.W;
        if (r.Right == m.Rect.Right) return Compass.XCOMlike.E;
        if (r.Bottom == m.Rect.Bottom) return Compass.XCOMlike.S;
        return null;
    }

    private void PlaceRefugeeParty(ZoneLoc dest) {
      var party = ConstructRefugeeParty();
      var leader = party[0];
      if (!SpawnIn(leader, dest, 1)) {
        s_RefugeePool!.AddRange(party);
        return;
      }

      // If the leader does not use advanced pathing (e.g., police), point him at the city
      if (null == leader.Threats) {
        var dir = entering_from(dest.Rect, dest.m);
        if (null != dir) {
          var escape = new Tasks.TaskEscapeNanny(leader, s_Arrival![(int)(dir.Value) /2].Value.Value);
          escape.Trigger(dest.m);
          dest.m.AddTimer(escape);
        }
      }

      party.RemoveAt(0);
      while(0 < party.Count) {
        var fo = party[0];
        if (!SpawnIn(fo, dest, 1)) {
          s_RefugeePool!.AddRange(party);
          return;
        }
        party.RemoveAt(0);
        leader.AddFollower(fo);
      }
      return;
    }

    // two options here (district must be on edge of reality bubble)
    // * if we have a highway, we can spawn up to two parties on the spawn zones near the highway
    // * if the district is compromised by Z, a pair of police can gate-crash things
    // we do an integrated guard-execute due to multi-threading.
    private void CheckFor_Fire_RefugeeParty(District district) {
      if (7 > district.EntryMap.LocalTime.Hour || 21 <= district.EntryMap.LocalTime.Hour) return;
      var world = World.Get;
      var code = world.EdgeCode(district);
      if (0 == code) return;    // can only walk in through outer district

      lock(lock_RefugeePool) {
        if (null == s_RefugeePool) return;

        // count: enemies of police, police
        // if have enemies of police, but no police, try to gate-crash on foot

        // otherwise, check for typical spawn zones
        Compass.XCOMlike dir;   // something illegal for incoming highway
        switch(code) {
        case 1:
          if (world.Extent.Anchor(Compass.XCOMlike.N) != district.WorldPosition) return;
          dir = Compass.XCOMlike.N;
          break;
        case 2:
          if (world.Extent.Anchor(Compass.XCOMlike.E) != district.WorldPosition) return;
          dir = Compass.XCOMlike.E;
          break;
        case 4:
          if (world.Extent.Anchor(Compass.XCOMlike.S) != district.WorldPosition) return;
          dir = Compass.XCOMlike.S;
          break;
        case 8:
          if (world.Extent.Anchor(Compass.XCOMlike.W) != district.WorldPosition) return;
          dir = Compass.XCOMlike.W;
          break;
        default: return;
        }

        var rules = Rules.Get;
        var numerator = s_RefugeePool.Count;
        var denominator = 8 * (district.EntryMap.LocalTime.TurnsTo(21) - 1) + 1;

        // theoretically could overflow at a high enough turns/day, but not for Angband-scale or coarser
        var dr = (numerator<denominator) ? rules.DiceRoller.Roll(0, denominator * denominator) : 0;
        if (dr >= 2 * numerator*denominator - numerator*numerator) return;

        if (dr <= numerator * denominator) {
          // select for left/top spawn zone
          PlaceRefugeeParty(s_RefugeeSpawnZones![(int)dir]);
          if (0 >= s_RefugeePool.Count) {
            Interlocked.Exchange(ref s_RefugeePool, null);
            return;
          }
          if (numerator*numerator < dr) return;
        }
        // select for right/bottom spawn zone
        PlaceRefugeeParty(s_RefugeeSpawnZones![(int)dir + 1]);
        if (0 >= s_RefugeePool.Count) Interlocked.Exchange(ref s_RefugeePool, null);
      }
    }

    // \todo re-wire the non-spawn part of this in
#if DEAD_FUNC
    private void FireEvent_UniqueActorArrive(Map map, UniqueActor unique)
    {
      if (!SpawnActorOnMapBorder(map, unique.TheActor, SPAWN_DISTANCE_TO_PLAYER)) return;
      unique.IsSpawned = true;
      if (map != Player.Location.Map || Player.IsSleeping || Player.Model.Abilities.IsUndead) return;
      PlayUniqueActorMusicAndMessage(unique, true);
      // XXX \todo district event
      Player.ActorScoring.AddEvent(Session.Get.WorldTime.TurnCounter, unique.TheActor.Name + " arrived.");
    }

        private void PlayUniqueActorMusicAndMessage(UniqueActor unique, bool hasArrived)    // alpha10: uses parameter to signal arrival (true) or first sighting (false).  \todo: implement first sighting
        {
            if (unique.EventMessage != null) {
                Overlay? highlightOverlay = null;

                if (unique.EventThemeMusic != null) {
                    m_MusicManager.Stop();
                    m_MusicManager.Play(unique.EventThemeMusic, MusicPriority.PRIORITY_EVENT);
                }

                ClearMessages();
                AddMessage(new Data.Message(unique.EventMessage, Session.Get.WorldTime.TurnCounter, Color.Pink));
                if (hasArrived) AddMessage((Player.Controller as PlayerController).MakeCentricMessage("Seems to come from", unique.TheActor.Location));
                else {
                    highlightOverlay = new OverlayRect(Color.Pink, new GDI_Rectangle(MapToScreen(unique.TheActor.Location.Position), SIZE_OF_TILE));
                    AddOverlay(highlightOverlay);
                }
                AddMessagePressEnter();
                ClearMessages();
                if (highlightOverlay != null) RemoveOverlay(highlightOverlay);
            }
        }
#endif

    // VAPORWARE: helicopter events: National Guard, supply drop, Black Ops
    // VAPORWARE: land vehicle events: Bikers (motorcycles), Gangsters (cars), Survivors (vans)
    // for now, allow the helicopter events to use legacy behavior
    // \todo but require the landbound events to arrive at the highway entrance zone
    private bool CheckForEvent_NationalGuard(Map map)
    {
      if (0 == s_Options.NatGuardFactor || map.LocalTime.IsNight || (map.LocalTime.Day < NATGUARD_DAY || map.LocalTime.Day >= NATGUARD_END_DAY) || !Rules.Get.RollChance(NATGUARD_INTERVENTION_CHANCE))
        return false;
      // yes, each national guardsman is double-counted
      int NationalGuardForceFactor()
      {
        int ret = 0;
        foreach(Actor a in map.Actors) {
          if (a.Model.Abilities.IsUndead) continue;
          ret += a.IsFaction(GameFactions.IDs.TheArmy) ? 2 : 1;
        }
        return ret;
      }

      return (double)(map.Actors.CountUndead() * s_Options.NatGuardFactor)/(double)(100* NationalGuardForceFactor()) >= NATGUARD_INTERVENTION_FACTOR;
    }

    private void FireEvent_NationalGuard(Map map)
    {
      var actor = SpawnNewNatGuardLeader(map);
      if (actor == null) return;

      for (int index = 0; index < NATGUARD_SQUAD_SIZE-1; ++index) {
        var other = SpawnNewNatGuardTrooper(actor);
        if (other != null) actor.AddFollower(other);
      }

      NotifyOrderablesAI(RaidType.NATGUARD, actor.Location);
    }

    static private int CountFoodItemsNutrition(Map map)
    {
      int nutrition(ItemFood food) { return food.NutritionAt(map.LocalTime.TurnCounter); }

      return map.SumOverAllInventory(inv => inv.GetItemsByType<ItemFood>()?.Sum(nutrition) ?? 0);
    }

    private bool CheckForEvent_ArmySupplies(Map map)
    {
      if (s_Options.SuppliesDropFactor == 0 || map.LocalTime.IsNight || map.LocalTime.Day < ARMY_SUPPLIES_DAY || !Rules.Get.RollChance(ARMY_SUPPLIES_CHANCE))
        return false;
      int num = 1 + map.Actors.Count(a => {
        if (!a.Model.Abilities.IsUndead && a.Model.Abilities.HasToEat)
          return a.IsFaction(GameFactions.IDs.TheCivilians);
        return false;
      });
      return (float)(1 + CountFoodItemsNutrition(map)) / (float)num < s_Options.SuppliesDropFactor / 100.0 * ARMY_SUPPLIES_FACTOR;
    }

    private const int army_supply_drop_checksum = 100;
    private static readonly KeyValuePair<Item_IDs, int>[] army_supply_drop_stock = {
        new(Item_IDs.FOOD_ARMY_RATION,80),
        new(Item_IDs.MEDICINE_MEDIKIT,20)
    };
    private static readonly Item_IDs[] army_supply_drop_stock_domain = army_supply_drop_stock.Select(x => x.Key).ToArray();

    private void FireEvent_ArmySupplies(Map map)
    {
      var dropPoint = FindDropSuppliesPoint(map);
      if (null == dropPoint) return;
      Rectangle survey = new Rectangle(dropPoint.Value - (Point)ARMY_SUPPLIES_SCATTER, (Point)(2 * ARMY_SUPPLIES_SCATTER + 1));
      map.TrimToBounds(ref survey);
      // finding the supply drop point does all of the legality testing -- the center must qualify, the edges need not
      survey.DoForEach(pt => {
          map.DropItemAt(GameItems.From(army_supply_drop_stock.UseRarityTable(Rules.Get.DiceRoller.Roll(0, army_supply_drop_checksum))).create(), in pt);
          Session.Get.Police.Investigate.Record(map, in pt);
          Location loc = new Location(map, pt);
          // inaccurate, but ensures proper prioritzation
          var already_known = Session.Get.Police.ItemMemory.WhatIsAt(loc) ?? new();
          already_known.UnionWith(army_supply_drop_stock_domain);
          Session.Get.Police.ItemMemory.Set(loc, already_known, map.LocalTime.TurnCounter);
        },pt => AirdropWithoutIncident(map, in pt));

      NotifyOrderablesAI(RaidType.ARMY_SUPLLIES, new Location(map, dropPoint.Value));
    }

    static private bool IsSuitableDropSuppliesPoint(Map map, in Point pt)  // XXX should be able to partially precalculate
    {
#if DEBUG
      if (!map.IsInBounds(pt)) throw new ArgumentOutOfRangeException(nameof(pt),pt, "!map.IsInBounds(pt)");
#endif
      return !map.IsInsideAt(pt) && map.GetTileModelAt(pt).IsWalkable && !map.HasActorAt(in pt) && !map.HasMapObjectAt(pt) && map.NoPlayersNearerThan(in pt,SPAWN_DISTANCE_TO_PLAYER);
    }

    static private bool AirdropWithoutIncident(Map map, in Point pt)  // XXX should be able to partially precalculate
    {
#if DEBUG
      if (!map.IsInBounds(pt)) throw new ArgumentOutOfRangeException(nameof(pt),pt, "!map.IsInBounds(pt)");
#endif
      if (   map.IsInsideAt(pt) || !map.GetTileModelAt(pt).IsWalkable        // VAPORWARE hit roof instead
          || map.HasActorAt(in pt))  // B-movies never hit anyone with an airdrop
        return false;
      var obj = map.GetMapObjectAt(pt);
      if (null == obj) return true;
      if (obj.IsOnFire) return false;   // incinerated?
      if (obj.IsWalkable || obj.IsJumpable) return true;    // RS Alpha rejected these.  They're not a harder landing than pavement.
      return false; // tree, or z-level issue (e.g., large fortifications)
    }

    private Point? FindDropSuppliesPoint(Map map)
    {
      var pts = map.Rect.Where(pt => IsSuitableDropSuppliesPoint(map, in pt));
      if (0 >= pts.Count) return null;
      return Rules.Get.DiceRoller.Choose(pts);
    }

#nullable enable
    static private bool HasRaidHappenedSince(RaidType raid, Map map, int sinceNTurns)
    {
      return map.HasRaidHappenedSince(raid, sinceNTurns);
    }

    private List<KeyValuePair<Map, KeyValuePair<ZoneLoc, HashSet<Point>>>> LandEventEntrance(Point world_pos)
    {
        int min_dist = int.MaxValue;
        List<KeyValuePair<Map, KeyValuePair<ZoneLoc, HashSet<Point>>>> ret = new();
        foreach(var x in s_Arrival!) {
          var dist = Rules.GridDistance(in world_pos, x.Key.District.WorldPosition);
          if (dist > min_dist) continue;
          if (dist < min_dist) {
            min_dist = dist;
            ret.Clear();
          }
          ret.Add(x);
        }
        return ret;
    }

    static private bool LZ_is_clear(ZoneLoc landing_zone, Faction friendly)
    {
        var scan = landing_zone.Listing;
        foreach(var loc in scan) if (null != loc.Actor) return false;
        short radius = (short)Actor.MaxLivingFOV(landing_zone.m);
        // cut some corners re FOV calculations
        var LoS_zone = new ZoneLoc(landing_zone.m, new Rectangle(landing_zone.Rect.Location-radius*Direction.NW, landing_zone.Rect.Size+2*Direction.SE));
        var scan2 = LoS_zone.Listing;
        foreach(var loc in scan2) {
          if (scan.Contains(loc)) continue;
          var a = loc.Actor;
          if (null != a && a.Faction.IsEnemyOf(friendly)) return false;
        }
        return true;
    }

    private Point nominate_edge(Map map, Actor toSpawn) {
      var tmp = new Zaimoni.Data.Stack<Point>(stackalloc Point[2 * (map.Rect.Width + map.Rect.Height - 2)]);
      map.Rect.WhereOnEdge(ref tmp, pt => {
         if (!map.IsWalkableFor(pt, toSpawn)) return false;
         return true;
      });
      return Rules.Get.DiceRoller.Choose(tmp);
    }

    private bool CheckForEvent_BikersRaid(Map map)
    {
        if (!World.Get.CHAR_CityLimits.Contains(map.District.WorldPosition)) return false;
      var day = map.LocalTime.Day;
      return day >= BIKERS_RAID_DAY && day < BIKERS_END_DAY
         && !HasRaidHappenedSince(RaidType.BIKERS, map, BIKERS_RAID_DAYS_GAP * WorldTime.TURNS_PER_DAY)
         &&  Rules.Get.RollChance(BIKERS_RAID_CHANCE_PER_TURN);
    }
#nullable restore

    private void FireEvent_BikersRaid(Map map)
    {
      var landing_zones = LandEventEntrance(map.District.WorldPosition);
      var n = landing_zones.Count;
      while(0 <= --n) {
        if (!LZ_is_clear(landing_zones[n].Value.Key, GameFactions.TheGangstas)) landing_zones.RemoveAt(n);
      }
      if (0 >= landing_zones.Count) return;
      n = Rules.Get.DiceRoller.Roll(0, landing_zones.Count); // choose a valid landing zone
      var turn = map.LocalTime.TurnCounter;
      var party = new Actor[BIKERS_RAID_SIZE];
      party[BIKERS_RAID_SIZE - 1] = SpawnNewBikerLeader(turn, Rules.Get.DiceRoller.Choose(GameGangs.BIKERS));

      var leader = party[BIKERS_RAID_SIZE - 1];
      var i = BIKERS_RAID_SIZE - 1;
      while(0 <= --i) {
        party[i] = SpawnNewBiker(leader);
        leader.AddFollower(party[i]);
      }

      // postprocess: leader to center
      party[BIKERS_RAID_SIZE - 1] = party[(BIKERS_RAID_SIZE - 1)/2];
      party[(BIKERS_RAID_SIZE - 1)/2] = leader;

      var lz = landing_zones[n].Value.Key.Listing;
      i = BIKERS_RAID_SIZE;
      while(0 <= --i) lz[i].Place(party[i]);

      var dir = entering_from(landing_zones[n].Value.Key.Rect, landing_zones[n].Key);
      if (null != dir) {
          // point the leader at the city (who won't have threat tracking),
          // then the destination the legacy implementation would have picked out.
          var final_dest = new HashSet<Point> { nominate_edge(map, party[0]) };

          var escape = new Tasks.TaskEscapeNanny(leader, final_dest);
          escape.Trigger(map);
          map.AddTimer(escape);
          escape = new Tasks.TaskEscapeNanny(leader, landing_zones[n].Value.Value);
          escape.Trigger(landing_zones[n].Key);
          landing_zones[n].Key.AddTimer(escape);
      }

      map.District.SetLastRaidTime(RaidType.BIKERS, map.LocalTime.TurnCounter);
      NotifyOrderablesAI(RaidType.BIKERS, leader.Location);
    }

#nullable enable
    private bool CheckForEvent_GangstasRaid(Map map)
    {
        if (!World.Get.CHAR_CityLimits.Contains(map.District.WorldPosition)) return false;
      var day = map.LocalTime.Day;
      return day >= GANGSTAS_RAID_DAY && day < GANGSTAS_END_DAY
         && !HasRaidHappenedSince(RaidType.GANGSTA, map, GANGSTAS_RAID_DAYS_GAP*WorldTime.TURNS_PER_DAY)
         &&  Rules.Get.RollChance(GANGSTAS_RAID_CHANCE_PER_TURN);
    }
#nullable restore

    private void FireEvent_GangstasRaid(Map map)
    {
      var landing_zones = LandEventEntrance(map.District.WorldPosition);
      var n = landing_zones.Count;
      while(0 <= --n) {
        if (!LZ_is_clear(landing_zones[n].Value.Key, GameFactions.TheGangstas)) landing_zones.RemoveAt(n);
      }
      if (0 >= landing_zones.Count) return;
      n = Rules.Get.DiceRoller.Roll(0, landing_zones.Count); // choose a valid landing zone
      var turn = map.LocalTime.TurnCounter;
      var party = new Actor[GANGSTAS_RAID_SIZE ];
      party[GANGSTAS_RAID_SIZE - 1] = SpawnNewGangstaLeader(turn, Rules.Get.DiceRoller.Choose(GameGangs.GANGSTAS));

      var leader = party[GANGSTAS_RAID_SIZE - 1];
      var i = GANGSTAS_RAID_SIZE - 1;
      while(0 <= --i) {
        party[i] = SpawnNewGangsta(leader);
        leader.AddFollower(party[i]);
      }

      // \todo postprocess: firearms skill should get priority re firearms

      // postprocess: leader to center
      party[GANGSTAS_RAID_SIZE - 1] = party[(GANGSTAS_RAID_SIZE - 1)/2];
      party[(GANGSTAS_RAID_SIZE - 1)/2] = leader;

      // \todo postprocess: firearms on edge

      var lz = landing_zones[n].Value.Key.Listing;
      i = GANGSTAS_RAID_SIZE;
      while(0 <= --i) lz[i].Place(party[i]);

      var dir = entering_from(landing_zones[n].Value.Key.Rect, landing_zones[n].Key);
      if (null != dir) {
          // point the leader at the city (who won't have threat tracking),
          // then the destination the legacy implementation would have picked out.
          var final_dest = new HashSet<Point> { nominate_edge(map, party[0]) };

          var escape = new Tasks.TaskEscapeNanny(leader, final_dest);
          escape.Trigger(map);
          map.AddTimer(escape);
          escape = new Tasks.TaskEscapeNanny(leader, landing_zones[n].Value.Value);
          escape.Trigger(landing_zones[n].Key);
          landing_zones[n].Key.AddTimer(escape);
      }

      map.District.SetLastRaidTime(RaidType.GANGSTA, map.LocalTime.TurnCounter);
      NotifyOrderablesAI(RaidType.GANGSTA, leader.Location);
    }

#nullable enable
    private bool CheckForEvent_BlackOpsRaid(Map map)
    {
      return map.LocalTime.Day >= BLACKOPS_RAID_DAY
         && !HasRaidHappenedSince(RaidType.BLACKOPS, map, BLACKOPS_RAID_DAY_GAP*WorldTime.TURNS_PER_DAY)
         &&  Rules.Get.RollChance(BLACKOPS_RAID_CHANCE_PER_TURN);
    }
#nullable restore

    private void FireEvent_BlackOpsRaid(Map map)
    {
      map.District.SetLastRaidTime(RaidType.BLACKOPS, map.LocalTime.TurnCounter);
      var actor = SpawnNewBlackOpsLeader(map);
      if (actor == null) return;
      for (int index = 0; index < BLACKOPS_RAID_SIZE-1; ++index) {
        var other = SpawnNewBlackOpsTrooper(actor);
        if (other != null) actor.AddFollower(other);    // do not sink this into the above function: black op refugees are not a logic paradox
      }
      NotifyOrderablesAI(RaidType.BLACKOPS, actor.Location);
    }

    // Post-apocalyptic survivors do *NOT* need gas stations to arrive, unlike bikers and gangsters.
    // Their vans are assumed to be retrofitted to use biodiesel or ethanol (and they may well be
    // carrying the means to synthesize either or both).
#nullable enable
    private bool CheckForEvent_BandOfSurvivors(Map map)
    {
        if (!World.Get.CHAR_CityLimits.Contains(map.District.WorldPosition)) return false;
      return map.LocalTime.Day >= SURVIVORS_BAND_DAY
         && !HasRaidHappenedSince(RaidType.SURVIVORS, map, SURVIVORS_BAND_DAY_GAP*WorldTime.TURNS_PER_DAY)
         &&  Rules.Get.RollChance(SURVIVORS_BAND_CHANCE_PER_TURN);
    }

    private void FireEvent_BandOfSurvivors(Map map)
    {
      var landing_zones = LandEventEntrance(map.District.WorldPosition);
      var n = landing_zones.Count;
      while(0 <= --n) {
        if (!LZ_is_clear(landing_zones[n].Value.Key, GameFactions.TheSurvivors)) landing_zones.RemoveAt(n);
      }
      if (0 >= landing_zones.Count) return;
      n = Rules.Get.DiceRoller.Roll(0, landing_zones.Count); // choose a valid landing zone
      var turn = map.LocalTime.TurnCounter;
      var party = new Actor[SURVIVORS_BAND_SIZE+1];
      var i = SURVIVORS_BAND_SIZE + 1;
      while(0 <= --i) party[i] = m_TownGenerator.CreateNewSurvivor(turn);

      // 2022-06-06: Survivors don't have a guaranteed leader(???).
      // \todo pre-lead like we do civilians

      var lz = landing_zones[n].Value.Key.Listing;
      i = SURVIVORS_BAND_SIZE + 1;
      while(0 <= --i) lz[i].Place(party[i]);

      var dir = entering_from(landing_zones[n].Value.Key.Rect, landing_zones[n].Key);
      if (null != dir) {
          // point the leaderless survivors at the city (they won't have threat tracking),
          // then the destination the legacy implementation would have picked out.
          var final_dest = new HashSet<Point> { nominate_edge(map, party[0]) };
          i = SURVIVORS_BAND_SIZE + 1;
          while(0 <= --i) {
            var escape = new Tasks.TaskEscapeNanny(party[i], final_dest);
            escape.Trigger(map);
            map.AddTimer(escape);
            escape = new Tasks.TaskEscapeNanny(party[i], landing_zones[n].Value.Value);
            escape.Trigger(landing_zones[n].Key);
            landing_zones[n].Key.AddTimer(escape);
          }
      }

      map.District.SetLastRaidTime(RaidType.SURVIVORS, map.LocalTime.TurnCounter);
      NotifyOrderablesAI(RaidType.SURVIVORS, party[2].Location);
    }

    static private int DistanceToPlayer(Map map, Point pos)
    {
	  var players = map.Players.Get;
	  if (0 >= players.Count) return int.MaxValue;  // 2020-01-27 optimizer is catching this
	  return players.Min(p=> Rules.GridDistance(p.Location.Position, pos));
    }

    private bool SpawnIn(Actor toSpawn, ZoneLoc dest, int minDistToPlayer) {
      var tmp = dest.Rect.Where(pt => {
        var loc = new Location(dest.m, pt);
        if (!Map.Canonical(ref loc)) return false;
        if (!loc.IsWalkableFor(toSpawn)) return false;
        if (!dest.m.NoPlayersNearerThan(in pt, minDistToPlayer)) return false;
        if (toSpawn.WouldBeAdjacentToEnemy(loc.Map, loc.Position)) return false;
        if (null != loc.Actor) return false;
        return true;
      });
      if (0 >= tmp.Count) return false;
      var loc = new Location(dest.m, Rules.Get.DiceRoller.Choose(tmp));
      if (!Map.Canonical(ref loc)) return false; // invariant failure
      loc.Place(toSpawn);
      OnActorEnterTile(toSpawn);
      return true;
    }

    private bool SpawnActorOnMapBorder(Map map, Actor actorToSpawn, int minDistToPlayer)
    {
      var tmp = new Zaimoni.Data.Stack<Point>(stackalloc Point[2 * (map.Rect.Width + map.Rect.Height - 2)]);
      map.Rect.WhereOnEdge(ref tmp, pt => {
         if (!map.IsWalkableFor(pt, actorToSpawn)) return false;
         if (!map.NoPlayersNearerThan(in pt, minDistToPlayer)) return false;
         if (actorToSpawn.WouldBeAdjacentToEnemy(map, pt)) return false;
         return true;
      });
      if (0 >= tmp.Count) return false;
      map.PlaceAt(actorToSpawn, Rules.Get.DiceRoller.Choose(tmp));
      OnActorEnterTile(actorToSpawn);
      return true;
    }

    static private Actor? SpawnActorNear(Location near, Actor actorToSpawn, int minDistToPlayer, int maxDistToPoint)
    {
      Map map = near.Map;
      Point nearPoint = near.Position;

      int num1 = 4 * (map.Width + map.Height);
      var range = new Rectangle((Point)1,(Point)maxDistToPoint);
      var rules = Rules.Get;
      while(0 <= --num1) {
        var p = nearPoint + rules.DiceRoller.Choose(range) - rules.DiceRoller.Choose(range);
        map.TrimToBounds(ref p);
        if (!map.IsInsideAt(p) && map.IsWalkableFor(p, actorToSpawn) && (DistanceToPlayer(map, p) >= minDistToPlayer && !actorToSpawn.WouldBeAdjacentToEnemy(map, p))) {
          map.PlaceAt(actorToSpawn, in p);
          return actorToSpawn;
        }
      }
      return null;
    }
#nullable restore

    private void SpawnNewUndead(Map map, int day)
    {
      Actor newUndead = m_TownGenerator.CreateNewUndead(map.LocalTime.TurnCounter);
      if (s_Options.AllowUndeadsEvolution && Session.Get.HasEvolution) {
        GameActors.IDs fromModelID = newUndead.Model.ID;
        if (fromModelID != GameActors.IDs.UNDEAD_ZOMBIE_LORD || ZOMBIE_LORD_EVOLUTION_MIN_DAY <= day) {
          var rules = Rules.Get;
          int chance = Math.Min(75, day * 2);
          if (rules.RollChance(chance)) {
            fromModelID = fromModelID.NextUndeadEvolution();
            if (rules.RollChance(chance)) fromModelID = fromModelID.NextUndeadEvolution();
            newUndead.Model = GameActors.From(fromModelID);
          }
        }
      }
//    SpawnActorOnMapBorder(map, newUndead, SPAWN_DISTANCE_TO_PLAYER, true);    // allows cheesy metagaming
      SpawnActorOnMapBorder(map, newUndead, 1);
    }

#nullable enable
    private void SpawnNewSewersUndead(Map map)
    {
      Actor newSewersUndead = m_TownGenerator.CreateNewSewersUndead(map.LocalTime.TurnCounter);
//    SpawnActorOnMapBorder(map, newSewersUndead, SPAWN_DISTANCE_TO_PLAYER, false); // allows cheesy metagaming
      SpawnActorOnMapBorder(map, newSewersUndead, 1);
    }

    // Balance issue is problematic here (cop spawning at distance 2 can severely damage most undead PCs)
    // eliminating PC spawn radius shielding to match undead

    // The ley line backstory doesn't work for livings.
    // The bands remain PC spawn radius shielded, for now.
    private Actor? SpawnNewSurvivor(Map map)
    {
      Actor newSurvivor = m_TownGenerator.CreateNewSurvivor(map.LocalTime.TurnCounter);
      return (SpawnActorOnMapBorder(map, newSurvivor, SPAWN_DISTANCE_TO_PLAYER) ? newSurvivor : null);
    }

    private Actor? SpawnNewSurvivor(Location near)
    {
      Actor newSurvivor = m_TownGenerator.CreateNewSurvivor(near.Map.LocalTime.TurnCounter);
      return SpawnActorNear(near, newSurvivor, SPAWN_DISTANCE_TO_PLAYER, 3);
    }

    private Actor? SpawnNewNatGuardLeader(Map map)
    {
      Actor armyNationalGuard = m_TownGenerator.CreateNewArmyNationalGuard(map.LocalTime.TurnCounter, "Sgt");   // \todo look up some example rank systems -- some would expect Cpl here)
      armyNationalGuard.StartingSkill(Skills.IDs.LEADERSHIP);
      if (map.LocalTime.Day > NATGUARD_ZTRACKER_DAY)
        armyNationalGuard.Inventory.AddAll(GameItems.ZTRACKER.create());
      return (SpawnActorOnMapBorder(map, armyNationalGuard, SPAWN_DISTANCE_TO_PLAYER) ? armyNationalGuard : null);
    }

    private Actor? SpawnNewNatGuardTrooper(Actor leader)
    {
      Actor armyNationalGuard = m_TownGenerator.CreateNewArmyNationalGuard("Pvt", leader);
      armyNationalGuard.Inventory.AddAll(Rules.Get.RollChance(50) ? GameItems.COMBAT_KNIFE.create()
                                                                  : m_TownGenerator.MakeItemGrenade());  // does not seem hyper-critical to use the town generator's RNG
      return SpawnActorNear(leader.Location, armyNationalGuard, SPAWN_DISTANCE_TO_PLAYER, 3);
    }

    private Actor SpawnNewBikerLeader(int turn, GameGangs.IDs gangId)
    {
      Actor newBikerMan = m_TownGenerator.CreateNewBikerMan(turn, gangId);
      newBikerMan.StartingSkill(Skills.IDs.LEADERSHIP);
      newBikerMan.StartingSkill(Skills.IDs.TOUGH,3);
      newBikerMan.StartingSkill(Skills.IDs.STRONG,3);
      return newBikerMan;
    }

    private Actor SpawnNewBiker(Actor leader)
    {
      Actor newBikerMan = m_TownGenerator.CreateNewBikerMan(leader);
      newBikerMan.StartingSkill(Skills.IDs.TOUGH);
      newBikerMan.StartingSkill(Skills.IDs.STRONG);
      return newBikerMan;
    }

    private Actor SpawnNewGangstaLeader(int turn, GameGangs.IDs gangId)
    {
      Actor newGangstaMan = m_TownGenerator.CreateNewGangstaMan(turn, gangId);
      newGangstaMan.StartingSkill(Skills.IDs.LEADERSHIP);
      newGangstaMan.StartingSkill(Skills.IDs.AGILE,3);
      newGangstaMan.StartingSkill(Skills.IDs.FIREARMS);
      return newGangstaMan;
    }

    private Actor SpawnNewGangsta(Actor leader)
    {
      Actor newGangstaMan = m_TownGenerator.CreateNewGangstaMan(leader);
      newGangstaMan.StartingSkill(Skills.IDs.AGILE);
      return newGangstaMan;
    }

    private Actor? SpawnNewBlackOpsLeader(Map map)
    {
      Actor newBlackOps = m_TownGenerator.CreateNewBlackOps(map.LocalTime.TurnCounter, "Officer");
      newBlackOps.StartingSkill(Skills.IDs.LEADERSHIP);
      newBlackOps.StartingSkill(Skills.IDs.AGILE,3);
      newBlackOps.StartingSkill(Skills.IDs.FIREARMS,3);
      newBlackOps.StartingSkill(Skills.IDs.TOUGH);
      return (SpawnActorOnMapBorder(map, newBlackOps, SPAWN_DISTANCE_TO_PLAYER) ? newBlackOps : null);
    }

    private Actor? SpawnNewBlackOpsTrooper(Actor leader)
    {
      Actor newBlackOps = m_TownGenerator.CreateNewBlackOps("Agent", leader);
      newBlackOps.StartingSkill(Skills.IDs.AGILE);
      newBlackOps.StartingSkill(Skills.IDs.FIREARMS);
      newBlackOps.StartingSkill(Skills.IDs.TOUGH);
      return SpawnActorNear(leader.Location, newBlackOps, SPAWN_DISTANCE_TO_PLAYER, 3);
    }

    public void StopTheWorld()
    {
      StopSimThread();  // alpha10 abort allowed when quitting
      m_IsGameRunning = false;
      m_MusicManager.Stop();
    }
#nullable restore

    // Advisor hint available overlay
    static private OverlayPopup? s_HintAvailableOverlay = null;  // alpha10
    static private string[] s_HintAvailableHeaderLines = null;

    // Don't want this in PlayerController; the required public accessors are unacceptable.
    static private List<KeyValuePair<PlayerController, KeyValuePair<int, PlayerCommand> > >? s_CountedCommands = null;

    // These don't alter game state
    static private readonly PlayerCommand[] s_ReadonlyCommands = new PlayerCommand[]{
      PlayerCommand.NONE,
      PlayerCommand.HELP_MODE,
      PlayerCommand.ADVISOR,
      PlayerCommand.OPTIONS_MODE,
      PlayerCommand.KEYBINDING_MODE,
      PlayerCommand.HINTS_SCREEN_MODE,
      PlayerCommand.SCREENSHOT,
      PlayerCommand.SAVE_GAME
    };

    private void HandlePlayerActor(PlayerController pc)
    {
#if DEBUG
      if (null == pc) throw new ArgumentNullException(nameof(pc));
#endif
      var player = pc.ControlledActor;
#if DEBUG
      if (player.IsSleeping) throw new InvalidOperationException("player.IsSleeping");
#endif
      pc.SparseReset();
      pc.UpdateSensors();
      pc.BeforeAction();
      setPlayer(m_Player);
      m_PlayerInTurn = player;
      SetCurrentMap(player.Location);  // multi-PC support

      GC.Collect(); // force garbage collection when things should be slow anyway
      GC.WaitForPendingFinalizers();
      play_timer.Stop();

      // debug inspection checkpoint -- use InfoPopup, etc. UI

      int hotkey_turns = 0;
      bool flag1 = true;
      do {
        if (Player!=player) SetCurrentMap(setPlayer(player).Location);  // multi-PC support
        m_UI.UI_SetCursor(null);
        // hint available?
        // alpha10 no hint if undead
        if (!Player.IsDead && !Player.Model.Abilities.IsUndead) {
          // alpha10 fix properly handle hint overlay
          int availableHint = -1;
          if (s_Options.IsAdvisorEnabled && (availableHint = GetAdvisorFirstAvailableHint()) != -1) {
            GetAdvisorHintText((AdvisorHint)availableHint, out string hintTitle, out var _);
            if (null == s_HintAvailableOverlay) {
              // constant due to above
              var overlayPos = MapToScreen(Player.Location.Position + Direction.NW + 2 * Direction.N);
              s_HintAvailableHeaderLines = new string[] {
                string.Format("HINT AVAILABLE PRESS <{0}>", s_KeyBindings.AsString(PlayerCommand.ADVISOR)),
                hintTitle
              };
              s_HintAvailableOverlay = new OverlayPopup(s_HintAvailableHeaderLines, Color.White, Color.White, Color.Black, overlayPos);
              AddOverlay(s_HintAvailableOverlay);
            } else {
              s_HintAvailableHeaderLines[1] = hintTitle;
              if (!HasOverlay(s_HintAvailableOverlay)) AddOverlay(s_HintAvailableOverlay);
            }
          } else if (s_HintAvailableOverlay != null && HasOverlay(s_HintAvailableOverlay)) {
            RemoveOverlay(s_HintAvailableOverlay);
          }
        }

#region Theme music
        foreach(var unique in Session.Get.UniqueActors.ToArray()) {
          if (null == unique.EventThemeMusic) continue;
          if ((unique.TheActor==player || null!=player.Sees(unique.TheActor)) && m_MusicManager.Music != unique.EventThemeMusic) m_MusicManager.Play(unique.EventThemeMusic, MusicPriority.PRIORITY_EVENT);
          else if (m_MusicManager.Music==unique.EventThemeMusic) m_MusicManager.Stop();
        }
#endregion

        if (2 <= hotkey_turns) m_Status2 = hotkey_turns.ToString();
        else m_Status2 = null;
        RedrawPlayScreen();

        ActorAction tmpAction = pc.AutoPilot();
        if (null != tmpAction) {
          play_timer.Start();
          tmpAction.Perform();
          // XXX following is duplicated code
          pc.UpdateSensors();
          SetCurrentMap(player.Location);
          pc.UpdatePrevLocation();
          Session.Get.LastTurnPlayerActed = Session.Get.WorldTime.TurnCounter;
          m_PlayerInTurn = null;
          return;
        }

        PlayerCommand? pc_command = null;
        int? index = null;
        if (null != s_CountedCommands) {
            var ub = s_CountedCommands.Count;
            while(0 <= --ub) {
              if (s_CountedCommands[ub].Key == pc) {
                if (null != pc.enemies_in_FOV) {
                  // enemies in sight: abort.  Don't use the more general "in combat" notion.
                  // \todo We actually should abort if we think we could be imminently attacked by an unseen enemy
                  s_CountedCommands.RemoveAt(ub);
                  break;
                }

                pc_command = s_CountedCommands[ub].Value.Value;
                var count = s_CountedCommands[ub].Value.Key;
                if (1 >= count) {
                  if (1 >= s_CountedCommands.Count) s_CountedCommands = null;
                  else s_CountedCommands.RemoveAt(ub);
                } else {
                  s_CountedCommands[ub] = new(pc,new(count-1, pc_command.Value));
                  index = ub;
                }
                break;
              }
            }
        }

        if (null != pc_command) {
          play_timer.Start();
          switch (pc_command.Value)
          {
          case PlayerCommand.MOVE_N:
          case PlayerCommand.MOVE_NE:
          case PlayerCommand.MOVE_E:
          case PlayerCommand.MOVE_SE:
          case PlayerCommand.MOVE_S:
          case PlayerCommand.MOVE_SW:
          case PlayerCommand.MOVE_W:
          case PlayerCommand.MOVE_NW:
            if (TryInsanity(pc) || !DoPlayerBump(player, Direction.COMPASS[(int)(pc_command.Value) - (int)(PlayerCommand.MOVE_N)])) {
              if (null != index) s_CountedCommands.RemoveAt(index.Value);
            };
            break;
          case PlayerCommand.WAIT_OR_SELF:
            if (TryInsanity(pc)) {
              if (null != index) s_CountedCommands.RemoveAt(index.Value);
            };
            DoWait(player);
            break;
          case PlayerCommand.USE_SPRAY:
            if (TryInsanity(pc) || !HandlePlayerUseSpray(player)) {
              if (null != index) s_CountedCommands.RemoveAt(index.Value);
            };
            break;
          }
          // XXX following is duplicated code
          pc.UpdateSensors();
          SetCurrentMap(player.Location);
          pc.UpdatePrevLocation();
          Session.Get.LastTurnPlayerActed = Session.Get.WorldTime.TurnCounter;
          m_PlayerInTurn = null;
          return;
        }

        var ui_event = m_UI.WaitKeyOrMouse();
        // backward compatibility
        var key = ui_event.Key.Key;
        var mouseButtons = ui_event.Key.Value;
        GDI_Point point = new(ui_event.Value.Key, ui_event.Value.Value);

        if (null != key) {
          PlayerCommand command = InputTranslator.KeyToCommand(key);
            switch (command) {  // start indentation failure
              case PlayerCommand.QUIT_GAME:
                if (HandleQuitGame()) {
                  m_PlayerInTurn = null;
                  StopTheWorld();
                  RedrawPlayScreen();
                  return;
                }
                break;
              case PlayerCommand.NONE: break;
              case PlayerCommand.HELP_MODE:
                HandleHelpMode();
                break;
              case PlayerCommand.ADVISOR:
                HandleAdvisor(pc);
                break;
              case PlayerCommand.OPTIONS_MODE:
                HandleOptions(true);
                ApplyOptions(true);
                break;
              case PlayerCommand.KEYBINDING_MODE:
                s_KeyBindings.HandleRedefineKeys();
                break;
              case PlayerCommand.HINTS_SCREEN_MODE:
                HandleHintsScreen();
                break;
              case PlayerCommand.SCREENSHOT:
                HandleScreenshot();
                break;
              case PlayerCommand.SAVE_GAME:
                DoSaveGame(GetUserSave());
                break;
              case PlayerCommand.LOAD_GAME:
                DoLoadGame(GetUserSave());
                player = Player;
                flag1 = false;
                m_HasLoadedGame = true;
                break;
              case PlayerCommand.ABANDON_GAME:
                if (HandleAbandonGame()) {
                  StopSimThread(); // alpha10 abort allowed when quitting
                  flag1 = false;
                  KillActor(null, Player, "suicide");
                  break;
                }
                break;
              case PlayerCommand.ABANDON_PC:
                if (HandleAbandonPC(pc)) {
                  StopSimThread();
                  flag1 = false;
                  break;
                }
                flag1 = false;
                break;
              case PlayerCommand.MOVE_N:
              case PlayerCommand.MOVE_NE:
              case PlayerCommand.MOVE_E:
              case PlayerCommand.MOVE_SE:
              case PlayerCommand.MOVE_S:
              case PlayerCommand.MOVE_SW:
              case PlayerCommand.MOVE_W:
              case PlayerCommand.MOVE_NW:
                if (TryInsanity(pc)) {
                  flag1 = false;
                  break;
                }
                if (DoPlayerBump(player, Direction.COMPASS[(int)(command) - (int)(PlayerCommand.MOVE_N)])) {
                  flag1 = false;
                  if (2 <= hotkey_turns) {
                    (s_CountedCommands ??= new()).Add(new(pc, new(hotkey_turns-1, command)));
                  }
                };
                break;
              case PlayerCommand.RUN_TOGGLE:
                if (TryInsanity(pc)) {
                  flag1 = false;
                  break;
                }
                HandlePlayerRunToggle(player);
                break;
              case PlayerCommand.WAIT_OR_SELF:
                if (TryInsanity(pc)) {
                  flag1 = false;
                  break;
                }
                flag1 = false;
                if (2 <= hotkey_turns) {
                  (s_CountedCommands ??= new()).Add(new(pc, new(hotkey_turns-1, PlayerCommand.WAIT_OR_SELF)));
                }
                DoWait(player);
                break;
              case PlayerCommand.BARRICADE_MODE:
                flag1 = !TryInsanity(pc) && !HandlePlayerBarricade(player);
                break;
              case PlayerCommand.BREAK_MODE:
                flag1 = !TryInsanity(pc) && !HandlePlayerBreak(player);
                break;
              case PlayerCommand.BUILD_LARGE_FORTIFICATION:
                flag1 = !TryInsanity(pc) && !HandlePlayerBuildFortification(player, true);
                break;
              case PlayerCommand.BUILD_SMALL_FORTIFICATION:
                flag1 = !TryInsanity(pc) && !HandlePlayerBuildFortification(player, false);
                break;
              case PlayerCommand.CLEAR_WAYPOINT:
                HandlePlayerClearWaypoint(player);
                break;
              case PlayerCommand.CLOSE_DOOR:
                flag1 = !TryInsanity(pc) && !HandlePlayerCloseDoor(player);
                break;
              case PlayerCommand.EAT_CORPSE:
                flag1 = !TryInsanity(pc) && !HandlePlayerEatCorpse(player, point);
                break;
              case PlayerCommand.FIRE_MODE:
                flag1 = !TryInsanity(pc) && !HandlePlayerFireMode(pc);
                break;
              case PlayerCommand.GIVE_ITEM:
                flag1 = !TryInsanity(pc) && !HandlePlayerGiveItem(player, point);
                break;
              case PlayerCommand.INITIATE_TRADE:
                flag1 = !TryInsanity(pc) && !HandlePlayerInitiateTrade(pc, point);
                break;
              case PlayerCommand.TRANSFER_ITEM:
                flag1 = !TryInsanity(pc) && !HandlePlayerTransferItem(pc);
                break;
              case PlayerCommand.AS_AI:
                HandleAiActor(player.AsAI);
                flag1 = false;
                if (2 <= hotkey_turns) {
                  (s_CountedCommands ??= new()).Add(new(pc, new(hotkey_turns-1, PlayerCommand.AS_AI)));
                }
                break;
              case PlayerCommand.LEAD_MODE:
                flag1 = !TryInsanity(pc) && !HandlePlayerTakeLead(player);
                break;
              case PlayerCommand.MARK_ENEMIES_MODE:
                if (TryInsanity(pc)) {
                  flag1 = false;
                  break;
                }
                HandlePlayerMarkEnemies(player);
                break;
              case PlayerCommand.ORDER_MODE:
                flag1 = !TryInsanity(pc) && !HandlePlayerOrderMode(pc);
                break;
              case PlayerCommand.ORDER_PC_MODE:
                flag1 = !TryInsanity(pc) && !HandlePlayerOrderPCMode(pc);
                if (flag1) play_timer.Reset(); // glitches otherwise
                break;
              case PlayerCommand.COUNTERMAND_PC:
                HandlePlayerCountermandPC(pc);
                break;
              case PlayerCommand.PULL_MODE: // alpha10
                flag1 = !TryInsanity(pc) && !HandlePlayerPull(player);
                break;
              case PlayerCommand.PUSH_MODE:
                flag1 = !TryInsanity(pc) && !HandlePlayerPush(player);
                break;
              case PlayerCommand.REVIVE_CORPSE:
                flag1 = !TryInsanity(pc) && !HandlePlayerReviveCorpse(player, point);
                break;
              case PlayerCommand.SET_WAYPOINT:
                HandlePlayerSetWaypoint(player);
                break;
              case PlayerCommand.SHOUT: 
                flag1 = !TryInsanity(pc) && !HandlePlayerShout(player, null);
                break;
              case PlayerCommand.SLEEP:
                flag1 = !TryInsanity(pc) && !HandlePlayerSleep(player);
                break;
              case PlayerCommand.SWITCH_PLACE:
                flag1 = !TryInsanity(pc) && !HandlePlayerSwitchPlace(player);
                break;
              case PlayerCommand.UNLOAD:
                flag1 = !TryInsanity(pc) && !HandlePlayerUnloadItem(pc, point);
                break;
              case PlayerCommand.USE_EXIT:
                flag1 = !TryInsanity(pc) && !DoLeaveMap(player, player.Location.Position);
                break;
              case PlayerCommand.USE_SPRAY:
                if (TryInsanity(pc)) {
                  flag1 = false;
                  break;
                }
                if (HandlePlayerUseSpray(player)) {
                  flag1 = false;
                  if (2 <= hotkey_turns) {
                    (s_CountedCommands ??= new()).Add(new(pc, new(hotkey_turns-1, PlayerCommand.USE_SPRAY)));
                  }
                };
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
              case PlayerCommand.FACTION_INFO:
                HandleFactionInfo();
                break;
              case PlayerCommand.DAIMON_MAP:    // cheat command
                HandleDaimonMap();
                break;
              case PlayerCommand.MESSAGE_LOG:
                HandleMessageLog();
                break;
              case PlayerCommand.ITEM_SLOT_0:
                if (0 == (key.Modifiers & (Keys.Alt | Keys.Control | Keys.Shift))) {
                  hotkey_turns = 1;
                  break;
                }
                flag1 = !TryInsanity(pc) && !DoPlayerItemSlot(player, 0, key);
                break;
              case PlayerCommand.ITEM_SLOT_1:
                if (0 == (key.Modifiers & (Keys.Alt | Keys.Control | Keys.Shift))) {
                  hotkey_turns = 2;
                  break;
                }
                flag1 = !TryInsanity(pc) && !DoPlayerItemSlot(player, 1, key);
                break;
              case PlayerCommand.ITEM_SLOT_2:
                if (0 == (key.Modifiers & (Keys.Alt | Keys.Control | Keys.Shift))) {
                  hotkey_turns = 3;
                  break;
                }
                flag1 = !TryInsanity(pc) && !DoPlayerItemSlot(player, 2, key);
                break;
              case PlayerCommand.ITEM_SLOT_3:
                if (0 == (key.Modifiers & (Keys.Alt | Keys.Control | Keys.Shift))) {
                  hotkey_turns = 4;
                  break;
                }
                flag1 = !TryInsanity(pc) && !DoPlayerItemSlot(player, 3, key);
                break;
              case PlayerCommand.ITEM_SLOT_4:
                if (0 == (key.Modifiers & (Keys.Alt | Keys.Control | Keys.Shift))) {
                  hotkey_turns = 5;
                  break;
                }
                flag1 = !TryInsanity(pc) && !DoPlayerItemSlot(player, 4, key);
                break;
              case PlayerCommand.ITEM_SLOT_5:
                if (0 == (key.Modifiers & (Keys.Alt | Keys.Control | Keys.Shift))) {
                  hotkey_turns = 6;
                  break;
                }
                flag1 = !TryInsanity(pc) && !DoPlayerItemSlot(player, 5, key);
                break;
              case PlayerCommand.ITEM_SLOT_6:
                if (0 == (key.Modifiers & (Keys.Alt | Keys.Control | Keys.Shift))) {
                  hotkey_turns = 7;
                  break;
                }
                flag1 = !TryInsanity(pc) && !DoPlayerItemSlot(player, 6, key);
                break;
              case PlayerCommand.ITEM_SLOT_7:
                if (0 == (key.Modifiers & (Keys.Alt | Keys.Control | Keys.Shift))) {
                  hotkey_turns = 8;
                  break;
                }
                flag1 = !TryInsanity(pc) && !DoPlayerItemSlot(player, 7, key);
                break;
              case PlayerCommand.ITEM_SLOT_8:
                if (0 == (key.Modifiers & (Keys.Alt | Keys.Control | Keys.Shift))) {
                  hotkey_turns = 9;
                  break;
                }
                flag1 = !TryInsanity(pc) && !DoPlayerItemSlot(player, 8, key);
                break;
              case PlayerCommand.ITEM_SLOT_9:
                if (0 == (key.Modifiers & (Keys.Alt | Keys.Control | Keys.Shift))) {
                  hotkey_turns = 0;
                  break;
                }
                flag1 = !TryInsanity(pc) && !DoPlayerItemSlot(player, 9, key);
                break;
              default: throw new ArgumentException("command unhandled");
            }  // end indentation failure
            if (0<pc.Recoil && PlayerCommand.FIRE_MODE != command && !s_ReadonlyCommands.Contains(command)) pc.ResetRecoil();
        } else if (!HandleMouseLook(point)) {
          if (HandleMouseInventory(point, mouseButtons, out bool hasDoneAction)) {
            if (!hasDoneAction) continue;
            pc.ResetRecoil();
            flag1 = false;
          }
          if (HandleMouseOverCorpses(point, mouseButtons, out hasDoneAction)) {
            if (!hasDoneAction) continue;
            pc.ResetRecoil();
            flag1 = false;
          }
          ClearOverlays();
        }
      }
      while (flag1);
      m_Status2 = null;
      pc.UpdateSensors();
      SetCurrentMap(player.Location);
      pc.UpdatePrevLocation();    // abandon PC results in null here
      Session.Get.LastTurnPlayerActed = Session.Get.WorldTime.TurnCounter;
      m_PlayerInTurn = null;
      play_timer.Restart();
    }

#nullable enable
    private bool TryInsanity(PlayerController pc)
    {
      var player = pc.ControlledActor;
      if (!player.IsInsane || !Rules.Get.RollChance(Rules.SANITY_INSANE_ACTION_CHANCE)) return false;
      var insaneAction = GenerateInsaneAction(player);
      if (null == insaneAction || !insaneAction.IsPerformable()) return false;
      pc.AddMessageForceRead(new("(your insanity takes over)", Player.Location.Map.LocalTime.TurnCounter, Color.Orange));
      insaneAction.Perform();
      return true;
    }

    private bool HandleQuitGame()
    {
      bool flag = YesNoPopup("REALLY QUIT GAME");
      AddMessage(new(flag ? "Bye!"
                          : "Good. Keep roguing!", Session.Get.WorldTime.TurnCounter, Color.Yellow));
      return flag;
    }

    private bool HandleAbandonGame()
    {
      bool flag = YesNoPopup("REALLY KILL YOURSELF");
      AddMessage(new(flag ? "You can't bear the horror anymore..."
                          : "Good. No reason to make the undeads life easier by removing yours!", Session.Get.WorldTime.TurnCounter, Color.Yellow));
      return flag;
    }

    // XXX \todo use different text than the suicide text above
    private bool HandleAbandonPC(PlayerController pc)
    {
      var player = pc.ControlledActor; // backward compatiblity
      bool confirm = YesNoPopup("REALLY ABANDON " + player.UnmodifiedName + " TO FATE");
      AddMessage(new(confirm ? "You can't bear the horror anymore..."
                             : "You retain your will to live.", Session.Get.WorldTime.TurnCounter, Color.Yellow));
      if (!confirm) return false;
      player.RevertToAI();
      Session.Get.Scoring.Deincarnate();
      player.DoForAllFollowers(fo => {
          if (fo.Controller is PlayerController fo_pc) {
              HandleAbandonPC(fo_pc);
              if (fo.IsPlayer) player.RemoveFollower(fo);  // NPCs cannot lead PCs; cf Actor::PrepareForPlayerControl
          }
      });
      return 0>= World.Get.PlayerCount;
    }

    private void HandleScreenshot()
    {
      int turn = Session.Get.WorldTime.TurnCounter;
      RedrawPlayScreen(new UI.Message("Taking screenshot...", turn, Color.Yellow));
      var screenshot = DoTakeScreenshot();
      RedrawPlayScreen(null == screenshot ? new UI.Message("Could not save screenshot.", turn, Color.Red)
                     : new UI.Message(string.Format("screenshot {0} saved.", screenshot), turn, Color.Yellow));
    }

    private string? DoTakeScreenshot()
    {
      string newScreenshotName = GetUserNewScreenshotName();
      return null != m_UI.UI_SaveScreenshot(ScreenshotFilePath(newScreenshotName)) ? newScreenshotName : null;
    }
#nullable restore

    private void HandleHelpMode()
    {
      if (m_Manual == null) {
        m_UI.ClearScreen();
        m_UI.UI_DrawStringBold(Color.Red, "Game manual not available ingame.", 0, 0, new Color?());
        m_UI.RenderFootnote("press ENTER");
        m_UI.WaitEnter();
      } else {
        List<string> formatedLines = m_Manual.FormatedLines;

        void display(int index) {
          m_UI.ClearScreen();
          DrawHeader();
          int gy1 = BOLD_LINE_SPACING;
          m_UI.UI_DrawStringBold(Color.Yellow, "Game Manual", 0, gy1);
          gy1 += BOLD_LINE_SPACING;
          m_UI.UI_DrawStringBold(Color.White, hr_plus, 0, gy1);
          gy1 += BOLD_LINE_SPACING;

          do {
            if ("<SECTION>" != formatedLines[index]) {
              m_UI.UI_DrawStringBold(Color.LightGray, formatedLines[index], 0, gy1);
              gy1 += BOLD_LINE_SPACING;
            }
            ++index;
          }
          while (index < formatedLines.Count && gy1 < CANVAS_HEIGHT - 2 * BOLD_LINE_SPACING);
          m_UI.UI_DrawStringBold(Color.White, hr_plus, 0, gy1);
          m_UI.RenderFootnote("cursor and PgUp/PgDn to move, numbers to jump to section, ESC to leave");
        };

        bool? handler(KeyEventArgs key) {
          int choiceNumber = KeyToChoiceNumber(key.KeyCode);
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
            switch (key.KeyCode) {
              case Keys.Prior:
                m_ManualLine -= TEXTFILE_LINES_PER_PAGE;
                break;
              case Keys.Next:
                m_ManualLine += TEXTFILE_LINES_PER_PAGE;
                break;
              case Keys.Up:
                --m_ManualLine;
                break;
              case Keys.Down:
                ++m_ManualLine;
                break;
            default: return null;
            }
          }
          if (m_ManualLine < 0) m_ManualLine = 0;
          else if (m_ManualLine + TEXTFILE_LINES_PER_PAGE >= formatedLines.Count) m_ManualLine = Math.Max(0, formatedLines.Count - TEXTFILE_LINES_PER_PAGE);
          display(m_ManualLine);
          return null;
        }

        display(m_ManualLine);
        m_UI.Modal(handler);
      }
    }

    private void HandleHintsScreen()
    {
      m_UI.ClearScreen();
      DrawHeader();
      int gy1 = BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.Yellow, "Advisor Hints", 0, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.White, "preparing...", 0, gy1, new Color?());
      m_UI.UI_Repaint();
      List<string> stringList = new();
      for (int index = 0; index < (int)AdvisorHint._COUNT; ++index) {
        GetAdvisorHintText((AdvisorHint) index, out string title, out string[] body);
        if (s_Hints.IsAdvisorHintGiven((AdvisorHint)index)) title += " (hint already given)"; // alpha10
        stringList.Add(string.Format("HINT {0} : {1}", index, title));
        stringList.AddRange(body);
        stringList.Add("~~~~");
        stringList.Add("");
      }

      void display(int index) {
        m_UI.ClearScreen();
        DrawHeader();
        int gy3 = BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.Yellow, "Advisor Hints", 0, gy3);
        gy3 += BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.White, hr_plus, 0, gy3);
        gy3 += BOLD_LINE_SPACING;

        do {
          m_UI.UI_DrawStringBold(Color.LightGray, stringList[index], 0, gy3);
          gy3 += BOLD_LINE_SPACING;
          ++index;
        }
        while (index < stringList.Count && gy3 < CANVAS_HEIGHT - 2 * BOLD_LINE_SPACING);
        m_UI.UI_DrawStringBold(Color.White, hr_plus, 0, gy3);
        m_UI.RenderFootnote("cursor and PgUp/PgDn to move, R to reset hints, ESC to leave");
      }

      int line_no = 0;
      bool? handler(KeyEventArgs key) {
        switch (key.KeyCode) {
          case Keys.Up:
            --line_no;
            break;
          case Keys.Down:
            ++line_no;
            break;
          case Keys.R:
            s_Hints.ResetAllHints();
            m_UI.ClearScreen();
            DrawHeader();
            m_UI.UI_DrawStringBold(Color.Yellow, "Advisor Hints", 0, BOLD_LINE_SPACING);
            m_UI.UI_DrawStringBold(Color.White, "Hints reset done.", 0, 2*BOLD_LINE_SPACING);
            m_UI.UI_Repaint();
            m_UI.UI_Wait(DELAY_LONG);
            break;
          case Keys.Prior:
            line_no -= TEXTFILE_LINES_PER_PAGE;
            break;
          case Keys.Next:
            line_no += TEXTFILE_LINES_PER_PAGE;
            break;
          default: return null;
        }
        if (line_no < 0) line_no = 0;
        else if (line_no + TEXTFILE_LINES_PER_PAGE >= stringList.Count) line_no = Math.Max(0, stringList.Count - TEXTFILE_LINES_PER_PAGE);
        display(line_no);
        return null;
      }

      display(line_no);
      m_UI.Modal(handler);
    }

    private void HandleMessageLog()
    {
      m_UI.ClearScreen();
      DrawHeader();
      int gy1 = BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.Yellow, "Message Log", 0, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.White, hr_plus, 0, gy1, new Color?());
      gy1 += BOLD_LINE_SPACING;
      Messages.DrawMessageLog(m_UI, 0, gy1);
      m_UI.RenderFootnote("press ESC to leave");
      m_UI.WaitEscape();
    }

    private void HandleCityInfo()
    {
      int gx = 0;
      int gy = 0;
      m_UI.ClearScreen();
      m_UI.UI_DrawStringBold(Color.White, "CITY INFORMATION -- "+Session.Seed.ToString()+" "+ Session.DescGameMode(Session.Get.GameMode), gx, gy, new Color?());
      gy = 2* BOLD_LINE_SPACING;
      if (Player.Model.Abilities.IsUndead) {
        m_UI.UI_DrawStringBold(Color.Red, "You can't remember where you are...", gx, gy, new Color?());
        gy += BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.Red, "Must be that rotting brain of yours...", gx, gy, new Color?());
        m_UI.RenderFootnote("press ESC to leave");
        m_UI.WaitEscape();
        return;
      }

      m_UI.UI_DrawStringBold(Color.White, "> DISTRICTS LAYOUT", gx, gy, new Color?());
      gy += BOLD_LINE_SPACING;
      int num2 = gy + BOLD_LINE_SPACING;
      for (int index = 0; index < World.Get.Size; ++index) {
        Color color = index == Player.Location.Map.DistrictPos.Y ? Color.LightGreen : Color.White;
        m_UI.UI_DrawStringBold(color, index.ToString(), 20, num2 + index * 3 * BOLD_LINE_SPACING + BOLD_LINE_SPACING, new Color?());
        m_UI.UI_DrawStringBold(color, ".", 20, num2 + index * 3 * BOLD_LINE_SPACING, new Color?());
        m_UI.UI_DrawStringBold(color, ".", 20, num2 + index * 3 * BOLD_LINE_SPACING + 2* BOLD_LINE_SPACING, new Color?());
      }
      for (int index = 0; index < World.Get.Size; ++index)
        m_UI.UI_DrawStringBold(index == Player.Location.Map.DistrictPos.X ? Color.LightGreen : Color.White, string.Format("..{0}..", (char)(65 + index)), 32 + index * 48, gy, new Color?());

      static ColorString DistrictToColorCode(DistrictKind d)
      {
        switch (d) {
          case DistrictKind.GENERAL: return new ColorString(Color.Gray, "Gen");
          case DistrictKind.RESIDENTIAL: return new ColorString(Color.Orange, "Res");
          case DistrictKind.SHOPPING: return new ColorString(Color.White, "Sho");
          case DistrictKind.GREEN: return new ColorString(Color.Green, "Gre");
          case DistrictKind.BUSINESS: return new ColorString(Color.Red, "Bus");
          case DistrictKind.INTERSTATE: return new ColorString(Color.White, "Hwy");
          default:
#if DEBUG
            throw new ArgumentOutOfRangeException("unhandled district kind");
#else
		    return new ColorString(Color.Blue, "BUG");
#endif
        }
	  }

      int num3 = gy + BOLD_LINE_SPACING;
      const int num4 = 32;
      int num5 = num3;
      for (int index1 = 0; index1 < World.Get.Size; ++index1) {
        for (int index2 = 0; index2 < World.Get.Size; ++index2) {
          District district = World.Get[index2, index1];
          char ch = district == CurrentMap.District ? '*' : (Player.ActorScoring.HasVisited(district.EntryMap) ? '-' : '?');
          var cs = DistrictToColorCode(district.Kind);
          string text = "".PadLeft(5,ch);
          m_UI.UI_DrawStringBold(cs.Key, text, num4 + index2 * 48, num5 + index1 * 3 * BOLD_LINE_SPACING, new Color?());
          m_UI.UI_DrawStringBold(cs.Key, string.Format("{0}{1}{2}", ch, cs.Value, ch), num4 + index2 * 48, num5 + (index1 * 3 + 1) * BOLD_LINE_SPACING, new Color?());
          m_UI.UI_DrawStringBold(cs.Key, text, num4 + index2 * 48, num5 + (index1 * 3 + 2) * BOLD_LINE_SPACING, new Color?());
        }
      }

        int num6 = World.Get.Size / 2;
        for (int index = 1; index < World.Get.Size; ++index)
          m_UI.UI_DrawStringBold(Color.White, "=", num4 + index * 48 - 8, num5 + num6 * 3 * BOLD_LINE_SPACING + BOLD_LINE_SPACING, new Color?());
        int gy3 = num3 + (World.Get.Size * 3 + 1) * BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.White, "Legend", gx, gy3, new Color?());
        gy3 += BOLD_LINE_SPACING;
        m_UI.UI_DrawString(Color.White, "  *   - current     ?   - unvisited", gx, gy3, new Color?());
        gy3 += LINE_SPACING;
        m_UI.UI_DrawString(Color.White, "  Bus - Business    Gen - General    Gre - Green", gx, gy3, new Color?());
        gy3 += LINE_SPACING;
        m_UI.UI_DrawString(Color.White, "  Res - Residential Sho - Shopping", gx, gy3, new Color?());
        gy3 += LINE_SPACING;
        m_UI.UI_DrawString(Color.White, "  =   - Subway Line", gx, gy3, new Color?());
        gy3 += LINE_SPACING + BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.White, "> NOTABLE LOCATIONS", gx, gy3, new Color?());
        int gy9 = gy3 + BOLD_LINE_SPACING;
        int num7 = gy9;
        for (int y = 0; y < World.Get.Size; ++y) {
          for (int x = 0; x < World.Get.Size; ++x) {
            Map entryMap = World.Get[x, y].EntryMap;
            Zone zoneByPartialName1;
            if ((zoneByPartialName1 = entryMap.GetZoneByPartialName("Subway Station")) != null) {
              m_UI.UI_DrawStringBold(Color.Blue, string.Format("at {0} : {1}.", World.CoordToString(x, y), zoneByPartialName1.Name), gx, gy9, new Color?());
              gy9 += BOLD_LINE_SPACING;
              if (gy9 >= CANVAS_HEIGHT - 2 * BOLD_LINE_SPACING) {
                gy9 = num7;
                gx += 350;
              }
            }
            Zone zoneByPartialName2;
            if ((zoneByPartialName2 = entryMap.GetZoneByPartialName("Sewers Maintenance")) != null) {
              m_UI.UI_DrawStringBold(Color.Green, string.Format("at {0} : {1}.", World.CoordToString(x, y), zoneByPartialName2.Name), gx, gy9, new Color?());
              gy9 += BOLD_LINE_SPACING;
              if (gy9 >= CANVAS_HEIGHT - 2 * BOLD_LINE_SPACING) {
                gy9 = num7;
                gx += 350;
              }
            }
            if (entryMap == Session.Get.UniqueMaps.PoliceStation_OfficesLevel.TheMap.District.EntryMap) {
              m_UI.UI_DrawStringBold(Color.CadetBlue, string.Format("at {0} : Police Station.", World.CoordToString(x, y)), gx, gy9, new Color?());
              gy9 += BOLD_LINE_SPACING;
              if (gy9 >= CANVAS_HEIGHT - 2 * BOLD_LINE_SPACING) {
                gy9 = num7;
                gx += 350;
              }
            }
            if (entryMap == Session.Get.UniqueMaps.Hospital_Admissions.TheMap.District.EntryMap) {
              m_UI.UI_DrawStringBold(Color.White, string.Format("at {0} : Hospital.", World.CoordToString(x, y)), gx, gy9, new Color?());
              gy9 += BOLD_LINE_SPACING;
              if (gy9 >= CANVAS_HEIGHT - 2 * BOLD_LINE_SPACING) {
                gy9 = num7;
                gx += 350;
              }
            }
            if (Session.Get.PlayerKnows_CHARUndergroundFacilityLocation && entryMap == Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap.District.EntryMap) {
              m_UI.UI_DrawStringBold(Color.Red, string.Format("at {0} : {1}.", World.CoordToString(x, y), Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap.Name), gx, gy9, new Color?());
              gy9 += BOLD_LINE_SPACING;
              if (gy9 >= CANVAS_HEIGHT - 2 * BOLD_LINE_SPACING) {
                gy9 = num7;
                gx += 350;
              }
            }
            var vip = Session.Get.UniqueActors.TheSewersThing.TheActor;
            if ((Player.Controller as PlayerController).KnowsWhere(vip) && entryMap == vip.Location.Map.District.EntryMap) {
              m_UI.UI_DrawStringBold(Color.Red, string.Format("at {0} : The Sewers Thing lives down there.", World.CoordToString(x, y)), gx, gy9, new Color?());
              gy9 += BOLD_LINE_SPACING;
              if (gy9 >= CANVAS_HEIGHT - 2 * BOLD_LINE_SPACING) {
                gy9 = num7;
                gx += 350;
              }
            }
          }
        }
      m_UI.RenderFootnote("press ESC to leave");
      m_UI.WaitEscape();
    }

#nullable enable
    private void ErrorPopup(string[] msg)
    {
      var staging_size = RogueForm.Get.Measure(msg);
#if DEBUG
      if (CANVAS_HEIGHT-4 < staging_size[^1].Height) throw new InvalidOperationException("test case: height " + staging_size[^1].Height.ToString());
      if (CANVAS_WIDTH-4  < staging_size[^1].Width) throw new InvalidOperationException("test case: width " + staging_size[^1].Width.ToString());
#endif
      var working = new OverlayPopup(msg, Color.Red, Color.Red, Color.Black, new GDI_Point((CANVAS_WIDTH - 4 - staging_size[^1].Width) / 2, (CANVAS_HEIGHT - 4 - staging_size[^1].Height) / 2));
      AddOverlay(working);
      RedrawPlayScreen();
      m_UI.WaitEscape();
      RemoveOverlay(working);
    }
    public void ErrorPopup(string msg) { ErrorPopup(new string[] { msg }); }

    private void InfoPopup(string[] msg)
    {
      var staging_size = RogueForm.Get.Measure(msg);
#if DEBUG
      if (CANVAS_HEIGHT-4 < staging_size[^1].Height) throw new InvalidOperationException("test case: height " + staging_size[^1].Height.ToString());
      if (CANVAS_WIDTH-4  < staging_size[^1].Width) throw new InvalidOperationException("test case: width " + staging_size[^1].Width.ToString());
#endif

      var working = new OverlayPopup(msg, Color.White, MODE_BORDERCOLOR, Color.Black, new GDI_Point((CANVAS_WIDTH - 4 - staging_size[^1].Width) / 2, (CANVAS_HEIGHT - 4 - staging_size[^1].Height) / 2));
      AddOverlay(working);
      RedrawPlayScreen();
      m_UI.WaitEscape();
      RemoveOverlay(working);
    }
    public void InfoPopup(string msg) { InfoPopup(new string[] { msg }); } // Ok: Waterfall i.e. SSADM lifecycle

    private bool YesNoPopup(string[] msg)
    {
      var staging_size = RogueForm.Get.Measure(msg);
#if DEBUG
      if (   CANVAS_HEIGHT-4 < staging_size[^1].Height
          || CANVAS_WIDTH-4  < staging_size[^1].Width)
        throw new InvalidOperationException("test case");
#endif

      var working = new OverlayPopup(msg, Color.White, MODE_BORDERCOLOR, Color.Black, new GDI_Point((CANVAS_WIDTH - 4 - staging_size[^1].Width) / 2, (CANVAS_HEIGHT - 4 - staging_size[^1].Height) / 2));
      AddOverlay(working);
      RedrawPlayScreen();
      var ret = m_UI.WaitYesOrNo();
      RemoveOverlay(working);
      return ret;
    }
    public bool YesNoPopup(string msg) { return YesNoPopup(new string[] { msg+"? (Y/N)" }); }

    public bool PagedPopup(string header,int strict_ub, Func<int,string> label, Predicate<int> details, bool centered=true)
    {
      var all_labels = new List<string>();
      int i = 0;
      while(i < strict_ub) all_labels.Add(label(i++));

      OverlayPopupTitle? working = null;
      var staging = new List<string>();
      var header_size = RogueForm.Get.Measure(header);
      header_size.Height += 1;

      int delta = EXTENDED_CHOICE_UB;
      int num1 = 0;
      int num2 = 0;

      void display() {
        staging.Clear();
        if (null == working) {
          for (num2 = 0; num2 < delta && num1 + num2 < strict_ub; ++num2) {
            int index = num1 + num2;
            staging.Add(ExtendedChoiceNumberToChar(num2) + " " + all_labels[index]);
          }
          if (delta < strict_ub) {
            staging.Add("prev <- -> next");
          }
          var staging_size = RogueForm.Get.Measure(staging);
          staging_size[^1].Height += header_size.Height;
          if (header_size.Width > staging_size[^1].Width) staging_size[^1].Width = header_size.Width;
#if DEBUG
          if (   CANVAS_HEIGHT-4 < staging_size[^1].Height 
              || CANVAS_WIDTH-4  < staging_size[^1].Width)
            throw new InvalidOperationException("test case");
#endif
          short working_y = (short)((CANVAS_HEIGHT - 4 - staging_size[^1].Height) / 2);
          if (centered) working = new OverlayPopupTitle(header, MODE_TEXTCOLOR, staging.ToArray(), Color.White, MODE_BORDERCOLOR, Color.Black, new Point((short)((CANVAS_WIDTH - 4 - staging_size[^1].Width) /2), working_y));
          else working = new OverlayPopupTitle(header, MODE_TEXTCOLOR, staging.ToArray(), Color.White, MODE_BORDERCOLOR, Color.Black, new Point((short)(CANVAS_WIDTH - 4 - staging_size[^1].Width), working_y));
          AddOverlay(working);
        }
        RedrawPlayScreen();
      }

      bool? handler(KeyEventArgs key) {
        if (delta < strict_ub) {
          if (Keys.Left == key.KeyCode) {
            if (0 < num1) num1 -= delta;
            else num1 = ((strict_ub - 1) / delta) * delta;
            RemoveOverlay(working);
            working = null;
          } else if (Keys.Right == key.KeyCode) {
            num1 += delta;
            if (num1 >= strict_ub) num1 = 0;
            RemoveOverlay(working);
            working = null;
          }
        }
        if (null != working) {
          int choiceNumber = KeyToExtendedChoiceNumber(key.KeyCode);
          if (choiceNumber >= 0 && choiceNumber < delta) {
            int index = num1 + choiceNumber;
            if (strict_ub > index && details(index)) return true;
          }
        } else display();
        return null;
      }

      display();
      var ret = m_UI.Modal(handler);
      RemoveOverlay(working);
      RedrawPlayScreen();
      return ret;
    }
#nullable restore

    private void HandleItemInfo()
    {
      var player_backup = Player;
      var item_classes = Player.Controller.WhatHaveISeen();
      if (null == item_classes || 0>=item_classes.Count) {
        ErrorPopup("You have seen no memorable items.");
        return;
      }
      item_classes.Sort();

      var tourism = Player.InterestingLocs;
      var sights_to_see = tourism?.All() ?? new();

      if (null != tourism) {
        Dictionary<Location, int> loc_distances = new();
        foreach(var loc in sights_to_see) loc_distances.Add(loc, Rules.GridDistance(Player.Location, loc));
        sights_to_see.Sort((x,y) => loc_distances[x].CompareTo(loc_distances[y]));
      }

      string label_tourism(int index) {
        var x = sights_to_see[index];
        return x.ToString();
      };

      bool details_tourism(int index) {
        var pc = Player.Controller as PlayerController;
          var x = sights_to_see[index];
          PanViewportTo(x);

          bool navigate(KeyEventArgs key) {
            switch (key.KeyCode)
            {
            case Keys.D1: // walk 1-9 steps to ....
            case Keys.D2:
            case Keys.D3:
            case Keys.D4:
            case Keys.D5:
            case Keys.D6:
            case Keys.D7:
            case Keys.D8:
            case Keys.D9:
              pc.WalkTo(x, (int)key.KeyCode - (int)Keys.D0);
              pc.AddWaypoint(x, "tourism");
              return true;
            }
            return false;
          }

          var tmp = new List<string>();
          tmp.Insert(0, "Walk 1) to 9) steps towards the item");
          return ShowSpecialDialogue(Player,tmp.ToArray(), navigate);
      }

      string label(int index) {
        if (index < item_classes.Count) return string.Format("{0}/{1} {2}.", index + 1, item_classes.Count, item_classes[index].ToString());
        return "Tourism";
      }

      bool details(int index) {
        if (item_classes.Count <= index) {
          return PagedPopup("Walk 1) to 9) steps to the item, recording a waypoint.", sights_to_see.Count, label_tourism, details_tourism, false);
        }

        var item_type = item_classes[index];
        var catalog = Player.Controller.WhereIs(item_type);
        var pc = Player.Controller as PlayerController;

        List<KeyValuePair<Location, int> > tmp_where = new();
        foreach(var loc_qty in catalog) {
          if (Player.CanEnter(loc_qty.Key)) tmp_where.Add(loc_qty);
          else {
            var obj = loc_qty.Key.MapObject as ShelfLike;
            if (null != obj) tmp_where.Add(loc_qty);
          }
        }

        Dictionary<Location, int> loc_distances = new();
        foreach(var loc in tmp_where) loc_distances[loc.Key] = Rules.GridDistance(Player.Location, loc.Key);
        tmp_where.Sort((x,y) => loc_distances[x.Key].CompareTo(loc_distances[y.Key]));

        // check for followers in-communication (cannot give orders to PC leader)
        var available_followers = pc.AvailableFollowers;

        string label_follower(int index) {
            return available_followers[index].Name;
        };

        string label2(int index) {
            var x = tmp_where[index];
            return x.Key.ToString() + ": " + x.Value.ToString();
        };

        bool details2(int index) {
          var x = tmp_where[index];
          PanViewportTo(x.Key);

          bool details_follower(int index) {
            var src = x.Key.Inv(item_type).Value;
            (available_followers[index].Controller as ObjectiveAI).Track(src, item_type);
            return true;
          };

          bool navigate(KeyEventArgs key) {
            switch (key.KeyCode)
            {
            case Keys.D1: // walk 1-9 steps to ....
            case Keys.D2:
            case Keys.D3:
            case Keys.D4:
            case Keys.D5:
            case Keys.D6:
            case Keys.D7:
            case Keys.D8:
            case Keys.D9:
              pc.WalkTo(x.Key, (int)key.KeyCode - (int)Keys.D0);
              pc.AddWaypoint(x.Key, item_type.ToString()+": T" + x.Value.ToString());
              return true;
            case Keys.O: 
              if (null != available_followers) {
                return PagedPopup("Who takes it?", available_followers.Count, label_follower, details_follower);
              }
              break;
            }
            return false;
          }

          var prompt = "Walk 1) to 9) steps towards the item";
          if (null != available_followers) prompt += ", or O)rder a follower to pick it up";
          var tmp = new List<string>();
          tmp.Insert(0, prompt);
          return ShowSpecialDialogue(Player,tmp.ToArray(), navigate);
        }

        return PagedPopup("Walk 1) to 9) steps to the item, recording a waypoint.", tmp_where.Count, label2, details2, false);
      }

      PagedPopup("Reviewing...", item_classes.Count+(null != tourism ? 1 : 0), label, details, false);
      PanViewportTo(player_backup);
      RedrawPlayScreen();
    }

    private void HandleAlliesInfo()
    {
      var player_allies = Player.Allies;
      if (null == player_allies) {
        InfoPopup("You have no nearby allies.");
        return;
      }
      var player_backup = Player;
      List<Actor> allies = player_allies.ToList();
      allies.Sort((a,b)=> string.Compare(a.Name,b.Name));

      string label_a(Actor a) {
        var leader = a.LiveLeader;
        return a.Name + (a.IsSleeping ? " (ZZZ)" : "") + (null == a.Followers ? "" : "[L]") + (null != leader ? "(leader " + leader.Name + ")" : "");
      }
      string label(int index) { return label_a(allies[index]); }
      bool details(int index) {
        Actor a = allies[index];
        if (a.IsSleeping) PanViewportTo(a.Location);
        else PanViewportTo(a);
        var tmp = new List<string>{a.Name};
        ItemMeleeWeapon best_melee = a.GetBestMeleeWeapon();
        tmp.Add("melee: "+(null == best_melee ? "unarmed" : best_melee.ModelID.ToString()));
        var ranged = a.Inventory.GetItemsByType<ItemRangedWeapon>();
        if (null != ranged) {
          string msg = "ranged:";
          foreach(ItemRangedWeapon rw in ranged) {
            msg += " "+rw.ModelID.ToString();
          }
          tmp.Add(msg);
        }
        var items = (a.Controller as ObjectiveAI).WhatHaveISeen();

        var critical = (a.Controller as ObjectiveAI).WhatDoINeedNow();
        if (null != items) critical.IntersectWith(items);
        else critical.Clear();
;       if (0<critical.Count) {
          string msg = "need now:";
          foreach(var x in critical) {
            if (60<msg.Length) {
              tmp.Add(msg);
              msg = "need now:";
            }
            msg += " "+x.ToString();
          }
          tmp.Add(msg);
        } else {
          critical = (a.Controller as ObjectiveAI).WhatDoIWantNow();
          if (null != items) critical.IntersectWith(items);
          else critical.Clear();
          if (0<critical.Count) {
            string msg = "want now:";
            foreach(var x in critical) {
              if (60<msg.Length) {
                tmp.Add(msg);
                msg = "want now:";
              }
              msg += " "+x.ToString();
            }
            tmp.Add(msg);
          }
        }

        ShowSpecialDialogue(Player,tmp.ToArray());
        return false;
      }

      PagedPopup("Reviewing...", allies.Count, label, details, false);
      PanViewportTo(player_backup);
    }

    private void HandleFactionInfo()
    {
      List<KeyValuePair<string, Action<List<string>>>> opts = new();
      string header = string.Empty;

      static void LeftPadColumn(List<string> dest, ref string header) {
            var len_ub = header.Length;
            var scan = dest.Count;
            while(0 <= --scan) {
                var len = dest[scan].Length;
                if (len_ub < len) len_ub = len;
            }
            scan = dest.Count;
            while(0 <= --scan) {
                var delta = len_ub - dest[scan].Length;
                if (0 < delta) dest[scan] = string.Concat(new string(' ', delta), dest[scan]);
            }
            var h_delta = len_ub - header.Length;
            if (0 < h_delta) {
                var h_delta_div_2 = h_delta/2;
                header = string.Concat(new string(' ', h_delta - h_delta_div_2), header, new string(' ', h_delta_div_2));
            }
      }

      static int AlignNames(List<KeyValuePair<string,int>> whom) {
            var align = whom[^1].Value;
            var len_ub = 0;
            var scan = whom.Count;
            while(0 <= --scan) {
                var delta = align - whom[scan].Value;
                if (0 < delta) whom[scan] = new(string.Concat(new string(' ', delta), whom[scan].Key), align);
                var len = whom[scan].Key.Length;
                if (len_ub < len) len_ub = len;
            }
            var whom_len_ub = len_ub;
            scan = whom.Count;
            while(0 <= --scan) {
                var delta = len_ub - whom[scan].Key.Length;
                if (0 < delta) whom[scan] = new(string.Concat(whom[scan].Key, new string(' ', delta)), align);
            }
            return whom_len_ub;
      }

    void police_status(List<string> display) {
         // full knowledge: police storyline
         if (0 <= Session.Get.ScriptStage_PoliceCHARrelations) {
                    // XXX should have NPC start-of-game district chief
                    display.Add("Aurora warning was called at noon, based on NASA forecast.  Confidence of breakers tripping on the magnetosphere generators");
                    display.Add(" between 21:00 and 3:00 is two in five.");
                    display.Add("The last contact the district chief had from CHAR was 19:12; some sort of 'containment failure', curfew requested.");
                    display.Add("The Metro Transit Authority confirmed that the subway has been shut down and the trains put in storage at 20:17.");
                    display.Add("Wasn't expecting all electric vehicles to be wrecked, when the breakers tripped on the magnetosphere generators at midnight sharp.");
         }
         if (1 <= Session.Get.ScriptStage_PoliceCHARrelations) {
                    // XXX should record first-aggressed cop
                    // Each CHAR office is to have one copy of the CHAR Operation Dead Hand document (the CHAR Guard Manual)
                    // XXX if the CHAR default orders document has been read then this text should be revised
                    display.Add("Something's very wrong; CHAR guards are attacking us cops.");
                    if (1 == Session.Get.ScriptStage_PoliceCHARrelations && 2 > Session.Get.ScriptStage_PoliceStationPrisoner) display.Add("That criminal CHAR forwarded to us, may not be.  We need a civilian to make " + Session.Get.UniqueActors.PoliceStationPrisoner.TheActor.HimOrHer + " squawk.");
                    // XXX ok for police to invade CHAR Offices at this point.
                    // XXX police will be able to aggress CHAR without risking murder at this point
         }
         if (2 <= Session.Get.ScriptStage_PoliceStationPrisoner) display.Add("That criminal CHAR forwarded to us, was a framed experimental subject; " + Session.Get.UniqueActors.PoliceStationPrisoner.TheActor.HeOrShe + " was contaminated by a ZM transformer agent.");
         if (2 <= Session.Get.ScriptStage_PoliceCHARrelations) {
                    // XXX should record sighting officer
                    // VAPORWARE the cell phone(?) used for last contact should be down here (plausibly not an artifact, however)
                    display.Add("We've found a CHAR research base they didn't tell us about.");
                    if (2 <= Session.Get.ScriptStage_PoliceStationPrisoner) display.Add("It's reasonable that whoever signed off on that ZM transformer agent was in this base.");
         }
#if PROTOTYPE
         if (3 <= Session.Get.ScriptStage_PoliceCHARrelations) {
                // XXX new map required: the lab where the ill-advised experimentation was done.
                // XXX be sure to include lab rat cages.  Unique hazards may be here.
                // XXX e.g. this area may be *contaminated* with a non-zero infection rate merely by existing here, in the infection modes
         }
         if (1 == Session.Get.ScriptStage_HospitalPowerup || 2 == Session.Get.ScriptStage_HospitalPowerup) {
                display.Add("The hospital needs its emergency power turned on.  We'll need to terminate the psychopathic murderer first.");
         }
#endif
      }

#if PROTOTYPE
      static void hospital_status(List<string> display) {
            if (....) {
              if (0 == Session.Get.ScriptStage_HospitalPowerup) {
                display.Add("We've sent a nurse down to turn on the emergency generators.");
              }
              if (1 == Session.Get.ScriptStage_HospitalPowerup) {
                display.Add("An escaped psyshopathic murderer is down near the emergency generators.  We'll need to evacuate.");
              }
              if (2 == Session.Get.ScriptStage_HospitalPowerup) {
                display.Add("We need someone to take out the psychopathic murderer.");
              }
              if (3 == Session.Get.ScriptStage_HospitalPowerup) {
                display.Add("Power has been restored.");
              }
            }
        display.Add("Placeholder");
      }
#endif

      static void placeholder_status(List<string> display)  => display.Add("Placeholder");

      if (Player.IsFaction(GameFactions.IDs.ThePolice)) {
        opts.Add(new("Status", police_status));
#if PROTOTYPE
      } else if (....) {
        opts.Add(new("Status", hospital_status));
#endif
      } else {
        opts.Add(new("Status", placeholder_status));
      }

      void enemies_by_aggression(List<string> display) {
        void name_him(Actor a) { display.Add(a.Name); }
        Player.Aggressing.DoForEach_(name_him, () => display.Add("Aggressed:"));
        Player.Aggressors.DoForEach_(name_him, () => display.Add("Defending from:"));
        if (SHOW_SPECIAL_DIALOGUE_LINE_LIMIT < display.Count) {
          display.RemoveRange(SHOW_SPECIAL_DIALOGUE_LINE_LIMIT, (display.Count-SHOW_SPECIAL_DIALOGUE_LINE_LIMIT)+1);
          display.Add("...");
        }
      }

      if (null == Player.Aggressing && null == Player.Aggressors) {
        header = "No personal enemies";
      } else {
        opts.Add(new("Enemies by aggression", enemies_by_aggression));
      }

      if (Player.IsFaction(GameFactions.IDs.ThePolice)) {
        var wanted = Session.Get.Police.Wanted();
        if (null != wanted) {
          void police_wanted(List<string> display) {
            void name_him(ActorTag a) { display.Add(a.Name); }
            wanted.DoForEach_(name_him, () => display.Add("Death-penalty criminals:"));
            if (SHOW_SPECIAL_DIALOGUE_LINE_LIMIT < display.Count) {
              display.RemoveRange(SHOW_SPECIAL_DIALOGUE_LINE_LIMIT-1, (display.Count-SHOW_SPECIAL_DIALOGUE_LINE_LIMIT)+1);
              display.Add("...");
            }
          }

          opts.Add(new("WANTED", police_wanted));
        }
        var scores = Session.Get.Police.Rankings();

        void police_rankings(List<string> display) {
            string header_score = "Score";
            string header_start = "Start";
            List<KeyValuePair<string,int>> whom = new();
            List<string> their_score = new();
            List<string> their_start = new();

            int me = -1;
            int scan = -1;
            ActorTag self = new(Player);

            scores.DoForEach(x => {
                ++scan;
                var place = string.Concat((scan + 1).ToString(), ") ");
                whom.Add(new(string.Concat(place, x.Key.Name), place.Length));
                their_score.Add(x.Value.TotalPoints.ToString());
                their_start.Add(x.Value.t0.ToString());
                if (x.Key == self) me = scan;
            });

            var raw_count = whom.Count;
            bool truncating = SHOW_SPECIAL_DIALOGUE_LINE_LIMIT - 1 < raw_count;
            if (truncating) {
              var trim = (whom.Count - SHOW_SPECIAL_DIALOGUE_LINE_LIMIT) + 2;
              whom.RemoveRange(SHOW_SPECIAL_DIALOGUE_LINE_LIMIT-2, trim);
              their_score.RemoveRange(SHOW_SPECIAL_DIALOGUE_LINE_LIMIT-2, trim);
              their_start.RemoveRange(SHOW_SPECIAL_DIALOGUE_LINE_LIMIT-2, trim);
            }

            var whom_len_ub = AlignNames(whom);
            LeftPadColumn(their_score, ref header_score);
            LeftPadColumn(their_start, ref header_start);

            string[] relay = new string[3];
            scan = -1;
            whom.DoForEach_(x => {
                ++scan;
                relay[0] = x.Key;
                relay[1] = their_score[scan];
                relay[2] = their_start[scan];
                display.Add(string.Join(" ", relay));
            }, () => {
                relay[0] = new string(' ', whom_len_ub);
                relay[1] = header_score;
                relay[2] = header_start;
                display.Add(string.Join(" ", relay));
            });
            if (truncating) display.Add("...");
        }
        opts.Add(new("Reputation", police_rankings));

        var deceased = Session.Get.Police.Deceased();
        if (0 < deceased.Count) {
            void police_graveyard(List<string> display) {
            string header_score = "Score";
            string header_start = "Start";
            string header_end = "End";
            string header_killer = "Killer";
            List<KeyValuePair<string,int>> whom = new();
            List<string> their_score = new();
            List<string> their_killer = new();
            List<string> their_start = new();
            List<string> their_end = new();

            int me = -1;
            int scan = -1;
            ActorTag self = new(Player);

            deceased.DoForEach(x => {
                ++scan;
                var place = string.Concat((scan + 1).ToString(), ") ");
                whom.Add(new(string.Concat(place, x.Key.Name), place.Length));
                their_score.Add(x.Value.Key.TotalPoints.ToString());
                their_start.Add(x.Value.Key.t0.ToString());
                their_killer.Add(x.Value.Value.killer.Name);
                their_end.Add(x.Value.Value.t1.ToString());
                if (x.Key == self) me = scan;
            });

            var raw_count = whom.Count;
            bool truncating = SHOW_SPECIAL_DIALOGUE_LINE_LIMIT - 1 < raw_count;
            if (truncating) {
              var trim = (whom.Count - SHOW_SPECIAL_DIALOGUE_LINE_LIMIT) + 2;
              whom.RemoveRange(SHOW_SPECIAL_DIALOGUE_LINE_LIMIT-2, trim);
              their_score.RemoveRange(SHOW_SPECIAL_DIALOGUE_LINE_LIMIT-2, trim);
              their_start.RemoveRange(SHOW_SPECIAL_DIALOGUE_LINE_LIMIT-2, trim);
            }

            var whom_len_ub = AlignNames(whom);
            LeftPadColumn(their_score, ref header_score);
            LeftPadColumn(their_start, ref header_start);
            LeftPadColumn(their_end, ref header_end);
            LeftPadColumn(their_killer, ref header_killer);

            string[] relay = new string[5];
            scan = -1;
            whom.DoForEach_(x => {
                ++scan;
                relay[0] = x.Key;
                relay[1] = their_killer[scan];
                relay[2] = their_score[scan];
                relay[3] = their_end[scan];
                relay[4] = their_start[scan];
                display.Add(string.Join(" ", relay));
            }, () => {
                relay[0] = new string(' ', whom_len_ub);
                relay[1] = header_killer;
                relay[2] = header_score;
                relay[3] = header_end;
                relay[4] = header_start;
                display.Add(string.Join(" ", relay));
            });
            if (truncating) display.Add("...");
            }
            opts.Add(new("Valhalla", police_graveyard));
        }
      }

      if (string.IsNullOrEmpty(header)) header = "Reviewing...";

      string label(int index) { return opts[index].Key; }
      bool details(int index) {
        List<string> display = new();
        opts[index].Value(display);
        ShowSpecialDialogue(Player,display.ToArray());
        return false;
      }

      PagedPopup(header, opts.Count, label, details);
    }

    private void HandleDaimonMap()
    {
      var sess = Session.Get;
      if (!sess.CMDoptionExists("socrates-daimon")) return;
      var turn = sess.WorldTime.TurnCounter;
      AddMessage(new("You pray for wisdom.", turn, Color.Green));
      World.Get.DaimonMap();
      AddMessage(new("Your prayers are unclearly answered.", turn, Color.Yellow));
    }

    /// not really; just need to block the other manual handlers from triggering
    private bool HandleMouseLook(GDI_Point mousePos) => null != MouseToLoc(mousePos.X, mousePos.Y);

    static private Location? m_LiveLook = null;
    private void HandleMouseLookAuto(int x, int y)
    {
      if (null == m_MapView) return;
      var examine = MouseToLoc(x, y);
      if (null == examine) return;
      var loc = examine.Value; // backward compatibility
      if (loc == m_LiveLook) return;
      if (IsVisibleToPlayer(in loc)) {
        List<Overlay> overlays = new();
        string[] lines = DescribeStuffAt(loc);
        if (lines != null) {
          var screen = MapToScreen(loc);
          overlays.Add(SetInfoOverlay(new OverlayPopup(lines, Color.White, Color.White, POPUP_FILLCOLOR, new GDI_Point(screen.X + TILE_SIZE, screen.Y))));
        }
        if (s_Options.ShowTargets) {
          var actorAt = loc.Actor;
          if (actorAt != null) DrawActorRelations(actorAt, overlays);
        }
        if (0 < overlays.Count) {
          m_LiveLook = loc;
          foreach(var o in overlays) AddOverlay(o);
          bool remove(int x, int y) {
            var test = MouseToLoc(x, y);
            if (null != test && null != m_LiveLook && test.Value == m_LiveLook.Value) return false;
            bool need_redraw = false;
            foreach(var o in overlays) {
              if (RemoveOverlay(o)) need_redraw = true;
            }
            m_LiveLook = null;
            if (need_redraw) RedrawPlayScreen();
            return true;
          }
          IRogueUI.UI.Add(remove);
          RedrawPlayScreen();
        }
      }
    }


    private bool _HandlePlayerInventory(Actor player, Item it)
    {
      if (it.IsEquipped) {
        if (player.CanUnequip(it, out string reason)) {
          it.UnequippedBy(Player);
          return false;
        }
        ErrorPopup(string.Format("Cannot unequip {0} : {1}.", it.TheName, reason));
        return false;
      }
      if (it.Model.IsEquipable) {
        if (player.CanEquip(it, out string reason)) {
          it.EquippedBy(player);
          return false;
        }
        ErrorPopup(string.Format("Cannot equip {0} : {1}.", it.TheName, reason));
        return false;
      }
      // Above strictly implies that an equippable item that also can be used, is not used by mouse click
      if (player.CanUse(it, out string reason1)) {
        DoUseItem(player, it);
        return true;
      }
      ErrorPopup(string.Format("Cannot use {0} : {1}.", it.TheName, reason1));
      return false;
    }

    private bool Interpret(_Action.TakeItem act)
    {
      if (!act.IsPerformable()) {
        ErrorPopup(string.Format("Cannot take {0} : {1}.", act.Take.TheName, act.FailReason));
        return false;
      }
      act.Perform();
      return true;
    }

    private bool HandleMouseInventory(GDI_Point mousePos, MouseButtons? mouseButtons, out bool hasDoneAction)
    {
      hasDoneAction = false;

      var invSpec = MouseToInventoryItem(mousePos);
      if (null == invSpec) return false; // this command can work against ground inventory

      bool isPlayerInventory = invSpec.Value.Key.a_owner == Player;

      bool OnRMBItem(Item it)
      {
        if (!isPlayerInventory) return false;
        if (Player.CanDrop(it, out string reason)) {
          DoDropItem(Player, it);
          return true;
        }
        ErrorPopup(string.Format("Cannot drop {0} : {1}.", it.TheName, reason));
        return false;
      }

      bool OnLMBItem(Item it)
      {
        if (isPlayerInventory) return _HandlePlayerInventory(Player, it);

        return Interpret(new _Action.TakeItem(Player, Player.Location, it));
      }

      ClearOverlays();
      var itemPos = invSpec.Value.Value; // backward compatibility
      AddOverlay(new OverlayRect(Color.Cyan, new GDI_Rectangle(itemPos.X, itemPos.Y, TILE_SIZE, TILE_SIZE)));
      AddOverlay(new OverlayRect(Color.Cyan, new GDI_Rectangle(itemPos.X + 1, itemPos.Y + 1, TILE_SIZE-2, TILE_SIZE-2)));
      var inventoryItem = invSpec.Value.Key.it!;
      AddOverlay(SetInfoOverlay(new OverlayPopup(DescribeItemLong(inventoryItem, isPlayerInventory), Color.White, Color.White, POPUP_FILLCOLOR, new GDI_Point(itemPos.X, itemPos.Y + TILE_SIZE))));
      if (mouseButtons.HasValue) {
          if (MouseButtons.Left == mouseButtons.Value) hasDoneAction = OnLMBItem(inventoryItem);
          else if (MouseButtons.Right == mouseButtons.Value) hasDoneAction = OnRMBItem(inventoryItem);
      }
      return true;
    }

#nullable enable
    private KeyValuePair<InventorySource<Item>, GDI_Point>? MouseToInventoryItem(GDI_Point screen)
    {
      var inventory = Player.Inventory;
      if (null == inventory) return null;
      var inventorySlot1 = MouseToInventorySlot(INVENTORYPANEL_X, INVENTORYPANEL_Y, screen);
      int index1 = inventorySlot1.X + inventorySlot1.Y * 10;
      if (index1 >= 0 && index1 < inventory.MaxCapacity) {
        if (null == inventory[index1]) return null;
        return new KeyValuePair<InventorySource<Item>, GDI_Point>(new(new InvOrigin(Player), inventory[index1]), InventorySlotToScreen(INVENTORYPANEL_X, INVENTORYPANEL_Y, inventorySlot1));
      }
      var invspec = Player.Location.InventoryAtFeet();
      if (null == invspec) return null;

      var itemsAt = invspec.Value.Inventory!;
      var inventorySlot2 = MouseToInventorySlot(INVENTORYPANEL_X, GROUNDINVENTORYPANEL_Y, screen);
      int index2 = inventorySlot2.X + inventorySlot2.Y * 10;
      if (index2 < 0 || index2 >= itemsAt.MaxCapacity) return null;
      if (null == itemsAt[index2]) return null;
      // hard crash if this inventory is flagged as seen by player
      return new KeyValuePair<InventorySource<Item>, GDI_Point>(new(invspec.Value, itemsAt[index2]), InventorySlotToScreen(INVENTORYPANEL_X, GROUNDINVENTORYPANEL_Y, inventorySlot2));
    }
#nullable restore

    private bool HandleMouseOverCorpses(GDI_Point mousePos, MouseButtons? mouseButtons, out bool hasDoneAction)
    {
      hasDoneAction = false;
      var corpse = MouseToCorpse(mousePos, out var corpsePos);
      if (null == corpse)  return false;

      bool OnRMBCorpse(Corpse c)
      {
        if (Player.Model.Abilities.IsUndead) {
          var act = new ActionEatCorpse(Player, c);
          if (act.IsPerformable()) {
            act.Perform();
            return true;
          }
          ErrorPopup(string.Format("Cannot eat {0} corpse : {1}.", c.DeadGuy.Name, act.FailReason));
          return false;
        }
        if (Player.CanButcher(c, out string reason1)) {
          DoButcherCorpse(Player, c);
          return true;
        }
       ErrorPopup(string.Format("Cannot butcher {0} corpse : {1}.", c.DeadGuy.Name, reason1));
       return false;
     }

     bool OnLMBCorpse(Corpse c)
     {
       switch(Player.CanStartStopDrag(c, out var reason))
       {
       case 2: // legal to stop drag
           Player.StopDraggingCorpse();
           return false;
       case 1: // legal to start drag
           Player.StartDragging(c);
           return false;
       default: // interpret as illegal to start drag, as that's what all of the surviving legacy reason strings are for
           ErrorPopup(string.Format("Cannot start dragging {0} corpse : {1}.", c.DeadGuy.Name, reason));
           return false;
       }
     }

      ClearOverlays();
      AddOverlay(new OverlayRect(Color.Cyan, new GDI_Rectangle(corpsePos.X, corpsePos.Y, TILE_SIZE, TILE_SIZE)));
      AddOverlay(new OverlayRect(Color.Cyan, new GDI_Rectangle(corpsePos.X + 1, corpsePos.Y + 1, TILE_SIZE-2, TILE_SIZE-2)));

      AddOverlay(new OverlayPopup(DescribeCorpseLong(corpse, true), Color.White, Color.White, POPUP_FILLCOLOR, new GDI_Point(corpsePos.X, corpsePos.Y + TILE_SIZE)));
      if (mouseButtons.HasValue) {
        if (MouseButtons.Left == mouseButtons.Value) hasDoneAction = OnLMBCorpse(corpse);
        else if (MouseButtons.Right == mouseButtons.Value) hasDoneAction = OnRMBCorpse(corpse);
      }
      return true;  // \todo test behavior when returning hasDoneAction
    }

#nullable enable
    private Corpse? MouseToCorpse(GDI_Point screen, out GDI_Point corpsePos)
    {
      corpsePos = GDI_Point.Empty;
      if (Player == null) return null;
      var corpsesAt = Player.Location.Corpses;
      if (null == corpsesAt) return null;
      var inventorySlot = MouseToInventorySlot(INVENTORYPANEL_X, CORPSESPANEL_Y, screen);
      corpsePos = InventorySlotToScreen(INVENTORYPANEL_X, CORPSESPANEL_Y, inventorySlot);
      int index = inventorySlot.X + inventorySlot.Y * 10;
      if (index >= 0 && index < corpsesAt.Count) return corpsesAt[index];
      return null;
    }

    private Corpse? MouseToCorpse(GDI_Point screen) { return MouseToCorpse(screen, out _); }

    private bool HandlePlayerEatCorpse(Actor player, GDI_Point mousePos)
    {
      var corpse = MouseToCorpse(mousePos);
      if (corpse == null) return false;
      var act = new ActionEatCorpse(player, corpse);
      if (act.IsPerformable()) {
        act.Perform();
        return true;
      }
      ErrorPopup(string.Format("Cannot eat {0} corpse : {1}.", corpse.DeadGuy.Name, act.FailReason));
      return false;
    }

    private bool HandlePlayerReviveCorpse(Actor player, GDI_Point mousePos)
    {
      var corpse = MouseToCorpse(mousePos);
      if (corpse == null) return false;
      if (!player.CanRevive(corpse, out string reason)) {
        ErrorPopup(string.Format("Cannot revive {0} : {1}.", corpse.DeadGuy.Name, reason));
        return false;
      }
      DoReviveCorpse(player, corpse);
      return true;
    }

#nullable restore

    public void DoButcherCorpse(Actor a, Corpse c)  // AI doesn't currently do this, but should be able to once it knows how to manage sanity
    {
      var witnesses = a.PlayersInLOS();
      a.SpendActionPoints();
      // XXX Unlike most sources of sanity loss, this is a living doing this.  Thus, this should affect reputation.
      SeeingCauseInsanity(a, Rules.SANITY_HIT_BUTCHERING_CORPSE, string.Format("{0} butchering {1}", a.Name, c.DeadGuy.Name));
      int num = a.DamageVsCorpses;
      if (null != witnesses) {
        RedrawPlayScreen(witnesses, MakePanopticMessage(a, string.Format("{0} {1} corpse for {2} damage.", VERB_BUTCHER.Conjugate(a), c.DeadGuy.Name, num)));
      }
      if (!c.TakeDamage(num)) return;
      a.Location.Map.Destroy(c);
      if (null != witnesses) {
        RedrawPlayScreen(witnesses, new(string.Format("{0} corpse is no more.", c.DeadGuy.Name), a.Location.Map.LocalTime.TurnCounter, Color.Purple));
      }
    }

    public void DoEatCorpse(Actor a, Corpse c)
    {
      var a_witness = a.PlayersInLOS();
      bool player = null != a_witness;
      if (player) PanViewportTo(a_witness);
      a.SpendActionPoints();
      int num = a.DamageVsCorpses;
      if (player) {
        AddMessage(a_witness, MakePanopticMessage(a, string.Format("{0} {1} corpse.", VERB_FEAST_ON.Conjugate(a), c.DeadGuy.Name)));
        // alpha10 replace with sfx
        m_MusicManager.Stop();
        m_MusicManager.Play(GameSounds.UNDEAD_EAT, MusicPriority.PRIORITY_EVENT);
      }
      if (c.TakeDamage(num)) {
        a.Location.Map.Destroy(c);
        if (player)
          AddMessage(a_witness, new(string.Format("{0} corpse is no more.", c.DeadGuy.Name), a.Location.Map.LocalTime.TurnCounter, Color.Purple));
      }
      if (a.Model.Abilities.IsUndead) {
        a.RegenHitPoints(a.BiteHpRegen(num));
        a.RottingEat(num);
      } else {
        a.LivingEat(a.BiteNutritionValue(num));
        a.Infect(c.TransmitInfection);
      }
      SeeingCauseInsanity(a, a.Model.Abilities.IsUndead ? Rules.SANITY_HIT_UNDEAD_EATING_CORPSE : Rules.SANITY_HIT_LIVING_EATING_CORPSE, string.Format("{0} eating {1}", a.Name, c.DeadGuy.Name));
    }

    public void DoReviveCorpse(Actor actor, Corpse corpse)
    {
      bool player = ForceVisibleToPlayer(actor);
      actor.SpendActionPoints();
      Map map = actor.Location.Map;
      var pointList = map.FilterAdjacentInMap(actor.Location.Position, pt => !map.HasActorAt(in pt) && !map.HasMapObjectAt(pt));
      var revive = corpse.DeadGuy;
      if (pointList == null) {
        if (player) AddMessage(MakeMessage(actor, string.Format("{0} not enough room for reviving {1}.", VERB_HAVE.Conjugate(actor), revive.Name)));
        return;
      }

      var inv = actor.Inventory;
      Item firstMatching = inv.GetFirstByModel(GameItems.MEDIKIT);
      inv.Consume(firstMatching);
      var rules = Rules.Get;
      if (rules.RollChance(actor.ReviveChance(corpse))) {
          revive.RevivedBy(actor);
          map.Remove(corpse);
          map.PlaceAt(revive, rules.DiceRoller.Choose(pointList));
          if (player) AddMessage(MakeMessage(actor, VERB_REVIVE.Conjugate(actor), revive));
          if (!actor.IsEnemyOf(revive)) revive.Say(actor, "Thank you, you saved my life!", default);
      } else {
          if (player) AddMessage(MakeMessage(actor, string.Format("{0} to revive", VERB_FAIL.Conjugate(actor)), revive));
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
      var it = player.Inventory[slot];
      if (it == null) {
        ErrorPopup(string.Format("No item at inventory slot {0}.", slot + 1));
        return false;
      }
      return _HandlePlayerInventory(player, it);
    }

    private bool DoPlayerItemSlotTake(Actor player, int slot)
    {
      var itemsAt = player.Location.Items;
      if (itemsAt == null) {
        ErrorPopup("No items on ground.");
        return false;
      }
      var it = itemsAt[slot];
      if (it == null) {
        ErrorPopup(string.Format("No item at ground slot {0}.", slot + 1));
        return false;
      }
      return Interpret(new _Action.TakeItem(player, player.Location, it));
    }

    private bool DoPlayerItemSlotDrop(Actor player, int slot)
    {
      var it = player.Inventory[slot];
      if (it == null) {
        ErrorPopup(string.Format("No item at inventory slot {0}.", slot + 1));
        return false;
      }
      if (player.CanDrop(it, out string reason)) {
        DoDropItem(player, it);
        return true;
      }
      ErrorPopup(string.Format("Cannot drop {0} : {1}.", it.TheName, reason));
      return false;
    }

#nullable enable
    private bool HandlePlayerShout(Actor player, string? text)
    {
      if (!player.CanShout(out string reason)) {
        ErrorPopup(string.Format("Can't shout : {0}.", reason));
        return false;
      }
      DoShout(player, text);
      return true;
    }

    private bool DirectionCommand<T>(Func<Direction,T> select, Predicate<T> execute)
    {
      do {
        ///////////////////
        // 1. Redraw
        // 2. Get input.
        // 3. Handle input
        ///////////////////
        RedrawPlayScreen();
        var dir = WaitDirectionOrCancel();
        if (null == dir) return false;
        T target = select(dir);
        if (execute(target)) return true;
      } while (true);
    }


    private bool DirectionCommandFiltered<T>(Func<Direction,T?> select, Predicate<T?> execute, string? no_options=null)
    {
      Dictionary<Direction,T>? options = null;
      var staging = select(Direction.NEUTRAL);
      if (null != staging) (options = new Dictionary<Direction, T>()).Add(Direction.NEUTRAL, staging);
      foreach(var dir in Direction.COMPASS) {
        staging = select(dir);
        if (null != staging) (options ??= new Dictionary<Direction, T>()).Add(dir, staging);
      }
      if (null == options) {
         if (!string.IsNullOrEmpty(no_options)) ErrorPopup(no_options);
         return false;
      }
      if (1 == options.Count) return execute(options.First().Value);

      do {
        ///////////////////
        // 1. Redraw
        // 2. Get input.
        // 3. Handle input
        ///////////////////
        RedrawPlayScreen();
        var dir = WaitDirectionOrCancel();
        if (null == dir) return false;
        var target = select(dir);
        if (execute(target)) return true;
      } while (true);
    }

    private bool HandlePlayerTransferItem(PlayerController pc) {
        const string MODE_TEXT = "TRANSFER ITEM - directions to select inventories, ESC cancels";
        const string SRC_TEXT = "Taking from:";
        const string DEST_TEXT = "Putting in:";
        ClearOverlays();
        AddOverlay(new OverlayPopup(MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));

        var player = pc.ControlledActor;
        Data.Model.InvOrigin src = default;
        Data.Model.InvOrigin dest = default;

        Data.Model.InvOrigin? transfer_from(Direction dir) {
          if (dir == Direction.NEUTRAL) {
            return player.Location.InventoryAtFeet();
          }
          var loc = player.Location + dir;
          if (!Map.Canonical(ref loc)) return null;
          var candidates = Map.AllItemsAt(loc);
          if (null == candidates) return null;
          return candidates[0];
        }

        bool record_transfer_from(Data.Model.InvOrigin? x) {
          if (null == x) return false;
          src = x.Value;
          return true;
        }

        var aux_offset = GDI_Point.Empty;
        aux_offset.Y += BOLD_LINE_SPACING+2;

        var aux = new OverlayPopup(SRC_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, aux_offset);
        AddOverlay(aux);

        if (!DirectionCommandFiltered(transfer_from, record_transfer_from, "Nothing in sight.")) {
          ClearOverlays();
          return false;
        }

        RemoveOverlay(aux);

        Data.Model.InvOrigin? transfer_to(Direction dir) {
          if (dir == Direction.NEUTRAL) {
            var ret = player.Location.DropOntoInventory();
            if (ret != src) return ret;
          }
          var loc = player.Location + dir;
          if (!Map.Canonical(ref loc)) return null;
          var candidates = Map.AllInventoriesAt(loc);
          if (null == candidates) return null;
          if (src != candidates[0]) return candidates[0];

          return null;
        }

        bool record_transfer_to(Data.Model.InvOrigin? x) {
          if (null == x) return false;
          dest = x.Value;
          return true;
        }

        aux = new OverlayPopup(DEST_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, aux_offset);
        AddOverlay(aux);

        if (!DirectionCommandFiltered(transfer_to, record_transfer_to, "Nothing in sight.")) {
          ClearOverlays();
          return false;
        }

        RemoveOverlay(aux);

        var it = Choose(src.Inventory, "Taking...");
        if (null == it) {
          ClearOverlays();
          return false;
        }

        player.SpendActionPoints();
        bool ret = src.Stance(player) && dest.Stance(player);
        src.Inventory!.Transfer(it, dest);
        ClearOverlays();
        RedrawPlayScreen();
        return ret;
    }

    private bool HandlePlayerUnloadItem(PlayerController pc, GDI_Point screen)
    {
      var invSpec = MouseToInventoryItem(screen);
      if (null == invSpec) return false; // this command can work against ground inventory

      if (!(invSpec.Value.Key.it is ItemRangedWeapon rw)) return false;

      var original_ammo = rw.Ammo;
      if (0 >= original_ammo) return false;

      var player = pc.ControlledActor;
      InventorySource<ItemRangedWeapon> src = new(invSpec.Value.Key.Origin, rw);
//    InventorySource<ItemRangedWeapon> src = (inv == player.Inventory) ? new(player, rw) : new(player.Location, player, rw);

      // try unloading to our own inventory first
      InvOrigin dest = new(player);
      if (DoUnload(player, src, dest)) return true;

      // If that fails, and we are unloading from a container, try the container's inventory
      if (null != src.obj_owner) {
        if (DoUnload(player, src, src.Origin)) return true;
      }

      // If that fails, try unload to ground inventory
      dest = new(player.Location,  player);
      return DoUnload(player, src, dest);
    }


    private bool HandlePlayerGiveItem(Actor player, GDI_Point screen)
    {
      const string GIVE_MODE_TEXT = "GIVE MODE - directions to give item to someone, ESC cancels";

      var invSpec = MouseToInventoryItem(screen);
      if (null == invSpec || invSpec.Value.Key.a_owner != player) return false;

      ClearOverlays();
      AddOverlay(new OverlayPopup(GIVE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));

      List<InvOrigin>? give_where_alt(Direction dir) { return Map.GetGivingInventoryOrigins(player, dir); }
      bool give_alt(List<InvOrigin>? src) {
        if (null == src) {
          ErrorPopup("Noone there.");
          return false;
        }

        var actorAt = src[0].a_owner; // backward compatibility
        if (actorAt != null) {
          if (player.CanGiveTo(actorAt, invSpec.Value.Key.it!, out string reason)) {
            DoGiveItemTo(player, actorAt, invSpec.Value.Key.it!);
            return true;
          }
          ErrorPopup(string.Format("Can't give {0} to {1} : {2}.", invSpec.Value.Key.it!.TheName, actorAt.TheName, reason));
          return false;
        }
        if (null != src[0].obj_owner) {
          var reason = src[0].obj_owner.ReasonCantPutItemIn(player);
          if (string.IsNullOrEmpty(reason)) {
            player.StandUp();
            DoPutItemInContainer(player, src[0].obj_owner, invSpec.Value.Key.it!);
            return true;
          }
          ErrorPopup(string.Format("Can't put {0} in {1} : {2}.", invSpec.Value.Key.it!.TheName, src[0].obj_owner.TheName, reason));
          return false;
        }
        if (null != src[0].loc) {
          DoDropItem(player, invSpec.Value.Key.it!, src[0].loc.Value);
          return true;
        }
        return false;
      }

      bool actionDone = DirectionCommand(give_where_alt, give_alt);

      ClearOverlays();
      return actionDone;
    }

    private void HandlePlayerRequestTrade(PlayerController pc)
    {
      var player = pc.ControlledActor;
      if (!player.Model.Abilities.CanTrade) {
        ErrorPopup("Incapable of trading.");
        return;
      }
      var can_trade_with = pc.GetTradingTargets(pc.friends_in_FOV); // this should only return legal trading targets

      // \todo filter these
      if (null == can_trade_with) {
        ErrorPopup("No visible non-enemy actors to trade with.");
        return;
      }

      var actorList = can_trade_with.Values.ToList();
      int index = 0;
      do {
        Actor target = actorList[index];
        ClearOverlays();
        AddOverlay(new OverlayPopup(INITIATE_TRADE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));
        AddOverlay(new OverlayImage(MapToScreen(target.Location), GameImages.ICON_TARGET));
        RedrawPlayScreen();
        KeyEventArgs key = m_UI.UI_WaitKey();
        if (key.KeyCode == Keys.Escape) break;
        else if (key.KeyCode == Keys.T) index = (index + 1) % actorList.Count;
        else if (key.KeyCode == Keys.E) {
          AddMessage(new(string.Format("Hey {0}, let's make a deal!", target.TheName), Session.Get.WorldTime.TurnCounter, PLAYER_ACTION_COLOR));
          var my_trading = new Gameplay.AI.Goals.Trade(player.Location.Map.LocalTime.TurnCounter, player, target);
          pc.SetObjective(my_trading);
          var o_oai = (target.Controller as ObjectiveAI)!;
          var your_trading = o_oai.Goal<Gameplay.AI.Goals.Trade>();
          if (null != your_trading) your_trading.Add(player);
          else {
            your_trading = new Gameplay.AI.Goals.Trade(player.Location.Map.LocalTime.TurnCounter, target, player);
            o_oai.SetObjective(your_trading);
          }
          break;
        }
      } while(true);
      ClearOverlays();
    }

    private bool HandlePlayerInitiateTrade(PlayerController pc, GDI_Point screen)
    {
      var player = pc.ControlledActor;
      var invSpec = MouseToInventoryItem(screen);
      if (null == invSpec || invSpec.Value.Key.a_owner != player) return false;

      ClearOverlays();
      AddOverlay(new OverlayPopup(INITIATE_TRADE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));

      List<InvOrigin>? trade_where_alt(Direction dir) { return Map.GetTradingInventoryOrigins(player, dir); }
      bool trade_alt(List<InvOrigin>? src) {
        if (null == src) {
          ErrorPopup("Noone there.");
          return false;
        }
        var actorAt = src[0].a_owner; // backward compatibility
        if (null != actorAt) {
          if (player.CanTradeWith(actorAt, out string reason)) {
            ClearOverlays();
            RedrawPlayScreen();
            if (DoTrade(pc, invSpec.Value.Key.it!, actorAt, true)) player.SpendActionPoints();
            return true;
          } else {
            ErrorPopup(string.Format("Can't trade with {0} : {1}.", actorAt.TheName, reason));
            return false;
          }
        }
        DoTrade(pc, invSpec.Value.Key.it!, src[0]);
        return true;
      }

      bool actionDone = DirectionCommand(trade_where_alt, trade_alt);

      ClearOverlays();
      return actionDone;
    }

    private void HandlePlayerRunToggle(Actor player)
    {
      if (!player.CanRun(out string reason) && !player.IsRunning) {
        ErrorPopup(string.Format("Cannot run now : {0}.", reason));
      } else {
        player.IsRunning = !player.IsRunning;
        AddMessage(MakeMessage(player, string.Format("{0} running.", (player.IsRunning ? VERB_START : VERB_STOP).Conjugate(player))));
      }
    }

    private bool HandlePlayerCloseDoor(Actor player)
    {
      const string CLOSE_DOOR_MODE_TEXT = "CLOSE MODE - directions to close, ESC cancels";

      ClearOverlays();
      AddOverlay(new OverlayPopup(CLOSE_DOOR_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));

      string err = "Nothing to close there.";

      DoorWindow? close_where(Direction dir) {
        err = "Nothing to close there.";
        if (dir == Direction.NEUTRAL) return null;
        var pos = player.Location.Position + dir;
        if (!player.Location.Map.IsInBounds(pos)) return null;  // doors never generate on map edges so IsInBounds ok

        var door = player.Location.Map.GetMapObjectAt(pos) as DoorWindow;
        if (null == door) return null;

        if (!player.CanClose(door, out string reason)) {
          err = string.Format("Can't close {0} : {1}.", door.TheName, reason);
          return null;
        }
        return door;
      }

      bool close(DoorWindow? door) {
        if (null != door) {
          DoCloseDoor(player, door, player.Location==(player.Controller as BaseAI).PrevLocation);
          return true;
        }
        ErrorPopup(err);
        return false;
      }

      bool actionDone = DirectionCommandFiltered(close_where, close, "Nothing to close here.");

      ClearOverlays();
      return actionDone;
    }

    private bool HandlePlayerBarricade(Actor player)
    {
      const string BARRICADE_MODE_TEXT = "BARRICADE/REPAIR MODE - directions to barricade/repair, ESC cancels";

      ClearOverlays();
      AddOverlay(new OverlayPopup(BARRICADE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));

      MapObject? build_where(Direction dir) {
        if (dir == Direction.NEUTRAL) return null;  // \todo? maybe not?  What about other-side-of-exit?
        var loc = player.Location + dir;
        if (!Map.Canonical(ref loc)) return null;
        return loc.MapObject;
      }

      bool build(MapObject? mapObjectAt) {
        if (null != mapObjectAt) {
          if (mapObjectAt is DoorWindow door) {
            if (player.CanBarricade(door, out string reason)) {
              DoBarricadeDoor(player, door);
              return true;
            } else {
              ErrorPopup(string.Format("Cannot barricade {0} : {1}.", door.TheName, reason));
              return false;
            }
          } else if (mapObjectAt is Fortification fort) {
            if (player.CanRepairFortification(fort, out string reason)) {
              DoRepairFortification(player, fort);
              return true;
            } else {
              ErrorPopup(string.Format("Cannot repair {0} : {1}.", fort.TheName, reason));
              return false;
            }
          } else {
            ErrorPopup(string.Format("{0} cannot be repaired or barricaded.", mapObjectAt.TheName));
            return false;
          }
        } else {
          ErrorPopup("Nothing to barricade there.");
          return false;
        }
      }

      bool actionDone = DirectionCommand(build_where, build);

      ClearOverlays();
      return actionDone;
    }

    private bool HandlePlayerBreak(Actor player)
    {
      const string BREAK_MODE_TEXT = "BREAK MODE - directions/wait to break an object, ESC cancels";

      ClearOverlays();
      AddOverlay(new OverlayPopup(BREAK_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));

      Point break_where(Direction dir) { return player.Location.Position + dir; }
      bool _break(Point pos) {
        string reason;
        MapObject? obj;
        if (pos == player.Location.Position) {
          var exitAt = player.Location.Exit;
          if (exitAt == null) {
            ErrorPopup("No exit there.");
            return false;
          }
          var actorAt = exitAt.Location.Actor;
          if (actorAt != null) {
            if (player.IsEnemyOf(actorAt)) {
              if (player.CanMeleeAttack(actorAt, out reason)) {
                DoMeleeAttack(player, actorAt);
                return true;
              } else {
                ErrorPopup(string.Format("Cannot attack {0} : {1}.", actorAt.Name, reason));
                return false;
              }
            } else {
              ErrorPopup(string.Format("{0} is not your enemy.", actorAt.Name));
              return false;
            }
          } else {
            obj = exitAt.Location.MapObject;
            if (null != obj) {
              if (player.CanBreak(obj, out reason)) {
                DoBreak(player, obj);
                return true;
              } else {
                ErrorPopup(string.Format("Cannot break {0} : {1}.", obj.TheName, reason));
                return false;
              }
            } else {
              ErrorPopup("Nothing to break or attack on the other side.");
              return false;
            }
          }
        }   // end NEUTRAL special case

        if (!player.Location.Map.IsValid(pos)) return false;

        obj = player.Location.Map.GetMapObjectAtExt(pos);
        if (null != obj) {
          if (player.CanBreak(obj, out reason)) {
            DoBreak(player, obj);
            RedrawPlayScreen();
            return true;
          } else {
            ErrorPopup(string.Format("Cannot break {0} : {1}.", obj.TheName, reason));
            return false;
          }
        } else {
          ErrorPopup("Nothing to break there.");
          return false;
        }
      }

      bool actionDone = DirectionCommand(break_where, _break);

      ClearOverlays();
      return actionDone;
    }

    private bool HandlePlayerBuildFortification(Actor player, bool isLarge)
    {
      const string BUILD_LARGE_FORT_MODE_TEXT = "BUILD LARGE FORTIFICATION MODE - directions to build, ESC cancels";
      const string BUILD_SMALL_FORT_MODE_TEXT = "BUILD SMALL FORTIFICATION MODE - directions to build, ESC cancels";

      if (0 == player.MySkills.GetSkillLevel(Skills.IDs.CARPENTRY)) {
        ErrorPopup("need carpentry skill.");
        return false;
      }
      int num = player.BarricadingMaterialNeedForFortification(isLarge);
      if (player.CountItems<ItemBarricadeMaterial>() < num) {
        ErrorPopup(string.Format("not enough barricading material, need {0}.", num));
        return false;
      }
      ClearOverlays();
      AddOverlay(new OverlayPopup(isLarge ? BUILD_LARGE_FORT_MODE_TEXT : BUILD_SMALL_FORT_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));

      Point? build_where(Direction dir) { return dir == Direction.NEUTRAL ? null : new Point?(player.Location.Position + dir); }
      bool build(Point? pos) {
        if (null == pos) return false;
        if (!player.Location.Map.IsValid(pos.Value)) return false;
        if (player.CanBuildFortification(pos.Value, isLarge, out string reason)) {
          DoBuildFortification(player, pos.Value, isLarge);
          return true;
        } else {
          ErrorPopup(string.Format("Cannot build here : {0}.", reason));
          return false;
        }
      }

      bool actionDone = DirectionCommand(build_where, build);

      ClearOverlays();
      return actionDone;
    }
#nullable restore

    private bool HandlePlayerFireMode(PlayerController pc)
    {
      const string FIRE_MODE_TEXT = "FIRE MODE - F to fire, T next target, M toggle mode, ESC cancels";

      var player = pc.ControlledActor;  // backward compability

      if (player.GetEquippedWeapon() is ItemGrenade || player.GetEquippedWeapon() is ItemGrenadePrimed)
        return HandlePlayerThrowGrenade(player);
      if (!(player.GetEquippedWeapon() is ItemRangedWeapon itemRangedWeapon)) {
        ErrorPopup("No weapon ready to fire.");
        return false;
      }
      if (itemRangedWeapon.Ammo <= 0) {
        ErrorPopup("No ammo left.");
        return false;
      }
      var enemiesInFov = player.GetEnemiesInFov(player.Controller.FOVloc);
      if (null == enemiesInFov || 0 >= enemiesInFov.Count) {
        ErrorPopup("No targets to fire at.");
        return false;
      }

      Attack attack = player.RangedAttack(0);
      int index = 0;

      var LoF = new List<Point>(attack.Range);
      FireMode mode = default;

      string desc(FireMode mode, Actor currentTarget) {
        switch(mode) {
          case FireMode.RAPID: return string.Format("RAPID fire average hit chances {0}% ({1}% {2}%)", player.ComputeChancesRangedHit(currentTarget, pc.Recoil+1), player.ComputeChancesRangedHit(currentTarget, 1), player.ComputeChancesRangedHit(currentTarget, 2));
          default: return string.Format("Normal fire average hit chance {0}%", player.ComputeChancesRangedHit(currentTarget, 0));
        }
      }

      // variables lifted/created for event handler setup
      Actor currentTarget = enemiesInFov[index];
      var overlayPopupText = new string[2];
      overlayPopupText[0] = FIRE_MODE_TEXT;
      overlayPopupText[1] = desc(mode, currentTarget);

      var modeDesc = new OverlayPopup(overlayPopupText, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty);
      var target = new OverlayImage(MapToScreen(currentTarget.Location), GameImages.ICON_TARGET);
      bool flag3 = player.CanFireAt(currentTarget, LoF, out string reason);

      List<OverlayImage> display_path(List<Point> los) {
        List<OverlayImage> ret = new();
        int num1 = Rules.InteractionDistance(player.Location, currentTarget.Location);
        string imageID = flag3 ? (num1 <= attack.EfficientRange ? GameImages.ICON_LINE_CLEAR : GameImages.ICON_LINE_BAD) : GameImages.ICON_LINE_BLOCKED;
        foreach (Point mapPosition in los)
          ret.Add(new OverlayImage(MapToScreen(mapPosition), imageID));
        return ret;
      }

      List<OverlayImage> LoS = display_path(LoF);

      ClearOverlays();

      // Initial redraw
      AddOverlay(modeDesc);
      AddOverlay(target);
      foreach(var o in LoS) AddOverlay(o);
      RedrawPlayScreen();

      bool? handler(KeyEventArgs key) {
        switch(key.KeyCode) {
          case Keys.T:
            if (1 < enemiesInFov.Count) {
              index = (index + 1) % enemiesInFov.Count;
              currentTarget = enemiesInFov[index];
              overlayPopupText[1] = desc(mode, currentTarget);
              RemoveOverlay(modeDesc);
              AddOverlay(modeDesc = new OverlayPopup(overlayPopupText, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));
              RemoveOverlay(target);
              AddOverlay(target = new OverlayImage(MapToScreen(currentTarget.Location), GameImages.ICON_TARGET));
              flag3 = player.CanFireAt(currentTarget, LoF, out reason);
              foreach(var o in LoS) RemoveOverlay(o);
              LoS = display_path(LoF);
              foreach(var o in LoS) AddOverlay(o);
              RedrawPlayScreen();
            }
            return null;
          case Keys.M:
            mode = (FireMode) ((int) (mode + 1) % 2);
            AddMessage(new(string.Format("Switched to {0} fire mode.", mode.ToString()), Session.Get.WorldTime.TurnCounter, Color.Yellow));
            overlayPopupText[1] = desc(mode, currentTarget);
            RemoveOverlay(modeDesc);
            AddOverlay(modeDesc = new OverlayPopup(overlayPopupText, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));
            RedrawPlayScreen();
            return null;
          case Keys.F:
            if (flag3) {
              (player.Controller as PlayerController).Exec_RangedAttack(currentTarget, LoF, mode);
              return true;
            } else
              ErrorPopup(string.Format("Can't fire at {0} : {1}.", currentTarget.TheName, reason));
           return null;
        }
        return null;
      }

      var ret = m_UI.Modal(handler);
      foreach(var o in LoS) RemoveOverlay(o);
      RemoveOverlay(target);
      RemoveOverlay(modeDesc);
      return ret;
    }

    public Actor? RecruitLOSchoose(List<Actor> actorList) {
      const string RECRUIT_MODE = "RECRUIT FOLLOWERS MODE - R to recruit, T next actor, ESC cancels";

      ClearOverlays();
      AddOverlay(new OverlayPopup(RECRUIT_MODE, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));

      int index = 0;
      Actor? ret = null;

      bool? handler(KeyEventArgs key) {
        switch (key.KeyCode) {
        case Keys.T:
          index = (index + 1) % actorList.Count;
          return true;
        case Keys.R:
          {
          ret = actorList[index];
          return true;
          }
        default: return null;
        }
      }

      OverlayImage? nominate;
      do {
        Actor target = actorList[index];
        nominate = new OverlayImage(MapToScreen(target.Location), GameImages.ICON_TARGET);
        AddOverlay(nominate);
        RedrawPlayScreen();
        if (!m_UI.Modal(handler)) break;
        RemoveOverlay(nominate);
      } while(null == ret);
      ClearOverlays();
      return ret;
    }

    public Actor? RecruitRadioChoose(List<Actor> actorList) {
      const string RECRUIT_MODE = "RECRUIT FOLLOWERS MODE - R to recruit, T next actor, ESC cancels";

      var player_backup = Player;
      ClearOverlays();
      AddOverlay(new OverlayPopup(RECRUIT_MODE, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));

      int index = 0;
      Actor? ret = null;

      bool? handler(KeyEventArgs key) {
        switch (key.KeyCode) {
        case Keys.T:
          index = (index + 1) % actorList.Count;
          return true;
        case Keys.R:
          {
          ret = actorList[index];
          return true;
          }
        default: return null;
        }
      }

      OverlayImage? nominate;
      do {
        Actor target = actorList[index];
        nominate = new OverlayImage(MapToScreen(target.Location), GameImages.ICON_TARGET);
        AddOverlay(nominate);
        PanViewportTo(target);
        if (!m_UI.Modal(handler)) break;
        RemoveOverlay(nominate);
      } while(null == ret);
      ClearOverlays();
      PanViewportTo(player_backup);
      return ret;
    }

    private void HandlePlayerMarkEnemies(Actor player)
    {
      const string MARK_ENEMIES_MODE = "MARK ENEMIES MODE - E to make enemy, T next actor, ESC cancels";

      if (player.Model.Abilities.IsUndead) {
        ErrorPopup("Undeads can't have personal enemies.");
        return;
      }
      var non_enemies = player.Controller.friends_in_FOV;
      if (null == non_enemies) {
        ErrorPopup("No visible non-enemy actors to mark.");
        return;
      }

      // 2024-10-07: simplify UI by not presenting invalid options
      var actorList = non_enemies.Values.ToList();
      int index = actorList.Count;
      while(0 <= --index) {
        Actor target = actorList[index];
        if (   target.Leader == player  // no followers as enemies
            || player.Leader == target) { // no leaders as enemies
            actorList.RemoveAt(index);
        }
      }
      if (0 >= actorList.Count) {
        ErrorPopup("No visible non-enemy actors to mark.");
        return;
      }

      ClearOverlays();
      AddOverlay(new OverlayPopup(MARK_ENEMIES_MODE, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));

      index = 0;

      bool? handler(KeyEventArgs key) {
        switch (key.KeyCode) {
        case Keys.T:
          index = (index + 1) % actorList.Count;
          return true;
        case Keys.E:
          {
          Actor target = actorList[index];
          AddMessage(new(string.Format("{0} is now a personal enemy.", target.TheName), Session.Get.WorldTime.TurnCounter, Color.Orange));
          DoMakeAggression(player, target);
          actorList.RemoveAt(index);
          if (actorList.Count <= index) index = actorList.Count - 1;
          while(0 <= index && player.IsEnemyOf(actorList[index])) {
            actorList.RemoveAt(index);
            if (actorList.Count <= index) index = actorList.Count - 1;
          }
          return true;
          }
        default: return null;
        }
      }

      OverlayImage? nominate;
      do {
        Actor target = actorList[index];
        nominate = new OverlayImage(MapToScreen(target.Location), GameImages.ICON_TARGET);
        AddOverlay(nominate);
        RedrawPlayScreen();
        if (!m_UI.Modal(handler)) break;
        RemoveOverlay(nominate);
      } while(0 < actorList.Count);
      ClearOverlays();
    }

    private bool HandlePlayerThrowGrenade(Actor player)
    {
      const string THROW_GRENADE_MODE_TEXT = "THROW GRENADE MODE - directions to select, F to fire,  ESC cancels";

      var itemGrenade = player.GetEquippedWeapon() as ItemGrenade;
      var itemGrenadePrimed = player.GetEquippedWeapon() as ItemGrenadePrimed;
#if DEBUG
      if (itemGrenade == null && itemGrenadePrimed == null) throw new InvalidOperationException("No grenade to throw.");  // precondition
#endif
      ActionThrowGrenade act = (null != itemGrenade) ? new ActionThrowGrenade(player, player.Location.Position, itemGrenade)
                                                     : new ActionThrowGrenade(player, player.Location.Position, itemGrenadePrimed);

      var mode = new OverlayPopup(THROW_GRENADE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty);
      List<OverlayImage> path = new();

      string? display() {
        bool can_throw = act.IsLegal();
        ClearOverlays();
        AddOverlay(mode);
        string imageID = can_throw ? GameImages.ICON_LINE_CLEAR : GameImages.ICON_LINE_BLOCKED;
        path.Clear();
        foreach (Point mapPosition in act.LoF) {
          var o = new OverlayImage(MapToScreen(mapPosition), imageID);
          path.Add(o);
          AddOverlay(o);
        }
        RedrawPlayScreen();
        return act.FailReason;
      }

      var err = display();

      bool? handler(KeyEventArgs key) {
        PlayerCommand command = InputTranslator.KeyToCommand(key);
        switch(key.KeyCode) {
        case Keys.F:
            if (string.IsNullOrEmpty(err)) {
              bool flag4 = true;
              if (act.ThrowerInBlast()) {
                flag4 = YesNoPopup(new string[] { "You are in the blast radius!", "Really throw there? (Y/N)" });
              }
              if (flag4) {
                act.Perform();
                return true;
              }
            } else {
              ErrorPopup(string.Format("Can't throw there : {0}.", err));
            }
            return null;
        default:
           {
           var dir = CommandToDirection(command);
           if (dir != null) {
              if (act.update(act.ThrowDest + dir)) {
                err = display();
              }
           }
           return null;
           }
        }
      }

      var flag2 = m_UI.Modal(handler);
      ClearOverlays();
      RedrawPlayScreen();
      return flag2;
    }

#nullable enable
    private bool HandlePlayerSleep(Actor player)
    {
      if (!player.CanSleep(out string reason)) {
        ErrorPopup(string.Format("Cannot sleep now : {0}.", reason));
        return false;
      }
      bool yes = YesNoPopup("Really sleep there");
      AddMessage(new(yes ? "Goodnight, happy nightmares!" : "Good, keep those eyes wide open.", Session.Get.WorldTime.TurnCounter, Color.Yellow));
      if (!yes) return false;
      player.StartSleeping();
      RedrawPlayScreen();
      // check music.
      m_MusicManager.PlayLooping(GameMusics.SLEEP, 1== World.Get.PlayerCount ? MusicPriority.PRIORITY_EVENT : MusicPriority.PRIORITY_BGM);
      return true;
    }

    private bool HandlePlayerSwitchPlace(Actor player)
    {
      const string SWITCH_PLACE_MODE_TEXT = "SWITCH PLACE MODE - directions to switch place with a follower, ESC cancels";

      ClearOverlays();
      AddOverlay(new OverlayPopup(SWITCH_PLACE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));

      Actor? swap_whom(Direction dir) {
        if (dir == Direction.NEUTRAL) return null;
        var loc = player.Location + dir;
        if (!Map.Canonical(ref loc)) return null;
        return loc.Actor;
      }

      bool swap(Actor? actorAt) {
        if (actorAt != null) {
          if (player.CanSwitchPlaceWith(actorAt, out string reason)) {
            DoSwitchPlace(player, actorAt);
            return true;
          } else {
            ErrorPopup(string.Format("Can't switch place : {0}", reason));
            return false;
          }
        } else {
          ErrorPopup("Noone there.");
          return false;
        }
      }

      bool actionDone = DirectionCommand(swap_whom, swap);

      ClearOverlays();
      return actionDone;
    }

    private bool HandlePlayerTakeLead(Actor player)
    {
      var test = (player.Controller as ObjectiveAI)?.RecruitLOS();
      if (null != test) {
        test.Perform();
        return true;
      }
      test = (player.Controller as ObjectiveAI)?.RecruitRadio();
      if (null != test) {
        test.Perform();
        return true;
      }
      const string TAKE_LEAD_MODE_TEXT = "TAKE LEAD MODE - directions to recruit a follower, ESC cancels";

      ClearOverlays();
      AddOverlay(new OverlayPopup(TAKE_LEAD_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));

      Actor? lead_whom(Direction dir) {
        if (dir == Direction.NEUTRAL) return null;
        var loc = player.Location + dir;
        if (!Map.Canonical(ref loc)) return null;
        return loc.Actor;
      }

      bool lead(Actor? actorAt) {
        if (actorAt != null) {
          if (player.CanTakeLeadOf(actorAt, out string reason)) {
            DoTakeLead(player, actorAt);
            int turn = Session.Get.WorldTime.TurnCounter;
            player.ActorScoring.AddEvent(turn, string.Format("Recruited {0}.", actorAt.TheName));
            actorAt.ActorScoring.AddEvent(turn, string.Format("Recruited by {0}.", player.TheName));
            AddMessage(new("(you can now set directives and orders for your new follower).", turn, Color.White));
            AddMessage(new(string.Format("(to give order : press <{0}>).", s_KeyBindings.AsString(PlayerCommand.ORDER_MODE)), turn, Color.White));
            return true;
          } else if (actorAt.Leader == player) {
            if (player.CanCancelLead(actorAt, out reason)) {
              int turn = Session.Get.WorldTime.TurnCounter;
              if (YesNoPopup(string.Format("Really ask {0} to leave", actorAt.TheName))) {
                DoCancelLead(player, actorAt);
                player.ActorScoring.AddEvent(turn, string.Format("Fired {0}.", actorAt.TheName));
                actorAt.ActorScoring.AddEvent(turn, string.Format("Fired by {0}.", player.TheName));
                return true;
              } else {
                AddMessage(new("Good, together you are strong.", turn, Color.Yellow));
                return false;
              }
            } else {
              ErrorPopup(string.Format("{0} can't leave : {1}.", actorAt.TheName, reason));
              return false;
            }
          } else {
            ErrorPopup(string.Format("Can't lead {0} : {1}.", actorAt.TheName, reason));
            return false;
          }
        } else {
          ErrorPopup("No one there.");
          return false;
        }
      }

      bool actionDone = DirectionCommandFiltered(lead_whom, lead, "No one here.");

      ClearOverlays();
      return actionDone;
    }

    private bool HandlePlayerPush(Actor player)
    {
      const string PUSH_MODE_TEXT = "PUSH/SHOVE MODE - directions to push/shove, ESC cancels";

      string err = player.ReasonCantPush();
      if (!string.IsNullOrEmpty(err)) {
        ErrorPopup(err);
        return false;
      }

      ClearOverlays();
      AddOverlay(new OverlayPopup(PUSH_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));

      Point? push_from(Direction dir) { return dir == Direction.NEUTRAL ? null : new Point?(player.Location.Position + dir); }
      bool push(Point? pos) {
        if (null == pos) return false;
        if (!player.Location.Map.IsValid(pos.Value)) return false;
        string reason;
        MapObject? mapObj;
        var other = player.Location.Map.GetActorAtExt(pos.Value);
        if (other != null) {
          if (player.CanShove(other,out reason)) {
            return HandlePlayerShoveActor(player, other);
          } else {
            ErrorPopup(string.Format("Cannot shove {0} : {1}.", other.TheName, reason));
            return false;
          }
        } else if (null != (mapObj = player.Location.Map.GetMapObjectAtExt(pos.Value))) {
          if (player.CanPush(mapObj, out reason)) {
            return HandlePlayerPushObject(player, mapObj);
          } else {
            ErrorPopup(string.Format("Cannot move {0} : {1}.", mapObj.TheName, reason));
            return false;
          }
        } else {
          ErrorPopup("Nothing to push there.");
          return false;
        }
      }

      bool actionDone = DirectionCommand(push_from, push);

      ClearOverlays();
      return actionDone;
    }

    private bool HandlePlayerPushObject(Actor player, MapObject mapObj)
    {
      const string PUSH_OBJECT_MODE_TEXT = "PUSHING {0} - directions to push, ESC cancels";

      ClearOverlays();
      AddOverlay(new OverlayPopup(string.Format(PUSH_OBJECT_MODE_TEXT,  mapObj.TheName), MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));
      AddOverlay(new OverlayRect(Color.Yellow, new GDI_Rectangle(MapToScreen(mapObj.Location), SIZE_OF_TILE)));

      Location? push_to(Direction dir) { return dir == Direction.NEUTRAL ? null : new Location?(mapObj.Location + dir); }
      bool push(Location? pos) {
        if (null == pos) return false;
        var loc = pos.Value;
        if (!Map.Canonical(ref loc)) return false;
        if (mapObj.CanPushTo(loc, out string reason)) {
          DoPush(player, mapObj, loc);
          return true;
        } else {
          ErrorPopup(string.Format("Cannot move {0} there : {1}.", mapObj.TheName, reason));
          return false;
        }
      }

      bool actionDone = DirectionCommand(push_to, push);

      ClearOverlays();
      return actionDone;
    }

    private bool HandlePlayerShoveActor(Actor player, Actor other)
    {
      const string SHOVE_ACTOR_MODE_TEXT = "SHOVING {0} - directions to shove, ESC cancels";

      ClearOverlays();
      AddOverlay(new OverlayPopup(string.Format(SHOVE_ACTOR_MODE_TEXT,  other.TheName), MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));
      AddOverlay(new OverlayRect(Color.Yellow, new GDI_Rectangle(MapToScreen(other.Location), SIZE_OF_ACTOR)));
      if (other.Controller is ObjectiveAI ai) {
        var dests = ai.WantToGoHere(other.Location);
        if (null != dests) foreach(var loc in dests) AddOverlay(new OverlayRect(Color.Green, new GDI_Rectangle(MapToScreen(loc), SIZE_OF_ACTOR)));
      }

      Location? shove_to(Direction dir) { return dir == Direction.NEUTRAL ? null : new Location?(other.Location + dir); }
      bool shove(Location? pos) {
        if (null == pos) return false;
        var loc = pos.Value;
        if (!Map.Canonical(ref loc)) return false;
        if (other.CanBeShovedTo(in loc, out string reason)) {
          DoShove(player, other, loc);
          return true;
        } else {
          ErrorPopup(string.Format("Cannot shove {0} there : {1}.", other.TheName, reason));
          return false;
        }
      }

      bool actionDone = DirectionCommand(shove_to, shove);

      ClearOverlays();
      return actionDone;
    }

    private bool HandlePlayerPull(Actor player) // alpha10
    {
      const string PULL_MODE_TEXT = "PULL MODE - directions to select object, ESC cancels"; // alpha10

      // fail immediately for stupid cases.
      string err = player.ReasonCantPull();
      if (!string.IsNullOrEmpty(err)) {
        ErrorPopup(err);
        return false;
      }

      ClearOverlays();
      AddOverlay(new OverlayPopup(PULL_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));

      Point? pull_from(Direction dir) { return dir == Direction.NEUTRAL ? null : new Point?(player.Location.Position + dir); }
      bool pull(Point? pos) {
        if (null == pos) return false;
        var loc = new Location(player.Location.Map, pos.Value);
        if (!Map.Canonical(ref loc)) return false;
        string reason;
        MapObject? mapObj;
        var other = loc.Actor;
        if (null != other) {
          // pull-shove.
          if (player.CanShove(other,out reason)) { // if can shove, can pull-shove.
            return HandlePlayerPullActor(player, other);
          } else {
            ErrorPopup(string.Format("Cannot pull {0} : {1}.", other.TheName, reason));
            return false;
          }
        } else if (null != (mapObj = loc.MapObject)) { // pull.
          if (player.CanPush(mapObj, out reason)) { // if can push, can pull.
            return HandlePlayerPullObject(player, mapObj);
          } else {
            ErrorPopup(string.Format("Cannot move {0} : {1}.", mapObj.TheName, reason));
            return false;
          }
        } else {
          ErrorPopup("Nothing to pull there."); // nothing to pull.
          return false;
        }
      }

      bool actionDone = DirectionCommand(pull_from, pull);

      ClearOverlays(); // cleanup.
      return actionDone; // return if we did an action.
    }

    bool HandlePlayerPullObject(Actor player, MapObject mapObj) // alpha10
    {
      const string PULL_OBJECT_MODE_TEXT = "PULLING {0} - directions to walk to, ESC cancels";  // alpha10

      ClearOverlays();
      AddOverlay(new OverlayPopup(string.Format(PULL_OBJECT_MODE_TEXT, mapObj.TheName), MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));
      AddOverlay(new OverlayRect(Color.Yellow, new GDI_Rectangle(MapToScreen(mapObj.Location.Position), SIZE_OF_TILE)));

      Point? pull_where(Direction dir) { return dir == Direction.NEUTRAL ? null : new Point?(player.Location.Position + dir); }
      bool pull(Point? pos) {
        if (null == pos) return false;
        if (!player.Location.Map.IsInBounds(pos.Value)) return false;   // \todo this is not cross-district
        if (player.CanPull(mapObj, new Location(player.Location.Map, pos.Value), out string reason)) {
          DoPull(player, mapObj, new Location(player.Location.Map, pos.Value));
          return true;
        } else {
          ErrorPopup(string.Format("Cannot pull there : {0}.", reason));
          return false;
        }
      }

      bool actionDone = DirectionCommand(pull_where, pull);

      ClearOverlays();    // cleanup.
      return actionDone;  // return if we did an action.
    }

    bool HandlePlayerPullActor(Actor player, Actor other)   // alpha10
    {
      const string PULL_ACTOR_MODE_TEXT = "PULLING {0} - directions to walk to, ESC cancels";  // alpha10

      ClearOverlays();
      AddOverlay(new OverlayPopup(string.Format(PULL_ACTOR_MODE_TEXT, other.TheName), MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));
      AddOverlay(new OverlayRect(Color.Yellow, new GDI_Rectangle(MapToScreen(other.Location.Position), SIZE_OF_TILE)));

      Location? pull_where(Direction dir) { return dir == Direction.NEUTRAL ? null : new Location?(player.Location + dir); }
      bool pull(Location? dest) {
        if (null == dest) return false;
        Location _dest = dest.Value;
        if (!Map.Canonical(ref _dest)) return false;
        if (player.CanPull(other, _dest, out string reason)) {
          DoPullActor(player, other, _dest);
          return true;
        } else {
          ErrorPopup(string.Format("Cannot pull there : {0}.", reason));
          return false;
        }
      }

      bool actionDone = DirectionCommand(pull_where, pull);
      ClearOverlays();    // cleanup.
      return actionDone;  // return if we did an action.
    }

    private bool HandlePlayerUseSpray(Actor player)
    {
      var equippedItem = player.GetEquippedItem(DollPart.LEFT_HAND);
      if (null != equippedItem) {
        if (equippedItem is ItemSprayPaint s_paint) return HandlePlayerTag(player, s_paint);
        if (equippedItem is ItemSprayScent spray) {
          // alpha10 new way to use stench killer
          return HandlePlayerSprayOdorSuppressor(player, spray);
        }
      }
      ErrorPopup("No spray equipped.");
      return false;
    }

    private bool HandlePlayerTag(Actor player, ItemSprayPaint spray)
    {
      const string TAG_MODE_TEXT = "TAG MODE - directions to tag a wall or on the floor, ESC cancels";

      if (spray.PaintQuantity <= 0) {
        ErrorPopup("No paint left.");
        return false;
      }
      ClearOverlays();
      AddOverlay(new OverlayPopup(TAG_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));

      // \todo This is not cross-district, but we're rethinking spray paint anyway)
      Point? spray_where(Direction dir) { return dir == Direction.NEUTRAL ? null : new Point?(player.Location.Position + dir); }
      bool spray_on(Point? pos) {
        if (null == pos) return false;
        if (!player.Location.Map.IsInBounds(pos.Value)) return false;
        if (CanTag(player.Location.Map, pos.Value, out string reason)) {
          DoTag(player, spray, pos.Value);
          return true;
        } else {
          ErrorPopup(string.Format("Can't tag there : {0}.", reason));
          return false;
        }
      }

      bool flag2 = DirectionCommand(spray_where,spray_on);
      ClearOverlays();
      return flag2;
    }
#nullable restore

    private bool CanTag(Map map, Point pos, out string reason)
    {
      if (!map.IsInBounds(pos)) {
        reason = "out of map";
        return false;
      }
      if (map.HasActorAt(in pos)) {
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

#nullable enable
    // alpha10 new way to use stench killer
    private bool HandlePlayerSprayOdorSuppressor(Actor player, ItemSprayScent spray)
    {
      const string SPRAY_MODE_TEXT = "SPRAY MODE - directions to spray or wait key to spray on yourself, ESC cancels";  // alpha10

      // Check if has odor suppressor, etc.
      if (!player.CanSprayOdorSuppressor(spray, out string reason)) {
        ErrorPopup(reason);
        return false;
      }

      bool actionDone = false;
      ClearOverlays();
      AddOverlay(new OverlayPopup(SPRAY_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));

      Actor? spray_who(Direction dir) { return dir == Direction.NEUTRAL ? player : player.Location.Map.GetActorAtExt(player.Location.Position + dir); }
      bool spray_on(Actor? who) {
        if (null == who) {
          ErrorPopup("No one to spray on there.");
          return false;
        } else if (player.CanSprayOdorSuppressor(spray, who, out string reason)) {
          DoSprayOdorSuppressor(player, spray, who);
          return true;
        } else {
          ErrorPopup(string.Format("Can't spray here : {0}.", reason));
          return false;
        }
      }

      actionDone = DirectionCommandFiltered(spray_who, spray_on, "No one to spray on here.");

      ClearOverlays();
      return actionDone;
    }
#nullable restore

    private bool HandlePlayerOrderPCMode(PlayerController pc) {
      // check for meaningful tasks to automate
      var orders = pc.GetValidSelfOrders();
      if (0 >= orders.Count) {
        ErrorPopup("No applicable orders for yourself.");
        return false;
      }

      string label(int index) { return orders[index].Key; }
      bool details(int index) { orders[index].Value(); return true; }

      PagedPopup("Orders for yourself:", orders.Count, label, details);
      return pc.AutoPilotIsOn;
    }

    private void HandlePlayerCountermandPC(PlayerController pc) {
      var orders = pc.CurrentSelfOrders;
      if (null == orders) {
        ErrorPopup("No current self-orders.");
        return;
      }

      string label(int index) { return orders[index].ToString(); }
      bool details(int index) {
        var ret = YesNoPopup("Countermand '"+orders[index].ToString()+"'");
        if (ret) pc.Countermand(orders[index]);
        return ret;
      }

      PagedPopup("Current self-orders", orders.Length, label, details);
    }

    private bool HandlePlayerOrderMode(PlayerController pc)
    {
      var player = pc.ControlledActor; // backward compatibiblity
      if (player.CountFollowers == 0) {
        ErrorPopup("No followers to give orders to.");
        return false;
      }
      Actor[] actorArray = new Actor[player.CountFollowers];
      HashSet<Point>[] pointSetArray = new HashSet<Point>[player.CountFollowers];
      bool[] flagArray = new bool[player.CountFollowers];
      int index1 = 0;
      foreach (Actor follower in player.Followers) {
        actorArray[index1] = follower;
        pointSetArray[index1] = LOS.ComputeFOVFor(follower);
        bool flag1 = pointSetArray[index1].Contains(player.Location.Position) && player.Controller.CanSee(follower.Location);
        bool flag2 = AreLinkedByPhone(player, follower);
        flagArray[index1] = (player.Controller as PlayerController).InCommunicationWith(follower);
        ++index1;
      }
      if (player.CountFollowers == 1 && flagArray[0]) {
        bool flag = HandlePlayerOrderFollower(pc, actorArray[0]);
        ClearOverlays();
        ClearMessages();
        return flag;
      }
      bool flag3 = true;
      bool flag4 = false;
      int num1 = 0;
      do {
        ClearOverlays();
        AddOverlay(new OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));
        ClearMessages();
        AddMessage(new("Choose a follower.", Session.Get.WorldTime.TurnCounter, Color.Yellow));
        int num2;
        for (num2 = 0; num2 < 5 && num1 + num2 < actorArray.Length; ++num2) {
          int index2 = num2 + num1;
          Actor follower = actorArray[index2];
          string str = DescribePlayerFollowerStatus(follower);
          if (flagArray[index2])
            AddMessage(new(string.Format("{0}. {1}/{2} {3}... {4}.", 1 + num2, index2 + 1, actorArray.Length, follower.Name, str), Session.Get.WorldTime.TurnCounter, Color.LightGreen));
          else
            AddMessage(new(string.Format("{0}. {1}/{2} ({3}) {4}.", 1 + num2, index2 + 1, actorArray.Length, follower.Name, str), Session.Get.WorldTime.TurnCounter, Color.DarkGray));
        }
        if (num2 < actorArray.Length)
          AddMessage(new("9. next", Session.Get.WorldTime.TurnCounter, Color.LightGreen));
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
            if (HandlePlayerOrderFollower(pc, follower)) {
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

    private bool HandlePlayerDirectiveFollower(Actor player, OrderableAI ordai)    // XXX could be void return but other HandlePlayer... return bool
    {
      var follower = ordai.ControlledActor;
      var mode = new OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty);
      AddOverlay(mode);

      bool flag1 = true;
      do {
        ActorDirective directives = ordai.Directives_nocreate ?? new(); // UI, so not as big a deal to thrash GC
        ClearMessages();
        AddMessage(new(string.Format("{0} directives...", follower.Name), Session.Get.WorldTime.TurnCounter, Color.Yellow));
        AddMessage(new(string.Format("1. {0} weapons.", directives.CanFireWeapons ? "Fire" : "Don't fire"), Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        AddMessage(new(string.Format("2. {0} grenades.", directives.CanThrowGrenades ? "Throw" : "Don't throw"), Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        AddMessage(new(string.Format("3. {0}.", directives.CanSleep ? "Sleep" : "Don't sleep"), Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        AddMessage(new(string.Format("4. {0}.", directives.CanTrade ? "Trade" : "Don't trade"), Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        RedrawPlayScreen(new UI.Message(string.Format("5. {0}.", directives.Courage.to_s()), Session.Get.WorldTime.TurnCounter, Color.LightGreen));
        KeyEventArgs keyEventArgs = m_UI.UI_WaitKey();
        int choiceNumber = KeyToChoiceNumber(keyEventArgs.KeyCode);
        if (keyEventArgs.KeyCode == Keys.Escape) flag1 = false;
        else {
          directives = ordai.Directives;    // force live copy
          switch (choiceNumber) {
            case 1:
              directives.CanFireWeapons = !directives.CanFireWeapons;
              break;
            case 2:
              directives.CanThrowGrenades = !directives.CanThrowGrenades;
              break;
            case 3:
              directives.CanSleep = !directives.CanSleep;
              break;
            case 4:
              directives.CanTrade = !directives.CanTrade;
              break;
            case 5:
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
      RemoveOverlay(mode);
      return false;
    }

    private bool HandlePlayerOrderFollower(PlayerController pc, Actor follower)
    {
      var player = pc.ControlledActor;  // backward compatibility
      if (!follower.IsTrustingLeader) {
        if (IsVisibleToPlayer(follower))
          follower.Say(player, "Sorry, I don't trust you enough yet.", Sayflags.IS_IMPORTANT | Sayflags.IS_FREE_ACTION);
        else if (AreLinkedByPhone(follower, player))
          pc.AddMessageForceRead(MakeMessage(follower, "Sorry, I don't trust you enough yet."));
        return false;
      }
      string str1 = DescribePlayerFollowerStatus(follower);
      var fovFor = LOS.ComputeFOVFor(follower);
      var fo_ordai = follower.Controller as OrderableAI;

      // a mechanical interpretation of the legacy order UI; intentionally not using programmer time to optimize UI
      List<KeyValuePair<string, Func<bool>>> orders = new();
      if (null != fo_ordai) {
          if (null != fo_ordai.Order) {
            orders.Add(new(string.Format("0. Cancel legacy order {0}.", str1), () => {
              DoCancelOrder(player, fo_ordai);
              return true;
            }));
          }

        orders.Add(new("Set directives...", () => {
          HandlePlayerDirectiveFollower(player, fo_ordai);
          return false;
        }));

        if (follower.Inventory.Has<ItemBarricadeMaterial>()) {
          orders.Add(new("Barricade (one)...", () => {
            return HandlePlayerOrderFollowerToBarricade(player, fo_ordai, fovFor, false);
          }));

          orders.Add(new("Barricade (max)...", () => {
            return HandlePlayerOrderFollowerToBarricade(player, fo_ordai, fovFor, true);
          }));
        }

        orders.Add(new("Guard...", () => {
          return HandlePlayerOrderFollowerToGuard(player, fo_ordai, fovFor);
        }));

        orders.Add(new("Patrol...", () => {
          return HandlePlayerOrderFollowerToPatrol(player, fo_ordai, fovFor);
        }));

        if (   0 < follower.MySkills.GetSkillLevel(Skills.IDs.CARPENTRY)
            && follower.Inventory.Has<ItemBarricadeMaterial>()) {
          var have = follower.CountItems<ItemBarricadeMaterial>();

          if (follower.BarricadingMaterialNeedForFortification(false) <= have) {
            orders.Add(new("Build small fort.", () => {
              return HandlePlayerOrderFollowerToBuildFortification(player, fo_ordai, fovFor, false);
            }));
          }

          if (follower.BarricadingMaterialNeedForFortification(true) <= have) {
            orders.Add(new("Build large fort.", () => {
              return HandlePlayerOrderFollowerToBuildFortification(player, fo_ordai, fovFor, true);
            }));
          }
        }

        orders.Add(new("Report events.", () => {
          DoGiveOrderTo(player, fo_ordai, new ActorOrder(ActorTasks.REPORT_EVENTS, follower.Location));
          return true;
        }));

        orders.Add(new("Sleep now.", () => {
          DoGiveOrderTo(player, fo_ordai, new ActorOrder(ActorTasks.SLEEP_NOW, follower.Location));
          return true;
        }));

        orders.Add(new((fo_ordai.DontFollowLeader ? "Start" : "Stop") + " following me.", () => {
          DoGiveOrderTo(player, fo_ordai, new ActorOrder(ActorTasks.FOLLOW_TOGGLE, follower.Location));
          return true;
        }));

        orders.Add(new("Where are you?", () => {
          DoGiveOrderTo(player, fo_ordai, new ActorOrder(ActorTasks.WHERE_ARE_YOU, follower.Location));
          return true;
        }));
      }

      // old-style but valid for PCs
      if (null == FollowerCannotGiveItems(player, follower)) {
        orders.Add(new("Give me...", () => {
            return HandlePlayerOrderFollowerToGiveItems(player, follower);
        }));
      }

      // new-style orders, that are valid for PCs, go here
      orders.Add(new("What's up?", () => {
        if (null != fo_ordai) fo_ordai.HandlePlayerCountermand();
        else HandlePlayerCountermandPC(follower.Controller as PlayerController);
        return false;
      }));

      if (!follower.IsPlayer) {
        orders.Add(new("We need to talk...", () => {
          follower.MakePC();
          Session.Get.Scoring.UseReincarnation(); // intentionally unconditional
          return true;
        }));
      }

      if (follower.IsPlayer || !follower.Controller.InCombat) {
        bool could_lead(Actor ally) {
          var oai = ally.Controller as ObjectiveAI;
          if (null == oai) return false;
//          InfoPopup(ally.Name+" : "+ ally.CountFollowers.ToString());
          if (ally.CountFollowers >= ally.MaxFollowers) return false;
//          InfoPopup((!ally.IsPlayer && ally.Controller.InCombat).ToString());
          if (!ally.IsPlayer && ally.Controller.InCombat) return false;
//          InfoPopup((!ally.IsPlayer && ally.Controller.InCombat).ToString());
          if (!oai.InCommunicationWith(player)) return false;
//          InfoPopup((!oai.InCommunicationWith(player)).ToString());
          if (!pc.InCommunicationWith(ally)) return false;
//          InfoPopup((!pc.InCommunicationWith(ally)).ToString());
          if (!oai.InCommunicationWith(follower)) return false;
//          InfoPopup((!oai.InCommunicationWith(follower)).ToString());
//        if (!pc.InCommunicationWith(follower)) return false; // precondition of the master function call
          return true;
        }

        void msg(Actor authority, Actor whom, Actor new_leader) {
          var authority_whom_code = pc.CommunicationMethodCode(whom);
          var authority_new_leader_code = pc.CommunicationMethodCode(new_leader);
          if (2 == authority_whom_code || 2 == authority_new_leader_code) {
            UI.Message msg = new(string.Format("(police radio, {0}) {1}, follow {2}.", authority.Name, whom.Name, new_leader.Name), Session.Get.WorldTime.TurnCounter, Color.White);
            Action<PlayerController> pc_add_msg = pc => pc.AddMessage(msg);
            authority.MessageAllInDistrictByRadio(NOP, FALSE, pc_add_msg, pc_add_msg, TRUE);
          }
        }

        var allies = player.Allies;
        var donate_to = player.FilterAllies(allies, could_lead);
        if (null != donate_to) {
          orders.Add(new("Follow ...", () => {
            string label(int index) { return donate_to[index].Name; }
            bool details(int index) {
                var act = new TransferFollower(player, follower, donate_to[index], msg);
                if (act.IsPerformable()) {
                    act.Perform();
                    return true;
                }
                InfoPopup(act.FailReason);
                return false;
            }

            return PagedPopup("Current self-orders", donate_to.Count, label, details);
          }));
        }
      }

      string label(int index) { return orders[index].Key; }
      bool details(int index) { return orders[index].Value(); }

      return PagedPopup(string.Format("Order {0} to...", follower.Name), orders.Count, label, details, false);
    }

    private bool HandlePlayerOrderFollowerToBuildFortification(Actor player, OrderableAI ordai, HashSet<Point> followerFOV, bool isLarge)
    {
      var follower = ordai.ControlledActor;

      var mode = new OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty);
      AddOverlay(mode);

      bool flag1 = true;
      bool flag2 = false;
      Map map1 = player.Location.Map;
      Point? nullable = null;
      OverlayRect selected = null;
      Color color = Color.White;
      do {
        ClearMessages();
        AddMessage(new(string.Format("Ordering {0} to build {1} fortification...", follower.Name, isLarge ? "large" : "small"), Session.Get.WorldTime.TurnCounter, Color.Yellow));
        RedrawPlayScreen(new UI.Message("<LMB> on a map object.", Session.Get.WorldTime.TurnCounter, Color.LightGreen));

        var ui_event = m_UI.WaitKeyOrMouse();
        // backward compatibility
        var key = ui_event.Key.Key;
        var mouseButtons = ui_event.Key.Value;
        GDI_Point mousePos = new(ui_event.Value.Key, ui_event.Value.Value);

        if (key != null) {
          if (key.KeyCode == Keys.Escape) flag1 = false;
        } else {
          Point map2 = MouseToMap(mousePos);
          if (map1.IsValid(map2) && IsInViewRect(map2)) {
            if (IsVisibleToPlayer(map1, in map2) && followerFOV.Contains(map2)) {
              if (follower.CanBuildFortification(map2, isLarge, out string reason)) {
                color = Color.LightGreen;
                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left) {
                  DoGiveOrderTo(player, ordai, new ActorOrder(isLarge ? ActorTasks.BUILD_LARGE_FORTIFICATION : ActorTasks.BUILD_SMALL_FORTIFICATION, new Location(player.Location.Map, map2)));
                  flag1 = false;
                  flag2 = true;
                }
              } else {
                color = Color.Red;
                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left)
                  ErrorPopup(string.Format("Can't build {0} fortification : {1}.", isLarge ? "large" : "small", reason));
              }
            } else {
              color = Color.Red;
            }
            if (null == nullable || nullable.Value != map2) {
              nullable = map2;
              if (null != selected) RemoveOverlay(selected);
              selected = new OverlayRect(color, new GDI_Rectangle(MapToScreen(nullable.Value), SIZE_OF_TILE));
              AddOverlay(selected);
            }
          }
        }
      }
      while (flag1);
      RemoveOverlay(mode);
      if (null != selected) RemoveOverlay(selected);
      return flag2;
    }

    private bool HandlePlayerOrderFollowerToBarricade(Actor player, OrderableAI ordai, HashSet<Point> followerFOV, bool toTheMax)
    {
      var follower = ordai.ControlledActor;

      var mode = new OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty);
      AddOverlay(mode);

      bool flag1 = true;
      bool flag2 = false;
      Map map1 = player.Location.Map;
      Point? nullable = null;
      OverlayRect selected = null;
      Color color = Color.White;
      do {
        ClearMessages();
        AddMessage(new(string.Format("Ordering {0} to barricade...", follower.Name), Session.Get.WorldTime.TurnCounter, Color.Yellow));
        RedrawPlayScreen(new UI.Message("<LMB> on a map object.", Session.Get.WorldTime.TurnCounter, Color.LightGreen));

        var ui_event = m_UI.WaitKeyOrMouse();
        // backward compatibility
        var key = ui_event.Key.Key;
        var mouseButtons = ui_event.Key.Value;
        GDI_Point mousePos = new(ui_event.Value.Key, ui_event.Value.Value);

        if (key != null) {
          if (key.KeyCode == Keys.Escape) flag1 = false;
        } else {
          Point map2 = MouseToMap(mousePos);
          if (map1.IsValid(map2) && IsInViewRect(map2)) {
            if (IsVisibleToPlayer(map1, in map2) && followerFOV.Contains(map2)) {
              if (map1.GetMapObjectAt(map2) is DoorWindow door) {
                if (follower.CanBarricade(door, out string reason)) {
                  color = Color.LightGreen;
                  if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left) {
                    DoGiveOrderTo(player, ordai, new ActorOrder(toTheMax ? ActorTasks.BARRICADE_MAX : ActorTasks.BARRICADE_ONE, door.Location));
                    flag1 = false;
                    flag2 = true;
                  }
                } else {
                  color = Color.Red;
                  if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left)
                    ErrorPopup(string.Format("Can't barricade {0} : {1}.", door.TheName, reason));
                }
              } else color = Color.Red;
            } else color = Color.Red;
            if (null == nullable || nullable.Value != map2) {
              nullable = map2;
              if (null != selected) RemoveOverlay(selected);
              selected = new OverlayRect(color, new GDI_Rectangle(MapToScreen(nullable.Value), SIZE_OF_TILE));
              AddOverlay(selected);
            }
          }
        }
      }
      while (flag1);
      RemoveOverlay(mode);
      if (null != selected) RemoveOverlay(selected);
      return flag2;
    }

    private bool HandlePlayerOrderFollowerToGuard(Actor player, OrderableAI ordai, HashSet<Point> followerFOV)
    {
      var follower = ordai.ControlledActor;

      var mode = new OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty);
      AddOverlay(mode);

      bool flag1 = true;
      bool flag2 = false;
      Map map1 = player.Location.Map;
      Point? nullable = null;
      OverlayRect selected = null;
      Color color = Color.White;
      do {
        ClearMessages();
        AddMessage(new(string.Format("Ordering {0} to guard...", follower.Name), Session.Get.WorldTime.TurnCounter, Color.Yellow));
        RedrawPlayScreen(new UI.Message("<LMB> on a map position.", Session.Get.WorldTime.TurnCounter, Color.LightGreen));

        var ui_event = m_UI.WaitKeyOrMouse();
        // backward compatibility
        var key = ui_event.Key.Key;
        var mouseButtons = ui_event.Key.Value;
        GDI_Point mousePos = new(ui_event.Value.Key, ui_event.Value.Value);

        if (key != null) {
          if (key.KeyCode == Keys.Escape) flag1 = false;
        } else {
          Point map2 = MouseToMap(mousePos);
          if (map1.IsValid(map2) && IsInViewRect(map2)) {
            if (IsVisibleToPlayer(map1, in map2) && followerFOV.Contains(map2)) {
              if (map2 == follower.Location.Position || map1.IsWalkableFor(map2, follower, out string reason)) {
                color = Color.LightGreen;
                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left) {
                  DoGiveOrderTo(player, ordai, new ActorOrder(ActorTasks.GUARD, new Location(map1, map2)));
                  flag1 = false;
                  flag2 = true;
                }
              } else {
                color = Color.Red;
                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left)
                  ErrorPopup(string.Format("Can't guard here : {0}", reason));
              }
            } else {
              color = Color.Red;
            }
            if (null == nullable || nullable.Value != map2) {
              nullable = map2;
              if (null != selected) RemoveOverlay(selected);
              selected = new OverlayRect(color, new GDI_Rectangle(MapToScreen(nullable.Value), SIZE_OF_TILE));
              AddOverlay(selected);
            }
          }
        }
      }
      while (flag1);
      RemoveOverlay(mode);
      if (null != selected) RemoveOverlay(selected);
      return flag2;
    }

    private bool HandlePlayerOrderFollowerToPatrol(Actor player, OrderableAI ordai, HashSet<Point> followerFOV)
    {
      var follower = ordai.ControlledActor;

      var mode = new OverlayPopup(ORDER_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty);
      AddOverlay(mode);

      bool flag1 = true;
      bool flag2 = false;
      Map map1 = player.Location.Map;
      Point? nullable = null;
      OverlayRect? selected = null;
      OverlayPopup? zones = null;
      Color color = Color.White;
      do {
        ClearMessages();
        AddMessage(new(string.Format("Ordering {0} to patrol...", follower.Name), Session.Get.WorldTime.TurnCounter, Color.Yellow));
        RedrawPlayScreen(new UI.Message("<LMB> on a map position.", Session.Get.WorldTime.TurnCounter, Color.LightGreen));

        var ui_event = m_UI.WaitKeyOrMouse();
        // backward compatibility
        var key = ui_event.Key.Key;
        var mouseButtons = ui_event.Key.Value;
        GDI_Point mousePos = new(ui_event.Value.Key, ui_event.Value.Value);

        if (key != null) {
          if (key.KeyCode == Keys.Escape) flag1 = false;
        } else {
          Point map2 = MouseToMap(mousePos);
          if (map1.IsValid(map2) && IsInViewRect(map2)) {
            if (IsVisibleToPlayer(map1, in map2) && followerFOV.Contains(map2)) {
              bool flag3 = true;
              string reason = "";
              if (map1.GetZonesAt(map2) == null) {
                flag3 = false;
                reason = "no zone here";
              } else if (map2 != follower.Location.Position && !map1.IsWalkableFor(map2, follower, out reason))
                flag3 = false;
              if (flag3) {
                color = Color.LightGreen;
                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left) {
                  DoGiveOrderTo(player, ordai, new ActorOrder(ActorTasks.PATROL, new Location(map1, map2)));
                  flag1 = false;
                  flag2 = true;
                }
              } else {
                color = Color.Red;
                if (mouseButtons.HasValue && mouseButtons.Value == MouseButtons.Left)
                  ErrorPopup(string.Format("Can't patrol here : {0}", reason));
              }
            } else {
              color = Color.Red;
            }
            if (null == nullable || nullable.Value != map2) {
              nullable = map2;
              if (null != selected) RemoveOverlay(selected);
              selected = new OverlayRect(color, new GDI_Rectangle(MapToScreen(nullable.Value), SIZE_OF_TILE));
              AddOverlay(selected);

              if (null != zones) RemoveOverlay(zones);
              var zonesAt = map1.GetZonesAt(nullable.Value);
              if (null != zonesAt) {
                string[] lines = new string[zonesAt.Count + 1];
                lines[0] = "Zone(s) here :";
                for (int index = 0; index < zonesAt.Count; ++index)
                 lines[index + 1] = string.Format("- {0}", zonesAt[index].Name);
                zones = new OverlayPopup(lines, Color.White, Color.White, POPUP_FILLCOLOR, MapToScreen(nullable.Value + Direction.SE));
                AddOverlay(zones);
              } else zones = null;
            }
          }
        }
      }
      while (flag1);
      RemoveOverlay(mode);
      if (null != selected) RemoveOverlay(selected);
      if (null != zones) RemoveOverlay(zones);
      return flag2;
    }

    private string? FollowerCannotGiveItems(Actor player, Actor follower) {
      if (follower.Inventory == null || follower.Inventory.IsEmpty) 
        return string.Format("{0} has no items to give.", follower.TheName);
      if (!Rules.IsAdjacent(player.Location, follower.Location))
        return string.Format("{0} is not next to you.", follower.TheName);
      return null;
    }

    private bool HandlePlayerOrderFollowerToGiveItems(Actor player, Actor follower)
    {
#if DEBUG
      var err = FollowerCannotGiveItems(player, follower);
      if (null != err) throw new InvalidOperationException(err);    // precondition violation
#endif

      var can_give = follower.Inventory.grep(obj => follower.CanGiveTo(player, obj));
      if (null == can_give) {
        ErrorPopup(string.Format("{0} can give no items.", follower.TheName));
        return false;
      }

      string label(int index) { return string.Format("{0}/{1} {2}.", index + 1, can_give.Count, DescribeItemShort(can_give[index])); }
      bool details(int index) {
        DoGiveItemTo(follower, player, can_give[index]);
        return true;
      }

      return PagedPopup(string.Format("Ordering {0} to give...", follower.Name), can_give.Count, label, details);
    }

#nullable enable
    private void HandleAiActor(ActorController ai)
    {
#if DEBUG
      if (ai is PlayerController) throw new ArgumentOutOfRangeException(nameof(ai), "ai is PlayerController");
#endif
      var aiActor = ai.ControlledActor; // backward compatibility

#if DEBUG
      if (aiActor.IsSleeping) throw new ArgumentOutOfRangeException(nameof(aiActor),"cannot act while sleeping");
      if (aiActor.IsDebuggingTarget) World.Get.DaimonMap(); // so we have a completely correct map when things go wrong
      if (aiActor!=aiActor.Location.Map.NextActorToAct) throw new InvalidProgramException("trying to process the wrong actor");

      int AP_checkpoint = aiActor.ActionPoints;
      Location loc_checkpoint = aiActor.Location;
#endif
      var actorAction = ai.GetAction();
      if (actorAction == null) throw new InvalidOperationException("AI returned null action.");
      if (!actorAction.IsPerformable()) throw new InvalidOperationException(string.Format("AI attempted illegal action {0}; actorAI: {1}; fail reason : {2}.", actorAction.GetType().ToString(), ai.GetType().ToString(), actorAction.FailReason));
      if (aiActor.IsInsane && Rules.Get.RollChance(Rules.SANITY_INSANE_ACTION_CHANCE)) {
        var insaneAction = GenerateInsaneAction(aiActor);
        if (null != insaneAction && insaneAction.IsPerformable()) actorAction = insaneAction;
      }
      // we need to know if this got past internal testing.
#if DEBUG
      if (aiActor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "action: "+actorAction.ToString()+"; starting AP "+aiActor.ActionPoints);
#endif
      actorAction.Perform();
#if DEBUG
      if (AP_checkpoint == aiActor.ActionPoints && loc_checkpoint == aiActor.Location && !(actorAction is ActionCloseDoor || actorAction is ActionSay)) {
        throw new InvalidOperationException(aiActor.Name+" got a free action "+actorAction.ToString());
      }
      if (aiActor.IsDebuggingTarget) Logger.WriteLine(Logger.Stage.RUN_MAIN, "ending AP "+aiActor.ActionPoints);
#endif
      if (actorAction is ActorTake || actorAction is ActorGive || actorAction is Use<Item>) {
        var errors = new List<string>();
        World.Get._RejectInventoryDamage(errors, aiActor);
        if (0 < errors.Count) throw new InvalidOperationException(aiActor.Name + " action " + actorAction + " triggered:\n" + string.Join("\n", errors));
      }
      if (actorAction is ActorDest && null != aiActor.Threats) ai.UpdateSensors(); // to trigger fast threat/tourism update
    }

    private void HandleAdvisor(PlayerController pc)
    {
      if (s_Hints.HasAdvisorGivenAllHints()) {
        ShowAdvisorMessage("YOU KNOW THE BASICS!", new string[7]{
          "The Advisor has given you all the hints.",
          "You can disable the advisor in the options.",
          "Read the manual or discover the rest of the game by yourself.",
          "Good luck and have fun!",
          string.Format("To REDEFINE THE KEYS : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.KEYBINDING_MODE)),
          string.Format("To CHANGE OPTIONS    : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.OPTIONS_MODE)),
          string.Format("To READ THE MANUAL   : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.HELP_MODE))
        });
      } else {
        for (int index = 0; index < (int)AdvisorHint._COUNT; ++index) {
          if (!s_Hints.IsAdvisorHintGiven((AdvisorHint) index) && IsAdvisorHintAppliable((AdvisorHint) index)) {
            AdvisorGiveHint((AdvisorHint) index);
            return;
          }
        }
        ShowAdvisorMessage("No hint available.", new string[5]{
          "The Advisor has no new hint for you in this situation.",
          "You will see a popup when he has something to say.",
          string.Format("To REDEFINE THE KEYS : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.KEYBINDING_MODE)),
          string.Format("To CHANGE OPTIONS    : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.OPTIONS_MODE)),
          string.Format("To READ THE MANUAL   : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.HELP_MODE))
        });
      }
    }

    // alpha10
    /// <returns>-1 if none</returns>
    private int GetAdvisorFirstAvailableHint()
    {
      for (int i = 0; i < (int)AdvisorHint._COUNT; i++) {
        if (s_Hints.IsAdvisorHintGiven((AdvisorHint)i)) continue;
        if (IsAdvisorHintAppliable((AdvisorHint)i)) return i;
      }

      return -1;
    }

    private void AdvisorGiveHint(AdvisorHint hint)
    {
      s_Hints.SetAdvisorHintAsGiven(hint);
      SaveHints();
      ShowAdvisorHint(hint);
    }
#nullable restore

    private bool IsAdvisorHintAppliable(AdvisorHint hint)
    {
      Map map = Player.Location.Map;
      Point position = Player.Location.Position;
      switch (hint)
      {
        case AdvisorHint.MOVE_BASIC: return true;
        case AdvisorHint.MOUSE_LOOK:
          return map.LocalTime.TurnCounter >= 2;
        case AdvisorHint.KEYS_OPTIONS: return true;
        case AdvisorHint.NIGHT:
          return map.LocalTime.TurnCounter >= WorldTime.TURNS_PER_HOUR;
        case AdvisorHint.RAIN:
          if (World.Get.Weather.IsRain())
            return map.LocalTime.TurnCounter >= 2*WorldTime.TURNS_PER_HOUR;
          return false;
        case AdvisorHint.ACTOR_MELEE: return Player.IsAdjacentToEnemy;
        case AdvisorHint.MOVE_RUN:
          if (map.LocalTime.TurnCounter >= 5)
            return Player.CanRun();
          return false;
        case AdvisorHint.MOVE_RESTING: return Player.IsTired;
        case AdvisorHint.MOVE_JUMP:
          return !Player.IsTired && map.AnyAdjacent<MapObject>(position, obj => obj.IsJumpable);
        case AdvisorHint.ITEM_GRAB_CONTAINER:
          return map.HasAnyAdjacentInMap(position, pt => (Player.Controller as PlayerController).CanGetFromContainer(pt));
        case AdvisorHint.ITEM_GRAB_FLOOR:
          var itemsAt = map.GetItemsAt(position);
          if (itemsAt == null) return false;
          foreach (var it in itemsAt.Items) if (Player.CanGet(it)) return true;
          return false;
        case AdvisorHint.ITEM_UNEQUIP:
          var inventory1 = Player.Inventory;
          if (inventory1?.IsEmpty ?? true) return false;
          foreach (Item it in inventory1.Items) if (Player.CanUnequip(it)) return true;
          return false;
        case AdvisorHint.ITEM_EQUIP:
          var inventory2 = Player.Inventory;
          if (inventory2?.IsEmpty ?? true) return false;
          foreach (Item it in inventory2.Items) if (!it.IsEquipped && Player.CanEquip(it)) return true;
          return false;
        case AdvisorHint.ITEM_TYPE_BARRICADING:
          var inventory3 = Player.Inventory;
          if (inventory3?.IsEmpty ?? true) return false;
          return inventory3.Has<ItemBarricadeMaterial>();
        case AdvisorHint.ITEM_DROP:
          var inventory4 = Player.Inventory;
          if (inventory4?.IsEmpty ?? true) return false;
          foreach (Item it in inventory4.Items) if (Player.CanDrop(it)) return true;
          return false;
        case AdvisorHint.ITEM_USE:
          var inventory5 = Player.Inventory;
          if (inventory5?.IsEmpty ?? true) return false;
          foreach (Item it in inventory5.Items) if (Player.CanUse(it)) return true;
          return false;
        case AdvisorHint.FLASHLIGHT: return Player.Inventory.Has<ItemLight>();
        case AdvisorHint.CELLPHONES: return Player.Inventory.GetFirstByModel(GameItems.CELL_PHONE) != null;
        case AdvisorHint.SPRAYS_PAINT: return Player.Inventory.Has<ItemSprayPaint>();
        case AdvisorHint.SPRAYS_SCENT: return Player.Inventory.Has<ItemSprayScent>();
        case AdvisorHint.WEAPON_FIRE:
          return 0 < ((Player.GetEquippedWeapon() as ItemRangedWeapon)?.Ammo ?? 0);
        case AdvisorHint.WEAPON_RELOAD:
          if (!(Player.GetEquippedWeapon() is ItemRangedWeapon)) return false;
          var inventory6 = Player.Inventory;
          if (inventory6?.IsEmpty ?? true) return false;
          foreach (Item it in inventory6.Items) if (it is ItemAmmo && Player.CanUse(it)) return true;
          return false;
        case AdvisorHint.GRENADE: return Player.Has<ItemGrenade>();
        case AdvisorHint.DOORWINDOW_OPEN:
          return map.AnyAdjacent<DoorWindow>(position, door => Player.CanOpen(door));
        case AdvisorHint.DOORWINDOW_CLOSE:
          return map.AnyAdjacent<DoorWindow>(position, door => Player.CanClose(door));
        case AdvisorHint.OBJECT_PUSH:
          return map.AnyAdjacent<MapObject>(position, mapObjectAt => Player.CanPush(mapObjectAt));
        case AdvisorHint.OBJECT_BREAK:
          return map.AnyAdjacent<MapObject>(position, mapObjectAt => Player.CanBreak(mapObjectAt));
        case AdvisorHint.BARRICADE:
          return map.AnyAdjacent<DoorWindow>(position, door => Player.CanBarricade(door));
        case AdvisorHint.EXIT_STAIRS_LADDERS: return map.HasExitAt(in position);
        case AdvisorHint.EXIT_LEAVING_DISTRICT:
          foreach (var point in position.Adjacent()) {
            if (!map.IsInBounds(point) && map.HasExitAt(in point)) return true;
          }
          return false;
        case AdvisorHint.STATE_SLEEPY: return Player.IsSleepy;
        case AdvisorHint.STATE_HUNGRY: return Player.IsHungry;
        case AdvisorHint.NPC_TRADE:
          return map.HasAnyAdjacentInMap(position, pt =>
         {
             Actor actorAt = map.GetActorAt(pt);
             if (actorAt == null) return false;
             return Player.CanTradeWith(actorAt);
         });
        case AdvisorHint.NPC_GIVING_ITEM:
          if (Player.Inventory?.IsEmpty ?? true) return false;
          return map.HasAnyAdjacentInMap(position, pt =>
         {
             Actor actorAt = map.GetActorAt(pt);
             if (actorAt == null) return false;
             return !Player.IsEnemyOf(actorAt);
         });
        case AdvisorHint.NPC_SHOUTING:
          return map.HasAnyAdjacentInMap(position, pt =>
         {
             Actor actorAt = map.GetActorAt(pt);
             if (!actorAt?.IsSleeping ?? true) return false;
             return !Player.IsEnemyOf(actorAt);
         });
        case AdvisorHint.BUILD_FORTIFICATION:
          return map.HasAnyAdjacentInMap(position, pt => Player.CanBuildFortification(in pt, false));
        case AdvisorHint.LEADING_NEED_SKILL:
          return map.HasAnyAdjacentInMap(position, pt =>
         {
             Actor actorAt = map.GetActorAt(pt);
             if (actorAt == null) return false;
             return !Player.IsEnemyOf(actorAt);
         });
        case AdvisorHint.LEADING_CAN_RECRUIT:
          return map.HasAnyAdjacentInMap(position, pt =>
         {
             Actor actorAt = map.GetActorAt(pt);
             if (actorAt == null) return false;
             return Player.CanTakeLeadOf(actorAt);
         });
        case AdvisorHint.LEADING_GIVE_ORDERS:
          return Player.CountFollowers > 0;
        case AdvisorHint.LEADING_SWITCH_PLACE:
          return map.HasAnyAdjacentInMap(position, pt =>
         {
             Actor actorAt = map.GetActorAt(pt);
             return (null != actorAt && Player.CanSwitchPlaceWith(actorAt));
         });
        case AdvisorHint.GAME_SAVE_LOAD:
          return map.LocalTime.Hour >= 7;
        case AdvisorHint.CITY_INFORMATION:
          return map.LocalTime.Hour >= 12;
        case AdvisorHint.CORPSE:
          if (!Player.Model.Abilities.IsUndead)
            return map.HasCorpsesAt(position);
          return false;
        case AdvisorHint.CORPSE_EAT:
          if (Player.Model.Abilities.IsUndead)
            return map.HasCorpsesAt(position);
          return false;

        // alpha10 new hints

        case AdvisorHint.SANITY: return  0.80f * Player.MaxSanity > Player.Sanity;
        case AdvisorHint.INFECTION: return 0 < Player.Infection;
        case AdvisorHint.TRAPS: return Player.Has<ItemTrap>();
        default: throw new InvalidOperationException("unhandled hint");
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
            string.Format("You can view and redefine the KEYS by pressing <{0}>.",  s_KeyBindings.AsString(PlayerCommand.KEYBINDING_MODE)),
            string.Format("You can change OPTIONS by pressing <{0}>.",  s_KeyBindings.AsString(PlayerCommand.OPTIONS_MODE)),
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
            string.Format("To TOGGLE RUNNING : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.RUN_TOGGLE))
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
            string.Format("To REST/WAIT : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.WAIT_OR_SELF))
          };
          break;
        case AdvisorHint.MOVE_JUMP:
          title = "MOVEMENT - JUMPING";
          body = new string[6]
          {
            "You can JUMP on or over an obstacle next to you.",
            "Typical jumpable objects are cars, fences and furniture.",
            "The object is described with 'Can be jumped on'.",
            "Some enemies can't jump and won't be able to follow you.",
            "Jumping is tiring and spends stamina.",
            "To jump, just MOVE on the obstacle."
          };
          break;
        case AdvisorHint.ITEM_GRAB_CONTAINER:
          title = "TAKING AN ITEM FROM A CONTAINER";
          body = new string[] {
            "You are next to a container, such as a warbrobe or a shelf.",
            "You can TAKE the item there by MOVING into the object.",
            string.Format("To INITIATE THE TRADE : move the mouse over your item and press <{0}>.",  s_KeyBindings.AsString(PlayerCommand.INITIATE_TRADE)),
            string.Format("To GIVE AN ITEM : move the mouse over your item and press <{0}>.",  s_KeyBindings.AsString(PlayerCommand.GIVE_ITEM))
          };
          break;
        case AdvisorHint.ITEM_GRAB_FLOOR:
          title = "TAKING AN ITEM FROM THE FLOOR";
          body = new string[] {
            "You are standing on a stack of items.",
            "The items are listed on the right panel in the ground inventory.",
            "To TAKE an item, move your mouse on the item on the ground inventory and <LMB>.",
            "Shortcut : <Ctrl-item slot number>."
          };
          break;
        case AdvisorHint.ITEM_UNEQUIP:
          title = "UNEQUIPING AN ITEM";
          body = new string[] {
            "You have equipped an item.",
            "The item is displayed with a green background.",
            "To UNEQUIP the item, <LMB> on it in your inventory.",
            "Shortcut: <Ctrl-item slot number>"
          };
          break;
        case AdvisorHint.ITEM_EQUIP:
          title = "EQUIPING AN ITEM";
          body = new string[] {
            "You have an equipable item in your inventory.",
            "Typical equipable items are weapons, lights and phones.",
            "To EQUIP the item, <LMB> on it in your inventory.",
            "Shortcut : <Ctrl-item slot number>"
          };
          break;
        case AdvisorHint.ITEM_TYPE_BARRICADING:
          title = "ITEM - BARRICADING MATERIAL";
          body = new string[] {
            "You have some barricading materials, such as planks.",
            "Barricading material is used when you barricade doors/windows or build fortifications.",
            "To build fortifications you need the CARPENTRY skill."
          };
          break;
        case AdvisorHint.ITEM_DROP:
          title = "DROPPING AN ITEM";
          body = new string[] {
            "You can drop items from your inventory.",
            "To DROP an item, <RMB> on it.",
            "The item must be unequiped first."
          };
          break;
        case AdvisorHint.ITEM_USE:
          title = "USING AN ITEM";
          body = new string[] {
            "You can use one of your item.",
            "Typical usable items are food, medecine and ammunition.",
            "To USE the item, <LMB> on it in your inventory.",
            "Shortcut: <Ctrl-item slot number>"
          };
          break;
        case AdvisorHint.FLASHLIGHT:
          title = "LIGHTING";
          body = new string[] {
            "You have found a lighting item, such as a flashlight.",
            "Equip the item to increase your view distance (FoV).",
            "Standing next to someone with a light on has the same effect.",
            "You can recharge flashlights at power generators."
          };
          break;
        case AdvisorHint.CELLPHONES:
          title = "CELLPHONES";
          body = new string[] {
            "You have found a cellphone.",
            "Cellphones are useful to keep contact with your follower(s).",
            "You and your follower(s) must have a cellphone equipped.",
            "You can recharge cellphones at power generators."
          };
          break;
        case AdvisorHint.SPRAYS_PAINT:
          title = "SPRAYS - SPRAYPAINT";
          body = new string[] {
            "You have found a can of spraypaint.",
            "You can tag a symbol on walls and floors.",
            "This is useful to mark some places and locations.",
            string.Format("To SPRAY : equip the spray and press <{0}>.", s_KeyBindings.AsString(PlayerCommand.USE_SPRAY))
          };
          break;
        case AdvisorHint.SPRAYS_SCENT:
          title = "SPRAYS - SCENT SPRAY";
          body = new string[] {
            "You have found a scent spray.",
            "You can spray some perfurme on yourself or another adjacent actor.",
            "This is useful to confuse the undeads because they hunt using their smell.",
            string.Format("To SPRAY : equip the spray and press <{0}>.", s_KeyBindings.AsString(PlayerCommand.USE_SPRAY))
          };
          break;
        case AdvisorHint.WEAPON_FIRE:
          title = "FIRING A WEAPON";
          body = new string[] {
            "You can fire your equiped ranged weapon.",
            "You need to have valid targets.",
            "To fire on a target you need ammunitions and a clear line of fine.",
            "The target must be within the weapon range.",
            "The closer the target is, the easier it is to hit and it does slightly more damage.",
            string.Format("To FIRE : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.FIRE_MODE)),
            "When firing you can switch to rapid fire mode : you will shoot twice but at reduced accurary.",
            "Remember you need to have visible enemies to fire at.",
            "Read the manual for more explanation about firing and ranged weapons."
          };
          break;
        case AdvisorHint.WEAPON_RELOAD:
          title = "RELOADING A WEAPON";
          body = new string[] {
            "You can reload your equiped ranged weapon.",
            "To RELOAD, just USE a compatible ammo item."
          };
          break;
        case AdvisorHint.GRENADE:
          title = "GRENADES";
          body = new string[] {
            "You have found a grenade.",
            "To THROW a GRENADE, EQUIP it and FIRE it.",
            string.Format("To FIRE : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.FIRE_MODE))
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
            string.Format("To CLOSE : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.CLOSE_DOOR))
          };
          break;
        case AdvisorHint.OBJECT_PUSH:
          title = "PUSHING/PULLING OBJECTS";
          body = new string[] {
            "You can PUSH/PULL an OBJECT around you.",
            "Only MOVABLE objects can be pushed/pulled.",
            "Movable objects will be described as 'Can be moved'",
            "You can also PUSH/PULL ACTORS around you.",
            string.Format("To PUSH : <{0}>.", s_KeyBindings.AsString(PlayerCommand.PUSH_MODE)),
            string.Format("To PULL : <{0}>.", s_KeyBindings.AsString(PlayerCommand.PULL_MODE))
          };
          break;
        case AdvisorHint.OBJECT_BREAK:
          title = "BREAKING OBJECTS";
          body = new string[3]
          {
            "You can try to BREAK an object around you.",
            "Typical breakable objects are furnitures, doors and windows.",
            string.Format("To BREAK : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.BREAK_MODE))
          };
          break;
        case AdvisorHint.BARRICADE:
          title = "BARRICADING A DOOR/WINDOW";
          body = new string[3]
          {
            "You can barricade an adjacent door or window.",
            "Barricading uses material such as planks.",
            string.Format("To BARRICADE : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.BARRICADE_MODE))
          };
          break;
        case AdvisorHint.EXIT_STAIRS_LADDERS:
          title = "USING STAIRS & LADDERS";
          body = new string[3]
          {
            "You are standing on stairs or a ladder.",
            "You can use this exit to go on another map.",
            string.Format("To USE THE EXIT : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.USE_EXIT))
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
            string.Format("To SLEEP : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.SLEEP)),
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
            string.Format("To INITIATE THE TRADE : move the mouse over your item and press <{0}>.",  s_KeyBindings.AsString(PlayerCommand.INITIATE_TRADE))
          };
          break;
        case AdvisorHint.NPC_GIVING_ITEM:
          title = "GIVING ITEMS";
          body = new string[2]
          {
            "You can GIVE ITEMS to other actors.",
            string.Format("To GIVE AN ITEM : move the mouse over your item and press <{0}>.",  s_KeyBindings.AsString(PlayerCommand.GIVE_ITEM))
          };
          break;
        case AdvisorHint.NPC_SHOUTING:
          title = "SHOUTING";
          body = new string[4]
          {
            "Someone is sleeping near you.",
            "You can SHOUT to try to wake him or her up.",
            "Other actors can also shout to wake their friends up when they see danger.",
            string.Format("To SHOUT : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.SHOUT))
          };
          break;
        case AdvisorHint.BUILD_FORTIFICATION:
          title = "BUILDING FORTIFICATIONS";
          body = new string[4]
          {
            "You can now build fortifications thanks to the carpentry skill.",
            "You need enough barricading materials.",
            string.Format("To BUILD SMALL FORTIFICATIONS : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.BUILD_SMALL_FORTIFICATION)),
            string.Format("To BUILD LARGE FORTIFICATIONS : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.BUILD_LARGE_FORTIFICATION))
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
            string.Format("To RECRUIT : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.LEAD_MODE))
          };
          break;
        case AdvisorHint.LEADING_GIVE_ORDERS:
          title = "LEADING - GIVING ORDERS";
          body = new string[4]
          {
            "You can give orders and directives to your follower.",
            "You can also fire your followers.",
            string.Format("To GIVE ORDERS : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.ORDER_MODE)),
            string.Format("To FIRE YOUR FOLLOWER : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.LEAD_MODE))
          };
          break;
        case AdvisorHint.LEADING_SWITCH_PLACE:
          title = "LEADING - SWITCHING PLACE";
          body = new string[2]
          {
            "You can switch place with followers next to you.",
            string.Format("To SWITCH PLACE : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.SWITCH_PLACE))
          };
          break;
        case AdvisorHint.GAME_SAVE_LOAD:
          title = "SAVING AND LOADING GAME";
          body = new string[7] {
            "Now could be a good time to save your game.",
            "You can have only one save game active.",
            string.Format("To SAVE THE GAME : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.SAVE_GAME)),
            string.Format("To LOAD THE GAME : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.LOAD_GAME)),
            "You can also load the game from the main menu.",
            "Saving or loading can take a bit of time, please be patient.",
            "Or consider turning some game options to lower settings."
          };
          break;
        case AdvisorHint.CITY_INFORMATION:
          title = "CITY INFORMATION";
          body = new string[3] {
            "You know the layout of your town.",
            "You aso know the most notable locations.",
            string.Format("To VIEW THE CITY INFORMATION : <{0}>.",  s_KeyBindings.AsString(PlayerCommand.CITY_INFO))
          };
          break;
        case AdvisorHint.CORPSE:
          title = "CORPSES";
          body = new string[] {
            "You are standing on a CORPSE.",
            "Corpses will slowly rot away but may resurrect as zombies.",
            "You can BUTCHER a corpse as a way to prevent that.",
            "You can also DRAG corpses to move them.",
            "You can try to REVIVE corpses if you have the medic skill and a medikit.",
            "If you are desperate and starving you can resort to cannibalism by EATING corpses.",
            "To act, hover the mouse on it in the corpse list and...",
            "TO BUTCHER the CORPSE : <RMB>",
            "TO DRAG the CORPSE : <LMB>",
            string.Format("TO REVIVE the CORPSE : <{0}>", s_KeyBindings.AsString(PlayerCommand.REVIVE_CORPSE)),
            string.Format("TO EAT the CORPSE : <{0}>", s_KeyBindings.AsString(PlayerCommand.EAT_CORPSE))
          };
          break;
        case AdvisorHint.CORPSE_EAT:
          title = "EATING CORPSES";
          body = new string[] {
            "You can eat a corpse to regain health.",
            "TO EAT A CORPSE : <RMB> on it in the corpse list."
          };
          break;
        // alpha10 new hints
        case AdvisorHint.SANITY:  // sanity
          title = "SANITY";
          body = new string[] {
            "You should care about your SANITY.",
            "If it gets too low, you can go insane.",
            "Living in this horrible world and seing horrible things will lower your sanity.",
            "You can recover sanity by :",
            "- Talking to people.",
            "- Having followers you trust.",
            "- Killing undeads.",
            "- Using entertainment items.",
            "- Taking pills."
          };
          break;
        case AdvisorHint.INFECTION:
          title = "INFECTION";
          body = new string[] {
            "You are INFECTED!",
            "Most undeads bites are infectious.",
            "A low infection value will make you sick.",
            "A full infection value is death.",
            "Infection only worsen when you are biten.",
            "Cure the infection with appropriate meds."
          };
          break;
        case AdvisorHint.TRAPS:
          title = "TRAPS";
          body = new string[] {
            "You are carrying TRAPS.",
            "Traps are a good way to protect places.",
            "Drop activated traps on tiles.",
            "Some traps are activated by dropping them.",
            "Other traps need to be activated before being dropped.",
            "You are always safe from your own traps.",
            "Traps layed by your followers are also safe."
          };
          break;
        default: throw new InvalidOperationException("unhandled hint");
      }
    }

    private void ShowAdvisorHint(AdvisorHint hint)
    {
      GetAdvisorHintText(hint, out string title, out string[] body);
      ShowAdvisorMessage(title, body);
    }

    private void ShowAdvisorMessage(string title, string[] lines)
    {
      var pc = Player.Controller as PlayerController ?? throw new ArgumentNullException("Player.Controller");

      string[] lines1 = new string[lines.Length + 2];
      lines1[0] = "HINT : " + title;
      Array.Copy(lines, 0, lines1, 1, lines.Length);
      lines1[lines.Length + 1] = string.Format("(hint {0}/{1})", s_Hints.CountAdvisorHintsGiven(), (int)AdvisorHint._COUNT);

      int turn = Session.Get.WorldTime.TurnCounter;
      List<UI.Message> msgs = new() {
        new("You can disable the advisor in the options screen.", turn, Color.White),
        new(string.Format("To show the options screen : <{0}>.", s_KeyBindings.AsString(PlayerCommand.OPTIONS_MODE)), turn, Color.White)
      };

      ClearOverlays();
      AddOverlay(new OverlayPopup(lines1, Color.White, Color.White, Color.Black, GDI_Point.Empty));
      pc.AddMessagesForceRead(msgs);

      pc.Messages.Clear();
      ClearOverlays();
      RedrawPlayScreen();
    }

#nullable enable
    private Direction? WaitDirectionOrCancel()
    {
        return m_UI.WaitEscape(key => CommandToDirection(InputTranslator.KeyToCommand(key)));
    }
#nullable restore

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

    private const int EXTENDED_CHOICE_UB = 36;
    static private int KeyToExtendedChoiceNumber(Keys key)
    {
      if (Keys.A <= key && Keys.Z >=key) return 10 + (key - Keys.A);
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

    static private char ExtendedChoiceNumberToChar(int src)
    {
      if (10 <= src && 35 >= src) return (char)((int)'A' + (src - 10));
      switch (src)
      {
        case 0: return '0';
        case 1: return '1';
        case 2: return '2';
        case 3: return '3';
        case 4: return '4';
        case 5: return '5';
        case 6: return '6';
        case 7: return '7';
        case 8: return '8';
        case 9: return '9';
#if DEBUG
        default: throw new ArgumentOutOfRangeException(nameof(src),src.ToString());
#else
        default: return ' ';
#endif
      }
    }

    static private string[] DescribeStuffAt(Location examine)
    {
#if DEBUG
      if (!Map.Canonical(ref examine)) throw new InvalidOperationException("tracing unvisualizable");
#endif

      Actor actorAt = examine.Actor;
      if (actorAt != null) return DescribeActor(actorAt);
      var mapObjectAt = examine.MapObject;
      if (mapObjectAt != null) return DescribeMapObject(mapObjectAt);
      var itemsAt = examine.Items;
      if (itemsAt != null) return DescribeInventory(itemsAt);
      var corpsesAt = examine.Corpses;
      return corpsesAt?.Describe();
    }

    // unclear where this goes; parking here as this is UI and I'd like code locality
    static private string TrustInLeaderText(int trust)
    {
      if (Actor.TRUST_BOND_THRESHOLD <= trust) return "Trust : BOND.";
      else if (Rules.TRUST_MAX <= trust) return "Trust : MAX.";
      else return string.Format("Trust : {0}/T:{1}-B:{2}.", trust, Actor.TRUST_TRUSTING_THRESHOLD, Rules.TRUST_MAX);
    }

    static private string[] DescribeActor(Actor actor)
    {
      var lines = new List<string>(10);
      var a_name = actor.Name.Capitalize();
      var a_faction = actor.Faction;
      if (null != a_faction) {
        lines.Add(actor.IsInAGang ? string.Format("{0}, {1}-{2}.", a_name, a_faction.MemberName, actor.GangID.Name())   // 2020-01-15 IL ok with explicit a_faction.MemberName
                                  : string.Format("{0}, {1}.", a_name, a_faction.MemberName));
      } else lines.Add(string.Format("{0}.", a_name));

      var a_model = actor.Model;
      lines.Add(string.Format("{0}.", a_model.Name.Capitalize()));
      lines.Add(string.Format("{0} since {1}.", a_model.Abilities.IsUndead ? "Undead" : "Staying alive", new WorldTime(actor.SpawnTime).ToString()));
      OrderableAI aiController = actor.Controller as OrderableAI;
      if (aiController?.Order != null) lines.Add(string.Format("Order : {0}.", aiController.Order.ToString())); // 2020-01-15 IL doesn't want temporary for aiController?.Order
      var leader = actor.LiveLeader;
      if (null != leader) {
        lines.Add(string.Format("Leader : {0}.", leader.Name.Capitalize()));
        if (leader.IsPlayer) {  // \todo should this test be leader == Player ?
          lines.Add(TrustInLeaderText(actor.TrustInLeader));

          if (null != aiController && aiController.DontFollowLeader) lines.Add("Ordered to not follow you.");
          lines.Add(string.Format("Foo : {0} {1}h", actor.FoodPoints, actor.HoursUntilHungry));
          lines.Add(string.Format("Slp : {0} {1}h", actor.SleepPoints, actor.HoursUntilSleepy));
          lines.Add(string.Format("San : {0} {1}h", actor.Sanity, actor.HoursUntilUnstable));
          lines.Add(string.Format("Inf : {0} {1}%", actor.Infection, actor.InfectionPercent));
        }
      }
      int murders = actor.MurdersOnRecord(Player);
      if (Player.Model.Abilities.IsLawEnforcer && 0 < murders) {
        if (0 < murders) {
          lines.Add("WANTED FOR MURDER!");
          lines.Add(string.Format("{0}!", "murder".QtyDesc(murders)));
        }
        if (!actor.IgnoredByPolice(Player)) {
          var blotter = Session.Get.Police.WantedFor(actor);
          if (null!=blotter) {
            lines.Add("WANTED:");
            foreach(var x in blotter) {
                switch(x.v_code) {
                case 1: lines.Add("death of officer: "+x.direct_object.Name);
                    break;
                case 2: lines.Add("assault of officer: "+x.direct_object.Name);
                    break;
                case 3: lines.Add("murder: "+x.direct_object.Name);
                    break;
                case 4: lines.Add("assault: "+x.direct_object.Name);
                    break;
                }
            }
          }
        }
      } else if (null != leader && leader.IsPlayer && actor.IsTrustingLeader) {
        lines.Add(0 < (murders = Session.Get.Police.CountMurders(actor))
                     ? string.Format("* Confess {0}! *", "murder".QtyDesc(murders))
                     : "Has committed no murders.");
        lines.Add(0 < (murders = Session.Get.Police.CountAssaults(actor))
                     ? string.Format("* Confess {0}! *", "assault".QtyDesc(murders))
                     : "Has committed no assaults.");
      }
      if (actor.IsAggressorOf(Player)) lines.Add("Aggressed you.");
      if (Player.IsSelfDefenceFrom(actor)) lines.Add(string.Format("You can kill {0} in self-defence.", actor.HimOrHer));
      if (Player.IsAggressorOf(actor)) lines.Add(string.Format("You aggressed {0}.", actor.HimOrHer));
      if (actor.IsSelfDefenceFrom(Player)) lines.Add("Killing you would be self-defence.");
      if (!Player.Faction.IsEnemyOf(a_faction) && Player.AreIndirectEnemies(actor)) lines.Add("You are enemies through groups.");   // RS Alpha 10 tests against Rules::AreGroupEnemies
#if POLICE_NO_QUESTIONS_ASKED
      if (Player.Model.Abilities.IsLawEnforcer && Player.Threats.IsThreat(actor)) {
        stringList.Add("Is wanted for unspecified violent crimes.");
      }
#endif
      lines.Add("");
      lines.Add(DescribeActorActivity(actor) ?? " ");
      if (a_model.Abilities.HasToSleep) {
        if (actor.IsExhausted) lines.Add("Exhausted!");
        else if (actor.IsSleepy) lines.Add("Sleepy.");
      }
      if (a_model.Abilities.HasToEat) {
        if (actor.IsStarving) lines.Add("Starving!");
        else if (actor.IsHungry) lines.Add("Hungry.");
      }
      else if (a_model.Abilities.IsRotting) {
        if (actor.IsRotStarving) lines.Add("Starving!");
        else if (actor.IsRotHungry) lines.Add("Hungry.");
      }
      if (a_model.Abilities.HasSanity) {
        if (actor.IsInsane) lines.Add("Insane!");
        else if (actor.IsDisturbed) lines.Add("Disturbed.");
      }

      if (Player.IsEnemyOf(actor)) {
      var a_defense_dist = Rules.SkillProbabilityDistribution(actor.Defence.Value);
      float melee_p_hit = a_defense_dist.LessThan(Rules.SkillProbabilityDistribution(Player.MeleeAttack(actor).HitValue));
      lines.Add("% hit: " + melee_p_hit);   // 2020-01-20 IL optimizer is eliminating this temporary
      var p_defense_dist = Rules.SkillProbabilityDistribution(Player.Defence.Value);
      float melee_a_hit = p_defense_dist.LessThan(Rules.SkillProbabilityDistribution(actor.MeleeAttack(Player).HitValue));
      lines.Add("% be hit: " + melee_a_hit);
      if (0<Player.CurrentRangedAttack.Range) {
        float ranged_p_hit = a_defense_dist.LessThan(Rules.SkillProbabilityDistribution(Player.RangedAttack(Rules.InteractionDistance(Player.Location, actor.Location), actor).HitValue));
        lines.Add("% shot: " + ranged_p_hit);
      }
      if (0<actor.CurrentRangedAttack.Range) {
        float ranged_a_hit = p_defense_dist.LessThan(Rules.SkillProbabilityDistribution(actor.RangedAttack(Rules.InteractionDistance(Player.Location, actor.Location), Player).HitValue));
        lines.Add("% be shot: " + ranged_a_hit);
      }
      } // m_Player.IsEnemyOf(actor)

      // main stat block
      lines.Add(string.Format("Spd : {0:F2}", actor.Speed / BASE_SPEED));
      var stringBuilder = new StringBuilder();
      int max;
      int tmp_i;
      stringBuilder.Append((tmp_i = actor.HitPoints) != (max = actor.MaxHPs)
                         ? string.Format("HP  : {0:D2}/{1:D2}", tmp_i, max)
                         : string.Format("HP  : {0:D2} MAX", tmp_i));
      if (a_model.Abilities.CanTire) {
        stringBuilder.Append((tmp_i = actor.StaminaPoints) != (max = actor.MaxSTA)
                           ? string.Format("   STA : {0}/{1}", tmp_i, max)
                           : string.Format("   STA : {0} MAX", tmp_i));
      }
      lines.Add(stringBuilder.ToString());
      Attack attack = actor.MeleeAttack();
      lines.Add(string.Format("Atk : {0:D2} Dmg : {1:D2}", attack.HitValue, attack.DamageValue));
      Defence defence = actor.Defence;
      lines.Add(string.Format("Def : {0:D2}", defence.Value));
      lines.Add(string.Format("Arm : {0}/{1}", defence.Protection_Hit, defence.Protection_Shot));
      lines.Add(" ");
      lines.Add(a_model.FlavorDescription);
      lines.Add(" ");
      var sk_table = actor.MySkills;
      var skills = sk_table.Skills;
      if (null != skills) {
        foreach (var sk in skills) lines.Add(string.Format("{0}-{1}", sk.Value, Skills.Name(sk.Key)));
        lines.Add(" ");
      }

      // alpha10
      // 8. Unusual abilities
      // unusual abilities for undeads
      if (a_model.Abilities.IsUndead) {
        // fov
        lines.Add(string.Format("- FOV : {0}.", a_model.BaseViewRange));

        // smell rating
        int smell = (int)(100 * actor.Smell);  // applies z-tracker skill
        lines.Add(smell == 0 ? "- Has no sense of smell." :
                  smell < 50 ? "- Has poor sense of smell." :
                  smell < 100 ? "- Has good sense of smell." :
                                "- Has excellent sense of smell.");

        // grab?
        if (0 < sk_table.GetSkillLevel(Skills.IDs.Z_GRAB)) lines.Add("- Z-Grab : this undead can grab its victims.");

        if (a_model.Abilities.IsUndeadMaster) lines.Add("- Other undeads follow this undead tracks.");
        else if (smell > 0) lines.Add("- This undead will follow zombie masters tracks.");
        if (a_model.Abilities.IsIntelligent) lines.Add("- This undead is intelligent.");
        if (a_model.Abilities.CanDisarm) lines.Add("- This undead can disarm.");
        if (a_model.Abilities.CanJump) {
          lines.Add(actor.Model.Abilities.CanJumpStumble ? "- This undead can jump but may stumble."
                                                         : "- This undead can jump.");
        }
        if (actor.AbleToPush) lines.Add("- This undead can push.");
        if (a_model.Abilities.ZombieAI_Explore) lines.Add("- This undead will explore.");

        // things some of them cannot do
        if (!a_model.Abilities.IsRotting) lines.Add("- This undead will not rot.");
        if (!a_model.Abilities.CanBashDoors) lines.Add("- This undead cannot bash doors.");
        if (!a_model.Abilities.CanBreakObjects) lines.Add("- This undead cannot break objects.");
        if (!a_model.Abilities.CanZombifyKilled) lines.Add("- This undead cannot infect livings.");
        if (!a_model.Abilities.AI_CanUseAIExits) lines.Add("- This undead live in this map.");
      }
      // misc unusual abilities
      if (a_model.Abilities.IsLawEnforcer) lines.Add("- Is a law enforcer.");
      if (a_model.Abilities.IsSmall) lines.Add("- Is small and can sneak through things.");

      // 9. Inventory.
      var inv = actor.Inventory;
      if (null != inv && !inv.IsEmpty) {
        lines.Add(string.Format("Items {0}/{1} : ", inv.CountItems, actor.MaxInv));
        lines.AddRange(DescribeInventory(inv));
      }

#if DEBUG
      // 10. Trading options; cf. ObjectiveAI::HaveTradingTargets()
      var TradeableItems = (Player.Controller as ObjectiveAI)?.GetTradeableItems();
      var order_ai = actor.Controller as OrderableAI;
      bool trade_ok = null!=order_ai && null != TradeableItems && 0 < TradeableItems.Count && !actor.IsPlayer && Player.CanTradeWith(actor) && null != Player.MinStepPathTo(Player.Location, actor.Location);
      if (trade_ok && 1== TradeableItems.Count) {
        var other_TradeableItems = order_ai.GetTradeableItems();
        if (null == other_TradeableItems) trade_ok = false;
        else if (1 == other_TradeableItems.Count && TradeableItems[0].ModelID == other_TradeableItems[0].ModelID) trade_ok = false;
      }
      if (trade_ok && order_ai.HasAnyInterestingItem(TradeableItems)) {
        // Cf. RogueGame::PickItemsToTrade
        var negotiate = order_ai.TradeOptions(Player);
        if (null != negotiate) {
              lines.Add("would trade with you:");
              while(0 < negotiate.Count) {
                var test = negotiate[0];
                var viewpoint_offer = negotiate.FindAll(it => it.Value==test.Value);
                var viewpoint_ask = negotiate.FindAll(it => it.Key==test.Key);
                if (viewpoint_ask.Count<=viewpoint_offer.Count) {
                  string msg = " asks " + test.Value + " for ";
                  if (1==viewpoint_offer.Count) msg += test.Key;
                  else {
                    while(1<viewpoint_offer.Count) {
                      msg += viewpoint_offer[viewpoint_offer.Count-1].Key +", ";
                      viewpoint_offer.RemoveAt(viewpoint_offer.Count - 1);
                      if (80<msg.Length) {
                        lines.Add(msg);
                        msg = "  ";
                      }
                    }
                    msg += "or "+viewpoint_offer[0].Key;
                  }
                  lines.Add(msg);
                  negotiate = negotiate.FindAll(it => it.Value != test.Value);
                } else {
                  string msg = " offers " + test.Key + " for ";
                  if (1==viewpoint_ask.Count) msg += test.Value;  // dead code
                  else {
                    while(1< viewpoint_ask.Count) {
                      msg += viewpoint_ask[viewpoint_ask.Count-1].Value +", ";
                      viewpoint_ask.RemoveAt(viewpoint_ask.Count - 1);
                      if (80<msg.Length) {
                        lines.Add(msg);
                        msg = "  ";
                      }
                    }
                    msg += "or "+ viewpoint_ask[0].Value;
                  }
                  lines.Add(msg);
                  negotiate = negotiate.FindAll(it => it.Key != test.Key);
                }
              }
            }
      }
#endif

      return lines.ToArray();
    }

#nullable enable
    static private string? DescribeActorActivity(Actor actor)
    {
      if (actor.IsPlayer) return null;
      switch (actor.Activity) {
        case Data.Activity.IDLE: return null;
        case Data.Activity.CHASING:
          if (actor.TargetActor == null)
            return "Chasing!";
          return string.Format("Chasing {0}!", actor.TargetActor.Name);
        case djack.RogueSurvivor.Data.Activity.FIGHTING:
          if (actor.TargetActor == null)
            return "Fighting!";
          return string.Format("Fighting {0}!", actor.TargetActor.Name);
        case Data.Activity.TRACKING: return "Tracking!";
        case Data.Activity.FLEEING: return "Fleeing!";
        case Data.Activity.FOLLOWING:
          if (actor.TargetActor == null) return "Following.";
          // alpha10
          if (actor.Leader == actor.TargetActor) return string.Format("Following {0} leader.", actor.HisOrHer);
          return string.Format("Following {0}.", actor.TargetActor.Name);
        case Data.Activity.SLEEPING: return "Sleeping.";
        case Data.Activity.FOLLOWING_ORDER: return "Following orders.";
        case Data.Activity.FLEEING_FROM_EXPLOSIVE: return "Fleeing from explosives!";
        default:
#if DEBUG
          throw new ArgumentException("unhandled activity " + actor.Activity);
#else
          return null;
#endif
      }
    }

    static private string DescribePlayerFollowerStatus(Actor follower)
    {
      string desc = string.Format("(trust:{0})", follower.TrustInLeader);
      if (follower.Controller is OrderableAI ai) {
        return (ai.Order?.ToString() ?? "(no orders)") + desc;
      } else {
        return "(is player)" + desc;
      }
    }

    static private string[] DescribeMapObject(MapObject obj)
    {
      var lines = new List<string>(4) { string.Format("{0}.", obj.AName) };
      if (obj.IsJumpable) lines.Add("Can be jumped on.");
      if (obj.IsCouch) lines.Add("Is a couch.");
      if (obj.GivesWood) lines.Add("Can be dismantled for wood.");
      if (obj.IsMovable) lines.Add("Can be moved.");
      if (obj.StandOnFovBonus) lines.Add("Increases view range.");
      var stringBuilder = new StringBuilder();
      if (obj.BreakState == MapObject.Break.BROKEN) stringBuilder.Append("Broken! ");
      if (obj.FireState == MapObject.Fire.ONFIRE) stringBuilder.Append("On fire! ");
      else if (obj.FireState == MapObject.Fire.ASHES) stringBuilder.Append("Burnt to ashes! ");
      lines.Add(stringBuilder.ToString());
      if (obj is PowerGenerator power) {
        lines.Add(power.IsOn ? "Currently ON." : "Currently OFF.");
        lines.Add(string.Format("The power gauge reads {0}%.", (int)(100.0 * obj.Location.Map.PowerRatio)));
      } else if (obj is Board bb) {
        lines.Add("The text reads : ");
        lines.AddRange(bb.Text);
      }
      int tmp_i = obj.MaxHitPoints;
      if (tmp_i > 0) {
        int tmp_hp;
        lines.Add((tmp_hp = obj.HitPoints) < tmp_i
                 ? string.Format("HP        : {0}/{1}", tmp_hp, tmp_i)
                 : string.Format("HP        : {0} MAX", tmp_hp));
        if (obj is DoorWindow doorWindow) {
          lines.Add((tmp_hp = doorWindow.BarricadePoints) < Rules.BARRICADING_MAX
                   ? string.Format("Barricades: {0}/{1}", tmp_hp, Rules.BARRICADING_MAX)
                   : string.Format("Barricades: {0} MAX", tmp_hp));
        }
      }
      if (0 < (tmp_i = obj.Weight)) lines.Add(string.Format("Weight    : {0}", tmp_i));
      var itemsAt = (obj as ShelfLike)?.NonEmptyInventory;
      if (null != itemsAt) lines.AddRange(DescribeInventory(itemsAt));
      itemsAt = obj.Location.Items;
      if (itemsAt != null) lines.AddRange(DescribeInventory(itemsAt));
      return lines.ToArray();
    }

    static private string[] DescribeInventory(Inventory inv)
    {
      var lines = new string[inv.CountItems];
      int n = 0;
      foreach(var it in inv.Items) lines[n++] = string.Format(it.IsEquipped ? "- {0} (equipped)"
                                                                            : "- {0}", DescribeItemShort(it));
      return lines;
    }

	// UI functions ... do not belong in Corpse class for now
    static private string[] DescribeCorpseLong(Corpse c, bool isInPlayerTile)
    {
	  static string DescribeCorpseLong_DescInfectionPercent(int num) {
	    return num != 0 ? (num >= 5 ? (num >= 15 ? (num >= 30 ? (num >= 55 ? (num >= 70 ? (num >= 99 ? "7/7 - total" : "6/7 - great") : "5/7 - important") : "4/7 - average") : "3/7 - low") : "2/7 - minor") : "1/7 - traces") : "0/7 - none";
	  }

 	  static string DescribeCorpseLong_DescRiseProbability(int num) {
	    return num >= 5 ? (num >= 20 ? (num >= 40 ? (num >= 60 ? (num >= 80 ? (num >= 99 ? "6/6 - certain" : "5/6 - most likely") : "4/6 - very likely") : "3/6 - likely") : "2/6 - possible") : "1/6 - unlikely") : "0/6 - extremely unlikely";
	  }

	  static string DescribeCorpseLong_DescRotLevel(int num) {
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
          throw new ArgumentOutOfRangeException(nameof(num),num,"must be in 0..5");
#else
		  return "The corpse is infested with software bugs.";
#endif
        }
	  }

	  static string DescribeCorpseLong_DescReviveChance(int num) {
		 return num != 0 ? (num >= 5 ? (num >= 20 ? (num >= 40 ? (num >= 60 ? (num >= 80 ? (num >= 99 ? "6/6 - certain" : "5/6 - most likely") : "4/6 - very likely") : "3/6 - likely") : "2/6 - possible") : "1/6 - unlikely") : "0/6 - extremely unlikely") : "impossible";
	  }

      var skills = Player.MySkills;
      int skillLevel = skills.GetSkillLevel(Skills.IDs.NECROLOGY);
      var lines = new List<string>(10){
        c.ToString().Capitalize()+".",
        " ",
        string.Format("Death     : {0}.", (skillLevel > 0 ? WorldTime.MakeTimeDurationMessage(Session.Get.WorldTime.TurnCounter - c.Turn) : "???")),
        string.Format("Infection : {0}.", (skillLevel >= Rules.SKILL_NECROLOGY_LEVEL_FOR_INFECTION ? DescribeCorpseLong_DescInfectionPercent(c.DeadGuy.InfectionPercent) : "???")),
        string.Format("Rise      : {0}.", (skillLevel >= Rules.SKILL_NECROLOGY_LEVEL_FOR_RISE ? DescribeCorpseLong_DescRiseProbability(2 * c.ZombifyChance(c.DeadGuy.Location.Map.LocalTime, false)) : "???")),
        " ",
	    DescribeCorpseLong_DescRotLevel(c.RotLevel),
        string.Format("Revive    : {0}.", (skills.GetSkillLevel(Skills.IDs.MEDIC) >= Rules.SKILL_MEDIC_LEVEL_FOR_REVIVE_EST ? DescribeCorpseLong_DescReviveChance(Player.ReviveChance(c)) : "???"))
      };
      if (isInPlayerTile) {
        lines.Add(" ");
        lines.Add("----");
        lines.Add("LBM to start/stop dragging.");
        bool is_undead = Player.Model.Abilities.IsUndead;
        lines.Add(string.Format("RBM to {0}.", is_undead ? "eat" : "butcher"));
        if (!is_undead) {
          lines.Add(string.Format("to eat: <{0}>", s_KeyBindings.AsString(PlayerCommand.EAT_CORPSE)));
          lines.Add(string.Format("to revive : <{0}>", s_KeyBindings.AsString(PlayerCommand.REVIVE_CORPSE)));
        }
      }
      return lines.ToArray();
    }

    static private string DescribeItemShort(Item it)
    {
      string str = it.Quantity > 1 ? it.Model.PluralName : it.AName;
      if (it is ItemFood food) {
        if (food.IsSpoiledAt(Session.Get.WorldTime.TurnCounter)) str += " (spoiled)";
        else if (food.IsExpiredAt(Session.Get.WorldTime.TurnCounter)) str += " (expired)";
      } else if (it is ItemRangedWeapon rw) {
        str += string.Format(" ({0}/{1})", rw.Ammo, rw.Model.MaxAmmo);
      } else if (it is ItemTrap trap) {
        if (trap.IsActivated) str += "(activated)";
        if (trap.IsTriggered) str += "(triggered)";
        if (trap.Owner == Player) str += "(yours)";  // alpha10
        if (trap.WouldLearnHowToBypass(Player)) str += "(need mentoring)";
        if (trap.IsSafeFor(Player)) str += "(safe)";
      }
      return (1 < it.Quantity) ? string.Format("{0} {1}", it.Quantity, str) : str;
    }

    static private string[] DescribeItemLong(Item it, bool isPlayerInventory)
    {
      List<string> lines = new();
      List<string> command_lines = new(); // these work on non-player inventories as well
      var model = it.Model;
      lines.Add(model.IsStackable ? string.Format("{0} {1}/{2}", DescribeItemShort(it), it.Quantity, model.StackingLimit)
                                  : DescribeItemShort(it));
      if (model.IsUnbreakable) lines.Add("Unbreakable.");
      string? inInvAdditionalDesc = null;
      if (it is ItemWeapon w) {
        lines.AddRange(DescribeItemWeapon(w));
        if (it is ItemRangedWeapon) {
          inInvAdditionalDesc = string.Format("to fire : <{0}>.", s_KeyBindings.AsString(PlayerCommand.FIRE_MODE));
          command_lines.Add(string.Format("to unload : <{0}>.", s_KeyBindings.AsString(PlayerCommand.UNLOAD)));
        }
      }
      else if (it is ItemFood food) lines.AddRange(DescribeItemFood(food));
      else if (it is ItemMedicine med) lines.AddRange(DescribeItemMedicine(med));
      else if (it is ItemBarricadeMaterial bar) {
        lines.AddRange(DescribeItemBarricadeMaterial(bar));
        inInvAdditionalDesc = string.Format("to build : <{0}>/<{1}>/<{2}>.", s_KeyBindings.AsString(PlayerCommand.BARRICADE_MODE), s_KeyBindings.AsString(PlayerCommand.BUILD_SMALL_FORTIFICATION), s_KeyBindings.AsString(PlayerCommand.BUILD_LARGE_FORTIFICATION));
      } else if (it is ItemBodyArmor armor) lines.AddRange(DescribeItemBodyArmor(armor));
      else if (it is ItemSprayPaint spray) {
        lines.AddRange(DescribeItemSprayPaint(spray));
        inInvAdditionalDesc = string.Format("to spray : <{0}>.", s_KeyBindings.AsString(PlayerCommand.USE_SPRAY));
      } else if (it is ItemSprayScent sscent) {
        lines.AddRange(DescribeItemSprayScent(sscent));
        inInvAdditionalDesc = string.Format("to spray : <{0}>.", s_KeyBindings.AsString(PlayerCommand.USE_SPRAY));
      } else if (it is ItemLight light) lines.AddRange(DescribeItemLight(light));
      else if (it is ItemTracker track) lines.AddRange(DescribeItemTracker(track));
      else if (it is ItemAmmo ammo) {
        lines.AddRange(DescribeItemAmmo(ammo));
        inInvAdditionalDesc = "to reload : left-click.";
      } else if (it is ItemExplosive ex) {
        lines.AddRange(DescribeItemExplosive(ex));
        inInvAdditionalDesc = string.Format("to throw : <{0}>.", s_KeyBindings.AsString(PlayerCommand.FIRE_MODE));
      } else if (it is ItemTrap trap) {
        lines.AddRange(DescribeItemTrap(trap));
        inInvAdditionalDesc = (trap.Model.ActivatesWhenDropped ? "to activate trap : drop it" : "to activate trap : use it");   // alpha10
      } else if (it is ItemEntertainment ent) lines.AddRange(DescribeItemEntertainment(ent));
      lines.Add(" ");
      lines.Add(model.FlavorDescription);
      if (isPlayerInventory) {
        lines.Add(" ");
        lines.Add("----");
        lines.Add(string.Format("to give : <{0}>.", s_KeyBindings.AsString(PlayerCommand.GIVE_ITEM)));
        lines.Add(string.Format("to trade : <{0}>.", s_KeyBindings.AsString(PlayerCommand.INITIATE_TRADE)));
        if (inInvAdditionalDesc != null) lines.Add(inInvAdditionalDesc);
        if (0 < command_lines.Count) lines.AddRange(command_lines);
      } else if (0 < command_lines.Count) {
        lines.Add(" ");
        lines.Add("----");
        lines.AddRange(command_lines);
      }
      return lines.ToArray();
    }

    static private List<string> DescribeItemExplosive(ItemExplosive ex)
    {
      ItemExplosiveModel itemExplosiveModel = ex.Model;
      var lines = new List<string>{ "> explosive" };
      ref readonly var blast = ref itemExplosiveModel.BlastAttack;
      if (blast.CanDamageObjects) lines.Add("Can damage objects.");
      if (blast.CanDestroyWalls) lines.Add("Can destroy walls.");
      var primed = ex as ItemPrimedExplosive;
      lines.Add((null != primed) ? string.Format("Fuse          : {0} turn(s) left!", primed.FuseTimeLeft)
                                 : string.Format("Fuse          : {0} turn(s)", itemExplosiveModel.FuseDelay));
      int tmp_i = blast.Radius;
      lines.Add(string.Format("Blast radius  : {0}", tmp_i));
      var stringBuilder = new StringBuilder();
      for (int distance = 0; distance <= tmp_i; ++distance)
        stringBuilder.AppendFormat("{0};", blast.DamageAt(distance));
      lines.Add(string.Format("Blast damages : {0}", stringBuilder.ToString()));
      if (ex is ItemGrenade grenade) {
        lines.Add("> grenade");
        int range = Player.MaxThrowRange((tmp_i = grenade.Model.MaxThrowDistance));
        lines.Add((range != tmp_i) ? string.Format("Throwing rng  : {0} ({1})", range, tmp_i)
                                   : string.Format("Throwing rng  : {0}", range));
      }
      if (null != primed) lines.Add("PRIMED AND READY TO EXPLODE!");
      return lines;
    }

    static private List<string> DescribeItemWeapon(ItemWeapon w)
    {
      var itemWeaponModel = w.Model;
      var lines = new List<string>{
        "> weapon",
        string.Format("Atk : +{0}", itemWeaponModel.Attack.HitValue),
        string.Format("Dmg : +{0}", itemWeaponModel.Attack.DamageValue)
      };
      // alpha10
      int tmp_i;
      if (0 != (tmp_i = itemWeaponModel.Attack.StaminaPenalty)) lines.Add(string.Format("Sta : -{0}", tmp_i));
      if (0 != (tmp_i = itemWeaponModel.Attack.DisarmChance)) lines.Add(string.Format("Disarm : +{0}%", tmp_i));

      if (w is ItemMeleeWeapon melee) {
        if (melee.IsFragile) lines.Add("Breaks easily.");
        var model = melee.Model;
        if (model.IsMartialArts) lines.Add("Uses martial arts.");
        // alpha10 tool
        if (model.IsTool) {
          lines.Add("Is a tool.");
          if (0 != (tmp_i = model.ToolBashDamageBonus)) lines.Add(string.Format("Tool Dmg   : +{0} = +{1}", tmp_i, tmp_i + model.Attack.DamageValue));
          float toolBuild = model.ToolBuildBonus;
          if (0 != toolBuild) lines.Add(string.Format("Tool Build : +{0}%", (int)(100 * toolBuild)));
        }
      } else if (w is ItemRangedWeapon rw) {
        var model = rw.Model;
        lines.Add(model.IsFireArm ? "> firearm"
                                  : (model.IsBow ? "> bow"
                                                 : "> ranged weapon"));

        lines.Add(string.Format("Rapid Fire Atk: {0} {1}", model.RapidFireHit1Value, model.RapidFireHit2Value));  // alpha10
        lines.Add(string.Format("Rng  : {0}-{1}", model.Attack.Range, model.Attack.EfficientRange));
        var ammo = rw.Ammo;
        lines.Add((ammo < (tmp_i = model.MaxAmmo)) ? string.Format("Ammo : {0}/{1}", ammo, tmp_i)
                                                   : string.Format("Ammo : {0} MAX", ammo));
        lines.Add(string.Format("Type : {0}", model.AmmoType.Describe(true)));
      }
      return lines;
    }

    static private string[] DescribeItemAmmo(ItemAmmo am)
    {
      return new string[] { "> ammo", string.Format("Type : {0}", am.AmmoType.Describe(true)) };
    }

    static private List<string> DescribeItemFood(ItemFood f)
    {
      var lines = new List<string>{ "> food" };
      int turn = Session.Get.WorldTime.TurnCounter;
      if (f.IsPerishable) {
        if (f.IsStillFreshAt(turn)) lines.Add("Fresh.");
        else if (f.IsExpiredAt(turn)) lines.Add("*Expired*");
        else if (f.IsSpoiledAt(turn)) lines.Add("**SPOILED**");
        lines.Add(string.Format("Best-Before : {0}", f.BestBefore.ToString()));
      } else
        lines.Add("Always fresh.");
      int baseValue = f.NutritionAt(turn);
      int num = Player.ItemNutritionValue(baseValue);
      lines.Add(num == baseValue ? string.Format("Nutrition   : +{0}", baseValue)
                                 : string.Format("Nutrition   : +{0} (+{1})", num, baseValue));
      return lines;
    }

    static private List<string> DescribeItemMedicine(ItemMedicine med)
    {
      var lines = new List<string>{ "> medicine" };
      var model = med.Model;

      // alpha10 dont add lines for zero values

      int base_effect = model.Healing;
      int actual = Player.ScaleMedicineEffect(base_effect);
      if (0 != actual) {
        lines.Add(actual == base_effect ? string.Format("Healing : +{0}", base_effect)
                                        : string.Format("Healing : +{0} (+{1})", actual, base_effect));
      }
      base_effect = model.StaminaBoost;
      actual = Player.ScaleMedicineEffect(base_effect);
      if (0 != actual) {
        lines.Add(actual == base_effect ? string.Format("Stamina : +{0}", base_effect)
                                        : string.Format("Stamina : +{0} (+{1})", actual, base_effect));
      }
      base_effect = model.SleepBoost;
      actual = Player.ScaleMedicineEffect(base_effect);
      if (0 != actual) {
        lines.Add(actual == base_effect ? string.Format("Sleep   : +{0}", base_effect)
                                        : string.Format("Sleep   : +{0} (+{1})", actual, base_effect));
      }
      base_effect = model.SanityCure;
      actual = Player.ScaleMedicineEffect(base_effect);
      if (0 != actual) {
        lines.Add(actual == base_effect ? string.Format("Sanity  : +{0}", base_effect)
                                        : string.Format("Sanity  : +{0} (+{1})", actual, base_effect));
      }
      if (Session.Get.HasInfection) {
        base_effect = model.InfectionCure;
        actual = Player.ScaleMedicineEffect(base_effect);
        if (0 != actual) {
          lines.Add(actual == base_effect ? string.Format("Cure    : +{0}", base_effect)
                                          : string.Format("Cure    : +{0} (+{1})", actual, base_effect));
        }
      }
      return lines;
    }

    static private string[] DescribeItemBarricadeMaterial(ItemBarricadeMaterial bm)
    {
      int barricade_value = bm.Model.BarricadingValue;
      int num = Player.ScaleBarricadingPoints(barricade_value);
      return new string[]{ "> barricade material",
                           (num == barricade_value) ? string.Format("Barricading : +{0}", barricade_value)
                                                    : string.Format("Barricading : +{0} (+{1})", num, barricade_value)};
    }

    static private List<string> DescribeItemBodyArmor(ItemBodyArmor b)
    {
      var lines = new List<string>{
        "> body armor",
        string.Format("Protection vs Hits  : +{0}", b.Protection_Hit),
        string.Format("Protection vs Shots : +{0}", b.Protection_Shot),
        string.Format("Encumbrance         : -{0} DEF", b.Encumbrance),
        string.Format("Weight              : -{0:F2} SPD", b.Weight/100.0f)
      };
      if (b.IsNeutral) return lines;

      // following general code is to be retained as-is; this is not a CPU-critical path.
      var unsuspicious = new List<string>(1);
      var suspicious = new List<string>(4);    // should be # of gangs
      if (b.IsFriendlyForCops()) unsuspicious.Add("Cops");
      else if (b.IsHostileForCops()) suspicious.Add("Cops");
      foreach (GameGangs.IDs gangID in GameGangs.BIKERS) {
        if (b.IsHostileForBiker(gangID)) suspicious.Add(gangID.Name());
        else if (b.IsFriendlyForBiker(gangID)) unsuspicious.Add(gangID.Name());
      }
      foreach (GameGangs.IDs gangID in GameGangs.GANGSTAS) {
        if (b.IsHostileForBiker(gangID)) suspicious.Add(gangID.Name());
        else if (b.IsFriendlyForBiker(gangID)) unsuspicious.Add(gangID.Name());
      }
      if (unsuspicious.Count > 0) {
        lines.Add("Unsuspicious to:");
        foreach (string str in unsuspicious) lines.Add("- " + str);
      }
      if (suspicious.Count > 0) {
        lines.Add("Suspicious to:");
        foreach (string str in suspicious) lines.Add("- " + str);
      }
      return lines;
    }

    static private string[] DescribeItemSprayPaint(ItemSprayPaint sp)
    {
      int max_paint = sp.Model.MaxPaintQuantity;
      int qty = sp.PaintQuantity;
      return new string[]{ "> spray paint",
                           (qty < max_paint) ? string.Format("Paint : {0}/{1}", qty, max_paint)
                                  : string.Format("Paint : {0} MAX", qty) };
    }

    static private string[] DescribeItemSprayScent(ItemSprayScent sp)
    {
      var model = sp.Model;
      int max_spray = model.MaxSprayQuantity;
      int qty = sp.SprayQuantity;
      return new string[]{ "> spray scent",
                           ((qty < max_spray) ? string.Format("Spray : {0}/{1}", qty, max_spray)
                                              : string.Format("Spray : {0} MAX", qty)), string.Format("Odor     : {0}",
                           model.Odor.ToString().ToLower().Capitalize()),
                           string.Format("Strength : {0}h", model.Strength / WorldTime.TURNS_PER_HOUR) };
    }

    static private string[] DescribeItemLight(ItemLight light)
    {
      return new string[]{ "> light", light.DescribeBatteries(), string.Format("FOV       : +{0}", light.FovBonus) };
    }

    static private List<string> DescribeItemTracker(ItemTracker tr)
    {
      var range_desc = tr.Model.RangeDesc;  // alpha10 range if applicable
      var lines = new List<string>{ "> tracker" };
      lines.Add(tr.DescribeBatteries());
      if (null != range_desc) lines.Add("Range: " + range_desc);    // This is a translation target so do not hardcode the prefix
      return lines;
    }

    static private List<string> DescribeItemTrap(ItemTrap tr)
    {
      var lines = new List<string> { "> trap" };

      // alpha10
      void commentary() {
        if (tr.IsSafeFor(Player)) {
          lines.Add("You will safely avoid this trap.");
          if (tr.Owner != null) lines.Add(string.Format("Trap setup by {0}.", tr.Owner.Name));
        } else if (tr.WouldLearnHowToBypass(Player)) {
          lines.Add("You would safely avoid this trap, with good advice.");
        }
      }

      if (tr.IsActivated) {
        lines.Add("** Activated! **");
        commentary();
      } else if (tr.IsTriggered) {
        lines.Add("** Triggered! **");
        commentary();
      }
      // alpha10
      lines.Add(string.Format("Trigger chance for you : {0}%.", tr.TriggerChanceFor(Player)));

      ItemTrapModel itemTrapModel = tr.Model;
      if (itemTrapModel.IsOneTimeUse) lines.Add("Desactives when triggered.");
      if (itemTrapModel.IsNoisy) lines.Add(string.Format("Makes {0} noise.", itemTrapModel.NoiseName));
      if (itemTrapModel.UseToActivate) lines.Add("Use to activate.");
      lines.Add(string.Format("Damage  : {0} x{1} = {2}", itemTrapModel.Damage, tr.Quantity, tr.Quantity * itemTrapModel.Damage));  // alpha10
      lines.Add(string.Format("Trigger : {0}% x{1} = {2}%", itemTrapModel.TriggerChance, tr.Quantity, tr.Quantity * itemTrapModel.TriggerChance));  // alpha10
      lines.Add(string.Format("Break   : {0}%", itemTrapModel.BreakChance));
      if (itemTrapModel.BlockChance > 0) lines.Add(string.Format("Block   : {0}%", itemTrapModel.BlockChance));
      if (itemTrapModel.BreakChanceWhenEscape > 0) lines.Add(string.Format("{0}% to break on escape", itemTrapModel.BreakChanceWhenEscape));
      return lines;
    }

    static private List<string> DescribeItemEntertainment(ItemEntertainment ent)
    {
      var lines = new List<string> { "> entertainment" };
      if (ent.IsBoringFor(Player)) lines.Add("* BORED OF IT! *");
      ItemEntertainmentModel entertainmentModel = ent.Model;
      int ent_value = entertainmentModel.Value;
      int num = Player.ScaleSanRegen(ent_value);
      lines.Add((num != ent_value) ? string.Format("Sanity : +{0} (+{1})", num, ent_value)
                                   : string.Format("Sanity : +{0}", ent_value));
      lines.Add(string.Format("Boring : {0}%", entertainmentModel.BoreChance));
      return lines;
    }
#nullable restore

    static private string DescribeSkillShort(Skills.IDs id)
    {
      switch (id) {
        case Skills.IDs.AGILE:
          return string.Format("+{0} melee ATK, +{1} DEF", Actor.SKILL_AGILE_ATK_BONUS, Rules.SKILL_AGILE_DEF_BONUS);
        case Skills.IDs.AWAKE:
          return string.Format("+{0}% max SLP, +{1}% SLP regen ", (int)(100.0 * Actor.SKILL_AWAKE_SLEEP_BONUS), (int)(100.0 * Actor.SKILL_AWAKE_SLEEP_REGEN_BONUS));
        case Skills.IDs.BOWS:
          return string.Format("bows +{0} ATK, +{1} DMG", Actor.SKILL_BOWS_ATK_BONUS, Actor.SKILL_BOWS_DMG_BONUS);
        case Skills.IDs.CARPENTRY:
          return string.Format("build, -{0} mat. at lvl 3, +{1}% barricading", Actor.SKILL_CARPENTRY_LEVEL3_BUILD_BONUS, (int)(100.0 * Actor.SKILL_CARPENTRY_BARRICADING_BONUS));
        case Skills.IDs.CHARISMATIC:
          return string.Format("+{0} trust per turn, +{1}% trade offers", Actor.SKILL_CHARISMATIC_TRUST_BONUS, Actor.SKILL_CHARISMATIC_TRADE_BONUS);
        case Skills.IDs.FIREARMS:
          return string.Format("firearms +{0} ATK, +{1} DMG", Actor.SKILL_FIREARMS_ATK_BONUS, Actor.SKILL_FIREARMS_DMG_BONUS);
        case Skills.IDs.HARDY:
          return string.Format("sleeping anywhere heals, +{0}% chance to heal when sleeping", Actor.SKILL_HARDY_HEAL_CHANCE_BONUS);
        case Skills.IDs.HAULER:
          return string.Format("+{0} inventory slots", Actor.SKILL_HAULER_INV_BONUS);
        case Skills.IDs.HIGH_STAMINA:
          return string.Format("+{0} STA", Actor.SKILL_HIGH_STAMINA_STA_BONUS);
        case Skills.IDs.LEADERSHIP:
          return string.Format("+{0} max Followers", Actor.SKILL_LEADERSHIP_FOLLOWER_BONUS);
        case Skills.IDs.LIGHT_EATER:
          return string.Format("+{0}% max FOO, +{1}% item food points", (int)(100.0 * Actor.SKILL_LIGHT_EATER_MAXFOOD_BONUS), (int)(100.0 * Actor.SKILL_LIGHT_EATER_FOOD_BONUS));
        case Skills.IDs.LIGHT_FEET:
          return string.Format("+{0}% to avoid and escape traps", Rules.SKILL_LIGHT_FEET_TRAP_BONUS);
        case Skills.IDs.LIGHT_SLEEPER:
          return string.Format("+{0}% noise wake up chance", Actor.SKILL_LIGHT_SLEEPER_WAKEUP_CHANCE_BONUS);
        case Skills.IDs.MARTIAL_ARTS:
          return string.Format("unarmed only +{0} Atk, +{1} Dmg", Actor.SKILL_MARTIAL_ARTS_ATK_BONUS, Actor.SKILL_MARTIAL_ARTS_DMG_BONUS);
        case Skills.IDs.MEDIC:
          return string.Format("+{0}% medicine effects, +{1}% revive ", (int)(100.0 * Actor.SKILL_MEDIC_BONUS), Actor.SKILL_MEDIC_REVIVE_BONUS);
        case Skills.IDs.NECROLOGY:
          return string.Format("+{0}/+{1} DMG vs undeads/corpses, data on corpses", Actor.SKILL_NECROLOGY_UNDEAD_BONUS, Actor.SKILL_NECROLOGY_CORPSE_BONUS);
        case Skills.IDs.STRONG:
          return string.Format("+{0} melee DMG, +{1} throw range", Actor.SKILL_STRONG_DMG_BONUS, Actor.SKILL_STRONG_THROW_BONUS);
        case Skills.IDs.STRONG_PSYCHE:
          return string.Format("+{0}% SAN threshold, +{1}% regen", (int)(100.0 * Actor.SKILL_STRONG_PSYCHE_LEVEL_BONUS), (int)(100.0 * Actor.SKILL_STRONG_PSYCHE_ENT_BONUS));
        case Skills.IDs.TOUGH:
          return string.Format("+{0} HP", Actor.SKILL_TOUGH_HP_BONUS);
        case Skills.IDs.UNSUSPICIOUS:
          return string.Format("+{0}% unnoticed by law enforcers and gangs", Actor.SKILL_UNSUSPICIOUS_BONUS);
        case Skills.IDs.Z_AGILE:
          return string.Format("+{0} melee ATK, +{1} DEF, can jump", Actor.SKILL_ZAGILE_ATK_BONUS, Rules.SKILL_ZAGILE_DEF_BONUS);
        case Skills.IDs.Z_EATER:
          return string.Format("+{0}% eating HP regen", (int)(100.0 * Actor.SKILL_ZEATER_REGEN_BONUS));
        case Skills.IDs.Z_GRAB:
          return string.Format("can grab enemies, +{0}% per level", Actor.SKILL_ZGRAB_CHANCE);
        case Skills.IDs.Z_INFECTOR:
          return string.Format("+{0}% infection damage", (int)(100.0 * Actor.SKILL_ZINFECTOR_BONUS));
        case Skills.IDs.Z_LIGHT_EATER:
          return string.Format("+{0}% max ROT, +{1}% from eating", (int)(100.0 * Actor.SKILL_ZLIGHT_EATER_MAXFOOD_BONUS), (int)(100.0 * Actor.SKILL_ZLIGHT_EATER_FOOD_BONUS));
        case Skills.IDs.Z_LIGHT_FEET:
          return string.Format("+{0}% to avoid traps", Rules.SKILL_ZLIGHT_FEET_TRAP_BONUS);
        case Skills.IDs.Z_STRONG:
          return string.Format("+{0} melee DMG, can push", Actor.SKILL_ZSTRONG_DMG_BONUS);
        case Skills.IDs.Z_TOUGH:
          return string.Format("+{0} HP", Actor.SKILL_ZTOUGH_HP_BONUS);
        case Skills.IDs.Z_TRACKER:
          return string.Format("+{0}% smell", (int)(100.0 * Actor.SKILL_ZTRACKER_SMELL_BONUS));
        default:
          throw new ArgumentOutOfRangeException("unhandled skill id");
      }
    }

    static private string DescribeWeather(Weather weather)
    {
      switch (weather) {
        case Weather.CLEAR: return "Clear";
        case Weather.CLOUDY: return "Cloudy";
        case Weather.RAIN: return "Rain";
        case Weather.HEAVY_RAIN: return "Heavy rain";
        default: throw new ArgumentOutOfRangeException("unhandled weather");
      }
    }

    static private Color WeatherColor(Weather weather)
    {
      switch (weather) {
        case Weather.CLEAR: return Color.Yellow;
        case Weather.CLOUDY: return Color.Gray;
        case Weather.RAIN: return Color.LightBlue;
        case Weather.HEAVY_RAIN: return Color.Blue;
        default: throw new ArgumentOutOfRangeException("unhandled weather");
      }
    }

#nullable enable
    public static Direction? CommandToDirection(PlayerCommand cmd)
    {
      switch (cmd) {
        case PlayerCommand.MOVE_N: return Direction.N;
        case PlayerCommand.MOVE_NE: return Direction.NE;
        case PlayerCommand.MOVE_E: return Direction.E;
        case PlayerCommand.MOVE_SE: return Direction.SE;
        case PlayerCommand.MOVE_S: return Direction.S;
        case PlayerCommand.MOVE_SW: return Direction.SW;
        case PlayerCommand.MOVE_W: return Direction.W;
        case PlayerCommand.MOVE_NW: return Direction.NW;
        case PlayerCommand.WAIT_OR_SELF: return Direction.NEUTRAL;
        default: return null;
      }
    }
#nullable restore

    public void DoMoveActor(Actor actor, in Location newLocation)
    {
       _Action.MoveStep._Ok(actor.Location, in newLocation, actor);
      if (!TryActorLeaveTile(actor)) {
        actor.SpendActionPoints();
        return;
      }
      Location location = actor.Location;
      if (location.Map != newLocation.Map) throw new NotImplementedException("DoMoveActor : illegal to change map.");
	  // committed to move now
      actor.StandUp();  // movement cancels crouching, for now (duck walk very low priority to implement)
	  actor.Moved();
      newLocation.Place(actor);
      var a_witness = actor.PlayersInLOS();
      var d_witness = PlayersInLOS(location);
      var ad_witness = a_witness?.Intersect(d_witness);
      var a_only_witness = a_witness?.SetDifference(ad_witness);
      var d_only_witness = d_witness?.SetDifference(ad_witness);

      bool dest_seen = null != a_witness;
      if (null != a_witness) PanViewportTo(a_witness);

      var draggedCorpse = actor.DraggedCorpse;
      if (draggedCorpse != null) {
        draggedCorpse.Location = actor.Location;
        var msg = MakePanopticMessage(actor, string.Format("{0} {1} corpse.", VERB_DRAG.Conjugate(actor), draggedCorpse.DeadGuy.TheName));
        if (null != a_witness) AddMessage(a_witness, msg);
        if (null != d_only_witness) AddMessage(d_only_witness, msg);
      }
      int actionCost = Actor.BASE_ACTION_COST;
      if (actor.IsRunning) {
        actionCost /= 2;
        actor.SpendStaminaPoints(Rules.STAMINA_COST_RUNNING);
      }
      var mapObjectAt = newLocation.MapObject;
      if (null != mapObjectAt && mapObjectAt.IsJumpable) {
        actor.SpendStaminaPoints(Rules.STAMINA_COST_JUMP);
        if (null != a_witness) AddMessage(a_witness, MakePanopticMessage(actor, VERB_JUMP_ON.Conjugate(actor), mapObjectAt));
        if (actor.Model.Abilities.CanJumpStumble && Rules.Get.RollChance(JUMP_STUMBLE_CHANCE)) {
          actionCost += JUMP_STUMBLE_ACTION_COST;
          if (null != a_witness) AddMessage(a_witness, MakePanopticMessage(actor, string.Format("{0}!", VERB_STUMBLE.Conjugate(actor))));
        }
      }

      const int STAMINA_COST_MOVE_DRAGGED_CORPSE = 8;
      if (draggedCorpse != null) actor.SpendStaminaPoints(STAMINA_COST_MOVE_DRAGGED_CORPSE);
      actor.SpendActionPoints(actionCost);

      if (actor.GetEquippedItem(DollPart.HIP_HOLSTER) is ItemTracker tracker) tracker.Batteries += 2;  // police radio recharge

      if (0 < actor.ActionPoints) actor.DropScent();    // alpha10 fix
      if (!actor.IsPlayer && (actor.Activity == Data.Activity.FLEEING || actor.Activity == Data.Activity.FLEEING_FROM_EXPLOSIVE) && (!actor.Model.Abilities.IsUndead && actor.Model.Abilities.CanTalk))
      {
        OnLoudNoise(newLocation, "A loud SCREAM");
        if (!dest_seen && Rules.Get.RollChance(PLAYER_HEAR_SCREAMS_CHANCE))
          AddMessageIfAudibleForPlayer(actor.Location, "You hear screams of terror");
      }
      OnActorEnterTile(actor);
      if (dest_seen || actor.IsPlayer) RedrawPlayScreen();
    }

#nullable enable
    public void OnActorEnterTile(Actor actor)
    {
	  Session.Get.Police.TrackThroughExitSpawn(actor);
      (actor.Controller as ObjectiveAI)?.OnMove();  // 2019-08-24: both calls required to pass regression test
      Map map = actor.Location.Map;
      Point pos = actor.Location.Position;
      if (!map.IsTrapCoveringMapObjectAt(pos)) {
        if (0 == OnActorReachIntoTile(actor, actor.Location)) return; // no further processing if dead
      }
      map.OnEnterTile(actor);
    }

    public int OnActorReachIntoTile(Actor actor, Location dest)
    {
      List<Actor>? trap_owners = null;
      int old_hp = actor.HitPoints;
      int cur_hp = old_hp;
      dest.Map.RemoveAt<ItemTrap>(trap => {
          bool trap_gone = TryTriggerTrap(trap, actor);
          int new_hp = actor.HitPoints;
          if (cur_hp > new_hp) {
              var owner = trap.Owner;
              if (null != owner) (trap_owners ??= new List<Actor>()).Add(owner);
              cur_hp = new_hp;
          }
          return trap_gone;
      }, dest.Position);
      if (0 >= cur_hp) {
        var owner = trap_owners?[0];
        // look the other way when it comes to friendly trap kills
        if (null != owner && !string.IsNullOrEmpty(owner.AIwillNotMurder)) owner = null;
        // the above can trigger killing of actor already; hard crash
        if (!actor.IsDead) KillActor(owner, actor, "trap");
        return 0;
      }
      return old_hp > cur_hp ? -1 : 1;
    }

#nullable restore

    public bool TryActorLeaveTile(Actor actor)
    {
      Map map = actor.Location.Map;
      Point position = actor.Location.Position;
      bool canLeave = true;
      if (!map.IsTrapCoveringMapObjectAt(position)) {
        bool live_trap = false;
        map.RemoveAt<ItemTrap>(trap => {
            if (!trap.IsTriggered) return false;
            live_trap = true;
            if (!TryEscapeTrap(trap, actor, out bool isDestroyed)) canLeave = false;
            else if (isDestroyed) return true;
            return false;
        }, in position);
        if (canLeave && live_trap) actor.Location.Items?.UntriggerAllTraps();
      }
      bool visible = ForceVisibleToPlayer(actor);
      // 2020-01-03: Z-grab extended to cross-exit
      actor.Location.ForEachAdjacent(loc => {
          var actorAt = loc.Actor;
          if (actorAt == null || !actorAt.Model.Abilities.IsUndead || !actorAt.IsEnemyOf(actor) || !Rules.Get.RollChance(actorAt.ZGrabChance(actor))) return;
          if (visible) AddMessage(MakeMessage(actorAt, VERB_GRAB.Conjugate(actorAt), actor));
          canLeave = false;
      });
      return canLeave;
    }

#nullable enable
    private bool TryTriggerTrap(ItemTrap trap, Actor victim)
    {
      // \todo possible micro-optimization from common tests behind these function calls
      // sole caller has trap at victim's location
      if (!trap.IsActivated) return false;
      if (!victim.Controller.IsEngaged && trap.LearnHowToBypass(victim, victim.Location)) return false;

      if (trap.TriggeredBy(victim)) return DoTriggerTrap(trap, victim);
      else if (IsVisibleToPlayer(victim))
        AddMessage(MakeMessage(victim, string.Format("safely {0} {1}.", VERB_AVOID.Conjugate(victim), trap.TheName)));
      return false;
    }

    private bool TryEscapeTrap(ItemTrap trap, Actor victim, out bool isDestroyed)
    {
      isDestroyed = false;
      if (trap.Model.BlockChance <= 0) return true;
      bool player = ForceVisibleToPlayer(victim);
      var rules = Rules.Get;
      bool flag = rules.CheckTrapEscape(trap, victim);
      if (flag) {
        trap.IsTriggered = false;
        if (player) AddMessage(MakeMessage(victim, string.Format("{0} {1}.", VERB_ESCAPE.Conjugate(victim), trap.TheName)));
        if (rules.CheckTrapEscapeBreaks(trap, victim)) {
          if (player) AddMessage(MakeMessage(victim, string.Format("{0} {1}.", VERB_BREAK.Conjugate(victim), trap.TheName)));
          isDestroyed = trap.Consume();
        }
      }
      else if (player) AddMessage(MakeMessage(victim, string.Format("is trapped by {0}!", trap.TheName)));
      return flag;
    }

    private void CheckMapObjectTriggersTraps(Map map, Point pos)
    {
      var obj = map.GetMapObjectAt(pos);
      if (null != obj && obj.TriggersTraps) {
        map.RemoveAt<ItemTrap>(trap => {
          if (!trap.IsActivated) return false;
          return DoTriggerTrap(trap, obj);
        }, pos);
      }
    }

    private void DefenderDamageIcon(Actor defender, string icon, string damage)
    {
      const int DAMAGE_DX = 10;
      const int DAMAGE_DY = 10;

      if (IsInViewRect(defender.Location)) {
        var screenPos = MapToScreen(defender.Location);
        AddOverlay(new OverlayImage(screenPos, icon));
        AddOverlay(new OverlayText(screenPos.Add(DAMAGE_DX, DAMAGE_DY), Color.White, damage, Color.Black));
      }
    }

    private bool DoTriggerTrap(ItemTrap trap, Actor victim)
    {
      var witnesses = victim.PlayersInLOS();
      trap.IsTriggered = true;
      ItemTrapModel trapModel = trap.Model;
      int dmg = trapModel.Damage * trap.Quantity;
      if (dmg > 0) {
        victim.TakeDamage(dmg);
        if (null != witnesses) {
          DefenderDamageIcon(victim, GameImages.ICON_MELEE_DAMAGE, dmg.ToString());
          ImportantMessage(witnesses, MakePanopticMessage(victim, string.Format("is hurt by {0} for {1} damage!", trap.AName, dmg)), victim.IsPlayer ? DELAY_NORMAL : DELAY_SHORT);
          ClearOverlays();
          RedrawPlayScreen();
        }
      }
      if (trapModel.IsNoisy) {
        if (null != witnesses) RedrawPlayScreen(witnesses, MakePanopticMessage(victim, string.Format("stepping on {0} makes a bunch of noise!", trap.AName)));
        OnLoudNoise(victim.Location, trapModel.NoiseName);
      }
      if (trapModel.IsOneTimeUse) trap.Desactivate();  //alpha10

      if (!trap.CheckStepOnBreaks()) return false;
      if (null != witnesses) RedrawPlayScreen(witnesses, MakePanopticMessage(victim, string.Format("{0} {1}.", VERB_CRUSH.Conjugate(victim), trap.TheName)));
      return trap.Consume();
    }

    private bool DoTriggerTrap(ItemTrap trap, MapObject obj)
    {
      ItemTrapModel trapModel = trap.Model;
      bool player = ForceVisibleToPlayer(obj);
      trap.IsTriggered = true;
      if (trapModel.IsNoisy) {
        if (player) AddMessage(new(string.Format("{0} makes a lot of noise!", trap.TheName.Capitalize()), obj.Location.Map.LocalTime.TurnCounter));
        OnLoudNoise(obj.Location, trapModel.NoiseName);
      }
      if (trapModel.IsOneTimeUse) trap.Desactivate();  //alpha10

      if (!trap.CheckStepOnBreaks(obj)) return false;
      if (player) AddMessage(new(string.Format("{0} breaks the {1}.", obj.TheName.Capitalize(), trap.TheName), obj.Location.Map.LocalTime.TurnCounter));
      return trap.Consume();
    }

    // Intentionally leaving in the askForConfirmation parameter: should be used only for plot-significant map changes
    public bool DoLeaveMap(Actor actor, in Point exitPoint, bool askForConfirmation=false)
    {
      bool isPlayer = actor.IsPlayer;
      Location origin = actor.Location;
      Map map = origin.Map;
      var exitAt = map.GetExitAt(exitPoint);
      if (exitAt == null) {
        if (isPlayer) ErrorPopup("There is nowhere to go there.");
#if DEBUG
        else throw new ArgumentNullException(nameof(exitAt));   // going to crash out anyway on the free move trap
#endif
        return true;
      }
      if (isPlayer && askForConfirmation) {
        if (!YesNoPopup(string.Format("REALLY LEAVE {0}", map.Name))) {
          RedrawPlayScreen(new UI.Message("Let's stay here a bit longer...", Session.Get.WorldTime.TurnCounter, Color.Yellow));
          return false;
        }
      }
      if (!TryActorLeaveTile(actor)) {
        actor.SpendActionPoints();
        if (Player==actor) RedrawPlayScreen();
        return false;
      }

      string reason = exitAt.ReasonIsBlocked(actor);
      if (!string.IsNullOrEmpty(reason)) {
        if (isPlayer) ErrorPopup(reason);
#if DEBUG
        else throw new InvalidOperationException(reason);   // going to crash out anyway on the free move trap
#endif
        return true;
      }

      Map exit_map = exitAt.ToMap;
      bool is_cross_district = map.DistrictPos != exit_map.DistrictPos;
      bool run_is_free_move = actor.RunIsFreeMove;
      if (   is_cross_district && run_is_free_move && actor.IsRunning
          && map.DistrictPos.IsScheduledBefore(in exit_map.DistrictPos)) {   // check for movement speed artifacts
          // the move itself is a free move; do not want to burn a run-is-free move on this
          // XXX \todo but if the free move denies an attack from a known attacker in the destination we'd want it anyway; it just won't do so for the source district.
          // Consider delegating to ObjectiveAI.
          // Objective::RunIfAdvisable handles the stamina cutoff recalculation checks.
            actor.Walk();    // cancel wasted running
            if (isPlayer) AddMessage(new("It doesn't feel worth running right now.", Session.Get.WorldTime.TurnCounter, Color.Yellow));
      }
      bool is_running = actor.IsRunning;
      bool run_was_free_move = run_is_free_move && is_running;  // we cannot quite simulate this correctly at district boundaries: the departure district will not respect the free move
      bool need_stamina_regen = (is_running ? !run_is_free_move : !actor.WalkIsFreeMove) && null!=exit_map.NextActorToAct;
      actor.SpendActionPoints(is_running ? Actor.BASE_ACTION_COST/2 : Actor.BASE_ACTION_COST);
      if (is_running) actor.SpendStaminaPoints(Rules.STAMINA_COST_RUNNING);
      else actor.StandUp();  // movement cancels crouching, for now (duck walk very low priority to implement)
      bool origin_seen = ForceVisibleToPlayer(actor);
      var mapObjectAt = exitAt.Location.MapObject;
      if (null != mapObjectAt && mapObjectAt.IsJumpable) {
        actor.SpendStaminaPoints(Rules.STAMINA_COST_JUMP);
        if (origin_seen) AddMessage(MakeMessage(actor, VERB_JUMP_ON.Conjugate(actor), mapObjectAt));   // XXX not quite right, cf. other jump usage
        if (actor.Model.Abilities.CanJumpStumble && Rules.Get.RollChance(JUMP_STUMBLE_CHANCE)) {
          actor.SpendActionPoints(JUMP_STUMBLE_ACTION_COST);
          if (origin_seen) AddMessage(MakeMessage(actor, string.Format("{0}!", VERB_STUMBLE.Conjugate(actor))));
        }
      }
      if (origin_seen && !is_cross_district) AddMessage(MakeMessage(actor, string.Format("{0} {1}.", VERB_LEAVE.Conjugate(actor), map.Name)));
      actor.RemoveFromMap();

      exitAt.Location.Place(actor); // Adds at last position by default
      if (   !is_cross_district // If we can see what we're getting into, we shouldn't visibly double-move (except that is the point of running)
          || run_was_free_move)
        exit_map.MoveActorToFirstPosition(actor);

      if (null != actor.DraggedCorpse) actor.DraggedCorpse.Location = actor.Location;
      if ((ForceVisibleToPlayer(actor) || isPlayer) && !is_cross_district) AddMessage(MakeMessage(actor, string.Format("{0} {1}.", VERB_ENTER.Conjugate(actor), exit_map.Name)));
      if (is_cross_district)
        actor.ActorScoring.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format("Entered district {0}.", exit_map.District.Name));
      if (need_stamina_regen) actor.PreTurnStart();
      OnActorEnterTile(actor);
      actor.Followers?.DoTheyEnterMap(exitAt.Location, in origin);
      if (   actor.Controller is OrderableAI ai && !is_cross_district
          && exitAt.Location.Map == exitAt.Location.Map.District.EntryMap) {
        ai.Avoid(exitAt.Location.Exit);
        // subways are problematic
        foreach(var dir in Direction.COMPASS_4) {
          var near = exitAt.Location + dir;
          if (!Map.Canonical(ref near)) continue;
          var exit = near.Exit;
          if (null != exit) ai.Avoid(exit);
          near += dir;
          if (!Map.Canonical(ref near)) continue;
          exit = near.Exit;
          if (null != exit) ai.Avoid(exit);
        }
      }
      if (isPlayer) PanViewportTo(actor.Location);
      return true;
    }

    public void DoSwitchPlace(Actor actor, Actor other)
    {
      actor.SpendActionPoints(2*Actor.BASE_ACTION_COST);
      Location a_loc = actor.Location;
      Location o_loc = other.Location;
      other.RemoveFromMap();
      o_loc.Place(actor);
      a_loc.Place(other);
      if (IsVisibleToPlayer(actor) || IsVisibleToPlayer(other))
        AddMessage(MakeMessage(actor, VERB_SWITCH_PLACE_WITH.Conjugate(actor), other));
    }

    public void DoTakeLead(Actor actor, Actor other)
    {
      actor.SpendActionPoints();
      actor.AddFollower(other);
      int trustIn = other.GetTrustIn(actor);
      other.TrustInLeader = trustIn;
#if PROTOTYPE
      AddMessageIfVisible(actor, VERB_PERSUADE.Conjugate(actor), other, " to join.", (actor.IsPlayer || other.IsPlayer) ? PLAYER_ACTION_COLOR : OTHER_ACTION_COLOR);
#else
      if (ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(other)) {
        if (Player == actor) ClearMessages();
        AddMessage(MakeMessage(actor, VERB_PERSUADE.Conjugate(actor), other, " to join."));
        if (0 != trustIn) other.Say(actor, "Ah yes I remember you.", Sayflags.IS_FREE_ACTION);
      }
#endif
      if (actor.IsPlayer && !other.IsPlayer && YesNoPopup("Make "+other+" a PC")) {
          other.MakePC();
          Session.Get.Scoring.UseReincarnation(); // intentionally unconditional
      }
    }

    public void DoCancelLead(Actor actor, Actor follower)
    {
      actor.SpendActionPoints();
      actor.RemoveFollower(follower);
      follower.SetTrustIn(actor, follower.TrustInLeader);
      follower.TrustInLeader = 0;
      if (!ForceVisibleToPlayer(actor) && !ForceVisibleToPlayer(follower)) return;
      if (Player == actor) ClearMessages();
      AddMessage(MakeMessage(actor, VERB_PERSUADE.Conjugate(actor), follower, " to leave."));
    }

    // It breaks immersion to have to run, rather than wait, to adjust the energy level above 50
    public void DoWait(Actor actor)
    {
      actor.Wait();
      if (ForceVisibleToPlayer(actor)) {
        AddMessage(MakeMessage(actor, (actor.StaminaPoints < actor.MaxSTA)
                                    ? string.Format("{0} {1} breath.", VERB_CATCH.Conjugate(actor), actor.HisOrHer)
                                    : string.Format("{0}.", VERB_WAIT.Conjugate(actor))));
      }
      actor.RegenStaminaPoints(Actor.STAMINA_REGEN_WAIT);
      if (Player == actor) RedrawPlayScreen();
    }
#nullable restore

    public bool DoPlayerBump(Actor player, Direction direction)
    {
      var actionBump = new ActionBump(player, direction);
      if (actionBump.IsPerformable()) {
        actionBump.Perform();
        return true;
      }
      if (player.Location.Map.GetMapObjectAt(player.Location.Position + direction) is DoorWindow doorWindow && doorWindow.IsBarricaded && !player.Model.Abilities.IsUndead) {
        if (!player.IsTired) {
          if (YesNoPopup("Really tear down the barricade")) {
            DoBreak(player, doorWindow);
            return true;
          }
          AddMessage(new("Good, keep everything secure.", Session.Get.WorldTime.TurnCounter, Color.Yellow));
          return false;
        }
        ErrorPopup("Too tired to tear down the barricade.");
        return false;
      }
      ErrorPopup(string.Format("Cannot do that : {0}.", actionBump.FailReason));
      return false;
    }

    public void RadioNotifyAggression(Actor aggressor, Actor target, string raw_msg) // i18n target
    {
        bool wasAlreadyEnemy = aggressor.IsAggressorOf(target) || target.IsAggressorOf(aggressor);
        UI.Message msg = new(string.Format(raw_msg, aggressor.Name, target.Name), Session.Get.WorldTime.TurnCounter, SAYOREMOTE_NORMAL_COLOR);
        UI.Message officer_msg = new(string.Format(raw_msg, aggressor.Name, target.Name), Session.Get.WorldTime.TurnCounter, SAYOREMOTE_DANGER_COLOR);
        Action<PlayerController> pc_add_msg = pc => {
            var a = pc.ControlledActor;
            if (a.Model.Abilities.IsLawEnforcer && !a.IsInGroupWith(target) && !a.IsEnemyOf(target)) target.RecordAggression(a);
            var danger = a.Model.Abilities.IsLawEnforcer || a.IsInGroupWith(target) || a.IsInGroupWith(aggressor);
            if (danger) pc.AddMessageForceRead(officer_msg);
            else pc.AddMessage(msg);
        };
        aggressor.MessageAllInDistrictByRadio(npc => target.RecordAggression(npc),
        npc => {
            if (npc == aggressor) return false;
            if (npc == target) return false;
            if (!npc.Model.Abilities.IsLawEnforcer) return false;
            if (npc.IsInGroupWith(target)) return false;
            if (npc.IsEnemyOf(target)) return false;
            return true;
        }, pc_add_msg, pc_add_msg, player => {
            if (player == aggressor) return false;
            if (player == target) return false;
            if (wasAlreadyEnemy && player.IsEnemyOf(target)) return false;
            return true;
        });
    }

    public void DoMakeAggression(Actor aggressor, Actor target)
    {
      if (aggressor.Faction.IsEnemyOf(target.Faction)) return;
      bool wasAlreadyEnemy = aggressor.IsAggressorOf(target) || target.IsAggressorOf(aggressor);

      // not really, but we don't have a good enough crime-tracking system
      if (aggressor.Faction.ID.ExtortionIsAggression()) {
#if DEBUG
        if (Rules.IsMurder(aggressor,target)) throw new InvalidOperationException("authorities do not murder");
#endif
        if (!target.IsAggressorOf(aggressor)) aggressor.RecordAggression(target);
      } else {
        aggressor.RecordAggression(target);
      }

      var police = Session.Get.Police;
      var police_relevant = police.EncodeAggression(aggressor, target, Session.Get.WorldTime.TurnCounter);
      if (null != police_relevant) {
        Actor? seen_by_police = (police.IsMine(target) && !target.IsSleeping) ? target : null;
        if (null == seen_by_police && police.IsMine(aggressor)) seen_by_police = aggressor; // aggressor will be awake
        if (null == seen_by_police) {
          var test = WitnessesInLOS(aggressor.Location);
          if (null != test) test = test.Value.grep(police.isMine());
          if (null != test) seen_by_police = test.Value.WitnessedBy(Actor.isAwake);
        }
        if (null == seen_by_police) {
          var test = WitnessesInLOS(target.Location);
          if (null != test) test = test.Value.grep(police.isMine());
          if (null != test) seen_by_police = test.Value.WitnessedBy(Actor.isAwake);
        }
        if (null != seen_by_police) {
          if (!aggressor.Model.Abilities.IsLawEnforcer || Rules.IsMurder(aggressor, seen_by_police)) {
            bool was_viewpoint_enemy = target == seen_by_police ? wasAlreadyEnemy : aggressor.IsAggressorOf(seen_by_police) || target.IsAggressorOf(seen_by_police);
            police.Record(police_relevant);
            UI.Message? msg = null;
            Action<PlayerController> pc_add_msg = pc => pc.AddMessage(msg ??= new("(police radio ai) " + aggressor.Name + " assaulting " + target.Name, Session.Get.WorldTime.TurnCounter, SAYOREMOTE_DANGER_COLOR));
            seen_by_police.MessageAllInDistrictByRadio(NOP, FALSE, pc_add_msg, pc_add_msg, TRUE);
            if (!GameFactions.ThePolice.IsEnemyOf(aggressor.Faction)) OnMakeEnemyOfCop(aggressor, seen_by_police, was_viewpoint_enemy);
          }
        }
      }

      // XXX \todo check for allies who witness this
      if (target.IsSleeping) return;
      if (!wasAlreadyEnemy) {
        string msg = (target.Controller as ObjectiveAI)?.AggressedBy(aggressor);
        if (!string.IsNullOrEmpty(msg)) target.Say(aggressor, msg, Sayflags.IS_FREE_ACTION | Sayflags.IS_DANGER);
      }
      Faction faction = target.Faction;
/*    if (GameFactions.ThePolice == faction) {
        if (aggressor.Model.Abilities.IsLawEnforcer && !Rules.IsMurder(aggressor, target)) return;
        OnMakeEnemyOfCop(aggressor, target, wasAlreadyEnemy);
      } else  */ if (GameFactions.TheArmy == faction) {
        OnMakeEnemyOfSoldier(aggressor, target, wasAlreadyEnemy);
      }
      var leader = target.LiveLeader;
      if (null != leader) {
        faction = leader.Faction;
        if (faction.IsEnemyOf(aggressor.Faction)) return;   // intercept invariant failure when aggressing a civilian follower of a cop
        if (faction == GameFactions.ThePolice) {
          if (aggressor.Model.Abilities.IsLawEnforcer && !Rules.IsMurder(aggressor, target)) return;
          OnMakeEnemyOfCop(aggressor, leader, wasAlreadyEnemy);
        } else if (faction == GameFactions.TheArmy) {
          OnMakeEnemyOfSoldier(aggressor, leader, wasAlreadyEnemy);
        }
      }
    }

    public static void DamnCHARtoPoliceInvestigation()
    {
#region 1) examine the underground base
      Map m = Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap;
      Point exit_to_surface = m.Extent/2;
      // underground base is square
      bool in_underground_base_room(Point p) {
        if (0 >= p.X) return false;
        if (m.Width-1 <= p.X) return false;
        if (exit_to_surface.X-2 <=p.X && exit_to_surface.X + 2 >= p.X) return false;
        if (0 >= p.Y) return false;
        if (m.Height-1 <= p.Y) return false;
        if (exit_to_surface.Y-2 <=p.Y && exit_to_surface.Y + 2 >= p.Y) return false;
        return true;
      }
      m.Rect.DoForEach(pt=>Session.Get.Police.Investigate.Record(m, in pt),
          pt => in_underground_base_room(pt));
#endregion
#region 2) examine all CHAR offices
     World.Get.DoForAllDistricts(District.DamnCHAROfficesToPoliceInvestigation);
#endregion
#region 3) signal police to look for CHAR base entrance
     if (LookingForCHARBase.IsSecret) {
        // \todo radio message, ultra-long range (repeater network); trigger could be confession, police trespass, or police takedown of murderer
        World.Get.DoForAllActors(a => {
            if (a.IsFaction(GameFactions.IDs.ThePolice)) (a.Controller as ObjectiveAI)?.AddFOVevent(new LookingForCHARBase(a));
        });
     }
#endregion
    }

    private void OnMakeEnemyOfCop(Actor aggressor, Actor cop, bool wasAlreadyEnemy)
    {
#if DEBUG
      if (GameFactions.ThePolice.IsEnemyOf(aggressor.Faction)) throw new InvalidOperationException("GameFactions.ThePolice.IsEnemyOf(aggressor.Faction)");
#else
      if (GameFactions.ThePolice.IsEnemyOf(aggressor.Faction)) return;
#endif
      if (!wasAlreadyEnemy)
        cop.Say(aggressor, string.Format("TO DISTRICT PATROLS : {0} MUST DIE!", aggressor.TheName), Sayflags.IS_FREE_ACTION | Sayflags.IS_DANGER);

      MakeEnemyOfTargetFactionInDistrict(aggressor, cop);
      // XXX this should be a more evident message to PC police
      if (aggressor.IsFaction(GameFactions.IDs.TheCHARCorporation) && 1 > Session.Get.ScriptStage_PoliceCHARrelations) {
        // Operation Dead Hand orders do not exclude police
        // XXX should require a policeman to read the CHAR Guard Manual for this effect?
        // XXX alternately: if a policeman reads the CHAR Guard Manual before contact w/CHAR HQ is re-established this becomes irreversible?
        Session.Get.ScriptStage_PoliceCHARrelations = 1;
//      GameFactions.ThePolice.AddEnemy(GameFactions.TheCHARCorporation);   // works here, but parallel when loading the game doesn't
        DamnCHARtoPoliceInvestigation();
      }
    }

    private void OnMakeEnemyOfSoldier(Actor aggressor, Actor soldier, bool wasAlreadyEnemy)
    {
#if DEBUG
      if (GameFactions.TheArmy.IsEnemyOf(aggressor.Faction)) throw new InvalidOperationException("GameFactions.TheArmy.IsEnemyOf(aggressor.Faction)");;
#else
      if (GameFactions.TheArmy.IsEnemyOf(aggressor.Faction)) return;
#endif
      if (!wasAlreadyEnemy)
        soldier.Say(aggressor, string.Format("TO DISTRICT SQUADS : {0} MUST DIE!", aggressor.TheName), Sayflags.IS_FREE_ACTION | Sayflags.IS_DANGER);

      MakeEnemyOfTargetFactionInDistrict(aggressor, soldier);
    }

#nullable enable
    private static void MakeEnemyOfTargetFactionInDistrict(Actor aggressor, Actor target)
    {
      // XXX this should actually be based on radio range
      // the range should include the entire district: radio must reach (district size-1,district size -1) from (0,0)
      // so first choice is grid distance vs. euclidean distance (noise and vision are on euclidean distance)
      // we then have a concept of "radio-equivalent coordinates"; subway and sewer are 1-1 with entry map, basement embeds in entry map
      // police station, hospital, CHAR base are problematic
      Faction faction = target.Faction;
      void IsAggressed(Actor a){ aggressor.RecordAggression(a); }
      bool IsAggressable(Actor a) {
        if (a == aggressor) return false;
        if (a.Leader == aggressor) return false;
        return a.Faction == faction;
      }

      List<UI.Message>? player_msgs = null;
      List<UI.Message> radio_messages() {
        int t0 = Session.Get.WorldTime.TurnCounter;
        return new List<UI.Message> {
            new("You get a message from your radio.", t0),
            new(string.Format("{0} is armed and dangerous. Shoot on sight!", aggressor.TheName), t0),
            new(string.Format("Current location : {0}", aggressor.Location), t0)
        };
      }

      void radio_displays(PlayerController pc) {
        pc.AddMessagesForceRead(player_msgs ??= radio_messages());
      }

      bool radio_should_record(Actor a) {
        if (a == aggressor || a.Leader == aggressor) return false;  // aggressor doesn't find this message informative
        if (a.IsEnemyOf(aggressor)) return false; // already an enemy...presumed informed
        return true;
      }

      target.MessageAllInDistrictByRadio(IsAggressed, IsAggressable, radio_displays, radio_displays, radio_should_record);
    }
#nullable restore

    public void DoMeleeAttack(Actor attacker, Actor defender)
    {
      const bool tracing = false; // debugging hook

      attacker.Aggress(defender);
      Attack attack = attacker.MeleeAttack(defender);
      Defence defence = defender.Defence;
      attacker.SpendActionPoints();
      attacker.SpendStaminaPoints(Rules.STAMINA_COST_MELEE_ATTACK + attack.StaminaPenalty);
      var rules = Rules.Get;
      int hitRoll = rules.RollSkill(attack.HitValue);
      int defRoll = rules.RollSkill(defence.Value);
      // 2x damage against sleeping targets
      int dmg = (hitRoll > defRoll ? rules.RollDamage(defender.IsSleeping ? attack.DamageValue * 2 : attack.DamageValue) - defence.Protection_Hit : 0);

      OnLoudNoise(attacker.Location, "Nearby fighting");

        var a_witness = attacker.PlayersInLOS();
        var d_witness = defender.PlayersInLOS();
        var ad_witness = a_witness?.Intersect(d_witness);
        var a_only_witness = a_witness?.SetDifference(ad_witness);
        var d_only_witness = d_witness?.SetDifference(ad_witness);
        KeyValuePair<bool,bool> sees = ForceVisibleToPlayer(attacker, defender, a_only_witness, ad_witness, d_only_witness);
        PlayerController? a_pc = IsPlayer(attacker) ? attacker.Controller as PlayerController : null;
        PlayerController? d_pc = IsPlayer(defender) ? defender.Controller as PlayerController : null;

      // backward compatibility
      bool isDefVisible = sees.Value;
      bool isAttVisible = sees.Key;
      bool isVisible = isAttVisible || isDefVisible;
      bool isPlayer = attacker.IsPlayer || defender.IsPlayer;   // (player1 OR player2) IMPLIES isPlayer?
      bool display_defender = isDefVisible || m_MapView.Contains(defender.Location); // hard-crash if this is false -- denormalization will be null

      bool player_knows(Actor a) {
        return     a.Controller.CanSee(defender.Location) // we already checked the attacker visibility, he's the sound origin
               && !rules.RollChance(PLAYER_HEAR_FIGHT_CHANCE);  // not clear enough; no message
      }
      void react(Actor a) {
        if (!(a.Controller is OrderableAI ai)) return;  // not that smart (ultimately would want to extend to handler FeralDogAI
        bool attacker_relevant = a.IsEnemyOf(attacker);
        bool defender_relevant = a.IsEnemyOf(defender);
        if (a.Controller is CHARGuardAI) {
          // CHAR guards generally ignore enemies not within a CHAR office. \todo should not be ignoring threats just outside of the doors
          // \todo should be much more concerned about threats in *our* CHAR office
          if (!IsInCHARProperty(attacker.Location)) attacker_relevant = false;
          if (!IsInCHARProperty(defender.Location)) defender_relevant = false;
        }

        if (!attacker_relevant && !defender_relevant) return;   // not relevant
        if (    ai.IsDistracted(ObjectiveAI.ReactionCode.NONE)
            || !rules.RollChance(PLAYER_HEAR_FIGHT_CHANCE))  // not clear enough
          return;
        if (!ai.CombatUnready()) {  // \todo should discriminate between cops/soldiers/CHAR guards and civilians here; civilians may avoid even if combat-ready
          if (a.IsEnemyOf(attacker)) ai.Terminate(attacker);
          if (a.IsEnemyOf(defender)) ai.Terminate(defender);
          return;
        }
        // \todo: get away from the fighting
      }

      PropagateSound(attacker.Location, "You hear fighting",react,player_knows);
      if (isAttVisible) {
        AddOverlay(new OverlayRect(Color.Yellow, new GDI_Rectangle(MapToScreen(attacker.Location), SIZE_OF_ACTOR)));
        if (display_defender) AddOverlay(new OverlayRect(Color.Red, new GDI_Rectangle(MapToScreen(defender.Location), SIZE_OF_ACTOR)));
        AddOverlay(new OverlayImage(MapToScreen(attacker.Location), GameImages.ICON_MELEE_ATTACK));
      }
      if (hitRoll > defRoll) {
        // alpha10
        // roll for attacker disarming defender
        if (attacker.Model.Abilities.CanDisarm && rules.RollChance(attack.DisarmChance)) {
          Item disarmIt = Disarm(defender);
          if (null != disarmIt) {
            // show
            if (null != d_witness) {
              d_pc?.Messages.Clear();
              AddMessage(null, ad_witness, d_only_witness, MakeMessages(attacker, VERB_DISARM.Conjugate(attacker), defender));
              AddMessage(d_witness, new(string.Format("{0} is sent flying!", disarmIt.TheName), attacker.Location.Map.LocalTime.TurnCounter));
              if (null != d_pc) {
                d_pc.AddMessagePressEnter();
              } else {
                RedrawPlayScreen();
                AnimDelay(DELAY_SHORT);
              }
            }
          }
        }

        if (dmg > 0) {
          bool fatal = defender.TakeDamage(dmg);
          if (attacker.Model.Abilities.CanZombifyKilled && !defender.Model.Abilities.IsUndead) {
            attacker.RegenHitPoints(attacker.BiteHpRegen(dmg));
            attacker.RottingEat(dmg);
            if (isVisible) AddMessage(a_only_witness, ad_witness, d_only_witness, MakeMessages(attacker, VERB_FEAST_ON.Conjugate(attacker), defender, " flesh !"));
            defender.Infect(attacker.InfectionForDamage(dmg));
          }
          if (fatal) {
            if (isVisible) {
              if (display_defender) AddOverlay(new OverlayImage(MapToScreen(defender.Location), GameImages.ICON_KILLED));
              ImportantMessage(a_only_witness, ad_witness, d_only_witness, MakeMessages(attacker, (defender.Model.Abilities.IsUndead ? VERB_DESTROY : (Rules.IsMurder(attacker, defender) ? VERB_MURDER : VERB_KILL)).Conjugate(attacker), defender, " !"), DELAY_LONG);
            }
            // need to know whether it's a zombifying hit or not
            bool to_immediately_zombify = Session.Get.HasImmediateZombification && attacker.Model.Abilities.CanZombifyKilled
                                      && !defender.Model.Abilities.IsUndead && Rules.Get.RollChance(s_Options.ZombificationChance);
            KillActor(attacker, defender, "hit");
            if (attacker.Model.Abilities.IsUndead && !defender.Model.Abilities.IsUndead)
              SeeingCauseInsanity(attacker, Rules.SANITY_HIT_EATEN_ALIVE, string.Format("{0} eaten alive", defender.Name));
            if (Session.Get.HasImmediateZombification) {
              if (to_immediately_zombify) {
                defender.Location.Map.TryRemoveCorpseOf(defender);
                Zombify(attacker, defender, false);
                if (isDefVisible) {
                  ImportantMessage(null, ad_witness, d_only_witness, MakeMessages(attacker, "turn".Conjugate(attacker), defender, " into a Zombie!"), DELAY_LONG);
                }
              }
            // RS Alpha 9 had instant player zombification on kill even in infection mode.  It is still possible to play as your zombified self,
            // but the game time has to elapse like for everyone else.
            }
          } else if (isVisible) {
            if (display_defender) DefenderDamageIcon(defender, GameImages.ICON_MELEE_DAMAGE, dmg.ToString());
            ImportantMessage(a_only_witness, ad_witness, d_only_witness, MakeMessages(attacker, attack.Verb.Conjugate(attacker), defender, string.Format(" for {0} damage.", dmg)), isPlayer ? DELAY_NORMAL : DELAY_SHORT);
          }
        } else if (isVisible) {
          if (display_defender) AddOverlay(new OverlayImage(MapToScreen(defender.Location), GameImages.ICON_MELEE_MISS));
          ImportantMessage(a_only_witness, ad_witness, d_only_witness, MakeMessages(attacker, attack.Verb.Conjugate(attacker), defender, " for no effect."), isPlayer ? DELAY_NORMAL : DELAY_SHORT);
        }
      }
      else if (isVisible) {
        if (display_defender) AddOverlay(new OverlayImage(MapToScreen(defender.Location), GameImages.ICON_MELEE_MISS));
        ImportantMessage(a_only_witness, ad_witness, d_only_witness, MakeMessages(attacker, VERB_MISS.Conjugate(attacker), defender), isPlayer ? DELAY_NORMAL : DELAY_SHORT);
      }
      if (attacker.GetEquippedWeapon() is ItemMeleeWeapon itemMeleeWeapon && !itemMeleeWeapon.Model.IsUnbreakable && rules.RollChance(itemMeleeWeapon.IsFragile ? Rules.MELEE_WEAPON_FRAGILE_BREAK_CHANCE : Rules.MELEE_WEAPON_BREAK_CHANCE))
      {
        attacker.OnUnequipItem(itemMeleeWeapon);
        attacker.Inventory.Consume(itemMeleeWeapon);
        if (null != a_witness) {
          ImportantMessage(a_witness, MakePanopticMessage(attacker, string.Format(": {0} breaks and is now useless!", itemMeleeWeapon.TheName)), isPlayer ? DELAY_NORMAL : DELAY_SHORT);
        }
      }
      if (isVisible) ClearOverlays();  // alpha10: if test
      if (!defender.IsDead) (attacker.Controller as ObjectiveAI)?.RecruitHelp(defender);
      defender.InferEnemy(attacker);
    }

    /// <param name="shotCounter">0 for normal shot, 1 for 1st rapid fire shot, 2 for 2nd rapid fire shot</param>
    public void DoSingleRangedAttack(Actor attacker, Actor defender, List<Point> LoF, int shotCounter)
    {
      // stamina penalty is simply copied through from the base ranged attack (calculated below)
      ref var r_attack = ref attacker.CurrentRangedAttack;
      attacker.SpendStaminaPoints(r_attack.StaminaPenalty);
      var rules = Rules.Get;

      if (   r_attack.Kind == AttackKind.FIREARM
          && (rules.RollChance(World.Get.Weather.IsRain() ? Rules.FIREARM_JAM_CHANCE_RAIN : Rules.FIREARM_JAM_CHANCE_NO_RAIN))) {
        var a_witnesses = _ForceVisibleToPlayer(attacker, defender);
        if (null != a_witnesses) RedrawPlayScreen(a_witnesses, MakePanopticMessage(attacker, " : weapon jam!"));
        return;
      }

        int distance = Rules.InteractionDistance(attacker.Location, defender.Location);
        if (!(attacker.GetEquippedWeapon() is ItemRangedWeapon itemRangedWeapon)) throw new InvalidOperationException("DoSingleRangedAttack but no equipped ranged weapon");
        --itemRangedWeapon.Ammo;
        // RS Alpha 9 considered glass objects perfect ablative armor for the target, but didn't inform the AI accordingly
        if (DoCheckFireThrough(attacker, LoF)) ++distance;  // XXX \todo distance penalty should be worse the further the object is from the target

        Attack attack = attacker.RangedAttack(distance, defender);
        Defence defence = defender.Defence;
        // resolve attack: alpha10
        int hitValue = (shotCounter == 0 ? attack.HitValue : shotCounter == 1 ? attack.Hit2Value : attack.Hit3Value);
        int hitRoll = rules.RollSkill(hitValue);
        int defRoll = rules.RollSkill(defence.Value);

        var a_witness = attacker.PlayersInLOS();
        var d_witness = defender.PlayersInLOS();
        var ad_witness = a_witness?.Intersect(d_witness);
        var a_only_witness = a_witness?.SetDifference(ad_witness);
        var d_only_witness = d_witness?.SetDifference(ad_witness);
        KeyValuePair<bool,bool> sees = ForceVisibleToPlayer(attacker, defender, a_only_witness, ad_witness, d_only_witness);
        PlayerController? a_pc = IsPlayer(attacker) ? attacker.Controller as PlayerController : null;
        PlayerController? d_pc = IsPlayer(defender) ? defender.Controller as PlayerController : null;

        // backward compatibility
        bool see_defender = sees.Value;
        bool see_attacker = sees.Key;
        bool player_involved = attacker.IsPlayer || defender.IsPlayer;
        bool display_defender = see_defender || m_MapView.Contains(defender.Location); // hard-crash if this is false -- denormalization will be null

        bool player_knows(Actor a) {
          return     a.Controller.CanSee(defender.Location) // we already checked the attacker visibility, he's the sound origin
                 && !rules.RollChance(PLAYER_HEAR_FIGHT_CHANCE);  // not clear enough; no message
        }
        void react(Actor a) {
          if (!(a.Controller is OrderableAI ai)) return;  // not that smart (ultimately would want to extend to handler FeralDogAI
          bool attacker_relevant = a.IsEnemyOf(attacker);
          bool defender_relevant = a.IsEnemyOf(defender);
          if (a.Controller is CHARGuardAI) {
            // CHAR guards generally ignore enemies not within a CHAR office. \todo should not be ignoring threats just outside of the doors
            // \todo should be much more concerned about threats in *our* CHAR office
            if (!IsInCHARProperty(attacker.Location)) attacker_relevant = false;
            if (!IsInCHARProperty(defender.Location)) defender_relevant = false;
          }

          if (!attacker_relevant && !defender_relevant) return;   // not relevant
          if (    ai.IsDistracted(ObjectiveAI.ReactionCode.NONE)
              || !rules.RollChance(PLAYER_HEAR_FIGHT_CHANCE))
            return;
          if (!ai.CombatUnready()) {  // \todo should discriminate between cops/soldiers/CHAR guards and civilians here; civilians may avoid even if combat-ready
            if (a.IsEnemyOf(attacker)) ai.Terminate(attacker);
            if (a.IsEnemyOf(defender)) ai.Terminate(defender);
            return;
          }
          // \todo: get away from the fighting
        }

        PropagateSound(attacker.Location, "You hear firing",react,player_knows);
        if (see_attacker) {
          AddOverlay(new OverlayRect(Color.Yellow, new GDI_Rectangle(MapToScreen(attacker.Location), SIZE_OF_ACTOR)));
          if (display_defender) AddOverlay(new OverlayRect(Color.Red, new GDI_Rectangle(MapToScreen(defender.Location), SIZE_OF_ACTOR)));
          AddOverlay(new OverlayImage(MapToScreen(attacker.Location), GameImages.ICON_RANGED_ATTACK));
        }
        if (hitRoll > defRoll) {
          int dmg = rules.RollDamage(defender.IsSleeping ? attack.DamageValue * 2 : attack.DamageValue) - defence.Protection_Shot;
          if (dmg > 0) {
            if (defender.TakeDamage(dmg)) {
              if (see_defender) {
                AddOverlay(new OverlayImage(MapToScreen(defender.Location), GameImages.ICON_KILLED));
              } else if (see_attacker) {
                if (display_defender) DefenderDamageIcon(defender, GameImages.ICON_RANGED_DAMAGE, "?");
              }
              if (null != d_witness) ImportantMessage(null, ad_witness, d_only_witness, MakeMessages(attacker, (defender.Model.Abilities.IsUndead ? VERB_DESTROY : (Rules.IsMurder(attacker, defender) ? VERB_MURDER : VERB_KILL)).Conjugate(attacker), defender, " !"), DELAY_LONG);
              if (null != a_only_witness) ImportantMessage(a_only_witness, MakeMessage(a_only_witness[0], attacker, attack.Verb.Conjugate(attacker), defender), player_involved ? DELAY_NORMAL : DELAY_SHORT);
              KillActor(attacker, defender, "shot");
            } else {
              if (see_defender) {
                DefenderDamageIcon(defender, GameImages.ICON_RANGED_DAMAGE, dmg.ToString());
              } else if (see_attacker) { // yes, no difference between destroying and merely attacking if defender isn't seen
                if (display_defender) DefenderDamageIcon(defender, GameImages.ICON_RANGED_DAMAGE, "?");
              }
              if (null != d_witness) ImportantMessage(null, ad_witness, d_only_witness, MakeMessages(attacker, attack.Verb.Conjugate(attacker), defender, string.Format(" for {0} damage.", dmg)), player_involved ? DELAY_NORMAL : DELAY_SHORT);
              if (null != a_only_witness) ImportantMessage(a_only_witness, MakeMessage(a_only_witness[0], attacker, attack.Verb.Conjugate(attacker), defender), player_involved ? DELAY_NORMAL : DELAY_SHORT);
            }
          } else {
            if (see_defender) {
              AddOverlay(new OverlayImage(MapToScreen(defender.Location), GameImages.ICON_RANGED_MISS));
            } else if (see_attacker) {
              if (display_defender) DefenderDamageIcon(defender, GameImages.ICON_RANGED_DAMAGE, "?");
            }
            if (null != d_witness) ImportantMessage(null, ad_witness, d_only_witness, MakeMessages(attacker, attack.Verb.Conjugate(attacker), defender, " for no effect."), player_involved ? DELAY_NORMAL : DELAY_SHORT);
            if (null != a_only_witness) ImportantMessage(a_only_witness, MakeMessage(a_only_witness[0], attacker, attack.Verb.Conjugate(attacker), defender), player_involved ? DELAY_NORMAL : DELAY_SHORT);
          }
        } else {
          bool are_adjacent = Rules.IsAdjacent(attacker.Location, defender.Location);
          if (see_defender) {
            AddOverlay(new OverlayImage(MapToScreen(defender.Location), GameImages.ICON_RANGED_MISS));
          } else if (see_attacker) {
            if (are_adjacent) {  // difference between melee range miss and hit is visible, even with firearms
              if (display_defender) AddOverlay(new OverlayImage(MapToScreen(defender.Location), GameImages.ICON_RANGED_MISS));
            } else {  // otherwise, not visible
              if (display_defender) DefenderDamageIcon(defender, GameImages.ICON_RANGED_DAMAGE, "?");
            }
          }
          if (null != d_witness) ImportantMessage(null, ad_witness, d_only_witness, MakeMessages(attacker, VERB_MISS.Conjugate(attacker), defender), player_involved ? DELAY_NORMAL : DELAY_SHORT);
          if (null != a_only_witness) ImportantMessage(a_only_witness, MakeMessage(a_only_witness[0], attacker, (are_adjacent ? VERB_MISS : attack.Verb).Conjugate(attacker), defender), player_involved ? DELAY_NORMAL : DELAY_SHORT);
        }
        if (see_attacker || see_defender) ClearOverlays();  // alpha10: if-clause bugfix
    }

    private bool DoCheckFireThrough(Actor attacker, List<Point> LoF)
    {
      foreach (Point point in LoF) {
        var mapObjectAt = attacker.Location.Map.GetMapObjectAt(point);
        if (mapObjectAt != null && mapObjectAt.BreaksWhenFiredThrough && (mapObjectAt.BreakState != MapObject.Break.BROKEN && !mapObjectAt.IsWalkable)) {
          bool player1 = ForceVisibleToPlayer(attacker);
          bool player2 = player1 ? IsVisibleToPlayer(mapObjectAt) : ForceVisibleToPlayer(mapObjectAt);
          if (player1 || player2) {
            if (player1) {
              AddOverlay(new OverlayRect(Color.Yellow, new GDI_Rectangle(MapToScreen(attacker.Location), SIZE_OF_ACTOR)));
              AddOverlay(new OverlayImage(MapToScreen(attacker.Location), GameImages.ICON_RANGED_ATTACK));
            }
            if (player2)
              AddOverlay(new OverlayRect(Color.Red, new GDI_Rectangle(MapToScreen(point), SIZE_OF_TILE)));
            AnimDelay(attacker.IsPlayer ? DELAY_NORMAL : DELAY_SHORT);
          }
          mapObjectAt.Destroy();
          return true;
        }
      }
      return false;
    }

    public void UI_ThrowGrenadeUnprimed(Actor actor, Point targetPos, ItemGrenade itemGrenade, List<PlayerController>? a_witness, List<PlayerController>? d_witness)
    {
#if DEBUG
        if (null == a_witness && null == d_witness) throw new InvalidOperationException("tracing");
#endif

        var ad_witness = a_witness?.Intersect(d_witness);
        var a_only_witness = a_witness?.SetDifference(ad_witness);
        var d_only_witness = d_witness?.SetDifference(ad_witness);
        KeyValuePair<bool,bool> sees = ForceVisibleToPlayer(actor, a_only_witness, ad_witness, d_only_witness);

      AddOverlay(new OverlayRect(Color.Yellow, new GDI_Rectangle(MapToScreen(actor.Location), SIZE_OF_ACTOR)));
      AddOverlay(new OverlayRect(Color.Red, new GDI_Rectangle(MapToScreen(targetPos), SIZE_OF_TILE)));
      var phrase = string.Format("{0} a {1}!", VERB_THROW.Conjugate(actor), itemGrenade.Model.SingleName);
      if (null != a_witness) ImportantMessage(a_witness, MakePanopticMessage(actor, phrase), DELAY_LONG);
      if (null != d_only_witness) ImportantMessage(d_only_witness, MakeMessage(d_only_witness[0], actor, phrase), DELAY_LONG);
      ClearOverlays();
      RedrawPlayScreen();
    }

    public void UI_ThrowGrenadePrimed(Actor actor, Point targetPos, ItemGrenadePrimed itemGrenadePrimed, List<PlayerController>? a_witness, List<PlayerController>? d_witness)
    {
#if DEBUG
        if (null == a_witness && null == d_witness) throw new InvalidOperationException("tracing");
#endif

        var ad_witness = a_witness?.Intersect(d_witness);
        var a_only_witness = a_witness?.SetDifference(ad_witness);
        var d_only_witness = d_witness?.SetDifference(ad_witness);
        KeyValuePair<bool,bool> sees = ForceVisibleToPlayer(actor, a_only_witness, ad_witness, d_only_witness);

      AddOverlay(new OverlayRect(Color.Yellow, new GDI_Rectangle(MapToScreen(actor.Location), SIZE_OF_ACTOR)));
      AddOverlay(new OverlayRect(Color.Red, new GDI_Rectangle(MapToScreen(targetPos), SIZE_OF_TILE)));
      var phrase = string.Format("{0} back a {1}!", VERB_THROW.Conjugate(actor), itemGrenadePrimed.Model.SingleName);
      if (null != a_witness) ImportantMessage(a_witness, MakePanopticMessage(actor, phrase), DELAY_LONG);
      if (null != d_only_witness) ImportantMessage(d_only_witness, MakeMessage(d_only_witness[0], actor, phrase), DELAY_LONG);
      ClearOverlays();
      RedrawPlayScreen();
    }


#nullable enable
    private void ShowBlastImage(GDI_Point screenPos, BlastAttack attack, int damage)
    {
      float alpha = (float) (0.1 + damage / (double)attack.Damage[0]);
      if (alpha > 1.0) alpha = 1f;
      AddOverlay(new OverlayTransparentImage(alpha, screenPos, GameImages.ICON_BLAST));
      AddOverlay(new OverlayText(screenPos, Color.Red, damage.ToString(), Color.Black));
    }

    private void DoBlast(Location location, BlastAttack blastAttack)
    {
      OnLoudNoise(location, "A loud EXPLOSION");
      bool isVisible = ForceVisibleToPlayer(in location);
      if (isVisible) {
        ShowBlastImage(MapToScreen(location), blastAttack, blastAttack.Damage[0]);
        RedrawPlayScreen();
        AnimDelay(DELAY_LONG);
      } else if (Rules.Get.RollChance(PLAYER_HEAR_EXPLOSION_CHANCE))
        AddMessageIfAudibleForPlayer(location, "You hear an explosion");
      ApplyExplosionDamage(in location, 0, blastAttack);
      for (short waveDistance = 1; waveDistance <= blastAttack.Radius; ++waveDistance) {
        if (ApplyExplosionWave(in location, waveDistance, blastAttack)) {
          isVisible = true; // alpha10
          RedrawPlayScreen();
          AnimDelay(DELAY_NORMAL);
        }
      }

      // alpha10 bug fix; clear overlays only if action is visible
      if (isVisible) ClearOverlays();
    }

    private bool ApplyExplosionWave(in Location center, short waveDistance, BlastAttack blast)
    {
      bool flag = false;
      foreach(var i in Enumerable.Range(0, 8 * waveDistance)) {
        flag |= ApplyExplosionWaveSub(in center, center.Position.RadarSweep(waveDistance, i), waveDistance, blast);
      }
      return flag;
    }

    private bool ApplyExplosionWaveSub(in Location blastCenter, Point pt, int waveDistance, BlastAttack blast)
    {
      var center = new Location(blastCenter.Map, pt);
      if (!Map.Canonical(ref center) || !LOS.CanTraceFireLine(in blastCenter, pt, waveDistance)) return false;
      int damage = ApplyExplosionDamage(in center, waveDistance, blast);
      if (!ForceVisibleToPlayer(in center)) return false;
      ShowBlastImage(MapToScreen(pt), blast, damage);
      return true;
    }
#nullable restore

    private int ApplyExplosionDamage(in Location location, int distanceFromBlast, BlastAttack blast)
    {
#if DEBUG
      if (blast.CanDestroyWalls) throw new InvalidOperationException("need to implement explosives destroying walls");
#endif
      int num1 = blast.DamageAt(distanceFromBlast);
      if (num1 <= 0) return 0;
      Map map = location.Map;
      Point pos = location.Position;
      var actorAt = map.GetActorAtExt(pos);
      if (actorAt != null) {
        ExplosionChainReaction(actorAt.Inventory, in location);
        var witnesses = actorAt.PlayersInLOS();
        int dmg = num1 - (actorAt.CurrentDefence.Protection_Hit + actorAt.CurrentDefence.Protection_Shot) / 2;
        if (dmg > 0) {
          if (null != witnesses) {
            RedrawPlayScreen(witnesses, MakePanopticMessage(actorAt, string.Format("is hit for {0} damage!", dmg), Color.Crimson));
          }
          if (actorAt.TakeDamage(dmg) && !actorAt.IsDead) {
            KillActor(null, actorAt, string.Format("explosion {0} damage", dmg));
            if (null != witnesses) {
              RedrawPlayScreen(witnesses, MakePanopticMessage(actorAt, "dies in the explosion!", Color.Crimson));
            }
          }
        } else {
          if (null != witnesses) {
            RedrawPlayScreen(witnesses, MakePanopticMessage(actorAt, "is hit for no damage.", Color.White));
          }
        }
      }
      var itemsAt = map.GetItemsAtExt(pos);
      if (itemsAt != null) {
        ExplosionChainReaction(itemsAt, in location);
        int chance = num1;
        map.RemoveAtExt<Item>(obj => {
            return !obj.IsUnique && !obj.Model.IsUnbreakable && (!(obj is ItemPrimedExplosive exp) || exp.FuseTimeLeft > 0) && Rules.Get.RollChance(chance);
        }, location.Position);
      }
      if (blast.CanDamageObjects) {
        var mapObjectAt = map.GetMapObjectAtExt(pos);
        if (mapObjectAt != null) {
          DoorWindow doorWindow = mapObjectAt as DoorWindow;
          if (mapObjectAt.IsBreakable || doorWindow != null && doorWindow.IsBarricaded) {
            int val2 = num1;
            if (doorWindow != null && doorWindow.IsBarricaded) {
              int num2 = Math.Min(doorWindow.BarricadePoints, val2);
              doorWindow.Barricade(-num2);
              val2 -= num2;
            }
            mapObjectAt.Damage(val2);
          }
        }
      }
      var corpsesAt = map.GetCorpsesAt(pos);
      if (corpsesAt != null) foreach (var c in corpsesAt) c.TakeDamage(num1);
      // XXX implementation of blast.CanDestroyWalls goes here
      return num1;
    }

#nullable enable
    static private void ExplosionChainReaction(Inventory? inv, in Location location)
    {
      if (null == inv || inv.IsEmpty) return;
      List<ItemExplosive>? itemExplosiveList = null;
      List<ItemPrimedExplosive>? itemPrimedExplosiveList = null;
      foreach (Item obj in inv.Items) {
        if (!(obj is ItemExplosive itemExplosive)) continue;
        if (itemExplosive is ItemPrimedExplosive primed) primed.Cook();
        else {
          if (null == itemPrimedExplosiveList) itemPrimedExplosiveList = new();
          (itemExplosiveList ?? new()).Add(itemExplosive);
          var primed_model = GameItems.Cast<ItemExplosiveModel>(itemExplosive.PrimedModelID);
          for (int index = 0; index < obj.Quantity; ++index) {
            itemPrimedExplosiveList.Add((ItemPrimedExplosive)(primed_model.create()));
          }
        }
      }
      if (null != itemExplosiveList) foreach (var it in itemExplosiveList) inv.RemoveAllQuantity(it);
      if (null != itemPrimedExplosiveList) foreach (var it in itemPrimedExplosiveList) location.Map.DropItemAtExt(it, location.Position);
    }
#nullable restore

    public void DoChat(Actor speaker, Actor target)
    {
      if (ForceVisibleToPlayer(speaker) || ForceVisibleToPlayer(target))
        AddMessage(MakeMessage(speaker, VERB_CHAT_WITH.Conjugate(speaker), target));
      if (speaker.IsPlayer || !speaker.CanTradeWith(target)) {
        speaker.SpendActionPoints();
        return;
      }
      DoTrade(speaker.Controller as OrderableAI, target.Controller as OrderableAI);

      // alpha10 recover san after "normal" chat or fast trade
      if (speaker.Model.Abilities.HasSanity) {
        speaker.RegenSanity(Rules.SANITY_RECOVER_CHAT_OR_TRADE);
        if (IsVisibleToPlayer(speaker)) AddMessage(MakeMessage(speaker, string.Format("{0} better after chatting with", VERB_FEEL.Conjugate(speaker)), target));
      }

      if (target.Model.Abilities.HasSanity) {
        target.RegenSanity(Rules.SANITY_RECOVER_CHAT_OR_TRADE);
        if (IsVisibleToPlayer(target)) AddMessage(MakeMessage(target, string.Format("{0} better after chatting with", VERB_FEEL.Conjugate(speaker)), speaker));
      }
    }

#nullable enable
    // intended to be a side-effecting free action.  We intentionally do not support null op here
    public bool DoBackgroundChat(Actor speaker, List<Actor> targets, string speaker_text, string target_text, Action<Actor> op, Sayflags flags = default)
    {
      var chat_competent = targets.FindAll(actor => Rules.CHAT_RADIUS >= Rules.InteractionDistance(speaker.Location, actor.Location));
      if (0 >= chat_competent.Count) return false;
      var target = Rules.Get.DiceRoller.Choose(chat_competent);    // \todo better choice method for this (can postpone until CHAT_RADIUS extended

      if (speaker.IsPlayer && Player!=speaker) {
        PanViewportTo(speaker);
      } else if (target.IsPlayer && Player!=target) {
        PanViewportTo(target);
      }

      bool see_speaker = ForceVisibleToPlayer(speaker);
      bool see_target = see_speaker ? IsVisibleToPlayer(target) : ForceVisibleToPlayer(target);
      bool speaker_heard_clearly = Rules.CHAT_RADIUS >= Rules.InteractionDistance(Player.Location,speaker.Location);
      bool target_heard_clearly = Rules.CHAT_RADIUS >= Rules.InteractionDistance(Player.Location,target.Location);
      flags |= Sayflags.IS_FREE_ACTION;

      if (see_speaker && speaker_heard_clearly) speaker.Say(target, speaker_text, flags);
      if (see_target && target_heard_clearly) target.Say(speaker, target_text, flags);
      if (!speaker_heard_clearly && !target_heard_clearly) {
        if (see_speaker) AddMessage(MakeMessage(speaker, VERB_CHAT_WITH.Conjugate(speaker), target));
        else if (see_target) AddMessage(MakeMessage(target, VERB_CHAT_WITH.Conjugate(target), speaker));
      }

      // not nearly as sanity-restoring as proper chat, but worth something
      speaker.RegenSanity(Rules.SANITY_RECOVER_CHAT_OR_TRADE/15);
      target.RegenSanity(Rules.SANITY_RECOVER_CHAT_OR_TRADE/15);

      // eavesdropping
      var survey = new Rectangle(speaker.Location.Position+Rules.CHAT_RADIUS*Direction.NW, (Point)(2*Rules.CHAT_RADIUS+1));
      survey.DoForEach(pt => {
          if (pt == speaker.Location.Position) return;
          Location loc = new Location(speaker.Location.Map, pt);
          if (Map.Canonical(ref loc)) {
              var overhear = loc.Actor;
              if (null != overhear && target != overhear) op(overhear);
          }
      });
      return true;
    }

    // the two lists are disjoint by construction.
    static private KeyValuePair<List<PlayerController>, List<Actor>>? Witnesses(Location loc, int radius, Func<Actor,bool> ok) {
        List<PlayerController> ret = new();
        List<Actor> ret_viewpoints = new();

        void classify(Actor? actor) {
            if (null != actor && !actor.IsDead && ok(actor)) {
                if (actor.Controller is PlayerController pc) ret.Add(pc);
                else ret_viewpoints.Add(actor);
            }
        };

        var survey = new Rectangle(loc.Position+(short)radius*Direction.NW, (Point)(2*radius+1));
        // lambda function access of loc.Position prevents converting to member function
        survey.DoForEach(pt => {
          Location test = new Location(loc.Map, pt);
          if (Map.Canonical(ref test)) classify(test.Actor);
        });

        var exit = loc.Exit;
        if (null != exit) classify(exit.Location.Actor);

        return (0<ret.Count || 0<ret_viewpoints.Count) ? new(ret, ret_viewpoints) : null;
    }

    static public KeyValuePair<List<PlayerController>, List<Actor>>? WitnessesInLOS(Location view) {
      if (!Map.Canonical(ref view)) return null;

      bool sees(Actor a) { return a.Controller.CanSee(view); };

      return Witnesses(view, Actor.MAX_VISION, sees);
    }


    // the two lists are disjoint by construction.
    // This excludes NPCS, who do not get UI messages
    static private List<PlayerController>? PCsNearby(Location loc, int radius, Func<Actor,bool> ok) {
        List<PlayerController> ret = new();

        void classify(Actor? actor) {
            if (null != actor && !actor.IsDead && actor.Controller is PlayerController pc && ok(actor)) ret.Add(pc);
        };

        var survey = new Rectangle(loc.Position+(short)radius*Direction.NW, (Point)(2*radius+1));
        // lambda function access of loc.Position prevents converting to member function
        survey.DoForEach(pt => {
          Location test = new Location(loc.Map, pt);
          if (Map.Canonical(ref test)) classify(test.Actor);
        });

        var exit = loc.Exit;
        if (null != exit) classify(exit.Location.Actor);

        return 0<ret.Count ? ret : null;
    }

    static public List<PlayerController>? PlayersInLOS(Location view) {
      if (!Map.Canonical(ref view)) return null;

      bool sees(Actor a) { return a.Controller.CanSee(view); };

      return PCsNearby(view, Actor.MAX_VISION, sees);
    }

    public bool DoBackgroundSpeech(Actor speaker, string speaker_text, Action<Actor> op, Sayflags flags = default)
    {
      bool is_awake(Actor a) { return !a.IsSleeping; };

      var chat_competent = speaker.Location.ThoseNearby(Rules.CHAT_RADIUS, is_awake);
      if (null == chat_competent) return false;
      chat_competent.Remove(speaker);
      if (0 >= chat_competent.Count) return false;
      var preferred_chat = chat_competent.Where(a => speaker.IsAlly(a)).ToArray();
      var target = (0 < preferred_chat.Length) ? preferred_chat[Rules.Get.DiceRoller.Roll(0, preferred_chat.Length)] : chat_competent[Rules.Get.DiceRoller.Roll(0, chat_competent.Count)];

      if (speaker.IsPlayer && Player!=speaker) {
        PanViewportTo(speaker);
      } else if (target.IsPlayer && Player!=target) {
        PanViewportTo(target);
      }

      bool see_speaker = ForceVisibleToPlayer(speaker);
      bool see_target = see_speaker ? IsVisibleToPlayer(target) : ForceVisibleToPlayer(target);
      bool speaker_heard_clearly = Rules.CHAT_RADIUS >= Rules.InteractionDistance(Player.Location,speaker.Location);
      bool target_heard_clearly = Rules.CHAT_RADIUS >= Rules.InteractionDistance(Player.Location,target.Location);
      flags |= Sayflags.IS_FREE_ACTION;

      if (see_speaker && speaker_heard_clearly) speaker.Say(target, speaker_text, flags);
      if (!speaker_heard_clearly && !target_heard_clearly) {
        if (see_speaker) AddMessage(MakeMessage(speaker, VERB_CHAT_WITH.Conjugate(speaker), target));
      }

      // not nearly as sanity-restoring as proper chat, but worth something
      speaker.RegenSanity(Rules.SANITY_RECOVER_CHAT_OR_TRADE/15);
      target.RegenSanity(Rules.SANITY_RECOVER_CHAT_OR_TRADE/15);

      // eavesdropping
      foreach(var actor in chat_competent) {
        if (speaker==actor) continue;
        op(actor);
      }
      return true;
    }

    public bool DoBackgroundPoliceRadioChat(Actor speaker, List<Actor> targets, string speaker_text, string target_text, Action<Actor> op, Sayflags flags = default)
    {
      var radio_competent = targets.FindAll(ally => ally.HasActivePoliceRadio);
      if (0 >= radio_competent.Count) return false;
      var target = Rules.Get.DiceRoller.Choose(radio_competent);    // \todo better choice method for this (trust-related)?
      var audience = new HashSet<Actor>();
      var audience2 = new HashSet<Actor>();

      var turnCounter = Session.Get.WorldTime.TurnCounter;
      List<UI.Message> format_msg(string src) {
        List<UI.Message> ret = new();
        string trail = string.Empty;
        while(MESSAGES_LINE_LENGTH < src.Length) {
          int split = src.LastIndexOf(' ');
          if (-1<split) {
            trail = src.Substring(split+1);
            src = src.Substring(0, split);
            ret.Add(new(src, turnCounter, Color.White));
            src = "  "+trail;
          }
        }
        ret.Add(new(src, turnCounter, Color.White));
        return ret;
      }

      var s_PC = speaker.Controller as PlayerController;
      var t_PC = target.Controller as PlayerController;
      var msg_question = format_msg(string.Format("(police radio, {0}) {1}", speaker.Name, speaker_text));
      var msg_answer = format_msg(string.Format("(police radio, {0}) {1}", target.Name, target_text));

      void heard_question(Actor a) { audience.Add(a); }
      void heard_answer(Actor a) { audience2.Add(a); }
      static void PC_message(PlayerController PC, List<UI.Message> msgs) { PC.AddMessages(msgs); }

      void PC_hear_question(PlayerController PC) { PC_message(PC, PC.ControlledActor == speaker ? format_msg(string.Format("({0} using police radio) {1}", speaker.Name, speaker_text)) : msg_question); }
      void PC_heard_question(PlayerController PC) {
        PC_hear_question(PC);
        heard_question(PC.ControlledActor);
      }
      void PC_hear_answer(PlayerController PC) { PC_message(PC, PC.ControlledActor == target ? format_msg(string.Format("({0} using police radio) {1}", target.Name, target_text)) : msg_answer); }
      void PC_heard_answer(PlayerController PC) {
        PC_hear_answer(PC);
        heard_answer(PC.ControlledActor);
      }

      bool reject_conversants(Actor a) {
        return a != speaker && a != target;
      }

      if (null != s_PC) PC_hear_question(s_PC);
      if (null != t_PC) PC_hear_question(t_PC);
      speaker.MessageAllInDistrictByRadio(heard_question, TRUE, PC_heard_question, PC_heard_question, reject_conversants);
      if (null != t_PC) PC_hear_answer(t_PC);
      if (null != s_PC) PC_hear_answer(s_PC);
      target.MessageAllInDistrictByRadio(heard_answer, TRUE, PC_heard_answer, PC_heard_answer, reject_conversants);

      // not nearly as sanity-restoring as proper chat, but worth something
      speaker.RegenSanity(Rules.SANITY_RECOVER_CHAT_OR_TRADE/15);
      target.RegenSanity(Rules.SANITY_RECOVER_CHAT_OR_TRADE/15);

      // eavesdropping
      foreach(var overhear in audience) {
        if (overhear!=speaker && overhear!=target && audience2.Contains(overhear)) op(overhear);
      }

      return true;
    }

    private bool DoTrade(PlayerController pc, Item itSpeaker, Actor target, bool doesTargetCheckForInterestInOffer)
    {
      var speaker = pc.ControlledActor; // i.e., always visible
#if DEBUG
      if (null == itSpeaker) throw new ArgumentNullException(nameof(itSpeaker));    // can fail for AI trades, but AI is now on a different path
#endif
      target.Inventory!.RejectCrossLink(speaker.Inventory!);

      bool wantedItem = true;
      bool flag3 = (target.Controller as ObjectiveAI).IsInterestingTradeItem(speaker, itSpeaker);
      if (target.Leader != speaker && doesTargetCheckForInterestInOffer) wantedItem = flag3;

      if (!wantedItem) { // offered item is not of perceived use
        AddMessage(MakeMessage(target, string.Format("is not interested in {0}.", itSpeaker.TheName)));
        return false;
      }

      var trade = PickItemToTrade(target, pc, itSpeaker); // XX rewrite target
      if (null == trade) {
        AddMessage(MakeMessage(speaker, string.Format("is not interested in {0} items.", target.Name)));
        return false;
      }

      AddMessage(MakeMessage(target, string.Format("{0} {1} for {2}.", VERB_OFFER.Conjugate(target), trade.AName, itSpeaker.AName)));
      AddOverlay(new OverlayPopup(TRADE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));
      RedrawPlayScreen();
      bool acceptDeal = m_UI.WaitYesOrNo();
      ClearOverlays();
//    RedrawPlayScreen(); // redraw handled by AddMessage calls below

      // no need to spam other PCs with above messages
      var witnesses = speaker.PlayersInLOS()!;

      if (!acceptDeal) {
        AddMessage(witnesses, MakePanopticMessage(speaker, string.Format("{0}.", VERB_REFUSE_THE_DEAL.Conjugate(speaker))));
        return false;
      }

      AddMessage(witnesses, MakePanopticMessage(speaker, string.Format("{0}.", VERB_ACCEPT_THE_DEAL.Conjugate(speaker))));

      if (target.Leader == speaker && flag3) target.Say(speaker, "Thank you for this good deal.", Sayflags.IS_FREE_ACTION);
      speaker.Remove(itSpeaker);
      target.Remove(trade);
      speaker.Inventory.AddAll(trade);
      target.Inventory.AddAll(itSpeaker);
      target.Inventory.RejectCrossLink(speaker.Inventory);
      return true;
    }

    private void DoTrade(PlayerController pc, Item itSpeaker, Inventory target)
    {
      var speaker = pc.ControlledActor;
#if DEBUG
      if (null == itSpeaker) throw new ArgumentNullException(nameof(itSpeaker));    // can fail for AI trades, but AI is now on a different path
#endif
      target.RejectCrossLink(speaker.Inventory!);
      var witnesses = speaker.PlayersInLOS()!;

      var trade = PickItemToTrade(target, pc, itSpeaker);
      if (null == trade) return;

      AddMessage(witnesses, MakePanopticMessage(speaker, string.Format("swaps {0} for {1}.", trade.AName, itSpeaker.AName)));

      AddOverlay(new OverlayPopup(TRADE_MODE_TEXT, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));
      RedrawPlayScreen();
      bool acceptDeal = m_UI.WaitYesOrNo();
      ClearOverlays();
//    RedrawPlayScreen(); // redraw will be triggered by messaging, below

      if (!acceptDeal) {
        AddMessage(witnesses, MakePanopticMessage(speaker, string.Format("{0}.", VERB_REFUSE_THE_DEAL.Conjugate(speaker))));
        return;
      }

      speaker.SpendActionPoints();
      speaker.Remove(itSpeaker);
      target.RemoveAllQuantity(trade);
      if (!itSpeaker.IsUseless) target.AddAsMuchAsPossible(itSpeaker);
      if (trade is ItemTrap trap) trap.Desactivate();
      speaker.Inventory.AddAsMuchAsPossible(trade);
      AddMessage(witnesses, MakePanopticMessage(speaker, string.Format("{0}.", VERB_ACCEPT_THE_DEAL.Conjugate(speaker))));
      target.RejectCrossLink(speaker.Inventory);
    }

    private void DoTrade(PlayerController pc, Item itSpeaker, InvOrigin target)
    {
      var speaker = pc.ControlledActor;
      if (speaker.Location != target.Location) {
        if (null != target.loc) {
          if (!speaker.IsCrouching) {
            if (speaker.CanCrouch()) speaker.Crouch();
            else {
              ErrorPopup("too tired to crouch for trading");
              return;
            }
          }
        }
        if (null != target.obj_owner) speaker.StandUp();
      }
      DoTrade(pc, itSpeaker, target.inv);
    }

    private void DoTradeWith(Actor actor, Inventory dest, Item give, Item take)
    {
      var inv = actor.Inventory;
#if DEBUG
      if (null == inv) throw new ArgumentNullException(nameof(actor)+".Inventory");
#endif
      inv.RejectCrossLink(dest);
      var witnesses = _ForceVisibleToPlayer(actor);
      if (null != witnesses) AddMessage(witnesses, MakePanopticMessage(actor, string.Format("swaps {0} for {1}.", give.AName, take.AName)));

      actor.SpendActionPoints();
      actor.Remove(give);
      dest.RemoveAllQuantity(take);
      if (!give.IsUseless) dest.AddAsMuchAsPossible(give);   // mitigate plausible multi-threading issue with stack targeting, but do not actually commit to locks
      inv.AddAsMuchAsPossible(take);
      inv.RejectCrossLink(dest);
    }

    public void DoTradeWithContainer(Actor actor, in ShelfLike obj, Item give, Item take)
    {
      DoTradeWith(actor, obj.Inventory, give, take);
    }

    public void DoTradeWithGround(Actor actor, in Location loc, Item give, Item take)
    {
      var dest = loc.Items;
#if DEBUG
      if (null == dest) throw new ArgumentNullException(nameof(dest));
#endif
      DoTradeWith(actor, dest, give, take);
    }

    /// <remark>speaker's item is Key of trade; target's item is Value</remark>
    private void DoTrade(OrderableAI speaker_c, KeyValuePair<Item, Item>? trade, OrderableAI target_c, bool doesTargetCheckForInterestInOffer)
    {
      var speaker = speaker_c.ControlledActor;
      var target = target_c.ControlledActor;
      bool flag1 = ForceVisibleToPlayer(speaker) || ForceVisibleToPlayer(target);
      speaker.SpendActionPoints();    // prevent hyper-active player trades

      // bail on null item from speaker early
      if (null == trade) {
        if (flag1) AddMessage(MakeMessage(target, string.Format("is not interested in {0} items.", speaker.Name)));
        return;
      }
#if DEBUG
      if (!target.Inventory.Contains(trade.Value.Value)) throw new InvalidOperationException("no longer have item");
      if (!speaker.Inventory.Contains(trade.Value.Key)) throw new InvalidOperationException("no longer have item");
#endif
      speaker.Inventory.RepairContains(trade.Value.Value, "already had ");
      target.Inventory.RepairContains(trade.Value.Key, "already had ");

      bool wantedItem = true;
      bool flag3 = target_c.IsInterestingTradeItem(speaker, trade.Value.Key);
      if (target.Leader != speaker && doesTargetCheckForInterestInOffer) wantedItem = flag3;

      if (!wantedItem)
      { // offered item is not of perceived use
        if (flag1) AddMessage(MakeMessage(target, string.Format("is not interested in {0}.", trade.Value.Key.TheName)));
        return;
      }

      if (flag1)
        AddMessage(MakeMessage(target, string.Format("{0} {1} for {2}.", VERB_OFFER.Conjugate(target), trade.Value.Value.AName, trade.Value.Key.AName)));

      var leader = target.LiveLeader;
      bool acceptDeal = null == leader || (target_c.Directives_nocreate?.CanTrade ?? ActorDirective.CanTrade_default);

      if (!acceptDeal) {
        if (flag1) AddMessage(MakeMessage(speaker, string.Format("{0}.", VERB_REFUSE_THE_DEAL.Conjugate(speaker))));
        return;
      }

      if (flag1) AddMessage(MakeMessage(speaker, string.Format("{0}.", VERB_ACCEPT_THE_DEAL.Conjugate(speaker))));

      if (leader == speaker && flag3) target.Say(speaker, "Thank you for this good deal.", Sayflags.IS_FREE_ACTION);
      Item donate = trade.Value.Key;
      speaker.Remove(donate);
      Item take = trade.Value.Value;
      target.Remove(take);
      speaker.Inventory.AddAll(take);
      target.Inventory.AddAll(donate);
    }

    public void DoTrade(OrderableAI speaker_c, OrderableAI target_c, Item give, Item take)
    {
      DoTrade(speaker_c, new KeyValuePair<Item, Item>(give, take), target_c, false);
    }

    public void DoTrade(OrderableAI speaker_c, OrderableAI target_c)
    {   // precondition: !speaker.IsPlayer (need different implementation)
      var target = target_c.ControlledActor;
#if DEBUG
      if (!speaker_c.ControlledActor.CanTradeWith(target, out string reason)) throw new ArgumentOutOfRangeException("Trading not supported",reason);
#endif
      DoTrade(speaker_c, PickItemsToTrade(speaker_c, target), target_c, false);
    }

    static private KeyValuePair<Item,Item>? PickItemsToTrade(OrderableAI speaker_c, Actor buyer)
    {
      var negotiate = speaker_c.TradeOptions(buyer);
      if (null == negotiate) return null;
      return Rules.Get.DiceRoller.Choose(negotiate);
    }

    static public KeyValuePair<Item,Item>? PickItemsToTrade(Actor speaker, Actor buyer, Item gift)
    {
      var buyer_offers = buyer.GetInterestingTradeableItems(speaker);  // charisma check involved for these
      if (null == buyer_offers || 0 >= buyer_offers.Count) return null;
      var negotiate = new List<KeyValuePair<Item,Item>>(buyer_offers.Count);   // relies on "small" inventory to avoid arithmetic overflow
      foreach(var b_item in buyer_offers) {
        if (ObjectiveAI.TradeVeto(gift,b_item)) continue;
        if (ObjectiveAI.TradeVeto(b_item,gift)) continue;
        // charisma can't do everything
        negotiate.Add(new KeyValuePair<Item,Item>(gift,b_item));
      }
      if (0 >= negotiate.Count) return null;
      return Rules.Get.DiceRoller.Choose(negotiate);
    }

    static public bool CanPickItemsToTrade(Actor speaker, Actor buyer, Item gift)
    {
      var buyer_offers = buyer.GetInterestingTradeableItems(speaker);  // charisma check involved for these
      if (null == buyer_offers || 0 >=buyer_offers.Count) return false;
      foreach(var b_item in buyer_offers) {
        if (ObjectiveAI.TradeVeto(gift,b_item)) continue;
        if (ObjectiveAI.TradeVeto(b_item,gift)) continue;
        // charisma can't do everything
        return true;
      }
      return false;
    }

    private Item? PickItemToTrade(Actor speaker, PlayerController buyer_c, Item itSpeaker)
    {
      var buyer = buyer_c.ControlledActor;
#if DEBUG
//    if (null == itSpeaker) throw new ArgumentNullException(nameof(itSpeaker));    // can fail for AI trades, but AI is now on a different path
#endif
      var objList = speaker.GetInterestingTradeableItems(buyer); // player as speaker would trivialize
      if (objList == null || 0>=objList.Count) return null;
      // following is AI-only
      IEnumerable<Item> tmp = (speaker.IsPlayer ? objList : objList.Where(it=>itSpeaker.ModelID != it.ModelID));
      // XXX disallow clearly non-mutual advantage trades
      switch(itSpeaker.ModelID)
      {
      // two weapons for the ammo
      case Item_IDs.RANGED_PRECISION_RIFLE:
      case Item_IDs.RANGED_ARMY_RIFLE:
        tmp = tmp.Where(it=> it.ModelID != Item_IDs.AMMO_HEAVY_RIFLE);
        break;
      case Item_IDs.AMMO_HEAVY_RIFLE:
        tmp = tmp.Where(it=> it.ModelID != Item_IDs.RANGED_PRECISION_RIFLE && it.Model.ID != Item_IDs.RANGED_ARMY_RIFLE);
        break;
      case Item_IDs.RANGED_PISTOL:
      case Item_IDs.RANGED_KOLT_REVOLVER:
        tmp = tmp.Where(it=> it.ModelID != Item_IDs.AMMO_LIGHT_PISTOL);
        break;
      case Item_IDs.AMMO_LIGHT_PISTOL:
        tmp = tmp.Where(it=> it.ModelID != Item_IDs.RANGED_PISTOL && it.Model.ID != Item_IDs.RANGED_KOLT_REVOLVER);
        break;
      // one weapon for the ammo
      case Item_IDs.RANGED_ARMY_PISTOL:
        tmp = tmp.Where(it=> it.ModelID != Item_IDs.AMMO_HEAVY_PISTOL);
        break;
      case Item_IDs.AMMO_HEAVY_PISTOL:
        tmp = tmp.Where(it=> it.ModelID != Item_IDs.RANGED_ARMY_PISTOL);
        break;
      case Item_IDs.RANGED_HUNTING_CROSSBOW:
        tmp = tmp.Where(it=> it.ModelID != Item_IDs.AMMO_BOLTS);
        break;
      case Item_IDs.AMMO_BOLTS:
        tmp = tmp.Where(it=> it.ModelID != Item_IDs.RANGED_HUNTING_CROSSBOW);
        break;
      case Item_IDs.RANGED_HUNTING_RIFLE:
        tmp = tmp.Where(it=> it.ModelID != Item_IDs.AMMO_LIGHT_RIFLE);
        break;
      case Item_IDs.AMMO_LIGHT_RIFLE:
        tmp = tmp.Where(it=> it.ModelID != Item_IDs.RANGED_HUNTING_RIFLE);
        break;
      case Item_IDs.RANGED_SHOTGUN:
        tmp = tmp.Where(it=> it.ModelID != Item_IDs.AMMO_SHOTGUN);
        break;
      case Item_IDs.AMMO_SHOTGUN:
        tmp = tmp.Where(it=> it.ModelID != Item_IDs.RANGED_SHOTGUN);
        break;
      // flashlights.  larger radius and longer duration are independently better...do not trade if both are worse
      case Item_IDs.LIGHT_FLASHLIGHT:
        tmp = tmp.Where(it=> it.ModelID != Item_IDs.LIGHT_BIG_FLASHLIGHT || (itSpeaker as BatteryPowered).Batteries<=(it as BatteryPowered).Batteries);
        break;
      case Item_IDs.LIGHT_BIG_FLASHLIGHT:
        tmp = tmp.Where(it=> it.ModelID != Item_IDs.LIGHT_FLASHLIGHT || (itSpeaker as BatteryPowered).Batteries>=(it as BatteryPowered).Batteries);
        break;
      }
      if (!tmp.Any()) return null;
      objList = tmp.ToList();

      string label(int index) { return string.Format("{0}/{1} {2}.", index + 1, objList.Count, DescribeItemShort(objList[index])); }

      Item? ret = null;
      bool details(int index) {
        ret = objList[index];
        return true;
      }

      PagedPopup("Trading for ...", objList.Count, label, details);
      return ret;
    }

    private Item? PickItemToTrade(Inventory speaker, PlayerController buyer_c, Item itSpeaker)
    {
      var buyer = buyer_c.ControlledActor;
#if DEBUG
//    if (null == itSpeaker) throw new ArgumentNullException(nameof(itSpeaker));    // can fail for AI trades, but AI is now on a different path
#endif
      List<Item> objList = speaker.Items.ToList(); // inventory has no mentality, trivialize
      if (objList == null || 0>=objList.Count) return null;

      string label(int index) { return string.Format("{0}/{1} {2}.", index + 1, objList.Count, DescribeItemShort(objList[index])); }

      Item? ret = null;
      bool details(int index) {
        ret = objList[index];
        return true;
      }

      PagedPopup("Trading for ...", objList.Count, label, details);
      return ret;
    }

    public void DoShout(Actor speaker, string? text)
    {
      speaker.SpendActionPoints();
      OnLoudNoise(speaker.Location, "A SHOUT");
      // Alpha 10- also allowed shouting through phone...we need to re-think this
      if (/* !AreLinkedByPhone(speaker, Player) && */ !ForceVisibleToPlayer(speaker)) return;
      if (Player == speaker.Leader && Player.Controller is PlayerController pc) {
        pc.Messages.Clear();
        AddOverlay((new OverlayRect(Color.Yellow, new GDI_Rectangle(MapToScreen(speaker.Location), SIZE_OF_ACTOR))));
        DoEmote(speaker, string.Format("{0}!!", VERB_RAISE_ALARM.Conjugate(speaker)), true);
        if (null != text) DoEmote(speaker, text, true);
        ClearOverlays();
        pc.AddMessagePressEnter();
      } else {
        var verb = VERB_SHOUT.Conjugate(speaker);
        if (null == text)
          DoEmote(speaker, string.Format("{0}!", verb), true);
        else
          DoEmote(speaker, string.Format("{0} \"{1}\"", verb, text), true);
      }
    }

    public void DoEmote(Actor actor, string text, bool isDanger = false)
    {
        var witnesses = actor.PlayersInLOS();
        if (null != witnesses) {
            RedrawPlayScreen(witnesses, MakePanopticMessage(actor, string.Format(": {0}", text), isDanger ? SAYOREMOTE_DANGER_COLOR : SAYOREMOTE_NORMAL_COLOR));
        }
    }

    private Item? Choose(Inventory? inv, string prompt) {
      if (null == inv) return null;
      if (inv.IsEmpty) return null;
      if (2 > inv.CountItems) return inv.TopItem!;

      Item? ret = null;

      string label(int index) { return string.Format("{0}/{1} {2}.", index + 1, inv.CountItems, DescribeItemShort(inv[index])); }
      bool details(int index) { ret = inv[index]!; return true; }

      PagedPopup(prompt, inv.CountItems, label, details);

      return ret;
    }

    public void HandlePlayerTakeItem(PlayerController pc, Data.Model.InvOrigin src)
    {
      var player = pc.ControlledActor;
      var inv = src.Inventory;
#if DEBUG
      if (inv.IsEmpty) throw new ArgumentNullException(nameof(src)+".inv");
      if (null == src.obj_owner && null == src.loc) throw new InvalidOperationException("only take from unresisting targets");
      if (1 < Rules.GridDistance(player.Location, src.Location)) throw new ArgumentOutOfRangeException(nameof(src), src, "not adjacent");
#endif

      var it = Choose(inv, "Taking...");
      if (null != it) Interpret(new _Action.TakeItem(Player, in src, it));
    }

    public void UI_TakeItem(Actor actor, Location loc, Item it)
    {
      var witnesses = _ForceVisibleToPlayer(actor);
      var see_take = PlayersInLOS(loc)?.SetDifference(witnesses);

      if (null != witnesses) {
        RedrawPlayScreen(witnesses, MakePanopticMessage(actor, VERB_TAKE.Conjugate(actor), it));
        if (null != see_take) {
            var msg = MakeMessage(see_take[0], actor, VERB_TAKE.Conjugate(actor), it);
            foreach (var pc in see_take) pc.Messages.Add(msg);
        }
      }
    }

    public void DoGiveItemTo(OrderableAI src_ordai, OrderableAI t_ordai, Item gift, Item? received)
    {
      var actor = src_ordai.ControlledActor;
#if DEBUG
      if (!actor.Inventory.Contains(gift)) throw new InvalidOperationException("no longer had gift");
#endif
      var target = t_ordai.ControlledActor;
      if (null != received) {
#if DEBUG
        if (!target.Inventory.Contains(received)) throw new InvalidOperationException("no longer had recieved");
#endif
        actor.Inventory.RepairContains(received, "already had received ");
      }
      target.Inventory.RepairContains(gift, "already had ");

      bool do_not_crash_on_target_turn = (0 < target.ActionPoints && target.Location.Map.NextActorToAct == target);  // XXX \todo fix this in cross-map case, or verify that this inexplicably works anyway
      // try to trade with NPC first
      {
        var trade = PickItemsToTrade(actor, target, gift);
        if (null != trade) {
          if (do_not_crash_on_target_turn) DoWait(target);
#if DEBUG
          if (!target.Inventory.Contains(trade.Value.Value)) throw new InvalidOperationException("no longer had recieved");
#endif
          actor.Inventory.RepairContains(trade.Value.Value, "already had recieved ");
          DoTrade(src_ordai, trade, t_ordai, false);
          return;
        }
      }
      // \todo trade with player path (blocked by aligning trade UI with RS Alpha 10.1)

      // If cannot trade, outright give
      if (target.Inventory.IsFull && !target.CanGet(gift)) {
        if (null == received) { // \todo refactor this -- repeat block from ActionGiveItem
          var recover = t_ordai.BehaviorMakeRoomFor(gift,actor.Location); // unsure if this works cross-map
          if (null != recover && !recover.IsLegal() && recover is ActionUseItem) recover = t_ordai.BehaviorMakeRoomFor(gift); // ammo can get confused, evidently

          static Item? parse_recovery(ActorAction? act) {
            if (act is Resolvable chain) return parse_recovery(chain.ConcreteAction); // historically ActionChain
            if (act is ActorGive trade) return trade.Give;
            if (act is Use<Item> use) return use.Use;
            return null;
          }
          received = parse_recovery(recover);
          if (null != received) {
#if DEBUG
            if (!target.Inventory.Contains(received)) throw new InvalidOperationException("no longer had recieved");
#endif
            actor.Inventory.RepairContains(received, "already had received ");
          }
        }

        if (null != received) {
          if (do_not_crash_on_target_turn) DoWait(target);
          DoTrade(src_ordai, new KeyValuePair<Item,Item>(gift,received), t_ordai, false);
          return;
        }
      }
      if (do_not_crash_on_target_turn) DoWait(target);
      DoGiveItemTo(actor,target,gift);
    }
#nullable restore

    public void DoGiveItemTo(Actor actor, Actor target, Item gift)
    {
      actor.SpendActionPoints();
      if (target.Leader == actor) {
        bool flag = (target.Controller as ObjectiveAI).IsInterestingItem(gift);
        target.Say(actor, flag ? "Thank you, I really needed that!" : "Thanks I guess...", Sayflags.IS_FREE_ACTION);
        ModifyActorTrustInLeader(target, flag ? Rules.TRUST_GOOD_GIFT_INCREASE : Rules.TRUST_MISC_GIFT_INCREASE, true);
      } else if (actor.Leader == target) {
        target.Say(actor, "Well, here it is...", Sayflags.IS_FREE_ACTION);
        ModifyActorTrustInLeader(actor, Rules.TRUST_GIVE_ITEM_ORDER_PENALTY, true);
      }

      if (gift is ItemTrap trap) trap.Desactivate();
      gift.UnequippedBy(actor, false);
      actor.Inventory.Transfer(gift, target.Inventory);
      target.SpendActionPoints();
      if (!gift.Model.DontAutoEquip && target.CanEquip(gift) && target.GetEquippedItem(gift.Model.EquipmentPart) == null)
        gift.EquippedBy(target);

      if (ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(target))
        AddMessage(MakeMessage(actor, string.Format("{0} {1} to", VERB_GIVE.Conjugate(actor), gift.TheName), target));
    }

    public void DoPutItemInContainer(Actor actor, ShelfLike container, Item gift)
    {
      actor.SpendActionPoints();
      if (container.PutItemIn(gift)) actor.Remove(gift, false);

      if (ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(container))
        AddMessage(MakeMessage(actor, string.Format("{0} {1} away", VERB_PUT.Conjugate(actor), gift.TheName)));
    }

    public void DoDropItem(Actor actor, Item it)
    {
      if (actor.CanUnequip(it)) it.UnequippedBy(actor,false);
      actor.SpendActionPoints();
      Item obj = it;
      if (it is ItemTrap trap) {
        ItemTrap clone = trap.Clone();
        if (trap.IsActivated) clone.Activate(actor);  // alpha10
        obj = clone;
        clone.Activate(actor); // alpha10
        trap.Desactivate();  // alpha10
#if FALSE_POSITIVE
        if (!clone.IsActivated) throw new ArgumentOutOfRangeException(nameof(it)," trap being dropped intentionally must be activated");
#endif
      }
      var witnesses = actor.PlayersInLOS();
      if (it.IsUseless) {
        DiscardItem(actor, it);
        if (null != witnesses) RedrawPlayScreen(witnesses, MakePanopticMessage(actor, VERB_DISCARD.Conjugate(actor), it));
        return;
      }
      // XXX using containers can go here, but we may want a different action anyway
      if (obj == it) DropItem(actor, it);
      else DropCloneItem(actor, it, obj);
      if (null != witnesses) RedrawPlayScreen(witnesses, MakePanopticMessage(actor, VERB_DROP.Conjugate(actor), obj));
      actor.Location.Items?.RejectCrossLink(actor.Inventory);
    }

    public void DoDropItem(Actor actor, Item it, Location dest)
    {
      if (actor.Location != dest) {
          if (!actor.IsCrouching) {
            if (actor.CanCrouch()) actor.Crouch();
            else {
              ErrorPopup("too tired to crouch for trading");
              return;
            }
          }
      }

      if (actor.CanUnequip(it)) it.UnequippedBy(actor,false);
      actor.SpendActionPoints();
      Item obj = it;
      if (it is ItemTrap trap) {
        ItemTrap clone = trap.Clone();
        if (trap.IsActivated) clone.Activate(actor);  // alpha10
        obj = clone;
        clone.Activate(actor); // alpha10
        trap.Desactivate();  // alpha10
#if FALSE_POSITIVE
        if (!clone.IsActivated) throw new ArgumentOutOfRangeException(nameof(it)," trap being dropped intentionally must be activated");
#endif
      }
      var witnesses = actor.PlayersInLOS();
      if (it.IsUseless) {
        DiscardItem(actor, it);
        if (null != witnesses) RedrawPlayScreen(witnesses, MakePanopticMessage(actor, VERB_DISCARD.Conjugate(actor), it));
        return;
      }
      // XXX using containers can go here, but we may want a different action anyway
      if (obj == it) DropItem(actor, it, in dest);
      else DropCloneItem(actor, it, obj, in dest);
      if (null != witnesses) RedrawPlayScreen(witnesses, MakePanopticMessage(actor, VERB_DROP.Conjugate(actor), obj));
      dest.Items?.RejectCrossLink(actor.Inventory);
    }

    static private void DiscardItem(Actor actor, Item it)
    {
      actor.Inventory.RemoveAllQuantity(it);
      it.Unequip();
    }

    static private void _Drop(Item it, in Location dest)
    {
      var invspec = dest.DropOntoInventory();

      if (null == invspec.obj_owner) {
         dest.Drop(it);
         it.Unequip();
         return;
      }

      if (!invspec.Inventory.IsFull) {
         invspec.Inventory.AddAll(it);
         it.Unequip();
         return;
      }

      // \todo XXX replace this failover later
      dest.Drop(it);
      it.Unequip();
    }

    static private void DropItem(Actor actor, Item it)
    {
      actor.Inventory.RemoveAllQuantity(it);
      _Drop(it, actor.Location);
    }

    static private void DropItem(Actor actor, Item it, in Location dest)
    {
      actor.Inventory.RemoveAllQuantity(it);
      _Drop(it, in dest);
    }


    static private void DropCloneItem(Actor actor, Item it, Item clone)
    {
      actor.Inventory.Consume(it);
      _Drop(clone, actor.Location);
    }

    static private void DropCloneItem(Actor actor, Item it, Item clone, in Location dest)
    {
      actor.Inventory.Consume(it);
      _Drop(clone, in dest);
    }

    public bool DoUnload(Actor actor, InventorySource<ItemRangedWeapon> src, InvOrigin dest)
    {
      var ammo = ItemAmmo.make(src.it.ModelID);
      ammo.Quantity = src.it.Ammo;
      if (null != dest.inv) {
        var added = dest.inv.AddAsMuchAsPossible(ammo);
        if (0 >= added) return false;
        src.it.Ammo -= added;
      } else {
        dest.loc.Value.Drop(ammo);
        src.it.Ammo = 0;
      }

      actor.SpendActionPoints();
      src.fireChange();
      dest.fireChange();

      var witnesses = actor.PlayersInLOS();
      if (null!=witnesses) RedrawPlayScreen(witnesses, MakePanopticMessage(actor, VERB_UNLOAD.Conjugate(actor), src.it));
      return true;
    }

    // At low time resolutions, it would make sense to allow using from adjacent reachable inventories
    public void DoUseItem(Actor actor, Item it)
    {
      // alpha10 defrag ai inventories
      bool defragInventory = !actor.IsPlayer && it.Model.IsStackable;

      if (it is ItemFood food) DoUseFoodItem(actor, food);
      else if (it is ItemMedicine med) DoUseMedicineItem(actor, med);
      else if (it is ItemAmmo am) am.Use(actor, actor.Inventory);
      else if (it is ItemTrap trap) trap.Use(actor, actor.Inventory);
      else if (it is ItemEntertainment ent) DoUseEntertainmentItem(actor, ent);

      // alpha10 defrag ai inventories
      if (defragInventory) actor.Inventory.Defrag();
      if (actor.IsPlayer) RedrawPlayScreen();
    }

#nullable enable
    private void DoUseFoodItem(Actor actor, ItemFood food)
    {
      if (Player == actor && actor.FoodPoints >= actor.MaxFood - 1) ErrorPopup("Don't waste food!");
      else food.Use(actor, actor.Inventory);
    }

    private void DoUseMedicineItem(Actor actor, ItemMedicine med)
    {
      if (Player == actor) {
        int num1 = actor.MaxHPs - actor.HitPoints;
        int num2 = actor.MaxSTA - actor.StaminaPoints;
        int num3 = actor.MaxSleep - 2 - actor.SleepPoints;
        int infection = actor.Infection;
        int num4 = actor.MaxSanity - actor.Sanity;
        if ((num1 <= 0 || med.Healing <= 0) && (num2 <= 0 || med.StaminaBoost <= 0) && ((num3 <= 0 || med.SleepBoost <= 0) && (infection <= 0 || med.InfectionCure <= 0)) && (num4 <= 0 || med.SanityCure <= 0))
        {
          ErrorPopup("Don't waste medicine!");
          return;
        }
      }
      med.Use(actor, actor.Inventory);
    }

    public void DoUseEntertainmentItem(Actor actor, ItemEntertainment ent)
    {
      var witnesses = _ForceVisibleToPlayer(actor);
      actor.SpendActionPoints();
      actor.RegenSanity(actor.ScaleSanRegen(ent.Model.Value));
      switch(ent.ModelID) {
      case Item_IDs.ENT_CHAR_GUARD_MANUAL:
        if (Player==actor) {  // this manual is highly informative
          List<string> display = new();
          display.Add("It appears CHAR management had done some contingency planning for what is currently happening.");
          display.Add("The absence of an exemption for police when clearing CHAR offices in event of losing communications with HQ,");
          display.Add("seems imprudent.  Even if there had been a law enforcement raid on the HQ.");
          display.Add("Projected looting:");
          display.Add("  Bikers: arrive day "+BIKERS_RAID_DAY.ToString()+", need "+BIKERS_RAID_DAYS_GAP.ToString()+" days to reorganize; bases overrun by zombies day "+BIKERS_END_DAY.ToString());
          display.Add("  Gangsters: arrive day "+GANGSTAS_RAID_DAY.ToString()+", need "+GANGSTAS_RAID_DAYS_GAP.ToString()+" days to reorganize; bases overrun by zombies day "+GANGSTAS_END_DAY.ToString());
          display.Add("Military response:");
          display.Add("  National Guard: arrive day "+NATGUARD_DAY.ToString()+"; zombies arrive at the local National Guard base day "+NATGUARD_END_DAY.ToString());
          display.Add("  Supply drops: start day "+ARMY_SUPPLIES_DAY.ToString()+".");
          display.Add("  Black ops: arrive day "+BLACKOPS_RAID_DAY.ToString()+", need "+BLACKOPS_RAID_DAY_GAP.ToString()+" days to reorganize.");
          display.Add("Refugees:");
          display.Add("  Civilians and police are expected to arrive around noon daily:");
          display.Add("  Survivalists: arrive day "+SURVIVORS_BAND_DAY.ToString()+", need "+SURVIVORS_BAND_DAY_GAP.ToString()+" days to reorganize.");
          ShowSpecialDialogue(Player,display.ToArray());
        }
#if PROTOTYPE
        if (GameFactions.IDs.ThePolice == actor.Faction.ID) {   // XXX police will realize that the guards are just out of communication with CHAR HQ as they are; CHAR guards' no-comm orders also target police
        }
#endif
        break;
      }
      if (null != witnesses) AddMessage(witnesses, MakePanopticMessage(actor, VERB_ENJOY.Conjugate(actor), ent));
      int boreChance = ent.Model.BoreChance;
      if (boreChance == 100) {
        actor.Inventory!.Consume(ent);
        if (null != witnesses) AddMessage(witnesses, MakePanopticMessage(actor, VERB_DISCARD.Conjugate(actor), ent));
      } else if (Rules.Get.RollChance(boreChance)) {
        ent.AddBoringFor(actor);
        if (null != witnesses) AddMessage(witnesses, MakePanopticMessage(actor, string.Format("{0} now bored of {1}.", VERB_BE.Conjugate(actor), ent.TheName)));
      }
      if (null != witnesses) RedrawPlayScreen();
    }

    public void UI_RechargeItemBattery(Actor actor, Item it)
    {
      var witnesses = actor.PlayersInLOS();
      if (null != witnesses) RedrawPlayScreen(witnesses, MakePanopticMessage(actor, VERB_RECHARGE.Conjugate(actor), it, " batteries."));
    }

    public void DoOpenDoor(Actor actor, DoorWindow door)
    {
      door.SetState(DoorWindow.STATE_OPEN);
      actor.SpendActionPoints();
      if (ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(door)) RedrawPlayScreen(MakeMessage(actor, VERB_OPEN.Conjugate(actor), door));    }

    public void DoCloseDoor(Actor actor, DoorWindow door, bool free)
    {
      door.SetState(DoorWindow.STATE_CLOSED);
      if (!free) actor.SpendActionPoints();
      if (ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(door)) RedrawPlayScreen(MakeMessage(actor, VERB_CLOSE.Conjugate(actor), door));
    }

    public void DoBarricadeDoor(Actor actor, DoorWindow door)
    {
      var inv = actor.Inventory!;
      var barricadeMaterial = inv.GetSmallestStackOf<ItemBarricadeMaterial>();
      inv.Consume(barricadeMaterial);
      door.Barricade(actor.ScaleBarricadingPoints(barricadeMaterial.Model.BarricadingValue));
      actor.SpendActionPoints();
      if (ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(door)) RedrawPlayScreen(MakeMessage(actor, VERB_BARRICADE.Conjugate(actor), door));
    }

    public void DoBuildFortification(Actor actor, in Point buildPos, bool isLarge)
    {
      actor.SpendActionPoints();
      var inv = actor.Inventory!;
      int num = actor.BarricadingMaterialNeedForFortification(isLarge);
      for (int index = 0; index < num; ++index) {
        inv.Consume(inv.GetSmallestStackOf<ItemBarricadeMaterial>());
      }
      Fortification fortification = isLarge ? BaseMapGenerator.MakeObjLargeFortification() : BaseMapGenerator.MakeObjSmallFortification();
      actor.Location.Map.PlaceAt(fortification, buildPos);  // XXX cross-map fortification change target

      bool is_visible = ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(fortification);
      if (is_visible) AddMessage(MakeMessage(actor, string.Format("{0} {1}.", VERB_BUILD.Conjugate(actor), fortification.AName)));
      CheckMapObjectTriggersTraps(actor.Location.Map, buildPos);
      if (is_visible) RedrawPlayScreen();
    }

    public void DoRepairFortification(Actor actor, Fortification fort)
    {
      var inv = actor.Inventory!;
      var barricadeMaterial = inv.GetSmallestStackOf<ItemBarricadeMaterial>();
      inv.Consume(barricadeMaterial);
      actor.SpendActionPoints();
      fort.Repair(actor.ScaleBarricadingPoints(barricadeMaterial.Model.BarricadingValue));
      if (ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(fort)) RedrawPlayScreen(MakeMessage(actor, VERB_REPAIR.Conjugate(actor), fort));
    }

    public void DoSwitchPowerGenerator(Actor actor, PowerGenerator powGen)
    {
      actor.SpendActionPoints();
      bool have_messaged = false;
      if (ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(powGen)) {
        AddMessage(MakeMessage(actor, VERB_SWITCH.Conjugate(actor), powGen, powGen.IsOn ? " off." : " on."));
        have_messaged = true;
      }
      powGen.TogglePower(actor);
      if (!have_messaged) {
        if (ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(powGen))
          AddMessage(MakeMessage(actor, VERB_SWITCH.Conjugate(actor), powGen, powGen.IsOn ? " off." : " on."));
      }
      RedrawPlayScreen();
    }
#nullable restore

    public void DoBreak(Actor actor, MapObject mapObj)
    {
      // NPCs know to use their best melee weapon
      if (!actor.IsPlayer) {
        var bestMeleeWeapon = actor.GetBestMeleeWeapon(mapObj);
        if (null!=bestMeleeWeapon) {
          if ((actor.GetEquippedWeapon() as ItemMeleeWeapon) != bestMeleeWeapon) bestMeleeWeapon.EquippedBy(actor);
        }
      }

        var a_witness = actor.PlayersInLOS();
        var d_witness = mapObj.PlayersInLOS();
        var ad_witness = a_witness?.Intersect(d_witness);
        var a_only_witness = a_witness?.SetDifference(ad_witness);
        var d_only_witness = d_witness?.SetDifference(ad_witness);
        KeyValuePair<bool,bool> sees = ForceVisibleToPlayer(actor, a_only_witness, ad_witness, d_only_witness);
        PlayerController? a_pc = IsPlayer(actor) ? actor.Controller as PlayerController : null;
        bool isVisible = sees.Key || sees.Value;

      Attack attack = actor.MeleeAttack(mapObj);
      if (mapObj is DoorWindow doorWindow && doorWindow.IsBarricaded) {
        actor.SpendActionPoints();
        actor.SpendStaminaPoints(Rules.STAMINA_COST_MELEE_ATTACK);
        doorWindow.Barricade(-attack.DamageValue);
        OnLoudNoise(doorWindow.Location, "A loud *BASH*");
        if (isVisible) {
          AddMessage(MakeMessage(actor, string.Format("{0} the barricade.", VERB_BASH.Conjugate(actor))));
        }
        bool player_knows(Actor a) {
          return     a.Controller.CanSee(actor.Location) // we already checked the door/window visibility, it's the sound origin
                 && !Rules.Get.RollChance(PLAYER_HEAR_BASH_CHANCE);  // not clear enough; no message
        }
        void react(Actor a) {
          if (!(a.Controller is OrderableAI ai)) return;  // not that smart (ultimately would want to extend to handler FeralDogAI
          if (!a.IsEnemyOf(actor)) return;   // not relevant \todo Civilian vs. GangAI may disagree with this, but would want to retreat
          if (a.Controller is CHARGuardAI && !IsInCHARProperty(actor.Location)) return; // CHAR guards generally ignore enemies not within a CHAR office. \todo should not be ignoring threats just outside of the doors
          if (    ai.IsDistracted(ObjectiveAI.ReactionCode.NONE)
              || !Rules.Get.RollChance(PLAYER_HEAR_BASH_CHANCE))  // not clear enough
            return;
          if (!ai.CombatUnready()) {  // \todo should discriminate between cops/soldiers/CHAR guards and civilians here; civilians may avoid even if combat-ready
            /* if (a.IsEnemyOf(actor)) */ ai.Terminate(actor);
            return;
          }
          // \todo: get away from the fighting
        }
        PropagateSound(doorWindow.Location, "You hear someone bashing barricades",react,player_knows);
        if (doorWindow.IsBarricaded) {
          if (actor.Controller is ObjectiveAI ai) ai.DeBarricade(doorWindow);
        }
        return;
      } else {
        actor.SpendActionPoints();
        actor.SpendStaminaPoints(Rules.STAMINA_COST_MELEE_ATTACK);
        bool flag = mapObj.Damage(attack.DamageValue);
        if (!flag) OnLoudNoise(mapObj.Location, "A loud *CRASH*");
        bool player1 = sees.Key;
        bool player2 = sees.Value;
        if (isVisible) {
          if (player1) AddOverlay(new OverlayRect(Color.Yellow, new GDI_Rectangle(MapToScreen(actor.Location), SIZE_OF_ACTOR)));
          if (player2) AddOverlay(new OverlayRect(Color.Red, new GDI_Rectangle(MapToScreen(mapObj.Location), SIZE_OF_TILE)));
          if (flag) {
            if (player1) AddOverlay(new OverlayImage(MapToScreen(actor.Location), GameImages.ICON_MELEE_ATTACK));
            if (player2) AddOverlay(new OverlayImage(MapToScreen(mapObj.Location), GameImages.ICON_KILLED));
            ImportantMessage(a_only_witness, ad_witness, d_only_witness, MakeMessages(actor, VERB_BREAK.Conjugate(actor), mapObj), DELAY_LONG);
          } else {
            if (player1) AddOverlay(new OverlayImage(MapToScreen(actor.Location), GameImages.ICON_MELEE_ATTACK));
            if (player2) AddOverlay(new OverlayImage(MapToScreen(mapObj.Location), GameImages.ICON_MELEE_DAMAGE));
            ImportantMessage(a_only_witness, ad_witness, d_only_witness, MakeMessages(actor, VERB_BASH.Conjugate(actor), mapObj), actor.IsPlayer ? DELAY_NORMAL : DELAY_SHORT);
          }
          ClearOverlays();
        }

        bool player_knows(Actor a) {
          return     a.Controller.CanSee(actor.Location) // we already checked the object visibility, it's the sound origin
                 && !Rules.Get.RollChance(flag ? PLAYER_HEAR_BREAK_CHANCE : PLAYER_HEAR_BASH_CHANCE);  // not clear enough; no message
        }
        void react(Actor a) {
          if (!(a.Controller is OrderableAI ai)) return;  // not that smart (ultimately would want to extend to handler FeralDogAI
          if (!a.IsEnemyOf(actor)) return;   // not relevant
          if (a.Controller is CHARGuardAI && !IsInCHARProperty(actor.Location)) return; // CHAR guards generally ignore enemies not within a CHAR office. \todo should not be ignoring threats just outside of the doors
          if (    ai.IsDistracted(ObjectiveAI.ReactionCode.NONE)
              || !Rules.Get.RollChance(flag ? PLAYER_HEAR_BREAK_CHANCE : PLAYER_HEAR_BASH_CHANCE))  // not clear enough
            return;
          if (!ai.CombatUnready()) {  // \todo should discriminate between cops/soldiers/CHAR guards and civilians here; civilians may avoid even if combat-ready
            ai.Terminate(actor);
            return;
          }
          // \todo: get away from the fighting
        }
        PropagateSound(mapObj.Location,(flag ? "You hear someone breaking furniture" : "You hear someone bashing furniture"),react,player_knows);
      }
    }

    private void DoPushPullFollowersHelp(Actor actor, MapObject mapObj, bool isPulling, ref int staCost)    // alpha10
    {
      bool isVisibleMobj = IsVisibleToPlayer(mapObj);

      List<Actor> helpers = null;
      foreach (Actor fo in actor.Followers) {
        // follower can help if: not sleeping, idle and adj to map object.
        if (!fo.IsSleeping && fo.IsAvailableToHelp && Rules.IsAdjacent(fo.Location, mapObj.Location)) {
          (helpers ??= new List<Actor>(actor.CountFollowers)).Add(fo);
        }
      }
      if (helpers != null) {
        // share the sta cost.
        staCost = mapObj.Weight / (1 + helpers.Count);
        foreach (Actor h in helpers) {
          h.SpendActionPoints();
          h.SpendStaminaPoints(staCost);
          if (isVisibleMobj || IsVisibleToPlayer(h)) AddMessage(MakeMessage(h, String.Format("{0} {1} {2} {3}.", VERB_HELP.Conjugate(h), actor.Name, (isPulling ? "pulling" : "pushing"), mapObj.TheName)));
        }
      }
    }

    public void DoPush(Actor actor, MapObject mapObj, in Location toPos)
    {
      bool flag = ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(mapObj);
      int staminaCost = mapObj.Weight;
      if (actor.CountFollowers > 0) DoPushPullFollowersHelp(actor, mapObj, false, ref staminaCost); // alpha10
      actor.SpendActionPoints();
      actor.SpendStaminaPoints(staminaCost);

      Map map = mapObj.Location.Map;

      // do it : move object then move actor
      Location actor_dest = mapObj.Location;
      Location objDest = toPos;

      // ...object
      mapObj.Remove();
      objDest.Place(mapObj);

      if (!Rules.IsAdjacent(mapObj.Location, actor.Location) && actor_dest.IsWalkableFor(actor)) {
        if (TryActorLeaveTile(actor)) { // RS alpha 10
          actor.Location.Map.Remove(actor);
          actor_dest.Place(actor);  // assumed to be walkable, checked by rules
          OnActorEnterTile(actor);  // RS alpha 10
        }
      }
      if (flag) RedrawPlayScreen(MakeMessage(actor, VERB_PUSH.Conjugate(actor), mapObj));
      OnLoudNoise(objDest, "Something being pushed");
      bool player_knows(Actor a) {
        return     a.Controller.CanSee(actor.Location) // we already checked the attacker visibility, he's the sound origin
               && !Rules.Get.RollChance(PLAYER_HEAR_PUSHPULL_CHANCE);  // not clear enough; no message
      }
      void react(Actor a) {
        if (!(a.Controller is OrderableAI ai)) return;  // not that smart (ultimately would want to extend to handler FeralDogAI
        if (!a.IsEnemyOf(actor)) return;   // not relevant
        if (a.Controller is CHARGuardAI && !IsInCHARProperty(actor.Location)) return; // CHAR guards generally ignore enemies not within a CHAR office. \todo should not be ignoring threats just outside of the doors
        if (    ai.IsDistracted(ObjectiveAI.ReactionCode.NONE)
            || !Rules.Get.RollChance(PLAYER_HEAR_PUSHPULL_CHANCE))  // not clear enough
          return;
        if (!ai.CombatUnready()) {  // \todo should discriminate between cops/soldiers/CHAR guards and civilians here; civilians may avoid even if combat-ready
          ai.Terminate(actor);
          return;
        }
        // \todo: get away from the fighting
      }
      PropagateSound(mapObj.Location, "You hear something being pushed",react,player_knows);
      CheckMapObjectTriggersTraps(objDest.Map, objDest.Position);
    }

    public void DoShove(Actor actor, Actor target, in Location dest)
    {
      actor.SpendActionPoints();
      if (TryActorLeaveTile(target)) {
        actor.SpendStaminaPoints(Rules.DEFAULT_ACTOR_WEIGHT);
        target.StopDraggingCorpse();
        var t_loc = target.Location;
        var new_t_loc = dest; // so Map.Canonical works
        if (!Map.Canonical(ref new_t_loc)) throw new InvalidOperationException("shoved off map entirely");
        bool non_adjacent = !Rules.IsAdjacent(in new_t_loc, actor.Location);
        if (non_adjacent && Location.RequiresJump(in t_loc)) {
#if DEBUG
          if (!actor.CanJump) throw new InvalidOperationException("shoving off a jumpable object this way requires jumping onto the object");
#endif
          actor.SpendStaminaPoints(Rules.STAMINA_COST_JUMP);
        }
        new_t_loc.Place(target);
        if (non_adjacent) {
          if (TryActorLeaveTile(actor)) {
            t_loc.Place(actor);
            OnActorEnterTile(actor);
          }
        }
        if (ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(target) || ForceVisibleToPlayer(in new_t_loc)) {
          RedrawPlayScreen(MakeMessage(actor, VERB_SHOVE.Conjugate(actor), target));
        }
        if (target.IsSleeping) DoWakeUp(target);
        OnActorEnterTile(target);
      }
    }

    public void DoPull(Actor actor, MapObject mapObj, in Location actor_dest) // alpha10
    {
      bool isVisible = ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(mapObj);
      int staCost = mapObj.Weight;

      if (!TryActorLeaveTile(actor)) {  // try leaving tile
        actor.SpendActionPoints();
        return;
      }

      // followers help?
      if (actor.CountFollowers > 0) DoPushPullFollowersHelp(actor, mapObj, true, ref staCost);

      // spend AP & STA.
      actor.SpendActionPoints();
      actor.SpendStaminaPoints(staCost);

      // do it : move actor then move object
      Location objDest = actor.Location;

      Map map = mapObj.Location.Map;
      // actor...
      objDest.Map.Remove(actor);
      actor_dest.Place(actor);  // assumed to be walkable, checked by rules
      // ...object
      mapObj.Remove();
      objDest.Place(mapObj);

      // noise/message.
      if (isVisible) RedrawPlayScreen(MakeMessage(actor, VERB_PULL.Conjugate(actor), mapObj));
      // loud noise.
      OnLoudNoise(mapObj.Location, "Something being pushed");

      bool player_knows(Actor a) {
        return     a.Controller.CanSee(actor.Location) // we already checked the attacker visibility, he's the sound origin
               && !Rules.Get.RollChance(PLAYER_HEAR_PUSHPULL_CHANCE);  // not clear enough; no message
      }
      void react(Actor a) {
        if (!(a.Controller is OrderableAI ai)) return;  // not that smart (ultimately would want to extend to handler FeralDogAI
        if (!a.IsEnemyOf(actor)) return;   // not relevant
        if (a.Controller is CHARGuardAI && !IsInCHARProperty(actor.Location)) return; // CHAR guards generally ignore enemies not within a CHAR office. \todo should not be ignoring threats just outside of the doors
        if (    ai.IsDistracted(ObjectiveAI.ReactionCode.NONE)
            || !Rules.Get.RollChance(PLAYER_HEAR_PUSHPULL_CHANCE))  // not clear enough
          return;
        if (!ai.CombatUnready()) {  // \todo should discriminate between cops/soldiers/CHAR guards and civilians here; civilians may avoid even if combat-ready
          ai.Terminate(actor);
          return;
        }
        // \todo: get away from the fighting
      }
      PropagateSound(mapObj.Location, "You hear something being pushed",react,player_knows);

      // check triggers
      OnActorEnterTile(actor);
      CheckMapObjectTriggersTraps(map, mapObj.Location.Position);
    }

    public void DoPullActor(Actor actor, Actor target, in Location dest)    // alpha10
    {
      bool isVisible = ForceVisibleToPlayer(actor) || ForceVisibleToPlayer(target);

      // try leaving tile, both actors and target
      if (!TryActorLeaveTile(actor) || !TryActorLeaveTile(target)) {
        actor.SpendActionPoints();
        return;
      }

      actor.SpendActionPoints();
      actor.SpendStaminaPoints(Rules.DEFAULT_ACTOR_WEIGHT);
      target.StopDraggingCorpse();

      // do it : move actor then move target
      Location src = actor.Location;

      // move actor...
     src.Map.Remove(actor);
     dest.Place(actor);
      // ...move target
     target.Location.Map.Remove(target);
     src.Place(target);

      // if target is sleeping, wakes him up!
      if (target.IsSleeping) DoWakeUp(target);

      // message
      if (isVisible) RedrawPlayScreen(MakeMessage(actor, VERB_PULL.Conjugate(actor), target));

      // Trigger stuff.
      OnActorEnterTile(actor);
      OnActorEnterTile(target);
    }

    public void DoWakeUp(Actor actor)
    {
      actor.Activity = Data.Activity.IDLE;
      actor.IsSleeping = false;
      var witnesses = _ForceVisibleToPlayer(actor);
      if (null != witnesses) AddMessage(witnesses, MakePanopticMessage(actor, string.Format("{0}.", VERB_WAKE_UP.Conjugate(actor))));
      // stop sleep music if player.
      if (actor.IsPlayer && m_MusicManager.Music == GameMusics.SLEEP) m_MusicManager.Stop();
    }

    private void DoTag(Actor actor, ItemSprayPaint spray, Point pos)
    {
      actor.SpendActionPoints();
      --spray.PaintQuantity;
      actor.Location.Map.AddDecorationAt(spray.Model.TagImageID, in pos);
      var witnesses = _ForceVisibleToPlayer(actor);
      if (null != witnesses) AddMessage(witnesses, MakePanopticMessage(actor, string.Format("{0} a tag.", VERB_SPRAY.Conjugate(actor))));
    }

#nullable enable
    // alpha10 new way to use spray scent
    public void DoSprayOdorSuppressor(Actor actor, ItemSprayScent suppressor, Actor sprayOn)
    {
      actor.SpendActionPoints();  // spend AP.
      --suppressor.SprayQuantity;   // spend spray.
      sprayOn.OdorSuppressorCounter += suppressor.Model.Strength; // add odor suppressor on spray target

      // message.
      if (ForceVisibleToPlayer(actor))
        AddMessage(MakeMessage(actor, string.Format("{0} {1}.", VERB_SPRAY.Conjugate(actor), (sprayOn == actor ? actor.HimselfOrHerself : sprayOn.Name))));
    }

    private void DoGiveOrderTo(Actor master, OrderableAI? ordai, ActorOrder order)
    {
      if (null == ordai) return;
      var slave = ordai.ControlledActor;
      master.SpendActionPoints();
      if (master != slave.Leader) slave.Say(master, "Who are you to give me orders?", Sayflags.IS_FREE_ACTION);
      else if (!slave.IsTrustingLeader) slave.Say(master, "Sorry, I don't trust you enough yet.", Sayflags.IS_IMPORTANT | Sayflags.IS_FREE_ACTION);
      else {
        ordai.SetOrder(order);
        if (ForceVisibleToPlayer(master) || ForceVisibleToPlayer(slave))
          AddMessage(MakeMessage(master, VERB_ORDER.Conjugate(master), slave, string.Format(" to {0}.", order.ToString())));
      }
    }

    private void DoCancelOrder(Actor master, OrderableAI ordai)
    {
      if (null == ordai) return;
      var slave = ordai.ControlledActor;
      master.SpendActionPoints();
      ordai.SetOrder(null);
      if (ForceVisibleToPlayer(master) || ForceVisibleToPlayer(slave))
        AddMessage(MakeMessage(master, VERB_ORDER.Conjugate(master), slave, " to forget its orders."));
    }

    public void OnLoudNoise(Location loc, string noiseName) {
      bool hears(Actor a) {
        if (!a.IsSleeping) return false;
        return Rules.Get.RollChance(a.LoudNoiseWakeupChance(Rules.GridDistance(loc, a.Location)));
      };

      var actors = loc.ThoseNearby(Rules.LOUD_NOISE_RADIUS, hears);
      if (null == actors) return;

      foreach(var actor in actors) {
        var witnesses = _ForceVisibleToPlayer(actor);
        DoWakeUp(actor);
        if (null != witnesses) {
          RedrawPlayScreen(witnesses, new(string.Format("{0} wakes {1} up!", noiseName, actor.TheName), loc.Map.LocalTime.TurnCounter, actor == Player ? Color.Red : Color.White));
        }
      }
    }

    static public int ItemSurviveKillProbability(Item it, string reason)
    {
      switch(reason)    // XXX be sure not to typo
      {
      case "infection":
      case "starvation":
      case "suicide":   return Rules.VICTIM_DROP_AMMOFOOD_ITEM_CHANCE;  // these methods are *exceptionally* forgiving on item survival
      case "trap":
      case "shot":
      case "hit":   return (it is ItemBodyArmor ? Rules.VICTIM_DROP_GENERIC_ITEM_CHANCE : Rules.VICTIM_DROP_AMMOFOOD_ITEM_CHANCE);  // armor likely to be trashed by these methods.  Other items, not so much
      // As in RS alpha 9.  Give a very high survival rate to ammo and food for balance, not so high for other items
      // appropriate for zombification and optimistic for explosions and crushing by gates
      default: return (it is ItemAmmo || it is ItemFood) ? Rules.VICTIM_DROP_AMMOFOOD_ITEM_CHANCE : Rules.VICTIM_DROP_GENERIC_ITEM_CHANCE;
      }
    }
#nullable restore

    public void KillActor(Actor killer, Actor deadGuy, string reason)
    {
#if DEBUG
      // for some reason, this can happen with starved actors (cf. alpha 8)
      if (deadGuy.IsDead)
        throw new InvalidOperationException(String.Format("killing deadGuy that is already dead : killer={0} deadGuy={1} reason={2}", (killer == null ? "N/A" : killer.TheName), deadGuy.TheName, reason));
#endif
      Actor m_Player_bak = Player;    // ForceVisibleToPlayer calls below can change this

      deadGuy.IsDead = true;
      var deadGuy_isUndead = new const_<bool>(deadGuy.Model.Abilities.IsUndead);
      deadGuy.Location.Map.Recalc(deadGuy); // \todo? could fold into Killed call, below

      var police_relevant = Session.Get.Police.EncodeKill(killer, deadGuy, Session.Get.WorldTime.TurnCounter);
      var police_targeted = Session.Get.Police.IsTargeted(deadGuy);

      var isMurder = new const_<bool>(Rules.IsMurder(killer, deadGuy));  // record this before it's invalidated (in POLICE_NO_QUESTIONS_ASKED build)
	  deadGuy.Killed(reason);
      deadGuy.StopDraggingCorpse();
      deadGuy.Location.Items?.UntriggerAllTraps();
      if (killer != null && !killer.Model.Abilities.IsUndead && deadGuy_isUndead.cache)
        killer.RegenSanity(killer.ScaleSanRegen(Rules.SANITY_RECOVER_KILL_UNDEAD));

      if (deadGuy.HasActivePoliceRadio) {
        UI.Message? msg = null;
        Action<PlayerController> pc_add_msg = pc => pc.AddMessage(msg ??= new("(police radio ai) " + deadGuy.Name + " killed: " + reason, Session.Get.WorldTime.TurnCounter, SAYOREMOTE_DANGER_COLOR));
        deadGuy.MessageAllInDistrictByRadio(NOP, FALSE, pc_add_msg, pc_add_msg, TRUE);
      }


      var clan = deadGuy.ChainOfCommand;    // both leader and immediate followers
      if (null != clan) foreach(var a in clan) {
        if (a.HasBondWith(deadGuy)) {
          a.SpendSanity(Rules.SANITY_HIT_BOND_DEATH);
          var witnesses = a.PlayersInLOS();
          if (null != witnesses) a.Controller.AddMessageForceRead(MakePanopticMessage(a, string.Format("{0} deeply disturbed by {1} sudden death!", VERB_BE.Conjugate(a), deadGuy.Name)), witnesses);
        }
      }

      int turn = deadGuy.Location.Map.LocalTime.TurnCounter;
      if (deadGuy.IsUnique) {
        // XXX \todo global event
        m_Player_bak.ActorScoring.AddEvent(turn,
            (killer != null
           ? string.Format("* {0} was killed by {1} {2}! *", deadGuy.TheName, killer.Model.Name, killer.TheName)
           : string.Format("* {0} died by {1}! *", deadGuy.TheName, reason)));
      }
      if (deadGuy == m_Player_bak) {
        setPlayer(m_Player_bak);
        if (0 >= World.Get.PlayerCount) PlayerDied(killer, reason);
      }
      deadGuy.RemoveAllFollowers();
      var leader = deadGuy.Leader;
      if (null != leader) {
        leader.ActorScoring.AddEvent(turn,
            (killer == null
           ? string.Format("Follower {0} died by {1}!", deadGuy.TheName, reason)
           : string.Format("Follower {0} was killed by {1} {2}!", deadGuy.TheName, killer.Model.Name, killer.TheName)));
        leader.RemoveFollower(deadGuy);
      }
      deadGuy.RemoveAllAgressorSelfDefenceRelations();
      deadGuy.RemoveFromMap();

      // note that if police went after a follower for murder, the police threat tracking historically would target the leader indefinitely.
      // We don't have the CPU or savefile size for that (we *should* be tracking something, but we'd need a more detailed
      // crime blotter representation).
      static bool police_wanted(Actor a) {
        if (a.Faction.IsEnemyOf(GameFactions.ThePolice)) return true;
        if (a.Aggressors.Any_(who => who.IsFaction(GameFactions.IDs.ThePolice))) return true;
        if (a.Aggressing.Any_(who => who.IsFaction(GameFactions.IDs.ThePolice) || GameFactions.ThePolice == who.LiveLeader?.Faction)) return true;
        return false;
      }

      Session.Get.Police.Threats.Audit(police_wanted); // cf. RadioFaction.Killed

      if (!deadGuy.Inventory?.IsEmpty ?? false) {
        // the implicit police radio goes explicit on death, as a generic item
        if (deadGuy.IsFaction(GameFactions.IDs.ThePolice)) {
          var it = GameItems.POLICE_RADIO.instantiate();
          if (Rules.Get.RollChance(ItemSurviveKillProbability(it, reason))) deadGuy.Location.Drop(it);
        }
        foreach (Item it in deadGuy.Inventory.Items.ToArray()) {
          if (it.IsUseless) continue;   // if the drop command/behavior would trigger discard instead, omit
          if (it.Model.IsUnbreakable || it.IsUnique || Rules.Get.RollChance(ItemSurviveKillProbability(it, reason)))
            DropItem(deadGuy, it);
        }
        deadGuy.Inventory.Clear(); // will stay data-live through corpses otherwise
        Session.Get.Police.Investigate.Record(deadGuy.Location);  // cheating ai: police consider death drops tourism targets
      }

      if (!deadGuy_isUndead.cache) {
        SplatterBlood(deadGuy.Location.Map, deadGuy.Location.Position);
        if (Session.Get.HasCorpses) DropCorpse(deadGuy);
      }
#if FAIL
      else UndeadRemains(deadGuy.Location.Map, deadGuy.Location.Position);
#endif

      if (null != killer) {
        killer.RecordKill(deadGuy);
        var killer_pc = killer.Controller as PlayerController;
        if (Session.Get.HasEvolution && killer.Model.Abilities.IsUndead) {
          var actorModel = CheckUndeadEvolution(killer);
          if (actorModel != null) {
            // don't need value-copy here due to how the model assignment works
            var skills = killer.MySkills.Skills;
            killer.Model = actorModel;
            killer.APreset(); // to avoid triggering a debug-mode crash
            if (null != killer_pc) killer.PrepareForPlayerControl();
            if (null != skills) {
              foreach (var sk in skills) {
                for (int index = 0; index < sk.Value; ++index) killer.SkillUpgrade(sk.Key);
              }
              killer.RecomputeStartingStats();
            }
            var witnesses = killer.PlayersInLOS();
            if (null != witnesses) {
              AddOverlay(new OverlayRect(Color.Yellow, new GDI_Rectangle(MapToScreen(killer.Location), SIZE_OF_ACTOR)));
              ImportantMessage(witnesses, MakePanopticMessage(killer, string.Format("{0} a {1} horror!", VERB_TRANSFORM_INTO.Conjugate(killer), actorModel.Name)), DELAY_LONG);
              ClearOverlays();
            }
          }
        }
        killer.DoForAllFollowers(fo => {
            if ((fo.TargetActor == deadGuy || fo.IsEnemyOf(deadGuy)) && Rules.IsAdjacent(fo.Location, deadGuy.Location)) {
              fo.Say(killer, "That was close! Thanks for the help!!", Sayflags.IS_FREE_ACTION);
              ModifyActorTrustInLeader(fo, Rules.TRUST_LEADER_KILL_ENEMY, true);
            }
        });
        if (isMurder.cache) {
          killer.HasMurdered(deadGuy);
          var witnesses = _ForceVisibleToPlayer(killer);
          if (null != witnesses) AddMessage(witnesses, MakePanopticMessage(killer, string.Format("murdered {0}!!", deadGuy.Name)));

          // \todo while soldiers won't actively track down murderers, they will respond if it happens in sight
          PropagateSight(killer.Location, a => {
            if (a.Leader != killer && killer.Leader != a) {
              if (a.Model.Abilities.IsLawEnforcer) {
                a.Say(killer, string.Format("MURDER! {0} HAS KILLED {1}!", killer.TheName, deadGuy.TheName), Sayflags.IS_IMPORTANT | Sayflags.IS_FREE_ACTION);
                RadioNotifyAggression(a, killer, "(police radio, {0}) Executing {1} for murder.");
                DoMakeAggression(a, killer);
              }
            }
          });
        }
        if (killer.Model.Abilities.IsLawEnforcer && !killer.Faction.IsEnemyOf(deadGuy.Faction)) {
          if (0 < Session.Get.Police.CountMurders(deadGuy)) {
            if (null != killer_pc)
              killer_pc.AddMessage(new("You feel like you did your duty with killing a murderer.", Session.Get.WorldTime.TurnCounter, Color.White));
            else
              killer.Say(deadGuy, "Good riddance, murderer!", Sayflags.IS_FREE_ACTION | Sayflags.IS_DANGER);
          } else if (0 < Session.Get.Police.CountAssaults(deadGuy)) {
            if (null != killer_pc)
              killer_pc.AddMessage(new("You feel like you did your duty with killing a violent criminal.", Session.Get.WorldTime.TurnCounter, Color.White));
            else
              killer.Say(deadGuy, "Good riddance!", Sayflags.IS_FREE_ACTION | Sayflags.IS_DANGER);
          }

          // Police report all (non-murder) kills via police radio.  National Guard likely to do same.
          if (!isMurder.cache) {
            // optimized version of this feasible...but if we want AI to respond directly then much of that optimization goes away
            // also need to consider background thread to main thread issues
            // possible verbs: killed, terminated, erased, downed, wasted.
            UI.Message msg = new(string.Format("(police radio, {0}) {1} killed.", killer.Name, deadGuy.Name), Session.Get.WorldTime.TurnCounter, Color.White);
            Action<PlayerController> pc_add_msg = pc => pc.AddMessage(msg);
            killer.MessageAllInDistrictByRadio(NOP, FALSE, pc_add_msg, pc_add_msg, TRUE);
          }
        }

        // achievement: killing the Sewers Thing
        // XXX \todo this achievement is newsworthy.
        if (deadGuy == Session.Get.UniqueActors.TheSewersThing.TheActor) {
          ShowNewAchievement(Achievement.IDs.KILLED_THE_SEWERS_THING, killer);
          var hero_team = killer.ChainOfCommand;
          if (null != hero_team && killer.Controller is ObjectiveAI ai) {
            foreach (Actor a in hero_team) {
              if (!ai.InCommunicationWith(a) && killer.LiveLeader != a) continue;   // historically RS Alpha 9 gave credit to a PC leader for an NPC follower sewers thing kill
              ShowNewAchievement(Achievement.IDs.KILLED_THE_SEWERS_THING, a);
            }
          }
        }
      }

      deadGuy.TargetActor = null; // savefile scanner said this wasn't covered.  Other fields targeted by Actor::OptimizeBeforeSaving are covered.
      Session.Get.Police.OnKilled(deadGuy, killer);
      Session.Get.Police.Record(police_relevant);

      if (deadGuy.IsPlayer && (!killer?.IsPlayer ?? false)) {
        // this may need to be multi-thread aware
        Actor reinc = killer.LiveLeader ?? killer;
        if (!reinc.IsPlayer && Session.Get.Scoring.ReincarnationNumber < s_Options.MaxReincarnations) {
          if (YesNoPopup("Use a reincarnation on your " + (killer == reinc ? " killer " : " killer's leader ") + reinc.Name)) {
            reinc.MakePC();
            Session.Get.Scoring.UseReincarnation();
          }
        }
      }

      // If m_Player has just died, then we should be in the current district and thus clear to find a player
      // furthermore, the viewport didn't pan away to another player
      if (Player == deadGuy) FindPlayer();
      if (null == m_SimThread) StartSimThread();
    }

    // alpha10
    /// <param name="actor"></param>
    /// <returns>the disarmed item or null if actor had no equipped item</returns>
    private Item Disarm(Actor actor)
    {
      // pick equipped item to disarm : prefer weapon, then any right handed item(?), then left handed.
      var disarmIt = actor.GetEquippedWeapon();
      if (null == disarmIt) {
        disarmIt = actor.GetEquippedItem(DollPart.RIGHT_HAND);
        if (null == disarmIt) disarmIt = actor.GetEquippedItem(DollPart.LEFT_HAND);
       }

       if (null == disarmIt) return null;

       // unequip, remove from inv and drop item in a random adjacent tile
       // if none possible, will drop on same tile (which then has no almost no gameplay effect 
       // because the actor can take it back asap at no ap cost... unless he dies)
       actor.Remove(disarmIt, false);
       List<Point> dropTiles = new List<Point>(8);
       actor.Location.Map.ForEachAdjacent(actor.Location.Position, pt => {
         // checking if can drop there is eq to checking if can throw it there
         if (!actor.Location.Map.IsBlockingThrow(pt)) dropTiles.Add(pt);
       });
       Point dropOnTile;
       if (dropTiles.Count > 0) dropOnTile = Rules.Get.DiceRoller.Choose(dropTiles);
       else dropOnTile = actor.Location.Position;
       actor.Location.Map.DropItemAt(disarmIt, in dropOnTile);  // formal fix \todo this could end up making a dropped item inaccessible?

       return disarmIt; // done
    }

#nullable enable
    static private ActorModel? CheckUndeadEvolution(Actor undead)
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
	  return (index != undead.Model.ID ? GameActors.From(index) : null);
    }

    static private void SplatterBlood(Map map, Point position)
    {
      const int BLOOD_WALL_SPLAT_CHANCE = 20;

      static bool is_floor(Map m, Point pt) { return m.GetTileModelAt(pt).IsWalkable; }
      static bool is_wall(Map m, Point pt) { return !m.GetTileModelAt(pt).IsWalkable && Rules.Get.RollChance(BLOOD_WALL_SPLAT_CHANCE); }

      map.AddTimedDecoration(position, GameImages.DECO_BLOODIED_FLOOR, WorldTime.TURNS_PER_DAY, is_floor);
      map.ForEachAdjacent(position,p => map.AddTimedDecoration(p, GameImages.DECO_BLOODIED_WALL, WorldTime.TURNS_PER_DAY, is_wall));    // no cross-district walls so ok
    }

#if DEAD_FUNC
    public void UndeadRemains(Map map, Point position)
    {
      Tile tileAt = map.GetTileAt(position);
//    if (!map.IsWalkable(position.X, position.Y) || tileAt.HasDecoration(GameImages.DECO_ZOMBIE_REMAINS)) return;
      tileAt.AddDecoration(GameImages.DECO_ZOMBIE_REMAINS);
      map.AddTimer(new TaskRemoveDecoration(WorldTime.TURNS_PER_DAY, position, GameImages.DECO_ZOMBIE_REMAINS));
    }
#endif

    static private void DropCorpse(Actor deadGuy)
    {
      deadGuy.Doll.AddDecoration(DollPart.TORSO, GameImages.BLOODIED);
      var rules = Rules.Get;
      float rotation = rules.Roll(30, 60);
      if (rules.RollChance(50)) rotation = -rotation;
      new Corpse(deadGuy, rotation); // XXX constructor inserts into containing map
    }
#nullable restore

    private void PlayerDied(Actor killer, string reason)
    {
      StopSimThread();   // alpha10 abort allowed when dying
      m_UI.UI_SetCursor(null);
      m_MusicManager.Stop();
      m_MusicManager.Play(GameMusics.PLAYER_DEATH, MusicPriority.PRIORITY_EVENT);

      Session.Get.LatestKill(killer,Player);

      Player.ActorScoring.DeathReason = killer == null ? string.Format("Death by {0}", reason)
                                                       : string.Format("{0} by {1} {2}", Rules.IsMurder(killer, Player) ? "Murdered" : "Killed", killer.Model.Name, killer.TheName);
      int turn = Session.Get.WorldTime.TurnCounter;
      Player.ActorScoring.AddEvent(turn, "Died.");

      AddOverlay(new OverlayPopup(new string[3] {
        "TIP OF THE DEAD",
        "Did you know that...",
        Rules.Get.DiceRoller.Choose(GameTips.TIPS)
      }, Color.White, Color.White, POPUP_FILLCOLOR, GDI_Point.Empty));

      List<UI.Message> msgs = new() { new("**** YOU DIED! ****", turn, Color.Red) };
      if (killer != null) msgs.Add(new(string.Format("Killer : {0}.", killer.TheName), turn, Color.Red));
      msgs.Add(new(string.Format("Reason : {0}.", reason), turn, Color.Red));
      msgs.Add(new(Player.Model.Abilities.IsUndead ? "You die one last time... Game over!"
                                                   : "You join the realm of the undeads... Game over!", turn, Color.Red));
      if (s_Options.IsDeathScreenshotOn) {
        RedrawPlayScreen();
        var screenshot = DoTakeScreenshot();
        msgs.Add((null == screenshot) ? MakeErrorMessage("could not save death screenshot.")
                                        : new(string.Format("Death screenshot saved : {0}.", screenshot), turn, Color.Red));
      }
      (Player.Controller as PlayerController)!.AddMessagesForceRead(msgs);

      HandlePostMortem();
      m_MusicManager.Stop();
    }

    static private string TimeSpanToString(TimeSpan rt)
    {
      // alpha10 shortened
#if OBSOLETE
      return string.Format("{0}{1}{2}{3}", rt.Days == 0 ? "" : string.Format("{0} days ", (object)rt.Days), rt.Hours == 0 ? "" : string.Format("{0:D2} hours ", (object)rt.Hours), rt.Minutes == 0 ? "" : string.Format("{0:D2} minutes ", (object)rt.Minutes), rt.Seconds == 0 ? "" : string.Format("{0:D2} seconds", (object)rt.Seconds));
#else
      return string.Format("{0}{1}{2}{3}", rt.Days == 0 ? "" : string.Format("{0} d ", (object)rt.Days), rt.Hours == 0 ? "" : string.Format("{0:D2} h ", (object)rt.Hours), rt.Minutes == 0 ? "" : string.Format("{0:D2} m ", (object)rt.Minutes), rt.Seconds == 0 ? "" : string.Format("{0:D2} s", (object)rt.Seconds));
#endif
    }

    private void HandlePostMortem()
    {
      string str1 = Player.HisOrHer;
      string str2 = Player.HimOrHer;
      string name = Player.UnmodifiedName;
      var g_scoring = Session.Get.Scoring;
      var p_scoring = Player.ActorScoring;
      var textFile = new TextFile();
      int test;
      textFile.Append(SetupConfig.GAME_NAME_CAPS+" "+SetupConfig.GAME_VERSION);
      textFile.Append("POST MORTEM");
      textFile.Append(string.Format("{0} was {1} and {2}.", name, Player.Model.Name.PrefixIndefiniteSingularArticle(), Player.Faction.MemberName.PrefixIndefiniteSingularArticle()));
      textFile.Append(string.Format("{0} survived to see {1}.", str1, (new WorldTime(p_scoring.TurnsSurvived)).ToString()));
      textFile.Append(string.Format("{0}'s spirit guided {1} for {2}.", name, str2, TimeSpanToString(g_scoring.RealLifePlayingTime)));
      if (0 < (test = g_scoring.ReincarnationNumber)) textFile.Append(string.Format("{0} was reincarnation {1}.", str1, test));
      textFile.Append(" ");
      textFile.Append("> SCORING");
      textFile.Append(string.Format("{0} scored a total of {1} points.", str1, p_scoring.TotalPoints));
      textFile.Append(string.Format("- difficulty rating of {0}%.", (int)(100.0 * p_scoring.DifficultyRating)));
      textFile.Append(string.Format("- {0} base points for survival.", p_scoring.SurvivalPoints));
      textFile.Append(string.Format("- {0} base points for kills.", p_scoring.KillPoints));
      textFile.Append(string.Format("- {0} base points for achievements.", p_scoring.AchievementPoints));
      textFile.Append(" ");
      textFile.Append("> ACHIEVEMENTS");
      p_scoring.DescribeAchievements(textFile);
      if (0 >= (test = p_scoring.CompletedAchievementsCount)) {
        textFile.Append("Didn't achieve anything notable. And then died.");
        textFile.Append(string.Format("(unlock all the {0} achievements to win this game version)", 8));
      } else {
        textFile.Append(string.Format("Total : {0}/{1}.", test, (int)Achievement.IDs._COUNT));
        textFile.Append(((int)Achievement.IDs._COUNT <= test)
            ? "*** You achieved everything! You can consider having won this version of the game! CONGRATULATIONS! ***"
            : "(unlock all the achievements to win this game version)");
        textFile.Append("(later versions of the game will feature real winning conditions and multiple endings...)");
      }
      textFile.Append(" ");
      textFile.Append("> DEATH");
      textFile.Append(string.Format("{0} in {1}.", p_scoring.DeathReason, Session.Get.Scoring_fatality.DeathPlace));
      textFile.Append(" ");
      textFile.Append("> KILLS");
      p_scoring.DescribeKills(textFile, str1);
      if (0 < (test = Session.Get.Police.CountMurders(Player))) textFile.Append(string.Format("{0} committed {1}!", str1, "murder".QtyDesc(test)));
      if (0 < (test = Session.Get.Police.CountAssaults(Player))) textFile.Append(string.Format("{0} committed {1}!", str1, "assault".QtyDesc(test)));
      textFile.Append(" ");
      textFile.Append("> FUN FACTS!");
      textFile.Append(string.Format("While {0} has died, others are still having fun!", name));
      foreach (string compileDistrictFunFact in CompileDistrictFunFacts(Player.Location.Map.District))
        textFile.Append(compileDistrictFunFact);
      textFile.Append("");
      textFile.Append("> SKILLS");
      var p_skills = Player.MySkills.Skills;
      if (null == p_skills) {
        textFile.Append(string.Format("{0} was a jack of all trades. Or an incompetent.", str1));
      } else {
        foreach (var sk in p_skills) textFile.Append(string.Format("{0}-{1}.", sk.Value, Skills.Name(sk.Key)));
      }
      textFile.Append(" ");
      textFile.Append("> INVENTORY");
      {
      var inv = Player.Inventory;
      if (null == inv || inv.IsEmpty) {
        textFile.Append(string.Format("{0} was humble. Or dirt poor.", str1));
      } else {
        foreach (Item it in inv.Items) {
          textFile.Append(string.Format((it.IsEquipped ? "- {0} (equipped)." : "- {0}."), DescribeItemShort(it)));
        }
      }
      }
      textFile.Append(" ");
      textFile.Append("> FOLLOWERS");
      { // scoping brace
      var followers = Session.Get.Scoring_fatality.FollowersWhendDied;
      if (null == followers) {
        textFile.Append(string.Format("{0} was doing fine alone. Or everyone else was dead.", str1));
      } else {
        int count_followers = followers.Count;  // greater than 0 by construction
        var stringBuilder = new StringBuilder(string.Format("{0} was leading", str1));
        bool flag = true;
        int num = 0;
        foreach (Actor actor in followers) {
          if (flag) stringBuilder.Append(' ');
          else if (num == count_followers) stringBuilder.Append('.');
          else if (num == count_followers - 1) stringBuilder.Append(" and ");
          else stringBuilder.Append(", ");
          stringBuilder.Append(actor.TheName);
          ++num;
          flag = false;
        }
        stringBuilder.Append('.');
        textFile.Append(stringBuilder.ToString());
        foreach (Actor actor in followers) {
          textFile.Append(string.Format("{0} skills : ", actor.Name));
          var a_skills = actor.MySkills.Skills;
          if (null != a_skills) foreach (var sk in a_skills) textFile.Append(string.Format("{0}-{1}.", sk.Value, Skills.Name(sk.Key)));
        }
      }
      } // scoping brace
      textFile.Append(" ");
      textFile.Append("> EVENTS");
      p_scoring.DescribeEvents(textFile, str1);
      textFile.Append(" ");
      textFile.Append("> CUSTOM OPTIONS");
      textFile.Append(string.Format("- difficulty rating of {0}%.", (int)(100.0 * p_scoring.DifficultyRating)));
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
      string player_his = Player.HisOrHer;
      textFile.Append(string.Format("May {0} soul rest in peace.", player_his));
      textFile.Append(string.Format("For {0} body is now a meal for evil.", player_his));
      textFile.Append("The End.");
      m_UI.ClearScreen();
      m_UI.UI_DrawStringBold(Color.Yellow, "Saving post mortem to graveyard...", 0, 0, new Color?());
      m_UI.UI_Repaint();
      string str4 = GraveFilePath(GetUserNewGraveyardName());
      if (!textFile.Save(str4)) {
        m_UI.UI_DrawStringBold(Color.Red, "Could not save to graveyard.", 0, BOLD_LINE_SPACING, new Color?());
      } else {
        m_UI.UI_DrawStringBold(Color.Yellow, "Grave saved to :", 0, BOLD_LINE_SPACING, new Color?());
        m_UI.UI_DrawString(Color.White, str4, 0, 2*BOLD_LINE_SPACING, new Color?());
      }
      m_UI.RenderFootnote("press ENTER");
      m_UI.WaitEnter();
      textFile.FormatLines(TEXTFILE_CHARS_PER_LINE);
      int index = 0;
      do {
        m_UI.ClearScreen();
        DrawHeader();
        int gy2 = BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.White, hr_plus, 0, gy2, new Color?());
        gy2 += BOLD_LINE_SPACING;
        for (int num5 = 0; num5 < 50 && index < textFile.FormatedLines.Count; ++num5) {
          m_UI.UI_DrawStringBold(Color.White, textFile.FormatedLines[index], 0, gy2, new Color?());
          gy2 += BOLD_LINE_SPACING;
          ++index;
        }
        m_UI.UI_DrawStringBold(Color.White, hr_plus, 0, CANVAS_HEIGHT - 2 * BOLD_LINE_SPACING, new Color?());
        m_UI.RenderFootnote(index < textFile.FormatedLines.Count ? "press ENTER for more" : "press ENTER to leave");
        m_UI.WaitEnter();
      }
      while (index < textFile.FormatedLines.Count);
      var stringBuilder1 = new StringBuilder();
      var skills = Player.MySkills.Skills;
      if (null != skills) foreach (var sk in skills) stringBuilder1.AppendFormat("{0}-{1} ", sk.Value, Skills.Name(sk.Key));
      if (!m_HiScoreTable.Register(new HiScore(g_scoring, p_scoring, stringBuilder1.ToString()))) return;
      SaveHiScoreTable();
      HandleHiScores(true);
    }

    private void PCsurvival(PlayerController pc, UI.Message msg_alive, UI.Message msg_welcome)
    {
        if (IsSimulating || !IsPlayer(pc.ControlledActor)) {
            pc.AddMessage(msg_alive);
            pc.AddMessage(msg_welcome);
        } else {
            m_MusicManager.Stop();
            m_MusicManager.PlayLooping(GameMusics.INTERLUDE, MusicPriority.PRIORITY_EVENT);
            pc.UpdateSensors();
            pc.AddMessageForceRead(msg_alive);
            // HandlePlayerDecideUpgrade(m_Player);    // XXX skill upgrade timing problems with non-following PCs
            pc.Messages.Clear();
            pc.AddMessage(msg_welcome);
            RedrawPlayScreen();
            m_MusicManager.Stop();
        }
    }

    private void HandleNewNight(Actor victor)
    {
      if (!victor.Model.Abilities.IsUndead) return;
	  // Proboards leonelhenry: PC should be on the same options as undead
	  if (s_Options.ZombifiedsUpgradeDays == GameOptions.ZupDays.OFF || !GameOptions.IsZupDay(s_Options.ZombifiedsUpgradeDays, victor.Location.Map.LocalTime.Day)) return;
      if ((GameMode.GM_VINTAGE == Session.Get.GameMode || !s_Options.SkeletonsUpgrade) && GameActors.IsSkeletonBranch(victor.Model)) return;
      if ((GameMode.GM_VINTAGE == Session.Get.GameMode || !s_Options.RatsUpgrade) && GameActors.IsRatBranch(victor.Model)) return;
      if ((GameMode.GM_VINTAGE == Session.Get.GameMode || !s_Options.ShamblersUpgrade) && GameActors.IsShamblerBranch(victor.Model)) return;
      if (victor.Controller is PlayerController pc) {
        int turn = Session.Get.WorldTime.TurnCounter;
        // \todo cache these messages in multiple-PC case?
        PCsurvival(pc, new("You will hunt another day!", turn, Color.Green),
                       new("Welcome to the night.", turn, Color.White));
      }
    }

    private void OnNewNight() => World.Get.DoForAllActors(a => HandleNewNight(a));
    private void OnNewDay() => World.Get.DoForAllActors(a => StayingAliveAchievements(a));

    private void HandlePlayerDecideUpgrade(PlayerController pc, Actor upgradeActor)
    {
      List<Skills.IDs> upgrade = RollSkillsToUpgrade(upgradeActor, 300);
      string str = upgradeActor == Player ? "You" : upgradeActor.Name;
      if (0 >= upgrade.Count) {
        ErrorPopup(str + " can't learn anything new!");
        return;
      }

      var skills = upgradeActor.MySkills;
      OverlayPopupTitle? popup = null;

      void display() {
          var popupLines = new List<string> { "" };

          for (int iChoice = 0; iChoice < upgrade.Count; iChoice++) {
            Skills.IDs sk = upgrade[iChoice];
            int level = skills.GetSkillLevel(sk);
            string text = string.Format("{0}. {1} {2}/{3}", iChoice + 1, Skills.Name(sk), level + 1, Skills.MaxSkillLevel(sk));

            popupLines.Add(text);
            popupLines.Add("    " + DescribeSkillShort(sk));
            popupLines.Add(" ");
          }

          popupLines.Add("ESC. don't upgrade; SPACE to get wiser skills.");

          if (upgradeActor != Player) {
            var current_skills = skills.Skills;
            if (null != current_skills) {
              popupLines.Add(" ");
              popupLines.Add(upgradeActor.Name + " current skills");
              foreach (var sk in current_skills) {
                popupLines.Add(string.Format("{0} {1}", Skills.Name(sk.Key), sk.Value));
              }
            }
          }

          AddOverlay(popup = new OverlayPopupTitle(upgradeActor == Player ? "Select skill to upgrade" : "Select skill to upgrade for " + upgradeActor.Name, Color.White, popupLines.ToArray(), Color.White, Color.White, Color.Black, new Point(64, 64)));
          RedrawPlayScreen();
      }

      bool? handler(KeyEventArgs key) {
          if (key.KeyCode == Keys.Space) {
            upgrade = RollSkillsToUpgrade(upgradeActor, 300);
            RemoveOverlay(popup);
            display();
            return null;
          }

          int choiceNumber = KeyToChoiceNumber(key.KeyCode);
          if (choiceNumber >= 1 && choiceNumber <= upgrade.Count) {
            var sk = upgrade[choiceNumber - 1];
            upgradeActor.SkillUpgrade(sk);
            int skill_level = skills.GetSkillLevel(sk);
		    string msg = (1 == skill_level ? string.Format("{0} learned skill {1}.", upgradeActor.Name, Skills.Name(sk))
			            : string.Format("{0} improved skill {1} to level {2}.", upgradeActor.Name, Skills.Name(sk), skill_level));
            upgradeActor.ActorScoring.AddEvent(Session.Get.WorldTime.TurnCounter, msg);
            RemoveOverlay(popup);
            pc.AddMessageForceRead(new(msg, Session.Get.WorldTime.TurnCounter, Color.LightGreen));
            return true;
          }
          RemoveOverlay(popup);
          display();
          return null;
      }

      display();
      m_UI.Modal(handler);

      // this is the change target for becoming a cop
      if (upgradeActor.Controller is ObjectiveAI oai) {
        if (oai.CanBecomeCop()) {
          if (YesNoPopup("Become a cop") && oai.BecomeCop()) {
            // 2020-09-12: NPC CivilianAI do not have pre-existing item memory and upgrade theirs without help.
            upgradeActor.UpgradePC();
          }
        }
      }

      if (upgradeActor.Controller is PlayerController u_pc) HandlePlayerFollowersUpgrade(u_pc);
    }

#nullable restore
    private void HandlePlayerFollowersUpgrade(PlayerController pc)
    {
        var player = pc.ControlledActor;
        var followers = player.Followers;
        if (null == followers) return;
        pc.AddMessageForceRead(new("Your followers learned new skills at your side!", Session.Get.WorldTime.TurnCounter, Color.Green));
        foreach (var fo in followers) HandlePlayerDecideUpgrade(pc, fo);
    }
#nullable restore

    private void HandleLivingNPCsUpgrade(Map map)
    {
      foreach (Actor actor in map.Actors) {
        if (actor.Model.Abilities.IsUndead) continue;
        var leader = actor.LiveLeader;
        if (null != leader && leader.IsPlayer) continue; // leader triggers upgrade
        if (actor.Controller is PlayerController pc) {
          HandlePlayerDecideUpgrade(pc, actor);
          continue;
        }
        Skills.IDs? upgrade2 = NPCPickSkillToUpgrade(actor);
        if (null != upgrade2) actor.SkillUpgrade(upgrade2.Value);
      }
    }

    private void HandleUndeadNPCsUpgrade(Map map)
    {
      foreach (Actor actor in map.Actors) {
        if (!actor.Model.Abilities.IsUndead) continue;
        if ((GameMode.GM_VINTAGE == Session.Get.GameMode || !s_Options.SkeletonsUpgrade) && GameActors.IsSkeletonBranch(actor.Model)) continue;
        if ((GameMode.GM_VINTAGE == Session.Get.GameMode || !s_Options.RatsUpgrade) && GameActors.IsRatBranch(actor.Model)) continue;
        if ((GameMode.GM_VINTAGE == Session.Get.GameMode || !s_Options.ShamblersUpgrade) && GameActors.IsShamblerBranch(actor.Model)) continue;
        var leader = actor.LiveLeader;
        if (null != leader && leader.IsPlayer) continue; // leader triggers upgrade
        if (actor.Controller is PlayerController pc) {
          HandlePlayerDecideUpgrade(pc, actor);
          continue;
        }
        Skills.IDs? upgrade2 = NPCPickSkillToUpgrade(actor);
        if (null != upgrade2) actor.SkillUpgrade(upgrade2.Value);
      }
    }

    static private List<Skills.IDs> RollSkillsToUpgrade(Actor actor, int maxTries)
    {
      int capacity = actor.Model.Abilities.IsUndead ? Rules.UNDEAD_UPGRADE_SKILLS_TO_CHOOSE_FROM : Rules.UPGRADE_SKILLS_TO_CHOOSE_FROM;
      var idsList = new List<Skills.IDs>(capacity);
      for (int index = 0; index < capacity; ++index) {
        int num = 0;
        Skills.IDs? upgrade;
        do {
          upgrade = RollRandomSkillToUpgrade(actor, maxTries);
          if (null == upgrade) return idsList;
        }
        while (idsList.Contains(upgrade.Value) && ++num < maxTries);
        idsList.Add(upgrade.Value);
      }
      return idsList;
    }

    static private void RollSkillsToUpgrade(Actor actor, int maxTries, Zaimoni.Data.Stack<Skills.IDs> idsList, int capacity)
    {
      for (int index = 0; index < capacity; ++index) {
        int num = 0;
        Skills.IDs? upgrade;
        do {
          upgrade = RollRandomSkillToUpgrade(actor, maxTries);
          if (null == upgrade) return;
        }
        while (idsList.Contains(upgrade.Value) && ++num < maxTries);
        idsList.push(upgrade.Value);
      }
    }

    static private Skills.IDs? NPCPickSkillToUpgrade(Actor npc)
    {
      int capacity = npc.Model.Abilities.IsUndead ? Rules.UNDEAD_UPGRADE_SKILLS_TO_CHOOSE_FROM : Rules.UPGRADE_SKILLS_TO_CHOOSE_FROM;
      var chooseFrom = new Zaimoni.Data.Stack<Skills.IDs>(stackalloc Skills.IDs[capacity]);
      RollSkillsToUpgrade(npc, 300, chooseFrom, capacity);
      int count = chooseFrom.Count;
      if (0 == count) return null;
      var idsList = new Zaimoni.Data.Stack<Skills.IDs>(stackalloc Skills.IDs[count]);
      int num = -1;
      while(0 < count--) {
        var id = idsList[count];
        var test = NPCSkillUtility(npc, id);
        if (num > test) continue;
        if (num < test) {
          idsList.Clear();
          num = test;
        }
        idsList.push(id);
      }
      return Rules.Get.DiceRoller.Choose(idsList);
    }

#nullable enable
    static private int NPCSkillUtility(Actor actor, Skills.IDs skID)
    {
      const int USELESS_UTIL = 0;
      const int LOW_UTIL = 1;
      const int AVG_UTIL = 2;
      const int HI_UTIL = 3;

      if (actor.Model.Abilities.IsUndead) {
        switch (skID) {
          case Skills.IDs.Z_AGILE:
          case Skills.IDs.Z_STRONG:
          case Skills.IDs.Z_TOUGH:
          case Skills.IDs.Z_TRACKER:
            return AVG_UTIL;
          case Skills.IDs.Z_EATER:
          case Skills.IDs.Z_LIGHT_FEET:
            return LOW_UTIL;
          case Skills.IDs.Z_GRAB:
          case Skills.IDs.Z_INFECTOR:
          case Skills.IDs.Z_LIGHT_EATER:
            return HI_UTIL;
          default: return USELESS_UTIL;
        }
      } else {
        Inventory? inv;
        switch (skID) {
          case Skills.IDs.AGILE: return 2;
          case Skills.IDs.AWAKE: return !actor.Model.Abilities.HasToSleep ? 0 : 3;
          case Skills.IDs.BOWS:
            if (null != (inv = actor.Inventory) && inv.Has<ItemRangedWeapon>(rw => rw.Model.IsBow)) return 3;
            return 0;
          case Skills.IDs.CARPENTRY: return 1;
          case Skills.IDs.CHARISMATIC: return actor.CountFollowers <= 0 ? 0 : 1;    // ???
          case Skills.IDs.FIREARMS:
            if (null != (inv = actor.Inventory) && inv.Has<ItemRangedWeapon>(rw => rw.Model.IsFireArm)) return 3;
            return 0;
          case Skills.IDs.HARDY: return !actor.Model.Abilities.HasToSleep ? 0 : 3;
          case Skills.IDs.HAULER: return 3;
          case Skills.IDs.HIGH_STAMINA: return 2;   // alpha10 wants 3
          case Skills.IDs.LEADERSHIP: return !actor.HasLeader ? 1 : 0;    // only because of lack of chain of command
          case Skills.IDs.LIGHT_EATER: return !actor.Model.Abilities.HasToEat ? 0 : 3;
          case Skills.IDs.LIGHT_FEET: return 2;
          case Skills.IDs.LIGHT_SLEEPER: return !actor.Model.Abilities.HasToSleep ? 0 : 2;
          case Skills.IDs.MARTIAL_ARTS:
            if (null != (inv = actor.Inventory) && inv.Has<ItemWeapon>()) return 1;
            return 2;
          case Skills.IDs.MEDIC: return 1;
          case Skills.IDs.NECROLOGY: return 1;  // alpha10; previously 0
          case Skills.IDs.STRONG: return 2;
          case Skills.IDs.STRONG_PSYCHE: return !actor.Model.Abilities.HasSanity ? 0 : 3;
          case Skills.IDs.TOUGH: return 3;
          case Skills.IDs.UNSUSPICIOUS: return !Session.Get.Police.IsTargeted(actor) || actor.Model.Abilities.IsLawEnforcer ? 0 : 1;
          default: return USELESS_UTIL;
        }
      }
    }
#nullable restore

    static private Skills.IDs? RollRandomSkillToUpgrade(Actor actor, int maxTries)
    {
      int num = 0;
      bool isUndead = actor.Model.Abilities.IsUndead;
      var dr = Rules.Get.DiceRoller;
      do {  // could unroll this loop, etc -- but this is profile-cold so ok to minimize IL size
        var id = isUndead ? Skills.RollUndead(dr) : Skills.RollLiving(dr);
        if (actor.MySkills.GetSkillLevel(id) < Skills.MaxSkillLevel(id)) return id;
      } while (++num < maxTries);
      return null;
    }

#nullable enable
    private void DoLooseRandomSkill(Actor actor)
    {
      var lost = actor.MySkills.LoseRandomSkill();
      if (null != lost) {
        var witnesses = _ForceVisibleToPlayer(actor);
        if (null != witnesses) AddMessage(witnesses, MakePanopticMessage(actor, string.Format("regressed in {0}!", Skills.Name(lost.Value))));
      }
    }

    private static void ChangeWeather()
    {
      var world = World.Get;
      var turn = Session.Get.WorldTime.TurnCounter;
      string msg = world.WeatherChanges();
      // XXX \todo should be "first PC who sees sky gets message
      if (Player.CanSeeSky) // alpha10
        AddMessage(new(msg, turn, Color.White));
      // XXX \todo global event
      Player.ActorScoring.AddEvent(turn, string.Format("The weather changed to {0}.", DescribeWeather(world.Weather)));
    }
#nullable restore

    private Actor Zombify(Actor zombifier, Actor deadVictim, bool isStartingGame)
    {
#if DEBUG
      if (null == deadVictim) throw new ArgumentNullException(nameof(deadVictim));
      if (isStartingGame && null!=zombifier) throw new InvalidOperationException(nameof(isStartingGame)+" && null!="+nameof(zombifier));
#endif
      Actor actor = BaseTownGenerator.MakeZombified(zombifier, deadVictim, isStartingGame ? 0 : deadVictim.Location.Map.LocalTime.TurnCounter);
      if (deadVictim.IsPlayer) {
        Session.Get.Scoring_fatality?.SetZombifiedPlayer(actor);
        if (Session.Get.Scoring.ReincarnationNumber < s_Options.MaxReincarnations) {
          if (YesNoPopup(deadVictim.Name + " rises as a zombie.  Use a reincarnation")) {
            actor.MakePC();
            Session.Get.Scoring.UseReincarnation();
          }
        }
      }
      if (!isStartingGame) {
        deadVictim.Location.Place(actor);
	    Session.Get.Police.TrackThroughExitSpawn(actor);
      }
      var skillTable = deadVictim.MySkills;
      var skill_count = skillTable.CountSkills;
      if (0 < skill_count) {
        var roller = Rules.Get.DiceRoller;
        var sks = new Zaimoni.Data.Stack<Skills.IDs>(stackalloc Skills.IDs[skill_count]);
        int n = skillTable.GetSkills(ref sks)/2;
        while(0 < n--) {
          Skills.IDs? sk = roller.Choose(sks).Zombify();
          if (null != sk) actor.SkillUpgrade(sk.Value);
        }
        actor.RecomputeStartingStats();
      }
      if (!isStartingGame) SeeingCauseInsanity(actor, Rules.SANITY_HIT_ZOMBIFY, string.Format("{0} turning into a zombie", deadVictim.Name));
      return actor;
    }

    // These are closely coordinated with Session.Get.CurrentMap
    private static bool IsInViewRect(Point mapPosition)
    {
      return MapViewRect.Contains(mapPosition);
    }

    private static bool IsInViewRect(in Location loc)
    {
      return m_MapView.Contains(in loc);
    }

    static private ColorString WeatherStatusText()
    {
      switch(CurrentMap.Lighting)
      {
        case Lighting.DARKNESS: return new ColorString(Color.Blue,"Darkness");
        case Lighting.OUTSIDE: {
          var weather = World.Get.Weather;
          return new ColorString(WeatherColor(weather),DescribeWeather(weather));
        }
        case Lighting.LIT: return new ColorString(Color.Yellow,"Lit");
        default: throw new ArgumentOutOfRangeException("unhandled lighting");
      }
    }

    public void RedrawPlayScreen()
    {
            if (IsSimulating) return;   // deadlocks otherwise
            lock (m_UI) {
                m_UI.ClearScreen();
                m_UI.UI_DrawLine(Color.DarkGray, RIGHTPANEL_X, RIGHTPANEL_Y, LOCATIONPANEL_X, LOCATIONPANEL_Y);
                DrawMap(CurrentMap);
                m_UI.UI_DrawLine(Color.DarkGray, LOCATIONPANEL_X, MINIMAP_Y- MINITILE_SIZE, CANVAS_WIDTH, MINIMAP_Y - MINITILE_SIZE);
                if (0 >= District.UsesCrossDistrictView(CurrentMap)) {
                    DrawMiniMap(CurrentMap.Rect);
                } else {
                    Rectangle view = new Rectangle(m_MapView.Center.Position - (Point)MINIMAP_RADIUS, (Point)(1+2*MINIMAP_RADIUS));
                    DrawMiniMap(view);
                }
                m_UI.UI_DrawLine(Color.DarkGray, MESSAGES_X, MESSAGES_Y, CANVAS_WIDTH, MESSAGES_Y);
                Messages.Draw(m_UI, Session.Get.LastTurnPlayerActed, MESSAGES_X, MESSAGES_Y);
                // \todo finetune spacing
                // We had one spare line of text in the location panel; the CPU line uses it.
                m_UI.UI_DrawLine(Color.DarkGray, LOCATIONPANEL_X, LOCATIONPANEL_Y, LOCATIONPANEL_X, CANVAS_HEIGHT);
                m_UI.UI_DrawString(Color.White, CurrentMap.Name, LOCATIONPANEL_TEXT_X, LOCATIONPANEL_TEXT_Y, new Color?());
                m_UI.UI_DrawString(Color.White, LocationText(), LOCATIONPANEL_TEXT_X, LOCATIONPANEL_TEXT_Y+LINE_SPACING, new Color?());
                m_UI.UI_DrawString(Color.White, string.Format("Day  {0}", Session.Get.WorldTime.Day), LOCATIONPANEL_TEXT_X, LOCATIONPANEL_TEXT_Y+2*LINE_SPACING, new Color?());
                m_UI.UI_DrawString(Color.White, string.Format("Hour {0}", Session.Get.WorldTime.Hour), LOCATIONPANEL_TEXT_X, LOCATIONPANEL_TEXT_Y+2*LINE_SPACING+BOLD_LINE_SPACING, new Color?());
                m_UI.UI_DrawString(Session.Get.WorldTime.IsNight ? NIGHT_COLOR : DAY_COLOR, Session.Get.WorldTime.Phase.to_s(), LOCATIONPANEL_TEXT_X_COL2, LOCATIONPANEL_TEXT_Y+2*LINE_SPACING, new Color?());
                m_UI.UI_DrawString(WeatherStatusText(), LOCATIONPANEL_TEXT_X_COL2, LOCATIONPANEL_TEXT_Y+2*LINE_SPACING+BOLD_LINE_SPACING);
                // end measure from above

                if (0<play_timer.Elapsed.TotalSeconds) m_UI.UI_DrawString(Color.White, string.Format("CPU: {0} s", play_timer.Elapsed.TotalSeconds), LOCATIONPANEL_TEXT_X, CANVAS_HEIGHT - 2 * BOLD_LINE_SPACING - LINE_SPACING, new Color?());

                // measure from below
                m_UI.UI_DrawString(Color.White, string.Format("Turn {0}", Session.Get.WorldTime.TurnCounter), LOCATIONPANEL_TEXT_X, CANVAS_HEIGHT-2*BOLD_LINE_SPACING);
                m_UI.UI_DrawString(Color.White, string.Format("Score   {0}@{1}% {2}", Player.ActorScoring.TotalPoints, (int)(100.0 * (double)s_Options.DifficultyRating((GameFactions.IDs)Player.Faction.ID)), Session.DescShortGameMode(Session.Get.GameMode)), LOCATIONPANEL_TEXT_X_COL2, CANVAS_HEIGHT-2*BOLD_LINE_SPACING);
                m_UI.UI_DrawString(Color.White, string.Format("Avatar  {0}/{1}", 1 + Session.Get.Scoring.ReincarnationNumber, 1 + s_Options.MaxReincarnations), LOCATIONPANEL_TEXT_X_COL2, CANVAS_HEIGHT-BOLD_LINE_SPACING);
                if (null != m_Player) {
                  var capital_crimes = Session.Get.Police.CountCapitalCrimes(Player);
                  if (0 < capital_crimes) m_UI.UI_DrawString(Color.White, string.Format("Capital crimes {0}", capital_crimes), LOCATIONPANEL_TEXT_X, CANVAS_HEIGHT-BOLD_LINE_SPACING);
                  DrawActorStatus(Player, RIGHTPANEL_TEXT_X, RIGHTPANEL_TEXT_Y);
                  if (Player.Inventory != null && Player.Model.Abilities.HasInventory)
                    DrawInventory(Player.Inventory, "Inventory", true, Map.GROUND_INVENTORY_SLOTS, Player.Inventory.MaxCapacity, INVENTORYPANEL_X, INVENTORYPANEL_Y);
                  var invspec = Player.Location.InventoryAtFeet();
                  DrawInventory(invspec?.Inventory, "Items on ground", true, Map.GROUND_INVENTORY_SLOTS, Map.GROUND_INVENTORY_SLOTS, INVENTORYPANEL_X, GROUNDINVENTORYPANEL_Y);
                  DrawCorpsesList(Player.Location.Corpses, "Corpses on ground", Map.GROUND_INVENTORY_SLOTS, INVENTORYPANEL_X, CORPSESPANEL_Y);
                  if (0 < Player.MySkills.CountSkills)
                    DrawActorSkillTable(Player, SKILLTABLE_X, SKILLTABLE_Y_ALT);
                }
                lock (m_Overlays) {
                  foreach (Overlay mOverlay in m_Overlays)
                    mOverlay.Draw(m_UI);
                }
                m_UI.UI_Repaint();
            }  // lock (m_UI)
    }

    public void RedrawPlayScreen(UI.Message msg)
    {
      AddMessage(msg);
      RedrawPlayScreen();
    }

    public void RedrawPlayScreen(List<PlayerController> PCs, UI.Message msg)
    {
        bool rendered = false;
        foreach(var pc in PCs) if (pc.AddMessage(msg)) rendered = true;
        if (!rendered) PanViewportTo(PCs);
    }

#nullable enable
    private static string LocationText()
    {
      var loc = new Location(CurrentMap, MapViewRect.Location + (Point)HALF_VIEW_WIDTH);
      var stringBuilder = new StringBuilder(string.Format("({0},{1}) ", loc.Position.X, loc.Position.Y));
      var zonesAt = loc.Map.GetZonesAt(loc.Position);
      if (null != zonesAt) foreach (var z in zonesAt) stringBuilder.AppendFormat("{0} ", z.Name);
      return stringBuilder.ToString();
    }
#nullable restore

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
      var imageID = World.Get.Weather switch {
          Weather.RAIN => Session.Get.WorldTime.TurnCounter % 2 == 0 ? GameImages.WEATHER_RAIN1 : GameImages.WEATHER_RAIN2,
          Weather.HEAVY_RAIN => Session.Get.WorldTime.TurnCounter % 2 == 0 ? GameImages.WEATHER_HEAVY_RAIN1 : GameImages.WEATHER_HEAVY_RAIN2,
          _ => null
      };

	  ThreatTracking threats = Player.Threats;    // these two should agree on whether they're null or not
      LocationSet sights_to_see = Player.InterestingLocs;
      var items = Player.Controller.ItemMemory;

      Point point = new Point();
      const int view_squares = (2*HALF_VIEW_WIDTH+1)*(2*HALF_VIEW_HEIGHT+1);
      Span<bool> is_visible = stackalloc bool[view_squares];
      string[] overlays = new string[view_squares];
      int working = 0;

      lock(m_MapView) {
      var num1 = MapViewRect.Left;
      var num2 = MapViewRect.Right;
      var num3 = MapViewRect.Top;
      var num4 = MapViewRect.Bottom;
      var view_center = MapViewRect.Location + (Point)HALF_VIEW_WIDTH;

      // as drawing is slow, we should be able to get away with thrashing the garbage collector here
      HashSet<Point> tainted = threats?.ThreatWhere(map, MapViewRect) ?? new HashSet<Point>();
      HashSet<Point> tourism = sights_to_see?.In(map, MapViewRect) ?? new HashSet<Point>();

      // the line of fire overlay is a non-local calculation -- historically, how to draw a tile was entirely knowable from the tile and its contents
      int i = view_squares;
      while(0 < i--) {
        point = MapViewRect.convert(i);
        is_visible[i] = IsVisibleToPlayer(map, in point);
        if (is_visible[i]) {
          var actorAt = map.GetActorAtExt(point);
          if (null == actorAt) continue;
          List<Point> LoF = (actorAt.Controller as ObjectiveAI)?.GetLoF();
          if (null == LoF) continue;
          foreach(Point pt in LoF) {
            if (pt==actorAt.Location.Position) continue;
            var delta = pt - actorAt.Location.Position + point;
            if (!MapViewRect.Contains(delta)) continue;
            working = MapViewRect.convert(delta);
            if (0 > working || view_squares <= working) continue;
            if (   i > working // not yet visibility-checked
                || is_visible[working])   // known-visible
              overlays[working] = GameImages.LINE_OF_FIRE_OVERLAY;
          }
        } else {
          overlays[i] = null;
          var loc = new Location(map, point);
          if (Map.Canonical(ref loc)) {
            if (null != Player.Controller.RejectUnwanted(items?.WhatIsAt(loc), loc)) overlays[i] = GameImages.ITEMS_OVERLAY;
          }

          if (tainted.Contains(point)) {
            overlays[i] = tourism.Contains(point) ? GameImages.THREAT_AND_TOURISM_OVERLAY : GameImages.THREAT_OVERLAY;
          } else if (tourism.Contains(point)) {
            overlays[i] = GameImages.TOURISM_OVERLAY;
          }
        }
      }

      bool isUndead = Player.Model.Abilities.IsUndead;
      bool canScentTrack = 0 < Player.Model.BaseSmellRating;
      bool p_is_awake = !Player.IsSleeping;
      for (var x = num1; x < num2; ++x) {
        point.X = x;
        for (var y = num3; y < num4; ++y) {
          point.Y = y;
          var loc = new Location(map,point);
          if (!Map.Canonical(ref loc)) continue;
          working = MapViewRect.convert(point);   // likely a VM issue if this throws
          var screen = MapToScreen(x, y);
          bool player = is_visible[working];
          bool flag2 = false;
          var tile = loc.Tile;
          tile.IsInView = player;
          tile.IsVisited = Player.Controller.IsKnown(new Location(map,point));
          DrawTile(tile, screen, tint);
          if (!string.IsNullOrEmpty(overlays[working])) m_UI.UI_DrawImage(overlays[working], screen.X, screen.Y, tint);

          if (player) {
            // XXX should be visible only if underlying AI sees corpses
            var corpsesAt = loc.Corpses;
            if (null != corpsesAt) foreach (Corpse c in corpsesAt) DrawCorpse(c, screen.X, screen.Y, tint);
          }
          if (s_Options.ShowPlayerTargets && p_is_awake && Player.Location.Position == point)
            DrawPlayerActorTargets(Player);
          var mapObjectAt = loc.MapObject;
          if (null != mapObjectAt) {
            DrawMapObject(mapObjectAt, screen, tile, tint, overlays[working]);
            flag2 = true;
            if (player) {
              // XXX the two AIs that don't see items but do have inventory, are feral dogs and the insane human ai.
              var itemsAt = (mapObjectAt as ShelfLike)?.NonEmptyInventory;  // will not handle concealed inventory
              if (null != itemsAt) DrawItemsStack(itemsAt, screen, tint);
            }
          }
          if (p_is_awake && Rules.GridDistance(Player.Location.Position, in point) <= 1) {    // grid distance 1 is always valid with cross-district visibility
            if (canScentTrack && isUndead) {
                int num5 = Player.SmellThreshold;
                int scentByOdorAt1 = map.GetScentByOdorAt(Odor.LIVING, in point);
                if (scentByOdorAt1 >= num5) {
                  float num6 = (float) (0.9 * scentByOdorAt1 / OdorScent.MAX_STRENGTH);
                  m_UI.UI_DrawTransparentImage(num6 * num6, GameImages.ICON_SCENT_LIVING, screen.X, screen.Y);
                }
                int scentByOdorAt2 = map.GetScentByOdorAt(Odor.UNDEAD_MASTER, in point);
                if (scentByOdorAt2 >= num5) {
                  float num6 = (float) (0.9 * scentByOdorAt2 / OdorScent.MAX_STRENGTH);
                  m_UI.UI_DrawTransparentImage(num6 * num6, GameImages.ICON_SCENT_ZOMBIEMASTER, screen.X, screen.Y);
                }
            }
          }
          if (player) {
            // XXX the two AIs that don't see items but do have inventory, are feral dogs and the insane human ai.
            var itemsAt = loc.Items;
            if (null != itemsAt) {
              DrawItemsStack(itemsAt, screen, tint);
              flag2 = true;
            }
            var actorAt = loc.Actor;
            if (null != actorAt) {
              DrawActorSprite(actorAt, screen, tint);
              flag2 = true;
            }
          }
          if (tile.HasDecorations) flag2 = true;
          if (flag2 && tile.Model.IsWater) DrawTileWaterCover(tile, screen, tint);
          if (player && imageID != null && !tile.IsInside)
            m_UI.UI_DrawImage(imageID, screen.X, screen.Y);
          if (view_center.X==x && view_center.Y==y && (map!=Player.Location.Map || view_center!=Player.Location.Position)) {
            m_UI.UI_DrawImage(GameImages.ITEM_SLOT, screen.X, screen.Y, tint);    // XXX overload this
          }
        }
      }
      } // lock(m_MapView)
      // exit display
      var e = Player.Location.Exit; // XXX does not change w/remote viewing
      if (null != e) {
        // VAPORWARE slots above entry map would be used for rooftops, etc. (helicopters in flight cannot see within buildings but can see rooftops)
        GDI_Point screen = MapToScreen(e.Location);
        var tile = e.Location.Tile;
        tile.IsInView = !Player.IsSleeping; // these two forced-true by adjacency, when awake
        tile.IsVisited = true;
        DrawTile(tile, screen, tint);   // mostly ignore overlays (tourism and line of fire auto-cleared, threat may be inferred by other means)
        if (tile.IsInView) {
          // XXX should be visible only if underlying AI sees corpses
          var corpsesAt = e.Location.Corpses;
          if (null != corpsesAt) foreach (var c in corpsesAt) DrawCorpse(c, screen.X, screen.Y, tint);
        }
        // XXX DrawPlayerActorTargets should take account of threat at the exit
        bool flag2 = false;
        var mapObjectAt = e.Location.MapObject;
        if (mapObjectAt != null) {
          DrawMapObject(mapObjectAt, screen, tile, tint, null);
          flag2 = true;
        }
        // XXX currently smell does not go through (vertical) exits directly
        if (tile.IsInView) {
            // XXX the two AIs that don't see items but do have inventory, are feral dogs and the insane human ai.
            var itemsAt = e.Location.Items;
            if (itemsAt != null) {
              DrawItemsStack(itemsAt, screen, tint);
              flag2 = true;
            }
            var actorAt = e.Location.Actor;
            if (actorAt != null) {
              DrawActorSprite(actorAt, screen, tint);
              flag2 = true;
            }
        }
       if (tile.HasDecorations) flag2 = true;
       if (flag2 && tile.Model.IsWater) DrawTileWaterCover(tile, screen, tint);
       if (tile.IsInView && imageID != null && !tile.IsInside)
         m_UI.UI_DrawImage(imageID, screen.X, screen.Y);
      }
    }

    static private readonly string[] _movingWaterImage = new string[]{
        GameImages.TILE_FLOOR_SEWER_WATER_ANIM1,
        GameImages.TILE_FLOOR_SEWER_WATER_ANIM2,
        GameImages.TILE_FLOOR_SEWER_WATER_ANIM3
    };

    public void DrawTile(Tile tile, GDI_Point screen, Color tint)
    {
      if (tile.IsInView) {
        m_UI.UI_DrawImage(tile.Model.ImageID, screen.X, screen.Y, tint);
        if (GameTiles.FLOOR_SEWER_WATER == tile.Model) m_UI.UI_DrawImage(_movingWaterImage[Session.Get.WorldTime.TurnCounter % _movingWaterImage.Length], screen.X, screen.Y, tint);
        tile.DoForAllDecorations(decoration => m_UI.UI_DrawImage(decoration, screen.X, screen.Y));
      } else {
        if (!tile.IsVisited || Player.IsSleeping) return;
        m_UI.UI_DrawGrayLevelImage(tile.Model.ImageID, screen.X, screen.Y);
        if (GameTiles.FLOOR_SEWER_WATER == tile.Model) m_UI.UI_DrawGrayLevelImage(_movingWaterImage[Session.Get.WorldTime.TurnCounter % _movingWaterImage.Length], screen.X, screen.Y);
        tile.DoForAllDecorations(decoration => m_UI.UI_DrawGrayLevelImage(decoration, screen.X, screen.Y));
      }
    }

    public void DrawTileWaterCover(Tile tile, GDI_Point screen, Color tint)
    {
      if (tile.IsInView) {
        m_UI.UI_DrawImage(tile.Model.WaterCoverImageID, screen.X, screen.Y, tint);
      } else if (tile.IsVisited && !Player.IsSleeping) {
        m_UI.UI_DrawGrayLevelImage(tile.Model.WaterCoverImageID, screen.X, screen.Y);
      }
    }

    public void DrawMapObject(MapObject mapObj, GDI_Point screen, Tile tile, Color tint, string overlay)    // tile is the one that the map object is on.
    {
      if (mapObj.IsMovable && tile.Model.IsWater) {
        int num = (mapObj.Location.Position.X + Session.Get.WorldTime.TurnCounter) % 2 == 0 ? -2 : 0;
        screen.Y -= num;
      }
      static void draw(MapObject obj, GDI_Point scr, string imageID, Action<string, int, int> drawFn) {
        drawFn(imageID, scr.X, scr.Y);
        if (obj.IsOnFire) drawFn(GameImages.EFFECT_ONFIRE, scr.X, scr.Y);
      }

      if (tile.IsInView) {
        draw(mapObj, screen, mapObj.ImageID, (imageID, gx, gy) => m_UI.UI_DrawImage(imageID, gx, gy, tint));
        if (mapObj.HitPoints < mapObj.MaxHitPoints && mapObj.HitPoints > 0)
          DrawMapHealthBar(mapObj.HitPoints, mapObj.MaxHitPoints, screen.X, screen.Y);
        if (!(mapObj is DoorWindow doorWindow) || 0 >= doorWindow.BarricadePoints) return;
        DrawMapHealthBar(doorWindow.BarricadePoints, Rules.BARRICADING_MAX, screen.X, screen.Y, Color.Green);
        m_UI.UI_DrawImage(GameImages.EFFECT_BARRICADED, screen.X, screen.Y, tint);
      } else if (tile.IsVisited && !Player.IsSleeping) {
        draw(mapObj, screen, mapObj.HiddenImageID, (imageID, gx, gy) => m_UI.UI_DrawGrayLevelImage(imageID, gx, gy));
        if (!string.IsNullOrEmpty(overlay)) m_UI.UI_DrawImage(overlay, screen.X, screen.Y, tint);
      }
    }

    static private string FollowerIcon(Actor actor)
    {
      if (actor.HasBondWith(actor.Leader)) return GameImages.PLAYER_FOLLOWER_BOND;
      if (actor.IsTrustingLeader) return GameImages.PLAYER_FOLLOWER_TRUST;
      return GameImages.PLAYER_FOLLOWER;
    }

    private static string ThreatIcon(Actor actor)
    {
      int threat_actions = Player.HowManyTimesOtherActs(1, actor);
      if (1 > threat_actions) return GameImages.ICON_THREAT_SAFE;
      if (1 < threat_actions) return GameImages.ICON_THREAT_HIGH_DANGER;
      return GameImages.ICON_THREAT_DANGER;
    }

#nullable enable
    private static string? EnemyIcon(Actor actor) {
        if (Player.IsSelfDefenceFrom(actor)) return GameImages.ICON_SELF_DEFENCE;
        else if (Player.IsAggressorOf(actor)) return GameImages.ICON_AGGRESSOR;
        else if (Player.AreIndirectEnemies(actor)) return GameImages.ICON_INDIRECT_ENEMIES;
        return null;
    }

    private static string? RunIcon(Actor actor) {
        if (actor.IsRunning) return GameImages.ICON_RUNNING;
        else if (actor.IsCrouching) return GameImages.ICON_CROUCHING;
        else if (actor.Model.Abilities.CanRun && !actor.CanRun()) return GameImages.ICON_CANT_RUN;
        return null;
    }

    private static string? SleepIcon(Actor actor) {
        if (actor.Model.Abilities.HasToSleep) {
            if (actor.IsExhausted) return GameImages.ICON_SLEEP_EXHAUSTED;
            else if (actor.IsSleepy) return GameImages.ICON_SLEEP_SLEEPY;
            else if (actor.IsAlmostSleepy) return GameImages.ICON_SLEEP_ALMOST_SLEEPY;
        }
        return null;
    }

    private static string? HungerIcon(Actor actor) {
        if (actor.Model.Abilities.HasToEat) {
            if (actor.IsStarving) return GameImages.ICON_FOOD_STARVING;
            else if (actor.IsHungry) return GameImages.ICON_FOOD_HUNGRY;
            else if (actor.IsAlmostHungry) return GameImages.ICON_FOOD_ALMOST_HUNGRY;
        } else if (actor.Model.Abilities.IsRotting) {
            if (actor.IsRotStarving) return GameImages.ICON_ROT_STARVING;
            else if (actor.IsRotHungry) return GameImages.ICON_ROT_HUNGRY;
            else if (actor.IsAlmostRotHungry) return GameImages.ICON_ROT_ALMOST_HUNGRY;
        }
        return null;
    }

    private static string? SanityIcon(Actor actor) {
        if (actor.Model.Abilities.HasSanity) {
            if (actor.IsInsane) return GameImages.ICON_SANITY_INSANE;
            else if (actor.IsDisturbed) return GameImages.ICON_SANITY_DISTURBED;
        }
        return null;
    }
#nullable restore

    public void DrawActorSprite(Actor actor, GDI_Point screen, Color tint)
    {
#if DEBUG
      if (null == actor) throw new ArgumentNullException(nameof(actor));
#endif
      int gx2 = screen.X;
      int gy2 = screen.Y;
      if (actor.Leader == Player) m_UI.UI_DrawImage(FollowerIcon(actor), gx2, gy2, tint);

      if (actor.Model.ImageID != null) m_UI.UI_DrawImage(actor.Model.ImageID, gx2, gy2, tint);
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
        m_UI.DrawTile(gx2, gy2);    // would hand off to sprite cache here
      }

      EnemyIcon(actor)?.DrawIcon(screen, tint);

      switch (actor.Activity) {
        case Data.Activity.IDLE:
          int maxHitPoints = actor.MaxHPs;
          if (actor.HitPoints < maxHitPoints) DrawMapHealthBar(actor.HitPoints, maxHitPoints, gx2, gy2);
          RunIcon(actor)?.DrawIcon(screen, tint);
          SleepIcon(actor)?.DrawIcon(screen, tint);
          HungerIcon(actor)?.DrawIcon(screen, tint);
          SanityIcon(actor)?.DrawIcon(screen, tint);
          if (Player.CanTradeWith(actor)) {
/*          var o_oai = actor.Controller as OrderableAI;
            if (null != o_oai) {
              // \todo evaluate whether ai actually will accept any trades
            } */
            m_UI.UI_DrawImage(GameImages.ICON_CAN_TRADE, gx2, gy2, tint);
          }
          if (actor.OdorSuppressorCounter > 0) m_UI.UI_DrawImage(GameImages.ICON_ODOR_SUPPRESSED, gx2, gy2, tint);  // alpha10 odor suppressed icon (will overlap with sleep healing but its fine)

          if (actor.IsSleeping && (actor.IsOnCouch || 0 < actor.HealChanceBonus)) m_UI.UI_DrawImage(GameImages.ICON_HEALING, gx2, gy2, tint);
          if (actor.CountFollowers > 0) m_UI.UI_DrawImage(GameImages.ICON_LEADER, gx2, gy2, tint);
          if (0 < actor.MySkills.GetSkillLevel(Skills.IDs.Z_GRAB)) m_UI.UI_DrawImage(GameImages.ICON_ZGRAB, gx2, gy2, tint); // alpha10: z-grab skill warning icon
          if (!s_Options.IsCombatAssistantOn || actor == Player || !actor.IsEnemyOf(Player)) break;
          m_UI.UI_DrawImage(ThreatIcon(actor), gx2, gy2, tint);
          break;
        case Data.Activity.CHASING:
        case Data.Activity.FIGHTING:
          if (!actor.IsPlayer && null != actor.TargetActor) {
            m_UI.UI_DrawImage(((actor.TargetActor == Player) ? GameImages.ACTIVITY_CHASING_PLAYER : GameImages.ACTIVITY_CHASING), gx2, gy2, tint);
          }
          goto case Data.Activity.IDLE;
        case Data.Activity.TRACKING:
          if (!actor.IsPlayer) m_UI.UI_DrawImage(GameImages.ACTIVITY_TRACKING, gx2, gy2, tint);
          goto case Data.Activity.IDLE;
        case Data.Activity.FLEEING:
          if (!actor.IsPlayer) m_UI.UI_DrawImage(GameImages.ACTIVITY_FLEEING, gx2, gy2, tint);
          goto case Data.Activity.IDLE;
        case Data.Activity.FOLLOWING:
          if (!actor.IsPlayer && null != actor.TargetActor) {
            m_UI.UI_DrawImage((actor.TargetActor.IsPlayer ? GameImages.ACTIVITY_FOLLOWING_PLAYER : (actor.TargetActor == actor.Leader ? GameImages.ACTIVITY_FOLLOWING_LEADER : GameImages.ACTIVITY_FOLLOWING)), gx2, gy2);
          }
          goto case Data.Activity.IDLE;
        case Data.Activity.SLEEPING:
          m_UI.UI_DrawImage(GameImages.ACTIVITY_SLEEPING, gx2, gy2);
          goto case Data.Activity.IDLE;
        case Data.Activity.FOLLOWING_ORDER:
          m_UI.UI_DrawImage(GameImages.ACTIVITY_FOLLOWING_ORDER, gx2, gy2);
          goto case Data.Activity.IDLE;
        case Data.Activity.FLEEING_FROM_EXPLOSIVE:
          if (!actor.IsPlayer) m_UI.UI_DrawImage(GameImages.ACTIVITY_FLEEING_FROM_EXPLOSIVE, gx2, gy2, tint);
          goto case Data.Activity.IDLE;
        default:
          throw new InvalidOperationException("unhandled activity " + actor.Activity);
      }
    }

#nullable enable
    public void DrawActorDecoration(Actor actor, DollPart part, Color tint)
    {
      var decorations = actor.Doll.GetDecorations(part);
      if (null == decorations) return;
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
      var decorations = actor.Doll.GetDecorations(part);
      if (null == decorations) return;
      foreach (string imageID in decorations)
        m_UI.UI_DrawImageTransform(imageID, gx, gy, rotation, scale);
    }

    public void DrawActorEquipment(Actor actor, DollPart part, Color tint)
    {
      var equippedItem = actor.GetEquippedItem(part);
      if (null != equippedItem) m_UI.AppendTile(equippedItem.ImageID, tint);
    }
#nullable restore

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
        DrawActorDecoration(deadGuy, gx, gy, DollPart.EYES, rotation, scale);
        DrawActorDecoration(deadGuy, gx, gy, DollPart.HEAD, rotation, scale);
      }
      int rot = c.RotLevel;
      if (0 >= rot) return;
      string imageID = "rot" + rot + "_" + (1+Session.Get.WorldTime.TurnCounter % 2);
      int num3 = Session.Get.WorldTime.TurnCounter % 5 - 2;
      int num4 = Session.Get.WorldTime.TurnCounter / 3 % 5 - 2;
      m_UI.UI_DrawImage(imageID, gx + num3, gy + num4);
    }

    public void DrawCorpsesList(List<Corpse> list, string title, int slots, int gx, int gy)
    {
      var is_dragging = Player.DraggedCorpse;
      int num2 = list == null ? 0 : list.Count;
      if (num2 > 0) title += " : " + num2;
      m_UI.UI_DrawStringBold(Color.White, title, gx, gy - BOLD_LINE_SPACING, new Color?());
      int gx1 = gx;
      for (int index = 0; index < slots; ++index) {
        m_UI.UI_DrawImage(GameImages.ITEM_SLOT, gx1, gy);
        if (index < num2) {
          var c = list[index];
          if (is_dragging == c) m_UI.UI_DrawImage(GameImages.CORPSE_DRAGGED, gx1, gy);
          DrawCorpse(c, gx1, gy, Color.White);
        }
        gx1 += TILE_SIZE;
      }
    }

    // alpha10
    /// <summary>
    /// Highlight with overlays which visible actors are
    /// - are the target of this actor
    /// - targeting this actor
    /// - in group with this actor
    /// </summary>
    private void DrawActorRelations(Actor actor, List<Overlay> overlays)
    {
      // target of this actor
      var a_target = actor.TargetActor;
      if (null != a_target && !a_target.IsDead && IsVisibleToPlayer(a_target))
        overlays.Add(new OverlayImage(MapToScreen(a_target.Location.Position), GameImages.ICON_IS_TARGET));

      // actors targeting this actor or in same group
      bool isTargettedHighlighted = false;
      foreach (Actor other in actor.Location.Map.Actors) {
        if (other == actor || other.IsDead || !IsVisibleToPlayer(other)) continue;

        // targetting this actor
        if (other.TargetActor == actor && other.IsEngaged) {
          if (!isTargettedHighlighted) {
            overlays.Add(new OverlayImage(MapToScreen(actor.Location.Position), GameImages.ICON_IS_TARGETTED));
            isTargettedHighlighted = true;
          }
          overlays.Add(new OverlayImage(MapToScreen(other.Location.Position), GameImages.ICON_IS_TARGETING));
        }

        // in group with actor
        if (other.IsInGroupWith(actor)) overlays.Add(new OverlayImage(MapToScreen(other.Location.Position), GameImages.ICON_IS_IN_GROUP));
      }

      if (actor.Controller is ObjectiveAI ai) {
        var dests = ai.WantToGoHere(actor.Location);
        if (null != dests) foreach(var loc in dests) overlays.Add(new OverlayRect(Color.Green, new GDI_Rectangle(MapToScreen(loc), SIZE_OF_ACTOR)));
      }
    }

    public void DrawPlayerActorTargets(Actor player)
    {
      if (null != player.Sees(player.TargetActor)) {
        var screen = MapToScreen(player.TargetActor.Location);
        m_UI.UI_DrawImage(GameImages.ICON_IS_TARGET, screen.X, screen.Y);
      }
      Actor? actor;
      foreach(var loc in player.Controller.FOVloc) {
        if (loc == player.Location) continue;
        actor = loc.Actor;
        if (null==actor || actor.IsDead) continue;
        if (actor.TargetActor == player && actor.IsEngaged) {
          var screen = MapToScreen(player.Location);
          m_UI.UI_DrawImage(GameImages.ICON_IS_TARGETTED, screen.X, screen.Y);
          break;
        }
      }
    }

    public void DrawItemsStack(Inventory inventory, GDI_Point screen, Color tint)
    {
#if DEBUG
      if (0>=(inventory?.CountItems ?? 0)) throw new ArgumentNullException(nameof(inventory));
#endif
      foreach (Item it in inventory.Items)
        DrawItem(it, screen.X, screen.Y, tint);
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
      m_UI.UI_FillRect(Color.Black, new GDI_Rectangle(x, y, 20, 4));
      if (width <= 0) return;
      m_UI.UI_FillRect(barColor, new GDI_Rectangle(x + 1, y + 1, width, 2));
    }

    public void DrawBar(int value, int previousValue, int maxValue, int refValue, int maxWidth, int height, int gx, int gy, Color fillColor, Color lossFillColor, Color gainFillColor, Color emptyColor)
    {
      m_UI.UI_FillRect(emptyColor, new GDI_Rectangle(gx, gy, maxWidth, height));
      int width1 = (int)(maxWidth * (double)previousValue / maxValue);
      int width2 = (int)(maxWidth * (double)value / maxValue);
      if (value > previousValue) {
        if (width2 > 0)
          m_UI.UI_FillRect(gainFillColor, new GDI_Rectangle(gx, gy, width2, height));
        if (width1 > 0)
          m_UI.UI_FillRect(fillColor, new GDI_Rectangle(gx, gy, width1, height));
      } else if (value < previousValue) {
        if (width1 > 0)
          m_UI.UI_FillRect(lossFillColor, new GDI_Rectangle(gx, gy, width1, height));
        if (width2 > 0)
          m_UI.UI_FillRect(fillColor, new GDI_Rectangle(gx, gy, width2, height));
      }
      else if (width2 > 0)
        m_UI.UI_FillRect(fillColor, new GDI_Rectangle(gx, gy, width2, height));

      int num = (int)(maxWidth * (double)refValue / maxValue);
      m_UI.UI_DrawLine(Color.White, gx + num, gy, gx + num, gy + height);
    }

    private void DrawDetected(Actor actor, string minimap_img, string map_img, Rectangle view)
    {
      Location loc = actor.Location;
      if (loc.Map != CurrentMap) {
        Location? test = CurrentMap.Denormalize(in loc);
        if (null == test) return;   // XXX invariant failure
        loc = test.Value;
      }
      if (view.Contains(loc.Position)) {
        Point point = new Point(MINIMAP_X, MINIMAP_Y);
        point += (loc.Position - view.Location)* MINITILE_SIZE;
        m_UI.UI_DrawImage(minimap_img, point.X - 1, point.Y - 1);
      }
      if (IsInViewRect(in loc) && !IsVisibleToPlayer(actor)) {
        var screen = MapToScreen(loc);
        m_UI.UI_DrawImage(map_img, screen.X, screen.Y);
      }
    }

#if PROTOTYPE
    // experiment...icons at scale rather than 2x scale  Was hard to read.
    private void DrawDetected(Actor actor, Color minimap_color, string map_img, Rectangle view)
    {
      Location loc = actor.Location;
      if (loc.Map != Player.Location.Map) {
        Location? test = Player.Location.Map.Denormalize(loc);
        if (null == test) return;   // XXX invariant failure
        loc = test.Value;
      }
	  m_UI.UI_SetMinimapColor((loc.Position.X - view.Left), (loc.Position.Y - view.Top), minimap_color);
      if (IsInViewRect(actor.Location) && !IsVisibleToPlayer(actor)) {
        var screen = MapToScreen(loc);
        m_UI.UI_DrawImage(map_img, screen.X, screen.Y);
      }
      m_UI.UI_DrawMinimap(MINIMAP_X, MINIMAP_Y);
    }
#endif

    private void DrawMiniMap(Rectangle view)
    {
      if (null == m_Player) return;   // fail-safe.
      Map map = CurrentMap;
	  ThreatTracking threats = Player.Threats;    // these two should agree on whether they're null or not
      LocationSet sights_to_see = Player.InterestingLocs;

	  if (s_Options.IsMinimapOn) {
        m_UI.UI_ClearMinimap(Color.Black);
#region set visited tiles color.
		if (null == threats) {
          view.DoForEach(pt => m_UI.UI_SetMinimapColor(pt.X - view.Left, pt.Y - view.Top, (map.HasExitAtExt(pt) ? Color.HotPink : map.GetTileModelAtExt(pt).MinimapColor)), pt => Player.Controller.IsKnown(new Location(map, pt)));
		} else {
          HashSet<Point> tainted = threats.ThreatWhere(map, view);
          HashSet<Point> tourism = sights_to_see.In(map, view);
          view.DoForEach(pos => {
              if (tainted.Contains(pos)) {
                m_UI.UI_SetMinimapColor(pos.X-view.Left, pos.Y-view.Top, Color.Maroon);
                return;
              }
              if (tourism.Contains(pos)) {
                m_UI.UI_SetMinimapColor(pos.X - view.Left, pos.Y - view.Top, Color.Magenta);
                return;
              }
              if (!Player.Controller.IsKnown(new Location(map, pos))) return;
              m_UI.UI_SetMinimapColor(pos.X - view.Left, pos.Y - view.Top, (map.HasExitAtExt(pos) ? Color.HotPink : map.GetTileModelAtExt(pos).MinimapColor));
          });
		}
#endregion
        m_UI.UI_DrawMinimap(MINIMAP_X, MINIMAP_Y);
      }
      var minimap_delta = (MapViewRect.Location-view.Location)*MINITILE_SIZE;
      m_UI.UI_DrawRect(Color.White, new GDI_Rectangle(MINIMAP_X + minimap_delta.X, MINIMAP_Y + minimap_delta.Y, MapViewRect.Width * MINITILE_SIZE, MapViewRect.Height * MINITILE_SIZE));
      if (s_Options.ShowPlayerTagsOnMinimap) {
        view.DoForEach(pt => {
            string imageID = null;
            if (map.HasDecorationAt(GameImages.DECO_PLAYER_TAG1, in pt)) imageID = GameImages.MINI_PLAYER_TAG1;
            else if (map.HasDecorationAt(GameImages.DECO_PLAYER_TAG2, in pt)) imageID = GameImages.MINI_PLAYER_TAG2;
            else if (map.HasDecorationAt(GameImages.DECO_PLAYER_TAG3, in pt)) imageID = GameImages.MINI_PLAYER_TAG3;
            else if (map.HasDecorationAt(GameImages.DECO_PLAYER_TAG4, in pt)) imageID = GameImages.MINI_PLAYER_TAG4;
            if (imageID != null) {
              var delta = (pt - view.Location)* MINITILE_SIZE;
              m_UI.UI_DrawImage(imageID, MINIMAP_X + delta.X - 1, MINIMAP_Y + delta.Y - 1);
            }
        },
        pt => Player.Controller.IsKnown(new Location(map, pt)));
      }
      if (!Player.IsSleeping) {
        // if we see an ally, we should be able to "read off body language" who they are aiming at 2020-09-11 zaimoni
        var friends = Player.Controller.friends_in_FOV;
        if (null != friends) foreach(var fr in friends.Values) {
          if (fr.IsSleeping) continue;
          if (!(fr.Controller is ObjectiveAI oai)) continue;
          var rw = oai.GetBestRangedWeaponWithAmmo();
          int detection_range = (null != rw ? rw.Model.Attack.Range : 1);
          var fr_enemies = fr.Controller.enemies_in_FOV;
          if (null == fr_enemies) continue;
          if (1 == detection_range && null != fr.Inventory?.GetFirst(Item_IDs.UNIQUE_FATHER_TIME_SCYTHE)) detection_range=2;
          foreach(var en in fr_enemies.Values) {
            if (detection_range < Rules.InteractionDistance(fr.Location, en.Location)) continue;
            if (en.IsDead) continue;
            // \todo more relevant icons (likely want CGI generation)
            if (en.IsFaction(GameFactions.IDs.TheUndeads)) DrawDetected(en, GameImages.MINI_UNDEAD_POSITION, GameImages.TRACK_UNDEAD_POSITION, view);
            else { // \todo respond to viewer's own faction; definitely "wrong" for blackops to have out-of-sight enemies labeled as blackops
              DrawDetected(en, GameImages.MINI_BLACKOPS_POSITION, GameImages.TRACK_BLACKOPS_POSITION, view);
            }
          }
        }

	    // normal detectors/lights
        Span<bool> find_us = stackalloc bool[(int)ItemTrackerModel.TrackingOffset.STRICT_UB];
        Player.Tracks(ref find_us);

        // do not assume tracker capabilities are mutually exclusive.
        if (find_us[(int)ItemTrackerModel.TrackingOffset.FOLLOWER_AND_LEADER]) {
          // VAPORWARE Cell phones are due for a major rethinking anyway.
          // We would like the AI to be able to defend key objectives (e.g. police should want to defend any building w/generators)
          // we then could include cell phone towers as a defensible objective and make the phones actually working depending on that.
          Player.DoForAllFollowers(fo => {
            if (Player.Location.Map.IsInViewRect(fo.Location, view)) {
              if (   fo.GetEquippedItem(DollPart.LEFT_HAND) is ItemTracker tracker
                  && tracker.CanTrackFollowersOrLeader)  {
                  DrawDetected(fo, GameImages.MINI_FOLLOWER_POSITION, GameImages.TRACK_FOLLOWER_POSITION, view);
              }
            }
          });
        }
        Actor? non_self(Point pt) {
          var actor = map.GetActorAtExt(pt);
          if (IsPlayer(actor)) return null;
          return actor;
        }

        KeyValuePair<string,string>? police_icon(Actor a) {
            if (!a.HasActivePoliceRadio) return null;
            if (a.IsFaction(GameFactions.IDs.ThePolice)) return new(GameImages.MINI_POLICE_POSITION, GameImages.TRACK_POLICE_POSITION);
            var leader = a.LiveLeader;
            if (leader?.IsFaction(GameFactions.IDs.ThePolice) ?? false) return new(GameImages.MINI_POLICE_POSITION, GameImages.TRACK_POLICE_DEPUTY_POSITION);
            bool is_murderer = Player.IsFaction(GameFactions.IDs.ThePolice) && Session.Get.Police.IsTargeted(a);
            if (Player.IsEnemyOf(a)) {
                if (is_murderer) {
                     return new(GameImages.MINI_BLACKOPS_POSITION, GameImages.TRACK_POLICE_EN_MURDERER_POSITION);
                } else {
                     return new(GameImages.MINI_BLACKOPS_POSITION, GameImages.TRACK_POLICE_EN_CIVILIAN_POSITION);
                }
            } else {
                if (is_murderer) {
                     return new(GameImages.MINI_POLICE_POSITION, GameImages.TRACK_POLICE_MURDERER_POSITION);
                } else {
                     return new(GameImages.MINI_POLICE_POSITION, GameImages.TRACK_POLICE_CIVILIAN_POSITION);
                }
            }
            return null;
        }

        Action<Actor> draw = null;
        if (find_us[(int)ItemTrackerModel.TrackingOffset.UNDEADS]) {
            draw = actor => {
                if (actor.Model.Abilities.IsUndead && Rules.GridDistance(actor.Location, Player.Location) <= Rules.ZTRACKINGRADIUS) DrawDetected(actor, GameImages.MINI_UNDEAD_POSITION, GameImages.TRACK_UNDEAD_POSITION, view);
            };
        }
        if (find_us[(int)ItemTrackerModel.TrackingOffset.BLACKOPS_FACTION]) {
            draw = draw.Compose(actor => {
                if (actor.IsFaction(GameFactions.IDs.TheBlackOps)) DrawDetected(actor, GameImages.MINI_BLACKOPS_POSITION, GameImages.TRACK_BLACKOPS_POSITION, view);
            });
        }
        if (find_us[(int)ItemTrackerModel.TrackingOffset.POLICE_FACTION]) {
            draw = draw.Compose(actor => {
                var icons = police_icon(actor);
                if (null != icons) DrawDetected(actor, icons.Value.Key, icons.Value.Value, view);
            });
        }
        if (null != draw) view.DoForEach(draw, non_self);
      }	// end if (!Player.IsSleeping)
      var denorm = map.Denormalize(Player.Location);
      if (null != denorm) {
        minimap_delta = (denorm.Value.Position - view.Location)*MINITILE_SIZE;
        m_UI.UI_DrawImage(GameImages.MINI_PLAYER_POSITION, MINIMAP_X + minimap_delta.X - 1, MINIMAP_Y + minimap_delta.Y - 1);
      }
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
      if (actor.IsCrouching) return new ColorString(Color.LightGreen, "CROUCHING");
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
      Defence defence = actor.Defence;
      return (actor.Model.Abilities.IsUndead
            ? string.Format("Def {0:D2} Spd {1:F2} En {2} FoV {3} Sml {4:F2} Kills {5}", defence.Value, (actor.Speed / BASE_SPEED), actor.ActionPoints, actor.FOVrange(Session.Get.WorldTime, World.Get.Weather), actor.Smell, actor.KillsCount)
            : string.Format("Def {0:D2} Arm {1:D1}/{2:D1} Spd {3:F2} En {4} FoV {5}/{6} Fol {7}/{8}", defence.Value, defence.Protection_Hit, defence.Protection_Shot, (actor.Speed / BASE_SPEED), actor.ActionPoints, actor.FOVrange(Session.Get.WorldTime, World.Get.Weather), actor.Model.BaseViewRange, actor.CountFollowers, actor.MaxFollowers));
    }

    static private string _gameStatus(Actor a) {
      if (a != m_PlayerInTurn) return "SIMULATING";
      if (!string.IsNullOrEmpty(m_Status)) return m_Status;
      var ai = a.Controller as ObjectiveAI;
      if (null != ai) {
        var recruit = ai.RecruitableRadio();
        if (null != recruit) return "RADIO RECRUIT";
        if (null != ai.ContrafactualZTracker(a.Location)) return "WANT ZTRACKER";
      }
      if (a.Controller.InCombat) return "IN COMBAT";
      if (null != ai) {
        var code = ai.InterruptLongActivity();
        if (ObjectiveAI.ReactionCode.NONE != code) {
          if (ObjectiveAI.ReactionCode.NONE != (code & ObjectiveAI.ReactionCode.ENEMY)) return "THREAT NEAR";
          if (ObjectiveAI.ReactionCode.NONE != (code & ObjectiveAI.ReactionCode.ITEM)) return "WANTED ITEMS";   // INTERESTING ITEMS is 1 character too long
          if (ObjectiveAI.ReactionCode.NONE != (code & ObjectiveAI.ReactionCode.TRADE)) return "WANTED TRADE";
        }
      }
      return null;
    }

    static private string _gameStatusAlt(Actor a) {
      if (a != m_PlayerInTurn) return null;
      if (!string.IsNullOrEmpty(m_Status2)) return m_Status2;
      return null;
    }

    public void DrawActorStatus(Actor actor, int gx, int gy)
    {
      m_UI.UI_DrawStringBold(Color.White, string.Format("{0}, {1}", actor.Name, actor.Faction.MemberName), gx, gy, new Color?());
      gy += BOLD_LINE_SPACING;
      int maxValue1 = actor.MaxHPs;
      m_UI.UI_DrawStringBold(Color.White, string.Format("HP  {0}", actor.HitPoints), gx, gy, new Color?());
      DrawBar(actor.HitPoints, actor.PreviousHitPoints, maxValue1, 0, 100, BOLD_LINE_SPACING, gx + 70, gy, Color.Red, Color.DarkRed, Color.OrangeRed, Color.Gray);
      m_UI.UI_DrawStringBold(Color.White, string.Format("{0}", maxValue1), gx + 84 + 100, gy, new Color?());
      {
      var msg = _gameStatus(actor);
      if (!string.IsNullOrEmpty(msg)) m_UI.UI_DrawStringBold(Color.Orange, msg, gx + 126 + 100, gy, new Color?());
      }
      gy += BOLD_LINE_SPACING;
      if (actor.Model.Abilities.CanTire) {
        int maxValue2 = actor.MaxSTA;
        m_UI.UI_DrawStringBold(Color.White, string.Format("STA {0}", actor.StaminaPoints), gx, gy, new Color?());
        DrawBar(actor.StaminaPoints, actor.PreviousStaminaPoints, maxValue2, 10, 100, BOLD_LINE_SPACING, gx + 70, gy, Color.Green, Color.DarkGreen, Color.LightGreen, Color.Gray);
        m_UI.UI_DrawStringBold(Color.White, string.Format("{0}", maxValue2), gx + 84 + 100, gy, new Color?());
        m_UI.UI_DrawStringBold(ActorRunningStatus(actor), gx + 126 + 100, gy);
      }
      gy += BOLD_LINE_SPACING;
      if (actor.Model.Abilities.HasToEat) {
        int maxValue2 = actor.MaxFood;
        m_UI.UI_DrawStringBold(Color.White, string.Format("FOO {0}", actor.FoodPoints), gx, gy, new Color?());
        DrawBar(actor.FoodPoints, actor.PreviousFoodPoints, maxValue2, Actor.FOOD_HUNGRY_LEVEL, 100, BOLD_LINE_SPACING, gx + 70, gy, Color.Chocolate, Color.Brown, Color.Beige, Color.Gray);
        m_UI.UI_DrawStringBold(Color.White, string.Format("{0}", maxValue2), gx + 84 + 100, gy, new Color?());
        m_UI.UI_DrawStringBold(ActorHungerStatus(actor), gx + 126 + 100, gy);
      } else if (actor.Model.Abilities.IsRotting) {
        int maxValue2 = actor.MaxRot;
        m_UI.UI_DrawStringBold(Color.White, string.Format("ROT {0}", actor.FoodPoints), gx, gy, new Color?());
        DrawBar(actor.FoodPoints, actor.PreviousFoodPoints, maxValue2, Actor.ROT_HUNGRY_LEVEL, 100, BOLD_LINE_SPACING, gx + 70, gy, Color.Chocolate, Color.Brown, Color.Beige, Color.Gray);
        m_UI.UI_DrawStringBold(Color.White, string.Format("{0}", maxValue2), gx + 84 + 100, gy, new Color?());
        m_UI.UI_DrawStringBold(ActorRotHungerStatus(actor), gx + 126 + 100, gy);
      }
      gy += BOLD_LINE_SPACING;
      if (actor.Model.Abilities.HasToSleep) {
        int maxValue2 = actor.MaxSleep;
        m_UI.UI_DrawStringBold(Color.White, string.Format("SLP {0}", actor.SleepPoints), gx, gy, new Color?());
        DrawBar(actor.SleepPoints, actor.PreviousSleepPoints, maxValue2, Actor.SLEEP_SLEEPY_LEVEL, 100, BOLD_LINE_SPACING, gx + 70, gy, Color.Blue, Color.DarkBlue, Color.LightBlue, Color.Gray);
        m_UI.UI_DrawStringBold(Color.White, string.Format("{0}", maxValue2), gx + 84 + 100, gy, new Color?());
        m_UI.UI_DrawStringBold(ActorSleepStatus(actor), gx + 126 + 100, gy);
      }
      gy += BOLD_LINE_SPACING;
      if (actor.Model.Abilities.HasSanity) {
        int maxValue2 = actor.MaxSanity;
        m_UI.UI_DrawStringBold(Color.White, string.Format("SAN {0}", actor.Sanity), gx, gy, new Color?());
        DrawBar(actor.Sanity, actor.PreviousSanity, maxValue2, actor.DisturbedLevel, 100, BOLD_LINE_SPACING, gx + 70, gy, Color.Orange, Color.DarkOrange, Color.OrangeRed, Color.Gray);
        m_UI.UI_DrawStringBold(Color.White, string.Format("{0}", maxValue2), gx + 84 + 100, gy, new Color?());
        m_UI.UI_DrawStringBold(ActorSanityStatus(actor), gx + 126 + 100, gy);
      }
      if (Session.Get.HasInfection && !actor.Model.Abilities.IsUndead) {
        int maxValue2 = actor.InfectionHPs;
        int refValue = Rules.INFECTION_LEVEL_1_WEAK * maxValue2 / 100;
        gy += BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.White, string.Format("INF {0}", actor.Infection), gx, gy, new Color?());
        DrawBar(actor.Infection, actor.Infection, maxValue2, refValue, 100, BOLD_LINE_SPACING, gx + 70, gy, Color.Purple, Color.Black, Color.Black, Color.Gray);
        m_UI.UI_DrawStringBold(Color.White, string.Format("{0}%", actor.InfectionPercent), gx + 84 + 100, gy, new Color?());
      }
      gy += BOLD_LINE_SPACING;
      Attack attack1 = actor.MeleeAttack();
      int num1 = actor.DamageBonusVsUndeads;
      m_UI.UI_DrawStringBold(Color.White, string.Format("Melee  Atk {0:D2}  Dmg {1:D2}/{2:D2}", attack1.HitValue, attack1.DamageValue, attack1.DamageValue + num1), gx, gy, new Color?());
      {
      var msg = _gameStatusAlt(actor);
      if (!string.IsNullOrEmpty(msg)) m_UI.UI_DrawStringBold(Color.Orange, msg, gx + 126 + 100, gy, new Color?());
      }
      gy += BOLD_LINE_SPACING;
      Attack attack2 = actor.RangedAttack(actor.CurrentRangedAttack.EfficientRange);
      if (actor.GetEquippedWeapon() is ItemRangedWeapon rw) {
        m_UI.UI_DrawStringBold(Color.White, string.Format("Ranged Atk {0:D2}  Dmg {1:D2}/{2:D2} Rng {3}-{4} Amo {5}/{6}", attack2.HitValue, attack2.DamageValue, attack2.DamageValue + num1, attack2.Range, attack2.EfficientRange, rw.Ammo, rw.Model.MaxAmmo), gx, gy, new Color?());
      }
      gy += BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.White, ActorStatString(actor), gx, gy, new Color?());

      // 5. Odor suppressor // alpha10
      gy += BOLD_LINE_SPACING;
      if (0 < actor.OdorSuppressorCounter) {
        m_UI.UI_DrawStringBold(Color.LightBlue, string.Format("Odor suppr : {0} -{1}", actor.OdorSuppressorCounter, actor.Location.OdorsDecay()), gx, gy);
      }
    }

    public void DrawInventory(Inventory inventory, string title, bool drawSlotsNumbers, int slotsPerLine, int maxSlots, int gx, int gy)
    {
      m_UI.UI_DrawStringBold(Color.White, title, gx, gy-BOLD_LINE_SPACING, new Color?());
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
        if (it is ItemRangedWeapon rw) {
          if (0 >= rw.Ammo) m_UI.UI_DrawImage(GameImages.ICON_OUT_OF_AMMO, gx2, gy2);
          DrawBar(rw.Ammo, rw.Ammo, rw.Model.MaxAmmo, 0, TILE_SIZE - 4, 3, gx2 + 2, gy2 + (TILE_SIZE - 5), Color.Blue, Color.Blue, Color.Blue, Color.DarkGray);
        } else if (it is ItemSprayPaint sprayPaint) {
          DrawBar(sprayPaint.PaintQuantity, sprayPaint.PaintQuantity, sprayPaint.Model.MaxPaintQuantity, 0, TILE_SIZE - 4, 3, gx2 + 2, gy2 + (TILE_SIZE - 5), Color.Gold, Color.Gold, Color.Gold, Color.DarkGray);
        } else if (it is ItemSprayScent sprayScent) {
          DrawBar(sprayScent.SprayQuantity, sprayScent.SprayQuantity, sprayScent.Model.MaxSprayQuantity, 0, TILE_SIZE - 4, 3, gx2 + 2, gy2 + (TILE_SIZE - 5), Color.Cyan, Color.Cyan, Color.Cyan, Color.DarkGray);
        }
        else if (it is BatteryPowered electric) {
          Color bar_color = (it is ItemLight ? Color.Yellow : Color.Pink);
          if (0 >= electric.Batteries) m_UI.UI_DrawImage(GameImages.ICON_OUT_OF_BATTERIES, gx2, gy2);
          DrawBar(electric.Batteries, electric.Batteries, electric.MaxBatteries, 0, TILE_SIZE-4, 3, gx2 + 2, gy2 + (TILE_SIZE - 5), bar_color, bar_color, bar_color, Color.DarkGray);
        } else if (it is ItemFood food) food.StatusIcon()?.DrawIcon(gx2, gy2);
        else if (it is ItemTrap trap) trap.StatusIcon()?.DrawIcon(gx2, gy2);
        else if (it is ItemEntertainment ent && ent.IsBoringFor(Player))
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

#nullable enable
    public void DrawItem(Item it, int gx, int gy, Color tint)
    {
      m_UI.UI_DrawImage(it.ImageID, gx, gy, tint);
      if (it.Model.IsStackable) {
        string text = string.Format("{0}", it.Quantity);
        int gx1 = gx + TILE_SIZE - 10;
        if (it.Quantity >= 100) gx1 -= 10;
        else if (it.Quantity >= 10) gx1 -= 4;
        m_UI.UI_DrawString(Color.DarkGray, text, gx1 + 1, gy + 1, new Color?());
        m_UI.UI_DrawString(Color.White, text, gx1, gy, new Color?());
      }
      (it as ItemTrap)?.StatusIcon()?.DrawIcon(gx, gy);
    }
#nullable restore

    public void DrawActorSkillTable(Actor actor, int gx, int gy)
    {
      m_UI.UI_DrawStringBold(Color.White, "Skills", gx, gy - BOLD_LINE_SPACING, new Color?());
      var skills = actor.MySkills.Skills;
      if (skills == null) return;
      int num = 0;
      int gx1 = gx;
      int gy1 = gy;
      foreach (var skill in skills) {
        Color skColor = Color.White;

        // alpha10 highlight if active skills are active or not
        switch (skill.Key) {
          case Skills.IDs.MARTIAL_ARTS:
            {
            var w = actor.GetEquippedWeapon();
            skColor = (null == w) ? Color.LightGreen : Color.Red;
            if (w is ItemMeleeWeapon melee && melee.Model.IsMartialArts) skColor = Color.LightGreen;
            }
            break;
          case Skills.IDs.HARDY:
            if (actor.IsSleeping) skColor = Color.LightGreen;
            break;
        }

        m_UI.UI_DrawString(skColor, String.Format("{0}-", skill.Value), gx1, gy1);  // depends on skill.Value being one character to work
        m_UI.UI_DrawString(skColor, Skills.Name(skill.Key), gx1 + SKILL_LINE_SPACING, gy1);

        if (++num >= SKILLTABLE_LINES) {
          num = 0;
          gy1 = gy;
          gx1 += SKILLTABLE_COL_WIDTH;
        } else gy1 += LINE_SPACING;
      }
    }

    public void AddOverlay(Overlay o)
    {
#if DEBUG
      if (IsSimulating) throw new InvalidOperationException("simulation manipulating overlays");
#endif
      lock (m_Overlays) { m_Overlays.Add(o); }
    }

    public void ClearOverlays()
    {
#if DEBUG
      if (IsSimulating) throw new InvalidOperationException("simulation manipulating overlays");
#endif
      Interlocked.Exchange(ref s_infoOverlay, null);
      lock (m_Overlays) { m_Overlays.Clear(); }
    }

    bool RemoveOverlay(Overlay o)   // alpha10
    {
#if DEBUG
      if (IsSimulating) throw new InvalidOperationException("simulation manipulating overlays");
#endif
      Interlocked.CompareExchange(ref s_infoOverlay, null, o);
      lock (m_Overlays) { return m_Overlays.Remove(o); }
    }

    bool HasOverlay(Overlay o) // alpha10
    {
      lock (m_Overlays) { return m_Overlays.Contains(o); }
    }

    private static Overlay? s_infoOverlay = null;

    Overlay SetInfoOverlay(Overlay src) {
      // Create Read Update Delete would indicate a ClearInfoOverlay, but that is handled as formally required
      var expired = Interlocked.Exchange(ref s_infoOverlay, src);
      if (null != expired) RemoveOverlay(expired);
      return src;
    }

    private static GDI_Point MapToScreen(int x, int y)
    {
      return new GDI_Point((x - MapViewRect.Left) * TILE_SIZE, (y - MapViewRect.Top) * TILE_SIZE);
    }

    private static GDI_Point MapToScreen(Point mapPosition)
    {
      return MapToScreen(mapPosition.X, mapPosition.Y);
    }

    public static GDI_Point MapToScreen(Location loc)
    {
      if (!m_MapView.ContainsExt(in loc)) {
          var e = loc.Exit;
          if (null!=e && e.Location == m_MapView.Center) {
            // VAPORWARE slots above entry map would be used for rooftops, etc. (helicopters in flight cannot see within buildings but can see rooftops)
            int exit_slot() {
                if (District.IsEntryMap(loc.Map)) return ENTRYMAP_EXIT_SLOT;
                var e_map = e.Location.Map;
                if (District.IsEntryMap(e_map)) return ENTRYMAP_EXIT_SLOT+1;
                var e_pos = e.Location.Position;
                if (e_map.HasDecorationAt(GameImages.DECO_STAIRS_DOWN, e_pos)) return ENTRYMAP_EXIT_SLOT + 1;
                if (e_map.HasDecorationAt(GameImages.DECO_STAIRS_UP, e_pos)) return ENTRYMAP_EXIT_SLOT;
                return ENTRYMAP_EXIT_SLOT + 1;    // default
            }

            return new GDI_Point(EXIT_SLOT_X, EXIT_SLOT_Y0+TILE_SIZE* exit_slot());
          }
          return new GDI_Point(-TILE_SIZE,-TILE_SIZE);   // off-screen, expected "safe" since we get correct behavior below
      }

      if (loc.Map == CurrentMap) return MapToScreen(loc.Position);

      Location? tmp = CurrentMap.Denormalize(in loc);
#if DEBUG
      if (null == tmp) throw new ArgumentNullException(nameof(tmp));
#endif
      return MapToScreen(tmp.Value.Position);
    }

    private Vector2D_stack<int> LogicalPixel(GDI_Point mouse)   // does not belong in IRogueUI -- return type incompatible
    {
      return new Vector2D_stack<int>((int)(mouse.X / (double)m_UI.UI_GetCanvasScaleX()), (int)(mouse.Y / (double)m_UI.UI_GetCanvasScaleY()));
    }

    private Point MouseToMap(GDI_Point mouse)
    {
      var logical = LogicalPixel(mouse) / TILE_SIZE;
      return new Point((short)(MapViewRect.Left + logical.X), (short)(MapViewRect.Top + logical.Y));
    }

    private Location? MouseToLoc(int mousex, int mousey)
    {
      var pixel = LogicalPixel(new GDI_Point(mousex, mousey));
      var logical = pixel / TILE_SIZE;
      Point pos = new((short)(MapViewRect.Left + logical.X), (short)(MapViewRect.Top + logical.Y));
      if (IsInViewRect(pos)) {
        Location loc = new(CurrentMap, pos);
        if (Map.Canonical(ref loc)) return loc;
      }
      var e = m_MapView.Center.Exit;
      if (null != e) {
        Rectangle bounds = new(new(EXIT_SLOT_X, EXIT_SLOT_Y0), new(TILE_SIZE, 2* TILE_SIZE));
        if (bounds.Contains((short)pixel.X, (short)pixel.Y)) return e.Location;
      }
      return null;
    }

    private Vector2D_stack<int> MouseToInventorySlot(int invX, int invY, GDI_Point mouse)
    {
      var logical = LogicalPixel(mouse);
      logical -= new Vector2D_stack<int>(invX,invY);
      return logical/ TILE_SIZE;
    }

    static private GDI_Point InventorySlotToScreen(int invX, int invY, Vector2D_stack<int> slot)
    {
      return new GDI_Point(invX + slot.X * TILE_SIZE, invY + slot.Y * TILE_SIZE);
    }

    private static bool IsVisibleToPlayer(in Location location)
    {
      return Player.Controller.IsVisibleTo(in location);
    }

#nullable enable
    private static bool IsVisibleToPlayer(Map map, in Point position)
    {
      return Player.Controller.IsVisibleTo(map, in position);
    }

    private static bool IsVisibleToPlayer(Actor actor)
    {
      return Player.Controller.IsVisibleTo(actor);
    }

    private static bool IsVisibleToPlayer(MapObject mapObj)
    {
      return Player.Controller.IsVisibleTo(mapObj.Location);
    }
#nullable restore

    public void PanViewportTo(in Location loc)
    {
      lock(m_MapView) { m_MapView = loc.View; }
      RedrawPlayScreen();
    }

#nullable enable
    public void PanViewportTo(Actor player) {
      if (Player != player) PanViewportTo(setPlayer(player).Location);
    }

    public void PanViewportTo(List<PlayerController> witnesses) {
      if (0 >= witnesses.Count) return;
      if (Player.Controller is PlayerController pc && witnesses.Contains(pc)) RedrawPlayScreen();
      else PanViewportTo(witnesses[0].ControlledActor);
    }

    /// Debugging tool: allows AI to pray for advice to player
    public ActorAction? AI_prayer(Actor actor, List<ActorAction> considering)
    {
        if (null == considering) return null;
        if (null == actor) return null;
        if (1 >= considering.Count) return null;
        if (IsSimulating) return null;
        PanViewportTo(actor);

        ActorAction? ret = null;

        string label(int index) { return string.Format("{0} {1}.", index + 1, considering[index].ToString()); }
        bool details(int index) {
            ret = considering[index];
            return true;
        }

        PagedPopup("Advising...", considering.Count, label, details);
        return ret;
    }
#nullable restore

    /// <returns>The final location looked at if confirmed; null if cancelled</returns>
    private Location? DoPlayerFarLook()
    {
      Location origin = Player.Location;
      Location viewpoint = origin;
      OverlayPopup inspect = null;
      var overlay_anchor = MapToScreen(viewpoint);
      overlay_anchor.X += TILE_SIZE;

      var pc = Player.Controller as PlayerController;
      var available_followers = pc.AvailableFollowers;

      string label_follower(int index) {
        return available_followers[index].Name;
      };

      bool details_follower(int index) {
        var oai = available_followers[index].Controller as ObjectiveAI;
        if (available_followers[index].CanEnter(viewpoint)) {
          oai.WalkTo(in viewpoint);
          ClearOverlays();
          PanViewportTo(Player.Location);
          return true;
        }
        return false;
      };

      var desc = "FAR LOOK MODE - movement keys ok; W)alk or R)un to the waypoint";
      if (null != available_followers) desc += ", O)rder a follower to stop by the waypoint";
      desc += ", or walk 1) to 9) steps and record waypoint. ESC cancels";

      ClearOverlays();
      AddOverlay(new OverlayPopup(desc, MODE_TEXTCOLOR, MODE_BORDERCOLOR, MODE_FILLCOLOR, GDI_Point.Empty));
      RedrawPlayScreen();

      do {
          var key = m_UI.UI_WaitKey();
          switch(key.KeyCode) {
          case Keys.Escape: // cancel
            ClearOverlays();
            PanViewportTo(Player.Location);
            return null;
          case Keys.R:  // run to ...
            if (Player.CanEnter(viewpoint)) {
              pc.RunTo(in viewpoint);
              ClearOverlays();
              PanViewportTo(Player.Location);
              return null;
            }
            break;  // XXX \todo be somewhat more informative
          case Keys.W:  // walk to ...
            if (Player.CanEnter(viewpoint)) {
              pc.WalkTo(in viewpoint);
              ClearOverlays();
              PanViewportTo(Player.Location);
              return null;
            }
            break;  // XXX \todo be somewhat more informative
          case Keys.O:  // walk to ...
              // other commands possible
              if (null != available_followers && viewpoint != Player.Location && !available_followers.Any(fo => viewpoint == fo.Location)) {
                if (PagedPopup("Who stops by there?", available_followers.Count, label_follower, details_follower)) return null;
              }
            break;
          case Keys.D1:
          case Keys.D2:
          case Keys.D3:
          case Keys.D4:
          case Keys.D5:
          case Keys.D6:
          case Keys.D7:
          case Keys.D8:
          case Keys.D9:
            if (Player.CanEnter(viewpoint)) {
              pc.WalkTo(in viewpoint, (int)key.KeyCode - (int)Keys.D0);
              pc.AddWaypoint(viewpoint, "manual: T"+viewpoint.Map.LocalTime.TurnCounter.ToString());
              ClearOverlays();
              PanViewportTo(Player.Location);
              return null;
            }
            break;  // XXX \todo be somewhat more informative
          case Keys.Return: // set waypoint
            ClearOverlays();
            PanViewportTo(Player.Location);
            return viewpoint;
          default: break;
          }
          PlayerCommand command = InputTranslator.KeyToCommand(key);
          Location? tmp = null;
          switch (command) {
            // would be good if this enumeration value set was aligned [then Direction.COMPASS] makes sense]
            case PlayerCommand.MOVE_N:
            case PlayerCommand.MOVE_NE:
            case PlayerCommand.MOVE_E:
            case PlayerCommand.MOVE_SE:
            case PlayerCommand.MOVE_S:
            case PlayerCommand.MOVE_SW:
            case PlayerCommand.MOVE_W:
            case PlayerCommand.MOVE_NW:
              tmp = viewpoint + Direction.COMPASS[(int)(command) - (int)(PlayerCommand.MOVE_N)];
              if (!tmp.Value.Map.IsInBounds(tmp.Value.Position)) tmp = viewpoint.Map.Normalize(viewpoint.Position+Direction.COMPASS[(int)(command)-(int)(PlayerCommand.MOVE_N)]);
              break;
            case PlayerCommand.USE_EXIT:
              tmp = viewpoint.Exit?.Location;
              break;
            default: break; // intentionally do not handle all commands
          }
          if (null == tmp) continue;
          if (    !Player.Controller.IsKnown(tmp.Value)                             // disallow not-known
              && (viewpoint.Map==tmp.Value.Map || viewpoint.Map.DistrictPos!=tmp.Value.Map.DistrictPos))    // but ok if same-district different map i.e. stairway
              continue;    // XXX probably should have some feedback here
          if (null != inspect) {
            RemoveOverlay(inspect);
            inspect = null;
          }
          viewpoint = tmp.Value;
          // from Staying Alive: inspect mode within far-look
          if (IsVisibleToPlayer(in viewpoint)) {
            string[] lines = DescribeStuffAt(viewpoint);
            if (null != lines) inspect = new OverlayPopup(lines, Color.White, Color.White, POPUP_FILLCOLOR, overlay_anchor);
          } else {
            List<string> lines = null;
            var allies = Player.Allies;
            if (null != allies) {
                foreach(var a in allies) {
                    if (viewpoint != a.Location) continue;
                    if (Player.Controller is ObjectiveAI p_oai && p_oai.InCommunicationWith(a)) {
                      (lines ??= new()).Add(a.Name);
                      break;
                    }
                }
            }
            var threat = Player.Threats;
            if (null != threat) {
                var compromised = threat.ThreatAt(in viewpoint);
                if (0 < compromised.Count) {
                   (lines ??= new()).Add("Possibly here:");
                   foreach(var x in compromised) lines.Add(x.Name);
                }
            }
            var items = Player.Controller.ItemMemory;
            if (null != items) {
                var what = items.WhatIsAt(viewpoint);
                if (null != what) {
                    (lines ??= new()).Add("Should be here:");
                    foreach(var it in what) lines.Add(it.ToString());
                }
            }
            if (null != lines) inspect = new OverlayPopup(lines.ToArray(), Color.White, Color.White, POPUP_FILLCOLOR, overlay_anchor);
          }
          if (null != inspect) AddOverlay(inspect);
          PanViewportTo(in viewpoint);
      } while(true);
    }

    private void HandlePlayerSetWaypoint(Actor player)
    {
      Location? target = DoPlayerFarLook();
      if (null == target) return;
      // build out implementation here -- minimap and viewport have to respond appropriately
      // try a green tint
    }

    private void HandlePlayerClearWaypoint(Actor player)
    {
      var pc = player.Controller as PlayerController;
      var w_pts = pc.Waypoints;
      if (null == w_pts) {
        ErrorPopup("No waypoints set.");
        return;
      }

      string label(int index) {
        var x = w_pts[index];
        return string.Format("{0}: {1}", x.dest.ToString(), x.label);
      }
      bool details(int index) {
        var x = w_pts[index];
        PanViewportTo(x.dest);

        bool navigate(KeyEventArgs key) {
          switch (key.KeyCode)
          {
          case Keys.Delete:
            pc.RemoveWaypoint(x.dest);
            return true;
          case Keys.D1: // walk 1-9 steps to ....
          case Keys.D2:
          case Keys.D3:
          case Keys.D4:
          case Keys.D5:
          case Keys.D6:
          case Keys.D7:
          case Keys.D8:
          case Keys.D9:
            pc.WalkTo(x.dest, (int)key.KeyCode - (int)Keys.D0);
            return true;
          }
          return false;
        }

        List<string> tmp = new();
        tmp.Insert(0, "Walk 1) to 9) steps towards the waypoint, or DEL)ete the waypoint.");
        ShowSpecialDialogue(Player,tmp.ToArray(), navigate);
        return true;
      }

      PagedPopup("Reviewing...", w_pts.Count, label, details, false);
      if (Player == player) {
        PanViewportTo(player.Location);
      } else {
        PanViewportTo(player);
      }
    }

#nullable enable
    private List<PlayerController>? _ForceVisibleToPlayer(Map map, in Point position)
    {
      if (null == map) return null; // convince Duckman to not superheroically crash many games on turn 0

      Location view = new(map, position);
      if (!Map.Canonical(ref view)) return null;

      var viewing = PlayersInLOS(view);
      if (null == viewing) return null;

      if (viewing.Contains(Player.Controller as PlayerController)) return viewing;
      PanViewportTo(viewing);
      return viewing;
    }
    private List<PlayerController>? _ForceVisibleToPlayer(in Location location) => _ForceVisibleToPlayer(location.Map, location.Position);

    private List<PlayerController>? _ForceVisibleToPlayer(Actor actor)
    {
      var viewing = actor.PlayersInLOS();
      if (null == viewing) return null;
      if (actor.Controller is PlayerController a_pc) {
        if (actor == Player) return viewing;
        if (viewing.Contains(a_pc)) {
          PanViewportTo(actor);
          return viewing;
        }
      }
      if (viewing.Contains(Player.Controller as PlayerController)) return viewing;
      PanViewportTo(viewing);
      return viewing;
    }

    private List<PlayerController>? _ForceVisibleToPlayer(Actor actor, Actor want)
    {
      var viewing = actor.PlayersInLOS();
      if (null == viewing) return null;
      if (actor.Controller is PlayerController a_pc) {
        if (actor == Player) return viewing;
        if (viewing.Contains(a_pc)) {
          PanViewportTo(actor);
          return viewing;
        }
      }
      if (want.Controller is PlayerController w_pc) {
        if (want == Player) return viewing;
        if (viewing.Contains(w_pc)) {
          PanViewportTo(want);
          return viewing;
        }
      }
      if (viewing.Contains(Player.Controller as PlayerController)) return viewing;
      PanViewportTo(viewing);
      return viewing;
    }
#nullable restore

    private bool ForceVisibleToPlayer(Map map, in Point position)
    {
      if (null == map) return false; // convince Duckman to not superheroically crash many games on turn 0

      Location view = new(map, position);
      if (!Map.Canonical(ref view)) return false;

      var viewing = PlayersInLOS(view);
      if (null == viewing) return false;

      if (viewing.Contains(Player.Controller as PlayerController)) return true;
      PanViewportTo(viewing);
      return true;
    }

    public bool ForceVisibleToPlayer(Actor actor)
    {
      if (actor == Player) return true;
      if (actor.IsViewpoint) {
        PanViewportTo(actor);
        return true;
      }
      if (IsVisibleToPlayer(actor.Location)) return true;
      return ForceVisibleToPlayer(actor.Location);
    }

    private bool ForceVisibleToPlayer(MapObject mapObj) { return ForceVisibleToPlayer(mapObj.Location); }
    private bool ForceVisibleToPlayer(in Location location) { return ForceVisibleToPlayer(location.Map, location.Position); }

    private bool ForceVisibleToPlayer(in InventorySource<Item> src) {
      if (null != src.obj_owner) return ForceVisibleToPlayer(src.obj_owner);
      if (null != src.loc) return ForceVisibleToPlayer(src.loc.Value);
      // a_owner will be non-null
      return ForceVisibleToPlayer(src.a_owner);
    }

    private KeyValuePair<bool,bool> ForceVisibleToPlayer(Actor a, Actor d, List<PlayerController>? a_only_witness, List<PlayerController>?  ad_witness, List<PlayerController>?  d_only_witness)
    {
        if (a.IsPlayer) {
            PanViewportTo(a);
            return new(true,true);
        } else if (d.IsPlayer) {
            PanViewportTo(d);
            return new(IsVisibleToPlayer(a.Location), true);
        } else if (null != ad_witness) {
            PanViewportTo(ad_witness[0].ControlledActor);
            return new(true,true);
        } else if (null != d_only_witness) {
            PanViewportTo(d_only_witness[0].ControlledActor);
            return new(false,true);
        } else if (null != a_only_witness) {
            PanViewportTo(a_only_witness[0].ControlledActor);
            return new(false,true);
        }
        return default;
    }

    private KeyValuePair<bool,bool> ForceVisibleToPlayer(Actor a, List<PlayerController>? a_only_witness, List<PlayerController>?  ad_witness, List<PlayerController>?  d_only_witness)
    {
        if (a.IsPlayer) {
            PanViewportTo(a);
            return new(true,true);
        } else if (null != ad_witness) {
            PanViewportTo(ad_witness[0].ControlledActor);
            return new(true,true);
        } else if (null != d_only_witness) {
            PanViewportTo(d_only_witness[0].ControlledActor);
            return new(false,true);
        } else if (null != a_only_witness) {
            PanViewportTo(a_only_witness[0].ControlledActor);
            return new(false,true);
        }
        return default;
    }

    static private int FindLongestLine(string[] lines)
    {
      if (lines == null || lines.Length == 0) return 0;
      int num = int.MinValue;
      foreach (string line in lines) {
        if (line != null && line.Length > num) num = line.Length;
      }
      return num;
    }

    // alpha10.1 start & stop sim thread here instead of caller
    private void DoSaveGame(string saveName)
    {
#if DEBUG
      if (string.IsNullOrEmpty(saveName)) throw new ArgumentNullException(nameof(saveName));
#endif
      StopSimThread(false); // alpha10.1

      m_Status = "SAVING";
      RedrawPlayScreen();
      m_UI.UI_Repaint();
      Session.Save(Session.Get, saveName, Session.SaveFormat.FORMAT_BIN);
      File.Copy(saveName, GetUserSaveBackup(),true);
      m_Status = null;
      RedrawPlayScreen();
      m_UI.UI_Repaint();

      StartSimThread();  // alpha10.1
    }

    // alpha10.1 start & stop sim thread here instead of caller
    private void DoLoadGame(string saveName)
    {
#if DEBUG
      if (string.IsNullOrEmpty(saveName)) throw new ArgumentNullException(nameof(saveName));
#endif
      StopSimThread(false); // alpha10.1

      ClearMessages();
      RedrawPlayScreen(new UI.Message("LOADING GAME, PLEASE WAIT...", Session.Get.WorldTime.TurnCounter, Color.Yellow));
      m_UI.UI_Repaint();
      if (!LoadGame(saveName)) AddMessage(new("LOADING FAILED, NO GAME SAVED OR VERSION NOT COMPATIBLE.", Session.Get.WorldTime.TurnCounter, Color.Red));

      StartSimThread();  // alpha10.1
    }

    private void DeleteSavedGame(string saveName)
    {
#if DEBUG
      if (string.IsNullOrEmpty(saveName)) throw new ArgumentNullException(nameof(saveName));
#endif
      if (!Session.Delete(saveName)) return;
      AddMessage(new("PERMADEATH : SAVE GAME DELETED!", Session.Get.WorldTime.TurnCounter, Color.Red));
    }

    private bool LoadGame(string saveName)
    {
#if DEBUG
      if (string.IsNullOrEmpty(saveName)) throw new ArgumentNullException(nameof(saveName));
#endif
      if (!Session.Load(saveName, Session.SaveFormat.FORMAT_BIN)) return false;
      World.Get.DoForAllMaps(m=>m.RegenerateZoneExits());
      World.Get.DoForAllMaps(m=>m.RepairZoneWalk());
      Direction_ext.Now();

      // we crash on FOVloc otherwise
      World.Get.DoForAllMaps(m => { foreach (var player in m.Players.Get) { player.Controller.UpdateSensors(); } },  m => 0<m.PlayerCount);
      InfoPopup(new string[] {
          "LOADING DONE.",
          "Welcome back to " + SetupConfig.GAME_NAME + "!"
      });

      // Test drivers that require a fully constructed world can go here.
      return true;
    }

    static private void LoadOptions() {
      s_Options = GameOptions.Load(GetUserOptionsFilePath());
    }

    static private void SaveOptions()
    {
      GameOptions.Save(s_Options, GetUserOptionsFilePath());
    }

    private void ApplyOptions(bool ingame)
    {
      m_MusicManager.IsMusicEnabled = s_Options.PlayMusic;
      m_MusicManager.Volume = s_Options.MusicVolume;
      if (!m_MusicManager.IsMusicEnabled) m_MusicManager.Stop();
    }

    private void LoadKeybindings()
    {
      m_UI.DrawHeadNote("Loading keybindings...");
      s_KeyBindings = Keybindings.Load(Path.Combine(GetUserConfigPath(), "keys.dat"));
      m_UI.DrawHeadNote("Loading keybindings... done!");
    }

    private void LoadHints()
    {
      m_UI.DrawHeadNote("Loading hints...");
      s_Hints = GameHintsStatus.Load(Path.Combine(GetUserConfigPath(), "hints.dat"));
      m_UI.DrawHeadNote("Loading hints... done!");
    }

    static private void SaveHints()
    {
      GameHintsStatus.Save(s_Hints, Path.Combine(GetUserConfigPath(), "hints.dat"));
    }

    public void DrawMenuOrOptions(int currentChoice, Color entriesColor, string[] entries, Color valuesColor, string[] values, int gx, ref int gy, bool valuesOnNewLine = false, int rightPadding = 256)
    {
#if DEBUG
      if (null == entries) throw new ArgumentNullException(nameof(entries));
      if (null != values && entries.Length != values.Length) throw new InvalidOperationException("null!=entries && entries.Length != values.Length");
#endif
      int gx1 = gx + rightPadding;
      Color color = Color.FromArgb(entriesColor.A, entriesColor.R / 2, entriesColor.G / 2, entriesColor.B / 2);
      for (int index = 0; index < entries.Length; ++index) {
        string text1 = string.Format(((index != currentChoice || valuesOnNewLine) ? "     {0}" : "---> {0}"), entries[index]);
        m_UI.UI_DrawStringBold(entriesColor, text1, gx, gy, new Color?(color));
        if (values != null) {
          string text2 = index != currentChoice ? values[index] : string.Format("{0} <---", values[index]);
          if (valuesOnNewLine) {
            gy += BOLD_LINE_SPACING;
            m_UI.UI_DrawStringBold(valuesColor, text2, gx + rightPadding, gy);
          } else {
            m_UI.UI_DrawStringBold(valuesColor, text2, gx1, gy, new Color?());
          }
        }
        gy += BOLD_LINE_SPACING;
      }
    }

    public void DrawHeader()
    {
      m_UI.UI_DrawStringBold(Color.Red, SetupConfig.GAME_NAME_CAPS+" - " + SetupConfig.GAME_VERSION, 0, 0, new Color?(Color.DarkRed));
    }

    public static string GetUserBasePath() => SetupConfig.DirPath;
    public static string GetUserSavesPath() => Path.Combine(GetUserBasePath(), "Saves");
    public static string GetUserSave() => Path.Combine(GetUserSavesPath(), "save.dat");

    public static string GetUserSaveBackup()
    {
      return Path.Combine(GetUserSavesPath(),
#if DEBUG
          "save." + Session.Get.WorldTime.TurnCounter.ToString()
#else
          "save.bak"
#endif
      );
    }

    public static string GetUserDocsPath() => Path.Combine(GetUserBasePath(), "Docs") + Path.DirectorySeparatorChar;
    public static string GetUserGraveyardPath() => Path.Combine(GetUserBasePath(), "Graveyard") + Path.DirectorySeparatorChar;

    static private string GetUserNewGraveyardName()
    {
      int num = 0;
      string graveName;
      do {
        graveName = string.Format("grave_{0:D3}", num++);
      } while (File.Exists(GraveFilePath(graveName)));
      return graveName;
    }

    public static string GraveFilePath(string graveName) { return Path.Combine(GetUserGraveyardPath(), graveName + ".txt"); }
    public static string GetUserConfigPath() { return Path.Combine(GetUserBasePath(), "Config") + Path.DirectorySeparatorChar; }
    public static string GetUserOptionsFilePath() { return Path.Combine(GetUserConfigPath(), "options.dat"); }
    public static string GetUserScreenshotsPath() { return Path.Combine(GetUserBasePath(), "Screenshots") + Path.DirectorySeparatorChar; }

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

    public string ScreenshotFilePath(string shotname) { return Path.Combine(GetUserScreenshotsPath(), shotname + "." + m_UI.UI_ScreenshotExtension()); }

    private static bool CreateDirectory(string path)
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
      gy += BOLD_LINE_SPACING;
      m_UI.UI_Repaint();
      bool directory = CreateDirectory(path);
      m_UI.UI_DrawString(Color.White, "ok.", 0, gy, new Color?());
      gy += BOLD_LINE_SPACING;
      m_UI.UI_Repaint();
      return directory;
    }

    const string _manualFile = "RS Manual.txt";

    static private bool CheckCopyOfManual()
    {
      string manualDest = Path.Combine(GetUserDocsPath(), _manualFile);
      bool flag = false;
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "checking for manual...");
      if (!File.Exists(manualDest)) {
        Logger.WriteLine(Logger.Stage.INIT_MAIN, "copying manual...");
        flag = true;
        File.Copy(Path.Combine("Resources", "Manual", _manualFile), manualDest);
        Logger.WriteLine(Logger.Stage.INIT_MAIN, "copying manual... done!");
      }
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "checking for manual... done!");
      return flag;
    }

    static private string GetUserManualFilePath() { return Path.Combine(GetUserDocsPath(), _manualFile); }
    static private string GetUserHiScorePath() { return GetUserSavesPath(); }
    static private string GetUserHiScoreFilePath() { return Path.Combine(GetUserHiScorePath(), "hiscores.dat"); }
    static private string GetUserHiScoreTextFilePath() { return Path.Combine(GetUserHiScorePath(), "hiscores.txt"); }

    private void GenerateWorld(bool isVerbose)  // XXX morally part of the World constructor, but we want the World constructor to not know about game-specific content
    {
      if (isVerbose) m_UI.DrawHeadNote("Generating game world...");
      if (isVerbose) m_UI.DrawHeadNote("Creating empty world...");

      static void _validateCity()
      {
        if (!Session.CommandLineOptions.TryGetValue("city",out string x)) return;
        int split = x.IndexOf(',');
        if (   1 > split || x.Length-2 < split
            || !short.TryParse(x.AsSpan(0, split), out short city_size)
            || !GameOptions.CitySize_ok(city_size)
            || !short.TryParse(x.AsSpan(split + 1), out short district_size)
            || !GameOptions.DistrictSize_ok(district_size)) {
          Session.CommandLineOptions.Remove("city");
          return;
        }
        s_Options.CitySize = city_size;
        s_Options.DistrictSize = district_size;
      }

      static void _validateSpawn()
      {
        if (!Session.CommandLineOptions.TryGetValue("spawn",out string x)) return;
        // character 0 is the game mode.  Choice values are 0..2 currently
        // we encode this as: C)lassic, I)nfection, V)intage, Z)-war
        // character 1 is the race.  Choice values are 1..2
        // we encode this as L)iving, Z)ombie
        // character 2 is the type
        // Living: M)ale, F)emale
        // Undead: Skeleton, Shambler, Zombie Master, z.man, z.woman
        // Livings then specify a starting skill.
      }

      _validateCity();  // use the --city option values if they are remotely valid
      _validateSpawn(); // prepare to use the --spawn option

      Session.Reset(m_CharGen.GameMode);
      Rules.Reset();
      Direction_ext.Now();
      World world = World.Get;
      var zoning = world.PreliminaryZoning;
      BaseTownGenerator.WorldGenInit(zoning);
      for (short index1 = 0; index1 < world.Size; ++index1) {
        for (short index2 = 0; index2 < world.Size; ++index2) {
          if (isVerbose) m_UI.DrawHeadNote(string.Format("Creating District@{0}...", World.CoordToString(index1, index2)));
          Point dest = new(index1, index2);
          District district = (world.CHAR_CityLimits.Contains(dest)) ? new District(dest, zoning[world.CHAR_CityLimits.convert(dest)])
                            : new District(dest, DistrictKind.INTERSTATE);
          world[index1, index2] = district;
#if DEBUG
          Logger.WriteLine(Logger.Stage.RUN_MAIN, district.Kind.ToString());
#endif
          // All map generation types should have an entry map so that is sort-of-ok to have as a member function of District
          district.GenerateEntryMap(world, s_Options.DistrictSize, m_TownGenerator);
#if DEBUG
          Logger.WriteLine(Logger.Stage.RUN_MAIN, "entry map ok");
#endif
          // other (hypothetical) map generation types are not guaranteed to have sewers or subways so leave those where they are
          if (world.CHAR_CityLimits.Contains(dest)) district.GenerateSewersMap(m_TownGenerator);
#if DEBUG
          Logger.WriteLine(Logger.Stage.RUN_MAIN, "sewers map ok");
#endif
        }
      }
      if (isVerbose) m_UI.DrawHeadNote("Generating unique maps...");
      Session.Get.UniqueMaps.CHARUndergroundFacility = CreateUniqueMap_CHARUndegroundFacility(world);
      if (isVerbose) m_UI.DrawHeadNote("Generating unique actors...");
      Session.Get.UniqueActors.init_UnboundUniques(m_TownGenerator);
      Session.Get.UniqueItems.TheSubwayWorkerBadge = SpawnUniqueSubwayWorkerBadge(world);

      PlayerController.Reset(); // not safe to use before this point (relies on unique actor/item data)

      void link_N(Map origin, Map? dest) {
          if (null != dest) {
#if DEBUG
            if (dest.Width != origin.Width) throw new InvalidOperationException("mismatched district width");
#endif
            for (short x2 = 0; x2 < origin.Width; ++x2) {
                Point from1 = new Point(x2, -1);
                Point from2 = new Point(x2, dest.Height);
                Point to1 = from2 + Direction.N;
                Point to2 = from1 + Direction.S;
                if (CheckIfExitIsGood(origin, to2)) GenerateExit(dest, from2, origin, to2);
                if (CheckIfExitIsGood(dest, to1)) GenerateExit(origin, from1, dest, to1);
            }
          }
      };

      void link_W(Map origin, Map? dest) {
          if (null != dest) {
#if DEBUG
            if (dest.Height != origin.Height) throw new InvalidOperationException("mismatched district height");
#endif
            for (short y2 = 0; y2 < origin.Height; ++y2) {
                Point from1 = new Point(-1, y2);
                Point from2 = new Point(dest.Width, y2);
                Point to1 = from2 + Direction.W;
                Point to2 = from1 + Direction.E;
                if (CheckIfExitIsGood(origin, to2)) GenerateExit(dest, from2, origin, to2);
                if (CheckIfExitIsGood(dest, to1)) GenerateExit(origin, from1, dest, to1);
            }
          }
      };

      void link_NW(Map origin, Map? dest) {
          if (null != dest) {
              Point from1 = new Point(-1, -1);
              Point from2 = new Point(dest.Width, dest.Height);
              Point to1 = from2 + Direction.NW;
              Point to2 = from1 + Direction.SE;
              if (CheckIfExitIsGood(origin, to2)) GenerateExit(dest, from2, origin, to2);
              if (CheckIfExitIsGood(dest, to1)) GenerateExit(origin, from1, dest, to1);
          }
      };

      void link_NE(Map origin, Map? dest) {
          if (null != dest) {
              Point to_origin = origin.Rect.Anchor(Compass.XCOMlike.NE);
              Point to_dest = dest.Rect.Anchor(Compass.XCOMlike.SW);
              Point from_dest = dest.Rect.Anchor(Compass.XCOMlike.SW) + Direction.SW;
              Point from_origin = origin.Rect.Anchor(Compass.XCOMlike.NE) + Direction.NE;
              if (CheckIfExitIsGood(origin, to_origin)) GenerateExit(dest, from_dest, origin, to_origin);
              if (CheckIfExitIsGood(dest, to_dest)) GenerateExit(origin, from_origin, dest, to_dest);
          }
      };

      for (short x1 = 0; x1 < world.Size; ++x1) {
        for (short y1 = 0; y1 < world.Size; ++y1) {
          if (isVerbose) m_UI.DrawHeadNote(string.Format("Linking District@{0}...", World.CoordToString(x1, y1)));
          Point dest = new Point(x1, y1);

          // In RS Alpha 9, the peacewalls meant the entry map and the sewers map had to be handled differently.
          // Retain this duplication for now.
          Map entryMap1 = world[dest].EntryMap;
          link_N(entryMap1, world.At(dest + Direction.N)?.EntryMap);
          link_W(entryMap1, world.At(dest + Direction.W)?.EntryMap);
          link_NW(entryMap1, world.At(dest + Direction.NW)?.EntryMap);
          link_NE(entryMap1, world.At(dest + Direction.NE)?.EntryMap);

          var sewersMap1 = world[dest].SewersMap;
          if (null != sewersMap1) {
            link_N(sewersMap1, world.At(dest + Direction.N)?.SewersMap);
            link_W(sewersMap1, world.At(dest + Direction.W)?.SewersMap);
            link_NW(sewersMap1, world.At(dest + Direction.NW)?.SewersMap);
            link_NE(sewersMap1, world.At(dest + Direction.NE)?.SewersMap);
          }

          // Subway has a different geometry than the other two canonical maps.
          // The diagonal corridors can have only of of two exits valid.
          var subwayMap1 = world[x1, y1].SubwayMap;
          if (null != subwayMap1) {
            link_N(subwayMap1, world.At(dest + Direction.N)?.SubwayMap);
            link_W(subwayMap1, world.At(dest + Direction.W)?.SubwayMap);
          }
        }
      }
      if (isVerbose) m_UI.DrawHeadNote("Spawning player...");

      world.DoForAllMaps(m=>m.RegenerateZoneExits()); // must run early to not crash police PCs
      world.DoForAllMaps(m=>m.RepairZoneWalk());

      Map? entryMap = null;
      if (!Session.Get.CMDoptionExists("no-spawn")) {
        if (Session.CommandLineOptions.TryGetValue("spawn-district", out string district_spec)) {
          var x = Convert.ToInt32(district_spec[0]) - Convert.ToInt32('A'); // breaks down at 27 N-S extent
          var y = Convert.ToInt32(district_spec[1]) - Convert.ToInt32('0'); // breaks down at 11 E-W extent
          entryMap = world[x, y].EntryMap;
        }
        if (null == entryMap) {
          int index = world.Size / 2;
          entryMap = world[index, index].EntryMap;
        }
        GeneratePlayerOnMap(entryMap);
      } else {
        if (0 >= world.PlayerCount) throw new InvalidOperationException("hard to start a game with zero PCs");
        entryMap = world.PlayerDistricts[0].EntryMap;
      }
      SetCurrentMap(new Location(entryMap,default));    // just need a valid map value to allow player refresh
      RefreshPlayer();
      foreach(Actor player in entryMap.Players.Get) {
        player.Controller.UpdateSensors();
      }
      if (s_Options.RevealStartingDistrict) {
        foreach(Actor player in entryMap.Players.Get) {
          var zonesAt1 = entryMap.GetZonesAt(player.Location.Position);
          if (null == zonesAt1) continue;
          Zone zone = zonesAt1[0];
          entryMap.Rect.DoForEach(pt => player.Controller.ForceKnown(pt),
          pt => {
            if (!entryMap.IsInsideAt(pt)) return true;
            var zonesAt2 = entryMap.GetZonesAt(pt);
            return null != zonesAt2 && zonesAt2[0] == zone;
          });
        }
      }
#if PRERELEASE_MOTHBALL
      World.Get.DoForAllMaps(m=>m.RegenerateChokepoints());
#endif
      World.Get.DoForAllMaps(m=>m.RegenerateMapGeometry());
      World.Get.DaimonMap();    // start of game cheat map...useful for figuring out who should be PC on the command line
      if (!isVerbose) return;
      m_UI.DrawHeadNote("Generating game world... done!");
    }

    static private bool CheckIfExitIsGood(Map toMap, Point to)
    {
      return toMap.GetTileModelAt(to).IsWalkable;
    }

    static private void GenerateExit(Map fromMap, Point from, Map toMap, Point to)
    {
      fromMap.SetExitAt(from, new Exit(toMap, in to));
    }

    static private UniqueItem SpawnUniqueSubwayWorkerBadge(World world)
    {
      Item it = new Item(GameItems.UNIQUE_SUBWAY_BADGE);
      // we intentionally do not take advantage of the current subway layout algorithm
      var mapList = new List<Map>();
      world.DoForAllDistricts(d=> {
        var subway = d.SubwayMap;
        if (null != subway && BaseTownGenerator.CanHaveSubwayStationBlocks(world.SubwayLayout(d.WorldPosition))) mapList.Add(subway);
      });
      if (0 >= mapList.Count) return new UniqueItem(it, false);
      var rules = Rules.Get;
      Map map = rules.DiceRoller.Choose(mapList);
      Rectangle bounds = map.GetZoneByPartialName("rails").Bounds;
      Point point = new Point(rules.Roll(bounds.Left, bounds.Right), rules.Roll(bounds.Top, bounds.Bottom));
      map.DropItemAt(it, in point);
      map.AddDecorationAt(GameImages.DECO_BLOODIED_FLOOR, in point);
      return new UniqueItem(it);
    }

    private UniqueMap CreateUniqueMap_CHARUndegroundFacility(World world)
    {
      District district = BaseTownGenerator.GetCHARbaseDistrict();
      var zoneList = district.EntryMap.GetZonesByPartialName("CHAR Office@");
      Zone officeZone = Rules.Get.DiceRoller.Choose(zoneList);
      Map mapCharUnderground = m_TownGenerator.GenerateUniqueMap_CHARUnderground(district.EntryMap, officeZone);
      district.AddUniqueMap(mapCharUnderground);
      return new UniqueMap(mapCharUnderground);
    }

    private void GeneratePlayerOnMap(Map map)
    {
      DiceRoller roller = new DiceRoller(map.Seed);
      Actor actor;
      if (m_CharGen.IsUndead) {
        switch (m_CharGen.UndeadModel) {
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
        actor.CreateCivilianDeductFoodSleep();
      }
      actor.MakePC();
      if (Session.CommandLineOptions.TryGetValue("spawn-district", out string district_spec)) {
        var prune = district_spec.IndexOf('@');
        if (-1 < prune) district_spec = district_spec.Substring(prune+1);
        if (!string.IsNullOrEmpty(district_spec)) {
          prune = district_spec.IndexOf(',');
          if (-1 < prune) {
            var x_src = district_spec.Substring(0, prune);
            if (!string.IsNullOrEmpty(x_src)) {
              var y_src = district_spec.Substring(prune+1);
              if (!string.IsNullOrEmpty(y_src)) {
                try {
                  var x = Convert.ToInt16(x_src);
                  var y = Convert.ToInt16(y_src);
                  MapGenerator.ActorPlace(actor, new Location(map, new Point(x, y)));
                  return;
                } catch (Exception e) {}    // intentional no-op on failure
              }
            }
          }
        }
      }

      if (   MapGenerator.ActorPlace(roller, map, actor, pt => {
               if (map.IsInsideAt(pt)) {
                 if (m_CharGen.IsUndead) return false;
               } else {
                 if (!m_CharGen.IsUndead) return false;
               }
               if (IsInCHAROffice(new Location(map, pt))) return false;
               var mapObjectAt = map.GetMapObjectAt(pt);
               if (m_CharGen.IsUndead) return mapObjectAt == null;
               return mapObjectAt?.IsCouch ?? false;
             })
          || MapGenerator.ActorPlace(roller, map, actor, pt => map.IsInsideAt(pt) && !IsInCHAROffice(new Location(map, pt))))   // XXX failover only works for livings
        return;
      do;
      while (!MapGenerator.ActorPlace(roller, map, actor, pt => !IsInCHAROffice(new Location(map, pt))));
    }

    private void RefreshPlayer()
    {
      var player = CurrentMap.FindPlayer;
      if (null != player) SetCurrentMap(setPlayer(player).Location);
    }

    private void SetCurrentMap(Location loc)
    {
      if (null!=m_MapView) lock(m_MapView) { m_MapView = loc.View; }
      else m_MapView = loc.View;

      // alpha10 update background music
      UpdateBgMusic();
    }

    private SimFlags ComputeSimFlagsForTurn(int turn)
    {
      return SimFlags.HIDETAIL_TURN;
    }

    private bool SimulateNearbyDistricts(District d)
    {
      var d1 = World.Get.CurrentSimulationDistrict();
      if (null == d1) return false;
      AdvancePlay(d1, ComputeSimFlagsForTurn(d.EntryMap.LocalTime.TurnCounter));    // void SimulateDistrict(d1) if becomes complicated again
      World.Get.ScheduleAdjacentForAdvancePlay(d1);
      return true;
    }

#if OBSOLETE
    private void RestartSimThread()
    {
      StopSimThread();
      StartSimThread();
    }
#endif

    private void StartSimThread()
    {
      if (m_SimThread == null) {
        m_SimThread = new Thread(new ThreadStart(SimThreadProc)) {
          Name = "Simulation Thread"
        };
        m_SimThread.IsBackground = true;
      }
      lock (m_SimStateLock) { m_CancelSource = new CancellationTokenSource(); } // alpha10
      m_SimThread.Start();
    }

    // alpha10 StopSimThread is now blocking until the sim thread has actually stopped
    // allowed to abort when ending a game or dying because of weird bug in release build where the sim thread 
    // doesnt want to stop when dying as undead and we have to abort it(!)
    /// <param name="abort">true to stop the thread by aborting, false to stop it cleanly (recommended)</param>
    private void StopSimThread(bool abort=true)
    {
      if (null == m_SimThread) return;
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "stopping & clearing sim thread...");

      // .NET 5.0: Thread::Abort() is unsupported
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "...telling sim thread to stop");
      lock (m_SimStateLock) { m_CancelSource.Cancel(); }
      Logger.WriteLine(Logger.Stage.RUN_MAIN, "...sim thread told to stop");

      Logger.WriteLine(Logger.Stage.RUN_MAIN, "...waiting for sim thread to stop");
retry:
      bool _stopped = m_SimThread.Join(300); // outlast the Sleep call
      if (_stopped) Logger.WriteLine(Logger.Stage.RUN_MAIN, "...sim thread has stopped");
      else if (!m_SimThread.IsAlive) Logger.WriteLine(Logger.Stage.RUN_MAIN, "...sim thread is not alive and did not stop properly, consider it stopped");
      else if (!abort) goto retry;
      else Logger.WriteLine(Logger.Stage.RUN_MAIN, "...aborting sim thread"); // not really

      m_SimThread = null;
      m_CancelSource.Dispose();
      m_CancelSource = null;

      Logger.WriteLine(Logger.Stage.RUN_MAIN, "stopping & clearing sim thread done!");
    }

    private void SimThreadProc()
    {
       bool have_simulated = false;
       while (m_SimThread.IsAlive) {
         lock (m_SimStateLock) { if (m_CancelSource.IsCancellationRequested) return; } // alpha10

         lock (m_SimMutex) {
#if DEBUG
           have_simulated = SimulateNearbyDistricts(Player.Location.Map.District);
#else
           try {
             have_simulated = SimulateNearbyDistricts(Player.Location.Map.District);
           } catch (OperationCanceledException ex) {
             return;
           } catch (Exception e) {
             Logger.WriteLine(Logger.Stage.RUN_MAIN, "sim thread: exception while running sim thread!");
             Logger.WriteLine(Logger.Stage.RUN_MAIN, "sim thread: " + e.Message);
             throw; // rethrow, as we hang otherwise
           }
#endif
         }
         lock (m_SimStateLock) { if (m_CancelSource.IsCancellationRequested) return; } // .NET 5.0 conversion
         if (!have_simulated) Thread.Sleep(200);
       }
    }

    public void ShowNewAchievement(Achievement.IDs id, Actor victor)
    {
      var score = victor.ActorScoring;
      score.SetCompletedAchievement(id);
      Achievement achievement = Scoring.GetAchievement(id);
      string name = achievement.Name;
      score.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format("** Achievement : {0} for {1} points. **", name, achievement.ScoreValue));
      if (!(victor.Controller is PlayerController pc)) return;
      string[] text = achievement.Text;
      string str = new string('*', Math.Max(FindLongestLine(text), 50));
      var lines = new List<string>(text.Length + 3 + 2){
        str,
        string.Format("ACHIEVEMENT : {0}", name),
        "CONGRATULATIONS!"
      };
      lines.AddRange(text);
      lines.Add(string.Format("Achievements : {0}/{1}.", score.CompletedAchievementsCount, (int)Achievement.IDs._COUNT));
      lines.Add(str);
      if (IsSimulating || victor!=Player) {
        foreach(var msg in lines) pc.AddMessage(new(msg, Session.Get.WorldTime.TurnCounter, Color.Gold));
      } else {
        m_MusicManager.PlayLooping(achievement.MusicID, MusicPriority.PRIORITY_EVENT);
        AddOverlay(new OverlayPopup(lines.ToArray(), Color.Gold, Color.Gold, Color.DimGray, GDI_Point.Empty));
        pc.Messages.Clear();
        pc.AddMessagePressEnter();
        ClearOverlays();
      }
    }

    private void StayingAliveAchievements(Actor victor) {
      if (victor.Model.Abilities.IsUndead) return;
      if (victor.Controller is PlayerController pc) {
        var turn = Session.Get.WorldTime.TurnCounter;
        // \todo cache these messages in multiple-PC case?
        PCsurvival(pc, new("You survived another night!", turn, Color.Green),
                       new("Welcome to tomorrow.", turn, Color.White));
      }
      // XXX \todo these are notable achievements
      switch(Session.Get.WorldTime.Day - new WorldTime(victor.SpawnTime).Day) {
        case 7:  ShowNewAchievement(Achievement.IDs.REACHED_DAY_07, victor); return;
        case 14: ShowNewAchievement(Achievement.IDs.REACHED_DAY_14, victor); return;
        case 21: ShowNewAchievement(Achievement.IDs.REACHED_DAY_21, victor); return;
        case 28: ShowNewAchievement(Achievement.IDs.REACHED_DAY_28, victor); return;
        default: return;
      }
    }

#nullable enable
    private const int SHOW_SPECIAL_DIALOGUE_LINE_LIMIT = 61;
    private void ShowSpecialDialogue(Actor speaker, string[] text)
    {
      m_MusicManager.Stop();
      m_MusicManager.PlayLooping(GameMusics.INTERLUDE, MusicPriority.PRIORITY_EVENT);
      var content = new OverlayPopup(text, Color.Gold, Color.Gold, Color.DimGray, GDI_Point.Empty);
      var who = new OverlayRect(Color.Yellow, new GDI_Rectangle(MapToScreen(speaker.Location), SIZE_OF_ACTOR));
      AddOverlay(content);
      AddOverlay(who);
      RedrawPlayScreen();
      m_UI.WaitEscape();
      RemoveOverlay(who);   // alpha10 fix
      RemoveOverlay(content);
      RedrawPlayScreen();
      m_MusicManager.Stop();
    }

    private bool ShowSpecialDialogue(Actor speaker, string[] text, Predicate<KeyEventArgs> ok)
    {
      m_MusicManager.Stop();
      m_MusicManager.PlayLooping(GameMusics.INTERLUDE, MusicPriority.PRIORITY_EVENT);
      var content = new OverlayPopup(text, Color.Gold, Color.Gold, Color.DimGray, GDI_Point.Empty);
      var who = new OverlayRect(Color.Yellow, new GDI_Rectangle(MapToScreen(speaker.Location), SIZE_OF_ACTOR));
      AddOverlay(content);
      AddOverlay(who);
      RedrawPlayScreen();
      var ret = m_UI.WaitEscape(ok);
      RemoveOverlay(who);   // alpha10 fix
      RemoveOverlay(content);
      m_MusicManager.Stop();
      return ret;
    }
#nullable restore

    private void CheckSpecialPlayerEventsAfterAction(Actor player)
    { // XXX player is always m_Player here.
      // arguably, we should instead reuqire not-hostile to CHAR and actual CHAR guards for credit for breaking into a CHAR office.
      var p_map = player.Location.Map;
      if (!player.ActorScoring.HasCompletedAchievement(Achievement.IDs.CHAR_FOUND_UNDERGROUND_FACILITY)) {
        Map CHARmap = Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap;
        if (p_map == CHARmap) {
          ShowNewAchievement(Achievement.IDs.CHAR_FOUND_UNDERGROUND_FACILITY, player);
          Session.Get.PlayerKnows_CHARUndergroundFacilityLocation = true;
          Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap.Expose();
        }
      }
      (player.Controller as PlayerController)?.AfterAction();
      // The Prisoner Who Should Not Be should only respond to civilian players; other factions should either be hostile, or colluding on
      // the fake charges used to frame him (CHAR, possibly police), or conned (possibly police)
      // Acceptable factions are civilians and survivors.
      // Even if the player is of an unacceptable faction, he will be thanked if not an enemy.
      if (p_map == Session.Get.UniqueMaps.PoliceStation_JailsLevel.TheMap && 2 > Session.Get.ScriptStage_PoliceStationPrisoner && 0 <= Session.Get.ScriptStage_PoliceStationPrisoner) {
        Actor prisoner = Session.Get.UniqueActors.PoliceStationPrisoner.TheActor;
        if (prisoner.IsDead) Session.Get.ScriptStage_PoliceStationPrisoner = -1;    // disable processing; police will never realize the framing w/o other evidence
        else {
          switch(Session.Get.ScriptStage_PoliceStationPrisoner) {
          case 0:
            if (p_map.AnyAdjacent<PowerGenerator>(player.Location.Position) && IsVisibleToPlayer(prisoner)) {  // alpha10 fix: and visible!)
                string[] local_6 = null;
                if (player.IsFaction(GameFactions.IDs.TheCivilians) || player.IsFaction(GameFactions.IDs.TheSurvivors)) {
                  if (prisoner.IsSleeping) DoWakeUp(prisoner);
                  local_6 = new string[] {    // standard message
                    "\" Psssst! Hey! You over there! \"",
                    string.Format("{0} is discreetly calling you from {1} cell. You listen closely...",  prisoner.Name,  prisoner.HisOrHer),
                    "\" Listen! I shouldn't be here! Just drove a bit too fast!",
                    "  Look, I know what's happening! I worked down there! At the CHAR facility!",
                    "  They didn't want me to leave but I did! Like I'm stupid enough to stay down there uh?",
                    "  Now listen! Let's make a deal...",
                    "  Stupid cops won't listen to me. You look clever...",
                    "  You just have to push this button to open my cell.",
//                  "  You just have to push the button at the end of the corridor to open my cell.",   // \todo this text is for when the PC is not adjacent to the relevant generator (RS Alpha 10.1 change)
                    "  The cops are too busy to care about small fish like me!",
                    "  Then I'll tell you where is the underground facility and just get the hell out of here.",
                    "  I don't give a fuck about CHAR anymore, you can do what you want with that!",
                    "  There are plenty of cool stuff to loot down there!",
                    "  Do it PLEASE! I REALLY shoudn't be there! \"",
                    string.Format("Looks like {0} wants you to turn the generator on to open the cells...",  prisoner.HeOrShe)
                  };
                }
#if FAIL
                if (player.Faction == GameFactions.ThePolice) {
                }
                if (player.Faction == GameFactions.TheCHARCorporation) {
                }
#endif
                if (null != local_6) {
                  ShowSpecialDialogue(prisoner, local_6);
                  player.ActorScoring.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format("{0} offered a deal.", prisoner.Name));
                }
                Session.Get.ScriptStage_PoliceStationPrisoner = 1;
            }
            break;
          case 1:
            if (!p_map.HasZonePrefixNamedAt(prisoner.Location.Position, "jail@") && Rules.IsAdjacent(player.Location.Position, prisoner.Location.Position) && !prisoner.IsSleeping && !prisoner.IsEnemyOf(player)) {
                var base_at = Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap.District;
                string[] local_7 = new string[8]
                {
                  "\" Thank you! Thank you so much!",
                  "  As promised, I'll tell you the big secret!",
                  string.Format("  The CHAR Underground Facility is in district {0}.",  World.CoordToString(base_at.WorldPosition.X, base_at.WorldPosition.Y)),
                  "  Look for a CHAR Office, a room with an iron door.",
                  "  Now I must hurry! Thanks a lot for saving me!",
                  "  I don't want them to... UGGH...",
                  "  What's happening? NO!",
                  "  NO NOT ME! aAAAAAaaaa! NOT NOW! AAAGGGGGGGRRR \""
                };
                ShowSpecialDialogue(prisoner, local_7);
                player.ActorScoring.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format("Freed {0}.", prisoner.Name));
                Session.Get.PlayerKnows_CHARUndergroundFacilityLocation = true;
                player.ActorScoring.AddEvent(Session.Get.WorldTime.TurnCounter, "Learned the location of the CHAR Underground Facility.");
                KillActor(null, prisoner, "transformation");
                p_map.TryRemoveCorpseOf(prisoner);
                Actor local_8 = Zombify(null, prisoner, false);
                if (Session.Get.HasAllZombies) local_8.Model =  GameActors.ZombiePrince;
                local_8.APreset();   // this was warned, player should get the first move
                player.ActorScoring.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format("{0} turned into a {1}!", prisoner.Name, local_8.Model.Name));
                m_MusicManager.PlayLooping(GameMusics.FIGHT, MusicPriority.PRIORITY_EVENT);
                // VAPORWARE: this area will have working security cameras (either generator is enough to keep security cameras and the off-game
                // camera monitoring going), so the police will learn the location of the CHAR Underground Facility as well.
                District.DamnCHAROfficesToPoliceInvestigation(base_at);
                Session.Get.ScriptStage_PoliceStationPrisoner = 2;
            }
            break;
          default: throw new ArgumentOutOfRangeException("unhandled script stage " + Session.Get.ScriptStage_PoliceStationPrisoner.ToString());
          }
        }
      }

      if (District.IsSubwayMap(p_map)) {
        var badge = Session.Get.UniqueItems.TheSubwayWorkerBadge.TheItem;
        if (   badge.IsEquipped
            && player.Inventory.Contains(badge)
            && p_map.AnyAdjacent<MapObject>(player.Location.Position, mapObjectAt => MapObject.IDs.IRON_GATE_CLOSED == mapObjectAt.ID)) {
          DoTurnAllGeneratorsOn(p_map, player);
          AddMessage(new("The gate system scanned your badge and turned the power on!", Session.Get.WorldTime.TurnCounter, Color.Green));
        }
      }
      if (!player.ActorScoring.HasVisited(p_map)) {
        player.ActorScoring.AddVisit(Session.Get.WorldTime.TurnCounter, p_map);
        player.ActorScoring.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format("Visited {0}.", p_map.Name));
      }
      // XXX \todo should be for all actors
      void update_sightings(Dictionary<Location,Actor> src) {
        if (null != src) foreach (var x in src.Values) player.ActorScoring.AddSighting(x.Model.ID);
      }
      update_sightings(player.Controller.friends_in_FOV);
      update_sightings(player.Controller.enemies_in_FOV);
    }

    private void HandleReincarnation()
    {
      if (s_Options.MaxReincarnations <= 0 || !AskForReincarnation()) {
        m_MusicManager.Stop();
        return;
      }

      m_MusicManager.PlayLooping(GameMusics.LIMBO, MusicPriority.PRIORITY_EVENT);
      m_UI.ClearScreen();
      m_UI.UI_DrawStringBold(Color.Yellow, "Reincarnation - Purgatory", 0, 0);
      m_UI.UI_DrawStringBold(Color.White, "(preparing reincarnations, please wait...)", 0, 2 * BOLD_LINE_SPACING);
      m_UI.UI_Repaint();
      Actor[] reincarnationAvatars = {
        FindReincarnationAvatar(GameOptions.ReincMode.RANDOM_ACTOR, out int matchingActors1),
        FindReincarnationAvatar(GameOptions.ReincMode.RANDOM_LIVING, out int matchingActors2),
        FindReincarnationAvatar(GameOptions.ReincMode.RANDOM_UNDEAD, out int matchingActors3),
        FindReincarnationAvatar(GameOptions.ReincMode.RANDOM_FOLLOWER, out int matchingActors4),
        FindReincarnationAvatar(GameOptions.ReincMode.KILLER, out matchingActors1),
        FindReincarnationAvatar(GameOptions.ReincMode.ZOMBIFIED, out matchingActors1)
      };
      string[] strArray = CompileDistrictFunFacts(Player.Location.Map.District);
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

      Actor newPlayerAvatar = null;
      Func<int,bool?> setup_handler = (currentChoice => {
        int gy1 = 0;
        m_UI.ClearScreen();
        m_UI.UI_DrawStringBold(Color.Yellow, "Reincarnation - Choose Avatar", 0, gy1);
        gy1 += 2*BOLD_LINE_SPACING;
        DrawMenuOrOptions(currentChoice, Color.White, entries, Color.LightGreen, values, 0, ref gy1);
        gy1 += 2*BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.Pink, ".-* District Fun Facts! *-.", 0, gy1);
        gy1 += BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.Pink, string.Format("at current date : {0}.", new WorldTime(Session.Get.WorldTime.TurnCounter).ToString()), 0, gy1);
        gy1 += 2*BOLD_LINE_SPACING;
        for (int index = 0; index < strArray.Length; ++index) {
          m_UI.UI_DrawStringBold(Color.Pink, strArray[index], 0, gy1);
          gy1 += BOLD_LINE_SPACING;
        }
        m_UI.DrawFootnote("cursor to move, ENTER to select, ESC to cancel and end game");
        return null;
      });
      Func<int, bool?> choice_handler = (currentChoice => {
        newPlayerAvatar = reincarnationAvatars[currentChoice];
        if (null != newPlayerAvatar) return true;
        return null;
      });
      if (!ChoiceMenu(choice_handler, setup_handler, entries.Length)) newPlayerAvatar = null;
      if (newPlayerAvatar == null) {
        m_MusicManager.Stop();
        return;
      }

      newPlayerAvatar.MakePC();
      if (newPlayerAvatar.Activity != Data.Activity.SLEEPING) newPlayerAvatar.Activity = Data.Activity.IDLE;
      newPlayerAvatar.PrepareForPlayerControl();
      setPlayer(newPlayerAvatar).ActorScoring.AddEvent(Session.Get.WorldTime.TurnCounter, string.Format("(reincarnation {0})", Session.Get.Scoring.StartNewLife()));
      // Historically, reincarnation completely wiped the is-visited memory.  We get that for free by constructing a new PlayerController.
      // This may not be a useful idea, however.
      m_MusicManager.Stop();
      Player.Controller.UpdateSensors();
      SetCurrentMap(Player.Location);
      ClearMessages();
      RedrawPlayScreen(new UI.Message(string.Format("{0} feels disoriented for a second...", Player.Name), Session.Get.WorldTime.TurnCounter, Color.Yellow));
      m_MusicManager.Play(GameMusics.REINCARNATE, MusicPriority.PRIORITY_EVENT);
      StopSimThread(false);  // alpha10 stop-start
      StartSimThread();
    }

    static private string DescribeAvatar(Actor a)
    {
      if (a == null) return "(N/A)";
      return string.Format("{0}, a {1}{2}", a.Name, a.Model.Name, (0 < a.CountFollowers ? ", leader" : (a.HasLeader ? ", follower" : "")));
    }

    private bool AskForReincarnation()
    {
      m_UI.ClearScreen();
      m_UI.UI_DrawStringBold(Color.Yellow, "Limbo", 0, 0);
      int gy = 2 * BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.White, string.Format("Leave body {0}/{1}.", 1 + Session.Get.Scoring.ReincarnationNumber, 1 + s_Options.MaxReincarnations), 0, gy);
      gy += BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.White, "Remember lives.", 0, gy);
      gy += BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.White, "Remember purpose.", 0, gy);
      gy += BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.White, "Clear again.", 0, gy);
      gy += BOLD_LINE_SPACING;
      if (Session.Get.Scoring.ReincarnationNumber >= s_Options.MaxReincarnations) {
        m_UI.UI_DrawStringBold(Color.LightGreen, "Humans interesting.", 0, gy);
        gy += BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.LightGreen, "Time to leave.", 0, gy);
        gy += 3 * BOLD_LINE_SPACING;
        m_UI.UI_DrawStringBold(Color.Yellow, "No more reincarnations left.", 0, gy);
        m_UI.RenderFootnote("press ENTER");
        m_UI.WaitEnter();
        return false;
      }
      m_UI.UI_DrawStringBold(Color.White, "Leave?", 0, gy);
      gy += BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.White, "Live?", 0, gy);
      gy += 2 * BOLD_LINE_SPACING;
      m_UI.UI_DrawStringBold(Color.Yellow, "Reincarnate? Y to confirm, N to cancel.", 0, gy);
      m_UI.UI_Repaint();
      return m_UI.WaitYesOrNo();
    }

    static private bool IsSuitableReincarnation(Actor a, bool asLiving)
    {
      if (a == null || a.IsDead || a.IsPlayer || a.Location.Map.DistrictPos != CurrentMap.DistrictPos || (a.Location.Map == Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap || a == Session.Get.UniqueActors.PoliceStationPrisoner.TheActor || District.IsSewersMap(a.Location.Map)))
        return false;
      if (asLiving)
        return !a.Model.Abilities.IsUndead && (!s_Options.IsLivingReincRestricted || a.IsFaction(GameFactions.IDs.TheCivilians));
      return a.Model.Abilities.IsUndead && (s_Options.CanReincarnateAsRat || a.Model != GameActors.RatZombie);
    }

    static private Actor FindReincarnationAvatar(GameOptions.ReincMode reincMode, out int matchingActors)
    {
      switch (reincMode) {
        case GameOptions.ReincMode.RANDOM_FOLLOWER:
          { // scoping brace
          int count_followers = Session.Get.Scoring_fatality.FollowersWhendDied?.Count ?? 0;
          if (0 >= count_followers) {
            matchingActors = 0;
            return null;
          }
          var actorList1 = new List<Actor>(count_followers);
          foreach (Actor a in Session.Get.Scoring_fatality.FollowersWhendDied) {
            if (IsSuitableReincarnation(a, true)) actorList1.Add(a);
          }
          matchingActors = actorList1.Count;
          return 0 >= matchingActors ? null : Rules.Get.DiceRoller.Choose(actorList1);
          } // scoping brace
        case GameOptions.ReincMode.KILLER:
          Actor killer = Session.Get.Scoring_fatality.Killer;
          if (IsSuitableReincarnation(killer, true) || IsSuitableReincarnation(killer, false)) {
            matchingActors = 1;
            return killer;
          }
          matchingActors = 0;
          return null;
        case GameOptions.ReincMode.ZOMBIFIED:
          Actor zombifiedPlayer = Session.Get.Scoring_fatality.ZombifiedPlayer;
          if (IsSuitableReincarnation(zombifiedPlayer, false)) {
            matchingActors = 1;
            return zombifiedPlayer;
          }
          matchingActors = 0;
          return null;
        case GameOptions.ReincMode.RANDOM_LIVING:
        case GameOptions.ReincMode.RANDOM_UNDEAD:
        case GameOptions.ReincMode.RANDOM_ACTOR:
          bool asLiving = reincMode == GameOptions.ReincMode.RANDOM_LIVING || (reincMode == GameOptions.ReincMode.RANDOM_ACTOR && Rules.Get.RollChance(50));
          // prior implementation iterated through all districts even though IsSuitableReincarnation requires m_Session.CurrentMap.District
          var actorList2 = CurrentMap.District.FilterActors(actor => IsSuitableReincarnation(actor, asLiving));
          matchingActors = actorList2.Count;
          return 0 >= matchingActors ? null : Rules.Get.DiceRoller.Choose(actorList2);
        default: throw new ArgumentOutOfRangeException(nameof(reincMode), reincMode, "unhandled reincarnation mode");
      }
    }

#nullable enable
    private ActorAction? GenerateInsaneAction(Actor actor)
    {
      var rules = Rules.Get;
      switch (rules.Roll(0, 5)) {
        case 0: return new ActionShout(actor, "AAAAAAAAAAA!!!");
        case 1: return new ActionBump(actor, rules.RollDirection());
        case 2:
          var mapObjectAt = actor.Location.Map.GetMapObjectAt(actor.Location.Position + rules.RollDirection());
          return null == mapObjectAt ? null : new ActionBreak(actor, mapObjectAt);
        case 3:
          var inventory = actor.Inventory;
          if (null == inventory || inventory.IsEmpty) return null;
          Item it = rules.DiceRoller.Choose(inventory.Items);
          var use = new ActionUseItem(actor, it);
          if (use.IsPerformable()) return use;
          if (it.IsEquipped) {
            it.UnequippedBy(actor);
            return new ActionWait(actor);   // historically, it took 2 insane actions to drop an equipped body armor
          }
          return new ActionDropItem(actor, it);
        case 4:
          var f_in_fov = actor.Controller.friends_in_FOV;
          if (null == f_in_fov) return null;
          foreach(var renege in f_in_fov.Values) {
            if (!rules.RollChance(50)) continue;
            var leader = actor.LiveLeader;
            if (null != leader) {
              leader.RemoveFollower(actor);
              actor.TrustInLeader = 0;
            }
            DoMakeAggression(actor, renege);
            return new ActionSay(actor, renege, "YOU ARE ONE OF THEM!!", Sayflags.IS_IMPORTANT | Sayflags.IS_DANGER);    // this takes a turn unconditionally for game balance.
          }
          return null;
        default: return null;
      }
    }
#nullable restore

    private void SeeingCauseInsanity(Actor whoDoesTheAction, int sanCost, string what)
    {
      Location loc = whoDoesTheAction.Location;
      int maxLivingFOV = Actor.MaxLivingFOV(whoDoesTheAction.Location.Map);

      bool sees(Actor a) {
        if (!a.Model.Abilities.HasSanity) return false;
        if (a.IsSleeping) return false;
        return a.Controller.CanSee(loc);
      };

      var victims = loc.ThoseNearby(maxLivingFOV, sees);
      if (null == victims) return;

      var pc_did_it = whoDoesTheAction.Controller as PlayerController;

      UI.Message? pc_saw = null;
      bool need_to_message_performer = true;
      foreach(var actor in victims) {
        actor.SpendSanity(sanCost);
        var PC_witnesses = actor.PlayersInLOS();
        if (null == PC_witnesses) continue;
        bool have_messaged = false;
        if (null != pc_did_it && PC_witnesses.Remove(pc_did_it)) {
          if (need_to_message_performer) {
              pc_did_it.AddMessage(new("That was a very disturbing thing to do...", loc.Map.LocalTime.TurnCounter, Color.Orange));
              need_to_message_performer = false;
              have_messaged = true;
              PanViewportTo(whoDoesTheAction);
          }
          if (0 >= PC_witnesses.Count) continue;
        }
        if (actor.Controller is PlayerController a_pc && PC_witnesses.Remove(a_pc)) {
          a_pc.AddMessage(pc_saw ?? (pc_saw = new(string.Format("Seeing {0} is very disturbing...", what), loc.Map.LocalTime.TurnCounter, Color.Orange)));
          have_messaged = true;
          PanViewportTo(actor);
          if (0 >= PC_witnesses.Count) continue;
        }
        var broadcast = whoDoesTheAction == actor ? MakePanopticMessage(actor, string.Format("{0} done something very disturbing...", VERB_HAVE.Conjugate(actor)))
                                                  : MakePanopticMessage(actor, string.Format("{0} something very disturbing...", VERB_SEE.Conjugate(actor)));
        foreach (var witness in PC_witnesses) witness.AddMessage(broadcast);
        if (!have_messaged) {
          PanViewportTo(PC_witnesses);
          return;
        }
      }
    }

    public void OnMapPowerGeneratorSwitch(Map map, Actor victor)
    {
      if (map == Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap) {
        lock (Session.Get) {
          if (1.0 <= map.PowerRatio) {
            if (map.Illuminate(true)) {
              if (0 < map.PlayerCount) {
                ClearMessages();
                RedrawPlayScreen(new UI.Message("The Facility lights turn on!", map.LocalTime.TurnCounter, Color.Green));
              }
              // XXX \todo severe reimplementation
              if (!victor.ActorScoring.HasCompletedAchievement(Achievement.IDs.CHAR_POWER_UNDERGROUND_FACILITY))
                ShowNewAchievement(Achievement.IDs.CHAR_POWER_UNDERGROUND_FACILITY,victor);
            }
          } else if (map.Illuminate(false)) {
            if (0 < map.PlayerCount) {
              ClearMessages();
              RedrawPlayScreen(new UI.Message("The Facility lights turn off!", map.LocalTime.TurnCounter, Color.Red));
            }
          }
        }
      }
      if (District.IsSubwayMap(map)) {
        lock (Session.Get) {
          if (1.0 <= map.PowerRatio) {
            if (map.Illuminate(true)) {
              if (0 < map.PlayerCount) {
                ClearMessages();
                AddMessage(new("The station power turns on!", map.LocalTime.TurnCounter, Color.Green));
                RedrawPlayScreen(new UI.Message("You hear the gates opening.", map.LocalTime.TurnCounter, Color.Green));
              }
              map.OpenAllGates();
            }
          } else if (map.Illuminate(false)) {
            if (0 < map.PlayerCount) {
              ClearMessages();
              AddMessage(new("The station power turns off!", map.LocalTime.TurnCounter, Color.Red));
              RedrawPlayScreen(new UI.Message("You hear the gates closing.", map.LocalTime.TurnCounter, Color.Red));
            }
            CloseAllGates(map,"gates");
          }
        }
      }
      if (map == Session.Get.UniqueMaps.PoliceStation_OfficesLevel.TheMap) {
        lock (Session.Get) {
          if (1.0 <= map.PowerRatio) {
            if (0 < map.PlayerCount) {
              ClearMessages();
              RedrawPlayScreen(new UI.Message("The lights turn on.", map.LocalTime.TurnCounter, Color.Green));
            }
            Session.Get.UniqueMaps.PoliceStation_OfficesLevel.TheMap.Illuminate(true);
            Session.Get.UniqueMaps.PoliceStation_JailsLevel.TheMap.Illuminate(true);
          } else {
            if (0 < map.PlayerCount) {
              ClearMessages();
              RedrawPlayScreen(new UI.Message("The lights turn off.", map.LocalTime.TurnCounter, Color.Green));
            }
            Session.Get.UniqueMaps.PoliceStation_OfficesLevel.TheMap.Illuminate(false);
            Session.Get.UniqueMaps.PoliceStation_JailsLevel.TheMap.Illuminate(false);
          }
        }
      }
      if (map == Session.Get.UniqueMaps.PoliceStation_JailsLevel.TheMap) {
        lock (Session.Get) {
          if (1.0 <= map.PowerRatio) {
            if (0 < map.PlayerCount) {
              ClearMessages();
              RedrawPlayScreen(new UI.Message("The cells are opening.", map.LocalTime.TurnCounter, Color.Green));
            }
            map.OpenAllGates();
            // even if we missed talking to the Prisoner Who Should Not Be, make sure he'll think of thanking us if not an enemy
            if (0 == Session.Get.ScriptStage_PoliceStationPrisoner) Session.Get.ScriptStage_PoliceStationPrisoner = 1;
              // prison breakout
              Map dest = map.District.EntryMap;
              // * use Goal_PathTo on all normal prisoners.  Use an extended timer so the prisoners don't forget to escape.
              var z = dest.GetZoneByPartialName("Police Station");
              Point doorAt = z.Bounds.Anchor(Compass.XCOMlike.S);   // XXX \todo read this as the chokepoint for this zone
              var safe_zone = new HashSet<Point>();
              foreach(Point pt in Enumerable.Range(0,16).Select(i=> doorAt.RadarSweep(2,i))) {
//              if (!dest.IsInBounds(pt)) continue; // radius 3 would need this test
                if (!dest.IsWalkableFor(pt, GameActors.MaleCivilian)) continue;
                if (dest.IsInsideAt(pt)) continue;
                safe_zone.Add(pt);
              }
              var escapees = new List<Actor>();
              (new Rectangle(1,4,17,1)).DoForEach(pt => {
                Actor a = map.GetActorAt(pt);
                if (null != a) escapees.Add(a);
              });
              var escape = new Tasks.TaskEscapeNanny(escapees, safe_zone);
              escape.Trigger(dest);
              dest.AddTimer(escape);
              // * VAPORWARE: AI isn't otherwise there, so don't worry about waking up anyone who slept through the gates opening (yet)
              // Currently, there is an off-map security camera control room (the player's jailbreak *is* recorded...as is the Prisoner Who Should Not Be's confession)
              // * VAPORWARE: actually have that map (may be hard to reach, e.g. concealed doors/stairs)
              // * VAPORWARE: have the Prisoner Who Should Not Be's confession cancel the charges related to the jailbreak
              // * VAPORWARE: just because the police are letting the player off, doesn't mean they're letting the prisoners off.  Charges are just
              //   "low priority" compared to Z or murderers.  Any unoccupied police in the area can set up ambushes, etc.
          } else {
            if (0 < map.PlayerCount) {
              ClearMessages();
              RedrawPlayScreen(new UI.Message("The cells are closing.", map.LocalTime.TurnCounter, Color.Green));
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
            RedrawPlayScreen(new UI.Message("The lights turn on and you hear something opening upstairs.", map.LocalTime.TurnCounter, Color.Green));
          }
          DoHospitalPowerOn();
        } else {
          if (map.Lighting == Lighting.DARKNESS) return;
          if (0 < map.PlayerCount) {
            ClearMessages();
            RedrawPlayScreen(new UI.Message("The lights turn off and you hear something closing upstairs.", map.LocalTime.TurnCounter, Color.Green));
          }
          DoHospitalPowerOff();
        }
      }
    }

#nullable enable
    private void CloseAllGates(Map map,string gate_name)
    {
      string singular_gate = gate_name;
      if (singular_gate.EndsWith('s')) singular_gate = singular_gate.Substring(0, singular_gate.Length-1);
      var closing = singular_gate+" closing";
      map.DoForAllMapObjects(obj => {
          if (MapObject.IDs.IRON_GATE_OPEN != obj.ID) return;
          obj.ID = MapObject.IDs.IRON_GATE_CLOSED;
          OnLoudNoise(obj.Location, closing);

          Actor? actorAt = map.GetActorAt(obj.Location.Position);
          if (null == actorAt) return;
          KillActor(null, actorAt, "crushed");    // XXX \todo credit the gate operator with a murder (with usual exemptions)
          var witnesses = _ForceVisibleToPlayer(actorAt);
          if (null != witnesses) AddMessage(witnesses, MakePanopticMessage(actorAt, string.Format("{0} {1} crushed between the closing " + gate_name + "!", VERB_BE.Conjugate(actorAt))));
      });
    }
#nullable restore

    static private void DoHospitalPowerOn()
    {
      var unique_maps = Session.Get.UniqueMaps;
      unique_maps.Hospital_Admissions.TheMap.Illuminate(true);
      unique_maps.Hospital_Offices.TheMap.Illuminate(true);
      unique_maps.Hospital_Patients.TheMap.Illuminate(true);
      unique_maps.Hospital_Power.TheMap.Illuminate(true);
      Map storage = unique_maps.Hospital_Storage.TheMap;
      storage.Illuminate(true);
      storage.OpenAllGates();    // other hospital maps do not have gates so no-op

      // handwaving police investigation of hospital storage for now
      // XXX \todo it should be obvious that the generators have been turned on from outside.  We then can trigger this
      // by a policeman sighting this.
      if (3>Session.Get.ScriptStage_HospitalPowerup) {
        Session.Get.ScriptStage_HospitalPowerup = 3;    // hospital has regained power
        foreach(var z in storage.Zones) {
          if ("storage@" != z.Name.Substring(0, "storage@".Length)) continue;
          z.Bounds.DoForEach(pt => {
            if (!storage.GetTileModelAt(pt).IsWalkable) return;  // storage.IsWalkable(pt) fails for intact containers
            if (storage.AnyAdjacent<DoorWindow>(pt)) return;
            Session.Get.Police.Investigate.Record(storage, in pt);
          });
        }
      }
    }

    private void DoHospitalPowerOff()
    {
      var unique_maps = Session.Get.UniqueMaps;
      unique_maps.Hospital_Admissions.TheMap.Illuminate(false);
      unique_maps.Hospital_Offices.TheMap.Illuminate(false);
      unique_maps.Hospital_Patients.TheMap.Illuminate(false);
      unique_maps.Hospital_Power.TheMap.Illuminate(false);
      Map storage = unique_maps.Hospital_Storage.TheMap;
      storage.Illuminate(false);
      CloseAllGates(storage, "gate");
    }

    static private void DoTurnAllGeneratorsOn(Map map, Actor victor)
    {
      foreach (var powGen in map.PowerGenerators.Get) {
        if (!powGen.IsOn) powGen.TogglePower(victor);
      }
    }

    public static bool IsInCHAROffice(Location location)
    {
      return location.Map.HasZonePrefixNamedAt(location.Position, "CHAR Office@");
    }

    static public bool IsInCHARProperty(Location location)
    {
      return location.Map == Session.Get.UniqueMaps.CHARUndergroundFacility.TheMap || IsInCHAROffice(location);
    }

    static private bool AreLinkedByPhone(Actor speaker, Actor target)
    {
      if (speaker.Leader != target && target.Leader != speaker) return false;
      return speaker.HasActiveCellPhone && target.HasActiveCellPhone;
    }

#nullable enable
    static private List<Actor> ListDistrictActors(District d, MapListFlags flags, Predicate<Actor> pred)
    {
      return d.FilterActors(pred, map => (flags & MapListFlags.EXCLUDE_SECRET_MAPS) == MapListFlags.NONE || !map.IsSecret);
    }
#nullable restore

    static private string FunFactActorResume(Actor a, string info)
    {
      if (a == null) return "(N/A)";
      return string.Format("{0} - {1}, a {2} - {3}", info, a.TheName, a.Model.Name, a.Location.Map.Name);
    }

    static private string[] CompileDistrictFunFacts(District d)
    {
      var stringList = new List<string>();
      List<Actor> actorList1 = ListDistrictActors(d, MapListFlags.EXCLUDE_SECRET_MAPS, a => {
        if (!a.IsDead) return !a.Model.Abilities.IsUndead;
        return false;
      });
      List<Actor> actorList2 = ListDistrictActors(d, MapListFlags.EXCLUDE_SECRET_MAPS, a => {
        if (!a.IsDead) return a.Model.Abilities.IsUndead;
        return false;
      });
      (Player.Model.Abilities.IsUndead ? actorList2 : actorList1).Add(Player);
      if (actorList1.Count > 0) {
        actorList1.SortIncreasing(x => x.SpawnTime);
        stringList.Add("- Oldest Livings Surviving");
        stringList.Add(string.Format("    1st {0}.", FunFactActorResume(actorList1[0], new WorldTime(actorList1[0].SpawnTime).ToString())));
        if (actorList1.Count > 1)
          stringList.Add(string.Format("    2nd {0}.", FunFactActorResume(actorList1[1], new WorldTime(actorList1[1].SpawnTime).ToString())));
      } else stringList.Add("    No living actors alive!");
      if (actorList2.Count > 0) {
        actorList2.SortIncreasing(x => x.SpawnTime);
        stringList.Add("- Oldest Undeads Rotting Around");
        stringList.Add(string.Format("    1st {0}.", FunFactActorResume(actorList2[0], new WorldTime(actorList2[0].SpawnTime).ToString())));
        if (actorList2.Count > 1)
          stringList.Add(string.Format("    2nd {0}.", FunFactActorResume(actorList2[1], new WorldTime(actorList2[1].SpawnTime).ToString())));
      }
      else stringList.Add("    No undeads shambling around!");
      if (actorList1.Count > 0) {
        actorList1.SortDecreasing(x => x.KillsCount);
        stringList.Add("- Deadliest Livings Kicking ass");
        if (actorList1[0].KillsCount > 0) {
          stringList.Add(string.Format("    1st {0}.", FunFactActorResume(actorList1[0], actorList1[0].KillsCount.ToString())));
          if (actorList1.Count > 1 && actorList1[1].KillsCount > 0)
            stringList.Add(string.Format("    2nd {0}.", FunFactActorResume(actorList1[1], actorList1[1].KillsCount.ToString())));
        }
        else stringList.Add("    Livings can't fight for their lives apparently.");
      }
      if (actorList2.Count > 0) {
        actorList2.SortDecreasing(x => x.KillsCount);
        stringList.Add("- Deadliest Undeads Chewing Brains");
        if (actorList2[0].KillsCount > 0) {
          stringList.Add(string.Format("    1st {0}.", FunFactActorResume(actorList2[0], actorList2[0].KillsCount.ToString())));
          if (actorList2.Count > 1 && actorList2[1].KillsCount > 0)
            stringList.Add(string.Format("    2nd {0}.", FunFactActorResume(actorList2[1], actorList2[1].KillsCount.ToString())));
        }
        else stringList.Add("    Undeads don't care for brains apparently.");
      }
      if (actorList1.Count > 0) {
        actorList1.SortDecreasing(x => Session.Get.Police.CountMurders(x));
        stringList.Add("- Most Murderous Murderer Murdering");
        if (0 < Session.Get.Police.CountMurders(actorList1[0])) {
          stringList.Add(string.Format("    1st {0}.", FunFactActorResume(actorList1[0], Session.Get.Police.CountMurders(actorList1[0]).ToString())));
          if (1 < actorList1.Count && 0 < Session.Get.Police.CountMurders(actorList1[1]))
            stringList.Add(string.Format("    2nd {0}.", FunFactActorResume(actorList1[1], Session.Get.Police.CountMurders(actorList1[1]).ToString())));
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
      Skills.LoadSkillsFromCSV(m_UI, Path.Combine("Resources", "Data", "Skills.csv"));
    }

    // alpha10
#region Background music
    private void UpdateBgMusic()
    {
       if (   !s_Options.PlayMusic || null == m_Player
           || (m_MusicManager.IsPlaying && m_MusicManager.Priority > MusicPriority.PRIORITY_BGM))  // don't interrupt music that has higher priority than bg
         return;

       // get current map music and play it if not already playing it
       string mapMusic = CurrentMap.BgMusic;
       if (!string.IsNullOrEmpty(mapMusic))m_MusicManager.Play(mapMusic, MusicPriority.PRIORITY_BGM);
     }
#endregion

    public abstract class Overlay   // could be an interface instead
    {
      public abstract void Draw(IRogueUI ui);
    }

    private class OverlayImage : Overlay
    {
      public readonly GDI_Point ScreenPosition;
      public readonly string ImageID;

      public OverlayImage(GDI_Point screenPosition, string imageID)
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
      public readonly GDI_Point ScreenPosition;
      public readonly string ImageID;

      public OverlayTransparentImage(float alpha, GDI_Point screenPosition, string imageID)
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
      public readonly GDI_Point ScreenPosition;
      public readonly string Text;
      public readonly Color Color;
      public readonly Color? ShadowColor;

      public OverlayText(GDI_Point screenPosition, Color color, string text, Color? shadowColor = null)
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

    // cf competing implementation : GameImages::MonochromeBorderTile class and image caching
    public class OverlayRect : Overlay
    {
      public readonly GDI_Rectangle Rectangle;
      public readonly Color Color;

      public OverlayRect(Color color, GDI_Rectangle rect)
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
      public GDI_Point ScreenPosition;
      public readonly Color TextColor;
      public readonly Color BoxBorderColor;
      public readonly Color BoxFillColor;
      public readonly string[] Lines;  // Cf. Advisor: need array entries to be writeable

      public OverlayPopup(string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, GDI_Point screenPos)
      {
        ScreenPosition = screenPos;
        TextColor = textColor;
        BoxBorderColor = boxBorderColor;
        BoxFillColor = boxFillColor;
        Lines = lines;
      }

      public OverlayPopup(string line, Color textColor, Color boxBorderColor, Color boxFillColor, GDI_Point screenPos)
      {
        ScreenPosition = screenPos;
        TextColor = textColor;
        BoxBorderColor = boxBorderColor;
        BoxFillColor = boxFillColor;
        Lines = new string[] { line };
      }

      public override void Draw(IRogueUI ui)
      {
        ui.UI_DrawPopup(Lines, TextColor, BoxBorderColor, BoxFillColor, ScreenPosition.X, ScreenPosition.Y);
      }
    }

    // alpha10
    private class OverlayPopupTitle : Overlay
    {
      public readonly Point ScreenPosition;
      public readonly string Title;
      public readonly Color TitleColor;
      public readonly string[] Lines;
      public readonly Color TextColor;
      public readonly Color BoxBorderColor;
      public readonly Color BoxFillColor;

      public OverlayPopupTitle(string title, Color titleColor, string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, Point screenPos)
      {
        ScreenPosition = screenPos;
        Title = title;
        TitleColor = titleColor;
        TextColor = textColor;
        BoxBorderColor = boxBorderColor;
        BoxFillColor = boxFillColor;
        Lines = lines;
      }

      public override void Draw(IRogueUI ui)
      {
        ui.UI_DrawPopupTitle(Title, TitleColor, Lines, TextColor, BoxBorderColor, BoxFillColor, ScreenPosition.X, ScreenPosition.Y);
      }
    }

#if PROTOTYPE
    class OverlayPopupTitleColors : Overlay
    {
            public Point ScreenPosition { get; set; }
            public string Title { get; set; }
            public Color TitleColor { get; set; }
            public string[] Lines { get; set; }
            public Color[] Colors { get; set; }
            public Color BoxBorderColor { get; set; }
            public Color BoxFillColor { get; set; }

            public OverlayPopupTitleColors(string title, Color titleColor, string[] lines, Color[] colors, Color boxBorderColor, Color boxFillColor, Point screenPos)
            {
                this.ScreenPosition = screenPos;
                this.Title = title;
                this.TitleColor = titleColor;
                this.Colors = colors;
                this.BoxBorderColor = boxBorderColor;
                this.BoxFillColor = boxFillColor;
                this.Lines = lines;
            }

            public override void Draw(IRogueUI ui)
            {
                ui.UI_DrawPopupTitleColors(Title, TitleColor, Lines, Colors, BoxBorderColor, BoxFillColor, ScreenPosition.X, ScreenPosition.Y);
            }
    }
#endif

    private struct CharGen
    {
      public GameMode GameMode;
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
    private enum MapListFlags
    {
      NONE = 0,
      EXCLUDE_SECRET_MAPS = 1,
    }
  }

#nullable enable
  static internal class RogueGame_ext {
    // These two are assumed to be working w/actor.VisibleIdentity
    // Using an extension function for the Verb class is intentional (same API as for raw strings)
    static public string Conjugate(this string verb, Actor actor)
    {
      return verb.Conjugate((RogueGame.IsPlayer(actor) && 1 == World.Get.PlayerCount) ? 2 : 3, actor.IsPluralName ? 3 : 1);
    }

    static public string Conjugate(this Verb verb, Actor actor)
    {
      return verb.Conjugate((RogueGame.IsPlayer(actor) && 1 == World.Get.PlayerCount) ? 2 : 3, actor.IsPluralName ? 3 : 1);
    }

    static public KeyValuePair<List<PlayerController>, List<Actor>>? SetDifference(this KeyValuePair<List<PlayerController>, List<Actor>> lhs, KeyValuePair<List<PlayerController>, List<Actor>>? rhs) {
        if (null == rhs) return null;
        KeyValuePair<List<PlayerController>, List<Actor>> ret = new(new(lhs.Key),new(lhs.Value));
        if (0 < ret.Key.Count) {
            foreach(var actor in rhs.Value.Key) ret.Key.Remove(actor);
        }
        if (0 < ret.Value.Count) {
            foreach(var actor in rhs.Value.Value) ret.Value.Remove(actor);
        }
        return (0 < ret.Key.Count || 0 < ret.Value.Count) ? ret : null;
    }

    static public List<PlayerController>? SetDifference(this List<PlayerController> lhs, List<PlayerController>? rhs) {
        if (null == rhs) return null;
        List<PlayerController> ret = new(lhs);
        if (0 < ret.Count) {
            foreach(var actor in rhs) ret.Remove(actor);
        }
        return 0 < ret.Count ? ret : null;
    }

    static public KeyValuePair<List<PlayerController>, List<Actor>>? Intersect(this KeyValuePair<List<PlayerController>, List<Actor>> lhs, KeyValuePair<List<PlayerController>, List<Actor>>? rhs) {
        if (null == rhs) return null;
        KeyValuePair<List<PlayerController>, List<Actor>> ret = new(new(), new());
        if (0 < lhs.Key.Count) {
            foreach(var actor in rhs.Value.Key) if (lhs.Key.Contains(actor)) ret.Key.Add(actor);
        }
        if (0 < lhs.Value.Count) {
            foreach(var actor in rhs.Value.Value) if (lhs.Value.Contains(actor)) ret.Value.Add(actor);
        }
        return (0 < ret.Key.Count || 0 < ret.Value.Count) ? ret : null;
    }

    static public List<PlayerController>? Intersect(this List<PlayerController> lhs, List<PlayerController>? rhs) {
        if (null == rhs) return null;
        List<PlayerController> ret = new();
        if (0 < lhs.Count) {
            foreach(var actor in rhs) if (lhs.Contains(actor)) ret.Add(actor);
        }
        return 0 < ret.Count ? ret : null;
    }

    static public void DrawIcon(this string icon, in GDI_Point origin, Color tint) {
        IRogueUI.UI.UI_DrawImage(icon, origin.X, origin.Y, tint);
    }

    static public void DrawIcon(this string icon, in GDI_Point origin) {
        IRogueUI.UI.UI_DrawImage(icon, origin.X, origin.Y);
    }

    static public void DrawIcon(this string icon, int x, int y, Color tint) {
        IRogueUI.UI.UI_DrawImage(icon, x, y, tint);
    }

    static public void DrawIcon(this string icon, int x, int y) {
        IRogueUI.UI.UI_DrawImage(icon, x, y);
    }

    static public string? StatusIcon(this ItemTrap trap)
    {
      // internal offset: (1,1)
      // internal dimensions: (9,8)
      // anchor for CGI generation: GameImages.ICON_TRAP_ACTIVATED_SAFE_GROUP
      var player = RogueGame.Player;
      if (trap.IsTriggered) {
        if (trap.Owner == player) return GameImages.ICON_TRAP_TRIGGERED_SAFE_PLAYER;
        else if (trap.IsSafeFor(player)) return GameImages.ICON_TRAP_TRIGGERED_SAFE_GROUP;
        else if (trap.WouldLearnHowToBypass(player)) return GameImages.ICON_TRAP_TRIGGERED_SAFE_GROUP;
        return GameImages.ICON_TRAP_TRIGGERED;
      }
      if (trap.IsActivated) {
        if (trap.Owner == player) return GameImages.ICON_TRAP_ACTIVATED_SAFE_PLAYER;
        else if (trap.IsSafeFor(player)) return GameImages.ICON_TRAP_ACTIVATED_SAFE_GROUP;
        else if (trap.WouldLearnHowToBypass(player)) return GameImages.ICON_TRAP_ACTIVATED_SAFE_GROUP;
        return GameImages.ICON_TRAP_ACTIVATED;
      }
      return null;
    }

    static public string? StatusIcon(this ItemFood food)
    {
      // internal offset: (22,1)
      // internal dimensions: (8,8)
      var turn = Session.Get.WorldTime.TurnCounter;
      if (food.IsExpiredAt(turn)) return GameImages.ICON_EXPIRED_FOOD;
      else if (food.IsSpoiledAt(turn)) return GameImages.ICON_SPOILED_FOOD;
      return null;
    }

    static public string DescribeBatteries(this BatteryPowered it)
    {
      int charge = it.Batteries;
      int hours = charge / WorldTime.TURNS_PER_HOUR;
      int max;
      return (charge < (max = it.MaxBatteries)) ? string.Format("Batteries : {0}/{1} ({2}h)", charge, max, hours)
                                                : string.Format("Batteries : {0} MAX ({1}h)", charge, hours);
    }

    static public string[] Describe(this List<Corpse> corpses)
    {
      var lines = new List<string>(corpses.Count + 2) {
        1 < corpses.Count ? "There are corpses there..." : "There is a corpse here.",
        " "
      };
      foreach (Corpse corpse in corpses) lines.Add("- "+corpse.ToString().Capitalize()+".");
      return lines.ToArray();
    }

    static public void DoTheyEnterMap(this IEnumerable<Actor> followers, Location to, in Location from)
    {
      var map = to.Map;
      List<Point>? pointList;    // \todo? candidate for GC micro-optimization, but cold path
      foreach(Actor fo in followers) {
        var fo_map = fo.Location.Map;
        if (fo_map == map) continue;  // already in destination, ok
        if (fo_map.DistrictPos != map.DistrictPos) continue;  // cross-district change
        pointList = Rules.IsAdjacent(from, fo.Location) ? map.FilterAdjacentInMap(to.Position, pt => map.IsWalkableFor(pt, fo) && !map.HasActorAt(in pt))
                                                        : null;
        var game = RogueGame.Game;  // if this becomes hot path we can test whether hoisting this is worth the IL size increase
        if (null != pointList && 0 < pointList.Count && game.TryActorLeaveTile(fo)) {
          map.PlaceAt(fo, Rules.Get.DiceRoller.Choose(pointList));
          map.MoveActorToFirstPosition(fo);
          game.OnActorEnterTile(fo);
        }
      }
    }

    // use Func rather than Predicate as that doesn't have problems w/local functions
    static public List<Actor>? ThoseNearby(this Location loc, int radius, Func<Actor,bool> ok) {
        List<Actor> ret = new();
        var survey = new Rectangle(loc.Position+(short)radius*Direction.NW, (Point)(2*radius+1));
        // lambda function access of loc.Position prevents converting to member function
        survey.DoForEach(pt => {
          Location test = new Location(loc.Map, pt);
          if (Map.Canonical(ref test)) {
              var actor = test.Actor;
              if (null != actor && !actor.IsDead && ok(actor)) ret.Add(actor);
          }
        });

        var exit = loc.Exit;
        if (null != exit) {
          var actor = exit.Location.Actor;
          if (null != actor && !actor.IsDead && ok(actor)) ret.Add(actor);
        }

        return 0<ret.Count ? ret : null;
    }

    static public void InferEnemy(this Actor defender, Actor attacker) {
      // Cf. game factions and default AI definitions
      if (   defender.IsFaction(GameFactions.IDs.TheUndeads)
          || defender.IsFaction(GameFactions.IDs.ThePsychopaths))
        return;

      bool sees(Actor a) {
        if (!(a.Controller is OrderableAI ordai)) return false; // PlayerController would respond only on autopilot; that would require ObjectiveAI here
        if (!a.Controller.CanSee(defender.Location)) return false;
        if (a.Controller.CanSee(attacker.Location)) return false;
        if (Rules.StdDistance(a.Location, attacker.Location) > a.AudioRange) return false;
        if (a.IsEnemyOf(defender)) return false;
        return a.IsEnemyOf(attacker);
      }

      // defender may be just-killed
      var responders = defender.Location.ThoseNearby(Actor.MAX_VISION, sees);
      if (null == responders) return;

      AI.Percept_<Actor> target = new(attacker, attacker.Location.Map.LocalTime.TurnCounter, attacker.Location);
      foreach(var actor in responders) {
        (actor.Controller as ObjectiveAI)!.Track(target);
      }
    }

    static public List<PlayerController>? PlayersInLOS(this ILocation view) => RogueGame.PlayersInLOS(view.Location);

    static public string? VisibleIdentity(this KeyValuePair<List<PlayerController>, List<Actor>> witnesses, Actor observer, Actor whom) {
        foreach(var pc in witnesses.Key) if (pc.ControlledActor == observer) return whom.TheName;
        foreach(var a in witnesses.Value) if (a == observer) return whom.TheName;
        return null;
    }

    static public string? VisibleIdentity(this KeyValuePair<List<PlayerController>, List<Actor>> witnesses, Actor observer, MapObject what) {
        foreach(var pc in witnesses.Key) if (pc.ControlledActor == observer) return what.TheName;
        foreach(var a in witnesses.Value) if (a == observer) return what.TheName;
        return null;
    }

    static public Actor? WitnessedBy(this KeyValuePair<List<PlayerController>, List<Actor>> witnesses, Func<Actor, bool> ok) {
        foreach(var pc in witnesses.Key) if (ok(pc.ControlledActor)) return pc.ControlledActor;
        foreach(var a in witnesses.Value) if (ok(a)) return a;
        return null;
    }

    static public KeyValuePair<List<PlayerController>, List<Actor>>? grep(this KeyValuePair<List<PlayerController>, List<Actor>> witnesses, Func<Actor, bool> ok) {
        List<PlayerController> ret = new();
        List<Actor> ret_viewpoints = new();

        foreach(var pc in witnesses.Key) if (ok(pc.ControlledActor)) ret.Add(pc);
        foreach(var a in witnesses.Value) if (ok(a)) ret_viewpoints.Add(a);

        return (0<ret.Count || 0<ret_viewpoints.Count) ? new(ret, ret_viewpoints) : null;
    }

  }
#nullable restore
}
