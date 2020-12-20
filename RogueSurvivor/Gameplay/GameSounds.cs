// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.GameSounds
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

namespace djack.RogueSurvivor.Gameplay
{
  internal static class GameSounds
  {
#if LINUX
    private const string PATH = "Resources/Sfx/";
#else
    private const string PATH = "Resources\\Sfx\\";
#endif
    public const string UNDEAD_EAT = "undead eat";
    public const string UNDEAD_EAT_FILE = PATH + "sfx - undead eat";
    public const string UNDEAD_RISE = "undead rise";
    public const string UNDEAD_RISE_FILE = PATH + "sfx - undead rise";
    public const string NIGHTMARE = "nightmare";
    public const string NIGHTMARE_FILE = PATH + "sfx - nightmare";
  }
}
