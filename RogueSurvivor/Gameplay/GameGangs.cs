// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Gameplay.GameGangs
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Gameplay
{
  public static class GameGangs
  {
	private const int MAX_GANGS = (int)IDs.GANGSTA_FLOODS+1;

    public static readonly IDs[] GANGSTAS = new IDs[2]
    {
      IDs.GANGSTA_CRAPS,
      IDs.GANGSTA_FLOODS
    };
    public static readonly GameGangs.IDs[] BIKERS = new IDs[2]
    {
      IDs.BIKER_HELLS_SOULS,
      IDs.BIKER_FREE_ANGELS
    };
    private static readonly string[] NAMES = new string[MAX_GANGS] {
      "(no gang)",
      "Hell's Souls",
      "Free Angels",
      "Craps",
      "Floods"
    };

    public static string Name(this IDs x)
    {
      return NAMES[(int)x];
    }

    [Serializable]
    public enum IDs
    {
      NONE,
      BIKER_HELLS_SOULS,
      BIKER_FREE_ANGELS,
      GANGSTA_CRAPS,
      GANGSTA_FLOODS,
    }
  }
}
