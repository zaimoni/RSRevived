// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.GameMusics
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

namespace djack.RogueSurvivor.Gameplay
{
  internal static class GameMusics
  {
#if LINUX
    private const string PATH = "Resources/Music/";
#else
    private const string PATH = "Resources\\Music\\";
#endif
    public const string ARMY = "army";
    public static readonly string ARMY_FILE = PATH + "RS - Army";
    public const string BIGBEAR_THEME_SONG = "big bear theme song";
    public static readonly string BIGBEAR_THEME_SONG_FILE = PATH + "RS - Big Bear Theme Song";
    public const string BIKER = "biker";
    public static readonly string BIKER_FILE = PATH + "RS - Biker";
    public const string CHAR_UNDERGROUND_FACILITY = "char underground facility";
    public static readonly string CHAR_UNDERGROUND_FACILITY_FILE = PATH + "RS - CUF";
    public const string DUCKMAN_THEME_SONG = "duckman theme song";
    public static readonly string DUCKMAN_THEME_SONG_FILE = PATH + "RS - Duckman Theme Song";
    public const string FAMU_FATARU_THEME_SONG = "famu fataru theme song";
    public static readonly string FAMU_FATARU_THEME_SONG_FILE = PATH + "RS - Famu Fataru Theme Song";
    public const string FIGHT = "fight";
    public static readonly string FIGHT_FILE = PATH + "RS - Fight";
    public const string GANGSTA = "gangsta";
    public static readonly string GANGSTA_FILE = PATH + "RS - Gangsta";
    public const string HANS_VON_HANZ_THEME_SONG = "hans von hanz theme song";
    public static readonly string HANS_VON_HANZ_THEME_SONG_FILE = PATH + "RS - Hans von Hanz Theme Song";
    public const string HEYTHERE = "heythere";
    public static readonly string HEYTHERE_FILE = PATH + "RS - Hey There";
    public const string HOSPITAL = "hospital";
    public static readonly string HOSPITAL_FILE = PATH + "RS - Hospital";
    public const string INSANE = "insane";
    public static readonly string INSANE_FILE = PATH + "RS - Insane";
    public const string INTERLUDE = "interlude";
    public static readonly string INTERLUDE_FILE = PATH + "RS - Interlude - Loop";
    public const string INTRO = "intro";
    public static readonly string INTRO_FILE = PATH + "RS - Intro";
    public const string LIMBO = "limbo";
    public static readonly string LIMBO_FILE = PATH + "RS - Limbo";
    public const string PLAYER_DEATH = "playerdeath";
    public static readonly string PLAYER_DEATH_FILE = PATH + "RS - Post Mortem";
    public const string REINCARNATE = "reincarnate";
    public static readonly string REINCARNATE_FILE = PATH + "RS - Reincarnate";
    public const string ROGUEDJACK_THEME_SONG = "roguedjack theme song";
    public static readonly string ROGUEDJACK_THEME_SONG_FILE = PATH + "RS - Roguedjack Theme Song";
    public const string SANTAMAN_THEME_SONG = "santaman theme song";
    public static readonly string SANTAMAN_THEME_SONG_FILE = PATH + "RS - Santaman Theme Song";
    public const string SEWERS = "sewers";
    public static readonly string SEWERS_FILE = PATH + "RS - Sewers";
    public const string SLEEP = "sleep";
    public static readonly string SLEEP_FILE = PATH + "RS - Sleep - Loop";
    public const string SUBWAY = "subway";
    public static readonly string SUBWAY_FILE = PATH + "RS - Subway";
    public const string SURVIVORS = "survivors";
    public static readonly string SURVIVORS_FILE = PATH + "RS - Survivors";

    // alpha10
    public const string SURFACE = "surface";
    public static readonly string SURFACE_FILE = PATH + "RS - Surface";
  }
}
