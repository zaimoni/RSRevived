// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Weather
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  // VAPORWARE while currently this is a single value at the World level, it logically should be per-district to allow
  // for fronts, etc. (also want to adjust sunrise/sunset times by longitude/latitude)

  [Serializable]
  internal enum Weather
  {
    CLEAR = 0,
    CLOUDY = 1,
    RAIN = 2,
    HEAVY_RAIN = 3,
    _COUNT = 4,
  }

  public static class WeatherExtension
  {
    internal static bool IsRain(this Weather w)
    {
      return Weather.RAIN==w || Weather.HEAVY_RAIN==w;
    }
  }
}
